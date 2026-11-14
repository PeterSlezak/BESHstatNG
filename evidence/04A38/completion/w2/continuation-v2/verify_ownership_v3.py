"""Independent partial ownership gate; unsupported closure and READY are rejected."""
from functools import cache
import hashlib,json
from pathlib import Path
import verify_slice_v2,verify_host_cache_v2
ROOT=Path(__file__).resolve().parents[5];HERE=Path(__file__).resolve().parent
def need(v,m):
 if not v:raise ValueError(m)
def inspect(root=ROOT,overrides=None):
 root=Path(root);prefix=HERE.relative_to(ROOT).as_posix()+'/';overrides=overrides or {}
 def raw(p):return overrides.get(p,(root/p).read_bytes())
 @cache
 def load(p):return json.loads(raw(p))
 new=verify_slice_v2.inspect(root,overrides);cache=verify_host_cache_v2.inspect(root,overrides)
 h=load(prefix+'historical-ownership-v2.json');now=load(prefix+'current-ownership-v2.json');u=load(prefix+'current-universe-b1-v1.json');old=load('evidence/04A38/completion/w2/continuation-v1/historical-ownership.json');prior=load('evidence/04A38/completion/w2/continuation-v1/current-ownership.json')
 expected={r['canonical_original_id'] for r in old['members'] if r['proof']};expected.update(r['original_id'] for r in new['proofs']);expected.update(r['original_id'] for r in cache['methods']+cache['initializers'])
 universe=load('evidence/04A38/completion/w1/member-behavior-evidence-target-matrix-v3.json')['members'];need(len(h['members'])==len(universe)==704 and {r['canonical_original_id'] for r in h['members']}=={r['canonical_original_id'] for r in universe},'Historical identity lost/duplicated')
 need({r['canonical_original_id'] for r in h['members'] if r['proof']}==expected and h['closed']==len(expected) and h['OPEN']==704-len(expected),'Unsupported historical HOST_ONLY/closure or counts')
 # Earlier39 full-body proofs require their actual current caller+callee+initializer records.
 carried=load('evidence/04A38/completion/w2/independent-delegated-proof-v2.json')['rows'];old_current=prior['members'];bykey={}
 for r in u['members']:bykey.setdefault(r['signature_key'],[]).append(r)
 for config in ['Debug','Release']:
  former={r['method']:r for r in load('evidence/04A38/completion/w2/method-bodies-v2-current-'+config+'.json')};current={r['method']:r for r in load(prefix+'inspection/b1-v1/MethodBodies38-'+config+'.json')}
  for row in carried:
   if row['status'].startswith('OPEN'):continue
   ids=[row['method'],row['target']];ctor=row['method'].split('(')[0].rsplit('.',1)[0]+'..cctor()'
   if ctor in former:ids.append(ctor)
   need(all(i in current and former[i]==current[i] for i in ids),'Previous full IL/target/static proof changed')
 prior_closed=set()
 for r in old_current:
  if not r['review_status'].startswith('OPEN'):
   matches=[m for m in bykey.get(r['signature_key'],[]) if m['body_token_sha256']==r['body_token_sha256'] and (m['parent_member'] or '').split('@')[0]==(r['parent_member'] or '').split('@')[0]]
   need(len(matches)==1,'Prior current body/lambda/parent identity changed or ambiguous')
   prior_closed.add(matches[0]['symbol_id'])
 supported_current=set(prior_closed)
 for r in h['members']:
  if r['proof']:supported_current.update(r['current_ids'])
 for r in now['accessor_proofs']:
  need(r['source']=='Function() Global.BESHStatNG.WorksheetFunctions.ExcelArgReaders.AsString(arg)','Unsupported callback closure');supported_current.add(r['id'])
 native_ids={r['symbol_id'] for r in u['members']};need(len(native_ids)==len(u['members']) and {r['symbol_id'] for r in now['members']}==native_ids,'Current identity orphan/duplicate')
 for r in u['members']:
  b=raw(r['path']);need(hashlib.sha256(b).hexdigest()==r['source_sha256'],'Current source identity stale')
 need({r['symbol_id'] for r in now['members'] if not r['review_status'].startswith('OPEN')}==supported_current,'Unsupported current body/lambda closure')
 need(len(now['fragments'])==len(u['fragments']) and {r['fragment_id'] for r in now['fragments']}=={r['fragment_id'] for r in u['fragments']},'Fragment/branch/inline omitted')
 for r in now['fragments']:need((r['review_status']=='COVERED_BY_REVIEWED_BODY')==(r['member_id'] in supported_current),'Unproved branch/inline parent')
 support=now['support'];need(len(support)==325 and {r['support_id'] for r in support}=={r['support_id'] for r in u['supports']},'Support identity lost/deduplicated')
 for r in support:
  if r['kind'] in ['ModuleBlock','ClassBlock']:need(not r['fully_reconciled'],'Container body closure inferred from declaration')
  if r['kind']=='PropertyStatement':
   need(r['fully_reconciled'] and len(r['proof'])==2 and all(p['entire_accessor_constructor_and_fields_exact'] for p in r['proof']),'Auto-property context missing')
   for p in r['proof']:
    config=p['configuration'];a=load(prefix+'metadata/b1-v1/original-'+config+'.json');b=load(prefix+'metadata/b1-v1/current-'+config+'.json');am={m['method']:m for m in a['methods']};bm={m['method']:m for m in b['methods']}
    for id in p['accessors']+p['constructors']:need(id in am and id in bm and verify_host_cache_v2.stable(am[id])==verify_host_cache_v2.stable(bm[id]),'Property/initializer metadata forged')
 need(h['OPEN']>0,'This bounded partial reader cannot authorize zero-OPEN completion')
 summary=load(prefix+'ownership-summary-v2.json');need(summary['status']=='NOT_READY_04A38' and summary['W2']=='IN_PROGRESS' and summary['W3']=='NOT_STARTED','Forged READY/W2 complete')
 need(summary['historical']==dict(total=704,closed=len(expected),OPEN=704-len(expected)),'Historical summary differs')
 need(summary['current']==dict(total=len(native_ids),closed=len(supported_current),OPEN=len(native_ids)-len(supported_current)),'Current summary differs')
 return dict(status='INDEPENDENT_PARTIAL_OWNERSHIP_PASS_NOT_READY',historical=summary['historical'],current=summary['current'],support=summary['support'],fragments=summary['fragments'],W2='IN_PROGRESS',W3='NOT_STARTED',no_new_reference_authority_needed=True)
if __name__=='__main__':
 try:print(json.dumps(inspect()))
 except Exception as e:print(json.dumps(dict(status='NOT_READY_04A38',error=type(e).__name__+': '+str(e))));raise SystemExit(1)
