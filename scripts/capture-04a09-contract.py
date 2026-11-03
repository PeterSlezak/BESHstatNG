"""Source/obligation lineage for 04A.09; public sanitized derivatives, no native release."""
import csv
import hashlib
import json
from pathlib import Path
import re
import subprocess

ROOT=Path(__file__).resolve().parents[1]
BASE='c25c6954d4e2948e728eebf395970d7ae635d6b6'
OUT=ROOT/'evidence/04A09'
FORM='BESHstatNG/src/UI/UiGLM.vb'
LEDGER='docs/architecture/WindowsMigrationLedger/04A01-approved-deferred-obligations.json'

def sha(p):return hashlib.sha256((ROOT/p).read_bytes()).hexdigest()
def members(text):return {m[2]:m[0] for m in re.finditer(r'^    (?:Public|Private|Friend)?\s*(?:(?:Shared|Async|Overrides|Overloads)\s+)*(Sub|Function)\s+(\w+)\b.*?^    End \1',text,re.M|re.S)}
def original(p):return subprocess.check_output(['git','-c','safe.directory='+ROOT.as_posix(),'show',BASE+':'+p],cwd=ROOT).decode('utf-8-sig').replace('\r\n','\n')
def emit(name,value):
    OUT.mkdir(exist_ok=True)
    with (OUT/name).open('x',encoding='utf-8',newline='\n') as f:json.dump(value,f,indent=2,ensure_ascii=False);f.write('\n')
def sanitize(v):
    if isinstance(v,dict):return {k:sanitize(x) for k,x in v.items()}
    if isinstance(v,list):return [sanitize(x) for x in v]
    if isinstance(v,str):
        v=re.sub(r'[A-Za-z]:[\\/]Users[\\/][^\\/]+[\\/]Desktop[\\/]WPy[^\\/\s]*[\\/][^\r\n"]*?python\.exe','<PYTHON_EXE>',v,flags=re.I)
        for prefix in [str(ROOT),ROOT.as_posix()]:v=re.sub(re.escape(prefix),'<REPO>',v,flags=re.I)
        v=re.sub(r'[A-Za-z]:[\\/]Program Files[\\/]Microsoft Visual Studio[\\/]18[\\/]Community','<VS2026>',v,flags=re.I)
        v=re.sub(r'[A-Za-z]:[\\/]Users[\\/][^\\/\s";]+', '<OWNER_HOME>',v,flags=re.I)
        v=re.sub(r'S-1-5-21-(?:\d+-){2,}\d+','<OWNER_SID>',v)
    return v

def capture():
    assert (ROOT/FORM).read_text(encoding='utf-8-sig')==original(FORM)
    paths=[FORM,'BESHstatNG/src/RegModels/BinaryClassificationReporting.vb','planning/BESHStatNG_CrossPlatform_MasterPlan.md',
           'BESHStat.Excel.Common.Tests/src/Regression09Baselines.vb']
    paths += [p.relative_to(ROOT).as_posix() for p in (ROOT/'BESHStat.Core/src/RegModels').glob('*.vb')]
    paths += [p.relative_to(ROOT).as_posix() for p in (ROOT/'evidence/04A09/baselines').glob('*.txt')]
    paths += [p.relative_to(ROOT).as_posix() for p in (ROOT/'BESHStat.Core.Tests/TestData').glob('*.csv')]
    before=members(original(FORM))
    names=['RunOLS','RunGLM','RunGLMNB2','RunZIP','RunOrdLogit','RunMultiLogit']
    emit('baseline-manifest.json',dict(base=BASE,native_executed=False,P='NO',
        scope='Actual Core-backed host-free pre-edit execution, twelve cases. Existing committed CSV inputs; NOT_GOLDEN. No Excel constructor/writer execution.',
        tolerance='max(1e-9, abs(expected)*1e-9); strings, CLR kind, metadata/order and RowIds exact; only computational-time text masked',
        pins={p:sha(p) for p in sorted(paths)},members={n:hashlib.sha256(body.encode()).hexdigest() for n,body in before.items()},
        capture='dotnet test Common.Tests Debug --no-restore --filter FullyQualifiedName~Regression09Baselines; explicit PRECHANGE-c25c695 capture flag; 12/12'))
    controls=set()
    for name in names:
        controls.update(re.findall(r'Me\.(\w+)',before[name]))
    with (ROOT/'docs/architecture/WindowsMigrationLedger/ui-controls.csv').open(encoding='utf-8-sig') as f:rows=list(csv.DictReader(f))
    ledger=json.loads((ROOT/LEDGER).read_bytes())
    selected=[]
    for row in rows:
        cid=row['control_id']
        if cid.split('.')[-2]=='UiGLM' and cid.split('.')[-1] in controls:
            obligation=next((r for r in ledger if r['original_id']==cid),None)
            digest=lambda r:hashlib.sha256(json.dumps(r,sort_keys=True,ensure_ascii=False).encode()).hexdigest()
            selected.append(dict(control_id=cid,control_row_sha256=digest(row),control_row=sanitize(row),
                obligation_id=obligation['obligation_id'] if obligation else None,obligation_row_sha256=digest(obligation) if obligation else None,
                obligation_row=sanitize(obligation),status='OPEN' if obligation else 'NO ORIGINAL DEFERRED OBLIGATION ROW',
                evidence='Source-associated; host-free adapter tests; native-blocked. No release or discharge.'))
    emit('restriction-crosswalk.json',dict(base=BASE,authorities={p:sha(p) for p in [LEDGER,'docs/architecture/WindowsMigrationLedger/ui-controls.csv']},
        controls=selected,associated_controls=len(selected),associated_obligations=sum(r['obligation_id'] is not None for r in selected),
        released=0,discharged=0,P='NO',N1='NO GO',N2='NO',C2='UNMET',
        placeholder_mapping={'<OWNER_HOME>':'Original owner-home prefix; raw source/row hashes refer to original bytes','<OWNER_SID>':'Original owner SID'},
        gate='Behavior-preserving extraction only. Original pre-change native gates remain; no 04A.05/27 timing exception applied.'))

if __name__=='__main__':capture()
