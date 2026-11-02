"""SOURCE-ONLY proposal: approved operator boundary, never a golden promoter.

No R process is invoked by import, tests, or default CLI behavior. Human decision
authenticity is an external operator responsibility; this code checks the record,
exact hashes, Windows file ownership/locks, subprocess outcome and candidate shape.
It never interprets a boolean as a cryptographic owner signature.
"""
from contextlib import ExitStack, contextmanager
import argparse
import ctypes
import hashlib
import json
import math
import os
from pathlib import Path, PureWindowsPath
import re
import stat
import subprocess
import uuid

ROOT = Path(__file__).resolve().parents[2]
MAX_JSON = 1024 * 1024
GATES = {'run_independent_reference', 'promote_numerical_golden', 'native_workbook_capture'}
DCF_FIELDS = ('DecisionId', 'CaseId', 'RVersion', 'StatsVersion', 'ScriptSHA256',
              'RscriptSHA256', 'RuntimeSetSHA256', 'MethodId', 'TailPolicy', 'FinitePolicy')
RESERVED = {'CON', 'PRN', 'AUX', 'NUL', 'CONIN$', 'CONOUT$'} | {
    f'{p}{n}' for p in ('COM', 'LPT') for n in list('0123456789') + ['¹', '²', '³']}


class Refused(ValueError):
    pass


def require(condition, message):
    if not condition:
        raise Refused(message)


def sha(data):
    return hashlib.sha256(data).hexdigest()


def canonical(value):
    return json.dumps(value, sort_keys=True, separators=(',', ':'), ensure_ascii=True, allow_nan=False).encode('ascii') + b'\n'


def unique_object(pairs):
    result = {}
    for key, value in pairs:
        require(key not in result, 'Duplicate JSON key: ' + key)
        result[key] = value
    return result


def strict_json(data):
    require(0 < len(data) <= MAX_JSON, 'Empty or oversized JSON')
    try:
        def no_constant(value):
            raise Refused('Nonfinite JSON token: ' + value)
        value = json.loads(data.decode('utf-8'), object_pairs_hook=unique_object, parse_constant=no_constant)
        # Also reject escaped unpaired surrogates accepted by Python's JSON parser.
        json.dumps(value, ensure_ascii=False, allow_nan=False).encode('utf-8')
        return value
    except (UnicodeError, json.JSONDecodeError, ValueError, OverflowError) as error:
        raise Refused('Invalid strict UTF-8 JSON') from error


def token(value, description):
    require(isinstance(value, str) and re.fullmatch(r'[A-Za-z0-9][A-Za-z0-9._-]{0,127}', value), description)
    return value


def hex_sha(value):
    require(isinstance(value, str) and re.fullmatch(r'[0-9a-f]{64}', value), 'Missing/invalid SHA256')
    return value


def component(name):
    require(isinstance(name, str) and name not in ('', '.', '..'), 'Invalid path component')
    require(not name.endswith((' ', '.')) and not any(c in name for c in ':<>"|?*\\/') and
            all(ord(c) >= 32 for c in name), 'Windows alias/ADS/path syntax refused')
    require(name.split('.')[0].upper() not in RESERVED, 'Windows device name refused')
    return name


def relative_component_path(value):
    require(isinstance(value, str) and not PureWindowsPath(value).is_absolute() and not PureWindowsPath(value).drive,
            'Runtime component must be relative')
    parts = re.split(r'[\\/]', value)
    for item in parts:
        component(item)
    return Path(*parts)


def no_reparse(path):
    info = path.lstat()
    require(not stat.S_ISLNK(info.st_mode) and not (getattr(info, 'st_file_attributes', 0) & 0x400),
            'Symlink/junction/reparse point refused')
    return info


