# Rollback · PROPOSED

Dừng collector job, đóng phiên và đóng egress thông qua OT nếu cần. Giữ nguyên file, queue và ACK chưa xác minh. Đối chiếu SCADA trước/sau và ghi audit. Không xóa IED record hoặc đổi protection. Với migration nháp dùng down script chỉ khi schema trống. Nếu đã có dữ liệu, bảo toàn và chuyển sang quy trình backup/export do DBA phê duyệt.
