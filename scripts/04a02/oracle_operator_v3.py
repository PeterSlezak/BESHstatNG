"""C08 successor. Candidate execution requires separately pinned owner and technical receipts.

The frozen v2 manifest/operator remain historical inputs. Import and default CLI
perform no R execution. Human review authenticity remains an external trust input.
Windows API-set/SxS and dynamic loader resolution are bounded assumptions, not a
claim of exhaustive loader closure. No packages, network or promotion operation.
"""
from contextlib import ExitStack
import argparse
import hashlib
import importlib.util
import os
from pathlib import Path
import subprocess
import uuid

ROOT = Path(__file__).resolve().parents[2]
SPEC = importlib.util.spec_from_file_location('frozen_oracle_v2', ROOT / 'scripts/04a02/oracle_operator.py')
v2 = importlib.util.module_from_spec(SPEC)
SPEC.loader.exec_module(v2)
require, Refused, sha, canonical = v2.require, v2.Refused, v2.sha, v2.canonical
POLICIES = {
    'docs/architecture/04A02/04A02-C07-scientific-decision-form.json': '7afdbec07a4331cda915d47e21b075b55506cf53d560b95f3f17b4dc67a27574',
    'docs/architecture/04A02/04A02-C07-scientific-decision-form.md': 'fcb6f5563f73c379e65265eee4afcac3cdb4e05184aa7a9c1285daee76044053',
}
STATEMENTS = [
    'I approve both proposed policies and also GUI execution. Please update codex prompt accordingly.',
    'I also approve running the independent R statistical scripts. Please update codex prompt accordingly.',
]
CASES = {'C30-SYN-ANOVA': ('C07-S-ANOVA-v1', 'APPROVED_EXACT_C07_S_ANOVA_V1'),
         'C30-SYN-KW': ('C07-S-KW-v1', 'APPROVED_EXACT_C07_S_KW_V1')}
FROZEN_MANIFEST = '92bfb3ecc8849b3f8ff120f2b910d17d6f3d85ebf3fdd7a7a4d361a53f34f6f4'
FROZEN_OPERATOR = '5aa59d822a824808d3fd42bef9b1c1bc8d789dfc92d80dc4a6f108202aa7d30b'


def digest(path):
    with Path(path).open('rb') as stream:
        return hashlib.file_digest(stream, 'sha256').hexdigest()


def pinned(path, expected):
    v2.hex_sha(expected)
    raw = Path(path).read_bytes()
    require(sha(raw) == expected, 'Current hash differs: ' + str(path))
    return v2.strict_json(raw)


def owner_admission(receipt, case_id):
    require(receipt.get('schema') == '04A02-C08-owner-authorization-v1', 'New owner receipt required')
    require(receipt.get('date') == '2026-10-30' and receipt.get('statements') == STATEMENTS,
            'Both exact owner statements, in order, required')
    require(receipt.get('policy_documents') == POLICIES, 'Original policy pins required')
    rows = receipt.get('cases', [])
    require(len(rows) == 2 and {r['case_id'] for r in rows} == set(CASES), 'Only two authorized cases')
    require(case_id in CASES, 'Denied case')
    for row in rows:
        policy_id, state = CASES[row['case_id']]
        require(row == dict(case_id=row['case_id'], policy_id=policy_id, S=state,
                            R='OWNER_AUTHORIZED_TECHNICAL_GO_PENDING', P='NO'), 'S alone is not R authority')
    require(receipt.get('official_04A02') == 'NOT_ACCEPTED', 'No batch acceptance')
    return receipt['decision_id']


def private_environment(job, r_home):
    """Allowlist, not inherited environment. Rscript --vanilla suppresses profiles/environ.

    R_DEFAULT_PACKAGES permits only bundled base/stats and utils (packageVersion).
    Existing empty private library paths prevent the Windows default personal path.
    R_ENVIRON/R_PROFILE paths also point to an exclusive empty local file.
    """
    folders = {name: job / name for name in ('home', 'user-library', 'site-library', 'temp')}
    for path in folders.values():
        path.mkdir(exist_ok=False)
    empty = job / 'empty-startup'
    v2.exclusive_bytes(empty, b'')
    system = Path(os.environ.get('SystemRoot', 'C:/Windows'))
    require(system.is_absolute() and system.is_dir(), 'SystemRoot unavailable')
    env = dict(SystemRoot=str(system), WINDIR=str(system), R_HOME=str(r_home), PATH=str(r_home / 'bin/x64'),
               HOME=str(folders['home']), R_USER=str(folders['home']), R_LIBS_USER=str(folders['user-library']),
               R_LIBS_SITE=str(folders['site-library']), R_LIBS='', R_DEFAULT_PACKAGES='utils,stats',
               TEMP=str(folders['temp']), TMP=str(folders['temp']), TMPDIR=str(folders['temp']),
               LC_ALL='C', LANG='C', LANGUAGE='C', TZ='UTC')
    env.update({key: str(empty) for key in ('R_ENVIRON', 'R_ENVIRON_USER', 'R_PROFILE', 'R_PROFILE_USER')})
    return env


