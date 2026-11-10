# FDC SQL Regression Evidence

Dữ liệu synthetic, SQL Server local, một DB do harness tạo cho mỗi Center Foundation/authority test. Lệnh dotnet test Fdc.Identity.slnx --no-build --logger trx --results-directory <evidence>/trx với opt-in A.9 và authority profile. Quy trình lưu build.log, tests.log, TRX, query-profile.json, scoped-query-plan.sqlplan, authority-miss-profile.json và cleanup.log. Lượt run-01 có 12 FAIL do script tổng hợp làm mất batch boundary ở ManifestSnapshotJson. run-02 chạy 12/12 PASS sau khi dùng migration operations và giữ batch nguyên bản. Cả hai lần kiểm kê còn 0 DB A.9.

Số liệu profile chỉ mô tả dữ liệu và tải loopback của fixture, không suy rộng sang 380 trạm. Báo cáo SKIP ghi từng tên test, expected/actual, duration và TRX. Trường hợp rollback/concurrency từ SQL assertions được giữ nguyên.

Run-06 UTC 2026-11-10 01:40, commit d5562798b194fd003a28e46a0b355f9035273087. Collector 46 PASS/1 SKIP, IAM 256 PASS/23 SKIP, Center 53 PASS/4 SKIP, 0 FAIL. SQL master kiểm kê 0 FDC_A9_TEST và FdcA7 fixture sau run. 15 bài từng SKIP gồm 12 Center database/profile và 3 transactional authority/profile.
