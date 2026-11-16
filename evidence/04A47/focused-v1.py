from common47 import *
import shutil
lock();version=sys.argv[1];run=runner();label='a47-wave-'+version;dest=PRIVATE/'builds'/label/'Debug/legacy';shutil.copytree(PRIVATE/'entry-builds/Debug/lib',dest)
r=command('wave-'+version+'-compile',[run.msbuild,'BESHStatNG.Test/BESHStatNG.Tests.vbproj','/t:Build','/p:Configuration=Debug','/p:BuildProjectReferences=false','/p:OutputPath='+str(dest)+'\\','/p:IntermediateOutputPath='+str(PRIVATE/'wave-obj'/version)+'\\','/m:4','/v:minimal','/nr:false']);assert not r['exit_code'],'FOCUSED_COMPILE_FAILED'
t=run.test('Debug',label,'Legacy',label+'-tests','FullyQualifiedName~GuiWiring47')
save(PRIVATE/('wave-'+version+'-tests.json'),t);save('waves/'+version+'.json',dict(outcomes=t['outcomes'],raw=pin(PRIVATE/('wave-'+version+'-tests.json')),native='NOT_RUN'))
assert not t['outcomes'].get('Failed',0),'FOCUSED_TEST_FAILURE'
print(version,t['outcomes'],flush=True)
