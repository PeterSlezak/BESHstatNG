"""Retain setup errors; create missing private output parent and correct path spelling."""
from common import *
from baseline_report import python_receipt
view=RUNROOT/'cc2-c1-view';(view/'.test-rationalization-04A38.tmp').mkdir(exist_ok=False);records=[]
for label,pattern in [('P0','test_p0.py'),('P1','test_p1_v3.py'),('publication','test_publication_v3.py')]:
 start=str(view/PREVIOUS.relative_to(ROOT))
 if label!='publication':start='\\\\?\\'+start
 name='C1-'+label+'-corrected-v2';r=command(name,[sys.executable,'-B','-m','unittest','discover','-s',start,'-p',pattern,'-v'],cwd=view);records.append(dict(label=label,command=clean(r),tests=python_receipt('w2c2-'+name)))
save('C1-reader-corrected-replay.json',dict(results=records,previous_setup='Three0-method/1ERROR setups: missing private output parent; publication additionally double extended-path prefix. All raw attempts retained. Only output parent creation/path spelling corrected; no immutable source/assertion changed.',original_queries=0,scope='EXACT_HISTORICAL_C1_SOURCE_PROJECTION_NOT_LIVE_CURRENT_IMPLEMENTATION'))
