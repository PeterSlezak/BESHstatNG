"""Compile the exact dormant native patch in an isolated source override; never apply it."""
import importlib.util
import argparse
from pathlib import Path
import shutil
import sys

ROOT=Path(__file__).resolve().parents[1]
spec=importlib.util.spec_from_file_location('v11',ROOT/'scripts/verify-04a11.py')
v=importlib.util.module_from_spec(spec);spec.loader.exec_module(v)
RUN=ROOT/'TestResults/04A-11-20261103-run01'
parser=argparse.ArgumentParser();parser.add_argument('--name',default='candidate-03');args=parser.parse_args()
work=RUN/'native-candidate-final'/args.name;work.mkdir(exist_ok=False)
target=work/'override.targets'
assert not target.exists()
text='''<Project xmlns="http://schemas.microsoft.com/developer/msbuild/2003">
  <ItemGroup Condition="'$(MSBuildProjectName)' == 'BESHStatNG'">
    <Compile Remove="src\\UI\\UiGLM.vb" />
    <Compile Include="'''+str(RUN/'native-candidate-final/UiGLM.vb')+'''" />
  </ItemGroup>
</Project>
'''
target.write_text(text,encoding='utf-8',newline='\n')
msbuild,_=v.base.tools()
(work/'bin').mkdir()
for project in ['BESHStat.Core','BESHStat.Excel.Common']:
    shutil.copyfile(ROOT/project/'bin/Debug/netstandard2.0'/(project+'.dll'),work/'bin'/(project+'.dll'))
name='native-'+args.name+'-compile'
code=v.base.command(RUN,name,[msbuild,'BESHstatNG/BESHStatNG.vbproj','/t:PrepareForBuild;PrepareResources;Compile;CopyFilesToOutputDirectory',
    '/p:Configuration=Debug','/p:BuildProjectReferences=false','/p:PythonExe='+sys.executable,
    '/p:CustomBeforeMicrosoftCommonTargets='+str(target),'/p:OutputPath='+str(work/'bin')+'\\',
    '/p:IntermediateOutputPath='+str(work/'obj')+'\\','/m','/v:normal'])
assert code==0,'Candidate compilation failed; inspect the new raw log'
log=(RUN/(name+'.log')).read_text(encoding='utf-8',errors='replace')
assert str(RUN/'native-candidate-final/UiGLM.vb') in log,'Candidate override was not compiled'
assert 'src\\UI\\UiGLM.vb ' not in log,'Original form unexpectedly compiled alongside candidate'
v.base.isolation.emit(ROOT/'evidence/04A11/native-candidate-validation-final.json',dict(status='PASS',native_executed=False,patch_applied=False,
    candidate_sha256=v.base.isolation.sha(RUN/'native-candidate-final/UiGLM.vb'),override_targets_sha256=v.base.isolation.sha(target),
    compile_exit_code=code,raw_log_sha256=v.base.isolation.sha(RUN/(name+'.log')),
    scope='Exact patched full UiGLM compiled via source override in fresh output/intermediate directories. Corrected alpha semantics execute in host-free Core/Common/adapter tests; no form constructor or native writer was executed.'))
