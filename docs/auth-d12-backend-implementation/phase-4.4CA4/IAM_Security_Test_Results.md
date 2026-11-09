# Security results

Run-01 build 0 warnings và 0 errors. Identity 278 PASS/1 SKIP, Center 49 PASS/1 SKIP, Collector 47 PASS. Tổng 374 .NET PASS, 2 SKIP, 0 FAIL. Node 11 PASS. Browser HTTPS loopback có 9 views và 3 viewports. Ma trận A4 có 28 PASS và 17 BLOCKED. Test thêm 14 cryptographic cases và một credential SQL rollback case. Test pass không xác minh signer, activation hoặc production policy thật.

Run-02 lặp lại 374 .NET PASS, 2 SKIP, 0 FAIL và 11 Node PASS sau khi mở rộng credential test thành domain-to-SQL login fixture. 28/45 PASS theo scope ghi trong matrix, 17 BLOCKED.
