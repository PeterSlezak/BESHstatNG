"""04A32 independently verifiable publication from real isolated receipts.
No historical rewrite;failed intermediate receipts remain local and classified.
"""
from audit import ROOT,HERE,RUN,BASE,WINDOWS,CORE_NEW,CORE_OLD,PROJECT,git,sha,pin,save,load,protection,public_path
from pathlib import Path
from collections import Counter,defaultdict
import csv,hashlib,json,os,re,sys
DOSSIER='docs/architecture/batch-04A-32-owner-review.md'
TESTS=['BESHStat.Core.Tests/src/Graphics/'+n+'.vb' for n in ['Graphics32Tests','Graphics32ViolinTests','LegacyGraphics32']]+['BESHStatNG.Test/src/Charts/Graphics32AdapterTests.vb']
SUITES=['Core','Common','Legacy','OfficeAddin','OfficeExecution','ControlMitigation']
def text(p):
 b=Path(p).read_bytes()
 try:return b.decode('utf-8')
 except UnicodeDecodeError:return b.decode('cp1252')
def clean(v):
 if isinstance(v,list):return [clean(x) for x in v]
 if isinstance(v,dict):return {k:clean(x) for k,x in v.items()}
 if not isinstance(v,str):return v
 for a,b in [(str(RUN),'<ISOLATED_RUN>'),(RUN.as_posix(),'<ISOLATED_RUN>'),(str(ROOT),'<REPO_ROOT>'),(ROOT.as_posix(),'<REPO_ROOT>'),(sys.executable,'<PYTHON>')]:v=v.replace(a,b)
 v=re.sub(re.escape(os.environ.get('COMPUTERNAME','!unset!')),'<HOST>',v,flags=re.I)
 v=re.sub(r'[A-Za-z]:[\\/]+Users[\\/]+[^\\/\s"\']+','<PRIVATE_HOME>',v,flags=re.I)
 return re.sub(r'S-1-5-\d+(?:-\d+)*','<PRIVATE_SID>',v)
def display(v):
 if re.search(r'\b[A-Za-z]:[\\/]|[A-Za-z0-9._%+-]+@[A-Za-z0-9.-]+\.[A-Za-z]{2,}|https?://[^\s/"<>:@]+:[^\s/"<>@]+@|\\\\[A-Za-z0-9_.-]+\\',v):return '<REDACTED_TEST_DISPLAY:'+hashlib.sha256(v.encode()).hexdigest()[:24]+'>'
 return clean(v)
def commands():
 rows=[]
 for p in sorted((RUN/'commands').glob('*/command.json')):
  r=load(p);first=Path(r['command'][0]).name.lower();r['command'][0]={'python.exe':'<PYTHON>','msbuild.exe':'<MSBUILD>','vstest.console.exe':'<VSTEST>'}.get(first,r['command'][0]);r.update(receipt_id=p.parent.name,raw_receipt_sha256=sha(p));rows.append(clean(r))
 versions={name:text(RUN/'commands'/('final-version-'+name)/'stdout.log').strip() for name in ['dotnet','node','python','msbuild','framework']}
 save('command-outcomes.json',dict(schema='04A32-commands-v1',commands=rows,versions=versions,runner=pin(ROOT/'scripts/test_suite_rationalization.py'),batch_wrapper=pin(HERE/'run.py'),raw='Ignored isolated run/commands/<receipt_id> contains original logs,source/build/TRX receipts. No private raw logs published.',manual_entry_commands=['git status/HEAD/main/parent/ancestry/index and authorized switch -c codex/04A-32 main','powershell -NoProfile -ExecutionPolicy Bypass -File evidence/04A32/capture.ps1 -Label before','Read original before/Debug Core assembly ChartAxisScalingCore for axis-baseline.csv'],wrapper_commands=['powershell -NoProfile -ExecutionPolicy Bypass -File scripts/verify-beshstatng.ps1','powershell -NoProfile -ExecutionPolicy Bypass -File scripts/verify-beshstatng.ps1 -Legacy'],wrappers_status='NOT_RUN_UNCHANGED_WRAPPERS;accepted isolated complete equivalents executed',wrapper_reason='Original wrappers hard-code shared TestResults/Codex and bin/obj;Legacy Build invokes packaging hooks. The batch explicitly prohibits historical clobbering/packaging/signing. Compile-only host targets and all full portable/Framework lanes run through unchanged accepted isolated runner,without restore/install.',intermediate='Build/name/interop-fixture errors,new grid/bin fixture errors and source-guard helper defects retained. verified-Debug matrix was superseded after exact UTF8 comment restoration in BoxPlot/HistogramBinningCore;its source_unchanged=False is not presented as final authority. final builds/matrices correspond to corrected source bytes.'))
 return versions
def indexed(records):
 buckets=defaultdict(list)
 for row in records:
  raw=(row['class_name'],row['method'],row['test_name']);key=hashlib.sha256(json.dumps(raw).encode()).hexdigest();buckets[key].append(row)
 return {(k,i):r for k,values in buckets.items() for i,r in enumerate(sorted(values,key=lambda r:r['outcome']))}
