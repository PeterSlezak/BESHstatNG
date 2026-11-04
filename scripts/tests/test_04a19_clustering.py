"""New 04A.19 source/authority/schema checks supplement real managed fits."""
import hashlib
import importlib.util
import json
from pathlib import Path
import re
import subprocess
import unittest

ROOT=Path(__file__).resolve().parents[2]
E=ROOT/'evidence/04A19'
def read(path): return (ROOT/path).read_text(encoding='utf-8-sig')
def doc(name): return json.loads((E/name).read_bytes())
def sha(path): return hashlib.sha256((ROOT/path).read_bytes()).hexdigest()
def git(*args): return subprocess.check_output(['git','-c','safe.directory='+ROOT.as_posix(),*args],cwd=ROOT,text=True).strip()

class Clustering19Contracts(unittest.TestCase):
    def test_live_base_branch_and_empty_index(self):
        state=doc('entry-state.json')
        self.assertEqual(state['base'],git('rev-parse','HEAD'))
        self.assertEqual(state['base'],git('rev-parse','main'))
        self.assertEqual('codex/04A-19',git('branch','--show-current'))
        self.assertEqual('',git('diff','--cached','--name-only'))
        subprocess.run(['git','-c','safe.directory='+ROOT.as_posix(),'merge-base','--is-ancestor','f539d4e31779a2083d4c135e2784314c0083cff4','HEAD'],cwd=ROOT,check=True)

    def test_shipping_handlers_use_requests_projections_and_one_fit(self):
        text=read('BESHstatNG/src/UI/Ui11PCA.vb')
        for name,family in [('RunKmeans','KMeans'),('RunHierarchicalClustering','Hierarchical')]:
            body=re.search(r'    Private Sub '+name+r'\(.*?    End Sub',text,re.S)[0]
            self.assertEqual(1,body.count('.Fit()'))
            self.assertIn('New Multivariate'+family+'Request',body)
            self.assertIn('request.Configure(',body)
            self.assertIn('WindowsMultivariateWorkflowAdapter.WriteClusterData',body)
            self.assertIn('MultivariateClusteringProjectionPolicy.'+family,body)
            self.assertIn('projection.LegacyTables()',body)
            self.assertIn('projection.ResultsSheetName',body)
        self.assertIn('WindowsMultivariateWorkflowAdapter.RenderDendrogram',text)
        self.assertLess(text.index('fitKmeans.startingCentersInputs(centers)'),text.index('Dim seed As Integer =',text.index('Private Sub RunKmeans')))
        h=text[text.index('Private Sub RunHierarchicalClustering'):text.index('Private Sub RunKmeans')]
        self.assertLess(h.index('request.Configure('),h.index('options.CutHeight = ParseUiDouble'))
        self.assertLess(h.index('rr.writeToSheet'),h.index('New MultivariateDendrogramRequest'))

    def test_core_native_writer_and_prior_test_bytes_unchanged(self):
        files=doc('prechange-method-inventory.json')['files']
        selected=[p for p in files if p.startswith('BESHStat.Core/') or '/Graphics/' in p or '/ResultWriter/' in p or 'Importer' in p or 'Refedit/' in p or p.endswith(('.Designer.vb','.resx')) or p.endswith('Multivariate18Tests.vb') or p.endswith('Multivariate17Tests.vb')]
        self.assertGreater(len(selected),100)
        for p in selected: self.assertEqual(files[p]['sha256'],sha(p),p)
        self.assertEqual(sha('BESHStatNG_Help_MkDocs/docs/_meta/methods.yml'),doc('documentation-audit.json')['metadata_sha256'])

    def test_all_other_form_methods_and_import_catches_preserved(self):
        spec=importlib.util.spec_from_file_location('capture18',ROOT/'scripts/capture-04a18-contract.py')
        m=importlib.util.module_from_spec(spec);spec.loader.exec_module(m)
        path='BESHstatNG/src/UI/Ui11PCA.vb'
        current=m.members(path,read(path).replace('\r\n','\n'))
        for n,r in doc('prechange-method-inventory.json')['files'][path]['methods'].items():
            if n not in ['RunKmeans','RunHierarchicalClustering']:
                self.assertEqual(r['sha256'],hashlib.sha256(current[n].encode()).hexdigest(),n)

    def test_requests_are_detached_and_contain_no_fit_rng_or_geometry(self):
        text=read('BESHStat.Excel.Common/src/Analysis/MultivariateClusteringRequest.vb')
        self.assertIn('Option Strict On',text)
        self.assertIn('_options = options.Copy()',text)
        self.assertIn('Return _options.Copy()',text)
        self.assertIn('_options.Seed <> Integer.MinValue',text)
        self.assertIn('result.CreateDendrogramLayout(',text)
        for forbidden in ['.Fit()', 'New Random', 'System.IO', 'Windows.Forms', 'Interop.Excel', 'Office.js', 'Microsoft.VisualBasic', 'Worksheet', 'RefEdit']:
            self.assertNotIn(forbidden,text)
        projection=read('BESHStat.Excel.Common/src/Results/MultivariateClusteringProjection.vb')
        self.assertNotIn('.Fit()',projection)
        self.assertNotIn('PrepareData(',projection)
        self.assertIn('MultivariateReportPolicy.Project',projection)
        self.assertIn('AsReadOnly()',projection)

    def test_rowid_issue_is_dormant_and_axes_remain_distinct(self):
        issue=doc('cluster-rowid-disposition.json')
        self.assertEqual('OPEN / CANDIDATE_ONLY',issue['status'])
        fixture=issue['fixture']
        self.assertEqual([101,105,118,131],fixture['source_rows'])
        self.assertEqual([1,3,4],fixture['active_input_ordinals'])
        self.assertEqual([1,2,3],fixture['active_leaf_ids'])
        text=read('BESHStat.Excel.Common.Tests/src/Multivariate19Tests.vb')
        self.assertIn('PhysicalRowsInputOrdinalsAndLeavesStayDistinct',text)
        for axis in ['InputOrdinal','SourceRowId','ActiveOrdinal','RemovedOrdinal','LeafId','LeafPosition','Membership']:
            self.assertIn(axis,read('BESHStat.Excel.Common/src/Results/MultivariateClusteringProjection.vb'))

    def test_host_free_centre_checks_are_exact_frozen_validation_statements(self):
        d=doc('legacy-import-validation-provenance.json')
        for path in [d['source'],d['derivative']]:
            text=read(path).replace('\r\n','\n')
            start=text.index('        If centersData.bZeroValid Then')
            end=text.index('        Return centersData.DataDbl',start)+len('        Return centersData.DataDbl')
            self.assertEqual(d['validation_block_sha256'],hashlib.sha256(text[start:end].encode()).hexdigest())

    def test_original_controls_case_and_gate_codes_are_retained(self):
        d=doc('original-control-crosswalk.json')
        self.assertEqual((0,0),(d['releases'],d['discharges']))
        self.assertEqual(('NO','NO GO','NO','UNMET'),tuple(d[k] for k in ['P','N1','N2','C2']))
        self.assertEqual(147,d['populations']['Ui11PCA']['controls'])
        self.assertEqual(39,d['populations']['Ui0OneRefeditMulticol']['controls'])
        self.assertEqual(152,len(d['obligations']))
        self.assertEqual('04A02-PCA-NATIVE-01',d['historical_case']['case_id'])
        for p,r in d['authorities'].items(): self.assertEqual(r['sha256'],sha(p))

    def test_frozen_payloads_and_original_capture_hashes_remain_exact(self):
        for p,r in doc('frozen-capture-hashes.json')['files'].items(): self.assertEqual(r['sha256'],sha(p),p)
        registry=json.loads((ROOT/'evidence/04A14/baseline-registry.json').read_bytes())
        self.assertEqual(100,len(registry['files']))
        for p,r in registry['files'].items(): self.assertEqual(r['sha256'],sha(p),p)
        for p,n in [('evidence/04A17/multivariate-comparisons-v1.json',1224),('evidence/04A18/pca-ca-mca-projection-comparisons-v1.json',916)]:
            self.assertEqual(n,len(json.loads((ROOT/p).read_bytes())['records']))

    def test_corpus_schema_links_real_outcomes_and_nonpasses(self):
        d=doc('kmeans-hierarchical-projection-comparisons-v1.json')
        self.assertEqual('NOT_GOLDEN',d['status'])
        self.assertEqual('SAME_CORE_COMPATIBILITY_ONLY',d['validation'])
        self.assertFalse(d['promoted'])
        self.assertGreater(d['record_count'],500)
        self.assertEqual(len(d['records']),d['record_count'])
        for key,payload in d['profile_payloads'].items():
            self.assertEqual(key,hashlib.sha256(json.dumps(payload,sort_keys=True,ensure_ascii=False,separators=(',',':'),allow_nan=False).encode()).hexdigest())
        for r in d['records']:
            self.assertIn(r['profile_id'],d['profile_payloads'])
            self.assertEqual('Passed',r['test_outcome'])
            self.assertRegex(r['raw_trx_sha256'],r'^[0-9a-f]{64}$')
        outcomes=set(d['outcomes'])
        self.assertIn('EXISTING_CORE_REJECTED',outcomes)
        self.assertIn('CORE_RETURNED',outcomes)
        entry=doc('entry-validation.json')
        for config in ['Debug','Release']:
            self.assertEqual(6,entry['configurations'][config]['logical_cases']['nonpassed'])
            self.assertNotEqual(0,entry['configurations'][config]['full_command_exit'])

    def test_native_bridge_arguments_and_targets_stay_authoritative(self):
        bridge=read('BESHstatNG/src/DM/WindowsMultivariateWorkflowAdapter.vb')
        for argument in ['topLeftCellAddress:=request.Anchor','chartWidth:=request.Width','chartHeight:=request.Height','heightMode:=request.HeightMode','orientation:=request.Orientation','labelMode:=request.LabelMode','chartTitle:=request.Title','cutMode:=request.Cut.Mode','membershipClusterCount:=request.Cut.Clusters','membershipCutHeight:=request.Cut.Height']:
            self.assertIn(argument,bridge)
        for project in ['BESHStat.Core/BESHStat.Core.vbproj','BESHStat.Excel.Common/BESHStat.Excel.Common.vbproj']:
            self.assertIn('<TargetFramework>netstandard2.0</TargetFramework>',read(project))
        self.assertIn('src\\Multivariate19AdapterTests.vb',read('BESHStatNG.Test/BESHStatNG.Tests.vbproj'))

    def test_help_and_public_evidence_are_truthful_and_trackable(self):
        for name in ['k-means-clustering','hierarchical-clustering']:
            text=read('BESHStatNG_Help_MkDocs/docs/methods/'+name+'.md')
            self.assertIn('analysis input positions',text)
            self.assertIn('Excel Online',text)
        for p in E.glob('*.json'):
            text=p.read_text(encoding='utf-8')
            self.assertIsNone(re.search(r'[A-Za-z]:[\\/]|S-1-5-21-',text),p.name)
            result=subprocess.run(['git','-c','safe.directory='+ROOT.as_posix(),'check-ignore','-q','--',p.relative_to(ROOT).as_posix()],cwd=ROOT)
            self.assertEqual(1,result.returncode,p.name)

if __name__=='__main__': unittest.main()
