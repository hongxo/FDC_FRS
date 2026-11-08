-- REVIEW CANDIDATE. Requires owner D09/schema disposition and confirmed disposable lab authorization.
-- Session context must be set explicitly by the authorized migration operator, never by application startup.
IF COALESCE(TRY_CONVERT(bit, SESSION_CONTEXT(N'Fdc.ConfirmedDisposableLab')),0) <> 1
 OR COALESCE(CONVERT(nvarchar(128),SESSION_CONTEXT(N'Fdc.ExpectedLabDatabase')),N'') <> DB_NAME()
 THROW 51000, 'Confirmed disposable lab and exact expected database required.', 1;

EXEC(N'-- Refuse rollback if any IAM data exists. Restore/roll-forward requires a separate reviewed runbook.
IF OBJECT_ID(N''[iam].[AdministrativeDelegation]'',N''U'') IS NOT NULL AND EXISTS(SELECT 1 FROM [iam].[AdministrativeDelegation]) THROW 51005, ''Nonempty IAM foundation cannot be destructively rolled back.'', 1;
IF OBJECT_ID(N''[ops].[AuditOutbox]'',N''U'') IS NOT NULL AND EXISTS(SELECT 1 FROM [ops].[AuditOutbox]) THROW 51005, ''Nonempty IAM foundation cannot be destructively rolled back.'', 1;
IF OBJECT_ID(N''[iam].[AuthorityVersion]'',N''U'') IS NOT NULL AND EXISTS(SELECT 1 FROM [iam].[AuthorityVersion]) THROW 51005, ''Nonempty IAM foundation cannot be destructively rolled back.'', 1;
IF OBJECT_ID(N''[iam].[Challenge]'',N''U'') IS NOT NULL AND EXISTS(SELECT 1 FROM [iam].[Challenge]) THROW 51005, ''Nonempty IAM foundation cannot be destructively rolled back.'', 1;
IF OBJECT_ID(N''[iam].[GrantFunction]'',N''U'') IS NOT NULL AND EXISTS(SELECT 1 FROM [iam].[GrantFunction]) THROW 51005, ''Nonempty IAM foundation cannot be destructively rolled back.'', 1;
IF OBJECT_ID(N''[iam].[MfaCredential]'',N''U'') IS NOT NULL AND EXISTS(SELECT 1 FROM [iam].[MfaCredential]) THROW 51005, ''Nonempty IAM foundation cannot be destructively rolled back.'', 1;
IF OBJECT_ID(N''[iam].[PasswordCredential]'',N''U'') IS NOT NULL AND EXISTS(SELECT 1 FROM [iam].[PasswordCredential]) THROW 51005, ''Nonempty IAM foundation cannot be destructively rolled back.'', 1;
IF OBJECT_ID(N''[iam].[RoleChangeApproval]'',N''U'') IS NOT NULL AND EXISTS(SELECT 1 FROM [iam].[RoleChangeApproval]) THROW 51005, ''Nonempty IAM foundation cannot be destructively rolled back.'', 1;
IF OBJECT_ID(N''[ops].[RoleHistory]'',N''U'') IS NOT NULL AND EXISTS(SELECT 1 FROM [ops].[RoleHistory]) THROW 51005, ''Nonempty IAM foundation cannot be destructively rolled back.'', 1;
IF OBJECT_ID(N''[iam].[RoleChangeRequest]'',N''U'') IS NOT NULL AND EXISTS(SELECT 1 FROM [iam].[RoleChangeRequest]) THROW 51005, ''Nonempty IAM foundation cannot be destructively rolled back.'', 1;
IF OBJECT_ID(N''[iam].[RolePermission]'',N''U'') IS NOT NULL AND EXISTS(SELECT 1 FROM [iam].[RolePermission]) THROW 51005, ''Nonempty IAM foundation cannot be destructively rolled back.'', 1;
IF OBJECT_ID(N''[iam].[Permission]'',N''U'') IS NOT NULL AND EXISTS(SELECT 1 FROM [iam].[Permission]) THROW 51005, ''Nonempty IAM foundation cannot be destructively rolled back.'', 1;
IF OBJECT_ID(N''[audit].[SecurityEvent]'',N''U'') IS NOT NULL AND EXISTS(SELECT 1 FROM [audit].[SecurityEvent]) THROW 51005, ''Nonempty IAM foundation cannot be destructively rolled back.'', 1;
IF OBJECT_ID(N''[ops].[SeedInstallation]'',N''U'') IS NOT NULL AND EXISTS(SELECT 1 FROM [ops].[SeedInstallation]) THROW 51005, ''Nonempty IAM foundation cannot be destructively rolled back.'', 1;
IF OBJECT_ID(N''[iam].[Session]'',N''U'') IS NOT NULL AND EXISTS(SELECT 1 FROM [iam].[Session]) THROW 51005, ''Nonempty IAM foundation cannot be destructively rolled back.'', 1;
IF OBJECT_ID(N''[iam].[StationGrant]'',N''U'') IS NOT NULL AND EXISTS(SELECT 1 FROM [iam].[StationGrant]) THROW 51005, ''Nonempty IAM foundation cannot be destructively rolled back.'', 1;
IF OBJECT_ID(N''[catalog].[Scope]'',N''U'') IS NOT NULL AND EXISTS(SELECT 1 FROM [catalog].[Scope]) THROW 51005, ''Nonempty IAM foundation cannot be destructively rolled back.'', 1;
IF OBJECT_ID(N''[catalog].[Station]'',N''U'') IS NOT NULL AND EXISTS(SELECT 1 FROM [catalog].[Station]) THROW 51005, ''Nonempty IAM foundation cannot be destructively rolled back.'', 1;
IF OBJECT_ID(N''[catalog].[ManagementUnit]'',N''U'') IS NOT NULL AND EXISTS(SELECT 1 FROM [catalog].[ManagementUnit]) THROW 51005, ''Nonempty IAM foundation cannot be destructively rolled back.'', 1;
IF OBJECT_ID(N''[catalog].[Corporation]'',N''U'') IS NOT NULL AND EXISTS(SELECT 1 FROM [catalog].[Corporation]) THROW 51005, ''Nonempty IAM foundation cannot be destructively rolled back.'', 1;
IF OBJECT_ID(N''[iam].[UserRole]'',N''U'') IS NOT NULL AND EXISTS(SELECT 1 FROM [iam].[UserRole]) THROW 51005, ''Nonempty IAM foundation cannot be destructively rolled back.'', 1;
IF OBJECT_ID(N''[iam].[Role]'',N''U'') IS NOT NULL AND EXISTS(SELECT 1 FROM [iam].[Role]) THROW 51005, ''Nonempty IAM foundation cannot be destructively rolled back.'', 1;
IF OBJECT_ID(N''[iam].[User]'',N''U'') IS NOT NULL AND EXISTS(SELECT 1 FROM [iam].[User]) THROW 51005, ''Nonempty IAM foundation cannot be destructively rolled back.'', 1;
IF OBJECT_ID(N''[iam].[PersonReference]'',N''U'') IS NOT NULL AND EXISTS(SELECT 1 FROM [iam].[PersonReference]) THROW 51005, ''Nonempty IAM foundation cannot be destructively rolled back.'', 1;
DROP TABLE IF EXISTS [iam].[AdministrativeDelegation];
DROP TABLE IF EXISTS [ops].[AuditOutbox];
DROP TABLE IF EXISTS [iam].[AuthorityVersion];
DROP TABLE IF EXISTS [iam].[Challenge];
DROP TABLE IF EXISTS [iam].[GrantFunction];
DROP TABLE IF EXISTS [iam].[MfaCredential];
DROP TABLE IF EXISTS [iam].[PasswordCredential];
DROP TABLE IF EXISTS [iam].[RoleChangeApproval];
DROP TABLE IF EXISTS [ops].[RoleHistory];
DROP TABLE IF EXISTS [iam].[RoleChangeRequest];
DROP TABLE IF EXISTS [iam].[RolePermission];
DROP TABLE IF EXISTS [iam].[Permission];
DROP TABLE IF EXISTS [audit].[SecurityEvent];
DROP TABLE IF EXISTS [ops].[SeedInstallation];
DROP TABLE IF EXISTS [iam].[Session];
DROP TABLE IF EXISTS [iam].[StationGrant];
DROP TABLE IF EXISTS [catalog].[Scope];
DROP TABLE IF EXISTS [catalog].[Station];
DROP TABLE IF EXISTS [catalog].[ManagementUnit];
DROP TABLE IF EXISTS [catalog].[Corporation];
DROP TABLE IF EXISTS [iam].[UserRole];
DROP TABLE IF EXISTS [iam].[Role];
DROP TABLE IF EXISTS [iam].[User];
DROP TABLE IF EXISTS [iam].[PersonReference];
-- Shared schemas and SQL roles are deliberately retained; remove membership/roles only with separate DBA review.
');
