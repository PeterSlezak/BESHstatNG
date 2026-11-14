"""Record genuine C10 preimages and the complete authenticated target entry bodies."""
from common import *
assert json.loads((HERE/'entry-v1.json').read_bytes())['status']=='LIVE_C9_AUTHENTICATED_BEFORE_C10_IMPLEMENTATION'
old=['BESHstatNG/src/ExcelUDFs/ContingencyTableUDFs.vb','BESHstatNG/src/ExcelUDFs/MultivariateAnalysisUDFs.vb','BESHStat.Excel.Common/src/Analysis/UdfContingencyPreparation38.vb','BESHStatNG.Test/src/UDFs/UdfPreparation38AdapterTests.vb']
new=['BESHStat.Excel.Common/src/Results/UdfMultivariateReportPreparation38.vb','BESHStat.Excel.Common.Tests/src/UdfContinuation10Tests.vb']
rows=[]
for path in old:
    target=HERE/'source-before'/(path+'.txt');target.parent.mkdir(parents=True,exist_ok=True)
    with target.open('xb') as f:f.write((ROOT/path).read_bytes())
    rows.append(dict(live=pin(ROOT/path),preimage=pin(target)))
for path in new:assert not (ROOT/path).exists()
save('implementation-plan-v1.json',dict(status='AUTHENTICATED_PREIMAGES_BEFORE_C10_IMPLEMENTATION',source_paths=old+new,source_before=rows,new_sources=[dict(path=p,entry_state='ABSENT') for p in new],entry=pin(HERE/'entry-v1.json'),scope='Eleven complete typed Contingency calculation/report phases and four PCA/FA report phases. Original Core engines, host conversions/acquisition/defaults, caches/handles, errors and exported ABI remain. FA fitting stays unchanged because its ordered typed Core delegation is already portable; source/CIL proof preferred to a facade. FA eigen header parsing stays lazy through a named native delegate and a ByRef Object slot; no extra boxed Object copies are allowed.',original_queries=0))
for name in ['source_views.py','MethodMetadata-v11.cs','native-metadata-owners-v3.txt','proof_kernel_v1.py','ReturnMetadata-v1.cs','DeclarationShapes-v1.cs']:
    with (HERE/name).open('xb') as f:f.write((PREVIOUS/name).read_bytes())
from source_views import body
u=json.loads((PREVIOUS/'current-universe-packet-v1.json').read_bytes());h=json.loads((PREVIOUS/'historical-ownership-v2.json').read_bytes())['members'];original=json.loads((W2.parent/'w1/original-universe.json').read_bytes());om={n['symbol_id']:n for n in original['members']};current={n['symbol_id']:n for n in u['members']};by={n['canonical_original_id']:n for n in h};snap={r['source']['path']:r['snapshot'] for r in json.loads((ROOT/'evidence/04A38/preedit/entry-source-snapshot.json').read_bytes())['files']};targets=[]
for queued in json.loads((PREVIOUS/'remaining-work-queue-v1.json').read_bytes())['first_members']:
    history=by[queued['original_id']];n=current[history['current_ids'][0]];o=om[history['git_original_id']];before,view=body((ROOT/n['path']).read_bytes(),n);authentic,original_view=body((ROOT/snap[o['path']]['path']).read_bytes(),o)
    targets.append(dict(original_id=queued['original_id'],name=n['name'],current_record=n,original_record=o,current_source=pin(ROOT/n['path']),original_source=snap[o['path']],current_whole_body=before,original_whole_body=authentic,current_view=view,original_view=original_view,complete_LF_identity=before.replace('\r\n','\n')==authentic.replace('\r\n','\n')))
save('target-entry-bodies-v1.json',dict(rows=targets,original_queries=0,scope='All24 exact machine-queued OPEN historical identities; complete authenticated source text/UTF16 span/body SHA. Passive original source reading only.'))
save('tooling-carryforward-v1.json',dict(files=[dict(before=pin(PREVIOUS/n),after=pin(HERE/n)) for n in ['source_views.py','MethodMetadata-v11.cs','native-metadata-owners-v3.txt','proof_kernel_v1.py','ReturnMetadata-v1.cs','DeclarationShapes-v1.cs']],original_queries=0))
print('C10 genuine source preimages, absent-new states and all24 authentic entry bodies recorded')
