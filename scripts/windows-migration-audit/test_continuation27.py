"""Falsification of C27 exact census, source pins and nonacceptance boundaries."""
import copy,hashlib,json,pathlib,tempfile,unittest
import continuation27_report as c
class Continuation27Tests(unittest.TestCase):
 @classmethod
 def setUpClass(cls):
  cls.targets=c.a.read(c.H/'targets.json');cls.f=c.a.read(c.H/'frozen-source.json');cls.members=c.a.read(c.H/'member-reviews.json');cls.controls=c.decisions();cls.ctx=c.a.read(c.H/'control-contexts.json');cls.symbols=c.catalog.load(c.L,'symbols.csv')
 def test_actual_targets(self):self.assertEqual([],c.target_errors(self.targets))
 def test_missing_duplicate_member_ids(self):
  for op in ('missing','duplicate'):
   v=copy.deepcopy(self.targets)
   if op=='missing':v['members'].pop()
   else:v['members'][0]=v['members'][1]
   self.assertTrue(c.target_errors(v))
 def test_swapped_body_hash_and_utf16_span(self):
  for key in ('body_sha256','span_start','span_length','name','source_excerpt'):
   v=copy.deepcopy(self.targets);v['members'][0][key]=v['members'][1][key];self.assertTrue(c.target_errors(v),key)
 def test_missing_foreign_duplicate_control_ids(self):
  for op in ('missing','foreign','duplicate','order'):
   v=copy.deepcopy(self.targets);cs=v['groups'][0]['controls']
   if op=='missing':cs.pop()
   elif op=='foreign':cs[0]['control_id']='BESHStatNG.Foreign.NoSuchControl'
   elif op=='duplicate':cs[0]=cs[1]
   else:cs.reverse()
   self.assertTrue(c.target_errors(v),op)
 def test_exact_member_source(self):self.assertEqual([],c.source_errors(self.symbols,self.members))
 def test_member_body_span_and_schema(self):
  for key in ('body_sha256','source_sha256','source_span_start','source_span_length','source_excerpt'):
   v=copy.deepcopy(self.members);v[0][key]='wrong';self.assertTrue(c.source_errors(self.symbols,v),key)
  header=list(map(str,range(81)));self.assertEqual([],c.schema_errors(header,header));self.assertTrue(c.schema_errors(header,header[:-1]));self.assertTrue(c.schema_errors(header,header[::-1]))
 def test_missing_constructor_state_and_delegate_source(self):
  for name in ('.ctor','UpdateCDFOptionState','UpdateViolinBandwidthOptionState','UpdateBoxPlotOptionState','UpdateCategoricalHistogramOptionState','lambda'):
   victim=next(s for s in self.f['full_form_members'] if s['name']==name)
   self.assertTrue(c.source_errors([s for s in self.symbols if s['symbol_id']!=victim['symbol_id']],self.members),name)
 def admission(self,rows,contexts=None):
  expected={d['control_id']:d for d in self.f['full_form_controls'] if d['control_id'] in {r['control_id'] for r in rows}}
  return c.admission(rows,expected,self.ctx if contexts is None else contexts,self.f)
 def test_actual_all195_adjudications(self):self.assertEqual([],self.admission(self.controls))
 def test_each_resumable_group(self):
  for group in 'ABCDE':self.assertEqual([],self.admission(c.decisions(group)),group)
 def test_missing_me_tag_reparenting_mode_context(self):
  for key in ('creation','ancestor_chain','mode_policy_pin'):
   ctx=copy.deepcopy(self.ctx);ctx[0][key]=[] if key!='mode_policy_pin' else {};self.assertTrue(self.admission([self.controls[0]],ctx),key)
 def test_missing_cross_group_lifecycle_or_constructor_dependency(self):
  for key in ('control_flow_pins','required_constructor_initialization_ids'):
   d=copy.deepcopy(self.controls[0]);d[key]='[]';self.assertTrue(self.admission([d]))
 def test_false_qualification_nonempty_fields(self):
  d=copy.deepcopy(next(d for d in self.controls if d['intent_completeness']=='deferred'));d.update(review_contract='04A.01-control-v1',intent_completeness='qualified',unsupported_contract_field='',defaults_contract='nonempty',option_contract='nonempty')
  self.assertTrue(self.admission([d]))
 def test_deferral_missing_owner_fixture_or_unsupported_field(self):
  for key in ('owner_role','future_batch','verification','unsupported_contract_field','owner_approval_marker'):
   d=copy.deepcopy(next(d for d in self.controls if d['intent_completeness']=='deferred'));d[key]='';self.assertTrue(self.admission([d]),key)
 def test_actual_and_missing_handler_crosswalk(self):
  edges=self.f['full_form_wiring'];e=next(e for e in edges if e['kind']=='AddHandler');row=dict(e,handler_source_ids=[e['owner_id']]);self.assertEqual([],c.wiring_errors([row],[e],self.f['full_form_members']));row['handler_source_ids']=[];self.assertTrue(c.wiring_errors([row],[e],self.f['full_form_members']))
 def test_literal_prefix_mutation_or_reorder(self):
  old=b'[{"a":1},{"b":2}]';self.assertEqual([],c.prefix_errors(old,b'[{"a":1},{"b":2},{"c":3}]',2));self.assertTrue(c.prefix_errors(old,b'[{ "a":1},{"b":2}]',2));self.assertTrue(c.prefix_errors(old,b'[{"b":2},{"a":1}]',2))
 def test_prior_artifact_same_size_and_size_corruption(self):
  with tempfile.TemporaryDirectory() as tmp:
   base=pathlib.Path(tmp);p=base/'history';p.write_bytes(b'abcd');rows=[dict(path='history',bytes=4,sha256=hashlib.sha256(b'abcd').hexdigest())];self.assertEqual([],c.checked_files(rows,base))
   for raw in (b'abce',b'abcde'):
    p.write_bytes(raw);self.assertTrue(c.checked_files(rows,base))
 def test_proposal_never_accepts_strict_zero(self):
  p=c.a.read(c.H/'acceptance-policy-proposal.json');self.assertEqual([],c.proposal_errors(p))
  for key,value in [('status','ACCEPTED'),('owner_approval_marker','APPROVED'),('strict_profile_exit_while_open',0),('implementation_authorized',True)]:
   v=copy.deepcopy(p);v[key]=value;self.assertTrue(c.proposal_errors(v),key)
  self.assertEqual(2,c.readiness_exit(0,1,0,622));self.assertEqual(1,c.readiness_exit(0,1,0,622,['integrity']))
if __name__=='__main__':unittest.main(verbosity=2)
