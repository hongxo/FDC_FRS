# FDC COMTRADE HTTP Race Evidence

HTTP test dùng user A có quyền download thật, metadata file A, digest và size đúng. Test opener thay ancestor bằng directory junction sau attributes check, mở handle trỏ sang thư mục ngoài private root có cùng bytes/digest. File source kiểm tra handle thực tế và từ chối staging. HTTP trả 404 với 0 byte. SQL xác nhận business.record.download.verification_denied và outbox có correlation khớp response.

Raw a10-http-path-swap.json và TRX ở tmp/a10g/tmp/phase4-4ca10/delivery. Unit tests vẫn kiểm tra traversal, alternate data stream, size/cap, wrong digest và source thay đổi sau staging. Browser business kiểm tra CFG/DAT hash, no-store và cross-station zero bytes. Test HTTP path swap sử dụng Windows junction. File symlink/Linux variant cần kiểm chứng riêng trên hệ điều hành đó.

Private storage vẫn cần ACL độc lập. Chưa có hardlink adversarial test hoặc kiểm chứng file handle trên mọi filesystem. Không đưa physical path vào API response.
