"""Read-only TRX/log comparison and compact 04A.08 successor publication evidence."""
from collections import Counter
import hashlib
import importlib.util
import json
from pathlib import Path
import re
import shutil
import subprocess
import xml.etree.ElementTree as ET

ROOT=Path(__file__).resolve().parents[1]
RUN=ROOT/'TestResults/04A-08-20261103-run01'
OUT=ROOT/'docs/architecture/04A08'
NS='{http://microsoft.com/schemas/VisualStudio/TeamTest/2010}'
OLD=json.loads((ROOT/'docs/architecture/04A07/validation-v2.json').read_bytes())
BASELINE=json.loads((RUN/'baseline.json').read_bytes())

def sha(path):return hashlib.sha256(path.read_bytes()).hexdigest()
def emit(path,value):
    with path.open('x',encoding='utf-8',newline='\n') as f:json.dump(value,f,indent=2,ensure_ascii=False);f.write('\n')
def git(*args):return subprocess.check_output(['git','-c','safe.directory='+ROOT.as_posix(),*args],cwd=ROOT).decode().strip()

def trx(path):
    tree=ET.parse(path)
    methods={n.attrib['id']:n.find(NS+'TestMethod').attrib for n in tree.findall('.//'+NS+'UnitTest')}
    results=[]
    for row in tree.findall('.//'+NS+'UnitTestResult'):
        m=methods[row.attrib['testId']]
        results.append(dict(identity=m['className']+'.'+m['name'],test_name=row.attrib['testName'],outcome=row.attrib['outcome']))
    return dict(path=path.relative_to(ROOT).as_posix(),sha256=sha(path),counters=tree.find('.//'+NS+'Counters').attrib,
        failures=[r for r in results if r['outcome']!='Passed'],
        batch08_tests=[r for r in results if 'RegressionEffectAuthoring08' in r['identity'] or 'RegressionEffects08' in r['identity']])

