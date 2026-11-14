"""Read-only prior readers/tests, isolated independent process fixtures."""
from concurrent.futures import ThreadPoolExecutor
from common import *
def main():
 p='evidence/04A38/';v1=p+'late-archived-entry-supplement/v1';v2=p+'late-archived-entry-supplement/v2-gee-profile';v3=p+'late-archived-entry-supplement/v3-mmrm-headered-profile'
 v4=p+'late-archived-entry-supplement/v4-mmrm-duplicate-guards'
 v5=p+'late-archived-entry-supplement/v5-mmrm-unmatched-profile'
 lanes=[('unmatched-reader',[v5+'/verify_supplement.py']),('unmatched-closeout-reader',[v5+'/verify_closeout.py','v2']),('duplicate-reader',[v4+'/verify_supplement.py']),('duplicate-closeout-reader',[v4+'/verify_closeout.py']),('preedit-reader',[p+'preedit_reader.py']),('addendum-reader',[p+'preedit_addendum_reader.py']),('checkpoint-reader',[p+'verify_review_packet.py']),
      ('continuation-reader',[p+'continuation/v1/verify_continuation.py']),('late432-reader',[v1+'/verify_supplement.py']),('late432-packet-reader',[v1+'/verify_packet.py']),
      ('gee-reader',[v2+'/verify_gee.py','--publication']),('gee-closeout-reader',[v2+'/verify_closeout.py']),
      ('headered-reader',[v3+'/verify_supplement.py']),('headered-closeout-reader',[v3+'/verify_closeout.py']),('preedit-adverse',[v1+'/replay_preedit_adverse.py'])]
 for name,folder,pattern in [('unmatched-adverse',v5,'test_supplement.py'),('unmatched-manifest-adverse',v5,'test_packet.py'),('old-group-gap-adverse',v5,'test_group_gap.py'),('unmatched-closeout-adverse',v5,'test_closeout.py'),('duplicate-adverse',v4,'test_supplement.py'),('duplicate-manifest-adverse',v4,'test_packet.py'),('old-mask-gap-adverse',v4,'test_mask_gap.py'),('checkpoint-adverse',p,'test_review_packet.py'),('continuation-adverse',p+'continuation/v1','test_branch_audit.py'),
      ('late432-adverse',v1,'test_supplement.py'),('late432-manifest-adverse',v1,'test_packet.py'),('gee-adverse',v2,'test_gee.py'),('protected-adverse',v2,'test_preservation.py'),
      ('gee-manifest-adverse',v2,'test_manifest.py'),('old-header-gap-adverse',v2,'test_mmrm_gap.py'),('headered-adverse',v3,'test_supplement.py'),
      ('headered-manifest-adverse',v3,'test_packet.py'),('old-guard-gap-adverse',v3,'test_guard_gap.py')]:
  lanes.append((name,['-m','unittest','discover','-s',folder,'-p',pattern,'-v']))
 for name,folder,pattern in [('intercept-adverse',str(V6),'test_supplement.py'),('intercept-packet-adverse',str(V6),'test_packet.py'),('w1-adverse',str(W1),'test_w1_v2.py'),('w1-publication-adverse',str(W1),'test_publication.py')]:
  lanes.append((name,['-m','unittest','discover','-s',folder,'-p',pattern,'-v']))
 lanes.extend([('intercept-reader',[str(V6/'verify_supplement.py')]),('w1-publication-reader',[str(W1/'verify_publication.py'),'v2'])])
 def run(lane):
  name,args=lane;result=command('prior-'+name,[sys.executable,'-B',*args]);return dict(lane=name,receipt=clean(result))
 with ThreadPoolExecutor(max_workers=4) as pool:results=list(pool.map(run,lanes))
 save('prior-validation.json',dict(executed=results,all_exit_zero=all(r['receipt']['exit_code']==0 for r in results),expectations_changed=False))
 assert all(r['receipt']['exit_code']==0 for r in results)
if __name__=='__main__':main()
