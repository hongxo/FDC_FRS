# IAM V1 compatibility

The requested IAM Administration V1 ref was absent from local and checked remote heads. Cross-branch compatibility remains unverified. The clean security branch uses the baseline canonical IAM and baseline regression tests. The source change extends only the OS worker integration harness. It exercises the existing export transaction and authority paths, including a release audit/outbox pair. Browser and IAM suites retain their existing UserRole, StationGrant, GrantFunction, AdministrativeDelegation, same-assignment, AuthorityVersion, session revoke and SoD assertions. See [suite evidence](FDC_A12_Raw_Evidence_Manifest.md).

The separate user-role UI experiment at `1209b90` is outside this security branch. The baseline integration result gives evidence against regressions in the checked-out IAM contracts. A clean replay with the actual IAM V1 branch is still required.
