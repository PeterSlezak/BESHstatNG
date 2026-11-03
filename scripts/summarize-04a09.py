"""Incremental sanitized 04A.09 evidence. Historical inputs are read only."""
import argparse
import base64
import csv
import hashlib
import importlib.util
import json
from pathlib import Path
import re
import subprocess
import xml.etree.ElementTree as ET

ROOT=Path(__file__).resolve().parents[1]
OUT=ROOT/'evidence/04A09'
RUN=ROOT/'TestResults/04A-09-20261103-run01'
spec=importlib.util.spec_from_file_location('contract09',ROOT/'scripts/capture-04a09-contract.py')
c=importlib.util.module_from_spec(spec);spec.loader.exec_module(c)
NS='{http://microsoft.com/schemas/VisualStudio/TeamTest/2010}'
def sha(p):return hashlib.sha256(p.read_bytes()).hexdigest()
def emit(name,value):
    if (OUT/name).exists():
        assert json.loads((OUT/name).read_bytes())==c.sanitize(value),'Existing incremental evidence differs: '+name
        return
    with (OUT/name).open('x',encoding='utf-8',newline='\n') as f:json.dump(c.sanitize(value),f,indent=2,ensure_ascii=False);f.write('\n')
def git(*args):return subprocess.check_output(['git','-c','safe.directory='+ROOT.as_posix(),*args],cwd=ROOT).decode().strip()
def all_members(text):
    return {m[3]:m[0] for m in re.finditer(r'^( +)(?:Public|Private|Friend)?\s*(?:(?:Shared|Async|Overrides|Overloads)\s+)*(Sub|Function)\s+(\w+)\b.*?^\1End \2',text,re.M|re.S)}
def writer_tail(body):
    tail=body.split("        'Dump results",1)[1]
    tail=re.sub(r'^        Dim res = [^\n]*(?:\n +If\([^\n]*)?\n','',tail,flags=re.M)
    if '            If clsRes IsNot Nothing' in tail:
        start=tail.index('            Global.BESHStatNG.regression.BinaryClassificationCore.ValidateBinaryInputs') if 'Global.BESHStatNG.regression.BinaryClassificationCore.ValidateBinaryInputs' in tail else tail.index('            \' Preserve the legacy post-fit')
        end=tail.index('            If clsRes IsNot Nothing',start)
        tail=tail[:start]+tail[end:]
    for model in ['lm','ordL','multL','zipFit','nb2','fitGlm']:
        for residual in ['AllResiduals_toPrint','wrapResiduals()','AllResiduals()']:
            tail=tail.replace('WriteRes.write('+model+'.'+residual+')','WriteRes.write(report.Rows.Residuals)')
        for fitted in ['Predicted','PredictedResponses']:
            tail=tail.replace('WriteRes.write('+model+'.'+fitted+', bTall:=True)','WriteRes.write(report.Rows.Fitted)')
    return '\n'.join(line.strip() for line in tail.splitlines() if line.strip())

