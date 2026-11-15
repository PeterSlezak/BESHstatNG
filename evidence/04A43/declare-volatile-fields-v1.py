from common43 import *
import re
old=load(PRIVATE/'fixtures-before-v3/observations.json');fields=[]
labels={'Created UTC','Computational time (s)','Execution time','Total fit time ms','Starting values time ms','Optimization time ms','Final evaluation time ms','KR workspace build time ms','KR derivative blocks time ms','KR P/Q/R matrices time ms','KR adjusted Var(beta) time ms','Result wrapping time ms','Average Information matrix time ms','Analytic gradient trace/quadratic contraction time ms'}
for op in old['operations']:
    out=op.get('output') or {};values=out.get('values',[])
    for i,cell in enumerate(values):
        if cell.get('type')!='System.String':continue
        text=cell.get('value','')
        if text in labels:
            assert i+1<len(values)
            fields.append(dict(operation=op['id'],kind='LABELED_VALUE_CELL',label_index=i,label=text,value_index=i+1,value_type=values[i+1]['type']))
        elif re.fullmatch(r'Computational time:?\s+[-+0-9.,Ee ]+ seconds\.',text):
            fields.append(dict(operation=op['id'],kind='FOOTNOTE_ELAPSED_NUMBER_ONLY',value_index=i,value_type=cell['type']))
sources=['BESHstatNG/src/ExcelUDFs/PropensityScoreUDFs.vb','BESHstatNG/src/ExcelUDFs/Regression/GeeUDFs.vb']
for p in (ROOT/'BESHStat.Core/src').rglob('*.vb'):
    b=p.read_bytes()
    if any(t.encode() in b for t in ['Execution time','Total fit time ms','Computational time:','Computational time {']):sources.append(p.relative_to(ROOT).as_posix())
save('fixtures/volatile-fields-v1.json',dict(kind='EXACT_SOURCE_REACHED_RUNTIME_CELL_QUALIFICATIONS;NO_STATISTICAL_NORMALIZATION',baseline=pin(PRIVATE/'fixtures-before-v3/observations.json'),fields=fields,source_authorities=[pin(p) for p in sources],raw_values_preserved=True,generated_GUID_policy='Actual36 reference graphs establish a bijection from captured opaque tokens to symbols;syntax/uniqueness/object association checked before any token replacement.',statistical_values_shapes_error_precedence='ALL_RETAINED_EXACT',gates=GATES))
s=(HERE/'compare43.py').read_text(encoding='utf-8-sig');s=s.replace("    return walk(data)",'''    result=walk(data);ops={r['id']:r for r in result['operations']}
    for f in load(HERE/'fixtures/volatile-fields-v1.json')['fields']:
        values=ops[f['operation']]['output']['values'];cell=values[f['value_index']];assert cell['type']==f['value_type']
        if f['kind']=='LABELED_VALUE_CELL':
            assert values[f['label_index']]['value']==f['label'];values[f['value_index']]=dict(type=cell['type'],runtime_only='<'+f['label']+'>')
        else:
            text=cell['value'];assert re.fullmatch(r'Computational time:?\\s+[-+0-9.,Ee ]+ seconds\\.',text)
            cell['value']=re.sub(r'(Computational time:?\\s+)[-+0-9.,Ee ]+( seconds\\.)',r'\\1<ELAPSED_NUMBER>\\2',text)
    return result''')
with (HERE/'compare43_v2.py').open('x',encoding='utf-8',newline='\n') as f:f.write(s)
print('Explicit source-pinned runtime cells',len(fields),'qualified;raw values kept;GUID reference bijection never replaced by shape-only comparison',flush=True)
