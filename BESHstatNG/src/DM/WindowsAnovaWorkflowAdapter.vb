Option Explicit On
Option Strict On
Imports System
Imports System.Collections.Generic
Imports System.Linq
Imports System.Threading
Imports BESHStatNG.ExcelCommon

''' <summary>Host-free data/result adaptation. Native acquisition, controls, charts and writes
''' are outside this bridge. Existing Core sorting effects are replayed only for legacy callers.</summary>
Public NotInheritable Class WindowsAnovaWorkflowAdapter
    Private Sub New()
    End Sub
    Public Shared Function AnalyzeGrouped(data As MultiGroupsUnpairedData, options As AnovaGroupedWorkflowOptions,
                                          Optional cancellation As CancellationToken = Nothing) As AnovaGroupedWorkflowResult
        Dim captured = ExcelDnaSharedApplicationAdapter.ToGroupedNumericData(data)
        Dim original = WindowsUnivariateInputAdapter.GetOriginalSelection(data)
        Dim sources As IEnumerable(Of UnivariateObservationSource) = Nothing
        If original IsNot Nothing AndAlso original.Names.SequenceEqual(captured.GroupNames) Then
            Dim values = original.Groups
            If values.Length = captured.Groups.Length AndAlso Enumerable.Range(0, values.Length).All(Function(g) values(g).SequenceEqual(captured.Groups(g))) Then sources = original.Sources
        End If
        Dim result = AnovaWorkflowPolicy.RunGrouped(New AssumptionReportInput(captured.Groups, captured.GroupNames, sources), options, cancellation)
        result.ThrowIfFailed()
        'The existing primary adapter already used cloned groups: DO NOT replay primary scratch into data.
        Return result
    End Function

    Public Shared Function PrepareSupplements(data As MultiGroupsUnpairedData, options As AnovaGroupedWorkflowOptions,
                                             Optional cancellation As CancellationToken = Nothing) As WindowsPreparedAssumptionReport
        Dim report = AnovaWorkflowPolicy.BuildSupplements(New AssumptionReportInput(data.X, data.varNames), options, cancellation)
        Dim working = report.LegacyWorkingGroups
        If working IsNot Nothing Then
            For g = 0 To working.Length - 1
                Array.Copy(working(g), data.X(g), working(g).Length)
            Next
        End If
        report.ThrowIfFailed()
        Dim tables = report.Tables.Select(Function(t) t.ToLegacyTable()).ToList()
        Dim box As graphics.BoxPlot = Nothing
        If options.BoxSummary Then
            cancellation.ThrowIfCancellationRequested()
            box = New graphics.BoxPlot(data.X, data.varNames)
            box.Calculate() : box.CalcForPlotting()
            tables.Add(AssumptionReportPolicy.BoxSummary(data.varNames, box.Q1, box.Medians, box.Q3, box.SmallOutliers, box.BigOutliers).ToLegacyTable())
        End If
        cancellation.ThrowIfCancellationRequested()
        Return New WindowsPreparedAssumptionReport(tables, box, report)
    End Function

    Public Shared Function Repeated(data As MultiGroupsPairedData, options As RepeatedAnovaWorkflowOptions,
                                    Optional cancellation As CancellationToken = Nothing) As WindowsPreparedAnovaReport
        Dim result = AnovaWorkflowPolicy.RunRepeated(If(data Is Nothing, Nothing, data.X), If(data Is Nothing, Nothing, data.varNames),
            options, WindowsPairedInputAdapter.GetBlockEvidence(data), cancellation)
        If data IsNot Nothing AndAlso result.LegacyWorkingMatrix IsNot Nothing Then Array.Copy(result.LegacyWorkingMatrix, data.X, data.X.Length)
        result.ThrowIfFailed()
        Dim tables = result.ToLegacyTables(), box As graphics.BoxPlot = Nothing
        If result.BoxSummary Then
            cancellation.ThrowIfCancellationRequested()
            box = New graphics.BoxPlot(data.X, data.varNames)
            box.Calculate() : box.CalcForPlotting()
            tables.Add(AssumptionReportPolicy.BoxSummary(data.varNames, box.Q1, box.Medians, box.Q3, box.SmallOutliers, box.BigOutliers).ToLegacyTable())
        End If
        cancellation.ThrowIfCancellationRequested()
        Return New WindowsPreparedAnovaReport(tables, box, result)
    End Function

    Public Shared Function Nested(data As MultiGroupsPairedDataObj, Optional cancellation As CancellationToken = Nothing) As AnovaModelReportResult
        Dim result = AnovaWorkflowPolicy.RunNested(If(data Is Nothing, Nothing, data.X), If(data Is Nothing, Nothing, data.varNames),
            WindowsPairedInputAdapter.GetNestedCoreInput(data), cancellation, WindowsPairedInputAdapter.GetNestedEvidence(data))
        If data IsNot Nothing AndAlso result.LegacyWorkingObjects IsNot Nothing Then Array.Copy(result.LegacyWorkingObjects, data.X, data.X.Length)
        result.ThrowIfFailed()
        Return result
    End Function
End Class

Public NotInheritable Class WindowsPreparedAnovaReport
    Friend Sub New(tables As List(Of ResultTable), box As graphics.BoxPlot, report As AnovaModelReportResult)
        Me.Tables = tables : Me.Box = box : Me.Report = report
    End Sub
    Public ReadOnly Property Tables As List(Of ResultTable)
    Public ReadOnly Property Box As graphics.BoxPlot
    Public ReadOnly Property Report As AnovaModelReportResult
End Class
