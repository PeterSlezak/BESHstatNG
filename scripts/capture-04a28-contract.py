"""Read-only entry qualification; only new 04A28 metadata/private run is written."""
from datetime import datetime, timezone
import hashlib
import json
import os
from pathlib import Path
import re
import subprocess

ROOT=Path(__file__).resolve().parents[1]
BASE='033cbe9033cc10bce3e19322e5582e5ea389fe22'
RUN=ROOT/'.test-rationalization-04a28-run01.tmp'
OUT=ROOT/'evidence/04A28'
WRITER='BESHstatNG/src/DM/ResultWriter/ExcelDnaResultWriter.vb'
ADAPTER='BESHstatNG/src/DM/ResultWriter/ExcelDnaSharedOutputPlanAdapter.vb'
def sha(p):return hashlib.sha256(Path(p).read_bytes()).hexdigest()
def git(*args):return subprocess.check_output(['git','-c','safe.directory='+ROOT.as_posix(),*args],cwd=ROOT,text=True).strip()
def clean(v):
    if isinstance(v,dict):return {k:clean(x) for k,x in v.items()}
    if isinstance(v,(list,tuple)):return [clean(x) for x in v]
    if isinstance(v,str):
        v=v.replace(str(ROOT),'<REPO_ROOT>').replace(ROOT.as_posix(),'<REPO_ROOT>')
        v=re.sub(r'(?i)[a-z]:[/\\]+Users[/\\]+[^/\\]+[/\\]+Desktop[/\\]+WPy[^/\\]+[/\\]+python[/\\]+python.exe','<PYTHON_EXE>',v)
        v=re.sub(r'(?i)[a-z]:[/\\]+[^\r\n]*?MSBuild.exe','<MSBUILD_EXE>',v)
        v=re.sub(r'(?i)[a-z]:[/\\]+[^\r\n]*?vstest.console.exe','<VSTEST_EXE>',v)
        v=re.sub(r'(?i)[a-z]:[/\\]+Users[/\\]+[^/\\\s"<>]+','<USER_PROFILE>',v)
        v=re.sub(r'\bS-1-\d+(?:-\d+)+\b','<SID>',v)
        return v
    return v
def save(path,data,sanitize=False):
    path.parent.mkdir(parents=True,exist_ok=True)
    with path.open('x',encoding='utf-8',newline='\n') as f:json.dump(clean(data) if sanitize else data,f,ensure_ascii=False,allow_nan=False,separators=(',',':'));f.write('\n')
def emit(name,data):save(OUT/name,data,True)
def result_membership():
    rows=[]
    for parent,dirs,files in os.walk(ROOT/'TestResults'):
        for name in files:
            p=Path(parent)/name;s=p.stat()
            rows.append([p.relative_to(ROOT).as_posix(),s.st_size,s.st_mtime_ns,s.st_ctime_ns,s.st_ino])
    return rows