def source(final=False):
    initial=json.loads((RUN/'baseline.json').read_bytes())['source_pins']
    records=[]
    paths=[c.FORM,'BESHstatNG/src/RegModels/BinaryClassificationReporting.vb','BESHstatNG/BESHStatNG.vbproj','BESHStatNG.Test/BESHStatNG.Tests.vbproj']
    before=c.members(c.original(c.FORM));after=c.members((ROOT/c.FORM).read_text(encoding='utf-8-sig'))
    changed=[n for n in before if before[n]!=after[n]]
    assert set(changed)=={'RunOLS','RunOrdLogit','RunMultiLogit','RunZIP','RunGLMNB2','RunGLM'},changed
    tails={n:writer_tail(before[n])==writer_tail(after[n]) for n in changed}
    assert all(tails.values()),tails
    for p in paths:
        records.append(dict(path=p,before_sha256=initial[p],after_sha256=sha(ROOT/p)))
    facade='BESHstatNG/src/RegModels/BinaryClassificationReporting.vb'
    old=all_members(c.original(facade));current=all_members((ROOT/facade).read_text(encoding='utf-8-sig'))
    assert old and all(current[n]==body for n,body in old.items())
    protected=[p for p in initial if p.startswith(('BESHStat.Core/','evidence/04A08/')) or p.endswith('.Designer.vb') or '/ExcelUDFs/' in p or p.startswith('planning/')]
    assert all(sha(ROOT/p)==initial[p] for p in protected)
    emit('source-delta-final.json' if final else 'source-delta.json',dict(base=c.BASE,modified_existing=records,
        modified_form_members={n:dict(before_sha256=hashlib.sha256(before[n].encode()).hexdigest(),after_sha256=hashlib.sha256(after[n].encode()).hexdigest()) for n in changed},
        unchanged_form_members={n:hashlib.sha256(body.encode()).hexdigest() for n,body in before.items() if n not in changed},
        added_form_members=sorted(set(after)-set(before)),existing_facade_members_unchanged=sorted(old),
        writer_tails_equal_after_only_documented_substitutions=tails,
        writer_substitutions=['wrapResults moved before first workbook write; called once','Core residual array -> validated report.Rows.Residuals','Core vector/tall -> aligned report.Rows.Fitted matrix; managed writer-base matrix parity tested','GLM classification computation replaced by Common; original post-fit reads/condition and chart/write tail retained'],
        protected_source_pins={p:initial[p] for p in sorted(protected)},native_executed=False))
    # Expand the initial direct-six-handler crosswalk to transitive input/validation reads.
    bodies='\n'.join(before[n] for n in changed+['GetData','BuildExpandedRegressionInputs','BuildExpandedZIPPoissonInputs','BuildExpandedZIPLogisticInputs','ValidateExpandedInitialValuesCount','GetClassificationThresholdUiValue','valiateInputs'])
    with (ROOT/'docs/architecture/WindowsMigrationLedger/ui-controls.csv').open(encoding='utf-8-sig') as f:controls=list(csv.DictReader(f))
    ledger=json.loads((ROOT/c.LEDGER).read_bytes());rows=[]
    digest=lambda r:hashlib.sha256(json.dumps(r,sort_keys=True,ensure_ascii=False).encode()).hexdigest()
    for row in controls:
        cid=row['control_id']
        if cid.split('.')[-2]=='UiGLM' and re.search(r'\b'+re.escape(cid.split('.')[-1])+r'\b',bodies):
            obligation=next((r for r in ledger if r['original_id']==cid),None)
            rows.append(dict(control_id=cid,control_row=row,control_row_sha256=digest(row),obligation_id=obligation['obligation_id'] if obligation else None,
                obligation_row=obligation,obligation_row_sha256=digest(obligation) if obligation else None,
                source_proven=True,host_free_tested='Adapter contracts/managed matrix; no control interaction asserted',native='BLOCKED',status='OPEN' if obligation else 'NO ORIGINAL DEFERRED OBLIGATION ROW'))
    emit('restriction-crosswalk-v2.json',dict(base=c.BASE,original_direct_crosswalk_sha256=sha(OUT/'restriction-crosswalk.json'),
        scope='Six fit handlers plus their unchanged input/expanded-count/threshold validation dependencies; associated is not a native exercise claim.',
        controls=rows,associated_controls=len(rows),associated_obligations=sum(r['obligation_id'] is not None for r in rows),released=0,discharged=0,
        authorities={p:sha(ROOT/p) for p in [c.LEDGER,'docs/architecture/WindowsMigrationLedger/ui-controls.csv','docs/architecture/04A02/04A02-final-amended-acceptance-PROPOSED.json']},
        P='NO',N1='NO GO',N2='NO',C2='UNMET',
        owner_disposition=dict(id='C09-NATIVE-STRICT-01',request='Approve exact-ID deferral of activating new strict Windows rejection paths until regression pre-change native capture/risk authority; retain observation/legacy validation meanwhile. No N release requested.',
            control_ids=[r['control_id'] for r in rows],obligation_ids=[r['obligation_id'] for r in rows if r['obligation_id']],
            withheld='Throw/display/stop based on new RegressionRequestPolicy diagnostics. No such refusal branch activated. Common strict validation remains available and tested.'),
        placeholder_mapping={'<OWNER_HOME>':'Original owner-home prefix; original-row hash refers to unsanitized bytes','<OWNER_SID>':'Original owner SID'}))
    emit('runtime-baselines.json',dict(base=c.BASE,
        net10='12 Core-backed cases executed before production edits. See original baseline-manifest.json.',
        net48='12 Core-backed cases executed after bridge edits against byte-unchanged Core source; separate runtime baselines retain Framework G-format diagnostic strings. NOT a pre-change native run.',
        files={p.name:dict(sha256=sha(p),bytes=p.stat().st_size) for p in sorted((OUT/'baselines').glob('*.txt'))},
        capture_source_original_sha256=sha(RUN/'preimages/Regression09Baselines.vb'),current_harness_sha256=sha(ROOT/'BESHStat.Excel.Common.Tests/src/Regression09Baselines.vb'),
        masking='Only computational wall-clock text. CLR types, labels, numbers, RowIds, table order and p-value metadata retained. Base64 string cells decode as UTF-8; D: is invariant round-trip Double. Separate runtime strings never normalized into parity.',
        native=False,P='NO'))
    historical={p:h for p,h in initial.items() if p.startswith(('BESHStat.Core.Tests/','BESHStat.Excel.Common.Tests/','BESHStatNG.Test/src/','BESHStatNG.Test/TestData/'))}
    assert all(sha(ROOT/p)==h for p,h in historical.items())
    emit('historical-source-protection.json',dict(base=c.BASE,pins=historical,all_unchanged=True,
        historical_testresults_files=json.loads((RUN/'start.json').read_bytes())['historical_files'],
        retention='Metadata inventory membership, sizes, creation/write timestamps and file identity checked at closeout; no historical rehash/rebaseline or native claim. Raw inventory remains private.'))

