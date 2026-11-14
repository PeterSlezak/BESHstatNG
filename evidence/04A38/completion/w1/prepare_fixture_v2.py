"""Fix fixture completeness, preserving every assertion and the failed attempt."""
from pathlib import Path
import hashlib,json
HERE=Path(__file__).resolve().parent;ROOT=HERE.parents[3]
test=HERE/'test_w1.py';data=test.read_bytes();text=data.decode()
needle="  for path in paths:\n"
assert text.count(needle)==1
text=text.replace(needle,"  paths.update(r['path'] for r in json.loads((reader.ROOT/(reader.W+'current-universe.json')).read_bytes())['sources'])\n"+needle)
needle=' def change(self,name,operation):'
text=text.replace(needle,"  if reader.inspect(cls.view)['status']!='VALID_W1_F_PASS_FULL_UNIVERSE_CONSOLIDATED_STOP':raise AssertionError('Intact fixture not valid before corruption')\n"+needle)
with (HERE/'test_w1_v2.py').open('x',encoding='utf-8',newline='\n') as f:f.write(text)
with (HERE/'adverse-attempt-v1.json').open('x',encoding='utf-8') as f:json.dump(dict(status='FAILED_INVALID_FIXTURE;NOMINAL_REJECTIONS_NOT_ACCEPTED_AS_PROOF',exit_code=1,total=18,nominal_pass=17,error=1,
 exception='Intact fixture lacked unchanged AgreementMethodUDFs.vb because previous manifest did not publish every unchanged live UDF file',
 preserved_test_sha256=hashlib.sha256(data).hexdigest(),raw_receipt='mmrm38-v6-w1-adverse',correction='Include all41 current source files; assert intact fixture passes before any mutation. No verifier, case expectation, original bytes or adversarial assertion relaxed.'),f,indent=2)
text=(HERE/'validate_w1.py').read_text(encoding='utf-8').replace('w1-independent-reader','w1-independent-reader-v2').replace("'w1-adverse'","'w1-adverse-v2'").replace("'test_w1.py'","'test_w1_v2.py'").replace('validation-attempt-v1.json','validation-attempt-v2.json')
with (HERE/'validate_w1_v2.py').open('x',encoding='utf-8',newline='\n') as f:f.write(text)
