# Năng lực IED

| Model | Vai trò theo hồ sơ | MMS/directory/read/COMTRADE | Hành động |
|---|---|---|---|
| Siemens 7SJ85 | Bảo vệ | BLOCKED, firmware chưa xác minh | Chốt target/interface, thử đầu tiên |
| ABB REF620 | Bảo vệ | BLOCKED, firmware chưa xác minh | Sau Siemens, phê duyệt riêng |
| Schneider P643 | Bảo vệ | BLOCKED, firmware chưa xác minh | Sau ABB, phê duyệt riêng |
| Toshiba GRL200, GRB200, SEL 311C, GE F650 | Bảo vệ | NOT_TESTED | Đợt mở rộng theo model/firmware |
| ABB REC670, Schneider C264, Toshiba GBU200 | BCU | NOT_TESTED | Xác minh chức năng và hồ sơ vendor trước khi đưa vào scope |
| RTU560, Gateway, MU, PLC, HMI, switch, REGDA | Hạ tầng/điều khiển/phụ trợ | Ngoài đợt File Services đầu tiên | Chỉ khảo sát theo nhiệm vụ riêng |

Nguồn model và phân loại xem [as-built theo hồ sơ](Lab_AsBuilt_Architecture_Assessment.md). Đánh dấu UNSUPPORTED chỉ khi có tài liệu đúng firmware hoặc phép thử được duyệt chứng minh giới hạn. Simulator cung cấp kết quả cho adapter độc lập với các dòng trên.
