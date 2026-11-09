# Primary technical references

Tất cả external sources dưới đây được tra cứu2026-11-09, primary sources. Không là project approval hoặc compliance certification; provider/library/device deployment phải review riêng.

| ID | Source | Dùng ở đâu/giới hạn |
|---|---|---|
| MS-PROXY | [Microsoft ASP.NET Core10 proxy trust](https://learn.microsoft.com/en-us/aspnet/core/host-and-deploy/proxy-load-balancer?view=aspnetcore-10.0) | Known proxies/header handling tham khảo D01; không tự tin header hoặcaccept all proxies |
| MS-LOCK | [Microsoft SQL Server sp_getapplock](https://learn.microsoft.com/en-us/sql/relational-databases/system-stored-procedures/sp-getapplock-transact-sql?view=sql-server-ver16) | Database-scoped transaction-owner lock semantics; không all-writer assurance |
| MS-DP | [Microsoft ASP.NET Core10 Data Protection](https://learn.microsoft.com/en-us/aspnet/core/security/data-protection/configuration/overview?view=aspnetcore-10.0) | Shared key storage/ACL/encryption choices, không opaque-session authority |
| MS-DP-DEFAULT | [Microsoft Data Protection environment defaults](https://learn.microsoft.com/en-us/aspnet/core/security/data-protection/configuration/default-settings?view=aspnetcore-10.0) | Default environment-dependent persistence không tự là approved multi-node key policy |
| NIST-AUTH | [NIST SP800-63B-4 authenticators](https://pages.nist.gov/800-63-4/sp800-63b/authenticators/) | Password/MFA reference, không tuyên bố FDC đạt assurance level |
| W3C-WA | [W3C WebAuthn Level3](https://www.w3.org/TR/webauthn-3/) | RP/origin/challenge/user verification protocol reference |
| NIST-OT | [NIST SP800-82rev3 final](https://csrc.nist.gov/pubs/sp/800/82/r3/final) | OT safety/reliability/topology reference; không tự áp một draft thay final |
