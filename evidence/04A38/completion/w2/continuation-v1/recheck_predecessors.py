"""Re-execute immutable readers on exact historical source versions.

The view is an explicit non-Git historical projection, not a worktree or an
original semantic execution. Actual current sources are verified separately.
"""
import shutil
from common import *
view=ROOT/'.w2c1-predecessor-v1.tmp';view.mkdir(exist_ok=False)
manifest=json.loads((W2/'publication-manifest-closeout-v2.json').read_bytes());aliases={}
for name,key in [('entry.json','source_before'),('source-before-p1.json','files'),('source-before-p1-reports.json','files')]:
 for r in json.loads((HERE/name).read_bytes())[key]:aliases[(r['live']['path'],r['live']['sha256'])]=ROOT/r['preimage']['path']
mapped=[]
for row in manifest['entries']:
 p=row['path'];src=ROOT/p
 if row['sha256'] is not None and sha(src.read_bytes())!=row['sha256']:
  src=aliases[(p,row['sha256'])];mapped.append(dict(path=p,retained=src.relative_to(ROOT).as_posix(),sha256=row['sha256']))
 if row['sha256'] is not None:assert len(src.read_bytes())==row['bytes'] and sha(src.read_bytes())==row['sha256']
 target=view/p;target.parent.mkdir(parents=True,exist_ok=True);shutil.copyfile(src,target)
save('predecessor-view.json',dict(scope='EXACT_OLD_W2_MANIFEST_PROJECTION_NOT_LIVE_GIT',payloads=len(manifest['entries'])-1,manifest=pin(W2/'publication-manifest-closeout-v2.json'),source_aliases=mapped,view_token=view.name,original_queries=0))
patterns=[('preedit','evidence/04A38','test_preedit_reader.py'),('checkpoint','evidence/04A38','test_review_packet.py'),('continuation','evidence/04A38/continuation/v1','test_branch_audit.py'),
 ('A','evidence/04A38/late-archived-entry-supplement/v1','test_supplement.py'),('A-manifest','evidence/04A38/late-archived-entry-supplement/v1','test_publication.py'),
 ('B','evidence/04A38/late-archived-entry-supplement/v2-gee-profile','test_*.py'),('C','evidence/04A38/late-archived-entry-supplement/v3-mmrm-headered-profile','test_*.py'),
 ('D','evidence/04A38/late-archived-entry-supplement/v4-mmrm-duplicate-guards','test_*.py'),('E','evidence/04A38/late-archived-entry-supplement/v5-mmrm-unmatched-profile','test_*.py'),
 ('F','evidence/04A38/late-archived-entry-supplement/v6-mmrm-intercept-group','test_*.py'),('W1','evidence/04A38/completion/w1','test_*.py'),
 ('G','evidence/04A38/late-archived-entry-supplement/v7-consolidated-gee-spec-mmrm-wide','test_*.py'),('old-W2-bodies','evidence/04A38/completion/w2','test_independent_slice_v3.py'),('old-W2-publication','evidence/04A38/completion/w2','test_publication.py')]
results=[]
for label,path,pattern in patterns:
 files=[p.name for p in (view/path).glob(pattern)]
 if not files:
  results.append(dict(label=label,status='NOT_RUN_NO_MATCH',pattern=pattern));continue
 r=command('predecessor-'+label,[sys.executable,'-B','-m','unittest','discover','-s',path,'-p',pattern,'-v'],cwd=view)
 results.append(dict(label=label,files=files,command=clean(r)))
save('predecessor-readers.json',dict(scope='HISTORICAL_MANIFEST_COPY_WITH_EXACT_FORMER_SOURCE_VERSIONS;NOT_LIVE_SOURCE_TESTS',results=results,original_queries=0,invalid_failed_attempt_test_files='Preserved; broad test discovery may re-expose deliberately archived setup errors; inspect identities, never convert to PASS'))
print('Historical reader replay complete; inspect exact nonpasses')
