"""Complete source-member preparation plan; never a migration completion claim.

Each callable binds to actual production CIL and at least one registered export.
Lambdas and initializers retain their real original span/parent identities.
"""
from collections import Counter,defaultdict
import csv
import json
import re
from preflight import HERE,ROOT,pin,save


def main():
    data=json.loads((HERE/'entry-members.json').read_bytes())
    compiled=json.loads((HERE/'preedit/compiled-Debug.json').read_bytes())
    closure=json.loads((HERE/'preedit/consumer-closure-Debug-v1.json').read_bytes())
    graph={r['method']:r for r in closure['methods']}
    by_name=defaultdict(list)
    for row in closure['methods']:by_name[row['method'].split('(')[0].replace('+','.')].append(row)
    consumers=defaultdict(set)
    for export in compiled['exports']:
        todo=[export['method']];seen=set()
        while todo:
            key=todo.pop()
            if key in seen:continue
            seen.add(key);consumers[key].add(export['effective_excel_name'])
            todo.extend(c['target'] for c in graph.get(key,{}).get('calls',[]) if c['assembly']=='BESHStatNG')
    with (HERE/'udf-member-crosswalk.csv').open(encoding='utf-8',newline='') as stream:history={r['current_member_id']:r for r in csv.DictReader(stream)}
    host_exact={
        'Get2D','Get2DOrScalar','CoerceToObjectMatrix','TryCoerceToObjectMatrix','GetFirstSourceRow','TryGetSurvival2D',
        'IsMissingArg','IsBlankCell','AsString','CellToTrimmedText','TryGetDouble','TryGetFiniteDoubleFlexible',
        'TryGetHandleKey','TryGetCachedHandle','LoggedUdfError','LoggedUdfExceptionText','IsNumericValue',
        'IsMmrmWorksheetRangeArgument','SafeExcelNumber','ConvertBinaryClassificationOutputForExcel',
    }
    fields=[];rows=[]
    for member in data['members']:
        name=member['name'];kind=member['kind'];owner=member['containing_type']
        bindings=by_name.get(owner+'.'+name,[])
        if name=='PrepareResultTableForUdf':
            typ='BESHStatNG.ResultTable' if 'table As ResultTable' in member['signature'] else 'System.Object'
            bindings=[r for r in bindings if '('+typ+')' in r['method']]
        if kind=='GetAccessorBlock':
            container,property_name=owner.rsplit('.',1)
            bindings=by_name.get(container+'.get_'+property_name,[])
        if kind in {'FunctionBlock','SubBlock','GetAccessorBlock'}:
            assert bindings and (len(bindings)==1 or '(Of ' in member['signature']),(member['symbol_id'],[r['method'] for r in bindings])
            assert any(consumers[b['method']] for b in bindings),member['symbol_id']
        direct=[c for b in bindings for c in b['calls']]
        portable=[c for c in direct if c['assembly'] in ['BESHStat.Core','BESHStat.Excel.Common']]
        registered=sorted(set().union(*(consumers[b['method']] for b in bindings))) if bindings else []
        parent=member['enclosing'][0] if member['enclosing'] else ''
        subject=owner+'.'+name
        if kind=='VariableDeclarator' and 'ConcurrentDictionary' in member['signature']:
            disposition='HOST_ONLY';scope='Windows session cache identity/comparer/allocation/lifetime. No dictionary moves to portable code.'
        elif kind in {'PropertyStatement','PropertyBlock','GetAccessorBlock'}:
            disposition='HOST_ONLY';scope='Host-owned mutable acquisition/cache/profile buffer shape. Pure completed-input/profile views and computations are separately projected; this tag does not declare all profile behavior native.'
        elif kind=='VariableDeclarator':
            disposition='SPLIT_HOST_AND_PORTABLE';scope='Sample-size hypothesis semantic constants move to consumed Common option policy; private Windows aliases preserve callers.'
        elif 'Lambda' in kind:
            if 'XlCall' in member['body_tokens']:
                disposition='HOST_ONLY';scope='Native xlCoerce callback retained at Get2D Windows boundary; source/metadata contract only, NATIVE_NOT_RUN.'
            elif 'h . Model .' in member['body_tokens']:
                disposition='SPLIT_HOST_AND_PORTABLE';scope='Windows cache capture and existing Core getter remain host adapter; Common consumes ordered typed axis selector and prepares labels/shape. No new factor calculation.'
            elif 'Object' in member['body_tokens']:
                disposition='SPLIT_HOST_AND_PORTABLE';scope='Native cell/object boxing remains Windows; normalized category/label decisions move to typed Common preparation.'
            else:
                disposition='PORTABLE_EXTRACT_AND_CONSUME';scope='Pure typed callback fragment moves with its real enclosing preparation member; exact original body/span and parent retained, not a fictitious standalone declaration.'
        elif member['exported'] and (name.endswith(('_DROP','_CLEAR_ALL')) or name=='PS_CLEANUP'):
            disposition='HOST_ONLY';scope='Export registration, session dictionary remove/clear/count and error mapping retained exactly; lifecycle redesign deferred to04A43.'
        elif name in host_exact or name.startswith('TryGet') and name.endswith('Handle'):
            disposition='HOST_ONLY';scope='Excel sentinel/native cell coercion, range/source observation, error marshaling, logging or session handle lookup. Existing typed transport/Core calls are reused; no domain numerical algorithm classified native.'
        elif member['exported']:
            body=member['body_tokens'][len(member['header_tokens']):]
            if portable and 'Object' not in member['signature'] and not any(t in body for t in [' For ',' Select ','Normalize','Parse','Dim out']):
                disposition='PORTABLE_ALREADY_CONSUMED';scope='Typed scalar wrapper already delegates calculation to the exact resolved Core target; registration and native error boundary remain Windows.'
            else:
                disposition='SPLIT_HOST_AND_PORTABLE';scope='Retain complete exported header, native acquisition/cache/error mapping; reuse resolved engines and extract remaining normalized options, typed numeric/row preparation and report projection. Public wrapper is not relocated.'
        elif 'Object' in member['signature']:
            disposition='SPLIT_HOST_AND_PORTABLE';scope='Native object/sentinel interpretation and worksheet marshaling retained. Move normalized typed option/default, row/label/shape, numerical or result-cell intent; never pass Excel-native objects to Common.'
        elif any(word in member['signature'] for word in ['Handle','LsmEstimate','LmmImportedInputs']):
            disposition='SPLIT_HOST_AND_PORTABLE';scope='Private host buffer/cache stays Windows; snapshot typed design/profile/label values for reusable Common preparation. No cache ownership or timing change.'
        else:
            disposition='PORTABLE_EXTRACT_AND_CONSUME';scope='Typed host-independent helper preparation moves to Core for numeric work or Common for options/labels/results, with old private signature delegating at its original call position.'
        rows.append(dict(current_member_id=member['symbol_id'],historical_member_id=history[member['symbol_id']]['historical_member_id'],
            path=member['path'],owner=owner,signature=member['signature'],kind=kind,source_sha256=member['source_sha256'],body_sha256=member['body_sha256'],
            original_enclosing_member=parent,compiled_bindings=json.dumps([b['method'] for b in bindings]),
            real_registered_consumers=json.dumps(registered),direct_core_common_edges=json.dumps(portable),
            planned_disposition=disposition,planned_scope=subject+': '+scope,
            completion_state='OPEN_PENDING_IMPLEMENTATION_AND_FINAL_CONSUMER_VALIDATION' if disposition not in ['HOST_ONLY','PORTABLE_ALREADY_CONSUMED'] else 'RETAINED_BOUNDARY_OR_EXISTING_PORTABLE_EDGE_REQUIRES_FINAL_PRESERVATION_CHECK',
            native_control='NOT_RUN',scientific='NOT_GOLDEN',future_semantic_issue=history[member['symbol_id']]['future_issue']))
    assert len(rows)==704
    destination=HERE/'udf-member-preparation-plan-v1.csv'
    with destination.open('x',encoding='utf-8',newline='') as stream:
        writer=csv.DictWriter(stream,fieldnames=list(rows[0]),lineterminator='\n');writer.writeheader();writer.writerows(rows)
    declarations=json.loads((HERE/'entry-supporting-declarations.json').read_bytes())['rows']
    save('member-preparation-plan-v1.json',dict(status='PLANNED_SCOPE_ONLY; NOT_MIGRATION_COMPLETION',
        behavioral_members=704,historical_members=703,additional_native_callback=1,
        supporting_declarations=347,primary_overlap=sum(r['already_in_primary_population'] for r in declarations),
        additional_supporting_population=sum(not r['already_in_primary_population'] for r in declarations),
        callable_cil_bindings=643,all_callable_members_have_registered_consumers=True,
        dispositions=dict(Counter(r['planned_disposition'] for r in rows)),
        remaining_open=sum(r['completion_state'].startswith('OPEN') for r in rows),
        preliminary_supporting_tag_qualification='entry-supporting-declarations.json records declaration location/buffer/cache shape, not a final HOST_ONLY disposition. Typed profile/input decisions still require consumed projections.',
        planned_new_core_files=['BESHStat.Core/src/WorksheetFunctions/UdfNumericPreparation38.vb'],
        planned_common_roles=['typed normalized option policy with exact legacy culture comparison','typed normalized row/label selection preserving source ordinals','immutable report-cell/shape intent and existing ResultTable metadata reuse','typed completed LSM profile/design preparation'],
        proposed_windows_scope=[r['path'] for r in data['sources']],
        forbidden_changes=['registration headers/flags/defaults','cache dictionaries/lifetime','statistical engines','Master Plan and owner README files','native rendering or activation','04A39 policy correction'],
        note='Core/Common SDK default Compile includes verified. No invented project include. Windows/tests explicit includes will be changed only if needed by actual new files.',
        evidence=[pin('evidence/04A38/udf-member-preparation-plan-v1.csv'),pin('evidence/04A38/preedit/consumer-closure-Debug-v1.json')]))
    print('Planned704 members;643 actual CIL bindings with registered callers;',sum(r['completion_state'].startswith('OPEN') for r in rows),'still open for implementation')


if __name__=='__main__':main()
