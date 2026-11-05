"""Public-sanitized 04A.24 accounting over actual immutable local receipts."""
import argparse
import base64
from collections import Counter
import hashlib
import json
import os
from pathlib import Path
import re
import platform
import subprocess
import struct
import copy
import sys
import xml.etree.ElementTree as ET
import importlib.util

ROOT=Path(__file__).resolve().parents[1]
def module(name,path):
    s=importlib.util.spec_from_file_location(name,ROOT/path);m=importlib.util.module_from_spec(s);s.loader.exec_module(m);return m
c=module('capture24','scripts/capture-04a24-contract.py')
p17=module('summary17','scripts/summarize-04a17.py')
p11=module('summary11','scripts/summarize-04a11.py')
p21=module('summary21','scripts/summarize-04a21.py')
RUN,OUT=c.RUN,c.OUT
NS=p17.NS
TRX_CACHE={}
def read_trx(path):
    key=str(path)
    if key not in TRX_CACHE:TRX_CACHE[key]=p11.trx(path)
    return copy.deepcopy(TRX_CACHE[key])
def write(name,value):c.emit(name,value)
def canonical(value):return json.dumps(value,ensure_ascii=False,sort_keys=True,separators=(',',':'),allow_nan=False).encode()
def command(name):return json.loads((RUN/'commands'/name/'command.json').read_bytes())
def log(name):return '\n'.join((RUN/'commands'/name/(v+'.log')).read_text(encoding='utf-8',errors='replace') for v in ['stdout','stderr'])
def trx(label,suite):
    repaired=RUN/'executions'/('requalified-Debug-'+suite)/'results'/(suite+'.trx')
    if label=='final-Debug' and suite in ['Core','Common'] and repaired.exists():return repaired
    return RUN/'executions'/(label+'-'+suite)/'results'/(suite+'.trx')