def assess_acl(evidence, root, execution_sid):
    """Conservative admission; raw allow ACEs are an upper bound, not AccessCheck.

    Independent review must resolve effective Modify/DELETE_CHILD, inheritance and
    generic masks for every ancestor. Unknown or unreadable entries always refuse.
    A child DACL cannot repair an unsafe or unreadable parent.
    """
    missing = []
    path = Path(os.path.abspath(root))
    expected = list(reversed([path, *path.parents]))
    rows = evidence.get('ancestors', [])
    if [r.get('path', '').casefold() for r in rows] != [str(p).casefold() for p in expected]:
        missing.append('complete ordered ancestor chain')
    if evidence.get('execution_sid') != execution_sid:
        missing.append('execution identity matches receipt')
    for row in rows:
        if row.get('error'):
            missing.append('unreadable ancestor: ' + row['path'])
        if row.get('reparse') is not False:
            missing.append('non-reparse proof: ' + row['path'])
        if not row.get('sddl') or not row.get('owner_sid'):
            missing.append('ACL/owner proof: ' + row['path'])
        if row.get('effective_modify_review') != 'INDEPENDENTLY_REVIEWED_EXCLUSIVE':
            missing.append('effective Modify/rename/delete-child/inheritance review: ' + row['path'])
        if row.get('unapproved_modify_sids', None) != []:
            missing.append('unapproved or unresolved modifying identities: ' + row['path'])
    if not evidence.get('independent_review_source') or not evidence.get('reviewer'):
        missing.append('independent owner-context ACL review source/reviewer')
    return missing


def reviewed_acl(observed, review):
    """Attach independent review only to exactly matching current raw evidence."""
    require(review.get('schema') == '04A02-owner-output-acl-review-v1' and review.get('reviewer') and
            review.get('source') and review.get('observed_sha256') == sha(canonical(observed)),
            'Missing independent review of exact current ACL evidence')
    require(len(review.get('ancestors', [])) == len(observed['ancestors']), 'Incomplete ACL review')
    result = dict(observed, reviewer=review['reviewer'], independent_review_source=review['source'])
    result['ancestors'] = []
    for raw, decision in zip(observed['ancestors'], review['ancestors']):
        require(decision.get('path') == raw['path'], 'ACL review path drift')
        result['ancestors'].append(dict(raw, effective_modify_review=decision.get('effective_modify_review'),
                                        unapproved_modify_sids=decision.get('unapproved_modify_sids')))
    return result


def admission(manifest_path, expected_manifest_sha, case_id):
    manifest = pinned(manifest_path, expected_manifest_sha)
    require(manifest.get('schema') == '04A02-oracle-execution-manifest-v3', 'Successor manifest required')
    require(manifest.get('runner_sha256') == digest(Path(__file__)), 'Successor operator drift')
    require(manifest.get('acl_collector_sha256') == digest(ROOT / 'scripts/04a02/inspect_output_acl_v3.ps1'),
            'ACL collector drift')
    require(digest(ROOT / 'scripts/04a02/oracle_operator.py') == FROZEN_OPERATOR, 'Frozen utility drift')
    owner = pinned(ROOT / manifest['owner_receipt']['path'], manifest['owner_receipt']['sha256'])
    decision_id = owner_admission(owner, case_id)
    for path, expected in POLICIES.items():
        require(digest(ROOT / path) == expected, 'STOP policy drift')
    old = pinned(ROOT / manifest['source_manifest']['path'], FROZEN_MANIFEST)
    require(manifest['source_manifest']['sha256'] == FROZEN_MANIFEST, 'Frozen manifest pin drift')
    require(manifest.get('cases') == old['cases'], 'Altered/extra proposal cases')
    require(manifest.get('P') == 'NO' and manifest.get('R_owner_approved') is True, 'No promotion; explicit R required')
    case, fixture = v2.load_case(old, case_id)
    policy = next(p for p in v2.strict_json((ROOT / next(iter(POLICIES))).read_bytes())['cases'] if p['case_id'] == case_id)
    require(digest(ROOT / case['script']) == case['script_sha256'], 'Frozen script drift')
    return manifest, owner, decision_id, case, fixture, policy


