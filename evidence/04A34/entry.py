"""Read-only entry capture. No historical payload scan or product writes."""
from pathlib import Path
import csv, hashlib, io, json, re, subprocess, os
ROOT = Path(__file__).resolve().parents[2]
HERE = Path(__file__).resolve().parent
BASE = 'b162a6ac801d7784fc5e27974fbf91aa57a2aee1'
def git(*args):
    return subprocess.check_output(['git', '-c', 'safe.directory='+ROOT.as_posix(), *args], cwd=ROOT).decode('utf-8-sig').strip()
def sha(b): return hashlib.sha256(b).hexdigest()
def pin(p):
    b=(ROOT/p).read_bytes()
    return dict(path=p, bytes=len(b), sha256=sha(b))
def save(name, value):
    (HERE/name).write_text(json.dumps(value, ensure_ascii=False, indent=2, allow_nan=False)+'\n', encoding='utf-8', newline='\n')
def public(p):
    if p.startswith('<PRIVATE_PROTECTED_PATH:'):return p
    tokens=[a.name for a in ROOT.parents if a.parent.name.casefold()=='users']+[os.environ.get('COMPUTERNAME','!unset!')]
    private=bool(re.search(r'DESKTOP-|LAPTOP-|S-1-5-|[A-Za-z]:',p,re.I)) or any(re.search(r'(?<![A-Za-z0-9])'+re.escape(t)+r'(?![A-Za-z0-9])',p,re.I) for t in tokens)
    return '<PRIVATE_PROTECTED_PATH:'+sha(p.encode())[:24]+'>' if private else p
if __name__ == '__main__':
    assert git('rev-parse','HEAD','main').splitlines()==[BASE,BASE]
    assert git('branch','--show-current')=='codex/04A-34'
    assert not git('diff','--name-only') and not git('diff','--cached','--name-only')
    assert not (HERE/'entry-authority.json').exists()
    files=[]
    for p in git('ls-files').splitlines():
        if (ROOT/p).is_file():
            r=pin(p); r['path']=public(p); files.append(r)
    save('entry-pins.json',dict(files=files, scope='Tracked files only. Ignored historical payload bytes NOT_REHASHED.'))
    qualified=[]
    for r in json.loads((ROOT/'evidence/04A33/publication-manifest.json').read_bytes())['files']:
        if r['sha256'] is None: continue
        b=(ROOT/r['path']).read_bytes(); lf=b.replace(b'\r\n',b'\n'); cr=lf.replace(b'\n',b'\r\n')
        variants={'DIRECT':b,'LF':lf,'CRLF':cr,'CRLF_EXCEPT_FINAL_LITERAL_LF':cr[:-2]+b'\n' if cr.endswith(b'\r\n') else cr}
        if r['path'].endswith('.csv'):
            s=io.StringIO(newline='');csv.writer(s).writerows(csv.reader(io.StringIO(lf.decode(),newline='')))
            variants['CSV_RECORD_CRLF_EMBEDDED_LF']=s.getvalue().encode()
        matches=[k for k,v in variants.items() if len(v)==r['bytes'] and sha(v)==r['sha256']]
        committed=subprocess.check_output(['git','-c','safe.directory='+ROOT.as_posix(),'show',BASE+':'+r['path']],cwd=ROOT)
        qualified.append(dict(path=r['path'], historical=r, current=pin(r['path']), digest_relations=matches, equals_merged_blob=b==committed, lf_equals_merged_blob=lf==committed.replace(b'\r\n',b'\n')))
    save('prior-publication-check.json',dict(files=qualified, status='PASS' if all(r['digest_relations'] and r['lf_equals_merged_blob'] for r in qualified) else 'BLOCKED', limit='Exact inverse EOL digest qualification; no new archive inspection.'))
    members=list(csv.DictReader((ROOT/'evidence/04A29/graphics-members-v1.csv').open(encoding='utf-8-sig')))
    rows=[dict(m,entry_sha256=pin(m['path'])['sha256'], range_authority='Historical 04A29; current source requires member reconciliation', disposition='OPEN_BLOCKER') for m in members if '04A.34' in m['proposed_batches'].split(';')]
    with (HERE/'operation-map-entry.csv').open('w',encoding='utf-8',newline='') as f:
        w=csv.DictWriter(f,fieldnames=list(rows[0])); w.writeheader(); w.writerows(rows)
    save('entry-authority.json',dict(schema='04A34-entry-v1',head=BASE,main=BASE,merge_base=git('merge-base','HEAD','main'),branch=git('branch','--show-current'),index='EMPTY', tracked_status=git('status','--short','--untracked-files=no'), entry_untracked=git('ls-files','--others','--exclude-standard').splitlines(), parent=git('rev-parse','HEAD^'), accepted_33_reference='43d20f86b80934c49b2f3dec90824aa62af5aaee', scope='Official04A34 only', gates=dict(P='NO',N1='NO GO',N2='NO',C2='UNMET',C3='UNAPPROVED'),scientific='NOT_GOLDEN',candidate_members=len(rows),families=sorted({r['family'] for r in rows})))
    (ROOT/'.test-rationalization-04A34.tmp').mkdir(exist_ok=False)
    print('Frozen',len(files),'tracked files;',len(rows),'candidate declarations;',len(qualified),'prior publication files;',len({r['family'] for r in rows}),'families')
