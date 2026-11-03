"""Seal/verify complete 04A.13 public candidates. Never stage or change Git ignore rules."""
import argparse
import base64
import hashlib
import importlib.util
import json
import os
from pathlib import Path
import re
import subprocess
import unicodedata

ROOT = Path(__file__).resolve().parents[1]
OUT = ROOT / 'evidence/04A13'
MANIFEST = 'evidence/04A13/publication-manifest.json'
PRIVACY = 'evidence/04A13/privacy.json'


def git(*args):
    return subprocess.check_output(['git', '-c', 'safe.directory=' + ROOT.as_posix(), *args], cwd=ROOT, text=True).strip()


def sha(path):
    return hashlib.sha256(path.read_bytes()).hexdigest()


def paths():
    return sorted(set(git('diff', '--name-only').splitlines()) | set(git('ls-files', '--others', '--exclude-standard').splitlines()) | {MANIFEST, PRIVACY})


def scan(candidates):
    tokens = set()
    for ancestor in ROOT.parents:
        if ancestor.parent.name.casefold() == 'users':
            tokens.add(ancestor.name)
    if os.environ.get('COMPUTERNAME'):
        tokens.add(os.environ['COMPUTERNAME'])
    # Current owner and host identifiers come from the environment. Historical
    # private maps stay outside the publication set; do not load or rewrite them.
    patterns = {
        'profile_or_home': r'(?i)(?:[a-z]:[/\\]+users[/\\]+[^/\\\s"<>]+|[/\\](?:home|Users)[/\\]+[^/\\\s"<>]+)',
        'appdata_or_drive_path': r'(?i)\b[a-z]:[/\\]+[A-Za-z0-9_]',
        'unc_share': r'\\\\[A-Za-z0-9_.-]+\\[A-Za-z0-9_$-]+',
        'domain_sid': r'S-1-5-21-\d+-\d+-\d+-\d+',
        'email': r'\b[A-Za-z0-9._%+-]+@[A-Za-z0-9.-]+\.[A-Za-z]{2,}\b',
        'secret': r'-----BEGIN (?:RSA |EC |OPENSSH )?PRIVATE KEY|\b(?:gh[pousr]_[A-Za-z0-9]{20,}|AKIA[A-Z0-9]{16}|sk-[A-Za-z0-9]{24,})',
        'credential_url': r'https?://[^\s/"<>:@]+:[^\s/"<>@]+@',
        'connection_secret': r'(?i)(?:password|pwd|accountkey|client_secret)\s*[=:]\s*[A-Za-z0-9+/]{8,}',
        'internal_hostname': r'(?i)\b(?:https?://)?[A-Za-z0-9_-]+\.(?:local|internal|corp)\b',
    }
    findings, reviewed = [], set()
    for path in candidates:
        if not (ROOT / path).is_file():
            assert path == MANIFEST, 'Missing public candidate'
            continue
        text = (ROOT / path).read_text(encoding='utf-8-sig')
        views = [('path', path), ('content', text)]
        if path.endswith('.json'):
            doc = json.loads(text, parse_constant=lambda value: (_ for _ in ()).throw(ValueError('Non-standard JSON constant')))
            views.append(('decoded_json', json.dumps(doc, ensure_ascii=False)))
        for encoded in re.findall(r'[A-Za-z0-9+/]{80,}={0,2}', text):
            try:
                views.append(('decoded_payload', base64.b64decode(encoded, validate=True).decode('utf-8')))
            except (ValueError, UnicodeError):
                pass
        for origin, value in views:
            value = unicodedata.normalize('NFKC', value)
            for token in tokens:
                pattern = r'(?i)(?<![A-Za-z0-9])' + re.escape(token) + r'(?![A-Za-z0-9])'
                if not re.search(pattern, value):
                    continue
                if token.casefold() == ROOT.name[:4].casefold():
                    # Manually reviewed public brand references in this method page
                    # and established public comparison-fixture basenames only.
                    brand_prefix = re.escape(token) + r'(?=_(?:lmm_|mmrm_|vs_r_))'
                    remainder = re.sub(brand_prefix, '', value, flags=re.I)
                    if not re.search(pattern, remainder):
                        reviewed.add((path, 'public_product_brand_or_comparison_fixture_prefix'))
                        continue
                findings.append(dict(path=path, origin=origin, category='known_private_identifier'))
            for category, pattern in patterns.items():
                inspected = value if category == 'unc_share' else value.replace('\\\\', '\\')
                for match in re.finditer(pattern, inspected):
                    if category == 'appdata_or_drive_path' and path == 'BESHstatNG/BESHStatNG.vbproj':
                        sdk = re.match(r'[Cc]:\\Program Files \(x86\)\\Windows Kits\\10\\bin\\[0-9.]+\\x64\\signtool\.exe', inspected[match.start():], re.I)
                        if sdk and sdk[0] in git('show', 'HEAD:BESHstatNG/BESHStatNG.vbproj'):
                            reviewed.add((path, 'inherited_public_windows_sdk_tool_path'))
                            continue
                    findings.append(dict(path=path, origin=origin, line=inspected.count('\n', 0, match.start()) + 1, category=category))
    return findings, sorted(reviewed)


