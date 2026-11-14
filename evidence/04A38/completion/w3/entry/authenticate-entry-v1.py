"""Read-only live authentication; writes exclusively new, versioned W3 receipts.

No recursive checkout/TestResults search, source repair, or original execution.
"""
import concurrent.futures
import hashlib
import json
import os
import platform
import subprocess
import sys
import time
from pathlib import Path

ROOT = Path(__file__).resolve().parents[5]
W3 = ROOT / 'evidence/04A38/completion/w3'
W2 = W3.parent / 'w2'
ACCEPTED = W2 / 'continuation-v17-postcorrection-v1'
BASE = 'ba1636f5356ac761509c2cfa16fc66b92430935d'
INDEX = '2036821003b1e1fb1077e72183e0b6c2fa7d22cdb2e784b760637e6d6fe9bc2c'


def load(p):
    return json.loads(Path(p).read_bytes())


def save(p, data):
    p = Path(p)
    p.parent.mkdir(parents=True, exist_ok=True)
    with p.open('x', encoding='utf-8', newline='\n') as f:
        json.dump(data, f, indent=2, ensure_ascii=False, allow_nan=False)
        f.write('\n')


def pin(p):
    p = Path(p)
    with p.open('rb') as f:
        h = hashlib.file_digest(f, 'sha256').hexdigest()
    return dict(path=p.relative_to(ROOT).as_posix(), bytes=p.stat().st_size, sha256=h)


def command(args):
    started = time.time()
    r = subprocess.run(args, cwd=ROOT, stdout=subprocess.PIPE, stderr=subprocess.PIPE,
                       env=dict(os.environ, GIT_OPTIONAL_LOCKS='0', PYTHONDONTWRITEBYTECODE='1'))
    return dict(command=args, start_utc=time.strftime('%Y-%m-%dT%H:%M:%SZ', time.gmtime(started)),
                seconds=time.time()-started, exit_code=r.returncode,
                stdout=r.stdout.decode('utf-8', 'replace'), stderr=r.stderr.decode('utf-8', 'replace'))


def git_receipt():
    commands = {}
    for label, args in [('branch', ['branch', '--show-current']), ('HEAD', ['rev-parse', 'HEAD']),
                        ('main', ['rev-parse', 'main']), ('merge-base', ['merge-base', 'HEAD', 'main']),
                        ('staged', ['diff', '--cached', '--name-only']), ('diff-check', ['diff', '--check']),
                        ('status', ['status', '--short'])]:
        commands[label] = command(['git', '-c', 'safe.directory='+ROOT.as_posix(), '--no-optional-locks', *args])
    return dict(commands=commands, index=pin(ROOT/'.git/index'))


def check(row, expected_id=None):
    p = ROOT / row['path']
    result = dict(path=row['path'], expected=row)
    try:
        if p.is_symlink() or not p.resolve().is_relative_to(ROOT):
            raise ValueError('UNSAFE_PIN_PATH')
        result['actual'] = pin(p)
        result['bytes_match'] = result['actual'] == row
        s = p.stat()
        result['actual_identity'] = dict(device_hex=format(s.st_dev, 'x'), file_id_hex=format(s.st_ino, 'x'))
        if expected_id is not None:
            result['expected_identity'] = expected_id
            result['identity_match'] = result['actual_identity'] == expected_id
        result['status'] = 'VERIFIED_PASS' if result['bytes_match'] and result.get('identity_match', True) else 'UNEXPECTED_DIFFERENCE'
    except (OSError, ValueError) as e:
        result.update(status='NOT_VERIFIABLE', error_type=type(e).__name__, error=str(e))
    return result


