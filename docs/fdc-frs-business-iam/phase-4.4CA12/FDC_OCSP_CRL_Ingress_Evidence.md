# OCSP/CRL on isolated HTTPS ingress

The Testing-only scheme accepts TLS peer identity on loopback and requires fresh signed CRL and issuer-signed OCSP status. A CRL denial short-circuits OCSP. Every request through the scheme rechecks evidence. The fixture maps the certificate fingerprint to an ephemeral Collector GUID and the underlying Pilot upload still checks current SQL station/IED/Collector registration. The scheme is opt-in in the isolated host. Main Center does not register it. The scheme returns a generic empty 401 on revocation failures.

In the clean run, valid + bound yielded 201 and the upload completed with one record and one processing outbox row. Ten revocation failure cases yielded 401 with empty body, no record and no processing outbox row. Wrong station yielded 403. Human cookie plus a spoofed cert header without a TLS peer yielded 401. See [raw evidence](FDC_A12_Raw_Evidence_Manifest.md).

The exercised policy is STRICT_COMBINED. CRL_ONLY and OCSP_REQUIRED are separate baseline validators. CRL_OR_OCSP semantics and source precedence await policy review. No independent denial audit/outbox or approved production listener was verified. [OCSP RFC 6960](https://www.rfc-editor.org/rfc/rfc6960.html)
