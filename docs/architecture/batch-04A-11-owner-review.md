# Official 04A.11 consolidated owner review

**OWNER REVIEW PENDING — one bounded native timing disposition:
`C11-NATIVE-CORRECTIONS-01`.** All permitted software work is implemented.
The LM GUI alpha assignment and Windows Data-header activation remain dormant;
this dossier does not claim those two shipping GUI defects are fixed.

**P NO; N1 NO GO; N2 NO; Gate C2 UNMET. Zero native-control releases and zero
original obligation discharges. Excel/COM/UIA NOT RUN.**

## Verified base, scope and preservation

The verified local `main`, HEAD and local `origin/main` tracking ref were
`463aaa12a6342cedfa084e266d5a985461de9d3a`, the accepted 04A.10 commit including
Master Plan Revision 2.7. The worktree/index were clean before creating the
authorized `codex/04A-11` branch. No server fetch/query is inferred from tracking-ref
equality. Initial read-only commands briefly observed the preceding branch/main
state while the owner refreshed the checkout; the matching merged state was
reverified before the branch cut. Codex did not merge, reset, rebase or fetch.
Git used command-local `safe.directory`; branch creation required approval to
write `.git/index.lock`. No global configuration changed.

HEAD remains that base. The index is empty. Only the eleven approved existing
files and the new proposal files in the publication manifest changed. No owner
work was discarded, stashed or overwritten. AGENTS.md, Revision 2.7, accepted
04A.09/10 dossiers, original crosswalks and the amended 04A.02 acceptance/gates
were read. The [pre-change inventory](../../evidence/04A11/prechange-manifest.json)
records exact entry source/evidence hashes and **180,375 historical TestResults
files**. Its untracked status already includes the newly created verification
harness; tracked product files were still clean.

Core/Common remain `netstandard2.0`, Windows/legacy remain .NET Framework 4.8.
No Office/WASM UX, Ribbon, Designer, public UDF export/signature, installer/MSI,
signing, version, acquisition, chart engine or result-writer source changed.
Revision 2.7 and all accepted older evidence/tests/approvals remain untouched.

## Four-family production delegation

The new Common files are
`BESHStat.Excel.Common/src/Analysis/ExtendedRegressionFamilyPolicy.vb` and
`BESHStat.Excel.Common/src/Results/ExtendedRegressionReport.vb`.
They compose the accepted request/component/options/vector/validation and
report/document/row-output contracts. There is no new request grammar, fitting
engine, formula parser, inverse link or statistical implementation.

| Actual route | Before/after call chain and distinct intent | Core and native behavior retained |
|---|---|---|
| `UiGLM.RunGLMNB2` | Existing expanded capture → `ExtendedRegressionFamilyPolicy.Project` → typed options/count/link → existing `GLM_NB.data/settingInputs/Fit/wrapResults` → typed adapter report → document-derived legacy tables | Estimated NB2 dispersion is distinct from CI alpha; actual `NBalpha` retained in response; offsets/weights/starts/iterations/residuals preserved; Data/GLM NB2 sheets unchanged |
| `UiGLM.RunZIP` | Existing count and logistic captures → two independent intent components → existing `ZeroInflatedPoisson.dataInputs/settingInputs/Fit/wrapResults` → typed report → existing writer | Separate matrices, names/effects, intercepts, starting vectors/counts; maximum/EM iterations; count offset only; no new ZIP weights; original Poisson/Logistic Data blocks and Zero-Inflated Poisson sheet |
| `UiGLM.RunOrdLogit` | Capture → family intent/validation/count/reference → existing `OrdinalLogitModel.Data/SettingInputs/Fit/wrapResults` and residuals → typed document → writer | Existing ascending/reversed category convention according to reference; no independent intercept added; actual Core class axes and threshold count; Data/Ordinal_LR sheets |
| `UiGLM.RunMultiLogit` | Capture → family intent/validation/count/reference → existing `MultinomialLogitModel.data/settingInputs/Fit/wrapResults` and residuals → typed document → writer | Arbitrary integer class identities, Core sorted probability axis and selected reference, parameter-count multiplication; Data/Multinomial_LR sheets |

All four methods consume the new intent for real Core inputs/options/counts and
consume `report.LegacyTables()` before the unchanged table writer. The report
rehydrates immutable document snapshots through the accepted 04A.10 table bridge;
this is not an unused helper or a replacement writer. Core cells, dimensions,
headings, footnotes, p-value columns/cells, optional-section order and blank
separators are retained. Host-free tests execute the shipping capture/typed report
overload and actual writer base, comparing complete output blocks and formatting.

[Exact input snapshots](../../evidence/04A11/input-projection.txt), including the
Framework variant, retain all matrices, names, original effects/keys, source RowIds,
offset/weight identities and values, independent starts, options, reference/counts
and validation diagnostics for the eight deterministic cases. These are public-safe
committed synthetic fixtures, not workbook data. Source-sidecar absence is explicit
in those fixtures; unchanged shipping capture and retained 04A.09 transport tests
cover physical coordinates when supplied. No row sorting, filtering, rounding,
weight normalization or Double → localized string → Double conversion is added.
Reversed/noncontiguous rows and sparse class-label tests preserve actual Core axes.

