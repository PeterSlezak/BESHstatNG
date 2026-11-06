"""Collect real 04A31 receipts, preserve nonpasses, seal exact publication bytes."""
from audit import ROOT,HERE,RUN,BASE,WINDOWS,NEW_CORE,TESTS,PROJECT,git,sha,pin,save,protection,public_path
from pathlib import Path
from collections import Counter,defaultdict
import csv,hashlib,json,os,re,sys
DOSSIER='docs/architecture/batch-04A-31-owner-review.md'
SUITES=['Core','Common','Legacy','OfficeAddin','OfficeExecution','ControlMitigation']
def read(p):return json.loads(Path(p).read_bytes())
def clean(v):
    if isinstance(v,list):return [clean(x) for x in v]
    if isinstance(v,dict):return {k:clean(x) for k,x in v.items()}
    if not isinstance(v,str):return v
    for a,b in [(str(RUN),'<ISOLATED_RUN>'),(RUN.as_posix(),'<ISOLATED_RUN>'),(str(ROOT),'<REPO_ROOT>'),(ROOT.as_posix(),'<REPO_ROOT>'),(sys.executable,'<PYTHON>')]:v=v.replace(a,b)
    v=re.sub(re.escape(os.environ.get('COMPUTERNAME','!unset!')),'<HOST>',v,flags=re.I)
    v=re.sub(r'[A-Za-z]:[\\/]+Users[\\/]+[^\\/\s"\']+','<PRIVATE_HOME>',v,flags=re.I)
    v=re.sub(r'S-1-5-\d+(?:-\d+)*','<PRIVATE_SID>',v)
    return v
def text(p):
    b=Path(p).read_bytes()
    try:return b.decode('utf-8')
    except UnicodeDecodeError:return b.decode('cp1252')
def safe_display(v):
    if re.search(r'\b[A-Za-z]:[\\/]|[A-Za-z0-9._%+-]+@[A-Za-z0-9.-]+\.[A-Za-z]{2,}|https?://[^\s/"<>:@]+:[^\s/"<>@]+@|\\\\[A-Za-z0-9_.-]+\\',v):return '<REDACTED_TEST_DISPLAY:'+hashlib.sha256(v.encode()).hexdigest()[:24]+'>'
    return clean(v)
def command_receipts():
    rows=[]
    for p in sorted((RUN/'commands').glob('*/command.json')):
        r=read(p);args=r['command'];first=Path(args[0]).name.lower()
        args[0]={'python.exe':'<PYTHON>','msbuild.exe':'<MSBUILD>','vstest.console.exe':'<VSTEST>'}.get(first,args[0])
        r.update(receipt_id=p.parent.name,raw_receipt_sha256=sha(p));rows.append(clean(r))
    save('command-outcomes.json',dict(schema='04A31-commands-v1',commands=rows,raw='Ignored isolated run/commands/<receipt_id>/;raw stdout/stderr/command receipts retained locally',manual_entry_commands='git status,main/HEAD/parent/ancestry/index,process classification,manifest byte qualification and authorized git switch documented in entry-state.json;no publication commands',initial_manual_guard='First direct helper build succeeded;first token check failed due EOF comparison in new helper. Corrected helper;final recorded guard succeeds. One subsequent helper compilation failed on a local C# variable name and is retained in command receipts.',verifier_wrappers=dict(commands=['powershell -NoProfile -ExecutionPolicy Bypass -File scripts/verify-beshstatng.ps1','powershell -NoProfile -ExecutionPolicy Bypass -File scripts/verify-beshstatng.ps1 -Legacy'],status='NOT_RUN_UNCHANGED_WRAPPERS',reason='Hard-code shared TestResults/Codex and bin/obj;Legacy Build includes production packaging hooks. More specific batch requires isolated outputs and compile-only/no packaging. Unchanged accepted test_suite_rationalization.py performs all equivalent portable/Framework build/test lanes with explicit isolated paths,offline MSBuild and compile-only Windows targets.',acceptance='No missing equivalent managed suite;full Debug/Release executed. Wrapper commands themselves are NOT_RUN,not claimed passes.'),runner=pin(ROOT/'scripts/test_suite_rationalization.py'),batch_wrapper=pin(HERE/'run.py')))
    return rows
