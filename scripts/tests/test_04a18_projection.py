"""Successor 04A.18 source contracts; frozen predecessor assertions stay intact."""
import importlib.util
import json
from pathlib import Path
import subprocess
import unittest

ROOT = Path(__file__).resolve().parents[2]
spec = importlib.util.spec_from_file_location('contract18', ROOT / 'scripts/capture-04a18-contract.py')
c = importlib.util.module_from_spec(spec)
spec.loader.exec_module(c)


def text(path):
    return (ROOT / path).read_text(encoding='utf-8-sig')


class Projection18SourceContracts(unittest.TestCase):
    def test_live_branch_index_and_protected_entry_bytes(self):
        self.assertEqual(c.git('branch', '--show-current'), 'codex/04A-18')
        self.assertEqual(c.git('rev-parse', 'HEAD'), c.BASE)
        self.assertEqual(c.git('rev-parse', 'main'), c.BASE)
        self.assertFalse(c.git('diff', '--cached', '--name-only'))
        pins = json.loads((c.RUN / 'baseline.json').read_bytes())['source_pins']
        for p, sha in pins.items():
            if p not in c.CHANGED:
                self.assertEqual(c.sha(ROOT / p), sha, p)

    def test_real_pca_mca_writers_consume_portable_sections_and_plots(self):
        methods = c.members(c.FORMS[0], text(c.FORMS[0]))
        for method, family in [('RunPCA', 'Pca'), ('RunMCA', 'Mca')]:
            body = methods[method]
            self.assertIn('MultivariateComponentProjectionPolicy.' + family + '(', body)
            self.assertIn('WindowsMultivariateWorkflowAdapter.Write' + family + 'Data(', body)
            self.assertIn('projection.ToReport(', body)
            self.assertIn('WindowsMultivariateWorkflowAdapter.Render' + family + '(projection,', body)
            self.assertEqual(body.count('.Calculate('), 1)
            self.assertLess(body.index('.Calculate('), body.index('Write' + family + 'Data('))
            self.assertLess(body.index('Write' + family + 'Data('), body.index('.wrapResults()'))
        self.assertLess(methods['RunMCA'].index('SubsetByRowIdValues'), methods['RunMCA'].index('MultivariateMcaTransformIntent'))

    def test_only_three_form_methods_change_and_spm_remains_exact(self):
        for path, expected in zip(c.FORMS, [{'RunPCA', 'RunMCA'}, {'RunCA'}]):
            old = c.members(path, c.original(path))
            new = c.members(path, text(path))
            self.assertEqual(set(old), set(new))
            self.assertEqual({m for m in old if old[m] != new[m]}, expected)
        old = c.members(c.ADAPTER, c.original(c.ADAPTER))
        new = c.members(c.ADAPTER, text(c.ADAPTER))
        for name in ['Capture', 'RenderFactor']:
            self.assertEqual(old[name], new[name])

    def test_labelled_ca_and_both_existing_destination_paths(self):
        body = c.members(c.FORMS[1], text(c.FORMS[1]))['RunCA']
        for call in ['CaptureLabelledCa(data, RowLabels)', 'ConvertLabelledCaCounts(data, countInput)',
                     'MultivariateComponentProjectionPolicy.SimpleCa(', 'MultivariateComponentProjectionPolicy.CaReport(',
                     'RenderCa(projection, ca)', 'RenderCa(projection, ca, WriteRes.ws)', 'rr.writeToSheet(WriteRes, True)']:
            self.assertIn(call, body)
        self.assertIn('AreaCheck(WriteRes.RowID, WriteRes.ColID', body)
        self.assertIn('Output range not empty! Overwrite?', body)
        self.assertLess(body.index('ca.Calculate()'), body.index('CaReport('))

    def test_unlabelled_failure_is_not_bypassed_and_patch_is_unapplied(self):
        old = c.members(c.FORMS[1], c.original(c.FORMS[1]))['RunCA']
        new = c.members(c.FORMS[1], text(c.FORMS[1]))['RunCA']
        start = '        Else\n            ReDim TableData'
        segment = lambda s: s[s.index(start):s.index('        End If', s.index(start))]
        self.assertEqual(segment(old), segment(new))
        self.assertIn('TableData(i, j - 1)', segment(new))
        self.assertNotIn('DormantUnlabelled', new)
        self.assertNotIn('DormantUnlabelled', text(c.ADAPTER))
        patch = 'evidence/04A18/C18-CA-NOLABEL-ACTIVATION-01.UNAPPLIED.patch'
        result = subprocess.run(['git', '-c', 'safe.directory=' + ROOT.as_posix(), 'apply', '--check', patch], cwd=ROOT, capture_output=True)
        self.assertEqual(result.returncode, 0, result.stderr)
        disposition = json.loads((c.OUT / 'c17-ca-nolabel-disposition.json').read_bytes())
        self.assertIn('OPEN', json.dumps(disposition))

    def test_frozen_oracle_and_existing_captures_are_immutable(self):
        provenance = json.loads((c.OUT / 'legacy-layout-provenance.json').read_bytes())
        self.assertEqual(c.sha(ROOT / provenance['derivative_path']), provenance['derivative_sha256'])
        registry = json.loads((ROOT / 'evidence/04A14/baseline-registry.json').read_bytes())
        self.assertEqual(len(registry['files']), 100)
        for path, pin in registry['files'].items():
            self.assertEqual(c.sha(ROOT / path), pin['sha256'])
            self.assertEqual((ROOT / path).stat().st_size, pin['bytes'])

    def test_portable_axes_borrowed_payload_and_nonblocking_boundary(self):
        policy = text('BESHStat.Excel.Common/src/Results/MultivariateComponentProjectionPolicy.vb')
        axis = text('BESHStat.Excel.Common/src/Results/MultivariateAxisProjection.vb')
        inputs = text('BESHStat.Excel.Common/src/Analysis/MultivariateCorrespondenceInputPolicy.vb')
        for source in [policy, axis, inputs]:
            self.assertIn('Option Strict On', source)
            for forbidden in ['Imports System.Windows.Forms', 'Imports ExcelDna', 'Imports System.IO', 'As DataObj', '.Calculate(', '.Fit(']:
                self.assertNotIn(forbidden, source)
        for name in ['Observations', 'Categories', 'VariableCategoryBlocks', 'RetainedComponents', 'EigenComponents', 'CorrespondenceFactors']:
            self.assertIn(name, axis)
        self.assertIn('blocking:=False', policy)
        self.assertIn('model.DesignMatrix', policy)
        self.assertIn('original', policy)
        self.assertIn('.AsReadOnly()', axis)
        adapter = text(c.ADAPTER)
        self.assertIn('MultivariateComponentProjectionPolicy.VisitPca(', adapter)
        self.assertIn('MultivariateComponentProjectionPolicy.VisitMca(', adapter)
        self.assertIn('writer.setRowPointer(section.StartRow)', adapter)

    def test_managed_successors_cover_layout_types_immutability_and_gating(self):
        common = text('BESHStat.Excel.Common.Tests/src/Multivariate18Tests.vb')
        windows = text('BESHStatNG.Test/src/Multivariate18AdapterTests.vb')
        for token in ['LegacyMultivariate18Layout.PcaData', 'LegacyMultivariate18Layout.McaData', 'sk-SK', 'UnlabelledFailureAndDormantIdentityMappingAreDifferent', 'McaEmptySmallNullAndMalformedAxesRetainCoreBoundary']:
            self.assertIn(token, common)
        for token in ['WritePcaData', 'WriteMcaData', 'ConvertLabelledCaCounts', 'CaReport', 'LegacyUnlabelledCounts']:
            self.assertIn(token, windows)
        project = text('BESHStatNG.Test/BESHStatNG.Tests.vbproj')
        for file in ['Multivariate18AdapterTests.vb', 'Multivariate18Tests.vb', 'LegacyMultivariate18Layout.vb']:
            self.assertIn(file, project)


if __name__ == '__main__':
    unittest.main()
