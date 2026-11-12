# Baseline hiệu năng

Run offline đo elapsed của fixture parser/acquisition trong run.json. Native simulator đo read và digest cho fixture, không đại diện WAN/IED. Không ngoại suy P95/P99 hay năng lực 380 trạm từ một mẫu.

Khi Lab duyệt, chạy theo từng model, một association, ghi directory latency, size/rate, retry/ACK, CPU/RAM/disk/queue và đồng hồ UTC. Người giám sát đối chiếu telemetry SCADA trước/trong/sau và giới hạn theo thiết bị. Lưu số mẫu, P50/P95/P99 khi có đủ mẫu, errors và trường hợp bị dừng. Hiệu năng IED thật BLOCKED.

Run local-02, một mẫu mỗi loại. Offline acquisition 89,75 ms. Native loopback connect 19,36 ms, listing 0,69 ms, tải DAT 30 byte trong 6,98 ms và MMS-to-SQLite-ready 73,98 ms. Không có mẫu IED thật.
