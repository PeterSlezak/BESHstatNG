# Official 04A.36 owner review

**NOT_READY_04A36 — software implementation and validation complete; external documentation closeout remains unresolved.** No acceptance, staging, commit, merge, push, publication, deployment or 04A.37 work occurred.

The remaining holds are outside the implemented chart payloads: concurrent Master Plan/project README changes require owner attribution, and full `git diff --check` reports the two-space Markdown line break on Master Plan line 3. Batch-scoped `git diff --check` passes. Those external bytes have been preserved; this batch does not amend the owner's plan. See [readiness](../../evidence/04A36/readiness.json), [external changes](../../evidence/04A36/external-changes.json) and [Git receipt](../../evidence/04A36/git-final.json).

## Verified base and authority

Branch `codex/04A-36` starts from actual local `main`/HEAD/merge-base `92f5916d9dc0aaa727c99c0ceea9c89c10eefef3`. Its parent `6f1a27dc2a3aaa3f58326557c087e508344500b8` contains accepted 04A.35; the subsequent owner commit renames the Windows-prohibited `aux.py` to `auxilary.py`. The old 04A.35 manifest is unchanged: its 78 external entries were verified against committed main with exact/inverse-EOL checks and that explicit owner filename alias. This is single-parent integration evidence, not an invented two-parent merge or ZIP authority. The earlier `f6b605b4a5fc3ffe4ab5f68ddb1f7ef75071311e` is historical, not this batch's base.

Entry worktree/index were clean. The independently tracked baseline contains 4,357 files. Revision 2.12 was read at entry; external Revision 2.13 and README status edits appeared during work and are separately inventoried. The 04A.36 objective/native restrictions remain consistent. AGENTS, the 04A.13/21–24/29/34/35 authorities, canonical 04A.29 v2 and relevant current sources were inspected. [Entry authority](../../evidence/04A36/entry-authority.json), [prior publication](../../evidence/04A36/prior-04A35-publication-check.json) and [authority reading](../../evidence/04A36/authority-reading.json) retain the hashes and qualifications.

## Implemented contracts and real consumers

| Family | Portable preparation | Shipping Windows consumption and retained boundary |
|---|---|---|
| KM | Core `KaplanMeierSeriesGeometry36`/`KaplanMeierSeries36`: independent survival, censor and upper/lower CI vectors; original origin, clipping, padding, source-group and source-slot metadata. Common `SurvivalChartPresentation36`: late localized series names, unit/axis/legend intent. | Both `KaplanMeierPlotExcel.Plot` routes reach per-phase preparation immediately before the original native series creation. `AddKMplot` remains callable. Windows retains chart/series creation, colors, markers, lines, title/axis APIs, legend deletion and errors. Empty native censor slots and malformed sparse censor-array failures remain. |
| Cox baseline | Core `CoxCurveGeometry36`: exact null/mismatch validation, accepted axis scaling and separate detached vector copies. Common supplies original baseline/title/axis constants. Existing `CoxCurveData` remains authoritative. | `CoxSurvivalPlotExcel.PlotCox` uses validation/scaling before chart creation and copies X and Y independently at their original late assignment points. Worksheet guard, empty return, optional coordinates, native series/style/catch behavior and array-read timing remain. No fitting or baseline recomputation. |
| Cox residual metadata | Lazy `CoxResidualData.ResidualPlotAvailability()` exposes copy-safe blocks/numeric cells and typed Score series only where explicit original record.Index keys join existing input RowIds. Dictionary order is preserved. Scalar/Schoenfeld blocks without a proven time join report `NOT_SUPPORTED_BY_EXISTING_SOURCE`. Nested metadata arrays are detached. | Existing report/table assembly remains unchanged. This is data/availability metadata; **no new residual chart consumer or GUI capability is claimed**. `Ui4Cox.ckResidualPlots.Enabled=False` and its unwired state remain unchanged. |
| SPC | Existing ten-channel/point/stage/exclusion policies reused. Common `SpcPanelPresentation36`/`SpcSeriesIntent36`: ordered finite admission, references/spec/target, zone modes, legend intent, last-value label ordinals and extents. Core `SpcZoneBands36`: seven band vectors and original zone eligibility. | Existing Ui21 and Ui22 paths both reach shared `SpcControlChartExcel.RenderPanel`, which translates the prepared roles/vectors. Native Series/Shapes, blank VARIANT marshaling, RGB/font/marker/dash/weight, secondary axes, measured stage pixels, legend edits and rollback remain Windows. Existing reflective bounds/eligibility adapter tests continue to execute the real delegating methods. |

Core/Common still target `netstandard2.0`; Windows/tests remain .NET Framework 4.8. All six new portable production files use `Option Strict On` and have no host, drawing or filesystem dependencies. Existing statistical engines, UI/Designer/UDF/Ribbon/help catalogs and framework targets were not edited. The existing Common report file adds only lazy residual availability access; its original member tokens are preserved. Required test Compile links are additive.

