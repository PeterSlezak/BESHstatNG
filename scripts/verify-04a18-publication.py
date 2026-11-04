"""Seal/verify the complete 04A.18 public candidate set. Never stage or publish."""
import argparse
import importlib.util
import json
from pathlib import Path
import subprocess

ROOT = Path(__file__).resolve().parents[1]
spec = importlib.util.spec_from_file_location('publication14', ROOT / 'scripts/verify-04a14-publication.py')
prior = importlib.util.module_from_spec(spec)
spec.loader.exec_module(prior)
MANIFEST = 'evidence/04A18/publication-manifest.json'
PRIVACY = 'evidence/04A18/privacy.json'
prior.MANIFEST = MANIFEST
SOURCES = {'BESHStat.Excel.Common/src/Analysis/MultivariateCorrespondenceInputPolicy.vb',
    'BESHStat.Excel.Common/src/Results/MultivariateAxisProjection.vb',
    'BESHStat.Excel.Common/src/Results/MultivariateComponentProjectionPolicy.vb',
    'BESHStat.Excel.Common.Tests/src/LegacyMultivariate18Layout.vb',
    'BESHStat.Excel.Common.Tests/src/Multivariate18Tests.vb',
    'BESHStatNG.Test/src/Multivariate18AdapterTests.vb',
    'scripts/verify-04a18.py','scripts/capture-04a18-contract.py','scripts/summarize-04a18.py',
    'scripts/verify-04a18-publication.py','scripts/tests/test_04a18_projection.py',
    'docs/architecture/batch-04A-18-owner-review.md'}


def git(*args):
    return prior.git(*args)


def paths():
    actual = set(git('diff', '--name-only').splitlines()) | set(git('ls-files', '--others', '--exclude-standard').splitlines()) | {MANIFEST, PRIVACY}
    assert all(p in SOURCES or p in CHANGED or p.startswith('evidence/04A18/') and (p.endswith('.json') or p == 'evidence/04A18/C18-CA-NOLABEL-ACTIVATION-01.UNAPPLIED.patch') for p in actual), 'Unexpected candidate path'
    return sorted(actual)


spec = importlib.util.spec_from_file_location('contract15', ROOT / 'scripts/capture-04a18-contract.py')
c = importlib.util.module_from_spec(spec)
spec.loader.exec_module(c)
CHANGED = c.CHANGED


def write(path, doc):
    (ROOT / path).write_text(json.dumps(doc, indent=2, ensure_ascii=False, allow_nan=False) + '\n', encoding='utf-8', newline='\n')


def ignored(path, no_index=False):
    args = ['git', '-c', 'safe.directory=' + ROOT.as_posix(), 'check-ignore']
    if no_index:
        args += ['--no-index']
    return subprocess.run(args + ['-q', '--', path], cwd=ROOT).returncode


def seal():
    assert git('branch', '--show-current') == 'codex/04A-18'
    assert not git('diff', '--cached', '--name-only')
    candidates = paths()
    receipt = dict(status='PASS_PUBLIC_CANDIDATE_SCAN', candidate_count=len(candidates), unresolved=0,
        scope='Every exact proposed source/test/document/evidence path and UTF-8 content; decoded JSON, escaped paths, NFKC/case variants and decodable base64; current environment owner/host identifiers, profile/drive/UNC/SID/credentials/secrets/email/internal-host patterns.',
        assurance='Bounded pattern scan plus review, not proof of absence of arbitrary secrets. Raw originals remain local; public derivatives have separate hashes.')
    write(PRIVACY, receipt)
    findings, reviewed = prior.scan(candidates)
    assert not findings, json.dumps(findings)
    receipt['reviewed_false_positive_groups'] = len(reviewed)
    write(PRIVACY, receipt)
    tracked = set(git('ls-files').splitlines())
    records = {}
    for path in candidates:
        if path == MANIFEST:
            continue
        if path in SOURCES:
            assert not any(line.rstrip(' \t') != line for line in (ROOT / path).read_text(encoding='utf-8-sig').splitlines()), path
        normal, rule = ignored(path), ignored(path, True)
        assert normal == 1 and (rule == 1 or path in tracked), path
        records[path] = dict(sha256=prior.sha(ROOT / path), bytes=(ROOT / path).stat().st_size,
            already_tracked=path in tracked, change='modified' if path in tracked else 'new',
            category='evidence' if path.startswith('evidence/') else 'documentation' if path.endswith('.md') else 'project' if path.endswith('.vbproj') else 'test' if 'Tests/' in path or '.Test/' in path or '/tests/' in path else 'source',
            role=('verification-evidence/' if path.startswith('evidence/') else 'owner-and-help-document/' if path.endswith('.md') else 'project-inclusion/' if path.endswith('.vbproj') else 'source-contract-test/' if 'Tests/' in path or '.Test/' in path or '/tests/' in path else 'implementation-and-verification/') + Path(path).stem,
            git_ignore_exit=normal, git_ignore_rule_exit=rule)
    write(MANIFEST, dict(base=c.BASE, head=git('rev-parse', 'HEAD'), branch='codex/04A-18', files=records,
        candidate_count=len(candidates), modified_paths=sorted(CHANGED & set(candidates)), new_paths=sorted(set(candidates)-tracked), deleted_paths=[], self_hash_policy='Only manifest itself excluded from hash map to avoid a cycle; its JSON, privacy, trackability and membership checked separately.',
        excluded=['All historical/ignored TestResults, raw command logs/TRX, private baseline/source maps and local receipt paths', 'Build outputs/caches/tool installations', 'Unchanged owner Master Plan; no owner edits absorbed'],
        publication='OWNER REVIEW PENDING; UNSTAGED / UNCOMMITTED / NOT PUSHED', native=False, P='NO', N1='NO GO', N2='NO', C2='UNMET'))
    verify()


