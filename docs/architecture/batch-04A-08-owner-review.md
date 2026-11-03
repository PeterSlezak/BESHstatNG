# Official 04A.08 — portable regression-effect authoring: owner review

**READY_FOR_OWNER_04A08_ACCEPTANCE_REVIEW** — the permitted software scope is
implemented and wired to the shipping controller. This requests owner review;
it does not record owner acceptance, native equivalence, scientific promotion,
Git publication, or authorization for 04A.09.

## Baseline and boundaries

Branch **`codex/04A-08`** was created from clean local `main` at
**`b48877406c3c185a992d5dd5c42f951611a52569`**. HEAD remains that commit.
The available local `origin/main`, `origin/codex/04A-07`, and `codex/04A-07`
refs named the same SHA. No fetch, pull, remote query, push, stage, commit,
merge, reset, stash or clean was performed. Cached tracking agreement is not
verification of the current server state. No unrelated owner modification was
present at entry or overwritten. The first Git invocation reported ownership
mismatch; subsequent commands used a repository-scoped `safe.directory` option.
Creating the explicitly authorized branch required the sandbox's approval to
write its ref; no global Git configuration or filesystem ACL was changed.

The live Master Plan is **Revision 2.5**. Its exact 04A.08 row, dependencies
01.07/04A.01, Windows implementation / Office contract-only boundary and
zero-host-dependency exit were reviewed. It remains unchanged. The live accepted
04A.07 `AnovaWorkflowPolicy` / `WindowsAnovaWorkflowAdapter` and actual grouped,
repeated and nested wiring are present. The 04A.05 input/report policies and
04A.06 `AssumptionReportPolicy` remain present. The accepted physical-area
importer remains SHA-256
`32942d3034c2fb80befe23e6bb317b9dde3097774b9cef0373536d9199f36fc9`.
The old pre-merge `b984e20...` was not used as this batch's base.

The baseline inventory was captured before editing shipping source. Its full
source hashes and original controller members are in the local-only
[baseline](04A08/baseline.json) and
`TestResults/04A-08-20261103-run01/baseline.json`.
The new executable oracle retains the original controller rules; its only
instrumentation replaces dialogs with message/state collectors and renames the
class. The original removal helper and its dependencies have a separate
source-derived executable oracle. These are compatibility evidence, **NOT_GOLDEN**
and not independent scientific/native proof. Original historical tests and pins
were not changed. Later message-state instrumentation changes only the new test
oracle's collector; initial and final oracle hashes are both retained.

## Implementation and ownership

The new APIs are in `BESHStatNG.ExcelCommon`:

* `RegressionEffectAuthoringPolicy.Apply`, `Remove`, `Inspect`, and
  `InspectHierarchy` own ordered selection, term authoring, duplicate/conflict
  decisions, optional inspection and legacy removal rules.
* `RegressionEffectModel`, `RegressionAuthoredEffect`, `RegressionEffectResult`,
  diagnostics/severity/operation contracts, and optional limits describe state
  without UI objects. `Changed` and `PartialSuccess` distinguish partial success
  from a failed operation that added nothing. Diagnostics carry raw/effect keys,
  not fabricated spreadsheet coordinates.
* `RegressionEffectSerialization.Serialize`, `TryDeserialize`, and
  `ProjectFormula` provide a versioned typed representation and a separately
  validated Core RHS projection.

Input sequences are materialized. Effects are read-only collections of frozen
wrappers; every mutable Core `TermSpec`, base/display list, observed-level list
and dictionary projection is copied. Mutating an input, returned `TermSpec`,
Windows list, or dictionary cannot mutate an existing Common snapshot. Warning
diagnostics can carry an immutable snapshot from their original processing
boundary, preserving which valid effects are visible before each warning.

`RegressionEffectsController` keeps its existing constructor, five public authoring
methods and mutable `TermSpecs` property. It snapshots `Items`/`SelectedIndices`,
calls Common, projects results into the same form-owned dictionary, and presents
the existing messages. Existing item/selection state is retained for surviving
effects; additions append and removals proceed backwards. It contains no local
effect-key/scale/term-construction policy. Dictionary identity is preserved;
the dictionary's mutable `TermSpec` values are projection copies. Supplying an
existing synchronized ordinal dictionary remains the supported form contract.
The setter retains the supplied dictionary reference; the next projection
synchronizes its entries against the authoritative effect list.

The typed model-list branch of `UIprocedures.Remove_Item` now delegates to
Common through the same controller. Its untyped variable-list path, pre-existing
empty-list early return and other helpers remain unchanged. Every ordinary
add/remove/clear/source-refresh path maintains matching list/spec membership
and contiguous order. No new repair of externally corrupted legacy state is
asserted. The copied legacy text classification for removal deliberately
retains its old punctuation/whitespace quirks; it is not an RHS formula parser.

