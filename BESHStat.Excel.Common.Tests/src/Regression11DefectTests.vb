Option Explicit On
Option Strict On
Imports System
Imports System.Collections.Generic
Imports System.Globalization
Imports System.Linq
Imports System.Reflection
Imports BESHStatNG
Imports BESHStatNG.ExcelCommon
Imports BESHStatNG.regression
Imports Microsoft.VisualStudio.TestTools.UnitTesting

<TestClass>
Public Class Regression11DefectTests
    <TestMethod>
    Public Sub AcceptedFourArgumentClrProjectionEntryPointRemainsAvailable()
        Dim signature = {GetType(RegressionRequest), GetType(TermSumOfSquaresType), GetType(Dictionary(Of String, Integer())), GetType(IEnumerable(Of String))}
        Assert.IsNotNull(GetType(LinearBinomialRegressionPolicy).GetMethod("Project", signature))
        Dim project As Func(Of RegressionRequest, TermSumOfSquaresType, Dictionary(Of String, Integer()), IEnumerable(Of String), LinearBinomialFitIntent) = AddressOf LinearBinomialRegressionPolicy.Project
        Dim r = Regression10PolicyTests.Request(Regression09Fixture.Load("glm_gaussian_full.csv"), True, False, False)
        Assert.AreEqual(0.1, project(r, TermSumOfSquaresType.TypeIII, Nothing, Nothing).EffectiveFitAlpha)
    End Sub
    Public Shared Function ConstantModel(response As Double, interceptOnly As Boolean, sections As Boolean, hl As Boolean) As GLM
        Dim data(23, If(interceptOnly, 0, 1)) As Double
        For i = 0 To 23
            data(i, 0) = response
            If Not interceptOnly Then data(i, 1) = i - 11.5
        Next
        Dim model As New GLM(New Binomial(), New Logit()) With {.bReturnCov = sections, .bIterationDetails = sections, .bComputeResiduals = sections, .bHosmerLemeshow = hl}
        model.data(data) : model.setVarNames(If(interceptOnly, {"y"}, {"y", "x"})) : model.settingInputs(0.05, 1, 0.00000000000001) : model.Fit(1, False)
        Return model
    End Function

    <DataTestMethod><DataRow(0.0)><DataRow(1.0)>
    Public Sub ConstantBinomialAllOptionalSectionsAreSafe(response As Double)
        For Each culture In {"", "sk-SK"}
            Dim saved = CultureInfo.CurrentCulture
            Try
                CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo(culture)
                For Each interceptOnly In {False, True}
                    For Each sections In {False, True}
                        For Each hl In {False, True}
                            Dim model = ConstantModel(response, interceptOnly, sections, hl), tables = model.wrapResults(Nothing, Nothing)
                            Assert.IsFalse(model.Converged)
                            Assert.IsFalse(model.HasReportData)
                            Assert.IsNull(model.AllResiduals) : Assert.IsNull(model.PredictedResponses)
                            Assert.AreEqual(1, tables.Count)
                            Assert.IsTrue(tables(0).ToOutputModel().Values.Cast(Of Object)().Any(Function(v) Object.Equals(v, "GLM results unavailable")))
                            Assert.IsFalse(tables(0).ToOutputModel().Values.Cast(Of Object)().Any(Function(v) TypeOf v Is Double))
                        Next
                    Next
                Next
            Finally
                CultureInfo.CurrentCulture = saved
            End Try
        Next
    End Sub

    <TestMethod>
    Public Sub BinomialOptionalPayloadsDoNotSurviveAnEarlyExitAndRecovery()
        Dim model = ConstantModel(1, False, True, True)
        Dim f = Regression09Fixture.Load("glm_binomial_full.csv")
        model.settingInputs(0.05, 100, 0.000001) : model.data(f.Data, f.Ids, f.Offset, f.Weights) : model.setVarNames(f.Names) : model.Fit(1, False)
        Assert.IsTrue(model.HasReportData)
        Assert.IsTrue(model.wrapResults().Count > 1)
        Dim data(23, 1) As Double
        For i = 0 To 23 : data(i, 0) = 1 : data(i, 1) = i - 11.5 : Next
        model.settingInputs(0.05, 1, 0.00000000000001) : model.data(data) : model.setVarNames({"y", "x"}) : model.Fit(1, False)
        Assert.IsFalse(model.HasReportData) : Assert.IsNull(model.PredictedResponses) : Assert.IsNull(model.AllResiduals)
        Assert.AreEqual(1, model.wrapResults().Count)
        model.settingInputs(0.05, 100, 0.000001) : model.data(f.Data, f.Ids, f.Offset, f.Weights) : model.setVarNames(f.Names) : model.Fit(1, False)
        Assert.IsTrue(model.HasReportData) : Assert.IsNotNull(model.PredictedResponses)
    End Sub

    <DataTestMethod><DataRow(False)><DataRow(True)>
    Public Sub CovarianceAndIterationFormattingNeverMutateIdentifiers(includeIntercept As Boolean)
        Dim f = Regression09Fixture.Load("glm_gaussian_full.csv")
        For Each cov In {False, True}
            For Each details In {False, True}
                For Each residual In {False, True}
                    Dim names = DirectCast(f.Names.Clone(), String()), original = DirectCast(names.Clone(), String())
                    Dim model As New GLM(New Gaussian(), New Identity()) With {.bReturnCov = cov, .bIterationDetails = details, .bComputeResiduals = residual}
                    model.data(f.Data, f.Ids) : model.setVarNames(names) : model.settingInputs(0.05, 100, 0.000001) : model.Fit(If(includeIntercept, 1, 0), False)
                    Dim coefficients = DirectCast(model.results.Coeffs_est.Clone(), Double()), matrix = DirectCast(model.VarCovar.Clone(), Double(,))
                    For pass = 0 To 2
                        Dim tables = model.wrapResults(Nothing, Nothing)
                        CollectionAssert.AreEqual(original, names)
                        Dim internalNames = DirectCast(GetType(GLM).GetField("pVarNames", BindingFlags.Instance Or BindingFlags.NonPublic).GetValue(model), String())
                        CollectionAssert.AreEqual(original, internalNames)
                        CollectionAssert.AreEqual(coefficients, model.results.Coeffs_est)
                        CollectionAssert.AreEqual(matrix.Cast(Of Double)().ToArray(), model.VarCovar.Cast(Of Double)().ToArray())
                        Assert.AreEqual(2 + If(cov, 1, 0) + If(details, 1, 0), tables.Count)
                    Next
                Next
            Next
        Next
    End Sub

    <DataTestMethod><DataRow(0)><DataRow(1)><DataRow(2)>
    Public Sub MissingOrShortCovarianceNamesAreNotBlindlyIndexed(index As Integer)
        Dim f = Regression09Fixture.Load("glm_gaussian_full.csv"), model As New GLM(New Gaussian(), New Identity()) With {.bReturnCov = True, .bIterationDetails = True}
        Dim names = If(index = 0, Nothing, If(index = 1, Array.Empty(Of String)(), {"y"}))
        model.data(f.Data, f.Ids) : model.setVarNames(names) : model.settingInputs(0.05, 100, 0.000001) : model.Fit(1, False)
        Dim tables = model.wrapResults(Nothing, Nothing)
        Assert.IsTrue(tables.Last().ToOutputModel().Values.Cast(Of Object)().Contains("Covariance parameter labels unavailable or incomplete."))
        If names IsNot Nothing AndAlso names.Length > 0 Then Assert.AreEqual("y", names(0))
    End Sub

    Public Shared Function FitLinear(intent As LinearBinomialFitIntent) As LinearModel
        Dim r = intent.Request, model As New LinearModel With {.Alpha = intent.EffectiveFitAlpha, .bReturnCov = r.Options.Covariance, .bComputeResiduals = r.Options.Residuals}
        model.Data(r.Primary.Data, r.Primary.Names, r.Primary.RowIds, If(r.Weights Is Nothing, Nothing, r.Weights.Values))
        model.Fit(intent.IncludeIntercept, intent.TermGroups, intent.SumOfSquares)
        Return model
    End Function

    <DataTestMethod><DataRow("")><DataRow("sk-SK")>
    Public Sub RequestedLmAlphaChangesOnlyConfidenceInference(culture As String)
        Dim saved = CultureInfo.CurrentCulture
        Try
            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo(culture)
            For Each intercept As Boolean In {False, True}
                For Each weighted In {False, True}
                    For Each ss In {TermSumOfSquaresType.TypeI, TermSumOfSquaresType.TypeIII}
                        Dim f = Regression09Fixture.Load("glm_gaussian_full.csv") : Array.Reverse(f.Ids)
                        Dim r = Regression10PolicyTests.Request(f, True, False, False, intercept, True, True, weighted)
                        Dim intent = LinearBinomialRegressionPolicy.Project(r, ss), corrected = FitLinear(intent)
                        Dim legacyIntent = LinearBinomialRegressionPolicy.Project(r, ss, alphaMode:=LinearFitAlphaMode.NativeLegacyDefault), legacy = FitLinear(legacyIntent)
                        Assert.AreEqual(0.1, corrected.Alpha) : Assert.AreEqual(0.05, legacy.Alpha)
                        CollectionAssert.AreEqual(legacy.results.Coeffs_est, corrected.results.Coeffs_est)
                        CollectionAssert.AreEqual(legacy.results.Coeffs_SEsT, corrected.results.Coeffs_SEsT)
                        CollectionAssert.AreEqual(legacy.Fitted, corrected.Fitted)
                        Assert.IsTrue(corrected.results.Coeffs_CIhighT(0) - corrected.results.Coeffs_CIlowT(0) < legacy.results.Coeffs_CIhighT(0) - legacy.results.Coeffs_CIlowT(0))
                        Dim report = LinearBinomialRegressionPolicy.Compose(intent, corrected.wrapResults(), corrected.Fitted, corrected.AllResiduals_toPrint, actualFitAlpha:=corrected.Alpha)
                        Assert.AreEqual(intent.RequestedAlpha, report.ActualFitAlpha)
                        CollectionAssert.AreEqual(f.Ids, report.Rows.RowIds)
                        Dim o = r.Options : o.Alpha = 0.05
                        Dim defaultRequest As New RegressionRequest(r.Kind, r.ResponseKey, r.Primary, o, weights:=r.Weights)
                        Dim atDefault = FitLinear(LinearBinomialRegressionPolicy.Project(defaultRequest, ss))
                        CollectionAssert.AreEqual(legacy.results.Coeffs_CIlowT, atDefault.results.Coeffs_CIlowT)
                    Next
                Next
            Next
        Finally
            CultureInfo.CurrentCulture = saved
        End Try
    End Sub

    <DataTestMethod><DataRow(Double.NaN)><DataRow(Double.PositiveInfinity)><DataRow(0.0)><DataRow(1.0)>
    Public Sub InvalidAlphaIsDiagnosticWithoutActivatingNativeStrictRefusal(alpha As Double)
        Dim r = Regression10PolicyTests.Request(Regression09Fixture.Load("glm_gaussian_full.csv"), True, False, False), o = r.Options : o.Alpha = alpha
        Dim bad As New RegressionRequest(r.Kind, r.ResponseKey, r.Primary, o, weights:=r.Weights)
        Dim intent = LinearBinomialRegressionPolicy.Project(bad)
        Assert.IsFalse(intent.Validation.IsValid)
        Assert.IsTrue(intent.Validation.Diagnostics.Any(Function(d) d.FieldPath = "options.alpha"))
        Assert.AreEqual(0.05, LinearBinomialRegressionPolicy.Project(bad, alphaMode:=LinearFitAlphaMode.NativeLegacyDefault).EffectiveFitAlpha)
        Assert.AreEqual(0.05, LinearBinomialRegressionPolicy.Project(New RegressionRequest(r.Kind, r.ResponseKey, r.Primary)).EffectiveFitAlpha)
    End Sub
End Class
