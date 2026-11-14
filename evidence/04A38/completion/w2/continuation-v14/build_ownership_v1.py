"""Exact universes/complements after complete source, property, callback and state proof."""
from common import *
from proof_kernel_v8 import map_current,definitions,semantic
from source_views import view
draft='--draft' in sys.argv;out=PRIVATE/'draft-proof-v1' if draft else HERE
def load(name):return json.loads(((out/name) if (out/name).exists() else HERE/name).read_bytes())
def emit(name,value):
 with (out/name).open('x',encoding='utf-8',newline='\n') as f:json.dump(value,f,indent=2,ensure_ascii=False);f.write('\n')
old=json.loads((PREVIOUS/'current-ownership-v2.json').read_bytes());oh=json.loads((PREVIOUS/'historical-ownership-v2.json').read_bytes())['members'];u=load('current-universe-packet-v1.json');o=json.loads((W2.parent/'w1/original-universe.json').read_bytes());cs=load('whole-body-contracts-v1.json')['contracts'];props=load('property-body-contracts-v1.json')['contracts'];cb=load('callback-proofs-v1.json')['rows'];proof=load('source-composition-proof-v1.json')
assert len(json.loads((HERE/'managed-Debug-identities-v1.json').read_bytes())['new_tests'])==52 and json.loads((HERE/'managed-validation-v1.json').read_bytes())['Debug']['new_passes']==52
assert proof['new_current_supported']==35 and {c['current_id'] for c in cs+props if c['new_support']}<={r['current_id'] for r in proof['rows']}
mapping=map_current(old['members'],u['members']);reverse={n['symbol_id']:k for k,n in mapping.items()};oldm={n['symbol_id']:n for n in old['members']}
inherited={mapping[n['symbol_id']]['symbol_id'] for n in old['members'] if not n['review_status'].startswith('OPEN')};assert len(inherited)==494
assert all(mapping[n['symbol_id']]['body_sha256']==n['body_sha256'] for n in old['members'] if not n['review_status'].startswith('OPEN'))
eligible={c['current_id'] for c in cs+props}|{c['current_id'] for c in cb};closed=inherited|eligible;new={c['original_id']:c for c in cs+props if c['new_support']};assert len(new)==len(closed-inherited)==35
history=[];newrows=[]
for h in oh:
 row=dict(h,current_ids=[mapping[k]['symbol_id'] for k in h['current_ids']])
 if h['canonical_original_id'] in new:
  c=new[h['canonical_original_id']];assert not h['proof'];assert row['current_ids']==[c['current_id']]
  row.update(proof=dict(kind='C14_COMPLETE_WHOLE_BODY_SOURCE_COMPOSITE_PROPERTY_CIL_DEPENDENCY_CALLBACK_FRAGMENT_AND_CURRENT_DIAGNOSTIC_PROOF',evidence='property-body-contracts-v1.json' if c in props else 'whole-body-contracts-v1.json',original_id=h['canonical_original_id'],current_id=c['current_id'],behavior='source-composition-proof-v1.json',source_scope='source-transition-independent-proof-v1.json',compiled_owner='constructed-owner-proofs-v1.json'),disposition='FULLY_SUPPORTED_C14_COMPLETE_RECONCILED_BODY_CONTEXT')
  newrows.append(dict(original_id=h['canonical_original_id'],current_ids=row['current_ids'],name=h['name'],proof=row['proof']))
 history.append(row)
