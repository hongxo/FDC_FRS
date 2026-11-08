using System.Collections.Immutable;
using System.Security.Cryptography;
using System.Text.Json;
using Fdc.Identity.Domain;

namespace Fdc.Identity.Application;

// All Current* inputs below come from trusted server repositories under the eventual commit gate, never request JSON.
public sealed record ReviewedChangeCatalog(long Version, ImmutableHashSet<string> SupportedCodes);
public sealed record RoleProposalPreview(RoleDefinition Draft, ImmutableArray<string> ProposedPermissions);
public static class RoleProposalValidator
{
    public static RoleProposalPreview Validate(RoleChangePayload payload, RoleDefinition? current,
        IReadOnlySet<string> currentRolePermissions, IReadOnlySet<string> occupiedCodes, ReviewedChangeCatalog catalog)
    {
        if (payload.Add.IsDefault || payload.Remove.IsDefault || !Enum.IsDefined(payload.Kind) || catalog.Version < 1 || payload.CatalogVersion != catalog.Version ||
            string.IsNullOrWhiteSpace(payload.Reason) || payload.Reason.Length > 1000 || payload.Reason.Any(char.IsControl))
            throw new ArgumentException("Invalid proposal or catalog version.");
        var additions = payload.Add.ToImmutableHashSet(StringComparer.Ordinal);
        var removals = payload.Remove.ToImmutableHashSet(StringComparer.Ordinal);
        foreach (var code in additions.Concat(removals))
            if (code.Contains('*') || code == "ied.configuration.write" || !catalog.SupportedCodes.Contains(code))
                throw new UnauthorizedAccessException("Unsupported or mandatory-deny permission.");
        if (additions.Overlaps(removals)) throw new ArgumentException("Conflicting permission diff.");
        if (payload.Kind != RoleChangeKind.PermissionDiff && (!additions.IsEmpty || !removals.IsEmpty))
            throw new ArgumentException("Permission changes require an explicit permission-diff proposal.");
        RoleDefinition draft;
        if (payload.Kind == RoleChangeKind.Create)
        {
            if (current is not null || payload.RoleId is not null || payload.BaseVersion != 0) throw new ArgumentException("Create cannot target an existing role.");
            draft = RoleDefinition.Create(payload.NewCode ?? "", payload.Name ?? "");
        }
        else
        {
            if (current is null || current.Id != payload.RoleId || current.Version != payload.BaseVersion)
                throw new InvalidOperationException("Role version conflict.");
            draft = payload.Kind switch
            {
                RoleChangeKind.Clone => current.CopyAsDraft(payload.NewCode ?? "", payload.Name ?? ""),
                RoleChangeKind.Rename => current.Rename(payload.Name ?? ""),
                _ => current
            };
            if (payload.Kind != RoleChangeKind.Clone && payload.NewCode is not null) throw new ArgumentException("Immutable role code.");
        }
        if (payload.Kind is RoleChangeKind.Create or RoleChangeKind.Clone && occupiedCodes.Contains(draft.Code))
            throw new InvalidOperationException("Role code already exists, including retired codes.");
        var proposed = payload.Kind == RoleChangeKind.Create ? ImmutableHashSet<string>.Empty.WithComparer(StringComparer.Ordinal)
            : currentRolePermissions.ToImmutableHashSet(StringComparer.Ordinal);
        proposed = proposed.Except(removals).Union(additions);
        if (proposed.Any(x => x.Contains('*') || x == "ied.configuration.write" || !catalog.SupportedCodes.Contains(x)))
            throw new UnauthorizedAccessException("Current configuration cannot be cloned or proposed outside supported catalog.");
        // Preview only: Activate does not change Active; clone never carries assignments/grants/delegations.
        return new(draft, proposed.Order(StringComparer.Ordinal).ToImmutableArray());
    }
}

public sealed record StationGrantProposal(Guid UserRoleId, Guid ScopeId, ImmutableArray<string> Functions,
    DateTimeOffset From, DateTimeOffset Until, long BaseVersion, long CatalogVersion, long HierarchyVersion, string Reason);
