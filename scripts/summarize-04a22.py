"""04A.22 actual receipt accounting. Historical records are read-only."""
import argparse
import base64
from collections import Counter
import hashlib
import importlib.util
import json
import platform
from pathlib import Path
import re
import struct
import subprocess
import xml.etree.ElementTree as ET

ROOT=Path(__file__).resolve().parents[1]
def module(name,path):
    spec=importlib.util.spec_from_file_location(name,ROOT/path)
    result=importlib.util.module_from_spec(spec);spec.loader.exec_module(result);return result
c=module('capture22','scripts/capture-04a22-contract.py')
prior=module('summary21','scripts/summarize-04a21.py')
old=prior.old
RUN,OUT=c.RUN,c.OUT
old.RUN=RUN
NS=old.NS

def clean(value):
    value=prior.sanitize(value)
    if isinstance(value,dict):return {k:clean(v) for k,v in value.items()}
    if isinstance(value,(list,tuple)):return [clean(v) for v in value]
    if isinstance(value,str):
        value=value.replace('<REPO>','<REPO_ROOT>')
        # Historical parameterized tests include synthetic credential URLs and
        # drive paths in their display identities. Preserve exact local TRX and
        # publish stable content-derived aliases instead of those literal values.
        for pattern,label in [(r'https?://[^\s/"<>:@]+:[^\s/"<>@]+@[^\s"<>]+','TEST_URL'),
                              (r'\b[A-Za-z0-9._%+-]+@[A-Za-z0-9.-]+\.[A-Za-z]{2,}\b','TEST_EMAIL'),
                              (r'\b[A-Za-z]:[/\\]+[A-Za-z0-9_][^"\s,<>)]*','LOCAL_TOOL_OR_TEST_PATH')]:
            value=re.sub(pattern,lambda m:'<'+label+':SHA256:'+hashlib.sha256(m.group().encode()).hexdigest()+'>',value)
        return value
    return value

def write(name,value,compact=False):
    (OUT/name).write_text(json.dumps(clean(value),ensure_ascii=False,allow_nan=False,indent=None if compact else 2,separators=(',',':') if compact else None)+'\n',encoding='utf-8',newline='\n')

def receipts(prefix,config):
    paths=[RUN/prefix/config/'results'/p for p in ['Core.trx','OfficeAddin.trx','OfficeExecution.trx']]
    paths += [RUN/'executions'/(prefix+'-'+p)/'results'/(p+'.trx') for p in ['Common','Legacy']]
    paths += [RUN/'projection-executions'/prefix/'results/ControlMitigation.trx']
    if prefix=='entry-Debug':
        paths[3:5]=[RUN/'executions'/('entry-qualified-Debug-'+p)/'results'/(p+'.trx') for p in ['Common','Legacy']]
    return [old.old.trx(p) for p in paths]

def measure(prefix,config,python_prefix=None):
    suites=receipts(prefix,config)
    groups=old.previous.typed_groups(suites)
    weight=old.prior.weight
    logical=dict(total=sum(weight(k) for k in groups),passed=sum(weight(k) for k,rows in groups.items() if all(r['outcome']=='Passed' for r in rows)))
    logical['nonpassed']=logical['total']-logical['passed']
    new={k:rows for k,rows in groups.items() if any('Spc22' in r['class_name'] for r in rows)}
    log=(RUN/(prefix+'-full.log')).read_text(encoding='utf-8',errors='replace')
    pylogs=[RUN/(prefix+'-full.log'),RUN/(prefix+'-common-python.log')]
    if python_prefix:
        pylogs=[RUN/(python_prefix+'-'+suffix+'.log') for suffix in ['tools','help','scripts','common-python']]
    py=old.previous.python(pylogs)
    inventory=[]
    for key,rows in groups.items():
        inventory.append(dict(identity=repr(key),weight=weight(key),class_name=rows[0]['class_name'],method=rows[0]['method'],test_name=rows[0]['test_name'],outcomes=[r['outcome'] for r in rows],source_hashes=rows[0].get('source_hashes',[]),batch22=any('Spc22' in r['class_name'] for r in rows)))
    write(('entry' if prefix.startswith('entry') else 'final')+'-'+config+'-logical-test-identities.json',dict(cases=inventory,accounting='Accepted source-hash/class/method/data-row identity deduplication; established typed null/empty cohort weights preserved. Raw executions separately reported.'))
    raw_inventory=[]
    for suite in suites:
        raw_inventory.append(dict(suite=suite['path'],sha256=suite['sha256'],records=suite['records']))
        tree=ET.parse(ROOT/suite['path'])
        messages={r.attrib['testName']:r.findtext('.//'+NS+'Message',default='') for r in tree.findall('.//'+NS+'UnitTestResult') if r.attrib['outcome']!='Passed'}
        for row in suite['nonpasses']:row['message']=messages.get(row['test_name'],'')
        suite['batch22_raw']=sum('Spc22' in r['class_name'] for r in suite['records'])
        suite['batch22_raw_passed']=sum('Spc22' in r['class_name'] and r['outcome']=='Passed' for r in suite['records'])
        suite.pop('records');suite.pop('batch11',None)
    write(('entry' if prefix.startswith('entry') else 'final')+'-'+config+'-raw-test-identities.json',dict(suites=raw_inventory,scope='All raw framework executions from the six qualifying suites; no repeated focused/semantic executions added to totals.'))
    return dict(managed_suites=suites,logical_cases=logical,runtime_executions=dict(total=sum(int(s['counters']['total']) for s in suites),passed=sum(int(s['counters']['passed']) for s in suites),failed=sum(int(s['counters']['failed']) for s in suites)),new_logical_total=sum(weight(k) for k in new),new_logical_passed=sum(weight(k) for k,rows in new.items() if all(r['outcome']=='Passed' for r in rows)),node_total=sum(map(int,re.findall(r'\btests (\d+)\s*$',log,re.M))),node_passed=sum(map(int,re.findall(r'\bpass (\d+)\s*$',log,re.M))),python=py,python_receipts=[p.relative_to(RUN).as_posix() for p in pylogs],full_command_exit=json.loads((RUN/(prefix+'-full-command.json')).read_bytes())['exit_code'])

