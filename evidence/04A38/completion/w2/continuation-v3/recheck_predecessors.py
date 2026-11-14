"""Unchanged historical readers; exact historical views, never new originals."""
import shutil
from common import *
from baseline_report import python_receipt
mode=sys.argv[1];records=[]
if mode=='C2':
 view=RUNROOT/'c3-C2-exact-view';view.mkdir();aliases={r['path']:ROOT/r['preimage']['path'] for r in json.loads((HERE/'source-transitions-final.json').read_bytes())['files']}
 manifest=PREVIOUS/'publication-manifest-v3.json';mapped=[]
 for r in json.loads(manifest.read_bytes())['entries']:
  p=r['path'];src=ROOT/p
  if r['sha256'] is not None and sha(src.read_bytes())!=r['sha256']:
   src=aliases[p];mapped.append(dict(path=p,preimage=src.relative_to(ROOT).as_posix()))
  if r['sha256'] is not None:assert (src.stat().st_size,sha(src.read_bytes()))==(r['bytes'],r['sha256'])
  dest=view/p;dest.parent.mkdir(parents=True,exist_ok=True);shutil.copyfile(src,dest)
 (view/'.test-rationalization-04A38.tmp').mkdir()
 patterns=[('ownership','test_readers.py'),('cache','test_host_cache_v4.py'),('text','test_final_domains.py'),('publication','test_publication_v2.py')]
 for label,pattern in patterns:
  name='C2-reader-'+label;r=command(name,[sys.executable,'-B','-m','unittest','discover','-s',str(view/PREVIOUS.relative_to(ROOT)),'-p',pattern,'-v'],cwd=view)
  records.append(dict(label=label,command=clean(r),tests=python_receipt('w2c3-'+name)))
 save('C2-reader-replay.json',dict(scope='EXACT_C2_PUBLICATION_SOURCE_PROJECTION;NOT_LIVE_CURRENT_SOURCE',source_aliases=mapped,results=records,new_original_queries=0))
elif mode=='early':
 view=RUNROOT/'w2-before-v2';assert view.is_dir()
 patterns=[('checkpoint','evidence/04A38','test_review_packet.py'),('continuation','evidence/04A38/continuation/v1','test_branch_audit.py')]
 for a,d in [('A','v1'),('B','v2-gee-profile'),('C','v3-mmrm-headered-profile'),('D','v4-mmrm-duplicate-guards'),('E','v5-mmrm-unmatched-profile'),('F','v6-mmrm-intercept-group'),('G','v7-consolidated-gee-spec-mmrm-wide')]:patterns.append((a,'evidence/04A38/late-archived-entry-supplement/'+d,'test_*.py'))
 patterns.append(('W1','evidence/04A38/completion/w1','test_*.py'))
 for label,path,pattern in patterns:
  name='earlier-reader-'+label;r=command(name,[sys.executable,'-B','-m','unittest','discover','-s',path,'-p',pattern,'-v'],cwd=Path('\\\\?\\'+str(view)))
  records.append(dict(label=label,command=clean(r),tests=python_receipt('w2c3-'+name)))
 save('earlier-reader-replay.json',dict(scope='AUTHENTICATED_PRE_W2_SOURCE_VIEW;NOT_CURRENT_IMPLEMENTATION',results=records,new_original_queries=0,qualification='Any preserved historical invalid v1 fixture remains nonpass; canonical later reader results are separately named'))
