# Station access management

Implemented SQL assignment-bound grant read adapter GET /api/identity/v1/admin/station-grants?userId=...&userRoleId=.... Both IDs are filters, not evidence. SQL joins UserRole owner and Role ID and intersects permitted user/assignment/grant/role IDs. GrantFunction query is also assignment-bound and permission-scoped; no station wildcard.

New StationGrantProposalValidator checks current active subject/assignment/role, exact UserRole linkage, independent actor PersonReference, server-approved scope envelope, GrantFunction subset of same role and supported catalog, UTC validity bounded by assignment/role/delegation expiry, grant/catalog/hierarchy versions, explicit reason. Returns canonical preview/digest and added/removed function diff. Overlap detection uses half-open lifetimes.

UserRoleProposal and AdministrativeDelegationProposal contracts added. Pending policy providers reject assignment conflicts/delegation changes until approved D04 rules; a station grant cannot stand in for global administration.

No effective grant/assignment/delegation writes, revoke/version invalidation writer or durable request/review/apply for grants are implemented. No automatic access to380 stations, unit or corporation. SQL read test PASS verifies wrong subject+assignment mix denied even if both assignment IDs are listed; proposal validation tests are domain checks, not live mutation acceptance.
