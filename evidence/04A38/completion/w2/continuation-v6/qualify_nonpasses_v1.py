"""Version the exact source-pin qualification; preserve every prior outcome."""
from common import *
detail=json.loads((HERE/'python-first-failures-v1.json').read_bytes());nonpasses=json.loads((HERE/'nonpasses-v1.json').read_bytes());pin_delta=json.loads((HERE/'python-source-pin-delta-v1.json').read_bytes())
found=[]
for record in detail['records']:
 if record['identity']==pin_delta['identity']:
  record['classification']='NEW_AUTHORIZED_C6_SOURCE_BODY_PIN_FAIL;NOT_WAIVED';found.append(record)
assert len(found)==1 and found[0]['outcome']=='FAIL'
save('python-first-failures-v2.json',dict(detail,authority='v2 corrects only the one new failure classification; all original outcomes/locations/messages/trace hashes remain exact',block_hash_convention='Decoded traceback block, CRLF→LF, re-encoded UTF8; exact whole raw stderr hashes retained separately'))
nonpasses.update(source_pin_delta=pin(HERE/'python-source-pin-delta-v1.json'),new_source_pin_nonpasses=1,changed_python_classification='C4→C5 unordered-SPC movement remains explained. C6 retains SPC ERROR at line68 with KeyError ValidateFiniteNonnegative. C6 also adds one correctly retained historical source-pin FAIL at test_04a27_transport_successor.py:60 for two authorized paired-import RowsToMatrix transitions. No exception relabeling or historical test/evidence rewrite.',authoritative_first_failures='python-first-failures-v2.json')
save('nonpasses-v2.json',nonpasses)
save('attempts/python-report-finalization-v1.json',dict(rejected_report=pin(HERE/'report_validation_v1.py'),
 investigation_v1='Rejected speculative three-member affected set; actual source projection proves exactly two changed members. ExtractCompletePairedNumericRows already delegated and stayed exact.',
 investigation_v2='Expected LF in raw CRLF stderr; no trace matched. Corrected new v3 reader explicitly normalizes newline view and keeps whole raw hashes.',
 report_v2='Dependent proof absent because v2 investigation rejected; no final receipt written.',
 report_v3='PASS: all192 current nonpass trace blocks accounted;764 PASS/181 FAIL/11 ERROR;916 complete Node identities including the formerly omitted multiline identity.',
 source_changes_during_investigation=0,historical_test_changes=0,original_queries=0))
