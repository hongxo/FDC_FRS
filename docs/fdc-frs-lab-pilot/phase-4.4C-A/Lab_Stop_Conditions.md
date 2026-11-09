# Điều kiện dừng · PROPOSED

Dừng nếu mất heartbeat, quality giảm, alarm ngoài kế hoạch, mất time lock, vượt ngưỡng đã ký, thay scope, identity mismatch, expiry/revoke, lỗi checksum, hết spool hoặc bất thường network. Stop owner yêu cầu hủy tác vụ và đóng association. OT xử lý cô lập nếu cần. Giữ log, UTC và nguyên nhân. Chỉ tiếp tục sau review scope mới.
