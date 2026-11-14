from common import *
rows=[]
for config in ['Debug','Release']:
 original=json.loads((HERE/('metadata-full/families-v2/original-'+config+'.json')).read_bytes())
 current=json.loads((HERE/('metadata-full/families-v2/current-'+config+'.json')).read_bytes())
 engines=lambda r:r['type'].startswith(('BESHStatNG.parametric.','BESHStatNG.nonparametric.','BESHStatNG.equivalencetests.','BESHStatNG.Matrix.')) or r['type']=='BESHStatNG.ExcelDnaSharedApplicationAdapter'
 common=lambda r:r['type'].startswith('BESHStatNG.ExcelCommon.') and '38' not in r['type']
 counts={}
 for name,test in [('engine_adapter_types',engines),('existing_typed_Common_types',common)]:
  a={r['type']:r for r in original['types'] if test(r)};b={r['type']:r for r in current['types'] if test(r)}
  assert a==b,'Complete type/field/readonly/static/literal/interface context changed '+name
  counts[name]=len(a)
 rows.append(dict(configuration=config,**counts))
save('full-type-field-context.json',dict(status='ENTIRE_ENGINE_TRANSPORT_TYPE_FIELD_CONTEXT_EXACT',rows=rows,original_queries=0))
print(json.dumps(rows))
