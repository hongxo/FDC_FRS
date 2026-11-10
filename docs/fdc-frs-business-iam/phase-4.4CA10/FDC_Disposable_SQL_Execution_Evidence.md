# FDC Disposable SQL Execution Evidence

A10IamSqlTest tạo FDC_A10_TEST_<GUID> riêng cho từng test của các suite legacy. IAM migration 001 chạy nguyên bản. Các guard còn giữ host allowlist, Windows authentication, explicit opt-in và dấu Fdc.A10.Run. Chỉ DB đã được chính fixture tạo, có run marker khớp, mới được DROP. Cleanup không cưỡng bức ngắt phiên.

A9DisposableSql tiếp tục dùng user WITHOUT LOGIN ở phạm vi schema cho các Center database queries. Fixture IAM legacy dùng Windows identity để tạo DB, ghi synthetic rows và quan sát SQL application locks. Không suy ra quyền SQL production từ cấu hình fixture này. Hai bài runtime non-SA/DPAPI giữ SKIP vì startup guard gắn với named Lab và chưa có fixture principal/DPAPI tương ứng cho DB ngẫu nhiên.

Bản sạch chạy tools/Invoke-A10DisposableRegression.ps1 -ConfirmDisposable với hai native artifact đã pin SHA-256. Raw evidence ở tmp/a10g/tmp/phase4-4ca10/delivery. Có build.log, tests.log, TRX, cleanup.log, run.json, scoped-query-plan.sqlplan, query-profile.json và authority-miss-profile.json. Mỗi bài legacy có tên, duration và TRX trong FDC_28_Skip_Regression_Report.md. Các profile synthetic không xác nhận tải 380 trạm.

Migration 007 giữ guard gốc. Harness thay đúng tên catalog trong bản script test sau kiểm tra ownership. Candidate 008–010 chỉ chấp nhận DB prefix A.10 và session/run marker, không chạy từ application startup. Các migration nền 001–006 giữ source guard gốc.

Final source SHA 498f995f0e9aaacf9950a483ff0e2e10974cef41. Bản Git archive sạch chạy 398 PASS / 2 SKIP / 0 FAIL, build 0 warnings / 0 errors, cleanup.log 0. Số bài theo assembly gồm Collector 47, IAM 279, Center 72 PASS / 2 SKIP. Node sạch 11 PASS. Hai runtime tests tiếp tục SKIP vì chưa có owned non-SA/DPAPI profile tương thích guard named Lab.
