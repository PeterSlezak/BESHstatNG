"""04A.16 immutable prechange audit and successor source evidence."""
import argparse
import hashlib
import importlib.util
import json
from pathlib import Path
import re
import subprocess

ROOT = Path(__file__).resolve().parents[1]
RUN = ROOT / 'TestResults/04A-16-20261104-run01'
OUT = ROOT / 'evidence/04A16'
BASE = '60bb832b8e10c94166b86cf3f661cc87f61eb7f3'
FORMS = ['BESHstatNG/src/UI/Ui18MMRM.vb', 'BESHstatNG/src/UI/Ui19LMM.vb']
PAGES = ['BESHStatNG_Help_MkDocs/docs/methods/' + p + '.md' for p in ['mmrm/lsmeans-and-contrasts', 'mmrm/options-and-output', 'lmm/options-and-output']]
CHANGED = set(FORMS + PAGES + ['BESHStat.Excel.Common/src/Results/MixedModelReportPolicy.vb',
    'BESHstatNG/src/DM/WindowsMixedModelWorkflowAdapter.vb', 'BESHStatNG.Test/BESHStatNG.Tests.vbproj'])
METHODS = {
    FORMS[0]: {'AddClassLevelRow', 'AddReferenceGridContinuousCovariates', 'AddReferenceGridMarginalFactors', 'AppendMMRMEstimatedMeanTables',
        'AppendMMRMReferenceGridTables', 'BuildMMRMClassLevelInformationTable', 'BuildMMRMDifferenceInChangeFromBaselineTableControlled',
        'BuildMMRMReferenceGridGroupContrastsTable', 'BuildMMRMVisitGroupDifferencesTableControlled', 'CaptureMMRMReportOptions',
        'GetMMRMCategoricalFactorBaseKeys', 'GetMMRMContinuousMainEffectBaseKeys', 'ResolveSelectedBaselineVisit', 'ResolveSelectedComparisonLevel',
        'ResolveSelectedControlLevel', 'RunMMRMAsync', 'TryChooseMMRMGroupingVariable', 'TryResolveMMRMGroupingVariableFromControls', 'WriteMMRMModelSheet', 'WriteMMRMResults'},
    FORMS[1]: {'AppendClassInfoRows', 'AppendLMMClassInfoTable', 'GetCategoricalMainEffectBaseKeys', 'RunLMMAsync', 'WriteLMMModelSheet', 'WriteLMMResults'}}
spec = importlib.util.spec_from_file_location('contract15', ROOT / 'scripts/capture-04a15-contract.py')
prior = importlib.util.module_from_spec(spec)
spec.loader.exec_module(prior)


def sha(path):
    return hashlib.sha256(Path(path).read_bytes()).hexdigest()


def git(*args):
    return prior.git(*args)


def original(path):
    return subprocess.check_output(['git', '-c', 'safe.directory=' + ROOT.as_posix(), 'show', BASE + ':' + path], cwd=ROOT).decode('utf-8-sig').replace('\r\n', '\n')


def write(name, value):
    (OUT / name).write_text(json.dumps(prior.prior.sanitize(value), ensure_ascii=False, indent=2, allow_nan=False) + '\n', encoding='utf-8', newline='\n')


def members(path, text):
    return prior.prior.get_members(path, text)