The [source transitions](../../evidence/04A36/source-transitions.json) inventory all 20 proposed production/test/project/dossier paths. The [contract inventory](../../evidence/04A36/coordinate-and-chart-spec-contract.json) lists public members, shapes, copy policies and ordinals. Numeric source-slot `-1` means synthetic KM origin; `-2` means original unassigned padding. Other slots are zero-based input array positions. They are never invented worksheet rows. Existing SPC source identity, aligned plot ordinal and native one-based point index remain distinct.

The [current crosswalk](../../evidence/04A36/member-ownership.csv) accounts for all 106 entry renderer members with zero in-scope OPEN. The [historical crosswalk](../../evidence/04A36/historical-crosswalk.csv) retains all 130 affected 04A.29 IDs, including native fields/enums and accepted 04A.34 `HasNaturalZero` relocation into Core. Removed private selection/scan helpers have explicit successors; changed private typed arguments have explicit header transitions. No false same-file match is used. This is a three-family 04A.36 delta; the accepted 17-family 04A.34 map is unchanged.

## Compatibility authority and limits

Original adapter sources and their numerical blocks were frozen before production edits, with source/body SHA-256, standalone compiler bindings and invariant/`sk-SK` fixtures. The original 5,472-row KM/Cox/SPC-band corpus remains immutable. It includes intentional malformed/null/nonfinite/shape rejection cases, signed zero and exact IEEE vectors. These are executed source slices, not live Excel runs; some malformed slice calls would be unreachable after an earlier renderer guard.

The first synthetic SPC panel fixture omitted matching panel signals for point rule numbers. V1 has 378 Core construction rejections and only six valid outputs per 384-row corpus; it provides insufficient series coverage. Those sources, outputs and failed test attempts are retained. [Versioned correction](../../evidence/04A36/fixture-correction-v2.json) supplies corresponding immutable signal metadata, compiles the unchanged archived preparation against entry assemblies, and retains V1. V2 has 330 valid outputs and 54 genuine construction rejections per 384-row corpus. It was produced after the initial extraction and is explicitly qualified as a corrected archived-source reference, never a pre-edit timestamp claim or a new-Core oracle. Old 04A.22 independent axis arithmetic is compiled separately.

| Per configuration | Rows | Final comparison |
|---|---:|---|
| Original KM/Cox/SPC bands | 5,472 | Exact bytes |
| Corrected SPC ordered series/options | 384 | Exact bytes |
| Corrected accepted legacy SPC axes | 384 | Exact bytes |
| **Total Debug / Release** | **6,240 each** | **Zero differences** |

[Comparison receipts](../../evidence/04A36/compatibility-comparison.json) retain each input/oracle/actual digest, exception type/parameter, cardinality and comparison policy. Double comparisons preserve raw IEEE bits, including signed zero and NaN payloads; no tolerance or locale was relaxed. Frozen generator source is authoritative; the expanded input JSON replays those immutable entry generators. Captions, defensive copies, repeat/concurrent access, actual fitted SPC families, explicit Cox residual key joins and compiled adapter delegation have focused tests. Earlier accepted 04A.13/21–24/30–35 regressions remain in the full suite.

[Renderer preservation](../../evidence/04A36/renderer-preservation.json) records token/header checks. SPC native area/line/marker creation, chart/secondary-axis configuration, point/label application, stage measurement and cleanup helpers are token-identical. Its band drawing tail is identical after the numeric prefix. The RenderPanel role translation preserves the original style/order mapping. KM/Cox retain original native loops/tails with prepared-value substitutions. These are source/managed proofs; **native visual equivalence is NOT_RUN**.

## Executed validation

| Lane | Accepted 04A.35 reference | Fresh merged-main entry | Final 04A.36 |
|---|---:|---:|---:|
| Managed Debug | 20,374 PASS / 14 FAIL | 20,374 PASS / 14 FAIL | **20,457 PASS / 14 FAIL** |
| Managed Release | 20,374 PASS / 14 FAIL | 20,374 PASS / 14 FAIL | **20,457 PASS / 14 FAIL** |
| New raw managed cases | — | — | **83 PASS each** |
| Node | 916 PASS | 916 PASS | **916 PASS** |
| Python aggregate | 813 PASS / 132 FAIL / 11 ERROR | **812 PASS / 132 FAIL / 12 ERROR** | **808 PASS / 137 FAIL / 11 ERROR** |
| Strict / profile | exits 1 / 1 | exits 1 / 1 | **exits 1 / 1** |
| Strict MkDocs | exit 0 | exit 0 | **exit 0** |

The same 14 managed nonpass identities/outcomes and every previous raw test multiplicity are preserved. New managed counts are Core 13, Common 27 and Framework 43. No managed case was removed or relabeled. Full Framework runs were serial; the prior 04A.35 concurrent deadline failures remain historical evidence and were not waived.

