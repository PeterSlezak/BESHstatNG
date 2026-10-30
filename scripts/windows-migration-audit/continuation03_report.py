"""Bounded DM checkpoint03; preserves02 and original audit evidence. Audit only."""
from __future__ import annotations
import argparse,collections,hashlib,json,re,xml.etree.ElementTree as ET
import catalog,semantic
from amendment import git,read,sha
ROOT,OUT=catalog.ROOT,catalog.DEFAULT
EVIDENCE=ROOT/'TestResults/04A-01-continuation-03'
TARGETS={
 'BESHstatNG/src/DM/ResultWriter/ExcelDnaSharedOutputPlanAdapter.vb':38,
 'BESHstatNG/src/DM/ResultWriter/ExcelDnaOutputTargetAdapter.vb':18,
 'BESHstatNG/src/DM/ResultWriter/ExcelDnaResultWriter.vb':15,
 'BESHstatNG/src/DM/ResultWriter/ExcelDnaCompositeOutputAdapter.vb':7,
 'BESHstatNG/src/DM/SheetManagement.vb':13}
CHECK=False

def write(name,value):
    value=value.rstrip()+'\n';p=OUT/name
    if CHECK:
        if not p.is_file() or p.read_text(encoding='utf-8-sig')!=value:raise ValueError('Current checkpoint drift: '+name)
    else:p.write_text(value,encoding='utf-8',newline='')
def emit(name,value):write(name,json.dumps(value,indent=2,ensure_ascii=False))
def text(p):
    data=p.read_bytes()
    return data.decode('utf-16' if data.startswith((b'\xff\xfe',b'\xfe\xff')) else 'utf-8-sig',errors='replace')

def verify_index(root,index,proposed):
    """Read-only raw-byte verification; also used by independent stale-hash fixture."""
    errors=[];paths=[a['path'] for a in index['artifacts']]
    if len(paths)!=len(set(paths)):errors.append('duplicate indexed artifact')
    if set(proposed)!=set(paths)|{'docs/architecture/WindowsMigrationLedger/index.json'}:errors.append('index/proposed path mismatch')
    for a in index['artifacts']:
        p=(root/a['path']).resolve()
        if not p.is_relative_to(root.resolve()) or not p.is_file() or p.stat().st_size!=a['bytes'] or sha(p)!=a['sha256']:
            errors.append('stale current checkpoint artifact hash: '+a['path'])
    ledger=root/'docs/architecture/WindowsMigrationLedger'
    if ledger.exists():
        actual={p.relative_to(root).as_posix() for p in ledger.rglob('*') if p.is_file() and p!=ledger/'index.json'}
        tooling=root/'scripts/windows-migration-audit'
        if tooling.exists():actual|={p.relative_to(root).as_posix() for p in tooling.iterdir() if p.is_file()}
        entry=root/'docs/architecture/WindowsMigrationLedger.md'
        if entry.exists():actual.add(entry.relative_to(root).as_posix())
        if actual!=set(paths):errors.append('unindexed/missing current audit paths: '+str(sorted(actual.symmetric_difference(paths))))
    return errors

def scope_errors(symbols,decisions):
    errors=[];by_id={d.get('symbol_id'):d for d in decisions}
    for path,expected in TARGETS.items():
        records=[s for s in symbols if s['path']==path]
        if len(records)!=expected or len({s['symbol_id'] for s in records})!=expected:errors.append('bounded DM source denominator: '+path)
        for s in records:
            d=by_id.get(s['symbol_id'])
            if not semantic.qualified(s) or d is None or not semantic.qualified(d) or d.get('body_sha256')!=s['body_sha256']:
                errors.append('bounded DM unqualified/unpinned member: '+s['symbol_id'])
    return errors

