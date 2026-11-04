"""Batch-qualified entry/source inventory; read-only historical authorities."""
import hashlib
import importlib.util
import json
from pathlib import Path
import subprocess

ROOT = Path(__file__).resolve().parents[1]
RUN = ROOT / 'TestResults/04A-21-20261104-run01'
OUT = ROOT / 'evidence/04A21'
BASE = '0f1980e5e223cf4770ce2bebe2d5828daa7ac81e'

def sha(p):
    return hashlib.sha256(Path(p).read_bytes()).hexdigest()

def git(*args):
    return subprocess.check_output(['git', '-c', 'safe.directory='+ROOT.as_posix(), *args], cwd=ROOT, text=True).strip()

def emit(name, value):
    (OUT/name).write_text(json.dumps(value, ensure_ascii=False, indent=2, allow_nan=False)+'\n', encoding='utf-8', newline='\n')

def entry():
    assert git('rev-parse','HEAD') == git('rev-parse','main') == BASE
    assert git('branch','--show-current') == 'codex/04A-21'
    assert not git('diff','--name-only') and not git('diff','--cached','--name-only')
    spec = importlib.util.spec_from_file_location('capture18', ROOT/'scripts/capture-04a18-contract.py')
    prior = importlib.util.module_from_spec(spec); spec.loader.exec_module(prior)
    paths = git('ls-files').splitlines()
    sources = [p for p in paths if p.endswith(('.vb','.cs','.vbproj','.csproj','.cjs','.py','.ps1')) and not p.startswith(('TestResults/','evidence/'))]
    inventory = {}
    for p in sources:
        b = (ROOT/p).read_bytes()
        t = b.decode('utf-8-sig', errors='replace').replace('\r\n','\n')
        methods = prior.members(p,t) if p.endswith('.vb') and any(v in p for v in ['Ui21ControlCharts.vb','SpcTypes.vb','SpcModels.vb','SpcEngine.vb','SpcRules.vb','SpcShewhartCharts.vb','SpcTimeWeightedCharts.vb']) else {}
        inventory[p] = dict(sha256=sha(ROOT/p),bytes=len(b),methods={n:dict(sha256=hashlib.sha256(body.encode()).hexdigest(),first_line=t[:t.index(body)].count('\n')+1) for n,body in methods.items()})
    emit('prechange-method-inventory.json',dict(base=BASE,files=inventory))
    snap = RUN/'preimages'; snap.mkdir(exist_ok=False)
    for p in ['BESHstatNG/src/UI/Ui21ControlCharts.vb']:
        (snap/Path(p).name).write_bytes((ROOT/p).read_bytes())
    frozen = [p for p in paths if p.startswith(('evidence/04A20/','evidence/04A01/','evidence/04A02/','evidence/04A17/','evidence/04A18/','evidence/04A19/','evidence/04A14/','docs/architecture/04A02/'))]
    emit('frozen-capture-hashes.json',dict(interpretation='Entry bytes, never regenerated. Prior payload status and hashes retained; NOT_GOLDEN.',files={p:dict(sha256=sha(ROOT/p),bytes=(ROOT/p).stat().st_size) for p in frozen}))
    emit('entry-state.json',dict(base=BASE,main=git('rev-parse','main'),origin_main=git('rev-parse','origin/main'),branch=git('branch','--show-current'),index_empty=True,tracked_clean=True,ancestry=git('log','-4','--format=%H %s').splitlines(),merge_interpretation='Local main and origin/main include accepted 04A.20 commit and shipping source. Linear history; owner statement supplies acceptance, no separate merge commit invented.',worktrees=1,untracked=git('ls-files','--others','--exclude-standard').splitlines(),gitignore_sha256=sha(ROOT/'.gitignore'),ownership='Per-command safe.directory; no global config change. Authorized branch ref required sandbox escalation.',gates=dict(P='NO',N1='NO GO',N2='NO',C2='UNMET'),native_releases=0))
    emit('implementation-plan.json',dict(batch='04A.21',modified=['BESHstatNG/src/UI/Ui21ControlCharts.vb','BESHStatNG.Test/BESHStatNG.Tests.vbproj','BESHStatNG_Help_MkDocs/docs/methods/control-charts.md'],new=['Common SPC point/stage/exclusion/request policies','Spc21Tests.vb','Spc21AdapterTests.vb','04a21 tooling/Python tests','evidence/04A21/*','docs/architecture/batch-04A-21-owner-review.md'],preserved='All Core numerics/SpcResultTables, Ui22/native renderers/Designers/importers/UDFs, old tests/evidence/captures, other UI and Help metadata, targets/packages/plan',tolerance='Exact same-Core serialized numerical/nonfinite/type/label/table identity. No independent oracle or scientific acceptance.'))
    print('Entry snapshots frozen:',len(inventory),'source entries;',len(frozen),'frozen authorities')

if __name__ == '__main__':
    entry()
