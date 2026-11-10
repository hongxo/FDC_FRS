# FDC mTLS Revocation Evidence

PinnedCrlCertificateValidator là candidate chưa đăng ký vào startup. Nó yêu cầu pinned direct RSA issuing CA, client-auth EKU, chain hợp lệ và full CRL còn hạn có chữ ký hợp lệ. Chain build tắt download và phần revocation luôn được kiểm tra bằng CRL đã cung cấp. Không truy cập URL từ certificate.

Các trạng thái gồm CertValid, CertRevoked, CertExpired, CertInvalid, RevocationUnknown và RevocationUnavailable. Test kiểm tra leaf hợp lệ/revoked/expired/wrong EKU/unknown issuer, CRL cũ, chữ ký hỏng và CRL không khả dụng. Kestrel mTLS loopback nhận đúng một request hợp lệ và từ chối handshake với revoked hoặc unavailable CRL. A8MachineIngressTests tiếp tục xác minh registry, station binding và SQL intake.

Raw a10-crl-mtls.json, a8-mtls.json và TRX ở tmp/a10g/tmp/phase4-4ca10/delivery. OCSP và CRL/OCSP HTTP responder outage chưa được triển khai. CRL không khả dụng ở test là thiếu trusted CRL bundle. Intermediates, delta/indirect CRL, key rotation và publication policy cần D07 disposition. Không đánh dấu toàn bộ A8-49 hay A9-63 OCSP là PASS.

CRL fixture được tạo bằng API .NET 10. [CertificateRevocationListBuilder.Build](https://learn.microsoft.com/en-us/dotnet/api/system.security.cryptography.x509certificates.certificaterevocationlistbuilder.build?view=net-10.0)

CRL handshake và SQL ingress được kiểm chứng bằng hai fixture riêng. Chưa có nghiệm thu một Center listener triển khai thực tế kết hợp cả signed CRL, registry SQL, proxy topology và deployment certificate lifecycle.