def entry():
    configs={v:measure('entry-Debug' if v=='Debug' else 'entry-full-Release',v) for v in ['Debug','Release']}
    write('entry-validation.json',dict(base=c.BASE,configurations=configs,qualification='Debug Common/Legacy freshly rebuilt and rerun under entry-qualified-Debug following initial helper alias setup error; all initial receipts retained. Release complete entry-full-Release plus earlier entry-Release retained. No production edits before entry qualification.'))
    for config,data in configs.items():print('entry',config,data['logical_cases'],data['runtime_executions'],'Node',data['node_passed'],data['node_total'],'Python',data['python']['passed'],data['python']['total'])

def validation():
    initial=json.loads((OUT/'entry-validation.json').read_bytes())['configurations']
    configs={}
    for config in ['Debug','Release']:
        qualifier='final-python-'+config if (RUN/('final-python-'+config+'-common-python.log')).exists() else None
        d=measure('final-'+config,config,qualifier)
        identity=lambda r:(r['class_name'].split('.')[-1],r['method'],r['test_name'].replace('"',''))
        before={identity(r) for s in initial[config]['managed_suites'] for r in s['nonpasses']}
        after={identity(r) for s in d['managed_suites'] for r in s['nonpasses']}
        d['new_managed_nonpasses']=sorted(after-before)
        d['removed_managed_nonpasses']=sorted(before-after)
        d['python_new_since_entry']={p:v for p,v in d['python']['nonpasses'].items() if p not in initial[config]['python']['nonpasses']}
        d['python_entry_now_passing']=sorted(set(initial[config]['python']['nonpasses'])-set(d['python']['nonpasses']))
        configs[config]=d
        print('final',config,d['logical_cases'],d['runtime_executions'],'new',d['new_logical_passed'],d['new_logical_total'],'Node',d['node_passed'],d['node_total'],'Python',d['python']['passed'],d['python']['total'])
    write('validation.json',dict(configurations=configs,status='NONZERO_FULL_SUITES_RETAINED',native='NOT_RUN / UNAUTHORIZED',R='NOT_RUN / UNAUTHORIZED',scientific_promotion=False))
    write('nonpasses.json',dict(entry={v:dict(managed=[dict(r,suite=s['path']) for s in d['managed_suites'] for r in s['nonpasses']],python=d['python']['nonpasses']) for v,d in initial.items()},final={v:dict(managed=[dict(r,suite=s['path']) for s in d['managed_suites'] for r in s['nonpasses']],python=d['python']['nonpasses'],new_managed=d['new_managed_nonpasses'],new_python=d['python_new_since_entry']) for v,d in configs.items()},interpretation='Old assertions are not rewritten. New frozen source-pin failures caused by authorized renderer/output extraction are retained and have new functional successor contracts. Native/scientific gates remain unmet.'))
    traces={}
    for config,d in configs.items():
        traces[config]={}
        for phase,receipt_paths in [('entry',initial[config]['python_receipts']),('final',d['python_receipts'])]:
            blocks=[]
            for receipt in receipt_paths:
                text=(RUN/receipt).read_text(encoding='utf-8',errors='replace')
                for part in re.split(r'^={60,}\s*$',text,flags=re.M):
                    match=re.match(r'\s*(FAIL|ERROR): ([^\n]+)',part)
                    if not match:continue
                    end=re.search(r'^Ran \d+ tests? in ',part,re.M)
                    blocks.append(dict(receipt=receipt,identity=match[2],outcome=match[1],trace_lines=(part[:end.start()] if end else part).splitlines()))
            traces[config][phase]=blocks
    write('python-nonpass-traces.json',dict(configurations=traces,scope='Sanitized actual assertion/exception traces, including each original failure identity. No inherited expectations changed.'))

