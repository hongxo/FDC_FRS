# Lab equipment inventory - reported, not device-verified

2026-11-09 Asia/Bangkok. Working-tree base:62267221990449f8d8e567cbdb828a5c44080154. Sources:read-only IP_LAB.xls sheet IP, BC_PhongLab_FINAL.pdf, and user messages. No connection to any inventory IP has been made. Reviewer:NOT_PROVIDED. Operational network addresses, notes/credentials and full source documents are not copied into GitHub deliverables.

| Sanitized alias | Function | Model/order information reported | Vendor/origin reported | Evidence | Firmware | Device read confirmation |
|---|---|---|---|---|---|---|
| ABB-A | F67 directional overcurrent | REF620 | ABB/EU | User explicitly supplied; IP sheet row30, PDF page109 list REF620 | UNKNOWN | NOT_TESTED |
| ABB-B | BCU | REC670 | ABB per PDF | IP sheet row34; PDF page109 | UNKNOWN | NOT_TESTED |
| SCHNEIDER-A | F87T transformer differential | P643 | Schneider/Anh per user | IP sheet row35; PDF page109; user message | UNKNOWN | NOT_TESTED |
| SCHNEIDER-B | F67 per inventory | P132 | Manufacturer not independently verified | IP sheet row36; do not infer firmware/File Services | UNKNOWN | NOT_TESTED |
| SIEMENS-A/B | F67 KTS-2 candidates | 7SJ85 | Siemens per PDF page105 | IP rows45 and51 repeat the same logical label; physical endpoints must be distinguished | UNKNOWN | NOT_TESTED |
| TOSHIBA-A/B | F87L differential | GRL200 | Toshiba/Viet Nam per user | IP rows28-29; exact first test endpoint not selected | UNKNOWN | NOT_TESTED |
| REGULATOR-A | F90 | REGDA (REG_DA in sheet) | UNKNOWN | User message and IP row39 | UNKNOWN | NOT_TESTED |
| SEL-A | F67 KTS | SEL order code supplied privately by user; sheet lists SEL_451 | SEL/My,2023 per user | IP row48; SKU/family relationship requires label verification | UNKNOWN | NOT_TESTED |

The user also listed F67 KTS-1/KTS-2/KTS-3 and digital switches. Do not map incomplete descriptions to a vendor/model merely from sequence or IEC61850-9-2 support. Process Bus/SV support does not prove MMS File Services or COMTRADE availability.

The workbook has24 named-device rows and18 distinct model strings; many other IP rows are blank/reserved/infrastructure, not an acquisition allowlist. The PDF158 pages describes Station Bus/Process Bus and RTU/Gateway-to-SCADA integration; relevant diagrams inspected on PDF pages70-72. These references describe design/inventory, not measured network isolation or a per-run authorization release.

Outstanding exact run inputs:chosen endpoint(s)/MMS port, allowed file directory, start/end window, timeout/poll/retry/bandwidth limits and operator stop procedure. General user confirmation that Lab-owner permission exists is recorded; no repeated approval requested, but omitted operational values must not be invented. Real connections remain blocked until those values are bound to a verified read-only profile. Existing independent IAM gates/readiness are unchanged.

Credential handling finding:the first local extraction included a notes cell containing credentials. The notes column was removed from the retained private inventory extraction, not used for authentication and is excluded from commits. Do not republish the original workbook or unfiltered extraction. Original source files remain unchanged.
