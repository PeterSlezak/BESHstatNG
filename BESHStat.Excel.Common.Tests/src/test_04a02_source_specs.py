"""Source-only C04 admission checks; never imports a host or launches a process."""
import hashlib
import json
from pathlib import Path
import unittest

ROOT = Path(__file__).resolve().parents[2]
DATA = ROOT / 'BESHStat.Excel.Common.Tests/TestData/04A02'


def load(name):
    return json.loads((DATA / name).read_text(encoding='utf-8'))


def pinned(ref):
    path = ROOT / ref['path']
    if hashlib.sha256(path.read_bytes()).hexdigest() != ref['sha256']:
        raise ValueError('source hash drift')
    if 'anchor' in ref and ref['anchor'] not in path.read_text(encoding='utf-8-sig'):
        raise ValueError('missing anchored symbol')


def admit_semantic(spec):
    if spec['status'] != 'CANDIDATE_SPECIFICATION_ONLY' or any(spec['release_gates'].values()):
        raise ValueError('promotion forbidden')
    for ref in spec['source_refs'] + spec['context_snapshots']:
        pinned(ref)
    for case in spec['cases']:
        fixture = json.loads((ROOT / case['fixture_path']).read_text())
        if hashlib.sha256((ROOT / case['fixture_path']).read_bytes()).hexdigest() != case['fixture_sha256']:
            raise ValueError('fixture hash drift')
        for key in ('dataset', 'dataset_sha256', 'options', 'alpha', 'seed', 'row_id_transport'):
            if case[key] != fixture[key]:
                raise ValueError('frozen input/options drift')
        if case['dataset']['group_labels'] != ['A', 'B', 'C']:
            raise ValueError('group order')
        if case['table_order'] != ['primary'] or len(case['tables']) != 1:
            raise ValueError('table order')
        table = case['tables'][0]
        if table['expected_numerical_reference'] is not None:
            raise ValueError('self-derived reference forbidden')
        anova = case['input_case_id'] == 'C30-SYN-ANOVA'
        expected_header = (['Source of Variation', 'SS', 'df', 'MS', 'F', 'P-value']
                           if anova else ['Kruskal-Wallis Test', '', ''])
        if table['header'] != expected_header:
            raise ValueError('header order/text')
        expected_rows = (['Between Groups', 'Within Groups', 'Total'] if anova else
                         ['n', 'Number of groups', 'Test statistics H', 'Two sided P-value',
                          'Test statistics Hcor', 'Two sided p-value'])
        if table['body_row_labels'] != expected_rows:
            raise ValueError('body order/text')
        if anova:
            if table['cell_types'][0] != ['System.Double', 'System.Int32', 'System.Double', 'System.Double', 'System.Double']:
                raise ValueError('CLR types')
            if table['cell_types'][1][-2:] != ['null', 'null']:
                raise ValueError('null is not zero/text')
        elif table['pvalue_cells'] != [[4, 2], [6, 2]] or set(case['metric_types']) != {
                'HStatistic', 'TieCorrectedHStatistic', 'PValue', 'TieCorrectedPValue'}:
            raise ValueError('raw/corrected p metadata')
        for ref in case['source_refs']:
            pinned(ref)


def admit_windows(spec):
    cross = json.loads((ROOT / 'docs/architecture/04A02/native-restriction-crosswalk-v1.json').read_text())
    if any(spec['release_gates'].values()) or spec['global_refedit_edge'] != cross['global_refedit_edge']:
        raise ValueError('native release/global risk drift')
    if spec['global_refedit_risk'] != 'UNKNOWN_HIGH':
        raise ValueError('risk suppressed')
    for ref in spec['source_registers'] + [spec['global_source']]:
        pinned(ref)
    if {c['case_id'] for c in spec['cases']} != {
            '04A02-ANOVA-NATIVE-01', '04A02-NORMALITY-NATIVE-01',
            '04A02-2X2-NATIVE-01', '04A02-PCA-NATIVE-01'}:
        raise ValueError('full scope missing')
    for case in spec['cases']:
        form = {'ANOVA': 'UibyID', 'NORMALITY': 'UibyID', '2X2': 'Ui82x2', 'PCA': 'Ui11PCA'}[case['case_id'].split('-')[1]]
        if case['status'] != 'NOT_RUN' or case['native_execution'] != 'BLOCKED':
            raise ValueError('invented capture')
        if case['blocked_control_ids'] != cross['forms'][form]['blocked_control_ids']:
            raise ValueError('exact blockers omitted')
        if [c['control_id'] for c in case['controls']] != case['blocked_control_ids']:
            raise ValueError('control coverage')
        for ref in case['source_refs']:
            pinned(ref)
        for control in case['controls']:
            for key, source in [('designer_assignments', 'designer_source'), ('source_uses', 'source')]:
                lines = (ROOT / control[source]).read_text(encoding='utf-8-sig').splitlines()
                for entry in control[key]:
                    if lines[entry['line'] - 1].strip() != entry['text']:
                        raise ValueError('source excerpt drift')
        for chart in case['chart_plan']:
            pinned(chart)
            if chart['captured_values'] is not None:
                raise ValueError('invented chart values')
            lines = (ROOT / chart['path']).read_text(encoding='utf-8-sig').splitlines()
            for entry in chart['observable_source_assignments']:
                if lines[entry['line'] - 1].strip() != entry['text']:
                    raise ValueError('chart source mapping drift')
        for option in case['option_controls']:
            for entries, path in [('initialization', 'designer_source'), ('mapping', 'source'),
                                  ('type_source', 'designer_source')]:
                lines = (ROOT / option[path]).read_text(encoding='utf-8-sig').splitlines()
                for entry in option[entries]:
                    if lines[entry['line'] - 1].strip() != entry['text']:
                        raise ValueError('option source mapping drift')


