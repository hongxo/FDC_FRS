using Fdc.Identity.Infrastructure;

namespace Fdc.Identity.Application;

public enum SessionRevocationOperation { Current, Everywhere }
public interface ISessionRevocationPolicy
{
    Task DemandConfiguredAsync(SessionRevocationOperation operation, CancellationToken ct);
    // Real current SQL session; operation-specific MFA/assurance policy, server correlation ID only.
    Task<string> DemandAsync(SessionRevocationOperation operation, CurrentHumanSession session, CancellationToken ct);
}
public sealed class PendingSessionRevocationPolicy : ISessionRevocationPolicy
{
    public Task DemandConfiguredAsync(SessionRevocationOperation operation, CancellationToken ct) =>
        throw new OwnerDecisionRequiredException("D05/D09/D10: self logout/revoke policy, current authority and audit disposition");
    public Task<string> DemandAsync(SessionRevocationOperation operation, CurrentHumanSession session, CancellationToken ct) =>
        throw new OwnerDecisionRequiredException("D03/D05: required assurance for session revocation operation");
}
public sealed record SessionRevocationResult(bool Revoked, int SessionsChanged, long SecurityVersion);
