# Gói trình phê duyệt · PROPOSED

| Quyết định | Phương án | Đề xuất và tác động | Owner | Bằng chứng cần | Nếu hoãn |
|---|---|---|---|---|---|
| D01/D07 | Tách listener hoặc shared proxy theo rule | Tách, giảm nguy cơ nhầm credential lane | Security/OT/PKI | Diagram, cert chain, revoke và direct bypass tests | Chỉ loopback |
| D02/D05 | Canonical password/session policy hoặc tiếp tục khóa | Server session có revoke hiện hành, D09 serialize | IAM/Security | Throttle, password provider, issuance, expiry, multi-instance | Không login/logout |
| D03/D06 | WebAuthn primary hoặc TOTP theo ngoại lệ; recovery owner-mediated | Chọn theo hỗ trợ thiết bị và ràng buộc recovery | Security/Operations | RP/origin, enrollment, retry, custody tests | Không MFA/recovery |
| D04 | Signed station/object mapping hoặc giữ default deny | Duyệt signed mapping và delegation theo assignment | IAM/Data owners | Signed catalog/hash, 7 role baseline custody, negative tests | Không đọc dữ liệu quản trị thực |
| D09 | Serialized SQL authority hoặc hoãn write | Serialize cùng khóa transaction với revoke/apply | DBA/IAM | Writer coverage, lock order, races, rollback | Không apply |
| D10 | Transactional outbox + immutable archive hoặc chỉ Lab evidence | Tách archive với retention/restore kiểm chứng | Security/Compliance | Tamper/restore, completeness, retention | Không audit acceptance |

Không có chữ ký hoặc quyết định hiệu lực trong package.
