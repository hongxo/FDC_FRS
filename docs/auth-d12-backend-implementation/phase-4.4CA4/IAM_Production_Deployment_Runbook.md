# Production deployment · PROPOSED / NO-GO

```text
Human Browser → Human HTTPS listener → IAM service → private SQL primary/HA
Collector → OT DMZ mTLS listener → ingest service → private COMTRADE storage
IAM audit outbox → restricted archive writer → independent immutable archive
```

1. Release owner (chưa chỉ định) xác minh order, change window, signed digest/decisions, UAT và rollback owner. Dừng nếu thiếu.
2. Security/OT xác minh route ACL, proxy trust và cert chain/revoke. Dùng identity riêng cho IAM runtime, migrations và archive. Secrets/key ring qua vault ACL, mã hóa, rotation và backup riêng.
3. DBA xác minh full/log backup, point-in-time restore trên bản sao, schema parity và session/restore epoch. Không auto-migrate. Deploy signed artifact vào canary theo kế hoạch đã ký.
4. Kiểm tra TLS, login/MFA, revoked session, scoped user/role/station, audit/outbox và machine denial theo test accounts có kiểm soát. Quan sát login failure, p95, SQL lock/failover, outbox age, archive integrity và cert expiry.
5. Go/No-Go với release owner, SRE, Security, DBA. Dừng nếu station leak, missing audit, verify failures, SQL inconsistency hoặc threshold đã duyệt bị vượt. Rollout tiếp chỉ khi independent acceptance.
