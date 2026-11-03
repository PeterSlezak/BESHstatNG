"""Sanitized incremental 04A.11 closeout; frozen earlier evidence is read-only."""
import argparse
import base64
from collections import defaultdict
import importlib.util
import json
from pathlib import Path
import re
import subprocess
import sys
import xml.etree.ElementTree as ET

ROOT=Path(__file__).resolve().parents[1]
spec=importlib.util.spec_from_file_location('c11',ROOT/'scripts/capture-04a11-contract.py')
c=importlib.util.module_from_spec(spec);spec.loader.exec_module(c)
OUT=c.OUT;RUN=c.RUN;NS='{http://microsoft.com/schemas/VisualStudio/TeamTest/2010}'

def trx(p):
    tree=ET.parse(p)
    definitions={n.attrib['id']:n.find(NS+'TestMethod').attrib for n in tree.findall('.//'+NS+'TestDefinitions/'+NS+'UnitTest')}
    records=[]
    for node in tree.findall('.//'+NS+'UnitTestResult'):
        d=definitions.get(node.attrib['testId'],{})
        records.append(dict(class_name=d.get('className'),method=d.get('name'),test_name=node.attrib['testName'],outcome=node.attrib['outcome']))
    return dict(path=p.relative_to(ROOT).as_posix(),sha256=c.sha(p),counters=tree.find('.//'+NS+'Counters').attrib,
                nonpasses=[r for r in records if r['outcome']!='Passed'],batch11=[r for r in records if 'Regression11' in (r['class_name'] or '')],records=records)

def test_sources(project, explicit=False):
    """Resolve linked/implicit test source identities; never infer shared code from a name alone."""
    project=ROOT/project;files=set()
    tree=ET.parse(project)
    for node in tree.iter():
        if node.tag.split('}')[-1]=='Compile' and node.get('Include'):
            path=node.get('Include')
            if '$(' not in path and '*' not in path:
                p=(project.parent/path.replace('\\','/')).resolve()
                if p.is_file():files.add(p)
    if not explicit:
        for suffix in ['*.vb','*.cs']:
            for p in project.parent.rglob(suffix):
                if not any(x.lower() in ['bin','obj','packages','node_modules','testresults'] for x in p.relative_to(project.parent).parts):files.add(p)
    classes=defaultdict(list)
    for p in files:
        text=p.read_text(encoding='utf-8-sig',errors='replace')
        for name in set(re.findall(r'\b(?:Class|class)\s+(\w+)',text)):
            classes[name].append((c.sha(p),text))
    return classes

def deduplicate(suites):
    projects=['BESHStat.Core.Tests/BESHStat.Core.Tests.vbproj','BESHStat.OfficeAddin/tests/OfficeAddin.Tests.csproj','BESHStat.WasmSmoke/tests/Execution/Execution.Tests.csproj',
              'BESHStat.Excel.Common.Tests/BESHStat.Excel.Common.Tests.vbproj','BESHStatNG.Test/BESHStatNG.Tests.vbproj','BESHStatNG.Test/ControlMitigation/ControlMitigation.Tests.vbproj']
    candidates=defaultdict(list)
    for index,suite in enumerate(suites):
        sources=test_sources(projects[index],explicit=index>=4)
        for ordinal,row in enumerate(suite['records']):
            leaf=row['class_name'].split('.')[-1]
            hashes={h for h,text in sources.get(leaf,[]) if re.search(r'\b'+re.escape(row['method'])+r'\s*\(',text)}
            # MSTest 3 quotes String arguments; MSTest 2 does not. Only merge this
            # presentation normalization when the exact source is shared and there is
            # at most one candidate per suite, avoiding quote-ambiguous data rows.
            normalized=row['test_name'].replace('"','')
            key=(leaf,row['method'],normalized)
            candidates[key].append(dict(suite=suite['path'],ordinal=ordinal,source_hashes=sorted(hashes),**row))
    groups={}
    for key,rows in candidates.items():
        shared=set(rows[0]['source_hashes'])
        for row in rows[1:]:shared.intersection_update(row['source_hashes'])
        unique_suites=len({r['suite'] for r in rows})==len(rows)
        if len(rows)>1 and shared and unique_suites:
            groups[('shared-source',sorted(shared)[0],*key)]=rows
        else:
            for row in rows:groups[('runtime-case',row['suite'],row['ordinal'])]=[row]
    return groups