Core still owns `TermSpec`, `PredictorScale`, key/coefficient-name builders,
required-raw-variable extraction, formula parsing, reference coding, design
expansion, rank and fitting. No Core source changed. No alternative formula
grammar, contrast algorithm or fitter was added. Core/Common remain
`netstandard2.0`; Windows/Test remain .NET Framework 4.8.

[Ownership matrix](04A08/ownership-matrix.json),
[publication/source/member hashes](04A08/publication-inventory-v2.json), and
[original removal characterization](04A08/removal-characterization.json)
provide the concrete files, members and exact before/after SHA-256 values.
Member hashes normalize LF; file hashes cover physical bytes.

## Characterized authoring behavior

| Operation | Applied behavior and compatibility |
|---|---|
| Continuous main | Raw label, including ` | Var...`, is the effect key. Exact duplicate is skipped; existing categorical main entry warns and skips only that variable. Scale continuous, degree 1, coefficient label from Core. |
| Categorical main | Core `[Cat] ` key; rejects a base already used by another term unless its categorical entry already exists. Default treatment/reference semantics are unchanged. |
| Polynomial | Degree >=2; Core quoted key, invariant degree and coefficient label. Existing categorical main effect rejects only that variable. All legacy Int32 degrees >=2, including Int32.MaxValue, remain accepted by the shipping authoring path; this is not endorsement of numerical fit safety at that degree. |
| Pairwise | Selected indices are sorted in catalog order, then nested pairs enumerate `(0,1),(0,2),...`. Core quotes each complete raw key. Ordinal, case-sensitive duplicates are skipped. Categorical scale is inferred from existing categorical main entries. |
| Custom N-way | One source-ordered interaction, at least two selected variables; same Core key/label and categorical inference. Reordered interactions remain different legacy effect identities. |
| Hierarchy | Creation remains permissive without explicit parents. Optional inspection reports each parent as available/missing, in term/base order, with warnings. No insertion, removal, reordering or rank restriction is introduced by hierarchy inspection. |
| Mixed validity | Valid effects survive. Exact old messages and intermediate warning-boundary models match the frozen controller; additions after a warning are not prematurely displayed. |
| Empty/invalid degree | Same selection/too-few/degree messages. Invalid polynomial degree precedes empty-selection validation. Windows owns message title/style and display. |
| Remove/clear/re-add | Old main-effect removal cascades to dependent text-classified terms; derived-only removal preserves its parents. Survivors retain order, with refreshed `TermSpec.Order`. Clear empties the model; re-add uses the current catalog. |
| Identity/refresh | Ordinal case-only names stay distinct. Raw identity is independent of trimmed coefficient labels. Source refresh and missing/renamed-key inspection do not infer replacement variables. |

The frozen-controller test covers all five operations, seven selection masks,
three seed states, two cultures and repeat addition; comparisons include exact
keys, complete metadata, required raw keys, messages and state at each warning.
Removal tests compare every selected position across ordinary and punctuation
fixtures with the frozen helper. These nested comparisons are not counted as
thousands of separate MSTest identities.

Null/unknown selections have stable typed errors. Whitespace, quotes, colons,
`[Cat]` prefixes, Unicode, long labels and formula-looking strings are data;
raw identities are neither trimmed nor executed. Legacy key collisions still
obey Core's exact effect-key duplicate rules; no new disambiguating key syntax
was invented. Optional portable budgets cover catalog/key size, degree,
interaction width and effect count. They are opt-in: the shipping bridge does
not impose a new cap. The opt-in defaults are catalog 2048, key 32768 UTF-16
units, degree 1024 and custom width 32. Deserialization defaults to 4,194,304
characters, with an explicit override. These are new portable API choices,
not native authoring restrictions or statistical limits.

## Typed serialization, formulas and references

The two outputs are deliberately distinct:

1. Ordered Core keys plus cloned `TermSpec` metadata remain the shipping design
   input. `[Cat] x`, `"x"^2`, and `"x":"y"` are effect identities.
2. Optional RHS uses the existing Core grammar, such as `factor(A, ref=2)`,
   `A^3`, and `A:factor(B)`. Relative source-column tokens bind to the explicitly
   supplied ordered predictor catalog; the compiler must use that same order.
   This avoids ambiguous quoted display names and treats malicious-looking names
   as data. It is not an Office editor or an availability claim for model fitting.

Canonical typed **version 1** is compact XML with stable field/attribute order,
explicit intercept and `contrast="core-default"`. Effects, bases, labels, degree,
scale, order, reference and observed levels are retained. Null strings/lists are
distinguished from empty strings/lists. Ordinary text uses XML escaping.
CR/LF and XML-ineligible strings use `encoding="utf16le-base64"`, preserving exact
UTF-16 code units, including isolated surrogates and embedded NULs. Doubles use
16 uppercase hexadecimal IEEE-754 bits, preserving negative zero and avoiding
runtime-dependent decimal formatting. This is metadata serialization, not new
numeric or contrast computation. Unknown versions/fields, declarations/DTDs,
noncanonical input and size violations return the typed `serialization` error.
No XML external resource or filesystem access is used.

