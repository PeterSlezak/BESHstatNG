"""C21 chart-dialog source/control audit projections; no review authoring or historical writer."""
from __future__ import annotations
import argparse,collections,datetime,hashlib,json
import catalog,semantic,amendment as a,continuation21_controls as control_guard
ROOT,OUT=catalog.ROOT,catalog.DEFAULT
EVIDENCE=ROOT/'TestResults/04A-01-continuation-21';GROUPS=a.read(EVIDENCE/'targets.json')
SOURCE={s['symbol_id']:s for g in GROUPS for s in g['records']};CONTROL_SCOPE={c['control_id']:c for g in GROUPS for c in g['controls']}
PREFIX='docs/architecture/WindowsMigrationLedger/';CHECK=False
NEW_OPS={'C21-'+g+'01-chart-request-presentation':'04A.34' for g in 'ABCD'}
EXTENSIONS={'C19-B01-shared-dialog-acquisition':'04A.27'}

def write(name,value):
 raw=(value.rstrip()+'\n').encode('utf-8');p=OUT/name
 if CHECK:assert p.is_file() and p.read_bytes()==raw,'C21 report byte drift: '+name
 else:p.write_bytes(raw)
def emit(name,value):write(name,json.dumps(value,indent=2,ensure_ascii=False))
def literal_objects(text):
 dec=json.JSONDecoder();pos=text.index('[')+1;out=[]
 while True:
  while text[pos] in ' \t\r\n,':pos+=1
  if text[pos]==']':return out
  start=pos;_,pos=dec.raw_decode(text,pos);out.append(text[start:pos])
def prefix_errors(now,old):
 previous=literal_objects(old);return [] if literal_objects(now)[:len(previous)]==previous else ['Prior ordered literal objects changed']
def excerpt(s):
 raw=(ROOT/s['path']).read_bytes().decode('utf-8-sig',errors='replace').encode('utf-16-le');i=int(s['span_start']);n=int(s['span_length']);return raw[2*i:2*(i+n)].decode('utf-16-le')
def scope_errors(symbols,decisions):
 ss={s['symbol_id']:s for s in symbols};own=[d for d in decisions if d.get('checkpoint_id')=='04A.01-continuation-21'];ids=[d['symbol_id'] for d in own];errors=[]
 if len(ids)!=len(set(ids)) or set(ids)!=set(SOURCE):return ['Missing/duplicate/foreign C21 member IDs']
 for d in own:
  s=ss.get(d['symbol_id']);f=SOURCE[d['symbol_id']]
  if s is None:errors.append('Missing C21 live source');continue
  if not semantic.qualified(d):errors.append('Unqualified C21 member')
  errors+=semantic.verify_member_contract(d,s)+semantic.verify_pins([d],'C21')
  for key,field in [('kind','source_kind'),('enclosing_symbol_id','source_enclosing_symbol_id'),('profile','source_profile'),('span_start','source_span_start'),('span_length','source_span_length'),('body_sha256','body_sha256'),('start_line','source_declared_start_line'),('end_line','source_declared_end_line'),('body_start_line','source_body_start_line'),('body_end_line','source_body_end_line')]:
   if s[key]!=f[key] or d.get(field)!=s[key]:errors.append('C21 stale '+key)
  if d.get('source_sha256')!=a.sha(ROOT/s['path']) or d.get('source_excerpt')!=excerpt(s):errors.append('C21 raw source/UTF16 excerpt drift')
  if d.get('review_group')!=next(g['group'] for g in GROUPS if d['symbol_id'] in {x['symbol_id'] for x in g['records']}):errors.append('C21 group drift')
  if d.get('equivalence_scope')!='none':errors.append('Unverified C21 whole/helper GUI equivalence')
  if d.get('registration_class')!='explicit non-export' or json.loads(d.get('registration_evidence','[]')):errors.append('False C21 export registration')
  if d['disposition'].startswith('extract-to-') and not all(d.get(k) for k in ('candidate_operation_id','future_batch','owner_role')):errors.append('Unowned C21 candidate')
 return errors