def audit():
    assert not git('diff', '--name-only') and not git('diff', '--cached', '--name-only')
    assert git('rev-parse', 'HEAD') == git('rev-parse', 'main') == BASE
    paths = FORMS + ['BESHStat.Excel.Common/src/Results/MixedModelReportPolicy.vb']
    paths += [p.relative_to(ROOT).as_posix() for p in (ROOT / 'BESHStat.Core/src/RegModels/MixedModels').glob('*.vb')]
    inventory = {}
    for path in paths:
        text = original(path)
        inventory[path] = dict(sha256=sha(ROOT / path), methods={name: dict(sha256=prior.prior.digest(body),
            first_line=text[:text.index(body)].count('\n') + 1, lines=body.count('\n') + 1) for name, body in members(path, text).items()})
    write('prechange-method-inventory.json', dict(base=BASE, source=inventory,
        report_order='Fixed effects; available KR term tests; Core additional tables then Windows supplemental class/means/contrasts; covariance parameters and selected G/R/BLUPs; fit statistics; optional performance/support/restart/KR diagnostics; convergence; optional optimizer trace.',
        warning_policy='Optional MMRM factories log failures and continue. Reference-spec/row creation failure escapes to the original outer catch. LMM class acquisition failure omits its whole optional table. No new Windows refusal.',
        identities='Post-estimation X/visit/group arrays follow acquisition order. Physical source RowIds are separate from Core subject/visit permutations.'))
    source = original(FORMS[0])
    selected = ['ResolveSelectedBaselineVisit', 'ResolveSelectedControlLevel', 'ResolveSelectedComparisonLevel',
        'BuildMMRMVisitGroupDifferencesTableControlled', 'BuildMMRMDifferenceInChangeFromBaselineTableControlled', 'BuildMMRMReferenceGridGroupContrastsTable']
    transformed = []
    for name in selected:
        text = members(FORMS[0], source)[name].replace('Private Function', 'Public Function', 1)
        text = text.replace('SelectedComboText(Me.cbMMRMBaselineVisit, MMRM_BASELINE_SMALLEST)', 'SelectedText(_options.BaselineSelection, MMRM_BASELINE_SMALLEST)')
        text = text.replace('SelectedComboText(Me.cbMMRMControlLevel, MMRM_CONTROL_FIRST)', 'SelectedText(_options.ControlSelection, MMRM_CONTROL_FIRST)')
        text = re.sub(r'        Dim cb = Me.cbMMRMComparisonLevel\n        Dim selectedText As String = If\(cb Is Nothing OrElse cb.SelectedItem Is Nothing,\n\s*"\(Auto\)",\n\s*CStr\(cb.SelectedItem\).Trim\(\)\)',
            '        Dim selectedText As String = If(_options.ComparisonSelection Is Nothing, "(Auto)", _options.ComparisonSelection.Trim())', text)
        assert 'Me.' not in text
        transformed.append(text)
    constants = '\n'.join(re.findall(r'^    Private Const MMRM_.*$', source, re.M))
    support = '''Option Explicit On
Option Strict On
Imports System
Imports System.Collections.Generic
Imports BESHStatNG
Imports BESHStatNG.ExcelCommon
Imports regression = BESHStatNG.regression

' Frozen prechange form methods. Only control reads/visibility are adapted;
' original Core calls, parser order, error strings and comparison branches remain.
Public NotInheritable Class LegacyMixed16Selection
    Private ReadOnly _options As MixedModelReportOptions
    Public Sub New(options As MixedModelReportOptions)
        _options = options.Copy()
    End Sub
    Private Shared Function SelectedText(value As String, fallback As String) As String
        Return If(String.IsNullOrWhiteSpace(value), fallback, value)
    End Function
'''
    support += constants + '\n' + '\n'.join(transformed) + '\nEnd Class\n'
    path = ROOT / 'BESHStat.Excel.Common.Tests/src/LegacyMixed16Selection.vb'
    with path.open('x', encoding='utf-8', newline='\n') as stream:
        stream.write(support)
    write('legacy-selection-provenance.json', dict(base=BASE, original_path=FORMS[0], original_sha256=sha(ROOT / FORMS[0]),
        original_methods={n: prior.prior.digest(members(FORMS[0], source)[n]) for n in selected},
        derivative_path=path.relative_to(ROOT).as_posix(), derivative_sha256=sha(path),
        adaptations='Private visibility made public; three existing ComboBox text reads replaced by copied option strings. No Core operation or fallback branch changed. Test-only compatibility oracle, not independent statistical validation.'))
    print('Prechange method inventory and frozen selection oracle recorded.')


