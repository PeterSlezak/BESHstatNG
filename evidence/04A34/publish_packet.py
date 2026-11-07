"""Prepare a local NOT_READY review packet. Never stage, commit, upload or publish."""
from entry import ROOT,HERE,BASE,save,pin
from verify_manifest import SOURCES,SELF,expected_scope,file_pin
import importlib.util,json,re
DOSSIER='docs/architecture/batch-04A-34-owner-review.md'
def load(n):return json.loads((HERE/n).read_bytes())
def dossier():
    v=load('validation.json');family=load('family-coverage.json');contracts=load('coordinate-and-chart-spec-contract.json')
    audit=load('audit-results.json') if (HERE/'audit-results.json').exists() else {'status':'NOT_RUN'}
    families='\n'.join('|'+r['family']+'|'+('Changed' if r['actually_changed'] else 'Unchanged')+'|'+str(r['current_distinct_candidate_members'])+'|'+str(r['current_dispositions'].get('OPEN_BLOCKER',0))+'|'+r['implementation_outcome']+'|' for r in family['families'])
    api='\n'.join('|'+r['source']['path'].split('/')[-1]+'|'+r['windows_consumption']+'|' for r in contracts['contracts'])
    suites=[]
    for suite in ['Core','Common','Legacy','OfficeAddin','OfficeExecution','ControlMitigation']:
        d=next(r for r in v['managed'] if r['suite']==suite and r['configuration']=='Debug');r=next(r for r in v['managed'] if r['suite']==suite and r['configuration']=='Release')
        fmt=lambda x:str(x.get('Passed',0))+'/'+str(x.get('Failed',0))+'/0/0'
        suites.append('|'+suite+'|'+fmt(d['baseline_debug'])+'|'+fmt(d['outcomes'])+'|'+fmt(r['outcomes'])+'|')
    failures='\n'.join('- `'+r['suite']+': '+r['class_name']+'.'+r['method']+'` — '+r['test_name'] for r in load('nonpasses.json')['nonpasses'] if r['configuration']=='Debug')
    files='\n'.join('- `'+p+'`' for p in SOURCES if p!=DOSSIER)
    text=f'''# Official batch 04A.34 partial implementation and owner evidence

**NOT_READY_04A34.** This packet preserves a tested partial extraction. It is not a completed acceptance candidate. The remaining implementation/member-reconciliation work is stated below; successful local tests do not close it. No04A.35 or later batch began.

Verified branch `codex/04A-34`; main, HEAD and merge-base all `{BASE}`. Main's parent is `43d20f86b80934c49b2f3dec90824aa62af5aaee`, the historical04A.33 pre-merge base. Main contains the accepted04A.33 production/tests/dossier/manifest. Entry tracked worktree/index were clean. Branch creation required the sandbox's local Git metadata approval; command-scoped safe.directory was used without global configuration changes. No reset, checkout of older source, cherry-pick, staging, commit, merge, push or deployment occurred.

The Master Plan Revision2.12, official34/35/36/37/39–48 rows, deferred-defect charter, post-phase retention rules,04A.01 ledger and04A.29–33 dossiers/current sources supplied scope. Owner acceptance supersedes the stale “33 next” wording. The plan is byte-preserved. Canonical04A.29 v2 lineage corrections remain authoritative; the two old helper uncertainties171/172 remain unresolved. Prior33's58 external publication entries were checked against merged Git with exact direct/inverse-EOL digest qualification. No ZIP was used as write authority or claimed newly inspected.

The index remains empty. Final status/stat/HEAD checks are in [git-final.json](../../evidence/04A34/git-final.json). Only12 previously tracked source/project files change;17 source/test files are additive. New dossier/evidence files are separate. Existing Core/Common engines, tests, UI/Designer/Ribbon/UDF/help/Office/WASM sources, framework targets and packaging/signing settings remain unchanged. Core/Common remain netstandard2.0 and Windows/Test remain net48.

## Implemented scope and current Windows consumption

| New portable source/API | Actual Windows consumption point |
|---|---|
{api}

Calibration now snapshots ordered precomputed bin points at the original rendering-time loop, with zero-based bin-input ordinals, a two-point identity and fixed[0,1] axes. Historical admission rejects NaN only and admits infinities; no finite correction or bin/CI/refit was introduced. Constructor aliases and title defaults remain Windows-owned.

Symmetry snapshots the existing sorted-tail arithmetic and axis scaling. Windows still calls the accepted Median and sorts its original aliased array at the original time. Upper-tail X/lower-tail Y, reference endpoint selection, small-vector exceptions and current captions are preserved. G29-SYMMETRY-AXIS-CAPTION remains unapplied. There is no fabricated worksheet-row identity.

Ladder label spreading moves with its original stable value/index order, clamp passes and explicit bounds/separation. The host still obtains native measurements. ECDF endpoint extension moves into a copy-safe payload; the private Windows invalid/no-extension paths retain their original array aliases. No CDF convention or filtering changes.

Bullet/Sankey measured chart layout and scalar mapping consume explicit host sizes/current numerical settings. Drawing/row layout results contain immutable value properties. Bullet's comparison-based clipping and Sankey's Math.Min/Max clipping stay distinct. The original constants, errors, stage-centre order and measured-size arithmetic are preserved. Their remaining bar/target/ribbon/node/label operations are **open work**, not falsely declared host-only.

One copy-safe two-point reference primitive removes duplicate endpoint construction for calibration, agreement, multiple comparisons and Love. Agreement fit endpoints use supplied intercept/slope, never a refit. Comparison ordinal Y and zero-inclusive baseline bounds move; CI error-length/title/validation/height behavior stays original and detailed35 work is reserved. Love reuses accepted PsmLovePlotProjection without changing SMD/order/masks/table/threshold/maxX; only finite-series admission and threshold reference intent move. SPC moves post-extents padding/domain bounds while keeping accepted ten-channel/mask/stage projection and native marshalling. Detailed36 annotations remain reserved. GIF milliseconds-to-centiseconds arithmetic moves; encoder/GDI+/rasterization/file/event/lifetime ownership remains Windows.

The [contract](../../evidence/04A34/coordinate-and-chart-spec-contract.json) lists real APIs/shapes, missing policies, identity and native limits. [Roslyn evidence](../../evidence/04A34/source-ownership.json) compares current/entry headers and member tokens across22 family sources, inventories950 current declarations and checks38 Windows Graphics includes and portable dependencies. It preserves existing public Windows headers; the private SPC HasNaturalZero helper is relocated into Core. This is syntax/source evidence, not live COM binding or complete mixed-renderer equivalence. Unchanged native members are token-compared; mixed methods still require the remaining family review.

## Seventeen-family accounting and blockers

The historical768 candidate declarations reconcile to764 distinct current candidate members; four historical header matches require relocation/reconciliation, rather than being invented as current members. There are {family['open_blockers']} current candidate **OPEN_BLOCKER** dispositions and {family['historical_open_blockers']} historical candidate OPEN_BLOCKER rows. These are conservative review/implementation dispositions, not a claim of126 missing independent algorithms. All768 historical IDs have exactly one recorded disposition, with current ranges/matches and targets. The complete22-file current census includes additional context outside the candidate package; it is not a migration count.

| Family | Source changed | Current candidate members | Open | Current outcome |
|---|---|---:|---:|---|
{families}

Minimum corrections before READY:

1. Complete remaining basic, reusable Bullet band/bar/target coordinates and numerical label/format preparation; Sankey cubic/ribbon/node anchors and label preparation; CDF/Dumbbell/Ladder axis and label seams; and the residual agreement base-endpoint seam. Preserve each native tail and actual option-evaluation timing. Add independent old-source captures for the affected members before further edits, actual adapter consumption and focused edge/copy/culture tests. These remain34 work, not35/36/37 authorization.
2. Resolve the126 current member dispositions and four unmatched historical headers with exact current/portable member evidence. Several conservative defaults intentionally remain OPEN_BLOCKER. The provisional HOST_ONLY/native and ALREADY_PORTABLE classifications require the final member-specific review before this table can certify completion. Do not convert arithmetic to HOST_ONLY merely to reduce the count.
3. Finish complete option/default/redraw/source-identity and renderer-tail evidence across the17-family map, including any direct public API argument edges not reached by the current validated adapters. Re-run affected lanes and reseal receipts/manifest after implementation. Existing passing subsets do not establish full-family coverage.

These are incomplete authorized implementation/evidence tasks, not a request to approve a later batch and not an assertion of an external permission blocker.

## Compatibility authority and tests

Before product edits,4074 tracked files and the candidate ownership rows were captured. Original compiled numerical slices, actual host-free adapter methods and explicit-size layout slices were executed against the accepted entry build. Frozen captures contain544 basic/adaptor rows,512 layout cases and1752 reference/bounds rows, in invariant and sk-SK cultures. The reference-source files were still unchanged when their supplementary slices were captured. Legacy slices adapt signatures/enclosing state/namespace/native measurement acquisition; original fragments and SHA256, accepted engine dependencies and exact compiled binding adjustments are recorded. New Core never supplies both expected and actual values.

The544-row actual host-free adapter post-capture matches its pre-edit corpus byte-for-byte. Layout and portable reference outputs are separately compared to executed legacy slices in managed tests. Finite/infinite/signed-zero values use exact IEEE bits; algorithmic NaNs use explicit classification in managed comparisons, with raw bits retained in CSV. Defensive-copy tests mutate inputs/getters, repeated reads and source ordinals; reference invariants are independently checked. These are **NOT_GOLDEN** software comparisons. Accepted StatFunc/Core dependencies are same-engine evidence, not independent scientific validation. No whole-chart, all-options or native-pixel equivalence is claimed.

Managed values are PASS/FAIL/ERROR/SKIP:

| Suite | Fresh production-main Debug | Final Debug | Final Release |
|---|---:|---:|---:|
{chr(10).join(suites)}
| **Total** | **20145/14/0/0** | **20250/14/0/0** | **20250/14/0/0** |

There are105 new raw managed passing cases per configuration:44 Core,2 Common and59 Framework, including linked shared tests. No original test identity/multiplicity/outcome was removed or changed. [Managed identities](../../evidence/04A34/managed-test-identities.csv) retain raw identity hashes and occurrence ordinals. Focused Core163/163,Common5/5,Framework214/214 pass in each configuration and include04A.30–33 regressions. These are distinct scopes, not totals to add together. All14 inherited managed failures remain:

{failures}

Node:916 PASS/0 FAIL in both entry/final runs; identity multisets retained. Explicit `node --test BESHStat.WasmSmoke/tests/*.test.cjs` additionally passes377 cases already included in the full916; do not double-count it.

Raw before-product Python was830 PASS/116 FAIL/10 ERROR, including two defects in the new entry-ledger publication artifact: existing privacy tests detected private path labels. The accepted33 reference was832/114/10. The raw receipts remain immutable, and their two extra failures are **new evidence defects**, not inherited product failures. The ledger's original private bytes were retained locally, path labels were redacted, and captured source digests were preserved. Existing privacy checks now pass. No original source/test/hash expectation was changed.

Latest full Python:818 PASS/127 FAIL/11 ERROR across956 reported tests, using the repository's unittest/subtest counting convention. Relative to the accepted product/source profile, fourteen new historical nonpasses are explained: eleven whole-file34 graphics pins, the26 agreement source pin, the25 five-policy census now six, and the22 retained-source-member comparison. The latter has an unordered member loop: an earlier run reports FAIL at ComputeYAxisBounds; the latest rerun reports ERROR KeyError HasNaturalZero after its private relocation. Both actual outcomes are retained. This error is a historical source-dictionary lookup, not a product statistical/runtime exception. The14 new nonpasses stay real nonpasses; old source assertions remain unchanged. All inherited identities and removed privacy-artifact failures are in [source/Node receipts](../../evidence/04A34/source-and-node-validation.json); no new unexplained functional nonpass was found in the exercised paths.

Strict/profile real exits1/1 retain the stale NormalPlot constructor review-decision and INTEGRITY_FAILURE diagnostics; they are not passes. Strict MkDocs exits0. New packet audits: {audit['status']}{(' — '+str(audit.get('passed'))+' PASS') if 'passed' in audit else ''}. Audit receipts are separate from the existing Python aggregate. Intermediate fixture/build/harness failures were corrected and retained locally; one initial wrapper propagated a failed child build incorrectly, was fixed, and all qualified final child build receipts are0. The Integer.MinValue data-row type mismatch was corrected without changing expected numerical outcomes.

Executed commands (working directory `<REPO_ROOT>`; `<PYTHON>` is the authorized portable interpreter):

```text
<PYTHON> -B evidence/04A34/entry.py
<PYTHON> -B evidence/04A34/run.py build Debug before
<PYTHON> -B evidence/04A34/run.py matrix Debug before
<PYTHON> -B evidence/04A34/freeze_slices.py
<PYTHON> -B evidence/04A34/freeze_layouts.py
<PYTHON> -B evidence/04A34/references.py
<VBC> /nologo /target:library /out:<ISOLATED_RUN>/<fixture>.dll /reference:<ENTRY_CORE_AND_HOST_AS_APPLICABLE> /reference:<FRAMEWORK_SYSTEM_CORE_AND_NETSTANDARD> evidence/04A34/<fixture>.vb
powershell -NoProfile -ExecutionPolicy Bypass -File evidence/04A34/capture.ps1
powershell -NoProfile -ExecutionPolicy Bypass -File evidence/04A34/capture-layouts.ps1
powershell -NoProfile -ExecutionPolicy Bypass -File evidence/04A34/capture-references.ps1
<PYTHON> -B evidence/04A34/run.py build Debug final
<PYTHON> -B evidence/04A34/run.py matrix Debug final
<PYTHON> -B evidence/04A34/run.py build Release final
<PYTHON> -B evidence/04A34/run.py managed Release final
<PYTHON> -B evidence/04A34/run.py focused Debug final
<PYTHON> -B evidence/04A34/run.py focused Release final
<PYTHON> -B evidence/04A34/run.py serial Debug final
<PYTHON> -B evidence/04A34/supplementary.py python
<PYTHON> -B evidence/04A34/supplementary.py versions
<PYTHON> -B evidence/04A34/supplementary.py cjs
dotnet <ISOLATED_RUN>/guard2-bin/SourceGuard.dll --history
<PYTHON> -B -m unittest discover -s evidence/04A34 -p test_*.py -v
<PYTHON> -B evidence/04A34/verify_manifest.py
git -c safe.directory=<REPO_ROOT> diff --check
git -c safe.directory=<REPO_ROOT> status --short
git -c safe.directory=<REPO_ROOT> diff --stat
```

The three frozen VB fixture compilations each exited0. The post capture loads final Debug host numeric wrappers, without a Chart instance. The Roslyn helper was built with `dotnet build evidence/04A34/helpers/SourceGuard.csproj --no-restore`, existing accepted assets and isolated intermediate/output paths, then exited0. Exact sanitized subprocess commands, versions, child exits and log hashes are in [command-outcomes.json](../../evidence/04A34/command-outcomes.json). Tools include SDK10.0.403,MSBuild18.11.1.51303,Python3.14.7,Node24.20.0 and Framework CLR4.0.30319.42000. Subprocess log decoding uses UTF8 or explicitly recorded Windows cp1252; fixture cultures were not changed to force matches.

The unchanged `powershell -NoProfile -ExecutionPolicy Bypass -File scripts/verify-beshstatng.ps1` and its `-Legacy` wrapper are **NOT_RUN** because they use shared historical output/bin/obj and Legacy Build packaging hooks. Accepted isolated equivalents executed complete managed lanes and source/Node/strict/MkDocs checks. Entry full Release is NOT_RUN: fresh entryDebug and accepted33 historicalRelease are distinct from the actually executed final fullRelease. No wrapper or missing run is inferred successful.

## Preservation, privacy and local packet

[Source preservation](../../evidence/04A34/source-preservation.json) compares all4074 tracked entry files; only12 authorized source/project changes differ. All prior04A.01–33 tracked dossiers/evidence/tests/engines and the external owner plan bytes remain unchanged. The original278-pin successor chain and100 mixed-model capture SHA checks are retained in [lineage](../../evidence/04A34/lineage-check.json) and [capture checks](../../evidence/04A34/mixed-model-captures-check.json). No old expected hash, failing test or native control was rewritten.

Historical metadata matches280953 files/138180795001 bytes and membership/size/mtime digest47e1430509183a035a67756061d424c2689567540489fb04349531d2cb1593c2. Their138GB payload bytes were **NOT_REHASHED**. Ignored original ledger/validation material is retained, not exhaustively re-byte-sealed here; original scientific/native obligations remain open. New generated runs stay isolated and excluded from the packet. No cleanup, archive relocation or post-phase retention work was performed.

The [manifest](../../evidence/04A34/publication-manifest.json) inventories every proposed external source/test/project/dossier/evidence artifact, with SHA256 and byte size and a sole nonrecursive null self-entry. It is a **local partial-review packet**, not an external publication. The independent verifier imports no producer; tamper tests cover missing/duplicate/case/path/hash/size/self entries and lineage rewrites. Raw logs, private original ledger paths and build/TestResults products are excluded. [Privacy checks](../../evidence/04A34/privacy-check.json) use the established publication scanner over final candidates. Public path redaction does not alter captured source SHA values.

P=NO; N1=NO GO; N2=NO; Gate C2=UNMET; Gate C3=UNAPPROVED. Zero native releases and zero original-control discharges. Live Excel/COM/UIA/RefEdit/charts, R/scientific oracle, browser/WASM/Mac/Online manual UI, installers, signing, packaging, deployment and networking are NOT_RUN. [Manual smoke proposal](../../evidence/04A34/native-smoke-proposal.md) includes Love,SPC,calibration/agreement,specialty,survival and existing WASM examples; all remain NOT_RUN until separately authorized.

Proposed source/test/project paths:

{files}

Owner checklist: review the partial code/captures and explicit gaps; require completion of the34-owned work and all OPEN_BLOCKER reconciliations; re-run affected lanes and independently verify the resealed manifest/privacy/preservation; only then consider acceptance. This packet must not be staged/committed/merged as a completed34 batch. The owner alone decides future Git publication. Nothing is staged or committed here, and35 remains unstarted.
'''
    for a,b in [('No04A.','No 04A.'),('all4074','all 4074'),('Before product edits,4074','Before product edits, 4074'),('only12','only 12'),('Only12','Only 12'),('There are105','There are 105'),('There are126','There are 126'),('There are129','There are 129'),('are44','are 44'),('SHA256','SHA-256'),('Core163/163,Common5/5,Framework214/214','Core 163/163, Common 5/5, Framework 214/214'),('Node:916','Node: 916'),('Python was830','Python was 830'),('Python:818','Python: 818'),('Metadata matches280953','Metadata matches 280953')]:text=text.replace(a,b)
    (ROOT/DOSSIER).write_text(text,encoding='utf-8',newline='\n')