def prepare():
    old=json.loads((ROOT/'evidence/04A23/validation.json').read_bytes())
    focused={}
    for suite in ['Core','Common','Legacy']:
        p=trx('entry-qualified-final',suite)
        if p.exists():
            d=p11.trx(p);d.pop('records');d.pop('batch11',None);focused[suite]=d
    write('entry-validation.json',dict(accepted_previous_configurations=old['configurations'],live_focused=focused,live_python23=dict(receipt='LOCAL_COMMAND:entry-python23',**command('entry-python23')),qualification='Accepted 04A.23 complete receipts reused only after exact/line-ending-only compiled-source hash reconciliation. Fresh live Debug builds and focused Core/Spc22/23 checks passed. Protected branch/publication assertions remeasured separately, not forced to historical totals. No complete live entry matrix repeated.',source_qualification='entry-source-qualification.json'))
    p21.c=c;p21.RUN=RUN;p21.OUT=OUT;p21.write=write;p21.authority()
    write('candidate-issues.json',dict(activated_behavior_fixes=0,C21_STACKED_ROWCOUNT_VALIDATION_01='OPEN / CANDIDATE_ONLY / UNAPPLIED',C24=[dict(id='C24-HISTORICAL-ORACLE-SCOPE-01',status='OPEN / CANDIDATE_ONLY / UNAPPLIED',source='BESHStat.Excel.Common.Tests/src/Spc23LegacyPreparation.vb',reproducer='BuildDiagnosticTables uses Dim signalOnly As Boolean = SignalOnly; the VB local shadows the fixture field. Compare the old helper scope=True with its explicit selected-list observation and the original Ui22 cbDiagnosticScope predicate.',retained_behavior='Original shipping predicate and the accepted 04A.23 independent SelectDiagnostics checks remain unchanged; old helper/corpus/expectations preserved byte-for-byte. New 04A.24 helper uses Me.SignalOnly to model the actual frozen UI predicate.',proposed_owner='Historical compatibility-evidence review; any publication correction requires separate owner authorization. No Core/native/GUI correction activated.',pending='Owner review of historical scope-labelled table hashes; no rerun or replacement of accepted corpora.')],predecessors='C09/C11/C12 native; C13; C14; C17/C18; C19; C20; C21 and accepted 22/23 candidates remain under their existing owners.',native='NOT_RUN / UNAUTHORIZED',R='NOT_RUN / UNAUTHORIZED',releases=0,discharges=0,P='NO',N1='NO GO',N2='NO',C2='UNMET'))
    write('parallel-preflight.json',dict(original_verifier=dict(path='scripts/verify-beshstatng.ps1',sha256=c.sha(ROOT/'scripts/verify-beshstatng.ps1'),unchanged=True),runner=dict(path='scripts/verify-04a24.py',sha256=c.sha(ROOT/'scripts/verify-04a24.py')),worker_limit=4,fifth_worker=False,builds='Sequential per configuration; offline --no-restore, /m:4. No compiler during prebuilt lanes.',lanes={
        'A':'Core then Common prebuilt containers; each copied binaries, isolated TRX/temp observations/KR exports. MaxCpuCount=1 and MSTest Workers=1 override assembly parallelism.',
        'B':'Windows then frozen ControlMitigation prebuilt containers; original bitness/apartment. Generated projection sources copied/hash-checked, never regenerated. No Excel process.',
        'C':'Office and WASM managed prebuilt containers then Office JS, resolver and WASM Node. --test-concurrency=1, no npm install/build/server/browser.',
        'D':'Read/check-only inventory/catalog/metadata/resolver/help/Office audits then one Python discovery at a time. Temporary Help copies serialized. Fresh isolated semantic observations precede Common Python.'},writers=[dict(source='Common/Windows fixed ../../../../TestResults/Codex/Batch0205/Wire and Batch0207',isolation='Copied DLL execution base resolves inside each unique execution/sandbox; asserted before launch.'),dict(source='KR export and 04A02 observation writers',isolation='Unique BESHSTAT_KR_EXPORT_DIR and BESHSTAT_04A02_OBSERVATIONS per invocation.'),dict(source='Python Help clones/temp files',isolation='Only lane D Python, one discovery at a time; no shared project compiler.'),dict(source='Strict/profile/MkDocs/publication/hash checks',isolation='Serial after lanes; MkDocs site under own run only; validators check-only.' )],coverage='Exact applicable original verifier commands adapted to build-once isolated prebuilt invocations; original verifier remains immutable. No TestResults/Codex or old reports reused.',resource_scope='OS root-command CPU/peak working set only; child/testhost aggregate memory not measured. No psutil installation. Four command roots plus bounded testhost infrastructure; no claim of four total operating-system PIDs.',ports='Selected Node tests are file/unit suites; no browser/manual server process authorized. Unknown writers serialized.',timeouts='Only current run-owned live Popen tree may be killed; abort receipt completed=false, no pass substitution. Every launched future collected even on another lane failure.'))
    pre=json.loads((OUT/'parallel-preflight.json').read_bytes())
    pre['fixture_readers']='Core and Legacy project-relative TestData are copied/hash-checked into each isolated bin-local TestData directory; original bin-local fixture priority retained. No fixture regeneration.'
    pre['observation_writers']='Frozen C02 full-suite optional writer left unset, matching the accepted earlier verifier; it only permits C02-named roots. Semantic-only 15-case probe explicitly enables a unique 04A24 observations directory; Common Python uses that directory.'
    pre['pilot_setup_corrections']=dict(initial_preflight='parallel-preflight-initial-v1.json',initial_runner_sha256=json.loads((OUT/'parallel-preflight-initial-v1.json').read_bytes())['runner']['sha256'] if (OUT/'parallel-preflight-initial-v1.json').exists() else None,Core='Initial copied Core container lacked project-relative fixtures: 71 setup failures; final Core full suite requalified after read-only fixture copying.',Common='Initial full-suite C02 optional observation writer triggered four frozen prefix guards; unset only optional full-suite writer and requalify the entire Common suite. No old assertion changed.',affected='Debug portable lane A only; other completed full Debug lanes retained. Release uses corrected runner. All failed receipts retained, no product/compiled-test change or rebuild required.')
    write('parallel-preflight.json',pre)
    write('toolchain.json',dict(python=dict(executable='<PYTHON_EXE>',version=platform.python_version(),sha256=c.sha(Path(sys.executable))),dotnet_sdk=subprocess.check_output(['dotnet','--version'],cwd=ROOT,text=True).strip(),node=subprocess.check_output(['node','--version'],cwd=ROOT,text=True).strip(),visual_studio='Installed VS2026 MSBuild18/vstest; exact version in private build stdout and sanitized command receipts.',targets=dict(Core='netstandard2.0',Common='netstandard2.0',Windows='net48',managed_office='net10.0'),packages='Existing offline packages only; no restore/download/install',native='NOT_RUN / UNAUTHORIZED'))
    sources={}
    for project,explicit in [('BESHStat.Core/BESHStat.Core.vbproj',False),('BESHStat.Excel.Common/BESHStat.Excel.Common.vbproj',False),('BESHstatNG/BESHStatNG.vbproj',True),('BESHStat.Core.Tests/BESHStat.Core.Tests.vbproj',False),('BESHStat.Excel.Common.Tests/BESHStat.Excel.Common.Tests.vbproj',False),('BESHStatNG.Test/BESHStatNG.Tests.vbproj',True),('BESHStat.OfficeAddin/tests/OfficeAddin.Tests.csproj',False),('BESHStat.WasmSmoke/tests/Execution/Execution.Tests.csproj',False)]:
        path=ROOT/project;tree=ET.parse(path);files=set()
        for node in tree.iter():
            v=node.get('Include','')
            if node.tag.split('}')[-1]=='Compile' and v and '*' not in v and '$(' not in v:
                p=(path.parent/v.replace('\\','/')).resolve()
                if p.is_file():files.add(p)
        if not explicit:
            files|={p for p in path.parent.rglob('*') if p.suffix in ['.vb','.cs'] and not set(s.lower() for s in p.relative_to(path.parent).parts)&{'bin','obj','packages','testresults','node_modules'}}
        baseline=json.loads((RUN/'baseline.json').read_bytes())['pins']
        oldtree=ET.fromstring(subprocess.check_output(['git','-c','safe.directory='+ROOT.as_posix(),'show',c.BASE+':'+project],cwd=ROOT))
        entryfiles=set()
        for node in oldtree.iter():
            v=node.get('Include','')
            if node.tag.split('}')[-1]=='Compile' and v and '*' not in v and '$(' not in v:
                p=(path.parent/v.replace('\\','/')).resolve()
                if p.is_relative_to(ROOT) and p.relative_to(ROOT).as_posix() in baseline:entryfiles.add(p.relative_to(ROOT).as_posix())
        if not explicit:entryfiles|={p for p in baseline if p.startswith(path.parent.relative_to(ROOT).as_posix()+'/') and p.endswith(('.vb','.cs')) and not set(s.lower() for s in Path(p).parts)&{'bin','obj','packages','testresults','node_modules'}}
        sources[project]=dict(entry_project_sha256=baseline[project],entry_files={p:baseline[p] for p in sorted(entryfiles)},project_sha256=c.sha(path),explicit_compile=explicit,files={p.relative_to(ROOT).as_posix():c.sha(p) for p in sorted(files)})
    write('project-compilation-inventory.json',dict(projects=sources,ownership='New Common files implicit SDK compile; Windows test five explicit Compile links; Windows Ui22 existing compiled member. Framework/package/target/project-reference settings unchanged.'))
    source()

