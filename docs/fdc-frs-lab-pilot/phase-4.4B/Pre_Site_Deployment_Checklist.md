# Điều kiện thử nghiệm tại trạm

- [ ] G3–G5 có bằng chứng theo thiết bị/firmware, COMTRADE digest, ACK, retry và exception.
- [ ] Đối chiếu SCL/as-built, cổng Station Bus và đường bảo vệ/điều khiển. Có network change ticket nếu cần.
- [ ] D01/D07 và IAM liên quan có quyết định, threat model và các test boundary đạt.
- [ ] Có người trực SCADA, threshold, nguồn đồng hồ, window và emergency stop.
- [ ] Có backup/restore kiểm chứng, rollback quy mô một Collector và giữ queue để retry.
- [ ] Chọn một trạm/nhóm IED nhỏ với owner trạm. Chạy read-only và mở rộng theo đánh giá độc lập.
- [ ] Lab acceptance và phương án pilot tại trạm được ký. Trạng thái G6 BLOCKED.
