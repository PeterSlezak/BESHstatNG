# Official 04A.02 — Checkpoint 04 source-only G3/G4 preparation

**04A.02 Checkpoint 04 SOURCE-ONLY G3/G4 PREPARATION / OWNER REVIEW REQUIRED; official 04A.02 NOT ACCEPTED.** This continuation authorizes source specifications and approved Python/managed tests only. Scientific method remains PENDING; independent reference execution, numerical promotion and native capture remain NO. No R/Rscript command, including a version probe, or native host was executed.

## Baseline and exact scope

Verified branch `codex/04A-02`, HEAD and main `12e25924aae93f0b36ab33937176c86c19641b50`, inventory ancestor `ec5e47a0cfc352ea5fa9fcd598e1f10b5b8fbd9b`, linear history, empty index and all 30 Checkpoint 03 paths/SHA-256 values against its final evidence. No archive was extracted. Owner-accepted 04A.01 remains accepted with restricted deferrals despite stale historical acceptance wording.

Four new paths only; no original Checkpoint 01/02/03 source/document bytes are edited:

```text
BESHStat.Excel.Common.Tests/TestData/04A02/semantic-table-source-spec-v1.json
BESHStat.Excel.Common.Tests/TestData/04A02/windows-option-chart-source-spec-v1.json
BESHStat.Excel.Common.Tests/src/test_04a02_source_specs.py
docs/architecture/batch-04A-02-checkpoint04-review.md
```

The JSON files follow existing Common test-data conventions, avoiding changes to the documentation allowlist or project file. The Python file is explicitly executed as a source-only test, not added to managed compilation or existing automatic test discovery. Cumulative paths: 34 (two pre-existing tracked modifications, 32 untracked); no staging, commit, merge, push or branch movement.

## G3 candidate semantic specification

[Semantic source specification](../../BESHStat.Excel.Common.Tests/TestData/04A02/semantic-table-source-spec-v1.json) adds `04A02-ANOVA-SEMANTIC-SPEC-01` and `04A02-KW-SEMANTIC-SPEC-01`, children of the existing `04A02-TABLE-SEMANTIC-01` coverage obligation. It references exact frozen C30 datasets/options/hashes and shipping source anchors. Numeric results are explicitly UNVERIFIED_AGAINST_INDEPENDENT_ORACLE; expected numerical references remain null.

- ANOVA: primary 4×6 assembled table; authored top/left headers, SS/df/MS/F/P order, Double versus Int32 body types, unassigned null cells, body-relative p-column 5. No Welch/post-hoc admitted by these explicit options. Absent Welch getters can return NaN; that is not an approved reference or a JSON finite number.
- KW: primary 7×3 table; exact six body-row labels, Int32 counts and Double statistics/p-values, empty-string annotations, and p-cells (4,2)/(6,2). Raw H/p and tie-corrected H/p stay distinct. The authored “Two sided” labels are preserved; scientific interpretation remains a separate pending decision.
- ResultTable assembly/OutputModel section metadata and Common request normalization are separate: null stays null, finite numerics stay numeric, and Double/Single nonfinite values become authored `#N/A`/`#Pinf`/`#Ninf` text in the request factory. This does not approve singular-input behavior.
- Document Id/Kind/Caption, table snapshots, diagnostic roles, layout order/one-based bounds/default gap and p-format metadata are source specifications. Exact case-specific p-number format/threshold/highlight presentation remains pending. Invariant/sk-SK checks retain numeric types and authored labels; there is no approved localized text or rendered workbook golden.

All Stage304/OutputSemantics/OutputLayout reference files are hash-pinned context, read without rewriting. Production GroupedNumericData carries GroupNames/Groups/GroupCounts, never provenance RowIds.

## G4 source-derived future capture plans

[Windows source specification](../../BESHStat.Excel.Common.Tests/TestData/04A02/windows-option-chart-source-spec-v1.json) keeps all four native IDs NOT_RUN / BLOCKED and evidence SOURCE_INFERRED_NOT_NATIVE_VERIFIED. It pins designers, handlers, downstream source and line-addressed excerpts. Every exact blocked ID and its `control:` obligation link remains present, including shared-form controls belonging to other modes; no applicability reduction or release is inferred.

| Future case | Exact form-wide blockers | Significant option mappings | Capture plan |
|---|---:|---:|---|
| 04A02-ANOVA-NATIVE-01 | 141 UibyID | 15 | Shared flags/primary tables, optional host supplementary tables, box/MCP chart configuration |
| 04A02-NORMALITY-NATIVE-01 | 141 UibyID, same IDs | 15 | SW/DA/AD tables and NA branches, descriptive/box outputs |
| 04A02-2X2-NATIVE-01 | 2 Ui82x2 | 16 | Screen/worksheet counts, Fisher/exact/mid, Pearson/association, Liddell/OR/RR and CI text; no chart call in handler |
| 04A02-PCA-NATIVE-01 | 75 Ui11PCA | 13 | Predictor/label/group input, matrix/extraction/convergence settings, Data/PCA results tables, guarded score/loading/scree/biplots |

