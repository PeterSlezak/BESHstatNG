"""Full cache declarations, accessors, all writers and exact object-copy boundaries."""
from common import *
from source_views import view
from proof_kernel_v4 import definitions,semantic
import re
u=json.loads((HERE/'current-universe-packet-v1.json').read_bytes());original=json.loads((W2.parent/'w1/original-universe.json').read_bytes())
prior=json.loads((PREVIOUS/'current-ownership-v2.json').read_bytes())
base='BESHStatNG.WorksheetFunctions.MultivariateAnalysisUDFs';path='BESHstatNG/src/ExcelUDFs/MultivariateAnalysisUDFs.vb'
snapshot=next(r['snapshot'] for r in json.loads((ROOT/'evidence/04A38/preedit/entry-source-snapshot.json').read_bytes())['files'] if r['source']['path']==path)
text,currentview=view((ROOT/path).read_bytes(),next(s['sha256'] for s in u['sources'] if s['path']==path))
oldtext,oldview=view((ROOT/snapshot['path']).read_bytes(),next(s['sha256'] for s in original['sources'] if s['path']==path))
def span(value,row):return value.encode('utf-16-le')[row['span_start']*2:(row['span_start']+row['span_length'])*2].decode('utf-16-le')
rows=[]
for name,fit,count in [('CorrespondenceHandle','CA_FIT',4),('MultipleCorrespondenceHandle','MCA_FIT',4),('DiscriminantHandle','DA_FIT',14)]:
    now=next(s for s in u['supports'] if s['kind']=='ClassBlock' and s['owner']==base and s['declaration']=='Private Class '+name)
    before=next(s for s in original['supports'] if s['kind']=='ClassBlock' and s['owner']==base and s['declaration']=='Private Class '+name)
    children=[s for s in u['supports'] if s['owner']==base+'.'+name]
    assert len(children)==count and all(s['kind']=='PropertyStatement' for s in children)
    assert span(text,now).replace('\r\n','\n')==span(oldtext,before).replace('\r\n','\n')
    rows.append(dict(source_support=now,original_support=before,current_whole_declaration=span(text,now),original_whole_declaration=span(oldtext,before),
        source=pin(ROOT/path),original_source=snapshot,current_view=currentview,original_view=oldview,children=children,
        properties=[re.search(r'Property (\w+)',s['declaration'])[1] for s in children],fit=fit,new_support=name!='CorrespondenceHandle',configurations=[]))
