"""04A.15 successor tests: never alter or waive historical source assertions."""
import importlib.util
import json
from pathlib import Path
import subprocess
import unittest

ROOT = Path(__file__).resolve().parents[2]
spec = importlib.util.spec_from_file_location('fitting15', ROOT / 'scripts/capture-04a15-contract.py')
c = importlib.util.module_from_spec(spec)
spec.loader.exec_module(c)


class Mixed15SourceContracts(unittest.TestCase):
    def test_branch_ancestry_empty_index_and_exact_existing_scope(self):
        self.assertEqual('codex/04A-15', c.git('branch', '--show-current'))
        for rev in ['HEAD', 'main']:
            self.assertEqual(c.BASE, c.git('rev-parse', rev))
        self.assertEqual(c.BASE, c.git('merge-base', 'HEAD', 'main'))
        self.assertEqual('', c.git('diff', '--cached', '--name-only'))
        self.assertEqual(c.CHANGED, set(c.git('diff', '--name-only').splitlines()))

    def test_all_protected_sources_and_new_source_hashes(self):
        doc = json.loads((c.OUT / 'source-contract.json').read_bytes())
        for p, digest in doc['protected_source_pins'].items():
            self.assertEqual(digest, c.sha(ROOT / p), p)
        for p, digest in doc['new_sources'].items():
            self.assertEqual(digest, c.sha(ROOT / p), p)
        for p, pair in doc['changed_existing'].items():
            self.assertEqual(pair['after'], c.sha(ROOT / p), p)

    def test_only_actual_shipping_factory_routes_change(self):
        for p, method in zip(c.FORMS, ['RunMMRMAsync', 'RunLMMAsync']):
            before = c.original(p)
            expected = before.replace('regression.MixedModelRStructUtils.createMixedModelRStruct(CStr(Me.cbCovarStruct.SelectedItem))',
                'WindowsMixedModelWorkflowAdapter.CreateResidualStructure(CStr(Me.cbCovarStruct.SelectedItem))')
            expected = expected.replace('regression.MixedModelGStructUtils.createMixedModelGStruct(CStr(Me.cbRandomCovarStruct.SelectedItem))',
                'WindowsMixedModelWorkflowAdapter.CreateRandomStructure(CStr(Me.cbRandomCovarStruct.SelectedItem))')
            text = (ROOT / p).read_text(encoding='utf-8-sig')
            self.assertEqual(expected, text)
            run = c.prior.get_members(p, text)[method]
            self.assertEqual(1, run.count('model.Fit()'))
            self.assertIn('workflow.CreateAcquiredCoreRequest(rStruct', run)
            self.assertNotIn('.CreateCoreRequest()', run)
            self.assertLess(run.index('CreateResidualStructure('), run.index('fitDecision.CoerceToReml'))
            self.assertLess(run.index('model.Fit()'), run.index('Write' + ('MMRM' if 'MMRM' in method else 'LMM') + 'Results('))

    def test_adapter_delegates_and_request_uses_specialization(self):
        adapter = (ROOT / 'BESHstatNG/src/DM/WindowsMixedModelWorkflowAdapter.vb').read_text()
        for kind in ['Residual', 'Random']:
            self.assertIn('Return MixedModelFittingOptions.Create' + kind + 'Structure(selectedToken)', adapter)
        workflow = (ROOT / 'BESHStat.Excel.Common/src/Analysis/MixedModelWorkflowPolicy.vb').read_text()
        for token in ['Fitting = New MixedModelFittingOptions(kind, _options)', 'Fitting.ApplyControl(ctl)', 'Fitting.Residual.CreateResidualStructure()',
                      'Fitting.Random.CreateRandomStructure()', 'MixedModelFittingOptions.ProjectOptimizer(kind, control, optimizer, gradient)', 'request.Fitting.DesignDiagnostics(d)']:
            self.assertIn(token, workflow)

    def test_frozen_captures_and_original_lineage(self):
        doc = json.loads((c.OUT / 'baseline-and-lineage.json').read_bytes())
        self.assertEqual((100, 177, 69, 67), (doc['baseline_count'], doc['original_controls'], doc['open_obligations'], doc['restricted_controls']))
        for p, record in doc['captures'].items():
            self.assertEqual(record['sha256'], c.sha(ROOT / p))
            self.assertEqual(record['bytes'], (ROOT / p).stat().st_size)
        self.assertEqual('NOT_GOLDEN', doc['status'])
        for p, digest in doc['original_authorities'].items():
            self.assertEqual(digest, c.sha(ROOT / p))

    def test_portable_surface_has_no_host_or_numeric_engine(self):
        text = (ROOT / 'BESHStat.Excel.Common/src/Analysis/MixedModelFittingOptions.vb').read_text()
        self.assertIn('Option Strict On', text)
        for token in ['Windows.Forms', 'ExcelDna', 'Microsoft.VisualBasic', 'System.IO', 'BuildG(', 'BuildR(', '.Fit()', 'Double(,)']:
            self.assertNotIn(token, text)
        self.assertIn('_random.ParamCount(q)', text)
        self.assertIn('_residual.ParamCount(data)', text)

    def test_dormant_windows_patches_still_apply_without_activation(self):
        for p in ['evidence/04A09/strict-validation-activation.UNAPPLIED.patch',
                  'evidence/04A11/C11-NATIVE-CORRECTIONS-01.final.UNAPPLIED.patch', 'evidence/04A12/C12-NATIVE-CLASSIFICATION-01.UNAPPLIED.patch']:
            result = subprocess.run(['git', '-c', 'safe.directory=' + ROOT.as_posix(), 'apply', '--check', p], cwd=ROOT, capture_output=True)
            self.assertEqual(0, result.returncode, p)


if __name__ == '__main__':
    unittest.main()
