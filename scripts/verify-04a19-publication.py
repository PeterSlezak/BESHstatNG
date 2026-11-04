"""Seal/check 04A.19 candidate hashes, privacy, trackability and executed evidence.
Never stage or publish. Manifest self-hash is excluded to avoid a circular digest.
"""
import argparse
import hashlib
import importlib.util
import json
from pathlib import Path
import subprocess

ROOT=Path(__file__).resolve().parents[1]
def module(name,path):
    s=importlib.util.spec_from_file_location(name,ROOT/path);m=importlib.util.module_from_spec(s);s.loader.exec_module(m);return m
c=module('capture19','scripts/capture-04a19-contract.py')
privacy=module('privacy14','scripts/verify-04a14-publication.py')
MANIFEST='evidence/04A19/publication-manifest.json'
PRIVACY='evidence/04A19/privacy.json'
privacy.MANIFEST=MANIFEST
NEW={'BESHStat.Excel.Common/src/Analysis/MultivariateClusteringRequest.vb','BESHStat.Excel.Common/src/Results/MultivariateClusteringProjection.vb',
    'BESHStat.Excel.Common.Tests/src/Multivariate19Tests.vb','BESHStatNG.Test/src/Multivariate19AdapterTests.vb',
    'scripts/verify-04a19.py','scripts/capture-04a19-contract.py','scripts/summarize-04a19.py','scripts/verify-04a19-publication.py',
    'scripts/tests/test_04a19_clustering.py','docs/architecture/batch-04A-19-owner-review.md'}

def paths():
    changed=set(c.git('diff','--name-only').splitlines())
    untracked=set(c.git('ls-files','--others','--exclude-standard').splitlines())
    allowed=set(json.loads((c.OUT/'implementation-plan.json').read_bytes())['modified'])
    result=changed|untracked|{MANIFEST,PRIVACY}
    assert changed<=allowed,changed-allowed
    assert all(p in allowed or p in NEW or (p.startswith('evidence/04A19/') and p.endswith('.json')) for p in result),'Unapproved candidate path'
    return sorted(result)

def ignored(path,no_index=False):
    command=['git','-c','safe.directory='+ROOT.as_posix(),'check-ignore']
    if no_index: command+=['--no-index']
    return subprocess.run(command+['-q','--',path],cwd=ROOT).returncode

def write(path,value):
    (ROOT/path).write_text(json.dumps(value,ensure_ascii=False,indent=2,allow_nan=False)+'\n',encoding='utf-8',newline='\n')

def evidence():
    read=lambda n:json.loads((c.OUT/n).read_bytes())
    assert read('entry-state.json')['base']==c.BASE==c.git('rev-parse','HEAD')==c.git('rev-parse','main')
    final=read('validation.json')
    for config in ['Debug','Release']:
        data=final['configurations'][config]
        assert data['new_logical_total']==data['new_logical_passed']>0
        assert not data['new_managed_nonpasses']
        assert data['logical_cases']['nonpassed']==6
        assert data['runtime_executions']['failed']==7
        assert data['node_passed']==data['node_total']>0
        assert not any('04a19' in p for p in data['python']['nonpasses'])
        assert data['full_command_exit']!=0
    corpus=read('kmeans-hierarchical-projection-comparisons-v1.json')
    assert corpus['version']==1 and corpus['status']=='NOT_GOLDEN' and corpus['validation']=='SAME_CORE_COMPATIBILITY_ONLY' and not corpus['promoted']
    assert corpus['record_count']==len(corpus['records'])==sum(corpus['outcomes'].values())
    assert corpus['payload_count']==len(corpus['profile_payloads'])
    assert {r['configuration'] for r in corpus['records']}=={'Debug','Release'}
    assert {r['tfm'] for r in corpus['records']}=={'net10.0','net48'}
    assert {'CORE_NONCONVERGED','EXISTING_CORE_REJECTED','EXISTING_CORE_REPORT_REJECTED','CORE_LAYOUT_RETURNED'}<=set(corpus['outcomes'])
    for key,payload in corpus['profile_payloads'].items():
        assert key==hashlib.sha256(json.dumps(payload,sort_keys=True,ensure_ascii=False,separators=(',',':'),allow_nan=False).encode()).hexdigest()
    for r in corpus['records']:
        assert r['profile_id'] in corpus['profile_payloads'] and r['test_outcome']=='Passed'
        assert len(r['raw_trx_sha256'])==64 and r['test_identity']
    inventory=read('test-inventory.json')
    assert inventory['logical_count']==final['configurations']['Debug']['new_logical_total']
    assert all(r['outcome']=='Passed' for case in inventory['cases'].values() for r in case['executions'])
    test_names={key.split('|',1)[1] for key in inventory['cases']}
    assert all(r['test_identity'].replace('"','') in test_names for r in corpus['records'])
    for p,h in inventory['source_hashes'].items(): assert c.sha(ROOT/p)==h,p
    cross=read('original-control-crosswalk.json')
    assert cross['releases']==cross['discharges']==0
    assert (cross['P'],cross['N1'],cross['N2'],cross['C2'])==('NO','NO GO','NO','UNMET')
    assert len(cross['controls'])==186 and len(cross['obligations'])==152
    for p,r in cross['authorities'].items(): assert c.sha(ROOT/p)==r['sha256']
    for p,r in read('frozen-capture-hashes.json')['files'].items(): assert c.sha(ROOT/p)==r['sha256'],p
    assert read('retention.json')['membership_metadata_errors']==0
    assert read('cluster-rowid-disposition.json')['status']=='OPEN / CANDIDATE_ONLY'
    for p,r in read('documentation-audit.json')['pages'].items(): assert c.sha(ROOT/p)==r['final_sha256']
    assert read('final-source-qualification.json')['compiled_sources_unchanged_after_final_suites']
    assert read('privacy.json')['unresolved']==0

