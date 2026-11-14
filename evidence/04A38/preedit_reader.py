"""Independent candidate reader. Standard library only; no producer imports.

This verifies compatibility-reference controls, not implementation readiness.
It must never infer an ownership disposition or authorize product edits.
"""
import argparse
from collections import Counter, defaultdict
import csv
import hashlib
import itertools
import json
from pathlib import Path, PurePosixPath
import re

BASE = 'ba1636f5356ac761509c2cfa16fc66b92430935d'
CALLABLE = {'FunctionBlock', 'SubBlock', 'GetAccessorBlock', 'SetAccessorBlock'}
CULTURES = {'', 'sk-SK'}
ENTRY_ANCHORS = {
    'preedit/entry-source-snapshot.json': '2c6ba6ce5769f966b094b13305038cadd192271c71bf66332c33ee5db7f62de8',
    'entry-members.json': 'e88356ebc7dbf4831d12e726e1b3aa01fbce9a68f260e1ef3237c3d608d2dc61',
    'preedit/compiled-Debug.json': '9267a807aa3a6907f890f9c743ca6b27ab3dfab59021701d0ce1cdc604f769fa',
    'preedit/compiled-Release.json': '7806b9bcfebbbd12cf092f2c814dbf51ff28cb2fbe9ddc283ec515207aeb72f0',
}
# Independently checked original-source v10 executions, before any product edit.
# They bind all three equal runs; changing every copy cannot refresh the oracle.
OUTPUT_ANCHORS = {
    'Debug': 'a33f7772c14a40cadd139258efde1960029503ae9294024efd7f66b23e3daf2e',
    'Release': '4d78707c8f2d0f2c5a431832be7c86a222db034958f16c540bb44b831b7ecccf',
}


class InvalidReference(ValueError):
    pass


def require(condition, message):
    if not condition:
        raise InvalidReference(message)


def digest(data):
    return hashlib.sha256(data).hexdigest()


def safe_path(root, path):
    p = PurePosixPath(path)
    require(not p.is_absolute() and '\\' not in path and ':' not in path and all(
        part not in {'', '.', '..'} and not part.endswith((' ', '.')) and not re.fullmatch(
            r'(?i)(CON|PRN|AUX|NUL|COM[1-9]|LPT[1-9])(?:\..*)?', part) for part in path.split('/')),
        'Unsafe reference path')
    resolved = (root / path).resolve()
    require(resolved.is_relative_to(root.resolve()), 'Reference escapes packet root')
    return resolved


def check_pin(root, row):
    data = safe_path(root, row['path']).read_bytes()
    require(type(row['bytes']) is int and row['bytes'] == len(data), 'Reference byte-size mismatch: ' + row['path'])
    require(re.fullmatch('[0-9a-f]{64}', row['sha256']) and digest(data) == row['sha256'], 'Reference digest mismatch: ' + row['path'])
    return data


def read_rows(path):
    csv.field_size_limit(1 << 28)
    with path.open(encoding='utf-8', newline='') as stream:
        rows = list(csv.DictReader(stream, delimiter='\t'))
    require(all(set(r) == {'case_id', 'input_sha256', 'outcome', 'phase', 'result', 'byref_arguments', 'error', 'caught_error_events'} and None not in r.values() for r in rows), 'Malformed outcome table')
    return rows


def grid(cases, cohort, keys, expected):
    rows = [r for r in cases if r['cohort'] == cohort]
    actual = {tuple(r[k] for k in keys) for r in rows}
    require(actual == set(itertools.product(*expected)), 'Incomplete independent cross-product: ' + cohort)


