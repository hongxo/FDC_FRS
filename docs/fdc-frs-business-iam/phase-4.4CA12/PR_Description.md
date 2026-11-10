# A.12 security integration follow-up

Two worker OS processes can contend for the same export lease. The integration test releases them from a shared claim barrier, checks that SQL records one owner, and lets both attempt release. Exactly one release and one audit/outbox pair were observed. The losing release and the winning worker's replay returned false.

The security branch is stacked on Draft #24. The source change is confined to the worker test harness. Existing canonical IAM contracts were exercised on the baseline because the IAM V1 ref was absent from local and checked remote branches. The separate user-role UI experiment is outside this branch.

The follow-up closes A11-043 and A11-061 at isolated scope. Mandatory E2E remains 10 PASS and 10 BLOCKED. G1/G2 remain BLOCKED and G3/G4 have no approval. Production remains NO-GO.
