"""04A30 publication validation. Reads raw isolated receipts; never edits authority.

Public output omits raw diagnostics/private paths. No historical validator or
product build is invoked by this script. Run after all selected lanes complete.
"""
from audit import ROOT,HERE,BASE,git,sha,save,pin
import csv,json,re,hashlib,struct,sys,time,os
from collections import Counter

RUN=ROOT/'.test-rationalization-04A30.tmp'
FINAL='verified'
DOSSIER='docs/architecture/batch-04A-30-owner-review.md'
NEW_CORE=['RocDataCore','NormalPlotDataCore','BoxSegmentsCore','HistogramDataCore','ScatterMatrixDataCore','XyzDataCore']
PRODUCTION=['BESHStat.Core/src/Graphics/'+n+'.vb' for n in NEW_CORE]+['BESHStat.Excel.Common/src/Analysis/OrderedPlotGroups.vb']
WINDOWS=['BESHstatNG/src/Graphics/'+n+'.vb' for n in ['ROC','NormalPlot','BoxPlot','Histogram','ScatterPlotMatrix','XYZscatter']]
TESTS=['BESHStat.Core.Tests/src/Graphics/'+n+'.vb' for n in ['Graphics30Tests','LegacyGraphics30','LegacyBoxBounds30']]+['BESHStat.Excel.Common.Tests/src/OrderedPlotGroups30Tests.vb','BESHStatNG.Test/src/Charts/Graphics30AdapterTests.vb','BESHStatNG.Test/BESHStatNG.Tests.vbproj']
def read(path):return json.loads(Path(path).read_bytes())
from pathlib import Path
def clean(value):
    if isinstance(value,list):return [clean(v) for v in value]
    if isinstance(value,dict):return {k:clean(v) for k,v in value.items()}
    if not isinstance(value,str):return value
    for a,b in [(str(RUN),'<AUDIT_RUN>'),(RUN.as_posix(),'<AUDIT_RUN>'),(str(ROOT),'<REPO_ROOT>'),(ROOT.as_posix(),'<REPO_ROOT>'),(sys.executable,'<PYTHON>')]:value=value.replace(a,b)
    computer=os.environ.get('COMPUTERNAME','!unset!')
    value=re.sub(re.escape(computer),'<HOST>',value,flags=re.I)
    value=re.sub(r'[A-Za-z]:[\\/]+Users[\\/]+[^\\/\s"\']+', '<PRIVATE_HOME>',value,flags=re.I)
    value=re.sub(r'S-1-5-\d+(?:-\d+)*',lambda m:'<PRIVATE_SID:'+hashlib.sha256(m.group().encode()).hexdigest()[:16]+'>',value)
    return value
def command_receipts():
    records=[]
    for path in sorted((RUN/'commands').glob('*/command.json')):
        value=read(path);args=value['command']
        first=Path(args[0]).name.lower()
        args[0]={'msbuild.exe':'<MSBUILD>','vstest.console.exe':'<VSTEST>','python.exe':'<PYTHON>'}.get(first,args[0])
        value['receipt_id']=path.parent.name
        value['raw_receipt_sha256']=sha(path)
        records.append(clean(value))
    save('command-outcomes.json',dict(commands=records,raw_location='<AUDIT_RUN>/commands/<receipt_id>/',preservation='Raw stdout/stderr/command receipts retained locally; only hashes and sanitized commands published',original_verifier='NOT_RUN unchanged: fixed TestResults/Codex writers and shared bin/obj; equivalent unchanged isolated runner used for portable and Legacy lanes',runner=pin(ROOT/'scripts/test_suite_rationalization.py'),wrapper=pin(HERE/'run.py'),development_nonpasses='Initial scaffold syntax/build issues were corrected; all command receipt exits including failed intermediate builds are retained. No existing assertion was weakened.'))
    return records

def managed():
    runs={};failures=[]
    for label,config in [('before','Debug'),(FINAL,'Debug'),(FINAL,'Release')]:
        for suite in ['Core','Common','Legacy','OfficeAddin','OfficeExecution','ControlMitigation']:
            path=RUN/'executions'/f'{label}-{config}-{suite}'/'cases.json'
            value=read(path);key=(label,config,suite);runs[key]=value
    def identity(r):return (r['class_name'],r['method'],r['test_name'])
    def indexed(records):
        buckets={}
        for row in records:buckets.setdefault(identity(row),[]).append(row)
        # Raw MSTest results can repeat an identical displayed identity. Keep its
        # multiplicity and outcome multiset; never silently deduplicate it.
        return {key+(ordinal,):row for key,bucket in buckets.items() for ordinal,row in enumerate(sorted(bucket,key=lambda r:r['outcome']))}
    deltas=[];suite_rows=[];identity_rows=[]
    for suite in ['Core','Common','Legacy','OfficeAddin','OfficeExecution','ControlMitigation']:
        values=[runs[(label,c,suite)] for label,c in [('before','Debug'),(FINAL,'Debug'),(FINAL,'Release')]]
        maps=[indexed(v['records']) for v in values]
        assert all(len(m)==len(v['records']) for m,v in zip(maps,values)),('Duplicate raw identity',suite)
        old,new,release=maps
        missing=set(old)-set(new)
        changed=[i for i in set(old)&set(new) if old[i]['outcome']!=new[i]['outcome']]
        added=set(new)-set(old)
        release_drift=[i for i in set(new)&set(release) if new[i]['outcome']!=release[i]['outcome']]
        assert not missing and not changed and not release_drift and set(new)==set(release),(suite,missing,changed,release_drift)
        assert all('Graphics30' in '|'.join(map(str,i)) or 'OrderedPlotGroups30' in '|'.join(map(str,i)) for i in added),added
        deltas.append(dict(suite=suite,missing_before_cases=0,changed_before_outcomes=0,added_04A30=len(added),release_vs_debug_outcome_changes=0))
        for i in sorted(new):
            display=i[2]
            # Security/redaction test display names themselves contain synthetic
            # drive paths and credential/email examples. Keep their exact raw
            # identity hash and method while excluding those payloads publicly.
            if re.search(r'\b[A-Za-z]:[\\/]|[A-Za-z0-9._%+-]+@[A-Za-z0-9.-]+\.[A-Za-z]{2,}|https?://[^\s/"<>:@]+:[^\s/"<>@]+@|\\\\[A-Za-z0-9_.-]+\\',display):
                display='<REDACTED_TEST_DISPLAY:'+hashlib.sha256(display.encode()).hexdigest()[:24]+'>'
            identity_rows.append(clean(dict(suite=suite,class_name=i[0],method=i[1],test_name=display,raw_identity_sha256=hashlib.sha256(json.dumps(i[:3]).encode()).hexdigest(),raw_occurrence_ordinal=i[3],before_debug=old[i]['outcome'] if i in old else 'NEW_04A30',after_debug=new[i]['outcome'],after_release=release[i]['outcome'])))
        for label,config in [('before','Debug'),(FINAL,'Debug'),(FINAL,'Release')]:
            v=runs[(label,config,suite)]
            source_build=read(RUN/'builds'/label/config/'build.json')
            suite_rows.append(dict(label=label,configuration=config,suite=suite,outcomes=v['outcomes'],counters=v['counters'],trx_sha256=v['sha256'],seconds=v['command']['seconds'],source_pins_sha256=hashlib.sha256(json.dumps(source_build['source_pins'],sort_keys=True).encode()).hexdigest(),raw_receipt_sha256=sha(RUN/'executions'/f'{label}-{config}-{suite}'/'cases.json')))
            for r in v['records']:
                if r['outcome']!='Passed':failures.append(dict(label=label,configuration=config,suite=suite,class_name=r['class_name'],method=r['method'],test_name=r['test_name'],outcome=r['outcome'],message_sha256=hashlib.sha256(r['message'].encode()).hexdigest(),summary=clean(r['message'].splitlines()[0][:500]) if r['message'] else 'See preserved raw TRX'))
    with (HERE/'managed-test-identities-v1.csv').open('w',encoding='utf-8',newline='') as f:
        writer=csv.DictWriter(f,fieldnames=list(identity_rows[0]));writer.writeheader();writer.writerows(identity_rows)
    save('managed-validation-v1.json',dict(suites=suite_rows,identity_deltas=deltas,nonpasses=failures,release_reference='Fresh full Release managed run. Entry Release NOT_RUN; its failures match the freshly executed original-source Debug identities. No prior Release trial reused.'))
    focused=[]
    for config in ['Debug','Release']:
        for suite in ['Core','Common','Legacy']:
            p=RUN/'executions'/f'{FINAL}-{config}-graphics30-{suite}'/'cases.json';v=read(p)
            assert v['command']['exit_code']==0 and set(v['outcomes'])=={'Passed'}
            focused.append(dict(configuration=config,suite=suite,outcomes=v['outcomes'],trx_sha256=v['sha256'],seconds=v['command']['seconds'],identities=[dict(class_name=r['class_name'],method=r['method'],test_name=r['test_name'],outcome=r['outcome']) for r in v['records']]))
    save('focused-validation-v1.json',dict(status='PASS',runs=focused,unique_new_cases=40,new_case_executions_per_configuration=76,baseline_cases='Focused filters also include unchanged HistogramBinningCore, ViolinPlotCore, CategoricalHistogramCore, ChartAxisScalingCore and Windows axis facade tests.',native='NATIVE_NOT_RUN',scientific='NOT_GOLDEN'))
    return suite_rows,deltas,focused,failures

