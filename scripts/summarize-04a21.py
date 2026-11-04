"""04A.21 receipt accounting; read-only legacy sources and isolated new evidence."""
import argparse,base64,csv,hashlib,importlib.util,json,platform,re,subprocess,sys
from pathlib import Path
from collections import Counter
import xml.etree.ElementTree as ET
ROOT=Path(__file__).resolve().parents[1]
def module(name,path):
 s=importlib.util.spec_from_file_location(name,ROOT/path);m=importlib.util.module_from_spec(s);s.loader.exec_module(m);return m
c=module('capture21','scripts/capture-04a21-contract.py')
book=module('book20','scripts/summarize-04a20.py')
RUN,OUT=c.RUN,c.OUT
old=book.old;old.RUN=RUN
book.RUN=RUN;book.OUT=OUT;book.c=c
NS=old.NS
sanitize=old.c.prior.prior.sanitize
def write(name,value):
 value=sanitize(value)
 def placeholders(v):
  if isinstance(v,dict):return {k:placeholders(item) for k,item in v.items()}
  if isinstance(v,(list,tuple)):return [placeholders(item) for item in v]
  if isinstance(v,str):return v.replace('<REPO>','<REPO_ROOT>')
  return v
 value=placeholders(value)
 if name=='spc-same-core-comparisons-v1.json':
  (OUT/name).write_text(json.dumps(value,ensure_ascii=False,separators=(',',':'),allow_nan=False)+'\n',encoding='utf-8',newline='\n')
 else:c.emit(name,value)
book.write=write
def measured(name, config):
    suites = old.suites_for(name, config)
    groups = old.previous.typed_groups(suites)
    weight = old.prior.weight
    new = {k: rows for k, rows in groups.items() if any('Spc21' in r['class_name'] for r in rows)}
    logical = dict(total=sum(weight(k) for k in groups), passed=sum(weight(k) for k,rows in groups.items() if all(r['outcome']=='Passed' for r in rows)))
    logical['nonpassed'] = logical['total']-logical['passed']
    for suite in suites:
        suite['batch21_total'] = sum('Spc21' in r['class_name'] for r in suite['records'])
        suite['batch21_passed'] = sum('Spc21' in r['class_name'] and r['outcome']=='Passed' for r in suite['records'])
        suite.pop('records'); suite.pop('batch11',None)
    python_logs = [RUN/(name+'-full.log'),RUN/(name+'-common-python.log')]
    if name.startswith('final-') and (RUN/('final-python-qualified-'+config+'-common-python.log')).exists():
        python_logs=[RUN/('final-python-'+config+'-'+root+'.log') for root in ['tools','help','scripts']]+[RUN/('final-python-qualified-'+config+'-common-python.log')]
    py = old.previous.python(python_logs)
    log = (RUN/(name+'-full.log')).read_text(encoding='utf-8',errors='replace')
    return dict(managed_suites=suites, logical_cases=logical,
        runtime_executions=dict(total=sum(int(s['counters']['total']) for s in suites),passed=sum(int(s['counters']['passed']) for s in suites),failed=sum(int(s['counters']['failed']) for s in suites)),
        new_logical_total=sum(weight(k) for k in new),new_logical_passed=sum(weight(k) for k,rows in new.items() if all(r['outcome']=='Passed' for r in rows)),
        node_passed=sum(map(int,re.findall(r'\bpass (\d+)\s*$',log,re.M))),node_total=sum(map(int,re.findall(r'\btests (\d+)\s*$',log,re.M))),
        python=py,python_receipts=[p.relative_to(RUN).as_posix() for p in python_logs],full_command_exit=json.loads((RUN/(name+'-full-command.json')).read_bytes())['exit_code'])

book.measured=measured

def entry():book.entry()
def validation():
 book.validation()
 if (OUT/'python-requalification.json').exists():
  value=json.loads((OUT/'validation.json').read_bytes());value['python_requalification']='python-requalification.json; initial nonzero full/attempt receipts retained separately.';write('validation.json',value)
def commands():book.commands()

