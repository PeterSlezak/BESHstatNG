"""New manifest adversarial/public-copy checks, preserving the sealed v1."""
from capture_current import *
s=(G.parent/'v6-mmrm-intercept-group/test_packet.py').read_text(encoding='utf-8').replace('import verify_packet as reader','import verify_publication as reader').replace('VALID_NOT_READY_MMRM_PUBLICATION','VALID_W2_PARTIAL_PUBLICATION_NOT_READY').replace('reader.PREFIX','reader.W').replace("'current-results/Release/results.tsv'","'preedit-current-results/Release/frozen-v2/results.tsv'")
with (HERE/'test_publication.py').open('x',encoding='utf-8') as f:f.write(s)
r=m.Runner(RUNROOT).command('w2-publication-adverse',[sys.executable,'-B','-m','unittest','discover','-s',str(HERE),'-p','test_publication.py','-v']);assert r['exit_code']==0
