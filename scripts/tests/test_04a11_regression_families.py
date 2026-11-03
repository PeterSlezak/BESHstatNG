"""04A.11 successor source/authority checks; real Core/adapter behavior runs in managed tests."""
import importlib.util
import json
from pathlib import Path
import unittest

ROOT=Path(__file__).resolve().parents[2]
spec=importlib.util.spec_from_file_location('contract11',ROOT/'scripts/capture-04a11-contract.py')
c=importlib.util.module_from_spec(spec);spec.loader.exec_module(c)
OUT=ROOT/'evidence/04A11'

class Regression11SuccessorTests(unittest.TestCase):
    def test_exact_six_form_deltas_and_native_writer_tails(self):
        c.source()
        d=json.loads((OUT/'source-delta-final.json').read_bytes())
        self.assertEqual({'RunOLS','RunGLM','RunGLMNB2','RunZIP','RunOrdLogit','RunMultiLogit'},set(d['changed_members']))
        self.assertIn('CaptureRegressionRequest',d['unchanged_members'])
        self.assertTrue(all(d['writer_tails_equal_after_documented_substitutions'].values()))
        self.assertTrue(d['Core_changes']['estimator_statements_unchanged'])

    def test_all_four_shipping_routes_consume_family_intents_and_documents(self):
        methods=c.prior.members((ROOT/c.FORM).read_text(encoding='utf-8-sig'))
        for name in ['RunGLMNB2','RunZIP','RunOrdLogit','RunMultiLogit']:
            text=methods[name]
            for token in ['ExtendedRegressionFamilyPolicy.Project(request)','intent.Validation','intent.Options','WindowsRegressionRequestAdapter.Report(intent, res,','res = report.LegacyTables()']:
                self.assertIn(token,text)
            self.assertLess(text.index('res = report.LegacyTables()'),text.index('AppGlobals.app.Workbooks.Add'))
        zip_body=methods['RunZIP']
        for token in ['intent.Primary.Data','intent.Logistic.Data','intent.Primary.Names','intent.Logistic.Names','request.Logistic.InitialValues','intent.LogisticIntercept']:
            self.assertIn(token,zip_body)

    def test_native_alpha_and_data_header_activations_remain_dormant(self):
        methods=c.prior.members((ROOT/c.FORM).read_text(encoding='utf-8-sig'))
        self.assertIn('LinearFitAlphaMode.NativeLegacyDefault',methods['RunOLS'])
        self.assertNotIn('lm.Alpha =',methods['RunOLS'])
        for name in ['RunGLM','RunGLMNB2']:self.assertIn('DeferredCovarianceDataHeaders',methods[name])
        patch=(OUT/'C11-NATIVE-CORRECTIONS-01.final.UNAPPLIED.patch').read_text()
        self.assertIn('+        lm.Alpha = intent.EffectiveFitAlpha',patch)
        self.assertIn('+        Dim dataHeaders = DirectCast(fitVarNames.Clone(), String())',patch)
        self.assertNotIn('CaptureRegressionRequest(kind',patch)
        self.assertEqual('aab350401c873d1a4f56450295688e9a046122aca2269fc9a618765f8fe79fe1',c.sha(ROOT/'evidence/04A09/strict-validation-activation.UNAPPLIED.patch'))

    def test_original_control_and_obligation_ids_hashes_and_gates_retained(self):
        d=json.loads((OUT/'restriction-crosswalk.json').read_bytes())
        parent=json.loads((ROOT/d['parent_path']).read_bytes())
        self.assertEqual(parent['controls'],d['controls'])
        self.assertEqual((34,7,0,0,'NO','NO GO','NO','UNMET'),tuple(d[k] for k in ['associated_controls','associated_obligations','released','discharged','P','N1','N2','C2']))
        methods=c.prior.members((ROOT/c.FORM).read_text(encoding='utf-8-sig'))
        self.assertNotRegex(methods['CaptureRegressionRequest'],r'If .*?(?:Blocking|IsValid)|Throw')

    def test_unaffected_sources_and_all_old_evidence_are_byte_protected(self):
        d=json.loads((OUT/'source-delta-final.json').read_bytes())
        for path,pin in d['protected_source_pins'].items():self.assertEqual(pin,c.sha(ROOT/path),path)
        for path,row in d['changed_existing'].items():self.assertEqual(row['after'],c.sha(ROOT/path),path)

    def test_common_has_no_host_solver_or_localized_numeric_reparse(self):
        for name in ['Analysis/ExtendedRegressionFamilyPolicy.vb','Results/ExtendedRegressionReport.vb']:
            text=(ROOT/'BESHStat.Excel.Common/src'/name).read_text()
            self.assertIn('Option Strict On',text)
            self.assertNotRegex(text,r'(?i)Imports .*?(Windows.Forms|Interop|ExcelDna|System.IO|Microsoft.VisualBasic)|\.Fit\(|Double.Parse|CDbl\(|writeToSheet')
        for project in ['BESHStat.Core','BESHStat.Excel.Common']:
            self.assertIn('<TargetFramework>netstandard2.0</TargetFramework>',(ROOT/project/(project+'.vbproj')).read_text())

    def test_prechange_real_core_and_defect_capture_registry(self):
        d=json.loads((OUT/'baseline-registry.json').read_bytes())
        self.assertEqual('NOT_GOLDEN',d['status']);self.assertEqual(18,len(d['files']))
        for path,row in d['files'].items():self.assertEqual(row['sha256'],c.sha(ROOT/path))
        before=(OUT/'baselines/defects-before.txt').read_text()
        for token in ['requested=0.1 actual=0.05','after=Intercept|x1|x2','System.NullReferenceException']:self.assertIn(token,before)

    def test_help_slugs_metadata_and_truthful_office_availability(self):
        for page in c.PAGES:
            text=(ROOT/'BESHStatNG_Help_MkDocs/docs/methods'/(page+'.md')).read_text(encoding='utf-8-sig')
            if page not in ['generalized-linear-models-glm','multiple-linear-regression-lm']:
                self.assertIn('Office add-in availability',text)
                self.assertIn('not currently executable',text)
            self.assertNotIn('## Using the Office add-in (Excel Online)',text)
        d=json.loads((OUT/'source-delta-final.json').read_bytes())
        self.assertIn('BESHStatNG_Help_MkDocs/docs/_meta/methods.yml',d['protected_source_pins'])

    def test_compile_inclusions_and_core_defensive_scope(self):
        project=(ROOT/'BESHStatNG.Test/BESHStatNG.Tests.vbproj').read_text(encoding='utf-8-sig')
        for name in ['Regression11Baselines.vb','Regression11PolicyTests.vb','Regression11DefectTests.vb','Regression11AdapterTests.vb']:self.assertIn(name,project)
        text=(ROOT/c.CORE).read_text(encoding='utf-8-sig')
        self.assertIn('Me.pVarNames.Clone()',text)
        self.assertIn('If Not Me.HasReportData Then',text)
        self.assertIn('Hosmer-Lemeshow test unavailable; no statistic or p-value was produced.',text)

if __name__=='__main__':unittest.main()
