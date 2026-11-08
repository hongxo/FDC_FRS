# Human/Machine boundary validation

Date: 2026-11-08 (Asia/Bangkok). Baseline `b8bd4c0b1d6731b0945b3a6bd12a7b0ea50465e4`; validated source `f60f8e2cdebc25416bbce5d4ab19b7bf6594b960`. Development/Lab only. Formal approvals, P0-IAM and production readiness unchanged.

PASS components: HumanSessionHandler denies HTTP, presented client certificate, duplicate cookie and forged identity headers; every handler authentication re-reads current authority. Real antiforgery token tests are in component suite. Phase4 directly exercises role GET adapter RequestDelegate with TEST-ONLY principal: HTTP401 before policy, HTTPS503 while policy pending. No listener or real authenticated principal is created by these tests.

Hardening aligns role GET adapter HTTPS/certificate checks and wrapped-SQL error handling with management GET adapter. No endpoint is registered in Center. Machine mTLS issuer/cert binding, invalid certificate/revocation/proxy bypass, Human cookie to actual Machine API and global RequireCertificate login behavior are BLOCKED for deployed validation.

Read-only original Center source inspection still finds Demo/AdminApiKey paths and global RequireCertificate configuration. Standalone IAM changes do not remove these or prove topology isolation. D01 route/listener/proxy change approval and actual test host are required; no production PKI/firewall/OT change made. Treat this as a high-priority integration blocker, not a demonstrated new remote exploit.
