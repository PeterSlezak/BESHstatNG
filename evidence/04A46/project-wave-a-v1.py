from common46 import *
import re
lock();census=load(HERE/'scope/family-census-v1.json');assert load(HERE/'entry/compiled-identities-v1.json')['rows']
chosen={
'UibyID':['checkInputs','GetHomogeneityMethods','optOutputRange_Click','optWorksheet_Click','optWorkbook_Click','optByColumn_Click','optByID_Click','cmbCDFPlotType_SelectedIndexChanged','ckCDFPercentileLines_CheckedChanged','ckCDFShowLegend_CheckedChanged','UpdateCDFOptionState','cmdBoxMean_CheckedChanged','cmbBoxPalette_SelectedIndexChanged','UpdateBoxPlotOptionState','ViolinBandwidth_SelectedIndexChanged','UpdateViolinBandwidthOptionState','CategoricalHistogramPlotType_CheckedChanged','UpdateCategoricalHistogramOptionState','UnpairedTtestHypothesis_CheckedChanged','ApplyUnpairedTtestInputLabels','UpdateUnpairedTtestOptionVisibility','ValidateUnpairedTtestOptionInputs'],
'Ui0OneRefeditMulticol':['checkInputs','optOutputRange_Click','optWorksheet_Click','optWorkbook_Click'],
'UiTwoInputRefedits':['checkInputs','ApplyDemingControlState','DemingControlStateChanged','ApplyLinCCCControlState','optLinCCCAnalytical_CheckedChanged','ApplyKappaControlState','optKappaAnalytical_CheckedChanged','optOutputRange_Click','optWorksheet_Click','optWorkbook_Click'],
'Ui82x2':['optWorksheetData_Click','optScreenData_Click','optOutputRange_Click','optWorksheet_Click','optWorkbook_Click'],
'Ui8Proportions':['optSingle_CheckedChanged','optIndependent_CheckedChanged','optPaired_CheckedChanged','HypothesisTypeChanged','ValidateIndependentMargin','UpdateIndependentHypothesisUi','optOutputRange_Click','optWorksheet_Click','optWorkbook_Click'],
'Ui9ANOVA2nested':['checkInputs','UpdateBlandAltmanOptionState','ApplyBlandDecisionLimitState','optOutputRange_Click','optWorksheet_Click','optWorkbook_Click'],
'Ui12SampleSizeTtestSingleProp':[],
'Ui3XYZplot':['spinBtnRotationX_ValueChanged','spinBtnRotationZ_ValueChanged','spinBtnZoom_ValueChanged','spinBtnShiftX_ValueChanged','spinBtnShiftY_ValueChanged','btResetView_Click','spinBtnXYPlanePointSize_ValueChanged','spinBtnYZPlanePointSize_ValueChanged','spinBtnXZPlanePointSize_ValueChanged','spinBtnLabelFontSize_ValueChanged','ckGridlines_CheckedChanged','spinBtnMarkerSize_ValueChanged','cbPointLabelPosition_SelectedIndexChanged','ckDataPointLabels_CheckedChanged','ckXYplanePoints_CheckedChanged','ckYZplanePoints_CheckedChanged','ckXZplanePoints_CheckedChanged','ckZdropLines_CheckedChanged'],
'Ui01BulletChart':['optOutputRange_CheckedChanged','optWorksheet_CheckedChanged','optWorkbook_CheckedChanged','UpdateOutputRangeState','ckShowScaleLabels_CheckedChanged','ckShowActualValueLabels_CheckedChanged','ckShowTargetValueLabels_CheckedChanged','UpdateOptionControlStates','GetPlotOptions','GetComboValue','ValidateOptionalNumericFormat'],
'Ui01ConvexHullPlot':['optOutputRange_CheckedChanged','optWorksheet_CheckedChanged','optWorkbook_CheckedChanged','UpdateOutputRangeState','GetPlotOptions'],
'Ui01DumbbellPlot':['optOutputRange_CheckedChanged','optWorksheet_CheckedChanged','optWorkbook_CheckedChanged','UpdateOutputRangeState','GetPlotOptions','GetComboValue'],
'Ui01LadderPlot':['optOutputRange_CheckedChanged','optWorksheet_CheckedChanged','optWorkbook_CheckedChanged','UpdateOutputRangeState','ckHorizontalJitter_CheckedChanged','ckShowLines_CheckedChanged','ckShowMarkers_CheckedChanged','cmbColorMode_SelectedIndexChanged','ckShowFirstEndpointLabels_CheckedChanged','ckShowSecondEndpointLabels_CheckedChanged','UpdateOptionControlStates','GetPlotOptions'],
'Ui01PolarPlot':['optOutputRange_CheckedChanged','optWorksheet_CheckedChanged','optWorkbook_CheckedChanged','UpdateOutputRangeState','GetPlotOptions'],
'Ui01SankeyPlot':['optOutputRange_CheckedChanged','optWorksheet_CheckedChanged','optWorkbook_CheckedChanged','UpdateOutputRangeState','ckShowNodeLabels_CheckedChanged','cmbNodeLabelMode_SelectedIndexChanged','UpdateLabelControlState','GetPlotOptions','GetSelectedNodeOrder','GetSelectedVerticalAlignment','GetSelectedRibbonColorMode','GetSelectedNodeLabelMode','GetSelectedPercentageBasis']}
save('scope/sealed-selection-wave-a-v1.json',dict(kind='CURRENT_MAIN_PRE_EDIT_04A46_SOURCE_SELECTION_BEFORE_NEW_BEHAVIOR_TESTS',selection=chosen,baseline=pin(HERE/'scope/family-census-v1.json'),vector_policy='Two valid option vectors;reversal;all selected event clauses;explicit rejected/boundary vectors. Native range acquisition,form initialization,automatic radio exclusivity and renderer/dialog paths remain pending. No previously observed native result is invented.'))
def block(source,name):
 m=re.search(r'(?ms)^    (?:Private|Public|Friend)(?: Shared)? (Sub|Function) '+re.escape(name)+r'(?:\(|\b).*?^    End \1',source);assert m,name
 return m.group()
