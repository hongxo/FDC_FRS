using System.Collections.Immutable;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Fdc.Identity.Domain;

namespace Fdc.Identity.Application;

public enum RoleChangeKind { Create, Rename, Clone, Activate, Deactivate, PermissionDiff }
public sealed record RoleChangePayload(RoleChangeKind Kind, Guid? RoleId, string? NewCode, string? Name,
    long BaseVersion, long CatalogVersion, ImmutableArray<string> Add, ImmutableArray<string> Remove, string Reason);
public sealed class RoleChangeRequest
{
    public Guid Id { get; } = Guid.NewGuid();
    public string RequesterPerson { get; }
    public string Digest { get; }
    public string PayloadJson { get; }
    public long Revision { get; }
    public DateTimeOffset ExpiresAt { get; }
    private readonly HashSet<string> approvers = new(StringComparer.Ordinal);
    public IReadOnlyCollection<string> ApproverPersons => approvers.ToArray();
    public RoleChangeRequest(string requesterPerson, RoleChangePayload payload, long revision, DateTimeOffset expiresAt)
    {
        if (string.IsNullOrWhiteSpace(requesterPerson) || revision < 1 || string.IsNullOrWhiteSpace(payload.Reason)) throw new ArgumentException("Invalid change request.");
        var add = payload.Add.Distinct(StringComparer.Ordinal).Order(StringComparer.Ordinal).ToImmutableArray();
        var remove = payload.Remove.Distinct(StringComparer.Ordinal).Order(StringComparer.Ordinal).ToImmutableArray();
        if (add.Intersect(remove, StringComparer.Ordinal).Any() || add.Concat(remove).Any(string.IsNullOrWhiteSpace)) throw new ArgumentException("Conflicting permissions.");
        RequesterPerson = requesterPerson; Revision = revision; ExpiresAt = expiresAt;
        PayloadJson = JsonSerializer.Serialize(payload with { Add = add, Remove = remove });
        var boundContent = JsonSerializer.Serialize(new { RequesterPerson, Revision, ExpiresAt, PayloadJson });
        Digest = Convert.ToHexStringLower(SHA256.HashData(Encoding.UTF8.GetBytes(boundContent)));
    }
    public void RecordApproval(string approverPerson, long expectedRevision, string expectedDigest,
        IReadOnlySet<string> currentBeneficiaryPersons, bool currentAuthorityVerified, DateTimeOffset now)
    {
        if (!currentAuthorityVerified || now >= ExpiresAt) throw new UnauthorizedAccessException("Current authority or request validity failed.");
        if (expectedRevision != Revision || expectedDigest != Digest) throw new InvalidOperationException("Stale revision or digest.");
        if (string.IsNullOrWhiteSpace(approverPerson) || approverPerson == RequesterPerson || currentBeneficiaryPersons.Contains(approverPerson))
            throw new UnauthorizedAccessException("Independent approver required.");
        if (!approvers.Add(approverPerson)) throw new InvalidOperationException("Duplicate person approval.");
    }
    public bool CanApply(long expectedRevision, string expectedDigest, int requiredIndependentApprovals,
        IReadOnlySet<string> currentBeneficiaryPersons, IReadOnlySet<string> currentlyAuthorizedApprovers, DateTimeOffset now)
        => requiredIndependentApprovals > 0 && now < ExpiresAt && expectedRevision == Revision && expectedDigest == Digest
           && approvers.Count >= requiredIndependentApprovals && approvers.All(p => p != RequesterPerson
               && !currentBeneficiaryPersons.Contains(p) && currentlyAuthorizedApprovers.Contains(p));
    // CanApply is a domain precondition, NEVER permission to bypass IAuthorizedCommitBoundary.
}
