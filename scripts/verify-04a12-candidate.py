"""Compile the exact pending GEE family guard in isolated outputs; never apply/run it."""
import argparse
import importlib.util
from pathlib import Path
import shutil
import sys

ROOT = Path(__file__).resolve().parents[1]
spec = importlib.util.spec_from_file_location('v12', ROOT / 'scripts/verify-04a12.py')
v = importlib.util.module_from_spec(spec)
spec.loader.exec_module(v)
spec = importlib.util.spec_from_file_location('c12', ROOT / 'scripts/capture-04a12-contract.py')
c = importlib.util.module_from_spec(spec)
spec.loader.exec_module(c)
parser = argparse.ArgumentParser()
parser.add_argument('--name', default='candidate-01')
args = parser.parse_args()
work = c.RUN / 'native-candidate' / args.name
work.mkdir(exist_ok=False)
target = work / 'override.targets'
target.write_text('''<Project xmlns="http://schemas.microsoft.com/developer/msbuild/2003">
  <ItemGroup Condition="'$(MSBuildProjectName)' == 'BESHStatNG'">
    <Compile Remove="src\\UI\\Ui13GEE.vb" />
    <Compile Include="''' + str(c.RUN / 'native-candidate/Ui13GEE.vb') + '''" />
  </ItemGroup>
</Project>
''', encoding='utf-8', newline='\n')
(work / 'bin').mkdir()
for project in ['BESHStat.Core', 'BESHStat.Excel.Common']:
    shutil.copyfile(ROOT / project / 'bin/Debug/netstandard2.0' / (project + '.dll'), work / 'bin' / (project + '.dll'))
msbuild, _ = v.base.tools()
name = 'native-' + args.name + '-compile'
code = v.base.command(c.RUN, name, [msbuild, 'BESHstatNG/BESHStatNG.vbproj',
    '/t:PrepareForBuild;PrepareResources;Compile;CopyFilesToOutputDirectory', '/p:Configuration=Debug',
    '/p:BuildProjectReferences=false', '/p:PythonExe=' + sys.executable,
    '/p:CustomBeforeMicrosoftCommonTargets=' + str(target), '/p:OutputPath=' + str(work / 'bin') + '\\',
    '/p:IntermediateOutputPath=' + str(work / 'obj') + '\\', '/m', '/v:normal'])
assert code == 0
log = (c.RUN / (name + '.log')).read_text(encoding='utf-8', errors='replace')
assert str(c.RUN / 'native-candidate/Ui13GEE.vb') in log
assert 'src\\UI\\Ui13GEE.vb ' not in log
c.emit('native-candidate-validation.json', dict(status='PASS', native_executed=False, patch_applied=False,
    candidate_sha256=c.sha(c.RUN / 'native-candidate/Ui13GEE.vb'), override_targets_sha256=c.sha(target),
    compile_exit=code, raw_log_sha256=c.sha(c.RUN / (name + '.log')),
    scope='Exact patched full form compiled in fresh output/intermediate directories. No form constructor or native writer executed. Simulated family-switch semantics execute in host-free adapter tests.'))
