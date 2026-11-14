"""Read-only prior readers/tests, isolated independent process fixtures."""
from concurrent.futures import ThreadPoolExecutor
from common import *
def main():
 p='evidence/04A38/';v1=p+'late-archived-entry-supplement/v1';v2=p+'late-archived-entry-supplement/v2-gee-profile';v3=p+'late-archived-entry-supplement/v3-mmrm-headered-profile'
 v4=p+'late-archived-entry-supplement/v4-mmrm-duplicate-guards'
 lanes=[('duplicate-reader',[v4+'/verify_supplement.py']),('duplicate-closeout-reader',[v4+'/verify_closeout.py']),('preedit-reader',[p+'preedit_reader.py']),('addendum-reader',[p+'preedit_addendum_reader.py']),('checkpoint-reader',[p+'verify_review_packet.py']),
      ('continuation-reader',[p+'continuation/v1/verify_continuation.py']),('late432-reader',[v1+'/verify_supplement.py']),('late432-packet-reader',[v1+'/verify_packet.py']),
      ('gee-reader',[v2+'/verify_gee.py','--publication']),('gee-closeout-reader',[v2+'/verify_closeout.py']),
      ('headered-reader',[v3+'/verify_supplement.py']),('headered-closeout-reader',[v3+'/verify_closeout.py']),('preedit-adverse',[v1+'/replay_preedit_adverse.py'])]
 for name,folder,pattern in [('duplicate-adverse',v4,'test_supplement.py'),('duplicate-manifest-adverse',v4,'test_packet.py'),('old-mask-gap-adverse',v4,'test_mask_gap.py'),('checkpoint-adverse',p,'test_review_packet.py'),('continuation-adverse',p+'continuation/v1','test_branch_audit.py'),
      ('late432-adverse',v1,'test_supplement.py'),('late432-manifest-adverse',v1,'test_packet.py'),('gee-adverse',v2,'test_gee.py'),('protected-adverse',v2,'test_preservation.py'),
      ('gee-manifest-adverse',v2,'test_manifest.py'),('old-header-gap-adverse',v2,'test_mmrm_gap.py'),('headered-adverse',v3,'test_supplement.py'),
      ('headered-manifest-adverse',v3,'test_packet.py'),('old-guard-gap-adverse',v3,'test_guard_gap.py')]:
  lanes.append((name,['-m','unittest','discover','-s',folder,'-p',pattern,'-v']))
 def run(lane):
  name,args=lane;result=command('prior-'+name,[sys.executable,'-B',*args]);return dict(lane=name,receipt=clean(result))
 with ThreadPoolExecutor(max_workers=4) as pool:results=list(pool.map(run,lanes))
 save('prior-validation.json',dict(executed=results,all_exit_zero=all(r['receipt']['exit_code']==0 for r in results),expectations_changed=False))
 assert all(r['receipt']['exit_code']==0 for r in results)
if __name__=='__main__':main()
