# FDC Security Regression Report

Full regression chạy offline mặc định và opt-in fresh SQL/Edge + native simulator. Shared DB suites giữ SKIP. Lượt đầu fixture heartbeat status vi phạm enum SQL; sửa thành giá trị canonical. Lượt hai CFG thiếu time multiplier 1999, parser chặn; bổ sung fixture và chạy lại. Không đổi constraints hoặc trigger.

run-04 trên code 29cab609faa2a23c4c3b6f46bfcea65347c37fd5 ghi Collector 47 PASS, IAM 256 PASS / 23 SKIP, Center 37 PASS / 19 SKIP. Không có FAIL. Node local có 12 PASS, gồm 1 test local có trước A.8 và nằm ngoài diff. Evidence nội bộ gồm tests.log, node.log và TRX.
