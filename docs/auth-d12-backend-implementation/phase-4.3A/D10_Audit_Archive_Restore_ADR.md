# D10 — Audit, archive vàrestore

Ngày rà soát:2026-11-09 (Asia/Bangkok). Package version0.1 PROPOSED. Baseline kết hợp PR6:d5ebc2acc18265786bae5af96ef3ba58c223e522; Phase4.2:9cd35889ddbbcc8d88e831419284877f4add5c6a. Source runtime/database: a0a6096; không sửa code trong 4.3A.
CURRENT là trạng thái source/DB; VERIFIED chỉ phạm vi quan sát/evidence được nêu; PROPOSED không thực thi. Approval date/evidence/người ký:NOT_PROVIDED. Independent reviewer:NOT_PROVIDED. Không tự đổi P0-IAM/readiness.
Nguồn đối chiếu và hash: [Baseline](Phase4_3A_Baseline_Inventory.md), [Evidence](Evidence_Index.md), Input_Manifest.json. Khuyến nghị cần được owner chọn, bind version/hash và phê duyệt; không phải configuration hoặc quyền đã có hiệu lực.

Decision ID:D10; D05; D09; E03/E06. Approval owner:Audit + Security + DBA + SRE. Current approval status:PENDING. Recommended/alternative options và tác động bên dưới.

## CURRENT / VERIFIED / NOT_IMPLEMENTED / PENDING

| Thành phần | CURRENT | VERIFIED evidence | Gap/status |
|---|---|---|---|
| audit.SecurityEvent | ActorUserId, EventCode, OutcomeCode, CorrelationId, PolicyVersion, RestoreEpoch +basefields | Original append-only trigger; role/session atomicfixtures | Thiếu target/request/reason/scope/fullversions/before-after; new D10 schema PENDING |
| ops.AuditOutbox | EventId, ArchivedAt, Fence | Role/session outbox failure rollback | No complete dispatcher/checkpoint/failure policy |
| audit.AuditDelivery/AuditCheckpoint | Safe metadata tables fromCenter002 | Schema/FK/checks only | Delivered/Verified flag không external integrity proof |
| RoleHistory | Applied originalversion/digest/state snapshot | Replay/outboxtests lịch sử | Not a replacement for security audit |
| SQL backup/restore | Empty53-table backup→unique disposable DB; DDL rollback probe | Actual toollogs/checks6 migrations | Không có dữ liệu audit/session/file reconciliation |
| SQL runtime role | Non-sa/non-db_owner; direct IAM/audit DML/secretread denied | Actual permission tests lịch sử | Migrationoperatorprivileged; archive/migrator roles scaffolds |

## Options và recommendation

A:transactional security event/outbox +independent immutable archive/externally anchored checkpoints; B:SQL-only hash chain/same-host archive; C:best-effort logs. PROPOSED:A. Rationale:tách app/DB compromise khỏi archive custody; trade-off:keys, delivery lag, gap handling, storage/retention cost. B không đủ chứng minh độc lập khi DBA/app cùngfaultdomain; C mất mandatory audit. No provider/key/retention purchase orSQLpermission change ở 4.3A.

## Proposed additive schema/release

Retain existing names/PKs/history. Extend SecurityEvent with server-derived ActorType/Id, service/cert reference(no privatekey), TargetType/Id, organization/station scope references, RequestId/revision/digest, reason/decision/outcome, CorrelationId andUTC, policy/catalog/hierarchy/security/grant versions, RestoreEpoch, factor assurance reference(no raw proof). Before/after chỉ allowlisted redacted fields orcontent hashes; không password/hash dump, raw session/OTP/reset proof, TOTP seed/privatekey/arbitrary request body. Role/Grant/delegation changes phải có actor+beneficiary impact +same-transaction audit/outbox; denied attempts cósafe event path không giảsuccessful mutation.

Archive records bind eventId/epoch, ordering/checkpoint range/content digest, source schema/policy version, signer/key version, delivery idempotency/fence. Sequence gap/duplicate/replay phải reconcile; hash chain cùng DB không độc lập immutable guarantee. Independent archive identity và key custody khác runtime/migrator/PKIcert keys; rotation/revoke/backup/verification workflow cần owner ký.

Retention options để owner đánh giá:hot90days/archive3years versushot365days/archive7years hoặc thời hạn khác theo business/legal classification. Không khẳng định pháp luật yêu cầu các số này, không config/deletejob đang chạy. Legal hold/deletion/privacy/scope phảiquyđịnhtrước archive activation. Secret redaction vàstorage location references được review theo threat model.

## Failure policy đề nghị

Required primary SecurityEvent+Outbox insert failure→rollback sensitive effect. Externalarchive temporarilyunavailable→committed outbox retained, bounded retries/alerts; threshold/backpressure/priority forcontainment phải owner quyết. Không undo audit hoặcmarkDelivered đểgiảmqueue; không archive outage→allow unlogged admin. SQL unavailable→noauthority mutation; incident recovery không làcached admin bypass. Dispatcher least privilege/fence, archivewriter append-only, verifier read-only, migrator/operator controlled; không grant runtime directauditupdates.

## RestoreEpoch vàreconciliation

1. Isolate restored target/listeners; verify backup/schema/catalog/policy/history, archive checkpoint vàprivate file inventory trước activation.
2. New trusted RestoreEpoch ởcurrent authority; old sessions/challenges/reset/step-up proofs invalid dùcookie/keyring restoredecrypt được. Source has field/equality checks nhưng ceremony/challenge epoch binding chưa complete.
3. Reconcile SQL audit/outbox against independent anchors:missing ranges/duplicateIDs/unknowncommit results stop/review; không replay archived privilege grants thànheffective.
4. Reconcile file manifests/checksums/Collector receipt history vớirestored SQL; machine resend không cấp IAM rights. SQL-only restore không đủ file consistency.
5. Verified independent reviewer signs actual có dữ liệu old-proof denial, archivetamper/gap tests, SQLprincipal deniedDML andversion parity. RPO/RTO budgets làE06, không suy từempty restore speed.

Acceptance:forced requiredoutboxerror atomicrollback; archive outage/backpressure; tampered/truncated/replayed archive; key rotation/missing key; restore older DB withnew epoch; two-node stale sessions denied; privileged directDML review; approved access-only audit/export. Existing PASS coverspath-specificSQLatomicity/empty restore, notD10 archive acceptance.

## Phiếu quyết định cần owner hoàn tất

Option được chọn:CHƯA CÓ. Named accountable approver/delegation authority:CHƯA CÓ. Security/OT/DBA concurrence và independent reviewer:CHƯA CÓ. Approval date/evidence URI/chữ ký:NOT_PROVIDED. Owner phải ghi exact option, document+catalog+route+policy/schema version/hash, phạm vi Lab/production riêng, rationale, trade-offs, residual risk/expiry/review triggers và prerequisite dispositions. Chỉ reviewer kiểm bằng chứng ký thực tế mới cập nhật register; không dùng checkbox/điền tên thay approval.
