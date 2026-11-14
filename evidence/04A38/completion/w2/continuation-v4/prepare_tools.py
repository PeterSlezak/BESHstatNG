from common import *
for name in ['build.py','focused.py','inspect_stage.py','MethodMetadata-v5.cs','native-metadata-owners-v5.txt','run_metadata_v5.py','compatibility.py','selected_release.py','run_full_validation.py','report_managed.py','report_validation.py']:
 s=(PREVIOUS/name).read_text(encoding='utf-8').replace('w2c3-validation-v1','w2c4-validation-v1').replace('names-v3','parametric-v1')
 if name=='report_validation.py':s=s.replace('w2c2-validation-v1','w2c3-validation-v1').replace('from_C2','from_C3')
 with (HERE/name).open('x',encoding='utf-8',newline='\n') as f:f.write(s)
# Original methods and original Core catch shims are observed only as metadata.
save('attempts/setup-quoting.json',dict(status='FAILED_ORCHESTRATION_RETAINED',steps=['PowerShell-escaped inline setup command SyntaxError before any write','preflight import common ModuleNotFoundError'],correction='setup.py supplies same narrow C4 common configuration; no production edit or original invocation occurred'))
