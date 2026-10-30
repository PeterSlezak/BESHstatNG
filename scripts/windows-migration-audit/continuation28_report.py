"""C28 risk sidecar admission. Never changes static qualification or strict acceptance."""
from __future__ import annotations
import argparse,collections,csv,hashlib,io,json,pathlib,re,functools
import amendment as a,catalog,semantic
R=catalog.ROOT;L=catalog.DEFAULT;H=R/'TestResults/04A-01-continuation-28';T=R/'scripts/windows-migration-audit'
COUNTS=dict(zip('ABCDEF',(182,136,186,194,160,34)))
def digest(v):return hashlib.sha256(json.dumps(v,sort_keys=True,ensure_ascii=False).encode()).hexdigest()
@functools.lru_cache(maxsize=None)
def file_sha(path):return a.sha(path)
def read(p):return json.loads(p.read_bytes())
def unsupported_for(ctx):
 def text(key):
  value=ctx['prior_individual_fields'].get(key,'');return value if isinstance(value,str) else value.get('bounded_excerpt','')
 if text('unsupported_contract_field'):return text('unsupported_contract_field')
 if ctx['prior_evidence_path']:
  for key in ('qualification_reason','rationale','validation_output_contract'):
   if text(key):return 'LEGACY_SCHEMA_NO_UNSUPPORTED_COLUMN; exact prior '+key+': '+text(key)+' [full original row/file hash retained]'
  raise AssertionError('Individual prior sidecar has no usable missing-contract evidence '+ctx['control_id'])
 return 'NO_INDIVIDUAL_CONTROL_SIDECAR: native binding/identity/teardown contract incomplete; minimum direct source inspection is pinned'
def rows(group=None):
 contexts={r['control_id']:r for r in read(H/'control-contexts.json')};out=[]
 for g in COUNTS:
  if group is not None and g!=group:continue
  for original in read(H/('group-'+g+'-triage.json')):
   r=dict(original);r['prior_unsupported_contract_field']=unsupported_for(contexts[r['control_id']]);out.append(r)
 return out