def python_node():
    suites=[];deltas=[]
    for suffix in ['python-tools','python-help','python-scripts','common-python']:
        populations=[]
        for label in ['before',FINAL]:
            directory='privacyfinal-final-python-scripts' if label==FINAL and suffix=='python-scripts' else f'{label}-Debug-{suffix}'
            p=RUN/'commands'/directory/'stderr.log';raw=p.read_bytes()
            try:text=raw.decode('utf-8');encoding='utf-8'
            except UnicodeDecodeError:text=raw.decode('cp1252');encoding='cp1252 (Windows redirected stderr)'
            n=int(re.search(r'Ran (\d+) tests?',text).group(1))
            failures=int(re.search(r'failures=(\d+)',text).group(1)) if re.search(r'failures=(\d+)',text) else 0
            errors=int(re.search(r'errors=(\d+)',text).group(1)) if re.search(r'errors=(\d+)',text) else 0
            # Git stderr can interleave a test's description and its completion.
            # Collect primary descriptions and explicit failure sections instead
            # of assuming every completion occupies one uninterrupted line.
            descriptions=re.findall(r'^(test_\S+ \([^)]*\))',text,re.M)
            nonpasses=re.findall(r'^(FAIL|ERROR): (test_\S+ \([^)]*\))([^\n]*)',text,re.M)
            identities=[(name,'+'.join(sorted(set(kind for kind,key,sub in nonpasses if key==name))) or 'ok') for name in descriptions]
            assert len(descriptions)==n,(suffix,len(descriptions),n)
            suites.append(dict(label=label,suite=suffix,total=n,passed=n-failures-errors,failed=failures,errors=errors,stderr_encoding=encoding,reported_primary_identities=len(identities),nonpasses=[dict(outcome=kind,identity=key,subtest=sub.strip()) for kind,key,sub in nonpasses],identity_count_qualification='Primary descriptions reconstructed around interleaved Git warnings; every explicit failure/error subtest retained separately.',stderr_sha256=sha(p),identities_sha256=hashlib.sha256(json.dumps(identities).encode()).hexdigest()))
            populations.append(identities)
        before=dict(populations[0]);after=dict(populations[1]);assert set(before)==set(after)
        differences=[dict(identity=i,before=before[i],after=after[i]) for i in sorted(before) if before[i]!=after[i]]
        deltas.append(dict(suite=suffix,status='EXACT_IDENTITY_POPULATION_MATCH',outcome_changes=differences))
    nodes=[]
    for label in ['before',FINAL]:
        for suffix in ['office-node','resolver','wasm-node']:
            p=RUN/'commands'/f'{label}-Debug-{suffix}'/'stdout.log';text=p.read_text(encoding='utf-8')
            def count(name):
                matches=re.findall(r'(?:# |ℹ )'+name+r' (\d+)',text)
                return int(matches[-1]) if matches else None
            nodes.append(dict(label=label,suite=suffix,total=count('tests'),passed=count('pass'),failed=count('fail'),stdout_sha256=sha(p)))
    save('source-and-node-validation-v1.json',dict(python=suites,python_identity_deltas=deltas,node=nodes,final_source_recheck='Scripts745-case lane rerun after final publication identity redaction; full matrix raw privacy failures retained separately in raw receipts, not reported as green.',release='NOT_RUN: configuration-independent source/Node suites executed in full Debug lane, not duplicated in Release managed lane'))
    return suites,nodes

def protection():
    entry=read(HERE/'entry-pins.json')['files'];paths=git('ls-files').splitlines()
    aliases={'<PRIVATE_PROTECTED_PATH:'+hashlib.sha256(p.encode()).hexdigest()[:24]+'>':p for p in paths}
    allowed=set(WINDOWS+['BESHStatNG.Test/BESHStatNG.Tests.vbproj']);changed=[];missing=[];protected_count=0
    for item in entry:
        public=item['path'];relative=aliases.get(public,public);p=ROOT/relative
        if not p.is_file():missing.append(public);continue
        if p.stat().st_size!=item['bytes'] or sha(p)!=item['sha256']:
            changed.append(dict(path=public,entry_sha256=item['sha256'],final_sha256=sha(p),allowed=relative in allowed))
        else:protected_count+=1
    assert not missing and all(x['allowed'] for x in changed),(missing,[x for x in changed if not x['allowed']])
    assert {x['path'] for x in changed}==allowed
    registry=read(ROOT/'evidence/04A14/baseline-registry.json')
    captures=[dict(path=p,sha256=sha(ROOT/p),matches_registry=sha(ROOT/p)==v['sha256']) for p,v in registry['files'].items()]
    assert len(captures)==100 and all(x['matches_registry'] for x in captures)
    metadata=read(HERE/'historical-results-preservation.json')
    save('source-preservation.json',dict(status='PASS_ENTRY_BYTE_PRESERVATION',entry_files=len(entry),unchanged=protected_count,changed=changed,missing=missing,all_04A29_committed_files_unchanged=True,original_ledger_and_04A02_unchanged=True,all_100_captures_match_registry=True,captures=captures,existing_12_core_graphics_unchanged=True,windows_primary_files=38,windows_changed=6,windows_unchanged=32,udf_ribbon_designer_office_result_writers_unchanged=True,project_membership='38 Windows entries unchanged; SDK default includes seven new portable sources. Five new test sources, four linked into Framework tests.',historical_results=metadata,qualification='Bytes compared to real merged entry; inherited 04A29 publication discrepancies remain separately disclosed. Historical TestResults payload NOT_REHASHED.'))
    return changed,metadata

def operation_map():
    source_map=HERE/'migration-operation-map-v1.csv';entry_map=HERE/'migration-operation-map-entry.csv'
    if not entry_map.exists():entry_map.write_bytes(source_map.read_bytes())
    members=list(csv.DictReader((ROOT/'evidence/04A29/graphics-members-v1.csv').open(encoding='utf-8-sig',newline='')))
    rows=list(csv.DictReader(entry_map.open(encoding='utf-8-sig',newline='')))
    # Refine the initial constructor plan to retain the original Windows alias.
    removed=[r for r in rows if r['family']=='Labels' and r['old_source'].endswith('/BoxPlot.vb')]
    rows=[r for r in rows if r not in removed]
    for name in ['AddBoxPlot','wrapResults']:
        old=next(m for m in members if m['path'].endswith('/BoxPlot.vb') and m['qualified_type']=='BESHStatNG.graphics.BoxPlot' and m['name']==name)
        row=dict(removed[0]);row.update(accepted_04A29_member_id=old['member_id'],accepted_04A29_operation_id=old['operation_id'],old_body_sha256=old['body_sha256'],old_lines=old['start_line']+'-'+old['end_line'],current_owner=old['ownership'],windows_consumer='BoxPlot.'+name+' -> SnapshotGroupNames',behavior='Snapshot the current ordered labels at consumption. Original constructor array alias retained; no relabel/trim/dedup/culture conversion. Native series, appearance and table shape preserved.')
        rows.append(row)
    guard=read(HERE/'source-ownership-guard.json');targets=guard['current_members']
    mapping={
        ('ROC.vb','compute'):[('RocDataCore','ComputeCurve')],
        ('ROC.vb','DeLongSE'):[('RocDataCore','ComputeDeLong')],
        ('ROC.vb','MidRanks'):[('RocDataCore','MidRanks')],
        ('NormalPlot.vb','.ctor'):[('NormalPlotDataCore','SortedCopy')],
        ('NormalPlot.vb','compute'):[('NormalPlotDataCore','Scores')],
        ('NormalPlot.vb','computeRefLIne'):[('NormalPlotDataCore','ReferenceLine')],
        ('BoxPlot.vb','CalcForPlotting'):[('BoxSegmentsCore','Compute')],
        ('Histogram.vb','compute'):[('HistogramDataCore','BinsCopy'),('HistogramDataCore','OverlayCopy')],
        ('Histogram.vb','addChart'):[('HistogramDataCore','HorizontalBounds')],
        ('ChartingFunc.vb','ChartScaling'):[('ChartAxisScalingCore','Compute')],
        ('ScatterPlotMatrix.vb','transformData'):[('ScatterMatrixDataCore','Normalize')],
        ('XYZscatter.vb','getnorm'):[('XyzDataCore','NormalizeAxis')]
    }
    for row in rows:
        old=next(m for m in members if m['member_id']==row['accepted_04A29_member_id'])
        recipes=[('ExcelCommon.OrderedPlotGroups','.ctor')] if row['family']=='Labels' else mapping[(Path(row['old_source']).name,old['name'])]
        actual=[]
        for type_name,name in recipes:
            selected=[t for t in targets if t['qualified_type']=='BESHStatNG.'+type_name and t['name']==name]
            assert selected,(type_name,name)
            actual.extend(selected)
        row.update(status='EXISTING_CORE_BRIDGE_VERIFIED' if row['family']=='Axis' else 'IMPLEMENTED_04A30_BOUNDED_SEAM',actual_destination_member_ids=';'.join(t['member_id'] for t in actual),actual_destination_source_sha256=';'.join(sorted(set(t['source_sha256'] for t in actual))),shipping_evidence='Managed Framework adapter tests PASS; AST call-through/source contracts PASS; native rendering NATIVE_NOT_RUN',remaining_owner='04A.31' if row['family'] in ['ROC','Normal Q-Q'] else row['remaining_owner'])
    with source_map.open('w',encoding='utf-8',newline='') as f:
        writer=csv.DictWriter(f,fieldnames=list(rows[0]));writer.writeheader();writer.writerows(rows)
    save('implementation-map-refinement.json',dict(entry_map=pin(entry_map),final_map=pin(source_map),reason='Box constructor label-array alias retained by moving Common label snapshot to actual category/table consumption. No constructor/public API or grouping-policy change.',completed_records=len(rows),overlap='Normal constructor has numerical and label suboperations; these records are not distinct algorithm counts.'))
    return rows

