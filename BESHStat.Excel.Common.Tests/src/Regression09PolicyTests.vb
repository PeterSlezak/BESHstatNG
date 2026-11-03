Option Explicit On
Option Strict On
Imports System
Imports System.Collections.Generic
Imports System.Globalization
Imports System.IO
Imports System.Linq
Imports System.Runtime.Serialization.Json
Imports System.Text
Imports BESHStatNG
Imports BESHStatNG.ExcelCommon
Imports BESHStatNG.regression
Imports Microsoft.VisualStudio.TestTools.UnitTesting

<TestClass>
Public Class Regression09PolicyTests
    Public Shared Function Effects(Optional intercept As Boolean = True, Optional key As String = "x") As RegressionEffectModel
        Return RegressionEffectModel.FromProjection({key}, New Dictionary(Of String, TermSpec)(), intercept)
    End Function
    Public Shared Function Options(kind As RegressionAnalysisKind) As RegressionOptions
        Dim o As New RegressionOptions
        If kind = RegressionAnalysisKind.GeneralizedLinearModel Then o.Family = RegressionFamily.Poisson
        If kind = RegressionAnalysisKind.GeneralizedLinearModel OrElse kind = RegressionAnalysisKind.NegativeBinomial2 Then o.Link = RegressionLink.Log
        Return o
    End Function
    Public Shared Function Component(Optional intercept As Boolean = True, Optional initial() As Double = Nothing,
                                      Optional rows() As Integer = Nothing) As RegressionComponent
        Return New RegressionComponent(New Double(,) {{2, 0}, {10, 1}, {2, 2}, {3, 3}}, {"y", "x"}, If(rows, {11, 19, 23, 41}), Effects(intercept), {"y", "x"}, initial)
    End Function
    Public Shared Function Request(kind As RegressionAnalysisKind, Optional o As RegressionOptions = Nothing,
                                   Optional primary As RegressionComponent = Nothing, Optional logistic As RegressionComponent = Nothing,
                                   Optional weights As RegressionVector = Nothing, Optional offset As RegressionVector = Nothing) As RegressionRequest
        Return New RegressionRequest(kind, "y", If(primary, Component()), If(o, Options(kind)), offset, weights,
                                     If(kind = RegressionAnalysisKind.ZeroInflatedPoisson, If(logistic, Component(False)), logistic))
    End Function
    Private Shared Sub HasIssue(r As RegressionValidation, field As String)
        Assert.IsFalse(r.IsValid)
        Assert.IsTrue(r.Diagnostics.Any(Function(d) d.FieldPath = field AndAlso d.Code = "regression." & field), String.Join(";", r.Diagnostics.Select(Function(d) d.Code)))
    End Sub

    <DataTestMethod>
    <DataRow(0, 2, 0)><DataRow(1, 2, 0)><DataRow(2, 2, 0)><DataRow(3, 2, 1)><DataRow(4, 3, 0)><DataRow(5, 4, 0)>
    Public Sub FamilyMatrixAfterExpansion(kindValue As Integer, primaryCount As Integer, logisticCount As Integer)
        Dim kind = CType(kindValue, RegressionAnalysisKind), r = RegressionRequestPolicy.Validate(Request(kind))
        Assert.IsTrue(r.IsValid, String.Join(";", r.Diagnostics.Select(Function(d) d.Message)))
        Assert.AreEqual(primaryCount, r.PrimaryParameterCount) : Assert.AreEqual(logisticCount, r.LogisticParameterCount)
        If kind = RegressionAnalysisKind.Ordinal OrElse kind = RegressionAnalysisKind.Multinomial Then CollectionAssert.AreEqual({2, 10, 3}, r.ObservedClasses.ToArray())
    End Sub

    <DataTestMethod><DataRow(0)><DataRow(1)><DataRow(2)><DataRow(3)><DataRow(4)><DataRow(5)>
    Public Sub SuppliedEmptyAndWrongStartingValuesAreDistinct(kindValue As Integer)
        Dim kind = CType(kindValue, RegressionAnalysisKind)
        Assert.IsTrue(RegressionRequestPolicy.Validate(Request(kind)).IsValid)
        HasIssue(RegressionRequestPolicy.Validate(Request(kind, primary:=Component(initial:=Array.Empty(Of Double)()))), "primary.initialValues.count")
        Dim n = RegressionRequestPolicy.Validate(Request(kind)).PrimaryParameterCount
        Assert.IsTrue(RegressionRequestPolicy.Validate(Request(kind, primary:=Component(initial:=New Double(n - 1) {}))).IsValid)
        Dim bad(n - 1) As Double : bad(0) = Double.NaN
        HasIssue(RegressionRequestPolicy.Validate(Request(kind, primary:=Component(initial:=bad))), "primary.initialValues")
    End Sub

    <DataTestMethod><DataRow(0)><DataRow(1)><DataRow(2)><DataRow(3)><DataRow(4)><DataRow(5)><DataRow(6)><DataRow(7)><DataRow(8)><DataRow(9)><DataRow(10)><DataRow(11)>
    Public Sub InvalidOptionsHaveStableFieldCodes(index As Integer)
        Dim kind = RegressionAnalysisKind.GeneralizedLinearModel, o = Options(kind), field As String
        Select Case index
            Case 0 : o.Alpha = 0 : field = "options.alpha"
            Case 1 : o.Alpha = 1 : field = "options.alpha"
            Case 2 : o.Alpha = Double.NaN : field = "options.alpha"
            Case 3 : o.Epsilon = Double.PositiveInfinity : field = "options.epsilon"
            Case 4 : o.Epsilon = 0 : field = "options.epsilon"
            Case 5 : o.MaximumIterations = 0 : field = "options.maximumIterations"
            Case 6 : o.Link = RegressionLink.Logit : field = "options.link"
            Case 7 : o.Family = Nothing : field = "options.family"
            Case 8 : o.Dispersion = -1 : field = "options.dispersion"
            Case 9 : o.Classification = True : field = "options.classification"
            Case 10 : o.Reference = CType(99, ReferenceCategory) : field = "options.reference"
            Case Else : o.Power = 2 : field = "options.power"
        End Select
        HasIssue(RegressionRequestPolicy.Validate(Request(kind, o)), field)
    End Sub

    <DataTestMethod><DataRow(0)><DataRow(1)><DataRow(2)><DataRow(3)><DataRow(4)><DataRow(5)>
    Public Sub RowAndVectorAlignmentRejectsAmbiguity(index As Integer)
        Dim kind = RegressionAnalysisKind.GeneralizedLinearModel
        Dim rows = {11, 19, 23, 41}, values = {1.0, 2, 0, 1}, field = "weights.alignment"
        Select Case index
            Case 0 : rows = {1, 2, 3, 4}
            Case 1 : rows = {11, 23, 19, 41}
            Case 2 : values = {1.0}
            Case 3 : rows = {11, 19, 23, 23}
            Case 4 : values(2) = -1 : field = "weights"
            Case 5 : values(2) = Double.NaN : field = "weights"
        End Select
        Dim r = RegressionRequestPolicy.Validate(Request(kind, weights:=New RegressionVector("w", values, rows)))
        Assert.IsFalse(r.IsValid) : Assert.IsTrue(r.Diagnostics.Any(Function(d) d.FieldPath = field))
        If index >= 4 Then Assert.AreEqual(CType(23, Integer?), r.Diagnostics.Single(Function(d) d.FieldPath = field).SourceRowId)
    End Sub

    <TestMethod>
    Public Sub MissingKeysRolesAndNoDataAreExplicit()
        Dim kind = RegressionAnalysisKind.LinearModel
        HasIssue(RegressionRequestPolicy.Validate(New RegressionRequest(kind, "", Component())), "response")
        HasIssue(RegressionRequestPolicy.Validate(New RegressionRequest(kind, "y", Nothing)), "primary")
        Dim empty As New RegressionComponent(New Double(,) {{1}, {2}}, {"y"}, {5, 9}, New RegressionEffectModel(includeIntercept:=False), {"y"})
        HasIssue(RegressionRequestPolicy.Validate(Request(kind, primary:=empty)), "primary.effects")
        Dim renamed As New RegressionComponent(New Double(,) {{1, 2}, {2, 3}}, {"y", "x"}, {5, 9}, Effects(), {"y", "renamed"})
        HasIssue(RegressionRequestPolicy.Validate(Request(kind, primary:=renamed)), "primary.effects.sourceKeys")
        Dim duplicate As New RegressionComponent(New Double(,) {{1, 2}, {2, 3}}, {"y", "x"}, {5, 9}, Effects(), {"y", "x", "x"})
        HasIssue(RegressionRequestPolicy.Validate(Request(kind, primary:=duplicate)), "primary.sourceKeys")
        Dim nonunique = Component(rows:={11, 19, 19, 41})
        HasIssue(RegressionRequestPolicy.Validate(Request(kind, primary:=nonunique)), "primary.rowIds")
        HasIssue(RegressionRequestPolicy.Validate(Request(kind, offset:=New RegressionVector("offset", {0.0, 0, 0, 0}, {11, 19, 23, 41}))), "offset")
    End Sub

    <TestMethod>
    Public Sub InterceptOnlyZeroWeightsAndExistingCategoryRoundingAreRetained()
        Dim component As New RegressionComponent(New Double(,) {{1.2}, {2.4}, {10.0}}, {"y"}, {4, 8, 99}, New RegressionEffectModel(), {"y"})
        Dim r = RegressionRequestPolicy.Validate(Request(RegressionAnalysisKind.Ordinal, primary:=component))
        Assert.IsTrue(r.IsValid) : Assert.AreEqual(2, r.PrimaryParameterCount)
        CollectionAssert.AreEqual({1, 2, 10}, r.ObservedClasses.ToArray())
        Dim weights As New RegressionVector("w", {0.0, 1, 1, 1}, {11, 19, 23, 41})
        Assert.IsTrue(RegressionRequestPolicy.Validate(Request(RegressionAnalysisKind.GeneralizedLinearModel, weights:=weights)).IsValid)
        Assert.IsFalse(RegressionRequestPolicy.Validate(Request(RegressionAnalysisKind.LinearModel, weights:=weights)).IsValid)
        Dim oneClass As New RegressionComponent(New Double(,) {{1}, {1}}, {"y"}, {1, 2}, New RegressionEffectModel(), {"y"})
        HasIssue(RegressionRequestPolicy.Validate(Request(RegressionAnalysisKind.Multinomial, primary:=oneClass)), "primary.response.categories")
    End Sub

    <DataTestMethod><DataRow(0)><DataRow(1)><DataRow(2)>
    Public Sub ZipComponentsRemainIndependentThroughFailure(index As Integer)
        Dim primary = Component(initial:={0.0, 0.0}), logistic = Component(False, initial:={0.0})
        If index = 0 Then logistic = Component(False, initial:={0.0, 0.0})
        If index = 1 Then logistic = Component(False, initial:={0.0}, rows:={11, 23, 19, 41})
        Dim r = RegressionRequestPolicy.Validate(Request(RegressionAnalysisKind.ZeroInflatedPoisson, primary:=primary, logistic:=logistic))
        Assert.AreEqual(index = 2, r.IsValid)
        Assert.AreEqual(2, r.PrimaryParameterCount) : Assert.AreEqual(1, r.LogisticParameterCount)
        CollectionAssert.AreEqual({0.0, 0.0}, primary.InitialValues)
        Assert.IsTrue(RegressionRequestPolicy.Validate(Request(RegressionAnalysisKind.ZeroInflatedPoisson, primary:=primary, logistic:=Component(False, initial:={0.0}))).IsValid)
    End Sub

    <DataTestMethod><DataRow("")><DataRow("sk-SK")>
    Public Sub TypedBoundaryIsCultureIndependentAndDefensivelyCopied(culture As String)
        Dim previous = CultureInfo.CurrentCulture
        Try
            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo(culture)
            Dim data = New Double(,) {{1.5, 2}, {2.5, 3}}, rows = {17, 94}, names = {"y", "x"}, initial = {1.0, 2.0}
            Dim effect = Effects(), c As New RegressionComponent(data, names, rows, effect, {"y", "x"}, initial)
            Dim o = Options(RegressionAnalysisKind.GeneralizedLinearModel)
            Dim v As New RegressionVector("w", {1.0, 2.0}, rows)
            Dim req As New RegressionRequest(RegressionAnalysisKind.GeneralizedLinearModel, "y", c, o, weights:=v)
            data(0, 0) = 999 : rows(0) = 0 : names(0) = "changed" : initial(0) = 99 : o.Alpha = -1
            Dim copy = c.Data : copy(0, 0) = 123
            req.Options.Alpha = 0
            Assert.AreEqual(1.5, c.Data(0, 0)) : Assert.AreEqual(17, c.RowIds(0)) : Assert.AreEqual("y", c.Names(0)) : Assert.AreEqual(1.0, c.InitialValues(0))
            Assert.AreEqual(0.05, req.Options.Alpha) : Assert.IsTrue(RegressionRequestPolicy.Validate(req).IsValid)
        Finally
            CultureInfo.CurrentCulture = previous
        End Try
    End Sub

    <TestMethod>
    Public Sub LargeBoundedExpandedInputAndDefaultMetadata()
        Dim data(4095, 1) As Double, rows = Enumerable.Range(100, 4096).ToArray()
        Dim c As New RegressionComponent(data, {"y", "x"}, rows, Effects(), {"y", "x"})
        Dim req As New RegressionRequest(RegressionAnalysisKind.LinearModel, "y", c)
        Dim validation = RegressionRequestPolicy.Validate(req)
        Assert.IsTrue(validation.IsValid) : Assert.IsTrue(validation.Diagnostics.Any(Function(d) d.Code = "regression.options.defaulted" AndAlso Not d.Blocking))
        Dim nonfinite = c.Data : nonfinite(3, 1) = Double.PositiveInfinity
        Dim r = RegressionRequestPolicy.Validate(New RegressionRequest(RegressionAnalysisKind.LinearModel, "y", New RegressionComponent(nonfinite, c.Names, rows, Effects(), c.SourceKeys)))
        Assert.AreEqual(CType(103, Integer?), r.Diagnostics.Single(Function(d) d.Code = "regression.numeric.nonfinite").SourceRowId)
        Assert.IsFalse(req.Options.WasSupplied("alpha"))
        Assert.IsTrue(validation.Diagnostics.Any(Function(d) d.Code = "regression.options.alpha.defaulted"))
        Dim explicitOptions As New RegressionOptions With {.Alpha = Double.NaN}
        Dim explicitRequest As New RegressionRequest(RegressionAnalysisKind.LinearModel, "y", c, explicitOptions)
        Assert.IsTrue(explicitRequest.Options.WasSupplied("alpha"))
        Assert.IsFalse(RegressionRequestPolicy.Validate(explicitRequest).Diagnostics.Any(Function(d) d.Code = "regression.options.alpha.defaulted"))
    End Sub

    <DataTestMethod><DataRow(0.0)><DataRow(0.5)><DataRow(1.0)>
    Public Sub ClassificationSectionsMatchCoreAndZeroWeightMissingSemantics(threshold As Double)
        Dim o As New RegressionOptions With {.Family = RegressionFamily.Binomial, .Classification = True, .Threshold = threshold, .ThresholdTable = True, .Calibration = True, .CalibrationBins = 2, .Brier = True}
        Dim y = {0.0, 1, 0, 1}, p = {0.1, 0.9, 0.6, 0.4}, w = {1.0, 2, 1, 1}
        Dim r = RegressionClassificationPolicy.Compose(y, p, w, o, 0.6)
        Dim expected = BinaryClassificationCore.WrapResults(BinaryClassificationCore.ComputeBinarySummary(y, p, threshold, w), BinaryClassificationCore.BuildThresholdTable(y, p, Nothing, w), BinaryClassificationCore.BuildCalibrationBins(y, p, 2, w), BinaryClassificationCore.ComputeBrierScore(y, p, w), 0.6, "GLM Binary Classification")
        AssertTables(expected, r.Document)
        Assert.AreEqual(2, r.CalibrationRows.Count)
        o.ThresholdTable = False : o.Calibration = False : o.Brier = False
        Assert.IsTrue(RegressionClassificationPolicy.Compose(y, p, w, o).Document.Tables.Count < r.Document.Tables.Count)
        o.Classification = False
        Assert.AreEqual(0, RegressionClassificationPolicy.Compose(Nothing, Nothing, Nothing, o).Document.Tables.Count)
        o.Classification = True
        Dim zero = RegressionClassificationPolicy.Compose(y, p, {0.0, 0, 0, 0}, o)
        AssertTables(BinaryClassificationCore.WrapResults(BinaryClassificationCore.ComputeBinarySummary(y, p, threshold, {0.0, 0, 0, 0}), analysisLabel:="GLM Binary Classification"), zero.Document)
    End Sub

    <DataTestMethod><DataRow(-0.01)><DataRow(1.01)><DataRow(Double.NaN)><DataRow(Double.PositiveInfinity)>
    Public Sub ClassificationRejectsInvalidThreshold(value As Double)
        Dim o As New RegressionOptions With {.Family = RegressionFamily.Binomial, .Classification = True, .Threshold = value}
        StringAssert.Contains(Assert.ThrowsException(Of ArgumentException)(Sub() RegressionClassificationPolicy.Compose({0.0, 1}, {0.2, 0.8}, Nothing, o)).Message, "regression.options.threshold")
    End Sub

    <TestMethod>
    Public Sub ClassificationNoDataAndIncompatibleFamilyDoNotProduceSuccess()
        Dim o As New RegressionOptions With {.Family = RegressionFamily.Binomial, .Classification = True}
        Assert.ThrowsException(Of ArgumentException)(Sub() RegressionClassificationPolicy.Compose({}, {}, Nothing, o))
        o.Family = RegressionFamily.Poisson
        Assert.ThrowsException(Of ArgumentException)(Sub() RegressionClassificationPolicy.Compose({0.0, 1}, {0.2, 0.8}, Nothing, o))
    End Sub

    <DataTestMethod><DataRow("LM", False)><DataRow("LM", True)><DataRow("GLM", False)><DataRow("GLM", True)><DataRow("NB2", False)><DataRow("NB2", True)><DataRow("ZIP", False)><DataRow("ZIP", True)><DataRow("Ordinal", False)><DataRow("Ordinal", True)><DataRow("Multinomial", False)><DataRow("Multinomial", True)>
    Public Sub ReportPlanPreservesActualCoreSectionsAndRowPayload(kind As String, alternate As Boolean)
        Dim f = Regression09Fixture.Fit(kind, alternate)
        Dim family = KindFromName(kind)
        Dim req As New RegressionRequest(family, "y", New RegressionComponent(f.Data, f.Names, f.Ids, Effects(), {"y", "x"}), Options(family))
        Dim plan = RegressionReportPolicy.Compose(req, f.Tables, New RegressionRowOutput(f.Ids, f.Fitted, f.Residual, categories:=f.Categories), f.Converged)
        AssertTables(f.Tables, plan.Document)
        CollectionAssert.AreEqual(f.Ids, plan.Rows.RowIds)
        CollectionAssert.AreEqual(f.Fitted.Cast(Of Double)().ToArray(), plan.Rows.Fitted.Cast(Of Double)().ToArray())
        If f.Residual IsNot Nothing Then CollectionAssert.AreEqual(f.Residual.Cast(Of Object)().ToArray(), plan.Rows.Residuals.Cast(Of Object)().ToArray())
        Assert.AreEqual(Serialize(plan.Document.ToContractSnapshot()), Serialize(plan.Document.ToContractSnapshot()))
        If family = RegressionAnalysisKind.LinearModel Then
            Assert.AreEqual(RegressionConvergence.NotIterative, plan.Convergence)
        Else
            Assert.AreNotEqual(RegressionConvergence.Unknown, plan.Convergence)
        End If
    End Sub

    Public Shared Function KindFromName(kind As String) As RegressionAnalysisKind
        Select Case kind
            Case "LM" : Return RegressionAnalysisKind.LinearModel
            Case "GLM" : Return RegressionAnalysisKind.GeneralizedLinearModel
            Case "NB2" : Return RegressionAnalysisKind.NegativeBinomial2
            Case "ZIP" : Return RegressionAnalysisKind.ZeroInflatedPoisson
            Case "Ordinal" : Return RegressionAnalysisKind.Ordinal
            Case Else : Return RegressionAnalysisKind.Multinomial
        End Select
    End Function
    Public Shared Sub AssertTables(expected As List(Of ResultTable), actual As AnalysisResultDocument)
        Assert.AreEqual(expected.Count, actual.Tables.Count)
        For i = 0 To expected.Count - 1
            Dim e = expected(i).ToOutputModel(), a = actual.Tables(i).Model
            Assert.AreEqual(i, actual.Tables(i).Index)
            Assert.AreEqual(e.RowCount, a.RowCount) : Assert.AreEqual(e.ColumnCount, a.ColumnCount)
            Assert.AreEqual(e.TitleRows, a.TitleRows) : Assert.AreEqual(e.HeaderTopRows, a.HeaderTopRows) : Assert.AreEqual(e.FooterRows, a.FooterRows)
            CollectionAssert.AreEqual(e.Values.Cast(Of Object)().ToArray(), a.Values.Cast(Of Object)().ToArray())
            CollectionAssert.AreEqual(e.PvalueColumns, a.PvalueColumns)
        Next
    End Sub
    Private Shared Function Serialize(snapshot As AnalysisResultDocumentSnapshot) As String
        Using stream As New MemoryStream
            Dim serializer As New DataContractJsonSerializer(GetType(AnalysisResultDocumentSnapshot))
            serializer.WriteObject(stream, snapshot)
            Return Encoding.UTF8.GetString(stream.ToArray())
        End Using
    End Function

    <DataTestMethod><DataRow(0)><DataRow(1)><DataRow(2)><DataRow(3)><DataRow(4)><DataRow(5)>
    Public Sub ActualCoreCategoricalPolynomialExpansionDrivesParameterCounts(kindValue As Integer)
        Dim kind = CType(kindValue, RegressionAnalysisKind)
        Dim raw As New glmData
        raw.DataImportRawMatrix(New Object(,) {{2, 1, 10}, {10, 2, 2}, {3, 3, 3}, {2, 4, 10}, {10, 5, 2}, {3, 6, 3}}, {"y", "x", "g"}, firstSourceRow:=21)
        Dim model = RegressionEffectAuthoringPolicy.Apply({"x", "g"}, {0}, Nothing, RegressionEffectOperation.ContinuousMain).Model
        model = RegressionEffectAuthoringPolicy.Apply({"x", "g"}, {0}, model, RegressionEffectOperation.Polynomial, 2).Model
        model = RegressionEffectAuthoringPolicy.Apply({"x", "g"}, {1}, model, RegressionEffectOperation.CategoricalMain).Model
        Dim expanded(,) As Double = Nothing, names() As String = Nothing
        RegressionDesignCore.BuildExpandedRegressionDataMatrix(raw, "y", model.Effects.Select(Function(e) e.Key).ToArray(), model.ToTermSpecs(), True, expanded, names)
        Assert.AreEqual(5, names.Length, "y, x, x^2, two categorical contrasts")
        Dim component As New RegressionComponent(expanded, names, raw.RowIds, model, {"y", "x", "g"})
        Dim req As New RegressionRequest(kind, "y", component, Options(kind), logistic:=If(kind = RegressionAnalysisKind.ZeroInflatedPoisson, component, Nothing))
        Dim result = RegressionRequestPolicy.Validate(req)
        Assert.IsTrue(result.IsValid)
        Assert.AreEqual(If(kind = RegressionAnalysisKind.Ordinal, 6, If(kind = RegressionAnalysisKind.Multinomial, 10, 5)), result.PrimaryParameterCount)
        Dim text = RegressionEffectSerialization.Serialize(req.Primary.Effects), restored As RegressionEffectModel = Nothing, issue As RegressionEffectDiagnostic = Nothing
        Assert.IsTrue(RegressionEffectSerialization.TryDeserialize(text, restored, issue))
        Assert.AreEqual(text, RegressionEffectSerialization.Serialize(restored))
    End Sub

    <TestMethod>
    Public Sub NoInterceptExpansionKeepsAllCategoryColumnsAndLossyFormulaRefusal()
        Dim raw As New glmData
        raw.DataImportRawMatrix(New Object(,) {{1, 2}, {2, 10}, {1, 3}, {2, 2}}, {"y", "g"}, firstSourceRow:=8)
        Dim model = RegressionEffectAuthoringPolicy.Apply({"g"}, {0}, New RegressionEffectModel(includeIntercept:=False), RegressionEffectOperation.CategoricalMain).Model
        Dim expanded(,) As Double = Nothing, names() As String = Nothing
        RegressionDesignCore.BuildExpandedRegressionDataMatrix(raw, "y", model.Effects.Select(Function(e) e.Key).ToArray(), model.ToTermSpecs(), False, expanded, names)
        Assert.AreEqual(4, names.Length)
        Dim request As New RegressionRequest(RegressionAnalysisKind.Multinomial, "y", New RegressionComponent(expanded, names, raw.RowIds, model, {"y", "g"}))
        Assert.AreEqual(3, RegressionRequestPolicy.Validate(request).PrimaryParameterCount)
        Dim spec = model.Effects.Single().Spec
        spec.ReferenceValue = 2
        spec.ObservedLevels = New List(Of Double) From {2, 10, 3}
        Dim withReference As New RegressionEffectModel({New RegressionAuthoredEffect(model.Effects.Single().Key, spec)}, False)
        Dim projection = RegressionEffectSerialization.ProjectFormula(withReference, {"g"})
        Assert.IsFalse(projection.Representable)
        Assert.AreEqual("formula-not-representable", projection.Diagnostics.Single().Code)
        StringAssert.Contains(projection.Diagnostics.Single().Message, "observed levels")
    End Sub

    <TestMethod>
    Public Sub ReportSnapshotsAndRowsAreIndependentOfCallerMutation()
        Dim req = Request(RegressionAnalysisKind.GeneralizedLinearModel), ids = req.Primary.RowIds
        Dim residuals = New Object(,) {{"Residual"}, {Double.NaN}, {1.0}, {2.0}, {Nothing}}
        Dim fitted = New Double(,) {{1}, {2}, {3}, {4}}, t As New ResultTable
        t.SetBody(New Double(,) {{0.03}}) : t.AddPvalueToFormat(1)
        Dim rows As New RegressionRowOutput(ids, fitted, residuals)
        Dim plan = RegressionReportPolicy.Compose(req, {t}, rows, False)
        fitted(0, 0) = 99 : residuals(2, 0) = 99 : ids(0) = 99
        Dim changed = rows.Fitted : changed(0, 0) = 100
        Assert.AreEqual(1.0, rows.Fitted(0, 0)) : Assert.AreEqual(1.0, CDbl(rows.Residuals(2, 0)))
        Assert.IsTrue(Double.IsNaN(CDbl(rows.Residuals(1, 0)))) : Assert.IsNull(rows.Residuals(4, 0))
        Dim snapshot = plan.ToContractSnapshot()
        Assert.AreEqual("GeneralizedLinearModel", snapshot.Kind) : Assert.AreEqual("Poisson", snapshot.Family) : Assert.AreEqual("Log", snapshot.Link)
        Assert.AreEqual("NotConverged", snapshot.Convergence)
        Using a As New MemoryStream, b As New MemoryStream
            Dim serializer As New DataContractJsonSerializer(GetType(RegressionReportSnapshot))
            serializer.WriteObject(a, snapshot) : serializer.WriteObject(b, plan.ToContractSnapshot())
            CollectionAssert.AreEqual(a.ToArray(), b.ToArray())
        End Using
        snapshot.RowIds(0) = 99
        Assert.AreEqual(11, plan.ToContractSnapshot().RowIds(0))
        Assert.ThrowsException(Of ArgumentException)(Sub() GC.KeepAlive(New RegressionRowOutput({1}, Nothing, New Object(,) {{"header"}, {New Object}})))
    End Sub

    <TestMethod>
    Public Sub CoreGlmNonconvergenceAndWarningsAreRetainedInReport()
        Dim f = Regression09Fixture.Load("glm_poisson_full.csv"), model As New GLM(New Poisson(), New Log())
        model.data(f.Data, f.Ids, f.Offset, f.Weights) : model.setVarNames(DirectCast(f.Names.Clone(), String()))
        model.settingInputs(0.05, 1, 0.00000000000001) : model.Fit(1, False)
        Assert.IsFalse(model.Converged)
        Dim req As New RegressionRequest(RegressionAnalysisKind.GeneralizedLinearModel, "y", New RegressionComponent(f.Data, f.Names, f.Ids, Effects(), {"y", "x"}), Options(RegressionAnalysisKind.GeneralizedLinearModel))
        Dim tables = model.wrapResults("offset", "weight")
        Dim plan = RegressionReportPolicy.Compose(req, tables, New RegressionRowOutput(f.Ids, Regression09Fixture.Column(model.PredictedResponses), Nothing), model.Converged)
        Assert.AreEqual(RegressionConvergence.NotConverged, plan.Convergence)
        AssertTables(tables, plan.Document)
        Assert.IsTrue(plan.Document.Warnings.Any(Function(w) w.Code = "regression.fit.not-converged"))
    End Sub

    <DataTestMethod><DataRow(False)><DataRow(True)>
    Public Sub ObservedIdentityAndCoreCategoryAxisAreNeverSortedDisplayLabels(first As Boolean)
        Dim f = Regression09Fixture.Load("ordinal_logit_dataset_basic.csv")
        For i = 0 To f.Data.GetLength(0) - 1
            Dim y = f.Data(i, 0) : f.Data(i, 0) = If(y = 1, 2, If(y = 2, 10, 3))
        Next
        Dim model As New OrdinalLogitModel With {.bComputeResiduals = True}
        model.Data(f.Data, f.Names, f.Ids, f.Offset, f.Weights) : model.SettingInputs(0.05, 100, 0.000001)
        model.Fit(If(first, ReferenceCategory.First, ReferenceCategory.Last), False)
        Dim req As New RegressionRequest(RegressionAnalysisKind.Ordinal, "y", New RegressionComponent(f.Data, f.Names, f.Ids, Effects(), {"y", "x"}))
        CollectionAssert.AreEqual({2, 10, 3}, RegressionRequestPolicy.Validate(req).ObservedClasses.ToArray())
        Dim plan = RegressionReportPolicy.Compose(req, model.wrapResults(), New RegressionRowOutput(f.Ids, model.Residuals.Probabilities, model.wrapResiduals(), categories:=model.Residuals.Categories))
        CollectionAssert.AreEqual(If(first, {10, 3, 2}, {2, 3, 10}), plan.Rows.Categories)
        Assert.IsFalse(plan.Rows.Categories.SequenceEqual({10, 2, 3}), "Lexicographic display sorting would mislabel probability columns.")
    End Sub

    <TestMethod>
    Public Sub ReportRejectsMisalignmentAndNeverInfersConvergence()
        Dim req = Request(RegressionAnalysisKind.GeneralizedLinearModel), rows = req.Primary.RowIds
        Assert.ThrowsException(Of ArgumentException)(Sub() GC.KeepAlive(New RegressionRowOutput(rows, New Double(1, 0) {}, Nothing)))
        Assert.ThrowsException(Of ArgumentException)(Sub() GC.KeepAlive(New RegressionRowOutput(rows, Nothing, New Object(1, 1) {})))
        Assert.ThrowsException(Of ArgumentException)(Sub() GC.KeepAlive(New RegressionRowOutput({1, 1}, Nothing, Nothing)))
        Assert.ThrowsException(Of ArgumentException)(Sub() RegressionReportPolicy.Compose(req, {}, New RegressionRowOutput({1, 2, 3, 4}, Nothing, Nothing)))
        Dim plan = RegressionReportPolicy.Compose(req, {}, New RegressionRowOutput(rows, Nothing, Nothing))
        Assert.AreEqual(RegressionConvergence.Unknown, plan.Convergence)
        Assert.IsTrue(plan.Diagnostics.Any(Function(d) d.Code = "regression.fit.convergence-unknown"))
        Dim failed = RegressionReportPolicy.Compose(req, {}, New RegressionRowOutput(rows, Nothing, Nothing), False)
        Assert.AreEqual(RegressionConvergence.NotConverged, failed.Convergence)
        Assert.AreEqual("regression.fit.not-converged", failed.Diagnostics(0).Code)
    End Sub

    <TestMethod>
    Public Sub ExistingNb2ConvergenceFailureIsCharacterizedWithoutNumericalChange()
        Dim f = Regression09Fixture.Load("glm_nb2_full.csv"), model As New GLM_NB(New Log())
        model.data(f.Data, f.Ids, f.Offset, f.Weights) : model.setVarNames(f.Names) : model.settingInputs(0.05, 1, 0.000001)
        StringAssert.Contains(Assert.ThrowsException(Of ArgumentException)(Sub() model.Fit(1, False)).Message, "Algorithm failed")
    End Sub
End Class
