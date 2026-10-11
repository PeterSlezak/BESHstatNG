Option Explicit On
Option Strict On

Imports System.Collections.Generic

Namespace ExcelCommon

    ''' <summary>
    ''' Host-neutral application result for the Kruskal-Wallis workflow.
    ''' </summary>
    Public Class KruskalWallisApplicationResult

        Public Sub New(tables As List(Of Global.BESHStatNG.ResultTableOutputModel),
                       hStatistic As Double,
                       tieCorrectedHStatistic As Double,
                       pValue As Double,
                       tieCorrectedPValue As Double,
                       groupNames As String(),
                       groupCounts As Integer())
            If tables Is Nothing OrElse tables.Count = 0 Then
                Throw New ArgumentException("At least one Kruskal-Wallis result table is required.", NameOf(tables))
            End If

            Me.Tables = tables
            Me.HStatistic = hStatistic
            Me.TieCorrectedHStatistic = tieCorrectedHStatistic
            Me.PValue = pValue
            Me.TieCorrectedPValue = tieCorrectedPValue
            Me.GroupNames = If(groupNames, Array.Empty(Of String)())
            Me.GroupCounts = If(groupCounts, Array.Empty(Of Integer)())
        End Sub

        Public ReadOnly Property Tables As List(Of Global.BESHStatNG.ResultTableOutputModel)

        Public ReadOnly Property Table As Global.BESHStatNG.ResultTableOutputModel
            Get
                Return Tables(0)
            End Get
        End Property

        Public ReadOnly Property HStatistic As Double
        Public ReadOnly Property TieCorrectedHStatistic As Double
        Public ReadOnly Property PValue As Double
        Public ReadOnly Property TieCorrectedPValue As Double
        Public ReadOnly Property GroupNames As String()
        Public ReadOnly Property GroupCounts As Integer()
    End Class

    ''' <summary>
    ''' Shared Kruskal-Wallis application service. It owns the host-neutral test and optional
    ''' Dunn/Bonferroni post-hoc orchestration so Excel-DNA and Office.js hosts submit the same
    ''' grouped-data contract and receive the same ResultTable output.
    ''' </summary>
    Public NotInheritable Class KruskalWallisApplicationService

        Private Sub New()
        End Sub

        Public Shared Function Run(input As SpreadsheetRangeData) As KruskalWallisApplicationResult
            Return Run(GroupedNumericRangeParser.Parse(input), Nothing)
        End Function

        Public Shared Function Run(input As SpreadsheetRangeData,
                                   options As KruskalWallisAnalysisOptions) As KruskalWallisApplicationResult
            Return Run(GroupedNumericRangeParser.Parse(input), options)
        End Function

        Public Shared Function Run(grouped As GroupedNumericData) As KruskalWallisApplicationResult
            Return Run(grouped, Nothing)
        End Function

        Public Shared Function Run(grouped As GroupedNumericData,
                                   options As KruskalWallisAnalysisOptions) As KruskalWallisApplicationResult
            If grouped Is Nothing Then Throw New ArgumentNullException(NameOf(grouped))

            Dim safeOptions As KruskalWallisAnalysisOptions = NormalizeOptions(options)
            Dim model As New Global.BESHStatNG.nonparametric.KruskallWalis(grouped.Groups, grouped.GroupNames)
            Dim testResult As Global.BESHStatNG.TestResult = model.compute()

            If safeOptions.IncludeDunnComparisons Then
                model.MCP(safeOptions.Alpha)
            End If

            Dim resultTables As List(Of Global.BESHStatNG.ResultTable) = model.wrapResults()
            If resultTables Is Nothing OrElse resultTables.Count = 0 Then
                Throw New InvalidOperationException("Kruskal-Wallis did not produce a ResultTable.")
            End If

            Dim outputTables As New List(Of Global.BESHStatNG.ResultTableOutputModel)(resultTables.Count)
            For Each table As Global.BESHStatNG.ResultTable In resultTables
                outputTables.Add(table.ToOutputModel())
            Next

            Return New KruskalWallisApplicationResult(
                outputTables,
                testResult.TestStatistics1,
                testResult.TestStatistics2,
                testResult.Pvalue,
                testResult.Pvalue2,
                DirectCast(grouped.GroupNames.Clone(), String()),
                grouped.GroupCounts)
        End Function

        Private Shared Function NormalizeOptions(options As KruskalWallisAnalysisOptions) As KruskalWallisAnalysisOptions
            Dim source As KruskalWallisAnalysisOptions = If(options, New KruskalWallisAnalysisOptions())
            Dim safeAlpha As Double = source.Alpha

            If Double.IsNaN(safeAlpha) OrElse Double.IsInfinity(safeAlpha) OrElse
               safeAlpha <= 0.0 OrElse safeAlpha >= 1.0 Then
                safeAlpha = 0.05
            End If

            Return New KruskalWallisAnalysisOptions With {
                .IncludeDunnComparisons = source.IncludeDunnComparisons,
                .Alpha = safeAlpha
            }
        End Function
    End Class

End Namespace
