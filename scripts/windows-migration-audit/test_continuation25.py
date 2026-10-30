import copy,hashlib,json,pathlib,tempfile,unittest
import continuation25_report as c
class Continuation25Tests(unittest.TestCase):
    @classmethod
    def setUpClass(cls):
        cls.g=c.a.read(c.H/'targets.json')[0];cls.f=c.a.read(c.H/'group-A-frozen-source.json')
        cls.m,cls.controls,_=c.snapshots('A');cls.member=cls.m[0]
        cls.control=next(x for x in cls.controls if x['intent_completeness']=='qualified')
        cls.deferred=next(x for x in cls.controls if x['intent_completeness']=='deferred')
        cls.entries=c.a.read(c.H/'group-A-entry-evidence.json');cls.contexts=c.a.read(c.H/'group-A-control-context.json')
    def member_errors(self,d):
        sid=d.get('symbol_id');s=next(x for x in self.g['records'] if x['symbol_id']==self.member['symbol_id'])
        return c.member_errors([d],{s['symbol_id']:s},{x['symbol_id']:x for x in self.f['members']},self.entries)
    def control_errors(self,d):
        expected={x['control_id']:x for x in self.g['controls'] if x['control_id']==d['control_id']}
        return c.control_errors([d],expected,self.contexts,self.f)
    def test_ordinary_member(self):self.assertEqual([],self.member_errors(self.member))
    def test_exact_missing_duplicate_foreign_ids(self):
        for rows in ([],[{'id':'a'},{'id':'a'}],[{'id':'b'}]):self.assertTrue(c.exact_ids(rows,{'a'},'id'))
    def test_body_span_profile_drift(self):
        for k in ('body_sha256','source_span_start','source_profile'):
            d=copy.deepcopy(self.member);d[k]='changed';self.assertTrue(self.member_errors(d))
    def test_source_excerpt_drift(self):
        d=copy.deepcopy(self.member);d['source_excerpt']='changed';self.assertTrue(self.member_errors(d))
    def test_source_file_pin_drift(self):
        d=copy.deepcopy(self.member);d['source_sha256']='0'*64;self.assertTrue(self.member_errors(d))
    def test_missing_portable_declarations(self):
        d=copy.deepcopy(self.member);d['compared_portable_declarations']='[]';self.assertTrue(self.member_errors(d))
    def test_changed_portable_body(self):
        d=copy.deepcopy(self.member);pins=json.loads(d['compared_portable_declarations']);pins[0]['body_sha256']='0'*64;d['compared_portable_declarations']=json.dumps(pins);self.assertTrue(self.member_errors(d))
    def test_no_invented_whole_equivalence(self):
        d=copy.deepcopy(self.member);d['equivalence_scope']='whole-method';self.assertTrue(self.member_errors(d))
    def test_unapproved_batch_and_export(self):
        for k,v in [('future_batch','04A.75'),('registration_class','export')]:
            d=copy.deepcopy(self.member);d[k]=v;self.assertTrue(self.member_errors(d))
    def test_unpinned_caller(self):
        d=copy.deepcopy(self.member);p=json.loads(d['observed_entry_references']);p['entry_sha256']='0'*64;d['observed_entry_references']=json.dumps(p);self.assertTrue(self.member_errors(d))
    def test_ordinary_control(self):self.assertEqual([],self.control_errors(self.control))
    def test_ordinary_deferred_control(self):self.assertEqual([],self.control_errors(self.deferred))
    def test_stale_default(self):
        d=copy.deepcopy(self.control);p=json.loads(d['source_control_pin']);p['initialization_defaults']='invented';d['source_control_pin']=json.dumps(p);self.assertTrue(self.control_errors(d))
    def test_missing_cross_group_dependencies(self):
        for k in ('control_flow_pins','required_constructor_initialization_ids'):
            d=copy.deepcopy(self.control);d[k]='[]';self.assertTrue(self.control_errors(d))
    def test_stale_context_or_edges(self):
        d=copy.deepcopy(self.control);p=json.loads(d['source_control_references']);p['entry_sha256']='0'*64;d['source_control_references']=json.dumps(p);self.assertTrue(self.control_errors(d))
    def test_deferred_not_complete(self):
        d=copy.deepcopy(self.deferred);d.update(intent_completeness='qualified',review_contract='04A.01-control-v1',defaults_contract='present',enabled_reset_contract='present',validation_output_contract='present',option_contract='present');self.assertTrue(self.control_errors(d))
    def test_unowned_deferral(self):
        d=copy.deepcopy(self.deferred);d['unsupported_contract_field']='';self.assertTrue(self.control_errors(d))
    def test_literal_prefix_bytes_and_order(self):
        old=b'[{"a":1},{"b":2}]';self.assertEqual([],c.prefix_errors(old,b'[{"a":1},{"b":2},{"c":3}]',2))
        for new in (b'[{ "a":1},{"b":2}]',b'[{"b":2},{"a":1}]'):self.assertTrue(c.prefix_errors(old,new,2))
    def test_same_size_history_corruption(self):
        with tempfile.TemporaryDirectory() as tmp:
            root=pathlib.Path(tmp);p=root/'old';p.write_bytes(b'abcd');rows=[dict(path='old',bytes=4,sha256=hashlib.sha256(b'abcd').hexdigest())]
            self.assertEqual([],c.checked_files(rows,root));p.write_bytes(b'abce');self.assertTrue(c.checked_files(rows,root))
    def test_missing_or_size_changed_history(self):
        with tempfile.TemporaryDirectory() as tmp:
            root=pathlib.Path(tmp);row=dict(path='x',bytes=4,sha256=hashlib.sha256(b'abcd').hexdigest());self.assertTrue(c.checked_files([row],root));(root/'x').write_bytes(b'abcde');self.assertTrue(c.checked_files([row],root))
    def test_schema_is_exact_81_columns(self):
        before=list(map(str,range(81)));self.assertEqual([],c.schema_errors(before,before));self.assertTrue(c.schema_errors(before,before+['extra']));self.assertTrue(c.schema_errors(before,before[::-1]))
    def test_strict_incomplete_vs_integrity(self):
        for values in [(1,0,0,0),(0,1,0,0),(0,0,1,0),(0,0,0,1)]:self.assertEqual(2,c.readiness_exit(*values))
        self.assertEqual(1,c.readiness_exit(1,1,1,1,['broken']));self.assertEqual(0,c.readiness_exit(0,0,0,0))
    def test_partition_counts_and_no_foreign_forms(self):
        groups=c.a.read(c.H/'targets.json');self.assertEqual([(42,74),(41,73),(56,70),(56,69),(56,69)],[(len(g['records']),len(g['controls'])) for g in groups])
        self.assertEqual(251,len({s['symbol_id'] for g in groups for s in g['records']}));self.assertEqual(355,len({s['control_id'] for g in groups for s in g['controls']}))
        self.assertTrue(all('Ui11PCA' in s['path'] or 'Ui21ControlCharts' in s['path'] for g in groups for s in g['records']))
    def test_all_scoped_edges_once(self):
        gs=c.a.read(c.H/'targets.json');ids=[i for g in gs for i in g['edge_ids']];self.assertEqual(len(ids),len(set(ids)));self.assertEqual(3284,len(ids))
    def test_individual_adjudication_membership(self):
        rows=[r for r in c.a.read(c.H/'qualified-control-adjudications.json') if r['control_id']==self.control['control_id']]
        self.assertEqual([],c.adjudication_errors(rows,[self.control]));self.assertTrue(c.adjudication_errors([],[self.control]));self.assertTrue(c.adjudication_errors(rows+rows,[self.control]))
    def test_individual_adjudication_binds_exact_decision(self):
        rows=[r for r in c.a.read(c.H/'qualified-control-adjudications.json') if r['control_id']==self.control['control_id']]
        rows[0]['decision_sha256']='0'*64;self.assertTrue(c.adjudication_errors(rows,[self.control]))
    def test_individual_adjudication_requires_state_parent_responsibility(self):
        for field in ('individual_contract','display_and_state_source','parent'):
            rows=[r for r in c.a.read(c.H/'qualified-control-adjudications.json') if r['control_id']==self.control['control_id']]
            rows[0][field]='';self.assertTrue(c.adjudication_errors(rows,[self.control]))
if __name__=='__main__':unittest.main(verbosity=2)