def main():
    start = time.time()
    # Authority precedence follows the owner's prompt. Read full JSON locally;
    # only bounded summaries go to the console.
    top = [W2/'continuation-v17-postcorrection-v1-postseal-handover-v2.md',
           ACCEPTED/'owner-review.md', W2/'continuation-v17-postcorrection-v1-review-delivery-receipt-v1.json',
           W2/'continuation-v17-postcorrection-v1-final-additive-readback-v1.json',
           W2/'continuation-v17-postcorrection-v1-review-parts-index-v1.json']
    names = ['production-test-freeze-v1.json', 'entry-live-authentication-v1.json',
             'whole-body-dependency-fragment-reconciliation-v1.json', 'canonical-identity-status-v1.json',
             'remaining-work-queue-v1.json', 'frozen-reference-provenance-v1.json',
             'managed-Debug-identities-v1.json', 'python-effective-identities-v1.json',
             'python-nonpass-delta-v1.json', 'validation-v1.json', 'publication-privacy-decisions-v1.json',
             'verify_postcorrection_v3.py', 'reader_controls_v3.py', 'private_preservation_v2.py',
             'verify_transport_v1.py', 'verify_postseal_supplement_v1.py', 'review-seal-v1.json']
    authorities = [pin(p) for p in top + [ACCEPTED/n for n in names] + [ROOT/'planning/BESHStatNG_CrossPlatform_MasterPlan.md']]
    receipt = load(top[2]); readback = load(top[3]); index = load(top[4])
    links = [receipt[k] for k in ['final_readback', 'core_index', 'supplement_reader']]
    links += [readback[k] for k in ['authority', 'handover', 'private_addendum', 'private_base', 'base_actual', 'core_parts_index']]
    seal = load(ACCEPTED/'review-seal-v1.json')
    links += seal['files']
    linked_checks = [check(r) for r in {r['path']: r for r in links}.values()]
    save(W3/'entry/authority-authentication-v1.json', dict(authorities=authorities, linked_checks=linked_checks,
         owner_assertion=dict(date='2026-11-13', source='Current owner W3 instruction history',
                              content='W2 source completion explicitly accepted following independent v17 post-correction review; no Phase04A.38/scientific/native acceptance and no 04A.39 authorization.'),
         independent_review='NOT_LOCATED_BY_AUTHORITY_CHAIN; no copy manufactured',
         master_plan='Revision 2.14 descriptive W2 state is historical v12; preserved unchanged'))
    git = git_receipt()
    save(W3/'entry/git-v1.json', git)
    freeze = load(ACCEPTED/'production-test-freeze-v1.json')
    sources = [check(r) for r in freeze['sources']]
    # Exact live-only set is taken from frozen package/installer-support paths;
    # crosswalk to exported manifest records accessible versus omitted paths.
    exported = {r['path'] for r in index['files']}
    live_only = [r for r in sources if r['path'] not in exported]
    save(W3/'entry/source-lock-v1.json', dict(freeze=pin(ACCEPTED/'production-test-freeze-v1.json'),
         rows=sources, count=len(sources), differences=[r for r in sources if r['status']!='VERIFIED_PASS'],
         not_in_review_payload=live_only))
    env = dict(os=platform.platform(), python=sys.version, python_executable=sys.executable,
               python_executable_sha256=hashlib.sha256(Path(sys.executable).read_bytes()).hexdigest(),
               process_architecture=platform.machine(), environment={k:os.environ.get(k) for k in
               ['LANG','LC_ALL','DOTNET_ROOT','DOTNET_CLI_HOME','NUGET_PACKAGES','PROCESSOR_ARCHITECTURE']})
    env['commands'] = [command(a) for a in [['git','--version'], ['dotnet','--info'], ['node','--version'],
        ['powershell','-NoProfile','-Command','[System.Globalization.CultureInfo]::CurrentCulture.Name; [System.Globalization.CultureInfo]::CurrentUICulture.Name; $PSVersionTable.PSVersion.ToString()']]]
    save(W3/'entry/environment-v1.json', env)
    # Union all exact protected pins; no recursive discovery.
    private = load(ACCEPTED/'retained-private-preservation-v2.json')
    add = load(W2/'continuation-v17-postcorrection-v1-postseal-private-addendum-v1.json')
    print('Private additive schema', list(add), flush=True)
    rows = {r['path']:r for r in private['private_pins']}
    additions = add['postseal_private_pins'] + receipt['last_completed_raw_pins'] + [receipt['postseal_supplement']]
    conflicts = []
    for r in additions:
        if r['path'] in rows and rows[r['path']] != r:
            conflicts.append(dict(old=rows[r['path']], new=r))
        rows[r['path']] = r
    ids = {r['path']:{k:v for k,v in r.items() if k!='path'} for r in private['current_filesystem_identities']}
    print('Auditing exact private pin union', len(rows), 'four hash threads; no TestResults enumeration', flush=True)
    with concurrent.futures.ThreadPoolExecutor(max_workers=4) as pool:
        results = list(pool.map(lambda r: check(r, ids.get(r['path'])), rows.values()))
    archives = {r['path']:r for r in private['retained_physical_archives']}
    archives[receipt['postseal_supplement']['path']] = receipt['postseal_supplement']
    save(W3/'preservation/live-private-v1.json', dict(expected_union=31910, actual_union=len(rows),
         expected_archives=152, actual_archives=len(archives), conflicts=conflicts, rows=results,
         differences=[r for r in results if r['status']!='VERIFIED_PASS'], archives=list(archives.values()),
         historical_TestResults='NOT_ENUMERATED', giant='EXACT_PROTECTED_PIN_STREAM_HASH_ONLY'))
    g = git['commands']
    errors = []
    if any(r['exit_code'] for r in g.values()): errors.append('GIT_COMMAND_NONZERO')
    if g['branch']['stdout'].strip() != 'codex/04A-38': errors.append('BRANCH_CHANGED')
    if any(g[k]['stdout'].strip()!=BASE for k in ['HEAD','main','merge-base']): errors.append('GIT_ANCHOR_CHANGED')
    if g['staged']['stdout'] or git['index']['sha256']!=INDEX: errors.append('INDEX_CHANGED')
    if len(sources)!=1350 or any(r['status']!='VERIFIED_PASS' for r in sources): errors.append('SOURCE_FREEZE_CHANGED')
    if any(r['status']!='VERIFIED_PASS' for r in linked_checks): errors.append('AUTHORITY_BYTES_CHANGED_OR_MISSING')
    if len(rows)!=31910 or len(archives)!=152 or conflicts or any(r['status']!='VERIFIED_PASS' for r in results): errors.append('PROTECTED_PRIVATE_PINS_CHANGED_OR_MISSING')
    save(W3/'entry/entry-manifest-v1.json', dict(status='ENTRY_AUTHENTICATED' if not errors else 'ENTRY_STOP',
         errors=errors, elapsed_seconds=time.time()-start, sources=len(sources), live_only=len(live_only),
         private_pins=len(rows), physical_archives=len(archives),
         receipts=[pin(W3/p) for p in ['entry/authority-authentication-v1.json','entry/git-v1.json',
                   'entry/source-lock-v1.json','entry/environment-v1.json','preservation/live-private-v1.json']],
         bounded_plan='No campaign starts unless entry locks pass. One build and six-suite campaign per configuration; current-only compatibility; no original execution. No TestResults enumeration.'))
    print('ENTRY', errors or 'PASS', flush=True)
    return 1 if errors else 0


if __name__ == '__main__':
    sys.exit(main())
