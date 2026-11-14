"""Execute unchanged original reader controls in a fresh private workspace."""
from common import *
if len(sys.argv)==1:
 r=command('retained-preedit-controls-v1',[sys.executable,'-B',str(HERE/'run_preedit_controls_v1.py'),'worker'])
 save('retained-preedit-controls-v1.json',dict(command=clean(r),scope='Unchanged test_preedit_reader assertions; only its private workspace root redirected to a fresh C6 directory',original_queries=0))
 assert r['exit_code']==0
else:
 import unittest,importlib.util
 spec=importlib.util.spec_from_file_location('retained_controls',ROOT/'evidence/04A38/test_preedit_reader.py')
 module=importlib.util.module_from_spec(spec);spec.loader.exec_module(module)
 module.PRIVATE=PRIVATE/'retained-controls-v1'
 result=unittest.TextTestRunner(verbosity=2).run(unittest.defaultTestLoader.loadTestsFromModule(module))
 raise SystemExit(not result.wasSuccessful())