def summary():
    prior_pins=json.loads((ROOT/'TestResults/04A-07-20261102-run01/baseline.json').read_bytes())['source_pins']
    # Four C07 checks and one C06 check fail on the already-committed Revision 2.5 plan.
    # All C07 protected paths that existed at entry are compared, not just the plan.
    protected=[p for p in prior_pins if p.startswith(('BESHStat.Core/','planning/','docs/','BESHStat.OfficeAddin/','BESHStat.WasmSmoke/')) or '/ExcelUDFs/' in p or p.endswith('.Designer.vb') or '/UI/Refedit/' in p or p.startswith('BESHStat.Excel.Common/src/') or p=='BESHstatNG/src/DM/ExcelDnaDataImporter.vb' or p=='BESHstatNG/src/DM/WindowsAssumptionReportAdapter.vb' or p.startswith('scripts/tests/')]
    entry_drift={p:dict(prior_sha256=prior_pins[p],entry_sha256=BASELINE['source_pins'].get(p),current_sha256=sha(ROOT/p)) for p in protected if BASELINE['source_pins'].get(p)!=prior_pins[p]}
    assert set(entry_drift)=={'planning/BESHStatNG_CrossPlatform_MasterPlan.md'},entry_drift
    assert all(d['entry_sha256']==d['current_sha256'] for d in entry_drift.values())
    expected_extra={'test_04a07_workflow_successor.Anova07SuccessorTests.'+n for n in [
        'test_core_old_tests_importer_udfs_designers_and_help_unchanged','test_exact_source_and_original_ledger_rows',
        'test_no_fabricated_obligation_for_previously_source_qualified_controls','test_restriction_unions_and_timing_exceptions_not_generalized']}
    expected_extra.add('test_04a06_assumptions_successor.Assumptions06SuccessorTests.test_importer_udf_designers_core_and_old_tests_unchanged')
    configs={}
    for config in ['Debug','Release']:
        name='final-fixed-'+config
        managed_name='final-managed-'+config
        paths=[RUN/managed_name/'results'/(s+'.trx') for s in ['Core','OfficeAddin','OfficeExecution']]
        paths += [RUN/'executions'/('final-authoring-'+config+'-'+s)/'results'/(s+'.trx') for s in ['Common','Legacy']]
        paths += [RUN/'projection-executions'/managed_name/'results/ControlMitigation.trx']
        suites=[trx(p) for p in paths]
        failures=[r for s in suites for r in s['failures']]
        key=lambda r:(r['identity'],r['test_name'],r['outcome'])
        assert Counter(map(key,failures))==Counter(map(key,OLD['configurations'][config]['managed_failures']))
        logs=[(RUN/(name+s+'.log')).read_text(encoding='utf-8',errors='replace') for s in ['-full','-common-python']]
        events=[];total=0;skips=0
        for log in logs:
            total+=sum(map(int,re.findall(r'^Ran (\d+) tests? in',log,re.M)))
            skips+=sum(map(int,re.findall(r'^(?:OK|FAILED) \([^\n]*skipped=(\d+)',log,re.M)))
            for outcome,identity,context in re.findall(r'^(FAIL|ERROR): [^\n]+? \((test_[^()]+)\)([^\n]*)$',log,re.M):
                events.append(dict(identity=identity,outcome=outcome,subtest=context.strip() or None))
        nonpasses={r['identity']:r['outcome'] for r in events}
        earlier=OLD['configurations'][config]['python_nonpasses']
        assert set(nonpasses)-set(earlier)==expected_extra
        assert not set(earlier)-set(nonpasses)
        assert all(nonpasses[k]==v for k,v in earlier.items())
        node_pass=sum(map(int,re.findall(r'\bpass (\d+)\s*$',logs[0],re.M)))
        node_fail=sum(map(int,re.findall(r'\bfail (\d+)\s*$',logs[0],re.M)))
        assert (node_pass,node_fail)==(916,0)
        new=[r for s in suites for r in s['batch08_tests']]
        assert len(new)==66 and all(r['outcome']=='Passed' for r in new)
        configs[config]=dict(managed_pass=sum(int(s['counters']['passed']) for s in suites),managed_total=sum(int(s['counters']['total']) for s in suites),
            managed_fail=len(failures),managed_skip=sum(int(s['counters']['notExecuted']) for s in suites),managed_suites=suites,managed_failures=failures,
            batch08_managed_pass=len(new),batch08_managed_fail=0,batch08_managed_skip=0,
            node_pass=node_pass,node_fail=node_fail,python_total=total,python_pass=total-len(nonpasses)-skips,python_nonpassing_methods=len(nonpasses),python_skip=skips,
            python_nonpasses=nonpasses,python_events=events,python_failure_events=len(events),
            inherited_from_reported04A07=earlier,inherited_entry_plan_conflicts=sorted(expected_extra),
            batch08_python_pass=8,new_functional_nonpasses=0)
    focused={n:trx(RUN/p) for n,p in {
        'Core':'core-focused/CoreFocused.trx','Common':'executions/focused-canonical/results/Common.trx',
        'Windows':'executions/focused-canonical-windows/results/Legacy.trx'}.items()}
    return dict(schema='04A08-validation-v2',base=BASELINE['head'],branch=git('branch','--show-current'),configurations=configs,focused=focused,
        entry_source_pin_conflict_proof=entry_drift,reported_prior_python_nonpasses=44,actual_entry_python_nonpasses=49,
        inferred_entry_python_counts=dict(pass_count=618,total=667,nonpass=49,
            basis='Reported 667-method corpus, all original 44 nonpass identities recur, plus five reproduced C06/C07 failures whose triggering plan drift was already present on main; no pre-edit whole-suite run claimed.'),
        initial_debug='final-Debug retained, superseded: Common was built before warning-boundary snapshot repair and five additional focused cases. Final-fixed runs repaired warning snapshots; their managed stages were subsequently superseded by final-managed.',
        final_managed_refresh='Final-managed Debug/Release supersede managed stages of final-fixed full runs after cross-runtime double-bit serialization and exact normalized-reference checks; Node/Python sources and full results remain applicable. No numerical methods changed.',
        final_authoring_refresh='Final-authoring Debug/Release supersede Common/Windows stages after LF-only string identity was added to the existing serialization edge test; all other source and executed suites remain applicable.',
        commands=[json.loads(p.read_bytes()) for p in sorted(RUN.glob('*-command.json'))],native='NOT RUN',P='NO',N1='NO GO',N2='NO',C2='UNMET')

