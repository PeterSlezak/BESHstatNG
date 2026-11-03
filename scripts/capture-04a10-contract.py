"""04A.10 source, baseline and original-restriction successor evidence; no native work."""
import hashlib
import importlib.util
import json
from pathlib import Path
import re
import subprocess

ROOT=Path(__file__).resolve().parents[1]
OUT=ROOT/'evidence/04A10'
RUN=ROOT/'TestResults/04A-10-20261103-run01'
BASE='49fcc30a6b39f6a2bebcc9353ce2dea0d3e13055'
FORM='BESHstatNG/src/UI/UiGLM.vb'
CHANGED={FORM,'BESHstatNG/src/DM/WindowsRegressionRequestAdapter.vb','BESHStatNG.Test/BESHStatNG.Tests.vbproj'}
spec=importlib.util.spec_from_file_location('contract09',ROOT/'scripts/capture-04a09-contract.py')
prior=importlib.util.module_from_spec(spec);spec.loader.exec_module(prior)

def sha(path):return hashlib.sha256(path.read_bytes()).hexdigest()
def digest(text):return hashlib.sha256(text.encode()).hexdigest()
def git(*args):return subprocess.check_output(['git','-c','safe.directory='+ROOT.as_posix(),*args],cwd=ROOT).decode().strip()
def original(path):return subprocess.check_output(['git','-c','safe.directory='+ROOT.as_posix(),'show',BASE+':'+path],cwd=ROOT).decode('utf-8-sig').replace('\r\n','\n')
def emit(name,value):
    value=prior.sanitize(value)
    path=OUT/name
    if path.exists():
        assert json.loads(path.read_bytes())==value,'Incremental evidence drift: '+name
        return
    with path.open('x',encoding='utf-8',newline='\n') as f:json.dump(value,f,indent=2,ensure_ascii=False);f.write('\n')

def baseline_bytes():
    receipt=json.loads((ROOT/'evidence/04A09/runtime-baselines.json').read_bytes())
    initial=json.loads((RUN/'baseline.json').read_bytes())['source_pins']
    rows=[]
    for name,row in receipt['files'].items():
        path='evidence/04A09/baselines/'+name
        raw=(ROOT/path).read_bytes()
        lf=raw.replace(b'\r\n',b'\n')
        hashed=lambda b:hashlib.sha256(b).hexdigest()
        blob=subprocess.check_output(['git','-c','safe.directory='+ROOT.as_posix(),'show',BASE+':'+path],cwd=ROOT)
        rows.append(dict(path=path,accepted_receipt=row['sha256'],entry=initial[path],current=hashed(raw),lf=hashed(lf),crlf=hashed(lf.replace(b'\n',b'\r\n')),main_blob=hashed(blob)))
    emit('baseline-byte-provenance.json',dict(base=BASE,all_entry_bytes_preserved=all(r['entry']==r['current'] for r in rows),rows=rows,
        scope='Raw SHA-256; accepted receipt is frozen. No baseline or expectation edited. LF/CRLF hashes diagnose checkout text conversion only.'))

