"""Read-only aggregation of 04A.07 logs/TRX; writes only new summary artifacts."""
from collections import Counter
import hashlib
import json
from pathlib import Path
import re
import xml.etree.ElementTree as ET

ROOT=Path(__file__).resolve().parents[1]
RUN=ROOT/'TestResults/04A-07-20261102-run01'
NS='{http://microsoft.com/schemas/VisualStudio/TeamTest/2010}'
OLD=ROOT/'TestResults/04A-06-20261102-run01/final-summary.json'
EXPECTED_NEW={
 'test_04a05_completion_successor.Completion05SuccessorTests.test_nonpaired_code_and_unrelated_forms_are_unchanged_from_partial':'Frozen Ui9/other-form scope assertion; only nested ANOVA member migrated, proportions/science paths unchanged.',
 'test_04a05_contingency.ContingencySourceGates.test_other_multicol_members_including_04a04_selection_unchanged':'Frozen multicol member assertion includes Run1WayRmANOVA; all unrelated members/acquisition/SelectBlocks calls unchanged.',
 'test_04a06_assumptions_successor.Assumptions06SuccessorTests.test_actual_form_routes_and_unchanged_native_tails':'C06 verifier forbids any other UibyID member change. C07 changes only the two additional approved grouped workflow members; all C06 report/native tails remain unchanged.',
 'test_04a06_assumptions_successor.Assumptions06SuccessorTests.test_native_population_is_union_not_sum':'Frozen C06 check first calls its whole-source verifier; failure is that earlier source scope, not changed population or ledger rows.',
 'test_04a06_assumptions_successor.Assumptions06SuccessorTests.test_new_scientific_candidates_are_explicitly_unmodified':'Frozen C06 whole-source check; all C06 scientific candidate/Core code is unchanged.',
 'test_04a06_assumptions_successor.Assumptions06SuccessorTests.test_no_duplicate_statistics_in_form_or_windows_adapter':'Frozen C06 whole-source scope assertion; new workflow and existing C06 service are checked explicitly by the C07 successor.',
 'test_04a06_assumptions_successor.Assumptions06SuccessorTests.test_production_sources_and_exact_open_ledger_lineage':'Frozen C06 source artifact check fails before ledger validation; all original rows/statuses checked separately in C07.',
 'test_investigation06.Investigation06Tests.test_anova_defaults_mapping_remain_explicit_source_facts':'Frozen text search expects property initializers inside UibyID; same flags now pass immutable option arguments to the existing unchanged dispatcher.'}
NO_LONGER_RECURRING=set()

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
        if 'AnovaWorkflow07' in method['className']:new.append(result)
    return dict(path=path.relative_to(ROOT).as_posix(),sha256=sha(path),counters=tree.find('.//'+NS+'Counters').attrib,failures=failed,new_tests=new)

