"""Seal/verify the complete 04A.16 public candidate set. Never stage or publish."""
import argparse
import importlib.util
import json
from pathlib import Path
import subprocess

ROOT = Path(__file__).resolve().parents[1]
spec = importlib.util.spec_from_file_location('publication14', ROOT / 'scripts/verify-04a14-publication.py')
prior = importlib.util.module_from_spec(spec)
spec.loader.exec_module(prior)
MANIFEST = 'evidence/04A16/publication-manifest.json'
PRIVACY = 'evidence/04A16/privacy.json'
prior.MANIFEST = MANIFEST
SOURCES = {'BESHStat.Excel.Common/src/Results/MixedModelDiagnosticPolicy.vb','BESHStat.Excel.Common/src/Results/MixedModelPostEstimationPolicy.vb',
    'BESHStat.Excel.Common.Tests/src/Mixed16PostEstimationTests.vb', 'BESHStat.Excel.Common.Tests/src/LegacyMixed16Selection.vb',
    'BESHStatNG.Test/src/Mixed16AdapterTests.vb', 'scripts/verify-04a16.py', 'scripts/capture-04a16-contract.py',
    'scripts/summarize-04a16.py', 'scripts/verify-04a16-publication.py', 'scripts/tests/test_04a16_post_estimation.py',
    'docs/architecture/batch-04A-16-owner-review.md'}


def git(*args):
    return prior.git(*args)


def paths():
    actual = set(git('diff', '--name-only').splitlines()) | set(git('ls-files', '--others', '--exclude-standard').splitlines()) | {MANIFEST, PRIVACY}
    assert all(p in SOURCES or p in CHANGED or p.startswith('evidence/04A16/') and p.endswith('.json') for p in actual), 'Unexpected candidate path'
    return sorted(actual)


spec = importlib.util.spec_from_file_location('contract15', ROOT / 'scripts/capture-04a16-contract.py')
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
    assert git('branch', '--show-current') == 'codex/04A-16'
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
            already_tracked=path in tracked, git_ignore_exit=normal, git_ignore_rule_exit=rule)
    write(MANIFEST, dict(base=c.BASE, head=git('rev-parse', 'HEAD'), branch='codex/04A-16', files=records,
        candidate_count=len(candidates), self_hash_policy='Only manifest itself excluded from hash map to avoid a cycle; its JSON, privacy, trackability and membership checked separately.',
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
    print('PASS', len(doc['files']), 'hashes/byte sizes;', doc['candidate_count'], 'candidate privacy/trackability checks; empty index; diff check.')
    print('Manifest checked separately:', prior.sha(ROOT / MANIFEST), (ROOT / MANIFEST).stat().st_size, 'bytes')


if __name__ == '__main__':
    p = argparse.ArgumentParser()
    p.add_argument('action', choices=['seal', 'verify'])
    globals()[p.parse_args().action]()
