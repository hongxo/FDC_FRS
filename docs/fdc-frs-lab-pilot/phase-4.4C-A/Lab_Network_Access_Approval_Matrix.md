# Network approval · PROPOSED

| Tuyến | Giới hạn | Owner / bằng chứng |
|---|---|---|
| Collector Lab → một IED | Station Bus, endpoint chính xác, read-only file service | OT xác nhận source/interface/port/allowlist |
| Collector → Center machine | Listener mTLS riêng, station binding, expiry/revoke | D01/D07, PKI/Security |
| Human → Center Human | HTTPS/cookie và station scope | D01/D04, IAM |
| SCADA/RTU/Process Bus | Duy trì phân đoạn, không cấp tuyến thử | SCADA/OT xác nhận isolation và stop |

Ghi access log, reviewer, expiry và emergency isolation. Tất cả phê duyệt còn PENDING.
