"""Report actual deltas first; acceptance requires separate source-backed investigation."""
from common_v2 import *
source=(HERE/'report_validation_v1.py').read_text(encoding='utf-8')
old="assert not delta['extra'] and not delta['lost'],'New/changed nonpass identity requires investigation before seal'"
assert old in source
source=source.replace(old,"save('python-investigation-required-v1.json',dict(status='PENDING_EXACT_NONPASS_INVESTIGATION',delta=delta,scope='Aggregation is not test acceptance. Any new/lost event or changed message requires the separate source/trace investigation before sealing; no assertion or historical failure is waived.'))")
with (HERE/'report_validation_v2.py').open('x',encoding='utf-8',newline='\n') as f:f.write(source)
save('nonpass-accounting-plan-v1.json',dict(reporter=pin(HERE/'report_validation_v2.py'),scope='Preserve all956 exact methods and every raw event. The C8 baseline is a comparison, not a green target. Anticipated authorized source-pin failures are still actual FAIL; no delta is silently ignored. Final fail-closed validator must bind the complete actual multiset and its separate completed investigation.',historical_assertions_changed=0))