def managed():
 suites=[];deltas=[];identity_rows=[];nonpasses=[];baseline={};final={}
 accepted=list(csv.DictReader((ROOT/'evidence/04A31/managed-test-identities.csv').open(encoding='utf-8-sig')))
 for suite in SUITES:
  p=RUN/'executions'/('before-Debug-'+suite)/'cases.json';old=load(p);b=indexed(old['records']);baseline[suite]=b
  reference={(r['raw_identity_sha256'],int(r['raw_occurrence_ordinal'])):r for r in accepted if r['suite']==suite}
  assert set(reference)==set(b) and all(reference[i]['debug']==b[i]['outcome'] for i in b),(suite,'fresh current-main baseline differs from accepted31')
  for config in ['Debug','Release']:
   p=RUN/'executions'/f'final-{config}-{suite}'/'cases.json';r=load(p);assert set(r['outcomes'])<={'Passed','Failed'};d=indexed(r['records']);final[(config,suite)]=d
   missing=set(b)-set(d);changed=[i for i in set(b)&set(d) if b[i]['outcome']!=d[i]['outcome']];added=set(d)-set(b)
   assert not missing and not changed,(suite,config,missing,changed)
   assert all('Graphics32' in d[i]['class_name'] and d[i]['outcome']=='Passed' for i in added)
   build=load(RUN/'builds/final'/config/'build.json');assert not any(build['exit_codes']) and build['source_unchanged']
   suites.append(dict(configuration=config,suite=suite,outcomes=r['outcomes'],counters=r['counters'],baseline_debug=old['outcomes'],trx_sha256=r['sha256'],raw_receipt_sha256=sha(p),build_source_pins_sha256=hashlib.sha256(json.dumps(build['source_pins'],sort_keys=True).encode()).hexdigest(),seconds=r['command']['seconds']))
   deltas.append(dict(configuration=config,suite=suite,removed=0,changed_baseline_outcomes=0,new_04A32_raw_cases=len(added),reference='Fresh actual current-main Debug baseline;baseline Release NOT_RUN,accepted31 historical Release separately retained'))
   for row in r['records']:
    if row['outcome']!='Passed':nonpasses.append(dict(configuration=config,suite=suite,class_name=row['class_name'],method=row['method'],test_name=display(row['test_name']),outcome=row['outcome'],message_sha256=hashlib.sha256(row['message'].encode()).hexdigest(),classification='INHERITED_EXACT_BASELINE_IDENTITY_AND_OUTCOME'))
  debug=final[('Debug',suite)];release=final[('Release',suite)]
  assert set(debug)==set(release) and all(debug[i]['outcome']==release[i]['outcome'] for i in debug)
  for i in sorted(debug):
   r=debug[i];identity_rows.append(dict(suite=suite,class_name=r['class_name'],method=r['method'],test_name=display(r['test_name']),raw_identity_sha256=i[0],raw_occurrence_ordinal=i[1],current_main_debug=b[i]['outcome'] if i in b else 'NEW_04A32',final_debug=r['outcome'],final_release=release[i]['outcome']))
 with (HERE/'managed-test-identities.csv').open('w',encoding='utf-8',newline='') as f:w=csv.DictWriter(f,fieldnames=list(identity_rows[0]));w.writeheader();w.writerows(identity_rows)
 focused=[]
 for c in ['Debug','Release']:
  for suite in ['Core','Legacy']:
   p=RUN/'executions'/f'final-{c}-graphics32-{suite}'/'cases.json';r=load(p);assert r['command']['exit_code']==0 and set(r['outcomes'])=={'Passed'}
   focused.append(dict(configuration=c,suite=suite,outcomes=r['outcomes'],trx_sha256=r['sha256'],identities=[dict(class_name=x['class_name'],method=x['method'],test_name=x['test_name'],outcome=x['outcome']) for x in r['records']]))
 return suites,deltas,focused,nonpasses
def parse_python(label,suffix):
 p=RUN/'commands'/(label+'-Debug-'+suffix)/'stderr.log';t=text(p);r=load(p.parent/'command.json')
 total=int(re.search(r'Ran (\d+) tests?',t).group(1));fail=int(re.search(r'failures=(\d+)',t).group(1)) if re.search(r'failures=(\d+)',t) else 0;error=int(re.search(r'errors=(\d+)',t).group(1)) if re.search(r'errors=(\d+)',t) else 0
 identities=re.findall(r'^(test_\S+ \([^)]*\))',t,re.M)
 nonpasses=[dict(outcome=k,identity=i,subtest=s.strip()) for k,i,s in re.findall(r'^(FAIL|ERROR): (test_\S+ \([^)]*\))([^\n]*)',t,re.M)]
 return dict(label=label,suite=suffix,total=total,passed=total-fail-error,failed=fail,errors=error,exit_code=r['exit_code'],stderr_sha256=sha(p),primary_identities=identities,nonpasses=nonpasses)
def source_node():
 python=[];nodes=[];new_failures=[]
 qualification=qualify_source_nonpasses()
 for suffix in ['python-tools','python-help','python-scripts','common-python']:
  old=parse_python('before',suffix);new=parse_python('final',suffix)
  assert old['total']==new['total'] and Counter(old['primary_identities'])==Counter(new['primary_identities'])
  def index(r):return Counter((x['outcome'],x['identity'],x['subtest']) for x in r['nonpasses'])
  added=index(new)-index(old);removed=index(old)-index(new)
  changes=[dict(outcome=k,identity=i,subtest=s,count=n,classification='AUTHORIZED_SOURCE_PIN_SUCCESSOR' if k=='FAIL' and (any(p in s for p in WINDOWS+CORE_OLD) or i in qualification) else 'UNCLASSIFIED') for (k,i,s),n in added.items()]
  new_failures.extend(x for x in changes if x['classification']=='UNCLASSIFIED')
  new.update(baseline=dict(total=old['total'],passed=old['passed'],failed=old['failed'],errors=old['errors'],stderr_sha256=old['stderr_sha256']),identity_population_preserved=True,added_nonpasses=changes,removed_nonpasses=[dict(outcome=k,identity=i,subtest=s,count=n) for (k,i,s),n in removed.items()],count_qualification='Unittest reported cases minus explicit FAIL/ERROR subtest counts;all raw failures retained,not made green by successor')
  python.append(new)
 for label in ['before','final']:
  for suffix in ['office-node','resolver','wasm-node']:
   p=RUN/'commands'/(label+'-Debug-'+suffix)/'stdout.log';t=text(p)
   def count(k):
    matches=re.findall(r'(?:# |\u2139 )'+k+r' (\d+)',t);assert matches,(suffix,k);return int(matches[-1])
   nodes.append(dict(label=label,suite=suffix,total=count('tests'),passed=count('pass'),failed=count('fail'),stdout_sha256=sha(p),exit_code=load(p.parent/'command.json')['exit_code']))
 save('source-and-node-validation.json',dict(schema='04A32-source-node-v1',python=python,node=nodes,unclassified_new_nonpasses=new_failures,node_cjs='wasm-node invokes node --test --test-concurrency=1 and includes all .cjs suites',configuration='Configuration-independent source/Node lanes run once per before/final Debug matrix;full Release managed separately executed'))
 return python,[x for x in nodes if x['label']=='final'],new_failures
