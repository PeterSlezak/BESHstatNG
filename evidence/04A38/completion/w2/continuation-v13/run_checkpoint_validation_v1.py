from common import *
for row in json.loads((HERE/'production-test-freeze-v1.json').read_bytes())['sources']:assert pin(ROOT/row['path'])==row
for label,script,args in [('packet-v1-Release-build-driver','build_v1.py',['packet-v1','Release']),
    ('packet-v1-inspection-driver','inspect_stage_v1.py',['packet-v1']),('packet-v1-metadata-driver','inspect_metadata_v5.py',['packet-v1']),
    ('packet-v1-compatibility-driver','compatibility_v1.py',['packet-v1']),('managed-full-Debug-driver-v1','validate_managed_v1.py',[]),
    ('managed-identity-reconciliation-v1','report_managed_v1.py',[]),('node-once-driver-v1','run_auxiliary_v1.py',['node']),
    ('strict-profile-docs-once-driver-v1','run_auxiliary_v1.py',['serial'])]:
    result=command(label,[sys.executable,'-B',str(HERE/script),*args]);assert result['exit_code']==0,'Preserve first unsuccessful step '+label
    print(label,'COMPLETE',flush=True)
for row in json.loads((HERE/'production-test-freeze-v1.json').read_bytes())['sources']:assert pin(ROOT/row['path'])==row
print('Dual-configuration compilation/ABI/compatibility, one full Debug/Node/strict/profile/docs campaign finished')
