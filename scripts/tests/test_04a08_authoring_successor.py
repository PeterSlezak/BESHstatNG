"""Additive source/wiring gates, never native equivalence or control release."""
import importlib.util
from pathlib import Path
import re
import unittest

ROOT=Path(__file__).resolve().parents[2]
spec=importlib.util.spec_from_file_location('contract08',ROOT/'scripts/capture-04a08-contract.py')
c=importlib.util.module_from_spec(spec);spec.loader.exec_module(c)

class Authoring08SuccessorTests(unittest.TestCase):
    def test_shipping_forms_have_all_five_live_event_routes(self):
        for form in c.FORMS:
            text=(ROOT/('BESHstatNG/src/UI/'+form+'.vb')).read_text(encoding='utf-8-sig')
            for operation in ['MainEffects','CategoricalEffects','PolynomialEffects','TwoWayInteractions','CustomInteraction']:
                calls=[body for body in c.members(text).values() if re.search(r'\.Add'+operation+r'FromSelectedVars\(',body)]
                self.assertGreaterEqual(len(calls),2 if form in ['UiGLM','Ui19LMM'] else 1,form+operation)
                for body in calls:self.assertIn('Handles ',body.splitlines()[0])

    def test_actual_component_constructors_are_independent(self):
        expected={'UiGLM':2,'Ui19LMM':2,'Ui13GEE':1,'Ui4Cox':1,'Ui18MMRM':1,'Ui20PropensityScoreMatching':1}
        for form,count in expected.items():
            text=(ROOT/('BESHstatNG/src/UI/'+form+'.vb')).read_text(encoding='utf-8-sig')
            args=re.findall(r'New RegressionEffectsController\((.*?)\)',text,re.S)
            self.assertEqual(count,len(args),form)
            self.assertEqual(count,len(set(re.findall(r'Me\.(\w+)',args[i])[-1] for i in range(count))))

    def test_controller_is_snapshot_projection_message_bridge(self):
        text=(ROOT/'BESHstatNG/src/UI/RegressionEffectsController.vb').read_text(encoding='utf-8-sig')
        self.assertIn('RegressionEffectAuthoringPolicy.Apply(',text)
        self.assertIn('RegressionEffectAuthoringPolicy.Remove(',text)
        self.assertIn('SelectedIndices.Cast(Of Integer)().ToArray()',text)
        self.assertIn('specs.Clear()',text)
        self.assertNotRegex(text,r'Make(?:Interaction|Polynomial|Categorical)EffectKey|New TermSpec|\.Kind\s*=|\.Scale\s*=')

    def test_common_has_no_host_or_io_dependencies(self):
        for name in ['RegressionEffectAuthoringPolicy.vb','RegressionEffectSerialization.vb']:
            text=(ROOT/('BESHStat.Excel.Common/src/Input/'+name)).read_text()
            self.assertIn('Option Strict On',text)
            self.assertNotRegex(text,r'(?i)Imports .*?(?:Windows.Forms|Interop|ExcelDna|Office|System.IO|Microsoft.VisualBasic)|\bListBox\b|\bMsgBox\s*\(')

    def test_original_numerics_and_shipping_forms_are_unchanged(self):
        for p in ['BESHStat.Core/src/RegModels/RegressionDesignCore.vb','BESHStat.Core/src/RegModels/RegressionFormulaParser.vb','BESHStat.Excel.Common/src/Input/ModelInputCompiler.vb']:
            self.assertEqual(c.original(p),(ROOT/p).read_text(encoding='utf-8-sig'))
        for form in c.FORMS:
            p='BESHstatNG/src/UI/'+form+'.vb'
            self.assertEqual(c.original(p),(ROOT/p).read_text(encoding='utf-8-sig'))

    def test_exact_id_crosswalk_retains_original_row_hashes_and_open_counts(self):
        data=c.build()
        self.assertGreater(data['counts']['source_associated_controls'],40)
        self.assertGreater(data['counts']['source_associated_obligations'],0)
        self.assertEqual((1514,802,239,218),tuple(data['counts'][k] for k in ['original_obligations','restricted_live_controls','transferred_obligations','transferred_controls']))
        self.assertEqual((0,0,'NO','NO GO','NO','UNMET'),tuple(data[k] for k in ['released_controls','discharged_obligations','P','N1','N2','C2']))
        for row in data['affected_controls']:
            self.assertEqual(row['control_row_sha256'],c.rowsha(row['control_row']))
            if row['obligation_row']:
                self.assertEqual('OPEN',row['status'])
                self.assertEqual(row['obligation_row_sha256'],c.rowsha(row['obligation_row']))

    def test_removal_delegation_preserves_untyped_lists(self):
        text=(ROOT/'BESHstatNG/src/DM/UIprocedures.vb').read_text(encoding='utf-8-sig')
        body=c.members(text)['Remove_Item']
        self.assertIn('termSpecs IsNot Nothing AndAlso TypeOf lbox Is System.Windows.Forms.ListBox',body)
        before=c.members(c.original('BESHstatNG/src/DM/UIprocedures.vb'))
        after=c.members(text)
        self.assertEqual({'Remove_Item'},{n for n in before if before[n]!=after[n]})
        self.assertTrue(body.endswith(before['Remove_Item'].split('        If lbox Is Nothing OrElse lbox.Items.Count = 0 Then Exit Sub',1)[1]))

    def test_oracles_are_test_only_and_not_numerical_goldens(self):
        project=(ROOT/'BESHStatNG.Test/BESHStatNG.Tests.vbproj').read_text()
        for name in ['LegacyRegressionEffects08.vb','LegacyRegressionRemoval08.vb','RegressionEffects08Tests.vb']:
            self.assertIn(name,project)
            self.assertNotIn(name,(ROOT/'BESHstatNG/BESHStatNG.vbproj').read_text())

if __name__=='__main__':unittest.main()
