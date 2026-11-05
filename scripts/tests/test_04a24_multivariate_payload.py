"""04A.24 functional-successor source/evidence contracts; no native execution."""
import hashlib
import importlib.util
import json
from pathlib import Path
import re
import unittest
import xml.etree.ElementTree as ET

ROOT=Path(__file__).resolve().parents[2]
def read(p):return (ROOT/p).read_text(encoding='utf-8-sig')
def doc(p):return json.loads((ROOT/'evidence/04A24'/p).read_bytes())
def sha(p):return hashlib.sha256((ROOT/p).read_bytes()).hexdigest()
def module(name,p):
    s=importlib.util.spec_from_file_location(name,ROOT/p);m=importlib.util.module_from_spec(s);s.loader.exec_module(m);return m

class MultivariatePayload24(unittest.TestCase):
    @classmethod
    def setUpClass(cls):
        cls.c=module('capture24_test','scripts/capture-04a24-contract.py')
        cls.form=read(cls.c.FORM);cls.methods=cls.c.methods(cls.form)
        cls.old=doc('prechange-method-provenance.json')['methods']
        cls.report=read('BESHStat.Excel.Common/src/Analysis/SpcMultivariateReportProjection.vb')
        cls.presentation=read('BESHStat.Excel.Common/src/Analysis/SpcMultivariatePresentation.vb')
        cls.chart=read('BESHStat.Excel.Common/src/Analysis/SpcMultivariateChartProjectionPolicy.vb')
        cls.corpus=doc('spc-multivariate-postfit-same-core-comparisons-v1.json')
        payloads={}
        for f in cls.corpus['payload_files']:
            if sha(f['path'])!=f['sha256']:raise AssertionError(f['path'])
            bucket=json.loads((ROOT/f['path']).read_bytes())['payloads']
            if len(bucket)!=f['payload_count'] or set(bucket)&set(payloads):raise AssertionError('duplicate/count')
            payloads.update(bucket)
        cls.corpus['payloads']=payloads

    def test_one_original_fit_request_cancellation_and_workbook_boundary(self):
        self.assertEqual(1,self.form.count('SpcMultivariate.Fit('))
        for name in ['BuildRequest','btCompute_Click','BuildAppearanceOptions','CreateResultWorkbook','CreateResultSheet','WriteResultTables','BuildHistoricalModel','ValidateSelections']:
            self.assertEqual(self.old[name]['body'],self.methods[name],name)

    def test_actual_shipping_projection_and_existing_four_arg_renderer(self):
        body=self.methods['WriteResultsToNewWorkbook']
        self.assertEqual(1,body.count('New SpcMultivariateReportProjection(result)'))
        self.assertIn('payload.ProjectSeries(appearance.UseSequenceValuesForHorizontalAxis)',body)
        self.assertIn('AddCharts(sheet, payload.RenderingResult, appearance, channels)',body)
        self.assertIn('SpcMultivariateChartProjectionPolicy.Prepare',self.methods['BuildChartRenderingResult'])

    def test_seven_toggle_order_and_chart_warning_native_catch(self):
        before=self.old['WriteResultsToNewWorkbook']['body'];after=self.methods['WriteResultsToNewWorkbook']
        toggles=re.compile(r'If (chk\w+)\.Checked Then')
        self.assertEqual(toggles.findall(before),toggles.findall(after))
        self.assertEqual(7,len(toggles.findall(after)))
        self.assertIn('Multivariate control-chart warning',after)
        self.assertIn('Catch ex As Exception',after)
        self.assertIn('"The statistical results were calculated, but the Excel chart could not be created: " & ex.Message',after)
        self.assertLess(after.index('CreateResultWorkbook()'),after.index('New SpcMultivariateReportProjection'))
        self.assertIn('firstOutput.Activate()',after)

    def test_all_six_report_methods_really_delegate(self):
        for name in ['BuildSummaryTables','BuildModelTables','BuildChartDataTable','BuildSignalsTable','BuildDiagnosticTables','BuildAuditTables']:
            self.assertIn('presentation.'+name+'(',self.methods[name])
            self.assertIn('Public Function '+name+'(',self.report)
        self.assertIn('SelectedComboValue(Of DiagnosticsScope)',self.methods['BuildDiagnosticTables'])
        self.assertIn('New SpcMultivariateAuditView',self.methods['BuildAuditTables'])

    def test_common_strict_host_free_no_second_engine(self):
        for text in [self.report,self.presentation,self.chart]:
            self.assertIn('Option Strict On',text)
            self.assertIn('Option Explicit On',text)
            for forbidden in ['Imports Microsoft.Office','Imports System.Windows.Forms','Excel.Application','Excel.Worksheet','SpcMultivariate.Fit(','SpcEngine.Fit(','File.Read','File.Write','Microsoft.VisualBasic']:
                self.assertNotIn(forbidden,text)
        self.assertIn('<TargetFramework>netstandard2.0</TargetFramework>',read('BESHStat.Excel.Common/BESHStat.Excel.Common.vbproj'))

    def test_original_numerical_ui21_renderer_and_prior_common_hashes(self):
        pins=doc('prechange-source-inventory.json')['files']
        prefixes=('BESHStat.Core/','BESHstatNG/src/Graphics/','BESHStat.Excel.Common/src/Analysis/SpcUnivariate','BESHStat.Excel.Common/src/Analysis/SpcMultivariateResultAxis','BESHstatNG/src/UI/Ui21')
        for path,h in pins.items():
            if path.startswith(prefixes):self.assertEqual(h,sha(path),path)

    def test_family_uses_actual_result_and_accepted_request_compatibility(self):
        self.assertIn('Request = result.Request : Model = result.Model',self.presentation)
        self.assertIn('Compatibility = SpcMultivariateRequestPolicy.Compatibility(Request.ChartType)',self.presentation)
        self.assertIn('Panels = Array.AsReadOnly(result.Panels)',self.presentation)
        self.assertIn('Family.Caption',self.report)
        self.assertIn('Family.HasSubgroups',self.report)

    def test_wrapper_and_ten_channels_reuse_accepted_policies(self):
        self.assertIn('SpcMultivariateResultAxisPolicy.RenderingInput(panels(0))',self.chart)
        self.assertIn('SpcUnivariateChartProjectionPolicy.Project(RenderingResult, useSequence)',self.chart)
        self.assertIn('result.ExecutionTimeMilliseconds',self.chart)
        self.assertIn('source.Points',self.chart)
        self.assertIn('source.Signals',self.chart)
        self.assertIn('intent.ValueAxisTitle.Trim()',self.chart)

    def test_model_covariance_and_optional_diagnostics_are_core_values(self):
        for prop in ['ProcessCovariance','AnalysisCovariance','AnalysisCovarianceInverse','Eigenvalues','Eigenvectors']:
            self.assertIn('model.'+prop,self.report)
        self.assertIn('context.SelectDiagnostics(signalOnly)',self.report)
        self.assertIn('SpcMultivariateDiagnosticSnapshot',self.report)
        self.assertIn('eigenvectors.GetLength(1)',self.report)
        self.assertIn('Me.Warnings.Values',self.report)

    def test_warning_union_ordinal_first_seen_without_normalization(self):
        part=self.presentation.split('Public NotInheritable Class SpcMultivariateWarningProjection')[1].split('End Class')[0]
        self.assertIn('StringComparer.Ordinal',part)
        self.assertLess(part.index('For Each warning In topLevel'),part.index('For Each panel In panels'))
        self.assertNotIn('.Trim(',part)
        self.assertNotIn('IgnoreCase',part)
        self.assertIn('values.AsReadOnly()',part)

    def test_entry_oracle_provenance_and_historical_scope_candidate(self):
        p=doc('legacy-projection-provenance.json')
        self.assertIn(sha('BESHStat.Excel.Common.Tests/src/Spc24LegacyProjection.vb'),json.dumps(p))
        self.assertIn('Me.SignalOnly',read('BESHStat.Excel.Common.Tests/src/Spc24LegacyProjection.vb'))
        old='BESHStat.Excel.Common.Tests/src/Spc23LegacyPreparation.vb'
        self.assertEqual(doc('prechange-source-inventory.json')['files'][old],sha(old))
        self.assertIn('C24-HISTORICAL-ORACLE-SCOPE-01',json.dumps(doc('candidate-issues.json')))
        self.assertEqual(0,doc('candidate-issues.json')['activated_behavior_fixes'])

    def test_corpus_counts_hashes_no_orphans_and_no_goldens(self):
        d=self.corpus
        self.assertEqual('NOT_GOLDEN',d['status']);self.assertFalse(d['promoted'])
        self.assertEqual(len(d['records']),d['record_count']);self.assertEqual(len(d['payloads']),d['payload_count'])
        self.assertEqual(set(d['payloads']),{r['profile_id'] for r in d['records']})
        self.assertEqual({f['path'] for f in d['payload_files']},{p.relative_to(ROOT).as_posix() for p in (ROOT/'evidence/04A24').glob('postfit-payload-bucket-*-v1.json')})
        for key,p in d['payloads'].items():
            actual=hashlib.sha256(json.dumps(p,ensure_ascii=False,sort_keys=True,separators=(',',':'),allow_nan=False).encode()).hexdigest()
            self.assertEqual(key,actual)
            self.assertEqual('NOT_GOLDEN',p['status'])
            for key,panel in p.get('presentation',{}).get('panel_payloads',{}).items():
                self.assertEqual(key,hashlib.sha256(json.dumps(panel,ensure_ascii=False,sort_keys=True,separators=(',',':')).encode()).hexdigest())
        self.assertTrue(all(r['test_outcome']=='Passed' for r in d['records']))

    def test_preserved_rejections_are_separate_from_positive_parity(self):
        from collections import Counter
        counts=Counter(self.corpus['payloads'][r['profile_id']]['outcome'] for r in self.corpus['records'])
        self.assertEqual(dict(counts),self.corpus['outcomes'])
        self.assertGreater(counts['POSITIVE_PARITY'],0);self.assertGreater(counts['PRESERVED_REJECTION'],0)
        for p in self.corpus['payloads'].values():
            if p['outcome']=='PRESERVED_REJECTION':self.assertIn('exception',p);self.assertNotIn('presentation',p)

    def test_six_actual_families_published_fixture_and_synthetic_separation(self):
        d=doc('multivariate-chart-family-projection-v1.json')
        self.assertEqual(6,len(d['actual_families']))
        self.assertEqual(6,d['UI_choices'])
        self.assertTrue(any(p['id']=='published-6' for p in self.corpus['payloads'].values()))
        self.assertTrue(any(p['outcome']=='SYNTHETIC_API_PRESENTATION' for p in self.corpus['payloads'].values()))
        self.assertEqual(self.corpus['fixture']['sha256'],sha(self.corpus['fixture']['path']))

    def test_typed_tables_masks_and_reference_oracle_executed(self):
        p=next(p for p in self.corpus['payloads'].values() if p['outcome']=='POSITIVE_PARITY')
        presentation=p['presentation']
        self.assertEqual({'Summary','Model','ChartData','Signals','Diagnostics','Audit'},{e['name'] for e in presentation['sections']})
        self.assertEqual(2,len(presentation['projections']))
        for panel in presentation['panel_payloads'].values():self.assertEqual(10,len(panel['channels']))
        text=read('BESHStat.Excel.Common.Tests/src/Spc24Fixture.vb')
        self.assertIn('Spc22Fixture.Compare(payload.RenderingResult, useSequence)',text)
        self.assertIn('Hash(before(i).ToOutputModel())',text)
        self.assertIn('Hash(after(i).ToOutputModel())',text)

    def test_compiled_adapter_membership_and_no_native_execution(self):
        project=read('BESHStatNG.Test/BESHStatNG.Tests.vbproj')
        for f in ['Spc24AdapterTests','Spc24Tests','Spc24Fixture','Spc24LegacyProjection','Spc24ExpectedRejections']:self.assertIn(f+'.vb',project)
        adapter=read('BESHStatNG.Test/src/Spc24AdapterTests.vb')
        self.assertIn('Marshal',adapter)
        self.assertNotIn('New Ui22MultivariateControlCharts',adapter)
        self.assertNotIn('New Excel.Application',adapter)

    def test_four_lanes_prebuilt_isolation_and_no_restore(self):
        text=read('scripts/verify-04a24.py');pre=doc('parallel-preflight.json')
        self.assertEqual(4,pre['worker_limit']);self.assertFalse(pre['fifth_worker'])
        for literal in ['ThreadPoolExecutor(max_workers=4)','MaxCpuCount>1','<Workers>1','--test-concurrency=1','--no-restore','BESHSTAT_KR_EXPORT_DIR','BESHSTAT_04A02_OBSERVATIONS','is_relative_to(execution']:
            self.assertIn(literal,text)
        self.assertIn('process.wait()',text);self.assertIn('job.result()',text)
        self.assertEqual(pre['original_verifier']['sha256'],sha(pre['original_verifier']['path']))

    def test_original_control_id_sets_and_gates_stay_closed(self):
        d=doc('original-control-crosswalk.json')
        for host,counts in [('Ui22MultivariateControlCharts',(182,320,154)),('Ui21ControlCharts',(208,226,115))]:
            p=d['populations'][host]
            self.assertEqual(counts,(p['controls'],p['linked_open_obligations'],p['restricted_original_controls']))
            self.assertEqual(p['controls'],len(set(p['control_ids'])))
        for k,v in [('P','NO'),('N1','NO GO'),('N2','NO'),('C2','UNMET'),('releases',0),('discharges',0)]:self.assertEqual(v,d[k])

    def test_frozen_evidence_authority_hashes_and_unchanged_metadata(self):
        for p,r in doc('frozen-capture-hashes.json')['files'].items():self.assertEqual(r['sha256'],sha(p),p)
        meta='BESHStatNG_Help_MkDocs/docs/_meta/methods.yml'
        self.assertEqual(doc('prechange-source-inventory.json')['files'].get(meta,doc('documentation-audit.json')['metadata_sha256']),sha(meta))

    def test_documentation_availability_and_actual_report_semantics(self):
        page=read('BESHStatNG_Help_MkDocs/docs/methods/multivariate-control-charts.md')
        for literal in ['sequence values where selected and available','Source worksheet row numbers','result warnings before panel warnings','does not provide an Office.js or Mac']:
            self.assertIn(literal,page)

    def test_publication_exact_membership_hashes_and_empty_index(self):
        verifier=module('publication24_test','scripts/verify-04a24-publication.py')
        verifier.verify(False)

    def test_open_candidates_and_retained_native_geometry(self):
        source=doc('source-preservation.json')
        self.assertTrue(source['core_ui21_renderer_prior_common_designers_udfs_ribbon_targets_metadata_unchanged'])
        self.assertTrue(source['index_empty'])
        self.assertIn('C21',json.dumps(doc('candidate-issues.json')))
        self.assertIn('Last finite reference label',doc('multivariate-signal-warning-chartdata-parity-v1.json')['retained_native'])

if __name__=='__main__':unittest.main()
