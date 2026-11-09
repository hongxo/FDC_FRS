namespace Fdc.Identity.Tests;

// These fixtures share an empty singleton authority and intentionally use rollback-only users.
// Serializing independent fixture setup prevents unconfigured probe rows from contaminating
// unrelated fixtures. Concurrent participants WITHIN CommitGateSqlLabTests remain concurrent.
[CollectionDefinition("IAM database lab", DisableParallelization = true)]
public sealed class IamDatabaseCollection;
