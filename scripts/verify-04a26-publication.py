"""Seal exact unstaged 04A.26 candidates; exclude concurrent owner plan revision."""
import importlib.util
import json
from pathlib import Path
import stat
import subprocess
import sys

ROOT=Path(__file__).resolve().parents[1]
def module(name,path):
    s=importlib.util.spec_from_file_location(name,ROOT/path);m=importlib.util.module_from_spec(s);s.loader.exec_module(m);return m
c=module('c26_publication','scripts/capture-04a26-contract.py')
audit=module('audit26_publication','scripts/audit-04a26.py')
privacy=module('privacy14_26','scripts/verify-04a14-publication.py')
MANIFEST='evidence/04A26/publication-manifest.json';PRIVACY='evidence/04A26/privacy.json'
privacy.MANIFEST=MANIFEST
def paths():
    evidence={p.relative_to(ROOT).as_posix() for p in c.OUT.rglob('*') if p.is_file()}
    return sorted((set(c.git('diff','--name-only').splitlines())|set(c.git('ls-files','--others','--exclude-standard').splitlines())|evidence|{MANIFEST,PRIVACY})-{c.PLAN})
def allowed(p):
    return p in audit.ALLOWED or p.startswith('evidence/04A26/') or p.startswith('BESHStat.Excel.Common/src/Analysis/') and Path(p).name.startswith(('Agreement','SampleSize')) or p.startswith('BESHStat.Excel.Common.Tests/src/') and Path(p).name.startswith(('Agreement26','Planning26')) or p in {'BESHStatNG.Test/src/Agreement26AdapterTests.vb','scripts/tests/test_04a26_planning_agreement.py','docs/architecture/batch-04A-26-owner-review.md'} or p.startswith('scripts/') and '/tests/' not in p and '-04a26' in p and p.endswith(('.py','.ps1'))
def ignored(p):return subprocess.run(['git','-c','safe.directory='+ROOT.as_posix(),'check-ignore','--no-index','-q','--',p],cwd=ROOT,capture_output=True).returncode
def inspect():
    findings,reviewed=privacy.scan([p for p in paths() if (ROOT/p).is_file()])
    (c.RUN/'privacy-initial-findings.json').write_text(json.dumps(findings,indent=2),encoding='utf-8')
    from collections import Counter
    print('Privacy categories',dict(Counter(r['category'] for r in findings)))
    print('Affected artifacts',sorted({r['path'] for r in findings}))
def seal():
    c.emit('privacy.json',dict(status='PENDING_SCAN'))
    candidates=paths();assert all(allowed(p) for p in candidates),[p for p in candidates if not allowed(p)]
    findings,reviewed=privacy.scan(candidates);assert not findings,json.dumps(findings)
    c.emit('privacy.json',dict(status='PASS_PUBLIC_CANDIDATE_SCAN',candidate_count=len(candidates),unresolved=0,scanner=dict(path='scripts/verify-04a14-publication.py',sha256=c.sha(ROOT/'scripts/verify-04a14-publication.py')),scope='Every candidate path/content, decoded JSON/base64, private owner/host/profile/drive/SID/credential patterns. Raw logs/TRX and historical corpora excluded.',reviewed_public_patterns=[list(v) for v in reviewed]))
    tracked=set(c.git('ls-files').splitlines());records={}
    for p in candidates:
        if p==MANIFEST:continue
        assert ignored(p)==1 or p in tracked,p
        path=ROOT/p;s=path.stat()
        records[p]=dict(sha256=c.sha(path),bytes=s.st_size,mode='100644',filesystem_permission_bits=stat.S_IMODE(s.st_mode),change='modified' if p in tracked else 'new',purpose='04A.26 software request/report extraction, test, Help or review evidence',git_ignore_exit=ignored(p))
    c.emit('publication-manifest.json',dict(base=c.BASE,head=c.git('rev-parse','HEAD'),branch=c.git('branch','--show-current'),files=records,candidate_count=len(candidates),excluded_owner_edits={c.PLAN:c.sha(ROOT/c.PLAN)},self_hash_policy='Only this manifest omits its own hash. Every other candidate has exact SHA-256/size/mode.',publication='OWNER_REVIEW_PENDING / UNSTAGED / UNCOMMITTED / NOT_PUSHED',index='EMPTY',gates=dict(P='NO',N1='NO GO',N2='NO',C2='UNMET'),native_releases=0,deferred_discharges=0))
    verify()
    data=json.loads((ROOT/MANIFEST).read_bytes())
    data['final_git_state']=dict(status=c.git('status','--short').splitlines(),untracked=c.git('ls-files','--others','--exclude-standard').splitlines(),staged=c.git('diff','--cached','--name-only'),diff_check_exit=0)
    from datetime import datetime,timezone
    started=datetime.fromtimestamp(c.RUN.stat().st_ctime,timezone.utc);finished=datetime.now(timezone.utc)
    data['terminal_timing']=dict(entry_freeze_utc=started.isoformat(),first_complete_seal_verification_utc=finished.isoformat(),seconds=(finished-started).total_seconds(),qualification='Includes entry/development/builds/matrices/requalification/serial gates/retention/dossier/first full privacy and manifest verification. Final manifest self-metadata append and any later read-only review excluded.')
    c.emit('publication-manifest.json',data)
def verify():
    data=json.loads((ROOT/MANIFEST).read_bytes());assert set(data['files'])|{MANIFEST}==set(paths())
    assert c.git('branch','--show-current')=='codex/04A-26'
    assert c.git('rev-parse','HEAD')==c.git('rev-parse','main')==c.BASE
    assert not c.git('diff','--cached','--name-only')
    for p,r in data['files'].items():
        path=ROOT/p;assert c.sha(path)==r['sha256'] and path.stat().st_size==r['bytes'],p
        assert stat.S_IMODE(path.stat().st_mode)==r['filesystem_permission_bits'],p
        assert ignored(p)==r['git_ignore_exit'],p
    for p,h in data['excluded_owner_edits'].items():assert c.sha(ROOT/p)==h,p
    findings,reviewed=privacy.scan(paths());assert not findings,json.dumps(findings)
    assert subprocess.run(['git','-c','safe.directory='+ROOT.as_posix(),'diff','--check'],cwd=ROOT,capture_output=True).returncode==0
    print('PASS',len(data['files']),'external hashes;',len(paths()),'candidates; privacy/trackability/empty index/diff')
if __name__=='__main__':globals()[sys.argv[1]]()
