"""04A.20 source/authority/evidence contracts supplement real managed FA/DA fits."""
import hashlib
import importlib.util
import json
from pathlib import Path
import re
import subprocess
import unittest

ROOT=Path(__file__).resolve().parents[2]
E=ROOT/'evidence/04A20'
def read(path): return (ROOT/path).read_text(encoding='utf-8-sig')
def doc(name): return json.loads((E/name).read_bytes())
def sha(path): return hashlib.sha256((ROOT/path).read_bytes()).hexdigest()
def git(*args): return subprocess.check_output(['git','-c','safe.directory='+ROOT.as_posix(),*args],cwd=ROOT,text=True).strip()
def module(name,path):
    s=importlib.util.spec_from_file_location(name,ROOT/path);m=importlib.util.module_from_spec(s);s.loader.exec_module(m);return m

class FactorDiscriminant20Contracts(unittest.TestCase):
    def test_live_main_branch_index_and_accepted_source(self):
        state=doc('entry-state.json')
        self.assertEqual(state['base'],git('rev-parse','HEAD'))
        self.assertEqual(state['base'],git('rev-parse','main'))
        self.assertEqual('codex/04A-20',git('branch','--show-current'))
        self.assertEqual('',git('diff','--cached','--name-only'))
        subprocess.run(['git','-c','safe.directory='+ROOT.as_posix(),'merge-base','--is-ancestor','ae7490bbce241dd8a444fd4d23ba413fb89a36ca','HEAD'],cwd=ROOT,check=True)
        self.assertIn('MultivariateClusteringProjectionPolicy',read('BESHstatNG/src/UI/Ui11PCA.vb'))

    def test_actual_handlers_consume_requests_projection_data_and_report_once(self):
        t=read('BESHstatNG/src/UI/Ui11PCA.vb')
        for name,family,fit,writer in [('RunFA','Factor','.Calculate()','WriteFactorData'),('RunDiscriminantAnalysis','Discriminant','.Fit()','WriteDiscriminantData')]:
            b=re.search(r'    Private Sub '+name+r'\(.*?    End Sub',t,re.S)[0]
            self.assertEqual(1,b.count(fit))
            self.assertIn('New Multivariate'+family+'Request',b)
            self.assertIn('Multivariate'+family+'ProjectionPolicy.Project',b)
            self.assertIn('WindowsMultivariateWorkflowAdapter.'+writer,b)
            self.assertIn('projection.ProjectReport(',b)
            self.assertIn('report.LegacyTables()',b)
            self.assertIn('request.ResultsSheetName',b)
            self.assertLess(b.index(fit),b.index(writer))
        self.assertIn('RenderFactorProjection(projection, fitFA)',t)

    def test_da_prior_and_validation_phase_parse_order_is_exact(self):
        t=read('BESHstatNG/src/UI/Ui11PCA.vb')
        b=re.search(r'    Private Sub RunDiscriminantAnalysis\(.*?    End Sub',t,re.S)[0]
        calls=['fitDA.dataInputs','request.ConfigureModel(fitDA)','Me.GetDiscriminantUserPriors','request.ApplyUserPriors','options.Validation =','options.Seed =','validation.Configure(fitDA)','fitDA.Fit()']
        positions=[b.index(s) for s in calls]
        self.assertEqual(sorted(positions),positions)
        self.assertIn('If priorMode = Multivariate.DiscriminantPriorMode.UserSpecified Then',b)
        request=read('BESHStat.Excel.Common/src/Analysis/MultivariateFactorDiscriminantRequest.vb')
        self.assertLess(request.index('model.priorInputs(labels, weights)'),request.index('Return New MultivariateUserPriorIntent'))
        self.assertNotIn('ParsePriors(',request)

    def test_all_other_form_methods_constructor_and_host_helpers_unchanged(self):
        m=module('c18','scripts/capture-04a18-contract.py');path='BESHstatNG/src/UI/Ui11PCA.vb'
        methods=m.members(path,read(path).replace('\r\n','\n'))
        for n,r in doc('prechange-method-inventory.json')['files'][path]['methods'].items():
            if n not in ['RunFA','RunDiscriminantAnalysis']:
                self.assertEqual(r['sha256'],hashlib.sha256(methods[n].encode()).hexdigest(),n)
        old=subprocess.check_output(['git','-c','safe.directory='+ROOT.as_posix(),'show',doc('entry-state.json')['base']+':'+path],cwd=ROOT).decode('utf-8-sig').replace('\r\n','\n')
        new=read(path).replace('\r\n','\n')
        constructor=lambda s:s[s.index('    Sub New('):s.index('    End Sub',s.index('    Sub New('))]
        self.assertEqual(constructor(old),constructor(new))

    def test_core_native_prior_tests_targets_and_metadata_are_entry_bytes(self):
        pins=doc('prechange-method-inventory.json')['files']
        chosen=[p for p in pins if p.startswith('BESHStat.Core/') or '/Graphics/' in p or '/ResultWriter/' in p or 'Importer' in p or 'Refedit/' in p or p.endswith('.Designer.vb') or re.search(r'Multivariate(17|18|19).*Tests\.vb$',p) or p.endswith('MultivariateLegacyChoices.vb')]
        self.assertGreater(len(chosen),100)
        for p in chosen:self.assertEqual(pins[p]['sha256'],sha(p),p)
        for path in ['BESHStat.Core/BESHStat.Core.vbproj','BESHStat.Excel.Common/BESHStat.Excel.Common.vbproj']:
            self.assertIn('<TargetFramework>netstandard2.0</TargetFramework>',read(path))
        self.assertEqual(doc('documentation-audit.json')['metadata_sha256'],sha('BESHStatNG_Help_MkDocs/docs/_meta/methods.yml'))

    def test_frozen_host_converter_bodies_are_exact_and_core_owns_group_normalization(self):
        d=doc('host-conversion-provenance.json')
        m=module('c18','scripts/capture-04a18-contract.py')
        methods=m.members(d['source'],read(d['source']))
        for name,r in d['methods'].items():
            self.assertEqual(r['original_sha256'],hashlib.sha256(methods[name].encode()).hexdigest())
            derivative=re.search(r'    Public Shared Function '+r['derivative_name']+r'\(.*?    End Function',read(d['derivative']),re.S)[0]
            self.assertEqual(r['derivative_sha256'],hashlib.sha256(derivative.encode()).hexdigest())
        self.assertIn('Convert.ToString(value, CultureInfo.InvariantCulture)',read('BESHStat.Core/src/RegModels/Multivariate/DiscriminantAnalysis.vb'))

    def test_common_has_no_fit_classifier_rotation_rng_or_host_dependency(self):
        files=['Analysis/MultivariateFactorDiscriminantRequest.vb','Results/MultivariateModelProjection.vb','Results/MultivariateFactorProjection.vb','Results/MultivariateDiscriminantProjection.vb']
        for file in files:
            text=read('BESHStat.Excel.Common/src/'+file)
            self.assertIn('Option Strict On',text)
            for forbidden in ['.Fit()', '.Calculate()', 'New Random', 'System.IO', 'Microsoft.VisualBasic', 'Windows.Forms', 'Interop.Excel', 'Office.js', 'BuildFoldAssignments(', 'EigenJk(', 'ShuffleInPlace(']:
                self.assertNotIn(forbidden,text,file)
        t=read('BESHStat.Excel.Common/src/Analysis/MultivariateFactorDiscriminantRequest.vb')
        self.assertIn('_options = options.Copy()',t)
        self.assertIn('Return _options.Copy()',t)

    def test_factor_axes_separate_scores_pattern_structure_phi_and_actual_count(self):
        t=read('BESHStat.Excel.Common/src/Results/MultivariateFactorProjection.vb')
        for token in ['model.NumberOfFactors','model.AnalysisRowIds','model.PatternMatrix','model.StructureMatrix','model.FactorCorrelationMatrix','model.RotationTransform','model.ScoreCoefficientMatrix','model.Scores','model.ExtractionConverged','model.RotationConverged']:
            self.assertIn(token,t)
        self.assertIn('MultivariateMappingState.Ambiguous',t)
        self.assertIn('No',doc('factor-discriminant-projections-v1.json')['raw_ownership'])

    def test_da_case_confusion_axes_and_copied_metrics_preserve_core_direction(self):
        t=read('BESHStat.Excel.Common/src/Results/MultivariateDiscriminantProjection.vb')
        for token in ['ObservedClasses','PredictedClasses','AccuracyFraction = value.OverallAccuracy','AccuracyPercent = value.OverallAccuracyPct','value.RecallPct.Clone()','value.PrecisionPct.Clone()','prepared.ActiveOriginalIndices','prepared.RemovedOriginalIndices','model.ValidationClassification','IsTrainingResubstitution']:
            self.assertIn(token,t)
        self.assertNotIn('counts.Sum(',t)
        self.assertIn('C20-DA-ROWID-DISPLAY-01',t)
        d=doc('candidate-issues.json')
        self.assertEqual(0,d['activated_behavior_fixes'])

    def test_seed_unavailability_and_prior_scope_are_not_fabricated(self):
        t=read('BESHStat.Excel.Common/src/Analysis/MultivariateFactorDiscriminantRequest.vb')
        self.assertIn('_options.Seed <> Integer.MinValue',t)
        self.assertIn('Public ReadOnly Property EffectiveSeed As Integer?',t)
        self.assertIn('public Core exposes no effective validation seed',doc('factor-discriminant-requests-v1.json')['validation_seed'])
        self.assertNotIn('GetField(',t)

    def test_frozen_hashes_all_corpora_and_72_nonconvergence_identities(self):
        for p,r in doc('frozen-capture-hashes.json')['files'].items():self.assertEqual(r['sha256'],sha(p),p)
        d=doc('historical-fa-nonconvergence.json')
        original=json.loads((ROOT/d['authority']).read_bytes())
        self.assertEqual([r for r in original['records'] if r['outcome']=='FA_EXTRACTION=False;ROTATION=True'],d['records'])
        self.assertEqual(72,d['record_count'])
        for p,n in [('evidence/04A17/multivariate-comparisons-v1.json',1224),('evidence/04A18/pca-ca-mca-projection-comparisons-v1.json',916),('evidence/04A19/kmeans-hierarchical-projection-comparisons-v1.json',2478)]:
            self.assertEqual(n,len(json.loads((ROOT/p).read_bytes())['records']))
        registry=json.loads((ROOT/'evidence/04A14/baseline-registry.json').read_bytes())
        self.assertEqual(100,len(registry['files']))
        for p,r in registry['files'].items():self.assertEqual(r['sha256'],sha(p))

    def test_original_control_ids_obligations_and_scientific_native_gates(self):
        d=doc('original-control-crosswalk.json')
        self.assertEqual((186,152),(len(d['controls']),len(d['obligations'])))
        self.assertEqual((147,39),(d['populations']['Ui11PCA']['controls'],d['populations']['Ui0OneRefeditMulticol']['controls']))
        self.assertEqual(('NO','NO GO','NO','UNMET'),tuple(d[k] for k in ['P','N1','N2','C2']))
        self.assertEqual((0,0),(d['releases'],d['discharges']))
        for p,r in d['authorities'].items():self.assertEqual(r['sha256'],sha(p))

    def test_real_profile_schema_hashes_test_links_and_honest_rejections(self):
        d=doc('fa-da-same-core-comparisons-v1.json')
        self.assertEqual(('NOT_GOLDEN','SAME_CORE_COMPATIBILITY_ONLY'),(d['status'],d['validation']))
        self.assertFalse(d['promoted']);self.assertGreater(d['record_count'],500)
        self.assertEqual(len(d['records']),d['record_count'])
        for key,payload in d['profile_payloads'].items():
            self.assertEqual(key,hashlib.sha256(json.dumps(payload,sort_keys=True,ensure_ascii=False,separators=(',',':'),allow_nan=False).encode()).hexdigest())
            self.assertIn(payload['family'],['FA','DA'])
        for r in d['records']:
            self.assertIn(r['profile_id'],d['profile_payloads']);self.assertEqual('Passed',r['test_outcome'])
            self.assertRegex(r['raw_trx_sha256'],r'^[0-9a-f]{64}$')
        self.assertIn('EXISTING_CORE_REJECTED',d['outcomes']);self.assertIn('CORE_NONCONVERGED',d['outcomes'])
        self.assertIn('UNSPECIFIED_SEED_OBSERVATION',d['outcomes'])

    def test_windows_plot_dispatch_and_data_contracts_are_source_equivalent(self):
        t=read('BESHstatNG/src/DM/WindowsMultivariateWorkflowAdapter.vb')
        b=re.search(r'    Public Shared Sub RenderFactorProjection\(.*?    End Sub',t,re.S)[0]
        positions=[b.index('FactorAnalysisPlotExcel.'+v+'(model)') for v in ['ScreePlot','LoadingPlot2D','LoadingPlot3D']]
        self.assertEqual(sorted(positions),positions);self.assertNotIn('NumberOfFactors',b)
        request=read('BESHStat.Excel.Common/src/Analysis/MultivariateFactorDiscriminantRequest.vb')
        self.assertIn('ClusterInputTable(original, Input.VariableNames, Input.SourceRowIds, Nothing)',request)
        self.assertIn('DiscriminantInputTable(original, Input.VariableNames, groups, Input.RowLabels)',request)

    def test_final_help_truth_privacy_trackability_and_entry_failure_accounting(self):
        for page in ['factor-analysis','discriminant-analysis']:
            text=read('BESHStatNG_Help_MkDocs/docs/methods/'+page+'.md')
            self.assertIn('Excel Online',text);self.assertIn('Excel for Mac',text)
        for p in E.glob('*.json'):
            self.assertIsNone(re.search(r'[A-Za-z]:[\\/]|S-1-5-21-',p.read_text(encoding='utf-8')),p.name)
            self.assertEqual(1,subprocess.run(['git','-c','safe.directory='+ROOT.as_posix(),'check-ignore','-q','--',p.relative_to(ROOT).as_posix()],cwd=ROOT).returncode)
        for v,d in doc('entry-validation.json')['configurations'].items():
            self.assertEqual(6,d['logical_cases']['nonpassed']);self.assertNotEqual(0,d['full_command_exit'])

if __name__=='__main__':unittest.main()
