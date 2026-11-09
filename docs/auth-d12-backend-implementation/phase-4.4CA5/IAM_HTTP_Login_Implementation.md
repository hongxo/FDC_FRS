# IAM HTTP Login Implementation

Candidate Minimal API nhận HTTPS JSON giới hạn 8 KiB sau exact-origin và antiforgery. Dùng AuthenticationService và SQL credential reader qua test-only transaction adapter. Password đúng chỉ được trả challenge, không phát hành cookie trên route login. Unknown user và password sai cùng HTTP 401. Production host chưa map route; SQL throttling/lockout cần release.
