"""04A.13 source/control lineage and reproducible non-native contract checks."""
import csv
import hashlib
import importlib.util
import json
from pathlib import Path
import re
import subprocess

ROOT = Path(__file__).resolve().parents[1]
OUT = ROOT / 'evidence/04A13'
RUN = ROOT / 'TestResults/04A-13-20261103-run01'
BASE = 'f7d1c693eadda871c82807947922b2fa8596b87b'
FORMS = ['BESHstatNG/src/UI/Ui4KMandLogRank.vb', 'BESHstatNG/src/UI/Ui4Cox.vb']
PAGES = ['BESHStatNG_Help_MkDocs/docs/methods/' + n + '.md' for n in ['kaplan-meier-plot', 'logrank-test', 'cox-regression']]
CHANGED = set(FORMS + PAGES + ['BESHstatNG/src/Graphics/KaplanMeierPlotExcel.vb',
    'BESHstatNG/BESHStatNG.vbproj', 'BESHStatNG.Test/BESHStatNG.Tests.vbproj'])
LEDGER = 'docs/architecture/WindowsMigrationLedger/04A01-approved-deferred-obligations.json'
CONTROLS = 'docs/architecture/WindowsMigrationLedger/ui-controls.csv'
spec = importlib.util.spec_from_file_location('source09', ROOT / 'scripts/capture-04a09-contract.py')
prior = importlib.util.module_from_spec(spec)
spec.loader.exec_module(prior)


def sha(path):
    return hashlib.sha256(Path(path).read_bytes()).hexdigest()


def digest(value):
    return hashlib.sha256(json.dumps(value, sort_keys=True, ensure_ascii=False).encode()).hexdigest()


def git(*args):
    return subprocess.check_output(['git', '-c', 'safe.directory=' + ROOT.as_posix(), *args], cwd=ROOT).decode().strip()


def original(path):
    return subprocess.check_output(['git', '-c', 'safe.directory=' + ROOT.as_posix(), 'show', BASE + ':' + path], cwd=ROOT).decode('utf-8-sig').replace('\r\n', '\n')


def write(name, value):
    (OUT / name).write_text(json.dumps(prior.sanitize(value), indent=2, ensure_ascii=False, allow_nan=False) + '\n', encoding='utf-8', newline='\n')


def source():
    assert git('branch', '--show-current') == 'codex/04A-13'
    assert git('rev-parse', 'HEAD') == git('rev-parse', 'main') == BASE
    assert not git('diff', '--cached', '--name-only')
    assert set(git('diff', '--name-only').splitlines()) == CHANGED
    pins = json.loads((RUN / 'baseline.json').read_bytes())['source_pins']
    protected = {p: h for p, h in pins.items() if p not in CHANGED}
    errors = [p for p, h in protected.items() if sha(ROOT / p) != h]
    assert not errors, errors
    members = {}
    for path in FORMS:
        before = prior.members(original(path))
        after = prior.members((ROOT / path).read_text(encoding='utf-8-sig'))
        changed = {n for n in before if before[n] != after[n]}
        assert set(before) == set(after)
        expected = {'RunKM'} if path == FORMS[0] else {'btCalculate_Click', 'BuildExpandedCoxInputs', 'BuildExpandedSurvivalRecords', 'ValidateExpandedInitialValuesCount'}
        assert changed == expected, (path, changed)
        members[path] = dict(changed={n: dict(before=digest(before[n]), after=digest(after[n])) for n in sorted(changed)},
            unchanged={n: digest(before[n]) for n in before if n not in changed})
        assert re.findall(r'WriteRes\.\w+\(', original(path)) == re.findall(r'WriteRes\.\w+\(', (ROOT / path).read_text(encoding='utf-8-sig'))
    renderer = 'BESHstatNG/src/Graphics/KaplanMeierPlotExcel.vb'
    anchor = '            Dim noGroups As Integer = plotData.GroupLabels.Length'
    assert original(renderer).split(anchor)[1] == (ROOT / renderer).read_text(encoding='utf-8-sig').split(anchor)[1]
    cox = (ROOT / FORMS[1]).read_text(encoding='utf-8-sig')
    old = original(FORMS[1])
    for pattern in [r'^.*strCellAddress.*$', r'^.*SeriesCollection.*$', r'^.*\.AddComment.*$', r'^.*\.text\(.*$', r'^.*ChartObjects.*$']:
        assert re.findall(pattern, old, re.M) == re.findall(pattern, cox, re.M), pattern
    assert re.findall(r'ActiveSheet\.name = "([^"]+)"', old) == re.findall(r'ActiveSheet\.name = "([^"]+)"', cox) == ['Cox Regression', 'Adjusted Curves', 'Residuals']
    patches = ['evidence/04A09/strict-validation-activation.UNAPPLIED.patch',
        'evidence/04A11/C11-NATIVE-CORRECTIONS-01.final.UNAPPLIED.patch', 'evidence/04A12/C12-NATIVE-CLASSIFICATION-01.UNAPPLIED.patch']
    for patch in patches:
        assert sha(ROOT / patch) == pins[patch]
        assert subprocess.run(['git', '-c', 'safe.directory=' + ROOT.as_posix(), 'apply', '--check', patch], cwd=ROOT, capture_output=True).returncode == 0
    write('source-delta.json', dict(base=BASE, branch='codex/04A-13', changed_existing={p: dict(before=pins[p], after=sha(ROOT / p)) for p in sorted(CHANGED)},
        members=members, protected_count=len(protected), protected_map_sha256=digest(protected),
        private_pin_map='LOCAL_BASELINE_ALIAS:source_pins; actual historical filename keys remain in ignored baseline.json, never replaced with imaginary paths.',
        unaffected='Every other baseline tracked/docs byte pin verified, including Core estimators, UDF exports, Designers, GEE and C09/C11/C12 candidates; no hash equivalence invented for changed forms.',
        renderer='Original KM native body from noGroups onward is byte-equal after LF normalization. Payload overload only; Cox renderer unchanged.',
        dormant_patches={p: dict(sha256=pins[p], apply_check_exit=0, applied=False) for p in patches},
        native_executed=False, staged=False, published=False))
    return members


