"""Focused actual-scan adverse controls; synthetic raw values stay in private test fixtures."""
from common import *
import collections, copy, io, tempfile, unittest, zlib
from diagnostic_producer import emit, sanitized
from publication_gate_v2 import safe_name, physical_inventory, scan, decide

class ReportingBoundaryTests(unittest.TestCase):
    def setUp(self):
        self.work=Path(tempfile.mkdtemp(prefix='boundary-',dir=PRIVATE));self.stage=self.work/'stage';self.stage.mkdir()
    def row(self,name,data):
        p=self.stage/name;p.parent.mkdir(parents=True,exist_ok=True);p.write_bytes(data)
        return dict(path=name,bytes=len(data),sha256=sha(data),crc32=f'{zlib.crc32(data):08x}')
    def test_projection_preserves_status_identity_count_and_raw_bytes(self):
        raw=dict(status='FAIL',count=1599,path=str(ROOT/'example.log'),events=[dict(identity='event-1',outcome='ERROR')])
        out,source,dest=emit('diagnostic-example',raw,self.work/'raw',self.work/'public')
        self.assertEqual(raw,load(source)['data']);self.assertEqual('FAIL',out['original_status']);self.assertEqual(1599,out['data']['count'])
        self.assertEqual(raw['events'],out['data']['events']);self.assertEqual(digest(source),out['raw_sha256']);self.assertTrue(dest.is_file())
    def test_sanitization_deterministic(self):
        value=dict(path=str(ROOT/'example.log'),message='ordinary assertion',outcome='FAIL')
        self.assertEqual(sanitized(value),sanitized(value));self.assertEqual('FAIL',sanitized(value)['outcome'])
    def test_raw_location_outside_private_is_rejected(self):
        with self.assertRaisesRegex(AssertionError,'RAW_LOCATION'):emit('x',{},HERE/'forbidden-raw',self.work/'public')
    def test_redacted_key_collision_is_rejected(self):
        from diagnostic_producer import redactor
        token=next(iter(redactor.TOKENS))
        with self.assertRaises(AssertionError):sanitized({token:1,'<PRIVATE_IDENTIFIER>':2})
    def test_encoded_sensitive_payload_is_rejected_and_raw_retained(self):
        import base64
        raw=dict(status='FAIL',payload=base64.b64encode((str(ROOT/'secret-diagnostic.log')*5).encode()).decode())
        with self.assertRaisesRegex(AssertionError,'SANITIZATION_INCOMPLETE'):emit('encoded',raw,self.work/'raw',self.work/'public')
        self.assertEqual(1,len(list((self.work/'raw').glob('*.json'))));self.assertEqual([],list((self.work/'public').iterdir()))
    def test_traversal_absolute_drive_unc_and_case_sensitive_guards(self):
        for name in ['../escape','/absolute',chr(67)+':'+chr(92)+'drive',chr(92)*2+'host'+chr(92)+'share','x/../y','x/NUL.txt','x/trailing.']:
            with self.subTest(name_hash=sha(name.encode())):
                with self.assertRaises(AssertionError):safe_name(name)
    def test_missing_extra_and_changed_payload_are_rejected(self):
        row=self.row('safe.txt',b'ordinary')
        self.assertEqual(['safe.txt'],physical_inventory(self.stage,[row],[]))
        self.row('extra.txt',b'extra')
        with self.assertRaisesRegex(AssertionError,'MISSING_OR_EXTRA'):physical_inventory(self.stage,[row],[])
        extra=self.stage/'extra.txt';expected=[row,dict(path='extra.txt',bytes=5,sha256=sha(b'extra'))]
        (self.stage/'safe.txt').write_bytes(b'changed')
        with self.assertRaisesRegex(AssertionError,'CHANGED_STAGED_BYTES'):physical_inventory(self.stage,expected,[])
    def test_raw_diagnostic_renamed_copy_is_rejected(self):
        row=self.row('renamed.txt',b'raw-protected-receipt')
        with self.assertRaisesRegex(AssertionError,'PROTECTED_RAW'):physical_inventory(self.stage,[row],[dict(path='different.raw',original=dict(sha256=row['sha256']))])
    def test_duplicate_manifest_identity_is_rejected(self):
        row=self.row('safe.txt',b'ordinary')
        with self.assertRaisesRegex(AssertionError,'DUPLICATE_EXPECTED'):physical_inventory(self.stage,[row,row],[])
    def test_actual_scanner_rejects_sensitive_staged_diagnostics(self):
        values=[str(ROOT/'raw.log'),chr(67)+':'+chr(92)+'Users'+chr(92)+'synthetic'+chr(92)+'secret.log',
                'https'+'://name:secret'+'@example.invalid','password'+'='+'abcdefgh1234', 'gh'+'p_'+'A'*24]
        for n,value in enumerate(values):
            row=self.row('case'+str(n)+'.txt',value.encode())
            findings,_,details,_=scan(self.stage,[row['path']],[])
            self.assertTrue(findings)
            with self.assertRaisesRegex(AssertionError,'UNRESOLVED'):decide(findings,details,[row],[])
    def test_unknown_non_utf8_is_rejected(self):
        row=self.row('unknown.txt',bytes([225]))
        with self.assertRaisesRegex(AssertionError,'UNKNOWN_NON_UTF8'):scan(self.stage,[row['path']],[])
    def test_pinned_legacy_views_still_detect_sensitive_material(self):
        raw=bytes([225])+b' '+str(ROOT/'raw.log').encode();row=self.row('known.vb',raw)
        findings,_,details,encoding=scan(self.stage,['known.vb'],[row])
        self.assertTrue(findings);self.assertEqual(1,len(encoding));self.assertEqual({'cp1250','cp1252'},{r['encoding_view'] for r in details})
        with self.assertRaisesRegex(AssertionError,'UNRESOLVED'):decide(findings,details,[row],[])

if __name__=='__main__':
    stream=io.StringIO();result=unittest.TextTestRunner(stream=stream,verbosity=2).run(unittest.defaultTestLoader.loadTestsFromTestCase(ReportingBoundaryTests))
    save(PRIVATE/'boundary-tests-raw.json',dict(stdout=stream.getvalue(),testsRun=result.testsRun,failures=len(result.failures),errors=len(result.errors)))
    save('validation/reporting-boundary-tests.json',dict(testsRun=result.testsRun,failures=len(result.failures),errors=len(result.errors),exit_code=0 if result.wasSuccessful() else 1,
        identities=unittest.defaultTestLoader.getTestCaseNames(ReportingBoundaryTests),raw=pin(PRIVATE/'boundary-tests-raw.json'),production_tests_changed=False))
    print('Reporting boundary controls:',result.testsRun,'failures',len(result.failures),'errors',len(result.errors),flush=True)
    raise SystemExit(0 if result.wasSuccessful() else 1)
