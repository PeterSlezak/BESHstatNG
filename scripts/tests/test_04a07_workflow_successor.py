"""Additive 04A.07 source/compatibility/restriction tests. Frozen parents are not rewritten."""
import hashlib
import importlib.util
import json
from pathlib import Path
import re
import unittest

ROOT=Path(__file__).resolve().parents[2]
spec=importlib.util.spec_from_file_location('contract07',ROOT/'scripts/verify-04a07-contract.py')
contract=importlib.util.module_from_spec(spec);spec.loader.exec_module(contract)

class Anova07SuccessorTests(unittest.TestCase):
    def test_exact_source_and_original_ledger_rows(self):
        record=contract.check()
        self.assertEqual(160,len(set(record['original_open_anova_normality_ids'])))
        self.assertEqual(239,len(set(record['original_open_transfer_ids'])))
        self.assertTrue(all(r['status']=='OPEN' for r in record['affected_controls'] if r['obligation_id']))

    def test_actual_four_shipping_routes_and_native_bodies(self):
        paths=contract.verify_sources()
        self.assertEqual(3,len(paths))
        self.assertEqual(4,sum(len(v) for v in contract.FORMS.values()))
        for path,source in paths.items():
            for name in contract.FORMS[path]:self.assertIn('WindowsAnovaWorkflowAdapter.',source['after'][name])
        self.assertEqual(paths['BESHstatNG/src/UI/UibyID.vb']['before']['RunSharedOutput'],paths['BESHstatNG/src/UI/UibyID.vb']['after']['RunSharedOutput'])

    def test_one_way_and_kw_share_existing_dispatcher_no_second_engine(self):
        source=(ROOT/'BESHStat.Excel.Common/src/Analysis/AnovaWorkflowPolicy.vb').read_text()
        self.assertEqual(1,source.count('SharedAnalysisApplicationService.Run('))
        self.assertNotIn('New parametric.OneWayANOVA(',source)
        self.assertNotIn('New nonparametric.KruskallWalis(',source)
        self.assertEqual(1,source.count('New parametric.OneWayRmANOVA('))
        self.assertEqual(1,source.count('New parametric.TwoWayNestedANOVA('))
        self.assertIn('AssumptionReportPolicy.Build(',source)
        self.assertNotRegex(source,r'DescriptiveStat\(|strConfidenceInterval.*Parse|Double\.Parse\(')

    def test_windows_bridge_has_no_core_statistics_writes_or_native_acquisition(self):
        source=(ROOT/'BESHstatNG/src/DM/WindowsAnovaWorkflowAdapter.vb').read_text()
        self.assertNotRegex(source,r'AppGlobals|assumptions\.|New parametric\.|\.compute\(|GetResultWriter|\.writeToSheet|\.AddBoxPlot\(|ExcelConnector|ReleaseComObject')
        self.assertIn('WindowsPairedInputAdapter.GetBlockEvidence',source)
        self.assertIn('WindowsPairedInputAdapter.GetNestedEvidence',source)
        self.assertIn('WindowsUnivariateInputAdapter.GetOriginalSelection',source)

    def test_portability_targets_and_compile_entries(self):
        source=(ROOT/'BESHStat.Excel.Common/src/Analysis/AnovaWorkflowPolicy.vb').read_text()
        self.assertIn('Option Strict On',source)
        self.assertNotRegex(source,r'(?m)^Imports .*(Interop|Windows.Forms|ExcelDna|Office|System.IO)')
        self.assertNotRegex(source,r'\b(?:DataObj|MultiGroupsPairedData|MultiGroupsUnpairedData|MsgBox|AppGlobals)\b')
        self.assertIn('<TargetFramework>netstandard2.0</TargetFramework>',(ROOT/'BESHStat.Excel.Common/BESHStat.Excel.Common.vbproj').read_text())
        host=(ROOT/'BESHstatNG/BESHStatNG.vbproj').read_text()
        self.assertEqual(1,host.count('Compile Include="src\\DM\\WindowsAnovaWorkflowAdapter.vb"'))
        tests=(ROOT/'BESHStatNG.Test/BESHStatNG.Tests.vbproj').read_text()
        for name in ['AnovaWorkflow07AdapterTests','LegacyAnovaWorkflow07']:self.assertEqual(1,tests.count('Compile Include="src\\'+name+'.vb"'))

    def test_core_old_tests_importer_udfs_designers_and_help_unchanged(self):
        pins=json.loads((contract.RUN/'baseline.json').read_bytes())['source_pins']
        for path,digest in pins.items():
            if path.startswith(('BESHStat.Core/','BESHStat.Core.Tests/','BESHStat.Excel.Common.Tests/','BESHStatNG.Test/src/','BESHStatNG_Help_MkDocs/','planning/','scripts/tests/')) or '/ExcelUDFs/' in path or path.endswith('.Designer.vb') or '/UI/Refedit/' in path:
                self.assertEqual(digest,contract.sha(path),path)
        self.assertEqual('32942d3034c2fb80befe23e6bb317b9dde3097774b9cef0373536d9199f36fc9',contract.sha('BESHstatNG/src/DM/ExcelDnaDataImporter.vb'))

    def test_no_fabricated_obligation_for_previously_source_qualified_controls(self):
        record=contract.check()
        mauchly=next(r for r in record['affected_controls'] if r['control_id']=='BESHStatNG.Ui0OneRefeditMulticol.ckMauchly')
        self.assertIsNone(mauchly['obligation_id'])
        self.assertEqual([],mauchly['case_ids'])
        self.assertEqual('NOT_IN_DEFERRED_LEDGER',mauchly['status'])
        self.assertFalse(mauchly['owner_released'])

    def test_restriction_unions_and_timing_exceptions_not_generalized(self):
        r=contract.check()
        expected=dict(original_obligations=1514,restricted_live=802,transferred_obligations=239,transferred_controls=218,shared_obligations=160,shared_controls=141,importer_timing_obligations=44)
        self.assertEqual(expected,{k:r['counts'][k] for k in expected})
        self.assertEqual((0,0,False),(r['released_controls'],r['discharged_obligations'],r['native_executed']))
        self.assertEqual(('NO','NO GO','NO','UNMET'),tuple(r[k] for k in ['P','N1','N2','C2']))
        self.assertIn('NOT generalized',r['gate_disposition'])

    def test_no_sorting_source_rows_or_fabricated_workbook_identity(self):
        source=(ROOT/'BESHstatNG/src/DM/WindowsAnovaWorkflowAdapter.vb').read_text()
        self.assertNotRegex(source,r'WorksheetRow\s*=|WorkbookId\s*=|WorksheetId\s*=|New UnivariateObservationSource')
        self.assertIn('SequenceEqual(captured.Groups(g))',source)
        common=(ROOT/'BESHStat.Excel.Common/src/Analysis/AnovaWorkflowPolicy.vb').read_text()
        self.assertIn('Original provenance remains on input, never on this scratch.',common)
        self.assertNotIn('OrderBy(',common)

    def test_pre_edit_fixture_and_no_scientific_reference_promotion(self):
        b=json.loads((ROOT/'docs/architecture/04A07/baseline-v1.json').read_bytes())
        self.assertEqual(b['fixture_sha256'],contract.sha('BESHStatNG.Test/src/LegacyAnovaWorkflow07.vb'))
        self.assertIn('NOT_GOLDEN',b['provenance'])
        self.assertFalse(b['native_executed'])
        pins=json.loads((contract.RUN/'baseline.json').read_bytes())['source_pins']
        for p in ['docs/architecture/04A02/04A02-C13-independent-reviewer-1.pending.json','docs/architecture/04A02/04A02-C13-independent-reviewer-2.pending.json','docs/architecture/04A02/04A02-C13-owner-P-decision.pending.json']:
            self.assertEqual(pins[p],contract.sha(p))

if __name__=='__main__':unittest.main()
