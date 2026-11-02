"""C13 actual owner startup admission; no R/Excel process is launched.

Original owner bytes stay outside source-control snapshots. Windows tests read
them through the new hash-pointer fixture. Retained ACL replay is explicitly
not qualification of this process; actual admission still collects a live token.
"""
import copy
import importlib.util
import json
from pathlib import Path
import tempfile
import unittest
from unittest.mock import patch

ROOT=Path(__file__).resolve().parents[2]
spec=importlib.util.spec_from_file_location('c13handoff',ROOT/'scripts/04a02/reference_handoff_c13.py')
h=importlib.util.module_from_spec(spec);spec.loader.exec_module(h);r=h.r


class Startup13Tests(unittest.TestCase):
    def test_real_successful_owner_startup_all_pins_and_native_identities(self):
        for case in h.CASES:
            m,d,c,f,p=h.verified_startup(case)
            self.assertEqual('NO',m['P']);self.assertEqual('4.5.1',p['runtime_proposal']['r_version'])
            self.assertIs(p['runtime_proposal']['scientific_method_approved'],False)
            self.assertEqual('C08-OWNER-20261030-S-R-N-v1',d)

    def test_hash_pointer_fixture_has_no_raw_owner_receipt(self):
        fixture=json.loads((ROOT/'scripts/tests/fixtures/04a02-c13-owner-c12-startup-pointer-v1.json').read_bytes())
        self.assertEqual(h.STARTUP_HASHES,fixture['hashes']);self.assertNotIn('raw_stdout',fixture)
        self.assertEqual('R_RUNTIME_PROBED',fixture['status'])

    def test_tampered_stdout_hash_stops_before_parser(self):
        original=r.digest
        def digest(p):return '0'*64 if p.name=='raw-stdout.bin' else original(p)
        with patch.object(r,'digest',side_effect=digest),self.assertRaisesRegex(r.Refused,'raw-stdout.bin'):h.verified_startup('C30-SYN-ANOVA')

    def test_corrupt_receipt_hash_fails_even_with_exit_zero(self):
        original=r.digest
        def digest(p):return '0'*64 if p.name=='startup-receipt.json' else original(p)
        with patch.object(r,'digest',side_effect=digest),self.assertRaisesRegex(r.Refused,'startup-receipt.json'):h.verified_startup('C30-SYN-KW')

    def test_frozen_v5_rejects_real_successful_v6_stdout(self):
        policy=json.loads((ROOT/r.POLICY).read_bytes())
        with self.assertRaisesRegex(r.Refused,'Compiler namespace/package provenance mismatch'):
            r.v5.validate_probe(r.v2.strict_json((h.JOB/'raw-stdout.bin').read_bytes()),h.JOB,policy)


