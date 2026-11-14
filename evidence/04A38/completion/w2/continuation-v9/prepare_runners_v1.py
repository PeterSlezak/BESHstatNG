"""Use isolated existing offline current-only runners, with C9 labels/selections."""
from common_v2 import *
for name in ['build_v1.py','focused_v3.py','inspect_stage_v1.py','inspect_metadata_v3.py','compatibility_v1.py','validate_managed_v1.py','run_auxiliary_v1.py']:
    source=(PREVIOUS/name).read_text(encoding='utf-8').replace('from common import *','from common_v2 import *').replace('w2c8','w2c9').replace('cont8','cont9').replace('continuation-v8','continuation-v9')
    if name=='focused_v3.py':
        source=source.replace('FullyQualifiedName~SampleSize','FullyQualifiedName~Agreement|FullyQualifiedName~BinaryClassification|FullyQualifiedName~Contingency',1)
        source=source.replace('FullyQualifiedName~UdfContinuation8Tests|FullyQualifiedName~SampleSize','FullyQualifiedName~UdfContinuation9Tests')
        source=source.replace('FullyQualifiedName~UdfPreparation38AdapterTests','FullyQualifiedName~Continuation9')
        source=source.replace("if tested['exit_code']!=0:break","assert sum(result['tests']['outcomes'].values())>0,'Zero-test focused selector'\n  if tested['exit_code']!=0:break")
    with (HERE/name).open('x',encoding='utf-8',newline='\n') as f:f.write(source)
save('runner-crosswalk-v1.json',dict(files=[dict(before=pin(PREVIOUS/n),after=pin(HERE/n)) for n in ['build_v1.py','focused_v3.py','inspect_stage_v1.py','inspect_metadata_v3.py','compatibility_v1.py','validate_managed_v1.py','run_auxiliary_v1.py']],unit_policy='Debug focused +one final comprehensive six-suite campaign. Full/selected Release unit NOT_RUN_OWNER_PREFERENCE; compile/ABI/frozen comparisons both configurations remain mandatory.',original_queries=0))
