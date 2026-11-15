Option Explicit On
Option Strict On
Imports System
Imports System.Linq
Imports BESHStatNG
Imports BESHStatNG.ExcelCommon
Imports BESHStatNG.regression
Imports BESHStatNG.survival
Imports Microsoft.VisualStudio.TestTools.UnitTesting

<TestClass>
Public Class Regression40MixedSurvivalTests
    <TestMethod>
    Public Sub LmmInferencePreparationRetainsFlagsAndRequestIdentity()
        For Each value In [Enum].GetValues(GetType(MixedModelFixedInferenceMethod)).Cast(Of MixedModelFixedInferenceMethod)().Concat({CType(99, MixedModelFixedInferenceMethod)})
            Dim a = Mixed14Requests.Request("LMM|Random Intercept|Large-sample normal|REML|toy").CreateCoreRequest()
            Dim b = Mixed14Requests.Request("LMM|Random Intercept|Large-sample normal|REML|toy").CreateCoreRequest()
            a.FixedInferenceMethod = value : b.FixedInferenceMethod = value
            Select Case value
                Case MixedModelFixedInferenceMethod.KenwardRoger : a.EnableFullKenwardRogerForLmm()
                Case MixedModelFixedInferenceMethod.Satterthwaite
                    a.UseSatterthwaite = True : a.UseKenwardRoger = False : a.FixedInferenceMethod = value
                Case Else
                    a.UseSatterthwaite = False : a.UseKenwardRoger = False : a.FixedInferenceMethod = value
            End Select
            Dim originalData = b.Data, names = b.FixedEffectNames
            UdfLmmInputPreparation38.ApplyInference(b, value)
            Assert.AreSame(originalData, b.Data)
            Assert.AreSame(names, b.FixedEffectNames)
            Assert.AreEqual(a.FixedInferenceMethod, b.FixedInferenceMethod)
            Assert.AreEqual(a.UseKenwardRoger, b.UseKenwardRoger)
            Assert.AreEqual(a.UseSatterthwaite, b.UseSatterthwaite)
            Assert.AreEqual(a.FitMethod, b.FitMethod)
        Next
    End Sub

    <TestMethod>
    Public Sub LmmActualFitIsUnchangedByExplicitInferencePreparation()
        Dim a = Mixed14Requests.Request("LMM|Random Intercept|Large-sample normal|REML|toy").CreateCoreRequest()
        Dim b = Mixed14Requests.Request("LMM|Random Intercept|Large-sample normal|REML|toy").CreateCoreRequest()
        UdfLmmInputPreparation38.ApplyInference(b, MixedModelFixedInferenceMethod.WaldNormal)
        Dim fitA = New LMM(a).Fit(), fitB = New LMM(b).Fit()
        CollectionAssert.AreEqual(fitA.Beta, fitB.Beta)
        CollectionAssert.AreEqual(fitA.BetaDF, fitB.BetaDF)
        CollectionAssert.AreEqual(fitA.FixedEffectNames, fitB.FixedEffectNames)
    End Sub

    <DataTestMethod>
    <DataRow("Cox|Breslow|plain")>
    <DataRow("Cox|Breslow|robust")>
    <DataRow("Cox|Efron|strata")>
    Public Sub CoxAcquiredConstructorUsesExactExistingIterationOptions(key As String)
        Dim intent = Survival13Requests.Cox(key), records = Survival13Fixture.Records(key), options = intent.Options
        Dim a As New CoxPH(records, intent.Expanded.Names, options.MaximumIterations, options.Epsilon)
        Dim b = CoxWorkflowPolicy.CreateAcquiredModel(records, intent.Expanded.Names, options)
        a.bRobustVariance = options.RobustVariance : b.bRobustVariance = options.RobustVariance
        Dim resultA = a.Fit(options.Ties), resultB = b.Fit(options.Ties)
        Assert.AreEqual(resultA.Converged, resultB.Converged)
        CollectionAssert.AreEqual(resultA.Coefficients, resultB.Coefficients)
        Assert.AreEqual(records.Count, Survival13Fixture.Records(key).Count)
    End Sub

    <TestMethod>
    Public Sub GeeExistingRequestOptionsStillDriveSharedConfiguration()
        Dim options As New RegressionOptions With {.Alpha = 0.1, .MaximumIterations = 37, .Epsilon = 0.000000001, .Residuals = True}
        Dim gee As New GeeWorkflowOptions With {.UseP = True}
        Dim model As New GEE(FamilyUtils.createFamily("Gaussian"), LinkUtils.createLink("Identity"), GEEcovStructUtils.createGEEcovMat("Independence"), "Robust")
        GeeWorkflowPolicy.ConfigureUdfFit(model, options, gee)
        Assert.IsTrue(model.bComputeResiduals)
        Assert.IsFalse(model.bIterationDetails)
        options.Residuals = False
        Assert.IsTrue(model.bComputeResiduals)
        Assert.ThrowsException(Of ArgumentNullException)(Sub() GeeWorkflowPolicy.ConfigureUdfFit(model, Nothing, gee))
        Assert.ThrowsException(Of ArgumentNullException)(Sub() CoxWorkflowPolicy.CreateAcquiredModel(Nothing, Nothing, Nothing))
    End Sub
End Class