def decode_values(values):
    output=[]
    for item in values:
        number=struct.unpack('>d',bytes.fromhex(item['bits']))[0]
        output.append(dict(value=None if item['nan'] or item['infinity'] else number,bits=item['bits'],null=False,nan=item['nan'],infinity=item['infinity'],infinity_sign=(-1 if number<0 else 1) if item['infinity'] else None))
    return output
def casebook(rows):
    cases={}
    for config in ['Debug','Release']:
        for suite in ['Core','Legacy']:
            run_id=f'{FINAL}-{config}-graphics30-{suite}'
            trx=read(RUN/'executions'/run_id/'cases.json')['sha256']
            for path in sorted((RUN/'compatibility-cases'/run_id).glob('*.json')):
                value=read(path);key=value['case']
                assert len(value['before'])==len(value['after'])
                assert all(a['bits']==b['bits'] or a['nan'] and b['nan'] for a,b in zip(value['before'],value['after']))
                entry=cases.setdefault(key,dict(case_id=key,observations=[]))
                entry['observations'].append(dict(configuration=config,runtime='net10.0' if suite=='Core' else 'net48',trx_sha256=trx,raw_case_sha256=sha(path),before=decode_values(value['before']),after=decode_values(value['after'])))
    roc_inputs=[[[4,8,6],[3,1,2,4]],[[1,1,1],[1,1]],[[1,2],[8,9,7]],[[9,8,7],[2,1]],[[2,2,4,3],[1,3,3,4,2]]]
    qq={6:[9,1,4,4,2,8],4:[2,2,2,2],2:[2,1],1:[3]}
    box=[[-9,-5,-3,-1],[-5,-1,0,3,6],[1,2,4,8],[0,0,0],[4]]
    for key,entry in cases.items():
        family='';options={};shape={};inputs=None
        if key.startswith('WINDOWS_ROC_'):
            family='ROC';inputs=[[5,1,3,3],[0,2,3]];options=dict(alpha=0.1 if key.endswith('_1') else 0.05,call_sequence='same wrapper first alpha=.05 then alpha=.1, legacy accumulator retained');shape=dict(statistics=['AUC','HanleySE','P','DeLongSE'])
        elif key.startswith('ROC_'):
            family='ROC';fixture=int(key.split('_')[1]);inputs=roc_inputs[fixture];options=dict(alpha=0.05 if key.endswith('_05') else 0.1,positive_group_index=0,negative_group_index=1)
            unique=len(set(sum(inputs,[])));shape=dict(flat_layout=[['statistics',9],['cutoffs',unique],['sensitivity',unique+1],['specificity',unique],['false_positive_rate',unique+1]],orientation='X=1-specificity,Y=sensitivity;first(1,1),last(0,0)')
        elif key.startswith('WINDOWS_QQ_') or key.startswith('QQ_'):
            family='Normal Q-Q';windows=key.startswith('WINDOWS');match=re.search(r'_(SPSS|OLS|R)(?:_(\d+))?$',key);assert match,key
            options=dict(score_method=key[len('WINDOWS_QQ_' if windows else 'QQ_'):match.start()].replace('_',' '),reference_method=match.group(1),orientation='X=data,Y=normal scores')
            inputs=[9,1,3,3,5,2] if windows else qq[int(match.group(2))]
            shape=dict(reference=[2,2],flat_layout='reference only' if windows else 'sorted data[n],scores[n],probabilities[n],reference[2x2] row-major',input_mutation='old constructor sorts caller vector; new portable and Windows constructor do not')
        elif key.startswith('BOX_'):
            family='Box';inputs=box[int(key.split('_')[1])];shape=dict(segments=['Blank','Median','Q3','Q3Minus','MedianMinus']);options=dict(quartile_owner='Existing DescriptiveStat; no harmonization',whiskers='Frozen Windows scans unchanged')
        elif key.startswith('HIST_'):
            family='Histogram';inputs=[9,-4,3,2,0,8,6,7,1,5]
            rules={'HIST_':'','HIST__Sturges_':'(Sturges)','HIST__Doane_':'(Doane)','HIST__Scott_':'(Scott)','HIST__Freedman_Diaconis_':'(Freedman-Diaconis)'}
            options=dict(rule=rules[key],PrettyBreaks=True);shape=dict(bins=['N',2],columns=['Double midpoint','Integer count'],flat_row_major=True)
        elif key.startswith('BOUNDS_'):
            family='Histogram';candidates=[[3],[0],[-2],[1,3,5],[3,1],[2,2],['NaN'],['Infinity']]
            def bits(x):return struct.pack('>d',float('nan') if x=='NaN' else float('inf') if x=='Infinity' else x).hex().upper()
            # Source-generated id includes the signed Int64 representation of the first midpoint.
            for candidate in candidates:
                signed=struct.unpack('>q',bytes.fromhex(bits(candidate[0])))[0]
                ident='BOUNDS_'+str(len(candidate))+'_'+str(signed).replace('-','_')
                if ident==key:inputs=candidate;break
            if inputs is None and entry['observations'][0]['before'][0]['nan']:inputs=['NaN']
            assert inputs is not None,key
            shape=dict(vector=['Minimum','Maximum','Width'])
        elif key.startswith('SCATTER_'):
            family='Scatter matrix';inputs=[[4,10],[1,4],[9,7]] if key.endswith('_4') else [[2,1],[2,5],[2,8]];options=dict(effective_margin=0.1,constructor_argument='ignored as before');shape=dict(matrix=[3,2],flat_row_major=True)
        elif key.startswith('XYZ_'):
            family='XYZ';inputs=[4,1,3];shape=dict(vector=3);match=re.fullmatch(r'XYZ_(_?1)_(_?[04])',key);assert match,key
            options=dict(minimum=1,direction=int(match.group(1).replace('_','-')),range=int(match.group(2).replace('_','-')),count=3,formula='((value-minimum)/range-.5)*direction; no zero-span fix')
        assert family,key
        entry.update(family=family,inputs_in_source_order=inputs,options=options,shape=shape,status='SOURCE_COMPATIBILITY_PASS',scientific='NOT_GOLDEN',native='NATIVE_NOT_RUN',comparison='Finite/infinite IEEE bits exact; NaN class equality, payload bits retained; zero tolerance',operation_ids=[r['accepted_04A29_operation_id'] for r in rows if r['family']==family],destination_member_ids=sorted(set(i for r in rows if r['family']==family for i in r['actual_destination_member_ids'].split(';'))),test_source=pin(ROOT/'BESHStat.Core.Tests/src/Graphics/Graphics30Tests.vb'))
    save('graphics-compatibility-casebook-v1.json',dict(schema='04A30-source-compatibility-v1',base=BASE,cases=list(cases.values()),fixture_provenance=pin(HERE/'legacy-fixture-provenance.json'),frozen_statement_proof='source-ownership-guard.json frozen_fixture_proofs: nine exact method bodies',exception_and_edge_contracts=dict(status='SOURCE_COMPATIBILITY_PASS',evidence='Graphics30Tests.RocInvalidAndMidrankCopies/NormalMutationFallbackEmptyAndNonfinite/HistogramExistingEngineCopies assert exact exception Type equality against frozen source or unchanged existing engine; arrays unchanged on errors. Windows adapter checks partial ROC state on DeLong failure and Q-Q pre-compute error/fallback behavior. Only successful numeric vectors are serialized; exception type comparisons are assertion evidence, not invented exception values.'),contract_cases=dict(status='NEW_PORTABLE_CONTRACT_PASS',source=pin(ROOT/'BESHStat.Excel.Common.Tests/src/OrderedPlotGroups30Tests.vb'),count=7,fixtures=['Duplicate/case/whitespace/number-vs-text labels','Ragged/empty/Nothing groups; nonfinite data retained','Filtered physical rows12/25 from accepted parser; chart order25/12','Explicit shuffled order [1,2,0], physical rows17/25/12','Invariant/sk-SK, no formatting/group coercion','Input/output defensive copies and invalid source/order','Non-zero-bound CLR vectors rejected without implicit reindexing']),intentional_differences=['NormalPlot constructor no longer sorts caller array; sorted values/numerics retained','Histogram compute protects FD/overlay calls with clones; caller order no longer indirectly sorted'],retained_candidates=['ROC repeated accumulator','Scatter constructor margin overwritten to .1','Legacy box/violin/hist quartile differences','SPSS/OLS/R reference choices and literal fallbacks','Zero spans/nonfinite propagation/sentinels','04A29 helper ordinals171/172 uncertain'],native_appearance='NATIVE_NOT_RUN; no screenshot/Excel execution',scientific='NOT_GOLDEN; frozen source and same-engine comparisons only'))
    return len(cases)

