from common41 import *
import re
lock();freeze=load(HERE/'matrix/source-freeze-v1.json')
def members(text):
    lines=text.splitlines(keepends=True);classes=[];out={}
    for i,line in enumerate(lines):
        c=re.match(r'[ \t]*(?:Public|Private|Friend)\s+(?:NotInheritable\s+)?Class\s+(\w+)',line)
        if c:classes.append((i,c[1]))
        m=re.match(r'([ \t]*)(Public|Private|Friend)\s+Shared\s+(Function|Sub)\s+(\w+)',line)
        if not m:continue
        end=next(j for j in range(i+1,len(lines)) if re.match(r'^'+re.escape(m[1])+r'End '+m[3]+r'\b',lines[j]))
        body=''.join(lines[i:end+1]);header=body.split('\n')[0].strip();owner=classes[-1][1]
        key=owner+'.'+m[4]+':'+header
        out[key]=dict(owner=owner,method=m[4],visibility=m[2],lines=[i+1,end+1],header=header,body_sha256=sha(body.encode()),Core_constructors=sorted(set(re.findall(r'(?i)New\s+((?:Agreement|parametric|nonparametric)\.[A-Za-z0-9_]+)',body))),Core_calls=sorted(set(re.findall(r'(?i)\b(?:assumptions|contingencytable|SampleSizeCalculator)\.[A-Za-z0-9_]+',body))),existing_model_calls=sorted(set(re.findall(r'\b(?:mdl|model)\.(\w+)\s*\(',body))))
    return out
rows=[];existing_differences=[]
for transition in freeze['source_delta']:
    p=transition['after']['path']
    if not p.startswith('BESHStat.Excel.Common/'):continue
    before=members((PRIVATE/'source-preimages'/p).read_text(encoding='utf-8-sig'));after=members((ROOT/p).read_text(encoding='utf-8-sig'))
    for key,row in after.items():
        if key not in before:rows.append(dict(source=pin(p),**row,classification='ADDED_TO_EXISTING_SERVICE',previous=None))
        elif row['body_sha256']!=before[key]['body_sha256']:existing_differences.append(dict(source=pin(p),before=before[key],after=row))
    assert not before.keys()-after.keys(),'EXISTING_COMMON_METHOD_REMOVED'
assert len([r for r in rows if r['visibility']=='Public'])==22 and len(rows)==23 and not existing_differences
core=[];initial=load(HERE/'entry/source-preimages-v1.json')['source_pins']
for p,h in freeze['source_pins'].items():
    if not p.startswith('BESHStat.Core/src/StatTests/'):continue
    if any(k in p.lower() for k in ['agreement','assumptions','parametric','contingency','samplesize']):
        assert h==initial[p];core.append(pin(p))
save('scope/portable-member-lock-v1.json',dict(existing_Common_services_extended=5,new_Common_classes=0,added_public_methods=22,added_private_finite_predicate=1,members=rows,existing_Common_method_bodies_changed=existing_differences,Core_numerical_source_locks=core,Core_algorithm_edits=0,compiled_Core_and_other_method_equality=[pin(HERE/('abi-cil/'+c+'-proof-v1.json')) for c in ['Debug','Release']],source_ownership='Every new method owns typed acquisition-independent preparation/orchestration/layout. Exact numerical calls and model constructors remain Core. Deferred host readers carry Windows-only conversion/order without native objects.',gates=GATES))
print('22 added public methods in five existing services;all existing Common method bodies exact;Core source locks exact',flush=True)
