"""04A.23 sanitized receipt accounting; immutable predecessors remain read-only."""
import argparse
import base64
from collections import Counter
import hashlib
import importlib.util
import inspect
import json
from pathlib import Path
import re
import platform
import subprocess
import xml.etree.ElementTree as ET
ROOT=Path(__file__).resolve().parents[1]
def module(name,path):
    spec=importlib.util.spec_from_file_location(name,ROOT/path);m=importlib.util.module_from_spec(spec);spec.loader.exec_module(m);return m
c=module('capture23','scripts/capture-04a23-contract.py')
previous=module('summary22','scripts/summarize-04a22.py')
RUN,OUT=c.RUN,c.OUT
previous.c=c;previous.RUN=RUN;previous.OUT=OUT;previous.old.RUN=RUN
def clean(value):
    # Apply accepted scalar redaction once per leaf, rather than repeatedly
    # traversing every ancestor of large immutable point/diagnostic arrays.
    if isinstance(value,dict):return {k:clean(v) for k,v in value.items()}
    if isinstance(value,(list,tuple)):return [clean(v) for v in value]
    return previous.clean(value)
NS=previous.NS
def write(name,value,compact=False):
    (OUT/name).write_text(json.dumps(clean(value),ensure_ascii=False,allow_nan=False,indent=None if compact else 2,separators=(',',':') if compact else None)+'\n',encoding='utf-8',newline='\n')
previous.write=write
def receipts(prefix,config):
    paths=[RUN/prefix/config/'results'/p for p in ['Core.trx','OfficeAddin.trx','OfficeExecution.trx']]
    paths += [RUN/'executions'/(prefix+'-'+p)/'results'/(p+'.trx') for p in ['Common','Legacy']]
    paths += [RUN/'projection-executions'/prefix/'results/ControlMitigation.trx']
    return [previous.old.old.trx(p) for p in paths]
previous.receipts=receipts

_measure_source=inspect.getsource(previous.measure).replace('Spc22','Spc23').replace('batch22','batch23')
def measure(prefix,config,python_prefix=None):
    # Accepted deduplication and null/empty cohort weights; only new-batch label changes.
    namespace=dict(previous.__dict__);namespace['write']=write;namespace['receipts']=receipts
    exec(_measure_source,namespace)
    qualified=('final-complete-' if (RUN/('final-complete-'+config+'-full-command.json')).exists() else 'final-qualified-')+config
    if prefix=='final-'+config and (RUN/(qualified+'-full-command.json')).exists():prefix=qualified
    return namespace['measure'](prefix,config,python_prefix)
previous.measure=measure

def entry():
    configs={v:measure('entry-'+v,v) for v in ['Debug','Release']}
    write('entry-validation.json',dict(base=c.BASE,configurations=configs,qualification='Actual merged main source, original assemblies rebuilt, both complete matrices before implementation edits. Installed owner Revision 2.9 plan preserved separately. No new tests compiled at entry.'))
    for name,d in configs.items():print('entry',name,d['logical_cases'],d['runtime_executions'],'Node',d['node_passed'],d['node_total'],'Python',d['python']['passed'],d['python']['total'])

def validation():
    previous.validation()
    p=OUT/'nonpasses.json';d=json.loads(p.read_bytes())
    d['interpretation']='All distinct final nonpass identities/outcomes were already present at actual merged-main entry. The inherited 04A.22 whole-source guard additionally encounters authorized Ui22 changes before its already-failing owner-plan pin; full entry/final traces retain this assertion-site change. No renderer code changed in 04A.23, no frozen expectation was rewritten, and new functional successors qualify transferred behavior without native/scientific release.'
    write(p.name,d)

def authority():
    previous.prior.c=c;previous.prior.RUN=RUN;previous.prior.OUT=OUT;previous.prior.write=write
    previous.prior.authority()
    write('candidate-issues.json',dict(activated_behavior_fixes=0,**{'C21-STACKED-ROWCOUNT-VALIDATION-01':'OPEN / CANDIDATE_ONLY / UNAPPLIED'},C23=[],retained_prior='C09/C11/C12 native; C13 survival; C14 diagnostics; C17/C18 CA; C19 clustering; C20 FA/DA and accepted 21/22 restrictions remain unchanged under their original owners.',native='NOT_RUN / UNAUTHORIZED',R='NOT_RUN / UNAUTHORIZED',P='NO',N1='NO GO',N2='NO',C2='UNMET',releases=0,discharges=0))

