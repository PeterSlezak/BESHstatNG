"""Prepare one final six-suite Debug campaign and unchanged checkpoint-only auxiliaries."""
from common import *
source=(PREVIOUS/'validate_managed_v3.py').read_text(encoding='utf-8').replace('w2c10','w2c11')
source=source.replace("['focused-v3-packet-A-v1-Debug.json','focused-bc-v1-packet-v2-Debug.json','focused-copy-corrective-v2.json']",
    "['focused-bc-v1-packet-v2-Debug.json']")
with (HERE/'validate_managed_v1.py').open('x',encoding='utf-8',newline='\n') as stream:
    stream.write(source)
source=(PREVIOUS/'run_auxiliary_v1.py').read_text(encoding='utf-8').replace('w2c10','w2c11')
with (HERE/'run_auxiliary_v1.py').open('x',encoding='utf-8',newline='\n') as stream:
    stream.write(source)
save('campaign-runner-crosswalk-v1.json',dict(managed=dict(before=pin(PREVIOUS/'validate_managed_v3.py'),
    after=pin(HERE/'validate_managed_v1.py')),auxiliary=dict(before=pin(PREVIOUS/'run_auxiliary_v1.py'),
    after=pin(HERE/'run_auxiliary_v1.py')),scope='Only C11 output labels and actual focused receipts. One full Debug campaign on frozen sources, complementary Common environments, distinct private paths; no Release units, restore or packaging.',original_queries=0))
