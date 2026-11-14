"""Original-only execution, strictly after independently checked typed input freeze."""
import sys
from pathlib import Path
from inspect_entry import ROOT,HERE,BASE,CLASS,pin,save
from build_original import PRIVATE,compiler,library_pin
from verify_inputs import inspect
sys.path.insert(0,str(ROOT/'evidence/04A38'))
from run import m,PRIVATE as RUNROOT
from baseline_report import clean

def main():
 config,mode=sys.argv[1:3];assert config in ['Debug','Release'] and mode in ['plain','repeat','trace']
 inspect()
 assert not (HERE/'original-output-seal.json').exists()
 folder=PRIVATE/('original-'+config+('-trace' if mode=='trace' else '-plain'))
 import json
 tooling=json.loads((HERE/'recorder-tooling-seal.json').read_bytes())
 for row in tooling['files']: assert pin(ROOT/row['path'])==row
 out=HERE/'original-results'/config/(mode+'-v2');out.mkdir(parents=True,exist_ok=False)
 exe=folder/('GeeCapture38-'+mode+'-v2.exe');runner=m.Runner(RUNROOT)
 facade=Path('C:/Program Files (x86)/Reference Assemblies/Microsoft/Framework/.NETFramework/v4.8/Facades/netstandard.dll')
 result=runner.command('gee38-v2-original-'+config+'-'+mode+'-recorder-v2',[compiler('csc.exe'),'/nologo','/target:exe','/reference:System.Core.dll',
       '/reference:System.Web.Extensions.dll','/reference:'+str(folder/'BESHStat.Core.dll'),'/reference:'+str(facade),'/out:'+str(exe),str(HERE/'GeeCapture38-v2.cs')])
 assert result['exit_code']==0
 result=runner.command('gee38-v2-original-'+config+'-'+mode+'-execute-v2',[str(exe),str(folder),str(HERE/('inputs-'+config+'.jsonl')),str(out/'results.tsv'),str(out/'execution.json'),'original'],
       env={'BESHSTAT_LATE38_CONFIGURATION':config});assert result['exit_code']==0
 save('original-execution-'+config+'-'+mode+'.json',dict(status='ORIGINAL_EXECUTED_BEFORE_CURRENT',configuration=config,mode=mode,source_commit=BASE,
      reference_class=CLASS,input_seal=pin(HERE/'input-seal.json'),executable=library_pin(exe),command=clean(result),
      files=[pin(p) for p in sorted(out.iterdir())],recorder_tooling_seal=pin(HERE/'recorder-tooling-seal.json'),current_executions=0))
if __name__=='__main__':main()
