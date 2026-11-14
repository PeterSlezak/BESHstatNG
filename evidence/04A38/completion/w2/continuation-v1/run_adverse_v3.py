from common import *
from baseline_report import python_receipt
r=command('p1-adverse-v3',[sys.executable,'-B','-m','unittest','discover','-s',str(HERE),'-p','test_p1_v3.py','-v']);receipt=python_receipt('w2c1-p1-adverse-v3');assert r['exit_code']==0 and receipt['total']==15
save('bounded-adverse-addendum-v3.json',dict(status='15_ACTUAL_P1_CONDITIONS_PASS',receipt=receipt,clarification='Earlier14 included a valid name mutation mislabeled default; original source/results preserved. This version independently reaches both name and optional-default conditions.',source=pin(HERE/'test_p1_v3.py'),new_original_queries=0))
