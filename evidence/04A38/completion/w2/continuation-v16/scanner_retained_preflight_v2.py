"""Cheap retained-result/source-authority preflight, without scanning or rehashing the corpus."""
from common import *
from retained_scan_cache_v16_v2 import retained_inputs,validated_report
historical=retained_inputs()
failed=json.loads((W2/'continuation-v14/publication-git-index-binding-v1.json').read_bytes())['snapshot']
base=RUNROOT/'completion-w2-cont15/python-v1/1-owner-replacement-v1/parallel-scan-1'
count=0;receipts=[]
for n in range(1,6):
 folder=base/str(n);plan=json.loads((folder/'input.json').read_bytes());report=json.loads((folder/'raw-result.json').read_bytes())
 count+=len(validated_report(report,plan,failed,historical,plan['authority']))
 receipts.append(dict(plan=pin(folder/'input.json'),result=pin(folder/'raw-result.json')))
assert count==5829
save('scanner-retained-input-preflight-v2.json',dict(status='ALL5829_RETAINED_SUCCESS_ROWS_BOUND_TO_IMMUTABLE_C15_INPUT_AUTHORITIES',successful_rows=count,receipts=receipts,source_kernel=pin(HERE/'retained_scan_cache_v16_v2.py'),original_scanner_calls=0,repository_corpus_rehashes=0,scope='Only small recorded plan/result/index metadata parsed. Real source/environment/current-byte validation happens once inside the ordinary scanner lane. Frozen self-document and later additive inputs use actual immutable final C15 index/exclusion pins, without a manufactured recursive hash.',original_queries=0))
print('Retained5829-row authority preflight PASS; no scanner or test execution',flush=True)
