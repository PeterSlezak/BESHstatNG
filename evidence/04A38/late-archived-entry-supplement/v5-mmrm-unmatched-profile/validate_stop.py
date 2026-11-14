from common import *
from concurrent.futures import ThreadPoolExecutor
def main():
 def run(job):
  label,args=job;return dict(label=label,receipt=clean(command(label,[sys.executable,'-B',*args])))
 jobs=[('group-gap-reader',[str(HERE/'verify_group_gap.py')]),('group-gap-adverse',['-m','unittest','discover','-s',str(HERE),'-p','test_group_gap.py','-v'])]
 with ThreadPoolExecutor(max_workers=2) as pool:results=list(pool.map(run,jobs))
 save('post-gate-stop-validation.json',dict(results=results,all_exit_zero=all(r['receipt']['exit_code']==0 for r in results)))
 assert all(r['receipt']['exit_code']==0 for r in results)
if __name__=='__main__':main()
