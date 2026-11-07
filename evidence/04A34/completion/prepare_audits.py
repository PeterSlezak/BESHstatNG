"""Strengthen this batch's partial-packet audits after actual scope reconciliation."""
from work import *
p=HERE.parent/'test_audit.py';s=p.read_text(encoding='utf-8')
archive=HERE/'checkpoint/test_audit.py'
if not archive.exists():write(archive.relative_to(ROOT),s)
s=s.replace('validate_lineage\n','validate_lineage,crosswalk_inputs,validate_crosswalk\n')
s=s.replace('test_seventeen_families_and_no_false_ready','test_seventeen_families_and_zero_open_ready_preconditions')
s=s.replace("self.assertGreater(v['open_blockers'],0)","self.assertEqual(0,v['open_blockers']);self.assertEqual(0,v['historical_open_blockers']);self.assertTrue(all(r['complete'] for r in v['families']))")
s=s.replace("'NOT_READY_04A34'","'READY_FOR_OWNER_04A34_ACCEPTANCE_REVIEW'")
s=s.replace("self.assertEqual(105,v['new_managed_cases_per_configuration'])","self.assertGreater(v['new_managed_cases_per_configuration'],105)")
s=s.replace("'OPEN_BLOCKER'", "'HOST_ONLY'") # remove OPEN from the allowed set, retaining the historical test structure
s=s.replace("str(load('family-coverage.json')['open_blockers'])+' current candidate'","'zero current OPEN'")
extra='''
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
'''
s=s.replace("if __name__=='__main__':unittest.main()",extra+"\nif __name__=='__main__':unittest.main()")
write(p.relative_to(ROOT),s)
print('Prepared stronger READY preconditions and negative crosswalk tamper audits')