def validation():
    prior=json.loads((ROOT/'evidence/04A10/validation.json').read_bytes());configs={}
    for config in ['Debug','Release']:
        name='final-'+config
        paths=[RUN/name/config/'results'/s for s in ['Core.trx','OfficeAddin.trx','OfficeExecution.trx']]
        paths += [RUN/'executions'/('final-compat-'+config+'-'+suite)/'results'/(suite+'.trx') for suite in ['Common','Legacy']]
        paths += [RUN/'projection-executions'/name/'results/ControlMitigation.trx']
        suites=[trx(p) for p in paths]
        ident=lambda r:(r['class_name'],r['method'],r['test_name'],r['outcome'])
        old={ident(r) for s in prior['configurations'][config]['managed_suites'] for r in s['nonpasses']}
        current={ident(r) for s in suites for r in s['nonpasses']}
        assert old<=current,(config,old-current)
        newly=[r for s in suites for r in s['nonpasses'] if ident(r) not in old]
        assert all(r['method']=='ConstantAndSeparatedBinomialPreserveActualFitWarnings' and 'True' in r['test_name'] for r in newly),newly
        assert len(newly)==2
        groups=deduplicate(suites)
        logical_pass=sum(all(r['outcome']=='Passed' for r in values) for values in groups.values())
        logical_failed=sum(any(r['outcome']=='Failed' for r in values) for values in groups.values())
        logical_skipped=sum(all(r['outcome'] in ['NotExecuted','Skipped'] for r in values) for values in groups.values())
        collisions=[dict(logical_identity=list(k),executions=v) for k,v in groups.items() if len(v)>1]
        for suite in suites:del suite['records']
        logs=[(RUN/(name+suffix+'.log')).read_text(encoding='utf-8',errors='replace') for suffix in ['-full','-common-python']]
        nonpasses={};events=[]
        for log in logs:
            for outcome,method,context in re.findall(r'^(FAIL|ERROR): [^\n]+? \((test_[^()]+)\)([^\n]*)$',log,re.M):
                nonpasses[method]=outcome;events.append(dict(outcome=outcome,identity=method,context=context.strip()))
        inherited=prior['configurations'][config]['python_nonpasses']
        assert set(inherited)<=set(nonpasses)
        new_python=sorted(set(nonpasses)-set(inherited))
        allowed={
            'test_04a05_contingency.ContingencySourceGates.test_no_designer_udf_numerical_packaging_or_framework_change',
            'test_04a09_regression_successor.Regression09SuccessorTests.test_frozen_tests_numerics_and_public_04a08_evidence_preserved',
            'test_04a10_regression_projection.Regression10ProjectionTests.test_only_two_form_routes_change_and_native_tails_match',
            'test_04a10_regression_projection.Regression10ProjectionTests.test_core_udfs_frameworks_and_accepted_evidence_are_byte_protected',
            'test_04a10_regression_projection.Regression10ProjectionTests.test_baselines_are_actual_gaussian_and_binomial_and_not_goldens',
            'test_04a27_geometry_completion.Geometry27CompletionTests.test_initial_implementation_and_historical_tests_not_rewritten',
            'test_04a27_transport_successor.Transport27SuccessorTests.test_core_designer_and_refedit_sources_are_unmodified'}
        assert set(new_python)==allowed,(config,new_python)
        total=sum(int(n) for log in logs for n in re.findall(r'^Ran (\d+) tests? in',log,re.M))
        configs[config]=dict(managed_suites=suites,total=sum(int(s['counters']['total']) for s in suites),passed=sum(int(s['counters']['passed']) for s in suites),failed=sum(int(s['counters']['failed']) for s in suites),
            assembly_count_scope='Per-assembly distinct test-case executions; linked cross-target cases included. Focused/semantic repeats excluded.',
            logical_case_deduplication=dict(total=len(groups),passed=logical_pass,failed=logical_failed,skipped=logical_skipped,
                identity='Exact shared test-source SHA + class/method + data-row display with only MSTest quote presentation normalized, only when unambiguous within each suite. Unresolved/ambiguous runtime cases remain separate. Any failed execution keeps the logical case failed; no failure discarded.',
                cross_target_duplicate_groups=collisions),
            batch11_executions=sum(len(s['batch11']) for s in suites),new_managed_nonpasses=[r for s in suites for r in s['batch11'] if r['outcome']!='Passed'],superseded_frozen_managed=newly,
            python_total=total,python_passed=total-len(nonpasses),python_nonpasses=nonpasses,python_failure_events=events,python_inherited_count=len(inherited),new_python_source_pin_identities=new_python,
            python_inherited_outcome_changes={k:dict(before=v,after=nonpasses[k]) for k,v in inherited.items() if nonpasses[k]!=v},
            node_passed=sum(int(n) for n in re.findall(r'\bpass (\d+)\s*$',logs[0],re.M)),node_failed=sum(int(n) for n in re.findall(r'\bfail (\d+)\s*$',logs[0],re.M)),final_command=json.loads((RUN/(name+'-full-command.json')).read_bytes()))
        assert not configs[config]['new_managed_nonpasses']
    c.emit('validation-final.json',dict(base=c.BASE,supersedes='validation.json: retained original display-name accounting, before reconciling MSTest 2/3 String quoting against exact linked source hashes',configurations=configs,prior_validation_sha256=c.sha(ROOT/'evidence/04A10/validation.json'),native=False,P='NO',
        historical_nonpasses_never_rewritten=True,managed_supersession='Two runtime executions of the one frozen constant-binomial True case now fail because the authorized defensive report replaces its expected NullReferenceException. Both remain FAIL; new successor matrix passes.'))
    print(json.dumps({k:{n:v for n,v in d.items() if n in ['total','passed','failed','python_total','python_passed','node_passed','node_failed','batch11_executions']} for k,d in configs.items()},indent=2))
    print('Logical cases:',{k:{n:v for n,v in d['logical_case_deduplication'].items() if n in ['total','passed','failed','skipped']} for k,d in configs.items()})