def numerical(prefix='final'):
 records=[];payloads={}
 for config in ['Debug','Release']:
  for tfm,suite in [('net10.0','Common'),('net48','Legacy')]:
   path=RUN/'executions'/(prefix+'-'+config+'-'+suite)/'results'/(suite+'.trx')
   if not path.exists() and config=='Debug':path=RUN/'executions'/(prefix+'-'+suite)/'results'/(suite+'.trx')
   if not path.exists():continue
   digest=c.sha(path)
   for node in ET.parse(path).getroot().findall('.//'+NS+'UnitTestResult'):
    stdout=node.find('.//'+NS+'StdOut')
    for line in (stdout.text if stdout is not None and stdout.text else '').splitlines():
     if not line.startswith('04A21_XML|'):continue
     data=sanitize(old.xml_value(ET.fromstring(base64.b64decode(line.split('|',1)[1]))))
     key=hashlib.sha256(json.dumps(data,sort_keys=True,ensure_ascii=False,separators=(',',':'),allow_nan=False).encode()).hexdigest()
     payloads.setdefault(key,data)
     records.append(dict(profile_id=key,configuration=config,tfm=tfm,os='Windows',test_identity=node.attrib['testName'],outcome=node.attrib['outcome'],raw_trx_alias='LOCAL_TRX:'+path.parents[1].name,raw_trx_sha256=digest))
 assert records
 write('spc-same-core-comparisons-v1.json',dict(version=1,status='NOT_GOLDEN',validation='SAME_CORE_COMPATIBILITY_ONLY',record_count=len(records),payload_count=len(payloads),records=records,payloads=payloads,outcomes=dict(Counter(payloads[r['profile_id']]['classification'] for r in records)),payload_identity='SHA-256 canonical sorted compact UTF-8 JSON, ensure_ascii=False; raw TRX hashes are separate.',tolerance='Exact same-runtime request, panel/point/parameter/signal and table value/type/header/format/order hashes. Only the three named Execution time (ms)/started UTC/completed UTC values are replaced with a typed volatile marker in table comparison; no statistical value normalization or scientific tolerance.',dataset=dict(path='BESHStatNG_Help_MkDocs/docs/assets/data/115controlcharts/115controlcharts.csv',sha256=c.sha(ROOT/'BESHStatNG_Help_MkDocs/docs/assets/data/115controlcharts/115controlcharts.csv'),rows=60,assumptions='Exact public decimal literals embedded in new Spc21Fixture; variants explicitly alter missing/invalid/layout/source metadata. No workbook or independent oracle read.'),legacy='Frozen original Windows pure helper/loop derivative; legacy-preparation-provenance.json pins original source. New policy is not the legacy preparation oracle.',native='NOT_RUN / UNAUTHORIZED',independent_R='NOT_RUN / UNAUTHORIZED',promoted=False))
 print('Profiles',len(records),'payloads',len(payloads),'outcomes',dict(Counter(payloads[r['profile_id']]['classification'] for r in records)))

def source():
 assert c.git('rev-parse','HEAD')==c.git('rev-parse','main')==c.BASE
 assert c.git('branch','--show-current')=='codex/04A-21' and not c.git('diff','--cached','--name-only')
 changes=set(c.git('diff','--name-only').splitlines());allowed=set(json.loads((OUT/'implementation-plan.json').read_bytes())['modified']);assert changes<=allowed,changes-allowed
 pins=json.loads((RUN/'baseline.json').read_bytes())['source_pins']
 for p,h in pins.items():
  if p not in changes:assert c.sha(ROOT/p)==h,p
 for p,r in json.loads((OUT/'frozen-capture-hashes.json').read_bytes())['files'].items():assert c.sha(ROOT/p)==r['sha256'],p
 path='BESHstatNG/src/UI/Ui21ControlCharts.vb';before=old.c.members(path,(RUN/'preimages/Ui21ControlCharts.vb').read_text(encoding='utf-8-sig'));after=old.c.members(path,(ROOT/path).read_text(encoding='utf-8-sig'))
 changed={n for n in before if before[n]!=after[n]}
 write('source-preservation.json',dict(base=c.BASE,changed={p:dict(before=pins[p],after=c.sha(ROOT/p),bytes=(ROOT/p).stat().st_size) for p in sorted(changes)},changed_form_methods={n:dict(before=hashlib.sha256(before[n].encode()).hexdigest(),after=hashlib.sha256(after[n].encode()).hexdigest()) for n in sorted(changed)},unchanged_form_methods=sorted(set(before)-changed),unchanged_entry_tracked_files=len(pins)-len(changes),Core_Spc_numerics_result_tables_native_Ui22_Designer_UDF_importer_targets_packages_unchanged=True,index_empty=True,private_all_tracked_pins=dict(alias='LOCAL_BASELINE:all-tracked-entry',sha256=c.sha(RUN/'baseline.json'))))
 registry=json.loads((ROOT/'evidence/04A14/baseline-registry.json').read_bytes());assert len(registry['files'])==100
 for p,r in registry['files'].items():assert c.sha(ROOT/p)==r['sha256'],p
 print('PASS source/frozen guards;',len(changed),'changed form methods')

