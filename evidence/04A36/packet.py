"""Seal public36 facts/manifest; independent reader is verify_packet.py."""
from work import *
from summarize import load,receipts
from verify_packet import SOURCES,SELF,scope,EXCLUDED,validate_public_text
import xml.etree.ElementTree as ET
from collections import Counter
def facts():
    ast=load(HERE/'final-members.json');current={n['key']:n for n in ast['current']}
    old_tokens=Counter((n['key'],n['token_sha256']) for n in ast['supporting_entry'])
    live_tokens=Counter((n['key'],n['token_sha256']) for n in ast['current'])
    assert not old_tokens-live_tokens,'Accepted Common member token multiset changed'
    contract=load(HERE/'coordinate-and-chart-spec-contract.json')
    extra=next(n for n in ast['current'] if n['name']=='ResidualPlotAvailability')
    contract['public_members'].append(dict(path=extra['path'],owner=extra['owner'],signature=extra['signature'],lines=[extra['start'],extra['end']],token_sha256=extra['token_sha256']))
    contract['public_members']=list({(r['path'],r['owner'],r['signature']):r for r in contract['public_members']}.values())
    contract['accepted_survival_report_member_tokens_preserved']=len(ast['supporting_entry']);save('coordinate-and-chart-spec-contract.json',contract)
    baseline={r['path']:r for r in load(HERE/'entry-tracked-pins.json')['files']}
    ui=['BESHstatNG/src/UI/'+n+'.vb' for n in ['Ui4Cox','Ui4Cox.Designer','Ui4KMandLogRank','Ui21ControlCharts','Ui22MultivariateControlCharts']]
    for p in ui:assert pin(p)['sha256']==baseline[p]['sha256']
    save('source-policy.json',dict(status='PASS',public_windows_headers='AST exact, except explicitly private native-buffer constructor/zone argument transitions',unchanged_ui=[pin(p) for p in ui],cox_residual_control='DISABLED_UNWIRED_UNCHANGED',core_common_target='netstandard2.0',windows_target='net48',new_portable_option_strict=True,no_new_host_dependencies=True,old_statistical_engines_unchanged=True,compiled_shipping_delegation='Graphics36AdapterTests.ShippingILConsumesKMAndCoxAtOriginalPhases / ActualSharedSpcPreparationAndBlankMarshalling; real compiled callers, no Excel execution'))
    project=ROOT/'BESHStatNG.Test/BESHStatNG.Tests.vbproj';includes=[n.attrib.get('Include','') for n in ET.parse(project).getroot().iter() if n.tag.split('}')[-1]=='Compile']
    expected=['Cases36.vb','Legacy36.vb','Graphics36GeometryTests.vb','Graphics36PresentationTests.vb','LegacyPresentation36.vb','LegacyPresentation36V2.vb','Graphics36AdapterTests.vb']
    assert all(sum(p.endswith(n) for p in includes)==1 for n in expected)
    save('project-includes.json',dict(status='PASS',framework_explicit_sources=expected,core_common_implicit_sdk_sources=True,both_configurations_built=True,targets_references_packages_unchanged=True))
    save('deferred-issues.json',dict(gates=dict(P='NO',N1='NO GO',N2='NO',C2='UNMET',C3='UNAPPROVED'),scientific='NOT_GOLDEN',original_controls_released=0,issues=[dict(id='C13-LOGRANK-R-01',status='OPEN_UNCHANGED',reason='Earliest-time censor/weighted log-rank restriction; no estimator/risk-set changes or R execution.',owner='04A.39-41; native45-48'),dict(id='C13-KM-CENSOR-AXIS-01',status='OPEN_UNCHANGED',reason='No-censor groups may be omitted in count dimension; original source-only drawing phase/index failures and upstream all-event Max failure retained.',owner='04A.39-41; native45-48'),dict(id='C13-RESIDUAL-PLOTS-01',status='OPEN_UNCHANGED',reason='Payload metadata does not activate disabled/unwired Cox residual GUI or native renderer.',owner='04A.45-48 native approval'),dict(id='C36-KM-CI-TIME-PADDING-01',status='CANDIDATE_ONLY',input='Cases36.KM(19), append extra SurvivalTime slot; CI X length6/Y length5 with original trailing zero',source='KaplanMeierPlotExcel original confidence loops -> KaplanMeierSeriesGeometry36.Confidence',risk='Native cardinality/coercion not independently observed; behavior preserved.',owner='04A.39-41 /45-48'),dict(id='C36-KM-EMPTY-CENSOR-SLOT-01',status='CANDIDATE_ONLY',input='CensoredCountByGroup(group)=0; native NewSeries still occurs without values/name assignment',source='KaplanMeierPlotExcel.Plot censor phase',risk='Native empty-series/legend behavior remains SOURCE_ONLY_NATIVE; no correction.',owner='04A.39-41 /45-48'),dict(id='C36-COX-LATE-ARRAY-AXIS-01',status='CANDIDATE_ONLY',input='Axis from times{1,2}; hypothetical native reentry mutates times before original XValues read',source='CoxSurvivalPlotExcel.PlotCox',risk='Original earlier scale/later vector timing retained; no native reentry executed.',owner='04A.39-41 /45-48'),dict(id='SPC_ORIGINAL_CONTROL_OBLIGATIONS',status='OPEN_UNCHANGED',reason='04A.21-24 phase/exclusion/model/native controls and stacked row-count candidate remain; no signals/limits/rules changed.',owner='04A.39-41 /45-48'),dict(id='G29_CANONICAL_HELPERS_171_172',status='UNCERTAINTY_RETAINED',reason='Canonical v2 lineage is unchanged; this batch discharges no historical cross-source uncertainty.',owner='Existing later-batch charter')]))
    save('tooling-corrections.json',dict(attempts=[dict(issue='Initial source reader had no SDK assets file',disposition='Offline SDK-only restore against local empty feed; no package/network install; subsequent build succeeds.'),dict(issue='Initial new test compilation: imported module names/loop inference, rectangular array list initializer and direct embedded interop call',disposition='New tests corrected with explicit locals/list/reflection; old tests untouched; all83 cases pass both configs.'),dict(issue='Private HasValidZones removed initially',disposition='Original reflective adapter test seam restored as a thin Core delegate; original8 tests pass.'),dict(issue='SPC V1 only6 valid outputs and378 construction rejections',disposition='All source/capture bytes retained; explicitV2 correction using original entry algorithms/assemblies,330 valid outputs+54 rejected per384 rows.'),dict(issue='Final focused36 Release summary filename collided with Debug summary after all3 test commands passed',disposition='Private original summary retained; all Core13/Common27/Framework43 Release cases and command receipts exist with exit0. Helper summary naming fixed to include config; no test rerun/deadline change needed.'),dict(issue='First development wrapper did not propagate build exit_codes',disposition='Actual underlying failed build command receipts remain exit1; local run.py now propagates nonzero build code. No green build inferred from outer wrapper.'),dict(issue='Shell quoting in local helper edits failed',disposition='No production edits resulted; helper changes applied with exact patches and reader rebuilt. No source expectation changed.')]))
    save('readiness.json',dict(status='NOT_READY_04A36',software_scope='IMPLEMENTED_VALIDATED_ZERO_IN_SCOPE_OPEN',in_scope_open=0,blockers=[dict(id='EXTERNAL_DOCUMENTATION_ATTRIBUTION',paths=[r['path'] for r in load(HERE/'external-changes.json')['files']],reason='Concurrent Master Plan and project README updates are preserved outside36. Owner clarification remains pending.'),dict(id='EXTERNAL_MASTER_PLAN_DIFF_CHECK',path='planning/BESHStatNG_CrossPlatform_MasterPlan.md:3',reason='Full git diff --check exits2 on original two-space Markdown hard break in concurrent Revision2.13. Batch-only check exits0. No authorization to amend owner plan; preserve bytes.')],gates=dict(P='NO',N1='NO GO',N2='NO',C2='UNMET',C3='UNAPPROVED')))
    focused=[]
    for config in ['Debug','Release']:
        for suite in ['Core','Common','Legacy']:
            p=ROOT/('.test-rationalization-04A36.tmp/executions/final-'+config+'-new36-'+suite+'/cases.json');r=load(p);assert r['command']['exit_code']==0
            focused.append(dict(configuration=config,suite=suite,outcomes=r['outcomes'],trx_sha256=r['sha256'],exit_code=0))
    save('focused-validation.json',dict(status='PASS',runs=focused))
    receipts();print('Prepared factual scope, control, fixture, tooling and honest readiness receipts')
