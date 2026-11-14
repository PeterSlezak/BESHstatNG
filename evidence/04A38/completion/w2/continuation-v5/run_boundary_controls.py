from common import *
r=command('independent-c5-boundary-controls',[sys.executable,'-B','-m','unittest','discover','-s',str(HERE),'-p','test_checkpoint_boundary.py','-v'])
save('independent-controls-boundary.json',dict(command=clean(r),passed=r['exit_code']==0,expected_test_count=32,
    scope='Final cells-v5 whole-source/actual-CIL/ByRef/acquisition/API/alias proof plus meaningful OPEN/branch/support/export/source/census/protected-evidence mutations'))
assert r['exit_code']==0
