using System.Data.Common;
using Fdc.Center.Persistence;
using Fdc.Identity.Infrastructure;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Fdc.Center.Tests;

internal sealed class A9DisposableSql : IAsyncDisposable
{
    private const string Host = "XOTH-ETC-2";
    private const string CatalogGuard = "DB_NAME()<>N'FDC_Center_Lab'";
    private readonly string runId = Guid.NewGuid().ToString("N");
    private readonly string master;
    private bool created;
    private string Name => "FDC_A9_TEST_" + runId;
    private string Connection { get; }

    public static bool Enabled => Environment.GetEnvironmentVariable("FDC_RUN_A9_DISPOSABLE") == "1";

    private A9DisposableSql(SqlConnectionStringBuilder settings)
    {
        if (!Enabled || settings.DataSource != "lpc:." || settings.InitialCatalog != "master" ||
            settings.AttachDBFilename.Length != 0 || settings.FailoverPartner.Length != 0 ||
            !settings.IntegratedSecurity)
            throw new InvalidOperationException("A.9 requires opted-in local Shared Memory master and Windows authentication.");
        settings.Pooling = false;
        master = settings.ConnectionString;
        settings.InitialCatalog = Name;
        Connection = settings.ConnectionString;
    }

    public static async Task<A9DisposableSql> CreateAsync()
    {
        var fixture = new A9DisposableSql(new SqlConnectionStringBuilder(
            Environment.GetEnvironmentVariable("FDC_A9_SQL_MASTER")));
        try
        {
            await using (var admin = new SqlConnection(fixture.master))
            {
                await admin.OpenAsync();
                await VerifyHostAsync(admin);
                await ExecuteAsync(admin, $"CREATE DATABASE [{fixture.Name}];");
                fixture.created = true;
                await admin.ChangeDatabaseAsync(fixture.Name);
                await ExecuteAsync(admin, "EXEC sys.sp_addextendedproperty @name=N'Fdc.A9.Run', @value=@run;", fixture.runId);
                await ExecuteAsync(admin, "EXEC sys.sp_addextendedproperty @name=N'Fdc.Disposable', @value=1;");
            }
            await fixture.MigrateAsync();
            return fixture;
        }
        catch
        {
            await fixture.DisposeAsync();
            throw;
        }
    }

    private async Task MigrateAsync()
    {
        await using var iam = new IamDbContext(new DbContextOptionsBuilder<IamDbContext>().UseSqlServer(Connection).Options);
        await iam.Database.OpenConnectionAsync();
        var sql = (SqlConnection)iam.Database.GetDbConnection();
        await VerifyHostAsync(sql);
        await VerifyOwnershipAsync(sql);
        await ExecuteAsync(sql, """
            EXEC sys.sp_set_session_context @key=N'Fdc.ConfirmedDisposableLab', @value=1, @read_only=1;
            EXEC sys.sp_set_session_context @key=N'Fdc.ExpectedLabDatabase', @value=@name, @read_only=1;
            """, name: Name);
        await iam.Database.MigrateAsync();

        await using var center = new CenterDbContext(new DbContextOptionsBuilder<CenterDbContext>().UseSqlServer(Connection).Options);
        var migrations = center.GetService<IMigrationsAssembly>();
        var history = center.GetService<IHistoryRepository>();
        foreach (var (id, type) in migrations.Migrations)
        {
            var migration = migrations.CreateMigration(type, center.Database.ProviderName!);
            var commands = center.GetService<IMigrationsSqlGenerator>().Generate(migration.UpOperations);
            await using var transaction = (SqlTransaction)await sql.BeginTransactionAsync();
            foreach (var command in commands)
            {
                // Preserve the original batch boundaries and every guard except the fixed catalog name.
                if (command.TransactionSuppressed || !command.CommandText.Contains(CatalogGuard, StringComparison.Ordinal))
                    throw new InvalidOperationException("Unexpected Center migration command.");
                var batch = command.CommandText.Replace(CatalogGuard, $"DB_NAME()<>N'{Name}'", StringComparison.Ordinal);
                await using var migrate = new SqlCommand(batch, sql, transaction);
                await migrate.ExecuteNonQueryAsync();
            }
            await using var record = new SqlCommand(history.GetInsertScript(new HistoryRow(id, ProductInfo.GetVersion())), sql, transaction);
            await record.ExecuteNonQueryAsync();
            await transaction.CommitAsync();
        }

        await ExecuteAsync(sql, """
            CREATE USER [fdc_a9_runtime] WITHOUT LOGIN;
            GRANT SELECT, INSERT, UPDATE, DELETE ON SCHEMA::iam TO [fdc_a9_runtime];
            GRANT SELECT, INSERT, UPDATE, DELETE ON SCHEMA::catalog TO [fdc_a9_runtime];
            GRANT SELECT, INSERT, UPDATE, DELETE ON SCHEMA::ops TO [fdc_a9_runtime];
            GRANT SELECT, INSERT, UPDATE, DELETE ON SCHEMA::ingest TO [fdc_a9_runtime];
            GRANT SELECT, INSERT, UPDATE, DELETE ON SCHEMA::audit TO [fdc_a9_runtime];
            GRANT SELECT ON OBJECT::dbo.__EFMigrationsHistory TO [fdc_a9_runtime];
            GRANT SHOWPLAN TO [fdc_a9_runtime];
            """);
    }

