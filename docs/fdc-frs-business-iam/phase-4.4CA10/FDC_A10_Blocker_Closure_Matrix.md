# A.10 Blocker Closure Matrix

Đối chiếu các hạng mục còn mở ở cuối A.9. Technical PASS chỉ áp dụng cho slice synthetic/disposable đã ghi. Các blocker policy/acceptance gốc vẫn giữ trong cột shared gate. Không cộng các pha vì có yêu cầu trùng nhau.

A.7 có 10/11 hạng mục được xác minh kỹ thuật trong fixture, còn independent acceptance. A.8 có 17/20 hạng mục được xác minh trong slice, còn combined CRL/OCSP, signed review và production. A.9 có 29/40 BLOCKED và 4/4 SKIP requirement observations chuyển PASS trong slice. OCSP và 10 readiness requirements còn mở. Registry status không bao phủ station/certificate rebinding, bounded JSON export không bao phủ bulk COMTRADE archive.

| Baseline ID | Previous state | A.10 technical observation | Raw evidence | Shared Lab / approval state |
|---|---|---|---|---|
| A7-10 | A.9 còn mở | PASS trong slice fixture | tmp/a10g/tmp/phase4-4ca10/delivery/a10-lab-browser-idle/absolute/security/revoked/mfa.json | BLOCKED_BY_POLICY / independent release required |
| A7-33 | A.9 còn mở | PASS trong slice fixture | tmp/a10g/tmp/phase4-4ca10/delivery/a10-http-path-swap.json | BLOCKED_BY_POLICY / independent release required |
| A7-44 | A.9 còn mở | PASS trong slice fixture | tmp/a10g/tmp/phase4-4ca10/delivery/lab-http.json + a10-lab-browser-idle.json | BLOCKED_BY_POLICY / independent release required |
| A7-45 | A.9 còn mở | PASS trong slice fixture | tmp/a10g/tmp/phase4-4ca10/delivery/lab-http.json + a10-lab-browser-idle.json | BLOCKED_BY_POLICY / independent release required |
| A7-46 | A.9 còn mở | PASS trong slice fixture | tmp/a10g/tmp/phase4-4ca10/delivery/lab-http.json + a10-lab-browser-idle.json | BLOCKED_BY_POLICY / independent release required |
| A7-47 | A.9 còn mở | PASS trong slice fixture | tmp/a10g/tmp/phase4-4ca10/delivery/lab-http.json + a10-lab-browser-idle.json | BLOCKED_BY_POLICY / independent release required |
| A7-48 | A.9 còn mở | PASS trong slice fixture | tmp/a10g/tmp/phase4-4ca10/delivery/lab-http.json + a10-lab-browser-idle.json | BLOCKED_BY_POLICY / independent release required |
| A7-49 | A.9 còn mở | PASS trong slice fixture | tmp/a10g/tmp/phase4-4ca10/delivery/lab-http.json + a10-lab-browser-idle.json | BLOCKED_BY_POLICY / independent release required |
| A7-50 | A.9 còn mở | PASS trong slice fixture | tmp/a10g/tmp/phase4-4ca10/delivery/lab-http.json + a10-lab-browser-idle.json | BLOCKED_BY_POLICY / independent release required |
| A7-59 | A.9 còn mở | PASS trong slice fixture | tmp/a10g/tmp/phase4-4ca10/delivery/a10-export-revoke.json (bounded Lab JSON artifact) | BLOCKED_BY_POLICY / independent release required |
| A7-60 | A.9 còn mở | BLOCKED | tmp/a10g/tmp/phase4-4ca10/delivery/Independent reviewer/signature missing | BLOCKED_BY_POLICY / independent release required |
| A8-11 | A.9 còn mở | PASS trong slice fixture | tmp/a10g/tmp/phase4-4ca10/delivery/a10-lab-browser-idle/absolute/security/revoked/mfa.json | BLOCKED_BY_POLICY / independent release required |
| A8-30 | A.9 còn mở | PASS trong slice fixture | tmp/a10g/tmp/phase4-4ca10/delivery/a10-http-path-swap.json | BLOCKED_BY_POLICY / independent release required |
| A8-38 | A.9 còn mở | PASS trong slice fixture | tmp/a10g/tmp/phase4-4ca10/delivery/a10-binding-browser.json (registry status; station immutable) | BLOCKED_BY_POLICY / independent release required |
| A8-39 | A.9 còn mở | PASS trong slice fixture | tmp/a10g/tmp/phase4-4ca10/delivery/Business browser TRX (22 history rows, safe ErrorCode) | BLOCKED_BY_POLICY / independent release required |
| A8-40 | A.9 còn mở | PASS trong slice fixture | tmp/a10g/tmp/phase4-4ca10/delivery/Business browser TRX (22 history rows, safe ErrorCode) | BLOCKED_BY_POLICY / independent release required |
| A8-49 | A.9 còn mở | BLOCKED, verified subset only | tmp/a10g/tmp/phase4-4ca10/delivery/a10-crl-mtls.json; OCSP missing | BLOCKED_BY_POLICY / independent release required |
| A8-51 | A.9 còn mở | PASS trong slice fixture | tmp/a10g/tmp/phase4-4ca10/delivery/lab-http.json + a10-lab-browser-idle.json + SQL TRX | BLOCKED_BY_POLICY / independent release required |
| A8-52 | A.9 còn mở | PASS trong slice fixture | tmp/a10g/tmp/phase4-4ca10/delivery/lab-http.json + a10-lab-browser-idle.json + SQL TRX | BLOCKED_BY_POLICY / independent release required |
| A8-53 | A.9 còn mở | PASS trong slice fixture | tmp/a10g/tmp/phase4-4ca10/delivery/lab-http.json + a10-lab-browser-idle.json + SQL TRX | BLOCKED_BY_POLICY / independent release required |
| A8-54 | A.9 còn mở | PASS trong slice fixture | tmp/a10g/tmp/phase4-4ca10/delivery/lab-http.json + a10-lab-browser-idle.json + SQL TRX | BLOCKED_BY_POLICY / independent release required |
| A8-55 | A.9 còn mở | PASS trong slice fixture | tmp/a10g/tmp/phase4-4ca10/delivery/lab-http.json + a10-lab-browser-idle.json + SQL TRX | BLOCKED_BY_POLICY / independent release required |
| A8-56 | A.9 còn mở | PASS trong slice fixture | tmp/a10g/tmp/phase4-4ca10/delivery/lab-http.json + a10-lab-browser-idle.json + SQL TRX | BLOCKED_BY_POLICY / independent release required |
| A8-57 | A.9 còn mở | PASS trong slice fixture | tmp/a10g/tmp/phase4-4ca10/delivery/lab-http.json + a10-lab-browser-idle.json + SQL TRX | BLOCKED_BY_POLICY / independent release required |
| A8-58 | A.9 còn mở | PASS trong slice fixture | tmp/a10g/tmp/phase4-4ca10/delivery/lab-http.json + a10-lab-browser-idle.json + SQL TRX | BLOCKED_BY_POLICY / independent release required |
| A8-59 | A.9 còn mở | PASS trong slice fixture | tmp/a10g/tmp/phase4-4ca10/delivery/lab-http.json + a10-lab-browser-idle.json + SQL TRX | BLOCKED_BY_POLICY / independent release required |
| A8-60 | A.9 còn mở | PASS trong slice fixture | tmp/a10g/tmp/phase4-4ca10/delivery/lab-http.json + a10-lab-browser-idle.json + SQL TRX | BLOCKED_BY_POLICY / independent release required |
| A8-66 | A.9 còn mở | PASS trong slice fixture | tmp/a10g/tmp/phase4-4ca10/delivery/a10-export-revoke.json (Lab evidence queue) | BLOCKED_BY_POLICY / independent release required |
| A8-67 | A.9 còn mở | PASS trong slice fixture | tmp/a10g/tmp/phase4-4ca10/delivery/a10-cache-browser.json + a10-proxy-sql.json | BLOCKED_BY_POLICY / independent release required |
| A8-68 | A.9 còn mở | BLOCKED, verified subset only | tmp/a10g/tmp/phase4-4ca10/delivery/Independent/production release missing | BLOCKED_BY_POLICY / independent release required |
| A8-70 | A.9 còn mở | BLOCKED, verified subset only | tmp/a10g/tmp/phase4-4ca10/delivery/Independent/production release missing | BLOCKED_BY_POLICY / independent release required |
| A9-17 | SKIP | PASS trong slice fixture | tmp/a10g/tmp/phase4-4ca10/delivery/exact legacy test TRX in FDC_28_Skip_Regression_Report.md | BLOCKED_BY_POLICY / shared release pending |
| A9-18 | SKIP | PASS trong slice fixture | tmp/a10g/tmp/phase4-4ca10/delivery/exact legacy test TRX in FDC_28_Skip_Regression_Report.md | BLOCKED_BY_POLICY / shared release pending |
| A9-19 | SKIP | PASS trong slice fixture | tmp/a10g/tmp/phase4-4ca10/delivery/exact legacy test TRX in FDC_28_Skip_Regression_Report.md | BLOCKED_BY_POLICY / shared release pending |
| A9-20 | SKIP | PASS trong slice fixture | tmp/a10g/tmp/phase4-4ca10/delivery/exact legacy test TRX in FDC_28_Skip_Regression_Report.md | BLOCKED_BY_POLICY / shared release pending |
| A9-29 | BLOCKED | PASS trong slice fixture | tmp/a10g/tmp/phase4-4ca10/delivery/a10-lab-browser-idle/absolute/security/revoked/mfa.json | BLOCKED_BY_POLICY / shared release pending |
| A9-36 | BLOCKED | PASS trong slice fixture | tmp/a10g/tmp/phase4-4ca10/delivery/lab-http.json + a10-lab-browser-idle.json + a10-terminal-outcomes.json | BLOCKED_BY_POLICY / shared release pending |
| A9-37 | BLOCKED | PASS trong slice fixture | tmp/a10g/tmp/phase4-4ca10/delivery/lab-http.json + a10-lab-browser-idle.json + a10-terminal-outcomes.json | BLOCKED_BY_POLICY / shared release pending |
| A9-38 | BLOCKED | PASS trong slice fixture | tmp/a10g/tmp/phase4-4ca10/delivery/lab-http.json + a10-lab-browser-idle.json + a10-terminal-outcomes.json | BLOCKED_BY_POLICY / shared release pending |
| A9-39 | BLOCKED | PASS trong slice fixture | tmp/a10g/tmp/phase4-4ca10/delivery/lab-http.json + a10-lab-browser-idle.json + a10-terminal-outcomes.json | BLOCKED_BY_POLICY / shared release pending |
| A9-40 | BLOCKED | PASS trong slice fixture | tmp/a10g/tmp/phase4-4ca10/delivery/lab-http.json + a10-lab-browser-idle.json + a10-terminal-outcomes.json | BLOCKED_BY_POLICY / shared release pending |
| A9-41 | BLOCKED | PASS trong slice fixture | tmp/a10g/tmp/phase4-4ca10/delivery/lab-http.json + a10-lab-browser-idle.json + a10-terminal-outcomes.json | BLOCKED_BY_POLICY / shared release pending |
| A9-42 | BLOCKED | PASS trong slice fixture | tmp/a10g/tmp/phase4-4ca10/delivery/lab-http.json + a10-lab-browser-idle.json + a10-terminal-outcomes.json | BLOCKED_BY_POLICY / shared release pending |
| A9-43 | BLOCKED | PASS trong slice fixture | tmp/a10g/tmp/phase4-4ca10/delivery/lab-http.json + a10-lab-browser-idle.json + a10-terminal-outcomes.json | BLOCKED_BY_POLICY / shared release pending |
| A9-44 | BLOCKED | PASS trong slice fixture | tmp/a10g/tmp/phase4-4ca10/delivery/lab-http.json + a10-lab-browser-idle.json + a10-terminal-outcomes.json | BLOCKED_BY_POLICY / shared release pending |
| A9-45 | BLOCKED | PASS trong slice fixture | tmp/a10g/tmp/phase4-4ca10/delivery/lab-http.json + a10-lab-browser-idle.json + a10-terminal-outcomes.json | BLOCKED_BY_POLICY / shared release pending |
| A9-46 | BLOCKED | PASS trong slice fixture | tmp/a10g/tmp/phase4-4ca10/delivery/lab-http.json + a10-lab-browser-idle.json + a10-terminal-outcomes.json | BLOCKED_BY_POLICY / shared release pending |
| A9-47 | BLOCKED | PASS trong slice fixture | tmp/a10g/tmp/phase4-4ca10/delivery/lab-http.json + a10-lab-browser-idle.json + a10-terminal-outcomes.json | BLOCKED_BY_POLICY / shared release pending |
| A9-48 | BLOCKED | PASS trong slice fixture | tmp/a10g/tmp/phase4-4ca10/delivery/lab-http.json + a10-lab-browser-idle.json + a10-terminal-outcomes.json | BLOCKED_BY_POLICY / shared release pending |
| A9-49 | BLOCKED | PASS trong slice fixture | tmp/a10g/tmp/phase4-4ca10/delivery/lab-http.json + a10-lab-browser-idle.json + a10-terminal-outcomes.json | BLOCKED_BY_POLICY / shared release pending |
| A9-50 | BLOCKED | PASS trong slice fixture | tmp/a10g/tmp/phase4-4ca10/delivery/lab-http.json + a10-lab-browser-idle.json + a10-terminal-outcomes.json | BLOCKED_BY_POLICY / shared release pending |
| A9-51 | BLOCKED | PASS trong slice fixture | tmp/a10g/tmp/phase4-4ca10/delivery/lab-http.json + a10-lab-browser-idle.json + a10-terminal-outcomes.json | BLOCKED_BY_POLICY / shared release pending |
| A9-52 | BLOCKED | PASS trong slice fixture | tmp/a10g/tmp/phase4-4ca10/delivery/lab-http.json + a10-lab-browser-idle.json + a10-terminal-outcomes.json | BLOCKED_BY_POLICY / shared release pending |
| A9-53 | BLOCKED | PASS trong slice fixture | tmp/a10g/tmp/phase4-4ca10/delivery/lab-http.json + a10-lab-browser-idle.json + a10-terminal-outcomes.json | BLOCKED_BY_POLICY / shared release pending |
| A9-54 | BLOCKED | PASS trong slice fixture | tmp/a10g/tmp/phase4-4ca10/delivery/lab-http.json + a10-lab-browser-idle.json + a10-terminal-outcomes.json | BLOCKED_BY_POLICY / shared release pending |
| A9-55 | BLOCKED | PASS trong slice fixture | tmp/a10g/tmp/phase4-4ca10/delivery/lab-http.json + a10-lab-browser-idle.json + a10-terminal-outcomes.json | BLOCKED_BY_POLICY / shared release pending |
| A9-61 | BLOCKED | PASS trong slice fixture | tmp/a10g/tmp/phase4-4ca10/delivery/a10-http-path-swap.json | BLOCKED_BY_POLICY / shared release pending |
| A9-62 | BLOCKED | PASS trong slice fixture | tmp/a10g/tmp/phase4-4ca10/delivery/a10-crl-mtls.json (pinned full CRL; no responder) | BLOCKED_BY_POLICY / shared release pending |
| A9-63 | BLOCKED | BLOCKED | tmp/a10g/tmp/phase4-4ca10/delivery/Policy/review/OCSP evidence missing | BLOCKED_BY_POLICY / shared release pending |
| A9-64 | BLOCKED | PASS trong slice fixture | tmp/a10g/tmp/phase4-4ca10/delivery/a10-crl-mtls.json (pinned full CRL; no responder) | BLOCKED_BY_POLICY / shared release pending |
| A9-66 | BLOCKED | PASS trong slice fixture | tmp/a10g/tmp/phase4-4ca10/delivery/a10-export-revoke.json | BLOCKED_BY_POLICY / shared release pending |
| A9-67 | BLOCKED | PASS trong slice fixture | tmp/a10g/tmp/phase4-4ca10/delivery/a10-cache-browser.json | BLOCKED_BY_POLICY / shared release pending |
| A9-73 | BLOCKED | PASS trong slice fixture | tmp/a10g/tmp/phase4-4ca10/delivery/a10-binding-browser.json + business browser TRX | BLOCKED_BY_POLICY / shared release pending |
| A9-74 | BLOCKED | PASS trong slice fixture | tmp/a10g/tmp/phase4-4ca10/delivery/a10-binding-browser.json + business browser TRX | BLOCKED_BY_POLICY / shared release pending |
| A9-75 | BLOCKED | PASS trong slice fixture | tmp/a10g/tmp/phase4-4ca10/delivery/a10-binding-browser.json + business browser TRX | BLOCKED_BY_POLICY / shared release pending |
| A9-81 | BLOCKED | BLOCKED | tmp/a10g/tmp/phase4-4ca10/delivery/Policy/review/OCSP evidence missing | BLOCKED_BY_POLICY / shared release pending |
| A9-82 | BLOCKED | BLOCKED | tmp/a10g/tmp/phase4-4ca10/delivery/Policy/review/OCSP evidence missing | BLOCKED_BY_POLICY / shared release pending |
| A9-83 | BLOCKED | BLOCKED | tmp/a10g/tmp/phase4-4ca10/delivery/Policy/review/OCSP evidence missing | BLOCKED_BY_POLICY / shared release pending |
| A9-84 | BLOCKED | BLOCKED | tmp/a10g/tmp/phase4-4ca10/delivery/Policy/review/OCSP evidence missing | BLOCKED_BY_POLICY / shared release pending |
| A9-85 | BLOCKED | BLOCKED | tmp/a10g/tmp/phase4-4ca10/delivery/Policy/review/OCSP evidence missing | BLOCKED_BY_POLICY / shared release pending |
| A9-86 | BLOCKED | BLOCKED | tmp/a10g/tmp/phase4-4ca10/delivery/Policy/review/OCSP evidence missing | BLOCKED_BY_POLICY / shared release pending |
| A9-87 | BLOCKED | BLOCKED | tmp/a10g/tmp/phase4-4ca10/delivery/Policy/review/OCSP evidence missing | BLOCKED_BY_POLICY / shared release pending |
| A9-88 | BLOCKED | BLOCKED | tmp/a10g/tmp/phase4-4ca10/delivery/Policy/review/OCSP evidence missing | BLOCKED_BY_POLICY / shared release pending |
| A9-89 | BLOCKED | BLOCKED | tmp/a10g/tmp/phase4-4ca10/delivery/Policy/review/OCSP evidence missing | BLOCKED_BY_POLICY / shared release pending |
| A9-90 | BLOCKED | BLOCKED | tmp/a10g/tmp/phase4-4ca10/delivery/Policy/review/OCSP evidence missing | BLOCKED_BY_POLICY / shared release pending |
