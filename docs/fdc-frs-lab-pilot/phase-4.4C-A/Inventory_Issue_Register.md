# Issue register

Mapping 4.4B có 17 dòng hợp lệ và 93 mục `MISSING_OR_INVALID_FIELD`. Code cũ phát một mục lỗi mỗi dòng sau khi kiểm tra năm trường bắt buộc. Mã này không xác định trường nào lỗi. 4.4C-A ghi riêng tên trường thiếu hoặc không hợp lệ mà không log giá trị. Một dòng có thể thiếu nhiều trường. Duplicate IP và duplicate function chỉ đánh giá ở các dòng vượt bước kiểm tra đầu vào. Duplicate function cần xem lại ngăn/interface, chưa đồng nghĩa duplicate IED.

Tách hàng đợi thiếu model, firmware, interface, type ambiguity, station/bay, PDF/Excel mismatch và unknown identity để reviewer kiểm tra trên hồ sơ private. Hồ sơ public không liệt kê IP hoặc dữ liệu nguồn.

Kết quả run-02. 93 mục đều thiếu NGĂN. Trong nhóm này, 86 thiếu THIẾT BỊ, 67 thiếu CHỨC NĂNG, 9 thiếu ĐỊA CHỈ IP và 1 thiếu STT. Không phát hiện trường có độ dài/ký tự không hợp lệ trong nhóm 93. Trong 17 dòng hợp lệ về cấu trúc, 9 chưa nhận diện model và cả 17 thiếu firmware/interface đã xác minh.