def managed():
    prior=list(csv.DictReader((ROOT/'evidence/04A30/managed-test-identities-v1.csv').open(encoding='utf-8-sig')))
    indexed={}
    for config in ['Debug','Release']:
        for suite in SUITES:
            r=read(RUN/'executions'/f'verified-{config}-{suite}'/'cases.json');buckets=defaultdict(list)
            for row in r['records']:
                raw=(row['class_name'],row['method'],row['test_name']);key=hashlib.sha256(json.dumps(raw).encode()).hexdigest()
                buckets[key].append(row)
            indexed[(config,suite)]={(key,ordinal):v for key,values in buckets.items() for ordinal,v in enumerate(sorted(values,key=lambda r:r['outcome']))}
    identity_rows=[];suites=[];deltas=[];nonpasses=[]
    for suite in SUITES:
        old={(r['raw_identity_sha256'],int(r['raw_occurrence_ordinal'])):r for r in prior if r['suite']==suite}
        debug=indexed[('Debug',suite)];release=indexed[('Release',suite)]
        missing=set(old)-set(debug);added=set(debug)-set(old)
        changed=[i for i in set(old)&set(debug) if old[i]['after_debug']!=debug[i]['outcome']]
        drift=[i for i in set(debug)&set(release) if debug[i]['outcome']!=release[i]['outcome']]
        assert not missing and not changed and set(debug)==set(release) and not drift,(suite,missing,changed,drift)
        assert all('Graphics31' in debug[i]['class_name'] and debug[i]['outcome']=='Passed' for i in added)
        deltas.append(dict(suite=suite,removed=0,changed_prior_outcomes=0,new_04A31_raw_cases=len(added),debug_release_drift=0,reference='Accepted04A30 managed-test-identities-v1.csv;historical reference,not a newly executed entry run'))
        for i in sorted(debug):
            row=debug[i]
            identity_rows.append(dict(suite=suite,class_name=row['class_name'],method=row['method'],test_name=safe_display(row['test_name']),raw_identity_sha256=i[0],raw_occurrence_ordinal=i[1],accepted_04A30=old[i]['after_debug'] if i in old else 'NEW_04A31',debug=row['outcome'],release=release[i]['outcome']))
        for config in ['Debug','Release']:
            r=read(RUN/'executions'/f'verified-{config}-{suite}'/'cases.json');build=read(RUN/'builds/verified'/config/'build.json')
            assert not any(build['exit_codes']) and build['source_unchanged']
            suites.append(dict(configuration=config,suite=suite,outcomes=r['outcomes'],counters=r['counters'],trx_sha256=r['sha256'],raw_receipt_sha256=sha(RUN/'executions'/f'verified-{config}-{suite}'/'cases.json'),build_source_pins_sha256=hashlib.sha256(json.dumps(build['source_pins'],sort_keys=True).encode()).hexdigest(),seconds=r['command']['seconds']))
            for row in r['records']:
                if row['outcome']!='Passed':nonpasses.append(dict(configuration=config,suite=suite,class_name=row['class_name'],method=row['method'],test_name=safe_display(row['test_name']),outcome=row['outcome'],message_sha256=hashlib.sha256(row['message'].encode()).hexdigest(),classification='INHERITED_EXACT_IDENTITY_AND_OUTCOME'))
    with (HERE/'managed-test-identities.csv').open('w',encoding='utf-8',newline='') as f:
        w=csv.DictWriter(f,fieldnames=list(identity_rows[0]));w.writeheader();w.writerows(identity_rows)
    focused=[]
    for config in ['Debug','Release']:
        for suite in ['Core','Common','Legacy']:
            p=RUN/'executions'/f'verified-{config}-graphics31-{suite}'/'cases.json';r=read(p)
            assert r['command']['exit_code']==0 and set(r['outcomes'])=={'Passed'}
            focused.append(dict(configuration=config,suite=suite,outcomes=r['outcomes'],trx_sha256=r['sha256'],test_identities=[dict(class_name=x['class_name'],method=x['method'],test_name=x['test_name'],outcome=x['outcome']) for x in r['records']]))
    return suites,deltas,focused,nonpasses