def qualify_source_nonpasses():
    # Establish the precise cause of five parent-level assertions that do not
    # identify their path as a unittest subtest. Never patch or rerun them green.
    diagnostic=text(RUN/'commands/final-Debug-python-scripts/stderr.log').replace('\r\n','\n')
    blocks={block.splitlines()[0][6:]:block for block in re.split(r'\n={20,}\n',diagnostic) if block.startswith('FAIL:')}
    entry={r['path']:r for r in load(HERE/'entry-pins.json')['files']}
    path='BESHStat.Core/src/Graphics/HistogramBinningCore.vb';old=entry[path]['sha256'];current=sha(ROOT/path)
    definitions=[
     ('test_no_designer_udf_numerical_packaging_or_framework_change (test_04a05_contingency.ContingencySourceGates.test_no_designer_udf_numerical_packaging_or_framework_change)','scripts/tests/test_04a05_contingency.py',141,147,'DIFF_SCOPE_ASSERTION'),
     ('test_all_udf_exports_core_numerics_designers_and_frameworks_unchanged (test_04a13_survival_workflow.Survival13SourceContracts.test_all_udf_exports_core_numerics_designers_and_frameworks_unchanged)','scripts/tests/test_04a13_survival_workflow.py',126,132,'EXACT_SHA_ASSERTION'),
     ('test_core_udf_exports_designers_and_accepted_survival_are_unchanged (test_04a14_mixed_workflow.Mixed14SourceContracts.test_core_udf_exports_designers_and_accepted_survival_are_unchanged)','scripts/tests/test_04a14_mixed_workflow.py',142,148,'EXACT_SHA_ASSERTION'),
     ('test_all_frozen_sources_authorities_and_accepted_renderer_hashes_are_unchanged (test_04a23_multivariate_spc.Spc23Contracts.test_all_frozen_sources_authorities_and_accepted_renderer_hashes_are_unchanged)','scripts/tests/test_04a23_multivariate_spc.py',142,147,'EXACT_SHA_ASSERTION'),
     ('test_core_udf_designer_bytes_remain_frozen (test_04a26_planning_agreement.PlanningAgreement26Contracts.test_core_udf_designer_bytes_remain_frozen)','scripts/tests/test_04a26_planning_agreement.py',31,32,'EXACT_SHA_ASSERTION')]
    changes=git('diff','--name-only').splitlines();banned=[p for p in changes if p.endswith('Designer.vb') or 'ExcelUDFs/' in p or p.startswith('BESHStat.Core/') or p.endswith('.dna')]
    assert set(banned)==set(CORE_OLD)
    rows=[]
    for identity,source,start,end,kind in definitions:
        assert identity in blocks,identity;block=blocks[identity]
        if kind=='EXACT_SHA_ASSERTION':assert old in block and current in block,(identity,'Not the qualified source SHA pair')
        else:assert 'AssertionError: True is not false' in block
        rows.append(dict(identity=identity,raw_outcome='FAIL_RETAINED',cause=kind,test_source=pin(ROOT/source),member_lines=f'{start}-{end}',entry_sha256=old if kind=='EXACT_SHA_ASSERTION' else None,final_sha256=current if kind=='EXACT_SHA_ASSERTION' else None,qualified_source=path if kind=='EXACT_SHA_ASSERTION' else banned,diagnostic_normalized_block_sha256=hashlib.sha256(block.encode()).hexdigest(),proof='Exact failing hash pair equals authorized entry/final HistogramBinningCore pins;Roslyn proves original algorithm body unchanged' if kind=='EXACT_SHA_ASSERTION' else 'Old assertion bans any tracked Core edit;the only matching paths are the three authorized Core dependencies. No Designer/UDF/DNA or target changed.'))
    save('source-nonpass-qualification.json',dict(schema='04A32-source-nonpass-v1',status='QUALIFIED_RETAINED_FAIL',parent_assertions=rows,additional_04A28_subtests='HistogramBinningCore,ViolinPlotCore,Windows ViolinPlot whole-file pins;actual source transitions protected by source-transition-successor.json',new_failed_assertions_total=8,old_assertions_and_expected_values='UNCHANGED;no mocks,waivers,subtractions or converted passes',native='NATIVE_NOT_RUN',scientific='NOT_GOLDEN'))
    return {x['identity'] for x in rows}
