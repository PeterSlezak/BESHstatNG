"""Combined A+B freeze, complete source composition and immutable Core/reference pins."""
from common import *
sys.path.append(str(W2/'continuation-v14'))
from verify_c14_composition_v2 import tokens
plan=json.loads((HERE/'implementation-plan-v2.json').read_bytes())
preimages={r['live']['path']:r for r in json.loads((HERE/'all-edited-source-preimages-v1.json').read_bytes())['files']}
preimages.update({r['live']['path']:r for r in plan['source_before']})
moves=sum([json.loads((HERE/('packet-'+p+'-source-moves-v1.json')).read_bytes())['moves'] for p in ['A','B']],[])
moves+=json.loads((HERE/'adjacent-LMM-source-moves-v1.json').read_bytes())['moves']
reconstructed=[]
for path in sorted({r['path'] for r in moves}):
 text=(ROOT/preimages[path]['preimage']['path']).read_text(encoding='utf-8-sig')
 for row in [r for r in moves if r['path']==path]:
  assert text.count(row['original_segment'])==1,(path,row['name'])
  text=text.replace(row['original_segment'],row['native_call'],1)
 assert tokens(text)==tokens((ROOT/path).read_text(encoding='utf-8-sig')),'UNRECONCILED_COMPLETE_NATIVE_FILE '+path
 reconstructed.append(dict(path=path,before=preimages[path]['live'],preimage=preimages[path]['preimage'],after=pin(ROOT/path)))
for path,row in preimages.items():
 if path not in plan['source_paths']:assert pin(ROOT/path)==row['live'],'UNAUTHORIZED_INHERITED_SOURCE_CHANGE '+path
core=json.loads((PREVIOUS/'final-v1/dependency-full-core.json').read_bytes())['sources']
for row in core:assert pin(ROOT/row['path'])==row
save('no-Core-source-formula-drift-v3.json',dict(status='ALL172_CORE_SOURCE_FILES_EXACTLY_MATCH_AUTHENTICATED_V15',sources=core,count=len(core),previous_census=pin(PREVIOUS/'final-v1/dependency-full-core.json'),Core_edits=0,original_queries=0))
transitions=[]
for path in plan['source_paths']:
 target=HERE/'source-after/freeze-v3'/(path+'.txt');target.parent.mkdir(parents=True,exist_ok=True)
 with target.open('xb') as f:f.write((ROOT/path).read_bytes())
 before=preimages.get(path)
 transitions.append(dict(path=path,before=before['live'] if before else None,preimage=before['preimage'] if before else None,after=pin(ROOT/path),snapshot=pin(target),entry_state='EXISTING' if before else 'ABSENT'))
save('source-transitions-freeze-v3.json',dict(files=transitions,whole_native_file_composites=reconstructed,original_queries=0))
paths=sorted(set(preimages)|set(plan['source_paths']))
save('production-test-freeze-v3.json',dict(status='COMBINED_TWO_PACKET_SOURCE_FREEZE_BEFORE_COMPREHENSIVE_CAMPAIGNS',sources=[pin(ROOT/p) for p in paths],changed_sources=plan['source_paths'],transitions=pin(HERE/'source-transitions-freeze-v3.json'),owner_override=pin(HERE/'owner-debug-only-override-v1.json'),Core_sources=pin(HERE/'no-Core-source-formula-drift-v3.json'),Debug_comprehensive_campaigns_executed=0,Python_campaigns_executed=0,Node_campaigns_executed=0,Release_compilation=DEFERRED,Release_unit_suites=DEFERRED,Release_ABI_CIL_exports=DEFERRED,Release_frozen_compatibility=DEFERRED,original_queries=0))
print('Combined source freeze',len(paths),'sources',len(moves),'exact source movements,172 Core files unchanged',flush=True)
