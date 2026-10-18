# Batch 00.01 — patch application guide

**Based exclusively on:** `BESHstatNG(20261013-183240).zip` (13 October 2026). Apply against this ZIP's unmodified file layout; do not apply blind to later checkpoints.

**Includes:** one updated test project file, one new MSTest source module, five JSON semantic fixtures, two manual host CSV inputs, three inventory/mapping CSV files, inventory verifier, and architecture/acceptance documentation. No production source was edited.

## Apply one of the two deliverables

- **Patch approach:** from the root of the extracted baseline project, run `git apply --check BESHStatNG_Batch_00.01.patch`, then `git apply BESHStatNG_Batch_00.01.patch`. The patch paths are relative to the solution root. (Place the patch somewhere accessible and pass its actual location.)
- **Replacement approach:** unzip `BESHStatNG_Batch_00.01_UpdatedFiles.zip` into the **solution root**, merging directories and replacing `BESHStat.Excel.Common.Tests/BESHStat.Excel.Common.Tests.vbproj`. Do not overwrite the full solution with the small bundle.
- Use **either** approach; do not apply both.

Then run `python tools/verify_stage304_inventory.py`, `dotnet test BESHStat.Excel.Common.Tests/BESHStat.Excel.Common.Tests.vbproj -c Release`, your existing Core tests and Windows solution/legacy test suite. Follow the Excel-DNA and Excel Online smoke instructions in `docs/architecture/shared-excel-contracts.md`.

The patch author validated inventory checks and patch application in a clean extraction. The .NET SDK is not installed in the patch-creation environment, so the new VB.NET tests and GUI smokes **must be executed locally** before the batch is accepted.

**Rollback:** reverse the patch (`git apply -R`) or restore the original test `.vbproj` and remove the added files. Next batch after approval: **00.02**, analysis IDs/descriptors.
