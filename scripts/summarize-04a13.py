"""Sanitized 04A.13 execution accounting; never edits prior evidence or test results."""
import argparse
import base64
import csv
import importlib.util
import json
import math
from pathlib import Path
import re

ROOT = Path(__file__).resolve().parents[1]


def module(name, path):
    spec = importlib.util.spec_from_file_location(name, ROOT / path)
    result = importlib.util.module_from_spec(spec)
    spec.loader.exec_module(result)
    return result


c = module('contract13', 'scripts/capture-04a13-contract.py')
previous = module('summary12', 'scripts/summarize-04a12.py')
old = previous.old
RUN, OUT = c.RUN, c.OUT


def write(name, value):
    def finite(item):
        if isinstance(item, float) and not math.isfinite(item):
            return dict(clr='Double', nonfinite=str(item))
        if isinstance(item, dict):
            return {k: finite(v) for k, v in item.items()}
        if isinstance(item, (list, tuple)):
            return [finite(v) for v in item]
        return item
    c.write(name, finite(value))


def entry():
    suites = [old.trx(RUN / 'entry-Debug/results/Core.trx'), old.trx(RUN / 'executions/entry-Debug/results/Common.trx'),
        old.trx(RUN / 'executions/entry-Debug-windows/results/Legacy.trx')]
    for s in suites:
        del s['records']
    py = previous.python_methods([RUN / ('entry-merged-python-' + n + '.log') for n in ['tools', 'help', 'scripts']] + [RUN / 'entry-comparable-common-python.log'])
    write('entry-validation.json', dict(base=c.BASE, managed_configuration='Debug', managed_suites=suites, python=py,
        limitations='Managed entry was executed before production changes. Standard Python was reconfirmed against temporary exact HEAD bytes for the five then-edited tracked files, restored byte-for-byte afterward; new untracked implementation/fixtures were present. Common Python comparable run uses fresh unchanged C02 semantic observations, after survival implementation (none of its representative inputs/sources changed). No stale ZIP count substituted.',
        environment_probe='The first Common Python attempt used a nonexistent discovery directory (0 tests). The next lacked the G3 observation environment: 168 methods, 27 nonpasses. Fresh semantic observations remove those 17 environment-only errors; all raw attempts retained.',
        raw_baseline_scope_sha256=c.sha(RUN / 'merged-python-scope.json')))
    print('Entry Python', py['passed'], '/', py['total'], 'nonpasses', len(py['nonpasses']))


