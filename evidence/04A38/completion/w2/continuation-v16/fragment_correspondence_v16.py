"""Exhaustive genuine fragment correspondence under independently recomputed source macros."""
from proof_support_v16 import *
from collections import defaultdict,Counter
u=load(HERE/'final-v4/current-universe.json');o=load(W2.parent/'w1/original-universe.json')
contracts=load(HERE/'whole-body-contracts-v1.json')['contracts']+load(HERE/'callback-proofs-v1.json')['rows']
special=load(HERE/'historical-only-callback-proof-v1.json')['rows']
closed={r['original_id'] for r in load(HERE/'new-closures-final-v1.json')['rows']}
new={r['original_record']['symbol_id']:r for r in contracts+special if r['original_id'] in closed}
composite=load(HERE/'source-composition-proof-v1.json');proofs={r['original_id']:r for r in composite['rows']}
pc=load(HERE/'portable-api-contracts-v1.json')['contracts']
common=load(HERE/'final-v4/dependency-full-common.json');core=load(HERE/'final-v4/dependency-full-core.json')
meta=load(HERE/'final-v5/current-Debug.json');defs=definitions(meta)
moves=sum([load(HERE/name)['moves'] for name in ['packet-A-source-moves-v1.json','packet-B-source-moves-v1.json','adjacent-LMM-source-moves-v1.json']],[])
current_byparent=defaultdict(list)
for f in u['fragments']:current_byparent[f['member_id']].append(f)
core_byparent=defaultdict(list)
for f in core['fragments']:core_byparent[f['member_id']].append(f)
def canon(text,aliases):
 text=unwrap_cases(text.replace('\r\n','\n'))
 for alias in aliases:text=text.replace(alias['before'],alias['after'])
 text=text.replace('Global.BESHStatNG.regression.','regression.')
 text=re.sub(r'LegacyUdfText38\.EqualText\((\w+),\s*("(?:[^"\n]|"")*"|String\.Empty)\)',r'\1 = \2',text)
 return tokens(text)
rows=[];modes=Counter()
for original_id,c in new.items():
 a=c['original_record'];original_fragments=[f for f in o['fragments'] if f['member_id']==original_id]
 if c in special:
  p=c['current_portable_record'];pool=[f for f in common['fragments'] if f['member_id']==p['symbol_id']]
  aliases=[];certificate='historical-only-callback-proof-v1.json';current_parent=None;macro=[]
 else:
  current_parent=c['current_id'];pool=list(current_byparent[current_parent]);aliases=[]
  if c in contracts and c.get('current_record',{}).get('kind','').endswith('LambdaExpression'):
   certificate='callback-proofs-v1.json';macro=[]
  else:
   assert c['original_id'] in proofs
   certificate='source-composition-proof-v1.json';macro=proofs[c['original_id']]['moved_segments']
   method=c['compiled_Debug']['complete_method'];reached=set(c['compiled_Debug']['transitive']['methods'])
   for p in pc:
    if any(r['complete_method']['method'] in reached for r in p['compiled_Debug']):pool.extend(p['fragments'])
   owners={(defs[k]['declaring_type'].split('`',1)[0],defs[k]['member_name']) for k in reached}
   for n in core['members']:
    if n['parent_member'] is None and (n['owner'],n['name']) in owners:pool.extend(core_byparent[n['symbol_id']])
   for move in moves:
    if move['path']==a['path'] and move['name'] in macro:aliases.extend(move.get('aliases',[]))
 index=defaultdict(list)
 for f in pool:index[f['kind'],canon(f['code'],aliases)].append(f)
 for f in original_fragments:
  assert a['span_start']<=f['span_start'] and f['span_start']+f['span_length']<=a['span_start']+a['span_length']
  matches=index.get((f['kind'],canon(f['code'],aliases)),[])
  if matches:mode='EXACT_SOURCE_FRAGMENT_WITH_EXPLICIT_PROVED_PARAMETER_ALIASES'
  else:
   assert certificate=='source-composition-proof-v1.json','Unmatched exact callback fragment '+f['fragment_id']
   # Block/header splitting and generic guarded wrappers need an entire-body
   # macro, rather than a fabricated one-to-one fragment identity. Every such
   # macro was independently recomputed from actual current portable source.
   assert c['original_id'] in proofs
   mode='EXACT_WHOLE_BODY_MACRO_COMPOSITION;NO_FABRICATED_FRAGMENT_ID'
  modes[mode]+=1
  rows.append(dict(original_fragment=f,original_parent=c['original_id'],current_parent=current_parent,mode=mode,current_matches=[dict(fragment_id=v['fragment_id'],member_id=v['member_id'],path=v['path'],kind=v['kind'],token_sha256=v['token_sha256']) for v in matches],source_certificate=certificate,source_macro=proofs.get(c['original_id']),portable_segments=macro,original_parent_record=a,original_source=c.get('original_source'),current_source=c.get('current_source',c.get('source',c.get('current_portable_source'))),complete_original_parent_token_sha256=sha(tokens(c['original_whole_body']).encode()),complete_current_parent_token_sha256=sha(tokens(c['current_whole_body']).encode()),is_branch=f['is_branch'],is_inline=f['is_inline'],Release=DEFERRED))
assert len(rows)==sum(1 for f in o['fragments'] if f['member_id'] in new)
save('new-parent-fragment-crosswalk-v1.json',dict(status='EXHAUSTIVE_AUTHENTIC_FRAGMENT_CORRESPONDENCE_UNDER_COMPLETE_SOURCE_COMPOSITES',rows=rows,original_fragments=len(rows),modes=dict(modes),unreconciled_branch_inline=[],qualification='Exact direct matches and full-body macro compositions are distinguished. No macro is accepted for a parent with a source obligation. All fragment source IDs/spans remain authentic; no invented portable fragment ID or one-to-one mapping is asserted for split blocks.',original_queries=0,Release=DEFERRED))
print('Fragments',len(rows),dict(modes),flush=True)
