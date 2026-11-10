# A.12 raw evidence manifest

Final technical source `064a1751c6b0e85c0bf618e1ed27a154cdd51941` was exported with git archive and restored from the trusted local package cache. Evidence directory relative to the delivery checkout is `tmp/a12delivery/tmp/phase4-4ca10/a12-delivery`.

Windows, .NET SDK 10.0.403, Node 22.12.0 and Playwright 1.62.1. SQL is local Shared Memory with Windows fixture administration. Runtime uses the generated non-SA principal. TLS, OCSP and simulator use loopback and synthetic assets. No A.12 migrations were added.

## Actual final results

- Build: 0 warnings / 0 errors.
- Center: 89 PASS / 0 FAIL / 1 SKIP.
- Identity: 279 PASS / 0 FAIL / 0 SKIP.
- Collector Pilot: 47 PASS / 0 FAIL / 0 SKIP.
- Combined xUnit: 415 PASS / 0 FAIL / 1 SKIP.
- Node: 11 PASS / 0 FAIL / 0 SKIP.
- The skipped A12WorkerEntryPoint is child-only. The parent executes that method in actual worker processes, including two hard-killed workers and two recovery workers. Child runs are not added to the parent count.
- SQL cleanup: 0 owned fixture DBs and 0 generated A.11 logins.
- UI raw UTC: 2026-11-10 08:57:02, including sanitized network paths/statuses. SQL evidence records session revoke, MFA, challenge consumption and paired events.
- Targeted source/history scan checked 59 file snapshots, with 0 known key/token/credential/IP findings. Scope includes all A.12 technical snapshots and deliverable Markdown. No broad third-party security certification is implied.

## Reproduction

Export the exact source, restore packages and provide installed browser dependencies via NODE_PATH. The runner pins native simulator artifact hashes. Use a new evidence run ID.

```powershell
$env:FDC_RUN_A11_RUNTIME = '1'
./tools/Invoke-A10DisposableRegression.ps1 -ConfirmDisposable -RunId a12-replay -NativeLibrary <pinned-dll> -Simulator <pinned-exe>
node --test tests/Fdc.Center.Tests/browser/management-errors.test.mjs
```

The runner accepts only local Windows-integrated fixture administration, creates random ownership-marked SQL DBs, and verifies cleanup. Separately verify generated login cleanup using local integrated SQL and pattern `fdc[_]a11[_]%`. Do not substitute a shared DB.

## Retained failed attempts

- `tmp/a12ui/tmp/focused` and `focused-02`: browser test expected successful Back; later assertions require observed HTTP 401 and no protected content.
- `tmp/a12ui/tmp/focused-03`: corrected UI PASS, persisted BFCache still not observed.
- `tmp/a12ui/tmp/phase4-4ca10/a12-ui-final`: legacy policy test failed because forms existed while disabled. Source fixed to create forms only on ACTIVE, original assertions retained.
- `tmp/a12release/tmp/phase4-4ca10/a12-release`: one COMTRADE fixture cleanup file-lock failure. Same test passed three isolated reruns in `tmp/a12release/tmp/file-cleanup-recheck`, and passed in the final full run. Lock owner/root cause remains unproven.
- `tmp/a12worker/tmp/focused` and `tmp/a12worker/tmp/phase4-4ca10/a12-final`: earlier process/combined coverage on source 65dc0fc. Historical runs excluded from final totals.

Raw logs, screenshots and TRX remain local for authorized reviewers. Hashes below identify the retained final files. TRX entries are identified by suite and hash.

```text
Fdc.Collector.Pilot.Tests.ComtradeParserTests 9caf29995d5b0e242e4da8b2c95848a639b45bb1bf7df036af4fdbcd7148e905
Fdc.Identity.Tests.RoleCrudSqlLabTests 2871ce7254cb182a3072f4f86f30d51dc992391ac03465ec60af92747a448d12
Fdc.Center.Tests.A10LabWorkflowTests 031be370e1f27f7a70c8ab55324409b4f3f147b4351607b256e80e544617e074
2652c1bb7581e29a49eccc3e72521e53e85a69707ffbc8ebe1f9525c3e085d20  build.log
05e072a096ba2d0d846136a29dbe0b6dfd0059fa49fd6283d69c6830206c5cd4  tests.log
13bf7b3039c63bf5a50491fa3cfd8eb4e699d1ba1436315aef9cbe5711530354  cleanup.log
13bf7b3039c63bf5a50491fa3cfd8eb4e699d1ba1436315aef9cbe5711530354  runtime-login-cleanup.log
c7974ebeaf64d2b6ff74476fa1a8879cef6a83e2072cd43d7520622bec26a0ee  node.log
971663a4070d5da4931407bca2a7b1a5165ee27a20940e723c2c38d973d29262  a12-mtls-revocation.json
c81b9abaa37a47e44356fd53b25356bf6caad9d5b09d0d0978175baf199c0e89  a12-worker-process.json
3745ff46cce44bfe74e3bb8717038e0f2eca318115ce8f185edb4dbd176d8670  a12-auth-ui.json
6a199b80cb72f963cc121059d2139a5b2bcb187737dfc5beb6bf7d22d5f8a983  a12-auth-ui-sql.json
```
