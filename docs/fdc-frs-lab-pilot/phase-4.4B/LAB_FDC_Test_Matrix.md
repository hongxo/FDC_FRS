# Ma trận LAB-FDC

Run local-02. SQL postcheck và kết quả lần đầu lưu dưới local-01. Build thành công. 297 PASS, 24 SKIP, 0 FAIL trong .NET. SKIP là các test opt-in chưa chạy trong profile này, không tính vào PASS. Browser đạt 8 màn offline, SVG và 3 viewport, không có yêu cầu mạng. TRX và evidence thô được giữ riêng. Firmware trong mọi ca thiết bị thật chưa xác minh; ca fixture không có firmware thiết bị. Reviewer độc lập chưa ký cho mọi dòng.

| ID | Phép thử | Môi trường/model | Điều kiện | Kỳ vọng | Kết quả thực thi | Bằng chứng | Kết quả | B44B defect | Reviewer |
|---|---|---|---|---|---|---|---|---|---|
| LAB-FDC-01 | Excel inventory | G1 / workbook riêng | Sheet IP, header 2 | Đọc và báo lỗi theo dòng | 17 dòng hợp lệ, 93 mục issue | mapping.log | PASS | — | Chờ review |
| LAB-FDC-02 | IP/device mapping | G1 / workbook riêng | PDF, workbook và người xác minh | Đúng từng model, bay, interface | Preview giữ nguyên IP; hiện trường chưa xác minh | mapping.private.json | BLOCKED | 01 | Chờ review |
| LAB-FDC-03 | Offline COMTRADE | G1 / fixture tổng hợp | CFG/DAT nguồn CLI | Parser và grouping thành công | Một record được tạo | offline/run.json; Collector TRX | PASS | — | Chờ review |
| LAB-FDC-04 | SQLite queue | G1 / fixture tổng hợp | SQLite riêng và fixture | Lease, retry, không mất dữ liệu | Queue, backoff và fencing assertions đạt | Collector TRX | PASS | — | Chờ review |
| LAB-FDC-05 | Center ingestion | G2 / mTLS fixture | SQL Lab và station binding fixture | Commit/outbox, ACK xác minh | Isolated mTLS → SQL/outbox → ACK đạt | Center PilotMtlsSqlTests TRX | PASS | — | Chờ review |
| LAB-FDC-06 | SQL integrity | G2 / SQL Lab | DB disposable local | Schema và consistency đạt | Schema/intake fixture đạt, DBCC không báo lỗi | Center TRX; sql-postcheck.txt | PASS | — | Chờ review |
| LAB-FDC-07 | Dashboard | G1 / fixture tổng hợp | Offline dashboard | 8 màn, waveform, responsive, không network | 8 màn và 3 viewport đạt | offline-dashboard-results.json | PASS | — | Chờ review |
| LAB-FDC-08 | MMS Siemens | G3 / Siemens 7SJ85 | Run scope và firmware được duyệt | MMS read-only | Chưa mở association | Real IED register | BLOCKED | 01/02 | Chờ review |
| LAB-FDC-09 | MMS ABB | G3 / ABB REF620 | Run scope và firmware được duyệt | MMS read-only | Chưa mở association | Real IED register | BLOCKED | 01/02 | Chờ review |
| LAB-FDC-10 | MMS Schneider | G3 / Schneider P643 | Run scope và firmware được duyệt | MMS read-only | Chưa mở association | Real IED register | BLOCKED | 01/02 | Chờ review |
| LAB-FDC-11 | File directory | G4 / IED thật | G3 và thư mục allowlist | Chỉ list thư mục đã duyệt | Native simulator list đạt; IED thật chưa chạy | native-simulated.json | BLOCKED | 02 | Chờ review |
| LAB-FDC-12 | COMTRADE download | G4 / IED thật | File set đã duyệt | Tải CFG/DAT đúng kích thước | Simulator tải khớp fixture; chưa có file thật | native-simulated.json | BLOCKED | 02/05 | Chờ review |
| LAB-FDC-13 | Checksum | G1/G2 / fixture | Byte fixture và manifest | SHA-256 khớp và reject tamper | Simulator byte compare và intake kiểm tra đạt | native-simulated.json; Center TRX | PASS | — | Chờ review |
| LAB-FDC-14 | Duplicate prevention | G1/G2 / fixture | Replay cùng dữ liệu | Không thêm record effective | Assertions fixture SQLite/SQL đạt | Collector và Center TRX | PASS | — | Chờ review |
| LAB-FDC-15 | Interrupted transfer | G1 / fixture | Inject partial file vào spool test | Không promote hoặc ACK; retry hữu hạn | Partial/low disk và backoff assertions đạt; chưa thử OT | Collector TRX | PASS | — | Chờ review |
| LAB-FDC-16 | Collector restart | G1 / fixture | Expired lease trên SQLite riêng | Fence lease cũ, giữ hàng đợi | Fixture restart/fencing đạt | Collector TRX | PASS | — | Chờ review |
| LAB-FDC-17 | Time synchronization | G3 / Lab | Nguồn UTC/lock và ngưỡng | Offset trong ngưỡng | Chưa có telemetry | SCADA baseline | BLOCKED | 04 | Chờ review |
| LAB-FDC-18 | SCADA coexistence | G4/G5 / Lab | Người giám sát và baseline | Không mất heartbeat hoặc quality | Chưa có telemetry | SCADA baseline | BLOCKED | 04 | Chờ review |
| LAB-FDC-19 | Unauthorized denial | G2 / fixture | Human/machine và station scope fixture | Từ chối sai route/scope | Boundary fixture đạt; integrated OT security còn BLOCKED | Center PilotMachineBoundaryTests TRX | PASS | 03 | Chờ review |
| LAB-FDC-20 | Multi-vendor E2E | G5 / IED thật | G3/G4 và identity đúng station | Mỗi model có bằng chứng hoặc giới hạn | Chưa chạy thiết bị thật | Real IED register | BLOCKED | 02/03/05 | Chờ review |
| LAB-FDC-21 | Recovery/rollback | G1 / SQLite fixture | DB local và queue fixture | Rollback/reapply đúng, từ chối khi có queue | Assertions fixture đạt; chưa thử rollback OT | Collector TRX | PASS | 05 | Chờ review |
| LAB-FDC-22 | Pre-site readiness | G6 / Lab | Biên bản ký và rollback ở trạm | G1–G5 và đánh giá độc lập | Chưa đủ điều kiện | Pre-site checklist | BLOCKED | 05 | Chờ review |
