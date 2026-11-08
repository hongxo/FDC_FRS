using System.Net;
using System.Security.Cryptography;
using System.Text;
using Fdc.Core;
using System.Security.Cryptography.X509Certificates;
using Microsoft.AspNetCore.Server.Kestrel.Https;

using CoreIdentity = Fdc.Core.Identity;

namespace Fdc.Center;

public static class CenterApplication
{
    public static WebApplication Build(WebApplicationOptions startup, Action<WebApplicationBuilder>? configure = null)
    {
        var builder = WebApplication.CreateBuilder(startup);
        builder.Configuration.AddJsonFile(Environment.GetEnvironmentVariable("FDC_CONFIG") ?? "appsettings.Local.json", true).AddEnvironmentVariables();
        configure?.Invoke(builder);
        if (builder.Configuration.GetValue<bool>("IdentityRuntime:Enabled")) return CenterIdentityRuntime.Build(builder);
        var options = builder.Configuration.GetSection("Center").Get<CenterOptions>() ?? new();
        options.DataRoot = Path.GetFullPath(options.DataRoot, builder.Environment.ContentRootPath);
        options.Inbox = Path.GetFullPath(options.Inbox, builder.Environment.ContentRootPath);
        if (options.Mode is not ("Demo" or "Live") || options.Mode == "Live" &&
            (options.AdminApiKey.Length < 32 || options.StationKeys.Count == 0 || options.StationKeys.Any(x => !CoreIdentity.SafeId(x.Key) || x.Value.Length < 32)))
            throw new InvalidOperationException("Live yêu cầu khóa quản trị và khóa riêng cho từng trạm, tối thiểu 32 ký tự.");
        if (options.Mode == "Live" && (!options.Mtls.Enabled || options.Mtls.AdminCertificateSha256.Length != 64
            || options.StationKeys.Keys.Any(x => !options.Mtls.StationCertificateSha256.ContainsKey(x))))
            throw new InvalidOperationException("Live yêu cầu mTLS và fingerprint chứng thư quản trị/từng trạm.");
        if (options.Mode == "Live" && (builder.Configuration["Urls"] ?? "").Split(';').Any(x => !x.StartsWith("https://", StringComparison.OrdinalIgnoreCase)))
            throw new InvalidOperationException("FRS Live chỉ cho phép listener HTTPS; đặt Urls=https://... và chạy --no-launch-profile.");
        if (options.Mtls.Enabled)
        {
            var serverCertificate = TlsCertificates.LoadPrivateCertificate(options.Mtls.ServerCertificatePath, options.Mtls.ServerCertificatePassword);
            var trustedCa = X509CertificateLoader.LoadCertificateFromFile(options.Mtls.TrustedCaPath);
            builder.WebHost.ConfigureKestrel(server => server.ConfigureHttpsDefaults(https =>
            {
                https.ServerCertificate = serverCertificate;
                https.ClientCertificateMode = ClientCertificateMode.RequireCertificate;
                https.ClientCertificateValidation = (certificate, chain, _) => TlsCertificates.Validate(certificate, trustedCa, TlsCertificates.ClientAuth, options.Mtls.CheckRevocation, chain);
            }));
        }
        builder.WebHost.ConfigureKestrel(x => x.Limits.MaxRequestBodySize = 310 * 1024 * 1024);
        builder.Services.AddSingleton(options); builder.Services.AddSingleton(new Store(options.DataRoot));
        builder.Services.AddSingleton(sp => new BundleReceiver(options.DataRoot, sp.GetRequiredService<Store>()));
        builder.Services.AddHostedService<InboxWorker>();
        var app = builder.Build();
        app.Use(async (context, next) =>
        {
            if (options.Mode == "Demo" && !(context.Connection.RemoteIpAddress is { } ip && IPAddress.IsLoopback(ip)))
            { context.Response.StatusCode = 403; return; }
            if (options.Mode == "Live" && context.Request.Path.StartsWithSegments("/api"))
            {
                var segments = context.Request.Path.Value!.Split('/', StringSplitOptions.RemoveEmptyEntries);
                string? expected = options.AdminApiKey;
                if (segments.Length > 2 && segments[1] == "stations") expected = options.StationKeys.GetValueOrDefault(segments[2]);
                if (expected == null || !CryptographicOperations.FixedTimeEquals(Encoding.UTF8.GetBytes(context.Request.Headers["X-Api-Key"].ToString()), Encoding.UTF8.GetBytes(expected)))
                { context.Response.StatusCode = 401; return; }
                var certificate = await context.Connection.GetClientCertificateAsync();
                var fingerprint = segments.Length > 2 && segments[1] == "stations"
                    ? options.Mtls.StationCertificateSha256.GetValueOrDefault(segments[2]) : options.Mtls.AdminCertificateSha256;
                if (!context.Request.IsHttps || certificate == null || fingerprint == null || !TlsCertificates.Matches(certificate, fingerprint))
                { context.Response.StatusCode = 403; return; }
            }
            try { await next(); }
            catch (Exception ex) when (ex is ArgumentException or InvalidDataException)
            { context.Response.StatusCode = 400; await context.Response.WriteAsJsonAsync(new { error = ex.Message }); }
        });
        app.UseDefaultFiles(); app.UseStaticFiles();
        app.MapGet("/api/status", (Store store) => new { mode = options.Mode, records = store.Records(), commands = store.Commands(), stations = store.Telemetry() });
        app.MapGet("/api/events", (Store store, string? ied) => store.Events(ied: ied));
        app.MapPost("/api/stations/{station}/events", (string station, SoeEvent[] events, Store store) =>
        {
            if (events.Length > 200 || events.Any(x => x.StationId != station || !CoreIdentity.SafeId(x.IedId) || x.Id.Length != 64 || x.Value.Length > 8192))
                return Results.BadRequest();
            store.AddEvents(events, delivered: true); return Results.Ok(events.Select(x => x.Id).ToArray());
        });
        app.MapPost("/api/stations/{station}/telemetry", (string station, System.Text.Json.JsonElement payload, Store store) =>
        {
            if (!CoreIdentity.SafeId(station) || payload.GetRawText().Length > 65536) return Results.BadRequest();
            store.Telemetry(station, payload.GetRawText()); return Results.Ok();
        });
        app.MapPost("/api/commands", (CommandRequest request, Store store) =>
        {
            if (options.Mode == "Live" && !options.StationKeys.ContainsKey(request.StationId)) return Results.BadRequest(new { error = "Trạm chưa đăng ký." });
            return Results.Ok(store.Enqueue(request));
        });
        app.MapGet("/api/stations/{station}/commands", (string station, Store store) =>
            store.Commands(station).Select(x => CommandSigning.Sign(x, options.Mode == "Demo" ? "" : options.StationKeys[station])));
        app.MapPost("/api/stations/{station}/commands/{id}/result", (string station, string id, CommandResult result, Store store) =>
        { store.CompleteCommand(id, station, result); return Results.Ok(); });
        app.MapGet("/api/stations/{station}/receipts/{id}", (string station, string id, Store store) =>
        {
            var row = store.Records().SingleOrDefault(x => x.Id == id);
            if (row == null || System.Text.Json.JsonSerializer.Deserialize<Manifest>(row.ManifestJson)?.StationId != station) return Results.NotFound();
            return Results.Ok(new Receipt(row.Id, CoreIdentity.Hash(row.ManifestJson), "Accepted"));
        });
        app.MapPost("/api/stations/{station}/bundles", async (string station, HttpRequest request, BundleReceiver receiver, CancellationToken ct) =>
        {
            if (options.Mode != "Demo" && !options.AllowHttpsBundles) return Results.NotFound();
            if (!CoreIdentity.SafeId(station)) return Results.BadRequest();
            var incoming = Path.Combine(options.DataRoot, "uploads"); Directory.CreateDirectory(incoming);
            var temp = Path.Combine(incoming, Guid.NewGuid().ToString("N") + ".zip");
            try
            {
                await using (var file = File.Create(temp)) await request.Body.CopyToAsync(file, ct);
                return Results.Ok(await receiver.ReceiveAsync(temp, station, ct));
            }
            finally { File.Delete(temp); }
        });
        app.MapGet("/api/records/{id}/download", (string id, Store store) =>
        {
            var row = store.Records().SingleOrDefault(x => x.Id == id);
            return row == null ? Results.NotFound() : Results.File(row.BundlePath, "application/zip", id + ".zip");
        });
        app.MapGet("/api/records/{id}/waveform", (string id, Store store, int? maxPoints, double? fromMs, double? toMs) =>
        {
            var row = store.Find(id);
            return row == null ? Results.NotFound() : Results.Ok(WaveformReader.Read(row.BundlePath, maxPoints ?? 2000, fromMs, toMs));
        });
        return app;

    }
}
