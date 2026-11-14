from common import *
from baseline_report import python_receipt
commands=[]
for label,script,args in [('publication-local-v1','verify_publication.py',[]),('publication-copy-v1','check_public_copy.py',[])]:
 r=command(label,[sys.executable,'-B',str(HERE/script),*args]);commands.append(dict(label=label,command=clean(r)));assert r['exit_code']==0
r=command('publication-adverse-v1',[sys.executable,'-B','-m','unittest','discover','-s',str(HERE),'-p','test_publication.py','-v']);commands.append(dict(label='publication-adverse-v1',command=clean(r)))
receipt=python_receipt('w2c1-publication-adverse-v1')
save('publication-verification-v1.json',dict(commands=commands,adverse=receipt,status='PARTIAL_PACKET_VERIFIED_NOT_BATCH_READY' if r['exit_code']==0 else 'PARTIAL_PACKET_ADVERSE_FAILED_NOT_READY',copy_scope='Only included payloads; live Git/omitted files/private assemblies require local receipts'))
assert r['exit_code']==0
