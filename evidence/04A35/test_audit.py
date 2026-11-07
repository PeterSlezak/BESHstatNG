"""Independent packet integrity, ownership, preservation and negative tamper tests."""
import unittest,copy,json,csv,hashlib,re
from verify_packet import *
class Packet35Tests(unittest.TestCase):
    def rows(self):return copy.deepcopy(load('publication-manifest.json')['files'])
    def reject(self,mutate):
        rows=self.rows();mutate(rows)
        with self.assertRaises(AssertionError):validate_manifest(rows,scope())
    def test_full_independent_verifier(self):self.assertGreater(verify(),30)
    def test_missing(self):self.reject(lambda r:r.pop())
    def test_duplicate(self):self.reject(lambda r:r.append(r[0]))
    def test_case_alias(self):self.reject(lambda r:r.append(dict(r[0],path=r[0]['path'].upper())))
    def test_foreign(self):self.reject(lambda r:r.append(dict(path='planning/BESHStatNG_CrossPlatform_MasterPlan.md',bytes=1,sha256='0'*64)))
    def test_traversal(self):self.reject(lambda r:r[0].update(path='../escape'))
    def test_absolute(self):self.reject(lambda r:r[0].update(path='/escape'))
    def test_drive_path(self):self.reject(lambda r:r[0].update(path='Q:'+chr(47)+'escape'))
    def test_backslash(self):self.reject(lambda r:r[0].update(path='a\\b'))
    def test_size(self):self.reject(lambda r:r[0].update(bytes=r[0]['bytes']+1))
    def test_hash(self):self.reject(lambda r:r[0].update(sha256='0'*64))
    def test_recursive_self(self):self.reject(lambda r:next(x for x in r if x['path']==SELF).update(bytes=1,sha256='0'*64))
    def test_ownership_source_consumption(self):validate_ownership(*ownership_inputs())
    def test_missing_old_member(self):
        data=list(ownership_inputs());data[0].pop(0)
        with self.assertRaises(AssertionError):validate_ownership(*data)
    def test_fake_ported(self):
        data=list(ownership_inputs());r=next(r for r in data[0] if r['disposition']=='HOST_ONLY');r.update(disposition='PORTED_AND_CONSUMED',target='Fake35',call_site='Fake35')
        with self.assertRaises(AssertionError):validate_ownership(*data)
    def test_open_cannot_be_ready(self):
        data=list(ownership_inputs());data[0][0]['disposition']='OPEN_BLOCKER'
        with self.assertRaises(AssertionError):validate_ownership(*data)
    def test_disconnected_port(self):
        data=list(ownership_inputs());next(r for r in data[0] if r['disposition']=='PORTED_AND_CONSUMED')['call_site']=''
        with self.assertRaises(AssertionError):validate_ownership(*data)
    def test_unclosed_reservation(self):
        data=list(ownership_inputs());next(r for r in data[0] if r['member']=='BuildTitle')['reserved34_closed']='False'
        with self.assertRaises(AssertionError):validate_ownership(*data)
    def test_frozen_source_and_capture(self):
        a=load('pre-edit-capture-authority-v2.json')
        for key in ['capture','invalid_earlier_capture','inputs']:
            r=a[key];self.assertEqual(r,pin(r['path']))
        self.assertEqual(4480,a['rows'])
        for p,r in load('pre-edit-source-hashes.json')['sources'].items():
            source=HERE/'entry-source'/(p.rsplit('/',1)[-1]+'.txt')
            self.assertEqual(r['sha256'],hashlib.sha256(source.read_bytes()).hexdigest())
    def test_both_config_captures(self):
        c=load('compatibility-comparison.json');self.assertEqual('PASS',c['status']);self.assertEqual({'Debug','Release'},{r['configuration'] for r in c['comparisons']})
        for r in c['comparisons']:
            self.assertEqual(r['pre_sha256'],r['post_sha256']);self.assertEqual(4480,r['rows']);self.assertTrue(r['byte_equal'])
    def test_windows_headers_and_native_source_boundary(self):
        v=load('renderer-preservation.json');self.assertEqual('PASS_SOURCE_BOUNDARY',v['status']);self.assertEqual(21,len(v['members']))
        self.assertTrue(all(r['header_preserved'] and r['native_operation_sequence_preserved'] and r['catches_preserved'] and r['native_styles_preserved'] for r in v['members']))
    def test_scope_all_sources_and_transitions(self):
        paths={r['path'] for r in load('source-transitions.json')['files']};self.assertEqual(set(SOURCES),paths)
        for r in load('source-transitions.json')['files']:self.assertEqual(r['sha256'],pin(r['path'])['sha256'])
    def test_278_lineage_and_100_captures(self):
        rows=load('lineage-check.json')['pins'];self.assertEqual(278,len(rows));self.assertEqual(278,len({r['path'] for r in rows}))
        old=json.loads((ROOT/'evidence/04A34/lineage-check.json').read_bytes());by={r['path']:r for r in old['pins']}
        for r in rows:
            self.assertEqual(by[r['path']]['original_04A28_sha256'],r['original_04A28_sha256']);self.assertEqual(by[r['path']]['final_04A34_sha256'],r['accepted34_sha256']);self.assertEqual(pin(r['path'])['sha256'],r['final35_sha256'])
        captures=load('mixed-model-captures-check.json');self.assertEqual(100,captures['count']);self.assertTrue(all(r['matches'] for r in captures['files']))
    def test_preservation_and_metadata_bound(self):
        p=load('preservation.json');self.assertEqual('PASS',p['status']);self.assertEqual(4283,p['tracked_entry']);self.assertEqual(5,len(p['authorized_changes']));self.assertFalse(p['unrelated_changes']);self.assertTrue(p['all_prior_tests_evidence_unchanged']);self.assertTrue(p['master_plan_unchanged'])
        self.assertEqual('MATCH_ACCEPTED_METADATA',load('historic-census-metadata.json')['status'])
    def test_real_counts_and_identity_retention(self):
        v=load('validation.json');self.assertEqual(READY,v['decision']);self.assertFalse(v['unclassified_managed']);self.assertFalse(v['unclassified_python'])
        self.assertTrue(all(r['removed']==r['changed_baseline_outcomes']==0 for r in v['identity_deltas']))
        for config in ['Debug','Release']:
            self.assertEqual(14,v['managed_totals'][config]['Failed']);self.assertEqual(20326+v['new_managed_cases_per_configuration'],v['managed_totals'][config]['Passed'])
        self.assertTrue(all(r['exit_code']==0 and set(r['outcomes'])=={'Passed'} for r in v['focused']))
        self.assertEqual({'total':916,'passed':916,'failed':0},v['node']);self.assertEqual(dict(strict=1,profile=1,docs=0),v['strict_profile_mkdocs'])
    def test_no_historical_identity_deletion(self):
        with (HERE/'managed-test-identities.csv').open(encoding='utf-8-sig') as f:rows=list(csv.DictReader(f))
        with (ROOT/'evidence/04A34/managed-test-identities-final.csv').open(encoding='utf-8-sig') as f:old=list(csv.DictReader(f))
        keys=lambda rs:{(r['suite'],r['raw_identity_sha256'],r['raw_occurrence_ordinal']) for r in rs}
        self.assertTrue(keys(old)<=keys(rows));self.assertEqual(load('validation.json')['new_managed_cases_per_configuration'],len(keys(rows)-keys(old)))
    def test_fixed_gates_and_no_native_claim(self):
        validate_gates(load('entry-authority.json')['gates']);validate_gates(load('deferred-issues.json')['gates'])
        self.assertEqual('NATIVE_NOT_RUN',load('coverage.json')['native']);self.assertEqual('NOT_GOLDEN',load('coverage.json')['scientific'])
    def test_gate_inflation_rejected(self):
        gates=load('entry-authority.json')['gates'];gates['C2']='MET'
        with self.assertRaises(AssertionError):validate_gates(gates)
    def test_frozen_hash_tamper_rejected(self):
        row=load('pre-edit-capture-authority-v2.json')['capture'];row['sha256']='0'*64
        with self.assertRaises(AssertionError):self.assertEqual(row,pin(row['path']))
    def test_privacy_leak_detected(self):
        import importlib.util,uuid
        spec=importlib.util.spec_from_file_location('privacy35_negative',ROOT/'scripts/verify-04a13-publication.py');scanner=importlib.util.module_from_spec(spec);spec.loader.exec_module(scanner)
        p=ROOT/'.test-rationalization-04A35.tmp'/('privacy-probe-'+uuid.uuid4().hex+'.txt')
        self.assertTrue(p.resolve().is_relative_to((ROOT/'.test-rationalization-04A35.tmp').resolve()))
        p.write_text('Q:'+chr(92)+'Users'+chr(92)+'SyntheticOwner'+chr(92)+'probe',encoding='utf-8')
        findings,_=scanner.scan([p.relative_to(ROOT).as_posix()]);self.assertTrue(findings)
    def test_host_free_new_sources(self):
        for p in SOURCES:
            if p.startswith(('BESHStat.Core/','BESHStat.Excel.Common/')):
                text=(ROOT/p).read_text(encoding='utf-8-sig');self.assertIn('Option Strict On',text)
                for token in ['System.Drawing','System.Windows.Forms','Microsoft.Office.Interop','ExcelDna','System.IO','Microsoft.VisualBasic']:
                    self.assertNotIn(token,text,p)
        self.assertIn('Option Compare Text',(ROOT/'BESHStat.Excel.Common/src/Analysis/ComparisonChartPolicy35.vb').read_text())
    def test_privacy_and_claims(self):
        self.assertEqual('PASS',load('privacy-check.json')['status'])
        dossier=(ROOT/'docs/architecture/batch-04A-35-owner-review.md').read_text(encoding='utf-8')
        self.assertIn(READY,dossier);self.assertIn('zero OPEN',dossier)
        self.assertIn(str(load('validation.json')['new_managed_cases_per_configuration'])+' new raw',dossier)
if __name__=='__main__':unittest.main()
