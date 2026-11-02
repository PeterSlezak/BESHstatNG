"""C03 uses dummy files and mocked subprocesses ONLY; never invokes R/Rscript."""
from concurrent.futures import ThreadPoolExecutor
import copy
import importlib.util
import json
import os
from pathlib import Path
import subprocess
import tempfile
import unittest
from unittest.mock import patch

SPEC = importlib.util.spec_from_file_location('oracle_operator03', Path(__file__).resolve().parents[1] / '04a02/oracle_operator.py')
operator = importlib.util.module_from_spec(SPEC)
SPEC.loader.exec_module(operator)


class OracleOperatorTests(unittest.TestCase):
    def setUp(self):
        self.temp = tempfile.TemporaryDirectory(prefix='04A02-C03-SCRATCH-NOT-R-')
        self.addCleanup(self.temp.cleanup)
        self.root = Path(self.temp.name)
        self.home = self.root / 'FAKE-R-NOT-EXECUTABLE'
        (self.home / 'bin/x64').mkdir(parents=True)
        self.exe = self.home / 'bin/x64/Rscript.exe'
        self.exe.write_bytes(b'DUMMY TEST BYTES; NOT AN EXECUTABLE')
        self.dll = self.home / 'bin/x64/R.dll'
        self.dll.write_bytes(b'DUMMY TEST RUNTIME; NOT R')
        self.output = self.root / 'new-output'
        self.output.mkdir()
        self.manifest = json.loads((operator.ROOT / 'docs/architecture/04A02/oracle-proposals/manifest-v2.json').read_bytes())
        self.case = self.manifest['cases'][0]
        self.manifest_path = self.root / 'scratch-manifest.json'
        self.manifest_path.write_bytes(operator.canonical(self.manifest))
        self.decision = dict(schema='04A02-owner-reference-decision-v2', status='OWNER_AUTHORIZED_REFERENCE_EXECUTION',
            decision_id='MOCK-ONLY-NOT-OWNER-AUTHORITY', case_id=self.case['case_id'], scientific_method_approved=True,
            domain_owner_decision_id='MOCK-DOMAIN-ONLY', roadmap_owner_execution_decision_id='MOCK-EXECUTION-ONLY',
            tolerance_policy_id='MOCK-SCHEMA-ONLY', release_gates=dict(run_independent_reference=True,
            promote_numerical_golden=False, native_workbook_capture=False),
            proposal_manifest_sha256=operator.sha(self.manifest_path.read_bytes()), runner_sha256=self.manifest['runner_sha256'],
            script_sha256=self.case['script_sha256'], r_version='4.5.1', stats_version='4.5.1',
            rscript_sha256=operator.sha(self.exe.read_bytes()), runtime_closure_reviewed=True, output_acl_reviewed=True,
            runtime_components=[dict(relative_path='bin/x64/R.dll', sha256=operator.sha(self.dll.read_bytes()))],
            method_id=self.case['method_id'], tail_policy=self.case['tail_policy'], finite_policy=self.case['finite_policy'],
            extreme_policy='reject-outside-approved-ordinary-range', ordinary_p_range=dict(min=0.001, max=0.999),
            tolerances={key: dict(atol=0.0, rtol=0.0, rationale='MOCK SHAPE TEST; NOT SCIENTIFIC APPROVAL') for key in self.case['tolerance_classes']})
        self.decision_path = self.root / 'scratch-decision-NOT-AN-APPROVAL.json'

    def candidate(self, command):
        dcf = operator.strict_dcf(Path(command[-1]).read_bytes())
        _, fixture = operator.load_case(self.manifest, self.decision['case_id'])
        metrics = {name: 1.0 for name in self.case['metric_fields']}
        metrics.update(self.case['df_fields'])
        for name in self.case['p_fields']:
            metrics[name] = 0.4  # Arbitrary mock protocol value, not a reference.
        if 'tie_correction' in metrics:
            metrics['tie_correction'] = 0.9
            metrics['rank_sums'] = [1.0, 2.0, 3.0]  # Deliberately not a statistical oracle.
        comparison = dict(case_id=self.case['case_id'], status='PENDING_REVIEW_NOT_A_GOLDEN', source_commit=fixture['source_commit'],
            dataset_sha256=fixture['dataset_sha256'], dataset=fixture['dataset'], row_id_transport='provenance-only',
            alpha=0.05, seed='N/A', culture='InvariantCulture/C', options=fixture['options'], method_id=self.case['method_id'],
            tail_policy=self.case['tail_policy'], metrics=metrics)
        provenance = dict(decision_id=dcf['DecisionId'], r_version='4.5.1', stats_version='4.5.1', script_version='proposal-2',
            script_sha256=dcf['ScriptSHA256'], rscript_sha256=dcf['RscriptSHA256'], runtime_set_sha256=dcf['RuntimeSetSHA256'])
        return dict(schema='04A02-independent-candidate-v2', comparison=comparison, provenance=provenance)

    def invoke(self, mutate=None, stdout=None, stderr=b'', returncode=0, name='new-case'):
        self.decision_path.write_bytes(operator.canonical(self.decision))
        def fake(command, **kwargs):
            self.assertEqual('--vanilla', command[1])
            self.assertFalse(kwargs['shell'])
            self.assertNotEqual(kwargs['stdout'], kwargs['stderr'])
            value = self.candidate(command)
            if mutate:
                mutate(value)
            raw = operator.canonical(value) if stdout is None else stdout
            kwargs['stdout'].write(raw)
            kwargs['stderr'].write(stderr)
            return subprocess.CompletedProcess(command, returncode)
        with patch.object(operator.subprocess, 'run', side_effect=fake) as mocked:
            result = operator.run_reference(self.decision_path, self.manifest_path, self.home, self.output, name)
            self.assertEqual(1, mocked.call_count)
            return result

    def refused_candidate(self, mutate=None, stdout=None, stderr=b'', returncode=0):
        with self.assertRaises((operator.Refused, ValueError)):
            self.invoke(mutate, stdout, stderr, returncode)
        job = self.output / 'new-case'
        self.assertFalse((job / 'candidate.json').exists())
        self.assertFalse((job / 'publication.json').exists())
        self.assertTrue((job / 'raw-stdout.bin').is_file())
        self.assertTrue((job / 'raw-stderr.bin').is_file())

    def refused_preflight(self):
        self.decision_path.write_bytes(operator.canonical(self.decision))
        with patch.object(operator.subprocess, 'run') as execute, self.assertRaises((operator.Refused, FileNotFoundError)):
            operator.run_reference(self.decision_path, self.manifest_path, self.home, self.output, 'new-case')
        execute.assert_not_called()
        self.assertFalse((self.output / 'new-case').exists())

    def test_mocked_anova_candidate_has_receipt_but_never_golden_status(self):
        job = self.invoke()
        value = operator.read_publication(job)
        self.assertEqual('CANDIDATE_ONLY_NOT_GOLDEN', value['status'])
        self.assertEqual('PENDING_REVIEW_NOT_A_GOLDEN', value['candidate']['comparison']['status'])
        self.assertEqual(b'', (job / 'raw-stderr.bin').read_bytes())
        self.assertEqual(2, len(list(job.glob('pending-*.bin'))))

    def test_mocked_kw_keeps_both_h_and_p_pairs_separate(self):
        self.case = self.manifest['cases'][1]
        for key in ('case_id', 'script_sha256', 'method_id', 'tail_policy', 'finite_policy'):
            self.decision[key] = self.case[key]
        self.decision['tolerances'] = {key: dict(atol=0.0, rtol=0.0, rationale='MOCK ONLY') for key in self.case['tolerance_classes']}
        value = operator.read_publication(self.invoke())
        metrics = value['candidate']['comparison']['metrics']
        self.assertTrue({'HStatistic', 'TieCorrectedHStatistic', 'PValue', 'TieCorrectedPValue'} <= set(metrics))

    def test_absent_release_decision_never_starts_process(self):
        with patch.object(operator.subprocess, 'run') as execute, self.assertRaises(FileNotFoundError):
            operator.run_reference(self.root / 'absent.json', self.manifest_path, self.home, self.output, 'new-case')
        execute.assert_not_called()
        with patch('sys.argv', ['oracle_operator.py', '--decision', str(self.root / 'absent.json')]), \
                patch.object(operator.subprocess, 'run') as execute, self.assertRaises(operator.Refused):
            operator.main()
        execute.assert_not_called()

    def test_pending_decision_is_not_execution_authority(self):
        self.decision['status'] = 'PENDING'
        self.refused_preflight()

    def test_method_approval_is_independent_of_execution_gate(self):
        self.decision['scientific_method_approved'] = False
        self.refused_preflight()

    def test_execution_gate_must_be_boolean_true(self):
        self.decision['release_gates']['run_independent_reference'] = 'YES'
        self.refused_preflight()

    def test_promotion_or_native_gate_cannot_be_admitted(self):
        self.decision['release_gates']['promote_numerical_golden'] = True
        self.refused_preflight()
        self.decision['release_gates']['promote_numerical_golden'] = False
        self.decision['release_gates']['native_workbook_capture'] = True
        self.refused_preflight()

    def test_wrong_executable_sha_refuses_before_output(self):
        self.decision['rscript_sha256'] = '0' * 64
        self.refused_preflight()

    def test_wrong_runtime_sha_refuses_before_output(self):
        self.decision['runtime_components'][0]['sha256'] = '0' * 64
        self.refused_preflight()

    def test_wrong_script_sha_refuses_before_output(self):
        self.decision['script_sha256'] = '0' * 64
        self.refused_preflight()

    def test_wrong_runner_sha_refuses_before_output(self):
        self.decision['runner_sha256'] = '0' * 64
        self.refused_preflight()

    def test_wrong_manifest_sha_refuses_before_output(self):
        self.decision['proposal_manifest_sha256'] = '0' * 64
        self.refused_preflight()

    def test_wrong_approved_version_refuses_before_output(self):
        self.decision['r_version'] = '4.6.0'
        self.refused_preflight()

    def test_wrong_scientific_tail_policy_refuses_before_output(self):
        self.decision['tail_policy'] = 'exact-permutation'
        self.refused_preflight()

    def test_runtime_aliases_and_missing_closure_review_are_refused(self):
        self.decision['runtime_closure_reviewed'] = False
        self.refused_preflight()
        self.decision['runtime_closure_reviewed'] = True
        self.decision['runtime_components'].append(dict(relative_path='bin\\x64\\r.dll', sha256='0' * 64))
        self.refused_preflight()

    def test_no_tolerance_defaults_are_invented(self):
        self.decision['tolerances'] = None
        self.refused_preflight()

    def test_unreviewed_output_acl_refuses_before_output(self):
        self.decision['output_acl_reviewed'] = False
        self.refused_preflight()

    def test_runtime_wrong_version_in_stdout_is_refused(self):
        self.refused_candidate(lambda value: value['provenance'].update(r_version='4.6.0'))

    def test_wrong_case_in_stdout_is_refused(self):
        self.refused_candidate(lambda value: value['comparison'].update(case_id='C30-SYN-KW'))

    def test_wrong_dataset_hash_in_stdout_is_refused(self):
        self.refused_candidate(lambda value: value['comparison'].update(dataset_sha256='0' * 64))

    def test_wrong_stdout_runtime_attestation_is_refused(self):
        self.refused_candidate(lambda value: value['provenance'].update(runtime_set_sha256='0' * 64))

    def test_wrong_stdout_policy_is_refused(self):
        self.refused_candidate(lambda value: value['comparison'].update(tail_policy='lower-F'))

    def test_nan_and_infinity_tokens_are_not_json_candidates(self):
        self.refused_candidate(stdout=b'{"n":NaN}\n')

    def test_duplicate_json_keys_are_refused(self):
        self.refused_candidate(stdout=b'{"schema":"a","schema":"b"}\n')

    def test_empty_output_is_refused(self):
        self.refused_candidate(stdout=b'')

    def test_partial_output_is_refused(self):
        self.refused_candidate(stdout=b'{"schema":')

    def test_stdout_log_pollution_is_not_admitted_as_json(self):
        self.refused_candidate(stdout=b'LOG: starting\n{}\n')

    def test_warning_or_stderr_prevents_publication(self):
        self.refused_candidate(stderr=b'Warning: mock\n')

    def test_process_failure_prevents_publication(self):
        self.refused_candidate(returncode=1)

    def test_fractional_row_id_alias_is_refused(self):
        self.refused_candidate(lambda value: value['comparison']['dataset']['row_ids'][0].__setitem__(0, 101.0))

    def test_boolean_data_alias_is_refused(self):
        self.refused_candidate(lambda value: value['comparison']['dataset']['values'][0].__setitem__(0, True))

    def test_wrong_df_or_p_domain_is_refused(self):
        self.refused_candidate(lambda value: value['comparison']['metrics'].update(DFBetween=2.0))

    def test_boundary_p_is_not_hidden_by_generic_tolerance(self):
        self.refused_candidate(lambda value: value['comparison']['metrics'].update(PValue=0.0))

    def test_malformed_unicode_is_refused(self):
        with self.assertRaises(operator.Refused):
            operator.strict_json(br'{"text":"\ud800"}')

    def test_dcf_duplicates_empty_fields_or_second_record_are_refused(self):
        good = operator.make_dcf({key: 'VALUE' for key in operator.DCF_FIELDS})
        self.assertEqual(len(operator.DCF_FIELDS), len(operator.strict_dcf(good)))
        for changed in [good.replace(b'CaseId: VALUE', b'DecisionId: VALUE'), good + b'\n' + good,
                        good.replace(b'RVersion: VALUE', b'RVersion: '), good.replace(b'\n', b'\r\n'), good[:-1]]:
            with self.subTest(changed=changed[:40]), self.assertRaises(operator.Refused):
                operator.strict_dcf(changed)

    def test_windows_path_alias_and_device_syntax_is_refused(self):
        for name in ['..', '.', 'CON', 'NUL.txt', 'COM1', 'LPT9.ext', 'COM¹', 'LPT²', 'CONIN$', 'name.', 'name ', 'file:stream', 'a/b', 'a\\b']:
            with self.subTest(name=name), self.assertRaises(operator.Refused):
                operator.component(name)
        for name in ['../R.dll', 'bin//R.dll', 'C:relative', '//server/share', 'bin/R.dll:stream']:
            with self.subTest(name=name), self.assertRaises(operator.Refused):
                operator.relative_component_path(name)

    def test_existing_destination_is_unchanged(self):
        target = self.output / 'candidate.json'
        target.write_bytes(b'KEEP ORIGINAL')
        before = target.stat()
        with self.assertRaises(FileExistsError):
            operator.publish_complete(self.output, 'candidate.json', b'new data')
        self.assertEqual(b'KEEP ORIGINAL', target.read_bytes())
        self.assertEqual(before.st_mtime_ns, target.stat().st_mtime_ns)
        self.assertEqual(1, len(list(self.output.glob('pending-*.bin'))))

    def test_concurrent_collision_has_exactly_one_complete_winner(self):
        def contender(data):
            try:
                operator.publish_complete(self.output, 'candidate.json', data)
                return True
            except FileExistsError:
                return False
        with ThreadPoolExecutor(max_workers=2) as pool:
            results = list(pool.map(contender, [b'A' * 50000, b'B' * 50000]))
        self.assertEqual(1, sum(results))
        self.assertIn((self.output / 'candidate.json').read_bytes(), [b'A' * 50000, b'B' * 50000])
        self.assertEqual(2, len(list(self.output.glob('pending-*.bin'))))

    def test_case_insensitive_existing_target_alias_cannot_be_replaced(self):
        (self.output / 'CANDIDATE.JSON').write_bytes(b'KEEP CASE ALIAS')
        with self.assertRaises(FileExistsError):
            operator.publish_complete(self.output, 'candidate.json', b'new')
        self.assertEqual(b'KEEP CASE ALIAS', (self.output / 'CANDIDATE.JSON').read_bytes())

    def test_failed_flush_leaves_staging_without_final_target(self):
        with patch.object(operator.os, 'fsync', side_effect=OSError('MOCK ENOSPC')), self.assertRaises(OSError):
            operator.publish_complete(self.output, 'candidate.json', b'partial-or-complete-pending')
        self.assertFalse((self.output / 'candidate.json').exists())
        self.assertEqual(1, len(list(self.output.glob('pending-*.bin'))))

    def test_link_permission_failure_has_no_copy_fallback(self):
        with patch.object(operator.os, 'link', side_effect=PermissionError('MOCK ACL')), self.assertRaises(PermissionError):
            operator.publish_complete(self.output, 'candidate.json', b'complete but unpublished')
        self.assertFalse((self.output / 'candidate.json').exists())
        self.assertEqual(1, len(list(self.output.glob('pending-*.bin'))))

    def test_existing_execution_directory_is_never_reused(self):
        (self.output / 'new-case').mkdir()
        with self.assertRaises(FileExistsError):
            self.invoke()
        self.assertEqual([], list((self.output / 'new-case').iterdir()))

    def test_commit_receipt_is_required_and_tampering_is_detected(self):
        job = self.invoke()
        (job / 'raw-stdout.bin').write_bytes(b'AFTER-RUN SCRATCH TAMPER')
        with self.assertRaises(operator.Refused):
            operator.read_publication(job)

    def test_receipt_collision_retains_existing_file_and_is_not_published(self):
        original = operator.os.link
        def collide(source, destination):
            if Path(destination).name == 'publication.json':
                Path(destination).write_bytes(b'EXISTING COLLISION OWNER')
            return original(source, destination)
        with patch.object(operator.os, 'link', side_effect=collide), self.assertRaises(FileExistsError):
            self.invoke()
        job = self.output / 'new-case'
        self.assertEqual(b'EXISTING COLLISION OWNER', (job / 'publication.json').read_bytes())
        self.assertTrue((job / 'candidate.json').is_file())
        self.assertEqual(2, len(list(job.glob('pending-*.bin'))))
        with self.assertRaises(operator.Refused):
            operator.read_publication(job)

    def test_decision_manifest_and_script_preflight_copies_are_exact(self):
        job = self.invoke()
        self.assertEqual(self.decision_path.read_bytes(), (job / 'decision.json').read_bytes())
        self.assertEqual(self.manifest_path.read_bytes(), (job / 'proposal-manifest.json').read_bytes())
        self.assertEqual(self.case['script_sha256'], operator.sha((job / 'proposal-source.R').read_bytes()))

    def test_reparse_object_is_refused_without_creating_one(self):
        with patch.object(Path, 'lstat', return_value=type('MockStat', (), {'st_mode': 0o40700, 'st_file_attributes': 0x400})()):
            with self.assertRaises(operator.Refused):
                operator.no_reparse(self.output)

    def test_windows_input_lock_denies_source_writes(self):
        with operator.locked_path(self.dll):
            with self.assertRaises(PermissionError):
                self.dll.write_bytes(b'TAMPER')
        self.assertEqual(b'DUMMY TEST RUNTIME; NOT R', self.dll.read_bytes())

    def test_v1_proposals_are_not_supported_for_execution(self):
        self.manifest['schema'] = '04A02-oracle-source-proposal-v1'
        self.manifest_path.write_bytes(operator.canonical(self.manifest))
        self.decision['proposal_manifest_sha256'] = operator.sha(self.manifest_path.read_bytes())
        self.refused_preflight()

    def test_timeout_retains_capture_files_without_publication(self):
        self.decision_path.write_bytes(operator.canonical(self.decision))
        with patch.object(operator.subprocess, 'run', side_effect=subprocess.TimeoutExpired('MOCK-NOT-R', 30)):
            with self.assertRaises(subprocess.TimeoutExpired):
                operator.run_reference(self.decision_path, self.manifest_path, self.home, self.output, 'new-case')
        self.assertTrue((self.output / 'new-case/raw-stdout.bin').is_file())
        self.assertTrue((self.output / 'new-case/raw-stderr.bin').is_file())
        self.assertFalse((self.output / 'new-case/publication.json').exists())

    def test_v1_bytes_and_v2_source_only_manifest_are_preserved(self):
        manifest = self.manifest
        self.assertTrue(all(value is False for value in manifest['release_gates'].values()))
        self.assertFalse(manifest['runtime_qualified'])
        for case in manifest['cases']:
            self.assertEqual('NOT_EXECUTED', case['execution_status'])
            self.assertEqual(case['script_sha256'], operator.sha((operator.ROOT / case['script']).read_bytes()))
            prior = case['supersedes']
            self.assertEqual(prior['script_sha256'], operator.sha((operator.ROOT / prior['script']).read_bytes()))
            text = (operator.ROOT / case['script']).read_text(encoding='utf-8')
            self.assertNotIn('open = "wx"', text)
            self.assertNotIn('file(args[', text)
            self.assertIn('cat(payload, "\\n", sep = "")', text)
            self.assertIn('anyDuplicated(fields)', text)


if __name__ == '__main__':
    unittest.main(verbosity=2)
