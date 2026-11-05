"""04A.26 shipping successors; source characterization is not native verification."""
import hashlib
import importlib.util
import json
from pathlib import Path
import re
import unittest

ROOT = Path(__file__).resolve().parents[2]
OUT = ROOT / 'evidence/04A26'
def read(path): return (ROOT / path).read_text(encoding='utf-8-sig')
def sha(path): return hashlib.sha256((ROOT / path).read_bytes()).hexdigest()
def doc(path): return json.loads((OUT / path).read_bytes())

class PlanningAgreement26Contracts(unittest.TestCase):
    @classmethod
    def setUpClass(cls):
        spec = importlib.util.spec_from_file_location('capture26_tests', ROOT / 'scripts/capture-04a26-contract.py')
        cls.c = importlib.util.module_from_spec(spec); spec.loader.exec_module(cls.c)
        cls.before = doc('prechange-method-provenance.json')['files']
        cls.current = {p: cls.c.methods(read(p)) for p in cls.before}
        cls.planning = cls.current[cls.c.FORM]
        cls.two = cls.current['BESHstatNG/src/UI/UiTwoInputRefedits.vb']
        cls.bland = cls.current['BESHstatNG/src/UI/Ui9ANOVA2nested.vb']
        cls.icc = cls.current['BESHstatNG/src/UI/Ui0OneRefeditMulticol.vb']
    def test_actual_merged_base_and_empty_index(self):
        entry = doc('entry-state.json')
        self.assertEqual('97d54617703287f20886d82bc2896bd95e475770', entry['main'])
        self.assertIn('97d5461 merge codex/04A-25: Fast-forward', entry['main_reflog'])
        self.assertEqual('', self.c.git('diff','--cached','--name-only'))
    def test_core_udf_designer_bytes_remain_frozen(self):
        for p, h in doc('source-preservation.json')['protected_sources'].items(): self.assertEqual(h, sha(p), p)
    def test_old_paired_and_import_transport_authority_unchanged(self):
        pins = doc('prechange-source-inventory.json')['files']
        paths = [p for p in pins if any(x in p for x in ['PairedBlockInputPolicy.vb','PairedOptionPolicy.vb','WindowsPairedInputAdapter.vb','ExcelDnaDataImporter.vb','AgreementPlotExcel.vb'])]
        self.assertGreaterEqual(len(paths), 4)
        for p in paths: self.assertEqual(pins[p], sha(p), p)
    def test_every_unrelated_form_member_preserved(self):
        allowed = {
            self.c.FORM: {'RunPTtest','RunUPTtest','RunSingleProp','RunIndependentProp','RunLogRank','RunCox','RunICC','RunBlandAltman','New'},
            'BESHstatNG/src/UI/UiTwoInputRefedits.vb': {'getData','getPairedCategoricalData','RunDeming','RunLinsCCC','RunCohensKappa'},
            'BESHstatNG/src/UI/Ui9ANOVA2nested.vb': {'getDataPB','RunBlandAltman'},
            'BESHstatNG/src/UI/Ui0OneRefeditMulticol.vb': {'getDataICC11','RunICC'}}
        for p, before in self.before.items():
            for name, h in before['method_sha256'].items():
                if name not in allowed[p]: self.assertEqual(h, hashlib.sha256(self.current[p][name].encode()).hexdigest(), (p,name))
    def test_all_eight_real_planning_routes_consume_request_report(self):
        for name in ['RunPTtest','RunUPTtest','RunSingleProp','RunIndependentProp','RunLogRank','RunCox','RunICC','RunBlandAltman']:
            body = self.planning[name]
            self.assertIn('New SampleSizePlanningRequest(', body)
            self.assertIn('SampleSizePlanningPolicy.Calculate(', body)
            self.assertIn('tbOutput.AppendText(', body)
            self.assertNotIn('SampleSizeCalculator.', body)
    def test_thirteen_core_calls_single_selected_branch(self):
        policy = read('BESHStat.Excel.Common/src/Analysis/SampleSizePlanningPolicy.vb')
        self.assertEqual(13, policy.count('SampleSizeCalculator.Calculate'))
        self.assertEqual(13, policy.count('Case SampleSizePlanningMethod.'))
        self.assertNotIn('SampleSizeCalculator.', read(self.c.FORM))
    def test_planning_parsing_validation_dispatch_and_save_are_original(self):
        old = self.before[self.c.FORM]['method_sha256']
        for name in ['CheckData','btCompute_Click','btnSaveToSheet_Click']:
            self.assertEqual(old[name], hashlib.sha256(self.planning[name].encode()).hexdigest())
        self.assertIn('CInt(', self.planning['RunICC'])
    def test_signed_margins_and_optional_cox_sentinels_retained(self):
        policy = read('BESHStat.Excel.Common/src/Analysis/SampleSizePlanningPolicy.vb')
        self.assertIn('Math.Abs(', policy)
        self.assertIn('Double.NaN', self.planning['RunCox'])
        self.assertIn('ParseOptionalUiDouble(Me.tbCustom4.Text, 0.0)', self.planning['RunCox'])
        self.assertIn('SelectedComboValue()', self.planning['RunCox'])
    def test_text_projection_has_explicit_crlf_and_no_new_formatting(self):
        text = read('BESHStat.Excel.Common/src/Analysis/SampleSizeTextProjection.vb')
        self.assertIn('PlanningTextNewLine.Value', text)
        self.assertNotIn('InvariantCulture', text)
        self.assertNotIn('Math.Round', text)
        req = read('BESHStat.Excel.Common/src/Analysis/SampleSizePlanningRequest.vb')
        self.assertIn('Convert.ToChar(13)', req)
        self.assertIn('Convert.ToChar(10)', req)
    def test_all_five_agreement_handlers_use_real_report_plans(self):
        for body in [self.two[n] for n in ['RunDeming','RunLinsCCC','RunCohensKappa']] + [self.bland['RunBlandAltman'], self.icc['RunICC']]:
            self.assertIn('AgreementReportPlan(', body)
            self.assertIn('TablesForWriter()', body)
            self.assertIn('NativeRowSpan(rr.TotRows)', body)
            self.assertIn('AgreementInputAxis', body) if body is self.icc['RunICC'] else None
    def test_deming_variance_and_ci_execution_intent_consumed(self):
        body = self.two['RunDeming']
        self.assertIn('DemingRequestSelection.VarianceForIndex(', body)
        for kind, call in [('AnalyticalMcr','DemingAnalyticalCI_MCR'),('AnalyticalClosedForm','DemingAnalyticalCI'),('Jackknife','FitJackknifeCI')]:
            self.assertIn('selection.Execution = DemingExecutionIntent.' + kind, body)
            self.assertIn(call, body)
        self.assertIn('New DemingRequestSelection(', body)
    def test_sd_refedit_conversion_messages_boundary_unchanged(self):
        name = 'GetDemingStandardDeviationVector'
        old = self.before['BESHstatNG/src/UI/UiTwoInputRefedits.vb']['method_sha256'][name]
        self.assertEqual(old, hashlib.sha256(self.two[name].encode()).hexdigest())
        self.assertEqual(2, self.two['RunDeming'].count('GetDemingStandardDeviationVector('))
    def test_lin_kappa_resampling_and_no_kappa_plot(self):
        for name, selection in [('RunLinsCCC','LinRequestSelection'),('RunCohensKappa','KappaRequestSelection')]:
            self.assertIn('New '+selection+'(', self.two[name])
            self.assertIn('tt.Fit(New AppInfrastructure.WinFormsProgressReporter(', self.two[name])
        self.assertNotIn('AddPlot(', self.two['RunCohensKappa'])
        self.assertIn('presentation.PlotRequested', self.two['RunLinsCCC'])
    def test_categorical_header_and_charcols_and_source_rows_retained(self):
        cat = self.two['getPairedCategoricalData']
        self.assertIn('CharCols:=1', cat)
        self.assertIn('If(Me.ckFirstRow.Checked, 1, 0)', cat)
        self.assertIn('SkipRow:=iStart', cat)
        self.assertIn('columData.RowIds', cat)
        self.assertIn('categoricalFirst:=', cat)
        self.assertIn('catData.axis.FirstCategories()', self.two['RunCohensKappa'])
    def test_bland_composes_prior_paired_policy_and_single_fit(self):
        body = self.bland['RunBlandAltman']
        for token in ['PairedOptionPolicy.ProjectBland','PairedOptionPolicy.ValidateBland','New BlandRequestSelection(', 'New AgreementInputAxis(', 'AgreementReportPlan.AppendDecisionTables']:
            self.assertIn(token, body)
        self.assertEqual(1, body.count('ba.Fit()'))
        for token in ['AssessAllowableBias(', 'AssessBlandAltmanAgainstDecisionLimits(']: self.assertEqual(1, body.count(token))
    def test_bland_and_standard_native_spacing_stay_distinct(self):
        plan = read('BESHStat.Excel.Common/src/Analysis/AgreementReportPlan.vb')
        self.assertIn('tableRows + tables.Length + tables.Length - 1', plan)
        self.assertIn('tableRows + tables.Length - 1', plan)
        self.assertIn('New List(Of ResultTable)(tables)', plan)
    def test_icc_six_calls_and_native_acquisition_stay_original(self):
        body = self.icc['RunICC']
        for token in ['ICC11(', 'ICC1k(', 'ICC21(', 'ICC2k(', 'ICC31(', 'ICC3k(']: self.assertEqual(1, body.count('icc.'+token), token)
        self.assertIn('New IccRequestIntent(', body)
        self.assertIn('intent.Label(data2.X.GetLength(1))', body)
        self.assertNotIn('AddPlot(', body)
    def test_new_portable_code_is_strict_and_host_free(self):
        files = [p for p in (ROOT/'BESHStat.Excel.Common/src/Analysis').glob('*.vb') if p.name.startswith(('Agreement','SampleSize'))]
        self.assertEqual(8, len(files))
        for p in files:
            text = p.read_text(encoding='utf-8-sig')
            self.assertIn('Option Strict On', text)
            for token in ['System.Windows.Forms','Microsoft.Office.Interop','ExcelDna','DataObj','System.IO','Microsoft.VisualBasic']:
                self.assertNotIn(token, text, p.name)
    def test_new_tests_linked_without_altering_old_project_references(self):
        import xml.etree.ElementTree as ET
        ns = {'m':'http://schemas.microsoft.com/developer/msbuild/2003'}
        before = ET.fromstring((ROOT/'TestResults/04A-26/run01/preimages/BESHStatNG.Tests.vbproj').read_bytes())
        after = ET.fromstring((ROOT/'BESHStatNG.Test/BESHStatNG.Tests.vbproj').read_bytes())
        encode = lambda root: [ET.tostring(n) for n in root.findall('.//m:ProjectReference', ns)]
        self.assertEqual(encode(before), encode(after))
        self.assertEqual(7, sum('26' in n.attrib['Include'] for n in after.findall('.//m:Compile', ns)))
    def test_original_controls_gates_and_no_assurance_invention(self):
        cross = doc('original-control-crosswalk.json')
        self.assertEqual({'P':'NO','N1':'NO GO','N2':'NO','C2':'UNMET'}, cross['gates'])
        self.assertEqual(0, cross['releases']); self.assertEqual(0, cross['discharges'])
        self.assertEqual(187, cross['counts']['host_controls']); self.assertEqual(30, cross['counts']['linked_open_obligations'])
        policy = read('BESHStat.Excel.Common/src/Analysis/SampleSizePlanningPolicy.vb')
        self.assertNotRegex(policy, r'(?i)(Calculate|Solve)Assurance')

if __name__ == '__main__': unittest.main()
