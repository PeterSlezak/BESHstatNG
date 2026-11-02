"""Read-only aggregation of 04A.06 logs/TRX; writes only new summary artifacts."""
from collections import Counter
import hashlib
import json
from pathlib import Path
import re
import xml.etree.ElementTree as ET

ROOT=Path(__file__).resolve().parents[1]
RUN=ROOT/'TestResults/04A-06-20261102-run01'
NS='{http://microsoft.com/schemas/VisualStudio/TeamTest/2010}'
OLD=ROOT/'TestResults/04A-27-20261102-completion-01/final-summary.json'
EXPECTED_NEW={
 'test_04a03_control_mitigation.ControlMitigationSourceTests.test_per_mode_consumers_and_branch_order':'Old source projection expects the removed in-form Core option reader chain.',
 'test_04a03_control_mitigation.ControlMitigationSourceTests.test_sealed_sources_are_reconciled_never_rewritten':'Frozen UibyID whole-file source pin; protected parent is unchanged.',
 'test_04a03_control_mitigation.NativeGateNegativeTests.test_review_packet_source_pins_match_current_sources':'Frozen UibyID whole-file source pin subtest.',
 'test_04a03_selection.UnivariateSelectionSourceTests.test_all_native_lifetime_cancel_write_and_reporting_methods_unchanged':'Two source-body subtests (RunNormality/RunDescStat); native tails and all lifecycle methods match in successor.',
 'test_04a03_selection.UnivariateSelectionSourceTests.test_only_two_form_methods_change':'Old extraction expected only two changed source methods; current seven-method extraction is intentional.',
 'test_04a27_geometry_completion.Geometry27CompletionTests.test_compile_targets_and_native_helpers_unchanged':'Frozen Windows test-project/source pin; additive compile entries only, helpers/targets unchanged.',
 'test_04a27_geometry_completion.Geometry27CompletionTests.test_real_consumer_routes_and_udf_exports_preserved':'Frozen complete UibyID member assertion; acquisition and UDF exports unchanged in successor.',
 'test_04a27_geometry_completion.Geometry27CompletionTests.test_source_owner_pins_and_all_44_original_rows':'Frozen tracked-source pins include UibyID/test project; all 44 original rows still match.',
 'test_investigation06.Investigation06Tests.test_normality_5000_disagreement_preserved_as_review_defect':'Old test searches bounds in form; production NormalityReportEligibility preserves bounds and executable n=5000 regression.'}
NO_LONGER_RECURRING={
 'test_04a04_alignment.PairedBlockSourceGates.test_udf_exports_and_packaging_unchanged',
 'test_04a05_completion_successor.Completion05SuccessorTests.test_no_designer_importer_targets_exports_or_signing_change',
 'test_04a05_contingency.ContingencySourceGates.test_no_designer_udf_numerical_packaging_or_framework_change'}

def sha(path):return hashlib.sha256(path.read_bytes()).hexdigest()
def emit(path,value):
    with path.open('x',encoding='utf-8',newline='\n') as stream:json.dump(value,stream,indent=2,ensure_ascii=False);stream.write('\n')

def trx(path):
    tree=ET.parse(path)
    methods={n.attrib['id']:n.find(NS+'TestMethod').attrib for n in tree.findall('.//'+NS+'UnitTest')}
    failed=[];new=[]
    for row in tree.findall('.//'+NS+'UnitTestResult'):
        method=methods[row.attrib['testId']];identity=method['className']+'.'+method['name']
        result=dict(identity=identity,test_name=row.attrib['testName'],outcome=row.attrib['outcome'])
        if row.attrib['outcome']!='Passed':failed.append(result)
        if 'AssumptionReport06' in method['className']:new.append(result)
    return dict(path=path.relative_to(ROOT).as_posix(),sha256=sha(path),counters=tree.find('.//'+NS+'Counters').attrib,failures=failed,new_tests=new)

