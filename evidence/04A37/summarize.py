"""Summarize actual isolated receipts; preserve raw identity occurrence accounting."""
from pathlib import Path
from collections import Counter, defaultdict
import csv
import hashlib
import importlib.util
import json
import os
import re
import sys

ROOT = Path(__file__).resolve().parents[2]
HERE = Path(__file__).resolve().parent
RUN = ROOT / '.test-rationalization-04A37.tmp'
SUITES = ['Core', 'Common', 'Legacy', 'ControlMitigation', 'OfficeAddin', 'OfficeExecution']


def sha(b):
    return hashlib.sha256(b).hexdigest()


def load(p):
    return json.loads(p.read_bytes())


def clean(value):
    if isinstance(value, list): return [clean(v) for v in value]
    if isinstance(value, dict): return {k: clean(v) for k, v in value.items()}
    if not isinstance(value, str): return value
    for a, b in [(str(RUN), '<ISOLATED_RUN>'), (RUN.as_posix(), '<ISOLATED_RUN>'), (str(ROOT), '<REPO_ROOT>'), (ROOT.as_posix(), '<REPO_ROOT>'), (sys.executable, '<PYTHON>')]:
        value = value.replace(a, b)
    value = re.sub(re.escape(os.environ.get('COMPUTERNAME', '!unset!')), '<HOST>', value, flags=re.I)
    value = re.sub(r'[A-Za-z]:[\\/]+Program Files(?: \(x86\))?[\\/]+', '<PROGRAM_FILES>/', value, flags=re.I)
    value = re.sub(r'[A-Za-z]:[\\/]+Users[\\/]+[^\\/\s"\']+', '<PRIVATE_HOME>', value, flags=re.I)
    return re.sub(r'S-1-5-\d+(?:-\d+)*', '<PRIVATE_SID>', value)


def display(value):
    if re.search(r'\b[A-Za-z]:[\\/]|[A-Za-z0-9._%+-]+@[A-Za-z0-9.-]+\.[A-Za-z]{2,}', value):
        return '<REDACTED_TEST_DISPLAY:' + sha(value.encode())[:24] + '>'
    return clean(value)


def save(name, value):
    (HERE / name).write_text(json.dumps(clean(value), ensure_ascii=False, indent=2) + '\n', encoding='utf-8', newline='\n')


def index(rows):
    grouped = defaultdict(list)
    for r in rows:
        grouped[sha(json.dumps((r['class_name'], r['method'], r['test_name'])).encode())].append(r)
    return {(key, i): r for key, values in grouped.items() for i, r in enumerate(sorted(values, key=lambda r: r['outcome']))}


def python_receipt(label, suffix):
    p = RUN / ('commands/' + label + '-' + suffix)
    raw = (p / 'stderr.log').read_bytes()
    try: text = raw.decode('utf-8-sig'); encoding = 'utf-8-sig'
    except UnicodeDecodeError: text = raw.decode('cp1252'); encoding = 'cp1252 log only; fixture strings unchanged'
    count = lambda k: int(re.search(k + r'=(\d+)', text).group(1)) if re.search(k + r'=(\d+)', text) else 0
    total = int(re.search(r'Ran (\d+) tests?', text).group(1)); failures = count('failures'); errors = count('errors')
    return dict(label=label, suite=suffix, total=total, passed=total-failures-errors, failed=failures, errors=errors,
        exit_code=load(p / 'command.json')['exit_code'], stderr_sha256=sha(raw), log_decoding=encoding,
        identities=re.findall(r'^(test_\S+ \([^)]*\))', text, re.M),
        nonpasses=[dict(outcome=k, identity=i, subtest=display(s.strip())) for k, i, s in re.findall(r'^(FAIL|ERROR): (test_\S+ \([^)]*\))([^\n]*)', text, re.M)],
        failure_receipts=[dict(identity=m.group(1), subtest=display(m.group(2).strip()), traceback=clean(block[:18000]))
            for block in re.split(r'(?=^(?:FAIL|ERROR): test_)', text, flags=re.M)
            if (m := re.match(r'(?:FAIL|ERROR): (test_\S+ \([^)]*\))([^\n]*)', block))])


