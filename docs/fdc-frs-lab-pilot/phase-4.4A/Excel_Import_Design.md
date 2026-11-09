# Excel import design

Preview selects an explicit sheet and header row. An optional JSON mapping supplies Number, Bay, Device, Function and Address header names. The reader supports `.xlsx` and legacy `.xls`, takes a bounded read-only snapshot and validates canonical IPv4. Only mapped fields enter the preview. Issue codes preserve Excel row numbers. Both sides of an in-workbook duplicate IP or code are excluded. A reviewed preview hash is required to commit. Row errors block commit by default. An operator can explicitly select `--valid-only` after reviewing the error list. Unchanged replay is a no-op and conflicts with existing rows roll back the batch.

```powershell
dotnet run --project tools/Fdc.LabPilot -- inventory-preview <workbook> <sheet> <header> <station> <private.html> [mapping.json]
dotnet run --project tools/Fdc.LabPilot -- inventory-commit <workbook> <sheet> <header> <station> <fdc.db> <reviewedHash> [mapping.json|-] [--valid-only]
dotnet run --project tools/Fdc.LabPilot -- inventory-review <fdc.db> <review.json>
```

Run under the designated local operator account in a protected Lab directory. Workbook content is treated as data. The program does not evaluate formulas or resolve hyperlinks. Review JSON records a classification assertion. The authenticated management host and authenticated reviewer identity binding remain pending.

2026-11-09 Asia/Bangkok. Independent reviewer pending.