def verify():
    doc = json.loads((ROOT / MANIFEST).read_bytes())
    tracked = set(git('ls-files').splitlines())
    assert set(doc['files']) | {MANIFEST} == set(paths())
    for path, record in doc['files'].items():
        assert prior.sha(ROOT / path) == record['sha256'] and (ROOT / path).stat().st_size == record['bytes'], path
        assert ignored(path) == 1 and (ignored(path, True) == 1 or path in tracked), path
    assert ignored(MANIFEST) == ignored(MANIFEST, True) == 1
    assert not git('diff', '--cached', '--name-only')
    assert subprocess.run(['git', '-c', 'safe.directory=' + ROOT.as_posix(), 'diff', '--check'], cwd=ROOT).returncode == 0
    findings, _ = prior.scan(paths())
    assert not findings, json.dumps(findings)
    verify_evidence()
    print('PASS', len(doc['files']), 'hashes/byte sizes;', doc['candidate_count'], 'candidate privacy/trackability checks; empty index; diff check.')
    print('Manifest checked separately:', prior.sha(ROOT / MANIFEST), (ROOT / MANIFEST).stat().st_size, 'bytes')


def verify_evidence():
    """Bounded version-one schema/link checks in addition to standard JSON parsing."""
    read = lambda name: json.loads((c.OUT / name).read_bytes())
    entry, final = read('entry-state.json'), read('validation.json')
    assert entry['base'] == c.BASE and entry['branch'] == 'codex/04A-18'
    for config in ['Debug', 'Release']:
        assert final['configurations'][config]['batch18_logical_total'] == final['configurations'][config]['batch18_logical_passed'] == 154
        assert not final['configurations'][config]['new_managed_nonpasses']
        assert not [p for p in final['configurations'][config]['python']['nonpasses'] if '04a18' in p]
    corpus = read('pca-ca-mca-projection-comparisons-v1.json')
    assert corpus['version'] == 1 and corpus['status'] == 'NOT_GOLDEN' and not corpus['promoted']
    assert corpus['record_count'] == len(corpus['records']) == sum(corpus['outcomes'].values()) == 916
    import hashlib
    assert corpus['payload_count'] == len(corpus['profile_payloads'])
    for key, payload in corpus['profile_payloads'].items():
        assert hashlib.sha256(json.dumps(payload, sort_keys=True, ensure_ascii=False, separators=(',', ':'), allow_nan=False).encode('utf-8')).hexdigest() == key
    for row in corpus['records']:
        assert row['configuration'] in ['Debug', 'Release'] and row['runtime'] in ['net10', 'net48']
        payload = corpus['profile_payloads'][row['profile_id']]
        assert {'family', 'case', 'culture', 'dataset_sha256', 'outcome', 'raw_rows', 'raw_columns', 'source_rows'} <= set(payload)
        assert len(payload['dataset_sha256']) == len(row['raw_trx_sha256']) == 64
    cross = read('original-control-crosswalk.json')
    assert len(cross['controls']) == 186 and len(cross['obligations']) == 152
    assert all(o['status'] == 'OPEN' for o in cross['obligations'])
    assert cross['releases'] == cross['discharges'] == 0
    for path, pin in cross['authorities'].items():
        assert prior.sha(ROOT / path) == pin['sha256'] and (ROOT / path).stat().st_size == pin['bytes']
    assert len(read('frozen-capture-hashes.json')['captures']) == 100
    disposition = read('c17-ca-nolabel-disposition.json')
    assert 'OPEN' in disposition['status'] and not disposition['patch']['applied']
    assert prior.sha(ROOT / disposition['source']['path']) == disposition['source']['source_sha256']
    assert prior.sha(ROOT / disposition['patch']['path']) == disposition['patch']['sha256']
    assert read('source-preservation.json')['unlabelled_branch_sha256'] == disposition['source']['original_unchanged_branch_sha256']
    for path, pin in read('documentation-audit.json').items():
        assert prior.sha(ROOT / path) == pin['sha256']
    retained = read('retention.json')
    assert retained['membership_metadata_errors'] == 0 and retained['root_files'] == 213539 and retained['nested_files'] == 139
    for d in [cross, read('frozen-capture-hashes.json')]:
        assert (d['P'], d['N1'], d['N2'], d['C2']) == ('NO', 'NO GO', 'NO', 'UNMET')
    assert read('privacy.json')['unresolved'] == 0


if __name__ == '__main__':
    p = argparse.ArgumentParser()
    p.add_argument('action', choices=['seal', 'verify'])
    globals()[p.parse_args().action]()
