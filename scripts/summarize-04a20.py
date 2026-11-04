"""04A.20 actual receipt accounting and source/authority checks; never rewrite history."""
import argparse
import base64
import csv
from collections import Counter
import hashlib
import importlib.util
import json
from pathlib import Path
import re
import platform
import subprocess
import sys
import xml.etree.ElementTree as ET

ROOT = Path(__file__).resolve().parents[1]
def module(name, path):
    s = importlib.util.spec_from_file_location(name, ROOT/path)
    m = importlib.util.module_from_spec(s); s.loader.exec_module(m); return m

c = module('capture20', 'scripts/capture-04a20-contract.py')
old = module('summary18', 'scripts/summarize-04a18.py')
RUN, OUT = c.RUN, c.OUT
old.RUN = RUN
NS = old.NS
sanitize = old.c.prior.prior.sanitize

def write(name, value):
    def placeholders(item):
        if isinstance(item,dict): return {k:placeholders(v) for k,v in item.items()}
        if isinstance(item,(list,tuple)): return [placeholders(v) for v in item]
        if isinstance(item,str): return item.replace('<REPO>','<REPO_ROOT>')
        return item
    value=placeholders(sanitize(value))
    if name=='fa-da-same-core-comparisons-v1.json':
        (OUT/name).write_text(json.dumps(value,ensure_ascii=False,separators=(',',':'),allow_nan=False)+'\n',encoding='utf-8',newline='\n')
    else:
        c.emit(name, value)

def measured(name, config):
    suites = old.suites_for(name, config)
    if name.startswith('final-'):
        for index,suite in [(3,'Common'),(4,'Legacy')]:
            suites[index]=old.old.trx(RUN/'executions'/('final-source-'+config+'-'+suite)/'results'/(suite+'.trx'))
    groups = old.previous.typed_groups(suites)
    weight = old.prior.weight
    new = {k: rows for k, rows in groups.items() if any('Multivariate20' in r['class_name'] for r in rows)}
    logical = dict(total=sum(weight(k) for k in groups), passed=sum(weight(k) for k,rows in groups.items() if all(r['outcome']=='Passed' for r in rows)))
    logical['nonpassed'] = logical['total']-logical['passed']
    for suite in suites:
        suite['batch20_total'] = sum('Multivariate20' in r['class_name'] for r in suite['records'])
        suite['batch20_passed'] = sum('Multivariate20' in r['class_name'] and r['outcome']=='Passed' for r in suite['records'])
        suite.pop('records'); suite.pop('batch11',None)
    python_logs = [RUN/(name+'-full.log'),RUN/(name+'-common-python.log')]
    py = old.previous.python(python_logs)
    log = (RUN/(name+'-full.log')).read_text(encoding='utf-8',errors='replace')
    return dict(managed_suites=suites, logical_cases=logical,
        runtime_executions=dict(total=sum(int(s['counters']['total']) for s in suites),passed=sum(int(s['counters']['passed']) for s in suites),failed=sum(int(s['counters']['failed']) for s in suites)),
        new_logical_total=sum(weight(k) for k in new),new_logical_passed=sum(weight(k) for k,rows in new.items() if all(r['outcome']=='Passed' for r in rows)),
        node_passed=sum(map(int,re.findall(r'\bpass (\d+)\s*$',log,re.M))),node_total=sum(map(int,re.findall(r'\btests (\d+)\s*$',log,re.M))),
        python=py,python_receipts=[p.relative_to(RUN).as_posix() for p in python_logs],full_command_exit=json.loads((RUN/(name+'-full-command.json')).read_bytes())['exit_code'])

def entry():
    configs = {v:measured('entry-'+v,v) for v in ['Debug','Release']}
    write('entry-validation.json',dict(base=c.BASE,configurations=configs,scope='Actual complete prechange source in both configurations; all failures retained. Logical and raw executions distinct.'))
    for v,d in configs.items(): print('entry',v,d['logical_cases'],d['runtime_executions'],'Node',d['node_passed'],d['node_total'],'Python',d['python']['passed'],d['python']['total'])