    public DbContextOptionsBuilder<T> Options<T>() where T : DbContext =>
        new DbContextOptionsBuilder<T>().UseSqlServer(Connection).AddInterceptors(new RuntimeUser(this));

    private sealed class RuntimeUser(A9DisposableSql fixture) : DbConnectionInterceptor
    {
        public override async Task ConnectionOpenedAsync(DbConnection connection, ConnectionEndEventData eventData,
            CancellationToken cancellationToken = default)
        {
            var sql = (SqlConnection)connection;
            await fixture.VerifyOwnershipAsync(sql);
            await ExecuteAsync(sql, "EXECUTE AS USER=N'fdc_a9_runtime';");
        }

        public override void ConnectionOpened(DbConnection connection, ConnectionEndEventData eventData) =>
            throw new InvalidOperationException("A.9 fixture requires asynchronous SQL connections.");
    }

    private static async Task VerifyHostAsync(SqlConnection connection)
    {
        await using var command = new SqlCommand("SELECT CONVERT(nvarchar(128), SERVERPROPERTY('MachineName'));", connection);
        if ((string?)await command.ExecuteScalarAsync() != Host)
            throw new InvalidOperationException("SQL Server host outside the A.9 allowlist.");
    }

    private Task VerifyOwnershipAsync(SqlConnection sql) => ExecuteAsync(sql, """
        IF DB_NAME() <> @name OR NOT EXISTS (
            SELECT 1 FROM sys.extended_properties WHERE class=0 AND name=N'Fdc.A9.Run'
              AND CONVERT(nvarchar(32),value)=@run)
            THROW 51000, 'A.9 database ownership validation failed.', 1;
        """, runId, Name);

    private static async Task ExecuteAsync(SqlConnection connection, string statement, string? run = null, string? name = null)
    {
        await using var command = new SqlCommand(statement, connection);
        if (run is not null) command.Parameters.AddWithValue("@run", run);
        if (name is not null) command.Parameters.AddWithValue("@name", name);
        await command.ExecuteNonQueryAsync();
    }

    public async ValueTask DisposeAsync()
    {
        if (!created) return;
        await using var sql = new SqlConnection(master);
        await sql.OpenAsync();
        await VerifyHostAsync(sql);
        await ExecuteAsync(sql, $"""
            IF NOT EXISTS (SELECT 1 FROM [{Name}].sys.extended_properties WHERE class=0
                AND name=N'Fdc.A9.Run' AND CONVERT(nvarchar(32),value)=@run)
                THROW 51000, 'A.9 cleanup refused: ownership marker mismatch.', 1;
            DROP DATABASE [{Name}];
            IF DB_ID(@name) IS NOT NULL THROW 51000, 'A.9 cleanup verification failed.', 1;
            """, runId, Name);
        created = false;
    }
}