def corpus(focused=False):
    records=[];payloads={};charts={}
    for config in ['Debug','Release']:
        for tfm,suite in [('net10.0','Common'),('net48','Legacy')]:
            label=('focused-09' if config=='Debug' else 'focused-10') if focused else 'final-'+config
            path=RUN/'executions'/(label+'-'+suite)/'results'/(suite+'.trx')
            assert path.exists(),path
            digest=c.sha(path)
            for result in ET.parse(path).getroot().findall('.//'+NS+'UnitTestResult'):
                for stdout in result.findall('.//'+NS+'StdOut'):
                    for line in (stdout.text or '').splitlines():
                        if not line.startswith('04A22_XML|'):continue
                        xml=base64.b64decode(line.split('|',1)[1]).decode('utf-8')
                        root=ET.fromstring(xml)
                        classification=root.attrib.get('classification','PRESENTATION_PARITY_SYNTHETIC')
                        payload=clean(dict(xml=xml,classification=classification,id=root.attrib.get('id','synthetic'),culture=root.attrib.get('culture',''),status='NOT_GOLDEN',validation='SAME_CORE_PRESENTATION_COMPATIBILITY_ONLY'))
                        key=hashlib.sha256(json.dumps(payload,ensure_ascii=False,sort_keys=True,separators=(',',':')).encode()).hexdigest()
                        payloads.setdefault(key,payload)
                        records.append(dict(profile_id=key,configuration=config,tfm=tfm,outcome=result.attrib['outcome'],test_identity=result.attrib['testName'],raw_trx_alias='LOCAL_TRX:'+label+'-'+suite,raw_trx_sha256=digest))
                        # Synthetic model shapes characterize alignment API edges;
                        # only real Fit cases establish chart-family panel shapes.
                        for projection in root.findall('projection'):
                            chart=projection.attrib['chart']
                            order=tuple(p.attrib['type'] for p in projection.findall('panel'))
                            charts.setdefault(chart,set()).add(order)
    assert records
    classifications=dict(Counter(payloads[r['profile_id']]['classification'] for r in records))
    write('spc-postfit-same-core-comparisons-v1.json',dict(version=1,status='NOT_GOLDEN',validation='SAME_CORE_PRESENTATION_COMPATIBILITY_ONLY',promoted=False,records=records,payloads=payloads,record_count=len(records),payload_count=len(payloads),classifications=classifications,tolerance='Exact Double bit identity (including signed zero), category runtime type and value, blank masks, Core object/source identity, original label content/order, stage predicate and Double i/n. Both paths receive the same Core result; no second Fit or independent oracle. Core/request rejections are explicitly not parity successes.',fixture=dict(path='BESHStatNG_Help_MkDocs/docs/assets/data/115controlcharts/115controlcharts.csv',sha256=c.sha(ROOT/'BESHStatNG_Help_MkDocs/docs/assets/data/115controlcharts/115controlcharts.csv'),mapping='Accepted Spc21Fixture embeds exact public decimal columns: 5-wide measurements, Individual, counts, sample sizes and exposures. np uses constant 100; variable np is a negative profile. Profiles 0..569 retain original request/missing/stage/exclusion/rule settings; XML request/settings hashes and original point/occurrence arrays are recorded. Synthetic sparse/identity fixtures separately identified.')),compact=True)
    write('spc-chart-family-panel-matrix-v1.json',dict(version=1,status='NOT_GOLDEN',availability='Windows only',charts={k:sorted(v) for k,v in sorted(charts.items())},synthetic_model_shapes_excluded=True,production='Only actual Fit cases establish this matrix. Core ChartFamily and returned panel order are consumed by Ui21 post-fit projection and real renderer; no independent family registry or UI activation.'))
    for name,scope in [('spc-univariate-axis-series-parity-v1.json','Typed representative categories; sparse shared union, all ten bitwise channels and blank masks; per-panel absent versus NaN identity in XML.'),('spc-signal-exclusion-marker-parity-v1.json','Core point flags, original rule enumeration, zero-based labels and separate occurrence windows/contributors/marked identities.'),('spc-stage-boundary-geometry-v1.json','OrdinalIgnoreCase adjacent representative stage changes, i/n Double fractions and exact alternative text. Phase-only changes do not add a boundary.'),('spc-zone-reference-geometry-v1.json','Eight statistic/reference channels transferred. Native HasValidZones, AddZoneBands, ComputeYAxisBounds, last finite reference labels, spec/target eligibility and natural/proportion clipping remain entry method bytes; source tests and new bridge tests cover unchanged inputs. No live Excel proof.')]:
        write(name,dict(version=1,status='NOT_GOLDEN / SAME_CORE_PRESENTATION_COMPATIBILITY_ONLY',scope=scope,corpus='spc-postfit-same-core-comparisons-v1.json',corpus_sha256=c.sha(OUT/'spc-postfit-same-core-comparisons-v1.json'),native='NOT_RUN / UNAUTHORIZED'))
    print('Corpus',len(records),'records',len(payloads),'payloads',classifications,'charts',len(charts))

