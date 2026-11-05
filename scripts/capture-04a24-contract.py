"""Freeze actual merged 04A.24 entry; no network, native execution or historical writes."""
import hashlib
import importlib.util
import json
import os
from pathlib import Path
import re
import subprocess

ROOT=Path(__file__).resolve().parents[1]
RUN=ROOT/'TestResults/04A-24/run01'
OUT=ROOT/'evidence/04A24'
BASE='44b32955bb4f1b3aa43a9b65e42564c4fc9d436c'
FORM='BESHstatNG/src/UI/Ui22MultivariateControlCharts.vb'
PLAN='planning/BESHStatNG_CrossPlatform_MasterPlan.md'
def sha(p):return hashlib.sha256(Path(p).read_bytes()).hexdigest()
def git(*args):return subprocess.check_output(['git','-c','safe.directory='+ROOT.as_posix(),*args],cwd=ROOT,text=True).strip()
def module(name,path):
    s=importlib.util.spec_from_file_location(name,ROOT/path);m=importlib.util.module_from_spec(s);s.loader.exec_module(m);return m
previous=module('summary23','scripts/summarize-04a23.py')
def clean(v):return previous.clean(v)
def emit(name,v):
    (OUT/name).write_text(json.dumps(clean(v),ensure_ascii=False,allow_nan=False,indent=2)+'\n',encoding='utf-8',newline='\n')
