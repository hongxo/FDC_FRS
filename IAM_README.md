# Standalone IAM import

This repository initially contained only README.md. The IAM library, tests and offline schema generator are imported from the local FDC/FRS source with explicit project-owner authorization. Original Center/Collector sources and runtime data/configuration are excluded. The original directory had no Git history; this import makes no claim of verified shared history.

Build: dotnet build Fdc.Identity.slnx
Test: dotnet test tests/Fdc.Identity.Tests/Fdc.Identity.Tests.csproj
Requires stable .NET10 SDK. No startup migration, user/password/permission seed or default administrator.

Phase2 delivery is PARTIALLY_COMPLETED/BLOCKED_BY_POLICY. The library is not connected to any running Center. See docs/auth-d12-backend-implementation/phase-2/Phase2_Implementation_Report.md for implemented components and unresolved policy gates. The library README describes the earlier foundation snapshot.

SQL tests skip unless FDC_IAM_CONFIRMED_LAB=FdcIam_DevelopmentLab and FDC_IAM_LAB_CONNECTION is supplied externally. They also enforce the exact local host/database/disposable labels and use rollback-only synthetic rows. Never supply production credentials. No test account or fixture password is installed.
