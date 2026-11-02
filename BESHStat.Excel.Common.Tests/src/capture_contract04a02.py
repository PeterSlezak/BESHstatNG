"""Test-only, read-only admission. No process/host/oracle execution or promotion API.

Mocks deliberately cannot become native evidence through a flag change. Production
capture admission remains gated and requires an externally reviewed evidence chain.
"""
import hashlib
import csv
import json
import math
import ntpath
import os
from pathlib import Path, PureWindowsPath

ROOT = Path(__file__).resolve().parents[2]
DATA = ROOT / 'BESHStat.Excel.Common.Tests/TestData/04A02'
DOC = ROOT / 'docs/architecture/04A02'


def require(ok, message):
    if not ok:
        raise ValueError(message)


def strict_json(data):
    def pairs(items):
        result = {}
        for key, value in items:
            require(key not in result, 'duplicate JSON field/metric/case')
            result[key] = value
        return result
    value = json.loads(data, object_pairs_hook=pairs,
                       parse_constant=lambda v: require(False, 'nonfinite token'))
    def finite(v):
        if isinstance(v, float): require(math.isfinite(v), 'nonfinite number')
        if isinstance(v, dict):
            for k, x in v.items():
                k.encode('utf-8', errors='strict')
                finite(x)
        if isinstance(v, list):
            for x in v: finite(x)
        if isinstance(v, str): v.encode('utf-8', errors='strict')
    finite(value)
    return value


def sha(path):
    return hashlib.sha256(Path(path).read_bytes()).hexdigest()


def canonical(value):
    encoded = json.dumps(value, sort_keys=True, ensure_ascii=True, allow_nan=False,
                         separators=(',', ':')).encode('utf-8')
    strict_json(encoded)
    return encoded


def exclusive_record(path, record):
    """Scratch/new evidence only; a partial failed file is retained, never retried."""
    raw = canonical(record) + b'\n'
    with Path(path).open('xb') as stream:
        stream.write(raw)
        stream.flush()
        os.fsync(stream.fileno())


def unique(items, label):
    require(len(items) == len(set(items)), 'duplicate ' + label)


def validate_crosswalk(plan, cross=None):
    cross = cross or strict_json((DOC / 'native-restriction-crosswalk-v1.json').read_bytes())
    expected = {'04A02-ANOVA-NATIVE-01': 'UibyID', '04A02-NORMALITY-NATIVE-01': 'UibyID',
                '04A02-2X2-NATIVE-01': 'Ui82x2', '04A02-PCA-NATIVE-01': 'Ui11PCA'}
    ids = [c['case_id'] for c in plan['cases']]
    unique(ids, 'case'); require(set(ids) == set(expected), 'case scope')
    require(not any(plan['release_gates'].values()), 'unapproved release')
    require(plan['global_refedit_edge'] == cross['global_refedit_edge'] and
            plan['global_refedit_risk'] == 'UNKNOWN_HIGH', 'global RefEdit edge')
    require(plan['global_refedit_owners'] == cross['global_refedit_owners'], 'edge owners')
    union = set()
    for case in plan['cases']:
        form = cross['forms'][expected[case['case_id']]]
        require(case['owner'] and case['status'] == 'NOT_RUN' and case['native_execution'] == 'BLOCKED', 'owner/status')
        require(not case.get('waiver_id'), 'invented waiver')
        for key in ('blocked_control_ids', 'deferred_obligation_ids'):
            unique(case[key], key)
            require(case[key] == form[key], 'full equality of ' + key)
        union.update(case['blocked_control_ids'])
        for pin in case['source_refs']:
            require(sha(ROOT / pin['path']) == pin['sha256'], 'source provenance')
            require(pin['anchor'] in (ROOT / pin['path']).read_text(encoding='utf-8-sig'), 'source anchor')
    require(len(union) == 218, '218 distinct blocked controls')
    return union


