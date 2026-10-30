"""Negative fixtures for C24 exact source/control/history and readiness integrity."""
import copy,hashlib,json,pathlib,tempfile,unittest
import continuation24_report as c

class Continuation24Tests(unittest.TestCase):
    @classmethod
    def setUpClass(cls):
        cls.members,cls.controls,cls.wiring=c.snapshots()
        cls.member=cls.members[0]
        cls.control=next(d for d in cls.controls if d['intent_completeness']=='qualified')
    def member_errors(self,row):return c.member_errors([row],{self.member['symbol_id']:c.SOURCE[self.member['symbol_id']]})
    def control_errors(self,row):return c.control_errors([row],{self.control['control_id']:c.CONTROLS[self.control['control_id']]})
    def test_exact_scope(self):
        self.assertEqual([148,94],[len(g['records']) for g in c.GROUPS])
        self.assertEqual([93,84],[len(g['controls']) for g in c.GROUPS])
        self.assertEqual(242,len(self.members));self.assertEqual(177,len(self.controls))
        self.assertEqual(set(c.SOURCE),{d['symbol_id'] for d in self.members})
        self.assertEqual(set(c.CONTROLS),{d['control_id'] for d in self.controls})
    def test_global_source_schema_fixed(self):
        before,after=c.source_headers()
        self.assertEqual(81,len(before));self.assertEqual([],c.schema_errors(before,after))
        self.assertTrue(c.schema_errors(before,before+['review_step']))
        self.assertTrue(c.schema_errors(before,list(reversed(before))))
    def test_member_ordinary(self):self.assertEqual([],self.member_errors(self.member))
    def test_missing_duplicate_foreign_members(self):
        expected={self.member['symbol_id']:c.SOURCE[self.member['symbol_id']]}
        self.assertTrue(c.member_errors([],expected))
        self.assertTrue(c.member_errors([self.member,self.member],expected))
        row=copy.deepcopy(self.member);row['symbol_id']='foreign';self.assertTrue(c.member_errors([row],expected))
    def test_stale_utf16_body_profile_file(self):
        for k in ('source_span_start','source_span_length','source_profile','source_kind','source_enclosing_symbol_id','source_excerpt','source_sha256','body_sha256','source_body_start_line'):
            row=copy.deepcopy(self.member);row[k]='stale';self.assertTrue(self.member_errors(row),k)
    def test_missing_comparison_and_bad_target(self):
        row=copy.deepcopy(self.member);row['compared_portable_declarations']='[]';self.assertTrue(self.member_errors(row))
        row=copy.deepcopy(self.member);pins=json.loads(row['compared_portable_declarations']);pins[0]['body_sha256']='0'*64;row['compared_portable_declarations']=json.dumps(pins);self.assertTrue(self.member_errors(row))
    def test_invented_whole_method_equivalence(self):
        row=copy.deepcopy(self.member);row['equivalence_scope']='whole-method';self.assertTrue(self.member_errors(row))
    def test_wrong_group_and_unapproved_batch(self):
        for key,value in [('review_group','Z'),('future_batch','04A.75')]:
            row=copy.deepcopy(self.member);row[key]=value;self.assertTrue(self.member_errors(row))
    def test_control_ordinary(self):self.assertEqual([],self.control_errors(self.control))
    def test_missing_duplicate_foreign_controls(self):
        expected={self.control['control_id']:c.CONTROLS[self.control['control_id']]}
        self.assertTrue(c.control_errors([],expected));self.assertTrue(c.control_errors([self.control,self.control],expected))
        row=copy.deepcopy(self.control);row['control_id']='foreign';self.assertTrue(c.control_errors([row],expected))
    def test_default_pin_drift(self):
        row=copy.deepcopy(self.control);pin=json.loads(row['source_control_pin']);pin['initialization_defaults']='Checked=True';row['source_control_pin']=json.dumps(pin);self.assertTrue(self.control_errors(row))
        context=c.a.read(c.EVIDENCE/'control-context.json')[0];expected={context['control_id']:c.CONTROLS[context['control_id']]}
        self.assertEqual([],c.context_errors([context],expected))
        for key in ('parent_invocations','designer_configuration_edges'):
            bad=copy.deepcopy(context);bad[key]=[];self.assertTrue(c.context_errors([bad],expected))
    def test_unpinned_constructor_handler_outcome(self):
        for k in ('required_constructor_initialization_ids','source_witnesses','control_flow_pins'):
            row=copy.deepcopy(self.control);row[k]='[]';self.assertTrue(self.control_errors(row),k)
    def test_dynamic_edge_removed_or_changed(self):
        row=copy.deepcopy(self.control);row['binding_edge_pins']='[]';self.assertTrue(self.control_errors(row))
        row=copy.deepcopy(self.control);pins=json.loads(row['binding_edge_pins']);pins[0]['target']='invented';row['binding_edge_pins']=json.dumps(pins);self.assertTrue(self.control_errors(row))
    def test_missing_cancel_close_lifecycle(self):
        row=copy.deepcopy(self.control);row['form_lifecycle_method_ids']='[]';self.assertTrue(self.control_errors(row))
    def test_unsupported_complete_or_unowned_deferred(self):
        row=copy.deepcopy(self.control);row['unsupported_contract_field']='native lifetime';self.assertTrue(self.control_errors(row))
        row=copy.deepcopy(self.control);row['intent_completeness']='deferred';self.assertTrue(self.control_errors(row))
    def test_same_size_historical_change(self):
        with tempfile.TemporaryDirectory() as tmp:
            root=pathlib.Path(tmp);p=root/'history.bin';p.write_bytes(b'abcd')
            rows=[dict(path=p.name,bytes=4,sha256=hashlib.sha256(b'abcd').hexdigest())]
            self.assertEqual([],c.checked_files(rows,root));p.write_bytes(b'abce');self.assertTrue(c.checked_files(rows,root))
    def test_missing_history_and_wrong_size(self):
        with tempfile.TemporaryDirectory() as tmp:
            root=pathlib.Path(tmp);row=dict(path='history',bytes=4,sha256=hashlib.sha256(b'abcd').hexdigest())
            self.assertTrue(c.checked_files([row],root));(root/'history').write_bytes(b'abcde');self.assertTrue(c.checked_files([row],root))
    def test_ordered_literal_objects(self):
        self.assertNotEqual(c.literals(b'[{"a":1},{"b":2}]'),c.literals(b'[{"b":2},{"a":1}]'))
        self.assertNotEqual(c.literals(b'[{"a":1}]'),c.literals(b'[{ "a":1}]'))
    def test_strict2_integrity1_and_eventual_ready(self):
        self.assertEqual(2,c.readiness_exit(1,0,0,0));self.assertEqual(2,c.readiness_exit(0,1,0,0))
        self.assertEqual(2,c.readiness_exit(0,0,1,0));self.assertEqual(2,c.readiness_exit(0,0,0,1))
        self.assertEqual(1,c.readiness_exit(1,1,1,1,['broken history']))
        self.assertEqual(0,c.readiness_exit(0,0,0,0))
    def test_prior_deferred_object_loss_mutation_and_order(self):
        old=b'[{"control_id":"old","intent_completeness":"deferred"},{"control_id":"full","review_contract":"04A.01-control-v1"}]'
        self.assertTrue(c.literal_prefix_matches(old,old,2))
        for changed in (b'[{"control_id":"full","review_contract":"04A.01-control-v1"}]',old.replace(b'deferred',b'complete'),old.replace(b'"old"',b'"bad"')):
            self.assertFalse(c.literal_prefix_matches(old,changed,2))
    def test_checkpoint_loss_duplicate_and_foreign(self):
        rows=c.a.read(c.EVIDENCE/'group-checkpoints.json')
        self.assertEqual([],c.checkpoint_errors(rows))
        self.assertTrue(c.checkpoint_errors(rows[1:]));self.assertTrue(c.checkpoint_errors(rows+[rows[0]]))
        rows=copy.deepcopy(rows);rows[0]['path']='foreign';self.assertTrue(c.checkpoint_errors(rows))
    def test_unpinned_entry_sidecar(self):
        row=copy.deepcopy(self.member);entry=json.loads(row['observed_entry_references']);entry['entry_sha256']='0'*64;row['observed_entry_references']=json.dumps(entry)
        self.assertTrue(self.member_errors(row))
    def test_lost_new_deferred_control_cannot_be_credit(self):
        d=next(x for x in self.controls if x['intent_completeness']=='deferred');expected={d['control_id']:c.CONTROLS[d['control_id']]}
        self.assertTrue(c.control_errors([],expected))
        bad=copy.deepcopy(d);bad.update(intent_completeness='qualified',review_contract='04A.01-control-v1',defaults_contract='present',enabled_reset_contract='present',validation_output_contract='present',option_contract='present')
        self.assertTrue(c.control_errors([bad],expected))

if __name__=='__main__':unittest.main(verbosity=2)
