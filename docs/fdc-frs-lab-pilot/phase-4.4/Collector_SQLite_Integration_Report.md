# Collector SQLite integration

Migration opt-in `001_PilotTransferQueue` thêm `pilot_*` tables cùng `fdc.db`. Test xác nhận bảng legacy tiếp tục dùng được, lease hết hạn được claim lại, stale worker bị từ chối, retry hữu hạn, disk reserve chặn promotion và ACK phải khớp digest. Rollback kiểm tra toàn bộ pilot tables rỗng trước khi drop. Luồng chạy thật còn cần scheduler theo profile và xử lý vòng đời spool.

Ngày 2026-11-09 (Asia/Bangkok). Source `1ad4f0a`. Evidence và lệnh tái lập xem [Evidence](Evidence.md). Reviewer độc lập chưa được chỉ định.