def contracts_and_handoff(rows):
    save('portable-plot-contracts-v1.json',dict(schema='04A30-portable-contracts-v1',targets=dict(Core='netstandard2.0',Common='netstandard2.0',Windows='net48'),new_files=[pin(ROOT/p) for p in PRODUCTION],contracts=[
        dict(type='RocDataCore / RocDataResult',entries=['Compute(groups,alpha=.05)','ComputeCurve(groups,alpha=.05,legacyAucSeed=0)','ComputeDeLong(groups,alpha=.05)','MidRanks(values)'],copies='Jagged groups cloned on every numerical entry. All result vectors and legacy mutable CIs copied on access.',shape='u cutoffs/specificities;u+1 sensitivities/false-positive rates;statistics/CI fields; X=1-specificity/Y=sensitivity',partial_result_contract='Compute produces full result. ComputeCurve computes only curve/Hanley fields (DeLongInterval Nothing); ComputeDeLong computes only DeLongStandardError/DeLongInterval (curve vectors and Hanley Interval Nothing; unused scalar slots default0). Windows uses only the meaningful fields.',compatibility='Explicit Windows accumulator seed retains repeated compute. Curve state is applied before separately calling DeLong, preserving singleton DeLong failure partial state. Other invalid pre-DeLong private-state persistence is not claimed; exact exception types are source-tested.',remaining='04A.31 completeness and exceptional/ref-line contracts; numerical corrections39-41 require separate authority'),
        dict(type='NormalPlotDataCore / NormalScoreData',entries=['SortedCopy','Compute','Scores(sortedData,method)','ReferenceLine(snapshot,lineMethod)','ReferenceLine(data,probabilities,scores,scoreMethod,lineMethod)'],copies='Sort a clone, snapshot inputs/getters; reference fitting receives copies. Windows still returns its mutable pZ vector and passes any edits into the compatibility overload.',shape='n sorted X data,n probabilities,n normal Y scores;reference2x2 row-major X/Y endpoints',methods=['Blom','Rankit','Van Der Waerden','SPSS','OLS','R'],fallback='Exact literal selection; unknown score yields zeros; unknown reference method yields zero Y at data min/max. No detrending.',source_axis='Core result contains sorted values; Common PointOrder requires an explicit observation permutation. Legacy Windows constructor has no physical RowId channel, so none is invented.'),
        dict(type='BoxSegmentsCore / BoxSegmentData',entries=['Compute(lower,median,upper)'],shape='Five scalar signed stacked segments per group:Blank/Median/Q3/Q3Minus/MedianMinus',compatibility='Original DescriptiveStat quartiles, outlier scans, whiskers, jitter, accumulated collection and native error bars retained.'),
        dict(type='HistogramDataCore',entries=['BinsCopy(values,rule)','OverlayCopy(values,midpoints)','HorizontalBounds(midpoints)'],copies='Protect existing bin/overlay engine with clones; no second binning/normal-density engine.',shape='N x2 Object matrix(Double midpoint,Integer frequency);overlay N x2 Double;bounds three scalars',compatibility='PrettyBreaks always on; native addChart keeps its zero-bin Exit Sub. New standalone HorizontalBounds(empty) explicitly throws ArgumentException; Windows never calls it for empty bins. Tukey/Y-scale/secondary-axis/events/Shapes stay Windows.'),
        dict(type='ScatterMatrixDataCore / ScatterMatrixNormalizedData',entries=['Normalize(n x p,margin)'],copies='Clone input and both output getters.',shape='n x p scaled values,p diagonal label X values',compatibility='Windows still overwrites margin to.1. Original correlation triangle/regression/labels/-100 break vectors stay Windows; IEEE zero-span behavior retained.'),
        dict(type='XyzDataCore',entries=['NormalizeAxis(values,minimum,range,direction,count)'],copies='Clone when non-null; zero count returns empty even for Nothing, matching the legacy empty object. No retained aliases.',shape='count centered-cube coordinates per raw axis',compatibility='Count still X-derived; bounds expansion/manual bounds/nonzero range policy/rotations/grid/cage/clipping/custom Draw stay Windows. No new zero-span policy.'),
        dict(type='ExcelCommon.OrderedPlotGroups / OrderedPlotPoint',composition=['GroupedNumericData','UnivariateObservationSource from accepted configurable/univariate parser'],copies='Deep data copies; labels/raw scalar arrays cloned; immutable point/source records and read-only point lists.',identity='Separate supplied group ordinal, original observation index, chart index and nullable physical WorksheetRow. PointOrder takes an explicit permutation. No fabricated physical rows.',labels='Supplied group order, duplicates, whitespace, case and numeric-vs-text raw labels preserved; no regroup/trim/collision repair. Raw labels accept immutable text/numeric/Boolean/Char scalars and Nothing; host-native categories/coercion not generalized.',shipping=['NormalPlot.New uses supplied variable display name','BoxPlot.SnapshotGroupNames used by AddBoxPlot category values and wrapResults table labels; original label-array alias retained'],remaining='No broad normalization of Core-backed specialty families; no original04A03 parser changes.'),
        dict(type='ChartAxisScalingCore / CHARTscale',status='EXISTING_CORE_BRIDGE_VERIFIED',copies='Existing immutable Core scalars mapped to mutable legacy CHARTscale Min/Max/Scale; no source change to calculator/bridge.',tests=['ChartAxisScalingCoreTests','ChartAxisScalingFacadeTests','Graphics30AdapterTests.ScatterXyzAndAxisWindowsConsumers'])],dependency_graph=['Windows numerical wrappers -> new Core data seams -> existing Core statistical/distribution routines','Windows label/category/table consumption -> Common OrderedPlotGroups -> existing grouped/source contracts','Windows ChartingFunc -> existing ChartAxisScalingCore / HistogramBinningCore unchanged','No new portable reference to ExcelDna/Interop/WinForms/System.Drawing/files/timers/Microsoft.VisualBasic'],native_exclusions='All actual chart/series/Shape/font/color/coercion/legend/export/clipboard/event/lifetime/cleanup remains Windows. Pure bounds/category argument assembly changed only as enumerated by AST guard.',office='No Office/Mac/WASM/LibreOffice plot feature or renderer enabled'))
    packages=[
        ('04A.31',['ROC','Normal Q-Q'],'Retained ROC corner-case/alpha/CI/repeated-state policy; exact Q-Q/reference-line edge semantics, copied sorted-view provenance completeness. Do not activate statistical corrections or detrending.'),
        ('04A.32',['Box','Histogram','Axis','Labels'],'Descriptive aggregates/whiskers/jitter, histogram Tukey summary/Y-band/overlay axes and violin/group semantics. Preserve different quartile definitions until authorized;04A.75 PrettyBreaks option remains separate.'),
        ('04A.33',['Scatter matrix','XYZ'],'Scatter correlation/regression/cell-grid/point-order payloads;XYZ rotations/cage/ticks/interpolation/bounds/custom-object boundary. Preserve .1 margin,-100 sentinel/nonfinite cases; no new perspective or cleaning.'),
        ('04A.34',['specialty/SPC/PSM/3D'],'No specialty extraction in04A30; accepted Core/Common projections remain authority, measured native geometry/lifetime still Windows.'),
        ('04A.35',['CI/agreement/PSM'],'No changes; CI/report specs and retained PSM finite/missing/threshold candidates remain assigned.'),
        ('04A.36',['KM/Cox/SPC'],'No changes; retain corrected KM123 lineage, existing Core payload route and blocked native obligations. Cox residual UI not activated.'),
        ('04A.37',['PCA/FA/cluster/CA/MCA'],'No changes; multivariate payload/label projections and original row/sign/orientation defects remain assigned.')]
    save('graphics-roadmap-handoff-v1.json',dict(completed_operation_records=len(rows),overlap='15 operation/member records across8 scope families, not15 independent algorithms or506 edits. Axis/binning engines were already portable. Only Box/Q-Q adopt the new label snapshot.',packages=[dict(batch=b,families=f,remaining_work=work,accepted_04A29_operation_ids=[r['accepted_04A29_operation_id'] for r in rows if r['family'] in f],candidate_only=True,acceptance='Separate owner authorization and real merged-main verification; host-free source comparisons and precise native boundary before implementation') for b,f,work in packages],gated_defects=['04A.39-41 statistical/input/corner-case changes require separate authority','04A.45-48 native captures/lifetime/control discharge require separate host authorization','C17-CA-NOLABEL-INDEX-01','C13-LOGRANK-R-01','C21-STACKED-ROWCOUNT-VALIDATION-01','C24-HISTORICAL-ORACLE-SCOPE-01','C25 Love/PSM candidates','C28 output/row-ID candidates','04A29 ordinal171/172 uncertain whole-helper contracts'],enhancements=dict(detrended='04A.68-69 NOT_IMPLEMENTED',PrettyBreaks_toggle='04A.75 NOT_IMPLEMENTED; default always on retained'),next_batch_started=False))
    original=read(ROOT/'evidence/04A29/original-control-crosswalk.json')
    save('original-controls-delta.json',dict(source=pin(ROOT/'evidence/04A29/original-control-crosswalk.json'),original_population=original['original_control_population'],overlap=original['overlap'],relevant_unique_count=original['relevant_unique_count'],status='OPEN_RETAINED',native_releases=0,original_control_discharges=0,scientific_promotions=0,gates=dict(P='NO',N1='NO GO',N2='NO',Gate_C2='UNMET',Gate_C3='UNAUTHORIZED'),scientific='NOT_GOLDEN',native='NATIVE_NOT_RUN',historical_lineage=dict(unchanged=1031,relocated=19,replaced_or_renamed=5,split=5,uncertain=2,uncertain_ordinals=[171,172],unmapped=0,authority='evidence/04A29/04A01-graphics-member-crosswalk-v2.csv'),deferred_defects='All original restrictions and named C13/C17/C21/C24/C25/C28 obligations retained; extraction candidates are not correction authorization.'))

