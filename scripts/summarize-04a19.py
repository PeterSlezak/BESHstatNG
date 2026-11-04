"""04A.19 actual receipt accounting and source/authority checks; never rewrite history."""
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

c = module('capture19', 'scripts/capture-04a19-contract.py')
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
    if name=='kmeans-hierarchical-projection-comparisons-v1.json':
        (OUT/name).write_text(json.dumps(value,ensure_ascii=False,separators=(',',':'),allow_nan=False)+'\n',encoding='utf-8',newline='\n')
    else:
        c.emit(name, value)

def measured(name, config):
    suites = old.suites_for(name, config)
    if name.startswith('final-'):
        # Only test receipt payload logging changed after the full matrices.
        # Use entire final-source Common/Windows reruns, never a focused subset.
        for index,suite in [(3,'Common'),(4,'Legacy')]:
            suites[index]=old.old.trx(RUN/'executions'/('final-source-'+config+'-'+suite)/'results'/(suite+'.trx'))
    groups = old.previous.typed_groups(suites)
    weight = old.prior.weight
    new = {k: rows for k, rows in groups.items() if any('Multivariate19' in r['class_name'] for r in rows)}
    logical = dict(total=sum(weight(k) for k in groups), passed=sum(weight(k) for k,rows in groups.items() if all(r['outcome']=='Passed' for r in rows)))
    logical['nonpassed'] = logical['total']-logical['passed']
    for suite in suites:
        suite['batch19_total'] = sum('Multivariate19' in r['class_name'] for r in suite['records'])
        suite['batch19_passed'] = sum('Multivariate19' in r['class_name'] and r['outcome']=='Passed' for r in suite['records'])
        suite.pop('records'); suite.pop('batch11',None)
    python_logs = [RUN/(name+'-full.log'),RUN/(name+'-common-python.log')]
    if name.startswith('final-'):
        python_logs=[RUN/('final-python-'+config+'-'+suffix+'.log') for suffix in ['tools','help','scripts','common-python']]
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
    after={}
    for v in ['Debug','Release']:
        for suffix in ['tools','help','scripts','common-python']:
            text=(RUN/('final-python-'+v+'-'+suffix+'.log')).read_text(encoding='utf-8',errors='replace')
            for m in re.finditer(r'^(?:FAIL|ERROR): [^\n]+? \((test_[^()]+)\)([^\n]*)\n-+\n(.*?)(?=\n=+\n|\n-+\nRan |\Z)',text,re.M|re.S):
                terminal=next((line for line in reversed(m[3].splitlines()) if line.strip()),'')
                terminal=re.sub(r'[A-Za-z]:[\\/][^\r\n]+','<local-path>',terminal)
                log=RUN/('final-python-'+v+'-'+suffix+'.log')
                after.setdefault(m[1],[]).append(dict(raw_alias='LOCAL_LOG:'+log.stem,raw_sha256=c.sha(log),decoded_LF_traceback_sha256=hashlib.sha256(m[3].encode()).hexdigest(),terminal=terminal[:700]))
    details=[]
    for identity,outcome in configs['Debug']['python']['nonpasses'].items():
        inherited=identity in initial['Debug']['python']['nonpasses']
        reason=prior_reasons.get(identity,'')
        if not inherited:
            reason='Newly triggered frozen source-layout/hash assertion: authorized clustering extraction changes form/adapter source ownership; prior assertion remains a genuine failure. Exact terminal and raw receipt hashes retained.'
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
            for node in ET.parse(path).getroot().findall('.//'+NS+'UnitTestResult'):
                for stdout in node.findall('.//'+NS+'StdOut'):
                    for line in (stdout.text or '').splitlines():
                        if not line.startswith('04A19_XML|'): continue
                        data=sanitize(old.xml_value(ET.fromstring(base64.b64decode(line.split('|',1)[1]))))
                        key=hashlib.sha256(json.dumps(data,sort_keys=True,ensure_ascii=False,separators=(',',':'),allow_nan=False).encode()).hexdigest()
                        payloads.setdefault(key,data)
                        records.append(dict(profile_id=key,configuration=config,tfm=tfm,os='Windows',test_identity=node.attrib['testName'],test_outcome=node.attrib['outcome'],raw_trx_alias='LOCAL_TRX:'+prefix+'-'+config+'-'+suite,raw_trx_sha256=c.sha(path)))
    assert records,'No executed profiles'
    write('kmeans-hierarchical-projection-comparisons-v1.json',dict(version=1,status='NOT_GOLDEN',validation='SAME_CORE_COMPATIBILITY_ONLY',records=records,profile_payloads=payloads,record_count=len(records),payload_count=len(payloads),outcomes=dict(Counter(payloads[r['profile_id']]['classification'] for r in records)),
        payload_identity='SHA-256 of canonical public JSON: sorted keys, compact separators, UTF-8, ensure_ascii=False. Raw XML/TRX hashes are distinct.',
        tolerance='Exact discrete labels/order/types/nonfinite classes; exact same-runtime numerical array digests, table doubles max(1e-9,abs(expected)*1e-9). No relabelling or cross-runtime scientific oracle.',
        provenance='Embedded basic fixture matches read-only historical CSV; synthetic edge variants are source-backed. Each execution links its actual test and raw receipt. Unspecified seeds are observed then replayed with the returned seed; no equality promised across unspecified fits.',
        fixture_authorities={p:dict(sha256=c.sha(ROOT/p),use=role) for p,role in [
            ('BESHStatNG.Test/TestData/cluster_dataset_basic.csv','Embedded six-row numerical fixture matches values; no file conversion in Common'),
            ('BESHStatNG.Test/TestData/cluster_dataset_missing.csv','Read-only provenance for missing variants; new interior deletion also exercised'),
            ('BESHStatNG.Test/R_referenceScripts/clustering_reference.R','READ_ONLY / NOT_EXECUTED; later independent validation remains gated'),
            ('BESHStatNG_Help_MkDocs/docs/assets/data/011xyzscatterplot/011xyzscatterplot_gif.csv','Documented public protein example; provenance only, screenshots and claims not retested'),
            ('BESHStat.Excel.Common.Tests/src/Multivariate19Tests.vb','Executed fixture generator and compatibility checks'),
            ('evidence/04A17/multivariate-comparisons-v1.json','Frozen prior same-Core profiles; not regenerated')]},
        native='NOT_RUN / UNAUTHORIZED',independent_R='NOT_RUN / UNAUTHORIZED',promoted=False))
    print('Profiles',len(records),'payloads',len(payloads))