class Qualification13Tests(unittest.TestCase):
    def setUp(self):
        self.temp=tempfile.TemporaryDirectory();self.addCleanup(self.temp.cleanup)
        self.path=Path(self.temp.name)/'go.json'
        self.observed=r.v2.strict_json((h.OWNER_ROOT/'owner-acl-raw.json').read_bytes())
        self.ns=r.v2.strict_json((ROOT/r.POLICY).read_bytes());self.pp=r.v2.strict_json((ROOT/r.PATH_POLICY).read_bytes())
        def ref(p):return dict(path=str(p),sha256=r.digest(p))
        self.go=dict(schema='04A02-C12-R-technical-v1',status='R_TECHNICAL_GO',case_id='C30-SYN-ANOVA',
            manifest_sha256=h.MANIFEST_SHA,output_root=str(h.OWNER_ROOT),acl_sha256=r.sha(r.canonical(self.observed)),
            acl_review=ref(ROOT/'docs/architecture/04A02/04A02-C11-owner-acl-review-v1.json'),
            startup_receipt=ref(h.JOB/'startup-receipt.json'),
            test_receipt=ref(ROOT/'docs/architecture/04A02/04A02-C12-operator-qualification-tests-v1.json'),
            namespace_policy=dict(path=r.POLICY,sha256=r.digest(ROOT/r.POLICY)),
            path_identity_policy=dict(path=r.PATH_POLICY,sha256=r.digest(ROOT/r.PATH_POLICY)),
            attempt_name=h.CASES['C30-SYN-ANOVA'],P='NO',N2='NO',activation='EXACT_NORMAL_OWNER_CONTEXT_AFTER_HASH_REVIEW')

    def qualify(self):
        self.path.write_bytes(r.canonical(self.go))
        return r.qualification(self.path,r.digest(self.path),h.MANIFEST_SHA,'C30-SYN-ANOVA',h.OWNER_ROOT,self.observed,self.ns,self.pp)

    def test_retained_owner_observation_replay_passes_not_current_token_go(self):self.assertEqual(self.go,self.qualify())
    def test_different_sid_refused(self):
        self.observed['execution_sid']='S-1-5-21-142429299-24905182-1790040237-1012'
        with self.assertRaisesRegex(r.Refused,'Owner-context ACL'):self.qualify()
    def test_different_root_refused(self):
        self.go['output_root']+='-other'
        with self.assertRaisesRegex(r.Refused,'Wrong case/manifest/output root'):self.qualify()
    def test_different_case_refused(self):
        self.go['case_id']='C30-SYN-KW'
        with self.assertRaisesRegex(r.Refused,'Wrong case/manifest/output root'):self.qualify()
    def test_stale_acl_hash_refused(self):
        self.go['acl_sha256']='0'*64
        with self.assertRaisesRegex(r.Refused,'ACL drift/stale'):self.qualify()
    def test_receipt_corrupted_pin_refused(self):
        self.go['startup_receipt']['sha256']='0'*64
        with self.assertRaises(r.Refused):self.qualify()
    def test_test_receipt_corrupted_pin_refused(self):
        self.go['test_receipt']['sha256']='0'*64
        with self.assertRaises(r.Refused):self.qualify()
    def test_namespace_policy_corrupted_pin_refused(self):
        self.go['namespace_policy']['sha256']='0'*64
        with self.assertRaisesRegex(r.Refused,'namespace policy drift'):self.qualify()
    def test_path_policy_corrupted_pin_refused(self):
        self.go['path_identity_policy']['sha256']='0'*64
        with self.assertRaisesRegex(r.Refused,'path identity policy drift'):self.qualify()
    def test_no_current_go_from_startup_only(self):
        self.go['status']='STARTUP_ONLY_NOT_STATISTICAL_GO'
        with self.assertRaisesRegex(r.Refused,'No current technical GO'):self.qualify()
    def test_p_promotion_refused_by_handoff(self):
        self.go['P']='YES'
        with self.assertRaisesRegex(r.Refused,'not authorized'):h.validate_go_scope(self.go,'C30-SYN-ANOVA','a','a')
    def test_n2_refused_by_handoff(self):
        self.go['N2']='YES'
        with self.assertRaisesRegex(r.Refused,'not authorized'):h.validate_go_scope(self.go,'C30-SYN-ANOVA','a','a')
    def test_review_requires_exact_bytes(self):
        with self.assertRaisesRegex(r.Refused,'exact GO bytes'):h.validate_go_scope(self.go,'C30-SYN-ANOVA','a','b')
    def test_other_attempt_refused(self):
        self.go['attempt_name']='c12-startup-20261101-v1'
        with self.assertRaisesRegex(r.Refused,'one-shot attempt'):h.validate_go_scope(self.go,'C30-SYN-ANOVA','a','a')


class Boundaries13Tests(unittest.TestCase):
    def value(self):return r.v2.strict_json((h.JOB/'raw-stdout.bin').read_bytes())
    def validate(self,v):return r.validate_probe(v,h.JOB,r.v2.strict_json((ROOT/r.POLICY).read_bytes()),r.v2.strict_json((ROOT/r.PATH_POLICY).read_bytes()))
    def test_unexpected_namespace(self):
        value=self.value();value['namespaces'].append('thirdparty')
        with self.assertRaises(r.Refused):self.validate(value)
    def test_invalid_short_alias_identity(self):
        value=self.value();value['compiler']['package_path']='C:/PROGRA~1/R/R-45~1.1/library/base'
        with self.assertRaises((r.Refused,r.identity.PathIdentityError)):self.validate(value)
    def test_injected_library_path(self):
        value=self.value();value['libraries'].append('C:/personal')
        with self.assertRaises(r.Refused):self.validate(value)
    def test_injected_profile(self):
        value=self.value();value['environment']['R_PROFILE_USER']='C:/personal.R'
        with self.assertRaises(r.Refused):self.validate(value)
    def test_output_collision(self):
        with tempfile.TemporaryDirectory() as temp:
            root=Path(temp);(root/'already').mkdir()
            with self.assertRaises(r.Refused):r.new_attempt_path(root,'already')
    def test_publication_collision_retains_original_and_partial(self):
        with tempfile.TemporaryDirectory() as temp:
            root=Path(temp);target=root/'reference-candidate.json';target.write_bytes(b'old')
            with self.assertRaises((r.Refused,FileExistsError,OSError)):r.v2.publish_complete(root,target.name,b'new')
            self.assertEqual(b'old',target.read_bytes());self.assertGreaterEqual(len(list(root.iterdir())),1)
    def test_changed_reparse_metadata_rejected_before_forwarding(self):
        with patch.object(r.identity.WindowsApi,'local_ntfs',side_effect=OSError('reparse/race refused')),self.assertRaisesRegex(r.Refused,'identity query failed'):self.validate(self.value())
    def test_consumed_startups_always_refused(self):
        for name in ['c10-startup-20261101-v1','c11-startup-20261101-v1','c12-startup-20261101-v1']:
            with self.assertRaises(r.Refused):r.new_attempt_path(h.OWNER_ROOT,name)
    def test_failed_attempts_not_retroactively_qualified(self):
        for name in ['c10-startup-20261101-v1','c11-startup-20261101-v1']:
            job=h.OWNER_ROOT/name;self.assertTrue(list(job.glob('failure-*.json')));self.assertFalse((job/'startup-receipt.json').exists())


if __name__=='__main__':unittest.main(verbosity=2)
