"""Append-only entry authority, before any 04A.38 product edit. No Git writes."""
from pathlib import Path
from datetime import datetime, timezone
from collections import Counter
import hashlib
import json
import os
import re
import subprocess

ROOT=Path(__file__).resolve().parents[2]; HERE=Path(__file__).resolve().parent
BASE='ba1636f5356ac761509c2cfa16fc66b92430935d'
GATES=dict(P='NO',N1='NO GO',N2='NO',C2='UNMET',C3='UNAPPROVED')
def sha(b):return hashlib.sha256(b).hexdigest()
def git(*args):return subprocess.check_output(['git','-c','safe.directory='+ROOT.as_posix(),*args],cwd=ROOT)
def pin(path):
    b=(ROOT/path).read_bytes();return dict(path=path,bytes=len(b),sha256=sha(b))
def save(name,value):
    p=HERE/name;p.parent.mkdir(parents=True,exist_ok=True)
    with p.open('x',encoding='utf-8',newline='\n') as f:json.dump(value,f,ensure_ascii=False,indent=2);f.write('\n')
def public_path(path):
    private=re.search(r'[A-Za-z]:|DESKTOP-|LAPTOP-|S-1-5-',path,re.I)
    private=private or any(re.search(r'(?<![A-Za-z0-9])'+re.escape(os.environ.get(k,'!unset!'))+r'(?![A-Za-z0-9])',path,re.I) for k in ['USERNAME','COMPUTERNAME'])
    return '<PRIVATE_PROTECTED_PATH:'+sha(path.encode())[:24]+'>' if private else path
def record_crlf(data):
    out=bytearray();quoted=False
    for ch in data:
        if ch==34:quoted=not quoted
        if ch==10 and not quoted:out.append(13)
        out.append(ch)
    return bytes(out)
def reconstruct(b,row):
    lf=b.replace(b'\r\n',b'\n');crlf=lf.replace(b'\n',b'\r\n')
    variants=dict(DIRECT=b,LF=lf,CRLF=crlf,CRLF_EXCEPT_FINAL_LITERAL_LF=crlf[:-2]+b'\n' if crlf.endswith(b'\r\n') else crlf,CSV_RECORD_CRLF_EMBEDDED_LF=record_crlf(lf))
    matches=[k for k,v in variants.items() if len(v)==row['bytes'] and sha(v)==row['sha256']]
    assert matches,'Substantive or unproved predecessor mismatch: '+row['path']
    return matches

def main():
    assert git('rev-parse','HEAD').decode().strip()==git('rev-parse','main').decode().strip()==git('merge-base','HEAD','main').decode().strip()==BASE
    assert git('branch','--show-current').decode().strip()=='codex/04A-38'
    assert not git('diff','--name-only') and not git('diff','--cached','--name-only')
    assert git('show','-s','--format=%P',BASE).decode().strip()=='a0680f546c5130fa5143c0b0c7136f852d650cc5'
    prior=json.loads((ROOT/'evidence/04A37/publication-manifest.json').read_bytes())
    assert prior['decision']=='READY_FOR_OWNER_04A37_ACCEPTANCE_REVIEW' and len(prior['files'])==197
    checked=[]
    for row in prior['files']:
        if row['sha256'] is None:
            assert row==dict(path='evidence/04A37/publication-manifest.json',bytes=None,sha256=None);continue
        committed=git('show',BASE+':'+row['path']);actual=(ROOT/row['path']).read_bytes()
        assert actual.replace(b'\r\n',b'\n')==committed.replace(b'\r\n',b'\n')
        checked.append(dict(accepted=row,committed_bytes=len(committed),committed_sha256=sha(committed),current=pin(row['path']),qualification=reconstruct(committed,row)))
    documents=[]
    for row in json.loads((ROOT/'evidence/04A37/entry-authority.json').read_bytes())['owner_documents_excluded']:
        assert pin(row['path'])==row;documents.append(row)
    assert len(documents)==16
    plan=(ROOT/'planning/BESHStatNG_CrossPlatform_MasterPlan.md').read_text(encoding='utf-8-sig')
    assert 'Revision 2.13' in plan.splitlines()[2] and not plan.splitlines()[2].endswith('  ')
    paths=git('ls-files','-z').decode().split('\0');tracked=[]
    for path in paths:
        if not path:continue
        assert not path.startswith('TestResults/'),'Tracked historic output needs separate review'
        tracked.append(dict(pin(path),path=public_path(path)))
    save('entry-source-pins.json',dict(files=tracked,scope='All live Git-tracked entry files; ignored historic TestResults NOT_REHASHED; sensitive path identities digested.'))
    udf=sorted((ROOT/'BESHstatNG/src/ExcelUDFs').rglob('*.vb'))
    assert len(udf)==41
    snapshots=[]
    for p in udf:
        rel=p.relative_to(ROOT).as_posix();dest=HERE/'preedit/source'/p.relative_to(ROOT/'BESHstatNG/src/ExcelUDFs')
        dest=dest.with_suffix(dest.suffix+'.txt');dest.parent.mkdir(parents=True,exist_ok=True)
        with dest.open('xb') as f:f.write(p.read_bytes())
        snapshots.append(dict(source=pin(rel),snapshot=pin(dest.relative_to(ROOT).as_posix())))
    save('preedit/entry-source-snapshot.json',dict(provenance='ENTRY_SOURCE_SNAPSHOT_BEFORE_PRODUCT_EDITS; NOT_EXECUTION_FREEZE_YET',source_commit=BASE,files=snapshots))
    save('entry-authority.json',dict(base=BASE,head=BASE,main=BASE,merge_base=BASE,branch='codex/04A-38',captured_utc=datetime.now(timezone.utc).isoformat(),
        entry_before_evidence='CLEAN main; branch absent; safe branch creation explicitly permitted by tool approval',
        porcelain_v1=git('status','--porcelain=v1').decode(),index='EMPTY',diff_check_exit=0,diff_check_output=git('diff','--check').decode(),
        history=git('log','-6','--format=%H %P %s').decode().splitlines(),accepted37_artifacts=checked,accepted37_manifest=pin('evidence/04A37/publication-manifest.json'),
        accepted37_route_a_approval=pin('evidence/04A37/late-archived-entry-supplement/v1/supplemental-approval.json'),
        accepted37_not_ready=pin('evidence/04A37/late-archived-entry-supplement/v1/entry-not-ready/readiness.json'),
        accepted37_chronology='6912 original/config; 22050 explicitly late/config. Exception limited to04A37, not04A38.',
        owner_documents_excluded=documents,gates=GATES,scientific='NOT_GOLDEN',native='NOT_RUN',next_batch='04A39_NOT_AUTHORIZED',
        line_endings='Historical claims reconstructed exactly by recorded full SHA/size, not waived on normalized text similarity.',
        product_edit_gate='BLOCKED until fresh entry validation, full inventory and executable orthogonal coverage freeze pass independent audit'))
    print('PASS merged-main authority;',len(checked),'accepted37 payloads;',len(tracked),'tracked pins;',len(snapshots),'UDF snapshots; product unchanged')

if __name__=='__main__':main()
