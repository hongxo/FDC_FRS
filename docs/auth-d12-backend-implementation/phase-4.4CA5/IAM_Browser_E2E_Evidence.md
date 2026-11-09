# IAM Browser E2E Evidence

Evidence riêng dưới tmp/phase4-4ca5. a5-browser.json chứa run, SHA, UTC, HTTP status và cookie attributes; a5-sql-verification.json chứa trạng thái revoke, challenge và outbox. Hai PNG ghi protected profile và replay deny. Browser gọi fetch thật trên HTTPS loopback, không phải UI S01–S08 đầy đủ. Không lưu password, factor, token hoặc private key.

Run-03 xác minh SHA f1ce6eaf9cbaabe647a8b860a4154a117409e928, UTC 11/09/2026 15:35:42, correlation a5-isolated. HTTP status và metadata proof đã rút gọn trong browser JSON, dữ liệu audit chỉ chứa mã event fixture.
