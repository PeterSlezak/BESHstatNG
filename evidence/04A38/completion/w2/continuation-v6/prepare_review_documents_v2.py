"""Correct two locale-decoded punctuation sequences in new review versions only."""
from common import *
records=[]
for old,new in [('owner-review.md','owner-review-v2.md'),('NEXT_WAVE_HANDOVER.md','NEXT_WAVE_HANDOVER-v2.md')]:
 text=(HERE/old).read_text(encoding='utf-8');fixed=text.replace('\u00e2\u20ac\u201d','\u2014').replace('\u00e2\u2020\u2019','\u2192')
 assert fixed!=text and fixed.startswith('NOT_READY_04A38 \u2014 W2 advanced; W3 NOT_STARTED')
 if old=='owner-review.md':fixed=fixed.replace('NEXT_WAVE_HANDOVER.md','NEXT_WAVE_HANDOVER-v2.md')
 else:fixed=fixed.replace('Read the owner review','Read owner-review-v2.md')
 with (HERE/new).open('x',encoding='utf-8',newline='\n') as f:f.write(fixed)
 records.append(dict(prior=pin(HERE/old),authoritative=pin(HERE/new)))
save('review-document-authority-v1.json',dict(status='FINAL_REVIEW_DOCUMENTS_V2',records=records,reason='An early default-codepage read of the new review-writing helper decoded UTF8 punctuation as CP1252. Production code and proofs unaffected. Prior generated documents retained; v2 corrects only those two punctuation sequences and final cross-links.',owner_documents_changed=0,production_sources_changed=0))
