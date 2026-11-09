# Kiến trúc giao diện IAM

Tái sử dụng HTML/CSS/ES modules và theme Center. Trang /identity/ là lối vào công khai chỉ có thông báo chính sách. Trang /identity/management.html được nhúng vào assembly và chỉ phục vụ trên Human HTTPS lane với Fdc.Human. Các file script/CSS không chứa dữ liệu IAM. Legacy Center không phục vụ thư mục identity.

Menu quản trị lấy các màn hình được phép từ backend, dùng cùng authority với API đọc. D04/D05 pending dẫn đến danh sách rỗng. UI ẩn menu và xóa dữ liệu khi rời trang hoặc mất phiên. API luôn kiểm tra lại authority. Không cấp quyền theo tên role hoặc từ state trình duyệt. Operational menu hiện trạng thái chờ mapping thay vì dẫn sang tuyến legacy.
