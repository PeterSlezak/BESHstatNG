"""04A.12 source/obligation/candidate receipts; no native activation or old receipt edits."""
import csv
import difflib
import hashlib
import importlib.util
import json
from pathlib import Path
import re
import subprocess

ROOT = Path(__file__).resolve().parents[1]
OUT = ROOT / 'evidence/04A12'
RUN = ROOT / 'TestResults/04A-12-20261103-run01'
BASE = '90d990c51a691f30a6f90dfdebe5019a4cb24c63'
FORM = 'BESHstatNG/src/UI/Ui13GEE.vb'
PAGE = 'BESHStatNG_Help_MkDocs/docs/methods/generalized-estimating-equations-gee.md'
CHANGED = {FORM, PAGE, 'BESHstatNG/BESHStatNG.vbproj', 'BESHStatNG.Test/BESHStatNG.Tests.vbproj'}
LEDGER = 'docs/architecture/WindowsMigrationLedger/04A01-approved-deferred-obligations.json'
CONTROLS = 'docs/architecture/WindowsMigrationLedger/ui-controls.csv'
spec = importlib.util.spec_from_file_location('contract09', ROOT / 'scripts/capture-04a09-contract.py')
prior = importlib.util.module_from_spec(spec)
spec.loader.exec_module(prior)


def sha(path):
    return hashlib.sha256(Path(path).read_bytes()).hexdigest()


def digest(value):
    return hashlib.sha256(json.dumps(value, sort_keys=True, ensure_ascii=False).encode()).hexdigest()


def emit(name, value):
    value = prior.sanitize(value)
    path = OUT / name
    if path.exists():
        assert json.loads(path.read_bytes()) == value, 'Current receipt drift: ' + name
    else:
        with path.open('x', encoding='utf-8', newline='\n') as f:
            json.dump(value, f, indent=2, ensure_ascii=False)
            f.write('\n')


def git(*args):
    return subprocess.check_output(['git', '-c', 'safe.directory=' + ROOT.as_posix(), *args], cwd=ROOT).decode().strip()


def original(path):
    return subprocess.check_output(['git', '-c', 'safe.directory=' + ROOT.as_posix(), 'show', BASE + ':' + path], cwd=ROOT).decode('utf-8-sig').replace('\r\n', '\n')


def source():
    pins = json.loads((RUN / 'baseline.json').read_bytes())['source_pins']
    assert git('branch', '--show-current') == 'codex/04A-12'
    assert git('rev-parse', 'main') == git('rev-parse', 'HEAD') == BASE
    assert not git('diff', '--cached', '--name-only')
    assert set(git('diff', '--name-only').splitlines()) == CHANGED
    before = prior.members(original(FORM))
    after = prior.members((ROOT / FORM).read_text(encoding='utf-8-sig'))
    changed = {name for name in before if before[name] != after[name]}
    assert changed == {'New', 'GetFamilyCodeFromDisplayName', 'RefreshLinkOptionsForSelectedFamily', 'BuildExpandedRegressionInputs', 'RunGEE'}
    assert set(before) == set(after)
    protected = {p: h for p, h in pins.items() if p not in CHANGED}
    assert all(sha(ROOT / p) == h for p, h in protected.items())
    # Workbook/chart/lifecycle effects are source-pinned separately from extracted calculations.
    a, b = before['RunGEE'], after['RunGEE']
    suffix = '                If clsRes IsNot Nothing AndAlso clsRes.Count > 0 Then'
    assert a[a.index(suffix):] == b[b.index(suffix):]
    assert re.findall(r'WriteRes\.\w+\(', a) == re.findall(r'WriteRes\.\w+\(', b)
    sheet = r'ActiveSheet\.(?:name|Name) = "([^"]+)"'
    assert re.findall(sheet, a) == re.findall(sheet, b) == ['Data', 'GEE', 'GEE Classification']
    assert b.index('fitGEE.Fit(') < b.index("''Dump results") < b.index('fitGEE.wrapResults()') < b.index('WindowsGeeWorkflowAdapter.Report(')
    assert b.index('If IsCurrentBinomialGeeFamily()') > b.index('rr.writeToSheet(WriteRes, True)')
    public_protected = {p: h for p, h in protected.items() if '/testresults/' not in p.lower() and not p.lower().startswith('testresults/')}
    emit('source-delta.json', dict(base=BASE, branch='codex/04A-12', changed_existing={p: dict(before=pins[p], after=sha(ROOT / p)) for p in sorted(CHANGED)},
        changed_members={n: dict(before=hashlib.sha256(before[n].encode()).hexdigest(), after=hashlib.sha256(after[n].encode()).hexdigest()) for n in sorted(changed)},
        unchanged_members={n: hashlib.sha256(before[n].encode()).hexdigest() for n in before if n not in changed},
        protected_source_pins=public_protected, protected_count=len(protected), public_protected_count=len(public_protected),
        private_historical_filename_pins=len(protected) - len(public_protected),
        invariants=dict(Core_GEE_GeeData_covariance_and_all_estimators_unchanged=True, UDF_exports_and_sources_unchanged=True,
            Designers_Ribbon_Office_installers_versioning_unchanged=True, worksheet_and_writer_call_order_unchanged=True,
            chart_error_finally_tail_byte_equal_after_normalizing_line_endings=True, post_fit_control_read_boundary_retained=True,
            original_full_tests_and_all_accepted_prior_evidence_unchanged=True),
        native_executed=False, C09_patch_applied=False, C11_patch_applied=False,
        hash_scope='Whole-file raw SHA-256; member text UTF-8 LF. Real historical filename pins remain private and are verified against actual files, never public aliases.'))
    return before, changed


