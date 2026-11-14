"""Read exact protected pins and retained identities; no broad historical-tree walk."""
from witness_lib import *
import collections

verify_frozen();assert load(HERE/'exact-25-fixtures.json')['count']==25
oldc=legacy()
oldpres=imported('old39_preservation_bound',OLD/'preservation.py')
original=oldpres.verify_checkpoint();assert len(original)==468
records={};authorities=[]
for path,field in [(ROOT/'evidence/04A39/preservation/new-pins-v1.json','files'),(ROOT/'evidence/04A39/preservation/post-publication-additions-v1.json','added'),
    (ROOT/'.test-rationalization-04A39-correction-v1.tmp/private-pins-after-postseal.json','files')]:
    rows=load(path)[field];authorities.append(dict(authority=pin(path),rows=len(rows),bytes=sum(r['bytes'] for r in rows)))
    for row in rows:
        if row['path'] in records:assert all(records[row['path']][k]==row[k] for k in ('bytes','sha256'))
        records[row['path']]=row
for n,row in enumerate(records.values()):
    assert pin(row['path'])=={k:row[k] for k in ('path','bytes','sha256')},'PROTECTED_PIN_CHANGED'
    if 'device' in row:
        stat=(ROOT/row['path']).stat();assert stat.st_dev==row['device'] and stat.st_ino==row['file_id'],'PROTECTED_FILE_ID_CHANGED'
    if (n+1)%2000==0:print('Protected exact-path pins',n+1,'verified',flush=True)
python=load(ROOT/'evidence/04A39/python-node-scanners/python-identities-v1.json')
baseline=load(ROOT/'evidence/04A38/completion/w3/python-node-scanners/python-identities-v1.json')
for doc,key in [(python,'parts'),(baseline,'executions')]:
    for row in doc[key]:assert pin(row['raw']['path'])==row['raw']
reader=imported('old39_register_verifier',ROOT/'evidence/04A39/verify-register-v5.py')
reg=load(ROOT/'evidence/04A39/register/defects-v7.json');counts=reader.verify(ROOT,reg)
assert (counts['canonical_entries'],counts['original_obligations'],counts['restricted_controls'])==(1169,1514,802)
assert len(reg['crosswalk'])==2967 and len(reg['current_source_pins'])==33
for row in reg['current_source_pins']:assert pin(row['path'])==row
mapped=load(ROOT/'evidence/04A39/map/method-policy-map-v1.json');assert mapped['focus_members']==len(mapped['methods'])==192
localreader=imported('old39_exact_diagnostic_reader',OLD/'verify_correction_v2.py')
old_verdict=localreader.local();assert old_verdict['status']=='BLOCKED_04A39_OWNER_DECISION_REQUIRED'
events=load(OLD/'diagnostics/changed-54-crosswalk-v2.json')['changes']
assert len(events)==len({r['stable_event_id'] for r in events})==54
parents=load(OLD/'diagnostics/parent-956-reconciliation.json');assert len(parents['parents'])==956
assert parents['original39_outcomes']==dict(PASS=833,FAIL=112,ERROR=11)
privacy=load(OLD/'privacy/focused-finding-reconciliation.json')
assert privacy['per_parent_findings']==1941 and privacy['original_snapshot_retained']==1599 and privacy['new_since_snapshot']==342
compat=[];exports=[]
for config in ('Debug','Release'):
    path=ROOT/('evidence/04A39/compatibility/'+config+'-retained-v1.json');doc=load(path)
    assert len(doc['case_identities'])==29424 and len({(r['corpus'],r['case_id']) for r in doc['case_identities']})==29424
    assert collections.Counter(r['provenance'] for r in doc['case_identities'])==dict(PRE_EDIT_FROZEN=28966,LATE_ARCHIVED_ENTRY_SUPPLEMENT=458)
    assert len(doc['results'])==12 and sum(r['cases'] for r in doc['results'])==29424 and all(r['exact'] for r in doc['results'])
    assert doc['original_queries']==doc['current_queries_04A39']==0
    compat.append(dict(configuration=config,receipt=pin(path),cases=29424,preedit=28966,supplement=458,corpora=12,mode='RETAINED_W3_NOT_FRESH'))
    path=ROOT/('evidence/04A39/abi-cil/'+config+'/exports-v1.json');doc=load(path)
    assert len(doc['exports'])==len({r['effective_excel_name'] for r in doc['exports']})==268
    exports.append(dict(configuration=config,receipt=pin(path),signatures=268,mode='RETAINED_NOT_FRESH_COMPILED_RUN'))
verify_frozen()
save('preservation-reconciliation.json',dict(status='EXACT_LISTED_PRESERVATION_AND_RETAINED_IDENTITY_READBACK_PASS',original_NOT_READY_files=468,source_test_plan_pins=1353,
    old_correction_files_unchanged=len(load(HERE/'entry.json')['old_correction_files']),protected_unique_paths=len(records),protected_bytes=sum(r['bytes'] for r in records.values()),authorities=authorities,
    raw_python_authorities=14,register=counts,register_crosswalk_rows=2967,current_register_source_pins=33,register_statuses_aliases_unchanged=True,policy_members=192,
    canonical_register=pin(ROOT/'evidence/04A39/register/defects-v7.json'),Python_parents=956,Python_original39=dict(PASS=833,FAIL=112,ERROR=11),Python_W3=dict(PASS=830,FAIL=113,ERROR=13),
    changed_diagnostic_FAIL_events=54,changed_diagnostics='SOURCE_EXPLAINED_FAILS_NOT_WAIVED',privacy=dict(W3='TWO_ERROR_PARENTS_UNWAIVED',current='TWO_FAIL_PARENTS_UNWAIVED',each=1941,snapshot_each=1599,added_each=342),
    compatibility=compat,exports=exports,retained_campaigns=load(OLD/'validation/retained-evidence-readback.json')['retained_facts'],
    inherited_W3_union=dict(files=37484,archive_declarations=154,scope='INHERITED_QUALIFIED;NOT_FRESH_FULL_UNION'),
    no_TestResults_recursive_search=True,original_reference_executions=0,full_campaigns_rerun=0,gates=GATES))
print('Preservation PASS:468 original dossier,1353 sources;',len(records),'protected paths;1169/1514/802 register,2967 crosswalk,33 pins;29424/config and268/config retained',flush=True)