def authority():
 prior=json.loads((ROOT/'evidence/04A20/original-control-crosswalk.json').read_bytes())
 csvpath=next(p for p in prior['authorities'] if p.endswith('.csv'));jsonpath=next(p for p in prior['authorities'] if p.endswith('.json'))
 controls=list(csv.DictReader((ROOT/csvpath).read_text(encoding='utf-8-sig').splitlines()));ledger=json.loads((ROOT/jsonpath).read_bytes())
 selected=[r for r in controls if Path(r['path']).name in ['Ui21ControlCharts.Designer.vb','Ui22MultivariateControlCharts.Designer.vb']]
 ids={r['control_id'] for r in selected};obligations=[r for r in ledger if set(r.get('linked_control_ids',[]))&ids]
 restricted={r['obligation_id'][len('control:'):] for r in ledger if r['obligation_id'].startswith('control:') and r.get('affected_live_golden_blocked')}
 populations={}
 for host in ['Ui21ControlCharts','Ui22MultivariateControlCharts']:
  group={r['control_id'] for r in selected if Path(r['path']).name==host+'.Designer.vb'}
  populations[host]=dict(controls=len(group),linked_open_obligations=sum(bool(group&set(r.get('linked_control_ids',[]))) for r in obligations),restricted_original_controls=len(group&restricted),control_ids=sorted(group),obligation_ids=sorted(r['obligation_id'] for r in obligations if group&set(r.get('linked_control_ids',[]))))
 digest=lambda v:hashlib.sha256(json.dumps(v,sort_keys=True,separators=(',',':'),ensure_ascii=False).encode()).hexdigest()
 both=[r['obligation_id'] for r in obligations if all(set(populations[h]['control_ids'])&set(r.get('linked_control_ids',[])) for h in populations)]
 casepath='docs/architecture/04A02/native-restriction-crosswalk-v1.json';cases=json.loads((ROOT/casepath).read_bytes())
 native_overlap={name:sorted(ids&set(row['blocked_control_ids'])) for name,row in cases['forms'].items()}
 renderer=[r for r in ledger if 'SpcControlChartExcel' in json.dumps(r.get('source_evidence',{}),ensure_ascii=False)]
 renderer_overlap=sorted(ids&{v for r in renderer for v in r.get('linked_control_ids',[])})
 restriction_fields=['source_record_path','source_record_sha256','earliest_approved_batches','dependent_batches','blocked_gate','safe_capture_preconditions','acceptance_condition','affected_live_golden_blocked','verification_status','owner_decision_status','capture_authorized','full_contract_newly_qualified','approval_authority']
 copied=[dict(obligation_id=r['obligation_id'],linked_control_ids=r.get('linked_control_ids',[]),original_row_sha256=digest(r),original_control_sha256=r.get('source_evidence',{}).get('original_control_sha256'),status='OPEN',restrictions={k:r.get(k) for k in restriction_fields}) for r in obligations]
 write('original-control-crosswalk.json',dict(authorities={p:dict(sha256=c.sha(ROOT/p),bytes=(ROOT/p).stat().st_size) for p in [csvpath,jsonpath,casepath]},controls=[dict(control_id=r['control_id'],path=r['path'],source_row_sha256=digest(r)) for r in selected],obligations=copied,populations=populations,overlap_obligation_ids=both,original_case_control_overlap=native_overlap,native_renderer=dict(path='BESHstatNG/src/Graphics/SpcControlChartExcel.vb',sha256=c.sha(ROOT/'BESHstatNG/src/Graphics/SpcControlChartExcel.vb'),source_evidence_linked_obligation_ids=[r['obligation_id'] for r in renderer],shared_control_ids=renderer_overlap,interpretation='Exact original ledger source_evidence string/linked-control trace only; absence of a link is not native clearance.'),union=dict(controls=len(ids),linked_open_obligations=len(obligations)),broader_union=dict(controls=len(controls),obligations=len(ledger),restricted_original_controls=len(restricted)),releases=0,discharges=0,P='NO',N1='NO GO',N2='NO',C2='UNMET',overlap='Host/control/native-case populations overlap, never summed as disjoint. All original case restrictions retained by frozen authorities; source tests discharge none.',native='NOT_RUN / UNAUTHORIZED'))
 print('SPC controls',[(k,{q:v for q,v in row.items() if q not in ['control_ids','obligation_ids']}) for k,row in populations.items()],'union',len(ids),len(obligations),'overlap',len(both))

