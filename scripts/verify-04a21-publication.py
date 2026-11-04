"""Detached 04A.21 JSON/hash/profile/authority/privacy verifier. Never stages or publishes."""
import argparse
import hashlib
import importlib.util
import json
from pathlib import Path
import subprocess

ROOT=Path(__file__).resolve().parents[1]
def module(name,path):
    s=importlib.util.spec_from_file_location(name,ROOT/path);m=importlib.util.module_from_spec(s);s.loader.exec_module(m);return m
c=module('capture21','scripts/capture-04a21-contract.py')
privacy=module('privacy14','scripts/verify-04a14-publication.py')
MANIFEST='evidence/04A21/publication-manifest.json';PRIVACY='evidence/04A21/privacy.json';privacy.MANIFEST=MANIFEST
NEW={
 'BESHStat.Excel.Common/src/Analysis/'+n for n in ['SpcInputPointPolicy.vb','SpcPhaseStagePolicy.vb','SpcExclusionPolicy.vb','SpcRequestPreparation.vb']
}|{'BESHStat.Excel.Common.Tests/src/'+n for n in ['Spc21Tests.vb','Spc21Fixture.vb','Spc21LegacyPreparation.vb']}|{
 'BESHStatNG.Test/src/Spc21AdapterTests.vb','scripts/capture-04a21-contract.py','scripts/summarize-04a21.py','scripts/verify-04a21.py','scripts/verify-04a21-publication.py','scripts/tests/test_04a21_spc.py','docs/architecture/batch-04A-21-owner-review.md'}
def write(path,value):(ROOT/path).write_text(json.dumps(value,ensure_ascii=False,indent=2,allow_nan=False)+'\n',encoding='utf-8',newline='\n')
def paths():
    changed=set(c.git('diff','--name-only').splitlines());untracked=set(c.git('ls-files','--others','--exclude-standard').splitlines())
    allowed=set(json.loads((c.OUT/'implementation-plan.json').read_bytes())['modified']);assert changed<=allowed,changed-allowed
    result=changed|untracked|{MANIFEST,PRIVACY}
    assert all(p in allowed or p in NEW or p.startswith('evidence/04A21/') and p.endswith('.json') for p in result),'Unexpected candidate'
    return sorted(result)
def ignored(path,no_index=False):
    cmd=['git','-c','safe.directory='+ROOT.as_posix(),'check-ignore']
    if no_index:cmd+=['--no-index']
    return subprocess.run(cmd+['-q','--',path],cwd=ROOT).returncode
def evidence():
    read=lambda n:json.loads((c.OUT/n).read_bytes())
    assert c.git('rev-parse','HEAD')==c.git('rev-parse','main')==c.BASE==read('entry-state.json')['base']
    assert c.git('branch','--show-current')=='codex/04A-21' and not c.git('diff','--cached','--name-only')
    before=read('entry-validation.json')['configurations'];after=read('validation.json')['configurations']
    for config,d in after.items():
        assert d['new_logical_total']==d['new_logical_passed']>0
        assert not d['new_managed_nonpasses'] and not d['removed_managed_nonpasses']
        assert d['logical_cases']['nonpassed']==before[config]['logical_cases']['nonpassed']
        assert d['runtime_executions']['failed']==before[config]['runtime_executions']['failed']
        assert d['node_passed']==d['node_total']>0
        assert not any('04a21' in p for p in d['python']['nonpasses'])
        assert d['full_command_exit']!=0
    corpus=read('spc-same-core-comparisons-v1.json')
    assert corpus['status']=='NOT_GOLDEN' and corpus['validation']=='SAME_CORE_COMPATIBILITY_ONLY' and not corpus['promoted']
    assert corpus['record_count']==len(corpus['records'])==sum(corpus['outcomes'].values())
    assert corpus['payload_count']==len(corpus['payloads'])
    assert {r['configuration'] for r in corpus['records']}=={'Debug','Release'}
    assert {r['tfm'] for r in corpus['records']}=={'net10.0','net48'}
    assert {'CORE_RETURNED','EXISTING_REJECTION','EXISTING_TWO_LEVEL_REJECTION'}<=set(corpus['outcomes'])
    for key,p in corpus['payloads'].items():
        assert key==hashlib.sha256(json.dumps(p,sort_keys=True,separators=(',',':'),ensure_ascii=False,allow_nan=False).encode()).hexdigest()
        assert p['status']=='NOT_GOLDEN' and {'id','classification','chart','culture','runtime','dataset_sha256'}<=set(p)
    inventory=read('test-inventory.json');assert inventory['logical_count']==after['Debug']['new_logical_total']
    names={k.split('|',1)[1] for k in inventory['cases']}
    for r in corpus['records']:
        assert r['outcome']=='Passed' and r['profile_id'] in corpus['payloads'] and len(r['raw_trx_sha256'])==64
        assert r['test_identity'].replace('"','') in names
    for case in inventory['cases'].values():
        assert case['production_symbols'] and case['expectation'] and case['fixture']
        assert c.sha(ROOT/case['source'])==case['source_sha256']
        assert all(r['outcome']=='Passed' for r in case['executions'])
        assert all(p in corpus['payloads'] for p in case['profile_ids'])
    for path,h in inventory['source_hashes'].items():assert c.sha(ROOT/path)==h,path
    cross=read('original-control-crosswalk.json')
    assert cross['releases']==cross['discharges']==0 and (cross['P'],cross['N1'],cross['N2'],cross['C2'])==('NO','NO GO','NO','UNMET')
    assert len({r['control_id'] for r in cross['controls']})==cross['union']['controls']
    assert len({r['obligation_id'] for r in cross['obligations']})==cross['union']['linked_open_obligations']
    for path,r in cross['authorities'].items():assert c.sha(ROOT/path)==r['sha256']
    for path,r in read('frozen-capture-hashes.json')['files'].items():assert c.sha(ROOT/path)==r['sha256'],path
    assert read('retention.json')['membership_metadata_errors']==0
    q=read('final-source-qualification.json');assert q['compiled_sources_unchanged_after_final_suites']
    for path,h in q['final_input_pins'].items():assert c.sha(ROOT/path)==h,path
    docs=read('documentation-audit.json');assert c.sha(ROOT/docs['page']['path'])==docs['page']['final_sha256']
    assert docs['strict_mkdocs']['final-mkdocs']['exit_code']==0
    assert read('candidate-issues.json')['activated_behavior_fixes']==0
    assert read('privacy.json')['unresolved']==0
    for n in ['spc-point-mapping-v1.json','spc-phase-stage-contracts-v1.json','spc-exclusion-rule-limit-contracts-v1.json']:
        value=read(n);assert value['version']==1 and value['status']=='NOT_GOLDEN / SAME_CORE_COMPATIBILITY_ONLY'
    probes=read('spc-negative-host-seams-v1.json')
    assert probes['status']=='NOT_GOLDEN / SAME_CORE_COMPATIBILITY_ONLY' and len(probes['records'])==6
    assert all(r['matched'] and r['exception_type']=='System.ArgumentException' for r in probes['records'])
    assert read('python-requalification.json')['complete']
    assert inventory['supplemental_comparisons']['sha256']==c.sha(c.OUT/'spc-negative-host-seams-v1.json')

