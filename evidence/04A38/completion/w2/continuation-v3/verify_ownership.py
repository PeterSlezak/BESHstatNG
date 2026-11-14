"""Recompute partial ownership; no implementer green flags or edge-only closure."""
from functools import cache
import json,hashlib
from pathlib import Path
from verify_names import ROOT,HERE,need,stable
import verify_names,verify_unchanged_v3,verify_text_domains
def inspect(root=ROOT,overrides=None):
 root=Path(root);overrides=overrides or {};base=HERE.relative_to(ROOT).as_posix()+'/'
 def raw(p):return overrides.get(p,(root/p).read_bytes())
 @cache
 def load(p):return json.loads(raw(p))
 names=verify_names.inspect(root,overrides);extra=verify_unchanged_v3.inspect(root,overrides);verify_text_domains.inspect(root,overrides)
 h=load(base+'historical-ownership.json');cur=load(base+'current-ownership.json');u=load(base+'current-universe-names-v3.json');old=load('evidence/04A38/completion/w2/continuation-v2/historical-ownership-v2.json');before=load('evidence/04A38/completion/w2/continuation-v2/current-ownership-v2.json');summary=load(base+'ownership-summary.json')
 expected={r['canonical_original_id'] for r in old['members'] if r['proof']}|{r['original_id'] for r in names['proofs']+extra['proofs']}
 need(len(h['members'])==704 and {r['canonical_original_id'] for r in h['members']}=={r['canonical_original_id'] for r in old['members']},'Lost/duplicated historical identity')
 need({r['canonical_original_id'] for r in h['members'] if r['proof']}==expected and h['closed']==len(expected) and h['OPEN']==704-len(expected),'Unsupported historical closure/host-only')
 bykey={}
 for r in u['members']:bykey.setdefault(r['signature_key'],[]).append(r)
 valid=set()
 for r in before['members']:
  if r['review_status'].startswith('OPEN'):continue
  matches=[m for m in bykey.get(r['signature_key'],[]) if m['body_token_sha256']==r['body_token_sha256'] and (m['parent_member'] or '').split('@')[0]==(r['parent_member'] or '').split('@')[0]];need(len(matches)==1,'Prior reviewed body/lambda changed or ambiguous');valid.add(matches[0]['symbol_id'])
 for r in h['members']:
  if r['proof']:valid.update(r['current_ids'])
 parent=next(r for r in u['members'] if r['name']=='GetVariableNames' and r['path'].endswith('UdfDataImportFormula.vb'));callbacks=[r for r in u['members'] if 'LambdaExpression' in r['kind'] and r['path']==parent['path'] and r['span_start']>=parent['span_start'] and r['span_start']+r['span_length']<=parent['span_start']+parent['span_length']]
 need(len(callbacks)==4 and {r['symbol_id'] for r in callbacks}=={r['id'] for r in cur['callback_proofs']},'Missing/forged callback scope')
 valid.update(r['symbol_id'] for r in callbacks)
 need(len(cur['members'])==len(u['members']) and {r['symbol_id'] for r in cur['members']}=={r['symbol_id'] for r in u['members']},'Current population lost/duplicated')
 need({r['symbol_id'] for r in cur['members'] if not r['review_status'].startswith('OPEN')}==valid,'Unsupported current closure')
 for r in u['members']:need(hashlib.sha256(raw(r['path'])).hexdigest()==r['source_sha256'],'Stale current source identity')
 need(len(cur['fragments'])==len(u['fragments']) and {r['fragment_id'] for r in cur['fragments']}=={r['fragment_id'] for r in u['fragments']},'Fragment/branch/inline omitted')
 for r in cur['fragments']:need((r['review_status']=='COVERED_BY_REVIEWED_BODY')==(r['member_id'] in valid),'Unsupported fragment parent')
 need(len(cur['support'])==325 and {r['support_id'] for r in cur['support']}=={r['support_id'] for r in u['supports']},'Support identity dropped or deduplicated')
 for c in ['Debug','Release']:
  o=load(base+'metadata-v5/names-v3/original-'+c+'.json');n=load(base+'metadata-v5/names-v3/current-'+c+'.json');om={r['method']:r for r in o['methods']};nm={r['method']:r for r in n['methods']}
  # Every generated callback must be a real production ldftn target, including nested accessor.
  main=next(r for r in n['methods'] if r['declaring_type']=='BESHStatNG.UdfDataImport' and r['member_name']=='GetVariableNames');queue=[main];seen=set()
  while queue:
   m=queue.pop()
   for i in m['instructions']:
    if i['op']=='ldftn' and isinstance(i['operand'],str) and i['operand'].startswith('METHOD:BESHStatNG.UdfDataImport+'):
     id=i['operand'][7:];need(id in nm,'Fabricated nested callback edge')
     if id not in seen:seen.add(id);queue.append(nm[id])
  need(len(seen)==4,'Missing actual native name-policy callback')
  for r in cur['support']:
   if r['kind'] in ['ModuleBlock','ClassBlock']:need(not r['fully_reconciled'],'Container header falsely closed children')
   if r['kind']=='PropertyStatement':
    p=next(p for p in r['proof'] if p['configuration']==c)
    for id in p['accessors']+p['constructors']:need(id in om and id in nm and {k:v for k,v in om[id].items() if k!='raw_IL_sha256'}=={k:v for k,v in nm[id].items() if k!='raw_IL_sha256'},'Auto-property/constructor proof drift')
  oldfull={r['method']:r for r in load('evidence/04A38/completion/w2/method-bodies-v2-current-'+c+'.json')};newfull={r['method']:r for r in load(base+'inspection/names-v3/MethodBodies38-'+c+'.json')}
  carried=load('evidence/04A38/completion/w2/independent-delegated-proof-v2.json')['rows'];count=0
  for r in carried:
   if r['status'].startswith('OPEN'):continue
   ids=[r['method'],r['target']];ctor=r['method'].split('(')[0].rsplit('.',1)[0]+'..cctor()'
   if ctor in oldfull:ids.append(ctor)
   need(all(id in newfull and oldfull[id]==newfull[id] for id in ids),'Carried full IL/dependency/init changed');count+=1
  need(count==39,'Missing previous39 proofs')
 need(summary['historical']==dict(total=704,closed=len(expected),OPEN=704-len(expected)),'Historical counts forged')
 need(summary['current']==dict(total=len(u['members']),closed=len(valid),OPEN=len(u['members'])-len(valid)),'Current counts forged')
 need(summary['status']=='NOT_READY_04A38' and summary['W2']=='IN_PROGRESS' and summary['W3']=='NOT_STARTED' and summary['historical']['OPEN']>0,'False READY/current closure')
 return dict(status='INDEPENDENT_PARTIAL_C3_OWNERSHIP_PASS_NOT_READY',historical=summary['historical'],current=summary['current'],support=summary['support'],fragments=summary['fragments'],original_queries=0,W2='IN_PROGRESS',W3='NOT_STARTED')
if __name__=='__main__':
 try:print(json.dumps(inspect()))
 except Exception as e:print(json.dumps(dict(status='NOT_READY_04A38',error=type(e).__name__+': '+str(e))));raise SystemExit(1)