| Case | Core key/typed model | RHS disposition |
|---|---|---|
| Continuous/categorical/polynomial | Exact Core fields/order preserved | Existing parser normalizes and re-parses; supported design matrices and names match. |
| Continuous×categorical, categorical×categorical, 3-way | Same source order and Core reference treatment | Supported when Core's ordinal interaction normalization retains the authored order. |
| Explicit reference 2 | Reference bits and optional observed levels retained | `factor(A, ref=2)` is supported; no level is inferred. |
| Intercept on/off | Separate Boolean, round-trips | No inserted `1`, `0` or LHS; compiler receives the separate option. |
| Empty/intercept-only | Valid typed empty model, required raw keys empty | Typed not-representable diagnostic: blank Core RHS means default main effects, so it must not be emitted as an empty model. |
| Reordered interaction | Exact legacy source order retained | Refused if parser normalization changes identity/order. |
| Whitespace identity, custom display metadata, fitted observed levels | Lossless typed representation | Refused where RHS/design normalization cannot retain that metadata. |
| Quotes/Unicode/case-only or duplicate coefficient labels | Raw keys remain distinct | Relative tokens avoid name-resolution ambiguity; quoted-name parsing remains Core-owned and covered by existing Core tests. |

Two existing compatibility findings are isolated, with executable reproducers:

* **C08-CORE-LABEL-01:** for a continuous main + categorical main + polynomial +
  interaction, the legacy Core label map can emit `Age^3:Group[1]` where a parsed
  formula emits `Age:Group[1]`. The fixture verifies equal numeric matrices and
  the differing labels. The new formula projector refuses that shared-base
  polynomial/interaction combination conservatively. Shipping keys, fitting and
  labels are untouched; this is not presented as a scientifically corrected label.
* **C08-FORMULA-REF-01:** Framework Core's normalized `G` reference formatting can
  round a high-precision factor reference. The projector re-parses normalized
  output and refuses loss of exact reference metadata. The typed bit representation
  remains lossless on both runtimes. No Core parser formatting was changed.

[Formula compatibility matrix](04A08/formula-compatibility.json) and
[before/after fixtures](04A08/authoring-fixtures.json) link these dispositions to
tests and assembly-attributed observations. The formula helper is additive and
not called by the existing Windows fitting paths, so its explicit refusals do
not activate a native behavior change. Observed-level validation needing data
remains Core-owned; inspection reports supplied-reference inconsistencies without
recoding levels or inventing contrasts.

## Every live consumer

| Form/component | Actual controller and dictionary | Evidence / limitation |
|---|---|---|
| GLM primary | `EffectsController`, `TermSpecs`, `lbSelectedEffectsList` | Five actual `Handles ...Click` routes remain; independent controller/dictionary regression and source constructor checks. |
| GLM ZIP logistic | `EffectsControllerLogistic`, `TermSpecsLogistic`, `lbSelectedEffectsListLogistic` | Separate five routes and removal/clear/source-refresh chain; no primary/logistic state sharing. |
| GEE | `EffectsController`, `TermSpecs` | Actual handlers route through the bridge; cluster/time/correlation/fitting source unchanged. |
| Cox | `EffectsController`, `TermSpecs` | Actual handlers preserved; censoring/ties/strata/residuals/plots/output unchanged. |
| MMRM | `EffectsController`, `TermSpecs` | Shared-controller compatibility and original event/source checks; broader model/covariance/reporting untouched. |
| LMM fixed | `FixedEffectsController`, `FixedTermSpecs` | Independent fixed/random models and dictionary ownership verified at the controller seam. |
| LMM random | `RandomEffectsController`, `RandomTermSpecs` | Five event routes plus removal/clear remain separate; no optimizer/LS-means/random covariance policy moved. |
| PSM | `EffectsController`, `TermSpecs` | Shared seam and actual event/source wiring checked; no matching/report pipeline changes. |

All six form files are byte-identical to entry. The tests instantiate standalone
managed ListBoxes and the shipping controller, on an STA thread, without showing
forms. Component-labelled seam cases exercise two independent model dictionaries;
they do not execute the shipping forms' constructors or synthesize real button
clicks in Excel. Actual event/constructor chains are verified from source.
No shipping form, live Excel/COM/ROT/UIA, Office/browser UI or native harness ran.
Cancellation/lifecycle/reporting paths are unchanged source, not newly established
native execution evidence.

## Original native restrictions

