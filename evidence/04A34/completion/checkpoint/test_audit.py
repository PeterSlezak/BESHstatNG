"""Independent packet, lineage and negative tamper tests. No product/native launch."""
from pathlib import Path
import copy,csv,hashlib,json,unittest
from verify_manifest import ROOT,HERE,SELF,SOURCES,verify_manifest,validate_rows,expected_scope,validate_lineage
def load(name):return json.loads((HERE/name).read_bytes())
class Publication34Tests(unittest.TestCase):
    def rows(self):return copy.deepcopy(load('publication-manifest.json')['files'])
    def reject(self,mutate):
        rows=self.rows();mutate(rows)
        with self.assertRaises(AssertionError):validate_rows(rows,expected_scope())
    def test_full_independent_packet(self):self.assertGreater(verify_manifest(),50)
    def test_missing_entry_rejected(self):self.reject(lambda r:r.pop(0))
    def test_duplicate_entry_rejected(self):self.reject(lambda r:r.append(r[0]))
    def test_case_alias_rejected(self):self.reject(lambda r:r.append(dict(r[0],path=r[0]['path'].upper())))
    def test_wrong_size_rejected(self):self.reject(lambda r:r[0].update(bytes=r[0]['bytes']+1))
    def test_wrong_hash_rejected(self):self.reject(lambda r:r[0].update(sha256='0'*64))
    def test_traversal_rejected(self):self.reject(lambda r:r[0].update(path='../outside'))
    def test_absolute_path_rejected(self):self.reject(lambda r:r[0].update(path='/outside'))
    def test_backslash_rejected(self):self.reject(lambda r:r[0].update(path=r[0]['path'].replace('/','\\')))
    def test_recursive_self_rejected(self):self.reject(lambda r:next(x for x in r if x['path']==SELF).update(bytes=1,sha256='0'*64))
    def test_foreign_entry_rejected(self):self.reject(lambda r:r.append(dict(path='planning/BESHStatNG_CrossPlatform_MasterPlan.md',bytes=1,sha256='0'*64)))
    def test_sources_and_transitions_exact(self):
        paths={r['path'] for r in load('source-transitions.json')['files']}
        self.assertEqual(set(SOURCES),paths)
        self.assertTrue(paths<={r['path'] for r in self.rows()})
    def test_seventeen_families_and_no_false_ready(self):
        v=load('family-coverage.json')
        self.assertEqual(17,len(v['families']))
        self.assertEqual(17,len({r['family'] for r in v['families']}))
        self.assertEqual(768,v['historical_candidate_count'])
        self.assertGreater(v['open_blockers'],0)
        self.assertEqual('NOT_READY_04A34',v['status'])
        self.assertIn('NOT_READY_04A34',(ROOT/'docs/architecture/batch-04A-34-owner-review.md').read_text(encoding='utf-8'))
    def test_each_candidate_has_one_disposition(self):
        with (HERE/'operation-map-final.csv').open(encoding='utf-8-sig') as stream:rows=list(csv.DictReader(stream))
        self.assertEqual(768,len(rows));self.assertEqual(768,len({r['historical_member_id'] for r in rows}))
        allowed={'PORTED_AND_CONSUMED','ALREADY_PORTABLE_REUSED','HOST_ONLY','LATER_BATCH_RESERVED','OPEN_BLOCKER'}
        self.assertTrue(all(r['disposition'] in allowed and r['reason'] for r in rows))
    def test_current_members_are_not_historical_counts(self):
        with (HERE/'family-ownership-map.csv').open(encoding='utf-8-sig') as stream:rows=list(csv.DictReader(stream))
        self.assertEqual(load('family-coverage.json')['current_distinct_candidate_count'],len(rows))
        self.assertEqual(len(rows),len({r['current_member_id'] for r in rows}))
    def test_278_pins_and_100_captures(self):
        lineage=load('lineage-check.json')
        validate_lineage(lineage)
        self.assertEqual(278,len(lineage['pins']))
        self.assertEqual(278,len({r['path'] for r in lineage['pins']}))
        self.assertEqual(100,load('mixed-model-captures-check.json')['count'])
        self.assertTrue(all(r['matches'] for r in load('mixed-model-captures-check.json')['files']))
    def test_lineage_source_transition_tamper_rejected(self):
        value=load('lineage-check.json');value['pins'][0]['final_04A34_sha256']='0'*64
        with self.assertRaises(AssertionError):validate_lineage(value)
    def test_original_pin_rewrite_rejected(self):
        value=load('lineage-check.json');value['pins'][0]['original_04A28_sha256']='0'*64
        with self.assertRaises(AssertionError):validate_lineage(value)
    def test_protected_source_and_existing_engines(self):
        p=load('source-preservation.json')
        self.assertEqual('PASS',p['status'])
        self.assertTrue(p['all_previous_tests_evidence_controls_unchanged'])
        self.assertTrue(p['all_04A30_33_core_engines_unchanged'])
        for r in load('source-transitions.json')['files']:
            self.assertEqual(r['sha256'],hashlib.sha256((ROOT/r['path']).read_bytes()).hexdigest())
    def test_actual_capture_matches_pre_edit(self):
        self.assertEqual((HERE/'pre-edit-compatibility-capture.csv').read_bytes(),(HERE/'post-edit-compatibility-capture.csv').read_bytes())
    def test_portable_and_real_adapter_receipts(self):
        v=load('validation.json')
        self.assertFalse(v['unclassified_new_managed_nonpasses'])
        self.assertTrue(all(r['removed']==0 and r['changed_baseline_outcomes']==0 for r in v['identity_deltas']))
        self.assertEqual(105,v['new_managed_cases_per_configuration'])
        for r in v['focused']:
            self.assertEqual(0,r['exit_code'])
            self.assertEqual({'Passed'},set(r['outcomes']))
    def test_unchanged_gates(self):
        self.assertEqual(dict(P='NO',N1='NO GO',N2='NO',C2='UNMET',C3='UNAPPROVED'),load('entry-authority.json')['gates'])
        self.assertEqual('NATIVE_NOT_RUN',load('family-coverage.json')['native'])
        self.assertEqual('NOT_GOLDEN',load('family-coverage.json')['scientific'])
    def test_claims_match_receipts(self):
        v=load('validation.json');d=(ROOT/'docs/architecture/batch-04A-34-owner-review.md').read_text(encoding='utf-8')
        self.assertEqual('NOT_READY_04A34',v['decision'])
        self.assertIn(str(v['new_managed_cases_per_configuration'])+' new raw',d)
        self.assertIn(str(load('family-coverage.json')['open_blockers'])+' current candidate',d)
        self.assertEqual('PASS',load('privacy-check.json')['status'])
if __name__=='__main__':unittest.main()