configs=[]
for config in ['Debug','Release']:
    meta=json.loads((HERE/('metadata-all/packet-v1-context-v5/current-'+config+'.json')).read_bytes())
    orig=json.loads((HERE/('metadata-all/packet-v1-context-v5/original-'+config+'.json')).read_bytes())
    previous=json.loads((PREVIOUS/('metadata-all/packet-v2-context-v5/current-'+config+'.json')).read_bytes())
    defs=definitions(meta);before=definitions(orig);pdefs=definitions(previous)
    types={t['type']:t for t in meta['types'] if t['type']};ots={t['type']:t for t in orig['types'] if t['type']}
    assert types[base]==ots[base],'Native module/cache fields changed'
    cctor=next(m for m in meta['methods'] if (m['declaring_type'],m['member_name'])==(base,'.cctor'))
    oldcctor=next(m for m in orig['methods'] if (m['declaring_type'],m['member_name'])==(base,'.cctor'))
    assert semantic(cctor)==semantic(oldcctor)
    for row in rows:
        owner=base+'+'+row['source_support']['declaration'].split()[-1];assert types[owner]==ots[owner]
        assert {f['name'] for f in types[owner]['fields']}=={'_'+name for name in row['properties']}
        assert all(not f['is_static'] for f in types[owner]['fields'])
        methods=[m for m in meta['methods'] if m['declaring_type']==owner];oldmethods=[m for m in orig['methods'] if m['declaring_type']==owner]
        assert len(methods)==2*len(row['properties'])+1 and {m['method'] for m in methods}=={m['method'] for m in oldmethods}
        assert all(semantic(m)==semantic(before[m['method']]) for m in methods)
        setters={m['method'] for m in methods if m['member_name'].startswith('set_')}
        writers=[dict(caller=m['method'],instruction=i,target=instruction['operand'][7:]) for m in meta['methods'] for i,instruction in enumerate(m['instructions'])
            if instruction['op'] in ['call','callvirt'] and isinstance(instruction['operand'],str) and instruction['operand'].startswith('METHOD:') and instruction['operand'][7:] in setters]
        fit=next(m for m in meta['methods'] if (m['declaring_type'],m['member_name'])==(base,row['fit']))
        stores=[i for i,instruction in enumerate(fit['instructions']) if instruction['op']=='callvirt' and isinstance(instruction['operand'],str) and '.set_Item(' in instruction['operand'] and 'ConcurrentDictionary' in instruction['operand']]
        assert len(stores)==1 and len(writers)==len(row['properties']) and all(w['caller']==fit['method'] and w['instruction']<stores[0] for w in writers)
        row['configurations'].append(dict(configuration=config,type=types[owner],original_type=ots[owner],methods=methods,original_methods=oldmethods,all_shipping_setter_callers=writers,fit_method=fit['method'],cache_insertion_instruction=stores[0]))
    def copies(method):return [i for i,x in enumerate(method['instructions']) if x['op']=='call' and isinstance(x['operand'],str) and 'RuntimeHelpers' in x['operand'] and '.GetObjectValue(' in x['operand']]
    boundaries=[]
    for name,api,owner in [('MCA_SUMMARY','BuildMcaSummary','UdfMultivariateReportPreparation38'),('DA_SUMMARY','BuildDiscriminantSummary','UdfMultivariateReportPreparation38'),
        ('TryGetCategoricalMatrix','CompleteCategoricalMatrixPrepared','UdfModelImportPreparation38'),('TryGetStringVector','TryGetStringVectorPrepared','UdfModelImportPreparation38'),('TryGetDoubleVector','TryGetDoubleVectorPrepared','UdfModelImportPreparation38')]:
        nativeowner=base if name.endswith('SUMMARY') else 'BESHStatNG.UdfDataImport'
        current=next(m for m in meta['methods'] if (m['declaring_type'],m['member_name'])==(nativeowner,name))
        old=next(m for m in previous['methods'] if (m['declaring_type'],m['member_name'])==(nativeowner,name))
        portable=next(m for m in meta['methods'] if (m['declaring_type'],m['member_name'])==('BESHStatNG.ExcelCommon.'+owner,api))
        assert len(copies(old))==len(copies(current))+len(copies(portable)),('Object copy boundary changed',name,copies(old),copies(current),copies(portable))
        if name=='TryGetCategoricalMatrix':assert portable['parameter_types'][1]=='System.Object&'
        named=[x['operand'][7:] for x in current['instructions'] if x['op']=='ldftn' and isinstance(x['operand'],str) and x['operand'].startswith('METHOD:')]
        expected={'TryGetCategoricalMatrix':['IsBlankCell','CellToTrimmedText','ResolveImportLabelNames'],'TryGetStringVector':['CellToTrimmedText'],'TryGetDoubleVector':['TryGetDouble']}.get(name,[])
        assert [defs[k]['member_name'] for k in named]==expected
        boundaries.append(dict(native=current,previous_native=old,portable=portable,previous_copy_sites=copies(old),native_copy_sites=copies(current),portable_copy_sites=copies(portable),
            named_policy_targets=[defs[k] for k in named],exception_policy='No delegate.DynamicInvoke, new Catch or wrapper; exact named direct delegates propagate original exception instance. Native catch and Err policy unchanged. New layout/prepared stack frames acknowledged.'))
    configs.append(dict(configuration=config,module_type=types[base],original_module_type=ots[base],cctor=cctor,original_cctor=oldcctor,boundaries=boundaries))
save('host-cache-contexts-v1.json',dict(rows=rows,configurations=configs,native_execution='NOT_RUN',original_queries=0,
    scope='Three source-declared handles, every backing field/accessor/constructor and every shipping writer before original FIT cache publication; module fields/initializer unchanged; five exact additive Object-copy boundaries and real named host policies.'))
print('All three host instance contexts, writers and five exact Object-copy boundaries authenticated')
