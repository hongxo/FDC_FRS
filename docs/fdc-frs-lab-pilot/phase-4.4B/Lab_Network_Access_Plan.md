# Kế hoạch truy cập mạng Lab

Logical flow đề xuất là IED Station Bus → Collector Lab → machine listener riêng → SQL/file store riêng. Dashboard human đi qua HTTPS riêng. Đường RTU/Gateway → SCADA tiếp tục độc lập theo sơ đồ báo cáo §3.7, PDF 72. Sơ đồ Process Bus §3.6, PDF 71 nằm ngoài đường Collector.

| Nguồn → đích | Kết nối vật lý đề xuất và dịch vụ | Điều kiện |
|---|---|---|
| Collector → một IED được duyệt | Cổng Station Bus hiện hữu do OT chỉ định → đúng IP/cổng MMS của IED | Allowlist theo thiết bị, một association lúc đầu, read-only, thư mục và giới hạn ký duyệt |
| Collector → Center machine | NIC/segment egress do OT chỉ định → ingestion mTLS | Station binding, pin/chain theo hồ sơ, D01/D07 và kiểm tra bypass |
| Human workstation → Center human | Mạng quản trị → HTTPS human | Cookie/RBAC, không đi machine ingress |
| Center → SQL và file store | Private services và principal riêng | Center duy nhất có quyền ghi |
| Collector → Process Bus/SCADA | Không cấp đường thử | Giữ SV/GOOSE và đường điều khiển độc lập |

Chưa có port vật lý/VLAN/route được duyệt cho phiên này. Không quét subnet. Stop ngay khi có cảnh báo SCADA, thay đổi chất lượng/heartbeat ngoài ngưỡng, đọc ngoài allowlist, time drift hoặc lỗi tải. Hủy tác vụ Collector, đóng association, giữ bằng chứng và báo người giám sát. Chỉ OT thực hiện cô lập cổng khi cần. Chỉ tiếp tục sau quyết định mới.