def source_node():
    python=[];nodes=[]
    old=read(ROOT/'evidence/04A30/source-and-node-validation-v1.json')
    for suffix in ['python-tools','python-help','python-scripts','common-python']:
        label='verified-Debug-'+suffix
        p=RUN/'commands'/label/'stderr.log';t=text(p);r=read(p.parent/'command.json')
        total=int(re.search(r'Ran (\d+) tests?',t).group(1))
        failures=int(re.search(r'failures=(\d+)',t).group(1)) if re.search(r'failures=(\d+)',t) else 0
        errors=int(re.search(r'errors=(\d+)',t).group(1)) if re.search(r'errors=(\d+)',t) else 0
        descriptions=re.findall(r'^(test_\S+ \([^)]*\))',t,re.M)
        nonpasses=[dict(outcome=k,identity=i,subtest=s.strip()) for k,i,s in re.findall(r'^(FAIL|ERROR): (test_\S+ \([^)]*\))([^\n]*)',t,re.M)]
        prior=next(x for x in old['python'] if x['label']=='verified' and x['suite']==suffix)
        prior_np=Counter((x['outcome'],x['identity'],x['subtest']) for x in prior['nonpasses'])
        now_np=Counter((x['outcome'],x['identity'],x['subtest']) for x in nonpasses)
        python.append(dict(suite=suffix,total=total,passed=total-failures-errors,failed=failures,errors=errors,exit_code=r['exit_code'],stderr_sha256=sha(p),primary_identities=descriptions,nonpasses=nonpasses,comparison_to_accepted_04A30=dict(same_reported_total=total==prior['total'],added_nonpasses=[dict(identity=i,outcome=k,subtest=s,count=n) for (k,i,s),n in (now_np-prior_np).items()],removed_nonpasses=[dict(identity=i,outcome=k,subtest=s,count=n) for (k,i,s),n in (prior_np-now_np).items()]),count_qualification='Unittest reported cases minus FAIL/ERROR subtest counts;all explicit nonpasses retained. These are software/audit results,not scientific proof.'))
    for suffix in ['office-node','resolver','wasm-node']:
        p=RUN/'commands'/('verified-Debug-'+suffix)/'stdout.log';t=text(p)
        def count(k):
            matches=re.findall(r'(?:# |\u2139 )'+k+r' (\d+)',t)
            assert matches,(suffix,k)
            return int(matches[-1])
        nodes.append(dict(suite=suffix,total=count('tests'),passed=count('pass'),failed=count('fail'),stdout_sha256=sha(p),exit_code=read(p.parent/'command.json')['exit_code']))
    save('source-and-node-validation.json',dict(schema='04A31-source-node-v1',python=python,node=nodes,configuration='Configuration-independent lanes executed once with Debug full matrix;Release managed separately complete',node_cjs='wasm-node executes all .cjs suites through node --test --test-concurrency=1',prior_reference=pin(ROOT/'evidence/04A30/source-and-node-validation-v1.json')))
    return python,nodes
def casebook():
    records=[]
    for config in ['Debug','Release']:
        for suite in ['Core','Common','Legacy']:
            folder=RUN/'compatibility-cases'/f'verified-{config}-graphics31-{suite}'
            for p in sorted(folder.glob('*.csv')):
                rows=list(csv.DictReader(p.open(encoding='utf-8-sig')));assert len(rows)==1
                r=rows[0];assert r['expected_exception']==r['actual_exception']
                if r['expected_exception']=='None':
                    left=r['expected_ieee'].split('|');right=r['actual_ieee'].split('|');assert len(left)==len(right)
                    def nan(x):
                        bits=int(x,16);return (bits&0x7ff0000000000000)==0x7ff0000000000000 and (bits&0xfffffffffffff)!=0
                    assert all(a==b or nan(a) and nan(b) for a,b in zip(left,right)),r['id']
                records.append(dict(configuration=config,suite=suite,**r,raw_csv_sha256=sha(p)))
    assert records
    unique={(r['suite'],r['id']) for r in records if r['configuration']=='Debug'}
    assert unique=={(r['suite'],r['id']) for r in records if r['configuration']=='Release'}
    save('graphics31-compatibility-casebook.json',dict(schema='04A31-compatibility-v1',authority='COMPATIBILITY_REFERENCE: accepted04A30 frozen LegacyGraphics30 source;HOST_FREE_EXECUTED:new tests in both runtimes/configurations;NOT_GOLDEN',fixture_sources=[pin(ROOT/p) for p in ['BESHStat.Core.Tests/src/Graphics/LegacyGraphics30.vb','BESHStat.Core.Tests/src/Graphics/Graphics30Tests.vb']+TESTS],comparison_rules=dict(numerics='Exact IEEE64 bits for finite/infinite/signed-zero values;NaNs compared by classification in numerical algorithms;no floating tolerance',coordinate_copies_and_provenance='Exact IEEE bits,including NaN payloads/signed zero;permutation tested directly and in Common',metadata='Expected permutations/rows manually specified in tests;new implementation is not an independent expected-value generator'),record_count=len(records),unique_suite_case_ids_per_configuration=len(unique),records=records,identity_tests='Graphics31SourceIdentityTests: shuffled supplied rows,existing parser blank-filter provenance,missing physical rows. Graphics31NormalPlotTests: hand-specified forward/inverse tie permutation,40 IEEE-distinguishable ties,copy isolation. See focused executed identities.',native='NATIVE_NOT_RUN',independent_R='NOT_RUN_UNAUTHORIZED'))
    return len(records),len(unique)
