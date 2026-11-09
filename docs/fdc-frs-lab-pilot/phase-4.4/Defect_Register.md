# Defect register

| Ref | Hiện tượng | Kết quả |
|---|---|---|
| DEV-01 | Restore ban đầu bị giới hạn truy cập NuGet | Restore trong phiên được cấp quyền đã qua |
| DEV-02 | Build lỗi namespace `Identity` và analyzer assertion | Sửa reference/assertion và build lại đạt |
| DEV-03 | Chữ trong bảng mobile bị ngắt quá hẹp | Bảng rộng cuộn ngang, browser test cuối đạt |
| SEC-01 | Lượt trích xuất IP ban đầu hiện một ô ghi chú chứa thông tin đăng nhập trong output công cụ | Loại cột ghi chú khỏi bản trích lưu lại. Không sử dụng thông tin đó để đăng nhập. Scan bản bàn giao theo dữ liệu inventory |
| LAB-01 | Nhãn Siemens trùng trong inventory | OPEN, cần đối chiếu thiết bị và endpoint |
| LAB-02 | Thiếu profile vận hành và bằng chứng đọc trên IED thật | OPEN, chặn real pilot |
| LAB-03 | D01/D07 và dashboard có IAM còn pending | OPEN, chặn runtime integration |


Ngày 2026-11-09 (Asia/Bangkok). Source `1ad4f0a`. Evidence và lệnh tái lập xem [Evidence](Evidence.md). Reviewer độc lập chưa được chỉ định.
