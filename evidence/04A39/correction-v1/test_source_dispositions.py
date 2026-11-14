from common import *
from publication_gate_v2 import scan,decide
import io,tempfile,unittest

class SourceDispositionControls(unittest.TestCase):
    def setUp(self):
        self.work=Path(tempfile.mkdtemp(prefix='source-disposition-',dir=PRIVATE))
        self.path='BESHStat.Excel.Common.Tests/src/CompositeOutputTests.vb'
        self.raw=(ROOT/self.path).read_bytes();target=self.work/self.path;target.parent.mkdir(parents=True);target.write_bytes(self.raw)
        self.files=[pin(self.path)];self.allowed=[r for r in load(PRIVATE/'source-literal-dispositions.json')['allowed'] if r['key'][0]==self.path]
    def test_exact_synthetic_source_occurrences_are_documented(self):
        f,_,d,_=scan(self.work,[self.path],self.files)
        self.assertGreater(len(f),0);self.assertEqual(0,decide(f,d,self.files,self.allowed)['unresolved'])
    def test_appended_private_diagnostic_invalidates_exact_source_disposition(self):
        (self.work/self.path).write_bytes(self.raw+b'\n'+str(ROOT/'raw.log').encode())
        row=dict(path=self.path,sha256=digest(self.work/self.path),bytes=(self.work/self.path).stat().st_size)
        f,_,d,_=scan(self.work,[self.path],[row])
        with self.assertRaises(AssertionError):decide(f,d,[row],self.allowed)
    def test_missing_synthetic_disposition_is_not_blanket_allowed(self):
        f,_,d,_=scan(self.work,[self.path],self.files)
        with self.assertRaisesRegex(AssertionError,'UNRESOLVED'):decide(f,d,self.files,[])
    def test_mutated_occurrence_key_cannot_reuse_previous_scan(self):
        import copy
        f,_,d,_=scan(self.work,[self.path],self.files);bad=copy.deepcopy(self.allowed);bad[0]['key'][1]='0'*64
        with self.assertRaises(AssertionError):decide(f,d,self.files,bad)
if __name__=='__main__':
    out=io.StringIO();r=unittest.TextTestRunner(stream=out,verbosity=2).run(unittest.defaultTestLoader.loadTestsFromTestCase(SourceDispositionControls))
    save(PRIVATE/'source-disposition-controls-raw.json',dict(stdout=out.getvalue(),testsRun=r.testsRun,failures=len(r.failures),errors=len(r.errors)))
    save('validation/source-disposition-controls.json',dict(testsRun=r.testsRun,failures=len(r.failures),errors=len(r.errors),exit_code=0 if r.wasSuccessful() else 1,identities=unittest.defaultTestLoader.getTestCaseNames(SourceDispositionControls),raw=pin(PRIVATE/'source-disposition-controls-raw.json')))
    print('Source disposition controls',r.testsRun,'failures',len(r.failures),'errors',len(r.errors))
    raise SystemExit(0 if r.wasSuccessful() else 1)