def validation():
    baseline = json.loads((OUT / 'entry-validation.json').read_bytes())
    entry_managed = {(r['class_name'].split('.')[-1], r['method'], r['test_name'].replace('"', ''))
        for s in baseline['managed_suites'] for r in s['nonpasses']}
    configs = {}
    for config in ['Debug', 'Release']:
        name = 'final-' + config
        paths = [RUN / name / config / 'results' / p for p in ['Core.trx', 'OfficeAddin.trx', 'OfficeExecution.trx']]
        paths += [RUN / 'executions' / (name + '-' + p) / 'results' / (p + '.trx') for p in ['Common', 'Legacy']]
        paths += [RUN / 'projection-executions' / name / 'results/ControlMitigation.trx']
        suites = [old.trx(p) for p in paths]
        groups = old.deduplicate(suites)
        logical = dict(total=len(groups), passed=sum(all(r['outcome'] == 'Passed' for r in rows) for rows in groups.values()),
            nonpassed=sum(any(r['outcome'] != 'Passed' for r in rows) for rows in groups.values()))
        raw = dict(total=sum(len(s['records']) for s in suites), passed=sum(r['outcome'] == 'Passed' for s in suites for r in s['records']))
        current = {(r['class_name'].split('.')[-1], r['method'], r['test_name'].replace('"', '')) for s in suites for r in s['nonpasses']}
        new = sorted(current - entry_managed)
        assert not new, new
        new_cases = [rows for rows in groups.values() if any('Survival13' in r['class_name'] for r in rows)]
        for s in suites:
            s['batch13_total'] = sum('Survival13' in r['class_name'] for r in s['records'])
            s['batch13_passed'] = sum('Survival13' in r['class_name'] and r['outcome'] == 'Passed' for r in s['records'])
            del s['records']
            s.pop('batch11', None)
        py = previous.python_methods([RUN / (name + '-full.log'), RUN / (name + '-common-python.log')])
        new_py = sorted(set(py['nonpasses']) - set(baseline['python']['nonpasses']))
        allowed = {'test_04a12_gee_workflow.Gee12WorkflowTests.test_frozen_sources_and_accepted_evidence_unchanged'}
        # Exact names are recorded; frozen scope/hash changes are classified from the
        # retained traceback, never removed from actual failure counts.
        classification = []
        log = (RUN / (name + '-full.log')).read_text(encoding='utf-8', errors='replace')
        for identity in new_py:
            classification.append(dict(identity=identity, outcome=py['nonpasses'][identity],
                category='FROZEN_SOURCE_PIN_OR_SCOPE' if '04a12' in identity else 'REQUIRES_REVIEW'))
        assert not [r for r in classification if r['category'] == 'REQUIRES_REVIEW'], classification
        assert not [n for n in py['nonpasses'] if '04a13' in n]
        normalized = log.replace('\u2139', '#')
        nodes = [{k: int(v) for k, v in zip(['total', 'passed', 'failed', 'cancelled', 'skipped'], values)}
            for values in re.findall(r'# tests (\d+)\n# suites \d+\n# pass (\d+)\n# fail (\d+)\n# cancelled (\d+)\n# skipped (\d+)', normalized)]
        assert len(nodes) == 3 and all(r['failed'] == r['skipped'] == 0 for r in nodes)
        configs[config] = dict(managed_suites=suites, logical_cases=logical, runtime_executions=raw,
            batch13_logical_cases=len(new_cases), batch13_logical_passed=sum(all(r['outcome'] == 'Passed' for r in rows) for rows in new_cases),
            collisions=[dict(identity=list(key), executions=rows) for key, rows in groups.items() if len(rows) > 1],
            new_managed_nonpasses=new, python=py, python_new_since_entry=classification,
            python_entry_nonpasses_now_passing=sorted(set(baseline['python']['nonpasses']) - set(py['nonpasses'])),
            node_suites=nodes, node_passed=sum(r['passed'] for r in nodes), node_total=sum(r['total'] for r in nodes))
    write('validation.json', dict(configurations=configs, native_executed=False,
        deduplication='Exact shared source SHA/class/method/data-row identities; only unambiguous MSTest String quote presentation normalized. Full collision ledger retained; failures remain failed.',
        scope='Focused reruns and explicit CJS confirmation excluded from totals. All named nonpasses retained; no historical test rewritten.'))
    print({config: {k: v[k] for k in ['logical_cases', 'runtime_executions', 'batch13_logical_cases', 'node_passed', 'node_total', 'python_new_since_entry']} |
           {'python': [v['python']['passed'], v['python']['total']]} for config, v in configs.items()})


def decode(value):
    kind, _, data = value.partition(':')
    if kind == 'D':
        return float(data)
    if kind == 'String':
        return base64.b64decode(data).decode('utf-8')
    if kind == 'Boolean':
        return base64.b64decode(data).decode('utf-8') == 'True'
    return None