def source():
    pins=json.loads((RUN/'baseline.json').read_bytes())['pins'];changed=set(c.git('diff','--name-only').splitlines())
    allowed={c.FORM,'BESHStatNG.Test/BESHStatNG.Tests.vbproj','BESHStatNG_Help_MkDocs/docs/methods/multivariate-control-charts.md'}
    assert changed<=allowed,changed-allowed
    for p,h in pins.items():
        if p not in allowed:assert c.sha(ROOT/p)==h,p
    for p,r in json.loads((OUT/'frozen-capture-hashes.json').read_bytes())['files'].items():assert c.sha(ROOT/p)==r['sha256'],p
    before=json.loads((OUT/'prechange-method-provenance.json').read_bytes())['methods'];after=c.methods((ROOT/c.FORM).read_text(encoding='utf-8-sig'))
    delta={n:dict(before=r['sha256'],after=hashlib.sha256(after[n].encode()).hexdigest() if n in after else None) for n,r in before.items() if n not in after or hashlib.sha256(after[n].encode()).hexdigest()!=r['sha256']}
    for n in ['BuildRequest','btCompute_Click','BuildAppearanceOptions','CreateResultWorkbook','CreateResultSheet','WriteResultTables']:
        assert n in after and before[n]['body']==after[n],n
    write('source-preservation.json',dict(base=c.BASE,modified={p:dict(entry=pins[p],final=c.sha(ROOT/p)) for p in sorted(changed)},method_changes=delta,all_other_tracked_entry_hashes_unchanged=True,frozen_authority_files=1666,core_ui21_renderer_prior_common_designers_udfs_ribbon_targets_metadata_unchanged=True,original_native_tails='Excel marshalling, reference-label scanning/placement, zones, axis scale, legend, chart rollback and styling unchanged; not native verified.',index_empty=not bool(c.git('diff','--cached','--name-only'))))
    page='BESHStatNG_Help_MkDocs/docs/methods/multivariate-control-charts.md'
    write('documentation-audit.json',dict(page=page,entry_sha256=pins[page],final_sha256=c.sha(ROOT/page),metadata_sha256=pins['BESHStatNG_Help_MkDocs/docs/_meta/methods.yml'],metadata_unchanged=True,published_examples_screenshots_workbooks_unchanged=True,changes='Variable/component axes; sequence/label/physical-row distinctions; masks/stage ID boundary; exact warning order and Windows-only availability.',availability='Windows Excel-DNA only; Office.js/WASM/Mac GUI not introduced.',strict_mkdocs={p.parent.name:json.loads(p.read_bytes())['exit_code'] for p in (RUN/'commands').glob('*mkdocs/command.json')}))

def columns(nodes,extra=None):
    keys=sorted({k for n in nodes for k in n.attrib});data=[]
    for n in nodes:
        row=[n.attrib.get(k) for k in keys]
        if extra:row+=extra(n)
        data.append(row)
    return dict(columns=keys,data=data)

def compact(root):
    """Lossless semantic columns over emitted XML; private TRX retains raw XML.

    No number is parsed/rounded: attributes/text retain exact original strings,
    CLR cell types and IEEE-754 hexadecimal bits. Panel payloads are shared only
    after exact canonical-content equality inside this one presentation.
    """
    out=dict(root.attrib)
    out['source']={e.tag:e.text for e in root if e.tag not in ['requested_options','presentation','exception']}
    out['requested_options']=[dict(e.attrib,value=e.text) for e in root.findall('requested_options/option')]
    ex=root.find('exception')
    if ex is not None:out['exception']=dict(ex.attrib,message=ex.text)
    p=root.find('presentation')
    if p is None:return out
    v=dict(p.attrib);v['variables']=[e.text for e in p.findall('variables/name')];v['warnings']=[e.text for e in p.findall('warnings/warning')]
    v['hashes']={e.tag:e.text for e in p if e.tag.endswith('_sha256')}
    v['sections']=[]
    for s in p.findall('section'):
        section=dict(s.attrib,tables=[])
        for t in s.findall('table'):
            table=dict(t.attrib,metadata_sha256=t.findtext('metadata_sha256'),row_hashes=[[r.attrib['index'],r.text] for r in t.findall('row_hash')],typed_rows=[[r.attrib['index'],[[cell.attrib['type'],cell.text] for cell in r.findall('cell')]] for r in t.findall('row')])
            section['tables'].append(table)
        v['sections'].append(section)
    v['projections']=[];v['panel_payloads']={}
    for q in p.findall('projection'):
        projection=dict(q.attrib,axis=columns(q.findall('axis'),lambda n:[[r.text for r in n.findall('source_rows/row')]]),boundaries=[dict(b.attrib,text=b.text) for b in q.findall('boundary')],panels=[])
        projection['axis']['extra_columns']=['source_rows']
        for n in q.findall('panel'):
            panel=dict(n.attrib,channels=[dict(ch.attrib,values=[e.text for e in ch.findall('v')]) for ch in n.findall('channel')],identities=columns(n.findall('identity'),lambda e:[[r.text for r in e.findall('source_rows/row')]]),occurrences=[dict(e.attrib,contributors=[i.text for i in e.findall('contributors/i')],marked=[i.text for i in e.findall('marked/i')]) for e in n.findall('occurrence')])
            panel['identities']['extra_columns']=['source_rows']
            for tag in ['point_labels','signal_labels','exclusion_labels']:panel[tag]=[[e.attrib['slot'],e.text] for e in n.findall(tag+'/label')]
            key=hashlib.sha256(canonical(panel)).hexdigest()
            if key in v['panel_payloads']:assert panel==v['panel_payloads'][key]
            v['panel_payloads'][key]=panel;projection['panels'].append(key)
        v['projections'].append(projection)
    out['presentation']=v
    return out

