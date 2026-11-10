# PKI rotation and revocation

Proposed lab sequence under D07/D08: issue synthetic successor → verify station binding → start bounded overlap → apply approved cutover → revoke predecessor → verify predecessor denied and successor accepted on fresh and reused connections. During outage verify the applicable fail-closed candidate and recovery with fresh signed evidence. Record issuer fingerprint, serial hash, timestamps and request correlation. Preserve keys outside the repository.

Rotation and revocation after cutover remain BLOCKED. Signed OCSP response coverage alone does not demonstrate these transitions.