[Exact crosswalk](04A08/restriction-crosswalk.json) contains **87 source-associated
original control rows**, of which **42** have original deferred obligation rows.
It retains literal control/obligation IDs, complete original rows, row digests,
declaration/source pins, triage records, owners and gates. The other 45 entries
have null obligation IDs; no `control:` obligation was fabricated for them.
None of these 42 is a member of the four-family 239-obligation transfer set.
Source association includes reset/reload/tab events that clear model lists;
it is not a claim that every associated control was exercised at runtime.

The original amendment SHA-256 remains
`8ba5614757a20cd9ca7d075529c058072a250e4ae97fc00d66228c9703e22307`;
the original 04A.01 ledger remains
`363ae57510b613eef06faebba7bc7e4152b928e4d3bbad7c1f67eea589c56d10`.
All **1,514** original obligations, **802** restricted live controls,
**239 OPEN** transferred obligations and **218** transferred controls retain their
restrictions. These populations overlap and must not be added. The four deferred
ANOVA/normality/2×2/PCA cases, 141/160 shared ANOVA-normality controls/obligations,
44 importer timing obligations and prior scientific candidates remain intact.

Applied Windows work is behavior-preserving internal delegation, including the
warning boundary and legacy removal quirks. No behavior-changing restricted patch
or unapplied scientific correction is part of this delivery. No new timing
exception is requested or borrowed from 04A.05/04A.27. A future behavior change
still requires its exact pre-change evidence, mitigation, independent risk review
and native authority, or an exact owner disposition before activation.

**Released controls 0; discharged obligations 0; P=NO; N1=NO GO; N2=NO;
Gate C2=UNMET.** Native identity/loaded-build/HWND/PID/session and independent
Windows/COM risk prerequisites remain 04A.45 work. No R/Python reference was
promoted, no golden expectation changed, and no installer/signing/release ran.

## Validation and nonpasses

Final counts are identical in Debug and Release. Counts below are distinct
identities within each configuration; development/focused repetitions are not
added to these totals. The suite is **not all-green**.

| Suite | Debug pass / total | Release pass / total | Nonpass / skip each |
|---|---:|---:|---:|
| Core | 1,055 / 1,055 | 1,055 / 1,055 | 0 / 0 |
| Common | 1,257 / 1,261 | 1,257 / 1,261 | 4 / 0 |
| Windows | 1,113 / 1,114 | 1,113 / 1,114 | 1 / 0 |
| Office managed | 505 / 505 | 505 / 505 | 0 / 0 |
| WASM execution managed | 45 / 45 | 45 / 45 | 0 / 0 |
| Retained managed control projection | 61 / 61 | 61 / 61 | 0 / 0 |
| **Managed total** | **4,036 / 4,041** | **4,036 / 4,041** | **5 / 0** |
| Node | **916 / 916** | **916 / 916** | **0 / 0** |
| Python methods | **626 / 675** | **626 / 675** | **49 / 0** |

**04A.08-specific:** **40 Common + 26 Windows = 66/66** managed cases per
configuration, with zero failures/skips; **8/8** additive Python source/wiring
methods pass. The focused run also passes 20 existing ModelInputCompiler cases
(60 Common total), 26 Windows cases and 14 Core parser/design cases.
Final full Common/Windows refreshes include the LF-only identity edge case.
Twenty exact before/after authoring observations per configuration, covering
both cultures, pass with actual assembly hashes. These observations are not
additional MSTest identities or native GUI evidence. Existing Core formula tests
retain their historical numerical assertions; no statistical estimate was fixed.

The five managed nonpasses match 04A.07 **by identity and data-row name**:

* `ProportionsPolicyTests.PairedDisjointCountContractAndPureSemanticProjection`
  rows `(20,30,50)`, `(20,15,40)`, `(0,0,0)`;
* `ProportionsPolicyTests.ExistingEquivalenceCoreDomainAndPairedOverflowAreNotReinterpreted`;
* `ContingencyAdapterTests.PairedCompatibilityEvidenceIsExplicitlyLegacy_NotACorrectedGolden`.

All **44** reported 04A.07 Python nonpass identities recur with the same outcomes.
Five further source-pin failures were already latent in the merged entry state:
one C06 and four C07 checks pin Master Plan Revision 2.4, while entry `main` has
the owner's Revision 2.5. Old pin:
`e08c89a060a250d8f600851a02aed124e0bfe74ceb1d8f04c8c6787ae0f4ec17`;
entry/current pin:
`666af3fc136ad34f2ed7fc4620d0a3b9349f783158864248fd3b22c79af895b9`.
The initial 04A.08 inventory and source-pin comparison prove that drift predates
this batch; the plan was not edited to repair these checks. Their identities are:

