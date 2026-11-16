from common45 import *
lock();wave=sys.argv[1];run=runner();label='a45-wave-'+wave+'-v1'
if wave=='a':
 # Seed portable dependencies from authenticated44 build, then compile only Windows/tests.
 import shutil
 old=ROOT/'.test-rationalization-04A44.tmp/builds/a44-wave-b-v3/Debug/legacy'
 target=PRIVATE/'builds'/label/'Debug/legacy';shutil.copytree(old,target)
 ms=run.msbuild
 common=['/p:Configuration=Debug','/p:BuildProjectReferences=false','/p:OutputPath='+str(target)+'\\','/m:4','/v:minimal','/nr:false']
 codes=[]
 for name,project,task in [('host','BESHstatNG/BESHstatNG.vbproj','PrepareForBuild;PrepareResources;Compile;CopyFilesToOutputDirectory'),('tests','BESHStatNG.Test/BESHStatNG.Tests.vbproj','Build')]:
  r=command('wave-a-'+name,[ms,project,'/t:'+task,*common,'/p:IntermediateOutputPath='+str(PRIVATE/'wave-a-obj'/name)+'\\']);codes.append(r['exit_code'])
  if r['exit_code']:break
 save(PRIVATE/'wave-a-build-v1.json',dict(exit_codes=codes))
 assert not any(codes),'WAVE_BUILD_FAILED'
else:
 r=run.build('Debug',label);save(PRIVATE/('wave-'+wave+'-build-v1.json'),r);assert not any(r['exit_codes'])
t=run.test('Debug',label,'Legacy',label+'-focused','FullyQualifiedName~WindowsHarness45')
save(PRIVATE/(label+'-tests.json'),t)
save('waves/'+wave+'-v1.json',dict(label=label,outcomes=t['outcomes'],raw=pin(PRIVATE/(label+'-tests.json')),native='NOT_RUN'))
assert not t['outcomes'].get('Failed',0),'FOCUSED_FAILURE'
print('Wave',wave,t['outcomes'],flush=True)
