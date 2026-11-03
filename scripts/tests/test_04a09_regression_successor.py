"""Version-aware additive regression gates; managed tests supply behavioral evidence."""
import hashlib
import importlib.util
import json
from pathlib import Path
import re
import subprocess
import unittest

ROOT=Path(__file__).resolve().parents[2]
spec=importlib.util.spec_from_file_location('contract09',ROOT/'scripts/capture-04a09-contract.py')
c=importlib.util.module_from_spec(spec);spec.loader.exec_module(c)
OUT=ROOT/'evidence/04A09'
def sha(p):return hashlib.sha256((ROOT/p).read_bytes()).hexdigest()

class Regression09SuccessorTests(unittest.TestCase):
    def test_exact_six_changed_members_and_unchanged_native_surroundings(self):
        data=json.loads((OUT/'source-delta-final.json').read_bytes())
        self.assertEqual({'RunOLS','RunGLM','RunGLMNB2','RunZIP','RunOrdLogit','RunMultiLogit'},set(data['modified_form_members']))
        self.assertEqual(['CaptureRegressionRequest'],data['added_form_members'])
        now=c.members((ROOT/c.FORM).read_text(encoding='utf-8-sig'))
        for name,pin in data['unchanged_form_members'].items():self.assertEqual(pin,hashlib.sha256(now[name].encode()).hexdigest(),name)
        for row in data['modified_existing']:self.assertEqual(row['after_sha256'],sha(row['path']))
        for p,h in data['protected_source_pins'].items():self.assertEqual(h,sha(p),p)
        self.assertTrue(all(data['writer_tails_equal_after_only_documented_substitutions'].values()))
        if (ROOT/'.git').exists():
            result=subprocess.run(['git','-c','safe.directory='+ROOT.as_posix(),'merge-base','--is-ancestor',c.BASE,'HEAD'],cwd=ROOT,capture_output=True)
            self.assertEqual(0,result.returncode,'The versioned source pins must descend from verified main; ZIP validation uses the same source hashes.')

    def test_real_six_routes_use_common_options_counts_and_reports(self):
        methods=c.members((ROOT/c.FORM).read_text(encoding='utf-8-sig'))
        for method in ['RunOLS','RunGLM','RunGLMNB2','RunZIP','RunOrdLogit','RunMultiLogit']:
            body=methods[method]
            self.assertIn('CaptureRegressionRequest(RegressionAnalysisKind.',body)
            self.assertIn('WindowsRegressionRequestAdapter.Report(request, res,',body)
            self.assertIn('options.Covariance',body)
            self.assertIn('options.Residuals',body)
            self.assertIn('report.Rows.Residuals',body)
            self.assertLess(body.index('WindowsRegressionRequestAdapter.Report'),body.index('AppGlobals.app.Workbooks.Add'))
            if method!='RunOLS':self.assertIn('requestValidation.PrimaryParameterCount',body)
        self.assertIn('requestValidation.LogisticParameterCount',methods['RunZIP'])
        self.assertIn('TermSpecsLogistic',methods['CaptureRegressionRequest'])
        self.assertIn('Legacy form/Core validation remains authoritative', (ROOT/c.FORM).read_text(encoding='utf-8-sig'))

    def test_common_is_strict_host_neutral_and_not_a_parallel_solver(self):
        for p in ['BESHStat.Excel.Common/src/Analysis/RegressionRequestPolicy.vb','BESHStat.Excel.Common/src/Analysis/RegressionClassificationPolicy.vb','BESHStat.Excel.Common/src/Results/RegressionReportPolicy.vb']:
            text=(ROOT/p).read_text()
            self.assertIn('Option Strict On',text)
            self.assertNotRegex(text,r'(?i)Imports .*?(?:Windows.Forms|Interop|ExcelDna|System.IO|Microsoft.VisualBasic)|\b(?:ListBox|Worksheet|ExcelReference|ExcelError)\b|\.Fit\(')
        classification=(ROOT/'BESHStat.Excel.Common/src/Analysis/RegressionClassificationPolicy.vb').read_text()
        for name in ['ValidateBinaryInputs','ComputeBinarySummary','BuildThresholdTable','BuildCalibrationBins','ComputeBrierScore','WrapResults']:self.assertIn('BinaryClassificationCore.'+name,classification)
        self.assertNotIn('sumY',classification)

    def test_exact_original_rows_and_no_false_release(self):
        data=json.loads((OUT/'restriction-crosswalk-v2.json').read_bytes())
        self.assertEqual((34,7,0,0,'NO','NO GO','NO','UNMET'),tuple(data[k] for k in ['associated_controls','associated_obligations','released','discharged','P','N1','N2','C2']))
        raw=json.loads((ROOT/c.LEDGER).read_bytes()) if (ROOT/c.LEDGER).exists() else None
        for row in data['controls']:
            self.assertEqual('BLOCKED',row['native'])
            if row['obligation_id']:
                self.assertEqual('OPEN',row['status'])
                if raw is not None:
                    original=next(r for r in raw if r['obligation_id']==row['obligation_id'])
                    self.assertEqual(row['obligation_row_sha256'],hashlib.sha256(json.dumps(original,sort_keys=True,ensure_ascii=False).encode()).hexdigest())
        for p,h in data['authorities'].items():
            if (ROOT/p).exists():self.assertEqual(h,sha(p))
        self.assertEqual('C09-NATIVE-STRICT-01',data['owner_disposition']['id'])

    def test_frozen_tests_numerics_and_public_04a08_evidence_preserved(self):
        data=json.loads((OUT/'historical-source-protection.json').read_bytes())
        for p,h in data['pins'].items():self.assertEqual(h,sha(p),p)
        baseline=json.loads((OUT/'baseline-manifest.json').read_bytes())
        for p,h in baseline['pins'].items():
            if p.startswith('BESHStat.Core/'):self.assertEqual(h,sha(p),p)
        self.assertEqual('netstandard2.0',re.search(r'<TargetFramework>(.*?)</TargetFramework>',(ROOT/'BESHStat.Excel.Common/BESHStat.Excel.Common.vbproj').read_text())[1])

    def test_runtime_baselines_are_incremental_and_complete(self):
        data=json.loads((OUT/'runtime-baselines.json').read_bytes())
        self.assertEqual(24,len(data['files']))
        for name,row in data['files'].items():self.assertEqual(row['sha256'],sha('evidence/04A09/baselines/'+name))
        self.assertFalse(data['native'])
        self.assertIn('after bridge edits',data['net48'])

    def test_original_facade_and_classification_chart_boundary(self):
        data=json.loads((OUT/'source-delta-final.json').read_bytes())
        self.assertIn('BuildBinaryCrosstabUdfOutput',data['existing_facade_members_unchanged'])
        source=(ROOT/c.FORM).read_text(encoding='utf-8-sig')
        self.assertIn('BinaryClassificationReporting.ComposeGlmReport',source)
        self.assertIn('ComputeWeightedEventRate(y, weights)',source)
        self.assertIn('post-fit control-read boundary',source)
        self.assertIn('AddRocResultsAndPlotToClassificationSheet',source)
        self.assertIn('New graphics.CalibrationPlot(calibrationRows)',source)
        bridge=(ROOT/'BESHstatNG/src/DM/WindowsRegressionRequestAdapter.vb').read_text()
        self.assertIn('GetPhysicalImportEvidence(raw.CoreTable)',bridge)
        self.assertNotRegex(bridge,r'\b(?:ListBox|Control|Worksheet|Application\.DoEvents)\b')

    def test_legacy_compile_inclusions_and_managed_behavioral_seams_exist(self):
        project=(ROOT/'BESHstatNG/BESHStatNG.vbproj').read_text(encoding='utf-8-sig')
        self.assertEqual(1,project.count('Compile Include="src\\DM\\WindowsRegressionRequestAdapter.vb"'))
        tests=(ROOT/'BESHStatNG.Test/BESHStatNG.Tests.vbproj').read_text(encoding='utf-8-sig')
        for name in ['Regression09AdapterTests.vb','Regression09Baselines.vb','Regression09PolicyTests.vb']:self.assertIn(name,tests)

if __name__=='__main__':unittest.main()
