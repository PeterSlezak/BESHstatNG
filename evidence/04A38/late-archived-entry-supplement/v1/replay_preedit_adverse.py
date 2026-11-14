"""Replay unchanged28 tests with only a fresh private fixture parent.

The original suite intentionally uses exist_ok=False at a fixed v1 path. Keep
the old view and tests intact; no assertion, reader or source input is changed.
"""
import hashlib,sys,unittest,uuid
from pathlib import Path
ROOT=Path(__file__).resolve().parents[4]
sys.path.insert(0,str(ROOT/'evidence/04A38'))
import test_preedit_reader as tests

if __name__=='__main__':
 source=Path(tests.__file__);before=hashlib.sha256(source.read_bytes()).hexdigest()
 tests.PRIVATE=ROOT/'.test-rationalization-04A38.tmp'/('replay-preedit-'+uuid.uuid4().hex)
 tests.PRIVATE.mkdir(exist_ok=False)
 result=unittest.TextTestRunner(verbosity=2).run(unittest.defaultTestLoader.loadTestsFromModule(tests))
 assert hashlib.sha256(source.read_bytes()).hexdigest()==before
 raise SystemExit(not result.wasSuccessful())