def closeout():
    changed=git('diff','--name-only').splitlines()
    normal=git('ls-files','--others','--exclude-standard').splitlines()
    files=sorted(set(changed+normal+[p.relative_to(ROOT).as_posix() for p in OUT.glob('*.json')]))
    spec=importlib.util.spec_from_file_location('c08',ROOT/'scripts/capture-04a08-contract.py');c=importlib.util.module_from_spec(spec);spec.loader.exec_module(c)
    def vb_members(text):
        def qualified(position,name):
            types=list(re.finditer(r'^\s*(?:(?:Public|Private|Friend|Protected|Partial|NotInheritable|MustInherit)\s+)*(?:Class|Module)\s+(\w+)\b',text[:position],re.M))
            return (types[-1][1]+'.' if types else '')+name
        result={qualified(m.start(),m[3]):m[0] for m in re.finditer(r'^([ \t]+)(?:(?:Public|Private|Friend|Protected|Shared|Async|Overrides|Overloads)\s+)*(Sub|Function)\s+(\w+)\b.*?^\1End \2',text,re.M|re.S)}
        for m in re.finditer(r'^([ \t]+)(?:(?:Public|Private|Friend|ReadOnly|WriteOnly)\s+)*Property\s+(\w+)\b[^\n]*\n(?=[ \t]+(?:Get|Set)\b).*?^\1End Property',text,re.M|re.S):result[qualified(m.start(),m[2])]=m[0]
        return result
    manifest=[]
    for p in files:
        if not (ROOT/p).is_file():continue
        before=BASELINE['source_pins'].get(p)
        entry=dict(path=p,before_sha256=before,after_sha256=sha(ROOT/p),ignored=subprocess.run(['git','-c','safe.directory='+ROOT.as_posix(),'check-ignore','-q',p],cwd=ROOT).returncode==0,
            publication='local-only retained baseline/superseded evidence' if p in ['docs/architecture/04A08/baseline.json','docs/architecture/04A08/validation.json','docs/architecture/04A08/publication-inventory.json'] else 'owner publication candidate; not staged')
        if p.endswith('.vb'):
            old=vb_members(c.original(p)) if before else {}
            new=vb_members((ROOT/p).read_text(encoding='utf-8-sig'))
            entry['members']={n:dict(before_sha256=hashlib.sha256(old[n].encode()).hexdigest() if n in old else None,
                after_sha256=hashlib.sha256(new[n].encode()).hexdigest() if n in new else None) for n in sorted(set(old)|set(new))}
            data=(ROOT/p).read_bytes();entry['line_endings']=dict(crlf=data.count(b'\r\n'),lf=data.count(b'\n'))
        manifest.append(entry)
    emit(OUT/'publication-inventory-v2.json',dict(schema='04A08-publication-v2',base=BASELINE['head'],branch=git('branch','--show-current'),files=manifest,
        member_hash_convention='Type-qualified complete Sub/Function/explicit Property members, UTF-8 with normalized LF; exact file SHA includes physical bytes. Supersedes v1 member names, which did not qualify constructors by class.',
        local_only=['TestResults/04A-08-20261103-run01/','docs/architecture/04A08/baseline.json','docs/architecture/04A08/validation.json','docs/architecture/04A08/publication-inventory.json'],
        inventory_self_digest_not_embedded=True,disk_free_bytes=shutil.disk_usage(ROOT).free,
        index_empty=not git('diff','--cached','--name-only'),no_commit_push_merge=True))

