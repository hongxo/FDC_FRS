# API

GET /api/lab/v1/readiness chỉ có trên Human lane của Center IAM, yêu cầu Fdc.Human. Khi được xác thực trả 503 với LAB_READINESS_STATION_POLICY_PENDING và correlation ID, không đọc record, không nhận draft. Legacy và machine lane không map route này.

PROPOSED các nhóm inventory, verification, permits, scope, network, SCADA/time, readiness và evidence. Endpoint ghi chỉ mở khi có catalog, trusted object-to-station resolver, CSRF, If-Match, authorization at commit và audit/outbox đã duyệt. Chưa triển khai mutation hoặc approval endpoint.
