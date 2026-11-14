"""Combined candidate source freeze before any validation campaign."""
from common import *
load=lambda p:json.loads(Path(p).read_bytes())
prior=load(PREVIOUS/'production-test-freeze-v3.json')
paths={r['path'] for r in prior['sources']}
paths.update(r['path'] for r in load(HERE/'development-source-snapshot-v5.json')['sources'])
for role in ['core','common']:
 for row in load(PREVIOUS/'final-v4'/('dependency-full-'+role+'.json'))['sources']:
  if role=='core':assert pin(ROOT/row['path'])==row
before={r['live']['path']:r['preimage'] for r in load(HERE/'entry-source-preimages-v1.json')['files']}
for row in load(HERE/'packet0-source-transitions-v1.json')['before']:
 before[row['path']]=row['preimage']
for row in load(HERE/'packet0-test-source-v1.json').get('before',[]):
 if isinstance(row,dict) and 'path' in row and 'preimage' in row:before.setdefault(row['path'],row['preimage'])
files=[];sources=[]
for path in sorted(paths):
 now=pin(ROOT/path);sources.append(now)
 prev=next((r for r in prior['sources'] if r['path']==path),None)
 snapshot=HERE/'source-after'/path;snapshot.parent.mkdir(parents=True,exist_ok=True)
 with snapshot.open('xb') as f:f.write((ROOT/path).read_bytes())
 files.append(dict(path=path,after=now,snapshot=pin(snapshot),prior_freeze=prev,preimage=before.get(path),changed_since_v16=prev!=now))
save('production-test-freeze-v1.json',dict(status='COMBINED_PRODUCTION_TEST_SOURCE_FREEZE',sources=sources,files=files,previous=pin(PREVIOUS/'production-test-freeze-v3.json'),Debug='REQUIRED',Release_compilation=DEFERRED,Release_full_selected_focused_tests=DEFERRED,Release_ABI_CIL_signatures_exports=DEFERRED,Release_frozen_compatibility=DEFERRED,original_queries=0))
script=(HERE/'build_development_Debug_v5.py').read_text(encoding='utf-8').replace('development-source-snapshot-v5.json','production-test-freeze-v1.json')
with (HERE/'build_final_Debug_v1.py').open('x',encoding='utf-8',newline='\n') as f:f.write(script)
inspect=(HERE/'inspect_candidate_Debug_v1.py').read_text(encoding='utf-8').replace('candidate-v1','final-v1').replace('candidate-','final-').replace('development-source-snapshot-v3.json','production-test-freeze-v1.json').replace('-v4','-v17-final1')
with (HERE/'inspect_final_Debug_v1.py').open('x',encoding='utf-8',newline='\n') as f:f.write(inspect)
print('Combined source freeze',len(sources),'files; Core unchanged172; Release deferred')
