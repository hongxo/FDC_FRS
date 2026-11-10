# A.12 security integration baseline

The security branch starts at `7d5400bb5176ac3b94533460ba4d2dea7c6f12e9` (Draft #24). The verified A.11 technical SHA is `c331cadbfde7d74075d86373496a858fd2e07547`. A.11 reported 34 PASS / 86 BLOCKED / 0 FAIL / 0 SKIP in its requirement matrix. The previous A.12 delivery reconciled eight original blocker IDs. This run keeps that baseline and records new closures separately.

`feature/fdc-frs-iam-admin-v1` was absent from local refs and the checked origin heads. The separate user-role UI experiment at `1209b90` is excluded from this branch. Pre-existing edits to management.mjs and its tests, and untracked local files, were excluded from the archive and preserved in the checkout.

See [original blocker IDs and current dispositions](FDC_A11_86_Blocker_Reconciliation.md).
