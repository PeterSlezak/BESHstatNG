"""Additive schema/reader repair: subtest is part of the occurrence identity."""
from common import *

doc=load(HERE/'diagnostics/changed-54-crosswalk.json')
events=load(PRIOR/'python-node-scanners/python-identities-v1.json')['events']
for row in doc['changes']:
    matches=[e for e in events if e['identity']==row['identity'] and sha(e['message'].encode())==row['current_message_sha256']]
    assert len(matches)==1
    row['subtest']=matches[0]['subtest'];row['occurrence']=matches[0]['occurrence']
    row['stable_event_id']=sha(json.dumps([row['identity'],row['subtest'],row['occurrence']],ensure_ascii=False).encode())
assert len({r['stable_event_id'] for r in doc['changes']})==54
doc['supersedes']=pin(HERE/'diagnostics/changed-54-crosswalk.json')
doc['correction']='Added explicit subtest field to the event key. No message, trace, outcome or assertion changed. Original failed identity reader retained.'
save('diagnostics/changed-54-crosswalk-v2.json',doc)
source=(HERE/'verify_correction.py').read_text()
replacements={"(r['identity'],r['occurrence'])":"(r['identity'],r['subtest'],r['occurrence'])", "(row['identity'],row['occurrence'])":"(row['identity'],row['subtest'],row['occurrence'])",'diagnostics/changed-54-crosswalk.json':'diagnostics/changed-54-crosswalk-v2.json'}
for before,after in replacements.items():
    assert before in source
    source=source.replace(before,after)
with (HERE/'verify_correction_v2.py').open('x',encoding='utf-8',newline='\n') as f:f.write(source)
source=(HERE/'test_evidence_reader.py').read_text().replace('from verify_correction import','from verify_correction_v2 import').replace('diagnostics/changed-54-crosswalk.json','diagnostics/changed-54-crosswalk-v2.json').replace('evidence-reader-controls-raw.json','evidence-reader-controls-v2-raw.json').replace('validation/evidence-reader-controls.json','validation/evidence-reader-controls-v2.json')
with (HERE/'test_evidence_reader_v2.py').open('x',encoding='utf-8',newline='\n') as f:f.write(source)
print('54 exact parent/subtest/occurrence event keys; reader assertion unchanged',flush=True)