Fresh entry Help recorded one real 120-second wrapper timeout. Its unchanged seven-case serial recheck and the final full 16-case Help suite pass. The failed attempt remains visible. Final Python has three additional authorized source-scope/pin failures: the old 04A.13 full renderer-tail assertion and the old 04A.28 Cox/KM whole-file hashes. Two further failures arise from external README edits: the old Core-directory dirty-scope assertion and the old architecture README pin. They remain actual FAILs, separately classified; no old assertion was changed. All inherited source-inspection errors, including the unordered historical SPC lookup, remain visible. [Nonpasses](../../evidence/04A36/nonpasses.json), [Python identities/dispositions](../../evidence/04A36/source-and-node-validation.json), [raw managed identity multiset](../../evidence/04A36/managed-test-identities.csv) and [validation](../../evidence/04A36/validation.json) provide exact identities and deltas.

Commands are recorded with actual exits, configurations and output hashes in [command receipts](../../evidence/04A36/command-receipts.json). Reproducible entry/final invocations use the existing isolated runner through `evidence/04A36/run.py build|managed <Debug|Release> <entry|final>`, `auxiliary.py source|contracts|captures final`, focused `Graphics36` filters, and `run.py serial Debug final`. Compiler/source-reader and capture PowerShell scripts are included. The original `verify-beshstatng.ps1` wrappers are **NOT_RUN**: they share historical outputs, and Legacy Build includes unapproved packaging hooks. Equivalent isolated Core/Common/Office/Framework scopes and compile-only Windows targets executed. No packaging pass is inferred. Failed development compiler/test/fixture attempts are preserved in separate receipts.

The independent packet audit/verifier outcome and manifest row count are in [audit results](../../evidence/04A36/audit-results.json) and [publication manifest](../../evidence/04A36/publication-manifest.json). Those audits protect source/identity coverage, honest readiness, gates, frozen fixtures and missing/modified/foreign/duplicate/traversal/case/reserved-name/size/hash/self-entry tampering. They are reported separately from the existing Python aggregate.

## Preservation and privacy

[Preservation](../../evidence/04A36/preservation.json) compares actual repository bytes against the 4,357-entry tracked baseline, distinguishing six authorized tracked batch changes from concurrent external documentation edits. Original test sources, dated dossiers, evidence, manifests, canonical 04A.29 v2 and earlier numerical engines remain unchanged. [Lineage](../../evidence/04A36/lineage-check.json) checks all **278** protected source pins with explicit current successor transitions. [Mixed-model captures](../../evidence/04A36/mixed-model-captures-check.json) rechecks **100/100** original hashes. [Historical census](../../evidence/04A36/historic-census-metadata.json) verifies membership/size/mtime metadata only; the approximately 138 GB historical payload population was not rehashed, modified, pruned, compressed or archived. Repository proof is distinct from any smaller ZIP subset.

Public artifacts exclude raw build logs, binaries, caches, private historical payloads and absolute user paths. Historical private path labels are redacted with digest identities retained. The local entry Master Plan copy is excluded from publication; source hashes and the external-change record provide authority without proposing a plan artifact. [Privacy](../../evidence/04A36/privacy-check.json) and the independent manifest verifier check the complete proposed publication scope, including exactly one null, nonrecursive manifest self-entry.

## Retained issues and unexecuted work

`C13-LOGRANK-R-01`, KM sparse censor-axis behavior, Cox empty-stratum/error behavior and disabled residual controls remain restricted. Existing SPC phase/exclusion/native obligations, historical `HasNaturalZero` source-inspection issues and canonical 04A.29 helpers 171/172 uncertainties remain unchanged. Newly characterized KM CI padding/empty-censor slots and Cox late-array/earlier-axis timing are compatibility candidates, not corrected scientific behavior. See [deferred issues](../../evidence/04A36/deferred-issues.json).

**P=NO; N1=NO GO; N2=NO; Gate C2=UNMET; Gate C3=UNAPPROVED.** Zero original-control releases/discharges. Every software comparison is **NOT_GOLDEN**.

**NOT_RUN:** live Excel/COM/UIA/RefEdit; chart visual/error-bar/data-label verification; R oracle/scientific validation; Cox residual GUI; Office/Mac/browser manual UI; installer/signing/packaging/deployment; original shared-output wrappers. Owner-gated smoke proposals are KM multi-group censor/CI on-off; Cox baseline/stratum plus confirmation residual control stays disabled; SPC phase/exclusion/signals with all four zone modes and limit labels. These need reviewed-versus-loaded native identity and separate authorization.

## Owner continuation checklist

1. Resolve attribution of concurrent README/plan changes and the Master Plan line-3 whitespace finding without overwriting owner work. Recheck actual Git provenance and reseal changed packet metadata.
2. Independently inspect source, frozen/V2 authorities, real consumers, identity-qualified failures and all publication hashes from a fresh project ZIP. No software acceptance is implied by this dossier.
3. Keep owner documentation changes separate from the 20 proposed batch source/test/project/dossier paths and manifest-listed evidence; exclude ignored/private outputs and the local Master Plan copy.
4. Only the owner may authorize staging/commit/merge after acceptance. No 04A.37 work or native/scientific gate promotion is included.