def source():
    assert c.git('rev-parse','HEAD')==c.git('rev-parse','main')==c.BASE
    assert c.git('branch','--show-current')=='codex/04A-19'
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
    assert changed=={'RunKmeans','RunHierarchicalClustering'},changed
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
    write('original-control-crosswalk.json',dict(authorities={p:dict(sha256=c.sha(ROOT/p),bytes=(ROOT/p).stat().st_size) for p in [authority.CONTROLS,authority.LEDGER]},controls=[dict(control_id=r['control_id'],path=r['path'],source_row_sha256=authority.digest(r)) for r in selected],obligations=[dict(obligation_id=r['obligation_id'],linked_control_ids=r.get('linked_control_ids',[]),original_row_sha256=authority.digest(r),status='OPEN') for r in obligations],populations=populations,historical_case=lineage['case'],inherited_native_lineage_sha256=c.sha(ROOT/'evidence/04A18/native-lineage-successor.json'),overlap='Populations overlap and must not be summed.',releases=0,discharges=0,P='NO',N1='NO GO',N2='NO',C2='UNMET',native='NOT_RUN / UNAUTHORIZED'))
    write('cluster-rowid-disposition.json',dict(id='C19-CLUSTER-ROWID-DISPLAY-01',status='OPEN / CANDIDATE_ONLY',fixture=dict(source_rows=[101,105,118,131],deleted_input_ordinal=2,active_input_ordinals=[1,3,4],active_source_rows=[101,118,131],active_leaf_ids=[1,2,3],internal_merge_ids=[4,5]),observed='Data OriginalRow is physical source RowId; Core membership OriginalRow is 1-based input ordinal, after deletion still 1,3,4. Active leaf 2 is input ordinal 3 and source RowId 118. Display labels do not define identity.',executed_tests=['PhysicalRowsInputOrdinalsAndLeavesStayDistinct(False)','PhysicalRowsInputOrdinalsAndLeavesStayDistinct(True)'],dormant_proposal='Use projected observation SourceRowId for a future explicitly approved output alignment, while retaining input ordinal provenance. No existing report cell or heading changed; no production activation switch.',owner='04A.28 / 04A.39-42 or separately approved timing exception',activation_gate='Separate owner-approved visible behavior decision and original applicable native prechange/capture gate; software extraction acceptance alone grants no release.'))
    print('Authority populations',populations,'OPEN linked',len(obligations))

