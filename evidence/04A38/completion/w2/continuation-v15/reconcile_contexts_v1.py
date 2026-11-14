"""Full native cache/data-class declarations, properties, constructors and actual writers."""
from common import *
from source_views import view,body
from verify_c14_composition_v2 import tokens
from collections import Counter

load=lambda path:json.loads(path.read_bytes())
u=load(HERE/'final-v1/current-universe.json');o=load(W2.parent/'w1/original-universe.json')
common=load(HERE/'final-v1/dependency-full-common.json')
meta={cfg:load(HERE/('final-v1/current-'+cfg+'.json')) for cfg in ['Debug','Release']}
orig={cfg:load(PREVIOUS/('metadata-all/packet-v1-context-v5/original-'+cfg+'.json')) for cfg in meta}
returns={cfg:load(ROOT/('evidence/04A38/preedit/compiled-'+cfg+'.json'))['methods'] for cfg in meta}
snapshots={r['source']['path']:r['snapshot'] for r in load(ROOT/'evidence/04A38/preedit/entry-source-snapshot.json')['files']}
key=lambda s:(s['path'],s['kind'],s['owner'],s['declaration'])


def declaration(universe,node,data):
    text,policy=view(data,next(s['sha256'] for s in universe['sources'] if s['path']==node['path']))
    result=text.encode('utf-16-le')[node['span_start']*2:(node['span_start']+node['span_length'])*2].decode('utf-16-le')
    assert tokens(result).startswith(tokens(node['declaration']))
    return result,policy


def cil(method):return {k:v for k,v in method.items() if k not in ['raw_IL_sha256','signature']}


rows=[]
for owner,name in [('CoxUDFs','CoxModelHandle'),('GEEUDFs','GeeHandle'),
                   ('GEEUDFs','GeeLsmEstimateProfileValue'),('GEEUDFs','GeeLsmEstimateComponent'),('GEEUDFs','GeeLsmEstimateAtProfile')]:
    source_owner='BESHStatNG.WorksheetFunctions.'+owner
    support=next(s for s in u['supports'] if s['owner']==source_owner and s['declaration']=='Private Class '+name)
    archived=next(s for s in o['supports'] if key(s)==key(support))
    now,nv=declaration(u,support,(ROOT/support['path']).read_bytes())
    before,bv=declaration(o,archived,(ROOT/snapshots[archived['path']]['path']).read_bytes())
    native=source_owner+'+'+name
    oldprops=sorted([n for n in o['members'] if n['owner']==source_owner+'.'+name and n['kind']=='PropertyStatement'],key=lambda n:n['span_start'])
    oldproptexts={n['name']:body((ROOT/snapshots[n['path']]['path']).read_bytes(),n)[0] for n in oldprops}
    compiled=[]
    for cfg,m in meta.items():
        current_type=next(t for t in m['types'] if t['type']==native)
        original_type=next(t for t in orig[cfg]['types'] if t['type']==native)
        native_methods=[r for r in m['methods'] if r['declaring_type']==native]
        original_methods=[r for r in orig[cfg]['methods'] if r['declaring_type']==native]
        old_by_name={r['member_name']:r for r in original_methods}
        if name in ['CoxModelHandle','GeeHandle']:
            assert tokens(now)==tokens(before)
            assert len(native_methods)==len(original_methods)
            assert all(cil(r)==cil(old_by_name[r['member_name']]) for r in native_methods)
            field_key=lambda values:Counter(json.dumps(v,sort_keys=True) for v in values)
            assert field_key(current_type['fields'])==field_key(original_type['fields'])
            base_methods=[];property_sources=[];mode='EXACT_NATIVE_DECLARATION_CTOR_GETTERS_SETTERS_AND_FIELDS'
        else:
            base=current_type['base_type'].split(' @ ',1)[0]
            base_root=base.split('`',1)[0]
            sourceprops=sorted([n for n in common['members'] if n['owner']==base_root and n['kind']=='PropertyStatement'],key=lambda n:n['span_start'])
            assert [n['name'] for n in sourceprops]==[n['name'] for n in oldprops]
            property_sources=[]
            for n in sourceprops:
                text,policy=body((ROOT/n['path']).read_bytes(),n)
                expected=oldproptexts[n['name']].replace('GeeLsmEstimateProfileValue','TValue') if name!='GeeLsmEstimateProfileValue' else oldproptexts[n['name']]
                assert tokens(text)==tokens(expected), 'Moved complete property/default declaration differs '+name+'.'+n['name']
                property_sources.append(dict(source=n,whole_declaration=text,source_view=policy,source_file=pin(ROOT/n['path']),original_declaration=oldproptexts[n['name']]))
            base_methods=[r for r in m['methods'] if r['declaring_type']==base and (r['member_name'] in ['.ctor']+[prefix+n['name'] for n in oldprops for prefix in ['get_','set_']])]
            assert len(base_methods)==1+2*len(oldprops)
            for n in oldprops:
                getter=next(r for r in base_methods if r['member_name']=='get_'+n['name'])
                setter=next(r for r in base_methods if r['member_name']=='set_'+n['name'])
                old_return=next(r for r in returns[cfg] if r['declaring_type']==native and r['name']=='get_'+n['name'])
                assert getter['signature']['return_type'].split(' @ ',1)[0]==old_return['return_type']
                assert setter['parameter_types']==old_by_name['set_'+n['name']]['parameter_types']
                assert sum(x['op']=='ldfld' for x in getter['instructions'])==1 and not getter['exceptions']
                assert sum(x['op']=='stfld' for x in setter['instructions'])==1 and not setter['exceptions']
            oldctor=old_by_name['.ctor'];basector=next(r for r in base_methods if r['member_name']=='.ctor')
            assert [x['op'] for x in oldctor['instructions']]==[x['op'] for x in basector['instructions']]
            constants=lambda r:[x for x in r['instructions'] if x['op'].startswith('ldc.')]
            assert constants(oldctor)==constants(basector)
            writes=lambda r:[x['operand'].split(':',1)[1].split(':',1)[0].rsplit('.',1)[-1] for x in r['instructions'] if x['op']=='stfld']
            assert writes(oldctor)==writes(basector)
            for ctor in [oldctor,basector]:
                for x in ctor['instructions']:
                    if x['op']=='call':assert x['operand'].startswith('METHOD:System.Object @ ')
                    if x['op']=='newobj':assert x['operand'].startswith('METHOD:System.Collections.Generic.List`1')
            assert len(native_methods)==1 and native_methods[0]['member_name']=='.ctor'
            assert [x['op'] for x in native_methods[0]['instructions']]==['ldarg.0','call','ret']
            assert native_methods[0]['instructions'][1]['operand']=='METHOD:'+basector['method']
            assert not current_type['fields']
            mode='EXACT_PROPERTY_DEFAULT_SEQUENCE_AND_CONSTRUCTED_INHERITED_ACCESSOR_AND_CTOR_COMPOSITE'
        targets={r['method'] for r in native_methods+base_methods}
        writers=[dict(caller=r['method'],instruction=i,op=x['op'],target=x['operand'][7:]) for r in m['methods'] for i,x in enumerate(r['instructions']) if x['op'] in ['call','callvirt','newobj','ldftn','ldvirtftn'] and isinstance(x['operand'],str) and x['operand'].startswith('METHOD:') and x['operand'][7:] in targets]
        compiled.append(dict(configuration=cfg,current_native_type=current_type,original_type=original_type,current_native_methods=native_methods,
                             original_methods=original_methods,current_inherited_methods=base_methods,moved_property_sources=property_sources,
                             all_actual_compiled_construction_and_accessor_consumers=writers,mode=mode))
    rows.append(dict(source_support=support,original_support=archived,current_whole_declaration=now,original_whole_declaration=before,
                     current_view=nv,original_view=bv,original_snapshot=snapshots[archived['path']],compiled=compiled,
                     original_queries=0))

