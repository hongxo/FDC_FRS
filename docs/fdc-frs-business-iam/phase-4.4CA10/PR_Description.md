# PR Description

Thêm workflow Lab synthetic và UI có SQL session/scope, PersonReference SoD, revision/ETag, permit gate và atomic audit/outbox. Export evidence được staged trong SQL và recheck authority trước release, Collector registry status có quy trình review/approval độc lập. Private download verification failure có audit tương ứng HTTP. UI xóa dữ liệu khi session/authority thay đổi.

Port 23 IAM và hai Center HTTPS legacy tests sang owned disposable DB. Thêm browser expiry/delayed response, HTTPS proxy A/B, signed full-CRL/mTLS, path swap và export/revoke tests. Native simulator chỉ chạy loopback với artifact hash pin.

Giới hạn còn lại gồm hai runtime non-SA/DPAPI cases, OCSP/responder lifecycle, multi-process queue recovery, real certificate/station rebinding và independent acceptance. Default policies tiếp tục pending. Shared Lab activation cần yêu cầu và release riêng.

Validation trên snapshot Git sạch 498f995f0e9aaacf9950a483ff0e2e10974cef41 ghi build 0 warnings/0 errors, .NET 398 PASS/2 SKIP/0 FAIL và Node 11 PASS. Có 26/28 bài SKIP A.9 chạy lại PASS. SQL cleanup kiểm kê 0 owned fixture DB. Ma trận 110 requirements có 88 PASS/20 BLOCKED/2 SKIP. G1/G2 chưa đóng, G3/G4 BLOCKED, Production NO-GO.
