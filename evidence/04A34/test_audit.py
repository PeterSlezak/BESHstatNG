"""Independent packet, lineage and negative tamper tests. No product/native launch."""
from pathlib import Path
import copy,csv,hashlib,json,unittest
from verify_manifest import ROOT,HERE,SELF,SOURCES,verify_manifest,validate_rows,expected_scope,validate_lineage,crosswalk_inputs,validate_crosswalk
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
    def test_seventeen_families_and_zero_open_ready_preconditions(self):
        v=load('family-coverage.json')
        self.assertEqual(17,len(v['families']))
        self.assertEqual(17,len({r['family'] for r in v['families']}))
        self.assertEqual(768,v['historical_candidate_count'])
        self.assertEqual(0,v['open_blockers']);self.assertEqual(0,v['historical_open_blockers']);self.assertTrue(all(r['complete'] for r in v['families']))
        self.assertEqual('READY_FOR_OWNER_04A34_ACCEPTANCE_REVIEW',v['status'])
        self.assertIn('READY_FOR_OWNER_04A34_ACCEPTANCE_REVIEW',(ROOT/'docs/architecture/batch-04A-34-owner-review.md').read_text(encoding='utf-8'))
    def test_each_candidate_has_one_disposition(self):
        with (HERE/'operation-map-final.csv').open(encoding='utf-8-sig') as stream:rows=list(csv.DictReader(stream))
        self.assertEqual(768,len(rows));self.assertEqual(768,len({r['historical_member_id'] for r in rows}))
        allowed={'PORTED_AND_CONSUMED','ALREADY_PORTABLE_REUSED','HOST_ONLY','LATER_BATCH_RESERVED','HOST_ONLY'}
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
        self.assertGreater(v['new_managed_cases_per_configuration'],105)
        for r in v['focused']:
            self.assertEqual(0,r['exit_code'])
            self.assertEqual({'Passed'},set(r['outcomes']))
    def test_unchanged_gates(self):
        self.assertEqual(dict(P='NO',N1='NO GO',N2='NO',C2='UNMET',C3='UNAPPROVED'),load('entry-authority.json')['gates'])
        self.assertEqual('NATIVE_NOT_RUN',load('family-coverage.json')['native'])
        self.assertEqual('NOT_GOLDEN',load('family-coverage.json')['scientific'])
    def test_claims_match_receipts(self):
        v=load('validation.json');d=(ROOT/'docs/architecture/batch-04A-34-owner-review.md').read_text(encoding='utf-8')
        self.assertEqual('READY_FOR_OWNER_04A34_ACCEPTANCE_REVIEW',v['decision'])
        self.assertIn(str(v['new_managed_cases_per_configuration'])+' new raw',d)
        self.assertIn('zero current OPEN',d)
        self.assertEqual('PASS',load('privacy-check.json')['status'])

    def test_exact_crosswalk_and_seven_explicit_relocations(self):validate_crosswalk(*crosswalk_inputs())
    def test_dropped_historical_candidate_rejected(self):
        data=list(crosswalk_inputs());data[1].pop()
        with self.assertRaises(AssertionError):validate_crosswalk(*data)
    def test_fabricated_relocation_target_rejected(self):
        data=list(crosswalk_inputs());data[5]['relocations'][0]['successor']='fake'
        with self.assertRaises((AssertionError,KeyError)):validate_crosswalk(*data)
    def test_open_member_disguised_as_ready_rejected(self):
        data=list(crosswalk_inputs());data[1][0]['disposition']='OPEN_BLOCKER'
        with self.assertRaises(AssertionError):validate_crosswalk(*data)
    def test_disconnected_port_rejected(self):
        data=list(crosswalk_inputs());r=next(r for r in data[5]['members'] if r['disposition']=='PORTED_AND_CONSUMED');r['portable_edges']=[]
        with self.assertRaises(AssertionError):validate_crosswalk(*data)
    def test_same_named_method_wrong_owner_rejected(self):
        data=list(crosswalk_inputs());edge=next(e for r in data[5]['members'] for e in r['portable_edges'] if e['expression']=='SankeyMeasuredGeometryCore.MapX')
        wrong=next(n for n in data[4]['current'] if n['path'].endswith('BulletMeasuredGeometryCore.vb') and n['name']=='MapX')
        edge.update(target=wrong['key']+'@'+str(wrong['source_span_start'])+':'+str(wrong['source_span_length']),target_path=wrong['path'],target_span=[wrong['start'],wrong['end']])
        with self.assertRaises(AssertionError):validate_crosswalk(*data)
    def test_no_false_same_file_relocation(self):
        data=list(crosswalk_inputs());next(r for r in data[5]['relocations'] if 'ca14220ee65e6934' in r['historical_member_id'])['same_file_header_match']=True
        with self.assertRaises(AssertionError):validate_crosswalk(*data)
    def test_both_configurations_all_frozen_corpora(self):
        v=load('completion/capture-comparison.json');self.assertEqual('PASS',v['status']);self.assertEqual(14,len(v['comparisons']))
        self.assertEqual({'Debug','Release'},{r['configuration'] for r in v['comparisons']})
        for r in v['comparisons']:
            b=(ROOT/r['pre_edit']).read_bytes();self.assertEqual(r['pre_sha256'],hashlib.sha256(b).hexdigest());self.assertEqual(r['pre_sha256'],r['post_sha256']);self.assertTrue(r['byte_equal'])
        self.assertTrue(load('completion/frozen-corpus-preservation.json')['root_corpora_unchanged'])
    def test_renderer_boundaries_and_measurement_order(self):
        v=load('completion/renderer-preservation.json');self.assertEqual('PASS',v['status']);self.assertEqual(0,v['unqualified_deltas']);self.assertEqual(6,v['qualified_deltas'])
        self.assertTrue(all(r['native_boundary_status'].startswith('PASS') for r in v['members']))
        self.assertTrue(load('source-ownership.json')['windows_headers_preserved'])
        self.assertTrue(all('Option Strict On' in (ROOT/r['source']['path']).read_text(encoding='utf-8-sig') for r in load('coordinate-and-chart-spec-contract.json')['contracts']))
    def test_final_identity_counts_and_inherited_failures(self):
        v=load('validation.json');self.assertFalse(v['unclassified_new_managed_nonpasses']);self.assertFalse(v['unclassified_new_python_nonpasses'])
        self.assertEqual({'strict':1,'profile':1,'docs':0},v['strict_profile_mkdocs'])
        for c in ['Debug','Release']:
            self.assertEqual(14,v['managed_totals'][c]['Failed'])
            self.assertEqual(20145+v['new_managed_cases_per_configuration'],v['managed_totals'][c]['Passed'])
        self.assertEqual({'total':916,'passed':916,'failed':0},v['node'])
        with (HERE/'managed-test-identities-final.csv').open(encoding='utf-8-sig') as f:rows=list(csv.DictReader(f))
        self.assertEqual(v['new_managed_cases_per_configuration'],sum(r['current_main_debug']=='NEW_04A34' for r in rows))
        partial=load('completion/checkpoint/validation.json');self.assertEqual(105,partial['new_managed_cases_per_configuration'])

if __name__=='__main__':unittest.main()