def trx(p):
    tree=ET.parse(p)
    counters=tree.find('.//'+NS+'Counters').attrib
    definitions={t.attrib['id']:t.find(NS+'TestMethod').attrib for t in tree.findall('.//'+NS+'TestDefinitions/'+NS+'UnitTest')}
    failures=[]
    batch09=[]
    for result in tree.findall('.//'+NS+'UnitTestResult'):
        d=definitions.get(result.attrib['testId'],{})
        if 'Regression09' in d.get('className',''):
            batch09.append(dict(class_name=d.get('className'),method=d.get('name'),test_name=result.attrib['testName'],outcome=result.attrib['outcome']))
        if result.attrib['outcome']!='Passed':
            failures.append(dict(class_name=d.get('className'),method=d.get('name'),test_name=result.attrib['testName'],outcome=result.attrib['outcome']))
    return dict(path=p.relative_to(ROOT).as_posix(),sha256=sha(p),counters=counters,nonpasses=failures,batch09_cases=batch09)

def validation():
    prior=json.loads((ROOT/'evidence/04A08/validation-v2.json').read_bytes())
    configs={}
    for config in ['Debug','Release']:
        name='final-'+config
        paths=[RUN/name/config/'results'/s for s in ['Core.trx','OfficeAddin.trx','OfficeExecution.trx']]
        # The accepted execution-copy helper places these directly under name/config/results.
        paths=[p if p.exists() else RUN/name/'results'/p.name for p in paths]
        paths += [RUN/'executions'/(name+'-'+suite)/'results'/(suite+'.trx') for suite in ['Common','Legacy']]
        paths += [RUN/'projection-executions'/name/'results/ControlMitigation.trx']
        suites=[trx(p) for p in paths]
        logs=[(RUN/(name+suffix+'.log')).read_text(encoding='utf-8',errors='replace') for suffix in ['-full','-common-python']]
        nonpasses={}
        events=[]
        for log in logs:
            for outcome,method,context in re.findall(r'^(FAIL|ERROR): [^\n]+? \((test_[^()]+)\)([^\n]*)$',log,re.M):
                nonpasses[method]=outcome
                events.append(dict(outcome=outcome,identity=method,context=context.strip()))
        inherited=prior['configurations'][config]['python_nonpasses']
        expected={'test_04a08_authoring_successor.Authoring08SuccessorTests.test_exact_id_crosswalk_retains_original_row_hashes_and_open_counts',
                  'test_04a08_authoring_successor.Authoring08SuccessorTests.test_original_numerics_and_shipping_forms_are_unchanged'}
        assert set(nonpasses)-set(inherited)==expected
        assert all(nonpasses.get(k)==v for k,v in inherited.items())
        python_total=sum(int(n) for log in logs for n in re.findall(r'^Ran (\d+) tests? in',log,re.M))
        configs[config]=dict(managed_suites=suites,total=sum(int(s['counters']['total']) for s in suites),passed=sum(int(s['counters']['passed']) for s in suites),
            failed=sum(int(s['counters']['failed']) for s in suites),python_total=python_total,python_passed=python_total-len(nonpasses),python_nonpasses=nonpasses,
            python_failure_events=events,python_distinct_nonpasses=len(nonpasses),inherited_python_nonpasses=len(inherited),new_python_pin_failures=sorted(expected),
            new_managed_case_executions=sum(len(s['batch09_cases']) for s in suites),new_managed_nonpasses=[t for s in suites for t in s['batch09_cases'] if t['outcome']!='Passed'],
            node_passed=sum(int(n) for n in re.findall(r'\bpass (\d+)\s*$',logs[0],re.M)),node_failed=sum(int(n) for n in re.findall(r'\bfail (\d+)\s*$',logs[0],re.M)),
            final_command=json.loads((RUN/(name+'-full-command.json')).read_bytes()))
    emit('validation-v2.json',dict(base=c.BASE,configurations=configs,prior_validation_sha256=sha(ROOT/'evidence/04A08/validation-v2.json'),
        supersedes='validation.json: aggregation parser v1 treated four extra subtest events as distinct methods; v2 preserves all events but counts method identities correctly. No test outcome changed.',
        prior_results='Recorded context only; current commands independently rerun. See exact current identities.',native=False,P='NO',
        public_command_mapping={'<OWNER_HOME>':'Owner home prefix; local Python executable recorded privately. Public runs use <PYTHON_EXE>.'}))
    print(json.dumps({k:{n:v for n,v in r.items() if n in ['total','passed','failed','python_total','python_passed','python_distinct_nonpasses','new_managed_case_executions','new_managed_nonpasses','node_passed','node_failed']} for k,r in configs.items()},indent=2))

