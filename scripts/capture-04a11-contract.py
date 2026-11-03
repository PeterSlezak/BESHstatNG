"""04A.11 source/restriction/baseline and dormant native patch receipts."""
import difflib
import hashlib
import importlib.util
import json
from pathlib import Path
import re
import subprocess

ROOT=Path(__file__).resolve().parents[1]
OUT=ROOT/'evidence/04A11';RUN=ROOT/'TestResults/04A-11-20261103-run01'
BASE='463aaa12a6342cedfa084e266d5a985461de9d3a'
FORM='BESHstatNG/src/UI/UiGLM.vb'
CORE='BESHStat.Core/src/RegModels/GLM.vb'
PAGES=['generalized-linear-models-glm','multiple-linear-regression-lm','negative-binomial-regression-nb2','zero-inflated-poisson-regression','ordinal-logistic-regression','multinomial-logistic-regression']
CHANGED={FORM,CORE,'BESHstatNG/src/DM/WindowsRegressionRequestAdapter.vb','BESHStatNG.Test/BESHStatNG.Tests.vbproj','BESHStat.Excel.Common/src/Analysis/LinearBinomialRegressionPolicy.vb'}|{'BESHStatNG_Help_MkDocs/docs/methods/'+p+'.md' for p in PAGES}
spec=importlib.util.spec_from_file_location('contract09',ROOT/'scripts/capture-04a09-contract.py')
prior=importlib.util.module_from_spec(spec);spec.loader.exec_module(prior)

def sha(p):return hashlib.sha256(p.read_bytes()).hexdigest()
def digest(s):return hashlib.sha256(s.encode()).hexdigest()
def core_members(text):
    return {m[2]:m[0] for m in re.finditer(r'^    (?:(?:Public|Private|Friend|Protected|Shared|Overridable|Overrides|Overloads|Async)\s+)*(Sub|Function)\s+(\w+)\b.*?^    End \1',text,re.M|re.S)}
def git(*args):return subprocess.check_output(['git','-c','safe.directory='+ROOT.as_posix(),*args],cwd=ROOT).decode().strip()
def original(p):return subprocess.check_output(['git','-c','safe.directory='+ROOT.as_posix(),'show',BASE+':'+p],cwd=ROOT).decode('utf-8-sig').replace('\r\n','\n')
def emit(name,value):
    path=OUT/name;value=prior.sanitize(value)
    if path.exists():assert json.loads(path.read_bytes())==value,'Incremental evidence drift: '+name;return
    with path.open('x',encoding='utf-8',newline='\n') as f:json.dump(value,f,indent=2,ensure_ascii=False);f.write('\n')

def native_patch():
    source=(ROOT/FORM).read_text(encoding='utf-8-sig')
    fixed=source.replace('LinearBinomialRegressionPolicy.Project(request, ss, customGroups, codingNotes, LinearFitAlphaMode.NativeLegacyDefault)',
                         'LinearBinomialRegressionPolicy.Project(request, ss, customGroups, codingNotes, LinearFitAlphaMode.Requested)')
    anchor='        lm.Fit(intent.IncludeIntercept, intent.TermGroups, intent.SumOfSquares)'
    assert fixed.count(anchor)==1
    fixed=fixed.replace(anchor,'        lm.Alpha = intent.EffectiveFitAlpha\n'+anchor)
    anchor='        Dim dataHeaders = WindowsRegressionRequestAdapter.DeferredCovarianceDataHeaders(request, fitVarNames)'
    assert fixed.count(anchor)==2
    fixed=fixed.replace(anchor,'        Dim dataHeaders = DirectCast(fitVarNames.Clone(), String())')
    patch=''.join(difflib.unified_diff(source.splitlines(True),fixed.splitlines(True),fromfile='a/'+FORM,tofile='b/'+FORM))
    path=OUT/'C11-NATIVE-CORRECTIONS-01.final.UNAPPLIED.patch'
    if path.exists():assert path.read_text()==patch
    else:path.open('x',encoding='utf-8',newline='\n').write(patch)
    destination=RUN/'native-candidate-final';destination.mkdir(exist_ok=True)
    candidate=destination/'UiGLM.vb'
    raw=b'\xef\xbb\xbf'+fixed.encode('utf-8')
    if candidate.exists():assert candidate.read_bytes()==raw
    else:candidate.write_bytes(raw)
    result=subprocess.run(['git','-c','safe.directory='+ROOT.as_posix(),'apply','--check',str(path)],cwd=ROOT,capture_output=True,text=True)
    assert result.returncode==0,result.stderr
    emit('native-patch-final.json',dict(id='C11-NATIVE-CORRECTIONS-01',supersedes='native-patch.json: source pins before the failed-fit classification availability guard',issues=['C10-LM-ALPHA-01','C10-COV-NAMES-01'],status='UNAPPLIED_GATED',source_path=FORM,source_sha256=sha(ROOT/FORM),patch_sha256=sha(path),
        isolated_candidate_sha256=sha(candidate),apply_check_exit_code=0,scope='Assign requested LM alpha before fit; stop quarantining the GLM/NB2 Data header. Portable corrections active; these Windows output changes remain dormant.',
        strict_patch_applied=False,strict_patch_sha256=sha(ROOT/'evidence/04A09/strict-validation-activation.UNAPPLIED.patch'),
        next_owners=['04A.28 output labels/formatting','04A.40 UDF parity','04A.47 native GUI confirmation'],
        owner_decision='Approve exact-ID native timing deferral for these two shipping activations until original pre-change evidence/risk/N authority is met. Retain tested portable fixes and native compatibility in the meantime. No N/P release requested.'))

