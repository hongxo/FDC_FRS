using Fdc.Identity.Infrastructure;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace Fdc.Testing;

// xUnit creates an instance for each test. These suites belong to nonparallel collections.
public abstract class A10IamSqlTest : IAsyncLifetime
{
    private static string? currentConnection;
    private static string? currentName;
    private readonly string run = Guid.NewGuid().ToString("N");
    private string? master;
    private bool created;
    public static bool Enabled => Environment.GetEnvironmentVariable("FDC_RUN_A10_SQL") == "1";
    public static string TargetDatabase => currentName ?? "FdcIam_DevelopmentLab";
    public static string? Connection => currentConnection ?? Environment.GetEnvironmentVariable("FDC_IAM_LAB_CONNECTION");

    public static string GuardSql(string statement)
    {
        if (!Enabled) return statement;
        if (currentName is null) throw new InvalidOperationException("Owned A.10 fixture not initialized.");
        return statement.Replace("N'FdcIam_DevelopmentLab'", $"N'{currentName}'", StringComparison.Ordinal);
    }

    public async Task InitializeAsync()
    {
        if (!Enabled) return;
        if (currentName is not null) throw new InvalidOperationException("Parallel A.10 IAM fixtures are forbidden.");
        var settings = new SqlConnectionStringBuilder(Environment.GetEnvironmentVariable("FDC_A10_SQL_MASTER"));
        if (settings.DataSource != "lpc:." || settings.InitialCatalog != "master" || !settings.IntegratedSecurity ||
            settings.AttachDBFilename.Length != 0 || settings.FailoverPartner.Length != 0)
            throw new InvalidOperationException("Opted-in Windows authentication local master required.");
        settings.Pooling = false;
        master = settings.ConnectionString;
        currentName = "FDC_A10_TEST_" + run;
        settings.InitialCatalog = currentName;
        currentConnection = settings.ConnectionString;
        try
        {
            await using var admin = new SqlConnection(master);
            await admin.OpenAsync();
            await HostAsync(admin);
            await CommandAsync(admin, $"CREATE DATABASE [{currentName}];");
            created = true;
            await admin.ChangeDatabaseAsync(currentName);
            await CommandAsync(admin, """
                EXEC sys.sp_addextendedproperty @name=N'Fdc.A10.Run', @value=@run;
                EXEC sys.sp_addextendedproperty @name=N'Fdc.Disposable', @value=1;
                EXEC sys.sp_addextendedproperty @name=N'Fdc.Environment', @value=N'DevelopmentLab';
                EXEC sys.sp_set_session_context @key=N'Fdc.ConfirmedDisposableLab', @value=1, @read_only=1;
                EXEC sys.sp_set_session_context @key=N'Fdc.ExpectedLabDatabase', @value=@name, @read_only=1;
                """);
            await using var iam = new IamDbContext(new DbContextOptionsBuilder<IamDbContext>().UseSqlServer(admin).Options);
            await iam.Database.MigrateAsync();
        }
        catch { await DisposeAsync(); throw; }
    }

    public async Task DisposeAsync()
    {
        try
        {
            if (!created) return;
            await using var admin = new SqlConnection(master);
            await admin.OpenAsync();
            await HostAsync(admin);
            await CommandAsync(admin, $"""
                IF NOT EXISTS(SELECT 1 FROM [{currentName}].sys.extended_properties
                    WHERE class=0 AND name=N'Fdc.A10.Run' AND CONVERT(nvarchar(32),value)=@run)
                    THROW 51000, 'Owned A.10 marker required for cleanup.', 1;
                DROP DATABASE [{currentName}];
                IF DB_ID(@name) IS NOT NULL THROW 51000, 'A.10 cleanup failed.', 1;
                """);
            created = false;
        }
        finally { currentName = null; currentConnection = null; }
    }

    private static async Task HostAsync(SqlConnection sql)
    {
        await using var command = new SqlCommand("SELECT CONVERT(nvarchar(128),SERVERPROPERTY('MachineName'));", sql);
        if ((string?)await command.ExecuteScalarAsync() != "XOTH-ETC-2")
            throw new InvalidOperationException("Unexpected SQL host.");
    }

    private async Task CommandAsync(SqlConnection sql, string statement)
    {
        await using var command = new SqlCommand(statement, sql);
        command.Parameters.AddWithValue("@run", run);
        command.Parameters.AddWithValue("@name", currentName);
        await command.ExecuteNonQueryAsync();
    }
}
