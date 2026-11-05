"""Check 04A.23 actual candidate membership, hashes, privacy and unchanged authority."""
import argparse
import importlib.util
import json
from pathlib import Path
import subprocess

ROOT=Path(__file__).resolve().parents[1]
def module(name,path):
    spec=importlib.util.spec_from_file_location(name,ROOT/path)
    result=importlib.util.module_from_spec(spec);spec.loader.exec_module(result);return result
c=module('capture22','scripts/capture-04a23-contract.py')
privacy=module('privacy14','scripts/verify-04a14-publication.py')
MANIFEST='evidence/04A23/publication-manifest.json'
PRIVACY='evidence/04A23/privacy.json'
privacy.MANIFEST=MANIFEST
MODIFIED={c.FORM,'BESHStatNG.Test/BESHStatNG.Tests.vbproj','BESHStatNG_Help_MkDocs/docs/methods/multivariate-control-charts.md'}
NEW={
    *('BESHStat.Excel.Common/src/Analysis/'+n+'.vb' for n in ['SpcMultivariatePointMapPolicy','SpcMultivariatePhaseExclusionPolicy','SpcMultivariateRequestPolicy','SpcMultivariateResultAxisPolicy']),
    *('BESHStat.Excel.Common.Tests/src/'+n+'.vb' for n in ['Spc23Tests','Spc23Fixture','Spc23Data','Spc23LegacyPreparation']),
    'BESHStatNG.Test/src/Spc23AdapterTests.vb','scripts/tests/test_04a23_multivariate_spc.py',
    *('scripts/'+n+'.py' for n in ['capture-04a23-contract','verify-04a23','generate-04a23-legacy','generate-04a23-fixture','summarize-04a23','verify-04a23-publication']),
    'docs/architecture/batch-04A-23-owner-review.md'}

def paths():
    changed=set(c.git('diff','--name-only').splitlines())
    assert c.sha(ROOT/c.OWNER_PLAN)==json.loads((c.OUT/'owner-plan-observation.json').read_bytes())['working_sha256']
    changed.discard(c.OWNER_PLAN)
    assert changed<=MODIFIED,changed-MODIFIED
    result=changed|set(c.git('ls-files','--others','--exclude-standard').splitlines())|{MANIFEST,PRIVACY}
    assert all(p in MODIFIED or p in NEW or p.startswith('evidence/04A23/') and p.endswith('.json') for p in result),result
    return sorted(result)

def ignored(p,rule=False):
    return subprocess.run(['git','-c','safe.directory='+ROOT.as_posix(),'check-ignore']+(['--no-index'] if rule else [])+['-q','--',p],cwd=ROOT).returncode

def write(p,value):
    (ROOT/p).write_text(json.dumps(value,ensure_ascii=False,indent=2,allow_nan=False)+'\n',encoding='utf-8',newline='\n')

def scan(candidates):
    """Reuse an actual prior scan only for identical bytes and identical scanners.

    The large presentation corpus is immutable between suite runs. Every call
    still hashes every candidate; changed artifacts and both self-referential
    receipts are scanned. A source change invalidates the entire scan cache.
    """
    receipt=json.loads((ROOT/PRIVACY).read_bytes()) if (ROOT/PRIVACY).exists() else {}
    source_hashes=dict(verifier=c.sha(Path(__file__)),scanner=c.sha(ROOT/'scripts/verify-04a14-publication.py'))
    cached=receipt.get('scanned_artifact_hashes',{}) if receipt.get('scanner_source_hashes')==source_hashes and receipt.get('status')=='PASS_PUBLIC_CANDIDATE_SCAN' else {}
    hashes={p:c.sha(ROOT/p) for p in candidates if p not in [MANIFEST,PRIVACY] and (ROOT/p).is_file()}
    selected=[p for p in candidates if p in [MANIFEST,PRIVACY] or hashes.get(p)!=cached.get(p)]
    findings,reviewed=privacy.scan(selected)
    return findings,reviewed,dict(scanner_source_hashes=source_hashes,scanned_artifact_hashes=hashes,reused_unchanged_artifacts=len(candidates)-len(selected),freshly_scanned_paths=selected)

