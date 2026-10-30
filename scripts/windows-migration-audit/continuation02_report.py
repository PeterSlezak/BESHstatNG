"""Reproducible controlled-continuation02 checkpoint; preserves earlier reports."""
from __future__ import annotations
import argparse
import collections
import json
import re
import xml.etree.ElementTree as ET
import catalog
import semantic
from amendment import git, read, sha

ROOT, OUT = catalog.ROOT, catalog.DEFAULT
EVIDENCE = ROOT / 'TestResults/04A-01-continuation-02'
CHECK = False


def write(name, value):
    value = value.rstrip() + '\n'
    path = OUT / name
    if CHECK:
        if not path.is_file() or path.read_text(encoding='utf-8-sig') != value:
            raise ValueError('Continuation report drift: ' + name)
    else:
        path.write_text(value, encoding='utf-8', newline='')


def emit(name, value):
    write(name, json.dumps(value, indent=2, ensure_ascii=False))


def text(path):
    data = path.read_bytes()
    return data.decode('utf-16' if data.startswith((b'\xff\xfe', b'\xfe\xff')) else 'utf-8-sig', errors='replace')


def validation():
    runs = read(EVIDENCE/'validation-runs.json') if (EVIDENCE/'validation-runs.json').exists() else []
    checks = read(EVIDENCE/'final-checks.json') if (EVIDENCE/'final-checks.json').exists() else []
    managed, other = [], []
    for config in ('Debug','Release'):
        for suite in ('Core.trx','Common.trx','OfficeAddin.trx','OfficeExecution.trx','Legacy.trx',f'OfficeAddin-{config}.trx'):
            path = EVIDENCE/config/suite
            if path.exists():
                managed.append(dict(configuration=config,suite=suite,**ET.parse(path).find('.//{*}Counters').attrib,
                    distinct_full_suite=suite!=f'OfficeAddin-{config}.trx',path=path.relative_to(ROOT).as_posix(),sha256=sha(path)))
        for kind in ('full','foundation'):
            path=EVIDENCE/config/(kind+'.log')
            if path.exists():
                content=text(path)
                other.append(dict(configuration=config,kind=kind,path=path.relative_to(ROOT).as_posix(),sha256=sha(path),
                    python_counts=re.findall(r'Ran (\d+) tests? in',content),node_counts=re.findall(r'(?:# |\u2139 )tests (\d+)',content),
                    node_skips=re.findall(r'(?:# |\u2139 )skipped (\d+)',content)))
    extra=[]
    for name in ('source-before.log','report-before.log','analyzer-build.log','negative-red.log','negative-green.log',
                 'protected-after.log','source-final.log','report-check.log','tracking-check.log'):
        path=EVIDENCE/name
        if path.exists(): extra.append(dict(path=path.relative_to(ROOT).as_posix(),sha256=sha(path)))
    negative_path=EVIDENCE/'negative-reconciliation.log'
    counts=re.findall(r'Ran (\d+) tests? in',text(negative_path)) if negative_path.exists() else []
    python='C:/Users/peter/Desktop/WPy64-31150/python-3.11.5.amd64/python.exe -B '
    dll='dotnet scripts/windows-migration-audit/bin/Debug/net10.0/WindowsMigrationAudit.dll '
    command_logs=[
        ('dotnet build scripts/windows-migration-audit/WindowsMigrationAudit.csproj --configfile scripts/windows-migration-audit/NuGet.Config -v minimal','analyzer-build.log',0),
        (dll+'--check','source-before.log',0),
        (python+'scripts/windows-migration-audit/report.py --check','report-before.log',0),
        (python+'scripts/windows-migration-audit/test_reconciliation.py','negative-red.log',1),
        (python+'scripts/windows-migration-audit/test_reconciliation.py','negative-green.log',0),
        (dll+'--write','source-final.log',0),
        (python+'scripts/windows-migration-audit/catalog.py --write','catalog-final.log',0),
        (python+'scripts/windows-migration-audit/continuation02_report.py','continuation-report-generation-encoding-failure.log',1),
        (python+'scripts/windows-migration-audit/continuation02_report.py --check','continuation-report-check-first-drift.log',1),
        (python+'scripts/windows-migration-audit/continuation02_report.py --check','continuation-report-check-self-log-drift.log',1),
        (python+'scripts/windows-migration-audit/report.py --check','report-check.log',0),
        (python+'scripts/windows-migration-audit/amendment.py check-tracking','tracking-check.log',0),
        (python+'TestResults/04A-01-continuation-02/session.py check','protected-final.log',0)]
    additional_commands=[dict(command=command,exit_code=code,log=(EVIDENCE/name).relative_to(ROOT).as_posix(),sha256=sha(EVIDENCE/name))
                         for command,name,code in command_logs if (EVIDENCE/name).exists()]
    return dict(scope='Fresh controlled continuation02 only; prior runs remain historical',runs=runs,checks=checks,
        audit_negative_test_counts=counts,parser_fixture_assertions=14,
        additional_commands=additional_commands,
        managed=managed,python_node=other,additional_logs=extra,
        development_failures=['Initial new-negative-test run used an intermediate stale generated ledger and exposed missing guards (7 failures/26); final guarded ledger is checked separately.',
            'A local transcription stopped before saving on an incorrect SafeWrite.vb dependency path; corrected to the actual Common Output/WriteResult.vb.',
            'Initial continuation report generation stopped on a legacy source encoding during the lexical logger census. BOM-aware UTF decoding with replacement matches the read-only scanner boundary; raw source hashes and Roslyn remain authoritative. No source re-encoding occurred.',
            'The first continuation report check detected newly created report/tracking log hashes that had not yet been rendered into the validation manifest; regenerated in documented order, then checked again.',
            'Adding the active continuation-check log to its own manifest caused a self-referential hash drift; removed that active-log hash. Immutable failed-check logs remain recorded; the final read-only check passes.',
            'Read-only source lookups using nonexistent CheckForUpdate.vb and Windows wildcard rg arguments failed; exact paths were then read. No source mutation occurred.'],
        side_effects='No new development-certificate output observed; both DOTNET_GENERATE_ASPNET_CERTIFICATE=false and telemetry opt-out set. Prior amendment certificate side effect remains historical, not erased.',
        not_run=['Live Excel/COM/XLL/Ribbon/RefEdit or browser execution','Authenticated Office or Mac runtime','04A.02 numerical golden capture','Installer/signing/deployment/publishing/network package installation'])


