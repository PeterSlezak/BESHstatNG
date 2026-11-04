"""04A.18 entry/final accounting over isolated immutable raw receipts."""
import argparse
import base64
from collections import Counter
import hashlib
import importlib.util
import json
from pathlib import Path
import re
import xml.etree.ElementTree as ET

ROOT = Path(__file__).resolve().parents[1]


def module(name, path):
    spec = importlib.util.spec_from_file_location(name, ROOT / path)
    result = importlib.util.module_from_spec(spec)
    spec.loader.exec_module(result)
    return result


c = module('contract18', 'scripts/capture-04a18-contract.py')
previous = module('summary17', 'scripts/summarize-04a17.py')
prior, old = previous.prior, previous.old
RUN, OUT = c.RUN, c.OUT
previous.c, previous.RUN, previous.OUT = c, RUN, OUT
NS = previous.NS


def write(name, value):
    c.write(name, value)


def suites_for(name, config):
    paths = [RUN / name / config / 'results' / p for p in ['Core.trx', 'OfficeAddin.trx', 'OfficeExecution.trx']]
    paths += [RUN / 'executions' / (name + '-' + p) / 'results' / (p + '.trx') for p in ['Common', 'Legacy']]
    paths += [RUN / 'projection-executions' / name / 'results/ControlMitigation.trx']
    return [old.trx(p) for p in paths]


def measured(name, config):
    suites = suites_for(name, config)
    groups = previous.typed_groups(suites)
    weight = prior.weight
    new_cases = {k: rows for k, rows in groups.items() if any('Multivariate18' in r['class_name'] for r in rows)}
    logical = dict(total=sum(weight(k) for k in groups), passed=sum(weight(k) for k, rows in groups.items() if all(r['outcome'] == 'Passed' for r in rows)),
                   nonpassed=sum(weight(k) for k, rows in groups.items() if any(r['outcome'] != 'Passed' for r in rows)))
    for suite in suites:
        suite['batch18_total'] = sum('Multivariate18' in r['class_name'] for r in suite['records'])
        suite['batch18_passed'] = sum('Multivariate18' in r['class_name'] and r['outcome'] == 'Passed' for r in suite['records'])
        suite.pop('records')
        suite.pop('batch11', None)
    py = previous.python([RUN / (name + '-full.log'), RUN / (name + '-common-python.log')])
    log = (RUN / (name + '-full.log')).read_text(encoding='utf-8', errors='replace')
    return dict(managed_suites=suites, logical_cases=logical,
        runtime_executions=dict(total=sum(int(s['counters']['total']) for s in suites), passed=sum(int(s['counters']['passed']) for s in suites), failed=sum(int(s['counters']['failed']) for s in suites)),
        batch18_logical_total=sum(weight(k) for k in new_cases), batch18_logical_passed=sum(weight(k) for k, rows in new_cases.items() if all(r['outcome'] == 'Passed' for r in rows)),
        node_passed=sum(map(int, re.findall(r'\bpass (\d+)\s*$', log, re.M))), node_total=sum(map(int, re.findall(r'\btests (\d+)\s*$', log, re.M))),
        python=py, full_command_exit=json.loads((RUN / (name + '-full-command.json')).read_bytes())['exit_code'])


def entry():
    configs = {config: measured('entry-' + config, config) for config in ['Debug', 'Release']}
    write('entry-state.json', dict(base=c.BASE, head=c.BASE, branch='codex/04A-18', configurations=configs,
        scope='Both complete matrices before production edits. Frozen derivative oracle added during entry Release has no TestClass/test identities. Historical 04A.17 counts are preserved separately; these are newly measured entry counts.',
        index_empty=True, native=False, R=False))
    for config, data in configs.items():
        print('entry', config, data['logical_cases'], data['runtime_executions'], 'Node', data['node_passed'], data['node_total'], 'Python', data['python']['passed'], data['python']['total'])


def validation():
    initial = json.loads((OUT / 'entry-state.json').read_bytes())['configurations']
    configs = {}
    identity = lambda r: (r['class_name'].split('.')[-1], r['method'], r['test_name'].replace('"', ''))
    for config in ['Debug', 'Release']:
        data = measured('final-' + config, config)
        before = {identity(r) for s in initial[config]['managed_suites'] for r in s['nonpasses']}
        after = {identity(r) for s in data['managed_suites'] for r in s['nonpasses']}
        data['new_managed_nonpasses'] = sorted(after - before)
        data['removed_managed_nonpass_identities'] = sorted(before - after)
        data['python_new_since_entry'] = {p: v for p, v in data['python']['nonpasses'].items() if p not in initial[config]['python']['nonpasses']}
        data['python_entry_now_passing'] = sorted(set(initial[config]['python']['nonpasses']) - set(data['python']['nonpasses']))
        assert not data['new_managed_nonpasses'], data['new_managed_nonpasses']
        assert data['batch18_logical_total'] == data['batch18_logical_passed'] == 154
        assert not [p for p in data['python']['nonpasses'] if '04a18' in p]
        configs[config] = data
        print('final', config, data['logical_cases'], data['runtime_executions'], 'Node', data['node_passed'], data['node_total'], 'Python', data['python']['passed'], data['python']['total'], 'new Python', data['python_new_since_entry'])
    write('validation.json', dict(configurations=configs,
        accounting='Exact shared source SHA/class/method/data-row identity deduplication, with established Mixed14 and Multivariate17 typed null/empty cohort weights preserved. Logical tests and raw framework executions are separate. Focused/semantic repeats excluded; no failure discarded.',
        all_final_production_sources=True, final_source_qualification='Both complete matrices follow final VB production, managed test, Python contract and Help edits. Later accounting/evidence/dossier-only edits are checked separately; no production behavior edit follows the aggregates.',
        new_logical_tests=154, new_python_contracts=8, independent_R='NOT RUN / NOT AUTHORIZED', native='NOT RUN / NOT AUTHORIZED', status='NOT_GOLDEN'))


