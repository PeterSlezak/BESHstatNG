"""04A.14 source/control lineage and reproducible non-native contract checks."""
import csv
import hashlib
import importlib.util
import json
from pathlib import Path
import re
import subprocess

ROOT = Path(__file__).resolve().parents[1]
OUT = ROOT / 'evidence/04A14'
RUN = ROOT / 'TestResults/04A-14-20261103-run01'
BASE = 'b02480a71c1f2de45beb9cb0726a564eae15076b'
FORMS = ['BESHstatNG/src/UI/Ui18MMRM.vb', 'BESHstatNG/src/UI/Ui19LMM.vb']
HELPER = 'BESHstatNG/src/RegModels/MixedModels/MixedModelFrontEndHelpers.vb'
OWNER_PLAN = 'planning/BESHStatNG_CrossPlatform_MasterPlan.md'
PAGES = ['BESHStatNG_Help_MkDocs/docs/methods/' + n + '.md' for n in ['mixed-models-for-repeated-measures-mmrm', 'linear-mixed-models-lmm']]
CHANGED = set(FORMS + PAGES + [HELPER,
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


def sanitize(value):
    if isinstance(value, dict):
        return {k: sanitize(v) for k, v in value.items()}
    if isinstance(value, (list, tuple)):
        return [sanitize(v) for v in value]
    if isinstance(value, str):
        if re.search(r'[A-Za-z]:[\\/]', value):
            while chr(92) * 2 in value:
                value = value.replace(chr(92) * 2, chr(92))
        return prior.sanitize(value)
    return value


def write(name, value):
    (OUT / name).write_text(json.dumps(sanitize(value), indent=2, ensure_ascii=False, allow_nan=False) + '\n', encoding='utf-8', newline='\n')


def source():
    assert git('branch', '--show-current') == 'codex/04A-14'
    assert git('rev-parse', 'HEAD') == git('rev-parse', 'main') == BASE
    assert not git('diff', '--cached', '--name-only')
    assert set(git('diff', '--name-only').splitlines()) == CHANGED | {OWNER_PLAN}
    pins = json.loads((RUN / 'baseline.json').read_bytes())['source_pins']
    protected = {p: h for p, h in pins.items() if p not in CHANGED and p != OWNER_PLAN}
    owner = json.loads((OUT / 'owner-plan-update.json').read_bytes())
    assert sha(ROOT / OWNER_PLAN) == owner['observed_sha256']
    assert pins[OWNER_PLAN] == owner['before_sha256']
    errors = [p for p, h in protected.items() if sha(ROOT / p) != h]
    assert not errors, errors
    members = {}
    expected = {
        FORMS[0]: {'AppendMMRMEstimatedMeanTables', 'AppendMMRMReferenceGridTables', 'ApplyMMRMCovarianceOptimizerSelections', 'BuildMMRMReferenceGridSpec', 'IsMMRMReferenceGridModeSelected', 'ResolveDirectedGroupPair', 'RunMMRMAsync', 'SelectedMultiplicityAdjustment', 'SelectedReferenceGridWeighting', 'UseObservedMeanReferenceCovariates', 'ValidateInputs', 'WriteMMRMDataSheet', 'WriteMMRMModelSheet', 'WriteMMRMResults'},
        FORMS[1]: {'AppendLMMClassInfoTable', 'ApplyLMMCovarianceOptimizerSelections', 'PromoteRandomCovarianceForGeneralRandomEffects', 'PromoteRandomCovarianceForMultipleRandomEffects', 'RunLMMAsync', 'ValidateInputs', 'WriteLMMDataSheet', 'WriteLMMModelSheet', 'WriteLMMResults'},
        HELPER: {'ParseFitMethod', 'ParseInferenceMethod', 'ParseCovarianceOptimizerMode', 'ParseCovarianceGradientMode', 'NormalizeToken', 'AsString'}}
    for path in FORMS + [HELPER]:
        before = get_members(path, original(path))
        after = get_members(path, (ROOT / path).read_text(encoding='utf-8-sig'))
        changed = {n for n in before if before[n] != after[n]}
        assert set(before) <= set(after)
        assert set(after) - set(before) == ({'CaptureMMRMReportOptions'} if path == FORMS[0] else set())
        assert changed == expected[path], (path, changed)
        members[path] = dict(changed={n: dict(before=digest(before[n]), after=digest(after[n])) for n in sorted(changed)},
            added={n: digest(after[n]) for n in set(after) - set(before)},
            unchanged={n: digest(before[n]) for n in before if n not in changed})
        if path in FORMS:
            for pattern in [r'WriteRes\.\w+\(', r'^.*ActiveSheet\.name.*$', r'^.*MsgBox\(.*$']:
                assert re.findall(pattern, original(path), re.M) == re.findall(pattern, (ROOT / path).read_text(encoding='utf-8-sig'), re.M), (path, pattern)
            run = after['RunMMRMAsync' if path == FORMS[0] else 'RunLMMAsync']
            assert run.count('model.Fit()') == 1
            assert 'workflow.CreateAcquiredCoreRequest(' in run
            assert 'Await Task.Run(Function() model.Fit())' in run
            assert 'workflow = Nothing' in run
    patches = ['evidence/04A09/strict-validation-activation.UNAPPLIED.patch',
        'evidence/04A11/C11-NATIVE-CORRECTIONS-01.final.UNAPPLIED.patch', 'evidence/04A12/C12-NATIVE-CLASSIFICATION-01.UNAPPLIED.patch']
    for patch in patches:
        assert sha(ROOT / patch) == pins[patch]
        assert subprocess.run(['git', '-c', 'safe.directory=' + ROOT.as_posix(), 'apply', '--check', patch], cwd=ROOT, capture_output=True).returncode == 0
    write('source-delta.json', dict(base=BASE, branch='codex/04A-14', changed_existing={p: dict(before=pins[p], after=sha(ROOT / p)) for p in sorted(CHANGED)},
        members=members, protected_count=len(protected), protected_map_sha256=digest(protected),
        private_pin_map='LOCAL_BASELINE_ALIAS:source_pins; actual historical filename keys remain in ignored baseline.json, never replaced with imaginary paths.',
        unaffected='Every other baseline tracked/docs byte pin verified, including Core estimators, UDF exports, Designers, GEE and C09/C11/C12 candidates; no hash equivalence invented for changed forms.',
        owner_plan=owner, writer='Original writer operation sequence, sheet names, messages, async/cancel/interrupt/cleanup members preserved. Typed report tables consumed by shipping writers.',
        dormant_patches={p: dict(sha256=pins[p], apply_check_exit=0, applied=False) for p in patches},
        native_executed=False, staged=False, published=False))
    return members


def restrictions(members):
    with (ROOT / CONTROLS).open(encoding='utf-8-sig') as stream:
        rows = list(csv.DictReader(stream))
    ledger = json.loads((ROOT / LEDGER).read_bytes())
    affected = {c for r in ledger if r.get('affected_live_golden_blocked') for c in r.get('linked_control_ids', [])}
    selected, summaries = [], {}
    for form, path in [('Ui18MMRM', FORMS[0]), ('Ui19LMM', FORMS[1])]:
        before = prior.members(original(path))
        after = prior.members((ROOT / path).read_text(encoding='utf-8-sig'))
        controls = [r for r in rows if r['control_id'].startswith('BESHStatNG.' + form + '.')]
        for row in controls:
            cid = row['control_id']
            obligation = next((r for r in ledger if r['original_id'] == cid), None)
            changed = [n for n in list(members[path]['changed']) + list(members[path]['added']) if re.search(r'(?<![\w.])(?:Me\.)?' + re.escape(cid.split('.')[-1]) + r'\b', before.get(n, '') + '\n' + after[n], re.I)]
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
    assert selected and all(r['retained_status'] in ['OPEN', 'NO ORIGINAL DEFERRED OBLIGATION ROW'] for r in selected)
    write('restriction-crosswalk.json', dict(authorities={p: sha(ROOT / p) for p in [LEDGER, CONTROLS,
        'docs/architecture/04A02/04A02-final-amended-acceptance-PROPOSED.json']}, forms=summaries, controls=selected,
        populations='1514 original obligations / 802 restricted controls; accepted 04A.02 transferred 239/218 and overlapping case subsets. Never add overlapping sets.',
        released=0, discharged=0, P='NO', N1='NO GO', N2='NO', C2='UNMET',
        directly_touched_without_obligation=[r['control_id'] for r in selected if r['source_associated_changed_methods'] and r['obligation_id'] is None],
        directly_touched_with_obligation=[r['obligation_id'] for r in selected if r['source_associated_changed_methods'] and r['obligation_id']],
        native='NOT RUN / NOT AUTHORIZED. Original whole-file pins remain historical; source-delta supplies successor provenance. No timing waiver requested for this behavior-preserving extraction.'))
    return summaries


def baselines():
    registry = json.loads((OUT / 'baseline-registry.json').read_bytes())
    assert len(registry['files']) == 100
    for path, pin in registry['files'].items():
        assert sha(ROOT / path) == pin['sha256']
        assert (ROOT / path).stat().st_size == pin['bytes']
    assert sha(ROOT / 'BESHStat.Excel.Common.Tests/src/Mixed14Baselines.vb') == registry['capture_source_sha256']


def get_members(path, text):
    if path == HELPER:
        text = re.sub(r'^    ', '', text, flags=re.M)
    return prior.members(text)


if __name__ == '__main__':
    baselines()
    result = restrictions(source())
    print({form: {k: v for k, v in value.items() if k != 'without_original_obligation'} for form, value in result.items()})
    print('PASS: source boundaries, exact original controls/OPEN obligations, frozen captures, dormant patches.')
