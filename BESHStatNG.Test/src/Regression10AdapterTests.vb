Option Explicit On
Option Strict On
Imports System
Imports System.Collections.Generic
Imports System.Globalization
Imports System.Linq
Imports BESHStatNG
Imports BESHStatNG.ExcelCommon
Imports BESHStatNG.regression
Imports Microsoft.VisualStudio.TestTools.UnitTesting

<TestClass>
Public Class Regression10AdapterTests
    Private NotInheritable Class MemoryWriter
        Inherits ResultTableWriterBase
        Public ReadOnly Blocks As New List(Of ResultTableOutputBlock)
        Protected Overrides Sub WriteOutputBlock(block As ResultTableOutputBlock)
            Blocks.Add(block)
        End Sub
    End Class

    <DataTestMethod>
    <DataRow("LM-ordinary", "")><DataRow("LM-alternate", "")><DataRow("Gaussian-ordinary", "")><DataRow("Gaussian-alternate", "")><DataRow("Binomial-ordinary", "")><DataRow("Binomial-alternate", "")>
    <DataRow("LM-ordinary", "sk-SK")><DataRow("LM-alternate", "sk-SK")><DataRow("Gaussian-ordinary", "sk-SK")><DataRow("Gaussian-alternate", "sk-SK")><DataRow("Binomial-ordinary", "sk-SK")><DataRow("Binomial-alternate", "sk-SK")>
    Public Sub ShippingAdapterAndTableWriterConsumeFamilyProjection(name As String, culture As String)
        Dim saved = CultureInfo.CurrentCulture
        Try
            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo(culture)
            Dim f = Regression10Fixture.Fit(name), linear = name.StartsWith("LM"), alternate = name.EndsWith("alternate")
            Dim req = Regression10PolicyTests.Request(f, linear, name.StartsWith("Binomial"), alternate, Not (linear AndAlso alternate), vectors:=Not linear OrElse alternate)
            Dim raw As New glmData With {.RowIds = f.Ids, .bOffset = Not linear, .bWeights = Not linear OrElse alternate,
                .OffsetData = f.Offset, .WeightData = f.Weights, .OffsetVarName = "offset", .WeightVarName = "weight"}
            Dim request = WindowsRegressionRequestAdapter.Capture(req.Kind, "y", raw, f.Data, f.Names, req.Primary.Effects, req.Options)
            Dim intent = LinearBinomialRegressionPolicy.Project(request, If(linear AndAlso alternate, TermSumOfSquaresType.TypeI, TermSumOfSquaresType.TypeIII))
            Assert.IsTrue(intent.Validation.IsValid, String.Join("; ", intent.Validation.Diagnostics.Select(Function(d) d.Code & ": " & d.Message)))
            Dim report = WindowsRegressionRequestAdapter.Report(intent, f.Tables, Enumerable.Range(0, f.Fitted.GetLength(0)).Select(Function(i) f.Fitted(i, 0)).ToArray(), f.Residual, f.Converged, If(linear, 0.05, 0.1))
            CollectionAssert.AreEqual(f.Ids, report.Rows.RowIds)
            Dim oldWriter As New MemoryWriter, projectedWriter As New MemoryWriter
            Dim originalTables As New ProcessListofResultTables(f.Tables), projectedTables As New ProcessListofResultTables(report.LegacyTables())
            originalTables.writeToSheet(oldWriter, True)
            projectedTables.writeToSheet(projectedWriter, True)
            SameBlocks(oldWriter.Blocks, projectedWriter.Blocks)
            oldWriter = New MemoryWriter : projectedWriter = New MemoryWriter
            oldWriter.write(f.Residual) : projectedWriter.write(report.Rows.Residuals)
            SameBlocks(oldWriter.Blocks, projectedWriter.Blocks)
            oldWriter = New MemoryWriter : projectedWriter = New MemoryWriter
            oldWriter.write(Enumerable.Range(0, f.Fitted.GetLength(0)).Select(Function(i) f.Fitted(i, 0)).ToArray(), True)
            projectedWriter.write(report.Rows.Fitted)
            SameBlocks(oldWriter.Blocks, projectedWriter.Blocks)
        Finally
            CultureInfo.CurrentCulture = saved
        End Try
    End Sub

    Private Shared Sub SameBlocks(a As List(Of ResultTableOutputBlock), b As List(Of ResultTableOutputBlock))
        Assert.AreEqual(a.Count, b.Count)
        For i = 0 To a.Count - 1
            Assert.AreEqual(a(i).StartRow, b(i).StartRow) : Assert.AreEqual(a(i).StartColumn, b(i).StartColumn)
            Dim x = a(i).Model, y = b(i).Model
            Assert.AreEqual(x.HeaderTopRows, y.HeaderTopRows) : Assert.AreEqual(x.HeaderLeftColumns, y.HeaderLeftColumns)
            Assert.AreEqual(x.TitleRows, y.TitleRows) : Assert.AreEqual(x.FooterRows, y.FooterRows)
            Assert.AreEqual(x.IsResultTable, y.IsResultTable)
            CollectionAssert.AreEqual(x.PvalueColumns, y.PvalueColumns)
            CollectionAssert.AreEqual(x.PvalueCells, y.PvalueCells)
            CollectionAssert.AreEqual(x.Values.Cast(Of Object)().ToArray(), y.Values.Cast(Of Object)().ToArray())
        Next
    End Sub

    <TestMethod>
    Public Sub PostFitClassificationUsesCurrentOptionsAndUnchangedFacade()
        Dim f = Regression10Fixture.Fit("Binomial-ordinary"), request = Regression10PolicyTests.Request(f, False, True, False)
        Dim y = Enumerable.Range(0, f.Data.GetLength(0)).Select(Function(i) f.Data(i, 0)).ToArray()
        Dim p = Enumerable.Range(0, f.Data.GetLength(0)).Select(Function(i) f.Fitted(i, 0)).ToArray()
        Dim current = request.Options : current.Classification = True : current.Threshold = 0.7
        current.Calibration = True : current.CalibrationBins = 5 : current.ThresholdTable = True : current.Brier = True
        Dim classification = BinaryClassificationReporting.ComposeGlmReport(y, p, f.Weights, current, WorksheetFunctions.ComputeWeightedEventRate(y, f.Weights))
        Dim plan As New LinearBinomialClassificationPlan(request, current, classification)
        current.Threshold = 0.1 : current.Calibration = False
        Assert.AreEqual(0.7, plan.Options.Threshold) : Assert.IsFalse(request.Options.Classification)
        CollectionAssert.AreEqual(f.Ids, plan.RowIds)
        CollectionAssert.AreEqual({"ROC", "Calibration"}, plan.ChartHooks.ToArray())
        Dim a As New MemoryWriter, b As New MemoryWriter
        Dim originalTables As New ProcessListofResultTables(classification.LegacyTables()), projectedTables As New ProcessListofResultTables(plan.LegacyTables())
        originalTables.writeToSheet(a, True)
        projectedTables.writeToSheet(b, True)
        SameBlocks(a.Blocks, b.Blocks)
    End Sub

    <TestMethod>
    Public Sub StrictDiagnosticsStayObservationalInShippingAdapter()
        Dim f = Regression09Fixture.Load("glm_gaussian_full.csv"), raw As New glmData With {.RowIds = f.Ids, .bWeights = True, .WeightData = f.Weights, .WeightVarName = "weight"}
        raw.WeightData(0) = -1
        Dim request = WindowsRegressionRequestAdapter.Capture(RegressionAnalysisKind.LinearModel, "y", raw, f.Data, f.Names,
            RegressionEffectModel.FromProjection(f.Names.Skip(1), New Dictionary(Of String, TermSpec)), New RegressionOptions)
        Dim intent = LinearBinomialRegressionPolicy.Project(request)
        Assert.IsFalse(intent.Validation.IsValid)
        Assert.IsTrue(intent.Validation.Diagnostics.Any(Function(d) d.FieldPath = "weights"))
    End Sub
End Class
