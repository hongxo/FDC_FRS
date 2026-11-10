# FDC mTLS Revocation Evidence

Fixture mTLS A.8 chạy lại với CA test và registry SQL cô lập, gồm wrong station, inactive collector, registry unavailable, expiry và wrong EKU. Revocation CRL/OCSP chưa được thực thi. Registry inactive không phải bằng chứng revocation của PKI. PKI owner cần nguồn CRL/OCSP test, leaf revoked và negative outage fail-closed.
