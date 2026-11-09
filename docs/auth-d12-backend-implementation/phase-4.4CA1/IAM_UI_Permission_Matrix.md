# Permission matrix

Mapping dưới đây là operation code trong source, chờ D04 release ánh xạ sang canonical permission codes. Không dùng bảng này để cấp quyền.

| Screen | Authority thực tế | Dependency |
|---|---|---|
| S01–S03 | Public generic status | D02/D03/D05/D06 cho flow |
| S04 | Public generic status, chưa có profile payload | D05 profile/session policy |
| S05 | Users, UserDetail, UserRoles | D04/D05 và server scope |
| S06 | IRoleAdministrationAuthority, PermissionCatalog | D04 catalog release |
| S07 | StationGrants và UserRole scope | D04/D09 |
| S08 | SecurityAudit, SecurityAuditDetail | D04/D10 |
| Request | ChangeRequest | D04/D08/D09, SoD |
| Lab | Human session, station mapping pending | D01/D04/D07 |

Mandatory deny ied.configuration.write giữ nguyên. UI không gán permission hoặc ghép role và StationGrant khác assignment.
