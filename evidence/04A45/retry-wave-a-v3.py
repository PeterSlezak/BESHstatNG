from common45 import *
lock();run=runner();label='a45-wave-a-v2';target=PRIVATE/'builds'/label/'Debug/legacy'
r=command('wave-a-v3-tests',[run.msbuild,'BESHStatNG.Test/BESHStatNG.Tests.vbproj','/t:Build','/p:Configuration=Debug','/p:BuildProjectReferences=false','/p:OutputPath='+str(target)+'\\','/p:IntermediateOutputPath='+str(PRIVATE/'wave-a-obj-v3')+'\\','/m:4','/v:minimal','/nr:false'])
assert not r['exit_code']
t=run.test('Debug',label,'Legacy','a45-wave-a-v3-focused','FullyQualifiedName~WindowsHarness45')
save(PRIVATE/'wave-a-v3-tests.json',t)
save('waves/a-v3.json',dict(build=r['exit_code'],outcomes=t['outcomes'],raw=pin(PRIVATE/'wave-a-v3-tests.json'),native='NOT_RUN'))
assert not t['outcomes'].get('Failed',0)
print('Wave A',t['outcomes'],flush=True)