class SourceSpecifications(unittest.TestCase):
    def setUp(self):
        self.semantic = load('semantic-table-source-spec-v1.json')
        self.windows = load('windows-option-chart-source-spec-v1.json')

    def test_semantic_source_pins_and_frozen_inputs(self):
        admit_semantic(self.semantic)

    def test_all_native_blockers_and_anchored_mappings(self):
        admit_windows(self.windows)

    def test_wrong_table_order_rejected(self):
        self.semantic['cases'][0]['table_order'] = ['optional', 'primary']
        with self.assertRaises(ValueError): admit_semantic(self.semantic)

    def test_wrong_cell_order_rejected(self):
        self.semantic['cases'][0]['tables'][0]['header'].reverse()
        with self.assertRaises(ValueError): admit_semantic(self.semantic)

    def test_wrong_clr_type_rejected(self):
        self.semantic['cases'][0]['tables'][0]['cell_types'][0][1] = 'System.String'
        with self.assertRaises(ValueError): admit_semantic(self.semantic)

    def test_unexpected_dunn_default_rejected(self):
        self.semantic['cases'][1]['options']['IncludeDunnComparisons'] = True
        with self.assertRaises(ValueError): admit_semantic(self.semantic)

    def test_null_not_empty_text(self):
        self.semantic['cases'][0]['tables'][0]['cell_types'][1][-1] = 'System.String'
        with self.assertRaises(ValueError): admit_semantic(self.semantic)

    def test_localized_authored_label_not_admitted(self):
        self.semantic['cases'][1]['tables'][0]['body_row_labels'][3] = 'Dvojstranna P-hodnota'
        with self.assertRaises(ValueError): admit_semantic(self.semantic)

    def test_swapped_corrected_p_cells_rejected(self):
        self.semantic['cases'][1]['tables'][0]['pvalue_cells'].reverse()
        with self.assertRaises(ValueError): admit_semantic(self.semantic)

    def test_source_hash_drift_rejected(self):
        self.semantic['source_refs'][0]['sha256'] = '0' * 64
        with self.assertRaises(ValueError): admit_semantic(self.semantic)

    def test_no_numeric_promotion(self):
        self.semantic['cases'][0]['tables'][0]['expected_numerical_reference'] = 1
        with self.assertRaises(ValueError): admit_semantic(self.semantic)

    def test_missing_exact_blocker_rejected(self):
        self.windows['cases'][2]['blocked_control_ids'].pop()
        with self.assertRaises(ValueError): admit_windows(self.windows)

    def test_global_refedit_not_waived(self):
        self.windows['global_refedit_risk'] = 'LOW'
        with self.assertRaises(ValueError): admit_windows(self.windows)

    def test_predicted_chart_not_capture(self):
        self.windows['cases'][3]['chart_plan'][0]['captured_values'] = [1, 2]
        with self.assertRaises(ValueError): admit_windows(self.windows)

    def test_fabricated_default_state_rejected(self):
        self.windows['cases'][0]['option_controls'][0]['initialization'][0]['text'] = 'Me.optByID.Checked = False'
        with self.assertRaises(ValueError): admit_windows(self.windows)

    def test_fabricated_chart_geometry_rejected(self):
        self.windows['cases'][3]['chart_plan'][0]['observable_source_assignments'][0]['text'] = '.Width = 999'
        with self.assertRaises(ValueError): admit_windows(self.windows)

    def test_stable_json_roundtrip(self):
        for spec in [self.semantic, self.windows]:
            encoded = json.dumps(spec, sort_keys=True, separators=(',', ':'), allow_nan=False)
            self.assertEqual(encoded, json.dumps(json.loads(encoded), sort_keys=True, separators=(',', ':'), allow_nan=False))


if __name__ == '__main__':
    unittest.main(verbosity=2)
