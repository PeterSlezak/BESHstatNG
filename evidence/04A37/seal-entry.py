"""Seal the independently executed merged-main entry, before production edits."""
from pathlib import Path
from collections import Counter, defaultdict
from datetime import datetime, timezone
import csv
import hashlib
import json
import re
import subprocess

ROOT = Path(__file__).resolve().parents[2]
HERE = Path(__file__).resolve().parent
RUN = ROOT / '.test-rationalization-04A37.tmp'


def digest(b):
    return hashlib.sha256(b).hexdigest()


def pin(p):
    b = (ROOT / p).read_bytes()
    return dict(path=p, bytes=len(b), sha256=digest(b))


def index(rows):
    grouped = defaultdict(list)
    for row in rows:
        grouped[digest(json.dumps((row['class_name'], row['method'], row['test_name'])).encode())].append(row)
    return {(k, i): r for k, values in grouped.items() for i, r in enumerate(sorted(values, key=lambda x: x['outcome']))}


def save(name, value):
    with (HERE / name).open('x', encoding='utf-8', newline='\n') as f:
        json.dump(value, f, ensure_ascii=False, indent=2)
        f.write('\n')


def main():
    git = lambda *a: subprocess.check_output(['git', '-c', 'safe.directory=' + ROOT.as_posix(), *a], cwd=ROOT)
    assert not git('diff', '--name-only') and not git('diff', '--cached', '--name-only')
    assert git('rev-parse', 'HEAD') == git('rev-parse', 'main')
    accepted = list(csv.DictReader((ROOT / 'evidence/04A36/managed-test-identities.csv').open(encoding='utf-8-sig')))
    totals = {}; suites = []; differences = []
    for config in ['Debug', 'Release']:
        counts = Counter()
        for suite in ['Core', 'Common', 'Legacy', 'ControlMitigation', 'OfficeAddin', 'OfficeExecution']:
            result = json.loads((RUN / f'executions/entry-{config}-serial-{suite}/cases.json').read_bytes())
            current = index(result['records'])
            prior = {(r['raw_identity_sha256'], int(r['raw_occurrence_ordinal'])): r for r in accepted if r['suite'] == suite}
            assert set(prior) == set(current), (suite, config, 'identity drift')
            changed = [k for k in prior if prior[k]['final_' + config.lower()] != current[k]['outcome']]
            assert not changed, (suite, config, changed)
            counts.update(result['outcomes'])
            suites.append(dict(configuration=config, suite=suite, outcomes=result['outcomes'], trx_sha256=result['sha256'],
                               removed=0, added=0, changed_outcomes=0, multiplicity='Exact accepted36 raw identity and occurrence multiset'))
        totals[config] = dict(counts)
        assert counts == Counter(Passed=20457, Failed=14)
    node = []
    for suite in ['office-node', 'resolver', 'wasm-node']:
        p = RUN / ('commands/entry-' + suite)
        t = (p / 'stdout.log').read_text(encoding='utf-8-sig')
        n = lambda k: int(re.findall(r'(?:# |\u2139 )' + k + r' (\d+)', t)[-1])
        node.append(dict(suite=suite, total=n('tests'), passed=n('pass'), failed=n('fail'),
                         stdout_sha256=digest((p / 'stdout.log').read_bytes())))
    assert sum(r['passed'] for r in node) == 916
    audits = []
    for batch in ['34', '35', '36']:
        p = RUN / ('commands/entry-accepted' + batch + '-audits')
        t = (p / 'stderr.log').read_text(encoding='utf-8-sig')
        audits.append(dict(batch=batch, exit=json.loads((p / 'command.json').read_bytes())['exit_code'],
                           summary=t[t.rfind('Ran '):].strip(), stdout_sha256=digest((p / 'stdout.log').read_bytes()),
                           stderr_sha256=digest((p / 'stderr.log').read_bytes()),
                           qualification='Raw frozen historical readers require pre-Git byte/EOL pins. Retained failures; independent exact inverse-EOL checks preserve accepted bytes, no historical reader rewritten.'))
    save('entry-baseline.json', dict(source_commit=git('rev-parse', 'HEAD').decode().strip(),
         managed=totals, suites=suites, node=node, prior_packet_audits=audits,
         python=dict(passed=809, failed=137, errors=10, total=956,
             receipts=['entry-source-validation.json', 'entry-source-validation-v2.json'],
             qualification='First attempt 807/138/11 includes two new inventory privacy defects, corrected before production. V2 scripts 609/131/5 plus tools27/help16/Common157/6/5. All first-attempt receipts retained privately.',
             accepted36_delta='Old Core-directory dirty-scope assertion now passes after owner documentation commit. Historical unordered SPC source lookup changes ERROR to FAIL in the same test identity; assertion remains nonpassing. No historical tests edited.'),
         strict_profile_mkdocs=dict(strict=1, profile=1, mkdocs=0),
         restrictions='P=NO; N1=NO GO; N2=NO; C2=UNMET; C3=UNAPPROVED; NOT_GOLDEN',
         wrappers='NOT_RUN: original shared-output wrapper; Legacy Build includes unapproved packaging hooks. Accepted compile-only isolated equivalent ran in both configurations. No package install or network restore.'))
    files = [pin(p.relative_to(ROOT).as_posix()) for p in (HERE / 'preedit').rglob('*') if p.is_file()]
    files += [pin('evidence/04A37/' + p) for p in ['freeze.py', 'capture.ps1', 'capture-series.ps1', 'Capture37.cs', 'CaptureSeries37.cs']]
    save('preedit/receipt.json', dict(source_commit=git('rev-parse', 'HEAD').decode().strip(),
         sealed_utc=datetime.now(timezone.utc).isoformat(), production_diff='EMPTY',
         capture_rows_per_configuration=5376, supplemental_dendrogram_rows=1536,
         valid_captures_never_overwritten=True, files=files,
         policy='Exact UTF-8 strings and ordering; Double IEEE bits including signed zero and NaN payload; exact array element type/rank/lower bound/length; exception type, parameter, message and recorded original phase. No tolerance or culture normalization.',
         limitations='Pure source slices and extracted original expressions, not native execution; raw/fitted input factories retained. Separate native creation/marshalling/visual behavior is SOURCE_ONLY_NATIVE_NOT_RUN.',
         corrections=['Reserved VB identifier and original helper bindings corrected before first valid capture; invalid sources retained in attempts.',
                      'Binding correction adds DataManagement import and exact original AxisPadding body because its Core container is Friend; no numerical substitution.',
                      'Generator construction and first captures overlapped baseline execution, all sealed after completed baseline and before production edits. First valid outputs retained.'],
         scientific='NOT_GOLDEN'))
    print('SEALED merged-main baseline; 20457/14 each configuration; 916 Node; captures 5376/config +1536; production unchanged')


if __name__ == '__main__':
    main()
