# A.7 / A.8 blocker closure

5/16 blocker A.7 và 5/25 blocker A.8 được xác minh theo scope fixture. Global navigation ở đây là menu Business; navigation ngoài Business shell không được bao phủ.

| ID | Requirement | A.9 kết quả và evidence |
|---|---|---|
| A7-06 | Global navigation | PASS trong fixture: Browser TRX + a7-browser.json |
| A7-08 | Paging UI | PASS trong fixture: Browser TRX + a7-browser.json |
| A7-10 | Session expiry UI | BLOCKED; đối chiếu nhóm tương ứng trong A.9 test matrix |
| A7-18 | Dashboard trends | PASS trong fixture: Browser TRX + a7-browser.json |
| A7-19 | Dashboard alerts | PASS trong fixture: Browser TRX + a7-browser.json |
| A7-20 | Station relationships | PASS trong fixture: Browser TRX + a7-browser.json |
| A7-33 | Symlink race | BLOCKED end-to-end; path swap tại file source PASS, còn thiếu HTTP audit swap |
| A7-44 | Lab list | BLOCKED; đối chiếu nhóm tương ứng trong A.9 test matrix |
| A7-45 | Lab details | BLOCKED; đối chiếu nhóm tương ứng trong A.9 test matrix |
| A7-46 | Lab evidence | BLOCKED; đối chiếu nhóm tương ứng trong A.9 test matrix |
| A7-47 | Lab permit | BLOCKED; đối chiếu nhóm tương ứng trong A.9 test matrix |
| A7-48 | Lab execute | BLOCKED; đối chiếu nhóm tương ứng trong A.9 test matrix |
| A7-49 | Lab approval | BLOCKED; đối chiếu nhóm tương ứng trong A.9 test matrix |
| A7-50 | Lab export | BLOCKED; đối chiếu nhóm tương ứng trong A.9 test matrix |
| A7-59 | Export revoke race | BLOCKED; đối chiếu nhóm tương ứng trong A.9 test matrix |
| A7-60 | Lab acceptance | BLOCKED; đối chiếu nhóm tương ứng trong A.9 test matrix |
| A8-10 | Global navigation | PASS trong fixture: Browser TRX + a7-browser.json |
| A8-11 | Session expiry clears DOM | BLOCKED; đối chiếu nhóm tương ứng trong A.9 test matrix |
| A8-12 | Trend scope | PASS trong fixture: Browser TRX + a7-browser.json |
| A8-13 | Alert scope | PASS trong fixture: Browser TRX + a7-browser.json |
| A8-14 | Relationship view | PASS trong fixture: Browser TRX + a7-browser.json |
| A8-15 | Paging controls multiple visible pages | PASS trong fixture: Browser TRX + a7-browser.json |
| A8-30 | Deterministic symlink race | BLOCKED end-to-end; path swap tại file source PASS, còn thiếu HTTP audit swap |
| A8-38 | Binding edit SoD | BLOCKED; đối chiếu nhóm tương ứng trong A.9 test matrix |
| A8-39 | Retry/error detail | BLOCKED; đối chiếu nhóm tương ứng trong A.9 test matrix |
| A8-40 | History UI paging controls | BLOCKED; đối chiếu nhóm tương ứng trong A.9 test matrix |
| A8-49 | CRL/OCSP revoked leaf | BLOCKED; đối chiếu nhóm tương ứng trong A.9 test matrix |
| A8-51 | Lab list | BLOCKED; đối chiếu nhóm tương ứng trong A.9 test matrix |
| A8-52 | Lab detail | BLOCKED; đối chiếu nhóm tương ứng trong A.9 test matrix |
| A8-53 | Lab evidence | BLOCKED; đối chiếu nhóm tương ứng trong A.9 test matrix |
| A8-54 | Lab permit | BLOCKED; đối chiếu nhóm tương ứng trong A.9 test matrix |
| A8-55 | Lab readiness browser | BLOCKED; đối chiếu nhóm tương ứng trong A.9 test matrix |
| A8-56 | Lab review | BLOCKED; đối chiếu nhóm tương ứng trong A.9 test matrix |
| A8-57 | Lab approve | BLOCKED; đối chiếu nhóm tương ứng trong A.9 test matrix |
| A8-58 | Lab execute | BLOCKED; đối chiếu nhóm tương ứng trong A.9 test matrix |
| A8-59 | Lab export | BLOCKED; đối chiếu nhóm tương ứng trong A.9 test matrix |
| A8-60 | Lab SoD E2E | BLOCKED; đối chiếu nhóm tương ứng trong A.9 test matrix |
| A8-66 | Export revoke race | BLOCKED; đối chiếu nhóm tương ứng trong A.9 test matrix |
| A8-67 | Multi-user cache isolation | BLOCKED; đối chiếu nhóm tương ứng trong A.9 test matrix |
| A8-68 | Independent signed Lab review | BLOCKED; đối chiếu nhóm tương ứng trong A.9 test matrix |
| A8-70 | Production acceptance | BLOCKED; đối chiếu nhóm tương ứng trong A.9 test matrix |