def corpus(labels,phase):
    records=[];payloads={};families={}
    reference_path=ROOT/'evidence/04A23/multivariate-spc-same-core-comparisons-v1.json'
    reference=json.loads(reference_path.read_bytes());reference_sha=c.sha(reference_path);negative_reference={}
    for row in reference['records']:
        p=reference['payloads'][row['profile_id']]
        if 'exception' in p:negative_reference[(row['tfm'],p['id'],p['culture'])]=(row['profile_id'],p)
    for config,label in labels.items():
        for suite,tfm in [('Common','net10.0'),('Legacy','net48')]:
            path=trx(label,suite);digest=c.sha(path)
            for result in ET.parse(path).findall('.//'+NS+'UnitTestResult'):
                if 'Spc24' not in result.attrib['testName'] and not result.findall('.//'+NS+'StdOut'):continue
                for out in result.findall('.//'+NS+'StdOut'):
                    for line in (out.text or '').splitlines():
                        if not line.startswith('04A24_XML|'):continue
                        assert result.attrib['outcome']=='Passed',result.attrib['testName']
                        xml=base64.b64decode(line.split('|',1)[1]).decode('utf-8');root=ET.fromstring(xml)
                        # These are pinned public fixtures/Core messages, not raw
                        # logs. Preserve values exactly; the complete candidate
                        # receives the decoded-JSON public privacy scan at seal.
                        payload=dict(compact(root),id=root.attrib['id'],chart=root.attrib.get('chart','SYNTHETIC'),culture=root.attrib.get('culture',''),outcome=root.attrib['outcome'],status='NOT_GOLDEN',purpose='SAME_CORE_COMPATIBILITY_ONLY')
                        if payload['outcome']=='PRESERVED_REJECTION':
                            key_before,before=negative_reference[(tfm,payload['id'],payload['culture'])]
                            assert payload['exception']['type']==before['exception']['type']
                            assert payload['exception']['message']==before['exception']['message']
                            if before.get('dataset_typed_sha256'):assert payload['source']['dataset_sha256']==before['dataset_typed_sha256']
                            payload['rejection_reference']=dict(path='evidence/04A23/multivariate-spc-same-core-comparisons-v1.json',sha256=reference_sha,profile_id=key_before,stage=before['exception'].get('stage'),classification=before['outcome'],parameter=before['exception'].get('parameter'),qualification='Boundary classification from frozen prechange same-fixture observation, joined by runtime/id/culture with exact exception type/message and dataset hash where recorded. Current Core/request sources unchanged; not independent scientific validation.')
                        key=hashlib.sha256(canonical(payload)).hexdigest()
                        if key in payloads:assert payloads[key]==payload
                        payloads[key]=payload
                        records.append(dict(profile_id=key,configuration=config,tfm=tfm,runtime=root.attrib.get('runtime','synthetic'),test_identity=result.attrib['testName'],test_outcome=result.attrib['outcome'],raw_xml_sha256=hashlib.sha256(xml.encode()).hexdigest(),raw_trx_alias='LOCAL_TRX:'+path.parents[1].name,raw_trx_sha256=digest))
                        p=root.find('presentation')
                        if p is not None and payload['outcome']=='POSITIVE_PARITY':families.setdefault(payload['chart'],set()).add((p.attrib['grouped'],p.attrib['correlation'],p.attrib['rank'],p.attrib['pseudo'],p.attrib['retained']))
    assert records and set(payloads)=={r['profile_id'] for r in records}
    expected={str(i):2 for i in range(360)}|{'rank-'+str(i):2 for i in range(36)}|{'published-'+str(i):1 for i in range(7)}|{'synthetic-sparse':1}
    for config in labels:
        for tfm in ['net10.0','net48']:
            assert dict(Counter(payloads[r['profile_id']]['id'] for r in records if r['configuration']==config and r['tfm']==tfm))==expected,(config,tfm,'missing/extra profile')
    counts=dict(Counter(payloads[r['profile_id']]['outcome'] for r in records))
    # Compact canonical payloads retain typed XML and every ordered row hash.
    doc=c.clean(dict(version=1,phase=phase,status='NOT_GOLDEN',purpose='SAME_CORE_COMPATIBILITY_ONLY',promoted=False,record_count=len(records),payload_count=len(payloads),outcomes=counts,records=records,payloads=payloads,tolerance='Exact typed-value/IEEE-754 bits and full table output-model equality, no tolerance. Same returned Core result, including identical elapsed time in assertions. Only exported timing cells normalize to a volatile marker for stable corpus hashes. Expected Framework exceptions rehydrate XML-normalized LF to CRLF; raw message hashes retained. Rejections/synthetic API records are not successful statistical parity.',hashing='SHA-256 of canonical UTF-8 JSON, sort_keys=True, ensure_ascii=False, comma/colon separators. Ordered typed columns preserve original attribute/text strings, category CLR types, hexadecimal Double bits and source lists without parsing/rounding. Per-presentation identical panels share canonical hashes; large tables retain every typed row hash plus header/representative cells. Full exact cell comparisons execute before export; raw XML hashes and private TRX preserve provenance.',fixture=dict(path='BESHStatNG_Help_MkDocs/docs/assets/data/116multivariatecontrolcharts/116multivariatecontrolcharts.csv',sha256=c.sha(ROOT/'BESHStatNG_Help_MkDocs/docs/assets/data/116multivariatecontrolcharts/116multivariatecontrolcharts.csv'),mapping='Spc23Data/Fixture columns Temperature_C, Pressure_kPa, Flow_L_min; deterministic 360 profiles and 36 rank/historical profiles, two cultures. Published7 monitoring/exclusion variants; synthetic sparse two-panel API result explicitly separate. All original 04A23/22 corpora read-only.')))
    # Four bounded JSON buckets avoid a near-limit monolithic Git blob. Each
    # payload remains content-addressed; the index pins every bucket's bytes.
    values=doc.pop('payloads');files=[]
    for bucket in range(4):
        selected={k:v for k,v in values.items() if int(k[0],16)%4==bucket}
        path=OUT/('postfit-payload-bucket-'+str(bucket)+'-v1.json')
        path.write_bytes(json.dumps(dict(version=1,status='NOT_GOLDEN',payloads=selected),ensure_ascii=False,separators=(',',':'),allow_nan=False).encode()+b'\n')
        files.append(dict(path=path.relative_to(ROOT).as_posix(),sha256=c.sha(path),payload_count=len(selected)))
    doc['payload_files']=files
    (OUT/'spc-multivariate-postfit-same-core-comparisons-v1.json').write_bytes(json.dumps(doc,ensure_ascii=False,separators=(',',':'),allow_nan=False).encode()+b'\n')
    reference=dict(corpus='spc-multivariate-postfit-same-core-comparisons-v1.json',sha256=c.sha(OUT/'spc-multivariate-postfit-same-core-comparisons-v1.json'),phase=phase,status='NOT_GOLDEN / SAME_CORE_COMPATIBILITY_ONLY',outcomes=counts)
    write('multivariate-chart-family-projection-v1.json',dict(**reference,actual_families={k:sorted(v) for k,v in families.items()},UI_choices=6,core='Only actual returned panels; six branches currently one panel each. Synthetic multi-panel characterization is not UI activation.',series='Accepted 04A22 ten-channel/typed-axis/marker/label/stage projection consumed by existing renderer four-argument overload.'))
    write('multivariate-model-covariance-report-parity-v1.json',dict(**reference,sections='Exact Summary/Model/Audit table headings/cell CLR types/order/omissions, model matrices/eigenpair returned dimensions, requested versus effective rank/pseudoinverse/retention. No covariance/inverse/limit estimator in Common.'))
    write('multivariate-signal-warning-chartdata-parity-v1.json',dict(**reference,sections='Chart Data, Signals and all/signal-only Diagnostics including optional vectors/subgroup matrices, warning first-seen Ordinal union; no signal reevaluation.',retained_native='Last finite reference label selection/formatting, axes/zones/legend/pixels/style remain Windows. Portable reference channels and masks consumed unchanged; no native appearance proof.'))
    print('Corpus',phase,len(records),len(payloads),counts,flush=True)

