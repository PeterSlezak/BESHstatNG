"""Correct a generator filename substitution; retain rejected v2 attempt."""
from common import *
def new(old,name,pairs):
    text=(HERE/old).read_text(encoding='utf-8')
    for a,b in pairs:assert a in text;(None);text=text.replace(a,b)
    with (HERE/name).open('x',encoding='utf-8',newline='\n') as f:f.write(text)
new('prove_publication_privacy_v2.py','prove_publication_privacy_v3.py',[("'prove_publication_privacy_v2.py'","'prove_publication_privacy_v1.py'"),('publication-privacy-equivalence-v2.json','publication-privacy-equivalence-v3.json')])
new('seal_and_controls_v2.py','seal_and_controls_v3.py',[('publication-privacy-equivalence-v2.json','publication-privacy-equivalence-v3.json')])
new('run_checkpoint_v2.py','run_checkpoint_v3.py',[('seal_and_controls_v2.py','seal_and_controls_v3.py')])
r=command('publication-privacy-proof-v3',[sys.executable,'-B',str(HERE/'prove_publication_privacy_v3.py')]);save('publication-privacy-proof-command-v3.json',clean(r));assert r['exit_code']==0
save('publication-tool-crosswalk-v2.json',dict(verifier=pin(HERE/'verify_c8_publication_v2.py'),publisher=pin(HERE/'publish_v3.py'),policy=pin(HERE/'publication_privacy_v2.py'),proof=pin(HERE/'publication-privacy-equivalence-v3.json'),scope='Three exact synthetic adverse-control source positives, including an empty key-marker example; full-file/category/literal rejection controls. Earlier rejected proof v2 filename substitution retained. No historical assertion altered.'))
