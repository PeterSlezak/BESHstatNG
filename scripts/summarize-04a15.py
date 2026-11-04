"""Sanitized 04A.15 accounting. Raw logs/TRX and prior evidence are immutable."""
import argparse
from collections import Counter
import importlib.util
import json
import re
import shutil
import xml.etree.ElementTree as ET
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]


def module(name, path):
    spec = importlib.util.spec_from_file_location(name, ROOT / path)
    result = importlib.util.module_from_spec(spec)
    spec.loader.exec_module(result)
    return result


c = module('contract15', 'scripts/capture-04a15-contract.py')
prior = module('summary14', 'scripts/summarize-04a14.py')
old = prior.old
RUN, OUT = c.RUN, c.OUT
NS = '{http://microsoft.com/schemas/VisualStudio/TeamTest/2010}'
NAMES = {'Debug': 'final-Debug-complete', 'Release': 'final-Release'}


def write(name, doc):
    c.write(name, doc)


def python(logs):
    return prior.previous.python_methods(logs)


def entry():
    suites = [old.trx(RUN / 'entry-Debug/results/Core.trx'), old.trx(RUN / 'executions/entry-Debug/results/Common.trx'),
              old.trx(RUN / 'executions/entry-Debug-windows/results/Legacy.trx')]
    for suite in suites:
        suite.pop('records')
    py = python([RUN / ('entry-python-' + p + '.log') for p in ['tools', 'help', 'scripts']] + [RUN / 'entry-comparable-common-python.log'])
    write('entry-validation.json', dict(base=c.BASE, managed_suites=suites, python=py,
        limits='Debug Core/Common/Windows measured before shipping-route edits. Release/Office/Node entry not separately measured. Common Python comparable probe uses fresh 15/15 unchanged C02 semantic observations; original missing-environment probe remains locally retained.'))
    print('Entry Python', py['passed'], '/', py['total'], len(py['nonpasses']), 'nonpasses')


def validation():
    baseline = json.loads((OUT / 'entry-validation.json').read_bytes())
    configs = {}
    for config in ['Debug', 'Release']:
        name = NAMES[config]
        paths = [RUN / name / config / 'results' / p for p in ['Core.trx', 'OfficeAddin.trx', 'OfficeExecution.trx']]
        paths += [RUN / 'executions' / (name + '-' + p) / 'results' / (p + '.trx') for p in ['Common', 'Legacy']]
        paths += [RUN / 'projection-executions' / name / 'results/ControlMitigation.trx']
        suites = [old.trx(p) for p in paths]
        groups = prior.typed_groups(suites)
        weight = prior.weight
        logical = dict(total=sum(weight(k) for k in groups), passed=sum(weight(k) for k, rows in groups.items() if all(r['outcome'] == 'Passed' for r in rows)),
            nonpassed=sum(weight(k) for k, rows in groups.items() if any(r['outcome'] != 'Passed' for r in rows)))
        new_cases = {k: rows for k, rows in groups.items() if any('Mixed15' in r['class_name'] for r in rows)}
        new_nonpasses = [r for rows in new_cases.values() for r in rows if r['outcome'] != 'Passed']
        assert not new_nonpasses, new_nonpasses
        for suite in suites:
            suite['batch15_total'] = sum('Mixed15' in r['class_name'] for r in suite['records'])
            suite['batch15_passed'] = sum('Mixed15' in r['class_name'] and r['outcome'] == 'Passed' for r in suite['records'])
            suite.pop('records')
            suite.pop('batch11', None)
        identity = lambda row: (row['class_name'].split('.')[-1], row['method'], row['test_name'].replace('"', ''))
        initial_managed = {identity(row) for suite in baseline['managed_suites'] for row in suite['nonpasses']}
        current_managed = {identity(row) for suite in suites for row in suite['nonpasses']}
        assert current_managed == initial_managed, (current_managed - initial_managed, initial_managed - current_managed)
        py = python([RUN / (name + '-full.log'), RUN / (name + '-common-python.log')])
        assert not [p for p in py['nonpasses'] if '04a15' in p]
        log = (RUN / (name + '-full.log')).read_text(encoding='utf-8', errors='replace')
        configs[config] = dict(managed_suites=suites, logical_cases=logical,
            runtime_executions=dict(total=sum(int(s['counters']['total']) for s in suites), passed=sum(int(s['counters']['passed']) for s in suites), failed=sum(int(s['counters']['failed']) for s in suites)),
            batch15_logical_total=sum(weight(k) for k in new_cases), batch15_logical_passed=sum(weight(k) for k, rows in new_cases.items() if all(r['outcome'] == 'Passed' for r in rows)),
            new_managed_nonpasses=[], changed_managed_failure_identities=[],
            node_passed=sum(map(int, re.findall(r'\bpass (\d+)\s*$', log, re.M))), node_total=sum(map(int, re.findall(r'\btests (\d+)\s*$', log, re.M))),
            python=py, python_new_since_entry={p: v for p, v in py['nonpasses'].items() if p not in baseline['python']['nonpasses']},
            python_entry_now_passing=sorted(set(baseline['python']['nonpasses']) - set(py['nonpasses'])),
            full_command_exit=json.loads((RUN / (name + '-full-command.json')).read_bytes())['exit_code'])
    write('validation.json', dict(configurations=configs,
        accounting='Logical managed identities use exact shared source SHA, class, method and data row with unambiguous MSTest 2/3 display normalization; retained Mixed14 null cohorts use their established logical weights. Any failed runtime keeps the logical case failed. Runtime executions separately counted. Focused and semantic repeats excluded.',
        all_final_production_sources=True, native=False, independent_R='NOT RUN; same-engine compatibility only, no scientific promotion.'))
    for config, data in configs.items():
        print(config, data['logical_cases'], 'Node', data['node_passed'], '/', data['node_total'], 'Python', data['python']['passed'], '/', data['python']['total'], 'new Python', data['python_new_since_entry'])


