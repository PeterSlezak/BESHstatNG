Option Explicit On
Option Strict On

Imports System.Collections.Generic
Imports System.Globalization

Namespace ExcelCommon

    ''' <summary>
    ''' Application-layer result for the shared one-way ANOVA workflow. Besides the primary
    ''' table it can carry Welch output, post-hoc tables, and host-neutral comparison-plot data.
    ''' </summary>
    Public Class OneWayAnovaApplicationResult

        Public Sub New(table As Global.BESHStatNG.ResultTableOutputModel,
                       fStatistic As Double,
                       pValue As Double,
                       groupNames As String(),
                       groupCounts As Integer())
            Me.New(
                New List(Of Global.BESHStatNG.ResultTableOutputModel) From {table},
                fStatistic,
                pValue,
                groupNames,
                groupCounts,
                Nothing,
                Nothing,
                Nothing,
                Nothing,
                Nothing)
        End Sub

        Public Sub New(tables As List(Of Global.BESHStatNG.ResultTableOutputModel),
                       fStatistic As Double,
                       pValue As Double,
                       groupNames As String(),
                       groupCounts As Integer(),
                       welchResult As Global.BESHStatNG.TestResult,
                       fisherLsdPlotData As Global.BESHStatNG.parametric.MultipleComparisonPlotData,
                       bonferroniPlotData As Global.BESHStatNG.parametric.MultipleComparisonPlotData,
                       tukeyKramerPlotData As Global.BESHStatNG.parametric.MultipleComparisonPlotData,
                       gamesHowellPlotData As Global.BESHStatNG.parametric.MultipleComparisonPlotData)
            If tables Is Nothing OrElse tables.Count = 0 Then
                Throw New ArgumentException("At least one one-way ANOVA result table is required.", NameOf(tables))
            End If

            Me.Tables = tables
            Me.FStatistic = fStatistic
            Me.PValue = pValue
            Me.GroupNames = If(groupNames, Array.Empty(Of String)())
            Me.GroupCounts = If(groupCounts, Array.Empty(Of Integer)())
            Me.WelchResult = welchResult
            Me.FisherLsdPlotData = fisherLsdPlotData
            Me.BonferroniPlotData = bonferroniPlotData
            Me.TukeyKramerPlotData = tukeyKramerPlotData
            Me.GamesHowellPlotData = gamesHowellPlotData
        End Sub

        Public ReadOnly Property Tables As List(Of Global.BESHStatNG.ResultTableOutputModel)

        Public ReadOnly Property Table As Global.BESHStatNG.ResultTableOutputModel
            Get
                Return Tables(0)
            End Get
        End Property

        Public ReadOnly Property FStatistic As Double
        Public ReadOnly Property PValue As Double
        Public ReadOnly Property GroupNames As String()
        Public ReadOnly Property GroupCounts As Integer()
        Public ReadOnly Property WelchResult As Global.BESHStatNG.TestResult
        Public ReadOnly Property FisherLsdPlotData As Global.BESHStatNG.parametric.MultipleComparisonPlotData
        Public ReadOnly Property BonferroniPlotData As Global.BESHStatNG.parametric.MultipleComparisonPlotData
        Public ReadOnly Property TukeyKramerPlotData As Global.BESHStatNG.parametric.MultipleComparisonPlotData
        Public ReadOnly Property GamesHowellPlotData As Global.BESHStatNG.parametric.MultipleComparisonPlotData

        Public ReadOnly Property HasWelch As Boolean
            Get
                Return WelchResult IsNot Nothing
            End Get
        End Property

        Public ReadOnly Property WelchFStatistic As Double
            Get
                Return If(WelchResult Is Nothing, Double.NaN, WelchResult.TestStatistics1)
            End Get
        End Property

        Public ReadOnly Property WelchPValue As Double
            Get
                Return If(WelchResult Is Nothing, Double.NaN, WelchResult.Pvalue)
            End Get
        End Property

        Public ReadOnly Property WelchDenominatorDf As Double
            Get
                Return If(WelchResult Is Nothing, Double.NaN, WelchResult.DF1)
            End Get
        End Property
    End Class

    ''' <summary>
    ''' Shared one-way ANOVA application service. It owns the complete host-neutral ANOVA
    ''' orchestration (classical ANOVA, optional Welch test, and optional post-hoc procedures).
    ''' Windows Excel-DNA and Office.js/WASM hosts therefore do not need to recreate the Core model.
    ''' </summary>
    Public NotInheritable Class OneWayAnovaApplicationService

        Private Sub New()
        End Sub

        Public Shared Function Run(input As SpreadsheetRangeData) As OneWayAnovaApplicationResult
            Return Run(GroupedNumericRangeParser.Parse(input), Nothing)
        End Function

        Public Shared Function Run(input As SpreadsheetRangeData,
                                   options As OneWayAnovaAnalysisOptions) As OneWayAnovaApplicationResult
            Return Run(GroupedNumericRangeParser.Parse(input), options)
        End Function

        Public Shared Function Run(grouped As GroupedNumericData) As OneWayAnovaApplicationResult
            Return Run(grouped, Nothing)
        End Function

        ''' <summary>
        ''' Runs the shared ANOVA workflow from grouped numeric data that has already been
        ''' imported/validated by a host adapter.
        ''' </summary>
        Public Shared Function Run(grouped As GroupedNumericData,
                                   options As OneWayAnovaAnalysisOptions) As OneWayAnovaApplicationResult
            If grouped Is Nothing Then Throw New ArgumentNullException(NameOf(grouped))

            Dim safeOptions As OneWayAnovaAnalysisOptions = NormalizeOptions(options)
            Dim model As New Global.BESHStatNG.parametric.OneWayANOVA(grouped.Groups, grouped.GroupNames)
            Dim raw As Object(,) = model.compute()

            Dim welch As Global.BESHStatNG.TestResult = Nothing
            If safeOptions.IncludeWelch Then
                welch = model.WelshANOVA()
            End If

            If safeOptions.IncludeFisherLsd Then model.FisherLSD(False, safeOptions.Alpha)
            If safeOptions.IncludeBonferroni Then model.FisherLSD(True, safeOptions.Alpha)
            If safeOptions.IncludeTukeyKramer Then model.TukeyKramer(safeOptions.Alpha)
            If safeOptions.IncludeGamesHowell Then model.GamesHowell(safeOptions.Alpha)

            Dim resultTables As List(Of Global.BESHStatNG.ResultTable) = model.wrapResults()
            If resultTables Is Nothing OrElse resultTables.Count = 0 Then
                Throw New InvalidOperationException("One-way ANOVA did not produce a ResultTable.")
            End If

            Dim outputTables As New List(Of Global.BESHStatNG.ResultTableOutputModel)(resultTables.Count)
            For Each table As Global.BESHStatNG.ResultTable In resultTables
                outputTables.Add(table.ToOutputModel())
            Next

            Dim fStatistic As Double = Convert.ToDouble(raw(0, 3), CultureInfo.InvariantCulture)
            Dim pValue As Double = Convert.ToDouble(raw(0, 4), CultureInfo.InvariantCulture)

            Return New OneWayAnovaApplicationResult(
                outputTables,
                fStatistic,
                pValue,
                DirectCast(grouped.GroupNames.Clone(), String()),
                grouped.GroupCounts,
                welch,
                model.FisherLSDPlotData,
                model.BonferroniPlotData,
                model.TukeyKramerPlotData,
                model.GamesHowellPlotData)
        End Function

        Private Shared Function NormalizeOptions(options As OneWayAnovaAnalysisOptions) As OneWayAnovaAnalysisOptions
            Dim source As OneWayAnovaAnalysisOptions = If(options, New OneWayAnovaAnalysisOptions())
            Dim safeAlpha As Double = source.Alpha

            If Double.IsNaN(safeAlpha) OrElse Double.IsInfinity(safeAlpha) OrElse
               safeAlpha <= 0.0 OrElse safeAlpha >= 1.0 Then
                safeAlpha = 0.05
            End If

            Return New OneWayAnovaAnalysisOptions With {
                .IncludeWelch = source.IncludeWelch,
                .IncludeFisherLsd = source.IncludeFisherLsd,
                .IncludeBonferroni = source.IncludeBonferroni,
                .IncludeTukeyKramer = source.IncludeTukeyKramer,
                .IncludeGamesHowell = source.IncludeGamesHowell,
                .Alpha = safeAlpha
            }
        End Function
    End Class

End Namespace
