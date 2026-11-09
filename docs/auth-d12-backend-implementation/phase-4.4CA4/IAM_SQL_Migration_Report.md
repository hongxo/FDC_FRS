# SQL migration

Không thêm/apply migration. SQL credential reader dùng bảng canonical và yêu cầu transaction đã mở bởi caller. Không thêm IAM store hoặc grant SQL. Credential reader chỉ đọc. Test-only adapter ghi session và audit/outbox trong transaction rollback. Fixture SQL credential rollback trong FdcIam_DevelopmentLab riêng; không seed durable. Production change yêu cầu review schema diff, non-empty dry-run và restore drill.
