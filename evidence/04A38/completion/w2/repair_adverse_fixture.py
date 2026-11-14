"""Preserve invalid first adverse fixture and include all protected dependencies."""
from capture_current import *
s=(HERE/'test_independent_slice.py').read_text(encoding='utf-8')
s=s.replace("  cls.hashes={}","  protection=profile.load(profile.ROOT/'evidence/04A38/preservation-validation-v2.json')\n  paths.update(r['current']['path'] for r in protection['lineage_pins'])\n  paths.update(r['path'] for r in protection['protected_mixed_model_captures'])\n  cls.hashes={}")
s=s.replace(' @classmethod\n def tearDownClass',"  profile.inspect(cls.view);bodies.inspect(cls.view);protected.inspect(cls.view,False)\n @classmethod\n def tearDownClass")
with (HERE/'test_independent_slice_v2.py').open('x',encoding='utf-8') as f:f.write(s)
with (HERE/'adverse-fixture-attempt-v1.json').open('x',encoding='utf-8') as f:json.dump(dict(status='INVALID_FIXTURE_NOT_ACCEPTED',executed=17,nominal_pass=15,errors=2,reason='Fixture omitted protected lineage payloads not present in predecessor compact publication manifest. Missing-file errors could falsely reject mutated data.',repair='Add actual protected lineage/capture payloads and require every intact reader to pass before any mutations',expectations_weakened=False,original_data_changes=0),f,indent=2)
r=m.Runner(RUNROOT).command('w2-independent-slice-adverse-v2',[sys.executable,'-B','-m','unittest','discover','-s',str(HERE),'-p','test_independent_slice_v2.py','-v']);assert r['exit_code']==0
