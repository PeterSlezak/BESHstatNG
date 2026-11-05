"""04A.25 shipping integration/authority successors. No native or scientific oracle."""
import hashlib
import importlib.util
import json
from pathlib import Path
import re
import subprocess
import unittest
import xml.etree.ElementTree as ET

ROOT=Path(__file__).resolve().parents[2]
OUT=ROOT/'evidence/04A25'
def read(p):return (ROOT/p).read_text(encoding='utf-8-sig')
def sha(p):return hashlib.sha256((ROOT/p).read_bytes()).hexdigest()
def doc(p):return json.loads((OUT/p).read_bytes())
class Psm25SourceContracts(unittest.TestCase):
    @classmethod
    def setUpClass(cls):
        spec=importlib.util.spec_from_file_location('c25_test',ROOT/'scripts/capture-04a25-contract.py');cls.c=importlib.util.module_from_spec(spec);spec.loader.exec_module(cls.c)
        cls.form=read(cls.c.FORM);cls.methods=cls.c.methods(cls.form)
        cls.prechange=doc('prechange-method-provenance.json')['files']
        cls.entry='\n'.join(cls.prechange[cls.c.FORM]['methods'].values())
        cls.source=read('BESHstatNG/src/Graphics/PsmLovePlotExcel.vb')
        cls.importer=read('BESHstatNG/src/DM/PSMdataImport.vb')
        cls.policies=[p for p in (ROOT/'BESHStat.Excel.Common/src/Analysis').glob('Psm*.vb')]
    def test_verified_entry_main_and_actual_merge_ancestry(self):
        entry=doc('entry-state.json')
        self.assertEqual('bd46904dca7f3860d799e2d6d9c069de9f2fb13a',entry['main'])
        self.assertIn('bd46904 merge codex/04A-24: Fast-forward',entry['main_reflog'])
        if (ROOT/'.git').exists():
            self.assertEqual(0,subprocess.run(['git','-c','safe.directory='+ROOT.as_posix(),'merge-base','--is-ancestor',entry['base'],'HEAD'],cwd=ROOT,capture_output=True).returncode)
    def test_core_designer_udf_and_owner_plan_remain_exact_entry_bytes(self):
        pins=doc('source-preservation.json')
        for p,h in pins['protected_sources'].items():self.assertEqual(h,sha(p),p)
        self.assertEqual(pins['plan_sha256'],sha(self.c.PLAN))
    def test_one_shipping_fit_and_original_progress_exception_path(self):
        self.assertEqual(1,self.form.count('PsmComprehensiveBackend.Fit('))
        self.assertIn('PsmComprehensiveBackend.Fit(myData.Input, fitOptions)',self.methods['RunPSM'])
        if self.entry:
            old=self.c.methods(self.entry)
            for n in ['RunPSM','btCalculate_Click','ValidateInputs','ValidateNoDuplicateRoleAssignments','ParseEnum','ParseOptionalDouble','BeginPsmComputation','FinishPsmComputation','EndPsmComputation']:
                self.assertEqual(old[n],self.methods[n],n)
    def test_real_parsed_options_use_snapshot_without_second_validation_grammar(self):
        b=self.methods['BuildFitOptions']
        self.assertIn('New PsmRequestSelection(fitOptions).CreateEffectiveOptions()',b)
        self.assertIn('PsmRequestSelection.CreateCemSpec(bins, standard)',b)
        self.assertIn('Math.Max(1, ParseUiInteger(tbMatchingRatio.Text',b)
        self.assertIn('Math.Max(2, ParseUiInteger(tbStrata.Text',b)
        self.assertIn('ParseOptionalDouble(tbCaliper.Text, Double.NaN)',b)
    def test_current_ui_defaults_and_distinct_core_ridge_are_explicit(self):
        defaults=read('BESHStat.Excel.Common/src/Analysis/PsmRequestSelection.vb')
        for text in ['LogisticRidgePenalty As Double = 0.0000001R','MaximumIterations As Integer = 100','MatchingRatio As Integer = 1','Strata As Integer = 5','CemBins As Integer = 5','OverlapBins As Integer = 20']:
            self.assertIn(text,defaults)
        self.assertIn('LogisticRidgePenalty As Double = 0.000001',read('BESHStat.Core/src/StatTests/PropensityScoreMatching/PSMcore.vb'))
        self.assertIn('FormatUiDouble(PsmSelectionDefaults.LogisticRidgePenalty)',self.methods['InitializeDefaultUiValues'])
    def test_control_intent_uses_core_capabilities_and_native_events_stay_native(self):
        control=read('BESHStat.Excel.Common/src/Analysis/PsmRequestSelection.vb')
        for name in ['UsesMatchingControls','UsesNearestNeighborOptions','UsesOptimalPairOptions','UsesSubclassificationControls','UsesCemControls','SupportsDoublyRobust','SupportedEstimands']:
            self.assertIn('PsmMethodCapabilities.'+name,control)
        body=self.methods['SetOptionControlsForCurrentSelections']
        self.assertIn('New PsmControlIntent(runMethod, estimand, scoreMethod)',body)
        self.assertIn('PsmControlIntent.NeedsCaliperDefault(distanceMetric, caliperScale)',body)
        self.assertIn('If String.IsNullOrWhiteSpace(Me.tbCaliper.Text)',body)
        self.assertIn('If Not Me.chkDoublyRobust.Enabled Then Me.chkDoublyRobust.Checked = False',body)
    def test_both_import_paths_keep_alignment_and_build_common_axis_after_input(self):
        self.assertIn('Public Sub DataImportFromWorksheet',self.importer)
        self.assertIn('Public Sub DataImportFromRawMatrices',self.importer)
        self.assertLess(self.importer.index('Input = New PsmInputData'),self.importer.index('AlignedAxis = New PsmAlignedInputAxis'))
        self.assertIn('out.AlignedAxis = importer.AlignedAxis',self.methods['GetData'])
        old=self.prechange['BESHstatNG/src/DM/PSMdataImport.vb']['methods']
        for name in ['AlignChildDataObjects','IntersectRowIds','SubsetChildToRows','BuildExactLabel']:
            if name in old:self.assertEqual(old[name],self.c.methods(self.importer)[name],name)
    def test_axis_is_observation_only_preserving_source_ids_and_category_roles(self):
        text=read('BESHStat.Excel.Common/src/Analysis/PsmAlignedInputAxis.vb')
        for name in ['SourceRow','Ordinal','EffectiveId','SuppliedScore','HasExactGroups','RawCovariates','Effects','ExpandedCovariates','CandidateDiagnostics']:
            self.assertIn(name,text)
        self.assertNotIn('.Validate(',text)
        self.assertNotIn('.Fit(',text)
        self.assertIn('sourceRowIds.Clone()',text)
    def test_postfit_metadata_uses_stored_core_indices_weights_and_flags(self):
        text=read('BESHStat.Excel.Common/src/Analysis/PsmResultPresentation.vb')
        for member in ['link.SetId','link.FocalRowIndex','link.MatchedRowIndex','link.TreatedRowIndex','link.ControlRowIndex','result.MatchingWeights','result.BalancingWeights','cem.RowStratumIds','observation.IncludedByCommonSupport','observation.IncludedByTrimming']:
            self.assertIn(member,text)
        self.assertNotIn('Compute',text)
        self.assertIn('byRow.TryGetValue',text)
    def test_writer_consumes_ordered_core_table_plan_at_original_checkbox_phases(self):
        body=self.methods['WritePsmResults']
        names=['BuildFitOptions()','CreatePsmResultWorkbook()','initial(0).BuildTables','initial(1).BuildTables','DiagnosticSheets(Me.chkWriteDiagnostics.Checked)','LoveSheets(Me.chkLovePlot.Checked','MatchSheets(Me.chkWriteMatches.Checked)','CemSheets(presentation.HasCem)','resultWs.Activate()']
        self.assertEqual(sorted(body.index(n) for n in names),[body.index(n) for n in names])
        self.assertIn('myData.AlignedAxis.SourceRowIds()',body)
        self.assertIn('DataImportSummaryTable(myData)',body)
    def test_report_plan_delegates_every_table_and_default_sensitivity(self):
        text=read('BESHStat.Excel.Common/src/Analysis/PsmReportSelectionPolicy.vb')
        for name in ['AnalyzedInputDataTable','GeneralResultTables','DiagnosticsTables','RowAuditTables','MatchedPairsTables','MatchedDatasetTables','CoarsenedExactTables']:
            self.assertIn('PsmFormattedResultTables.'+name,text)
        self.assertIn('includeDefaultSensitivity:=True',text)
        for title in ['Input Data','Results','Diagnostics','Row Audit','Love Plot','Matched Pairs','Matched Data','CEM']:
            self.assertIn('"'+title+'"',text)
    def test_workbook_sheet_writer_naming_empty_skips_activation_unchanged(self):
        if self.entry:
            before=self.c.methods(self.entry)
            for n in ['CreatePsmResultWorkbook','CreateOrAddResultSheet','WriteResultTablesToNewSheet','WriteResultTablesToWorksheet','DataImportSummaryTable','MakeUniqueWorksheetName','CleanWorksheetName','WorksheetNameExists']:
                self.assertEqual(before[n],self.methods[n],n)
    def test_love_source_table_and_chart_share_one_frozen_projection(self):
        b=self.methods['WriteLovePlotSheetIfAvailable']
        self.assertEqual(1,b.count('New PsmLovePlotProjection(fitResult.LovePlotRows)'))
        self.assertIn('BuildProjectedDataTable(projection)',b)
        self.assertIn('AddProjectedChart(ws, projection)',b)
        self.assertIn('Love plot chart warning',b)
        self.assertLess(b.index('WriteResultTablesToWorksheet'),b.index('        Try'))
        self.assertLess(b.index('        Try'),b.index('AddProjectedChart'))
    def test_legacy_renderer_public_apis_and_native_geometry_remain(self):
        for declaration in ['BuildPlotDataTable(rows As IList(Of PsmLovePlotRow))','AddChart(ws As Worksheet,','Shapes.AddChart','AddScatterSeries','AddThresholdSeries','AddVariableLabels','.ReversePlotOrder = True']:
            self.assertIn(declaration,self.source)
        self.assertNotIn('OrderByDescending',self.source)
        self.assertNotIn('MaxFiniteAbs',self.source)
        self.assertIn('Dim xBefore = projection.Before',self.source)
    def test_portable_plot_sort_finite_missing_threshold_and_headers_are_explicit(self):
        text=read('BESHStat.Excel.Common/src/Analysis/PsmLovePlotProjection.vb')
        for token in ['OrderByDescending','StringComparer.OrdinalIgnoreCase','FirstOrDefault(Function(v) Finite(v) AndAlso v > 0)','0.1R','0.25R','Threshold * 1.25R','maxValue * 1.15R','Return Double.NaN','Return Nothing']:
            if token=='Return Double.NaN':self.assertIn('Double.NaN',text)
            else:self.assertIn(token,text)
        for h in ['Plot Row','Variable','|SMD| before','|SMD| after matching','|SMD| after weighting','Threshold','Flag']:
            self.assertIn('"'+h+'"',text)
    def test_common_is_strict_host_free_and_not_an_estimator(self):
        self.assertEqual(5,len(self.policies))
        for p in self.policies:
            text=p.read_text()
            self.assertIn('Option Strict On',text,p.name)
            self.assertNotRegex(text,r'(?i)Imports .*?(Windows.Forms|Interop|ExcelDna|System.IO|Microsoft.VisualBasic)|\b(ComboBox|TextBox|ListBox|Worksheet|TermSpec|DataObj)\b|\.Fit\(')
    def test_legacy_derivative_is_source_pinned_and_not_a_scientific_golden(self):
        d=doc('legacy-projection-provenance.json')
        self.assertEqual(d['derivative']['sha256'],sha(d['derivative']['path']))
        self.assertEqual('NOT_GOLDEN / SAME_CORE_COMPATIBILITY_ONLY',d['status'])
        legacy=read(d['derivative']['path'])
        self.assertNotIn('PsmLovePlotProjection',legacy)
        self.assertIn('OrderByDescending',legacy)
    def test_frozen_control_ids_and_obligations_stay_open(self):
        d=doc('original-control-crosswalk.json')
        self.assertEqual(103,len(d['controls']))
        self.assertEqual(54,len(d['obligations']))
        self.assertTrue(all(r['status']=='OPEN' for r in d['obligations']))
        self.assertEqual(0,d['releases']);self.assertEqual(0,d['discharges'])
        self.assertEqual(dict(P='NO',N1='NO GO',N2='NO',C2='UNMET'),d['gates'])
    def test_windows_tests_are_compiled_and_common_targets_are_unchanged(self):
        project=read('BESHStatNG.Test/BESHStatNG.Tests.vbproj')
        for name in ['Psm25Tests','Psm25Fixture','Psm25Legacy','Psm25AdapterTests']:
            self.assertIn(name+'.vb',project)
        for p in ['BESHStat.Core/BESHStat.Core.vbproj','BESHStat.Excel.Common/BESHStat.Excel.Common.vbproj']:
            self.assertIn('<TargetFramework>netstandard2.0</TargetFramework>',read(p))
    def test_runner_is_offline_isolated_bounded_and_starts_python_first(self):
        text=read('scripts/verify-04a25.py')
        for token in ['max_workers=4',"for label in 'DABC'",'MaxCpuCount>1','--no-restore','--test-concurrency=1','BESHSTAT_KR_EXPORT_DIR','fixture_sources','env.pop(\'BESHSTAT_04A02_OBSERVATIONS\'']:
            self.assertIn(token,text)
        self.assertNotIn('max_workers=5',text)

if __name__=='__main__':unittest.main()