Typed request validation remains observational in Windows. Supplied-empty versus
absent starts/vectors, duplicate/misaligned rows, key collisions, nonfinite values,
unsupported roles and invalid options remain diagnostics rather than new shipping
refusals. Existing expanded-count dialogs and Core failure paths remain in place.
NB2 is the estimated-dispersion engine; fixed-dispersion NB remains the existing
general GLM path and accepted typed request policy. Tests explicitly distinguish
that path rather than pretending GLM_NB has a fixed-dispersion setting.

Count outputs have one fitted-response column; NB2 residuals retain seven columns,
ZIP two. Categorical residual width is checked against Core's five class blocks
plus three diagnostic columns, and the supplied probability/category axes must
match source identities. The projector does not reorder category columns or
invent probabilities. Missing optional sections and malformed lengths/bounds/
headers/axes are explicit. Request validation, optimizer convergence and successful
document composition remain separate; false/unknown convergence is never success.

## Three agreed defects: actual before/after and shipping status

[Current issue records](../../evidence/04A11/issues-final.json) contain source/test
hashes, all affected original control/obligation IDs, numerical dispositions and
next owners. Earlier current-batch receipts are retained, with explicit successor
links; accepted older records were not rewritten.

| Issue | Current status | Implemented and withheld behavior |
|---|---|---|
| `C10-LM-ALPHA-01` | **PORTABLE_FIXED_NATIVE_PENDING**; GUI switch **UNAPPLIED_GATED** | Portable intent uses requested finite alpha, with existing validation and .05 default. Tests assign it before Core fitting: .10 gives 90% CIs, .05 gives 95%; coefficients, SEs and fitted rows remain exact. Shipping RunOLS explicitly selects `NativeLegacyDefault`, so actual .05 is reported truthfully until activation. |
| `C10-CORE-CONSTANT-01` | **FIXED_AND_ACTIVE** for defensive Core reporting | A typed `HasReportData` availability check prevents dereferencing absent inference/diagnostic payloads. Early exits return one diagnostic-only table, absent predictions/residuals and false convergence. Missing HL is guarded, no statistic/p-value invented. Binomial report/cache state resets prevent stale output across recovery. The Windows classification tail is skipped only when no report data exists; completed fits, including nonconverged fits, retain their original post-fit controls/chart order. |
| `C10-COV-NAMES-01` | **PORTABLE_FIXED_NATIVE_PENDING**; Data-header switch **UNAPPLIED_GATED** | Core clones predictor names before covariance label formatting; iteration formatting also avoids blind missing-name indexing. Caller/Core/Common identifiers remain intact across repeated wraps, with unchanged numbers/order. Windows temporarily reproduces its previous Data header on a separate display-name clone; no erroneous naming rule is embedded in Common or treated as permanent correctness. |

The [pre-change captures](../../evidence/04A11/baseline-registry.json) reproduce
requested .10/effective .05, before/after name arrays, and all-zero/all-one,
intercept-only/slope, HL on/off and optional-section outcomes on unedited Core.
The old constant response could also fail on unavailable covariance/residual
state, not only HL. The active correction returns an explicit unavailable-fit
report for that state; it does not turn zero-initialized arrays into valid MLEs.
Completed nonconverged/separated fits retain their real output and warnings.

The exact Core `Fit` statement comparison removes only the documented binomial
lifecycle reset and verifies every original estimator statement remains identical.
No IRLS, likelihood, linear algebra, scale/dispersion formula or scientific
inference algorithm was replaced. Fresh normal report/table/vector captures retain
their established values. Reused binomial state is deliberately isolated; it is
not claimed to preserve the previous stale-cache defect. Missing/short name tests
do not fabricate categorical identifiers. Other existing covariance axis/layout
limitations, including no-intercept labels, were not silently redesigned.

The accepted four-argument CLR projection entry point is retained through a
forwarding overload, with an executable signature test. The additive alpha-mode
overload supports the corrected policy and the temporary native mode. Generic
snapshot alpha remains request metadata; `actualFitAlpha` records what Core used.
No serialization schema or UDF export was redefined.

The [real-Core numerical delta probe](../../evidence/04A11/defect-numerical-deltas.txt)
records actual 95% versus 90% interval limits and asserts unchanged coefficient,
SE, fitted and all numeric model/ANOVA/df/VIF/covariance cells. Its public C# source
calls existing Core/Common APIs only, including the retained four-argument entry
point. Offline build/run both exit 0; source/binary/output hashes are recorded in
[probe provenance](../../evidence/04A11/diagnostic-probe-provenance.json). It also
records repeated-wrap identifier stability and diagnostic-only constant outputs.
This supplementary probe is not padded into unit-test counts or promoted as a
scientific reference.

Read-only UDF reconnaissance confirms LM_FIT already assigns alpha, GLMNB_FIT
estimates dispersion while general GLM supports fixed NB dispersion, ZIP keeps
two designs, and ordinal/multinomial retain their Core references/parameter counts.
Public UDF sources are unchanged. The current GLM UDF handle guard checks basic
coefficient arrays, not the new complete-report availability state; reconciling
failed-handle/inference semantics is explicitly **04A.40** work, not claimed fixed
by a safe Core wrap. No live Excel UDF execution occurred.