def frozen():return {r['control']['control_id']:r for g in read(H/'targets.json')['groups'] for r in g['records']}
def record_errors(entries,expected=None,contexts=None,root=None):
 file_sha.cache_clear();root=root or R;expected=expected or frozen();contexts=contexts or {c['control_id']:c for c in read(H/'control-contexts.json')};errors=[]
 ids=[r.get('control_id') for r in entries]
 if len(ids)!=len(set(ids)) or set(ids)!=set(expected):errors.append('Missing/duplicate/foreign/already qualified control')
 for r in entries:
  ident=r.get('control_id');f=expected.get(ident)
  if not f:continue
  c=f['control'];ctx=contexts[ident];p=r.get('source_pin',{})
  if r.get('prior_unsupported_contract_field')!=unsupported_for(ctx):errors.append('Missing/misdescribed prior unsupported-field evidence '+ident)
  if any(r.get(k)!=v for k,v in [('path',c['path']),('declaration_line',int(c['line'])),('source_sha256',f['source_sha256']),('current_review_contract',c['review_contract'])]):errors.append('Control source association/hash/qualification drift '+ident)
  if r.get('full_contract_qualified') is not False or r.get('current_contract_status')!='unqualified':errors.append('Risk triage cannot qualify control-v1')
  wanted=dict(original_control_sha256=f['original_control_sha256'],declaration_path=c['path'],declaration_line=int(c['line']),declaration_text=f['declaration_text'],designer_source_sha256=f['source_sha256'],codebehind_path=c['path'].replace('.Designer.vb','.vb'),codebehind_source_sha256=a.sha(R/c['path'].replace('.Designer.vb','.vb')),context_path='TestResults/04A-01-continuation-28/control-contexts.json',context_sha256=digest(ctx),member_body_pins=[dict(symbol_id=s['symbol_id'],body_sha256=s['body_sha256']) for s in ctx['related_member_pins']],prior_record_sha256=f['prior_record_sha256'])
  if p!=wanted:errors.append('Unpinned declaration/member body/prior context '+ident)
  src=root/c['path']
  if not src.is_file() or a.sha(src)!=f['source_sha256']:errors.append('Actual source bytes drift '+ident)
  else:
   lines=src.read_text(encoding='utf-8-sig').splitlines()
   if lines[int(c['line'])-1]!=f['declaration_text']:errors.append('Declaration line association drift '+ident)
  prior=f['prior_evidence_path']
  if prior and (r.get('prior_review_evidence_path')!=prior or not (root/prior).is_file() or file_sha(root/prior)!=f['prior_evidence_file_sha256']):errors.append('Missing/corrupt prior individual sidecar '+ident)
  required='risk_justification affected_behavior owner_role target_approved_batch target_gate named_test_fixture mitigation_or_safe_capture_constraint review_confidence decision_needed exception_link unresolved_missing_fact'.split()
  if any(not r.get(k) for k in required) or r.get('owner_role')=='Windows UI maintainer':errors.append('Generic/unowned/unbounded triage '+ident)
  if not re.fullmatch(r'04A\.(?:0[3-9]|[1-3][0-9]|4[0-8])',r.get('target_approved_batch','')):errors.append('Invented/unapproved future batch '+ident)
  if r.get('owner_approval_marker')!='PENDING; NO C28 DEFERRAL ACCEPTANCE' or r.get('fixture_status')!='NOT RUN':errors.append('Unapproved acceptance/test claim '+ident)
  tier=r.get('risk_tier');block=tier in ('P0','P1','UNKNOWN_HIGH')
  if r.get('is_04A02_goldens_blocker') is not block:errors.append('Unsafe golden blocker erasure '+ident)
  treatment={'P0':'MUST_MITIGATE_BEFORE_04A02','P1':'IMPLEMENTATION_OWNED_04Axx','P2':'GATE_C2_OWNER_REVIEW','P3':'GATE_C2_OWNER_REVIEW','UNKNOWN_HIGH':'BLOCKED_UNKNOWN'}.get(tier)
  if r.get('acceptance_treatment_proposal')!=treatment:errors.append('Risk acceptance downgrade '+ident)
  if tier=='UNKNOWN_HIGH' and (r.get('triage_status')!='insufficient-evidence-blocker' or r.get('evidence_level')!='unknown' or 'missing' not in r.get('decision_needed','').lower()):errors.append('Unknown risk accepted without missing fact/owner '+ident)
  if tier=='P3':
   proof=r.get('P3_exclusion_proof',{});role=proof.get('role');name=ident.rsplit('.',1)[-1]
   inert={'lblCount','lbCounts','btAddCounts','btRemoveCounts','lblSampleSize','lbSampleSize','btAddSampleSize','btRemoveSampleSize','lblExposure','lbExposure','btAddExposure','btRemoveExposure'}
   ok=(role=='caption-only' and c['type_declaration'].endswith('.Label') and not ctx['live_reference_lines'] and proof.get('external_source_token_hits')==[] and proof.get('dynamic_lookup_excluded') is True and r.get('mechanism')=='ornament') or (role=='exact12-reserved-remnant' and '.Ui22MultivariateControlCharts.' in ident and name in inert and ctx['direct_consumer_member_names']==['ConfigureDesignerCorrections'] and r.get('mechanism')=='inert-remnant')
   if not ok:errors.append('High-impact control cannot become P3 without source exclusion '+ident)
  if r.get('related_wiring_edge_ids')!=[e['edge_id'] for e in ctx['related_edges']] or r.get('related_member_ids')!=[s['symbol_id'] for s in ctx['related_member_pins']]:errors.append('Lost actual wiring/member dependencies '+ident)
 return errors
def invariant_errors(v):
 expected=dict(source_members=3866,ui_members=1547,qualified_controls=1164,incomplete_controls=892,unresolved_wiring=622,exports=268,strict_exit=2,gate_c2='UNMET',acceptance_policy='UNCHANGED')
 return [] if all(v.get(k)==x for k,x in expected.items()) else ['C28 cannot alter source/control/wiring/strict/gate denominator or acceptance']
def literal_errors(old,new):return [] if old==new else ['Prior literal decision bytes/order changed']
def artifact_errors(entries,root):
 return ['Indexed size/hash drift '+e['path'] for e in entries if not (root/e['path']).is_file() or (root/e['path']).stat().st_size!=e['bytes'] or a.sha(root/e['path'])!=e['sha256']]
def mode_errors(matrix,entries):
 ids={r['control_id'] for r in entries if '.UibyID.' in r['control_id']};old=catalog.load(H/'original/docs/architecture/WindowsMigrationLedger','continuation27-mode-control-matrix.csv');expected=[r for r in old if r['control_id'] in ids]
 return [] if matrix==expected and len(matrix)==len(ids)*17 and len({(r['control_id'],r['mode']) for r in matrix})==len(matrix) else ['Unpinned Tag/mode/tab reparenting dependency']