def xml_value(node):
    if len(node) == 0:
        if node.attrib:
            result = dict(node.attrib)
            if node.text: result['text'] = node.text
            return result
        return node.text or ''
    result = dict(node.attrib)
    for child in node:
        if child.tag in result:
            if not isinstance(result[child.tag], list): result[child.tag] = [result[child.tag]]
            result[child.tag].append(xml_value(child))
        else:
            result[child.tag] = xml_value(child)
    return result


def numerical():
    records = []
    for config in ['Debug', 'Release']:
        for runtime, folder in [('net10', 'Common'), ('net48', 'Legacy')]:
            path = RUN / 'executions' / ('final-' + config + '-' + folder) / 'results' / (folder + '.trx')
            for node in ET.parse(path).getroot().findall('.//' + NS + 'UnitTestResult'):
                for stdout in node.findall('.//' + NS + 'StdOut'):
                    for line in (stdout.text or '').splitlines():
                        if line.startswith('04A18_XML|'):
                            payload = ET.fromstring(base64.b64decode(line.split('|', 1)[1]))
                            record = dict(configuration=config, runtime=runtime, raw_trx_alias='LOCAL_TRX:final-' + config + '-' + folder,
                                raw_trx_sha256=c.sha(path), record=xml_value(payload))
                            records.append(record)
    assert len(records) == 229 * 4, len(records)
    outcomes = Counter(r['record'].get('outcome', '') for r in records)
    payloads = {}
    for row in records:
        payload = c.prior.prior.sanitize(row.pop('record'))
        identity = hashlib.sha256(json.dumps(payload, sort_keys=True, ensure_ascii=False, separators=(',', ':'), allow_nan=False).encode('utf-8')).hexdigest()
        payloads.setdefault(identity, payload)
        row['profile_id'] = identity
    write('pca-ca-mca-projection-comparisons-v1.json', dict(version=1, status='NOT_GOLDEN', records=records, profile_payloads=payloads, payload_count=len(payloads), record_count=len(records), outcomes=dict(outcomes),
        payload_identity='profile_id hashes canonical public profile JSON: sorted keys, UTF-8, ensure_ascii=False, compact comma/colon separators, no NaN constants. Repeated runtime/configuration payloads share one detached descriptor; all 916 executions retain separate TRX provenance. These are derivative hashes, not raw XML hashes.',
        provenance='102 source-backed positive/edge PCA/MCA/labelled-CA profiles x two cultures; 4 retained unlabelled failures, 1 dormant mapping, 8 PCA negatives and 6 MCA negatives x two cultures, in each runtime/configuration. Frozen actual prechange write statements are the layout oracle; Core calculations are reused, not independently validated.',
        tolerance='Finite double max(1e-9, abs(expected)*1e-9), fixed before comparisons. Exact CLR kinds/nonfinite classes, labels, ordering, metadata, dimensions, write positions, report rehydration and final pointers. No sign canonicalization or geometry recalculation. Dataset digests encode CLR kinds, invariant round-trip values, shapes, source RowIds and names.',
        interpretation='CORE_RETURNED does not assert statistical validity/convergence. Existing exceptions and nonfinite results are explicit; no rejected record is a successful fit. CA-DORMANT is isolated candidate Core execution, not the unlabelled GUI path.',
        independent_R='NOT RUN / NOT AUTHORIZED', native='NOT RUN / NOT AUTHORIZED', promoted=False))
    print('New compatibility records', len(records), dict(outcomes))


def traces(names):
    result = {}
    for name in names:
        for suffix in ['-full.log', '-common-python.log']:
            path = RUN / (name + suffix)
            source = path.read_text(encoding='utf-8', errors='replace')
            for match in re.finditer(r'^(?:FAIL|ERROR): [^\n]+? \((test_[^()]+)\)([^\n]*)\n-+\n(.*?)(?=\n=+\n|\n-+\nRan |\Z)', source, re.M | re.S):
                terminal = next((line for line in reversed(match[3].splitlines()) if line.strip()), '')
                terminal = re.sub(r'[A-Za-z]:[\\/][^\r\n]+', '<local-path>', terminal)
                result.setdefault(match[1], []).append(dict(raw_alias='LOCAL_LOG:' + path.stem, raw_sha256=c.sha(path),
                    decoded_LF_traceback_sha256=hashlib.sha256(match[3].encode()).hexdigest(), terminal=terminal[:700]))
    return result


