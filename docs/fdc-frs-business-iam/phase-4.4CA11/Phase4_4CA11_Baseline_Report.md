# Baseline A.11

- Base `d27171e`, Draft PR [#22](https://github.com/hongxo/FDC_FRS/pull/22). A.10 implementation ancestor `498f995`.
- A.10 reported 398 .NET PASS, 2 SKIP, 11 Node PASS and A.10 matrix 88 PASS, 20 BLOCKED, 2 SKIP. Local A.10 raw evidence is under `tmp/a10g/tmp/phase4-4ca10/delivery` in the outer workspace.
- A10-019 and A10-020 require the runtime SQL profile and were skipped by the disposable runner. A.11 exercises the same assertions through an explicitly owned fixture.
- Three existing working-tree changes were excluded from A.11 commits and clean-checkout verification. No real IED or shared SQL was used.
