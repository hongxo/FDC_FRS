# Database migration

Canonical Center Lab đang dùng EF migrations 002–006 và IAM 001. Candidate 007 trong tools/Fdc.Center.Database/lab-readiness là script additive standalone, chưa đưa vào đường auto-migrate. Tạo schema labprep cho proposal, draft/version và audit draft, cùng rowversion, FK, unique, CHECK và application lock. Không có bảng authority hay effective approval, không cấp DML cho runtime.

Guard yêu cầu transaction và disposable FDC_Center_Lab có 006. Replay dùng version marker. Down từ chối khi có dữ liệu. Trước khi activate cần DBA/owner duyệt station key binding, stored write authority, audit atomicity, rowversion race, rollback và quyền migration trên bản Lab riêng. Candidate chưa được apply.

VERIFIED trong phạm vi kiểm tra cú pháp. SQL Server local đã parse cả up/down bằng SET PARSEONLY ON; hậu kiểm đọc xác nhận NO_LABPREP_SCHEMA. Test SQL hiện hữu chạy trong suite 4.4C-A; replay/rollback/concurrency của 007 vẫn BLOCKED.
