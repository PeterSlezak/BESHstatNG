from common44 import *
s=(HERE/'wave-v2.py').read_text().replace('-v2','-v3')
with (HERE/'wave-v3.py').open('x',encoding='utf-8',newline='\n') as f:f.write(s)
save('waves/failed-a-v2.json',dict(status='RETAINED_NEW_TEST_COMPILE_FAILURE;NO_PRODUCTION_CHANGE',codes=['BC31539','BC30112'],source=pin(PRIVATE/'failed-wave-a-test-source-v2.vb'),build=pin(PRIVATE/'wave-a-build-v2.json'),correction='Remove test-only embedded Excel.Application type access;explicitly type sample loop variable to avoid System.Text namespace resolution. No existing test changed.',gates=GATES))
