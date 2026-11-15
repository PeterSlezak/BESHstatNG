from common44 import *
base={r['path']:r['sha256'] for r in load(ROOT/'.test-rationalization-04A43.tmp/tracked-entry-inventory-v1.json')['files']};freeze=load(ROOT/'evidence/04A43/matrix/source-freeze-v1.json');base.update(freeze['source_pins']);base[freeze['plan']['path']]=freeze['plan']['sha256'];base.update({r['path']:r['sha256'] for r in load(ROOT/'.test-rationalization-04A43.tmp/python-input-freeze-v1.json')['files']})
for phase,name in [('13','survival_workflow'),('14','mixed_workflow')]:
 test=module('test'+phase,ROOT/('scripts/tests/test_04a'+phase+'_'+name+'.py'));c=test.c;pins=load(c.RUN/'baseline.json')['source_pins'];print(phase,'missing',sorted(set(pins)-set(base)))
