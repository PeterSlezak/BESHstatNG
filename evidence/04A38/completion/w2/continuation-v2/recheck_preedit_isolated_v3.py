"""Actual unchanged28test suite, fresh private outputs; no retained file is replaced."""
import importlib.util,unittest
from common import *
view=RUNROOT/'w2-before-v2';folder=view/'evidence/04A38';sys.path.insert(0,str(folder))
spec=importlib.util.spec_from_file_location('preedit38_unchanged_test',folder/'test_preedit_reader.py');module=importlib.util.module_from_spec(spec);spec.loader.exec_module(module)
target=RUNROOT/'c2-preedit-reader-isolation-v3';target.mkdir(exist_ok=False);module.PRIVATE=target
result=unittest.TextTestRunner(verbosity=2).run(unittest.defaultTestLoader.loadTestsFromTestCase(module.PreeditReaderAdverseTests))
if not result.wasSuccessful():raise SystemExit(1)
