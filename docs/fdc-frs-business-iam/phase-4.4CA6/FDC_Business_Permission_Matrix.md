# FDC Business Permission Matrix

| Operation | Candidate catalog | Scope / object | Approval / audit |
|---|---|---|---|
| Dashboard station count | dashboard.view | StationGrant per station | D04 signed mapping |
| Station list/detail | station.view | Station.Id | D04 |
| IED list/detail | ied.view | IedDevice.StationId | D04 |
| Record metadata | records.events.view | DisturbanceRecord.StationId | D04 |
| Download CFG/DAT | records.download | RecordFile joined to record + station | D04/D10/E03; prepared event/outbox |
| Collector human | CATALOG_GAP | CollectorRegistration.StationId | D04/D07 |
| Lab view/prepare/approve/execute | CATALOG_GAP | Permit, station, IED | OT safety and permit independent |
| Machine ingest | Machine operation, no Human role | Registry and station binding | D01/D07 |

Các code là tham chiếu đề xuất, không seed. Không suy quyền theo role name.
