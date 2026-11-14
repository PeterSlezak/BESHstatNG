"""Version source-stable C10 validation runners; preserve prior algorithms."""
from common import *
for name in ['validate_managed_v2.py','report_managed_v1.py','run_auxiliary_v1.py']:
    source=(PREVIOUS/name).read_text(encoding='utf-8').replace('from common_v2 import *','from common import *').replace('w2c9','w2c10').replace('cont9','cont10')
    source=source.replace('compatibility-packet-v1.json','compatibility-packet-v2.json')
    if name=='validate_managed_v2.py':
        source=source.replace("['focused-v3-packet-v1-Debug.json','focused-corrective-v1.json']","['focused-v3-packet-A-v1-Debug.json','focused-BC-packet-v2-Debug.json','focused-copy-corrective-v2.json']")
    if name=='report_managed_v1.py':
        source=source.replace('NEW_C9','NEW_C10').replace('PRESERVED_C8_IDENTITY','PRESERVED_C9_IDENTITY').replace('EXACT_C8','EXACT_C9').replace('len(added)==24','len(added)==30').replace('Passed=20744','Passed=20774').replace('new_passes=24','new_passes=30').replace('comparison_C8','comparison_C9').replace('Passed=20720','Passed=20744').replace('24 genuinely','30 genuinely').replace('old C8','old C9').replace('new_PASS=24','new_PASS=30')
    with (HERE/name).open('x',encoding='utf-8',newline='\n') as f:f.write(source)
source=(HERE/'verify_c10_checkpoint_template_v1.py').read_text(encoding='utf-8').replace('inspection/packet-v1/','inspection/packet-v2/').replace('build-packet-v1-','build-packet-v2-')
source=source.replace('Unexpected C8 to C9','Unexpected C9 to C10').replace('Lost original C8 compiled','Lost prior C9 compiled').replace('retained_C8_methods','retained_C9_methods')
with (HERE/'verify_c10_checkpoint_template_v2.py').open('x',encoding='utf-8',newline='\n') as f:f.write(source)
save('campaign-runner-crosswalk-v1.json',dict(files=[dict(before=pin(PREVIOUS/n),after=pin(HERE/n)) for n in ['validate_managed_v2.py','report_managed_v1.py','run_auxiliary_v1.py']],reader=dict(first=pin(HERE/'verify_c10_checkpoint_template_v1.py'),corrected=pin(HERE/'verify_c10_checkpoint_template_v2.py')),scope='C10 paths/baseline and actual final packet-v2 shipping outputs. No production/test/assertion changes; the first reader remains retained. Comprehensive managed campaign preserves the same complementary Common fixture partition.',original_queries=0))