members=[dict(n,review_status='FULLY_SUPPORTED_C14_WHOLE_BODY_SOURCE_CIL_CONSTRUCTED_CONTEXT' if n['symbol_id'] in closed else oldm[reverse[n['symbol_id']]]['review_status']) for n in u['members']]
oldss={(s['path'],s['kind'],s['owner'],s['declaration']):s for s in old['support']};rawss={s['support_id']:s for s in u['supports']};supports=[];support_proof=[]
host=load('host-cache-contexts-v1.json');psm_class=host['source_support'];assert psm_class['support_id'] in rawss and host['current_whole_declaration'].replace('\r\n','\n')==host['original_whole_declaration'].replace('\r\n','\n') and len(host['properties'])==6
def key(s):return s['path'],s['kind'],s['owner'],s['declaration']
def complete(s):
 if oldss[key(s)]['fully_reconciled']:return True
 if s['support_id']==psm_class['support_id']:return True
 if s['kind']=='ModuleBlock' and s['declaration'] in ['Public Module PlotDataUDFs','Public Module PropensityScoreUDFs']:
  base=s['owner']+'.'+s['declaration'].split()[-1]
  children=[n for n in u['members'] if n['path']==s['path'] and (n['owner']==base or n['owner'].startswith(base+'.'))]
  nested=[x for x in u['supports'] if x['path']==s['path'] and x['support_id']!=s['support_id'] and s['span_start']<=x['span_start'] and x['span_start']+x['span_length']<=s['span_start']+s['span_length']]
  return all(n['symbol_id'] in closed for n in children) and all(complete(x) for x in nested)
 return False
snapshots={r['source']['path']:r['snapshot'] for r in json.loads((ROOT/'evidence/04A38/preedit/entry-source-snapshot.json').read_bytes())['files']}
for s in u['supports']:
 was=oldss[key(s)];done=complete(s);p=was.get('proof')
 if done and not was['fully_reconciled']:
  p=dict(kind='C14_COMPLETE_SOURCE_DECLARATION_ALL_CHILD_BODIES_STATE_AND_CIL',evidence='support-container-proofs-v1.json',support_id=s['support_id'])
  a=next(x for x in o['supports'] if key(x)==key(s));now,nv=view((ROOT/s['path']).read_bytes(),next(x['sha256'] for x in u['sources'] if x['path']==s['path']));before,bv=view((ROOT/snapshots[s['path']]['path']).read_bytes(),next(x['sha256'] for x in o['sources'] if x['path']==s['path']))
  span=lambda t,n:t.encode('utf-16-le')[n['span_start']*2:(n['span_start']+n['span_length'])*2].decode('utf-16-le')
  base=s['owner']+'.'+s['declaration'].split()[-1];children=[n for n in u['members'] if n['path']==s['path'] and (n['owner']==base or n['owner'].startswith(base+'.'))]
  support_proof.append(dict(current_source=s,original_source=a,current_whole_declaration=span(now,s),original_whole_declaration=span(before,a),current_view=nv,original_view=bv,original_snapshot=snapshots[s['path']],all_primary_children=children,blocking_primary_ids=[n['symbol_id'] for n in children if n['symbol_id'] not in closed],state_and_compiled_evidence='host-cache-contexts-v1.json' if 'Psm' in base or 'Propensity' in base else 'constructed-owner-proofs-v1.json'))
 supports.append(dict(s,fully_reconciled=done,proof=p,children=was.get('children',[]),distinct=not s['already_in_primary_population']))
new_support=[s['support_id'] for s in supports if s['fully_reconciled'] and not oldss[key(s)]['fully_reconciled']];assert len(new_support)==3
assert {s['declaration'] for s in supports if s['support_id'] in new_support}=={'Private Class PsmUdfHandle','Public Module PropensityScoreUDFs','Public Module PlotDataUDFs'}
multi='BESHStatNG.WorksheetFunctions.MultivariateAnalysisUDFs';psm='BESHStatNG.WorksheetFunctions.PropensityScoreUDFs';classes={(s['owner'],s['declaration']) for s in supports if s['kind']=='ClassBlock' and s['owner'] in [multi,psm] and s['fully_reconciled']};owners={owner+'.'+decl.split()[-1] for owner,decl in classes}
def headers(universe):
 found={};supported={key(s) for s in supports if s['fully_reconciled']}
 for s in universe['supports']:
  if key(s) not in supported:continue
  if s['kind']=='ModuleBlock':kinds=['ModuleStatement','EndModuleStatement']
  elif s['kind']=='ClassBlock' and (s['owner'],s['declaration']) in classes:kinds=['ClassStatement','EndClassStatement']
  elif s['kind']=='PropertyStatement' and s['owner'] in owners:kinds=['PropertyStatement']
  else:continue
  for f in universe['fragments']:
   if f['member_id'] is None and f['path']==s['path'] and f['kind'] in kinds and s['span_start']<=f['span_start'] and f['span_start']+f['span_length']<=s['span_start']+s['span_length']:found[f['fragment_id']]=s['support_id']
 return found