def validation():
    initial = json.loads((OUT/'entry-validation.json').read_bytes())['configurations']
    configs = {}
    identity = lambda r:(r['class_name'].split('.')[-1],r['method'],r['test_name'].replace('"',''))
    for v in ['Debug','Release']:
        d = measured('final-'+v,v)
        before={identity(r) for s in initial[v]['managed_suites'] for r in s['nonpasses']}
        after={identity(r) for s in d['managed_suites'] for r in s['nonpasses']}
        d['new_managed_nonpasses']=sorted(after-before)
        d['removed_managed_nonpasses']=sorted(before-after)
        d['python_new_since_entry']={p:r for p,r in d['python']['nonpasses'].items() if p not in initial[v]['python']['nonpasses']}
        d['python_entry_now_passing']=sorted(set(initial[v]['python']['nonpasses'])-set(d['python']['nonpasses']))
        configs[v]=d
        print('final',v,d['logical_cases'],d['runtime_executions'],'new',d['new_logical_passed'],d['new_logical_total'],'Node',d['node_passed'],d['node_total'],'Python',d['python']['passed'],d['python']['total'],'new failures',d['new_managed_nonpasses'],list(d['python_new_since_entry']))
    write('validation.json',dict(configurations=configs,accounting='Accepted source-hash/class/method/data-row deduplication and historical null/empty cohort weights; repeated focused and semantic runs excluded.',native='NOT_RUN / UNAUTHORIZED',independent_R='NOT_RUN / UNAUTHORIZED',golden_promotion=False))
    nonpasses = {v:dict(managed=[dict(r,suite=s['path']) for s in d['managed_suites'] for r in s['nonpasses']],python=d['python']['nonpasses'],new_managed=d['new_managed_nonpasses'],new_python=d['python_new_since_entry']) for v,d in configs.items()}
    prior_reasons={r['identity']:r.get('root_cause','') for r in json.loads((ROOT/'evidence/04A18/nonpasses.json').read_bytes())['python']}
    before=old.traces(['entry-Debug','entry-Release'])
    after=old.traces(['final-Debug','final-Release'])
    details=[]
    for identity,outcome in configs['Debug']['python']['nonpasses'].items():
        inherited=identity in initial['Debug']['python']['nonpasses']
        reason=prior_reasons.get(identity,'')
        if not inherited:
            reason='Newly triggered frozen source-layout/hash assertion: authorized FA/DA extraction changes form/adapter source ownership; prior assertion remains a genuine failure. Exact terminal and raw receipt hashes retained.'
        if not reason:
            reason='Inherited current-main branch/source-publication pin or prior assertion/error; see exact entry/final terminals. Not rewritten or treated as passed.'
        details.append(dict(identity=identity,outcome=outcome,inherited_at_entry=inherited,root_cause=reason,entry_evidence=before.get(identity,[]),final_evidence=after.get(identity,[])))
    messages=[]
    for v in ['Debug','Release']:
        for suite in configs[v]['managed_suites']:
            for node in ET.parse(ROOT/suite['path']).getroot().findall('.//'+NS+'UnitTestResult'):
                if node.get('outcome')=='Passed': continue
                message=node.find('.//'+NS+'ErrorInfo/'+NS+'Message')
                messages.append(dict(configuration=v,test_name=node.get('testName'),suite=suite['path'],message=message.text if message is not None else '',inherited_at_entry=True))
    write('nonpasses.json',dict(configurations=nonpasses,python_details=details,managed_messages=messages,all_nonzero_statuses_retained=True,prior_register_sha256=c.sha(ROOT/'evidence/04A18/nonpasses.json'),strict='Inherited stale UibyID/NormalPlot dependency; exit 1',profile='Inherited INTEGRITY_FAILURE; exit 1'))