def supplements():
    fixtures={}
    def cell(line):
        if line=='NULL':return None
        kind,value=line.split(':',1)
        if kind=='D':return value # Preserve round-trip decimal text, including NaN.
        return base64.b64decode(value).decode('utf-8')
    for p in sorted((OUT/'baselines').glob('*.txt')):
        lines=p.read_text().splitlines();sections=[];i=3
        while i<len(lines) and lines[i].startswith('TABLE '):
            metadata=lines[i];rows,cols=map(int,lines[i+1].split());values=[cell(x) for x in lines[i+2:i+2+rows*cols]]
            sections.append(dict(metadata=metadata,first_rows=[values[r*cols:(r+1)*cols] for r in range(min(rows,4))]))
            i+=2+rows*cols
        assert lines[i]=='FITTED';i+=1
        shape=list(map(int,lines[i].split()));i+=1
        fitted=[cell(x) for x in lines[i:i+min(3,shape[0])*shape[1]]]
        ids=list(map(int,lines[1].split(',')))
        fixtures[p.name]=dict(sha256=sha(p),row_count=len(ids),source_rows_first=ids[:3],source_rows_last=ids[-3:],
            convergence=lines[2],table_count=len(sections),sections=sections,fitted_shape=shape,fitted_first_rows=fitted)
    emit('fixture-readable-summary.json',dict(base=c.BASE,scope='Readable first rows only; complete table/numeric/residual payloads are the hashed baselines. Synthetic committed fixture values.',fixtures=fixtures))
    old=json.loads((ROOT/'evidence/04A08/restriction-crosswalk.json').read_bytes())['affected_controls']
    new=json.loads((OUT/'restriction-crosswalk-v2.json').read_bytes())['controls']
    a={r['control_id'] for r in old};b={r['control_id'] for r in new}
    ao={r['obligation_id'] for r in old if r['obligation_id']};bo={r['obligation_id'] for r in new if r['obligation_id']}
    emit('restriction-delta.json',dict(prior_associated_controls=len(a),current_associated_controls=len(b),overlapping_controls=sorted(a&b),newly_associated_controls=sorted(b-a),
        prior_associated_obligations=len(ao),current_associated_obligations=len(bo),overlapping_obligations=sorted(ao&bo),newly_associated_obligations=sorted(bo-ao),
        interpretation='Set differences describe association with extraction scope only; no new original obligation, native release or discharge. Historic populations overlap and must not be summed.'))
    initial=json.loads((RUN/'baseline.json').read_bytes())['source_pins']
    form=c.FORM
    source_delta=json.loads((OUT/'source-delta-final.json').read_bytes())
    tests=['test_04a08_authoring_successor.Authoring08SuccessorTests.test_exact_id_crosswalk_retains_original_row_hashes_and_open_counts',
           'test_04a08_authoring_successor.Authoring08SuccessorTests.test_original_numerics_and_shipping_forms_are_unchanged']
    emit('frozen-pin-successor.json',dict(base=c.BASE,newly_nonpassing_frozen_test_ids=tests,
        old_expectation='UiGLM unchanged from b488774 (04A.07); source was still identical at c25c695 entry main. Exact whole-form equality and c08 build() assertion remain frozen.',
        old_sha256=initial[form],new_sha256=sha(ROOT/form),changed_members=source_delta['modified_form_members'],added_members=source_delta['added_form_members'],
        new_evidence='83 Common + 99 host-free Windows managed cases; eight additive source checks; full final executions recorded separately.',
        disposition='Both remain FAIL. No skip/xfail/rebaseline/original-test edit; only versioned successor evidence. All original native/scientific gates remain.',
        other_changed_pins=source_delta['modified_existing']))
    historical={}
    for name in ['historical-strict','historical-profile']:
        log=RUN/(name+'.log');command=json.loads((RUN/(name+'-command.json')).read_bytes())
        historical[name]=dict(command=command,raw_log_sha256=sha(log),output=log.read_text(encoding='utf-8',errors='replace'))
    preservation=json.loads((RUN/'before-closeout-preservation.json').read_bytes()) if (RUN/'before-closeout-preservation.json').exists() else None
    emit('historical-checkers.json',dict(base=c.BASE,checks=historical,preservation=preservation,
        limitations='Original strict/profile expectations are untouched; actual checkers fail on stale historical dependencies/pins. They are nonpasses, never relabelled as success.'))