def validation(suites,deltas,focused,failures,python,nodes,commands,case_count):
    def totals(label,config):return dict(sum((Counter(s['outcomes']) for s in suites if s['label']==label and s['configuration']==config),Counter()))
    current_python=[x for x in python if x['label']==FINAL]
    py=dict(total=sum(x['total'] for x in current_python),passed=sum(x['passed'] for x in current_python),failed=sum(x['failed'] for x in current_python),errors=sum(x['errors'] for x in current_python))
    current_nodes=[x for x in nodes if x['label']==FINAL];node=dict(total=sum(x['total'] for x in current_nodes),passed=sum(x['passed'] for x in current_nodes),failed=sum(x['failed'] for x in current_nodes))
    matrix=read(RUN/f'{FINAL}-Debug-matrix.json');release=read(RUN/f'{FINAL}-Release-managed.json')
    assert matrix['source_unchanged'] and release['source_unchanged']
    builds=[read(RUN/'builds'/FINAL/c/'build.json') for c in ['Debug','Release']]
    assert all(not any(b['exit_codes']) and b['source_unchanged'] for b in builds)
    discrepancy=read(HERE/'inherited-04A29-manifest-discrepancies.json')
    blockers=['Owner disposition pending for93 inherited committed04A29 publication hash discrepancies; frozen evidence was preserved, not repaired.',
              'Unchanged04A28 frozen source-pin test now has six genuine failing subtests for the authorized Windows Graphics edits. New AST/entry-delta guards pass; owner-specific disposition is needed before calling this software batch ready. No old assertion/expected hash was changed.']
    serial={name:next(c for c in commands if c['receipt_id']==f'{FINAL}-{name}')['exit_code'] for name in ['strict','profile','docs']}
    value=dict(decision='NOT_READY',blockers=blockers,branch=git('branch','--show-current'),base=BASE,head=git('rev-parse','HEAD'),builds=[dict(configuration=b['configuration'],exit_codes=b['exit_codes'],source_unchanged=b['source_unchanged']) for b in builds],focused=dict(status='PASS',Debug=dict(Core=120,Common=7,Legacy=42),Release=dict(Core=120,Common=7,Legacy=42),unique_new_cases=40,new_raw_executions_each_configuration=76),managed=dict(entry_debug=totals('before','Debug'),final_debug=totals(FINAL,'Debug'),final_release=totals(FINAL,'Release'),existing_identity_deltas=deltas,unexpected_changed_managed_outcomes=0,inherited_raw_failures=14,entry_release='NOT_RUN: fresh Debug original-source baseline used for before/after identities; full final Release executed'),python=py,node=node,serial_controls=serial,source_ownership_guard='PASS',numeric_export_case_ids=case_count,framework_targets='netstandard2.0 / net48 unchanged',native_Excel_COM_UIA_RefEdit='NATIVE_NOT_RUN',independent_R='NOT_RUN',scientific_goldens='NOT_GOLDEN; zero promotions',browser_Office_WASM_manual='NOT_RUN',product_verifier_default='NOT_RUN unchanged because shared/historical output writers; equivalent isolated complete lanes RUN',help='NO_USER_VISIBLE_HELP_CHANGE',timings=dict(entry_debug_matrix_seconds=read(RUN/'before-Debug-matrix.json')['seconds'],final_debug_matrix_seconds=matrix['seconds'],final_release_managed_seconds=release['seconds'],historical_metadata_seconds=read(HERE/'historical-results-preservation.json')['seconds'],comparison='Actual durations only; no matched sequential-control speedup claim'),historical_04A29_helper_24_tests='REUSED_HISTORICAL_REFERENCE only; not reported as04A30 execution',index='EMPTY',gates=dict(P='NO',N1='NO GO',N2='NO',Gate_C2='UNMET'),native_releases=0,original_control_discharges=0)
    value['python_scope_qualification']='Final aggregate uses the complete final745-case scripts rerun after publication redaction, plus unchanged full Debug tools/help/Common lanes. Original full Debug raw Python838/108/10 retained in nonpasses.json.'
    value['OfficeAddin_application_build']='NOT_RUN: no Office source changes; actual host infrastructure/services/Razor components compile and test through the linked OfficeAddin.Tests project in both configurations.'
    value['WasmSmoke_application_build']='NOT_RUN: no WASM UI changes; actual linked Execution infrastructure compiles/tests in both configurations. No browser/native E2E.'
    save('validation.json',value)
    save('nonpasses.json',dict(decision='NOT_READY',acceptance_blockers=blockers,managed=dict(raw_failed=14,before_after='Same exact identities/outcomes as original-source Debug; full Release has same failures. See managed-validation-v1.json.',unexpected_new_failures=0),python=dict(**py,inherited_remaining_failures=100,inherited_errors=10,new_frozen_source_pin_subtest_failures=6,initial_audit_privacy_failures_fixed=2,full_verified_matrix_raw=dict(passed=838,failed=108,errors=10),final_scripts_rerun=dict(total=745,passed=640,failed=100,errors=5),qualification='Entry Debug Python was844pass/102fail/10error;two privacy checks initially failed on provisional04A30 metadata. Final verified full matrix was838/108/10 before synthetic security-test display payloads were redacted in the new identity export. The final745-case scripts rerun is640/100/5; together with unchanged tools/help/Common lanes this yields final840/106/10. Six authorized source-pin failures remain FAIL. All earlier receipts retained; no assertion or historical expected hash changed.'),strict_exit=serial['strict'],profile_exit=serial['profile'],strict_mkdocs_exit=serial['docs'],authority_manifest_discrepancies=discrepancy['count'],native='NATIVE_NOT_RUN',R='NOT_RUN',goldens='NOT_GOLDEN',retained_defects='All named original restrictions, ROC repeated accumulator/scatter margin/legacy definitions and historical171/172 uncertainties retained.',intermediate_runs='product/focused builds and first complete matrix retained in raw receipts; final verified label supersedes them after one XYZ empty-input preservation fix. No failed receipt was overwritten.'))
    return value

