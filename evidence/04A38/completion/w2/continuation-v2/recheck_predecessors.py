"""Immutable reader replay, exact historical source projections; no original queries."""
import shutil
from common import *
from baseline_report import python_receipt
before=RUNROOT/'w2-before-v2';assert before.is_dir()
for r in json.loads((ROOT/'evidence/04A38/preedit/entry-source-snapshot.json').read_bytes())['files']:assert (before/r['snapshot']['path']).read_bytes()==(ROOT/r['snapshot']['path']).read_bytes()
patterns=[('preedit','evidence/04A38','test_preedit_reader.py'),('checkpoint','evidence/04A38','test_review_packet.py'),('continuation','evidence/04A38/continuation/v1','test_branch_audit.py')]
for a,d in [('A','v1'),('B','v2-gee-profile'),('C','v3-mmrm-headered-profile'),('D','v4-mmrm-duplicate-guards'),('E','v5-mmrm-unmatched-profile'),('F','v6-mmrm-intercept-group'),('G','v7-consolidated-gee-spec-mmrm-wide')]:patterns.append((a,'evidence/04A38/late-archived-entry-supplement/'+d,'test_*.py'))
patterns.append(('W1','evidence/04A38/completion/w1','test_*.py'));records=[]
for label,path,pattern in patterns:
 name='predecessor-'+label;r=command(name,[sys.executable,'-B','-m','unittest','discover','-s',path,'-p',pattern,'-v'],cwd=Path('\\\\?\\'+str(before)))
 records.append(dict(label=label,command=clean(r),receipt=python_receipt('w2c2-'+name)))
save('earlier-reader-replay.json',dict(scope='AUTHENTICATED_PRE_W2_HISTORICAL_SOURCE_VIEW_NOT_CURRENT_IMPLEMENTATION',results=records,original_queries=0,failures='All archived malformed setup tests remain visible; actual raw outcomes retained'))
# Replay exact C1 producer-independent tests with its immutable manifest/source versions.
view=RUNROOT/'cc2-c1-view';view.mkdir(exist_ok=False);aliases={}
for r in json.loads((HERE/'source-transitions-final.json').read_bytes())['files']:aliases[(r['path'],r['before']['sha256'])]=ROOT/r['preimage']['path']
manifest=PREVIOUS/'publication-manifest-v4.json';rows=json.loads(manifest.read_bytes())['entries'];mapped=[]
for row in rows:
 p=row['path'];src=ROOT/p
 if row['sha256'] is not None and sha(src.read_bytes())!=row['sha256']:
  src=aliases[(p,row['sha256'])];mapped.append(dict(path=p,retained=src.relative_to(ROOT).as_posix()))
 if row['sha256'] is not None:assert pin(src)['sha256']==row['sha256'] and src.stat().st_size==row['bytes']
 dest=view/p;dest.parent.mkdir(parents=True,exist_ok=True);shutil.copyfile(src,dest)
for label,pattern in [('P0','test_p0.py'),('P1','test_p1_v3.py'),('publication','test_publication_v3.py')]:
 name='C1-'+label;r=command(name,[sys.executable,'-B','-m','unittest','discover','-s',str(PREVIOUS.relative_to(ROOT)),'-p',pattern,'-v'],cwd=Path('\\\\?\\'+str(view)))
 records.append(dict(label=label,command=clean(r),receipt=python_receipt('w2c2-'+name)))
save('predecessor-reader-replay.json',dict(results=records,old_C1_manifest=pin(manifest),source_aliases=mapped,view='EXACT_C1_2644_PAYLOAD_PROJECTION_NOT_GIT_OR_CURRENT_SOURCE',original_queries=0))
