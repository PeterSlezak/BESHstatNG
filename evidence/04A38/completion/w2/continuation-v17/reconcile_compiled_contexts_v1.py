"""Cache/initializer/property/delegate boundaries and exact original conversion graph."""
from proof_support_v17_v3 import *
from collections import Counter
u=load(FINAL/'current-universe.json');o=load(W2.parent/'w1/original-universe.json');c=load(FINAL/'dependency-full-common.json')
meta=load(FINAL/'current-Debug.json');original=load(W2/'continuation-v14/metadata-all/packet-v1-context-v5/original-Debug.json');prior=load(PREVIOUS/'final-v5/current-Debug.json')
setup(meta);setup(original)
snapshots={r['source']['path']:r['snapshot'] for r in load(ROOT/'evidence/04A38/preedit/entry-source-snapshot.json')['files']}
contexts=[]
for family,name in [('MixedModelUDFs','MmrmHandle'),('MixedModelUDFs','MmrmLsmEstimateProfileValue'),('MixedModelUDFs','MmrmLsmEstimateComponent'),('MixedModelUDFs','MmrmLsmEstimateAtProfile'),('MultinomialLogitUDFs','MultinomialLogitHandle'),('OrdinalLogitUDFs','OrdinalLogitHandle'),('ZIPUDFs','ZipHandle')]:
 owner='BESHStatNG.WorksheetFunctions.'+family
 matching=[t for t in meta['types'] if t['type']==owner+'+'+name]
 if not matching:
  # Actual source owners, never an inferred equivalent-looking cache identity.
  choices=[t for t in meta['types'] if t['type'] and t['type'].startswith(owner+'+') and '_Closure$' not in t['type']]
  assert family in ['MultinomialLogitUDFs','OrdinalLogitUDFs','ZIPUDFs'] and len(choices)==1
  matching=choices;name=matching[0]['type'].split('+')[1]
 nt=matching[0];native=nt['type'];ot=next(t for t in original['types'] if t['type']==native)
 current=[m for m in meta['methods'] if m['declaring_type']==native];old=[m for m in original['methods'] if m['declaring_type']==native]
 properties=[]
 if 'LsmEstimate' not in name:
  assert Counter(json.dumps(f,sort_keys=True) for f in nt['fields'])==Counter(json.dumps(f,sort_keys=True) for f in ot['fields'])
  assert {m['member_name'] for m in current}=={m['member_name'] for m in old}
  for m in current:assert instructions(m)==instructions(next(x for x in old if x['member_name']==m['member_name']))
  mode='EXACT_ORIGINAL_NATIVE_CACHE_FIELDS_CTOR_ACCESSORS_AND_LIFETIME_BOUNDARY'
 else:
  base=nt['base_type'].split(' @ ',1)[0];root=base.split('`',1)[0]
  original_props=sorted([n for n in o['members'] if n['owner']==owner+'.'+name and n['kind']=='PropertyStatement'],key=lambda n:n['span_start'])
  portable_props=sorted([n for n in c['members'] if n['owner']==root and n['kind']=='PropertyStatement'],key=lambda n:n['span_start'])
  assert [n['name'] for n in original_props]==[n['name'] for n in portable_props]
  for a,n in zip(original_props,portable_props):
   before=body((ROOT/snapshots[a['path']]['path']).read_bytes(),a)[0];after=body((ROOT/n['path']).read_bytes(),n)[0]
   same(before.replace('MmrmLsmEstimateProfileValue','TValue'),after,'Complete property '+native+'.'+n['name'])
   getter=method(meta,base,'get_'+n['name']);setter=method(meta,base,'set_'+n['name']);oldsetter=next(m for m in old if m['member_name']=='set_'+n['name'])
   assert setter['parameter_types']==oldsetter['parameter_types'];assert [x['op'] for x in setter['instructions']]==['ldarg.0','ldarg.1','stfld','ret']
   assert sum(x['op']=='ldfld' for x in getter['instructions'])==1 and not getter['exceptions']
   properties.append(dict(original=a,portable=n,original_source=snapshots[a['path']],portable_source=pin(ROOT/n['path']),original_declaration=before,portable_declaration=after,getter=getter,setter=setter))
  oc=next(m for m in old if m['member_name']=='.ctor');bc=method(meta,base,'.ctor')
  assert [x['op'] for x in oc['instructions']]==[x['op'] for x in bc['instructions']]
  assert [x for x in oc['instructions'] if x['op'].startswith('ldc.')]==[x for x in bc['instructions'] if x['op'].startswith('ldc.')]
  assert len(current)==1 and not nt['fields'];assert current[0]['instructions'][1]['operand']=='METHOD:'+bc['method']
  mode='FULL_PROPERTY_DECLARATION_DEFAULT_ORDER_NATIVE_CTOR_AND_CONSTRUCTED_COMMON_BASE'
 targets={m['method'] for m in current}
 consumers=[dict(caller=m['method'],index=i,instruction=x) for m in meta['methods'] for i,x in enumerate(m['instructions']) if isinstance(x['operand'],str) and x['operand'].startswith('METHOD:') and x['operand'][7:] in targets]
 contexts.append(dict(type=nt,original_type=ot,current_native_methods=current,original_methods=old,properties=properties,consumers=consumers,mode=mode))
