"""Seal current-entry observational fixtures before any test or product edits."""
from common39 import *
import shutil

source_lock()
source=HERE/'fixtures/Observe39v2.cs'
original=W3/'abi-cil/Debug-shipping-v1.json'
shipping=load(original)['assemblies']
folder=PRIVATE/'fixtures-before-v2';folder.mkdir(exist_ok=False)
for row in shipping:
    assert pin(row['path'])==row
    shutil.copy2(ROOT/row['path'],folder/Path(row['path']).name)
locator=Path(os.environ['ProgramFiles(x86)'])/'Microsoft Visual Studio/Installer/vswhere.exe'
csc=subprocess.check_output([str(locator),'-products','*','-version','[18.0,19.0)','-find','MSBuild/Current/Bin/Roslyn/csc.exe'],text=True).splitlines()[0]
refs=[folder/x for x in ['BESHStatNG.dll','BESHStat.Core.dll','BESHStat.Excel.Common.dll','ExcelDna.Integration.dll']]
framework=Path(os.environ['ProgramFiles(x86)'])/'Reference Assemblies/Microsoft/Framework/.NETFramework/v4.8'
refs += [framework/'System.Web.Extensions.dll',framework/'Facades/netstandard.dll']
executable=folder/'Observe39.exe'
receipt=command('fixture-before-compile-v2',[csc,'/nologo','/target:exe','/out:'+str(executable),*[('/r:'+str(p)) for p in refs],str(source)])
save('fixtures/before-compile-v2.json',receipt);assert receipt['exit_code']==0
receipt=command('fixture-before-execute-v2',[str(executable),str(HERE/'fixtures/before-v2.json')])
save('fixtures/before-execute-v2.json',receipt);assert receipt['exit_code']==0
data=load(HERE/'fixtures/before-v2.json');ids=[x['id'] for x in data['rows']];assert len(ids)==len(set(ids))
save('fixtures/frozen-input-manifest-v2.json',dict(kind='04A39_CURRENT_PRE_CHANGE',fixture=pin(HERE/'fixtures/before-v2.json'),
    driver=pin(source),driver_executable=pin(executable),shipping_authority=pin(original),assemblies=shipping,
    cases=[dict(id=x['id'],input_sha256=sha(json.dumps(x['input'],sort_keys=True,ensure_ascii=False).encode()),
                observation_sha256=sha(json.dumps(x,sort_keys=True,ensure_ascii=False).encode())) for x in data['rows']],
    culture_policy='Explicit Invariant/sk-SK thread culture, restored after execution; typed Double bits recorded.',
    reference_limit='Current accepted W3 shipping binary bound to accepted sources; historical original precision oracle REFERENCE_UNAVAILABLE/CANDIDATE_ONLY.',
    production_input_changed=False))
source_lock()
print('Sealed',len(ids),'pre-change cases',flush=True)