There are 218 distinct form-wide blocked IDs across the three forms; ANOVA and normality share the same UibyID restrictions. The exact shared `global_refedit_edge` remains `BESHstatNG/src/UI/Refedit/Excel2007RefEdit.vb|WithEvents|xlApp@34`, UNKNOWN_HIGH, with 04A.27/04A.45–47 owners. Each native release still needs exact-ID mitigation, executed regression evidence, explicit owner release AND disposable-workbook authorization. Static inspection is allowed; constructors/events were not invoked.

Designer defaults are not claimed as observed runtime state. UibyID designer enables Welch/box/descriptives and defaults to ByID/new worksheet; these differ from the frozen typed classical-only options. Constructor/global preferences/tag/event overrides need future before/after capture. 2x2 defaults to screen counts 10 and new worksheet; PCA correlation/eigen extraction, maxIter 50/epsilon text .000001 are designer source facts, not approved scientific settings. Normality excludes n=5000 (`n<5000`) although its NA text says `n>5000`; record this boundary discrepancy for separate review without modifying behavior.

Chart plans preserve source assignments and request ordered series/X/Y/type, axes bounds/units/titles, labels/legend/error bars/configuration, chart/plot rectangles in points and culture/Excel version. No native geometry, screenshot, chart value or successful click is fabricated. PCA component guards and eigenvector sign/tied-subspace comparison policy need future scientific review. Native tables also need rendered text/error cells/formatting/anchors and occupancy evidence.

## Evidence stewardship and interim issues

First exclusive root: `TestResults/04A-02-initial-fixtures-checkpoint04-20261030T164610Z/`, created before edits, census 133,004 historical files. Its 15-test source run and 41 managed tests passed. Full preparation refused because the new start record omitted the helper's required `source_pins` field (the live script hash did not change); its census also used `size` instead of the helper's `bytes` key. Neither record was overwritten. The initial census was subsequently checked with its actual schema and passed.

Corrected exclusive execution root: `TestResults/04A-02-initial-fixtures-checkpoint04-20261030T164948Z/`, complete census 133,209 including the first retained attempt, correct helper schema and original tracked-source hashes. This is a supplemental execution preflight, not a historical protection rebaseline. Both roots and every new output remain retained through Phase 04A.

An initial artifact-check adapter mistakenly resolved root-relative C01/C02 manifest paths under their evidence directories. Its missing-target diagnostics are retained; `prior-artifact-verification-corrected.json` fixes path interpretation and freshly passes 1,527 C01, 877 C02 and 875 C03 hashes. No protected file drift was found. Four newly observed invariant/sk-SK candidate files match C01 bytes exactly (`candidate-continuity.json`). No numerical values were promoted.

Original strict/profile audit statuses remain 1/1, NOT green; 24 historical failures remain diagnosed and unchanged. Their protected C03 command evidence was freshly hash-verified, not rerun in C04. Supplemental diagnostic exit 2 is not strict success. Gate C2 UNMET, 1,514 restricted obligations, 802 affected live captures, E1–E7 OPEN and GC3–GC8 DEFERRED remain unchanged.

## Remaining official batch and owner decisions

The complete coverage register/restriction crosswalk remains byte-identical, including ANOVA, normality, 2x2, PCA and Windows tables/charts/form states. Source specifications and Python admission checks do not satisfy independent numerical references or native regression acceptance. Still required: separately recorded method/oracle/runtime/hash/tolerance/edge decisions; separately authorized independent execution; raw candidate/version/hash/comparison/exception evidence and independent double review before any promotion; approved semantic output cases including diagnostics and presentation; exact-ID native release and disposable-workbook execution; full G5 independent review and owner approval. No later numbered batch starts.

The [C03 unsigned decision form](04A02/checkpoint03-owner-decisions.pending.json) remains unchanged. Method is PENDING; reference execution, golden promotion and native capture gates remain false. Manual smoke proposals only, NOT RUN: classical ANOVA with explicit Welch/MCP/host supplementary flags and occupied-output handling; PCA score/loading geometry and form-state capture after exact releases. Office/WASM automation does not establish either native result.

## Actual validation and commands

Both complete isolated verification wrappers exited 0, with sequential builds and no restore/package/network activity:

| Suite | Debug | Release | Failures / skips |
|---|---:|---:|---|
| Core managed | 1,024 | 1,024 | 0 / 0 |
| Common managed | 783 | 783 | 0 / 0 |
| OfficeAddin managed | 505 | 505 | 0 / 0 |
| Independent WasmSmoke execution | 45 | 45 | 0 / 0 |
| Legacy .NET Framework 4.8 | 790 | 790 | 0 / 0 |
| Distinct managed total | **3,147** | **3,147** | 0 / 0 |
| Python (27 tooling + 16 Help + 75 scripts) | **118** | **118** | 0 / 0 |
| Node (468 Office + 71 Help + 377 WasmSmoke) | **916** | **916** | 0 / 0 |

