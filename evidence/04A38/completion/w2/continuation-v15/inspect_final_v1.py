"""Fresh corrected-source current-only census, export ABI, constructed signatures/CIL and source transitions."""
from common import *
folder=HERE/'final-v1';folder.mkdir(exist_ok=False)
commands=[];files=[]
metadata_tool=PRIVATE/'signatures-v1/MethodMetadata38.exe'
export_tool=PRIVATE/'packet-v2/inspection/Inspect38.exe'
shape_tool=PRIVATE/'declaration-shapes-v1/bin/Census.dll'
syntax_tool=RUNROOT/'completion-w2-cont8/portable-census-v1/bin/Census.dll'
native_tool=RUNROOT/'mmrm38-v6/w1-census-tool-v2/bin/Census.dll'
for cfg in ['Debug','Release']:
    lib=PRIVATE/'packet-v3'/cfg/'lib'
    for label,tool,args in [
        ('metadata',metadata_tool,[str(lib),str(folder/('current-'+cfg+'.json')),str(HERE/'native-metadata-owners-v3.txt')]),
        ('exports',export_tool,[str(lib),str(folder/('exports-'+cfg+'.json')),str(ROOT/'evidence/04A38/entry-members.json')])]:
        r=command('final-'+label+'-'+cfg+'-v1',[str(tool),*args]);commands.append(clean(r));assert r['exit_code']==0
    exports=json.loads((folder/('exports-'+cfg+'.json')).read_bytes())['exports']
    original=json.loads((ROOT/('evidence/04A38/preedit/compiled-'+cfg+'.json')).read_bytes())['exports']
    assert len(exports)==268 and sorted(exports,key=lambda r:r['effective_excel_name'])==sorted(original,key=lambda r:r['effective_excel_name'])
    files.extend([pin(folder/('current-'+cfg+'.json')),pin(folder/('exports-'+cfg+'.json'))])
r=command('final-native-syntax-v1',['dotnet',str(native_tool),str(ROOT),str(folder/'current-universe.json')]);commands.append(clean(r));assert r['exit_code']==0
universe=json.loads((folder/'current-universe.json').read_bytes());assert len(universe['members'])==726
paths_by_role={role:sorted(p.relative_to(ROOT).as_posix() for p in (ROOT/base).rglob('*.vb')) for role,base in [('common','BESHStat.Excel.Common/src'),('core','BESHStat.Core/src')]}
for role,paths in paths_by_role.items():
    r=command('final-'+role+'-syntax-v1',['dotnet',str(syntax_tool),str(ROOT),str(folder/('dependency-full-'+role+'.json')),*paths]);commands.append(clean(r));assert r['exit_code']==0
paths=list(dict.fromkeys([r['path'] for r in universe['sources']]+paths_by_role['common']+paths_by_role['core']))
shapes={};sources={}
for index in range(0,len(paths),70):
    output=folder/('declaration-shapes-part-'+str(index//70)+'.json')
    r=command('final-declaration-part-'+str(index//70)+'-v1',['dotnet',str(shape_tool),str(ROOT),str(output),*paths[index:index+70]]);commands.append(clean(r));assert r['exit_code']==0
    doc=json.loads(output.read_bytes())
    for row in doc['rows']:assert shapes.setdefault(row['symbol_id'],row)==row
    for row in doc['sources']:assert sources.setdefault(row['path'],row)==row
save('final-v1/declaration-shapes.json',dict(scope='EXACT_CORRECTED_CURRENT_NATIVE_COMMON_CORE_SYNTAX_UNION',sources=[sources[k] for k in sorted(sources)],rows=[shapes[k] for k in sorted(shapes)]))
plan=json.loads((HERE/'implementation-plan-v2.json').read_bytes());before={r['live']['path']:r for r in plan['source_before']};transitions=[]
for path in plan['source_paths']:
    target=HERE/'source-after/final-v1'/(path+'.txt');target.parent.mkdir(parents=True,exist_ok=True)
    with target.open('xb') as stream:stream.write((ROOT/path).read_bytes())
    row=before.get(path)
    transitions.append(dict(path=path,before=row['live'] if row else None,preimage=row['preimage'] if row else None,
                            after=pin(ROOT/path),snapshot=pin(target),entry_state='EXISTING' if row else 'ABSENT'))
save('source-transitions-final-v1.json',dict(files=transitions,original_queries=0,scope='Complete actual current corrected source/test bytes, exact entry preimages and absent-entry declarations. Earlier packet/defect snapshots remain immutable.'))
for row in json.loads((HERE/'production-test-freeze-v2.json').read_bytes())['sources']:assert pin(ROOT/row['path'])==row
save('final-inspection-v1.json',dict(commands=commands,tools=[pin(p) for p in [metadata_tool,export_tool,shape_tool,syntax_tool,native_tool]],
    files=files,native_members=726,public_exports_per_configuration=268,source_freeze=pin(HERE/'production-test-freeze-v2.json'),
    current_only=True,original_queries=0,original_activity='READ_EXISTING_AUTHENTICATED_JSON_ONLY;NO_ORIGINAL_DLL_LOAD_OR_EXECUTION'))
print('Corrected current Debug/Release268 ABI, constructed signatures/CIL, complete syntax and source transitions ready')
