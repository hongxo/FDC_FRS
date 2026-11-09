# Baseline 4.4C-A.2

Official remote hongxo/FDC_FRS. Worktree sạch trên nhánh 4.4C-A.1 ở 79298ec, PR #13, dựa trên PR #12 và các nhánh Lab trước. IAM migration 001 và Center 002–006. Candidate 007 của 4.4C-A chưa apply. Đọc các báo cáo gap, API, permission, defect của 4.4C-A.1, 4.4C-A/4.4B, sáu hồ sơ Dynamic RBAC từ workspace nguồn, D01–D10 và tests/migrations hiện tại.

Bằng chứng A.1 báo cáo 339 .NET PASS, 2 SKIP, 10 Node PASS và 18/25 IAM-UI PASS theo phạm vi. Source đã có search/detail S05, catalog/detail S06 và audit detail. Những mục này trong yêu cầu A.2 là các gap lịch sử, đã được đối chiếu với HEAD. MISSING_INPUT gồm release có chữ ký và scope deployment cho policy activation.
