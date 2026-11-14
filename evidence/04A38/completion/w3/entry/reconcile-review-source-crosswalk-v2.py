"""Separate direct derivative coverage from live freeze verification."""
from common import *
from collections import Counter
rows=load(ACCEPTED/'production-test-freeze-v1.json')['sources']
special=[r for r in rows if 'package' in r['path'].lower() or 'install' in r['path'].lower()]
save('entry/review-source-crosswalk-v2.json',dict(complete_live_freeze_verified=1350,
     all_live_bytes_match=all(pin(ROOT/r['path'])==r for r in rows),
     support_candidates=special,support_count=len(special),
     external_review_assertion=dict(accessible=1332,live_only=18,source='Owner W3 prompt; external reviewer artifact not yet located'),
     direct_index_coverage=518,external_source_coverage='NOT_VERIFIABLE_FROM_CORE_INDEX_ALONE',
     v1_disposition='REPORT_ONLY_FALSE_ASSUMPTION: core index is a complementary review derivative, not a complete source checkout. All1350 actual source files were already verified; no production/source-lock mismatch.',
     count_by_top_directory=dict(Counter(r['path'].split('/')[0] for r in rows))))
print('Support paths',len(special),json.dumps([r['path'] for r in special]),flush=True)
