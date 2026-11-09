# Kết quả preflight

Ca thiếu trường hoặc tham chiếu trả NOT_READY. Draft đầy đủ hình thức trả BLOCKED_BY_POLICY kể cả có digest hợp lệ. Hết hạn tại đúng end trả EXPIRED. Thay endpoint hoặc version làm đổi fingerprint. Kết quả không dùng cấp quyền. Xem [CA results](CA01_CA30_Test_Results.md).

Run-02 đã chạy draft xuất từ UI qua CLI và nhận BLOCKED_BY_POLICY. Thêm ApprovedBy vào JSON bị deserializer từ chối, không tạo result. Test expiry và hash sử dụng đồng hồ fixture, không có cửa sổ Lab được duyệt.