def supplements():
    initial=json.loads((RUN/'baseline.json').read_bytes())['source_pins']
    pin_analysis={}
    for path in ['planning/BESHStatNG_CrossPlatform_MasterPlan.md','BESHStat.Core/src/RegModels/GLM.vb']:
        prior=json.loads((ROOT/'evidence/04A10/source-delta.json').read_bytes())['protected_source_pins'][path]
        pin_analysis[path]=dict(frozen_04A10=prior,live_entry=initial[path],current=c.sha(ROOT/path),entry_already_different=prior!=initial[path])
    byte_analysis=[]
    prior=json.loads((ROOT/'evidence/04A10/baseline-registry.json').read_bytes())['files']
    for path,record in prior.items():
        raw=(ROOT/path).read_bytes()
        byte_analysis.append(dict(path=path,frozen_receipt=record['sha256'],entry=initial[path],current=c.sha(ROOT/path),
            crlf_sha256=__import__('hashlib').sha256(raw.replace(b'\r\n',b'\n').replace(b'\n',b'\r\n')).hexdigest()))
    c.emit('frozen-pin-successor.json',dict(base=c.BASE,pins=pin_analysis,baseline_bytes=byte_analysis,
        entry_context='04A.10 commit includes owner Revision 2.7, while its frozen source receipt pinned Revision 2.5. Its new text baselines were committed with LF checkout normalization against CRLF raw capture receipts. Entry byte hashes are retained, not restored/rebaselined.',
        approved_implementation_drift='Core GLM reporting/cache guards and clone fix; exact estimator statement proof and all untouched sources are in source-delta.json. Four-family form changes supersede frozen whole-form expectations.',
        native_scientific_gates_unchanged=True))
    c.emit('diagnostic-reference-candidate.json',dict(status='NOT_GOLDEN',software='Python '+sys.version.split()[0]+' standard library',
        code='for y in ([0.0]*24, [1.0]*24): mean=sum(y)/len(y); variance=mean*(1-mean); observed_classes=len(set(y))',
        cases=[dict(response=value,rows=24,mean=value,bernoulli_variance_at_empirical_mean=value*(1-value),observed_classes=1) for value in [0.0,1.0]],
        interpretation='Independent diagnostic candidate identifying one observed response class and boundary empirical variance. It does not establish a valid finite MLE or a general HL p-value. Core absence of its post-fit payload is reported, never filled with a fabricated statistic.',
        provenance='Deterministic synthetic all-zero/all-one arrays; separately computed from Core. No R invocation, new coefficient reference, scientific golden promotion or independent external scientific signoff.'))
    source=json.loads((OUT/'source-delta-final.json').read_bytes());tests={p:c.sha(ROOT/p) for p in ['BESHStat.Excel.Common.Tests/src/Regression11DefectTests.vb','BESHStat.Excel.Common.Tests/src/Regression11PolicyTests.vb','BESHStatNG.Test/src/Regression11AdapterTests.vb']}
    cross=json.loads((OUT/'restriction-crosswalk.json').read_bytes());controls=[r['control_id'] for r in cross['controls']];obligations=[r['obligation_id'] for r in cross['controls'] if r['obligation_id']]
    common=dict(source_pins=source['changed_existing'],test_pins=tests,affected_control_ids=controls,retained_original_obligation_ids=obligations,original_row_hashes='restriction-crosswalk.json',native_executed=False,P='NO')
    c.emit('issues-final.json',dict(supersedes=['issues.json: before CLR compatibility overload','issues-v2.json: before failed-fit classification guard'],issues=[
        dict(id='C10-LM-ALPHA-01',status='PORTABLE_FIXED_NATIVE_PENDING',before='Requested .10; Core effective .05; 95% coefficient CIs.',after='Portable Requested intent supplies .10 before fitting: 90% CIs, same coefficients/SE/fitted vectors; default .05 still 95%. Shipping RunOLS explicitly keeps NativeLegacyDefault with actual .05 metadata.',shipping_status='UNAPPLIED_GATED',next_owners=['04A.40 GUI/UDF parity','04A.47 GUI confirmation'],**common),
        dict(id='C10-CORE-CONSTANT-01',status='FIXED_AND_ACTIVE',before='Constant-binomial early exit could dereference absent HL, covariance or residual payload; prechange captures record exact exception matrix.',after='HasReportData false yields a diagnostic-only GLM report and absent predictions/residuals. HL null guard, binomial lifecycle cache reset, no optimizer/formula changes or fabricated statistic. Completed nonconverged fits remain distinct.',shipping_status='Core defensive report active through existing calls; full UDF handle/inference-status reconciliation remains 04A.40.',next_owners=['04A.40 UDF failed-handle status','04A.47 native diagnostics verification'],**common),
        dict(id='C10-COV-NAMES-01',status='PORTABLE_FIXED_NATIVE_PENDING',before='Covariance wrap aliases names and changes response key to Intercept.',after='Clone before label formatting; repeated wraps and malformed-name tests safe; numbers/order unchanged. Native Data header held in explicit temporary compatibility quarantine, outside Core/Common.',shipping_status='UNAPPLIED_GATED Data-header activation; Core clone is active.',next_owners=['04A.28 output/formatting','04A.40 UDF parity','04A.47 GUI confirmation'],**common)],
        single_owner_timing_disposition='C11-NATIVE-CORRECTIONS-01',strict_validation_activation='C09-NATIVE-STRICT-01 remains deferred and UNAPPLIED.'))
    c.emit('documentation-impact.json',dict(pages=[dict(page='BESHStatNG_Help_MkDocs/docs/methods/'+p+'.md',before_sha256=initial['BESHStatNG_Help_MkDocs/docs/methods/'+p+'.md'],after_sha256=c.sha(ROOT/'BESHStatNG_Help_MkDocs/docs/methods'/(p+'.md')),
        change='Truthful Office availability and unchanged behavior' if p in ['zero-inflated-poisson-regression','ordinal-logistic-regression','multinomial-logistic-regression'] else 'Current alpha/output-label limitations or active unavailable-fit diagnostics documented; native activations explicitly pending.',
        Windows='Existing Excel-DNA route; no native screenshot verification.',Office='Regression task-pane method not executable; no fictitious Office run instructions added.',screenshots='Existing assets unchanged; new/revalidated native screenshots NOT RUN, pending 04A.47.') for p in c.PAGES],
        metadata='Method IDs/slugs, authored methods.yml, generated metadata, resolver and navigation identities unchanged.',
        strict_build=json.loads((RUN/'docs-build-strict-command.json').read_bytes()),help_link_checks='Original catalog/metadata/Help resolver/link checks rerun by both full verifier configurations; see command-outcomes.json.',deferred=['04A.28 labels/formatting','04A.40 UDF parity','04A.47 native screenshot/GUI checks']))