def nonpasses():
    configs = json.loads((OUT / 'validation.json').read_bytes())['configurations']
    entry_doc = json.loads((OUT / 'entry-state.json').read_bytes())['configurations']['Debug']
    accepted = json.loads((ROOT / 'evidence/04A17/nonpasses.json').read_bytes())
    accepted_map = {r['identity']: r for r in accepted['python']}
    before, after = traces(['entry-Debug', 'entry-Release']), traces(['final-Debug', 'final-Release'])
    records = []
    for identity, outcome in configs['Debug']['python']['nonpasses'].items():
        inherited = identity in entry_doc['python']['nonpasses']
        reason = accepted_map.get(identity, {}).get('root_cause')
        if identity.startswith('test_04a17_multivariate.'):
            reason = 'Frozen 04A.17 branch/source-publication pins already fail on merged main/current 04A.18 branch at entry; current exact trace also records any newly extracted form/adapter source differences. Old assertions/evidence remain unchanged.'
        if identity == 'test_04a17_multivariate.Multivariate17SourceContracts.test_public_targets_and_successor_hashes':
            reason = 'Already failed at entry: frozen CA Help after-pin ae184653793dda39310db5f1b626398cbde4d542079f39ca1bf904c52255a901 hashes CRLF bytes; merged main has LF SHA b27bec34b3bab684a6ec1f1d9e5f4df3e906efdd984a50198ce2b821da19e3ec. Converting only those entry LF bytes to CRLF reproduces the old pin. Current exact trace additionally reflects the authorized projection/Help edits. No predecessor pin changed.'
        if not inherited:
            causes = {
                'test_04a07_workflow_successor.Anova07SuccessorTests.test_actual_four_shipping_routes_and_native_bodies': 'Frozen 07 verify_sources requires the exact historical set of changed methods in the shared Ui0OneRefeditMulticol form. The additional authorized RunCA labelled extraction makes that set differ. All original ANOVA methods and every other Ui0 method are byte-equivalent under the successor method inventory; the old assertion remains FAIL.',
                'test_04a17_multivariate.Multivariate17SourceContracts.test_actual_six_handlers_delegate_and_keep_one_calculation': 'Frozen 17 expects direct MultivariateReportPolicy.Project and RenderPca/RenderMca(report,...) inside the form. PCA/MCA now use projection.ToReport and Render(projection,...); the same accepted report policy/render dispatch is consumed through the actual adapter. Successor Common and real adapter block/type/order tests pass.',
                'test_04a17_multivariate.Multivariate17SourceContracts.test_native_writer_statements_and_spm_dispatch_stay_identical': 'Frozen 17 compares literal Data-sheet set/write statements inside RunPCA/RunMCA. Those statements are now Common section traversal and the real Windows WritePcaData/WriteMcaData bridge. Frozen actual statements remain in LegacyMultivariate18Layout; functional successor comparisons prove equal effective write blocks/payloads/metadata/final pointers. SPM and all other listed form methods remain unchanged.'}
            reason = causes.get(identity, 'UNCLASSIFIED NEW FAILURE: inspect exact entry/current trace before accepting. Never relabel as passing.')
        records.append(dict(identity=identity, outcome=outcome, inherited_at_entry=inherited,
            root_cause=reason or 'Inherited branch/source pin or existing scientific/input assertion; exact current and entry trace details retained without relabelling as pass.',
            entry_raw_evidence=before.get(identity, []), final_raw_evidence=after.get(identity, []),
            terminal_details_changed_since_entry=inherited and {r['terminal'] for r in before.get(identity, [])} != {r['terminal'] for r in after.get(identity, [])}))
    managed = []
    old_managed = {r['method']: r['root_cause'] for r in accepted['managed']}
    for suite in configs['Debug']['managed_suites']:
        for row in suite['nonpasses']:
            managed.append(dict(**row, root_cause=old_managed[row['method']], inherited_at_entry=True, suite=suite['path']))
    write('nonpasses.json', dict(managed=managed, python=records, python_new_since_entry=configs['Debug']['python_new_since_entry'],
        strict=dict(exit_code=1, reason='Inherited stale UibyID/NormalPlot dependency'), profile=dict(exit_code=1, reason='Inherited INTEGRITY_FAILURE; no gate bypass'),
        original_04A17_register_sha256=c.sha(ROOT / 'evidence/04A17/nonpasses.json'),
        traceback_digest_policy='Raw SHA hashes exact private log bytes. Decoded traceback digests hash UTF-8/LF-normalized text segments; public sanitized terminals are separate derivatives. No raw-public hash equivalence.',
        no_historical_tests_or_evidence_rewritten=True))


def retention():
    previous.retention()


def commands():
    previous.commands()


if __name__ == '__main__':
    parser = argparse.ArgumentParser()
    parser.add_argument('action', choices=['entry', 'validation', 'numerical', 'nonpasses', 'retention', 'commands'])
    globals()[parser.parse_args().action]()
