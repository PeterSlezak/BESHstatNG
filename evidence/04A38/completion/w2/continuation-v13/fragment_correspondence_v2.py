"""Genuine original fragments, explicitly bound moved/native/portable callbacks."""
from common import *
from verify_c13_composition_v3 import inspect,tokens,unwrap,unwrap_cases
from source_views import body
proof=inspect();assert proof==json.loads((HERE/'source-composition-proof-v1.json').read_bytes())
cs=json.loads((HERE/'whole-body-contracts-v1.json').read_bytes())['contracts'];callbacks=json.loads((HERE/'callback-proofs-v1.json').read_bytes())['rows']
u=json.loads((HERE/'current-universe-packet-v1.json').read_bytes());o=json.loads((W2.parent/'w1/original-universe.json').read_bytes())
pc=json.loads((HERE/'portable-api-contracts-v1.json').read_bytes())['contracts'];ph=json.loads((HERE/'portable-historical-callback-proof-v1.json').read_bytes())
new={c['original_record']['symbol_id']:c for c in cs+callbacks if c['new_support'] and c['original_id']}
new[ph['original_record']['symbol_id']]=None
moves=json.loads((HERE/'implementation-source-composition-v2.json').read_bytes())['phases'];rows=[]
for f in o['fragments']:
 if f['member_id'] not in new:continue
 c=new[f['member_id']];bound=f['code'];pool=[]
 if c:
  pool=[v for v in u['fragments'] if v['member_id']==c['current_id']]
  if c['current_record']['parent_member'] is None:
   reached=set(c['compiled'][0]['transitive']['methods'])
   for p in pc:
    if p['compiled'][0]['method']['method'] in reached:pool.extend(p['fragments'])
  move=next((m for m in moves if ('Function '+c['current_record']['name']+'(') in m['parent']),None)
  if move:
   for a,b in move['bindings'].items():bound=bound.replace(a,b)
  name=c['current_record']['name']
  if name=='GetHistogramBinRule':bound=bound.replace('Global.BESHStatNG.WorksheetFunctions.ExcelArgReaders.AsString(arg)','arg')
  if name=='ParseRocDirection':bound=bound.replace('Global.BESHStatNG.WorksheetFunctions.ExcelArgReaders.AsString(direction)','direction')
  # Named option methods are unchanged portable APIs outside the limited report universe.
  if name in ['GetHistogramBinRule','ParseRocDirection']:
   full=json.loads((HERE/'dependency-full-common-v1.json').read_bytes());p=next(p for p in full['members'] if p['owner']=='BESHStatNG.ExcelCommon.UdfDataImportOptions38' and p['name']==name)
   pool.extend(v for v in full['fragments'] if v['member_id']==p['symbol_id'])
 else:
  bound=bound.replace('NormalizeOptionKey(x)','UdfDataImportOptions38.NormalizeOptionKey(token)').replace('Function(x)','Function(token)')
  pu=json.loads((HERE/'portable-universe-v1.json').read_bytes());pool=[v for v in pu['fragments'] if v['member_id']==ph['current_source_record']['symbol_id']]
 def canon(text):
  import re
  return tokens(re.sub(r'LegacyUdfText38.EqualText\(token, ("[^"]+")\)',r'token = \1',unwrap_cases(unwrap(text))))
 matches=[v['fragment_id'] for v in pool if v['kind']==f['kind'] and canon(v['code']) in [canon(f['code']),canon(bound)]]
 rows.append(dict(fragment_id=f['fragment_id'],original_source_record=f,original_parent=c['original_id'] if c else ph['original_id'],current_parent=c['current_id'] if c else None,
  portable_parent=None if c else ph['current_source_record']['symbol_id'],bound_source=bound,matched_current_fragments=matches,
  proof='EXACT_GENUINE_FRAGMENT_UNDER_COMPILED_ARGUMENT_BINDINGS' if matches else 'EXPLICIT_WHOLE_BODY_COMPOSITE',dynamic_coverage='NOT_INFERRED'))
unmatched=[r for r in rows if not r['matched_current_fragments'] and (r['original_source_record']['is_branch'] or r['original_source_record']['is_inline'])]
save('new-parent-fragment-crosswalk-v2.json',dict(rows=rows,unmatched_branch_inline=unmatched,original_queries=0))
for r in unmatched:print(r['original_source_record']['kind'],r['original_source_record']['code'])
print('fragments',len(rows),'unmatched branch/inline',len(unmatched))
assert not unmatched,'Preserve actual unmatched evidence; resolve only with true source/compiled binding'