def numerical(prefix='final'):
    records=[]; payloads={}
    for config in ['Debug','Release']:
        for tfm,suite in [('net10.0','Common'),('net48','Legacy')]:
            path=RUN/'executions'/(prefix+'-'+config+'-'+suite)/'results'/(suite+'.trx')
            if not path.exists(): continue
            receipt_sha=c.sha(path)
            for node in ET.parse(path).getroot().findall('.//'+NS+'UnitTestResult'):
                for stdout in node.findall('.//'+NS+'StdOut'):
                    for line in (stdout.text or '').splitlines():
                        if not line.startswith('04A20_XML|'): continue
                        data=sanitize(old.xml_value(ET.fromstring(base64.b64decode(line.split('|',1)[1]))))
                        key=hashlib.sha256(json.dumps(data,sort_keys=True,ensure_ascii=False,separators=(',',':'),allow_nan=False).encode()).hexdigest()
                        payloads.setdefault(key,data)
                        records.append(dict(profile_id=key,configuration=config,tfm=tfm,os='Windows',test_identity=node.attrib['testName'],test_outcome=node.attrib['outcome'],raw_trx_alias='LOCAL_TRX:'+prefix+'-'+config+'-'+suite,raw_trx_sha256=receipt_sha))
    assert records,'No executed profiles'
    write('fa-da-same-core-comparisons-v1.json',dict(version=1,status='NOT_GOLDEN',validation='SAME_CORE_COMPATIBILITY_ONLY',records=records,profile_payloads=payloads,record_count=len(records),payload_count=len(payloads),outcomes=dict(Counter(payloads[r['profile_id']]['classification'] for r in records)),
        payload_identity='SHA-256 of canonical public JSON: sorted keys, compact separators, UTF-8, ensure_ascii=False. Raw XML/TRX hashes are distinct.',
        tolerance='Exact discrete labels/order/types/nonfinite classes; exact same-runtime numerical array digests, table doubles max(1e-9,abs(expected)*1e-9). No relabelling or cross-runtime scientific oracle.',
        provenance='FA stable fixture follows the source-owned Core test generator; DA synthetic unequal/case-distinct classes and edge variants are new source-backed cases. Fixed validation seeds compare exact direct/request Core outputs. Unspecified seed is an observation only: public Core does not expose its resolved seed; no fabricated seed or independent split equality.',
        fixture_authorities={p:dict(sha256=c.sha(ROOT/p),use=role) for p,role in [
            ('BESHStat.Core.Tests/src/Multivariate/FactorAnalysisCoreTests.vb','Read-only stable factor fixture provenance'),
            ('BESHStat.Core.Tests/src/Multivariate/DiscriminantAnalysisCoreTests.vb','Read-only reference and validation case provenance; existing tests retained'),
            ('BESHStatNG_Help_MkDocs/docs/assets/data/011xyzscatterplot/011xyzscatterplot_gif.csv','Documented public protein example; provenance only, screenshots and claims not retested'),
            ('BESHStat.Excel.Common.Tests/src/Multivariate20Tests.vb','Executed fixture generator and compatibility checks'),
            ('evidence/04A17/multivariate-comparisons-v1.json','Frozen prior same-Core profiles; not regenerated')]},
        native='NOT_RUN / UNAUTHORIZED',independent_R='NOT_RUN / UNAUTHORIZED',promoted=False))
    print('Profiles',len(records),'payloads',len(payloads))

def source():
    assert c.git('rev-parse','HEAD')==c.git('rev-parse','main')==c.BASE
    assert c.git('branch','--show-current')=='codex/04A-20'
    assert not c.git('diff','--cached','--name-only')
    changes=set(c.git('diff','--name-only').splitlines())
    allowed=set(json.loads((OUT/'implementation-plan.json').read_bytes())['modified'])
    assert changes<=allowed,changes-allowed
    pins=json.loads((RUN/'baseline.json').read_bytes())['source_pins']
    for p,h in pins.items():
        if p not in changes: assert c.sha(ROOT/p)==h,p
    for p,r in json.loads((OUT/'frozen-capture-hashes.json').read_bytes())['files'].items():
        assert c.sha(ROOT/p)==r['sha256'] and (ROOT/p).stat().st_size==r['bytes'],p
    form='BESHstatNG/src/UI/Ui11PCA.vb'
    oldmethods=old.c.members(form,(RUN/'preimages/Ui11PCA.vb').read_text(encoding='utf-8-sig'))
    newmethods=old.c.members(form,(ROOT/form).read_text(encoding='utf-8-sig'))
    changed={n for n,t in oldmethods.items() if newmethods[n]!=t}
    assert changed=={'RunFA','RunDiscriminantAnalysis'},changed
    write('source-preservation.json',dict(base=c.BASE,unchanged_entry_tracked_files=len(pins)-len(changes),changed={p:dict(before=pins[p],after=c.sha(ROOT/p),bytes=(ROOT/p).stat().st_size) for p in sorted(changes)},changed_form_methods=sorted(changed),changed_method_hashes={n:dict(before=hashlib.sha256(oldmethods[n].encode()).hexdigest(),after=hashlib.sha256(newmethods[n].encode()).hexdigest()) for n in sorted(changed)},unchanged_form_methods=sorted(set(oldmethods)-changed),Core_numerics_unchanged=True,native_renderers_writers_importers_RefEdit_UDF_Designer_metadata_targets_unchanged=True,index_empty=True,private_baseline=dict(alias='LOCAL_BASELINE:all-tracked-entry',sha256=c.sha(RUN/'baseline.json'))))
    registry=json.loads((ROOT/'evidence/04A14/baseline-registry.json').read_bytes())
    assert len(registry['files'])==100
    for p,r in registry['files'].items(): assert c.sha(ROOT/p)==r['sha256'],p
    print('PASS source/frozen history guards;',len(changed),'changed form methods')