def dossier(value,rows):
    def table_files(paths):return '\n'.join('- `'+p+'`' for p in paths)
    suite_rows=read(HERE/'managed-validation-v1.json')['suites']
    table='| Suite | Entry Debug pass/fail | Final Debug pass/fail | Final Release pass/fail |\n|---|---:|---:|---:|\n'
    for suite in ['Core','Common','Legacy','OfficeAddin','OfficeExecution','ControlMitigation']:
        counts=[]
        for label,c in [('before','Debug'),(FINAL,'Debug'),(FINAL,'Release')]:
            r=next(r for r in suite_rows if r['suite']==suite and r['label']==label and r['configuration']==c)
            counts.append(str(r['outcomes'].get('Passed',0))+'/'+str(r['outcomes'].get('Failed',0)))
        table+='|'+suite+'|'+'|'.join(counts)+'|\n'
    text=f'''# Batch 04A.30 owner review

**NOT_READY — software implemented; two explicit evidence/validation dispositions remain.**

All eight opening scope families have a real Windows consumer, including ROC and ordinary normal Q–Q numerical delegation. All 40 new unique managed cases pass on both runtimes, the focused sets pass, and no existing managed outcome changed. This is not a green full matrix or native/scientific acceptance. The owner must disposition (1) the 93 inherited committed 04A.29 manifest mismatches and (2) the unchanged 04A.28 source-pin test's six failures on this batch's authorized Windows source changes. No frozen receipt, expected hash or assertion was edited. [Nonpasses](../../evidence/04A30/nonpasses.json) identifies these separately from inherited failures. The proposed decision is acceptance of **04A.30 software migration only after those dispositions and independent review**; no 04A.31 start or native release follows automatically.

## Entry, authority and publication

Branch `codex/04A-30`; real merged-main entry and unchanged HEAD `{BASE}`. That commit is the accepted 04A.29 audit, whose parent is `02a6d01cd3520116539fcefdd77612bbd9550129`; accepted 04A.28 `ed94086e449f22ee1a0fc3888d6209ecba85a8b5` and test rationalization `033cbe9033cc10bce3e19322e5582e5ea389fe22` are ancestors. Entry had no tracked edits or staged files. The first branch command was denied by the sandbox; the authorized metadata-only retry created the correct branch. No reset/stash/clean/stage/commit/merge/push occurred.

Revision 2.11's stale handoff heading is retained; the owner's acceptance controls status. AGENTS.md, the plan and original 04A.02/03 restrictions were inspected and preserved. Canonical 04A.29 v2 remains frozen: 1,031 unchanged,19 relocated,5 replaced/renamed,5 split,2 uncertain (171/172),zero unmapped. Neither uncertainty was discharged. [Entry state](../../evidence/04A30/entry-state.json), [exact entry pins](../../evidence/04A30/entry-pins.json) and [inherited manifest discrepancy list](../../evidence/04A30/inherited-04A29-manifest-discrepancies.json) qualify the actual checkout. All 93 mismatches match their committed blobs; they are not local drift and were not repaired here.

The complete proposed publication, including product/tests, this single owner dossier and all 04A.30 evidence, is [publication-manifest.json](../../evidence/04A30/publication-manifest.json). It includes itself with null bytes/SHA to avoid recursive hashing. The corrected 04A.29 canonical crosswalk and its 52-file superseded attempt are untouched. Seventy private historical path names use stable SHA-derived aliases in public entry pins; raw diagnostics and temporary implementation scaffolding remain in the fresh ignored run root.

## Operation ownership and actual integration

The [pre-edit map](../../evidence/04A30/migration-operation-map-entry.csv) was recorded before product edits. The [final operation map](../../evidence/04A30/migration-operation-map-v1.csv) gives 15 overlapping member/suboperation records, each linked to an accepted 04A.29 ID, exact entry source/body hash and actual new destination IDs. These are eight scope families, not 506 edits or 15 distinct algorithms. The Box label plan was refined to preserve the original constructor label-array alias; the [refinement receipt](../../evidence/04A30/implementation-map-refinement.json) records why.

| Family | Existing/current owner and new bounded seam | Actual Windows consumer and evidence | Later owner |
|---|---|---|---|
| ROC | New `RocDataCore` moves the original cutoff/count/AUC/Hanley/DeLong/midrank operations. Existing Core distributions/array routines reused. | `ROC.compute` applies `ComputeCurve`, then `DeLongSE` calls `ComputeDeLong`; `MidRanks` forwards. Frozen-source tests and real Framework wrapper tests pass; `wrapResults`/`addROCplot` signatures and bodies retained. |04A.31; disputed numeric policy39–41|
| Normal Q–Q | New `NormalPlotDataCore` sorts a copy and computes original probabilities/Z/reference endpoints with existing Core statistical routines. | `NormalPlot.New/compute/computeRefLIne` delegate; `addChart` retained. Three score × three reference choices, ties/constants/small/nonfinite/fallback/pre-compute cases tested. |04A.31; detrending68–69|
| Box | New `BoxSegmentsCore.Compute` returns five signed Peltier segments only. DescriptiveStat/whisker/outlier/jitter owners retained. | `CalcForPlotting` consumes scalars; frozen whole-method and Framework adapter comparisons pass. No repeated collection reset. |04A.32|
| Histogram | New `HistogramDataCore` protects existing `HistogramBinningCore` with clones and moves midpoint horizontal bounds. No second engine. | `Histogram.compute` calls copy wrappers; `addChart` uses bounds. Existing bin/overlay tests and source-bound/Framework checks pass. |04A.32; raw-bin toggle75|
| Axis | `ChartAxisScalingCore.Compute` was already migrated. No new calculator or bridge edit. | Existing `ChartingFunc.ChartScaling`→mutable `CHARTscale.Min/Max/Scale`; existing and new edge adapter tests pass. |04A.32|
| Scatter matrix | New `ScatterMatrixDataCore.Normalize` owns n×p panel-scaled values and diagonal label X vector. | `transformData` consumes results; frozen transform and Framework regression/sentinel parity pass. Constructor still forces `.1`. |04A.33|
| XYZ | New `XyzDataCore.NormalizeAxis` owns the centered-cube axis transform only. | `getnorm` calls it for aligned X/Y/Z. Source and Framework tests cover direction/manual bounds/zero span/ragged/zero-point behavior. |04A.33|
| Labels/groups | New Common `OrderedPlotGroups/OrderedPlotPoint` composes `GroupedNumericData` and accepted `UnivariateObservationSource`. | `NormalPlot.New` gets the supplied display name; Box `SnapshotGroupNames` supplies category/table labels at consumption. No parser/grouping rewrite or specialty-family mass refactor. |04A.31/32; specialty34/37|

Real entry callers were traced in `UI/UibyID.vb` (ROC/Normal/Histogram/Box), `Ui0OneRefeditMulticol.vb` (Box), `Ui11PCA.vb` (scatter), `Ui3XYZplot.vb` (XYZ), and assumption/ANOVA result adapters. Those acquisition/caller files are unchanged. Existing configurable and univariate parsers remain acquisition authority; a Common test passes their filtered physical-row sources through the new descriptor. No new host GUI or Office/Mac plotting capability is implied.

## Shapes, identity and compatibility

ROC keeps positive group0/negative group1, sorted concatenated copies, midpoint cutoffs including terminal max+1, tie0.5 credit, both CI families/alpha/table order, u cutoffs/specificities and u+1 plotted sensitivity/FPR vectors. Plot X is 1-specificity; Y is sensitivity; initial(1,1),terminal(0,0). The explicit Windows accumulator seed retains repeated `compute` accumulation. Applying the Hanley snapshot before the DeLong call preserves the characterized singleton failure state. Exact invalid-input exception types are tested; arbitrary pre-DeLong invalid-call private-state persistence is not claimed and remains a 04A.31 edge contract. Core `Compute` is first-call/stateless; its partial compatibility entry results are documented precisely in [portable contracts](../../evidence/04A30/portable-plot-contracts-v1.json).

Q–Q keeps literal **Blom / Rankit / Van Der Waerden**, **SPSS / OLS / R**, average ties, existing quartiles/rounding and `Slope/Intercept` argument order, X=data/Y=normal scores and 2×2 reference endpoints. Unknown score labels still yield zero vectors; unknown reference labels still produce zero Y endpoints. The Core snapshots/getters are copied, while the Windows returned score-vector alias remains supported through the reference-line compatibility overload. There is no detrended mode or new R semantics.

Two authorized input-only deltas are explicit: the old Normal constructor sorted the caller array; the new constructor sorts a clone. Histogram FD/overlay used to sort caller arrays indirectly via quartiles; its new wrappers clone before invoking the unchanged engines. Tests characterize the old mutations and assert new input integrity on success/error/repeat paths. Numeric source comparisons pass; no scientific correction is claimed.

Box retains the original quartile/whisker/outlier definitions and five signed segment channels. Histogram retains N×2 typed midpoint/count bins, exact PrettyBreaks default always on, overlay values and native secondary/Y-band/Tukey/event semantics. A new standalone empty `HorizontalBounds` request is rejected explicitly; native `addChart` still exits before calling it for zero bins. Scatter retains n×p orientation, p×p placement, `.1` margin overwrite, correlations/regression order and -100 break interpretation. XYZ retains raw alignment, `[-.5,.5]` normalization/directions, existing bounds/manual limits/nonzero-range policy and downstream rotations/shift/zoom conventions. A final empty-object regression was caught and corrected: count0 with Nothing axis returns an empty vector as the old loop did.

Common preserves supplied group order, raw vs display scalar types, duplicates/case/whitespace and ragged/empty/missing vectors. It does not trim/regroup/deduplicate/format, filter nonfinite observations or coerce native categories. Group ordinal, source observation index, chart index and optional physical WorksheetRow are distinct; explicit permutations preserve shuffled/filtered rows. Existing Windows constructors do not carry physical rows, so none is invented. New numeric results expose sorted values; complete source-to-sorted permutation projection remains a specific later 04A.31 contract. Box keeps the original label-array alias and snapshots its latest values on category/table consumption. Non-zero-bound CLR rank-one arrays are not silently coerced into typed vectors. New Common/Core files contain no native/filesystem/drawing/timer dependencies.

## Changed and new files

Six existing production files changed:

{table_files(WINDOWS)}

Seven new portable production files:

{table_files(PRODUCTION)}

Five new test sources and the existing Framework test project include changes:

{table_files(TESTS)}

The test project links the same new Core/Common fixture tests into Framework4.8. No existing test body or expected value was edited. The new legacy fixture files retain nine exact original method statement bodies, with namespace/access/host-field scaffolding only; their arithmetic is not a second copy of the new algorithm. The Windows Matrix wrapper's already-existing forwarding targets are reused in the fixture shim. [AST guard](../../evidence/04A30/source-ownership-guard.json) preserves every original Graphics member declaration/default, accounts for 13 bounded old-member deltas plus one private Box snapshot helper, verifies 38 compiled files and checks the new portable dependencies. Histogram's native chart tail is token-identical; other native bodies are unchanged except Box's category argument consumes the neutral label snapshot. Chart creation/series/style/Shapes/exports/events/cleanup remain Windows-owned.

## Executed validation and genuine nonpasses

{table}

Final Debug raw managed **{value['managed']['final_debug']['Passed']} PASS / {value['managed']['final_debug']['Failed']} FAIL**; full Release managed **{value['managed']['final_release']['Passed']} PASS / {value['managed']['final_release']['Failed']} FAIL**. Exact before/after identities, including duplicate raw multiplicities, have zero removed cases or changed existing managed outcomes. All new cases pass. [Managed identities](../../evidence/04A30/managed-test-identities-v1.csv) and [TRX/source receipts](../../evidence/04A30/managed-validation-v1.json) preserve the evidence. Entry Release was NOT_RUN; final Release was fully executed. The comparison to fresh original-source Debug is labeled explicitly, not presented as an entry Release run.

Focused final runs: **Core120/120,Common7/7,Legacy42/42** in each Debug and Release. These include40 new unique cases/76 new raw executions per configuration, plus unchanged histogram/violin/categorical/axis suites. [Focused identities](../../evidence/04A30/focused-validation-v1.json). [Casebook](../../evidence/04A30/graphics-compatibility-casebook-v1.json) contains {value['numeric_export_case_ids']} successful numeric case IDs with actual old/new IEEE bits, NaN/infinity flags, shapes/options/source links and runtime/TRX hashes. Error compatibility and copy/identity contracts are identified as assertion evidence; no fabricated exception/output values or scientific golden is supplied.

Node **916/916 PASS**. Python final scope aggregate **840 PASS /106 FAIL /10 ERROR**,956 runner cases. The full verified matrix itself reported838/108/10, including two later-fixed publication-privacy failures; the final745-case scripts rerun reported640/100/5 and is combined with the unchanged tools/help/Common lanes above. Entry Python was844/102/10. The two privacy checks initially saw provisional raw entry-path metadata and later synthetic credential/drive examples in security-test display names. Seventy historical path aliases and stable display hashes fixed those exports; raw identity hashes/class/method/multiplicity remain preserved. Six failures now come from the unchanged04A28 whole-source snapshot test, one per authorized Windows file. The remaining100 failures/10 errors retain their earlier identities. These six remain real FAIL subtests; their dedicated successor is the exact entry-delta/AST/call-through proof, not edited historical expected hashes. [Source/Node identities and deltas](../../evidence/04A30/source-and-node-validation-v1.json). Unittest primary descriptions were reconstructed around interleaved Git warnings; subtest failures were not discarded. Raw Windows stderr encoding is recorded where needed.

Strict audit exit **{value['serial_controls']['strict']}**, profile exit **{value['serial_controls']['profile']}**, strict MkDocs exit **{value['serial_controls']['docs']}**. No strict gate was softened. The stock verifier was inspected but **NOT_RUN unchanged**, because it writes historical `TestResults/Codex` and shared `bin/obj`. Its complete portable/Windows lanes were executed through the accepted isolated runner; no default historical destination was used. Compile-only Windows targets skipped version/help generation, XLL packaging and post-build/signing/deployment. All commands, real durations and raw diagnostic hashes are in [command-outcomes](../../evidence/04A30/command-outcomes.json). No network/package restore or SDK install occurred. The audit helper reused already-existing empty net10 SDK assets; raw scaffolding and all failed intermediate receipts remain local.

Full Debug elapsed {value['timings']['final_debug_matrix_seconds']:.2f}s; full Release managed {value['timings']['final_release_managed_seconds']:.2f}s; scoped historical metadata traversal {value['timings']['historical_metadata_seconds']:.2f}s. No speedup is claimed. Earlier product/focused runs were retained and superseded after the XYZ count0 fix; final accepted-for-review source hashes are the `verified` receipts.

## Preservation, safety and gates

All3,820 tracked entry files were pinned once. The only changed tracked files are the six approved Windows Graphics sources and the Framework test project; original12 Core Graphics algorithms, other32 Windows Graphics files, Common/Office/result writers/UDF/Ribbon/Designer/AGENTS/plan and all frozen authority remain byte-identical to actual entry. All100 mixed-model captures match their original registry. Historical TestResults has280,953 files/138,180,795,001 bytes and the same membership/size/mtime digest as the accepted receipt. Payload bytes were **NOT_REHASHED**; no stronger claim is made. [Source preservation](../../evidence/04A30/source-preservation.json).

Entry process inspection found no build/test/Excel writer and six Node processes; CIM command-line inspection was denied. Fresh isolated output/obj/log/TRX/sandbox roots and serial inner test hosts avoided shared outputs; no permanent exclusive lease is asserted. Final source protection checks found no unexpected writer change. Private diagnostics stay local; the publication privacy/trackability/index/schema checks are recorded in their receipts. Git index is empty and HEAD remains the entry commit; no publication operation was performed.

**P=NO; N1=NO GO; N2=NO; Gate C2=UNMET.** Gate C3/native releases remain unauthorized. Zero native releases, zero original-control discharges and zero golden promotions. Native Excel/COM/UIA/RefEdit/chart appearance and independent R are **NATIVE_NOT_RUN / NOT_RUN / NOT_GOLDEN**. 04A.29's24 helper passes and04A.28 matrices are historical references only. All C13/C17/C21/C24/C25/C28 candidates,171/172 uncertain contracts and original overlapping obligations remain open.

## Handoff and proposed owner smokes

[Handoff](../../evidence/04A30/graphics-roadmap-handoff-v1.json) maps remaining operations to the official04A.31/32/33 packages and retains specialty04A.34–37 ownership. There is no claim that ROC/Q–Q corner cases, all box/hist/violin aggregates or all3D/scatter geometry are complete. Histogram raw-bin/PrettyBreaks UI belongs04A.75; detrending belongs04A.68–69. Defect corrections/native closure remain separately gated04A.39–48 work. No later batch started.

Three proposed owner checks, **NOT_RUN pending separate native authorization**:

1. ROC with tied two-group observations: compare both CI tables, cutoff order, ROC/diagonal and unchanged source-sheet ordering.
2. Ordinary Q–Q from unsorted duplicates: Blom plus a reference line; verify unchanged source data and X-data/Y-score orientation.
3. Mixed-sign box/histogram or XYZ manual-bound/inversion case: compare labels, overlays/native geometry and cleanup against the reviewed source/build.

**NO_USER_VISIBLE_HELP_CHANGE**; help routes, options, installed identity and Ribbon/UDF signatures are unchanged. No Office/Mac GUI or browser E2E is newly enabled; those manual statuses remain NOT_RUN. After the two explicit evidence dispositions, send this Codex report and updated full ZIP for independent04A.30 acceptance review. Acceptance is software-only and does not authorize04A.31, scientific corrections or native observation.
'''
    (ROOT/DOSSIER).write_text(text,encoding='utf-8')

