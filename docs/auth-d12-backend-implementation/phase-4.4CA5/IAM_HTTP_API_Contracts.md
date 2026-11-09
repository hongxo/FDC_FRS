# IAM HTTP API Contracts

Candidate /api/identity/v1/auth. GET /csrf cấp request token. POST /login trả 202 challenge hoặc generic 401. POST /mfa/verify trả 204 và secure cookie khi hoàn tất, 401 khi proof/factor sai. POST /logout yêu cầu Human session và CSRF, trả 204 sau revoke. JSON invalid 400, guard deny 403, authority unavailable 503. Chỉ map trong fixture, không có recovery hoặc issuance bypass.
