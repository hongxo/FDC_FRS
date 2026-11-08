using System.Text.RegularExpressions;
using Fdc.Center.Persistence;
using Microsoft.EntityFrameworkCore;
if(args.Length>0 && args[0]=="provision-runtime"){await Fdc.Center.Database.LabRuntimePrincipal.RunAsync(args);return;}
if(args.Length>0 && args[0]=="recover"){await Fdc.Center.Database.LabRecovery.RunAsync(args);return;}
if(args.Length>0 && args[0]=="deploy"){await Fdc.Center.Database.LabDeployment.RunAsync(args);return;}
if(args.Length!=2 || args[0]!="generate")throw new ArgumentException("Use generate <workspace-output>; no database is opened by generation.");
var output=Path.GetFullPath(args[1]);Directory.CreateDirectory(output);
if(File.Exists(Path.Combine(output,"CurrentSchema.review.sql")))throw new InvalidOperationException("Never overwrite an existing review artifact. Historical EF migrations are immutable; generate into a new review directory.");
using var db=new CenterDbContext(new DbContextOptionsBuilder<CenterDbContext>().UseSqlServer("Server=unconfigured.invalid;Database=NeverConnect;Integrated Security=true").Options);
var guard="""
IF DB_NAME()<>N'FDC_Center_Lab' OR CONVERT(nvarchar(128),SERVERPROPERTY('MachineName'))<>N'XOTH-ETC-2'
 OR COALESCE(TRY_CONVERT(bit,SESSION_CONTEXT(N'Fdc.ConfirmedDisposableLab')),0)<>1
 OR COALESCE(CONVERT(nvarchar(128),SESSION_CONTEXT(N'Fdc.ExpectedLabDatabase')),N'')<>DB_NAME()
 OR NOT EXISTS(SELECT 1 FROM sys.extended_properties WHERE class=0 AND name=N'Fdc.Disposable' AND TRY_CONVERT(bit,value)=1)
 THROW 51100,'Exact explicitly confirmed Center lab required.',1;
""";
string Guard(string sql)=>string.Join("\nGO\n",Regex.Split(sql,@"^\s*GO\s*$",RegexOptions.Multiline|RegexOptions.IgnoreCase).Where(s=>!string.IsNullOrWhiteSpace(s)).Select(s=>guard+"\nEXEC(N'"+s.Replace("'","''",StringComparison.Ordinal)+"');\n"));
var views="""
CREATE OR ALTER VIEW [catalog].[vStationInventory] AS
 SELECT s.Id AS StationId,(SELECT COUNT_BIG(*) FROM catalog.IedDevice d WHERE d.StationId=s.Id) AS IedCount,
 (SELECT COUNT_BIG(*) FROM ops.CollectorRegistration c WHERE c.StationId=s.Id) AS CollectorCount FROM catalog.Station s;
GO
CREATE OR ALTER VIEW [ingest].[vRecordSummary] AS
 SELECT StationId,State,COUNT_BIG(*) AS RecordCount FROM ingest.DisturbanceRecord GROUP BY StationId,State;
GO
CREATE OR ALTER VIEW [ops].[vFaultSummary] AS
 SELECT StationId,AnalysisStatus,COUNT_BIG(*) AS EventCount FROM ops.FaultEvent GROUP BY StationId,AnalysisStatus;
GO
-- No members/logins/credentials. These roles are lab permission contracts, not IAM grants.
IF DATABASE_PRINCIPAL_ID(N'fdc_center_runtime') IS NULL CREATE ROLE fdc_center_runtime;
IF DATABASE_PRINCIPAL_ID(N'fdc_center_reporting') IS NULL CREATE ROLE fdc_center_reporting;
IF DATABASE_PRINCIPAL_ID(N'fdc_center_audit_writer') IS NULL CREATE ROLE fdc_center_audit_writer;
IF DATABASE_PRINCIPAL_ID(N'fdc_center_migration') IS NULL CREATE ROLE fdc_center_migration;
IF DATABASE_PRINCIPAL_ID(N'fdc_center_maintenance') IS NULL CREATE ROLE fdc_center_maintenance;
GRANT SELECT ON OBJECT::iam.[User] TO fdc_center_runtime;
GRANT SELECT ON OBJECT::iam.PersonReference TO fdc_center_runtime;
GRANT SELECT ON OBJECT::iam.Session TO fdc_center_runtime;
GRANT SELECT ON OBJECT::iam.AuthorityVersion TO fdc_center_runtime;
DENY INSERT,UPDATE,DELETE ON SCHEMA::iam TO fdc_center_runtime;
DENY SELECT ON OBJECT::iam.PasswordCredential TO fdc_center_runtime;
DENY SELECT ON OBJECT::iam.MfaCredential TO fdc_center_runtime;
DENY INSERT,UPDATE,DELETE ON SCHEMA::audit TO fdc_center_runtime;
DENY SELECT,INSERT,UPDATE,DELETE ON SCHEMA::ingest TO fdc_center_runtime;
DENY SELECT ON OBJECT::catalog.vStationInventory TO fdc_center_reporting;
DENY SELECT ON OBJECT::ingest.vRecordSummary TO fdc_center_reporting;
DENY SELECT ON OBJECT::ops.vFaultSummary TO fdc_center_reporting;
DENY INSERT,UPDATE,DELETE ON SCHEMA::iam TO fdc_center_reporting;
DENY INSERT,UPDATE,DELETE ON SCHEMA::audit TO fdc_center_reporting;
DENY INSERT,UPDATE,DELETE ON SCHEMA::ingest TO fdc_center_reporting;
-- Pending D09/D10: no authority DML or archive signing powers are granted to runtime/audit roles.
GRANT VIEW DEFINITION TO fdc_center_migration;
GRANT VIEW DATABASE STATE TO fdc_center_maintenance;
GO
""";
await File.WriteAllTextAsync(Path.Combine(output,"CurrentSchema.review.sql"),Guard(db.Database.GenerateCreateScript()+views));
var entities=db.Model.GetEntityTypes().Where(e=>typeof(CenterRow).IsAssignableFrom(e.ClrType)).ToList();var ordered=new List<string>();
while(entities.Count>0){var leaf=entities.First(e=>!entities.Any(other=>other!=e&&other.GetForeignKeys().Any(f=>f.PrincipalEntityType==e)));ordered.Add($"[{leaf.GetSchema()}].[{leaf.GetTableName()}]");entities.Remove(leaf);}
var down="DROP VIEW IF EXISTS catalog.vStationInventory; DROP VIEW IF EXISTS ingest.vRecordSummary; DROP VIEW IF EXISTS ops.vFaultSummary;\n";
foreach(var table in ordered)down+=$"IF EXISTS(SELECT 1 FROM {table}) THROW 51101,'Nonempty domain cannot be destructively rolled back.',1;\n";
foreach(var table in ordered)down+=$"DROP TABLE {table};\n";
await File.WriteAllTextAsync(Path.Combine(output,"CurrentSchema.empty-down.review.sql"),Guard(down));
var dictionary=db.Model.GetEntityTypes().Where(e=>typeof(CenterRow).IsAssignableFrom(e.ClrType)).Select(e=>new{Schema=e.GetSchema(),Table=e.GetTableName(),Columns=e.GetProperties().Select(p=>new{Name=p.Name,Type=p.GetColumnType(),Nullable=p.IsNullable,Concurrency=p.IsConcurrencyToken}),ForeignKeys=e.GetForeignKeys().Select(f=>new{Columns=f.Properties.Select(p=>p.Name),Target=f.PrincipalEntityType.GetSchema()+"."+f.PrincipalEntityType.GetTableName(),Delete=f.DeleteBehavior.ToString()})});
await File.WriteAllTextAsync(Path.Combine(output,"model.json"),System.Text.Json.JsonSerializer.Serialize(dictionary,new System.Text.Json.JsonSerializerOptions{WriteIndented=true}));
Console.WriteLine($"Generated {ordered.Count} Center tables; SQL executions=0. Existing IAM/catalog reference tables excluded.");
