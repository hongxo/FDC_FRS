using System.Diagnostics;
using System.Net;
using System.Security.Cryptography;
using System.Text.Json;
using Fdc.Identity.Infrastructure;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace Fdc.Center.Tests;

public sealed partial class A10LabWorkflowTests
{
    [A10Fact]
    public async Task Worker_process_is_killed_after_staging_and_replacement_rechecks_SQL_authority()
    {
        await using var fixture = await Host.CreateAsync();
        var permit = await fixture.Create(fixture.Client("demo.operator"), Guid.NewGuid(), fixture.Draft());
        var auditor = fixture.Client("demo.auditor");
        var releasedJob = await fixture.ExportRequest(auditor, permit);
        var revokedJob = await fixture.ExportRequest(auditor, permit);
        var released = await RecoverAsync(fixture, releasedJob, false);
        var quarantined = await RecoverAsync(fixture, revokedJob, true);

        await using var iam = fixture.Iam();
        var events = await (from audit in iam.Set<SecurityEventRow>()
            join outbox in iam.Set<AuditOutboxRow>() on audit.Id equals outbox.EventId
            where audit.EventCode == "lab.export.released" || audit.EventCode == "lab.export.quarantined"
            select audit.EventCode).ToListAsync();
        Assert.Single(events, code => code == "lab.export.released");
        Assert.Single(events, code => code == "lab.export.quarantined");
        await fixture.WriteEvidence("a12-worker-process.json", new { Released = released, Quarantined = quarantined, Events = events });
    }

    private static async Task<object> RecoverAsync(Host fixture, Guid job, bool revoke)
    {
        var signal = Path.Combine(Path.GetTempPath(), "fdc-a12-" + Guid.NewGuid().ToString("N"));
        var tokenA = Guid.NewGuid();
        var tokenB = Guid.NewGuid();
        using var deadline = new CancellationTokenSource(TimeSpan.FromSeconds(75));
        using var workerA = StartProcess(job, tokenA, signal, "stage");
        var started = DateTimeOffset.UtcNow;
        try
        {
            while (!File.Exists(signal))
            {
                Assert.False(workerA.HasExited, "Worker A exited before its committed staging barrier.");
                await Task.Delay(50, deadline.Token);
            }
            var stage = JsonSerializer.Deserialize<WorkerResult>(await File.ReadAllTextAsync(signal, deadline.Token))!;
            Assert.True(stage.Prepared);
            Assert.NotEqual(Environment.ProcessId, stage.Pid);
            await using var lab = fixture.Lab();
            var before = await lab.Exports.AsNoTracking().SingleAsync(row => row.Id == job);
            Assert.Equal("Preparing", before.State);
            Assert.Equal(tokenA, before.LeaseOwner);
            Assert.NotNull(before.PayloadDigest);
            using var processA = Process.GetProcessById(stage.Pid);
            workerA.Kill(true);
            await workerA.WaitForExitAsync(deadline.Token);
            await processA.WaitForExitAsync(deadline.Token);
            var killed = DateTimeOffset.UtcNow;
            if (revoke) await fixture.RevokeActor("demo.auditor");

            using var workerB = StartProcess(job, tokenB, signal + ".replacement", "recover");
            WorkerResult recovered;
            try
            {
                await workerB.WaitForExitAsync(deadline.Token);
                Assert.Equal(0, workerB.ExitCode);
                recovered = JsonSerializer.Deserialize<WorkerResult>(await File.ReadAllTextAsync(signal + ".replacement", deadline.Token))!;
            }
            finally
            {
                if (!workerB.HasExited) { workerB.Kill(true); await workerB.WaitForExitAsync(); }
            }

            Assert.NotEqual(stage.Pid, recovered.Pid);
            Assert.Equal(!revoke, recovered.Prepared);
            Assert.Equal(!revoke, recovered.Released);
            Assert.False(recovered.Replayed);
            var after = await lab.Exports.AsNoTracking().SingleAsync(row => row.Id == job);
            Assert.Equal(revoke ? "Quarantined" : "Completed", after.State);
            Assert.Null(after.LeaseOwner);
            Assert.Equal(revoke ? null : before.PayloadDigest, after.PayloadDigest);
            if (revoke) Assert.Null(after.Payload);
            using var download = await fixture.Client("demo.auditor").GetAsync($"/api/lab/v1/tests/exports/{job}/download", deadline.Token);
            Assert.Equal(revoke ? HttpStatusCode.Unauthorized : HttpStatusCode.OK, download.StatusCode);
            var bytes = await download.Content.ReadAsByteArrayAsync(deadline.Token);
            if (revoke) Assert.Empty(bytes);
            else Assert.Equal(after.PayloadDigest, Convert.ToHexStringLower(SHA256.HashData(bytes)));
            return new { Download = (int)download.StatusCode, DownloadBytes = bytes.Length, StartedUtc = started, KilledUtc = killed, Stage = stage, Recovery = recovered,
                Before = before.State, After = after.State, before.LeaseUntilUtc, ArtifactHash = after.PayloadDigest,
                LeaseExpiry = "Deterministic worker clock advanced 61 seconds", RevokedDuringDowntime = revoke };
        }
        finally
        {
            if (!workerA.HasExited) { workerA.Kill(true); await workerA.WaitForExitAsync(); }
            File.Delete(signal);
            File.Delete(signal + ".pending");
            File.Delete(signal + ".replacement");
            File.Delete(signal + ".replacement.pending");
        }
    }

