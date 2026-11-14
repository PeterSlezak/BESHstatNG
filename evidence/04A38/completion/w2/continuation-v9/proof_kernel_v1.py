"""Pure source/constructed-handle binding; no compiler, source or evidence mutation."""
import re
from collections import defaultdict
def need(value,message):
    if not value:raise ValueError(message)
def semantic(row):return {k:v for k,v in row.items() if k!='raw_IL_sha256'}
def definitions(meta):
    result={r['method']:r for r in meta['methods']};need(len(result)==len(meta['methods']),'Duplicate compiled member handle');return result
def closure(meta,roots):
    defs=definitions(meta);pending=list(roots);seen=set();outside=set();edges=[]
    while pending:
        key=pending.pop()
        if key in seen:continue
        row=defs.get(key)
        if row is None:outside.add(key);continue
        need(row['declaring_type'] is not None,'Unconstructed reached owner');seen.add(key)
        for index,i in enumerate(row['instructions']):
            if i['op'] in ['call','callvirt','newobj','ldftn','ldvirtftn'] and isinstance(i['operand'],str) and i['operand'].startswith('METHOD:'):
                target=i['operand'][7:];pending.append(target);edges.append(dict(caller=key,instruction=index,op=i['op'],target=target))
    need(not any(k.split(' @ ',1)[1].split(',',1)[0] in ['BESHStatNG','BESHStat.Core','BESHStat.Excel.Common'] for k in outside),'Missing internal real constructed target')
    return dict(methods=sorted(seen),external_runtime_or_ExcelDna=sorted(outside),edges=edges)
def map_current(before,after):
    exact={n['symbol_id']:n for n in after};bykey=defaultdict(list);result={}
    for n in after:bykey[n['signature_key']].append(n)
    for n in before:
        if n['symbol_id'] in exact:
            candidate=exact[n['symbol_id']];need(candidate['signature_key']==n['signature_key'],'Exact source ID signature changed')
            if n['parent_member'] is not None:need(candidate['body_sha256']==n['body_sha256'],'Existing callback body changed')
        else:
            choices=[v for v in bykey[n['signature_key']] if (v['parent_member'] or '').split('@')[0]==(n['parent_member'] or '').split('@')[0] and (n['parent_member'] is None or v['body_sha256']==n['body_sha256'])]
            need(len(choices)==1,'Ambiguous source identity; no signature-only callback alias allowed');candidate=choices[0]
        result[n['symbol_id']]=candidate
    need(len({n['symbol_id'] for n in result.values()})==len(result),'Overlapping old/current source identity')
    return result
ALIASES={'Object':'System.Object','String':'System.String','Double':'System.Double','Integer':'System.Int32','Boolean':'System.Boolean','Long':'System.Int64','Single':'System.Single','Decimal':'System.Decimal','Date':'System.DateTime','Byte':'System.Byte','Short':'System.Int16','Char':'System.Char'}
def source_arguments(value):
    result=[];start=0;depth=0
    for index,ch in enumerate(value):
        if ch=='(':depth+=1
        elif ch==')':depth-=1
        elif ch==',' and depth==0:result.append(value[start:index].strip());start=index+1
    result.append(value[start:].strip());return result
def actual_arguments(value):
    content=value[value.index('[[')+1:-1];result=[];depth=0;start=0
    for index,ch in enumerate(content):
        if ch=='[':
            if depth==0:start=index+1
            depth+=1
        elif ch==']':
            depth-=1
            if depth==0:
                argument=content[start:index];nested=0;stop=len(argument)
                for j,c in enumerate(argument):
                    if c=='[':nested+=1
                    elif c==']':nested-=1
                    elif c==',' and nested==0:stop=j;break
                result.append(argument[:stop])
    return result
def parameter_matches(source,actual):
    byref=actual.endswith('&')
    if byref:actual=actual[:-1]
    if source['byref']!=byref:return False
    name=(source['type'] or 'Object').replace('Global.','')
    arrays=[]
    for part in [source['identifier'],name]:
        while re.search(r'\((,*)\)$',part):
            match=re.search(r'\((,*)\)$',part);arrays.append(match[1]);part=part[:match.start()]
    while re.search(r'\((,*)\)$',name):name=re.sub(r'\((,*)\)$','',name)
    for rank in arrays:
        suffix='['+rank+']'
        if not actual.endswith(suffix):return False
        actual=actual[:-len(suffix)]
    if name.endswith('?'):
        return actual.startswith('System.Nullable`1[[') and len(actual_arguments(actual))==1 and parameter_matches(dict(type=name[:-1],identifier='',byref=False),actual_arguments(actual)[0])
    if '(Of ' in name:
        prefix=name.split('(Of ',1)[0].split('.')[-1]
        if re.search(r'(?:^|\.)'+re.escape(prefix)+r'`\d+\[\[',actual) is None:return False
        expected=source_arguments(name.split('(Of ',1)[1][:-1]);got=actual_arguments(actual)
        return len(expected)==len(got) and all(parameter_matches(dict(type=e,identifier='',byref=False),a) for e,a in zip(expected,got))
    expected=ALIASES.get(name,name)
    return actual==expected or actual.endswith('.'+expected)
def bind(node,meta,shapes):
    shape=shapes[node['symbol_id']];params=shape['parameters'] or []
    candidates=[r for r in meta['methods'] if (r['declaring_type'],r['member_name'])==(node['owner'],node['name']) and len(r['parameter_types'])==len(params) and all(parameter_matches(p,a) for p,a in zip(params,r['parameter_types']))]
    need(len(candidates)==1,'Missing/ambiguous full declared parameter/ByRef/rank binding: '+node['symbol_id'])
    return candidates[0]
