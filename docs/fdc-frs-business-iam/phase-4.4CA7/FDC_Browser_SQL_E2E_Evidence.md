# FDC Browser SQL E2E Evidence

a7-browser.json và ảnh a7-stations.png/a7-download-link.png giữ local. Một session operator đi từ login/MFA tới Dashboard, Station/IED/Record/Collector, private DAT, SQL revoke, logout và cookie replay denial. Audit/outbox kiểm trong SQL. Evidence không ghi password/cookie/token/private key.

Run-06, SHA 14e9d3c0457e4fc764397f091f17241b990ce70c, UTC 11/09/2026 16:15:41. Browser nhận 15 byte DAT, digest đúng, B và revoke đều 0 byte. SQL correlation trùng HTTP header, outbox linked, grant và session revoked. Role matrix JSON xác minh sáu identity còn lại. 338 .NET PASS, 43 SKIP, 0 FAIL, 12 Node PASS (gồm một test local có trước). Suites cần shared SQL bị bỏ qua; không tính là PASS.