def public_paths():
    # The entry was clean; all changed/new nonignored paths must be within the
    # explicit batch surface. This includes every proposed product/test file.
    candidates=set(git('diff','--name-only').splitlines())|set(git('ls-files','--others','--exclude-standard').splitlines())
    allowed=set(PRODUCTION+WINDOWS+TESTS+[DOSSIER])
    unexpected=[p for p in candidates if p not in allowed and not p.startswith('evidence/04A30/')]
    assert not unexpected,unexpected
    assert set(PRODUCTION+WINDOWS+TESTS+[DOSSIER]).issubset(candidates)
    return sorted(candidates)

def publication_checks():
    paths=public_paths();private=[];ignored=[]
    computer=os.environ.get('COMPUTERNAME','!unset!')
    for relative in paths:
        p=ROOT/relative
        assert p.is_file(),relative
        text=p.read_bytes().decode('utf-8-sig')
        if re.search(r'[A-Za-z]:[\\/]+Users[\\/]+[^\\/\s"\']+',text,re.I) or computer.casefold() in text.casefold() or re.search(r'S-1-5-\d',text):private.append(relative)
        result=__import__('subprocess').run(['git','-c','safe.directory='+ROOT.as_posix(),'check-ignore','--no-index',relative],cwd=ROOT,stdout=__import__('subprocess').PIPE,stderr=__import__('subprocess').PIPE)
        if result.returncode==0:ignored.append(relative)
        assert result.returncode in [0,1]
    assert not private and not ignored,(private,ignored)
    assert not git('diff','--cached','--name-only')
    save('privacy.json',dict(status='PASS',publication_paths=len(paths),private_home_hostname_SID_hits=private,private_historical_path_aliases=70,raw_diagnostics='Retained only in ignored local run root; no workbook content or installed-tool private paths published',trackability=dict(status='PASS',ignored=ignored)))
    save('final-git-state.json',dict(branch=git('branch','--show-current'),head=git('rev-parse','HEAD'),main=git('rev-parse','main'),index='EMPTY',staged_paths=[],status_short=git('status','--short').splitlines(),publication_paths=paths,committed=False,merged=False,pushed=False))
    save('schema-validation.json',dict(status='PASS',checks=['38 compiled Windows files from project and disk;13 bounded old-member changes and one private helper','All original method/property block signatures/defaults retained','Nine frozen source-fixture method statement bodies match exactly','Seven new portable files pass strict/dependency guard;12 existing algorithms unchanged','15 final operation records resolve accepted IDs and current destination IDs','70 numeric case IDs resolve source operations; IEEE/NaN flags and old/new comparisons valid','Exact managed identity/multiplicity/outcome comparison; every old case retained','Every source/Python/Node suite has executed/not-run status; new frozen-pin failures explicit','Publication allowlist/trackability/privacy/index checks','All candidate files including source/tests enumerated; self-hash exception explicit']))
    manifest='evidence/04A30/publication-manifest.json'
    paths=sorted(set(public_paths())|{manifest})
    save('publication-manifest.json',dict(schema='04A30-publication-v1',self_rule='Self included;bytes/sha256 null,avoiding recursive hashing',files=[dict(path=p,bytes=None,sha256=None) if p==manifest else pin(ROOT/p) for p in paths]))
    # Qualify the metadata just generated too, including the privacy receipt and
    # manifest themselves. Refresh their pins only after the complete scan.
    complete=public_paths()
    for relative in complete:
        text=(ROOT/relative).read_bytes().decode('utf-8-sig')
        assert not re.search(r'[A-Za-z]:[\\/]+Users[\\/]+[^\\/\s"\']+',text,re.I),relative
        assert computer.casefold() not in text.casefold(),relative
        assert not re.search(r'S-1-5-\d',text),relative
    import importlib.util
    spec=importlib.util.spec_from_file_location('accepted_privacy14',ROOT/'scripts/verify-04a14-publication.py')
    policy=importlib.util.module_from_spec(spec);spec.loader.exec_module(policy)
    findings,reviewed=policy.scan(complete)
    assert not findings,findings
    privacy=read(HERE/'privacy.json');privacy['publication_paths']=len(complete);privacy['complete_metadata_scan']='PASS including this receipt, final Git/schema receipts and manifest';save('privacy.json',privacy)
    privacy['accepted_comprehensive_policy']='PASS scripts/verify-04a14-publication.py.scan: decoded JSON/base64/NFKC, owner/host, drive/UNC, SID,email,key/credential/connection/internal-host patterns'
    privacy['redacted_test_displays']='Synthetic security-test payloads replaced by stable display hashes; exact raw identity SHA and class/method/multiplicity preserved.'
    save('privacy.json',privacy)
    state=read(HERE/'final-git-state.json');state['publication_paths']=complete;save('final-git-state.json',state)
    save('publication-manifest.json',dict(schema='04A30-publication-v1',self_rule='Self included;bytes/sha256 null,avoiding recursive hashing',files=[dict(path=p,bytes=None,sha256=None) if p==manifest else pin(ROOT/p) for p in complete]))
    verify_publication()