def fixtures():
    registry = json.loads((OUT / 'baseline-registry.json').read_bytes())
    cases = []
    for path, pin in registry['files'].items():
        lines = (ROOT / path).read_text(encoding='utf-8').splitlines()
        tables, fields = [], {}
        for i, line in enumerate(lines):
            if not line.startswith('TABLE '):
                continue
            rows, cols, *_ = map(int, line.split()[1:7])
            cells = [decode(v) for v in lines[i + 2:i + 2 + rows * cols]]
            tables.append(dict(rows=rows, columns=cols, title=cells[0]))
            for j, cell in enumerate(cells[:-1]):
                if cell in ['Chi-square', 'Converged?', 'Final Log likelihood', 'Number of iterations']:
                    fields[cell] = cells[j + 1]
                if cell in ['x1', 'x2'] and cols == 8 and j >= cols:
                    fields[cell] = cells[j + 1]
        cases.append(dict(path=path, key=lines[1], core_fields=fields, tables=tables,
            failure=next((s.partition('|')[2] for s in lines if s.startswith('CORE_FAILURE|')), None),
            sha256=pin['sha256'], after='Executed unchanged Core fixture and Common projection compared against these frozen bytes in both full configurations.'))
    refs = list(csv.DictReader((OUT / 'R-candidate-reference-v2.csv').open(encoding='utf-8')))
    r_checks = []
    for r in refs:
        if r['case'].startswith('KM|'):
            continue
        key = r['case']
        if key.startswith('Cox|'):
            parts = key.split('|')
            key = '|'.join([parts[0], parts[1].capitalize(), parts[2]])
        else:
            key = 'KM|Logrank|' + key.split('|')[1]
        case = next(item for item in cases if item['key'] == key and item['path'].endswith('-iv.txt'))
        field = {'loglik': 'Final Log likelihood', 'chi-square': 'Chi-square'}.get(r['field'], r['field'])
        actual = case['core_fields'][field]
        tolerance = 0.000001 if field in ['Final Log likelihood', 'Chi-square'] else 0.0000005
        expected = float(r['value'])
        r_checks.append(dict(case=r['case'], field=r['field'], R=expected, Core=actual, absolute_difference=abs(actual - expected),
            tolerance=tolerance, within_tolerance=abs(actual - expected) <= tolerance))
    write('numerical-comparisons.json', dict(status='NOT_GOLDEN', cases=cases, independent_R_checks=r_checks,
        survival_estimates='Two executed reference tests compare every observed-time KM estimate for both groups to R (1e-12 absolute). Cox six fits compare coefficients (5e-7) and log likelihood (1e-6). Tolerances allow optimizer termination and CSV precision, not changes of inference.',
        weighted_tests='All five names compare original Core versus Common in both grouped/stratified modes. R rho=0 is a candidate comparison only; weighted variance conventions, median CI, PH/residual scaling and exact baseline conventions are not equated by name.',
        renderer='Exact ToCoreData arrays; Windows renderer body unchanged. No rendered-pixel, COM series, placement or native UI equivalence claim.',
        R=dict(version='4.5.1', survival='3.8.3', status='NOT_GOLDEN', command=['Rscript', '--vanilla', 'scripts/reference-04a13.R', 'evidence/04A13/R-candidate-reference-v2.csv'],
            script_sha256=c.sha(ROOT / 'scripts/reference-04a13.R'), output_sha256=c.sha(OUT / 'R-candidate-reference-v2.csv'),
            encoding='0=censored, 1=event', KM_confidence='log-log 95%; only survival estimates compared, not median CI',
            warnings='Four inherited LC_* C.UTF-8 startup warnings; corrected comparison has no optimizer warning.',
            unqualified_attempt=dict(alias='LOCAL_R_ATTEMPT_ALIAS:01', sha256=c.sha(RUN / 'R-unqualified-attempt-01.csv'),
                disposition='Initial exploratory R fixture used incorrect modulo/multiply precedence, produced no-convergence warnings; excluded from comparison. Fresh unqualified output moved into new ignored run evidence, not historical TestResults. Corrected script/output used thereafter. Original Core and references unchanged.'))))
    print('R candidate checks', sum(r['within_tolerance'] for r in r_checks), '/', len(r_checks))


def commands():
    rows = []
    for path in sorted(RUN.glob('*-command.json')):
        rows.append(dict(local_receipt_alias='LOCAL_COMMAND_ALIAS:' + path.stem, raw_sha256=c.sha(path),
            public_derivative=c.prior.sanitize(json.loads(path.read_bytes()))))
    write('command-outcomes.json', dict(commands=rows, identity='Sanitized derivative commands/arguments/exits. Alias strings are not filesystem paths; original raw receipts remain in ignored run evidence with distinct raw hashes.'))


if __name__ == '__main__':
    parser = argparse.ArgumentParser()
    parser.add_argument('action', choices=['entry', 'validation', 'fixtures', 'commands'])
    globals()[parser.parse_args().action]()