def source():
    pins=json.loads((RUN/'baseline.json').read_bytes())['source_pins']
    changes=set(c.git('diff','--name-only').splitlines())
    allowed={c.FORM,'BESHStatNG.Test/BESHStatNG.Tests.vbproj','BESHStatNG_Help_MkDocs/docs/methods/multivariate-control-charts.md'}
    assert changes-{c.OWNER_PLAN}<=allowed,changes-allowed-{c.OWNER_PLAN}
    assert c.sha(ROOT/c.OWNER_PLAN)==json.loads((OUT/'owner-plan-observation.json').read_bytes())['working_sha256']
    for p,h in pins.items():
        if p not in allowed:assert c.sha(ROOT/p)==h,p
    for p,r in json.loads((OUT/'frozen-capture-hashes.json').read_bytes())['files'].items():assert c.sha(ROOT/p)==r['sha256'],p
    registry=json.loads((ROOT/'evidence/04A14/baseline-registry.json').read_bytes())
    for p,r in registry['files'].items():assert c.sha(ROOT/p)==r['sha256'],p
    before=json.loads((OUT/'prechange-method-provenance.json').read_bytes())['methods'];after=c.members((ROOT/c.FORM).read_text(encoding='utf-8-sig'))
    changed={n:dict(before=r['sha256'],after=hashlib.sha256(after[n].encode()).hexdigest() if n in after else None) for n,r in before.items() if n not in after or r['sha256']!=hashlib.sha256(after[n].encode()).hexdigest()}
    write('source-preservation.json',dict(base=c.BASE,changed={p:dict(before=pins[p],after=c.sha(ROOT/p)) for p in sorted(changes-{c.OWNER_PLAN})},form_method_changes=changed,added_methods=sorted(set(after)-set(before)),unchanged_tracked_entry_files=len(pins)-len(changes-{c.OWNER_PLAN}),owner_plan_preserved=True,Core_Ui21_Common21_22_renderer_Designer_UDF_Ribbon_Office_targets_packages_unchanged=True,original_mixed_captures=len(registry['files']),frozen_exact_hashes_pass=True,index_empty=not bool(c.git('diff','--cached','--name-only'))))
    page='BESHStatNG_Help_MkDocs/docs/methods/multivariate-control-charts.md'
    write('documentation-audit.json',dict(page=dict(path=page,entry_sha256=pins[page],final_sha256=c.sha(ROOT/page)),metadata_sha256=c.sha(ROOT/'BESHStatNG_Help_MkDocs/docs/_meta/methods.yml'),dataset_sha256=c.sha(ROOT/'BESHStatNG_Help_MkDocs/docs/assets/data/116multivariatecontrolcharts/116multivariatecontrolcharts.csv'),metadata_changed=False,published_captures_changed=False,availability='Windows only; Office/WASM/Mac multivariate SPC GUI remains unavailable.',strict_mkdocs={n:json.loads((RUN/(n+'-command.json')).read_bytes())['exit_code'] for n in ['entry-mkdocs','final-mkdocs','final-mkdocs-qualification'] if (RUN/(n+'-command.json')).exists()}))
    print('Source/frozen preservation PASS;',len(changed),'form methods; owner plan untouched')

def qualify():
    names=set(c.git('diff','--name-only').splitlines())|set(c.git('ls-files','--others','--exclude-standard').splitlines())
    pins={p:c.sha(ROOT/p) for p in sorted(names) if p not in [c.OWNER_PLAN,'docs/architecture/batch-04A-23-owner-review.md'] and (p.endswith(('.vb','.vbproj','.cs','.csproj','.md')) or p.endswith('.py') and '/tests/' in p)}
    write('qualification-before-final.json',dict(pins=pins,scope='Actual compiled/test/Help source bytes before full matrices; owner plan separately pinned. Evidence/dossier/accounting checked by terminal publication tests.'))

