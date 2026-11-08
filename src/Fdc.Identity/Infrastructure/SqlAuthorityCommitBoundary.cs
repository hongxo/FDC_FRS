using Fdc.Identity.Application;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace Fdc.Identity.Infrastructure;

// Candidate implementation, NOT registered by any host. Default plan provider remains pending.
// All cooperating mutations/revocations must use this same DB/namespace. Direct DML is not protected by applocks.
public sealed class SqlAuthorityCommitBoundary(IamDbContext db, ISqlCommitGatePlanProvider plans) : IAuthorizedCommitBoundary
{
    public async Task<T> CommitAsync<T>(Guid actorUserId, Func<CancellationToken, Task<T>> recheckMutateAndAudit, CancellationToken ct)
    {
        var plan = (await plans.ResolveAsync(actorUserId, ct)).Normalize(actorUserId);
        if (db.Database.CurrentTransaction is not null || System.Transactions.Transaction.Current is not null)
            throw new InvalidOperationException("Authority boundary must own the transaction; nesting is forbidden.");
        if (db.ChangeTracker.Entries().Any(x => x.State is EntityState.Added or EntityState.Modified or EntityState.Deleted))
            throw new InvalidOperationException("Authority boundary cannot flush mutations prepared outside its gate.");
        await using var transaction = await BeginAsync(plan, ct);
        var committing = false;
        try
        {
            await AcquireAsync(plan.ResourcePrefix + "/Policy", plan.ExclusivePolicy, plan.LockTimeoutMilliseconds, ct);
            foreach (var user in plan.Users)
                await AcquireAsync(plan.ResourcePrefix + "/User/" + user.UserId.ToString("N"), user.Exclusive, plan.LockTimeoutMilliseconds, ct);
            // Callback must freshly check real session/MFA/permission/scope/SoD and persist required audit/outbox.
            var result = await recheckMutateAndAudit(ct);
            committing = true;
            await transaction.CommitAsync(ct);
            return result;
        }
        catch (Exception error)
        {
            var rollbackFailed = false;
            try { await transaction.RollbackAsync(CancellationToken.None); }
            catch
            {
                rollbackFailed = true;
                try { await db.Database.CloseConnectionAsync(); } catch { /* Discard this scope; never return success or retry. */ }
            }
            // Prevent failed mutations from being flushed later by reuse of this scoped DbContext.
            db.ChangeTracker.Clear();
            if (committing) throw new CommitOutcomeUnknownException();
            if (rollbackFailed || SqlAuthorityErrors.IsUnavailable(error)) throw new AuthorityUnavailableException();
            throw;
        }
    }
    private async Task AcquireAsync(string resource, bool exclusive, int timeout, CancellationToken ct)
    {
        await using var command = db.Database.GetDbConnection().CreateCommand();
        command.Transaction = db.Database.CurrentTransaction!.GetDbTransaction();
        command.CommandTimeout = (int)Math.Ceiling(timeout / 1000d) + 2;
        command.CommandText = "DECLARE @result int; EXEC @result=sys.sp_getapplock @Resource=@resource, @LockMode=@mode, @LockOwner=N'Transaction', @LockTimeout=@timeout, @DbPrincipal=N'public'; SELECT @result;";
        void Parameter(string name, object value) { var parameter = command.CreateParameter(); parameter.ParameterName = name; parameter.Value = value; command.Parameters.Add(parameter); }
        Parameter("@resource", resource); Parameter("@mode", exclusive ? "Exclusive" : "Shared"); Parameter("@timeout", timeout);
        var result = await command.ExecuteScalarAsync(ct);
        if (result is not int code || code < 0) throw new AuthorityUnavailableException();
    }
    private async Task<IDbContextTransaction> BeginAsync(SqlCommitGatePlan plan, CancellationToken ct)
    {
        try
        {
            await SqlSessionAuthorityReader.RequireWritablePrimaryAsync(db, ct);
            return await db.Database.BeginTransactionAsync(plan.Isolation, ct);
        }
        catch (Exception error) when (SqlAuthorityErrors.IsUnavailable(error)) { throw new AuthorityUnavailableException(); }
    }
}
