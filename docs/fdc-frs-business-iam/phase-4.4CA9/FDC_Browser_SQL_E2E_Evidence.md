# FDC Browser SQL E2E Evidence

HTTPS loopback, Edge headless, SQL synthetic. a7-browser.json chứa steps/correlation và screenshot nội bộ; a7-sql-verification.json đối chiếu hai tải file với audit/outbox. a8-mtls.json là bằng chứng riêng cho machine. Không đưa raw screenshots, cookie hoặc secrets vào Git. Luồng Lab browser-to-SQL, thời điểm session expiry và multi-user cache còn thiếu.

Run-06 commit d5562798b194fd003a28e46a0b355f9035273087. Đã xác minh IED pagination 20+1, 4 nút, thứ tự và không xuất hiện đối tượng B. Chuỗi Trạm–IED–Collector–COMTRADE được click qua tới link DAT. Navigation SQL trả rỗng sau khi grant bị thu hồi. Thử browser tại 390px PASS sau sửa wrap ở run-04.
