"""Independent source + complete dual-configuration metadata proof of host caches.
No model, constructor, static field or worksheet operation is executed.
"""
import hashlib,json,re
from pathlib import Path
ROOT=Path(__file__).resolve().parents[5];HERE=Path(__file__).resolve().parent
def need(v,m):
 if not v:raise ValueError(m)
def sha(b):return hashlib.sha256(b).hexdigest()
def stable(r):return {k:v for k,v in r.items() if k!='raw_IL_sha256'}
def inspect(root=ROOT,overrides=None):
 root=Path(root);base=HERE.relative_to(ROOT).as_posix();overrides=overrides or {}
 def raw(p):return overrides.get(p,(root/p).read_bytes())
 def load(p):return json.loads(raw(p))
 def text(p):return raw(p).decode('utf-8-sig').replace('\r\n','\n')
 current=load(base+'/current-universe-b1-v1.json');historical=load('evidence/04A38/completion/w2/continuation-v1/historical-ownership.json')['members']
 original=load('evidence/04A38/completion/w1/member-behavior-evidence-target-matrix-v3.json')['members'];byid={r['canonical_original_id']:r for r in original}
 sources={r['source']['path']:r['snapshot'] for r in load('evidence/04A38/preedit/entry-source-snapshot.json')['files']}
 metadata={}
 for config in ['Debug','Release']:
  pair=[]
  for role in ['original','current']:
   p=base+'/metadata-v4/b1-v1/'+role+'-'+config+'.json';x=load(p);need(x['metadata_only'] and x['original_semantic_calls']==0,'Non-passive source')
   need(set(x['scope'])==set(text(base+'/native-metadata-owners.txt').splitlines()) and len(x['scope'])==33,'Missing native module scope')
   pair.append((x,{r['method']:r for r in x['methods']},{r['type']:r for r in x['types']}))
  metadata[config]=pair
 def old_fragment(row):
  o=byid[row['canonical_original_id']];snap=sources[row['path']];b=raw(snap['path']);need(sha(b)==snap['sha256'],'Original source mismatch')
  # W1 UTF16 spans are original-source identities, not current positions.
  start=o['source_range']['span_start'] if 'source_range' in o else o.get('span_start')
  if start is None:
   tail=row['canonical_original_id'].rsplit('@',1)[1];start,length=map(int,tail.split(':'))
  else:length=o['source_range']['span_length'] if 'source_range' in o else o['span_length']
  return b.decode('utf-8-sig').encode('utf-16-le')[start*2:(start+length)*2].decode('utf-16-le')
 def current_match(row,fragment):
  source=raw(row['path']);matches=[r for r in current['members'] if r['path']==row['path'] and r['signature_key'] in {v.split('@')[0] for v in row['current_ids']}]
  need(len(matches)==1,'Ambiguous current identity '+row['canonical_original_id']);r=matches[0]
  need(sha(source)==r['source_sha256'],'Stale current source span')
  s=source.decode('utf-8-sig').encode('utf-16-le')[r['span_start']*2:(r['span_start']+r['span_length'])*2].decode('utf-16-le')
  need(s==fragment,'Host body/initializer source changed '+row['canonical_original_id']);return r
 def full_method(owner,name):
  proof=[]
  for config,[(oa,om,ot),(ca,cm,ct)] in metadata.items():
   methods=[r for r in om.values() if r['declaring_type']==owner and r['member_name']==name];need(len(methods)==1,'Ambiguous/missing original method '+owner+'.'+name);o=methods[0]
   need(o['method'] in cm and stable(o)==stable(cm[o['method']]),'Whole CIL/local/EH/field-context changed '+owner+'.'+name)
   need(ot[owner]==ct[owner],'Owning field/base context changed '+owner)
   ctors=[r for r in om.values() if r['declaring_type']==owner and r['member_name']=='.cctor']
   for ctor in ctors:need(ctor['method'] in cm and stable(ctor)==stable(cm[ctor['method']]),'Owning static initializer changed')
   proof.append(dict(configuration=config,method=o['method'],original_record_sha256=sha(json.dumps(stable(o),sort_keys=True).encode()),original_raw_IL_sha256=o['raw_IL_sha256'],current_raw_IL_sha256=cm[o['method']]['raw_IL_sha256'],full_resolved_instructions=True,locals_EH_filters_fields_generic_arity=True,type_initializer_count=len(ctors),assembly_sources=[r['sha256'] for r in oa['assemblies'] if r['name'].startswith('BESHStatNG,')]+[r['sha256'] for r in ca['assemblies'] if r['name'].startswith('BESHStatNG,')]))
  return proof
 rows=[];approved={'IsMissingArg','IsBlankCell','AsString','TryGetHandleKey','TryGetCachedHandle','PS_CLEANUP','TryGetPsmHandle','LoggedUdfError','LoggedUdfExceptionText'}
 multihandles={'TryGetKMeansHandle','TryGetHierarchicalHandle','TryGetPcaHandle','TryGetFaHandle','TryGetCaHandle','TryGetMcaHandle','TryGetDaHandle'}
 for row in historical:
  id=row['canonical_original_id'];owner=id.split('|')[1];o=byid.get(id)
  if not o:continue
  name=o['name']
  if name not in approved and name not in multihandles and not name.endswith('_DROP'):continue
  fragment=old_fragment(row);r=current_match(row,fragment);proof=full_method(owner,name)
  # Exact native dependency closure; no fitting/query/calculation may hide behind a name match.
  oldmethods=metadata['Debug'][0][1];method=next(v for v in oldmethods.values() if v['declaring_type']==owner and v['member_name']==name)
  dependencies=[]
  for i in method['instructions']:
   operand=i['operand']
   if i['op'] not in ['call','callvirt','newobj'] or not isinstance(operand,str):continue
   if ' @ BESHStatNG,' in operand and 'METHOD:BESHStatNG.' in operand:
    deps=[v for v in oldmethods.values() if 'METHOD:'+v['method']==operand]
    # Generic call construction has concrete type arguments; same named generic definition is proven separately.
    if not deps and '.TryGetCachedHandle``1(' in operand:deps=[v for v in oldmethods.values() if v['member_name']=='TryGetCachedHandle' and v['declaring_type']=='BESHStatNG.WorksheetFunctions.UdfCacheHelpers']
    need(len(deps)==1,'Unreviewed native dependency '+operand)
    d=deps[0];need(d['member_name'] in approved or d['member_name'] in multihandles,'Hidden non-cache dependency '+operand);full_method(d['declaring_type'],d['member_name']);dependencies.append(d['method'])
  if name.endswith('_DROP') or name=='PS_CLEANUP':reason='Native session handle eviction/count/clear; cache lookup/text acquisition and diagnostic error boundary; no estimator, portable import or report preparation'
  elif name.startswith('LoggedUdf'):reason='Host logger invocation, caught exception/fallback policy and public diagnostic propagation; accepted CoreServices logger interface remains host injected'
  elif name in ['IsMissingArg','IsBlankCell','AsString']:reason='Actual ExcelMissing/ExcelEmpty classification and raw-host Object string acquisition; includes original Convert.ToString/Trim throw timing, not portable statistical parsing'
  else:reason='Native model-session handle/key/cache lease lookup; no model computation or profile/report policy'
  rows.append(dict(original_id=id,current_id=r['symbol_id'],name=name,disposition='HOST_ONLY_WITH_BOUNDARY_PROOF',reason=reason,source_body_exact=True,source_sha256=r['source_sha256'],full_CIL=proof,native_dependencies=dependencies))
 initializers=[]
 for row in historical:
  if '|VariableDeclarator|' not in row['canonical_original_id']:continue
  f=old_fragment(row);r=current_match(row,f);owner=row['canonical_original_id'].split('|')[1];name=re.match(r'\s*(\w+)',f).group(1)
  record=[];fielduses=[]
  for config,[(oa,om,ot),(ca,cm,ct)] in metadata.items():
   a=next((v for v in ot[owner]['fields'] if v['name']==name),None);b=next((v for v in ct[owner]['fields'] if v['name']==name),None);need(a and a==b,'Initializer field changed '+name)
   if 'ConcurrentDictionary' in f:
    need(a['is_static'] and a['is_readonly'] and not a['is_literal'],'Cache storage contract changed')
    need(re.fullmatch(r'\w+ As New ConcurrentDictionary\(Of String, \w+\)\(StringComparer.OrdinalIgnoreCase\)',f.strip()),'Unreviewed cache initializer')
    proof=full_method(owner,'.cctor');ctor=next(v for v in om.values() if v['declaring_type']==owner and v['member_name']=='.cctor')
    stores=[i for i,v in enumerate(ctor['instructions']) if v['op']=='stsfld' and isinstance(v['operand'],str) and v['operand'].startswith('FIELD:'+owner+'.'+name+':')];need(len(stores)==1,'Cache field assignment missing/duplicate');i=stores[0]
    need(i>=2 and 'StringComparer' in str(ctor['instructions'][i-2]['operand']) and 'OrdinalIgnoreCase' in str(ctor['instructions'][i-2]['operand']) and ctor['instructions'][i-1]['op']=='newobj','Wrong cache constructor/comparer')
    mode='HOST_CACHE_INITIALIZER_SOURCE_AND_FULL_STATIC_CONTEXT'
   else:
    need(owner.endswith('SampleSizeUDFs') and a['is_literal'] and a['literal'] in ['superiority','noninferiority','equivalence'],'Unreviewed constant declaration');need('"'+a['literal']+'"' in f,'Constant value mismatch');proof=[];mode='LITERAL_CONSTANT_NO_EXECUTABLE_INITIALIZER'
   uses=[v['method'] for v in cm.values() if any(isinstance(i['operand'],str) and i['operand'].startswith('FIELD:'+owner+'.'+name+':') for i in v['instructions'])];fielduses.append(dict(configuration=config,methods=uses))
   record.append(dict(configuration=config,field=a,static_initializer=proof))
  initializers.append(dict(original_id=row['canonical_original_id'],current_id=r['symbol_id'],declaration=f,disposition=mode,metadata=record,field_uses=fielduses,contained_fitting_query_bodies='REMAIN_INDEPENDENTLY_OPEN'))
 need(len(initializers)==21,'Missing initializer/constant identity')
 return dict(status='BOUNDED_HOST_CACHE_SOURCE_FULL_CIL_PASS_NOT_READY',metadata_only=True,original_queries=0,methods=rows,initializers=initializers,whole_batch='NOT_READY_04A38',scope='Only source-enumerated cache/host-reader/logging bodies and21initializers; no inference that cache consumers/model fits are closed')

if __name__=='__main__':
 try:
  r=inspect();print(json.dumps(dict(status=r['status'],methods=len(r['methods']),initializers=len(r['initializers']))))
 except Exception as e:print(json.dumps(dict(status='NOT_READY_04A38',error=type(e).__name__+': '+str(e))));raise SystemExit(1)