def authority():
    prior.c=c;prior.RUN=RUN;prior.OUT=OUT;prior.write=write
    prior.authority()
    write('candidate-issues.json',dict(activated_behavior_fixes=0,**{'C21-STACKED-ROWCOUNT-VALIDATION-01':'OPEN / CANDIDATE_ONLY / UNAPPLIED'},C22=[],predecessors='All accepted C09-C20 candidate and native-control restrictions retained byte-for-byte in predecessor dossiers/evidence. No correction activated.',native='NOT_RUN / UNAUTHORIZED',R='NOT_RUN / UNAUTHORIZED',P='NO',N1='NO GO',N2='NO',C2='UNMET',releases=0,discharges=0))

def source():
    pins=json.loads((RUN/'baseline.json').read_bytes())['source_pins']
    changed=set(c.git('diff','--name-only').splitlines())
    allowed={c.FORM,c.RENDERER,'BESHStatNG.Test/BESHStatNG.Tests.vbproj','BESHStatNG_Help_MkDocs/docs/methods/control-charts.md'}
    assert changed<=allowed,changed-allowed
    for p,h in pins.items():
        if p not in changed:assert c.sha(ROOT/p)==h,p
    for p,r in json.loads((OUT/'frozen-capture-hashes.json').read_bytes())['files'].items():assert c.sha(ROOT/p)==r['sha256'],p
    registry=json.loads((ROOT/'evidence/04A14/baseline-registry.json').read_bytes())
    assert len(registry['files'])==100
    for p,r in registry['files'].items():assert c.sha(ROOT/p)==r['sha256'],p
    before=json.loads((OUT/'prechange-method-inventory.json').read_bytes())['renderer_methods']
    after=c.members((ROOT/c.RENDERER).read_text(encoding='utf-8-sig'))
    method_changes={n:dict(before=r['sha256'],after=hashlib.sha256(after[n].encode()).hexdigest() if n in after else None) for n,r in before.items() if n not in after or r['sha256']!=hashlib.sha256(after[n].encode()).hexdigest()}
    write('source-preservation.json',dict(base=c.BASE,changed={p:dict(before=pins[p],after=c.sha(ROOT/p)) for p in changed},renderer_method_changes=method_changes,renderer_added_methods=sorted(set(after)-set(before)),unchanged_entry_files=len(pins)-len(changed),frozen_exact_hashes_pass=True,original_mixed_captures=100,Core_numerics_UDF_Designer_Ui22_targets_packages_unchanged=True,index_empty=not bool(c.git('diff','--cached','--name-only'))))
    page='BESHStatNG_Help_MkDocs/docs/methods/control-charts.md'
    write('documentation-audit.json',dict(page=dict(path=page,entry_sha256=pins[page],final_sha256=c.sha(ROOT/page)),metadata_sha256=c.sha(ROOT/'BESHStatNG_Help_MkDocs/docs/_meta/methods.yml'),dataset_sha256=c.sha(ROOT/'BESHStatNG_Help_MkDocs/docs/assets/data/115controlcharts/115controlcharts.csv'),metadata_changed=False,published_captures_changed=False,availability='Windows only; Office/Mac SPC GUI remains unavailable.',strict_mkdocs={n:json.loads((RUN/(n+'-command.json')).read_bytes())['exit_code'] for n in ['final-mkdocs'] if (RUN/(n+'-command.json')).exists()}))
    print('Preservation PASS;',len(changed),'tracked paths;',len(method_changes),'renderer methods')

