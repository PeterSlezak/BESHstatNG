"""Scoped preservation and source-transition checks; historical receipts are read-only."""
from entry import ROOT,HERE,BASE,git,pin,public,save,sha
from pathlib import Path
import json,csv,re
from integrate import transforms
WINDOWS=['BESHstatNG/src/Graphics/'+n+'.vb' for n in ['XYZscatter','ScatterPlotMatrix','3Dobjects']]
CORE=['BESHStat.Core/src/Graphics/'+n+'.vb' for n in ['XyzGeometryCore','XyzProjectionContext','ScatterMatrixSeriesCore','ScatterMatrixPanelData','WireObjectGeometryCore']]
TESTS=['BESHStat.Core.Tests/src/Graphics/'+n+'.vb' for n in ['Graphics33Tests','LegacyXyz33','LegacyScatter33','LegacyWire33']]+['BESHStatNG.Test/src/Charts/Graphics33AdapterTests.vb']
PROJECT='BESHStatNG.Test/BESHStatNG.Tests.vbproj'
OWNER_PLAN='planning/BESHStatNG_CrossPlatform_MasterPlan.md'
OWNER_PLAN_SHA='3b998b5f004a071d4ce797ee51903773616c42053e594ee419ddb12ea739eed7'
def load(p):return json.loads(Path(p).read_bytes())
def validate_transition(v):
 assert v['schema']=='04A33-source-transition-v1' and v['status']=='PASS'
 assert len(v['pins'])==278 and len({r['path'] for r in v['pins']})==278
 prior=load(ROOT/'evidence/04A32/source-transition-successor.json')
 assert v['prior']==pin('evidence/04A32/source-transition-successor.json')
 old={r['path']:r for r in prior['pins']}
 before={r['path']:r for r in load(HERE/'entry-pins.json')['files']}
 for row in v['pins']:
  p=row['path'];assert row['accepted_04A32_sha256']==old[p]['final_04A32_sha256']
  assert row['merged_main_sha256']==before[p]['sha256']
  assert row['final_04A33_sha256']==pin(p)['sha256']
  assert row['transition']==('AUTHORIZED_04A33' if p in WINDOWS else 'RETAINED_04A32_LINEAGE')
def validate_identity(source,returned):
 assert returned==list(range(source)),'Source ordinal reassignment'
def validate_gates(g):
 assert g==dict(P='NO',N1='NO GO',N2='NO',C2='UNMET',C3='UNAPPROVED',releases=0,discharges=0)
def protection():
 entry=load(HERE/'entry-pins.json')['files'];aliases={public(p):p for p in git('ls-files').splitlines()}
 changed=[]
 for r in entry:
  p=aliases[r['path']];assert (ROOT/p).is_file(),r['path'];now=pin(p)
  if now['bytes']!=r['bytes'] or now['sha256']!=r['sha256']:changed.append(dict(path=p,entry_sha256=r['sha256'],final_sha256=now['sha256']))
 assert {r['path'] for r in changed}==set(WINDOWS+[PROJECT,OWNER_PLAN]),changed
 assert pin(OWNER_PLAN)['sha256']==OWNER_PLAN_SHA,'External owner plan changed again; investigate before resealing'
 owner_change=[r for r in changed if r['path']==OWNER_PLAN]
 save('external-owner-change.json',dict(schema='04A33-external-owner-change-v1',status='PRESERVED_OUTSIDE_BATCH_PUBLICATION',change=owner_change,observation='Detected by all-tracked hash guard during validation. Codex never wrote this path. Revision2.11 to2.12 updates accepted-through32 status;official33 scope/gates retained. HEAD/main unchanged.',current=pin(OWNER_PLAN),source='External writer;no process attribution claimed',publication='Excluded from04A33 manifest;owner retains independent plan update'))
 changed=[r for r in changed if r['path']!=OWNER_PLAN]
 for p,text in transforms().items():assert (ROOT/p).read_bytes()==text.encode('utf-8'),p
 prior=load(ROOT/'evidence/04A32/source-transition-successor.json');before={r['path']:r for r in entry};rows=[]
 for r in prior['pins']:
  p=r['path'];merged=before[p]['sha256'];old=r['final_04A32_sha256']
  assert merged==old,(p,'Unexplained predecessor lineage delta')
  rows.append(dict(path=p,original_04A28_sha256=r['original_04A28_sha256'],accepted_04A32_sha256=old,merged_main_sha256=merged,final_04A33_sha256=pin(p)['sha256'],transition='AUTHORIZED_04A33' if p in WINDOWS else 'RETAINED_04A32_LINEAGE'))
 receipt=dict(schema='04A33-source-transition-v1',status='PASS',prior=pin('evidence/04A32/source-transition-successor.json'),pins=rows,new_transitions=[r for r in rows if r['transition']=='AUTHORIZED_04A33'],other_authorized_changes=[r for r in changed if r['path'] not in {x['path'] for x in rows}],new_production=[pin(p) for p in CORE],historical_failures='Original source-pin assertions retained unchanged; additive transitions do not make historical failures pass.')
 validate_transition(receipt);save('source-transition-successor.json',receipt)
 captures=[dict(path=public(p),matches=pin(p)['sha256']==v['sha256'],sha256=pin(p)['sha256']) for p,v in load(ROOT/'evidence/04A14/baseline-registry.json')['files'].items()]
 assert len(captures)==100 and all(x['matches'] for x in captures)
 save('source-preservation.json',dict(schema='04A33-source-preservation-v1',status='PASS_WITH_DOCUMENTED_EXTERNAL_OWNER_PLAN_UPDATE',tracked_entry=len(entry),unchanged=len(entry)-len(changed)-1,changed=changed,external_owner_changes=owner_change,all_35_other_windows_graphics_unchanged=True,all_23_existing_core_graphics_unchanged=True,all_previous_tests_evidence_and_controls_unchanged=True,all_100_captures_preserved=True,captures=captures,original_payload='NOT_REHASHED;separate membership/size/mtime receipt',whole_file_delta_guard='Exact deterministic extraction recipe from merged Git blobs;native tails and unmodified members protected additionally by Roslyn',native_releases=0,control_discharges=0))
 # Reconcile historical candidate IDs with current member/token ranges.
 old=list(csv.DictReader((HERE/'operation-map-entry.csv').open(encoding='utf-8-sig')));members=load(HERE/'source-ownership.json')['members'];final=[]
 for r in old:
  hits=[m for m in members if m['path']==r['path'] and m['name']==('New' if r['name']=='.ctor' else r['name'])]
  r=dict(r,final_sha256=pin(r['path'])['sha256'],current_member_ranges=';'.join(str(m['final_start'])+'-'+str(m['final_end']) for m in hits),current_owner='Core numeric projection;Windows renderer/state orchestration' if any(not m['body_preserved'] for m in hits) else 'Windows retained declaration/renderer/state;not indiscriminately migrated')
  final.append(r)
 with (HERE/'operation-map-final.csv').open('w',encoding='utf-8',newline='') as f:w=csv.DictWriter(f,fieldnames=list(final[0]));w.writeheader();w.writerows(final)
 save('source-pins.json',dict(schema='04A33-public-source-pins-v1',base=BASE,files=[dict(pin(p),entry_sha256=before.get(p,{}).get('sha256')) for p in WINDOWS+CORE+TESTS+[PROJECT]]))
 validate_gates(load(HERE/'entry-state.json')['gates'])
 print('PASS tracked preservation;278 lineage pins;3 authorized transitions;100 captures;exact source recipe')
if __name__=='__main__':protection()