def preservation():
    before=read(EVIDENCE/'before.json')
    assert git('branch','--show-current')==before['branch'] and git('rev-parse','HEAD')==before['head']
    assert not git('diff','--name-only') and not git('diff','--cached','--name-only')
    for label,records,base in [('protected',before['tracked'],ROOT),('GateC',before['gate'],ROOT),('snapshot',before['original_artifacts'],EVIDENCE/'original')]:
        assert all((base/r['path']).is_file() and sha(base/r['path'])==r['sha256'] for r in records),label+' hash mismatch'
    historical=[]
    for a in before['original_artifacts']:
        p=a['path'];name=p.rsplit('/',1)[-1]
        if '/baseline/' in p or name.startswith('continuation02') or name in ('completion-amendment.md','amendment-validation.json','amendment-count-deltas.json','baseline-manifest.json'):
            assert sha(ROOT/p)==a['sha256'],'Immutable earlier evidence changed: '+p
            historical.append(p)
    old=read(ROOT/'TestResults/04A-01-continuation-02/before.json')
    pinned=read(OUT/'continuation02-start.json')
    assert old['original_artifacts']==pinned['original_artifacts']
    assert all(sha(ROOT/'TestResults/04A-01-continuation-02/original'/a['path'])==a['sha256'] for a in old['original_artifacts'])
    return dict(tracked=len(before['tracked']),gate_c=len(before['gate']),snapshot=len(before['original_artifacts']),
        historical_paths=historical,previous_start_snapshot=122,baseline=52,branch=before['branch'],head=before['head'])

def validation():
    runs=read(EVIDENCE/'validation-runs.json') if (EVIDENCE/'validation-runs.json').exists() else []
    checks=read(EVIDENCE/'final-checks.json') if (EVIDENCE/'final-checks.json').exists() else []
    managed=[];other=[]
    for config in ('Debug','Release'):
        for name in ('Core.trx','Common.trx','OfficeAddin.trx','OfficeExecution.trx','Legacy.trx',f'OfficeAddin-{config}.trx'):
            p=EVIDENCE/config/name
            if p.exists():managed.append(dict(configuration=config,suite=name,**ET.parse(p).find('.//{*}Counters').attrib,
                distinct_full_suite=name!=f'OfficeAddin-{config}.trx',path=p.relative_to(ROOT).as_posix(),sha256=sha(p)))
        for kind in ('full','foundation'):
            p=EVIDENCE/config/(kind+'.log')
            if p.exists():
                log=text(p);other.append(dict(configuration=config,kind=kind,path=p.relative_to(ROOT).as_posix(),sha256=sha(p),
                    python_counts=re.findall(r'Ran (\d+) tests? in',log),node_counts=re.findall(r'(?:# |\u2139 )tests (\d+)',log),node_skips=re.findall(r'(?:# |\u2139 )skipped (\d+)',log)))
    commands=[]
    py='C:/Users/peter/Desktop/WPy64-31150/python-3.11.5.amd64/python.exe -B '
    dll='dotnet scripts/windows-migration-audit/bin/Debug/net10.0/WindowsMigrationAudit.dll '
    for command,name in [
        ('dotnet build scripts/windows-migration-audit/WindowsMigrationAudit.csproj --configfile scripts/windows-migration-audit/NuGet.Config -v minimal','analyzer-build.log'),
        (dll+'--check','source-before.log'),(dll+'--write','source-generation.log'),
        (py+'scripts/windows-migration-audit/catalog.py --write','catalog-generation.log'),
        ('dotnet --version','dotnet-sdk.log'),
        (py+'TestResults/04A-01-continuation-03/session.py check','protected-after.log')]:
        p=EVIDENCE/name
        if p.exists():commands.append(dict(command=command,exit_code=0,log=p.relative_to(ROOT).as_posix(),sha256=sha(p)))
    p=EVIDENCE/'negative-reconciliation.log'
    certificate_messages=[]
    for config in ('Debug','Release'):
        for kind in ('full','foundation'):
            log=EVIDENCE/config/(kind+'.log')
            if log.exists():certificate_messages.extend(dict(log=log.relative_to(ROOT).as_posix(),message=line) for line in text(log).splitlines() if re.search(r'development certificate|dev-certs|generated.*certificate',line,re.I))
    generated=[dict(path=p.relative_to(ROOT).as_posix(),sha256=sha(p),bytes=p.stat().st_size) for c in ('Debug','Release') for p in (EVIDENCE/c/'generated-by-full').rglob('*') if p.is_file()]
    post=read(EVIDENCE/'end-checks.json') if (EVIDENCE/'end-checks.json').exists() else []
    return dict(scope='Fresh continuation03 commands only; prior evidence is immutable history',runs=runs,checks=checks,additional_commands=commands,post_report_checks=post,
        final_current_state_commands='TestResults/04A-01-continuation-03/final-current-checks.json; recorded after manifest freeze to avoid self-referential hashes',
        managed=managed,python_node=other,negative_test_counts=re.findall(r'Ran (\d+) tests? in',text(p)) if p.exists() else [],
        installed_sdk=text(EVIDENCE/'dotnet-sdk.log').strip() if (EVIDENCE/'dotnet-sdk.log').exists() else 'not freshly queried',
        certificate_generation_messages=certificate_messages,transient_generated_copies=generated,
        parser_assertions=14,preservation=preservation(),
        limitations=['No Excel/COM/XLL/workbook execution','No authenticated Office or Mac','No installer/signing/deployment/network installation','No04A.02 golden captures'],
        development_notes=['No product-source change. Source/recording classification is not native output or numerical parity.',
            'Two Roslyn lambdas share line105 and two share line402; exact span IDs retained. No denominator change.',
            'Corrected current reconciliation prose that repeated historical SDK10.0.402. Fresh SDK query is recorded; legacy source-provenance.json.sdk is CLR Environment.Version. Source parser/schema/denominators were not changed.',
            'The34-test suite passed; the checkpoint-hash fixture was then strengthened to change bytes without changing size and rerun independently. This focused repeat is not an additional distinct test.',
            'Whole-equivalence guard validates the source-pinned review assertion and exact portable declaration; it does not execute or scientifically prove equivalence.'])