def commands():
    records=[]
    for path in sorted(RUN.glob('*-command.json')):
        row=json.loads(path.read_bytes());row['receipt']=path.relative_to(RUN).as_posix();row['receipt_sha256']=c.sha(path);records.append(row)
    for path in sorted((RUN/'executions').glob('*/execution.json')):
        row=json.loads(path.read_bytes());records.append(dict(command=row['command'],exit_code=row['exit_code'],seconds=row['seconds'],receipt=path.relative_to(RUN).as_posix(),receipt_sha256=c.sha(path)))
    write('command-outcomes.json',dict(records=records,all_nonzero_retained=True,public_paths='Commands sanitized to <PYTHON_EXE>, <REPO_ROOT> and tool aliases; raw logs/TRX remain local.'))

def inventory():
    data=json.loads((OUT/'final-Debug-logical-test-identities.json').read_bytes())['cases']
    cases=[r for r in data if r['batch22']]
    executions=[]
    for config in ['Debug','Release']:
        raw=json.loads((OUT/('final-'+config+'-raw-test-identities.json')).read_bytes())
        for suite in raw['suites']:
            executions.extend(dict(configuration=config,suite=suite['suite'],**r) for r in suite['records'] if 'Spc22' in r['class_name'])
    for r in cases:
        source='BESHStatNG.Test/src/Spc22AdapterTests.vb' if r['class_name'].split('.')[-1]=='Spc22AdapterTests' else 'BESHStat.Excel.Common.Tests/src/Spc22Tests.vb'
        r['source']=source;r['source_sha256']=c.sha(ROOT/source)
        r['fixture']='Accepted Spc21Profile/Spc21Fixture request plus invariant/sk-SK projections' if r['method']=='SameCoreEntryRendererAndShippingProjection' else 'New Spc22 immutable Core model/request fixture; see named test method and source'
        r['expectation']='Exact entry-renderer presentation/bridge compatibility or explicitly retained Core construction/rejection; no independent numerical oracle or native execution'
        r['executions']=[e for e in executions if e['class_name'].split('.')[-1]==r['class_name'].split('.')[-1] and e['method']==r['method'] and e['test_name'].replace('"','')==r['test_name'].replace('"','')]
        assert r['executions'] and all(e['outcome']=='Passed' for e in r['executions'])
        r['requirements']='15 families / same-result legacy parity / axis-channel-label-stage identity' if r['method']=='SameCoreEntryRendererAndShippingProjection' else r['method']
        r['production_symbols']=['SpcUnivariateChartProjectionPolicy.Project','Ui21ControlCharts.WriteResultsToNewWorkbook','SpcControlChartExcel.AddCharts/MarshalChartValues/MarshalChartCategories']
    write('test-inventory.json',dict(logical_count=sum(r['weight'] for r in cases),cases=cases,python_source='scripts/tests/test_04a22_spc.py',python_contract_count=14,scope='Each full logical test identity maps to source hash, method/fixture and production symbols. Raw repeated framework executions are not new logical cases. Core/request rejections in matrix are negative observations, not parity successes.'))

