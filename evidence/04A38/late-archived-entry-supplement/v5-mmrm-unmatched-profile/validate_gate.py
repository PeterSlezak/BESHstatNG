"""Run independent focused readers and temporary-copy adversarial controls."""
from concurrent.futures import ThreadPoolExecutor
from common import *
def main():
 lanes=[('supplement-adverse',['-m','unittest','discover','-s',str(HERE),'-p','test_supplement.py','-v']),('packet-adverse',['-m','unittest','discover','-s',str(HERE),'-p','test_packet.py','-v']),('gate-independent-reader',[str(HERE/'verify_supplement.py')]),('original-source-reader',[str(HERE/'verify_original_source.py')])]
 def run(lane):
  name,args=lane;return dict(lane=name,receipt=clean(command(name,[sys.executable,'-B',*args])))
 with ThreadPoolExecutor(max_workers=4) as pool:results=list(pool.map(run,lanes))
 save('focused-validation-attempt-v1.json',dict(results=results,all_exit_zero=all(r['receipt']['exit_code']==0 for r in results)))
 assert all(r['receipt']['exit_code']==0 for r in results)
if __name__=='__main__':main()
