import copy
import json
import os
from pathlib import Path
import tempfile
import threading
import unittest
import capture_contract04a02 as c


def native_plan():
    return c.strict_json((c.DOC / '04A02-G4-native-capture-ready-v1.json').read_bytes())


def mock_capture(case):
    """Synthetic schema examples, never an observed workbook or numerical oracle."""
    rect = dict(left=0, top=0, width=400, height=300)
    axis = dict(min=0, max=1, major_unit=.2, minor_unit=.1, scale='linear', title='MOCK', tick_labels=[])
    return dict(classification='MOCK_SCHEMA_TEST_NOT_NATIVE_CAPTURE', status='NOT_RUN', verified=False,
                case_id=case['case_id'], source_hashes=case['source_hashes'],
                release_decision='MOCK_NOT_AUTHORITY', dataset_sha256=case['dataset_sha256'],
                workbook=case['workbook'], input_range=case['input_range'], output_range=case['output_range'],
                culture=case['culture'], host=case['host'],
                option_before={k: 'MOCK_UNOBSERVED' for k in case['required_options']},
                option_after={k: 'MOCK_UNOBSERVED' for k in case['required_options']},
                control_ids=list(case['exercised_controls']),
                artifacts=[dict(path='mock-schema-only/example.xlsx', sha256='0'*64),
                           dict(path='mock-schema-only/example.png', sha256='1'*64)],
                cells=[dict(sheet=case['workbook']['output_sheet'], address='A1', value=1, type='Int32',
                            display='1', number_format='0', error=None, warning=None, merged_range=None)],
                charts=[dict(chart_id=cid, units='points', chart_area=copy.deepcopy(rect),
                             plot_area=copy.deepcopy(rect), axes=dict(x=copy.deepcopy(axis), y=copy.deepcopy(axis)),
                             title='MOCK', legend=dict(visible=False, position='MOCK'),
                             series=[dict(name=name, x=[0, 1], y=[1, 0], type='MOCK_SCATTER',
                                          labels=[], markers='MOCK', error_bars=None)
                                     for name in case['series_order'][cid]])
                        for cid in case['expected_chart_ids']])