def write_report(suites,deltas,focused,python,nodes,serial,blockers):
    decision='NOT_READY' if blockers else 'READY_FOR_OWNER_04A31_ACCEPTANCE_REVIEW'
    rows=[]
    for suite in SUITES:
        debug=next(x['outcomes'] for x in suites if x['suite']==suite and x['configuration']=='Debug')
        release=next(x['outcomes'] for x in suites if x['suite']==suite and x['configuration']=='Release')
        rows.append(f"|{suite}|{debug.get('Passed',0)} / {debug.get('Failed',0)}|{release.get('Passed',0)} / {release.get('Failed',0)}|")
    sums={c:Counter() for c in ['Debug','Release']}
    for row in suites:sums[row['configuration']].update(row['outcomes'])
    py={k:sum(r[k] for r in python) for k in ['total','passed','failed','errors']}
    node={k:sum(r[k] for r in nodes) for k in ['total','passed','failed']}
    focus='; '.join(f"{x['configuration']} {x['suite']} {x['outcomes']['Passed']}/{x['outcomes']['Passed']}" for x in focused)
    files='\n'.join('- `'+p+'`' for p in WINDOWS+NEW_CORE+TESTS+[PROJECT])
    report=f'''# Official batch 04A.31 owner review

**{decision}** — software portability/compatibility only. {'Blockers: '+ '; '.join(blockers) if blockers else 'No batch acceptance blocker identified; inherited full-suite nonpasses remain real failures.'}

Branch `codex/04A-31`;actual verified post-04A.30 merged-main base/unchanged HEAD `{BASE}`;parent `45466a88e7567c9f4711ba4fbbe295a7a2e69bb4`. Entry tracked tree/index were empty;no owner untracked file was overwritten. Branch creation required sandbox Git-metadata approval. Other Codex/Node/GitHubDesktop processes were visible;three MSBuild processes were idle nodes. No global-writer exclusivity is claimed. All new outputs are isolated. [Entry authority](../../evidence/04A31/entry-state.json) records the stale Revision2.11 plan handoff and exact matching official04A.30–33 rows;the plan/AGENTS remain unchanged.

The actual merged compiled source is authoritative. Accepted04A.30 Core engines and all18 pre-existing Core Graphics files remain byte-identical to entry. Its119 externally hashed publication files were authenticated against the existing local pre-merge publication archive:14 direct,105 exact CRLF-to-LF transformations to committed main blobs,zero unexplained. This qualifies new post-merge normalization,including mixed endings;the archive was never branch authority. Accepted04A.29 original-byte receipt remains frozen:120 authenticated,27 direct/93 transformations. No old archive was requested or historical proof rewritten.

## Delivered files and ownership

{files}

These are two existing production adapters,two new portable production files,five new test/helper sources,and additive Framework test-project Compile entries. No Common production edit was necessary:existing `OrderedPlotGroups.PointOrder` already supplies the correct provenance seam. Other36 Windows Graphics modules,all original12 Core Graphics implementations plus six04A.30 engines,UI callers,Ribbon/UDFs,host capabilities,targets,packaging and installers are protected. No04A.32 work began.

[Pre-edit operation map](../../evidence/04A31/roc-qq-operation-map-entry.csv) and [final map](../../evidence/04A31/roc-qq-operation-map-final.csv) bind source paths,entry/final hashes/ranges,04A.29 member/operation IDs and04A.30 map links. `RocDataCore.ComputeCurve/ComputeDeLong/MidRanks`,`NormalPlotDataCore.SortedCopy/Scores/ReferenceLine`,existing ranks/distributions/quartiles/regression and Common grouping/source contracts are reused. No second numerical engine was introduced. The initial read traced `UI/UibyID.RunROC/RunNormalPlot`,binary-classification reporting and two ROC UDF routes;all callers remain unchanged.

## Coordinate and provenance contracts

`RocPlotSeriesData` snapshots X=FPR,Y=sensitivity in exact accepted order,initial(1,1),terminal(0,0). There are u cutoffs/specificities,u+1 X/Y points. Cutoff i maps curve slot i+1;slot0 has no cutoff. `CutoffPointIndexes` exposes this relationship;there is no fabricated extra threshold. Diagonal X/Y each `[0,1]`;axis0..1,major0.2,crossing0. Every getter copies. Existing CI-copy behavior is retained. [Exact ROC contract](../../evidence/04A31/roc-plot-coordinate-contract.json).

`NormalPlotSeriesData` exposes n observed-X/normal-Y/probabilities,exact method strings,n forward/inverse identity slots and a2×2 reference matrix:rows=endpoints,columns=X/Y. All three score methods × all three line methods reuse accepted calculations. `NormalPlotObservationOrder` calls unchanged `SortedCopy`;each sorted IEEE value takes the earliest unused original ordinal with identical bits. Identical-bit ties are stable. Numeric-equal signed zeros/distinct NaN payloads keep the legacy sorted coordinate slots;no historical Array.Sort stability is claimed. Both permutations and every coordinate/line getter copy independently. [Exact Q–Q contract](../../evidence/04A31/normal-qq-coordinate-and-permutation-contract.json).

Original input ordinal,sorted plot ordinal,Common group ordinal and neutral ChartIndex are zero-based and distinct. Optional physical row is one-based and comes only from actual `UnivariateObservationSource`;missing physical rows remain nullable NotAvailable. The proposed native point ordinal is sorted slot+1,not a worksheet row,and is NATIVE_NOT_RUN. Common tests prove shuffled supplied physical rows and accepted-parser filtered-row alignment. The real Windows constructor receives values/name only;`PlotPointOrder` exposes group0/input/sorted identity without invented physical rows.

Windows `ROC.compute` consumes the projection;`addROCplot` uses X/Y and Core diagonal. `NormalPlot.New/compute/computeRefLIne` consume order/series projections;`addChart` uses observedX/expectedY/reference columns. Public legacy signatures/default alpha/string spelling remain intact. Mutable Windows return-array aliases and ROC seed/repeated-call/partial DeLong-failure state remain Windows-owned. Fresh immutable snapshots capture current alias edits without changing earlier snapshots. `wrapResults` is token-identical. [Roslyn source guard](../../evidence/04A31/source-ownership.json) proves renderer differences are bounded coordinate substitutions;creation,size,labels,series order,colors,markers,axes,cleanup and object lifetime remain native-owned. This is source evidence,not executed Excel calls or chart equivalence.

## Executed validation

|Managed suite|Debug PASS / FAIL|Release PASS / FAIL|
|---|---:|---:|
{chr(10).join(rows)}

Full Debug: **{sums['Debug'].get('Passed',0)} PASS / {sums['Debug'].get('Failed',0)} FAIL**. Full Release: **{sums['Release'].get('Passed',0)} PASS / {sums['Release'].get('Failed',0)} FAIL**. Every inherited raw identity,multiplicity and outcome matches the accepted04A.30 reference;no existing case removed. New raw cases per configuration:{sum(x['new_04A31_raw_cases'] for x in deltas)},all PASS. This is a fresh full Release execution,not the04A.28 trial. Prior04A.30 counts are historical references only. [Managed identities](../../evidence/04A31/managed-test-identities.csv) and [validation/source-build/TRX pins](../../evidence/04A31/graphics31-validation.json).

Focused final: {focus}. These include unchanged04A.30 regressions. ROC matrix covers both separation directions,overlap,ties,constant/unbalanced/boundary/nonfinite/negative-zero/floating ties,alpha,permutations,copies,CI copies and explicit seeds/repeated Windows calls. Q–Q covers all nine combinations,sorted/reversed/shuffled/tied/constant/singleton/two-point/empty/missing/nonfinite/low/high magnitude data,exact string fallbacks,pre-score calls,mutable adapter aliases and source identities. [Casebook](../../evidence/04A31/graphics31-compatibility-casebook.json) records source-backed expected versus actual IEEE output,fixtures,options,culture,shapes and exceptions. Finite/infinite/signed-zero values compare by bits;numeric NaNs compare by classification;copy/provenance checks use exact bits,including NaN payloads. No floating tolerance is used. Expectations come from frozen source or explicit metadata fixtures,never from the new implementation presented as an independent oracle.

Node: **{node['passed']} PASS / {node['failed']} FAIL**,including all `.cjs` suites. Python full aggregate:**{py['passed']} PASS / {py['failed']} FAIL / {py['errors']} ERROR** out of {py['total']} reported cases;all explicit subtest failures preserved. Python identities/nonpass deltas against the accepted04A.30 reference are in [source/Node receipts](../../evidence/04A31/source-and-node-validation.json). Additional new publication/lineage audit tests:**12 PASS**,including adversarial duplicate/missing/hash/size/self/path-escape cases and post-seal validation. The compiled Core API guard also executes two independent inverse-permutation copy checks with hand-specified metadata expectations. Strict/profile exits **{serial['strict']}/{serial['profile']}**;strict MkDocs exit **{serial['docs']}**. None of these nonzero statuses is reported green. [Nonpasses](../../evidence/04A31/nonpasses.json) separates inherited issues,post-merge EOL receipt effects and any blockers.

Core/Common netstandard2.0 and Windows Framework4.8 compile in both configurations with isolated outputs. Windows compile-only targets omit packaging/post-build/signing/version/help generators. No restore/install/network request occurred. The unchanged verifier wrappers themselves are NOT_RUN because they hard-code shared historical output paths and Legacy packaging hooks;their complete portable/Legacy managed equivalents were executed through the accepted isolated runner. Exact commands,real exits and failed intermediate receipts are retained in [command outcomes](../../evidence/04A31/command-outcomes.json). New Python evidence/manifest/privacy tests are separately recorded there;the full existing Python matrix remains unmodified.

## Preservation, restrictions and handoff

[Preservation](../../evidence/04A31/source-preservation.json) checks every3933 entry tracked file:only the two authorized adapters and additive test project changed. All100 mixed-model captures authenticate against the frozen registry;04A.01 decisions,04A.29 canonical-v2 lineage/uncertainties,04A.30 source/evidence and all historical tests/expected values remain unchanged. [Source successor](../../evidence/04A31/source-transition-successor.json) retains the278-pin lineage:272 original unchanged,four retained04A.30 transitions,two further authorized04A.31 transitions. The historical six failing source-pin subtests remain unchanged failures. [Historical stat receipt](../../evidence/04A31/historical-results-preservation.json) compares membership,size and mtime only;the138GB historical payload was NOT_REHASHED. No historical result was deleted,archived or rewritten.

[Deferred register](../../evidence/04A31/deferred-issues.json) retains ROC cutoff-table versus curve-slot indexing,nonfinite/terminal/alpha methodology,legacy repeated state,Q–Q degenerate/quartile/R fitting semantics and unavailable physical row acquisition. Scientific/report investigation belongs04A.39–41,native confirmation04A.45–48;detrending04A.68–69. No statistical correction was activated. Box/Histogram/Violin/axis remain04A.32;other families remain04A.33–37.

P=NO;N1=NO GO;N2=NO;Gate C2=UNMET;Gate C3 not approved. Original native controls/deferred obligations remain OPEN,zero releases/discharges. All comparisons **NOT_GOLDEN**. Actual Excel/COM/UIA/RefEdit/chart rendering/callbacks/native smoke and independent R/scientific oracle execution are **NOT_RUN / NATIVE_NOT_RUN**. Software compatibility is not independent scientific accuracy,chart appearance equivalence or Office/Mac availability. [Proposed synthetic native smoke](../../evidence/04A31/native-smoke-proposal.md) is reproducible but requires separate authorization. Browser/WASM manual smoke is NOT_RUN;no new production renderer is introduced.

Final intended publication is exactly [publication-manifest.json](../../evidence/04A31/publication-manifest.json):relative paths,byte sizes,SHA256 for every file,and a null self-entry to avoid recursion. [Privacy](../../evidence/04A31/privacy.json) reports sanitized published paths/identity displays;private raw logs remain ignored. Integrity,manifest,traceability,Git diff and empty-index checks must pass at sealing. Nothing is staged,committed,merged,pushed,deployed,signed or installed. Owner publication and independent acceptance review remain separate;04A.32 is not started.
'''
    (ROOT/DOSSIER).write_text(report,encoding='utf-8')
    return decision,sums,py,node
