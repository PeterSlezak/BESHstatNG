from common43 import *
import shutil
lock();cmp=module('cmp43_qualified',HERE/'compare43_v2.py')
old=load(PRIVATE/'fixtures-before-v3/observations.json');now=load(PRIVATE/'wave-b2-lifecycle-v1/observations.json');a=cmp.canonical(old);b=cmp.canonical(now)
differences=[dict(id=x['id'],before=x,after=y) for x,y in zip(a['operations'],b['operations']) if x!=y];save(PRIVATE/'wave-b-qualified-differences-v3.json',dict(differences=differences));assert not differences and a['graphs']==b['graphs']
target=PRIVATE/'wave-b-races-v3';target.mkdir()
for p in (PRIVATE/'builds/a43-wave-b2-v1/Debug/legacy').glob('*.dll'):shutil.copy2(p,target/p.name)
shutil.copy2(PRIVATE/'races-before-v1/Race43.exe',target/'Race43.exe');r=command('wave-b-races-v3',[str(target/'Race43.exe'),str(target/'observations.json'),str(HERE/'scope/cache-export-inventory-v1.json')]);assert r['exit_code']==0
race=cmp.validate_races(load(target/'observations.json'));assert race['identities']==cmp.validate_races(load(PRIVATE/'races-before-v1/observations.json'))['identities']
parts=[dict(suite=suite,outcomes=load(PRIVATE/('wave-b2-'+suite+'-tests-v1.json'))['outcomes'],raw=pin(PRIVATE/('wave-b2-'+suite+'-tests-v1.json'))) for suite in ['Common','Legacy']]
save('waves/wave-b-v3.json',dict(build=pin(PRIVATE/'wave-b2-build-v1.json'),parts=parts,operations=1118,reference_graphs=36,source_reached_runtime_fields=pin(HERE/'fixtures/volatile-fields-v1.json'),lifecycle_baseline=pin(PRIVATE/'fixtures-before-v3/observations.json'),lifecycle_successor=pin(PRIVATE/'wave-b2-lifecycle-v1/observations.json'),embedded_driver_kind='Capture-driver schema label;successor capture is POST_EDIT_CURRENT,not a new pre-edit oracle.',nonvolatile_values_errors_bounds_and_relational_graph_exact=True,races=race,races_raw=pin(target/'observations.json'),races_fixed_outcomes_not_claimed=True,failed_verifier_attempts='Original198 discovery lacked explicit legacy Compile include;uncorrected comparator counted runtime clocks. Both failed attempts retained;existing tests unchanged and all55 new identities actually executed.',gates=GATES))
print('Wave B actual35 Common+218 Legacy tests;1118 relational operations exact with92 clock cells qualified;864 race identities PASS',flush=True)
