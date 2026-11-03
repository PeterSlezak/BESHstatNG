"""04A.11 publication successor. Never regenerate historical raw receipts in place.

Run with the established local Python: backup (private originals first), sanitize,
refresh (manifest DAG), verify. No product code or historical TestResults writes.
Private originals, mapping and initial inventory live outside this repository.
"""
import argparse
import base64
import hashlib
import json
import os
from pathlib import Path
import re
import shutil
import subprocess
import uuid
import unicodedata

ROOT = Path(__file__).resolve().parents[1]
OUT = 'evidence/04A11/'
EXPECTED = {
    'prechange-manifest.json': '718194351f8744a346de084d07ec968536f041de7ae54973a3fcc47072df1404',
    'source-delta.json': 'c9b928dc7f709e06c4e5e46c0d1f0f1eff7ca967dcaafa0adfa95a1f367828b4',
    'source-delta-v2.json': 'd0c4fcc8546b098a85c0f2583185ee842b25fa1ce1ce7dca52e684973ab18169',
    'source-delta-final.json': '5eb53e69b926a4663285f3d142eef133793c005a92d6be466ad70a16f4ba8b10',
}
PLACEHOLDER = 'REDACTED_OWNER_HOST'
DATED = re.compile(r'^(.+?)([ _]20\d{2}[-_]\d{2}[-_]\d{2}.*)$')
FINAL = OUT + 'publication-manifest-sanitized.json'
NOTE = 'docs/architecture/batch-04A-11-public-evidence-sanitization-owner-review.md'
ADDITIONS = [FINAL, NOTE, 'scripts/verify-04a11-publication.py',
             'scripts/tests/test_04a11_publication.py', OUT + 'public-sanitization.json',
             OUT + 'publication-privacy.json', OUT + 'publication-validation.json']
EDITABLE = {OUT + n for n in EXPECTED} | {
    OUT + 'publication-manifest.json', OUT + 'publication-manifest-final.json', OUT + 'README.md'}


def git(*args):
    return subprocess.check_output(['git', '-c', 'safe.directory=' + ROOT.as_posix(), *args],
                                   cwd=ROOT, text=True).strip()


def sha(data):
    return hashlib.sha256(data).hexdigest()


def encoded(obj):
    return (json.dumps(obj, indent=2, ensure_ascii=False) + '\n').encode('utf-8')


def load(path):
    return json.loads(Path(path).read_bytes())


def emit(path, obj):
    Path(path).write_bytes(encoded(obj))


def private_root():
    return Path(os.environ['LOCALAPPDATA']) / 'BESHStatNG' / '04A11-private-evidence'


def vault():
    # Sandboxed and owner processes can have different LOCALAPPDATA values.
    roots = {private_root()}
    for ancestor in ROOT.parents:
        if ancestor.parent.name.casefold() == 'users':
            roots.add(ancestor / 'AppData' / 'Local' / 'BESHStatNG' / '04A11-private-evidence')
    found = [p.parent for root in roots for p in root.glob('sanitization-*/private-receipt.json')
             if load(p).get('repository') == str(ROOT)]
    assert len(found) == 1, 'Require exactly one verified private receipt for this checkout'
    return found[0]


def candidates(v):
    return sorted(set(load(v / 'private-receipt.json')['initial_candidates']) | set(ADDITIONS))


def project(raw):
    """Only rename dated historical dictionary keys; preserve every JSON value."""
    doc = json.loads(raw)
    field = 'source_pins' if 'source_pins' in doc else 'protected_source_pins'
    old = doc[field]
    pairs, identifiers, new = {}, set(), {}
    for key in old:
        if '/testresults/' in key.lower():
            for part in key.split('/'):
                match = DATED.fullmatch(part)
                if match and match[1] != PLACEHOLDER:
                    identifiers.add(match[1])
    assert len(identifiers) == 1
    identity_parts = next(iter(identifiers)).split('_')
    for key, value in old.items():
        parts = key.split('/')
        if '/testresults/' in key.lower():
            for i, part in enumerate(parts):
                match = DATED.fullmatch(part)
                if match:
                    parts[i] = PLACEHOLDER + match[2]
                elif part in identifiers or (len(part.split('_')) >= 2 and
                        all(segment in identity_parts for segment in part.split('_'))):
                    parts[i] = PLACEHOLDER
        alias = '/'.join(parts)
        assert alias not in new, 'Redaction collision'
        new[alias] = value
        if alias != key:
            pairs[key] = alias
    assert len(pairs) == 70 and len(identifiers) == 1
    assert len(set(pairs.values())) == 70
    doc[field] = new
    reverse = {alias: key for key, alias in pairs.items()}
    restored = dict(doc)
    restored[field] = {reverse.get(k, k): val for k, val in new.items()}
    assert restored == json.loads(raw), 'Non-sensitive structure/value changed'
    assert encoded(restored) == encoded(json.loads(raw)), 'JSON field/dictionary order changed'
    return encoded(doc), pairs, identifiers


