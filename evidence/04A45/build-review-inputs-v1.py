from common45 import *
import collections,stat,re
lock()
for c in ['Debug','Release']:
 proof=load(HERE/('abi-cil/'+c+'-proof-v1.json'));assert proof['exports']==268 and not proof['changed_methods'] and not proof['unexpected']
 assert load(HERE/('compatibility/'+c+'-v1.json'))['cases']==29424
 assert load(HERE/('fixtures/'+c+'-44-v2.json'))['cases']==591
debug=load(HERE/'build-and-tests/Debug-identities-v1.json');release=load(HERE/'build-and-tests/Release-identities-v1.json')
assert debug['summary']['outcomes']==dict(Passed=21397,Failed=14) and len(debug['added'])==145
assert all(r['classification']=='RETAINED_IDENTICAL' for r in debug['nonpasses'])
assert release['summary']['outcomes']==dict(Passed=1230) and release['full_Release']=='NOT_RUN'
old=load(ROOT/'evidence/04A44/build-and-tests/Debug-identities-v1.json')
save('final/accepted44-test-identities-v1.json',dict(baseline=pin('evidence/04A44/build-and-tests/Debug-identities-v1.json'),tests=[{k:r[k] for k in ['suite','identity_sha256','occurrence','outcome']} for r in old['tests']],nonpasses=old['nonpasses'],historical_scope_FAIL=pin('evidence/04A44/register/issue-delta-v2.json'),historical_scope_status='FAIL_UNWAIVED'))
for p in ['BESHstatNG/BESHStatAddIn.vb','BESHstatNG/Resources/RibbonXML.xml','BESHstatNG/src/UI/UibyID.vb','BESHstatNG/src/UI/Refedit/RefEditEventLease.vb','BESHstatNG/src/UI/Refedit/Excel2007RefEdit.vb','BESHstatNG/src/AppInfrastructure/WindowsExecutionRun.vb']:
 assert pin(p)['sha256']==(load(HERE/'entry/source-preimages-v1.json')['source_pins'].get(p) or pin(p)['sha256'])
original=(PRIVATE/'source-preimages/BESHStatNG.Test/BESHStatNG.Tests.vbproj').read_bytes()
dest=HERE/'source/BESHStatNG.Tests.pre45.vbproj';dest.parent.mkdir(parents=True,exist_ok=True)
with dest.open('xb') as f:f.write(original)
patch=git('diff','--','BESHStatNG.Test/BESHStatNG.Tests.vbproj','planning/BESHStatNG_CrossPlatform_MasterPlan.md')
with (HERE/'source/review-v1.patch').open('xb') as f:f.write(patch)
save('final/software-verification-v1.json',dict(Debug=debug['summary'],Release=release['summary'],full_Release='NOT_RUN',compiled_proof=pin(HERE/'abi-cil/compiled-ui-and-build-attribution-v1.json'),ABI_exports_per_configuration=268,CIL_production_scope='All Core/Common/selected Windows ABI owners plus complete emitted UI/Ribbon/RefEdit metadata/IL inventory exact accepted44;all production source bytes exact main.',compatibility_per_configuration=dict(total=29424,PRE_EDIT_FROZEN=28966,LATE_ARCHIVED_ENTRY_SUPPLEMENT=458,new_original_queries=0),retained_current_cases=dict(batch39=77,batch40=48,batch41=390,batch42=1802,batch43_operations=1118,batch43_graphs=36,batch43_races=864,batch43_policy=15,batch44=591,batch44_fake_progress=72),strict_profile_MkDocs=load(HERE/'matrix/serial-checks-v1.json')['exits'],Node='INHERITED_916_PASS;NOT_RERUN',managed_Office_contracts='FRESH_FULL_DEBUG_PASS',live_Excel_COM_UIA_RefEdit='NOT_RUN',native_HWND_PID_XLL='NOT_VERIFIED',scientific='NOT_GOLDEN',new_harness_identities=145,prior_failed_attempts='Retained in waves and protected raw attempts;not promoted to PASS',gates=GATES))
print('Reviewer software summary and accepted44 identity comparison materialized',flush=True)
