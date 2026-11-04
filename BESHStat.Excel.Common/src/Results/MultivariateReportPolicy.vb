Option Explicit On
Option Strict On
Imports System
Imports System.Collections.Generic
Imports System.Linq

Namespace ExcelCommon
    Public Enum MultivariatePlotKind
        Scree
        Scores2D
        Loadings2D
        Biplot
        Scores3D
        Loadings3D
        Correspondence
        Contribution
        Dendrogram
    End Enum
    Public Enum MultivariatePlotDataRole
        CoreEigenvalues
        CoreScores
        CoreLoadings
        CoreBiplot
        CoreCorrespondence
        CoreContributions
        CoreDendrogram
    End Enum

    ''' <summary>Ordered rendering request over existing Core model data. Coordinates,
    ''' biplot scaling mathematics, sign orientation and chart styles stay in their
    ''' established owners; no chart geometry is recomputed by this specification.</summary>
    Public NotInheritable Class MultivariatePlotIntent
        Friend Sub New(kind As MultivariatePlotKind, role As MultivariatePlotDataRole,
                       Optional scaling As Double = 0, Optional axis As Integer = 0, Optional rowContribution As Boolean = False)
            Me.Kind = kind : Me.DataRole = role : Me.Scaling = scaling : Me.Axis = axis : Me.RowContribution = rowContribution
        End Sub
        Public ReadOnly Property Kind As MultivariatePlotKind
        Public ReadOnly Property DataRole As MultivariatePlotDataRole
        Public ReadOnly Property Scaling As Double
        Public ReadOnly Property Axis As Integer
        Public ReadOnly Property RowContribution As Boolean
        Public ReadOnly Property MinimumDimensions As Integer
            Get
                Select Case Kind
                    Case MultivariatePlotKind.Scores3D, MultivariatePlotKind.Loadings3D : Return 3
                    Case MultivariatePlotKind.Scores2D, MultivariatePlotKind.Loadings2D, MultivariatePlotKind.Biplot, MultivariatePlotKind.Correspondence : Return 2
                    Case MultivariatePlotKind.Contribution : Return Axis + 1
                    Case Else : Return 1
                End Select
            End Get
        End Property
    End Class

    Public NotInheritable Class MultivariateReport
        Friend Sub New(input As MultivariateInputIntent, tables As IEnumerable(Of ResultTable), dendrogram As Boolean)
            Me.Input = input
            Document = New AnalysisResultDocument("multivariate." & input.Kind.ToString().ToLowerInvariant(),
                tables.Select(Function(t) t.ToOutputModel()), Array.Empty(Of KeyValuePair(Of String, Double))(), Nothing)
            Plots = MultivariateReportPolicy.PlotPlan(input.Kind, dendrogram)
        End Sub
        Public ReadOnly Property Input As MultivariateInputIntent
        Public ReadOnly Property Document As AnalysisResultDocument
        Public ReadOnly Property Plots As IReadOnlyList(Of MultivariatePlotIntent)
        Public Function LegacyTables() As List(Of ResultTable)
            Return Document.Tables.Select(Function(t) LinearBinomialReport.Rehydrate(t.Model)).ToList()
        End Function
    End Class

    Public NotInheritable Class MultivariateReportPolicy
        Private Sub New()
        End Sub
        Public Shared Function Project(input As MultivariateInputIntent, tables As IEnumerable(Of ResultTable), Optional dendrogram As Boolean = False) As MultivariateReport
            If input Is Nothing Then Throw New ArgumentNullException(NameOf(input))
            If tables Is Nothing Then Throw New ArgumentNullException(NameOf(tables))
            Return New MultivariateReport(input, tables, dendrogram)
        End Function
        Public Shared Function PlotPlan(kind As MultivariateAnalysisKind, Optional dendrogram As Boolean = False) As IReadOnlyList(Of MultivariatePlotIntent)
            Dim plots As New List(Of MultivariatePlotIntent)
            Select Case kind
                Case MultivariateAnalysisKind.PCA
                    plots.Add(New MultivariatePlotIntent(MultivariatePlotKind.Scree, MultivariatePlotDataRole.CoreEigenvalues))
                    plots.Add(New MultivariatePlotIntent(MultivariatePlotKind.Scores2D, MultivariatePlotDataRole.CoreScores))
                    plots.Add(New MultivariatePlotIntent(MultivariatePlotKind.Loadings2D, MultivariatePlotDataRole.CoreLoadings))
                    For Each scaling In {0.0, 0.5, 1.0}
                        plots.Add(New MultivariatePlotIntent(MultivariatePlotKind.Biplot, MultivariatePlotDataRole.CoreBiplot, scaling))
                    Next
                    plots.Add(New MultivariatePlotIntent(MultivariatePlotKind.Scores3D, MultivariatePlotDataRole.CoreScores))
                    plots.Add(New MultivariatePlotIntent(MultivariatePlotKind.Loadings3D, MultivariatePlotDataRole.CoreLoadings))
                Case MultivariateAnalysisKind.MCA
                    plots.Add(New MultivariatePlotIntent(MultivariatePlotKind.Correspondence, MultivariatePlotDataRole.CoreCorrespondence))
                    plots.Add(New MultivariatePlotIntent(MultivariatePlotKind.Contribution, MultivariatePlotDataRole.CoreContributions, axis:=0))
                    plots.Add(New MultivariatePlotIntent(MultivariatePlotKind.Contribution, MultivariatePlotDataRole.CoreContributions, axis:=1))
                Case MultivariateAnalysisKind.CA
                    For Each row In {True, False}
                        For axis = 0 To 1
                            plots.Add(New MultivariatePlotIntent(MultivariatePlotKind.Contribution, MultivariatePlotDataRole.CoreContributions, axis:=axis, rowContribution:=row))
                        Next
                    Next
                    plots.Add(New MultivariatePlotIntent(MultivariatePlotKind.Correspondence, MultivariatePlotDataRole.CoreCorrespondence))
                Case MultivariateAnalysisKind.Factor
                    plots.Add(New MultivariatePlotIntent(MultivariatePlotKind.Scree, MultivariatePlotDataRole.CoreEigenvalues))
                    plots.Add(New MultivariatePlotIntent(MultivariatePlotKind.Loadings2D, MultivariatePlotDataRole.CoreLoadings))
                    plots.Add(New MultivariatePlotIntent(MultivariatePlotKind.Loadings3D, MultivariatePlotDataRole.CoreLoadings))
                Case MultivariateAnalysisKind.Hierarchical
                    If dendrogram Then plots.Add(New MultivariatePlotIntent(MultivariatePlotKind.Dendrogram, MultivariatePlotDataRole.CoreDendrogram))
                ' K-means and DA have report/membership/classification output, no GUI charts.
            End Select
            Return plots.AsReadOnly()
        End Function
    End Class
End Namespace