## One exact native timing disposition

The retained amended 04A.02 gate requires pre-change evidence/risk/N authority or
a fresh exact case/criterion/ID-bound owner deferral before changing an affected
Windows consumer. The current task explicitly authorizes portable corrections
and minimal defensive Core guards, but does not supply a timing exception for
the shipping statistical CI change or Data-header correction. Neither the 2×2
nor importer exception is appropriated.

**Request `C11-NATIVE-CORRECTIONS-01`:** approve the bounded deferral of activating
the LM requested-alpha assignment and corrected GLM/NB2 Windows Data headers until
the original native pre-change prerequisites and exact timing authority are met;
accept the completed portable fixes and four-family software work meanwhile.
This is an administrative disposition ID, not a new original obligation.

The [current unapplied patch](../../evidence/04A11/C11-NATIVE-CORRECTIONS-01.final.UNAPPLIED.patch)
and [hash receipt](../../evidence/04A11/native-patch-final.json) specify the exact
changes against current source. `git apply --check` passes. Its exact patched full
form compiles via an isolated source override, with fresh outputs, as recorded in
[candidate validation](../../evidence/04A11/native-candidate-validation-final.json).
Corrected alpha semantics execute through Core/Common/adapter tests. No form
constructor, live native writer or Excel instance was used. A dormant patch is
not represented as a shipping GUI fix.

Remaining output/format reconciliation belongs to **04A.28**, GUI/UDF parity to
**04A.40**, real GUI/screenshot confirmation to **04A.47**. The shared HWND/PID,
trusted XLL/assembly attribution and independent Windows/COM risk prerequisites
remain assigned to 04A.45–48.

`C09-NATIVE-STRICT-01` remains approved as a deferral. Its activation patch stays
UNAPPLIED at SHA-256
`aab350401c873d1a4f56450295688e9a046122aca2269fc9a618765f8fe79fe1`.
`CaptureRegressionRequest` is unchanged, with no new Blocking/IsValid throw.
The exact crosswalk retains all **34** associated controls and seven original
OPEN obligations:

* `control:BESHStatNG.UiGLM.lbY`
* `control:BESHStatNG.UiGLM.lbOffset`
* `control:BESHStatNG.UiGLM.lbWeights`
* `control:BESHStatNG.UiGLM.lbSelectedEffectsList`
* `control:BESHStatNG.UiGLM.lbSelectedEffectsListLogistic`
* `control:BESHStatNG.UiGLM.lblProgress`
* `control:BESHStatNG.UiGLM.ProgressBar1`

The original **1,514/802**, transferred **239/218**, shared ANOVA/normality
**160/141**, importer **44**, and **three 2×2** obligations remain OPEN and overlap;
never add them. No original ID/hash was invented, released or discharged.
04A.08 formula/reference/label precision and 04A.05–07 candidates remain with
their planned owners; this batch did not silently resolve them.

## Documentation impact and Help verification

| Existing method page | Change/current behavior | Windows / Office | Screenshots and next obligation |
|---|---|---|---|
| `negative-binomial-regression-nb2.md` | Four-family extraction is behavior-preserving; disclose pending native Data-header correction | Windows existing route; Office task-pane regression unavailable | Existing assets unchanged; native revalidation pending 04A.47 |
| `zero-inflated-poisson-regression.md` | documentation-impact: none for fitting/output; truthful Office availability and existing no-weights boundary | Windows ZIP; no Office run instructions | Existing images unchanged; native verification pending |
| `ordinal-logistic-regression.md` | documentation-impact: none for category/reference behavior; Office availability note | Windows ordinal; Office unavailable | Existing images unchanged; native verification pending |
| `multinomial-logistic-regression.md` | documentation-impact: none for class/probability behavior; Office availability note | Windows multinomial; Office unavailable | Existing images unchanged; native verification pending |
| `multiple-linear-regression-lm.md` | Correctly document current GUI .05 limitation, portable correction and pending activation; UDF alpha distinction | Existing Windows/UDF paths, no new Office screen | Historical example images are not new GUI verification |
| `generalized-linear-models-glm.md` | Active unavailable-fit diagnostic/no fabricated HL; preserved identifiers and pending native Data-header activation | Existing Windows GLM; no new Office regression UI | Changed diagnostic screenshots remain pending native verification |

The [documentation receipt](../../evidence/04A11/documentation-impact.json) pins
each before/after page. Method IDs, slugs, `_meta/methods.yml`, generated metadata,
resolver, navigation identities, examples/data/assets and existing cross-links
remain unchanged. No fictional “Using the Office add-in” subsection was added.
The qualified local MkDocs strict build exits **0**; original Help/catalog/link
and actual built-page checks run in both full configurations. There is no missing
documentation runtime or new screenshot fixture. Aalen–Johansen/Fine–Gray and
07.08–07.10 are untouched.

## Verification, numerical provenance and limitations

