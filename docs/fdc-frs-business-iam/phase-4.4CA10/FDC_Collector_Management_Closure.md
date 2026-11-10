# FDC Collector Management Closure

Heartbeat và transfer có optional paged response với total/items, scope lọc trong SQL trước count/paging. Array contract cũ tiếp tục hoạt động. Browser kiểm tra 22 row, page 20+2 và quay lại đúng thứ tự cho cả hai history. Retry attempt được hiển thị, ErrorCode chỉ trả allowlist TIMEOUT/CHECKSUM_MISMATCH/STORAGE_UNAVAILABLE/TRANSFER_INTERRUPTED, unreviewed text bị loại.

CollectorBindingService và UI collector-bindings.html triển khai Request → Review → Approve → Apply cho registry status Bound/Revoked trên synthetic Collector. Bốn PersonReference độc lập, ETag, expiry, expected ConfigurationVersion, station scope và audit/outbox được kiểm tra. Apply giữ nguyên StationId và không nhận certificate/key từ browser. Test audit failure xác nhận SQL rollback và tracker được xóa trước retry cùng scope.

Raw a10-binding.json, a10-binding-browser.json, a10-binding-rollback.json cùng browser business TRX ở tmp/a10g/tmp/phase4-4ca10/delivery. Station move, certificate rebinding/rotation và production registry adapter nằm ngoài slice registry status đã kiểm chứng, cần D07/D04 thiết kế và phê duyệt.