def commands():
    records=[]
    for p in sorted(RUN.glob('*-command.json')):
        d=json.loads(p.read_bytes()); records.append(dict(alias='LOCAL_COMMAND:'+p.stem,raw_sha256=c.sha(p),**{k:d[k] for k in ['command','exit_code','seconds']}))
    for p in sorted((RUN/'executions').glob('*/execution.json')):
        d=json.loads(p.read_bytes()); records.append(dict(alias='LOCAL_EXECUTION:'+p.parent.name,raw_sha256=c.sha(p),**{k:d[k] for k in ['command','exit_code','seconds']}))
    write('command-outcomes.json',dict(records=records,hash_policy='Raw local receipt hashes; sanitized command derivatives are not byte-identical raw evidence.'))
    write('execution-adaptations.json',dict(records=[dict(alias=p.stem,raw_sha256=c.sha(p),**json.loads(p.read_bytes())) for p in sorted(RUN.glob('*-verify-isolated.provenance.json'))],reason='Original verifier would overwrite TestResults/Codex and invoke restore/packaging. Exact-anchor derivative uses no-restore and isolated results/assemblies, compiles real Windows host without installers/signing, continues independent suites and retains all nonzero exits.'))

def authority():
    authority = old.c.prior.prior
    rows=list(csv.DictReader((ROOT/authority.CONTROLS).read_text(encoding='utf-8-sig').splitlines()))
    ledger=json.loads((ROOT/authority.LEDGER).read_bytes())
    selected=[r for r in rows if Path(r['path']).name in ['Ui11PCA.Designer.vb','Ui0OneRefeditMulticol.Designer.vb']]
    ids={r['control_id'] for r in selected}
    obligations=[r for r in ledger if set(r.get('linked_control_ids',[])) & ids]
    previous=json.loads((ROOT/'evidence/04A18/original-control-crosswalk.json').read_bytes())
    assert ids=={r['control_id'] for r in previous['controls']}
    assert {r['obligation_id'] for r in obligations}=={r['obligation_id'] for r in previous['obligations']}
    populations={}
    for name in ['Ui11PCA','Ui0OneRefeditMulticol']:
        group=[r for r in selected if Path(r['path']).name==name+'.Designer.vb']
        groupids={r['control_id'] for r in group}
        designer=(ROOT/group[0]['path']).read_text(encoding='utf-8-sig')
        declarations=set(re.findall(r'Friend WithEvents (\w+) As ',designer))
        assert declarations=={r['control_id'].split('.')[-1] for r in group},name
        restricted={r['obligation_id'][len('control:'):] for r in ledger if r['obligation_id'].startswith('control:') and r.get('affected_live_golden_blocked')}
        populations[name]=dict(controls=len(groupids),linked_open_obligations=sum(bool(groupids & set(r.get('linked_control_ids',[]))) for r in obligations),restricted_original_controls=len(groupids & restricted))
    lineage=json.loads((ROOT/'evidence/04A18/native-lineage-successor.json').read_bytes())
    for p,pin in lineage['authorities'].items(): assert c.sha(ROOT/p)==pin['sha256'],p
    all_controls={r['control_id'] for r in rows};all_obligations={r['obligation_id'] for r in ledger}
    mixed={r['control_id'] for r in rows if Path(r['path']).name in ['Ui18MMRM.Designer.vb','Ui19LMM.Designer.vb']}
    broader=dict(controls=len(all_controls),obligations=len(all_obligations),restricted_original_controls=len(restricted),control_id_union_sha256=hashlib.sha256(json.dumps(sorted(all_controls),separators=(',',':')).encode()).hexdigest(),obligation_id_union_sha256=hashlib.sha256(json.dumps(sorted(all_obligations),separators=(',',':')).encode()).hexdigest())
    mixed_direct={r['obligation_id'] for r in ledger if r['obligation_id'].startswith('control:') and r['obligation_id'][len('control:'):] in mixed}
    mixed_population=dict(controls=len(mixed),direct_control_obligations=len(mixed_direct),linked_open_obligations=sum(bool(mixed & set(r.get('linked_control_ids',[]))) for r in ledger),restricted_original_controls=len(mixed & restricted),direct_obligation_ids=sorted(mixed_direct),interpretation='Original 177/69/67 lineage counts direct control obligations. The broader linked graph also includes edge obligations; these overlapping populations must not be conflated or added.')
    write('original-control-crosswalk.json',dict(authorities={p:dict(sha256=c.sha(ROOT/p),bytes=(ROOT/p).stat().st_size) for p in [authority.CONTROLS,authority.LEDGER]},controls=[dict(control_id=r['control_id'],path=r['path'],source_row_sha256=authority.digest(r)) for r in selected],obligations=[dict(obligation_id=r['obligation_id'],linked_control_ids=r.get('linked_control_ids',[]),original_row_sha256=authority.digest(r),status='OPEN') for r in obligations],populations=populations,broader_union=broader,mixed_model_population=mixed_population,historical_case=lineage['case'],inherited_native_lineage_sha256=c.sha(ROOT/'evidence/04A18/native-lineage-successor.json'),overlap='Populations overlap and must not be summed.',releases=0,discharges=0,P='NO',N1='NO GO',N2='NO',C2='UNMET',native='NOT_RUN / UNAUTHORIZED'))
    print('Authority populations',populations,'OPEN linked',len(obligations))

