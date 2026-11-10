# OCSP and CRL

`PinnedOcspCertificateValidator` is an unregistered candidate. The chain is pinned and validated through the existing CRL validator chain helper. OCSP requires a matching certificate ID, pinned issuer signature and responder identity, and current response timestamps. It denies unknown/revoked and invalid signatures, reports unavailable distinctly, bounds payload/timeout, and does not follow redirects or use AIA URLs.

`A11OcspTests` uses actual signed OCSP responses from loopback Kestrel. It covers good, revoked, unknown, wrong responder, bad signature, stale, timeout, HTTP 500, unreachable and mismatched certificate ID. The candidate supports directly issuing RSA CA responders. Delegated responders, combined CRL/OCSP policy, ingress integration and rotation are open. Existing CRL behavior remains independently exercised by regression. [RFC 6960](https://www.rfc-editor.org/rfc/rfc6960.html)
