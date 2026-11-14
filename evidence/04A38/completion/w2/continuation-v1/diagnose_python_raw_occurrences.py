from collections import Counter
from common import *
from baseline_report import python_receipt
old_groups=[python_receipt('w2-validation-v1-'+s) for s in ['python-tools','python-help','python-scripts','common-python']]
old=[r for g in old_groups for r in g['nonpasses']]
for label in ['13','14']:
 rerun=python_receipt('w2-privacy-artifact-correction-'+label)
 for record in rerun['nonpasses']:
  old=[r for r in old if r['identity']!=record['identity']]+[record]
groups=[python_receipt('w2c1-validation-v1-'+s) for s in ['python-tools','python-help','python-scripts','common-python']]
key=lambda r:(r['outcome'],r['identity'],r['subtest'])
a=Counter(key(r) for r in old);b=Counter(key(r) for g in groups for r in g['nonpasses'])
delta=[dict(outcome=k[0],identity=k[1],subtest=k[2],extra_occurrences=n) for k,n in (b-a).items()]
lost=[dict(key=k,count=n) for k,n in (a-b).items()]
save('attempts/python-v1-raw-occurrence-delta.json',dict(extra=delta,lost=lost,old_nonpass_occurrences=sum(a.values()),current_nonpass_occurrences=sum(b.values()),qualification='Reconstructed from retained original W2 raw logs plus the two targeted privacy reruns; old identity-only dictionary omitted repeated subtests and is preserved, not rewritten. Failed occurrence comparison against that lossy dictionary is explicitly superseded here.'))
print(json.dumps(dict(extra=delta,lost=lost,old_count=sum(a.values()),current_count=sum(b.values()))))
