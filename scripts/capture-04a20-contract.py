"""Batch-qualified entry/source inventory; read-only historical authorities."""
import hashlib
import importlib.util
import json
from pathlib import Path
import subprocess

ROOT = Path(__file__).resolve().parents[1]
RUN = ROOT / 'TestResults/04A-20-20261104-run01'
OUT = ROOT / 'evidence/04A20'
BASE = 'ae7490bbce241dd8a444fd4d23ba413fb89a36ca'

def sha(p):
    return hashlib.sha256(Path(p).read_bytes()).hexdigest()

def git(*args):
    return subprocess.check_output(['git', '-c', 'safe.directory='+ROOT.as_posix(), *args], cwd=ROOT, text=True).strip()

def emit(name, value):
    (OUT/name).write_text(json.dumps(value, ensure_ascii=False, indent=2, allow_nan=False)+'\n', encoding='utf-8', newline='\n')

def entry():
    assert git('rev-parse','HEAD') == git('rev-parse','main') == BASE
    assert git('branch','--show-current') == 'codex/04A-20'
    assert not git('diff','--name-only') and not git('diff','--cached','--name-only')
    spec = importlib.util.spec_from_file_location('capture18', ROOT/'scripts/capture-04a18-contract.py')
    prior = importlib.util.module_from_spec(spec); spec.loader.exec_module(prior)
    paths = git('ls-files').splitlines()
    sources = [p for p in paths if p.endswith(('.vb','.cs','.vbproj','.csproj','.cjs','.py','.ps1')) and not p.startswith(('TestResults/','evidence/'))]
    inventory = {}
    for p in sources:
        b = (ROOT/p).read_bytes()
        t = b.decode('utf-8-sig', errors='replace').replace('\r\n','\n')
        methods = prior.members(p,t) if p.endswith('.vb') and any(v in p for v in ['Ui11PCA.vb','FactorAnalysis.vb','DiscriminantAnalysis.vb','WindowsMultivariateWorkflowAdapter.vb','MultivariateWorkflowPolicy.vb','MultivariateLegacyChoices.vb']) else {}
        inventory[p] = dict(sha256=sha(ROOT/p),bytes=len(b),methods={n:dict(sha256=hashlib.sha256(body.encode()).hexdigest(),first_line=t[:t.index(body)].count('\n')+1) for n,body in methods.items()})
    emit('prechange-method-inventory.json',dict(base=BASE,files=inventory))
    snap = RUN/'preimages'; snap.mkdir(exist_ok=False)
    for p in ['BESHstatNG/src/UI/Ui11PCA.vb','BESHStat.Excel.Common/src/Analysis/MultivariateWorkflowPolicy.vb','BESHStat.Excel.Common/src/Analysis/MultivariateLegacyChoices.vb']:
        (snap/Path(p).name).write_bytes((ROOT/p).read_bytes())
    frozen = [p for p in paths if p.startswith(('evidence/04A17/','evidence/04A18/','evidence/04A19/','evidence/04A14/','docs/architecture/04A02/'))]
    emit('frozen-capture-hashes.json',dict(interpretation='Entry bytes, never regenerated. Prior payload status and hashes retained; NOT_GOLDEN.',files={p:dict(sha256=sha(ROOT/p),bytes=(ROOT/p).stat().st_size) for p in frozen}))
    emit('entry-state.json',dict(base=BASE,main=git('rev-parse','main'),origin_main=git('rev-parse','origin/main'),branch=git('branch','--show-current'),index_empty=True,tracked_clean=True,ancestry=git('log','-4','--format=%H %s').splitlines(),merge_interpretation='Local main and origin/main include accepted 04A.19 commit and shipping source. Linear history; owner statement supplies acceptance, no separate merge commit invented.',worktrees=1,untracked=git('ls-files','--others','--exclude-standard').splitlines(),gitignore_sha256=sha(ROOT/'.gitignore'),ownership='Per-command safe.directory; no global config change. Authorized branch ref required sandbox escalation.',gates=dict(P='NO',N1='NO GO',N2='NO',C2='UNMET'),native_releases=0))
    emit('implementation-plan.json',dict(batch='04A.20',modified=['BESHstatNG/src/UI/Ui11PCA.vb','BESHstatNG/src/DM/WindowsMultivariateWorkflowAdapter.vb','BESHStatNG.Test/BESHStatNG.Tests.vbproj','BESHStatNG_Help_MkDocs/docs/methods/factor-analysis.md','BESHStatNG_Help_MkDocs/docs/methods/discriminant-analysis.md'],new=['Common FA/DA requests and projections','Multivariate20Tests.vb','Multivariate20AdapterTests.vb','04a20 Python verification/accounting contracts','evidence/04A20/*','docs/architecture/batch-04A-20-owner-review.md'],preserved='Core, prior tests/evidence, importers/RefEdit, native renderers/writers, other form methods, metadata, master plan, targets/packages',tolerance='Finite max(1e-9, abs(expected)*1e-9); exact cell types/identities/order, NaN and signed infinity classes. No relabelling.'))
    print('Entry snapshots frozen:',len(inventory),'source entries;',len(frozen),'frozen authorities')

if __name__ == '__main__':
    entry()
