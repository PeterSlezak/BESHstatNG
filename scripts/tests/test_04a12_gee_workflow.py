"""04A.12 current-source successors. Original frozen assertions remain untouched."""
import importlib.util
import json
from pathlib import Path
import re
import subprocess
import unittest

ROOT = Path(__file__).resolve().parents[2]
spec = importlib.util.spec_from_file_location('c12', ROOT / 'scripts/capture-04a12-contract.py')
c = importlib.util.module_from_spec(spec)
spec.loader.exec_module(c)


class Gee12WorkflowSuccessorTests(unittest.TestCase):
    def test_exact_source_scope_and_all_old_byte_pins(self):
        c.source()
        receipt = json.loads((c.OUT / 'source-delta.json').read_bytes())
        self.assertEqual(5, len(receipt['changed_members']))
        self.assertGreater(receipt['protected_count'], 3200)
        self.assertEqual(70, receipt['private_historical_filename_pins'])
        self.assertTrue(all(receipt['invariants'].values()))

    def test_real_form_uses_portable_intent_report_and_writer_consumption(self):
        methods = c.prior.members((ROOT / c.FORM).read_text(encoding='utf-8-sig'))
        run = methods['RunGEE']
        for token in ['WindowsGeeWorkflowAdapter.Capture(', 'fitGEE = intent.CreateCoreModel()',
                      'fitGEE.Fit(', 'GeeReportPolicy.Predictions(intent,', 'GeeReportPolicy.Residuals(intent,',
                      'WindowsGeeWorkflowAdapter.Report(intent,', 'res = report.LegacyTables()',
                      'GeeReportPolicy.Classification(report,', 'clsRes = classification.LegacyTables()']:
            self.assertIn(token, run)
        self.assertNotIn('fitGEE.settingInputs(', run)
        self.assertNotIn('fitGEE.data(', run)
        self.assertNotIn('New GEE(', run)
        self.assertNotRegex(run, r'If .*?(?:intent\.Validation\.(?:IsValid|Diagnostics)|\.Blocking)')

    def test_actual_import_and_correlation_engine_stay_in_core_unchanged(self):
        before = json.loads((c.OUT / 'prechange-manifest.json').read_bytes())['source_pins']
        for path in ['BESHStat.Core/src/DM/GeeData.vb', 'BESHStat.Core/src/RegModels/GEE.vb',
                     'BESHStat.Core/src/RegModels/GEEcovStruct.vb', 'BESHStat.Core/src/DM/RowArrayUtilities.vb']:
            self.assertEqual(before[path], c.sha(ROOT / path))
        data = (ROOT / 'BESHStat.Core/src/DM/GeeData.vb').read_text(encoding='utf-8-sig')
        self.assertIn('RowArrayUtilities.SortRows(data2', data)
        self.assertIn('Public Overrides Sub SubsetByRowIdValues', data)

    def test_common_is_strict_host_free_and_uses_existing_core_services(self):
        paths = ['Analysis/GeeWorkflowPolicy.vb', 'Analysis/GeeContrastPolicy.vb', 'Results/GeeReportPolicy.vb']
        for path in paths:
            text = (ROOT / 'BESHStat.Excel.Common/src' / path).read_text(encoding='utf-8-sig')
            self.assertIn('Option Strict On', text)
            self.assertNotRegex(text, r'(?i)Imports .*?(Windows.Forms|Interop|ExcelDna|System.IO|Microsoft.VisualBasic)|Double.Parse|writeToSheet|\.Fit\(')
        policy = (ROOT / 'BESHStat.Excel.Common/src/Analysis/GeeWorkflowPolicy.vb').read_text(encoding='utf-8-sig')
        self.assertIn('GEEcovStructUtils.createGEEcovMat', policy)
        self.assertIn('fit.settingInputs(EffectiveFitAlpha', policy)
        for path in ['BESHStat.Core/BESHStat.Core.vbproj', 'BESHStat.Excel.Common/BESHStat.Excel.Common.vbproj']:
            self.assertIn('<TargetFramework>netstandard2.0</TargetFramework>', (ROOT / path).read_text(encoding='utf-8-sig'))

    def test_exact_gee_control_lineage_and_no_borrowed_uiglm_obligations(self):
        row = json.loads((c.OUT / 'restriction-crosswalk.json').read_bytes())
        self.assertEqual((87, 35, 34, 52, 0, 0), tuple(row[k] for k in ['form_controls', 'original_open_obligations', 'original_restricted_controls', 'obligation_less_source_controls', 'released', 'discharged']))
        raw = json.loads((ROOT / c.LEDGER).read_bytes())
        by_id = {r['obligation_id']: r for r in raw}
        for control in row['controls']:
            self.assertTrue(control['control_id'].startswith('BESHStatNG.Ui13GEE.'))
            if control['obligation_id']:
                self.assertEqual(c.digest(by_id[control['obligation_id']]), control['obligation_row_sha256'])
                self.assertEqual('OPEN', control['status'])
            else:
                self.assertEqual('NO ORIGINAL DEFERRED OBLIGATION ROW', control['status'])
        self.assertEqual(('NO', 'NO GO', 'NO', 'UNMET'), tuple(row[k] for k in ['P', 'N1', 'N2', 'C2']))

    def test_frozen_real_core_captures_are_both_runtimes_and_cultures(self):
        registry = json.loads((c.OUT / 'baseline-registry.json').read_bytes())
        self.assertEqual('NOT_GOLDEN', registry['status'])
        self.assertEqual(120, len(registry['files']))
        self.assertEqual(30, registry['cases_per_runtime'])
        for path, record in registry['files'].items():
            self.assertEqual(record['sha256'], c.sha(ROOT / path))
            self.assertEqual(record['bytes'], (ROOT / path).stat().st_size)
        self.assertEqual(registry['capture_source_sha256'], c.sha(ROOT / 'BESHStat.Excel.Common.Tests/src/Gee12Baselines.vb'))

    def test_three_native_patches_remain_unapplied_and_exact_candidate_compiled(self):
        native = json.loads((c.OUT / 'native-patch.json').read_bytes())
        for path, expected in [
            ('evidence/04A09/strict-validation-activation.UNAPPLIED.patch', native['C09_sha256']),
            ('evidence/04A11/C11-NATIVE-CORRECTIONS-01.final.UNAPPLIED.patch', native['C11_sha256']),
            ('evidence/04A12/C12-NATIVE-CLASSIFICATION-01.UNAPPLIED.patch', native['patch_sha256'])]:
            self.assertEqual(expected, c.sha(ROOT / path))
            check = subprocess.run(['git', '-c', 'safe.directory=' + ROOT.as_posix(), 'apply', '--check', path], cwd=ROOT, capture_output=True)
            self.assertEqual(0, check.returncode)
        compiled = json.loads((c.OUT / 'native-candidate-validation.json').read_bytes())
        self.assertEqual('PASS', compiled['status'])
        self.assertFalse(compiled['native_executed'])
        self.assertFalse(compiled['patch_applied'])
        self.assertEqual(native['candidate_sha256'], compiled['candidate_sha256'])

    def test_original_udf_exports_and_no_gui_contrast_panel(self):
        path = 'BESHstatNG/src/ExcelUDFs/Regression/GeeUDFs.vb'
        pins = json.loads((c.OUT / 'prechange-manifest.json').read_bytes())['source_pins']
        self.assertEqual(pins[path], c.sha(ROOT / path))
        text = (ROOT / path).read_text(encoding='utf-8-sig')
        for name in ['GEE_FIT', 'GEE_SUMMARY', 'GEE_WCORR', 'GEE_VCOV', 'GEE_RESID', 'GEE_PRED', 'GEE_LSMESTIMATE']:
            self.assertIn('Public Function ' + name + '(', text)
        form = (ROOT / c.FORM).read_text(encoding='utf-8-sig')
        self.assertNotIn('GEE_LSMESTIMATE(', form)
        self.assertIn('NO_SHIPPING_GUI_CONTRAST_PATH', (ROOT / 'BESHStat.Excel.Common/src/Analysis/GeeContrastPolicy.vb').read_text(encoding='utf-8-sig'))

    def test_project_inclusions_and_unchanged_host_lifecycle(self):
        project = (ROOT / 'BESHstatNG/BESHStatNG.vbproj').read_text(encoding='utf-8-sig')
        self.assertIn('Compile Include="src\\DM\\WindowsGeeWorkflowAdapter.vb"', project)
        tests = (ROOT / 'BESHStatNG.Test/BESHStatNG.Tests.vbproj').read_text(encoding='utf-8-sig')
        for name in ['Gee12Baselines', 'Gee12PolicyTests', 'Gee12SemanticTests', 'Gee12FailureAndProvenanceTests', 'Gee12AdapterTests']:
            self.assertIn(name + '.vb', tests)
        before = c.prior.members(c.original(c.FORM))
        after = c.prior.members((ROOT / c.FORM).read_text(encoding='utf-8-sig'))
        for name in ['GetData', 'valiateInputs', 'ValidateExpandedInitialValuesCount', 'BeginRegressionComputation', 'EndRegressionComputation', 'RegressionForm_FormClosing']:
            self.assertEqual(before[name], after[name], name)

    def test_documentation_and_help_metadata_remain_truthful(self):
        page = (ROOT / c.PAGE).read_text(encoding='utf-8-sig')
        for phrase in ['Office add-in availability', 'not executable', 'no GEE contrast panel', 'nonnumeric cluster', 'original source Row ID', 'supplied-start limitation']:
            self.assertIn(phrase, page)
        self.assertNotIn('Using the Office add-in (Excel Online)', page)
        pins = json.loads((c.OUT / 'prechange-manifest.json').read_bytes())['source_pins']
        for path in ['BESHStatNG_Help_MkDocs/docs/_meta/methods.yml', 'BESHstatNG/Resources/RibbonXML.xml', 'BESHstatNG/src/UI/Ui13GEE.Designer.vb']:
            self.assertEqual(pins[path], c.sha(ROOT / path))


if __name__ == '__main__':
    unittest.main()