def snapshot_errors(decisions):
 ds={d['symbol_id']:d for d in decisions};changes={r['symbol_id']:r for r in a.read(EVIDENCE/'member-source-refinements.json')};errors=[]
 for g in GROUPS:
  rows=a.read(EVIDENCE/('group-'+g['group']+'-member-evidence.json'))['decisions']
  if {d['symbol_id'] for d in rows}!={s['symbol_id'] for s in g['records']}:errors.append('C21 snapshot ID drift')
  for old in rows:
   change=changes.get(old['symbol_id'])
   if change and change['before']!=old:errors.append('Unbound C21 refinement')
   if ds.get(old['symbol_id'])!=(change['after'] if change else old):errors.append('Unrecorded C21 member change')
 return errors
def operations(selected):
 grouped=collections.defaultdict(list);members=[]
 for d in selected:
  op=d.get('candidate_operation_id','');candidate=d['disposition'].startswith('extract-to-');assert candidate==bool(op)
  if candidate:
   assert op in NEW_OPS|EXTENSIONS,'Unknown C21 operation';assert d['future_batch']==(NEW_OPS|EXTENSIONS)[op],'Existing operation batch changed';assert d['owner_role'];grouped[op].append(d)
  members.append(dict(symbol_id=d['symbol_id'],operation_id=op,disposition=d['disposition'],destination=d['destination'],owner=d['owner_role'],approved_batch=d['future_batch'],member_class='candidate' if candidate else 'host-only'))
 return [dict(operation_id=op,member_rows=len(ds),approved_batch=(NEW_OPS|EXTENSIONS)[op],status='EXTENSION OF EXISTING OPERATION' if op in EXTENSIONS else 'NEW PROVISIONAL OPERATION',destination='BESHStat.Excel.Common',owner='Windows shared-dialog maintainer; Core/Common request/output policy owner' if op in EXTENSIONS else '; '.join(sorted({d['owner_role'] for d in ds})),contributing_owner_roles='; '.join(sorted({d['owner_role'] for d in ds})),related_existing_operations='C14-A01-chart-style-policy; C14/C15/C16 specialty chart policy (comparison/dependency only; no owner or batch reassignment)' if op in NEW_OPS else '',freeze='NOT FROZEN; no project-wide total',source_ids='; '.join(d['symbol_id'] for d in ds)) for op,ds in sorted(grouped.items())],members
def readiness_errors(v):
 return ['Incomplete UI/member/control/wiring falsely READY'] if v.get('status') in ('READY','READY FOR ACCEPTANCE') and (v.get('unreviewed_for_acceptance') or v.get('unresolved_edge_rows') or v.get('qualified_full_control_contracts',0)<2056) else []

def accounting_errors(rows):
 ids=[r.get('control_id') for r in rows];errors=[]
 if len(ids)!=242 or len(set(ids))!=242 or set(ids)!=set(CONTROL_SCOPE):errors.append('Missing/duplicate/foreign C21 scoped control IDs')
 for r in rows:
  if r.get('intent_completeness') not in ('qualified','deferred'):errors.append('Unaccounted C21 control')
  if not all(r.get(k) for k in ('qualification_reason','owner_role','future_batch','verification','source_control_pin','source_witnesses','binding_edge_pins','lifetime_contract')):errors.append('Incomplete owned C21 control evidence')
  if r.get('intent_completeness')=='deferred' and semantic.control_qualified(r):errors.append('Deferred control falsely qualified')
  g=next((g['group'] for g in GROUPS if r.get('control_id') in {c['control_id'] for c in g['controls']}),None)
  if r.get('review_group')!=g:errors.append('Wrong C21 control form/group')
 return errors