def documentation():
    pins=json.loads((RUN/'baseline.json').read_bytes())['source_pins']
    pages=['BESHStatNG_Help_MkDocs/docs/methods/'+v+'.md' for v in ['k-means-clustering','hierarchical-clustering']]
    metadata='BESHStatNG_Help_MkDocs/docs/_meta/methods.yml'
    assert c.sha(ROOT/metadata)==pins[metadata]
    write('documentation-audit.json',dict(pages={p:dict(entry_sha256=pins[p],final_sha256=c.sha(ROOT/p),bytes=(ROOT/p).stat().st_size,amendments='Narrow Windows output identity/seed/scale/cut clarifications, actual sheet names, truthful Office/Mac unavailability; existing screenshots/example values preserved.') for p in pages},metadata_sha256=pins[metadata],metadata_changed=False,screenshots_changed=False,strict_mkdocs='See final-mkdocs raw command receipt and command-outcomes.json; no native screenshots generated.'))
    write('clustering-request-projections-v1.json',dict(version=1,status='NOT_GOLDEN / SAME_CORE_COMPATIBILITY_ONLY',
        shipping=dict(KMeans='Import labels and numeric matrix -> original optional centre importer -> seed/control parsing -> detached request.Configure -> one Core Fit -> adapter Data section -> projection over Result and wrapResults -> LegacyTables -> existing writer',Hierarchical='Labels/matrix -> detached fit request.Configure before cut-field parse -> cut.Configure -> one Core Fit -> adapter Data section -> projection over existing Core memberships/tables -> writer -> optional PlotPlan -> late display snapshot -> adapter/native CreateDendrogramChart'),
        original_call_equivalence=dict(native_renderer='Unchanged ClusterAnalysisPlotExcel.CreateDendrogramChart; existing Result and newly created Dendrogram worksheet; A1,680.0,420.0; same height/orientation/label/cut getters and parsed fields, evaluated only after result output and Dendrogram sheet naming.',parameters={'topLeftCellAddress':'request.Anchor = A1','chartWidth':'request.Width = 680.0','chartHeight':'request.Height = 420.0','heightMode':'same GetSelectedHierarchicalDendrogramHeightMode','orientation':'same GetSelectedHierarchicalDendrogramOrientation','labelMode':'same GetSelectedHierarchicalDendrogramLabelMode','chartTitle':'request.Title = Hierarchical Clustering Dendrogram','cutMode':'same late GetSelectedHierarchicalMembershipMode','membershipClusterCount':'same late CInt(nudHierarchicalClusters.Value)','membershipCutHeight':'same late ParseUiDouble(tbHierarchicalCutHeight.Text, Cut height)'},proof='Prechange method hashes and locally retained exact source; new source-call-graph contracts plus managed Core-layout equality and writer block comparisons. No independent or native chart golden claimed.'),
        data_section=dict(sheet='Data',header_row=1,body_row=2,columns='OriginalRow, optional RowLabel, acquired raw variables in selected order; existing ClusterInputTable retains CLR cells.'),
        results_sheets=['KMeans results','Hierarchical results'],raw_matrix_ownership='Borrowed for immediate synchronous Core/writer calls only. Requests/projections do not retain raw N x P matrices or mutable Core arrays.',
        fixtures='Actual per-execution option, axis, seed, failure and sequence payloads are linked by profile_id in kmeans-hierarchical-projection-comparisons-v1.json. Source keys preserve selected order; display duplicates never define identity.',native='NOT_RUN / UNAUTHORIZED'))
    write('candidate-issues.json',dict(issues=[dict(id='C19-CLUSTER-ROWID-DISPLAY-01',status='OPEN / UNAPPLIED',owner='04A.28 / 04A.39-42',reproduction='PhysicalRowsInputOrdinalsAndLeavesStayDistinct: source 101,105,118,131; delete input 2; result OriginalRow remains 1,3,4.'),dict(id='C19-HIERARCHICAL-NONFINITE-POWER-01',status='OPEN / CANDIDATE_ONLY',owner='04A.40-41 scientific/domain policy',reproduction='Hierarchical profile 165: Minkowski power NaN passes settings but Fit throws KeyNotFoundException for key -1; profile 166 positive infinity is retained as Core-returned, not scientifically validated.',proposal='Separately review finite-power admission/exception behavior; no new Common or Windows refusal activated.'),dict(id='C19-HIERARCHICAL-INVERSION-CUT-01',status='OPEN / CANDIDATE_ONLY',owner='04A.40-41 scientific policy and 04A.28 reporting',reproduction='ExistingInversionAndPrefixHeightCutRemainCandidateOnly: (-1,0),(1,0),(0,1.8), centroid/median Euclidean; heights 2 then 1.8. Cut 1.9 returns three memberships because Core stops at the first higher merge, despite the existing all-merges <= footnote.',proposal='Separate scientific/reporting decision required. Keep merge order, height cut, workbook footnote and geometry unchanged.')],prior_deferrals='C09/C11/C12 unapplied; C13 log-rank earliest-censored-time OPEN for 04A.40-41; C14 candidate-only; C17/C18 unlabelled CA activation dormant/unapplied; frozen PCA/layout failures retained.',activated_corrections=0))

