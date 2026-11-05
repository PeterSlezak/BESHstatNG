"""Freeze actual 04A.23 entry and owner-provided plan before production edits."""
import hashlib
import json
from pathlib import Path
import re
import subprocess

ROOT=Path(__file__).resolve().parents[1]
RUN=ROOT/'TestResults/04A-23-20261105-run01'
OUT=ROOT/'evidence/04A23'
BASE='519e875a329111270685a0ae4be8666abb55ae04'
FORM='BESHstatNG/src/UI/Ui22MultivariateControlCharts.vb'
OWNER_PLAN='planning/BESHStatNG_CrossPlatform_MasterPlan.md'
RENDERER='BESHstatNG/src/Graphics/SpcControlChartExcel.vb'

def sha(p):return hashlib.sha256(Path(p).read_bytes()).hexdigest()
def git(*args):return subprocess.check_output(['git','-c','safe.directory='+ROOT.as_posix(),*args],cwd=ROOT,text=True).strip()
def emit(name,value):
    (OUT/name).write_text(json.dumps(value,ensure_ascii=False,indent=2,allow_nan=False)+'\n',encoding='utf-8',newline='\n')
def members(text):
    return {m.group(2):m.group() for m in re.finditer(r'^    (?:Private|Public|Friend) (?:Shared )?(Sub|Function) (\w+)\b.*?^    End \1',text,re.M|re.S)}

def capture():
    assert git('branch','--show-current')=='codex/04A-23'
    assert git('rev-parse','HEAD')==git('rev-parse','main')==git('rev-parse','origin/main')==BASE
    assert not git('diff','--cached','--name-only')
    assert set(git('diff','--name-only').splitlines())=={OWNER_PLAN}
    assert (RUN/'start.json').exists()
    paths=git('ls-files').splitlines()
    source=[p for p in paths if p.endswith(('.vb','.cs','.vbproj','.csproj','.py','.ps1','.cjs')) and not p.startswith(('TestResults/','evidence/'))]
    emit('prechange-source-inventory.json',dict(base=BASE,files={p:dict(sha256=sha(ROOT/p),bytes=(ROOT/p).stat().st_size) for p in source}))
    raw=(ROOT/FORM).read_text(encoding='utf-8-sig')
    emit('prechange-method-provenance.json',dict(source=FORM,source_sha256=sha(ROOT/FORM),methods={n:dict(body=b,sha256=hashlib.sha256(b.encode()).hexdigest(),first_line=raw[:raw.index(b)].count('\n')+1,last_line=raw[:raw.index(b)+len(b)].count('\n')+1) for n,b in members(raw).items()},hashing='Exact whole working-file SHA; method statements/boundaries hashed as decoded UTF-8 with normalized LF.'))
    pre=RUN/'preimages';pre.mkdir(exist_ok=True)
    for p in [FORM,OWNER_PLAN,'BESHStatNG.Test/BESHStatNG.Tests.vbproj']:
        dest=pre/Path(p).name
        with dest.open('xb') as f:f.write((ROOT/p).read_bytes())
    original_plan=subprocess.check_output(['git','-c','safe.directory='+ROOT.as_posix(),'show',BASE+':'+OWNER_PLAN],cwd=ROOT)
    emit('owner-plan-observation.json',dict(path=OWNER_PLAN,working_sha256=sha(ROOT/OWNER_PLAN),main_git_blob_sha256=hashlib.sha256(original_plan).hexdigest(),revision='2.9',ownership='Owner edit appeared between initial clean-main observation and approved branch creation. Retained exact bytes, excluded from 04A.23 implementation/publication scope. Never edited by this batch.',initial_observation='main clean at 519e875; live plan initially 2.8, then owner installed 2.9 while branch approval pending.'))
    frozen=[p for p in paths if p.startswith(('evidence/','docs/architecture/','planning/')) and p!=OWNER_PLAN]
    authorities=json.loads((ROOT/'evidence/04A22/original-control-crosswalk.json').read_bytes())['authorities']
    frozen=sorted(set(frozen)|set(authorities))
    emit('frozen-capture-hashes.json',dict(files={p:dict(sha256=sha(ROOT/p),bytes=(ROOT/p).stat().st_size) for p in frozen},method='Live entry exact bytes; historical ZIP/source-manifest differences are not rewritten. Historical TestResults membership/metadata separately pinned. Owner plan working hash separately protected.'))
    emit('entry-state.json',dict(base=BASE,main=BASE,origin_main=git('rev-parse','origin/main'),parent=git('rev-parse','HEAD^'),tree=git('rev-parse','HEAD^{tree}'),branch=git('branch','--show-current'),index_empty=True,initial_main_clean=True,preexisting_owner_edits=[OWNER_PLAN],worktrees=1,acceptance='Owner explicitly accepted/merged 04A.22. Actual main is its shipping commit 519e875, descendant of 9330f8d; original policies/renderer consumers/tests/evidence present. No invented merge commit and no fetch.',gates=dict(P='NO',N1='NO GO',N2='NO',C2='UNMET'),native_releases=0,deferred_discharges=0,protected_prior_implementation={p:sha(ROOT/p) for p in paths if '/Analysis/Spc' in p or p in [RENDERER,'BESHstatNG/src/UI/Ui21ControlCharts.vb']}))
    emit('implementation-plan.json',dict(batch='04A.23',modified=[FORM,'BESHStatNG.Test/BESHStatNG.Tests.vbproj','BESHStatNG_Help_MkDocs/docs/methods/multivariate-control-charts.md'],new=['Common multivariate point/stage/exclusion/request/result context contracts','New Spc23 Common/Windows/Python tests and frozen legacy helper','04a23 capture/verification/accounting/publication tooling','evidence/04A23/*','docs/architecture/batch-04A-23-owner-review.md'],owner_edit_excluded=OWNER_PLAN,preserved='All Core algorithms, Ui21/Common21-22/shared renderer, Designer/UDF/Ribbon/Office UI, old tests/evidence/captures and targets/packages; no native/scientific releases.'))
    print('Entry frozen:',len(source),'source files;',len(frozen),'historical authorities; owner plan protected')

if __name__=='__main__':capture()