def seal():
    assert git('branch', '--show-current') == 'codex/04A-13'
    assert not git('diff', '--cached', '--name-only')
    candidates = paths()
    # Create the receipt before scanning, so it is included in its own privacy scope.
    receipt = dict(status='PASS_PUBLIC_CANDIDATE_SCAN', candidate_count=len(candidates),
        scope=['Every proposed changed/new Git path and its UTF-8 content', 'JSON-decoded keys/values, escaped paths, Unicode NFKC and case variants',
               'Decodable base64 table/capture payloads', 'Known current owner/home and host components from the local environment',
               'Profile, home, AppData, drive and UNC paths; SIDs; keys/tokens/credential URLs; email/internal hosts/connection secrets'],
        unresolved=0, public_false_positive_policy='Public product-brand prose/comparison-fixture prefixes and unchanged shared Windows SDK tool paths manually reviewed; never whitelist complete owner/host identifiers. Existing public signing-certificate fingerprint is not a private key or credential; signing targets unchanged/unexecuted.',
        assurance='Pattern scan plus targeted review, not proof of absence of all secrets. Private originals, mapping, raw TRX/logs, baseline.json and caches excluded.')
    (ROOT / PRIVACY).write_text(json.dumps(receipt, indent=2) + '\n', encoding='utf-8', newline='\n')
    findings, reviewed = scan(candidates)
    if findings:
        print(json.dumps(dict(unresolved=findings), indent=2))
        raise SystemExit(1)
    receipt['reviewed_false_positive_groups'] = len(reviewed)
    receipt['reviewed_false_positive_categories'] = sorted({category for _, category in reviewed})
    (ROOT / PRIVACY).write_text(json.dumps(receipt, indent=2) + '\n', encoding='utf-8', newline='\n')
    tracked = set(git('ls-files').splitlines())
    records = {}
    for path in candidates:
        if path == MANIFEST:
            continue
        prefix = ['git', '-c', 'safe.directory=' + ROOT.as_posix(), 'check-ignore']
        normal = subprocess.run(prefix + ['-q', '--', path], cwd=ROOT).returncode
        rule = subprocess.run(prefix + ['--no-index', '-q', '--', path], cwd=ROOT).returncode
        assert normal == 1 and (rule == 1 or path in tracked), path
        records[path] = dict(sha256=sha(ROOT / path), bytes=(ROOT / path).stat().st_size,
                             git_ignore_exit=normal, git_ignore_rule_exit=rule, already_tracked=path in tracked)
    manifest = dict(base=git('rev-parse', 'main'), head=git('rev-parse', 'HEAD'), branch=git('branch', '--show-current'),
        files=records, candidate_count=len(candidates), self_hash_policy='Manifest omitted from its own hash map; included in privacy/trackability scope. No circular hashes.',
        excluded=['Historical TestResults/ignored raw logs and TRX', 'Private source-pin baseline and original ledgers not independently approved for publication',
                  'Private raw evidence vault/reverse mappings', 'Compiled outputs, caches, installed tools and unrelated local files'],
        publication='OWNER REVIEW PENDING; NOT STAGED / NOT COMMITTED / NOT PUSHED', native_executed=False, P='NO', N1='NO GO', N2='NO', C2='UNMET')
    (ROOT / MANIFEST).write_text(json.dumps(manifest, indent=2) + '\n', encoding='utf-8', newline='\n')
    verify()


def verify():
    manifest = json.loads((ROOT / MANIFEST).read_bytes())
    assert set(manifest['files']) | {MANIFEST} == set(paths())
    for path, record in manifest['files'].items():
        assert sha(ROOT / path) == record['sha256'] and (ROOT / path).stat().st_size == record['bytes'], path
        assert subprocess.run(['git', '-c', 'safe.directory=' + ROOT.as_posix(), 'check-ignore', '-q', '--', path], cwd=ROOT).returncode == 1
    assert subprocess.run(['git', '-c', 'safe.directory=' + ROOT.as_posix(), 'check-ignore', '--no-index', '-q', '--', MANIFEST], cwd=ROOT).returncode == 1
    assert not git('diff', '--cached', '--name-only')
    assert subprocess.run(['git', '-c', 'safe.directory=' + ROOT.as_posix(), 'diff', '--check'], cwd=ROOT).returncode == 0
    findings, _ = scan(paths())
    assert not findings, json.dumps(findings)
    print('PASS:', len(manifest['files']), 'external hashes/bytes;', manifest['candidate_count'], 'candidate privacy/trackability checks; empty index; diff check.')


if __name__ == '__main__':
    parser = argparse.ArgumentParser()
    parser.add_argument('action', choices=['seal', 'verify'])
    globals()[parser.parse_args().action]()
