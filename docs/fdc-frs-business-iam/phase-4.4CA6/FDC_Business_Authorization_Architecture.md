# FDC Business Authorization Architecture

HumanSessionHandler → BusinessAuthorization → canonical SqlTransactionalAuthorityReader + PermissionEvaluator → SQL scoped projection. Hai EF contexts dùng chung connection và serializable transaction. Pending policy vẫn chặn Center shared. Không có thêm IAM store. Không suy scope từ RoleName. Thao tác ghi và D09 commit gate không mở.