def collect():
    protection();commands=command_receipts();suites,deltas,focused,nonpasses=managed();python,nodes=source_node();case_count,unique=casebook()
    serial={name:read(RUN/'commands'/('final-'+name)/'command.json')['exit_code'] for name in ['strict','profile','docs']}
    audit_command=read(RUN/'commands/final-audit-tests/command.json')
    audit_text=text(RUN/'commands/final-audit-tests/stderr.log')
    assert audit_command['exit_code']==0 and re.search(r'Ran 12 tests',audit_text) and 'skipped=' not in audit_text
    execution_summaries=[]
    for name in ['verified-Debug-matrix.json','verified-Release-managed.json']:
        p=RUN/name;summary=read(p);assert summary['source_unchanged']
        execution_summaries.append(dict(name=name,receipt_sha256=sha(p),source_unchanged=True))
    blockers=[]
    if serial['docs']!=0:blockers.append('Strict MkDocs failed')
    if any(x['failed'] for x in nodes):blockers.append('Node suite failure')
    for row in python:
        # Post-merge EOL discrepancies in frozen04A29 audit tests are separately
        # qualified by authenticated publication bytes/current Git,not rewritten.
        for x in row['comparison_to_accepted_04A30']['added_nonpasses']:
            if 'test_04a29_graphics' not in x['identity']:blockers.append('Unclassified Python nonpass: '+x['identity'])
    decision,sums,py,node=write_report(suites,deltas,focused,python,nodes,serial,blockers)
    save('graphics31-validation.json',dict(schema='04A31-validation-v1',decision=decision,blockers=blockers,managed=suites,identity_deltas=deltas,focused=focused,full_totals={k:dict(v) for k,v in sums.items()},builds=[dict(configuration=c,exit_codes=read(RUN/'builds/verified'/c/'build.json')['exit_codes'],source_unchanged=read(RUN/'builds/verified'/c/'build.json')['source_unchanged'],receipt_sha256=sha(RUN/'builds/verified'/c/'build.json')) for c in ['Debug','Release']],execution_summaries=execution_summaries,python=py,new_python_audit_tests=dict(total=12,passed=12,failed=0,errors=0,skipped=0,exit_code=audit_command['exit_code'],stderr_sha256=sha(RUN/'commands/final-audit-tests/stderr.log'),qualification='Tests verify the sealed payload set;final metadata receipts are refreshed/resealed afterwards and verified again without changing product or tests.'),runtime_inverse_copy_checks=read(HERE/'source-ownership.json')['runtime_inverse_copy_checks'],node=node,strict_profile_mkdocs=serial,casebook_records=case_count,unique_suite_case_ids_per_config=unique,existing_suite_policy='Full managed Debug/Release;Node/Python once;strict/profile/MkDocs real exits;isolated accepted runner;no frozen assertions changed',native='NATIVE_NOT_RUN',R='NOT_RUN_UNAUTHORIZED',scientific='NOT_GOLDEN'))
    save('nonpasses.json',dict(schema='04A31-nonpasses-v1',decision=decision,blockers=blockers,managed=nonpasses,python=python,strict_profile_mkdocs=serial,historical_04A30=dict(managed_each='19930 PASS/14 FAIL',node='916 PASS',python='840 PASS/106 FAIL/10 ERROR',strict_profile='1/1',mkdocs='PASS',authority='Historical only;not substituted for04A31 execution'),six_original_source_pin_assertions='Retained FAIL;successor protects authorized two further transitions;zero frozen expected edits',postmerge_normalization='Entry authority authenticates all11904A30 payloads;14 direct/105 exact transformations. Any frozen04A29 normalization-sensitive receipt assertions remain actual nonpasses,not byte corruption.',intermediate='trial build failed on reserved VB variable in new tests;trial2 focused had new parser fixture error,then corrected to established increasing-source-row policy. Core calculations never changed. Failed receipts remain locally;final verified focus supersedes them. New helper variable/EOF guard defects fixed and recorded.'))
    print(decision,dict((c,dict(s)) for c,s in sums.items()),'Python',py,'Node',node,'blockers',blockers)