def measure(config):
    label='final-'+config;suites=[read_trx(trx(label,s)) for s in ['Core','OfficeAddin','OfficeExecution','Common','Legacy','ControlMitigation']]
    groups=p17.typed_groups(suites);weight=p17.prior.weight
    logical=dict(total=sum(weight(k) for k in groups),passed=sum(weight(k) for k,r in groups.items() if all(v['outcome']=='Passed' for v in r)))
    logical['nonpassed']=logical['total']-logical['passed']
    inventory=[dict(identity=repr(k),weight=weight(k),**{n:r[0][n] for n in ['class_name','method','test_name']},outcomes=[v['outcome'] for v in r],source_hashes=r[0].get('source_hashes',[])) for k,r in groups.items()]
    write('final-'+config+'-logical-test-identities.json',dict(cases=inventory,accounting='Accepted exact shared-source/class/method/data-row dedup and typed null/empty cohort weights. Additional focused/semantic repeats excluded.'))
    raw=[];new={k:r for k,r in groups.items() if any('Spc24' in v['class_name'] for v in r)}
    for suite in suites:
        tree=ET.parse(ROOT/suite['path']);messages={n.attrib['testName']:n.findtext('.//'+NS+'Message','') for n in tree.findall('.//'+NS+'UnitTestResult') if n.attrib['outcome']!='Passed'}
        for row in suite['nonpasses']:row['message']=messages.get(row['test_name'],'')
        raw.append(dict(suite=suite['path'],sha256=suite['sha256'],records=suite.pop('records')));suite.pop('batch11',None)
    write('final-'+config+'-raw-test-identities.json',dict(suites=raw))
    pylogs=[RUN/'commands'/(label+'-python-'+n)/'stderr.log' for n in ['tools','help','scripts']]+[RUN/'commands'/(label+'-common-python')/'stderr.log']
    py=p17.python(pylogs);py['failure_methods']=sum(v=='FAIL' for v in py['nonpasses'].values());py['error_methods']=sum(v=='ERROR' for v in py['nonpasses'].values())
    py['raw_failure_events']=sum(v['outcome']=='FAIL' for v in py['events']);py['raw_error_events']=sum(v['outcome']=='ERROR' for v in py['events'])
    old_inventory=json.loads((ROOT/'evidence/04A23'/('final-'+config+'-logical-test-identities.json')).read_bytes())['cases']
    # Existing weighted null/empty cohorts have an order-dependent displayed
    # representative. Their accepted canonical key includes exact source SHA,
    # authored method/choice and weight; compare that key, never discard a case.
    signature=lambda r:('canonical-typed-null-cohort',r['identity'],r['weight']) if 'shared-typed-null-cohort' in r['identity'] else (r['class_name'].split('.')[-1],r['method'],r['test_name'].replace('"',''),r['weight'])
    assert Counter(signature(r) for r in c.clean(inventory) if 'Spc24' not in r['class_name'])==Counter(signature(r) for r in old_inventory),'old logical identity/weight drift'
    for suite in ['Common','Legacy']:
        focused='focused-07' if config=='Debug' else 'focused-08'
        identities=lambda path:Counter((r['class_name'].split('.')[-1],r['method'],r['test_name'].replace('"','')) for r in read_trx(path)['records'] if 'Spc24' in r['class_name'])
        assert identities(trx(label,suite))==identities(trx(focused,suite)),(config,suite,'new test discovery/execution drift')
    traces=[]
    for p in pylogs:
        for part in re.split(r'^={60,}\s*$',p.read_text(encoding='utf-8',errors='replace'),flags=re.M):
            m=re.match(r'\s*(FAIL|ERROR): ([^\n]+)',part)
            if m:traces.append(dict(receipt=p.relative_to(RUN).as_posix(),identity=m[2],outcome=m[1],trace=part.split('\nRan ')[0]))
    write('final-'+config+'-python-nonpass-traces.json',dict(traces=traces))
    nodes='\n'.join(log(label+'-'+n) for n in ['office-js','resolver','wasm-js'])
    return dict(managed_suites=suites,logical_cases=logical,runtime_executions=dict(total=sum(int(s['counters']['total']) for s in suites),passed=sum(int(s['counters']['passed']) for s in suites),nonpassed=sum(int(s['counters']['total'])-int(s['counters']['passed']) for s in suites)),new_logical_total=sum(weight(k) for k in new),new_logical_passed=sum(weight(k) for k,r in new.items() if all(v['outcome']=='Passed' for v in r)),node_total=sum(map(int,re.findall(r'\btests (\d+)\s*$',nodes,re.M))),node_passed=sum(map(int,re.findall(r'\bpass (\d+)\s*$',nodes,re.M))),python=py,lanes=json.loads((RUN/'lanes'/(label+'-summary.json')).read_bytes()))

def final():
    configurations={v:measure(v) for v in ['Debug','Release']};entry=json.loads((ROOT/'evidence/04A23/validation.json').read_bytes())['configurations']
    identity=lambda r:(r['class_name'].split('.')[-1],r['method'],r['test_name'].replace('"',''))
    for v,d in configurations.items():
        before={identity(r) for s in entry[v]['managed_suites'] for r in s['nonpasses']};after={identity(r) for s in d['managed_suites'] for r in s['nonpasses']}
        d['new_managed_nonpasses']=sorted(after-before);d['removed_managed_nonpasses']=sorted(before-after)
        d['new_python_nonpasses']={k:r for k,r in d['python']['nonpasses'].items() if k not in entry[v]['python']['nonpasses']}
        d['changed_python_nonpass_outcomes']={k:dict(before=entry[v]['python']['nonpasses'][k],after=r) for k,r in d['python']['nonpasses'].items() if k in entry[v]['python']['nonpasses'] and entry[v]['python']['nonpasses'][k]!=r}
        print(v,d['logical_cases'],d['runtime_executions'],'new',d['new_logical_passed'],d['new_logical_total'],'Node',d['node_passed'],d['node_total'],'Python',d['python']['passed'],d['python']['total'],flush=True)
    write('validation.json',dict(configurations=configurations,full_status='NONZERO / NONPASSES_RETAINED',native='NOT_RUN / UNAUTHORIZED',R='NOT_RUN / UNAUTHORIZED',golden_promotion=False))
    write('nonpasses.json',dict(entry_accepted_qualification=entry,final={v:dict(managed=[r for s in d['managed_suites'] for r in s['nonpasses']],python=d['python']['nonpasses'],new_managed=d['new_managed_nonpasses'],new_python=d['new_python_nonpasses']) for v,d in configurations.items()},classification='Inherited identities remain nonpasses. Newly triggered old Spc23 compiled chart-wrapper call-layout pins and Python exact Ui22 table/helper pins follow authorized transfer; new 04A24 compiled call-site and exact functional successors qualify transitive behavior. No frozen test was modified. Actual exceptions retained, no nonzero exit converted to pass.'))
    write('entry-final-test-identities.json',dict(entry='entry-validation.json',entry_source_reconciliation='entry-source-qualification.json',final={v:dict(logical='final-'+v+'-logical-test-identities.json',raw='final-'+v+'-raw-test-identities.json',python_traces='final-'+v+'-python-nonpass-traces.json') for v in configurations},counts={v:dict(accepted_entry=entry[v]['logical_cases'],final=d['logical_cases'],new_managed_nonpasses=d['new_managed_nonpasses'],new_python_nonpasses=d['new_python_nonpasses']) for v,d in configurations.items()}))
    corpus({v:'final-'+v for v in configurations},'FINAL')
    inventory();source();retain();receipts();closeout()