def dependency_errors(rows):
 live={s['symbol_id']:s for s in catalog.load(OUT,'symbols.csv')};live.update(semantic.portable_sources());errors=[]
 for row in rows:
  s=live.get(row.get('symbol_id'))
  if s is None or row.get('symbol_id') in SOURCE:errors.append('Missing/foreign C21 external dependency pin');continue
  if any(row.get(k)!=s.get(k) for k in ('path','kind','containing_type','signature','profile','span_start','span_length','body_sha256')):errors.append('Stale C21 external declaration/span/body')
  if row.get('source_sha256')!=a.sha(ROOT/s['path']) or row.get('source_excerpt')!=excerpt(s):errors.append('Stale C21 external dependency bytes')
  if not all(row.get(k) for k in ('dependency_only','owner','future_batch','fixture')):errors.append('Unowned/unverified C21 external dependency')
 return errors
def preservation(history=False):
 started=datetime.datetime.now(datetime.timezone.utc).isoformat();b=a.read(EVIDENCE/'before.json');assert a.git('branch','--show-current')==b['branch'] and a.git('rev-parse','HEAD')==a.git('rev-parse','main')==b['head'];assert not a.git('diff','--name-only') and not a.git('diff','--cached','--name-only');prefix={}
 for name,n in [('review-decisions',2737),('control-review-decisions',406),('wiring-review-decisions',569)]:
  old=(EVIDENCE/'original/scripts/windows-migration-audit'/(name+'.json')).read_bytes().decode('utf-8-sig');now=(semantic.TOOLING/(name+'.json')).read_bytes().decode('utf-8-sig');assert len(json.loads(old))==n and not prefix_errors(now,old),name;prefix[name]=n
 for label,records,base in [('tracked',b['tracked'],ROOT),('GateC',b['gate'],ROOT),('compiled',b['compiled'],ROOT),('start copies',b['original_artifacts'],EVIDENCE/'original')]:assert all((base/r['path']).is_file() and a.sha(base/r['path'])==r['sha256'] and (base/r['path']).stat().st_size==r.get('bytes',(base/r['path']).stat().st_size) for r in records),label
 assert all(a.sha(ROOT/r['retained_path'])==r['sha256'] for r in b['baseline']['entries'])
 historic=[r for r in b['original_artifacts'] if '/baseline/' in r['path'] or r['path'].rsplit('/',1)[-1].startswith(tuple('continuation'+str(i).zfill(2) for i in range(1,21)))];assert all(a.sha(ROOT/r['path'])==r['sha256'] for r in historic),'Historical C01-C20 modified'
 frozen=a.read(EVIDENCE/'decision-freeze.json');assert all(a.sha(semantic.TOOLING/name)==digest for name,digest in frozen['stores'].items());assert all(a.sha(ROOT/r['path'])==r['sha256'] for r in frozen['group_files'])
 raw=a.read(EVIDENCE/'prior-raw-evidence-hashes.json')['files']
 if history:
  errors=[]
  for r in raw:
   p=ROOT/r['path']
   if not p.is_file() or p.stat().st_size!=r['bytes'] or a.sha(p)!=r['sha256']:errors.append(r['path'])
  assert not errors,errors
 return dict(status='PASS exact protected/source/prefix/history evidence',branch=b['branch'],head=b['head'],prior_literal_objects=prefix,tracked=len(b['tracked']),gate_c=len(b['gate']),compiled=len(b['compiled']),baseline=len(b['baseline']['entries']),start_indexed_plus_index=len(b['original_artifacts']),preserved_c19_deferrals=12,preserved_c20_deferrals=23,raw_history_checked=history,raw_expected=len(raw),raw_checked=len(raw) if history else 0,scan_started_utc=started,scan_completed_utc=datetime.datetime.now(datetime.timezone.utc).isoformat())
