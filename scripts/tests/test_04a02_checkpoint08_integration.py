"""Whole successor admission refuses before any subprocess on current NO-GO evidence."""
import copy
import importlib.util
import json
from pathlib import Path
import tempfile
import unittest
from unittest.mock import patch

ROOT = Path(__file__).resolve().parents[2]
SPEC = importlib.util.spec_from_file_location('oracle_c08_integration', ROOT / 'scripts/04a02/oracle_operator_v3.py')
r = importlib.util.module_from_spec(SPEC)
SPEC.loader.exec_module(r)
DOC = ROOT / 'docs/architecture/04A02'


class RSuccessorIntegration08Tests(unittest.TestCase):
    def setUp(self):
        self.manifest_path = DOC / 'oracle-proposals/manifest-v3.json'
        self.manifest = json.loads(self.manifest_path.read_bytes())

    def test_current_no_go_receipt_blocks_both_cases_before_any_subprocess_or_output(self):
        with tempfile.TemporaryDirectory() as folder:
            output = Path(folder) / 'MUST-NOT-CREATE'
            for case in r.CASES:
                technical = DOC / ('04A02-C08-R-technical-' + case + '.json')
                with self.subTest(case=case), patch.object(r.subprocess, 'run') as execute, self.assertRaises(r.Refused):
                    r.run_reference(self.manifest_path, r.digest(self.manifest_path), technical, r.digest(technical),
                                    case, output, 'unique-no-go')
                execute.assert_not_called()
                self.assertFalse(output.exists())

    def refused_manifest(self, manifest):
        with tempfile.TemporaryDirectory() as folder:
            path = Path(folder) / 'manifest.json'
            path.write_bytes(r.canonical(manifest))
            with patch.object(r.subprocess, 'run') as execute, self.assertRaises(r.Refused):
                r.admission(path, r.digest(path), 'C30-SYN-ANOVA')
            execute.assert_not_called()

    def test_changed_operator_hash_refuses(self):
        self.manifest['runner_sha256'] = '0' * 64
        self.refused_manifest(self.manifest)

    def test_changed_acl_collector_hash_refuses(self):
        self.manifest['acl_collector_sha256'] = '0' * 64
        self.refused_manifest(self.manifest)

    def test_changed_owner_record_hash_refuses(self):
        self.manifest['owner_receipt']['sha256'] = '0' * 64
        self.refused_manifest(self.manifest)

    def test_changed_script_or_fixture_or_dataset_hash_refuses_even_rehashed_manifest(self):
        for field in ['script_sha256', 'fixture_file_sha256', 'dataset_sha256']:
            changed = copy.deepcopy(self.manifest)
            changed['cases'][0][field] = '0' * 64
            with self.subTest(field=field):
                self.refused_manifest(changed)

    def test_extra_unapproved_statistical_case_refuses(self):
        self.manifest['cases'].append(dict(case_id='UNAPPROVED'))
        self.refused_manifest(self.manifest)

    def test_no_execution_inferred_from_manifest_installation_or_s(self):
        for key, value in [('R_owner_approved', False), ('P', 'YES'), ('schema', '04A02-oracle-source-proposal-v2')]:
            with self.subTest(key=key):
                self.refused_manifest(dict(self.manifest, **{key: value}))

    def test_missing_owner_receipt_refuses_without_subprocess(self):
        self.manifest['owner_receipt']['path'] = 'docs/architecture/04A02/ABSENT-OWNER-RECEIPT.json'
        with self.assertRaises(FileNotFoundError), tempfile.TemporaryDirectory() as folder:
            path = Path(folder) / 'manifest.json'
            path.write_bytes(r.canonical(self.manifest))
            with patch.object(r.subprocess, 'run') as execute:
                try:
                    r.admission(path, r.digest(path), 'C30-SYN-ANOVA')
                finally:
                    execute.assert_not_called()

    def test_false_technical_check_wrong_case_or_command_policy_refuses(self):
        with tempfile.TemporaryDirectory() as folder:
            root = Path(folder)
            mock = dict(schema='04A02-C08-R-technical-v1', status='R_TECHNICAL_GO', case_id='C30-SYN-ANOVA',
                manifest_sha256='a' * 64, output_root=str(root.absolute()), checks={k: 'PASS' for k in 'abcdefgh'},
                command_policy=dict(arguments=['--vanilla'], timeout_seconds=30, stdin='DEVNULL', stderr='MUST_BE_EMPTY', shell=False),
                runtime_review_source='MOCK-NOT-A-REVIEW', isolation_probe_receipt='MOCK', qualification_tests='MOCK')
            variants = [dict(mock, case_id='C30-SYN-KW'), dict(mock, checks={}), dict(mock, command_policy={}),
                        dict(mock, runtime_review_source=None), dict(mock, isolation_probe_receipt=None)]
            for i, value in enumerate(variants):
                path = root / (str(i) + '.json')
                path.write_bytes(r.canonical(value))
                with self.subTest(i=i), self.assertRaises(r.Refused):
                    r.technical_admission(path, r.digest(path), 'a' * 64, 'C30-SYN-ANOVA', root)


if __name__ == '__main__':
    unittest.main(verbosity=2)
