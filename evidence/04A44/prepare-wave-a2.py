from common44 import *
s=(HERE/'wave-v1.py').read_text().replace("label='a44-wave-'+wave+'-v1'","label='a44-wave-'+wave+'-v2'").replace("'-build-v1.json'","'-build-v2.json'").replace("'wave-'+wave+'-fixtures'","'wave-'+wave+'-fixtures-v2'").replace("wave+'-v1.json'","wave+'-v2.json'")
with (HERE/'wave-v2.py').open('x',encoding='utf-8',newline='\n') as f:f.write(s)
save('waves/failed-a-v1.json',dict(status='RETAINED_TEST_SOURCE_COMPILE_FAILURE;NOT_PRODUCTION_REGRESSION',code='BC30035',failed_test=pin(PRIVATE/'failed-wave-a-test-source-v1.vb'),build=pin(PRIVATE/'wave-a-build-v1.json'),correction='VB standalone New expression replaced by local declaration and invocation;existing assertions unchanged;no full campaign had run.',gates=GATES))