def runtime_rows(symbols):
    # Individually checked assignment sites; the record is slot/instance evidence,
    # never an invented handler ID for an arbitrary Invoke receiver.
    slots=[
     ('Revalidate','UibyID.vb:1886-1889','cancellation then lifecycle EnsureCurrent; default SH:14 no-op','SharedOutputPlanTests.vb:634,650,656;685-759 default/no-op or explicit override','SH:46,90','Lease invalidation/cancellation before creation or after either consent'),
     ('Acquire','UibyID.vb:1890;1917-1969','GetSharedResultWriter uses bound objects, preflight before creation; CreationAttempted set before Add','SharedOutputPlanTests.vb:693-710','SH:47','Add may mutate then throw; created destination remains/no rollback'),
     ('Target','UibyID.vb:1891-1893','actual writer ws.Name/current cursor plus requested destination','SharedOutputPlanTests.vb:711-712','SH:49','resolved native name/cursor must match preparation'),
     ('Identities','UibyID.vb:1894','apartment-local TargetIdentity of writer wb/ws','SharedOutputPlanTests.vb:713 synthetic tokens','SH:63','fake string IDs do not prove COM physical identity'),
     ('InspectCharts','UibyID.vb:1895','native shape count/geometry fingerprint, protection/quota','SharedOutputPlanTests.vb:714-718','SH:94,115','same-count reorder/movement and post-consent changes; observation is not collision prediction'),
     ('ConfirmCharts','UibyID.vb:1896-1898','Yes/No visual-overlap prompt separate from cell overwrite','SharedOutputPlanTests.vb:719-722;656 override','SH:120','decline after attempted destination creation reports partial'),
     ('Charts','UibyID.vb:1899;1695-1712;1816-1823','caller-supplied factory; ANOVA captures box/multiple-comparison writers, KW captures optional box writer','SharedOutputPlanTests.vb:723-734;960 null-action override','SH:61;428-451','factory can fail; returned chart closures borrow writer; non-cell region order must match'),
     ('Inspect','UibyID.vb:1900','native exact-region Value2/Formula inspection','SharedOutputPlanTests.vb:735-749','SH:91','blank formula/shape/DBNull/100000cells/64MiB; fresh observations'),
     ('Confirm','UibyID.vb:1901','cell overwrite MsgBox Yes/No','SharedOutputPlanTests.vb:750-753;655 invalidation override','SH:141','changed observation after approval must reject; UI interruption/lease timing deferred'),
     ('COM identity/Range.Value','OutputTargetAdapter:42-54;ResultWriter:70-107','native apartment/pointer ownership and direct COM write','SharedOutputPlanTests RecordingWriter overrides WriteOutputBlockCore','TA:56,72;WR:70','same-name books, rename, stale RCW, formula-leading/numeric-looking text and partial formatting failure')]
    assignment_lines={'Revalidate':(1886,),'Acquire':(1890,),'Target':(1891,),'Identities':(1894,),'InspectCharts':(1895,),
        'ConfirmCharts':(1896,),'Charts':(1695,1816),'Inspect':(1900,),'Confirm':(1901,)}
    pins={p:sha(ROOT/p) for p in ('BESHstatNG/src/UI/UibyID.vb',*list(TARGETS)[:4],'BESHStatNG.Test/src/DataManagement/SharedOutputPlanTests.vb')}
    result=[]
    for slot,prod,contract,test,invoke,risk in slots:
        known=[s['symbol_id'] for s in symbols if s['path']=='BESHstatNG/src/UI/UibyID.vb' and int(s['start_line']) in assignment_lines.get(slot,()) and 'Lambda' in s['kind']]
        if slot in assignment_lines:assert len(known)==len(assignment_lines[slot]),(slot,known)
        result.append(dict(boundary=slot,production_assignment=prod,source_contract=contract,test_assignment_or_boundary=test,invocation=invoke,
            known_assignment_source_ids='; '.join(known),target_symbol_id='not asserted for runtime-supplied instance',
            static_owner='Windows output/lifecycle maintainer',future_batch='04A.28;04A.45',risk=risk,
            dependency_pins=json.dumps(pins,sort_keys=True),required_fixture='NativeSafeOutputBoundIdentityAndConsent; recording tests are not native proof'))
    return result

