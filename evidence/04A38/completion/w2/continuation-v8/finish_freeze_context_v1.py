"""Pin tool/runtime/private history and prepared final-boundary source inputs."""
from common import *
import importlib.util,collections
commands=json.loads((PREVIOUS/'command-receipts-v1.json').read_bytes())['commands'];retained=[]
for row in commands:
    for key in ['command_receipt','raw_stdout','raw_stderr']:
        assert pin(ROOT/row[key]['path'])==row[key];retained.append(row[key])
publication=json.loads((W2/'continuation-v7-publication-receipt-v1.json').read_bytes())
for number in [1,2]:
    folder=RUNROOT/'commands'/('w2c7-publication-copied-reader-v'+str(number));raw=json.loads((folder/'command.json').read_bytes())
    for suffix in ['stdout','stderr']:
        data=(folder/(suffix+'.log')).read_bytes();assert sha(data)==raw[suffix+'_sha256'];retained.append(pin(folder/(suffix+'.log')))
    if number==2:
        assert raw['exit_code']==0 and all(raw[k]==publication['copied_executable'][k] for k in ['exit_code','stdout_sha256','stderr_sha256'])
        assert json.loads((folder/'stdout.log').read_bytes())==publication['copied_result']
save('v7-private-command-preservation-v1.json',dict(commands=len(commands),streams=2*len(commands)+4,all_exact=True,pins=retained,C7_receipt=pin(PREVIOUS/'command-receipts-v1.json'),copy_attempts='First long-path failure and corrected short-copy success preserved',original_queries=0))
exe=library_pin(Path(sys.executable));old_exes=[a for row in commands for a in row['direct_file_arguments'] if a['name']=='python.exe'];assert old_exes and all(a==exe for a in old_exes)
runner=m.Runner(RUNROOT);tool_files=[Path(sys.executable),Path(runner.msbuild),Path(runner.vstest),Path(compiler('csc.exe')),Path(compiler('vbc.exe'))]
python_dll=Path(sys.executable).parent/'python314.dll'
if python_dll.is_file():tool_files.append(python_dll)
save('toolchain-inputs-v1.json',dict(owner_Python=exe,Python=sys.version,tools=[library_pin(p) for p in tool_files],C7_python_executable_byte_identity=True,managed_runner=pin(ROOT/'scripts/test_suite_rationalization.py'),test_settings=pin(ROOT/'test.runsettings'),production_projects=[pin(ROOT/p) for p in ['BESHStat.Core/BESHStat.Core.vbproj','BESHStat.Excel.Common/BESHStat.Excel.Common.vbproj','BESHstatNG/BESHstatNG.vbproj','BESHStatNG.Test/BESHStatNG.Tests.vbproj']],source_freeze=pin(HERE/'source-freeze-managed-v1.json'),restore='NOT_RUN',install='NOT_RUN',original_queries=0))
source=(HERE/'verify_c8_checkpoint_template_v5.py').read_text(encoding='utf-8')
with (HERE/'verify_c8_checkpoint_template_v6.py').open('x',encoding='utf-8',newline='\n') as f:f.write(source)
save('publication-runtime-carryforward-v1.json',dict(Python=exe,version=sys.version,C7_same_executable_inputs=len(old_exes),old_policy=pin(W2/'continuation-v6/publication_policy.py'),new_policy=pin(HERE/'publication_privacy_v1.py'),scope='Same owner Python executable and unchanged policy source, with path/content keyed old decisions. Every actual new payload hash and CRC remains fresh; new/changed privacy evaluation is explicit. No independent scientific truth follows.',original_queries=0))
save('source-static-freeze-v1.json',dict(sources=[pin(ROOT/p) for p in json.loads((HERE/'implementation-plan-v1.json').read_bytes())['source_paths']],current_census=pin(HERE/'current-universe-packet-v1.json'),portable_census=pin(HERE/'portable-universe-v1.json'),source_contracts=pin(HERE/'whole-body-contracts-v1.json'),callback_contracts=pin(HERE/'callback-proofs-v2.json'),historical=pin(HERE/'historical-ownership-v2.json'),current=pin(HERE/'current-ownership-v2.json'),frozen=pin(HERE/'compatibility-packet-v1.json'),managed=pin(HERE/'managed-validation-v1.json'),original_queries=0))
print('Source/tool/runtime and all prior command/copy raw streams authenticated')
