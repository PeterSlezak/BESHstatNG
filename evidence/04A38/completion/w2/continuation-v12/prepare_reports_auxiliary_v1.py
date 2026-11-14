from common import *
rows=[]
for name in ['report_managed_v1.py','run_auxiliary_v1.py']:
    source=PREVIOUS/name;value=source.read_text(encoding='utf-8')
    value=value.replace('w2c11-checkpoint-v1','w2c12-checkpoint-v1')
    if name=='report_managed_v1.py':
        value=value.replace('effective C7','v11').replace('NEW_C11','NEW_C12').replace('C10','C11')
        value=value.replace('len(added)==29','len(added)==31').replace('new_passes=29','new_passes=31').replace('new_PASS=29','new_PASS=31')
        value=value.replace('Passed=20803','Passed=20834').replace('Passed=20774','Passed=20803').replace('29 genuinely added','31 genuinely added')
    target=HERE/name
    with target.open('x',encoding='utf-8',newline='\n') as stream:stream.write(value)
    rows.append(dict(before=pin(source),after=pin(target),scope='C12 labels and measured31 new Debug identities; assertions unchanged in strength, exact prior multiset/messages required.'))
save('report-auxiliary-tools-crosswalk-v1.json',dict(rows=rows,original_queries=0))
