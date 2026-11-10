# FDC Lab Browser SQL E2E Evidence

a10-lab.cjs dùng Edge headless, HTTPS loopback và canonical SQL session fixtures. Browser tạo plan từ form rồi chuyển Submit → Review → Approve → Permit → Ready → Apply trên các context actor độc lập. UI chứng minh self-review và reviewer approval bị từ chối, trạm B không thấy plan A. Auditor tải JSON evidence, SQL xác nhận workflow history và audit/outbox.

Sessions được fixture tạo trong DB disposable. Đây là luồng workflow dùng phiên SQL hợp lệ. Login/MFA/cookie issuance/logout được chạy riêng bởi các bài CenterHttpsSqlLabTests legacy và business browser, không giả định đã đăng nhập bằng UI Login ở từng context Lab.

Raw a10-lab-browser-idle/absolute/security/revoked/mfa.json, a10-lab-completed.png và ảnh cleared ở tmp/a10g/tmp/phase4-4ca10/delivery. Mỗi JSON ghi commit, UTC, environment và trace path/status đã loại proof material.
