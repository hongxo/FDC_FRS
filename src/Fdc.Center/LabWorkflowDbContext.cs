using Microsoft.EntityFrameworkCore;

namespace Fdc.Center;

// Maps the existing 007 PermitDraft model with the optional disposable-only 008 extension.
public sealed class LabWorkflowDbContext(DbContextOptions<LabWorkflowDbContext> options) : DbContext(options)
{
    public DbSet<LabPermit> Permits => Set<LabPermit>();
    public DbSet<LabWorkflowHistory> History => Set<LabWorkflowHistory>();
    public DbSet<LabExportJob> Exports => Set<LabExportJob>();
    protected override void OnModelCreating(ModelBuilder model)
    {
        var permit = model.Entity<LabPermit>();
        permit.ToTable("PermitDraft", "labprep");
        permit.HasKey(row => new { row.PermitId, row.PermitVersion });
        permit.Property(row => row.VersionStamp).IsRowVersion();
        permit.Property(row => row.StartsUtc).HasColumnType("datetime2(3)");
        permit.Property(row => row.EndsUtc).HasColumnType("datetime2(3)");
        model.Entity<LabWorkflowHistory>().ToTable("WorkflowHistory", "labprep").HasKey(row => row.Id);
        model.Entity<LabExportJob>().ToTable("ExportJob", "labprep").HasKey(row => row.Id);
        model.Entity<LabExportJob>().Property(row => row.VersionStamp).IsRowVersion();
        model.Entity<CollectorBindingRequest>().ToTable("CollectorBindingRequest", "ops").HasKey(row => row.Id);
        model.Entity<CollectorBindingRequest>().Property(row => row.VersionStamp).IsRowVersion();
        model.Entity<CollectorBindingHistory>().ToTable("CollectorBindingHistory", "ops").HasKey(row => row.Id);
    }
}

public sealed class CollectorBindingRequest
{
    public Guid Id { get; set; }
    public Guid CollectorId { get; set; }
    public Guid StationId { get; set; }
    public long ExpectedConfigurationVersion { get; set; }
    public string TargetStatus { get; set; } = "";
    public string State { get; set; } = "Pending";
    public string RequesterPerson { get; set; } = "";
    public string? ReviewerPerson { get; set; }
    public string? ApproverPerson { get; set; }
    public string? ApplierPerson { get; set; }
    public DateTimeOffset ExpiresUtc { get; set; }
    public byte[] VersionStamp { get; set; } = [];
}
public sealed class CollectorBindingHistory
{
    public Guid Id { get; set; }
    public Guid RequestId { get; set; }
    public Guid EventId { get; set; }
}

public sealed class LabExportJob
{
    public Guid Id { get; set; }
    public string PermitId { get; set; } = "";
    public int PermitVersion { get; set; }
    public Guid StationId { get; set; }
    public Guid ActorUserId { get; set; }
    public string SessionDigest { get; set; } = "";
    public string State { get; set; } = "Pending";
    public DateTimeOffset ExpiresUtc { get; set; }
    public Guid? LeaseOwner { get; set; }
    public DateTimeOffset? LeaseUntilUtc { get; set; }
    public byte[]? Payload { get; set; }
    public string? PayloadDigest { get; set; }
    public byte[] VersionStamp { get; set; } = [];
}

public sealed class LabPermit
{
    public string PermitId { get; set; } = "";
    public int PermitVersion { get; set; }
    public Guid InventoryId { get; set; }
    public byte[] ScopeDigest { get; set; } = [];
    public DateTime StartsUtc { get; set; }
    public DateTime EndsUtc { get; set; }
    public string DraftStatus { get; set; } = "DRAFT";
    public string DraftJson { get; set; } = "";
    public byte[] VersionStamp { get; set; } = [];
    public Guid? StationId { get; set; }
    public Guid? IedId { get; set; }
    public string? TestType { get; set; }
    public bool? IsSynthetic { get; set; }
    public string? RequesterPerson { get; set; }
    public string? ReviewerPerson { get; set; }
    public string? ApproverPerson { get; set; }
    public string? ApplierPerson { get; set; }
    public string? PermitDigest { get; set; }
    public string? ResultDigest { get; set; }
}

public sealed class LabWorkflowHistory
{
    public Guid Id { get; set; }
    public string PermitId { get; set; } = "";
    public int PermitVersion { get; set; }
    public Guid ActorUserId { get; set; }
    public string ActorPerson { get; set; } = "";
    public string State { get; set; } = "";
    public string Action { get; set; } = "";
    public string CorrelationId { get; set; } = "";
    public DateTimeOffset RecordedUtc { get; set; }
    public Guid RequestId { get; set; }
    public string RequestDigest { get; set; } = "";
    public byte[] ReplyVersion { get; set; } = [];
}
