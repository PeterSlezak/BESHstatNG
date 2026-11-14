"""Compile current actual UDF paths only after the original-output seal exists."""
import json,os,shutil,subprocess,sys
from pathlib import Path
from prepare_original import ROOT,HERE,PRIVATE,sha,pin,save
from compile_original_harness import instrument,TRACE
sys.path.insert(0,str(ROOT/'evidence/04A38'))
from run import m,PRIVATE as RUNROOT
from baseline_report import clean

def main():
 config=sys.argv[1];assert config in ['Debug','Release']
 seal=json.loads((HERE/'original-output-seal.json').read_bytes())
 for row in seal['files']:assert pin(ROOT/row['path'])==row
 assert seal['current_supplement_executed_at_seal'] is False
 # Reuse only the source-pinned, already fully validated current libraries; the
 # unregistered current UDF source is freshly compiled below, with real calls to
 # these current Core/Common services. No reference result generation here.
 checkpoint=json.loads((ROOT/'evidence/04A38/continuation/v1/entry.json').read_bytes())
 for row in checkpoint['checkpoint_payloads']:
  if not row['path'].startswith('evidence/'):assert pin(ROOT/row['path'])==row
 folder=PRIVATE/('current-'+config+'-trace');folder.mkdir(exist_ok=False)
 build=RUNROOT/'builds/validation-v2'/config/'legacy'
 for name in ['BESHStatNG.dll','BESHStat.Core.dll','BESHStat.Excel.Common.dll','ExcelDna.Integration.dll']:shutil.copy2(build/name,folder/name)
 sources=[];original=json.loads((ROOT/'evidence/04A38/preedit/entry-source-snapshot.json').read_bytes())['files']
 for row in original:
  p=ROOT/row['source']['path'];source=p;names=[]
  if p.name=='PlotDataUDFs.vb':names=['CALIB_POINTS','BuildCalibrationPointsTable']
  if p.name=='UdfDataImport.vb':names=['TryGetGroupedCompleteMatrix','TryGetNestedThreeColumnData']
  if names:
   text=p.read_text(encoding='utf-8-sig')
   for name in names:text,_=instrument(text,name)
   source=folder/p.name;source.write_text(text,encoding='utf-8',newline='\n')
  sources.append(source)
 sources += [ROOT/'BESHstatNG/src/DM/WindowsSpreadsheetTransport.vb',ROOT/'BESHstatNG/src/RegModels/MixedModels/MixedModelFrontEndHelpers.vb',
  ROOT/'evidence/04A38/preedit/native-safety/NativeBoundary38.vb',ROOT/'evidence/04A38/preedit/native-safety/DependencySlices38.vb']
 trace=folder/'LateBranch38.vb';trace.write_text(TRACE,encoding='utf-8',newline='\n');sources.append(trace)
 locator=Path(os.environ['ProgramFiles(x86)'])/'Microsoft Visual Studio/Installer/vswhere.exe'
 def compiler(name):return subprocess.check_output([str(locator),'-products','*','-version','[18.0,19.0)','-find','MSBuild/Current/Bin/Roslyn/'+name],text=True).splitlines()[0]
 facade=Path(os.environ['ProgramFiles(x86)'])/'Reference Assemblies/Microsoft/Framework/.NETFramework/v4.8/Facades/netstandard.dll'
 refs=[folder/name for name in ['BESHStatNG.dll','BESHStat.Core.dll','BESHStat.Excel.Common.dll','ExcelDna.Integration.dll']]+[facade]
 runner=m.Runner(RUNROOT)
 command=[compiler('vbc.exe'),'/nologo','/target:library','/rootnamespace:BESHStatNG','/optioncompare:text','/optioninfer+','/optionexplicit+',
  '/imports:System,System.Collections,System.Collections.Generic,System.Diagnostics,System.Linq,System.Globalization','/optimize'+('+' if config=='Release' else '-'),
  '/out:'+str(folder/'BESHStat.Core.Tests.dll'),'/reference:System.Core.dll','/reference:System.dll','/reference:System.Data.dll','/reference:System.Xml.Linq.dll',*['/reference:'+str(p) for p in refs],*map(str,sources)]
 result=runner.command('late38-current-'+config+'-source-compile',command);assert result['exit_code']==0
 executable=folder/'LateCurrentCapture38.exe'
 result=runner.command('late38-current-'+config+'-recorder',[compiler('csc.exe'),'/nologo','/target:exe','/reference:System.Core.dll','/reference:System.Web.Extensions.dll','/reference:'+str(folder/'BESHStat.Core.dll'),'/reference:'+str(facade),'/out:'+str(executable),str(HERE/'LateCapture38.cs')]);assert result['exit_code']==0
 out=HERE/'current-results'/config;out.mkdir(parents=True,exist_ok=False)
 result=runner.command('late38-current-'+config+'-execute',[str(executable),str(folder),str(HERE/('inputs-'+config+'.jsonl')),str(out/'results.tsv'),str(out/'execution.json'),'current'],env={'BESHSTAT_LATE38_CONFIGURATION':config});assert result['exit_code']==0
 save('current-execution-'+config+'.json',dict(status='CURRENT_SUPPLEMENT_EXECUTED_AFTER_ORIGINAL_SEAL',configuration=config,original_output_seal=pin(HERE/'original-output-seal.json'),
  results=pin(out/'results.tsv'),execution=pin(out/'execution.json'),branches=pin(out/'results.tsv.branches.json'),command=clean(result),
  actual_current_source_files=[pin(ROOT/row['source']['path']) for row in original],
  libraries=[dict(name=p.name,bytes=p.stat().st_size,sha256=sha(p.read_bytes())) for p in sorted(folder.glob('*.dll'))],
  chronological_qualification='Current432 execution after original plain/repeat/trace outputs and seal; no expected bytes refreshed',native='NOT_RUN',scientific='NOT_GOLDEN'))
 print('Executed current',config,'only after authenticated original-output seal')

if __name__=='__main__':main()
