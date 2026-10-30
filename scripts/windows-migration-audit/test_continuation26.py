import copy,hashlib,json,pathlib,tempfile,unittest
import continuation26_report as c
class Continuation26Tests(unittest.TestCase):
    @classmethod
    def setUpClass(cls):
        cls.groups=c.a.read(c.H/'targets.json');cls.g=cls.groups[0];cls.f=c.a.read(c.H/'group-A-frozen-source.json');cls.m,cls.controls=c.snapshots('A')
        cls.entries=c.a.read(c.H/'group-A-entry-evidence.json');cls.contexts=c.a.read(c.H/'group-A-control-context.json')
    def test_actual_partition_175_182(self):self.assertEqual([],c.target_errors(self.groups))
    def test_missing_duplicate_foreign_member_ids(self):
        for mode in ('missing','duplicate','foreign'):
            gs=copy.deepcopy(self.groups)
            if mode=='missing':gs[0]['records'].pop()
            elif mode=='duplicate':gs[0]['records'][0]=gs[0]['records'][1]
            else:gs[0]['records'][0]['path']='BESHstatNG/src/UI/UibyID.Designer.vb'
            self.assertTrue(c.target_errors(gs))
    def test_erroneous_182_control_set(self):
        for mode in ('missing','duplicate','foreign'):
            gs=copy.deepcopy(self.groups)
            if mode=='missing':gs[0]['controls'].pop()
            elif mode=='duplicate':gs[0]['controls'][0]=gs[0]['controls'][1]
            else:gs[0]['controls'][0]['path']='BESHstatNG/src/UI/UibyID.Designer.vb'
            self.assertTrue(c.target_errors(gs))
    def member_errors(self,d):
        s=next(x for x in self.g['records'] if x['symbol_id']==self.m[0]['symbol_id'])
        return c.member_errors([d],{s['symbol_id']:s},{s['symbol_id']:s for s in self.f['members']},self.entries)
    def test_actual_member_body(self):self.assertEqual([],self.member_errors(self.m[0]))
    def test_stale_body_span_and_comparison(self):
        for key in ('body_sha256','source_span_start','source_sha256','source_excerpt'):
            d=copy.deepcopy(self.m[0]);d[key]='stale';self.assertTrue(self.member_errors(d))
    def test_missing_and_fabricated_equivalence(self):
        for key,value in [('compared_portable_declarations','[]'),('equivalence_scope','whole-method')]:
            d=copy.deepcopy(self.m[0]);d[key]=value;self.assertTrue(self.member_errors(d))
    def test_actual_deferred_control(self):
        d=self.controls[0];self.assertEqual([],c.control_admission([d],{s['control_id']:s for s in self.g['controls'] if s['control_id']==d['control_id']},self.contexts,self.f))
    def test_fabricated_control_promotion(self):
        d=copy.deepcopy(self.controls[0]);d.update(intent_completeness='qualified',review_contract='04A.01-control-v1',unsupported_contract_field='',defaults_contract='filled',enabled_reset_contract='filled',validation_output_contract='filled',option_contract='filled')
        self.assertTrue(c.control_admission([d],{s['control_id']:s for s in self.g['controls'] if s['control_id']==d['control_id']},self.contexts,self.f))
    def test_missing_dynamic_handler_sources(self):
        e=next(e for e in self.f['full_form_wiring'] if e['kind']=='AddHandler');d=dict(e,handler_source_ids='[]')
        self.assertTrue(c.wiring_errors([e],self.f['full_form_members'],[d]))
        d['handler_source_ids']=json.dumps([e['owner_id']]);self.assertEqual([],c.wiring_errors([e],self.f['full_form_members'],[d]))
    def test_literal_prefix_bytes_and_order(self):
        old=b'[{"a":1},{"b":2}]';self.assertEqual([],c.prefix_errors(old,b'[{"a":1},{"b":2},{"c":3}]',2))
        self.assertTrue(c.prefix_errors(old,b'[{ "a":1},{"b":2}]',2));self.assertTrue(c.prefix_errors(old,b'[{"b":2},{"a":1}]',2))
    def test_index_membership_and_same_size_hash_drift(self):
        with tempfile.TemporaryDirectory() as tmp:
            root=pathlib.Path(tmp);(root/'a').write_bytes(b'abcd');index=dict(artifacts=[dict(path='a',bytes=4,sha256=hashlib.sha256(b'abcd').hexdigest())])
            self.assertEqual([],c.index_errors(index,{'a'},root));self.assertTrue(c.index_errors(index,{'a','b'},root))
            duplicate=copy.deepcopy(index);duplicate['artifacts']*=2;self.assertTrue(c.index_errors(duplicate,{'a'},root))
            (root/'a').write_bytes(b'abce');self.assertTrue(c.index_errors(index,{'a'},root))
    def test_schema_stays_exact_81(self):
        columns=list(map(str,range(81)));self.assertEqual([],c.schema_errors(columns,columns));self.assertTrue(c.schema_errors(columns,columns[::-1]));self.assertTrue(c.schema_errors(columns,columns+['extra']))
if __name__=='__main__':unittest.main(verbosity=2)
