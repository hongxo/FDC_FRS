# FDC COMTRADE Race Security Evidence

Test path-swap sử dụng cùng bytes và digest ở hai phía, thay file bằng symbolic link giữa kiểm tra thuộc tính và open. Assertion yêu cầu handle thực sự mở và boundary từ chối staging. Kết quả cuối ghi trong ma trận. Ca này ở lớp file source, chưa bao gồm audit HTTP tại thời điểm swap. CFG/DAT digest và audit/outbox có trong browser fixture riêng.

Run-05 file symlink fixture FAIL vì không mở được redirect. Run-06 dùng Windows directory junction để kiểm chứng cùng ranh giới reparse, sau attributes check và trước open. Assertion xác nhận handle thực sự mở, không trả stream và spool không có file. A7-33/A8-30 vẫn BLOCKED ở lớp browser/SQL audit.
