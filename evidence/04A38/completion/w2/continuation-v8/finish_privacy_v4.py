"""Cover the retained generator's identical synthetic marker, with exact bounds."""
from common import *
def new(old,name,pairs):
    text=(HERE/old).read_text(encoding='utf-8')
    for a,b in pairs:assert a in text;text=text.replace(a,b)
    with (HERE/name).open('x',encoding='utf-8',newline='\n') as f:f.write(text)
new('publication_privacy_v2.py','publication_privacy_v3.py',[('privacy-authority-v2.json','privacy-authority-v3.json')])
new('prove_publication_privacy_v3.py','prove_publication_privacy_v4.py',[("'prepare_parallel_scan_v2.py','prove_publication_privacy_v1.py'","'prepare_parallel_scan_v2.py','prove_publication_privacy_v1.py','prepare_privacy_v2.py'"),("name=='prove_publication_privacy_v1.py'","name in ['prove_publication_privacy_v1.py','prepare_privacy_v2.py']"),('publication_privacy_v2','publication_privacy_v3'),('privacy-authority-v2.json','privacy-authority-v3.json'),('publication-privacy-equivalence-v3.json','publication-privacy-equivalence-v4.json'),('Three exact','Four exact'),('and16','and20')])
new('verify_c8_publication_v2.py','verify_c8_publication_v3.py',[('publication_privacy_v2','publication_privacy_v3')])
new('publish_v3.py','publish_v4.py',[('verify_c8_publication_v2','verify_c8_publication_v3')])
new('seal_and_controls_v3.py','seal_and_controls_v4.py',[('privacy-authority-v2.json','privacy-authority-v3.json'),('publication_privacy_v2.py','publication_privacy_v3.py'),('publication-privacy-equivalence-v3.json','publication-privacy-equivalence-v4.json')])
new('run_checkpoint_v3.py','run_checkpoint_v4.py',[('seal_and_controls_v3.py','seal_and_controls_v4.py')])
new('write_review_v2.py','write_review_v3.py',[('Three exact new synthetic','Four exact new synthetic')])
r=command('publication-privacy-proof-v4',[sys.executable,'-B',str(HERE/'prove_publication_privacy_v4.py')]);save('publication-privacy-proof-command-v4.json',clean(r));assert r['exit_code']==0
save('publication-tool-crosswalk-v3.json',dict(verifier=pin(HERE/'verify_c8_publication_v3.py'),publisher=pin(HERE/'publish_v4.py'),policy=pin(HERE/'publication_privacy_v3.py'),proof=pin(HERE/'publication-privacy-equivalence-v4.json'),scope='Four exact synthetic source positives: two original home/URI fixture generators and two empty key-marker negative-control source/generator files. Whole-file/category/literal bounds; all first attempts retained; historical tests unchanged.'))
