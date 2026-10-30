"""Negatives mutate copies of actual C22 identities, bodies and evidence only."""
import unittest,copy,json,pathlib,tempfile,hashlib
from unittest import mock
import catalog,semantic,continuation22_report as c,continuation22_controls as guard

class Continuation22SourceControlTests(unittest.TestCase):
 @classmethod
 def setUpClass(cls):
  cls.ss=catalog.load(c.OUT,'symbols.csv');cls.cc=catalog.load(c.OUT,'ui-controls.csv');cls.ee=catalog.load(c.OUT,'ui-wiring.csv');cls.ds=semantic.read_store('review-decisions.json');cls.cs=semantic.read_store('control-review-decisions.json');cls.own=[d for d in cls.ds if d.get('checkpoint_id')=='04A.01-continuation-22'];cls.new=[d for d in cls.cs if d.get('checkpoint_id')=='04A.01-continuation-22']
 def verify(self,**kw):return guard.verify(kw.get('cs',self.cs),kw.get('ss',self.ss),kw.get('cc',self.cc),kw.get('ee',self.ee))
 def test_22_exact_frozen_scope(self):
  self.assertEqual([50,48,52],[len(g['records']) for g in c.GROUPS]);self.assertEqual([71,65,84],[len(g['controls']) for g in c.GROUPS]);self.assertEqual(150,len(self.own));self.assertEqual([],c.scope_errors(self.ss,self.ds));self.assertEqual([],c.snapshot_errors(self.ds));self.assertEqual([],self.verify())
 def test_22_missing_duplicate_foreign_and_group_ids(self):
  for g in c.GROUPS:
   victim=g['records'][0]['symbol_id'];self.assertTrue(c.scope_errors(self.ss,[d for d in self.ds if d['symbol_id']!=victim]))
  self.assertTrue(c.scope_errors(self.ss,self.ds+[self.own[0]]))
  for field,val in [('symbol_id','foreign'),('review_group','Z')]:
   rows=copy.deepcopy(self.ds);next(d for d in rows if d['symbol_id']==self.own[0]['symbol_id'])[field]=val;self.assertTrue(c.scope_errors(self.ss,rows))
 def test_22_utf16_body_profile_identity(self):
  for key in ('source_span_start','source_span_length','source_profile','source_kind','source_enclosing_symbol_id','source_excerpt','body_sha256','source_sha256'):
   rows=copy.deepcopy(self.ds);next(d for d in rows if d['symbol_id']==self.own[0]['symbol_id'])[key]='changed';self.assertTrue(c.scope_errors(self.ss,rows),key)
 def test_22_designer_init_dispose_required(self):
  for g in c.GROUPS:
   for s in g['records']:
    if s['name'] in ('InitializeComponent','Dispose'):
     self.assertTrue(self.verify(ss=[x for x in self.ss if x['symbol_id']!=s['symbol_id']]))
  for field in ('required_constructor_initialization_ids','source_witnesses'):
   rows=copy.deepcopy(self.cs);d=next(d for d in rows if d['control_id']==self.new[0]['control_id']);d[field]='[]';self.assertTrue(self.verify(cs=rows))
 def test_22_initializer_records_are_not_dropped(self):
  ids={s['symbol_id'] for s in c.SOURCE.values() if s['kind']=='VariableDeclarator'};self.assertEqual(5,len(ids));self.assertTrue(c.scope_errors(self.ss,[d for d in self.ds if d['symbol_id'] not in ids]))
 def test_22_output_radio_handles_and_state_required(self):
  for g in c.GROUPS:
   paths={s['path'] for s in g['records']}
   for e in self.ee:
    if e['path'] in paths and e['kind'] in ('Handles','AddHandler','AddressOf'):
     self.assertTrue(self.verify(ee=[x for x in self.ee if x['edge_id']!=e['edge_id']]))
   if g['group']!='C':
    victim=next(s for s in g['records'] if s['name']=='UpdateOutputRangeState');self.assertTrue(self.verify(ss=[x for x in self.ss if x['symbol_id']!=victim['symbol_id']]))
 def test_22_optional_header_weight_and_worksheet_identity_required(self):
  for group,names in [('A',['HasLeadingHeader','ValidateOptionalAlignedRange','AreRangesOnSameWorksheet','GetCommonInputLastRow']),('B',['ComputeStagedResult','ComputeSourceTargetResult','ToStageVector','ToWeightVector','AreRangesOnSameWorksheet']),('C',['getData','getDataMultipleGroups','Create3DObjectsList','TryApplyManualAxisLimits'])]:
   for name in names:
    victim=next(s for s in c.GROUPS[ord(group)-65]['records'] if s['name']==name);rows=copy.deepcopy(self.ss);next(s for s in rows if s['symbol_id']==victim['symbol_id'])['body_sha256']='0'*64;self.assertTrue(self.verify(ss=rows),name)
 def test_22_reparent_option_and_color_dependencies_required(self):
  for group,names in [('A',['UpdateOptionControlStates','btnActualBarColor_Click','btnTargetColor_Click']),('B',['PrepareSharedOutputGroup','MoveOutputGroupToActiveInputTab','TabMultipage_SelectedIndexChanged','UpdateLabelControlState']),('C',['btResetView_Click','btnAnimatedGif_Click','ChooseAnimatedGifOutput','ckXreverseAxis_CheckedChanged','cbXreverseAxis_CheckedChanged'])]:
   for name in names:
    victim=next(s for s in c.GROUPS[ord(group)-65]['records'] if s['name']==name);self.assertTrue(self.verify(ss=[x for x in self.ss if x['symbol_id']!=victim['symbol_id']]))
  e=next(e for e in self.ee if e['path'].endswith('/Ui01BulletChart.vb') and e['kind']=='Handles' and 'btnTargetColor' in e['event_or_control']);rows=copy.deepcopy(self.ee);next(x for x in rows if x['edge_id']==e['edge_id'])['target']='wrong panel';self.assertTrue(self.verify(ee=rows))
 def test_22_actual_copied_member_body_mutations_rejected(self):
  # Redirect all dependency reads to a temporary clone, never modify product source.
  deps={p for d in self.new for p in json.loads(d['dependency_pins'])};old=guard.ROOT
  with tempfile.TemporaryDirectory(prefix='beshstat-c21-') as directory:
   root=pathlib.Path(directory)
   for name in deps:
    p=root/name;p.parent.mkdir(parents=True,exist_ok=True);p.write_bytes((old/name).read_bytes())
   with mock.patch.object(guard,'ROOT',root):
    self.assertEqual([],self.verify())
    cases=[('A','HasLeadingHeader'),('A','AreRangesOnSameWorksheet'),('A','UpdateOptionControlStates'),('B','MoveOutputGroupToActiveInputTab'),('B','ComputeStagedResult'),('B','ToStageVector'),('C','getData'),('C','ChooseAnimatedGifOutput'),('C','ckXreverseAxis_CheckedChanged')]
    cases += [(group,name) for group in 'ABC' for name in ('InitializeComponent','Dispose')] + [(group,name) for group in 'AB' for name in ('optOutputRange_CheckedChanged','ResolveOutputTarget')]
    for group,name in cases:
     s=next(s for s in c.GROUPS[ord(group)-65]['records'] if s['name']==name);p=root/s['path'];original=p.read_bytes();body=c.excerpt(s).encode('utf-8');self.assertIn(body,original);p.write_bytes(original.replace(body,body.replace(b' ',b'\t',1),1));self.assertEqual(len(original),p.stat().st_size);self.assertTrue(self.verify());p.write_bytes(original)
 def test_22_missing_qualified_controls_rejected(self):
  self.assertTrue(self.verify(cs=[d for d in self.cs if d['control_id']!=self.new[0]['control_id']]));self.assertTrue(self.verify(cs=self.cs+[self.new[0]]))
 def test_22_false_qualified_refedit_compute_or_empty_combo(self):
  for ident in ('BESHStatNG.Ui01BulletChart.RefEdit_MeasureLabels','BESHStatNG.Ui01SankeyPlot.grpOutput','BESHStatNG.Ui3XYZplot.btnAnimatedGif'):
   d=copy.deepcopy(self.new[0]);d['control_id']=ident;self.assertTrue(semantic.control_qualified(d));self.assertTrue(self.verify(cs=self.cs+[d]))
 def test_22_control_defaults_and_contract_fields(self):
  for field in ('defaults_contract','enabled_reset_contract','option_contract','validation_output_contract','verification','owner_role','control_flow_pins'):
   rows=copy.deepcopy(self.cs);next(d for d in rows if d['control_id']==self.new[0]['control_id'])[field]='';self.assertTrue(self.verify(cs=rows))
  rows=copy.deepcopy(self.cc);next(d for d in rows if d['control_id']==self.new[0]['control_id'])['initialization_defaults']='Checked=True';self.assertTrue(self.verify(cc=rows))
 def test_22_missing_owner_operation_fixture(self):
  for field in ('owner_role','candidate_operation_id','proposed_test','future_batch'):
   rows=copy.deepcopy(self.ds);next(d for d in rows if d.get('checkpoint_id')=='04A.01-continuation-22' and d.get('candidate_operation_id'))[field]='';self.assertTrue(c.scope_errors(self.ss,rows))
 def test_22_no_whole_form_or_false_export(self):
  for field,val in [('equivalence_scope','whole-method'),('registration_class','export')]:
   rows=copy.deepcopy(self.ds);next(d for d in rows if d['symbol_id']==self.own[0]['symbol_id'])[field]=val;self.assertTrue(c.scope_errors(self.ss,rows))
 def test_22_prior_ordered_literals_and_normalization(self):
  for name,n in [('review-decisions',2867),('control-review-decisions',608),('wiring-review-decisions',785)]:
   old=(c.EVIDENCE/'original/scripts/windows-migration-audit'/(name+'.json')).read_bytes().decode('utf-8-sig');now=(semantic.TOOLING/(name+'.json')).read_bytes().decode('utf-8-sig');self.assertEqual(n,len(json.loads(old)));self.assertEqual([],c.prefix_errors(now,old));self.assertTrue(c.prefix_errors(json.dumps(list(reversed(json.loads(now)))),old))
   if '\r\n' in old:self.assertTrue(c.prefix_errors(old.replace('\r\n','\n'),old))
 def test_22_same_size_history_corruption_rejected(self):
  import continuation03_report as check
  with tempfile.TemporaryDirectory(prefix='beshstat-c21-hash-') as directory:
   root=pathlib.Path(directory);p=root/'history';original=(c.EVIDENCE/'original/docs/architecture/WindowsMigrationLedger/continuation21-preservation.json').read_bytes();p.write_bytes(original);idx={'artifacts':[dict(path='history',bytes=len(original),sha256=hashlib.sha256(original).hexdigest())]};proposed=['history','docs/architecture/WindowsMigrationLedger/index.json'];self.assertEqual([],check.verify_index(root,idx,proposed));p.write_bytes(original.replace(b'PASS',b'FAIL',1));self.assertEqual(len(original),p.stat().st_size);self.assertTrue(check.verify_index(root,idx,proposed))
 def test_22_incomplete_ready_rejected(self):
  self.assertTrue(c.readiness_errors(dict(status='READY',unreviewed_for_acceptance=999,unresolved_edge_rows=1,qualified_full_control_contracts=555)))
 def test_22_deferred_accounting_ids_owners_fixtures(self):
  rows=[r for g in c.GROUPS for r in c.a.read(c.EVIDENCE/('group-'+g['group']+'-control-evidence.json'))['controls']];self.assertEqual([],c.accounting_errors(rows));self.assertTrue(c.accounting_errors(rows[1:]));self.assertTrue(c.accounting_errors(rows+[rows[0]]))
  for field in ('review_group','owner_role','future_batch','verification','qualification_reason'):
   bad=copy.deepcopy(rows);next(r for r in bad if r['intent_completeness']=='deferred')[field]='';self.assertTrue(c.accounting_errors(bad))
 def test_22_external_declarations_and_owners_are_pinned_without_review_credit(self):
  rows=catalog.load(c.OUT,'continuation22-external-dependencies.csv');self.assertEqual([],c.dependency_errors(rows));self.assertFalse({r['symbol_id'] for r in rows}&set(c.SOURCE))
  for key in ('body_sha256','span_start','source_excerpt','owner','future_batch','fixture'):
   bad=copy.deepcopy(rows);bad[0][key]='';self.assertTrue(c.dependency_errors(bad))


 def test_22_exact_deferred_and_qualified_control_form_assignment(self):
  rows=[r for g in c.GROUPS for r in c.a.read(c.EVIDENCE/('group-'+g['group']+'-control-evidence.json'))['controls']]
  self.assertEqual(220,len(rows));self.assertEqual(78,sum(r['intent_completeness']=='deferred' for r in rows));self.assertEqual(142,len(self.new))
  bad=copy.deepcopy(rows);bad[0]['control_id']='BESHStatNG.Ui01SankeyPlot.'+bad[0]['control_id'].split('.')[-1];self.assertTrue(c.accounting_errors(bad))
 def test_22_active_ck_and_unproven_cb_reverse_helpers_distinct(self):
  group=c.GROUPS[2];paths={s['path'] for s in group['records']};edges=[e for e in self.ee if e['path'] in paths]
  for axis in 'XYZ':
   active=next(s for s in group['records'] if s['name']=='ck'+axis+'reverseAxis_CheckedChanged');helper=next(s for s in group['records'] if s['name']=='cb'+axis+'reverseAxis_CheckedChanged')
   self.assertTrue(any(e['kind']=='Handles' and e['owner_id']==active['symbol_id'] for e in edges));self.assertFalse(any(e['kind'] in ('Handles','AddHandler','AddressOf') and helper['name'] in e['target'] for e in edges))

if __name__=='__main__':unittest.main(verbosity=2)