nd=definitions(meta);pd=definitions(prior);changed=[];removed=[]
for key,m in pd.items():
 if key not in nd:removed.append(m);continue
 if instructions(m)!=instructions(nd[key]):changed.append(dict(before=m,after=nd[key]))
assert not any(r['before']['declaring_type'] and r['before']['method'].split(' @ ',1)[1].split(',',1)[0]=='BESHStat.Core' for r in changed)
existing_common=[r for r in changed if r['before']['method'].split(' @ ',1)[1].split(',',1)[0]=='BESHStat.Excel.Common']
assert len(existing_common)==1 and existing_common[0]['before']['member_name']=='NormalizeMixedModelResidualCovarianceName'
assert len(removed)==3 and all(m['member_name'].startswith('_Lambda$__') and 'UdfDataImport' in m['declaring_type'] for m in removed)
owner='BESHStatNG.UdfScalarUtilities';name='NormalizeMixedModelResidualCovarianceName';now=method(meta,owner,name);old=method(original,owner,name)
conv=lambda m:[dict(index=i,instruction=x) for i,x in enumerate(m['instructions']) if x['op'] in ['call','callvirt'] and '.AsString(' in str(x['operand'])]
assert len(conv(now))==2 and len(conv(old))==1
oldtoken=method(original,owner,'NormalizeMixedModelOptionToken');assert len(conv(oldtoken))==1
asstring=method(meta,'BESHStatNG.WorksheetFunctions.ExcelArgReaders','AsString');oldas=method(original,'BESHStatNG.WorksheetFunctions.ExcelArgReaders','AsString');assert instructions(asstring)==instructions(oldas)
classifier=method(meta,'BESHStatNG.ExcelCommon.UdfScalarUtilitiesOptions38','TryClassifyMixedModelResidualCovarianceName')
assert classifier['parameter_types'][-1]=='System.String&' and classifier['signature']['return_type'].startswith('System.Boolean @ ')
assert not now['exceptions'] and not classifier['exceptions']
ci_source=ROOT/'BESHStat.Core/src/StatTests/Survival.vb';text=ci_source.read_text(encoding='utf-8-sig')
assert 'temp(i, 0) = MedianSurv(i)' in text and 'temp(i, 1) = ll(i)' in text and 'temp(i, 2) = ul(i)' in text
save('constructed-host-and-property-contexts-v1.json',dict(rows=contexts,Release=DEFERRED,original_queries=0))
save('compiled-method-deltas-v1.json',dict(changed=changed,removed_native_callbacks=removed,added=[nd[k] for k in sorted(nd.keys()-pd.keys())],Core_changed=0,existing_Common_changed=1,approved_Common_change='Original Object reconversion boundary plus retained String API fallback',Release=DEFERRED,original_queries=0))
save('conversion-and-alias-boundaries-v1.json',dict(native_covariance=now,original_covariance=old,original_token=oldtoken,current_classifier=classifier,native_AsString=asstring,original_AsString=oldas,ordered_current_conversions=conv(now),decision='First native conversion precedes pure classification. False/unknown branch alone reaches second conversion. Recognized/blank branch returns canonical. No new catch/clone; default and original exception remain native.',intercept_alias_correction=pin(HERE/'intercept-alias-correction-v1.json'),median_unbox_domain=dict(source=pin(ci_source),authority='Unchanged existing Core producer assigns boxed Double from typed MedianSurv/ll/ul arrays to every CI cell; DirectCast has the same reachable result without a VB helper.'),Release=DEFERRED,original_queries=0))
print('Complete cache/property contexts',len(contexts),'unchanged Core CIL; three moved callbacks; one approved Common API body change; covariance two-stage conversion')
