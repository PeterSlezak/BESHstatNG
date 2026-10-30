# Windows migration audit tooling (04A.01)

This tooling writes/checks audit artifacts only. Existing projects, source, generated metadata, Help, manifests, installer and numerical fixtures are inputs. The resulting ledger is explicitly **NOT READY FOR ACCEPTANCE** while its semantic and wiring investigations remain open.

Run from the repository root with the installed .NET 10 SDK (Roslyn VB is referenced from that SDK), Windows PowerShell 5.1+, Python 3.10+ and PyYAML. No NuGet package reference or install is needed for the analyzer. `NuGet.Config` clears all network package sources for its SDK-only restore. The installed Excel-DNA 1.9.0 assembly supplies read-only inherited-callback metadata. Do not install prerequisites implicitly.

```powershell
dotnet build scripts/windows-migration-audit/WindowsMigrationAudit.csproj --configfile scripts/windows-migration-audit/NuGet.Config -v minimal
dotnet scripts/windows-migration-audit/bin/Debug/net10.0/WindowsMigrationAudit.dll --self-test

# Read-only reconciliation. Set PythonExe to an already-installed interpreter.
dotnet scripts/windows-migration-audit/bin/Debug/net10.0/WindowsMigrationAudit.dll --check
& $env:PythonExe -B scripts/windows-migration-audit/catalog.py --check
& $env:PythonExe -B scripts/windows-migration-audit/test_reconciliation.py

# Acceptance diagnostic: expected exit 2 while this partial audit has open risks.
dotnet scripts/windows-migration-audit/bin/Debug/net10.0/WindowsMigrationAudit.dll --check --strict
```

The analyzer independently re-enumerates explicit project Compile inputs, parses source, and compares deterministic expected CSV/JSON bytes to the ledger. It rejects missing/stale file/member/event/export rows, stale body-pinned review decisions, duplicates, unbound UDF annotations and unknown Ribbon action declarations. `--strict` additionally rejects the recorded ownership/parser/dynamic-event blockers. Python independently checks project file sets/hashes and authored YAML/XML mappings, field completeness and ledger membership/count consistency. Its fixture tests remove symbols, events, exports, files, Ribbon rows and owners from temporary ledger copies and assert failure.

The parser inventories method/constructor/operator/accessor/property/custom-event/Declare/declaration-only bodies, lambdas, field/static-local/auto-property initializers (including As New), types/delegates, uninitialized state, Handles/AddHandler/RemoveHandler/AddressOf/RaiseEvent and indirect/reflection candidates. Source spans, enclosing IDs, signatures and hashes retain provenance. Actual project MyType=Windows and the generated WindowsForms alternative cover the two checked-in conditional regions; alternative-only code is labeled. Method/property containers and child accessors are intentionally separate records. Identical lambda signatures share movement keys and require span IDs. No compiler semantic model, COM dispatch, resource-applied runtime default, full call graph, overload binding or numerical equivalence is inferred from text.

`physical-source-census.csv` checks authored/project VB separately from bin/obj. `ignored-vb-outputs.csv` records the local build-generated VB snapshot; these outputs may be absent or change without an authored-source change and are not the portable-source denominator or a deterministic source-check requirement. `protected-hashes.csv` and the Gate C hash manifest bind existing evidence independently.

`review-decisions.json` holds body-pinned static boundary decisions; current counts are in `semantic-review-coverage.json`. Detailed `04A.01-member-v1` decisions include responsibility, caller/state purpose, mixed responsibilities, whole/helper equivalent evidence, operation/destination/batch, risks and named fixtures. Legacy pins lacking those details remain separately incomplete for acceptance. No generated family rule counts as manual review. `wiring-review-decisions.json` and `control-review-decisions.json` pin the individually inspected dynamic targets and control intents to their exact source dependencies. A stale/missing/reverted decision fails checking; a populated disposition does not satisfy semantic coverage.

To regenerate within an authorized audit, review source changes first, then:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File scripts/windows-migration-audit/inspect-external-callback.ps1
dotnet scripts/windows-migration-audit/bin/Debug/net10.0/WindowsMigrationAudit.dll --write
& $env:PythonExe -B scripts/windows-migration-audit/catalog.py --write
& $env:PythonExe -B scripts/windows-migration-audit/report.py
```

`--ledger=<path>` lets the analyzer compare against a fixture copy; Python accepts `--ledger <path>`. Generation never changes owner metadata. `report.py` requires the recorded validation/protected manifests under ignored TestResults/04A-01; it renders the report and hash index and isolates audit UTC in audit-run.json. Timestamp-independent source data remain deterministic. Report generation is a separate evidence-rendering step, not a replacement for `--check`.

Existing verification: `protect.ps1 -Mode Capture` records **all tracked baseline bytes** once and refuses to overwrite its snapshot; `-Mode Check` checks every hash. The initial audit snapshot is retained locally, not recreated on a reviewer checkout. The optional `run-validation.ps1` invokes existing supported Debug/Release full and Office scripts with local-only RestoreSources/NuGetAudit=false. It uses this machine's already-installed Python path; adapt only to an existing approved interpreter. Legacy builds may regenerate VersionAttributes/Help; only the three expressly backed-up protected generated paths may be restored automatically. Changed generated copies are preserved before restoring exact originals. Other changes fail loudly. The runner needs a prior Capture; do not run installer, PreRelease/signing, live Excel or publishing. Existing standard test result folders should be backed up first; the recorded audit did so.

Ledger files under docs/architecture/WindowsMigrationLedger are ignored by the existing repository rules. No .gitignore exception is added in this audit. Inspect index.json or `git status --short --ignored`; the owner will need explicit force inclusion when eventually staging reviewed ledger files. Keep all changes unstaged/uncommitted until owner review.

The completion amendment preserves the original 51 indexed artifacts plus their index under `WindowsMigrationLedger/baseline/`, and separate original run evidence under `TestResults/04A-01-amendment/original/`. Do not overwrite those snapshots. `completion-amendment.md` and `amendment-validation.json` distinguish fresh evidence from historical validation. Product verification is run by `run-amendment-validation.ps1`, with local package sources, separate logs, backups of existing standard results, and hash-verified restoration of only generated version/Help paths. It executes no live Excel, installer, publishing or signing operation.

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File scripts/windows-migration-audit/check-amendment.ps1
& $env:PythonExe -B scripts/windows-migration-audit/report.py --check
& $env:PythonExe -B scripts/windows-migration-audit/amendment.py check-tracking
```

