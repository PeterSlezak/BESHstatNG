"""Actual six-route successor contracts; predecessor assertions remain immutable."""
import importlib.util
import json
from pathlib import Path
import re
import subprocess
import unittest

ROOT=Path(__file__).resolve().parents[2]
s=importlib.util.spec_from_file_location('contract17',ROOT/'scripts/capture-04a17-contract.py')
c=importlib.util.module_from_spec(s);s.loader.exec_module(c)


def form():
    return c.members(c.FORM,(ROOT/c.FORM).read_text(encoding='utf-8-sig').replace('\r\n','\n'))


class Multivariate17SourceContracts(unittest.TestCase):
    def test_live_base_branch_index_and_protected_sources(self):
        self.assertEqual('codex/04A-17',c.git('branch','--show-current'))
        self.assertEqual(c.BASE,c.git('rev-parse','HEAD'));self.assertEqual(c.BASE,c.git('rev-parse','main'))
        self.assertEqual('',c.git('diff','--cached','--name-only'))
        c.source()

    def test_actual_six_handlers_delegate_and_keep_one_calculation(self):
        for name in ['RunPCA','RunMCA','RunKmeans','RunHierarchicalClustering','RunFA','RunDiscriminantAnalysis']:
            body=form()[name];old=c.members(c.FORM,c.original(c.FORM))[name]
            for token in ['WindowsMultivariateWorkflowAdapter.Capture(', 'MultivariateReportPolicy.Project(', 'LegacyTables()', 'New ProcessListofResultTables(', 'writeToSheet(']:self.assertIn(token,body,name)
            self.assertEqual(len(re.findall(r'\.(?:Fit|Calculate)\(',old)),len(re.findall(r'\.(?:Fit|Calculate)\(',body)),name)
            self.assertLess(body.index('WorkflowAdapter.Capture('),body.index('.Calculate(') if '.Calculate(' in body else body.index('.Fit('))
            self.assertLess(body.index('.Calculate(') if '.Calculate(' in body else body.index('.Fit('),body.index('Workbooks.Add('))
            self.assertIn('selectedKeys:=',body)
        for n,call in [('RunPCA','RenderPca'),('RunMCA','RenderMca'),('RunFA','RenderFactor')]:self.assertIn('WindowsMultivariateWorkflowAdapter.'+call+'(report,',form()[n])
        self.assertIn('MultivariateReportPolicy.PlotPlan(intent.Kind',form()['RunHierarchicalClustering'])

    def test_native_writer_statements_and_spm_dispatch_stay_identical(self):
        old=c.members(c.FORM,c.original(c.FORM));new=form()
        for name in ['RunSPM','btCalculate_Click','New','Populate','ImportKMeansStartingCenters']:
            self.assertEqual(old[name],new[name],name)
        pattern=r'^\s*(?:(?:Dim \w+ As .*|\w+) = .*Workbooks\.Add|.*(?:ActiveWorkbook\.|Worksheets\.|\.Name = |\.wb = |\.ws = |WriteRes\.(?:write|set|shift)|wrData\.write|rr\.writeToSheet|chartWidth:=|chartHeight:=|chartTitle:=|topLeftCellAddress:=)).*$'
        for n in ['RunPCA','RunMCA','RunKmeans','RunHierarchicalClustering','RunFA','RunDiscriminantAnalysis']:
            self.assertEqual(re.findall(pattern,old[n],re.M),re.findall(pattern,new[n],re.M),n)

    def test_acquisition_error_order_and_legacy_grammars_preserved(self):
        new=form()
        self.assertIn('ImportInto(MyData, ref, False, 20000)',new['GetData'])
        self.assertIn('MyData.bAllowMissing = True',new['GetData'])
        self.assertIn('ImportInto(MyData, ref, False, 0, skipRows)',new['GetDiscriminantData'])
        self.assertIn('importKeys.Add(groupingKey)',new['GetDiscriminantData'])
        self.assertIn('bStartRow:=True',new['ImportKMeansStartingCenters'])
        self.assertIn('SubsetByRowIdValues(ids)',new['RunMCA'])
        self.assertLess(new['RunMCA'].index('SubsetByRowIdValues(ids)'),new['RunMCA'].index('WorkflowAdapter.Capture('))
        for n in ['ReadPcaAuxiliaryColumn','GetSelectedKMeansRowLabels','GetSelectedDiscriminantRowLabels']:
            self.assertIn('Cells(MyData.RowIds(i), info.ColumnNumber).Value',new[n]);self.assertIn('MultivariateWorkflowPolicy.',new[n])
        self.assertIn('ParseUiInteger(Me.tbKmeansSeed.Text, "Random seed")',new['RunKmeans'])
        self.assertEqual(c.members(c.FORM,c.original(c.FORM))['GetSelectedDiscriminantRandomSeed'],new['GetSelectedDiscriminantRandomSeed'])
        self.assertIn('MultivariateLegacyChoices.ParsePriors(',new['GetDiscriminantUserPriors'])
        self.assertLess(new['RunDiscriminantAnalysis'].index('options.ConfigureModel('),new['RunDiscriminantAnalysis'].index('GetDiscriminantUserPriors('))
        self.assertLess(new['RunDiscriminantAnalysis'].index('priorInputs('),new['RunDiscriminantAnalysis'].index('options.ConfigureValidation('))

    def test_exact_method_migration_and_frozen_oracle(self):
        old=c.members(c.FORM,c.original(c.FORM));new=form()
        oracle=json.loads((c.OUT/'legacy-selection-provenance.json').read_bytes())
        choice=set(oracle['original_methods'])
        expected=choice|{'RunDiscriminantAnalysis','RunFA','RunHierarchicalClustering','RunKmeans','RunMCA','RunPCA','GetData','GetDiscriminantData','ReadPcaAuxiliaryColumn','GetSelectedKMeansRowLabels','BuildKMeansInputDataTable','GetSelectedFARetentionValue','BuildDiscriminantInputDataTable','GetSelectedDiscriminantRowLabels','GetDiscriminantUserPriors'}
        self.assertEqual(expected,{n for n in old if old[n]!=new[n]})
        self.assertEqual(oracle['derived_sha256'],c.sha(ROOT/'BESHStat.Excel.Common.Tests/src/LegacyMultivariate17Selection.vb'))
        for n in choice:self.assertIn('MultivariateLegacyChoices.'+n+'(',new[n])

    def test_common_ownership_and_no_new_native_refusal(self):
        for path in ['Analysis/MultivariateLegacyChoices.vb','Analysis/MultivariateWorkflowPolicy.vb','Results/MultivariateReportPolicy.vb']:
            text=(ROOT/'BESHStat.Excel.Common/src'/path).read_text()
            self.assertIn('Option Strict On',text)
            for token in ['Imports Microsoft.VisualBasic','System.Windows.Forms','Interop.Excel','DataObj','AppGlobals','RefEdit','System.IO',r'\.Fit\(',r'\.Calculate\(']:
                self.assertNotRegex(text,token)
        text=(ROOT/'BESHStat.Excel.Common/src/Analysis/MultivariateWorkflowPolicy.vb').read_text()
        self.assertIn('blocking:=False',text)
        for method in form().values():self.assertNotIn('.Validation.IsValid',method)
        report=(ROOT/'BESHStat.Excel.Common/src/Results/MultivariateReportPolicy.vb').read_text()
        self.assertIn('For Each scaling In {0.0, 0.5, 1.0}',report)
        self.assertIn('LinearBinomialReport.Rehydrate',report)

    def test_original_ca_defect_lineage_and_all_frozen_captures(self):
        source=(ROOT/'BESHstatNG/src/UI/Ui0OneRefeditMulticol.vb').read_text(encoding='utf-8-sig').replace('\r\n','\n')
        body=c.members('BESHstatNG/src/UI/Ui0OneRefeditMulticol.vb',source)['RunCA']
        self.assertIn('For j = 0 To UBound(data.FinalData, 2)',body);self.assertIn('TableData(i, j - 1)',body)
        self.assertNotIn('MultivariateReportPolicy',body)
        cross=json.loads((c.OUT/'original-control-crosswalk.json').read_bytes())
        self.assertEqual('04A02-PCA-NATIVE-01',cross['original_case']);self.assertEqual(0,cross['releases'])
        self.assertEqual(186,len(cross['controls']));self.assertEqual(152,len(cross['obligations']))
        registry=json.loads((ROOT/'evidence/04A14/baseline-registry.json').read_bytes())
        self.assertEqual(100,len(registry['files']))
        for path,pin in registry['files'].items():self.assertEqual(pin['sha256'],c.sha(ROOT/path));self.assertEqual(pin['bytes'],(ROOT/path).stat().st_size)

    def test_public_targets_and_successor_hashes(self):
        for p in ['BESHStat.Core/BESHStat.Core.vbproj','BESHStat.Excel.Common/BESHStat.Excel.Common.vbproj']:
            self.assertIn('<TargetFramework>netstandard2.0</TargetFramework>',(ROOT/p).read_text())
        doc=json.loads((c.OUT/'source-contract.json').read_bytes())
        for p,pin in doc['new_sources'].items():self.assertEqual(pin['sha256'],c.sha(ROOT/p),p)
        for p,pin in doc['changed'].items():self.assertEqual(pin['after'],c.sha(ROOT/p),p)
        project=(ROOT/'BESHstatNG/BESHStatNG.vbproj').read_text(encoding='utf-8-sig')
        self.assertIn('src\\DM\\WindowsMultivariateWorkflowAdapter.vb',project)


if __name__=='__main__':unittest.main()
