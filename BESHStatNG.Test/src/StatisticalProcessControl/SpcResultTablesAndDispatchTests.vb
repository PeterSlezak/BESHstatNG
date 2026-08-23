Option Explicit On
Option Strict On
Option Infer On

Imports System
Imports System.Collections.Generic
Imports System.Linq
Imports Microsoft.VisualStudio.TestTools.UnitTesting
Imports BESHStatNG
Imports BESHStatNG.StatisticalProcessControl

Friend NotInheritable Class SyntheticRunCalculator
    Implements ISpcChartCalculator

    Private ReadOnly _returnNothing As Boolean

    Public Sub New(Optional returnNothing As Boolean = False)
        _returnNothing = returnNothing
    End Sub

    Public Function CanCalculate(chartType As SpcChartType) As Boolean _
        Implements ISpcChartCalculator.CanCalculate
        Return chartType = SpcChartType.RunChart
    End Function

    Public Function Calculate(request As SpcFitRequest,
                              cancellationRequested As Func(Of Boolean)) As SpcCalculationResult _
        Implements ISpcChartCalculator.Calculate
        If _returnNothing Then Return Nothing
        If cancellationRequested() Then Throw New OperationCanceledException()
        Dim measurements As Double(,) = request.Data.Measurements
        Dim points(measurements.GetLength(0) - 1) As SpcPointResult
        For i As Integer = 0 To points.Length - 1
            points(i) = New SpcPointResult(
                i, measurements(i, 0), 0.0, -3.0, 3.0,
                standardError:=1.0,
                standardizedValue:=measurements(i, 0))
        Next
        Return New SpcCalculationResult({
            New SpcPanelResult(SpcPanelType.Run, "Synthetic run", points)
        }, {"Synthetic calculator warning"})
    End Function
End Class