class CaptureContracts(unittest.TestCase):
    def setUp(self):
        self.source = c.strict_json((c.DATA / 'windows-option-chart-source-spec-v1.json').read_bytes())

    def test_complete_source_crosswalk_equality(self):
        self.assertEqual(218, len(c.validate_crosswalk(self.source)))

    def test_all_239_ledger_rows_identified_with_original_digest_convention(self):
        rows = c.obligation_evidence(self.source)
        self.assertEqual(239, len(rows))
        self.assertTrue(all(x['status'] == 'LEDGER_RECORD_IDENTIFIED_NOT_RELEASED' for x in rows))
        self.assertTrue(all(x['source_record_status'] == 'CSV_ROW_HASH_MATCH' for x in rows))
        self.assertTrue(all(x['owners'] and not x['capture_authorized'] for x in rows))

    def test_source_plan_option_and_input_identity(self):
        for case in native_plan()['cases']:
            self.assertEqual(case['dataset_sha256'], __import__('hashlib').sha256(c.canonical(case['dataset'])).hexdigest())
            self.assertIsNone(case['actually_exercised_controls'])
            self.assertEqual('BLOCKED', case['restriction_status'])
            self.assertTrue(case['required_options'])
            self.assertTrue(case['reachability_evidence'])

    def test_two_by_two_has_no_chart(self):
        case = native_plan()['cases'][2]
        self.assertEqual([], case['expected_chart_ids'])
        self.assertEqual([], mock_capture(case)['charts'])

    def test_native_import_refuses_mock_even_with_claimed_approval(self):
        with self.assertRaises(ValueError):
            c.refuse_unreleased_native(mock_capture(native_plan()['cases'][0]), dict(approved=True))

    def test_strict_json_rejects_duplicate_metric_and_case_fields(self):
        for raw in [b'{"PValue":1,"PValue":2}', b'{"case_id":1,"case_id":2}',
                    b'{"nested":{"x":1,"x":2}}', b'{"x":NaN}', b'{"x":Infinity}', b'{"x":1e400}', b'{"x":']:
            with self.subTest(raw=raw), self.assertRaises((ValueError, json.JSONDecodeError)):
                c.strict_json(raw)

    def test_exclusive_output_never_clobbers(self):
        with tempfile.TemporaryDirectory() as folder:
            p = Path(folder) / 'new.json'
            c.exclusive_record(p, dict(classification='comparison', approved_golden=None))
            before = p.read_bytes(), p.stat().st_mtime_ns
            with self.assertRaises(FileExistsError): c.exclusive_record(p, dict(changed=True))
            self.assertEqual(before, (p.read_bytes(), p.stat().st_mtime_ns))

    def test_exclusive_concurrent_collision_has_one_winner(self):
        with tempfile.TemporaryDirectory() as folder:
            p = Path(folder) / 'new.json'
            outcomes = []
            def write():
                try: c.exclusive_record(p, dict(classification='candidate')); outcomes.append('created')
                except FileExistsError: outcomes.append('exists')
            workers = [threading.Thread(target=write) for _ in range(2)]
            for worker in workers: worker.start()
            for worker in workers: worker.join()
            self.assertCountEqual(['created', 'exists'], outcomes)
            self.assertEqual('candidate', c.strict_json(p.read_bytes())['classification'])

    def test_artifact_path_aliases_rejected(self):
        for name in ['../outside', 'C:/outside', 'x:stream', 'CON', 'dir/NUL.png', 'dir/trailing.']:
            with self.subTest(name=name), self.assertRaises(ValueError): c.safe_artifact_path(name)

    def test_canonical_roundtrip_is_deterministic(self):
        case = native_plan()['cases'][3]
        raw = c.canonical(mock_capture(case))
        self.assertEqual(raw, c.canonical(c.strict_json(raw)))

    def test_packet_covers_all_original_cases_and_six_gates(self):
        original = c.strict_json((c.DOC / 'coverage-register-v1.json').read_bytes())
        matrix = c.strict_json((c.DOC / '04A02-acceptance-coverage-matrix-v2.json').read_bytes())
        ids = [x['case_id'] for x in matrix['cases']]
        c.unique(ids, 'matrix cases')
        self.assertTrue({x['case_id'] for x in original['cases']} <= set(ids))
        self.assertEqual({'G0', 'G1', 'G2', 'G3', 'G4', 'G5'}, {x['criterion_id'] for x in matrix['criteria']})
        self.assertFalse(matrix['official_accepted'])
        for row in matrix['criteria']:
            for key in ['required_evidence', 'actual_files', 'validation', 'remaining_blocker', 'owner', 'minimum_next_decision_or_capture']:
                self.assertTrue(row[key])
            for pin in row['actual_files']:
                self.assertEqual(pin['sha256'], c.sha(c.ROOT / pin['path']))

    def test_packet_pending_decisions_confer_no_authority(self):
        packet = c.strict_json((c.DOC / '04A02-owner-closeout-decisions.pending.json').read_bytes())
        self.assertIsNone(packet['owner_identity'])
        self.assertIsNone(packet['decision_date'])
        self.assertFalse(packet['restricted_deferral_amendment']['approved'])
        for case in packet['reference_cases']:
            self.assertFalse(case['scientific_method_approved'])
            self.assertFalse(any(case['release_gates'].values()))
        for case in packet['native_cases']:
            self.assertFalse(case['N'])
            self.assertIsNone(case['executed_regressions'])

    def test_packet_g3_contains_real_candidate_receipts_not_references(self):
        evidence = c.strict_json((c.DOC / '04A02-G3-semantic-evidence-v1.json').read_bytes())
        self.assertIsNone(evidence['reference'])
        self.assertIsNone(evidence['approved_golden'])
        self.assertEqual(4, len(evidence['observations']))
        for pin in evidence['observations']:
            self.assertEqual(pin['sha256'], c.sha(c.ROOT / pin['path']))
            candidate = c.strict_json((c.ROOT / pin['path']).read_bytes())
            self.assertEqual('candidate', candidate['classification'])
            c.validate_candidate(candidate['service'])

    def test_packet_retains_additional_21_deferred_obligations(self):
        plan = native_plan()
        c.validate_ready_plan(plan)
        blocked = {i for x in plan['cases'] for i in x['blocked_control_ids']}
        deferred = {i for x in plan['cases'] for i in x['deferred_obligation_ids']}
        self.assertEqual(218, len(blocked))
        self.assertEqual(239, len(deferred))
        self.assertEqual(21, len(deferred - {'control:'+i for i in blocked}))
        self.assertEqual(deferred, {x['obligation_id'] for x in plan['obligation_trace']})

    def test_ready_manifest_deleted_or_relabelled_deferred_id_rejected(self):
        for defect in ['delete', 'relabel', 'duplicate']:
            plan = native_plan()
            ids = plan['cases'][0]['deferred_obligation_ids']
            if defect == 'delete': ids.pop()
            elif defect == 'relabel': ids[0] += '-invented'
            else: ids.append(ids[0])
            with self.subTest(defect=defect), self.assertRaises(ValueError): c.validate_ready_plan(plan)

    def test_ready_manifest_cannot_fabricate_release_or_runtime_state(self):
        for defect in ['gate', 'runtime', 'owner', 'waiver', 'trace']:
            plan = native_plan()
            if defect == 'gate': plan['release_gates']['N'] = True
            elif defect == 'runtime': plan['cases'][0]['status'] = 'CAPTURED'
            elif defect == 'owner': plan['cases'][0]['owner'] = ''
            elif defect == 'waiver': plan['cases'][0]['waiver_id'] = 'invented'
            else: plan['obligation_trace'].pop()
            with self.subTest(defect=defect), self.assertRaises(ValueError): c.validate_ready_plan(plan)

    def test_packet_no_fake_reference_bundle_or_golden(self):
        self.assertFalse((c.DOC / '04A02-reference-comparison-v1.json').exists())
        self.assertTrue(all(x['status'] == 'NOT_RUN' and x['verification_status'] == 'NOT_VERIFIED'
                            for x in native_plan()['cases']))


