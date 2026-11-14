from common import *
source=(PREVIOUS/'test_readers_v2.py').read_text(encoding='utf-8')
prefix='''import sys
from pathlib import Path
C5 = Path(__file__).resolve().parent
C5ROOT = C5.parents[4]
sys.path.insert(0,str(C5.parent/'continuation-v4'))
'''
source=source.replace('from verify_slice_v5 import ROOT,HERE,inspect','from verify_slice_v5 import ROOT,HERE,inspect as prior_slice')
source=source.replace('from verify_ownership_v2 import inspect as ownership','from verify_ownership_v2 import inspect as prior_ownership')
source=source.replace('from verify_text_domains_v2 import inspect as text','from verify_text_domains_v2 import inspect as prior_text')
wrapper='''
entry=json.loads((C5/'entry-v2.json').read_bytes())
historical_source={r['live']['path']:(C5ROOT/r['preimage']['path']).read_bytes() for r in entry['source_before']}
def projection(reader,root=ROOT,overrides=None):
 merged=dict(historical_source);merged.update(overrides or {})
 return reader(root,merged)
def inspect(root=ROOT,overrides=None):return projection(prior_slice,root,overrides)
def ownership(root=ROOT,overrides=None):return projection(prior_ownership,root,overrides)
def text(root=ROOT,overrides=None):return projection(prior_text,root,overrides)
'''
source=source.replace("BASE=HERE.relative_to(ROOT).as_posix()+'/'",wrapper+"\nBASE=HERE.relative_to(ROOT).as_posix()+'/'")
with (HERE/'C4_controls_projected_v2.py').open('x',encoding='utf-8',newline='\n') as stream:stream.write(prefix+source)
r=command('retained-c4-controls-v2',[sys.executable,'-B','-m','unittest','discover','-s',str(HERE),'-p','C4_controls_projected_v2.py','-v'])
save('retained-C4-controls-v2.json',dict(command=clean(r),passed=r['exit_code']==0,
    projection='Exact retained preimages for the seven C5 modified existing source/test files. All mutations overlay after projection. Historical tests unchanged except explicit reader adapters.',
    original_queries=0))
assert r['exit_code']==0
