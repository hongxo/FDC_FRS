# Dynamic role management

Existing SQL role list/detail/current-permission queries reused. New RoleProposalValidator creates inactive zero-permission previews, validates unique occupied/retired codes using server-supplied set, rejects unsupported/wildcard/OT permissions, validates base version/catalog and explicit permission-diff kind. Clone gets new ID/code, no seed provenance, stays inactive and carries only supported role configuration, never UserRole/StationGrant/delegation.

These are preview/domain checks. SQL UNIQUE/immutable-code/history constraints already exist in migration001, untouched. Current-role state comes from server repositories; preview objects are never accepted as authorization. Activate preview does not activate a row. Existing seven-role manifest remains pending/unseeded. No role-name/code authorization branch or default superuser added.

Effective create/edit/clone/activate/deactivate/permission writes and role-assignment/history API extensions are not activated or completed. D04 operation mapping is missing; role.permission.manage is not silently expanded into review/approve/read permissions. D09/D10 atomic persistence and approved workflow mapping remain required. Full role CRUD acceptance remains NOT_RUN.