def documentation():
    pins=json.loads((RUN/'baseline.json').read_bytes())['source_pins']
    pages=['BESHStatNG_Help_MkDocs/docs/methods/'+v+'.md' for v in ['factor-analysis','discriminant-analysis']]
    metadata='BESHStatNG_Help_MkDocs/docs/_meta/methods.yml'
    udf='BESHStatNG_Help_MkDocs/docs/udf/multivariate-analysis.md'
    assert c.sha(ROOT/metadata)==pins[metadata] and c.sha(ROOT/udf)==pins[udf]
    docs_receipts={name:dict(alias='LOCAL_COMMAND:'+name,raw_sha256=c.sha(RUN/(name+'-command.json')),exit_code=json.loads((RUN/(name+'-command.json')).read_bytes())['exit_code']) for name in ['entry-mkdocs','final-mkdocs']}
    assert all(r['exit_code']==0 for r in docs_receipts.values())
    write('documentation-audit.json',dict(pages={p:dict(entry_sha256=pins[p],final_sha256=c.sha(ROOT/p),bytes=(ROOT/p).stat().st_size,amendments='Narrow FA/DA Windows axis, convergence, retained dimensions, priors and validation clarifications; truthful Office/Mac unavailability; screenshots and numerical examples unchanged.') for p in pages},metadata_sha256=pins[metadata],udf_page_sha256=pins[udf],metadata_changed=False,screenshots_changed=False,strict_mkdocs='Final Help bytes pinned here; both strict executions completed.',receipts=docs_receipts))
    write('factor-discriminant-requests-v1.json',dict(version=1,status='NOT_GOLDEN / SAME_CORE_COMPATIBILITY_ONLY',FA='Core dataInputs -> copied FactorOptions.Configure -> one Calculate; all UI parsing/defaults/disabled control readings remain Windows-owned.',DA='Core dataInputs after host group-first CDbl conversion -> request.ConfigureModel -> conditional original ParsePriors -> request.ApplyUserPriors delegates priorInputs before snapshot -> later validation snapshot.Configure -> one Fit.',prior_provenance='Ordered raw immutable scalar labels and weights copied only after Core priorInputs; no prior normalization or second parser in Common.',validation_seed='Blank Integer.MinValue distinct from explicit zero; public Core exposes no effective validation seed. Unspecified-seed observation has no exact independent split replay claim.',first_row='Post-import metadata only; ckFirstRow skip remains in unchanged GetDiscriminantData/ExcelDnaDataImporter.',data=dict(FA='Data: OriginalRow physical RowIds, no RowLabel; original cells/order, header1/body2.',DA='Data: optional Row label then original group/predictor cells; no extra RowId column.'),native='NOT_RUN / UNAUTHORIZED'))
    write('factor-discriminant-projections-v1.json',dict(version=1,FA='Actual AnalysisRowIds and unique source/input mapping; ambiguous duplicate IDs are diagnostic-only. Actual K, eigen axis, P x K unrotated/pattern/structure/score coefficients, N_active x K scores, K x K Phi/rotation and P x P model matrices; Core convergence/iterations retained.',DA='Input ordinal vs physical source RowId; active/removed/training/validation populations; first-observed Core classes; posterior/score/distance columns, observed confusion rows vs predicted columns. Core counts/totals/recall/precision/accuracy copied, never recomputed.',sections='Actual wrapResults -> existing ReportPolicy/AnalysisResultDocument -> ordered ModelSection indices -> LegacyTables -> native existing ProcessListofResultTables.',sheets=['Data','Factor analysis results','Discriminant results'],plots=dict(FA=['Scree','Loadings2D','Loadings3D'],DA=[]),native_arguments='Same existing FactorAnalysisPlotExcel methods with the same fitted Core object, in original order and after results; no new dimensional filtering, chart options or geometry.',row_issue='C20-DA-ROWID-DISPLAY-01: Core casewise Row retains local input ordinal; no visible remapping activated.',numerical_profiles='fa-da-same-core-comparisons-v1.json profile_id/test/receipt links carry actual shape/section/identity and exception/convergence observations.',raw_ownership='No raw N x P matrix or mutable Core model stored in projections. Immediate writer borrowing only; copied lightweight axes and G x G confusion metadata.',native='NOT_RUN / UNAUTHORIZED'))
    write('candidate-issues.json',dict(issues=[
        dict(id='C20-DA-ROWID-DISPLAY-01',status='OPEN / DORMANT / UNAPPLIED',reproduction='DiscriminantSourceOrdinalsConfusionAndCopiesStayIndependent: noncontiguous reversed physical IDs, first input deleted; first training Core Row=2 maps to a different physical SourceRowId. Data has no unconditional physical-ID column.',proposal='Use the detached source mapping for a future explicit presentation decision; no current cell or heading changes.',owner='04A.28 / 04A.39-42 or separately approved timing exception',gate='Separate visible-output owner authorization and original applicable native prechange/capture gate'),
        dict(id='C20-FA-ROWID-AMBIGUOUS-01',status='CANDIDATE_ONLY / NONBLOCKING',reproduction='FactorDeletionUsesCorePhysicalScoreIdsAndDoesNotGuessDuplicateSources(True): all supplied RowIds=101, one deleted; Core exposes IDs and removed count, not the deleted input position.',proposal='Do not guess source ordinals or introduce new Windows refusal; metadata records Ambiguous.',owner='Future provenance/output policy 04A.28/39-42'),
        dict(id='C20-DA-PRIOR-NONFINITE-01',status='OPEN / CANDIDATE_ONLY',reproduction='DA profiles 175/176: NaN/infinite positive-test bypass and Core prior normalization/outcomes are preserved; see actual exception or returned nonfinite profile, never a scientifically accepted result.',proposal='Separate scientific finite-weight admission decision; no Common check.',owner='04A.40-41'),
        dict(id='C20-DA-FOLD-UPPER-BOUND-01',status='SOURCE_ONLY / CANDIDATE_ONLY',reproduction='Core RunKFoldValidation loops through configured folds, with only lower clamping. New executed requests include 50/100; extreme Integer.MaxValue not run to avoid an unbounded validation loop.',proposal='No upper clamp introduced; performance/domain decision belongs to Core owner.',owner='Future Core domain/performance decision')],retained_prior='All C09/C11/C12 unapplied, C13 log-rank OPEN, C14 candidate-only, C17/C18 no-label CA dormant, C19 row-ID/nonfinite-power/inversion candidates and PCA/source-layout assertions remain unchanged.',activated_behavior_fixes=0))