def review():
    validation=json.loads((OUT/'validation-v2.json').read_bytes())
    restriction=json.loads((OUT/'restriction-crosswalk.json').read_bytes())
    rows=[]
    for name,op in [('AddMainEffectsFromSelectedVars','ContinuousMain'),('AddCategoricalEffectsFromSelectedVars','CategoricalMain'),('AddPolynomialEffectsFromSelectedVars','Polynomial'),('AddTwoWayInteractionsFromSelectedVars','PairwiseInteractions'),('AddCustomInteractionFromSelectedVars','CustomInteraction')]:
        rows.append(dict(source='BESHstatNG/src/UI/RegressionEffectsController.vb',member=name,owner='RegressionEffectAuthoringPolicy.Apply',operation=op,
            windows_retains='Selection/index snapshot, projection, original warning presentation',core_retains='Effect identity, coefficient labels, TermSpec and PredictorScale'))
    rows.append(dict(source='BESHstatNG/src/DM/UIprocedures.vb',member='Remove_Item',owner='RegressionEffectAuthoringPolicy.Remove',
        windows_retains='Untyped variable-list path and existing early-empty return; typed list projection/messages',compatibility='Exact original text-based cascade and order refresh'))
    protected=['BESHStat.Core/src/RegModels/RegressionDesignCore.vb','BESHStat.Core/src/RegModels/RegressionFormulaParser.vb','BESHStat.Core/src/RegModels/RegressionFormulaDesignService.vb',
        'BESHStat.Excel.Common/src/Input/ModelInput.vb','BESHStat.Excel.Common/src/Input/ModelInputCompiler.vb','BESHstatNG/src/DM/ExcelDnaDataImporter.vb',
        'BESHStat.Excel.Common/src/Analysis/AnovaWorkflowPolicy.vb','BESHStat.Excel.Common/src/Input/ContingencyInputPolicy.vb',
        'BESHStat.Excel.Common/src/Results/ContingencyReportPolicy.vb','BESHStat.Excel.Common/src/Results/ProportionsReportPolicy.vb','BESHStat.Excel.Common/src/Results/AssumptionReportPolicy.vb',
        'planning/BESHStatNG_CrossPlatform_MasterPlan.md','BESHstatNG/Generated/VersionAttributes.vb']
    assert all(sha(ROOT/p)==BASELINE['source_pins'][p] for p in protected)
    emit(OUT/'ownership-matrix.json',dict(schema='04A08-ownership-v1',base=BASELINE['head'],members=rows,
        consumers=[dict(path=r['path'],source_sha256=r['source_sha256'],unchanged=r['unchanged'],controllers=r['controllers'],
            handlers=[dict(member=h['member'],body_sha256=h['body_sha256'],edges=h['edges']) for h in r['handlers']]) for r in restriction['ownership']],
        protected_source_pins={p:sha(ROOT/p) for p in protected},
        common_contract='Ordered read-only effects and defensive Core TermSpec projections; ordinal identity; optional diagnostic snapshots; no host or IO dependency',
        host_evidence='Standalone managed ListBoxes and shipping controller, STA; source-anchored event routes. Actual form constructors and native Excel UI NOT RUN.'))
    formula=[
        ('continuous/categorical/polynomial','SUPPORTED','Core helpers + parser; relative tokens bind to supplied catalog; no alternate grammar','SingleTermFormulaAndCoreDesignRoundTripQuotedUnicodeNames'),
        ('continuous x categorical','SUPPORTED when Core preserves authored order','Same matrix/columns; separate intercept option','FormulaParserAndTypedCompilerUseSeparateIntercept'),
        ('categorical x categorical / 3-way','SUPPORTED when Core preserves authored order','Existing Core coding, no inferred contrast','CategoricalCrossesAndThreeWayPreserveCoreMatrixAndOrder'),
        ('explicit ref=2','SUPPORTED','Reference is metadata, not a level-inference algorithm','ExplicitReferenceCodingAndDuplicateCoefficientLabelsKeepRawIdentity'),
        ('reordered interaction','NOT REPRESENTABLE when parser reorders','Typed source order retained; no silently changed effect key','FormulaNormalizationRefusesChangedInteractionOrderAndEmptyFallback'),
        ('empty / intercept-only','TYPED ONLY','Blank Core RHS means all default main effects; no 1/0/LHS grammar invented','FormulaNormalizationRefusesChangedInteractionOrderAndEmptyFallback'),
        ('polynomial + interaction sharing a base','NOT REPRESENTABLE (conservative)','C08-CORE-LABEL-01: existing label map differs; numeric matrix equal in reproducer','ExistingCorePolynomialInteractionLabelDifferenceIsNotSilentlyNormalized'),
        ('high-precision factor reference normalized on Framework','NOT REPRESENTABLE if normalized text loses bits','C08-FORMULA-REF-01; Core G formatting unchanged','CanonicalDoubleMetadataMatchesPortableRuntimeContract'),
        ('observed levels / custom display metadata','TYPED ONLY','No inference or silent metadata loss','ReferenceLevelsDisplayMetadataAndSnapshotsAreDeepCopies'),
        ('whitespace / XML-ineligible strings / CR / LF','LOSSLESS TYPED','XML escaping; CR/LF or invalid XML code units use explicit utf16le-base64','NonXmlCodeUnitsAndWhitespaceHaveLosslessTypedSerialization'),
        ('double metadata','LOSSLESS TYPED','16 uppercase IEEE-754 hex bits; both runtimes test negative zero and nontrivial reference','CanonicalMetadataHasExactRuntimeIndependentDoubleBits')]
    emit(OUT/'formula-compatibility.json',dict(schema='04A08-formula-compatibility-v1',typed_version=1,contrast='core-default',
        intercept='Separate Boolean; no RHS intercept token',rows=[dict(case=a,status=b,rule=c,test=d) for a,b,c,d in formula],
        native_fitting_change=False,Core_modified=False,publication_status='Candidate technical evidence, NOT_GOLDEN'))
    observations={}
    for config in ['Debug','Release']:
        path=RUN/('authoring-observations-'+config+'-complete.json')
        data=json.loads(path.read_text(encoding='utf-8-sig'))
        assert len(data['observations'])==20 and all(r['equal'] and r['before_sha256']==r['after_sha256'] for r in data['observations'])
        observations[config]=dict(path=path.relative_to(ROOT).as_posix(),sha256=sha(path),assemblies=data['assemblies'],observations=data['observations'])
    a={(r['culture'],r['method'],r['seed']):r['canonical_typed_spec'] for r in observations['Debug']['observations']}
    b={(r['culture'],r['method'],r['seed']):r['canonical_typed_spec'] for r in observations['Release']['observations']}
    assert a==b
    release=observations['Release'];release['observations']=[{k:v for k,v in r.items() if k!='canonical_typed_spec'} for r in release['observations']]
    emit(OUT/'authoring-fixtures.json',dict(schema='04A08-authoring-fixtures-v1',base=BASELINE['head'],
        original_controller_sha256=BASELINE['source_pins']['BESHstatNG/src/UI/RegressionEffectsController.vb'],
        configurations=observations,debug_release_specs_identical=True,source_derived=True,native_executed=False,NOT_GOLDEN=True,
        count_convention='20 operation/culture/seed observations per config; nested repeated/orphan/removal/error cases are in executable tests, not extra snapshot files or MSTest identities.'))
    doc=ROOT/'docs/architecture/batch-04A-08-owner-review.md'
    assert '## Complete Python nonpass identities' not in doc.read_text(encoding='utf-8')
    config=validation['configurations']['Debug']
    appendix='\n\n## Complete Python nonpass identities\n\nBoth configurations have the same outcomes. **FAIL/ERROR remain nonpasses.**\n\n| Outcome | Exact method ID | Entry classification |\n|---|---|---|\n'
    entry=set(config['inherited_entry_plan_conflicts'])
    for identity,outcome in sorted(config['python_nonpasses'].items()):
        appendix+='| '+outcome+' | `'+identity+'` | '+('Entry Revision 2.5 plan conflict' if identity in entry else 'Retained 04A.07 nonpass')+' |\n'
    appendix+='\nReview artifact generation: `& $py -B scripts/summarize-04a08.py review`; final inventory generation: `& $py -B scripts/summarize-04a08.py closeout`. Both operate only on this batch\'s new review files and read existing evidence.\n'
    with doc.open('a',encoding='utf-8',newline='\n') as f:f.write(appendix)

if __name__=='__main__':
    import sys
    if sys.argv[1:] == ['closeout']:closeout()
    elif sys.argv[1:] == ['review']:review()
    else:
        value=summary();emit(OUT/'validation-v2.json',value)
        for c,r in value['configurations'].items():print(c,r['managed_pass'],r['managed_total'],r['node_pass'],r['python_pass'],r['python_total'],r['python_nonpassing_methods'])
