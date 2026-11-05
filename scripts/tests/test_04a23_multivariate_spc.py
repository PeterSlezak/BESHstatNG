"""04A.23 shipping consumers, source parity, authority and publication contracts."""
import hashlib
import importlib.util
import json
from pathlib import Path
import re
import subprocess
import unittest

ROOT=Path(__file__).resolve().parents[2]
OUT=ROOT/'evidence/04A23'
FORM='BESHstatNG/src/UI/Ui22MultivariateControlCharts.vb'
BASE='519e875a329111270685a0ae4be8666abb55ae04'
POLICIES=['SpcMultivariatePointMapPolicy','SpcMultivariatePhaseExclusionPolicy','SpcMultivariateRequestPolicy','SpcMultivariateResultAxisPolicy']

def read(p):return (ROOT/p).read_text(encoding='utf-8-sig')
def doc(p):return json.loads((OUT/p).read_bytes())
def sha(p):return hashlib.sha256((ROOT/p).read_bytes()).hexdigest()
def methods(text):return {m.group(2):m.group() for m in re.finditer(r'^    (?:Private|Public|Friend) (?:Shared )?(Sub|Function) (\w+)\b.*?^    End \1',text,re.M|re.S)}
def module(name,path):
    spec=importlib.util.spec_from_file_location(name,ROOT/path);m=importlib.util.module_from_spec(spec);spec.loader.exec_module(m);return m

