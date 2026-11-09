# Isolated evidence

.NET test ký/xác minh ECDSA với khóa sinh trong bộ nhớ, rollback-only SQL credential read, and HTTPS loopback với session fixture. Browser xác minh các đường từ chối và UI responsive. Không có live credential issuance, login/MFA, positive business-data E2E hoặc approval workflow. Raw TRX/ảnh giữ riêng trong tmp/phase4-4ca4.

Run-02 kết hợp AuthenticationService, password hash thật, SQL reader thật và test-only transaction adapter. Password sai không tạo session. Password đúng ghi session digest và audit/outbox trong transaction, sau đó rollback và kiểm không còn rows. Ca này không dùng HTTP login/MFA hoặc reviewed SQL writer.
