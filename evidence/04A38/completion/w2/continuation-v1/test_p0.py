"""Adverse source/caller/ByRef proof fixtures; no original semantic queries."""
import json,os,unittest,uuid,contextlib,hashlib
from pathlib import Path
import verify_p0 as reader

class P0Tests(unittest.TestCase):
 @classmethod
 def setUpClass(cls):
  cls.view=reader.ROOT/'.test-rationalization-04A38.tmp'/('w2c1-p0-adverse-'+uuid.uuid4().hex[:8]);cls.view.mkdir()
  here=reader.HERE;paths={p.relative_to(reader.ROOT).as_posix() for p in here.rglob('*') if p.is_file()}
  paths.update(['evidence/04A38/preedit/source/Regression/MixedModelUDFs.vb.txt','BESHStat.Core/src/RegModels/MixedModels/MixedModelPostEstimation.vb','BESHStat.Core/src/WorksheetFunctions/UdfProfilePreparation38.vb'])
  for c in ['Debug','Release']:paths.add('evidence/04A38/preedit/compiled-'+c+'.json')
  for row in reader.load(here/'compatibility-p0-v2.json')['results']:paths.update([row['original']['path'],row['current']['path']])
  for p in paths:
   src=reader.ROOT/p;dst=cls.view/p;dst.parent.mkdir(parents=True,exist_ok=True);os.link(src,dst)
  reader.inspect(cls.view)
 @contextlib.contextmanager
 def change(self,path,mutate):
  p=self.view/path;old=p.read_bytes();new=mutate(old);self.assertNotEqual(old,new,'Invalid adverse fixture')
  tmp=self.view/'replacement';tmp.write_bytes(new);os.replace(tmp,p)
  try:yield
  finally:tmp.write_bytes(old);os.replace(tmp,p)
 def rejects(self,path,mutate):
  with self.change(path,mutate):
   with self.assertRaises((ValueError,OSError,KeyError,TypeError)):reader.inspect(self.view)
 def test_intact(self):self.assertEqual(2,reader.inspect(self.view)['new_historical_proof_supported'])
 def test_stale_before(self):self.rejects(reader.HERE.relative_to(reader.ROOT).as_posix()+'/source-before/'+reader.MIXED+'.txt',lambda b:b+b' ')
 def test_changed_description_separator(self):self.rejects(reader.HERE.relative_to(reader.ROOT).as_posix()+'/source-after/p0-v2/'+reader.COMMON+'.txt',lambda b:b.replace(b'String.Join(", ", parts)',b'String.Join("; ", parts)',1))
 def test_reordered_mask_guard(self):self.rejects(reader.HERE.relative_to(reader.ROOT).as_posix()+'/source-after/p0-v2/'+reader.COMMON+'.txt',lambda b:b.replace(b'If matchedCount <= 0 Then',b'If matchedCount < 0 Then',1))
 def test_missing_original(self):self.rejects('evidence/04A38/preedit/source/Regression/MixedModelUDFs.vb.txt',lambda b:b[:-1])
 def test_current_output_mutation(self):self.rejects(reader.HERE.relative_to(reader.ROOT).as_posix()+'/captures/p0-v2/Debug/C/results.tsv',lambda b:b[:-20])
 def test_early_formatter_caller_forged(self):
  def mutate(b):
   d=json.loads(b);m=next(m for m in d['methods'] if m['name']=='TryBuildMmrmAtProfileRowMask');m['calls']=[c for c in m['calls'] if c['opcode']!='ldftn'];return json.dumps(d).encode()
  self.rejects(reader.HERE.relative_to(reader.ROOT).as_posix()+'/inspection/p0-v2/Inspect38-Debug.json',mutate)
 def test_portable_hidden_state(self):
  def mutate(b):
   d=json.loads(b);r=next(r for r in d if '.ExcelCommon.UdfModelProfilePreparation38.TryBuildMmrmAtProfileRowMask(' in r['method']);r['nonliteral_static_fields']=1;return json.dumps(d).encode()
  self.rejects(reader.HERE.relative_to(reader.ROOT).as_posix()+'/inspection/p0-v2/MethodBodies38-Release.json',mutate)
 def test_formatter_reimplementation(self):self.rejects('BESHStat.Core/src/RegModels/MixedModels/MixedModelPostEstimation.vb',lambda b:b.replace(b'"G6"',b'"G5"',1))
if __name__=='__main__':unittest.main()