Report generation order: analyzer `--write`, catalog `--write`, `report.py`, `amendment_report.py`, then `report.py` again to hash the final amendment artifacts. Report `--check` is read-only. The proposed tracked path set in `proposed-tracked-files.json` must equal all `index.json` artifact paths plus index.json itself; `future-staging.txt` gives per-file `git add -f` for ignored ledger evidence and normal staging for visible audit tooling/top-level entry. These commands are future review instructions only; the amendment never stages files. Strict acceptance independently requires every member's detailed source review and every UI control's full default/enabled/reset/validation/output/option contract, in addition to all original integrity/investigation/parser/wiring guards. Routing-only pins are reported separately and do not satisfy complete control acceptance. Integrity failures return1; legitimate incomplete acceptance returns2.

Controlled continuation02 preserves the previous checkpoint's122 indexed paths under ignored `TestResults/04A-01-continuation-02/original/` and its hashes in `continuation02-start.json`. Do not rerun `amendment_report.py` for this checkpoint: its historical report and validation stay intact. Detailed continuation decisions additionally bind an exact JSON source identity and a required dependency-path manifest; removing a dependency or required detail is an integrity failure, not an acceptable strict blocker. These guards do not confer manual review on generated family labels. Full control contracts now survive the derived control CSV and have independent removal/incompleteness guards.

