from common import *
from baseline_report import python_receipt
old=json.loads((W2/'nonpasses.json').read_bytes())['python_reconciled_nonpasses'];before={r['identity']:r for r in old}
groups=[python_receipt('w2c1-validation-v1-'+s) for s in ['python-tools','python-help','python-scripts','common-python']];now={r['identity']:r for g in groups for r in g['nonpasses']}
delta=[dict(identity=k,before=before[k]['outcome'] if k in before else 'ABSENT',after=v['outcome'],record=v) for k,v in now.items() if k not in before or before[k]['outcome']!=v['outcome']]
save('attempts/python-v1-delta.json',dict(groups=groups,delta=delta,source_state='First source audit overlapped calibration correction; retained as first run, not final state'))
print(json.dumps(delta))
