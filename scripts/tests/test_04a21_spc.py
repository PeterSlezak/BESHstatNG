"""04A.21 real SPC consumers and protected-authority contracts; managed tests fit Core."""
import hashlib
import importlib.util
import json
from pathlib import Path
import re
import subprocess
import unittest

ROOT=Path(__file__).resolve().parents[2]
E=ROOT/'evidence/04A21'
def read(path):return (ROOT/path).read_text(encoding='utf-8-sig')
def doc(name):return json.loads((E/name).read_bytes())
def sha(path):return hashlib.sha256((ROOT/path).read_bytes()).hexdigest()
def git(*args):return subprocess.check_output(['git','-c','safe.directory='+ROOT.as_posix(),*args],cwd=ROOT,text=True).strip()
def members(path):
    s=importlib.util.spec_from_file_location('members18',ROOT/'scripts/capture-04a18-contract.py');m=importlib.util.module_from_spec(s);s.loader.exec_module(m)
    return m.members(path,read(path))
FORM='BESHstatNG/src/UI/Ui21ControlCharts.vb'
P='BESHStat.Excel.Common/src/Analysis/'

class Spc21Contracts(unittest.TestCase):
    def test_live_merged_main_ancestry_branch_and_empty_index(self):
        state=doc('entry-state.json')
        self.assertEqual('0f1980e5e223cf4770ce2bebe2d5828daa7ac81e',git('rev-parse','main'))
        self.assertEqual(state['base'],git('rev-parse','HEAD'))
        self.assertEqual('codex/04A-21',git('branch','--show-current'))
        self.assertEqual('',git('diff','--cached','--name-only'))
        self.assertIn('MultivariateFactorProjectionPolicy.Project',read('BESHstatNG/src/UI/Ui11PCA.vb'))

    def test_actual_point_stage_exclusion_request_consumers(self):
        methods=members(FORM)
        mapping={'GetInputRowContext':['SpcInputPointPolicy.FirstDataRow','SpcInputPointPolicy.FromWorksheetRows'],
            'BuildInputData':['context.PointMap.SourceRowIndices.ToArray()','SpcInputData.FromIndividualSequence','SpcInputData.FromWideSubgroups','SpcInputData.FromStackedObservations','SpcInputData.FromAggregatedCounts'],
            'btApplyQuickPhaseSetup_Click':['SpcPhaseStagePolicy.Quick'],
            'btImportStages_Click':['SpcPhaseStagePolicy.Collapse','SpcPhaseStagePolicy.ImportedRows'],
            'BuildStagesFromGrid':['SpcPhaseStagePolicy.Manual','SpcPhaseStagePolicy.FinishManual'],
            'btImportExclusions_Click':['IsExclusionIndicator(value)','SpcExclusionPolicy.Import'],
            'BuildExclusionsFromGrid':['SpcExclusionPolicy.CoreIndex'],
            'BuildRequest':['New SpcLimitRequest','New SpcRuleRequest','SpcRequestPreparation.Analysis','SpcRequestPreparation.Create','SpcEngine.Validate(request)']}
        for name,tokens in mapping.items():
            for token in tokens:self.assertIn(token,methods[name],name)

    def test_single_fit_cancellation_and_original_native_output_tail(self):
        methods=members(FORM);self.assertEqual(1,read(FORM).count('SpcEngine.Fit('))
        self.assertIn('SpcEngine.Fit(request, AddressOf CancellationRequested)',methods['btCompute_Click'])
        pins=doc('prechange-method-inventory.json')['files'][FORM]['methods']
        for name in ['btCompute_Click','BeginComputation','EndComputation','CancellationRequested','WriteResultsToNewWorkbook','BuildAppearanceOptions','CreateResultWorkbook','CreateResultSheet']:
            self.assertEqual(pins[name]['sha256'],hashlib.sha256(methods[name].encode()).hexdigest(),name)
        tail=methods['WriteResultsToNewWorkbook'];names=['SPC Summary','Control Charts','Chart Data','Signals','Settings and Audit']
        self.assertEqual(sorted(tail.index('"'+n+'"') for n in names),[tail.index('"'+n+'"') for n in names])
        self.assertIn('graphics.SpcControlChartExcel.AddCharts',tail)

    def test_original_header_bounds_roles_and_read_chronology(self):
        methods=members(FORM);b=methods['GetInputRowContext']
        for token in ['required.AddRange(RoleColumnInfos(lbValues))','LastUsedRow(info.ColumnNumber)','Math.Max(lastRow,','If lastRow < firstRow','ReadTextColumn(subgroupInfo, firstRow, lastRow)']:
            self.assertIn(token,b)
        self.assertNotIn('lbLabels',b);self.assertNotIn('lbSequence',b)
        for name in ['ValidateRoleSelections','ValidateActiveRoleDuplicates','RequireExactlyOne','ReadObjectColumn','ReadNumericColumn','ReadTextColumn','NormalizeWorksheetValue','IsExclusionIndicator']:
            pin=doc('prechange-method-inventory.json')['files'][FORM]['methods'][name]['sha256']
            self.assertEqual(pin,hashlib.sha256(methods[name].encode()).hexdigest(),name)

    def test_phase_clear_and_manual_failure_precedence_are_preserved(self):
        m=members(FORM)
        self.assertLess(m['btImportStages_Click'].index('SpcPhaseStagePolicy.Collapse'),m['btImportStages_Click'].index('dgvStages.Rows.Clear()'))
        self.assertLess(m['btImportStages_Click'].index('dgvStages.Rows.Clear()'),m['btImportStages_Click'].index('SpcPhaseStagePolicy.ImportedRows'))
        self.assertLess(m['btApplyQuickPhaseSetup_Click'].index('dgvStages.Rows.Clear()'),m['btApplyQuickPhaseSetup_Click'].index('CInt(spinLastPhaseIPoint.Value)'))
        self.assertLess(m['BuildExclusionsFromGrid'].index('SpcExclusionPolicy.CoreIndex'),m['BuildExclusionsFromGrid'].index('ParseExclusionScope'))
        self.assertIn('Iterator Function ImportedRows',read(P+'SpcPhaseStagePolicy.vb'))

    def test_stacked_and_stage_comparers_are_distinct(self):
        point=read(P+'SpcInputPointPolicy.vb');stage=read(P+'SpcPhaseStagePolicy.vb')
        self.assertIn('Dictionary(Of String, Integer)(StringComparer.Ordinal)',point)
        self.assertIn('HashSet(Of String)(StringComparer.OrdinalIgnoreCase)',stage)
        self.assertIn('StringComparison.Ordinal)',stage)
        self.assertIn('runNumber.ToString(CultureInfo.InvariantCulture)',stage)
        self.assertNotIn('.OrderBy(',point)

    def test_exclusions_any_row_all_reasons_and_scope_are_not_redefined(self):
        t=read(P+'SpcExclusionPolicy.vb')
        self.assertIn('If indicators(row) Then excluded(point) = True',t)
        self.assertIn('If reasons IsNot Nothing AndAlso reasons(row).Trim().Length > 0 Then',t)
        self.assertIn('HashSet(Of String)(StringComparer.Ordinal)',t)
        self.assertIn('String.Join("; ", sets(point).ToArray())',t)
        self.assertNotIn('Distinct(',members(FORM)['BuildExclusionsFromGrid'])

    def test_rule_limit_assembly_and_host_parsing_sequence(self):
        m=members(FORM);b=m['BuildRequest']
        calls=['BuildInputData()','New SpcLimitRequest','New SpcRuleRequest','BuildCustomRules','SelectedComboValue(Of SpcMissingValuePolicy)','BuildStagesFromGrid(input)','BuildExclusionsFromGrid(input)','BuildHistoricalParameters()','BuildSpecificationLimits(chartType)','BuildChartParameters(chartType)','Me.Text','tbChartTitle.Text.Trim()','tbValueAxisTitle.Text.Trim()','SpcEngine.Validate(request)']
        positions=[b.index(c) for c in calls];self.assertEqual(sorted(positions),positions)
        t=read(P+'SpcRequestPreparation.vb')
        self.assertIn('SpcRulePreset.None, Preset)',t)
        self.assertIn('_custom.Clone()',t)
        self.assertIn('.GapBehavior = GapBehavior',t)
        self.assertNotIn('ResolveRules(',t)

    def test_presets_ui_state_defaults_and_conversion_helpers_stay_entry(self):
        m=members(FORM);pins=doc('prechange-method-inventory.json')['files'][FORM]['methods']
        for name in ['InitializeOptionControls','UpdateChartDependentControls','PopulateDataLayouts','UpdateRoleAvailability','UpdateLimitOptions','UpdateHistoricalColumns','UpdateRuleAvailability','RestoreCustomRules','LoadSelectedRulePreset','BuildSpecificationLimits','BuildChartParameters','OptionalDoubleCell','OptionalDoubleText','TryParseDouble','ParseRuleKind','ParseRuleSide','ParseRuleScope']:
            self.assertEqual(pins[name]['sha256'],hashlib.sha256(m[name].encode()).hexdigest(),name)

    def test_core_ui22_native_designers_prior_tests_and_targets_unchanged(self):
        pins=doc('prechange-method-inventory.json')['files'];selected=[p for p in pins if p.startswith('BESHStat.Core/') or 'SpcControlChartExcel' in p or 'Ui22Multivariate' in p or '.Designer.' in p or 'Multivariate20' in p or '/StatisticalProcessControl/Spc' in p]
        self.assertTrue(selected)
        for p in selected:self.assertEqual(pins[p]['sha256'],sha(p),p)
        for p in ['BESHStat.Core/BESHStat.Core.vbproj','BESHStat.Excel.Common/BESHStat.Excel.Common.vbproj']:self.assertIn('<TargetFramework>netstandard2.0</TargetFramework>',read(p))

    def test_portable_policies_have_no_host_math_or_fit(self):
        for filename in ['SpcInputPointPolicy.vb','SpcPhaseStagePolicy.vb','SpcExclusionPolicy.vb','SpcRequestPreparation.vb']:
            t=read(P+filename);self.assertIn('Option Strict On',t)
            for token in ['System.Windows.Forms','Interop.Excel','Microsoft.VisualBasic','System.IO','SpcEngine.Fit','SpcEngine.Validate','SpcStatistics.','New Random','SpcRuleEvaluator','Math.Sqrt','Binomial','Poisson']:
                self.assertNotIn(token,t,filename)
        self.assertIn('New SpcFitRequest(',read(P+'SpcRequestPreparation.vb'))

    def test_legacy_reference_is_frozen_source_derived_and_not_new_policy(self):
        d=doc('legacy-preparation-provenance.json');m=members('BESHStat.Excel.Common.Tests/src/Spc21LegacyPreparation.vb')
        pins=doc('prechange-method-inventory.json')['files'][FORM]['methods']
        for name,h in d['method_hashes'].items():self.assertEqual(h,pins[name]['sha256'],name)
        t=read(d['derivative'])
        for token in ['SpcInputPointPolicy','SpcPhaseStagePolicy','SpcExclusionPolicy','SpcRequestPreparation']:
            self.assertNotIn(token,t)

    def test_frozen_authorities_comparisons_and_original_100_captures(self):
        for p,r in doc('frozen-capture-hashes.json')['files'].items():self.assertEqual(r['sha256'],sha(p),p)
        registry=json.loads((ROOT/'evidence/04A14/baseline-registry.json').read_bytes())
        self.assertEqual(100,len(registry['files']))
        for p,r in registry['files'].items():self.assertEqual(r['sha256'],sha(p))

    def test_actual_spc_control_union_and_unchanged_gates(self):
        d=doc('original-control-crosswalk.json')
        self.assertEqual(len({r['control_id'] for r in d['controls']}),d['union']['controls'])
        self.assertEqual(len({r['obligation_id'] for r in d['obligations']}),d['union']['linked_open_obligations'])
        for p,r in d['authorities'].items():self.assertEqual(r['sha256'],sha(p))
        for host,row in d['populations'].items():
            names=set(re.findall(r'Friend WithEvents (\w+) As ',read('BESHstatNG/src/UI/'+host+'.Designer.vb')))
            self.assertEqual(names,{n.split('.')[-1] for n in row['control_ids']})
        self.assertEqual(('NO','NO GO','NO','UNMET'),tuple(d[k] for k in ['P','N1','N2','C2']))
        self.assertEqual((0,0),(d['releases'],d['discharges']))

    def test_executed_profile_schema_payload_hashes_and_negative_classifications(self):
        d=doc('spc-same-core-comparisons-v1.json')
        self.assertEqual(('NOT_GOLDEN','SAME_CORE_COMPATIBILITY_ONLY'),(d['status'],d['validation']))
        self.assertFalse(d['promoted']);self.assertEqual(len(d['records']),d['record_count'])
        self.assertEqual(len(d['payloads']),d['payload_count'])
        for key,p in d['payloads'].items():
            self.assertEqual(key,hashlib.sha256(json.dumps(p,sort_keys=True,separators=(',',':'),ensure_ascii=False,allow_nan=False).encode()).hexdigest())
            self.assertEqual('NOT_GOLDEN',p['status'])
        for r in d['records']:self.assertIn(r['profile_id'],d['payloads']);self.assertEqual('Passed',r['outcome'])
        self.assertTrue({'CORE_RETURNED','EXISTING_REJECTION','EXISTING_TWO_LEVEL_REJECTION'}<=set(d['outcomes']))
        self.assertEqual(d['dataset']['sha256'],sha(d['dataset']['path']))

    def test_candidate_only_disposition_and_unchanged_prior_issues(self):
        d=doc('candidate-issues.json');self.assertEqual(0,d['activated_behavior_fixes'])
        self.assertTrue(any(r['id']=='C21-STACKED-ROWCOUNT-VALIDATION-01' for r in d['issues']))
        self.assertIn('C20',d['retained_prior'])

    def test_help_availability_metadata_and_public_trackability(self):
        t=read('BESHStatNG_Help_MkDocs/docs/methods/control-charts.md')
        for token in ['Excel Online','Excel for Mac','first-appearance','logical chart points']:self.assertIn(token,t)
        p='BESHStatNG_Help_MkDocs/docs/_meta/methods.yml'
        self.assertEqual(doc('documentation-audit.json')['metadata_sha256'],sha(p))
        for p in E.glob('*.json'):
            self.assertIsNone(re.search(r'[A-Za-z]:[\\/]|S-1-5-21-',p.read_text(encoding='utf-8')),p.name)
            self.assertEqual(1,subprocess.run(['git','-c','safe.directory='+ROOT.as_posix(),'check-ignore','-q','--',p.relative_to(ROOT).as_posix()],cwd=ROOT).returncode)

    def test_entry_nonzero_is_measured_and_not_relabelled_pass(self):
        for config,d in doc('entry-validation.json')['configurations'].items():
            self.assertEqual(6,d['logical_cases']['nonpassed'])
            self.assertEqual(7,d['runtime_executions']['failed'])
            self.assertNotEqual(0,d['full_command_exit'])
            self.assertTrue(d['python']['nonpasses'])

if __name__=='__main__':unittest.main()