def closeout():
    paths=git('diff','--name-only').splitlines()+git('ls-files','--others','--exclude-standard').splitlines()
    expected={'BESHstatNG/src/UI/UiGLM.vb','BESHstatNG/src/RegModels/BinaryClassificationReporting.vb','BESHstatNG/BESHStatNG.vbproj','BESHStatNG.Test/BESHStatNG.Tests.vbproj'}
    assert set(git('diff','--name-only').splitlines())==expected
    assert not git('diff','--cached','--name-only')
    files={p:dict(sha256=sha(ROOT/p),bytes=(ROOT/p).stat().st_size,status='modified tracked' if p in git('diff','--name-only').splitlines() else 'untracked publication candidate') for p in sorted(set(paths)) if (ROOT/p).is_file()}
    for p in files:
        result=subprocess.run(['git','-c','safe.directory='+ROOT.as_posix(),'check-ignore','-q',p],cwd=ROOT)
        assert result.returncode==1,'Publication path ignored: '+p
    emit('publication-manifest.json',dict(base=c.BASE,head=git('rev-parse','HEAD'),branch=git('branch','--show-current'),index_empty=not git('diff','--cached','--name-only'),files=files,
        ignored_private='Entire TestResults history, baseline metadata/preimages, raw logs/TRXs/commands remain ignored and excluded. No historical bytes or prior public evidence changed.',
        placeholder_mapping={'<OWNER_HOME>':'Public sanitized derivative of owner home prefix','<OWNER_SID>':'Public sanitized derivative of owner SID','<PYTHON_EXE>':'Qualified local portable Python'},
        manifest_self_excluded=True,stage_commit_push=False))

