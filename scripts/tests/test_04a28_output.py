"""04A.28 source/preservation contracts; native acceptance is expressly absent."""
import ast
import hashlib
import importlib.util
import json
from pathlib import Path
import re
import subprocess
import unittest
from unittest.mock import patch

ROOT=Path(__file__).resolve().parents[2]
BASE='033cbe9033cc10bce3e19322e5582e5ea389fe22'
WRITER='BESHstatNG/src/DM/ResultWriter/ExcelDnaResultWriter.vb'
ADAPTER='BESHstatNG/src/DM/ResultWriter/ExcelDnaSharedOutputPlanAdapter.vb'
def read(path):return (ROOT/path).read_text(encoding='utf-8-sig')
def original(path):return subprocess.check_output(['git','-c','safe.directory='+ROOT.as_posix(),'show',BASE+':'+path],cwd=ROOT).decode('utf-8-sig')
def method(source,name):
    match=re.search(r'(?im)^\s*(?:(?:Public|Private|Friend|Protected|Shared|Overridable|Overrides|ReadOnly)\s+)*(Sub|Function) '+re.escape(name)+r'\b',source)
    if not match:raise AssertionError(name)
    end=re.search(r'(?im)^\s*End '+match[1]+r'\s*$',source[match.start():])
    return source[match.start():match.start()+end.end()].replace('\r\n','\n').strip()

