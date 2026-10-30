"""Isolated C28 falsification. No production or live evidence mutations."""
import copy,json,pathlib,shutil,tempfile,unittest
import continuation28_report as c
class Continuation28Tests(unittest.TestCase):
 @classmethod
 def setUpClass(cls):
  cls.all=c.rows();cls.f=c.frozen();cls.contexts={r['control_id']:r for r in c.read(c.H/'control-contexts.json')}
 def selected(self,tier='P0'):
  r=copy.deepcopy(next(r for r in self.all if r['risk_tier']==tier));return r,{r['control_id']:self.f[r['control_id']]}
 def check(self,r,f,root=None):return c.record_errors(r,f,self.contexts,root)
 def test_28_complete_exact_census(self):
  self.assertEqual(892,len(self.all));self.assertEqual([],self.check(self.all,self.f));self.assertEqual({'A':182,'B':136,'C':186,'D':194,'E':160,'F':34},c.COUNTS)
 def test_28_missing_target(self):self.assertTrue(self.check(self.all[:-1],self.f))
 def test_28_duplicate_target(self):self.assertTrue(self.check(self.all+[self.all[0]],self.f))
 def test_28_foreign_or_qualified_target(self):
  r,f=self.selected();r['control_id']='BESHStatNG.UibyID.grpDescriptiveStat';self.assertTrue(self.check([r],f))
 def test_28_changed_source_hash(self):
  r,f=self.selected();r['source_sha256']='0'*64;self.assertTrue(self.check([r],f))
 def test_28_wrong_declaration_line(self):
  r,f=self.selected();r['declaration_line']+=1;self.assertTrue(self.check([r],f))
 def test_28_wrong_form(self):
  r,f=self.selected();r['path']='BESHstatNG/src/UI/UibyID.Designer.vb';self.assertTrue(self.check([r],f))
 def test_28_unpinned_member_body(self):
  r,f=self.selected();r['source_pin']['member_body_pins'][0]['body_sha256']='f'*64;self.assertTrue(self.check([r],f))
 def test_28_control_v1_promotion(self):
  r,f=self.selected();r['current_review_contract']='04A.01-control-v1';r['full_contract_qualified']=True;self.assertTrue(self.check([r],f))
 def test_28_prior_sidecar_deleted_on_isolated_copy(self):
  r,f=self.selected();entry=next(iter(f.values()))
  with tempfile.TemporaryDirectory(prefix='beshstat-c28-') as name:
   root=pathlib.Path(name);p=root/r['path'];p.parent.mkdir(parents=True);shutil.copyfile(c.R/r['path'],p)
   prior=root/entry['prior_evidence_path'];prior.parent.mkdir(parents=True,exist_ok=True);shutil.copyfile(c.R/entry['prior_evidence_path'],prior)
   self.assertEqual([],self.check([r],f,root));prior.unlink();self.assertTrue(self.check([r],f,root))
 def test_28_actual_same_size_corruption_on_copy(self):
  r,f=self.selected()
  with tempfile.TemporaryDirectory(prefix='beshstat-c28-') as name:
   root=pathlib.Path(name);p=root/r['path'];p.parent.mkdir(parents=True);raw=(c.R/r['path']).read_bytes();p.write_bytes(b'X'+raw[1:]);self.assertTrue(self.check([r],f,root))
 def test_28_prior_literal_order_or_content(self):
  self.assertTrue(c.literal_errors(b'[{"a":1},{"b":2}]',b'[{"b":2},{"a":1}]'));self.assertTrue(c.literal_errors(b'[{"a":1}]',b'[{"a":2}]'))
 def test_28_indexed_same_size_history_corruption(self):
  with tempfile.TemporaryDirectory(prefix='beshstat-c28-index-') as name:
   root=pathlib.Path(name);p=root/'prior.csv';p.write_bytes(b'old history\n');entry=dict(path='prior.csv',bytes=p.stat().st_size,sha256=c.a.sha(p));self.assertEqual([],c.artifact_errors([entry],root));p.write_bytes(b'bad history\n');self.assertTrue(c.artifact_errors([entry],root));p.write_bytes(b'old history\nextra');self.assertTrue(c.artifact_errors([entry],root))
 def test_28_changed_report_cannot_suppress_strict(self):
  with tempfile.TemporaryDirectory(prefix='beshstat-c28-report-') as name:
   p=pathlib.Path(name)/'report.json';v=dict(source_members=3866,ui_members=1547,qualified_controls=1164,incomplete_controls=892,unresolved_wiring=622,exports=268,strict_exit=0,gate_c2='UNMET',acceptance_policy='UNCHANGED');p.write_text(json.dumps(v),encoding='utf-8');self.assertTrue(c.invariant_errors(json.loads(p.read_text(encoding='utf-8'))))
 def test_28_unpinned_tag_or_tab_modes(self):
  matrix=[r for r in c.catalog.load(c.L,'continuation27-mode-control-matrix.csv') if r['control_id'] in self.f];self.assertEqual(2720,len(matrix));self.assertEqual([],c.mode_errors(matrix,self.all));self.assertTrue(c.mode_errors(matrix[:-1],self.all));bad=copy.deepcopy(matrix);bad[0]['tab_attachment']='assumed-dead';self.assertTrue(c.mode_errors(bad,self.all))
 def test_28_high_impact_cannot_be_p3(self):
  r,f=self.selected();r.update(risk_tier='P3',is_04A02_goldens_blocker=False,acceptance_treatment_proposal='GATE_C2_OWNER_REVIEW',P3_exclusion_proof={});self.assertTrue(self.check([r],f))
 def test_28_unknown_cannot_be_accepted(self):
  r,f=self.selected('UNKNOWN_HIGH');r.update(acceptance_treatment_proposal='LATER_NATIVE_OR_RELEASE_GATE',owner_approval_marker='ACCEPTED',triage_status='source-supported-assigned');self.assertTrue(self.check([r],f))
 def test_28_invented_batch(self):
  r,f=self.selected();r['target_approved_batch']='04A.99';self.assertTrue(self.check([r],f))
 def test_28_owner_fixture_required(self):
  for key in ('owner_role','named_test_fixture','mitigation_or_safe_capture_constraint'):
   r,f=self.selected();r[key]='';self.assertTrue(self.check([r],f))
  r=copy.deepcopy(next(r for r in self.all if 'LEGACY_SCHEMA_NO_UNSUPPORTED_COLUMN' in r['prior_unsupported_contract_field']));f={r['control_id']:self.f[r['control_id']]};r['prior_unsupported_contract_field']='NO_INDIVIDUAL_CONTROL_SIDECAR';self.assertTrue(self.check([r],f))
 def test_28_actual_edges_cannot_be_synthetic(self):
  r,f=self.selected();r['related_wiring_edge_ids']=['invented-one-edge-per-control'];self.assertTrue(self.check([r],f))
 def test_28_unchanged_wiring_and_strict_denominators(self):
  value=dict(source_members=3866,ui_members=1547,qualified_controls=1164,incomplete_controls=892,unresolved_wiring=622,exports=268,strict_exit=2,gate_c2='UNMET',acceptance_policy='UNCHANGED');self.assertEqual([],c.invariant_errors(value))
  for key,val in [('unresolved_wiring',0),('strict_exit',0),('incomplete_controls',0),('qualified_controls',2056),('acceptance_policy','APPROVED')]:
   bad=dict(value);bad[key]=val;self.assertTrue(c.invariant_errors(bad))
if __name__=='__main__':unittest.main(verbosity=2)
