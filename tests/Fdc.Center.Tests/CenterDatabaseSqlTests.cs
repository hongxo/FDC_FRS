using Fdc.Center.Persistence;
using Fdc.Core;
using Fdc.Identity.Application;
using Fdc.Identity.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
namespace Fdc.Center.Tests;

[CollectionDefinition("Center database lab",DisableParallelization=true)] public sealed class CenterDatabaseCollection { }
[Collection("Center database lab")]
public sealed partial class CenterDatabaseSqlTests
{
 [CenterFoundationFact] public async Task Schema_has_53_tables_canonical_iam_and_three_views()
 {
  await using var f=await Fixture.Create();await using var db=f.Db();
  Assert.Equal(53,await db.Database.SqlQueryRaw<int>("SELECT COUNT(*) AS [Value] FROM sys.tables WHERE name<>N'__EFMigrationsHistory'").SingleAsync());
  Assert.Equal(3,await db.Database.SqlQueryRaw<int>("SELECT COUNT(*) AS [Value] FROM sys.views WHERE is_ms_shipped=0").SingleAsync());
  Assert.Equal(6,await db.Database.SqlQueryRaw<int>("SELECT COUNT(*) AS [Value] FROM dbo.__EFMigrationsHistory").SingleAsync());
 }
 [CenterFoundationFact] public async Task Composite_station_fk_checks_unique_and_rowversion_are_enforced()
 {
  await using var f=await Fixture.Create();await using(var db=f.Db())
  {
   db.Add(new CollectorIed{StationId=f.OtherStation,CollectorId=f.Collector,IedId=f.Ied,Active=true,IsSynthetic=true});await Assert.ThrowsAsync<DbUpdateException>(()=>db.SaveChangesAsync());
  }
  await using(var db=f.Db()){db.Add(new IedDevice{StationId=f.Station,Code="IED-SYNTHETIC",DisplayName="duplicate",IsSynthetic=true});await Assert.ThrowsAsync<DbUpdateException>(()=>db.SaveChangesAsync());}
  await using(var db=f.Db()){db.Add(new CollectorHeartbeat{StationId=f.Station,CollectorId=f.Collector,ReceivedAtUtc=DateTimeOffset.UtcNow,Status="Healthy",QueueDepth=-1,IsSynthetic=true});await Assert.ThrowsAsync<DbUpdateException>(()=>db.SaveChangesAsync());}
  await using(var orphan=f.Db()){await using var rollback=await orphan.Database.BeginTransactionAsync();orphan.Add(new OperationalEvent{StationId=Guid.NewGuid(),CollectorId=null,Code="SYNTHETIC",CorrelationId="SYNTHETIC",IsSynthetic=true});await Assert.ThrowsAsync<DbUpdateException>(()=>orphan.SaveChangesAsync());await rollback.RollbackAsync();}
  await using var a=f.Db();await using var b=f.Db();var first=await a.Set<IedDevice>().SingleAsync(x=>x.Id==f.Ied);var stale=await b.Set<IedDevice>().SingleAsync(x=>x.Id==f.Ied);first.DisplayName="edited";await a.SaveChangesAsync();stale.DisplayName="stale";await Assert.ThrowsAsync<DbUpdateConcurrencyException>(()=>b.SaveChangesAsync());
 }
 [CenterFoundationFact] public async Task Real_files_ingest_replay_and_concurrent_duplicates_are_atomic()
 {
  await using var f=await Fixture.Create();var package=await f.Package();
  async Task<IngestionReceipt> Send(){await using var db=f.Db();return await new SqlRecordIngestion(db,new FixtureMachine(f),TimeProvider.System).AcceptAsync(package,Guid.NewGuid().ToString("N"),true,default);}
  var first=await Send();Assert.False(first.Duplicate);var duplicates=await Task.WhenAll(Send(),Send());Assert.All(duplicates,r=>{Assert.True(r.Duplicate);Assert.Equal(first.RecordId,r.RecordId);});
  await using var check=f.Db();Assert.Equal(1,await check.Set<DisturbanceRecord>().CountAsync());Assert.Equal(package.ManifestJson,(await check.Set<DisturbanceRecord>().SingleAsync()).ManifestSnapshotJson);Assert.Equal(2,await check.Set<RecordFile>().CountAsync());Assert.Equal(3,await check.Set<IngestionAttempt>().CountAsync());Assert.Equal(1,await check.Set<ProcessingOutbox>().CountAsync());Assert.Equal(1,await check.Set<RecordStatusHistory>().CountAsync());
 }
 [CenterFoundationFact] public async Task Required_outbox_failure_rolls_back_record_files_attempt_and_history()
 {
  await using var f=await Fixture.Create();var package=await f.Package();await using(var db=f.Db(new FailOutbox()))await Assert.ThrowsAsync<InvalidOperationException>(()=>new SqlRecordIngestion(db,new FixtureMachine(f),TimeProvider.System).AcceptAsync(package,"SYNTHETIC-rollback",true,default));
  await using var check=f.Db();Assert.False(await check.Set<DisturbanceRecord>().AnyAsync());Assert.False(await check.Set<RecordFile>().AnyAsync());Assert.False(await check.Set<IngestionAttempt>().AnyAsync());Assert.False(await check.Set<ProcessingOutbox>().AnyAsync());
 }
 [CenterFoundationFact] public async Task Scope_filter_precedes_page_and_pending_machine_and_human_authorities_deny()
 {
  await using var f=await Fixture.Create();var package=await f.Package();await using var db=f.Db();
  await Assert.ThrowsAsync<OwnerDecisionRequiredException>(()=>new SqlRecordIngestion(db,new PendingMachineRecordAuthority(),TimeProvider.System).AcceptAsync(package,"SYNTHETIC-deny",true,default));
  await Assert.ThrowsAsync<OwnerDecisionRequiredException>(()=>new SqlStationRecords(db,new PendingStationDataAuthority()).ListAsync(10,default));
  await new SqlRecordIngestion(db,new FixtureMachine(f),TimeProvider.System).AcceptAsync(package,"SYNTHETIC-allow-fixture",true,default);
  Assert.Empty(await new SqlStationRecords(db,new FixtureScope(f.OtherStation)).ListAsync(1,default));Assert.Single(await new SqlStationRecords(db,new FixtureScope(f.Station)).ListAsync(1,default));
 }
 [CenterFoundationFact] public async Task Checksum_tamper_is_rejected_before_sql_mutation()
 {
  await using var f=await Fixture.Create();var package=await f.Package();await File.AppendAllTextAsync(Path.Combine(f.Root,"records",package.Manifest.Id,"test.dat"),"tamper");
  await Assert.ThrowsAsync<InvalidDataException>(()=>VerifiedRecordPackage.VerifyAsync(f.Root,package.Manifest,default));await using var db=f.Db();Assert.False(await db.Set<DisturbanceRecord>().AnyAsync());
 }
 [CenterProfileFact] public async Task Synthetic_5000_record_scoped_query_profile_and_actual_plan()
 {
  await using var f=await Fixture.Create();await using var db=f.Db();var now=DateTimeOffset.UtcNow;
  for(var i=0;i<5000;i++)db.Add(new DisturbanceRecord{StationId=f.Station,IedId=f.Ied,CollectorId=f.Collector,ManifestId=Fdc.Core.Identity.Hash("SYNTHETIC-PROFILE-"+i),ManifestSha256=Fdc.Core.Identity.Hash("manifest-"+i),RemoteIdentity="SYNTHETIC-METADATA-"+i,CollectedAtUtc=now,EventAtUtc=now.AddSeconds(i),TimeSource="SYNTHETIC",CorrelationId="SYNTHETIC-PROFILE",IsSynthetic=true});
  await db.SaveChangesAsync();db.ChangeTracker.Clear();var service=new SqlStationRecords(db,new FixtureScope(f.Station));for(var i=0;i<5;i++)Assert.Equal(50,(await service.ListAsync(50,default)).Count);
  var samples=new List<double>();using var process=System.Diagnostics.Process.GetCurrentProcess();var cpu=process.TotalProcessorTime;var memory=GC.GetTotalMemory(false);var all=System.Diagnostics.Stopwatch.StartNew();
  for(var i=0;i<50;i++){var sample=System.Diagnostics.Stopwatch.StartNew();Assert.Equal(50,(await service.ListAsync(50,default)).Count);sample.Stop();samples.Add(sample.Elapsed.TotalMilliseconds);}all.Stop();samples.Sort();
  double Percentile(double p)=>samples[(int)Math.Ceiling(p*samples.Count)-1];
  var output=Environment.GetEnvironmentVariable("FDC_CENTER_EVIDENCE")??throw new InvalidOperationException("Explicit local evidence directory required for profile.");Directory.CreateDirectory(output);
  var query=db.Set<DisturbanceRecord>().Where(r=>r.StationId==f.Station).OrderByDescending(r=>r.EventAtUtc).ThenBy(r=>r.Id).Take(50).Select(r=>r.Id).ToQueryString();await db.Database.OpenConnectionAsync();
  await using(var command=db.Database.GetDbConnection().CreateCommand())
  {command.CommandText="SET STATISTICS XML ON;\n"+query+"\nSET STATISTICS XML OFF;";await using var reader=await command.ExecuteReaderAsync();do{while(await reader.ReadAsync())if(reader.FieldCount==1&&reader.GetValue(0) is string plan&&plan.Contains("ShowPlanXML",StringComparison.Ordinal))await File.WriteAllTextAsync(Path.Combine(output,"scoped-query-plan.sqlplan"),plan);}while(await reader.NextResultAsync());}
  Assert.True(File.Exists(Path.Combine(output,"scoped-query-plan.sqlplan")));
  await File.WriteAllTextAsync(Path.Combine(output,"query-profile.json"),System.Text.Json.JsonSerializer.Serialize(new{Dataset="SYNTHETIC 2 stations,1 assigned IED,5000 metadata-only records,0 waveform ingestion in this profile",Warmup=5,Samples=samples.Count,P50Ms=Percentile(.50),P95Ms=Percentile(.95),P99Ms=Percentile(.99),QueriesPerSecond=50/all.Elapsed.TotalSeconds,DotNetCpuMilliseconds=(process.TotalProcessorTime-cpu).TotalMilliseconds,DotNetManagedMemoryBefore=memory,DotNetManagedMemoryAfter=GC.GetTotalMemory(false),UnexpectedErrors=0,Limits="One local process/SQL instance,not380-Collector benchmark; SQL host CPU/disk/network not collected; no SLA threshold approved",SortedSamplesMs=samples},new System.Text.Json.JsonSerializerOptions{WriteIndented=true}));
 }
 [CenterFoundationFact] public async Task Actual_core_soe_contract_replay_is_idempotent_and_conflicting_replay_denied()
 {
  await using var f=await Fixture.Create();await using var db=f.Db();var service=new SqlSoeIngestion(db,new FixtureMachine(f));var e=new SoeEvent("SYNTHETIC-SOE",f.StationCode,"IED-SYNTHETIC","SYNTHETIC/RCB","SYNTHETIC/PTRC.Tr","true","SYNTHETIC",DateTimeOffset.UtcNow,null,"Collector",null,null,1,null,1,1,false);
  Assert.Equal(1,await service.AcceptAsync([e],true,default));Assert.Equal(0,await service.AcceptAsync([e],true,default));await Assert.ThrowsAsync<InvalidDataException>(()=>service.AcceptAsync([e with{Value="conflicting"}],true,default));Assert.Equal(1,await db.Set<StationSoeEvent>().CountAsync());
 }
 [CenterFoundationFact] public async Task Fault_record_file_chain_is_station_bound_and_unimplemented_analysis_has_no_fabricated_result()
 {
  await using var f=await Fixture.Create();var package=await f.Package();await using var db=f.Db();var receipt=await new SqlRecordIngestion(db,new FixtureMachine(f),TimeProvider.System).AcceptAsync(package,"SYNTHETIC-fault",true,default);
  var fault=new FaultEvent{StationId=f.Station,EventAtUtc=DateTimeOffset.UtcNow,TimeSource="SYNTHETIC",IsSynthetic=true};var other=new FaultEvent{StationId=f.OtherStation,EventAtUtc=DateTimeOffset.UtcNow,TimeSource="SYNTHETIC",IsSynthetic=true};
  db.AddRange(fault,other,new FaultEventRecord{StationId=f.Station,FaultEventId=fault.Id,RecordId=receipt.RecordId,IsSynthetic=true},new AnalysisAlgorithm{Id=f.Algorithm,Code="SYNTHETIC-NOT-IMPLEMENTED",Version="0",Implemented=false,IsSynthetic=true});await db.SaveChangesAsync();
  var job=new AnalysisJob{StationId=f.Station,FaultEventId=fault.Id,AlgorithmId=f.Algorithm,CorrelationId="SYNTHETIC",IsSynthetic=true};db.Add(job);await db.SaveChangesAsync();var result=new AnalysisResult{JobId=job.Id,Status="NotImplemented",ResultJson=null,IsSynthetic=true};db.Add(result);await db.SaveChangesAsync();
  await using(var bad=f.Db()){bad.Add(new FaultEventRecord{StationId=f.OtherStation,FaultEventId=other.Id,RecordId=receipt.RecordId,IsSynthetic=true});await Assert.ThrowsAsync<DbUpdateException>(()=>bad.SaveChangesAsync());}
  result.ResultJson="{\"fake\":true}";await Assert.ThrowsAsync<DbUpdateException>(()=>db.SaveChangesAsync());
 }
 private sealed class FixtureMachine(Fixture f):IMachineRecordAuthority { public Task<MachineRecordBinding> DemandAsync(string station,string ied,CancellationToken ct){if(station!=f.StationCode||ied!="IED-SYNTHETIC")throw new UnauthorizedAccessException();return Task.FromResult(new MachineRecordBinding(f.Station,f.Ied,f.Collector));} }
 private sealed class FixtureScope(Guid station):IStationDataAuthority { public Task<IReadOnlySet<Guid>> DemandReadAsync(CancellationToken ct)=>Task.FromResult<IReadOnlySet<Guid>>(new HashSet<Guid>{station}); }
 private sealed class FailOutbox:SaveChangesInterceptor {public override ValueTask<InterceptionResult<int>> SavingChangesAsync(DbContextEventData e,InterceptionResult<int> r,CancellationToken ct=default){if(e.Context!.ChangeTracker.Entries<ProcessingOutbox>().Any(x=>x.State==EntityState.Added))throw new InvalidOperationException("SYNTHETIC required outbox failure");return base.SavingChangesAsync(e,r,ct);} }
 private sealed class Fixture:IAsyncDisposable
 {
  private A9DisposableSql? disposable;
  public string Connection=Environment.GetEnvironmentVariable("FDC_CENTER_LAB_CONNECTION")!;public Guid Algorithm=Guid.NewGuid(),Corporation=Guid.NewGuid(),Unit=Guid.NewGuid(),Station=Guid.NewGuid(),OtherStation=Guid.NewGuid(),Ied=Guid.NewGuid(),Collector=Guid.NewGuid();public string StationCode="SYNTHETIC-"+Guid.NewGuid().ToString("N");public string Root=Path.Combine(Path.GetTempPath(),"FdcCenter-SYNTHETIC-"+Guid.NewGuid().ToString("N"));
  public CenterDbContext Db(IInterceptor? interceptor=null){var b=disposable?.Options<CenterDbContext>() ?? new DbContextOptionsBuilder<CenterDbContext>().UseSqlServer(Connection);if(interceptor!=null)b.AddInterceptors(interceptor);return new(b.Options);}
  public static async Task<Fixture> Create()
  {
   var f=new Fixture();
   if(A9DisposableSql.Enabled)f.disposable=await A9DisposableSql.CreateAsync();
   try
   {
   if(f.disposable is null)
   {
   var settings=new Microsoft.Data.SqlClient.SqlConnectionStringBuilder(f.Connection);if(settings.DataSource!="lpc:."||settings.InitialCatalog!="FDC_Center_Lab")throw new InvalidOperationException("Unexpected lab.");
   await using var guardDb=f.Db();Assert.Equal(1,await guardDb.Database.SqlQueryRaw<int>("SELECT CASE WHEN CONVERT(nvarchar(128),SERVERPROPERTY('MachineName'))=N'XOTH-ETC-2' AND EXISTS(SELECT 1 FROM sys.extended_properties WHERE class=0 AND name=N'Fdc.Disposable' AND TRY_CONVERT(bit,value)=1) THEN 1 ELSE 0 END AS [Value]").SingleAsync());
   }
   await using var db=f.Db();
   await using(var iam=new IamDbContext((f.disposable?.Options<IamDbContext>() ?? new DbContextOptionsBuilder<IamDbContext>().UseSqlServer(f.Connection)).Options))
   {Assert.False(await iam.Set<UserRow>().AnyAsync());var now=DateTimeOffset.UtcNow;iam.AddRange(new CorporationRow{Id=f.Corporation,Code=f.StationCode,CreatedAt=now},new ManagementUnitRow{Id=f.Unit,CorporationId=f.Corporation,Code=f.StationCode,CreatedAt=now},new StationRow{Id=f.Station,ManagementUnitId=f.Unit,Code=f.StationCode,HierarchyVersion=1,CreatedAt=now},new StationRow{Id=f.OtherStation,ManagementUnitId=f.Unit,Code=f.StationCode+"-B",HierarchyVersion=1,CreatedAt=now});await iam.SaveChangesAsync();}
   db.AddRange(new IedDevice{Id=f.Ied,StationId=f.Station,Code="IED-SYNTHETIC",DisplayName="SYNTHETIC protection IED",IsSynthetic=true},new CollectorRegistration{Id=f.Collector,StationId=f.Station,Code=f.StationCode,Active=true,BindingStatus="Bound",IsSynthetic=true},new CollectorIed{CollectorId=f.Collector,IedId=f.Ied,StationId=f.Station,Active=true,IsSynthetic=true});await db.SaveChangesAsync();return f;
   }
   catch { if(f.disposable is not null)await f.disposable.DisposeAsync(); throw; }
  }
  public async Task<VerifiedRecordPackage> Package()
  {
   var files=new Dictionary<string,string>{{"test.cfg","SYNTHETIC,IED,1999\n1,1A,0D\n1,Ia,A,,A,1,0,0,-32768,32767,1,1,P\n50\n1\n1000,2\n01/01/2026,00:00:00.000000\n01/01/2026,00:00:00.000000\nASCII\n1\n"},{"test.dat","1,0,0\n2,1000,1\n"}};
   var digests=files.Select(k=>new FileDigest(k.Key,System.Text.Encoding.UTF8.GetByteCount(k.Value),Fdc.Core.Identity.Hash(k.Value))).ToArray();var id=Fdc.Core.Identity.Hash(StationCode+"\nIED-SYNTHETIC\nSYNTHETIC-source\n"+string.Join('\n',digests.Select(d=>d.Name+":"+d.Sha256)));var dir=Path.Combine(Root,"records",id);Directory.CreateDirectory(dir);foreach(var p in files)await File.WriteAllTextAsync(Path.Combine(dir,p.Key),p.Value);return await VerifiedRecordPackage.VerifyAsync(Root,new Manifest(id,StationCode,"IED-SYNTHETIC","","","","","SYNTHETIC-source",DateTimeOffset.UtcNow,null,"SYNTHETIC",digests),default);
  }
  public async ValueTask DisposeAsync()
  {
   if(disposable is not null)
   {
    try { await disposable.DisposeAsync(); }
    finally { DeleteFixtureFiles(); }
    return;
   }
   await using var db=Db();await using var tx=await db.Database.BeginTransactionAsync();
#pragma warning disable EF1003 // Source-only fixed table allowlist; both station IDs remain SQL parameters.
   await db.Database.ExecuteSqlInterpolatedAsync($"DELETE FROM ops.AnalysisResult WHERE JobId IN (SELECT Id FROM ops.AnalysisJob WHERE StationId={Station} OR StationId={OtherStation})");
   foreach(var table in new[]{"ops.AnalysisJob","ops.FaultEventRecord","ops.FaultEvent","ops.StationSoeEvent","ops.ProcessingOutbox","ingest.RecordStatusHistory","ingest.IngestionAttempt","ingest.RecordFile","ingest.DisturbanceRecord","ops.CollectorHeartbeat","ops.CollectorIed","catalog.IedDevice","ops.CollectorRegistration"})
    await db.Database.ExecuteSqlRawAsync("DELETE FROM "+table+" WHERE StationId IN ({0},{1})",Station,OtherStation);
#pragma warning restore EF1003
   await db.Database.ExecuteSqlInterpolatedAsync($"DELETE FROM ops.AnalysisAlgorithm WHERE Id={Algorithm}; DELETE FROM catalog.Station WHERE Id={Station} OR Id={OtherStation}; DELETE FROM catalog.ManagementUnit WHERE Id={Unit}; DELETE FROM catalog.Corporation WHERE Id={Corporation}");await tx.CommitAsync();
   DeleteFixtureFiles();
  }
  private void DeleteFixtureFiles()
  {
   if(Directory.Exists(Root)&&Path.GetFullPath(Root).StartsWith(Path.GetFullPath(Path.GetTempPath()),StringComparison.OrdinalIgnoreCase)&&Path.GetFileName(Root).StartsWith("FdcCenter-SYNTHETIC-",StringComparison.Ordinal))Directory.Delete(Root,true);
  }
 }
}
internal sealed class CenterFoundationFactAttribute:FactAttribute
{
 public CenterFoundationFactAttribute(){if(!A9DisposableSql.Enabled && (Environment.GetEnvironmentVariable("FDC_CENTER_CONFIRMED_LAB")!="FDC_Center_Lab"||string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("FDC_CENTER_LAB_CONNECTION"))))Skip="Explicit new Center disposable lab required.";}
}



internal sealed class CenterProfileFactAttribute:FactAttribute
{
 public CenterProfileFactAttribute(){if((!A9DisposableSql.Enabled && (Environment.GetEnvironmentVariable("FDC_CENTER_CONFIRMED_LAB")!="FDC_Center_Lab"||string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("FDC_CENTER_LAB_CONNECTION"))))||string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("FDC_CENTER_EVIDENCE")))Skip="Explicit Center lab and local profile evidence directory required.";}
}