def audit_receipt(label):
    folder=ROOT/('.test-rationalization-04A34.tmp/commands/'+label+'-packet-audits')
    raw=json.loads((folder/'command.json').read_bytes());log=(folder/'stderr.log').read_text(encoding='utf-8')
    total=int(re.search(r'Ran (\d+) tests?',log).group(1))
    assert raw['exit_code']==0 and re.search(r'\nOK\s*$',log)
    save('audit-results.json',dict(status='PASS',total=total,passed=total,failed=0,errors=0,skipped=0,exit_code=raw['exit_code'],stderr_sha256=__import__('hashlib').sha256((folder/'stderr.log').read_bytes()).hexdigest(),command_receipt=label+'-packet-audits',qualification='Actual executed independent packet and negative tests. Metadata/report refreshed and resealed afterward; independent byte verifier runs after final seal. Earlier UTF8 reader-only harness errors were corrected without weakening assertions.'))
def privacy():
    spec=importlib.util.spec_from_file_location('established_privacy13',ROOT/'scripts/verify-04a13-publication.py')
    m=importlib.util.module_from_spec(spec);spec.loader.exec_module(m)
    paths=sorted(p for p in expected_scope() if p not in (SELF,'evidence/04A34/privacy-check.json') and (ROOT/p).is_file())
    findings,reviewed=m.scan(paths)
    save('privacy-check.json',dict(status='PASS' if not findings else 'FAIL',scanner=pin('scripts/verify-04a13-publication.py'),checked_paths=paths,findings=findings,reviewed=reviewed,qualification='Established content/path/decoded-JSON/payload scanner; self receipt and nonrecursive manifest excluded from own scan. Synthetic fixtures only; raw logs/builds/private originals excluded.'))
    assert not findings,findings
def seal():
    paths=expected_scope()
    save('publication-manifest.json',dict(schema='04A34-publication-v1',decision='NOT_READY_04A34',files=[dict(path=p,bytes=None,sha256=None) if p==SELF else file_pin(p) for p in sorted(paths)],qualification='Local partial review packet only. No upload/publication/deployment. The sole self entry is nonrecursive.'))
if __name__=='__main__':
    import sys
    if sys.argv[1]=='dossier':dossier()
    elif sys.argv[1]=='privacy':privacy()
    elif sys.argv[1]=='seal':seal()
    elif sys.argv[1]=='audit-receipt':audit_receipt(sys.argv[2])
    else:raise ValueError(sys.argv[1])