All eight frozen four-family cases per runtime preserve actual Core cells,
vectors, table order/types/metadata and actual convergence. Double tolerance is
`max(1e-9, abs(expected)*1e-9)`; strings/types/axes are exact within a runtime;
only elapsed-time strings are masked. Invariant/sk-SK numeric cells compare to
frozen values, and culture-dependent Core strings compare within the same
culture. No localized numeric reparse is added. Existing NIST/GLM independent
reference tests retain their original tolerances and fixtures. No reference was
promoted or an external scientific review self-certified.

[Independent diagnostic candidate](../../evidence/04A11/diagnostic-reference-candidate.json)
uses separately computed Python standard-library all-zero/all-one observations
and empirical boundary variance. It identifies a diagnostic condition, not a
new fitted reference or a universal HL p-value theorem. It is NOT_GOLDEN.
Qualified R was not needed or rerun; the old R startup investigation is untouched.

Focused tests cover the four real pipelines, optional sections, probability sums,
arbitrary/sparse labels, reference axes, reversed rows, ZIP independence/starts,
NB power-link metadata/fixed-versus-estimated boundary, malformed/nonfinite inputs,
missing output, cloning, constant/separated/nonconverged states and recovery,
covariance name immutability and requested/default/invalid LM alpha. Existing
04A.09 edge/transport and Core numerical suites were rerun, not duplicated or
weakened. Source evidence establishes wiring, not native chart/control execution.

Full verifier sequences ran once per configuration. A final ABI audit required
retaining the accepted four-argument entry point; an unavailable-fit classification
guard also completed the defensive path. Only affected Common/Windows suites and
source checks were rerun for these late changes. Earlier raw runs and intermediate
source/issue receipts remain intact. No unrelated suite was rebaselined.

The literal `scripts/verify-beshstatng.ps1` is unchanged and was **not executed
directly**: its fixed historical TestResults/Codex writers and restore/packaging
hooks conflict with the stronger retention/offline/native restrictions. Audited,
source-hashed execution copies run the same portable/Legacy/WASM/JS/Python
sequence with fresh destinations, `--no-restore`, VS 2026, safe host compile
targets and continuation after known failures. Exact diffs, commands and exit
codes are in [command outcomes](../../evidence/04A11/command-outcomes-final.json).

Principal commands from repository root (`<PYTHON_EXE>` is the owner's specified
portable WinPython executable, not a substituted Python):

```powershell
& <PYTHON_EXE> -B scripts/verify-04a11.py init --evidence TestResults/04A-11-20261103-run01
# Explicit pre-edit capture flag used only for these create-new captures.
& <PYTHON_EXE> -B scripts/verify-04a11.py capture --evidence TestResults/04A-11-20261103-run01 --name prechange-net10-02
& <PYTHON_EXE> -B scripts/verify-04a11.py capture --evidence TestResults/04A-11-20261103-run01 --name prechange-net48 --windows
& <PYTHON_EXE> -B scripts/verify-04a11.py focused --evidence TestResults/04A-11-20261103-run01 --name focused-08-api --windows
& <PYTHON_EXE> -B scripts/verify-04a11-candidate.py --name candidate-03
& <PYTHON_EXE> -B scripts/verify-04a11-probe.py
& <PYTHON_EXE> -B scripts/verify-04a11.py docs --evidence TestResults/04A-11-20261103-run01 --name docs-build-strict
& <PYTHON_EXE> -B scripts/verify-04a11.py final --evidence TestResults/04A-11-20261103-run01 --configuration Debug --name final-Debug
& <PYTHON_EXE> -B scripts/verify-04a11.py final --evidence TestResults/04A-11-20261103-run01 --configuration Release --name final-Release
& <PYTHON_EXE> -B scripts/verify-04a11.py managed --evidence TestResults/04A-11-20261103-run01 --configuration Debug --name final-compat-Debug
& <PYTHON_EXE> -B scripts/verify-04a11.py managed --evidence TestResults/04A-11-20261103-run01 --configuration Release --name final-compat-Release
node --test BESHStat.WasmSmoke/tests/*.test.cjs
& <PYTHON_EXE> -B scripts/verify-04a11.py historical --evidence TestResults/04A-11-20261103-run01 --name historical
& <PYTHON_EXE> -B scripts/verify-04a11.py preserve --evidence TestResults/04A-11-20261103-run01 --name final
git -c safe.directory=<REPO> diff --check
git -c safe.directory=<REPO> diff --cached --exit-code
git -c safe.directory=<REPO> status --short --untracked-files=all
```

The full execution copies pass `-Legacy -Wasm -Js -Python -VisualStudio 2026`,
the requested configuration and the specified Python executable. Supplementary
CJS repetition is reported separately and not added to distinct Node counts.
Focused/semantic repeats are not padded into final totals. Linked managed cases
are separately reported as runtime executions and deduplicated logical cases.
Final counts, every failure identity, preservation and the acceptance checklist
follow in the appended closeout ledger.

## Owner proposal and manual smoke NOT RUN