def guard(group=None,history=False):
 b=read(H/'before.json');errors=[];controls=catalog.load(L,'ui-controls.csv');current={c['control_id']:c for c in controls if c['review_contract']!='04A.01-control-v1'};f=frozen()
 sem=read(L/'semantic-review-coverage.json');ui=next(x for x in sem['families'] if x['family']=='UI')
 errors+=invariant_errors(dict(source_members=sem['qualified_source_reviews'],ui_members=ui['qualified_source_reviews'],qualified_controls=sem['qualified_full_control_contracts'],incomplete_controls=len(current),unresolved_wiring=sum(sem['remaining_unresolved_edges_by_kind'].values()),exports=len(catalog.load(L,'udf-exports.csv')),strict_exit=2,gate_c2='UNMET',acceptance_policy='UNCHANGED'))
 if a.git('branch','--show-current')!=b['branch'] or a.git('rev-parse','HEAD')!=b['head'] or a.git('rev-parse','main')!=b['main'] or a.git('diff','--name-only') or a.git('diff','--cached','--name-only'):errors.append('Git source/index authority drift')
 if set(current)!=set(f) or len(controls)!=2056 or len(current)!=892:errors.append('C27 unqualified census drift')
 for ident in set(current)&set(f):
  if current[ident]!=f[ident]['control']:errors.append('Original control record changed '+ident)
 groups=read(H/'targets.json')['groups']
 if {g['group']:len(g['records']) for g in groups}!=COUNTS or len(f)!=892:errors.append('Six group census drift')
 for item in b['decision_prefixes']:
  p=pathlib.Path('scripts/windows-migration-audit')/(item['store']+'.json');errors+=literal_errors((H/'original'/p).read_bytes(),(R/p).read_bytes())
 errors+=artifact_errors(b['original_artifacts'],H/'original')
 if a.sha(H/'original/docs/architecture/WindowsMigrationLedger/index.json')!=b['index_sha256']:errors.append('C27 self index hash drift')
 for e in read(H/'scope-freeze-manifest.json'):
  if a.sha(R/e['path'])!=e['sha256']:errors.append('Source/target freeze drift '+e['path'])
 for name in ('symbols.csv','files.csv','ui-controls.csv','ui-wiring.csv','udf-exports.csv','remaining-member-review.csv'):
  if (L/name).read_bytes()!=(H/'original/docs/architecture/WindowsMigrationLedger'/name).read_bytes():errors.append('Current source/qualification/edge projection changed '+name)
 for name in ('tracked','gate','compiled'):
  for p in b[name]:
   if not (R/p['path']).is_file() or a.sha(R/p['path'])!=p['sha256']:errors.append('Protected drift '+p['path'])
 for e in b['original_artifacts']:
  p=pathlib.PurePosixPath(e['path']);n=p.name
  if '/baseline/' in str(p) or re.match(r'continuation(?:0[1-9]|1[0-9]|2[0-7])(?:[.-])',n):
   if not (R/p).is_file() or (R/p).stat().st_size!=e['bytes'] or a.sha(R/p)!=e['sha256']:errors.append('Historical indexed artifact drift '+str(p))
 for g in (group if group else COUNTS):
  entries=rows(g);expected={i:x for i,x in f.items() if any(i==z['control']['control_id'] for z in next(x for x in groups if x['group']==g)['records'])};errors+=record_errors(entries,expected)
  for e in read(H/('checkpoint-'+g+'.json'))['immutable_evidence']:
   if a.sha(R/e['path'])!=e['sha256']:errors.append('Frozen group checkpoint drift '+e['path'])
 if history:
  manifest=read(H/'prior-raw-evidence-hashes.json')['files']
  for e in manifest:
   p=R/e['path']
   if not p.is_file() or p.stat().st_size!=e['bytes'] or a.sha(p)!=e['sha256']:errors.append('Historical raw path/hash drift '+e['path'])
  actual={p.relative_to(R).as_posix() for p in (R/'TestResults').rglob('*') if p.is_file() and not p.is_relative_to(H)}
  if not {e['path'] for e in manifest}<=actual:errors.append('Historical raw path membership lost')
 if errors:raise AssertionError(errors)
 return dict(status='PASS',groups_checked=group or 'ABCDEF',history_checked=history,source_members=3866,ui_members=1547,qualified_controls=1164,incomplete_controls=892,unresolved_wiring=622,exports=268,strict_exit=2,gate_c2='UNMET',acceptance_policy='UNCHANGED')
def csv_bytes(entries):
 o=io.StringIO(newline='');w=csv.DictWriter(o,fieldnames=list(entries[0]),lineterminator='\n');w.writeheader()
 for r in entries:w.writerow({k:json.dumps(v,ensure_ascii=False,sort_keys=True) if isinstance(v,(dict,list,bool)) else v for k,v in r.items()})
 return o.getvalue().encode()
