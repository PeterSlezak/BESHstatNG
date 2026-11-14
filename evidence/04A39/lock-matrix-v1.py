"""Predeclare campaigns and exact accepted authorities before managed execution."""
from common39 import *
source_lock()
rows=[]
for config in ['Debug','Release']:
    for category,mode in [('build','build'),('managed','full' if config=='Debug' else 'focused')]:
        rows.append(dict(configuration=config,category=category,campaign_id='a39v1-'+config+'-'+mode,
            driver=['managed-v1.py',config,mode],command_authority=pin('scripts/test_suite_rationalization.py'),
            execution='SCHEDULED_FRESH',parallelism='MSBuild4;managed test hosts serial;7 disjoint Debug partitions',
            expected='All21094 inherited occurrences retained plus new39 tests; Release selected only, no full-suite claim.'))
    for category in ['abi-cil-exports','new-current-fixtures']:
        rows.append(dict(configuration=config,category=category,execution='SCHEDULED_FRESH',expected_exports=268))
    rows.append(dict(configuration=config,category='sealed29424-compatibility',execution='RETAINED_W3_SOURCE_BOUND_READBACK',
        expected_pre_edit=28966,expected_late=458,total=29424,
        reason='No production source, UDF input semantics or ownership implementation changed. All production hashes remain exact39 entry preimages; compiled metadata and new fixtures must independently confirm equality. Fresh current-only comparator becomes required if this proof fails.'))
rows += [dict(category='Python',execution='SCHEDULED_ONE_FRESH_FINAL_CAMPAIGN',expected_parents=956,
    expected_outcomes=dict(PASS=830,FAIL=113,ERROR=13),expected_events=dict(FAIL=185,ERROR=13),
    worker_limit=3,scanner_workers=1,helpers=0,authority=pin(W3/'python-node-scanners/campaign-v1/python_workers_v4.py')),
    dict(category='Node',execution='SCHEDULED_FRESH',expected=916),dict(category='strict-profile-MkDocs',execution='SCHEDULED_FRESH_SERIAL',previous_exits=[1,1,0]),
    dict(category='native-scientific-Excel-browser-R-install-sign-release',execution='NOT_RUN_UNAUTHORIZED')]
save('matrix/verification-matrix-v1.json',dict(status='LOCKED_BEFORE_MANAGED_CAMPAIGNS',utc=utc(),rows=rows,
    changes=['BESHStat.Excel.Common.Tests/src/UdfInputPolicy39Tests.vb','BESHStatNG.Test/src/UDFs/UdfInputBoundary39Tests.vb',
             'BESHStatNG.Test/BESHStatNG.Tests.vbproj','planning/BESHStatNG_CrossPlatform_MasterPlan.md',
             'planning/BESHStatNG_CrossPlatform_MasterPlan2.14.md','evidence/04A39/**'],production_files_changed=0,
    Release_full_suite_decision='Not required for test-only/audit-only changes with exact per-configuration shipping CIL and268exports; targeted policies plus fixtures run. Escalate on concrete discrepancy.',
    wrapper_policy='Original verify-beshstatng.ps1 and-Legacy write shared historical TestResults/Codex and implicitly restore. Do not launch; authenticated offline isolated equivalent build/test graph is used, exactly as W3. No wrapper PASS claimed.',
    driver_prechange_stop='Observe39v1 reflected helpers through moved Core DataObj assembly,47TypeLoad errors; retained invalid diagnostics. Version2 selects actual Windows adapter assembly. No product/test change in that correction.',
    register_prelaunch_stop='v1 misread the218-control transferred subset as global802. v2 uses authoritative continuation28 triage802 and asserts subset containment. No source/control reclassification.'))
print('Matrix locked',flush=True)
