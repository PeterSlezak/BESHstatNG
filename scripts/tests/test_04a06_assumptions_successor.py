"""Versioned successor contracts; original source/receipt checkers remain unchanged."""
import hashlib
import importlib.util
import json
from pathlib import Path
import re
import subprocess
import unittest

ROOT=Path(__file__).resolve().parents[2]
spec=importlib.util.spec_from_file_location('assumption06contract',ROOT/'scripts/verify-04a06-contract.py')
contract=importlib.util.module_from_spec(spec);spec.loader.exec_module(contract)

class Assumptions06SuccessorTests(unittest.TestCase):
    def test_production_sources_and_exact_open_ledger_lineage(self):
        record=contract.check()
        self.assertEqual(160,len(set(record['all_shared_open_ids'])))
        self.assertTrue(all(r['status']=='OPEN' and not r['owner_released'] for r in record['affected']))

    def test_actual_form_routes_and_unchanged_native_tails(self):
        before,after=contract.verify_extraction()
        for name in ['RunNormality','RunOutliers','RunHomogeneityVar','RunDescStat','RunSymmetry']:
            self.assertIn('WindowsAssumptionReportAdapter.Prepare',after[name])
            self.assertLess(after[name].index('Prepare('),after[name].index('GetResultWriter()'))
        for name in ['RunOneWayANOVA','RunKruskallWalis','RunROC','RunTtest','getDataMultipleGroups','BindPilotLifecycle','btCompute_Click','optByID_Click','optByColumn_Click']:
            self.assertEqual(before[name],after[name])

    def test_no_duplicate_statistics_in_form_or_windows_adapter(self):
        _,after=contract.verify_extraction()
        adapter=(ROOT/'BESHstatNG/src/DM/WindowsAssumptionReportAdapter.vb').read_text()
        self.assertIn('AssumptionReportPolicy.Build',adapter)
        self.assertIn('AssumptionReportPolicy.BoxSummary',adapter)
        self.assertNotIn('assumptions.',adapter)
        self.assertNotIn('New DescriptiveStat',adapter)
        self.assertIn('result.ThrowIfFailed()',adapter)
        self.assertNotRegex(adapter,r'GetResultWriter|writeToSheet|AppGlobals|New .*Form|ExcelConnector')

    def test_portability_and_compile_entries(self):
        common=(ROOT/'BESHStat.Excel.Common/src/Results/AssumptionReportPolicy.vb').read_text()
        self.assertIn('Option Strict On',common)
        self.assertNotRegex(common,r'(?m)^Imports .*(?:Interop|Windows.Forms|ExcelDna|Office)')
        self.assertNotRegex(common,r'\b(?:DataObj|MultiGroupsUnpairedData|MsgBox|AppGlobals|FileStream|File\.Write|CoreServices\.Log)\b')
        host=(ROOT/'BESHstatNG/BESHStatNG.vbproj').read_text()
        self.assertEqual(1,host.count('Compile Include="src\\DM\\WindowsAssumptionReportAdapter.vb"'))
        project=(ROOT/'BESHStatNG.Test/BESHStatNG.Tests.vbproj').read_text()
        for name in ['AssumptionReport06AdapterTests','LegacyAssumption06Characterization']:
            self.assertEqual(1,project.count('Compile Include="src\\'+name+'.vb"'))

    def test_importer_udf_designers_core_and_old_tests_unchanged(self):
        baseline=json.loads((contract.RUN/'baseline.json').read_bytes())['source_pins']
        for path,digest in baseline.items():
            if path.startswith(('BESHStat.Core/','planning/','scripts/tests/','BESHStatNG.Test/src/','BESHStat.Excel.Common.Tests/src/')) or '/ExcelUDFs/' in path or path.endswith('.Designer.vb') or '/UI/Refedit/' in path:
                self.assertEqual(digest,contract.sha(path),path)
        self.assertEqual('32942d3034c2fb80befe23e6bb317b9dde3097774b9cef0373536d9199f36fc9',contract.sha('BESHstatNG/src/DM/ExcelDnaDataImporter.vb'))

    def test_baseline_fixture_is_source_derived_not_native_or_golden(self):
        record=json.loads((ROOT/'docs/architecture/04A06/baseline-v1.json').read_bytes())
        self.assertIn('NOT native',record['provenance'])
        self.assertIn('NOT_GOLDEN',record['provenance'])
        self.assertFalse(record['native_executed'])
        self.assertEqual(record['fixture_sha256'],contract.sha('BESHStatNG.Test/src/LegacyAssumption06Characterization.vb'))
        self.assertEqual([3,6,9],record['pvalue_body_rows'])

    def test_native_population_is_union_not_sum(self):
        record=contract.check()
        self.assertEqual(dict(shared_blocked=141,shared_obligations=160,transferred_blocked=218,transferred_obligations=239,restricted_live=802,original_obligations=1514,importer_timing_obligations=44),record['populations'])
        self.assertEqual(0,record['released_controls'])
        self.assertEqual(0,record['discharged_obligations'])
        self.assertEqual(('NO','NO GO','NO','UNMET'),tuple(record[n] for n in ['P','N1','N2','C2']))

    def test_new_scientific_candidates_are_explicitly_unmodified(self):
        record=contract.check()
        self.assertTrue(any('SC-06-DAGOSTINO-OVERFLOW' in s for s in record['scientific_candidates']))
        self.assertTrue(any('SC-06-SHAPIRO-BOUNDS' in s for s in record['scientific_candidates']))
        self.assertTrue(any('SC-06-EMPTY-HOMOGENEITY' in s for s in record['scientific_candidates']))

if __name__=='__main__':unittest.main()