* `test_04a06_assumptions_successor.Assumptions06SuccessorTests.test_importer_udf_designers_core_and_old_tests_unchanged`;
* `test_04a07_workflow_successor.Anova07SuccessorTests.test_core_old_tests_importer_udfs_designers_and_help_unchanged`;
* `test_04a07_workflow_successor.Anova07SuccessorTests.test_exact_source_and_original_ledger_rows`;
* `test_04a07_workflow_successor.Anova07SuccessorTests.test_no_fabricated_obligation_for_previously_source_qualified_controls`;
* `test_04a07_workflow_successor.Anova07SuccessorTests.test_restriction_unions_and_timing_exceptions_not_generalized`.

The four C07 protected-source checks encounter only that plan difference among
their protected entry paths; the C06 check fails on the same plan pin. Thus the
live inherited population is 49 nonpassing methods, rather than the previously
reported 44. This is source attribution plus current executions; no pre-edit
whole-suite run is claimed. The reported 667-method old corpus would have
618 passes / 49 nonpasses on that entry state. Eight new passing methods give
626/675. No inherited failure is skipped, xfailed, rebaselined or relabelled as a
pass. The complete 49 identities/outcomes and 53 failure events, including four
extra subtest events, are in [final validation](04A08/validation-v2.json).
The generated appendix below lists every nonpassing Python method.

The original `verify-beshstatng.ps1` is unchanged. Running it literally would
reuse historical `TestResults/Codex` report names, restore packages and invoke
legacy packaging/version targets. The accepted isolation helper therefore
produced hash/diff-recorded execution copies, using `--no-restore`, fresh report
paths and copied Common/Windows assemblies plus writer data. They preserve
independent suite execution after inherited failures. Both full verifier copies
ran `-Legacy -Wasm -Js -Python -VisualStudio 2026` in their stated configuration.
Windows production builds used `PrepareForBuild;PrepareResources;Compile;CopyFilesToOutputDirectory`
and disabled project-reference rebuilds, avoiding generators/signing/XLL packaging.
The .NET Framework MSTest discovery warning about old non-public `MatrixTestHelpers`
is retained and is not a new 04A.08 failure.

The initial `final-Debug` run is retained and superseded. Warning-boundary repair
and added cases justified `final-fixed-*`; final canonical double-bit/reference
hardening justified complete `final-managed-*` refreshes. A final LF-only string
edge check justified complete Common/Windows `final-authoring-*` refreshes.
Unchanged Core/Office/control-projection and full Node/Python results are retained
from their latest applicable runs. No redundant full Node/Python repetition was
needed for that isolated codec edge. Development compile/test and observation
helper failures are retained or disclosed; they are repaired implementation
attempts, not hidden final failures. Historical source assertions were untouched.

Exact principal commands (all from the repository root):

```powershell
$py = 'C:\Users\peter\Desktop\WPy64-31470\python\python.exe'
& $py -B scripts/verify-04a08.py init --evidence TestResults/04A-08-20261103-run01
& $py -B scripts/capture-04a08-contract.py
& $py -B scripts/verify-04a08.py focused --evidence TestResults/04A-08-20261103-run01 --name focused-canonical
dotnet test BESHStat.Core.Tests/BESHStat.Core.Tests.vbproj -c Debug --no-restore --filter 'FullyQualifiedName~RegressionFormulaParserBuilder|FullyQualifiedName~RegressionDesign' --logger 'trx;LogFileName=CoreFocused.trx' --results-directory TestResults/04A-08-20261103-run01/core-focused
& $py -B scripts/verify-04a08.py final --evidence TestResults/04A-08-20261103-run01 --configuration Debug --name final-fixed-Debug
& $py -B scripts/verify-04a08.py final --evidence TestResults/04A-08-20261103-run01 --configuration Release --name final-fixed-Release
& $py -B scripts/verify-04a08.py managed --evidence TestResults/04A-08-20261103-run01 --configuration Debug --name final-managed-Debug
& $py -B scripts/verify-04a08.py managed --evidence TestResults/04A-08-20261103-run01 --configuration Release --name final-managed-Release
& $py -B scripts/verify-04a08.py authoring --evidence TestResults/04A-08-20261103-run01 --configuration Debug --name final-authoring-Debug
& $py -B scripts/verify-04a08.py authoring --evidence TestResults/04A-08-20261103-run01 --configuration Release --name final-authoring-Release
powershell -NoProfile -ExecutionPolicy Bypass -File scripts/capture-04a08-observations.ps1 -AssemblyDirectory TestResults/04A-08-20261103-run01/executions/final-authoring-Debug-Legacy/sandbox/Legacy/bin/Debug/net48 -EvidenceDirectory TestResults/04A-08-20261103-run01 -Configuration Debug -Name complete
powershell -NoProfile -ExecutionPolicy Bypass -File scripts/capture-04a08-observations.ps1 -AssemblyDirectory TestResults/04A-08-20261103-run01/executions/final-authoring-Release-Legacy/sandbox/Legacy/bin/Release/net48 -EvidenceDirectory TestResults/04A-08-20261103-run01 -Configuration Release -Name complete
& $py -B -m unittest discover -s scripts/tests -p test_04a08_authoring_successor.py -v
& $py -B scripts/capture-04a08-contract.py check
& $py -B scripts/summarize-04a08.py
& $py -B scripts/verify-04a08.py preserve --evidence TestResults/04A-08-20261103-run01 --name final
git -c safe.directory=C:/Users/peter/source/repos/BESHstatNG diff --check
git -c safe.directory=C:/Users/peter/source/repos/BESHstatNG diff --cached --exit-code
git -c safe.directory=C:/Users/peter/source/repos/BESHstatNG status --short --untracked-files=all
git -c safe.directory=C:/Users/peter/source/repos/BESHstatNG diff --stat
```

