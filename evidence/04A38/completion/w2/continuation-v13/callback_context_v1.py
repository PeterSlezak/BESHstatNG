"""Exact KM count/fill slots and independently bound original-only normalization callback."""
from common import *
from source_views import body
from proof_kernel_v4 import bind,definitions
before=json.loads((PREVIOUS/'current-ownership-v2.json').read_bytes())
after=json.loads((HERE/'current-universe-packet-v1.json').read_bytes())
original=json.loads((W2.parent/'w1/original-universe.json').read_bytes())
bm={n['symbol_id']:n for n in before['members']};am={n['symbol_id']:n for n in after['members']};om={n['symbol_id']:n for n in original['members']}
shapes={r['symbol_id']:r for r in json.loads((HERE/'declaration-shapes-v1.json').read_bytes())['rows']}
oldshapes={r['symbol_id']:r for r in json.loads((PREVIOUS/'declaration-shapes-v1.json').read_bytes())['rows']}
snapshots={r['source']['path']:r['snapshot'] for r in json.loads((ROOT/'evidence/04A38/preedit/entry-source-snapshot.json').read_bytes())['files']}
oldparent=next(n for n in bm.values() if n['name']=='BuildKaplanMeierStepTable');newparent=next(n for n in am.values() if n['name']=='BuildKaplanMeierStepTable')
oldchildren=sorted([n for n in bm.values() if n['parent_member']==oldparent['symbol_id']],key=lambda n:n['span_start'])
newchildren=sorted([n for n in am.values() if n['parent_member']==newparent['symbol_id']],key=lambda n:n['span_start'])
assert len(oldchildren)==len(newchildren)==2
phase=next(r for r in json.loads((HERE/'implementation-source-composition-v2.json').read_bytes())['phases'] if r['parent']=='BuildKaplanMeierStepTable')
oldraw=(HERE/'source-before'/(oldparent['path']+'.txt')).read_bytes();newraw=(ROOT/newparent['path']).read_bytes()
oldtext,_=body(oldraw,oldparent);newtext,_=body(newraw,newparent)
assert oldtext.replace(phase['original_segment'],phase['actual_native_call'])==newtext
pu=json.loads((HERE/'portable-universe-v1.json').read_bytes())
commonparent=next(n for n in pu['members'] if n['name']=='BuildKaplanMeierStepTablePrepared')
normalparent=next(n for n in pu['members'] if n['name']=='GuessPositiveClass' and n['owner']=='BESHStatNG.ExcelCommon.UdfClassLabelPreparation38')
normalchild=next(n for n in pu['members'] if n['parent_member']==normalparent['symbol_id'])
history=json.loads((PREVIOUS/'historical-ownership-v2.json').read_bytes())['members']
oldnormalparent=next(n for n in om.values() if n['name']=='GuessPositiveClass')
oldnormalchild=next(n for n in om.values() if n['parent_member']==oldnormalparent['symbol_id'])
historical=next(h for h in history if h['git_original_id']==oldnormalchild['symbol_id']);assert not historical['proof'] and historical['current_ids']==[]
records=[];normalrecords=[]
def targets(method,defs):return [i['operand'][7:] for i in method['instructions'] if i['op']=='ldftn' and i['operand'][7:] in defs and defs[i['operand'][7:]]['member_name'].startswith('_Lambda$__')]
for config in ['Debug','Release']:
    prior=json.loads((PREVIOUS/('metadata-all/packet-v1-context-v5/current-'+config+'.json')).read_bytes())
    meta=json.loads((HERE/('metadata-all/packet-v1-context-v5/current-'+config+'.json')).read_bytes())
    orig=json.loads((HERE/('metadata-all/packet-v1-context-v5/original-'+config+'.json')).read_bytes())
    pd=definitions(prior);md=definitions(meta);od=definitions(orig)
    bp=bind(oldparent,prior,oldshapes);cp=bind(newparent,meta,shapes);ps=targets(bp,pd);cs=targets(cp,md)
    assert len(ps)==len(cs)==2 and ps==cs
    portable=bind(commonparent,meta,shapes)
    selections=[j for j,i in enumerate(portable['instructions']) if i['op'] in ['call','callvirt'] and '.Select' in (i.get('operand') or '') and 'Enumerable' in i['operand']]
    assert len(selections)==2
    for number,index in enumerate(selections,1):
        window=portable['instructions'][max(0,index-5):index]
        assert any(i['op']=='ldarg.'+str(number) for i in window),'Missing actual count/fill selector parameter load'
    for index,(old,new) in enumerate(zip(oldchildren,newchildren))):
        oldbody,_=body(oldraw,old);newbody,_=body(newraw,new);assert oldbody==newbody
        assert pd[ps[index]]==md[cs[index]],'Actual existing native cast callback CIL/signature/context changed'
        owner=md[cs[index]]['declaring_type']
        currenttype=next(t for t in meta['types'] if t['type']==owner);priortype=next(t for t in prior['types'] if t['type']==owner)
        assert currenttype==priortype
        records.append(dict(configuration=config,prior_current_id=old['symbol_id'],current_id=new['symbol_id'],role=['countRecord','fillRecord'][index],
            source_sibling_index=index,original_relative_utf16=[466,1457][index],current_relative_utf16=new['span_start']-newparent['span_start'],
            complete_body=newbody,prior_parent=bp['method'],current_parent=cp['method'],actual_ldftn=cs[index],complete_callback=md[cs[index]],
            owner_context=currenttype,constructors=[m for m in meta['methods'] if m['declaring_type']==owner and m['member_name'] in ['.ctor','.cctor']],
            portable_method=portable,portable_select_instruction=selections[index],parameter_slot=index+1,
            composition=pin(HERE/'implementation-source-composition-v2.json')))
    # Archived original lambda -> actual current Common lambda; no original DLL
    # load, compilation, query or new capture is involved.
    os=dict(shapes);os[oldnormalparent['symbol_id']]=oldshapes[next(n['symbol_id'] for n in bm.values() if n['name']=='GuessPositiveClass')]
    op=bind(oldnormalparent,orig,os);np=bind(normalparent,meta,shapes);ots=targets(op,od);nts=targets(np,md)
    assert len(ots)==len(nts)==1
    oldlambda=od[ots[0]];newlambda=md[nts[0]]
    oldcalls=[i['operand'][7:] for i in oldlambda['instructions'] if i['op']=='call' and '.NormalizeOptionKey(' in (i.get('operand') or '')]
    newcalls=[i['operand'][7:] for i in newlambda['instructions'] if i['op']=='call' and '.NormalizeOptionKey(' in (i.get('operand') or '')]
    assert len(oldcalls)==len(newcalls)==1
    normalized=[dict(i,operand='METHOD:'+newcalls[0]) if i.get('operand')=='METHOD:'+oldcalls[0] else i for i in oldlambda['instructions']]
    assert normalized==newlambda['instructions'] and oldlambda['parameter_types']==newlambda['parameter_types'] and oldlambda['locals']==newlambda['locals'] and oldlambda['exception_regions']==newlambda['exception_regions']
    native_normal=next(n for n in om.values() if n['owner']=='BESHStatNG.UdfDataImport' and n['name']=='NormalizeOptionKey')
    common_full=json.loads((HERE/'dependency-full-common-v1.json').read_bytes())
    portable_normal=next(n for n in common_full['members'] if n['owner']=='BESHStatNG.ExcelCommon.UdfDataImportOptions38' and n['name']=='NormalizeOptionKey')
    source_old,_=body((ROOT/snapshots[native_normal['path']]['path']).read_bytes(),native_normal)
    source_new,_=body((ROOT/portable_normal['path']).read_bytes(),portable_normal)
    content=lambda s:'\n'.join(line.strip() for line in s.splitlines()[1:] if line.strip())
    assert content(source_old)==content(source_new),'Original/portable normalization body changed'
    callback_old,_=body((ROOT/snapshots[oldnormalchild['path']]['path']).read_bytes(),oldnormalchild)
    callback_new,_=body((ROOT/normalchild['path']).read_bytes(),normalchild)
    assert callback_old=='Function(x) NormalizeOptionKey(x)' and callback_new=='Function(token) UdfDataImportOptions38.NormalizeOptionKey(token)'
    normalrecords.append(dict(configuration=config,original_callback=oldlambda,current_callback=newlambda,
        original_parent=op['method'],current_parent=np['method'],original_normalizer=od[oldcalls[0]],current_normalizer=md[newcalls[0]],
        original_normalizer_source=source_old,current_normalizer_source=source_new,original_callback_source=callback_old,current_callback_source=callback_new,
        owner_context=next(t for t in meta['types'] if t['type']==newlambda['declaring_type']),
        constructors=[m for m in meta['methods'] if m['declaring_type']==newlambda['declaring_type'] and m['member_name'] in ['.ctor','.cctor']]))