def corpus(focused=False):
    """Columnar, content-addressed typed payloads; raw original XML remains in local TRX."""
    payloads={};records=[];shapes={}
    for config,label in [('Debug','focused-15'),('Release','focused-16')]:
        if not focused:label=('final-complete-' if (RUN/('final-complete-'+config+'-full-command.json')).exists() else 'final-qualified-' if (RUN/('final-qualified-'+config+'-full-command.json')).exists() else 'final-')+config
        for tfm,suite in [('net10.0','Common'),('net48','Legacy')]:
            path=RUN/'executions'/(label+'-'+suite)/'results'/(suite+'.trx')
            assert path.exists(),path
            trx_sha=c.sha(path)
            for result in ET.parse(path).getroot().findall('.//'+NS+'UnitTestResult'):
                for stdout in result.findall('.//'+NS+'StdOut'):
                    for line in (stdout.text or '').splitlines():
                        if not line.startswith('04A23_XML|'):continue
                        xml=base64.b64decode(line.split('|',1)[1]);root=ET.fromstring(xml)
                        payload=dict(root.attrib);runtime=payload.pop('runtime');payload['names']=[n.text or '' for n in root.findall('names/name')]
                        identifier=payload['id'];mode=int(identifier)//6 if identifier.isdigit() else None
                        payload['input_kind']='PUBLISHED_EXAMPLE_SETTINGS' if identifier.startswith('published-') else 'SUPPLEMENTAL_SINGULAR_TIED_TWO_VARIABLE_MODEL' if mode is None else 'SYNTHETIC_API_SHAPE_OR_OPTION_OUTSIDE_UI_ROUTE' if mode in [29,30,38,39,40,41,42,44,45,46,47,48,49,50,51] else 'TYPED_ACQUIRED_INPUT_OR_GRID_CASE'
                        payload['rejection_boundary_note']='Preparation includes source grouping, original grid parsing/validation and original Core constructor; core-fit means the authoritative Fit rejection. API-invalid options/shapes are not claims of GUI availability.'
                        payload['dataset_typed_sha256']=root.findtext('dataset_sha256');payload['request_sha256']=root.findtext('request_sha256')
                        payload['options']={n.attrib['name']:dict(type=n.attrib['type'],value=n.text) for n in root.findall('requested_options/option')}
                        payload['source_points']=[dict(**p.attrib,input_rows=p.findtext('input_rows'),physical_rows=p.findtext('physical_rows')) for p in root.findall('source_point')]
                        rows=root.find('request_rows')
                        if rows is not None:payload['request_rows']=dict(**rows.attrib,metadata_sha256=rows.findtext('metadata_sha256'))
                        exception=root.find('exception')
                        if exception is not None:payload['exception']=dict(**exception.attrib,message=exception.text)
                        fit=root.find('result')
                        if fit is not None:
                            model=dict(fit.attrib);model['model_sha256']=fit.findtext('model_sha256');model['diagnostics_sha256']=fit.findtext('diagnostics_sha256');model['tables_sha256']=fit.findtext('tables_sha256');model['warnings']=[w.text for w in fit.findall('warnings/warning')];model['variable_names']=[v.text or '' for v in fit.findall('variable_names/name')]
                            model['diagnostic_tables']=[dict(**t.attrib,sha256=t.text) for t in fit.findall('diagnostic_tables')]
                            model['panels']=[]
                            for p in fit.findall('panel'):
                                points=p.findall('point');columns=sorted({k for point in points for k in point.attrib})
                                panel=dict(p.attrib);panel.update(rendering_input_sha256=p.findtext('rendering_input_sha256'),point_columns=columns,points=[[v.attrib.get(k) for k in columns] for v in points],physical_rows=[v.findtext('source_rows') for v in points],signals=[dict(**s.attrib,message=s.text) for s in p.findall('signal')])
                                model['panels'].append(panel)
                            diagnostics=fit.findall('diagnostic');columns=sorted({k for d in diagnostics for k in d.attrib})
                            model['diagnostic_columns']=columns;model['diagnostics']=[[d.attrib.get(k) for k in columns]+[d.findtext('optional_arrays_sha256')] for d in diagnostics]
                            payload['result']=model
                            shapes.setdefault(payload['chart'],set()).add(tuple(p['type'] for p in model['panels']))
                        key=hashlib.sha256(json.dumps(payload,sort_keys=True,ensure_ascii=False,separators=(',',':')).encode()).hexdigest()
                        payloads.setdefault(key,payload)
                        records.append(dict(profile_id=key,configuration=config,tfm=tfm,runtime=runtime,test_identity=result.attrib['testName'],test_outcome=result.attrib['outcome'],raw_xml_sha256=hashlib.sha256(xml).hexdigest(),raw_trx_sha256=trx_sha,raw_receipt='LOCAL_TRX:'+label+'-'+suite))
    assert records and all(r['test_outcome']=='Passed' for r in records)
    counts=Counter(payloads[r['profile_id']]['outcome'] for r in records)
    write('multivariate-spc-same-core-comparisons-v1.json',dict(version=1,status='NOT_GOLDEN',purpose='SAME_CORE_COMPATIBILITY_ONLY',promoted=False,record_count=len(records),payload_count=len(payloads),outcomes=dict(counts),records=records,payloads=payloads,tolerance='Exact CLR types/array shape/property values and IEEE-754 Double bits, including signed zero. Request compared to frozen entry constructor/loops; post-fit paths receive ONE unchanged Core Fit result. Old table builders remain source-identical; diagnostic selection and rendering-input arrays compared against original loops. Table hashes normalize only established execution-time cells. Core rejections and preparation errors are separate observations, not positive statistical parity.',payload_encoding='Columnar point/diagnostic arrays retain original names/types/bits/source lists. SHA-256 keys hash UTF-8 canonical JSON (sorted keys, ensure_ascii=False, separators comma/colon). Diagnostics final column is optional_arrays_sha256. Runtime/configuration belongs to each execution record; culture belongs to payload.',fixture=dict(path='BESHStatNG_Help_MkDocs/docs/assets/data/116multivariatecontrolcharts/116multivariatecontrolcharts.csv',sha256=c.sha(ROOT/'BESHStatNG_Help_MkDocs/docs/assets/data/116multivariatecontrolcharts/116multivariatecontrolcharts.csv'),columns=['Temperature_C','Pressure_kPa','Flow_L_min'],profiles='0..359: deterministic InputFor/FillGrids transformations, stages baseline first 2/3, exclusion point 3 (or explicit negative). published-0..6: original 100/50 individual or 20/10 subgroup phases, points36..40/subgroup8 exclusions, PCA k2, lambda .2, ridge0. All inputs/options/negative mutations reproducible from pinned Spc23Data/Spc23Fixture/Spc23Tests.')),compact=True)
    model_observations=Counter((p['chart'],p['result']['model_source'],p['result']['effective_dimension'],p['result']['pseudo_inverse'],len(p['result'].get('variable_names',[]))) for p in payloads.values() if 'result' in p)
    write('multivariate-family-observations-v1.json',dict(version=1,status='NOT_GOLDEN',panels={k:sorted(v) for k,v in shapes.items()},negative_counts=dict(counts),effective_model_observations=[dict(chart=k[0],source=k[1],effective_dimension=k[2],used_pseudo_inverse=k[3],variable_count=k[4],unique_payloads=n) for k,n in sorted(model_observations.items())],UI_choices=6,scope='Actual Core returned panel/model order and flags only. No family-specific plot geometry extraction or 04A.24 implementation.'))
    for name,scope in [('source-point-stage-mapping-v1.json','Physical source rows, acquired row ordinals, first-seen Trim/Ordinal groups, logical/display points, manual/quick/import phases and exclusions; raw N row expansion from logical G grid ranges.'),('multivariate-model-intents-v1.json','Ordered variable axis, model source and historical P/PxP detached shape; raw options versus reported Core effective rank, pseudoinverse, retained components, baseline counts, df and warnings. No covariance/limit computation in Common.')]:
        write(name,dict(version=1,status='NOT_GOLDEN / SAME_CORE_COMPATIBILITY_ONLY',scope=scope,corpus='multivariate-spc-same-core-comparisons-v1.json',corpus_sha256=c.sha(OUT/'multivariate-spc-same-core-comparisons-v1.json'),positive_records=counts['POSITIVE_PARITY'],other_observations=sum(counts.values())-counts['POSITIVE_PARITY'],native='NOT_RUN / UNAUTHORIZED'))
    print('Corpus',len(records),'records',len(payloads),'payloads',dict(counts))

