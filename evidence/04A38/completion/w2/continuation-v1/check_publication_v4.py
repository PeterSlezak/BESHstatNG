from common import *
from baseline_report import python_receipt
r=command('publication-adverse-v3',[sys.executable,'-B','-m','unittest','discover','-s',str(HERE),'-p','test_publication_v3.py','-v']);receipt=python_receipt('w2c1-publication-adverse-v3')
save('publication-verification-v4.json',dict(command=clean(r),adverse=receipt,status='PARTIAL_PACKET_15_ACTUAL_NEGATIVE_CONTROLS_PASS_NOT_READY' if r['exit_code']==0 else 'PARTIAL_PACKET_ADVERSE_FAILED_NOT_READY',source=pin(HERE/'test_publication_v3.py'),manifest_explicit='publication-manifest-v3.json',reviewed_literal='privacy-reviewed-adverse-literal.json',failed_fixture='attempts/publication-adverse-v2-default-manifest.json'))
assert r['exit_code']==0 and receipt['total']==15