def review():
    value=json.loads((OUT/'validation-v2.json').read_bytes())
    config=value['configurations']['Debug']
    other=value['configurations']['Release']
    assert config['python_nonpasses']==other['python_nonpasses']
    assert config['new_managed_nonpasses']==other['new_managed_nonpasses']==[]
    prior=json.loads((ROOT/'evidence/04A08/validation-v2.json').read_bytes())['configurations']['Debug']['python_nonpasses']
    path=ROOT/'docs/architecture/batch-04A-09-owner-review.md'
    body=path.read_text(encoding='utf-8')
    if '## Complete Python nonpass identities' not in body:
        appendix='\n\n## Complete Python nonpass identities\n\nBoth configurations have the same outcomes. Failure/error remain nonpasses.\n\n| Outcome | Exact method ID | Attribution |\n|---|---|---|\n'
        for identity,outcome in sorted(config['python_nonpasses'].items()):
            appendix+='| '+outcome+' | `'+identity+'` | '+('Inherited 04A.08 identity/outcome' if identity in prior else 'Newly triggered frozen whole-form pin; versioned successor evidence only')+' |\n'
        path.write_text(body+appendix,encoding='utf-8',newline='\n')
    preservation=RUN/'preservation-final.json'
    emit('preservation-final.json',dict(receipt=json.loads(preservation.read_bytes()),raw_receipt_sha256=sha(preservation),
        starting_inventory_count=json.loads((RUN/'start.json').read_bytes())['historical_files'],
        historical_inventory_private=True,initial_free_bytes=json.loads((RUN/'start.json').read_bytes())['disk_free_bytes']))
    commands=[]
    for p in sorted(RUN.glob('*-command.json')):
        record=json.loads(p.read_bytes());log=p.with_name(p.name.replace('-command.json','.log'))
        commands.append(dict(path=p.relative_to(ROOT).as_posix(),raw_sha256=sha(p),record=record,
            raw_log_sha256=sha(log) if log.exists() else None))
    for p in sorted((RUN/'executions').glob('*/execution.json')):
        commands.append(dict(path=p.relative_to(ROOT).as_posix(),raw_sha256=sha(p),record=json.loads(p.read_bytes())))
    copies=[]
    for p in sorted(RUN.glob('*-verify-isolated.provenance.json')):
        copies.append(dict(path=p.relative_to(ROOT).as_posix(),raw_sha256=sha(p),record=json.loads(p.read_bytes())))
    emit('command-outcomes.json',dict(base=c.BASE,commands=commands,execution_copies=copies,
        literal_original_verifier_run=False,reason='Original fixed historical paths, restore and packaging conflict with explicit retention/no-network/no-signing instructions; recorded execution copies used.',
        placeholder_mapping={'<REPO>':'Local absolute repository prefix','<PYTHON_EXE>':'Qualified owner portable Python executable','<VS2026>':'Visual Studio 2026 installation prefix','<OWNER_HOME>':'Owner home prefix','<OWNER_SID>':'Owner SID'}))

if __name__=='__main__':
    p=argparse.ArgumentParser(description=__doc__);p.add_argument('action',choices=['source','source-final','validation','supplements','closeout','review']);a=p.parse_args()
    dict(source=source,**{'source-final':lambda:source(True)},validation=validation,supplements=supplements,closeout=closeout,review=review)[a.action]()
