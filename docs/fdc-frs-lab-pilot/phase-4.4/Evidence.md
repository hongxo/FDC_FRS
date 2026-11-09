# Evidence and reproduction

Tested source `1ad4f0a`. Windows, .NET SDK 10.0.403, SQL Server 2022 disposable Lab, libiec61850 loopback simulator and headless Edge.

- `dotnet restore Fdc.Identity.slnx` and `dotnet build Fdc.Identity.slnx --no-restore` restored and built the solution.
- `dotnet test Fdc.Identity.slnx --no-build --no-restore --logger trx` ran with Center Lab and native simulator opt-ins. The test attributes list the required environment variables. Supply database credentials through the local secret channel and pin both simulator and library hashes.
- `dotnet run --project tools/Fdc.LabPilot -- offline-demo <new-output-directory>` produces the fixture viewer.
- `node tests/Fdc.Collector.Pilot.Tests/browser/offline-dashboard.cjs` verifies that viewer using the local Playwright installation and evidence paths.
- SQL postcheck counted the schema and rows and ran `DBCC CHECKCONSTRAINTS WITH ALL_CONSTRAINTS`.

[Automated tests](evidence/tests.json), [simulated MMS](evidence/native-simulated.json), [offline run](evidence/offline-run.json), [browser checks](evidence/offline-dashboard-results.json) and [SQL postcheck](evidence/center-after.txt) are sanitized outputs. Raw TRX and screenshots are available locally under the ignored `tmp/phase4-4-evidence` directory. Optional older IAM SQL tests and two Center profiles were not enabled in the final run.
