"""Source-derived successor evidence for 04A.08. No native capture, release or old evidence mutation."""
import csv
import hashlib
import json
from pathlib import Path
import re
import subprocess

ROOT=Path(__file__).resolve().parents[1]
BASE='b48877406c3c185a992d5dd5c42f951611a52569'
OUT=ROOT/'docs/architecture/04A08'
FORMS=['UiGLM','Ui13GEE','Ui4Cox','Ui18MMRM','Ui19LMM','Ui20PropensityScoreMatching']
LEDGER='docs/architecture/WindowsMigrationLedger/04A01-approved-deferred-obligations.json'
AMENDMENT='docs/architecture/04A02/04A02-final-amended-acceptance-PROPOSED.json'

def sha(p):return hashlib.sha256((ROOT/p).read_bytes()).hexdigest()
def rowsha(row):return hashlib.sha256(json.dumps(row,sort_keys=True,ensure_ascii=False).encode()).hexdigest()
def original(p):return subprocess.check_output(['git','-c','safe.directory='+ROOT.as_posix(),'show',BASE+':'+p],cwd=ROOT).decode('utf-8-sig').replace('\r\n','\n')
def members(text):return {m[2]:m[0] for m in re.finditer(r'^    (?:Public|Private|Friend)?\s*(?:(?:Shared|Async|Overrides|Overloads)\s+)*(Sub|Function)\s+(\w+)\b.*?^    End \1',text,re.M|re.S)}
def emit(name,value):
    with (OUT/name).open('x',encoding='utf-8',newline='\n') as f:json.dump(value,f,indent=2,ensure_ascii=False);f.write('\n')

def restriction_snapshot(root=ROOT):
    # Development/testing authority stays local. Publication renditions may be
    # sanitized and must never replace original audit inputs.
    return root/'docs/architecture/04A08/restriction-crosswalk.json'

def build():
    assert sha(AMENDMENT)=='8ba5614757a20cd9ca7d075529c058072a250e4ae97fc00d66228c9703e22307'
    assert sha(LEDGER)=='363ae57510b613eef06faebba7bc7e4152b928e4d3bbad7c1f67eea589c56d10'
    ledger=json.loads((ROOT/LEDGER).read_bytes())
    amendment=json.loads((ROOT/AMENDMENT).read_bytes())
    transfers={r['obligation_id'] for r in amendment['obligation_transfers']}
    with (ROOT/'docs/architecture/WindowsMigrationLedger/ui-controls.csv').open(encoding='utf-8-sig') as f:controls=list(csv.DictReader(f))
    with (ROOT/'docs/architecture/WindowsMigrationLedger/continuation28-control-triage.csv').open(encoding='utf-8-sig') as f:triage=list(csv.DictReader(f))
    selected=[];ownership=[]
    for form in FORMS:
        p='BESHstatNG/src/UI/'+form+'.vb'
        text=(ROOT/p).read_text(encoding='utf-8-sig')
        assert text==original(p),'Form changed outside shared controller scope: '+p
        handlers=[];names=set()
        for name,body in members(text).items():
            if re.search(r'\.Add(?:Main|Categorical|Polynomial|TwoWay|Custom).*FromSelectedVars\(',body) or re.search(r'Remove_Item\([^\n]*TermSpecs',body):
                handles=re.findall(r'Handles ([^\n]+)',body.splitlines()[0])
                edges=re.findall(r'(\w+)\.(\w+)',handles[0]) if handles else []
                names.update(n for n,e in edges)
                handlers.append(dict(member=name,body_sha256=hashlib.sha256(body.encode()).hexdigest(),edges=edges,body=body))
        # Model lists and polynomial degree inputs participate in the same path.
        for m in re.finditer(r'New RegressionEffectsController\((.*?)\)',text,re.S):
            names.update(re.findall(r'Me\.(lb\w+)',m[1]))
        names.update(re.findall(r'CInt\(Me\.(spinBtnPoly\w*)\.Value\)',text))
        for row in controls:
            cid=row['control_id']
            if cid.split('.')[-2]==form and cid.split('.')[-1] in names:
                obligation=next((r for r in ledger if r['original_id']==cid),None)
                tr=next((r for r in triage if r.get('control_id')==cid),None)
                selected.append(dict(control_id=cid,control_row_sha256=rowsha(row),control_row=row,
                    triage_row_sha256=rowsha(tr) if tr else None,triage_row=tr,
                    obligation_id=obligation['obligation_id'] if obligation else None,
                    obligation_row_sha256=rowsha(obligation) if obligation else None,obligation_row=obligation,
                    transferred=bool(obligation and obligation['obligation_id'] in transfers),status='OPEN' if obligation else 'NO ORIGINAL DEFERRED OBLIGATION ROW',
                    active_change='Behavior-preserving delegation; no native release; source-derived oracle only'))
        ownership.append(dict(path=p,source_sha256=sha(p),unchanged=True,handlers=handlers,
            controllers=re.findall(r'Me\.(\w+) = New RegressionEffectsController\((.*?)\)',text,re.S),
            scope='Original event handlers, source refresh, fitting/reporting, lifecycle and all model options unchanged'))
    return dict(schema='04A08-restriction-crosswalk-v1',base=BASE,affected_controls=selected,ownership=ownership,
        authorities={p:sha(p) for p in [LEDGER,AMENDMENT,'docs/architecture/WindowsMigrationLedger/ui-controls.csv','docs/architecture/WindowsMigrationLedger/continuation28-control-triage.csv']},
        counts=dict(original_obligations=1514,restricted_live_controls=802,transferred_obligations=239,transferred_controls=218,
            source_associated_controls=len(selected),source_associated_obligations=sum(r['obligation_id'] is not None for r in selected)),
        released_controls=0,discharged_obligations=0,P='NO',N1='NO GO',N2='NO',C2='UNMET',native_executed=False,
        gate='No new behavior-changing native path activated. Pre-change proof or exact owner exception remains mandatory for any future behavior change; 04A.05/27 timing exceptions do not transfer.')

def removal_oracle():
    p='BESHstatNG/src/DM/UIprocedures.vb'
    source=members(original(p))
    names=['IsDerivedEffectKey','ContainsAnyBaseKey','SyncTermSpecsToListBoxItems','ExtractBaseKeysForSubset','Remove_Item']
    body='\n\n'.join(source[n] for n in names)
    target=ROOT/'BESHStatNG.Test/src/LegacyRegressionRemoval08.vb'
    content='Option Explicit On\nOption Strict Off\nImports System\nImports System.Collections.Generic\nImports BESHStatNG\n\' Frozen '+BASE+' source; no native execution.\nFriend Class LegacyRegressionRemoval08\n'+body+'\nEnd Class\n'
    with target.open('x',encoding='utf-8',newline='\n') as f:f.write(content)
    emit('removal-characterization.json',dict(source=p,before_sha256=hashlib.sha256(original(p).encode()).hexdigest(),
        members={n:dict(body_sha256=hashlib.sha256(source[n].encode()).hexdigest(),body=source[n]) for n in names},oracle_sha256=sha(target.relative_to(ROOT)),native=False))

if __name__=='__main__':
    import sys
    if sys.argv[1:] == ['check']:
        assert json.loads(json.dumps(build()))==json.loads(restriction_snapshot().read_bytes())
        print('04A.08 shipping wiring/retained exact-ID restrictions PASS')
    else:
        removal_oracle()
        emit('restriction-crosswalk.json',build())
        print('04A.08 source/row lineage captured; no restrictions released')