Full/managed/authoring commands exit **1** for the inherited failures; compile
stages, Core/Office/Node, focused authoring, source/wiring, observations and
preservation checks pass. `*-command.json`, execution provenance, all TRXs and
`final-fixed-*-full.log` / `-common-python.log` retain exact expanded commands,
outcomes, timings and paths. This includes npm/Node `.cjs` execution, rather than
relying on managed discovery. WASM projects built; browser/Excel smoke is NOT RUN.
Raw evidence remains local under `TestResults/04A-08-20261103-run01/`.

## Preservation and publication preflight

The preservation receipt is **PASS**, with no errors, covering **167,994**
historical TestResults files: complete initial membership, sizes, creation/write
timestamps, file/device identity and frozen source/authority hashes. This is not
a second complete historical-byte hash scan. The receipt records free disk
**535,568,515,072 bytes**. No historical TestResults cleanup, relocation,
compression, deletion, retimestamping, baseline edit or overwrite occurred.
All writes there use this batch's fresh evidence root. Initial free disk was
approximately 499.77 GiB; thresholds were advisory and no cleanup was attempted.

Only three existing tracked files changed:

* `BESHstatNG/src/UI/RegressionEffectsController.vb`;
* `BESHstatNG/src/DM/UIprocedures.vb` (the typed removal branch only);
* `BESHStatNG.Test/BESHStatNG.Tests.vbproj` (three explicit test/oracle includes).

The normally visible new publication candidates are:

* `BESHStat.Excel.Common/src/Input/RegressionEffectAuthoringPolicy.vb`;
* `BESHStat.Excel.Common/src/Input/RegressionEffectSerialization.vb`;
* `BESHStat.Excel.Common.Tests/src/RegressionEffectAuthoring08Tests.vb`;
* `BESHStatNG.Test/src/RegressionEffects08Tests.vb`;
* `BESHStatNG.Test/src/LegacyRegressionEffects08.vb`;
* `BESHStatNG.Test/src/LegacyRegressionRemoval08.vb`;
* `scripts/verify-04a08.py`;
* `scripts/capture-04a08-contract.py`;
* `scripts/capture-04a08-observations.ps1`;
* `scripts/summarize-04a08.py`;
* `scripts/tests/test_04a08_authoring_successor.py`;
* this dossier.

Current ignored JSON publication candidates in `docs/architecture/04A08/` are
`removal-characterization.json`, `restriction-crosswalk.json`,
`ownership-matrix.json`, `formula-compatibility.json`, `authoring-fixtures.json`,
`validation-v2.json`, and `publication-inventory-v2.json`. These need a separate
later publication decision; nothing was force-added. `baseline.json` is the
larger local-only source capture; `validation.json` is retained superseded v1
validation. `publication-inventory.json` is retained superseded v1 preflight;
v2 qualifies member names by class so all constructors have distinct entries.
These three local-only drafts are not proposed as current publication evidence. The exact
normally visible/ignored/local-only list and source/member hashes are recorded
in the publication inventory, which excludes its own recursive content hash.
The raw historical/local TestResults tree is not a publication candidate.

`git diff --check` passes; the index is empty. Source-byte review found only the
intended extraction/removal branch/project inclusions. The rewritten controller
is UTF-8/LF; no unrelated line-ending-only change was introduced. SDK Common/test
projects include new VB files automatically; the legacy test project explicitly
includes all three new test files. Designers, framework targets, native helpers,
Ribbon/UDF registrations, methods/help metadata, old tests, installers, signing,
version attributes and all six consumer form files remain unchanged. No Master
Plan completion claim or 04A.09 work is included.

## Future manual checks — NOT RUN

1. After separately approved native case/identity/risk authority, exercise GLM
   primary and ZIP logistic models with categorical and polynomial conflicts,
   multi-selection in reverse click order, partial-valid warnings, interactions
   without explicit parents, remove/clear/re-add and intercept on/off. Confirm
   intermediate warning state, exact list labels/order and component isolation;
   fitting/output/native equivalence still requires its own approved evidence.
