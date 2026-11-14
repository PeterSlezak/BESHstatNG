"""Current UDF compilation/execution only after the fixed original output seal."""
import json,shutil,sys
from pathlib import Path
from inspect_entry import ROOT,HERE,BASE,CLASS,pin,sha,save
from build_original import PRIVATE,compiler,library_pin
from compile_harness import instrument,TRACE
from verify_inputs import inspect
sys.path.insert(0,str(ROOT/'evidence/04A38'))
from run import m,PRIVATE as RUNROOT
from baseline_report import clean
SEAL='a87736973eb6cee5b76b1fe95dd508de679c75ceffde3dd93647a047a9f63b98'

def main():
 config=sys.argv[1];assert config in ['Debug','Release'];inspect()
 assert sha((HERE/'original-output-seal.json').read_bytes())==SEAL
 seal=json.loads((HERE/'original-output-seal.json').read_bytes())
 for row in seal['files']:assert pin(ROOT/row['path'])==row
 assert seal['current_implementation_executed'] is False
 # These current assemblies are the isolated source-pinned checkpoint builds.
 # All current source bytes must still match the old checkpoint assertions.
 checkpoint=json.loads((ROOT/'evidence/04A38/continuation/v1/entry.json').read_bytes())
 for row in checkpoint['checkpoint_payloads']:
  if not row['path'].startswith('evidence/'):assert pin(ROOT/row['path'])==row
 folder=PRIVATE/('current-'+config+'-trace');folder.mkdir(exist_ok=False)
 build=RUNROOT/'builds/validation-v2'/config/'legacy'
 known=json.loads((ROOT/('evidence/04A38/late-archived-entry-supplement/v1/current-execution-'+config+'.json')).read_bytes())['libraries']
 known={r['name']:r for r in known}
 for name in ['BESHStatNG.dll','BESHStat.Core.dll','BESHStat.Excel.Common.dll','ExcelDna.Integration.dll']:
  assert library_pin(build/name)==known[name],name
  shutil.copy2(build/name,folder/name)
 sources=[];maps=[]
 for row in json.loads((ROOT/'evidence/04A38/preedit/entry-source-snapshot.json').read_bytes())['files']:
  source=ROOT/row['source']['path']
  if source.name=='GeeUDFs.vb':
   text,maps=instrument(source.read_text(encoding='utf-8-sig'))
   source=folder/source.name;source.write_text(text,encoding='utf-8',newline='\n')
  sources.append(source)
 sources += [ROOT/'BESHstatNG/src/DM/WindowsSpreadsheetTransport.vb',ROOT/'BESHstatNG/src/RegModels/MixedModels/MixedModelFrontEndHelpers.vb',
             ROOT/'evidence/04A38/preedit/native-safety/NativeBoundary38.vb',ROOT/'evidence/04A38/preedit/native-safety/DependencySlices38.vb']
 trace=folder/'GeeBranch38.vb';trace.write_text(TRACE,encoding='utf-8',newline='\n');sources.append(trace)
 facade=Path('C:/Program Files (x86)/Reference Assemblies/Microsoft/Framework/.NETFramework/v4.8/Facades/netstandard.dll')
 refs=[folder/name for name in ['BESHStatNG.dll','BESHStat.Core.dll','BESHStat.Excel.Common.dll','ExcelDna.Integration.dll']]+[facade]
 runner=m.Runner(RUNROOT);commands=[]
 args=[compiler('vbc.exe'),'/nologo','/target:library','/rootnamespace:BESHStatNG','/optioncompare:text','/optioninfer+','/optionexplicit+',
       '/imports:System,System.Collections,System.Collections.Generic,System.Diagnostics,System.Linq,System.Globalization',
       '/optimize'+('+' if config=='Release' else '-'),'/out:'+str(folder/'BESHStat.Core.Tests.dll'),
       '/reference:System.Core.dll','/reference:System.dll','/reference:System.Data.dll','/reference:System.Xml.Linq.dll',
       *['/reference:'+str(p) for p in refs],*map(str,sources)]
 result=runner.command('gee38-v2-current-'+config+'-source-compile',args);commands.append(clean(result));assert result['exit_code']==0
 exe=folder/'GeeCurrentCapture38.exe'
 result=runner.command('gee38-v2-current-'+config+'-recorder',[compiler('csc.exe'),'/nologo','/target:exe','/reference:System.Core.dll','/reference:System.Web.Extensions.dll',
       '/reference:'+str(folder/'BESHStat.Core.dll'),'/reference:'+str(facade),'/out:'+str(exe),str(HERE/'GeeCapture38-v2.cs')]);commands.append(clean(result));assert result['exit_code']==0
 out=HERE/'current-results'/config;out.mkdir(parents=True,exist_ok=False)
 result=runner.command('gee38-v2-current-'+config+'-execute',[str(exe),str(folder),str(HERE/('inputs-'+config+'.jsonl')),str(out/'results.tsv'),str(out/'execution.json'),'current'],
       env={'BESHSTAT_LATE38_CONFIGURATION':config});commands.append(clean(result));assert result['exit_code']==0
 save('current-execution-'+config+'.json',dict(status='CURRENT_EXECUTED_AFTER_ORIGINAL_SEAL',configuration=config,reference_class=CLASS,
      original_output_seal=pin(HERE/'original-output-seal.json'),commands=commands,files=[pin(p) for p in sorted(out.iterdir())],
      assemblies=[library_pin(p) for p in sorted(folder.glob('*.dll'))],instrumentation=maps,recorder=pin(HERE/'GeeCapture38-v2.cs'),
      current_source=[pin(ROOT/row['source']['path']) for row in json.loads((ROOT/'evidence/04A38/preedit/entry-source-snapshot.json').read_bytes())['files']],
      scientific='NOT_GOLDEN',native='NOT_RUN'))
 print('Current',config,'executed after original seal; comparison is a separate reader operation')
if __name__=='__main__':main()
