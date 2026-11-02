"""Actual retained reference readers and synthetic review-schema adversaries.

All reviewer names/signatures/verification below are TEST FIXTURES, never human
reviews or authority. No R/Excel process or production promotion is executed.
"""
import copy
import hashlib
import importlib.util
import json
from pathlib import Path
import tempfile
import unittest
from unittest.mock import patch
ROOT=Path(__file__).resolve().parents[2]
spec=importlib.util.spec_from_file_location('review14',ROOT/'scripts/04a02/scientific_review_c14.py')
v=importlib.util.module_from_spec(spec);spec.loader.exec_module(v);r=v.reader.r
DOC=ROOT/'docs/architecture/04A02'
def sha(p):return hashlib.sha256(p.read_bytes()).hexdigest()
def ref(p):return dict(path=str(p),sha256=sha(p))

class Actual14Tests(unittest.TestCase):
    def test_actual_two_owner_publications_and_both_cultures_reproduce(self):
        for cid in v.reader.PUBLICATIONS:
            case=v.reader.load_case(cid);n=10 if cid.endswith('ANOVA') else 4
            self.assertTrue(all(c['mapped_pass']==n for c in case['comparisons']))
            self.assertEqual([0,0] if n==10 else [4,4],[c['reference_only'] for c in case['comparisons']])
            self.assertEqual('NOT_CAPTURED_NOT_COMPARED',case['rendered_excel_format'])
    def test_actual_owner_receipts_are_not_scientific_signatures(self):
        for short in ['anova','kw']:
            receipt=json.loads((ROOT/('TestResults/04A-02-c13-owner-review-'+short+'-20261101-v1.json')).read_bytes())
            self.assertEqual('NOT_A_SCIENTIFIC_REVIEW',receipt['scientific_review']);self.assertNotIn('signature',receipt)
    def test_unknown_case_refused(self):
        with self.assertRaises(r.Refused):v.reader.load_case('OTHER')
    def test_wrong_publication_hash_refused(self):
        job=v.reader.h.OWNER_ROOT/v.reader.h.CASES['C30-SYN-ANOVA']
        with self.assertRaisesRegex(r.Refused,'hash mismatch'):v.reader.verify_publication(job,'0'*64)
    def test_member_parent_traversal_refused(self):
        with self.assertRaises(r.Refused):v.reader.relative_name('../secret')
    def test_member_absolute_refused(self):
        with self.assertRaises(r.Refused):v.reader.relative_name('C:/private')
    def test_member_ads_refused(self):
        with self.assertRaises(r.Refused):v.reader.relative_name('raw-stdout.bin:stream')
    def test_duplicate_json_refused(self):
        with self.assertRaises((r.Refused,ValueError)):r.v2.strict_json(b'{"x":1,"x":2}')
    def test_nonfinite_json_refused(self):
        with self.assertRaises((r.Refused,ValueError)):r.v2.strict_json(b'{"x":NaN}')
    def test_current_packet_identity_and_p_are_pending(self):
        for slot in [1,2]:
            packet=json.loads((DOC/('04A02-C14-scientific-reviewer-'+str(slot)+'.pending.json')).read_bytes())
            self.assertIsNone(packet['reviewer']['name']);self.assertEqual('NO',packet['P'])
    def test_unc_evidence_path_refused_before_filesystem_or_network(self):
        with patch.object(Path,'lstat',side_effect=AssertionError('Must not access network')),self.assertRaises(r.Refused):v.reader.read(r'\\untrusted-server\share\review.json')
    def test_parent_traversal_refused_before_filesystem(self):
        with patch.object(Path,'lstat',side_effect=AssertionError('Must not access path')),self.assertRaises(r.Refused):v.reader.read('C:/untrusted/../review.json')

