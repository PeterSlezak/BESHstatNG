"""C29 sidecar integrity and deterministic projection; no acceptance credit."""
from __future__ import annotations
import argparse,collections,csv,hashlib,io,json,pathlib,re
import amendment as a,catalog
from continuation25_report import literals
R=catalog.ROOT;L=catalog.DEFAULT;H=R/'TestResults/04A-01-continuation-29'
COUNTS=dict(zip('ABCDEF',(174,138,90,69,79,72)))
CATEGORIES=('display-only/inert','container/parent-managed','value-input-consumed-by-compute','host-refedit/selection-owned','designer-or-runtime-bound-state','delegate-dynamically-wired','event-contract-without-local-Handles','unbounded/unknown')
def read(p):return json.loads(p.read_bytes())
def sha(p):
    with p.open('rb') as f:return hashlib.file_digest(f,'sha256').hexdigest()
def digest(v):return hashlib.sha256(json.dumps(v,sort_keys=True,ensure_ascii=False).encode()).hexdigest()
def declaration_span(f,root=R):
    with (root/f['edge']['path']).open(encoding='utf-8-sig',newline='') as stream:lines=stream.read().splitlines(keepends=True)
    n=int(f['edge']['start_line'])-1
    return dict(start=len(''.join(lines[:n]).encode('utf-16-le'))//2,length=len(f['declaration_text'].encode('utf-16-le'))//2,unit='UTF-16 code units; full physical declaration line excluding newline')
def frozen():return {f['edge']['edge_id']:f for rs in read(H/'targets.json')['groups'].values() for f in rs}
def rows(group=None):return [r for g in COUNTS if group is None or group==g for r in read(H/('group-'+g+'-triage.json'))]
def artifacts_errors(entries,root=R,progress=False):
    errors=[]
    for n,e in enumerate(entries,1):
        p=root/e['path']
        if not p.is_file() or p.stat().st_size!=e['bytes'] or sha(p)!=e['sha256']:errors.append('Missing/size/hash drift '+e['path'])
        if progress and n%10000==0:print('HISTORY '+str(n)+'/'+str(len(entries))+' files; mismatches='+str(len(errors)),flush=True)
    return errors
def literal_errors(old,new):return [] if old==new else ['Prior literal objects/bytes/order changed']
def allowed_write(path):
    p=pathlib.PurePosixPath(path)
    return (str(p).startswith('TestResults/04A-01-continuation-29/') or
            str(p).startswith('docs/architecture/WindowsMigrationLedger/continuation29') or
            str(p) in {'scripts/windows-migration-audit/continuation29_report.py','scripts/windows-migration-audit/test_continuation29.py','scripts/windows-migration-audit/test_reconciliation.py','scripts/windows-migration-audit/report.py','scripts/windows-migration-audit/README.md',
            'docs/architecture/WindowsMigrationLedger/README.md','docs/architecture/WindowsMigrationLedger/reconciliation.md','docs/architecture/WindowsMigrationLedger/index.json','docs/architecture/WindowsMigrationLedger/proposed-tracked-files.json','docs/architecture/WindowsMigrationLedger/future-staging.txt'})
def record_errors(entries,expected=None,contexts=None,root=R):
    expected=frozen() if expected is None else expected
    contexts={p['edge_id']:p for p in read(H/'edge-contexts.json')} if contexts is None else contexts
    named=read(H/'named-intent-decisions.json');mode_path=L/'continuation27-mode-control-matrix.csv'
    mode_rows=catalog.load(L,mode_path.name);mode_sha=sha(mode_path)
    errors=[];ids=[r.get('edge_id') for r in entries]
    if len(ids)!=len(set(ids)) or set(ids)!=set(expected):errors.append('Missing/duplicate/foreign edge ID')
    for r in entries:
        f=expected.get(r.get('edge_id'))
        if f is None:continue
        e=f['edge'];p=contexts[e['edge_id']]
        identity=dict(group=f['group'],path=e['path'],containing_type=e['containing_type'],owner_id=e['owner_id'],original_target=e['target'],original_target_status=e['target_status'],original_row_sha256=f['row_sha256'],declaration_line=int(e['start_line']),declaration_end_line=int(e['end_line']),declaration_text=f['declaration_text'],source_sha256=f['source_sha256'],source_context_sha256=digest(p),member_pins=p['member_pins'],control_ids=[c['control_id'] for c in f['controls']],C28_control_ids=[c['control_id'] for c in f['C28_triages']])
        if any(r.get(k)!=v for k,v in identity.items()):errors.append('Stale source/declaration/owner/control/group/context '+e['edge_id'])
        src=root/e['path']
        if not src.is_file() or sha(src)!=f['source_sha256']:errors.append('Source file hash mismatch '+e['edge_id'])
        elif src.read_text(encoding='utf-8-sig').splitlines()[int(e['start_line'])-1]!=f['declaration_text']:errors.append('Declaration mismatch '+e['edge_id'])
        elif r.get('declaration_span_utf16')!=declaration_span(f,root):errors.append('Declaration UTF16 span mismatch '+e['edge_id'])
        cat=r.get('intent_category')
        if cat not in CATEGORIES:errors.append('Unversioned intent category')
        if any(r.get(k)!=v for k,v in named[e['edge_id']].items()):errors.append('Named individual source decision/witness drift')
        required='source_intent_basis negative_evidence unresolved_mechanism owner_action named_test_fixture target_gate safe_capture_preconditions evidence_link'.split()
        if any(not r.get(k) for k in required):errors.append('Missing bounded per-edge evidence/owner action/fixture')
        if r.get('classification_basis')!='explicit-named-source-review; no type inference' or not r.get('intent_witnesses'):errors.append('Type-only/generated classification')
        if r.get('source_intent_classification') not in ('source-supported','partially-source-supported','unknown'):errors.append('Invalid source-intent status')
        if r.get('review_outcome') not in ('source-resolved-intent','documented-deferred','owner-decision-required','insufficient-evidence-blocker'):errors.append('Invalid outcome')
        if r.get('runtime_verification')!='NOT RUN' or r.get('capture_authorized') is not False or r.get('full_edge_contract_qualified') is not False or r.get('control_contract_upgraded') is not False:errors.append('Unsubstantiated handler/contract/runtime/golden qualification')
        if r.get('owner_approval')!='PENDING; PROPOSAL ONLY' or r.get('strict_exit')!=2:errors.append('False approval/strict suppression')
        if r.get('C28_risks')!=[{k:t[k] for k in ('control_id','risk_tier','risk_operation_id','owner_role','target_approved_batch','named_test_fixture','is_04A02_goldens_blocker','mitigation_or_safe_capture_constraint')} for t in f['C28_triages']]:errors.append('C28 risk/ownership/golden downgrade')
        if r.get('C28_risk_explanations')!=[{k:t[k] for k in ('control_id','risk_justification','affected_behavior','mechanism','unresolved_missing_fact','upstream_dependencies','downstream_dependencies')} for t in f['C28_triages']]:errors.append('C28 mechanism/dependency drift')
        risks=r.get('C28_risks',[])
        if risks:
            if r.get('risk_tiers')!=sorted({t['risk_tier'] for t in risks}) or r.get('owner_roles')!=sorted({t['owner_role'] for t in risks}) or r.get('approved_target_batches')!=sorted({t['target_approved_batch'] for t in risks}):errors.append('Risk owner/batch carry-forward mismatch')
            if r.get('affected_live_golden_blocked') is not any(t['is_04A02_goldens_blocker']=='true' for t in risks):errors.append('Golden blocker erasure')
        else:
            if r.get('risk_tiers')!=['P2'] or not r.get('independent_edge_risk_justification') or not r.get('qualified_control_evidence'):errors.append('Unmapped risk/qualified-contract justification absent')
        if not r.get('owner_roles') or not r.get('approved_target_batches'):errors.append('Unowned edge')
        if any(not re.fullmatch(r'04A\.(?:0[3-9]|1[0-9]|2[0-8]|3[1-6]|4[4-7])',b) for b in r.get('approved_target_batches',[])):errors.append('Unapproved/invented batch')
        if cat=='delegate-dynamically-wired':
            name=e['event_or_control'];active=[x for x in p['event_search_scope'] if not x['is_comment'] and re.search(r'\bAddHandler\s+(?:Me\.)?'+re.escape(name)+r'\.',x['text'],re.I)]
            if not active or r.get('dynamic_registration_witnesses')!=active:errors.append('Unsubstantiated direct dynamic event participation')
        if 'UNKNOWN_HIGH' in r.get('risk_tiers',[]):
            if r.get('source_intent_classification')!='unknown' or r.get('review_outcome')!='insufficient-evidence-blocker' or not r.get('affected_live_golden_blocked'):errors.append('UNKNOWN_HIGH closed/downgraded')
        if e['containing_type']=='BESHStatNG.UibyID':
            wanted=[m for m in mode_rows if m['control_id'] in r['control_ids']]
            if r.get('UibyID_mode_matrix_sha256')!=mode_sha or r.get('UibyID_mode_records')!=wanted or len(wanted)!=17:errors.append('UibyID mode/Tag/parent dependency lost')
        if cat=='host-refedit/selection-owned' and not r.get('inherited_refedit_member_pins'):errors.append('Missing nested RefEdit lifetime evidence')
    return errors
def guard(group=None,history=False):
    b=read(H/'before.json');errors=[];f=frozen();entries=rows(group);f={i:x for i,x in f.items() if group is None or x['group']==group}
    errors+=record_errors(entries,f)
    if {g:len(v) for g,v in read(H/'targets.json')['groups'].items()}!=COUNTS:errors.append('Group census drift')
    original_targets=[e for e in catalog.load(H/'original/docs/architecture/WindowsMigrationLedger','ui-wiring.csv') if e['target_status']=='no-Handles-found; decorative/default/indirect intent unresolved']
    target_groups=read(H/'targets.json')['groups']
    if {i:x['edge'] for i,x in frozen().items()}!={e['edge_id']:e for e in original_targets}:errors.append('Frozen exact C28 edge census/owner drift')
    for g in COUNTS:
        if read(H/('group-'+g+'-targets.json'))!=target_groups[g]:errors.append('Frozen group association drift '+g)
    for e in b['decision_prefixes']:
        p=pathlib.Path('scripts/windows-migration-audit')/(e['store']+'.json');old=(H/'original'/p).read_bytes();new=(R/p).read_bytes();errors+=literal_errors(old,new)
        if len(literals(new))!=e['objects']:errors.append('Decision object count drift')
    for label in ('tracked','gate','compiled'):errors+=artifacts_errors(b[label])
    errors+=artifacts_errors(b['original_artifacts'],H/'original')
    errors+=artifacts_errors(read(H/'targets.json')['source_pins'])
    for name in ('symbols.csv','files.csv','ui-controls.csv','ui-wiring.csv','udf-exports.csv','coverage.json','semantic-review-coverage.json'):
        if (L/name).read_bytes()!=(H/'original/docs/architecture/WindowsMigrationLedger'/name).read_bytes():errors.append('Original schema/source/qualification projection drift '+name)
    if len(catalog.load(L,'symbols.csv')[0])!=81:errors.append('Member schema is not 81 columns')
    if a.git('branch','--show-current')!=b['branch'] or a.git('rev-parse','HEAD')!=b['head'] or a.git('rev-parse','main')!=b['main'] or a.git('diff','--name-only') or a.git('diff','--cached','--name-only'):errors.append('Branch/source/index drift')
    pre=read(H/'preexisting-paths.json')
    for e in pre:
        if e['path'].startswith('TestResults/'):continue
        if not allowed_write(e['path']):errors+=artifacts_errors([e])
    for g in (group or 'ABCDEF'):
        marker=H/('checkpoint-'+g+'.json')
        if marker.exists():errors+=artifacts_errors(read(marker)['immutable_evidence'])
        else:errors.append('Missing checkpoint marker '+g)
    if history:errors+=artifacts_errors(read(H/'prior-raw-evidence-hashes.json')['files'],progress=True)
    if errors:raise AssertionError(errors)
    return dict(status='PASS',groups_checked=group or 'ABCDEF',classified=len(entries),history_checked=history,qualified_members=3866,qualified_controls=1164,incomplete_controls=892,original_unresolved_wiring=622,static_qualified_new_edges=0,strict_exit=2,gate_c2='UNMET')
def csv_bytes(entries):
    out=io.StringIO(newline='');w=csv.DictWriter(out,fieldnames=list(entries[0]),lineterminator='\n');w.writeheader()
    for r in entries:w.writerow({k:json.dumps(v,sort_keys=True,ensure_ascii=False) if isinstance(v,(dict,list,bool)) else v for k,v in r.items()})
    return out.getvalue().encode()
def render(check=False):
    guard();rs=rows();values={'continuation29-targets.json':(H/'targets.json').read_bytes(),'continuation29-wiring-intent-triage.csv':csv_bytes(rs)}
    cross=[];risk=[];fixtures=[];operations=collections.defaultdict(list)
    for r in rs:
        for control in r['control_ids'] or ['']:
            ts=[t for t in r['C28_risks'] if t['control_id']==control]
            cross.append(dict(edge_id=r['edge_id'],group=r['group'],control_id=control,mapping_evidence='Exact declaration path+line+containing type+identifier; list-valued model permits 0-to-many',C28_control_ids=[t['control_id'] for t in ts],C28_risks=ts,qualified_contract_evidence=r['qualified_control_evidence'],unmatched_justification='Not applicable: exact declaration matched' if control else r['unmatched_justification'],independent_edge_risk_justification=r['independent_edge_risk_justification']))
        risk.append({k:r[k] for k in 'edge_id risk_tiers C28_risks owner_roles approved_target_batches target_gate affected_live_golden_blocked safe_capture_preconditions capture_authorized owner_action'.split()})
        fixtures.append({k:r[k] for k in 'edge_id intent_category named_test_fixture owner_roles approved_target_batches target_gate runtime_verification safe_capture_preconditions capture_authorized'.split()})
        for t in r['C28_risks']:operations[t['risk_operation_id']].append(r['edge_id'])
    values['continuation29-wiring-control-crosswalk.csv']=csv_bytes(cross);values['continuation29-risk-dependency-crosswalk.csv']=csv_bytes(risk);values['continuation29-fixture-crosswalk.csv']=csv_bytes(fixtures)
    values['continuation29-candidate-operations.csv']=csv_bytes([dict(existing_C28_risk_operation_id=i,extension='Source-intent evidence extension to existing risk group; no new algorithm/handler/extraction credit',edge_ids=es,implementation_authorized=False) for i,es in sorted(operations.items())])
    for g in COUNTS:values['continuation29-checkpoint-'+g+'.json']=(H/('checkpoint-'+g+'.json')).read_bytes()
    for name,raw in values.items():
        if check:
            if not (L/name).is_file() or (L/name).read_bytes()!=raw:raise AssertionError('Stale C29 projection '+name)
        else:(L/name).write_bytes(raw)
    return dict(status='PASS',classified=len(rs),groups=COUNTS,categories=dict(collections.Counter(r['intent_category'] for r in rs)),source_intent=dict(collections.Counter(r['source_intent_classification'] for r in rs)),outcomes=dict(collections.Counter(r['review_outcome'] for r in rs)),crosswalk_rows=len(cross),C28_controls=sum(bool(r['C28_risks']) for r in rs),qualified_controls=sum(bool(r['qualified_control_evidence']) for r in rs),original_unresolved=622,static_qualified_new=0,strict_exit=2)
if __name__=='__main__':
    p=argparse.ArgumentParser();p.add_argument('--guard',action='store_true');p.add_argument('--group',choices=list(COUNTS));p.add_argument('--history',action='store_true');p.add_argument('--check',action='store_true');x=p.parse_args();print(json.dumps(guard(x.group,x.history) if x.guard else render(x.check),indent=2))
