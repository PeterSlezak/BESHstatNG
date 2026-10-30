"""Basic statistical UDF export checkpoint08; historical reports stay immutable."""
from __future__ import annotations
import argparse,collections,hashlib,json,re
import catalog,semantic,continuation03_report as logs,continuation07_report as prior
from amendment import git,read,sha
ROOT,OUT=catalog.ROOT,catalog.DEFAULT
EVIDENCE=ROOT/'TestResults/04A-01-continuation-08'
BASE='BESHstatNG/src/ExcelUDFs/'
TARGETS={BASE+p:(g,n) for p,g,n in (
 ('DistributionUDFs.vb','A',3),('SampleSizeUDFs.vb','A',22),('SurvivalUDFs.vb','A',4),('UdfLinearAlgebra.vb','A',1),
 ('ParametricUDFs.vb','B',13),('NonparametricUDFs.vb','B',16),('ContingencyTableUDFs.vb','C',19),('AgreementMethodUDFs.vb','C',16))}
EXPECTED_EXPORTS=dict(zip(TARGETS,(3,9,4,0,10,14,15,16)))
REG_FIELDS=('export_id','path','attribute_line','attribute_end_line','attribute','effective_name','symbol_id','implementing_name','signature','start_line','end_line')
FIELDS=prior.FIELDS+('registration_evidence',);CHECK=False
def write(name,value):
    value=value.rstrip()+'\n';p=OUT/name
    if CHECK:
        if not p.is_file() or p.read_text(encoding='utf-8-sig')!=value:raise ValueError('Checkpoint08 drift: '+name)
    else:p.write_text(value,encoding='utf-8',newline='')
def emit(name,value):write(name,json.dumps(value,indent=2,ensure_ascii=False))
def historical_errors(root,records):
    errors=prior.historical_errors(root,records)
    for a in records:
        if a['path'].rsplit('/',1)[-1].startswith('continuation07'):
            p=root/a['path']
            if not p.is_file() or sha(p)!=a['sha256']:errors.append('Historical07 evidence changed: '+a['path'])
    return errors
def raw_history_errors(root,records):
    errors=[]
    for a in records:
        p=root/a['path']
        if not p.is_file() or p.stat().st_size!=a['bytes']:errors.append('Prior raw evidence missing/size changed: '+a['path']);continue
        h=hashlib.sha256()
        with p.open('rb') as f:
            for block in iter(lambda:f.read(1024*1024),b''):h.update(block)
        if h.hexdigest()!=a['sha256']:errors.append('Prior raw evidence hash changed: '+a['path'])
    return errors
def preservation():
    b=read(EVIDENCE/'before.json')
    assert git('branch','--show-current')==b['branch'] and git('rev-parse','HEAD')==b['head']
    assert git('rev-parse','main')==b['head'] and not git('diff','--name-only') and not git('diff','--cached','--name-only')
    for label,records,base in [('protected',b['tracked'],ROOT),('GateC',b['gate'],ROOT),('snapshot08',b['original_artifacts'],EVIDENCE/'original')]:
        assert all((base/a['path']).is_file() and sha(base/a['path'])==a['sha256'] for a in records),label+' mismatch'
    errors=historical_errors(ROOT,b['original_artifacts']);assert not errors,errors
    assert all(sha(ROOT/a['retained_path'])==a['sha256'] for a in b['baseline']['entries'])
    files=catalog.load(OUT,'files.csv');assert len(files)==171 and all(sha(ROOT/f['path'])==f['sha256'] for f in files)
    raw=read(EVIDENCE/'prior-raw-evidence-hashes.json')['files'];errors=raw_history_errors(ROOT,raw);assert not errors,errors
    return dict(branch=b['branch'],head=b['head'],tracked=len(b['tracked']),gate_c=len(b['gate']),compiled=len(files),snapshot=len(b['original_artifacts']),baseline=len(b['baseline']['entries']),prior_raw_files=len(raw),historical='02/03/04/05/06/07 reports, snapshots and all recorded raw files unchanged')
