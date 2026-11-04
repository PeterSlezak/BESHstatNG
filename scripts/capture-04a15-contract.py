"""04A.15 source/lineage evidence. Historical artifacts are read-only."""
import hashlib
import importlib.util
import json
from pathlib import Path
import subprocess

ROOT = Path(__file__).resolve().parents[1]
RUN = ROOT / 'TestResults/04A-15-20261104-run01'
OUT = ROOT / 'evidence/04A15'
BASE = '0ed7f42a57e781443bf2c3495703a963b4332c47'
FORMS = ['BESHstatNG/src/UI/Ui18MMRM.vb', 'BESHstatNG/src/UI/Ui19LMM.vb']
PAGES = ['BESHStatNG_Help_MkDocs/docs/methods/' + p + '.md' for p in
         ['mmrm/options-and-output', 'mmrm/user-interface', 'lmm/options-and-output', 'lmm/user-interface', 'lmm/random-effects-and-covariance']]
CHANGED = set(FORMS + PAGES + ['BESHStat.Excel.Common/src/Analysis/MixedModelWorkflowPolicy.vb',
    'BESHstatNG/src/DM/WindowsMixedModelWorkflowAdapter.vb', 'BESHStatNG.Test/BESHStatNG.Tests.vbproj'])
spec = importlib.util.spec_from_file_location('contract14', ROOT / 'scripts/capture-04a14-contract.py')
prior = importlib.util.module_from_spec(spec)
spec.loader.exec_module(prior)


def sha(path):
    return hashlib.sha256(Path(path).read_bytes()).hexdigest()


def git(*args):
    return subprocess.check_output(['git', '-c', 'safe.directory=' + ROOT.as_posix(), *args], cwd=ROOT, text=True).strip()


def original(path):
    return subprocess.check_output(['git', '-c', 'safe.directory=' + ROOT.as_posix(), 'show', BASE + ':' + path], cwd=ROOT).decode('utf-8-sig').replace('\r\n', '\n')


def write(name, value):
    (OUT / name).write_text(json.dumps(prior.sanitize(value), ensure_ascii=False, indent=2, allow_nan=False) + '\n', encoding='utf-8', newline='\n')


def source():
    assert git('branch', '--show-current') == 'codex/04A-15'
    assert git('rev-parse', 'HEAD') == git('rev-parse', 'main') == BASE
    assert not git('diff', '--cached', '--name-only')
    assert set(git('diff', '--name-only').splitlines()) == CHANGED
    pins = json.loads((RUN / 'baseline.json').read_bytes())['source_pins']
    protected = {p: h for p, h in pins.items() if p not in CHANGED}
    errors = [p for p, h in protected.items() if sha(ROOT / p) != h]
    assert not errors, errors
    members = {}
    for path, method in zip(FORMS, ['RunMMRMAsync', 'RunLMMAsync']):
        before = prior.get_members(path, original(path))
        current = prior.get_members(path, (ROOT / path).read_text(encoding='utf-8-sig'))
        changed = {n for n in before if before[n] != current[n]}
        assert changed == {method}
        expected = original(path).replace('regression.MixedModelRStructUtils.createMixedModelRStruct(CStr(Me.cbCovarStruct.SelectedItem))',
            'WindowsMixedModelWorkflowAdapter.CreateResidualStructure(CStr(Me.cbCovarStruct.SelectedItem))')
        expected = expected.replace('regression.MixedModelGStructUtils.createMixedModelGStruct(CStr(Me.cbRandomCovarStruct.SelectedItem))',
            'WindowsMixedModelWorkflowAdapter.CreateRandomStructure(CStr(Me.cbRandomCovarStruct.SelectedItem))')
        assert expected == (ROOT / path).read_text(encoding='utf-8-sig'), path
        members[path] = dict(changed={method: dict(before=prior.digest(before[method]), after=prior.digest(current[method]))},
            unchanged={n: prior.digest(v) for n, v in before.items() if n != method})
    new = git('ls-files', '--others', '--exclude-standard').splitlines()
    source_new = {p: sha(ROOT / p) for p in new if p.endswith(('.vb', '.py'))}
    public_protected = {p: h for p, h in protected.items() if
        p.startswith(('BESHStat.Core/src/', 'BESHstatNG/src/ExcelUDFs/')) or
        any(k in p for k in ('Mixed14', 'MixedModelFrontEndHelpers', 'Ui18MMRM.Designer', 'Ui19LMM.Designer', 'MixedModelReportPolicy', '_meta/methods.yml'))}
    write('source-contract.json', dict(base=BASE, branch='codex/04A-15', changed_existing={p: dict(before=pins[p], after=sha(ROOT / p)) for p in sorted(CHANGED)},
        new_sources=source_new, protected_source_pins=public_protected, members=members,
        full_local_protected_count=len(protected), full_local_protected_pass=True,
        form_delta='Only original factory call tokens changed; every other character, method, event binding, writer and cleanup remains identical to main.',
        index_empty=True, source_scope='Core algorithms, UDF exports/handles, Designers, report policy and historical tests unchanged.'))
    registry = json.loads((ROOT / 'evidence/04A14/baseline-registry.json').read_bytes())
    assert len(registry['files']) == 100
    for p, pin in registry['files'].items():
        assert sha(ROOT / p) == pin['sha256'] and (ROOT / p).stat().st_size == pin['bytes']
    crosswalk = json.loads((ROOT / 'evidence/04A14/restriction-crosswalk.json').read_bytes())
    for p, digest in crosswalk['authorities'].items():
        assert sha(ROOT / p) == digest
    write('baseline-and-lineage.json', dict(status='NOT_GOLDEN', baseline_count=100, registry=dict(path='evidence/04A14/baseline-registry.json', sha256=sha(ROOT / 'evidence/04A14/baseline-registry.json')),
        frozen_source_sha256=registry['capture_source_sha256'], captures=registry['files'],
        restriction_crosswalk=dict(path='evidence/04A14/restriction-crosswalk.json', sha256=sha(ROOT / 'evidence/04A14/restriction-crosswalk.json')),
        original_authorities=crosswalk['authorities'], original_controls=177, open_obligations=69, restricted_controls=67,
        exact_ID_policy='All original IDs and row hashes are retained in the referenced byte-identical 04A.14 crosswalk; no newly invented obligations, releases or discharges.',
        populations='Original 1514/802; transferred 239/218; ANOVA/normality 160/141; importer 44; 2x2 three; UiGLM seven/34; GEE 35/34; survival 12/10; mixed 69/67. Overlapping, never additive.',
        dormant=['C09-NATIVE-STRICT-01', 'C11-NATIVE-CORRECTIONS-01', 'C12-NATIVE-CLASSIFICATION-01'],
        open=['C13-LOGRANK-R-01', 'C14-INPUT-FINITE-01'], P='NO', N1='NO GO', N2='NO', C2='UNMET'))
    audited = PAGES + ['BESHStatNG_Help_MkDocs/docs/methods/' + p + '.md' for p in ['mmrm/implementation-details', 'lmm/implementation-details']]
    write('documentation-impact.json', {p: dict(sha256=sha(ROOT / p), disposition='Editorial clarification of unchanged optimizer/order or Windows-only availability behavior.' if p in PAGES else
        'NO CHANGE: algorithms, likelihood, inference, fitted outputs and post-estimation ownership unchanged; see options pages for exact optimizer overrides.') for p in audited})
    print('PASS source boundaries, all protected tracked bytes, 100 frozen captures and original obligation authorities.')


if __name__ == '__main__':
    source()
