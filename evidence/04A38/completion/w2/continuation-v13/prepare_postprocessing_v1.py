from common import *
source=PREVIOUS/'report_validation_v1.py';value=source.read_text(encoding='utf-8').replace('w2c12','w2c13').replace('C11_actual_parent_baseline','C12_actual_parent_baseline').replace('INHERITED_C11','INHERITED_C12').replace('NEW_C12','NEW_C13')
with (HERE/'report_validation_v1.py').open('x',encoding='utf-8',newline='\n') as stream:stream.write(value)
save('validation-accounting-source-crosswalk-v1.json',dict(before=pin(source),after=pin(HERE/'report_validation_v1.py'),scope='Exact raw identity/event/worker/Node reconciliation, current labels and actual v12 baseline. No historical test or assertion change.',original_queries=0))
result=command('validation-identity-report-v1',[sys.executable,'-B',str(HERE/'report_validation_v1.py')]);save('validation-report-execution-v1.json',clean(result));assert result['exit_code']==0