def scope_errors(symbols,decisions,queue):
    source={s['symbol_id']:s for s in symbols};pins={d.get('symbol_id'):d for d in decisions};errors=[]
    exports=catalog.load(OUT,'udf-exports.csv')
    reviewed=[d for d in decisions if d.get('checkpoint_id')=='04A.01-continuation-08']
    if len(reviewed)!=94 or len({d['symbol_id'] for d in reviewed})!=94:errors.append('checkpoint08 missing/duplicate or out-of-scope decision count')
    for d in reviewed:
        if json.loads(d.get('source_identity','{}')).get('path') not in TARGETS:errors.append('checkpoint08 outside scope ID: '+d['symbol_id'])
    for p,(group,count) in TARGETS.items():
        ids={s['symbol_id'] for s in queue if s['path']==p}
        if len(ids)!=count or sum(s['path']==p for s in symbols)!=count:errors.append('checkpoint08 source/queue denominator: '+p)
        for ident in ids:
            s=source.get(ident);d=pins.get(ident)
            if s is None or d is None or not semantic.qualified(s) or not semantic.qualified(d):errors.append('checkpoint08 missing/unqualified ID: '+ident);continue
            if d.get('checkpoint_id')!='04A.01-continuation-08' or d.get('review_group')!=group:errors.append('checkpoint08 incorrect group: '+ident)
            if d.get('body_sha256')!=s['body_sha256'] or d.get('signature_key')!=s['signature_key']:errors.append('checkpoint08 top-level source pin drift: '+ident)
            if d.get('source_kind')!=s['kind'] or d.get('source_enclosing_symbol_id')!=s.get('enclosing_symbol_id',''):errors.append('checkpoint08 kind/enclosing drift: '+ident)
            recorded=json.loads(d.get('registration_evidence','[]'))
            project=lambda rows:[{k:r.get(k,'') for k in REG_FIELDS} for r in rows]
            if project(recorded)!=project([e for e in exports if e['symbol_id']==ident]):errors.append('checkpoint08 member annotation evidence drift: '+ident)
            errors+=semantic.verify_member_contract(d,s)+semantic.verify_pins([d],'checkpoint08')
    return errors
def export_rows(selected,exports):
    values=prior.export_rows(selected,exports)
    for value in values:
        records=[{k:e[k] for k in REG_FIELDS} for e in exports if e['symbol_id']==value['symbol_id']]
        value['registration_source_evidence']=json.dumps(records,sort_keys=True)
        value['attribute_evidence_status']='Actual parsed ExcelFunction annotations; public visibility alone is not registration'
    return values
def registration_errors(exports,original):
    project=lambda rows:sorted(({k:r.get(k,'') for k in REG_FIELDS} for r in rows),key=lambda r:r['export_id'])
    errors=[]
    if len(exports)!=268 or len({e['export_id'] for e in exports})!=268:errors.append('Global export count/duplicate drift')
    if project(exports)!=project(original):errors.append('Export annotation/name/signature/source identity changed')
    for path,count in EXPECTED_EXPORTS.items():
        if sum(e['path']==path for e in exports)!=count:errors.append('Per-file registration denominator drift: '+path)
    return errors
def export_errors(selected,exports,rows):
    errors=[] if export_rows(selected,exports)==rows else ['checkpoint08 export/helper mapping drift']
    if sum(e['symbol_id'] in {s['symbol_id'] for s in selected} for e in exports)!=71:errors.append('checkpoint08 requires actual71 exported target IDs')
    if len(rows)!=94 or len({r['symbol_id'] for r in rows})!=94:errors.append('checkpoint08 export/helper member omission or duplicate')
    return errors
def prior_decision_errors(current_text,original_text):
    def objects(text):
        decoder=json.JSONDecoder();pos=text.index('[')+1;out={}
        while True:
            while pos<len(text) and (text[pos].isspace() or text[pos]==','):pos+=1
            if text[pos]==']':return out
            start=pos;value,pos=decoder.raw_decode(text,pos);key=value.get('symbol_id') or value['signature_key']
            if key in out:raise ValueError('Duplicate ownership decision: '+key)
            out[key]=text[start:pos].encode('utf-8')
    now,old=objects(current_text),objects(original_text)
    return ['Prior decision bytes altered: '+key for key,value in old.items() if now.get(key)!=value]
def operation_rows(selected):
    values=prior.operation_rows(selected)
    for v in values:v['status']='NEW PROVISIONAL GROUP' if v['operation_id'].startswith('C08-') else 'EXTENSION OF EXISTING GROUP; NOT A NEW OPERATION'
    return values
def operation_errors(selected,operations):
    errors=[] if operation_rows(selected)==operations else ['checkpoint08 operation mapping drift']
    existing={r['operation_id'] for n in ('03','04','05','06','07') for r in catalog.load(OUT,'continuation'+n+'-candidate-operations.csv')}
    for o in operations:
        if not o['operation_id'].startswith('C08-') and o['operation_id'] not in existing:errors.append('Unknown claimed prior operation: '+o['operation_id'])
    return errors
def alpha_errors(decisions,records):
    errors=[];current={d.get('symbol_id'):d for d in decisions}
    if len(records)!=6 or len({r['symbol_id'] for r in records})!=6:errors.append('Missing/duplicate alpha-boundary review record')
    for r in records:
        d=current.get(r['symbol_id'])
        if d!=r['after'] or r['source_observation'] not in d.get('source_purpose',''):errors.append('Alpha-boundary decision/evidence drift: '+r['symbol_id']);continue
        comparison=json.loads(d['compared_portable_declarations'])
        if r['guard'] not in comparison:errors.append('Alpha-boundary guard comparison omitted: '+r['symbol_id'])
        errors+=semantic.verify_pins([d],'C08 alpha boundaries')
    return errors
