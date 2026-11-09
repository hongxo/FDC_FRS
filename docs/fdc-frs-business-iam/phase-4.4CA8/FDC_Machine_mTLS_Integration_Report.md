# FDC Machine mTLS Integration Report

Host TEST_ONLY riêng trên loopback, root CA sinh trong bộ nhớ, validate chain/EKU/expiry và pin cert trước khi SQL registry ràng collector/station/IED active. Dùng PilotFileIntake và SqlRecordIngestion hiện có. Không đăng ký vào Center runtime. Registry fixture cố ý phát lỗi authority để kiểm 503. CRL/OCSP và production revocation chưa nghiệm thu.