def publication_paths():
    return sorted(set(WINDOWS+NEW_CORE+TESTS+[PROJECT,DOSSIER]+[p.relative_to(ROOT).as_posix() for p in HERE.rglob('*') if p.is_file() and '__pycache__' not in p.parts]))
def privacy():
    findings=[]
    patterns=[r'[A-Za-z]:[\\/]+Users[\\/]+',r'S-1-5-\d+(?:-\d+)+',r'(?i)(?:DESKTOP|LAPTOP)-[A-Z0-9]+']
    for relative in publication_paths():
        t=(ROOT/relative).read_text(encoding='utf-8-sig')
        # Regex source syntax is not a private value;these patterns seek actual paths/IDs.
        if any(re.search(p,t) for p in patterns):findings.append(relative)
        host=os.environ.get('COMPUTERNAME','!unset!')
        if host.lower() in t.lower():findings.append(relative)
    assert not findings,findings
    save('privacy.json',dict(schema='04A31-privacy-v1',status='PASS',files_scanned=len(publication_paths()),findings=[],rules='No actual private home/drive paths,SIDs,hostnames,personal workbooks,secrets or raw user observations in proposed publication;synthetic test inputs only. Existing security-test displays redacted by SHA identity;raw logs remain local/ignored.',relative_paths_only=True))
