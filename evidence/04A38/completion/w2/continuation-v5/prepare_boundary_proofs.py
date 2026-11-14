from common import *
import re
def emit(name,text):
    with (HERE/name).open('x',encoding='utf-8',newline='\n') as stream:stream.write(text)
text=(HERE/'verify_slice_v2.py').read_text(encoding='utf-8')
text=re.sub(r"REVIEW_SHA256 = '[0-9a-f]+'","REVIEW_SHA256 = '"+sha((HERE/'body-contracts-v3.json').read_bytes())+"'",text)
text=text.replace('body-contracts-v2.json','body-contracts-v3.json').replace('inspection/final-v3/','inspection/final-v4/').replace('build-cells-v4-','build-cells-v5-').replace('completion-w2-cont5/cells-v4','completion-w2-cont5/cells-v5').replace('compatibility-cells-v4.json','compatibility-cells-v5.json').replace('source-transitions-v3.json','source-transitions-v4.json').replace('dependency-metadata-v8/current-','dependency-metadata-v9/current-').replace('cell-probe-v2-','cell-probe-v3-')
needle="    config_records = []"
insert="""    phase_rows = load(PREFIX + 'prepared-phase-source-proof-inputs.json')['rows']
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
        need(not re.search(r'^\\s*(?:result|groupMatrices|groupNames|data|names)\\s*=\\s*(?:-1|0|defaultValue|Nothing)\\s*$', prepared_body,re.M), 'Prepared phase repeats an entry reset')
    aliases = load(PREFIX + 'compiler-generated-aliases-final.json')['aliases']
    need(aliases == {'BESHStatNG.ExcelCommon.UdfNumericImportPreparation38+_Closure$__6-0':'BESHStatNG.ExcelCommon.UdfNumericImportPreparation38+_Closure$__8-0'}, 'Unsupported generated-name alias')
    def relocate(value):
        if isinstance(value,str):
            for old,new in aliases.items(): value=value.replace(old,new)
            return value
        if isinstance(value,list): return [relocate(v) for v in value]
        if isinstance(value,dict): return {k:relocate(v) for k,v in value.items()}
        return value
"""
assert needle in text;text=text.replace(needle,insert+needle)
needle="                       ('BESHStatNG.UdfOutputTables','IsNumericValue')}"
replace="""                       ('BESHStatNG.UdfOutputTables','IsNumericValue'),
                       *[('BESHStatNG.WorksheetFunctions.ExcelArgNumeric',n) for n in ['TryGetBinary01','TryGetWholeNumber','TryGetOptionalPositiveInteger']],
                       *[('BESHStatNG.UdfDataImport',n) for n in ['TryGetGroupedCompleteMatrix','TryGetNestedThreeColumnData']],
                       *[('BESHStatNG.ExcelCommon.UdfScalarChoice38',n) for n in ['TryBinary','TryWholeNumber','TryPositiveInteger']],
                       *[('BESHStatNG.ExcelCommon.UdfNumericImportPreparation38',n) for n in ['TryGetGroupedCompleteMatrix','TryGetNestedThreeColumnData']]}"""
assert needle in text;text=text.replace(needle,replace)
text=text.replace("need(method['method'] in nm and semantic(method) == semantic(nm[method['method']]),", "mapped = relocate(method)\n            need(mapped['method'] in nm and semantic(mapped) == semantic(nm[mapped['method']]),")
text=text.replace("need(context == nt.get(context['type']),", "mapped_context = relocate(context)\n            need(mapped_context == nt.get(mapped_context['type']),")
needle="        method = lookup('BESHStatNG.WorksheetFunctions.ExcelArgNumeric','TryGetDouble')"
insert="""        for owner,name,target in [
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
"""
assert needle in text;text=text.replace(needle,insert+needle)
emit('verify_slice_v3.py',text)
# Reconcile fresh native syntax, allowing only two explicitly reviewed changed inherited bodies.
text=(HERE/'reconcile_final.py').read_text(encoding='utf-8').replace('from verify_slice_v2 import','from verify_slice_v3 import').replace('current-universe-final-v3.json','current-universe-final-v4.json').replace('body-contracts-v2.json','body-contracts-v3.json').replace('independent-slice-proof-v2.json','independent-slice-proof-v3.json')
for name in ['historical-ownership','current-ownership','ownership-summary','original-fragment-parent-crosswalk','fragment-coverage','remaining-work-queue','per-family-ownership']:
    text=text.replace("save('"+name+"-v2.json'","save('"+name+"-v3.json'")
needle="    assert len(matches) == 1, row['symbol_id']"
insert="""    if not matches:
        reviewed_keys={h['git_original_id'].split('@')[0] for h in history if h['canonical_original_id'] in review}
        assert row['signature_key'] in reviewed_keys, row['symbol_id']
        matches=list(bykey[row['signature_key']])
"""
assert needle in text;text=text.replace(needle,insert+needle)
emit('reconcile_boundary.py',text)
text=(HERE/'verify_ownership_v2.py').read_text(encoding='utf-8').replace('from verify_slice_v2 import','from verify_slice_v3 import').replace('current-universe-final-v3.json','current-universe-final-v4.json')
for name in ['historical-ownership','current-ownership','ownership-summary','original-fragment-parent-crosswalk','fragment-coverage','remaining-work-queue']:
    text=text.replace("PREFIX+'"+name+"-v2.json'","PREFIX+'"+name+"-v3.json'")
needle="        need(len(matches)==1,'Lost inherited current body/callback')"
insert="""        if not matches:
            review_ids=set(proof['original_ids'])
            reviewed_keys={r['git_original_id'].split('@')[0] for r in previous if r['canonical_original_id'] in review_ids}
            need(r['signature_key'] in reviewed_keys,'Unreviewed changed inherited body')
            matches=list(bykey[r['signature_key']])
"""
assert needle in text;text=text.replace(needle,insert+needle)
text=text.replace("    universe = load(PREFIX+'current-universe-final-v4.json')", "    need(sha(raw(PREFIX+'current-universe-final-v4.json'))=='"+sha((HERE/'current-universe-final-v4.json').read_bytes())+"','Authenticated current census changed')\n    universe = load(PREFIX+'current-universe-final-v4.json')")
emit('verify_ownership_v3.py',text)
text=(HERE/'verify_preservation_v2.py').read_text(encoding='utf-8').replace('source-transitions-v3.json','source-transitions-v4.json')
emit('verify_preservation_v3.py',text)
text=(HERE/'record_edges_v2.py').read_text(encoding='utf-8').replace('body-contracts-v2.json','body-contracts-v3.json').replace('dependency-metadata-v8','dependency-metadata-v9').replace("save('production-consumer-edges-v2.json'","save('production-consumer-edges-v3.json'").replace("save('entry-discrepancies-v2.json'","save('entry-discrepancies-v3.json'")
emit('record_edges_v3.py',text)
