"""Earlier readers require the authenticated pre-W2 source view, not W2 bytes."""
from common import *
view=RUNROOT/'w2-before-v2';assert view.is_dir()
originals=json.loads((ROOT/'evidence/04A38/preedit/entry-source-snapshot.json').read_bytes())['files']
for row in originals:assert (view/row['snapshot']['path']).read_bytes()==(ROOT/row['snapshot']['path']).read_bytes()
for row in json.loads((W2/'source-transitions.json').read_bytes())['files']:
 if row['before']:
  b=(view/row['path']).read_bytes();assert sha(b)==row['before']['sha256']
(view/'.test-rationalization-04A38.tmp').mkdir(exist_ok=True)
patterns=[('preedit','evidence/04A38','test_preedit_reader.py'),('checkpoint','evidence/04A38','test_review_packet.py'),('continuation','evidence/04A38/continuation/v1','test_branch_audit.py')]
for a,d in [('A','v1'),('B','v2-gee-profile'),('C','v3-mmrm-headered-profile'),('D','v4-mmrm-duplicate-guards'),('E','v5-mmrm-unmatched-profile'),('F','v6-mmrm-intercept-group'),('G','v7-consolidated-gee-spec-mmrm-wide')]:patterns.append((a,'evidence/04A38/late-archived-entry-supplement/'+d,'test_*.py'))
patterns.append(('W1','evidence/04A38/completion/w1','test_*.py'))
results=[]
for label,path,pattern in patterns:
 r=command('early-reader-'+label,[sys.executable,'-B','-m','unittest','discover','-s',path,'-p',pattern,'-v'],cwd=view)
 results.append(dict(label=label,command=clean(r),files=[p.name for p in (view/path).glob(pattern)]))
save('early-reader-replay.json',dict(scope='PRE_W2_INDEPENDENT_PRIVATE_SNAPSHOT;SOURCE_PINS_AUTHENTICATED;NOT_LIVE_CURRENT_SOURCE',results=results,new_original_queries=0,limitations='Older adverse test temporary files may exceed Windows path limits in nested views; preserve those setup errors separately from test-condition passes. No old source/assertion modified.'))
