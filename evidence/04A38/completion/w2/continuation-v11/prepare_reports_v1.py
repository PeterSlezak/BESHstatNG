from common import *
source=(PREVIOUS/'report_managed_v1.py').read_text(encoding='utf-8').replace('w2c10','w2c11')
source=source.replace('C10','__C11__').replace('C9','C10').replace('__C11__','C11')
source=source.replace('len(added)==30','len(added)==29').replace('totals==Counter(Passed=20774,Failed=14)','totals==Counter(Passed=20803,Failed=14)')
source=source.replace('new_passes=30','new_passes=29').replace('new_PASS=30','new_PASS=29').replace('30 genuinely','29 genuinely')
source=source.replace('dict(Passed=20744,Failed=14)','dict(Passed=20774,Failed=14)')
with (HERE/'report_managed_v1.py').open('x',encoding='utf-8',newline='\n') as stream:stream.write(source)
source=(PREVIOUS/'report_validation_v1.py').read_text(encoding='utf-8').replace('w2c10','w2c11')
source=source.replace('C10','__C11__').replace('C9','C10').replace('__C11__','C11')
source=source.replace('dict(PASS=830,FAIL=115,ERROR=11)','dict(PASS=830,FAIL=113,ERROR=13)')
with (HERE/'report_validation_v1.py').open('x',encoding='utf-8',newline='\n') as stream:stream.write(source)
save('reporter-source-crosswalk-v1.json',dict(files=[dict(before=pin(PREVIOUS/n),after=pin(HERE/n)) for n in ['report_managed_v1.py','report_validation_v1.py']],
    scope='Exact C11 output labels, true v10 parent baseline830/113/13 and29 genuinely new managed cases. Preserve parent versus event units and all old/current hashes. No assertions weakened.',original_queries=0))
result=command('managed-identity-report-v1',[sys.executable,'-B',str(HERE/'report_managed_v1.py')])
save('managed-report-execution-v1.json',clean(result));assert result['exit_code']==0
