"""C5 fail-closed whole-body/source/context/actual-consumer reader. Never READY."""
import hashlib
import json
import re
from pathlib import Path

ROOT = Path(__file__).resolve().parents[5]
HERE = Path(__file__).resolve().parent
PREFIX = HERE.relative_to(ROOT).as_posix() + '/'
PRIOR = 'evidence/04A38/completion/w2/continuation-v4/'
# Filled from the completed, reviewed dossier by seal_review.py before execution.
REVIEW_SHA256 = '049d6a37d7567587b42a4c4a7caec37f224292f46870c0661ab3dac2fcbb8e45'


def need(value, message):
    if not value:
        raise ValueError(message)


def sha(data):
    return hashlib.sha256(data).hexdigest()


def semantic(method):
    return {k: v for k, v in method.items() if k != 'raw_IL_sha256'}


def tokens(source):
    return [s for s in re.findall(r'"(?:""|[^"])*"|\x27[^\n]*|[A-Za-z_][A-Za-z_0-9]*|\d+(?:\.\d+)?|[^\s]', source)
            if not s.startswith("'")]


def body(source):
    lines = source.splitlines()
    start = next(i for i, line in enumerate(lines) if ') As ' in line and not line.rstrip().endswith(','))
    return '\n'.join(lines[start + 1:-1])