def backup():
    assert git('branch', '--show-current') == 'codex/04A-11'
    assert git('rev-parse', 'HEAD') == git('rev-parse', 'main')
    assert not git('diff', '--cached', '--name-only')
    assert not list(private_root().glob('sanitization-*/private-receipt.json')), 'Inspect existing backup first'
    manifest = load(ROOT / (OUT + 'publication-manifest-final.json'))
    paths = sorted(set(manifest['files']) | {OUT + 'publication-manifest-final.json'})
    assert len(paths) == 75
    for name, expected in EXPECTED.items():
        assert sha((ROOT / OUT / name).read_bytes()) == expected
    v = private_root() / ('sanitization-' + uuid.uuid4().hex)
    assert not v.resolve().is_relative_to(ROOT.resolve())
    v.mkdir(parents=True, exist_ok=False)
    # Restrict this newly-created vault only. Retain owner, SYSTEM and administrators.
    sid = subprocess.check_output(['powershell', '-NoProfile', '-Command',
        '[System.Security.Principal.WindowsIdentity]::GetCurrent().User.Value'], text=True).strip()
    acl = subprocess.run(['icacls', str(v), '/inheritance:r', '/grant:r',
                         '*' + sid + ':(OI)(CI)F', '*S-1-5-18:(OI)(CI)F',
                         '*S-1-5-32-544:(OI)(CI)F'], capture_output=True)
    assert acl.returncode == 0, 'Private vault ACL protection failed'
    receipt = dict(repository=str(ROOT), head=git('rev-parse', 'HEAD'),
                   initial_status=git('status', '--porcelain=v1', '--untracked-files=all'),
                   initial_candidates=paths, initial_hashes={}, raw_files={}, acl_protected=True)
    for path in paths:
        src, dest = ROOT / path, v / 'originals' / path
        dest.parent.mkdir(parents=True, exist_ok=True)
        shutil.copyfile(src, dest)
        assert src.read_bytes() == dest.read_bytes()
        receipt['initial_hashes'][path] = sha(dest.read_bytes())
    receipt['gitignore_sha256'] = sha((ROOT / '.gitignore').read_bytes())
    for name, expected in EXPECTED.items():
        path = OUT + name
        raw = (v / 'originals' / path).read_bytes()
        assert sha(raw) == expected
        public, mapping, _ = project(raw)
        planned = v / 'planned-public' / name
        planned.parent.mkdir(exist_ok=True)
        planned.write_bytes(public)
        receipt['raw_files'][path] = dict(raw_sha256=expected, raw_bytes=len(raw),
            public_sha256=sha(public), public_bytes=len(public), mapping=mapping)
    emit(v / 'private-receipt.json', receipt)
    print('Private ACL-protected backup: 75 original candidates; four expected raw hashes verified.')