def obligation_evidence(plan):
    """Missing/local-only raw objects are explicitly UNVERIFIABLE, never recreated."""
    validate_crosswalk(plan)
    path = DOC.parent / 'WindowsMigrationLedger/04A01-approved-deferred-obligations.json'
    ids = sorted({i for c in plan['cases'] for i in c['deferred_obligation_ids']})
    if not path.is_file():
        return [dict(obligation_id=i, status='UNVERIFIABLE', reason='ledger absent') for i in ids]
    records = strict_json(path.read_bytes())
    lookup = {v['obligation_id']: v for v in records}
    require(len(lookup) == len(records), 'duplicate ledger obligations')
    results = []
    for oid in ids:
        require(oid.startswith('control:'), 'obligation namespace')
        v = lookup.get(oid)
        if v is None:
            results.append(dict(obligation_id=oid, status='UNVERIFIABLE', reason='ledger object absent'))
            continue
        require(v['original_id'] == oid.removeprefix('control:') and v['owner_roles'], 'ledger identity/owner')
        original = ROOT / v['source_record_path']
        # C30 source_record_sha256 is digest(CSV ROW DICT), not a file hash.
        # Match the original read-only digest convention, never rewrite CSV bytes.
        row_hash = None
        if original.is_file():
            with original.open(encoding='utf-8-sig', newline='') as stream:
                rows = [row for row in csv.DictReader(stream) if row.get('control_id') == v['original_id']]
            require(len(rows) == 1, 'source CSV row identity/uniqueness')
            row_hash = hashlib.sha256(json.dumps(rows[0], sort_keys=True, ensure_ascii=False).encode('utf-8')).hexdigest()
        raw = ('UNVERIFIABLE_LOCAL_ONLY_OBJECT_ABSENT' if row_hash is None else
               'CSV_ROW_HASH_MATCH' if row_hash == v['source_record_sha256'] else
               'CSV_ROW_PIN_MISMATCH_NOT_REBASELINED')
        results.append(dict(obligation_id=oid, status='LEDGER_RECORD_IDENTIFIED_NOT_RELEASED',
                            ledger_path=path.relative_to(ROOT).as_posix(), ledger_sha256=sha(path),
                            owners=v['owner_roles'], earliest_batches=v['earliest_approved_batches'],
                            risk=v['risk_tiers'], source_record_path=v['source_record_path'],
                            expected_record_sha256=v['source_record_sha256'], source_record_status=raw,
                            actual_record_sha256=row_hash,
                            source_file_sha256=sha(original) if original.is_file() else None,
                            capture_authorized=v['capture_authorized'],
                            required_evidence=v['safe_capture_preconditions']))
    return results


def validate_ready_plan(plan):
    source = strict_json((DATA / 'windows-option-chart-source-spec-v1.json').read_bytes())
    validate_crosswalk(source)
    require(plan['release_gates'] == {'S': 'PENDING', 'R': False, 'P': False, 'N': False}, 'ready-plan gates')
    require(plan['global_refedit_edge'] == source['global_refedit_edge'] and
            plan['global_refedit_owners'] == source['global_refedit_owners'], 'ready-plan global edge/owners')
    ids = [x['case_id'] for x in plan['cases']]
    unique(ids, 'ready-plan case')
    require(set(ids) == {x['case_id'] for x in source['cases']}, 'ready-plan scope')
    for case in plan['cases']:
        original = next(x for x in source['cases'] if x['case_id'] == case['case_id'])
        for key in ('blocked_control_ids', 'deferred_obligation_ids'):
            unique(case[key], 'ready '+key)
            require(case[key] == original[key], 'ready equality '+key)
        require(case['owner'] == original['owner'] and case['owner'], 'ready owner')
        require(case['status'] == 'NOT_RUN' and case['restriction_status'] == 'BLOCKED' and
                case['verification_status'] == 'NOT_VERIFIED' and case['actually_exercised_controls'] is None,
                'ready fabricated runtime state')
        require(not case.get('waiver_id'), 'ready invented waiver')
        require(case['source_hashes'] == {p['path']: p['sha256'] for p in original['source_refs']}, 'ready source provenance')
    expected = {i for case in source['cases'] for i in case['deferred_obligation_ids']}
    trace_ids = [x['obligation_id'] for x in plan['obligation_trace']]
    unique(trace_ids, 'ready ledger trace')
    require(set(trace_ids) == expected, 'ready complete ledger trace')
    for row in plan['obligation_trace']:
        require(row['status'] == 'LEDGER_RECORD_IDENTIFIED_NOT_RELEASED' and row['owners'] and
                not row['capture_authorized'] and row['source_record_status'] == 'CSV_ROW_HASH_MATCH', 'ready ledger verification')
    return True


