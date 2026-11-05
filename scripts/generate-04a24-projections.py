"""One-time reviewed source extraction from frozen live entry, never Core math."""
from pathlib import Path
import hashlib
import re
import importlib.util
import json
ROOT=Path(__file__).resolve().parents[1]
s=importlib.util.spec_from_file_location('capture24',ROOT/'scripts/capture-04a24-contract.py');c=importlib.util.module_from_spec(s);s.loader.exec_module(c)
P=c.RUN/'preimages/Ui22MultivariateControlCharts.vb'
PRELUDE='''Option Explicit On
Option Strict On
Option Infer On
Imports System
Imports System.Collections.Generic
Imports System.Globalization
Imports System.Linq
Imports BESHStatNG
Imports BESHStatNG.StatisticalProcessControl
'''
REPORTS=['BuildSummaryTables','BuildModelTables','BuildChartDataTable','BuildSignalsTable','BuildDiagnosticTables','BuildAuditTables','BuildWarningsTable']
HELPERS=['CreateTable','CreateVectorTable','CreateIndexedVectorTable','CreateMatrixTable','ComponentNames','AddVectorValues','Row','PhaseText','MissingPolicyText','ModelSourceText','SequenceGapBehaviorText','ChartTypeText','YesNo','NullableValue','JoinIntegers','ExclusionScopeText']

