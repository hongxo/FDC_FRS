# Phase4 4CA10 Baseline Report

Mốc nguồn A.9 d5562798b194fd003a28e46a0b355f9035273087 là ancestor của nhánh A.10. Nhánh bắt đầu từ 6d0f2aa, xếp trên Draft PR #21. Raw TRX A.9 run-06 được đọc trực tiếp và xác nhận 355 Passed / 28 NotExecuted / 0 Failed. cleanup.log ghi 0 database fixture. Đã đối chiếu các báo cáo A.9, 42-skip report, ma trận 90 ca, blocker matrix, Lab proposal và Decision_Records.json.

Ba thay đổi có trước A.10 nằm ngoài commit gồm management.mjs, management-errors.test.mjs và IAM_Role_Test_Assignment.md. Bản xác nhận được xuất bằng git archive từ commit 498f995f0e9aaacf9950a483ff0e2e10974cef41. Bản restore đầu trong sandbox dùng package cache khác user và lỗi NETSDK1064 khi build ngoài sandbox. Restore lại bằng cache local của cùng Windows user khắc phục lỗi môi trường đó.

ENV-1/2/3 áp dụng cho synthetic fixtures, database owned riêng và HTTPS loopback. ENV-4 chỉ có hồ sơ readiness. ENV-5 tiếp tục NO-GO.

Giới hạn baseline. A.9 được xác minh bằng raw run-06 lưu sẵn. Phase này không tạo một fresh run A.9 mới trước khi sửa code. Fresh build/test xác nhận được chạy trên snapshot A.10 đã commit.
