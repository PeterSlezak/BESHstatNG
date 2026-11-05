"""Fault-focused tests for review runner isolation and identity preservation."""
from collections import Counter
import importlib.util
from pathlib import Path
import tempfile
import unittest
from unittest.mock import patch

ROOT=Path(__file__).resolve().parents[2]
spec=importlib.util.spec_from_file_location('test_review_runner',ROOT/'scripts/test_suite_rationalization.py')
r=importlib.util.module_from_spec(spec);spec.loader.exec_module(r)

class TestRationalizationRunner(unittest.TestCase):
    def test_result_collision_preserves_original_receipt(self):
        with tempfile.TemporaryDirectory() as temp:
            path=Path(temp)/'receipt.json';r.save(path,{'result':'original'})
            before=path.read_bytes()
            with self.assertRaises(FileExistsError):r.save(path,{'result':'replacement'})
            self.assertEqual(before,path.read_bytes())
    def test_execution_directory_collision_refuses_existing(self):
        with tempfile.TemporaryDirectory() as temp:
            path=Path(temp)/'execution';r.fresh(path)
            with self.assertRaises(FileExistsError):r.fresh(path)
    def test_private_run_rejects_historical_and_parent_escape(self):
        root=Path(tempfile.gettempdir())/'review-root'
        for path in [root/'TestResults/new',root/'.test-rationalization-a.tmp/child',root/'../.test-rationalization-a.tmp',root/'ordinary']:
            with self.assertRaises(ValueError):r.require_private_run(path,root)
        self.assertEqual((root/'.test-rationalization-a.tmp').resolve(),r.require_private_run(root/'.test-rationalization-a.tmp',root))
    def test_private_writer_resolution_stays_in_sandbox(self):
        root=Path(tempfile.gettempdir())/'execution'
        base=root/'sandbox/Core/bin/Debug/net10.0'
        self.assertTrue(r.inside(base/'../../../../TestResults/Codex/Batch0205/Wire',root))
        self.assertFalse(r.inside(base/'../../../../../../outside',root))
    def test_missing_parameterized_case_is_detected(self):
        before=[dict(class_name='C',method='M',test_name='M (1)'),dict(class_name='C',method='M',test_name='M (2)')]
        with self.assertRaises(ValueError):r.require_identity_preserved(before,before[:1])
    def test_duplicate_execution_multiplicity_is_preserved(self):
        case=dict(class_name='C',method='M',test_name='M (1)')
        with self.assertRaises(ValueError):r.require_identity_preserved([case,case],[case])
        r.require_identity_preserved([case,case],[case,case])
    def test_reordering_is_not_identity_loss_but_runtime_change_is(self):
        a=dict(class_name='Portable.C',method='M',test_name='M')
        b=dict(class_name='Framework.C',method='M',test_name='M')
        r.require_identity_preserved([a,b],[b,a])
        with self.assertRaises(ValueError):r.require_identity_preserved([a,b],[a,a])
    def test_outcome_changes_remain_visible_separately_from_identity(self):
        a=dict(class_name='C',method='M',test_name='M',outcome='Failed')
        b=dict(a,outcome='Passed')
        r.require_identity_preserved([a],[b])
        self.assertNotEqual(r.case_counter([a]),r.case_counter([b]))
    def test_empty_trx_is_never_a_pass(self):
        with tempfile.TemporaryDirectory() as temp:
            path=Path(temp)/'empty.trx';path.write_text('<TestRun xmlns="http://microsoft.com/schemas/VisualStudio/TeamTest/2010"/>')
            with self.assertRaisesRegex(ValueError,'ZERO_TESTS'):r.trx(path)
    def test_mixed_failed_skipped_and_passed_results_keep_exact_identities(self):
        with tempfile.TemporaryDirectory() as temp:
            path=Path(temp)/'cases.trx'
            path.write_text('<TestRun xmlns="http://microsoft.com/schemas/VisualStudio/TeamTest/2010"><TestDefinitions><UnitTest id="1"><TestMethod className="C" name="M"/></UnitTest></TestDefinitions><Results><UnitTestResult testId="1" testName="M (1)" outcome="Passed"/><UnitTestResult testId="1" testName="M (2)" outcome="Failed"><Output><ErrorInfo><Message>real failure</Message></ErrorInfo></Output></UnitTestResult><UnitTestResult testId="1" testName="M (3)" outcome="NotExecuted"/></Results></TestRun>')
            actual=r.trx(path)
            self.assertEqual({'Passed':1,'Failed':1,'NotExecuted':1},actual['outcomes'])
            self.assertEqual('real failure',actual['records'][1]['message'])
            self.assertEqual(['M (1)','M (2)','M (3)'],[c['test_name'] for c in actual['records']])
    def test_full_matrix_cannot_hide_nonzero_exit_or_not_run(self):
        self.assertTrue(r.failed({'lanes':[{'results':[{'command':{'exit_code':1}}]}]}))
        self.assertTrue(r.failed({'lanes':[{'results':[{'status':'NOT_RUN'}]}]}))
        self.assertFalse(r.failed({'lanes':[{'results':[{'command':{'exit_code':0}}]}]}))
        self.assertTrue(r.failed({'source_unchanged':False,'results':[{'exit_code':0}]}))
        serial=r.Runner.__new__(r.Runner);serial.run=Path(tempfile.gettempdir())/'review-serial'
        serial.command=lambda name,args: {'exit_code':0 if name.endswith('-docs') else 1}
        results=serial.serial('unit')
        self.assertEqual([1,1,0],[v['exit_code'] for v in results])
        self.assertTrue(r.failed(results),'Gate failures must survive orchestration while documentation still executes')
    def test_exact_spc_sources_still_match_without_tolerance_normalization(self):
        for name in r.SPC:
            a=(ROOT/'BESHStat.Core.Tests/src/StatisticalProcessControl'/(name+'.vb')).read_text(encoding='utf-8-sig')
            b=(ROOT/'BESHStatNG.Test/src/StatisticalProcessControl'/(name+'.vb')).read_text(encoding='utf-8-sig')
            self.assertEqual(a,b,name)
    def test_all_original_managed_suites_and_serial_inner_policy_remain(self):
        self.assertEqual({'Core','Common','Legacy','OfficeAddin','OfficeExecution'},set(r.SUITES))
        self.assertEqual(5,len(r.SPC))
    def test_portable_and_framework_containers_cannot_mix_mstest_dependencies(self):
        with tempfile.TemporaryDirectory() as temp:
            root=Path(temp);run=root/'run';build=run/'builds/entry/Debug'
            for folder,dll,dependency in [('portable','BESHStat.Core.Tests.dll','MSTest3'),('legacy','BESHStatNG.Test.dll','MSTest2')]:
                output=build/folder;output.mkdir(parents=True)
                (output/dll).write_text('test assembly')
                (output/'framework.dll').write_text(dependency)
            runner=r.Runner.__new__(r.Runner);runner.run=run
            with patch.object(r,'ROOT',root):
                _,portable,_=runner.container('Debug','entry','Core','portable-case')
                _,framework,_=runner.container('Debug','entry','Legacy','framework-case')
            self.assertEqual('MSTest3',(portable.parent/'framework.dll').read_text())
            self.assertEqual('MSTest2',(framework.parent/'framework.dll').read_text())
            self.assertNotEqual(portable.parent,framework.parent)

if __name__=='__main__':unittest.main(verbosity=2)
