"""04A.08 publication derivation/checks; original local snapshots stay the development authority."""
import argparse
import hashlib
import json
from pathlib import Path
import re
import subprocess

ROOT = Path(__file__).resolve().parents[1]
BASE = 'b48877406c3c185a992d5dd5c42f951611a52569'
OLD = Path('docs/architecture/04A08')
NEW = Path('evidence/04A08')
PINS = {
    'authoring-fixtures.json': '36ec16f860627749dc9d1d3ed57485471d52fa8abb2bac0b8de80345349016e4',
    'formula-compatibility.json': 'e2df4a1395f37d490805ccc71408d64c7d0c35bb20e4ef157b7bbe4e2409ea59',
    'ownership-matrix.json': '8e1b610db3928caf27a7d0c6230a202b484f9511208fbb2b307810e2e26ba2c7',
    'removal-characterization.json': 'f2852d17dbe035813b27b4e42e0693b49ee9ca96cadf3b047219d210702bedd1',
    'restriction-crosswalk.json': '1d97e62efc5d172706814011cecb2db6fd2c2682c11b0ee6bebf4a6f67cb15c9',
    'validation-v2.json': 'e8da3029d7f8493c770e651378c94d6b9b079e8cb4d4f6f3853ef3b8ffe6d1c2',
    'publication-inventory-v2.json': '899e53098a05bf95e1418e4270201b9b5fec781d6a9c0ec4de6368a6da043ef2',
}
EXCLUDED = ['baseline.json', 'validation.json', 'publication-inventory.json']
LOCAL_MANIFEST = OLD/'relocation-manifest-v1-local-original.json'
RESTRICTIONS = dict(all_native_obligations='OPEN', released_controls=0, discharged_obligations=0,
                    P='NO', N1='NO GO', N2='NO', C2='UNMET', numerical_references='unpromoted candidates')
OWNER_DECISION = (
    'Official Batch 04A.08 is owner-accepted for its software scope: portable regression-effect '
    'authoring and typed model-specification policy in BESHStat.Excel.Common, with '
    'RegressionEffectsController and typed removal integration in the Windows host. '
    'The owner accepts the five managed failures and 49 Python nonpasses as documented '
    'nonpasses. Core coefficient-label and categorical-reference normalization issues are '
    'unresolved; numerical references are unpromoted candidates. All native obligations '
    'remain OPEN, with zero control releases and zero obligation discharges. '
    'P = NO; N1 = NO GO; N2 = NO; Gate C2 = UNMET.')


def sha(data):
    return hashlib.sha256(data).hexdigest()


def git(root, *args):
    return subprocess.run(['git', '-c', 'safe.directory='+root.as_posix(), *args],
                          cwd=root, text=True, capture_output=True)


def not_ignored(root, path):
    result = git(root, 'check-ignore', '-v', '--no-index', '--', path.as_posix())
    if result.returncode != 1 or result.stdout or result.stderr:
        raise ValueError('Ignored path or failed ignore check: '+str(path)+'; '+result.stdout+result.stderr)
    return dict(path=path.as_posix(), exit_code=1, output='', command='git check-ignore -v --no-index -- '+path.as_posix())


def read_object(path, expected):
    data = path.read_bytes()
    if sha(data) != expected:
        raise ValueError('SHA-256 mismatch: '+str(path))
    value = json.loads(data)
    if not isinstance(value, dict):
        raise ValueError('Expected the approved JSON object: '+str(path))
    return data, value


def read_development(root=ROOT):
    """Original hash-pinned local objects only; never fall back to sanitized publications."""
    return {name: read_object(root/OLD/name, pin)[1] for name, pin in PINS.items()}


def sanitize_text(text):
    """Stable general placeholders; no account/root literals are embedded in this policy."""
    for executable, placeholder in [('python.exe', '<PYTHON_EXE>'), ('MSBuild.exe', '<MSBUILD_EXE>'), ('vstest.console.exe', '<VSTEST_EXE>')]:
        pattern = r'(?i)\b[A-Z]:[\\/][^\r\n"<>|]*?[\\/]'+re.escape(executable)+r'\b'
        text = re.sub(pattern, lambda match: placeholder, text)
    text = re.sub(r'(?i)\b[A-Z]:[\\/](?:[^\r\n"<>|]*?[\\/])?BESHstatNG(?=[\\/]|$)', '<REPOSITORY_ROOT>', text)
    text = re.sub(r'(?i)\b[A-Z]:[\\/]Users[\\/][^\\/\s:"\']+', '<USER_PROFILE>', text)
    # Any remaining absolute local path is documentation, not an executable binding.
    return re.sub(r'(?i)\b[A-Z]:[\\/][^\r\n"<>|]*', '<LOCAL_PATH>', text)


