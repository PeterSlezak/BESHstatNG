"""Prepare the local owner-review candidate only after substantive software gates."""
from work import *
sys.path.insert(0,str(HERE.parent))
from verify_manifest import SOURCES,SELF,expected_scope,file_pin,validate_crosswalk,crosswalk_inputs
from summarize_final import clean,display
P=HERE.parent;READY='READY_FOR_OWNER_04A34_ACCEPTANCE_REVIEW'
def load(n):return json.loads((P/n).read_bytes())
def put(n,v):write('evidence/04A34/'+n,json.dumps(v,ensure_ascii=False,indent=2,allow_nan=False)+'\n')
def qualify():
    v=load('validation.json');f=load('family-coverage.json')
    assert not v['unclassified_new_managed_nonpasses'] and not v['unclassified_new_python_nonpasses']
    assert all(r['removed']==r['changed_baseline_outcomes']==0 for r in v['identity_deltas'])
    assert v['managed_totals']['Debug']==v['managed_totals']['Release']=={'Passed':20326,'Failed':14}
    assert v['new_managed_cases_per_configuration']==181
    assert v['node']=={'total':916,'passed':916,'failed':0}
    assert v['strict_profile_mkdocs']==dict(strict=1,profile=1,docs=0)
    assert f['open_blockers']==f['historical_open_blockers']==0 and len(f['families'])==17
    assert load('source-preservation.json')['status']=='PASS'
    assert load('completion/renderer-preservation.json')['status']=='PASS'
    assert load('completion/capture-comparison.json')['status']=='PASS'
    validate_crosswalk(*crosswalk_inputs())
    v.update(decision=READY,final_full_managed_label='review3',focused_label='review3',node_and_python_matrix_label='completed',final_python_scripts_label='review',publication='Local proposed packet only; no upload, Git publication or native approval')
    f.update(status=READY)
    put('validation.json',v);put('family-coverage.json',f)
    old=load('compatibility-casebook.json')
    old.update(scientific='NOT_GOLDEN',supplemental=load('completion/capture-comparison.json'),frozen_original_corpus=load('completion/frozen-corpus-preservation.json'),source_slices=['completion/geometry-spec.json','completion/extra-geometry-spec.json','completion/preparation-spec.json','completion/series-spec.json','completion/policy-spec.json','completion/spc-basis-spec.json'],limitations=['Host-free calculations and real Windows pure adapters only; no native chart/shape execution or fake chart proof.','Accepted Core factories provide typed fixture inputs/source identities; they do not supply an independent scientific oracle. Source slices are compiled separately and do not call new methods.','Native error/measurement/lifetime source fingerprints and option policies supplement numerical parity; native visual equivalence remains NOT_RUN.','Original invalid/no-extension ECDF aliases and symmetry input sorting remain explicit host exceptions to copy-safe payloads.'])
    put('compatibility-casebook.json',old)
