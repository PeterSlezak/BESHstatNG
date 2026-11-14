from common import *
rows=[]
for c in ['Debug','Release']:
    old=json.loads((PREVIOUS/'metadata-v6/parametric-v4'/('current-'+c+'.json')).read_bytes())
    new=json.loads((HERE/'inspection/final-v4'/('MethodMetadata38-'+c+'.json')).read_bytes())
    for name in ['TryBinaryPrepared','TryWholeNumberPrepared','TryPositiveIntegerPrepared','TryGetGroupedCompleteMatrixPrepared','TryGetNestedThreeColumnDataPrepared']:
        m=next(m for m in new['methods'] if m['member_name']==name)
        rows.append(dict(configuration=c,name=name,exceptions=m['exceptions'],type_initializer=m['type_initializer'],fields=m['nonliteral_static_fields']))
save('attempts/prepared-foreach-context-v3.json',dict(status='READER_GUARD_FAILURE_RETAINED',
    reason='Grouped source already owns a compiler-generated For Each disposal Finally region. Reject new catches/hidden state, but preserve and rebase this exact existing Finally region rather than require zero EH.',methods=rows,production_defect=False))
text=(HERE/'verify_slice_v3.py').read_text(encoding='utf-8')
text=text.replace("need(not prepared['exceptions'] and not prepared['type_initializer']", "need(all(e['flags']=='Finally' for e in prepared['exceptions']) and not prepared['type_initializer']")
old="need(trimmed==after['instructions'] and before['locals']==after['locals'] and before['parameter_types']==after['parameter_types'] and before['exceptions']==after['exceptions'], 'Prepared complete CIL is not original minus entry reset')"
new="""old_eh=[]
            for region in before['exceptions']:
                e=dict(region)
                for key in ['try_start','try_end','handler_start','handler_end','filter_start']:
                    if e[key] is not None:e[key]-=count
                old_eh.append(e)
            need(trimmed==after['instructions'] and before['locals']==after['locals'] and before['parameter_types']==after['parameter_types'] and old_eh==after['exceptions'], 'Prepared complete CIL is not original minus entry reset')"""
assert old in text;text=text.replace(old,new)
with (HERE/'verify_slice_v4.py').open('x',encoding='utf-8',newline='\n') as stream:stream.write(text)
