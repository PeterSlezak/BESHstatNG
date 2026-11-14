"""Read-only exact-byte, whole-body, compiled and exhaustive ownership checkpoint.

This is a partial W2 checkpoint. It executes no original code or test campaign.
"""
import hashlib,json,sys,posixpath,runpy,copy
from pathlib import Path
from collections import Counter
ROOT=Path(__file__).resolve().parents[5]
HERE=Path(__file__).resolve().parent
PREFIX='evidence/04A38/completion/w2/continuation-v15/'
OLD='evidence/04A38/completion/w2/continuation-v14/'
sys.path.insert(0,str(HERE))
AUTH=__AUTH__

def need(value,message):
 if not value:raise ValueError(message)

def compute(root=ROOT,overrides=None,predecessor=True):
 root=Path(root);overrides=overrides or {};cache={}
 def raw(path):
  path=posixpath.normpath(path)
  if path not in cache:cache[path]=overrides[path] if path in overrides else (root/path).read_bytes()
  return cache[path]
 load=lambda path:json.loads(raw(path))
 def check(row):need((len(raw(row['path'])),hashlib.sha256(raw(row['path'])).hexdigest())==(row['bytes'],row['sha256']),'Changed source/evidence '+row['path'])
 for path,digest in AUTH.items():need(hashlib.sha256(raw(path)).hexdigest()==digest,'Sealed C15 input changed '+path)
 transitions=load(PREFIX+'source-transitions-final-v1.json')['files'];projected=dict(overrides)
 need(len(transitions)==8,'Wrong changed-source population')
 for t in transitions:
  check(t['after']);check(t['snapshot']);need(raw(t['path'])==raw(t['snapshot']['path']),'Current source/snapshot differs')
  if t['before']:
   check(t['preimage']);need((t['before']['bytes'],t['before']['sha256'])==(t['preimage']['bytes'],t['preimage']['sha256']),'Entry source preimage differs');projected[t['path']]=raw(t['preimage']['path'])
  else:need(t['entry_state']=='ABSENT','Missing new-source entry state')
 for row in load(PREFIX+'production-test-freeze-v3.json')['sources']:check(row)
 need(load(PREFIX+'owner-replacement-debug-authorization-v1.json')['original_queries']==0,'Wrong replacement authority')
 if predecessor:
  sys.path.append(str(root/OLD))
  from verify_c14_checkpoint_v2 import compute as prior_compute
  prior_result=prior_compute(root,projected)
 else:prior_result='SKIPPED_ONLY_FOR_EXPLICIT_READER_CONTROL'
 from proof_kernel_v8 import map_current,bind,definitions,closure,NESTED_CACHE_DECLARATIONS
 from source_views import body
 u=load(PREFIX+'final-v1/current-universe.json');o=load('evidence/04A38/completion/w1/original-universe.json')
 old=load(OLD+'current-ownership-v2.json');oh=load(OLD+'historical-ownership-v2.json')['members']
 mapping=map_current(old['members'],u['members']);reverse={n['symbol_id']:k for k,n in mapping.items()};oldm={n['symbol_id']:n for n in old['members']}
 members={n['symbol_id']:n for n in u['members']};om={n['symbol_id']:n for n in o['members']};hm={h['canonical_original_id']:h for h in oh}
 need(len(mapping)==len(members)==726 and len(oh)==704,'Changed source universe')
 inherited={mapping[n['symbol_id']]['symbol_id'] for n in old['members'] if not n['review_status'].startswith('OPEN')}
 need(len(inherited)==529 and all(mapping[n['symbol_id']]['body_sha256']==n['body_sha256'] for n in old['members'] if not n['review_status'].startswith('OPEN')),'Previously supported body changed')
 cs=load(PREFIX+'whole-body-contracts-v1.json')['contracts'];cb=load(PREFIX+'callback-proofs-v1.json')['rows'];props=load(PREFIX+'property-body-contracts-v1.json')['contracts'];pc=load(PREFIX+'portable-api-contracts-v1.json')['contracts']
 need((len(cs),len(cb),len(props),len(pc))==(102,13,4,91),'Incomplete whole-body/callback/property/portable cohort')
 snapshots={r['source']['path']:r['snapshot'] for r in load('evidence/04A38/preedit/entry-source-snapshot.json')['files']}
 for c in cs+cb:
  n=members[c['current_id']];need(n==c['current_record'],'Current identity/span forged');text,v=body(raw(n['path']),n)
  need(text==c['current_whole_body'],'Complete current body differs')
  if c['original_id']:
   original=om[hm[c['original_id']]['git_original_id']];need(original==c['original_record'] and c['original_source']==snapshots[original['path']],'Wrong original authority');check(c['original_source']);text,v=body(raw(c['original_source']['path']),original);need(text==c['original_whole_body'],'Complete original body differs')
  need(c['new_support']==oldm[reverse[c['current_id']]]['review_status'].startswith('OPEN'),'Wrong new/retained disposition')
 for c in props:
  original=om[hm[c['original_id']]['git_original_id']];need(original==c['original_record'] and c['current_ids']==[],'Invented current property identity')
  check(c['original_source']);check(c['current_portable_source']);need(body(raw(c['original_source']['path']),original)[0]==c['original_whole_body'],'Wrong original property body')
 new={c['original_id']:c for c in cs+cb+props if c['new_support']};closed=inherited|{c['current_id'] for c in cs+cb}
 need(None not in new and len(new)==41 and len(closed)==566,'Wrong new closure counts')
 shapes={r['symbol_id']:r for r in load(PREFIX+'final-v1/declaration-shapes.json')['rows']}
 diag=load(PREFIX+'cohort-diagnostic-v1.json');graphs={}
 for owner,name in [('CoxUDFs','CoxModelHandle'),('GEEUDFs','GeeHandle')]:NESTED_CACHE_DECLARATIONS['BESHStatNG.WorksheetFunctions.'+owner]={name}
 for cfg in ['Debug','Release']:
  meta=load(PREFIX+'final-v1/current-'+cfg+'.json');prior=load(OLD+'metadata-all/packet-v1-context-v5/current-'+cfg+'.json');defs=definitions(meta);bd=definitions(prior)
  need(meta['metadata_only'] and meta['original_semantic_calls']==0,'Unexpected original activity')
  for c in cs:
   r=next(r for r in c['compiled'] if r['configuration']==cfg);m=bind(c['current_record'],meta,shapes)
   need(r['complete_method']==m and r['method']==m['method'] and r['transitive']==closure(meta,[m['method']]),'Actual method/signature/ByRef/CIL/dependency differs')
  for c in cb:
   r=next(r for r in c['compiled'] if r['configuration']==cfg);m=r['complete_method'];need(m==defs[m['method']],'Actual callback CIL differs')
   parent=defs[r['parent_method']];sites=[x for x in parent['instructions'] if x['op']=='ldftn' and x['operand']=='METHOD:'+m['method']]
   need(len(sites)==1,'Missing exact compiled parent/callback binding')
  for p in pc:
   check(p['source']);text,v=body(raw(p['source_record']['path']),p['source_record']);need(text==p['whole_body'],'Portable complete source differs')
   r=next(r for r in p['compiled'] if r['configuration']==cfg);need(bind(p['source_record'],meta,shapes)==r['complete_method'],'Portable complete compiled signature/CIL differs')
   need(r['signature']==r['complete_method']['signature'],'Portable signature differs')
  graph=closure(meta,diag['roots'][cfg]);need(graph==diag['graphs'][cfg] and len(graph['methods'])==1053,'Transitive ownership graph differs');graphs[cfg]=len(graph['methods'])
  ownerproof=next(r for r in load(PREFIX+'constructed-owner-proofs-v1.json')['configurations'] if r['configuration']==cfg)
  need({r['type']['type'] for r in ownerproof['owners']}=={defs[k]['declaring_type'] for k in graph['methods']},'Constructed owner omitted')
  for r in ownerproof['owners']:
   need(r['type']==next(t for t in meta['types'] if t['type']==r['type']['type']) and r['constructors_and_initializers']==[m for m in meta['methods'] if m['declaring_type']==r['type']['type'] and m['member_name'] in ['.ctor','.cctor']],'Constructed fields/default/initializer mismatch')
  state=next(r for r in load(PREFIX+'cox-module-state-v1.json')['rows'] if r['configuration']==cfg)
  ometa=load(OLD+'metadata-all/packet-v1-context-v5/original-'+cfg+'.json');owner='BESHStatNG.WorksheetFunctions.CoxUDFs'
  need(state['current_type']==next(t for t in meta['types'] if t['type']==owner) and state['original_type']==next(t for t in ometa['types'] if t['type']==owner),'Cox module state context forged')
  need(state['current_type']['fields']==state['original_type']['fields'],'Cox native cache fields changed')
  for kind,data in [('current',meta),('original',ometa)]:need(state[kind+'_initializers']==[m for m in data['methods'] if m['declaring_type']==owner and m['member_name'] in ['.ctor','.cctor']],'Cox module initializer binding changed')
  strip_init=lambda m:{k:v for k,v in m.items() if k not in ['raw_IL_sha256','signature']}
  need(list(map(strip_init,state['current_initializers']))==list(map(strip_init,state['original_initializers'])),'Cox native cache initializer body changed')
  delta=next(r for r in load(PREFIX+'compiled-method-deltas-v1.json')['configurations'] if r['configuration']==cfg)
  strip=lambda m:{k:v for k,v in m.items() if k not in ['raw_IL_sha256','signature']}
  changed={k for k in bd if k in defs and strip(bd[k])!=strip(defs[k])};added=set(defs)-set(bd)
  need(changed=={r['method'] for r in delta['changed']} and len(changed)==12 and len(added)==14 and not set(bd)-set(defs),'Compiled delta population differs')
  need(all(r['original']==bd[r['method']] and r['current']==defs[r['method']] for r in delta['changed']),'Compiled delta bytes differ')
  context=[k for k in changed if '<PrivateImplementationDetails>' in k];need(len(context)==1,'Compiler context delta not isolated')
  k=context[0];need({a:b for a,b in strip(bd[k]).items() if a!='nonliteral_static_fields'}=={a:b for a,b in strip(defs[k]).items() if a!='nonliteral_static_fields'},'Compiler-generated method body changed')
  need(all(not defs[k]['declaring_type'].startswith('BESHStatNG.ExcelCommon.') and 'BESHStat.Core,' not in k for k in changed-set(context)),'Unexpected portable/scientific method change')
  exports=load(PREFIX+'final-v1/exports-'+cfg+'.json')['exports'];original_exports=load('evidence/04A38/preedit/compiled-'+cfg+'.json')['exports']
  need(len(exports)==268 and sorted(exports,key=lambda r:r['effective_excel_name'])==sorted(original_exports,key=lambda r:r['effective_excel_name']),'Export ABI/attributes/help/defaults differs')
 results={}
 for label in ['composition','fragments','host']:
  results[label]=runpy.run_path(str(root/(PREFIX+'verify_c15_'+label+'_v1.py')))['RESULT']
 cur=load(PREFIX+'current-ownership-v3.json');history=load(PREFIX+'historical-ownership-v2.json')['members']
 need([h['canonical_original_id'] for h in history]==[h['canonical_original_id'] for h in oh],'Historical identity/order differs')
 for h,b in zip(history,oh):
  need(bool(h['proof'])==(bool(b['proof']) or h['canonical_original_id'] in new),'Unsupported historical closure')
  need(h['current_ids']==[mapping[k]['symbol_id'] for k in b['current_ids']],'Historical/current counterpart changed')
  if h['canonical_original_id'] not in new:need(h['proof']==b['proof'],'Retained proof changed')
 need(Counter(n['symbol_id'] for n in cur['members'])==Counter(members.keys()),'Current complement differs')
 need(all((not n['review_status'].startswith('OPEN'))==(n['symbol_id'] in closed) for n in cur['members']),'Unsupported current closure')
 skey=lambda s:(s['path'],s['kind'],s['owner'],s['declaration'])
 before={skey(s):s for s in old['support']};ss=cur['support'];contexts=load(PREFIX+'host-cache-contexts-v1.json')['rows'];new_support={r['source_support']['support_id'] for r in contexts}|{s['support_id'] for s in ss if s['declaration']=='Public Module CoxUDFs'}
 need(len(new_support)==6 and Counter(s['support_id'] for s in ss)==Counter(s['support_id'] for s in u['supports']),'Support declaration population differs')
 for s in ss:
  need(s['children']==next(t for t in u['supports'] if t['support_id']==s['support_id']).get('children',[]),'Stale current child identity list')
  expected=before[skey(s)]['fully_reconciled'] or s['support_id'] in new_support;need(s['fully_reconciled']==expected,'Unsupported support closure')
  if s['support_id'] in new_support:
   owner=s['owner'].rstrip('.')+'.'+s['declaration'].split()[-1]
   need(all(n['symbol_id'] in closed for n in u['members'] if n['path']==s['path'] and (n['owner']==owner or n['owner'].startswith(owner+'.'))),'Support container has unclosed child')
 need(sum(s['fully_reconciled'] for s in ss if s['distinct'])==262,'Distinct support count differs')
 headers=load(PREFIX+'support-fragment-crosswalk-v1.json');hc={h['git_original_id'] for h in history if h['proof']}
 for population,universe,rows in [('current',u,cur['fragments']),('original',o,load(PREFIX+'original-fragment-parent-crosswalk-v2.json')['rows'])]:
  source={f['fragment_id']:f for f in universe['fragments']};need(Counter(f['fragment_id'] for f in rows)==Counter(source.keys()),'Fragment identity/complement differs')
  need(set(headers[population]).issubset(source),'Invented header fragment')
  for f in rows:
   covered=f['member_id'] in (closed if population=='current' else hc) or f['fragment_id'] in headers[population]
   need(all(f[k]==v for k,v in source[f['fragment_id']].items()) and (f['review_status']!='OPEN' if population=='current' else f['covered'])==covered,'Unsupported fragment closure')
   if f['fragment_id'] in headers[population]:need(f['member_id'] is None,'Support closure masked member branch')
 queue=load(PREFIX+'remaining-work-queue-v2.json');remaining=[r for r in load(OLD+'remaining-work-queue-v1.json')['members'] if r['original_id'] not in new]
 need(queue['members']==remaining and queue['first_members']==remaining[:24] and len(remaining)==167,'Ordered historical queue differs')
 need(Counter(r['current_id'] for r in queue['current_OPEN'])==Counter(set(members)-closed),'Current OPEN complement differs')
 need(Counter(r['support_id'] for r in queue['support_OPEN'])==Counter(s['support_id'] for s in ss if s['distinct'] and not s['fully_reconciled']),'Support OPEN complement differs')
 ledger=load(PREFIX+'canonical-identity-status-v1.json')['entries'];need(len(ledger)==1733 and len({(r['population'],r['identity']) for r in ledger})==1733,'Canonical ledger duplicate/missing identities')
 need(Counter(r['transition'] for r in ledger)==Counter(NEW=84,RETAINED=1281,OPEN=368),'Canonical transition counts differ')
 for r in ledger:need(r['supported']==(r['transition']!='OPEN') and (r['transition']=='RETAINED')==r['prior_supported'],'Canonical transition differs')
 summary=load(PREFIX+'ownership-summary-v2.json');need(summary['historical']==dict(total=704,closed=537,OPEN=167) and summary['current']==dict(total=726,closed=566,OPEN=160) and summary['support']['distinct_closed']==262,'Wrong final denominators')
 validation=load(PREFIX+'validation-v1.json');need(validation['Debug']['outcomes']==dict(Passed=20990,Failed=14) and validation['Debug']['new_passes']==64,'Managed final identity result differs')
 need(validation['Python']['executed']==956 and validation['Node_total']==916 and validation['strict_profile_MkDocs']==dict(strict=1,profile=1,docs=0),'Checkpoint validation census differs')
 need(validation['Release_full']==validation['Release_selected']=='NOT_RUN_OWNER_PREFERENCE','Release unit preference changed')
 compatibility=load(PREFIX+'compatibility-packet-v3.json');need(compatibility['all_exact'] and compatibility['new_original_queries']==0,'Frozen compatibility differs')
 prior_compat=load(OLD+'compatibility-packet-v1.json');oldresults={(r['configuration'],r['corpus']):r for r in prior_compat['results']}
 for result in compatibility['results']:
  before=oldresults[result['configuration'],result['corpus']]
  need(all(result[k]==before[k] for k in ['original','inputs','recorder','cases']),'Frozen original/input/recorder provenance changed')
  for k in ['original','inputs','recorder','current']:check(result[k])
  need(result['exact'] and (result['current']['bytes'],result['current']['sha256'])==(result['original']['bytes'],result['original']['sha256']) and result['command']['exit_code']==0,'Current frozen output differs')
 for cfg in ['Debug','Release']:
  need(sum(r['cases'] for r in compatibility['results'] if r['configuration']==cfg)==29424,'Frozen comparison count differs')
  need(sum(r['cases'] for r in compatibility['results'] if r['configuration']==cfg and r['corpus'] in list('ABCDEFG'))==458,'Late provenance count differs')
 from verify_c15_validation_v3 import inspect as validate
 results['validation']=validate(load,raw,PREFIX)
 readiness=load(PREFIX+'readiness-v1.json');need(readiness['W2']=='IN_PROGRESS' and readiness['W3']=='NOT_STARTED' and readiness['phase04A39']=='BLOCKED' and readiness['science']=='NOT_GOLDEN','Readiness promoted')
 return dict(status='NOT_READY_04A38',W2='IN_PROGRESS',W3='NOT_STARTED',historical_supported=537,historical_OPEN=167,current_supported=566,current_OPEN=160,support_supported=262,support_OPEN=41,compiled_graphs=graphs,whole_body_rechecks=results,Debug=dict(PASS=20990,FAIL=14,new_PASS=64),Python=validation['Python']['methods'],Node_PASS=916,strict_profile_MkDocs=validation['strict_profile_MkDocs'],predecessor=prior_result,original_queries=0,phase04A39='BLOCKED')

if __name__=='__main__':
 try:print(json.dumps(compute()))
 except (AssertionError,ValueError,OSError,KeyError,TypeError) as e:print(json.dumps(dict(status='NOT_READY_04A38',error=str(e))));raise SystemExit(1)
