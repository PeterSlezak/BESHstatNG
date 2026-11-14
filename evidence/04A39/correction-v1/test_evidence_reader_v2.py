from common import *
from verify_correction_v2 import verify_documents
import copy, io, unittest

docs=[load(HERE/p) for p in ['diagnostics/parent-956-reconciliation.json','diagnostics/changed-54-crosswalk-v2.json','diagnostics/focused-reconciliation.json','privacy/focused-finding-reconciliation.json','validation/reporting-boundary-tests.json']]
class EvidenceReaderControls(unittest.TestCase):
    def test_actual_evidence_retains_blocking_verdict(self):self.assertEqual('BLOCKED_04A39_OWNER_DECISION_REQUIRED',verify_documents(*docs)['status'])
    def test_missing_parent_rejected(self):
        d=copy.deepcopy(docs);d[0]['parents'].pop()
        with self.assertRaises(AssertionError):verify_documents(*d)
    def test_duplicate_parent_rejected(self):
        d=copy.deepcopy(docs);d[0]['parents'][-1]=d[0]['parents'][0]
        with self.assertRaises(AssertionError):verify_documents(*d)
    def test_changed_nonpass_rejected(self):
        d=copy.deepcopy(docs);d[0]['parents'][0]['original39_outcome']='ERROR'
        with self.assertRaises(AssertionError):verify_documents(*d)
    def test_missing_diagnostic_rejected(self):
        d=copy.deepcopy(docs);d[1]['changes'].pop()
        with self.assertRaises(AssertionError):verify_documents(*d)
    def test_substantive_trace_drift_rejected(self):
        d=copy.deepcopy(docs);d[1]['changes'][0]['trace_difference_only_assertion_message']=False
        with self.assertRaises(AssertionError):verify_documents(*d)
    def test_privacy_count_erasure_rejected(self):
        d=copy.deepcopy(docs);d[3]['per_parent_findings']=0
        with self.assertRaises(AssertionError):verify_documents(*d)
    def test_failed_controls_rejected(self):
        d=copy.deepcopy(docs);d[4]['failures']=1
        with self.assertRaises(AssertionError):verify_documents(*d)
if __name__=='__main__':
    stream=io.StringIO();r=unittest.TextTestRunner(stream=stream,verbosity=2).run(unittest.defaultTestLoader.loadTestsFromTestCase(EvidenceReaderControls))
    save(PRIVATE/'evidence-reader-controls-v2-raw.json',dict(stdout=stream.getvalue(),testsRun=r.testsRun,failures=len(r.failures),errors=len(r.errors)))
    save('validation/evidence-reader-controls-v2.json',dict(testsRun=r.testsRun,failures=len(r.failures),errors=len(r.errors),exit_code=0 if r.wasSuccessful() else 1,identities=unittest.defaultTestLoader.getTestCaseNames(EvidenceReaderControls),raw=pin(PRIVATE/'evidence-reader-controls-v2-raw.json')))
    print('Evidence reader controls',r.testsRun,'failures',len(r.failures),'errors',len(r.errors))
    raise SystemExit(0 if r.wasSuccessful() else 1)
