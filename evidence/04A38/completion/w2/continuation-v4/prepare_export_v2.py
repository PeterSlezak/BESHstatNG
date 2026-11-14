"""Preserve failed export; create new scripts/inventory with unchanged privacy rules."""
from common import *
old=HERE/'final_live.py';before=pin(old)
text=old.read_text(encoding='utf-8');phrase='/'.join(['UTF8','UTF16','JSON','home','key','token','URI'])
assert phrase in text
text=text.replace(phrase,'all encoded privacy patterns').replace('publication-manifest-v2.json','publication-manifest-v3.json').replace("stem+'review.zip'","stem+'review_v2.zip'")
with (HERE/'final_live_v2.py').open('x',encoding='utf-8') as f:f.write(text)
text=(HERE/'export_review.py').read_text(encoding='utf-8').replace('20261110_review.zip','20261110_review_v2.zip').replace("'export-physical-zip'","'export-physical-zip-v2'").replace('w2c4-export-physical-zip/','w2c4-export-physical-zip-v2/').replace("'c4-public-review-copy'","'c4-public-review-copy-v2'").replace("'c4-independent-copy.zip'","'c4-independent-copy-v2.zip'").replace("'export-copied-executable'","'export-copied-executable-v2'").replace('w2c4-export-copied-executable/','w2c4-export-copied-executable-v2/')
with (HERE/'export_review_v2.py').open('x',encoding='utf-8') as f:f.write(text)
save('publication-failed-attempt-v2.json',dict(status='FAILED_PHYSICAL_EXPORT_RETAINED_LOCALLY',exit_code=1,command='w2c4-export-physical-zip',first_zip='BESHstatNG_04A38_W2_continuation_v4_20261110_review.zip',rejected=before,reason='Descriptive slash-separated wording matched unchanged home-path pattern. No exception or relaxed policy. Original helper/ZIP retained locally and helper excluded from new derivative.',corrected=pin(HERE/'final_live_v2.py'),production_reference_changes=0))