2. Under separately approved LMM/MMRM native scenarios, author independent fixed
   and random models, remove a main effect with dependencies and refresh the
   source list. Confirm dictionary/list synchronization, cancel/close and no
   cross-component mutation. No optimizer, covariance, LS-means or report
   equivalence follows from this authoring checklist.

Recommended single owner disposition: **APPROVE software-scope 04A.08 with the
documented inherited failures, formula limitations and all original native and
scientific restrictions retained.** No exact-ID native timing waiver is needed
for this behavior-preserving delivery. Only the owner can accept the batch.
Publication and starting 04A.09 require separate authorization.


## Complete Python nonpass identities

Both configurations have the same outcomes. **FAIL/ERROR remain nonpasses.**

| Outcome | Exact method ID | Entry classification |
|---|---|---|
| FAIL | `test_04a02_checkpoint09.Native09Tests.test_unapplied_patch_routes_every_active_selection_binding_and_guards_workbook` | Retained 04A.07 nonpass |
| FAIL | `test_04a02_checkpoint11.Namespace11Tests.test_exact_retained_bytes_reproduce_only_compiler_rejection` | Retained 04A.07 nonpass |
| ERROR | `test_04a02_checkpoint11.Provenance11Tests.test_both_authorized_case_pins_pass_and_frozen_false_is_not_owner_decision` | Retained 04A.07 nonpass |
| ERROR | `test_04a02_checkpoint12.Diagnosis12Tests.test_both_authorized_case_pins_keep_original_compiler_and_scientific_policy` | Retained 04A.07 nonpass |
| FAIL | `test_04a02_checkpoint12.Diagnosis12Tests.test_exact_owner_bytes_negative_for_frozen_v5` | Retained 04A.07 nonpass |
| FAIL | `test_04a02_checkpoint13.Startup13Tests.test_corrupt_receipt_hash_fails_even_with_exit_zero` | Retained 04A.07 nonpass |
| ERROR | `test_04a02_checkpoint13.Startup13Tests.test_real_successful_owner_startup_all_pins_and_native_identities` | Retained 04A.07 nonpass |
| FAIL | `test_04a02_checkpoint13.Startup13Tests.test_tampered_stdout_hash_stops_before_parser` | Retained 04A.07 nonpass |
| ERROR | `test_04a02_checkpoint14.Actual14Tests.test_actual_two_owner_publications_and_both_cultures_reproduce` | Retained 04A.07 nonpass |
| ERROR | `test_04a02_source_specs.SourceSpecifications.test_all_native_blockers_and_anchored_mappings` | Retained 04A.07 nonpass |
| FAIL | `test_04a03_control_mitigation.ControlMitigationSourceTests.test_per_mode_consumers_and_branch_order` | Retained 04A.07 nonpass |
| FAIL | `test_04a03_control_mitigation.ControlMitigationSourceTests.test_sealed_sources_are_reconciled_never_rewritten` | Retained 04A.07 nonpass |
| FAIL | `test_04a03_control_mitigation.NativeGateNegativeTests.test_review_packet_source_pins_match_current_sources` | Retained 04A.07 nonpass |
| FAIL | `test_04a03_selection.UnivariateSelectionSourceTests.test_all_native_lifetime_cancel_write_and_reporting_methods_unchanged` | Retained 04A.07 nonpass |
| FAIL | `test_04a03_selection.UnivariateSelectionSourceTests.test_only_two_form_methods_change` | Retained 04A.07 nonpass |
| FAIL | `test_04a04_alignment.PairedBlockSourceGates.test_exact_crosswalk_and_authorities` | Retained 04A.07 nonpass |
| FAIL | `test_04a04_alignment.PairedBlockSourceGates.test_legacy_geometry_ambiguity_is_not_hidden` | Retained 04A.07 nonpass |
| FAIL | `test_04a05_completion_successor.Completion05SuccessorTests.test_independent_candidate_provenance_not_core_or_golden` | Retained 04A.07 nonpass |
| FAIL | `test_04a05_completion_successor.Completion05SuccessorTests.test_nonpaired_code_and_unrelated_forms_are_unchanged_from_partial` | Retained 04A.07 nonpass |
| FAIL | `test_04a05_completion_successor.Completion05SuccessorTests.test_owner_bound_successor_and_source_hashes` | Retained 04A.07 nonpass |
| FAIL | `test_04a05_contingency.ContingencySourceGates.test_historical_ledgers_and_importer_unchanged` | Retained 04A.07 nonpass |
| FAIL | `test_04a05_contingency.ContingencySourceGates.test_other_multicol_members_including_04a04_selection_unchanged` | Retained 04A.07 nonpass |
| FAIL | `test_04a05_contingency.ContingencySourceGates.test_prior_04a04_whole_file_source_check_remains_frozen_and_disclosed` | Retained 04A.07 nonpass |
| FAIL | `test_04a05_contingency.ContingencySourceGates.test_scientific_ambiguity_is_not_a_permanent_portable_wrong_table` | Retained 04A.07 nonpass |
| FAIL | `test_04a05_contingency.ContingencySourceGates.test_ui82x2_source_wiring_explicitly_withheld` | Retained 04A.07 nonpass |
| FAIL | `test_04a05_contingency.ContingencySourceGates.test_versioned_successor_sources_and_case_boundaries` | Retained 04A.07 nonpass |
| FAIL | `test_04a06_assumptions_successor.Assumptions06SuccessorTests.test_actual_form_routes_and_unchanged_native_tails` | Retained 04A.07 nonpass |
| FAIL | `test_04a06_assumptions_successor.Assumptions06SuccessorTests.test_importer_udf_designers_core_and_old_tests_unchanged` | Entry Revision 2.5 plan conflict |
| FAIL | `test_04a06_assumptions_successor.Assumptions06SuccessorTests.test_native_population_is_union_not_sum` | Retained 04A.07 nonpass |
| FAIL | `test_04a06_assumptions_successor.Assumptions06SuccessorTests.test_new_scientific_candidates_are_explicitly_unmodified` | Retained 04A.07 nonpass |
| FAIL | `test_04a06_assumptions_successor.Assumptions06SuccessorTests.test_no_duplicate_statistics_in_form_or_windows_adapter` | Retained 04A.07 nonpass |
| FAIL | `test_04a06_assumptions_successor.Assumptions06SuccessorTests.test_production_sources_and_exact_open_ledger_lineage` | Retained 04A.07 nonpass |
| FAIL | `test_04a07_workflow_successor.Anova07SuccessorTests.test_core_old_tests_importer_udfs_designers_and_help_unchanged` | Entry Revision 2.5 plan conflict |
| FAIL | `test_04a07_workflow_successor.Anova07SuccessorTests.test_exact_source_and_original_ledger_rows` | Entry Revision 2.5 plan conflict |
| FAIL | `test_04a07_workflow_successor.Anova07SuccessorTests.test_no_fabricated_obligation_for_previously_source_qualified_controls` | Entry Revision 2.5 plan conflict |
| FAIL | `test_04a07_workflow_successor.Anova07SuccessorTests.test_restriction_unions_and_timing_exceptions_not_generalized` | Entry Revision 2.5 plan conflict |
| FAIL | `test_04a27_geometry_completion.Geometry27CompletionTests.test_compile_targets_and_native_helpers_unchanged` | Retained 04A.07 nonpass |
| FAIL | `test_04a27_geometry_completion.Geometry27CompletionTests.test_real_consumer_routes_and_udf_exports_preserved` | Retained 04A.07 nonpass |
| FAIL | `test_04a27_geometry_completion.Geometry27CompletionTests.test_source_owner_pins_and_all_44_original_rows` | Retained 04A.07 nonpass |
| FAIL | `test_04a27_transport_successor.Transport27SuccessorTests.test_exact_successor_and_unapplied_patch` | Retained 04A.07 nonpass |
| ERROR | `test_capture_contract04a02.CaptureContracts.test_all_239_ledger_rows_identified_with_original_digest_convention` | Retained 04A.07 nonpass |
| ERROR | `test_capture_contract04a02.CaptureContracts.test_complete_source_crosswalk_equality` | Retained 04A.07 nonpass |
| FAIL | `test_capture_contract04a02.CaptureContracts.test_packet_covers_all_original_cases_and_six_gates` | Retained 04A.07 nonpass |
| ERROR | `test_capture_contract04a02.CaptureContracts.test_packet_retains_additional_21_deferred_obligations` | Retained 04A.07 nonpass |
| ERROR | `test_investigation06.Investigation06Tests.test_all_239_exact_ids_have_reversible_root_cause_source_maps` | Retained 04A.07 nonpass |
| FAIL | `test_investigation06.Investigation06Tests.test_anova_defaults_mapping_remain_explicit_source_facts` | Retained 04A.07 nonpass |
| FAIL | `test_investigation06.Investigation06Tests.test_normality_5000_disagreement_preserved_as_review_defect` | Retained 04A.07 nonpass |
| FAIL | `test_investigation06.Investigation06Tests.test_refedit_disposal_and_sheetactivate_guards_are_source_not_native_proof` | Retained 04A.07 nonpass |
| FAIL | `test_investigation06.Investigation06Tests.test_two_by_two_or_condition_and_no_chart_handler` | Retained 04A.07 nonpass |

Review artifact generation: `& $py -B scripts/summarize-04a08.py review`; final inventory generation: `& $py -B scripts/summarize-04a08.py closeout`. Both operate only on this batch's new review files and read existing evidence.
