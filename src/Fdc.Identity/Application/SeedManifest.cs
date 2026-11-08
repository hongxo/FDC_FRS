using System.Collections.Immutable;
using System.Security.Cryptography;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Fdc.Identity.Application;

public sealed record SeedRole(Guid? RoleId, string? RoleCode, string RoleName, ImmutableArray<string> PermissionCodes);
public sealed record SeedManifest(string ManifestId, long CatalogVersion, string? ApprovalReference, ImmutableArray<SeedRole> Roles);
public interface ISeedApprovalVerifier
{
    Task<bool> VerifyAsync(string manifestSha256, string approvalReference, CancellationToken ct);
}
public sealed class PendingSeedApprovalVerifier : ISeedApprovalVerifier
{
    public Task<bool> VerifyAsync(string manifestSha256, string approvalReference, CancellationToken ct) => Task.FromResult(false);
}
public static class SeedManifestValidator
{
    public static async Task<SeedManifest> ValidateAsync(ReadOnlyMemory<byte> exactManifestBytes,
        ISeedApprovalVerifier verifier, IReadOnlySet<string> approvedImplementedCatalog, CancellationToken ct)
    {
        var manifest = JsonSerializer.Deserialize<SeedManifest>(exactManifestBytes.Span,
            new JsonSerializerOptions(JsonSerializerDefaults.Web) { UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow })
            ?? throw new ArgumentException("Missing seed manifest.");
        if (manifest.CatalogVersion < 1 || string.IsNullOrWhiteSpace(manifest.ManifestId) || string.IsNullOrWhiteSpace(manifest.ApprovalReference))
            throw new OwnerDecisionRequiredException("D04: approved versioned seed manifest");
        if (manifest.Roles.IsDefaultOrEmpty || manifest.Roles.Any(r => r.RoleId is null || r.RoleId == Guid.Empty || string.IsNullOrWhiteSpace(r.RoleCode)
            || string.IsNullOrWhiteSpace(r.RoleName) || r.PermissionCodes.IsDefault)) throw new ArgumentException("Incomplete seed identities/catalog.");
        if (manifest.Roles.Select(r => r.RoleId).Distinct().Count() != manifest.Roles.Length
            || manifest.Roles.Select(r => r.RoleCode).Distinct(StringComparer.Ordinal).Count() != manifest.Roles.Length)
            throw new ArgumentException("Duplicate immutable seed identity.");
        if (manifest.Roles.SelectMany(r => r.PermissionCodes).Any(p => p == "ied.configuration.write" || !approvedImplementedCatalog.Contains(p)))
            throw new UnauthorizedAccessException("Seed requests unsupported or forbidden permissions.");
        var hash = Convert.ToHexStringLower(SHA256.HashData(exactManifestBytes.Span));
        if (!await verifier.VerifyAsync(hash, manifest.ApprovalReference, ct)) throw new OwnerDecisionRequiredException("D04: external seed approval verification");
        return manifest;
    }
}
