# FDC Multi User Cache Isolation Evidence

A10ReadProxy là reverse proxy HTTPS loopback thực, có shared URL cache cho response public có max-age. Nó chuyển tiếp cookie tới upstream có certificate pin và giữ no-store/private response ngoài shared cache. Browser có context A/B độc lập trong cùng instance, xem Lab metadata và Business scoped aggregates A/B.

Trong final slice, mỗi user tải export của chính mình, foreign artifact trả 404 với 0 byte. Sau revoke A, API/exports A bị từ chối và DOM được xóa, B vẫn truy cập được dữ liệu riêng. Cookie switch rồi reload trong context A chỉ hiển thị B. SQL và proxy kiểm tra không có private response được cache. Raw a10-cache-browser.json và a10-proxy-sql.json ở tmp/a10g/tmp/phase4-4ca10/delivery.

Bằng chứng bao phủ proxy của fixture. Chưa phải nghiệm thu reverse proxy/CDN của deployment thật. Không có cache nội dung private phía server trong slice này. Header X-Human-Session-View là hash hiển thị, không dùng làm credential.
