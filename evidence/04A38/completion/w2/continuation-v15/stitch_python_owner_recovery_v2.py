"""Reconcile durable916 plus owner-approved40; retain all interrupted evidence."""
from common import *
from collections import Counter
import re
draft=PRIVATE/'final-drafts';load=lambda p:json.loads(p.read_bytes())
interrupted=load(draft/'python-interruption-recovery-plan-v1.json')
authority=load(draft/'owner-replacement-python-lane-authorization-v1.json')
assert authority['owner_message']=='I approve one replacement 40-parent scanner lane'
execution=load(draft/'owner-replacement-python-lane-execution-v1.json');assert execution['status']=='OWNER_REPLACEMENT_40_PARENT_LANE_COMPLETE'
inventory=load(HERE/'python-discovery-inventory-v1.json')['entries'];receipts=[];allrecords=[]
for old in interrupted['completed_executions']:
 report=load(ROOT/old['raw']['path']);folder=(ROOT/old['command']['path']).parent;commanddoc=load(folder/'command.json')
 receipts.append(dict(lane=old['lane'],phase=old['phase'],raw=old['raw'],command=clean(commanddoc),command_folder=folder.relative_to(ROOT).as_posix(),copy_provenance=old['copy_provenance'],attempt='ORIGINAL_COMPLETE_EXECUTION_NOT_REPEATED'))
 allrecords.extend(report['records'])
assert len(allrecords)==916
folder=RUNROOT/'commands/w2c15-python-worker-v2-1-owner-replacement';report=load(ROOT/execution['raw']['path'])
focus=load(draft/'owner-focused-privacy-execution-v1.json');focused=load(ROOT/focus['raw']['path']);selected={r['id'] for r in focused['records']};assert len(selected)==2 and focused['outcomes']==dict(ERROR=2)
retained=[r for r in report['records'] if r['id'] not in selected];assert len(retained)==38
receipts.append(dict(lane=1,phase='MAIN',raw=execution['raw'],command=clean(load(folder/'command.json')),command_folder=folder.relative_to(ROOT).as_posix(),copy_provenance=None,attempt='OWNER_AUTHORIZED_REPLACEMENT_LANE_ONCE',effective_ids=[r['id'] for r in retained]))
focus_folder=RUNROOT/'commands/w2c15-python-privacy-focused-v1'
receipts.append(dict(lane=1,phase='FOCUSED_PRIVACY',raw=focus['raw'],command=clean(load(focus_folder/'command.json')),command_folder=focus_folder.relative_to(ROOT).as_posix(),copy_provenance=None,attempt='OWNER_AUTHORIZED_TWO_PRIVACY_CORRECTIONS_ONCE',effective_ids=sorted(selected)))
allrecords.extend(retained+focused['records']);assert len(allrecords)==956 and Counter(r['id'] for r in allrecords)==Counter(r['id'] for r in inventory)
stderr=(RUNROOT/'commands/w2c15-python-worker-v1-1-main/stderr.log').read_text(encoding='utf-8-sig')
started=re.findall(r'^test_\S+ \(([^\n()]+)\) \.\.\. ',stderr,re.M)
assert len(started)==len(set(started))==14 and set(started)<={r['id'] for r in inventory if r['lane']==1}
printed=interrupted['partial_lane_printed_outcomes'];assert len(printed)==13
current={r['id']:r for r in report['records']};comparisons=[]
for row in printed:
 identity=row['display_name'].rsplit('(',1)[1][:-1];outcome='PASS' if row['outcome']=='ok' else row['outcome']
 assert current[identity]['outcome']==outcome,'A printed partial outcome changed; retain and investigate before sealing'
 comparisons.append(dict(identity=identity,original_printed_outcome=outcome,replacement_outcome=current[identity]['outcome'],original_message='NOT_EMITTED_BEFORE_INTERRUPTION' if outcome!='PASS' else 'NO_NONPASS_REPORTED'))
save('owner-replacement-python-lane-authorization-v1.json',authority)
save('owner-focused-privacy-authorization-v1.json',load(draft/'owner-focused-privacy-authorization-v1.json'))
save('python-interruption-recovery-v1.json',dict(clean(interrupted),partial_started_ids=started,partial_started_count=14,partial_completed_headers=13,partial_inflight_unknown=1,printed_outcome_reconciliation=comparisons,authorization=pin(HERE/'owner-replacement-python-lane-authorization-v1.json'),replacement_execution=clean(execution),focused_authority=pin(HERE/'owner-focused-privacy-authorization-v1.json'),focused_execution=clean(focus),focused_ids=sorted(selected),effective_complete_unique_parents=956,known_total_parent_starts=972,known_completed_parent_attempts=971,interrupted_unknown_outcomes=1,complete_original916_not_repeated=True,retained_other954_not_repeated_after_tool_correction=True,raw_interruption_preserved=True,source_assertions_changed=0,tool_defect='Helper UnicodeDecodeError at protected C14 binary snapshot was translated into two parent AssertionError FAILs; those raw FAILs remain preserved. Corrected focused runs reach unchanged sequential scanner and report actual UnicodeDecodeError ERRORs. Prior C14 malformed-JSON ERROR chronology remains unchanged.'))
times=[load(ROOT/r['raw']['path']) for r in receipts if r['lane']!=1]
replacement=load(ROOT/execution['raw']['path']);assert all(r['end_utc']<replacement['start_utc'] for r in times)
schedule=dict(other_lanes_completed_before_scanner=True,max_non_scanner_workers=5,max_scanner_helpers=5,max_scanner_parent_plus_helpers=6,original_interrupted_then_owner_replacement=True,original_complete_reports=6,replacement_complete_reports=1,other_parent_identities_repeated=0,original916_not_repeated=True)
save('python-worker-receipts-v1.json',dict(workers=6,receipts=receipts,source_unchanged=True,schedule=schedule,owner_exception=pin(HERE/'owner-replacement-python-lane-authorization-v1.json'),raw_transcripts='All six original complete reports retained; interrupted command/stdout/stderr/helper jobs retained separately. One approved replacement lane with unchanged test executor/assertions provides40 complete raw records.'))
totals=Counter(r['outcome'] for r in allrecords)
save('python-execution-summary-v1.json',dict(status='EXACT_EFFECTIVE956_WITH_EXPLICIT_OWNER_INTERRUPTION_AND_TWO_FOCUS_EXCEPTIONS',workers_at_peak=6,outcomes=dict(totals),executed=956,known_raw_parent_starts=972,corrective_repeated_parent_starts=16,owner_replacement_lanes=1,owner_focused_privacy_parents=2,missing=[],duplicated_effective_ids=[],skipped=[],source_unchanged=True))
save('python-frozen-input-postcheck-v1.json',dict(status='EXACT_FROZEN_INPUTS_UNCHANGED',checked=execution['frozen_inputs_checked'],inventory=pin(HERE/'python-scan-input-freeze-v2.json'),after_interruption=pin(draft/'python-frozen-post-interruption-v1.json'),after_replacement=pin(draft/'owner-replacement-python-lane-execution-v1.json'),scope='Every preexisting frozen public input was rehashed after interruption and before/after the approved replacement. Later bookkeeping is newly created and receives separate publication policy/hash checks.',original_queries=0))
print('Recovered956 complete unique parents',dict(totals),'with972 known starts and explicit owner exceptions')
