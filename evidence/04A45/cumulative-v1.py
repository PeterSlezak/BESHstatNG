from common45 import *
import shutil
lock();config=sys.argv[1];lib=PRIVATE/'compiled'/config/'lib';sys.path.insert(0,str(ROOT/'evidence/04A43'));compare=module('actual43_reference',ROOT/'evidence/04A43/compare43_v3.py')
def execute(label,exe,extra,original):
 target=PRIVATE/('fixture-final-v2-'+config+'-'+label);target.mkdir()
 for p in lib.glob('*.dll'):shutil.copy2(p,target/p.name)
 shutil.copy2(exe,target/exe.name);r=command('final-v2-'+config+'-'+label,[str(target/exe.name),str(target/'observations.json'),*extra]);assert r['exit_code']==0
 return load(original),load(target/'observations.json'),pin(target/'observations.json')
scope=str(ROOT/'evidence/04A43/scope/cache-export-inventory-v1.json');before=ROOT/'.test-rationalization-04A43.tmp'
a,b,p=execute('43',before/'fixtures-before-v3/Observe43V3.exe',[scope],before/'fixtures-before-v3/observations.json');assert compare.canonical(a)==compare.canonical(b)
save('fixtures/'+config+'-43-v2.json',dict(kind='POST_EDIT44_PRESERVED43_RELATIONAL_CONTRACTS',operations=1118,graphs=36,families=18,baseline=pin(before/'fixtures-before-v3/observations.json'),current=p,generated_reference_bijection_validated=True,nonvolatile_typed_outputs_errors_shapes_exact=True,original_qualifications=pin(ROOT/'evidence/04A43/fixtures/volatile-fields-v1.json'),gates=GATES))
a,b,p=execute('43-races',before/'races-before-v1/Race43.exe',[scope],before/'races-before-v1/observations.json');left=compare.validate_races(a);right=compare.validate_races(b);assert left['identities']==right['identities']
save('fixtures/'+config+'-43-races-v2.json',dict(before=left,after=right,current=p,identities_exact=True,allowed_outcome_invariants_pass=True,fixed_schedule_equality_claimed=False,live_Excel='NOT_RUN'))
a,b,p=execute('43-policy',before/('policy-simulation-'+config+'-v1/Policy43.exe'),[],before/('policy-simulation-'+config+'-v1/observations.json'));assert a==b and len(a['rows'])==15
save('fixtures/'+config+'-43-policy-v2.json',dict(cases=15,exact=True,current=p,scope='DEFINED_POLICY_ONLY;NO_NATIVE_WORKBOOK_IDENTITY'))
a,b,p=execute('44',ROOT/'.test-rationalization-04A44.tmp/fixtures-before-v1/Observe44.exe',[],ROOT/'.test-rationalization-04A44.tmp/fixtures-before-v1/observations.json');assert a==b and len(a['rows'])==591
save('fixtures/'+config+'-44-v2.json',dict(kind='POST_EDIT_CURRENT44;DRIVER_SCHEMA_LABEL_IS_NOT_CAPTURE_TIME',cases=591,exact=True,baseline=pin(ROOT/'.test-rationalization-04A44.tmp/fixtures-before-v1/observations.json'),current=p,excluded_runtime_fields=[],scientific='NOT_GOLDEN'))
print(config,'1118/36graphs +864races +15policy43 retained;591 actual44 cases exact',flush=True)