def restrictions(before, changed):
    rows = list(csv.DictReader((ROOT / CONTROLS).open(encoding='utf-8-sig')))
    ledger = json.loads((ROOT / LEDGER).read_bytes())
    affected = {control for r in ledger if r.get('affected_live_golden_blocked') for control in r.get('linked_control_ids', [])}
    selected = []
    for row in rows:
        cid = row['control_id']
        if not cid.startswith('BESHStatNG.Ui13GEE.'):
            continue
        obligation = next((r for r in ledger if r['original_id'] == cid), None)
        linked_methods = [name for name in sorted(changed) if re.search(r'(?<![\w.])(?:Me\.)?' + re.escape(cid.split('.')[-1]) + r'\b', before[name], re.I)]
        selected.append(dict(control_id=cid, control_row_sha256=digest(row), declaration_path=row['path'],
            declaration_line=row['line'], source_associated_changed_methods=linked_methods,
            original_restricted=cid in affected, obligation_id=obligation['obligation_id'] if obligation else None,
            obligation_row_sha256=digest(obligation) if obligation else None,
            source_record_path=obligation.get('source_record_path') if obligation else None,
            source_record_sha256=obligation.get('source_record_sha256') if obligation else None,
            status='OPEN' if obligation else 'NO ORIGINAL DEFERRED OBLIGATION ROW',
            verification_status=obligation.get('verification_status') if obligation else 'SOURCE ONLY; NOT NATIVE VERIFICATION',
            original_capture_authorized=obligation.get('capture_authorized') if obligation else False,
            original_owner_roles=obligation.get('owner_roles') if obligation else row['owner_role'],
            original_dependent_batches=obligation.get('dependent_batches') if obligation else row['future_batch']))
    assert len(selected) == 87
    assert sum(r['original_restricted'] for r in selected) == 34
    assert sum(r['obligation_id'] is not None for r in selected) == 35
    emit('restriction-crosswalk.json', dict(base=BASE, authorities={p: sha(ROOT / p) for p in [LEDGER, CONTROLS,
        'docs/architecture/04A02/04A02-final-amended-acceptance-PROPOSED.json']},
        controls=selected, form_controls=87, original_restricted_controls=34, original_open_obligations=35,
        obligation_less_source_controls=52, changed_method_source_controls=sum(bool(r['source_associated_changed_methods']) for r in selected),
        released=0, discharged=0, P='NO', N1='NO GO', N2='NO', C2='UNMET',
        original_universe='1514 obligations / 802 restricted controls; 239/218 transferred and other subsets overlap, never add.',
        UiGLM='Seven OPEN obligations / 34 restricted controls remain separate and untouched.',
        native='NOT RUN; source/host-free evidence cannot release a control.',
        pending_activation='C12-NATIVE-CLASSIFICATION-01 is a new administrative timing disposition, not a new original obligation.'))


