# FDC Export Revoke Race Evidence

LabExportCoordinator dùng queue durable trong SQL cho JSON evidence tối đa 64 KiB. Request gắn actor, session digest, immutable revision, station, deadline và idempotency job ID. Prepare và release là hai transaction riêng, có worker lease và recheck canonical authority. Một worker khác không lấy được lease còn hiệu lực. Download yêu cầu đúng actor và scope hiện tại, kiểm tra digest lần nữa.

Barrier test tạm dừng sau khi staging đã commit, trước release. Revoke tăng GrantVersion và vô hiệu hóa grant bằng cùng candidate gate namespace. Worker tiếp tục bị từ chối, job Quarantined, payload/digest bị xóa và audit/outbox ghi kết quả. Download cả job bị quarantine lẫn job từng Completed trước revoke trả 401 với 0 byte. Raw a10-export-revoke.json và TRX ở tmp/a10g/tmp/phase4-4ca10/delivery.

Queue không có background hosted worker được kích hoạt mặc định. Đây là bounded Lab evidence JSON, chưa triển khai COMTRADE ZIP/bulk export. Test có duplicate lease owner và authority recheck, chưa có multi-process worker, crash/restart lease takeover, cancellation hoặc failure injection cho mọi trạng thái queue. SQL staging tránh partial filesystem artifact trong slice hiện tại.
