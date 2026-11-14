"""Private-copy adversarial tests; no original file edited or reference run."""
import contextlib,json,os,unittest,uuid
from pathlib import Path
import verify_supplement as reader

class SupplementAdverseTests(unittest.TestCase):
 @classmethod
 def setUpClass(cls):
  cls.view=reader.ROOT/'.test-rationalization-04A38.tmp'/('late38-adverse-'+uuid.uuid4().hex);cls.view.mkdir(exist_ok=False)
  cls.originals=[]
  for source in (reader.ROOT/reader.PREFIX).rglob('*'):
   if not source.is_file():continue
   target=cls.view/source.relative_to(reader.ROOT);target.parent.mkdir(parents=True,exist_ok=True);os.link(source,target)
   cls.originals.append((source,reader.digest(source.read_bytes())))
 @classmethod
 def tearDownClass(cls):
  for path,expected in cls.originals:assert reader.digest(path.read_bytes())==expected,'Original changed'
 @contextlib.contextmanager
 def changed(self,path,mutate):
  target=self.view/reader.PREFIX/path;old=target.read_bytes();replacement=target.with_name(target.name+'.'+uuid.uuid4().hex)
  replacement.write_bytes(mutate(old));os.replace(replacement,target)
  try:yield
  finally:replacement.write_bytes(old);os.replace(replacement,target)
 def rejects(self,path,mutate):
  with self.changed(path,mutate):
   with self.assertRaises((reader.InvalidSupplement,OSError,ValueError,KeyError)):reader.inspect(self.view)
 def jchange(self,path,key,value):self.rejects(path,lambda b:(json.dumps(dict(json.loads(b),**{key:value}))+'\n').encode())
 def test_intact_exact_late_supplement(self):self.assertEqual('LATE_SUPPLEMENT_EXACT_AND_COVERAGE_PASS_NOT_BATCH_READY',reader.inspect(self.view)['status'])
 def test_wrong_original_commit(self):self.jchange('original-source-authenticity.json','source_commit','0'*40)
 def test_changed_original_source_hash(self):
  self.rejects('original-source-authenticity.json',lambda b:b.replace(b'"sha256": "',b'"sha256": "0',1))
 def test_missing_owner(self):self.jchange('owner-authorization.json','owner_quote','UNAPPROVED')
 def test_wrong_owner_date(self):self.jchange('owner-authorization.json','owner_decision_date','2000-01-01')
 def test_late_mislabeled_preedit(self):self.jchange('original-output-seal.json','is_pre_edit_frozen',True)
 def test_reference_after_current(self):self.jchange('original-output-seal.json','current_supplement_executed_at_seal',True)
 def test_wrong_loaded_original_assembly(self):
  self.rejects('original-results/Debug/plain-v2/execution.json',lambda b:b.replace(b'"sha256":"',b'"sha256":"0',1))
 def test_omitted_case(self):self.rejects('inputs.jsonl',lambda b:b.split(b'\n',1)[1])
 def test_duplicate_case(self):self.rejects('inputs.jsonl',lambda b:b+b.splitlines(keepends=True)[0])
 def test_casefold_duplicate(self):self.rejects('inputs.jsonl',lambda b:b+b.splitlines(keepends=True)[0].swapcase())
 def test_reordered_input(self):self.rejects('inputs.jsonl',lambda b:b''.join(reversed(b.splitlines(keepends=True))))
 def test_changed_culture(self):self.rejects('inputs.jsonl',lambda b:b.replace(b'"culture":""',b'"culture":"en-US"',1))
 def test_changed_array_bound(self):self.rejects('inputs.jsonl',lambda b:b.replace(b'"lower_bounds":[0,0]',b'"lower_bounds":[1,0]',1))
 def test_changed_array_value(self):self.rejects('inputs.jsonl',lambda b:b.replace(b'3FA999999999999A',b'3FB999999999999A',1))
 def test_configuration_cases_differ(self):self.rejects('inputs-Release.jsonl',lambda b:b[:-1])
 def test_altered_original_output(self):self.rejects('original-results/Debug/plain-v2/results.tsv',lambda b:b+b'corrupt')
 def test_altered_repeat(self):self.rejects('original-results/Release/repeat-v2/results.tsv',lambda b:b[:-1])
 def test_guard_falsely_counted_as_report(self):
  self.rejects('original-coverage.json',lambda b:b.replace(b'"report_reached": 90',b'"report_reached": 144',1))
 def test_missing_trace(self):self.rejects('original-results/Debug/trace-v2/results.tsv.branches.json',lambda b:b'[]')
 def test_current_mismatch(self):self.rejects('current-results/Debug/results.tsv',lambda b:b+b'corrupt')
 def test_current_branch_fabrication(self):self.rejects('current-results/Release/results.tsv.branches.json',lambda b:b'[]')
 def test_current_case_order(self):self.rejects('current-results/Release/results.tsv',lambda b:b.splitlines(keepends=True)[0]+b''.join(reversed(b.splitlines(keepends=True)[1:])))
 def test_current_lacks_reference_seal(self):
  self.rejects('current-execution-Debug.json',lambda b:b.replace(reader.SEAL.encode(),b'0'*64))

if __name__=='__main__':unittest.main()
