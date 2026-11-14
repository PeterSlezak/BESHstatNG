"""Include the exact first missing-reference harness attempt in final accounting."""
from common_v2 import *
source=(HERE/'record_receipts_v3.py').read_text(encoding='utf-8').replace("attempts=[", "attempts=[dict(name='Missing earlier reference rejection harness',classification='INHERITED_READER_ALREADY_FAILED_CLOSED',receipt='control-harness-correction-v1.json',outcome='Missing None bytes raised inherited TypeError; the first harness did not catch that rejection. Only this directed control accepts that error in v4; other TypeErrors remain failures.'),")
with (HERE/'record_receipts_v4.py').open('x',encoding='utf-8',newline='\n') as f:f.write(source)
print('Final attempt accounting source prepared')