def fixtures():
    def cell(line):
        if line=='NULL':return None
        kind,value=line.split(':',1)
        return value if kind=='D' else base64.b64decode(value).decode('utf-8')
    cases={}
    for p in sorted((OUT/'baselines').glob('*.txt')):
        if p.name.startswith('defects-'):continue
        lines=p.read_text(encoding='utf-8').splitlines();i=3;sections=[];state=lines[2]
        while lines[i].startswith('TABLE '):
            metadata=lines[i];rows,cols=map(int,lines[i+1].split());values=[cell(s) for s in lines[i+2:i+2+rows*cols]]
            matrix=[values[r*cols:(r+1)*cols] for r in range(rows)]
            for row in matrix:
                for j in range(len(row)-1):
                    if row[j]=='Converged?' and row[j+1] in ['True','False']:state=row[j+1]
            sections.append(dict(metadata=metadata,first_rows=matrix[:4]));i+=2+rows*cols
        assert lines[i]=='FITTED';i+=1
        if lines[i]=='RESIDUAL':shape=None;first=[]
        else:
            shape=list(map(int,lines[i].split()));i+=1;first=[cell(s) for s in lines[i:i+min(3,shape[0])*shape[1]]]
        cases[p.name]=dict(sha256=c.sha(p),source_row_count=len(lines[1].split(',')),table_count=len(sections),sections=sections,
            convergence_from_Core=state,fitted_shape=shape,fitted_first_values=first)
    c.emit('numerical-comparisons.json',dict(status='NOT_GOLDEN',cases=cases,
        interpretation='Readable excerpts only. Complete typed raw cells, row identities and residual vectors remain in the pinned captures. Passing Regression11 pipeline comparisons use exact strings/types/metadata/order within runtime and established Double tolerances.',
        inputs={p.name:c.sha(p) for p in sorted(OUT.glob('input-projection*.txt'))},
        culture='Invariant and sk-SK: frozen numeric cells and exact same-culture projection compared, without numeric localization/reparse.',native=False,P='NO'))