class Output28SourceContracts(unittest.TestCase):
    def test_core_numerics_udfs_designers_importer_and_canonical_spc_preserved(self):
        pins=json.loads(read('evidence/04A28/source-preservation.json'))['protected_compiled_sources']
        self.assertGreater(len(pins),100)
        for path,expected in pins.items():
            with self.subTest(path=path):self.assertEqual(expected,hashlib.sha256((ROOT/path).read_bytes()).hexdigest())

    def test_legacy_pvalue_conversion_and_fallback_bodies_are_identical(self):
        for name in ['LegacyPValueIsHighlighted','ResolvePValueAlpha','ResolvePValueNumberFormat','ApplyPValueHighlight']:
            with self.subTest(name=name):self.assertEqual(method(original(WRITER),name),method(read(WRITER),name))

    def test_native_formatting_operation_order_preserved(self):
        # Compare every native property statement, not just presence of tokens.
        def operations(source):
            for new,old in [('axis.HasTopBorder','TitlesCount = 0'),('axis.TopHeaderRow(i)','i + TitlesCount'),('axis.FirstFooterRow(rng.Rows.Count)','rng.Rows.Count - foots + 1'),('axis.LastBodyRow(rng.Rows.Count)','rng.Rows.Count - foots'),('axis.LastBodyRow(.Rows.Count)','.Rows.Count - foots')]:
                source=source.replace(new,old)
            return [line.strip().split("'",1)[0].strip() for line in source.splitlines()
                if re.search(r'\.(?:Font|Borders|NumberFormat|Interior|HorizontalAlignment|VerticalAlignment|AutoFit|Value2|Value)\b',line)
                and not re.search(r'\b(?:command|request|requested)\.',line)]
        self.assertEqual(operations(original(WRITER)),operations(read(WRITER)))

    def test_common_has_no_native_or_filesystem_dependency(self):
        for path in ['Output/ResultTablePresentationIntent.vb','Output/ResultTableSequencePolicy.vb','Results/GeePredictionOutputAxis.vb']:
            text=read('BESHStat.Excel.Common/src/'+path)
            self.assertIn('Option Strict On',text)
            self.assertNotRegex(text,r'(?i)(Microsoft\.Office|ExcelDna|System\.IO|System\.Windows\.Forms|Microsoft\.VisualBasic|\bAppGlobals\b)')

    def test_all_three_shipping_routes_reach_common_presentation(self):
        writer=read(WRITER);adapter=read(ADAPTER)
        self.assertIn('ResultTablePresentationIntent.Capture(block)',method(writer,'WriteOutputBlockCore'))
        self.assertIn('WriteOutputBlockCore',method(writer,'WriteSharedRequest'))
        self.assertIn('command.Presentation',method(writer,'WriteSharedCommand'))
        self.assertIn('ResultTablePresentationIntent.Capture',adapter)
        self.assertIn('ResultTableSequencePolicy.PreflightBounds',adapter)
        self.assertIn('ResultTableSequencePolicy.OrderedRegions',adapter)
        self.assertIn('ExcelDnaSharedOutputRun',read('BESHstatNG/src/UI/UibyID.vb'))

    def test_deferred_windows_ui_and_covariance_quarantine_unchanged(self):
        for path in ['BESHstatNG/src/UI/'+x+'.vb' for x in ['Ui82x2','UibyID','UiGLM','Ui13GEE']]+['BESHstatNG/src/DM/WindowsRegressionRequestAdapter.vb']:
            self.assertEqual(original(path),read(path))
        self.assertIn('C11-NATIVE-CORRECTIONS-01',read('BESHstatNG/src/DM/WindowsRegressionRequestAdapter.vb'))

    def test_liddell_current_label_and_exact_body_axis_remain(self):
        source=read('BESHStat.Excel.Common/src/Results/ContingencyReportPolicy.vb')
        self.assertIn('Table("Liddell\'s Test", {{"P-value", value.Item1.Pvalue}, {"Risk ratio", value.Item2.Estimate}',source)
        self.assertIn('CI(metrics, "liddell", value.Item2)',source)
        self.assertEqual(original('BESHStat.Excel.Common/src/Results/ContingencyReportPolicy.vb'),source)

    def test_gee_descriptor_is_detached_and_not_active_windows_reordering(self):
        axis=read('BESHStat.Excel.Common/src/Results/GeePredictionOutputAxis.vb')
        self.assertIn('ClusteredToInputIndices',axis)
        for path in ['BESHstatNG/src/UI/Ui13GEE.vb','BESHstatNG/src/DM/WindowsGeeWorkflowAdapter.vb']:
            self.assertNotIn('GeePredictionOutputAxis.Describe',read(path))

    def test_default_full_runner_and_spc_links_unchanged(self):
        self.assertEqual(original('scripts/test_suite_rationalization.py'),read('scripts/test_suite_rationalization.py'))
        project=read('BESHStatNG.Test/BESHStatNG.Tests.vbproj')
        for name in ['SpcShewhartChartTests','SpcTimeWeightedChartTests','SpcMultivariateTests','SpcRuleEvaluatorTests','SpcResultTablesAndDispatchTests']:
            self.assertIn('..\\BESHStat.Core.Tests\\src\\StatisticalProcessControl\\'+name+'.vb',project)

    def test_new_inventory_maps_each_declared_test_to_source_and_behavior(self):
        inventory=json.loads(read('evidence/04A28/new-test-inventory.json'))
        for row in inventory['managed_methods']:
            source=read(row['source'])
            self.assertRegex(source,r'(?:Public Sub|public void) '+re.escape(row['method'])+r'\(')
            self.assertTrue(row['behavior']);self.assertTrue(row['owner'])
        expected={(p.as_posix(),name) for p in [ROOT/'BESHStat.Excel.Common.Tests/src/Output28PresentationTests.vb',ROOT/'BESHStat.Excel.Common.Tests/src/Output28SafetyAndAxisTests.vb',ROOT/'BESHStatNG.Test/src/DataManagement/Output28AdapterTests.vb',ROOT/'BESHStat.OfficeAddin/tests/Output28ContractTests.cs'] for name in re.findall(r'(?:Public Sub|public void) (\w+)\(',p.read_text()) if name!='New'}
        actual={((ROOT/row['source']).as_posix(),row['method']) for row in inventory['managed_methods']}
        self.assertEqual(expected,actual)

    def test_opt_in_runner_has_explicit_release_selection_and_no_default_changes(self):
        tree=ast.parse(read('scripts/verify-04a28.py'))
        self.assertTrue(any(isinstance(n,ast.ClassDef) and n.name=='BatchRunner' for n in tree.body))
        source=read('scripts/verify-04a28.py')
        self.assertIn("runner.matrix('Debug'",source)
        self.assertIn("configuration='Release'",source)
        self.assertIn("('CAPTURE' in key or 'OBSERVATION' in key)",source)
        self.assertNotIn("runner.matrix('Release'",source)

    def test_runner_disables_historical_capture_modes_but_keeps_isolated_observations(self):
        spec=importlib.util.spec_from_file_location('batch28_test_runner',ROOT/'scripts/verify-04a28.py')
        runner=importlib.util.module_from_spec(spec);spec.loader.exec_module(runner)
        instance=runner.BatchRunner.__new__(runner.BatchRunner)
        received={}
        def record(self,name,args,cwd,env):
            received.update(env);return {'exit_code':0}
        with patch.dict('os.environ',{'BESHSTAT_04A12_INPUT_CAPTURE':'protected-original','BESHSTAT_04A02_OBSERVATIONS':'protected-original'}),patch.object(runner.r.Runner,'command',record):
            result=instance.command('isolated', ['no-process'],env={'BESHSTAT_04A02_OBSERVATIONS':'new-private-sandbox'})
        self.assertEqual('',received['BESHSTAT_04A12_INPUT_CAPTURE'])
        self.assertEqual('new-private-sandbox',received['BESHSTAT_04A02_OBSERVATIONS'])
        self.assertEqual(0,result['exit_code'])

if __name__=='__main__':unittest.main()