Accept the completed 04A.11 four-family/portable software scope, active defensive
Core correction and documentation, with the single bounded
`C11-NATIVE-CORRECTIONS-01` native timing deferral above. Do not call the dormant
LM GUI/Data-header activations fixed. Preserve all scientific/native qualifications
and historical nonpasses. No publication or next-batch permission is requested.

Use the [current validation receipt](../../evidence/04A11/validation-final.json)
and [current publication manifest](../../evidence/04A11/publication-manifest-final.json).
Earlier current-batch receipts remain as chronological snapshots and are explicitly
superseded. In particular, final logical-case accounting verifies shared source
hashes and reconciles MSTest string quoting, rather than equating unrelated case
names or counting the same linked case twice.

Later authorized native smoke proposals — **NOT RUN**:

1. Fit NB2 with offset/unequal weights, covariance/residual/iteration toggles and
   compare Data/GLM NB2; fit ZIP with distinct count/logistic designs/starts and
   compare both Data blocks and Zero-Inflated Poisson tables. Check progress,
   cancellation, failure dialogs and exact source RowIds against native pre-change
   evidence after risk/N authority.
2. Fit ordinal/multinomial with first/last references and noncontiguous rows;
   compare class labels, probability axes, residuals and table order. Separately
   verify constant-binomial diagnostics and no classification/chart attempt when
   Core report data is unavailable.
3. Only after exact activation authority, apply the current native candidate:
   LM alpha .10 must show 90% CIs (.05 remains 95%) with unchanged coefficients/
   SE/fitted rows; GLM/NB2 Data response header must stay the original name with
   covariance on/off. Revalidate screenshots/output labels under 04A.28/47.

No Excel/COM/ROT/UIA, native harness, Office authenticated workbook/browser smoke,
installer, deployment, signing, network/package installation, Git staging/commit/
merge/push or 04A.12 work was performed. Historical TestResults were not cleaned,
compressed, relocated, retimestamped or overwritten. Public derivatives exclude
local owner paths/SIDs/secrets; scan scope and hashes are explicit in the final
publication manifest, not a claim of general secret certification.


## Final validation and complete failure ledger

P/F/S/T below means passed / failed / skipped / total. Managed suite rows are distinct case executions within that assembly. The logical row merges cross-target cases only with an exact shared source hash and unambiguous class/method/data-row identity, normalizing the MSTest 2/3 difference in String quoting. Unresolved/ambiguous runtime cases remain separate. Any failed execution keeps the logical case failed; no failure is discarded. The full collision ledger is in validation-final.json. Focused/semantic repeats and extra CJS/probe runs are excluded.

| Suite | Debug P/F/S/T | Release P/F/S/T |
|---|---:|---:|
| Core | 1,055 / 0 / 0 / 1,055 | 1,055 / 0 / 0 / 1,055 |
| Common | 1,438 / 5 / 0 / 1,443 | 1,438 / 5 / 0 / 1,443 |
| Windows .NET Framework 4.8 | 1,336 / 2 / 0 / 1,338 | 1,336 / 2 / 0 / 1,338 |
| Office managed | 505 / 0 / 0 / 505 | 505 / 0 / 0 / 505 |
| WASM execution managed | 45 / 0 / 0 / 45 | 45 / 0 / 0 / 45 |
| Retained control projection | 61 / 0 / 0 / 61 | 61 / 0 / 0 / 61 |
| **Managed logical cases (deduplicated)** | **4,259 / 6 / 0 / 4,265** | **4,259 / 6 / 0 / 4,265** |
| Managed runtime executions (historical comparison only) | 4,440 / 7 / 0 / 4,447 | 4,440 / 7 / 0 / 4,447 |
| Node | 916 / 0 / 0 / 916 | 916 / 0 / 0 / 916 |
| Python distinct methods | 649 / 61 nonpasses / 0 / 710 | 649 / 61 nonpasses / 0 / 710 |

Python nonpasses split into **52 FAIL and 9 ERROR** per configuration (65 events including subtests). There are no skips. All **67 new logical managed cases** pass, comprising **55 Common and 67 Windows executions** (122 across runtimes); all nine new Python source checks pass. The frozen constant-binomial True case adds one logical failed case/two runtime failures because it still expects NullReferenceException. No old test, fixture or expected value was edited. No unexplained new functional failure remains.

Both complete verifier copies and both affected Common/Windows refreshes exit **1** for retained nonpasses. Core/Office/WASM tests, both modern host builds, safe Windows builds, all Node suites, candidate compilation, strict MkDocs, source recheck and the real-Core numerical probe exit **0**. The historical strict checker exits **1** at its old UibyID/NormalPlot stale review dependency; the profile checker exits **1** with INTEGRITY_FAILURE and an empty detail string. The actual raw outputs and hashes are preserved, not replaced by older checker totals.

The full Python runs precede only the narrowly documented ABI/failure-classification completion. Current source checks were rerun after those changes; their pass outcomes remain unchanged. Current Common/Windows suite results come from final-compat-Debug/Release. Original full-run results, the API-only refresh and current results remain separately retained.

### Exact managed nonpassing executions

