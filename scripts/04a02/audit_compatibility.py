"""Read-only 04A.02 explanation of historical audit failures, never an acceptance gate.

Original audit inputs/checkers are not imported or modified. Only exclusive new
04A.02 output paths may be written by the CLI. JSON meaning is parsed independently
with exact Decimal values; byte disagreement is retained even when meaning agrees.
"""
import argparse
import ast
from decimal import Decimal
from functools import lru_cache
import hashlib
import json
from pathlib import Path
import re
import subprocess

ROOT = Path(__file__).resolve().parents[2]
BASE = '12e25924aae93f0b36ab33937176c86c19641b50'
INVENTORY = 'ec5e47a0cfc352ea5fa9fcd598e1f10b5b8fbd9b'
OLD_BASE = '11fa5d99a040de2e62c75ec201c49852c1d6b289'
PRIOR = 'TestResults/04A-02-initial-fixtures-20261030T131010Z'
STORES = ('review-decisions.json', 'control-review-decisions.json', 'wiring-review-decisions.json')


def digest(data):
    return hashlib.sha256(data).hexdigest()


def blob_id(data):
    return hashlib.sha1(b'blob ' + str(len(data)).encode('ascii') + b'\0' + data).hexdigest()


def stats(data):
    return dict(bytes=len(data), sha256=digest(data), computed_git_blob_id=blob_id(data),
                crlf=data.count(b'\r\n'), bare_lf=data.count(b'\n') - data.count(b'\r\n'),
                bare_cr=data.count(b'\r') - data.count(b'\r\n'))


def meaning(value):
    """Typed canonical parse tree; no binary64/decimal precision aliases."""
    if value is None:
        return ['null']
    if isinstance(value, bool):
        return ['boolean', value]
    if isinstance(value, (int, Decimal)):
        sign, digits, exponent = Decimal(value).as_tuple()
        digits = list(digits)
        while len(digits) > 1 and digits[-1] == 0:
            digits.pop()
            exponent += 1
        if not any(digits):
            sign, digits, exponent = 0, [0], 0
        return ['number', sign, digits, exponent]
    if isinstance(value, str):
        return ['string', value]
    if isinstance(value, list):
        return ['array', [meaning(v) for v in value]]
    return ['object', [[key, meaning(value[key])] for key in sorted(value)]]


@lru_cache(maxsize=8)
def semantic_hash(data):
    def reject_constant(value):
        raise ValueError('Nonfinite JSON: ' + value)
    parsed = json.loads(data.decode('utf-8-sig'), parse_float=Decimal, parse_constant=reject_constant)
    return digest(json.dumps(meaning(parsed), ensure_ascii=True, separators=(',', ':')).encode('ascii'))


def newline_parts(data):
    parts = data.split(b'\n')
    bodies, endings = [], []
    for part in parts[:-1]:
        bodies.append(part[:-1] if part.endswith(b'\r') else part)
        endings.append(b'\r\n' if part.endswith(b'\r') else b'\n')
    bodies.append(parts[-1])
    endings.append(b'')
    return bodies, endings


def compare_bytes(original, current):
    first = next((i for i, (a, b) in enumerate(zip(original, current)) if a != b), None)
    if first is None and len(original) != len(current):
        first = min(len(original), len(current))
    old_bodies, old_endings = newline_parts(original)
    new_bodies, new_endings = newline_parts(current)
    same_bodies = old_bodies == new_bodies
    old_from_new = b''.join(body + end for body, end in zip(new_bodies, old_endings)) if same_bodies else None
    new_from_old = b''.join(body + end for body, end in zip(old_bodies, new_endings)) if same_bodies else None
    normalized_equal = original.replace(b'\r\n', b'\n') == current.replace(b'\r\n', b'\n')
    result = dict(original=stats(original), current=stats(current), byte_equal=original == current,
                  first_difference_offset=first, normalized_crlf_to_lf_equal=normalized_equal,
                  reconstruct_original_using_current_bodies=old_from_new == original,
                  reconstruct_current_using_original_bodies=new_from_old == current,
                  outcome='BYTE_IDENTICAL' if original == current else
                  'BYTE_MISMATCH_NEWLINE_ONLY' if normalized_equal and same_bodies else 'NON_NEWLINE_DRIFT_DETECTED')
    try:
        a, b = semantic_hash(original), semantic_hash(current)
        result.update(json_meaning='EQUAL' if a == b else 'DIFFERENT', original_semantic_sha256=a, current_semantic_sha256=b)
    except (ValueError, UnicodeError):
        result['json_meaning'] = 'NOT_VALID_JSON'
    return result


