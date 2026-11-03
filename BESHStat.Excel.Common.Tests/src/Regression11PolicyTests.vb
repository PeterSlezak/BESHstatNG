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
Public Class Regression11PolicyTests
    Public Shared Function Request(kindName As String, alternate As Boolean) As RegressionRequest
        Dim kind = Regression09PolicyTests.KindFromName(kindName)
        Dim f = Regression09Fixture.Load(If(kindName = "NB2", "glm_nb2_full.csv", If(kindName = "ZIP", "zip_dataset_basic_count.csv", If(kindName = "Ordinal", "ordinal_logit_dataset_basic.csv", "mlogit_dataset_grouped_basic.csv"))))
        Dim o = Regression09PolicyTests.Options(kind)
        o.Alpha = 0.05 : o.MaximumIterations = 100 : o.Epsilon = 0.000001 : o.EmIterations = 100
        o.Covariance = If(kindName = "NB2" OrElse kindName = "ZIP", True, alternate)
        o.Residuals = Not ((kindName = "NB2" OrElse kindName = "ZIP") AndAlso alternate)
        o.IterationDetails = alternate : o.Reference = If(alternate, ReferenceCategory.First, ReferenceCategory.Last)
        Dim intercept = Not (kindName = "Multinomial" AndAlso alternate)
        Dim effects = RegressionEffectModel.FromProjection(f.Names.Skip(1), New Dictionary(Of String, TermSpec), intercept)
        Dim primary As New RegressionComponent(f.Data, f.Names, f.Ids, effects, f.Names)
        Dim logistic As RegressionComponent = Nothing
        If kindName = "ZIP" Then
            Dim z = Regression09Fixture.Load("zip_dataset_basic_zero.csv")
            logistic = New RegressionComponent(z.Data, z.Names, z.Ids, RegressionEffectModel.FromProjection(z.Names.Skip(1), New Dictionary(Of String, TermSpec), Not alternate), z.Names)
        End If
        Return New RegressionRequest(kind, f.Names(0), primary, o, New RegressionVector("offset", f.Offset, f.Ids),
            If(kindName = "ZIP", Nothing, New RegressionVector("weight", f.Weights, f.Ids)), logistic)
    End Function

    Public Shared Function Fit(intent As ExtendedRegressionIntent, Optional ByRef dispersion As Nullable(Of Double) = Nothing) As Regression09Fixture
        Dim r = intent.Request, o = intent.Options, p = intent.Primary
        Dim f As New Regression09Fixture With {.Data = p.Data, .Names = p.Names, .Ids = p.RowIds}
        Dim offset = If(r.Offset Is Nothing, Nothing, r.Offset.Values), weights = If(r.Weights Is Nothing, Nothing, r.Weights.Values)
        Dim offsetKey = If(r.Offset Is Nothing, Nothing, r.Offset.Key), weightKey = If(r.Weights Is Nothing, Nothing, r.Weights.Key)
        Select Case r.Kind
            Case RegressionAnalysisKind.NegativeBinomial2
                Dim model As New GLM_NB(intent.CreateLink()) With {.bReturnCov = o.Covariance, .bComputeResiduals = o.Residuals, .bIterationDetails = o.IterationDetails}
                model.data(p.Data, p.RowIds, offset, weights) : model.setVarNames(p.Names) : model.settingInputs(o.Alpha, o.MaximumIterations, o.Epsilon)
                If p.InitialValues IsNot Nothing Then model.startParams = p.InitialValues
                model.Fit(intent.PrimaryIntercept, p.InitialValues IsNot Nothing)
                f.Tables = model.wrapResults(offsetKey, weightKey) : f.Fitted = Regression09Fixture.Column(model.PredictedResponses) : f.Converged = model.Converged : dispersion = model.NBalpha
                If o.Residuals Then f.Residual = model.AllResiduals
            Case RegressionAnalysisKind.ZeroInflatedPoisson
                Dim z = intent.Logistic, model As New ZeroInflatedPoisson With {.bReturnCov = o.Covariance, .bComputeResiduals = o.Residuals, .bIterationDetails = o.IterationDetails}
                model.dataInputs(p.Data, z.Data, p.Names, z.Names, p.RowIds, offset, offsetKey) : model.settingInputs(o.Alpha, o.MaximumIterations, o.EmIterations, o.Epsilon)
                If p.InitialValues IsNot Nothing Then model.startParamsPois = p.InitialValues
                If z.InitialValues IsNot Nothing Then model.startParamsLog = z.InitialValues
                model.Fit(intent.PrimaryIntercept, intent.LogisticIntercept, p.InitialValues IsNot Nothing, z.InitialValues IsNot Nothing)
                f.Tables = model.wrapResults(offsetKey, Nothing) : f.Fitted = Regression09Fixture.Column(model.Predicted) : f.Converged = model.Converged
                If o.Residuals Then f.Residual = model.AllResiduals
            Case RegressionAnalysisKind.Ordinal
                Dim model As New OrdinalLogitModel With {.bReturnCov = o.Covariance, .bComputeResiduals = o.Residuals, .bIterationDetails = o.IterationDetails}
                model.Data(p.Data, p.Names, p.RowIds, offset, weights) : model.SettingInputs(o.Alpha, o.MaximumIterations, o.Epsilon)
                If p.InitialValues IsNot Nothing Then model.startParams = p.InitialValues
                model.Fit(intent.Reference, p.InitialValues IsNot Nothing)
                f.Tables = model.wrapResults(offsetKey, weightKey)
                If o.Residuals Then f.Fitted = model.Residuals.Probabilities : f.Residual = model.wrapResiduals() : f.Categories = model.Residuals.Categories
            Case RegressionAnalysisKind.Multinomial
                Dim model As New MultinomialLogitModel With {.bReturnCov = o.Covariance, .bComputeResiduals = o.Residuals, .bIterationDetails = o.IterationDetails}
                model.data(p.Data, p.Names, p.RowIds, offset, weights) : model.settingInputs(o.Alpha, o.MaximumIterations, o.Epsilon)
                If p.InitialValues IsNot Nothing Then model.startParams = p.InitialValues
                model.Fit(intent.PrimaryIntercept, intent.Reference, p.InitialValues IsNot Nothing)
                f.Tables = model.wrapResults(offsetKey, weightKey)
                If o.Residuals Then f.Fitted = model.ResidualDiagnostics.Probabilities : f.Residual = model.wrapResiduals() : f.Categories = model.ResidualDiagnostics.Categories
        End Select
        Return f
    End Function
    Public Shared Function Compose(intent As ExtendedRegressionIntent, f As Regression09Fixture, Optional dispersion As Nullable(Of Double) = Nothing) As ExtendedRegressionReport
        Return ExtendedRegressionFamilyPolicy.Compose(intent, f.Tables, New RegressionRowOutput(f.Ids, f.Fitted, f.Residual, 1, f.Categories), f.Converged, dispersion)
    End Function

    <DataTestMethod>
    <DataRow("NB2", False)><DataRow("NB2", True)><DataRow("ZIP", False)><DataRow("ZIP", True)><DataRow("Ordinal", False)><DataRow("Ordinal", True)><DataRow("Multinomial", False)><DataRow("Multinomial", True)>
    Public Sub ActualFamilyIntentFitAndDocumentMatchFrozenCore(kind As String, alternate As Boolean)
        Dim saved = CultureInfo.CurrentCulture
        Try
            CultureInfo.CurrentCulture = CultureInfo.InvariantCulture
            Dim intent = ExtendedRegressionFamilyPolicy.Project(Request(kind, alternate)), dispersion As Nullable(Of Double) = Nothing
            Assert.IsTrue(intent.Validation.IsValid, String.Join(";", intent.Validation.Diagnostics.Select(Function(d) d.Message)))
            Dim f = Fit(intent, dispersion), report = Compose(intent, f, dispersion)
            Dim name = kind & If(alternate, "-alternate", "-ordinary") & If(Environment.Version.Major < 5, "-net48", "") & ".txt"
            Regression10Fixture.AssertCapture(File.ReadAllLines(Path.Combine(Regression11Fixture.Root(), name)), Regression09Fixture.Canonical(f), name)
            f.Tables = report.LegacyTables()
            Regression10Fixture.AssertCapture(File.ReadAllLines(Path.Combine(Regression11Fixture.Root(), name)), Regression09Fixture.Canonical(f), name & " document roundtrip")
            CollectionAssert.AreEqual(intent.Primary.RowIds, report.Rows.RowIds)
            If kind = "NB2" Then
                Assert.AreEqual(RegressionDispersionMode.EstimatedNegativeBinomial2, intent.DispersionMode)
                Assert.IsTrue(report.EstimatedDispersion.Value > 0)
            End If
            If f.Categories IsNot Nothing Then
                CollectionAssert.AreEqual(f.Categories, report.Rows.Categories)
                For i = 0 To f.Ids.Length - 1
                    Assert.AreEqual(1.0, Enumerable.Range(0, f.Categories.Length).Sum(Function(j) f.Fitted(i, j)), 0.000000001)
                Next
            End If
            Assert.AreNotEqual(RegressionConvergence.Unknown, report.Convergence)
        Finally
            CultureInfo.CurrentCulture = saved
        End Try
    End Sub

    <DataTestMethod><DataRow("")><DataRow("sk-SK")>
    Public Sub CultureDoesNotReparseOrReorderNumbers(culture As String)
        Dim saved = CultureInfo.CurrentCulture
        Try
            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo(culture)
            For Each kind In {"NB2", "ZIP", "Ordinal", "Multinomial"}
                Dim intent = ExtendedRegressionFamilyPolicy.Project(Request(kind, False)), f = Fit(intent), report = Compose(intent, f)
                Regression09PolicyTests.AssertTables(f.Tables, report.Document)
                Dim baseline = File.ReadAllLines(Path.Combine(Regression11Fixture.Root(), kind & "-ordinary" & If(Environment.Version.Major < 5, "-net48", "") & ".txt"))
                Regression10Fixture.AssertCapture(baseline.Where(Function(s) s.StartsWith("D:")).ToArray(), Regression09Fixture.Canonical(f).Where(Function(s) s.StartsWith("D:")).ToArray(), kind)
                Assert.AreEqual(intent.Request.ResponseKey, report.Report.ToContractSnapshot().ResponseKey)
            Next
        Finally
            CultureInfo.CurrentCulture = saved
        End Try
    End Sub

    <TestMethod>
    Public Sub ZipComponentsStartsAndOptionsAreIndependentAndCloned()
        Dim r = Request("ZIP", False), z = r.Logistic, data = z.Data
        Dim narrow(data.GetLength(0) - 1, 1) As Double
        For i = 0 To data.GetLength(0) - 1 : narrow(i, 0) = data(i, 0) : narrow(i, 1) = data(i, 1) : Next
        Dim p = r.Primary
        Dim count As New RegressionComponent(p.Data, p.Names, p.RowIds, p.Effects, p.SourceKeys, {0.1, 0.2, 0.3})
        Dim logit As New RegressionComponent(narrow, {"y", "xzero"}, z.RowIds, RegressionEffectModel.FromProjection({"xzero"}, New Dictionary(Of String, TermSpec), False), {"y", "xzero"}, {0.4})
        Dim intent = ExtendedRegressionFamilyPolicy.Project(New RegressionRequest(r.Kind, r.ResponseKey, count, r.Options, r.Offset, logistic:=logit))
        Assert.AreNotEqual(intent.PrimaryParameterCount, intent.LogisticParameterCount)
        Assert.IsTrue(intent.Validation.IsValid)
        logit.Data(0, 1) = 999 : logit.InitialValues(0) = 999 : intent.Options.EmIterations = 1
        Assert.AreEqual(0.4, intent.Logistic.InitialValues(0)) : Assert.AreEqual(100, intent.Options.EmIterations)
        Assert.AreNotEqual(intent.Primary.Data.GetLength(1), intent.Logistic.Data.GetLength(1))
        Dim wrong As New RegressionComponent(narrow, {"y", "xzero"}, z.RowIds, logit.Effects, logit.SourceKeys, Array.Empty(Of Double)())
        Assert.IsFalse(ExtendedRegressionFamilyPolicy.Project(New RegressionRequest(r.Kind, r.ResponseKey, count, r.Options, r.Offset, logistic:=wrong)).Validation.IsValid)
    End Sub

    <DataTestMethod><DataRow("NB2")><DataRow("ZIP")><DataRow("Ordinal")><DataRow("Multinomial")>
    Public Sub DiagnosticsNeverCreateAnIntentRefusal(kind As String)
        Dim r = Request(kind, False), o = r.Options, p = r.Primary
        o.MaximumIterations = Integer.MaxValue : o.Epsilon = Double.PositiveInfinity
        Dim rows = p.RowIds : rows(1) = rows(0)
        Dim component As New RegressionComponent(p.Data, p.Names, rows, p.Effects, p.SourceKeys, Array.Empty(Of Double)())
        Dim intent = ExtendedRegressionFamilyPolicy.Project(New RegressionRequest(r.Kind, r.ResponseKey, component, o, r.Offset, r.Weights, r.Logistic))
        Assert.IsFalse(intent.Validation.IsValid)
        Assert.IsTrue(intent.Validation.Diagnostics.Any(Function(d) d.FieldPath = "primary.rowIds"))
        Assert.IsTrue(intent.Validation.Diagnostics.Any(Function(d) d.FieldPath = "primary.initialValues.count"))
        Assert.AreEqual(Integer.MaxValue, intent.Options.MaximumIterations, "Overflow-prone option retained as metadata; do not allocate a Core iteration table in this negative test.")
    End Sub

    <TestMethod>
    Public Sub EstimatedNbLinkAndFixedDispersionRemainDistinct()
        Dim r = Request("NB2", False), o = r.Options : o.Link = RegressionLink.Power : o.Power = 0.5
        Dim intent = ExtendedRegressionFamilyPolicy.Project(New RegressionRequest(r.Kind, r.ResponseKey, r.Primary, o, r.Offset, r.Weights))
        Assert.IsTrue(intent.Validation.IsValid) : Assert.AreEqual(0.5, DirectCast(intent.CreateLink(), Power).pwr)
        o.Family = RegressionFamily.NegativeBinomial : o.Dispersion = 0.8
        Dim fixed As New RegressionRequest(RegressionAnalysisKind.GeneralizedLinearModel, r.ResponseKey, r.Primary, o, r.Offset, r.Weights)
        Assert.IsTrue(RegressionRequestPolicy.Validate(fixed).IsValid)
        Assert.ThrowsException(Of ArgumentException)(Sub() ExtendedRegressionFamilyPolicy.Project(fixed))
    End Sub

    <DataTestMethod><DataRow(0)><DataRow(1)><DataRow(2)><DataRow(3)>
    Public Sub ProbabilityAndResidualAxesRejectMisalignment(index As Integer)
        Dim intent = ExtendedRegressionFamilyPolicy.Project(Request("Ordinal", False)), f = Fit(intent)
        Select Case index
            Case 0 : f.Fitted = New Double(0, 2) {}
            Case 1 : f.Categories = {99, 100, 101}
            Case 2 : f.Residual = New Object(f.Ids.Length, 1) {}
            Case 3 : f.Residual(0, 0) = 2.0
        End Select
        Assert.ThrowsException(Of ArgumentException)(Sub() Compose(intent, f))
    End Sub

    <TestMethod>
    Public Sub UnknownConvergenceAndUnavailableSectionsDoNotBecomeFitSuccess()
        Dim intent = ExtendedRegressionFamilyPolicy.Project(Request("Ordinal", False))
        Dim report = ExtendedRegressionFamilyPolicy.Compose(intent, New List(Of ResultTable), New RegressionRowOutput(intent.Primary.RowIds, Nothing, Nothing))
        Assert.AreEqual(RegressionConvergence.Unknown, report.Convergence)
        Assert.IsTrue(report.Document.Warnings.Count > 0)
        Assert.AreEqual(0, report.LegacyTables().Count)
    End Sub

    <DataTestMethod><DataRow("NB2")><DataRow("ZIP")><DataRow("Ordinal")><DataRow("Multinomial")>
    Public Sub OptionalSectionsAndSourceClonesFollowTheActualFamily(kind As String)
        Dim r = Request(kind, False)
        For Each enabled In {False, True}
            Dim o = r.Options : o.Covariance = enabled : o.Residuals = enabled : o.IterationDetails = enabled
            Dim intent = ExtendedRegressionFamilyPolicy.Project(New RegressionRequest(r.Kind, r.ResponseKey, r.Primary, o, r.Offset, r.Weights, r.Logistic))
            Dim f = Fit(intent), plan = Compose(intent, f)
            Assert.AreEqual(enabled, plan.Rows.Residuals IsNot Nothing)
            Regression09PolicyTests.AssertTables(f.Tables, plan.Document)
            Dim saved = plan.Document.Tables(0).Model.Values.Cast(Of Object)().ToArray()
            f.Tables(0).SetBody(New Object(,) {{"caller mutation"}})
            CollectionAssert.AreEqual(saved, plan.Document.Tables(0).Model.Values.Cast(Of Object)().ToArray())
            plan.LegacyTables()(0).SetBody(New Object(,) {{"bridge mutation"}})
            CollectionAssert.AreEqual(saved, plan.Document.Tables(0).Model.Values.Cast(Of Object)().ToArray())
        Next
    End Sub

    <DataTestMethod><DataRow("Ordinal", False)><DataRow("Ordinal", True)><DataRow("Multinomial", False)><DataRow("Multinomial", True)>
    Public Sub ArbitrarySparseClassLabelsAndReversedRowsKeepCoreAxes(kind As String, first As Boolean)
        Dim r = Request(kind, False), data = r.Primary.Data, rows = r.Primary.RowIds : Array.Reverse(rows)
        For i = 0 To data.GetLength(0) - 1
            data(i, 0) = If(data(i, 0) = 1, 10, If(data(i, 0) = 2, 100, -7))
        Next
        Dim p As New RegressionComponent(data, r.Primary.Names, rows, r.Primary.Effects, r.Primary.SourceKeys)
        Dim o = r.Options : o.Reference = If(first, ReferenceCategory.First, ReferenceCategory.Last)
        Dim offset = New RegressionVector(r.Offset.Key, r.Offset.Values, rows), weights = New RegressionVector(r.Weights.Key, r.Weights.Values, rows)
        Dim intent = ExtendedRegressionFamilyPolicy.Project(New RegressionRequest(r.Kind, r.ResponseKey, p, o, offset, weights))
        Dim f = Fit(intent), plan = Compose(intent, f)
        CollectionAssert.AreEqual(rows, plan.Rows.RowIds)
        Dim expected = If(kind = "Ordinal" AndAlso first, {100, 10, -7}, {-7, 10, 100})
        CollectionAssert.AreEqual(expected, plan.Rows.Categories)
        For i = 0 To rows.Length - 1
            Assert.AreEqual(1.0, Enumerable.Range(0, expected.Length).Sum(Function(j) plan.Rows.Fitted(i, j)), 0.000000001)
        Next
        Dim snapshot = plan.Report.ToContractSnapshot()
        CollectionAssert.AreEqual(expected, snapshot.Categories)
        Assert.AreEqual(o.Reference.ToString(), snapshot.Reference)
    End Sub

    <TestMethod>
    Public Sub NegativeInputsAndUnsupportedRolesStayDiagnostics()
        For Each kind In {"NB2", "ZIP", "Ordinal", "Multinomial"}
            Dim r = Request(kind, False), p = r.Primary, data = p.Data
            data(0, 0) = Double.NaN
            Dim bad As New RegressionComponent(data, {"y", "duplicate", "duplicate"}, p.RowIds, p.Effects, {"y", "x", "x"})
            Dim intent = ExtendedRegressionFamilyPolicy.Project(New RegressionRequest(r.Kind, r.ResponseKey, bad, r.Options, New RegressionVector("offset", {Double.PositiveInfinity}, {1}), r.Weights, r.Logistic))
            Assert.IsFalse(intent.Validation.IsValid)
            Assert.IsTrue(intent.Validation.Diagnostics.Any(Function(d) d.FieldPath = "offset.alignment"))
        Next
        Dim ordinal = Request("Ordinal", False), dataOnlyOne = ordinal.Primary.Data
        For i = 0 To dataOnlyOne.GetLength(0) - 1 : dataOnlyOne(i, 0) = 17 : Next
        Dim oneClass As New RegressionComponent(dataOnlyOne, ordinal.Primary.Names, ordinal.Primary.RowIds, ordinal.Primary.Effects, ordinal.Primary.SourceKeys)
        Assert.IsFalse(ExtendedRegressionFamilyPolicy.Project(New RegressionRequest(ordinal.Kind, "y", oneClass, ordinal.Options)).Validation.IsValid)
        Dim zip = Request("ZIP", False)
        Dim notSupportedWeights As New RegressionVector("weight", Enumerable.Repeat(1.0, zip.Primary.RowIds.Length).ToArray(), zip.Primary.RowIds)
        Assert.IsFalse(ExtendedRegressionFamilyPolicy.Project(New RegressionRequest(zip.Kind, "y", zip.Primary, zip.Options, zip.Offset, notSupportedWeights, zip.Logistic)).Validation.IsValid)
    End Sub

    <TestMethod>
    Public Sub ExactInputOptionsAndComponentProjectionSnapshot()
        Dim lines As New List(Of String) From {"04A11-input-projection-NOT_GOLDEN"}
        For Each kind In {"NB2", "ZIP", "Ordinal", "Multinomial"}
            For Each alternate In {False, True}
                Dim r = Request(kind, alternate), intent = ExtendedRegressionFamilyPolicy.Project(r), o = intent.Options
                lines.Add(kind & ":alternate=" & alternate.ToString())
                lines.Add("alpha=" & o.Alpha.ToString("R", CultureInfo.InvariantCulture) & ";epsilon=" & o.Epsilon.ToString("R", CultureInfo.InvariantCulture))
                lines.Add($"maximumIterations={o.MaximumIterations};emIterations={o.EmIterations};covariance={o.Covariance};residuals={o.Residuals};iterationDetails={o.IterationDetails};reference={o.Reference}")
                lines.Add($"primaryParameterCount={intent.PrimaryParameterCount};logisticParameterCount={intent.LogisticParameterCount};dispersion={intent.DispersionMode};link={o.Link}")
                For Each component In {intent.Primary, intent.Logistic}
                    If component Is Nothing Then
                        lines.Add("COMPONENT NULL") : Continue For
                    End If
                    lines.Add("NAMES " & String.Join("|", component.Names))
                    lines.Add("SOURCE_KEYS " & String.Join("|", component.SourceKeys))
                    lines.Add("ROWIDS " & String.Join(",", component.RowIds.Select(Function(v) v.ToString(CultureInfo.InvariantCulture))))
                    lines.Add("EFFECTS " & RegressionEffectSerialization.Serialize(component.Effects))
                    lines.Add("COORDINATES " & If(component.ResponseSources Is Nothing, "NOT_SUPPLIED_IN_SYNTHETIC_FIXTURE", "SUPPLIED"))
                    lines.Add("STARTS " & If(component.InitialValues Is Nothing, "NULL", String.Join(",", component.InitialValues.Select(Function(v) v.ToString("R", CultureInfo.InvariantCulture)))))
                    Dim data = component.Data
                    lines.Add($"DATA {data.GetLength(0)} {data.GetLength(1)}")
                    For Each value As Double In data : lines.Add("D:" & value.ToString("R", CultureInfo.InvariantCulture)) : Next
                Next
                For Each vector In {r.Offset, r.Weights}
                    If vector Is Nothing Then
                        lines.Add("VECTOR NULL") : Continue For
                    End If
                    lines.Add("VECTOR " & vector.Key)
                    lines.Add("ROWIDS " & String.Join(",", vector.RowIds.Select(Function(v) v.ToString(CultureInfo.InvariantCulture))))
                    For Each value In vector.Values : lines.Add("D:" & value.ToString("R", CultureInfo.InvariantCulture)) : Next
                Next
                lines.Add("DIAGNOSTICS " & String.Join("|", intent.Validation.Diagnostics.Select(Function(d) d.Code)))
                CollectionAssert.AreEqual(r.Primary.Data.Cast(Of Double)().ToArray(), intent.Primary.Data.Cast(Of Double)().ToArray())
                If r.Logistic IsNot Nothing Then CollectionAssert.AreEqual(r.Logistic.Data.Cast(Of Double)().ToArray(), intent.Logistic.Data.Cast(Of Double)().ToArray())
            Next
        Next
        Dim path = IO.Path.Combine(Regression09Fixture.Root(), "evidence", "04A11", "input-projection" & If(Environment.Version.Major < 5, "-net48", "") & ".txt")
        If Environment.GetEnvironmentVariable("BESHSTAT_04A11_INPUT_CAPTURE") = "CANDIDATE" Then
            Using stream As New FileStream(path, FileMode.CreateNew)
                Using writer As New StreamWriter(stream, New Text.UTF8Encoding(False)) With {.NewLine = ChrW(10)}
                    For Each line In lines : writer.WriteLine(line) : Next
                End Using
            End Using
        End If
        CollectionAssert.AreEqual(File.ReadAllLines(path), lines.ToArray())
    End Sub
End Class