def sanitize():
    v = vault()
    receipt = load(v / 'private-receipt.json')
    records = {}
    for path, record in receipt['raw_files'].items():
        raw = (v / 'originals' / path).read_bytes()
        assert sha(raw) == record['raw_sha256']
        public, mapping, _ = project(raw)
        assert mapping == record['mapping'] and sha(public) == record['public_sha256']
        assert sha((ROOT / path).read_bytes()) in [record['raw_sha256'], record['public_sha256'], record.get('initial_plan_sha256')] + record.get('prior_public_hashes', [])
        (ROOT / path).write_bytes(public)
        records[path] = {k: record[k] for k in ['raw_sha256', 'raw_bytes', 'public_sha256', 'public_bytes']}
        records[path]['redacted_keys'] = len(mapping)
        records[path]['redacted_path_components'] = sum(
            a != b for key, alias in mapping.items() for a, b in zip(key.split('/'), alias.split('/')))
    emit(ROOT / OUT / 'public-sanitization.json', dict(
        status='PUBLIC_DERIVATIVES; private originals authoritative for real local filenames',
        date='2026-11-03', files=records, placeholder=PLACEHOLDER,
        policy='Replace only owner/host components of historical TestResults dictionary keys, including dated prefixes and repeated deployment subfolders; retain suffixes and all source SHA-256 values. Aliases are display identities, not real filenames.',
        verification='Private reverse mapping restores original JSON structure exactly; 70 unique mappings in each file; no added source-document schema keys.',
        private_preservation='Byte-exact originals, original publication manifests and private mapping retained in an ACL-protected owner-local vault outside repository and TestResults.',
        manifests='Original 72/74-entry manifests privately retained; public revisions refresh their subset hashes. publication-manifest-sanitized.json is authoritative for the complete current publication set; itself omitted from its hash map.'))
    print('Four public derivatives written; 70 unique keys each; all values and other subtrees unchanged.')


def replan():
    """Privately retain first plan and cover repeated deployment-folder aliases."""
    v = vault()
    receipt = load(v / 'private-receipt.json')
    revision = receipt.get('plan_revision', 1)
    saved = v / ('private-receipt-plan-' + str(revision) + '.json')
    assert not saved.exists()
    shutil.copyfile(v / 'private-receipt.json', saved)
    for path, record in receipt['raw_files'].items():
        raw = (v / 'originals' / path).read_bytes()
        assert sha(raw) == record['raw_sha256']
        public, mapping, _ = project(raw)
        record.setdefault('prior_public_hashes', []).append(record['public_sha256'])
        record['initial_plan_sha256'] = record['public_sha256']
        record.update(public_sha256=sha(public), public_bytes=len(public), mapping=mapping)
    receipt['plan_revision'] = revision + 1
    emit(v / 'private-receipt.json', receipt)
    print('Private mapping revision retained; raw original bytes unchanged.')


def inventory(v):
    tracked = set(git('ls-files').splitlines())
    result = {}
    for p in candidates(v):
        if p == FINAL:
            continue
        prefix = ['git', '-c', 'safe.directory=' + ROOT.as_posix(), 'check-ignore']
        normal = subprocess.run(prefix + ['-q', '--', p], cwd=ROOT).returncode
        rule = subprocess.run(prefix + ['--no-index', '-q', '--', p], cwd=ROOT).returncode
        assert normal == 1 and (rule == 1 or p in tracked), 'Not trackable: ' + p
        result[p] = dict(sha256=sha((ROOT / p).read_bytes()), bytes=(ROOT / p).stat().st_size,
                         git_ignore_exit=normal, git_ignore_rule_exit=rule, git_tracked=p in tracked)
    return result


def refresh():
    v = vault()
    current = inventory(v)
    for name in ['publication-manifest.json', 'publication-manifest-final.json']:
        path = OUT + name
        doc = load(v / 'originals' / path)
        for p in doc['files']:
            doc['files'][p] = dict(current[p], sha256=sha((ROOT / p).read_bytes()), bytes=(ROOT / p).stat().st_size)
        doc['public_revision'] = dict(status='SANITIZED_HISTORICAL_SUBSET', date='2026-11-03',
            original_manifest_sha256=sha((v / 'originals' / path).read_bytes()),
            authoritative_current_manifest='publication-manifest-sanitized.json',
            privacy_receipt='publication-privacy.json', historical_privacy_scan='Original limited scan is not current publication assurance')
        emit(ROOT / path, doc)
    files = inventory(v)
    assert all(r['git_ignore_exit'] == 1 for r in files.values())
    emit(ROOT / FINAL, dict(status='AUTHORITATIVE_PUBLIC_SANITIZATION_SUCCESSOR',
         base=git('rev-parse', 'main'), head=git('rev-parse', 'HEAD'), branch=git('branch', '--show-current'),
         files=files, self_hash_policy='This manifest is included in candidate/privacy/trackability scope but omitted from its own hash map; no circular hashes.',
         candidate_count=len(files) + 1, index_empty=not git('diff', '--cached', '--name-only'),
         exclusions=['Private vault and reverse mapping', 'Ignored raw run records and baseline.json',
                     'Historical TestResults and compiled outputs', 'Caches and unrelated local files'],
         privacy_receipt='publication-privacy.json', validation_receipt='publication-validation.json',
         publication='NOT STAGED / NOT COMMITTED / NOT PUBLISHED; owner publication review only'))
    print('Publication manifest DAG refreshed:', len(files), 'external hashes; self omitted.')