def inventory(prefix='final'):
    paths=[RUN/'executions'/(prefix+'-'+v+'-'+s)/'results'/(s+'.trx') for v in ['Debug','Release'] for s in ['Common','Legacy']]
    cases={}
    for path in paths:
        if not path.exists(): continue
        suite=old.old.trx(path)
        for r in suite['records']:
            if 'Multivariate19' not in r['class_name']: continue
            key=r['class_name'].split('.')[-1]+'|'+r['test_name'].replace('"','')
            cases.setdefault(key,dict(method=r['method'],class_name=r['class_name'].split('.')[-1],executions=[]))['executions'].append(dict(receipt=path.relative_to(RUN).as_posix(),outcome=r['outcome']))
    write('test-inventory.json',dict(logical_count=len(cases),cases=cases,source_hashes={p:c.sha(ROOT/p) for p in ['BESHStat.Excel.Common.Tests/src/Multivariate19Tests.vb','BESHStatNG.Test/src/Multivariate19AdapterTests.vb','scripts/tests/test_04a19_clustering.py']},requirements=dict(matrix='281 seeded K-means/hierarchical profiles, each in invariant and sk-SK cultures, both runtimes/configurations; direct options calls compared to new request/projection',additional='Identity/deletion/exact survivors, copy safety/report views, unspecified-seed observation/replay, three fixed seeds, SSE/display distance, 48 layout/display/cut requests, cut boundaries, centroid/median inversion; 31 Windows-only writer/capture/result-writer/import-validation cases',python='12 new source/evidence/authority/privacy contracts; full old suites remain unmodified.'),counting='Linked Common methods count once logically, twice as raw framework executions. Culture loops remain execution profiles within an authored logical test.'))

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
    compiled=['BESHstatNG/src/UI/Ui11PCA.vb','BESHstatNG/src/DM/WindowsMultivariateWorkflowAdapter.vb','BESHStatNG.Test/BESHStatNG.Tests.vbproj','BESHStat.Excel.Common/src/Analysis/MultivariateClusteringRequest.vb','BESHStat.Excel.Common/src/Results/MultivariateClusteringProjection.vb','BESHStat.Excel.Common.Tests/src/Multivariate19Tests.vb','BESHStatNG.Test/src/Multivariate19AdapterTests.vb']
    for config in ['Debug','Release']:
        start=(RUN/('final-source-'+config+'-build.log')).stat().st_ctime_ns
        assert all((ROOT/p).stat().st_mtime_ns<=start for p in compiled)
    records=[]
    for config in ['Debug','Release']:
        for suite in ['Common','Legacy']:
            p=RUN/'executions'/('final-source-'+config+'-'+suite)/'execution.json'
            d=json.loads(p.read_bytes())
            records.append(dict(configuration=config,suite=suite,exit_code=d['exit_code'],raw_receipt_sha256=c.sha(p),copied_assemblies=[r for r in d['copied'] if r['path'].endswith('.dll') and r['path'].startswith('BESHStat')]))
    write('final-source-qualification.json',dict(compiled_sources_unchanged_after_final_suites=True,compiled_files={p:dict(sha256=c.sha(ROOT/p),bytes=(ROOT/p).stat().st_size) for p in compiled},
        production='All final production bytes were present for both complete full matrices. Later managed-test edit adds receipt fields for report-rejected fits and plot source axes only; no assertions or production algorithms changed. Complete affected Common/Windows suites rebuilt/reran in both configurations after that edit.',
        python='After removing a privacy-scanner-matching regex literal in the new test, all four discovery roots reran separately per configuration; comparable C02 semantic observations from the full matrices remain unchanged. Earlier transient failures remain in raw receipts, excluded from current counts by explicitly selected newer complete component runs.',
        component_selection='Core/Office/managed WASM/Node and builds: final-{configuration}; Common/Windows: final-source-{configuration}; Python: final-python-{configuration}-{tools,help,scripts,common-python}. No focused subset substitutes for a full suite. Focused final Release and new Python repeat supplement, not add to totals.',records=records,
        native='NOT_RUN / UNAUTHORIZED',independent_R='NOT_RUN / UNAUTHORIZED',no_framework_package_target_changes=True))
    write('development-corrections.json',dict(corrections=[
        dict(receipt='dev01-build',issue='New test function and local both named Centers; VB compilation refused.',resolution='Renamed only the new test fixture function StartingCenters.'),
        dict(receipt='dev02-tests',issue='New For Each labels collided with a Framework test helper named Labels.',resolution='Declared explicit enum labelMode in the new test.'),
        dict(receipt='preview-Debug-Common',issue='New inversion test used Core enum value 6 (Ward), assuming UI ordering.',resolution='Used actual Core centroid/median values 4 and 5 after inspecting enum authority; both observed inversions remain unchanged.'),
        dict(receipt='historical rerun guard',issue='Prior wrapper fixed receipt names refused an existing log with FileExistsError.',resolution='New batch wrapper prefixes final-authority names. No historical/default log overwritten.'),
        dict(receipt='final-Debug and final-Release Python',issue='New privacy-test regex literal matched old home-path scanner, causing two new privacy failures.',resolution='Removed the redundant literal while retaining drive/SID assertions and the full independent candidate privacy scan. All four Python roots rerun in both configurations; prior tests unchanged.'),
        dict(receipt='final-source-Debug/Release',issue='Evidence completeness for fitted-but-report-rejected outcomes.',resolution='Only new test logging adds actual available Core row/leaf axes and explicit absence at earlier rejection. Entire affected Common/Windows suites rerun; counts/assertions unchanged.')],production_numerical_corrections=0))
    write('toolchain.json',dict(python=dict(resolved_executable='<PYTHON_EXE>',version=platform.python_version(),executable_sha256=c.sha(Path(sys.executable)),resolution='Configured portable WinPython; actual resolved executable in private command receipts, never public home paths.'),
        dotnet_sdk=subprocess.check_output(['dotnet','--version'],cwd=ROOT,text=True).strip(),node=subprocess.check_output(['node','--version'],cwd=ROOT,text=True).strip(),os=dict(system=platform.system(),release=platform.release(),version=platform.version(),architecture=platform.machine()),
        TFMs=dict(core='netstandard2.0',common='netstandard2.0',windows='net48',managed_office_and_tests='net10.0'),VS='MSBuild 18.11.1 / VSTest 18.11.0 (VS2026), as recorded by actual build/test receipts',packages='Existing offline packages only; no restore/install/network authority exercised.'))

if __name__=='__main__':
    p=argparse.ArgumentParser();p.add_argument('action',choices=['entry','validation','numerical','source','commands','authority','documentation','inventory','qualification']);p.add_argument('--prefix',default='final-source');a=p.parse_args()
    if a.action=='numerical': numerical(a.prefix)
    elif a.action=='inventory': inventory(a.prefix)
    else: globals()[a.action]()
