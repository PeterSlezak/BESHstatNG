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
REVIEW_SHA256 = '0fb67738d14cc6acf059b5178d97b74c38aa6df3ab6cef738f86e8f4558354e1'


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
    review_bytes = raw(PREFIX + 'body-contracts-v3.json')
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
    phase_rows = load(PREFIX + 'prepared-phase-source-proof-inputs.json')['rows']
    need(len(phase_rows) == 5, 'Missing prepared phase source proof')
    for phase in phase_rows:
        need(sha(raw(phase['preimage']['path'])) == phase['preimage']['sha256'], 'Prepared phase preimage changed')
        need(sha(raw(phase['path'])) == phase['current_source']['sha256'], 'Prepared phase current source changed')
        old_body = body(phase['old_public_body'])
        api_body = body(phase['new_public_body'])
        prepared_body = body(phase['prepared_body'])
        marker = 'Return ' + phase['name'] + 'Prepared('
        need(marker in api_body, 'Public API does not consume prepared phase')
        prefix_body = api_body.rsplit(marker,1)[0]
        need(tokens(prefix_body + prepared_body) == tokens(old_body), 'Existing public portable API operation order changed')
        need(not re.search(r'^\s*(?:result|groupMatrices|groupNames|data|names)\s*=\s*(?:-1|0|defaultValue|Nothing)\s*$', prepared_body,re.M), 'Prepared phase repeats an entry reset')
    aliases = load(PREFIX + 'compiler-generated-aliases-final.json')['aliases']
    need(aliases == {'BESHStatNG.ExcelCommon.UdfNumericImportPreparation38+_Closure$__6-0':'BESHStatNG.ExcelCommon.UdfNumericImportPreparation38+_Closure$__8-0'}, 'Unsupported generated-name alias')
    def relocate(value):
        if isinstance(value,str):
            for old,new in aliases.items(): value=value.replace(old,new)
            return value
        if isinstance(value,list): return [relocate(v) for v in value]
        if isinstance(value,dict): return {k:relocate(v) for k,v in value.items()}
        return value
    config_records = []
    allowed_changed = {('BESHStatNG.WorksheetFunctions.ExcelArgReaders','CellToTrimmedText'),
                       ('BESHStatNG.WorksheetFunctions.ExcelArgNumeric','TryGetDouble'),
                       ('BESHStatNG.UdfOutputTables','IsNumericValue'),
                       *[('BESHStatNG.WorksheetFunctions.ExcelArgNumeric',n) for n in ['TryGetBinary01','TryGetWholeNumber','TryGetOptionalPositiveInteger']],
                       *[('BESHStatNG.UdfDataImport',n) for n in ['TryGetGroupedCompleteMatrix','TryGetNestedThreeColumnData']],
                       *[('BESHStatNG.ExcelCommon.UdfScalarChoice38',n) for n in ['TryBinary','TryWholeNumber','TryPositiveInteger']],
                       *[('BESHStatNG.ExcelCommon.UdfNumericImportPreparation38',n) for n in ['TryGetGroupedCompleteMatrix','TryGetNestedThreeColumnData']]}
    for config in ['Debug', 'Release']:
        n = load(PREFIX + 'inspection/final-v4/MethodMetadata38-' + config + '.json')
        prior = load(PRIOR + 'metadata-v6/parametric-v4/current-' + config + '.json')
        o = load(PREFIX + 'inspection/final-v4/original-' + config + '.json')
        old_original = load(PRIOR + 'metadata-v6/parametric-v4/original-' + config + '.json')
        need(o['assemblies'] == old_original['assemblies'] and o['methods'] == old_original['methods'] and o['types'] == old_original['types'], 'Original metadata/source/IL identity changed')
        nm = {m['method']: m for m in n['methods']}
        nt = {t['type']: t for t in n['types']}
        retained = 0
        for method in prior['methods']:
            if (method['declaring_type'], method['member_name']) in allowed_changed:
                continue
            mapped = relocate(method)
            need(mapped['method'] in nm and semantic(mapped) == semantic(nm[mapped['method']]), 'Unreviewed compiled body/dependency changed: ' + method['method'])
            retained += 1
        for context in prior['types']:
            mapped_context = relocate(context)
            need(mapped_context == nt.get(mapped_context['type']), 'Retained fields/type/initializer context changed')
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
        for owner,name,target in [
            ('BESHStatNG.WorksheetFunctions.ExcelArgNumeric','TryGetBinary01','TryBinaryPrepared'),
            ('BESHStatNG.WorksheetFunctions.ExcelArgNumeric','TryGetWholeNumber','TryWholeNumberPrepared'),
            ('BESHStatNG.WorksheetFunctions.ExcelArgNumeric','TryGetOptionalPositiveInteger','TryPositiveIntegerPrepared'),
            ('BESHStatNG.UdfDataImport','TryGetGroupedCompleteMatrix','TryGetGroupedCompleteMatrixPrepared'),
            ('BESHStatNG.UdfDataImport','TryGetNestedThreeColumnData','TryGetNestedThreeColumnDataPrepared')]:
            native=lookup(owner,name)
            portable_owner='BESHStatNG.ExcelCommon.UdfScalarChoice38' if owner.endswith('ExcelArgNumeric') else 'BESHStatNG.ExcelCommon.UdfNumericImportPreparation38'
            prepared=lookup(portable_owner,target)
            need('METHOD:'+prepared['method'] in [i['operand'] for i in native['instructions']], 'Native uses resetting API after callback/acquisition')
            need(not prepared['exceptions'] and not prepared['type_initializer'] and not prepared['nonliteral_static_fields'], 'Prepared phase has hidden context')
            if owner.endswith('ExcelArgNumeric'):
                parser='TryGetFiniteDouble' if name=='TryGetOptionalPositiveInteger' else 'TryGetDouble'
                call_index=next(i for i,op in enumerate(native['instructions']) if ('.'+parser+'(') in str(op['operand']))
                writes=[i for i,op in enumerate(native['instructions']) if op['op']=='stind.i4']
            else:
                call_index=next(i for i,op in enumerate(native['instructions']) if '.Get2D(' in str(op['operand']))
                writes=[i for i,op in enumerate(native['instructions']) if op['op']=='stind.ref']
            need(writes and all(i<call_index for i in writes), 'Native output reset moved after callback/acquisition')
            consumer_edges.append(dict(caller=native['method'],target=prepared['method'],entry_reset_before_callback=True))
        # Four complete prepared bodies are old portable bodies minus only their leading resets.
        for owner,name,count in [('BESHStatNG.ExcelCommon.UdfScalarChoice38','TryBinary',3),('BESHStatNG.ExcelCommon.UdfScalarChoice38','TryWholeNumber',3),
                                 ('BESHStatNG.ExcelCommon.UdfNumericImportPreparation38','TryGetGroupedCompleteMatrix',6),('BESHStatNG.ExcelCommon.UdfNumericImportPreparation38','TryGetNestedThreeColumnData',6)]:
            before=next(m for m in prior['methods'] if m['declaring_type']==owner and m['member_name']==name)
            after=lookup(owner,name+'Prepared')
            old_ops=before['instructions']
            need(all(old_ops[i+2]['op'] in ['stind.i4','stind.ref'] for i in range(0,count,3)), 'Old portable entry reset premise changed')
            trimmed=[]
            for op in old_ops[count:]:
                operand=[v-count for v in op['operand']] if isinstance(op['operand'],list) else op['operand']
                trimmed.append(dict(op=op['op'],operand=operand))
            need(trimmed==after['instructions'] and before['locals']==after['locals'] and before['parameter_types']==after['parameter_types'] and before['exceptions']==after['exceptions'], 'Prepared complete CIL is not original minus entry reset')
        alias_after=load(PREFIX+'alias-probe-after-'+config+'.json')
        alias_before=load(PREFIX+'alias-probe-before-'+config+'.json')
        need(alias_after['original_calls']==alias_before['original_calls']==0 and len(alias_after['cases'])==len(alias_before['cases'])==10, 'Aliased conversion probe provenance changed')
        need(all(r['output']==17 for r in alias_after['cases']) and all(r['output']!=17 for r in alias_before['cases']), 'Aliased output side effects erased or adverse evidence lost')
        for left,right in zip(alias_before['cases'],alias_after['cases']):
            need({k:v for k,v in left.items() if k!='output'}=={k:v for k,v in right.items() if k!='output'}, 'ByRef correction changed conversion/result/exception contract')
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
        expanded_new = load(PREFIX + 'dependency-metadata-v9/current-' + config + '.json')
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
        built = load(PREFIX + 'build-cells-v5-' + config + '.json')
        need(built['all_exit_zero'], 'Production build failed')
        for assembly in n['assemblies']:
            need(dict(name=assembly['file_name'], bytes=assembly['bytes'], sha256=assembly['sha256']) in built['assemblies'], 'Current assembly not bound to completed build')
        exports = load(PREFIX + 'inspection/final-v4/Inspect38-' + config + '.json')['exports']
        original_exports = load('evidence/04A38/preedit/compiled-' + config + '.json')['exports']
        key = lambda r: r['effective_excel_name']
        need(len(exports) == 268 and sorted(exports, key=key) == sorted(original_exports, key=key), 'Export signature/default/attribute altered')
        probe = load(PREFIX + 'cell-probe-v3-after-' + config + '.json')
        need(probe == load(PREFIX + 'cell-probe-v3-before-' + config + '.json') and len(probe['cases']) == 198 and probe['original_calls'] == 0, 'Current conversion/exception/ByRef probe drift')
        for role in ['original','current']:
            for assembly in (o if role == 'original' else n)['assemblies']:
                if role == 'original':
                    lib = root / '.test-rationalization-04A38.tmp/consolidated38-v7/original-build' / config / assembly['file_name']
                else:
                    lib = root / '.test-rationalization-04A38.tmp/completion-w2-cont5/cells-v5' / config / 'lib' / assembly['file_name']
                # Omitted DLLs in a compact copy are not claimed as freshly verified there.
                if lib.is_file():
                    need(sha(lib.read_bytes()) == assembly['sha256'], 'Actual compiled DLL identity changed')
        callbacks = [dict(method=m['method'], body=m) for m in n['methods'] if '_Lambda$' in m['member_name'] and m['declaring_type'].startswith('BESHStatNG.UdfDataImport')]
        config_records.append(dict(configuration=config, retained_full_methods=retained, protected_dependency_methods=len(accepted),
                                   actual_consumer_edges=consumer_edges, callback_bodies=len(callbacks), exports=268))
    # Recompare actual immutable output bytes, not a claimed all_exact flag.
    compatibility = load(PREFIX + 'compatibility-cells-v5.json')
    need(len(compatibility['results']) == 24 and compatibility['new_original_queries'] == 0, 'Compatibility provenance/population changed')
    counts = {}
    for row in compatibility['results']:
        need(raw(row['current']['path']) == raw(row['original']['path']) and sha(raw(row['original']['path'])) == row['original']['sha256'], 'Sealed current/original output mismatch')
        counts[row['configuration']] = counts.get(row['configuration'], 0) + row['cases']
    need(counts == dict(Debug=29424,Release=29424), 'Missing frozen compatibility cases')
    for row in load(PREFIX + 'source-transitions-v4.json')['files']:
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
