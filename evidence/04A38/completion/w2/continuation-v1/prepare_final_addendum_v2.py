from common import *
s=(HERE/'final_addendum.py').read_text().replace("\nwith (HERE/'owner-review-closeout-v2.md')", "\n with (HERE/'owner-review-closeout-v2.md')")
with (HERE/'final_addendum_v2.py').open('x',encoding='utf-8') as f:f.write(s)
save('attempts/final-addendum-setup-v1.json',dict(exit_code=1,error='IndentationError: unexpected indent',source=pin(HERE/'final_addendum.py'),line=17,product_or_evidence_payloads_changed=False,correction='New version indents the final Markdown write inside main; retain failed source.'))