def inventory(prefix='final'):
    paths=[RUN/'executions'/(prefix+'-'+v+'-'+t)/'results'/(t+'.trx') for v in ['Debug','Release'] for t in ['Common','Legacy']]
    cases={}
    corpus=json.loads((OUT/'fa-da-same-core-comparisons-v1.json').read_bytes())
    classifications={}
    for r in corpus['records']: classifications.setdefault(r['test_identity'].replace('"',''),set()).add(corpus['profile_payloads'][r['profile_id']]['classification'])
    for path in paths:
        if not path.exists(): continue
        for r in old.old.trx(path)['records']:
            if 'Multivariate20' not in r['class_name']: continue
            name=r['test_name'].replace('"','');key=r['class_name'].split('.')[-1]+'|'+name
            family='Factor' if 'Factor' in name or '(FA,' in name else 'Discriminant' if any(t in name for t in ['Discriminant','Prior','Validation','DA,','HostConversion']) else 'Factor/Discriminant'
            source='BESHStatNG.Test/src/Multivariate20AdapterTests.vb' if 'Adapter' in r['class_name'] else 'BESHStat.Excel.Common.Tests/src/Multivariate20Tests.vb'
            cases.setdefault(key,dict(method=r['method'],class_name=r['class_name'].split('.')[-1],source=source,source_sha256=c.sha(ROOT/source),production_symbols=['Multivariate'+family+'Request','Multivariate'+family+'ProjectionPolicy.Project'] if family!='Factor/Discriminant' else ['MultivariateFactorRequest','MultivariateDiscriminantRequest'],expectation='Exact direct Core/request configuration exceptions or detached shape/axis/table/writer fidelity; targeted copy/null/parser/seed assertions are identified by the method name.',fixture='Multivariate20Fixture.FactorData/FactorOptions or DiscriminantData/DiscriminantOptions; host conversion derivative for adapter cases.',classifications=sorted(classifications.get(name,{'TARGETED_CONTRACT_ASSERTIONS'})),executions=[]))['executions'].append(dict(receipt=path.relative_to(RUN).as_posix(),outcome=r['outcome']))
    for key,case in cases.items():
        name=key.split('|',1)[1]
        case['profile_ids']=sorted({r['profile_id'] for r in corpus['records'] if r['test_identity'].replace('"','')==name})
        case['expectation']=case['method']+': '+case['expectation']
        if 'PriorGrammar' in case['method']:case['production_symbols']=['MultivariateLegacyChoices.ParsePriors']
        if 'PriorInputs' in case['method']:case['production_symbols']=['MultivariateDiscriminantRequest.ApplyUserPriors']
        if 'ValidationSeed' in case['method']:case['production_symbols']=['MultivariateDiscriminantValidationRequest.Configure','MultivariateDiscriminantProjectionPolicy.Project']
        if 'HostConversion' in case['method']:case['production_symbols']=['Ui11PCA.ExtractDiscriminantPredictorData','Ui11PCA.ExtractDiscriminantPredictorNames']
        if 'ActualFactorWriter' in case['method']:case['production_symbols']+=['WindowsMultivariateWorkflowAdapter.WriteFactorData','MultivariateModelReport.LegacyTables']
        if 'ActualDiscriminantWriter' in case['method']:case['production_symbols']+=['WindowsMultivariateWorkflowAdapter.WriteDiscriminantData','MultivariateModelReport.LegacyTables']
        case['fixture']=name+'; '+case['fixture']
    python_source='scripts/tests/test_04a20_factor_discriminant.py'
    python_cases={name:dict(source=python_source,source_sha256=c.sha(ROOT/python_source),expectation=name,classification='SOURCE_AUTHORITY_EVIDENCE_CONTRACT',production_symbols=['Ui11PCA.RunFA','Ui11PCA.RunDiscriminantAnalysis','MultivariateFactorRequest','MultivariateDiscriminantRequest'],receipts=['final-Debug-full.log','final-Release-full.log','final-focused-python.log']) for name in re.findall(r'    def (test_\w+)\(self\):',(ROOT/python_source).read_text(encoding='utf-8-sig'))}
    write('test-inventory.json',dict(logical_count=len(cases),cases=cases,python_count=len(python_cases),python_cases=python_cases,source_hashes={p:c.sha(ROOT/p) for p in ['BESHStat.Excel.Common.Tests/src/Multivariate20Tests.vb','BESHStatNG.Test/src/Multivariate20AdapterTests.vb',python_source]},requirements=dict(FA='180 extraction x rotation x score x matrix profiles, 46 retention/settings/missing/degenerate cases, two cultures; actual Core dimensions/flags and exception equality.',DA='144 method x scale x priors x validation x stratification profiles, 44 negative/edge cases, three cultures; actual folds/classes/confusions and exact fixed-seed parity.',targeted='Copy safety, null/mismatch APIs, retention boundary, source identities, prior grammar and sequencing, unspecified seed observation, native-free writer/host conversion tests.'),counting='Common linked tests count once logically, twice as raw executions. Culture loops are per-test profiles, not new logical tests.'))