def seal():
    if not (ROOT/PRIVACY).exists():write(PRIVACY,dict(status='INITIAL_UNSCANNED'))
    candidates=paths()
    findings,reviewed,cache=scan(candidates)
    assert not findings,json.dumps(findings)
    write(PRIVACY,dict(status='PASS_PUBLIC_CANDIDATE_SCAN',unresolved=0,candidate_count=len(candidates),scope='Every actual candidate path/content, decoded JSON and base64, Unicode NFKC/case variants, local owner/host identifiers, profile/drive/UNC/SID/credential patterns. Raw local logs/identities excluded; public placeholders used.',assurance='Bounded pattern scan and targeted source review; no independent native/scientific verification. Identical artifacts reuse prior scans only with matching artifact and scanner SHA-256 values; both receipts are always scanned.',**cache))
    findings,_=privacy.scan([PRIVACY]);assert not findings,json.dumps(findings)
    tracked=set(c.git('ls-files').splitlines())
    files={}
    for p in candidates:
        if p==MANIFEST:continue
        normal,rule=ignored(p),ignored(p,True)
        assert normal==1 and (rule==1 or p in tracked),p
        files[p]=dict(sha256=c.sha(ROOT/p),bytes=(ROOT/p).stat().st_size,change='modified' if p in tracked else 'new',mode='100644',tracked=p in tracked,role='evidence' if p.startswith('evidence/') else 'documentation' if p.endswith('.md') else 'project' if p.endswith('.vbproj') else 'test' if '/tests/' in p or 'Tests/' in p or '.Test/' in p else 'implementation/verification',git_ignore_exit=normal,git_ignore_rule_exit=rule)
    write(MANIFEST,dict(base=c.BASE,head=c.git('rev-parse','HEAD'),branch=c.git('branch','--show-current'),files=files,candidate_count=len(candidates),self_hash_policy='Only this manifest excluded from its own hash map; manifest membership, privacy and trackability checked separately. No circular hashes.',publication='OWNER_REVIEW_PENDING / UNSTAGED / UNCOMMITTED / NOT_PUSHED',excluded='All ignored TestResults/raw logs/TRX/snapshots/drafts/build binaries/caches; unchanged historical evidence/captures/data/workbooks; separately pinned preexisting owner Revision 2.9 plan edit excluded from batch candidates.',index='EMPTY',native_releases=0,deferred_discharges=0,P='NO',N1='NO GO',N2='NO',C2='UNMET',reviewed_false_positive_groups=len(reviewed)))
    verify(check_receipts=False)

def verify(check_receipts=True):
    d=json.loads((ROOT/MANIFEST).read_bytes())
    assert set(d['files'])|{MANIFEST}==set(paths())
    assert c.git('rev-parse','HEAD')==c.git('rev-parse','main')==c.BASE
    assert c.git('branch','--show-current')=='codex/04A-23'
    assert not c.git('diff','--cached','--name-only')
    tracked=set(c.git('ls-files').splitlines())
    for p,r in d['files'].items():
        assert c.sha(ROOT/p)==r['sha256'] and (ROOT/p).stat().st_size==r['bytes'],p
        assert ignored(p)==1 and (ignored(p,True)==1 or p in tracked),p
    assert ignored(MANIFEST)==ignored(MANIFEST,True)==1
    assert subprocess.run(['git','-c','safe.directory='+ROOT.as_posix(),'diff','--check'],cwd=ROOT).returncode==0
    findings,_,_=scan(paths());assert not findings,json.dumps(findings)
    for p,r in json.loads((c.OUT/'frozen-capture-hashes.json').read_bytes())['files'].items():assert c.sha(ROOT/p)==r['sha256'],p
    if check_receipts:
        validation=json.loads((c.OUT/'validation.json').read_bytes())['configurations']
        for config,row in validation.items():
            assert row['new_logical_total']==row['new_logical_passed']>0
            assert not any('Spc23' in str(x) for x in row['new_managed_nonpasses'])
            assert not any('04a23' in x for x in row['python']['nonpasses'])
            assert row['node_passed']==row['node_total']>0
        q=json.loads((c.OUT/'final-source-qualification.json').read_bytes())
        assert q['compiled_sources_unchanged_after_final_suites']
        for p,h in q['final_input_pins'].items():assert c.sha(ROOT/p)==h,p
        retention=json.loads((c.OUT/'retention.json').read_bytes())
        assert retention['membership_metadata_errors']==0
        assert json.loads((c.OUT/'documentation-audit.json').read_bytes())['strict_mkdocs']['final-mkdocs']==0
        assert json.loads((c.OUT/'candidate-issues.json').read_bytes())['activated_behavior_fixes']==0
    print('PASS',len(d['files']),'external hashes;',d['candidate_count'],'candidates; privacy/trackability; empty index; diff check')

if __name__=='__main__':
    parser=argparse.ArgumentParser();parser.add_argument('action',choices=['seal','verify']);args=parser.parse_args();globals()[args.action]()
