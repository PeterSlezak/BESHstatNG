import json,sys
from pathlib import Path
from concurrent.futures import ThreadPoolExecutor
HERE=Path(__file__).resolve().parent;ROOT=HERE.parents[3]
sys.path.insert(0,str(ROOT/'evidence/04A38/late-archived-entry-supplement/v6-mmrm-intercept-group'))
from common import command,clean,RUNROOT
def main():
 jobs=[('w1-independent-reader',[str(HERE/'verify_w1_v2.py')]),('w1-adverse',['-m','unittest','discover','-s',str(HERE),'-p','test_w1.py','-v'])]
 def run(job):label,args=job;return dict(label=label,receipt=clean(command(label,[sys.executable,'-B',*args])))
 with ThreadPoolExecutor(max_workers=2) as pool:results=list(pool.map(run,jobs))
 with (HERE/'validation-attempt-v1.json').open('x',encoding='utf-8') as f:json.dump(dict(results=results,all_exit_zero=all(r['receipt']['exit_code']==0 for r in results)),f,indent=2)
 assert all(r['receipt']['exit_code']==0 for r in results)
if __name__=='__main__':main()
