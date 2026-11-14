from common import *
r=command('independent-c6-controls-v1',[sys.executable,'-B','-m','unittest','discover','-s',str(HERE),'-p','test_checkpoint_v1.py','-v'])
save('independent-controls-v1.json',dict(command=clean(r),passed=r['exit_code']==0,scope='Actual source/CIL/ownership/fragment/support/export/consumer/boxed-copy/Err/generic-overload/original-reference mutations; all changes in memory, protected files untouched',original_queries=0))
assert r['exit_code']==0,'Preserve failed controls; fix the defect without waiving the assertion'