def source():
    initial=json.loads((RUN/'baseline.json').read_bytes())['source_pins']
    before=prior.members(original(FORM));after=prior.members((ROOT/FORM).read_text(encoding='utf-8-sig'))
    changed={n for n in before if before[n]!=after[n]}
    assert changed=={'RunOLS','RunGLM','RunGLMNB2','RunZIP','RunOrdLogit','RunMultiLogit'} and set(before)==set(after)
    tails={}
    for n in changed:
        old=before[n].split("        'Dump results",1)[1];new=after[n].split("        'Dump results",1)[1]
        new=new.replace('WriteRes.write(dataHeaders)','WriteRes.write(fitVarNames)')
        new=new.replace('If fitGlm.HasReportData AndAlso IsCurrentBinomialGlmFamily()', 'If IsCurrentBinomialGlmFamily()')
        assert old==new,n+' writer/chart tail changed'
        tails[n]=True
    protected={p:h for p,h in initial.items() if p not in CHANGED}
    assert all(sha(ROOT/p)==h for p,h in protected.items())
    assert set(git('diff','--name-only').splitlines())==CHANGED
    oldcore=core_members(original(CORE));newcore=core_members((ROOT/CORE).read_text(encoding='utf-8-sig'))
    assert {n for n in oldcore if oldcore[n]!=newcore[n]}=={'wrapResults','Fit'}
    fit=newcore['Fit']
    start=fit.index("        ' Reset report/fit-lifecycle")
    end=fit.index("        'Set defaults",start)
    stripped=fit[:start]+fit[end:]
    normalize=lambda t:'\n'.join(s.strip() for s in t.splitlines() if s.strip())
    assert normalize(stripped)==normalize(oldcore['Fit']),'Estimator statement drift'
    emit('source-delta-final.json',dict(base=BASE,supersedes=['source-delta.json: before four-argument CLR compatibility overload','source-delta-v2.json: before failed-fit classification availability guard'],changed_existing={p:dict(before=initial[p],after=sha(ROOT/p)) for p in sorted(CHANGED)},
        changed_members={n:dict(before=digest(before[n]),after=digest(after[n])) for n in sorted(changed)},
        unchanged_members={n:digest(before[n]) for n in before if n not in changed},writer_tails_equal_after_documented_substitutions=tails,
        substitutions=['Data-header values quarantined in Windows for pending covariance label activation','Classification skipped only when Core has no report data; completed fits, including nonconverged fits, retain original post-fit controls/chart order'],
        protected_source_pins=protected,protected_count=len(protected),
        Core_changes=dict(methods=['wrapResults','Fit'],properties=['HasReportData (new)','AllResiduals (unavailable guard)','PredictedResponses (unavailable guard)'],estimator_statements_unchanged=True,
                          fit_delta='Binomial report/fit-lifecycle cache reset only; no formula/optimizer statement change. Other Core methods unchanged.'),
        native_executed=False,strict_patch_applied=False,hash_scope='Whole-file hashes are raw SHA-256; member-block hashes use UTF-8 LF normalized text.'))
    parent=json.loads((ROOT/'evidence/04A10/restriction-crosswalk.json').read_bytes())
    emit('restriction-crosswalk.json',dict(parent_path='evidence/04A10/restriction-crosswalk.json',parent_sha256=sha(ROOT/'evidence/04A10/restriction-crosswalk.json'),
        controls=parent['controls'],associated_controls=34,associated_obligations=7,additional_controls=[],released=0,discharged=0,
        P='NO',N1='NO GO',N2='NO',C2='UNMET',original_obligations=1514,original_restricted_controls=802,
        retained_overlapping_populations=['239/218 transferred','160/141 ANOVA/normality','44 importer obligations','3 2x2 obligations'],
        gate='C09-NATIVE-STRICT-01 remains deferred; C11-NATIVE-CORRECTIONS-01 is the single pending native timing disposition, not a new original obligation.'))
    files={p.relative_to(ROOT).as_posix():dict(sha256=sha(p),bytes=p.stat().st_size) for p in sorted((OUT/'baselines').glob('*.txt'))}
    assert len(files)==18
    emit('baseline-registry.json',dict(base=BASE,status='NOT_GOLDEN',native=False,files=files,
        capture_source_sha256=sha(RUN/'preimages/Regression11Baselines-capture.vb'),current_capture_source_sha256=sha(ROOT/'BESHStat.Excel.Common.Tests/src/Regression11Baselines.vb'),
        prechange_net10=dict(command=json.loads((RUN/'prechange-net10-02-command.json').read_bytes()),passed=9,total=9),
        prechange_net48=dict(build=json.loads((RUN/'prechange-net48-host-command.json').read_bytes()),passed=9,total=9),
        meaning='Eight real-Core four-family table/vector captures plus one executable defect-observation capture per runtime, before production edits. Defect observation code retained; original accepted 04A.10 failure expectation remains frozen.',
        tolerances='Double max(1e-9,abs(expected)*1e-9); strings/types/axes/table order/metadata exact within runtime; only elapsed-time text masked; LF output explicit.',
        original_evidence_preserved=True))
    native_patch()

if __name__=='__main__':source()