def commands():
    records=[]
    for p in sorted(RUN.glob('*-command.json')):records.append(dict(path=p.relative_to(ROOT).as_posix(),raw_sha256=c.sha(p),record=json.loads(p.read_bytes())))
    for p in sorted((RUN/'executions').glob('*/execution.json')):
        data=json.loads(p.read_bytes());records.append(dict(path=p.relative_to(ROOT).as_posix(),raw_sha256=c.sha(p),record={k:data[k] for k in ['command','exit_code','seconds']}))
    copies=[dict(path=p.relative_to(ROOT).as_posix(),raw_sha256=c.sha(p),record=json.loads(p.read_bytes())) for p in sorted(RUN.glob('*-verify-isolated.provenance.json'))]
    c.emit('command-outcomes-final.json',dict(supersedes='command-outcomes.json: retained before the supplemental numerical-delta probe',records=records,execution_copies=copies,public_derivative=True,
        deviation='Literal verify-beshstatng.ps1 not run: its fixed historical TestResults/Codex writers and restore/Excel-DNA packaging hooks conflict with retention/offline/native restrictions. Same suite sequence via audited source-hashed execution copies, fresh output paths, --no-restore, safe compile targets, independent continuation after inherited failures. Original script unchanged.'))
    checks={}
    for name in ['historical-strict','historical-profile']:
        p=RUN/(name+'.log');checks[name]=dict(command=json.loads((RUN/(name+'-command.json')).read_bytes()),raw_log_sha256=c.sha(p),output=p.read_text(encoding='utf-8',errors='replace'))
    c.emit('historical-checkers.json',checks)

