"""Check actual 04A.24 candidate membership, public privacy, hashes and gates."""
import argparse
import importlib.util
import json
from pathlib import Path
import subprocess

ROOT=Path(__file__).resolve().parents[1]
def module(name,path):
    s=importlib.util.spec_from_file_location(name,ROOT/path);m=importlib.util.module_from_spec(s);s.loader.exec_module(m);return m
c=module('capture24','scripts/capture-04a24-contract.py')
privacy=module('privacy14','scripts/verify-04a14-publication.py')
MANIFEST='evidence/04A24/publication-manifest.json';PRIVACY='evidence/04A24/privacy.json';privacy.MANIFEST=MANIFEST
MODIFIED={c.FORM,'BESHStatNG.Test/BESHStatNG.Tests.vbproj','BESHStatNG_Help_MkDocs/docs/methods/multivariate-control-charts.md'}
NEW={*('BESHStat.Excel.Common/src/Analysis/'+n+'.vb' for n in ['SpcMultivariatePresentation','SpcMultivariateReportProjection','SpcMultivariateChartProjectionPolicy']),*('BESHStat.Excel.Common.Tests/src/'+n+'.vb' for n in ['Spc24Tests','Spc24Fixture','Spc24LegacyProjection','Spc24ExpectedRejections']),'BESHStatNG.Test/src/Spc24AdapterTests.vb','scripts/tests/test_04a24_multivariate_payload.py',*('scripts/'+n+'.py' for n in ['capture-04a24-contract','verify-04a24','generate-04a24-projections','integrate-04a24','generate-04a24-expectations','summarize-04a24','verify-04a24-publication']),'docs/architecture/batch-04A-24-owner-review.md'}
def paths():
    modified=set(c.git('diff','--name-only').splitlines());assert modified<=MODIFIED,modified-MODIFIED
    result=modified|set(c.git('ls-files','--others','--exclude-standard').splitlines())|{MANIFEST,PRIVACY}
    assert all(p in MODIFIED or p in NEW or p.startswith('evidence/04A24/') and p.endswith('.json') for p in result),result-(MODIFIED|NEW)
    return sorted(result)
def ignored(p,rule=False):return subprocess.run(['git','-c','safe.directory='+ROOT.as_posix(),'check-ignore']+(['--no-index'] if rule else [])+['-q','--',p],cwd=ROOT).returncode
def write(p,d):(ROOT/p).write_text(json.dumps(d,ensure_ascii=False,allow_nan=False,indent=2)+'\n',encoding='utf-8',newline='\n')
def scan(candidates):
    receipt=json.loads((ROOT/PRIVACY).read_bytes()) if (ROOT/PRIVACY).exists() else {}
    sources=dict(verifier=c.sha(Path(__file__)),scanner=c.sha(ROOT/'scripts/verify-04a14-publication.py'))
    cache=receipt.get('scanned_artifact_hashes',{}) if receipt.get('scanner_source_hashes')==sources and receipt.get('status')=='PASS_PUBLIC_CANDIDATE_SCAN' else {}
    hashes={p:c.sha(ROOT/p) for p in candidates if p not in [MANIFEST,PRIVACY] and (ROOT/p).exists()}
    selected=[p for p in candidates if p in [MANIFEST,PRIVACY] or hashes.get(p)!=cache.get(p)]
    findings,reviewed=privacy.scan(selected)
    return findings,reviewed,dict(scanner_source_hashes=sources,scanned_artifact_hashes=hashes,reused_unchanged_artifacts=len(candidates)-len(selected))