def inspect(root, version='capture-v10', expected_count=24732):
    here = root / 'evidence/04A38'
    for relative, expected in ENTRY_ANCHORS.items():
        require(digest((here / relative).read_bytes()) == expected, 'Protected entry authority changed: ' + relative)
    folder = here / 'preedit/attempts' / version
    census = json.loads((here / 'entry-members.json').read_bytes())
    source_receipt = json.loads((here / 'preedit/entry-source-snapshot.json').read_bytes())
    require(len(census['sources']) == len(source_receipt['files']) == 41, '41-source population lost')
    exports = census['exports']
    require(len(exports) == 268 and len({r['effective_excel_name'].casefold() for r in exports}) == 268, '268 unique exports lost')
    require(len({r['path'] for r in exports}) == 23, 'Export-bearing/support population changed')
    require(len(census['members']) == 704, 'Current behavioral population changed')
    require(len({r['symbol_id'] for r in census['members']}) == 704, 'Duplicate current member identity')
    for row in source_receipt['files']:
        original = check_pin(root, row['snapshot'])
        require(len(original) == row['source']['bytes'] and digest(original) == row['source']['sha256'], 'Snapshot differs from entry source')
    authority = json.loads((here / 'entry-authority.json').read_bytes())
    require(BASE in json.dumps(authority), 'Wrong merged-main entry authority')
    specification = json.loads((folder / 'specification.json').read_bytes())
    raw_cases = check_pin(root, specification['cases'])
    recorder = (folder / 'Capture38.cs.txt').read_bytes()
    require(digest(recorder) == specification['recorder']['sha256'] and len(recorder) == specification['recorder']['bytes'], 'Versioned recorder source lost')
    cases = [json.loads(line) for line in raw_cases.decode('utf-8').splitlines()]
    require(len(cases) == specification['rows'] == expected_count, 'Candidate cardinality changed')
    require(len({r['case_id'].casefold() for r in cases}) == expected_count, 'Repeated/casefolded case identity')
    for case in cases:
        payload = dict(case)
        identity = payload.pop('case_id')
        expected = payload['target'] + '.' + digest(json.dumps(payload, sort_keys=True, separators=(',', ':')).encode())[:24]
        require(identity == expected, 'Case identity does not bind its complete descriptor')
        require(case['culture'] in CULTURES and isinstance(case['arguments'], dict) and case['expected_stage'], 'Incomplete culture/argument/phase descriptor')
    ordinary = [r for r in cases if r['cohort'] == 'all_exports_ordinary']
    require(Counter((r['target'], r['culture']) for r in ordinary) == Counter(itertools.product([e['effective_excel_name'] for e in exports], CULTURES)), 'An export lacks its ordinary managed-body fixture')
    grid(cases, 'independent_import_shape_header_gap', ['culture','target','rows','columns','header','gap'],
         [CULTURES, ['ExtractCompleteNumericMatrixCompleteCases','ExtractNumericColumnIgnoringNonNumeric','TryReadGroupedNumericColumns','TryReadIndependentNumericColumns','TryReadNumericColumn','TryReadNumericMatrix'], [0,1,2,4], [0,1,2,3], ['none','text','numeric_text','blank'], ['clean','blank','error','NaN']])
    grid(cases, 'pattern_structure_independent', ['culture','rows','columns','pattern_profile','structure_profile'],
         [CULTURES, [0,1,2,4], [0,1,2,3], ['ordinary','zero','signed_zero','nonfinite'], ['ordinary','zero','signed_zero','nonfinite']])
    grid(cases, 'independent_linear_predictor_dimensions', ['culture','rows','columns','row_index','include_intercept','beta_profile','offset_profile'],
         [CULTURES, [0,1,3], [0,1,2], [-1,0,2], ['true','false'], ['null','doubles:','doubles:0.25,-0.5,1.5'], ['null','doubles:','doubles:0.1,0.2,0.3']])
    grid(cases, 'isolated_actual_cache_helper', ['culture','cache_comparer','input_state'],
         [CULTURES, ['ordinal','ordinal_ignore_case'], ['null','missing','empty','error','blank','whitespace','text:LM:known','text:lm:KNOWN','text:LM:absent','number:1']])
    members = {r['symbol_id']: r for r in census['members']}
    callable_ids = {k for k,r in members.items() if r['kind'] in CALLABLE}
    require(len(callable_ids) == 643, 'Callable member population changed')
    controls = {}
    detailed = []
    configurations = {}
    for config in ['Debug','Release']:
        paths = [folder / ('capture-' + config + '-' + suffix + '.tsv') for suffix in ['original-v1','repeat-v2','trace-v1']]
        data = [p.read_bytes() for p in paths]
        require(digest(data[0]) == OUTPUT_ANCHORS[config], config + ' immutable original output changed')
        require(data[0] == data[1], config + ' original run is not repeatable')
        require(data[0] == data[2], config + ' coverage instrumentation changes outputs')
        rows = read_rows(paths[0])
        require([r['case_id'] for r in rows] == [r['case_id'] for r in cases], config + ' cases missing/duplicated/reordered')
        traces = [json.loads(s) for s in Path(str(paths[2]) + '.reachability.jsonl').read_text(encoding='utf-8').splitlines()]
        require([r['case_id'] for r in traces] == [r['case_id'] for r in cases], 'Trace case correlation lost')
        reached = set().union(*(set(r['reached']) for r in traces))
        require(reached == callable_ids, config + ' callable-source reachability incomplete')
        require(not any('OPAQUE:' in r['result'] + r['byref_arguments'] for r in rows), 'An outcome hides a typed payload')
        require(not any(r['phase'] in {'BUILD_INPUT','SETUP_FIT_SEED'} for r in rows), 'A fixture failed before target dispatch')
        for suffix in ['original-v1','repeat-v2','trace-v1']:
            execution = json.loads((folder / ('execution-' + config + '-' + suffix + '.json')).read_bytes())
            require(execution['rows'] == expected_count and execution['native_attempts'] == 0, 'Execution receipt has wrong rows/native attempts')
            require(execution['outcomes'] == dict(Counter(r['outcome'] for r in rows)), 'Execution outcome accounting differs')
            require(execution['scientific'] == 'NOT_GOLDEN' and 'NOT_RUN' in execution['native'], 'Native/scientific gate promotion')
        outcomes = Counter(r['outcome'] for r in rows)
        by_cohort = defaultdict(Counter)
        for case, row, trace in zip(cases, rows, traces):
            if case['target_kind'] == 'export':
                target_ids = {e['source_symbol_id'] for e in exports if e['effective_excel_name'] == case['target']}
            else:
                target_ids = {k for k,m in members.items() if m['containing_type'] == case['type'] and m['name'] == case['target'] and m['kind'] in CALLABLE}
            target_reached = bool(target_ids.intersection(trace['reached']))
            dispatch = 'ORIGINAL_SOURCE_BODY_REACHED' if target_reached else 'MANAGED_REFLECTION_BINDER_REJECTED_NOT_SHIPPING_EXCEL_BEHAVIOR'
            if case['cohort'] == 'all_exports_ordinary':
                require(target_reached, 'Ordinary public export never reached its source body')
            by_cohort[case['cohort']][dispatch + '/' + row['outcome']] += 1
            detailed.append(dict(configuration=config,case_id=case['case_id'],cohort=case['cohort'],culture=case['culture'],target=case['target'],dispatch=dispatch,outcome=row['outcome'],phase=row['phase']))
        controls[config] = dict(rows=len(rows),sha256=digest(data[0]),bytes=len(data[0]),
            repeat_exact=True,trace_exact=True,callable_entries_reached=len(reached),native_attempts=0,
            outcomes=dict(outcomes),cohorts={k:dict(v) for k,v in by_cohort.items()})
        configurations[config] = rows
    differing = [a['case_id'] for a,b in zip(configurations['Debug'],configurations['Release']) if a != b]
    return dict(status='COMPATIBILITY_CANDIDATE_CONTROLS_PASS; NOT_AN_IMPLEMENTATION_READINESS_DECISION',
        source_commit=BASE,candidate=version,sources=41,exports=268,behavioral_members=704,callable_members=643,
        configurations=controls,cross_configuration_different_rows=differing,
        policy='Each configuration has its own exact original reference. No cross-configuration normalization or default tolerance.',
        native='NATIVE_EXCEL_NOT_RUN; unregistered native boundary test doubles only',scientific='NOT_GOLDEN',
        production_authorized_by_this_reader=False), detailed


def main():
    parser=argparse.ArgumentParser();parser.add_argument('--root',type=Path,default=Path(__file__).resolve().parents[2]);parser.add_argument('--version',default='capture-v10');parser.add_argument('--count',type=int,default=24732);parser.add_argument('--output',type=Path)
    args=parser.parse_args()
    try:
        result, detailed=inspect(args.root,args.version,args.count)
        if args.output:
            with args.output.open('x',encoding='utf-8',newline='\n') as stream:json.dump(result,stream,indent=2);stream.write('\n')
            with args.output.with_suffix('.cases.csv').open('x',encoding='utf-8',newline='') as stream:
                writer=csv.DictWriter(stream,fieldnames=list(detailed[0]),lineterminator='\n');writer.writeheader();writer.writerows(detailed)
        print(json.dumps(dict(status=result['status'],configurations={k:dict(rows=v['rows'],sha256=v['sha256'],callable_entries_reached=v['callable_entries_reached']) for k,v in result['configurations'].items()})))
    except (InvalidReference,OSError,KeyError,ValueError) as error:
        print(json.dumps(dict(status='NOT_READY_04A38',reference_error=str(error))));raise SystemExit(1)


if __name__=='__main__':main()
