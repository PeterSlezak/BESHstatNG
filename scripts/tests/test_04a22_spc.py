"""04A.22 shipping-source, preservation, authority and publication contracts."""
import hashlib
import importlib.util
import json
from pathlib import Path
import re
import subprocess
import unittest

ROOT = Path(__file__).resolve().parents[2]
OUT = ROOT/'evidence/04A22'
FORM = 'BESHstatNG/src/UI/Ui21ControlCharts.vb'
RENDERER = 'BESHstatNG/src/Graphics/SpcControlChartExcel.vb'
POLICY = 'BESHStat.Excel.Common/src/Analysis/SpcUnivariateChartProjectionPolicy.vb'
BASE = '9330f8df18c0c73d2b233ec4b06dc7199cbb244a'
def read(p): return (ROOT/p).read_text(encoding='utf-8-sig')
def doc(p): return json.loads((OUT/p).read_bytes())
def sha(p): return hashlib.sha256((ROOT/p).read_bytes()).hexdigest()
def entry(p): return subprocess.check_output(['git','-c','safe.directory='+ROOT.as_posix(),'show',BASE+':'+p],cwd=ROOT).decode('utf-8-sig').replace('\r\n','\n')
def methods(text, indent=8):
    pad=' '*indent
    return {m.group(2):m.group() for m in re.finditer('^'+pad+r'(?:Private|Public|Friend) (?:Shared )?(Sub|Function) (\w+)\b.*?^'+pad+r'End \1',text,re.M|re.S)}

