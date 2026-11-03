"""04A.10 additive source compatibility checks; shipping adapter behavior is tested in net48."""
import hashlib
import importlib.util
import json
from pathlib import Path
import re
import unittest

ROOT=Path(__file__).resolve().parents[2]
spec=importlib.util.spec_from_file_location('contract10',ROOT/'scripts/capture-04a10-contract.py')
c=importlib.util.module_from_spec(spec);spec.loader.exec_module(c)
OUT=ROOT/'evidence/04A10'

class Regression10ProjectionTests(unittest.TestCase):
    def test_only_two_form_routes_change_and_native_tails_match(self):
        c.source()
        d=json.loads((OUT/'source-delta.json').read_bytes())
        self.assertEqual({'RunOLS','RunGLM'},set(d['changed_members']))
        for name in ['RunGLMNB2','RunZIP','RunOrdLogit','RunMultiLogit','CaptureRegressionRequest']:
            self.assertIn(name,d['unchanged_members'])
        self.assertTrue(all(t['equal'] for t in d['native_writer_tails'].values()))

    def test_actual_shipping_routes_consume_projected_intent_and_tables(self):
        methods=c.prior.members((ROOT/c.FORM).read_text(encoding='utf-8-sig'))
        for name in ['RunOLS','RunGLM']:
            body=methods[name]
            for token in ['LinearBinomialRegressionPolicy.Project','WindowsRegressionRequestAdapter.Report(intent, res,','.LegacyTables()']:
                self.assertIn(token,body)
            self.assertLess(body.index('.LegacyTables()'),body.index('AppGlobals.app.Workbooks.Add'))
        self.assertNotIn('lm.Alpha =',methods['RunOLS'])
        self.assertIn('actualFitAlpha:=lm.Alpha',methods['RunOLS'])

    def test_strict_deferral_and_original_ids_hashes_unchanged(self):
        d=json.loads((OUT/'restriction-crosswalk.json').read_bytes())
        parent=json.loads((ROOT/d['parent_path']).read_bytes())
        self.assertEqual(parent['controls'],d['controls'])
        self.assertEqual((34,7,0,0),tuple(d[k] for k in ['associated_controls','associated_obligations','released','discharged']))
        self.assertEqual('aab350401c873d1a4f56450295688e9a046122aca2269fc9a618765f8fe79fe1',c.sha(ROOT/'evidence/04A09/strict-validation-activation.UNAPPLIED.patch'))
        capture=c.prior.members((ROOT/c.FORM).read_text(encoding='utf-8-sig'))['CaptureRegressionRequest']
        self.assertNotRegex(capture,r'If .*?(?:IsValid|Blocking)|Throw')

    def test_common_is_host_free_and_has_no_solver_or_writes(self):
        for name in ['Analysis/LinearBinomialRegressionPolicy.vb','Results/LinearBinomialReport.vb']:
            text=(ROOT/'BESHStat.Excel.Common/src'/name).read_text()
            self.assertIn('Option Strict On',text)
            self.assertNotRegex(text,r'(?i)Imports .*?(?:Windows.Forms|Interop|ExcelDna|System.IO|Microsoft.VisualBasic)|\.Fit\(|writeToSheet|\bDoEvents\b')
        text=(ROOT/'BESHStat.Excel.Common/src/Analysis/LinearBinomialRegressionPolicy.vb').read_text()
        self.assertIn('FamilyUtils.createFamily',text)
        self.assertIn('RegressionRequestPolicy.Validate',text)
        self.assertIn('RegressionReportPolicy.Compose',text)

    def test_baselines_are_actual_gaussian_and_binomial_and_not_goldens(self):
        d=json.loads((OUT/'baseline-registry.json').read_bytes())
        self.assertEqual('NOT_GOLDEN',d['status'])
        self.assertEqual(12,len(d['files']))
        for path,row in d['files'].items():self.assertEqual(row['sha256'],c.sha(ROOT/path))
        self.assertIn('ordinary=Poisson',d['inherited_GLM'])
        for path,pin in d['inherited_04A09_baselines'].items():self.assertEqual(pin,c.sha(ROOT/path))

    def test_postfit_classification_controls_and_chart_order_stay_native(self):
        body=c.prior.members((ROOT/c.FORM).read_text(encoding='utf-8-sig'))['RunGLM']
        self.assertLess(body.index('fitGlm.Fit('),body.index('Dim threshold As Double = GetClassificationThresholdUiValue()'))
        self.assertLess(body.index('Dim classificationOptions = options.Copy()'),body.index('New LinearBinomialClassificationPlan'))
        self.assertLess(body.index('Dim rrClass'),body.index('AddRocResultsAndPlotToClassificationSheet'))
        self.assertLess(body.index('AddRocResultsAndPlotToClassificationSheet'),body.index('New graphics.CalibrationPlot'))

    def test_core_udfs_frameworks_and_accepted_evidence_are_byte_protected(self):
        d=json.loads((OUT/'source-delta.json').read_bytes())
        for path,pin in d['protected_source_pins'].items():self.assertEqual(pin,c.sha(ROOT/path),path)
        for project in ['BESHStat.Core','BESHStat.Excel.Common']:
            text=(ROOT/project/(project+'.vbproj')).read_text()
            self.assertIn('<TargetFramework>netstandard2.0</TargetFramework>',text)

    def test_shipping_net48_behavioral_test_inclusions(self):
        text=(ROOT/'BESHStatNG.Test/BESHStatNG.Tests.vbproj').read_text(encoding='utf-8-sig')
        for name in ['Regression10AdapterTests.vb','Regression10Baselines.vb','Regression10PolicyTests.vb']:
            self.assertEqual(1,text.count('Compile Include="'+('src\\' if 'Adapter' in name else '..\\BESHStat.Excel.Common.Tests\\src\\')+name+'"'))

if __name__=='__main__':unittest.main()
