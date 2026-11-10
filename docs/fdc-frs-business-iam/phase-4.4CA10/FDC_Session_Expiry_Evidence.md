# FDC Session Expiry Evidence

Browser và server dùng clock được kiểm soát. Có các ca idle expiry, absolute expiry, SecurityVersion tăng, server revoke và MFA age hết hạn. Hai tab Lab và một tab Business đang hiển thị dữ liệu được theo dõi. Expired session trả 401 và xóa DOM. MFA quá tuổi làm scope trả rỗng, UI xóa dữ liệu và backend không trả đối tượng được bảo vệ.

Một response chi tiết đã hoàn thành tại server được giữ lại ở browser route, rồi trả sau khi authority mất hiệu lực. Abort/generation check ngăn render dữ liệu cũ. UI đọc fingerprint một chiều X-Human-Session-View để phát hiện thay phiên, dùng no-store và Vary Cookie, hủy fetch khi pagehide/visibilitychange. Proxy test thực hiện cookie switch rồi reload và chỉ thấy dữ liệu B.

Thời gian phát hiện khi tab đang hiển thị phụ thuộc polling 15 giây. Không tuyên bố xóa DOM tức thời trước khi nhận tín hiệu expiry. Logout và cookie replay được kiểm tra ở luồng authentication riêng. Back/forward cache và logout tại chính tab có request pending chưa có ca browser độc lập, còn trong regression bổ sung.