def numerical():
    records = []
    for config in ['Debug', 'Release']:
        for runtime, folder, filename in [('net10', 'Common', 'Common'), ('net48', 'Legacy', 'Legacy')]:
            path = RUN / 'executions' / (NAMES[config] + '-' + folder) / 'results' / (filename + '.trx')
            root = ET.parse(path).getroot()
            for node in root.findall('.//' + NS + 'UnitTestResult'):
                for stdout in node.findall('.//' + NS + 'StdOut'):
                    for line in (stdout.text or '').splitlines():
                        if line.startswith('04A15_CASE|'):
                            parts = line.split('|')[1:]
                            records.append(dict(configuration=config, runtime=runtime, kind=parts[0], residual=parts[1], random=parts[2], optimizer=parts[3], fit=parts[4], culture=parts[5] or 'invariant', status=parts[6], objective=parts[7] if len(parts) > 7 else None))
    assert len(records) == 176 * 2 * 2 * 2, len(records)
    write('fit-comparisons-v1.json', dict(version=1, status='NOT_GOLDEN', records=records, statuses=dict(Counter(r['status'] for r in records)),
        provenance='New 04A.15 synthetic missing-longitudinal and expanded random-design fixtures. Original Core factories and frozen LegacyMixed14 parsers plus prechange control projection compared with specialized requests on the identical input blocks. Old 25 fixture families/100 references also executed unchanged in both runtime/configuration matrices.',
        tolerance='max(1e-9, abs(expected)*1e-9) for double captures/objective; exact labels, row/block permutation, dimensions, CLR cell kinds, statuses, inference and table order. Computational timing cells alone masked by unchanged Mixed14 canonicalizer. Tolerance fixed before first fit; never tuned.',
        interpretation='CONVERGED/NONCONVERGED/CORE_REJECTED are original Core outcomes, not PASS fit relabels. Policy-only 108 R/G combinations are separate metadata tests. No independent statistical validation or native Excel equivalence claimed.'))
    print('Numerical records', len(records), dict(Counter(r['status'] for r in records)))


def nonpasses():
    final = json.loads((OUT / 'validation.json').read_bytes())['configurations']
    entry_doc = json.loads((OUT / 'entry-validation.json').read_bytes())
    accepted = json.loads((ROOT / 'evidence/04A14/nonpasses.json').read_bytes())
    accepted_map = {row['identity']: row for row in accepted['python']}
    traces = {}
    for config in ['Debug', 'Release']:
        for log in [RUN / (NAMES[config] + '-full.log'), RUN / (NAMES[config] + '-common-python.log')]:
            text = log.read_text(encoding='utf-8', errors='replace')
            for match in re.finditer(r'^(?:FAIL|ERROR): [^\n]+? \((test_[^()]+)\)([^\n]*)\n-+\n(.*?)(?=\n=+\n|\n-+\nRan |\Z)', text, re.M | re.S):
                terminal = next((line for line in reversed(match[3].splitlines()) if line.strip()), '')
                terminal = re.sub(r'[A-Za-z]:[\\/][^\r\n]+', '<local-path>', terminal)
                traces.setdefault(match[1], []).append(dict(raw_log_alias='LOCAL_LOG:' + log.stem,
                    raw_sha256=c.sha(log), traceback_sha256=__import__('hashlib').sha256(match[3].encode()).hexdigest(),
                    terminal=terminal[:700]))
    records = []
    for identity, outcome in final['Debug']['python']['nonpasses'].items():
        inherited = identity in entry_doc['python']['nonpasses']
        old_record = accepted_map.get(identity, {})
        explanation = old_record.get('explanation', old_record.get('root_cause'))
        if not explanation:
            explanation = 'Frozen historical branch/main/source-scope or provenance expectation differs on the accepted successor branch; no historical assertion was edited.'
        records.append(dict(identity=identity, outcome=outcome, inherited_at_entry=inherited,
            accepted_04A14_nonpass=identity in accepted_map, root_cause=explanation, current_raw_traceback_evidence=traces.get(identity, [])))
    managed = []
    for suite in final['Debug']['managed_suites']:
        for row in suite['nonpasses']:
            method = row['method']
            if method == 'ConstantAndSeparatedBinomialPreserveActualFitWarnings':
                reason = 'Frozen assertion expects NullReferenceException for constant binomial response; accepted defensive report returns without throwing.'
            elif method == 'PairedCompatibilityEvidenceIsExplicitlyLegacy_NotACorrectedGolden':
                reason = 'Existing paired scientific mapping assertion differs (expected approximately 6.493107e-8 versus actual 0.4995598); unchanged by mixed-model work.'
            elif method == 'ExistingEquivalenceCoreDomainAndPairedOverflowAreNotReinterpreted':
                reason = 'Frozen assertion expects OverflowException; current accepted implementation returns without throwing.'
            else:
                reason = 'Existing paired geometry/inference assertion expects refusal of a conflicting mapping; current accepted validation does not refuse it.'
            managed.append(dict(**row, suite=suite['path'], root_cause=reason, inherited_at_entry=True))
    write('nonpasses.json', dict(managed=managed, python=records,
        python_new_since_entry=final['Debug']['python_new_since_entry'],
        strict=dict(exit_code=1, reason='Stale UibyID/NormalPlot review dependency; historical sources/evidence unchanged.'),
        profile=dict(exit_code=1, reason='INTEGRITY_FAILURE under historical inventory-complete-with-approved-deferrals profile; no gate bypass.'),
        original_04A14_register_sha256=c.sha(ROOT / 'evidence/04A14/nonpasses.json'),
        no_historical_tests_or_evidence_rewritten=True))


