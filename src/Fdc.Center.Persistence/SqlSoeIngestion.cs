using System.Data;
using System.Reflection;
using Fdc.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
namespace Fdc.Center.Persistence;
// Preserves the existing Core SOE contract without modifying edge SQLite or exposing an unapproved machine API.
public sealed class SqlSoeIngestion(CenterDbContext db,IMachineRecordAuthority authority)
{
 public async Task<int> AcceptAsync(IReadOnlyList<SoeEvent> events,bool synthetic,CancellationToken ct)
 {
  if(events.Count is <1 or >500)throw new ArgumentException("Bounded SOE batch required.");var first=events[0];
  if(events.Any(e=>e.StationId!=first.StationId||e.IedId!=first.IedId)||events.Select(e=>e.Id).Distinct(StringComparer.Ordinal).Count()!=events.Count)throw new ArgumentException("One bound station/IED and unique source IDs required.");
  var binding=await authority.DemandAsync(first.StationId,first.IedId,ct);
  if(db.Database.CurrentTransaction!=null||db.ChangeTracker.HasChanges())throw new InvalidOperationException("SOE writer owns a clean transaction.");
  await using var tx=await db.Database.BeginTransactionAsync(IsolationLevel.Serializable,ct);var committing=false;
  try
  {
   await using(var lockCommand=db.Database.GetDbConnection().CreateCommand())
   {lockCommand.Transaction=tx.GetDbTransaction();lockCommand.CommandText="DECLARE @r int;EXEC @r=sys.sp_getapplock @Resource=@resource,@LockMode=N'Exclusive',@LockOwner=N'Transaction',@LockTimeout=5000;SELECT @r;";var p=lockCommand.CreateParameter();p.ParameterName="@resource";p.Value="Fdc.Center.Soe.v1/"+binding.StationId.ToString("N")+"/"+binding.IedId.ToString("N");lockCommand.Parameters.Add(p);if(Convert.ToInt32(await lockCommand.ExecuteScalarAsync(ct))<0)throw new Fdc.Identity.Application.AuthorityUnavailableException();}
   var valid=await(from a in db.Set<CollectorIed>() join c in db.Set<CollectorRegistration>() on a.CollectorId equals c.Id join i in db.Set<IedDevice>() on a.IedId equals i.Id where a.StationId==binding.StationId&&a.IedId==binding.IedId&&a.CollectorId==binding.CollectorId&&a.Active&&c.Active&&c.BindingStatus=="Bound"&&i.Active&&i.Code==first.IedId select a.Id).AnyAsync(ct);
   var stationCode=await db.Database.SqlQuery<string>($"SELECT Code AS [Value] FROM catalog.Station WHERE Id={binding.StationId}").SingleOrDefaultAsync(ct);if(!valid||stationCode!=first.StationId)throw new UnauthorizedAccessException();
   var added=0;
   foreach(var e in events)
   {
    var row=new StationSoeEvent{StationId=binding.StationId,IedId=binding.IedId,SourceEventId=e.Id,RcbReference=e.RcbReference,DataReference=e.DataReference,Value=e.Value,Source=e.Source,ReceivedAtUtc=e.ReceivedAt.ToUniversalTime(),IedTimestampUtc=e.IedTimestamp?.ToUniversalTime(),TimeSource=e.TimeSource,TimeQuality=e.TimeQuality,Quality=e.Quality,Reason=e.Reason,EntryId=e.EntryId,Sequence=e.Sequence,ConfRev=e.ConfRev,BufferOverflow=e.BufferOverflow,SubSequence=e.SubSequence,MoreSegments=e.MoreSegments,IsSynthetic=synthetic};
    var old=await db.Set<StationSoeEvent>().SingleOrDefaultAsync(x=>x.StationId==binding.StationId&&x.IedId==binding.IedId&&x.SourceEventId==e.Id,ct);
    if(old==null){db.Add(row);added++;}
    else if(typeof(StationSoeEvent).GetProperties(BindingFlags.DeclaredOnly|BindingFlags.Public|BindingFlags.Instance).Any(p=>!Equals(p.GetValue(old),p.GetValue(row))))throw new InvalidDataException("Conflicting SOE source replay.");
   }
   await db.SaveChangesAsync(ct);committing=true;await tx.CommitAsync(ct);return added;
  }
  catch
  {var rollbackFailed=false;try{await tx.RollbackAsync(CancellationToken.None);}catch{rollbackFailed=true;}db.ChangeTracker.Clear();if(committing)throw new Fdc.Identity.Application.CommitOutcomeUnknownException();if(rollbackFailed)throw new Fdc.Identity.Application.AuthorityUnavailableException();throw;}
 }
}
