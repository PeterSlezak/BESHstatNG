from common import *
from baseline_report import python_receipt
from collections import Counter
labels=['w2c4-validation-v1-python-scripts','w2c5-validation-v1-python-scripts','w2c5-validation-v2-python-scripts','w2c5-final-python-privacy13-after-encoding']
rows=[]
for label in labels:
    r=python_receipt(label);rows.append(r)
    print(label,{k:r[k] for k in ['total','passed','failed','errors']})
    if label.startswith('w2c5'):
        print('privacy outcomes',[p for p in r['nonpasses'] if 'publication_set' in p['identity'] or 'proposed_publication' in p['identity']])
old=Counter((p['outcome'],p['identity'],p['subtest']) for p in rows[0]['nonpasses'])
now=Counter((p['outcome'],p['identity'],p['subtest']) for p in rows[2]['nonpasses'])
print('raw final extra',list((now-old).items()));print('raw final lost',list((old-now).items()))
save('python-raw-accounting-before-correction.json',dict(receipts=rows,raw_extra=[dict(identity=k,occurrences=v) for k,v in (now-old).items()],raw_lost=[dict(identity=k,occurrences=v) for k,v in (old-now).items()]))