def sanitize_value(value):
    if isinstance(value, dict):
        return {sanitize_text(key): sanitize_value(item) for key, item in value.items()}
    if isinstance(value, list):
        return [sanitize_value(item) for item in value]
    return sanitize_text(value) if isinstance(value, str) else value


def publication_bytes(name, source):
    if name != 'validation-v2.json':
        return source
    value = sanitize_value(json.loads(source))
    value['publication_derivation'] = dict(kind='sanitized-derived', policy='general-machine-path-placeholders-v1',
        source_path=(OLD/name).as_posix(), source_sha256=PINS[name],
        use='Non-executable publication rendition. Use the original local file for testing and development.')
    return (json.dumps(value, indent=2, ensure_ascii=False)+'\n').encode('utf-8')


def read_manifest(root):
    manifest = json.loads((root/NEW/'relocation-manifest-v1.json').read_bytes())
    if manifest.get('schema') != '04A08-evidence-publication-v2':
        raise ValueError('Expected sanitized publication mapping schema v2')
    items = manifest['items']
    if len(items) != 7 or len({r['old_path'] for r in items}) != 7 or len({r['new_path'] for r in items}) != 7:
        raise ValueError('Expected seven unique source/destination paths')
    expected = {(OLD/name).as_posix(): (NEW/name).as_posix() for name in PINS}
    if {r['old_path']: r['new_path'] for r in items} != expected:
        raise ValueError('Mapping differs from owner-approved seven-file scope')
    for row in items:
        name = Path(row['new_path']).name
        if row['source_sha256'] != PINS[name]:
            raise ValueError('Original evidence pin changed')
        derived = name == 'validation-v2.json'
        if row['disposition'] != ('sanitized-derived' if derived else 'copied-byte-identically'):
            raise ValueError('Invalid derivation disposition')
        if not derived and row['sha256'] != PINS[name]:
            raise ValueError('Unchanged publication copy differs from original pin')
    return manifest


def read_published(root=ROOT, require_sources=False):
    """Publication-integrity checks only; statistical/development checks use read_development."""
    manifest = read_manifest(root)
    result = {}
    for row in manifest['items']:
        name = Path(row['new_path']).name
        data, value = read_object(root/NEW/name, row['sha256'])
        if require_sources:
            original, _ = read_object(root/OLD/name, PINS[name])
            if data != publication_bytes(name, original):
                raise ValueError('Publication differs from documented source derivation: '+name)
        result[name] = value
    return result


def strings(value, pointer=''):
    if isinstance(value, dict):
        for key, item in value.items():
            path = pointer+'/'+key.replace('~', '~0').replace('/', '~1')
            yield path+'/$key', key
            yield from strings(item, path)
    elif isinstance(value, list):
        for index, item in enumerate(value):
            yield from strings(item, pointer+'/'+str(index))
    elif isinstance(value, str):
        yield pointer, value


def privacy_inventory(value):
    """Triage inventory, not a credential detector or safety assurance. Never prints secret matches."""
    patterns = {
        'windows_absolute_paths': re.compile(r'(?i)\b[A-Z]:[\\/][^\r\n"<>|]*'),
        'windows_accounts': re.compile(r'(?i)\b[A-Z]:[\\/]Users[\\/]([^\\/\s:"\']+)'),
        'windows_sids': re.compile(r'\bS-1-(?:\d+-){1,14}\d+\b'),
        'unc_paths': re.compile(r'(?<!\\)\\\\(?![?.]\\)[^\\/\s"\']+[\\/][^\r\n"<>|]*'),
        'private_ipv4': re.compile(r'\b(?:10\.\d{1,3}|192\.168|172\.(?:1[6-9]|2\d|3[01]))\.\d{1,3}\.\d{1,3}\b'),
        'secret_token_patterns': re.compile(r'(?i)(?:\bgh[pousr]_[A-Za-z0-9_]{20,}|\bgithub_pat_[A-Za-z0-9_]{20,}|\bsk-(?:proj-)?[A-Za-z0-9_-]{16,}|\bAKIA[0-9A-Z]{16}\b|-----BEGIN [A-Z ]*PRIVATE KEY-----|\bBearer\s+[A-Za-z0-9._~+/=-]{12,}|\b(?:password|passwd|api[_-]?key|client[_-]?secret|access[_-]?token|refresh[_-]?token)\s*[:=]\s*["\']?[^\s"\'<>]{6,})'),
    }
    matches = {key: [] for key in patterns}
    for pointer, text in strings(value):
        for category, pattern in patterns.items():
            for match in pattern.finditer(text):
                finding = dict(json_pointer=pointer)
                if category == 'secret_token_patterns':
                    finding.update(match_sha256=sha(match[0].encode()), value='withheld; manual review required')
                else:
                    finding['value'] = match[1] if category == 'windows_accounts' else match[0]
                matches[category].append(finding)
    return dict(status='privacy-review-pending', findings=matches,
                caveat='Basic regex inventory only; no assurance that secrets or identifying information are absent. Exact bytes are unredacted; owner review is required before public Git publication.')


