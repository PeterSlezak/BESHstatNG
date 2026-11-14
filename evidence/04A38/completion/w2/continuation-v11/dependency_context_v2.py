"""Current syntax-only full source context; actual graph controls proof scope."""
from common import *
tool=RUNROOT/'completion-w2-cont8/portable-census-v1/bin/Census.dll'
shape_tool=PRIVATE/'declaration-shapes-v1/bin/Census.dll'
receipts=[];shape_documents=[]
for role,folder in [('common','BESHStat.Excel.Common/src'),('core','BESHStat.Core/src')]:
    paths=sorted(p.relative_to(ROOT).as_posix() for p in (ROOT/folder).rglob('*.vb'))
    result=command('dependency-full-'+role+'-syntax-v2',['dotnet',str(tool),str(ROOT),
        str(HERE/('dependency-full-'+role+'-v2.json')),*paths])
    receipts.append(clean(result));assert result['exit_code']==0
    for index in range(0,len(paths),80):
        target=HERE/('dependency-shapes-'+role+'-'+str(index//80)+'-v2.json')
        result=command('dependency-shapes-'+role+'-'+str(index//80)+'-v2',['dotnet',str(shape_tool),
            str(ROOT),str(target),*paths[index:index+80]])
        receipts.append(clean(result));assert result['exit_code']==0;shape_documents.append(target)
base=json.loads((HERE/'declaration-shapes-v1.json').read_bytes())
shapes={r['symbol_id']:r for r in base['rows']};sources={r['path']:r for r in base['sources']}
for path in shape_documents:
    doc=json.loads(path.read_bytes())
    for row in doc['rows']:assert shapes.setdefault(row['symbol_id'],row)==row
    for row in doc['sources']:assert sources.setdefault(row['path'],row)==row
save('declaration-shapes-v2.json',dict(scope='CURRENT_SYNTAX_ONLY_EXACT_UNION',
    sources=[sources[k] for k in sorted(sources)],rows=[shapes[k] for k in sorted(shapes)]))
graph=json.loads((HERE/'cohort-diagnostic-v2.json').read_bytes())['graphs']['Debug']['methods']
meta=json.loads((HERE/'metadata-all/packet-v2-context-v5/current-Debug.json').read_bytes())
defs={r['method']:r for r in meta['methods']}
owners={defs[k]['declaring_type'] for k in graph}
owners.update(r['owner'] for r in json.loads((HERE/'portable-universe-v1.json').read_bytes())['members'])
owners.update((HERE/'return-metadata-owners-v1.txt').read_text().splitlines())
owner_path=HERE/'return-metadata-owners-v2.txt'
with owner_path.open('x',encoding='utf-8',newline='\n') as stream:stream.write('\n'.join(sorted(owners))+'\n')
exe=PRIVATE/'return-metadata-v1/ReturnMetadata.exe'
for config in ['Debug','Release']:
    result=command('return-metadata-'+config+'-v2',[str(exe),str(PRIVATE/'packet-v2'/config/'lib'),
        str(HERE/('return-metadata-'+config+'-v2.json')),str(owner_path)])
    receipts.append(clean(result));assert result['exit_code']==0
save('dependency-context-authority-v2.json',dict(commands=receipts,syntax_tool=pin(tool),
    shape_tool=pin(shape_tool),returns_tool=pin(exe),owners=pin(owner_path),
    scope='Current source context only, no added ownership closure outside the901-method compiled graph and exact native cohort. No original compilation, DLL load, query or capture.',original_queries=0))
print('Current full dependency-source/parameter/return context recorded; ownership scope unchanged')
