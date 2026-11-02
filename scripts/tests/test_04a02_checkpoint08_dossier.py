"""Audit successor case/criterion scope and ensure conditional permissions release no obligations."""
import hashlib
import json
from pathlib import Path
import unittest

ROOT = Path(__file__).resolve().parents[2]
DOC = ROOT / 'docs/architecture/04A02'


def read(name):
    return json.loads((DOC / name).read_bytes())


class C08DossierTests(unittest.TestCase):
    def test_all_original_cases_criteria_and_consumers_remain(self):
        old = read('04A02-acceptance-coverage-matrix-v4.json')
        new = read('04A02-acceptance-coverage-matrix-v5.json')
        self.assertEqual([x['case_id'] for x in old['cases']], [x['case_id'] for x in new['cases']])
        self.assertEqual([x['criterion_id'] for x in old['criteria']], [x['criterion_id'] for x in new['criteria']])
        self.assertEqual(11, len(new['cases']))
        self.assertEqual(6, len(new['criteria']))
        for a, b in zip(old['cases'], new['cases']):
            for key in ['dataset', 'settings', 'consumed_by', 'blocker_ids']:
                self.assertEqual(a[key], b[key], (a['case_id'], key))

    def test_exact_239_obligations_and_11_groups_release_nothing(self):
        old = read('04A02-C07-exact-ID-disposition.json')
        new = read('04A02-C08-native-risk-ledger.json')
        self.assertEqual(218, new['blocked_unique'])
        self.assertEqual(239, new['deferred_unique'])
        self.assertEqual(11, len(new['risk_groups']))
        self.assertEqual([r['obligation_id'] for r in old['per_id']], [r['obligation_id'] for r in new['per_id']])
        for a, b in zip(old['per_id'], new['per_id']):
            self.assertTrue(all(b[k] == v for k, v in a.items()))
            self.assertFalse(b['obligation_discharged'])
            self.assertFalse(b['N_released'])
        self.assertEqual(0, new['live_attempts'])
        self.assertIsNone(new['independent_risk_review']['reviewer'])

    def test_owner_r_approval_does_not_become_execution_comparison_or_p(self):
        for case in ['C30-SYN-ANOVA', 'C30-SYN-KW']:
            value = read('04A02-C08-R-technical-' + case + '.json')
            self.assertTrue(value['R_OWNER_APPROVED'])
            for key in ['R_TECHNICALLY_QUALIFIED', 'R_EXECUTED', 'REFERENCE_COMPARED', 'P_APPROVED']:
                self.assertFalse(value[key])
            self.assertEqual('R_TECHNICAL_NO_GO', value['status'])
            self.assertEqual(set('abcdefgh'), set(value['checks']))

    def test_all_four_native_families_keep_individual_n1_n2_no_go(self):
        ready = read('04A02-G4-native-capture-ready-v1.json')
        ledger = read('04A02-C08-native-risk-ledger.json')
        self.assertEqual(4, len(ledger['native_cases']))
        for old, new in zip(ready['cases'], ledger['native_cases']):
            for key in ['case_id', 'blocked_control_ids', 'deferred_obligation_ids']:
                self.assertEqual(old[key], new[key])
            self.assertEqual('NO_GO_NOT_ATTEMPTED', new['N1'])
            self.assertEqual('NO_GO_NOT_ATTEMPTED', new['N2'])

    def test_no_invented_independent_reviews_goldens_or_scope_amendments(self):
        packet = read('04A02-C08-independent-review-packet.json')
        for case in packet['cases']:
            self.assertIsNone(case['raw_reference'])
            self.assertIsNone(case['field_comparison'])
            self.assertIsNone(case['reviewer_1']['name'])
            self.assertIsNone(case['reviewer_2']['name'])
            self.assertEqual('NO', case['P'])
        amendments = read('04A02-C08-narrow-amendment-options.json')
        self.assertFalse(amendments['blanket_deferral'])
        self.assertEqual('NONE_SELECTED_NONE_APPROVED', amendments['status'])
        self.assertTrue(all(x['status'] == 'PROPOSED_ONLY_NOT_SELECTED_NOT_APPROVED' for x in amendments['options']))

    def test_historical_audit_and_g5_acceptance_stay_blocked(self):
        matrix = read('04A02-acceptance-coverage-matrix-v5.json')
        self.assertFalse(matrix['official_accepted'])
        self.assertFalse(matrix['next_numbered_batch_authorized'])
        self.assertEqual((1, 1, 24, 'UNMET'), tuple(matrix['upstream'][k] for k in
            ['strict_exit', 'profile_exit', 'historical_failures', 'Gate_C2']))

    def test_pinned_packet_artifacts_match_current_bytes(self):
        packet = read('04A02-C08-independent-review-packet.json')
        for value in packet.values():
            if isinstance(value, dict) and set(value) == {'path', 'sha256'}:
                self.assertEqual(value['sha256'], hashlib.sha256((ROOT / value['path']).read_bytes()).hexdigest())


if __name__ == '__main__':
    unittest.main(verbosity=2)
