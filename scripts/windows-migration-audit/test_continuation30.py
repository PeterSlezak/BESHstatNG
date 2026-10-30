"""C30 falsification of pending dossier, not an alternate acceptance checker."""
import copy,json,pathlib,tempfile,unittest,os
import continuation30_report as c
class Continuation30Tests(unittest.TestCase):
    @classmethod
    def setUpClass(cls):cls.rs=c.obligations();cls.expected={r['obligation_id']:r for r in cls.rs}
    def one(self,kind='control',risk=None):
        r=copy.deepcopy(next(r for r in self.rs if r['id_kind']==kind and (risk is None or risk in r['risk_tiers'])));return r,{r['obligation_id']:self.expected[r['obligation_id']]}
    def test_30_exact1514_composite_obligations(self):
        self.assertEqual(1514,len(self.rs));self.assertEqual(892,sum(r['id_kind']=='control' for r in self.rs));self.assertEqual(622,sum(r['id_kind']=='edge' for r in self.rs));self.assertEqual([],c.record_errors(self.rs,self.expected))
    def test_30_missing_control(self):self.assertTrue(c.record_errors(self.rs[1:],self.expected))
    def test_30_missing_edge(self):self.assertTrue(c.record_errors(self.rs[:-1],self.expected))
    def test_30_duplicate_control(self):self.assertTrue(c.record_errors(self.rs+[self.rs[0]],self.expected))
    def test_30_duplicate_edge(self):self.assertTrue(c.record_errors(self.rs+[self.rs[-1]],self.expected))
    def test_30_foreign_or_conflated_composite_id(self):
        r,e=self.one();r['obligation_id']='edge:'+r['original_id'];self.assertTrue(c.record_errors([r],e))
    def test_30_altered_risk_or_original_body(self):
        for k,v in [('risk_tiers',['P3']),('source_record_sha256','0'*64),('unsupported_contract','invented')]:
            r,e=self.one(risk='P0');r[k]=v;self.assertTrue(c.record_errors([r],e))
    def test_30_changed_P0_golden_flag(self):
        r,e=self.one(risk='P0');r['affected_live_golden_blocked']=False;self.assertTrue(c.record_errors([r],e))
    def test_30_fabricated_full_qualification(self):
        for kind in ('control','edge'):
            r,e=self.one(kind);r['full_contract_newly_qualified']=True;self.assertTrue(c.record_errors([r],e))
    def test_30_missing_owner_batch_fixture(self):
        for k in ('owner_roles','earliest_approved_batches','named_fixture'):
            r,e=self.one();r[k]=[] if isinstance(r[k],list) else '';self.assertTrue(c.record_errors([r],e))
    def test_30_invented_continuation_or_closure_owner(self):
        for batch in ('C30','04A.30','04A.48','04A.99'):
            r,e=self.one();r['earliest_approved_batches']=[batch];self.assertTrue(c.record_errors([r],e))
    def test_30_blanket_P0_waiver(self):
        p=c.profile();p['blanket_P0_waiver']=True;self.assertTrue(c.policy_errors(p))
        r,e=self.one(risk='P0');r['deferral_accepted']=True;self.assertTrue(c.record_errors([r],e))
    def test_30_omitted_explicit_blocker(self):
        for ident in c.EXCEPTIONS:self.assertTrue(c.record_errors([r for r in self.rs if r['original_id']!=ident],self.expected))
    def test_30_three_actual_exceptions_remain_pending(self):
        es=c.exception_entries();self.assertEqual(3,len(es));self.assertEqual({'P0','UNKNOWN_HIGH'},{e['risk'] for e in es});self.assertTrue(all(e['decision']=='PENDING_OWNER_DECISION' and e['approved_at'] is None and not e['source_resolved'] for e in es))
    def test_30_unapproved_effective_profile(self):
        for k,v in [('effective_profile','inventory-complete-with-approved-deferrals'),('approval_status','APPROVED'),('implemented',True),('effective',True),('implementation_authorized',True),('approved_at','2026-10-30'),('04A02_authorized',True)]:
            p=c.profile();p[k]=v;self.assertTrue(c.policy_errors(p),k)
    def test_30_original_strict_and_integrity_not_aliased(self):
        p=c.profile();self.assertEqual([],c.policy_errors(p));p['original_strict_exit']=0;self.assertTrue(c.policy_errors(p));p=c.profile();p['integrity_failure_exit']=2;self.assertTrue(c.policy_errors(p))
    def test_30_unsigned_per_ID_fields(self):
        for k,v in [('project_owner_signatory','inferred owner'),('approved_at','2026-10-30'),('owner_decision_status','APPROVED')]:
            r,e=self.one();r[k]=v;self.assertTrue(c.record_errors([r],e))
    def test_30_false_capture_or_runtime_pass(self):
        for k,v in [('capture_authorized',True),('verification_status','PASS')]:
            r,e=self.one('edge');r[k]=v;self.assertTrue(c.record_errors([r],e))
    def test_30_synthetic_candidates_are_conditional(self):
        v=c.read(c.L/'continuation30-safe-fixture-candidates.json');self.assertEqual([],c.safe_scope_errors(v));self.assertEqual(802,len(v['blocked_live_control_ids']))
    def test_30_invented_native_R_or_reference_output(self):
        for key,val in [('executed',True),('reference_output',{'new_p':0.05}),('capture_authorized',True),('scientific_admission','ALWAYS_SAFE_CORE')]:
            v=c.read(c.L/'continuation30-safe-fixture-candidates.json');v['cases'][0][key]=val;self.assertTrue(c.safe_scope_errors(v))
    def test_30_04A02_authorization_not_implied(self):
        v=c.read(c.L/'continuation30-safe-fixture-candidates.json');v['04A02_authorized']=True;self.assertTrue(c.safe_scope_errors(v))
    def test_30_75_original_plan_ids_and_dependencies(self):
        rows=c.plan();self.assertEqual(75,len(rows));self.assertTrue(all(r['dependencies'] and not r['implementation_authorized'] for r in rows));self.assertEqual('04A.48',next(r['batch_id'] for r in rows if r['batch_id']=='04A.48'))
    def test_30_historical_same_size_hash_and_index_drift(self):
        with tempfile.TemporaryDirectory(prefix='beshstat-c30-index-') as name:
            root=pathlib.Path(name);p=root/'old.log';p.write_bytes(b'old\n');e=dict(path='old.log',bytes=4,sha256=c.sha(p));self.assertEqual([],c.artifact_errors([e],root));p.write_bytes(b'bad\n');self.assertTrue(c.artifact_errors([e],root))
    def test_30_history_deletion_rejected(self):
        with tempfile.TemporaryDirectory(prefix='beshstat-c30-history-') as name:
            root=pathlib.Path(name);p=root/'old.log';p.write_bytes(b'old\n');s=p.stat();e=dict(path='old.log',bytes=4,sha256=c.sha(p));m=dict(path='old.log',bytes=4,mtime_ns=s.st_mtime_ns,ctime_ns=s.st_ctime_ns,ino=s.st_ino,dev=s.st_dev);p.unlink();self.assertTrue(c.history_continuity([e],[m],root)[0])
    def test_30_changed_history_metadata_forces_hash(self):
        with tempfile.TemporaryDirectory(prefix='beshstat-c30-history-') as name:
            root=pathlib.Path(name);p=root/'old.log';p.write_bytes(b'old\n');s=p.stat();e=dict(path='old.log',bytes=4,sha256=c.sha(p));m=dict(path='old.log',bytes=4,mtime_ns=s.st_mtime_ns,ctime_ns=s.st_ctime_ns,ino=s.st_ino,dev=s.st_dev);self.assertEqual(([],[]),c.history_continuity([e],[m],root));p.write_bytes(b'bad\n');os.utime(p,ns=(s.st_atime_ns,s.st_mtime_ns+1_000_000_000));errors,rehashes=c.history_continuity([e],[m],root);self.assertTrue(errors);self.assertEqual(['old.log'],rehashes)
    def test_30_original_literal_bytes_remain_exact(self):
        b=c.read(c.H/'before.json')
        for e in b['decision_prefixes']:
            p=pathlib.Path('scripts/windows-migration-audit')/(e['store']+'.json');old=(c.H/'original'/p).read_bytes();new=(c.R/p).read_bytes();self.assertEqual(old,new);self.assertNotEqual(old,b' '+new)
    def test_30_out_of_scope_writes_rejected(self):
        for path in ('BESHstatNG/src/UI/UibyID.vb','scripts/windows-migration-audit/amendment.py','scripts/windows-migration-audit/report.py','docs/architecture/WindowsMigrationLedger/continuation29.md','TestResults/04A-01-continuation-29/final-facts.json','TestResults/04A-01-continuation-30/../../old.log'):self.assertFalse(c.allowed(path),path)
        self.assertTrue(c.allowed('scripts/windows-migration-audit/continuation30_report.py'))
    def test_30_same_byte_history_rewrite_is_not_no_writer_proof(self):
        p=dict(rehashed_changed_metadata=['old.json'],historical_write_protocol='NO_HISTORY_WRITES',historical_rewrite_authorized=False);self.assertTrue(c.preservation_claim_errors(p))
        p['historical_write_protocol']='DEVIATION_PENDING_OWNER_REVIEW';self.assertEqual([],c.preservation_claim_errors(p))
    def test_30_content_identity_never_authorizes_historical_rewrites(self):
        p=dict(rehashed_changed_metadata=['old.json'],historical_write_protocol='DEVIATION_PENDING_OWNER_REVIEW',historical_rewrite_authorized=True);self.assertTrue(c.preservation_claim_errors(p))
if __name__=='__main__':unittest.main(verbosity=2)
