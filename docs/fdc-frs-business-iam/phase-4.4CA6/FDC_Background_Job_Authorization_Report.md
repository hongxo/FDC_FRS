# FDC Background Job Authorization Report

InboxWorker legacy và ProcessingOutbox là system paths. Chưa mở export hoặc user job. Khi triển khai phải lưu actor/operation/object/authority version, reauthorize trước đọc và xuất, thu hồi link/cache sau revoke, không dùng account khác. D09/D10 cần test race và audit.
