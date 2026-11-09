# Phụ thuộc activation · PROPOSED

| Luồng | Phụ thuộc bắt buộc trước tích hợp đầy đủ |
|---|---|
| Human login/session | D01 → D02/D03/D05/D06 → D09/D10 → scope D12 |
| Machine/OT | D07 + D01 + E04 → D08 và phiếu test riêng |
| Administration | D01/D05 + D04 signed map → D09 atomic apply/revoke → D10 audit |
| Archive/restore | D09/D10 + E06 → D11 independent review |
| Rollout | E01–E08 evidence, D11 review và D12 release scope |

Các mũi tên biểu diễn thứ tự readiness. Những phụ thuộc hai chiều trong ma trận cần review chung. Không kích hoạt từng gate chỉ dựa trên graph.