@contextmanager
def locked_path(path, directory=False):
    """Windows deny-delete directory / deny-write-and-delete file handle.

    Directory ACLs must already be trusted; no existing ACL is changed. No
    execution on other platforms is qualified by the filesystem-only tests.
    """
    require(os.name == 'nt', 'Approved execution currently requires Windows')
    import msvcrt
    from ctypes import wintypes
    info = no_reparse(path)
    require(stat.S_ISDIR(info.st_mode) if directory else stat.S_ISREG(info.st_mode), 'Wrong filesystem object type')
    kernel = ctypes.WinDLL('kernel32', use_last_error=True)
    create = kernel.CreateFileW
    create.argtypes = [wintypes.LPCWSTR, wintypes.DWORD, wintypes.DWORD, wintypes.LPVOID,
                       wintypes.DWORD, wintypes.DWORD, wintypes.HANDLE]
    create.restype = wintypes.HANDLE
    close = kernel.CloseHandle
    close.argtypes = [wintypes.HANDLE]
    close.restype = wintypes.BOOL
    handle = create(str(path), 0x80 if directory else 0x80000000, 3 if directory else 1,
                    None, 3, 0x02000000 if directory else 0, None)
    if handle == wintypes.HANDLE(-1).value:
        raise ctypes.WinError(ctypes.get_last_error())
    if directory:
        try:
            no_reparse(path)
            yield None
        finally:
            close(handle)
    else:
        try:
            descriptor = msvcrt.open_osfhandle(handle, os.O_RDONLY | os.O_BINARY)
        except BaseException:
            close(handle)
            raise
        with os.fdopen(descriptor, 'rb') as stream:
            current = no_reparse(path)
            opened = os.fstat(stream.fileno())
            require((current.st_dev, current.st_ino) == (opened.st_dev, opened.st_ino), 'Input path identity changed')
            yield stream


@contextmanager
def locked_directory_chain(path, trust_anchor=None):
    path = Path(os.path.abspath(path))
    require(not str(path).startswith(('\\\\', '//')) and path.is_absolute(), 'UNC/device/relative roots refused')
    # Inspect all ancestors for aliases, but do not request handles on unrelated
    # protected profile/system directories. Their ACLs are a trusted boundary.
    current = Path(path.anchor)
    no_reparse(current)
    for name in path.parts[1:]:
        component(name)
        current /= name
        no_reparse(current)
    anchor = Path(trust_anchor) if trust_anchor is not None else ROOT if path.is_relative_to(ROOT) else path
    require(path.is_relative_to(anchor), 'Directory escaped explicit trust anchor')
    with ExitStack() as stack:
        current = anchor
        stack.enter_context(locked_path(current, directory=True))
        for name in path.relative_to(anchor).parts:
            current /= name
            stack.enter_context(locked_path(current, directory=True))
        yield path


def exclusive_bytes(path, data):
    """Retain partial bytes on failure; never truncate, remove or retry a path."""
    flags = os.O_CREAT | os.O_EXCL | os.O_WRONLY | getattr(os, 'O_BINARY', 0)
    descriptor = os.open(path, flags, 0o600)
    with os.fdopen(descriptor, 'wb') as stream:
        stream.write(data)
        stream.flush()
        os.fsync(stream.fileno())


def publish_complete(directory, name, data):
    """Complete retained staging inode -> atomic no-replace NTFS hard link.

    Both names remain. No rename, deletion, symlink, fallback copy or overwrite.
    Unsupported hard links/permissions fail closed, preserving the staging bytes.
    """
    component(name)
    require(Path(name).name == name, 'Publication must be one fixed basename')
    with locked_directory_chain(directory):
        pending = directory / ('pending-' + uuid.uuid4().hex + '.bin')
        exclusive_bytes(pending, data)
        os.link(pending, directory / name)
    return directory / name


def strict_dcf(data):
    try:
        text = data.decode('ascii')
    except UnicodeError as error:
        raise Refused('DCF must be ASCII') from error
    require(text.endswith('\n') and '\r' not in text, 'Canonical LF-terminated DCF required')
    lines = text[:-1].split('\n')
    require(len(lines) == len(DCF_FIELDS), 'DCF must contain exactly one complete record')
    record = {}
    for expected, line in zip(DCF_FIELDS, lines):
        require(line.startswith(expected + ': '), 'Wrong/duplicate/missing DCF field')
        value = line[len(expected) + 2:]
        token(value, 'Empty/multiline/non-token DCF value')
        record[expected] = value
    return record


