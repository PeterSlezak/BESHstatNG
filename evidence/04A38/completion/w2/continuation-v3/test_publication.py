"""Physical ZIP controls reject hidden extras as well as manifest corruption."""
import copy,io,json,zipfile,hashlib,unittest
from pathlib import Path
from verify_publication import ROOT,HERE,PREFIX,inspect,inspect_zip,privacy,safe_path,FIXTURES
MANIFEST=PREFIX+'publication-manifest-v1.json'
class PublicationControls(unittest.TestCase):
 def manifest(self,change):
  x=json.loads((ROOT/MANIFEST).read_bytes());change(x)
  with self.assertRaises((ValueError,KeyError,StopIteration)):inspect(manifest_path=MANIFEST,overrides={MANIFEST:json.dumps(x).encode()},semantic=False)
 def test_true_partial(self):self.assertEqual(inspect(manifest_path=MANIFEST)['status'],'VERIFIED_PARTIAL_C3_NOT_READY')
 def test_duplicate(self):self.manifest(lambda x:x['entries'].append(x['entries'][0]))
 def test_casefold(self):self.manifest(lambda x:x['entries'].append(dict(x['entries'][0],path=x['entries'][0]['path'].upper())))
 def test_traversal(self):self.manifest(lambda x:x['entries'][0].update(path='../escape'))
 def test_reserved(self):self.manifest(lambda x:x['entries'][0].update(path='NUL.txt'))
 def test_more_null(self):self.manifest(lambda x:x['entries'][0].update(bytes=None,sha256=None))
 def test_recursive_self(self):self.manifest(lambda x:next(r for r in x['entries'] if r['path']==MANIFEST).update(bytes=1,sha256='0'*64))
 def test_omitted_payload(self):self.manifest(lambda x:x['entries'].pop(0))
 def test_changed_hash(self):self.manifest(lambda x:x['entries'][0].update(sha256='0'*64))
 def test_wrong_status(self):self.manifest(lambda x:x.update(status='READY_FOR_OWNER_04A38_ACCEPTANCE_REVIEW'))
 def fake_zip(self,extras=(),mutate=None):
  p='fixture.txt';body=b'safe';rows=[dict(path=p,bytes=len(body),sha256=hashlib.sha256(body).hexdigest()),dict(path=MANIFEST,bytes=None,sha256=None)]
  if mutate:mutate(rows)
  stream=io.BytesIO()
  with zipfile.ZipFile(stream,'w') as z:
   z.writestr(p,body);z.writestr(MANIFEST,json.dumps(dict(entries=rows)))
   for name,b in extras:z.writestr(name,b)
  stream.seek(0);return stream
 def test_physical_safe_minimal(self):self.assertEqual(inspect_zip(self.fake_zip(),MANIFEST)['entries'],2)
 def test_extra_unmanifested(self):
  with self.assertRaises(ValueError):inspect_zip(self.fake_zip([('hidden.txt',b'safe')]),MANIFEST)
 def test_extra_raw_diagnostic(self):
  with self.assertRaises(ValueError):inspect_zip(self.fake_zip([(PREFIX+'full-managed-Debug-v1.json',b'{}')]),MANIFEST)
 def test_extra_cache(self):
  with self.assertRaises(ValueError):inspect_zip(self.fake_zip([('prior/__pycache__/foo.pyc',b'bytes')]),MANIFEST)
 def test_physical_hash(self):
  with self.assertRaises(ValueError):inspect_zip(self.fake_zip(mutate=lambda x:x[0].update(sha256='0'*64)),MANIFEST)
 def test_unattested_home_any_prefix(self):
  with self.assertRaises(ValueError):privacy('older/safe.json',('/'.join(['C:','Users','SYNTHETIC_PRIVATE','file'])).encode())
 def test_json_escaped_home(self):
  with self.assertRaises(ValueError):privacy('older/safe.json',json.dumps(dict(path='/'.join(['C:','Users','SYNTHETIC_PRIVATE','file']))).encode())
 def test_private_utf16(self):
  with self.assertRaises(ValueError):privacy('older/safe.txt',('/'.join(['C:','Users','SYNTHETIC_PRIVATE','file'])).encode('utf-16'))
 def test_unicode_escaped_json_home(self):
  b=json.dumps(dict(path='/'.join(['C:','Users','SYNTHETIC_PRIVATE','file']))).replace('Users','\\u0055sers').encode()
  with self.assertRaises(ValueError):privacy('older/safe.json',b)
 def test_unattested_uri_any_prefix(self):
  with self.assertRaises(ValueError):privacy('older/safe.txt',('https'+':/'+'/user'+':value'+'@example.invalid').encode())
 def test_fixture_changed_or_extra_occurrence(self):
  p='BESHStat.Excel.Common.Tests/src/CompositeOutputTests.vb'
  with self.assertRaises(ValueError):privacy(p,(ROOT/p).read_bytes()+b'\nchanged')
 def test_inherited_fixture_exact(self):
  p='BESHStat.Excel.Common.Tests/src/CompositeOutputTests.vb';self.assertTrue(privacy(p,(ROOT/p).read_bytes()))
 def test_raw_diagnostic_banned_even_synthetic_safe(self):
  with self.assertRaises(ValueError):safe_path(PREFIX+'full-managed-Debug-v1.json')
 def test_duplicate_manifest_inside_zip(self):
  with self.assertRaises(ValueError):inspect_zip(self.fake_zip(mutate=lambda x:x.append(x[0])),MANIFEST)
if __name__=='__main__':unittest.main(verbosity=2)
