"""Read all new portable declarations/fragments with the authenticated Roslyn census."""
from common import *
from source_views import body
tool=PRIVATE/'portable-census-v1';tool.mkdir(exist_ok=False)
old=ROOT/'evidence/04A38/completion/w1/Census.cs';source=old.read_text(encoding='utf-8-sig')
needle='Directory.GetFiles(Path.Combine(root,"BESHstatNG/src/ExcelUDFs"),"*.vb",SearchOption.AllDirectories).Order()'
assert source.count(needle)==1;source=source.replace(needle,'args.Skip(2).Select(p=>Path.Combine(root,p)).Order()')
with (HERE/'PortableCensus-v1.cs').open('x',encoding='utf-8',newline='\n') as f:f.write(source)
project=(ROOT/'evidence/04A38/completion/w1/Census.csproj').read_text(encoding='utf-8-sig').replace('Include="Census.cs"','Include="'+str(HERE/'PortableCensus-v1.cs')+'"')
with (tool/'Census.csproj').open('x',encoding='utf-8') as f:f.write(project)
assets=RUNROOT/'mmrm38-v6/w1-census-tool-v2/obj/project.assets.json';assert assets.is_file()
r=command('portable-census-v1-build',['dotnet','msbuild',str(tool/'Census.csproj'),'/t:Build','/p:ProjectAssetsFile='+str(assets),'/p:OutputPath='+str(tool/'bin')+'\\','/p:IntermediateOutputPath='+str(tool/'obj')+'\\','/p:ProduceReferenceAssembly=false','/v:minimal','/nr:false']);assert r['exit_code']==0
paths=['BESHStat.Excel.Common/src/Analysis/UdfSampleSizePreparation38.vb','BESHStat.Excel.Common/src/Analysis/UdfModelImportPreparation38.vb']
r=command('portable-census-v1-current',['dotnet',str(tool/'bin/Census.dll'),str(ROOT),str(HERE/'portable-universe-v1.json'),*paths]);assert r['exit_code']==0
u=json.loads((HERE/'portable-universe-v1.json').read_bytes());contracts=[]
for n in u['members']:
    text,view=body((ROOT/n['path']).read_bytes(),n);compiled=[]
    for config in ['Debug','Release']:
        data=json.loads((HERE/('metadata-all/packet-v1-context-v3/current-'+config+'.json')).read_bytes())
        methods=[m for m in data['methods'] if (m['declaring_type'],m['member_name'])==(n['owner'],n['name'])];assert len(methods)==1
        context=next(t for t in data['types'] if t['type']==n['owner']);assert not context['fields']
        compiled.append(dict(configuration=config,method=methods[0],declaring_type=context))
    contracts.append(dict(source_record=n,source=pin(ROOT/n['path']),whole_body=text,source_view=view,fragments=[f for f in u['fragments'] if f['member_id']==n['symbol_id']],compiled=compiled))
enum_types=[]
for name,expected in [('UdfSampleSizeError38',{'Value':0,'Numeric':1}),('UdfSampleSizeHypothesis38',{'Invalid':0,'Superiority':1,'NonInferiority':2,'Equivalence':3})]:
    rows=[]
    for config in ['Debug','Release']:
        data=json.loads((HERE/('metadata-all/packet-v1-context-v3/current-'+config+'.json')).read_bytes());context=next(t for t in data['types'] if t['type']=='BESHStatNG.ExcelCommon.'+name)
        assert {f['name']:f['literal'] for f in context['fields'] if f['is_literal']}==expected
        rows.append(dict(configuration=config,context=context))
    enum_types.append(dict(name=name,values=expected,compiled=rows))
assert len(contracts)==15
save('portable-api-contracts-v1.json',dict(contracts=contracts,enum_types=enum_types,public_preparation_apis=12,new_portable_methods_including_private_ctors_and_failure_helper=15,source_universe=pin(HERE/'portable-universe-v1.json'),reader=pin(HERE/'PortableCensus-v1.cs'),original_reader=pin(old),assets=pin(assets),scope='Separate all-declaration census outside the historical704/current726 Windows UDF frontier. No portable method hidden as a historical ID; complete source/UTF16/fragment and real shipping CIL/type/field contexts in both configurations. No original source compilation.',original_queries=0))
print('All15 new portable method declarations and2 enum type contexts authenticated;12 public prepared APIs')
