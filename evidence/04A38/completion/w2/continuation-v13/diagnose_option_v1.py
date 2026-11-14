from common import *
from source_views import body
from verify_c13_composition_v2 import phase,unwrap_cases
cs=json.loads((HERE/'whole-body-contracts-v1.json').read_bytes())['contracts'];full=json.loads((HERE/'dependency-full-common-v1.json').read_bytes())
for name in ['GetHistogramBinRule','ParseRocDirection']:
 c=next(c for c in cs if c['current_record']['name']==name)
 p=next(p for p in full['members'] if p['owner']=='BESHStatNG.ExcelCommon.UdfDataImportOptions38' and p['name']==name)
 print(name,'ORIGINAL',phase(c['original_whole_body']),'PORTABLE',unwrap_cases(phase(body((ROOT/p['path']).read_bytes(),p)[0])))