def closeout():
    c.source()
    p=RUN/'preservation-final.json';c.emit('preservation.json',dict(raw_receipt_sha256=c.sha(p),receipt=json.loads(p.read_bytes()),scope='Historical membership/size/timestamps/file identity and frozen source hashes; not a full historical TestResults byte audit.'))
    paths=c.git('diff','--name-only').splitlines()+c.git('ls-files','--others','--exclude-standard').splitlines()
    assert not c.git('diff','--cached','--name-only')
    files={};privacy=[]
    for name in sorted(set(paths)):
        if name=='evidence/04A11/publication-manifest-final.json':continue
        p=ROOT/name;assert p.is_file()
        code=subprocess.run(['git','-c','safe.directory='+ROOT.as_posix(),'check-ignore','-q',name],cwd=ROOT).returncode;assert code==1,name
        content=p.read_text(encoding='utf-8-sig',errors='replace')
        if name.startswith('evidence/04A11/baselines/'):
            content+='\n'+'\n'.join(base64.b64decode(line.split(':',1)[1]).decode('utf-8') for line in content.splitlines() if line.startswith('String:'))
        if re.search(r'(?i)[A-Z]:[\\/]Users[\\/]|S-1-5-21-(?:\d+-){2,}\d+|BEGIN (?:RSA |EC |OPENSSH )?PRIVATE KEY',content):privacy.append(name)
        files[name]=dict(sha256=c.sha(p),bytes=p.stat().st_size,git_ignore_exit=code)
    assert not privacy,privacy
    c.emit('publication-manifest-final.json',dict(base=c.BASE,supersedes='publication-manifest.json: earlier proposal snapshot before source-backed MSTest accounting reconciliation',head=c.git('rev-parse','HEAD'),branch=c.git('branch','--show-current'),index_empty=True,files=files,manifest_self_excluded=True,
        privacy_scan=dict(findings=privacy,scope='All proposal text and all decoded baseline String scalars: common local absolute owner paths, complete SIDs and private-key signatures. Not a general secret-certification.'),
        excluded='All ignored TestResults, historical/local-only traces, caches/build outputs and candidate compiled assemblies remain local and in place.',publication='No Git stage/commit/merge/push, deployment, installer, signing, Excel/COM/UIA or 04A.12 action.'))

