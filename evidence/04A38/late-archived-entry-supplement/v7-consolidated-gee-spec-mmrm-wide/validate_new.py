"""Raw receipts for separately executed independent readers and controls."""
from common import *
results=[]
for name,args in [('supplement-adverse',['-m','unittest','discover','-s',str(HERE),'-p','test_supplement.py','-v']),('packet-adverse',['-m','unittest','discover','-s',str(HERE),'-p','test_packet.py','-v']),('gate-independent-reader',[str(HERE/'verify_supplement.py')]),('public-copy-reader',[str(HERE/'verify_public_copy.py')])]:
 r=command(name,[sys.executable,'-B',*args]);results.append(dict(lane=name,receipt=clean(r)))
save('focused-validation.json',dict(results=results,all_exit_zero=all(r['receipt']['exit_code']==0 for r in results),earlier_direct_supplement_control_run='56 PASS; no semantic original queries; repeated here to retain raw command/outcome receipts'))
assert all(r['receipt']['exit_code']==0 for r in results)