def inspect(root=ROOT, overrides=None):
    root = Path(root)
    overrides = overrides or {}
    def raw(path):
        return overrides[path] if path in overrides else (root / path).read_bytes()
    def load(path):
        return json.loads(raw(path))
    def text(path):
        return raw(path).decode('utf-8-sig').replace('\r\n', '\n')
    def member(source, name, expected):
        need(source.count(expected) == 1, 'Reviewed complete source body missing/changed: ' + name)
    review_bytes = raw(PREFIX + 'body-contracts-v2.json')
    need(sha(review_bytes) == REVIEW_SHA256, 'Unreviewed/rewritten body contract dossier')
    review = json.loads(review_bytes)['contracts']
    historical = load(PRIOR + 'historical-ownership.json')['members']
    original_map = {r['source']['path']: r['snapshot'] for r in load('evidence/04A38/preedit/entry-source-snapshot.json')['files']}
    seen = set()
    for row in review:
        identity = row['original_id']
        need(identity not in seen, 'Duplicate reviewed identity')
        seen.add(identity)
        old = next(h for h in historical if h['canonical_original_id'] == identity)
        need(row['new_support'] == (not bool(old['proof'])), 'Recounted inherited review')
        need(row['path'] == old['path'] and row['name'] == old['name'], 'Canonical source/member mapping changed')
        snapshot = original_map[row['path']]
        need(snapshot == row['original_snapshot'] and sha(raw(snapshot['path'])) == snapshot['sha256'], 'Original source dependency changed')
        member(text(snapshot['path']), row['name'], row['original_whole_body'])
        member(text(row['path']), row['name'], row['current_whole_body'])
        for portable in row['portable_bodies']:
            need(sha(raw(portable['path'])) == portable['source_pin']['sha256'], 'Portable source dependency changed')
            member(text(portable['path']), portable['name'], portable['whole_body'])
        need(row['disposition'] == 'REVIEWED_COMPLETE_SOURCE_COMPOSITION_WITH_ACTUAL_CIL_AND_RESOLVED_DEPENDENCIES', 'Unsupported host-only classification')
        need(row['review'] and row['branch_proof'] and row['exception_byref'], 'Missing whole-body/branch/ByRef proof')
    supported = {h['canonical_original_id'] for h in historical if h['proof']} | seen
    for row in review:
        need(set(row['dependency_ids']) <= supported, 'Unresolved source dependency')
    byname = {r['name']: r for r in review}
    cell = byname['CellToTrimmedText']
    original = body(cell['original_whole_body'])
    prefix = 'If ExcelArgPredicates.IsMissingArg(v) Then Return ""'
    need(tokens(original)[0:len(tokens(prefix))] == tokens(prefix), 'Original missing-value guard changed')
    need(tokens(original.replace('CStr(v)', 'DirectCast(v, String)'))[len(tokens(prefix)):] == tokens(body(cell['portable_bodies'][0]['whole_body'])), 'Cell conversion complete operation order changed')
    numeric = byname['TryGetDouble']
    original = body(numeric['original_whole_body'])
    old_kernel = body(numeric['portable_bodies'][0]['whole_body'])
    expected = original.replace(old_kernel.strip(), 'Return Global.BESHStatNG.ExcelCommon.UdfCellConversion38.ConvertFiniteNumber(v)')
    # Ignore indentation only; every native guard, Try/Catch and return remains.
    if tokens(expected) != tokens(body(numeric['current_whole_body'])):
        old_tokens = tokens(original)
        kernel_tokens = tokens(old_kernel)
        pos = next((i for i in range(len(old_tokens)) if old_tokens[i:i+len(kernel_tokens)] == kernel_tokens), None)
        need(pos is not None, 'Original numeric kernel missing')
        expected_tokens = old_tokens[:pos] + tokens('Return Global.BESHStatNG.ExcelCommon.UdfCellConversion38.ConvertFiniteNumber(v)') + old_tokens[pos+len(kernel_tokens):]
        need(expected_tokens == tokens(body(numeric['current_whole_body'])), 'Native numeric catch/guard sequencing changed')
    config_records = []
    allowed_changed = {('BESHStatNG.WorksheetFunctions.ExcelArgReaders','CellToTrimmedText'),
                       ('BESHStatNG.WorksheetFunctions.ExcelArgNumeric','TryGetDouble'),
                       ('BESHStatNG.UdfOutputTables','IsNumericValue')}
    for config in ['Debug', 'Release']:
        n = load(PREFIX + 'inspection/final-v3/MethodMetadata38-' + config + '.json')
        prior = load(PRIOR + 'metadata-v6/parametric-v4/current-' + config + '.json')
        o = load(PREFIX + 'inspection/final-v3/original-' + config + '.json')
        old_original = load(PRIOR + 'metadata-v6/parametric-v4/original-' + config + '.json')
        need(o['assemblies'] == old_original['assemblies'] and o['methods'] == old_original['methods'] and o['types'] == old_original['types'], 'Original metadata/source/IL identity changed')
        nm = {m['method']: m for m in n['methods']}
        nt = {t['type']: t for t in n['types']}
        retained = 0
        for method in prior['methods']:
            if (method['declaring_type'], method['member_name']) in allowed_changed:
                continue
            need(method['method'] in nm and semantic(method) == semantic(nm[method['method']]), 'Unreviewed compiled body/dependency changed: ' + method['method'])
            retained += 1
        for context in prior['types']:
            need(context == nt.get(context['type']), 'Retained fields/type/initializer context changed')
        portable = [m for m in n['methods'] if m['declaring_type'] == 'BESHStatNG.ExcelCommon.UdfCellConversion38']
        need(len(portable) == 4 and all(not m['exceptions'] and not m['type_initializer'] and not m['nonliteral_static_fields'] for m in portable), 'New conversion has hidden catch/static context')
        def lookup(owner, name):
            methods = [m for m in n['methods'] if m['declaring_type'] == owner and m['member_name'] == name]
            need(len(methods) == 1, 'Ambiguous/missing compiled method ' + owner + '.' + name)
            return methods[0]
        consumer_edges = []
        for owner, name, target in [('BESHStatNG.WorksheetFunctions.ExcelArgReaders','CellToTrimmedText','CellToTrimmedText'),
                                    ('BESHStatNG.WorksheetFunctions.ExcelArgNumeric','TryGetDouble','ConvertFiniteNumber'),
                                    ('BESHStatNG.UdfOutputTables','IsNumericValue','IsNumericTypeCode')]:
            method = lookup(owner, name)
            target_method = lookup('BESHStatNG.ExcelCommon.UdfCellConversion38', target)
            need('METHOD:' + target_method['method'] in [i['operand'] for i in method['instructions']], 'Missing actual Windows-to-Common consumer')
            consumer_edges.append(dict(caller=method['method'], target=target_method['method']))
        method = lookup('BESHStatNG.WorksheetFunctions.ExcelArgNumeric','TryGetDouble')
        original_method = next(m for m in o['methods'] if m['method'] == method['method'])
        need(len(method['exceptions']) == len(original_method['exceptions']) == 1, 'Native real conversion catch missing')
        helpers = lambda m: [i['operand'] for i in m['instructions'] if isinstance(i['operand'], str) and 'Microsoft.VisualBasic.CompilerServices.ProjectData @ Microsoft.VisualBasic,' in i['operand']]
        need(helpers(method) == helpers(original_method) and len(helpers(method)) == 2, 'Framework Err helpers changed')
        index = next(i for i, instruction in enumerate(method['instructions']) if '.ConvertFiniteNumber(' in str(instruction['operand']))
        need(method['exceptions'][0]['try_start'] <= index < method['exceptions'][0]['try_end'], 'Conversion moved outside native catch')
        # Preserve the actual original boxed-value copy count, not merely outputs.
        copy_calls = lambda m: sum('.GetObjectValue(' in str(i['operand']) for i in m['instructions'])
        portable_number = lookup('BESHStatNG.ExcelCommon.UdfCellConversion38','ConvertFiniteNumber')
        need(copy_calls(original_method) == 1 and copy_calls(method) == 0 and copy_calls(portable_number) == 1, 'Unintended boxed-value copy introduced')
        for pure in portable:
            need(not any('Microsoft.VisualBasic.' in str(i['operand']) for i in pure['instructions']), 'New portable VB conversion helper introduced')
            if pure['member_name'] in ['ConvertFiniteNumber','CellToTrimmedText']:
                need(pure['parameter_types'] == ['System.Object&'] and not any(i['op'] in ['stind.ref','starg','starg.s'] for i in pure['instructions']), 'Portable input alias mutated')
        expanded_old = load(PREFIX + 'dependency-metadata-v7/original-' + config + '.json')
        expanded_new = load(PREFIX + 'dependency-metadata-v8/current-' + config + '.json')
        em = {m['method']: m for m in expanded_new['methods']}
        protected_owners = ('BESHStatNG.AppInfrastructure.', 'BESHStatNG.PValuePresentation', 'BESHStatNG.ResultTable', 'BESHStatNG.parametric.Parametric+OneWayANOVA')
        accepted = [m for m in expanded_old['methods'] if m['declaring_type'].startswith(protected_owners)]
        need(accepted, 'Missing settings/engine/output dependency context')
        for method in accepted:
            need(method['method'] in em and semantic(method) == semantic(em[method['method']]), 'Settings/formatter/ANOVA/result dependency changed')
        et = {t['type']: t for t in expanded_new['types']}
        for context in expanded_old['types']:
            if context['type'].startswith(protected_owners):
                need(context == et.get(context['type']), 'Settings/engine type context changed')
        built = load(PREFIX + 'build-cells-v4-' + config + '.json')
        need(built['all_exit_zero'], 'Production build failed')
        for assembly in n['assemblies']:
            need(dict(name=assembly['file_name'], bytes=assembly['bytes'], sha256=assembly['sha256']) in built['assemblies'], 'Current assembly not bound to completed build')
        exports = load(PREFIX + 'inspection/final-v3/Inspect38-' + config + '.json')['exports']
        original_exports = load('evidence/04A38/preedit/compiled-' + config + '.json')['exports']
        key = lambda r: r['effective_excel_name']
        need(len(exports) == 268 and sorted(exports, key=key) == sorted(original_exports, key=key), 'Export signature/default/attribute altered')
        probe = load(PREFIX + 'cell-probe-v2-after-' + config + '.json')
        need(probe == load(PREFIX + 'cell-probe-v2-before-' + config + '.json') and len(probe['cases']) == 198 and probe['original_calls'] == 0, 'Current conversion/exception/ByRef probe drift')
        for role in ['original','current']:
            for assembly in (o if role == 'original' else n)['assemblies']:
                if role == 'original':
                    lib = root / '.test-rationalization-04A38.tmp/consolidated38-v7/original-build' / config / assembly['file_name']
                else:
                    lib = root / '.test-rationalization-04A38.tmp/completion-w2-cont5/cells-v4' / config / 'lib' / assembly['file_name']
                # Omitted DLLs in a compact copy are not claimed as freshly verified there.
                if lib.is_file():
                    need(sha(lib.read_bytes()) == assembly['sha256'], 'Actual compiled DLL identity changed')
        callbacks = [dict(method=m['method'], body=m) for m in n['methods'] if '_Lambda$' in m['member_name'] and m['declaring_type'].startswith('BESHStatNG.UdfDataImport')]
        config_records.append(dict(configuration=config, retained_full_methods=retained, protected_dependency_methods=len(accepted),
                                   actual_consumer_edges=consumer_edges, callback_bodies=len(callbacks), exports=268))
    # Recompare actual immutable output bytes, not a claimed all_exact flag.
    compatibility = load(PREFIX + 'compatibility-cells-v4.json')
    need(len(compatibility['results']) == 24 and compatibility['new_original_queries'] == 0, 'Compatibility provenance/population changed')
    counts = {}
    for row in compatibility['results']:
        need(raw(row['current']['path']) == raw(row['original']['path']) and sha(raw(row['original']['path'])) == row['original']['sha256'], 'Sealed current/original output mismatch')
        counts[row['configuration']] = counts.get(row['configuration'], 0) + row['cases']
    need(counts == dict(Debug=29424,Release=29424), 'Missing frozen compatibility cases')
    for row in load(PREFIX + 'source-transitions-v3.json')['files']:
        need(raw(row['path']) == raw(row['snapshot']['path']) and sha(raw(row['path'])) == row['after']['sha256'], 'Source transition successor changed')
        if row['before']:
            need(sha(raw(row['preimage']['path'])) == row['before']['sha256'], 'Source transition preimage changed')
    return dict(status='C5_WHOLE_BODY_PROOFS_SUPPORTED_NOT_READY', reviewed_contracts=len(review),
                newly_supported=sum(r['new_support'] for r in review), original_ids=sorted(seen), configurations=config_records,
                original_queries=0, W3='NOT_STARTED', full_scope='Only reviewed identities; other mandatory OPEN remain')


if __name__ == '__main__':
    try:
        print(json.dumps(inspect()))
    except Exception as error:
        print(json.dumps(dict(status='NOT_READY_04A38', error=type(error).__name__ + ': ' + str(error))))
        raise SystemExit(1)
