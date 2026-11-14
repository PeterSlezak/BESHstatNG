"""Correct two new v17 fixture mistakes after the single full campaign ended."""
from common import *
import shutil
full=json.loads((HERE/'managed-full-execution-v1.json').read_bytes());assert full['outcomes']=={'Passed':21076,'Failed':16} and full['source_unchanged']
rows=[]
for path in ['BESHStat.Excel.Common.Tests/src/UdfContinuation17Tests.vb','BESHStatNG.Test/src/UDFs/UdfPreparation38AdapterTests.vb']:
 raw=(ROOT/path).read_bytes();snapshot=HERE/'failed-new-test-fixtures-v1'/path;snapshot.parent.mkdir(parents=True,exist_ok=True);snapshot.write_bytes(raw);s=raw.decode('utf-8-sig')
 if path.startswith('BESHStat.Excel.Common.Tests'):
  first=' & '.join('Char.ConvertFromUtf32(&H'+format(ord(c),'X')+')' for c in '\uff26\uff29\uff32\uff33\uff34')
  lr=' & '.join('Char.ConvertFromUtf32(&H'+format(ord(c),'X')+')' for c in '\uff2c\uff32')
  pairs=[('GetReferenceCategory("?????")','GetReferenceCategory('+first+')'),('ParseSurvivalWeightText("??")','ParseSurvivalWeightText('+lr+')')]
 else:pairs=[('Convert.ToString(ExcelError.ExcelErrorNA).Trim()','ExcelError.ExcelErrorNA.ToString()')]
 for a,b in pairs:assert s.count(a)==1,(path,a);s=s.replace(a,b,1)
 (ROOT/path).write_bytes((b'\xef\xbb\xbf' if raw.startswith(b'\xef\xbb\xbf') else b'')+s.encode('utf-8'));rows.append(dict(path=path,before=pin(snapshot),after=pin(ROOT/path),replacements=[dict(before=a,after=b) for a,b in pairs]))
freeze=json.loads((HERE/'production-test-freeze-v4.json').read_bytes());prior={r['path']:r for r in freeze['sources']}
for r in freeze['files']:
 r['after']=pin(ROOT/r['path']);p=HERE/'source-after-v5'/r['path'];p.parent.mkdir(parents=True,exist_ok=True);p.write_bytes((ROOT/r['path']).read_bytes());r['snapshot']=pin(p);r['changed_since_v16']=r['after']!=r['prior_freeze']
freeze['sources']=[r['after'] for r in freeze['files']];freeze['prior_candidate']=pin(HERE/'production-test-freeze-v4.json')
assert {r['path'] for r in freeze['sources'] if prior[r['path']]!=r}=={r['path'] for r in rows}
save('production-test-freeze-v5.json',freeze)
save('post-campaign-new-fixture-correction-v1.json',dict(corrections=rows,full_campaign=pin(HERE/'managed-full-execution-v1.json'),full_campaign_scope='GENUINE_COMPREHENSIVE_PRODUCTION_AND_TEST_FREEZE4;PRE_TWO_NEW_TEST_FIXTURE_CORRECTIONS;NOT_A_COMPREHENSIVE_FINAL_TEST_FREEZE5_RUN',production_unchanged=True,existing_tests_weakened=0,reason='New width fixture had question marks from PowerShell transport encoding, replaced by explicit Unicode code points. New enum expectation accidentally selected Convert.ToString(Integer), while the original Object adapter invokes Object.ToString. The original source/CIL prove the enum-name contract; use enum.ToString. Both genuine failed outcomes remain immutable.',additional_full_campaigns=0,Release=DEFERRED))
label='w2c17-new-fixture-correction-v1';run=RUNROOT/'builds'/label/'Debug';run.mkdir(parents=True,exist_ok=False)
for role in ['portable','legacy']:shutil.copytree(RUNROOT/'builds/w2c17-checkpoint-v1/Debug'/role,run/role)
runner=m.Runner(RUNROOT);commands=[]
for suite,project,tool,role in [('Common','BESHStat.Excel.Common.Tests/BESHStat.Excel.Common.Tests.vbproj','dotnet','portable'),('Legacy','BESHStatNG.Test/BESHStatNG.Tests.vbproj',runner.msbuild,'legacy')]:
 args=([tool,'msbuild'] if tool=='dotnet' else [tool])+[project,'/t:Build','/p:Configuration=Debug','/p:BuildProjectReferences=false','/p:ProduceReferenceAssembly=false','/p:OutputPath='+str(run/role)+'\\','/p:IntermediateOutputPath='+str(run/'obj'/suite)+'\\','/m:4','/v:minimal','/nr:false']
 result=command('new-fixture-correction-build-'+suite,args);commands.append(clean(result));assert result['exit_code']==0
results=[]
for suite,selection in [('Common','FullyQualifiedName~UdfContinuation17Tests'),('Legacy','FullyQualifiedName~Covariance17|FullyQualifiedName~Mmrm17NoInterceptRetainsOriginalArrayAliasAndNullPrecedence')]:
 result=runner.test('Debug',label,suite,label+'-'+suite,selection);results.append(clean(result))
for r in freeze['sources']:assert pin(ROOT/r['path'])==r
save('managed-new-fixture-focused-recheck-v1.json',dict(configuration='Debug',source_freeze=pin(HERE/'production-test-freeze-v5.json'),commands=commands,results=results,scope='ONLY39_NEW_V17_IDENTITIES;NO_SECOND_FULL_CAMPAIGN;PRODUCTION_IDENTICAL_TO_COMPREHENSIVE_FREEZE4',full_campaign=pin(HERE/'managed-full-execution-v1.json'),Release=DEFERRED))
assert all(not any(r['outcomes'].get(k,0) for k in ['Failed','NotExecuted']) for r in results)
print('New fixture focused Debug rechecks PASS',[(r['suite'],r['outcomes']) for r in results])
