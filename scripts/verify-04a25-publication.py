"""Seal exact unstaged 04A.25 candidates; historical files and index are read-only."""
import importlib.util
import json
from pathlib import Path
import stat
import subprocess
import sys

ROOT=Path(__file__).resolve().parents[1]
def module(name,path):
    s=importlib.util.spec_from_file_location(name,ROOT/path);m=importlib.util.module_from_spec(s);s.loader.exec_module(m);return m
c=module('c25_publication','scripts/capture-04a25-contract.py')
privacy=module('privacy14_25','scripts/verify-04a14-publication.py')
MANIFEST='evidence/04A25/publication-manifest.json';PRIVACY='evidence/04A25/privacy.json'
privacy.MANIFEST=MANIFEST
MODIFIED={c.FORM,'BESHstatNG/src/DM/PSMdataImport.vb','BESHstatNG/src/Graphics/PsmLovePlotExcel.vb','BESHStatNG.Test/BESHStatNG.Tests.vbproj','BESHStatNG_Help_MkDocs/docs/methods/propensity-score-matching.md'}
def paths():
    evidence={p.relative_to(ROOT).as_posix() for p in c.OUT.rglob('*') if p.is_file()}
    return sorted(set(c.git('diff','--name-only').splitlines())|set(c.git('ls-files','--others','--exclude-standard').splitlines())|evidence|{MANIFEST,PRIVACY})
def allowed(path):
    return path in MODIFIED or path.startswith(('evidence/04A25/','BESHStat.Excel.Common/src/Analysis/Psm','BESHStat.Excel.Common.Tests/src/Psm25')) or path in {'BESHStatNG.Test/src/Psm25AdapterTests.vb','scripts/tests/test_04a25_psm.py','docs/architecture/batch-04A-25-owner-review.md'} or (path.startswith('scripts/') and '/tests/' not in path and '-04a25' in path and path.endswith('.py'))
def ignored(path):return subprocess.run(['git','-c','safe.directory='+ROOT.as_posix(),'check-ignore','--no-index','-q','--',path],cwd=ROOT,capture_output=True).returncode
def seal():
    c.emit('privacy.json',dict(status='PENDING_SCAN'))
    candidates=paths();assert all(allowed(p) for p in candidates)
    findings,reviewed=privacy.scan(candidates);assert not findings,json.dumps(findings)
    c.emit('privacy.json',dict(status='PASS_PUBLIC_CANDIDATE_SCAN',candidate_count=len(candidates),unresolved=0,scanner=dict(path='scripts/verify-04a14-publication.py',sha256=c.sha(ROOT/'scripts/verify-04a14-publication.py')),scope='Every candidate path/content; decoded JSON/base64; owner/host/profile/drive/UNC/SID/credential patterns. Raw local logs/TRX excluded.',reviewed_public_patterns=[list(v) for v in reviewed]))
    tracked=set(c.git('ls-files').splitlines());records={}
    for p in candidates:
        if p==MANIFEST:continue
        assert ignored(p)==1 or p in tracked,p
        path=ROOT/p;st=path.stat()
        records[p]=dict(sha256=c.sha(path),bytes=st.st_size,mode='100644',filesystem_permission_bits=stat.S_IMODE(st.st_mode),change='modified' if p in tracked else 'new',git_ignore_exit=ignored(p))
    c.emit('publication-manifest.json',dict(base=c.BASE,head=c.git('rev-parse','HEAD'),branch=c.git('branch','--show-current'),files=records,candidate_count=len(candidates),self_hash_policy='Only this manifest omits its own hash; all other candidates have exact byte/hash/mode records.',publication='OWNER_REVIEW_PENDING / UNSTAGED / UNCOMMITTED / NOT_PUSHED',index='EMPTY',gates=dict(P='NO',N1='NO GO',N2='NO',C2='UNMET'),native_releases=0,deferred_discharges=0))
    verify()
def verify():
    data=json.loads((ROOT/MANIFEST).read_bytes());assert set(data['files'])|{MANIFEST}==set(paths())
    assert c.git('branch','--show-current')=='codex/04A-25'
    assert c.git('rev-parse','HEAD')==c.git('rev-parse','main')==c.BASE
    assert not c.git('diff','--cached','--name-only')
    for p,r in data['files'].items():
        path=ROOT/p;assert c.sha(path)==r['sha256'] and path.stat().st_size==r['bytes'],p
        assert stat.S_IMODE(path.stat().st_mode)==r['filesystem_permission_bits'],p
        assert ignored(p)==r['git_ignore_exit'],p
    findings,reviewed=privacy.scan(paths());assert not findings,json.dumps(findings)
    assert subprocess.run(['git','-c','safe.directory='+ROOT.as_posix(),'diff','--check'],cwd=ROOT,capture_output=True).returncode==0
    print('PASS',len(data['files']),'external hashes;',len(paths()),'candidates; privacy/trackability/index/diff')
def inspect():
    from collections import Counter
    findings,reviewed=privacy.scan([p for p in paths() if (ROOT/p).is_file()])
    (c.RUN/'privacy-initial-findings.json').write_text(json.dumps(findings,indent=2),encoding='utf-8')
    print('Privacy categories',dict(Counter(r['category'] for r in findings)))
    print('Affected artifacts',sorted({r['path'] for r in findings}))
if __name__=='__main__':globals()[sys.argv[1]]()