def main():
    before=read(EVIDENCE/'before.json')
    original=EVIDENCE/'original/docs/architecture/WindowsMigrationLedger'
    old= catalog.load(original,'symbols.csv')
    symbols=catalog.load(OUT,'symbols.csv')
    sem=read(OUT/'semantic-review-coverage.json')
    rev=read(OUT/'review-coverage.json')
    old_sem=before['starting_review']; old_rev=before['starting_coverage']
    edges=catalog.load(OUT,'ui-wiring.csv')
    old_by_id={s['symbol_id']:s for s in old}
    changed=[s for s in symbols if s.get('review_revision')=='04A.01-continuation-02']
    legacy=[s for s in old if s['review_status']=='manually-reviewed-static' and not semantic.qualified(s)]
    now={s['symbol_id']:s for s in symbols}
    assert len(legacy)==36
    assert all(semantic.qualified(now[s['symbol_id']]) for s in legacy)
    for name in ('completion-amendment.md','amendment-validation.json','amendment-count-deltas.json'):
        assert sha(OUT/name)==sha(original/name), 'Historical checkpoint was overwritten: '+name
    fields=('symbol_id','path','start_line','end_line','signature','containing_type','body_sha256','family','disposition','responsibility',
            'source_purpose','separable_responsibilities','equivalence_scope','core_common_equivalent','equivalent_evidence','reusable_operation',
            'destination','future_batch','risk','exception_tags','exception_applicability','proposed_test','owner_role','runtime_boundary','dependency_pins')
    write('continuation02-member-reviews.csv',catalog.render([{**{k:s.get(k,'') for k in fields},
        'previous_disposition':old_by_id[s['symbol_id']]['disposition'], 'previous_review_contract':old_by_id[s['symbol_id']].get('review_contract','')} for s in changed]))
    write('continuation02-legacy-upgrades.csv',catalog.render([dict(symbol_id=s['symbol_id'],path=s['path'],start_line=s['start_line'],
        old_disposition=s['disposition'],new_disposition=now[s['symbol_id']]['disposition'],new_qualified=semantic.qualified(now[s['symbol_id']]),
        future_batch=now[s['symbol_id']]['future_batch'],evidence='continuation02-member-reviews.csv') for s in legacy]))
    candidates=[s for s in symbols if semantic.qualified(s) and s['disposition'].startswith('extract-to-')]
    write('inspected-candidates.csv',catalog.render([{k:s.get(k,'') for k in fields if k not in ('responsibility','source_purpose','runtime_boundary','dependency_pins')} for s in candidates]))
    reasons=collections.Counter((s['family'],'detailed source review' if semantic.qualified(s) else 'no pinned source review',s['disposition'],s['future_batch'],
        'inspected' if semantic.qualified(s) else 'provisional; review owed in04A.01') for s in symbols)
    write('counts-by-reason-and-batch.csv',catalog.render([dict(family=f,review_reason=r,disposition=d,batch=b,allocation_evidence=a,records=n) for (f,r,d,b,a),n in sorted(reasons.items())]))
    remaining_controls=[dict(control_id=c['control_id'],path=c['path'],line=c['line'],state=c['state'],
        review_reason='routing intent only; full contract incomplete' if c['intent_review_status']!='unreviewed' else 'no independent control contract',
        owner_role=c['owner_role'],future_batch=c['future_batch'],verification=c['verification']) for c in catalog.load(OUT,'ui-controls.csv') if not semantic.control_qualified(c)]
    write('continuation02-remaining-controls.csv',catalog.render(remaining_controls))
    pinned_edges=[e for e in edges if e['target_status']=='reviewed-runtime-supplied-delegate; runtime deferred']
    write('continuation02-runtime-boundaries.csv',catalog.render([{k:e.get(k,'') for k in ('edge_id','path','start_line','owner_id','target_ids','target_status','instance_contract','lifetime_contract','static_boundary','owner_role','future_batch','verification')} for e in pinned_edges]))
    changes={}
    for key,a,b in (('member_v1',old_sem['qualified_source_reviews'],sem['qualified_source_reviews']),
                    ('legacy_detail_gaps',old_sem['legacy_pinned_reviews_requiring_details'],sem['legacy_pinned_reviews_requiring_details']),
                    ('unpinned',old_rev['unreviewed_static_records'],rev['unreviewed_static_records']),
                    ('member_investigations',old_rev['member_investigations'],rev['member_investigations']),
                    ('unresolved_edges',old_rev['unresolved_edge_rows'],rev['unresolved_edge_rows']),
                    ('full_controls',old_sem['qualified_full_control_contracts'],sem['qualified_full_control_contracts']),
                    ('files_without_qualified_review',len(old_sem['files_without_qualified_review']),len(sem['files_without_qualified_review']))):
        changes[key]=dict(before=a,after=b,delta=b-a)
    emit('continuation02-count-deltas.json',dict(changes=changes,families=sem['families'],batch_allocations=sem['batch_allocations'],
        new_or_upgraded_member_rows=len(changed),candidate_records=len(candidates),candidate_operations='NOT DEDUPLICATED OR FROZEN; complete semantic inventory still owed in04A.01'))
    emit('continuation02-start.json',{k:before[k] for k in ('captured_utc','branch','head','status','ignored_status','original_artifacts','zip_evidence')})
    gc8=[
        ('BESHStat.Core/src/AppInfrastructure/CoreInfrastructure.vb','25-70','IAppLogger / NullLogger','Interface accepts text and raw exceptions; default no-op alternative is not file IO.','Core/host architecture maintainer'),
        ('BESHStat.Core/src/AppInfrastructure/CoreInfrastructure.vb','93-136;325-343','DefaultErrorReporter / CoreServices.Logger','Injected logger is transitive runtime dependency; setter Nothing selects NullLogger and rebuilds default error reporter.','Core/host architecture maintainer'),
        ('BESHStat.Core/src/LinearAlgebra/MatrixFactorizationCore.vb','37;143;158;228','Cholesky / LU / solve error paths','CoreServices.Errors.LogAndThrow enters default reporter and injected IAppLogger unless host supplies another IErrorReporter; Windows Matrix facades therefore transitively depend on logger/host error policy.','Core numerical/host architecture maintainer'),
        ('BESHStat.Core/src/AppInfrastructure/CoreInfrastructure.vb','394-589','SimpleFileLogger','Owns Logs files, timer/writer synchronization, buffered text, warning/error stack capture and disposal. Core still contains filesystem implementation.','Core/host architecture maintainer'),
        ('BESHStat.Core/src/AppInfrastructure/AppSettings.vb','110-192','BeshStatNgSettingsStore','Additional Core filesystem/XML implementation; recovery invokes global logger and attempts default rewrite.','Settings/Core maintainer'),
        ('BESHstatNG/BESHStatAddIn.vb','28-29;54-69','AutoOpen / AutoClose','Only production New SimpleFileLogger site observed; installs Core global and diagnostic sink; close disposes logger and clears injected host state.','Windows Excel-DNA lifecycle maintainer'),
        ('BESHstatNG/src/AppInfrastructure/AppGlobals.vb','73-79','gLogger As IAppLogger','Property forwards CoreServices.Logger; does not own second logger instance.','Windows host maintainer'),
        ('BESHstatNG/src/AppInfrastructure/WindowsDiagnostics.vb','12-28','CoreLoggerDiagnosticSink.New / EmitAsync','Borrowed IAppLogger receives Common safe DiagnosticRecord strings; no raw exception overload in this adapter, no logger disposal ownership.','Diagnostics maintainer'),
        ('BESHStat.Excel.Common/src/Infrastructure/Diagnostics.vb','1-178','IDiagnosticSink / DiagnosticEmitter','Typed bounded safe record contract used by host sink; not identical to raw Core IAppLogger signature.','Common diagnostics maintainer'),
        ('BESHStat.OfficeAddin/Infrastructure/FoundationServices.cs','32-34','service registration','Scoped OfficeDiagnosticLog implements Common IDiagnosticSink; no SimpleFileLogger installation here.','Office host maintainer'),
        ('BESHStat.OfficeAddin/Services/OfficeDiagnosticLog.cs','7-30','OfficeDiagnosticLog','Bounded pane-local in-memory sink/export/disposal; alternative to Windows sink for Common records, not wholesale Core logger replacement.','Office diagnostics maintainer')]
    write('continuation02-gc8-evidence.csv',catalog.render([dict(path=p,source_range=span,contract=contract,source_finding=finding,
        sha256=sha(ROOT/p),owner_role=owner,future_batch='04A.44',gate='GC8 remains open; E7 host interface boundary',
        static_limit='Actual transitive logger destination depends on host-injected CoreServices.Logger; lexical callers alone do not prove runtime instance.',
        required_fixture='04A.44 LoggerInjectionOwnershipAndNoFilesystemHost: NullLogger/default reporter, native file logger dispose, safe Common record and pane-only sink; no native execution performed')
        for p,span,contract,finding,owner in gc8]))
    by_path=collections.defaultdict(list)
    for member in symbols+catalog.load(OUT,'portable-symbols.csv'):
        by_path[member['path']].append(member)
    callsites=[]
    pattern=re.compile(r'CoreServices\.(?:Logger|Log\b|Errors)|AppGlobals\.gLogger|\bgLogger\.(?:Info|Warn|Debug|Trace|Error|WriteError)|New SimpleFileLogger')
    for path,members in sorted(by_path.items()):
        raw=(ROOT/path).read_bytes()
        content=raw.decode('utf-16' if raw.startswith((b'\xff\xfe',b'\xfe\xff')) else 'utf-8-sig',errors='replace')
        for line,code in enumerate(content.splitlines(),1):
            if code.lstrip().startswith("'") or not pattern.search(code):
                continue
            owners=[s for s in members if int(s['start_line'])<=line<=int(s['end_line'])]
            callsites.append(dict(path=path,line=line,source_text=code.strip(),sha256=sha(ROOT/path),
                enclosing_source_ids='; '.join(sorted({s['symbol_id'] for s in owners})),
                evidence_status='lexical callsite census only; no semantic-review promotion',
                text_decoding='BOM-aware UTF; replacement for legacy undecodable text; raw hash/source/Roslyn are authoritative',
                boundary='Global interface destination depends on host injection; Core default NullLogger; Windows AutoOpen installs SimpleFileLogger. Errors may be host-overridden.',
                owner_role='Core / host diagnostics maintainer',future_batch='04A.44',required_fixture='LoggerInjectionOwnershipAndNoFilesystemHost'))
    write('continuation02-logger-callsites.csv',catalog.render(callsites))
    val=validation()
    emit('continuation02-validation.json',val)
    table='\n'.join('| '+' | '.join(map(str,(f['family'],f['parsed_records'],f['qualified_source_reviews'],f['unreviewed_for_acceptance'],f['needs_investigation'],f['inspected_extraction_candidates'])))+' |' for f in sem['families'])
    per_file=collections.Counter(s['path'] for s in changed)
    file_table='\n'.join('| '+p+' | '+str(n)+' | '+str(sum(semantic.qualified(s) for s in symbols if s['path']==p))+'/'+str(sum(s['path']==p for s in symbols))+' |' for p,n in sorted(per_file.items()))
    counter_table='\n'.join('| '+k+' | '+str(v['before'])+' | '+str(v['after'])+' |' for k,v in changes.items())
    managed='\n'.join('| '+' | '.join(str(r.get(k,0)) for k in ('configuration','suite','passed','failed','notExecuted'))+' |' for r in val['managed'])
    distinct={c:sum(int(r.get('passed',0)) for r in val['managed'] if r['configuration']==c and r['distinct_full_suite']) for c in ('Debug','Release')}
    write('continuation02.md',f'''# 04A.01 controlled continuation02 — partial checkpoint

**04A.01 ongoing / NOT READY FOR ACCEPTANCE.** This is a same-batch checkpoint, not batch acceptance or permission for04A.02. Owner review of this partial checkpoint is required; the outstanding source inventory remains mandatory in04A.01. Gate C2 is not met; E1–E7 open, GC3–GC8 deferred,40/223 accepted remain unchanged.

Branch `{before['branch']}`, HEAD `{before['head']}`. Actual main resolves to `{git('rev-parse','main')}`. No tracked/staged changes at start or end. All2,109 protected tracked paths,1,543 Gate C evidence hashes,171 compiled source hashes and the immutable52-file original baseline remain byte-identical. The122 indexed paths at continuation start were verified and copied to a separately named ignored `TestResults/04A-01-continuation-02/original/`; no baseline was recaptured. The51-entry original baseline is retained, and the continuation-start121-entry index remains in that snapshot. No ZIP is present locally; ZIP byte comparisons remain owner-supplied historical evidence. Git source provenance remains in source-provenance.json: the19 changed VB files belong to the already-merged pre-audit cleanup, with no tracked worktree source delta.

## Progress and remaining population

Denominators remain171 files /3,866 records (3,864 active plus2 alternative), zero parser diagnostics;2,056 controls and19,701 wiring/configuration rows. Registration remains84 Ribbon buttons/81 actions plus inherited LoadImage,268 UDF exports in23 exporting/18 helper-only files and authored76 methods/5 commands/2 pages. Common Office executable capability remains ANOVA/Kruskal–Wallis only. No authority or product code changed.

| Counter | Start | End |
|---|---:|---:|
{counter_table}

| Family | Records | Member-v1 | Remaining | Investigations | Inspected candidate rows |
|---|---:|---:|---:|---:|---:|
{table}

All36 legacy gaps are upgraded individually; exact old/new IDs/dispositions are in continuation02-legacy-upgrades.csv. Each new/upgraded decision has exact source identity, body hash, required dependency manifest and dependency hashes, caller/state purpose, exact equivalent declarations where verified, equivalence scope, owner, batch, compatibility risks and named fixtures. No generated rule contributes review coverage. The files reviewed in this continuation are:

| Source | New/upgraded rows | Total detailed/file population |
|---|---:|---:|
{file_table}

Priority B is partial: AppInfrastructure, Help and Update are complete; RegModels Matrix is complete but the rest of RegModels, DM and project support remain. The four legacy DM upgrades do not imply their files are complete. Next coherent group: ExcelDnaSharedOutputPlanAdapter/ExcelDnaCompositeOutputAdapter/ExcelDnaOutputTargetAdapter, comparing Common placement/intent/safe-write and actual writer scope, then importer/UIprocedures and remaining RegModels. Complete source IDs remain in remaining-member-review.csv; full control IDs remain in continuation02-remaining-controls.csv. The remaining UI1,372, Graphics1,048 and UDF673 record queues remain mandatory, including all268 export-specific boundaries. Their complete family denominators are1,547/1,062/703 respectively, as shown above. No new UDF member review was added here.

## Ownership findings

Matrix typed forwarding facades preserve public names/defaults/ByRef outputs and explicitly target Core MatrixArithmeticCore, MatrixFactorizationCore, MatrixDecompositionCore, MatrixStatisticsCore, DM/ArrayUtilities and StatTests/CorrelationMatrixCore. Exact paths/signatures are in continuation02-member-reviews.csv and review-decisions.json. SVD/LU/QR/eigen result projections are helper reuse, not complete replacement. Core LU back substitution mutates the supplied RHS. Identity/constant-vector arguments are highest indices, VerticalStack appends columns and HorizontalStack appends rows. No numerical parity was newly proved. DiagMatFromVector already duplicates public Core DiagonalMatrixFromVector; Pythag duplicates a private Core scaled hypotenuse helper and requires a future visibility seam, not a second algorithm. Copy/extraction candidates preserve row/column shape, culture and formatting. Matrix numerical ownership goes to04A.27/04A.48 and diagnostic string policy to04A.44 as individually pinned.

AppInfrastructure native lifecycle watches, four attach/detach delegate pairs, UI thread checks, inline execution/checkpoints and output outcome semantics reuse Common services without claiming native equivalence. Retained adapter rows identify exact state/caller roles; mixed responsibilities and helper-only evidence are explicit. CoreServices logger/global proxy is a complete property facade; raw native logging is distinct from Common bounded diagnostic records. LegacyPreferenceStore bridges legacy XML versions1–3 to Common immutable schema1 preferences, including unsupported-future no-rewrite, malformed recovery, saved-state truth and synchronous Task.FromResult IO.

GC8 remains open: BESHstatNG/BESHStatAddIn.vb:28 installs `New SimpleFileLogger`, AppGlobals.gLogger:73–79 delegates to CoreServices.Logger, WindowsDiagnostics.CoreLoggerDiagnosticSink emits safe record strings into that interface, and numerical/transitive Core error paths may use the injected logger. CoreInfrastructure.vb:394 SimpleFileLogger owns directory/files/timer/streams and raw stack/exception formatting; the default Core logger is NullLogger. Adjacent BESHStat.Core/src/AppInfrastructure/AppSettings.vb also contains file-backed settings IO. Core is therefore not described as filesystem-free. Office Infrastructure/FoundationServices.cs instead registers the bounded pane-local Services/OfficeDiagnosticLog.cs implementing Common IDiagnosticSink; this is a host alternative, not proof all IAppLogger consumers were migrated. continuation02-gc8-evidence.csv pins exact inspected sources, caller/contract edges, owner and future fixture. No migration/removal was performed.

Help enum/Form routing is distinct from Common HelpDestination.ForMethodKey/ForPageKey, HelpLaunchTarget.CreateHome/GetLaunchTarget/SafeOpenUrl. GetSafeOpenUrl is a whole forwarding facade. Legacy title normalization is an inspected Common candidate. Update.IsNewerVersion compares signed Integer.Parse components only to the shorter array length, returning False for equal prefixes; Common ProductVersionDescriptor.TryParse requires four unsigned components and is not equivalent. Update eligibility/version policy is a04A.44 candidate; roaming XML IO, network, native queue/prompt remain host boundaries. Date parsing, future-clock suppression, failed-fetch daily throttle and remote detailsURL risks are recorded, not corrected here.

Candidate total is{len(candidates)} inspected member rows, not unique operation count and not frozen. counts-by-reason-and-batch.csv and continuation02-count-deltas.json separate inspected allocations from provisional unreviewed ones. Existing04A.02–04A.48 roadmap entries are preserved; no75-batch roadmap change.

## UI and runtime boundaries

Only Ui99ExportChart.btnHelp was upgraded to a full control-v1 contract: literal Button defaults, direct Form parent, inherited enabled/visible/validation state, no Help reset/output/cancel semantics, WireHelp capture/disposal, exact title fallback, native focus/shell test. Constructor parameter `tagn As Integer` boxes Tag as Integer, so HelpContext ignores that Tag and uses generated class/title ExportChart mapping; a title change risks Home fallback. Help never invokes the separate export handler or SaveFileDialog. Remaining53 routing pins are not full contracts; RefEdit and UibyID contracts were not completed in this window.

Unresolved edges remain{rev['unresolved_edge_rows']}; no source routing was falsely closed. Original389 direct Handles /864 indirect candidates /803 decorative candidates are separate from independent intent coverage.1667 no-direct-Handles controls minus37 other-prefix matches yields1630 original no-Handles-found edges; see semantic-review-coverage.json. All80 dynamic/external sites remain pinned, including7 runtime-supplied delegate boundaries in continuation02-runtime-boundaries.csv; host.Revalidate is not assigned an invented handler ID. Owners, exact uncertain caller/instance, lifetime risk and named future fixtures are retained. Native proof is deferred; source review blockers remain in04A.01.

## Fresh validation and safety

Exact commands, exits, logs/TRX SHA256, failures and skips are in continuation02-validation.json. Fresh distinct managed passed: Debug{distinct['Debug']}, Release{distinct['Release']}; foundation suite reruns are excluded from distinct totals. These counts apply only to completed recorded runs, not historical substitutes.

| Configuration | Suite | Passed | Failed | Not executed |
|---|---|---:|---:|---:|
{managed}

Analyzer source/check/self-test, catalog, reconciliation negative tests, strict, protected/evidence guard and Git checks run through check-amendment.ps1 with `-EvidencePath TestResults/04A-01-continuation-02`. Strict remains exit2 for real unreviewed ownership/control/investigation blockers; corruption is exit1. New negatives reject removed required dependency, dependency manifest, exact source identity, member-v1 field and complete control contract; existing missing source/member/export/handler/Ribbon/owner/dynamic-target guards remain. A synthetic control fixture tests persistence without granting source coverage. No parser denominator changed.

Product verification uses run-amendment-validation.ps1 with the same EvidencePath, separate standard-result backups, local packages/cache only, and explicit certificate/telemetry opt-outs. Hash-verified generated VersionAttributes copies are retained before restoring exact protected originals; unexpected tracked deltas fail. The initial new-negative run failed during an intermediate stale ledger plus missing guards; the corrected final run is recorded separately. Read/transcription development failures are disclosed in the validation JSON. No native Excel, installer, signing, deployment, package installation or04A.02 golden run occurred. No newly observed dev-certificate output was normalized away.

Future smoke proposals only: click ExportChart Help with empty chart selection and pending export, rename/localize title, close/reopen and inspect duplicate attachments; exercise ANOVA/KW cancellation/source-sheet close and safe overwrite guards. WASM: confirm only ANOVA/KW remain executable. These were not run.

## Review visibility

continuation02-artifacts.csv lists every proposed path and added/modified/unchanged status against this continuation start. amendment-artifacts.csv retains comparison with the immutable original51-entry audit and adds this continuation comparison. The ignored ledger must accompany any future PR. proposed-tracked-files.json is exactly the final index artifacts plus index.json; future-staging.txt specifies per-file future `git add -f` for ledger and normal staging for visible tooling. Ignore policy is unchanged. No staging, commit, merge, push or next batch occurred. Owner: review this partial checkpoint and decide the next same-batch file group; batch acceptance is not requested.
''')
    # Enumerate all ledger/tooling paths only after writing every new output.
    names={'continuation02-artifacts.csv','amendment-artifacts.csv','proposed-tracked-files.json','future-staging.txt'}
    paths={p.relative_to(ROOT).as_posix() for p in OUT.rglob('*') if p.is_file()}
    paths|={(OUT/n).relative_to(ROOT).as_posix() for n in names}
    paths|={p.relative_to(ROOT).as_posix() for p in semantic.TOOLING.iterdir() if p.is_file()}
    paths.add('docs/architecture/WindowsMigrationLedger.md')
    old_hashes={a['path']:a['sha256'] for a in before['original_artifacts']}
    original_index=read(original/'baseline/docs/architecture/WindowsMigrationLedger/index.json')
    baseline_hashes={a['path']:a['sha256'] for a in original_index['artifacts']}
    artifacts=[]
    for p in sorted(paths):
        current=ROOT/p
        change='added' if p not in old_hashes else 'modified' if not current.exists() or sha(current)!=old_hashes[p] else 'unchanged'
        if p.endswith('/proposed-tracked-files.json') or p.endswith('/future-staging.txt'):
            change='modified' if p in old_hashes else 'added'
        if p=='docs/architecture/WindowsMigrationLedger/index.json' or p.endswith('/continuation02-artifacts.csv') or p.endswith('/amendment-artifacts.csv'):
            change='modified; self/hash manifest' if p in old_hashes else 'added; self/hash manifest'
        artifacts.append(dict(path=p,change_from_continuation_start=change,tracking='ignored; future individual force-add' if p.startswith('docs/architecture/WindowsMigrationLedger/') else 'visible untracked audit file',current_hash_authority='index.json; index excludes itself'))
    write('continuation02-artifacts.csv',catalog.render(artifacts))
    write('amendment-artifacts.csv',catalog.render([dict(path=a['path'],
        change_from_original_audit='added' if a['path'] not in baseline_hashes else 'unchanged' if sha(ROOT/a['path'])==baseline_hashes[a['path']] else 'modified',
        original_sha256=baseline_hashes.get(a['path'],''),change_from_continuation_start=a['change_from_continuation_start'],current_hash_authority=a['current_hash_authority']) for a in artifacts]))
    emit('proposed-tracked-files.json',dict(status='PROPOSED ONLY; no staging or commit performed',paths=sorted(paths),comparison='Exactly final index artifacts plus index.json; ignore policy unchanged'))
    write('future-staging.txt','# Future owner-reviewed staging only; NOT executed during04A.01.\n# Per-file force inclusion preserves .gitignore policy.\n'+'\n'.join(
        ('git add -f -- ' if p.startswith('docs/architecture/WindowsMigrationLedger/') else 'git add -- ')+json.dumps(p) for p in sorted(paths)))
    print(json.dumps(dict(status='NOT READY FOR ACCEPTANCE',member_v1=sem['qualified_source_reviews'],legacy_gaps=sem['legacy_pinned_reviews_requiring_details'],proposed_paths=len(paths))))


if __name__=='__main__':
    parser=argparse.ArgumentParser(__doc__)
    parser.add_argument('--check',action='store_true')
    CHECK=parser.parse_args().check
    main()