def casebook():
 records=[]
 for c in ['Debug','Release']:
  for suite in ['Core','Legacy']:
   for p in sorted((RUN/'compatibility-cases'/f'final-{c}-graphics32-{suite}').glob('*.csv')):
    rows=list(csv.DictReader(p.open(encoding='utf-8-sig')));assert len(rows)==1;r=rows[0];assert r['expected_exception']==r['actual_exception']
    if r['expected_exception']=='None':
     a=r['expected_ieee'].split('|');b=r['actual_ieee'].split('|');assert len(a)==len(b)
     def nan(x):
      v=int(x,16);return (v&0x7ff0000000000000)==0x7ff0000000000000 and (v&0xfffffffffffff)!=0
     assert all(x==y or nan(x) and nan(y) for x,y in zip(a,b)),r['id']
    records.append(dict(configuration=c,suite=suite,**r,raw_csv_sha256=sha(p)))
 assert records
 expected=(HERE/'adapter-capture-entry.csv').read_bytes();captures=[]
 for c in ['Debug','Release']:
  p=RUN/f'final-{c}-adapter-capture.csv';actual=p.read_bytes();assert actual==expected
  captures.append(dict(configuration=c,rows=len(list(csv.DictReader(p.open(encoding='utf-8-sig')))),bytes=len(actual),sha256=sha(p),equal_expected_bytes=True,expected=pin(HERE/'adapter-capture-entry.csv'),assembly_sha256=sha(RUN/'builds/final'/c/'legacy/BESHstatNG.dll'),status='HOST_FREE_EXECUTED;no native chart/worksheet created'))
 unique={r['id'] for r in records if r['configuration']=='Debug'};assert unique=={r['id'] for r in records if r['configuration']=='Release'}
 appearance_source=RUN/'before-Debug-appearance-probe.csv';appearance_bytes=appearance_source.read_bytes();target=HERE/'appearance-capture-entry.csv'
 if target.exists():assert target.read_bytes()==appearance_bytes
 else:target.write_bytes(appearance_bytes)
 appearance=[]
 for c in ['Debug','Release']:
  path=RUN/f'final-{c}-appearance-probe.csv';assert path.read_bytes()==appearance_bytes
  appearance.append(dict(configuration=c,rows=len(list(csv.DictReader(path.open(encoding='utf-8-sig')))),bytes=path.stat().st_size,sha256=sha(path),equal_expected_bytes=True,expected=pin(target),core_assembly_sha256=sha(RUN/'builds/final'/c/'legacy/BESHStat.Core.dll'),host_assembly_sha256=sha(RUN/'builds/final'/c/'legacy/BESHstatNG.dll')))
 assert all(r['rows']==90 for r in appearance)
 save('appearance-validation.json',dict(schema='04A32-appearance-probe-v1',status='HOST_FREE_EXECUTED_PASS',comparisons=appearance,fixture=[pin(HERE/'helpers/AppearanceProbe.cs'),pin(HERE/'appearance-probe.ps1')],before_host_sha256=sha(RUN/'builds/before/Debug/legacy/BESHstatNG.dll'),before_core_sha256=sha(RUN/'builds/before/Debug/legacy/BESHStat.Core.dll'),authority='Probes executed after implementation against preserved real before/Debug and final Framework assemblies. Old validator/axis outputs,exception types and parameter names are the compatibility reference;not generated by the new implementation.',method='Managed reflection only:Box ValidateAppearance;Violin ValidateAppearance then ResolveAxisContext. No chart/worksheet/COM instance. Integer-only options use representable boundary inputs;other numeric options include NaN/Infinity/negative/zero/ordinary values.',numeric_option_precondition='The neutral violin domain mirrors the former private resolver;finite/positive appearance validation remains the retained Windows validator. Direct portable callers must validate their axis options before projecting. No new rejection/coercion rule was silently added.',scientific='NOT_GOLDEN',native='NATIVE_NOT_RUN'))
 save('compatibility-casebook.json',dict(schema='04A32-casebook-v1',authority='COMPATIBILITY_REFERENCE: actual pre-edit main Framework adapter captures and frozen source;HOST_FREE_EXECUTED final Core/Framework tests;NOT_GOLDEN',sources=[pin(HERE/'pre-edit-compatibility-authority.json'),pin(HERE/'current-main-baseline.json'),pin(HERE/'axis-baseline.csv')]+[pin(ROOT/p) for p in TESTS],rules='New comparisons use exact IEEE finite/infinite/signed-zero bits;algorithmic NaNs compare by classification. Copy/provenance assertions use exact bits where meaningful. Existing regression tolerances remain unchanged;no new floating tolerance used.',records=records,record_count=len(records),unique_case_ids=len(unique),adapter_capture_comparisons=captures,appearance_probe=pin(HERE/'appearance-validation.json'),capture_limit='Full original expected values,shapes and exceptions are in adapter-capture-entry.csv;each final raw CSV is byte-identical in all3302 rows and authenticated by the stated hashes,so duplicate copies are not published. Numeric capture integers are encoded through Double IEEE64;Boolean flags use True/False. The additional90-row appearance/axis probe is separately identified.',quartile_provenance='Live DS/Histogram/Violin all ultimately use StatFunc.QuartilesComp;CDF result2/3.5/5 for1..6 differs from hand-specified alternative type7 2.25/4.75,which is not adopted. No nonexistent between-family quartile difference is fabricated.',identity='Box raw/sorted/per-group ordinals and Violin accepted original global ordinals asserted separately;no physical row invented.',native='NATIVE_NOT_RUN',R='NOT_RUN_UNAUTHORIZED'))
 return len(records),len(unique),captures
