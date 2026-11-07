"""Independent packet positives and adversarial metadata/payload checks."""
from pathlib import Path
import unittest,copy,json,csv,tempfile,hashlib
import ast
import verify_packet as v
class Packet36Tests(unittest.TestCase):
    def rows(self):return copy.deepcopy(v.load('publication-manifest.json')['files'])
    def reject(self,rows):
        with self.assertRaises((AssertionError,FileNotFoundError,ValueError)):v.validate_manifest(rows,v.scope())
    def ownership(self):
        with (v.HERE/'member-ownership.csv').open(encoding='utf-8-sig') as f:rows=list(csv.DictReader(f))
        return rows,v.load('final-members.json'),v.load('coverage.json')
    def identities(self):
        with (v.HERE/'managed-test-identities.csv').open(encoding='utf-8-sig') as f:return list(csv.DictReader(f))
    def test_independent_reader_verifies_every_external_artifact(self):self.assertGreater(v.verify(),60)
    def test_all_current_members_have_real_successors(self):v.validate_ownership(*self.ownership())
    def test_all_historical_ids_retained_without_false_same_file_relocation(self):
        with (v.HERE/'historical-crosswalk.csv').open(encoding='utf-8-sig') as f:v.validate_history(list(csv.DictReader(f)),v.load('coverage.json'))
    def test_original_and_corrected_fixture_corpora_remain_distinct(self):
        for name in ['pre-edit-presentation.csv','pre-edit-presentation-v2.csv']:
            with (v.HERE/name).open(encoding='utf-8-sig') as f:rows=list(csv.DictReader(f))
            self.assertEqual(384,len(rows))
            self.assertEqual(6 if name=='pre-edit-presentation.csv' else 330,sum(r['exception']=='None' for r in rows))
    def test_all_six_compatibility_pairs_are_exact(self):v.validate_comparison(v.load('compatibility-comparison.json'))
    def test_managed_identity_multisets_and_genuine_nonpasses(self):v.validate_validation(v.load('validation.json'),self.identities())
    def test_portable_projects_and_sources_remain_host_free(self):
        for p in v.SOURCES:
            if p.startswith(('BESHStat.Core/src/','BESHStat.Excel.Common/src/')) and p.endswith('36.vb'):
                s=(v.ROOT/p).read_text(encoding='utf-8-sig');self.assertIn('Option Strict On',s)
                for dependency in ['Microsoft.Office.Interop','System.Drawing','System.Windows.Forms','ExcelDna','Office.js','System.IO','Microsoft.VisualBasic.']:self.assertNotIn(dependency,s)
        for p in ['BESHStat.Core/BESHStat.Core.vbproj','BESHStat.Excel.Common/BESHStat.Excel.Common.vbproj']:self.assertIn('<TargetFramework>netstandard2.0</TargetFramework>',(v.ROOT/p).read_text())
    def test_residual_control_stays_disabled(self):self.assertIn('ckResidualPlots.Enabled = False',(v.ROOT/'BESHstatNG/src/UI/Ui4Cox.Designer.vb').read_text())
    def test_missing_manifest_entry_rejected(self):self.reject(self.rows()[1:])
    def test_duplicate_manifest_entry_rejected(self):r=self.rows();r.append(r[0]);self.reject(r)
    def test_case_colliding_manifest_entry_rejected(self):r=self.rows();r.append(dict(r[0],path=r[0]['path'].upper()));self.reject(r)
    def test_traversal_path_rejected(self):
        with self.assertRaises(AssertionError):v.canonical('evidence/../private.txt')
    def test_absolute_path_rejected(self):
        with self.assertRaises(AssertionError):v.canonical('/private.txt')
    def test_backslash_path_rejected(self):
        with self.assertRaises(AssertionError):v.canonical('evidence\\private.txt')
    def test_reserved_windows_name_rejected(self):
        with self.assertRaises(AssertionError):v.canonical('evidence/AUX.py')
    def test_trailing_dot_or_space_path_rejected(self):
        for p in ['evidence/file.','evidence/file ']:
            with self.assertRaises(AssertionError):v.canonical(p)
    def test_control_character_path_rejected(self):
        with self.assertRaises(AssertionError):v.canonical('evidence/file\x00')
    def test_wrong_size_rejected(self):r=self.rows();r[0]['bytes']+=1;self.reject(r)
    def test_wrong_digest_rejected(self):r=self.rows();r[0]['sha256']='0'*64;self.reject(r)
    def test_foreign_entry_rejected(self):r=self.rows();r.append(dict(path='outside.txt',bytes=0,sha256='0'*64));self.reject(r)
    def test_recursive_self_hash_rejected(self):r=self.rows();next(x for x in r if x['path']==v.SELF)['sha256']='0'*64;self.reject(r)
    def test_self_size_rejected(self):r=self.rows();next(x for x in r if x['path']==v.SELF)['bytes']=0;self.reject(r)
    def test_foreign_null_hash_rejected(self):r=self.rows();r[0]['sha256']=None;self.reject(r)
    def temporary_payload(self,mutation):
        run=v.ROOT/'.test-rationalization-04A36.tmp';assert run.resolve().parent==v.ROOT.resolve()
        with tempfile.TemporaryDirectory(prefix='independent-audit-',dir=run) as folder:
            root=Path(folder);assert root.resolve().is_relative_to(run.resolve())
            (root/'sample.txt').write_bytes(b'original')
            rows=[v.pin('sample.txt',root),dict(path=v.SELF,bytes=None,sha256=None)]
            mutation(root/'sample.txt')
            with self.assertRaises((AssertionError,FileNotFoundError)):v.validate_manifest(rows,{'sample.txt',v.SELF},root)
    def test_actual_missing_payload_rejected(self):self.temporary_payload(lambda p:p.unlink())
    def test_actual_modified_payload_rejected(self):self.temporary_payload(lambda p:p.write_bytes(b'tampered'))
    def test_false_ported_claim_without_adapter_consumption_rejected(self):
        rows,ast,c=self.ownership();r=next(x for x in rows if x['disposition']=='HOST_ONLY');r.update(disposition='PORTED_AND_CONSUMED',target='Imaginary',compiled_consumer='not a compiled consumer')
        with self.assertRaises(AssertionError):v.validate_ownership(rows,ast,c)
    def test_missing_current_member_rejected(self):
        rows,ast,c=self.ownership()
        with self.assertRaises(AssertionError):v.validate_ownership(rows[1:],ast,c)
    def test_missing_historical_member_rejected(self):
        with (v.HERE/'historical-crosswalk.csv').open(encoding='utf-8-sig') as f:rows=list(csv.DictReader(f))
        with self.assertRaises(AssertionError):v.validate_history(rows[1:],v.load('coverage.json'))
    def test_open_blocker_cannot_be_disguised_as_ready(self):
        rows,ast,c=self.ownership();rows[0]['disposition']='OPEN_BLOCKER'
        with self.assertRaises(AssertionError):v.validate_ownership(rows,ast,c)
    def test_deleted_historical_test_identity_rejected(self):
        rows=self.identities();rows.pop(next(i for i,r in enumerate(rows) if r['merged_main_debug']!='NEW_04A36'))
        with self.assertRaises(AssertionError):v.validate_validation(v.load('validation.json'),rows)
    def test_suppressed_historical_failure_rejected(self):
        rows=self.identities();next(r for r in rows if r['final_debug']=='Failed')['final_debug']='Passed'
        with self.assertRaises(AssertionError):v.validate_validation(v.load('validation.json'),rows)
    def test_fake_native_approval_rejected(self):
        x=v.load('validation.json');x['native']='APPROVED'
        with self.assertRaises(AssertionError):v.validate_validation(x,self.identities())
    def test_fake_r_approval_rejected(self):
        x=v.load('validation.json');x['r']='PASSED'
        with self.assertRaises(AssertionError):v.validate_validation(x,self.identities())
    def test_gate_inflation_rejected(self):
        g=v.load('entry-authority.json')['gates']
        for k in g:
            changed=dict(g);changed[k]='APPROVED'
            with self.assertRaises(AssertionError):v.validate_gates(changed)
    def test_frozen_fixture_hash_tamper_rejected(self):
        c=v.load('compatibility-comparison.json');c['comparisons'][0]['pre_edit']['sha256']='0'*64
        with self.assertRaises(AssertionError):v.validate_comparison(c)
    def test_disconnected_source_callsite_claim_rejected(self):
        rows,ast,c=self.ownership();n=next(n for n in ast['current'] if n['name']=='RenderPanel');n['tokens']=n['tokens'].replace('PreparePresentation36','Disconnected')
        with self.assertRaises(AssertionError):v.validate_ownership(rows,ast,c)
    def test_ready_requires_resolved_external_changes_and_full_diff_check(self):
        with self.assertRaises(AssertionError):v.validate_readiness(v.READY,dict(decision=v.READY),dict(blockers=[]),dict(full_diff_check_exit=2),dict(status='OWNER_CONFIRMATION_PENDING'))
    def test_unclassified_python_failure_cannot_be_hidden(self):
        x=v.load('validation.json');x['unclassified_python']=[dict(identity='unexpected runtime failure')]
        with self.assertRaises(AssertionError):v.validate_validation(x,self.identities())
    def test_private_windows_home_path_rejected(self):
        text=chr(67)+':'+chr(92)+'Users'+chr(92)+'PrivateUser'+chr(92)+'book.xlsx'
        with self.assertRaises(AssertionError):v.validate_public_text(text)
    def test_private_signing_material_rejected(self):
        text='-----BE'+'GIN PRIVATE KEY-----'
        with self.assertRaises(AssertionError):v.validate_public_text(text)
    def test_credential_like_token_rejected(self):
        text='AK'+'IA'+'A'*16
        with self.assertRaises(AssertionError):v.validate_public_text(text)
    def test_missing_production_transition_rejected(self):
        rows=v.load('source-transitions.json')['files']
        with self.assertRaises(AssertionError):v.validate_transitions(rows[1:])
    def test_wrong_production_transition_digest_rejected(self):
        rows=v.load('source-transitions.json')['files'];rows[0]['sha256']='0'*64
        with self.assertRaises(AssertionError):v.validate_transitions(rows)
    def test_every_public_python_helper_parses(self):
        for p in v.HERE.rglob('*.py'):
            if not any(x in p.parts for x in ['bin','obj','__pycache__']):ast.parse(p.read_text(encoding='utf-8-sig'),filename=p.name)
if __name__=='__main__':unittest.main()