| Class / exact case | Outcome | Attribution |
|---|---|---|
| `BESHStat.Excel.Common.Tests.Regression10PolicyTests.ConstantAndSeparatedBinomialPreserveActualFitWarnings (True)` | Failed | Frozen old broken-behavior expectation, superseded by active Core defensive report; remains FAIL |
| `BESHStat.Excel.Common.Tests.ProportionsPolicyTests.PairedDisjointCountContractAndPureSemanticProjection (20,30,50)` | Failed | Inherited 04A.10 identity/outcome |
| `BESHStat.Excel.Common.Tests.ProportionsPolicyTests.PairedDisjointCountContractAndPureSemanticProjection (20,15,40)` | Failed | Inherited 04A.10 identity/outcome |
| `BESHStat.Excel.Common.Tests.ProportionsPolicyTests.PairedDisjointCountContractAndPureSemanticProjection (0,0,0)` | Failed | Inherited 04A.10 identity/outcome |
| `BESHStat.Excel.Common.Tests.ProportionsPolicyTests.ExistingEquivalenceCoreDomainAndPairedOverflowAreNotReinterpreted` | Failed | Inherited 04A.10 identity/outcome |
| `BESHStatNG.Test.ContingencyAdapterTests.PairedCompatibilityEvidenceIsExplicitlyLegacy_NotACorrectedGolden` | Failed | Inherited 04A.10 identity/outcome |
| `BESHStatNG.Test.Regression10PolicyTests.ConstantAndSeparatedBinomialPreserveActualFitWarnings (True)` | Failed | Frozen old broken-behavior expectation, superseded by active Core defensive report; remains FAIL |

### Exact Python nonpassing methods

All original **54** identities and outcomes remain unchanged. Three additional identities were already stale at the live merged entry (Revision 2.7 versus the old source receipt, plus LF/CRLF capture receipt normalization). Four more are scope/hash guards triggered by the explicitly authorized Core report changes. The current source receipt proves all estimator statements and unrelated sources remain protected. These are nonpasses, never relabelled as green.