def supporting():
    raw=(RUN/'preimages/Ui21ControlCharts.vb').read_text(encoding='utf-8-sig')
    methods={m.group(2):dict(first_line=raw[:m.start()].count('\n')+1,sha256=hashlib.sha256(m.group().encode()).hexdigest(),body=m.group()) for m in re.finditer(r'^    (?:Private|Public|Friend) (?:Shared )?(Sub|Function) (\w+)\b.*?^    End \1',raw,re.M|re.S)}
    write('prechange-ui-method-inventory.json',dict(source=c.FORM,source_sha256=json.loads((OUT/'prechange-method-inventory.json').read_bytes())['files'][c.FORM]['sha256'],methods=methods,hashing='Whole-file hash pins exact entry bytes; method body/hash uses decoded UTF-8 and normalized LF, retaining exact statements and boundaries.'))
    tracked=c.git('ls-files').splitlines()
    projects={}
    for project in ['BESHStat.Core/BESHStat.Core.vbproj','BESHStat.Excel.Common/BESHStat.Excel.Common.vbproj','BESHStat.Excel.Common.Tests/BESHStat.Excel.Common.Tests.vbproj','BESHstatNG/BESHStatNG.vbproj','BESHStatNG.Test/BESHStatNG.Tests.vbproj']:
        entry_bytes=subprocess.check_output(['git','-c','safe.directory='+ROOT.as_posix(),'show',c.BASE+':'+project],cwd=ROOT)
        parsed=ET.fromstring(entry_bytes);base=Path(project).parent
        includes=[]
        for node in parsed.iter():
            if node.tag.split('}')[-1]=='Compile' and node.get('Include'):
                includes.append(node.get('Include'))
        implicit_new=['src/Analysis/SpcUnivariateChartProjectionPolicy.vb'] if project.startswith('BESHStat.Excel.Common/') else ['src/Spc22Tests.vb','src/Spc22LegacyRenderer.vb'] if project.startswith('BESHStat.Excel.Common.Tests/') else []
        projects[project]=dict(entry_git_blob_sha256=hashlib.sha256(entry_bytes).hexdigest(),entry_working_sha256=json.loads((RUN/'baseline.json').read_bytes())['source_pins'][project],final_sha256=c.sha(ROOT/project),entry_explicit_compile_includes=includes,entry_implicit_compile_sources=[p for p in tracked if p.startswith(base.as_posix()+'/') and p.endswith('.vb')] if parsed.attrib.get('Sdk') else [],final_added_explicit_compile_includes=[n.get('Include') for n in ET.parse(ROOT/project).getroot().iter() if n.tag.split('}')[-1]=='Compile' and n.get('Include') and n.get('Include') not in includes],final_implicit_added_sources=implicit_new)
    write('project-compilation-inventory.json',dict(projects=projects,targets_unchanged=True,packages_unchanged=True,Windows_build='Compile/CopyFilesToOutputDirectory only; generators, packaging and signing targets not run. Core/Common built separately without restore.'))
    observations={}
    entry_pins=json.loads((RUN/'baseline.json').read_bytes())['source_pins']
    for batch in ['17','18','19','20','21']:
        manifest=ROOT/('evidence/04A'+batch+'/publication-manifest.json')
        if not manifest.exists():continue
        rows=json.loads(manifest.read_bytes()).get('files',{})
        diffs=[]
        for p,r in rows.items():
            if not (ROOT/p).is_file() or not isinstance(r,dict) or 'sha256' not in r:continue
            actual=c.sha(ROOT/p)
            entry_hash=entry_pins.get(p,actual)
            if entry_hash==r['sha256']:continue
            raw_bytes=(ROOT/p).read_bytes() if actual==entry_hash else subprocess.check_output(['git','-c','safe.directory='+ROOT.as_posix(),'show',c.BASE+':'+p],cwd=ROOT)
            lf=hashlib.sha256(raw_bytes.replace(b'\r\n',b'\n')).hexdigest()
            crlf=hashlib.sha256(raw_bytes.replace(b'\r\n',b'\n').replace(b'\n',b'\r\n')).hexdigest()
            diffs.append(dict(path=p,historical_sha256=r['sha256'],live_entry_sha256=entry_hash,classification='LINE_ENDINGS_ONLY' if r['sha256'] in [lf,crlf] else 'ACCEPTED_SUCCESSOR_OR_HISTORICAL_MANIFEST_DISCREPANCY_NOT_04A22_MUTATION'))
        observations[batch]=dict(manifest_sha256=c.sha(manifest),differences=diffs)
    write('historical-manifest-observations.json',dict(observations=observations,interpretation='Historical manifests are frozen claims from their own reviewed candidates, not current-base measurements. Live entry hashes are pinned independently. Later accepted successor source is not rewritten to satisfy old source hashes; CRLF/LF-only differences recorded explicitly.'))
    write('toolchain.json',dict(dotnet_sdk='10.0.403',VS2026_MSBuild='18.11.1-1.26513.3+940033be3',VSTest_modern='18.7.0',VSTest_framework='18.11.0',node='24.20.0',python=platform.python_version()+'; portable <PYTHON_EXE> -B',frameworks=['netstandard2.0','net48','net10.0'],restore='NOT_RUN; --no-restore and existing local packages',R='NOT_RUN / UNAUTHORIZED',native='NOT_RUN / UNAUTHORIZED',commands='command-outcomes.json; raw toolchain-dotnet receipt retained locally.'))

def qualify(label):
    names=set(c.git('diff','--name-only').splitlines())|set(c.git('ls-files','--others','--exclude-standard').splitlines())
    pins={p:c.sha(ROOT/p) for p in sorted(names) if p!='docs/architecture/batch-04A-22-owner-review.md' and (p.endswith(('.vb','.vbproj','.cs','.csproj','.md')) or p.endswith('.py') and '/tests/' in p)}
    write(label+'.json',dict(pins=pins,scope='Exact candidate compiled/test/Help inputs before full qualification; unchanged historical compiled files checked separately. Dossier/evidence/accounting tooling are qualified by final publication checks.'))

def final_qualification():
    a=json.loads((OUT/'qualification-before-final.json').read_bytes())['pins']
    for p,h in a.items():assert c.sha(ROOT/p)==h,p
    write('final-source-qualification.json',dict(compiled_sources_unchanged_after_final_suites=True,final_input_pins=a,configurations=['Debug','Release'],source_receipts='Final full suites and isolated assembly-copy manifests in command-outcomes.json. Later edits limited to evidence/dossier/accounting tooling; production/VB tests remain exact.',frameworks=['netstandard2.0','net48','net10.0'],native=False,R=False))

