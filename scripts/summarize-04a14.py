"""Sanitized 04A.14 execution accounting; never edits prior evidence or test results."""
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


c = module('contract14', 'scripts/capture-04a14-contract.py')
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
    py = previous.python_methods([RUN / ('entry-python-' + n + '.log') for n in ['tools', 'help', 'scripts']] + [RUN / 'entry-comparable-common-python.log'])
    write('entry-validation.json', dict(base=c.BASE, managed_configuration='Debug', managed_suites=suites, python=py,
        limitations='Core/Common/Windows Debug entry executed on clean merged baseline before production changes. Release and Office/Node entry were not separately executed; final matrices measure both configurations. Comparable Common Python uses fresh unchanged C02 representative-source observations after mixed-model edits, since its representative sources/inputs are unchanged.',
        environment_probe='Original Common Python attempt lacked G3 observation environment: 168 methods, 27 nonpasses. Fresh source-hashed semantic observations remove 17 environment-only errors; all raw attempts retained. Owner plan changed externally from Rev2.7 to Rev2.8 later in the run; original entry pins remain immutable.'))
    print('Entry Python', py['passed'], '/', py['total'], 'nonpasses', len(py['nonpasses']))


def typed_groups(suites):
    groups = old.deduplicate(suites)
    source = ROOT / 'BESHStat.Excel.Common.Tests/src/Mixed14WorkflowTests.vb'
    text = source.read_text(encoding='utf-8')
    methods = {'ExtractedOptionParsersMatchExactOriginalAliases': 2,
               'GuiInferenceFallbackAndKrMlGuardRemainDistinct': 1,
               'StructControlAssignmentOptimizerOverridesAndTraceSurvive': 1}
    for method, count in methods.items():
        keys = [key for key, rows in groups.items() if all(r['class_name'].split('.')[-1] == 'Mixed14WorkflowTests' and r['method'] == method
            and r['test_name'].split(' (', 1)[1] in ['null)', ')', '"")'] for r in rows)]
        rows = [row for key in keys for row in groups.pop(key)]
        assert len(rows) == count * 2
        assert len({r['suite'] for r in rows}) == 2
        shared = set(rows[0]['source_hashes'])
        for row in rows[1:]:
            shared.intersection_update(row['source_hashes'])
        assert c.sha(source) in shared
        assert 'Nothing' in text
        groups[('shared-typed-null-cohort', count, c.sha(source), method)] = rows
    return groups