def source():
    assert git('branch', '--show-current') == 'codex/04A-16'
    assert git('rev-parse', 'HEAD') == git('rev-parse', 'main') == BASE
    assert not git('diff', '--cached', '--name-only')
    assert set(git('diff', '--name-only').splitlines()) == CHANGED
    pins = json.loads((RUN / 'baseline.json').read_bytes())['source_pins']
    protected = {p: h for p, h in pins.items() if p not in CHANGED}
    errors = [p for p, h in protected.items() if sha(ROOT / p) != h]
    assert not errors, errors
    method_map = {}
    for path in FORMS:
        before = members(path, original(path))
        after = members(path, (ROOT / path).read_text(encoding='utf-8-sig'))
        assert set(before) == set(after)
        changed = {n for n in before if before[n] != after[n]}
        assert changed == METHODS[path], changed
        method_map[path] = dict(changed={n: dict(before=prior.prior.digest(before[n]), after=prior.prior.digest(after[n])) for n in sorted(changed)},
            unchanged={n: prior.prior.digest(body) for n, body in before.items() if n not in changed})
    provenance = json.loads((OUT / 'legacy-selection-provenance.json').read_bytes())
    assert sha(ROOT / provenance['derivative_path']) == provenance['derivative_sha256']
    new_sources = {p: sha(ROOT / p) for p in git('ls-files', '--others', '--exclude-standard').splitlines() if p.endswith(('.py', '.vb'))}
    public_protected = {p: h for p, h in protected.items() if p.startswith(('BESHStat.Core/src/', 'BESHstatNG/src/ExcelUDFs/')) or
        any(k in p for k in ('Mixed14', 'Mixed15', 'MixedModelFrontEndHelpers', 'Ui18MMRM.Designer', 'Ui19LMM.Designer', '_meta/methods.yml'))}
    write('source-contract.json', dict(base=BASE, branch='codex/04A-16', changed_existing={p: dict(before=pins[p], after=sha(ROOT / p)) for p in sorted(CHANGED)},
        new_sources=new_sources, protected_source_pins=public_protected, members=method_map, full_local_protected_count=len(protected), full_local_protected_pass=True,
        index_empty=True, source_scope='Only post-fit selection/orchestration, optional class construction and detached diagnostics/report metadata changed. Core numerics/factories, UDF exports/handles, Designers, acquisition/progress/cleanup and old tests/evidence remain unchanged.'))
    registry = json.loads((ROOT / 'evidence/04A14/baseline-registry.json').read_bytes())
    for p, pin in registry['files'].items():
        assert sha(ROOT / p) == pin['sha256'] and (ROOT / p).stat().st_size == pin['bytes']
    crosswalk = json.loads((ROOT / 'evidence/04A14/restriction-crosswalk.json').read_bytes())
    for p, h in crosswalk['authorities'].items():
        assert sha(ROOT / p) == h
    retained = {p: dict(sha256=sha(ROOT / p), bytes=(ROOT / p).stat().st_size) for p in pins if p.startswith(('evidence/04A14/', 'evidence/04A15/'))}
    fit15 = json.loads((ROOT / 'evidence/04A15/fit-comparisons-v1.json').read_bytes())
    write('retained-evidence.json', dict(status='NOT_GOLDEN', captures=registry['files'], retained=retained,
        original_controls=177, open_obligations=69, restricted_controls=67, crosswalk=dict(path='evidence/04A14/restriction-crosswalk.json', sha256=sha(ROOT / 'evidence/04A14/restriction-crosswalk.json')),
        original_authorities=crosswalk['authorities'], batch15_records=len(fit15['records']), batch15_statuses=fit15['statuses'],
        restrictions='Original 1514/802; transferred 239/218; ANOVA/normality 160/141; importer 44; 2x2 three; UiGLM seven/34; GEE 35/34; survival 12/10; mixed 69/67. Overlapping, never additive. Zero releases/discharges.',
        dormant=['C09-NATIVE-STRICT-01', 'C11-NATIVE-CORRECTIONS-01', 'C12-NATIVE-CLASSIFICATION-01'], open=['C13-LOGRANK-R-01', 'C14-INPUT-FINITE-01'], P='NO', N1='NO GO', N2='NO', C2='UNMET'))
    audited = ['BESHStatNG_Help_MkDocs/docs/methods/' + p + '.md' for p in ['mmrm/lsmeans-and-contrasts', 'mmrm/options-and-output', 'mmrm/user-interface', 'mmrm/implementation-details', 'mmrm/examples', 'lmm/options-and-output', 'lmm/user-interface', 'lmm/implementation-details', 'lmm/examples']]
    write('documentation-impact.json', {p: dict(sha256=sha(ROOT / p), disposition='Editorial clarification of unchanged table families, multiplicity scope, limits or missing LMM/Office GUI availability.' if p in PAGES else 'NO CHANGE: controls/screenshots, numerical algorithms and example values are unchanged. Availability notices from prior accepted batches remain truthful.') for p in audited})
    print('PASS exact post-fit method scope; protected tracked bytes; frozen captures/04A.15 ledger/lineage.')


if __name__ == '__main__':
    parser = argparse.ArgumentParser()
    parser.add_argument('action', choices=['audit', 'source'])
    globals()[parser.parse_args().action]()
