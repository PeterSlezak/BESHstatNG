from common import *
text=(HERE/'report_managed_v2.py').read_text(encoding='utf-8').replace('parametric-v4','cells-v3').replace('NEW_CONTINUATION_V4','NEW_CONTINUATION_V5').replace('INHERITED_C3','INHERITED_C4')
with (HERE/'report_managed_v3.py').open('x',encoding='utf-8',newline='\n') as stream:stream.write(text)
text=(HERE/'report_validation.py').read_text(encoding='utf-8').replace('w2c3-validation-v1','w2c4-validation-v1').replace('from_C3','from_C4').replace('1568736','1608882')
with (HERE/'report_validation_v2.py').open('x',encoding='utf-8',newline='\n') as stream:stream.write(text)