def verify():
    v = vault()
    receipt = load(v / 'private-receipt.json')
    assert git('branch', '--show-current') == 'codex/04A-11'
    assert git('rev-parse', 'HEAD') == git('rev-parse', 'main') == receipt['head']
    assert not git('diff', '--cached', '--name-only')
    changed = set(git('diff', '--name-only').splitlines())
    untracked = set(git('ls-files', '--others', '--exclude-standard').splitlines())
    assert changed | untracked == set(candidates(v)), 'Unexpected publication/worktree membership'
    assert sha((ROOT / '.gitignore').read_bytes()) == receipt['gitignore_sha256']
    shared = None
    for path, r in receipt['raw_files'].items():
        raw = (v / 'originals' / path).read_bytes()
        assert sha(raw) == r['raw_sha256'] == EXPECTED[Path(path).name]
        public, mapping, _ = project(raw)
        assert (ROOT / path).read_bytes() == public and sha(public) != sha(raw)
        assert mapping == r['mapping']
        if shared is None:
            shared = mapping
        assert shared == mapping
    for path, h in receipt['initial_hashes'].items():
        if path not in EDITABLE:
            assert sha((ROOT / path).read_bytes()) == h, 'Unexpected change: ' + path
    m = load(ROOT / FINAL)
    assert m['files'] == inventory(v)
    assert set(m['files']) | {FINAL} == set(candidates(v))
    assert subprocess.run(['git', '-c', 'safe.directory=' + ROOT.as_posix(),
                           'check-ignore', '--no-index', '-q', '--', FINAL], cwd=ROOT).returncode == 1
    for name in ['publication-manifest.json', 'publication-manifest-final.json']:
        old = load(ROOT / OUT / name)
        for p, r in old['files'].items():
            assert r == m['files'][p], 'Stale subset manifest entry: ' + p
    assert subprocess.run(['git', '-c', 'safe.directory=' + ROOT.as_posix(), 'diff', '--check'], cwd=ROOT).returncode == 0
    print('PASS: four raw originals, 280 aliases, structural equality, unchanged implementation, manifest DAG, trackability, empty index and diff check.')


def public():
    """Portable published-byte checks, independent of the private originals/branch.

    Git tracking fields describe the preflight snapshot; a later owner commit can
    change tracked status without changing artifact bytes. Raw-local equivalence
    and retention require verify and the private vault, never public placeholders.
    """
    manifest = load(ROOT / FINAL)
    assert FINAL not in manifest['files']
    assert manifest['candidate_count'] == len(manifest['files']) + 1
    for path, record in manifest['files'].items():
        assert (ROOT / path).resolve().is_relative_to(ROOT.resolve())
        data = (ROOT / path).read_bytes()
        assert sha(data) == record['sha256'] and len(data) == record['bytes'], path
        assert subprocess.run(['git', '-c', 'safe.directory=' + ROOT.as_posix(),
                               'check-ignore', '-q', '--', path], cwd=ROOT).returncode == 1
    for name in ['publication-manifest.json', 'publication-manifest-final.json']:
        assert all(record == manifest['files'][path] for path, record in load(ROOT / OUT / name)['files'].items())
    for path, record in load(ROOT / OUT / 'public-sanitization.json')['files'].items():
        data = (ROOT / path).read_bytes()
        assert sha(data) == record['public_sha256'] and len(data) == record['public_bytes']
        doc = json.loads(data)
        pins = doc.get('source_pins', doc.get('protected_source_pins'))
        assert sum(PLACEHOLDER in key for key in pins) == record['redacted_keys'] == 70
    assert subprocess.run(['git', '-c', 'safe.directory=' + ROOT.as_posix(),
                           'check-ignore', '-q', '--', FINAL], cwd=ROOT).returncode == 1
    print('PASS public-only mode: all current hashes/bytes, subset manifests, aliases and trackability; no private vault accessed.')


