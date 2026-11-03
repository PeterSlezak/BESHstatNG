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
Public Class Regression11AdapterTests
    Private NotInheritable Class MemoryWriter
        Inherits ResultTableWriterBase
        Public ReadOnly Blocks As New List(Of ResultTableOutputBlock)
        Protected Overrides Sub WriteOutputBlock(block As ResultTableOutputBlock)
            Blocks.Add(block)
        End Sub
    End Class
    Private Shared Function Raw(component As RegressionComponent, offset As RegressionVector, weights As RegressionVector) As glmData
        Return New glmData With {.RowIds = component.RowIds, .bOffset = offset IsNot Nothing, .bWeights = weights IsNot Nothing,
            .OffsetVarName = If(offset Is Nothing, Nothing, offset.Key), .OffsetData = If(offset Is Nothing, Nothing, offset.Values),
            .WeightVarName = If(weights Is Nothing, Nothing, weights.Key), .WeightData = If(weights Is Nothing, Nothing, weights.Values)}
    End Function

    <DataTestMethod><DataRow("NB2", False)><DataRow("NB2", True)><DataRow("ZIP", False)><DataRow("ZIP", True)><DataRow("Ordinal", False)><DataRow("Ordinal", True)><DataRow("Multinomial", False)><DataRow("Multinomial", True)>
    Public Sub ShippingFourFamilyCaptureFitReportAndWriter(kind As String, alternate As Boolean)
        For Each culture In {"", "sk-SK"}
            Dim saved = CultureInfo.CurrentCulture
            Try
                CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo(culture)
                Dim r = Regression11PolicyTests.Request(kind, alternate), p = r.Primary, z = r.Logistic
                Dim request = WindowsRegressionRequestAdapter.Capture(r.Kind, r.ResponseKey, Raw(p, r.Offset, r.Weights), p.Data, p.Names, p.Effects, r.Options, p.InitialValues,
                    If(z Is Nothing, Nothing, Raw(z, Nothing, Nothing)), If(z Is Nothing, Nothing, z.Data), If(z Is Nothing, Nothing, z.Names), If(z Is Nothing, Nothing, z.Effects), If(z Is Nothing, Nothing, z.InitialValues))
                Dim intent = ExtendedRegressionFamilyPolicy.Project(request), dispersion As Nullable(Of Double) = Nothing
                Assert.IsTrue(intent.Validation.IsValid)
                Dim f = Regression11PolicyTests.Fit(intent, dispersion), report = WindowsRegressionRequestAdapter.Report(intent, f.Tables, f.Fitted, f.Residual, f.Converged, f.Categories, dispersion)
                CollectionAssert.AreEqual(p.RowIds, report.Rows.RowIds)
                Dim original As New MemoryWriter, projected As New MemoryWriter
                Dim before As New ProcessListofResultTables(f.Tables), after As New ProcessListofResultTables(report.LegacyTables())
                before.writeToSheet(original, True) : after.writeToSheet(projected, True)
                SameBlocks(original.Blocks, projected.Blocks)
                If f.Residual IsNot Nothing Then
                    original = New MemoryWriter : projected = New MemoryWriter
                    original.write(f.Residual) : projected.write(report.Rows.Residuals)
                    SameBlocks(original.Blocks, projected.Blocks)
                End If
                If kind = "ZIP" Then
                    Assert.IsNotNull(report.Intent.Logistic)
                    CollectionAssert.AreEqual(z.Names, report.Intent.Logistic.Names)
                End If
            Finally
                CultureInfo.CurrentCulture = saved
            End Try
        Next
    End Sub

    Private Shared Sub SameBlocks(a As List(Of ResultTableOutputBlock), b As List(Of ResultTableOutputBlock))
        Assert.AreEqual(a.Count, b.Count)
        For i = 0 To a.Count - 1
            Assert.AreEqual(a(i).StartRow, b(i).StartRow) : Assert.AreEqual(a(i).StartColumn, b(i).StartColumn)
            Dim x = a(i).Model, y = b(i).Model
            Assert.AreEqual(x.HeaderLeftColumns, y.HeaderLeftColumns) : Assert.AreEqual(x.HeaderTopRows, y.HeaderTopRows)
            Assert.AreEqual(x.TitleRows, y.TitleRows) : Assert.AreEqual(x.FooterRows, y.FooterRows)
            CollectionAssert.AreEqual(x.PvalueColumns, y.PvalueColumns) : CollectionAssert.AreEqual(x.PvalueCells, y.PvalueCells)
            CollectionAssert.AreEqual(x.Values.Cast(Of Object)().ToArray(), y.Values.Cast(Of Object)().ToArray())
        Next
    End Sub

    <DataTestMethod><DataRow(0.0)><DataRow(1.0)>
    Public Sub FailedConstantFitFlowsThroughShippingAdapterWithoutFabricatedStatistics(response As Double)
        Dim model = Regression11DefectTests.ConstantModel(response, False, True, True)
        Dim data(23, 1) As Double
        For i = 0 To 23 : data(i, 0) = response : data(i, 1) = i - 11.5 : Next
        Dim rawData As New glmData With {.RowIds = Enumerable.Range(0, 24).Select(Function(i) 91 - 3 * i).ToArray()}
        Dim request = WindowsRegressionRequestAdapter.Capture(RegressionAnalysisKind.GeneralizedLinearModel, "y", rawData, data, {"y", "x"}, Regression09PolicyTests.Effects(),
            New RegressionOptions With {.Family = RegressionFamily.Binomial, .Link = RegressionLink.Logit, .Residuals = True, .Classification = True, .ThresholdTable = True, .Calibration = True, .Brier = True})
        Dim report = WindowsRegressionRequestAdapter.Report(LinearBinomialRegressionPolicy.Project(request), model.wrapResults(Nothing, Nothing), model.PredictedResponses, model.AllResiduals, model.Converged)
        Assert.AreEqual(RegressionConvergence.NotConverged, report.Convergence)
        Assert.IsNull(report.Rows.Fitted) : Assert.IsNull(report.Rows.Residuals)
        Assert.IsTrue(report.Intent.Request.Options.Classification, "A requested classification section cannot manufacture data for an unavailable fit.")
        Assert.AreEqual(1, report.LegacyTables().Count)
        Assert.IsFalse(report.Document.Tables.SelectMany(Function(t) t.Model.Values.Cast(Of Object)()).Any(Function(v) TypeOf v Is Double))
        CollectionAssert.AreEqual(rawData.RowIds, report.Rows.RowIds)
    End Sub

    <TestMethod>
    Public Sub NativeCovarianceQuarantineNeverMutatesCoreOrRequestNames()
        Dim f = Regression09Fixture.Load("glm_gaussian_full.csv"), r = Regression10PolicyTests.Request(f, False, False, False)
        Dim names = f.Names, actual = DirectCast(names.Clone(), String()), headers = WindowsRegressionRequestAdapter.DeferredCovarianceDataHeaders(r, names)
        Assert.AreEqual("Intercept", headers(0)) : CollectionAssert.AreEqual(actual, names)
        CollectionAssert.AreEqual(actual, r.Primary.Names)
        Dim o = r.Options : o.Covariance = False
        Dim withoutCov As New RegressionRequest(r.Kind, r.ResponseKey, r.Primary, o)
        CollectionAssert.AreEqual(actual, WindowsRegressionRequestAdapter.DeferredCovarianceDataHeaders(withoutCov, names))
    End Sub

    <TestMethod>
    Public Sub PortableRequestedAlphaAndDormantNativeAssignmentUseTheSameFitIntent()
        Dim f = Regression09Fixture.Load("glm_gaussian_full.csv"), r = Regression10PolicyTests.Request(f, True, False, False)
        Dim corrected = Regression11DefectTests.FitLinear(LinearBinomialRegressionPolicy.Project(r))
        Dim retained = Regression11DefectTests.FitLinear(LinearBinomialRegressionPolicy.Project(r, alphaMode:=LinearFitAlphaMode.NativeLegacyDefault))
        Assert.AreEqual(0.1, corrected.Alpha) : Assert.AreEqual(0.05, retained.Alpha)
        CollectionAssert.AreEqual(corrected.results.Coeffs_est, retained.results.Coeffs_est)
    End Sub
End Class