def seal():
    assert c.git('branch','--show-current')=='codex/04A-19'
    assert not c.git('diff','--cached','--name-only')
    candidates=paths()
    write(PRIVACY,dict(status='PASS_PUBLIC_CANDIDATE_SCAN',candidate_count=len(candidates),unresolved=0,
        scope='Every candidate path and UTF-8 content, decoded JSON/base64, NFKC/case variants, local owner/host identifiers and profile/drive/SID/credential patterns. Raw originals remain local with separate hashes.',assurance='Bounded scan plus source review, not proof of absence of arbitrary secrets.'))
    findings,reviewed=privacy.scan(candidates)
    assert not findings,json.dumps(findings)
    tracked=set(c.git('ls-files').splitlines())
    files={}
    for p in candidates:
        if p==MANIFEST: continue
        normal,rule=ignored(p),ignored(p,True)
        assert normal==1 and (rule==1 or p in tracked),p
        if p in NEW:
            assert all(line.rstrip(' \t')==line for line in (ROOT/p).read_text(encoding='utf-8-sig').splitlines()),p
        files[p]=dict(sha256=c.sha(ROOT/p),bytes=(ROOT/p).stat().st_size,change='modified' if p in tracked else 'new',
            role='evidence' if p.startswith('evidence/') else 'documentation' if p.endswith('.md') else 'project' if p.endswith('.vbproj') else 'test' if 'Tests/' in p or '.Test/' in p or '/tests/' in p else 'implementation/verification',
            git_ignore_exit=normal,git_ignore_rule_exit=rule)
    write(MANIFEST,dict(base=c.BASE,head=c.git('rev-parse','HEAD'),branch='codex/04A-19',files=files,candidate_count=len(candidates),
        self_hash_policy='Only manifest itself excluded to avoid circularity. Its content, membership, privacy and trackability checked separately.',
        excluded=['Historical/ignored TestResults including raw logs/TRX, private source snapshots and drafts','Build binaries/caches, prior evidence, owner plan and unrelated files'],
        index='EMPTY',publication='OWNER_REVIEW_PENDING / UNSTAGED / UNCOMMITTED / NOT_PUSHED',native=False,R=False,P='NO',N1='NO GO',N2='NO',C2='UNMET',reviewed_false_positive_groups=len(reviewed)))
    verify()

def verify():
    d=json.loads((ROOT/MANIFEST).read_bytes())
    assert set(d['files'])|{MANIFEST}==set(paths())
    tracked=set(c.git('ls-files').splitlines())
    for p,r in d['files'].items():
        assert c.sha(ROOT/p)==r['sha256'] and (ROOT/p).stat().st_size==r['bytes'],p
        assert ignored(p)==1 and (ignored(p,True)==1 or p in tracked),p
    assert ignored(MANIFEST)==ignored(MANIFEST,True)==1
    assert not c.git('diff','--cached','--name-only')
    assert subprocess.run(['git','-c','safe.directory='+ROOT.as_posix(),'diff','--check'],cwd=ROOT).returncode==0
    findings,_=privacy.scan(paths());assert not findings,json.dumps(findings)
    evidence()
    print('PASS',len(d['files']),'external hashes;',d['candidate_count'],'candidates; schema/link/privacy/trackability; empty index; diff check')
    print('Manifest separate SHA-256',c.sha(ROOT/MANIFEST))

if __name__=='__main__':
    p=argparse.ArgumentParser();p.add_argument('action',choices=['seal','verify','evidence']);globals()[p.parse_args().action]()
