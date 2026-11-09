# Lab test matrix

| ID | Test | Expected | Actual evidence | Real Lab |
|---|---|---|---|---|
| LAB-01 | ABB connectivity | Xác nhận abb connectivity trong phạm vi đã duyệt | REF620 chưa kết nối | BLOCKED |
| LAB-02 | Siemens connectivity | Xác nhận siemens connectivity trong phạm vi đã duyệt | 7SJ85 chưa kết nối | BLOCKED |
| LAB-03 | Schneider connectivity | Xác nhận schneider connectivity trong phạm vi đã duyệt | P643 chưa kết nối | BLOCKED |
| LAB-04 | MMS capability discovery | Xác nhận mms capability discovery trong phạm vi đã duyệt | SIMULATED probe | BLOCKED |
| LAB-05 | File directory listing | Xác nhận file directory listing trong phạm vi đã duyệt | SIMULATED listing | BLOCKED |
| LAB-06 | COMTRADE CFG/DAT acquisition | Xác nhận comtrade cfg/dat acquisition trong phạm vi đã duyệt | SIMULATED byte acquisition | BLOCKED |
| LAB-07 | Checksum verification | Xác nhận checksum verification trong phạm vi đã duyệt | Synthetic size/hash and original bytes | BLOCKED |
| LAB-08 | Duplicate detection | Xác nhận duplicate detection trong phạm vi đã duyệt | SQL replay and queue dedupe | BLOCKED |
| LAB-09 | Interrupted transfer/retry | Xác nhận interrupted transfer/retry trong phạm vi đã duyệt | Partial-file rejection and finite retry | BLOCKED |
| LAB-10 | Collector restart recovery | Xác nhận collector restart recovery trong phạm vi đã duyệt | SQLite lease reclaim and staged Center restart | BLOCKED |
| LAB-11 | Center ingestion | Xác nhận center ingestion trong phạm vi đã duyệt | Pinned loopback mTLS and ACK | BLOCKED |
| LAB-12 | SQL metadata integrity | Xác nhận sql metadata integrity trong phạm vi đã duyệt | SQL record, files, outbox and cleanup | BLOCKED |
| LAB-13 | COMTRADE parsing | Xác nhận comtrade parsing trong phạm vi đã duyệt | OFFLINE_FIXTURE parser suite | BLOCKED |
| LAB-14 | Waveform preview | Xác nhận waveform preview trong phạm vi đã duyệt | OFFLINE_FIXTURE browser SVG | BLOCKED |
| LAB-15 | Station isolation | Xác nhận station isolation trong phạm vi đã duyệt | SQL binding and scoped regression | BLOCKED |
| LAB-16 | Human/Machine security boundary | Xác nhận human/machine security boundary trong phạm vi đã duyệt | Loopback cookie and forged-header denial | BLOCKED |
| LAB-17 | SCADA coexistence | Xác nhận scada coexistence trong phạm vi đã duyệt | Chưa đo trên SCADA | BLOCKED |
| LAB-18 | End-to-end acquisition | Xác nhận end-to-end acquisition trong phạm vi đã duyệt | Các chặng synthetic đã chạy riêng | BLOCKED |

Thiết bị, firmware và endpoint cụ thể xem inventory. Với từng bài thật, lưu ngày giờ, model/firmware, profile, hash evidence và reviewer. Ba hãng chưa có kết quả kết nối thật.

Ngày 2026-11-09 (Asia/Bangkok). Source `1ad4f0a`. Evidence và lệnh tái lập xem [Evidence](Evidence.md). Reviewer độc lập chưa được chỉ định.
