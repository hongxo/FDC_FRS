using Fdc.Core;

namespace Fdc.Center;
public sealed class CenterOptions
{
    public string Mode { get; set; } = "Demo";
    public string DataRoot { get; set; } = "data/center";
    public string Inbox { get; set; } = "data/incoming";
    public string AdminApiKey { get; set; } = "";
    public Dictionary<string, string> StationKeys { get; set; } = [];
    public MtlsServerOptions Mtls { get; set; } = new();
    public bool AllowHttpsBundles { get; set; }
}
public sealed class MtlsServerOptions
{
    public bool Enabled { get; set; }
    public string ServerCertificatePath { get; set; } = "";
    public string ServerCertificatePassword { get; set; } = "";
    public string TrustedCaPath { get; set; } = "";
    public bool CheckRevocation { get; set; } = true;
    public string AdminCertificateSha256 { get; set; } = "";
    public Dictionary<string, string> StationCertificateSha256 { get; set; } = [];
}
public sealed class InboxWorker(CenterOptions options, BundleReceiver receiver, ILogger<InboxWorker> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken ct)
    {
        Directory.CreateDirectory(options.Inbox);
        while (!ct.IsCancellationRequested)
        {
            foreach (var station in options.StationKeys.Keys)
            {
                var directory = Path.Combine(options.Inbox, station);
                if (!Directory.Exists(directory)) continue;
                foreach (var ready in Directory.GetFiles(directory, "*.zip.ready"))
                {
                    try
                    {
                        var archive = ready[..^6];
                        if (!File.Exists(archive)) continue;
                        if ((await File.ReadAllTextAsync(ready, ct)).Trim() != Fdc.Core.Identity.FileHash(archive))
                            throw new InvalidDataException("Hash gói SFTP chưa khớp.");
                        await receiver.ReceiveAsync(archive, station, ct);
                        File.Delete(ready); File.Delete(archive);
                    }
                    catch (Exception ex) when (!ct.IsCancellationRequested)
                    { logger.LogWarning(ex, "Nhập kho thất bại {File}", ready); }
                }
            }
            await Task.Delay(TimeSpan.FromSeconds(3), ct);
        }
    }
}
