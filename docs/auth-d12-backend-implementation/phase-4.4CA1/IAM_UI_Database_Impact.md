# Database impact

Tái sử dụng IAM schema, SQL session reader và management queries. Không thêm bảng, seed hoặc migration. Hai Lab database giữ riêng. Live HTTPS test dùng session ngắn hạn do fixture tạo trong transaction rollback-only tại FdcIam_DevelopmentLab. SQL Center suite dùng FDC_Center_Lab cho regression, không đổi 007. Không thực hiện setup production.
