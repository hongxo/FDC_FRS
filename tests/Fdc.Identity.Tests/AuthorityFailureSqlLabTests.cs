using System.Data.Common;
using Fdc.Identity.Application;
using Fdc.Identity.Infrastructure;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace Fdc.Identity.Tests;

[Collection("IAM database lab")]
public sealed class AuthorityFailureSqlLabTests
{
    [SqlLabFact] public async Task P4_real_sql_missing_database_connection_fails_closed()
    {
        var connection = await ConfirmedConnection();
        var builder = new SqlConnectionStringBuilder(connection) { InitialCatalog = "FdcIam_Missing_" + Guid.NewGuid().ToString("N"), ConnectTimeout = 2, Pooling = false };
        var reader = new SqlSessionAuthorityReader(new Factory(builder.ConnectionString, false));
        await Assert.ThrowsAsync<AuthorityUnavailableException>(() => reader.ReadAsync(new string('A', 64), default));
    }
    [SqlLabFact] public async Task P4_real_sql_command_timeout_fails_closed()
    {
        var reader = new SqlSessionAuthorityReader(new Factory(await ConfirmedConnection(), true));
        await Assert.ThrowsAsync<AuthorityUnavailableException>(() => reader.ReadAsync(new string('A', 64), default));
    }
    private static async Task<string> ConfirmedConnection()
    {
        var connection = Environment.GetEnvironmentVariable("FDC_IAM_LAB_CONNECTION") ?? throw new InvalidOperationException("Lab required.");
        var settings = new SqlConnectionStringBuilder(connection);
        if (settings.DataSource != "lpc:." || settings.InitialCatalog != "FdcIam_DevelopmentLab") throw new InvalidOperationException("Unexpected target.");
        await using var db = new IamDbContext(new DbContextOptionsBuilder<IamDbContext>().UseSqlServer(connection).Options);
        var valid = await db.Database.SqlQueryRaw<int>("SELECT CASE WHEN CONVERT(nvarchar(128),SERVERPROPERTY('MachineName'))=N'XOTH-ETC-2' AND DB_NAME()=N'FdcIam_DevelopmentLab' AND EXISTS(SELECT 1 FROM sys.extended_properties WHERE class=0 AND name=N'Fdc.Disposable' AND TRY_CONVERT(bit,value)=1) THEN 1 ELSE 0 END AS [Value]").SingleAsync();
        Assert.Equal(1, valid); return connection;
    }
    private sealed class Factory(string connection, bool delay) : IDbContextFactory<IamDbContext>
    {
        public IamDbContext CreateDbContext()
        {
            var options = new DbContextOptionsBuilder<IamDbContext>().UseSqlServer(connection, sql => sql.CommandTimeout(1));
            if (delay) options.AddInterceptors(new Delay());
            return new(options.Options);
        }
    }
    // Test-only bounded delay on this connection. No server outage, configuration change or production query.
    private sealed class Delay : DbCommandInterceptor
    {
        public override ValueTask<InterceptionResult<DbDataReader>> ReaderExecutingAsync(DbCommand command, CommandEventData eventData,
            InterceptionResult<DbDataReader> result, CancellationToken cancellationToken = default)
        {
            command.CommandText = "WAITFOR DELAY '00:00:03'; " + command.CommandText;
            return ValueTask.FromResult(result);
        }
    }
}
