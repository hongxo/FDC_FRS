# API và SQL

Center dùng GET auth-status → FeatureActivationGate → pending reader/verifier, không ghi activation. Canonical protected read APIs giữ nguyên server scope. SQL session/role/grant/audit regression chạy trên FDC_Center_Lab và FdcIam_DevelopmentLab tách biệt với rollback fixtures. Không thêm hoặc apply migration, không seed admin. Positive SQL issuance, deployment trust, mutation và archive chưa tích hợp.
