using System.Formats.Asn1;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

namespace Fdc.Center;

public enum CollectorCertificateStatus
{
    CertValid, CertRevoked, CertExpired, CertInvalid, RevocationUnknown, RevocationUnavailable
}

// Candidate for a directly issuing pinned RSA CA. No URL from a peer certificate is fetched.
// The caller must supply a fresh signed full CRL through an independently controlled channel.
// Not registered by Center startup. Intermediates, indirect CRLs, deltas and OCSP require separate policy.
public sealed class PinnedCrlCertificateValidator(X509Certificate2 issuer, TimeProvider clock)
{
    public CollectorCertificateStatus Validate(X509Certificate2 leaf, ReadOnlyMemory<byte>? fullCrl)
    {
        var chainStatus = ValidateChain(leaf);
        if (chainStatus != CollectorCertificateStatus.CertValid) return chainStatus;
        if (fullCrl is null) return CollectorCertificateStatus.RevocationUnavailable;
        try { return CheckCrl(leaf, fullCrl.Value, clock.GetUtcNow()); }
        catch (Exception error) when (error is AsnContentException or CryptographicException or ArgumentException)
        { return CollectorCertificateStatus.RevocationUnknown; }
    }

    internal CollectorCertificateStatus ValidateChain(X509Certificate2 leaf)
    {
        var now = clock.GetUtcNow();
        if (now.UtcDateTime < leaf.NotBefore.ToUniversalTime() || now.UtcDateTime >= leaf.NotAfter.ToUniversalTime())
            return CollectorCertificateStatus.CertExpired;
        if (!leaf.Extensions.OfType<X509EnhancedKeyUsageExtension>()
                .Any(extension => extension.EnhancedKeyUsages.Cast<Oid>().Any(oid => oid.Value == "1.3.6.1.5.5.7.3.2")) ||
            leaf.Extensions.OfType<X509BasicConstraintsExtension>().Any(extension => extension.CertificateAuthority) ||
            !issuer.Extensions.OfType<X509KeyUsageExtension>().Any(extension => extension.KeyUsages.HasFlag(X509KeyUsageFlags.CrlSign)))
            return CollectorCertificateStatus.CertInvalid;
        using var chain = new X509Chain();
        chain.ChainPolicy.TrustMode = X509ChainTrustMode.CustomRootTrust;
        chain.ChainPolicy.CustomTrustStore.Add(issuer);
        chain.ChainPolicy.RevocationMode = X509RevocationMode.NoCheck;
        chain.ChainPolicy.DisableCertificateDownloads = true;
        chain.ChainPolicy.VerificationTime = now.UtcDateTime;
        chain.ChainPolicy.ApplicationPolicy.Add(new Oid("1.3.6.1.5.5.7.3.2"));
        if (!chain.Build(leaf) || chain.ChainElements.Count != 2 ||
            !chain.ChainElements[1].Certificate.RawData.AsSpan().SequenceEqual(issuer.RawData))
            return CollectorCertificateStatus.CertInvalid;
        return CollectorCertificateStatus.CertValid;
    }

    private CollectorCertificateStatus CheckCrl(X509Certificate2 leaf, ReadOnlyMemory<byte> encoded, DateTimeOffset now)
    {
        if (encoded.Length is < 32 or > 4 * 1024 * 1024) return CollectorCertificateStatus.RevocationUnknown;
        var reader = new AsnReader(encoded, AsnEncodingRules.DER);
        var certificateList = reader.ReadSequence();
        reader.ThrowIfNotEmpty();
        var tbs = certificateList.ReadEncodedValue();
        var algorithm = certificateList.ReadEncodedValue();
        var algorithmReader = new AsnReader(algorithm, AsnEncodingRules.DER).ReadSequence();
        if (algorithmReader.ReadObjectIdentifier() != "1.2.840.113549.1.1.11") return CollectorCertificateStatus.RevocationUnknown;
        if (algorithmReader.HasData) algorithmReader.ReadNull();
        algorithmReader.ThrowIfNotEmpty();
        var signature = certificateList.ReadBitString(out var unused);
        certificateList.ThrowIfNotEmpty();
        using var key = issuer.GetRSAPublicKey();
        if (unused != 0 || key is null || !key.VerifyData(tbs.Span, signature, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1))
            return CollectorCertificateStatus.RevocationUnknown;

        var body = new AsnReader(tbs, AsnEncodingRules.DER).ReadSequence();
        if (body.PeekTag().HasSameClassAndValue(Asn1Tag.Integer) && body.ReadInteger() != 1)
            return CollectorCertificateStatus.RevocationUnknown;
        if (!body.ReadEncodedValue().Span.SequenceEqual(algorithm.Span) ||
            !body.ReadEncodedValue().Span.SequenceEqual(issuer.SubjectName.RawData))
            return CollectorCertificateStatus.RevocationUnknown;
        var issued = Time(body);
        if (!body.HasData || !IsTime(body.PeekTag())) return CollectorCertificateStatus.RevocationUnknown;
        var next = Time(body);
        if (issued > now || next <= now || next <= issued) return CollectorCertificateStatus.RevocationUnknown;
        var serial = leaf.GetSerialNumber();
        Array.Reverse(serial);
        var revoked = false;
        if (body.HasData && body.PeekTag().HasSameClassAndValue(Asn1Tag.Sequence))
        {
            var entries = body.ReadSequence();
            while (entries.HasData)
            {
                var entry = entries.ReadSequence();
                var number = entry.ReadIntegerBytes();
                var revokedAt = Time(entry);
                if (revokedAt <= now && Normalize(number.Span).SequenceEqual(Normalize(serial))) revoked = true;
                if (entry.HasData) Extensions(entry.ReadSequence(), false);
                entry.ThrowIfNotEmpty();
            }
        }
        if (body.HasData)
        {
            var extensions = body.ReadSequence(new Asn1Tag(TagClass.ContextSpecific, 0, true));
            Extensions(extensions.ReadSequence(), true);
            extensions.ThrowIfNotEmpty();
        }
        body.ThrowIfNotEmpty();
        return revoked ? CollectorCertificateStatus.CertRevoked : CollectorCertificateStatus.CertValid;
    }

    private static ReadOnlySpan<byte> Normalize(ReadOnlySpan<byte> bytes)
    {
        while (bytes.Length > 1 && bytes[0] == 0) bytes = bytes[1..];
        return bytes;
    }
    private static bool IsTime(Asn1Tag tag) => tag.HasSameClassAndValue(new Asn1Tag(UniversalTagNumber.UtcTime)) ||
        tag.HasSameClassAndValue(new Asn1Tag(UniversalTagNumber.GeneralizedTime));
    private static DateTimeOffset Time(AsnReader reader) =>
        reader.PeekTag().HasSameClassAndValue(new Asn1Tag(UniversalTagNumber.UtcTime)) ? reader.ReadUtcTime() : reader.ReadGeneralizedTime();

    private static void Extensions(AsnReader extensions, bool crl)
    {
        while (extensions.HasData)
        {
            var extension = extensions.ReadSequence();
            var oid = extension.ReadObjectIdentifier();
            var critical = extension.HasData && extension.PeekTag().HasSameClassAndValue(Asn1Tag.Boolean) && extension.ReadBoolean();
            // Indirect/delta CRLs and certificateIssuer entry semantics are not supported.
            if (critical || (crl && oid is not ("2.5.29.35" or "2.5.29.20")) ||
                (!crl && oid is not ("2.5.29.21" or "2.5.29.24")))
                throw new CryptographicException("Unsupported CRL extension.");
            extension.ReadOctetString();
            extension.ThrowIfNotEmpty();
        }
    }
}