def validation():
    old_e,old_p=logs.EVIDENCE,logs.preservation
    try:logs.EVIDENCE=EVIDENCE;logs.preservation=preservation;value=logs.validation()
    finally:logs.EVIDENCE,logs.preservation=old_e,old_p
    value['scope']='Fresh continuation08 runs only; full corrected negative suite, no native/numerical golden execution'
    value['development_notes']=[
      'Initial source viewer encountered Windows cp1252 encoding on a Unicode annotation; set UTF-8 and reread every affected body. No decision was inferred from truncated output.',
      'C author script had two unescaped apostrophes; syntax corrected before any C store write; exact overloaded Core assessment methods pinned by declaration line.',
      'Baseline candidate register is373 rows, not stale299 in instruction text: Continuation07 added74; original artifacts retained.',
      'All94 bodies individually reviewed;958 earlier decision objects also checked as exact raw UTF-8 object bytes. No family heuristic/whole-wrapper equivalence.',
      'After immutable A/B/C snapshots,39 table-producing records received additional individually selected conditional display/settings dependency pins; raw display-dependency-strengthening.json and progress-final.json retain before/after decisions. No old958 meaning or byte changed.',
      'Final transitive alpha guard reread added six explicit boundary comparisons after the first78-test full pass; original successful evidence retained and whole suite rerun after final edits, including a new alpha-evidence drift case. Product source is identical, so product suites were not needlessly repeated.',
      'Full Debug/Release and foundation runs are fresh; foundation repeats excluded. Generated transients retained and protected originals restored; certificate messages are explicit.',
      'Complete corrected negative suite run after all audit edits; initial Continuation07 failure/recheck remains immutable history, not fresh08 evidence.']
    for c in value['additional_commands']:c['command']=c['command'].replace('continuation-03','continuation-08')
    initial=EVIDENCE/'source-before-original-review-check.log'
    if initial.exists():value['additional_commands'].append(dict(command='dotnet scripts/windows-migration-audit/bin/Debug/net10.0/WindowsMigrationAudit.dll --check --reviews=TestResults/04A-01-continuation-08/original/scripts/windows-migration-audit/review-decisions.json',exit_code=0,log=initial.relative_to(ROOT).as_posix(),sha256=sha(initial),purpose='Fresh unchanged source/census check against original958 decisions before semantic regeneration'))
    value['final_current_state_commands']='TestResults/04A-01-continuation-08/final-current-checks.json; raw post-manifest commands avoid self-reference'
    log=EVIDENCE/'negative-reconciliation.log'
    if log.exists():
        contents=logs.text(log);names=set(re.findall(r'(?:^|\n|python\.exe : )(test_\w+)',contents));counts=re.findall(r'Ran (\d+) tests? in',contents)
        assert counts and len(names)==int(counts[-1]) and re.search(r'\nOK\s*$',contents),'Full negative suite must complete successfully'
        value['distinct_negative_tests']=len(names)
    refined=EVIDENCE/'negative-reconciliation-final.log'
    if refined.exists():
        initial=logs.text(log);final=logs.text(refined)
        names=lambda text:set(re.findall(r'(?:^|\n|python\.exe : )(test_\w+)',text))
        first,last=names(initial),names(final);assert len(first)==78 and len(last)==79 and first<=last and re.search(r'\nOK\s*$',final)
        value['negative_test_counts']=['79'];value['distinct_negative_tests']=len(first|last)
        value['negative_runs']=[dict(path=log.relative_to(ROOT).as_posix(),sha256=sha(log),tests=78,passed=78,failed=0,stage='first full suite before final alpha-boundary refinement'),dict(path=refined.relative_to(ROOT).as_posix(),sha256=sha(refined),tests=79,passed=79,failed=0,stage='entire corrected suite after all final edits')]
        value['negative_repeated_identities']=len(first&last)
        value['refinement_commands']=read(EVIDENCE/'refinement-commands.json')
    return value