class Spc23Contracts(unittest.TestCase):
    def test_actual_merged_base_and_protected_owner_plan(self):
        e=doc('entry-state.json')
        self.assertEqual(BASE,e['base'])
        self.assertEqual(dict(P='NO',N1='NO GO',N2='NO',C2='UNMET'),e['gates'])
        p=doc('owner-plan-observation.json')
        self.assertEqual('2.9',p['revision'])
        self.assertEqual(p['working_sha256'],sha(p['path']))

    def test_real_build_request_consumes_portable_choices_map_and_expansion(self):
        m=methods(read(FORM));b=m['BuildRequest']
        for value in ['SpcMultivariateModelIntent','SpcMultivariateOptions','SpcMultivariateRequestPolicy.Create','context.PointMap.Expand','BuildPointAssignments(context.PointCount)','BuildPointExclusions(context.PointCount)']:
            self.assertIn(value,b)
        self.assertNotIn('New SpcMultivariateRequest(',b)
        self.assertIn('SpcMultivariatePointMapPolicy.FromWorksheetRows',m['GetInputRowContext'])
        self.assertIn('context.PointMap.NormalizeGroupedMetadata',m['NormalizeGroupedMetadata'])

    def test_one_core_fit_and_cancellation_busy_error_flow_are_exact(self):
        source=read(FORM);before=doc('prechange-method-provenance.json')['methods'];after=methods(source)
        self.assertEqual(1,source.count('SpcMultivariate.Fit('))
        self.assertIn('SpcMultivariate.Fit(request, AddressOf CancellationRequested)',source)
        for name in ['btCompute_Click','CancellationRequested','BeginComputation','EndComputation','ValidateSelections']:
            self.assertEqual(before[name]['body'],after[name],name)

    def test_native_workbook_order_warning_and_appearance_are_exact(self):
        before=doc('prechange-method-provenance.json')['methods'];after=methods(read(FORM))
        for name in ['WriteResultsToNewWorkbook','BuildAppearanceOptions','WriteResultTables','PrepareOutputSheet','AutoFitOutputSheet']:
            if name in before:self.assertEqual(before[name]['body'],after[name],name)
        body=after['WriteResultsToNewWorkbook']
        order=['Multivariate Summary','Model Details','Multivariate Chart','Chart Data','Signals','Diagnostics','Settings and Audit']
        self.assertEqual(order,sorted(order,key=body.index))
        self.assertIn('Catch ex As Exception',body)
        self.assertIn('ex.Message',body)
        self.assertIn('graphics.SpcControlChartExcel.AddCharts',body)

    def test_native_cell_conversion_and_historical_grid_read_are_exact(self):
        before=doc('prechange-method-provenance.json')['methods'];after=methods(read(FORM))
        for name in ['BuildHistoricalModel','RequiredIntegerCell','RequiredDoubleCell','CellText','GridRowIsBlank','TryParseDouble','ReadNumericColumn','ReadTextColumn','ReadObjectColumn','NormalizeWorksheetValue','ParseExclusionScope','IsExclusionIndicator']:
            self.assertEqual(before[name]['body'],after[name],name)
        self.assertIn('SelectedChartType() <> SpcChartType.GeneralizedVariance',after['BuildHistoricalModel'])

    def test_stage_and_exclusion_validation_interleave_with_native_parsing(self):
        m=methods(read(FORM));stage=m['BuildPointAssignments'];exclusion=m['BuildPointExclusions']
        order=['builder.BeginStage(id)','RequiredIntegerCell(row, colStageFirstPoint','RequiredIntegerCell(row, colStageLastPoint','builder.ValidateRange','ParsePhaseText','builder.Assign','builder.Finish']
        self.assertEqual(order,sorted(order,key=stage.index))
        order=['RequiredIntegerCell','builder.CoreIndex','builder.IncludeScope','ParseExclusionScope','builder.IncludeReason','builder.Finish']
        self.assertEqual(order,sorted(order,key=exclusion.index))
        self.assertIn('StringComparer.OrdinalIgnoreCase',read('BESHStat.Excel.Common/src/Analysis/SpcMultivariatePhaseExclusionPolicy.vb'))

    def test_quick_import_chronology_and_lazy_partial_stage_rows_remain(self):
        m=methods(read(FORM))
        quick=m['btApplyQuickPhaseSetup_Click'];self.assertLess(quick.index('dgvStages.Rows.Clear'),quick.index('SpcMultivariatePhasePolicy.Quick'))
        imported=m['btImportStages_Click']
        self.assertLess(imported.index('CollapseImported'),imported.index('dgvStages.Rows.Clear'))
        self.assertLess(imported.index('dgvStages.Rows.Clear'),imported.index('ImportedRows'))
        p=read('BESHStat.Excel.Common/src/Analysis/SpcMultivariatePhaseExclusionPolicy.vb')
        self.assertIn('Shared Iterator Function ImportedRows',p)
        self.assertIn('If Not used.Add(id) Then Throw',p)
        self.assertIn('StringComparison.Ordinal)',p)

    def test_result_context_is_consumed_and_diagnostic_table_tail_is_exact(self):
        m=methods(read(FORM));old=doc('prechange-method-provenance.json')['methods']
        for name in ['BuildChartRenderingResult','BuildDiagnosticTables']:self.assertIn('SpcMultivariateResultAxisPolicy.Project(result)',m[name])
        self.assertIn('SpcMultivariateResultAxisPolicy.RenderingInput(panels(0))',m['BuildChartRenderingResult'])
        self.assertIn('context.SelectDiagnostics(signalOnly)',m['BuildDiagnosticTables'])
        self.assertEqual(old['BuildDiagnosticTables']['body'].split('        Dim names As',1)[1],m['BuildDiagnosticTables'].split('        Dim names As',1)[1])
        self.assertEqual(old['BuildChartRenderingResult']['body'].split('        Dim title As',1)[1],m['BuildChartRenderingResult'].split('        Dim title As',1)[1])
        panel_loop=lambda text:text.split('        Dim panels(',1)[1].split('        Next',1)[0]
        self.assertEqual(panel_loop(old['BuildChartRenderingResult']['body']),panel_loop(m['BuildChartRenderingResult']))
        p=read('BESHStat.Excel.Common/src/Analysis/SpcMultivariateResultAxisPolicy.vb')
        for token in ['panel.GetPoint(diagnostic.PointIndex)','point.IsSignalled','If(rows.Length > 0, rows(0), points(i).PointIndex)','points(i).SequenceValue','points(i).Label']:self.assertIn(token,p)

    def test_existing_tables_and_warnings_remain_byte_exact_methods(self):
        m=methods(read(FORM));old=doc('prechange-method-provenance.json')['methods']
        for name in ['BuildSummaryTables','BuildModelTables','BuildChartDataTable','BuildSignalsTable','BuildAuditTables','BuildWarningsTable','CollectWarnings']:
            self.assertEqual(old[name]['body'],m[name],name)

    def test_six_choices_and_ui_enabled_side_effects_are_retained(self):
        source=read(FORM);p=read('BESHStat.Excel.Common/src/Analysis/SpcMultivariateRequestPolicy.vb')
        self.assertEqual(6,len(re.findall(r'New ChartChoice\(',source)))
        for name in ['HotellingT2','GeneralizedVariance','PcaT2','PcaQ','Mewma','Mcusum']:self.assertIn('SpcChartType.'+name,source)
        for name in ['GChart','TChart','LaneyPPrime','LaneyUPrime']:self.assertNotIn('SpcChartType.'+name,source)
        self.assertIn('pAllowPseudoInverseBeforeGeneralizedVariance',source)
        self.assertIn('chkAllowPseudoInverse.Checked = False',source)
        self.assertIn('chkUseLowerHotellingLimit.Checked = False',source)
        self.assertIn('choice.ChartType = SpcChartType.HotellingT2 AndAlso\n            compatibility.UseLowerHotellingLimitAvailable(IsGroupedStructure())',source)
        self.assertIn('If(isMewma, chkSpecifyMewmaControlLimit.Checked, False)',source)
        self.assertIn('If(isGv, chkSpecifyGvSigmaMultiplier.Checked, False)',source)
        for token in ['compatibility.IsPca','compatibility.IsSequential','compatibility.UseLowerHotellingLimitAvailable','compatibility.AlphaEnabled']:self.assertIn(token,source)
        self.assertIn('ChartType = SpcChartType.HotellingT2 AndAlso Not grouped',p)

    def test_portable_contracts_are_strict_host_free_and_have_no_statistical_engine(self):
        for name in POLICIES:
            p=read('BESHStat.Excel.Common/src/Analysis/'+name+'.vb')
            self.assertIn('Option Strict On',p)
            self.assertIn('Option Explicit On',p)
            for token in ['Microsoft.Office','Windows.Forms','ExcelDna','System.IO','Microsoft.VisualBasic','SpcMultivariate.Fit','SpcEngine.Fit','Math.Sqrt','Math.Exp','Math.Log','MatrixInverse','ResultTable(']:self.assertNotIn(token,p,name)
        self.assertIn('<TargetFramework>netstandard2.0</TargetFramework>',read('BESHStat.Excel.Common/BESHStat.Excel.Common.vbproj'))

    def test_frozen_prechange_derivative_and_method_provenance_match(self):
        p=doc('legacy-preparation-provenance.json');original=doc('prechange-method-provenance.json')
        self.assertEqual(p['source_sha256'],original['source_sha256'])
        self.assertEqual(p['derivative_sha256'],sha(p['derivative']))
        for name,h in p['method_hashes'].items():self.assertEqual(h,original['methods'][name]['sha256'])
        legacy=read(p['derivative'])
        for name in POLICIES:self.assertNotIn(name,legacy)
        self.assertIn('New SpcMultivariateRequest(',legacy)

    def test_original_controls_overlap_and_negative_gates_remain_open(self):
        x=doc('original-control-crosswalk.json')
        for name,counts in [('Ui22MultivariateControlCharts',(182,320,154)),('Ui21ControlCharts',(208,226,115))]:
            p=x['populations'][name];self.assertEqual(counts,(p['controls'],p['linked_open_obligations'],p['restricted_original_controls']))
            self.assertEqual(len(set(p['control_ids'])),p['controls'])
        self.assertEqual(0,x['releases']);self.assertEqual(0,x['discharges'])
        self.assertTrue(all(o['status']=='OPEN' for o in x['obligations']))
        candidates=doc('candidate-issues.json')
        self.assertEqual('OPEN / CANDIDATE_ONLY / UNAPPLIED',candidates['C21-STACKED-ROWCOUNT-VALIDATION-01'])
        self.assertEqual(0,candidates['activated_behavior_fixes'])

    def test_all_frozen_sources_authorities_and_accepted_renderer_hashes_are_unchanged(self):
        for path,row in doc('frozen-capture-hashes.json')['files'].items():self.assertEqual(row['sha256'],sha(path),path)
        for path,h in doc('entry-state.json')['protected_prior_implementation'].items():self.assertEqual(h,sha(path),path)
        inventory=doc('prechange-source-inventory.json')['files']
        for path,row in inventory.items():
            if path.startswith('BESHStat.Core/') or 'Designer.vb' in path or 'Ribbon' in path or '/UDF' in path:self.assertEqual(row['sha256'],sha(path),path)

    def test_comparison_payloads_have_real_six_chart_results_and_explicit_rejections(self):
        x=doc('multivariate-spc-same-core-comparisons-v1.json')
        self.assertEqual('NOT_GOLDEN',x['status']);self.assertFalse(x['promoted'])
        self.assertEqual(len(x['records']),x['record_count'])
        charts=set()
        for key,p in x['payloads'].items():
            self.assertEqual(key,hashlib.sha256(json.dumps(p,sort_keys=True,ensure_ascii=False,separators=(',',':')).encode()).hexdigest())
            if p['outcome']=='POSITIVE_PARITY':
                charts.add(p['chart']);self.assertTrue(p['result']['panels']);self.assertTrue(p['request_sha256'])
            else:self.assertIn('exception',p)
        self.assertEqual(6,len(charts))
        self.assertGreater(x['outcomes']['EXISTING_CORE_REJECTION'],0)
        self.assertGreater(x['outcomes']['EXISTING_PREPARATION_EXCEPTION'],0)

    def test_projects_compile_new_contracts_tests_and_legacy_framework_links(self):
        p=doc('project-compilation-inventory.json')
        self.assertTrue(p['targets_unchanged']);self.assertTrue(p['packages_unchanged'])
        project=read('BESHStatNG.Test/BESHStatNG.Tests.vbproj')
        for name in ['Spc23Tests','Spc23Fixture','Spc23Data','Spc23LegacyPreparation','Spc23AdapterTests']:self.assertIn(name+'.vb',project)
        inventory=doc('test-inventory.json');self.assertGreater(inventory['logical_count'],700)
        self.assertEqual(18,inventory['python_count'])

    def test_help_metadata_fixture_and_capture_availability_are_preserved(self):
        p=doc('documentation-audit.json');self.assertFalse(p['metadata_changed']);self.assertFalse(p['published_captures_changed'])
        self.assertEqual('b3c5c52b1a1c570342b89cbee53045085a4c64eb32084596147afd16c2d75a7c',p['dataset_sha256'])
        page=read(p['page']['path']);self.assertIn('first appearance of an ID',page)
        self.assertIn('Office.js, WASM or Mac',page)
        self.assertIn('only for individual-observation Hotelling T-squared',page)

    def test_actual_publication_membership_hash_privacy_trackability_and_empty_index(self):
        verifier=module('publication23','scripts/verify-04a23-publication.py')
        verifier.verify(check_receipts=False)

if __name__=='__main__':unittest.main()
