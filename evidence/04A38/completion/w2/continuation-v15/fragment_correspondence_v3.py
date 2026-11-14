"""Actual fragment identities with exact aliases and verified whole-source compositions."""
from common import *
from verify_c14_composition_v2 import tokens,unwrap_cases
import re
load=lambda path:json.loads(path.read_bytes())
u=load(HERE/'final-v1/current-universe.json');o=load(W2.parent/'w1/original-universe.json')
contracts=load(HERE/'whole-body-contracts-v1.json')['contracts']+load(HERE/'callback-proofs-v1.json')['rows']
new={r['original_record']['symbol_id']:r for r in contracts if r['new_support']}
pc=load(HERE/'portable-api-contracts-v1.json')['contracts']
moves=load(HERE/'implementation-source-moves-v1.json')['moves']
textmove=load(HERE/'implementation-input-source-move-v1.json')
composition=load(HERE/'source-composition-proof-v2.json')
assert all(r['full_original_entry_current_reconciled'] for r in composition['rows'])

def canonical(text):
 text=unwrap_cases(text.replace('\r\n','\n'))
 for move in moves:
  for a in move['aliases']:text=text.replace(a['before'],a['after'])
 text=text.replace('ChrW(9)','Convert.ToChar(9)')
 text=text.replace('Global.BESHStatNG.WorksheetFunctions.ExcelArgReaders.AsString(arg)','arg')
 text=text.replace('Global.BESHStatNG.regression.GetCanonicalLinkFromDisplayName','regression.GetCanonicalLinkFromDisplayName')
 text=text.replace('New UdfReportProjection38(New Global.BESHStatNG.ResultTableOutputModel(out), UdfReportPreparation38.Raw)','out').replace('New UdfReportProjection38(New Global.BESHStatNG.ResultTableOutputModel(outNoHdr), UdfReportPreparation38.Raw)','outNoHdr')
 text=text.replace('UdfReportProjection38.FromFault(UdfReportFault38.NotAvailable)','ExcelError.ExcelErrorNA').replace('UdfReportProjection38.SafeNumber(','SafeExcelNumber(')
 text=re.sub(r'LegacyUdfText38.EqualText\(tp, ("(?:survival|cumhaz|hazard)")\)',r'tp = \1',text)
 return tokens(text.replace('ExcelError.ExcelErrorNA','unavailable'))

rows=[]
for f in o['fragments']:
 if f['member_id'] not in new:continue
 c=new[f['member_id']];pool=[v for v in u['fragments'] if v['member_id']==c['current_id']]
 reached=set(c['compiled'][0].get('transitive',{}).get('methods',[]))
 for p in pc:
  if p['compiled'][0]['complete_method']['method'] in reached:pool.extend(p['fragments'])
 variants=[('EXACT_FRAGMENT_UNDER_PROVED_PARENT_BINDINGS',f['code'])]
 reconstructed=f['code'].replace('\r\n','\n')
 used=[]
 for move in moves+[textmove]:
  if move['path']==f['path'] and move['original_segment'] in reconstructed:
   reconstructed=reconstructed.replace(move['original_segment'],move['native_call']);used.append(move.get('name','TryGetLooseNumericTextPrepared'))
 if used:variants.append(('EXACT_FRAGMENT_COMPOSED_FROM_PROVED_NATIVE_CALL_AND_FULL_PORTABLE_SEGMENTS',reconstructed))
 matches=[]
 for v in pool:
  if v['kind']!=f['kind']:continue
  for mode,code in variants:
   if canonical(code)==canonical(v['code']):
    matches.append(dict(current_fragment_id=v['fragment_id'],current_member_id=v['member_id'],mode=mode,segments=used if mode.startswith('EXACT_FRAGMENT_COMPOSED') else []));break
 rows.append(dict(fragment_id=f['fragment_id'],original_source_record=f,original_parent=c['original_id'],current_parent=c['current_id'],matches=matches,parent_composition='source-composition-proof-v2.json'))
full=load(HERE/'final-v1/dependency-full-common.json')
for c in load(HERE/'property-body-contracts-v1.json')['contracts']:
 owner=c['current_portable_owner'].split('`')[0]
 n=next(n for n in full['members'] if n['owner']==owner and n['name']==c['original_record']['name'] and n['kind']=='PropertyStatement')
 assert tokens(c['original_whole_body'].replace('GeeLsmEstimateProfileValue','TValue'))==tokens(n['signature'])
 for f in o['fragments']:
  if f['member_id']!=c['original_record']['symbol_id']:continue
  matches=[dict(current_fragment_id=v['fragment_id'],current_member_id=v['member_id'],mode='EXACT_CONSTRUCTED_GENERIC_PROPERTY_SOURCE_AND_GET_SET_CTOR_CONTEXT',segments=[]) for v in full['fragments'] if v['member_id']==n['symbol_id'] and v['kind']==f['kind'] and tokens(f['code'].replace('GeeLsmEstimateProfileValue','TValue'))==tokens(v['code'])]
  assert matches
  rows.append(dict(fragment_id=f['fragment_id'],original_source_record=f,original_parent=c['original_id'],current_parent=None,matches=matches,parent_composition='host-cache-contexts-v1.json'))
unmatched=[r for r in rows if not r['matches'] and (r['original_source_record']['is_branch'] or r['original_source_record']['is_inline'])]
save('new-parent-fragment-crosswalk-v3.json',dict(rows=rows,original_fragments=len(rows),matches=sum(bool(r['matches']) for r in rows),unmatched_branch_inline=unmatched,original_queries=0))
for r in unmatched:print(new[r['original_source_record']['member_id']]['current_record']['name'],r['original_source_record']['kind'],repr(r['original_source_record']['code'][:150]))
print('total',len(rows),'unmatched branch/inline',len(unmatched))

assert not unmatched, 'A real original branch/inline fragment lacks proof'