def inventory(focused=False):
    cases={}
    for config,label in [('Debug','focused-15'),('Release','focused-16')]:
        if not focused:label=('final-complete-' if (RUN/('final-complete-'+config+'-full-command.json')).exists() else 'final-qualified-' if (RUN/('final-qualified-'+config+'-full-command.json')).exists() else 'final-')+config
        for suite in ['Common','Legacy']:
            path=RUN/'executions'/(label+'-'+suite)/'results'/(suite+'.trx')
            receipt_sha=c.sha(path)
            for r in previous.old.old.trx(path)['records']:
                if 'Spc23' not in r['class_name']:continue
                name=r['test_name'].replace('"','');key=r['class_name'].split('.')[-1]+'|'+name
                source='BESHStatNG.Test/src/Spc23AdapterTests.vb' if 'Adapter' in r['class_name'] else 'BESHStat.Excel.Common.Tests/src/Spc23Tests.vb'
                row=cases.setdefault(key,dict(method=r['method'],source=source,source_sha256=c.sha(ROOT/source),requirement=r['method'],production_symbols=['Ui22MultivariateControlCharts.BuildRequest','SpcMultivariatePointMapPolicy','SpcMultivariatePhasePolicy','SpcMultivariateStageBuilder','SpcMultivariateExclusionBuilder','SpcMultivariateRequestPolicy','SpcMultivariateResultAxisPolicy'],expectation='Captured original preparation parity / exact same-Core result context or named identity, negative, culture, immutable and compiled-call-site contract.',executions=[]))
                row['executions'].append(dict(configuration=config,suite=suite,outcome=r['outcome'],receipt_sha256=receipt_sha))
    assert cases and all(all(e['outcome']=='Passed' for e in r['executions']) for r in cases.values())
    requirements={
        'CapturedUiRequestAndSameCoreResultRemainCompatible':['3.1','3.2','3.3','3.4','3.5','5:legacy/same-Core/culture/rejections'],
        'SixOrdinaryPublishedDataMethodsProduceAuthoritativeResults':['3.1:six methods','3.5:actual returned panels'],
        'PublishedPhaseAndExclusionExampleKeepsActualModelAndMasks':['3.3:phase/exclusion','3.5:masks/alerts/labels','5:public fixture'],
        'CompatibilityRetainsSixChartStructureAndEnabledStatePredicates':['3.1:compatibility/enabled state'],
        'GroupedNormalizationPreservesSignedZeroAndFailurePartialMutation':['3.2:grouped metadata','3.6:exception timing/Double bits'],
        'PhysicalRowsGroupedIdentityAndDefensiveSnapshotsRemainDistinct':['3.2:five identities/row expansion','3.3:reason union','3.6:copy safety'],
        'HistoricalAxesAreDetachedAndNoCovarianceCorrectionOccurs':['3.4:ordered historical axes/shape','3.6:copy safety'],
        'ResultContextAndRequestArraysAreDetachedFromCallerMutations':['3.5:actual Core result context','3.6:copy safety'],
        'SparseRenderingInputKeepsFirstSourceAndCoreIndexFallback':['3.5.2:source provenance','3.5.4:legacy renderer bridge'],
        'QuickPhaseRowsAndRejectionsMatchCapturedSource':['3.3.1:quick phases/rejection order'],
        'ImportedPhaseRunsPreserveCollapseAndPartialGridErrors':['3.3.3:imports/lazy partial grid'],
        'ExclusionImportUsesOnlyFlaggedRowsReasonsAndOrdinalUniqueness':['3.3.4:exclusion import/reasons'],
        'ThousandsOfNoncontiguousMembersRetainFirstSeenOrderAcrossRuns':['3.2:first-seen grouping','3.6:bounded repeated runs'],
        'SingularTiedAndTwoVariableModelChoicesKeepOriginalRequests':['3.4:historical/singular/ridge/pseudoinverse/effective dimension','5:two-variable/tied/culture observations'],
        'ShippingCompatibilityWrapperUsesActualResultAndSharedRenderer':['3.5:shipping post-fit consumption','4:compiled real call-site/renderer compatibility']}
    profile_index={}
    for r in json.loads((OUT/'multivariate-spc-same-core-comparisons-v1.json').read_bytes())['records']:
        profile_index.setdefault(r['test_identity'].replace('"',''),set()).add(r['profile_id'])
    for key,row in cases.items():
        row['requirement_sections']=requirements[row['method']]
        row['profile_ids']=sorted(profile_index.get(key.split('|',1)[1],set()))
        row['fixture']='Source-pinned Spc23Fixture/InputFor/FillGrids, captured original Ui22 helper, public CSV and named targeted case; per-profile full settings/axes/hashes in corpus when emitted.'
    py='scripts/tests/test_04a23_multivariate_spc.py';names=re.findall(r'    def (test_\w+)\(self\):',(ROOT/py).read_text(encoding='utf-8'))
    write('test-inventory.json',dict(logical_count=len(cases),cases=cases,python_count=len(names),python_cases={n:dict(source=py,source_sha256=c.sha(ROOT/py),requirement=n) for n in names},scope='Full managed logical identities, linked Common cases deduplicated; exact raw executions retained per configuration/framework. Negative-fit observations are not positive scientific validations.'))
    print('New logical inventory',len(cases),'Python',len(names))

