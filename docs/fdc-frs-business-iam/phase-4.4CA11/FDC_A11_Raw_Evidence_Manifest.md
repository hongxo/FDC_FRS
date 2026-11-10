# Raw evidence manifest

Source: `c331cadbfde7d74075d86373496a858fd2e07547` exported using git archive. Working tree user edits excluded.
Local evidence directory relative to the delivery checkout: `tmp/a11clean/tmp/phase4-4ca10/a11-clean`. Raw files are retained locally for authorized reviewers. The repository records this manifest only. Browser traces, screenshots and TRX may contain machine metadata.

- Build: 0 warnings, 0 errors.
- xUnit: Center 86, Identity 279, Collector Pilot 47. Total 412 PASS, 0 FAIL, 0 SKIP.
- Node: 11 PASS, 0 FAIL, 0 SKIP.
- Cleanup: 0 owned fixture databases and 0 generated A.11 logins.
- Loopback browser, signed PKI, and pinned native simulator artifacts used. No production or shared Lab targets.
- Scoped scan: 441 Phase 4.4 changed/untracked paths, zero findings against configured known OT IP/credential values, restricted file types and key/token markers. This is a targeted scan.

Replay after package restore and configuring local Chromium through NODE_PATH:

```powershell
$env:FDC_RUN_A11_RUNTIME='1'
./tools/Invoke-A10DisposableRegression.ps1 -ConfirmDisposable -RunId a11-replay -NativeLibrary <pinned-dll> -Simulator <pinned-exe>
node --test tests/Fdc.Center.Tests/browser/management-errors.test.mjs
```

Runner checks owned DB cleanup. Separately check `sys.sql_logins` for the escaped SQL pattern `fdc[_]a11[_]%` using integrated authentication over `lpc:.`.

```text
818b9881ea3b3878bde54d12a3da56670b193f8da6541bc1941965ff97444c17  trx/ (file 1 by SHA-256)
379a0fccfbd158106f46b5b930101403a2f26ead11a896b5b9f42979fab90be2  trx/ (file 2 by SHA-256)
6132a5727188dc4a57f9847a2fe7122515d4769026fabe59599927277ae5db72  trx/ (file 3 by SHA-256)
9757f2f65f39218b2284741ee9ee8a536296d14efaac97b5d6b1abfc804cf3fe  build.log
6f82b6626272eb4b1d1592df0ec790ca60a92cc72cba308e050ad6b15bdf6de6  tests.log
13bf7b3039c63bf5a50491fa3cfd8eb4e699d1ba1436315aef9cbe5711530354  cleanup.log
13bf7b3039c63bf5a50491fa3cfd8eb4e699d1ba1436315aef9cbe5711530354  runtime-login-cleanup.log
f212c8a404a98914614b73dd3a64bc4bed5cb79c0bea962e71156f6d72949162  node.log
2dcb15f4acd9bbd53cd32547d9ca08ab0f6b1c60e2b6d0757bf7bd9184a28ddd  a11-export-lease.json
9dfccc3c6c5506629a005e7ea7eb5d41db2266fa7ae10b58ad259c2ce855a3a7  a11-export-quarantine.json
d2cb9b7ecade786dd1b7b4c540fd044fff2ad2b3e5de327cb4e616c3fec34bf5  a10-crl-mtls.json
```
