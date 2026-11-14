"""Run corresponding current cases only after the fixed original output seal."""
from common import *
from verify_inputs import inspect
from compile_harness import compile_library
SEAL='edcad8369d6d3db5c9bdb7c61518a01951059b8fbc7ff2f3326738c858ab28da'
def main():
 config=sys.argv[1];assert config in ['Debug','Release'];inspect()
 assert sha((HERE/'original-output-seal.json').read_bytes())==SEAL
 seal=json.loads((HERE/'original-output-seal.json').read_bytes());assert seal['current_executions']==0
 for row in seal['files']:assert pin(ROOT/row['path'])==row
 known={r['name']:r for r in json.loads((V2/('current-execution-'+config+'.json')).read_bytes())['assemblies']}
 build=RUNROOT/'builds/validation-v2'/config/'legacy'
 for name in ['BESHStatNG.dll','BESHStat.Core.dll','BESHStat.Excel.Common.dll','ExcelDna.Integration.dll']:assert library_pin(build/name)==known[name]
 folder=compile_library(config,'current','trace');exe=folder/'MmrmCurrentCapture38.exe'
 facade=Path('C:/Program Files (x86)/Reference Assemblies/Microsoft/Framework/.NETFramework/v4.8/Facades/netstandard.dll')
 compiled=command('current-'+config+'-recorder',[compiler('csc.exe'),'/nologo','/target:exe','/reference:System.Core.dll','/reference:System.Web.Extensions.dll',
      '/reference:'+str(folder/'BESHStat.Core.dll'),'/reference:'+str(facade),'/out:'+str(exe),str(HERE/'MmrmCapture38.cs')]);assert compiled['exit_code']==0
 out=HERE/'current-results'/config;out.mkdir(parents=True,exist_ok=False)
 result=command('current-'+config,[str(exe),str(folder),str(HERE/('inputs-'+config+'.jsonl')),str(out/'results.tsv'),str(out/'execution.json'),'current'],env={'BESHSTAT_LATE38_CONFIGURATION':config});assert result['exit_code']==0
 save('current-execution-'+config+'.json',dict(configuration=config,role='current',reference_class=CLASS,original_output_seal=pin(HERE/'original-output-seal.json'),
      commands=[clean(compiled),clean(result)],files=[pin(p) for p in sorted(out.iterdir())],executable=library_pin(exe),
      assemblies=[library_pin(p) for p in sorted(folder.glob('*.dll'))],current_sources=[pin(ROOT/r['source']['path']) for r in json.loads((ROOT/'evidence/04A38/preedit/entry-source-snapshot.json').read_bytes())['files']]))
 print('Executed current',config,'after input/original seals; independent comparison required')
if __name__=='__main__':main()