def build_validation(suites,deltas,focused,python,node,unknown,case_count,unique,captures,versions):
 serial={name:load(RUN/'commands'/('final-'+name)/'command.json')['exit_code'] for name in ['strict','profile','docs']}
 summaries=[]
 for name in ['before-Debug-matrix.json','final-Debug-matrix.json','final-Release-managed.json']:
  p=RUN/name;r=load(p);assert r['source_unchanged'];summaries.append(dict(name=name,sha256=sha(p),source_unchanged=True))
 sums={c:Counter() for c in ['Debug','Release']}
 for r in suites:sums[r['configuration']].update(r['outcomes'])
 py={k:sum(r[k] for r in python) for k in ['total','passed','failed','errors']};nd={k:sum(r[k] for r in node) for k in ['total','passed','failed']}
 blockers=['Unclassified Python nonpass:'+x['identity'] for x in unknown]
 audit_path=RUN/'commands/final-audit-tests/command.json'
 if audit_path.exists():
  audit=load(audit_path);audit_log=text(audit_path.parent/'stderr.log');assert audit['exit_code']==0 and 'Ran 16 tests' in audit_log and 'skipped=' not in audit_log
  evidence_tests=dict(status='PASS',total=16,passed=16,failed=0,errors=0,skipped=0,stderr_sha256=sha(audit_path.parent/'stderr.log'),exit_code=0,qualification='Post-seal tests execute the independent verifier and adversarial/source/gate assertions. Final metadata receipts refreshed/resealed afterward;final independent byte verifier runs again.')
 else:
  evidence_tests=dict(status='PENDING_POSTSEAL',preseal_total=16,preseal_passed=15,preseal_skipped=1)
  blockers.append('Post-seal independent manifest/evidence audits pending')
 if serial['docs']!=0:blockers.append('Strict MkDocs failed')
 if nd['failed']:blockers.append('Node failure')
 decision='NOT_READY_04A32' if blockers else 'READY_FOR_OWNER_04A32_ACCEPTANCE_REVIEW'
 v=dict(schema='04A32-validation-v1',decision=decision,blockers=blockers,managed=suites,identity_deltas=deltas,focused=focused,managed_totals={k:dict(v) for k,v in sums.items()},python=py,node=nd,strict_profile_mkdocs=serial,source_execution_summaries=summaries,builds=[dict(configuration=c,exit_codes=load(RUN/'builds/final'/c/'build.json')['exit_codes'],source_unchanged=load(RUN/'builds/final'/c/'build.json')['source_unchanged'],receipt_sha256=sha(RUN/'builds/final'/c/'build.json')) for c in ['Debug','Release']],casebook_records=case_count,unique_case_ids=unique,adapter_capture_comparisons=captures,versions=versions,native='NATIVE_NOT_RUN',R='NOT_RUN_UNAUTHORIZED',scientific='NOT_GOLDEN',not_run=['Entry Release full managed (entry Debug freshly executed;final full Release executed)','Unchanged shared-output/packaging verifier wrappers;complete isolated equivalents run','Live Excel/COM/UIA/RefEdit/native chart smoke;independent R;Office/Mac/browser/WASM manual smoke;packaging/signing/install/deploy'],common='No Common change;focused Common NOT_APPLICABLE;full Common Debug/Release executed')
 v['new_evidence_tests']=evidence_tests
 v['additional_appearance_checks']=load(HERE/'appearance-validation.json')
 save('validation.json',v);return v