hc=headers(u);ho=headers(o);historicalclosed={h['git_original_id'] for h in history if h['proof']}
fragments=[dict(f,review_status='FULLY_SUPPORTED_SOURCE_AND_COMPILED_CONTEXT' if f['member_id'] in closed or f['fragment_id'] in hc else 'OPEN',proof_member=f['member_id'] if f['member_id'] in closed else hc.get(f['fragment_id'])) for f in u['fragments']]
originalfragments=[dict(f,covered=f['member_id'] in historicalclosed or f['fragment_id'] in ho,proof_member=f['member_id'] if f['member_id'] in historicalclosed else ho.get(f['fragment_id'])) for f in o['fragments']]
def count(rows,p):return dict(total=len(rows),covered=sum(p(r) for r in rows),branches=sum(r['is_branch'] for r in rows),branches_covered=sum(r['is_branch'] and p(r) for r in rows),inline=sum(r['is_inline'] for r in rows),inline_covered=sum(r['is_inline'] and p(r) for r in rows))
summary=dict(historical=dict(total=704,closed=sum(bool(h['proof']) for h in history),OPEN=sum(not h['proof'] for h in history)),current=dict(total=726,closed=len(closed),OPEN=726-len(closed)),support=dict(raw=len(supports),distinct=303,distinct_closed=sum(s['fully_reconciled'] for s in supports if s['distinct']),distinct_OPEN=sum(not s['fully_reconciled'] for s in supports if s['distinct'])),fragments=dict(current=count(fragments,lambda f:f['review_status']!='OPEN'),original=count(originalfragments,lambda f:f['covered'])))
assert (summary['historical']['closed'],summary['current']['closed'],summary['support']['distinct_closed'])==(496,529,256)
emit('historical-ownership-v2.json',dict(members=history,total=704));emit('current-ownership-v2.json',dict(members=members,support=supports,fragments=fragments));emit('ownership-summary-v2.json',summary);emit('support-container-proofs-v1.json',dict(rows=support_proof,original_queries=0));emit('support-fragment-crosswalk-v1.json',dict(current=hc,original=ho));emit('original-fragment-parent-crosswalk-v2.json',dict(rows=originalfragments,original_queries=0))
emit('new-closures-final-v1.json',dict(rows=newrows,new_support=new_support,historical_new=35,current_new=35,distinct_support_new=3,current_only=[],ROC_repaired=['ROC_POINTS','ROC_STATS','GuessPositiveClass','TryGetRocInput'],original_queries=0))
priorq=json.loads((PREVIOUS/'remaining-work-queue-v1.json').read_bytes());remaining=[r for r in priorq['members'] if r['original_id'] not in new]
emit('remaining-work-queue-v1.json',dict(members=remaining,first_members=remaining[:24],current_OPEN=[dict(current_id=n['symbol_id'],name=n['name'],path=n['path'],next='Complete whole-body original/current/portable/callback/fragment proof; no original-query authority.') for n in members if n['review_status'].startswith('OPEN')],support_OPEN=[s for s in supports if s['distinct'] and not s['fully_reconciled']],original_queries=0))
emit('readiness-v1.json',dict(status='NOT_READY_04A38',W2='IN_PROGRESS',W3='NOT_STARTED',P='NO',N1='NO GO',N2='NO',C2='UNMET',C3='UNAPPROVED',science='NOT_GOLDEN',phase04A39='BLOCKED',mandatory_blockers=dict(historical_OPEN=208,current_OPEN=197,distinct_support_OPEN=47),owner_acceptance='NOT_ACCEPTED_NOT_MERGED',original_queries=0))
print(json.dumps(summary))
