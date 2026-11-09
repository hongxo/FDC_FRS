using Fdc.Core.Lab;

namespace Fdc.Center.Tests;

public sealed class LabReadinessGateTests
{
    private static readonly DateTimeOffset Now = new(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);

    [Fact]
    public void Complete_proposal_remains_blocked_even_with_well_formed_evidence()
    {
        var draft = Draft();
        var result = LabReadinessGate.Evaluate(draft, Now);
        Assert.Equal("BLOCKED_BY_POLICY", result.Status);
        Assert.Equal(64, result.ScopeHash!.Length);
        Assert.Equal(result.ScopeHash, LabReadinessGate.Evaluate(draft, Now).ScopeHash);
    }

    [Theory]
    [InlineData("MODEL", "INVENTORY_FIELDS_REQUIRED")]
    [InlineData("FIRMWARE", "INVENTORY_FIELDS_REQUIRED")]
    [InlineData("WILDCARD", "EXACT_UNICAST_ENDPOINT_REQUIRED")]
    [InlineData("MULTICAST", "EXACT_UNICAST_ENDPOINT_REQUIRED")]
    [InlineData("CONTROL", "EXPLICIT_READ_ONLY_SCOPE_REQUIRED")]
    [InlineData("PATH", "EXPLICIT_READ_ONLY_SCOPE_REQUIRED")]
    [InlineData("FILE", "EXPLICIT_READ_ONLY_SCOPE_REQUIRED")]
    [InlineData("LIMIT", "FINITE_LIMITS_REQUIRED")]
    [InlineData("TIMEZONE", "VALID_WINDOW_REQUIRED")]
    [InlineData("STOP", "STOP_OWNER_REQUIRED")]
    [InlineData("ROLLBACK", "ROLLBACK_REQUIRED")]
    [InlineData("MANIFEST", "INVALID_EVIDENCE_MANIFEST")]
    public void Invalid_proposals_fail_closed(string field, string expected)
    {
        var valid = Draft();
        var draft = field switch
        {
            "MODEL" => valid with { Model = null },
            "FIRMWARE" => valid with { Firmware = null },
            "WILDCARD" => valid with { IpAddress = "*" },
            "MULTICAST" => valid with { IpAddress = "224.0.0.1" },
            "CONTROL" => valid with { Operations = ["WRITE"] },
            "PATH" => valid with { Directories = ["/records/../etc"] },
            "FILE" => valid with { FileNames = ["*.dat"] },
            "LIMIT" => valid with { MaximumSessions = 0 },
            "TIMEZONE" => valid with { TimeZone = "not-a-time-zone" },
            "STOP" => valid with { StopOwner = null },
            "ROLLBACK" => valid with { RollbackReference = null },
            "MANIFEST" => valid with { Evidence = [new("SCADA", "placeholder", "bad")] },
            _ => throw new ArgumentOutOfRangeException(nameof(field))
        };
        var result = LabReadinessGate.Evaluate(draft, Now);
        Assert.Equal("NOT_READY", result.Status);
        Assert.Null(result.ScopeHash);
        Assert.Contains(expected, result.Findings);
    }

    [Theory]
    [InlineData("INVENTORY")]
    [InlineData("SCADA")]
    [InlineData("TIME")]
    [InlineData("SECURITY")]
    [InlineData("NETWORK")]
    public void Missing_evidence_reference_fails_closed(string kind)
    {
        var draft = Draft();
        var result = LabReadinessGate.Evaluate(draft with
        {
            Evidence = draft.Evidence!.Where(item => item.Kind != kind).ToArray()
        }, Now);
        Assert.Equal("NOT_READY", result.Status);
        Assert.Contains(kind + "_REFERENCE_REQUIRED", result.Findings);
    }

    [Fact]
    public void Exact_expiration_boundary_is_expired()
    {
        var draft = Draft();
        Assert.Equal("BLOCKED_BY_POLICY", LabReadinessGate.Evaluate(draft, draft.End.AddTicks(-1)).Status);
        Assert.Equal("EXPIRED", LabReadinessGate.Evaluate(draft, draft.End).Status);
    }

    [Fact]
    public void Scope_changes_and_new_versions_produce_distinct_hashes()
    {
        var draft = Draft();
        var original = LabReadinessGate.Evaluate(draft, Now).ScopeHash;
        Assert.NotEqual(original, LabReadinessGate.Evaluate(draft with { Port = 103 }, Now).ScopeHash);
        Assert.NotEqual(original, LabReadinessGate.Evaluate(draft with { Version = 2 }, Now).ScopeHash);
    }

    private static LabPermitDraft Draft() => new()
    {
        PermitId = "LAB-IED-2026-0001", Version = 1, InventoryId = "fixture-row",
        Station = "fixture", Bay = "fixture", Vendor = "fixture", Model = "fixture", Firmware = "fixture",
        IedName = "fixture", IpAddress = "192.0.2.99", InterfaceName = "fixture", Port = 102,
        Operations = ["ASSOCIATE", "LIST", "READ"], Directories = ["/records"], FileNames = ["sample.cfg", "sample.dat"],
        MaximumFiles = 2, MaximumFileBytes = 1024, MaximumSessions = 1, ConnectionTimeoutSeconds = 1,
        RetryLimit = 0, PollIntervalSeconds = 5, MaximumDurationSeconds = 5, MaximumBytesPerSecond = 1024,
        Start = Now.AddMinutes(1), End = Now.AddMinutes(2), TimeZone = "UTC",
        Operator = "fixture", ProtectionEngineer = "fixture", ScadaSupervisor = "fixture",
        SecurityReviewer = "fixture", StopOwner = "fixture", StopConditions = "fixture",
        EmergencyContact = "fixture", IsolationReference = "fixture", RollbackReference = "fixture",
        CommunicationReference = "fixture", RequestedBy = "fixture",
        Evidence = new[] { "INVENTORY", "SCADA", "TIME", "SECURITY", "NETWORK" }
            .Select(kind => new LabEvidenceReference(kind, "fixture", new string('a', 64))).ToArray()
    };
}
