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
    End Enum

    ''' <summary>
    ''' Base class for strongly typed analysis options. It keeps the generic request
    ''' extensible without relying on late binding or host-specific dictionaries.
    ''' </summary>
    Public MustInherit Class SpreadsheetAnalysisOptions
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
    End Class

    ''' <summary>
    ''' Generic analysis result. Result tables and scalar metrics are directly consumable by
    ''' any host; <see cref="Detail"/> carries an optional strongly typed analysis-specific
    ''' result for consumers that also need artifacts such as plot payloads.
    ''' </summary>
    Public Class SpreadsheetAnalysisResult

        Public Sub New(analysisKind As SpreadsheetAnalysisKind,
                       tables As List(Of Global.BESHStatNG.ResultTableOutputModel),
                       metrics As Dictionary(Of String, Double),
                       detail As Object)
            If tables Is Nothing Then Throw New ArgumentNullException(NameOf(tables))
            If metrics Is Nothing Then Throw New ArgumentNullException(NameOf(metrics))

            Me.AnalysisKind = analysisKind
            Me.Tables = tables
            Me.Metrics = metrics
            Me.Detail = detail
        End Sub

        Public ReadOnly Property AnalysisKind As SpreadsheetAnalysisKind
        Public ReadOnly Property Tables As List(Of Global.BESHStatNG.ResultTableOutputModel)
        Public ReadOnly Property Metrics As Dictionary(Of String, Double)
        Public ReadOnly Property Detail As Object

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
