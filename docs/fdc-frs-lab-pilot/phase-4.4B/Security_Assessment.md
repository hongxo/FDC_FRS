# Đánh giá bảo mật

Reuse read-only adapter và deny-by-default inventory gate trước khi mở adapter. Inventory được xác minh chỉ là điều kiện đầu vào. Phê duyệt chạy cần được authority độc lập xác thực và gắn đúng thiết bị, thư mục, giới hạn và cửa sổ. Runtime OT chỉ mở sau khi policy vận hành được duyệt.

Human cookie và Collector mTLS có route, listener, principal và scope riêng. Chạy SQL local dùng shared-memory và disposable test guard. Harness so SHA-256 binary và lưu evidence riêng. Pinned fixture và chứng thư tự cấp cho test chỉ chứng minh path cô lập. Cần kiểm tra listener isolation, cert revocation, proxy trust, direct backend bypass, audit và rollback theo topology thật. Không hạ tiêu chí IAM/RBAC hoặc mở P0-IAM.
