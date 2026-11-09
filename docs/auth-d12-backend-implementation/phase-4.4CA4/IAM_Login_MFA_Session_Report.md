# S01–S04

SQL credential reader dùng IAM User/PasswordCredential và caller transaction, kiểm writable primary, chỉ đọc account status và phiên bản. Test rollback-only dùng fixture và PasswordProofs thật. AuthenticationService từ A.3 kiểm tra sai password, challenge proof, session digest và lỗi commit bằng isolated transaction double. Chưa có SQL writer cho issuance, throttling, lockout, rotation hoặc logout.

S01/S02 disabled, S03/S04 status only. Không có factor/recovery backend hiệu lực. D02/D03/D05/D06/D09/D10 phải chốt policy, origin/CSRF, rate limits, cookie, Data Protection, replay và audit trước activation.