def make_dcf(record):
    data = ''.join(key + ': ' + token(record[key], 'Unsafe DCF value') + '\n' for key in DCF_FIELDS).encode('ascii')
    require(strict_dcf(data) == record, 'DCF round-trip failed')
    return data


def load_case(manifest, case_id):
    require(manifest.get('schema') == '04A02-oracle-source-proposal-v2', 'Only the v2 proposal is supported')
    require(set(manifest.get('release_gates', {})) == GATES and
            all(v is False for v in manifest['release_gates'].values()), 'Proposal release-state corruption')
    matches = [case for case in manifest['cases'] if case['case_id'] == case_id]
    require(len(matches) == 1, 'Missing/duplicate case')
    case = matches[0]
    require(case_id in ('C30-SYN-ANOVA', 'C30-SYN-KW'), 'Unapproved case identity')
    fixture_path = ROOT / 'BESHStat.Excel.Common.Tests/TestData/04A02' / (case_id + '.json')
    raw = fixture_path.read_bytes()
    require(sha(raw) == case['fixture_file_sha256'], 'Frozen fixture hash drift; no repinning')
    fixture = strict_json(raw)
    require(fixture['expected_numerical_reference'] is None and fixture['reference_status'] == 'NOT_APPROVED' and
            fixture['live_control_capture'] is False, 'Fixture promotion/capture refused')
    for pin in fixture['source_pins']:
        require(sha((ROOT / pin['path']).read_bytes()) == pin['sha256'], 'Pinned source hash drift')
    require(fixture['dataset_sha256'] == case['dataset_sha256'], 'Dataset hash drift')
    return case, fixture


def validate_decision(decision, manifest, case, manifest_data):
    require(decision.get('schema') == '04A02-owner-reference-decision-v2' and
            decision.get('status') == 'OWNER_AUTHORIZED_REFERENCE_EXECUTION', 'Absent/pending execution decision')
    require(decision.get('scientific_method_approved') is True, 'Scientific method decision required independently')
    gates = decision.get('release_gates', {})
    require(set(gates) == GATES and gates['run_independent_reference'] is True and
            gates['promote_numerical_golden'] is False and gates['native_workbook_capture'] is False,
            'Execution, promotion and native gates must remain independent')
    for key in ('decision_id', 'domain_owner_decision_id', 'roadmap_owner_execution_decision_id', 'tolerance_policy_id'):
        token(decision.get(key), 'Missing separately recorded owner/domain decision')
    require(decision.get('case_id') == case['case_id'], 'Wrong decision case')
    require(decision.get('proposal_manifest_sha256') == sha(manifest_data), 'Wrong proposal manifest SHA')
    require(hex_sha(decision.get('runner_sha256')) == manifest['runner_sha256'] == sha(Path(__file__).read_bytes()), 'Wrong runner SHA')
    require(hex_sha(decision.get('script_sha256')) == case['script_sha256'], 'Wrong source script SHA')
    require(decision.get('r_version') == decision.get('stats_version') == '4.5.1', 'Wrong approved runtime/version')
    for key in ('method_id', 'tail_policy', 'finite_policy'):
        require(decision.get(key) == case[key], 'Wrong scientific policy: ' + key)
    require(decision.get('runtime_closure_reviewed') is True, 'Runtime dependency closure needs independent review')
    require(decision.get('output_acl_reviewed') is True, 'Trusted private output-root ACL review required')
    hex_sha(decision.get('rscript_sha256'))
    components = decision.get('runtime_components')
    require(isinstance(components, list) and components, 'Missing runtime hashes')
    seen = set()
    for entry in components:
        path = relative_component_path(entry['relative_path']).as_posix()
        require(path.casefold() not in seen, 'Runtime path alias/duplicate')
        seen.add(path.casefold())
        hex_sha(entry['sha256'])
    require('bin/x64/r.dll' in seen, 'Pinned R.dll required; additional closure is owner-reviewed')
    bounds = decision.get('ordinary_p_range', {})
    for value in bounds.values():
        require(type(value) in (int, float) and math.isfinite(value), 'Missing finite ordinary-p policy')
    require(set(bounds) == {'min', 'max'} and 0 < bounds['min'] < bounds['max'] < 1,
            'Extreme-tail policy not approved')
    require(decision.get('extreme_policy') == 'reject-outside-approved-ordinary-range', 'Wrong extreme-tail policy')
    tolerances = decision.get('tolerances')
    require(isinstance(tolerances, dict) and set(tolerances) == set(case['tolerance_classes']), 'Missing case-specific tolerances')
    for rule in tolerances.values():
        require(isinstance(rule, dict) and set(rule) == {'atol', 'rtol', 'rationale'}, 'Incomplete tolerance rationale')
        for key in ('atol', 'rtol'):
            require(type(rule[key]) in (int, float) and math.isfinite(rule[key]) and rule[key] >= 0, 'Invalid tolerance')
        require(isinstance(rule['rationale'], str) and rule['rationale'].strip(), 'Tolerance rationale required')


