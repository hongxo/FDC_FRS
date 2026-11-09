# FDC CA6 E2E Evidence

SQL A.6 kiểm đếm đúng một trạm, Station/IED allow/deny, sửa station filter, Collector/COMTRADE metadata và revoke. Raw TRX dưới tmp/phase4-4ca6. Chưa có browser login kết hợp A.6, download bytes, Lab view hoặc proof production. Machine mTLS regression tách riêng.

Run-04, code d78fcc6. 376 .NET PASS, 2 SKIP, 0 FAIL. Node 12 PASS gồm một test local từ trước phase, chưa commit. Ma trận 24 PASS, 26 BLOCKED. A.5 browser và machine mTLS regression chạy lại, không suy thành A.6 browser E2E. Lượt đầu cleanup thất bại do TR_Role_NoDelete. Trong một transaction phục hồi chỉ với fixture, tạm tắt rồi bật lại trigger và xác minh is_disabled=0 cùng 0 users/0 stations. Test sau rollback. Lượt hai lỗi EF projection, đã sửa.
