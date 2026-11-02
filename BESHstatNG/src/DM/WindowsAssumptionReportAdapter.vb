Option Explicit On
Option Strict On

Imports System
Imports System.Collections.Generic
Imports System.Linq
Imports System.Threading
Imports BESHStatNG.ExcelCommon

''' <summary>Detached computation and legacy conversion only. Never acquires Excel, creates
''' controls, selects an output destination or writes. Box calculation stays in Windows
''' graphics; the caller alone may attach a worksheet and render the chart.</summary>
Public NotInheritable Class WindowsAssumptionReportAdapter
    Private Sub New()
    End Sub

    Public Shared Function Prepare(data As MultiGroupsUnpairedData, options As AssumptionReportOptions,
                                   Optional cancellation As CancellationToken = Nothing) As WindowsPreparedAssumptionReport
        Dim input = If(data Is Nothing, Nothing, New AssumptionReportInput(data.X, data.varNames))
        Dim result = AssumptionReportPolicy.Build(input, options, cancellation)
        For Each issue In result.Validation.Issues
            Select Case issue.Code
                Case "NORMALITY_SW_NOT_APPLICABLE" : AppInfrastructure.CoreServices.Log("N not in range between 3 and 5000")
                Case "NORMALITY_DA_NOT_APPLICABLE" : AppInfrastructure.CoreServices.Log("N not >= 9")
                Case "NORMALITY_AD_NOT_APPLICABLE" : AppInfrastructure.CoreServices.Log("N not > 1")
            End Select
        Next
        'Existing Core calls sort the actual vectors. Replay only that computational effect
        'for legacy plotting/supplementary callers, preserving references and NO row-map claim.
        Dim working = result.LegacyWorkingGroups
        If data IsNot Nothing AndAlso data.X IsNot Nothing AndAlso working IsNot Nothing AndAlso data.X.Length = working.Length Then
            For g = 0 To working.Length - 1
                If data.X(g) IsNot Nothing AndAlso working(g) IsNot Nothing AndAlso data.X(g).Length = working(g).Length Then
                    Array.Copy(working(g), data.X(g), working(g).Length)
                End If
            Next
        End If
        result.ThrowIfFailed() 'existing form outer catch; no output is acquired before success
        Dim tables = result.Tables.Select(Function(t) t.ToLegacyTable()).ToList()
        Dim box As graphics.BoxPlot = Nothing
        If result.Stages.Contains(AssumptionOutputStage.WindowsBoxSummaryAndChart) Then
            cancellation.ThrowIfCancellationRequested()
            box = New graphics.BoxPlot(data.X, data.varNames)
            box.Calculate()
            box.CalcForPlotting()
            tables.Add(AssumptionReportPolicy.BoxSummary(data.varNames, box.Q1, box.Medians, box.Q3, box.SmallOutliers, box.BigOutliers).ToLegacyTable())
        End If
        cancellation.ThrowIfCancellationRequested()
        Return New WindowsPreparedAssumptionReport(tables, box, result)
    End Function

    ''' <summary>Compatibility for ANOVA/KW and other callers. No primary analysis migrates here.</summary>
    Public Shared Function Descriptive(data As MultiGroupsUnpairedData, Optional statistics As IEnumerable(Of String) = Nothing) As ResultTable
        Return Prepare(data, New AssumptionReportOptions(AssumptionReportKind.Descriptive, statistics:=statistics)).Tables(0)
    End Function

    Public Shared Function Homogeneity(data As MultiGroupsUnpairedData, selected As HomogeneityMethods) As ResultTable
        If selected = HomogeneityMethods.None Then Return New ResultTable()
        Return Prepare(data, New AssumptionReportOptions(AssumptionReportKind.Homogeneity, homogeneity:=selected)).Tables(0)
    End Function
End Class

Public NotInheritable Class WindowsPreparedAssumptionReport
    Friend Sub New(tables As List(Of ResultTable), box As graphics.BoxPlot, computation As AssumptionReportResult)
        Me.Tables = tables
        Me.Box = box
        Me.Computation = computation
    End Sub
    Public ReadOnly Property Tables As List(Of ResultTable)
    Public ReadOnly Property Box As graphics.BoxPlot
    Public ReadOnly Property Computation As AssumptionReportResult
End Class