def methods(text):return {m.group(2):m.group() for m in re.finditer(r'^    (?:Private|Public|Friend) (?:Shared )?(Sub|Function) (\w+)\b.*?^    End \1',text,re.M|re.S)}
def capture():
    assert git('rev-parse','HEAD')==git('rev-parse','main')==git('rev-parse','origin/main')==BASE
    assert git('branch','--show-current')=='codex/04A-24'
    assert not git('diff','--name-only') and not git('diff','--cached','--name-only')
    RUN.mkdir(parents=True,exist_ok=False);OUT.mkdir(exist_ok=False)
    pins={p:sha(ROOT/p) for p in git('ls-files').splitlines() if (ROOT/p).is_file()}
    (RUN/'baseline.json').write_text(json.dumps(dict(base=BASE,pins=pins)),encoding='utf-8')
    history=[]
    for folder in [ROOT/'TestResults',*(ROOT/p/'TestResults' for p in ['BESHStat.Core.Tests','BESHStat.Excel.Common.Tests','BESHStatNG.Test'])]:
        for parent,_,files in os.walk(folder):
            for name in files:
                p=Path(parent)/name
                if p.is_relative_to(ROOT/'TestResults/04A-24'):continue
                s=p.stat();history.append(dict(path=p.relative_to(ROOT).as_posix(),bytes=s.st_size,mtime_ns=s.st_mtime_ns,ctime_ns=s.st_ctime_ns,ino=s.st_ino,dev=s.st_dev))
    (RUN/'historical-metadata.json').write_text(json.dumps(history),encoding='utf-8')
    frozen={p.relative_to(ROOT).as_posix():dict(sha256=sha(p),bytes=p.stat().st_size,tracked=p.relative_to(ROOT).as_posix() in pins) for prefix in ['evidence','docs/architecture','planning'] for p in (ROOT/prefix).rglob('*') if p.is_file() and not p.is_relative_to(OUT)}
    emit('frozen-capture-hashes.json',dict(files=frozen,scope='Actual live bytes, including existing ignored historical corpora; history membership/metadata separately frozen. No archive used to repair history.'))
    raw=(ROOT/FORM).read_text(encoding='utf-8-sig');m=methods(raw)
    emit('prechange-method-provenance.json',dict(source=FORM,source_sha256=pins[FORM],main_blob_sha256=hashlib.sha256(subprocess.check_output(['git','-c','safe.directory='+ROOT.as_posix(),'show',BASE+':'+FORM],cwd=ROOT)).hexdigest(),methods={n:dict(body=b,sha256=hashlib.sha256(b.encode()).hexdigest(),first_line=raw[:raw.index(b)].count('\n')+1,last_line=raw[:raw.index(b)+len(b)].count('\n')+1) for n,b in m.items()},hashing='Exact file bytes; method hash is normalized LF UTF-8.'))
    source={p:h for p,h in pins.items() if p.endswith(('.vb','.vbproj','.cs','.csproj','.py','.ps1','.cjs','.mjs')) and not p.startswith(('evidence/','TestResults/'))}
    emit('prechange-source-inventory.json',dict(files=source))
    qualified=json.loads((ROOT/'evidence/04A23/final-source-qualification.json').read_bytes())['final_input_pins'];comparisons=[]
    for p,h in qualified.items():
        b=(ROOT/p).read_bytes();actual=sha(ROOT/p);lf=b.replace(b'\r\n',b'\n');line_hashes={hashlib.sha256(v).hexdigest() for v in [lf,lf.replace(b'\n',b'\r\n')]}
        comparisons.append(dict(path=p,previous_qualified_sha256=h,actual_entry_sha256=actual,classification='EXACT' if actual==h else 'LINE_ENDINGS_ONLY' if h in line_hashes else 'SOURCE_DIFF_REQUIRES_BASELINE_WIDENING'))
    assert all(r['classification']!='SOURCE_DIFF_REQUIRES_BASELINE_WIDENING' for r in comparisons),comparisons
    emit('entry-source-qualification.json',dict(previous_qualification_sha256=sha(ROOT/'evidence/04A23/final-source-qualification.json'),comparisons=comparisons,decision='Reuse accepted full 04A.23 receipts for inherited identities; compile and run focused live Core/SPC22/23 checks, plus source guards. No redundant complete entry matrix.'))
    for p in ['entry-state','validation','nonpasses','original-control-crosswalk','source-preservation','publication-manifest']:
        d=json.loads((ROOT/('evidence/04A23/'+p+'.json')).read_bytes())
        assert isinstance(d,dict)
    emit('entry-state.json',dict(base=BASE,branch=git('branch','--show-current'),main=git('rev-parse','main'),origin_main=git('rev-parse','origin/main'),parent=git('rev-parse','HEAD^'),tree=git('rev-parse','HEAD^{tree}'),initial_main_clean=True,index_empty=True,owner_edits=[],acceptance='Owner explicitly accepted/merged 04A.23; actual main commit contains its production contracts, real Ui22 consumers, tests and dossier. Actual merge source is 44b3295, parent519e875; no ZIP inference/fetch.',plan=dict(path=PLAN,sha256=pins[PLAN],revision='2.9',progress='Stale 04A.23 next narrative retained; later explicit owner acceptance controls scope.'),historical_files=len(history),frozen_files=len(frozen),gates=dict(P='NO',N1='NO GO',N2='NO',C2='UNMET',C3='UNCHANGED',E1_E7='UNCHANGED'),native_releases=0,deferred_discharges=0))
    pre=RUN/'preimages';pre.mkdir()
    for p in [FORM,'BESHStatNG.Test/BESHStatNG.Tests.vbproj','BESHStatNG_Help_MkDocs/docs/methods/multivariate-control-charts.md']:(pre/Path(p).name).write_bytes((ROOT/p).read_bytes())
    emit('implementation-plan.json',dict(modified=[FORM,'BESHStatNG.Test/BESHStatNG.Tests.vbproj','BESHStatNG_Help_MkDocs/docs/methods/multivariate-control-charts.md'],new=['Focused Common multivariate family/chart/report/warning projections','Spc24 managed/Windows/Python contracts and entry-source legacy helper','Batch-local four-lane verifier/accounting/evidence','docs/architecture/batch-04A-24-owner-review.md'],protected='Core algorithms/SpcResultTables, Ui21, renderer/old Common APIs, Designers/UDFs/Ribbon, metadata, targets/packages, all old source tests/evidence/captures and TestResults.'))
    print('Frozen',len(pins),'tracked files;',len(history),'historical results;',len(frozen),'authority files; previous compiled qualification matches')
if __name__=='__main__':capture()