def main():
    assert git('rev-parse','HEAD')==git('rev-parse','main')==BASE
    assert git('branch','--show-current')=='codex/04A-28'
    assert not git('diff','--name-only') and not git('diff','--cached','--name-only')
    ancestors={}
    for branch in ['codex/04A-26','codex/04A-27','codex/test-suite-rationalization']:
        subprocess.run(['git','-c','safe.directory='+ROOT.as_posix(),'merge-base','--is-ancestor',branch,'main'],check=True)
        ancestors[branch]=git('rev-parse',branch)
    RUN.mkdir(exist_ok=False);OUT.mkdir(exist_ok=False)
    sources={p:sha(ROOT/p) for p in git('ls-files').splitlines() if p.endswith(('.vb','.cs','.vbproj','.csproj','.py','.ps1','.runsettings','.props','.targets','.cjs','.mjs','.xml','.resx')) and not p.startswith('TestResults/') and (ROOT/p).is_file()}
    save(RUN/'source-pins.json',sources)
    frozen={}
    for prefix in ['evidence','docs/architecture','planning']:
        for p in (ROOT/prefix).rglob('*'):
            if p.is_file() and not p.is_relative_to(OUT):frozen[p.relative_to(ROOT).as_posix()]=dict(sha256=sha(p),bytes=p.stat().st_size)
    save(RUN/'frozen-pins.json',frozen)
    historic=result_membership();save(RUN/'historical-result-metadata.json',historic)
    pre=RUN/'preimages';pre.mkdir()
    paths=[WRITER,ADAPTER,'BESHStatNG.Test/BESHStatNG.Tests.vbproj','BESHstatNG/src/DM/SheetManagement.vb','BESHstatNG/src/UI/UibyID.vb','BESHstatNG/src/UI/Ui82x2.vb','BESHstatNG/src/UI/UiGLM.vb','BESHstatNG/src/UI/Ui13GEE.vb','BESHStatNG_Help_MkDocs/docs/global-settings.md']
    for p in paths:(pre/Path(p).name).write_bytes((ROOT/p).read_bytes())
    registry=json.loads((ROOT/'evidence/04A14/baseline-registry.json').read_bytes())
    assert len(registry['files'])==100
    assert all(sha(ROOT/p)==v['sha256'] for p,v in registry['files'].items())
    emit('entry-state.json',dict(base=BASE,main=BASE,head=BASE,branch=git('branch','--show-current'),initial_worktree=[],index_empty=True,owner_exclusivity='Explicit fresh owner confirmation for official 04A28 in active conversation',owner_acceptances='04A26, out-of-sequence 04A27 and test rationalization accepted/committed/merged by owner; ancestry verified',excluded_owner_edits=[],started_utc=datetime.now(timezone.utc).isoformat(),gates=dict(P='NO',N1='NO GO',N2='NO',C2='UNMET'),native_releases=0,obligation_discharges=0))
    emit('entry-merge-and-test-rationalization-qualification.json',dict(ancestors=ancestors,main=BASE,test_project_sha256=sha(ROOT/'BESHStatNG.Test/BESHStatNG.Tests.vbproj'),runner_sha256=sha(ROOT/'scripts/test_suite_rationalization.py'),prior_validation_sha256=sha(ROOT/'evidence/TestRationalization/validation-v1.json'),entry_reference='Accepted merged maintenance receipts; historical context, not invented fresh entry verdicts',historical_debug=dict(logical_shared_source=dict(total=11921,passed=11908),logical_continuity=dict(total=11968,passed=11955),raw=dict(total=19519,passed=19505,failed=14),Node=916,Python=dict(total=944,passed=838,FAIL=96,ERROR=10))))
    emit('source-preservation.json',dict(entry_sources={p:sources.get(p,sha(ROOT/p)) for p in paths},protected_compiled_sources={p:h for p,h in sources.items() if p.startswith('BESHStat.Core/') or 'Designer.vb' in p or '/ExcelUDFs/' in p or '/Graphics/' in p or p.endswith(('ExcelDnaDataImporter.vb','SpcShewhartChartTests.vb','SpcTimeWeightedChartTests.vb','SpcMultivariateTests.vb','SpcRuleEvaluatorTests.vb','SpcResultTablesAndDispatchTests.vb'))},plan_sha256=sha(ROOT/'planning/BESHStatNG_CrossPlatform_MasterPlan.md')))
    emit('frozen-capture-hashes.json',dict(files=frozen,original_mixed_model_captures=100,registry_sha256=sha(ROOT/'evidence/04A14/baseline-registry.json'),historical_TestResults_files=len(historic),historical_result_qualification='Exact membership/stat identity retained privately; original TestResults bytes not all rehashed. Every earlier evidence/architecture authority byte was hashed. No historical record modified.'))
    print('04A28 entry frozen:',len(sources),'source inputs,',len(frozen),'authority/evidence hashes,',len(historic),'historical result stat rows')
if __name__=='__main__':main()
