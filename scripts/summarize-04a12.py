"""Sanitized 04A.12 result accounting. Originals and historic assertions are read-only."""
import argparse
import base64
from collections import Counter
import importlib.util
import json
import math
from pathlib import Path
import re
import sys
import xml.etree.ElementTree as ET

ROOT = Path(__file__).resolve().parents[1]
spec = importlib.util.spec_from_file_location('summary11', ROOT / 'scripts/summarize-04a11.py')
old = importlib.util.module_from_spec(spec)
spec.loader.exec_module(old)
spec = importlib.util.spec_from_file_location('contract12', ROOT / 'scripts/capture-04a12-contract.py')
c = importlib.util.module_from_spec(spec)
spec.loader.exec_module(c)
OUT, RUN, NS = c.OUT, c.RUN, old.NS


def write(name, value):
    # Current unaccepted summary may be refreshed after bounded focused fixes.
    def finite_json(item):
        if isinstance(item, float) and not math.isfinite(item):
            return dict(clr='Double', nonfinite='NaN' if math.isnan(item) else ('+Infinity' if item > 0 else '-Infinity'))
        if isinstance(item, list):
            return [finite_json(v) for v in item]
        if isinstance(item, dict):
            return {k: finite_json(v) for k, v in item.items()}
        return item
    value = finite_json(c.prior.sanitize(value))
    (OUT / name).write_text(json.dumps(value, indent=2, ensure_ascii=False, allow_nan=False) + '\n', encoding='utf-8', newline='\n')


def python_methods(logs):
    identities, events, totals = {}, [], []
    for log in logs:
        text = log.read_text(encoding='utf-8', errors='replace')
        for outcome, method, context in re.findall(r'^(FAIL|ERROR): [^\n]+? \((test_[^()]+)\)([^\n]*)$', text, re.M):
            identities[method] = outcome
            events.append(dict(identity=method, outcome=outcome, context=context.strip()))
        totals.extend(int(n) for n in re.findall(r'^Ran (\d+) tests? in ', text, re.M))
    return dict(total=sum(totals), passed=sum(totals) - len(identities), nonpasses=identities, events=events,
                scope='Distinct unittest methods per disjoint discovery roots; subtest failures stay attached to their real method, never discarded. Extra probes are excluded.')


def entry():
    suites = [old.trx(RUN / 'entry-Debug/results/Core.trx'),
              old.trx(RUN / 'executions/entry-Debug/results/Common.trx'),
              old.trx(RUN / 'executions/entry-Debug-windows/results/Legacy.trx')]
    for suite in suites:
        del suite['records']
    py = python_methods([RUN / ('entry-python-' + name + '.log') for name in ['tools', 'help', 'scripts']])
    write('entry-validation.json', dict(base=c.BASE, managed_configuration='Debug', managed_suites=suites,
        python_discovery=py, limitation='Entry re-executes Core/Common/Windows and the three standard Python roots. Office/JS and Common semantic-projection Python are run in final matrix, not included in entry denominator.',
        owner_checkpoint='Historical six logical managed and 61 Python nonpasses are references, not substituted for these live observations. Publication-mode/branch snapshots can add inherited source nonpasses after merge.'))