def documentation():
 path='BESHStatNG_Help_MkDocs/docs/methods/control-charts.md';pins=json.loads((RUN/'baseline.json').read_bytes())['source_pins'];metadata='BESHStatNG_Help_MkDocs/docs/_meta/methods.yml';assert c.sha(ROOT/metadata)==pins[metadata]
 receipts={n:dict(alias='LOCAL_COMMAND:'+n,raw_sha256=c.sha(RUN/(n+'-command.json')),exit_code=json.loads((RUN/(n+'-command.json')).read_bytes())['exit_code']) for n in ['entry-mkdocs','final-mkdocs'] if (RUN/(n+'-command.json')).exists()}
 write('documentation-audit.json',dict(page=dict(path=path,entry_sha256=pins[path],final_sha256=c.sha(ROOT/path),bytes=(ROOT/path).stat().st_size),metadata_sha256=pins[metadata],metadata_changed=False,screenshots_data_workbooks_changed=False,strict_mkdocs=receipts,availability='Windows dialog only; portable preparation is not Office/Mac GUI capability. No native recapture.'))

def inventory(prefix='final'):
 corpus=json.loads((OUT/'spc-same-core-comparisons-v1.json').read_bytes());cases={}
 for config in ['Debug','Release']:
  for suite in ['Common','Legacy']:
   path=RUN/'executions'/(prefix+'-'+config+'-'+suite)/'results'/(suite+'.trx')
   if not path.exists():continue
   for r in old.old.trx(path)['records']:
    if 'Spc21' not in r['class_name']:continue
    name=r['test_name'].replace('"','');key=r['class_name'].split('.')[-1]+'|'+name
    file='BESHStatNG.Test/src/Spc21AdapterTests.vb' if 'Adapter' in r['class_name'] else 'BESHStat.Excel.Common.Tests/src/Spc21Tests.vb'
    case=cases.setdefault(key,dict(method=r['method'],source=file,source_sha256=c.sha(ROOT/file),expectation=name,fixture='Spc21Fixture/public 115controlcharts.csv or named targeted parsed-value/identity/negative fixture',production_symbols=['Ui21ControlCharts.BuildRequest','SpcInputPointPolicy','SpcPhaseStagePolicy','SpcExclusionPolicy','SpcLimitRequest','SpcRuleRequest','SpcRequestPreparation'],profile_ids=sorted({v['profile_id'] for v in corpus['records'] if v['test_identity'].replace('"','')==name}),executions=[]));case['executions'].append(dict(receipt=path.relative_to(RUN).as_posix(),outcome=r['outcome']))
 py='scripts/tests/test_04a21_spc.py';pycases=re.findall(r'    def (test_\w+)\(self\):',(ROOT/py).read_text(encoding='utf-8-sig')) if (ROOT/py).exists() else []
 files=[p.as_posix() for p in [Path('BESHStat.Excel.Common.Tests/src/Spc21Tests.vb'),Path('BESHStat.Excel.Common.Tests/src/Spc21Fixture.vb'),Path('BESHStat.Excel.Common.Tests/src/Spc21LegacyPreparation.vb'),Path('BESHStatNG.Test/src/Spc21AdapterTests.vb'),Path(py)] if (ROOT/p).exists()]
 write('test-inventory.json',dict(logical_count=len(cases),cases=cases,python_count=len(pycases),python_cases={n:dict(source=py,source_sha256=c.sha(ROOT/py),expectation=n,classification='SOURCE_AUTHORITY_EVIDENCE_CONTRACT') for n in pycases},source_hashes={p:c.sha(ROOT/p) for p in files},supplemental_comparisons=dict(path='spc-negative-host-seams-v1.json',sha256=c.sha(OUT/'spc-negative-host-seams-v1.json'),records=6,meaning='Direct invocation of qualified net48 frozen legacy helper and shipping policy for typed empty/blank subgroup IDs and exact physical-row diagnostics. Additional compatibility observations, not new framework/logical test identities.'),counting='Linked Common tests count once logically and twice as raw framework executions; culture/profile loops do not inflate logical totals. Existing Foundation/Control overlap conventions retained.'))
 print('Inventory logical',len(cases),'Python',len(pycases))

