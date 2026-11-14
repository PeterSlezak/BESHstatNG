"""Isolated read-only old readers/tests; only independent fixture roots overlap."""
import sys
from concurrent.futures import ThreadPoolExecutor
from inspect_entry import ROOT,HERE,pin,save
sys.path.insert(0,str(ROOT/'evidence/04A38'))
from run import m,PRIVATE
from baseline_report import clean

def main():
 runner=m.Runner(PRIVATE)
 p='evidence/04A38/'
 lanes=[
 ('preedit-reader',[p+'preedit_reader.py']),
 ('addendum-reader',[p+'preedit_addendum_reader.py']),
 ('checkpoint-reader',[p+'verify_review_packet.py']),
 ('continuation-reader',[p+'continuation/v1/verify_continuation.py']),
 ('late432-reader',[p+'late-archived-entry-supplement/v1/verify_supplement.py']),
 ('late432-packet-reader',[p+'late-archived-entry-supplement/v1/verify_packet.py']),
 ('preedit-adverse',[p+'late-archived-entry-supplement/v1/replay_preedit_adverse.py']),
 ('checkpoint-adverse',['-m','unittest','discover','-s',p.rstrip('/'),'-p','test_review_packet.py','-v']),
 ('continuation-adverse',['-m','unittest','discover','-s',p+'continuation/v1','-p','test_branch_audit.py','-v']),
 ('late432-adverse',['-m','unittest','discover','-s',p+'late-archived-entry-supplement/v1','-p','test_supplement.py','-v']),
 ('late432-manifest-adverse',['-m','unittest','discover','-s',p+'late-archived-entry-supplement/v1','-p','test_packet.py','-v'])]
 def run(lane):
  name,args=lane
  return dict(lane=name,receipt=clean(runner.command('gee38-v2-prior-'+name,[sys.executable,'-B',*args])))
 with ThreadPoolExecutor(max_workers=4) as pool:results=list(pool.map(run,lanes))
 save('prior-validation.json',dict(executed=results,all_exit_zero=all(r['receipt']['exit_code']==0 for r in results),
      expectations_changed=False,reference_generators_rerun=False,old_not_ready_decisions_preserved=True))
 if not all(r['receipt']['exit_code']==0 for r in results):raise SystemExit(1)
if __name__=='__main__':main()
