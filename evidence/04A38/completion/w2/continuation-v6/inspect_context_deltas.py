from common import *
def key(r):return (r['declaring_type'],r['member_name'],tuple(r['parameter_types']))
def sem(r):return {k:v for k,v in r.items() if k!='raw_IL_sha256'}
for config in ['Debug','Release']:
 old=json.loads((PREVIOUS/('dependency-metadata-v9/current-'+config+'.json')).read_bytes())
 new=json.loads((HERE/('metadata-full/families-v2/current-'+config+'.json')).read_bytes())
 a={key(r):r for r in old['methods']};b={key(r):r for r in new['methods']}
 changed=[k for k in a if k in b and sem(a[k])!=sem(b[k])]
 missing=[k for k in a if k not in b]
 print(config,'CHANGED',json.dumps(changed),'MISSING',json.dumps(missing))
 print('Numeric closure types',[t['type'] for t in new['types'] if t['type'].startswith('BESHStatNG.ExcelCommon.UdfNumericImportPreparation38+')])
 original=json.loads((HERE/('metadata-full/families-v2/original-'+config+'.json')).read_bytes())
 engines=lambda r:r['declaring_type'].startswith(('BESHStatNG.parametric.','BESHStatNG.nonparametric.','BESHStatNG.equivalencetests.','BESHStatNG.Matrix.')) or r['declaring_type']=='BESHStatNG.ExcelDnaSharedApplicationAdapter'
 ao={key(r):r for r in original['methods'] if engines(r)};bn={key(r):r for r in new['methods'] if engines(r)}
 assert ao.keys()==bn.keys()
 differences=[k for k in ao if sem(ao[k])!=sem(bn[k])]
 print('ENGINE/ADAPTER',len(ao),'differences',json.dumps(differences))
 assert not differences