def validation():
    entry_record = json.loads((OUT / 'entry-validation.json').read_bytes())
    entry_managed = {(r['class_name'], r['method'], r['test_name'], r['outcome']) for s in entry_record['managed_suites'] for r in s['nonpasses']}
    entry_py = entry_record['python_discovery']['nonpasses']
    prior_py = json.loads((ROOT / 'evidence/04A11/validation-final.json').read_bytes())['configurations']['Debug']['python_nonpasses']
    configs = {}
    for configuration in ['Debug', 'Release']:
        name = 'final-' + configuration
        paths = [RUN / name / configuration / 'results' / p for p in ['Core.trx', 'OfficeAddin.trx', 'OfficeExecution.trx']]
        paths += [RUN / 'executions' / (name + '-' + p) / 'results' / (p + '.trx') for p in ['Common', 'Legacy']]
        paths += [RUN / 'projection-executions' / name / 'results/ControlMitigation.trx']
        suites = [old.trx(path) for path in paths]
        groups = old.deduplicate(suites)
        logical = dict(total=len(groups), passed=sum(all(r['outcome'] == 'Passed' for r in rows) for rows in groups.values()),
            failed=sum(any(r['outcome'] == 'Failed' for r in rows) for rows in groups.values()),
            skipped=sum(all(r['outcome'] in ['NotExecuted', 'Skipped'] for r in rows) for rows in groups.values()))
        current = {(r['class_name'], r['method'], r['test_name'], r['outcome']) for s in suites for r in s['nonpasses']}
        newly = sorted(current - entry_managed)
        collisions = [dict(identity=list(key), executions=rows) for key, rows in groups.items() if len(rows) > 1]
        new_cases = [rows for key, rows in groups.items() if any('Gee12' in (r['class_name'] or '') for r in rows)]
        for s in suites:
            s['batch12'] = [r for r in s['records'] if 'Gee12' in (r['class_name'] or '')]
            del s['records']
        py = python_methods([RUN / (name + '-full.log'), RUN / (name + '-common-python.log')])
        new_py = sorted(set(py['nonpasses']) - set(entry_py))
        full = (RUN / (name + '-full.log')).read_text(encoding='utf-8', errors='replace')
        node_text = full.replace('\u2139', '#')
        nodes = [{k: int(v) for k, v in zip(['total', 'passed', 'failed', 'cancelled', 'skipped'], match)} for match in
                 re.findall(r'# tests (\d+)\n# suites \d+\n# pass (\d+)\n# fail (\d+)\n# cancelled (\d+)\n# skipped (\d+)', node_text)]
        historical_current_pass = sorted(set(prior_py) - set(py['nonpasses']))
        assert set(historical_current_pass) <= {'test_04a05_contingency.ContingencySourceGates.test_no_designer_udf_numerical_packaging_or_framework_change'}
        new_source = sorted(set(py['nonpasses']) - set(prior_py) - set(entry_py))
        allowed_source = {'test_04a11_publication.Publication11Tests.test_accepted_dossier_and_dormant_patches_unchanged'}
        assert set(new_source) <= allowed_source, 'Unexplained new Python nonpass: ' + str(new_source)
        assert not newly, 'New functional managed nonpass: ' + str(newly)
        assert len(nodes) == 3 and all(n['failed'] == 0 and n['skipped'] == 0 for n in nodes)
        configs[configuration] = dict(managed_suites=suites, logical_cases=logical,
            identity_policy='Exact shared test source SHA/class/method/data-row identity, unambiguous across suites; only MSTest String quote presentation normalized. Failed execution keeps logical case failed; unresolved identities stay separate.',
            collision_ledger=collisions, new_functional_managed_nonpasses=newly,
            batch12_logical_cases=len(new_cases), batch12_logical_passed=sum(all(r['outcome'] == 'Passed' for r in rows) for rows in new_cases),
            python=py, python_new_since_entry=new_py,
            python_inherited_prior_04A11=sorted(prior_py),
            historical_nonpass_currently_passing_scope_guard=historical_current_pass,
            historical_scope_guard_explanation='The untouched C05 test inspects git diff, not prior Core history. It already passes at this merged entry because C11 Core changes are committed and C12 changes no tracked Core product file. Its historical failing observation/receipt remains unchanged; this is not a statistical correction or historical failure erasure.',
            python_additional_already_present_at_live_entry=sorted((set(py['nonpasses']) - set(prior_py)) & set(entry_py)),
            python_new_expected_source_pin_cases=new_source,
            new_source_pin_disposition='C11 publication task-entry snapshot still pins the Windows test project. Four approved current-file changes trigger its frozen equality test. New C12 scope/private-pin test verifies all unrelated source and prior evidence bytes; original assertion remains failing.',
            node_suites=nodes, node_total=sum(r['total'] for r in nodes), node_passed=sum(r['passed'] for r in nodes))
    write('validation.json', dict(configurations=configs, native_executed=False, P='NO',
        focused_repeats='Excluded from full distinct totals. Explicit CJS repeat is coverage confirmation, not an extra denominator.',
        inherited_frozen_expectations='Unchanged tests remain actual nonpasses. Source-pin/scope successors do not rewrite original test assertions.'))
    print({config: dict(managed=data['logical_cases'], python=(data['python']['passed'], data['python']['total']),
                       node=(data['node_passed'], data['node_total']), new_python=data['python_new_since_entry'],
                       new_managed=data['new_functional_managed_nonpasses']) for config, data in configs.items()})


