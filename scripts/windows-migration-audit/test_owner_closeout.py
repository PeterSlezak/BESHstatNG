"""Owner approved inventory policy and preservation falsification tests."""
import copy,pathlib,unittest
import owner_closeout as o
import isolated_common_tests as isolation
class CloseoutTests(unittest.TestCase):
    @classmethod
    def setUpClass(cls):cls.rows=o.approved_obligations()
    def altered(self,key,value,kind='control'):
        row=copy.deepcopy(next(r for r in self.rows if r['id_kind']==kind));expected=[copy.deepcopy(row)];row[key]=value
        self.assertTrue(o.record_errors([row],expected))
    def test_complete1514_exact_composite_inventory(self):
        self.assertEqual(1514,len(self.rows));self.assertEqual([],o.record_errors(self.rows));self.assertEqual(892,sum(r['id_kind']=='control' for r in self.rows));self.assertEqual(622,sum(r['id_kind']=='edge' for r in self.rows))
    def test_missing_control(self):self.assertTrue(o.record_errors(self.rows[1:]))
    def test_missing_edge(self):self.assertTrue(o.record_errors(self.rows[:-1]))
    def test_duplicate_control(self):self.assertTrue(o.record_errors(self.rows+[self.rows[0]]))
    def test_duplicate_edge(self):self.assertTrue(o.record_errors(self.rows+[self.rows[-1]]))
    def test_foreign_id(self):self.altered('obligation_id','control:foreign')
    def test_source_pin(self):self.altered('source_evidence',{})
    def test_risk_downgrade(self):self.altered('risk_tiers',['P3'])
    def test_capture_flag(self):self.altered('affected_live_golden_blocked',False)
    def test_ownerless(self):self.altered('owner_roles',[])
    def test_missing_fixture(self):self.altered('named_fixture','')
    def test_invalid_batch(self):self.altered('earliest_approved_batches',['04A.76'])
    def test_invented_control_qualification(self):self.altered('full_contract_newly_qualified',True)
    def test_invented_edge_qualification(self):self.altered('full_contract_newly_qualified',True,'edge')
    def test_runtime_not_approved(self):self.altered('verification_status','PASSED')
    def test_capture_not_approved(self):self.altered('capture_authorized',True)
    def test_explicit_classification_authority(self):self.altered('approval_record','')
    def test_final_acceptance_not_implied(self):
        v=o.policy();v['official_batch_accepted']=True;self.assertTrue(o.policy_errors(v))
    def test_04A02_not_implied(self):
        v=o.policy();v['04A02_authorized']=True;self.assertTrue(o.policy_errors(v))
    def test_no_blanket_P0(self):
        v=o.policy();v['blanket_P0_waiver']=True;self.assertTrue(o.policy_errors(v))
    def test_cleanup_not_authorized(self):
        v=o.policy();v['cleanup_authorized']=True;self.assertTrue(o.policy_errors(v))
    def test_exception_each_required(self):
        for k in o.dispositions():
            v=o.dispositions();del v[k];self.assertTrue(o.disposition_errors(v))
    def test_Cox_source_unchanged_and_risk_retained(self):
        self.assertEqual('P0',o.dispositions()['Cox']['risk']);self.assertIn('implementation not performed',o.dispositions()['Cox']['decision'])
    def test_xlApp_no_downgrade(self):
        v=o.dispositions();v['RefEdit']['risk']='P3';self.assertTrue(o.disposition_errors(v))
    def test_versioned_distinct_policy(self):
        self.assertEqual([],o.policy_errors(o.policy()));self.assertEqual(2,o.policy()['original_strict_exit']);self.assertEqual(1,o.policy()['integrity_failure_exit'])
    def test_historical_bin_execution_rejected(self):
        with self.assertRaises(ValueError):isolation.routes(o.R/'BESHStat.Excel.Common.Tests/bin/Debug/net10.0',o.H/'sandbox')
    def test_sandbox_escape_rejected(self):
        with self.assertRaises(ValueError):isolation.routes(o.H/'sandbox',o.H/'sandbox')
    def test_expected_writer_resolution(self):
        sandbox=o.H/'unit-sandbox';roots=isolation.routes(sandbox/'Common/bin/Debug/net10.0',sandbox)
        self.assertEqual([sandbox/'TestResults/Codex/Batch0205/Wire',sandbox/'TestResults/Codex/Batch0207'],roots)
    def test_history_metadata_change_and_deletion_rejected(self):
        import tempfile
        with tempfile.TemporaryDirectory(dir=o.H) as tmp:
            p=pathlib.Path(tmp)/'file';p.write_bytes(b'original');s=p.stat();m=dict(path='file',bytes=s.st_size,mtime_ns=s.st_mtime_ns,ctime_ns=s.st_ctime_ns,ino=s.st_ino,dev=s.st_dev)
            self.assertEqual([],o.metadata_errors([m],pathlib.Path(tmp)));m['mtime_ns']-=1;self.assertTrue(o.metadata_errors([m],pathlib.Path(tmp)));p.unlink();self.assertTrue(o.metadata_errors([m],pathlib.Path(tmp)))
    def test_old_source_and_decision_hash_mismatch_rejected(self):
        import tempfile
        with tempfile.TemporaryDirectory(dir=o.H) as tmp:
            p=pathlib.Path(tmp)/'decision.json';p.write_bytes(b'[]');e=dict(path='decision.json',bytes=2,sha256='0'*64);self.assertTrue(o.c.artifact_errors([e],pathlib.Path(tmp)))
    def test_actual_compiled_source_mismatch_rejected(self):
        b=o.read(o.H/'before.json');b['compiled'][0]['sha256']='0'*64;self.assertTrue(o.protected_errors(b))
    def test_actual_decision_prefix_mismatch_rejected(self):
        b=o.read(o.H/'before.json');b['decision_prefixes'][0]['sha256']='0'*64;self.assertTrue(o.protected_errors(b))
    def test_profile_integrity_failure_returns1_not_success_or_strict2(self):
        from unittest.mock import patch
        import contextlib,io
        original=o.read
        def altered(p):
            v=original(p)
            if p.name=='04A01-inventory-profile-v1.json':v['official_batch_accepted']=True
            return v
        with patch.object(o,'read',side_effect=altered),patch('sys.argv',['owner_closeout.py','--profile','inventory-complete-with-approved-deferrals']),contextlib.redirect_stdout(io.StringIO()):self.assertEqual(1,o.main())
if __name__=='__main__':unittest.main(verbosity=2)
