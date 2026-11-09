# S01/S04 Authentication và Session

Tái sử dụng AuthenticationService và PasswordProofs. Completion lọc proof trên rejected/unavailable, yêu cầu opaque proof hợp lệ cho MFA challenge, chỉ trả session proof của digest đã đề xuất khi transaction store báo thành công. Isolated tests kiểm tra sai mật khẩu, challenge, session và commit failure. Các store/policy fixture chỉ trong tests.

GET auth-status đọc gate. S01 và S04 có thông báo gate riêng, controls vẫn disabled. Center chưa map credential ingestion, cookie issuance, logout hoặc session mutation. SQL session read/revocation primitives có sẵn. D02/D05/D09/D10 chờ chính sách và trusted transaction implementation.