class Review14Tests(unittest.TestCase):
    def setUp(self):
        self.temp=tempfile.TemporaryDirectory();self.addCleanup(self.temp.cleanup);self.base=Path(self.temp.name)
        self.packet_refs=[];self.reviews=[]
        for slot in [1,2]:
            path=DOC/('04A02-C14-scientific-reviewer-'+str(slot)+'.pending.json');packet=json.loads(path.read_bytes())
            signature=self.base/('TEST-ONLY-signature-'+str(slot));signature.write_bytes(('SYNTHETIC SIGNATURE FIXTURE '+str(slot)).encode())
            identity='SYNTHETIC-TEST-IDENTITY-'+str(slot);verification=self.base/('TEST-ONLY-verification-'+str(slot)+'.json')
            verification.write_text(json.dumps(dict(status='OWNER_VERIFIED_EXTERNAL_HUMAN_REVIEW',verifier='SYNTHETIC TEST ONLY',source='UNIT TEST; NOT REAL REVIEW EVIDENCE',date='2026-11-01',reviewer_identity_id=identity,signature_sha256=sha(signature),packet_sha256=sha(path))),encoding='utf-8')
            review=dict(schema='04A02-C14-completed-scientific-review-v1',status='COMPLETED',slot=slot,packet=ref(path),reviewer=dict(identity_id=identity,name='SYNTHETIC TEST REVIEWER '+str(slot),role='TEST FIXTURE',qualification='SIMULATED ONLY',origin='EXTERNAL_HUMAN',independent_of_codex_and_other_reviewer=True),date='2026-11-01T19:00:00Z',signature=dict(kind='SIGNED_EXTERNAL_RECORD',record=ref(signature)),verification_receipt=ref(verification),cases=packet['required_case_bindings'],verdict='APPROVE_EVIDENCE',deviations=[],tolerance_changes=[],P='NO')
            self.packet_refs.append(ref(path));self.reviews.append(review)
    def one(self):return v.completed_review(self.reviews[0],self.packet_refs[0])
    def both(self):return v.two_reviews(*self.reviews,*self.packet_refs)
    def test_complete_synthetic_schema_links_pass_but_do_not_prove_humans_or_promote(self):
        result=self.both();self.assertFalse(result['promoted']);self.assertEqual('NO',result['P']);self.assertFalse(result['human_authenticity_machine_proven'])
    def test_absent_identity(self):
        del self.reviews[0]['reviewer']['identity_id']
        with self.assertRaises(r.Refused):self.one()
    def test_blank_name(self):
        self.reviews[0]['reviewer']['name']=' '
        with self.assertRaises(r.Refused):self.one()
    def test_missing_qualification(self):
        self.reviews[0]['reviewer']['qualification']=''
        with self.assertRaises(r.Refused):self.one()
    def test_automated_review_is_not_external_human(self):
        self.reviews[0]['reviewer']['origin']='CODEX'
        with self.assertRaises(r.Refused):self.one()
    def test_same_reviewer_normalized_name(self):
        self.reviews[1]['reviewer']['name']='  SYNTHETIC test reviewer 1 '
        with self.assertRaisesRegex(r.Refused,'Same reviewer'):self.both()
    def test_same_identity_id_even_with_other_name(self):
        self.reviews[1]['reviewer']['identity_id']=self.reviews[0]['reviewer']['identity_id']
        path=Path(self.reviews[1]['verification_receipt']['path']);value=json.loads(path.read_bytes());value['reviewer_identity_id']=self.reviews[0]['reviewer']['identity_id'];path.write_text(json.dumps(value));self.reviews[1]['verification_receipt']=ref(path)
        with self.assertRaisesRegex(r.Refused,'Same reviewer'):self.both()
    def test_wrong_case(self):
        self.reviews[0]['cases'][0]['case_id']='UNSUPPORTED'
        with self.assertRaises(r.Refused):self.one()
    def test_wrong_publication_hash(self):
        self.reviews[0]['cases'][0]['publication']['sha256']='0'*64
        with self.assertRaises(r.Refused):self.one()
    def test_missing_reference(self):
        del self.reviews[0]['cases'][0]['raw_stdout']
        with self.assertRaises(r.Refused):self.one()
    def test_changed_policy_hash(self):
        self.reviews[0]['cases'][0]['scientific_policy']['sha256']='0'*64
        with self.assertRaises(r.Refused):self.one()
    def test_tolerance_change_unapproved(self):
        self.reviews[0]['tolerance_changes']=['widen p to 1e-6']
        with self.assertRaises(r.Refused):self.one()
    def test_unsigned_review(self):
        self.reviews[0]['signature']['kind']='UNSIGNED'
        with self.assertRaises(r.Refused):self.one()
    def test_tampered_signed_record(self):
        Path(self.reviews[0]['signature']['record']['path']).write_bytes(b'TAMPERED')
        with self.assertRaises(r.Refused):self.one()
    def test_empty_unsigned_record(self):
        path=Path(self.reviews[0]['signature']['record']['path']);path.write_bytes(b'');self.reviews[0]['signature']['record']=ref(path)
        with self.assertRaises(r.Refused):self.one()
    def test_reused_verification_receipt(self):
        self.reviews[1]['verification_receipt']=self.reviews[0]['verification_receipt']
        with self.assertRaises(r.Refused):self.both()
    def test_missing_external_verifier(self):
        path=Path(self.reviews[0]['verification_receipt']['path']);value=json.loads(path.read_bytes());value['verifier']=None;path.write_text(json.dumps(value));self.reviews[0]['verification_receipt']=ref(path)
        with self.assertRaises(r.Refused):self.one()
    def test_wrong_packet_verification(self):
        self.reviews[0]['packet']['sha256']='0'*64
        with self.assertRaises(r.Refused):self.one()
    def test_pending_is_not_completed(self):
        self.reviews[0]['status']='PENDING'
        with self.assertRaises(r.Refused):self.one()
    def test_p_before_reviews_refused(self):
        self.reviews[0]['P']='YES'
        with self.assertRaises(r.Refused):self.both()
    def test_second_review_absent(self):
        with self.assertRaises(r.Refused):v.two_reviews(self.reviews[0],{},*self.packet_refs)
    def test_rejection_blocks_readiness(self):
        self.reviews[1]['verdict']='REJECT'
        with self.assertRaises(r.Refused):self.both()
    def test_unresolved_deviations_block_readiness(self):
        self.reviews[0]['deviations']=['unresolved statistical concern']
        with self.assertRaises(r.Refused):self.both()
    def test_additional_unknown_fields_fail_closed(self):
        self.reviews[0]['override_P']=True
        with self.assertRaises(r.Refused):self.one()
    def test_unreviewed_replacement_packet_is_not_authority(self):
        path=self.base/'replacement-packet.json';original=Path(self.packet_refs[0]['path']);value=json.loads(original.read_bytes());value['required_case_bindings'][0]['case_id']='UNSUPPORTED';path.write_text(json.dumps(value));new_ref=ref(path)
        self.reviews[0]['packet']=new_ref
        with self.assertRaisesRegex(r.Refused,'replacement packet'):v.completed_review(self.reviews[0],new_ref)
    def test_same_signature_cannot_count_twice(self):
        self.reviews[1]['signature']=copy.deepcopy(self.reviews[0]['signature'])
        path=Path(self.reviews[1]['verification_receipt']['path']);value=json.loads(path.read_bytes());value['signature_sha256']=self.reviews[1]['signature']['record']['sha256'];path.write_text(json.dumps(value));self.reviews[1]['verification_receipt']=ref(path)
        with self.assertRaisesRegex(r.Refused,'Reused signature'):self.both()

if __name__=='__main__':unittest.main(verbosity=2)