def source_status(data, expected_sha256):
    return 'SOURCE_HASH_MATCH' if digest(data) == expected_sha256 else 'SOURCE_HASH_DRIFT_DETECTED'


def literal_objects(data):
    text = data.decode('utf-8-sig')
    decoder = json.JSONDecoder(parse_float=Decimal)
    position = text.index('[') + 1
    result = []
    while True:
        while text[position].isspace() or text[position] == ',':
            position += 1
        if text[position] == ']':
            return result
        start = position
        item, position = decoder.raw_decode(text, position)
        key = next(item[name] for name in ('symbol_id', 'control_id', 'edge_id', 'signature_key') if name in item)
        result.append((key, text[start:position].encode('utf-8')))


def compare_prefix(original, current):
    old, new = literal_objects(original), literal_objects(current)
    new_by_id = dict(new)
    old_ids = [key for key, _ in old]
    present = all(key in new_by_id for key in old_ids)
    ordered = old_ids == [key for key, _ in new[:len(old)]]
    old_stream = b'[' + b','.join(raw for _, raw in old) + b']'
    new_stream = b'[' + b','.join(new_by_id.get(key, b'null') for key in old_ids) + b']'
    result = compare_bytes(old_stream, new_stream)
    result.update(prior_objects=len(old), current_objects=len(new), all_prior_ids_present=present,
                  original_order_preserved=ordered,
                  offset_basis='Constructed ordered raw-object comparison stream, not whole-file offset')
    if not present or not ordered:
        result['outcome'] = 'NON_NEWLINE_DRIFT_DETECTED'
    return result


def git(*arguments, binary=False):
    command = ['git', '-c', 'safe.directory=' + ROOT.as_posix(), *arguments]
    result = subprocess.run(command, cwd=ROOT, stdout=subprocess.PIPE, stderr=subprocess.PIPE)
    if result.returncode:
        return None
    return result.stdout if binary else result.stdout.decode('utf-8').strip()


@lru_cache(maxsize=96)
def pin(path):
    relative = path.relative_to(ROOT).as_posix()
    if not path.is_file():
        return dict(path=relative, availability='EVIDENCE_UNAVAILABLE')
    data = path.read_bytes()
    identity = git('rev-parse', 'HEAD:' + relative)
    record = dict(path=relative, availability='PRESENT', **stats(data), main_git_object=identity,
                  git_tracked_at_main=identity is not None)
    if identity:
        record['main_object_comparison'] = compare_bytes(git('cat-file', 'blob', identity, binary=True), data)
    return record


def emit(path, value):
    path.parent.mkdir(parents=True, exist_ok=True)
    with path.open('x', encoding='utf-8', newline='\n') as stream:
        json.dump(value, stream, indent=2)
        stream.write('\n')


def test_source(identity, suite):
    module, class_name, method = identity.rsplit('.', 2)
    module = suite.removeprefix('audit-').replace('-', '') if module == '__main__' else module
    mapping = {'reconciliation': 'test_reconciliation', 'continuation30': 'test_continuation30', 'ownercloseout': 'test_owner_closeout'}
    module = mapping.get(module, module)
    path = ROOT / 'scripts/windows-migration-audit' / (module + '.py')
    data = path.read_bytes()
    tree = ast.parse(data.decode('utf-8-sig'))
    cls = next(node for node in tree.body if isinstance(node, ast.ClassDef) and node.name == class_name)
    node = next(node for node in cls.body if isinstance(node, ast.FunctionDef) and node.name == method)
    return dict(file=pin(path), class_name=class_name, method=method, line=node.lineno, end_line=node.end_lineno)