Use separate evidence paths so verification does not overwrite prior logs:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File scripts/windows-migration-audit/run-amendment-validation.ps1 -EvidencePath TestResults/04A-01-continuation-02
# After all suites finish and protected generated originals are restored:
dotnet scripts/windows-migration-audit/bin/Debug/net10.0/WindowsMigrationAudit.dll --write
& $env:PythonExe -B scripts/windows-migration-audit/catalog.py --write
powershell -NoProfile -ExecutionPolicy Bypass -File scripts/windows-migration-audit/check-amendment.ps1 -EvidencePath TestResults/04A-01-continuation-02
& $env:PythonExe -B scripts/windows-migration-audit/report.py
& $env:PythonExe -B scripts/windows-migration-audit/continuation02_report.py
& $env:PythonExe -B scripts/windows-migration-audit/report.py
& $env:PythonExe -B scripts/windows-migration-audit/continuation02_report.py --check
& $env:PythonExe -B scripts/windows-migration-audit/report.py --check
& $env:PythonExe -B scripts/windows-migration-audit/amendment.py check-tracking
```

`continuation02-artifacts.csv` compares paths against the preserved continuation start; `remaining-member-review.csv` and `continuation02-remaining-controls.csv` retain exact unfinished identities. Candidate rows remain provisional until operation-level deduplication and source-wide review. No runtime, numerical golden or owner-acceptance claim follows from these checks. Use a new explicitly named evidence directory in any later continuation; capture its own start before editing and preserve prior results.

Controlled continuation03 is bounded to the91 records in SharedOutputPlanAdapter38, OutputTargetAdapter18, ResultWriter15, CompositeOutputAdapter7 and SheetManagement13. `continuation03_report.py` independently requires all91 actual source IDs and qualified pins; it does not infer review from a file label. New decisions bind profile/character span and exact portable declarations, with comparison-only declarations separate from verified whole/helper equivalents. The false-whole guard checks a pinned source-review assertion and exact declaration evidence, not executable native/scientific parity.

The continuation03 start preserves the current133 indexed artifacts plus index.json under new ignored `TestResults/04A-01-continuation-03/original/`. Previous continuation02 reports/start snapshots and all52 original baseline paths are immutable. Do not run either historical amendment/continuation02 renderer to refresh current totals. `check-amendment.ps1 -EvidencePath TestResults/04A-01-continuation-03` also runs the03 preservation guard. Source/fixture integrity failures remain exit1 and incomplete full04A.01 strict acceptance remains exit2.

After supported fresh product suites finish and the documented generated-original restoration completes, regenerate/check in this order:

```powershell
dotnet scripts/windows-migration-audit/bin/Debug/net10.0/WindowsMigrationAudit.dll --write
& $env:PythonExe -B scripts/windows-migration-audit/catalog.py --write
powershell -NoProfile -ExecutionPolicy Bypass -File scripts/windows-migration-audit/check-amendment.ps1 -EvidencePath TestResults/04A-01-continuation-03
& $env:PythonExe -B scripts/windows-migration-audit/report.py
& $env:PythonExe -B scripts/windows-migration-audit/continuation03_report.py
& $env:PythonExe -B scripts/windows-migration-audit/report.py
& $env:PythonExe -B scripts/windows-migration-audit/continuation03_report.py --check
& $env:PythonExe -B scripts/windows-migration-audit/report.py --check
& $env:PythonExe -B scripts/windows-migration-audit/amendment.py check-tracking
```

The03 report check verifies current artifact hashes/sizes, exact indexed path completeness and proposed tracking membership. It rejects a stale current checkpoint rather than rendering a success from stale prose. The per-file reconciliation, provisional deduplicated operation groups and complete remaining-DM ID queue are versioned separately. Audit-only changes remain unstaged; all ledger paths remain ignored and future force-add instructions must include historical and current evidence.

Controlled continuation04 completes the104 remaining DM IDs in three persisted source-review groups: A39 acquisition/import/host/utilities, B40 PSM input coordination, C25 UI helper policy. It adds checkpoint/group markers to the existing stronger continuation03 member-v1 revision; it does not grant review through a family rule or change any older decision. The immutable start has143 indexed artifacts plus index.json under `TestResults/04A-01-continuation-04/original/`. Historical continuation02/03 reports and original snapshots remain immutable; do not run their renderers to update current totals.

Use the same approved local-only validation runner with `-EvidencePath TestResults/04A-01-continuation-04`. After restoring generated protected originals and regenerating analyzer/catalog, `check-amendment.ps1` with this evidence path adds the04 preservation guard. New independent negatives exercise missing04 review/source IDs, source/dependency/portable-comparison pins, operation mapping, same-size historical/index hash drift and strict2 versus integrity1. These assertions validate evidence, not native or scientific parity.

```powershell
# Following fresh suites/restoration and analyzer/catalog --write:
powershell -NoProfile -ExecutionPolicy Bypass -File scripts/windows-migration-audit/check-amendment.ps1 -EvidencePath TestResults/04A-01-continuation-04
& $env:PythonExe -B scripts/windows-migration-audit/report.py
& $env:PythonExe -B scripts/windows-migration-audit/continuation04_report.py
& $env:PythonExe -B scripts/windows-migration-audit/report.py
& $env:PythonExe -B scripts/windows-migration-audit/continuation04_report.py --check
& $env:PythonExe -B scripts/windows-migration-audit/report.py --check
& $env:PythonExe -B scripts/windows-migration-audit/amendment.py check-tracking
```

The04 read-only check derives all104 source identities, per-file/family counters and provisional operation crosswalks, verifies raw-byte index completeness and preserves all prior514 decisions. A header-only04 remaining-DM CSV means no outstanding DM member reviews; it says nothing about the mandatory non-DM/control/event backlog. `continuation04-artifacts.csv` reconciles all proposed paths, with SHA/size authority in index.json. Re-render only current04 evidence after adding validation records, then refresh the current index; retain final post-freeze command logs separately to avoid recursive evidence hashes. No future staging instructions are executed. 04A.01 remains NOT READY; Gate C2, exceptions and04A.02 authorization are unchanged.

Controlled continuation05 reviews155 exact IDs: A47 remaining RegModels, B90 AddIn/Ribbon/lifecycle, C18 generated resources/settings (including two alternative-profile records). The stronger continuation03 member-v1 schema is reused with05/group markers; prior618 reviews remain unchanged. No generated-family exemption or control-contract promotion. New153-path start snapshot is under ignored `TestResults/04A-01-continuation-05/original/`; all02/03/04 checkpoints and original snapshots remain immutable.

After fresh local-only suites restore generated originals, use analyzer/catalog `--write`, then `check-amendment.ps1 -EvidencePath TestResults/04A-01-continuation-05`, `report.py`, `continuation05_report.py`, `report.py` again, and the read-only checks `continuation05_report.py --check`, `report.py --check`, `amendment.py check-tracking`. Do not invoke historical renderers to update their checkpoints. The05 guard preserves all earlier evidence; new negatives cover exact A/B/C IDs, RegModels source/dependency/Core-forward pins, omitted/duplicate Ribbon usages, invalid generated ownership/provenance and historical/index same-size hash corruption. Strict incomplete remains2; integrity failures remain1.

`continuation05-ribbon-targets.csv` is derived from actual XML plus canonical crosswalk and body-pinned decisions, not a competing authority or native runtime proof. Zero-record support files receive provenance notes, not invented symbols. Candidate operation groups exclude whole forwards/native clicks/generated wrappers and remain provisional. Future per-file force-add instructions include the full ignored ledger; none are executed. Fresh post-freeze checks stay in raw ignored evidence to avoid recursive hashes. Completing these two source families does not complete04A.01, qualify the remaining2055 control contracts or authorize04A.02.

Continuation06 is bounded to108 non-export UDF helper IDs: A42 ExcelArgModules/UDFhelpers/UdfOutputTables, B34 UdfDataImport, C32 UdfDataImportRangeReaders. Prior773 reviews and02/03/04/05 history remain immutable. The new start snapshots162 indexed artifacts plus index.json under `TestResults/04A-01-continuation-06/original/`. Existing stronger member-v1 guards are reused, with06/group markers; no family rule grants review and no outside caller/cache-owner member is promoted.

Use the existing local-only suite runner with `-EvidencePath TestResults/04A-01-continuation-06`, protect/restore generated originals, then analyzer/catalog `--write`, the evidence-path check wrapper, `report.py`, `continuation06_report.py`, `report.py`, followed by read-only `continuation06_report.py --check`, `report.py --check` and `amendment.py check-tracking`. Never run historical renderers to refresh old totals. Final post-manifest checks live separately in raw evidence to avoid recursive hashes. Full strict acceptance remains2; integrity remains1.

The06 reporter derives per-ID export/helper status from actual ExcelFunction rows, compares268 exports before/after and preserves the108 helper distinction. It deduplicates an existing C05 blank/spill operation separately from new06 groups. `continuation06-cache-owner-sites.csv` is a source-pinned declaration/FIT/insertion/DROP-or-PS_CLEANUP crosswalk for11 actual caller maps, not a completed review of those external members or native recalc lifetime. New negatives reject missing/stale pins, generic triage, fabricated export flags, operation/owner omissions and same-size historical/index drift. Initial full negatives and any focused dependency recheck are counted by distinct test identity; repeated cases excluded. Every ignored ledger path stays in the future individual force-add plan; none is staged. UI/control, Graphics and remaining UDF ownership remains obligatory in04A.01; no04A.02/native/golden execution follows from this checkpoint.

Continuation07 reviews only77 specialized import-helper records in10 files: A18 options/formula, B18 MMRM, C28 agreement/contingency/CA, D13 plot/PSM/survival. Immutable start174 paths and rich A/B/C/D decisions live under `TestResults/04A-01-continuation-07/`; an explicit two-row optional-parser reread correction preserves the original C snapshot. The stronger member-v1 contract is retained, with additional kind/enclosing identity checks and source-pinned comparisons. Prior881 decisions and all02–06 historical reports/snapshots/raw outputs remain unchanged; the new guard checks14554 prior raw-file hashes as well as source/Gate C protection.

Use the existing runner/check wrapper with the new07 evidence path. After protected restoration: analyzer/catalog `--write`, `check-amendment.ps1 -EvidencePath TestResults/04A-01-continuation-07`, `report.py`, `continuation07_report.py`, then `report.py`; finish with read-only07 `--check`, report `--check`, and tracking check. Do not rerun historical renderers. The07 reporter verifies exact77 scope, actual helper/export annotations, existing-operation links, before/after counters and historical hashes; index excludes itself. Added negative fixtures reject missing/group-misassigned IDs, stale source/dependency/portable/enclosing pins, forged exports, candidate omissions and same-size index/history drift. Strict remains2 until the full04A.01 review is complete; integrity failures remain1. No staging, next batch, native execution or parity claim is authorized.

Continuation08 is limited to94 basic-statistical source records in8 UDF files, groups A30/B29/C35. Actual annotations identify71 exports and23 non-export records; all268 global registration names/signatures/attributes stay unchanged. The new184-path start and17571 prior raw-evidence hashes preserve02–07 history, while all958 old review objects are checked as exact raw UTF-8 object bytes. New candidate rows extend the live373-row baseline, not the stale299 before07. Group snapshots and39 later conditional p-display dependency additions are retained separately under new ignored08 evidence.

Use the same local-only runner/check workflow with `TestResults/04A-01-continuation-08`, after generated protected restoration and analyzer/catalog `--write`. Run the entire corrected `test_reconciliation.py` suite through the evidence check wrapper after final edits, then `report.py`, `continuation08_report.py`, `report.py`; finish with read-only08 `--check`, report/tracking checks and source/Git/hash guards. The08 reporter independently reconciles94 exact IDs,71 literal annotations, original268 export signatures and names, duplicate/missing reviews, dependency/portable/source pins, old decision bytes, operation mapping and current SHA/size manifest. No whole wrapper equivalence follows from an existing Core calculation. Strict remains2, corruption1; no staging, later batch implementation, native execution or numerical golden capture.

The08 first full78-test pass and first rendered checkpoint are retained separately from a final six-record transitive alpha-boundary refinement. `continuation08-alpha-boundaries.csv` pins actual Core half-alpha guards versus proportion-planning open-interval policy. The entire corrected suite runs again after this refinement, including its drift fixture; repeated test identities are excluded from distinct totals. No product or numerical behavior changes accompany the evidence refinement.


Continuation09 is bounded to84 exact pending regression records: formula/output/LM A32, GLM/NB/ZIP B39, Cox C13. Parsed annotations identify38 exports and46 nonexports; global268 annotations and prior1052 decision objects are immutable. The195-path indexed start, extra ledger/tooling copies,52 baseline paths and20660 previous raw-evidence hashes remain under TestResults/04A-01-continuation-09. Reused capture stdout has obsolete08 counts; actual before.json fields are authoritative. Immutable A/B/C snapshots precede explicit new-only dependency/registration refinement. Sixteen GUI/UDF/Core alignment candidates are future comparisons, not product fixes or correctness assertions.

After the offline validation runner finishes and restores generated originals, run analyzer/catalog --write, the entire corrected test_reconciliation.py suite (check-amendment.ps1 with the09 EvidencePath includes it), then report.py, continuation09_report.py, report.py. Finish with09/report --check, amendment.py check-tracking,09 --guard, source/catalog/strict and Git checks. The09 reporter rejects duplicate/missing IDs, registration/helper drift, stale declarations/dependencies, old decision-object changes, unowned candidate operations, false READY status, and incomplete index membership/hashes. Strict remains exit2/NOT READY; integrity corruption is exit1. No native execution, later extraction, staging or gate closure authorized.


Continuation10 reviews63 exact GEE/ordinal/multinomial IDs in A39/B11/C13,27 exports/36 nonexports. Original1136 decisions and02-09 evidence are immutable. Start212 indexed paths and23727 previous raw evidence hashes are retained under TestResults/04A-01-continuation-10. Only the approved C:/Users/peter/Desktop/WPy64-31470/python/python.exe may run fresh Python (-B); pass -PythonExe explicitly to wrappers, and set PYTHONDONTWRITEBYTECODE=1. Do not run run-validation.ps1 with its stale internal override. No cache files enter the index/tracking plan.

After guarded offline validation/restoration: analyzer/catalog --write; check-amendment.ps1 -PythonExe <approved runtime> -EvidencePath TestResults/04A-01-continuation-10 (full negatives plus10 preservation); report.py; continuation10_report.py; report.py; then10/report --check and amendment.py check-tracking. Strict remains2, integrity1. Twelve pinned GUI/UDF/Core comparisons are future candidates, no product changes or runtime certification.


Continuation11 reviews only63 exact MMRM/LMM IDs (A38/B25),30 parsed exports/33 nonexports; prior1199 decisions and02-10 history remain immutable. The231-path indexed start and26832 raw-history hashes are retained under TestResults/04A-01-continuation-11. Use only C:/Users/peter/Desktop/WPy64-31470/python/python.exe with -B and explicit -PythonExe on guarded wrappers.

After guarded Debug/Release suites and generated-byte restoration: analyzer/catalog --write; check-amendment.ps1 with approved Python and continuation11 EvidencePath (full negatives plus preservation); report.py; continuation11_report.py; report.py; repeat current11 render after tracking materializes to settle manifest labels; report.py; finish11/report --check and amendment.py check-tracking. No historical renderer is rerun. Whole-Core wrapper claims, raw newline drift of prior objects, stale alignment/source pins and false READY are rejected. Strict remains2/NOT READY; integrity1. No product source/fixtures, native UI/Excel, R/golden generation or gate acceptance.


Continuation12 reviews only50 exact classification/plot/PSM IDs (A5/B13/C32),20 parsed exports/30 nonexports; prior1262 decisions and02-11 history remain immutable. The250-path indexed start and29971 raw-history hashes are retained under TestResults/04A-01-continuation-12. New-only wording corrections have explicit source-refinements.json evidence; original group snapshots are preserved. Use only C:/Users/peter/Desktop/WPy64-31470/python/python.exe with -B and explicit -PythonExe on guarded wrappers.

After guarded Debug/Release suites and generated-byte restoration: analyzer/catalog --write; check-amendment.ps1 with approved Python and continuation12 EvidencePath (entire negatives plus preservation); report.py; continuation12_report.py; report.py; repeat current12 render after tracking materializes to settle manifest labels; report.py; finish12/report --check and amendment.py check-tracking. Historical renderers are not rerun. Wrong BESH.PSM.MATCHES spelling, false whole-Core claims, stale source/alignment pins, prior-object mutation and false READY are rejected. Strict remains2/NOT READY; integrity1. No product source/fixtures, native UI/Excel, R/golden generation or gate acceptance.


Continuation13 reviews exactly134 MultivariateAnalysisUDFs.vb source IDs: A25/B15/C31/D15/E48,70 parsed exports/64 nonexports, including7 cache initializers and9 lambdas. Start270 indexed paths,1312 prior ordered literal decision objects and33158 historical raw-file hashes are retained under TestResults/04A-01-continuation-13. Source-based transitive pin/wording refinements and initial helper-authoring failures remain separate from immutable group snapshots; no product or test fixture changes.

Use only C:/Users/peter/Desktop/WPy64-31470/python/python.exe with -B and explicit -PythonExe for guarded Debug/Release full and Office suites. After restoration and FINAL source-note edits, run analyzer/catalog --write before the entire check-amendment.ps1 suite with continuation13 EvidencePath. Then report.py; continuation13_report.py; report.py; settle current13 manifest and report.py; finish13/report --check and amendment.py check-tracking. Historical renderers/evidence remain immutable. Missing state/lambda records, wrong registrations, prior decision reordering, false whole-Core claims, stale pins, unowned/disconnected operations and false READY are rejected. Strict stays2/NOT READY; integrity failures1. No staging, UI/Graphics review, native/GUI/workbook/R/golden execution, extraction or04A.02. The legacy top-level audit entry remains unchanged; current authority is the ledger README and13 report.

Continuation14 reviews only216 original pending Graphics IDs across24 modules: A26/B55/C38/D41/E56. Two previously qualified Compute facades in CumulativeDistributionPlot/CategoricalHistogram are explicitly excluded; no file-only scope expansion. Targets are nonexports; global268 registrations and prior1446 ordered literal objects stay unchanged. Start292 indexed paths and36377 prior raw hashes remain under TestResults/04A-01-continuation-14. Five rich snapshots/checkpoints precede final full validation. Source bodies with legacy non-UTF8 bytes use Roslyn-compatible replacement in excerpts only; raw bytes/hashes never change.

Use approved portable Python with -B and explicit -PythonExe on guarded wrappers. After one fresh guarded Debug/Release/full+foundation run and exact generated-byte restoration: analyzer/catalog --write; check-amendment.ps1 -PythonExe <approved executable> -EvidencePath TestResults/04A-01-continuation-14 (entire negatives and14 preservation guard); report.py; continuation14_report.py; report.py; settle current14 manifest; report.py; final14/report --check and amendment.py check-tracking. Do not rerun historic renderers. Missing/duplicate IDs, prior facade recredit, initializer/accessor/event/lambda omissions, stale pins, false whole-Core/native equivalence, unowned/disconnected operations, old-object mutation/order, ignored force-add omissions and false READY are rejected. Strict stays2/NOT READY; integrity failures1. Common has opaque plot references, not a new graphics renderer/handler. Native COM/clipboard/GDI+/WinForms/event/disposal, GUI, workbook and new scientific goldens are NOT RUN; no staging, migration,04A.02/.29/.75 implementation or Gate acceptance.
C14 final source refinement: five direct fault callers now pin CoreInfrastructure.DefaultErrorReporter.LogAndThrow and the injected logger/message boundary; one new-review GC8/04A.44 annotation is clarified. Original group objects and the first187-test full pass remain immutable in raw evidence; final entire188-test audit rerun follows source/catalog derivation. Fifteen existing source-pinned test declarations are comparison evidence only; no new product tests/goldens. Cox step expansion is explicitly a negative finding: Core BaseSurvivalForPloting already creates prepared steps, so no GUI step defect is inferred from a straight native line renderer.

Continuation15 is bounded to232 original pending Graphics IDs: Box A38, Hull B45,
Kite C46, Polar D44 and Violin E59. Preserve the previously reviewed Violin Compute
and all1662 earlier ordered literal decision objects. The313 starting indexed paths,
extra audit files and39704 prior raw evidence hashes remain under
TestResults/04A-01-continuation-15. Five immutable group snapshots precede explicit
new-only source/wording refinements. No native or scientific parity is inferred.

Use only the approved WinPython with -B and explicit -PythonExe. After one guarded
Debug/Release full+foundation run and exact generated-byte restoration, regenerate
analyzer/catalog with --write, then run check-amendment.ps1 with the continuation15
EvidencePath (entire negative suite plus15 preservation guard). Render report.py,
continuation15_report.py, then report.py; settle the current15 manifest and rerender
report.py. Finish15/report --check, amendment.py check-tracking and final Git guards.
Do not rerun historical renderers. New negatives reject missing/duplicate/cross-file
IDs, omitted initializer/accessor/constructor/lambda records, stale source/portable
pins, unauthorized snapshot/refinement changes, false whole renderer equivalence,
unmapped candidates, false runtime claims, old-object changes/order and force-add
omissions. Strict remains2; integrity failures1. Existing Core numeric owners are
reused; Common OutputPlotArtifact is only an opaque reference. Operations extend
C14 style and axis groups; no duplicate hull/kite/polar algorithms. Count-1 native
series indexing, culture matching, clipping, shape lifetime and scientific alignment
questions are evidence for later approved work, not fixes or newly run native tests.
04A.02 and all migrations remain unstarted; stop for owner review, nothing staged.

Controlled continuation16 is bounded to250 originally pending IDs in CumulativeDistributionPlotExcel78, DumbbellPlotExcel79 and LadderPlotExcel93. Frozen start-line groups A38/B40/C46/D33/E49/F44 are under TestResults/04A-01-continuation-16/targets.json. Prior1894 literal ordered decisions, all source/protected/history and C01-C15 reports stay immutable. Member excerpts preserve actual UTF-16 Roslyn spans and CRLF. No whole renderer is a portable numeric forwarder. The E/F overlapping check failure is retained, with sequential corrected E checks; refinements retain before/after new-only objects.

After the single guarded Debug/Release product cycle with explicit current -PythonExe, run analyzer --write, catalog --write and check-amendment.ps1 -PythonExe <approved interpreter> -EvidencePath TestResults/04A-01-continuation-16. The wrapper adds the16 preservation guard and full corrected negative suite. Then run report.py, continuation16_report.py, report.py again, continuation16_report.py and report.py to settle exact current manifests; finally continuation16_report.py --check, report.py --check and amendment.py check-tracking. Never rerender C01-C15 to update current totals. Strict remains exit2; an integrity exit1 requires correction, never acceptance. Future-staging individual force-add commands are proposed only. No product changes, native Excel, migration, golden capture or acceptance is authorized.


Controlled continuation18 is audit-only and bounded to144 frozen Graphics IDs: Sankey
A40/B23/C11/D20 and XYZ E29/F21, assigned by Roslyn start_line. Start index364
artifacts plus itself and2350 ordered literal prior decisions remain immutable in
TestResults/04A-01-continuation-18. Six group snapshots require exact source/catalog
and member-v1 pins; no data-only enum/field gains fictional review credit.

Use only C:/Users/peter/Desktop/WPy64-31470/python/python.exe with -B and explicit
-PythonExe for run-amendment-validation.ps1 and check-amendment.ps1. Never execute
run-validation.ps1 with its retired internal override. After the single guarded
Debug/Release cycle and allowed generated-byte restoration: analyzer --write,
catalog --write, check-amendment.ps1 with continuation18 EvidencePath, report.py,
continuation18_report.py, report.py (settle current18 manifest only if needed).
Finish current18/report --check, amendment.py check-tracking, source/catalog/syntax
checks and Git/byte guards. Historical writers remain untouched; do not rerender
C01-C17. Passed full preservation scans are retained, not repeated on resumption.

Negative fixtures reject exact-ID omissions/duplicates/stale spans/body/excerpts,
false whole-Core/COM portability/export claims, unowned operations, old decision
mutation/order and incomplete UI/control/wiring falsely READY. Integrity exit1;
strict still-incomplete exit2. Core owns existing Sankey flow math; Radians is only
a primitive in XYZ, not a whole projection equivalent. Common has no verified
Sankey/XYZ renderer/spec. Operations remain provisional; no native/golden/UI review
credit or migration implementation. No staging or later batch is authorized.

Controlled Continuation 21 is audit-only within unfinished 04A.01: Polar A16/35,
Convex Hull B31/46, Dumbbell C37/87 and Ladder D46/74 frozen members/controls.
Source-derived IDs and immutable A-D snapshots live in
`TestResults/04A-01-continuation-21/`; 2737 old member,406 control and569 wiring
literal objects remain byte/order pinned. The C20 starting index has421 artifacts
plus itself. Its56812 prior-history paths pass preflight; the final extended
history has60355 paths. Existing C19/C20 deferrals remain12+23.

Use approved WinPython3.14.7 only, with -B/PYTHONDONTWRITEBYTECODE=1 and explicit
`-PythonExe` for `scripts/windows-migration-audit/run-amendment-validation.ps1`
and `check-amendment.ps1`, both with
`-EvidencePath TestResults/04A-01-continuation-21`. Actual verify script supports
`-VisualStudio 2026`. Keep local RestoreSources/NuGetAudit=false and protected
generated-byte restoration; do not invoke the retired internal Python override.

After the one protected Debug/Release/full+foundation cycle, use source analyzer
--write and catalog.py --write, then the entire corrected check-amendment suite
including the C21 final history guard. Render only current report.py and
continuation21_report.py; settle current manifest/index, then read-only --check
for each and amendment.py check-tracking. Preserve failed attempts; do not run
historical report writers. The first C21 negatives exposed a missing all-form
output-radio dependency and a temporary index fixture membership error; explicit
new-only control-source-refinements.json preserves original group objects while
strengthening edge/default witnesses. The final full suite follows corrections.

C21 qualifies202 controls and explicitly defers40, including RefEdit/output
lifetime and Ladder's empty Designer DropDownList versus stale TextBox comment.
No whole GUI/Core equivalence, actual inherited native state, chart renderer
parity or output transaction is inferred. All390 future fixtures are NOT RUN.
Strict acceptance remains exit2/NOT READY; integrity failure remains1. Gate C2
UNMET, E1-E7 OPEN and GC3-GC8 DEFERRED. No product/API changes, native execution,
new goldens, staging or later continuation is authorized; stop for owner review.

Controlled Continuation 22 is audit-only within unfinished04A.01: Bullet A50/71,
Sankey B48/65, XYZ C52/84 exact frozen members/controls. New raw evidence is
`TestResults/04A-01-continuation-22/`; original A/B/C snapshots are immutable.
Prior2867 member/608 control/785 wiring literal objects retain exact bytes/order;
C19/C20/C21 deferrals12/23/40 remain open. The445-artifact C21 index plus itself
is retained. Known63927 historical paths pass hashes; current71475 historical
04A-01 raw files are independently inventoried and finally preserved.

Use approved WinPython3.14.7 with -B, PYTHONDONTWRITEBYTECODE=1 and explicit
`-PythonExe` on run-amendment-validation.ps1/check-amendment.ps1, with
`-EvidencePath TestResults/04A-01-continuation-22`. Existing offline restore,
VS2026 and exact protected generated-byte restoration remain required. Run one
sequential Debug/Release full+foundation cycle; do not overlap mutable writers.
After restoration, analyzer/catalog --write, focused C22 negatives followed by
the ENTIRE corrected test_reconciliation.py/check-amendment suite and C22 history
guard. Render only report.py/continuation22_report.py, settle tracking/current
index, then --check for each and amendment.py check-tracking. Never rerender old
C01-C21 reports. Preserve failures and rerun the whole corrected audit suite.

C22 source/control refinements explicitly pin Common declarations and external
SheetManagement/GifAnimator dependencies without new external review credit.
Bullet57, Sankey47 and XYZ38 static controls qualify;78 deferred IDs retain owned
fixtures, all NOT RUN. Source routing does not prove reparenting, closed-chart,
RefEdit, native overlay/resize or interrupted GIF export lifetime. Core owns
Bullet/Sankey math; no dedicated portable XYZ projection engine is verified.
Common OutputPlotArtifact is an opaque reference. Strict remains exit2/NOT READY;
integrity failure remains1. GateC2 UNMET, E1-E7 OPEN, GC3-GC8 DEFERRED. No native
execution, new goldens, staging, implementation, C23 or04A.02 is authorized.

Controlled Continuation23 is the owner's newly authorized audit-only extension
inside unfinished04A.01. It freezes179 pending Roslyn IDs and292 controls in GEE
A53/87, GLM B65/102 and PSM C61/103. Two input/options and compute/output/lifetime
snapshots per group, the C22 index467+itself and actual75061 prior raw paths are
under `TestResults/04A-01-continuation-23/`. Prior3017 member,750 control and934
wiring literal objects, including697 full contracts and all earlier deferrals,
are immutable. The original source/control/group bytes survive new-only explicit
dependency/schema refinements. Caller traces use a pinned sidecar to retain the
established81-column source table and default CSV parser limit.

Use only approved WinPython3.14.7 with -B/PYTHONDONTWRITEBYTECODE=1 and explicit
`-PythonExe` on the protected run-amendment-validation/check-amendment wrappers.
C23's one sequential Debug/Release full+foundation cycle is already complete;
resume reporting/checks without rerunning verified product suites. Generated
attributes have exact originals/transient copies and byte-perfect restoration.
After restored source analyzer/catalog --write, check-amendment runs the ENTIRE
audit suite and C23 --guard --history. Do not rerun historical renderers.

Settle only current report.py and continuation23_report.py, then current report
again for index/tracking; repeat the C23/current pair only if current manifest
membership needs settling. Finish both --check, amendment.py check-tracking,
source/catalog/strict and Git guards, plus independent outside-scope CSV/prefix/
protected/source/hash checks. The final historical scan is reused after success;
no parallel worker, benchmark or speedup is claimed. All modes --check/--guard
remain read-only. Source/control omissions, stale UTF-16/body/comparison/default/
handler/outcome pins, invented equivalence, failed history including same-size
changes, and unsupported complete controls are integrity exit1; genuine open
audit is strict exit2. READY FOR OWNER REVIEW requires actual zero residual
members/control/investigation/wiring blockers, not a changed test threshold.

C23 has181 complete static controls and111 explicit deferrals. Existing Core
algorithms/controller/importer reviews are dependencies, not new whole-method
equivalence or credit. GUI/UDF legacy differences are future correctness cases;
Common does not gain a registered GEE/GLM/PSM Office handler. E1-E7 stay OPEN,
GC3-GC8 DEFERRED. No product/native/golden/migration/staging change is authorized.
Stop after C23 for owner review; C24 and04A.02 remain unstarted.

Continuation24 is the owner's audit-only extension within unfinished04A.01.
It freezes exactly148 MMRM and94 LMM source IDs,93+84 controls and all scoped
edges under `TestResults/04A-01-continuation-24/`. A1/A2/B1/B2 snapshots and
checkpoints are immutable. C23's485 indexed artifacts plus index itself,
3196/931/1128 ordered literal member/control/wiring objects,878 full controls
and all historical deferrals remain protected; the actual raw-history census
at C24 start is78724 paths. New-only metadata normalization retains the
Dictionary<string,string> reader and81-column source schema; caller traces
use hash-pinned entry sidecars. No product source or behavior is changed.

Use approved WinPython3.14.7 with -B/PYTHONDONTWRITEBYTECODE=1 and explicit
`-PythonExe` on run-amendment-validation/check-amendment, with the new C24
EvidencePath. Run one sequential protected Debug/Release full+foundation cycle,
then analyzer --write, catalog --write, the entire check-amendment suite and
C24 --guard --history. After exact generated-original restoration, settle
only report.py/continuation24_report.py/report.py and repeat that current pair
if needed for tracking membership. Finish both --check twice, check-tracking,
source/catalog/strict, index contents and Git/protection guards. Never rerun
historical report writers. Read-only --check/--guard write no evidence.

C24 records108 qualified static contracts and69 explicit deferrals, with
source pins, owners and fixtures. Strict unfinished acceptance remains exit2;
integrity errors remain exit1. No fixed threshold hides incomplete controls;
zero actual residual blockers is required for READY FOR OWNER REVIEW. Common
production handlers remain ANOVA/KW only. E1-E7 OPEN; GC3-GC8 DEFERRED; GateC2
UNMET. No native Excel/COM/XLL, new R/golden, Mac/authenticated Office, network
install, staging, product extraction, C25 or04A.02 is authorized. Stop after C24.

Continuation25 is the owner's subsequent audit-only authorization within the
same unfinished04A.01. Live remaining-member IDs freeze83 PCA and168 univariate
SPC members,147+208 controls and3284 scoped edges in five source-sorted A–E
checkpoints under `TestResults/04A-01-continuation-25/`. Full-form dependencies
cross checkpoint partitions explicitly. Prior3438/1039/1260 ordered literal
member/control/wiring objects,505 C24 index artifacts plus index,82474 actual
historical raw files and all original protected sets remain immutable.

Use approved WinPython3.14.7 with `-B`, `PYTHONDONTWRITEBYTECODE=1` and explicit
`-PythonExe` on the existing validation/check wrappers. After the one protected
sequential Debug/Release full+foundation cycle and exact generated restoration,
run analyzer/catalog generation, check-amendment with C25 EvidencePath, and
`continuation25_report.py --guard --history`. The wrapper includes the full
current reconciliation suite and genuine strict2 versus integrity1 guards.
Do not repeat the already successful full history hash scan unnecessarily.

Settle only current report.py, continuation25_report.py, then current report.py
again for index/tracking. Finish both `--check` modes twice, source/catalog,
strict and check-tracking; compare exact outside-scope CSV bytes/order, prefix
hashes, protected sets and historical membership independently. Neither check
mode writes evidence. Never rerun historical continuation renderers.

C25 preserves the81-column member source schema and existing CSV reader limit;
the oversized SPC Designer excerpt is linked through its existing field to the
exact hash-pinned full UTF16 snapshot.143 explicitly adjudicated display/container
contracts qualify statically;212 individual active/indirect/grid/lifetime
deferrals receive no complete credit. Source routing is not native UI proof.
The16 GUI/UDF/input alignment cases and9 deduplicated behavior groups remain
future correctness/extraction work. Core algorithms are reused; no numerical
winner, new production Office handler or runtime parity is claimed. E1-E7 OPEN,
GC3-GC8 DEFERRED, GateC2 UNMET. Stop for C25 owner review; no C26,04A.02,
product change, staging, commit, push, native execution or deployment.


C27 is an audit-only continuation of unfinished04A.01. Freeze two remaining
UibyID Designer spans and195 controls before reviews; five declaration-ordered
39-ID groups and individual source contexts are under
`TestResults/04A-01-continuation-27/`. Previous C01-C26 reports, literal decisions,
source/protected bytes and every preexisting raw path/hash remain immutable.

Use approved WinPython3.14.7 with -B/PYTHONDONTWRITEBYTECODE and explicit
PythonExe on the existing offline VS2026 validation/check wrappers. Keep builds,
source/catalog/store/index writers, standard-result restoration and history
hash scans sequential. Retain generated attribute transients and restore the
original byte-for-byte. Never rerender historical continuation reports.

The entire `test_reconciliation.py` suite registers C27 falsification tests.
`continuation27_report.py --guard --group A` (through E) validates each source
group; final `--guard --history` verifies preservation. Publish current-only
report/C27/index projections, settle exact tracking membership, then run both
reporters with --check twice, catalog/source --check, check-tracking and original
strict acceptance. Strict2 means incomplete full contracts/wiring; integrity1
must be investigated. No acceptance criteria have changed.

The versioned acceptance/deferral schema is PROPOSAL ONLY, approval PENDING;
no new profile is implemented. Deferred controls retain unreviewed contracts
and no qualified credit. C28-C30 need owner approval after the C27 stop. GateC2
UNMET; E1-E7 OPEN; GC3-GC8 DEFERRED. Native Excel/COM/XLL, Mac, authenticated
Office, new R/goldens, staging/commit/push, product migrations and04A.02 NOT RUN.


C28 is a risk sidecar only:892 exact unqualified IDs in six frozen groups
A-F182/136/186/194/160/34, risk-first executionF,A,B,C,D,E. Each checkpoint
retains prior individual sidecar/source/declaration/body/wiring pins, explicit
missing facts, domain owners, approved implementation batches and NOT RUN fixtures.
No member/control/wiring decision is appended or modified. Qualified controls
remain1164/2056; incomplete892 and unresolved wiring622. Source3866/3866,
UI1547/1547, exports268 and structural investigations0 remain unchanged.

Use only approved WinPython3.14.7 -B and explicit PythonExe with established
offline VS2026 sequential Debug/Release and foundation wrappers. Snapshot generated
attributes; retain transients and restore original bytes. Preserve every preexisting
TestResults path/hash, including overwritten standard test outputs. Never rerender
C01-C27 reports. test_reconciliation.py adds test_continuation28.py falsification;
continuation28_report.py --guard --group A (through F) checks individual groups;
--guard --history checks fresh historical preservation. Read-only --check verifies
current C28 deterministic projections and sealed publication inputs. Publish C28,
then current report/index/tracking, settle artifact membership and finish both
--check modes twice, check-tracking, source/catalog/parser and original strict2.
Integrity1 is a failure, never a substitute for strict2. No checker criteria changed.

Risk triage differs from contract qualification and owner acceptance. P0 and
UNKNOWN_HIGH require mitigation/regression/owner decision before affected capture;
P1 options also block affected live-control goldens until validated. P2/P3 do not
move existing Gate C2 obligations. The synthetic fixture-input JSON contains no
computed golden/reference outputs. Proposed boundaryC28.1 is NOT APPROVED; no
new acceptance profile exists. E1-E7 OPEN; GC3-GC8 DEFERRED; Gate C2 UNMET.
STOP for C28 owner review; C29/C30/04A.02/product changes/native execution,
network/installers/signing/publishing and Git staging/commit/push are not authorized.

Controlled continuation29 is separately owner-authorized audit-only work within
official04A.01. It classifies the exact622 unresolved WithEvents source-intent
rows in six disjoint groups174/138/90/69/79/72. The C29 sidecar schema and explicit
named source decisions add zero full edge/control/member qualification credit.
Original ui-wiring.csv and all3866/1217/1403 literal decision objects are unchanged.
continuation29_report.py --guard --group A (through F) checks resumable markers;
--guard --history also hashes all freshly enumerated pre-work historical files.
--check is read-only. Run test_reconciliation.py for all prior and C29 falsification
tests; test_continuation29.py is the focused suite. Never rerender historical
C01-C28 checkpoint reports. Render C29 sidecars, then current report.py and current
tracking/index projections; repeat read-only checks against the settled index.

C29 uses approved WinPython3.14.7 -B, offline local NuGet with NuGetAudit=false,
VS2026 and sequential Debug/Release/full/foundation operations. Audit-only script
copies redirect standard TRX results to the isolated C29 tree so historical
TestResults/Codex and04-01 files are never overwritten. Exact routing-only copy
differences and hashes are recorded in product-validation.json. Save generated
version transients and restore original VersionAttributes bytes after each build.
Native Excel/COM/XLL/GUI, Mac/authenticated Office, R/new goldens and cleanup remain
NOT RUN. C28 risks/802 live-golden blockers are not waived. Strict remains2,
integrity failures remain1, Gate C2 UNMET. Stop at C29 owner review; C30 and
official04A.02 require later explicit authorization. The C30 acceptance handoff
and external-archive retention policy are proposals only; nothing is staged.