def supporting():
    baseline=json.loads((RUN/'baseline.json').read_bytes())['source_pins'];projects={}
    for project in ['BESHStat.Core/BESHStat.Core.vbproj','BESHStat.Excel.Common/BESHStat.Excel.Common.vbproj','BESHStat.Excel.Common.Tests/BESHStat.Excel.Common.Tests.vbproj','BESHstatNG/BESHStatNG.vbproj','BESHStatNG.Test/BESHStatNG.Tests.vbproj']:
        blob=subprocess.check_output(['git','-c','safe.directory='+ROOT.as_posix(),'show',c.BASE+':'+project],cwd=ROOT)
        before=ET.fromstring(blob);after=ET.parse(ROOT/project).getroot()
        includes=lambda tree:[n.get('Include') for n in tree.iter() if n.tag.split('}')[-1]=='Compile' and n.get('Include')]
        folder=Path(project).parent.as_posix()+'/'
        new=[p for p in c.git('ls-files','--others','--exclude-standard').splitlines() if p.startswith(folder) and p.endswith('.vb')]
        projects[project]=dict(entry_working_sha256=baseline[project],entry_git_blob_sha256=hashlib.sha256(blob).hexdigest(),final_sha256=c.sha(ROOT/project),entry_explicit_compile_includes=includes(before),final_added_explicit_compile_includes=sorted(set(includes(after))-set(includes(before))),entry_implicit_compile_sources=[p for p in baseline if p.startswith(folder) and p.endswith('.vb')] if before.get('Sdk') else [],final_implicit_added_sources=new if after.get('Sdk') else [])
    write('project-compilation-inventory.json',dict(projects=projects,targets_unchanged=True,packages_unchanged=True,Windows='Compile/CopyFilesToOutputDirectory only; no generators/signing/packaging; Core/Common no-restore.'))
    raw=json.loads((OUT/'prechange-method-provenance.json').read_bytes())['methods']
    request=raw['BuildRequest']['body']
    mapping={k:v.strip() for k,v in re.findall(r'(\w+):=([^,\n]+)',request)}
    designer='BESHstatNG/src/UI/Ui22MultivariateControlCharts.Designer.vb'
    designer_text=(ROOT/designer).read_text(encoding='utf-8-sig')
    controls={control:[line.strip() for line in designer_text.splitlines() if re.search(r'\b'+re.escape(control)+r'\.(?:Value|Minimum|Maximum|Increment|DecimalPlaces|Checked|Enabled|Text)\s*=',line)] for control in sorted(set(re.findall(r'\b(?:spin|chk|cb|rb)\w+',request)))}
    chart_rows=[dict(chart=k,allows_subgroups=k in ['HotellingT2','GeneralizedVariance'],requires_subgroups=k=='GeneralizedVariance',individual_only=k not in ['HotellingT2','GeneralizedVariance'],lower_checkbox=k=='HotellingT2',alpha_disabled='MCUSUM always; MEWMA supplied UCL; GV supplied sigma multiplier',effective_calculations='Core only') for k in ['HotellingT2','GeneralizedVariance','PcaT2','PcaQ','Mewma','Mcusum']]
    runtime_controls={control:[dict(method=n,statement=line.strip()) for n,r in raw.items() for line in r['body'].splitlines() if re.search(r'\b'+re.escape(control)+r'\.(?:Value|Minimum|Maximum|Increment|DecimalPlaces|Checked|Enabled|Text)\s*=',line)] for control in controls}
    write('request-option-coverage-v1.json',dict(version=1,charts=chart_rows,original_constructor_mapping=mapping,original_numeric_control_settings=controls,original_runtime_control_assignments=runtime_controls,control_default_precedence='Designer property declarations are raw initial values; original form initialization assignments override them. Chart/selection-dependent Enabled/Checked/Maximum assignments remain native and source-identical apart from delegating pure predicates. Runtime statements are pinned by original method names/hashes.',original_optional_conversions=request[request.index('        Dim pcaCount As'):request.index('        Return New SpcMultivariateRequest')],designer_sha256=c.sha(ROOT/designer),source_sha256=json.loads((OUT/'prechange-method-provenance.json').read_bytes())['source_sha256'],production='Ui22 BuildRequest -> immutable model/options -> Common Create -> one original Core constructor; UpdateChartDependentControls/PopulateObservationStructures consume compatibility. Host numeric conversion retained. Requested values are not effective Core estimates.',Core_default_alpha=.0027,supported_missing=['Reject','OmitPoint'],unsupported_available_measurements=False,legacy_source='legacy-preparation-provenance.json'))
    write('toolchain.json',dict(dotnet_sdk=subprocess.check_output(['dotnet','--version'],cwd=ROOT,text=True).strip(),node=subprocess.check_output(['node','--version'],cwd=ROOT,text=True).strip(),python=platform.python_version(),python_executable='<PYTHON_EXE>',VS2026_MSBuild='18.11.1-1.26513.3+940033be3',frameworks=['netstandard2.0','net48','net10.0'],restore='NOT_RUN / offline existing packages',native='NOT_RUN / UNAUTHORIZED',R='NOT_RUN / UNAUTHORIZED'))
    observations={}
    for batch in range(14,23):
        manifest=ROOT/('evidence/04A'+str(batch)+'/publication-manifest.json')
        if not manifest.exists():continue
        differences=[]
        for p,r in json.loads(manifest.read_bytes()).get('files',{}).items():
            if not isinstance(r,dict) or 'sha256' not in r or p not in baseline or r['sha256']==baseline[p]:continue
            raw=(ROOT/p).read_bytes() if c.sha(ROOT/p)==baseline[p] else subprocess.check_output(['git','-c','safe.directory='+ROOT.as_posix(),'show',c.BASE+':'+p],cwd=ROOT)
            lf=raw.replace(b'\r\n',b'\n');variants=[hashlib.sha256(v).hexdigest() for v in [lf,lf.replace(b'\n',b'\r\n')]]
            differences.append(dict(path=p,historical_sha256=r['sha256'],live_entry_sha256=baseline[p],classification='LINE_ENDINGS_ONLY' if r['sha256'] in variants else 'PREEXISTING_ACCEPTED_SUCCESSOR_OR_MANIFEST_DISCREPANCY_NOT_04A23_MUTATION'))
        observations[str(batch)]=dict(manifest_sha256=c.sha(manifest),differences=differences)
    write('historical-manifest-observations.json',dict(observations=observations,interpretation='Frozen previous reviewed candidates are not current-base measurements. Live entry independently pinned; preexisting export line endings and accepted successor differences remain historical observations, never attributed to this batch.'))

