# Dynamic authorization report

SqlCurrentAuthorityReader now loads current SQL UserRole/Role/RolePermission/StationGrant/GrantFunction/AdministrativeDelegation and authority versions. Every read creates a fresh context and coherent serializable read transaction on a writable primary. No cache or memory mutex grants authority. Trusted object ownership must resolve using the same context/transaction; no request stationId is proof. UUID scope IDs use canonical D formatting; trusted resolver must use the same identity representation.

PendingPermissionPolicy resolves no descriptors; unsupported/unreviewed permission fails closed. No business RoleCode/RoleName checks or wildcard admin. Existing evaluator retains same-assignment joins, current versions, MFA/SoD and mandatory OT write deny. SQL reader does not activate a catalog or change approved matrix.

Executed SQL integration proves missing session denies and separate session reader observes current grant version/disable/revoke/epoch changes. Populated SQL end-to-end permission joins, scoped list/count/download SQL filtering, concurrent grant changes and two-node revoke/commit ordering: NOT_RUN. Serializable read snapshot does not retain a gate through a later business commit. PendingD09CommitBoundary remains blocking.

Full CRUD/assignment apply/request-review workflow and effective role activation belong to Phase3 after dependencies. No station grant/user/password/permission seed was installed. No source registration in Center or Collector.
