"""C08 host-free qualification. Dummy protocol values never constitute references."""
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

ROOT = Path(__file__).resolve().parents[2]


def module(name, path):
    spec = importlib.util.spec_from_file_location(name, ROOT / path)
    result = importlib.util.module_from_spec(spec)
    spec.loader.exec_module(result)
    return result


r = module('oracle_c08', 'scripts/04a02/oracle_operator_v3.py')
g = module('gui_c08', 'scripts/04a02/gui_pilot_v2.py')
DOC = ROOT / 'docs/architecture/04A02'


class RAdmission08Tests(unittest.TestCase):
    def setUp(self):
        self.receipt = json.loads((DOC / '04A02-C08-owner-authorization-20261030.json').read_bytes())

    def test_both_exact_owner_statements_admit_each_named_case(self):
        for case in r.CASES:
            self.assertEqual(self.receipt['decision_id'], r.owner_admission(self.receipt, case))

    def test_s_approval_alone_never_admits_r(self):
        self.receipt['cases'][0]['R'] = 'NO'
        with self.assertRaises(r.Refused):
            r.owner_admission(self.receipt, 'C30-SYN-ANOVA')

    def test_altered_missing_reordered_statements_refuse(self):
        for statements in [[], r.STATEMENTS[:1], list(reversed(r.STATEMENTS)), ['changed', r.STATEMENTS[1]]]:
            with self.subTest(statements=statements), self.assertRaises(r.Refused):
                r.owner_admission(dict(self.receipt, statements=statements), 'C30-SYN-ANOVA')

    def test_denied_extra_duplicate_cases_refuse(self):
        for case in ['other', '', '04A02-ANOVA-NATIVE-01']:
            with self.subTest(case=case), self.assertRaises(r.Refused):
                r.owner_admission(self.receipt, case)
        self.receipt['cases'].append(self.receipt['cases'][0])
        with self.assertRaises(r.Refused):
            r.owner_admission(self.receipt, 'C30-SYN-ANOVA')

    def test_policy_drift_and_promotion_refuse(self):
        for key, value in [('policy_documents', {}), ('cases', []), ('official_04A02', 'ACCEPTED')]:
            with self.subTest(key=key), self.assertRaises(r.Refused):
                r.owner_admission(dict(self.receipt, **{key: value}), 'C30-SYN-KW')
        self.receipt['cases'][0]['P'] = 'YES'
        with self.assertRaises(r.Refused):
            r.owner_admission(self.receipt, 'C30-SYN-KW')

    def test_frozen_pending_form_or_disabled_manifest_not_r_admission(self):
        for path in ['04A02-C07-scientific-decision-form.json', 'oracle-proposals/manifest-v2.json']:
            with self.subTest(path=path), self.assertRaises(r.Refused):
                r.owner_admission(json.loads((DOC / path).read_bytes()), 'C30-SYN-ANOVA')

    def test_successor_manifest_checks_all_current_source_pins(self):
        path = DOC / 'oracle-proposals/manifest-v3.json'
        for case in r.CASES:
            manifest, _, _, actual, fixture, policy = r.admission(path, r.digest(path), case)
            self.assertEqual(case, actual['case_id'])
            self.assertEqual(policy['dataset'], fixture['dataset'])
            self.assertEqual('NO', manifest['P'])

    def test_changed_receipt_script_runtime_and_operator_fail_pinned_reads(self):
        with tempfile.TemporaryDirectory() as folder:
            path = Path(folder) / 'pinned.json'
            original = r.canonical(dict(status='MOCK'))
            path.write_bytes(original)
            for changed in [b'{}', original + b' ', b'changed runtime', b'changed script']:
                path.write_bytes(changed)
                with self.assertRaises(r.Refused):
                    r.pinned(path, r.sha(original))

    def test_no_go_technical_record_never_starts_subprocess(self):
        with tempfile.TemporaryDirectory() as folder:
            path = Path(folder) / 'technical.json'
            path.write_bytes(r.canonical(dict(schema='04A02-C08-R-technical-v1', status='R_TECHNICAL_NO_GO')))
            with patch.object(r.subprocess, 'run') as execute, self.assertRaises(r.Refused):
                r.technical_admission(path, r.digest(path), '0' * 64, 'C30-SYN-ANOVA', Path(folder))
            execute.assert_not_called()

    def test_private_environment_excludes_inherited_profiles_packages_and_locale(self):
        with tempfile.TemporaryDirectory() as folder, patch.dict(os.environ, {
                'HOME': 'ATTACK', 'R_USER': 'ATTACK', 'R_LIBS_USER': 'PERSONAL', 'R_LIBS_SITE': 'ATTACK',
                'R_PROFILE': 'ATTACK', 'R_ENVIRON': 'ATTACK', 'R_DEFAULT_PACKAGES': 'lme4',
                'PATH': 'ATTACK', 'PYTHONPATH': 'ATTACK', 'LC_ALL': 'sk_SK'}):
            job = Path(folder)
            env = r.private_environment(job, Path('C:/Program Files/R/R-4.5.1'))
            self.assertNotIn('PYTHONPATH', env)
            self.assertEqual('utils,stats', env['R_DEFAULT_PACKAGES'])
            self.assertEqual('C', env['LC_ALL'])
            self.assertTrue(all('ATTACK' not in v and 'PERSONAL' not in v for v in env.values()))
            for key in ['HOME', 'R_USER', 'R_LIBS_USER', 'R_LIBS_SITE', 'TEMP', 'TMP', 'TMPDIR']:
                self.assertTrue(Path(env[key]).is_dir())
                self.assertTrue(Path(env[key]).is_relative_to(job))
            for key in ['R_PROFILE', 'R_PROFILE_USER', 'R_ENVIRON', 'R_ENVIRON_USER']:
                self.assertEqual(b'', Path(env[key]).read_bytes())
            with self.assertRaises(FileExistsError):
                r.private_environment(job, job)

    def test_acl_requires_every_ancestor_identity_effective_review_and_no_reparse(self):
        root = ROOT / 'TestResults'
        chain = list(reversed([root, *root.parents]))
        observed = dict(execution_sid='MOCK', ancestors=[dict(path=str(p), owner_sid='OWNER', sddl='MOCK',
            reparse=False, effective_modify_review='INDEPENDENTLY_REVIEWED_EXCLUSIVE', unapproved_modify_sids=[]) for p in chain],
            reviewer='MOCK-NOT-A-REVIEW', independent_review_source='MOCK')
        self.assertEqual([], r.assess_acl(observed, root, 'MOCK'))
        for key, value in [('error', 'access denied'), ('reparse', True), ('sddl', ''),
                           ('effective_modify_review', 'PENDING'), ('unapproved_modify_sids', ['OTHER'])]:
            changed = copy.deepcopy(observed)
            changed['ancestors'][2][key] = value
            self.assertTrue(r.assess_acl(changed, root, 'MOCK'), key)
        self.assertTrue(r.assess_acl(dict(observed, ancestors=observed['ancestors'][1:]), root, 'MOCK'))
        self.assertTrue(r.assess_acl(observed, root, 'DIFFERENT'))

    def test_raw_acl_observation_and_child_only_review_never_qualify(self):
        observed = dict(ancestors=[dict(path=str(ROOT))], execution_sid='MOCK')
        self.assertTrue(r.assess_acl(observed, ROOT, 'MOCK'))
        with self.assertRaises(r.Refused):
            r.reviewed_acl(observed, dict(schema='04A02-owner-output-acl-review-v1', reviewer='MOCK',
                source='MOCK', observed_sha256='0' * 64, ancestors=[]))