def validate_candidate(value):
    require(value['schema'] == '04A02-candidate-observation-v1', 'candidate schema')
    require(value['status'] == 'UNVERIFIED_AGAINST_INDEPENDENT_ORACLE' and
            value['live_control_capture'] is False, 'candidate classification')
    cid = value['case_id']; require(cid in ('C30-SYN-ANOVA', 'C30-SYN-KW'), 'case')
    fixture = strict_json((DATA / (cid + '.json')).read_bytes())
    require(value['dataset_sha256'] == fixture['dataset_sha256'] and
            value['alpha'] == .05 and value['seed'] == 'N/A', 'candidate identity/options')
    require(value['source_commit'] == fixture['source_commit'] and
            value['fixture_sha256'] == hashlib.sha256(canonical(fixture)).hexdigest(), 'candidate fixture/source hash')
    require(value['group_names'] == ['A', 'B', 'C'] and value['group_counts'] == [3, 3, 3], 'group identity')
    expected = {'FStatistic', 'PValue'} if cid.endswith('ANOVA') else {
        'HStatistic', 'TieCorrectedHStatistic', 'PValue', 'TieCorrectedPValue'}
    require(set(value['metrics']) == expected, 'metric identity')
    for v in value['metrics'].values():
        require(type(v) in (float, int) and math.isfinite(v), 'finite metric')
    require(len(value['tables']) == 1, 'primary table count')
    spec = strict_json((DATA / 'semantic-table-source-spec-v1.json').read_bytes())
    expected_table = next(c for c in spec['cases'] if c['input_case_id'] == cid)['tables'][0]
    table = value['tables'][0]
    for key in ('rows', 'columns', 'header_top_rows', 'header_left_columns', 'footer_rows', 'title_rows', 'pvalue_columns'):
        require(table[key] == expected_table[key], 'candidate table '+key)
    for row in table['cells']:
        require(len(row) == table['columns'], 'candidate column count')
    require(len(table['cells']) == table['rows'], 'candidate row count')
    require([c['value'] for c in table['cells'][0]] == expected_table['header'], 'candidate header order')
    for i, row in enumerate(table['cells'][1:]):
        require(row[0]['value'] == expected_table['body_row_labels'][i], 'candidate body order')
        for j, cell in enumerate(row[table['header_left_columns']:]):
            require(cell['type'] == expected_table['cell_types'][i][j], 'candidate CLR cell type')
            kind = cell['type']; scalar = cell['value']
            if kind == 'null': require(scalar is None, 'null versus text')
            elif kind == 'System.String': require(isinstance(scalar, str), 'string payload')
            elif kind == 'System.Int32': require(type(scalar) is int and -(2**31) <= scalar < 2**31, 'Int32 payload')
            elif kind == 'System.Double': require(type(scalar) in (int, float) and math.isfinite(scalar), 'Double payload')
    require(table['pvalue_cells'] == [dict(BodyRow=x[0], BodyColumn=x[1]) for x in expected_table['pvalue_cells']], 'p-cell coordinates')
    if cid.endswith('KW'):
        require(table['cells'][4][2]['value'] == 'ties un-corrected' and
                table['cells'][6][2]['value'] == 'ties corrected', 'KW annotation distinction')
    canonical(value)  # recursively reject nonfinite tables/metadata
    return value