def main():
    identities = []; nonpasses = []; suite_results = []; deltas = []; totals = defaultdict(Counter)
    for suite in SUITES:
        results = {}; before = {}
        for config in ['Debug', 'Release']:
            b = load(RUN / f'executions/entry-{config}-serial-{suite}/cases.json')
            a = load(RUN / f'executions/final-{config}-serial-{suite}/cases.json')
            bi = index(b['records']); ai = index(a['records']); before[config] = bi; results[config] = ai
            assert set(bi) <= set(ai), ('Removed historical cases', suite, config)
            assert all(bi[k]['outcome'] == ai[k]['outcome'] for k in bi), ('Changed historical outcomes', suite, config)
            added = set(ai) - set(bi)
            assert all(ai[k]['outcome'] == 'Passed' and 'Graphics37' in ai[k]['class_name'] for k in added)
            totals[config].update(a['outcomes'])
            deltas.append(dict(suite=suite, configuration=config, removed=0, changed_outcomes=0, added=len(added)))
            suite_results.append(dict(suite=suite, configuration=config, entry=b['outcomes'], final=a['outcomes'], entry_trx_sha256=b['sha256'], final_trx_sha256=a['sha256'], exit=a['command']['exit_code']))
            for row in a['records']:
                if row['outcome'] != 'Passed':
                    nonpasses.append(dict(configuration=config, suite=suite, class_name=row['class_name'], method=row['method'], test_name=display(row['test_name']), outcome=row['outcome'], message_sha256=sha(row['message'].encode()), classification='INHERITED_EXACT_IDENTITY_AND_OCCURRENCE_OUTCOME'))
        assert set(results['Debug']) == set(results['Release'])
        for key, row in sorted(results['Debug'].items()):
            assert row['outcome'] == results['Release'][key]['outcome']
            identities.append(dict(suite=suite, class_name=row['class_name'], method=row['method'], test_name=display(row['test_name']), raw_identity_sha256=key[0], raw_occurrence_ordinal=key[1],
                merged_main_debug=before['Debug'][key]['outcome'] if key in before['Debug'] else 'NEW_04A37', merged_main_release=before['Release'][key]['outcome'] if key in before['Release'] else 'NEW_04A37',
                final_debug=row['outcome'], final_release=results['Release'][key]['outcome']))
    for config in ['Debug', 'Release']:
        assert totals[config] == Counter(Passed=20507, Failed=14)
        assert sum(r['added'] for r in deltas if r['configuration'] == config) == 50
    with (HERE / 'managed-test-identities.csv').open('w', encoding='utf-8', newline='') as f:
        w = csv.DictWriter(f, fieldnames=list(identities[0])); w.writeheader(); w.writerows(identities)
    python = []; new_nonpasses = []; removed_nonpasses = []
    for suffix in ['python-tools', 'python-help', 'python-scripts', 'common-python']:
        b = python_receipt('entry-privacy-corrected' if suffix == 'python-scripts' else 'entry', suffix)
        a = python_receipt('final', suffix)
        assert b['total'] == a['total'] and Counter(b['identities']) == Counter(a['identities'])
        key = lambda r: Counter((v['outcome'], v['identity'], v['subtest']) for v in r['nonpasses'])
        for k, n in (key(a)-key(b)).items():
            new_nonpasses.append(dict(outcome=k[0], identity=k[1], subtest=k[2], occurrences=n, classification='REQUIRES_EXPLICIT_SOURCE_TRANSITION_REVIEW', traceback=next((v['traceback'] for v in a['failure_receipts'] if v['identity'] == k[1] and v['subtest'] == k[2]), '')))
        removed_nonpasses.extend(dict(outcome=k[0], identity=k[1], subtest=k[2], occurrences=n) for k, n in (key(b)-key(a)).items())
        a['baseline'] = {k: b[k] for k in ['total', 'passed', 'failed', 'errors', 'exit_code', 'stderr_sha256']}
        a['baseline_nonpasses'] = b['nonpasses']; python.append(a)
    node = []
    for suffix in ['office-node', 'resolver', 'wasm-node']:
        rows = []
        for label in ['entry', 'final']:
            p = RUN / ('commands/' + label + '-' + suffix); raw = (p / 'stdout.log').read_bytes(); text = raw.decode('utf-8-sig')
            count = lambda k: int(re.findall(r'(?:# |\u2139 )' + k + r' (\d+)', text)[-1])
            ids = re.findall(r'^[\u2714\u2716] (.*?) \([\d.]+ms\)', text, re.M)
            rows.append(dict(label=label, suite=suffix, total=count('tests'), passed=count('pass'), failed=count('fail'), exit_code=load(p / 'command.json')['exit_code'], identity_multiset_sha256=sha(json.dumps(sorted(ids)).encode()), stdout_sha256=sha(raw)))
        assert rows[0]['identity_multiset_sha256'] == rows[1]['identity_multiset_sha256']
        node.extend(rows)
    save('source-and-node-validation.json', dict(python=python, node=node, new_nonpasses=new_nonpasses, removed_nonpasses=removed_nonpasses))
    save('nonpasses.json', dict(managed=nonpasses, python_new=new_nonpasses, python_removed=removed_nonpasses))
    attempts = []
    for p in (RUN / 'executions').glob('*/cases.json'):
        if p.parent.name.startswith(('entry-', 'final-')): continue
        r = load(p); attempts.append(dict(label=p.parent.name, suite=r['suite'], configuration=r['configuration'], outcomes=r['outcomes'], exit=r['command']['exit_code'], trx_sha256=r['sha256'], nonpasses=[dict(class_name=v['class_name'], method=v['method'], outcome=v['outcome'], message=clean(v['message'])) for v in r['records'] if v['outcome'] != 'Passed']))
    save('development-attempts.json', dict(runs=attempts, qualifications=['Invalid pre-edit compiler sources retained under preedit/attempts; no valid capture was overwritten.', 'Early project-link insertion and new helper naming errors corrected; raw failed commands retained.', 'First post capture had an outer container type and implicit-cp1252 Unicode binding defect; development-v1 remains immutable. Frozen references unchanged.', 'Modern negative-zero caption discrepancy corrected in portable text only. Coordinate bits unchanged.']))
    save('command-receipts.json', dict(commands=[dict(name=p.parent.name, **clean(load(p))) for p in sorted((RUN / 'commands').glob('*/command.json'))], direct_capture_commands=[
        dict(command=['powershell','-NoProfile','-ExecutionPolicy','Bypass','-File','evidence/04A37/capture.ps1','-Configuration', c],exit_code=0,output='preedit/capture-'+c+'.tsv') for c in ['Debug','Release']] + [
        dict(command=['powershell','-NoProfile','-ExecutionPolicy','Bypass','-File','evidence/04A37/capture-post.ps1','-Label','final','-Configuration',c],exit_code=0,output='postedit/capture-'+c+'.tsv') for c in ['Debug','Release']],
        qualification='Actual runner commands include raw output hashes, failures and attempts. Direct standalone captures have tool execution exits and hashed output files, not invented raw-console hashes. No native Excel/R/packaging executed.'))
    serial = {suffix: load(RUN / ('commands/final-' + suffix + '/command.json'))['exit_code'] for suffix in ['strict','profile','docs']}
    save('validation.json', dict(status='VALIDATION_EXECUTED_PENDING_PACKET_AUDIT', managed={c:dict(v) for c,v in totals.items()}, suites=suite_results, identity_deltas=deltas,
        new_managed_cases_per_configuration=50, new_managed_by_suite=dict(Core=11,Common=8,Legacy=31), node=dict(total=916,passed=916,failed=0),
        python={k:sum(r[k] for r in python) for k in ['total','passed','failed','errors']}, strict_profile_mkdocs=serial,
        unclassified_python=len(new_nonpasses), native='NOT_RUN', scientific='NOT_GOLDEN', next_batch='04A38_NOT_STARTED'))
    print('Managed:',dict(totals['Debug']),dict(totals['Release']),'; Python:',{k:sum(r[k] for r in python) for k in ['passed','failed','errors']},'; new source nonpasses:',len(new_nonpasses))


if __name__ == '__main__':
    main()
