# FDC Browser E2E Evidence

Browser login/MFA → Dashboard/Station/IED/Collector → CFG/DAT → revoke → logout/replay. Chạy với SQL disposable riêng. Evidence local a7-browser.json, a7-sql-verification.json, a8-collector-mobile.png. Giữ tên artifact a7 vì tái dùng harness. Không ghi session/password/factor.

run-04 ngày 2026-11-09 lúc 16:25:11 UTC trên code 29cab609faa2a23c4c3b6f46bfcea65347c37fd5. Browser kiểm tra digest CFG/DAT, tải cross-station trả 0 byte và replay sau logout trả 401. Hai correlation tải được đối chiếu với SQL audit/outbox.
