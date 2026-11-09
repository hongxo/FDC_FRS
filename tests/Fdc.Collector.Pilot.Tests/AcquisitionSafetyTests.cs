using System.Collections.Immutable;
using System.Net;
using Fdc.Collector.Pilot;

namespace Fdc.Collector.Pilot.Tests;

public sealed class AcquisitionSafetyTests
{
    [Theory] [InlineData("../x.cfg")] [InlineData("/a/../x.cfg")] [InlineData("a\\x.cfg")]
    [InlineData("x.cfg:stream")] [InlineData("%2e%2e/x.cfg")] [InlineData("*.cfg")]
    public void Unsafe_names_rejected_before_any_file_or_network_operation(string path) => Assert.Throws<InvalidDataException>(() => AcquisitionSafety.FileName(path));
    [Fact] public void Profile_and_default_pending_authority_prevent_network_before_native_load()
    {
        var now = DateTimeOffset.UtcNow; var target = new IedTarget("TEST", "IED", IPAddress.Loopback, 18102, "/", null, null, null);
        var profile = new LabReadProfile("FIXTURE-ONLY", "NOT-REAL-APPROVAL", now.AddMinutes(-1), now.AddMinutes(1), ImmutableHashSet.Create("127.0.0.1:18102"), 1000, 1000, 10, 10000, 10000, TimeSpan.FromMilliseconds(1));
        AcquisitionSafety.Validate(target, profile, now);
        Assert.Throws<InvalidOperationException>(() => new MmsReadOnlyAdapter("missing-native", "", target, profile, new PendingLabReadAuthorization(), TimeProvider.System, EvidenceKind.SIMULATED, default));
        Assert.Throws<InvalidOperationException>(() => AcquisitionSafety.Validate(target, profile with { Endpoints = [] }, now));
        Assert.Throws<InvalidOperationException>(() => AcquisitionSafety.Validate(target, profile with { Until = now }, now));
        Assert.Throws<InvalidOperationException>(() => AcquisitionSafety.Validate(target, profile with { RequestTimeoutMs = 0 }, now));
    }
    [Theory] [InlineData(4, CapabilityState.UNSUPPORTED_FILE_SERVICE)] [InlineData(21, CapabilityState.ACCESS_DENIED)] [InlineData(20, CapabilityState.FAILED)]
    public void Protocol_errors_are_not_equated_with_support(int code, CapabilityState expected) => Assert.Equal(expected, new IedFileServiceException(code).State);
}
