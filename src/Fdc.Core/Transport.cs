using System.Net.Http.Json;
using Renci.SshNet;
using System.Security.Cryptography;

namespace Fdc.Core;

public sealed class Transport(CollectorOptions options, HttpClient http)
{
    public async Task SendAsync(RecordRow row, CancellationToken ct)
    {
        if (options.Mode == "Demo" || options.TransferProtocol == "Https")
        {
            await using var input = File.OpenRead(row.BundlePath);
            using var content = new StreamContent(input);
            using var response = await http.PostAsync($"api/stations/{options.StationId}/bundles", content, ct);
            response.EnsureSuccessStatusCode();
        }
        else
        {
            if (!options.Sftp.Enabled) throw new InvalidOperationException("SFTP chưa bật.");
            await Task.Run(() => Upload(row, ct), ct);
        }
        // A successful SFTP upload alone is not an acknowledgement of validated ingestion.
        var receipt = await http.GetFromJsonAsync<Receipt>($"api/stations/{options.StationId}/receipts/{row.Id}", ct);
        if (receipt == null || receipt.State != "Accepted" || receipt.ManifestSha256 != Identity.Hash(row.ManifestJson))
            throw new IOException("Trung tâm chưa xác nhận đúng manifest hash.");
    }
    private void Upload(RecordRow row, CancellationToken ct)
    {
        var config = options.Sftp;
        if (string.IsNullOrWhiteSpace(config.HostKeySha256)) throw new InvalidOperationException("Phải cấu hình SHA256 host key của SFTP server.");
        using var key = new PrivateKeyFile(config.PrivateKeyPath);
        using var client = new SftpClient(config.Host, config.Port, config.Username, key);
        client.ConnectionInfo.Timeout = TimeSpan.FromSeconds(15); client.OperationTimeout = TimeSpan.FromSeconds(30);
        client.HostKeyReceived += (_, e) =>
        {
            var actual = "SHA256:" + Convert.ToBase64String(SHA256.HashData(e.HostKey)).TrimEnd('=');
            e.CanTrust = CryptographicOperations.FixedTimeEquals(System.Text.Encoding.ASCII.GetBytes(actual), System.Text.Encoding.ASCII.GetBytes(config.HostKeySha256));
        };
        client.Connect();
        var directory = config.RemoteRoot.TrimEnd('/') + "/" + options.StationId;
        if (!client.Exists(directory)) client.CreateDirectory(directory);
        var target = directory + "/" + row.Id + ".zip";
        using var input = File.OpenRead(row.BundlePath);
        client.UploadFile(input, target + ".part", true, _ => ct.ThrowIfCancellationRequested());
        if (client.Exists(target)) client.DeleteFile(target); // Central inbox only; never IED files.
        client.RenameFile(target + ".part", target);
        using var ready = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(Identity.FileHash(row.BundlePath)));
        client.UploadFile(ready, target + ".ready", true);
    }
}
