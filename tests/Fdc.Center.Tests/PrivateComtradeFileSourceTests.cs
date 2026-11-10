using System.Security.Cryptography;
using System.Text;
using System.Diagnostics;
using Fdc.Center;

namespace Fdc.Center.Tests;

public sealed class PrivateComtradeFileSourceTests
{
    [Fact]
    public async Task Prepared_stream_is_verified_bounded_and_independent_of_later_source_changes()
    {
        using var fixture = new FileFixture();
        var bytes = Encoding.UTF8.GetBytes("synthetic DAT bytes");
        await File.WriteAllBytesAsync(fixture.PathToFile, bytes);
        var digest = Convert.ToHexString(SHA256.HashData(bytes));
        await using (var stream = await fixture.Source.OpenVerifiedAsync(fixture.Key, bytes.Length, digest, 100, default))
        {
            Assert.NotNull(stream);
            await File.WriteAllTextAsync(fixture.PathToFile, "changed after verification");
            using var observed = new MemoryStream();
            await stream.CopyToAsync(observed);
            Assert.Equal(bytes, observed.ToArray());
        }
        Assert.Empty(Directory.EnumerateFiles(fixture.Staging));
    }

    [Fact]
    public async Task Wrong_size_digest_traversal_and_excess_size_do_not_return_a_stream()
    {
        using var fixture = new FileFixture();
        var bytes = Encoding.UTF8.GetBytes("synthetic DAT bytes");
        await File.WriteAllBytesAsync(fixture.PathToFile, bytes);
        var digest = Convert.ToHexString(SHA256.HashData(bytes));
        Assert.Null(await fixture.Source.OpenVerifiedAsync(fixture.Key, bytes.Length, new string('0', 64), 100, default));
        Assert.Null(await fixture.Source.OpenVerifiedAsync(fixture.Key, bytes.Length + 1, digest, 100, default));
        Assert.Null(await fixture.Source.OpenVerifiedAsync(fixture.Key, bytes.Length, digest, 1, default));
        Assert.Null(await fixture.Source.OpenVerifiedAsync("records/../fixture.dat", bytes.Length, digest, 100, default));
        Assert.Null(await fixture.Source.OpenVerifiedAsync(fixture.Key + ":alternate", bytes.Length, digest, 100, default));
        Assert.Empty(Directory.EnumerateFiles(fixture.Staging));
    }

    [A9FileLinkFact]
    public async Task Path_swapped_after_attributes_check_cannot_stage_matching_bytes_outside_root()
    {
        using var fixture = new FileFixture();
        var bytes = Encoding.UTF8.GetBytes("same digest both sides");
        var digest = Convert.ToHexString(SHA256.HashData(bytes));
        await File.WriteAllBytesAsync(fixture.PathToFile, bytes);
        var outside = Directory.CreateTempSubdirectory("FdcA9-outside-").FullName;
        var redirected = Path.Combine(outside, "fixture.dat");
        var directory = Path.GetDirectoryName(fixture.PathToFile)!;
        var moved = directory + "-original";
        var linked = false;
        Exception? swapError = null;
        try
        {
            await File.WriteAllBytesAsync(redirected, bytes);
            var opened = false;
            await using var stream = await fixture.Source.OpenVerifiedAsync(fixture.Key, bytes.Length, digest, 100, path =>
            {
                try
                {
                    Directory.Move(directory, moved);
                    CreateDirectoryRedirect(directory, outside);
                    linked = true;
                    var result = File.OpenRead(path);
                    opened = true;
                    return result;
                }
                catch (Exception error)
                {
                    swapError = error;
                    throw;
                }
            }, default);
            Assert.Null(swapError);
            Assert.True(opened);
            Assert.Null(stream);
            Assert.Empty(Directory.EnumerateFiles(fixture.Staging));
        }
        finally
        {
            if (linked) Directory.Delete(directory);
            File.Delete(redirected);
            Directory.Delete(outside);
        }
    }

    internal static void CreateDirectoryRedirect(string path, string target)
    {
        if (!OperatingSystem.IsWindows())
        {
            Directory.CreateSymbolicLink(path, target);
            return;
        }
        var start = new ProcessStartInfo("powershell.exe") { UseShellExecute = false, CreateNoWindow = true };
        foreach (var argument in new[] { "-NoProfile", "-NonInteractive", "-EncodedCommand" })
            start.ArgumentList.Add(argument);
        const string script = "$ErrorActionPreference='Stop'; New-Item -ItemType Junction -Path $env:FDC_A9_LINK -Target $env:FDC_A9_TARGET | Out-Null";
        start.ArgumentList.Add(Convert.ToBase64String(Encoding.Unicode.GetBytes(script)));
        start.Environment["FDC_A9_LINK"] = path;
        start.Environment["FDC_A9_TARGET"] = target;
        using var process = Process.Start(start) ?? throw new InvalidOperationException("Could not launch junction fixture.");
        if (!process.WaitForExit(10000))
        {
            process.Kill();
            throw new InvalidOperationException("Junction fixture did not exit.");
        }
        if (process.ExitCode != 0) throw new InvalidOperationException("Could not create test junction.");
    }

    private sealed class FileFixture : IDisposable
    {
        private readonly string root = Directory.CreateTempSubdirectory("FdcA7-file-test-").FullName;
        public string Key { get; } = $"records/{new string('a', 64)}/fixture.dat";
        public string PathToFile { get; }
        public string Staging { get; }
        public PrivateComtradeFileSource Source { get; }

        public FileFixture()
        {
            PathToFile = Path.Combine(root, Key);
            Directory.CreateDirectory(Path.GetDirectoryName(PathToFile)!);
            Staging = Directory.CreateDirectory(Path.Combine(root, "spool")).FullName;
            Source = new(root, Staging);
        }

        public void Dispose() => Directory.Delete(root, recursive: true);
    }
}

internal sealed class A9FileLinkFactAttribute : FactAttribute
{
    public A9FileLinkFactAttribute()
    {
        if (!(OperatingSystem.IsWindows() || OperatingSystem.IsLinux()) ||
            Environment.GetEnvironmentVariable("FDC_RUN_A9_FILE_LINK") != "1")
            Skip = "A.9 requires explicit opt-in on a host permitted to create symbolic links.";
    }
}