public sealed record CurrentGrantEnvelope(UserIdentity Target, RoleAssignment Assignment, RoleDefinition Role,
    long CurrentGrantVersion, long CatalogVersion, long HierarchyVersion, ImmutableHashSet<Guid> AuthorizedScopeIds,
    ImmutableHashSet<string> CurrentRolePermissions, ImmutableHashSet<string> SupportedPermissions,
    DateTimeOffset MaximumUntil);
public sealed record GrantProposalPreview(StationGrantProposal Proposal, ImmutableArray<string> Added, ImmutableArray<string> Removed, string Digest);
public static class StationGrantProposalValidator
{
    public static GrantProposalPreview Validate(StationGrantProposal proposed, CurrentGrantEnvelope current,
        string actorPerson, IReadOnlySet<string> previousFunctions, DateTimeOffset now)
    {
        if (string.IsNullOrWhiteSpace(actorPerson) || actorPerson == current.Target.PersonReference)
            throw new UnauthorizedAccessException("No self-grant, including aliases of the same person.");
        if (!current.Target.Active || !current.Assignment.Active || !current.Role.Active ||
            current.Assignment.UserId != current.Target.Id || current.Assignment.RoleId != current.Role.Id ||
            current.Assignment.Id != proposed.UserRoleId || current.Assignment.From > now || current.Assignment.Until <= now || current.Role.ValidUntil <= now)
            throw new UnauthorizedAccessException("Current subject/assignment/role is not eligible.");
        if (current.CatalogVersion < 1 || current.HierarchyVersion < 1 || proposed.CatalogVersion != current.CatalogVersion ||
            proposed.HierarchyVersion != current.HierarchyVersion || proposed.BaseVersion != current.CurrentGrantVersion)
            throw new InvalidOperationException("Stale grant/catalog/hierarchy version.");
        if (!current.AuthorizedScopeIds.Contains(proposed.ScopeId)) throw new UnauthorizedAccessException("Scope outside current delegation envelope.");
        if (proposed.From.Offset != TimeSpan.Zero || proposed.Until.Offset != TimeSpan.Zero || proposed.Until <= proposed.From ||
            proposed.From < current.Assignment.From || proposed.Until > current.Assignment.Until || proposed.Until > current.MaximumUntil ||
            (current.Role.ValidUntil is { } roleUntil && proposed.Until > roleUntil) || proposed.Until <= now)
            throw new ArgumentException("Grant validity exceeds approved envelope.");
        if (string.IsNullOrWhiteSpace(proposed.Reason) || proposed.Reason.Length > 1000 || proposed.Reason.Any(char.IsControl)) throw new ArgumentException("Reason required.");
        if (proposed.Functions.IsDefault) throw new ArgumentException("Functions required.");
        var functions = proposed.Functions.ToImmutableHashSet(StringComparer.Ordinal);
        if (functions.Count == 0 || functions.Any(x => string.IsNullOrWhiteSpace(x) || x.Contains('*') || x == "ied.configuration.write" ||
            !current.CurrentRolePermissions.Contains(x) || !current.SupportedPermissions.Contains(x)))
            throw new UnauthorizedAccessException("Functions must be a supported subset of this assignment's current role.");
        var normalized = proposed with { Functions = functions.Order(StringComparer.Ordinal).ToImmutableArray() };
        var digest = Convert.ToHexString(SHA256.HashData(JsonSerializer.SerializeToUtf8Bytes(normalized)));
        return new(normalized, functions.Except(previousFunctions).Order(StringComparer.Ordinal).ToImmutableArray(),
            previousFunctions.Except(functions, StringComparer.Ordinal).Order(StringComparer.Ordinal).ToImmutableArray(), digest);
    }
    public static bool Overlaps(DateTimeOffset firstFrom, DateTimeOffset firstUntil, DateTimeOffset secondFrom, DateTimeOffset secondUntil)
        => firstFrom < secondUntil && secondFrom < firstUntil;
}
