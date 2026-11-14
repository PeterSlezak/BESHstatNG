from collections import Counter
from common import *
from baseline_report import python_receipt
old=json.loads((W2/'nonpasses.json').read_bytes())['python_reconciled_nonpasses']
groups=[python_receipt('w2c1-validation-v1-'+s) for s in ['python-tools','python-help','python-scripts','common-python']]
key=lambda r:(r['outcome'],r['identity'],r['subtest'])
a=Counter(key(r) for r in old);b=Counter(key(r) for g in groups for r in g['nonpasses'])
delta=[dict(outcome=k[0],identity=k[1],subtest=k[2],extra_occurrences=n) for k,n in (b-a).items()]
save('attempts/python-v1-occurrence-delta.json',dict(extra=delta,lost=[dict(key=k,count=n) for k,n in (a-b).items()],rule='Identity AND subtest/occurrence; identity-only deduplication cannot hide an added source-pin failure'))
print(json.dumps(delta));d=json.loads((HERE/'tracked-path-preservation.json').read_bytes());print({k:d[k] for k in ['original_entry_exact','authorized_transitions','continuation_further_transitions']})
