"""Complete only the unstarted README write after preserved missing-directory report stop."""
from common39 import *
import ast

for name in ['final/readiness.json','final/owner-review.md','final/next-gate-handover.md']:
    assert (HERE/name).is_file()
assert load(HERE/'final/readiness.json')['status']=='NOT_READY_04A39'
tree=ast.parse((HERE/'write-dossier-v2.py').read_text(encoding='utf-8'))
values=[x.value for x in ast.walk(tree) if isinstance(x,ast.Constant) and isinstance(x.value,str) and x.value.startswith('NOT_READY39 review derivative.')]
assert len(values)==1
(HERE/'publication').mkdir(exist_ok=True)
with (HERE/'publication/README-v1.md').open('x',encoding='utf-8',newline='\n') as stream:stream.write(values[0])
save('final/dossier-continuation-v1.json',dict(status='ONLY_UNSTARTED_README_COMPLETED',
    existing=[pin(HERE/name) for name in ['final/readiness.json','final/owner-review.md','final/next-gate-handover.md']],
    readme=pin(HERE/'publication/README-v1.md'),
    source=pin(HERE/'write-dossier-v2.py'),reason='Writer completed all final result files,then failed because publication directory did not yet exist. Existing outputs were not overwritten; no campaign or verifier replay.'))
source_lock()
