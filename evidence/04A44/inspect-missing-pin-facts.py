from common44 import *
code=(HERE/'python-dispositions-v4.py').read_text();ns={};exec(code[:code.index('for row in d')],ns)
for phase,name in [('13','survival_workflow'),('14','mixed_workflow')]:
 t=module('pin_probe_'+phase,ROOT/('scripts/tests/test_04a'+phase+'_'+name+'.py'));pins=load(t.c.RUN/'baseline.json')['source_pins'];missing=sorted(set(pins)-set(ns['old43pins']));print(phase,'count',len(missing),'first',missing[:6])
 save(PRIVATE/('missing-baseline-pin-facts-'+phase+'.json'),dict(paths=missing,live=[pin(p) for p in missing]))