def validate_candidate(stdout, stderr, decision, case, fixture, pins):
    require(stderr == b'', 'Any stderr/warning/log prevents publication')
    require(stdout.endswith(b'\n') and stdout.count(b'\n') == 1, 'Partial/polluted stdout')
    body = stdout[:-2] if stdout.endswith(b'\r\n') else stdout[:-1]
    require(body.startswith(b'{') and body.endswith(b'}'), 'Exactly one JSON object and no stdout logs required')
    value = strict_json(body)
    require(set(value) == {'schema', 'comparison', 'provenance'} and value['schema'] == '04A02-independent-candidate-v2', 'Wrong output schema')
    comparison = value['comparison']
    expected = dict(case_id=case['case_id'], status='PENDING_REVIEW_NOT_A_GOLDEN',
                    source_commit=fixture['source_commit'], dataset_sha256=fixture['dataset_sha256'],
                    dataset=fixture['dataset'], row_id_transport='provenance-only', alpha=0.05,
                    seed='N/A', culture='InvariantCulture/C', options=fixture['options'],
                    method_id=case['method_id'], tail_policy=case['tail_policy'])
    require(set(comparison) == set(expected) | {'metrics'}, 'Wrong/extra comparison fields')
    for key, wanted in expected.items():
        require(type(comparison[key]) is type(wanted) and comparison[key] == wanted, 'Wrong case/data/options: ' + key)
    # JSON 101.0 must not alias an integer provenance ID; bool must not alias 1.
    for group in comparison['dataset']['row_ids']:
        require(all(type(v) is int and -(2**31) <= v < 2**31 for v in group), 'RowIds are exact Int32 provenance only')
    for group in comparison['dataset']['values']:
        require(all(type(v) in (int, float) and math.isfinite(v) for v in group), 'Boolean/nonfinite data alias refused')
    require(all(type(v) is bool and v is False for v in comparison['options'].values()), 'Unexpected optional comparison')
    metrics = comparison['metrics']
    require(set(metrics) == set(case['metric_fields']), 'Distinct metric names/shapes required')
    for key, number in metrics.items():
        if key == 'rank_sums':
            require(isinstance(number, list) and len(number) == 3 and all(type(n) in (int, float) and math.isfinite(n) for n in number), 'Invalid rank array')
        else:
            require(type(number) in (int, float) and math.isfinite(number), 'Nonfinite/wrong metric type: ' + key)
            require(number >= 0, 'Negative statistic/df/sum: ' + key)
    if 'tie_correction' in metrics:
        require(0 < metrics['tie_correction'] <= 1, 'Invalid tie correction')
    for key in case['p_fields']:
        require(decision['ordinary_p_range']['min'] <= metrics[key] <= decision['ordinary_p_range']['max'], 'Unapproved extreme/boundary p')
    for key, wanted in case['df_fields'].items():
        require(type(metrics[key]) is int and metrics[key] == wanted, 'Wrong exact df')
    provenance = value['provenance']
    expected_provenance = dict(decision_id=decision['decision_id'], r_version=decision['r_version'],
        stats_version=decision['stats_version'], script_version='proposal-2', script_sha256=pins['script_sha256'],
        rscript_sha256=pins['rscript_sha256'], runtime_set_sha256=pins['runtime_set_sha256'])
    require(provenance == expected_provenance, 'Runtime/version/hash/decision attestation mismatch')
    return value


