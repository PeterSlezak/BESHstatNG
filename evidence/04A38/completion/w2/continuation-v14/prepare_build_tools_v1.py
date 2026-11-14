from common import *
sources=['build_v1.py','focused_v1.py','inspect_stage_v1.py','inspect_metadata_v5.py','MethodMetadata-v11.cs','native-metadata-owners-v3.txt','ReturnMetadata-v1.cs','DeclarationShapes-v1.cs','source_views.py','proof_kernel_v4.py','proof_kernel_v5.py','proof_kernel_v6.py','compatibility_v1.py','validate_managed_v1.py']
rows=[]
for name in sources:
 s=PREVIOUS/name;v=s.read_text(encoding='utf-8-sig')
 if name=='focused_v1.py':v=v.replace('Continuation13','Continuation14')
 if name=='validate_managed_v1.py':v=v.replace('w2c13','w2c14')
 with (HERE/name).open('x',encoding='utf-8',newline='\n') as f:f.write(v)
 rows.append(dict(before=pin(s),after=pin(HERE/name)))
save('build-validation-source-crosswalk-v1.json',dict(rows=rows,original_queries=0))