def summarize():
    prior=json.loads(OLD.read_bytes());configurations={}
    for config in ['Debug','Release']:
        name='final-'+config
        paths=[RUN/name/config/'results'/(s+'.trx') for s in ['Core','OfficeAddin','OfficeExecution']]
        paths += [RUN/'executions'/(name+'-'+s)/'results'/(s+'.trx') for s in ['Common','Legacy']]
        paths += [RUN/'projection-executions'/name/'results/ControlMitigation.trx']
        suites=[trx(p) for p in paths]
        logs=[(RUN/(name+s+'.log')).read_text(encoding='utf-8',errors='replace') for s in ['-full','-common-python']]
        events=[];total=0;skips=0
        for log in logs:
            total+=sum(int(n) for n in re.findall(r'^Ran (\d+) tests? in',log,re.M))
            for outcome,identity,context in re.findall(r'^(FAIL|ERROR): [^\n]+? \((test_[^()]+)\)([^\n]*)$',log,re.M):
                events.append(dict(identity=identity,outcome=outcome,subtest=context.strip() or None))
            skips+=sum(int(n) for n in re.findall(r'^(?:OK|FAILED) \([^\n]*skipped=(\d+)',log,re.M))
        failures={e['identity']:e['outcome'] for e in events}
        old=prior['configurations'][config]
        additional={k:v for k,v in failures.items() if k not in old['python_nonpasses']}
        absent={k:v for k,v in old['python_nonpasses'].items() if k not in failures}
        managed=[f for s in suites for f in s['failures']]
        assert Counter(f['identity'] for f in managed)==Counter(f['identity'] for f in old['managed_failures']), 'Actual managed regression'
        assert set(additional)==set(EXPECTED_NEW), 'Unexplained additional Python nonpass'
        assert set(absent)==NO_LONGER_RECURRING,'Unexpected historical outcome change'
        assert all(failures[k]==v for k,v in old['python_nonpasses'].items() if k in failures)
        node_pass=sum(int(n) for n in re.findall(r'\bpass (\d+)\s*$',logs[0],re.M))
        node_fail=sum(int(n) for n in re.findall(r'\bfail (\d+)\s*$',logs[0],re.M))
        assert node_pass==916 and node_fail==0
        new_tests=[n for s in suites for n in s['new_tests']]
        assert len(new_tests)==106 and all(n['outcome']=='Passed' for n in new_tests)
        observations=json.loads((RUN/('observations-'+config+'-v2.json')).read_bytes())
        assert len(observations['observations'])==22 and all(o['equal'] for o in observations['observations'])
        configurations[config]=dict(managed_pass=sum(int(s['counters']['passed']) for s in suites),managed_total=sum(int(s['counters']['total']) for s in suites),
            managed_fail=len(managed),managed_skip=sum(int(s['counters']['notExecuted']) for s in suites),managed_suites=suites,
            managed_failures=managed,new_managed_tests=106,managed_functional_regressions=0,
            node_pass=node_pass,node_fail=node_fail,
            python_total=total,python_pass=total-len(failures)-skips,python_failed_methods=len(failures),python_failure_events=len(events),python_skip=skips,
            python_nonpasses=failures,python_events=events,
            prior_30_historical_nonpasses=old['python_nonpasses'],historical_not_recurring=absent,
            historical_not_recurring_reason='These three frozen checks use current Git-diff scope. After the owner published 04A.27, its importer/earlier-form changes are in main, not the new batch diff. No old test/expectation was modified or corrected.',
            additional_source_conflicts={k:dict(outcome=additional[k],cause=v) for k,v in EXPECTED_NEW.items()},
            newly_triggered_functional_regressions=0,successor_python_tests=8,source_check_nonpasses_not_waived=True,
            observation_pairs=22,observation_path=(RUN/('observations-'+config+'-v2.json')).relative_to(ROOT).as_posix(),semantic_repetitions=15,
            full_exit_code=1,explanation='Full commands retain failing exits; independent suites continue for accounting. Nonpasses remain failures.')
    assert configurations['Debug']['python_nonpasses']==configurations['Release']['python_nonpasses']
    assert configurations['Debug']['managed_pass']==configurations['Release']['managed_pass']
    record=dict(schema='04A06-final-validation-v1',base='4a3d2e16b4d76a559427d596a424d382bcb0b915',branch='codex/04A-06',
        configurations=configurations,prior_summary_sha256=sha(OLD),native_executed=False,new_numerical_reference_runs=0,
        released_controls=0,discharged_obligations=0,P='NO',N1='NO GO',N2='NO',C2='UNMET')
    emit(RUN/'final-summary.json',record)
    emit(ROOT/'docs/architecture/04A06/validation-v1.json',record)
    # One culture's canonical cells plus exact before/after evidence hashes; no duplicated raw logs.
    obs=json.loads((RUN/'observations-Debug-v2.json').read_bytes())
    canonical=[dict(scenario=o['scenario'],equal=o['equal'],table=o['after']) for o in obs['observations'] if o['culture']=='']
    emit(ROOT/'docs/architecture/04A06/report-fixtures-v1.json',dict(schema='04A06-source-characterized-report-fixtures-v1',
        provenance=obs['provenance'],scenarios=canonical,
        culture_checks=['InvariantCulture','sk-SK'],configurations=['Debug','Release'],
        evidence={p.name:sha(p) for p in [RUN/'observations-Debug-v2.json',RUN/'observations-Release-v2.json']},
        input_recipe='capture-04a06-observations.ps1 New-Data, reversed sin/cos vectors n30/n32; Rosner inputs 0,1,2,10 injected extremes',
        native_executed=False,P='NO'))
    print(json.dumps({c:{k:v for k,v in r.items() if k in ['managed_pass','managed_total','managed_fail','managed_skip','node_pass','node_fail','python_total','python_pass','python_failed_methods','python_failure_events','python_skip','new_managed_tests','observation_pairs']} for c,r in configurations.items()},indent=2))

if __name__=='__main__':summarize()