Additional focused tests: **17 new Python source-spec identities**, all passed in the final handoff run; **41 managed fixture identities**, all passed using a copied Common assembly/dependencies/TestData. These repetitions are not added to per-configuration totals. Checkpoint-wide Python coverage is 118 existing plus 17 new identities, not 135 executed by each full wrapper. No new managed code was added.

The 17 checks include actual source/fixture pins, every native control/option excerpt, wrong table/header/cell order, wrong CLR type, null-versus-text, unexpected Dunn default, authored culture/text shape, raw/corrected p-cell order, promotion refusal, missing blocker/global risk, fabricated initialization and chart geometry, and deterministic JSON roundtrip. No independent statistical expected value is derived from the service.

`$P` below is exactly `C:\Users\peter\Desktop\WPy64-31470\python\python.exe`; `$E` is the corrected execution root above. All execution names were new; replay needs a new root/census/name.

```powershell
& $P -B BESHStat.Excel.Common.Tests/src/test_04a02_source_specs.py
& $P -B scripts/verify-04a02-fixtures.py common --evidence $E --configuration Debug --name c04-focused-Debug --filter FullyQualifiedName~MigrationFixture04A02Tests
& $P -B scripts/verify-04a02-fixtures.py prepare --evidence $E --configuration Debug --name c04-full-Debug
powershell -NoProfile -ExecutionPolicy Bypass -File "$E/Debug/c04-full-Debug/verify-isolated.ps1" -Legacy -Wasm -Js -Python -VisualStudio 2026 -PythonExe $P -Configuration Debug
& $P -B scripts/verify-04a02-fixtures.py prepare --evidence $E --configuration Release --name c04-full-Release
powershell -NoProfile -ExecutionPolicy Bypass -File "$E/Release/c04-full-Release/verify-isolated.ps1" -Legacy -Wasm -Js -Python -VisualStudio 2026 -PythonExe $P -Configuration Release
& $P -B scripts/verify-04a02-fixtures.py summarize --evidence $E --name checkpoint04-complete
& $P -B scripts/verify-04a02-fixtures.py preservation --evidence $E --name final
git -c safe.directory=C:/Users/peter/source/repos/BESHstatNG diff --check
git -c safe.directory=C:/Users/peter/source/repos/BESHstatNG diff --cached --exit-code
git -c safe.directory=C:/Users/peter/source/repos/BESHstatNG status --short --untracked-files=all
```

`validation-commands.json` retains exact executed process arguments/exits; `full-Debug.log` / `full-Release.log` retain all suites. TRX paths relative to `$E`: `{Debug,Release}/c04-full-{Debug,Release}/results/{Core,OfficeAddin,OfficeExecution}.trx` and `executions/c04-full-{Debug,Release}-{Common,Legacy}/results/{Common,Legacy}.trx`; focused TRX `executions/c04-focused-Debug/results/Common.trx`.

`final-identities-and-source-protection.json` records exact managed executed names including data arguments and Python identities; method-only counting would collapse data-driven tests, so it is not used. `node-complete-identities.json` records all 916 per configuration, including the line omitted by the initial narrow duration parser. Full logs preserve the original lines and timings. `test-identities-checkpoint04-complete.json` covers all managed attempts. The first 15-test run and subsequent 17-test refinements remain retained. Existing legacy non-public-helper discovery advisory did not produce a skip/failure.

Common and legacy writers ran in copied per-execution sandboxes; TRX routing alone was not used as isolation. The production verification script remained source-hash pinned and unchanged. Legacy build-generated VersionAttributes bytes were retained separately for both configurations, then original source bytes restored exactly; no historical TestResults restoration or metadata manipulation occurred. All **2,174 tracked source hashes** and all **30 original C03 worktree hashes** match the preflight.

Final preservation **PASS: 133,209 historical files, zero errors**, free disk **502,817,243,136 bytes**, above warning/review thresholds. The earlier original 133,004-file census also passes. Preservation/scope/free-space evidence is recorded in `preservation-final.json` and `final-git-and-source-checks.json` below `$E`; `artifact-hashes.json` seals this new checkpoint evidence excluding itself. This is a complete metadata/file-identity census plus fresh source/prior artifact hashes, not a claim of rereading every historical byte.

**STOP — 04A.02 Checkpoint 04 SOURCE-ONLY G3/G4 PREPARATION / OWNER REVIEW REQUIRED; official 04A.02 NOT ACCEPTED.** Separate owner decisions remain required; no next checkpoint, numerical/reference/native release or 04A.03 is inferred.