def report(v,nonpasses):
 decision=v['decision'];p=v['python'];n=v['node'];serial=v['strict_profile_mkdocs']
 totals=v['managed_totals'];files='\n'.join('- `'+x+'`' for x in WINDOWS+CORE_OLD+CORE_NEW+TESTS+[PROJECT])
 table=[]
 for suite in SUITES:
  before=next(r['baseline_debug'] for r in v['managed'] if r['configuration']=='Debug' and r['suite']==suite)
  d=next(r['outcomes'] for r in v['managed'] if r['configuration']=='Debug' and r['suite']==suite);r=next(r['outcomes'] for r in v['managed'] if r['configuration']=='Release' and r['suite']==suite)
  table.append(f"|{suite}|{before.get('Passed',0)}/{before.get('Failed',0)}|{d.get('Passed',0)}/{d.get('Failed',0)}|{r.get('Passed',0)}/{r.get('Failed',0)}|")
 pychanges=load(HERE/'source-and-node-validation.json')['python'];changes=sum(sum(x['count'] for x in row['added_nonpasses']) for row in pychanges)
 dossier=f'''# Official batch 04A.32 owner review

**{decision}** — software coordinate/compatibility review only. {'Blockers: '+ '; '.join(v['blockers']) if v['blockers'] else 'No batch implementation blocker remains. The full matrix has real inherited and authorized-source-pin failures;it is not a green scientific/native acceptance.'}

## Entry and authority

Branch `codex/04A-32`,actual merged-main base and unchanged HEAD `{BASE}`;parent `f840b1695fd95be25faba6fd5408c9498a31d804`. The owner accepted/merged04A.31;actual Git and compiled source confirm it. Entry working tree/index were clean;no unrelated owner output was overwritten. Branch metadata creation required sandbox approval;no other publication action occurred. Two MSBuild processes were visible and no dotnet/testhost/python/Excel was observed at entry;global writer exclusivity is not claimed. All output writers use isolated run/build/execution directories. [Entry state](../../evidence/04A32/entry-state.json) and [entry source pins](../../evidence/04A32/entry-pins.json).

The plan's old accepted-through04A.28 handoff remains unchanged;its official04A.32 row agrees with this scope. The current code corrects one prompt assumption:standalone `DescriptiveStat.compute`,Histogram and Violin all ultimately use the same `StatFunc.QuartilesComp`. Separate wrappers,finite policies and whisker methods are retained. We do not invent a fixture where those currently identical finite quartile routes disagree. The1..6 fixture proves CDF quartiles2/5 and rejects silently substituting alternative type7 values2.25/4.75.

All37 externally hashed04A.31 publication payloads match the merged Git blobs directly or through digest-exact EOL/CSV serializer reconstruction (17 direct,17 uniform CRLF,2 CRLF except literal final LF,1 CSV record-CRLF with embedded LF). This is qualified inverse reconstruction,not a claim to re-inspect an original31 archive. Accepted04A.29 original-byte resolution and04A.30/31 source/evidence lineage are frozen. No historical receipt was rewritten or pristine archive requested.

## Files and operation ownership

{files}

Production comprises three Windows adapters,three new Core files,and three bounded changes to existing Core sources. `ChartingFunc.vb` already forwards the shared axis/binning engines and is byte-unchanged. Common needs no change;existing label/source contracts are reused where supplied. The small existing-Core dependencies are explicit:HistogramBinning exposes the actual edges from the same calculation;HistogramData adds a copy-safe entry;ViolinPlotCore carries accepted original ordinals beside existing accepted values. No KDE,bandwidth,scale,quartile,distribution or binning engine is forked.

[Entry operation map](../../evidence/04A32/operation-map-entry.csv) links historical04A.29 member/operation IDs and04A.30 handoff ownership. Its old audit ranges are explicitly historical. [Pre-edit frozen authority](../../evidence/04A32/pre-edit-compatibility-authority.json) pins actual numeric slices/ranges before product changes;[final map](../../evidence/04A32/operation-map-final.csv) reconciles actual merged-base and final ranges/hashes. Real callers in `UI/UibyID`,`Ui0OneRefeditMulticol`,Windows ANOVA/assumption reports,and the existing Violin facade/UI were inspected and remain unchanged. No Ribbon/UDF/help/OfficeAddin/WasmSmoke/installer/target/signing configuration was edited.

## Neutral contracts and actual Windows consumption

`BoxPlotDataCore` supplies immutable descriptive aggregate/raw/sorted/outlier/flag/permutation records,grouped series,group coordinates,whisker lengths and existing Peltier components. It calls the unchanged `DescriptiveStat.compute(False)` and `BoxSegmentsCore`. `BoxPlot.Calculate` consumes aggregate/flag data while retaining the mechanical count/matrix commits and collection append order. `CalcForPlotting` obtains Core whisker lengths in legacy commit order. `AddBoxPlot` obtains the Core axis result;raw-point X uses the shared deterministic Core jitter. Supplied label aliases,table body/headers,native series/styles and public mutable Q/outlier-count arrays remain compatible. Source arrays stay unchanged. The private accumulated collection now holds immutable aggregate records;it is still Windows-owned and deliberately not reset.

`HistogramPlotDataCore` supplies bin midpoints/counts,actual classification edges,normal XY values,Tukey summaries/outliers and explicit numeric axes/band. The original public binning signature remains;its exact arithmetic/control flow exists once in private `ComputeBins`,with a copied break-vector capture at successful return. The old direct mutation behavior of the original binning API remains;new portable boundaries clone inputs. Display bounds retain their separate midpoint/one-bin conventions. Overlay ordinates are used exactly as already frequency-scaled. Windows `compute` consumes the edge-enabled shared bins and Core Tukey summary,keeping stage/reset/error order. `PlotData` snapshots current state;`addChart` consumes its axis values. Primary/secondary Y limits and major units remain identical;the box band is -.32*Ymax,centre-.18*Ymax,halfheight.055*Ymax. All native chart/overlay/event/pixel operations remain Windows-owned.

`ViolinPlotDomainData` exposes data-space X/Y bounds,optional explicit major,category positions/label anchors,series outlines and raw observation coordinates. It reuses the accepted result;no KDE/group grammar is repeated. A bounded additive metadata path captures flat input ordinal i beside each accepted observation;the old Friend constructor and all old public members remain. The copied ordinal vector follows existing filtering and per-group raw order,including nonzero-based category arrays. `ViolinPlotExcel.ResolveAxisContext`,label anchors,freeform data coordinates,inner spans and observation geometry consume Core projections/helpers. Native pixel transforms,clipping,minimum pixel sizes,Office enum calls,styling and cleanup remain in Windows. Automatic major unit remains Excel-owned;explicit major forwarding and validation stay unchanged and are inspectable in the neutral domain. The255-series limit remains host-only.

Input ordinal,sorted ordinal,group ordinal and chart/shape ordinal are separate. Core ordinals are zero-based;category X is1..g;native series/point conventions remain host mappings. Physical worksheet rows are unavailable unless an established caller actually supplies them;this batch never invents them. Every stored vector/matrix getter clones,including nested group/series arrays and source ordinals;immutable child records can be shared safely. [Full contract schema and source examples](../../evidence/04A32/coordinate-and-identity-contracts.json).

## Compatibility and execution

The fresh current-main baseline was built and fully executed in Debug before edits. Actual Framework Box/Histogram computations captured3,302 fields/shapes/exception-state rows without creating worksheet/chart objects. Both final Debug and Release adapter captures match every captured byte and row. A frozen source fixture reuses accepted `LegacyBox30.CalcForPlotting`;only missing numeric slices were added. Exact baseline axis IEEE tuples come from the actual pre-edit assembly,including the legacy zero-axis maximum slightly above literal1.2. [Current-main baseline](../../evidence/04A32/current-main-baseline.json),[capture values](../../evidence/04A32/adapter-capture-entry.csv),[casebook](../../evidence/04A32/compatibility-casebook.json).

|Managed suite|Fresh main Debug PASS/FAIL|Final Debug PASS/FAIL|Final Release PASS/FAIL|
|---|---:|---:|---:|
{chr(10).join(table)}

Final Debug:**{totals['Debug'].get('Passed',0)} PASS / {totals['Debug'].get('Failed',0)} FAIL / 0 ERROR / 0 SKIP**. Final Release:**{totals['Release'].get('Passed',0)} PASS / {totals['Release'].get('Failed',0)} FAIL / 0 ERROR / 0 SKIP**. Every inherited identity,outcome and raw multiplicity matches fresh current-main Debug and accepted31;no old test removed or weakened. New raw cases per configuration:{sum(x['new_04A32_raw_cases'] for x in v['identity_deltas'] if x['configuration']=='Debug')},all PASS. Entry Release was NOT_RUN;final full Release was genuinely executed. [Raw identity/multiplicity CSV](../../evidence/04A32/managed-test-identities.csv).

Focused final **Core131/131 and Framework81/81 in each configuration**,including existing distribution/axis/04A.30 regressions. Common focused is NOT_APPLICABLE because Common is unchanged;full Common ran both configurations. Cases cover jagged/matrix constructors,duplicates/empty labels,sort/reverse/shuffle,ties/fences/outlier order/whiskers,signed components,jitter,all array copies,seedless stateless versus mutable repeated-state behavior;all histogram rules/toggles/boundaries/edge-domain/overlay/Tukey/finite-axis cases;all3 bandwidth×3 scale×2 trim violin combinations,category lower bounds,excluded rows,source ordinals,automatic/explicit axes,geometry and copy safety. Existing Violin fixtures continue to cover validation/bandwidth/scale details. An additional [90-case appearance/axis probe](../../evidence/04A32/appearance-validation.json) matches actual before/final Framework validators in both configurations,including nonfinite/invalid option exceptions and parameter names;these are separate from managed TRX counts and create no Excel objects. The pure neutral domain mirrors the former private resolver and assumes upstream validated appearance options;retained Windows validation remains authoritative for rejection. New comparisons use exact IEEE finite/infinite/signed-zero bits and explicit NaN classification;copy/provenance checks use exact bits. No new floating tolerance is used;old test tolerances are preserved. Expectations come from source/baseline captures or hand-specified metadata,not the new implementation presented as an independent scientific oracle.

Node:**{n['passed']} PASS / {n['failed']} FAIL**,all `.cjs` lanes included. Python full:**{p['passed']} PASS / {p['failed']} FAIL / {p['errors']} ERROR** of{p['total']} reported cases. Compared with the freshly executed baseline,{changes} added failing scope/hash assertions reflect authorized exact source transitions:one old ban on any tracked Core change,four parent SHA assertions on HistogramBinningCore,and three04A.28 whole-source subtests. All remain FAIL. [Exact nonpass qualification](../../evidence/04A32/source-nonpass-qualification.json) binds their source lines and old/new SHA pairs to this authorized change;no mock,expected-value edit or subtraction makes them green. All other Python identities/nonpasses are retained;any unclassified change is an acceptance blocker. Strict/profile real exits:**{serial['strict']}/{serial['profile']}**;strict MkDocs exit:**{serial['docs']}**. [Detailed validation](../../evidence/04A32/validation.json),[source/Node deltas](../../evidence/04A32/source-and-node-validation.json),[nonpasses](../../evidence/04A32/nonpasses.json). New adversarial evidence/manifest audits:**{v['new_evidence_tests'].get('passed',0)} PASS** with status `{v['new_evidence_tests']['status']}`;reported separately from the full historical Python matrix.

Core/Common retain netstandard2.0;Windows Framework4.8 compiles both configurations. Isolated builds omit packaging/post-build/signing/help/version generation. The unchanged repository verifier wrappers themselves are NOT_RUN because of their shared historical output paths and Legacy packaging hooks;all complete portable/Framework equivalents ran via the accepted isolated runner,with exact commands/exits/versions and source pins in [command outcomes](../../evidence/04A32/command-outcomes.json). No restore,package/network install or native/R dependency was introduced. Failed intermediate name/interop/fixture/guard receipts are retained locally. Byte-preservation caught an unintended UTF8 comment encoding change in two edited files;the exact source was restored and final builds/full matrices rerun against corrected bytes. The earlier verified-label matrix is explicitly superseded and not final authority.

## Preservation,gates and owner handoff

[Roslyn ownership guard](../../evidence/04A32/source-ownership.json) preserves all old method/property/constructor declarations,proves binning arithmetic/control-flow equality and Violin numeric/group-parser equality after removing only ordinal metadata,and records actual member ranges/token hashes. Complete Windows files equal the bounded numeric substitutions in the versioned transform specification;native styles,series order,labels,table headers,shape/event/pixel code outside those substitutions remain unchanged. This is source evidence,not an executed native chart claim.

[Preservation](../../evidence/04A32/source-preservation.json) checks all3,968 entry tracked files:only six authorized production files and the additive test project changed. All other35 Windows Graphics modules,17 other existing Core Graphics files,accepted ROC/Q–Q,all historical tests/evidence,AGENTS/master plan/original ledgers/canonical lineage and all100 mixed-model captures remain byte-preserved. [278-pin source successor](../../evidence/04A32/source-transition-successor.json) reconciles the actual prior schema and retains every original pin/transition;it adds only the five applicable authorized transitions and separately records the accepted30 HistogramData helper change outside that original pin population. [Historical receipt](../../evidence/04A32/historical-results-preservation.json) compares membership,size and mtime only;138GB payload bytes were NOT_REHASHED. Nothing historical was pruned,archived or rewritten.

[Deferred issues](../../evidence/04A32/deferred-issues.json) retain accumulated Box state,short/nonfinite/odd-whisker policy,histogram one-bin display/overlay conventions and inactive/error state,and a source-only Violin category-series indexing candidate. Independent numeric/state decisions belong04A.39–41 and qualified native confirmation04A.45–48. PrettyBreaks setting remains04A.75;detrending remains04A.68–69. No04A.33 or later work started.

P=NO;N1=NO GO;N2=NO;Gate C2=UNMET;Gate C3=UNAPPROVED. All original controls/deferred obligations remain OPEN,zero releases/discharges. All comparisons remain **NOT_GOLDEN**. Live Excel/COM/UIA/RefEdit/native chart creation,appearance,callbacks and smoke are **NATIVE_NOT_RUN**. Independent R/scientific oracle and Office/Mac/browser/WASM manual smoke are **NOT_RUN**. Software portability is not native/scientific accuracy or Office/Mac availability. [Synthetic box/histogram/violin smoke proposal](../../evidence/04A32/native-smoke-proposal.md) requires separate authorization and qualified source/build/session attribution.

The exact proposed publication is [publication-manifest.json](../../evidence/04A32/publication-manifest.json),with relative paths,byte sizes,SHA256 and a null self-entry. [Independent verifier and adversarial tests](../../evidence/04A32/verify_manifest.py) reject tampered sizes/hashes,missing/duplicate/escaping paths and recursive self-hashes. Privacy scans keep raw logs/private identities outside publication. Final Git diff/index/branch/merge-base and manifest checks must pass before handoff. Nothing is staged,committed,merged,pushed,installed,signed,deployed or published.

Owner acceptance checklist:review the per-family contracts and actual adapter transitions;review all inherited/source-pin nonpasses and retained defects;verify the exact publication manifest and privacy/preservation receipts;accept only this04A.32 software scope;authorize Git publication and any later/native work separately.
'''
 (ROOT/DOSSIER).write_text(dossier,encoding='utf-8',newline='\n')
