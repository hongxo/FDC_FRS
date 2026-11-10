# Collector security integration

The isolated machine route uses a real TLS client certificate, signed strict CRL/OCSP policy, fixture fingerprint mapping and SQL Collector/station/IED validation. The wrong-station case returns 403. Human cookie with a forged certificate header returns 401. See [ingress evidence](FDC_OCSP_CRL_Ingress_Evidence.md).

Atomic rebind, certificate overlap and successor cutover remain BLOCKED. A single-station ingress test provides no evidence of the complete request/review/approve/apply workflow. See [Collector rebind gap](FDC_Collector_Rebinding_Evidence.md).
