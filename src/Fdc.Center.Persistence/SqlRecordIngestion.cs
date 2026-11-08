using System.Data;
using System.Security.Cryptography;
using System.Text.Json;
using Fdc.Core;
using Fdc.Identity.Application;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
namespace Fdc.Center.Persistence;

public sealed record MachineRecordBinding(Guid StationId,Guid IedId,Guid CollectorId);
public interface IMachineRecordAuthority { Task<MachineRecordBinding> DemandAsync(string stationCode,string iedCode,CancellationToken ct); }
public sealed class PendingMachineRecordAuthority : IMachineRecordAuthority
{
 public Task<MachineRecordBinding> DemandAsync(string stationCode,string iedCode,CancellationToken ct)=>throw new OwnerDecisionRequiredException("D07/E04: verified Collector certificate/station/IED binding required");
}
public sealed class VerifiedRecordPackage
{
 public Manifest Manifest { get; } public string ManifestSha256 { get; } public IReadOnlyDictionary<string,string> ObjectKeys { get; }
 private VerifiedRecordPackage(Manifest manifest,Dictionary<string,string> keys){Manifest=manifest with{Files=manifest.Files.ToArray()};ManifestSha256=Fdc.Core.Identity.Hash(JsonSerializer.Serialize(Manifest));ObjectKeys=keys.AsReadOnly();}
 public static async Task<VerifiedRecordPackage> VerifyAsync(string storageRoot,Manifest input,CancellationToken ct)
 {
  if(!Path.IsPathFullyQualified(storageRoot)||!Fdc.Core.Identity.SafeId(input.StationId)||!Fdc.Core.Identity.SafeId(input.IedId)||input.Files.Count is <2 or >4||input.Id.Length!=64||input.Id.Any(c=>!char.IsAsciiHexDigit(c)||char.IsUpper(c)))throw new InvalidDataException("Invalid package identity.");
  var manifest=input with{Files=input.Files.ToArray()};var keys=new Dictionary<string,string>(StringComparer.OrdinalIgnoreCase);var root=Path.GetFullPath(storageRoot);var directory=Path.Combine(root,"records",manifest.Id);
  foreach(var folder in new[]{root,Path.Combine(root,"records"),directory})if((File.GetAttributes(folder)&FileAttributes.ReparsePoint)!=0)throw new InvalidDataException("Reparse storage paths forbidden.");
  foreach(var file in manifest.Files)
  {
   var ext=Path.GetExtension(file.Name).ToLowerInvariant();
   if(file.Name!=Fdc.Core.Identity.FileName(file.Name)||ext is not(".cfg" or ".dat" or ".hdr" or ".inf")||file.Size<0||file.Size>100*1024*1024||file.Sha256.Length!=64||file.Sha256.Any(c=>!char.IsAsciiHexDigit(c)||char.IsUpper(c))||!keys.TryAdd(file.Name,"records/"+manifest.Id+"/"+file.Name))throw new InvalidDataException("Invalid file manifest.");
   var path=Path.Combine(directory,file.Name);if((File.GetAttributes(path)&FileAttributes.ReparsePoint)!=0)throw new InvalidDataException("Reparse files forbidden.");
   await using var stream=new FileStream(path,FileMode.Open,FileAccess.Read,FileShare.Read,65536,true);
   if(stream.Length!=file.Size||Convert.ToHexStringLower(await SHA256.HashDataAsync(stream,ct))!=file.Sha256)throw new InvalidDataException("File checksum mismatch.");
  }
  var cfg=manifest.Files.Single(f=>Path.GetExtension(f.Name).Equals(".cfg",StringComparison.OrdinalIgnoreCase));var dat=manifest.Files.Single(f=>Path.GetExtension(f.Name).Equals(".dat",StringComparison.OrdinalIgnoreCase));
  Comtrade.Validate(Path.Combine(directory,cfg.Name),Path.Combine(directory,dat.Name));
  var id=Fdc.Core.Identity.Hash(manifest.StationId+"\n"+manifest.IedId+"\n"+manifest.RemoteIdentity+"\n"+string.Join('\n',manifest.Files.Select(f=>f.Name+":"+f.Sha256)));
  if(id!=manifest.Id)throw new InvalidDataException("Manifest content identity mismatch.");
  return new(manifest,keys);
 }
}
public sealed record IngestionReceipt(Guid RecordId,bool Duplicate);
// Candidate metadata writer. Not mapped to Machine HTTP until PKI/protocol decisions are approved.
public sealed class SqlRecordIngestion(CenterDbContext db,IMachineRecordAuthority authority,TimeProvider clock)
{
 public async Task<IngestionReceipt> AcceptAsync(VerifiedRecordPackage package,string correlationId,bool synthetic,CancellationToken ct)
 {
  if(string.IsNullOrWhiteSpace(correlationId)||correlationId.Length>200)throw new ArgumentException("Bounded correlation required.");
  var m=package.Manifest;var binding=await authority.DemandAsync(m.StationId,m.IedId,ct);
  if(db.Database.CurrentTransaction!=null||db.ChangeTracker.HasChanges())throw new InvalidOperationException("Ingestion owns a clean transaction.");
  await using var tx=await db.Database.BeginTransactionAsync(IsolationLevel.Serializable,ct);var committing=false;
  try
  {
   await using(var command=db.Database.GetDbConnection().CreateCommand())
   {
    command.Transaction=tx.GetDbTransaction();command.CommandText="DECLARE @r int; EXEC @r=sys.sp_getapplock @Resource=@resource,@LockMode=N'Exclusive',@LockOwner=N'Transaction',@LockTimeout=5000; SELECT @r;";
    var p=command.CreateParameter();p.ParameterName="@resource";p.Value="Fdc.Center.Ingest.v1/"+binding.StationId.ToString("N")+"/"+binding.IedId.ToString("N")+"/"+m.Id;command.Parameters.Add(p);
    if(Convert.ToInt32(await command.ExecuteScalarAsync(ct))<0)throw new AuthorityUnavailableException();
   }
   var station=await db.Set<StationReference>().AnyAsync(s=>s.Id==binding.StationId,ct);
   var valid=await(from a in db.Set<CollectorIed>() join c in db.Set<CollectorRegistration>() on a.CollectorId equals c.Id join i in db.Set<IedDevice>() on a.IedId equals i.Id
     where a.CollectorId==binding.CollectorId&&a.IedId==binding.IedId&&a.StationId==binding.StationId&&a.Active&&c.Active&&c.BindingStatus=="Bound"&&i.Active&&i.Code==m.IedId select a.Id).AnyAsync(ct);
   var stationCode=await db.Database.SqlQuery<string>($"SELECT [Code] AS [Value] FROM [catalog].[Station] WHERE [Id]={binding.StationId}").SingleOrDefaultAsync(ct);
   if(!station||!valid||stationCode!=m.StationId)throw new UnauthorizedAccessException();
   var row=await db.Set<DisturbanceRecord>().SingleOrDefaultAsync(r=>r.StationId==binding.StationId&&r.IedId==binding.IedId&&r.ManifestId==m.Id,ct);var duplicate=row!=null;var now=clock.GetUtcNow();
   if(row!=null&&row.ManifestSha256!=package.ManifestSha256)throw new InvalidDataException("Conflicting replay manifest.");
   if(row==null)
   {
    row=new(){StationId=binding.StationId,IedId=binding.IedId,CollectorId=binding.CollectorId,ManifestId=m.Id,ManifestSha256=package.ManifestSha256,RemoteIdentity=m.RemoteIdentity,CollectedAtUtc=m.CollectedAt.ToUniversalTime(),SourceTimestampRaw=m.TriggerTimeRaw,TimeSource=m.TimeSource,State="Validated",CorrelationId=correlationId,IsSynthetic=synthetic,CreatedAtUtc=now};db.Add(row);
    foreach(var f in m.Files)db.Add(new RecordFile{StationId=binding.StationId,RecordId=row.Id,Name=f.Name,Extension=Path.GetExtension(f.Name).ToLowerInvariant(),SizeBytes=f.Size,Sha256=f.Sha256,StorageObjectKey=package.ObjectKeys[f.Name],ValidationStatus="Verified",IsSynthetic=synthetic,CreatedAtUtc=now});
    db.Add(new ProcessingOutbox{StationId=binding.StationId,RecordId=row.Id,Kind="RecordValidated",AvailableAtUtc=now,IsSynthetic=synthetic,CreatedAtUtc=now});db.Add(new RecordStatusHistory{StationId=binding.StationId,RecordId=row.Id,State="Validated",CorrelationId=correlationId,IsSynthetic=synthetic,CreatedAtUtc=now});
   }
   var number=(await db.Set<IngestionAttempt>().Where(a=>a.RecordId==row.Id).Select(a=>(int?)a.Number).MaxAsync(ct)??0)+1;
   db.Add(new IngestionAttempt{StationId=binding.StationId,RecordId=row.Id,Number=number,Outcome=duplicate?"Duplicate":"Accepted",CorrelationId=correlationId,IsSynthetic=synthetic,CreatedAtUtc=now});await db.SaveChangesAsync(ct);committing=true;await tx.CommitAsync(ct);return new(row.Id,duplicate);
  }
  catch
  {
   var rollbackFailed=false;try{await tx.RollbackAsync(CancellationToken.None);}catch{rollbackFailed=true;}db.ChangeTracker.Clear();if(committing)throw new CommitOutcomeUnknownException();if(rollbackFailed)throw new AuthorityUnavailableException();throw;
  }
 }
}
public interface IStationDataAuthority { Task<IReadOnlySet<Guid>> DemandReadAsync(CancellationToken ct); }
public sealed class PendingStationDataAuthority:IStationDataAuthority { public Task<IReadOnlySet<Guid>> DemandReadAsync(CancellationToken ct)=>throw new OwnerDecisionRequiredException("D04: current SQL StationGrant operation mapping required"); }
public sealed class SqlStationRecords(CenterDbContext db,IStationDataAuthority authority)
{
 public async Task<IReadOnlyList<Guid>> ListAsync(int limit,CancellationToken ct)
 {
  if(limit is <1 or >100)throw new ArgumentException("Bounded page required.");var allowed=(await authority.DemandReadAsync(ct)).ToArray();return await db.Set<DisturbanceRecord>().AsNoTracking().Where(r=>allowed.Contains(r.StationId)).OrderByDescending(r=>r.EventAtUtc).ThenBy(r=>r.Id).Take(limit).Select(r=>r.Id).ToListAsync(ct);
 }
}