def inventory():
    cases={}
    for config in ['Debug','Release']:
        for suite in ['Common','Legacy']:
            path=trx('final-'+config,suite)
            receipt_sha=c.sha(path)
            for r in read_trx(path)['records']:
                if 'Spc24' not in r['class_name']:continue
                key=r['class_name'].split('.')[-1]+'|'+r['test_name'].replace('"','')
                source='BESHStatNG.Test/src/Spc24AdapterTests.vb' if 'Adapter' in r['class_name'] else 'BESHStat.Excel.Common.Tests/src/Spc24Tests.vb'
                row=cases.setdefault(key,dict(source=source,source_sha256=c.sha(ROOT/source),method=r['method'],requirements=['3.1:family/axes','3.2:warning/chart/data/signal','3.3:model/diagnostics','3.4:compiled shipping consumption','4:legacy/exact/culture/negative/immutable'],production=['SpcMultivariateReportProjection','SpcMultivariateChartProjectionPolicy','SpcMultivariateFamilyPresentation','SpcMultivariateWarningProjection','Ui22.WriteResultsToNewWorkbook','SpcControlChartExcel.AddCharts/MarshalPanel'],executions=[]))
                row['executions'].append(dict(configuration=config,suite=suite,outcome=r['outcome'],trx_sha256=receipt_sha))
    assert cases and all(all(e['outcome']=='Passed' for e in r['executions']) for r in cases.values())
    python='scripts/tests/test_04a24_multivariate_payload.py';py=re.findall(r'    def (test_\w+)\(self\):',(ROOT/python).read_text());pycases={}
    for n in py:
        executions=[]
        for config in ['Debug','Release']:
            text=log('final-'+config+'-python-scripts');match=re.search(r'^'+re.escape(n)+r' \(test_04a24_multivariate_payload\.[^\n]+\) \.\.\. (\w+)\s*$',text,re.M)
            assert match and match[1]=='ok',(config,n,'not actually passed')
            executions.append(dict(configuration=config,outcome='PASS',receipt='LOCAL_LOG:final-'+config+'-python-scripts'))
        pycases[n]=dict(source=python,sha256=c.sha(ROOT/python),requirement=n,executions=executions)
    write('new-test-inventory.json',dict(logical_count=len(cases),cases=cases,python_count=len(py),python_cases=pycases,counting='Linked Common cases once logically, twice raw. Full-case identities equal both focused discovery/execution sets. All old logical identities and typed null/empty cohort weights equal accepted 04A23. Negative tests pass only when exact preserved rejection is asserted; rejected Fits separately counted in corpus, never positive statistical parity. Corpus profiles are observations, not extra test identities.'))

def receipts():
    rows=[]
    for p in sorted((RUN/'commands').glob('*/command.json')):
        d=json.loads(p.read_bytes());d.update(local_receipt_sha256=c.sha(p),stdout_alias='LOCAL_LOG:'+p.parent.name+'/stdout',stderr_alias='LOCAL_LOG:'+p.parent.name+'/stderr');rows.append(d)
    write('command-outcomes.json',dict(commands=rows,qualification='All successful/failed/setup/zero-test/transient command receipts retained. Direct initial compilation and one-off setup diagnostics additionally remain in local run/tool transcript; never counted as full matrix passes.'))
    lanes=[json.loads(p.read_bytes()) for p in sorted((RUN/'lanes').glob('final-*-summary.json'))]
    requalified=[command('requalified-Debug-'+s) for s in ['Core','Common'] if (RUN/'commands'/('requalified-Debug-'+s)/'command.json').exists()]
    write('parallel-validation-audit.json',dict(preflight='parallel-preflight.json',initial_preflight='parallel-preflight-initial-v1.json',runner_sha256=c.sha(ROOT/'scripts/verify-04a24.py'),initial_runner_sha256=c.sha(RUN/'drafts/verify-04a24-initial.py'),configurations=lanes,debug_portable_requalification=requalified,debug_requalification_test_command_seconds=sum(r['seconds'] for r in requalified),matrix_wall_seconds=sum(r['seconds'] for r in lanes),builds=[dict(name=p.parent.name,seconds=d['seconds'],exit=d['exit_code']) for p in (RUN/'commands').glob('final-build-*/command.json') for d in [json.loads(p.read_bytes())]],resource_observations=dict(max_recorded_root_working_set_bytes=max((r.get('metrics') or {}).get('peak_root_working_set_bytes') or 0 for r in rows),max_concurrent_command_roots=max([v['max_active_command_roots'] for v in lanes] or [0]),timeouts=[r['name'] for r in rows if not r['completed']]),performance='Four command roots, each single test worker; associated testhost infrastructure is not counted as only four total OS PIDs. Measured wall-clock and root-process counters only. Initial Debug pilot includes rejected setup results; qualified Debug substitutes entire repaired Core/Common receipts. Sum of observed lane times estimates serial lane scheduling; no independent serial benchmark/speedup claimed. Requalification command time excludes container setup. No fifth worker, native process or simultaneous compilers.'))