def run_reference(decision_path, manifest_path, r_home, evidence_root, run_name):
    """Future approved use only. Tests mock subprocess.run; no current R release."""
    token(run_name, 'Simple new execution name required')
    component(run_name)
    manifest_data = manifest_path.read_bytes()
    manifest = strict_json(manifest_data)
    decision_data = decision_path.read_bytes()
    decision = strict_json(decision_data)
    case, fixture = load_case(manifest, decision.get('case_id'))
    validate_decision(decision, manifest, case, manifest_data)
    script = ROOT / case['script']
    require(script.parent == ROOT / 'docs/architecture/04A02/oracle-proposals' and script.name.endswith('-v2.R'), 'Only pinned v2 source accepted')
    r_home = Path(os.path.abspath(r_home))
    executable = r_home / 'bin/x64/Rscript.exe'
    job = evidence_root / run_name
    with ExitStack() as stack:
        stack.enter_context(locked_directory_chain(evidence_root))
        stack.enter_context(locked_directory_chain(r_home / 'bin/x64', r_home))
        inputs = [(script, case['script_sha256'], 'script_sha256'),
                  (executable, decision['rscript_sha256'], 'rscript_sha256'),
                  (Path(__file__), decision['runner_sha256'], 'runner_sha256')]
        inputs += [(r_home / relative_component_path(c['relative_path']), c['sha256'], c['relative_path']) for c in decision['runtime_components']]
        pins = {}
        script_bytes = None
        for path, expected, name in inputs:
            stack.enter_context(locked_directory_chain(path.parent, r_home if path.is_relative_to(r_home) else ROOT))
            stream = stack.enter_context(locked_path(path))
            actual = hashlib.file_digest(stream, 'sha256').hexdigest()
            require(actual == expected, 'Actual preflight SHA mismatch: ' + name)
            pins[name] = actual
            if name == 'script_sha256':
                stream.seek(0)
                script_bytes = stream.read()
        runtime = {c['relative_path']: pins[c['relative_path']] for c in decision['runtime_components']}
        pins['runtime_set_sha256'] = sha(canonical(runtime))
        # Atomic namespace ownership; no pre-existing execution directory is reused.
        job.mkdir()
        stack.enter_context(locked_path(job, directory=True))
        dcf = make_dcf(dict(zip(DCF_FIELDS, [decision['decision_id'], case['case_id'], decision['r_version'],
            decision['stats_version'], pins['script_sha256'], pins['rscript_sha256'], pins['runtime_set_sha256'],
            case['method_id'], case['tail_policy'], case['finite_policy']])))
        exclusive_bytes(job / 'approved-input.dcf', dcf)
        exclusive_bytes(job / 'decision.json', decision_data)
        exclusive_bytes(job / 'proposal-manifest.json', manifest_data)
        exclusive_bytes(job / 'proposal-source.R', script_bytes)
        exclusive_bytes(job / 'preflight.json', canonical(dict(status='HASH_VERIFIED_NOT_GOLDEN', pins=pins,
            decision_sha256=sha(decision_data), manifest_sha256=sha(manifest_data))))
        try:
            with locked_path(job / 'approved-input.dcf'), locked_path(job / 'proposal-source.R'):
                out_fd = os.open(job / 'raw-stdout.bin', os.O_CREAT | os.O_EXCL | os.O_WRONLY | os.O_BINARY, 0o600)
                with os.fdopen(out_fd, 'wb') as stdout:
                    err_fd = os.open(job / 'raw-stderr.bin', os.O_CREAT | os.O_EXCL | os.O_WRONLY | os.O_BINARY, 0o600)
                    with os.fdopen(err_fd, 'wb') as stderr:
                        env = {key: os.environ[key] for key in ('SystemRoot', 'WINDIR', 'COMSPEC') if key in os.environ}
                        env.update(R_HOME=str(r_home), PATH=str(r_home / 'bin/x64'), TEMP=str(job), TMP=str(job), LC_ALL='C')
                        result = subprocess.run([str(executable), '--vanilla', str(job / 'proposal-source.R'), str(job / 'approved-input.dcf')],
                            cwd=job, env=env, stdin=subprocess.DEVNULL, stdout=stdout, stderr=stderr, timeout=30,
                            shell=False, creationflags=getattr(subprocess, 'CREATE_NO_WINDOW', 0))
                        stdout.flush(); os.fsync(stdout.fileno())
                        stderr.flush(); os.fsync(stderr.fileno())
            require(result.returncode == 0, 'Reference process failed')
            require((job / 'raw-stdout.bin').stat().st_size <= MAX_JSON and (job / 'raw-stderr.bin').stat().st_size == 0, 'Output limit/stderr failure')
            raw = (job / 'raw-stdout.bin').read_bytes()
            candidate = validate_candidate(raw, (job / 'raw-stderr.bin').read_bytes(), decision, case, fixture, pins)
            output = canonical(dict(schema='04A02-operator-candidate-v2', status='CANDIDATE_ONLY_NOT_GOLDEN',
                candidate=candidate, preflight=pins, decision_sha256=sha(decision_data), raw_stdout_sha256=sha(raw)))
            publish_complete(job, 'candidate.json', output)
            receipt = dict(schema='04A02-publication-receipt-v2', status='COMPLETE_CANDIDATE_ONLY',
                           candidate_sha256=sha(output), raw_stdout_sha256=sha(raw), raw_stderr_sha256=sha(b''))
            publish_complete(job, 'publication.json', canonical(receipt))
            return job
        except BaseException as error:
            # Retain raw, pending, partial and complete files, including failed links.
            try:
                exclusive_bytes(job / ('failure-' + uuid.uuid4().hex + '.json'), canonical(dict(status='NOT_PUBLISHED', error_type=type(error).__name__, message=str(error))))
            except OSError:
                pass  # Even ENOSPC/ACL denial must not trigger cleanup or overwrites.
            raise


