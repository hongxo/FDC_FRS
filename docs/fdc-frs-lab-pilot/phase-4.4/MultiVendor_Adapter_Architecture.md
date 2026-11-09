# Multi-vendor adapter

`IIedFileAcquisitionAdapter` tách khảo sát capability, directory, metadata và download. `MmsReadOnlyAdapter` dùng pin native SHA-256 và profile theo endpoint. Capability được lưu cùng nguồn evidence, model và firmware ở dạng inventory claim. Vendor thiếu File Services hoặc quyền đọc giữ trạng thái riêng. Một phiên hoạt động theo endpoint trong mỗi tiến trình.

Ngày 2026-11-09 (Asia/Bangkok). Source `1ad4f0a`. Evidence và lệnh tái lập xem [Evidence](Evidence.md). Reviewer độc lập chưa được chỉ định.
