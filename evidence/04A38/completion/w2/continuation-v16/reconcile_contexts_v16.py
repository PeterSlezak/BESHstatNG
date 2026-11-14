"""Current Debug native state and typed-row projection; no archived execution."""
from proof_support_v16 import *
from collections import Counter
OUT=HERE
u=load(HERE/'final-v4/current-universe.json');o=load(W2.parent/'w1/original-universe.json')
common=load(HERE/'final-v4/dependency-full-common.json')
meta=load(HERE/'final-v5/current-Debug.json')
original=load(W2/'continuation-v14/metadata-all/packet-v1-context-v5/original-Debug.json')
snapshots={r['source']['path']:r['snapshot'] for r in load(ROOT/'evidence/04A38/preedit/entry-source-snapshot.json')['files']}
proof=load(HERE/'whole-body-contracts-v1.json')['contracts']
moves=load(HERE/'packet-B-source-moves-v1.json')['moves']
getmove=next(r for r in moves if r['kind']=='PROPERTY_GETTER_VALUE_PROJECTION')
def declaration(universe,node,data):
 text,policy=view(data,next(s['sha256'] for s in universe['sources'] if s['path']==node['path']))
 return text.encode('utf-16-le')[node['span_start']*2:(node['span_start']+node['span_length'])*2].decode('utf-16-le'),policy
def methods(owner,m):return [r for r in m['methods'] if r['declaring_type']==owner]
def writes(m):return [r for r in m['instructions'] if r['op']=='stfld']
rows=[]
for family,name in [('GLMNbUDFs','GlmNbHandle'),('GLMUDFs','GlmHandle'),('LinearModelUDFs','LinearModelHandle'),('LmmUDFs','LmmHandle'),('LmmUDFs','LmmImportedInputs')]:
 owner='BESHStatNG.WorksheetFunctions.'+family
 support=next(s for s in u['supports'] if s['owner']==owner and s['declaration']==('Friend Class ' if name=='LmmHandle' else 'Private Class ')+name)
 before=next(s for s in o['supports'] if (s['path'],s['owner'],s['kind'],s['declaration'])==(support['path'],support['owner'],support['kind'],support['declaration']))
 now,nv=declaration(u,support,(ROOT/support['path']).read_bytes())
 prior,pv=declaration(o,before,(ROOT/snapshots[before['path']]['path']).read_bytes())
 expected=prior
 if name=='LmmImportedInputs':
  assert expected.count(getmove['original_segment'])==1
  expected=expected.replace(getmove['original_segment'],getmove['native_call'],1)
 assert tokens(expected)==tokens(now),'Full state declaration changed '+name
 native=owner+'+'+name
 current_type=next(t for t in meta['types'] if t['type']==native)
 original_type=next(t for t in original['types'] if t['type']==native)
 assert Counter(json.dumps(f,sort_keys=True) for f in current_type['fields'])==Counter(json.dumps(f,sort_keys=True) for f in original_type['fields'])
 current=methods(native,meta);old=methods(native,original)
 byname={r['member_name']:r for r in old}
 assert len(current)==len(old)
 for method in current:
  if method['member_name']=='get_N' and name=='LmmImportedInputs':continue
  assert cil(method)==cil(byname[method['member_name']]),'Unplanned native state CIL change '+method['method']
 targets={r['method'] for r in current}
 consumers=[dict(caller=r['method'],index=i,instruction=x) for r in meta['methods'] for i,x in enumerate(r['instructions']) if isinstance(x['operand'],str) and x['operand'].startswith('METHOD:') and x['operand'][7:] in targets]
 if name=='LmmImportedInputs':
  gety=next(r for r in current if r['member_name']=='get_Y')
  sety=next(r for r in current if r['member_name']=='set_Y')
  assert sum(x['op']=='ldfld' for x in gety['instructions'])==1 and not gety['exceptions'] and not any(x['op'] in ['call','callvirt','stfld'] for x in gety['instructions'])
  assert [x['op'] for x in sety['instructions']]==['ldarg.0','ldarg.1','stfld','ret'] and not sety['exceptions']
  getter=next(r for r in current if r['member_name']=='get_N')
  pure=next(r for r in meta['methods'] if r['declaring_type']=='BESHStatNG.ExcelCommon.UdfLmmReportPreparation38' and r['member_name']=='ObservationCount')
  assert not pure['exceptions'] and not any(x['op'] in ['call','callvirt','stfld','stsfld'] for x in pure['instructions'])
  calls=[x['operand'] for x in getter['instructions'] if x['op'] in ['call','callvirt']]
  assert calls==['METHOD:'+gety['method'],'METHOD:'+pure['method']]
 rows.append(dict(source=support,original_source=before,current_declaration=now,original_declaration=prior,current_view=nv,original_view=pv,original_snapshot=snapshots[before['path']],current_type=current_type,original_type=original_type,current_methods=current,original_methods=old,all_compiled_construction_and_accessor_consumers=consumers,mode='COMPLETE_UNCHANGED_NATIVE_STATE' if name!='LmmImportedInputs' else 'COMPLETE_NATIVE_STATE_WITH_PURE_N_GET_VALUE_PROJECTION',Release=DEFERRED))

