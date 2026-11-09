# Baseline SCADA coexistence

Sơ đồ lịch sử §3.7 PDF 72 mô tả RTU/Gateway và SCADA. Đo baseline hiện tại bằng telemetry hiện có của người quản trị SCADA. Người giám sát ghi timestamp UTC và nguồn, trước/trong/sau phiên đo cùng cửa sổ so sánh.

| Chỉ số | Thu thập | Điều kiện dừng |
|---|---|---|
| RTU/Gateway heartbeat, link/error counters | Export read-only từ công cụ SCADA/OT hiện hữu | Mất heartbeat hoặc vượt ngưỡng đã ký |
| Point quality, event/alarms và latency | So sánh snapshot và histogram theo cửa sổ | Alarm bất thường hoặc chất lượng giảm |
| IEC 61850 association, switch load | Telemetry đã được cấp quyền | Mất phiên ngoài phạm vi hoặc tải vượt ngưỡng |
| Time source và offset | NTP/PTP/GPS hiện hữu, đọc trạng thái | Drift/lock loss vượt giới hạn đã ký |

Actual BLOCKED. Chưa có cửa sổ, owner, ngưỡng hoặc telemetry hiện trường. Không phát lệnh SCADA, không tạo GOOSE/SV, không cấu hình mirror port.