class Spc22Contracts(unittest.TestCase):
    def test_real_entry_shipping_base_and_unchanged_gates(self):
        e=doc('entry-state.json')
        self.assertEqual(BASE,e['base'])
        self.assertEqual(dict(P='NO',N1='NO GO',N2='NO',C2='UNMET'),e['gates'])
        self.assertEqual(4,len(e['shipping_policies']))
        self.assertEqual(0,e['native_releases'])

    def test_actual_ui_projection_and_single_fit_remain_inside_warning_boundary(self):
        t=read(FORM)
        self.assertEqual(1,t.count('SpcEngine.Fit('))
        self.assertIn('SpcEngine.Fit(request, AddressOf CancellationRequested)',t)
        body=methods(t,4)['WriteResultsToNewWorkbook']
        self.assertLess(body.index('Try'),body.index('SpcUnivariateChartProjectionPolicy.Project'))
        self.assertLess(body.index('SpcUnivariateChartProjectionPolicy.Project'),body.index('graphics.SpcControlChartExcel.AddCharts(sheet, result, appearance, projection)'))
        self.assertIn('Catch ex As Exception',body)
        self.assertIn('SPC control-chart warning',body)
        self.assertIn(' & ex.Message',body)

    def test_workbook_output_order_toggles_and_failure_timing_exact(self):
        before=methods(entry(FORM),4)['WriteResultsToNewWorkbook']
        after=methods(read(FORM),4)['WriteResultsToNewWorkbook']
        new='''                Dim appearance = BuildAppearanceOptions()
                Dim projection = SpcUnivariateChartProjectionPolicy.Project(result, appearance.UseSequenceValuesForHorizontalAxis)
                graphics.SpcControlChartExcel.AddCharts(sheet, result, appearance, projection)'''
        self.assertEqual(before,after.replace(new,'                graphics.SpcControlChartExcel.AddCharts(sheet, result, BuildAppearanceOptions())'))
        for mask in range(32):
            selected=[name for i,name in enumerate(['SPC Summary','Control Charts','Chart Data','Signals','Settings and Audit']) if mask & (1<<i)]
            self.assertEqual(selected,sorted(selected,key=after.index))

    def test_renderer_consumes_payload_and_preserves_public_three_argument_api(self):
        t=read(RENDERER)
        for token in ['Return AddCharts(worksheet, result, appearance, Nothing)','projection.Panels(panelIndex)','MarshalAxis(projection)','MarshalPanel(panelProjection)','MarshalChartValues(projection.Channels','panelProjection.PointLabels','panelProjection.SignalLabels','panelProjection.ExclusionLabels','For Each boundary In axisContext.Projection.StageBoundaries','plotWidth * boundary.Fraction','boundary.AlternativeText']:
            self.assertIn(token,t)
        self.assertNotIn('BuildSharedAxisContext(',t)
        self.assertNotIn('BuildPanelChartData(',t)
        self.assertNotIn('FormatRuleNumbers(',t)
        self.assertIn('series.Points(zeroBasedPointIndex + 1)',t)
        self.assertIn('createdCharts(i).Delete()',t)

    def test_all_retained_native_methods_and_appearance_remain_exact(self):
        a,b=methods(entry(RENDERER)),methods(read(RENDERER))
        changed={'AddCharts','RenderPanel','BuildSharedAxisContext','BuildPanelChartData','AddStageBoundaries','AddPointLabels','AddSignalLabels','AddExclusionLabels','FormatRuleNumbers','ChartValue'}
        for name in set(a)-changed:
            self.assertEqual(a[name],b[name],name)
        self.assertEqual(entry(RENDERER).split('    Public NotInheritable Class SpcControlChartExcel')[0],read(RENDERER).replace('Imports BESHStatNG.ExcelCommon\n','').split('    Public NotInheritable Class SpcControlChartExcel')[0])

    def test_portable_contract_is_strict_immutable_and_has_no_engine_or_host(self):
        t=read(POLICY)
        self.assertIn('Option Strict On',t)
        for token in ['Interop','Windows.Forms','Microsoft.VisualBasic','System.IO','SpcEngine.','SpcRuleEvaluator','Math.Sqrt','Math.Exp','Object()','ResultTable(']:
            self.assertNotIn(token,t)
        for token in ['Array.AsReadOnly','As IReadOnlyList','point.IsSignalled','point.IsExplicitlyExcluded','StringComparison.OrdinalIgnoreCase','CDbl(ordinal) / CDbl(count)','Me.ChartFamily = result.ChartFamily']:
            self.assertIn(token,t)
        self.assertNotIn('Case SpcChartType.',t)

    def test_legacy_adapter_is_source_derived_and_has_no_new_policy(self):
        p=doc('legacy-renderer-provenance.json')
        self.assertEqual(p['source_sha256'],doc('prechange-method-inventory.json')['files'][RENDERER]['sha256'])
        self.assertEqual(p['derivative_sha256'],sha(p['derivative']))
        self.assertNotIn('SpcUnivariateChartProjection',read(p['derivative']))
        for name,pin in p['methods'].items():
            self.assertEqual(pin,hashlib.sha256(methods(entry(RENDERER))[name].encode()).hexdigest())

    def test_core_ui22_udfs_designers_old_tests_targets_and_frozen_evidence(self):
        pins=doc('prechange-method-inventory.json')['files']
        for p,v in pins.items():
            if p.startswith('BESHStat.Core/') or 'Ui22Multivariate' in p or '.Designer.' in p or 'Spc21' in p or '/ExcelUDFs/' in p:
                self.assertEqual(v['sha256'],sha(p),p)
        for p,v in doc('frozen-capture-hashes.json')['files'].items():
            self.assertEqual(v['sha256'],sha(p),p)
        for p in ['BESHStat.Core/BESHStat.Core.vbproj','BESHStat.Excel.Common/BESHStat.Excel.Common.vbproj']:
            self.assertIn('<TargetFramework>netstandard2.0</TargetFramework>',read(p))

    def test_original_controls_are_id_sets_and_zero_discharge(self):
        d=doc('original-control-crosswalk.json')
        p=d['populations']['Ui21ControlCharts']
        self.assertEqual((208,226,115),(p['controls'],p['linked_open_obligations'],p['restricted_original_controls']))
        self.assertEqual(0,d['releases'])
        self.assertEqual(0,d['discharges'])
        self.assertEqual(len({r['control_id'] for r in d['controls']}),d['union']['controls'])
        self.assertEqual(len({r['obligation_id'] for r in d['obligations']}),d['union']['linked_open_obligations'])
        for p,r in d['authorities'].items():self.assertEqual(r['sha256'],sha(p))

    def test_c21_candidate_and_native_scientific_restrictions_remain(self):
        d=doc('candidate-issues.json')
        self.assertEqual(0,d['activated_behavior_fixes'])
        self.assertEqual('OPEN / CANDIDATE_ONLY / UNAPPLIED',d['C21-STACKED-ROWCOUNT-VALIDATION-01'])
        self.assertEqual('NOT_RUN / UNAUTHORIZED',d['native'])
        self.assertEqual('NOT_RUN / UNAUTHORIZED',d['R'])

    def test_corpus_is_non_golden_and_payload_hashes_match(self):
        d=doc('spc-postfit-same-core-comparisons-v1.json')
        self.assertEqual('NOT_GOLDEN',d['status'])
        self.assertEqual('SAME_CORE_PRESENTATION_COMPATIBILITY_ONLY',d['validation'])
        self.assertFalse(d['promoted'])
        self.assertEqual(d['record_count'],len(d['records']))
        self.assertEqual({'Debug','Release'},{r['configuration'] for r in d['records']})
        for key,value in d['payloads'].items():
            self.assertEqual(key,hashlib.sha256(json.dumps(value,sort_keys=True,separators=(',',':'),ensure_ascii=False).encode()).hexdigest())
        self.assertIn('CORE_OR_REQUEST_REJECTION_NOT_PARITY_SUCCESS',d['classifications'])

    def test_all_fifteen_choices_have_real_successful_panel_observations(self):
        d=doc('spc-chart-family-panel-matrix-v1.json')
        self.assertEqual(15,len(d['charts']))
        for chart,panel_orders in d['charts'].items():self.assertTrue(panel_orders,chart)
        self.assertEqual('Windows only',d['availability'])

    def test_documentation_and_authored_catalog_truth(self):
        d=doc('documentation-audit.json')
        self.assertEqual(d['metadata_sha256'],sha('BESHStatNG_Help_MkDocs/docs/_meta/methods.yml'))
        self.assertEqual(d['dataset_sha256'],sha('BESHStatNG_Help_MkDocs/docs/assets/data/115controlcharts/115controlcharts.csv'))
        self.assertIn('Stage boundaries',read('BESHStatNG_Help_MkDocs/docs/methods/control-charts.md'))

    def test_publication_membership_hashes_empty_index_and_privacy(self):
        spec=importlib.util.spec_from_file_location('publication22',ROOT/'scripts/verify-04a22-publication.py')
        module=importlib.util.module_from_spec(spec);spec.loader.exec_module(module)
        module.verify(check_receipts=False)

if __name__=='__main__':unittest.main()
