using System.Security.Cryptography;
using System.Text;
using Fdc.Testing;
using Microsoft.Data.SqlClient;

namespace Fdc.Center.Tests;

internal sealed class A11RuntimeProfile : IAsyncDisposable
{
    private readonly string adminConnection;
    private readonly string login;
    private byte[] protectedProfile = [];
    private bool created;
    public Guid RunId { get; }

    private A11RuntimeProfile()
    {
        adminConnection = A10IamSqlTest.Connection ?? throw new InvalidOperationException("Owned database required.");
        var name = A10IamSqlTest.TargetDatabase;
        if (!A10IamSqlTest.Enabled || !name.StartsWith("FDC_A10_TEST_", StringComparison.Ordinal) ||
            !Guid.TryParseExact(name["FDC_A10_TEST_".Length..], "N", out var run))
            throw new InvalidOperationException("Owned database required.");
        RunId = run;
        login = "fdc_a11_" + run.ToString("N");
    }

    public static async Task<A11RuntimeProfile> CreateAsync()
    {
        var fixture = new A11RuntimeProfile();
        try { await fixture.InitializeAsync(); return fixture; }
        catch { await fixture.DisposeAsync(); throw; }
    }

    private async Task InitializeAsync()
    {
        await using var db = new SqlConnection(adminConnection);
        await db.OpenAsync();
        await VerifyAsync(db);
        await using (var context = new SqlCommand("""
            EXEC sys.sp_set_session_context @key=N'Fdc.ConfirmedDisposableLab', @value=1;
            EXEC sys.sp_set_session_context @key=N'Fdc.ExpectedLabDatabase', @value=@name;
            """, db))
        {
            context.Parameters.AddWithValue("@name", db.Database);
            await context.ExecuteNonQueryAsync();
        }
        await A9DisposableSql.ApplyCenterMigrationsAsync(db, db.Database);
        var password = "A!9" + Convert.ToHexString(RandomNumberGenerator.GetBytes(32));
        await using (var create = new SqlCommand("""
            DECLARE @statement nvarchar(max)=N'CREATE LOGIN '+QUOTENAME(@login)+N' WITH PASSWORD='+
                QUOTENAME(@password,CHAR(39))+N', CHECK_POLICY=ON, CHECK_EXPIRATION=ON';
            EXEC sys.sp_executesql @statement;
            """, db))
        {
            create.Parameters.AddWithValue("@login", login);
            create.Parameters.AddWithValue("@password", password);
            await create.ExecuteNonQueryAsync();
            created = true;
        }
        await using (var grants = new SqlCommand($"""
            CREATE USER [{login}] FOR LOGIN [{login}];
            GRANT SELECT ON OBJECT::iam.[User] TO [{login}];
            GRANT SELECT ON OBJECT::iam.PersonReference TO [{login}];
            GRANT SELECT ON OBJECT::iam.Session TO [{login}];
            GRANT SELECT ON OBJECT::iam.AuthorityVersion TO [{login}];
            GRANT SELECT ON OBJECT::ingest.DisturbanceRecord(Id) TO [{login}];
            """, db))
            await grants.ExecuteNonQueryAsync();
        var profile = new SqlConnectionStringBuilder(adminConnection)
        {
            IntegratedSecurity = false, UserID = login, Password = password, Pooling = false
        };
        if (!OperatingSystem.IsWindows()) throw new PlatformNotSupportedException();
        var plaintext = Encoding.UTF8.GetBytes(profile.ConnectionString);
        try { protectedProfile = ProtectedData.Protect(plaintext, RunId.ToByteArray(), DataProtectionScope.CurrentUser); }
        finally { CryptographicOperations.ZeroMemory(plaintext); }
    }

    public string ReadConnection()
    {
        if (!OperatingSystem.IsWindows()) throw new PlatformNotSupportedException();
        var plaintext = ProtectedData.Unprotect(protectedProfile, RunId.ToByteArray(), DataProtectionScope.CurrentUser);
        try { return Encoding.UTF8.GetString(plaintext); }
        finally { CryptographicOperations.ZeroMemory(plaintext); }
    }

    public void AssertWrongEntropyDenied()
    {
        Assert.Throws<CryptographicException>(() =>
        {
            if (!OperatingSystem.IsWindows()) throw new PlatformNotSupportedException();
            ProtectedData.Unprotect(protectedProfile, Guid.Empty.ToByteArray(), DataProtectionScope.CurrentUser);
        });
    }

    private async Task VerifyAsync(SqlConnection db)
    {
        await using var guard = new SqlCommand("""
            IF CONVERT(nvarchar(128),SERVERPROPERTY('MachineName'))<>N'XOTH-ETC-2'
              OR NOT EXISTS(SELECT 1 FROM sys.extended_properties WHERE class=0 AND name=N'Fdc.A10.Run'
                  AND CONVERT(nvarchar(32),value)=@run)
                THROW 51000,'Owned A.11 runtime fixture required.',1;
            """, db);
        guard.Parameters.AddWithValue("@run", RunId.ToString("N"));
        await guard.ExecuteNonQueryAsync();
    }

    public async ValueTask DisposeAsync()
    {
        CryptographicOperations.ZeroMemory(protectedProfile);
        if (!created) return;
        await using var db = new SqlConnection(adminConnection);
        await db.OpenAsync();
        await VerifyAsync(db);
        await using var drop = new SqlCommand($"DROP LOGIN [{login}]; IF SUSER_ID(@login) IS NOT NULL THROW 51000,'Runtime login cleanup failed.',1;", db);
        drop.Parameters.AddWithValue("@login", login);
        await drop.ExecuteNonQueryAsync();
        created = false;
    }
}