def review():
    value=json.loads((OUT/'validation-final.json').read_bytes());a=value['configurations']['Debug'];b=value['configurations']['Release']
    assert a['python_nonpasses']==b['python_nonpasses']
    assert a['python_inherited_outcome_changes']==b['python_inherited_outcome_changes']=={}
    receipt=json.loads((RUN/'preservation-final.json').read_bytes());assert receipt['status']=='PASS' and not receipt['errors']
    source=json.loads((OUT/'source-delta-final.json').read_bytes())
    patch=json.loads((OUT/'native-patch-final.json').read_bytes())
    text='\n\n## Final validation and complete failure ledger\n\n'
    text+='P/F/S/T below means passed / failed / skipped / total. Managed suite rows are distinct case executions within that assembly. The logical row merges cross-target cases only with an exact shared source hash and unambiguous class/method/data-row identity, normalizing the MSTest 2/3 difference in String quoting. Unresolved/ambiguous runtime cases remain separate. Any failed execution keeps the logical case failed; no failure is discarded. The full collision ledger is in validation-final.json. Focused/semantic repeats and extra CJS/probe runs are excluded.\n\n'
    text+='| Suite | Debug P/F/S/T | Release P/F/S/T |\n|---|---:|---:|\n'
    names=['Core','Office managed','WASM execution managed','Common','Windows .NET Framework 4.8','Retained control projection']
    for i in [0,3,4,1,2,5]:
        fields=[]
        for conf in [a,b]:
            n=conf['managed_suites'][i]['counters'];fields.append(f"{int(n['passed']):,} / {int(n['failed']):,} / {int(n.get('notExecuted',0)):,} / {int(n['total']):,}")
        text+='| '+names[i]+' | '+' | '.join(fields)+' |\n'
    for label,key in [('**Managed logical cases (deduplicated)**','logical_case_deduplication')]:
        fields=[]
        for conf in [a,b]:
            n=conf[key];fields.append(f"**{n['passed']:,} / {n['failed']:,} / {n['skipped']:,} / {n['total']:,}**")
        text+='| '+label+' | '+' | '.join(fields)+' |\n'
    text+=f"| Managed runtime executions (historical comparison only) | {a['passed']:,} / {a['failed']:,} / 0 / {a['total']:,} | {b['passed']:,} / {b['failed']:,} / 0 / {b['total']:,} |\n"
    text+=f"| Node | {a['node_passed']} / {a['node_failed']} / 0 / 916 | {b['node_passed']} / {b['node_failed']} / 0 / 916 |\n"
    text+=f"| Python distinct methods | {a['python_passed']} / 61 nonpasses / 0 / {a['python_total']} | {b['python_passed']} / 61 nonpasses / 0 / {b['python_total']} |\n\n"
    text+='Python nonpasses split into **52 FAIL and 9 ERROR** per configuration (65 events including subtests). There are no skips. All **67 new logical managed cases** pass, comprising **55 Common and 67 Windows executions** (122 across runtimes); all nine new Python source checks pass. The frozen constant-binomial True case adds one logical failed case/two runtime failures because it still expects NullReferenceException. No old test, fixture or expected value was edited. No unexplained new functional failure remains.\n\n'
    text+='Both complete verifier copies and both affected Common/Windows refreshes exit **1** for retained nonpasses. Core/Office/WASM tests, both modern host builds, safe Windows builds, all Node suites, candidate compilation, strict MkDocs, source recheck and the real-Core numerical probe exit **0**. The historical strict checker exits **1** at its old UibyID/NormalPlot stale review dependency; the profile checker exits **1** with INTEGRITY_FAILURE and an empty detail string. The actual raw outputs and hashes are preserved, not replaced by older checker totals.\n\n'
    text+='The full Python runs precede only the narrowly documented ABI/failure-classification completion. Current source checks were rerun after those changes; their pass outcomes remain unchanged. Current Common/Windows suite results come from final-compat-Debug/Release. Original full-run results, the API-only refresh and current results remain separately retained.\n\n'
    text+='### Exact managed nonpassing executions\n\n| Class / exact case | Outcome | Attribution |\n|---|---|---|\n'
    for suite in a['managed_suites']:
        for row in suite['nonpasses']:
            reason='Inherited 04A.10 identity/outcome'
            if row['method']=='ConstantAndSeparatedBinomialPreserveActualFitWarnings':reason='Frozen old broken-behavior expectation, superseded by active Core defensive report; remains FAIL'
            text+=f"| `{row['class_name']}.{row['test_name']}` | {row['outcome']} | {reason} |\n"
    text+='\n### Exact Python nonpassing methods\n\nAll original **54** identities and outcomes remain unchanged. Three additional identities were already stale at the live merged entry (Revision 2.7 versus the old source receipt, plus LF/CRLF capture receipt normalization). Four more are scope/hash guards triggered by the explicitly authorized Core report changes. The current source receipt proves all estimator statements and unrelated sources remain protected. These are nonpasses, never relabelled as green.\n\n| Outcome | Exact method identity | Attribution |\n|---|---|---|\n'
    for identity,outcome in sorted(a['python_nonpasses'].items()):
        reason='Inherited reported 04A.10 nonpass'
        if identity in a['new_python_source_pin_identities']:
            reason='Live-entry normalization/source-receipt drift, plus current approved source changes' if identity.startswith('test_04a10_') else 'Approved Core change triggers frozen source/scope guard; successor proof retained'
        text+=f'| {outcome} | `{identity}` | {reason} |\n'
    text+='\n### Preservation, publication and acceptance checklist\n\n'
    text+=f"Preservation **PASS**: **{receipt['historical_files']:,} historical files**, zero membership/metadata/source-pin errors and **{receipt['disk_free_bytes']:,} bytes free** at the receipt. Checks cover original path membership, size, creation/write nanosecond timestamps, file/device identity and frozen source hashes; they are not a full byte rehash of historical TestResults. All **{source['protected_count']:,} protected entry source/document files**, including 76 prior public evidence files and 842 architecture/planning files, retain their entry bytes.\n\n"
    text+=f"Current form SHA-256 `{patch['source_sha256']}`; current dormant patch SHA-256 `{patch['patch_sha256']}`. Both that patch and C09 strict activation remain UNAPPLIED. `git diff --check` and empty-index checks pass. Publication candidates remain non-ignored and privacy-scanned; raw owner-local evidence is separately ignored and retained.\n\n"
    text+='| Required result | Disposition |\n|---|---|\n| Verified merged base/new branch and protected history | MET; local main/HEAD 463aaa12a6342cedfa084e266d5a985461de9d3a, no Git publication |\n| Four substantive family intents/results consumed by shipping writers | MET; exact Core input captures, Core-backed parity and executable net48 adapter/writer tests |\n| Three correction investigations and permitted implementations | MET for authorized portable/Core work; two Windows activations remain explicitly gated |\n| Same-batch Help/Office availability/documentation | MET; six existing pages, strict build/Help checks pass; native screenshots NOT RUN |\n| Focused/full/culture/negative verification and honest failure ledger | MET; all new cases pass; inherited/superseded/source-pin failures retained |\n| Original native/scientific restrictions and IDs/hashes | MET as restrictions retained; zero release/discharge; P NO / N1 NO GO / N2 NO / C2 UNMET |\n| Owner acceptance boundary | PENDING; accept software with the single C11-NATIVE-CORRECTIONS-01 timing deferral; no 04A.12 authorization |\n'
    path=ROOT/'docs/architecture/batch-04A-11-owner-review.md';body=path.read_text(encoding='utf-8')
    marker='\n\n## Final validation and complete failure ledger'
    if marker in body:body=body[:body.index(marker)]
    path.write_text(body+text,encoding='utf-8',newline='\n')

if __name__=='__main__':
    p=argparse.ArgumentParser();p.add_argument('action',choices=['validation','supplements','fixtures','commands','closeout','review']);args=p.parse_args();globals()[args.action]()