class RProtocol08Tests(unittest.TestCase):
    def setUp(self):
        self.temp = tempfile.TemporaryDirectory()
        self.addCleanup(self.temp.cleanup)
        self.job = Path(self.temp.name)
        self.old = json.loads((DOC / 'oracle-proposals/manifest-v2.json').read_bytes())
        self.case, self.fixture = r.v2.load_case(self.old, 'C30-SYN-ANOVA')
        self.policy = json.loads((DOC / '04A02-C07-scientific-decision-form.json').read_bytes())['cases'][0]
        self.runtime_set = 'a' * 64

    def candidate(self):
        case, fixture = self.case, self.fixture
        metrics = {key: 1.0 for key in case['metric_fields']}
        metrics.update(case['df_fields'])
        metrics.update({key: 0.4 for key in case['p_fields']})
        if 'rank_sums' in metrics:
            metrics['rank_sums'] = [1.0, 2.0, 3.0]  # Arbitrary MOCK values, not a reference.
            metrics['tie_correction'] = 0.9
        return dict(schema='04A02-independent-candidate-v2', comparison=dict(case_id=case['case_id'],
            status='PENDING_REVIEW_NOT_A_GOLDEN', source_commit=fixture['source_commit'], dataset_sha256=fixture['dataset_sha256'],
            dataset=fixture['dataset'], row_id_transport='provenance-only', alpha=.05, seed='N/A', culture='InvariantCulture/C',
            options=fixture['options'], method_id=case['method_id'], tail_policy=case['tail_policy'], metrics=metrics),
            provenance=dict(decision_id='MOCK', r_version='4.5.1', stats_version='4.5.1', script_version='proposal-2',
                script_sha256=case['script_sha256'], rscript_sha256=self.policy['runtime_proposal']['rscript_sha256'],
                runtime_set_sha256=self.runtime_set))

    def invoke(self, raw=None, stderr=b'', code=0, error=None, mutate=None):
        def fake(command, **kwargs):
            self.assertEqual(['FAKE-NOT-R.exe', '--vanilla', 'MOCK-SCRIPT', 'MOCK-DCF'], command)
            self.assertEqual(30, kwargs['timeout'])
            self.assertFalse(kwargs['shell'])
            candidate = self.candidate()
            if mutate:
                mutate(candidate)
            kwargs['stdout'].write(r.canonical(candidate) if raw is None else raw)
            kwargs['stderr'].write(stderr)
            if error:
                raise error
            return subprocess.CompletedProcess(command, code)
        with patch.object(r.subprocess, 'run', side_effect=fake):
            r.execute_protocol(['FAKE-NOT-R.exe', '--vanilla', 'MOCK-SCRIPT', 'MOCK-DCF'], self.job, {},
                               'MOCK', self.case, self.fixture, self.policy, self.runtime_set)

    def refused(self, **kwargs):
        with self.assertRaises((r.Refused, subprocess.TimeoutExpired)):
            self.invoke(**kwargs)
        self.assertFalse((self.job / 'publication-v3.json').exists())
        self.assertTrue((self.job / 'raw-stdout.bin').exists())

    def test_mock_anova_has_candidate_only_and_raw_hashes(self):
        self.invoke()
        receipt = json.loads((self.job / 'publication-v3.json').read_bytes())
        self.assertEqual('REFERENCE_EXECUTED_NOT_COMPARED_NOT_GOLDEN', receipt['status'])
        self.assertEqual(r.digest(self.job / 'raw-stdout.bin'), receipt['hashes']['raw-stdout.bin'])

    def test_mock_kw_retains_raw_and_corrected_fields(self):
        self.case, self.fixture = r.v2.load_case(self.old, 'C30-SYN-KW')
        self.policy = json.loads((DOC / '04A02-C07-scientific-decision-form.json').read_bytes())['cases'][1]
        self.invoke()
        value = json.loads((self.job / 'reference-candidate.json').read_bytes())
        self.assertIn('TieCorrectedPValue', value['candidate']['comparison']['metrics'])
        self.assertIn('PValue', value['candidate']['comparison']['metrics'])

    def test_stderr_warning_refuses(self):
        self.refused(stderr=b'Warning: MOCK')

    def test_nonzero_subprocess_refuses(self):
        self.refused(code=1)

    def test_timeout_retains_partial_raw_and_never_publishes(self):
        self.refused(error=subprocess.TimeoutExpired('FAKE', 30))

    def test_malformed_duplicate_or_nonfinite_json_refuses(self):
        self.refused(raw=b'{"schema":1,"schema":2,"x":NaN}\n')

    def test_partial_json_refuses(self):
        self.refused(raw=b'{"schema":')

    def test_runtime_attestation_change_refuses(self):
        self.refused(mutate=lambda v: v['provenance'].update(r_version='4.4.3'))

    def test_extra_metric_refuses(self):
        self.refused(mutate=lambda v: v['comparison']['metrics'].update(EXTRA=1))

    def test_ordinary_p_domain_refuses_boundary(self):
        self.refused(mutate=lambda v: v['comparison']['metrics'].update(PValue=1e-13))

    def test_collision_never_overwrites_existing_raw(self):
        (self.job / 'raw-stdout.bin').write_bytes(b'RETAIN')
        with self.assertRaises(FileExistsError):
            self.invoke()
        self.assertEqual(b'RETAIN', (self.job / 'raw-stdout.bin').read_bytes())

    def test_concurrent_publication_has_one_winner_preserves_both_staging_files(self):
        def publish(data):
            try:
                r.v2.publish_complete(self.job, 'concurrent.json', data)
                return True
            except FileExistsError:
                return False
        with ThreadPoolExecutor(2) as pool:
            results = list(pool.map(publish, [b'ONE', b'TWO']))
        self.assertEqual(1, sum(results))
        self.assertEqual(2, len(list(self.job.glob('pending-*.bin'))))
        self.assertIn((self.job / 'concurrent.json').read_bytes(), [b'ONE', b'TWO'])

    def test_partial_publication_preserves_staging_on_failed_link(self):
        with patch.object(r.v2.os, 'link', side_effect=OSError('MOCK PUBLICATION FAILURE')):
            with self.assertRaises(OSError):
                r.v2.publish_complete(self.job, 'complete.json', b'RETAIN PARTIAL ATTEMPT')
        self.assertFalse((self.job / 'complete.json').exists())
        self.assertEqual(b'RETAIN PARTIAL ATTEMPT', next(self.job.glob('pending-*.bin')).read_bytes())