def read_publication(job):
    require(no_reparse(job) is not None, 'Invalid publication directory')
    for name in ('candidate.json', 'publication.json', 'raw-stdout.bin', 'raw-stderr.bin'):
        no_reparse(job / name)
    receipt = strict_json((job / 'publication.json').read_bytes())
    candidate = (job / 'candidate.json').read_bytes()
    require(set(receipt) == {'schema', 'status', 'candidate_sha256', 'raw_stdout_sha256', 'raw_stderr_sha256'} and
            receipt.get('schema') == '04A02-publication-receipt-v2' and receipt.get('status') == 'COMPLETE_CANDIDATE_ONLY', 'Missing completion receipt')
    require(sha(candidate) == receipt['candidate_sha256'] and sha((job / 'raw-stdout.bin').read_bytes()) == receipt['raw_stdout_sha256'] and
            receipt['raw_stderr_sha256'] == sha(b'') and (job / 'raw-stderr.bin').read_bytes() == b'', 'Published file integrity mismatch')
    value = strict_json(candidate)
    require(value.get('schema') == '04A02-operator-candidate-v2' and value.get('status') == 'CANDIDATE_ONLY_NOT_GOLDEN' and
            value['raw_stdout_sha256'] == receipt['raw_stdout_sha256'], 'No golden promotion is supported')
    return value


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--execute-approved-reference', action='store_true')
    parser.add_argument('--decision', type=Path, required=True)
    parser.add_argument('--manifest', type=Path, default=ROOT / 'docs/architecture/04A02/oracle-proposals/manifest-v2.json')
    parser.add_argument('--r-home', type=Path)
    parser.add_argument('--evidence-root', type=Path)
    parser.add_argument('--name')
    args = parser.parse_args()
    require(args.execute_approved_reference, 'No execution by default; separate affirmative owner authorization required')
    require(args.r_home is not None and args.evidence_root is not None and args.name is not None, 'Explicit approved paths required')
    root = args.evidence_root.absolute()
    require(root.parent == ROOT / 'TestResults' and root.name.startswith('04A-02-initial-fixtures-') and
            (root / 'start.json').is_file() and (root / 'historical-metadata.json').is_file(), 'Fresh protected 04A.02 evidence root required')
    run_reference(args.decision, args.manifest, args.r_home, root, args.name)


if __name__ == '__main__':
    main()