def validate_mapping(root=ROOT, require_sources=False, check_ignore=True):
    values = read_published(root, require_sources)
    manifest = read_manifest(root)
    items = manifest['items']
    if len(items) != 7 or len({r['old_path'] for r in items}) != 7 or len({r['new_path'] for r in items}) != 7:
        raise ValueError('Expected seven unique source/destination paths')
    if set(manifest['excluded_local_only']) != {(OLD/name).as_posix() for name in EXCLUDED}|{LOCAL_MANIFEST.as_posix()}:
        raise ValueError('Excluded evidence list differs')
    if manifest['original_inventory_sha256'] != PINS['publication-inventory-v2.json'] or manifest['restrictions'] != RESTRICTIONS:
        raise ValueError('Inventory identity or retained restrictions differ')
    for row in items:
        path = root/row['new_path']
        if row['byte_count'] != path.stat().st_size:
            raise ValueError('Invalid copy accounting')
        if check_ignore:
            not_ignored(root, Path(row['new_path']))
    if any((root/NEW/name).exists() for name in EXCLUDED):
        raise ValueError('Excluded local-only evidence found in publication directory')
    for name, value in values.items():
        if any(privacy_inventory(value)['findings'].values()):
            raise ValueError('Machine/secret-pattern finding remains in publication: '+name)
    if any(privacy_inventory(manifest)['findings'].values()):
        raise ValueError('Private identifying detail remains in public mapping')
    crosswalk = values['restriction-crosswalk.json']
    if tuple(crosswalk[k] for k in ['released_controls', 'discharged_obligations', 'P', 'N1', 'N2', 'C2']) != (0, 0, 'NO', 'NO GO', 'NO', 'UNMET'):
        raise ValueError('Native/scientific statuses changed')
    if any(row['status'] != 'OPEN' for row in crosswalk['affected_controls'] if row['obligation_id'] is not None):
        raise ValueError('Original native obligation is no longer OPEN')
    return manifest


def publication_manifest(captures, archive_sha=None):
    items = []
    for name, (source, original) in captures.items():
        data = publication_bytes(name, source)
        public = json.loads(data)
        counts = {key: len(rows) for key, rows in privacy_inventory(original)['findings'].items()}
        items.append(dict(old_path=(OLD/name).as_posix(), new_path=(NEW/name).as_posix(), source_sha256=PINS[name],
            sha256=sha(data), source_byte_count=len(source), byte_count=len(data),
            disposition='sanitized-derived' if name == 'validation-v2.json' else 'copied-byte-identically',
            privacy=dict(status='general-placeholders-applied' if name == 'validation-v2.json' else 'no-configured-findings',
                original_finding_counts=counts,
                public_finding_counts={key: len(rows) for key, rows in privacy_inventory(public)['findings'].items()},
                raw_values_and_locations='Omitted from publication; retained only in original local evidence.')))
    return dict(schema='04A08-evidence-publication-v2', owner_decision_date='2026-11-03', owner_decision=OWNER_DECISION,
        scope='Owner-authorized general-placeholder sanitation of publication material. Original local evidence remains unchanged for testing/development.',
        branch='codex/04A-08', head=BASE, original_inventory_sha256=PINS['publication-inventory-v2.json'], items=items,
        excluded_local_only=[(OLD/name).as_posix() for name in EXCLUDED]+[LOCAL_MANIFEST.as_posix()], restrictions=RESTRICTIONS,
        local_raw_manifest=dict(path=LOCAL_MANIFEST.as_posix(), sha256=archive_sha),
        placeholder_policy=dict(version=1, values=['<PYTHON_EXE>', '<MSBUILD_EXE>', '<VSTEST_EXE>', '<REPOSITORY_ROOT>', '<USER_PROFILE>', '<LOCAL_PATH>'],
            effect='Machine-specific path text only; test identities, outcomes, counts, source pins and scientific/native statuses retained.'),
        index_empty=True, staged=False, public_publication='owner-approval-required; basic privacy screening is not a safety assurance',
        historical_identity_note='Original paths/pins remain historical identities. validation-v2.json is explicitly derived, not byte-identical.',
        clone_scope='Public renditions are readable without ignored sources. Testing/development and historical reproduction require original local files.',
        manifest_self_hash='not embedded; avoid recursive hashing')