def seal():
    if not (ROOT/PRIVACY).exists():write(PRIVACY,dict(status='INITIAL_UNSCANNED'))
    candidates=paths();findings,reviewed,cache=scan(candidates);assert not findings,json.dumps(findings)
    write(PRIVACY,dict(status='PASS_PUBLIC_CANDIDATE_SCAN',unresolved=0,candidate_count=len(candidates),scope='Actual public candidate bytes, decoded JSON/base64, Unicode/case variants, owner/host/profile/drive/UNC/SID/credential patterns. Raw local logs excluded. Unchanged content reuse requires both artifact and scanner SHA matches; receipts always rescanned.',**cache))
    findings,_=privacy.scan([PRIVACY]);assert not findings,json.dumps(findings)
    tracked=set(c.git('ls-files').splitlines());files={}
    for p in candidates:
        if p==MANIFEST:continue
        assert (ROOT/p).is_file(),p
        normal,rule=ignored(p),ignored(p,True);assert normal==1 and (rule==1 or p in tracked),p
        files[p]=dict(sha256=c.sha(ROOT/p),bytes=(ROOT/p).stat().st_size,mode='100644',change='modified' if p in tracked else 'new',tracked=p in tracked,role='evidence' if p.startswith('evidence/') else 'documentation' if p.endswith('.md') else 'project' if p.endswith('.vbproj') else 'test' if 'Tests/' in p or '.Test/' in p or '/tests/' in p else 'implementation/verification',git_ignore_exit=normal,git_ignore_rule_exit=rule)
    write(MANIFEST,dict(base=c.BASE,head=c.git('rev-parse','HEAD'),branch=c.git('branch','--show-current'),files=files,candidate_count=len(candidates),self_hash_policy='Only this manifest excluded from its own hash map; membership, privacy and Git trackability separately verified. Privacy receipt hashes all external candidates except itself/manifest to avoid cycles.',publication='OWNER_REVIEW_PENDING / UNSTAGED / UNCOMMITTED / NOT_PUSHED',excluded='All ignored historical/local TestResults, binaries and raw traces. All unchanged authority/evidence/captures; no old ZIP normalization differences included.',index='EMPTY',native_releases=0,deferred_discharges=0,P='NO',N1='NO GO',N2='NO',C2='UNMET',reviewed_false_positive_groups=len(reviewed)))
    verify(False)
def verify(final=True):
    d=json.loads((ROOT/MANIFEST).read_bytes());assert set(d['files'])|{MANIFEST}==set(paths())
    assert c.git('rev-parse','HEAD')==c.git('rev-parse','main')==c.BASE
    assert c.git('branch','--show-current')=='codex/04A-24'
    assert not c.git('diff','--cached','--name-only')
    tracked=set(c.git('ls-files').splitlines())
    for p,r in d['files'].items():
        assert c.sha(ROOT/p)==r['sha256'] and (ROOT/p).stat().st_size==r['bytes'],p
        assert ignored(p)==1 and (ignored(p,True)==1 or p in tracked),p
    assert ignored(MANIFEST)==ignored(MANIFEST,True)==1
    assert subprocess.run(['git','-c','safe.directory='+ROOT.as_posix(),'diff','--check'],cwd=ROOT).returncode==0
    findings,_,_=scan(paths());assert not findings,json.dumps(findings)
    for p,r in json.loads((c.OUT/'frozen-capture-hashes.json').read_bytes())['files'].items():assert c.sha(ROOT/p)==r['sha256'],p
    if final:
        for row in json.loads((c.OUT/'validation.json').read_bytes())['configurations'].values():
            assert row['new_logical_total']==row['new_logical_passed']>0
            assert not any('Spc24' in str(r) for r in row['new_managed_nonpasses'])
            assert not any('04a24' in r for r in row['python']['nonpasses'])
            assert row['node_total']==row['node_passed']>0
            assert not row['lanes']['errors'] and row['lanes']['max_active_command_roots']<=4
        assert json.loads((c.OUT/'final-source-qualification.json').read_bytes())['compiled_sources_unchanged_after_final_suites']
        assert json.loads((c.OUT/'retention.json').read_bytes())['membership_metadata_errors']==0
        assert json.loads((c.OUT/'documentation-audit.json').read_bytes())['strict_mkdocs']['final-serial-mkdocs']==0
    print('PASS',len(d['files']),'external hashes;',d['candidate_count'],'candidates; privacy/trackability/frozen authority/index/diff')
if __name__=='__main__':
    p=argparse.ArgumentParser();p.add_argument('action',choices=['seal','verify']);a=p.parse_args();globals()[a.action]()
