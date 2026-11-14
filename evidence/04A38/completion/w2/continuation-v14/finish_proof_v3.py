from common import *
steps=[('nonpass-source-deltas-v3', 'postprocess_v3.py'), ('shipping-binding-v1', 'bind_current_validation_v1.py'), ('fixture-crosswalk-v1', 'fixture_crosswalk_v1.py'), ('independent-validation-v1', 'final_validation_v1.py'), ('final-source-binding-v1', 'bind_final_validation_v1.py'), ('publication-authority-v1', 'prepare_publication_authority_v1.py')]
for label,name in steps:
 r=command(label,[sys.executable,'-B',str(HERE/name)]);print(label,r['exit_code'],flush=True);assert r['exit_code']==0,'Preserve failed attempt '+label
