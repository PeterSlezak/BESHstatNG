"""Adverse tests exercise the independent reader without modifying frozen artifacts."""
from pathlib import Path
import copy
import csv
import json
import tempfile
import unittest
import xml.etree.ElementTree as ET
import verify_packet as v


class Packet37Tests(unittest.TestCase):
    def rows(self): return copy.deepcopy(v.load('publication-manifest.json')['files'])
    def ownership(self):
        with (v.HERE/'member-ownership.csv').open(encoding='utf-8-sig') as f:return list(csv.DictReader(f))
    def identities(self):
        with (v.HERE/'managed-test-identities.csv').open(encoding='utf-8-sig') as f:return list(csv.DictReader(f))
    def check_bad(self, rows, expected=None):
        with self.assertRaises((AssertionError,KeyError,TypeError,ValueError)):v.validate_manifest(rows,v.scope() if expected is None else expected)
    def test_independent_reader_validates_complete_packet(self):self.assertGreater(v.verify(),70)
    def test_missing_entry_fails(self):self.check_bad(self.rows()[:-1])
    def test_duplicate_entry_fails(self):
        r=self.rows();r.append(copy.deepcopy(r[0]));self.check_bad(r)
    def test_casefold_collision_fails(self):
        r=self.rows();x=copy.deepcopy(r[0]);x['path']=x['path'].upper();r.append(x);r.sort(key=lambda x:x['path']);self.check_bad(r,{x['path'] for x in r})
    def test_reordered_manifest_fails(self):self.check_bad(list(reversed(self.rows())))
    def test_foreign_entry_fails(self):
        r=self.rows();r[0]['path']='planning/foreign.md';r.sort(key=lambda x:x['path']);self.check_bad(r)
    def test_traversal_fails(self):
        with self.assertRaises(AssertionError):v.canonical('evidence/04A37/../outside.json')
    def test_absolute_path_fails(self):
        with self.assertRaises(AssertionError):v.canonical('/outside.json')
    def test_backslash_path_fails(self):
        with self.assertRaises(AssertionError):v.canonical('evidence'+chr(92)+'bad.json')
    def test_reserved_windows_paths_fail(self):
        for name in ['AUX.txt','CON','NUL.json','COM1.txt','LPT9.md','trailing.','space ']:
            with self.subTest(name=name),self.assertRaises(AssertionError):v.canonical('evidence/04A37/'+name)
    def test_invalid_digest_fails(self):
        r=self.rows();r[0]['sha256']='x'*64;self.check_bad(r)
    def test_corrupted_size_fails(self):
        r=self.rows();r[0]['bytes']+=1;self.check_bad(r)
    def test_boolean_size_fails(self):
        r=self.rows();r[0]['bytes']=True;self.check_bad(r)
    def test_nonself_null_hash_fails(self):
        r=self.rows();r[0]['sha256']=None;self.check_bad(r)
    def test_hashed_self_entry_fails(self):
        r=self.rows();s=next(x for x in r if x['path']==v.SELF);s['bytes']=1;s['sha256']='0'*64;self.check_bad(r)
    def test_missing_self_entry_fails(self):self.check_bad([r for r in self.rows() if r['path']!=v.SELF])
    def test_extra_record_field_fails(self):
        r=self.rows();r[0]['owner']='invented';self.check_bad(r)
    def test_modified_payload_bytes_fail(self):
        with tempfile.TemporaryDirectory(dir=v.ROOT/'.test-rationalization-04A37.tmp') as d:
            root=Path(d);p=root/'evidence/04A37/fixture.txt';p.parent.mkdir(parents=True);p.write_text('original',encoding='utf-8')
            rows=sorted([v.pin('evidence/04A37/fixture.txt',root),dict(path=v.SELF,bytes=None,sha256=None)],key=lambda r:r['path'])
            expected={r['path'] for r in rows};v.validate_manifest(rows,expected,root)
            p.write_text('corrupted',encoding='utf-8')
            with self.assertRaises(AssertionError):v.validate_manifest(rows,expected,root)
    def test_missing_ownership_member_fails(self):
        with self.assertRaises(AssertionError):v.validate_ownership(self.ownership()[:-1],v.load('final-members.json'),v.load('coverage.json'))
    def test_false_ownership_signature_fails(self):
        rows=self.ownership();rows[0]['entry_signature']='invented'
        with self.assertRaises(AssertionError):v.validate_ownership(rows,v.load('final-members.json'),v.load('coverage.json'))
    def test_false_member_successor_fails(self):
        rows=self.ownership();rows[0]['successor']=rows[1]['successor']
        with self.assertRaises(AssertionError):v.validate_ownership(rows,v.load('final-members.json'),v.load('coverage.json'))
    def test_falsely_reserved_in_scope_member_fails(self):
        rows=self.ownership();rows[0]['disposition']='RESERVED_NEXT_AUTHORIZED_BATCH'
        with self.assertRaises(AssertionError):v.validate_ownership(rows,v.load('final-members.json'),v.load('coverage.json'))
    def test_host_operation_cannot_be_falsely_claimed_as_ported(self):
        rows=self.ownership();r=next(r for r in rows if r['disposition']=='HOST_ONLY');r['disposition']='PORTED_AND_CONSUMED'
        with self.assertRaises(AssertionError):v.validate_ownership(rows,v.load('final-members.json'),v.load('coverage.json'))
    def test_open_member_fails(self):
        rows=self.ownership();rows[0]['disposition']='OPEN'
        with self.assertRaises(AssertionError):v.validate_ownership(rows,v.load('final-members.json'),v.load('coverage.json'))
    def test_duplicate_historical_identity_fails(self):
        with (v.HERE/'historical-crosswalk.csv').open(encoding='utf-8-sig') as f:rows=list(csv.DictReader(f))
        rows[1]['historical_member_id']=rows[0]['historical_member_id']
        with self.assertRaises(AssertionError):v.validate_history(rows,self.ownership())
    def texts(self):
        return {k:(v.ROOT/p).read_text(encoding='utf-8-sig') for k,p in dict(Pca='BESHstatNG/src/Graphics/PcaPlotExcel.vb',Factor='BESHstatNG/src/Graphics/FactorAnalysisPlotExcel.vb',CA='BESHstatNG/src/Graphics/CorrespondenceAnalysisPlotExcel.vb',Cluster='BESHstatNG/src/Graphics/ClusterAnalysisPlotExcel.vb').items()}
    def test_unconsumed_biplot_contract_fails(self):
        text=self.texts();text['Pca']=text['Pca'].replace('MultivariatePlotGeometry37.Biplot','DiscardedCoordinates')
        with self.assertRaises(AssertionError):v.validate_consumption(text)
    def test_contract_name_in_comment_is_not_consumption(self):
        text=self.texts();text['Pca']=text['Pca'].replace('MultivariatePlotGeometry37.Biplot','DiscardedCoordinates')+"\n' MultivariatePlotGeometry37.Biplot"
        with self.assertRaises(AssertionError):v.validate_consumption(text)
    def test_injected_statistical_recomputation_fails(self):
        text=self.texts();text['Factor']+='\nDim x = Math.Sqrt(3)'
        with self.assertRaises(AssertionError):v.validate_consumption(text)
    def test_chart_creation_before_layout_validation_fails(self):
        text=self.texts();text['Cluster']='If figure Is Nothing Then\n'+text['Cluster']
        with self.assertRaises(AssertionError):v.validate_consumption(text)
    def test_missing_frozen_capture_fails(self):
        value=copy.deepcopy(v.load('compatibility-comparison.json'));value['comparisons'][0]['pre']['path']='evidence/04A37/missing.tsv'
        with self.assertRaises(AssertionError):v.validate_comparison(value)
    def test_duplicate_and_reordered_cases_fail(self):
        source=v.HERE/'postedit/capture-Debug.tsv'
        with source.open(encoding='utf-8-sig',newline='') as f:r=list(csv.DictReader(f,delimiter='\t'))
        for mode in ['duplicate','reorder']:
            with self.subTest(mode=mode),tempfile.TemporaryDirectory(dir=v.ROOT/'.test-rationalization-04A37.tmp') as d:
                rows=copy.deepcopy(r)
                if mode=='duplicate':rows[1]=copy.deepcopy(rows[0])
                else:rows[0],rows[1]=rows[1],rows[0]
                p=Path(d)/'cases.tsv'
                with p.open('w',encoding='utf-8',newline='') as f:
                    w=csv.DictWriter(f,fieldnames=list(rows[0]),delimiter='\t');w.writeheader();w.writerows(rows)
                with self.assertRaises(AssertionError):v.validate_capture(p,'plot-prefixes')
    def test_private_home_path_fails(self):
        text='C'+':'+chr(92)+chr(92).join(['Users','example-user','book'])
        with self.assertRaises(AssertionError):v.validate_privacy(text)
    def test_private_key_marker_fails(self):
        text='-----'+'BEGIN '+'RSA '+'PRIVATE '+'KEY-----'
        with self.assertRaises(AssertionError):v.validate_privacy(text)
    def test_gates_cannot_be_promoted(self):
        for name in v.GATES:
            g=copy.deepcopy(v.GATES);g[name]='APPROVED'
            with self.subTest(gate=name),self.assertRaises(AssertionError):v.validate_gates(g)
    def test_false_ready_with_open_blocker_fails(self):
        r=copy.deepcopy(v.load('readiness.json'));r['status']=v.READY;r['blockers']=['unresolved']
        with self.assertRaises(AssertionError):v.validate_ready(v.READY,r,v.load('validation.json'))
    def test_false_ready_with_unclassified_failure_fails(self):
        r=copy.deepcopy(v.load('readiness.json'));r['status']=v.READY;r['blockers']=[]
        value=copy.deepcopy(v.load('validation.json'));value['unclassified_python']=1
        with self.assertRaises(AssertionError):v.validate_ready(v.READY,r,value)
    def test_false_ready_cannot_hide_missing_gh_sq_preedit_geometries(self):
        r=copy.deepcopy(v.load('readiness.json'));r['status']=v.READY;r['blockers']=[]
        self.assertFalse(v.required_preedit_coverage()['eligible'])
        with self.assertRaises(AssertionError):v.validate_ready(v.READY,r,v.load('validation.json'))
    def test_guard_returns_are_not_counted_as_biplot_geometry(self):
        coverage=v.required_preedit_coverage()
        self.assertEqual(0,coverage['scales']['0.0']['geometries'])
        self.assertEqual(0,coverage['scales']['0.5']['geometries'])
        self.assertEqual(32,coverage['scales']['0.5']['guard_returns'])
    def test_false_new_case_count_fails(self):
        value=copy.deepcopy(v.load('validation.json'));value['new_managed_cases_per_configuration']=49
        with self.assertRaises(AssertionError):v.validate_validation(value,self.identities())
    def test_lost_inherited_occurrence_fails(self):
        r=self.identities();r.pop()
        with self.assertRaises(AssertionError):v.validate_validation(v.load('validation.json'),r)
    def test_changed_inherited_outcome_fails(self):
        r=self.identities();x=next(x for x in r if x['merged_main_debug']=='Passed');x['final_debug']='Failed'
        with self.assertRaises(AssertionError):v.validate_validation(v.load('validation.json'),r)
    def test_framework_targets_remain_original(self):
        for path in ['BESHStat.Core/BESHStat.Core.vbproj','BESHStat.Excel.Common/BESHStat.Excel.Common.vbproj']:
            root=ET.fromstring((v.ROOT/path).read_bytes());self.assertEqual('netstandard2.0',root.findtext('.//TargetFramework'))
        for path in ['BESHstatNG/BESHStatNG.vbproj','BESHStatNG.Test/BESHStatNG.Tests.vbproj']:
            root=ET.fromstring((v.ROOT/path).read_bytes());self.assertEqual('v4.8',root.findtext('.//{http://schemas.microsoft.com/developer/msbuild/2003}TargetFrameworkVersion'))


if __name__=='__main__':unittest.main()