class FakeAdapter:
    def __init__(self, expected):
        self.expected = expected
        self.context_data = dict(expected, unexpected_modal=False, other_workbooks=[], security_prompt=False, unknown_provider=False)
        self.state = {k: 'MOCK-OBSERVED' for k in g.STATE}
        self.calls = []
        self.failure = None

    def context(self):
        return dict(self.context_data)

    def workbook_state(self):
        self.calls.append('state')
        return dict(self.state)

    def invoke_reviewed_ribbon_id(self):
        self.calls.append('ribbon')
        if self.failure:
            raise self.failure

    def initialized_controls(self):
        self.calls.append('controls')
        return [dict(automation_id='MOCK-ID', name='MOCK', enabled=True, visible=True, value='10', toggle=None, window_state='MOCK')]

    def screenshot(self):
        return b'MOCK-NOT-A-SCREENSHOT'


class GuiProtocol08Tests(unittest.TestCase):
    def setUp(self):
        self.temp = tempfile.TemporaryDirectory()
        self.addCleanup(self.temp.cleanup)
        self.output = Path(self.temp.name) / 'attempt'
        expected = {key: 'MOCK-' + key for key in g.CONTEXT}
        self.plan = dict(case_id=g.CASE, stage='N1', owner_gui_permission='OWNER_PERMITTED_CONDITIONAL',
            expected=expected, actions=['invoke_reviewed_ribbon_id', 'capture_initialized_state'], max_seconds=15,
            measurements=list(g.STATE), screenshot_allowed=True,
            risk_review={key: 'MOCK-NOT-A-REVIEW' for key in ['reviewer', 'source', 'date', 'shared_refedit_receipt',
                'direct_handler_receipt', 'helper_sha256', 'supervisor_sha256', 'provider_side_effect_review',
                'shutdown_protocol', 'external_deadline_receipt']})
        self.plan['risk_review'].update(status='INDEPENDENTLY_ACCEPTED_N1_ONLY', obligation_ids=sorted(g.OBLIGATIONS),
                                        exact_uia_ids_review=['MOCK-ID'])
        self.adapter = FakeAdapter(expected)

    def invoke(self, **kwargs):
        g.capture_protocol(self.plan, self.adapter, self.output, **kwargs)

    def test_n0_missing_risk_review_blocks_before_any_provider_callback(self):
        self.plan['risk_review'] = {}
        with self.assertRaises(ValueError):
            self.invoke()
        self.assertEqual([], self.adapter.calls)
        self.assertFalse(self.output.exists())

    def test_mock_capture_measures_before_after_and_does_not_authorize_n2(self):
        self.invoke()
        result = json.loads((self.output / 'result.json').read_bytes())
        self.assertEqual('MOCK_PROTOCOL_COMPLETE_NOT_NATIVE_EVIDENCE', result['status'])
        self.assertIn('NO_GO', result['N2'])
        self.assertEqual(['state', 'ribbon', 'controls', 'state'], self.adapter.calls)

    def test_wrong_process_pid_reuse_session_window_owner_or_workbook_blocks(self):
        for key in g.CONTEXT:
            with self.subTest(key=key):
                actual = dict(self.adapter.context_data, **{key: 'CHANGED'})
                with self.assertRaises(ValueError):
                    g.check_context(actual, self.plan['expected'])

    def test_unexpected_modal_or_workbook_blocks(self):
        for key in ['unexpected_modal', 'security_prompt', 'unknown_provider', 'other_workbooks']:
            with self.subTest(key=key), self.assertRaises(ValueError):
                g.check_context(dict(self.adapter.context_data, **{key: ['UNEXPECTED'] if key == 'other_workbooks' else True}),
                                self.plan['expected'])

    def test_provider_getter_context_mutation_is_detected_and_partial_retained(self):
        original = self.adapter.initialized_controls
        def mutate():
            self.adapter.context_data['pid'] = 'REUSED'
            return original()
        self.adapter.initialized_controls = mutate
        with self.assertRaises(ValueError):
            self.invoke()
        self.assertTrue((self.output / 'before.json').exists())
        self.assertTrue((self.output / 'failure.json').exists())

    def test_provider_workbook_side_effect_detected_without_restore(self):
        original = self.adapter.initialized_controls
        def mutate():
            self.adapter.state['dirty_flag'] = 'CHANGED'
            return original()
        self.adapter.initialized_controls = mutate
        with self.assertRaises(ValueError):
            self.invoke()
        self.assertEqual('CHANGED', self.adapter.state['dirty_flag'])
        self.assertTrue((self.output / 'after.json').exists())

    def test_callback_failure_retains_attempt_and_no_retry(self):
        self.adapter.failure = RuntimeError('MOCK CALLBACK')
        with self.assertRaises(RuntimeError):
            self.invoke()
        self.assertEqual(['state', 'ribbon'], self.adapter.calls)
        with self.assertRaises(FileExistsError):
            self.invoke()

    def test_deadline_stops_before_provider(self):
        ticks = iter([0, 16])
        with self.assertRaises(ValueError):
            self.invoke(monotonic=lambda: next(ticks))
        self.assertEqual([], self.adapter.calls)

    def test_failed_screenshot_preserves_initialized_controls(self):
        self.adapter.screenshot = lambda: None
        with self.assertRaises(ValueError):
            self.invoke()
        self.assertTrue((self.output / 'controls.json').exists())
        self.assertFalse((self.output / 'result.json').exists())

    def test_unknown_uia_identifier_has_no_fallback(self):
        self.plan['risk_review']['exact_uia_ids_review'] = ['DIFFERENT-ID']
        with self.assertRaises(ValueError):
            self.invoke()

    def test_n2_or_missing_help_obligation_never_inherits_n1(self):
        self.plan['stage'] = 'N2'
        self.assertTrue(g.prerequisites(self.plan))
        self.plan['stage'] = 'N1'
        self.plan['risk_review']['obligation_ids'].pop()
        self.assertTrue(g.prerequisites(self.plan))

    def test_unobservable_workbook_measurements_cannot_pass_as_no_write(self):
        self.adapter.state['used_range_content'] = None
        with self.assertRaises(ValueError):
            self.invoke()


if __name__ == '__main__':
    unittest.main(verbosity=2)
