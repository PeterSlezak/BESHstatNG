"""Read-only entry authority checks against live merged main; append-only receipts."""
from pathlib import Path
import collections
import hashlib
import json
import os
import re
import subprocess

ROOT = Path(__file__).resolve().parents[2]
HERE = Path(__file__).resolve().parent


def sha(data):
    return hashlib.sha256(data).hexdigest()


def git(*args):
    return subprocess.check_output(['git', '-c', 'safe.directory=' + ROOT.as_posix(), *args], cwd=ROOT)


def pin(path):
    data = (ROOT / path).read_bytes()
    return dict(path=path, bytes=len(data), sha256=sha(data))


def public_path(path):
    names = [os.environ.get('COMPUTERNAME', '!unset!'), os.environ.get('USERNAME', '!unset!')]
    private = re.search(r'[A-Za-z]:|DESKTOP-|LAPTOP-|S-1-5-', path, re.I) or any(
        re.search(r'(?<![A-Za-z0-9])' + re.escape(n) + r'(?![A-Za-z0-9])', path, re.I) for n in names)
    return '<PRIVATE_PROTECTED_PATH:' + sha(path.encode())[:24] + '>' if private else path


def record_crlf(data):
    result = bytearray()
    quoted = False
    for c in data:
        if c == 34:
            quoted = not quoted
        if c == 10 and not quoted:
            result.append(13)
        result.append(c)
    return bytes(result)


def save(name, value):
    with (HERE / name).open('x', encoding='utf-8', newline='\n') as f:
        json.dump(value, f, indent=2, ensure_ascii=False)
        f.write('\n')


def main():
    head = git('rev-parse', 'HEAD').decode().strip()
    assert head == git('rev-parse', 'main').decode().strip() == git('merge-base', 'HEAD', 'main').decode().strip()
    assert git('branch', '--show-current').decode().strip() == 'codex/04A-37'
    assert not git('diff', '--name-only') and not git('diff', '--cached', '--name-only')
    manifest = json.loads((ROOT / 'evidence/04A36/publication-manifest.json').read_bytes())
    checked = []
    assert len(manifest['files']) == 152
    for row in manifest['files']:
        if row['sha256'] is None:
            assert row == dict(path='evidence/04A36/publication-manifest.json', bytes=None, sha256=None)
            continue
        data = git('show', head + ':' + row['path'])
        lf = data.replace(b'\r\n', b'\n')
        crlf = lf.replace(b'\n', b'\r\n')
        variants = dict(DIRECT=data, LF=lf, CRLF=crlf,
                        CRLF_EXCEPT_FINAL_LITERAL_LF=crlf[:-2] + b'\n' if crlf.endswith(b'\r\n') else crlf,
                        CSV_RECORD_CRLF_EMBEDDED_LF=record_crlf(lf))
        matches = [name for name, value in variants.items() if len(value) == row['bytes'] and sha(value) == row['sha256']]
        assert matches, row['path']
        assert (ROOT / row['path']).read_bytes().replace(b'\r\n', b'\n') == lf
        checked.append(dict(accepted=row, committed_bytes=len(data), committed_sha256=sha(data), qualification=matches))
    external = json.loads((ROOT / 'evidence/04A36/external-changes.json').read_bytes())
    documents = []
    for row in external['files']:
        actual = pin(row['path'])
        assert actual == row['current'], row['path']
        assert sha(git('show', head + ':' + row['path'])) == actual['sha256']
        documents.append(actual)
    assert len(documents) == 16
    plan = pin('planning/BESHStatNG_CrossPlatform_MasterPlan.md')
    assert plan['sha256'] == '436b66c4307bd1571649c2604f8d90b5f0b5d843df3567b918698392fe5d6e7c'
    save('entry-authority.json', dict(head=head, main=head, merge_base=head,
         branch='codex/04A-37', parents=git('show', '-s', '--format=%P', head).decode().strip(),
         integration='Single-parent owner integration, not an asserted two-parent merge.',
         entry_worktree='CLEAN before batch evidence creation', index='EMPTY',
         history=git('log', '-8', '--format=%H %P %s').decode().splitlines(),
         gates=dict(P='NO', N1='NO GO', N2='NO', C2='UNMET', C3='UNAPPROVED'),
         scientific='NOT_GOLDEN', accepted36_artifacts=checked,
         accepted36_review=pin('docs/architecture/batch-04A-36-owner-review.md'),
         accepted36_manifest=pin('evidence/04A36/publication-manifest.json'),
         accepted36_closeout=pin('evidence/04A36/closeout/v1/closeout-note.md'),
         owner_documents_excluded=documents,
         line_endings='Git LF normalization; exact inverse reconstruction of previously captured CRLF including CSV quoted embedded LF. Historical files unchanged.'))
    tracked = []
    for path in git('ls-files', '-z').decode().split('\0'):
        if path and (ROOT / path).is_file():
            if path.startswith('TestResults/'):
                raise ValueError('Unexpected tracked historical payload; require metadata-only policy')
            tracked.append(dict(pin(path), path=public_path(path)))
    save('entry-tracked-pins.json', dict(files=tracked, scope='Live tracked entry files only; ignored historical TestResults NOT_REHASHED.'))
    print('PASS merged-main entry authority;', len(checked), 'accepted36 artifacts;', len(documents), 'owner documents;', len(tracked), 'tracked pins')


if __name__ == '__main__':
    main()
