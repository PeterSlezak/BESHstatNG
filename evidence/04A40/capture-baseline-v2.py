from common40_v3 import *
import shutil
lock()
folder=PRIVATE/'fixtures-before-v2';folder.mkdir(exist_ok=False)
shipping=PRIVATE/'builds/a40-entry-v1/Debug/legacy'
for name in ['BESHStatNG.dll','BESHStat.Core.dll','BESHStat.Excel.Common.dll','ExcelDna.Integration.dll']:shutil.copy2(shipping/name,folder/name)
vswhere=Path(os.environ['ProgramFiles(x86)'])/'Microsoft Visual Studio/Installer/vswhere.exe'
csc=subprocess.check_output([str(vswhere),'-products','*','-version','[18.0,19.0)','-find','MSBuild/Current/Bin/Roslyn/csc.exe'],text=True).splitlines()[0]
framework=Path(os.environ['ProgramFiles(x86)'])/'Reference Assemblies/Microsoft/Framework/.NETFramework/v4.8'
refs=[folder/p for p in ['BESHStatNG.dll','BESHStat.Core.dll','BESHStat.Excel.Common.dll','ExcelDna.Integration.dll']]+[framework/'System.Web.Extensions.dll',framework/'Facades/netstandard.dll']
executable=folder/'Observe40.exe'
record=command('baseline-fixture-compile-v2',[csc,'/nologo','/target:exe','/out:'+str(executable),*[('/r:'+str(p)) for p in refs],str(HERE/'fixtures/Observe40v2.cs')])
save(PRIVATE/'baseline-fixture-compile-v2.json',record);assert record['exit_code']==0
record=command('baseline-fixture-execute-v2',[str(executable),str(PRIVATE/'before-v2.json'),'legacy'])
save(PRIVATE/'baseline-fixture-execute-v2.json',record);assert record['exit_code']==0
data=load(PRIVATE/'before-v2.json');ids=[r['id'] for r in data['rows']];assert len(ids)==len(set(ids))
save('baseline/frozen-fixtures-v2.json',dict(kind='04A40_CURRENT_PRE_CHANGE',source_lock=pin(HERE/'entry/source-preimages-v3.json'),driver=pin(HERE/'fixtures/Observe40v2.cs'),
    assemblies=[pin(p) for p in folder.glob('*.dll')],raw=pin(PRIVATE/'before-v2.json'),cases=[dict(id=r['id'],input_sha256=sha(json.dumps(r['input'],sort_keys=True).encode()),outcome=r['outcome'],output_sha256=sha(json.dumps(r,sort_keys=True).encode())) for r in data['rows']],
    original_reference='NOT_HISTORICAL_ORIGINAL;NO_NEW_ORIGINAL_EXECUTION',scientific='NOT_GOLDEN',native='NOT_RUN_UNAUTHORIZED'))
print('Frozen',len(ids),'materialized current-main observations',flush=True)
