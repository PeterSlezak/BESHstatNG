Option Explicit On
Option Strict On
Imports System
Imports System.Collections.Generic
Imports System.Globalization
Imports System.IO
Imports System.Linq
Imports BESHStatNG
Imports BESHStatNG.ExcelCommon
Imports BESHStatNG.regression
Imports Microsoft.VisualStudio.TestTools.UnitTesting

<TestClass>
Public Class Regression10PolicyTests
    Public Shared Function Request(f As Regression09Fixture, linear As Boolean, binomial As Boolean, alternate As Boolean,
                                   Optional intercept As Boolean = True, Optional covariance As Boolean = True,
                                   Optional residuals As Boolean = True, Optional vectors As Boolean = True,
                                   Optional initial() As Double = Nothing) As RegressionRequest
        Dim options As New RegressionOptions With {.Alpha = 0.1, .Covariance = covariance, .Residuals = residuals,
            .IterationDetails = True, .Family = If(binomial, RegressionFamily.Binomial, RegressionFamily.Gaussian),
            .Link = If(binomial, If(alternate, RegressionLink.Probit, RegressionLink.Logit), If(alternate, RegressionLink.Log, RegressionLink.Identity))}
        If linear Then options.Family = Nothing : options.Link = Nothing
        Dim effects = RegressionEffectModel.FromProjection(f.Names.Skip(1), New Dictionary(Of String, TermSpec), intercept)
        Return New RegressionRequest(If(linear, RegressionAnalysisKind.LinearModel, RegressionAnalysisKind.GeneralizedLinearModel), "y",
            New RegressionComponent(f.Data, f.Names, f.Ids, effects, f.Names, initial), options,
            If(Not linear AndAlso vectors, New RegressionVector("offset", f.Offset, f.Ids), Nothing),
            If(vectors, New RegressionVector("weight", f.Weights, f.Ids), Nothing))
    End Function

    Public Shared Function Fit(intent As LinearBinomialFitIntent) As LinearBinomialReport
        Dim request = intent.Request, o = request.Options
        If intent.IsLinearModel Then
            Dim lm As New LinearModel With {.bReturnCov = o.Covariance, .bComputeResiduals = o.Residuals}
            lm.Data(request.Primary.Data, request.Primary.Names, request.Primary.RowIds, If(request.Weights Is Nothing, Nothing, request.Weights.Values))
            lm.SetPredictorCodingFootnotes(If(intent.PredictorCodingFootnotes Is Nothing, Nothing, intent.PredictorCodingFootnotes.ToList()))
            lm.Fit(intent.IncludeIntercept, intent.TermGroups, intent.SumOfSquares)
            Return LinearBinomialRegressionPolicy.Compose(intent, lm.wrapResults(), lm.Fitted, If(o.Residuals, lm.AllResiduals_toPrint, Nothing), actualFitAlpha:=lm.Alpha)
        Else
            Dim glm As New GLM(intent.CreateFamily(), intent.CreateLink()) With {.bReturnCov = o.Covariance, .bComputeResiduals = o.Residuals, .bIterationDetails = o.IterationDetails}
            glm.data(request.Primary.Data, request.Primary.RowIds, If(request.Offset Is Nothing, Nothing, request.Offset.Values), If(request.Weights Is Nothing, Nothing, request.Weights.Values))
            glm.setVarNames(request.Primary.Names) : glm.settingInputs(intent.EffectiveFitAlpha, o.MaximumIterations, o.Epsilon)
            Dim starts = request.Primary.InitialValues
            If starts IsNot Nothing Then glm.startParams = starts
            glm.Fit(If(intent.IncludeIntercept, 1, 0), starts IsNot Nothing)
            Return LinearBinomialRegressionPolicy.Compose(intent, glm.wrapResults(If(request.Offset Is Nothing, Nothing, request.Offset.Key), If(request.Weights Is Nothing, Nothing, request.Weights.Key)),
                glm.PredictedResponses, If(o.Residuals, glm.AllResiduals, Nothing), glm.Converged, intent.EffectiveFitAlpha)
        End If
    End Function

    <DataTestMethod>
    <DataRow("LM-ordinary")><DataRow("LM-alternate")><DataRow("Gaussian-ordinary")><DataRow("Gaussian-alternate")><DataRow("Binomial-ordinary")><DataRow("Binomial-alternate")>
    Public Sub RealCoreResponsesRoundTripEveryCellAndAnnotation(name As String)
        Dim saved = CultureInfo.CurrentCulture
        Try
            CultureInfo.CurrentCulture = CultureInfo.InvariantCulture
            Dim f = Regression10Fixture.Fit(name), linear = name.StartsWith("LM"), alternate = name.EndsWith("alternate")
            Dim r = Request(f, linear, name.StartsWith("Binomial"), alternate, Not (linear AndAlso alternate), vectors:=Not linear OrElse alternate)
            Dim o = r.Options : o.Alpha = 0.05
            r = New RegressionRequest(r.Kind, r.ResponseKey, r.Primary, o, r.Offset, r.Weights)
            Dim intent = LinearBinomialRegressionPolicy.Project(r, If(alternate AndAlso linear, TermSumOfSquaresType.TypeI, TermSumOfSquaresType.TypeIII),
                codingFootnotes:=If(linear, New List(Of String) From {"Coding note first", "Coding note second"}, Nothing))
            Dim actual = Fit(intent)
            Dim rebuilt As New Regression09Fixture With {.Ids = f.Ids, .Tables = actual.LegacyTables(), .Fitted = actual.Rows.Fitted, .Residual = actual.Rows.Residuals, .Converged = f.Converged}
            Regression10Fixture.AssertCapture(Regression09Fixture.Canonical(f), Regression09Fixture.Canonical(rebuilt), name)
            Assert.AreEqual(If(linear, RegressionConvergence.NotIterative, If(f.Converged.Value, RegressionConvergence.Converged, RegressionConvergence.NotConverged)), actual.Convergence)
            Assert.AreEqual(1, actual.Rows.ResidualHeaderRows)
        Finally
            CultureInfo.CurrentCulture = saved
        End Try
    End Sub

    <DataTestMethod><DataRow(False, False)><DataRow(False, True)><DataRow(True, False)><DataRow(True, True)>
    Public Sub LinearOptionsMatrix(intercept As Boolean, weighted As Boolean)
        For Each sums In {TermSumOfSquaresType.TypeI, TermSumOfSquaresType.TypeIII}
            For Each cov In {False, True}
                For Each residual In {False, True}
                    Dim f = Regression09Fixture.Load("glm_gaussian_full.csv") : Array.Reverse(f.Ids)
                    Dim request = Regression10PolicyTests.Request(f, True, False, False, intercept, cov, residual, weighted)
                    Dim intent = LinearBinomialRegressionPolicy.Project(request, sums)
                    Assert.IsTrue(intent.Validation.IsValid)
                    Dim report = Fit(intent)
                    Assert.AreEqual(0.1, intent.RequestedAlpha) : Assert.AreEqual(0.05, report.ActualFitAlpha)
                    Assert.AreEqual(sums, report.Intent.SumOfSquares)
                    Assert.AreEqual(residual, report.Rows.Residuals IsNot Nothing)
                    Assert.AreEqual(5 + If(cov, 1, 0), report.Document.Tables.Count)
                    CollectionAssert.AreEqual(f.Ids, report.Rows.RowIds)
                    Regression09PolicyTests.AssertTables(report.LegacyTables(), report.Document)
                Next
            Next
        Next
    End Sub

    <DataTestMethod><DataRow(False, False)><DataRow(False, True)><DataRow(True, False)><DataRow(True, True)>
    Public Sub GlmOptionsMatrix(binomial As Boolean, alternate As Boolean)
        For Each intercept As Boolean In {False, True}
            For Each vectors In {False, True}
                For Each sections In {False, True}
                    Dim f = Regression09Fixture.Load(If(binomial, "glm_binomial_full.csv", "glm_gaussian_full.csv"))
                    Dim request = Regression10PolicyTests.Request(f, False, binomial, alternate, intercept, sections, sections, vectors)
                    Dim o = request.Options : o.IterationDetails = sections
                    request = New RegressionRequest(request.Kind, "y", request.Primary, o, request.Offset, request.Weights)
                    Dim intent = LinearBinomialRegressionPolicy.Project(request), report = Fit(intent)
                    Assert.IsTrue(intent.Validation.IsValid)
                    Assert.AreEqual(0.1, report.ActualFitAlpha)
                    Assert.AreEqual(If(binomial, "probability", "response"), intent.ResponseScale)
                    Assert.AreEqual(sections, report.Rows.Residuals IsNot Nothing)
                    Assert.AreNotEqual(RegressionConvergence.Unknown, report.Convergence)
                    Assert.AreEqual(report.Convergence = RegressionConvergence.NotConverged,
                        report.Report.Diagnostics.Any(Function(d) d.Code = "regression.fit.not-converged"))
                    Regression09PolicyTests.AssertTables(report.LegacyTables(), report.Document)
                    If binomial Then Assert.IsTrue(report.Rows.Fitted.Cast(Of Double)().All(Function(v) v >= 0 AndAlso v <= 1))
                Next
            Next
        Next
    End Sub

    <DataTestMethod><DataRow("")><DataRow("sk-SK")>
    Public Sub NumericBaselinesAndProjectionUnderBothCultures(culture As String)
        Dim saved = CultureInfo.CurrentCulture
        Try
            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo(culture)
            For Each name In {"LM-ordinary", "LM-alternate", "Gaussian-ordinary", "Gaussian-alternate", "Binomial-ordinary", "Binomial-alternate"}
                Dim f = Regression10Fixture.Fit(name), actual = Regression09Fixture.Canonical(f)
                Dim expected = File.ReadAllLines(Path.Combine(Regression09Fixture.Root(), "evidence", "04A10", "baselines", name & If(Environment.Version.Major < 5, "-net48", "") & ".txt"))
                ' Core embeds culture-sensitive strings. Only numeric values are compared across
                ' cultures; exact strings/metadata are compared within each culture below.
                Regression10Fixture.AssertCapture(expected.Where(Function(s) s.StartsWith("D:")).ToArray(), actual.Where(Function(s) s.StartsWith("D:")).ToArray(), name)
                Dim request = Regression10PolicyTests.Request(f, name.StartsWith("LM"), name.StartsWith("Binomial"), name.EndsWith("alternate"))
                Dim report = LinearBinomialRegressionPolicy.Compose(LinearBinomialRegressionPolicy.Project(request), f.Tables,
                    Enumerable.Range(0, f.Fitted.GetLength(0)).Select(Function(i) f.Fitted(i, 0)).ToArray(), f.Residual, f.Converged)
                Regression09PolicyTests.AssertTables(report.LegacyTables(), report.Document)
                Dim snapshot = report.Report.ToContractSnapshot()
                Assert.AreEqual(request.Options.PValuePresentation.DecimalPlaces, snapshot.PValueDecimalPlaces)
            Next
        Finally
            CultureInfo.CurrentCulture = saved
        End Try
    End Sub

    <TestMethod>
    Public Sub IntentAndResponseDoNotRetainMutableCoreArrays()
        Dim f = Regression10Fixture.Fit("LM-ordinary")
        Dim groups As New Dictionary(Of String, Integer()) From {{"first", {0}}}
        Dim notes As New List(Of String) From {"first", "second"}
        Dim r = Request(f, True, False, False), intent = LinearBinomialRegressionPolicy.Project(r, groups:=groups, codingFootnotes:=notes)
        groups("first")(0) = 99 : notes.Clear() : intent.TermGroups("first")(0) = 42
        Assert.AreEqual(0, intent.TermGroups("first")(0)) : Assert.AreEqual(2, intent.PredictorCodingFootnotes.Count)
        Dim values = Enumerable.Range(0, f.Fitted.GetLength(0)).Select(Function(i) f.Fitted(i, 0)).ToArray()
        Dim report = LinearBinomialRegressionPolicy.Compose(intent, f.Tables, values, f.Residual)
        values(0) = 99 : f.Residual(1, 0) = 99 : f.Tables(0).SetBody(New Object(,) {{"changed"}})
        Assert.AreNotEqual(99.0, report.Rows.Fitted(0, 0))
        report.Rows.Fitted(0, 0) = 99
        Dim first = report.LegacyTables() : first(0).SetBody(New Object(,) {{"changed again"}})
        Regression09PolicyTests.AssertTables(report.LegacyTables(), report.Document)
        Assert.AreNotEqual(99.0, report.Rows.Fitted(0, 0))
    End Sub

    <DataTestMethod><DataRow(0)><DataRow(1)><DataRow(2)><DataRow(3)><DataRow(4)>
    Public Sub MalformedCoreResponseFailsBeforeWriting(index As Integer)
        Dim f = Regression10Fixture.Fit("Gaussian-ordinary"), intent = LinearBinomialRegressionPolicy.Project(Request(f, False, False, False))
        Dim fitted = Enumerable.Range(0, f.Fitted.GetLength(0)).Select(Function(i) f.Fitted(i, 0)).ToArray(), residual = f.Residual
        Select Case index
            Case 0 : fitted = {1.0}
            Case 1 : residual = New Object(2, 6) {}
            Case 2 : residual = New Object(f.Ids.Length, 5) {}
            Case 3 : residual(0, 0) = 4.0
            Case 4 : residual = DirectCast(Array.CreateInstance(GetType(Object), {f.Ids.Length + 1, 7}, {1, 1}), Object(,))
        End Select
        Assert.ThrowsException(Of ArgumentException)(Sub() LinearBinomialRegressionPolicy.Compose(intent, f.Tables, fitted, residual, True))
    End Sub

    <TestMethod>
    Public Sub EmptyOptionalSectionsNonfiniteAndConvergenceAreIndependent()
        Dim f = Regression09Fixture.Load("glm_binomial_full.csv"), r = Request(f, False, True, False), intent = LinearBinomialRegressionPolicy.Project(r)
        Dim fitted(f.Ids.Length - 1) As Double : fitted(0) = Double.NaN : fitted(1) = Double.PositiveInfinity
        Dim report = LinearBinomialRegressionPolicy.Compose(intent, New List(Of ResultTable), fitted, Nothing, False)
        Assert.AreEqual(0, report.Document.Tables.Count) : Assert.AreEqual(0, report.LegacyTables().Count)
        Assert.AreEqual(RegressionConvergence.NotConverged, report.Convergence)
        Assert.IsTrue(Double.IsNaN(report.Rows.Fitted(0, 0)))
        Assert.AreEqual(AnalysisDocumentStatus.Succeeded, report.Document.Status)
        Assert.IsTrue(report.Report.Diagnostics.Any(Function(d) d.Code = "regression.output.no-residuals"))
    End Sub

    <TestMethod>
    Public Sub DiagnosticsDoNotBecomeIntentRefusalsAndGlmZeroWeightsStayAccepted()
        Dim f = Regression09Fixture.Load("glm_gaussian_full.csv"), request = Regression10PolicyTests.Request(f, False, False, False)
        Dim o = request.Options : o.MaximumIterations = 0 : o.Epsilon = Double.PositiveInfinity : o.Link = RegressionLink.Logit
        request = New RegressionRequest(request.Kind, "y", request.Primary, o, request.Offset, request.Weights)
        Dim intent = LinearBinomialRegressionPolicy.Project(request)
        Assert.IsFalse(intent.Validation.IsValid)
        Assert.IsTrue(intent.Validation.Diagnostics.Any(Function(d) d.FieldPath = "options.link"))
        f.Weights(0) = 0
        Assert.IsTrue(LinearBinomialRegressionPolicy.Project(Regression10PolicyTests.Request(f, False, False, False)).Validation.IsValid)
        Assert.IsFalse(LinearBinomialRegressionPolicy.Project(Regression10PolicyTests.Request(f, True, False, False)).Validation.IsValid)
    End Sub

    <TestMethod>
    Public Sub StartingValuesAndPostFitClassificationRemainSeparateSnapshots()
        Dim f = Regression09Fixture.Load("glm_binomial_full.csv"), starts = {0.0, 0, 0}
        Dim request = Regression10PolicyTests.Request(f, False, True, False, initial:=starts), intent = LinearBinomialRegressionPolicy.Project(request)
        Assert.IsTrue(intent.Validation.IsValid)
        Dim report = Fit(intent), before = request.Options
        Assert.IsFalse(before.Classification)
        Dim after = before.Copy() : after.Classification = True : after.Threshold = 0.7
        after.ThresholdTable = True : after.Calibration = True : after.CalibrationBins = 5 : after.Brier = True
        Dim y = Enumerable.Range(0, f.Data.GetLength(0)).Select(Function(i) f.Data(i, 0)).ToArray()
        Dim p = Enumerable.Range(0, f.Data.GetLength(0)).Select(Function(i) report.Rows.Fitted(i, 0)).ToArray()
        Dim classification = RegressionClassificationPolicy.Compose(y, p, f.Weights, after)
        Assert.IsFalse(request.Options.Classification)
        Assert.IsTrue(classification.Document.Tables.Count >= 3) : Assert.IsNotNull(classification.CalibrationRows)
        after.Threshold = Double.NaN
        Assert.ThrowsException(Of ArgumentException)(Sub() RegressionClassificationPolicy.Compose(y, p, f.Weights, after))
        Assert.IsFalse(LinearBinomialRegressionPolicy.Project(Regression10PolicyTests.Request(f, False, True, False, initial:={0.0})).Validation.IsValid)
        Assert.IsFalse(LinearBinomialRegressionPolicy.Project(Regression10PolicyTests.Request(f, False, True, False, initial:=Array.Empty(Of Double)())).Validation.IsValid)
    End Sub

    <DataTestMethod><DataRow(0)><DataRow(1)><DataRow(2)><DataRow(3)>
    Public Sub LinearDegenerateDesignsPreserveCoreOutcome(index As Integer)
        Dim data(11, If(index = 0, 0, 2)) As Double
        For i = 0 To 11
            data(i, 0) = If(index = 3, 4, 1 + i + (i Mod 3) * 0.2)
            If index <> 0 Then
                data(i, 1) = i
                data(i, 2) = If(index = 2, i + If(i Mod 2 = 0, 0.00000001, 0), 2 * i)
            End If
        Next
        Dim names = If(index = 0, {"y"}, {"y", "x1", "x2"}), ids = Enumerable.Range(0, 12).Select(Function(i) 100 - i * 3).ToArray()
        Dim f As New Regression09Fixture With {.Data = data, .Names = names, .Ids = ids}
        Dim intent = LinearBinomialRegressionPolicy.Project(Request(f, True, False, False, vectors:=False))
        Dim direct As New LinearModel With {.bComputeResiduals = True, .bReturnCov = True}
        direct.Data(data, names, ids)
        Dim failure As Exception = Nothing
        Try
            direct.Fit(True)
        Catch ex As Exception
            failure = ex
        End Try
        If failure IsNot Nothing Then
            Try
                Fit(intent)
                Assert.Fail("Expected the unchanged Core failure: " & failure.GetType().Name)
            Catch ex As Exception
                Assert.AreEqual(failure.GetType(), ex.GetType()) : Assert.AreEqual(failure.Message, ex.Message)
            End Try
            Console.WriteLine("Core outcome " & index & ": " & failure.GetType().Name & ": " & failure.Message)
        Else
            Dim expected = direct.wrapResults(), actual = Fit(intent)
            Regression09PolicyTests.AssertTables(expected, actual.Document)
            Assert.AreEqual(RegressionConvergence.NotIterative, actual.Convergence)
            Console.WriteLine("Core outcome " & index & ": " & expected.Count & " tables")
        End If
    End Sub

    <DataTestMethod><DataRow(False)><DataRow(True)>
    Public Sub ConstantAndSeparatedBinomialPreserveActualFitWarnings(constantResponse As Boolean)
        Dim data(23, 1) As Double
        For i = 0 To 23
            data(i, 0) = If(constantResponse OrElse i >= 12, 1, 0) : data(i, 1) = i - 11.5
        Next
        Dim f As New Regression09Fixture With {.Data = data, .Names = {"y", "x"}, .Ids = Enumerable.Range(0, 24).Select(Function(i) 17 + i * 2).ToArray()}
        Dim request = Regression10PolicyTests.Request(f, False, True, False, vectors:=False)
        Dim o = request.Options : o.MaximumIterations = 1 : o.Epsilon = 0.00000000000001
        request = New RegressionRequest(request.Kind, "y", request.Primary, o)
        Dim intent = LinearBinomialRegressionPolicy.Project(request), model As New GLM(New Binomial(), New Logit()) With {.bComputeResiduals = True, .bReturnCov = True, .bIterationDetails = True}
        model.data(data, f.Ids) : model.setVarNames(f.Names) : model.settingInputs(o.Alpha, o.MaximumIterations, o.Epsilon)
        model.Fit(1, False)
        If constantResponse Then
            ' Existing Core early exit has no Hosmer-Lemeshow payload; wrapResults throws.
            ' C10-CORE-CONSTANT-01: characterize both paths, never fabricate a section.
            Assert.IsFalse(model.Converged)
            Assert.ThrowsException(Of NullReferenceException)(Sub() model.wrapResults(Nothing, Nothing))
            Assert.ThrowsException(Of NullReferenceException)(Sub() Fit(intent))
            Return
        End If
        Dim projected = Fit(intent)
        Assert.AreEqual(If(model.Converged, RegressionConvergence.Converged, RegressionConvergence.NotConverged), projected.Convergence)
        Assert.IsFalse(model.Converged)
        Dim expected As New Regression09Fixture With {.Ids = f.Ids, .Tables = model.wrapResults(Nothing, Nothing), .Fitted = Regression09Fixture.Column(model.PredictedResponses), .Residual = model.AllResiduals, .Converged = model.Converged}
        Dim actual As New Regression09Fixture With {.Ids = f.Ids, .Tables = projected.LegacyTables(), .Fitted = projected.Rows.Fitted, .Residual = projected.Rows.Residuals, .Converged = model.Converged}
        Regression10Fixture.AssertCapture(Regression09Fixture.Canonical(expected), Regression09Fixture.Canonical(actual), "separation")
    End Sub

    <TestMethod>
    Public Sub ZeroObservationResponseHasAnExplicitHeaderAndNoRenumbering()
        Dim effects = RegressionEffectModel.FromProjection(Array.Empty(Of String)(), New Dictionary(Of String, TermSpec), True)
        Dim request As New RegressionRequest(RegressionAnalysisKind.LinearModel, "y", New RegressionComponent(New Double(-1, 0) {}, {"y"}, Array.Empty(Of Integer)(), effects, {"y"}), New RegressionOptions With {.Residuals = True})
        Dim residual(0, 5) As Object
        For c = 0 To 5 : residual(0, c) = "header " & c : Next
        Dim report = LinearBinomialRegressionPolicy.Compose(LinearBinomialRegressionPolicy.Project(request), New List(Of ResultTable), Array.Empty(Of Double)(), residual)
        Assert.AreEqual(0, report.Rows.RowIds.Length) : Assert.AreEqual(1, report.Rows.Residuals.GetLength(0))
        Assert.IsFalse(report.Intent.Validation.IsValid)
        Assert.AreEqual(RegressionConvergence.NotIterative, report.Convergence)
    End Sub

    <DataTestMethod><DataRow(False)><DataRow(True)>
    Public Sub GaussianSuppliedStartsUseTheSelectedCoreLink(alternate As Boolean)
        Dim f = Regression09Fixture.Load("glm_gaussian_full.csv")
        Dim request = Regression10PolicyTests.Request(f, False, False, alternate, initial:={1.0, 0, 0})
        Dim intent = LinearBinomialRegressionPolicy.Project(request), report = Fit(intent)
        Assert.IsTrue(intent.Validation.IsValid)
        CollectionAssert.AreEqual({1.0, 0, 0}, report.Intent.Request.Primary.InitialValues)
        Assert.AreEqual("Gaussian", intent.CreateFamily().ToString())
        Assert.AreEqual(If(alternate, "Log", "Identity"), intent.CreateLink().GetType().Name)
        Assert.AreEqual(f.Ids.Length, report.Rows.Fitted.GetLength(0))
    End Sub

    <DataTestMethod><DataRow(1)><DataRow(3)>
    Public Sub OriginalTermKeysGroupsAndCodingNotesFlowIntoTheActualLm(sums As Integer)
        Dim cells(23, 2) As Object
        For i = 0 To 23
            cells(i, 0) = 2.0 + i * 0.1 + (i Mod 3) * 0.7 + (i Mod 2) * 0.05
            cells(i, 1) = CDbl(i) : cells(i, 2) = CDbl(i Mod 3)
        Next
        Dim raw As New glmData : raw.DataImportRawMatrix(cells, {"y", "x", "g"}, firstSourceRow:=31)
        Dim effects = RegressionEffectAuthoringPolicy.Apply({"x", "g"}, {0}, Nothing, RegressionEffectOperation.ContinuousMain).Model
        effects = RegressionEffectAuthoringPolicy.Apply({"x", "g"}, {0}, effects, RegressionEffectOperation.Polynomial, 2).Model
        effects = RegressionEffectAuthoringPolicy.Apply({"x", "g"}, {1}, effects, RegressionEffectOperation.CategoricalMain).Model
        Dim data(,) As Double = Nothing, names() As String = Nothing, groups As Dictionary(Of String, Integer()) = Nothing
        RegressionDesignCore.BuildExpandedLmDataMatrix(raw, "y", effects.Effects.Select(Function(e) e.Key).ToArray(), effects.ToTermSpecs(), True, data, names, groups)
        Dim notes = RegressionDesignCore.BuildCategoricalReferenceFootnotesForLm(raw, effects.Effects.Select(Function(e) e.Key).ToArray(), effects.ToTermSpecs(), True)
        Dim request As New RegressionRequest(RegressionAnalysisKind.LinearModel, "y", New RegressionComponent(data, names, raw.RowIds, effects, {"y", "x", "g"}), New RegressionOptions With {.Covariance = True, .Residuals = True})
        Dim intent = LinearBinomialRegressionPolicy.Project(request, CType(sums, TermSumOfSquaresType), groups, notes)
        Dim report = Fit(intent), direct As New LinearModel With {.bReturnCov = True, .bComputeResiduals = True}
        direct.Data(data, names, raw.RowIds) : direct.SetPredictorCodingFootnotes(notes) : direct.Fit(True, groups, CType(sums, TermSumOfSquaresType))
        Regression09PolicyTests.AssertTables(direct.wrapResults(), report.Document)
        Assert.AreEqual(RegressionEffectSerialization.Serialize(effects), RegressionEffectSerialization.Serialize(report.Intent.Request.Primary.Effects))
        CollectionAssert.AreEqual(notes, report.Intent.PredictorCodingFootnotes.ToList())
    End Sub

    <DataTestMethod><DataRow("")><DataRow("sk-SK")>
    Public Sub NistNorrisCoefficientsAndStandardErrorsReachTheProjectedDocument(culture As String)
        Dim saved = CultureInfo.CurrentCulture
        Try
            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo(culture)
            Dim f = Regression09Fixture.Load(Path.Combine("NIST_LLS", "nist_lls_norris_data.csv"))
            Dim report = Fit(LinearBinomialRegressionPolicy.Project(Request(f, True, False, False, vectors:=False), TermSumOfSquaresType.TypeI))
            Dim reference = File.ReadAllLines(Path.Combine(Regression09Fixture.Root(), "BESHStat.Core.Tests", "TestData", "NIST_LLS", "nist_lls_norris_reference.csv"))
            Dim coefficients = reference.Skip(1).Select(Function(line) line.Split(","c)).Where(Function(row) row(0) = "coefficient").ToArray()
            Dim table = report.Document.Tables(0).Model, values = table.Values
            For i = 0 To coefficients.Length - 1
                For j = 0 To 1
                    Dim expected = Double.Parse(coefficients(i)(2 + j), CultureInfo.InvariantCulture)
                    Assert.AreEqual(expected, CDbl(values(table.TitleRows + table.HeaderTopRows + i, table.HeaderLeftColumns + j)), Math.Max(0.0000000001, Math.Abs(expected) * 0.00000001))
                Next
            Next
            CollectionAssert.AreEqual(f.Ids, report.Rows.RowIds)
            Assert.AreEqual(0.05, report.ActualFitAlpha)
        Finally
            CultureInfo.CurrentCulture = saved
        End Try
    End Sub
End Class
