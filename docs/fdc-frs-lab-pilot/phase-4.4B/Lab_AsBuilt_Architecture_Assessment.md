# Kiến trúc as-built theo hồ sơ

Mô hình TBA 110 kV/22 kV và bố trí ngăn lộ được mô tả tại §3.2–3.3, PDF 60–61. Sơ đồ §3.5–3.7, PDF 69–72 cho thấy mạng trạm, Process Bus có MU/SV và đường SCADA qua RTU/Gateway. Đây là dấu vết thiết kế và lắp đặt trong hồ sơ. Các thiết bị còn hoạt động, port patching, thời gian GPS lock, tình trạng switch và firmware hiện hành cần đội Lab xác nhận.

| Nhóm | Thiết bị tham chiếu | Nguồn |
|---|---|---|
| Bảo vệ ưu tiên | Siemens 7SJ85, Schneider P643, ABB REF620 | §5.2, PDF 105, 109 |
| Bảo vệ mở rộng | Toshiba GRL200/GRB200, SEL 311C, GE F650 | §5.2, danh mục ngăn lộ, PDF 105–113 |
| BCU | Toshiba GBU200, Schneider C264, ABB REC670 | §5.2, danh mục ngăn lộ, PDF 105–111 |
| SCADA | ABB RTU560, Gateway, máy tính Gateway, SCADA/HMI | §5.2, PDF 115–117 và phụ lục F PDF 147 |
| Process Bus | MU, switch công nghiệp, máy chủ Process Bus | §3.6 PDF 71, §5.2 PDF 113/117/119 |
| Phụ trợ | GPS, PLC S7-1200, HMI | §5.2, PDF 104–119 và phụ lục F PDF 147–156 |

Đối chiếu REF620 là dòng ABB được người dùng bổ sung. Xác minh mapping riêng từng row trước khi xác định đích. PLC S7-1200 thuộc tự động hóa. BCU, MU, RTU và Gateway chỉ được đưa vào scope File Services nếu có bằng chứng và phê duyệt riêng. §4.8 PDF 85 ghi thử nghiệm hệ thống lịch sử. Phase 4.4B chưa xác minh MMS hoặc COMTRADE trên IED thật.