class CandidateComparisonTests(unittest.TestCase):
    def setUp(self):
        path = Path(os.environ['BESHSTAT_04A02_G3_CANDIDATES'])
        self.observations = [c.strict_json(p.read_bytes())['service'] for p in sorted(path.glob('C05-G3-*.json'))]
        self.assertEqual(4, len(self.observations), 'fresh execution-backed observations required')

    def test_all_real_candidate_fields_admitted_without_promotion(self):
        for value in self.observations:
            c.validate_candidate(value)
            result = c.compare_candidates(value, copy.deepcopy(value))
            self.assertEqual([], result['differences'])
            self.assertIsNone(result['reference'])
            self.assertIsNone(result['approved_golden'])
            self.assertFalse(result['tolerance_applied'])

    def test_changed_numeric_observation_is_visible_not_tolerance_tuned(self):
        left = self.observations[0]; right = copy.deepcopy(left)
        key = next(iter(right['metrics']))
        right['metrics'][key] += .125
        result = c.compare_candidates(left, right)
        self.assertEqual('CANDIDATE_DIFFERENCE_DETECTED', result['status'])
        self.assertTrue(any(d['path'] == '/metrics/'+key for d in result['differences']))

    def test_scalar_type_alias_visible_in_comparison(self):
        left = self.observations[0]; right = copy.deepcopy(left)
        right['tables'][0]['cells'][1][1]['value'] = str(right['tables'][0]['cells'][1][1]['value'])
        # A declared CLR type cannot hide a differently typed JSON payload.
        with self.assertRaises(ValueError): c.compare_candidates(left, right)

    def test_cross_case_comparison_rejected(self):
        anova = next(o for o in self.observations if o['case_id'] == 'C30-SYN-ANOVA')
        kw = next(o for o in self.observations if o['case_id'] == 'C30-SYN-KW')
        with self.assertRaises(ValueError): c.compare_candidates(anova, kw)


def candidate_mutation(name):
    def test(self):
        value = copy.deepcopy(self.observations[0])
        if name == 'dataset': value['dataset_sha256'] = '0'*64
        elif name == 'fixture': value['fixture_sha256'] = '0'*64
        elif name == 'source': value['source_commit'] = '0'*40
        elif name == 'alpha': value['alpha'] = 0
        elif name == 'seed': value['seed'] = 'invented'
        elif name == 'metric': value['metrics']['invented'] = 1
        elif name == 'metric_type': value['metrics'][next(iter(value['metrics']))] = True
        elif name == 'header': value['tables'][0]['cells'][0].reverse()
        elif name == 'row': value['tables'][0]['cells'][1][0]['value'] = 'invented'
        elif name == 'cell_type': value['tables'][0]['cells'][1][1]['type'] = 'System.String'
        elif name == 'status': value['status'] = 'APPROVED_GOLDEN'
        elif name == 'native': value['live_control_capture'] = True
        elif name == 'nonfinite': value['metrics'][next(iter(value['metrics']))] = float('nan')
        with self.assertRaises(ValueError): c.validate_candidate(value)
    setattr(CandidateComparisonTests, 'test_candidate_reject_' + name, test)


def add_valid_case(index):
    def test(self):
        case = native_plan()['cases'][index]
        result = c.validate_mock_capture(mock_capture(case), case)
        self.assertFalse(result['admissible_as_native'])
    setattr(CaptureContracts, 'test_mock_valid_' + str(index), test)


