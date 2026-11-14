from common import *
for name in ['run_publication_checks.py','export_review.py','record_git_closeout.py']:
 s=(PREVIOUS/name).read_text(encoding='utf-8').replace('w2c3-','w2c4-').replace('continuation_v3','continuation_v4').replace('c3-public-review-copy','c4-public-review-copy').replace('c3-independent-copy','c4-independent-copy').replace('source-transitions-final.json','source-transitions.json')
 with (HERE/name).open('x',encoding='utf-8',newline='\n') as f:f.write(s)
