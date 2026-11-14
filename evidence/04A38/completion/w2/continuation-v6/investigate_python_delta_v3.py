"""Explain the newly observed historical body-pin failure without changing it."""
from common import *
import importlib.util,re
spec=importlib.util.spec_from_file_location('historical_27_pin_reader',ROOT/'scripts/verify-04a27-contract.py')
module=importlib.util.module_from_spec(spec);spec.loader.exec_module(module)
data=json.loads(module.DEST.read_bytes());baseline=json.loads((module.RUN/'baseline.json').read_bytes())
actual=module.current_members(baseline['main']);transitions=json.loads((HERE/'source-transitions-final-v1.json').read_bytes())['files']
reader=next(r for r in transitions if r['path'].endswith('/UdfDataImportRangeReaders.vb'))
assert pin(ROOT/reader['path'])==reader['after'] and pin(ROOT/reader['preimage']['path'])==reader['preimage']
original_root=module.ROOT;module.git=git
class EntryView:
 def __truediv__(self,path):return ROOT/reader['preimage']['path'] if path==reader['path'] else ROOT/path
module.ROOT=EntryView();entry=module.current_members(baseline['main']);module.ROOT=original_root
assert entry==data['current_member_anchors'],'Historical pin was already different at C6 entry: investigate separately'
changes=[];contracts=json.loads((HERE/'whole-body-contracts-v1.json').read_bytes())['contracts']
for old,new in zip(entry,actual):
 if old==new:continue
 assert old['path']==new['path']==reader['path'] and all(old[k]==new[k] for k in old if k!='after_sha256')
 contract=next(r for r in contracts if r['name']==new['member'] and r['path']==new['path'])
 assert contract['new_support'] and contract['original_id']
 changes.append(dict(member=new['member'],canonical_original_id=contract['original_id'],current_id=contract['current_id'],entry=old,current=new,proof='whole-body-contracts-v1.json; source-composition-proof-v1.json; actual Core RowsToMatrix consumer/CIL and all frozen outputs exact'))
assert {r['member'] for r in changes}=={'TryReadPairedNumericColumns','ExtractPairedNumericColumnsIgnoringNonNumeric'}
raw=(RUNROOT/'commands/w2c6-final-python-scripts-cached-v1/stderr.log').read_bytes();text=raw.decode('utf-8-sig').replace('\r\n','\n')
identity='test_original_member_ids_and_body_pins_not_fabricated (test_04a27_transport_successor.Transport27SuccessorTests.test_original_member_ids_and_body_pins_not_fabricated)'
block=re.search(r'^FAIL: '+re.escape(identity)+r'\n-+\n(.*?)(?=\n={5,}|\Z)',text,re.M|re.S).group(1)
save('python-source-pin-delta-v1.json',dict(status='NEW_AUTHORIZED_C6_SOURCE_PIN_FAIL_EXPLAINED_NOT_WAIVED',identity=identity,outcome='FAIL',occurrences=1,first_location='scripts/tests/test_04a27_transport_successor.py:60',
 original_historical_test=pin(ROOT/'scripts/tests/test_04a27_transport_successor.py'),historical_contract=pin(ROOT/'scripts/verify-04a27-contract.py'),historical_record=pin(module.DEST),
 entry_projection_equals_historical_anchors=True,changed_members=changes,transition=reader,raw_stderr_sha256=sha(raw),exact_trace=clean(block),
 cause='Historical current-member hash anchors matched the C6 preimage. Two complete paired-import bodies now delegate matrix copying to existing Core RowsToMatrix under the authorized C6 transition. ExtractCompletePairedNumericRows already delegated and its body pin is unchanged. The historical equality assertion correctly fails and stays FAIL. Numerical/output compatibility is separately exact; no test or older ledger is rewritten.',
 count_effect=dict(passed=-1,failed=1,errors=0),new_original_semantic_queries=0))
save('attempts/rejected-validation-reconciliation-v1.json',dict(producer=pin(HERE/'report_validation_v1.py'),result='REJECTED_UNEXPECTED_NEW_FAIL',identity=identity,reason='Guard allowed only the already known SPC outcome movement. Investigation now proves a separate new authorized source-pin failure; this rejected attempt remains intact.'))
print('New FAIL explained: two authorized paired-import Core RowsToMatrix body transitions; FAIL retained')