def gate_receipts():
    gates={}
    for phase in ['entry','final']:
        gates[phase]={}
        for kind in ['strict','profile']:
            qualified='final-complete-historical-'+kind
            name=qualified if phase=='final' and (RUN/(qualified+'-command.json')).exists() else phase+'-historical-'+kind
            command=RUN/(name+'-command.json');log=RUN/(name+'.log')
            if not command.exists():continue
            record=json.loads(command.read_bytes());gates[phase][kind]=dict(command=record['command'],exit_code=record['exit_code'],raw_command_sha256=c.sha(command),raw_log_sha256=c.sha(log),diagnostic_lines=log.read_text(encoding='utf-8',errors='replace').splitlines(),status='UNMET' if kind=='strict' else 'INTEGRITY_FAILURE')
    write('gate-validation.json',dict(runs=gates,P='NO',N1='NO GO',N2='NO',C2='UNMET',native_releases=0,deferred_discharges=0,native='NOT_RUN / UNAUTHORIZED',R='NOT_RUN / UNAUTHORIZED'))
    workflows={}
    for name in ['entry-Debug','entry-Release','final-Debug','final-Release','final-qualified-Debug','final-qualified-Release','final-complete-Debug','final-complete-Release']:
        path=RUN/(name+'-verify-isolated.provenance.json')
        if path.exists():workflows[name]=dict(raw_receipt_sha256=c.sha(path),**json.loads(path.read_bytes()))
    write('verification-workflow-provenance.json',dict(original_verifier='scripts/verify-beshstatng.ps1',source_sha256=c.sha(ROOT/'scripts/verify-beshstatng.ps1'),source_unchanged=True,workflows=workflows,coverage='Existing full suite enumeration; offline/no-restore, fresh isolated result/output paths and Windows compile-only avoid overwriting historical logs or invoking generators/signing/packaging. Nonzero suites retained. No blanket test filters. Explicit WASM Node glob supplemental, not double-counted.'))

