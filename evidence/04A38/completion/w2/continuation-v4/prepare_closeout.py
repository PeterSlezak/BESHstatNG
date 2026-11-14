from common import *
from validation_report import safe_label
source=HERE/'full-managed-Debug-v1.json';x=json.loads(source.read_bytes());changes=[]
for a,suite in enumerate(x['results']):
 for i,r in enumerate(suite['records']):
  old=r['test_name'];new=safe_label(old)
  if old!=new:changes.append(dict(pointer=['results',a,'records',i,'test_name'],original_label_sha256=sha(old.encode()),public_label_sha256=sha(new.encode()),public_label=new));r['test_name']=new
save('full-managed-Debug-public-v1.json',x);save('diagnostic-public-derivative.json',dict(raw_local=pin(source),public=pin(HERE/'full-managed-Debug-public-v1.json'),changes=changes,scope='Only declared diagnostic display labels; no source/input/output/branch/identity/outcome changes',excluded_raw_retained_locally=True))
for name in ['verify_derivative.py','record_environment.py','verify_binary_lineage.py']:
 s=(PREVIOUS/name).read_text(encoding='utf-8').replace('metadata-v5/names-v3','metadata-v6/parametric-v4').replace("PRIVATE/'names-v3'","PRIVATE/'parametric-v4'").replace('build-names-v3','build-parametric-v4').replace('names-v3','parametric-v4')
 # Native edge scope expands to actual inversion and report services.
 if name=='record_environment.py':
  s=s.replace("'BESHStatNG.UdfOutputTables'","'BESHStatNG.UdfOutputTables','BESHStatNG.UdfLinearAlgebra'")
  s=s.replace("['UdfNamePreparation38','UdfVariableNameRange38','UdfSpillPreparation38']","['UdfNamePreparation38','UdfVariableNameRange38','UdfSpillPreparation38','ParametricUdfReports38','UdfNumericPreparation38']")
 with (HERE/name).open('x',encoding='utf-8',newline='\n') as f:f.write(s)
s=(PREVIOUS/'verify_publication.py').read_text(encoding='utf-8').replace('from verify_ownership import','from verify_ownership_v2 import').replace('from verify_scope_addendum_v2 import','from verify_fragment_scope import').replace('VERIFIED_PARTIAL_C3_NOT_READY','VERIFIED_PARTIAL_C4_NOT_READY').replace("{'Passed':20666,'Failed':14}","{'Passed':20674,'Failed':14}")
with (HERE/'verify_publication.py').open('x',encoding='utf-8',newline='\n') as f:f.write(s)
s=(PREVIOUS/'test_publication.py').read_text(encoding='utf-8').replace('VERIFIED_PARTIAL_C3_NOT_READY','VERIFIED_PARTIAL_C4_NOT_READY')
with (HERE/'test_publication.py').open('x',encoding='utf-8',newline='\n') as f:f.write(s)
s=(PREVIOUS/'publish.py').read_text(encoding='utf-8').replace("PREVIOUS/'publication-manifest-v3.json'","PREVIOUS/'publication-manifest-v2.json'").replace('source-transitions-final.json','source-transitions.json').replace('C3','C4')
s=s.replace("paths.update(['BESHStat.Excel.Common.Tests", "paths.update(r['current']['path'] for r in json.loads((HERE/'accepted-Core-dependency-pins.json').read_bytes())['files'])\npaths.update(['BESHStat.Excel.Common.Tests")
with (HERE/'publish.py').open('x',encoding='utf-8',newline='\n') as f:f.write(s)
