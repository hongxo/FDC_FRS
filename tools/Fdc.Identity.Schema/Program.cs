using Fdc.Identity.Infrastructure;
using Microsoft.EntityFrameworkCore;

// Offline artifact generator ONLY. Never calls OpenConnection, EnsureCreated, Migrate or executes SQL.
if (args.Length != 1) throw new ArgumentException("Pass an explicit workspace output directory.");
var output = Path.GetFullPath(args[0]);
Directory.CreateDirectory(output);
using var db = new IamDbContext(new DbContextOptionsBuilder<IamDbContext>()
    .UseSqlServer("Server=unconfigured.invalid;Database=NeverConnect;Integrated Security=true;Encrypt=true").Options);
var guard = """
-- REVIEW CANDIDATE. Requires owner D09/schema disposition and confirmed disposable lab authorization.
-- Session context must be set explicitly by the authorized migration operator, never by application startup.
IF COALESCE(TRY_CONVERT(bit, SESSION_CONTEXT(N'Fdc.ConfirmedDisposableLab')),0) <> 1
 OR COALESCE(CONVERT(nvarchar(128),SESSION_CONTEXT(N'Fdc.ExpectedLabDatabase')),N'') <> DB_NAME()
 THROW 51000, 'Confirmed disposable lab and exact expected database required.', 1;
GO

""";
var sql = db.Database.GenerateCreateScript();
var triggers = """

CREATE OR ALTER TRIGGER [iam].[TR_Role_ImmutableCode] ON [iam].[Role] AFTER UPDATE AS
BEGIN
 SET NOCOUNT ON;
 IF EXISTS (SELECT 1 FROM inserted i JOIN deleted d ON i.Id=d.Id WHERE i.RoleCode COLLATE Latin1_General_100_BIN2 <> d.RoleCode COLLATE Latin1_General_100_BIN2)
  THROW 51001, 'RoleCode is immutable.', 1;
END;
GO
CREATE OR ALTER TRIGGER [iam].[TR_Role_NoDelete] ON [iam].[Role] INSTEAD OF DELETE AS
BEGIN
 THROW 51002, 'Deactivate roles; preserve history.', 1;
END;
GO
CREATE OR ALTER TRIGGER [audit].[TR_SecurityEvent_AppendOnly] ON [audit].[SecurityEvent] INSTEAD OF UPDATE, DELETE AS
BEGIN
 THROW 51003, 'Security events are append-only.', 1;
END;
GO
CREATE OR ALTER TRIGGER [iam].[TR_RoleChangeRequest_ImmutableRevision] ON [iam].[RoleChangeRequest] AFTER UPDATE AS
BEGIN
 SET NOCOUNT ON;
 IF EXISTS (SELECT 1 FROM inserted i JOIN deleted d ON i.Id=d.Id
   WHERE i.Revision<>d.Revision OR i.RequesterUserId<>d.RequesterUserId OR i.Digest<>d.Digest
      OR i.PayloadJson COLLATE Latin1_General_100_BIN2 <> d.PayloadJson COLLATE Latin1_General_100_BIN2
      OR i.BaseRoleVersion<>d.BaseRoleVersion OR i.PolicyVersion<>d.PolicyVersion OR i.CatalogVersion<>d.CatalogVersion
      OR i.ExpiresAt<>d.ExpiresAt OR i.IdempotencyKey<>d.IdempotencyKey
      OR ISNULL(CONVERT(nvarchar(36),i.RoleId),N'')<>ISNULL(CONVERT(nvarchar(36),d.RoleId),N''))
  THROW 51004, 'Create a new immutable request revision.', 1;
END;
GO
-- No logins, users, default roles/permissions, sessions or station grants are installed.
IF DATABASE_PRINCIPAL_ID(N'fdc_iam_catalog_reader') IS NULL CREATE ROLE [fdc_iam_catalog_reader];
GRANT SELECT ON OBJECT::[iam].[Role] TO [fdc_iam_catalog_reader];
GRANT SELECT ON OBJECT::[iam].[Permission] TO [fdc_iam_catalog_reader];
DENY INSERT,UPDATE,DELETE ON SCHEMA::[iam] TO [fdc_iam_catalog_reader];
DENY SELECT ON OBJECT::[iam].[PasswordCredential] TO [fdc_iam_catalog_reader];
DENY SELECT ON OBJECT::[iam].[MfaCredential] TO [fdc_iam_catalog_reader];
DENY SELECT ON OBJECT::[iam].[Session] TO [fdc_iam_catalog_reader];
GO
""";
// Recheck the lab guard in EVERY batch: clients may continue after an earlier GO batch fails.
// EXEC keeps CREATE TRIGGER/SCHEMA first-statement rules inside a guarded batch.
string GuardBatches(string content) => string.Join("\nGO\n", System.Text.RegularExpressions.Regex.Split(content,
    @"^\s*GO\s*$", System.Text.RegularExpressions.RegexOptions.Multiline | System.Text.RegularExpressions.RegexOptions.IgnoreCase)
    .Where(b => !string.IsNullOrWhiteSpace(b)).Select(b => guard.Replace("GO\n", "", StringComparison.Ordinal)
       + "\nEXEC(N'" + b.Replace("'", "''", StringComparison.Ordinal) + "');\n"));
await File.WriteAllTextAsync(Path.Combine(output, "001_IamFoundation.up.sql"), GuardBatches(sql + triggers));
var entities = db.Model.GetEntityTypes().ToList();
var ordered = new List<string>();
while (entities.Count > 0)
{
    var leaf = entities.First(e => !entities.Any(other => other != e && other.GetForeignKeys().Any(f => f.PrincipalEntityType == e)));
    ordered.Add($"[{leaf.GetSchema()}].[{leaf.GetTableName()}]"); entities.Remove(leaf);
}
var rollback = "-- Refuse rollback if any IAM data exists. Restore/roll-forward requires a separate reviewed runbook.\n";
foreach (var table in ordered) rollback += $"IF OBJECT_ID(N'{table}',N'U') IS NOT NULL AND EXISTS(SELECT 1 FROM {table}) THROW 51005, 'Nonempty IAM foundation cannot be destructively rolled back.', 1;\n";
foreach (var table in ordered) rollback += $"DROP TABLE IF EXISTS {table};\n";
rollback += "-- Shared schemas and SQL roles are deliberately retained; remove membership/roles only with separate DBA review.\n";
await File.WriteAllTextAsync(Path.Combine(output, "001_IamFoundation.down.sql"), GuardBatches(rollback));
Console.WriteLine($"Generated SQL for {ordered.Count} tables and guarded empty-lab rollback. SQL executions: 0.");