def require_authorized_workspace():
    if git(ROOT, 'branch', '--show-current').stdout.strip() != 'codex/04A-08' or git(ROOT, 'rev-parse', 'HEAD').stdout.strip() != BASE or git(ROOT, 'rev-parse', 'main').stdout.strip() != BASE:
        raise ValueError('Unexpected branch/base; stop')
    if git(ROOT, 'diff', '--cached', '--exit-code').returncode:
        raise ValueError('Index is not empty; stop')


def copy():
    require_authorized_workspace()
    if (ROOT/NEW).exists():
        raise ValueError('Destination already exists; no overwrite is permitted')
    # Verify everything, including live global/parent ignore rules, before the first copy.
    captures = {name: read_object(ROOT/OLD/name, pin) for name, pin in PINS.items()}
    ignore = [not_ignored(ROOT, path) for path in [NEW.parent, NEW]+[NEW/name for name in PINS]]
    (ROOT/NEW).mkdir(parents=True, exist_ok=False)
    for name, (data, value) in captures.items():
        with (ROOT/NEW/name).open('xb') as stream:
            stream.write(publication_bytes(name, data))
    manifest = publication_manifest(captures)
    manifest['ignore_preflight'] = ignore
    with (ROOT/NEW/'relocation-manifest-v1.json').open('x', encoding='utf-8', newline='\n') as stream:
        json.dump(manifest, stream, indent=2, ensure_ascii=False)
        stream.write('\n')
    validate_mapping(ROOT, require_sources=True)
    print('PASS: six exact copies and one sanitized derivative; original source pins preserved; nothing staged')


def sanitize():
    require_authorized_workspace()
    captures = {name: read_object(ROOT/OLD/name, pin) for name, pin in PINS.items()}
    prior_path = ROOT/NEW/'relocation-manifest-v1.json'
    prior_bytes = prior_path.read_bytes()
    prior = json.loads(prior_bytes)
    archive = ROOT/LOCAL_MANIFEST
    for name, (source, _) in captures.items():
        current = (ROOT/NEW/name).read_bytes()
        if current not in (source, publication_bytes(name, source)):
            raise ValueError('Unexpected owner-edited publication: '+name)
    if prior['schema'] == '04A08-evidence-relocation-v1':
        if archive.exists():
            if archive.read_bytes() != prior_bytes:
                raise ValueError('Existing raw local manifest differs; no overwrite')
        else:
            result = git(ROOT, 'check-ignore', '--no-index', '--', LOCAL_MANIFEST.as_posix())
            if result.returncode != 0:
                raise ValueError('Raw local archive must remain ignored')
            with archive.open('xb') as stream:
                stream.write(prior_bytes)
    elif prior['schema'] != '04A08-evidence-publication-v2':
        raise ValueError('Unexpected publication mapping schema')
    for name, (source, _) in captures.items():
        desired = publication_bytes(name, source)
        if (ROOT/NEW/name).read_bytes() != desired:
            (ROOT/NEW/name).write_bytes(desired)
    manifest = publication_manifest(captures, sha(archive.read_bytes()) if archive.exists() else None)
    prior_path.write_text(json.dumps(manifest, indent=2, ensure_ascii=False)+'\n', encoding='utf-8', newline='\n')
    print('PASS: publication identifiers replaced; six original copies unchanged; local source files retained')


if __name__ == '__main__':
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('action', choices=['copy', 'sanitize', 'check'])
    args = parser.parse_args()
    if args.action == 'copy':
        copy()
    elif args.action == 'sanitize':
        sanitize()
    else:
        validate_mapping(ROOT, require_sources=True)
        print('PASS: original pins, publication hashes/derivation, placeholders, JSON, ignore rules and OPEN restrictions')
