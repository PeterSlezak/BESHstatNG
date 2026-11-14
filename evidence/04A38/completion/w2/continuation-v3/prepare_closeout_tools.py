from common import *
# Adapt only orchestration to this checkpoint; predecessors stay byte-for-byte.
for name,replacements in {
 'record_tracked_paths.py': [('AUTHORIZED_V2_SOURCE_TRANSITION','AUTHORIZED_V3_SOURCE_TRANSITION'),('EXACT_C1_CHECKPOINT','EXACT_C2_CHECKPOINT')],
 'verify_binary_lineage.py': [('metadata-v4/b1-v1','metadata-v5/names-v3'),("PRIVATE/'b1-v1'","PRIVATE/'names-v3'"),('build-b1-v1-','build-names-v3-')],
 'report_validation.py': [('w2c2-validation-v1','w2c3-validation-v1'),('w2c1-validation-v2','w2c2-validation-v1'),('python_occurrence_delta_from_C1','python_occurrence_delta_from_C2'),("historical_C1_final=dict(passed=766,failed=180,errors=10)","historical_C2_final=dict(passed=765,failed=180,errors=11)"),('historical_C1_first','historical_C1_first')]
}.items():
 s=(PREVIOUS/name).read_text(encoding='utf-8')
 for a,b in replacements:s=s.replace(a,b)
 with (HERE/name).open('x',encoding='utf-8',newline='\n') as f:f.write(s)
# Use the established diagnostic-only label policy, with an independently
# reversible field/pointer ledger. No reference/source/result byte rewriting.
sys.path.insert(0,str(ROOT/'evidence/04A38'))
from validation_report import safe_label
source=HERE/'full-managed-Debug-v1.json';original=json.loads(source.read_bytes());public=json.loads(source.read_bytes());changes=[]
for s,suite in enumerate(public['results']):
 for i,r in enumerate(suite['records']):
  old=r['test_name'];new=safe_label(old)
  if old!=new:
   changes.append(dict(pointer=['results',s,'records',i,'test_name'],original_label_sha256=sha(old.encode()),public_label_sha256=sha(new.encode()),public_label=new))
   r['test_name']=new
save('full-managed-Debug-public-v1.json',public)
save('diagnostic-public-derivative.json',dict(raw_local=pin(source),public=pin(HERE/'full-managed-Debug-public-v1.json'),changes=changes,scope='Only test_name diagnostic display labels through previously accepted safe_label; raw identity/occurrence/outcome and all source/input/result/trace bytes unchanged',excluded_raw_retained_locally=True))
