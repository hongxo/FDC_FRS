# Phase4 4CA10 Final Assessment

Kết quả kỹ thuật được gắn với commit 498f995f0e9aaacf9950a483ff0e2e10974cef41 và raw run trong tmp/a10g/tmp/phase4-4ca10/delivery. Source gồm Lab workflow/UI, SQL regression harness, session expiry, HTTP file verification denial audit, full-CRL candidate, bounded export queue và synthetic Collector registry workflow.

CODE_IMPLEMENTED và TEST_EXECUTED được đánh giá theo từng ca. ISOLATED_E2E_VERIFIED chỉ áp dụng cho slice có evidence. INDEPENDENTLY_REVIEWED/POLICY_APPROVED/LAB_AUTHORIZED/LAB_ACTIVATED chưa đạt. PRODUCTION_READY chưa đạt.

G1/G2 còn giới hạn runtime/DPAPI, OCSP và các failure/recovery cases chưa chạy. G3/G4 BLOCKED. G5 NO-GO. Ban quản lý dự án cần bố trí reviewer độc lập. OT/Security cần phê duyệt scope, permit và D01/D04/D07/D08. DBA và audit owner cần disposition D09/D10 cùng E dependencies. Không thay PENDING bằng fixture approval.

## Trả lời các điều kiện nghiệm thu

1. A.7 xác minh thêm 10/11 hạng mục trong fixture. A.8 xác minh 17/20 hạng mục còn mở theo slice. A.9 có 29/40 BLOCKED và 4/4 SKIP requirements chuyển PASS trong slice. Các con số trùng yêu cầu giữa pha nên không cộng.
2. Có 26/28 bài SKIP A.9 chạy lại PASS. Hai bài named runtime/non-SA/DPAPI còn SKIP.
3. Snapshot sạch có 398 .NET PASS / 2 SKIP / 0 FAIL. Node management regression của source đã commit có 11 PASS / 0 FAIL. Các lượt trước có lỗi fixture và môi trường, được giữ nguyên raw evidence và ghi trong defect register.
4. Lab workflow đã chạy browser-to-SQL bằng session fixture canonical trên HTTPS loopback.
5. SoD đã kiểm chứng SQL/HTTP/browser. Alias cùng PersonReference bị unique constraint từ chối. Hai approver cạnh tranh cho một approval history và status 200/409.
6. Idle/absolute/security-version/revoke/MFA expiry xóa DOM ở Lab và Business. Ba tab được quan sát. Response trễ bị bỏ qua. Back-forward cache và pending logout riêng vẫn cần test bổ sung.
7. HTTP junction swap trả 404 với 0 byte, có denial audit/outbox khớp correlation.
8. Full CRL có chữ ký được kiểm chứng, revoked/unavailable bị mTLS từ chối. OCSP và responder HTTP outage chưa có implementation/evidence.
9. Export/revoke có barrier sau staging commit, recheck, quarantine và HTTP 401 với 0 byte artifact. Slice giới hạn JSON evidence 64 KiB. Multi-process recovery và đầy đủ failure matrix chưa được kiểm chứng.
10. Shared HTTPS proxy fixture xác minh A/B, dashboard/station metadata, private exports, revoke và cookie switch. Chưa nghiệm thu reverse proxy deployment.
11. Collector có history paging 20+2, scoped total, allowlisted retry error và request/review/approve/apply cho registry status. Station/certificate rebinding chưa triển khai.
12. Independent human review chưa hoàn tất.
13. D01/D04/D07/D08/D09/D10 và các records D/E hiện đọc đều PENDING, không có signed approval.
14. G1/G2 chưa được đóng do residual implementation/test gaps. G3/G4 BLOCKED vì thiếu independent review, signed decisions và separate authorization.
15. Shared Lab chưa được phép kích hoạt.
16. Production tiếp tục NO-GO.
17. Thiếu runtime principal/DPAPI disposable evidence, OCSP/responder lifecycle, worker recovery/failure matrix, deployable proxy/HA/backup/restore, ký duyệt scope/release và independent review.
18. PM cần chỉ định reviewer. IAM/Security duyệt D04 mapping và SoD, OT owner duyệt D08 permit/inventory/window, PKI owner duyệt D07, DBA duyệt D09 writer set/lock namespace/migrations/rollback, Audit owner duyệt D10 archive/retention và topology owner duyệt D01 cùng E dependencies. Mọi approval cần exact SHA, target/scope, validity và revocation disposition.

Ma trận A.10 ghi 88 PASS / 20 BLOCKED / 2 SKIP trên 110 requirements. Evidence chỉ xác nhận môi trường và slice đã ghi.