<TestClass>
Public Class SpcResultTablesAndDispatchTests

    Private Shared Function AuditedResult() As SpcFitResult
        Dim values As Double() = IndividualsData()
        Dim options As SpcAnalysisOptions = OptionsWithRules(SpcRulePreset.Nelson)
        options.Stages = BaselineMonitoringStages(values.Length, 20)
        options.Exclusions = {
            New SpcExclusionDefinition(4, SpcExclusionScope.EstimationAndRules,
                                       "Known setup event")
        }
        Return SpcEngine.Fit(New SpcFitRequest(
            SpcChartType.IndividualsMovingRange,
            SpcInputData.FromIndividualSequence(
                values, Labels(values.Length), Sequence(values.Length),
                sourceRowIndices:=Enumerable.Range(101, values.Length).ToArray(),
                valueName:="Measurement"),
            options,
            specificationLimits:=New SpcSpecificationLimits(9.0, 10.0, 11.5),
            requestLabel:="SPC audit test",
            chartTitle:="I-MR unit test",
            valueAxisTitle:="Value"))
    End Function

    <TestMethod>
    <TestCategory("SPC-Engine")>
    Public Sub ExplicitCalculatorRegistry_DispatchesAndAppliesRuleLayer()
        Dim engine As New SpcEngine({CType(New SyntheticRunCalculator(), ISpcChartCalculator)})
        Dim options As SpcAnalysisOptions = OptionsWithRules(SpcRulePreset.RuleOneOnly)
        Dim result As SpcFitResult = engine.FitWithRegisteredCalculators(
            New SpcFitRequest(
                SpcChartType.RunChart,
                SpcInputData.FromIndividualSequence({0.0, 4.0, 0.0}),
                options))
        Assert.AreEqual(SpcChartType.RunChart, result.ChartType)
        Assert.AreEqual(SpcChartFamily.Run, result.ChartFamily)
        Assert.AreEqual(1, result.PanelCount)
        Assert.AreEqual(1, result.SignalCount)
        Assert.AreEqual(1, result.Signals(0).TerminalPointIndex)
        Assert.IsTrue(result.Warnings.Contains("Synthetic calculator warning"))
    End Sub

    <TestMethod>
    <TestCategory("SPC-Engine")>
    Public Sub CalculatorRegistry_RejectsMissingDuplicateAndNullResults()
        Dim request As New SpcFitRequest(
            SpcChartType.RunChart,
            SpcInputData.FromIndividualSequence({0.0, 1.0}),
            NoRulesOptions())

        Dim empty As New SpcEngine(Array.Empty(Of ISpcChartCalculator)())
        Assert.ThrowsException(Of NotSupportedException)(
            Sub() empty.FitWithRegisteredCalculators(request))

        Dim duplicate As New SpcEngine({
            CType(New SyntheticRunCalculator(), ISpcChartCalculator),
            CType(New SyntheticRunCalculator(), ISpcChartCalculator)
        })
        Assert.ThrowsException(Of InvalidOperationException)(
            Sub() duplicate.FitWithRegisteredCalculators(request))

        Dim nullResult As New SpcEngine({
            CType(New SyntheticRunCalculator(returnNothing:=True), ISpcChartCalculator)
        })
        Assert.ThrowsException(Of InvalidOperationException)(
            Sub() nullResult.FitWithRegisteredCalculators(request))

        AssertThrowsAssignable(Of ArgumentException)(
            Sub()
                Dim unused = New SpcEngine({CType(Nothing, ISpcChartCalculator)})
            End Sub)
    End Sub

    <TestMethod>
    <TestCategory("SPC-Models")>
    Public Sub CalculationAndFitResults_DefensivelyCopyPanelsWarningsAndFlattenedCollections()
        Dim points As SpcPointResult() = {
            New SpcPointResult(0, 0.0, 0.0, -3.0, 3.0,
                               standardError:=1.0, standardizedValue:=0.0)
        }
        Dim panel As New SpcPanelResult(SpcPanelType.Run, "Run", points)
        Dim panels As SpcPanelResult() = {panel}
        Dim warnings As String() = {" warning "}
        Dim calculation As New SpcCalculationResult(panels, warnings)
        panels(0) = Nothing
        warnings(0) = "changed"
        Assert.IsNotNull(calculation.Panels(0))
        Assert.AreEqual("warning", calculation.Warnings(0))

        Dim request As New SpcFitRequest(
            SpcChartType.RunChart,
            SpcInputData.FromIndividualSequence({0.0}),
            NoRulesOptions())
        Dim result As New SpcFitResult(request, calculation.Panels, calculation.Warnings)
        Dim returned As SpcPanelResult() = result.Panels
        returned(0) = Nothing
        Assert.IsNotNull(result.Panels(0))
        Assert.AreEqual(1, result.ChartPointCount)
        Assert.AreEqual(1, result.PanelPointCount)
        Assert.AreEqual(0, result.SignalCount)
    End Sub

    <TestMethod>
    <TestCategory("SPC-Output")>
    Public Sub GroupedResultTableBuilders_ReturnCompleteSummaryChartSignalAndAuditSets()
        Dim result As SpcFitResult = AuditedResult()
        Dim summary As List(Of ResultTable) = SpcResultTables.BuildSummaryTables(result)
        Dim chartCombined As List(Of ResultTable) =
            SpcResultTables.BuildChartDataTables(result, separatePanels:=False)
        Dim chartSeparate As List(Of ResultTable) =
            SpcResultTables.BuildChartDataTables(result, separatePanels:=True)
        Dim signals As List(Of ResultTable) = SpcResultTables.BuildSignalTables(result)
        Dim audit As List(Of ResultTable) = SpcResultTables.BuildAuditTables(result)

        Assert.AreEqual(4, summary.Count)
        Assert.AreEqual(1, chartCombined.Count)
        Assert.AreEqual(result.PanelCount, chartSeparate.Count)
        Assert.AreEqual(2, signals.Count)
        Assert.AreEqual(6, audit.Count)

        Dim allTables As IEnumerable(Of ResultTable) =
            summary.Concat(chartCombined).Concat(chartSeparate).Concat(signals).Concat(audit)
        For Each table As ResultTable In allTables
            Assert.IsNotNull(table)
            Assert.IsTrue(table.TotalRows > 0)
            Assert.IsTrue(table.TotalCols > 0)
            Assert.AreEqual(table.TotalRows, table.returnSelf().GetLength(0))
            Assert.AreEqual(table.TotalCols, table.returnSelf().GetLength(1))
        Next
    End Sub

    <TestMethod>
    <TestCategory("SPC-Output")>
    Public Sub PointDataTable_UsesOneBasedDisplayPointAndPreservesZeroBasedIndexAndSourceRows()
        Dim table As ResultTable = SpcResultTables.BuildPointDataTable(AuditedResult())
        Dim output As Object(,) = table.returnSelf()
        Assert.AreEqual("SPC chart data", CStr(output(0, 0)))
        Assert.AreEqual("Point", CStr(output(1, 2)))
        Assert.AreEqual("Point Index (0-based)", CStr(output(1, 3)))
        Assert.AreEqual(1, CInt(output(2, 2)))
        Assert.AreEqual(0, CInt(output(2, 3)))
        Assert.AreEqual("101", CStr(output(2, 4)))
    End Sub

    <TestMethod>
    <TestCategory("SPC-Output")>
    Public Sub IndividualResultTables_ExposeStagesExclusionsSpecificationsAndExecutionAudit()
        Dim result As SpcFitResult = AuditedResult()
        Dim stages As Object(,) = SpcResultTables.BuildStagesTable(result).returnSelf()
        Dim exclusions As Object(,) = SpcResultTables.BuildExclusionsTable(result).returnSelf()
        Dim specs As Object(,) = SpcResultTables.BuildSpecificationLimitsTable(result).returnSelf()
        Dim settings As Object(,) = SpcResultTables.BuildSettingsTable(result).returnSelf()
        Dim execution As Object(,) = SpcResultTables.BuildExecutionAuditTable(result).returnSelf()

        Assert.IsTrue(MatrixContains(stages, "Baseline"))
        Assert.IsTrue(MatrixContains(stages, "Monitoring"))
        Assert.IsTrue(MatrixContains(exclusions, "Known setup event"))
        Assert.IsTrue(MatrixContains(specs, "Lower specification limit"))
        Assert.IsTrue(MatrixContains(settings, "Nelson"))
        Assert.IsTrue(MatrixContains(execution, "Execution time (ms)"))
        Assert.IsTrue(result.ExecutionStartedUtc.HasValue)
        Assert.IsTrue(result.ExecutionCompletedUtc.HasValue)
        Assert.IsTrue(result.ExecutionTimeMilliseconds >= 0.0)
    End Sub

    <TestMethod>
    <TestCategory("SPC-Output")>
    Public Sub EmptySignalAndWarningTables_RemainValidAndExplainEmptyState()
        Dim result As SpcFitResult = SpcEngine.Fit(New SpcFitRequest(
            SpcChartType.Individuals,
            SpcInputData.FromIndividualSequence({10.0, 10.1, 9.9, 10.0}),
            NoRulesOptions()))
        Dim signalTable As Object(,) =
            SpcResultTables.BuildSignalsTable(result).returnSelf()
        Dim warningTable As Object(,) =
            SpcResultTables.BuildWarningsTable(result).returnSelf()
        Assert.IsTrue(MatrixContains(signalTable, "No special-cause signals"))
        Assert.IsTrue(MatrixContains(warningTable, "No warnings were generated"))
    End Sub

    Private Shared Function MatrixContains(values As Object(,), sought As String) As Boolean
        For i As Integer = 0 To values.GetLength(0) - 1
            For j As Integer = 0 To values.GetLength(1) - 1
                If values(i, j) IsNot Nothing AndAlso
                   CStr(values(i, j)).IndexOf(sought, StringComparison.OrdinalIgnoreCase) >= 0 Then
                    Return True
                End If
            Next
        Next
        Return False
    End Function

End Class