def transients():
    write('transient-validation.json',dict(all_durable_receipts_retained=True,details=[dict(attempt='focused-01',issue='New fixture syntax: VB loop variable split resolved to helper; missing inert audit controls in host-free legacy table helper.',resolution='Renamed splitPoint; explicit plain audit snapshots, new legacy-v3 draft/provenance. No shipping behavior changed.'),dict(attempt='focused-02',issue='Common 741 passed; Windows new adapter used Count(predicate) on IReadOnlyList.Count.',resolution='Explicit LINQ Where.Count; focused-03 Common741/Legacy747 passed.'),dict(attempt='focused-04',issue='New signed-zero test expected expansion despite a later-group conflict; both old/new preserve original unexpanded sequence.',resolution='Assert unchanged partial state on failure, then exact signed-zero after successful validation. No production correction.'),dict(attempt='focused-05',issue='New published subgroup fixture replaced historical options with Phase I estimation but retained historical arrays, triggering existing original Core-constructor rejection.',resolution='Set public example history to Nothing and GV requested pseudo flag to existing disabled state; final focused-09/10 pass Common757/Legacy763.'),dict(attempt='accounting setup',issue='Initial summary wrapper self-referenced replaced measure source, NameError.',resolution='Pin original measure source before replacement; rerun actual entry accounting successfully.'),dict(attempt='corpus/inventory initial generation',issue='Duplicate signal count/list dict key caused TypeError. First inventory repeatedly hashed each large TRX per case; first corpus redacted ancestors repeatedly.',resolution='Distinct update of signal list; hash receipt once and redact scalar leaves once using unchanged accepted sanitizer. Superseded own helper processes interrupted; local drafts and tool transcript retain attempts. No failed comparison/test replaced.'),dict(attempt='initial publication seal',issue='New privacy receipt did not yet exist; predecessor scanner rejected the missing candidate.',resolution='Create explicit INITIAL_UNSCANNED receipt, then scan actual bytes and emit PASS only after successful scan/seal. No old scanner/test relaxed.')],scope='Local build/TRX/command artifacts remain immutable within batch-qualified paths. Failed tool-generation attempts are in the tool transcript and this sanitized register, not invented as successful framework executions. Failed attempts never substituted for final qualifying receipts.'))
    p=OUT/'transient-validation.json';d=json.loads(p.read_bytes())
    d['details'] += [dict(attempt='initial final-Debug/Release matrices',issue='Every new test passed, but final source review found eager UI argument evaluation replacing original chart-specific short circuits.',resolution='Restore Hotelling-only observation lookup and applicable-chart-only supplied-limit checkbox reads; strengthen new source assertions. Retain v1 pins and full receipts. Complete final-qualified pair reran all suites on corrected adapter.'),dict(attempt='focused-11 / focused-12 covariance fixture builds',issue='New fixture used reserved Variant identifier; a broad rename also changed InvariantCulture spelling in the same new test file.',resolution='Use profileKind and restore exact InvariantCulture spelling; no protected test or production numerical source changed. Focused13/14 pass829 Common/835 Framework; focused15/16 also emit actual Core variable names.'),dict(attempt='final-complete-Debug/Release',issue='Additional successful pseudoinverse, tied historical covariance and two-variable observation fixtures added after previous full pair.',resolution='Retain v2 pins and previous receipts; rebuild and execute every complete suite on final production/test bytes. Only this last pair contributes final totals; all835 new logical managed and18 Python contracts pass.')]
    write(p.name,d)

