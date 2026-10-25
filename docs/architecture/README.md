# Engineering architecture and verification notes

This folder contains implementation decisions, contract descriptions, migration reviews, numerical/input/output invariants, testing instructions and historical acceptance reports. It complements the current source and project-level READMEs; it is not the public BESHStatNG help site.

## Where to start

- [`shared-excel-contracts.md`](shared-excel-contracts.md) describes the shared application boundary.
- [`development-validation.md`](development-validation.md) covers repository-wide verification, prerequisites and reproducibility.
- [`dotnet-10-migration.md`](dotnet-10-migration.md) describes the modern SDK/test/WASM toolchain update.
- [`gate-b-common-v1-contract-freeze.md`](gate-b-common-v1-contract-freeze.md) captures approved Common contract stability and exceptions.
- Additional topic-specific reports cover metadata, input parsing, output semantics, host lifecycle, Office readiness, selected-range acquisition, guarded writes, worksheet-based model specification, global Options/localization and Office Ribbon integration.

Some historical reports retain **pending** wording from the time they were authored. Read them as **historical evidence**, then consult the current [`cross-platform master plan`](../../planning/BESHStatNG_CrossPlatform_MasterPlan.md) and project READMEs for reconciled status. The accepted Office application-layer implementation precedes a separate formal Gate C exit review; accepted batches alone do not certify Mac/desktop deployment. A simulated browser test is not a substitute for real Excel Online/Mac/Windows desktop host acceptance.

## Documentation maintenance

When shared contracts or host behavior change, update the relevant report, add tests near the implementation and preserve the distinctions among computed statistical results, spreadsheet semantic planning and host-specific mutation. Do not publish raw workbook data, credential-bearing diagnostic traces or proprietary assets in development logs.