def compare_candidates(left, right):
    validate_candidate(left); validate_candidate(right)
    require(left['case_id'] == right['case_id'] and left['dataset_sha256'] == right['dataset_sha256'], 'cross-case comparison')
    differences = []
    def visit(a, b, path):
        if type(a) is not type(b):
            differences.append(dict(path=path, kind='TYPE', candidate_left=a, candidate_right=b))
        elif isinstance(a, dict):
            for k in sorted(a.keys() | b.keys()):
                if k not in a or k not in b: differences.append(dict(path=path+'/'+k, kind='FIELD_MEMBERSHIP'))
                else: visit(a[k], b[k], path+'/'+k)
        elif isinstance(a, list):
            if len(a) != len(b): differences.append(dict(path=path, kind='LENGTH'))
            for i, (x, y) in enumerate(zip(a, b)): visit(x, y, path+'/'+str(i))
        elif a != b:
            entry = dict(path=path, kind='VALUE', candidate_left=a, candidate_right=b)
            if type(a) in (int, float): entry['absolute_candidate_difference'] = abs(a-b)
            differences.append(entry)
    visit(left, right, '')
    return dict(schema='04A02-candidate-continuity-comparison-v1', classification='comparison',
                inputs_classification='candidate', reference=None, approved_golden=None,
                status='CANDIDATE_IDENTICAL_NOT_REFERENCE_VALIDATED' if not differences else 'CANDIDATE_DIFFERENCE_DETECTED',
                differences=differences, tolerance_applied=False)


def safe_artifact_path(name):
    require(isinstance(name, str) and '\\' not in name and ':' not in name, 'artifact path')
    p = PureWindowsPath(name)
    require(not p.is_absolute() and '..' not in p.parts and p.parts, 'artifact path escape')
    for part in p.parts:
        require(part and part == part.rstrip(' .') and not ntpath.isreserved(part), 'path alias')
    return '/'.join(p.parts).casefold()


