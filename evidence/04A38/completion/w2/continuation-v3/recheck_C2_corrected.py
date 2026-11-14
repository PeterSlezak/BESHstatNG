"""Correct test filename selection; preserve first zero-discovery attempts."""
from common import *
from baseline_report import python_receipt
view=RUNROOT/'c3-C2-exact-view';assert view.is_dir();records=[]
for label,pattern in [('slice','test_independent_slice.py'),('ownership','test_ownership.py'),('cache','test_host_cache.py')]:
 assert (view/PREVIOUS.relative_to(ROOT)/pattern).is_file()
 name='C2-reader-'+label+'-selected-v2';r=command(name,[sys.executable,'-B','-m','unittest','discover','-s',str(view/PREVIOUS.relative_to(ROOT)),'-p',pattern,'-v'],cwd=view);tests=python_receipt('w2c3-'+name);assert tests['total']>0
 records.append(dict(label=label,command=clean(r),tests=tests))
save('C2-reader-corrected-replay.json',dict(scope='EXACT_C2_3124_PAYLOAD_SOURCE_PROJECTION;NOT_CURRENT_SOURCE',results=records,previous_attempt='recheck_predecessors.py selected three nonexistent patterns; exit5 and zero tests retained, never counted as PASS',original_queries=0))
