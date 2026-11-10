# Phase4 4CA9 Baseline Report

Mốc A.8 `a0e040179ea533a23d79e79b1b45c4d5c20b85f4`, Draft #20. A.8 ghi 340 PASS / 42 SKIP / 0 FAIL và 16 blocker A.7 còn mở. Ba thay đổi local có trước A.9 được giữ ngoài scope. Baseline mặc định trong sandbox có lỗi Windows PFX key import tại test PilotMachineBoundaryTests. Lượt chạy ngoài sandbox dùng local Windows auth và loopback, không đổi quyền host hay dịch vụ.

Đã đối chiếu báo cáo A.7/A.8, Decision_Records.json, hồ sơ Dynamic RBAC tại workspace gốc, migration IAM 001/Center 002–006, Lab candidate 007, SQL/HTTPS fixtures. Các tài liệu Dynamic RBAC có ở workspace gốc, nằm ngoài clone giao hàng.