def generate():
    raw=P.read_text(encoding='utf-8-sig');blocks=[(m.group(2),m.group()) for m in re.finditer(r'^    (?:Private|Public|Friend) (?:Shared )?(Sub|Function) (\w+)\b.*?^    End \1',raw,re.M|re.S)]
    methods=dict(blocks)
    # Independent legacy adapter: actual current entry table bodies, original
    # accepted 23 context. Only inert native-control reads are injected.
    legacy=PRELUDE+'''Imports BESHStatNG.ExcelCommon
Public NotInheritable Class Spc24LegacyProjection
    Public Class AuditCell
        Public Text As String = ""
        Public Checked As Boolean
        Public Value As Decimal
    End Class
    Public SignalOnly As Boolean
    Public DefaultCaption As String = "Default chart"
    Public ReadOnly tbChartTitle As New AuditCell
    Public ReadOnly tbValueAxisTitle As New AuditCell
    Public ReadOnly tbHorizontalAxisTitle As New AuditCell
    Public ReadOnly chkUseSequenceValuesForHorizontalAxis As New AuditCell
    Public ReadOnly spinChartWidth As New AuditCell With {.Value=760D}
    Public ReadOnly spinChartHeight As New AuditCell With {.Value=360D}
'''
    for name,body in blocks:
        if name in REPORTS+HELPERS+['CollectWarnings','BuildChartRenderingResult']:
            body=body.replace('Private ','Public ',1).replace('SelectedComboValue(Of DiagnosticsScope)(cbDiagnosticsScope) = DiagnosticsScope.SignalledPointsOnly','Me.SignalOnly').replace('SelectedChartChoice().DisplayText','DefaultCaption')
            legacy+=body+'\n\n'
    legacy+='End Class\n'
    dest=ROOT/'BESHStat.Excel.Common.Tests/src/Spc24LegacyProjection.vb'
    with dest.open('x',encoding='utf-8',newline='\n') as f:f.write(legacy)
    c.emit('legacy-projection-provenance.json',dict(source=c.FORM,source_sha256=c.sha(P),derivative=dest.relative_to(ROOT).as_posix(),derivative_sha256=hashlib.sha256(legacy.encode()).hexdigest(),methods={n:hashlib.sha256(b.encode()).hexdigest() for n,b in blocks if n in REPORTS+HELPERS+['CollectWarnings','BuildChartRenderingResult']},adaptations='Visibility widened; six inert audit control cells, diagnostic scope Boolean and explicit selected-caption value replace WinForms state only. Actual entry table/wrapper/warning bodies and accepted SpcMultivariateResultAxisPolicy retained. No new 24 policy reference or Core formula.',classification='NOT_GOLDEN / SAME_CORE_COMPATIBILITY_ONLY'))
    # Production transfer is deliberately source-derived and narrowly reviewed.
    generated=PRELUDE+'''Namespace ExcelCommon
    '''+"''' <summary>Selected sections over one immutable Core fit. Fresh ResultTable objects belong to the caller.</summary>\n"+'''    Public NotInheritable Class SpcMultivariateReportProjection
        Private ReadOnly _result As SpcMultivariateFitResult
        Private ReadOnly _context As Lazy(Of SpcMultivariateResultContext)
        Private ReadOnly _warnings As Lazy(Of SpcMultivariateWarningProjection)
        Public Sub New(result As SpcMultivariateFitResult)
            If result Is Nothing Then Throw New ArgumentNullException(NameOf(result))
            _result = result
            _context = New Lazy(Of SpcMultivariateResultContext)(Function() SpcMultivariateResultAxisPolicy.Project(result))
            _warnings = New Lazy(Of SpcMultivariateWarningProjection)(Function() SpcMultivariateWarningProjection.FromResult(result))
        End Sub
        Public ReadOnly Property Result As SpcMultivariateFitResult
            Get
                Return _result
            End Get
        End Property
        Public ReadOnly Property Context As SpcMultivariateResultContext
            Get
                Return _context.Value
            End Get
        End Property
        Public ReadOnly Property Warnings As SpcMultivariateWarningProjection
            Get
                Return _warnings.Value
            End Get
        End Property
'''
    for name,body in blocks:
        if name not in REPORTS+HELPERS:continue
        body=body.replace('Private ','Public ',1)
        if name in REPORTS:
            signature='result As SpcMultivariateFitResult'
            parameter='signalOnly As Boolean' if name=='BuildDiagnosticTables' else 'view As SpcMultivariateAuditView' if name=='BuildAuditTables' else ''
            body=body.replace(signature,parameter,1)
            nl=body.index('\n');body=body[:nl+1]+'        Dim result = _result\n'+body[nl+1:]
            body=body.replace('BuildWarningsTable(result)','BuildWarningsTable()').replace('CollectWarnings(result)','Me.Warnings.Values')
            if name=='BuildWarningsTable':body=body.replace('Dim warnings As List(Of String)','Dim warnings As IReadOnlyList(Of String)')
            if name=='BuildDiagnosticTables':
                body=body.replace('SpcMultivariateResultAxisPolicy.Project(result)','Me.Context').replace('        Dim signalOnly As Boolean = SelectedComboValue(Of DiagnosticsScope)(cbDiagnosticsScope) = DiagnosticsScope.SignalledPointsOnly\n','')
                body=body.replace('Dim selected = context.SelectDiagnostics(signalOnly)','Dim selected = context.SelectDiagnostics(signalOnly).Select(Function(item) New SpcMultivariateDiagnosticSnapshot(item)).ToList()').replace('As SpcMultivariatePointDiagnostic In selected','As SpcMultivariateDiagnosticSnapshot In selected')
            if name=='BuildModelTables':
                insertion='''        Dim mean = model.ProcessMean
        Dim scale = model.AnalysisScale
        Dim covariance = model.ProcessCovariance
        Dim analysisCovariance = model.AnalysisCovariance
        Dim inverse = model.AnalysisCovarianceInverse
        Dim eigenvalues = model.Eigenvalues
        Dim eigenvectors = model.Eigenvectors
'''
                index=body.index('        If model.ProcessMean IsNot Nothing')
                body=body[:index]+insertion+body[index:]
                for a,b in [('model.ProcessMean','mean'),('model.AnalysisScale','scale'),('model.ProcessCovariance','covariance'),('model.AnalysisCovarianceInverse','inverse'),('model.AnalysisCovariance','analysisCovariance'),('model.Eigenvalues','eigenvalues'),('model.Eigenvectors','eigenvectors')]:
                    # Only following use sites; source getters in capture stay intact.
                    before,tail=body.split(insertion,1);body=before+insertion+tail.replace(a,b)
            if name=='BuildAuditTables':
                for a,b in [('tbChartTitle.Text.Trim()','view.ChartTitle'),('tbValueAxisTitle.Text.Trim()','view.ValueAxisTitle'),('tbHorizontalAxisTitle.Text.Trim()','view.HorizontalAxisTitle'),('chkUseSequenceValuesForHorizontalAxis.Checked','view.UseSequence'),('spinChartWidth.Value','view.Width'),('spinChartHeight.Value','view.Height')]:body=body.replace(a,b)
        generated+='\n'.join('    '+line if line else '' for line in body.splitlines())+'\n\n'
    generated+='    End Class\nEnd Namespace\n'
    dest=ROOT/'BESHStat.Excel.Common/src/Analysis/SpcMultivariateReportProjection.vb'
    with dest.open('x',encoding='utf-8',newline='\n') as f:f.write(generated)
    print('Frozen oracle and exact-source report transfer generated')
if __name__=='__main__':generate()
