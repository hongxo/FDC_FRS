# FDC COMTRADE Secure Download Design

File ID → SQL RecordFile join RecordId/StationId → verified metadata, exact manifest key → private adapter. Adapter từ chối traversal, ADS và reparse components, copy bằng buffer 64 KiB vào private delete-on-close staging, kiểm size/SHA-256, trả stream chỉ sau audit/outbox commit. Giới hạn 16 MiB, không Range, no-store, không public paths. Root/staging cần ACL không cho untrusted writers, nằm ngoài web root. Audit outcome prepared khác started/completed/aborted; các event sau chưa có.
