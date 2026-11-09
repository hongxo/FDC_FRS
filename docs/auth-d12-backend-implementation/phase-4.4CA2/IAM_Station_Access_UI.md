# S07 Station access

/identity/management.html#grants hiển thị Grant ID, station ID và assignment. GET /api/identity/v1/admin/station-grants/{grantId}/functions yêu cầu Human và StationGrants read authority, bind userId + userRoleId + grantId. Service gọi SQL join hiện hữu trước projection. Scope lẫn subject/assignment/grant cho kết quả rỗng. API không nhận permission từ UI và không tính effective quyền đối tượng. Khi đổi ID, client hủy request cũ và xóa kết quả.

D04/D09 pending nên Lab phiên hợp lệ nhận 503. Gắn/revoke grant, preview quyền hiệu lực và history chưa mở.