| Outcome | Exact method identity | Attribution |
|---|---|---|
| FAIL | `test_04a02_checkpoint09.Native09Tests.test_unapplied_patch_routes_every_active_selection_binding_and_guards_workbook` | Inherited reported 04A.10 nonpass |
| FAIL | `test_04a02_checkpoint11.Namespace11Tests.test_exact_retained_bytes_reproduce_only_compiler_rejection` | Inherited reported 04A.10 nonpass |
| ERROR | `test_04a02_checkpoint11.Provenance11Tests.test_both_authorized_case_pins_pass_and_frozen_false_is_not_owner_decision` | Inherited reported 04A.10 nonpass |
| ERROR | `test_04a02_checkpoint12.Diagnosis12Tests.test_both_authorized_case_pins_keep_original_compiler_and_scientific_policy` | Inherited reported 04A.10 nonpass |
| FAIL | `test_04a02_checkpoint12.Diagnosis12Tests.test_exact_owner_bytes_negative_for_frozen_v5` | Inherited reported 04A.10 nonpass |
| FAIL | `test_04a02_checkpoint13.Startup13Tests.test_corrupt_receipt_hash_fails_even_with_exit_zero` | Inherited reported 04A.10 nonpass |
| ERROR | `test_04a02_checkpoint13.Startup13Tests.test_real_successful_owner_startup_all_pins_and_native_identities` | Inherited reported 04A.10 nonpass |
| FAIL | `test_04a02_checkpoint13.Startup13Tests.test_tampered_stdout_hash_stops_before_parser` | Inherited reported 04A.10 nonpass |
| ERROR | `test_04a02_checkpoint14.Actual14Tests.test_actual_two_owner_publications_and_both_cultures_reproduce` | Inherited reported 04A.10 nonpass |
| ERROR | `test_04a02_source_specs.SourceSpecifications.test_all_native_blockers_and_anchored_mappings` | Inherited reported 04A.10 nonpass |
| FAIL | `test_04a03_control_mitigation.ControlMitigationSourceTests.test_per_mode_consumers_and_branch_order` | Inherited reported 04A.10 nonpass |
| FAIL | `test_04a03_control_mitigation.ControlMitigationSourceTests.test_sealed_sources_are_reconciled_never_rewritten` | Inherited reported 04A.10 nonpass |
| FAIL | `test_04a03_control_mitigation.NativeGateNegativeTests.test_review_packet_source_pins_match_current_sources` | Inherited reported 04A.10 nonpass |
| FAIL | `test_04a03_selection.UnivariateSelectionSourceTests.test_all_native_lifetime_cancel_write_and_reporting_methods_unchanged` | Inherited reported 04A.10 nonpass |
| FAIL | `test_04a03_selection.UnivariateSelectionSourceTests.test_only_two_form_methods_change` | Inherited reported 04A.10 nonpass |
| FAIL | `test_04a04_alignment.PairedBlockSourceGates.test_exact_crosswalk_and_authorities` | Inherited reported 04A.10 nonpass |
| FAIL | `test_04a04_alignment.PairedBlockSourceGates.test_legacy_geometry_ambiguity_is_not_hidden` | Inherited reported 04A.10 nonpass |
| FAIL | `test_04a05_completion_successor.Completion05SuccessorTests.test_independent_candidate_provenance_not_core_or_golden` | Inherited reported 04A.10 nonpass |
| FAIL | `test_04a05_completion_successor.Completion05SuccessorTests.test_nonpaired_code_and_unrelated_forms_are_unchanged_from_partial` | Inherited reported 04A.10 nonpass |
| FAIL | `test_04a05_completion_successor.Completion05SuccessorTests.test_owner_bound_successor_and_source_hashes` | Inherited reported 04A.10 nonpass |
| FAIL | `test_04a05_contingency.ContingencySourceGates.test_historical_ledgers_and_importer_unchanged` | Inherited reported 04A.10 nonpass |
| FAIL | `test_04a05_contingency.ContingencySourceGates.test_no_designer_udf_numerical_packaging_or_framework_change` | Approved Core change triggers frozen source/scope guard; successor proof retained |
| FAIL | `test_04a05_contingency.ContingencySourceGates.test_other_multicol_members_including_04a04_selection_unchanged` | Inherited reported 04A.10 nonpass |
| FAIL | `test_04a05_contingency.ContingencySourceGates.test_prior_04a04_whole_file_source_check_remains_frozen_and_disclosed` | Inherited reported 04A.10 nonpass |
| FAIL | `test_04a05_contingency.ContingencySourceGates.test_scientific_ambiguity_is_not_a_permanent_portable_wrong_table` | Inherited reported 04A.10 nonpass |
| FAIL | `test_04a05_contingency.ContingencySourceGates.test_ui82x2_source_wiring_explicitly_withheld` | Inherited reported 04A.10 nonpass |
| FAIL | `test_04a05_contingency.ContingencySourceGates.test_versioned_successor_sources_and_case_boundaries` | Inherited reported 04A.10 nonpass |
| FAIL | `test_04a06_assumptions_successor.Assumptions06SuccessorTests.test_actual_form_routes_and_unchanged_native_tails` | Inherited reported 04A.10 nonpass |
| FAIL | `test_04a06_assumptions_successor.Assumptions06SuccessorTests.test_importer_udf_designers_core_and_old_tests_unchanged` | Inherited reported 04A.10 nonpass |
| FAIL | `test_04a06_assumptions_successor.Assumptions06SuccessorTests.test_native_population_is_union_not_sum` | Inherited reported 04A.10 nonpass |
| FAIL | `test_04a06_assumptions_successor.Assumptions06SuccessorTests.test_new_scientific_candidates_are_explicitly_unmodified` | Inherited reported 04A.10 nonpass |
| FAIL | `test_04a06_assumptions_successor.Assumptions06SuccessorTests.test_no_duplicate_statistics_in_form_or_windows_adapter` | Inherited reported 04A.10 nonpass |
| FAIL | `test_04a06_assumptions_successor.Assumptions06SuccessorTests.test_production_sources_and_exact_open_ledger_lineage` | Inherited reported 04A.10 nonpass |
| FAIL | `test_04a07_workflow_successor.Anova07SuccessorTests.test_core_old_tests_importer_udfs_designers_and_help_unchanged` | Inherited reported 04A.10 nonpass |
| FAIL | `test_04a07_workflow_successor.Anova07SuccessorTests.test_exact_source_and_original_ledger_rows` | Inherited reported 04A.10 nonpass |
| FAIL | `test_04a07_workflow_successor.Anova07SuccessorTests.test_no_fabricated_obligation_for_previously_source_qualified_controls` | Inherited reported 04A.10 nonpass |
| FAIL | `test_04a07_workflow_successor.Anova07SuccessorTests.test_restriction_unions_and_timing_exceptions_not_generalized` | Inherited reported 04A.10 nonpass |
| FAIL | `test_04a08_authoring_successor.Authoring08SuccessorTests.test_exact_id_crosswalk_retains_original_row_hashes_and_open_counts` | Inherited reported 04A.10 nonpass |
| FAIL | `test_04a08_authoring_successor.Authoring08SuccessorTests.test_original_numerics_and_shipping_forms_are_unchanged` | Inherited reported 04A.10 nonpass |
| FAIL | `test_04a09_regression_successor.Regression09SuccessorTests.test_exact_six_changed_members_and_unchanged_native_surroundings` | Inherited reported 04A.10 nonpass |
| FAIL | `test_04a09_regression_successor.Regression09SuccessorTests.test_frozen_tests_numerics_and_public_04a08_evidence_preserved` | Approved Core change triggers frozen source/scope guard; successor proof retained |
| FAIL | `test_04a09_regression_successor.Regression09SuccessorTests.test_real_six_routes_use_common_options_counts_and_reports` | Inherited reported 04A.10 nonpass |
| FAIL | `test_04a09_regression_successor.Regression09SuccessorTests.test_runtime_baselines_are_incremental_and_complete` | Inherited reported 04A.10 nonpass |
| FAIL | `test_04a10_regression_projection.Regression10ProjectionTests.test_baselines_are_actual_gaussian_and_binomial_and_not_goldens` | Live-entry normalization/source-receipt drift, plus current approved source changes |
| FAIL | `test_04a10_regression_projection.Regression10ProjectionTests.test_core_udfs_frameworks_and_accepted_evidence_are_byte_protected` | Live-entry normalization/source-receipt drift, plus current approved source changes |
| FAIL | `test_04a10_regression_projection.Regression10ProjectionTests.test_only_two_form_routes_change_and_native_tails_match` | Live-entry normalization/source-receipt drift, plus current approved source changes |
| FAIL | `test_04a27_geometry_completion.Geometry27CompletionTests.test_compile_targets_and_native_helpers_unchanged` | Inherited reported 04A.10 nonpass |
| FAIL | `test_04a27_geometry_completion.Geometry27CompletionTests.test_initial_implementation_and_historical_tests_not_rewritten` | Approved Core change triggers frozen source/scope guard; successor proof retained |
| FAIL | `test_04a27_geometry_completion.Geometry27CompletionTests.test_real_consumer_routes_and_udf_exports_preserved` | Inherited reported 04A.10 nonpass |
| FAIL | `test_04a27_geometry_completion.Geometry27CompletionTests.test_source_owner_pins_and_all_44_original_rows` | Inherited reported 04A.10 nonpass |
| FAIL | `test_04a27_transport_successor.Transport27SuccessorTests.test_core_designer_and_refedit_sources_are_unmodified` | Approved Core change triggers frozen source/scope guard; successor proof retained |
| FAIL | `test_04a27_transport_successor.Transport27SuccessorTests.test_exact_successor_and_unapplied_patch` | Inherited reported 04A.10 nonpass |
| ERROR | `test_capture_contract04a02.CaptureContracts.test_all_239_ledger_rows_identified_with_original_digest_convention` | Inherited reported 04A.10 nonpass |
| ERROR | `test_capture_contract04a02.CaptureContracts.test_complete_source_crosswalk_equality` | Inherited reported 04A.10 nonpass |
| FAIL | `test_capture_contract04a02.CaptureContracts.test_packet_covers_all_original_cases_and_six_gates` | Inherited reported 04A.10 nonpass |
| ERROR | `test_capture_contract04a02.CaptureContracts.test_packet_retains_additional_21_deferred_obligations` | Inherited reported 04A.10 nonpass |
| ERROR | `test_investigation06.Investigation06Tests.test_all_239_exact_ids_have_reversible_root_cause_source_maps` | Inherited reported 04A.10 nonpass |
| FAIL | `test_investigation06.Investigation06Tests.test_anova_defaults_mapping_remain_explicit_source_facts` | Inherited reported 04A.10 nonpass |
| FAIL | `test_investigation06.Investigation06Tests.test_normality_5000_disagreement_preserved_as_review_defect` | Inherited reported 04A.10 nonpass |
| FAIL | `test_investigation06.Investigation06Tests.test_refedit_disposal_and_sheetactivate_guards_are_source_not_native_proof` | Inherited reported 04A.10 nonpass |
| FAIL | `test_investigation06.Investigation06Tests.test_two_by_two_or_condition_and_no_chart_handler` | Inherited reported 04A.10 nonpass |

