from common45 import *
lock();run=runner();target=PRIVATE/'builds/a45-wave-a-v2/Debug/legacy'
r=command('wave-b-tests-compile-v1',[run.msbuild,'BESHStatNG.Test/BESHStatNG.Tests.vbproj','/t:Build','/p:Configuration=Debug','/p:BuildProjectReferences=false','/p:OutputPath='+str(target)+'\\','/p:IntermediateOutputPath='+str(PRIVATE/'wave-b-obj-v1')+'\\','/m:4','/v:minimal','/nr:false'])
assert not r['exit_code']
t=run.test('Debug','a45-wave-a-v2','Legacy','a45-wave-b-focused-v1','FullyQualifiedName~WindowsHarness45|FullyQualifiedName~MigrationRefEditBoundary|FullyQualifiedName~ExecutionAdapterTests|FullyQualifiedName~DocumentLifecycleAdapterTests')
save(PRIVATE/'wave-b-focused-v1.json',t)
save('waves/b-v1.json',dict(build=r['exit_code'],outcomes=t['outcomes'],raw=pin(PRIVATE/'wave-b-focused-v1.json'),native='NOT_RUN'))
assert not t['outcomes'].get('Failed',0)
print('Wave B',t['outcomes'],flush=True)