def seal():
    assert not git('diff','--cached','--name-only')
    assert not git('diff','--check')
    assert git('branch','--show-current')=='codex/04A-31' and git('rev-parse','HEAD','main').splitlines()==[BASE,BASE]
    privacy()
    paths=sorted(set(publication_paths()+['evidence/04A31/publication-manifest.json']))
    files=[dict(path=p,bytes=None,sha256=None) if p=='evidence/04A31/publication-manifest.json' else pin(ROOT/p) for p in paths]
    save('publication-manifest.json',dict(schema='04A31-publication-v1',self_rule='Self included with null bytes/SHA256;no recursive hash',files=files))
    verify()
def verify():
    manifest=read(HERE/'publication-manifest.json');check_manifest(manifest['files'],set(publication_paths()))
    changed=set(git('diff','--name-only').splitlines());assert changed==set(WINDOWS+[PROJECT])
    assert not git('diff','--cached','--name-only') and not git('diff','--check')
    extras=set(git('ls-files','--others','--exclude-standard').splitlines())
    assert extras==set(publication_paths())-set(git('ls-files').splitlines()),extras-set(publication_paths())
    print('PASS manifest exact bytes/paths/self;privacy;traceability;scoped Git diff;index EMPTY;',len(manifest['files']),'publication entries')
def check_manifest(rows,expected_paths):
    paths=[r['path'] for r in rows]
    assert len(paths)==len(set(paths)) and set(paths)==expected_paths
    assert all(p and '\\' not in p and ':' not in p and not p.startswith('/') and all(x not in ('','..','.') for x in p.split('/')) for p in paths),'Noncanonical publication path'
    for row in rows:
        if row['path']=='evidence/04A31/publication-manifest.json':assert row['sha256'] is None and row['bytes'] is None
        else:assert row['sha256'] is not None and row==pin(ROOT/row['path']),row['path']
if __name__=='__main__':{'collect':collect,'seal':seal,'verify':verify}[sys.argv[1]]()