# A delegate introduced for the report is the unchanged host display policy; enumerate its actual construction and use.
delegates=[]
for cfg,m in meta.items():
    parent=next(r for r in m['methods'] if r['declaring_type']=='BESHStatNG.WorksheetFunctions.CoxUDFs' and r['member_name']=='COX_SUMMARY')
    target=next(r for r in m['methods'] if r['declaring_type']=='BESHStatNG.UdfRegressionOutput' and r['member_name']=='ExpForDisplay')
    sites=[i for i,x in enumerate(parent['instructions']) if x['op']=='ldftn' and x['operand']=='METHOD:'+target['method']]
    assert len(sites)==1
    prepared=next(r for r in m['methods'] if r['declaring_type']=='BESHStatNG.ExcelCommon.UdfCoxGeeReportPreparation38' and r['member_name']=='BuildCoxSummaryTable')
    invocations=[dict(index=i,instruction=x) for i,x in enumerate(prepared['instructions']) if x['op']=='callvirt' and 'System.Func`2' in str(x['operand']) and '.Invoke(' in str(x['operand'])]
    assert len(invocations)==3 and not prepared['exceptions']
    delegates.append(dict(configuration=cfg,parent=parent,target=target,construction_index=sites[0],construction=parent['instructions'][sites[0]:sites[0]+2],prepared=prepared,ordered_invocations=invocations,
                          scope='Three ordered calls to the unchanged fixed native display policy; no new source lambda/cached-model reference is introduced into Common.'))

save('host-cache-contexts-v1.json',dict(rows=rows,display_delegates=delegates,original_queries=0,
    policy='Native ConcurrentDictionary, handle creation/drop, wizard, import/error/exception/logging and mutable fitted-model lifetime remain Windows-owned. No Common helper retains a cache/model handle. Current-only focused tests and original compiled source contracts remain distinct from a new oracle.'))
print('Five complete native data/cache class contexts and fixed display delegate contracts proved')