def collect():
 protection();versions=commands();s,d,f,np=managed();p,n,unknown=source_node();case_count,unique,captures=casebook();v=build_validation(s,d,f,p,n,unknown,case_count,unique,captures,versions)
 save('nonpasses.json',dict(schema='04A32-nonpasses-v1',decision=v['decision'],blockers=v['blockers'],managed=np,python=p,strict_profile_mkdocs=v['strict_profile_mkdocs'],historical_reference='Accepted31:19999PASS/14FAIL each managed;916Node;840PASS/106FAIL/10ERROR Python;12 separate evidence audits;strict/profile1/1. Current-main baseline freshly executed,not assumed.',intermediate='All trial/verified raw receipts retained;final label is authoritative. No old assertion/expected value edited.',scientific='NOT_GOLDEN',native='NATIVE_NOT_RUN'))
 report(v,np);print(v['decision'],v['managed_totals'],'Python',v['python'],'Node',v['node'],'blockers',v['blockers'])
def publication_paths():return sorted(set(WINDOWS+CORE_OLD+CORE_NEW+TESTS+[PROJECT,DOSSIER]+[p.relative_to(ROOT).as_posix() for p in HERE.rglob('*') if p.is_file() and '__pycache__' not in p.parts]))
def privacy():
 findings=[]
 for p in publication_paths():
  t=(ROOT/p).read_text(encoding='utf-8-sig')
  if any(re.search(pattern,t) for pattern in [r'[A-Za-z]:[\\/]+Users[\\/]+',r'S-1-5-\d+(?:-\d+)+',r'(?i)(?:DESKTOP|LAPTOP)-[A-Z0-9]+']):findings.append(p)
  if os.environ.get('COMPUTERNAME','!unset!').lower() in t.lower():findings.append(p)
 assert not findings,findings
 save('privacy.json',dict(schema='04A32-privacy-v1',status='PASS',files_scanned=len(publication_paths()),findings=[],rules='Synthetic inputs only;no actual private home paths,SIDs,hostnames,personal workbooks,credentials or raw logs published. Security-test displays are SHA-redacted while exact identity hashes/multiplicities remain.',relative_publication_paths=True))