def fixtures():
    registry = json.loads((OUT / 'baseline-registry.json').read_bytes())
    def decode(line):
        kind, _, payload = line.partition(':')
        if kind == 'String':
            return base64.b64decode(payload).decode('utf-8')
        if kind == 'D':
            return float(payload)
        if kind.startswith(('Int', 'UInt')):
            return int(base64.b64decode(payload).decode('utf-8'))
        return payload
    cases = []
    for path in registry['files']:
        lines = (ROOT / path).read_text(encoding='utf-8').splitlines()
        failure = next((s.partition('|')[2] for s in lines if s.startswith('CORE_FAILURE|')), None)
        tables, convergence, qic, qicu = [], None, None, None
        for i, line in enumerate(lines):
            if not line.startswith('TABLE '):
                continue
            rows, cols, *_ = map(int, line.split()[1:7])
            cells = [decode(s) for s in lines[i + 2:i + 2 + rows * cols]]
            title = cells[0] if cells else None
            tables.append(dict(rows=rows, columns=cols, first_cell=title))
            for pos, value in enumerate(cells):
                if value == 'Converged?':
                    convergence = str(cells[pos + 1]).lower() == 'true'
                elif value == 'QIC':
                    qic = cells[pos + 1]
                elif value == 'QICu':
                    qicu = cells[pos + 1]
        cases.append(dict(path=path, key=lines[1], source_rows=lines[2].partition('|')[2],
            failure=failure, Core_converged=convergence, QIC=qic, QICu=qicu, tables=tables,
            before_sha256=registry['files'][path]['sha256'], after='Automated portable and shipping-adapter cases compare to these same frozen bytes with the established typed-cell tolerance.'))
    write('numerical-comparisons.json', dict(status='NOT_GOLDEN', cases=cases, Core_math_unchanged=True,
        fitted_scale='Actual Core response means; no second inverse link. Explicit cluster-to-input mapping is identity for sorted Windows import.',
        independent_reference='No new R or external reference executed. Existing committed GEE reference tests are retained and rerun in full Core suites.',
        input_trace=dict(paths=['evidence/04A12/input-time.txt', 'evidence/04A12/input-notime.txt', 'evidence/04A12/input-irregular.txt'],
            chronology='Expanded raw/filtered/Core-input trace emitted after extraction from the unchanged source-pinned pre-edit fixture and unchanged Core importer. Original pre-edit fit captures remain immutable. These trace files are not represented as earlier-created receipts.')))


def commands():
    records = []
    for path in sorted(RUN.glob('*-command.json')):
        record = json.loads(path.read_bytes())
        records.append(dict(raw_receipt_sha256=c.sha(path), local_receipt=path.relative_to(ROOT).as_posix(),
                            public_derivative=c.prior.sanitize(record)))
    write('command-outcomes.json', dict(commands=records,
        command_identity='Exact arguments/exit codes with owner-local paths replaced by portable placeholders. Original raw receipts stay in ignored local evidence; derivative bytes do not share raw hashes.'))


if __name__ == '__main__':
    parser = argparse.ArgumentParser()
    parser.add_argument('action', choices=['entry', 'validation', 'fixtures', 'commands'])
    globals()[parser.parse_args().action]()
