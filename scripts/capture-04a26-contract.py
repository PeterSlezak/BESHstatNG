"""Offline entry freeze for official 04A.26. Never writes predecessor evidence."""
import hashlib
import json
import os
from pathlib import Path
import re
import subprocess

ROOT = Path(__file__).resolve().parents[1]
RUN_ID = os.environ.get('BESHSTAT_04A26_RUN', 'run01')
if not re.fullmatch(r'[A-Za-z0-9_-]{1,40}', RUN_ID): raise ValueError('Invalid batch run identifier')
RUN = ROOT / 'TestResults/04A-26' / RUN_ID
OUT = ROOT / 'evidence/04A26'
BASE = '97d54617703287f20886d82bc2896bd95e475770'
FORM = 'BESHstatNG/src/UI/Ui12SampleSizeTtestSingleProp.vb'
PLAN = 'planning/BESHStatNG_CrossPlatform_MasterPlan.md'
def sha(p): return hashlib.sha256(Path(p).read_bytes()).hexdigest()
def git(*args): return subprocess.check_output(['git','-c','safe.directory='+ROOT.as_posix(),*args],cwd=ROOT,text=True).strip()
def clean(v):
    if isinstance(v,dict): return {k:clean(x) for k,x in v.items()}
    if isinstance(v,list): return [clean(x) for x in v]
    if isinstance(v,str):
        value=v.replace(str(ROOT),'<REPO_ROOT>').replace(ROOT.as_posix(),'<REPO_ROOT>')
        value=re.sub(r'(?i)[a-z]:[/\\]+Users[/\\]+[^/\\]+[/\\]+Desktop[/\\]+WPy[^/\\]+[/\\]+python[/\\]+python.exe','<PYTHON_EXE>',value)
        value=re.sub(r'(?i)[a-z]:[/\\]+[^\r\n]*?MSBuild.exe','<MSBUILD_EXE>',value)
        value=re.sub(r'(?i)[a-z]:[/\\]+[^\r\n]*?vstest.console.exe','<VSTEST_EXE>',value)
        value=re.sub(r'(?i)[a-z]:[/\\]+Users[/\\]+[^/\\\s"<>]+','<USER_PROFILE>',value)
        value=re.sub(r'(?i)[/\\]+(?:home|Users)[/\\]+[^/\\\s"<>]+','<HOME_PROFILE>',value)
        value=re.sub(r'https?://[^\s/"<>:@]+:[^\s/"<>@]+@','<REDACTED_CREDENTIAL_URL>@',value)
        value=re.sub(r'\b[A-Za-z0-9._%+-]+@[A-Za-z0-9.-]+\.[A-Za-z]{2,}\b','<REDACTED_EMAIL>',value)
        value=re.sub(r'(?i)\b[a-z]:[/\\]+[^\s"\'<>),]*','<DRIVE_PATH>',value)
        value=re.sub(r'(?i)\b(?:password|pwd|accountkey|client_secret)\s*[=:]\s*[A-Za-z0-9+/]{8,}','<REDACTED_TEST_CREDENTIAL>',value)
        return value
    return v
def emit(name,v):
    (OUT/name).write_text(json.dumps(clean(v),ensure_ascii=False,allow_nan=False,indent=2)+'\n',encoding='utf-8',newline='\n')
def methods(text): return {m.group(2):m.group() for m in re.finditer(r'^    (?:Private|Public|Friend) (?:Shared )?(Sub|Function) (\w+)\b.*?^    End \1',text,re.M|re.S)}
def capture():
    assert git('rev-parse','HEAD') == git('rev-parse','main') == BASE
    assert git('branch','--show-current') == 'codex/04A-26'
    assert not git('diff','--name-only') and not git('diff','--cached','--name-only')
    RUN.mkdir(parents=True,exist_ok=False); OUT.mkdir(exist_ok=False)
    pins={p:sha(ROOT/p) for p in git('ls-files').splitlines() if (ROOT/p).is_file()}
    (RUN/'baseline.json').write_text(json.dumps(dict(base=BASE,pins=pins)),encoding='utf-8')
    history=[]
    for folder in [ROOT/'TestResults',*(ROOT/p/'TestResults' for p in ['BESHStat.Core.Tests','BESHStat.Excel.Common.Tests','BESHStatNG.Test'])]:
        for parent,dirs,files in os.walk(folder):
            dirs[:]=[d for d in dirs if not (Path(parent)/d).is_relative_to(ROOT/'TestResults/04A-26')]
            for name in files:
                p=Path(parent)/name;s=p.stat()
                history.append(dict(path=p.relative_to(ROOT).as_posix(),bytes=s.st_size,mtime_ns=s.st_mtime_ns,ctime_ns=s.st_ctime_ns,ino=s.st_ino,dev=s.st_dev))
    (RUN/'historical-metadata.json').write_text(json.dumps(history),encoding='utf-8')
    frozen={p.relative_to(ROOT).as_posix():dict(sha256=sha(p),bytes=p.stat().st_size,tracked=p.relative_to(ROOT).as_posix() in pins) for prefix in ['evidence','docs/architecture','planning'] for p in (ROOT/prefix).rglob('*') if p.is_file() and not p.is_relative_to(OUT)}
    emit('frozen-capture-hashes.json',dict(files=frozen,scope='Live historical authority/corpus bytes; no prior file altered.'))
    pre=RUN/'preimages';pre.mkdir()
    sources=[FORM,'BESHstatNG/src/UI/UiTwoInputRefedits.vb','BESHstatNG/src/UI/Ui9ANOVA2nested.vb','BESHstatNG/src/UI/Ui0OneRefeditMulticol.vb','BESHStatNG.Test/BESHStatNG.Tests.vbproj','BESHStat.Excel.Common/src/Input/PairedOptionPolicy.vb']
    for p in sources: (pre/Path(p).name).write_bytes((ROOT/p).read_bytes())
    emit('source-preservation.json',dict(entry_files={p:pins[p] for p in sources},protected_sources={p:h for p,h in pins.items() if p.startswith('BESHStat.Core/') or 'Designer.vb' in p or '/ExcelUDFs/' in p},plan_sha256=pins[PLAN]))
    emit('entry-state.json',dict(base=BASE,main=git('rev-parse','main'),head=git('rev-parse','HEAD'),branch=git('branch','--show-current'),parent=git('rev-parse','HEAD^'),tree=git('rev-parse','HEAD^{tree}'),main_reflog=git('reflog','show','main','-3','--format=%h %gs').splitlines(),initial_status=[],index_empty=True,owner_edits=[],acceptance='Owner reports formal 04A.25 acceptance; local main reflog records fast-forward merge codex/04A-25, same HEAD and accepted dossier. Git cannot independently prove a human review decision.',authority={p:pins[p] for p in ['AGENTS.md',PLAN,'docs/architecture/batch-04A-25-owner-review.md','scripts/verify-04a25.py','scripts/verify-beshstatng.ps1']},history_files=len(history),frozen_files=len(frozen),gates=dict(P='NO',N1='NO GO',N2='NO',C2='UNMET'),native_releases=0,deferred_discharges=0))
    emit('prechange-source-inventory.json',dict(files={p:h for p,h in pins.items() if p.endswith(('.vb','.vbproj','.cs','.csproj','.py','.ps1','.cjs','.mjs')) and not p.startswith(('evidence/','TestResults/'))}))
    print('Frozen',len(pins),'tracked files;',len(history),'historical results;',len(frozen),'authorities')
if __name__=='__main__': capture()
