using Fdc.Center.Persistence;
using Fdc.Core;
using Microsoft.EntityFrameworkCore;

namespace Fdc.Center.Tests;

public sealed class PilotIntakeContractTests
{
    [Theory] [InlineData("files")] [InlineData("station")] [InlineData("id")] [InlineData("hash")] [InlineData("name")]
    public async Task Malformed_manifest_never_opens_sql_or_creates_storage(string field)
    {
        await using var db = new CenterDbContext(new DbContextOptionsBuilder<CenterDbContext>().UseSqlServer("Server=unconfigured.invalid;Database=NeverConnect;Integrated Security=true").Options);
        var root = Path.Combine(Path.GetTempPath(), "FdcPilot-NO-CREATE-" + Guid.NewGuid().ToString("N"));
        var limits = new PilotIntakeLimits(1048576, 4194304, 1024, 10, TimeSpan.FromMinutes(1), new(1048576, 1048576, 32, 32, 1000, 20), "UTF-8", 12);
        var authority = new NeverAuthority();
        var service = new PilotFileIntake(root, limits, authority, new SqlRecordIngestion(db, authority, TimeProvider.System), TimeProvider.System, true);
        var m = new Manifest(new string('a', 64), "TEST", "IED", "", "", "", "", "/file.cfg", DateTimeOffset.UtcNow, null, "SYNTHETIC", [new("file.cfg", 1, new string('a', 64)), new("file.dat", 1, new string('a', 64))]);
        m = field switch
        {
            "files" => m with { Files = null! }, "station" => m with { StationId = null! },
            "id" => m with { Id = null! }, "hash" => m with { Files = [new("file.cfg", 1, null!), new("file.dat", 1, new string('a', 64))] },
            "name" => m with { Files = [new("../file.cfg", 1, new string('a', 64)), new("file.dat", 1, new string('a', 64))] },
            _ => throw new InvalidOperationException()
        };
        await Assert.ThrowsAsync<InvalidDataException>(() => service.BeginAsync(m, "SYNTHETIC", default));
        Assert.False(Directory.Exists(root)); Assert.Equal(0, authority.Calls);
    }
    private sealed class NeverAuthority : IMachineRecordAuthority
    {
        public int Calls;
        public Task<MachineRecordBinding> DemandAsync(string stationCode, string iedCode, CancellationToken ct) { Calls++; throw new InvalidOperationException("Validation must reject before SQL/network/storage."); }
    }
}