def validation():
 # Reuse the tested evidence READER, with C21 inputs; never call historical main/writers.
 import continuation19_report as reader
 prior_e,prior_g=reader.EVIDENCE,reader.GROUPS
 try:reader.EVIDENCE,reader.GROUPS=EVIDENCE,GROUPS;result=reader.validation()
 finally:reader.EVIDENCE,reader.GROUPS=prior_e,prior_g
 result['control_refinements']=dict(path=(EVIDENCE/'control-source-refinements.json').relative_to(ROOT).as_posix(),sha256=a.sha(EVIDENCE/'control-source-refinements.json'),records=len(a.read(EVIDENCE/'control-source-refinements.json')),original_groups_unchanged=True)
 result['inherited_default_metadata']=dict(path=(EVIDENCE/'framework-defaults-final.json').relative_to(ROOT).as_posix(),sha256=a.sha(EVIDENCE/'framework-defaults-final.json'),record_count=71,no_controls_instantiated=True)
 result['history_preflight']=a.read(EVIDENCE/'history-preflight.json')
 result['audit_test_identity_analysis']=a.read(EVIDENCE/'audit-identity-analysis.json')
 supplemental=EVIDENCE/'post-authoring-checks.json'
 if supplemental.exists():
  import re,continuation03_report as logreader
  extra=a.read(supplemental);result['post_authoring_checks']=extra
  record=next(r for r in extra if r['id']=='entire-corrected-reconciliation');p=ROOT/record['log'];content=logreader.text(p)
  assert record['exit_code']==0 and a.sha(p)==record['sha256']
  result['negative']=dict(path=p.relative_to(ROOT).as_posix(),sha256=a.sha(p),reported_counts=re.findall(r'Ran (\d+) tests? in',content),passed=bool(re.search(r'\nOK\s*$',content)),failed=re.findall(r'(?:FAIL|ERROR): (test_\w+)',content),authority='Final entire rerun after report-helper correction; original297-pass log remains unchanged')
 result['post_freeze_corrections']=a.read(EVIDENCE/'post-freeze-corrections.json') if (EVIDENCE/'post-freeze-corrections.json').exists() else []
 before=a.read(EVIDENCE/'before.json');version='BESHstatNG/Generated/VersionAttributes.vb';pre=next(r['sha256'] for r in before['tracked'] if r['path']==version)
 result['generated_byte_restoration']=dict(path=version,before_sha256=pre,after_sha256=a.sha(ROOT/version),byte_perfect=pre==a.sha(ROOT/version),transient_copies=[dict(path=p.relative_to(ROOT).as_posix(),sha256=a.sha(p),bytes=p.stat().st_size) for p in sorted(EVIDENCE.glob('*/generated-by-*/BESHstatNG/Generated/VersionAttributes.vb'))])
 return result
