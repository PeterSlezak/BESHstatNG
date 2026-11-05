"""Generate host-free legacy test code solely from the frozen entry method bodies.

Outputs a local draft, never compiles it into the entry test measurement.
Only native acquisition, inert grid storage and visibility are adapted.
"""
import hashlib
import importlib.util
import json
from pathlib import Path
import re

ROOT=Path(__file__).resolve().parents[1]
spec=importlib.util.spec_from_file_location('c23',ROOT/'scripts/capture-04a23-contract.py')
c=importlib.util.module_from_spec(spec);spec.loader.exec_module(c)

def generate():
    provenance=json.loads((c.OUT/'prechange-method-provenance.json').read_bytes())
    bodies={n:r['body'] for n,r in provenance['methods'].items()}
    names=['BuildPointAssignments','BuildPointExclusions','NormalizeGroupedMetadata','GenerateStageIdsFromPhaseRuns','GridRowIsBlank','CellText','RequiredIntegerCell','RequiredDoubleCell','TryParseDouble','ParsePhaseText','ParseCollapsedPhase','CollapsedTextValue','ParseExclusionScope','IsExclusionIndicator']
    header='''Option Explicit On
Option Strict On
Option Infer On
Imports System
Imports System.Collections.Generic
Imports System.Globalization
Imports System.Linq
Imports BESHStatNG.StatisticalProcessControl
Imports BESHStatNG

' Frozen entry-source characterization, NOT_GOLDEN. No WinForms/Excel references.
Public Class Spc23LegacyPreparation
    Public Class DataGridViewCell
        Public Property Value As Object
    End Class
    Public Class DataGridViewRow
        Public Property IsNewRow As Boolean
        Public ReadOnly Cells As New List(Of DataGridViewCell)
        Public Sub New(ParamArray values As Object())
            For Each value In values : Cells.Add(New DataGridViewCell With {.Value = value}) : Next
        End Sub
    End Class
    Public Class Grid
        Public ReadOnly Rows As New List(Of DataGridViewRow)
    End Class
    Public Class Column
        Public Sub New(index As Integer)
            Me.Index = index
        End Sub
        Public ReadOnly Index As Integer
    End Class
    Public ReadOnly dgvStages As New Grid
    Public ReadOnly dgvExclusions As New Grid
    Public ReadOnly colStageID As New Column(0)
    Public ReadOnly colStageFirstPoint As New Column(1)
    Public ReadOnly colStageLastPoint As New Column(2)
    Public ReadOnly colStagePhase As New Column(3)
    Public ReadOnly colExclusionPoint As New Column(0)
    Public ReadOnly colExclusionScope As New Column(1)
    Public ReadOnly colExclusionReason As New Column(2)
    Public Class InputRowContext
        Public Property FirstRow As Integer
        Public Property LastRow As Integer
        Public Property PointIndexByRow As Integer()
        Public Property PointCount As Integer
        Public Property IsGrouped As Boolean
        Public ReadOnly Property RowCount As Integer
            Get
                Return LastRow - FirstRow + 1
            End Get
        End Property
    End Class
    Public Class PointAssignments
        Public Property StageIds As String()
        Public Property Phases As SpcPhase()
    End Class
    Public Class PointExclusions
        Public Property Scopes As SpcExclusionScope()
        Public Property Reasons As String()
    End Class
'''
    code=header+'\n\n'.join(bodies[n].replace('Private ', 'Public ',1) for n in names)+'\n'
    body=bodies['GetInputRowContext']
    start=body.index('        Dim pointIndices(');end=body.index('        spinLastPhaseIPoint.Maximum')
    mapping=body[start:end].replace('Dim subgroupIds As String() = ReadTextColumn(ColumnInfo(CStr(lbSubgroupID.Items(0))), firstRow, lastRow)','Dim subgroupIds As String() = ids')
    code+='''    Public Shared Function Map(firstRow As Integer, lastRow As Integer, grouped As Boolean, ids As String()) As InputRowContext
        Dim rowCount = lastRow - firstRow + 1
'''+mapping+'''        Return New InputRowContext With {.FirstRow=firstRow,.LastRow=lastRow,.IsGrouped=grouped,.PointCount=pointCount,.PointIndexByRow=pointIndices}
    End Function
'''
    body=bodies['btImportStages_Click']
    start=body.index('            Dim pointStages(');end=body.index('            ClearQuickPhaseSelection()',start)
    imported=body[start:end].replace('phaseInfo Is Nothing','rowPhases Is Nothing').replace('stageInfo IsNot Nothing','rowStages IsNot Nothing').replace('stageInfo Is Nothing','rowStages Is Nothing')
    code+='''    Public Sub Imported(context As InputRowContext, rowStages As String(), rowPhases As String())
'''+imported+'''    End Sub
    Public Sub AddStageRow(id As String, first As Integer, last As Integer, phase As SpcPhase)
        dgvStages.Rows.Add(New DataGridViewRow(id,first,last,If(phase=SpcPhase.PhaseI,"Phase I","Phase II")))
    End Sub
'''
    body=bodies['btApplyQuickPhaseSetup_Click']
    start=body.index('            If rbSinglePhaseI.Checked Then');end=body.index('        Catch ex',start)
    quick=body[start:end].replace('rbSinglePhaseI.Checked','allPhaseI').replace('rbSinglePhaseII.Checked','allPhaseII').replace('SelectedComboValue(Of SpcMultivariateModelSource)(cbModelSource)','modelSource').replace('CInt(spinLastPhaseIPoint.Value)','lastPhaseIValue')
    code+='''    Public Sub Quick(context As InputRowContext, allPhaseI As Boolean, allPhaseII As Boolean, lastPhaseIValue As Integer, modelSource As SpcMultivariateModelSource)
'''+quick+'''    End Sub
'''
    body=bodies['BuildRequest']
    start=body.index('        Dim phases(context.RowCount');end=body.index('        Dim modelSource',start)
    expansion=body[start:end]
    code+='''    Public Class RowArrays
        Public Phases As SpcPhase()
        Public Stages As String()
        Public Scopes As SpcExclusionScope()
        Public Reasons As String()
        Public SourceRows As Integer()
    End Class
    Public Shared Function Expand(context As InputRowContext, pointAssignments As PointAssignments, pointExclusions As PointExclusions) As RowArrays
'''+expansion+'''        Return New RowArrays With {.Phases=phases,.Stages=stages,.Scopes=scopes,.Reasons=reasons,.SourceRows=sourceRows}
    End Function
'''
    corecall=body[body.index('        Return New SpcMultivariateRequest('):body.rindex('    End Function')]
    replacements={'SelectedChartType()':'chart','SelectedComboValue(Of SpcMissingValuePolicy)(cbMissingValuePolicy)':'options.MissingValuePolicy','modelSource:=modelSource':'modelSource:=options.ModelSource','CDbl(spinControlLimitAlpha.Value)':'options.Alpha','chkUseLowerHotellingLimit.Checked':'options.LowerLimit','CDbl(spinCovarianceRegularization.Value)':'options.Ridge','chkAllowPseudoInverse.Checked':'options.AllowPseudo','SelectedComboValue(Of Boolean)(cbPcaMatrix)':'options.Correlation','pcaComponentCount:=pcaCount':'pcaComponentCount:=If(options.FixedComponents, CType(options.Components,Integer?), Nothing)','CDbl(spinPcaCumulativeVariance.Value)':'options.CumulativePercent','CDbl(spinMewmaLambda.Value)':'options.Lambda','mewmaControlLimit:=mewmaLimit':'mewmaControlLimit:=If(options.SuppliedMewma, CType(options.MewmaLimit,Double?), Nothing)','CDbl(spinMcusumReferenceValue.Value)':'options.K','CDbl(spinMcusumDecisionInterval.Value)':'options.H','generalizedVarianceSigmaMultiplier:=gvMultiplier':'generalizedVarianceSigmaMultiplier:=If(options.SuppliedGv, CType(options.GvSigma,Double?), Nothing)','chkResetAtStageBoundary.Checked':'options.ResetStage','chkResetAtPhaseBoundary.Checked':'options.ResetPhase','chkResetAfterSignal.Checked':'options.ResetSignal','Me.Text':'options.RequestLabel','SelectedComboValue(Of SpcSequenceGapBehavior)(cbSequenceGapBehavior)':'options.Gap'}
    for old,new in replacements.items():
        assert old in corecall,old
        corecall=corecall.replace(old,new)
    code+='''    Public Class RawOptions
        Public MissingValuePolicy As SpcMissingValuePolicy = SpcMissingValuePolicy.Reject
        Public ModelSource As SpcMultivariateModelSource
        Public Alpha As Double = 0.0027R
        Public LowerLimit As Boolean
        Public Ridge As Double
        Public AllowPseudo As Boolean = True
        Public Correlation As Boolean
        Public FixedComponents As Boolean
        Public Components As Integer = 1
        Public CumulativePercent As Double = 90
        Public Lambda As Double = 0.2R
        Public SuppliedMewma As Boolean
        Public MewmaLimit As Double = 12
        Public K As Double = 0.5R
        Public H As Double = 5.5R
        Public SuppliedGv As Boolean
        Public GvSigma As Double = 3
        Public ResetStage As Boolean = True
        Public ResetPhase As Boolean = True
        Public ResetSignal As Boolean
        Public RequestLabel As String = "Multivariate SPC fixture"
        Public Gap As SpcSequenceGapBehavior
    End Class
    Public Shared Function Compose(chart As SpcChartType, measurements As Double(,), variableNames As String(), subgroupIds As String(), labels As String(), phases As SpcPhase(), stages As String(), sequences As Double(), sourceRows As Integer(), scopes As SpcExclusionScope(), reasons As String(), historicalMean As Double(), historicalCovariance As Double(,), options As RawOptions) As SpcMultivariateRequest
'''+corecall+'''    End Function
'''
    body=bodies['BuildChartRenderingResult'];start=body.index('        Dim points As SpcPointResult()');end=body.index('        Dim title As String',start)
    render=body[start:end].replace('panels(0).Points','panel.Points').replace('        Dim input As SpcInputData =','        Return')
    code+='''    Public Shared Function RenderingInput(panel As SpcPanelResult) As SpcInputData
'''+render+'''    End Function
'''
    body=bodies['BuildDiagnosticTables'];start=body.index('        Dim diagnostics As');end=body.index('        Dim names As',start)
    diag=body[start:end].replace('        Dim signalOnly As Boolean = SelectedComboValue(Of DiagnosticsScope)(cbDiagnosticsScope) = DiagnosticsScope.SignalledPointsOnly\n','')
    code+='''    Public Shared Function SelectedDiagnostics(result As SpcMultivariateFitResult, signalOnly As Boolean) As List(Of SpcMultivariatePointDiagnostic)
'''+diag+'''        Return selected
    End Function
End Class
'''
    table_names=['BuildSummaryTables','BuildModelTables','BuildChartDataTable','BuildSignalsTable','BuildDiagnosticTables','BuildAuditTables','BuildWarningsTable','CollectWarnings','CreateTable','CreateVectorTable','CreateIndexedVectorTable','CreateMatrixTable','ComponentNames','AddVectorValues','Row','PhaseText','MissingPolicyText','ModelSourceText','SequenceGapBehaviorText','ChartTypeText','YesNo','NullableValue','JoinIntegers','ExclusionScopeText']
    raw=(c.RUN/'preimages/Ui22MultivariateControlCharts.vb').read_text(encoding='utf-8-sig')
    table_blocks=[m.group() for m in re.finditer(r'^    (?:Private|Public) (?:Shared )?(Sub|Function) (\w+)\b.*?^    End \1',raw,re.M|re.S) if m.group(2) in table_names]
    tables='\n\n'.join(b.replace('Private ','Public ',1) for b in table_blocks)
    tables=tables.replace('SelectedComboValue(Of DiagnosticsScope)(cbDiagnosticsScope) = DiagnosticsScope.SignalledPointsOnly','SignalOnly')
    stubs='''    Public Class AuditControlSnapshot
        Public Text As String = ""
        Public Checked As Boolean
        Public Value As Decimal
    End Class
    Public ReadOnly tbChartTitle As New AuditControlSnapshot
    Public ReadOnly tbValueAxisTitle As New AuditControlSnapshot
    Public ReadOnly tbHorizontalAxisTitle As New AuditControlSnapshot
    Public ReadOnly chkUseSequenceValuesForHorizontalAxis As New AuditControlSnapshot
    Public ReadOnly spinChartWidth As New AuditControlSnapshot With {.Value=760D}
    Public ReadOnly spinChartHeight As New AuditControlSnapshot With {.Value=360D}
'''
    code=code.rsplit('End Class\n',1)[0]+'    Public Property SignalOnly As Boolean\n'+stubs+tables+'\nEnd Class\n'
    destination=c.RUN/'drafts/Spc23LegacyPreparation-v3.vb';destination.parent.mkdir(exist_ok=True)
    with destination.open('x',encoding='utf-8',newline='\n') as f:f.write(code)
    c.emit('legacy-preparation-provenance.json',dict(source=c.FORM,source_sha256=provenance['source_sha256'],derivative='BESHStat.Excel.Common.Tests/src/Spc23LegacyPreparation.vb',derivative_sha256=hashlib.sha256(code.encode()).hexdigest(),method_hashes={n:provenance['methods'][n]['sha256'] for n in set(names+table_names+['GetInputRowContext','btImportStages_Click','btApplyQuickPhaseSetup_Click','BuildRequest','BuildChartRenderingResult','BuildDiagnosticTables'])},table_method_variants=[dict(name=re.search(r'\b(?:Function|Sub) (\w+)',b)[1],sha256=hashlib.sha256(b.encode()).hexdigest()) for b in table_blocks],adaptation='Visibility widened; inert managed grid/cell storage replaces WinForms storage only; worksheet acquisition injected as typed arrays/context. Original loops, errors, predicates/order and Core constructor expression preserved. Scalar control reads replaced by plain RawOptions fields; direct original percent /100 retained. Rendering-input and diagnostic-selection loops extracted unchanged. Table builders copied with only diagnostic selection control replaced by a Boolean property. No new Common policy is imported by oracle.',status='NOT_GOLDEN / SAME_CORE_COMPATIBILITY_ONLY',draft='Local drafts v1 and v2 retained until entry suites finish; no new TestClass in entry assemblies.'))
    print('Generated frozen host-free legacy draft',len(code),'characters')

if __name__=='__main__':generate()
