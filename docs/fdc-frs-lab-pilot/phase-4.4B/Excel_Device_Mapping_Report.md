# Đối chiếu workbook và thiết bị

Công cụ `acceptance-map` dùng parser Excel 4.4A để xuất JSON riêng, giữ nguyên IPv4 được parser chấp nhận và số dòng trong sheet. Các cột gồm ExcelRow, Station, Bay, Function, Vendor, Model, DeviceType, IpAddress, IedName, Firmware, Protocol, SourceDocument, Confidence, VerifiedBy, VerifiedAt và ApprovedForTesting. Function giữ mã/giá trị nhập từ cột CHỨC NĂNG. Tất cả dòng preview có trạng thái UNVERIFIED_IMPORT và chưa được phép thử.

Dùng workbook gốc, sheet IP, header dòng 2. Soát lỗi thiếu IP, IP trùng và alias chức năng trong danh sách issue. Một nhãn chức năng trùng cần người quản lý xác định theo dòng và interface. Khi hai IP có thể thuộc cùng chassis, dùng serial/MAC/IED name và sơ đồ port được cấp phép để phân biệt. Không dùng suy luận theo tên để gán firmware, model hoặc ngăn còn thiếu. Kiểm tra model/loại/ngăn với nhãn tủ và revision bản vẽ, giữ từng record nguồn để truy vết. Xem `mapping.private.json` và `inventory.html` trong thư mục run bị Git ignore. Không nhập batch vào kho runtime.

Run local-02 xuất 17 dòng hợp lệ và 93 mục issue. Không có phê duyệt hoặc người xác minh trong preview.
