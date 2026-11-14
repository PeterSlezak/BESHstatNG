"""Execute only the sealed authorized original cases, never current code."""
from common import *
from verify_inputs import inspect

def main():
 config,mode=sys.argv[1:3];assert config in ['Debug','Release'] and mode in ['plain','repeat','trace']
 inspect();assert not (HERE/'original-output-seal.json').exists() and not (HERE/'current-results').exists()
 folder=PRIVATE/('original-'+config+('-trace' if mode=='trace' else '-plain'));exe=folder/'MmrmCapture38.exe'
 out=HERE/'original-results'/config/mode;out.mkdir(parents=True,exist_ok=False)
 result=command('original-'+config+'-'+mode,[str(exe),str(folder),str(HERE/('inputs-'+config+'.jsonl')),str(out/'results.tsv'),str(out/'execution.json'),'original'],
      env={'BESHSTAT_LATE38_CONFIGURATION':config});assert result['exit_code']==0
 save('original-execution-'+config+'-'+mode+'.json',dict(role='original',configuration=config,mode=mode,input_seal=pin(HERE/'input-seal.json'),
      executable=library_pin(exe),files=[pin(p) for p in sorted(out.iterdir())],command=clean(result),current_executions=0,reference_class=CLASS))
if __name__=='__main__':main()
