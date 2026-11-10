# FDC Lab SoD Evidence

A10LabWorkflowTests kiểm tra qua HTTPS thật và SQL thật. Requester self-review/self-approve, reviewer approve và approver apply bị từ chối. Actor ở trạm khác, actor thiếu grant và actor lấy permission/grant từ hai assignment khác nhau đều bị từ chối. Schema IAM có unique PersonReference nên tài khoản alias cùng người bị SQL từ chối ngay khi tạo, không tạo alias runtime để giả lập PASS.

ETag cũ trả 409. Hai approver hợp lệ cạnh tranh cùng revision cho kết quả 200/409 và một approval history. Permit sai IED, sai loại, thiếu hoặc hết hạn khi session vẫn còn hiệu lực trả 403. Grant/security version stale bị Human handler từ chối. Test thêm trigger gây lỗi audit/outbox để xác nhận không có effect/history một phần. Các trigger sẵn có luôn hoạt động.

Raw lab-http.json, lab-rollback.json, a10-lab-concurrency.json và TRX tại tmp/a10g/tmp/phase4-4ca10/delivery. HTTP trace và history chứa actor fixture, revision, state, denial status và correlation. Denied transition audit được nối với outbox bằng correlation. FAILED và ABORTED terminal có bài kiểm thử riêng.