def main():
    b=read(EVIDENCE/'before.json');oldroot=EVIDENCE/'original/docs/architecture/WindowsMigrationLedger'
    if CHECK:
        errors=logs.verify_index(ROOT,read(OUT/'index.json'),read(OUT/'proposed-tracked-files.json')['paths']);assert not errors,errors
    old=catalog.load(oldroot,'symbols.csv');symbols=catalog.load(OUT,'symbols.csv');decisions=semantic.read_store('review-decisions.json')
    queue=[s for s in catalog.load(oldroot,'remaining-member-review.csv') if s['path'] in TARGETS]
    errors=scope_errors(symbols,decisions,queue);assert not errors,errors
    ids={s['symbol_id'] for s in queue};selected=sorted((s for s in symbols if s['symbol_id'] in ids),key=lambda s:(s['path'],int(s['span_start'])))
    assert len(selected)==len(ids)==94
    current={d['symbol_id']:d for d in decisions}
    for d in read(EVIDENCE/'original/scripts/windows-migration-audit/review-decisions.json'):assert current.get(d['symbol_id'])==d,'Prior review altered'
    errors=prior_decision_errors((semantic.TOOLING/'review-decisions.json').read_text(encoding='utf-8-sig'),(EVIDENCE/'original/scripts/windows-migration-audit/review-decisions.json').read_text(encoding='utf-8-sig'));assert not errors,errors
    alpha=read(EVIDENCE/'alpha-boundary-refinement.json')['records'];errors=alpha_errors(decisions,alpha);assert not errors,errors
    write('continuation08-alpha-boundaries.csv',catalog.render([{k:(json.dumps(r[k],sort_keys=True) if k=='guard' else r[k]) for k in ('symbol_id','path','name','start_line','end_line','body_sha256','guard','source_observation','owner_role','future_batch','proposed_test')} for r in alpha]))
    assert len(symbols)==3866 and len(catalog.load(OUT,'files.csv'))==171
    errors=semantic.verify(symbols,catalog.load(OUT,'ui-wiring.csv'),catalog.load(OUT,'ui-controls.csv'));assert not errors,errors
    sem=read(OUT/'semantic-review-coverage.json');review=read(OUT/'review-coverage.json')
    per_file=[dict(path=p,group=g,records=sum(s['path']==p for s in symbols),before_qualified=sum(semantic.qualified(s) for s in old if s['path']==p),after_qualified=sum(semantic.qualified(s) for s in symbols if s['path']==p),remaining=sum(not semantic.qualified(s) for s in symbols if s['path']==p),dispositions=dict(collections.Counter(s['disposition'] for s in selected if s['path']==p))) for p,(g,n) in TARGETS.items()]
    # Merge only extra audit fields; source CSV remains the source identity authority.
    rich=[{**s,**{k:v for k,v in current[s['symbol_id']].items() if k not in s}} for s in selected]
    write('continuation08-member-reviews.csv',catalog.render([{k:s.get(k,'') for k in FIELDS} for s in rich]))
    candidates=[s for s in symbols if semantic.qualified(s) and s['disposition'].startswith('extract-to-')]
    write('inspected-candidates.csv',catalog.render([{k:s.get(k,'') for k in FIELDS} for s in candidates]))
    operations=operation_rows(selected);assert not operation_errors(selected,operations)
    write('continuation08-candidate-operations.csv',catalog.render(operations))
    exports=catalog.load(OUT,'udf-exports.csv');old_exports=catalog.load(oldroot,'udf-exports.csv')
    assert len(exports)==len(old_exports)==268 and {e['export_id'] for e in exports}=={e['export_id'] for e in old_exports}
    errors=registration_errors(exports,old_exports);assert not errors,errors
    mapped=export_rows(selected,exports);assert not export_errors(selected,exports,mapped)
    write('continuation08-export-helper-map.csv',catalog.render(mapped))
    export_files=[dict(path=p,source_records=n,exports_before=sum(e['path']==p for e in old_exports),exports_after=sum(e['path']==p for e in exports),reviewed_helpers=sum(r['path']==p and r['excel_function_export']=='no' for r in mapped)) for p,(g,n) in TARGETS.items()]
    write('continuation08-export-counts.csv',catalog.render(export_files+[dict(path='ALL WINDOWS EXPORTS',source_records=3866,exports_before=len(old_exports),exports_after=len(exports),reviewed_helpers=sum(r['excel_function_export']=='no' for r in mapped))]))
    reasons=collections.Counter((s['family'],'detailed source review' if semantic.qualified(s) else 'no pinned source review',s['disposition'],s['future_batch'],'inspected' if semantic.qualified(s) else 'provisional; review owed in04A.01') for s in symbols)
    write('counts-by-reason-and-batch.csv',catalog.render([dict(family=f,review_reason=r,disposition=d,batch=x,allocation_evidence=e,records=n) for (f,r,d,x,e),n in sorted(reasons.items())]))
    deltas={}
    for key,a,z in [('member_v1',b['starting_review']['qualified_source_reviews'],sem['qualified_source_reviews']),('unreviewed',b['starting_review']['unreviewed_for_acceptance'],sem['unreviewed_for_acceptance']),('unpinned',b['starting_coverage']['unreviewed_static_records'],review['unreviewed_static_records']),('ownership_investigations',b['starting_coverage']['member_investigations'],review['member_investigations']),('unresolved_edges',b['starting_coverage']['unresolved_edge_rows'],review['unresolved_edge_rows']),('full_controls',b['starting_review']['qualified_full_control_contracts'],sem['qualified_full_control_contracts']),('legacy_detail_gaps',b['starting_review']['legacy_pinned_reviews_requiring_details'],sem['legacy_pinned_reviews_requiring_details']),('files_without_qualified_review',len(b['starting_review']['files_without_qualified_review']),len(sem['files_without_qualified_review']))]:deltas[key]=dict(before=a,after=z,delta=z-a)
    old_candidates=sum(semantic.qualified(s) and s['disposition'].startswith('extract-to-') for s in old);old_families={f['family']:f for f in b['starting_review']['families']}
    emit('continuation08-count-deltas.json',dict(counters=deltas,files=per_file,families=[dict(before=old_families[f['family']],after=f) for f in sem['families']],batch_allocations=sem['batch_allocations'],candidate_member_rows=dict(before=old_candidates,after=len(candidates),delta=len(candidates)-old_candidates),new_candidate_rows=sum(int(o['member_rows']) for o in operations),new_operation_groups=sum(o['operation_id'].startswith('C08-') for o in operations),existing_operation_extensions=[o['operation_id'] for o in operations if not o['operation_id'].startswith('C08-')],candidate_freeze='NOT FROZEN',export_counts=export_files,initially_noninvestigation_new_reviews=[{k:s[k] for k in ('symbol_id','path','name','disposition')} for s in old if s['symbol_id'] in ids and s['disposition']!='needs-investigation']))
    emit('continuation08-start.json',{k:b[k] for k in ('captured_utc','branch','head','status','ignored_status','original_artifacts','starting_review','starting_coverage')})
    val=validation();emit('continuation08-validation.json',val)
    ft='\n'.join('| '+f['group']+' | '+f['path']+' | '+str(f['before_qualified'])+' | '+str(f['after_qualified'])+'/'+str(f['records'])+' |' for f in per_file)
    ct='\n'.join('| '+k+' | '+str(v['before'])+' | '+str(v['after'])+' |' for k,v in deltas.items())
    ot='\n'.join('| '+o['operation_id']+' | '+str(o['member_rows'])+' | '+o['approved_batch']+' | '+o['status']+' |' for o in operations)
    distinct={c:sum(int(r.get('passed',0)) for r in val['managed'] if r['configuration']==c and r['distinct_full_suite']) for c in ('Debug','Release')}
    write('continuation08.md',f'''# 04A.01 Continuation08 — basic statistical UDF exports

**CONTINUATION 08 COMPLETE — PARTIAL CHECKPOINT / NOT READY FOR ACCEPTANCE.**94 individually source-verified records in8 files, not whole04A.01 completion. No04A.02, later extraction batch implementation, product/native/golden changes or Gate C2 closure. E1–E7 open/GC3–GC8 deferred,40/223 accepted and75 Phase04A batches unchanged.

Branch `{b['branch']}`, HEAD/main `{b['head']}`. No tracked/index changes or staging. Start183 indexed artifacts plus index preserved as184 paths under new TestResults/04A-01-continuation-08/original; original indexSHA6641112032955fbb7bd465e3451b295d48514306812b85a6e1b0dcb3bb513f17. Guards preserve2109 tracked-protected,1543 Gate C,171 compiled-source hashes,52 original baseline paths and17571 prior02–07 raw evidence files. Original958 decisions retain exact object bytes; no earlier checkpoint renderer run. No local ZIP available for independent archive inclusion/diff verification. The source/census check with original958 review snapshot is separately logged, not mislabeled as a current semantic check.

| Group | Source | Before | After |
|---|---|---:|---:|
{ft}

A30/B29/C35 immutable rich snapshots contain exact IDs, profiles, source spans/kinds/enclosing IDs, body/dependency/portable declaration pins, actual responsibilities/options/error paths, owners, approved batches and named future fixtures. Final39 conditional p-display/settings dependency additions are separately recorded with before/after decisions in display-dependency-strengthening.json and progress-final.json; original group evidence stays unchanged. Dependency reads confer no outside review. Census3866(3864active+2alternative),171files,zero diagnostics remains unchanged.

| Counter | Before | After |
|---|---:|---:|
{ct}

Overall1052/3866 and UDF309/703;2814 pending =UI1372+Graphics1048+UDF394. Remaining IDs/reasons/provisional owners/batches stay in remaining-member-review.csv.1/2056 full controls,1604 unresolved wiring/intent edges and0 legacy-detail gaps remain. No control/routing promotion from exports. Four authorities unchanged:268 ExcelFunction exports,84Ribbon buttons/81actions plus inheritedLoadImage, authored76methods/5commands/2pages, Common runnable registry ANOVA/Kruskal–Wallis only. Export count is not Office capability.

## Export and source traceability

Exactly71 target registrations plus23 non-export member records. A16exports(3distribution+9planning+4survival)/14helpers; B24exports/5helpers; C31exports/4helpers. UdfLinearAlgebra is non-export. Every member maps actual ExcelFunction attribute text/range, Name, implementing name, signature and source ID (or explicit no annotation) in continuation08-export-helper-map.csv; source registration fields also pinned in each decision. Per-file/global before/after268 totals are in continuation08-export-counts.csv. Public/Friend visibility is not registration, IsThreadSafe annotation is not runtime certification. Names/signatures/attributes and all global export IDs remain unchanged.

## A — distribution, planning and survival

PRTRNG checks NaN/domain but not all infinite arguments; Core Distributions.PRTRNG rejects df<1 through iFault, despite wrapper v>0 admission. q0 permitted withvaliddf. Upper tail is1-CDF withfiniteclipping, not separate quadrature. F_PDF x0 explicitlyreturns0(df1>2),1(df1=2),Num(df1<2); existing Core rawformula differs atzero. Exact Core declarations in BaseStat/Distributions.vb are helper-only reuse, no new algorithm.

Sample-size request policies distinguish strict typed-value rejectionValue from domainNum and logged exceptionsValue. Beta passed as typeII error, not power. Superiority/NI/EQ aliases and positive margin ->negative/symmetric limits are source-pinned separately, including3 literal initializer IDs. Core StatTests/SampleSize.vb SampleSizeCalculator already owns all9 planning kernels and NI/EQ components; numerical forwards are partial helpers because UDF validation/hypothesis/spills remain mixed. Superiority group counts floor allocation after rounded experimental N; NI/EQ proportion paths ceil, swap upper-bound groups/ratio and retain driving-bound policy. Endpoints accepted by outer proportion checks can be rejected by Core margin helpers. ICC exact-integer raters have CInt overflow boundary; ICC/BA planning bracket/bisect limits canthrow. Cohen-like/clinical interpretations are not inferred from names.

Projection records individually pin2x2/3x2/4x4/3x3/7x5/8x2/6x2/7x2 bodies, exact row/header strings and Integer/Double/text cells. Cox missing event proportion uses NaN and text Not supplied/Not estimated; no workbook writing/layout/format equivalence follows from a spill. Common ResultTableWriteRequest is a placement/presentation contract, not identical planning spill. .41 owns request/result policy, .28 projection; .38 remains future registration/extraction work. GC8 is tagged for actual logger/error paths, not literal constants/projectors.

LOGRANK_P/STAT call C07 import+Core engine, mapFalse toNum; neither has localcatch. STAT relies on helper nonnull promise; P guardsnonnull/nonfinite. MEDIAN_CI catches/logsValue while invalidalpha/helperFalse mapNum. KM_TABLE flattens existing Core Survival_KM_LR.SurvivalCurveTabularOutput grouped/risk-sorted/tied-time records into noheaderNx7 without converting nonfinite values toNA, unlike median helper. Legacy VisualStyles/TaskbarClock imports are unused by these bodies; no WinForms owner is inferred. .42/.41 future helper/request crosswalks do not authorize runtime execution.

Non-export TryInvertMatrix guardssquare/nonempty, calls Windows Matrix.MatInv(SVD,False), already a complete numeric forward to Core LinearAlgebra/MatrixDecompositionCore.InvertMatrix. Core clones matrix/strict-rank-checks SVD and canlogthrow; Try facade suppresses exceptions/clearsoutput and checks returnedshape/errorcode, not finite entries. Compatibility facade retained; no duplicate inversion proposed.

## B — parametric/nonparametric

Core StatTests/Parametric.vb owns OneWayANOVA, Welch, RM/corrections, nested models and t calculations. Common OneWayAnovaApplicationService.Run and KruskalWallisApplicationService.Run share Core typed workflows but do not reproduce legacy Excel header/error/scalar/spill contracts wholesale. ANOVA names/independent filtering and RM complete-row pairing remain distinct. WelshANOVA.DF1 stores denominator df; Welch UDF constructs its own numerator/denominator table. RM correction aliases drive variable-width table0; nested output selects classical/Satterthwaite or stacks both. MCP selection invokes existing Core Fisher/Bonferroni/Tukey/Games/RM2/sphericity routines; fixed wrap order and CI footnotes preserved. No algorithms re-extracted.

Unpaired t returns pooled/Welch orboth; paired uses joint valid rows and fixed engine default. NI/EQ locals initializealpha.025 but C07 TryParseAlpha resets omitted to.05, so documented .025 differs from execution. Core equivalent-CIalpha becomes2*actual alpha. Proportion NI/EQ has same issue. A final dependency reread additionally verifies Core NumericGuards.ValidateAlphaOneSided requiresalpha<0.5: values[0.5,1) parse in the four inference wrappers but later throw/logValue, while parser-rejected bounds mapNum. T-planning NI/EQ has the same half-alpha guard via OneSidedAlphaToTwoSidedEquivalent; proportion planning instead uses its open(0,1) margin helper without that half cutoff. Six exact IDs/guard body hashes/fixtures are crosswalked in continuation08-alpha-boundaries.csv, with immutable refinement before/after evidence. These are parity fixtures, not bug fixes. Core mean/proportion equivalence canthrow for observed difference outside margins rather than return unsupportedFalse; wrappers maptoValue. Direction experimental-control versus ordinary p1-p2/x-y is explicit per ID. ResolveNames explicit malformed list can fall back X1.. instead of inferred labels; StackWithBlankRow returns aliases whenonearraynull and otherwise Nothing separator/padding, later Object projection canmakeemptyString. Variance/option grammars preserved with exact fallbacks/throws.

Core StatTests/Nonparametric.vb owns ranks/ties/normal/exact/MCP. MW normal export still triggers Core exact work for total<=50; exact export imposes50 limit, uses minU tails with lower<=observed/upper>observed and clips finite probabilities. Wilcoxon removeszeros, uses up to60 NONZERO differences; allzero Core earlyreturn hasP1 but exact-availability flag notset, so exact UDFNum versus normal1. Spearman/Kendall run Core exact/permutation sidework atsmalln but P exports return asymptotic Pvalue, notPvalueExact. Kendall coefficient min3 versusp min4; rawcoefficient/pfinite handling differs between exports. KW returnsH/Hcor orclippedp; Friedman unknown selector silentlyT1, statraw versuspfinitecheck, T2 sentinel1e30 remains Core. KW/Friedman MCP validatesalpha although noCI and adjustedp formulas do not consumealpha. Dunn/Conover tableindices/order and blankrows/pvalue-displaystrings remain exact. Scalar p staysnumeric; metadata-marked spill p canbetext through runtime PValuePresentation. Catch/noCatch/local-logging differences pinned, not normalized.

## C — contingency/agreement

C07 numeric table import uses1e-7 bankers tolerance andInt32 conversion/partial-array failure. CT totals/DF use Int32 accumulators, unlike Common exactInt64 count validation. Chi2/nominal zero-marginal behavior and selective finite guards differ; onlyselected primary fields guarded, noteverysecondary metric. Fisher2x2 Core rotatesminimumcell andcapsN1000, one-sided isminimumtail; midp fields distinct. FFH Core workspace/engine defaults and catchNum differ fromneighboringValue. OR usesrow-major a*d/(b*c) and twoCI methods, riskratio usesCOLUMN risks a/(a+c) dividedbyb/(b+d); no silent transpose. Trend requires2xK/Kx2 withK>=3 and transposesrows2 toorderedrows. Ordinal SomersD columnsdependent, current-culture CIlabel and16bodyrows retained.

Mantel–Haenszel admits nearintegers but DOESNOT roundbeforepassingDoublematrix; Core floors it INPLACE, so value justbelowinteger maychange count. Stackedorientation, zerocell0.5correction, unchecked secondaryp/OR/CI and mutation getnamedfixture. Paired proportion wrapper checksindividualmarginals but notonly1+only2+both<=N; Core uses implicitneither evenwhennegative. Plain CI alpha GetOptionalDouble quietlydefaults invalidtext/nonfinite to.05, unlike TryParseAlpha rejecting; NI/EQ defaultreset discrepancy notedabove. Existing Core ContingencyTable/FisherExact/Equivalence methods are helpers, not complete native table equivalents.

Agreement exports each createfresh Core instances; no handle/session dictionary here. Passing–Bablok ordinaryversusgrouped-block CI andslope-first/intercept-second table shapes are source-traced in Core Agreement/Agreement.vb. Deming wrapper localmin2 conflicts with Coreconstructor>=3; lambda sigmaX²/sigmaY², pointwiseSD/CV/intercept, defaultjackknife/replicateclamp/seed and coefficient/report projections stay distinct. Core WeightedDemingRegression.Fit dispatchesreal CI algorithms; no parity from class name.

Bland–Altman full/reduced/default plot requests differ. Core requires3pairs whilewrappersfirstcheck2, transforms/repeatedfallback/singletons andseed canaffectoutputs. Existing C07 sharedrequest/output operations extended; plot preparation stillfitsmodel/CI, not nativechartcreation. Allowable-bias/decision exports call exact Core fitted-result overloads in EquivalenceNoninferiorityMethods.vb (lines684/706), notrawsimplepair refits; equalitylimits allowed, bias/observedLoA/LoACI flags separate.

LinCCC min3/options/nullconcordance/CI/seed projected tofulltables orrawscalar. Core jackknife request currentlyusesanalytical approximation withnote; BCa usesjackknifeacceleration. Kappa defaultQUADRATIC, malformedcategorylist silentlyignored, customweight enum exposedwithoutcustommatrixargument; category order/duplicates/missingmembers andactualCoreCI dispatch needfixtures. Scalar CCC/Kappa lackICC_VALUE'sfiniteguard. ICC6variants distinguishraggedsubject rows versuscompletematrix, average/single andrater-variance flags; FIT optionallyaddsRC/SEM state, VALUE rawfiniteICC includesnegativeestimates, RC own3rowtable andSEM. Existing Core IntraclassCorrelation ownsformulas; noUI/registration/cachequalifications inferred.

## Provisional candidates, validation and residuals

Baseline inspected candidates are{old_candidates}, not stale299 frominstructiontext;07 added74. Global member candidates{old_candidates}->{len(candidates)}, selectedcandidate rows{sum(int(o['member_rows']) for o in operations)}. New provisional groups{sum(o['operation_id'].startswith('C08-') for o in operations)}, existinggroups extendedseparately. All remain NOT FROZEN; property/helper/initializer overlaps are members, not separate algorithms. Primary .41, related .39/.28/.38 and specialized dependencies are futureowners only.

| Operation | Member rows | Approved batch | Status |
|---|---:|---|---|
{ot}

Fresh distinct managed passes Debug{distinct['Debug']}/Release{distinct['Release']}, foundation repeats excluded. Exact commands/exits/testidentities/counts/log/TRX SHA, Python/Node failures/skips,14 parser assertions, full correctednegative suite and source/index/tracking/protection/generation checks are in continuation08-validation.json. Post-index commands storedseparately under ignored final-current-checks.json avoidrecursivehashes. Strict2 is required forunfinished reviews/controls/wiring; corruption1 remains failure. Nothingwasdisabled toforcegreen. Tooldevelopment encoding/author-syntax corrections are explicit and didnotchangeproductsource.

NOT RUN: nativeExcel/COM/XLL/workbooks/UDFrecalc, Mac/authenticatedOffice, installer/sign/publish/deploy/networkinstall/update,04A.02 numerical goldens. Futuremanualsmoke only: compare omittedversusexplicitalpha NI/EQ andscalar-versusspillp undercurrentformatsettings; compare2xKorientation/MHnearintegers/pairedcounts andagreementcategory/CIseed defaults. Namedfuturefixtures perID are notexecuted parity.

continuation08-artifacts.csv iscomplete added/modified/unchanged proposed-path register; currentindexSHA/sizes covereveryartifact exceptindexself. Futuretracking equalsindexedpaths+index, withindividualforce-add commands forignoredledger; noneexecuted. All priorhistories/sourceauthorities protected. No product/Core/Common/UI/Graphics/exportsignature/manifest/installer/ownerREADME/framework/.gitignore changes; no stage/commit/merge/push. UI1372/Graphics1048/UDF394 pluscontrol/wiring obligations remainfor04A.01.

04A.01 remains incomplete; Gate C2 and exceptions unchanged; 04A.02 was not started. Await owner review of this partial checkpoint.
''')
    paths={p.relative_to(ROOT).as_posix() for p in OUT.rglob('*') if p.is_file()};names=('continuation08-artifacts.csv','proposed-tracked-files.json','future-staging.txt')
    paths|={(OUT/n).relative_to(ROOT).as_posix() for n in names};paths|={p.relative_to(ROOT).as_posix() for p in semantic.TOOLING.iterdir() if p.is_file()};paths.add('docs/architecture/WindowsMigrationLedger.md')
    old_paths={a['path']:a['sha256'] for a in b['original_artifacts']};artifacts=[]
    for p in sorted(paths):
        change='added' if p not in old_paths else 'modified' if not (ROOT/p).exists() or sha(ROOT/p)!=old_paths[p] else 'unchanged'
        if p in {(OUT/n).relative_to(ROOT).as_posix() for n in names}|{'docs/architecture/WindowsMigrationLedger/index.json'}:change='modified' if p in old_paths else 'added'
        artifacts.append(dict(path=p,change_from_continuation08_start=change,tracking='ignored; future individual force-add' if p.startswith('docs/architecture/WindowsMigrationLedger/') else 'visible audit tooling/index',hash_authority='index.json; self excluded'))
    write('continuation08-artifacts.csv',catalog.render(artifacts));emit('proposed-tracked-files.json',dict(status='PROPOSED ONLY; nothing staged',paths=sorted(paths),comparison='Exact indexed artifacts plus self-excluded index.json'))
    write('future-staging.txt','# Future owner-reviewed commands only; NOT EXECUTED.\n'+'\n'.join(('git add -f -- ' if p.startswith('docs/architecture/WindowsMigrationLedger/') else 'git add -- ')+json.dumps(p) for p in sorted(paths)))
    print(json.dumps(dict(status='PARTIAL CHECKPOINT / NOT READY',new_reviews=len(selected),overall=sem['qualified_source_reviews'],selected_exports=sum(r['excel_function_export']=='yes' for r in mapped),proposed_paths=len(paths))))
if __name__=='__main__':
    p=argparse.ArgumentParser(__doc__);p.add_argument('--check',action='store_true');p.add_argument('--guard',action='store_true');args=p.parse_args();CHECK=args.check
    try:
        if args.guard:print(json.dumps(preservation()))
        else:main()
    except (AssertionError,ValueError,KeyError,OSError) as e:print('INTEGRITY FAILURE: '+str(e));raise SystemExit(1)
