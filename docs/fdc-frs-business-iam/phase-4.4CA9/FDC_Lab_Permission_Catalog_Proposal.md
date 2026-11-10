# FDC Lab Permission Catalog Proposal

PROPOSED / AWAITING_APPROVAL. Catalog workspace có candidate.create.edit, candidate.submit, candidate.review, candidate.approve, evidence.export, audit.business.view và deny-only ied.configuration.write. Không tự áp alias cho Lab. D04 cần quyết định mã đề xuất dưới đây hoặc ánh xạ canonical đã ký.

| Code đề xuất | Action | Scope | Risk | Approval / audit / migration |
|---|---|---|---|---|
| lab.test.read | List/detail/readiness và evidence metadata | Station + IED + permit revision | Medium | IAM + OT. Audit truy cập nhạy cảm. Catalog migration sau release. |
| lab.test.request | Soạn và submit proposal bất biến theo revision | Station + IED | High | IAM + OT. Audit requester/version/outbox. Catalog migration sau release. |
| lab.test.review | Review độc lập requester | Station + IED + revision | High | Security + OT. Audit actor/MFA/revision/outbox. Catalog migration sau release. |
| lab.test.approve | Approve độc lập requester/reviewer | Station + IED + revision | Critical | Security + OT. Audit decision/MFA/digest/outbox. Catalog migration sau release. |
| lab.test.apply | Gate synthetic theo permit, revision và validity | Station + IED + permit | Critical | OT + Security. Audit allow/deny/result. Catalog migration sau release. |
| lab.test.export | Chuẩn bị artifact evidence có recheck | Station + IED + revision | High | IAM + Security. Audit request/worker/release. Catalog migration sau release. |
