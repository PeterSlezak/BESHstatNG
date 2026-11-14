"""Independent-reader corruption controls on isolated copies only."""
import contextlib,json,os,unittest,uuid
from pathlib import Path
import verify_p1_v5 as reader
class P1Tests(unittest.TestCase):
 @classmethod
 def setUpClass(cls):
  cls.view=reader.ROOT/'.test-rationalization-04A38.tmp'/('w2c1-p1-adverse-'+uuid.uuid4().hex[:8]);cls.view.mkdir()
  paths={p.relative_to(reader.ROOT).as_posix() for p in reader.HERE.rglob('*') if p.is_file()}
  paths.update([reader.PROFILE,reader.NUMERIC,reader.REPORT,reader.PREFIX+'Regression/GeeUDFs.vb',reader.PREFIX+'Regression/MixedModelUDFs.vb',reader.PREFIX+'PlotDataUDFs.vb',reader.PREFIX+'UDFdataImport/UdfDataImport.vb',reader.PREFIX+'UDFdataImport/UdfDataImportRangeReaders.vb','BESHStat.Excel.Common/src/Analysis/UdfRangePreparation38.vb','BESHStat.Excel.Common.Tests/src/UdfModelProfilePreparation38Tests.vb','BESHStatNG.Test/src/UDFs/UdfPreparation38AdapterTests.vb','evidence/04A38/preedit/entry-source-snapshot.json','evidence/04A38/completion/w1/member-behavior-evidence-target-matrix-v3.json'])
  for row in reader.load(reader.ROOT/'evidence/04A38/preedit/entry-source-snapshot.json')['files']:paths.add(row['snapshot']['path'])
  for c in ['Debug','Release']:paths.add('evidence/04A38/preedit/compiled-'+c+'.json')
  for row in reader.load(reader.HERE/('compatibility-'+reader.STAGE+'.json'))['results']:paths.update([row['original']['path'],row['current']['path']])
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
   with self.assertRaises((ValueError,OSError,KeyError,TypeError,StopIteration)):reader.inspect(self.view)
 @property
 def here(self):return reader.HERE.relative_to(reader.ROOT).as_posix()+'/'
 def test_intact(self):self.assertEqual(11,reader.inspect(self.view)['proof_supported_historical_additions'])
 def test_stale_preimage(self):self.rejects(self.here+'source-before/p1/'+reader.PROFILE+'.txt',lambda b:b+b' ')
 def test_original_snapshot_lost(self):self.rejects('evidence/04A38/preedit/source/PlotDataUDFs.vb.txt',lambda b:b[:-1])
 def test_current_result_changed(self):self.rejects(self.here+'captures/p1-v5/Debug/A/results.tsv',lambda b:b.replace(b'D4045000000000000',b'D4044000000000000',1))
 def test_case_omission(self):self.rejects(self.here+'captures/p1-v5/Release/G/results.tsv',lambda b:b.split(b'\n',1)[1])
 def test_extra_case(self):self.rejects(self.here+'captures/p1-v5/Debug/C/results.tsv',lambda b:b+b'UNAPPROVED\n')
 def test_fake_edge(self):
  def mutate(b):
   d=json.loads(b);r=next(r for r in d['methods'] if r['name']=='ApplyGeeLsmEstimateAtProfile');r['calls']=[];return json.dumps(d).encode()
  self.rejects(self.here+'inspection/p1-v5/Inspect38-Debug.json',mutate)
 def test_changed_export_default(self):
  def mutate(b):
   d=json.loads(b);d['exports'][0]['effective_excel_name']+='-ALTERED';return json.dumps(d).encode()
  self.rejects(self.here+'inspection/p1-v5/Inspect38-Release.json',mutate)
 def test_hidden_static_state(self):
  def mutate(b):
   d=json.loads(b);r=next(r for r in d if '.ExcelCommon.PlotDataUDFsReports38.BuildCalibrationPointsTable(' in r['method']);r['nonliteral_static_fields']=1;return json.dumps(d).encode()
  self.rejects(self.here+'inspection/p1-v5/MethodBodies38-Debug.json',mutate)
 def test_ornamental_contains(self):
  def mutate(b):
   d=json.loads(b);r=next(r for r in d if '.UdfModelProfilePreparation38.ApplyGeeAtProfile(' in r['method']);r['instructions']=[i for i in r['instructions'] if not (isinstance(i['operand'],str) and 'GeeComponentHasProfileColumn(' in i['operand'])];return json.dumps(d).encode()
  self.rejects(self.here+'inspection/p1-v5/MethodBodies38-Release.json',mutate)
 def test_failed_tests_not_green(self):
  def mutate(b):
   d=json.loads(b);d['results'][-1]['tests']['outcomes']={'Passed':27,'Failed':1};return json.dumps(d).encode()
  self.rejects(self.here+'focused-p1-v5-Debug.json',mutate)
 def test_self_consistent_changed_column_guard(self):
  self.semantic_mutation(reader.PROFILE,b'If value.ColumnIndex = columnIndex Then Return True',b'If value.ColumnIndex <> columnIndex Then Return True')
 def test_self_consistent_error_coercion_change(self):
  self.semantic_mutation(reader.REPORT,b'If(Double.IsNaN(r.LowerCI), missingIntervalValue,',b'If(Double.IsNaN(r.LowerCI), 0.0R,')
 def test_self_consistent_group_nonfinite_policy_change(self):
  self.semantic_mutation(reader.NUMERIC,b'OrElse Double.IsInfinity(d.Value)',b'OrElse False')
 def semantic_mutation(self,path,old,new):
  transition=self.here+'source-transitions-p1-v5.json';records=reader.load(self.view/transition);row=next(r for r in records['files'] if r['after']['path']==path);payload=(self.view/path).read_bytes().replace(old,new,1)
  row['after']['sha256']=reader.digest(payload);row['after']['bytes']=len(payload);row['snapshot']['sha256']=reader.digest(payload);row['snapshot']['bytes']=len(payload)
  with self.change(path,lambda b:payload),self.change(row['snapshot']['path'],lambda b:payload),self.change(transition,lambda b:json.dumps(records).encode()):
   with self.assertRaises(ValueError):reader.inspect(self.view)
if __name__=='__main__':unittest.main()
