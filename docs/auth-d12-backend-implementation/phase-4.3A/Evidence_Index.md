# Evidence index

Baseline d5ebc2a. Symbol anchors are immutable source references; source comments chưa cập nhật không thắng startup/DI. Historical PASS chỉ đọc lại từ phase reports/raw hashes, không runtime rerun trong 4.3A. Current new executions làread-only SQL inventory, source/workbook inspection và documentation QA.

## Source anchors

| ID | Source/symbol |
|---|---|
| S01 | [src/Fdc.Center/CenterApplication.cs:12](https://github.com/hongxo/FDC_FRS/blob/d5ebc2acc18265786bae5af96ef3ba58c223e522/src/Fdc.Center/CenterApplication.cs#L12) |
| S02 | [src/Fdc.Center/CenterIdentityRuntime.cs:17](https://github.com/hongxo/FDC_FRS/blob/d5ebc2acc18265786bae5af96ef3ba58c223e522/src/Fdc.Center/CenterIdentityRuntime.cs#L17) |
| S03 | [src/Fdc.Identity/Api/HumanSessionHandler.cs:13](https://github.com/hongxo/FDC_FRS/blob/d5ebc2acc18265786bae5af96ef3ba58c223e522/src/Fdc.Identity/Api/HumanSessionHandler.cs#L13) |
| S04 | [src/Fdc.Identity/Application/AuthenticationService.cs:53](https://github.com/hongxo/FDC_FRS/blob/d5ebc2acc18265786bae5af96ef3ba58c223e522/src/Fdc.Identity/Application/AuthenticationService.cs#L53) |
| S05 | [src/Fdc.Identity/Security/AuthenticationPrimitives.cs:11](https://github.com/hongxo/FDC_FRS/blob/d5ebc2acc18265786bae5af96ef3ba58c223e522/src/Fdc.Identity/Security/AuthenticationPrimitives.cs#L11) |
| S06 | [src/Fdc.Identity/Application/MfaContracts.cs:9](https://github.com/hongxo/FDC_FRS/blob/d5ebc2acc18265786bae5af96ef3ba58c223e522/src/Fdc.Identity/Application/MfaContracts.cs#L9) |
| S07 | [src/Fdc.Identity/Security/PermissionEvaluator.cs:6](https://github.com/hongxo/FDC_FRS/blob/d5ebc2acc18265786bae5af96ef3ba58c223e522/src/Fdc.Identity/Security/PermissionEvaluator.cs#L6) |
| S08 | [src/Fdc.Identity/Infrastructure/SqlCurrentAuthorityReader.cs:27](https://github.com/hongxo/FDC_FRS/blob/d5ebc2acc18265786bae5af96ef3ba58c223e522/src/Fdc.Identity/Infrastructure/SqlCurrentAuthorityReader.cs#L27) |
| S09 | [src/Fdc.Identity/Application/OperationPermissionResolver.cs:7](https://github.com/hongxo/FDC_FRS/blob/d5ebc2acc18265786bae5af96ef3ba58c223e522/src/Fdc.Identity/Application/OperationPermissionResolver.cs#L7) |
| S10 | [src/Fdc.Identity/Infrastructure/SqlRoleCrudService.cs:11](https://github.com/hongxo/FDC_FRS/blob/d5ebc2acc18265786bae5af96ef3ba58c223e522/src/Fdc.Identity/Infrastructure/SqlRoleCrudService.cs#L11) |
| S11 | [src/Fdc.Identity/Application/IndependentReviewWorkflow.cs:7](https://github.com/hongxo/FDC_FRS/blob/d5ebc2acc18265786bae5af96ef3ba58c223e522/src/Fdc.Identity/Application/IndependentReviewWorkflow.cs#L7) |
| S12 | [src/Fdc.Identity/Infrastructure/SqlAuthorityCommitBoundary.cs:9](https://github.com/hongxo/FDC_FRS/blob/d5ebc2acc18265786bae5af96ef3ba58c223e522/src/Fdc.Identity/Infrastructure/SqlAuthorityCommitBoundary.cs#L9) |
| S13 | [src/Fdc.Identity/Application/SqlCommitGatePolicy.cs:22](https://github.com/hongxo/FDC_FRS/blob/d5ebc2acc18265786bae5af96ef3ba58c223e522/src/Fdc.Identity/Application/SqlCommitGatePolicy.cs#L22) |
| S14 | [src/Fdc.Identity/Infrastructure/SqlSessionRevocationService.cs:8](https://github.com/hongxo/FDC_FRS/blob/d5ebc2acc18265786bae5af96ef3ba58c223e522/src/Fdc.Identity/Infrastructure/SqlSessionRevocationService.cs#L8) |
| S15 | [src/Fdc.Identity/Infrastructure/Entities.cs:79](https://github.com/hongxo/FDC_FRS/blob/d5ebc2acc18265786bae5af96ef3ba58c223e522/src/Fdc.Identity/Infrastructure/Entities.cs#L79) |
| S16 | [src/Fdc.Center/CenterPersistenceRegistration.cs:7](https://github.com/hongxo/FDC_FRS/blob/d5ebc2acc18265786bae5af96ef3ba58c223e522/src/Fdc.Center/CenterPersistenceRegistration.cs#L7) |
| S17 | [src/Fdc.Center.Persistence/SqlRecordIngestion.cs:42](https://github.com/hongxo/FDC_FRS/blob/d5ebc2acc18265786bae5af96ef3ba58c223e522/src/Fdc.Center.Persistence/SqlRecordIngestion.cs#L42) |
| S18 | [src/Fdc.Center.Persistence/SqlSoeIngestion.cs:8](https://github.com/hongxo/FDC_FRS/blob/d5ebc2acc18265786bae5af96ef3ba58c223e522/src/Fdc.Center.Persistence/SqlSoeIngestion.cs#L8) |

## Reports vàread-only evidence

Historical:../phase-4/Phase4_Final_Report.md, Defect_Register.md, DR01_DR29_Execution_Matrix.md; ../phase-4-1/Phase4_1_Final_Assessment.md; ../phase-4.2/Phase4_2_Final_Assessment.md và../../database/fdc-center-lab/Database_Final_Assessment.md. Full Input_Manifest lưu hash, phân biệt OFFICIAL_CHECKOUT/EXTERNAL_WORKSPACE_INPUT; external AUTH-R2/approval/catalog workbooks không giả vờ đã nằm trongGitHub.

| Local artifact (ignored tmp/phase4-3a-evidence) | SHA256 | Scope |
|---|---|---|
| AUTH_R2_Decision_Matrix.json | 886ad71def864b036d04a275aa9579211071e23d871b52028793b87cb17f24e8 | Read-only/source artifact; no credentials |
| AUTH_RBAC_Station_Scope_Design.json | 2ddd058441b883968bb402135148ff0f2feccf70c12adb656e2c03a27fa3da4b | Read-only/source artifact; no credentials |
| FDC_Center_Lab-owners.txt | 63d4af94c8e7a29f71e11e17fc1957689f4c2088f0c17dd91d9f3d494ab1db38 | Read-only/source artifact; no credentials |
| FDC_Center_Lab-read-only.txt | f9506952f6f460c2ba1793a83b104754753af318e399880461b7111e3597062e | Read-only/source artifact; no credentials |
| FdcIam_DevelopmentLab-owners.txt | 58a77148082626b0c06536bbe3b0ac33f12444190a0364dd8be1f9cb17162bab | Read-only/source artifact; no credentials |
| FdcIam_DevelopmentLab-read-only.txt | 7ba4025c80036c305e0ee517f311244466c900aea6ff6ae60081b5f0b0aeeac4 | Read-only/source artifact; no credentials |
| input-manifest.json | fc52f867a1a1ddaa975b9dad5241def23a652d77a3b3a7d522b623ea1a5b6803 | Read-only/source artifact; no credentials |