records=[];code='''Option Strict On
Option Explicit On
Imports System
Imports System.Drawing
Imports System.Globalization
Imports BESHStatNG.ExcelCommon
Imports ComboBox = BESHStatNG.Test.Harness46.FakeControl46
Namespace Harness46
'''
for owner,names in chosen.items():
 file=next(r['path'] for r in census['members'] if r['owner']=='BESHStatNG.'+owner and not r['path'].endswith('.Designer.vb'));source=(ROOT/file).read_text(encoding='utf-8-sig');parts=[block(source,n) for n in names]
 extras=''
 if owner in ['Ui01BulletChart','Ui01DumbbellPlot','Ui01LadderPlot']:
  m=re.search(r'(?ms)^    Private NotInheritable Class ComboItem\(Of T\).*?^    End Class',source);assert m;extras=m.group()+'\n'
 if owner=='UibyID':extras+=' Friend Const BoxPlotLegacyPaletteIndex As Integer = 4\n'
 if owner=='Ui0OneRefeditMulticol':extras+=' Private Function CheckKiteInputRange() As Boolean\n  Throw New InvalidOperationException("HOST_BOUNDARY_KITE_RANGE")\n End Function\n'
 if owner=='Ui3XYZplot':extras+=' Friend pFigure As Object\n Friend pbPreventEvents As Boolean\n Friend Recalculations As Integer\n Private Sub Recalculate()\n  Recalculations += 1\n End Sub\n'
 full='\n'.join(parts);fields=set(re.findall(r'\bMe\.([A-Za-z_]\w*)\.',full));fields.update(re.findall(r'\bHandles\s+([A-Za-z_]\w*)\.',full))
 for handles in re.findall(r'(?ms)\bHandles\s+(.*?)(?=\n\s*(?:If|Me\.|Dim|Select|Return|$))',full):fields.update(re.findall(r'\b(\w+)\.(?:Click|CheckedChanged|SelectedIndexChanged|ValueChanged)',handles))
 fields-={'Tag','Text'};fields-=set(names)
 code+='Friend Class '+owner+'Projection46\n Friend Tag As HelpTopic\n Friend Text As String = ""\n'+extras
 for field in sorted(fields):
  t='FakeRefEdit46' if field.lower().startswith('refedit') else 'FakeStringControl46' if owner=='Ui8Proportions' and field=='cbHypothesisType' else 'FakeControl46'
  code+=' Friend WithEvents '+field+' As New '+t+'()\n'
 if owner=='Ui8Proportions':code+=' Friend Sub New()\n  AddHandler Me.cbHypothesisType.SelectedIndexChanged, AddressOf Me.HypothesisTypeChanged\n End Sub\n'
 for n,p in zip(names,parts):
  transformed=p.replace('MsgBox(', 'FakeMessages46.Record(')
  code+=transformed+'\n'
  records.append(dict(owner=owner,method=n,path=file,source=pin(file),source_text_sha256=sha(p.encode()),original_text=p,transforms=[['MsgBox(','FakeMessages46.Record(']],field_names=sorted(fields),classification='SOURCE_DERIVED_FAKE_ELIGIBLE',materialization='CURRENT_MAIN_PRE_EDIT_04A46;native not captured'))
 code+='End Class\n'
code+='End Namespace\n'
target=ROOT/'BESHStatNG.Test/src/GuiWiring46Projections.vb'
with target.open('x',encoding='utf-8',newline='\n') as f:f.write(code)
save('fixtures/projections-wave-a-v1.json',dict(kind='CURRENT_MAIN_PRE_EDIT_04A46_SOURCE_DERIVED_PROJECTION',members=records,projection=pin(target),constructors='No real form constructor. Ui8 dynamic AddHandler copied exactly;fake state vectors are explicit,not native default captures.',host_boundaries='CheckRefEdit/RefEditReset mocked selection facts;XYZ Recalculate records request count only;MsgBox records strings;all renderer/import/writer APIs unreachable.',native='NOT_RUN'))
print('Wave A projected',len(records),'exact methods across14 owner shells',flush=True)