# No constructor or property introduces copying, normalization, side effects,
# ownership of cache/session state, or a partial native publication on failure.
dto='BESHStatNG.ExcelCommon.UdfLmmPreparedRows38'
dm=methods(dto,meta);dt=next(t for t in meta['types'] if t['type']==dto)
props=[dict(n,name=re.search(r'Property (\w+)',n['declaration'])[1]) for n in common['supports'] if n['owner']==dto and n['kind']=='PropertyStatement']
assert {n['name'] for n in props}=={'Y','X','Z','Subject','Visit','DroppedRows'}
for n in props:
 getter=next(r for r in dm if r['member_name']=='get_'+n['name']);setter=next(r for r in dm if r['member_name']=='set_'+n['name'])
 assert sum(x['op']=='ldfld' for x in getter['instructions'])==1 and not getter['exceptions']
 assert not any(x['op'] in ['call','callvirt','stfld','stsfld'] for x in getter['instructions'])
 assert [x['op'] for x in setter['instructions']]==['ldarg.0','ldarg.1','stfld','ret'] and not setter['exceptions']
ctor=next(r for r in dm if r['member_name']=='.ctor')
assert not writes(ctor) and not ctor['exceptions'] and [x['op'] for x in ctor['instructions']]==['ldarg.0','call','ret']
assert ctor['instructions'][1]['operand'].startswith('METHOD:System.Object @ ')
rowmove=next(r for r in moves if r['kind']=='TYPED_PREPARED_ROWS_AND_NATIVE_NAME_METADATA_COMPOSITE')
assert rowmove['native_call'].index('If Not Global.')<rowmove['native_call'].index('imported = New LmmImportedInputs')
contexts=[]
for owner in ['GLMNbUDFs','GLMUDFs','LinearModelUDFs','LmmUDFs']:
 native='BESHStatNG.WorksheetFunctions.'+owner
 for m in methods(native,meta):
  if m['member_name']!='.cctor':continue
  old=next(r for r in methods(native,original) if r['member_name']=='.cctor')
  assert cil(m)==cil(old)
  contexts.append(dict(owner=native,current=m,original=old,mode='EXACT_ORIGINAL_WINDOWS_MODULE_INITIALIZATION'))
write_json(OUT,'native-state-contexts-v1.json',dict(status='COMPLETE_NATIVE_STATE_AND_TYPED_ROW_BRIDGE',classes=rows,dto=dict(type=dt,methods=dm,properties=props,projection=rowmove,policy='Array/reference aliases retained. Data conversion/filtering completes before native imported object publication; six pure auto-properties carry prepared values. Name/letter metadata remains native.'),module_initializers=contexts,original_queries=0,Release=DEFERRED))
print('Native state contexts',len(rows),'and six-property DTO proved',flush=True)