def restrictions(members):
    with (ROOT / CONTROLS).open(encoding='utf-8-sig') as stream:
        rows = list(csv.DictReader(stream))
    ledger = json.loads((ROOT / LEDGER).read_bytes())
    affected = {c for r in ledger if r.get('affected_live_golden_blocked') for c in r.get('linked_control_ids', [])}
    selected, summaries = [], {}
    for form, path in [('Ui4KMandLogRank', FORMS[0]), ('Ui4Cox', FORMS[1])]:
        before = prior.members(original(path))
        after = prior.members((ROOT / path).read_text(encoding='utf-8-sig'))
        controls = [r for r in rows if r['control_id'].startswith('BESHStatNG.' + form + '.')]
        for row in controls:
            cid = row['control_id']
            obligation = next((r for r in ledger if r['original_id'] == cid), None)
            changed = [n for n in members[path]['changed'] if re.search(r'(?<![\w.])(?:Me\.)?' + re.escape(cid.split('.')[-1]) + r'\b', before[n] + '\n' + after[n], re.I)]
            item = dict(control_id=cid, control_row_sha256=digest(row), declaration_path=row['path'], declaration_line=row['line'],
                original_restricted=cid in affected, source_associated_changed_methods=changed,
                obligation_id=obligation['obligation_id'] if obligation else None,
                obligation_row_sha256=digest(obligation) if obligation else None,
                retained_status='OPEN' if obligation else 'NO ORIGINAL DEFERRED OBLIGATION ROW',
                source_record_path=obligation.get('source_record_path') if obligation else None,
                source_record_sha256=obligation.get('source_record_sha256') if obligation else None)
            if obligation:
                ev = obligation['source_evidence']
                item.update(original_control_sha256=ev['original_control_sha256'], original_source_pins={k: ev[k] for k in
                    ['codebehind_path', 'codebehind_source_sha256', 'declaration_path', 'designer_source_sha256']},
                    member_body_pins=ev['member_body_pins'], owner_roles=obligation['owner_roles'], dependent_batches=obligation['dependent_batches'],
                    named_fixture=obligation['named_fixture'], verification_status=obligation['verification_status'],
                    owner_decision_status=obligation['owner_decision_status'], capture_authorized=obligation['capture_authorized'],
                    full_contract_newly_qualified=obligation['full_contract_newly_qualified'])
            selected.append(item)
        subset = [r for r in selected if r['control_id'].startswith('BESHStatNG.' + form + '.')]
        summaries[form] = dict(controls=len(subset), obligations=sum(r['obligation_id'] is not None for r in subset), restricted=sum(r['original_restricted'] for r in subset),
            touched_source_controls=sum(bool(r['source_associated_changed_methods']) for r in subset),
            without_original_obligation=[r['control_id'] for r in subset if r['obligation_id'] is None])
    assert len(selected) == 103 and sum(r['obligation_id'] is not None for r in selected) == 12
    write('restriction-crosswalk.json', dict(authorities={p: sha(ROOT / p) for p in [LEDGER, CONTROLS,
        'docs/architecture/04A02/04A02-final-amended-acceptance-PROPOSED.json']}, forms=summaries, controls=selected,
        populations='1514 original obligations / 802 restricted controls; accepted 04A.02 transferred 239/218 and overlapping case subsets. Never add overlapping sets.',
        released=0, discharged=0, P='NO', N1='NO GO', N2='NO', C2='UNMET',
        native='NOT RUN / NOT AUTHORIZED. Original whole-file pins remain historical; source-delta supplies successor provenance. No timing waiver requested for this behavior-preserving extraction.'))
    return summaries


def baselines():
    files = {p.relative_to(ROOT).as_posix(): dict(sha256=sha(p), bytes=p.stat().st_size) for p in sorted((OUT / 'baselines').glob('*.txt'))}
    assert len(files) == 148
    registry = OUT / 'baseline-registry.json'
    value = dict(base=BASE, cases_per_runtime=37, cultures=['invariant', 'sk-SK'], runtimes=['net10', 'net48'], files=files,
        provenance='Captured from unchanged merged Core before any production edit, with the original shipping calculation order. Initial fixture compile errors corrected before capture; no historical fixture assertion changed.',
        tolerance='max(1e-9,abs(expected)*1e-9), exact CLR/labels/order/metadata/nulls and failure types; only computational-time text masked.', status='NOT_GOLDEN')
    if registry.exists():
        assert json.loads(registry.read_bytes()) == value, 'Frozen new capture drift'
    else:
        write('baseline-registry.json', value)


if __name__ == '__main__':
    baselines()
    result = restrictions(source())
    print({form: {k: v for k, v in value.items() if k != 'without_original_obligation'} for form, value in result.items()})
    print('PASS: source boundaries, exact original controls/OPEN obligations, frozen captures, dormant patches.')
