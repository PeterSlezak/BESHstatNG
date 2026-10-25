# Gate C — production Office application-layer formal exit review

**Recommendation: NO_GO. Owner decision: UNSIGNED. Gate C is NOT APPROVED.**

The deterministic source/contract/build/simulation evidence passes. Required
current-source genuine Excel Online activation and the complete native matrix
are not observed or adequately source-bound in this audit (GC1). The smallest
corrective action is evidence collection in an identified, owner-authorized
authenticated Edge session with a disposable workbook, followed by an explicit
owner gate decision. No statistical, UI or writer repair is proposed or applied.
Do not start 04A.01 from this report. Desktop/Mac release qualification is a
separate planned gate, not the reason for this NO_GO.

## Provenance, authority and audit scope

Audit date: 25 October 2026. Initial branch `main`, clean worktree; initial and
current source commit: `da1ffa27303d66c562881b8a6b78d0f2711bc3e3`. Main contains accepted
04.07 at `50a689e` and Revision 2.0 at `da1ffa2`. A documentation-only branch
`codex/gate-c` was created from that clean merged baseline. No fetch/reset,
commit/merge/push, deployment, signing, installer operation or phase work occurred.

Owner instruction and active Revision 2.0 supersede historical pending wording.
All seven Phase 04 commits are verified ancestors of main. The roadmap contains
**223 unique numbered IDs, 40 accepted, 183 outstanding**: accepted counts by
phase are 8/11/8/6/7 for 00/01/02/03/04. All **75 Phase 04A batches are planned**.
A historical 01.05A summary row repeats the roadmap identity; 224 table appearances
must not be mistaken for 224 unique batches. Gate C is not a numbered batch.
No master-plan row/count/status was changed by this audit.

Read root AGENTS/.editorconfig, all 15 current owner READMEs, active plan,
Gate B freeze, seven 04.01–04.07 implementation reports and validation scripts.
`source-review.json` records hashes/read inventory; `report-review-excerpts.json`
records the relevant historical sections. These reports remain unchanged.
Current README product/development boundaries agree with the active source.

