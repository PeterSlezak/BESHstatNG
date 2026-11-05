Option Explicit On
Option Strict On
Option Infer On
Imports System
Imports System.Collections.Generic
Imports System.Linq
Imports BESHStatNG.StatisticalProcessControl

Namespace ExcelCommon
    ''' <summary>Detached output-only native choices; Decimal dimensions retain audit cell types.</summary>
    Public NotInheritable Class SpcMultivariateAuditView
        Public Sub New(chartTitle As String, valueAxisTitle As String, horizontalAxisTitle As String, useSequence As Boolean, width As Decimal, height As Decimal)
            Me.ChartTitle = chartTitle : Me.ValueAxisTitle = valueAxisTitle : Me.HorizontalAxisTitle = horizontalAxisTitle
            Me.UseSequence = useSequence : Me.Width = width : Me.Height = height
        End Sub
        Public ReadOnly Property ChartTitle As String
        Public ReadOnly Property ValueAxisTitle As String
        Public ReadOnly Property HorizontalAxisTitle As String
        Public ReadOnly Property UseSequence As Boolean
        Public ReadOnly Property Width As Decimal
        Public ReadOnly Property Height As Decimal
    End Class

    Public Enum SpcMultivariateReportSection
        Summary
        ModelDetails
        Chart
        ChartData
        Signals
        Diagnostics
        SettingsAudit
    End Enum

    Public NotInheritable Class SpcMultivariateSectionDefinition
        Friend Sub New(section As SpcMultivariateReportSection, name As String)
            Me.Section = section : SheetName = name
        End Sub
        Public ReadOnly Property Section As SpcMultivariateReportSection
        Public ReadOnly Property SheetName As String
    End Class

    Public NotInheritable Class SpcMultivariateSections
        Private Sub New()
        End Sub
        Public Shared Function OrderedDefinitions() As IReadOnlyList(Of SpcMultivariateSectionDefinition)
            Return Array.AsReadOnly({New SpcMultivariateSectionDefinition(SpcMultivariateReportSection.Summary, "Multivariate Summary"),
                New SpcMultivariateSectionDefinition(SpcMultivariateReportSection.ModelDetails, "Model Details"),
                New SpcMultivariateSectionDefinition(SpcMultivariateReportSection.Chart, "Multivariate Chart"),
                New SpcMultivariateSectionDefinition(SpcMultivariateReportSection.ChartData, "Chart Data"),
                New SpcMultivariateSectionDefinition(SpcMultivariateReportSection.Signals, "Signals"),
                New SpcMultivariateSectionDefinition(SpcMultivariateReportSection.Diagnostics, "Diagnostics"),
                New SpcMultivariateSectionDefinition(SpcMultivariateReportSection.SettingsAudit, "Settings and Audit")})
        End Function
        Public Shared Function Definition(section As SpcMultivariateReportSection) As SpcMultivariateSectionDefinition
            Return OrderedDefinitions()(CInt(section))
        End Function
    End Class

    ''' <summary>Original first-seen Ordinal warning union. Empty strings skipped; no trim or case folding.</summary>
    Public NotInheritable Class SpcMultivariateWarningProjection
        Private Sub New(values As List(Of String))
            Me.Values = values.AsReadOnly()
        End Sub
        Public ReadOnly Property Values As IReadOnlyList(Of String)
        Public Shared Function FromResult(result As SpcMultivariateFitResult) As SpcMultivariateWarningProjection
            Return FromWarnings(result.Warnings, result.Panels.Select(Function(panel) CType(panel.Warnings, IEnumerable(Of String))))
        End Function
        Public Shared Function FromWarnings(topLevel As IEnumerable(Of String), panels As IEnumerable(Of IEnumerable(Of String))) As SpcMultivariateWarningProjection
            Dim values As New List(Of String), seen As New HashSet(Of String)(StringComparer.Ordinal)
            For Each warning In topLevel
                If warning.Length > 0 AndAlso seen.Add(warning) Then values.Add(warning)
            Next
            For Each panel In panels
                For Each warning In panel
                    If warning.Length > 0 AndAlso seen.Add(warning) Then values.Add(warning)
                Next
            Next
            Return New SpcMultivariateWarningProjection(values)
        End Function
    End Class

    ''' <summary>Core outcome and request applicability only, never a covariance/limit calculation.</summary>
    Public NotInheritable Class SpcMultivariateFamilyPresentation
        Public Sub New(result As SpcMultivariateFitResult)
            Request = result.Request : Model = result.Model
            Caption = SpcMultivariateReportProjection.ChartTypeText(Request.ChartType)
            Compatibility = SpcMultivariateRequestPolicy.Compatibility(Request.ChartType)
            InputVariables = Array.AsReadOnly(Request.VariableNames)
            AnalysisVariables = InputVariables
            Panels = Array.AsReadOnly(result.Panels)
        End Sub
        Public ReadOnly Property Request As SpcMultivariateRequest
        Public ReadOnly Property Model As SpcMultivariateModelResult
        Public ReadOnly Property Caption As String
        Public ReadOnly Property Compatibility As SpcMultivariateChartCompatibility
        Public ReadOnly Property InputVariables As IReadOnlyList(Of String)
        Public ReadOnly Property AnalysisVariables As IReadOnlyList(Of String)
        Public ReadOnly Property Panels As IReadOnlyList(Of SpcPanelResult)
        Public ReadOnly Property HasSubgroups As Boolean
            Get
                Return Request.HasSubgroups
            End Get
        End Property
        Public ReadOnly Property UsesCorrelationScale As Boolean
            Get
                Return Compatibility.IsPca AndAlso Request.PcaUseCorrelationMatrix
            End Get
        End Property
        Public ReadOnly Property HasSuppliedMewmaLimit As Boolean
            Get
                Return Request.ChartType = SpcChartType.Mewma AndAlso Request.MewmaControlLimit.HasValue
            End Get
        End Property
    End Class

    ''' <summary>One defensive Core getter capture per selected diagnostic/optional array.
    ''' Internal arrays are borrowed only by the table builder; no form-owned array is retained.</summary>
    Friend NotInheritable Class SpcMultivariateDiagnosticSnapshot
        Private ReadOnly _point As SpcMultivariatePointDiagnostic
        Public Sub New(point As SpcMultivariatePointDiagnostic)
            _point = point
            ObservationVector = point.ObservationVector : StateVector = point.StateVector
            ComponentScores = point.ComponentScores : ResidualVector = point.ResidualVector
            Contributions = point.Contributions : SubgroupCovariance = point.SubgroupCovariance
        End Sub
        Public ReadOnly Property ObservationVector As Double()
        Public ReadOnly Property StateVector As Double()
        Public ReadOnly Property ComponentScores As Double()
        Public ReadOnly Property ResidualVector As Double()
        Public ReadOnly Property Contributions As Double()
        Public ReadOnly Property SubgroupCovariance As Double(,)
        Public ReadOnly Property PointIndex As Integer
            Get
                Return _point.PointIndex
            End Get
        End Property
        Public ReadOnly Property SourceRowIndices As Integer()
            Get
                Return _point.SourceRowIndices
            End Get
        End Property
        Public ReadOnly Property Label As String
            Get
                Return _point.Label
            End Get
        End Property
        Public ReadOnly Property StageId As String
            Get
                Return _point.StageId
            End Get
        End Property
        Public ReadOnly Property Phase As SpcPhase
            Get
                Return _point.Phase
            End Get
        End Property
        Public ReadOnly Property Statistic As Double
            Get
                Return _point.Statistic
            End Get
        End Property
        Public ReadOnly Property EffectiveSampleSize As Double
            Get
                Return _point.EffectiveSampleSize
            End Get
        End Property
        Public ReadOnly Property ContributionTotal As Double
            Get
                Return _point.ContributionTotal
            End Get
        End Property
        Public ReadOnly Property ContributionBasis As String
            Get
                Return _point.ContributionBasis
            End Get
        End Property
    End Class
End Namespace
