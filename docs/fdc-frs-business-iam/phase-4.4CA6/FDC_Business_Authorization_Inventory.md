# FDC Business Authorization Inventory

## Route inventory

| Route / UI | Method, handler | Data / ownership | Current boundary | Gap / permission / risk |
|---|---|---|---|---|
| Legacy /, /events.html, /waveform.html và assets | GET, static CenterApplication | Legacy Store, events/files | Demo hoặc legacy Live key/cert | Không dùng cho Human IAM; high |
| /api/status | GET CenterApplication / CenterIdentityRuntime | Records, commands, telemetry | Legacy; Human 503 | Dashboard scope; high |
| /api/events | GET cùng hai host | SOE; station/IED | Legacy; Human 503 | records.events.view; high |
| /api/commands | POST cùng hai host | Command, station | Legacy; Human 503 | Approved write workflow; critical |
| /api/records/{id}/download, /waveform | GET cùng hai host | Legacy record path | Legacy; Human 503 | Separate record/download permission; critical |
| /api/stations/{station}/events, /telemetry, /bundles | POST legacy | Station and IED data, bundle/file | Legacy station boundary; Human 403 | D07 machine migration; critical |
| /api/stations/{station}/commands, /receipts/{id} | GET legacy | Station command and receipt | Legacy station boundary; Human 403 | Machine operation binding; high |
| /api/stations/{station}/commands/{id}/result | POST legacy | Station command result | Legacy station boundary; Human 403 | Machine operation binding; high |
| /api/pilot/v1/ingestion, /{ticket}/complete | POST PilotIngestionEndpoints | Manifest, ticket, station binding | Separate machine scheme | D01/D07 remain gated; critical |
| /api/pilot/v1/ingestion/{ticket}/files/{name} | PUT PilotIngestionEndpoints | Private staged content | Machine ticket and registry | D01/D07; critical |
| /api/pilot/v1/ingestion/{ticket} | GET PilotIngestionEndpoints | Ticket status | Machine bound | D01/D07; high |
| /api/lab/v1/readiness | GET LabReadinessEndpoints | Status only | Human 503 | CATALOG_GAP for Lab operations; high |
| /identity/, /identity/management.html | GET HumanIdentityUi | Status/management shell | Public status; management Human | Business nav release pending |
| /api/identity/v1/ui/auth-status, /navigation | GET HumanIdentityUi | Status, effective menu | Public status; Human nav | No operational data |
| /api/identity/v1/admin/* | GET role/management adapters | IAM scope, grant, audit | Human and policy | No business authority inferred |
| /identity/business.html | GET A.6 | Scoped read screen | Human; DB profile | No write or Lab execute |
| /api/business/v1/dashboard | GET A.6 | Station aggregate | Human + reviewed op + SQL scope | dashboard.view proposed |
| /api/business/v1/{stations,ieds,records,collectors}[/{id}] | GET A.6 | Station canonical, IED, record, collector | Human + per-op scope; SQL filter | CATALOG_GAP for Collector read mapping |
| /api/business/v1/record-files/{id}/download | GET A.6 | Verified file joined to record and station | Human + separate read policy + private store + audit | Storage provider pending |
| InboxWorker / processing outbox / analysis job | Background | Bundle, station-owned rows | Legacy or system component | Human job export/recheck pending |

Không thấy WebSocket/SSE hoặc PATCH/DELETE business endpoint trong Center source rà soát. Không có Lab execute endpoint trong Human lane. UI prototype và fixture không mở IED.
