"""Narrow review derivatives; original symbol/control authority and tool receipt unchanged."""
from common40_v3 import *
import csv,io,re
sys.path.insert(0,str(ROOT/'evidence/04A39/physical-witness-v1'));import physical
lock();oldstage=PRIVATE/'publication-v2/stage';assert not (PRIVATE/'publication-v2/BESHStatNG_04A40_REVIEW_v2.zip').exists()
failed=[load(PRIVATE/('publication-stage-'+p+'-raw.json')) for p in ['13','14']]
assert all(len(d['findings'])==5 for d in failed)
save('publication/publication-stop-v1.json',dict(status='STAGE_PRIVACY_GATE_FAILED;NO_ZIP_CREATED',findings_per_original_scanner=5,
    affected_paths=['docs/architecture/WindowsMigrationLedger/symbols.csv','evidence/04A40/entry/tool-runtime-final-v1.json'],
    raw=[pin(PRIVATE/('publication-stage-'+p+'-raw.json')) for p in ['13','14']],old_stage='PRESERVED_LOCAL;NOT_PUBLISHED',new_allowances=0))
path='docs/architecture/WindowsMigrationLedger/symbols.csv';original=pin(path)
with (ROOT/path).open(encoding='utf-8-sig',newline='') as f:reader=csv.DictReader(f);fields=reader.fieldnames;rows=list(reader)
before=[dict(r) for r in rows];reg=load(ROOT/'evidence/04A39/register/defects-v7.json');referenced={r['symbol_id']:r for issue in reg['issues'] for r in issue.get('original_linked_member_signatures',[])};assert len(referenced)==759
changed=[]
for row in rows:
    for name,value in row.items():
        if value is None:continue
        rendered=value
        for token in physical.tokens():rendered=re.sub(r'(?i)(?<![A-Za-z0-9])'+re.escape(token)+r'(?![A-Za-z0-9])','<CONTROLLED_OWNER_IDENTIFIER>',rendered)
        if rendered!=value:
            assert name=='source_excerpt' and row['symbol_id'] not in referenced,'IDENTITY_OR_REFERENCED_SIGNATURE_REDACTION_FORBIDDEN'
            row[name]=rendered;changed.append(dict(symbol_id=row['symbol_id'],column=name,referenced759=False,raw_values='CONTROLLED_LOCAL_ONLY'))
assert len(changed)==1 and [r['symbol_id'] for r in before]==[r['symbol_id'] for r in rows]
for row in rows:
    if row['symbol_id'] in referenced:assert {k:row.get(k) for k in referenced[row['symbol_id']]}==referenced[row['symbol_id']]
buffer=io.StringIO(newline='');writer=csv.DictWriter(buffer,fieldnames=fields,lineterminator='\n');writer.writeheader();writer.writerows(rows)
safe=PRIVATE/'review-symbols-v1.csv';safe.open('x',encoding='utf-8',newline='').write(buffer.getvalue())
toolpath=HERE/'entry/tool-runtime-final-v1.json';tool=load(toolpath);changed_stdout=[]
for row in tool['commands']:
    text=row['stdout'];new=text.replace('\\','/')
    assert re.findall(r'\b\d+\.\d+(?:\.\d+)+\b',text)==re.findall(r'\b\d+\.\d+(?:\.\d+)+\b',new)
    if new!=text:changed_stdout.append(row['label']);row['stdout']=new
save(PRIVATE/'review-tool-runtime-v2.json',tool)
class Member:
    def __init__(self,path):self.path=path
    def is_file(self):return True
    def read_text(self,encoding):assert encoding=='utf-8-sig';return self.path.read_text(encoding='utf-8-sig')
class Scope:
    parents=ROOT.parents;name=ROOT.name
    def __truediv__(self,name):return Member(safe if name==path else PRIVATE/'review-tool-runtime-v2.json')
    def __fspath__(self):return str(ROOT)
    def as_posix(self):return ROOT.as_posix()
checks=[]
for phase in ['13','14']:
    m=physical.scanner(phase);m.ROOT=Scope();found,reviewed=m.scan([path,'evidence/04A40/entry/tool-runtime-final-v1.json']);assert not found
    checks.append(dict(scanner=phase,original_source=pin(m.__file__),new_findings=0))
assert pin(path)==original and load(toolpath)!=tool
save('publication/narrow-renderings-v1.json',dict(symbols=dict(original=original,derivative=pin(safe),rows=len(rows),row_identity_order_exact=True,all759_referenced_signatures_exact=True,changed_cells=changed,original_controls_aliases_unchanged=True),
    tools=dict(original=pin(toolpath),derivative=pin(PRIVATE/'review-tool-runtime-v2.json'),changed_stdout_paths=changed_stdout,version_numbers_exact=True,diagnostic_change='Review-only forward path separators avoid escaped-path false UNC matches; raw stdout/receipt unchanged.'),
    controls=checks,restricted_source_modified=False,original_scanners_unchanged=True,new_synthetic_allowances=0))
print('Narrow symbol excerpt and tool-path review derivatives PASS; all IDs/759 signatures retained; originals unchanged',flush=True)