def qualification():
    raw=json.loads((RUN/'preservation-final-retention.json').read_bytes())
    assert raw['status']=='PASS' and not raw['errors']
    nested=json.loads((RUN/'nested-initial-metadata.json').read_bytes())
    current={p.relative_to(ROOT).as_posix() for name in ['BESHStat.Core.Tests','BESHStat.Excel.Common.Tests','BESHStatNG.Test'] for p in (ROOT/name/'TestResults').rglob('*') if p.is_file()}
    assert current=={r['path'] for r in nested}
    for r in nested:
        s=(ROOT/r['path']).stat()
        assert (s.st_size,s.st_mtime_ns,s.st_ctime_ns,s.st_ino,s.st_dev)==tuple(r[k] for k in ['bytes','mtime_ns','ctime_ns','ino','dev'])
    write('retention.json',dict(status='PASS_HISTORICAL_MEMBERSHIP_METADATA',root_files=raw['historical_files'],nested_files=len(nested),membership_metadata_errors=0,receipt=dict(alias='LOCAL_RETENTION:final-retention',sha256=c.sha(RUN/'preservation-final-retention.json')),scope='All initial historical path membership, size, creation/write nanoseconds and file identity. Frozen payloads/authorities/source additionally SHA-256 checked; not an all-historical-byte audit.',retained_through='End of Phase 04A; no cleanup/compression/relocation/overwrite.'))
    initial_pins=json.loads((RUN/'final-input-pins.json').read_bytes())
    pins=json.loads((RUN/'final-qualified-input-pins.json').read_bytes())
    changed={p for p in pins if pins[p]!=initial_pins[p]}
    assert changed=={'BESHStat.Excel.Common.Tests/src/Multivariate20Tests.vb'},changed
    for p,h in pins.items(): assert c.sha(ROOT/p)==h,p
    write('final-source-qualification.json',dict(compiled_sources_unchanged_after_final_suites=True,final_input_pins=pins,source_snapshot_sha256=c.sha(RUN/'final-qualified-input-pins.json'),initial_full_matrix_snapshot_sha256=c.sha(RUN/'final-input-pins.json'),late_test_only_edit={p:dict(before=initial_pins[p],after=pins[p]) for p in changed},component_selection='Complete final-Debug/final-Release matrices qualify all production/Python/Help bytes. After the new-test-only nonfinite-prior fixture correction, full Common and Windows suites were rebuilt and rerun in both configurations as final-source-Debug/Release; these replace their earlier components in aggregate totals and corpus. Other components were unchanged. Focused repeats do not add to totals.',native='NOT_RUN / UNAUTHORIZED',independent_R='NOT_RUN / UNAUTHORIZED',no_framework_package_target_changes=True))
    write('development-corrections.json',dict(corrections=[dict(receipt='dev02-Common',issue='Mutation test oracle retained Core header-array aliases; expected table mutated with Core names.',resolution='Detached the reference through the existing legacy ReportPolicy before mutating Core. Production projection was already detached; no old tests or numerics changed.'),dict(receipt='final-source-Debug/Release-Common/Legacy',issue='New nonfinite-prior fixtures 175/176 originally supplied only two of the three/four class labels and reached missing-prior rejection first.',resolution='Kept the complete generated prior list and replaced its first weight with NaN/Infinity; added observed nonfinite report-cell counts. Full affected Common and Windows suites rerun in Debug and Release after this new-test-only edit; production unchanged.')],production_numerical_corrections=0))
    write('toolchain.json',dict(python=dict(resolved_executable='<PYTHON_EXE>',version=platform.python_version(),executable_sha256=c.sha(Path(sys.executable)),resolution='Configured portable WinPython; actual resolved executable in private command receipts, never public home paths.'),
        dotnet_sdk=subprocess.check_output(['dotnet','--version'],cwd=ROOT,text=True).strip(),node=subprocess.check_output(['node','--version'],cwd=ROOT,text=True).strip(),os=dict(system=platform.system(),release=platform.release(),version=platform.version(),architecture=platform.machine()),
        TFMs=dict(core='netstandard2.0',common='netstandard2.0',windows='net48',managed_office_and_tests='net10.0'),VS='MSBuild 18.11.1 / VSTest 18.11.0 (VS2026), as recorded by actual build/test receipts',packages='Existing offline packages only; no restore/install/network authority exercised.'))

if __name__=='__main__':
    p=argparse.ArgumentParser();p.add_argument('action',choices=['entry','validation','numerical','source','commands','authority','documentation','inventory','qualification']);p.add_argument('--prefix',default='final');a=p.parse_args()
    if a.action=='numerical': numerical(a.prefix)
    elif a.action=='inventory': inventory(a.prefix)
    else: globals()[a.action]()
