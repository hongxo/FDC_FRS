using System.Buffers;
using System.Security.Cryptography;

namespace Fdc.Center;

// Roots must be provisioned privately, outside the web root, with ACLs excluding untrusted writers.
// Path checks do not replace that deployment boundary. No default root is selected.
public sealed class PrivateComtradeFileSource : IReviewedBusinessFileSource
{
    private readonly string root;
    private readonly string staging;

    public PrivateComtradeFileSource(string storageRoot, string stagingRoot)
    {
        if (!Path.IsPathFullyQualified(storageRoot) || !Path.IsPathFullyQualified(stagingRoot))
            throw new ArgumentException("Absolute private roots required.");
        root = Path.GetFullPath(storageRoot);
        staging = Path.GetFullPath(stagingRoot);
    }

    internal static bool IsSafeFileName(string name) =>
        !string.IsNullOrWhiteSpace(name) && name is not ("." or "..") &&
        name.IndexOfAny([':', '/', (char)92]) < 0 &&
        name.IndexOfAny(Path.GetInvalidFileNameChars()) < 0 && !name.Any(char.IsControl);

    public async Task<Stream?> OpenVerifiedAsync(string objectKey, long size, string sha256, int cap, CancellationToken ct)
    {
        if (size <= 0 || size > cap || sha256.Length != 64 || !sha256.All(char.IsAsciiHexDigit)) return null;
        var parts = objectKey.Split('/');
        if (parts.Length != 3 || parts[0] != "records" || parts[1].Length != 64 ||
            !parts[1].All(c => char.IsAsciiHexDigit(c) && !char.IsUpper(c)) ||
            !IsSafeFileName(parts[2]) ||
            Path.GetExtension(parts[2]).ToLowerInvariant() is not (".cfg" or ".dat" or ".hdr" or ".inf"))
            return null;

        FileStream? copy = null;
        try
        {
            var source = Path.Combine(root, parts[0], parts[1], parts[2]);
            for (var directory = new DirectoryInfo(Path.GetDirectoryName(source)!); directory is not null; directory = directory.Parent)
                if ((directory.Attributes & FileAttributes.ReparsePoint) != 0 || !directory.Exists) return null;
            for (var directory = new DirectoryInfo(staging); directory is not null; directory = directory.Parent)
                if ((directory.Attributes & FileAttributes.ReparsePoint) != 0 || !directory.Exists) return null;
            if ((File.GetAttributes(source) & (FileAttributes.ReparsePoint | FileAttributes.Directory)) != 0) return null;
            await using var input = new FileStream(source, FileMode.Open, FileAccess.Read, FileShare.Read, 65536, FileOptions.Asynchronous);
            if (input.Length != size) return null;

            copy = new FileStream(Path.Combine(staging, Guid.NewGuid().ToString("N")), FileMode.CreateNew, FileAccess.ReadWrite,
                FileShare.None, 65536, FileOptions.Asynchronous | FileOptions.DeleteOnClose);
            using var digest = IncrementalHash.CreateHash(HashAlgorithmName.SHA256);
            var buffer = ArrayPool<byte>.Shared.Rent(65536);
            try
            {
                long copied = 0;
                int read;
                while ((read = await input.ReadAsync(buffer.AsMemory(0, 65536), ct)) != 0)
                {
                    copied += read;
                    if (copied > size) return null;
                    digest.AppendData(buffer, 0, read);
                    await copy.WriteAsync(buffer.AsMemory(0, read), ct);
                }
                if (copied != size || !string.Equals(Convert.ToHexString(digest.GetHashAndReset()), sha256,
                        StringComparison.OrdinalIgnoreCase)) return null;
            }
            finally { ArrayPool<byte>.Shared.Return(buffer, clearArray: true); }
            copy.Position = 0;
            var verified = copy;
            copy = null;
            return verified;
        }
        catch (Exception error) when (error is IOException or UnauthorizedAccessException or ArgumentException)
        { return null; }
        finally { if (copy is not null) await copy.DisposeAsync(); }
    }
}