def retain():
    history=json.loads((RUN/'historical-metadata.json').read_bytes());expected={r['path'] for r in history};current=set();errors=[]
    for folder in [ROOT/'TestResults',*(ROOT/p/'TestResults' for p in ['BESHStat.Core.Tests','BESHStat.Excel.Common.Tests','BESHStatNG.Test'])]:
        for parent,_,files in os.walk(folder):
            for name in files:
                p=Path(parent)/name
                if not p.is_relative_to(ROOT/'TestResults/04A-24'):current.add(p.relative_to(ROOT).as_posix())
    assert current==expected,('membership',sorted(current-expected)[:5],sorted(expected-current)[:5])
    for r in history:
        s=(ROOT/r['path']).stat()
        if (s.st_size,s.st_mtime_ns,s.st_ctime_ns,s.st_ino,s.st_dev)!=tuple(r[k] for k in ['bytes','mtime_ns','ctime_ns','ino','dev']):errors.append(r['path'])
    assert not errors,errors[:10]
    write('retention.json',dict(status='PASS_HISTORICAL_MEMBERSHIP_METADATA',historical_files=len(history),membership_metadata_errors=0,snapshot_sha256=c.sha(RUN/'historical-metadata.json'),scope='Every preexisting TestResults path, size, write/creation nanoseconds and filesystem identity. Frozen source/evidence/captures separately SHA-256 rechecked; not a reread of all historical result bytes.',retained_through='End of Phase04A'))
    builds={}
    for v in ['Debug','Release']:
        p=RUN/'builds'/('final-build-'+v+'.json')
        if p.exists():
            d=json.loads(p.read_bytes())
            for n,h in d['source_pins'].items():assert c.sha(ROOT/n)==h,n
            builds[v]=dict(receipt_sha256=c.sha(p),source_pins=d['source_pins'])
    write('final-source-qualification.json',dict(configurations=builds,compiled_sources_unchanged_after_final_suites=bool(len(builds)==2),final_input_pins={p:c.sha(ROOT/p) for p in sorted(set(c.git('diff','--name-only').splitlines())|set(c.git('ls-files','--others','--exclude-standard').splitlines())) if p.endswith(('.vb','.vbproj')) or '/tests/' in p and p.endswith('.py') or p.endswith('.md') and not p.startswith('docs/architecture/batch-04A-24')},evidence_only_rechecks='Accounting/dossier changes after matrices require terminal manifest/privacy/source contracts, not a rebuild. No behaviorally compiled edits after final build pins.',native='NOT_RUN / UNAUTHORIZED'))

def closeout():
    pins=json.loads((RUN/'baseline.json').read_bytes())['pins']
    captures=json.loads((ROOT/'evidence/04A14/baseline-registry.json').read_bytes())['files']
    for p,row in captures.items():assert c.sha(ROOT/p)==row['sha256'],p
    write('mixed-capture-preservation.json',dict(files=captures,count=len(captures),status='NOT_GOLDEN / PRESERVED_EXACT',promotion=False))
    preimage=RUN/'preimages/Ui22MultivariateControlCharts.vb';assert c.sha(preimage)==pins[c.FORM]
    raw=preimage.read_text(encoding='utf-8-sig');members=[]
    for m in re.finditer(r'^    (?:Private|Public|Friend) (?:Shared )?(Sub|Function) (\w+)\b.*?^    End \1',raw,re.M|re.S):members.append(dict(name=m[2],signature=m[0].splitlines()[0],first_line=raw[:m.start()].count('\n')+1,last_line=raw[:m.end()].count('\n')+1,sha256=hashlib.sha256(m[0].encode()).hexdigest()))
    write('prechange-overload-provenance-v1.json',dict(source=c.FORM,entry_source_sha256=c.sha(preimage),members=members,scope='Supplemental complete member list including both NullableValue overloads, derived from the exact entry preimage and original main. The initial named-method map has one key per name; full-file hash and this list preserve overload distinction. LF-normalized method hashes; original file bytes remain private and pinned.'))
    gates={}
    for name in ['strict','profile','mkdocs']:
        label='final-serial-'+name;d=command(label);gates[name]=dict(command=d['command'],exit_code=d['exit_code'],completed=d['completed'],seconds=d['seconds'],stdout=log(label).splitlines(),stdout_sha256=d['stdout_sha256'],stderr_sha256=d['stderr_sha256'])
    write('gate-validation.json',dict(commands=gates,prior='evidence/04A23/gate-validation.json',observations='Same inherited strict stale UibyID/NormalPlot review dependency and profile INTEGRITY_FAILURE with empty error as accepted 04A23. Both actual nonzero exits retained; not labelled passing. Strict MkDocs exit separately recorded.',P='NO',N1='NO GO',N2='NO',C2='UNMET',C3='UNCHANGED',E1_E7='UNCHANGED',native_releases=0,deferred_discharges=0,native='NOT_RUN / UNAUTHORIZED',R='NOT_RUN / UNAUTHORIZED'))
    args=['git','-c','safe.directory='+ROOT.as_posix()]
    rawgit={n:subprocess.check_output(args+v,cwd=ROOT,text=True) for n,v in dict(status=['status','--porcelain=v1'],diff=['diff','--name-status'],index=['diff','--cached','--name-only'],worktrees=['worktree','list','--porcelain'],ignored=['status','--short','--ignored','--untracked-files=normal']).items()}
    (RUN/'final-git-raw.json').write_text(json.dumps(rawgit,indent=2),encoding='utf-8')
    assert not rawgit['index']
    assert subprocess.run(args+['diff','--check'],cwd=ROOT).returncode==0
    assert c.git('rev-parse','HEAD')==c.git('rev-parse','main')==c.BASE
    assert subprocess.run(args+['merge-base','--is-ancestor','main','HEAD'],cwd=ROOT).returncode==0
    # Ignored local filenames/worktree details can contain workstation/account
    # identifiers despite path-prefix redaction. Keep exact raw state private;
    # publish complete candidate paths and content-derived ignored-path aliases.
    public_git=dict(status=rawgit['status'],diff=rawgit['diff'],index=rawgit['index'],worktree_root='<REPO_ROOT>',worktree_count=rawgit['worktrees'].count('worktree '),ignored=[dict(status=line[:2],path_alias='LOCAL_IGNORED_PATH:SHA256:'+hashlib.sha256(line[3:].encode()).hexdigest()) for line in rawgit['ignored'].splitlines() if line.startswith('!!')])
    write('final-git-state.json',dict(branch=c.git('branch','--show-current'),head=c.git('rev-parse','HEAD'),main=c.git('rev-parse','main'),origin_main=c.git('rev-parse','origin/main'),parent=c.git('rev-parse','HEAD^'),tree=c.git('rev-parse','HEAD^{tree}'),main_is_ancestor=True,public_state=public_git,raw_receipt_sha256=c.sha(RUN/'final-git-raw.json'),diff_check_exit=0,index_empty=True,staged=False,committed=False,merged=False,pushed=False,fetch=False,scope='Final status includes every current untracked candidate. Exact ignored/worktree state is private under the reported receipt hash; public ignored paths use content-derived aliases to avoid local workstation/account filenames. Public manifest is resealed after dossier/evidence-only closeout; terminal publication guard verifies the resulting complete set.'))
    worked_examples()