def native_candidate():
    text = (ROOT / FORM).read_text(encoding='utf-8-sig')
    anchor = '            If IsCurrentBinomialGeeFamily() AndAlso Me.grpClassification.Enabled And cbPerformClasification.Checked Then'
    assert text.count(anchor) == 1
    replacement = anchor.replace('If IsCurrent', 'If intent.MeanRequest.Options.Family = RegressionFamily.Binomial AndAlso IsCurrent')
    fixed = text.replace(anchor, replacement)
    patch = ''.join(difflib.unified_diff(text.splitlines(True), fixed.splitlines(True), fromfile='a/' + FORM, tofile='b/' + FORM))
    path = OUT / 'C12-NATIVE-CLASSIFICATION-01.UNAPPLIED.patch'
    if path.exists():
        assert path.read_text() == patch
    else:
        path.write_text(patch, encoding='utf-8', newline='\n')
    candidate = RUN / 'native-candidate/Ui13GEE.vb'
    candidate.parent.mkdir(exist_ok=True)
    data = b'\xef\xbb\xbf' + fixed.encode()
    if candidate.exists():
        assert candidate.read_bytes() == data
    else:
        candidate.write_bytes(data)
    result = subprocess.run(['git', '-c', 'safe.directory=' + ROOT.as_posix(), 'apply', '--check', str(path)], cwd=ROOT, capture_output=True)
    assert result.returncode == 0
    emit('native-patch.json', dict(id='C12-NATIVE-CLASSIFICATION-01', status='UNAPPLIED_GATED',
        source_path=FORM, source_sha256=sha(ROOT / FORM), patch_sha256=sha(path), candidate_sha256=sha(candidate), apply_check_exit=0,
        before='The original post-fit UI-selected binomial family can classify an earlier non-binomial fit when controls change during progress/writing. Windows-only compatibility quarantine preserves this rare path.',
        portable='Common GEE classification correctly requires the actual fitted binomial family. Pure host-free simulated post-fit control change reproduces before/after; real WinForms transition NOT TESTED.',
        after='Guard classification by the captured fitted family as well as the original post-fit UI predicate; leave estimator, controls, normal binomial output and chart/writer order unchanged.',
        affected_controls=['BESHStatNG.Ui13GEE.cbFamily', 'BESHStatNG.Ui13GEE.grpClassification', 'BESHStatNG.Ui13GEE.cbPerformClasification', 'BESHStatNG.Ui13GEE.btCalculate', 'BESHStatNG.Ui13GEE.lblProgress', 'BESHStatNG.Ui13GEE.ProgressBar1'],
        lineage='Exact original IDs and row hashes in restriction-crosswalk.json; no fabricated obligation for option controls without original deferred rows.',
        owner_timing_request='Accept software scope with exact activation deferred until original native pre-change evidence/risk/N authority or a fresh exact-ID timing exception. No native/P release requested.',
        C09_sha256=sha(ROOT / 'evidence/04A09/strict-validation-activation.UNAPPLIED.patch'),
        C11_sha256=sha(ROOT / 'evidence/04A11/C11-NATIVE-CORRECTIONS-01.final.UNAPPLIED.patch'),
        native_executed=False, patch_applied=False, next_owners=['04A.40 GUI/UDF parity', '04A.47 native confirmation', '04A.45-48 native prerequisites']))


if __name__ == '__main__':
    before, changed = source()
    restrictions(before, changed)
    native_candidate()
    print('PASS: source scope, estimator/evidence preservation, 87 GEE controls / 35 OPEN obligations / 34 restrictions, exact dormant patch.')