def git_state():
    if not (OUT/'final-git-state.json').exists():write('final-git-state.json',{})
    assert not c.git('diff','--cached','--name-only')
    assert subprocess.run(['git','-c','safe.directory='+ROOT.as_posix(),'diff','--check'],cwd=ROOT).returncode==0
    assert subprocess.run(['git','-c','safe.directory='+ROOT.as_posix(),'merge-base','--is-ancestor',c.BASE,'HEAD'],cwd=ROOT).returncode==0
    write('final-git-state.json',dict(branch=c.git('branch','--show-current'),head=c.git('rev-parse','HEAD'),main=c.git('rev-parse','main'),origin_main_existing_ref=c.git('rev-parse','origin/main'),parent=c.git('rev-parse','HEAD^'),tree=c.git('rev-parse','HEAD^{tree}'),main_ancestry_verified=True,index_empty=True,diff_check_exit=0,status_short=c.git('status','--short','--untracked-files=all').splitlines(),owner_edit_preserved=json.loads((OUT/'owner-plan-observation.json').read_bytes()),ignored_paths='Local final-git-status-ignored receipt, sanitized command SHA in command-outcomes.json. Account-bearing historical file names excluded.',staged=False,committed=False,merged=False,pushed=False,next_batch_started=False))

def retention():
    proof=RUN/'preservation-final-retention.json';r=json.loads(proof.read_bytes());assert r['status']=='PASS' and not r['errors']
    nested=json.loads((RUN/'nested-initial-metadata.json').read_bytes());actual={p.relative_to(ROOT).as_posix() for folder in ['BESHStat.Core.Tests','BESHStat.Excel.Common.Tests','BESHStatNG.Test'] for p in (ROOT/folder/'TestResults').rglob('*') if p.is_file()}
    assert actual=={v['path'] for v in nested}
    for v in nested:
        stat=(ROOT/v['path']).stat()
        assert tuple(getattr(stat,'st_'+k) for k in ['size','mtime_ns','ctime_ns','ino','dev'])==tuple(v[k] for k in ['bytes','mtime_ns','ctime_ns','ino','dev']),v['path']
    write('retention.json',dict(historical_files=r['historical_files'],nested_historical_files=len(nested),membership_metadata_errors=0,selected_frozen_hashes='frozen-capture-hashes.json; source-preservation.json; original 100 captures rehashed',raw_receipt_alias='LOCAL_RECEIPT:'+proof.name,raw_receipt_sha256=c.sha(proof),method=r['method'],complete_historical_byte_audit=False))

def python_inventory():
    configurations={};prior=json.loads((ROOT/'evidence/04A22/validation.json').read_bytes())['configurations'];entry_data=json.loads((OUT/'entry-validation.json').read_bytes())['configurations'];final_data=json.loads((OUT/'validation.json').read_bytes())['configurations'];deltas={}
    for config in ['Debug','Release']:
        configurations[config]={}
        for phase,d in [('entry',entry_data[config]),('final',final_data[config])]:
            identities=set()
            for receipt in d['python_receipts']:
                identities.update(re.findall(r'^test_\w+ \(([\w.]+)\) \.\.\.',(RUN/receipt).read_text(encoding='utf-8',errors='replace'),re.M))
            assert len(identities)==d['python']['total'],(phase,config,len(identities),d['python']['total'])
            configurations[config][phase]=dict(total=len(identities),cases={name:d['python']['nonpasses'].get(name,'PASS') for name in sorted(identities)},receipts=[dict(alias='LOCAL_LOG:'+p,sha256=c.sha(RUN/p)) for p in d['python_receipts']],nonpass_method_counts=dict(Counter(d['python']['nonpasses'].values())))
        before=prior[config]['python']['nonpasses'];now=entry_data[config]['python']['nonpasses']
        deltas[config]=dict(previous_accepted_passed=prior[config]['python']['passed'],previous_accepted_total=prior[config]['python']['total'],actual_entry_passed=entry_data[config]['python']['passed'],actual_entry_total=entry_data[config]['python']['total'],new_entry_nonpass_identities=sorted(set(now)-set(before)),removed_nonpass_identities=sorted(set(before)-set(now)),changed_outcomes={k:dict(before=before[k],entry=v) for k,v in now.items() if k in before and before[k]!=v})
    write('python-test-identities.json',dict(configurations=configurations,scope='Actual verbose unittest method identities from all disjoint discovery roots, asserted equal to measured total; failed subtests remain attached to real parent methods. Raw failure events/traces retained separately.'))
    write('historical-entry-deltas.json',dict(previous_04A22_validation_sha256=c.sha(ROOT/'evidence/04A22/validation.json'),configurations=deltas,interpretation='Two newly triggered protected 04A.22 source/publication pins already fail at this actual merged-main/owner-2.9-plan entry. They are not 04A.23 regressions. No expectations rewritten; exact entry/final traces retained. Historical reported counts are not forced onto live main.'))

if __name__=='__main__':
    parser=argparse.ArgumentParser();parser.add_argument('action',choices=['entry','validation','authority','source','qualify','commands','final-qualification','retention','receipts','supporting','corpus','corpus-focused','inventory','inventory-focused','transients','git-state','python-inventory']);args=parser.parse_args()
    if args.action=='receipts':gate_receipts()
    elif args.action in ['corpus-focused','inventory-focused']:globals()[args.action.split('-')[0]](True)
    elif args.action.replace('-','_') in globals():globals()[args.action.replace('-','_')]()
    else:getattr(previous,args.action.replace('-','_'))()
