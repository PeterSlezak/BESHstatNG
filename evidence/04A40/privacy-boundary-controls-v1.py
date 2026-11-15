"""Actual original scanners: raw path leakage rejected, deterministic derivative clean."""
from common40_v3 import *
import re,ast
sys.path.insert(0,str(ROOT/'evidence/04A39/physical-witness-v1'));import physical
sys.path.append(str(ROOT/'evidence/04A38'));from baseline_report import clean
lock();rawpath=ROOT/'evidence/04A40/baseline/Debug-build-v1.json';original=pin(rawpath);raw=load(rawpath)
sanitized=clean(raw);data=(json.dumps(sanitized,ensure_ascii=False,indent=2,allow_nan=False)+'\n').encode()
assert data==(json.dumps(clean(raw),ensure_ascii=False,indent=2,allow_nan=False)+'\n').encode() and sanitized!=raw
assert raw['exit_codes']==sanitized['exit_codes'] and raw['source_pins']==sanitized['source_pins']
save(PRIVATE/'baseline-Debug-build-review-v1.json',sanitized)
class Member:
    def __init__(self,text):self.text=text
    def is_file(self):return True
    def read_text(self,encoding):assert encoding=='utf-8-sig';return self.text
class Root:
    parents=ROOT.parents;name=ROOT.name
    def __init__(self,text):self.text=text
    def __truediv__(self,path):return Member(self.text)
    def __fspath__(self):return str(ROOT)
    def as_posix(self):return ROOT.as_posix()
reports=[]
for phase in ['13','14']:
    m=physical.scanner(phase);f,_=m.scan(['evidence/04A40/baseline/Debug-build-v1.json']);assert len(f)==6
    m.ROOT=Root(data.decode());safe,_=m.scan(['review.json']);assert not safe
    controls=[]
    for token in physical.tokens():
        m.ROOT=Root(json.dumps(dict(value=token)));found,_=m.scan(['probe.json']);assert any(r['category']=='known_private_identifier' for r in found);controls.append('OWNER_IDENTIFIER_REJECTED')
    for value,category in [('private'+chr(64)+'invalid.example','email'),('-----BEGIN '+'PRIVATE KEY','secret'),('Z'+':'+chr(92)+'sensitive','appdata_or_drive_path')]:
        m.ROOT=Root(json.dumps(dict(value=value)));found,_=m.scan(['probe.json']);assert any(r['category']==category for r in found);controls.append(category+'_REJECTED')
    m=physical.scanner(phase);project='BESHstatNG/BESHStatNG.vbproj';normal,reviewed=m.scan([project]);assert not normal
    saved_git=m.git;m.git=lambda *args:''
    missing,_=m.scan([project]);m.git=saved_git;assert len(missing)==2 and {r['line'] for r in missing}=={603,606}
    reports.append(dict(scanner=phase,raw_findings=6,sanitized_findings=0,controls=controls,SDK_missing_HEAD_literal_findings=2,SDK_actual_reviewed=reviewed,original_source=pin(m.__file__)))
assert pin(rawpath)==original
save('publication/privacy-boundary-controls-v1.json',dict(status='FOCUSED_CONTROLS_PASS',raw_source=original,raw_unchanged=True,
    deterministic_sanitized=pin(PRIVATE/'baseline-Debug-build-review-v1.json'),counts_status_source_identities_preserved=True,results=reports,
    protected_raw_paths=[original],raw_repository_disposition='SIX_CURRENT_RULE_OCCURRENCES_UNWAIVED;ONE_RAW_OUTPUT_PATH_IN_CONTENT_AND_JSON_VIEWS;LOCAL_PROTECTED',
    producer=dict(historical_recipe=pin(HERE/'baseline-build-v1.py'),active_final_pipeline='Build raw receipts written to task-private storage; public source-freezes contain only receipt pins and non-sensitive facts. Publication derives sanitized JSON and independently scans actual ZIP.'),
    owner_identifiers_and_values='PRIVATE_ONLY',new_synthetic_allowances=0,SDK_HEAD=dict(HEAD=MAIN,source_sha256=sha(git('show','HEAD:BESHstatNG/BESHStatNG.vbproj')),size=len(git('show','HEAD:BESHstatNG/BESHStatNG.vbproj')))))
print('Raw6 findings retained; deterministic sanitized0; original privacy/owner/secret/SDK adverse controls PASS',flush=True)
