# FDC Collector Management Report

GET /api/business/v1/collectors/{id}/heartbeat và /transfers kiểm policy riêng, SQL station scope trước phân trang; transfer join RecordId + StationId. UI hiển thị timeline và lịch sử truyền. Browser đọc A và 404 cho B. Projection chỉ có UTC/status/outcome/attempt và record ID, không raw error hoặc credential. CATALOG_GAP cho mappings live.
