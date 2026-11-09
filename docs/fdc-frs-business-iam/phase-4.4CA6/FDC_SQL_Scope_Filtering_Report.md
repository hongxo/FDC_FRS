# FDC SQL Scope Filtering Report

SQL authority snapshot và truy vấn data dùng chung serializable transaction. Ứng viên scope lấy từ StationGrant. PermissionEvaluator đánh giá same-assignment, version, grant/function, MFA và mandatory deny. Hierarchy chỉ trực tiếp, không suy ancestor. Scope SQL trước projection, count và paging. Recheck thời hạn trước commit. Không cache. Chưa có benchmark đa trạm/plan production hoặc HA acceptance.
