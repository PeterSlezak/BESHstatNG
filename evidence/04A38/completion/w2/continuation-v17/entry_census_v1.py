"""Exact entire remaining W2 universe; no implementation or compatibility execution."""
from common import *
from collections import Counter
sys.path[:0]=[str(PREVIOUS),str(W2/'continuation-v15'),str(W2/'continuation-v14')]
from source_views import body
from proof_kernel_v16 import bind,definitions,closure,NESTED_CACHE_DECLARATIONS
load=lambda p:json.loads(Path(p).read_bytes())
q=load(PREVIOUS/'remaining-work-queue-v2.json');history=load(PREVIOUS/'historical-ownership-v2.json')['members'];current=load(PREVIOUS/'current-ownership-v1.json')
o=load(W2.parent/'w1/original-universe.json');u=load(PREVIOUS/'final-v4/current-universe.json')
assert len(history)==704 and sum(bool(r['proof']) for r in history)==607
assert len(current['members'])==726 and sum(not r['review_status'].startswith('OPEN') for r in current['members'])==635
assert len(current['support'])==325
distinct=[s for s in current['support'] if s['distinct']];assert len(distinct)==303 and sum(s['fully_reconciled'] for s in distinct)==271
assert [r['original_id'] for r in q['members']]==[r['canonical_original_id'] for r in history if not r['proof']] and len(q['members'])==97
assert [r['current_id'] for r in q['current_OPEN']]==[r['symbol_id'] for r in current['members'] if r['review_status'].startswith('OPEN')] and len(q['current_OPEN'])==91
assert [r['support_id'] for r in q['support_OPEN']]==[s['support_id'] for s in distinct if not s['fully_reconciled']] and len(q['support_OPEN'])==32
assert load(PREVIOUS/'ownership-summary-v2.json')['fragments']['original']['covered']==16465
assert len(u['fragments'])==13695 and sum(f['review_status']!='OPEN' for f in current['fragments'])==9973
for row in load(PREVIOUS/'production-test-freeze-v3.json')['sources']:assert pin(ROOT/row['path'])==row
for name in ['dependency-full-core.json','dependency-full-common.json','current-universe.json']:
 for row in load(PREVIOUS/'final-v4'/name)['sources']:assert pin(ROOT/row['path'])==row
hm={h['canonical_original_id']:h for h in history};cm={n['symbol_id']:n for n in u['members']};om={n['symbol_id']:n for n in o['members']}
snapshots={r['source']['path']:r['snapshot'] for r in load(ROOT/'evidence/04A38/preedit/entry-source-snapshot.json')['files']}
rows=[]
for ordinal,entry in enumerate(q['members'],1):
 h=hm[entry['original_id']];a=om[h['git_original_id']];text,policy=body((ROOT/snapshots[a['path']]['path']).read_bytes(),a)
 candidates=[]
 for key in h['current_ids']:
  n=cm[key];now,nv=body((ROOT/n['path']).read_bytes(),n)
  candidates.append(dict(record=n,source=pin(ROOT/n['path']),whole_body=now,source_view=nv,fragments=[f for f in u['fragments'] if f['member_id']==key]))
 rows.append(dict(ordinal=ordinal,original_id=h['canonical_original_id'],historical=h,original_record=a,original_source=snapshots[a['path']],original_whole_body=text,original_view=policy,original_fragments=[f for f in o['fragments'] if f['member_id']==a['symbol_id']],current_candidates=candidates,unresolved_counterpart=len(candidates)!=1,policy='Candidate counterparts are not fabricated bijections. Repeated lambdas must bind source-parent ordinal and actual compiled ldftn/delegate signatures before promotion. Original-only properties/lambdas require an authenticated constructed Common counterpart.',status='OPEN_ENTRY;NOT_A_CLOSURE'))
paths=sorted({r['original_record']['path'] for r in rows});preimages=[]
for p in paths:
 data=(ROOT/p).read_bytes();target=HERE/'source-before'/p;target.parent.mkdir(parents=True,exist_ok=True)
 with target.open('xb') as f:f.write(data)
 preimages.append(dict(live=pin(ROOT/p),preimage=pin(target)))
# Complete existing current and original metadata are retained without touching
# either original DLL. A separate passive current-Debug census checks shipping bytes.
meta=load(PREVIOUS/'final-v5/current-Debug.json');original=load(W2/'continuation-v14/metadata-all/packet-v1-context-v5/original-Debug.json')
for t in meta['types']:
 name=t['type'] or ''
 if '+' in name and '_Closure$' not in name:
  owner,child=name.split('+',1)
  if '+' not in child:NESTED_CACHE_DECLARATIONS.setdefault(owner,set()).add(child)
shapes={r['symbol_id']:r for r in load(PREVIOUS/'final-v4/declaration-shapes.json')['rows']}
roots=[];bindings=[]
for n in current['members']:
 if not n['review_status'].startswith('OPEN') or n['parent_member'] is not None or n['kind'] not in ['FunctionBlock','SubBlock','ConstructorBlock']:continue
 m=bind(n,meta,shapes);roots.append(m['method']);bindings.append(dict(source=n,compiled_Debug=m))
graph=closure(meta,roots);source_rows=[r['source'] for r in bindings]
supports=[s for s in current['support'] if s['distinct'] and not s['fully_reconciled']]
save('entry-queue-receipt-v1.json',dict(status='EXACT_97_HISTORICAL_91_CURRENT_32_DISTINCT_SUPPORT_OPEN;PRE_EDIT',rows=rows,full_ordered_OPEN_complements=q,source_preimages=preimages,current_Debug_bindings=bindings,compiled_dependency_graph=graph,compiled_methods=[m for m in meta['methods'] if m['method'] in set(graph['methods'])],support_contexts=supports,original_metadata=pin(W2/'continuation-v14/metadata-all/packet-v1-context-v5/original-Debug.json'),current_metadata=pin(PREVIOUS/'final-v5/current-Debug.json'),production_edits=0,original_queries=0,Release=DEFERRED))
save('entry-source-preimages-v1.json',dict(files=preimages,original_queries=0))
print('Whole remaining queue authenticated',len(rows),'historical;',len(q['current_OPEN']),'current;',len(supports),'support;',len(graph['methods']),'actual Debug dependencies',flush=True)
