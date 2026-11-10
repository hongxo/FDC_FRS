# FDC Security Defect Register

| Defect | Owner | State | Evidence |
|---|---|---|---|
| A9-D01 Migration 005 batch compilation | DBA/QA | Resolved in harness | run-01 FAIL, run-02 PASS; source guards giữ nguyên |
| A9-D02 Browser overflow do mã COMTRADE dài | Center/QA | Retest in final run | run-04 browser fail ở 390px; thêm wrap |
| A9-D03 Lab workflow/SoD và export worker | IAM/OT/DBA | Open | Chưa có HTTP+SQL E2E |
| A9-D04 CRL/OCSP và cache A/B | PKI/QA | Open | Thiếu fixture tương ứng |
| A9-D05 Session expiry với DOM đang hiển thị | Center/QA | Open | Polling đã triển khai, chưa có time-based E2E |
| A9-D06 IAM SQL suite legacy | IAM/DBA | Open | Chưa port named DB/DPAPI fixtures |
| A9-D07 PFX trong sandbox | QA | Environment | Baseline lỗi UserKeySet. Lượt local ngoài sandbox dùng ngữ cảnh Windows phù hợp. |

run-06 xác minh lại A9-D02, không còn overflow. Setup link lần đầu thất bại ở run-05; thay bằng Windows junction ở run-06, thực sự mở redirect và file source từ chối. Giữ gap HTTP swap audit.