Repository provenance is in `TestResults/GateC/provenance.json`: main/HEAD/branch,
clean start, worktree identity fingerprint and credential-free origin fingerprint
(origin host GitHub, repository path redacted). No remote authentication occurred.
Evidence root is **TestResults/GateC/**; it is intentionally ignored by Git.
The owner must archive the evidence package before signing; this report is the
only versioned edit. Raw logs/TRX remain local because they can contain machine
paths. Safe summaries/index capture no private worksheet values, identifying
ranges, browser tokens, user identity or credential material.

| Installed tool / host | Observed value |
|---|---|
| OS | Windows NT 10.0.26300, win-x64 |
| Visual Studio | Visual Studio Community 2026 18.11.12315.225 |
| VS MSBuild / SDK MSBuild | 18.11.1.51303 / 18.9.15+a8dd0577c |
| .NET SDK / runtime | 10.0.402 / 10.0.13; installed wasm-tools |
| Node / npm | v24.20.0 / 11.19.0 |
| Python / PyYAML / MkDocs | 3.11.5 / 6.0.3 / 1.6.1 |
| Edge (isolated simulations) | 156.0.4314.42 |
| Official manifest validator | office-addin-manifest 3.0.1, existing isolated cache |

Core/Common remain netstandard2.0; Windows Excel-DNA/legacy tests remain .NET
Framework 4.8; Office/WasmSmoke and modern tests remain net10.0. No packages,
frameworks, product version/date, manifest or authored metadata were edited.

## Sixteen criterion decisions

PASS below means the stated source/automated scope passed. It does not transform
fakes into native Office observations. PASS-WITH-EXCEPTION carries an explicitly
listed limitation; it is not an owner waiver or gate sign-off. Evidence IDs refer
to entries in `TestResults/GateC/evidence-index.json`; each contains exact paths,
counts, config, source binding and artifact SHA-256. `criteria.json` is the
machine-readable matrix. Selected managed identities were checked against the
executed TRX definitions, not invented from filenames.

| ID | Criterion | Decision | Evidence IDs | Exception(s) |
|---|---|---|---|---|
| C01 | Accepted baseline and roadmap | **PASS** | `SRC-PROVENANCE`, `OWNER-0407` | — |
| C02 | Core / Common / Office boundaries | **PASS-WITH-EXCEPTION** | `SRC-FACTS`, `M-Debug-Core`, `M-Debug-Common`, `M-Debug-OfficeAddin` | E7, GC8 |
| C03 | Common v1 freeze and E1-E7 | **PASS-WITH-EXCEPTION** | `SRC-DOCUMENTS`, `M-Debug-Common`, `M-Release-Common` | E1, E2, E3, E4, E5, E6, E7 |
| C04 | Typed input, identity, bounds and provenance | **PASS-WITH-EXCEPTION** | `M-Debug-Common`, `M-Debug-OfficeAddin`, `N-Debug-Office`, `SIM-08`, `SIM-09` | E3 |
| C05 | Runnable ANOVA/KW and numeric semantics | **PASS** | `M-Debug-Core`, `M-Debug-Common`, `M-Debug-OfficeAddin`, `M-Release-OfficeAddin`, `SIM-06`, `SIM-09` | — |
| C06 | 04.05 read-only builders | **PASS** | `M-Debug-OfficeAddin`, `M-Release-OfficeAddin`, `SIM-05` | — |
| C07 | Guarded output, consent and native outcomes | **PASS-WITH-EXCEPTION** | `M-Debug-OfficeAddin`, `N-Debug-Office`, `SIM-06`, `SIM-07`, `SIM-09`, `SIM-10`, `HIST-0404-1` | E1, E4, E5 |
| C08 | Options, Help, logging and language | **PASS** | `M-Debug-OfficeAddin`, `N-Debug-Office`, `N-Debug-Help`, `SIM-01`, `SIM-02`, `SIM-03`, `SIM-04` | — |
| C09 | Ribbon inventory and capability intersection | **PASS** | `RIBBON-GENERATOR`, `P-Debug-Ribbon`, `P-Release-Ribbon`, `MANIFEST`, `SIM-RIBBON` | — |
| C10 | Four-tab UI, header, About and accessibility | **PASS-WITH-EXCEPTION** | `M-Debug-OfficeAddin`, `M-Release-OfficeAddin`, `SIM-RIBBON`, `SIM-09`, `SIM-11` | GC7 |
| C11 | Unavailable and forged execution paths | **PASS-WITH-EXCEPTION** | `M-Debug-OfficeAddin`, `N-Debug-Office`, `SIM-RIBBON`, `MANIFEST` | GC4, GC5 |
| C12 | Command/shared-runtime lifecycle and write safety | **PASS-WITH-EXCEPTION** | `M-Debug-OfficeAddin`, `N-Debug-Office`, `SIM-07`, `SIM-14`, `SIM-15`, `SIM-16`, `SIM-20`, `SIM-RIBBON` | GC3 |
| C13 | Full native Windows automated validation | **PASS-WITH-EXCEPTION** | `FULL-Debug`, `FULL-Release`, `FOUNDATION-Debug`, `FOUNDATION-Release`, `SOLUTION-Debug`, `SOLUTION-Release`, `XLL-ARCH`, `MANIFEST`, `RIBBON-GENERATOR`, `P-Debug-Ribbon` | GC6 |
| C14 | Genuine current-source Excel Online matrix | **BLOCKED** | `LIVE-CURRENT`, `OWNER-0407`, `HIST-0404-1`, `HIST-0404-2` | GC1 |
| C15 | Platform and release claims | **PASS-WITH-EXCEPTION** | `PLATFORM-WINDOWS`, `PLATFORM-MAC`, `DOC-MICROSOFT`, `SRC-DOCUMENTS` | GC3, GC6, GC7 |
| C16 | Owner gate decision and exception acknowledgement | **BLOCKED** | `OWNER-GATE-C` | GC2 |

### C01 — Accepted baseline and roadmap

All 04.01-04.07 commits are ancestors of main. Explicit owner acceptance and Revision 2.0 establish 40/223 accepted, 183 outstanding; Gate C is not numbered. Historical 01.05A appears in a summary and roadmap; count 223 unique IDs, not 224 table appearances.

Source/contract anchors: `planning/BESHStatNG_CrossPlatform_MasterPlan.md:3-13`; `Git main ancestry / source-review.json`.

Test identities: Governance/source/full-suite or unexecuted native procedure as explicitly identified in the evidence entries; no fabricated standalone test.
Evidence paths: `TestResults/GateC/provenance.json`; `TestResults/GateC/source-review.json`; `planning/BESHStatNG_CrossPlatform_MasterPlan.md`.
Exceptions: None within this criterion’s stated scope.

### C02 — Core / Common / Office boundaries

Core/Common retain netstandard2.0, Windows net48 and modern hosts net10.0. Office invokes Common handlers/Core math; no reversed Office/Excel-DNA/WinForms project dependency was found. WasmSmoke is separate. Legacy file logger remains in Core but is not installed by Office; do not claim the entire library is filesystem-free.

Source/contract anchors: `BESHStat.Core/BESHStat.Core.vbproj:5`; `BESHStat.Excel.Common/BESHStat.Excel.Common.vbproj:5`; `BESHStat.OfficeAddin/BESHStat.OfficeAddin.csproj:3`; `BESHStat.OfficeAddin/Services/AnalysisPilotState.cs:395`; `BESHStat.Core/src/AppInfrastructure/CoreInfrastructure.vb:325`.

Test identities: `OfficeCommandTests.LedgerAndRegistryRetainAllOccurrencesButOnlyActualEditorsExecute`.
Evidence paths: `TestResults/GateC/source-facts.json`; `TestResults/GateC/boundary-audit.json`; `TestResults/GateC/Debug/Core.trx`; `TestResults/GateC/full-Debug.log`; `TestResults/GateC/Debug/Common.trx`; `TestResults/GateC/full-Debug.log`; `TestResults/GateC/Debug/OfficeAddin.trx`; `TestResults/GateC/full-Debug.log`.
Exceptions: E7, GC8.

### C03 — Common v1 freeze and E1-E7

Reserved enum/code/type boundaries and goldens are tested. Existing v1 projections are not a universal generic serializer. All seven accepted Gate B exceptions remain open/carry-forward with explicit role, phase and trigger; later settings implementation only partially addresses E7.

Source/contract anchors: `docs/architecture/gate-b-common-v1-contract-freeze.md:498`; `BESHStat.Excel.Common.Tests/src/GateBOutputContractCompatibilityTests.vb:15`.

Test identities: `GateBOutputContractCompatibilityTests.PublishedEnumMeanings_RemainInt32AndKeepExistingValues`; `GateBOutputContractCompatibilityTests.SafeWriteAndCapabilityCodes_KeepPublishedSpellings`.
Evidence paths: `TestResults/GateC/source-review.json`; `TestResults/GateC/report-review-excerpts.json`; `TestResults/GateC/Debug/Common.trx`; `TestResults/GateC/full-Debug.log`; `TestResults/GateC/Release/Common.trx`; `TestResults/GateC/full-Release.log`.
Exceptions: E1, E2, E3, E4, E5, E6, E7.

### C04 — Typed input, identity, bounds and provenance

Host references/source leases, bounded whole-column normalization and physical long-row alignment are preserved. Missing/error/date policies remain Common-owned. Dense legacy grouped values lack per-observation coordinates; typed frame/model physical provenance must not be confused with that legacy limitation.

Source/contract anchors: `BESHStat.Excel.Common/src/Contracts/SpreadsheetRangeReferences.vb`; `BESHStat.Excel.Common/src/Input/CellValuePolicy.vb`; `BESHStat.OfficeAddin/Services/PhysicalInputView.cs`; `BESHStat.OfficeAddin/Services/WorksheetColumnCatalog.cs:43`; `BESHStat.OfficeAddin/wwwroot/js/workbookAdapter.mjs`.

Test identities: `FormatInputAmendmentTests.DifferentStartsWithoutHeadersPairByWorksheetRowAndMapActualIssuePositions`; `WorksheetDiscoveryTests.SparseProjectionHasNoSyntheticAddressAndRetainsPhysicalColumns`.
Evidence paths: `TestResults/GateC/Debug/Common.trx`; `TestResults/GateC/full-Debug.log`; `TestResults/GateC/Debug/OfficeAddin.trx`; `TestResults/GateC/full-Debug.log`; `TestResults/GateC/full-Debug.log`; `TestResults/GateC/simulated/08-range-capture-browser.log`; `TestResults/GateC/simulated/09-format-input-browser.log`.
Exceptions: E3.

### C05 — Runnable ANOVA/KW and numeric semantics

Only ANOVA/KW editors and verified typed Common handlers execute. Post-hoc p-values remain numeric, modes change presentation only, and effective per-run alpha controls significance. Native visual/parity observation at this source is separate under C14.

Source/contract anchors: `BESHStat.OfficeAddin/Services/AnalysisEditorRegistry.cs:29`; `BESHStat.Excel.Common/src/Analysis/AnalysisHandlerRegistry.vb`; `BESHStat.OfficeAddin/Services/OutputPlanPresentation.cs:8`.

Test identities: `FormatInputAmendmentTests.PresentationPreservesGoldenCellsGeometryAndUsesExistingEffectivePreferencePolicy`; `FormatInputAmendmentTests.SmallPValuesNearAlphaAndBlanksRetainNumericStorageAndFrozenSignificance`.
Evidence paths: `TestResults/GateC/Debug/Core.trx`; `TestResults/GateC/full-Debug.log`; `TestResults/GateC/Debug/Common.trx`; `TestResults/GateC/full-Debug.log`; `TestResults/GateC/Debug/OfficeAddin.trx`; `TestResults/GateC/full-Debug.log`; `TestResults/GateC/Release/OfficeAddin.trx`; `TestResults/GateC/full-Release.log`; `TestResults/GateC/simulated/06-pilot-browser.log`; `TestResults/GateC/simulated/09-format-input-browser.log`.
Exceptions: None within this criterion’s stated scope.

### C06 — 04.05 read-only builders

Worksheet discovery, Header | VarA naming, sparse physical coordinates, local roles/formula/design and repeated/mixed previews are read-only. Switching/retirement invalidates preview; no unsupported fit/write registration is activated.

Source/contract anchors: `BESHStat.OfficeAddin/Services/SpecificationBuilderState.cs:8`; `BESHStat.OfficeAddin/Services/SpecificationCompilation.cs:83`; `BESHStat.OfficeAddin/Services/WorksheetColumnCatalog.cs:43`.

Test identities: `AnalysisComponentTests.SpecificationPreviewIsOptInAccessibleAndLocalEditingDoesNotReadOrWrite`; `WorksheetDiscoveryTests.SwitchingSheetsResetsRolesTermsReferencesAndUsesSelectedSheetId`.
Evidence paths: `TestResults/GateC/Debug/OfficeAddin.trx`; `TestResults/GateC/full-Debug.log`; `TestResults/GateC/Release/OfficeAddin.trx`; `TestResults/GateC/full-Release.log`; `TestResults/GateC/simulated/05-specification-builder-browser.log`.
Exceptions: None within this criterion’s stated scope.

### C07 — Guarded output, consent and native outcomes

Exact regions, lowest available sheet suffix, fresh source/destination inspection, single-use consent and stale-token refusal precede mutation. Values/formulas/styles are read back; only exact declared Web 15-significant-digit and no-fill representations normalize. Partial/uncertain writes remain latched; no automatic rollback/retry. Historic genuine writes do not prove all current native paths.

Source/contract anchors: `BESHStat.OfficeAddin/Services/AnalysisPilotState.cs:415`; `BESHStat.OfficeAddin/Services/WorkbookTransport.cs:22`; `BESHStat.OfficeAddin/wwwroot/js/productionWriter.mjs:40`; `BESHStat.OfficeAddin/wwwroot/js/tablePresentation.mjs:38`.

Test identities: `FormatInputAmendmentTests.OneClickWriteInspectsOncePausesForConsentOrWritesAndRejectsStaleConsent`; `OfficeCommandTests.CommandsRetireMethodConsentRejectSubmittedWritesAndKeepUncertaintyAcrossMethodSwitch`.
Evidence paths: `TestResults/GateC/Debug/OfficeAddin.trx`; `TestResults/GateC/full-Debug.log`; `TestResults/GateC/full-Debug.log`; `TestResults/GateC/simulated/06-pilot-browser.log`; `TestResults/GateC/simulated/07-pilot-browser-uncertain.log`; `TestResults/GateC/simulated/09-format-input-browser.log`; `TestResults/GateC/simulated/10-corrective-workflow-browser.log`; `TestResults/GateC/historical-live-summary.json`.
Exceptions: E1, E4, E5.

### C08 — Options, Help, logging and language

One immutable Common preference snapshot plus host-only forceEnglish defaults true; durable save/readback, old optional-field compatibility and explicit session-only fallback are tested. Retirement precedes persistence; safety locks and explicit alpha survive correctly. Help is version-aware/allowlisted and diagnostics bounded/redacted, not persisted/uploaded.

Source/contract anchors: `BESHStat.OfficeAddin/Services/OfficePreferenceSession.cs:45`; `BESHStat.OfficeAddin/Services/OfficePreferenceStore.cs`; `BESHStat.OfficeAddin/Services/OfficeStrings.cs`; `BESHStat.OfficeAddin/Services/OfficeHelp.cs`; `BESHStat.OfficeAddin/Services/OfficeDiagnosticLog.cs:9`; `BESHStat.OfficeAddin/wwwroot/js/preferences.mjs`.

Test identities: `ForceEnglishTests.RetirementFailureBeforePersistenceCannotLeakFailedLanguageOrAlphaOnReload`; `GlobalOptionsComponentTests.ForceEnglishDraftSaveDiscardDefaultsAndRenderedMessagesFollowEffectiveChoice`.
Evidence paths: `TestResults/GateC/Debug/OfficeAddin.trx`; `TestResults/GateC/full-Debug.log`; `TestResults/GateC/full-Debug.log`; `TestResults/GateC/full-Debug.log`; `TestResults/GateC/simulated/01-production-options-browser-force-english.log`; `TestResults/GateC/simulated/02-production-options-browser.log`; `TestResults/GateC/simulated/03-production-options-browser-sk.log`; `TestResults/GateC/simulated/04-production-options-browser-storage-blocked.log`.
Exceptions: None within this criterion’s stated scope.

### C09 — Ribbon inventory and capability intersection

84 Windows buttons = 79 method appearances plus five utilities, 76 canonical methods, 12 categories, three repeated methods. Generator rejects mismatches. Only the two actual editor/handler methods plus working utilities are enabled; Open is a separate launcher.

Source/contract anchors: `BESHstatNG/Resources/RibbonXML.xml`; `BESHStatNG_Help_MkDocs/docs/_meta/methods.yml`; `docs/architecture/batch-04-07-ribbon-parity.json`; `BESHStat.OfficeAddin/tools/generate_ribbon.py`; `BESHStat.OfficeAddin/Services/OfficeCommands.cs:12`.

Test identities: `OfficeCommandTests.LedgerAndRegistryRetainAllOccurrencesButOnlyActualEditorsExecute`; `OfficeCommandTests.ManifestResourcesDisabledItemsAndLedgerAreExact`.
Evidence paths: `TestResults/GateC/ribbon-python-Debug.log`; `TestResults/GateC/ribbon-python-Release.log`; `TestResults/GateC/ribbon-generator.log`; `docs/architecture/batch-04-07-ribbon-parity.json`; `TestResults/GateC/manifest-validator.log`; `TestResults/GateC/simulated/ribbon-browser.log`; `TestResults/GateC/simulated/ribbon.json`.
Exceptions: None within this criterion’s stated scope.

### C10 — Four-tab UI, header, About and accessibility

Analysis/Options/Export Chart placeholder/Diagnostics remain mounted. Header About/Help, Apache-2.0 text, modal Close/Escape focus return, four-tab roving navigation and narrow/200% text reflow pass simulations. This is not complete accessibility or native Office qualification.

Source/contract anchors: `BESHStat.OfficeAddin/App.razor:14`; `BESHStat.OfficeAddin/Components/HelpAction.razor`; `BESHStat.OfficeAddin/Resources/Ui.resx:110`; `BESHStat.OfficeAddin/wwwroot/css/app.css`.

Test identities: `OfficeCommandTests.HeaderAndExportPageUseLocalizedAccessibleLinksWithoutRedundantControls`; `GlobalOptionsComponentTests.FourTabsRoveWrapAndRetainDraftsWithScreenReaderLinks`.
Evidence paths: `TestResults/GateC/Debug/OfficeAddin.trx`; `TestResults/GateC/full-Debug.log`; `TestResults/GateC/Release/OfficeAddin.trx`; `TestResults/GateC/full-Release.log`; `TestResults/GateC/simulated/09-format-input-browser.log`; `TestResults/GateC/simulated/11-options-browser.log`; `TestResults/GateC/simulated/ribbon-browser.log`; `TestResults/GateC/simulated/ribbon.json`.
Exceptions: GC7.

### C11 — Unavailable and forged execution paths

Chart export and unavailable methods fail independent dispatch checks; new-workbook destination 2 is rejected by the wire. Placeholder/Help availability is not execution. Actual greyed-out rendering in Excel remains C14 evidence.

Source/contract anchors: `BESHStat.OfficeAddin/Services/OfficeCommands.cs:37`; `BESHStat.OfficeAddin/Components/OutputPicker.razor`; `BESHStat.OfficeAddin/wwwroot/js/productionWriter.mjs:54`; `BESHStat.OfficeAddin/manifest-dev.xml`.

Test identities: `OfficeCommandTests.UnsupportedForgedAndUnavailableCommandsCannotNavigate`; `AnalysisComponentTests.ActualDropdownEventsRepresentAllMethodsAndRejectUnsupportedGroupsAndForgedChoices`.
Evidence paths: `TestResults/GateC/Debug/OfficeAddin.trx`; `TestResults/GateC/full-Debug.log`; `TestResults/GateC/full-Debug.log`; `TestResults/GateC/manifest-validator.log`; `TestResults/GateC/simulated/ribbon-browser.log`; `TestResults/GateC/simulated/ribbon.json`.
Exceptions: GC4, GC5.

### C12 — Command/shared-runtime lifecycle and write safety

One shared pane and one bounded expiring pending intent; repeated registration/late owners are guarded. Busy navigation cannot switch a running editor. Submitted native writes are not promised cancellable; completion/uncertainty cannot be overwritten by navigation/preferences. Native show promise still relies on Office timeout; Marketplace opening is not promised.

Source/contract anchors: `BESHStat.OfficeAddin/Services/OfficeCommands.cs:63`; `BESHStat.OfficeAddin/Services/OfficeCommandBridge.cs`; `BESHStat.OfficeAddin/wwwroot/js/ribbonCommands.mjs`; `BESHStat.OfficeAddin/Services/AnalysisPilotState.cs:508`.

Test identities: `OfficeCommandTests.BusyRapidDisposedAndUnsupportedHostRejectionsNeverReplay`; `PreferenceLifecycleTests.SubmittedWriteRejectsSettingsAndObservedCompletedOrUncertainOutcomeSurvivesLaterSave`.
Evidence paths: `TestResults/GateC/Debug/OfficeAddin.trx`; `TestResults/GateC/full-Debug.log`; `TestResults/GateC/full-Debug.log`; `TestResults/GateC/simulated/07-pilot-browser-uncertain.log`; `TestResults/GateC/simulated/14-workbook-browser-pending-sync.log`; `TestResults/GateC/simulated/15-readiness-browser-fixture-excel.log`; `TestResults/GateC/simulated/16-readiness-browser-fixture-pending.log`; `TestResults/GateC/simulated/20-readiness-browser-fixture-bridge-regression.log`; `TestResults/GateC/simulated/ribbon-browser.log`; `TestResults/GateC/simulated/ribbon.json`.
Exceptions: GC3.

### C13 — Full native Windows automated validation

Complete solution, both WASM hosts, Core/Common/Office/legacy managed suites, Node and MkDocs/Python tooling pass in Debug and Release. Four packed-XLL PE checks pass and protected bytes restore. Native Excel XLL loading is NOT TESTED; no existing independent interactive load script was found, and PE proof is not relabelled as a load.

Source/contract anchors: `scripts/verify-beshstatng.ps1`; `scripts/verify-office-foundation.ps1`; `BESHStat.OfficeAddin/tools/generate_ribbon.py`.

Test identities: Governance/source/full-suite or unexecuted native procedure as explicitly identified in the evidence entries; no fabricated standalone test.
Evidence paths: `TestResults/GateC/solution-Debug.log`; `TestResults/GateC/full-Debug.log`; `TestResults/GateC/foundation-Debug.log`; `TestResults/GateC/ribbon-python-Debug.log`; `TestResults/GateC/solution-Release.log`; `TestResults/GateC/full-Release.log`; `TestResults/GateC/foundation-Release.log`; `TestResults/GateC/ribbon-generator.log`; `docs/architecture/batch-04-07-ribbon-parity.json`; `TestResults/GateC/manifest-validator.log`; `TestResults/GateC/built-xll-architectures.json`.
Exceptions: GC6.

### C14 — Genuine current-source Excel Online matrix

NOT TESTED in Gate C: no authorized identified disposable session. Owner reports genuine 04.07 acceptance; historical genuine 04.04 records have two completed writes/fresh consent but lack exact executing commit binding and real 04.07 Ribbon activation. They do not substitute for the required native matrix.

Source/contract anchors: `planning/BESHStatNG_CrossPlatform_MasterPlan.md:726`; `BESHStat.OfficeAddin/tests/browser/live-edge-workflow.mjs`; `TestResults/GateC/live-status.json`.

Test identities: Governance/source/full-suite or unexecuted native procedure as explicitly identified in the evidence entries; no fabricated standalone test.
Evidence paths: `planning/BESHStatNG_CrossPlatform_MasterPlan.md`; `TestResults/GateC/historical-live-summary.json`; `TestResults/GateC/historical-live-summary.json`; `TestResults/GateC/live-status.json`.
Exceptions: GC1.

### C15 — Platform and release claims

Windows desktop command/GUI/native XLL load and Mac desktop are NOT TESTED / DEFERRED TO RELEASE GATE under 12.05/12.06/13.06 and Gate G. They are not made new Gate C blockers. Shared-runtime/Marketplace and partial localization limits are explicit; no released Office product is claimed.

Source/contract anchors: `BESHStat.OfficeAddin/README.md`; `planning/BESHStatNG_CrossPlatform_MasterPlan.md:655`; `planning/BESHStatNG_CrossPlatform_MasterPlan.md:732`; `TestResults/GateC/microsoft-references.json`.

Test identities: Governance/source/full-suite or unexecuted native procedure as explicitly identified in the evidence entries; no fabricated standalone test.
Evidence paths: `TestResults/GateC/source-review.json`; `TestResults/GateC/report-review-excerpts.json`; `TestResults/GateC/live-status.json`; `TestResults/GateC/live-status.json`; `TestResults/GateC/microsoft-references.json`.
Exceptions: GC3, GC6, GC7.

### C16 — Owner gate decision and exception acknowledgement

UNSIGNED. Gate B exceptions remain accepted carry-forward; Gate C proposed exceptions are not silently approved. This audit recommends NO_GO for missing current required native evidence. Only explicit owner sign-off can approve Gate C and a separate instruction can begin 04A.01.

Source/contract anchors: `docs/architecture/gate-c-office-application-layer-exit.md`; `TestResults/GateC/exception-register.md`; `planning/BESHStatNG_CrossPlatform_MasterPlan.md:795`.

Test identities: Governance/source/full-suite or unexecuted native procedure as explicitly identified in the evidence entries; no fabricated standalone test.
Evidence paths: `TestResults/GateC/exception-register.md`.
Exceptions: GC2.

## Verification execution and distinct counts

| Distinct suite | Debug passed | Release passed |
|---|---:|---:|
| Core managed | 1,024 | 1,024 |
| Common managed | 742 | 742 |
| Windows legacy managed | 787 | 787 |
| Office managed/components | 505 | 505 |
| WasmSmoke managed | 45 | 45 |
| Office Node | 468 | 468 |
| Help Node | 71 | 71 |
| WasmSmoke Node | 377 | 377 |
| Windows/catalog Python | 27 | 27 |
| Help/UDF/MkDocs Python | 16 | 16 |
| Python discovery | 5 | 5 |
| Ribbon generator Python | 8 | 8 |
| **Distinct total** | 4,075 | 4,075 |

Both solution builds, both full verification scripts and both Office foundation
gates exited **0**. All managed/Node suites have zero failures/skips; Python,
including actual MkDocs builds, passes. There were no functional/source repairs
and no altered fixtures or weakened tests in this audit.

Counts are derived from copied TRX and runner summaries. Foundation repeats the
same 505 Office managed and 468 Node cases; explicit JS reruns repeat the three
Node suites; explicit UDF docs rerun repeats five cases already within HelpDocs
16. None is added twice. Ribbon Python's eight cases are omitted by the root
script, so they were run explicitly for each configuration and included once.
Source checks, generator/schema validations, four PE checks and 21 browser
scenarios are separately reported, not added to the 4,075 unit-case total.

Exact orchestration, with per-config TRX capture, local-cache environment and
finally restoration: `TestResults/GateC/run-full.ps1`. The executed root commands
for **each Debug and Release** were:

```powershell
& $msbuildPath BESHStatNG.sln /t:Build /p:Configuration=Debug '/p:Platform=Any CPU' /p:PythonExe=$env:PythonExe /p:BuildDate=2026-10-25 /p:PostBuildEvent= /m /v:minimal
powershell -NoProfile -ExecutionPolicy Bypass -File scripts/verify-beshstatng.ps1 -Configuration Debug -VisualStudio 2026 -Legacy -Wasm -Js -Python
powershell -NoProfile -ExecutionPolicy Bypass -File scripts/verify-office-foundation.ps1 -Configuration Debug
# Repeat the same commands with Release; both were executed.
```

`$msbuildPath` resolves to the installed VS2026 Community MSBuild under Program
Files; `$env:PythonExe` resolves through the known installed WinPython interpreter
used by the repository resolver. `RestoreSources` points to the existing local
packages directory; NuGet audit is disabled for offline-cache validation. No
package installation or source package/version change occurred. Full logs retain
the exact expanded invocations; normalized distributable commands redact personal
tool paths. Solution `PostBuildEvent` is suppressed to avoid signing/deployment.

Additional commands executed successfully:

```powershell
& $env:PythonExe -B BESHStat.OfficeAddin/tools/generate_ribbon.py --check
& $env:PythonExe -B -m unittest discover -s BESHStat.OfficeAddin/tools/tests -v
& $env:PythonExe -B BESHstatNG/tools/validate_methods_catalog.py
& $env:PythonExe -B BESHstatNG/tools/generate_analysis_metadata.py --check
& $env:PythonExe -B BESHStatNG_Help_MkDocs/generate_help_resolver_routes.py --check
& $env:PythonExe -B BESHstatNG/tools/generate_help_links.py --methods-yml BESHStatNG_Help_MkDocs/docs/_meta/methods.yml --project-dir BESHstatNG --check
& $env:PythonExe -B -m unittest discover -s BESHStatNG_Help_MkDocs/tests -p test_generate_udf_docs_v2.py -v
node --test BESHStat.OfficeAddin/tests/*.test.mjs
node --test BESHStatNG_Help_MkDocs/tests/help-resolver.test.mjs
node --test BESHStat.WasmSmoke/tests/*.test.cjs
npm.cmd exec --offline --cache TestResults/04-07/npm-cache --package office-addin-manifest -- office-addin-manifest validate BESHStat.OfficeAddin/manifest-dev.xml
powershell -NoProfile -ExecutionPolicy Bypass -File TestResults/GateC/run-browser.ps1
node TestResults/GateC/ribbon-browser-run.mjs
```

The Ribbon runner in ignored evidence redirects only output location and preserves
its original import.meta base; assertions and accepted test/application sources
are unchanged. The prior 20 scenarios run unchanged, with current logs under
GateC/simulated and their original fixed screenshot folders. All **21/21**
scenarios pass. The new Ribbon/header scenario reports zero browser exceptions
and captures `export-header-360.png`, `about-360.png`, `ribbon.png` in GateC/simulated.
This is genuine Edge running **SIMULATED Office**, not genuine Excel E2E.

Warnings per configuration: **one** ambiguous Windows Form+Title Help fallback,
**five** TRX overwrite notices and **one UTA001** for a non-public helper. No
compiler warning/error lines occur in the full logs. `warnings.json` retains the
counts; full stdout/stderr is preserved. Nonfatal Help fallback remains intact.

Both WASM hosts build in each configuration. All four fresh packed-XLL PE checks
pass: Debug/Release x86 0x014C, x64 0x8664, MZ/PE signatures valid. Exact byte sizes
and built hashes are in `built-xll-architectures.json`. Legacy tests load managed
assemblies/dependencies; **native Excel XLL loading was NOT TESTED**. No existing
standalone native interactive load script was found. PE inspection is not a native
load, and native XLL/desktop Excel qualification is carried to release Gate G.

Official validator output is **manifest valid / exit 0**. It checks source XML,
not actual Ribbon repaint, user gesture behavior or host compatibility. The
manifest retains its development identity, HTTPS source, ReadWriteDocument,
VersionOverrides Workbook/DesktopFormFactor, long shared runtime and the same
runtime/FunctionFile/ShowTaskpane URL. AddinCommands/SharedRuntime/RibbonApi 1.1
requirements remain; method operations independently require ExcelApi 1.17.
Unsupported methods/export are statically disabled and direct dispatch rejects
them. No requestUpdate or newer RibbonApi feature is used.

Microsoft currently distinguishes Marketplace function-command pane opening from
sideload/central deployment; this dev host cannot promise identical Marketplace
behavior. See [show/hide policy](https://learn.microsoft.com/en-us/office/dev/add-ins/develop/show-hide-add-in),
[shared runtime configuration](https://learn.microsoft.com/en-us/office/dev/add-ins/develop/configure-your-add-in-to-use-a-shared-runtime)
and [initial command availability](https://learn.microsoft.com/en-us/office/dev/add-ins/design/disable-add-in-commands),
retrieved 25 October 2026. Schema acceptance and these documents do not certify Mac
or Windows desktop operation. The production distribution decision remains Phase 12.

## Genuine-host matrix and missing evidence

An early request asked for an identified authorized authenticated Edge test
session/disposable workbook. None was supplied to this audit. No browser account,
credential/MFA, unrelated tab or workbook was accessed. This records absence of
an authorized audit target, not a claim about whether the owner has Excel open.
The genuine segment stopped without substituting fake Office.js results.

| Native procedure | Gate C observation | Existing evidence / next action |
|---|---|---|
| Real Ribbon/categories, actual enabled/disabled states, cold launch/reuse | NOT TESTED | Owner reports accepted 04.07; needs source/host/date-bound observation |
| Ribbon/header About license/focus, Help, Settings/Log, four tabs, Force English OFF/ON | NOT TESTED | Run with current sideload and Slovak display locale |
| ANOVA wide/long/whole-column, KW, numeric comparisons/style/native readback | NOT TESTED at current source | Historical 04.04 native records are partial background evidence |
| Occupied No then fresh Yes; option rerun unchanged input; two completed writes | NOT TESTED at current source | Existing runner supports this subset after authorized target identification |
| Duplicate-base new worksheet suffix and sanitized warning | NOT TESTED | Verify lowest free suffix without overwriting an existing sheet |
| Worksheet discovery/read-only model and subject/visit previews; switching | NOT TESTED | No unsupported fit/write permitted |
| Retry/reopen/source change, keyboard/narrow behavior, safety latch | NOT TESTED | If uncertainty occurs, preserve latch and do not retry blindly |
| Windows desktop Excel GUI/commands/native XLL loading | NOT TESTED / DEFERRED TO RELEASE GATE | 12.05/12.06, 13.06, Gate G; not a Gate C blocker here |
| Mac desktop | NOT TESTED / DEFERRED TO RELEASE GATE | 12.06, 13.06, Gate G; no Mac result claimed |

`historical-live-summary.json` hashes two earlier 04.04 JSON captures that identify
themselves as genuine, non-simulated Edge Excel Online. They report first and second
verified completed writes, fresh consent and unchanged source. Gate C inspected
the safe flags only and did not rerun them. Their exact executing source commit
is not recorded in the top-level artifact, and neither establishes real 04.07
Ribbon activation. Their index sourceCommit is deliberately null, with an explicit
historical/source-binding qualification. Owner-reported 04.07 acceptance remains
valid; it does not automatically create the missing formal gate evidence.

Minimal corrective evidence procedure (requires authorized disposable workbook):
start the unchanged local HTTPS host, identify the test Edge session and sideload
identity, record source commit/manifest hash/host version/date, then execute all
seven native rows above. Reuse `tests/browser/live-edge-workflow.mjs` only for its
actual occupied-output/option-rerun/two-write subset; it does not certify native
Ribbon clicks. Archive sanitized screenshots, actual disabled states, value/style
readback and fixed-code event outcomes under GateC/live, with no workbook identities
or credentials in distributable summaries. If a real safety failure is found,
stop and separately scope a repair; do not patch it during this governance audit.

## Exception / limitation register and downstream ownership

The complete local register is `TestResults/GateC/exception-register.md` with
machine-readable `exceptions.json`. The table below carries every finding,
impact, owner role, deadline/trigger and mitigation. E1–E7 are **already accepted
Gate B carry-forward**, not resolved. GC entries are **proposed**, requiring owner
acknowledgement; named assignments are not invented. GC1 is the missing-evidence
blocker for this recommendation; GC2 independently prevents phase authorization.

| ID / severity / state | Finding and user impact | Owner role / future batch or gate / trigger | Mitigation |
|---|---|---|---|
| **E1** / Medium / ACCEPTED-CARRIED-OPEN | Windows legacy renderer is not universally the Common semantic cell renderer; accepted numeric fixes do not close every role/text/precision difference. | Windows output / Common maintainer; 04A.28, Gate C2; parity release checks 13.03; **Before broad renderer parity is claimed** | Keep current Windows behavior and explicit numeric/format fixtures; no blanket rendering-equivalence claim. |
| **E2** / Medium / ACCEPTED-CARRIED-OPEN | Strict Office table/composite wire v1 is a restricted host projection, not a generic Common v1 serializer. Production session protocol v1 does not resolve that version boundary. | Common contracts / Office transport maintainer; Production transport design before future schema extension; Phase 12/13 verification; **Before a new wire field/kind or generic serializer is advertised** | Retain exact-key validation and existing Common projection; negotiate separately versioned changes with reader tests. |
| **E3** / Medium / ACCEPTED-CARRIED-OPEN | Legacy GroupedNumericData/result collections and typed detail retain aliases; successful grouped arrays do not carry per-observation row provenance. | Core/Common input and result maintainer; 04A.27 and applicable 05+ typed family migrations; **Before a migrated consumer requires stronger grouped/detail provenance** | Use existing typed frames/models for stronger identity; never infer physical rows from a dense group ordinal. |
| **E4** / Medium / ACCEPTED-CARRIED-OPEN | Common consent is same-instance; host prepared tokens are session-local. There is no durable cross-process workbook identity/authorization protocol. | Common execution / host safety maintainer; Any future asynchronous/remote executor design; Phase 12/13 qualification; **Before cross-process/durable authorization is introduced** | Keep live instance/session binding and single-use Office tokens; never persist them in preferences. |
| **E5** / Low / ACCEPTED-CARRIED-OPEN | A frozen Common guard field does not imply every host supports that guard. Windows and Office reject unsupported guard scope. | Output host adapter maintainer; 04A.28; future exact guard consumer authorization; **Before enabling any new guard consumer** | Preserve field/API and host refusal; test exact non-clear/consent scope before claiming support. |
| **E6** / Low / ACCEPTED-CARRIED-OPEN | Historical reports contain older pending/count wording. Active Revision 2.0 supersedes it; historical counts are not the current ledger. | Owner / architecture documentation maintainer; Every active checkpoint review, including Gate C; **When publishing the next checkpoint** | Retain history; use the current 223 unique / 40 accepted / 183 outstanding ledger. Do not rewrite historical prose. |
| **E7** / Medium / ACCEPTED-CARRIED-OPEN | Gate B did not freeze full-family details/charts, durable workbook identity, production settings or a generic Office wire. Settings and a narrow host wire now exist; unresolved portions remain open. | Core/Common architecture owner; 04A.44 and family/serialization/chart work in 05-12; Gate G; **Before each remaining contract is promoted as production/frozen** | Treat later settings as their accepted schema 1 and Force English as host-only; retain separate metadata/Ribbon/UDF/handler authorities. |
| **GC1** / Critical / OPEN-BLOCKER | Current-source genuine Excel Online Ribbon/disabled-state and complete compact native matrix are not observed or source-bound in Gate C. Owner acceptance and historical 04.04 writes are different evidence. | Owner / authorized Excel Online acceptance operator; Gate C, before authorization of 04A.01; **Before Gate C sign-off** | Identify an authorized authenticated disposable session; run the native checklist and archive redacted source/host/date-bound results. No simulation substitution. |
| **GC2** / High / UNSIGNED | Owner Gate C decision, evidence/exception acknowledgement and authorization are absent. Batch 04.07 acceptance is not Gate C sign-off. | Project owner; Gate C governance; **Before starting 04A.01** | Keep owner decision blank and Phase 04A entirely planned; do not infer approval from test success. |
| **GC3** / Medium / PROPOSED-DEFERRED | Function-command pane opening uses shared runtime and sideload/central deployment semantics; Marketplace pane-opening behavior is not equivalent. | Office deployment maintainer / owner; 12.02-12.04, 12.06; **Before production distribution or Marketplace claim** | Retain ShowTaskpane launcher and dispatch capability checks; requalify command architecture for chosen distribution. No deployment in this audit. |
| **GC4** / Medium / PROPOSED-DEFERRED | Office New Workbook write is unavailable. A new workbook handoff does not itself verify populated output in a new context. No native feasibility proof or OOXML writer exists here. | Office output maintainer / owner; Separate authorized populated-workbook feasibility; Phases 12/13 release disposition; **Before enabling/advertising New Workbook output** | Keep destination disabled and reject forged destination 2. Any bounded XLSX/handoff design needs separate approval and native readback evidence. |
| **GC5** / Medium / PROPOSED-DEFERRED | Chart Export is informational only; other 74 documented methods are not executable Office analyses. Read-only builders are not fitted models. | Office analysis/chart maintainers / owner; 04A reuse gates then Phases 05-10; **Before enabling each method/export capability** | Keep static disabled Ribbon controls and independent dispatch refusal. Do not equate Help or metadata availability with execution. |
| **GC6** / Medium / PROPOSED-DEFERRED | Windows desktop Office command/GUI and native packed-XLL loading, and Mac desktop operation, are not newly observed. PE checks and legacy managed tests are not an interactive native load test. | Release QA / Windows and Mac host maintainers; 12.05, 12.06, 13.06 / Gate G; **Before declared platform release compatibility** | Report NOT TESTED / DEFERRED TO RELEASE GATE. Existing plan does not make these desktop observations Gate C blockers. |
| **GC7** / Low / PROPOSED-DEFERRED | Localization covers new resources, not all historical Analysis controls. Keyboard/narrow simulations do not constitute complete accessibility/security qualification. | UI/localization/accessibility maintainer; 13.04, Gate G; **Before complete translation/accessibility claims** | Force English remains default ON; unsupported locales fall back to English. Retain invariant numeric grammar and explicit partial coverage. |
| **GC8** / Low / PROPOSED-DEFERRED | Core still contains legacy SimpleFileLogger infrastructure. The Office composition uses NullLogger/default Core behavior and a bounded Common diagnostic sink; full Core infrastructure extraction is not claimed. | Core / Windows portability maintainer; 04A.44, Gate C2; **Before full host-infrastructure extraction is declared** | Do not install the filesystem logger in WASM or mutate Core global defaults from pane settings. Preserve legacy behavior until separately scoped extraction. |

No new statistics, charts, Custom Functions, Windows migration, installer payload,
deployment or generic protocol are implemented. Core's legacy SimpleFileLogger
is acknowledged rather than hidden; current Office DI does not install it and
Core defaults use NullLogger. This is a bounded application-layer review, not
completion of the planned full Windows/Core infrastructure portability audit.

## Protected-file preservation and source-control result

All **29/29** pre/post hashes match, including **15/15 owner READMEs**, four
assembly/version files, root props, authored methods, Windows Ribbon, both
manifests, the Revision 2.0 master plan and four packed XLLs. Build-side-effect
VersionAttributes and packed artifacts were backed up before testing and restored
in finally. `protected-before.json`, `protected-artifacts/` and
`protected-comparison.json` provide the capture and comparison. Equal SHA-256
values below mean before = after:

| Protected path | SHA-256 before = after |
|---|---|
| `scripts/README.md` | `C629C477FC157D543738D1ABF5917431C83E4B840ADA85DA3EB01D3C1A62D5D7` |
| `README.md` | `E855795ADC7CF5F3396F478C95AEFAF181E7F8DC8051C7D0B120720B103CC4EC` |
| `planning/README.md` | `36B380049953EE07F8DFBB511FB612B5F433DC52A36DADA0E62DD8B68385465E` |
| `BESHstatNG/README.md` | `FFFDC6F908EB3D93E8920AFB870CBF56B15EB6D99AE447D24FCF814EEF5C2797` |
| `docs/architecture/README.md` | `7D0B182F001E2EA6761180C7BFEFDBB2A8D237DABA59785F8A55B62671EFC4C4` |
| `BESHStat.Excel.Common/README.md` | `F177EEA13C7A89B6FB0FA8020E5B7EFD3871F92B02971BD0213A2EB645BF3CBE` |
| `BESHStatNG_Installer/README.md` | `787E67928A74FE7B9F337CED17BD61A674FACF487F92C0D1C7C32D585B7AE1FF` |
| `BESHStatNG_Help_MkDocs/README.md` | `EE0B63D8D2F48B9E24153AFAFD196F72F6D35F3010A563BBE1C9C980F81581FC` |
| `BESHStat.OfficeAddin/README.md` | `C8C15185E643A4819BD3B95B12805BE07A791604B0F5D01EC0DDE33FED06DE2B` |
| `BESHStat.Core.Tests/README.md` | `97008634160A4C76A2FF82BA2937C312D6EFC2F055DC0D5F729DDB294C524E05` |
| `BESHStatNG.Test/README.md` | `5826152C271B697959284CBD63052E4A3D5B0EB7CC0F53B968C6EC3793AE9734` |
| `BESHStat.Excel.Common.Tests/README.md` | `8A20054A4EC27001E769B8DE896B8FFF2486B195F8F9961D5849A9BA22115B2C` |
| `BESHStat.WasmSmoke/README.md` | `42C0EC552AC4B6077B899574EB077EB7D648FAF68C86CE39C79CDB6476633C4D` |
| `BESHStat.OfficeAddin/tests/README.md` | `E5C7B9B41873999F38317BC1D561B2BCB6F00397F3A1B4BF133291A12D87FE19` |
| `BESHStat.Core/README.md` | `A960804836D2A4E5C5D6BB4244BD868473C3E35EF1B7D8F30869BE1D97C7445D` |
| `BESHstatNG/Generated/VersionAttributes.vb` | `E43FFACF84BAAB46F1C8A5484E6256D8381BC8766BE965CB6031FF2EF24B64CF` |
| `BESHstatNG/My Project/AssemblyInfo.vb` | `7938FD1DA79CCE5D4DAC7ACF5CD04B9AFD22864849709D4B4276D198A48E25B0` |
| `BESHStatNG.Test/My Project/AssemblyInfo.vb` | `04F500B6E171787B0D067D6CF4473FDB6E0E8E22BA4F1104F6954BC831B1CEA7` |
| `BESHStat.Core/Properties/AssemblyInfo.vb` | `C472E5864C9AE4CBA2082C68AF5282E14C15D06B86BB5501EE519AB35A2F3FDD` |
| `Directory.Build.props` | `B69E29BEF7EFBD0EAABE4BACBA05CAD0F58A207688C5DBC956B231B175AEF3C4` |
| `BESHStatNG_Help_MkDocs/docs/_meta/methods.yml` | `6ED831B8789426CF4A88D82776A27D1317DF64D6212E8D3EB815B8CFF2DBF1D2` |
| `BESHstatNG/Resources/RibbonXML.xml` | `162370CB5D317F58DAC392E8B62BEDD4A6817E138C3763FCF5A39E80D32021E3` |
| `BESHStat.OfficeAddin/manifest-dev.xml` | `486033CD65FDD5AD479BB9FE1CEE5839227BDFF2788861C06690A482B6E853F5` |
| `BESHStat.WasmSmoke/office-manifest.xml` | `5B97404E848E260CF7C9F9123DA41552DDE7AC3F4CFC5D636C0515926A553DF8` |
| `planning/BESHStatNG_CrossPlatform_MasterPlan.md` | `86D8EDC68B45022CF9B53A203F02F94338210E6923A7E766F8601D14B1D85BCB` |
| `BESHstatNG/bin/Debug/BESHStatNG-AddIn-packed.xll` | `95897EEBEB3EAA7346097E04851AAC773DB2AA10A7F9445BD7C2B61BBFBA488B` |
| `BESHstatNG/bin/Debug/BESHStatNG-AddIn64-packed.xll` | `6076D66AC1101EF954D34B019BFD948581E220905C4D65E1B9D37427D3F4643E` |
| `BESHstatNG/bin/Release/BESHStatNG-AddIn-packed.xll` | `CE9AB314CFBA8D1D84F71560B68A797294FA2326BD8F5A2F00FDB0722A383C5D` |
| `BESHstatNG/bin/Release/BESHStatNG-AddIn64-packed.xll` | `CE9EC51488207135E56304B11DA36145CA9D3A2AA2163378D2907B3D2DFE3A1B` |

The only versioned addition is this report. Evidence/index/register/summary/scripts
are under ignored TestResults/GateC. No README, historical report, plan, accepted
application/test source, generated metadata, manifest, version, installer payload
or package file was changed. `git diff --check` and final exact status are captured
in the evidence package; nothing is staged or committed.

## Final Git handoff

Branch: `codex/gate-c`. HEAD remains `da1ffa27303d66c562881b8a6b78d0f2711bc3e3`.
No tracked file is modified; the only new versioned path is the report below.
The index has zero changes; the report remains unstaged/uncommitted. Evidence
under TestResults/GateC is ignored by existing policy. Final status:

```text
?? docs/architecture/gate-c-office-application-layer-exit.md
```

`git-state-final.json`, `git-status-final.txt` and `diff-check.exit` record the
final commands/result (diff check exit 0). This audit is complete as an unsigned
NO_GO evidence/governance handoff; no future phase has been started.

## Unsigned owner decision — required before phase authorization

- Owner name/role: **UNSIGNED**
- Decision: **NOT RECORDED**
- Decision date/time: **NOT RECORDED**
- Native evidence IDs/source/host/date acknowledged: **NOT RECORDED**
- E1–E7 carry-forward acknowledged: **NOT RECORDED FOR GATE C** (Gate B approval remains)
- GC exception dispositions / named owners / deadlines accepted: **NOT RECORDED**
- Phase 04A.01 authorization: **NOT GIVEN BY THIS AUDIT**

Recommendation remains **NO_GO** for GC1. Green tests and owner acceptance of
04.07 do not sign Gate C. After the missing evidence is supplied, the owner must
explicitly record a Gate C decision and any accepted exception dispositions.
Only then may the active plan be updated to link that signed decision, keeping
40/223 unchanged because the gate is not a batch; 04A.01 still requires explicit
implementation authorization. No plan change is made or implied here.

## GC1 live-evidence preflight addendum — 2026-10-25T17:49:16.369114+00:00

Repository source remains `da1ffa27303d66c562881b8a6b78d0f2711bc3e3` on `codex/gate-c`.
Manifest SHA-256: `486033CD65FDD5AD479BB9FE1CEE5839227BDFF2788861C06690A482B6E853F5` (matches the audited bytes).
Protected comparison: 29/29 equal, including all 15 READMEs. Only the existing
untracked audit document was present before this attempt; no accepted source,
manifest, fixture, version, README, master-plan or deployment file was changed.

The current request did not identify a specific authenticated Edge test session
and confirm its disposable workbook. A clarification requested a test-profile,
connection or page ID and disposable-workbook authorization. None was supplied
at this evidence snapshot. This does not assert that the owner has no Excel
session open. No browser/session was attached or enumerated, no host was started,
no sign-in/MFA or sideload was attempted, and the live runner was not invoked.
No simulation was run as a replacement. Runtime build identity, Edge/Office host,
requirement sets, TLS and actual sideload binding remain NOT OBSERVED.

**GC1-A through GC1-G: NOT TESTED (precondition blocked).** Native cases executed
0; passed 0; failed 0; blocked/unexecuted 7. Repository integrity was observed;
native post-run workbook integrity was not. No native result is labelled
GENUINE-EXCEL-ONLINE. **NO_GO / GC1 OPEN; GC2 UNSIGNED** remains unchanged.
Prior 4,075 Debug/Release unit checks and 21 simulations are retained as earlier,
separate evidence and were not rerun or inflated. Historical 04.04 records remain
historical. E1–E7 and proposed GC3–GC8 dispositions require owner acknowledgement;
Windows desktop/Mac release qualifications remain untested/deferred.

Safe attempt evidence: `TestResults/GateC/live/preflight-20261025T174916Z/preflight.json`,
`cases.json`, protected comparison and Git snapshots. Exact prior audit/index/
summary/register/integrity bytes were copied into its `before/` directory before
this evidence-only addendum. Existing index artifact references affected by the
addendum are rebound to those immutable snapshots, preserving their old hashes
and original paths; the original evidence-index bytes remain archived as well.

To proceed, the owner must identify the existing test session/page and confirm
that the selected workbook is disposable and authorized for the GC1 read/write
matrix. The owner performs sign-in/MFA and workbook selection. Do not supply a
workbook name, private URL, identity or credential. No exception is waived and no
Gate C decision or Phase 04A authorization is inferred from this request.

## Authorized GC1 genuine Excel Online collection — 25 October 2026 (partial)

This addendum preserves the original unsigned NO_GO review and earlier no-authorization
preflight. The owner subsequently confirmed that the selected example workbook is
disposable and explicitly authorized cell writes, new worksheets and preference changes.
One existing authenticated Edge Excel Online page was targeted; no sign-in/MFA,
credentials, unrelated workbook/tab inspection, fixture Office.js injection or package
installation was performed.

Source is still `da1ffa27303d66c562881b8a6b78d0f2711bc3e3` on `codex/gate-c`.
Manifest source SHA-256 remains
`486033CD65FDD5AD479BB9FE1CEE5839227BDFF2788861C06690A482B6E853F5`.
The accepted existing Debug host was started with
`dotnet run --project BESHStat.OfficeAddin/BESHStat.OfficeAddin.csproj --no-build --launch-profile https`.
A normal HTTPS HEAD to `https://localhost:7141/` returned 200 without a certificate
bypass. Cached MCP tooling attached to the existing Edge debugger; the accepted
`live-edge-workflow.mjs` was read but not run verbatim because it assumes existing
worksheet names/cells. Ignored, run-local procedures used dedicated synthetic sheets.
The `worker.mjs` procedure and per-step `*-code.js`/JSON evidence are under
`TestResults/GateC/live/run-20261025T175920Z/`.

Genuine `Office.context` and native `Excel.run` were observed: Excel/OfficeOnline,
Office `16.0.20613.42304`, display locale `sk-SK`, Edge runtime UA `156.0.0.0`
(the installed-binary audit reported `156.0.4314.42`). ExcelApi 1.1/1.17,
SharedRuntime 1.1, RibbonApi 1.1 and AddinCommands 1.1 reported supported.
The loaded fingerprinted Core/Common/Office WASM SHA-256 values matched the
audited on-disk Debug build (`bound-assets.json`, `source-binding.json`). There
is no embedded commit ID; binding combines unchanged accepted source, prior build
provenance and loaded artifact hashes. **This does not establish which cached
manifest Excel sideloaded.** The custom BESHStatNG Ribbon tab was absent and
current-manifest re-sideload/real command activation remains required.

| Case | Current disposition | Genuine observations and remaining evidence |
|---|---|---|
| GC1-A | BLOCKED | Native host/build observed. Current-manifest cold launch, Ribbon categories, disabled states and command activation remain unverified. Owner re-sideload requested; hidden/unactionable Add-ins controls were not forced. |
| GC1-B | BLOCKED / partial observations | Four tabs/Export placeholder present; About opened and Close/Escape returned focus. Force English started ON on Slovak Office; OFF saved and restored Slovak resources after pane reload; ON saved/restored English after reload. Other preferences stayed unchanged. Full physical keyboard/narrow-pane, About text assertions, actual external Help opening and close/reopen remain incomplete. |
| GC1-C | BLOCKED / partial observations | ANOVA wide columns, row orientation, separate long-format ranges with blank overhang, and whole columns passed accepted statistic comparisons. All five tables produced with all post-hoc options. KW/Dunn preview and repeat Run passed with existing G10 display tolerance. ANOVA/Welch and four post-hoc numeric Excel values/significance colors were read back. KW-specific native output/readback remains outstanding. |
| GC1-D | PASS | Occupied target: No left output/source unchanged; fresh Yes completed verified write. Welch/Fisher option change with unchanged references required fresh consent and completed a second verified write. Two new-sheet writes selected lowest free suffixes 2/3, kept the base field unchanged, and emitted visible/sanitized warning. **Four verified Completed writes total.** |
| GC1-E | BLOCKED / partial observations | Active-sheet discovery and sparse `Response \| VarA`, `Dose \| VarC`, `Treatment \| VarE` captions observed, with no fit/write controls. Design widths/repeated/duplicate matrix did not complete. Automation waits and a pane reset interrupted collection; these do not establish a product defect or a passing preview. |
| GC1-F | NOT TESTED for full matrix | Post-completion option rerun is covered by D. Busy cancellation/navigation, bounded Retry and reopen checks remain outstanding. No mutation-uncertain outcome occurred; no failure was injected and no latch was cleared. |
| GC1-G | PASS WITH LIMITATION at recorded observation | All six originals matched pre-write values/formulas/used-bounds/number-format fingerprints; only nine owned synthetic fixture/output sheets were added. Output values stayed inside intended regions, controls/source stayed unchanged and original preferences were restored semantically. Other workbook tabs were not inspected; this is not a full binary/style comparison. Repeat after resumed tests. |

Exact positive/negative observations, timestamps, source/host identities, limitations
and file hashes are in `cases-current.json` and the separate authorized attempt in
`TestResults/GateC/evidence-index.json`. Setup/automation failures are retained;
they are not silently reclassified as application failures or passes. In particular,
the first KW comparison used too-tight tolerance for G10 preview text and was
rechecked with display-appropriate tolerance; source/goldens were not modified.
Fixed-true license/version fields in an exploratory workspace helper are not used
as independent native assertions of those strings.

Protected comparison is **29/29 equal**, including all 15 READMEs, generated
version/assembly files, both manifests, active plan, methods metadata, Windows
Ribbon and XLLs. `git diff --check` exited 0. Only the pre-existing untracked Gate C
audit document is versionable; source/tests/fixtures/manifests/README/plan stayed
unchanged and nothing was staged or committed. Original 4,075 distinct checks per
Debug/Release and 21 simulated scenarios are prior evidence, not rerun or inflated
by these native UI actions. Windows desktop and Mac remain NOT TESTED/deferred to
the approved release qualification gates.

**NO_GO / GC1 OPEN; GC2 UNSIGNED.** Partial genuine observations now exist, but
they do not complete GC1 or waive E1–E7/GC3–GC8. Current-manifest re-sideload,
remaining native matrix and final integrity recapture are the next steps. Only the
owner may sign/date the Gate C decision, acknowledge exceptions and separately
authorize 04A.01. No Gate C approval, plan/status change or phase start is implied.

## GC1 resumed after owner manifest-ready confirmation — 25 October 2026

The owner confirmed "ready" after re-sideload instructions. Fresh Git checks still
show `codex/gate-c` at `da1ffa27303d66c562881b8a6b78d0f2711bc3e3`, with manifest
SHA-256 `486033CD65FDD5AD479BB9FE1CEE5839227BDFF2788861C06690A482B6E853F5`.
The owner-authorized disposable workbook was reidentified by all six original
worksheet identities in memory; no names/values were exposed. Existing source,
manifest, test, README, version, plan and deployment files were not changed.

New genuine evidence now establishes the actual **BESHStatNG Ribbon** and
**cold ANOVA menu activation**, then reuse of a single pane for KW, ANOVA,
repeated ANOVA clicks, Settings and Show Log. Editors opened without calculating.
Export Chart was visibly disabled; the Parametric menu showed disabled future
methods and enabled One-Way ANOVA. Sanitized command-completion events appeared.
Office now reported `16.0.20619.42309`, OfficeOnline/Excel, `sk-SK`; supported
ExcelApi 1.17, SharedRuntime/RibbonApi/AddinCommands 1.1. This is a separate
observation from the earlier Office build. All three newly loaded WASM hashes
again matched the audited Debug artifacts. Source binding combines that proof,
unchanged accepted source and owner-confirmed sideload, not a screenshot alone.

The resumed native KW/Dunn write completed and read back an actual numeric
tie-corrected p-value matching the accepted fixture, significant color, numeric
adjusted Dunn p-values and a non-General number format. There are now **five
verified Completed writes** across this authorized source-bound collection:
two consented existing-sheet ANOVA writes, two numbered new-sheet ANOVA writes,
and one existing-sheet KW/Dunn write. No mutation-uncertain outcome occurred.

Read-only native LM previews passed design widths **3, 4 and 5**, sparse physical
captions and active worksheet discovery. Switching to the repeated source retired
the old model preview. The required repeated-subject/duplicate cases did **not**
complete in automation: final observed codes included `spec.variables-ready` and
`spec.variables-reset`; no 3-subject/6-observation or duplicate-rejection pass is
inferred. No fit/write action exists in the builder, and no source repair was made.

Native About opening confirmed Apache License 2.0, version 1.2.6.0 and development
build 04.07. Full Escape/focus/keyboard assertions were not completed. The runner
also encountered an actually minimized Edge window, hidden-page timing and 80%
zoom pointer issues. Restoring the existing window returned actual visible/focused
state; no fake Office runtime or focus emulation was installed. Attempts to reset
zoom left it unchanged at 80%. Failed automation observations are retained and
are not asserted as proven application defects. A clean pane reload restored
navigation availability; full busy/cancel/Retry coverage is still not established.

Six real Help actions were attempted using trusted browser input on the real
production controls (Ribbon/header Home, Export Chart, Global Settings, ANOVA,
KW). **No newly opened published Help page was observed in the attached Edge
context.** This cannot distinguish a blocked opening from a page opened outside
that connection, and does not prove the documentation opened. Source/version-aware
resolver tests remain earlier automated evidence; no successful native Help page
is invented. An explicit human observation can supplement this gap.

Current dispositions: GC1-C calculations/numeric output and GC1-D guarded writes
are verified; GC1-A has genuine command evidence but incomplete full matrix;
GC1-B is partial; GC1-E is partial; GC1-F remains incomplete. Final GC1-G integrity
is separately recorded in `r2-final-workbook-integrity.json` and protected-file
evidence. Missing required cases keep **NO_GO / GC1 OPEN**. **GC2 is UNSIGNED**.
The owner's general statement that no issue was visible is preserved as a broad
owner observation, not substituted for specific unexecuted cases or gate approval.
A concise per-case human-verification question was requested for the residual gaps.

Evidence stays under `TestResults/GateC/live/run-20261025T175920Z/`, with unique
`r2-*` observations and a separate resumed attempt in the evidence index. Original
audit/NO_GO history, all 4,075 distinct checks per configuration and 21 simulations
remain intact; those suites were not rerun. All 29 protected files, including 15
README files, matched again and `git diff --check` exited 0. Only the untracked
audit document is versionable; local index/summary/integrity artifacts are ignored.
Nothing is staged/committed. Windows desktop and Mac remain NOT TESTED/deferred;
E1–E7 and GC3–GC8 are not waived. No roadmap status or Phase 04A authorization changes.