mapping={r['prior_current_id']:r['current_id'] for r in records};assert len(mapping)==2
save('callback-identity-context-v1.json',dict(status='EXACT_SOURCE_COMPOSITE_AND_ACTUAL_CIL_PARAMETER_SLOT_IDENTITIES_VERIFIED',mapping=mapping,rows=records,
    prior_parent=oldparent,current_parent=newparent,source_before=pin(HERE/'source-before'/(oldparent['path']+'.txt')),source_after=pin(ROOT/newparent['path']),
    scope='Only these two exact KM callbacks. No generic ordinal, same-name or body-only alias is permitted. Count/fill ownership requires full source reconstruction and actual portable parameter loads.',original_queries=0))
save('portable-historical-callback-proof-v1.json',dict(status='ORIGINAL_ONLY_CALLBACK_COMPLETE_SOURCE_NORMALIZER_AND_CIL_RECONCILED',
    original_id=historical['canonical_original_id'],original_record=oldnormalchild,original_parent=oldnormalparent,current_source_record=normalchild,current_parent=normalparent,
    original_source=snapshots[oldnormalchild['path']],current_source=pin(ROOT/normalchild['path']),compiled=normalrecords,
    current_Windows_ids=[],qualification='Historical original callback has a real current Common source/compiled counterpart, not a guessed Windows callback ID. Every parameter, normalization body and actual CIL instruction/exception region is authenticated independently. Ledger transition remains separate.',original_queries=0))
save('entry-original-callback-addendum-v1.json',dict(status='ADDITIVE_ORIGINAL_GRAPH_CALLBACK_DEPENDENCY_NO_NEW_REFERENCE',
    queue=pin(HERE/'entry-queue-receipt-v1.json'),historical_callback=historical['canonical_original_id'],proof=pin(HERE/'portable-historical-callback-proof-v1.json'),
    reason='The original GuessPositiveClass callback is a necessary child of an initial ROC dependency. Its counterpart is an already existing Common lambda; the three native row policies are separate current-only callbacks. Initial current graph does not substitute for this original callback proof.',production_source_frozen=True,original_queries=0))
print('Separate KM count/fill identities and original-only Common normalization callback proved in both configurations')