    private static Process StartProcess(Guid job, Guid token, string signal, string action)
    {
        var command = new ProcessStartInfo("dotnet") { UseShellExecute = false, CreateNoWindow = true };
        command.ArgumentList.Add("vstest");
        command.ArgumentList.Add(typeof(A10LabWorkflowTests).Assembly.Location);
        command.ArgumentList.Add("/Tests:" + nameof(A12WorkerEntryPoint));
        command.Environment["FDC_RUN_A10_SQL"] = "0";
        command.Environment["FDC_A12_WORKER"] = action;
        command.Environment["FDC_A12_DB"] = TargetDatabase;
        command.Environment["FDC_A12_JOB"] = job.ToString("D");
        command.Environment["FDC_A12_OWNER"] = token.ToString("D");
        command.Environment["FDC_A12_SIGNAL"] = signal;
        return Process.Start(command) ?? throw new InvalidOperationException("Could not start worker testhost.");
    }

    [A12WorkerFact]
    public async Task A12WorkerEntryPoint()
    {
        const string prefix = "FDC_A10_TEST_";
        var name = Environment.GetEnvironmentVariable("FDC_A12_DB") ?? "";
        var action = Environment.GetEnvironmentVariable("FDC_A12_WORKER");
        Assert.True(name.StartsWith(prefix, StringComparison.Ordinal) && Guid.TryParseExact(name[prefix.Length..], "N", out _));
        var settings = new SqlConnectionStringBuilder(Environment.GetEnvironmentVariable("FDC_A10_SQL_MASTER"));
        Assert.True(settings.DataSource == "lpc:." && settings.InitialCatalog == "master" && settings.IntegratedSecurity &&
            settings.AttachDBFilename.Length == 0 && settings.FailoverPartner.Length == 0);
        settings.InitialCatalog = name;
        settings.Pooling = false;
        await using (var sql = new SqlConnection(settings.ConnectionString))
        {
            await sql.OpenAsync();
            await using var guard = new SqlCommand("""
                SELECT COUNT(*) FROM sys.extended_properties WHERE class=0 AND name=N'Fdc.A10.Run'
                AND CONVERT(nvarchar(32),value)=@run;
                """, sql);
            guard.Parameters.AddWithValue("@run", name[prefix.Length..]);
            Assert.Equal(1, Convert.ToInt32(await guard.ExecuteScalarAsync()));
        }

        var job = Guid.Parse(Environment.GetEnvironmentVariable("FDC_A12_JOB")!);
        var owner = Guid.Parse(Environment.GetEnvironmentVariable("FDC_A12_OWNER")!);
        var signal = Environment.GetEnvironmentVariable("FDC_A12_SIGNAL")!;
        await using var worker = await Host.StartWorkerAsync(settings.ConnectionString, action == "recover");
        var prepared = await worker.Prepare(job, owner);
        var released = action == "recover" && prepared && await worker.Release(job, owner);
        var replayed = released && await worker.Release(job, owner);
        await File.WriteAllTextAsync(signal + ".pending", JsonSerializer.Serialize(
            new WorkerResult(Environment.ProcessId, DateTimeOffset.UtcNow, prepared, released, replayed)));
        File.Move(signal + ".pending", signal);
        if (action == "stage") await Task.Delay(TimeSpan.FromSeconds(90));
    }

    private sealed record WorkerResult(int Pid, DateTimeOffset Utc, bool Prepared, bool Released, bool Replayed);
}

internal sealed class A12WorkerFactAttribute : FactAttribute
{
    public A12WorkerFactAttribute()
    {
        if (Environment.GetEnvironmentVariable("FDC_A12_WORKER") is not ("stage" or "recover"))
            Skip = "Explicit disposable A.12 worker child process required.";
    }
}