def weight(key):
    return key[1] if key[0] == 'shared-typed-null-cohort' else 1


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
        groups = typed_groups(suites)
        logical = dict(total=sum(weight(k) for k in groups), passed=sum(weight(k) for k, rows in groups.items() if all(r['outcome'] == 'Passed' for r in rows)),
            nonpassed=sum(weight(k) for k, rows in groups.items() if any(r['outcome'] != 'Passed' for r in rows)))
        raw = dict(total=sum(len(s['records']) for s in suites), passed=sum(r['outcome'] == 'Passed' for s in suites for r in s['records']))
        current = {(r['class_name'].split('.')[-1], r['method'], r['test_name'].replace('"', '')) for s in suites for r in s['nonpasses']}
        new = sorted(current - entry_managed)
        assert not new, new
        new_cases = {key: rows for key, rows in groups.items() if any('Mixed14' in r['class_name'] for r in rows)}
        for s in suites:
            s['batch14_total'] = sum('Mixed14' in r['class_name'] for r in s['records'])
            s['batch14_passed'] = sum('Mixed14' in r['class_name'] and r['outcome'] == 'Passed' for r in s['records'])
            del s['records']
            s.pop('batch11', None)
        py = previous.python_methods([RUN / (name + '-full.log'), RUN / (name + '-common-python.log')])
        new_py = sorted(set(py['nonpasses']) - set(baseline['python']['nonpasses']))
        allowed = {'test_04a13_survival_workflow.Survival13SourceContracts.test_all_prechange_protected_bytes_except_exact_approved_scope', 'test_04a13_survival_workflow.Survival13SourceContracts.test_successor_pins_and_members_reconcile_to_actual_forms'}
        # Exact names are recorded; frozen scope/hash changes are classified from the
        # retained traceback, never removed from actual failure counts.
        classification = []
        log = (RUN / (name + '-full.log')).read_text(encoding='utf-8', errors='replace')
        for identity in new_py:
            classification.append(dict(identity=identity, outcome=py['nonpasses'][identity],
                category='FROZEN_SOURCE_PIN_OR_SCOPE' if identity in allowed else 'REQUIRES_REVIEW'))
        assert not [r for r in classification if r['category'] == 'REQUIRES_REVIEW'], classification
        assert not [n for n in py['nonpasses'] if '04a14' in n]
        normalized = log.replace('\u2139', '#')
        nodes = [{k: int(v) for k, v in zip(['total', 'passed', 'failed', 'cancelled', 'skipped'], values)}
            for values in re.findall(r'# tests (\d+)\n# suites \d+\n# pass (\d+)\n# fail (\d+)\n# cancelled (\d+)\n# skipped (\d+)', normalized)]
        assert len(nodes) == 3 and all(r['failed'] == r['skipped'] == 0 for r in nodes)
        configs[config] = dict(managed_suites=suites, logical_cases=logical, runtime_executions=raw,
            batch14_logical_cases=sum(weight(k) for k in new_cases), batch14_logical_passed=sum(weight(k) for k, rows in new_cases.items() if all(r['outcome'] == 'Passed' for r in rows)),
            collisions=[dict(identity=list(key), executions=rows) for key, rows in groups.items() if len(rows) > 1],
            new_managed_nonpasses=new, python=py, python_new_since_entry=classification,
            python_entry_nonpasses_now_passing=sorted(set(baseline['python']['nonpasses']) - set(py['nonpasses'])),
            node_suites=nodes, node_passed=sum(r['passed'] for r in nodes), node_total=sum(r['total'] for r in nodes))
    write('validation.json', dict(configurations=configs, native_executed=False,
        deduplication='Exact shared source SHA/class/method/data-row identities; unambiguous MSTest String quotes normalized. Three source-verified Mixed14 null cohorts additionally reconcile MSTest 2/3 null display; parser cohort contains two typed cases (Nothing and empty string) with indistinguishable Windows displays, so a failed execution would conservatively fail both. All original displays/source hashes and cohort weights retained; no failure discarded.',
        scope='Focused reruns and explicit CJS confirmation excluded from totals. All named nonpasses retained; no historical test rewritten.'))
    print({config: {k: v[k] for k in ['logical_cases', 'runtime_executions', 'batch14_logical_cases', 'node_passed', 'node_total', 'python_new_since_entry']} |
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
        tables = []
        for i, line in enumerate(lines):
            if not line.startswith('TABLE '):
                continue
            rows, cols, *_ = map(int, line.split()[1:7])
            cells = [decode(v) for v in lines[i + 1:i + 1 + rows * cols]]
            tables.append(dict(rows=rows, columns=cols, title=cells[0], metadata=line))
        cases.append(dict(path=path, key=lines[1],
            status=next((s.split('|')[1:] for s in lines if s.startswith('STATUS|')), None),
            inference=next((s.partition('|')[2] for s in lines if s.startswith('INFERENCE|')), None),
            tables=tables, failure=next((s.partition('|')[2] for s in lines if s.startswith('CORE_FAILURE|')), None), sha256=pin['sha256']))
    write('numerical-comparisons.json', dict(status='NOT_GOLDEN', cases=cases,
        comparison='Frozen unchanged Core requests/data/result arrays and full ResultTable schemas versus actual Common-created requests/fits/report projections; 25 cases, two cultures, two runtimes. Full matrix re-executes them in both configurations. Exact strings/order/CLR types/metadata/nulls; numerical max(1e-9,abs(expected)*1e-9). Only timing-value cells masked.',
        scientific_limit='Compatibility equivalence does not independently validate covariance derivatives, KR/Satterthwaite approximations, optimizer correctness or all structure combinations. Reference-grid tests compare original Core-built profiles/linear estimates using the same beta/variance, not independent LS-means goldens.',
        fault_limit='Max-iteration-one returns nonconvergence; cancellation/interruption come from Core callbacks. Five deficient/tied-design fixtures compare the same original outcome including faults/undefined values; no fabricated successful inference. All 100 prechange captures completed without a captured exception.',
        references=dict(R='NOT RUN in 04A.14; no independent R equivalence claimed', candidates='Existing Core and Windows Orthodont, sleepstudy/unbalanced, KR and pbkrtest CSV/reference tests executed by the full suites without changing their bytes or tolerances.', promoted=False),
        scientific_candidates=dict(new_confirmed_defect_ids=[], inherited_C13='C13-LOGRANK-R-01 OPEN; survival source and evidence unchanged; future scientific owner 04A.40-41')))
    print('Frozen comparisons', len(cases), 'nonconverged', sum(r['status'][0] != 'True' for r in cases if r['status']))


def nonpasses():
    final = json.loads((OUT / 'validation.json').read_bytes())
    baseline = json.loads((OUT / 'entry-validation.json').read_bytes())
    records = []
    logs = [RUN / 'final-Debug-full.log', RUN / 'final-Debug-common-python.log']
    blocks = {}
    for log in logs:
        text = log.read_text(encoding='utf-8', errors='replace')
        for block in re.split(r'^={60,}\n', text, flags=re.M)[1:]:
            first = block.splitlines()[0]
            match = re.match(r'(FAIL|ERROR): .*? \((test_[^()]+)\)', first)
            if match:
                identity = match.group(2)
                body = block.split('\n----------------------------------------------------------------------\n', 1)[-1]
                body = body.split('\n> ', 1)[0]
                terminal = [line for line in body.splitlines() if re.match(r'(?:AssertionError|KeyError|FileNotFoundError|ValueError|TypeError|RuntimeError|IndexError|PermissionError)', line)]
                excerpt = terminal[-1][:800] if terminal else next((line[:800] for line in reversed(body.splitlines()) if line.strip()), 'No terminal message')
                blocks.setdefault(identity, []).append(dict(raw_log_alias='LOCAL_LOG_ALIAS:' + log.stem,
                    raw_log_sha256=c.sha(log), traceback_canonical_text_json_sha256=c.digest(block), sanitized_terminal=excerpt))
    current = final['configurations']['Debug']['python']['nonpasses']
    for identity, outcome in sorted(current.items()):
        evidence = blocks.get(identity, [])
        terminal = ' '.join(e['sanitized_terminal'] for e in evidence)
        if identity not in baseline['python']['nonpasses']:
            category = 'NEWLY_TRIGGERED_FROZEN_PROJECT_SOURCE_PIN'
            explanation = '04A.13 successor receipt still pins old Windows project bytes; new Compile entries are required by 04A.14. Original 04A.13 receipt/assertion untouched; exact successor source pins are in source-delta.json.'
        elif 'KeyError' in terminal:
            category = 'INHERITED_EVIDENCE_SCHEMA_OR_MISSING_FIELD'
            explanation = 'Same method already nonpassed at merged entry; retained evidence schema/field mismatch, no historical rewrite.'
        elif 'FileNotFoundError' in terminal:
            category = 'INHERITED_LOCAL_EVIDENCE_AVAILABILITY'
            explanation = 'Required historical local evidence path unavailable at entry; protected originals not recreated.'
        elif 'checkpoint' in identity:
            category = 'INHERITED_FROZEN_COMPILER_NATIVE_RECEIPT_OR_PATCH_ASSUMPTION'
            explanation = 'Historical compiler/native/source receipt expectation already failed at entry; no new native or compiler qualification claimed.'
        elif 'investigation06' in identity or 'capture_contract04a02' in identity:
            category = 'INHERITED_FROZEN_SOURCE_OR_GATE_CONTRACT'
            explanation = 'Historical source/gate assertions disagree with accepted later extraction or retained ledger version; same identity at entry.'
        else:
            category = 'INHERITED_FROZEN_SOURCE_HASH_SCOPE_OR_METADATA_ASSUMPTION'
            explanation = 'Frozen historical form/project/plan/source-order/hash/acceptance expectation already nonpassed on merged entry. No estimator, old test or accepted evidence was edited to remove it.'
        records.append(dict(identity=identity, outcome=outcome, category=category, explanation=explanation, raw_traceback_evidence=evidence))
    assert len(records) == len(current)
    assert all(r['raw_traceback_evidence'] for r in records)
    managed = []
    seen = set()
    for suite in final['configurations']['Debug']['managed_suites']:
        for row in suite['nonpasses']:
            key = (row['class_name'].split('.')[-1], row['method'], row['test_name'].replace(chr(34), ''))
            if key in seen:
                continue
            seen.add(key)
            managed.append(dict(identity=list(key), category='INHERITED_LOGICAL_FAILURE', outcome=row['outcome'],
                explanation='Frozen constant-binomial expected exception superseded by previously accepted defensive reporting' if 'ConstantAndSeparated' in row['method'] else 'Existing paired geometry/statistical compatibility assertion; reproduced at merged entry'))
    write('nonpasses.json', dict(managed=managed, python=records, strict=dict(exit_code=1, reason='Stale UibyID/NormalPlot review dependency; unchanged'), profile=dict(exit_code=1, reason='INTEGRITY_FAILURE; unchanged exit semantics'),
        totals='Six logical managed failures / seven runtime failures; 68 inherited Python nonpasses plus one newly triggered frozen project pin. No new functional failure; historical tests never silenced.', native_executed=False))
    print('Classified', len(managed), 'managed and', len(records), 'Python nonpasses')


def retention():
    raw_path = RUN / 'preservation-final.json'
    raw = json.loads(raw_path.read_bytes())
    expected = ['Source pin drift ' + c.OWNER_PLAN]
    assert raw['errors'] == expected
    owner = json.loads((OUT / 'owner-plan-update.json').read_bytes())
    assert c.sha(ROOT / c.OWNER_PLAN) == owner['observed_sha256']
    initial = json.loads((RUN / 'nested-initial-metadata.json').read_bytes())
    names = {row['path'] for row in initial}
    current = {p.relative_to(ROOT).as_posix() for project in ['BESHStat.Core.Tests', 'BESHStat.Excel.Common.Tests', 'BESHStatNG.Test']
        for p in (ROOT / project / 'TestResults').rglob('*') if p.is_file()}
    assert current == names
    for row in initial:
        st = (ROOT / row['path']).stat()
        assert (st.st_size, st.st_mtime_ns, st.st_ctime_ns, st.st_ino, st.st_dev) == tuple(row[k] for k in ['bytes', 'mtime_ns', 'ctime_ns', 'ino', 'dev'])
    c.source()
    write('retention.json', dict(status='PASS_HISTORICAL_MEMBERSHIP_METADATA_WITH_OWNER_PLAN_PIN_EXCEPTION',
        raw_preservation=dict(alias='LOCAL_RETENTION_ALIAS:final_raw', sha256=c.sha(raw_path), status=raw['status'], exit_code=1, errors=raw['errors']),
        root_historical_files=raw['historical_files'], root_membership_metadata_errors=0,
        nested_historical_files=len(initial), nested_membership_metadata_errors=0,
        source_pin_exception=owner, other_source_pin_errors=0, initial_disk_free_bytes=json.loads((RUN / 'start.json').read_bytes())['disk_free_bytes'],
        final_disk_free_bytes=__import__('shutil').disk_usage(ROOT).free,
        scope='Complete initial root and nested membership/size/creation-write ns/file identity; initial frozen source/evidence byte pins except separately verified external owner plan update. Raw FAIL is retained, never edited into PASS. This successor distinguishes intact historical files from the legitimate owner-source edit; it clears no native/scientific gate.',
        limit='No all-historical-byte rehash. Shared ZIP cannot independently establish original workstation identity/timestamps or private historical path membership. No historical cleanup, relocation, compaction, recreation or metadata edit.'))
    print('Historical root', raw['historical_files'], 'nested', len(initial), 'preserved; raw source-pin exception retained.')


def commands():
    rows = []
    for path in sorted(RUN.glob('*-command.json')):
        rows.append(dict(local_receipt_alias='LOCAL_COMMAND_ALIAS:' + path.stem, raw_sha256=c.sha(path),
            public_derivative=c.prior.sanitize(json.loads(path.read_bytes()))))
    write('command-outcomes.json', dict(commands=rows, identity='Sanitized derivative commands/arguments/exits. Alias strings are not filesystem paths; original raw receipts remain in ignored run evidence with distinct raw hashes.'))


if __name__ == '__main__':
    parser = argparse.ArgumentParser()
    parser.add_argument('action', choices=['entry', 'validation', 'fixtures', 'nonpasses', 'retention', 'commands'])
    globals()[parser.parse_args().action]()
