"""Current source census and current Debug CLR only. Original metadata is read-only."""
from common import *
import shutil
mode=sys.argv[1]
folder=HERE/'final-v1'
oldprivate=RUNROOT/'completion-w2-cont15'
metadata_tool=oldprivate/'signatures-v1/MethodMetadata38.exe'
export_tool=oldprivate/'packet-v2/inspection/Inspect38.exe'
shape_tool=oldprivate/'declaration-shapes-v1/bin/Census.dll'
syntax_tool=RUNROOT/'completion-w2-cont8/portable-census-v1/bin/Census.dll'
native_tool=RUNROOT/'mmrm38-v6/w1-census-tool-v2/bin/Census.dll'
commands=[]
if mode=='syntax':
 folder.mkdir(exist_ok=False)
 r=command('final-native-syntax-v17-final1',['dotnet',str(native_tool),str(ROOT),str(folder/'current-universe.json')]);commands.append(clean(r));assert r['exit_code']==0
 universe=json.loads((folder/'current-universe.json').read_bytes());assert len(universe['members'])==723
 paths_by_role={role:sorted(p.relative_to(ROOT).as_posix() for p in (ROOT/base).rglob('*.vb')) for role,base in [('common','BESHStat.Excel.Common/src'),('core','BESHStat.Core/src')]}
 for role,paths in paths_by_role.items():
  r=command('final-'+role+'-syntax-v17-final1',['dotnet',str(syntax_tool),str(ROOT),str(folder/('dependency-full-'+role+'.json')),*paths]);commands.append(clean(r));assert r['exit_code']==0
 adjacent='BESHstatNG/src/RegModels/MixedModels/MixedModelFrontEndHelpers.vb'
 r=command('final-adjacent-syntax-v17-final1',['dotnet',str(syntax_tool),str(ROOT),str(folder/'dependency-full-adjacent.json'),adjacent]);commands.append(clean(r));assert r['exit_code']==0
 paths=list(dict.fromkeys([r['path'] for r in universe['sources']]+paths_by_role['common']+paths_by_role['core']+[adjacent]))
 shapes={};sources={}
 for index in range(0,len(paths),70):
  output=folder/('declaration-shapes-part-'+str(index//70)+'.json')
  r=command('final-declaration-part-'+str(index//70)+'-v17-final1',['dotnet',str(shape_tool),str(ROOT),str(output),*paths[index:index+70]]);commands.append(clean(r));assert r['exit_code']==0
  data=json.loads(output.read_bytes())
  for row in data['rows']:assert shapes.setdefault(row['symbol_id'],row)==row
  for row in data['sources']:assert sources.setdefault(row['path'],row)==row
 save('final-v1/declaration-shapes.json',dict(scope='EXACT_COMBINED_CURRENT_NATIVE_COMMON_CORE_SYNTAX_UNION',sources=[sources[k] for k in sorted(sources)],rows=[shapes[k] for k in sorted(shapes)]))
 save('final-syntax-inspection-v1.json',dict(commands=commands,tools=[pin(t) for t in [shape_tool,syntax_tool,native_tool]],source_files=len(sources),native_members=723,Release=DEFERRED,original_queries=0))
elif mode=='compiled':
 stage=sys.argv[2];lib=PRIVATE/stage/'Debug/lib'
 for label,tool,args in [('metadata',metadata_tool,[str(lib),str(folder/'current-Debug.json'),str(PREVIOUS/'native-metadata-owners-v1.txt')]),('exports',export_tool,[str(lib),str(folder/'exports-Debug.json'),str(ROOT/'evidence/04A38/entry-members.json')])]:
  r=command('final-'+label+'-Debug-v17-final1',[str(tool),*args]);commands.append(clean(r));assert r['exit_code']==0
 exports=json.loads((folder/'exports-Debug.json').read_bytes())['exports']
 original=json.loads((ROOT/'evidence/04A38/preedit/compiled-Debug.json').read_bytes())['exports']
 assert len(exports)==268 and sorted(exports,key=lambda r:r['effective_excel_name'])==sorted(original,key=lambda r:r['effective_excel_name'])
 save('final-Debug-inspection-v1.json',dict(configuration='Debug',commands=commands,tools=[pin(t) for t in [metadata_tool,export_tool]],files=[pin(folder/n) for n in ['current-Debug.json','exports-Debug.json']],public_exports=268,source_freeze=pin(HERE/'production-test-freeze-v1.json'),current_only=True,Release_compilation=DEFERRED,Release_ABI_CIL_signatures_exports=DEFERRED,Release_unit_suites=DEFERRED,Release_frozen_compatibility=DEFERRED,original_queries=0,original_activity='READ_EXISTING_AUTHENTICATED_METADATA_ONLY;NO_ORIGINAL_DLL_LOAD_OR_EXECUTION'))
else:raise ValueError(mode)
print('Final combined '+mode+' inspection PASS; Release deferred',flush=True)
