"""C29 adversarial sidecar checks. Isolated copies only; no product mutation."""
import copy,json,pathlib,shutil,tempfile,unittest
import continuation29_report as c
class Continuation29Tests(unittest.TestCase):
    @classmethod
    def setUpClass(cls):
        cls.rs=c.rows();cls.f=c.frozen();cls.ctx={p['edge_id']:p for p in c.read(c.H/'edge-contexts.json')}
    def check(self,rs,f=None,root=c.R):return c.record_errors(rs,self.f if f is None else f,self.ctx,root)
    def one(self,category=None,tier=None):
        r=copy.deepcopy(next(r for r in self.rs if (category is None or r['intent_category']==category) and (tier is None or tier in r['risk_tiers'])));return r,{r['edge_id']:self.f[r['edge_id']]}
    def test_29_exact622_and_six_disjoint_groups(self):
        self.assertEqual(622,len(self.rs));self.assertEqual([],self.check(self.rs));self.assertEqual(dict(zip('ABCDEF',(174,138,90,69,79,72))),c.COUNTS)
    def test_29_missing_edge(self):self.assertTrue(self.check(self.rs[:-1]))
    def test_29_duplicate_edge(self):self.assertTrue(self.check(self.rs+[self.rs[0]]))
    def test_29_foreign_edge(self):
        r,f=self.one();r['edge_id']='foreign';self.assertTrue(self.check([r],f))
    def test_29_misassigned_group(self):
        r,f=self.one();r['group']='F' if r['group']!='F' else 'A';self.assertTrue(self.check([r],f))
    def test_29_stale_declaration_owner_status_source(self):
        for key in ('declaration_line','declaration_text','source_sha256','owner_id','original_target_status','original_row_sha256'):
            r,f=self.one();r[key]='stale';self.assertTrue(self.check([r],f),key)
    def test_29_body_utf16_span_pin_drift(self):
        r,f=self.one();r['member_pins'][0]['span_start']='0';self.assertTrue(self.check([r],f))
    def test_29_type_only_classification_rejected(self):
        r,f=self.one();r['classification_basis']='WithEvents type';self.assertTrue(self.check([r],f))
        r,f=self.one();r['intent_witnesses']=[dict(text=r['declaration_text'])];self.assertTrue(self.check([r],f))
    def test_29_named_category_cannot_be_inferred_instead(self):
        r,f=self.one('value-input-consumed-by-compute');r['intent_category']='display-only/inert';self.assertTrue(self.check([r],f))
    def test_29_dynamic_handler_needs_actual_registration(self):
        r,f=self.one('delegate-dynamically-wired');self.assertEqual([],self.check([r],f));r['dynamic_registration_witnesses']=[];self.assertTrue(self.check([r],f))
    def test_29_unsubstantiated_full_handler_qualification(self):
        r,f=self.one();r['full_edge_contract_qualified']=True;self.assertTrue(self.check([r],f))
    def test_29_control_v1_not_promoted(self):
        r,f=self.one();r['control_contract_upgraded']=True;self.assertTrue(self.check([r],f))
    def test_29_invented_control_mapping(self):
        r,f=self.one();r['control_ids']=['invented-one-edge-per-control'];self.assertTrue(self.check([r],f))
    def test_29_no_risk_owner_fixture_exceptions(self):
        for key in ('risk_tiers','owner_roles','approved_target_batches','named_test_fixture','safe_capture_preconditions','owner_action'):
            r,f=self.one(tier='P0');r[key]=[] if isinstance(r[key],list) else '';self.assertTrue(self.check([r],f),key)
    def test_29_absent_C28_risk_crosswalk(self):
        r,f=self.one(tier='P0');r['C28_risks']=[];self.assertTrue(self.check([r],f))
    def test_29_unsupported_risk_downgrade(self):
        r,f=self.one(tier='P0');r['risk_tiers']=['P3'];r['C28_risks'][0]['risk_tier']='P3';self.assertTrue(self.check([r],f))
    def test_29_unknown_cannot_be_closed(self):
        r,f=self.one(tier='UNKNOWN_HIGH');self.assertEqual([],self.check([r],f));r.update(source_intent_classification='source-supported',review_outcome='source-resolved-intent',affected_live_golden_blocked=False);self.assertTrue(self.check([r],f))
    def test_29_false_golden_authorization(self):
        r,f=self.one();r['capture_authorized']=True;self.assertTrue(self.check([r],f))
        r,f=self.one(tier='P0');r['affected_live_golden_blocked']=False;self.assertTrue(self.check([r],f))
    def test_29_false_runtime_and_owner_approval(self):
        for key,value in [('runtime_verification','PASS'),('owner_approval','ACCEPTED'),('strict_exit',0)]:
            r,f=self.one();r[key]=value;self.assertTrue(self.check([r],f))
    def test_29_unapproved_continuation_or_closure_batch(self):
        for batch in ('C30','04A.29','04A.48','04A.99'):
            r,f=self.one(tier='P0');r['approved_target_batches']=[batch];self.assertTrue(self.check([r],f))
    def test_29_literal_prefix_mutation_and_reordering(self):
        self.assertEqual([],c.literal_errors(b'[{"a":1},{"b":2}]',b'[{"a":1},{"b":2}]'))
        self.assertTrue(c.literal_errors(b'[{"a":1},{"b":2}]',b'[{"b":2},{"a":1}]'))
        self.assertTrue(c.literal_errors(b'[{"a":1}]',b'[{"a":2}]'))
    def test_29_same_size_actual_source_corruption(self):
        r,f=self.one()
        with tempfile.TemporaryDirectory(prefix='beshstat-c29-source-') as directory:
            root=pathlib.Path(directory);p=root/r['path'];p.parent.mkdir(parents=True);raw=(c.R/r['path']).read_bytes();p.write_bytes(b'X'+raw[1:]);self.assertTrue(self.check([r],f,root))
    def test_29_same_size_history_and_missing_artifact(self):
        with tempfile.TemporaryDirectory(prefix='beshstat-c29-evidence-') as directory:
            root=pathlib.Path(directory);p=root/'old.log';p.write_bytes(b'old history\n');entries=[dict(path='old.log',bytes=p.stat().st_size,sha256=c.sha(p))];self.assertEqual([],c.artifacts_errors(entries,root));p.write_bytes(b'bad history\n');self.assertTrue(c.artifacts_errors(entries,root));p.unlink();self.assertTrue(c.artifacts_errors(entries,root))
    def test_29_out_of_scope_product_write(self):
        self.assertFalse(c.allowed_write('BESHstatNG/src/UI/UibyID.vb'));self.assertFalse(c.allowed_write('BESHStat.Core/src/RegModels/GLM.vb'));self.assertFalse(c.allowed_write('TestResults/04A-01-continuation-28/final-facts.json'));self.assertTrue(c.allowed_write('scripts/windows-migration-audit/continuation29_report.py'))
    def test_29_parent_mode_matrix_loss(self):
        r,f=self.one();self.assertEqual('BESHStatNG.UibyID',r['containing_type']);r['UibyID_mode_records']=r['UibyID_mode_records'][:-1];self.assertTrue(self.check([r],f))
    def test_29_refedit_requires_inherited_lifetime_pins(self):
        r,f=self.one('host-refedit/selection-owned');r['inherited_refedit_member_pins']=[];self.assertTrue(self.check([r],f))
    def test_29_member_schema_stays81(self):self.assertEqual(81,len(c.catalog.load(c.L,'symbols.csv')[0]))
if __name__=='__main__':unittest.main(verbosity=2)