def source():
    baseline_bytes()
    initial=json.loads((RUN/'baseline.json').read_bytes())['source_pins']
    before=prior.members(original(FORM));after=prior.members((ROOT/FORM).read_text(encoding='utf-8-sig'))
    changed={n for n in before if before[n]!=after[n]}
    assert changed=={'RunOLS','RunGLM'} and set(before)==set(after)
    tails={}
    for n in changed:
        old=before[n].split("        'Dump results",1)[1]
        new=after[n].split("        'Dump results",1)[1]
        if n=='RunGLM':
            new=new.replace('            Dim classificationPlan = New LinearBinomialClassificationPlan(request, classificationOptions, classification)\n            Dim clsRes = classificationPlan.LegacyTables()',
                            '            Dim clsRes = classification.LegacyTables()')
        assert old==new,n+' native tail changed'
        tails[n]=dict(equal=True,exception='GLM classification table list now rehydrated from the post-fit immutable document; all control reads/charts unchanged' if n=='RunGLM' else None)
    protected={p:h for p,h in initial.items() if p not in CHANGED}
    assert all(sha(ROOT/p)==h for p,h in protected.items())
    assert set(git('diff','--name-only').splitlines())==CHANGED
    emit('source-delta.json',dict(base=BASE,changed_existing={p:dict(before=initial[p],after=sha(ROOT/p)) for p in sorted(CHANGED)},
        changed_members={n:dict(before=digest(before[n]),after=digest(after[n])) for n in sorted(changed)},
        unchanged_members={n:digest(before[n]) for n in before if n not in changed},native_writer_tails=tails,
        protected_count=len(protected),protected_source_pins=protected,Core_unchanged=True,UDF_exports_unchanged=True,
        native_executed=False,strict_patch_applied=False,strict_patch_sha256=sha(ROOT/'evidence/04A09/strict-validation-activation.UNAPPLIED.patch')))
    parent=json.loads((ROOT/'evidence/04A09/restriction-crosswalk-v2.json').read_bytes())
    # Source references remain inside the original 34-control union, including Type I/III.
    controls={r['control_id'].split('.')[-1] for r in parent['controls']}
    scoped_refs=set(re.findall(r'Me\.(\w+)',before['RunOLS']+before['RunGLM']+after['RunOLS']+after['RunGLM']))
    with (ROOT/'docs/architecture/WindowsMigrationLedger/ui-controls.csv').open(encoding='utf-8-sig') as f:
        rows=list(__import__('csv').DictReader(f))
    actual={r['control_id'].split('.')[-1] for r in rows if '.UiGLM.' in r['control_id'] and r['control_id'].split('.')[-1] in scoped_refs}
    assert actual <= controls,actual-controls
    emit('restriction-crosswalk.json',dict(parent_path='evidence/04A09/restriction-crosswalk-v2.json',parent_sha256=sha(ROOT/'evidence/04A09/restriction-crosswalk-v2.json'),
        controls=parent['controls'],associated_controls=34,associated_obligations=7,additional_controls=[],released=0,discharged=0,
        affected_direct_controls=sorted(actual),scope='Retained original union, not a new native exercise claim. Exact original-row hashes and IDs carried forward unchanged.',
        P='NO',N1='NO GO',N2='NO',C2='UNMET',original_universe_obligations=1514,original_restricted_controls=802,
        strict_gate=dict(id='C09-NATIVE-STRICT-01',status='Owner-approved deferral retained; no new diagnostic refusal',patch_applied=False)))
    files={p.relative_to(ROOT).as_posix():dict(sha256=sha(p),bytes=p.stat().st_size) for p in sorted((OUT/'baselines').glob('*.txt'))}
    assert len(files)==12
    emit('baseline-registry.json',dict(base=BASE,status='NOT_GOLDEN',native=False,
        capture_source_sha256=sha(RUN/'preimages/Regression10Baselines-capture.vb'),current_source_sha256=sha(ROOT/'BESHStat.Excel.Common.Tests/src/Regression10Baselines.vb'),
        harness_delta='After capture only: clone names before GLM.setVarNames, because Core covariance wrap mutates the response name to Intercept. All captured tables/vectors remain unchanged.',
        prechange_net10=dict(command=json.loads((RUN/'prechange-net10-command.json').read_bytes()),passed=6,total=6),
        prechange_net48=dict(host=json.loads((RUN/'prechange-net48-host-command.json').read_bytes()),tests=json.loads((RUN/'prechange-net48-tests-command.json').read_bytes()),passed=6,total=6),
        cases={'LM-ordinary':'OLS/intercept/Type III/covariance/residuals/coding notes', 'LM-alternate':'WLS/no intercept/Type I/covariance/residuals/coding notes',
               'Gaussian-ordinary':'Gaussian identity; offset/weights/covariance/residuals/iterations', 'Gaussian-alternate':'Gaussian log; same roles/sections',
               'Binomial-ordinary':'Binomial logit; same roles/sections; Core nonconvergence retained','Binomial-alternate':'Binomial probit; same roles/sections; Core nonconvergence retained'},
        rows='7+3i identities reversed, data order unchanged',files=files,
        inherited_04A09_baselines={p.relative_to(ROOT).as_posix():sha(p) for p in sorted((ROOT/'evidence/04A09/baselines').glob('*.txt'))},
        inherited_GLM='ordinary=Poisson; alternate=binomial. Neither is relabelled Gaussian.',
        tolerance='Double cells max(1e-9,abs(expected)*1e-9); NaN explicit; types/strings/table order/header metadata/p-value annotations exact within runtime; only elapsed wall-clock strings masked.',
        culture='Invariant and sk-SK; cross-culture numeric cells compared to frozen captures; culture-dependent Core strings compared exactly to same-culture projection.',
        references='Existing NIST 11-dataset suite and committed GLM all-link CSV/reference tests retain their original tolerances and run in full regressions. No new external reference or independent scientific review.'))

if __name__=='__main__':source()