def validate_mock_capture(record, case):
    """Schema tests only. This API categorically refuses native/CAPTURED/VERIFIED records."""
    require(record['classification'] == 'MOCK_SCHEMA_TEST_NOT_NATIVE_CAPTURE', 'mock cannot be promoted')
    require(record['status'] == 'NOT_RUN' and record['verified'] is False, 'fabricated verification')
    require(record['case_id'] == case['case_id'] and record['source_hashes'] == case['source_hashes'], 'case/source pin')
    require(record['release_decision'] == 'MOCK_NOT_AUTHORITY', 'missing release decision')
    for key in ('dataset_sha256', 'workbook', 'input_range', 'output_range', 'culture', 'host'):
        require(record[key] == case[key], 'capture '+key)
    require(set(record['option_before']) == set(case['required_options']) == set(record['option_after']), 'option state coverage')
    require(record['control_ids'] == case['exercised_controls'], 'control identity/order')
    unique(record['control_ids'], 'control IDs')
    paths = [safe_artifact_path(a['path']) for a in record['artifacts']]
    unique(paths, 'artifact paths/aliases')
    for a in record['artifacts']:
        require(isinstance(a['sha256'], str) and len(a['sha256']) == 64 and all(c in '0123456789abcdef' for c in a['sha256']), 'artifact hash')
    require(record['artifacts'], 'artifact evidence')
    require(record['cells'], 'rendered cells')
    cell_ids = []
    for cell in record['cells']:
        require(set(cell) == {'sheet', 'address', 'value', 'type', 'display', 'number_format', 'error', 'warning', 'merged_range'}, 'rendered cell fields')
        require(cell['sheet'] == case['workbook']['output_sheet'], 'wrong sheet')
        require(cell['type'] in ('Double', 'Int32', 'String', 'Null', 'ExcelError', 'Boolean'), 'cell type')
        if cell['type'] in ('Double', 'Int32'):
            require(type(cell['value']) in (int, float) and math.isfinite(cell['value']), 'numeric cell type/value')
            if cell['type'] == 'Int32':
                require(type(cell['value']) is int and -(2**31) <= cell['value'] < 2**31, 'integer precision/range')
        elif cell['type'] == 'String': require(isinstance(cell['value'], str), 'text type/value')
        elif cell['type'] == 'Null': require(cell['value'] is None, 'null type/value')
        require(isinstance(cell['display'], str) and isinstance(cell['number_format'], str), 'rendered text/format')
        cell_ids.append(cell['sheet']+'!'+cell['address'])
    unique(cell_ids, 'cell addresses')
    require([c['chart_id'] for c in record['charts']] == case['expected_chart_ids'], 'chart membership/order')
    for chart in record['charts']:
        require(chart['units'] == 'points', 'geometry units')
        for rect in ('chart_area', 'plot_area'):
            require(set(chart[rect]) == {'left', 'top', 'width', 'height'}, 'chart rectangle')
            require(all(type(v) in (int, float) and math.isfinite(v) for v in chart[rect].values()) and
                    chart[rect]['width'] > 0 and chart[rect]['height'] > 0, 'chart geometry')
        require([s['name'] for s in chart['series']] == case['series_order'][chart['chart_id']], 'series order')
        for series in chart['series']:
            require(set(series) == {'name', 'x', 'y', 'type', 'labels', 'markers', 'error_bars'}, 'series fields')
            require(series['x'] and len(series['x']) == len(series['y']), 'missing chart X/Y')
        require(set(chart['axes']) == {'x', 'y'}, 'axes')
        for axis in chart['axes'].values():
            require(set(axis) == {'min', 'max', 'major_unit', 'minor_unit', 'scale', 'title', 'tick_labels'}, 'axis fields')
        require('legend' in chart and 'title' in chart, 'chart labels/configuration')
    canonical(record)
    return dict(status='MOCK_SCHEMA_VALID_NOT_NATIVE_EVIDENCE', admissible_as_native=False)


def refuse_unreleased_native(record, decision=None):
    """There is no approved capture importer in this source-only closeout.

    Positive owner releases require a separate reviewed receipt/artifact verifier;
    a dict claiming approval is insufficient authority for this mock schema tool.
    """
    raise ValueError('N CLOSED: no authenticated owner release + regression + artifact chain; no native admission')


def comparison_cli():
    import argparse
    parser = argparse.ArgumentParser(description='Candidate continuity only; no reference or promotion operation.')
    parser.add_argument('--left', type=Path, required=True)
    parser.add_argument('--right', type=Path, required=True)
    parser.add_argument('--output', type=Path, required=True)
    args = parser.parse_args()
    output = args.output.resolve()
    roots = [p for p in output.parents if p.parent == ROOT / 'TestResults']
    require(len(roots) == 1 and roots[0].name.startswith('04A-02-initial-fixtures-'), 'new evidence root required')
    root = roots[0]
    require((root / 'start.json').is_file() and (root / 'historical-metadata.json').is_file(), 'preflight required')
    require(not (root / 'artifact-hashes.json').exists(), 'sealed evidence root cannot receive new comparisons')
    require(output.parent.is_dir(), 'create a fresh approved output directory before comparison')
    def read_candidate(path):
        value = strict_json(path.read_bytes())
        return value['service'] if value.get('schema') == '04A02-G3-execution-candidate-v1' else value
    result = compare_candidates(read_candidate(args.left), read_candidate(args.right))
    exclusive_record(output, result)
    print(result['status'])
    return 0 if not result['differences'] else 2


if __name__ == '__main__':
    raise SystemExit(comparison_cli())