def worked_examples():
    d=json.loads((OUT/'spc-multivariate-postfit-same-core-comparisons-v1.json').read_bytes());payloads={}
    for row in d['payload_files']:
        assert c.sha(ROOT/row['path'])==row['sha256'];payloads.update(json.loads((ROOT/row['path']).read_bytes())['payloads'])
    examples=[]
    def number(bits):return None if bits=='BLANK' else struct.unpack('>d',bytes.fromhex(bits))[0]
    for index in range(7):
        row=next(r for r in d['records'] if r['configuration']=='Debug' and r['tfm']=='net10.0' and payloads[r['profile_id']]['id']=='published-'+str(index))
        p=payloads[row['profile_id']];v=p['presentation'];panel=next(iter(v['panel_payloads'].values()));channels={ch['kind']:ch['values'] for ch in panel['channels']}
        examples.append(dict(id=p['id'],chart=p['chart'],profile_id=row['profile_id'],dataset_typed_sha256=p['source']['dataset_sha256'],options=p['requested_options'],panel_type=panel['type'],axis_point_count=len(v['projections'][0]['axis']['data']),first_axis_rows=v['projections'][0]['axis']['data'][:2],axis_columns=v['projections'][0]['axis']['columns']+['source_rows'],stage_boundaries=v['projections'][0]['boundaries'],signal_marker_count=sum(b!='BLANK' for b in channels['Signal']),excluded_marker_count=sum(b!='BLANK' for b in channels['Excluded']),first_statistic=number(channels['Statistic'][0]),first_cl=number(channels['CenterLine'][0]),first_lcl=number(channels['LowerControlLimit'][0]),first_ucl=number(channels['UpperControlLimit'][0]),last_statistic=number(channels['Statistic'][-1]),rank=v['rank'],retained_components=v['retained'],pseudo=v['pseudo'],warnings=v['warnings'],section_shapes=[dict(name=s['name'],tables=[dict(rows=t['rows'],columns=t['columns'],sha256=t['sha256']) for t in s['tables']]) for s in v['sections']]))
    negatives=[]
    for family in sorted({p['chart'] for p in payloads.values() if p['outcome']=='PRESERVED_REJECTION'}):
        row=next(r for r in d['records'] if r['configuration']=='Debug' and r['tfm']=='net10.0' and payloads[r['profile_id']]['chart']==family and payloads[r['profile_id']]['outcome']=='PRESERVED_REJECTION')
        p=payloads[row['profile_id']];negatives.append(dict(id=p['id'],chart=family,profile_id=row['profile_id'],exception=p['exception'],reference=p['rejection_reference']))
    write('worked-compatibility-examples-v1.json',dict(status='NOT_GOLDEN / SAME_CORE_COMPATIBILITY_ONLY',positive=examples,negative=negatives,numbers='Decoded directly from captured Core-result Double bits for readable examples, no statistical calculation. Complete exact typed values/row hashes/channels in the content-addressed corpus. No native rendering or independent scientific accuracy claim.'))

def finish():
    """Resume evidence-only closeout without repeating qualified tests/corpus."""
    path=OUT/'spc-multivariate-postfit-same-core-comparisons-v1.json';d=json.loads(path.read_bytes());assert d['phase']=='FINAL'
    hashes={}
    for row in d['records']:
        p=trx('final-'+row['configuration'],'Common' if row['tfm']=='net10.0' else 'Legacy')
        key=str(p)
        if key not in hashes:hashes[key]=c.sha(p)
        assert hashes[key]==row['raw_trx_sha256']
        row['raw_trx_alias']='LOCAL_TRX:'+p.parents[1].name
    path.write_bytes(json.dumps(d,ensure_ascii=False,separators=(',',':'),allow_nan=False).encode()+b'\n')
    for name in ['multivariate-chart-family-projection-v1.json','multivariate-model-covariance-report-parity-v1.json','multivariate-signal-warning-chartdata-parity-v1.json']:
        v=json.loads((OUT/name).read_bytes());v['sha256']=c.sha(path);write(name,v)
    write('accounting-transients.json',dict(scope='Evidence-generation/tooling only; no statistical/production/compiled-test source changes or test-result replacement.',attempts=[dict(command='<PYTHON_EXE> -B scripts/summarize-04a24.py final',exit=1,cause='Compared order-dependent display representatives of existing null/empty cohorts; canonical source-backed cohort keys/weights match. Fixed accounting signature, old assertions/results retained.'),dict(command='<PYTHON_EXE> -B scripts/summarize-04a24.py final',status='NOT_COMPLETED / AGENT_OWNED_ACCOUNTING_PROCESS_STOPPED',exit=1,cause='Repeated full TRX hashing per inventory row. Stopped only the identified accounting process after verifying its exact PID/start-time/executable; no test worker remained. Hash each immutable receipt once, resume finish from completed measured/corpus artifacts.'),dict(command='<PYTHON_EXE> -B scripts/summarize-04a24.py finish',purpose='Resume only inventory/preservation/receipts/dossier support; no rerun or promotion of statistical captures.')],raw_scope='Full test/build/focused stdout/stderr/exit receipts are preserved under the isolated run. Direct accounting diagnostics and the optional denied WMI probe are in the Codex execution transcript; no fabricated raw file/hash/timing is asserted. Root OS process metrics remain the only resource measurements.'))
    inventory();source();retain();receipts();closeout()

if __name__=='__main__':
    p=argparse.ArgumentParser();p.add_argument('action',choices=['prepare','focused','final','source','receipts','retain','finish','closeout']);p.add_argument('--label',default='focused-05');a=p.parse_args()
    if a.action=='focused':corpus({'Debug':a.label},'FOCUSED')
    else:globals()[a.action]()
