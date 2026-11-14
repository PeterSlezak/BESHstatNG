from common import *
from baseline_report import python_receipt
r=command('publication-adverse-v2',[sys.executable,'-B','-m','unittest','discover','-s',str(HERE),'-p','test_publication_v2.py','-v']);receipt=python_receipt('w2c1-publication-adverse-v2')
save('publication-verification-v3.json',dict(command=clean(r),adverse=receipt,status='PARTIAL_PACKET_NEGATIVE_CONTROLS_PASS_NOT_READY' if r['exit_code']==0 else 'PARTIAL_PACKET_ADVERSE_FAILED_NOT_READY',reviewed_literal='privacy-reviewed-adverse-literal.json',old_failed_privacy_receipt='attempts/export-v2-synthetic-source-literal.json'))
assert r['exit_code']==0 and receipt['total']==15