def seal():
 assert not git('diff','--cached','--name-only') and not git('diff','--check')
 assert git('branch','--show-current')=='codex/04A-32' and git('rev-parse','HEAD','main').splitlines()==[BASE,BASE] and git('merge-base','HEAD','main')==BASE
 privacy();paths=sorted(set(publication_paths()+['evidence/04A32/publication-manifest.json']))
 save('publication-manifest.json',dict(schema='04A32-publication-v1',self_rule='Self included with null bytes/SHA256;no recursive hash',files=[dict(path=p,bytes=None,sha256=None) if p=='evidence/04A32/publication-manifest.json' else pin(ROOT/p) for p in paths]));verify()
def verify():
 from verify_manifest import verify_manifest
 verify_manifest(HERE/'publication-manifest.json',set(publication_paths()))
 assert set(git('diff','--name-only').splitlines())==set(WINDOWS+CORE_OLD+[PROJECT])
 assert not git('diff','--cached','--name-only') and not git('diff','--check')
 assert set(git('ls-files','--others','--exclude-standard').splitlines())==set(publication_paths())-set(git('ls-files').splitlines())
 print('PASS exact publication bytes/self,privacy,scoped diff,branch/base,index EMPTY;',len(publication_paths()),'publication files')
if __name__=='__main__':{'collect':collect,'seal':seal,'verify':verify}[sys.argv[1]]()
