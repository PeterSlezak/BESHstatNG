from common import *
from baseline_report import python_receipt
from verify_ownership_v4 import inspect
r=command('independent-ownership-v4',[sys.executable,'-B',str(HERE/'verify_ownership_v4.py')]);assert r['exit_code']==0
save('independent-ownership-proof.json',inspect())
r2=command('independent-ownership-adverse',[sys.executable,'-B','-m','unittest','discover','-s',str(HERE),'-p','test_ownership.py','-v'])
save('independent-ownership-controls.json',dict(reader=clean(r),adverse=clean(r2),tests=python_receipt('w2c2-independent-ownership-adverse')));assert r2['exit_code']==0
