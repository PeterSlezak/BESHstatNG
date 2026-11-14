"""Retain one additional exact empty key-marker negative-control source."""
from common import *
def new(old,name,replacements):
    text=(HERE/old).read_text(encoding='utf-8')
    for before,after in replacements:
        assert before in text,(old,before);text=text.replace(before,after)
    with (HERE/name).open('x',encoding='utf-8',newline='\n') as f:f.write(text)
new('publication_privacy_v1.py','publication_privacy_v2.py',[
('privacy-authority-v1.json','privacy-authority-v2.json'),
("['home','credential_uri']","['home','credential_uri','private_key']"),
("need(collapsed==expected", "if kind=='private_key':expected='-----'+'BEGIN '+'PRIVATE KEY'+'-----'\n            need(collapsed==expected")])
new('prove_publication_privacy_v1.py','prove_publication_privacy_v2.py',[
("['prove_parallel_scan_v1.py','prepare_parallel_scan_v2.py']","['prove_parallel_scan_v1.py','prepare_parallel_scan_v2.py','prove_publication_privacy_v1.py']"),
("assert set(kinds)=={'home','credential_uri'},(name,kinds)","assert set(kinds)==({'private_key'} if name=='prove_publication_privacy_v1.py' else {'home','credential_uri'}),(name,kinds)"),
('privacy-authority-v1.json','privacy-authority-v2.json'),('publication_privacy_v1','publication_privacy_v2'),('publication-privacy-equivalence-v1.json','publication-privacy-equivalence-v2.json'),
("b'-----BEGIN PRIVATE KEY-----'","('-----'+'BEGIN '+'PRIVATE KEY'+'-----').encode()"),
('Two exact new synthetic control source files','Three exact new synthetic control source files'),('cache and12','cache and16')])
new('verify_c8_publication_v1.py','verify_c8_publication_v2.py',[('publication_privacy_v1','publication_privacy_v2')])
new('publish_v2.py','publish_v3.py',[('verify_c8_publication_v1','verify_c8_publication_v2')])
new('seal_and_controls_v1.py','seal_and_controls_v2.py',[("'privacy-authority-v1.json','publication_privacy_v1.py','publication-privacy-equivalence-v1.json'","'privacy-authority-v2.json','publication_privacy_v2.py','publication-privacy-equivalence-v2.json'")])
new('run_checkpoint_v1.py','run_checkpoint_v2.py',[('seal_and_controls_v1.py','seal_and_controls_v2.py')])
new('write_review_v1.py','write_review_v2.py',[('Two exact new synthetic adverse-control source files retain bounded positive home/URI examples','Three exact new synthetic adverse-control source files retain bounded positive home/URI and empty private-key-marker examples')])
result=command('publication-privacy-proof-v2',[sys.executable,'-B',str(HERE/'prove_publication_privacy_v2.py')]);save('publication-privacy-proof-command-v2.json',clean(result));assert result['exit_code']==0
save('publication-tool-crosswalk-v2.json',dict(verifier=pin(HERE/'verify_c8_publication_v2.py'),publisher=pin(HERE/'publish_v3.py'),policy=pin(HERE/'publication_privacy_v2.py'),proof=pin(HERE/'publication-privacy-equivalence-v2.json'),scope='One additional exact empty synthetic key-marker control source; full-file/category/literal bounds and changed-byte/path/profile negatives. Earlier source/policy/proof retained, no historical assertion altered.',first_attempt='Prepublication source-only privacy check rejected the empty synthetic marker; conversation transcript retained.'))
