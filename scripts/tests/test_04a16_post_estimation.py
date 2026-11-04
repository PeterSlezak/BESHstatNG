"""04A.16 successor source contracts; frozen predecessor assertions stay intact."""
import importlib.util
import json
from pathlib import Path
import subprocess
import unittest

ROOT = Path(__file__).resolve().parents[2]
spec = importlib.util.spec_from_file_location('post16', ROOT / 'scripts/capture-04a16-contract.py')
c = importlib.util.module_from_spec(spec)
spec.loader.exec_module(c)


class Mixed16SourceContracts(unittest.TestCase):
    def test_branch_base_index_and_exact_scope(self):
        self.assertEqual('codex/04A-16', c.git('branch', '--show-current'))
        for rev in ['HEAD', 'main']:
            self.assertEqual(c.BASE, c.git('rev-parse', rev))
        self.assertEqual(c.BASE, c.git('merge-base', 'HEAD', 'main'))
        self.assertEqual('', c.git('diff', '--cached', '--name-only'))
        self.assertEqual(c.CHANGED, set(c.git('diff', '--name-only').splitlines()))

    def test_unchanged_methods_designers_udfs_and_core(self):
        doc = json.loads((c.OUT / 'source-contract.json').read_bytes())
        for p, pin in doc['protected_source_pins'].items():
            self.assertEqual(pin, c.sha(ROOT / p), p)
        for p, hashes in doc['changed_existing'].items():
            self.assertEqual(hashes['after'], c.sha(ROOT / p), p)
        for p, pin in doc['new_sources'].items():
            self.assertEqual(pin, c.sha(ROOT / p), p)
        for p in c.FORMS:
            before = c.members(p, c.original(p))
            after = c.members(p, (ROOT / p).read_text(encoding='utf-8-sig'))
            self.assertEqual(c.METHODS[p], {n for n in before if before[n] != after[n]})
            for n in before.keys() - c.METHODS[p]:
                self.assertEqual(before[n], after[n], (p, n))

    def test_actual_mmrm_refresh_then_snapshot_and_common_orchestration(self):
        methods = c.members(c.FORMS[0], (ROOT / c.FORMS[0]).read_text(encoding='utf-8-sig'))
        append = methods['AppendMMRMEstimatedMeanTables']
        self.assertLess(append.index('RefreshMMRMContrastControlsFromData('), append.index('CaptureMMRMReportOptions()'))
        for token in ['New MixedModelPostEstimationIntent(', 'MixedModelPostEstimationPolicy.ProjectMMRM(', 'AppendMMRMReferenceGridTables(',
                      'WindowsMixedModelWorkflowAdapter.ApplySupplemental(result, supplemental)', 'mmrmData.Raw.RowIds', 'reportOptions.Alpha = alphaValue']:
            self.assertIn(token, append)
        self.assertNotIn('regression.MMRMPostEstimation.Build', append)
        self.assertIn('MixedModelPostEstimationPolicy.ProjectReference(', methods['AppendMMRMReferenceGridTables'])
        self.assertIn('MixedModelPostEstimationPolicy.BuildReferenceContrasts(', methods['BuildMMRMReferenceGridGroupContrastsTable'])
        for name in ['AddReferenceGridMarginalFactors', 'AddReferenceGridContinuousCovariates']:
            self.assertIn('MixedModelPostEstimationPolicy.AddReferenceColumn(', methods[name])
            self.assertIn('ExtractNumericColumnFromData(', methods[name])
        for name in ['ResolveSelectedBaselineVisit', 'ResolveSelectedControlLevel', 'ResolveSelectedComparisonLevel']:
            self.assertIn('MixedModelPostEstimationPolicy.Resolve', methods[name])
            self.assertNotIn('Double.TryParse', methods[name])

    def test_actual_lmm_class_projection_and_writer_consumption(self):
        methods = c.members(c.FORMS[1], (ROOT / c.FORMS[1]).read_text(encoding='utf-8-sig'))
        self.assertIn('MixedModelPostEstimationPolicy.LmmClassReport(rows)', methods['AppendLMMClassInfoTable'])
        self.assertIn('WindowsMixedModelWorkflowAdapter.ApplySupplemental(', methods['AppendLMMClassInfoTable'])
        self.assertIn('MixedModelPostEstimationPolicy.MainEffectKeys(', methods['GetCategoricalMainEffectBaseKeys'])
        for p, kind in zip(c.FORMS, ['MMRM', 'LMM']):
            m = c.members(p, (ROOT / p).read_text(encoding='utf-8-sig'))
            writer = m['Write' + kind + 'ModelSheet']
            self.assertIn('MixedModelReportPolicy.Project(', writer)
            self.assertIn('report.LegacyTables()', writer)
            self.assertIn('New ProcessListofResultTables(tables)', writer)
            self.assertIn('rr.writeToSheet(writeRes', writer)
            self.assertIn('decision)', writer)
        self.assertNotIn('ReferenceGrid', methods['AppendLMMClassInfoTable'])

    def test_run_callbacks_fit_and_native_writer_operations_stay_identical(self):
        for p, kind in zip(c.FORMS, ['MMRM', 'LMM']):
            before = c.members(p, c.original(p))
            after = c.members(p, (ROOT / p).read_text(encoding='utf-8-sig'))
            method = 'Run' + kind + 'Async'
            expected = before[method]
            if kind == 'MMRM':
                expected = expected.replace('AppendMMRMEstimatedMeanTables(result, x, fixedNames, visit, mmrmGuiData)', 'AppendMMRMEstimatedMeanTables(result, x, fixedNames, visit, mmrmGuiData, workflow.Decision)')
                expected = expected.replace('WriteMMRMResults(mmrmGuiData, result, y, x, fixedNames, subjectId, visit)', 'WriteMMRMResults(mmrmGuiData, result, y, x, fixedNames, subjectId, visit, workflow.Decision)')
            else:
                expected = expected.replace('WriteLMMResults(lmmGuiData, result, y, x, fixedNames, z, randomNames, subjectId, visit)', 'WriteLMMResults(lmmGuiData, result, y, x, fixedNames, z, randomNames, subjectId, visit, workflow.Decision)')
            self.assertEqual(expected, after[method])
            self.assertEqual(1, after[method].count('model.Fit()'))
            for name in ['Write' + kind + 'Results', 'Write' + kind + 'ModelSheet']:
                # Audit native/writer statements verbatim, independent of the new argument plumbing.
                tokens = ['AppGlobals.', 'Worksheets.', 'ActiveSheet.', 'writeRes.', 'rr.writeToSheet', 'Write' + kind + 'DataSheet', 'ShouldWriteTrace', 'RandomEffects.Clear', 'CovarianceUserScale = Nothing', 'CorrelationUserScale = Nothing']
                pick = lambda body: [line.strip() for line in body.splitlines() if any(t in line for t in tokens)]
                self.assertEqual(pick(before[name]), pick(after[name]))

    def test_portable_numerical_ownership_and_weak_attachment(self):
        policy = (ROOT / 'BESHStat.Excel.Common/src/Results/MixedModelPostEstimationPolicy.vb').read_text(encoding='utf-8')
        for forbidden in ['Windows.Forms', 'DataObj', 'ExcelDna', 'System.IO', 'Microsoft.VisualBasic', '.Fit()', 'T_Inv', 'AdjustPValues(']:
            self.assertNotIn(forbidden, policy)
        for operation in ['MMRMPostEstimation.BuildEstimatedMeansByVisitTable', 'MixedModelReferenceGridService.BuildReferenceGridRows', 'MixedModelReferenceGridService.BuildContrastsAgainstControlByFactor']:
            self.assertIn(operation, policy)
        self.assertIn('Option Strict On', policy)
        adapter = (ROOT / 'BESHstatNG/src/DM/WindowsMixedModelWorkflowAdapter.vb').read_text(encoding='utf-8-sig')
        for token in ['ConditionalWeakTable(Of MixedModelResult, AttachedTables)', 'report.LegacyTables()', 'result.AdditionalResultTables.InsertRange(position, attached.Tables)']:
            self.assertIn(token, adapter)
        self.assertNotIn('AdditionalResultTables.Add', policy)

    def test_frozen_captures_fit_ledger_original_ids_and_oracle(self):
        doc = json.loads((c.OUT / 'retained-evidence.json').read_bytes())
        self.assertEqual((177, 69, 67, 1408), (doc['original_controls'], doc['open_obligations'], doc['restricted_controls'], doc['batch15_records']))
        self.assertEqual({'CONVERGED': 712, 'NONCONVERGED': 696}, doc['batch15_statuses'])
        self.assertEqual(100, len(doc['captures']))
        for p, record in doc['retained'].items():
            self.assertEqual(record['sha256'], c.sha(ROOT / p), p)
            self.assertEqual(record['bytes'], (ROOT / p).stat().st_size)
        provenance = json.loads((c.OUT / 'legacy-selection-provenance.json').read_bytes())
        self.assertEqual(provenance['derivative_sha256'], c.sha(ROOT / provenance['derivative_path']))
        for p, pin in doc['original_authorities'].items():
            self.assertEqual(pin, c.sha(ROOT / p))

    def test_dormant_patches_still_apply_without_activation(self):
        for p in ['evidence/04A09/strict-validation-activation.UNAPPLIED.patch', 'evidence/04A11/C11-NATIVE-CORRECTIONS-01.final.UNAPPLIED.patch', 'evidence/04A12/C12-NATIVE-CLASSIFICATION-01.UNAPPLIED.patch']:
            result = subprocess.run(['git', '-c', 'safe.directory=' + ROOT.as_posix(), 'apply', '--check', p], cwd=ROOT, capture_output=True)
            self.assertEqual(0, result.returncode, p)


if __name__ == '__main__':
    unittest.main()