def retention():
    raw = json.loads((RUN / 'preservation-final.json').read_bytes())
    assert raw['errors'] == [] and raw['status'] == 'PASS', raw['errors']
    initial = json.loads((RUN / 'nested-initial-metadata.json').read_bytes())
    current = {p.relative_to(ROOT).as_posix() for project in ['BESHStat.Core.Tests', 'BESHStat.Excel.Common.Tests', 'BESHStatNG.Test'] for p in (ROOT / project / 'TestResults').rglob('*') if p.is_file()}
    assert current == {row['path'] for row in initial}
    for row in initial:
        s = (ROOT / row['path']).stat()
        assert (s.st_size, s.st_mtime_ns, s.st_ctime_ns, s.st_ino, s.st_dev) == tuple(row[k] for k in ['bytes', 'mtime_ns', 'ctime_ns', 'ino', 'dev'])
    c.source()
    write('retention.json', dict(status='PASS_HISTORICAL_MEMBERSHIP_METADATA', root_files=raw['historical_files'], nested_files=len(initial), membership_metadata_errors=0,
        raw_receipt=dict(alias='LOCAL_RETENTION:final', sha256=c.sha(RUN / 'preservation-final.json'), exit_code=0, status=raw['status'], errors=raw['errors']),
        raw_reason='Inherited generic preservation runner passes every original historical membership/metadata and protected documentation/evidence source pin. Source-contract additionally proves all other original tracked bytes and the ten approved before/after hashes, including five MkDocs pages.',
        historical_scope='Membership, byte size, creation/write nanoseconds and file identity; not an all-historical-byte audit. Frozen captures/ledgers/protected sources additionally SHA-256 verified.',
        initial_free_bytes=json.loads((RUN / 'start.json').read_bytes())['disk_free_bytes'], final_free_bytes=shutil.disk_usage(ROOT).free))


def commands():
    records = []
    for path in sorted(RUN.glob('*-command.json')):
        raw = json.loads(path.read_bytes())
        records.append(dict(alias='LOCAL_COMMAND:' + path.stem, raw_sha256=c.sha(path), command=raw['command'], exit_code=raw['exit_code'], seconds=raw['seconds']))
    for path in sorted((RUN / 'executions').glob('*/execution.json')):
        raw = json.loads(path.read_bytes())
        records.append(dict(alias='LOCAL_EXECUTION:' + path.parent.name, raw_sha256=c.sha(path),
            command=raw['command'], exit_code=raw['exit_code'], seconds=raw['seconds']))
    write('command-outcomes.json', dict(records=records, raw_scope='Local ignored command receipts; public commands sanitize machine/profile paths. No raw-to-public hash equivalence asserted.'))
    adaptations = []
    for path in sorted(RUN.glob('*-verify-isolated.provenance.json')):
        adaptations.append(dict(alias=path.stem, raw_sha256=c.sha(path), **json.loads(path.read_bytes())))
    write('execution-adaptations.json', dict(records=adaptations,
        reason='Unmodified default verifier overwrites historical TestResults/Codex and invokes packaging/build hooks. Accepted exact-anchor derivatives redirect results, use no-restore, isolate managed tests and compile the real Windows host without packaging/signing. Continue independent suites while retaining nonzero aggregate status.'))


if __name__ == '__main__':
    p = argparse.ArgumentParser()
    p.add_argument('action', choices=['entry', 'validation', 'numerical', 'nonpasses', 'retention', 'commands'])
    globals()[p.parse_args().action]()
