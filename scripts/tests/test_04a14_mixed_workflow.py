"""04A.14 successor source contracts; historical assertions remain unchanged."""
import csv
import importlib.util
import json
from pathlib import Path
import re
import subprocess
import unittest

ROOT = Path(__file__).resolve().parents[2]

def module(name, path):
    spec = importlib.util.spec_from_file_location(name, ROOT / 'scripts' / path)
    result = importlib.util.module_from_spec(spec)
    spec.loader.exec_module(result)
    return result

c = module('mixed14_contract', 'capture-04a14-contract.py')

class Mixed14SourceContracts(unittest.TestCase):
    def test_branch_base_ancestry_and_empty_index(self):
        self.assertEqual('codex/04A-14', c.git('branch', '--show-current'))
        for rev in ['HEAD', 'main', 'HEAD^{commit}']:
            self.assertEqual(c.BASE, c.git('rev-parse', rev))
        self.assertEqual(c.BASE, c.git('merge-base', 'HEAD', 'main'))
        self.assertEqual('', c.git('diff', '--cached', '--name-only'))

    def test_exact_scope_and_external_owner_plan_are_separate(self):
        pins = json.loads((c.RUN / 'baseline.json').read_bytes())['source_pins']
        errors = [p for p, h in pins.items() if p not in c.CHANGED | {c.OWNER_PLAN} and c.sha(ROOT / p) != h]
        self.assertEqual([], errors)
        self.assertEqual(c.CHANGED | {c.OWNER_PLAN}, set(c.git('diff', '--name-only').splitlines()))
        owner = json.loads((c.OUT / 'owner-plan-update.json').read_bytes())
        self.assertEqual(pins[c.OWNER_PLAN], owner['before_sha256'])
        self.assertEqual(c.sha(ROOT / c.OWNER_PLAN), owner['observed_sha256'])
        self.assertFalse(owner['initial_protection_record_modified'])

    def test_original_members_and_successor_pins_are_reconcilable(self):
        delta = json.loads((c.OUT / 'source-delta.json').read_bytes())
        for path, pin in delta['changed_existing'].items():
            self.assertEqual(c.sha(ROOT / path), pin['after'])
        for path, receipt in delta['members'].items():
            before = c.get_members(path, c.original(path))
            current = c.get_members(path, (ROOT / path).read_text(encoding='utf-8-sig'))
            for name, pin in receipt['unchanged'].items():
                self.assertEqual(before[name], current[name], (path, name))
                self.assertEqual(c.digest(current[name]), pin)
            for name, pins in receipt['changed'].items():
                self.assertEqual(c.digest(before[name]), pins['before'])
                self.assertEqual(c.digest(current[name]), pins['after'])

    def test_actual_mmrm_async_handler_uses_common_request_and_one_core_fit(self):
        text = (ROOT / c.FORMS[0]).read_text(encoding='utf-8-sig')
        run = c.prior.members(text)['RunMMRMAsync']
        for token in ['MixedModelWorkflowPolicy.DecideFit(', 'WindowsMixedModelWorkflowAdapter.CaptureMMRM(',
                      'workflow.CreateAcquiredCoreRequest(rStruct)', 'rowNumbers:=raw.RowIds',
                      'sortWithinSubjectByVisit:=True', 'req.ProgressReporter = AddressOf',
                      'req.CancellationRequested = AddressOf', 'req.InterruptionRequested = AddressOf',
                      'Await Task.Run(Function() model.Fit())', 'workflow = Nothing', 'ReleaseMMRMLargeRunReferences(']:
            self.assertIn(token, run)
        self.assertEqual(1, run.count('model.Fit()'))
        self.assertLess(run.index('CompleteMMRMRunOnUiThread()'), run.index('AppendMMRMEstimatedMeanTables('))
        self.assertNotIn('.CreateCoreRequest()', run)

    def test_actual_lmm_async_handler_uses_common_request_and_one_core_fit(self):
        text = (ROOT / c.FORMS[1]).read_text(encoding='utf-8-sig')
        run = c.prior.members(text)['RunLMMAsync']
        for token in ['MixedModelWorkflowPolicy.DecideFit(', 'WindowsMixedModelWorkflowAdapter.CaptureLMM(',
                      'workflow.CreateAcquiredCoreRequest(rStruct, gStruct)', 'ValidateRandomStructureAgainstExpandedDesign(',
                      'sortWithinSubjectByVisit:=(visit IsNot Nothing)', 'rowNumbers:=raw.RowIds',
                      'req.ProgressReporter = AddressOf', 'req.CancellationRequested = AddressOf',
                      'req.InterruptionRequested = AddressOf', 'workflow = Nothing', 'ReleaseLMMLargeRunReferences(']:
            self.assertIn(token, run)
        self.assertEqual(1, run.count('model.Fit()'))
        self.assertNotIn('.CreateCoreRequest()', run)

    def test_kr_messages_workbook_output_order_and_acquisition_are_preserved(self):
        for path in c.FORMS:
            old = c.original(path)
            current = (ROOT / path).read_text(encoding='utf-8-sig')
            for pattern in [r'WriteRes\.\w+\(', r'^.*ActiveSheet\.name.*$', r'^.*MsgBox\(.*$']:
                self.assertEqual(re.findall(pattern, old, re.M), re.findall(pattern, current, re.M))
            before, after = c.prior.members(old), c.prior.members(current)
            for name in before:
                if any(t in name for t in ['GetData', 'BuildExpanded', 'Progress', 'Cancellation', 'Interruption', 'Cleanup', 'LargeRunReferences']):
                    self.assertEqual(before[name], after[name], name)
            self.assertIn('If fitDecision.CoerceToReml Then', current)
            self.assertIn('Me.cbFitMethod.SelectedItem = "REML"', current)

    def test_reports_are_consumed_by_actual_writers_and_append_seams(self):
        for path, kind in zip(c.FORMS, ['MMRM', 'LMM']):
            current = (ROOT / path).read_text(encoding='utf-8-sig')
            members = c.prior.members(current)
            self.assertIn('MixedModelReportPolicy.Project(', members['Write' + kind + 'ModelSheet'])
            self.assertIn('.LegacyTables()', members['Write' + kind + 'ModelSheet'])
            self.assertIn('MixedModelReportPolicy.ShouldWriteTrace(', current)
            self.assertIn('MixedModelReportPolicy.ShouldWriteResidual(', current)
        mmrm = c.prior.members((ROOT / c.FORMS[0]).read_text(encoding='utf-8-sig'))
        self.assertIn('MixedModelReportPolicy.Plan(', mmrm['AppendMMRMEstimatedMeanTables'])
        self.assertIn('New MixedModelReferenceIntent(', mmrm['BuildMMRMReferenceGridSpec'])
        self.assertIn('.CreateCoreSpec()', mmrm['BuildMMRMReferenceGridSpec'])
        self.assertIn('RefreshMMRMContrastControlsFromData', mmrm['AppendMMRMEstimatedMeanTables'])

    def test_original_controls_obligations_and_restriction_hashes(self):
        ledger = json.loads((ROOT / c.LEDGER).read_bytes())
        with (ROOT / c.CONTROLS).open(encoding='utf-8-sig') as f:
            rows = list(csv.DictReader(f))
        receipt = json.loads((c.OUT / 'restriction-crosswalk.json').read_bytes())
        self.assertEqual(177, len(receipt['controls']))
        self.assertEqual(69, sum(r['obligation_id'] is not None for r in receipt['controls']))
        self.assertEqual(67, sum(r['original_restricted'] for r in receipt['controls']))
        for row in receipt['controls']:
            original = next(r for r in rows if r['control_id'] == row['control_id'])
            self.assertEqual(c.digest(original), row['control_row_sha256'])
            if row['obligation_id']:
                obligation = next(r for r in ledger if r['obligation_id'] == row['obligation_id'])
                self.assertEqual(c.digest(obligation), row['obligation_row_sha256'])
                self.assertEqual('OPEN', row['retained_status'])
                self.assertFalse(row['capture_authorized'])
                self.assertEqual(obligation['source_evidence']['original_control_sha256'], row['original_control_sha256'])
        delta = json.loads((c.OUT / 'source-delta.json').read_bytes())
        for path in c.FORMS:
            form = Path(path).stem
            members = c.prior.members((ROOT / path).read_text(encoding='utf-8-sig'))
            text = '\n'.join(members[n] for n in list(delta['members'][path]['changed']) + list(delta['members'][path]['added']))
            refs = set(re.findall(r'Me\.((?:cb|ck|tb|lb|spin|bt|btn)\w+)', text))
            covered = {r['control_id'].split('.')[-1] for r in receipt['controls'] if '.' + form + '.' in r['control_id'] and r['source_associated_changed_methods']}
            self.assertTrue(refs <= covered, sorted(refs - covered))
        self.assertEqual(0, receipt['released'])
        self.assertEqual(0, receipt['discharged'])

    def test_prechange_captures_are_frozen_both_cultures_runtimes(self):
        c.baselines()
        registry = json.loads((c.OUT / 'baseline-registry.json').read_bytes())
        self.assertEqual('NOT_GOLDEN', registry['status'])
        self.assertEqual(100, len(registry['files']))
        self.assertEqual(25, registry['cases_per_runtime'])
        text = '\n'.join((ROOT / p).read_text(encoding='utf-8') for p in registry['files'])
        for token in ['Satterthwaite', 'Kenward-Roger', 'Orthodont', 'sleep']:
            self.assertIn(token.lower(), text.lower())

    def test_core_udf_exports_designers_and_accepted_survival_are_unchanged(self):
        pins = json.loads((c.RUN / 'baseline.json').read_bytes())['source_pins']
        for path, pin in pins.items():
            if path.startswith(('BESHStat.Core/', 'evidence/04A13/')) or '/ExcelUDFs/' in path or path.endswith(('.Designer.vb', 'Ui4KMandLogRank.vb', 'Ui4Cox.vb')):
                self.assertEqual(pin, c.sha(ROOT / path), path)
        for project in ['BESHStat.Core', 'BESHStat.Excel.Common']:
            self.assertIn('<TargetFramework>netstandard2.0</TargetFramework>', (ROOT / project / (project + '.vbproj')).read_text())

    def test_dormant_c09_c11_c12_apply_checks_still_pass(self):
        receipt = json.loads((c.OUT / 'source-delta.json').read_bytes())
        for path, pin in receipt['dormant_patches'].items():
            self.assertFalse(pin['applied'])
            self.assertEqual(c.sha(ROOT / path), pin['sha256'])
            self.assertEqual(0, subprocess.run(['git', '-c', 'safe.directory=' + ROOT.as_posix(), 'apply', '--check', path], cwd=ROOT, capture_output=True).returncode)

    def test_shared_parser_signatures_and_delegation_preserve_udf_seam(self):
        before = c.get_members(c.HELPER, c.original(c.HELPER))
        after = c.get_members(c.HELPER, (ROOT / c.HELPER).read_text(encoding='utf-8-sig'))
        for name in ['ParseFitMethod', 'ParseInferenceMethod', 'ParseCovarianceOptimizerMode', 'ParseCovarianceGradientMode', 'NormalizeToken', 'AsString']:
            self.assertEqual(before[name].splitlines()[0], after[name].splitlines()[0])
            self.assertIn('ExcelCommon.MixedModelOptionPolicy.' + name + '(', after[name])

    def test_common_is_strict_host_neutral_and_keeps_statistical_authority(self):
        for path in ['Analysis/MixedModelOptionPolicy.vb', 'Analysis/MixedModelWorkflowPolicy.vb', 'Results/MixedModelReportPolicy.vb']:
            source = (ROOT / 'BESHStat.Excel.Common/src' / path).read_text()
            self.assertIn('Option Strict On', source)
            self.assertNotRegex(source, r'Microsoft\.VisualBasic|Microsoft\.Office|WinForms|ExcelDnaRef|System\.IO|Office\.js|\bDataObj\b')
            self.assertNotIn('Function Fit(', source)
        source = (ROOT / 'BESHStat.Excel.Common/src/Analysis/MixedModelWorkflowPolicy.vb').read_text()
        options = (ROOT / 'BESHStat.Excel.Common/src/Analysis/MixedModelOptionPolicy.vb').read_text()
        for side, kind in [('R', 'Residual'), ('G', 'Random')]:
            core = (ROOT / ('BESHStat.Core/src/RegModels/MixedModels/MixedModel' + side + 'Struct.vb')).read_text(encoding='utf-8-sig')
            start = core.index('            Dim f As MixedModel' + side + 'Struct')
            end = core.index('            Return f', start) + len('            Return f')
            expected = []
            for line in core[start:end].splitlines():
                if 'CoreServices.Logger.' in line:
                    continue
                expected.append('                Return Nothing' if 'Throw New ApplicationException' in line else line)
            fragment = options.split('Friend Shared Function ' + kind + 'ForValidation(', 1)[1].split('        End Function', 1)[0]
            actual = fragment[fragment.index('            Dim f As'):].rstrip()
            self.assertEqual('\n'.join(expected).rstrip(), actual)
        validation = source.split('Public Shared Function Validate(request', 1)[1]
        self.assertNotIn('createMixedModelRStruct(', validation)
        self.assertNotIn('createMixedModelGStruct(', validation)
        self.assertIn('req.Control = ctl', source)
        self.assertIn('MixedModelFitRequest.CreateMMRM', source)
        self.assertIn('MixedModelFitRequest.CreateLMM', source)
        report = (ROOT / 'BESHStat.Excel.Common/src/Results/MixedModelReportPolicy.vb').read_text()
        self.assertIn('result.wrapResults(', report)
        self.assertIn('LinearBinomialReport.Rehydrate', report)

    def test_both_windows_test_project_links_and_real_adapter_inclusions(self):
        tests = (ROOT / 'BESHStatNG.Test/BESHStatNG.Tests.vbproj').read_text()
        for name in ['Mixed14Baselines', 'Mixed14WorkflowTests', 'Mixed14ReportTests', 'LegacyMixed14Options', 'Mixed14AdapterTests']:
            self.assertIn(name + '.vb', tests)
        self.assertIn('WindowsMixedModelWorkflowAdapter.vb', (ROOT / 'BESHstatNG/BESHStatNG.vbproj').read_text(encoding='utf-8-sig'))
        adapter = (ROOT / 'BESHstatNG/src/DM/WindowsMixedModelWorkflowAdapter.vb').read_text()
        self.assertIn('MixedModelWorkflowData.FromAcquiredBlocks(', adapter)

    def test_documentation_availability_and_no_native_claims(self):
        for path in c.PAGES:
            source = (ROOT / path).read_text()
            self.assertIn('## Office add-in availability', source)
            self.assertIn('not yet available', source)
            self.assertIn('historical Windows images', source)
            self.assertNotIn('## Using the Office add-in (Excel Online)', source)
            self.assertIn('Kenward', source)

    def test_entire_proposed_publication_set_privacy(self):
        pub = module('mixed14_publication', 'verify-04a14-publication.py')
        self.assertNotIn(c.OWNER_PLAN, pub.paths())
        findings, _ = pub.scan([p for p in pub.paths() if (ROOT / p).is_file()])
        self.assertEqual([], findings)

if __name__ == '__main__':
    unittest.main()
