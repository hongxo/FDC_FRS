# Policy dependency register

Hồ sơ Decision_Records.json 4.3A có 20 subdecision PENDING. Các owner dưới đây là vai trò cần phê duyệt, chưa có người ký được xác minh.

| Feature | Decision | Status | Owner | Evidence | Permitted | Activate | Blocker / action |
|---|---|---|---|---|---|---|---|
| Human/Machine | D01/D07 | PENDING | Security, OT, PKI | 4.3A trust ADR và register | Loopback và negative tests | Không | Ký listener, routing, TLS/PKI, bypass tests |
| Login | D02/D05 | PENDING | IAM/Security | 4.3A Auth ADR | Disabled UI, GET status, test | Không | Password/throttle, cookie và session issuance |
| MFA/recovery | D03/D06 | PENDING | IAM/Security, Operations | 4.3A Auth ADR | Disabled UI, trạng thái chung | Không | Chốt factor, fallback, custody |
| Users/roles/grants | D04 | PENDING | IAM/Data owners | RBAC ADR/catalog | Authorized read adapters fail closed | Không | Ký mapping, delegation và catalog |
| Apply/revoke | D09 | PENDING | DBA/IAM | Transaction ADR | Tests và contracts | Không | Ký lock order, writer coverage, current-authority check |
| Audit | D10 | PENDING | Security/Compliance | Audit ADR | Minimized read-side | Không | Ký outbox, retention, archive/restore |
| Development | D12 | Dev/Lab scope | Product/Security | Prior Lab implementation | Code và rollback-only tests | Dev/Lab độc lập | Không thay approval D01–D10 |
