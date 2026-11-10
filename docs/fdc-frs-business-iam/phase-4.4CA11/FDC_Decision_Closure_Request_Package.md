# Decisions requiring owner disposition

| Decision | Decision required | Evidence to request | Approver |
|---|---|---|---|
| D01 | Listener/proxy trust and route isolation | Two-node forwarding/spoof/bypass test | Architecture, Security, OT, App owner |
| D04 | Rebinding scope, independent review and cutover | SoD, both station scopes, replay and concurrency | IAM and business owners, Security |
| D07 | Issuance, renewal, rotation and binding | Synthetic end-to-end predecessor/successor cutover | PKI, OT, Security |
| D08 | CRL/OCSP evidence priority and outage behavior | Signed answers, stale/outage and ingress trace | PKI, Security, OT |
| D09 | SQL principal separation, HA, revocation/commit, restore epoch | Distinct identities, failover, restore and commit ordering | DBA, Security, Architecture |
| D10 | Audit/outbox integrity, retention and access | Outbox fault, archive/restore and correlations | Security, Audit, Data owner |

All listed decisions remain PENDING per the formal records. Request selected option, limits, owner signatures, date and exact version. No row authorizes activation.
