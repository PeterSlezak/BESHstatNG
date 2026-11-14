"""Freeze executable W3 campaign recipes before launching any test identities."""
from common import *

lock()
runner=m.Runner(RUNROOT)
PRIVATE.mkdir(exist_ok=False)
rows=[]
suites=[('Core','all',None),('Common','main','FullyQualifiedName!~MigrationSemantic04A02Tests'),
        ('Common','semantic','FullyQualifiedName~MigrationSemantic04A02Tests'),('Legacy','all',None),
        ('ControlMitigation','all',None),('OfficeAddin','all',None),('OfficeExecution','all',None)]
for config in ['Debug','Release']:
    for category in ['compilation','managed-six-suites','ABI-CIL-signatures-exports-closure','frozen-current-only-compatibility']:
        rows.append(dict(configuration=config,category=category,status='SCHEDULED_FRESH_W3_EXECUTION',
            campaign_id='w3v1-'+config,expected_cases=29424 if category=='frozen-current-only-compatibility' else None,
            expected_managed_identities=21094 if config=='Debug' and category=='managed-six-suites' else None,
            Release_identity_policy='Publish separately discovered actual multiset; compare Debug multiset without assuming equality',
            expected_exports=268 if category=='ABI-CIL-signatures-exports-closure' else None))
save('matrix/verification-matrix-v1.json',dict(status='LOCKED_BEFORE_EXECUTION',rows=rows,
    source_freeze=pin(ACCEPTED/'production-test-freeze-v1.json'),entry=pin(HERE/'entry/entry-manifest-v2.json'),
    managed=dict(suites=[dict(suite=s,part=p,filter=f,execution_name='w3'+c[0]+'-'+s+('-'+p if s=='Common' else ''))
                        for c in ['Debug','Release'] for s,p,f in suites],
                 runner=pin(ROOT/'scripts/test_suite_rationalization.py'),parallelism=1,inner_hosts=1,
                 source_policy='No source/test repair or normalization; one complete execution per configuration; halt unexplained new nonpass'),
    compatibility=dict(corpora=load(ACCEPTED/'compatibility-replacement-v1.json')['results'],
        original_Release_authority='Pre-existing sealed Release original outputs in corresponding 12 corpora; no original execution',
        PRE_EDIT_FROZEN=28966,LATE_ARCHIVED_ENTRY_SUPPLEMENT=458,parallelism=4,
        configurations=['Debug','Release'],command_authority=pin(ACCEPTED/'compatibility_Debug_v3.py')),
    build=dict(projects=m.PROJECTS,msbuild=runner.msbuild,vstest=runner.vstest,
        command_recipe=pin(ROOT/'scripts/test_suite_rationalization.py'),restore=False,
        host_targets='PrepareForBuild;PrepareResources;Compile;CopyFilesToOutputDirectory',
        source_unchanged_required=True),
    python=dict(status='DECISION_PENDING_EXACT_INPUT_ENVIRONMENT_CROSSWALK',expected_parents=956,
        expected_parent_outcomes=dict(PASS=830,FAIL=113,ERROR=13),expected_nonpass_events=dict(FAIL=185,ERROR=13),
        execution_limit=8,ordinary_worker_limit=3,scanner_workers=1,full_campaigns_per_source=1),
    node=dict(status='SCHEDULED_FRESH_W3_EXECUTION',expected_identities=916,parallelism=1,
        commands=[dict(cwd=cwd,args=args) for cwd,args in [('BESHStat.OfficeAddin',['node','--test','--test-concurrency=1','tests/*.test.mjs']),
             ('.',['node','--test','--test-concurrency=1','BESHStatNG_Help_MkDocs/tests/help-resolver.test.mjs']),
             ('BESHStat.WasmSmoke',['node','--test','--test-concurrency=1'])]]),
    serial=dict(command_recipe='Runner.serial: strict/profile/MkDocs, once after build and tests',
         command_authority=pin(ROOT/'scripts/test_suite_rationalization.py'),expected_exits=dict(strict=1,profile=1,docs=0)),
    wrapper=dict(commands=['powershell -NoProfile -ExecutionPolicy Bypass -File scripts/verify-beshstatng.ps1',
                           'powershell -NoProfile -ExecutionPolicy Bypass -File scripts/verify-beshstatng.ps1 -Legacy'],
         status='NOT_RUN',reason='Implicit restore and shared historical TestResults writers; isolated offline equivalents used'),
    restricted='NOT_RUN_UNAUTHORIZED: native Excel/COM/UIA/RefEdit, scientific/R, Office/Mac/browser/manual acceptance, installers/XLL/signing/deploy/original execution/04A39'))
save('matrix/tool-fingerprints-v1.json',dict(tools=[pin(Path(p)) for p in [runner.msbuild,runner.vstest] if Path(p).is_relative_to(ROOT)],
     external_tools=[dict(path=p,bytes=Path(p).stat().st_size,sha256=sha(Path(p).read_bytes()))
                     for p in [runner.msbuild,runner.vstest,compiler('vbc.exe'),compiler('csc.exe')]],
     compiler_arguments='Exact frozen runner plus compatibility recorder recipes; no added profiles/timeouts/package versions'))
print('W3 matrix locked; no test identities executed',flush=True)