def reconcile(run):
    """Explain retained failures; never execute or stand in for original checkers."""
    prior = ROOT / PRIOR
    required = ['audit-identities.json', 'audit-checks.json', 'audit-reconciliation.log', 'audit-continuation30.log', 'artifact-hashes.json']
    missing = [str(prior.relative_to(ROOT) / name) for name in required if not (prior / name).is_file()]
    local_inputs = [ROOT / 'scripts/windows-migration-audit' / name for name in STORES]
    local_inputs += [run / 'start.json', run / 'immutable-tracked-source.json']
    missing += [path.relative_to(ROOT).as_posix() for path in local_inputs if not path.is_file()]
    if missing:
        return dict(outcome='EVIDENCE_UNAVAILABLE', unavailable=missing, original_failures_reclassified_as_pass=False)
    start = json.loads((run / 'start.json').read_bytes())
    immutable = json.loads((run / 'immutable-tracked-source.json').read_bytes())
    product_prefixes = ('BESHStat.Core/', 'BESHStat.Excel.Common/', 'BESHstatNG/', 'BESHStat.OfficeAddin/')
    product = {p: source_status((ROOT / p).read_bytes(), h) if (ROOT / p).is_file() else 'SOURCE_HASH_DRIFT_DETECTED'
               for p, h in immutable.items() if p.startswith(product_prefixes)}
    if any(value != 'SOURCE_HASH_MATCH' for value in product.values()):
        return dict(outcome='NEW_PRODUCT_REGRESSION_DETECTED', source_hashes=product, stop_required=True)
    evidence_errors = []
    prior_manifest = json.loads((prior / 'artifact-hashes.json').read_bytes())['artifacts']
    for item in prior_manifest:
        path = ROOT / item['path']
        if not path.is_file() or path.stat().st_size != item['bytes'] or digest(path.read_bytes()) != item['sha256']:
            evidence_errors.append(item['path'])
    if evidence_errors:
        return dict(outcome='EVIDENCE_UNAVAILABLE', unavailable_or_altered=evidence_errors, original_failures_reclassified_as_pass=False)
    suites = json.loads((prior / 'audit-identities.json').read_bytes())
    rows = []
    comparisons = {}
    current_stores = {name: (ROOT / 'scripts/windows-migration-audit' / name).read_bytes() for name in STORES}
    for suite in suites:
        if not suite['failures']:
            continue
        log_path = prior / (suite['suite'] + '.log')
        log = log_path.read_text(encoding='utf-8')
        for failure in suite['failures']:
            identity = failure['identity']
            checkpoint = 30 if suite['suite'] == 'audit-continuation30' else int(re.search(r'Continuation(\d+)', identity)[1])
            marker = 'FAIL: ' + failure['method'] + ' (' + identity + ')'
            block = log[log.index(marker):]
            block = re.split(r'\n={10,}|\n-{10,}\nRan ', block, maxsplit=1)[0]
            assertion = re.search(r'AssertionError: ([\s\S]+)', block)
            assertion_text = assertion[1].strip() if assertion else 'ASSERTION TEXT UNAVAILABLE'
            strict = 'strict' in failure['method']
            item = dict(identity=identity, suite=suite['suite'], checkpoint=checkpoint,
                        original_test_outcome='FAILED', original_expected='exit 2' if strict else 'exact original bytes / empty byte-error list',
                        checkpoint01_actual=('exit 1' if '2 != 1' in assertion_text else 'see exact assertion') if strict else 'byte assertion failed',
                        assertion=assertion_text[:2200], full_assertion_block_sha256=digest(block.encode('utf-8')),
                        log=pin(log_path), source=test_source(identity, suite['suite']),
                        source_case_branch='codex/04A-01', source_case_commit=OLD_BASE,
                        current_branch=git('branch', '--show-current'), current_commit=git('rev-parse', 'HEAD'),
                        depends_on_local_only_evidence=True, comparisons=[])
            if strict:
                item['classification'] = 'ORIGINAL_SNAPSHOT_NOT_REPRODUCIBLE_IN_CURRENT_CHECKOUT'
                item['cause'] = 'Original coverage/source provenance include different HEAD/main/branch; exit 1 retained, not a pass'
                item['original_source_snapshots'] = [pin(ROOT / 'docs/architecture/WindowsMigrationLedger' / name) for name in ('coverage.json', 'source-provenance.json')]
                item['scratch_review_override'] = 'Original TemporaryDirectory override not retained; test source and assertion/log remain available'
            else:
                item['classification'] = 'ORIGINAL_SNAPSHOT_NOT_REPRODUCIBLE_IN_CURRENT_CHECKOUT'
                for name in STORES:
                    snapshot = ROOT / ('TestResults/04A-01-continuation-%02d/original/scripts/windows-migration-audit' % checkpoint) / name
                    if not snapshot.is_file():
                        item['comparisons'].append(dict(store=name, availability='EVIDENCE_UNAVAILABLE'))
                        continue
                    key = str(checkpoint) + '/' + name
                    if key not in comparisons:
                        original = snapshot.read_bytes()
                        current = current_stores[name]
                        references = {}
                        for ref in (OLD_BASE, INVENTORY, BASE):
                            oid = git('rev-parse', ref + ':scripts/windows-migration-audit/' + name)
                            references[ref] = dict(object_id=oid, comparison_to_current=None if oid is None else compare_bytes(git('cat-file', 'blob', oid, binary=True), current))
                        comparisons[key] = dict(store=name, original_snapshot=pin(snapshot), current_source=pin(ROOT / 'scripts/windows-migration-audit' / name),
                            full_file=compare_bytes(original, current), ordered_prefix=compare_prefix(original, current), git_objects=references,
                            checkout_attributes=git('check-attr', 'text', 'eol', '--', 'scripts/windows-migration-audit/' + name),
                            historical_store_commit_note='Snapshot was local audit work at old baseline; absence of this path at OLD_BASE is not missing sealed evidence')
                    comparison = comparisons[key]
                    item['comparisons'].append(key)
                    relevant = comparison['full_file'] if checkpoint == 30 else comparison['ordered_prefix']
                    if relevant['outcome'] == 'NON_NEWLINE_DRIFT_DETECTED' or relevant.get('json_meaning') == 'DIFFERENT':
                        item['classification'] = 'NON_NEWLINE_EVIDENCE_DRIFT_DETECTED'
                item['cause'] = 'Byte disagreement retained; independently check whole-file and ordered-prefix comparisons and bidirectional newline reconstruction'
            rows.append(item)
    office_changes = git('diff', '--name-only', INVENTORY, BASE).splitlines()
    outcome = 'ORIGINAL_SNAPSHOT_NOT_REPRODUCIBLE_IN_CURRENT_CHECKOUT'
    if any(row['classification'] == 'NON_NEWLINE_EVIDENCE_DRIFT_DETECTED' for row in rows):
        outcome = 'NON_NEWLINE_EVIDENCE_DRIFT_DETECTED'
    if len(rows) != 24 or any(isinstance(c, dict) for row in rows for c in row['comparisons']):
        outcome = 'EVIDENCE_UNAVAILABLE'
    return dict(schema='04A02-audit-compatibility-diagnostic-v1', outcome=outcome,
                is_original_strict_or_inventory_verifier=False, original_failures_reclassified_as_pass=False,
                historical_checker_outcomes=json.loads((prior / 'audit-checks.json').read_bytes()), source_baseline=start['head'],
                product_files_verified=len(product), product_source_hash_drift=[], checkpoint01_artifacts_verified=len(prior_manifest),
                expected_failed_identities=24, observed_failed_identities=len(rows), failed_cases=rows, byte_comparisons=comparisons,
                accepted_office_changes=office_changes, office_interpretation='Real accepted Office source/test changes, not just metadata. Strict Windows/Core/Common census excludes Office; provenance git-log output changes. No new edits to product source.',
                diagnostic_requires_owner_review=True)


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--evidence', required=True)
    parser.add_argument('--name', default='audit-compatibility-diagnostic')
    args = parser.parse_args()
    run = Path(args.evidence).resolve()
    if run.parent != ROOT / 'TestResults' or not run.name.startswith('04A-02-initial-fixtures-checkpoint02-'):
        raise ValueError('Only a fresh Checkpoint 02 evidence root is allowed')
    if not re.fullmatch(r'[A-Za-z0-9][A-Za-z0-9-]*', args.name):
        raise ValueError('Simple new diagnostic name required')
    source_sha256 = digest(Path(__file__).read_bytes())
    result = reconcile(run)
    result['diagnostic_source_sha256'] = source_sha256
    emit(run / (args.name + '.json'), result)
    print(result['outcome'], 'failed identities:', result.get('observed_failed_identities', 'UNAVAILABLE'))
    # 2 means explanatory incompatibility, explicitly NOT a green historical gate.
    return {'NEW_PRODUCT_REGRESSION_DETECTED': 1, 'EVIDENCE_UNAVAILABLE': 3,
            'NON_NEWLINE_EVIDENCE_DRIFT_DETECTED': 4}.get(result['outcome'], 2)


if __name__ == '__main__':
    raise SystemExit(main())