def main():
 ss=catalog.load(OUT,'symbols.csv');ds=semantic.read_store('review-decisions.json');cs=semantic.read_store('control-review-decisions.json');cc=catalog.load(OUT,'ui-controls.csv');ee=catalog.load(OUT,'ui-wiring.csv')
 assert len(catalog.load(OUT,'files.csv'))==171 and len(ss)==3866 and len(cc)==2056 and len(catalog.load(OUT,'udf-exports.csv'))==268
 assert collections.Counter(s['profile'] for s in ss)=={'project MyType=Windows':3864,'alternative generated MyType=WindowsForms branch':2}
 assert not scope_errors(ss,ds)+snapshot_errors(ds)+control_guard.verify(cs,ss,cc,ee)
 assert not dependency_errors(catalog.load(OUT,'continuation21-external-dependencies.csv'))
 proof=preservation();proof.pop('scan_started_utc');proof.pop('scan_completed_utc');final=a.read(EVIDENCE/'final-checks.json');full=next(r for r in final if r['id']=='continuation21-preservation');assert full['exit_code']==0 and a.sha(ROOT/full['log'])==full['sha256']
 import continuation03_report as logreader
 guard=json.loads(logreader.text(ROOT/full['log']));assert guard['raw_history_checked'] and guard['raw_checked']==guard['raw_expected'];proof['full_historical_scan']=dict(command=full,result=guard)
 sem=a.read(OUT/'semantic-review-coverage.json');cov=a.read(OUT/'review-coverage.json');assert (sem['qualified_source_reviews'],sem['unreviewed_for_acceptance'])==(2867,999);assert not readiness_errors({**sem,**cov})
 selected=[{**SOURCE[d['symbol_id']],**d} for d in ds if d.get('checkpoint_id')=='04A.01-continuation-21'];ops,members=operations(selected)
 emit('continuation21-targets.json',GROUPS)
 controls=[]
 for g in GROUPS:
  rows=[control_guard.authored_controls()[d['control_id']] for d in a.read(EVIDENCE/('group-'+g['group']+'-control-evidence.json'))['controls']];controls.extend({**CONTROL_SCOPE[d['control_id']],**d} for d in rows)
  emit('continuation21-group-'+g['group']+'.json',dict(group=g['group'],exact_member_ids=[s['symbol_id'] for s in g['records']],exact_control_ids=[c['control_id'] for c in g['controls']],progress=a.read(EVIDENCE/('progress-'+g['group']+'.json')),member_snapshot_sha256=a.sha(EVIDENCE/('group-'+g['group']+'-member-evidence.json')),control_snapshot_sha256=a.sha(EVIDENCE/('group-'+g['group']+'-control-evidence.json'))))
 assert not accounting_errors(controls);assert 353+sum(c['intent_completeness']=='qualified' for c in controls)==sem['qualified_full_control_contracts']
 paths={s['path'] for s in SOURCE.values()};comparisons=[]
 for d in selected:
  for target in json.loads(d['compared_portable_declarations']):
   p=semantic.portable_sources()[target['symbol_id']];comparisons.append({**p,'source_symbol_id':d['symbol_id'],'equivalence_scope':'comparison-only; no whole GUI wrapper equivalence','source_sha256':a.sha(ROOT/p['path']),'source_excerpt':excerpt(p),'comparison':d['source_purpose']})
 for name,rows in [('member-reviews',selected),('control-contracts',controls),('wiring-crosswalk',[e for e in ee if e['path'] in paths]),('portable-comparisons',comparisons),('candidate-operations',ops),('operation-members',members),('gui-core-alignment',a.read(EVIDENCE/'alignment-reviewed.json')),('fixture-crosswalk',a.read(EVIDENCE/'fixtures-reviewed.json'))]:write('continuation21-'+name+'.csv',catalog.render(rows))
 c19=a.read(EVIDENCE/'original/docs/architecture/WindowsMigrationLedger/continuation19-count-deltas.json')['deferred_control_ids'];assert len(c19)==12
 c20=a.read(EVIDENCE/'original/docs/architecture/WindowsMigrationLedger/continuation20-count-deltas.json')['deferred_control_ids'];assert len(c20)==23
 counts=dict(before=a.read(EVIDENCE/'before.json')['starting_review'],after=sem,before_remaining=a.read(EVIDENCE/'before.json')['starting_coverage'],after_remaining=cov,scope_members=len(selected),scope_controls=len(controls),new_complete_controls=sum(c['intent_completeness']=='qualified' for c in controls),deferred_control_ids=[c['control_id'] for c in controls if c['intent_completeness']=='deferred'],preserved_c19_deferred_control_ids=c19,preserved_c20_deferred_control_ids=c20,candidate_member_rows=sum(m['member_class']=='candidate' for m in members),distinct_new_provisional_operations=sum(o['status']=='NEW PROVISIONAL OPERATION' for o in ops),existing_operation_extensions=sum(o['status'].startswith('EXTENSION') for o in ops),host_only_member_rows=sum(m['member_class']=='host-only' for m in members))
 old_wiring=a.read(EVIDENCE/'original/scripts/windows-migration-audit/wiring-review-decisions.json');old_ids={d['edge_id'] for d in old_wiring};new_wire=[d for d in semantic.read_store('wiring-review-decisions.json') if d['edge_id'] not in old_ids]
 counts.update(scope_wiring_rows=sum(e['path'] in paths for e in ee),new_static_wiring_decisions=len(new_wire),prior_wiring_objects_preserved=len(old_wiring),per_group=[dict(group=g['group'],members=len(g['records']),controls=len(g['controls']),qualified=sum(d['intent_completeness']=='qualified' for d in controls if d['review_group']==g['group']),deferred_control_ids=[d['control_id'] for d in controls if d['review_group']==g['group'] and d['intent_completeness']=='deferred'],actual_wiring_rows=sum(e['path'] in {s['path'] for s in g['records']} for e in ee)) for g in GROUPS])
 emit('continuation21-count-deltas.json',counts);emit('continuation21-preservation.json',proof);emit('continuation21-validation.json',validation());write('continuation21.md',(EVIDENCE/'report-text.md').read_text(encoding='utf-8'))
 import continuation14_report as fields
 current={d['symbol_id']:d for d in ds};candidates=[{**s,**current[s['symbol_id']]} for s in ss if semantic.qualified(s) and s['disposition'].startswith('extract-to-')]
 write('inspected-candidates.csv',catalog.render([{k:s.get(k,'') for k in fields.FIELDS} for s in candidates]));reasons=collections.Counter((s['family'],'detailed source review' if semantic.qualified(s) else 'no pinned source review',s['disposition'],s['future_batch'],'inspected' if semantic.qualified(s) else 'provisional; review owed in04A.01') for s in ss)
 write('counts-by-reason-and-batch.csv',catalog.render([dict(family=f,review_reason=r,disposition=d,batch=b,allocation_evidence=e,records=n) for (f,r,d,b,e),n in sorted(reasons.items())]))
 tracked={p.relative_to(ROOT).as_posix() for p in OUT.rglob('*') if p.is_file()}|{p.relative_to(ROOT).as_posix() for p in semantic.TOOLING.iterdir() if p.is_file()}|{'docs/architecture/WindowsMigrationLedger.md',PREFIX+'continuation21-artifacts.csv'};assert not any('__pycache__' in p or '/bin/' in p or '/obj/' in p or p.endswith('.pyc') for p in tracked)
 emit('proposed-tracked-files.json',dict(status='PROPOSED ONLY; nothing staged',paths=sorted(tracked),comparison='Exact indexed artifacts plus index.json'))
 write('future-staging.txt','# Future owner-reviewed commands only; NOT EXECUTED.\n'+'\n'.join(('git add -f -- ' if p.startswith(PREFIX) else 'git add -- ')+json.dumps(p) for p in sorted(tracked)))
 before={r['path']:r['sha256'] for r in a.read(EVIDENCE/'before.json')['original_artifacts']}
 write('continuation21-artifacts.csv',catalog.render([dict(path=p,change_from_C20='unchanged' if p in before and a.sha(ROOT/p)==before[p] else 'modified' if p in before else 'added',tracking='ignored; individual future force-add' if p.startswith(PREFIX) else 'visible audit tooling',hash_authority='index.json size/SHA; self excluded') for p in sorted(tracked)]))
 if CHECK:assert not logreader.verify_index(ROOT,a.read(OUT/'index.json'),sorted(tracked))
 print(json.dumps(dict(status='C21 complete source audit; 04A.01 NOT READY',members=len(selected),overall=2867,UI=548,remaining=999,complete_controls=sem['qualified_full_control_contracts'],new_controls=counts['new_complete_controls'],deferred_controls=len(counts['deferred_control_ids']),investigations=cov['member_investigations'],unresolved_wiring=cov['unresolved_edge_rows'],candidate_rows=counts['candidate_member_rows'],operations=len(ops),proposed_paths=len(tracked))))
if __name__=='__main__':
 parser=argparse.ArgumentParser();parser.add_argument('--check',action='store_true');parser.add_argument('--guard',action='store_true');args=parser.parse_args();CHECK=args.check
 if args.guard:print(json.dumps(preservation(history=True)))
 else:main()