def dossier():
    v=load('validation.json');f=load('family-coverage.json');s=load('source-preservation.json');g=load('git-final.json');c=load('coordinate-and-chart-spec-contract.json');a=load('audit-results.json')
    live={n['key']+'@'+str(n['source_span_start'])+':'+str(n['source_span_length']):n for n in load('completion/member-source.json')['current']}
    family='\n'.join('|'+r['family']+'|'+str(r['current_distinct_candidate_members'])+'|'+str(r['historical_candidates'])+'|'+('Changed' if r['actually_changed'] else 'Reused / native')+'|'+', '.join(k+':'+str(n) for k,n in r['current_dispositions'].items())+'|' for r in f['families'])
    api='\n'.join('|'+r['source']['path'].split('/')[-1]+'|'+', '.join(sorted({i.split('|')[1].split('.')[-1]+':'+live[i]['name'] for i in r['windows_consumption']}))+'|' for r in c['contracts'])
    suite=[]
    for name in ['Core','Common','Legacy','OfficeAddin','OfficeExecution','ControlMitigation']:
        d=next(r for r in v['managed'] if r['suite']==name and r['configuration']=='Debug');r=next(r for r in v['managed'] if r['suite']==name and r['configuration']=='Release')
        fmt=lambda t:str(t.get('Passed',0))+' / '+str(t.get('Failed',0))
        suite.append('|'+name+'|'+fmt(d['baseline_debug'])+'|'+fmt(d['outcomes'])+'|'+fmt(r['outcomes'])+'|')
    py=v['python'];audit=str(a.get('passed'))+' PASS' if a.get('command_receipt','').endswith('-04A34-packet-audits') else 'Final execution pending; checkpoint23 PASS is historical only'
    nonpasses=load('nonpasses-final.json')['nonpasses'];count=__import__('collections').Counter((r['suite'],r['class_name'],r['method']) for r in nonpasses if r['configuration']=='Debug')
    inherited='\n'.join('- `'+suite+': '+cls+'.'+method+'` ('+str(n)+' raw occurrence'+('s' if n!=1 else '')+').' for (suite,cls,method),n in sorted(count.items()))
    sourcefails='\n'.join('- `'+r['identity']+'` '+r['subtest']+' — '+r['classification']+'. '+r['proof'] for r in load('source-and-node-validation-final.json')['new_nonpasses'])
    census=load('historic-census-metadata.json');cap=load('completion/capture-comparison.json');capturecounts={r['family_capture']:r['rows'] for r in cap['comparisons'] if r['configuration']=='Debug'}
    paths='\n'.join('- `'+p+'`' for p in SOURCES)
    text=f'''# Official batch 04A.34 owner acceptance review

**{READY}** — bounded software portability and Windows delegation only. All seventeen basic family responsibilities are accounted for, with **zero current OPEN** dispositions. This is an owner-review candidate, not acceptance, scientific-golden validation, native Excel approval or authorization to ship. No04A.35 or later work began.

Branch `codex/04A-34`; HEAD, main and merge-base `{g['head']}`. Parent `43d20f86b80934c49b2f3dec90824aa62af5aaee` is the historical04A.33 pre-merge reference. Accepted04A.33 production, tests, dossier and manifest were verified on merged main. The owner's Master Plan Revision2.12, official34–37/39–48 rows, deferred-defect charter,04A.01 ledger, accepted29 v2 lineage and29–33 dossiers/current sources remain authoritative and byte-preserved. Owner acceptance supersedes the stale “33 next” wording; the plan was not rewritten.

Continuation entry preserved the existing partial implementation and verified its96 external manifest hashes plus nonrecursive self-entry. It did not restart from a ZIP. [Continuation entry](../../evidence/04A34/completion/entry.json) preserves actual source/evidence hashes and the partial metadata snapshot. [Original entry](../../evidence/04A34/entry-authority.json) and [prior publication check](../../evidence/04A34/prior-publication-check.json) retain merged-main ancestry and58 accepted33 external entries with explicit Git/EOL qualification. No unrelated owner modification was observed. Final [Git status/stat/index](../../evidence/04A34/git-final.json) records an empty index and unstaged/uncommitted changes; no reset, stash, cherry-pick, stage, commit, merge, push, upload or deployment occurred.

## Implemented contracts and Windows integration

Fourteen Windows source files delegate to26 new portable Core/Common source contracts. Three test projects add only required source links. All previous Core/Common engines and04A.30–33 source bytes remain unchanged; no estimator, matching/flow solver, binning, CI or regression replacement was introduced. Core/Common remain netstandard2.0 with Option Strict On and no native dependency; Windows/Test remain net48. Ribbon/UI/Designer/RefEdit/UDF/help/Office/WASM/installer/signing targets are unchanged.

| Portable source | Actual Windows consuming member(s), including helper chains |
|---|---|
{api}

Bullet now consumes explicit-size band/bar/target/scale/tick rectangles and label positions, preserving clipping, admission, palette rank, current-culture numeric format policy, input order and actual/target boundaries. Native font/text acquisition, RGB, enums and shape creation remain Windows. Existing ComputeLayout/ComputeRow/MapX/Clip01 semantics are unchanged.

Sankey consumes ordered node/anchor/ribbon cubic controls, stage and legend geometry and labels with the existing percentage basis/culture. The accepted staged flow solver, category identity and curvature are reused. Native freeform path order, measured text, mutable RGB overrides, resize registry and cleanup remain Windows. No Bullet clipping convention is substituted.

CDF retains accepted empirical/fitted/percentile vectors, step endpoints, group ordering and Percent defaults. It now delegates axes, numeric formats, legend representative policy, names/titles and percentile references. The private invalid/no-extension wrapper keeps original array aliases. Dumbbell delegates axes, connector/endpoint/category/auxiliary vectors, numeric labels and source ordinals; native date interpretation and pixel label offset remain. Ladder delegates axes, endpoint/side/label vectors, text, explicit acquired-height separation and legend flags, reusing stable spreading/clamping. Native HashSet legend lifetime and measurement acquisition remain at their original call points.

Agreement's four public overloads retain their signatures and accepted GetPlotData engines. Basic observed/reference/fit endpoints and the Bland–Altman base span are consumed without refitting. Multiple comparison retains title/validation/order/SuggestedHeight semantics with basic coordinates/zero-reference and height projection. Detailed agreement/CI/error/report/Love work remains04A.35. PSM retains accepted absolute-SMD/order/mask/table/maxX/threshold policy and Y reversal; its basic finite/threshold intent is consumed. SPC reuses ten-channel projection and adds only basic bounds/optional extent/constant references/panel placement/text; detailed channel/stage/zone annotations remain04A.36. No native SPC controls are released.

Calibration preserves bin order, historical NaN-only admission (including infinity), identity and fixed[0,1] axes. Symmetry preserves upper-tail X/lower-tail Y, original host sorting/alias and exceptions; its deferred caption is unchanged. Accepted33 wire sphere/ellipsoid projection/sampling/current-state drawing are untouched. Theil–Sen remains accepted Core plot data. Hull/Kite/Polar reuse accepted points/polygons/grid/source order; Hull/Polar label policies and explicit measured Polar square are consumed. GIF retains only the tested delay conversion; encoding/GDI+/raster/file/event/lifetime stays native.

All new scalar/settings/geometry results are readonly to external consumers. Vector payloads copy constructor inputs and getters, including label/source vectors. Accepted Core source ordinals are zero-based input identities; grouped/series/stage/link ordinals remain distinct and native Points are one-based. No worksheet row is inferred from values-only arrays. Settings are read per preparation/draw call, after existing validation, rather than frozen at constructor time; repeated calls and live appearance mutations are tested. Detailed API/header/shape/missing/culture ownership is in the [contract](../../evidence/04A34/coordinate-and-chart-spec-contract.json).

## All17 families and exact member identities

The [historical crosswalk](../../evidence/04A34/operation-map-final.csv) retains768 unique historical IDs with one disposition each. The [current map](../../evidence/04A34/family-ownership-map.csv) retains exactly764 pre-existing continuation candidate identities, with separate final successor IDs/source spans; new portable members are a separate census. Zero historical or current OPEN entries remain. HOST_ONLY reasons are specific to native state, validation/marshalling, concrete styles, measurement or lifetime; mixed numerical methods delegate their pure preparation. Later reservations cite official35/36 scope. This does not certify completion of those batches.

| Family | Pre-existing current candidates | Historical IDs | Source | Current dispositions |
|---|---:|---:|---|---|
{family}

The four previously unmatched headers remain truthful relocations, without fabricated same-file matches:

- Ladder `c2396206bed850c5@30089:21` → portable SpreadLabelPositions OrderBy value selector; `@30172:13` → its ThenBy original-ordinal tie breaker.
- Love `53143a2ee7cb7aa2@8095:41` → PsmLoveSeriesPolicy.HasAnyFinite's Any predicate, with the same NaN/infinity admission.
- SPC `ca14220ee65e6934@51030:1071` → SpcAxisBoundsCore.HasNaturalZero, called by FromExtents through the actual Windows ComputeYAxisBounds.

Three additional explicit mappings record removal of the unused duplicate private Ladder.Clamp in favor of its already-consumed portable clamp, and Sankey's two stable sort selectors into Common order policies. The final source has six unmatched historical same-file headers (the original four plus Clamp and one Sankey header); the seventh relocation is a same-header selector whose operator ownership moved. Exact IDs, token hashes, operator context, source spans and actual caller edges are in [member consumption](../../evidence/04A34/completion/member-consumption.json). Canonical29 v2 historical uncertainties171/172 remain unresolved; these relocations do not discharge them.

## Compatibility authority and renderer preservation

The original544 adapter rows,512 layout cases and1752 reference/bounds rows and their source signatures/hashes remain byte-identical to continuation entry. Before each additional seam changed, independent test-only original-source bodies/prefixes were frozen and compiled/executed. Source/namespace/owner/accepted-engine binding adaptations are explicit in the supplemental specifications. New Core is never used as both oracle and actual. Accepted Core factories supply typed fixture inputs and source identity, labelled ACCEPTED_CORE/SAME_CORE_WIRING rather than independent scientific evidence.

Final actual pure Windows adapters and portable contracts reproduce **{sum(capturecounts.values())} frozen rows per configuration**, Debug and Release, with exact CSV/IEEE values, signed zero and raw NaN bits. Capture groups: `{capturecounts}`. Post payloads are content-identical and remain ignored to avoid duplicate published megabytes; [comparison receipts](../../evidence/04A34/completion/capture-comparison.json), original bytes and executable capture readers reproduce them. Additional managed checks compare NaN by classification and preserve observable exception types/parameter names, null/empty/mismatched/constant/tied/nonfinite paths, sentinel/order/cardinality and deliberate aliases.

Independent analytical tests cover identity/zero references, measured bar mapping, Bézier endpoints, square centering, SPC padding/bounds and symmetry orientation, alongside frozen compatibility. Input/getter mutation, repeated reads, settings snapshots and native-boundary date/legend state are tested. [Entry portable IL](../../evidence/04A34/completion/entry-portable-il.csv) compares all prior partial Core methods in their original types using resolved metadata-token normalization; existing engines are separately protected by exact source hashes. All software data remains **NOT_GOLDEN**; no R/scientific oracle was executed.

[Renderer preservation](../../evidence/04A34/completion/renderer-preservation.json) compares native call sequence/count/arguments, style assignments, exception boundaries and native measurement order against frozen continuation sources. Only six exact coordinate/copied-label substitutions are qualified: Bullet title width, subtitle top, tick bottom, actual-label top; Dumbbell copied category text; Sankey stage-label left. All other recorded arguments/styles are exact. Pure numeric prefixes move; native chart/shape/freeform/series loops, RGB/fonts/enums, error/lifetime tails stay Windows. CDF legend branch intent and Ladder's real host-free HashSet state are tested separately. Merged-main public Windows headers are preserved by Roslyn; private Clamp/HasNaturalZero removals are explicit. This is SOURCE_ONLY boundary evidence, not fake-chart or real Excel visual equivalence.

## Actual validation and retained nonpasses

Commands use the accepted isolated offline runner, preserving historical output folders. [Final command receipts](../../evidence/04A34/command-outcomes-final.json) retain actual arguments, scope, exit codes and raw-log hashes, including earlier harness/compile failures and successful corrections. Public commands use `<PYTHON>`/`<REPO_ROOT>`/`<ISOLATED_RUN>` placeholders. Final Debug/Release compile-only host and all portable/test builds exit0; no packaging/signing hooks ran. Full suites with inherited failures exit1 and remain failed suites.

| Managed suite | Fresh-main Debug PASS / FAIL | Final Debug PASS / FAIL | Final Release PASS / FAIL |
|---|---:|---:|---:|
{chr(10).join(suite)}

Totals: **20,326 PASS /14 FAIL /0 ERROR /0 SKIP** per configuration. **181 new raw**04A.34 passes per configuration, including the original105 partial passes plus76 continuation passes. No accepted33 identity/occurrence was removed or changed; no new managed failure was introduced. Full final identity CSV and inherited nonpass records preserve multiplicities in [identities](../../evidence/04A34/managed-test-identities-final.csv) and [nonpasses](../../evidence/04A34/nonpasses-final.json). The partial identity/nonpass receipts remain historical and unmodified. Focused04A.30–34 Core/Common/Framework runs pass in both configurations; exact counts are in validation.json.

The14 inherited raw failures per configuration are:

{inherited}

Node: **916 PASS /0 FAIL**, identical baseline identities. Python: **{py['passed']} PASS /{py['failed']} FAIL /{py['errors']} ERROR**, with exact nonpass identities and source transitions retained. Accepted33 was832/114/10; fresh34 entry830/116/10 included two entry privacy-artifact failures, subsequently corrected. Partial final818/127/11 was a different execution identity set, not an interchangeable baseline. Final packet audits: **{audit}**. Strict/profile exits **1/1** remain genuine nonzero gates; strict MkDocs exits **0**.

New Python nonpasses relative to fresh-main entry are qualified individually below; original assertions/hashes were not altered:

{sourcefails}

The old SPC source-dictionary test is unordered: a changed native-member hash may FAIL before lookup of relocated HasNaturalZero, or lookup may ERROR KeyError first. Both historical run outcomes remain recorded; this is a source-inspection failure, not a product runtime/statistical error. The additive PSM and Agreement source-count assertions remain genuine FAILs. No unexplained functional/statistical failure remains. [Source/Node validation](../../evidence/04A34/source-and-node-validation-final.json) compares exact identities and source hashes, and [validation](../../evidence/04A34/validation.json) binds counts to receipts.

Fresh entry full Release was **NOT_RUN**, using fresh-main Debug plus the independently accepted33 Release reference; both final configurations executed locally. Full wrapper scripts `verify-beshstatng.ps1` and `-Legacy` remain **NOT_RUN** under the continuation's explicit isolation restriction: they touch shared historical outputs and Legacy packaging. The equivalent isolated Core/Common/Framework/Office, Node/Python, strict/profile/docs lanes ran. No wrapper PASS is inferred.

## Preservation, privacy and local publication inventory

All **{s['tracked_entry_files']}** tracked entry files were compared by actual SHA256/size: **{s['unchanged']} unchanged**, **{len(s['changed'])} authorized tracked source/project transitions**, zero unrelated mismatches. Owner plan, all previous tests/evidence/controls and accepted Core engines remain unchanged. Original278 lineage pins and100 protected mixed-model captures were independently checked; original pins, hashes, historical test statuses/manifests and canonical29 v2 corrections were not rewritten. New authorized source hashes are successor transitions, never retroactive old-pin passes. See [preservation](../../evidence/04A34/source-preservation.json), [source transitions](../../evidence/04A34/source-transitions.json), [lineage](../../evidence/04A34/lineage-check.json) and [captures](../../evidence/04A34/mixed-model-captures-check.json).

Historical payload census: **{census['count']} files /{census['bytes']} bytes**, matching accepted metadata membership/size/mtime hash. The roughly138GB payload population was **NOT_REHASHED**. Only tracked source/evidence and100 protected captures were byte-checked as stated; historical ignored payload bytes are excluded. The owner's prior ZIP independently attested3673 unchanged packaged files and12 transitions, with389 historical tracked files absent from that ZIP; that is review evidence only. The actual4074-entry repository ledger remains the main authority. No historical cleanup/deletion was performed or authorized.

The [local proposed manifest](../../evidence/04A34/publication-manifest.json) contains every source/test/project/dossier/evidence artifact with exact byte size/SHA256 and exactly one null/nonrecursive self-entry. The independent reader imports no producer and rejects missing/foreign/duplicate/case/traversal/absolute/size/hash/self tampering. New negative audits also reject missing candidates, disconnected ports and false relocations/OPEN-to-READY claims. Privacy scans cover proposed external contents/paths/decoded JSON; raw private logs/builds and duplicate post captures stay ignored. Privacy and manifest receipts are refreshed after final artifacts; no upload or publication occurred.

## Restricted gates, deferred issues and manual work

**P=NO; N1=NO GO; N2=NO; Gate C2=UNMET; Gate C3=UNAPPROVED.** Existing C13/survival and SPC/native restrictions remain. G29-SYMMETRY-AXIS-CAPTION, C25-LOVE-THRESHOLD-MISSING-01, C25-LOVE-AXIS-NOFINITE-01, regression/report/axis candidates and canonical29 uncertainties171/172 are unchanged. Recorded infinity-admission and short-symmetry-vector candidates remain deferred; no convenient numerical correction is included. Detailed35/36/37 work and39–48 defect/native adjudication are not implemented.

**NOT_RUN:** live Excel/COM/UIA/RefEdit, native controls/visual parity, independent R, Mac/Online/Office/LibreOffice UI, installer/package/signing/deployment. Proposed independently approved native smokes remain PSM Love (finite/missing/category/reversal), SPC (channels/stages/exclusions/specs), calibration/agreement, Bullet/Sankey/CDF/Dumbbell/Ladder/symmetry specialty options, and survival only as the plan's gated manual example. Existing WASM UI smoke is not native approval. No gate is discharged by a portable payload.

## Owner review and Git publication checklist

1. Review this dossier, the complete member/contract/source-transition maps, genuine nonpasses and independently verified manifest against a fresh owner-prepared project ZIP/current repository.
2. Confirm the approved basic34 scope, all seventeen dispositions/relocations, unchanged native/scientific gates and exact preservation evidence. Keep accepted historical payloads; post-phase cleanup needs a separate explicit owner task.
3. If independently accepted, the owner may stage/commit/merge in a separate authorized action. This agent leaves everything unstaged and uncommitted and does not start04A.35.

## Proposed source/test/project/dossier paths

{paths}
'''
    for old,new in [('No04A.','No 04A.'),('its96','its 96'),('and58','and 58'),('to26','to 26'),('All17','All 17'),('original544','original 544'),('rows,512','rows, 512'),('and1752','and 1752'),('include768','include 768'),('retains768','retains 768'),('exactly764','exactly 764'),('new raw**04A.34','new raw** 04A.34'),('original105','original 105'),('plus76','plus 76'),('independently accepted33','independently accepted 04A.33'),('roughly138GB','roughly 138 GB')]:text=text.replace(old,new)
    write('docs/architecture/batch-04A-34-owner-review.md',text)
def seal():
    assert load('validation.json')['decision']==READY
    put('publication-manifest.json',dict(schema='04A34-publication-v1',decision=READY,files=[dict(path=p,bytes=None,sha256=None) if p==SELF else file_pin(p) for p in sorted(expected_scope())],qualification='Local proposed owner-review packet only; no upload, staging, commit, merge, native/scientific approval or deployment. Sole null self-entry is nonrecursive.'))
if __name__=='__main__':
    action=sys.argv[1]
    if action=='qualify':qualify()
    elif action=='dossier':dossier()
    elif action=='seal':seal()
    else:raise ValueError(action)
