# Phiếu thử IED · TEMPLATE

Mã LAB-IED-YYYY-NNNN, phiên bản, InventoryId, station/bay, vendor/model/firmware, IED name, IP/cổng/interface và bằng chứng inventory. Khai báo thao tác read-only, thư mục và tên file chính xác. Điền số file, byte/file, số phiên, timeout, retry, poll, thời lượng và tốc độ tối đa do Lab duyệt.

Ghi start/end ISO 8601 và timezone. Điền operator, protection engineer, SCADA supervisor, security reviewer, stop owner, emergency contact, cách liên lạc, cô lập và rollback. Tham chiếu SCADA/time/security/network baseline và SHA-256.

Approval record riêng gồm RequestedBy, ReviewedBy, ApprovedBy, UTC, evidence ref, phiên bản và hash. Execution gồm start/end, correlation ID, manifest, trạng thái. Mọi trường chữ ký/thi hành còn trống trong phase này.