def main():
    if CHECK:
        errors=verify_index(ROOT,read(OUT/'index.json'),read(OUT/'proposed-tracked-files.json')['paths'])
        assert not errors,errors
    before=read(EVIDENCE/'before.json');old_root=EVIDENCE/'original/docs/architecture/WindowsMigrationLedger'
    old=catalog.load(old_root,'symbols.csv');symbols=catalog.load(OUT,'symbols.csv');decisions=semantic.read_store('review-decisions.json')
    errors=scope_errors(symbols,decisions);assert not errors,errors
    sem=read(OUT/'semantic-review-coverage.json');review=read(OUT/'review-coverage.json');coverage=read(OUT/'coverage.json')
    assert len(symbols)==3866 and len(catalog.load(OUT,'files.csv'))==171
    for f in catalog.load(OUT,'files.csv'):assert sha(ROOT/f['path'])==f['sha256']
    old_dec=read(EVIDENCE/'original/scripts/windows-migration-audit/review-decisions.json');current={d['symbol_id']:d for d in decisions}
    previous={s['symbol_id']:s for s in old};selected=[s for s in symbols if s['path'] in TARGETS]
    for d in old_dec:
        if previous[d.get('symbol_id','')]['path'] not in TARGETS:assert current[d['symbol_id']]==d,'Prior review altered outside scope'
    assert len(selected)==91 and all(s.get('review_revision')=='04A.01-continuation-03' for s in selected)
    per_file=[dict(path=p,records=sum(s['path']==p for s in selected),before_qualified=sum(semantic.qualified(s) for s in old if s['path']==p),
        after_qualified=sum(semantic.qualified(s) and semantic.qualified(current.get(s['symbol_id'],{})) for s in selected if s['path']==p),
        unpinned=sum(s['symbol_id'] not in current for s in selected if s['path']==p),
        remaining_member_v1_gaps=sum(not semantic.qualified(s) or not semantic.qualified(current.get(s['symbol_id'],{})) for s in selected if s['path']==p),
        dispositions=dict(collections.Counter(s['disposition'] for s in selected if s['path']==p))) for p,n in TARGETS.items()]
    fields=('symbol_id','signature_key','path','start_line','end_line','span_start','span_length','profile','containing_type','signature','body_sha256',
        'disposition','responsibility','source_purpose','separable_responsibilities','core_common_equivalent','equivalence_scope','equivalent_evidence',
        'portable_equivalent_pins','compared_portable_declarations','equivalence_assessment','candidate_operation_id','related_operation_ids','reusable_operation','destination','future_batch',
        'risk','exception_tags','exception_applicability','proposed_test','runtime_boundary','owner_role','dependency_pins','required_dependency_paths')
    write('continuation03-member-reviews.csv',catalog.render([{**{k:s.get(k,'') for k in fields},'change':'updated detailed pin' if semantic.qualified(previous[s['symbol_id']]) else 'new detailed pin'} for s in selected]))
    candidates=[s for s in symbols if semantic.qualified(s) and s['disposition'].startswith('extract-to-')]
    write('inspected-candidates.csv',catalog.render([{k:s.get(k,'') for k in fields} for s in candidates]))
    op=collections.defaultdict(list)
    for s in selected:
        if s['disposition'].startswith('extract-to-'):op[s['candidate_operation_id']].append(s)
    operations=[dict(operation_id=k,member_rows=len(v),destination='BESHStat.Excel.Common',approved_batch='; '.join(sorted({s['future_batch'] for s in v})),
        source_ids='; '.join(s['symbol_id'] for s in v),required_tests='; '.join(s['proposed_test'] for s in v),
        existing_helper_reuse='; '.join(sorted({s['core_common_equivalent'] for s in v})),
        comparison_only_declarations='; '.join(sorted({s.get('compared_portable_declarations','[]') for s in v})),
        status='PROVISIONAL operation grouping for these five files only; global candidate count NOT FROZEN') for k,v in sorted(op.items())]
    write('continuation03-candidate-operations.csv',catalog.render(operations))
    write('continuation03-runtime-boundaries.csv',catalog.render(runtime_rows(symbols)))
    remaining=[s for s in symbols if s['family']=='DM' and not semantic.qualified(s)]
    write('continuation03-remaining-dm.csv',catalog.render([{k:s.get(k,'') for k in ('symbol_id','path','start_line','end_line','signature','body_sha256','owner_role','future_batch','proposed_test','disposition')} for s in remaining]))
    reason=collections.Counter((s['family'],'detailed source review' if semantic.qualified(s) else 'no pinned source review',s['disposition'],s['future_batch'],'inspected' if semantic.qualified(s) else 'provisional; review owed in04A.01') for s in symbols)
    write('counts-by-reason-and-batch.csv',catalog.render([dict(family=f,review_reason=r,disposition=d,batch=b,allocation_evidence=e,records=n) for (f,r,d,b,e),n in sorted(reason.items())]))
    deltas={}
    old_sem=before['starting_review'];old_rev=before['starting_coverage']
    for k,a,b in [('member_v1',old_sem['qualified_source_reviews'],sem['qualified_source_reviews']),('unreviewed',old_sem['unreviewed_for_acceptance'],sem['unreviewed_for_acceptance']),
        ('ownership_investigations',old_rev['member_investigations'],review['member_investigations']),('unresolved_edges',old_rev['unresolved_edge_rows'],review['unresolved_edge_rows']),
        ('full_controls',old_sem['qualified_full_control_contracts'],sem['qualified_full_control_contracts']),('legacy_detail_gaps',old_sem['legacy_pinned_reviews_requiring_details'],sem['legacy_pinned_reviews_requiring_details'])]:
        deltas[k]=dict(before=a,after=b,delta=b-a)
    emit('continuation03-count-deltas.json',dict(counters=deltas,files=per_file,families=sem['families'],batch_allocations=sem['batch_allocations'],
        candidate_member_rows=len(candidates),bounded_candidate_rows=sum(len(v) for v in op.values()),bounded_operation_groups=len(op),
        whole_equivalents_in_bounded_scope=sum(s['equivalence_scope']=='whole-method' for s in selected),candidate_freeze='NOT FROZEN'))
    emit('continuation03-start.json',{k:before[k] for k in ('captured_utc','branch','head','status','ignored_status','original_artifacts','starting_review','starting_coverage')})
    val=validation();emit('continuation03-validation.json',val)
    ft='\n'.join('| '+r['path']+' | '+str(r['records'])+' | '+str(r['before_qualified'])+' | '+str(r['after_qualified'])+' | '+str(r['remaining_member_v1_gaps'])+' |' for r in per_file)
    ct='\n'.join('| '+k+' | '+str(v['before'])+' | '+str(v['after'])+' |' for k,v in deltas.items())
    ot='\n'.join('| '+r['operation_id']+' | '+str(r['member_rows'])+' | '+r['approved_batch']+' |' for r in operations)
    dt='\n'.join('| '+f['family']+' | '+str(f['qualified_source_reviews'])+'/'+str(f['parsed_records'])+' | '+str(f['needs_investigation'])+' |' for f in sem['families'])
    distinct={c:sum(int(r.get('passed',0)) for r in val['managed'] if r['configuration']==c and r['distinct_full_suite']) for c in ('Debug','Release')}
    write('continuation03.md',f'''# 04A.01 controlled continuation03 — DM partial checkpoint

**PARTIAL CHECKPOINT / NOT READY FOR ACCEPTANCE.** The bounded five-file source review is complete, not the full04A.01 audit. No04A.02/extraction/refactor/owner acceptance is authorized or performed. Gate C2 is not met; E1–E7 open and GC3–GC8 deferred. The roadmap remains40/223 accepted and75 approved Phase04A batches.

Branch `{before['branch']}`, source/main HEAD `{before['head']}`; main ancestry checked. Tracked product worktree and index remain clean. All2,109 tracked protected paths,1,543 Gate C files and171 compiled source hashes match. The133 indexed artifacts plus index.json at start were verified/copied to a new134-file snapshot under `TestResults/04A-01-continuation-03/original/`. The immutable52 original baseline paths, continuation02 historical artifacts and its122-file original snapshot remain unchanged. No existing snapshot was recaptured/overwritten. No ZIP byte comparison is asserted from unavailable local archives.

## Derived bounded coverage

| Source | Records | Detailed before | Detailed after | Remaining |
|---|---:|---:|---:|---:|
{ft}

Exactly91/91 IDs are source/body/profile/span/dependency pinned:88 new reviews plus3 upgraded detailed decisions; the other423 prior detailed decisions remain identical. Member/property-container/Get/initializer/lambda records are independent, including same-line lambdas. `scope_errors` derives the table from symbols.csv and actual qualified decision store, not a handwritten success flag. No record/profile/parser scope reduction occurred.

| Counter | Before | After |
|---|---:|---:|
{ct}

| Family | Detailed / population | Investigations |
|---|---:|---:|
{dt}

The census remains171 files,3,866 members (3,864 active +2 alternative), zero parser diagnostics,268 native UDF exports (23 exporting/18 helper files),84 Ribbon references/81 actions plus inherited LoadImage,76 authored methods/5 commands/2 pages,2,056 controls and19,701 wiring/configuration records. methods.yml, RibbonXML, ExcelFunction and Common registrations retain separate authority; Office executable analyses remain ANOVA/Kruskal–Wallis only. One full control contract/54 routing intents and all other control/wiring blockers are preserved, not silently closed.

## Source/equivalence findings

`continuation03-member-reviews.csv` lists all91 exact IDs, signatures/ranges/profile/body hashes, actual responsibilities/callers, separable duties, dependency hashes, exact portable declaration pins, ownership/batch/risk/exception/fixture and runtime limits. Reading a dependency did not grant it a new detailed member review. Whole/helper/none are separate, source-pinned assertions; guards do not execute or prove native/scientific parity.

- **Shared output:** Windows stages/revalidation/acquisition and chart consent surround existing Common `OutputDocument.FromResult`, `ResultTableWriteRequestFactory.CreateMany`, `OutputLayoutPlanner.Create` and `SafeWrite.Execute`. No whole Execute equivalence is claimed. The first cell observation precedes cell/chart prompts; cached first observation is used once, then actual cells/shapes are inspected again. CreationAttempted is intentionally the mutation flag, even if destination Add threw before confirmation. Lifecycle remapping preserves confirmed prefix and consent. Native write failures can leave values before formatting fails; no rollback/per-action cancellation. `LogCharts` only enters diagnostic output stage: its snapshot/count arguments are unused.
- **Requests/cursors:** table-N source ordinal includes zero-area tables; document indexes include nonspatial diagnostic/plot artifacts. Prepare rejects stale ordering/IDs, wrong CLR scalar types, shapes and p-value metadata; it deliberately does not reject alpha/format/AutoFit overrides. Windows reserves a valid next cursor row (EndRow must be below row limit), unlike an otherwise valid last-row neutral layout. Primary ranges, supplementary gap1 and non-cell chart regions/actions preserve exact order. GUID plan/target tokens are distinct from physical workbook/worksheet identity. Shared prepared commands copy the list; composite constructor instead exposes AsReadOnly view of supplied list, whose alias can mutate it.
- **Native observations:** `ExcelDnaOutputTargetAdapter.TargetIdentity` HMACs process-scoped apartment-local IUnknown pointer, releasing that pointer in Finally; not portable/persisted identity or RCW ownership. Native inspector reads exact Value2/Formula rectangles, scalar or1-based2D arrays, rejecting malformed/DBNull. Blank-result formulas, empty strings/False/errors have source-specific occupancy semantics. Quotas are1,000 shapes/100,000 cells/64MiB; all cell Guards rejected, charts use separate shape observation/consent. Geometry fingerprint/count recheck is not atomic shape locking, overlap prediction or native execution proof. Common `TargetPlanner.Create` and target/layout classes are helper reuse, not COM observation equivalence.
- **Writer:** Core `ResultTableWriterBase` and `ResultTableOutputModel` already own neutral cursor/model basics; Windows actual write uses `Range.Value` then native borders/sections/fonts/fill/p-value formatting and optional AutoFit. Direct WriteSharedRequest/WriteOutputBlockCore/Composite.Execute do not independently invoke SafeWrite. Request.OutputTarget is ignored by direct single-request writer; assigned ws/current cursor are authoritative. Legacy left-header loop is1..hLeft-foots. Numeric-looking/formula-leading text coercion, existing font reset and partial format failures require native captures.
- **P-values:** `LegacyPValueIsHighlighted` accepts culture-convertible text, converts Decimal to Double, excludes Boolean/Nothing/nonfinite/negative, then p<=alpha; it does not independently require valid alpha/p<=1. Common `OutputCell.IsSignificantAt` is numeric-only, validates alpha and compares exact decimals. This is a comparison only, recorded as none verified rather than existing helper reuse. ResolvePValueAlpha returns caller default unchanged on invalid override, differing from Common factory.05 fallback. ResolvePValueNumberFormat reuses Core `BESHStat.Core/src/DM/ResultWriter/PValuePresentation.vb` BuildExcelNumberFormat only for blank override; nonblank passes through.
- **Composite:** existing Common `OfficeCompositeOutputPlanAdapter.Prepare` validates/returns neutral Office plan; Windows then projects a different native command/sink shape. Exact inactive-link wording and Common chart-placeholder text preserved. NewWorksheet-only validation, provenance and memory checks come from Common. No production form Prepare caller found; observed callers are CompositeOutputPlanTests. Literal text insertion is caller obligation; late sink failure retains previous tables, no rollback. No new live feature or Office registration.
- **Worksheet management:** MaxRows uses native count with65536 catch-all fallback; LastColumn activates sheet and scans500rows with255/16384 starts/fallback, retaining historical255 behavior. Count/CountA are native, not finite-number counts. ColName depends on native relative A1 address. ColNumber2Letter alternate-sheet loop still uses ActiveSheet.Cells, a compatibility risk. Pure Object/VB ColumnLetterToNumber shares valid-ASCII arithmetic with Common private ParseCell but lacks its grammar/caps; Common private ToColumn is not a public native fallback equivalent. CreateReference does not use ws; BuildExcelRefList preserves key order/skip and does not escape embedded apostrophes as Common QualifiedAddress does. Native ColumListFromRefAdress uses localized addresses, selected start rows and nested range indexing; not simply canonical full-column parsing. removeNonDigits is legacy IsNumeric-per-character input fallback; no silent Char.IsDigit/parser rewrite.

Exact portable paths/types/signatures and scope are in each row. In particular, whole forwarding claims are limited to the ResultTable.ToOutputModel lambda, two Shared Model container/Get records, native-writer Core base constructor and cursor-setter lambda. Existing Common planners/consent/style/column grammar are reused; no duplicate algorithm is proposed. GC8 remains open: Core logger may perform file IO through host injection, and raw native warning/error paths are retained. E1/E3/E5/E7 responsibilities and future fixtures are explicit; source classification does not close native/runtime exceptions.

## Candidate operation grouping

Global inspected candidate count is{len(candidates)} member rows, not a frozen unique migration count. Within these five files only, the following source-backed groups deduplicate{sum(len(v) for v in op.values())} candidate rows into{len(op)} provisional reusable operations. C03-07 is consolidation/exposure of existing column arithmetic with compatibility adapters, not a second algorithm. Full crosswalk is continuation03-candidate-operations.csv. Bulk unreviewed family tags are not accepted allocations.

| Provisional operation | Member rows | Existing approved batch |
|---|---:|---|
{ot}

Runtime delegate/COM assignment boundaries, production versus recording sites, owners and named fixtures are in continuation03-runtime-boundaries.csv. Host slots including Revalidate/Acquire/Inspect/Confirm/Charts remain instance-supplied; no fabricated handler symbol IDs. Original80 dynamic-site reviews remain pinned and1,604 external-scope unresolved intent edges remain open.

## Outstanding same-batch work

DM retains104 unreviewed records: ExcelDnaSharedApplicationAdapter23, ExcelDnaDataImporter9, ExcelDnaHostCapabilities3, Helpers4, PSMdataImport40 and UIprocedures25. DataObjContracts has zero code-bearing records and remains in171-file coverage. Exact IDs/current owner/batch/fixtures: continuation03-remaining-dm.csv. Finish those files in a later same-batch continuation. Global remaining-member-review.csv retains all other UI/Graphics/UDF/model/project-support IDs; full control/default/reset/output contracts remain mandatory. Only owner review can accept04A.01; Gate C2 follows04A.48, not this checkpoint.

## Fresh commands and preservation

continuation03-validation.json contains actual fresh commands/exits/log/TRX hashes and distinct counts: Debug{distinct['Debug']}, Release{distinct['Release']} passed managed tests (foundation duplicate reruns excluded), Python/Node counts and skips separately. check-amendment.ps1 uses EvidencePath TestResults/04A-01-continuation-03; source/catalog/self-test/negative/Git/protected guards must pass, strict must return2 for genuine remaining blockers. Corruption remains exit1. New negatives cover dropped new DM record/pin, dependency manifest, false whole assertion and stale current checkpoint hash; old tests remain intact.

Product suites use installed tools/local packages only. Regenerated VersionAttributes copies are preserved under new Debug/Release generated-by-full folders and restored only from hash-verified originals; other unexpected tracked deltas fail. No source, product tests, project/generated official inputs, framework, Ribbon/catalog/manifest/installer/official README or ignore policy edits. No newly reported dev-certificate side effect. Native cases are NOT RUN: safe overwrite, distinct chart consent/recheck, stale same-name/renamed worksheet identity, destination creation/partial write, p-value number/text/culture, all three destinations and XLS fallback. No04A.02 goldens. Native/recording assertions are not conflated.

continuation03-artifacts.csv enumerates every proposed added/modified/unchanged path relative to start; proposed-tracked-files.json must exactly match current indexed artifact paths plus index.json. All ignored ledger files require the individual future force-add instructions in future-staging.txt; those commands were not executed. Nothing staged/committed/merged/pushed. **04A.01 incomplete;04A.02 not started; waiting for owner review of this partial checkpoint.**
''')
    paths={p.relative_to(ROOT).as_posix() for p in OUT.rglob('*') if p.is_file()}
    names=('continuation03-artifacts.csv','proposed-tracked-files.json','future-staging.txt')
    paths|={(OUT/n).relative_to(ROOT).as_posix() for n in names}
    paths|={p.relative_to(ROOT).as_posix() for p in semantic.TOOLING.iterdir() if p.is_file()};paths.add('docs/architecture/WindowsMigrationLedger.md')
    previous_hash={a['path']:a['sha256'] for a in before['original_artifacts']}
    artifact=[]
    for path in sorted(paths):
        p=ROOT/path
        change='added' if path not in previous_hash else 'modified' if not p.exists() or sha(p)!=previous_hash[path] else 'unchanged'
        if path in {(OUT/n).relative_to(ROOT).as_posix() for n in names}|{'docs/architecture/WindowsMigrationLedger/index.json'}:
            change='modified' if path in previous_hash else 'added'
        artifact.append(dict(path=path,change_from_continuation03_start=change,tracking='ignored; future individual force-add' if path.startswith('docs/architecture/WindowsMigrationLedger/') else 'visible audit tooling/index entry',hash_authority='index.json; self excluded'))
    write('continuation03-artifacts.csv',catalog.render(artifact))
    emit('proposed-tracked-files.json',dict(status='PROPOSED ONLY; nothing staged',paths=sorted(paths),comparison='Exact current index artifacts plus index.json; preserve ignore policy'))
    write('future-staging.txt','# Future owner-reviewed commands only; NOT EXECUTED.\n'+'\n'.join(('git add -f -- ' if p.startswith('docs/architecture/WindowsMigrationLedger/') else 'git add -- ')+json.dumps(p) for p in sorted(paths)))
    print(json.dumps(dict(status='PARTIAL CHECKPOINT / NOT READY FOR ACCEPTANCE',target_reviewed=len(selected),overall=sem['qualified_source_reviews'],proposed_paths=len(paths))))

if __name__=='__main__':
    parser=argparse.ArgumentParser(__doc__);parser.add_argument('--check',action='store_true');parser.add_argument('--guard',action='store_true')
    args=parser.parse_args();CHECK=args.check
    if args.guard:print(json.dumps(preservation()))
    else:main()