def retention():
    proof=RUN/'preservation-final-post.json'
    record=json.loads(proof.read_bytes())
    assert record['status']=='PASS' and not record['errors']
    nested=json.loads((RUN/'nested-initial-metadata.json').read_bytes())
    errors=[]
    for row in nested:
        p=ROOT/row['path']
        if not p.exists():errors.append(row['path']);continue
        stat=p.stat()
        if tuple(getattr(stat,'st_'+key) for key in ['size','mtime_ns','ctime_ns','ino','dev'])!=tuple(row[k] for k in ['bytes','mtime_ns','ctime_ns','ino','dev']):errors.append(row['path'])
    assert not errors,errors
    write('retention.json',dict(historical_files=record['historical_files'],nested_historical_files=len(nested),membership_metadata_errors=0,selected_frozen_hashes='frozen-capture-hashes.json; source-preservation.json; original 100 captures rehashed',raw_receipt_alias='LOCAL_RECEIPT:preservation-final-post.json',raw_receipt_sha256=c.sha(proof),method=record['method'],complete_historical_byte_audit=False))

def worked():
    path=OUT/'spc-postfit-same-core-comparisons-v1.json'
    data=json.loads(path.read_bytes());examples=[]
    for chart,index in [('XBarR','6'),('Ewma','13'),('Cusum','12'),('PChart','8'),('NpChart','9')]:
        row=next(r for r in data['records'] if r['configuration']=='Debug' and r['tfm']=='net10.0' and data['payloads'][r['profile_id']]['id']==index and data['payloads'][r['profile_id']]['culture']=='')
        root=ET.fromstring(data['payloads'][row['profile_id']]['xml']);projection=root.find('projection');panels=[]
        for panel in projection.findall('panel'):
            channels={ch.attrib['kind']:[v.text for v in ch.findall('v')] for ch in panel.findall('channel')}
            decode=lambda v:None if v=='BLANK' else struct.unpack('>d',bytes.fromhex(v))[0]
            panels.append(dict(panel=panel.attrib,first_four_values={k:[decode(v) for v in values[:4]] for k,values in channels.items()},signal_series_slots_zero_based=[i for i,v in enumerate(channels['Signal']) if v!='BLANK'],excluded_series_slots_zero_based=[i for i,v in enumerate(channels['Excluded']) if v!='BLANK'],full_original_arrays_and_masks='Referenced corpus payload XML, all IEEE-754 bits/BLANK entries retained.'))
        examples.append(dict(chart=chart,fixture_id=index,profile_id=row['profile_id'],options_xml=ET.tostring(root.find('options'),encoding='unicode'),panels=panels,boundaries=[dict(b.attrib,text=b.text) for b in projection.findall('boundary')]))
    negative=next(r for r in data['records'] if r['configuration']=='Debug' and r['tfm']=='net10.0' and data['payloads'][r['profile_id']]['id']=='supplemental-11')
    write('spc-worked-compatibility-examples-v1.json',dict(version=1,status='NOT_GOLDEN / SAME_CORE_PRESENTATION_COMPATIBILITY_ONLY',corpus_sha256=c.sha(path),examples=examples,varying_np_rejection=dict(profile_id=negative['profile_id'],payload=data['payloads'][negative['profile_id']]),column_mapping=dict(XBarR=['Measure_1','Measure_2','Measure_3','Measure_4','Measure_5'],Ewma=['Individual_Value'],Cusum=['Individual_Value'],PChart=['Nonconforming_Count','Sample_Size'],NpChart=['Nonconforming_Count','Sample_Size_Constant']),fixture_sha256=c.sha(ROOT/'BESHStatNG_Help_MkDocs/docs/assets/data/115controlcharts/115controlcharts.csv'),interpretation='Values are actual unchanged Core returns, not an independent statistical oracle. Display samples retain round-trip Double values; full bit vectors remain in linked payloads. No Excel chart was executed.'))

