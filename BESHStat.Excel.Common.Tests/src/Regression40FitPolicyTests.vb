Option Explicit On
Option Strict On
Imports System
Imports Microsoft.VisualStudio.TestTools.UnitTesting
Imports BESHStatNG
Imports BESHStatNG.ExcelCommon
Imports BESHStatNG.regression

<TestClass>
Public Class Regression40FitPolicyTests
    Private Shared Function Data() As Double(,)
        Return New Double(,) {{1, 0}, {2.1, 1}, {2.8, 2}, {4.2, 3}, {4.8, 4}, {6.2, 5}, {7.1, 6}, {8.3, 7}}
    End Function

    <DataTestMethod>
    <DataRow(0.05)>
    <DataRow(0.1)>
    Public Sub LmActualFitRetainsRequestedAlphaWeightedRowsAndCoefficients(alpha As Double)
        Dim a As New LinearModel(), b As New LinearModel()
        a.Alpha = alpha : a.bComputeResiduals = True : a.bReturnCov = False
        Dim options As New RegressionOptions With {.Alpha = alpha, .Residuals = True, .Covariance = False}
        LinearBinomialRegressionPolicy.ConfigureUdfFit(b, options)
        Dim rows() As Integer = {3, 7, 11, 15, 19, 23, 27, 31}, weights() As Double = {1, 2, 1, 3, 2, 1, 2, 1}
        a.Data(Data(), {"Y", "X"}, rows, weights) : b.Data(Data(), {"Y", "X"}, rows, weights)
        a.Fit() : b.Fit()
        Assert.AreEqual(alpha, b.Alpha)
        CollectionAssert.AreEqual(a.results.Coeffs_est, b.results.Coeffs_est)
        CollectionAssert.AreEqual(a.results.Coeffs_SEs, b.results.Coeffs_SEs)
        CollectionAssert.AreEqual(a.Fitted, b.Fitted)
        CollectionAssert.AreEqual(a.results.varNames, b.results.varNames)
        options.Alpha = 0.2
        Assert.AreEqual(alpha, b.Alpha, "Configuration copies values; it must not retain a mutable options builder.")
    End Sub

    <DataTestMethod>
    <DataRow(0.05)>
    <DataRow(0.1)>
    Public Sub GaussianGlmRealFitKeepsDataAndDiagnostics(alpha As Double)
        Dim a As New GLM(FamilyUtils.createFamily("Gaussian"), LinkUtils.createLink("Identity"))
        Dim b As New GLM(FamilyUtils.createFamily("Gaussian"), LinkUtils.createLink("Identity"))
        a.bComputeResiduals = True : a.bReturnCov = False : a.bIterationDetails = False
        a.settingInputs(alpha, 30, 0.00000001)
        RegressionRequestPolicy.ConfigureUdfGlmFit(b, New RegressionOptions With {.Alpha = alpha, .MaximumIterations = 30, .Epsilon = 0.00000001, .Residuals = True})
        Dim raw = Data(), names() As String = {"Y", "X"}, rows() As Integer = {3, 7, 11, 15, 19, 23, 27, 31}
        a.data(raw, rows) : b.data(raw, rows)
        a.setVarNames(names) : b.setVarNames(names)
        a.Fit(1, False) : b.Fit(1, False)
        CollectionAssert.AreEqual(a.results.Coeffs_est, b.results.Coeffs_est)
        CollectionAssert.AreEqual(a.results.Coeffs_SEs, b.results.Coeffs_SEs)
        CollectionAssert.AreEqual(a.results.varNames, b.results.varNames)
        CollectionAssert.AreEqual({3, 7, 11, 15, 19, 23, 27, 31}, rows)
    End Sub

    <TestMethod>
    Public Sub AlreadyParsedSettingsDoNotIntroduceDomainRefusal()
        Dim model As New GLM(FamilyUtils.createFamily("Gaussian"), LinkUtils.createLink("Identity"))
        RegressionRequestPolicy.ConfigureUdfGlmFit(model, New RegressionOptions With {.Alpha = Double.NaN, .MaximumIterations = -3, .Epsilon = Double.NegativeInfinity})
        Assert.IsFalse(model.bComputeResiduals)
        Assert.ThrowsException(Of ArgumentNullException)(Sub() RegressionRequestPolicy.ConfigureUdfGlmFit(Nothing, New RegressionOptions()))
        Assert.ThrowsException(Of ArgumentNullException)(Sub() LinearBinomialRegressionPolicy.ConfigureUdfFit(New LinearModel(), Nothing))
    End Sub

    <TestMethod>
    Public Sub CategoricalAndZipSettingsKeepCallerDomainRulesSeparate()
        ExtendedRegressionFamilyPolicy.ConfigureUdfOrdinalFit(New OrdinalLogitModel(), New RegressionOptions With {.Alpha = 0.1, .MaximumIterations = 50, .Epsilon = 0.0000000001, .Residuals = True})
        ExtendedRegressionFamilyPolicy.ConfigureUdfMultinomialFit(New MultinomialLogitModel(), New RegressionOptions With {.Alpha = 0.1, .MaximumIterations = 50, .Epsilon = 0.0000000001, .Residuals = True})
        ExtendedRegressionFamilyPolicy.ApplyZipFitSettings(New ZeroInflatedPoisson(), New RegressionOptions With {.Alpha = 0.1, .MaximumIterations = 25, .EmIterations = 50, .Epsilon = 0.000000001})
        Assert.ThrowsException(Of ArgumentNullException)(Sub() ExtendedRegressionFamilyPolicy.ConfigureUdfOrdinalFit(Nothing, New RegressionOptions()))
        Assert.ThrowsException(Of ArgumentNullException)(Sub() ExtendedRegressionFamilyPolicy.ConfigureUdfMultinomialFit(New MultinomialLogitModel(), Nothing))
        Assert.ThrowsException(Of ArgumentNullException)(Sub() ExtendedRegressionFamilyPolicy.ApplyZipFitSettings(Nothing, New RegressionOptions()))
    End Sub
End Class
