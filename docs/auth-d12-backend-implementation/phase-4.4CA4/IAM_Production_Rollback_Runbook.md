# Production rollback · PROPOSED

Rollback owner chưa chỉ định. Nếu lỗi security, failover bất nhất, outbox hụt, sai station scope hoặc vượt ngưỡng đã ký, dừng wave và freeze writers. Bảo toàn DB log, artifacts, audit, evidence và queue. Thu hồi activation qua trusted authority khi đã được duyệt. Không tự bỏ qua policy.

Quay app về artifact tương thích schema. Với restore, DBA chọn point-in-time và đối chiếu metadata/file/outbox, cập nhật restore epoch theo chính sách rồi xác minh session invalidation, replay và reconciliation. Schema có dữ liệu phải giữ hoặc restore theo change plan; tránh down destructive. Test backup restore và smoke lại trước quyết định mở traffic.
