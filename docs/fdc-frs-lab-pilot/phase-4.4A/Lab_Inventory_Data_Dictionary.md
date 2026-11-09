# Lab inventory data dictionary

| Entity | Fields and constraints |
|---|---|
| pilot_inventory_schema | Version 1 marker |
| pilot_inventory_batches | Preview hash PK, sheet, header row, import UTC |
| pilot_inventory_devices | Station plus normalized function code PK, unique station plus IPv4, typed JSON containing original serial, bay, device, classification and nullable metadata |
| pilot_inventory_rows | Batch plus Excel row PK, FK to batch and optional FK to device, validation code list |
| pilot_inventory_reviews | Append-only classification review with reviewer assertion, reference and UTC, FK to accepted batch row, indexed for latest review |

The commit runs in one SQLite transaction. Imported TestEnabled and ApprovedForTesting are false and CapabilityStatus is NOT_TESTED. Firmware, IED name and protocol remain unknown until evidence is available. Independent test scope lives outside the import. Phase 4.4 SQLite acquisition history and Center SQL ingest/outbox are reused. SQL inventory deployment was not part of this run.

2026-11-09 Asia/Bangkok. Independent reviewer pending.