def scan():
    """Fail closed, logging paths/categories only; never print matched private values."""
    v = vault()
    raw = (v / 'originals' / OUT / 'prechange-manifest.json').read_bytes()
    _, _, identifiers = project(raw)
    tokens = set(identifiers)
    for identifier in identifiers:
        tokens.update(t for t in identifier.split('_') if len(t) >= 3)
    patterns = {
        'local_profile_or_appdata': r'(?i)(?:[a-z]:[/\\]+users[/\\]+[^/\\\s"<>]+|[/\\](?:home|Users)[/\\]+[^/\\\s"<>]+|[a-z]:[/\\]+[^\r\n"<>]*[/\\]AppData[/\\])',
        'absolute_drive_path': r'(?i)\b[a-z]:[/\\]+[A-Za-z0-9_]',
        'unc_share': r'\\\\[A-Za-z0-9_.-]+\\[A-Za-z0-9_$-]+',
        'domain_sid': r'S-1-5-21-\d+-\d+-\d+-\d+',
        'email': r'\b[A-Za-z0-9._%+-]+@[A-Za-z0-9.-]+\.[A-Za-z]{2,}\b',
        'private_key_or_token': r'-----BEGIN (?:RSA |EC |OPENSSH )?PRIVATE KEY|\b(?:gh[pousr]_[A-Za-z0-9]{20,}|AKIA[A-Z0-9]{16}|sk-[A-Za-z0-9]{24,})',
        'credential_url': r'https?://[^\s/"<>:@]+:[^\s/"<>@]+@',
        'connection_secret': r'(?i)(?:password|pwd|accountkey|client_secret)\s*[=:]\s*[A-Za-z0-9+/]{8,}',
        'internal_hostname': r'(?i)\b(?:https?://)?[A-Za-z0-9_-]+\.(?:local|internal|corp)\b',
    }
    hits = []
    for path in candidates(v):
        if not (ROOT / path).is_file():
            continue
        text = (ROOT / path).read_text(encoding='utf-8-sig')
        views = [('path', path), ('content', text)]
        if path.endswith('.json'):
            # Decode escaped Unicode and slash variants in JSON keys and values.
            views.append(('decoded_json', json.dumps(json.loads(text), ensure_ascii=False)))
        # Captures are serialized as base64 in existing baseline registries.
        for match in re.finditer(r'[A-Za-z0-9+/]{80,}={0,2}', text):
            try:
                decoded = base64.b64decode(match[0], validate=True).decode('utf-8')
                if decoded.isprintable() or '\n' in decoded:
                    views.append(('decoded_payload', decoded))
            except (ValueError, UnicodeError):
                pass
        for origin, content in views:
            content = unicodedata.normalize('NFKC', content)
            for token in tokens:
                if re.search(r'(?i)(?<![A-Za-z0-9])' + re.escape(token) + r'(?![A-Za-z0-9])', content):
                    # Manually reviewed ambiguity: the public product brand also
                    # appears in authored Help and long-established CSV basenames.
                    if token.casefold() == ROOT.name[:4].casefold() and (path in {OUT + n for n in EXPECTED} or path in {
                        'BESHStatNG_Help_MkDocs/docs/methods/generalized-linear-models-glm.md',
                        'BESHStatNG_Help_MkDocs/docs/methods/zero-inflated-poisson-regression.md'}):
                        continue
                    hits.append(dict(path=path, origin=origin, category='known_private_identifier'))
            for category, pattern in patterns.items():
                inspected = content if category == 'unc_share' else content.replace('\\\\', '\\')
                for match in re.finditer(pattern, inspected):
                    line = content.count('\n', 0, match.start()) + 1
                    hits.append(dict(path=path, origin=origin, line=line, category=category))
    # These are category/path/line references only: no sensitive matched values.
    print(json.dumps(dict(candidate_count=len(candidates(v)), findings=hits), indent=2))
    return hits


if __name__ == '__main__':
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('action', choices=['backup', 'sanitize', 'refresh', 'verify', 'public', 'scan', 'replan'])
    action = parser.parse_args().action
    outcome = globals()[action]()
    if action == 'scan' and outcome:
        raise SystemExit(1)
