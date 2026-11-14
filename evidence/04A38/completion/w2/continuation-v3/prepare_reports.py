from common import *
s=(PREVIOUS/'report_managed.py').read_text(encoding='utf-8').replace('w2c2-validation-v1','w2c3-validation-v1').replace('b1-v1','names-v3').replace('NEW_CONTINUATION_V2','NEW_CONTINUATION_V3').replace('INHERITED_C1','INHERITED_C2')
with (HERE/'report_managed.py').open('x',encoding='utf-8',newline='\n') as f:f.write(s)
s=(PREVIOUS/'run_full_validation.py').read_text(encoding='utf-8').replace('w2c2-validation-v1','w2c3-validation-v1')
# Existing copied script already uses this label; no source/project wrapper invocation here.
save('report-adaptation.json',dict(previous=pin(PREVIOUS/'report_managed.py'),new=pin(HERE/'report_managed.py'),policy='Retain every inherited test identity/occurrence/outcome; actual current new cases counted from raw TRX. No assumed cardinality.'))