def transients():
    write('transient-validation.json',dict(receipts='All unsuccessful/superseded command, build, test and filter receipts retained under the fresh run. command-outcomes.json includes every durable suite receipt, including nonzero exits.',observations=[
        dict(attempt='entry-Debug initial Common build',issue='No-TestClass legacy helper used an unresolved label-list alias; BC30002. Helper isolated before qualifying original entry sources.',resolution='Fresh entry-qualified-Debug Common/Legacy full reruns and complete entry-full-Release; zero-test/stale receipts excluded from totals.'),
        dict(attempt='local helper generation',issue='IndentationError then NameError in one-off generation commands; recorded in assistant tool transcript.',resolution='Frozen entry method bodies generated with explicit generic label-list signatures and provenance. No production source behavior changed.'),
        dict(attempt='focused-01 / focused-02',issue='No matching Spc22 tests while helpers were being integrated; process exits 0 are NOT counted as successful test validation.',resolution='Subsequent enumerated focused/full suites qualify actual tests.'),
        dict(attempt='focused-03',issue='Four syntax diagnostics in new VB tests (single-line declaration lambdas and shadowed lambda parameter).',resolution='New test syntax corrected; protected tests untouched.'),
        dict(attempt='focused-04',issue='New test assumed input rule order 2,1; Core result already normalizes to 1,2.',resolution='Assert actual immutable Core enumeration and legacy renderer text R1,2; no production sort/correction.'),
        dict(attempt='focused-06',issue='New supplemental tests expected to reach Fit, but span 1/infinite input reject in Core constructors.',resolution='Capture exact earlier constructor type/message; retain rejection as non-parity outcome.'),
        dict(attempt='initial publication scan',issue='Synthetic credential URL/drive-path literals in historical test display names.',resolution='Public stable SHA-256 aliases; exact identities remain in local TRX. No credential literals published.'),
        dict(attempt='focused-python-01',issue='SyntaxError in a new test separator; discovery ran one failed-loader case.',resolution='New test syntax corrected; focused-python-02 and both complete final matrices enumerate all 14 contracts.')],final_behavioral_sources='qualification-before-final.json -> final-source-qualification.json; no compiled/test source edit after both full matrices began.'))

def git_state():
    p=OUT/'final-git-state.json'
    if not p.exists():write(p.name,{})
    assert not c.git('diff','--cached','--name-only')
    assert subprocess.run(['git','-c','safe.directory='+ROOT.as_posix(),'diff','--check'],cwd=ROOT).returncode==0
    assert subprocess.run(['git','-c','safe.directory='+ROOT.as_posix(),'merge-base','--is-ancestor',c.BASE,'HEAD'],cwd=ROOT).returncode==0
    write(p.name,dict(branch=c.git('branch','--show-current'),head=c.git('rev-parse','HEAD'),main=c.git('rev-parse','main'),origin_main_existing_ref=c.git('rev-parse','origin/main'),parent=c.git('rev-parse','HEAD^'),tree=c.git('rev-parse','HEAD^{tree}'),main_ancestry_verified=True,index_empty=True,diff_check_exit=0,status_short=c.git('status','--short','--untracked-files=all').splitlines(),ignored_paths='Private LOCAL_COMMAND:final-git-status-ignored; raw SHA/exit in command-outcomes.json. Historical account-bearing filenames excluded publicly.',staged=False,committed=False,pushed=False,next_batch_started=False))

def receipts():
    gates={}
    for phase in ['entry','final']:
        gates[phase]={}
        for kind in ['strict','profile']:
            name=phase+'-historical-'+kind
            command=RUN/(name+'-command.json');log=RUN/(name+'.log')
            record=json.loads(command.read_bytes())
            gates[phase][kind]=dict(command=record['command'],exit_code=record['exit_code'],raw_command_sha256=c.sha(command),raw_log_sha256=c.sha(log),diagnostic=log.read_text(encoding='utf-8',errors='replace'),status='UNMET' if kind=='strict' else 'INTEGRITY_FAILURE')
    write('gate-validation.json',dict(runs=gates,P='NO',N1='NO GO',N2='NO',C2='UNMET',native_releases=0,deferred_discharges=0,native='NOT_RUN / UNAUTHORIZED',R='NOT_RUN / UNAUTHORIZED'))
    workflows={}
    for name in ['entry-Debug','entry-full-Release','final-Debug','final-Release']:
        path=RUN/(name+'-verify-isolated.provenance.json')
        workflows[name]=dict(raw_receipt_sha256=c.sha(path),**json.loads(path.read_bytes()))
    write('verification-workflow-provenance.json',dict(original_verifier='scripts/verify-beshstatng.ps1',source_sha256=c.sha(ROOT/'scripts/verify-beshstatng.ps1'),source_unchanged=True,workflows=workflows,coverage='Original complete verifier suite enumeration preserved; fresh result paths, isolated assembly writers, --no-restore and Windows Compile-only host build replace unsafe historical writes/restores/generators/packaging/signing. Nonzero suites continue to be counted as nonpasses. Explicit WASM Node glob rerun is supplemental, not double-counted.'))

if __name__=='__main__':
    parser=argparse.ArgumentParser();parser.add_argument('action',choices=['entry','validation','corpus','corpus-focused','authority','source','commands','inventory','supporting','qualify','final-qualification','retention','worked','transients','git-state','receipts']);args=parser.parse_args()
    if args.action=='qualify':qualify('qualification-before-final')
    elif args.action=='corpus-focused':corpus(focused=True)
    else:globals()[args.action.replace('-','_')]()