### Preservation, publication and acceptance checklist

Preservation **PASS**: **180,375 historical files**, zero membership/metadata/source-pin errors and **532,742,897,664 bytes free** at the receipt. Checks cover original path membership, size, creation/write nanosecond timestamps, file/device identity and frozen source hashes; they are not a full byte rehash of historical TestResults. All **3,198 protected entry source/document files**, including 76 prior public evidence files and 842 architecture/planning files, retain their entry bytes.

Current form SHA-256 `47d99e58838647bba33610e834d9eadd59db0fb5c8bd31f3274a02f7a34a94f6`; current dormant patch SHA-256 `2283553890563a7b2e662699d7c05319d30a5cc932099179fd24d88c1516b3fc`. Both that patch and C09 strict activation remain UNAPPLIED. `git diff --check` and empty-index checks pass. Publication candidates remain non-ignored and privacy-scanned; raw owner-local evidence is separately ignored and retained.

| Required result | Disposition |
|---|---|
| Verified merged base/new branch and protected history | MET; local main/HEAD 463aaa12a6342cedfa084e266d5a985461de9d3a, no Git publication |
| Four substantive family intents/results consumed by shipping writers | MET; exact Core input captures, Core-backed parity and executable net48 adapter/writer tests |
| Three correction investigations and permitted implementations | MET for authorized portable/Core work; two Windows activations remain explicitly gated |
| Same-batch Help/Office availability/documentation | MET; six existing pages, strict build/Help checks pass; native screenshots NOT RUN |
| Focused/full/culture/negative verification and honest failure ledger | MET; all new cases pass; inherited/superseded/source-pin failures retained |
| Original native/scientific restrictions and IDs/hashes | MET as restrictions retained; zero release/discharge; P NO / N1 NO GO / N2 NO / C2 UNMET |
| Owner acceptance boundary | PENDING; accept software with the single C11-NATIVE-CORRECTIONS-01 timing deferral; no 04A.12 authorization |
