"""04A.13 successors preserve original assertions, bytes and native restrictions."""
import csv
import hashlib
import importlib.util
import json
from pathlib import Path
import re
import subprocess
import unittest

ROOT = Path(__file__).resolve().parents[2]


def module(name, filename):
    spec = importlib.util.spec_from_file_location(name, ROOT / 'scripts' / filename)
    result = importlib.util.module_from_spec(spec)
    spec.loader.exec_module(result)
    return result


c = module('survival13contract', 'capture-04a13-contract.py')


class Survival13SourceContracts(unittest.TestCase):
    def test_local_main_ancestry_empty_index_and_dedicated_branch(self):
        self.assertEqual('codex/04A-13', c.git('branch', '--show-current'))
        self.assertEqual(c.BASE, c.git('rev-parse', 'HEAD'))
        self.assertEqual(c.BASE, c.git('rev-parse', 'main'))
        self.assertEqual('', c.git('diff', '--cached', '--name-only'))
        self.assertEqual(c.BASE, c.git('merge-base', 'HEAD', 'main'))

    def test_all_prechange_protected_bytes_except_exact_approved_scope(self):
        pins = json.loads((c.RUN / 'baseline.json').read_bytes())['source_pins']
        errors = [p for p, h in pins.items() if p not in c.CHANGED and c.sha(ROOT / p) != h]
        self.assertEqual([], errors)
        self.assertEqual(c.CHANGED, set(c.git('diff', '--name-only').splitlines()))

    def test_successor_pins_and_members_reconcile_to_actual_forms(self):
        receipt = json.loads((c.OUT / 'source-delta.json').read_bytes())
        for path, pair in receipt['changed_existing'].items():
            self.assertEqual(c.sha(ROOT / path), pair['after'], path)
        for path, members in receipt['members'].items():
            before = c.prior.members(c.original(path))
            current = c.prior.members((ROOT / path).read_text(encoding='utf-8-sig'))
            for name, pin in members['unchanged'].items():
                self.assertEqual(c.digest(before[name]), pin)
                self.assertEqual(before[name], current[name])

    def test_shipping_km_handler_actually_uses_workflow_reports_and_payload(self):
        source = (ROOT / c.FORMS[0]).read_text(encoding='utf-8-sig')
        for token in ['WindowsSurvivalWorkflowAdapter.Capture(', 'intent.ExecuteAcquiredRecords(SurvD)',
                      'SurvivalReportPolicy.Compose(intent, LRKM.wrapResults())', 'report.LegacyTables()',
                      'SurvivalReportPolicy.Plot(', 'KaplanMeierPlotExcel.Plot(plot.ToCoreData()',
                      'ExcelDnaDataImporter.ImportInto(byIdData, refFinal, True, CharCols)']:
            self.assertIn(token, source)
        self.assertIn('Me.RunKM(data)', c.prior.members(source)['RunLogrank'])
        self.assertIn('HelpTopic.KaplanMeierPlot', source)
        self.assertIn('HelpTopic.LogrankTest', source)

    def test_shipping_cox_handler_uses_one_fit_and_actual_common_projections(self):
        source = (ROOT / c.FORMS[1]).read_text(encoding='utf-8-sig')
        handler = c.prior.members(source)['btCalculate_Click']
        for token in ['WindowsCoxWorkflowAdapter.Capture(', 'intent.CreateAcquiredModel(fitRecords)',
                      'CoxReportPolicy.Compose(intent, cox.wrapResults(', 'report.Report.LegacyTables()',
                      'CoxReportPolicy.Curve(', 'curve.StepValues', 'CoxReportPolicy.Residuals(intent, cox.wrapResiduals())',
                      'residuals.RowIds', 'residuals.Predictors']:
            self.assertIn(token, source)
        self.assertEqual(1, handler.count('cox.Fit('))
        self.assertIn('New AppInfrastructure.WinFormsProgressReporter', handler)
        self.assertIn('CoxWorkflowPolicy.BuildExpandedRecords(', source)
        self.assertIn('CoxWorkflowPolicy.InitialValuesCountMatches(vals, expectedCount)', source)
        self.assertIn('CoxWorkflowPolicy.OmitCategoricalReference', source)
        self.assertNotIn('intent.Validation.IsValid', source)

    def test_original_chart_renderer_formula_comment_and_series_indexing_preserved(self):
        path = 'BESHstatNG/src/Graphics/KaplanMeierPlotExcel.vb'
        anchor = '            Dim noGroups As Integer = plotData.GroupLabels.Length'
        self.assertEqual(c.original(path).split(anchor)[1], (ROOT / path).read_text(encoding='utf-8-sig').split(anchor)[1])
        original = c.original(c.FORMS[1])
        current = (ROOT / c.FORMS[1]).read_text(encoding='utf-8-sig')
        for pattern in [r'^.*strCellAddress.*$', r'^.*SeriesCollection.*$', r'^.*\.text\(.*$']:
            self.assertEqual(re.findall(pattern, original, re.M), re.findall(pattern, current, re.M))

    def test_original_survival_control_obligations_and_row_hashes_are_exact(self):
        with (ROOT / c.CONTROLS).open(encoding='utf-8-sig') as stream:
            rows = list(csv.DictReader(stream))
        ledger = json.loads((ROOT / c.LEDGER).read_bytes())
        receipt = json.loads((c.OUT / 'restriction-crosswalk.json').read_bytes())
        self.assertEqual(103, len(receipt['controls']))
        self.assertEqual(12, sum(r['obligation_id'] is not None for r in receipt['controls']))
        self.assertEqual(10, sum(r['original_restricted'] for r in receipt['controls']))
        for row in receipt['controls']:
            original = next(r for r in rows if r['control_id'] == row['control_id'])
            self.assertEqual(c.digest(original), row['control_row_sha256'])
            if row['obligation_id']:
                obligation = next(r for r in ledger if r['obligation_id'] == row['obligation_id'])
                self.assertEqual(c.digest(obligation), row['obligation_row_sha256'])
                self.assertEqual('OPEN', row['retained_status'])
                self.assertFalse(row['capture_authorized'])
                self.assertEqual(obligation['source_evidence']['original_control_sha256'], row['original_control_sha256'])
        self.assertEqual(0, receipt['released'])
        self.assertEqual(0, receipt['discharged'])

    def test_residual_plot_is_disabled_unwired_and_not_accidentally_implemented(self):
        designer = (ROOT / 'BESHstatNG/src/UI/Ui4Cox.Designer.vb').read_text(encoding='utf-8-sig')
        source = (ROOT / c.FORMS[1]).read_text(encoding='utf-8-sig')
        self.assertIn('Me.ckResidualPlots.Enabled = False', designer)
        self.assertNotIn('ckResidualPlots', source)
        receipt = json.loads((c.OUT / 'restriction-crosswalk.json').read_bytes())
        row = next(r for r in receipt['controls'] if r['control_id'] == 'BESHStatNG.Ui4Cox.ckResidualPlots')
        self.assertEqual('control:BESHStatNG.Ui4Cox.ckResidualPlots', row['obligation_id'])
        self.assertEqual('OPEN', row['retained_status'])

    def test_frozen_captures_cover_both_runtimes_cultures_and_actual_core_failures(self):
        receipt = json.loads((c.OUT / 'baseline-registry.json').read_bytes())
        self.assertEqual('NOT_GOLDEN', receipt['status'])
        self.assertEqual(148, len(receipt['files']))
        for path, record in receipt['files'].items():
            self.assertEqual(c.sha(ROOT / path), record['sha256'])
        text = '\n'.join((ROOT / p).read_text(encoding='utf-8') for p in receipt['files'])
        self.assertIn('CORE_FAILURE|System.InvalidOperationException', text)
        self.assertIn('CORE_FAILURE|System.ArgumentOutOfRangeException', text)
        for weight in ['Logrank', 'Tarone-Ware', 'Gehan-Breslow', 'Peto', 'modified Peto']:
            self.assertIn('KM|' + weight, text)

    def test_all_udf_exports_core_numerics_designers_and_frameworks_unchanged(self):
        pins = json.loads((c.RUN / 'baseline.json').read_bytes())['source_pins']
        for path, h in pins.items():
            if path.startswith('BESHStat.Core/') or '/ExcelUDFs/' in path or path.endswith('.Designer.vb'):
                self.assertEqual(h, c.sha(ROOT / path), path)
        for project in ['BESHStat.Core', 'BESHStat.Excel.Common']:
            self.assertIn('<TargetFramework>netstandard2.0</TargetFramework>', (ROOT / project / (project + '.vbproj')).read_text(encoding='utf-8'))

    def test_new_common_sources_are_strict_host_neutral_and_do_not_duplicate_engines(self):
        for path in ['Analysis/SurvivalWorkflowPolicy.vb', 'Analysis/CoxWorkflowPolicy.vb', 'Results/SurvivalReportPolicy.vb']:
            source = (ROOT / 'BESHStat.Excel.Common/src' / path).read_text(encoding='utf-8')
            self.assertIn('Option Strict On', source)
            self.assertNotRegex(source, r'Microsoft\.VisualBasic|Interop|WinForms|ExcelDnaRef|System\.IO|Office\.js|DataObj')
            self.assertNotIn('Function Fit(', source)
        workflow = (ROOT / 'BESHStat.Excel.Common/src/Analysis/SurvivalWorkflowPolicy.vb').read_text(encoding='utf-8')
        order = ['WeightedLogRankTest(', 'BrookmeyerCrowleyMedianSurvivalCI(', 'SurvivalCurveTabularOutput(', 'EqualityOfMedianTest(', 'CompareCurveFixTimePoint(']
        self.assertEqual(sorted(workflow.index(x) for x in order), [workflow.index(x) for x in order])

    def test_c09_c11_c12_dormant_candidates_gee_and_public_accepted_evidence_unchanged(self):
        pins = json.loads((c.RUN / 'baseline.json').read_bytes())['source_pins']
        for path, h in pins.items():
            if path.startswith(('evidence/', 'docs/architecture/')) or 'Gee' in path or path.endswith('Ui13GEE.vb'):
                self.assertEqual(h, c.sha(ROOT / path), path)
        for path in json.loads((c.OUT / 'source-delta.json').read_bytes())['dormant_patches']:
            result = subprocess.run(['git', '-c', 'safe.directory=' + ROOT.as_posix(), 'apply', '--check', path], cwd=ROOT, capture_output=True)
            self.assertEqual(0, result.returncode)

    def test_help_availability_screenshots_and_native_capability_claims_are_truthful(self):
        for path in c.PAGES:
            source = (ROOT / path).read_text(encoding='utf-8')
            self.assertIn('## Office add-in availability', source)
            self.assertNotIn('## Using the Office add-in (Excel Online)', source)
            self.assertIn('historical Windows images', source)
            self.assertIn('not verified', re.sub(r'\s+', ' ', source))
        self.assertIn('disabled and has no functional wiring', (ROOT / c.PAGES[2]).read_text(encoding='utf-8'))

    def test_complete_existing_publication_set_has_no_unresolved_private_values(self):
        p = module('publication13', 'verify-04a13-publication.py')
        candidates = [path for path in p.paths() if (ROOT / path).is_file()]
        findings, _ = p.scan(candidates)
        self.assertEqual([], findings)


if __name__ == '__main__':
    unittest.main()