def technical_admission(technical_path, expected_sha, manifest_sha, case_id, root):
    go = pinned(technical_path, expected_sha)
    require(go.get('schema') == '04A02-C08-R-technical-v1' and go.get('status') == 'R_TECHNICAL_GO',
            'R_TECHNICAL_NO_GO: qualified current receipt required')
    require(go.get('case_id') == case_id and go.get('manifest_sha256') == manifest_sha, 'Wrong technical case/hash')
    require(go.get('output_root') == str(root.absolute()), 'Unapproved output root')
    require(go.get('checks') == {key: 'PASS' for key in 'abcdefgh'}, 'Every mandatory technical check required')
    require(go.get('command_policy') == dict(arguments=['--vanilla'], timeout_seconds=30,
                                            stdin='DEVNULL', stderr='MUST_BE_EMPTY', shell=False), 'Command policy drift')
    require(go.get('runtime_review_source') and go.get('isolation_probe_receipt') and go.get('qualification_tests'),
            'Executed runtime/environment/negative qualification evidence required')
    return go


def run_reference(manifest_path, manifest_sha, technical_path, technical_sha, case_id, evidence_root, name):
    v2.token(name, 'Unique attempt name required')
    v2.component(name)
    evidence_root = evidence_root.absolute()
    manifest, owner, decision_id, case, fixture, policy = admission(manifest_path, manifest_sha, case_id)
    go = technical_admission(technical_path, technical_sha, manifest_sha, case_id, evidence_root)
    # Raw ACLs must be recollected before every run, not accepted from an old GO.
    acl = collect_acl(evidence_root)
    require(sha(canonical(acl)) == go['current_acl_sha256'], 'Current ACL/context drift')
    review = pinned(ROOT / go['acl_review']['path'], go['acl_review']['sha256'])
    require(not assess_acl(reviewed_acl(acl, review), evidence_root, go['execution_sid']),
            'Current ancestor ACL qualification missing')
    r_home = Path(manifest['r_home'])
    require(str(r_home).casefold() == r'c:\program files\r\r-4.5.1'.casefold(), 'Only qualified R 4.5.1')
    runtime = policy['runtime_proposal']
    executable = r_home / 'bin/x64/Rscript.exe'
    job = evidence_root / name
    with ExitStack() as stack:
        stack.enter_context(v2.locked_directory_chain(evidence_root))
        # Pin and hold all admitted inputs and source/runtime directory chains.
        inputs = [(Path(__file__), manifest['runner_sha256']),
                  (ROOT / 'scripts/04a02/inspect_output_acl_v3.ps1', manifest['acl_collector_sha256']),
                  (ROOT / go['acl_review']['path'], go['acl_review']['sha256']),
                  (ROOT / 'scripts/04a02/oracle_operator.py', FROZEN_OPERATOR),
                  (manifest_path, manifest_sha), (technical_path, technical_sha),
                  (ROOT / manifest['owner_receipt']['path'], manifest['owner_receipt']['sha256']),
                  (ROOT / manifest['source_manifest']['path'], FROZEN_MANIFEST),
                  (ROOT / case['script'], case['script_sha256']),
                  (ROOT / 'BESHStat.Excel.Common.Tests/TestData/04A02' / (case_id + '.json'), case['fixture_file_sha256']),
                  (executable, runtime['rscript_sha256'])]
        inputs += [(ROOT / p, h) for p, h in POLICIES.items()]
        inputs += [(ROOT / p['path'], p['sha256']) for p in fixture['source_pins']]
        inputs += [(r_home / v2.relative_component_path(p['relative_path']), p['sha256']) for p in runtime['runtime_components']]
        pins = {}
        for path, expected in inputs:
            stack.enter_context(v2.locked_directory_chain(path.parent, r_home if path.is_relative_to(r_home) else ROOT))
            stream = stack.enter_context(v2.locked_path(path))
            require(hashlib.file_digest(stream, 'sha256').hexdigest() == expected, 'Locked input changed: ' + str(path))
            pins[str(path)] = expected
        job.mkdir(exist_ok=False)
        stack.enter_context(v2.locked_path(job, directory=True))
        try:
            env = private_environment(job, r_home)
            runtime_set = sha(canonical({p['relative_path']: p['sha256'] for p in runtime['runtime_components']}))
            dcf = dict(zip(v2.DCF_FIELDS, [decision_id, case_id, '4.5.1', '4.5.1', case['script_sha256'],
                       runtime['rscript_sha256'], runtime_set, case['method_id'], case['tail_policy'], case['finite_policy']]))
            v2.exclusive_bytes(job / 'approved-input.dcf', v2.make_dcf(dcf))
            v2.exclusive_bytes(job / 'proposal-source.R', (ROOT / case['script']).read_bytes())
            command = [str(executable), '--vanilla', str(job / 'proposal-source.R'), str(job / 'approved-input.dcf')]
            v2.exclusive_bytes(job / 'technical-go.json', technical_path.read_bytes())
            v2.exclusive_bytes(job / 'preflight.json', canonical(dict(pins=pins, command=command, cwd=str(job),
                env=env, timeout_seconds=30, owner_receipt=owner, manifest=manifest, status='TECHNICAL_GO_NOT_GOLDEN')))
            with v2.locked_path(job / 'proposal-source.R'), v2.locked_path(job / 'approved-input.dcf'):
                execute_protocol(command, job, env, decision_id, case, fixture, policy, runtime_set)
            return job
        except BaseException as error:
            v2.exclusive_bytes(job / ('failure-' + uuid.uuid4().hex + '.json'), canonical(dict(
                status='FAILED_RETAINED_NO_RETRY', error_type=type(error).__name__, message=str(error))))
            raise


