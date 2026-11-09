using System.Security.Cryptography;
using System.Text;
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