def cross_mutation(name):
    def test(self):
        plan = copy.deepcopy(self.source); case = plan['cases'][0]
        if name == 'delete_deferred': case['deferred_obligation_ids'].pop()
        elif name == 'duplicate_deferred': case['deferred_obligation_ids'].append(case['deferred_obligation_ids'][0])
        elif name == 'relabel_deferred': case['deferred_obligation_ids'][0] += '-invented'
        elif name == 'delete_blocked': case['blocked_control_ids'].pop()
        elif name == 'duplicate_blocked': case['blocked_control_ids'].append(case['blocked_control_ids'][0])
        elif name == 'release': plan['release_gates']['native_capture'] = True
        elif name == 'waiver': case['waiver_id'] = 'invented'
        elif name == 'owner': case['owner'] = ''
        elif name == 'edge_owner': plan['global_refedit_owners'].pop()
        elif name == 'hash': case['source_refs'][0]['sha256'] = '0'*64
        elif name == 'captured': case['status'] = 'CAPTURED'
        elif name == 'duplicate_case': plan['cases'].append(copy.deepcopy(case))
        with self.assertRaises(ValueError): c.validate_crosswalk(plan)
    setattr(CaptureContracts, 'test_crosswalk_reject_' + name, test)


def capture_mutation(name):
    def test(self):
        case = native_plan()['cases'][3]; record = mock_capture(case)
        if name == 'missing_x': del record['charts'][0]['series'][0]['x']
        elif name == 'missing_y': record['charts'][0]['series'][0]['y'] = []
        elif name == 'series_order': record['charts'][0]['series'].reverse()
        elif name == 'chart_order': record['charts'].reverse()
        elif name == 'option_before': record['option_before'].pop(next(iter(record['option_before'])))
        elif name == 'option_after': record['option_after'].pop(next(iter(record['option_after'])))
        elif name == 'input_range': record['input_range'] = 'Other!Z1'
        elif name == 'output_range': record['output_range'] = 'Other!Z1'
        elif name == 'sheet': record['cells'][0]['sheet'] = 'Other'
        elif name == 'culture': record['culture'] = 'de-DE'
        elif name == 'type': record['cells'][0]['type'] = 'String'
        elif name == 'integer_alias': record['cells'][0]['value'] = 1.0
        elif name == 'duplicate_control': record['control_ids'].append(record['control_ids'][0])
        elif name == 'artifact_reuse': record['artifacts'].append(copy.deepcopy(record['artifacts'][0]))
        elif name == 'artifact_case_alias': record['artifacts'].append(dict(path=record['artifacts'][0]['path'].upper(), sha256='2'*64))
        elif name == 'hash': record['source_hashes'] = {'fake': '0'*64}
        elif name == 'verified': record['verified'] = True
        elif name == 'status': record['status'] = 'VERIFIED'
        elif name == 'native_classification': record['classification'] = 'Windows-native-capture'
        elif name == 'release': record['release_decision'] = None
        elif name == 'host': record['host'] = {}
        elif name == 'units': record['charts'][0]['units'] = 'pixels'
        elif name == 'rect': record['charts'][0]['plot_area']['width'] = 0
        elif name == 'axes': del record['charts'][0]['axes']['x']['major_unit']
        elif name == 'number_format': del record['cells'][0]['number_format']
        elif name == 'display': record['cells'][0]['display'] = None
        elif name == 'nonfinite': record['charts'][0]['series'][0]['x'][0] = float('inf')
        elif name == 'dataset': record['dataset_sha256'] = '0'*64
        elif name == 'screenshot_hash': record['artifacts'][1]['sha256'] = 'not-a-hash'
        with self.assertRaises((ValueError, KeyError)): c.validate_mock_capture(record, case)
    setattr(CaptureContracts, 'test_capture_reject_' + name, test)


for i in range(4): add_valid_case(i)
for defect in ['delete_deferred', 'duplicate_deferred', 'relabel_deferred', 'delete_blocked',
               'duplicate_blocked', 'release', 'waiver', 'owner', 'edge_owner', 'hash', 'captured', 'duplicate_case']:
    cross_mutation(defect)
for defect in ['missing_x', 'missing_y', 'series_order', 'chart_order', 'option_before', 'option_after',
               'input_range', 'output_range', 'sheet', 'culture', 'type', 'integer_alias', 'duplicate_control',
               'artifact_reuse', 'artifact_case_alias', 'hash', 'verified', 'status', 'native_classification',
               'release', 'host', 'units', 'rect', 'axes', 'number_format', 'display', 'nonfinite', 'dataset',
               'screenshot_hash']:
    capture_mutation(defect)
for defect in ['dataset', 'fixture', 'source', 'alpha', 'seed', 'metric', 'metric_type', 'header',
               'row', 'cell_type', 'status', 'native', 'nonfinite']:
    candidate_mutation(defect)


if __name__ == '__main__':
    unittest.main(verbosity=2)