def verify_publication():
    value=read(HERE/'publication-manifest.json');paths=[r['path'] for r in value['files']]
    assert len(paths)==len(set(paths)) and paths==sorted(public_paths())
    for record in value['files']:
        if record['path']=='evidence/04A30/publication-manifest.json':
            assert record['sha256'] is None and record['bytes'] is None
        else:assert pin(ROOT/record['path'])==record,record['path']
    guard=read(HERE/'source-ownership-guard.json')
    ids=[m['member_id'] for m in guard['current_members']];assert len(ids)==len(set(ids))
    old_ids={m['member_id'] for m in csv.DictReader((ROOT/'evidence/04A29/graphics-members-v1.csv').open(encoding='utf-8-sig',newline=''))}
    operation_rows=list(csv.DictReader((HERE/'migration-operation-map-v1.csv').open(encoding='utf-8-sig',newline='')))
    assert len(operation_rows)==15
    for row in operation_rows:
        assert row['accepted_04A29_member_id'] in old_ids
        assert all(i in ids for i in row['actual_destination_member_ids'].split(';'))
        assert row['status'] in ['IMPLEMENTED_04A30_BOUNDED_SEAM','EXISTING_CORE_BRIDGE_VERIFIED']
    assert git('branch','--show-current')=='codex/04A-30' and git('rev-parse','HEAD','main').splitlines()==[BASE,BASE]
    assert not git('diff','--cached','--name-only')
    __import__('subprocess').run(['git','-c','safe.directory='+ROOT.as_posix(),'diff','--check'],cwd=ROOT,check=True,stdout=__import__('subprocess').PIPE,stderr=__import__('subprocess').PIPE)
    print('PASS publication SHA/bytes/complete membership, source/operation IDs, unique member IDs, taxonomy, branch/index/diff checks;',len(paths),'files')

def main():
    if len(sys.argv)>1 and sys.argv[1]=='verify':verify_publication();return
    begin=time.perf_counter()
    suites,deltas,focused,failures=managed();python,nodes=python_node()
    commands=command_receipts();changed,metadata=protection();rows=operation_map()
    count=casebook(rows);contracts_and_handoff(rows)
    value=validation(suites,deltas,focused,failures,python,nodes,commands,count)
    dossier(value,rows)
    provenance=read(HERE/'legacy-fixture-provenance.json')
    entry={p['path']:p for p in read(HERE/'entry-pins.json')['files']}
    provenance['source_files']=[entry[p] for p in WINDOWS]
    provenance['fixtures']=[pin(ROOT/p) for p in TESTS[:3] if 'Legacy' in p]
    provenance['forwarding_shim']='Exact Windows Matrix forwarding target ArrayUtilities.GetColumn;CorrelationMatrixCore.Compute via reflection of the existing Friend provider. No new correlation algorithm.'
    provenance['statement_proof']='source-ownership-guard.json frozen_fixture_proofs'
    save('legacy-fixture-provenance.json',provenance)
    # Casebook points to the final provenance pin, after the source receipt refresh.
    cases=read(HERE/'graphics-compatibility-casebook-v1.json');cases['fixture_provenance']=pin(HERE/'legacy-fixture-provenance.json');save('graphics-compatibility-casebook-v1.json',cases)
    save('README.json',dict(decision='NOT_READY',dossier=DOSSIER,manifest='evidence/04A30/publication-manifest.json',authoritative_operation_map='migration-operation-map-v1.csv',casebook='graphics-compatibility-casebook-v1.json',reproduce=['Use existing restored/cache-only assets and isolated runner; do not invoke stock verifier default historical outputs','run.py focused/matrix/managed/serial/guard configuration label; raw paths stay under fresh ignored run root','finalize.py verify validates candidate membership, all SHA/bytes and references without product build','Do not rerun entry/map over accepted receipts or modify frozen04A29 authority'],elapsed_finalization_seconds=time.perf_counter()-begin))
    publication_checks()

if __name__=='__main__':main()
