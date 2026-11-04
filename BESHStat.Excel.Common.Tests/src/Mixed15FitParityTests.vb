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
Public Class Mixed15FitParityTests
    Public Shared Function Cases() As IEnumerable(Of Object())
        Dim rows As New List(Of Object())
        For Each r In MixedModelRStruct.RStructsList
            For Each optimizer In {"AI/Fisher scoring (default)", "Projected BFGS (auto gradient)", "Projected BFGS (analytic gradient)", "Projected BFGS (finite-difference gradient)"}
                For Each method In {"REML", "ML"}
                    rows.Add({"MMRM", r, "None", optimizer, method, 0})
                Next
            Next
        Next
        For Each g In MixedModelGStruct.GStructsList.Skip(1)
            Dim q = If(g = "Random Intercept", 1, If(g = "Random Intercept + Slope", 2, 3))
            For Each optimizer In {"AI/Fisher scoring (default)", "Projected BFGS (auto gradient)", "Projected BFGS (analytic gradient)", "Projected BFGS (finite-difference gradient)"}
                For Each method In {"REML", "ML"}
                    rows.Add({"LMM", "Identity", g, optimizer, method, q})
                Next
            Next
        Next
        For Each r In {"CS", "AR1", "UN", "CSH", "HAR1", "TOEP", "TOEPH", "Diagonal"}
            For Each g In {"Random Intercept", "Random Intercept + Slope"}
                rows.Add({"LMM", r, g, "Projected BFGS (auto gradient)", "REML", If(g = "Random Intercept", 1, 2)})
            Next
        Next
        Return rows
    End Function

    Public Shared Function Build(kind As MixedModelWorkflowKind, r As String, g As String, optimizer As String, method As String, q As Integer,
                                 Optional fixture As Mixed14Fixture = Nothing) As MixedModelWorkflowRequest
        Dim f = If(fixture, Mixed14Fixture.Load(If(kind = MixedModelWorkflowKind.MMRM,
            "MMRM|CS|Large-sample normal|REML|missing", "LMM|Variance Components (VC/Diag)|Large-sample normal|REML|toy")))
        f.RandomNames = If(q = 0, Nothing, Enumerable.Range(0, q).Select(Function(j) If(j = 0, "Intercept", "visit" & j.ToString(CultureInfo.InvariantCulture))).ToArray())
        f.Z = Nothing
        If q > 0 Then
            ReDim f.Z(f.Y.Length - 1, q - 1)
            For i = 0 To f.Y.Length - 1
                For j = 0 To q - 1
                    f.Z(i, j) = Math.Pow(f.X(i, 1), j)
                Next
            Next
        End If
        Dim effects As New List(Of RegressionAuthoredEffect)
        For j = 1 To q - 1
            effects.Add(New RegressionAuthoredEffect("visit" & j.ToString(CultureInfo.InvariantCulture),
                New TermSpec With {.Kind = "MainEffect", .BaseVarKeys = New List(Of String) From {"visit"}, .Degree = j}))
        Next
        Dim o = MixedModelWorkflowOptions.Defaults(kind)
        o.ResidualStructure = r : o.RandomStructure = g : o.Optimizer = optimizer : o.FitMethod = method
        o.Inference = "Large-sample normal" : o.MaximumIterations = 12 : o.Trace = True
        Return New MixedModelWorkflowRequest(kind, MixedModelWorkflowData.FromArrays(kind, f.Y, f.X, f.Subjects, f.Visit, f.Rows, f.Z),
            "response", "subject", If(f.Visit Is Nothing, "", "visit"), f.FixedNames, f.RandomNames, Mixed14Requests.Effects(True),
            If(q = 0, Nothing, New RegressionEffectModel(effects, True)), "visit", If(q = 0, "", "random design"), o)
    End Function

    ' Only this new 04A.15 case set is versioned here. Never write any old capture.
    <DataTestMethod><DynamicData(NameOf(Cases), DynamicDataSourceType.Method)>
    Public Sub SpecializedRequestMatchesOriginalFactoryAndControlFit(kindName As String, r As String, g As String, optimizer As String, method As String, q As Integer)
        Dim saved = CultureInfo.CurrentCulture
        Try
            For Each culture In {"", "sk-SK"}
                CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo(culture)
                Dim kind = If(kindName = "MMRM", MixedModelWorkflowKind.MMRM, MixedModelWorkflowKind.LMM)
                Dim workflow = Build(kind, r, g, optimizer, method, q), actual = workflow.CreateCoreRequest()
                Dim expected = If(kind = MixedModelWorkflowKind.MMRM,
                    MixedModelFitRequest.CreateMMRM(workflow.Data.CoreData, MixedModelRStructUtils.createMixedModelRStruct(r), actual.FitMethod),
                    MixedModelFitRequest.CreateLMM(workflow.Data.CoreData, MixedModelRStructUtils.createMixedModelRStruct(r), MixedModelGStructUtils.createMixedModelGStruct(g), actual.FitMethod))
                expected.ResponseVarName = "response" : expected.SubjectVarName = "subject" : expected.VisitVarName = "visit"
                expected.FixedEffectNames = workflow.FixedNames : expected.RandomEffectNames = workflow.RandomNames
                expected.FixedFormulaText = "visit" : expected.RandomFormulaText = If(q = 0, "", "random design")
                expected.RequestLabel = kindName : expected.FixedInferenceMethod = MixedModelFixedInferenceMethod.WaldNormal
                Dim ctl = expected.Control : Mixed15Legacy.Apply(kind, ctl, workflow.Options) : expected.Control = ctl
                Dim key = kindName & "|" & If(q = 0, r, g) & "|Large-sample normal|" & method & "|toy"
                Dim before As MixedModelResult = Nothing, after As MixedModelResult = Nothing
                Dim beforeError As String = Nothing, afterError As String = Nothing
                Try
                    before = Mixed14Fixture.Fit(key, expected)
                Catch ex As ApplicationException
                    beforeError = ex.GetType().FullName & ":" & ex.Message
                End Try
                Try
                    after = Mixed14Fixture.Fit(key, actual)
                Catch ex As ApplicationException
                    afterError = ex.GetType().FullName & ":" & ex.Message
                End Try
                Assert.AreEqual(beforeError, afterError)
                If beforeError IsNot Nothing Then
                    Console.WriteLine("04A15_CASE|" & String.Join("|", {kindName, r, g, optimizer, method, culture, "CORE_REJECTED"}))
                    Continue For
                End If
                Assert.AreEqual(before.Converged, after.Converged)
                Assert.AreEqual(before.Cancelled, after.Cancelled) : Assert.AreEqual(before.Interrupted, after.Interrupted)
                Assert.AreEqual(before.Objective, after.Objective, Math.Max(0.000000001, Math.Abs(before.Objective) * 0.000000001))
                Regression10Fixture.AssertCapture(Mixed14Fixture.Capture(key, expected, before), Mixed14Fixture.Capture(key, actual, after), key)
                CollectionAssert.AreEqual(workflow.Data.BlockRowIds, actual.Data.Blocks.SelectMany(Function(b) b.RowIndices).ToArray())
                Console.WriteLine("04A15_CASE|" & String.Join("|", {kindName, r, g, optimizer, method, culture, If(after.Converged, "CONVERGED", "NONCONVERGED"),
                    after.Objective.ToString("R", CultureInfo.InvariantCulture)}))
            Next
        Finally
            CultureInfo.CurrentCulture = saved
        End Try
    End Sub

    <DataTestMethod><DataRow("AI/Fisher scoring (default)")><DataRow("Projected BFGS (auto gradient)")><DataRow("Projected BFGS (analytic gradient)")><DataRow("Projected BFGS (finite-difference gradient)")>
    Public Sub CallbacksAndOneIterationStatusStayCoreOwned(optimizer As String)
        For Each kind In {MixedModelWorkflowKind.MMRM, MixedModelWorkflowKind.LMM}
            Dim workflow = Build(kind, "CS", "Random Intercept", optimizer, "REML", If(kind = MixedModelWorkflowKind.MMRM, 0, 1))
            Dim req = workflow.CreateCoreRequest(), progress As Integer
            req.ProgressReporter = Sub(info) progress += 1
            req.CancellationRequested = Function() True
            Dim key = kind.ToString() & "|CS|Large-sample normal|REML|toy", result = Mixed14Fixture.Fit(key, req)
            Assert.IsTrue(result.Cancelled) : Assert.IsFalse(result.Converged)
            req = workflow.CreateCoreRequest() : req.InterruptionRequested = Function() True
            result = Mixed14Fixture.Fit(key, req)
            Assert.IsTrue(result.Interrupted) : Assert.IsFalse(result.Converged)
            req = workflow.CreateCoreRequest()
            Dim ctl = req.Control : ctl.MaxIter = 1 : req.Control = ctl
            result = Mixed14Fixture.Fit(key, req)
            Assert.IsFalse(result.Converged)
        Next
    End Sub

    <DataTestMethod><DataRow("zero")><DataRow("constant")><DataRow("invalid-Q")>
    Public Sub NegativeAndDeficientDesignOutcomesRemainOriginal(scenario As String)
        Dim f = Mixed14Fixture.Load("LMM|Random Intercept + Slope|Large-sample normal|REML|toy")
        If scenario <> "invalid-Q" Then
            For i = 0 To f.Y.Length - 1
                f.Y(i) = If(scenario = "zero", 0, 8)
            Next
        End If
        Dim request = Build(MixedModelWorkflowKind.LMM, "AR1", If(scenario = "invalid-Q", "RI", "Random Intercept + Slope"), "Projected BFGS (auto gradient)", "REML", 2, f)
        If scenario = "invalid-Q" Then
            Assert.IsFalse(request.Validation.IsValid)
            Assert.IsTrue(request.Validation.Diagnostics.Any(Function(d) d.Code = "mixed.random.dimension"))
            Dim core = request.CreateAcquiredCoreRequest()
            Assert.ThrowsException(Of ApplicationException)(Sub() core.Validate())
        Else
            Dim original = request.CreateAcquiredCoreRequest(MixedModelRStructUtils.createMixedModelRStruct("AR1"), New RandomInterceptSlope())
            Dim current = request.CreateAcquiredCoreRequest()
            Regression10Fixture.AssertCapture(Mixed14Fixture.Capture("LMM|Random Intercept + Slope|Large-sample normal|REML|toy", original),
                Mixed14Fixture.Capture("LMM|Random Intercept + Slope|Large-sample normal|REML|toy", current), scenario)
        End If
    End Sub

    <DataTestMethod><DataRow("Auto")><DataRow("Analytic")><DataRow("Analytic score + finite-difference validation")><DataRow("Numerical")>
    Public Sub SeparateGradientChoicesReachCoreUnchanged(gradient As String)
        For Each kind In {MixedModelWorkflowKind.MMRM, MixedModelWorkflowKind.LMM}
            Dim workflow = Build(kind, "AR1", "Variance Components (VC/Diag)", "Projected BFGS (auto gradient)", "REML", If(kind = MixedModelWorkflowKind.MMRM, 0, 3))
            Dim o = workflow.Options : o.Gradient = gradient
            Dim request As New MixedModelWorkflowRequest(kind, workflow.Data, workflow.ResponseKey, workflow.SubjectKey, workflow.VisitKey,
                workflow.FixedNames, workflow.RandomNames, workflow.FixedEffects, workflow.RandomEffects, workflow.FixedText, workflow.RandomText, o)
            Dim actual = request.CreateCoreRequest(), expected = workflow.CreateCoreRequest(), ctl = expected.Control
            Mixed15Legacy.Apply(kind, ctl, o) : expected.Control = ctl
            Dim key = kind.ToString() & "|AR1|Large-sample normal|REML|toy"
            Regression10Fixture.AssertCapture(Mixed14Fixture.Capture(key, expected), Mixed14Fixture.Capture(key, actual), gradient)
        Next
    End Sub
End Class
