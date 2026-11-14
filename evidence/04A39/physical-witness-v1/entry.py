from witness_lib import *
import shutil

lock()
assert set(git('diff','--name-only').decode().splitlines())=={'BESHStatNG.Test/BESHStatNG.Tests.vbproj','planning/BESHStatNG_CrossPlatform_MasterPlan.md'}
PRIVATE.mkdir(exist_ok=False)
files=[pin(p) for p in sorted(OLD.rglob('*')) if p.is_file() and '__pycache__' not in p.parts]
raw=dict(utc=utc(),branch=git('branch','--show-current').decode().strip(),HEAD=git('rev-parse','HEAD').decode().strip(),main=git('rev-parse','main').decode().strip(),merge_base=git('merge-base','HEAD','main').decode().strip(),
    parent=git('rev-parse','HEAD^').decode().strip(),status=git('status','--short').decode(),status_all=git('status','--porcelain=v1','-uall').decode(),staged=git('diff','--cached','--name-only').decode(),diff_check=git('diff','--check').decode(),worktree=git('worktree','list','--porcelain').decode(),index=pin('.git/index'))
save(PRIVATE/'entry-git-raw.json',raw)
archives=[]
for name,size,expected in [
 ('BESHStatNG_04A39_CORRECTION_REVIEW_v1.zip',74763995,'6164b6ff1f605997a4a2c482794682ab3e70bb85a48288721f252dec0bb03cab'),
 ('BESHStatNG_04A39_CORRECTION_POSTSEAL_v2.zip',182435,'871ec6511fbdf8236e5653df8e1153a21c0137ba609b644c00faf860e0abae76')]:
    row=pin(ROOT/'.test-rationalization-04A39-correction-v1.tmp'/name)
    assert row['bytes']==size and row['sha256']==expected
    archives.append(row)
guard=ROOT/'evidence/04A38/completion/w2/continuation-v6/privacy_regex_guard.py'
proof=ROOT/'evidence/04A38/completion/w2/continuation-v6/privacy-regex-guard-proof-v1.json'
g=pin(guard);p=load(proof)
assert g==p['source'] and g['sha256']=='ad17f82adca850afb94fa60d3c0ca5866b446860a7724dc77a5e5b3111c9ce5f'
provenance=[]
for path in [ROOT/'evidence/04A38/completion/w3/python-node-scanners/campaign-v1/python-optimization-authentication-v1.json',ROOT/'evidence/04A39/python-node-scanners/campaign-v2/python-optimization-authentication-v1.json']:
    doc=load(path);assert doc['prefixes']['privacy_regex_guard.py']==g and doc['prefixes']['privacy-regex-guard-proof-v1.json']==pin(proof)
    provenance.append(pin(path))
scanner=[]
for phase,expected in [('13','b26596e5bf9644e81a479fad4f28d3db8d8af23611175b8e582c759007009a03'),('14','b4b4fd3567947d807c73e63022a0128ded37199248d0067f786cb472811d5a25')]:
    row=pin('scripts/verify-04a'+phase+'-publication.py');assert row['sha256']==expected
    scanner.append(row)
for source,name in [(guard,'privacy_regex_guard.py'),(proof,'privacy-regex-guard-proof-v1.json')]:
    dest=HERE/'dependencies'/name;dest.parent.mkdir(parents=True,exist_ok=True)
    with dest.open('xb') as f:f.write(source.read_bytes())
    assert digest(dest)==digest(source)
authority=dict(date='2026-11-14',type='OWNER_ASSERTION_FROM_CURRENT_INSTRUCTION',scope='04A39 physical review-derivative publication gate only',
    exact_quote='The authorization does not change original repository-candidate scanner scope, waive the two inherited 1,941-finding privacy FAIL parents, change earlier 1,599 snapshots or W3 ERRORs, or authorize protected raw evidence modification/deletion.',
    acceptance=False,required_fixture_matches=25,required_unresolved=0)
safe={k:v for k,v in raw.items() if k not in {'status_all','worktree'}}
save('entry.json',dict(utc=utc(),Git=safe,actual_commands=['git branch --show-current','git rev-parse HEAD main','git merge-base HEAD main','git diff --cached --name-only','git diff --check','git status --short','SHA256 .git/index'],
    command_prefix='git -c safe.directory=<LIVE_REPO> --no-optional-locks; GIT_OPTIONAL_LOCKS=0',raw_git=pin(PRIVATE/'entry-git-raw.json'),
    source_lock=pin(OLD/'entry/source-lock.json'),source_count=1353,old_correction_files=files,input_archives=archives,
    guard=g,guard_proof=pin(proof),guard_provenance=provenance,scanners=scanner,authority=authority,gates=GATES,
    files_to_add='Only evidence/04A39/physical-witness-v1 and its ignored task-private directory; no existing source/test/plan changes.'))
print('Entry authenticated: branch39 main22da; index unchanged; guard842 bytes authenticated; both scanners and exact ZIP hashes PASS',flush=True)