def seal():
    candidates=paths()
    write(PRIVACY,dict(status='PASS_PUBLIC_CANDIDATE_SCAN',candidate_count=len(candidates),unresolved=0,scope='Every candidate path/content; decoded JSON/base64, NFKC/case variants, local owner/host identifiers and profile/drive/SID/credential patterns. Private raw logs remain local with separate SHA references.',assurance='Bounded machine scan and source review; not independent scientific/native verification or proof of absence of arbitrary secrets.'))
    findings,reviewed=privacy.scan(candidates);assert not findings,json.dumps(findings)
    tracked=set(c.git('ls-files').splitlines());files={}
    for p in candidates:
        if p==MANIFEST:continue
        normal,rule=ignored(p),ignored(p,True);assert normal==1 and (rule==1 or p in tracked),p
        if p in NEW:assert all(line.rstrip(' \t')==line for line in (ROOT/p).read_text(encoding='utf-8-sig').splitlines()),p
        files[p]=dict(sha256=c.sha(ROOT/p),bytes=(ROOT/p).stat().st_size,change='modified' if p in tracked else 'new',tracked=p in tracked,role='evidence' if p.startswith('evidence/') else 'documentation' if p.endswith('.md') else 'project' if p.endswith('.vbproj') else 'test' if '/tests/' in p or 'Tests/' in p or '.Test/' in p else 'implementation/verification',git_ignore_exit=normal,git_ignore_rule_exit=rule,status='OWNER_REVIEW_PENDING / UNSTAGED')
    write(MANIFEST,dict(base=c.BASE,head=c.git('rev-parse','HEAD'),branch='codex/04A-21',files=files,candidate_count=len(candidates),self_hash_policy='Only manifest itself excluded from hash map to avoid circularity; membership/privacy/trackability checked separately.',excluded=['All ignored historical/current TestResults/raw logs/TRX/snapshots/drafts/build binaries/caches','Unchanged owner plan/authorities/screenshots/source data/workbooks'],index='EMPTY',publication='OWNER_REVIEW_PENDING / UNSTAGED / UNCOMMITTED / NOT_PUSHED',native=False,R=False,P='NO',N1='NO GO',N2='NO',C2='UNMET',reviewed_false_positive_groups=len(reviewed)))
    verify()
def verify():
    d=json.loads((ROOT/MANIFEST).read_bytes());assert set(d['files'])|{MANIFEST}==set(paths())
    tracked=set(c.git('ls-files').splitlines())
    for p,r in d['files'].items():
        assert c.sha(ROOT/p)==r['sha256'] and (ROOT/p).stat().st_size==r['bytes'],p
        assert ignored(p)==1 and (ignored(p,True)==1 or p in tracked),p
    assert ignored(MANIFEST)==ignored(MANIFEST,True)==1
    assert not c.git('diff','--cached','--name-only')
    assert subprocess.run(['git','-c','safe.directory='+ROOT.as_posix(),'diff','--check'],cwd=ROOT).returncode==0
    findings,_=privacy.scan(paths());assert not findings,json.dumps(findings)
    evidence()
    print('PASS',len(d['files']),'external hashes;',d['candidate_count'],'candidates; schema/links/privacy/trackability; empty index; diff check')
    print('Manifest external SHA-256',c.sha(ROOT/MANIFEST))
if __name__=='__main__':
    p=argparse.ArgumentParser();p.add_argument('action',choices=['seal','verify','evidence']);globals()[p.parse_args().action]()