def seal():
    paths=scope();paths.discard(SELF);missing=[p for p in SOURCES if not (ROOT/p).is_file()];assert not missing,missing
    scanned=[];findings=[]
    for p in sorted(paths):
        if p.endswith('privacy-check.json'):continue
        b=(ROOT/p).read_bytes()
        try:validate_public_text(b.decode('utf-8-sig'))
        except (AssertionError,UnicodeDecodeError):findings.append(p)
        scanned.append(dict(path=p,bytes=len(b),sha256=sha(b)))
    save('privacy-check.json',dict(status='PASS' if not findings else 'FAIL',files_checked=len(scanned),findings=findings,qualification='Every proposed public text artifact scanned for private home paths/SIDs/credential/private-key markers. Synthetic data only; raw logs/binaries/caches/private payloads excluded. Manifest self is canonical relative-path metadata.'))
    assert not findings,findings
    save('publication-manifest.json',dict(decision=load(HERE/'readiness.json')['status'],scope='Official36 proposal only; no actual publication or native/scientific approval',files=[pin(p) for p in sorted(scope()) if p!=SELF]+[dict(path=SELF,bytes=None,sha256=None)]))
    print('Sealed',len(scope())-1,'external artifacts plus null self; privacy PASS')
if __name__=='__main__':
    if len(sys.argv)>1 and sys.argv[1]=='seal':seal()
    else:facts()