def execute_protocol(command, job, env, decision_id, case, fixture, policy, runtime_set):
    """Mockable subprocess boundary. No success publication after any protocol failure."""
    with (job / 'raw-stdout.bin').open('xb') as stdout, (job / 'raw-stderr.bin').open('xb') as stderr:
        result = subprocess.run(command, cwd=job, env=env, stdin=subprocess.DEVNULL, stdout=stdout, stderr=stderr,
                                timeout=30, shell=False, creationflags=getattr(subprocess, 'CREATE_NO_WINDOW', 0))
        stdout.flush()
        os.fsync(stdout.fileno())
        stderr.flush()
        os.fsync(stderr.fileno())
    v2.exclusive_bytes(job / 'process-exit.json', canonical(dict(exit_code=result.returncode)))
    require(result.returncode == 0, 'Failed subprocess')
    require((job / 'raw-stdout.bin').stat().st_size <= v2.MAX_JSON, 'Output too large')
    decision = dict(decision_id=decision_id, r_version='4.5.1', stats_version='4.5.1',
                    ordinary_p_range={k: policy['ordinary_p_range_proposal'][k] for k in ('min', 'max')})
    pins = dict(script_sha256=case['script_sha256'], rscript_sha256=policy['runtime_proposal']['rscript_sha256'],
                runtime_set_sha256=runtime_set)
    candidate = v2.validate_candidate((job / 'raw-stdout.bin').read_bytes(), (job / 'raw-stderr.bin').read_bytes(),
                                     decision, case, fixture, pins)
    v2.publish_complete(job, 'reference-candidate.json', canonical(dict(status='UNREVIEWED_REFERENCE_CANDIDATE_NOT_GOLDEN',
                                                                     candidate=candidate)))
    v2.publish_complete(job, 'publication-v3.json', canonical(dict(status='REFERENCE_EXECUTED_NOT_COMPARED_NOT_GOLDEN',
        hashes={p.name: digest(p) for p in job.iterdir() if p.is_file()})))


def collect_acl(root):
    """Read only. No live R, COM, Excel, ACL writes or elevation."""
    script = ROOT / 'scripts/04a02/inspect_output_acl_v3.ps1'
    result = subprocess.run(['powershell', '-NoProfile', '-ExecutionPolicy', 'Bypass', '-File', str(script),
                             '-OutputRoot', str(root.absolute())], cwd=ROOT, capture_output=True, timeout=30)
    require(result.returncode == 0 and not result.stderr, 'ACL collector failed')
    return v2.strict_json(result.stdout)


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--execute-approved-reference', action='store_true')
    parser.add_argument('--manifest', type=Path, required=True)
    parser.add_argument('--manifest-sha256', required=True)
    parser.add_argument('--technical-go', type=Path, required=True)
    parser.add_argument('--technical-go-sha256', required=True)
    parser.add_argument('--case', choices=tuple(CASES), required=True)
    parser.add_argument('--output-root', type=Path, required=True)
    parser.add_argument('--name', required=True)
    args = parser.parse_args()
    require(args.execute_approved_reference, 'No execution by default')
    run_reference(args.manifest, args.manifest_sha256, args.technical_go, args.technical_go_sha256,
                  args.case, args.output_root, args.name)


if __name__ == '__main__':
    main()
