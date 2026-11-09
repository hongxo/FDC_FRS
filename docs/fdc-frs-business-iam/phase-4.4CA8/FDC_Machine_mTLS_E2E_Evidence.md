# FDC Machine mTLS E2E Evidence

a8-mtls.json và TRX local ghi CA test, EKU/expiry deny, Human cookie/spoof 401, station mismatch 403, registry inactive 403, authority outage 503, upload 201 và complete 200. SQL có 1 record, 2 files và 1 outbox. Không ghi private key hoặc fingerprint hoạt động thật.

run-04 ngày 2026-11-09 lúc 16:25:06 UTC trên code 29cab609faa2a23c4c3b6f46bfcea65347c37fd5. Fixture ghi 1 disturbance record, 2 file, 1 outbox. HTTP trả 201 khi mở upload và 200 khi hoàn tất. Human 401, sai trạm 403, Collector inactive 403, registry unavailable 503. EKU sai và chứng thư hết hạn bị từ chối ở bước kiểm tra chain trong test.