def summarize():
    prior=json.loads(OLD.read_bytes());configurations={}
    for config in ['Debug','Release']:
        name='final-fixed-'+config
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
        outcome_changes={k:dict(before=v,after=failures[k]) for k,v in old['python_nonpasses'].items() if k in failures and failures[k]!=v}
        assert outcome_changes=={'test_04a03_control_mitigation.ControlMitigationSourceTests.test_per_mode_consumers_and_branch_order':dict(before='ERROR',after='FAIL')}, 'Unexplained historical outcome change'
        node_pass=sum(int(n) for n in re.findall(r'\bpass (\d+)\s*$',logs[0],re.M))
        node_fail=sum(int(n) for n in re.findall(r'\bfail (\d+)\s*$',logs[0],re.M))
        assert node_pass==916 and node_fail==0
        new_tests=[n for s in suites for n in s['new_tests']]
        assert len(new_tests)==237 and all(n['outcome']=='Passed' for n in new_tests)
        observations=json.loads((RUN/('observations-'+config+'-post-fix.json')).read_bytes())
        assert len(observations['observations'])==16 and all(o['equal'] for o in observations['observations'])
        configurations[config]=dict(managed_pass=sum(int(s['counters']['passed']) for s in suites),managed_total=sum(int(s['counters']['total']) for s in suites),
            managed_fail=len(managed),managed_skip=sum(int(s['counters']['notExecuted']) for s in suites),managed_suites=suites,
            managed_failures=managed,new_managed_tests=237,managed_functional_regressions=0,
            node_pass=node_pass,node_fail=node_fail,
            python_total=total,python_pass=total-len(failures)-skips,python_failed_methods=len(failures),python_failure_events=len(events),python_skip=skips,
            python_nonpasses=failures,python_events=events,
            prior_36_historical_nonpasses=old['python_nonpasses'],historical_not_recurring=absent,
            historical_not_recurring_reason='All prior 36 nonpassing methods recur; none was erased or relabeled as passing.',historical_outcome_changes=outcome_changes,
            outcome_change_reason='Frozen per-mode consumer test now fails an earlier in-form option-text assertion before its old missing-substring error. Still a nonpass, not a repair or native qualification.',
            additional_source_conflicts={k:dict(outcome=additional[k],cause=v) for k,v in EXPECTED_NEW.items()},
            newly_triggered_functional_regressions=0,successor_python_tests=10,source_check_nonpasses_not_waived=True,
            observation_pairs=16,observation_path=(RUN/('observations-'+config+'-post-fix.json')).relative_to(ROOT).as_posix(),semantic_repetitions=15,
            full_exit_code=1,explanation='Full commands retain failing exits; independent suites continue for accounting. Nonpasses remain failures.')
    assert configurations['Debug']['python_nonpasses']==configurations['Release']['python_nonpasses']
    assert configurations['Debug']['managed_pass']==configurations['Release']['managed_pass']
    record=dict(schema='04A07-final-validation-v2',base='b984e20bd5a8112a4f22ade94df8851cca3a61e8',branch='codex/04A-07',
        configurations=configurations,prior_summary_sha256=sha(OLD),native_executed=False,new_numerical_reference_runs=0,
        released_controls=0,discharged_obligations=0,P='NO',N1='NO GO',N2='NO',C2='UNMET',
        superseded_validation_sha256=sha(RUN/'final-summary.json'),genuine_development_regressions_found=1,genuine_development_regressions_remaining=0,
        repair='A real net48 numeric-text/high-precision nested-input provenance rejection was found after first full runs. Preserve raw evidence separately from surviving legacy Core input; two new real-producer culture cases added. No Core/legacy numeric input change.',
        full_repetitions='Original final-Debug/final-Release runs and summaries retained unchanged. Both full configurations repeated as final-fixed-* because production bridge and tests changed.')
    emit(RUN/'final-summary-v2.json',record)
    emit(ROOT/'docs/architecture/04A07/validation-v2.json',record)
    # One culture's canonical cells plus exact before/after evidence hashes; no duplicated raw logs.
    obs=json.loads((RUN/'observations-Debug-post-fix.json').read_bytes())
    canonical=[dict(scenario=o['scenario'],equal=o['equal'],tables=o['after'],metadata=o['metadata']) for o in obs['observations'] if o['culture']=='']
    emit(ROOT/'docs/architecture/04A07/report-fixtures-v2.json',dict(schema='04A07-source-characterized-report-fixtures-v2',
        provenance=obs['provenance'],scenarios=canonical,
        culture_checks=['InvariantCulture','sk-SK'],configurations=['Debug','Release'],
        evidence={p.name:sha(p) for p in [RUN/'observations-Debug-post-fix.json',RUN/'observations-Release-post-fix.json']},
        input_recipe='capture-04a07-observations.ps1: unequal grouped vectors 4/5/3, existing six-subject RM/Mauchly matrix, nested balanced/unbalanced arithmetic fixture',
        native_executed=False,P='NO'))
    print(json.dumps({c:{k:v for k,v in r.items() if k in ['managed_pass','managed_total','managed_fail','managed_skip','node_pass','node_fail','python_total','python_pass','python_failed_methods','python_failure_events','python_skip','new_managed_tests','observation_pairs']} for c,r in configurations.items()},indent=2))

if __name__=='__main__':summarize()
