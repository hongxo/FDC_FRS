# Security tests

Lượt run-01 build thành công. IAM có một FAIL do inventory test cũ yêu cầu đúng 8 GET routes, trong khi GET grant functions là route thứ 9. Đã cập nhật test để kiểm tra route chính xác, policy Human và method GET. Lượt đầu dừng trước Center/Collector và browser. Giữ raw TRX.

Các test dùng SQL local disposable, fixture rollback-only và browser HTTPS thật. Không cấp login proof qua flow phát hành session. Các test UNIT/INTEGRATION xác minh một phần SoD, CSRF, revoke và authority race, không xác minh policy activation toàn tuyến.
Run-02 build 0 warnings/0 errors. IAM 243 PASS/1 SKIP, Center 49 PASS/1 SKIP, Collector 47 PASS. Node 11 PASS. Browser 9 views, 3 viewports, policy/denial only. Ma trận A2 có 22 PASS và 8 BLOCKED.
