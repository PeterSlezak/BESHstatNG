Option Explicit On
Option Strict On

Imports System.Collections.Generic

Namespace ExcelCommon

    ''' <summary>
    ''' Identifies an analysis exposed through the shared spreadsheet application boundary.
    ''' New cross-platform analyses should be registered here and dispatched by
    ''' <see cref="SharedAnalysisApplicationService"/>.
    ''' </summary>
    Public Enum SpreadsheetAnalysisKind
        OneWayAnova = 1
        KruskalWallis = 2
    End Enum

    ''' <summary>
    ''' Base class for strongly typed analysis options. It keeps the generic request
    ''' extensible without relying on late binding or host-specific dictionaries.
    ''' </summary>
    Public MustInherit Class SpreadsheetAnalysisOptions
        ''' <summary>Option validation notes that do not change existing calculation defaults.</summary>
        Public Overridable Function GetWarnings() As IEnumerable(Of AnalysisWarning)
            Return Array.Empty(Of AnalysisWarning)()
        End Function
    End Class

    ''' <summary>
    ''' Options for the one-way ANOVA workflow. These options are statistical/application
    ''' choices only; output destination and Excel formatting remain separate concerns.
    ''' </summary>
    Public Class OneWayAnovaAnalysisOptions
        Inherits SpreadsheetAnalysisOptions

        Public Sub New()
            Alpha = 0.05
        End Sub

        Public Property IncludeWelch As Boolean
        Public Property IncludeFisherLsd As Boolean
        Public Property IncludeBonferroni As Boolean
        Public Property IncludeTukeyKramer As Boolean
        Public Property IncludeGamesHowell As Boolean
        Public Property Alpha As Double

        Public Overrides Function GetWarnings() As IEnumerable(Of AnalysisWarning)
            If Not Double.IsNaN(Alpha) AndAlso Not Double.IsInfinity(Alpha) AndAlso Alpha > 0.0 AndAlso Alpha < 1.0 Then
                Return Array.Empty(Of AnalysisWarning)()
            End If
            Return New AnalysisWarning() {New AnalysisWarning(AnalysisWarningCodes.AlphaDefaulted,
                "Invalid alpha was replaced by the existing default (0.05).", "options.alpha")}
        End Function
    End Class


    ''' <summary>
    ''' Options for the Kruskal-Wallis workflow. The Windows GUI historically always
    ''' produces Dunn's Bonferroni-adjusted pairwise comparisons, while other hosts can
    ''' choose whether to request that supplementary table.
    ''' </summary>
    Public Class KruskalWallisAnalysisOptions
        Inherits SpreadsheetAnalysisOptions

        Public Sub New()
            IncludeDunnComparisons = True
            Alpha = 0.05
        End Sub

        Public Property IncludeDunnComparisons As Boolean
        Public Property Alpha As Double

        Public Overrides Function GetWarnings() As IEnumerable(Of AnalysisWarning)
            If Not Double.IsNaN(Alpha) AndAlso Not Double.IsInfinity(Alpha) AndAlso Alpha > 0.0 AndAlso Alpha < 1.0 Then
                Return Array.Empty(Of AnalysisWarning)()
            End If
            Return New AnalysisWarning() {New AnalysisWarning(AnalysisWarningCodes.AlphaDefaulted,
                "Invalid alpha was replaced by the existing default (0.05).", "options.alpha")}
        End Function
    End Class

    ''' <summary>
    ''' Generic request crossing the shared Excel application boundary. The input is a
    ''' strongly typed host-neutral contract, not an Excel Range, Office.js object, or COM object.
    ''' </summary>
    Public Class SpreadsheetAnalysisRequest

        Public Sub New()
            AnalysisKind = SpreadsheetAnalysisKind.OneWayAnova
        End Sub

        Public Sub New(analysisKind As SpreadsheetAnalysisKind,
                       input As SpreadsheetAnalysisInput,
                       options As SpreadsheetAnalysisOptions)
            Me.AnalysisKind = analysisKind
            Me.Input = input
            Me.Options = options
        End Sub

        Public Property AnalysisKind As SpreadsheetAnalysisKind
        Public Property Input As SpreadsheetAnalysisInput
        Public Property Options As SpreadsheetAnalysisOptions
    End Class

    ''' <summary>
    ''' Stable metric names returned by the generic analysis result. Typed detail objects remain
    ''' available for richer analysis-specific consumers, while hosts that only need headline
    ''' statistics can use this dictionary without knowing the concrete analysis implementation.
    ''' </summary>
    Public NotInheritable Class AnalysisMetricNames

        Private Sub New()
        End Sub

        Public Const FStatistic As String = "FStatistic"
        Public Const PValue As String = "PValue"
        Public Const WelchFStatistic As String = "WelchFStatistic"
        Public Const WelchPValue As String = "WelchPValue"
        Public Const WelchDenominatorDf As String = "WelchDenominatorDf"
        Public Const HStatistic As String = "HStatistic"
        Public Const TieCorrectedHStatistic As String = "TieCorrectedHStatistic"
        Public Const TieCorrectedPValue As String = "TieCorrectedPValue"
    End Class

    ''' <summary>
    ''' Generic analysis result. Result tables and scalar metrics are directly consumable by
    ''' any host; <see cref="Detail"/> carries an optional strongly typed analysis-specific
    ''' result for consumers that also need artifacts such as plot payloads.
    ''' </summary>
    Public Class SpreadsheetAnalysisResult

        ''' <summary>Original four-parameter constructor; retained for binary and source compatibility.</summary>
        Public Sub New(analysisKind As SpreadsheetAnalysisKind,
                       tables As List(Of Global.BESHStatNG.ResultTableOutputModel),
                       metrics As Dictionary(Of String, Double),
                       detail As Object)
            Me.New(analysisKind, tables, metrics, detail, Nothing)
        End Sub

        Public Sub New(analysisKind As SpreadsheetAnalysisKind,
                       tables As List(Of Global.BESHStatNG.ResultTableOutputModel),
                       metrics As Dictionary(Of String, Double),
                       detail As Object,
                       warnings As IEnumerable(Of AnalysisWarning))
            If tables Is Nothing Then Throw New ArgumentNullException(NameOf(tables))
            If metrics Is Nothing Then Throw New ArgumentNullException(NameOf(metrics))

            Me.AnalysisKind = analysisKind
            Me.Tables = tables
            Me.Metrics = metrics
            Me.Detail = detail
            Dim descriptor As IAnalysisDescriptor = Nothing
            Dim analysisId As String = If(AnalysisCatalog.TryGetByLegacyKind(analysisKind, descriptor),
                                          descriptor.Id, "legacy.unknown")
            Dim typedDetail As AnalysisDetailArtifact = Nothing
            If detail IsNot Nothing Then typedDetail = AnalysisDetailArtifact.FromValue(detail)
            Document = New AnalysisResultDocument(analysisId, tables, metrics, typedDetail, warnings)
        End Sub

        Public ReadOnly Property AnalysisKind As SpreadsheetAnalysisKind
        Public ReadOnly Property Tables As List(Of Global.BESHStatNG.ResultTableOutputModel)
        Public ReadOnly Property Metrics As Dictionary(Of String, Double)
        Public ReadOnly Property Detail As Object
        ''' <summary>Immutable typed v1 document; the existing table/metric/detail APIs remain unchanged.</summary>
        Public ReadOnly Property Document As AnalysisResultDocument

        Friend Function WithWarnings(warnings As IEnumerable(Of AnalysisWarning)) As SpreadsheetAnalysisResult
            Return New SpreadsheetAnalysisResult(AnalysisKind, Tables, Metrics, Detail, warnings)
        End Function

        Public ReadOnly Property PrimaryTable As Global.BESHStatNG.ResultTableOutputModel
            Get
                If Tables.Count = 0 Then Return Nothing
                Return Tables(0)
            End Get
        End Property

        Public Function TryGetMetric(name As String, ByRef value As Double) As Boolean
            If String.IsNullOrWhiteSpace(name) Then
                value = 0.0
                Return False
            End If
            Return Metrics.TryGetValue(name, value)
        End Function
    End Class

End Namespace