def qualification():
 retention_path=RUN/'preservation-final-retention-closeout.json'
 if not retention_path.exists():retention_path=RUN/'preservation-final-retention.json'
 raw=json.loads(retention_path.read_bytes());assert raw['status']=='PASS' and not raw['errors']
 nested=json.loads((RUN/'nested-initial-metadata.json').read_bytes());current={p.relative_to(ROOT).as_posix() for name in ['BESHStat.Core.Tests','BESHStat.Excel.Common.Tests','BESHStatNG.Test'] for p in (ROOT/name/'TestResults').rglob('*') if p.is_file()};assert current=={r['path'] for r in nested}
 for r in nested:
  st=(ROOT/r['path']).stat();assert (st.st_size,st.st_mtime_ns,st.st_ctime_ns,st.st_ino,st.st_dev)==tuple(r[k] for k in ['bytes','mtime_ns','ctime_ns','ino','dev'])
 pins=json.loads((RUN/'final-input-pins.json').read_bytes())
 for p,h in pins.items():assert c.sha(ROOT/p)==h,p
 write('final-source-qualification.json',dict(final_input_pins=pins,compiled_sources_unchanged_after_final_suites=True,snapshot_sha256=c.sha(RUN/'final-input-pins.json'),component_selection='Both complete final matrices and final focused reruns use these production/test/Python/Help bytes. Final Python selects all three sequential source discovery roots plus corrected Common discovery using already executed final-source semantic observations; exact receipts in python-requalification.json. Initial full and incorrect-env attempts remain nonzero and separately recorded. Supplemental framework reruns/API comparisons excluded from totals.',python_requalification_sha256=c.sha(OUT/'python-requalification.json'),native='NOT_RUN / UNAUTHORIZED',independent_R='NOT_RUN / UNAUTHORIZED'))
 write('retention.json',dict(status='PASS_HISTORICAL_MEMBERSHIP_METADATA',root_files=raw['historical_files'],nested_files=len(nested),membership_metadata_errors=0,receipt=dict(alias='LOCAL_RETENTION:'+retention_path.stem,sha256=c.sha(retention_path)),scope='All initial path membership/size/creation-write nanoseconds/file identity; frozen source/payload/authority additionally SHA-256 checked. Not an all-historical-byte reread.',retained_through='End of Phase04A; no cleanup/compression/relocation/overwrite.'))
 write('toolchain.json',dict(python=dict(executable='<PYTHON_EXE>',version=platform.python_version(),executable_sha256=c.sha(Path(sys.executable)),resolved='Configured portable WinPython; private resolved path kept in local receipt only.'),dotnet_sdk=subprocess.check_output(['dotnet','--version'],cwd=ROOT,text=True).strip(),node=subprocess.check_output(['node','--version'],cwd=ROOT,text=True).strip(),os=dict(system=platform.system(),release=platform.release(),version=platform.version(),architecture=platform.machine()),TFMs=dict(core='netstandard2.0',common='netstandard2.0',windows='net48',managed_office_tests='net10.0'),packages='Existing offline packages; no network/restore/install'))

if __name__=='__main__':
 p=argparse.ArgumentParser();p.add_argument('action',choices=['entry','validation','numerical','source','commands','authority','documentation','inventory','qualification']);p.add_argument('--prefix',default='final');a=p.parse_args()
 if a.action in ['numerical','inventory']:globals()[a.action](a.prefix)
 else:globals()[a.action]()