def render(check=False):
 guard();entries=rows();values={'continuation28-targets.json':(json.dumps(read(H/'targets.json'),indent=2,ensure_ascii=False)+'\n').encode(),'continuation28-control-triage.csv':csv_bytes(entries)}
 if check and (H/'publication-input-manifest.json').exists():
  errors=artifact_errors(read(H/'publication-input-manifest.json'),R)
  if errors:raise AssertionError(errors)
 ownkeys='control_id risk_operation_id owner_role target_approved_batch target_gate named_test_fixture is_04A02_goldens_blocker mitigation_or_safe_capture_constraint triage_status owner_approval_marker'.split();values['continuation28-deferral-ownership.csv']=csv_bytes([{k:r[k] for k in ownkeys} for r in entries])
 operations=collections.defaultdict(list)
 for r in entries:operations[r['risk_operation_id']].append(r)
 priority=[];gold=[]
 for ident,rs in sorted(operations.items()):
  r=rs[0];base=dict(risk_operation_id=ident,affected_control_ids=[x['control_id'] for x in rs],risk_tier=r['risk_tier'],behavior=r['affected_behavior'],risk_justification=r['risk_justification'],owner_role=r['owner_role'],target_approved_batch=r['target_approved_batch'],named_test_fixture=r['named_test_fixture'],source_pins=[x['source_pin'] for x in rs],acceptance_treatment_proposal=r['acceptance_treatment_proposal'],owner_approval_marker=r['owner_approval_marker'])
  base['target_approved_batch']=sorted({x['target_approved_batch'] for x in rs});base['owner_role']=sorted({x['owner_role'] for x in rs});base['named_test_fixture']=[x['named_test_fixture'] for x in rs]
  if r['risk_tier'] in ('P0','P1','UNKNOWN_HIGH'):priority.append(base)
  gold.append(dict(**base,live_control_capture='BLOCKED' if r['is_04A02_goldens_blocker'] else 'CONDITIONAL; native unvalidated',safe_capture_constraint=r['mitigation_or_safe_capture_constraint'],capture_authorized=False,fixture_status='NOT RUN'))
 values['continuation28-priority-register.csv']=csv_bytes(priority);values['continuation28-golden-capture-preconditions.csv']=csv_bytes(gold)
 matrix=[r for r in catalog.load(L,'continuation27-mode-control-matrix.csv') if r['control_id'] in {x['control_id'] for x in entries}];errors=mode_errors(matrix,entries)
 if errors:raise AssertionError(errors)
 mapping={r['control_id']:r for r in entries};mode_sha=a.sha(L/'continuation27-mode-control-matrix.csv');values['continuation28-mode-dependency-crosswalk.csv']=csv_bytes([dict(**r,C28_risk_operation_id=mapping[r['control_id']]['risk_operation_id'],C28_risk_tier=mapping[r['control_id']]['risk_tier'],C28_target_batch=mapping[r['control_id']]['target_approved_batch'],prior_mode_matrix_sha256=mode_sha) for r in matrix])
 for name,raw in values.items():
  if check:
   if not (L/name).is_file() or (L/name).read_bytes()!=raw:raise AssertionError('Stale C28 projection '+name)
  else:(L/name).write_bytes(raw)
 files=sorted([p for p in L.glob('continuation28*') if p.name!='continuation28-artifacts.csv']+[T/n for n in ('continuation28_report.py','test_continuation28.py','test_reconciliation.py','report.py','README.md')])
 artifact_raw=csv_bytes([dict(path=p.relative_to(R).as_posix(),bytes=p.stat().st_size,sha256=a.sha(p)) for p in files])
 if check:
  if (L/'continuation28-artifacts.csv').read_bytes()!=artifact_raw:raise AssertionError('C28 artifact size/hash manifest drift')
 else:(L/'continuation28-artifacts.csv').write_bytes(artifact_raw)
 members={p.relative_to(R).as_posix() for p in L.rglob('*') if p.is_file()}|{p.relative_to(R).as_posix() for p in T.iterdir() if p.is_file()}|{'docs/architecture/WindowsMigrationLedger.md'}
 proposal=read(L/'proposed-tracked-files.json');proposal['paths']=sorted(members);proposal['comparison']='C28 exact future staging proposal only; no staging executed';raw=(json.dumps(proposal,indent=2,ensure_ascii=False)+'\n').encode()
 if check:
  if (L/'proposed-tracked-files.json').read_bytes()!=raw:raise AssertionError('C28 staging membership drift')
 else:(L/'proposed-tracked-files.json').write_bytes(raw)
 return dict(status='PASS',risk_triages=len(entries),groups=COUNTS,risk_tiers=dict(collections.Counter(r['risk_tier'] for r in entries)),risk_operations=len(operations),priority_operations=len(priority),strict_exit=2)
if __name__=='__main__':
 p=argparse.ArgumentParser();p.add_argument('--group',choices=list(COUNTS));p.add_argument('--guard',action='store_true');p.add_argument('--history',action='store_true');p.add_argument('--check',action='store_true');x=p.parse_args();print(json.dumps(guard(x.group,x.history) if x.guard else render(x.check),indent=2))
