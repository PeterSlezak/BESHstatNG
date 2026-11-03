Option Explicit On
Option Strict On
Imports System
Imports System.Collections.Generic
Imports System.Globalization
Imports System.Linq
Imports BESHStatNG
Imports BESHStatNG.ExcelCommon
Imports Microsoft.VisualStudio.TestTools.UnitTesting

<TestClass>
Public Class RegressionEffectAuthoring08Tests
    Private Shared ReadOnly Catalog As String() = {"Age | VarB", "Group | VarC", "α | VarD", "age | VarE"}
    Private Shared Function Add(model As RegressionEffectModel, op As RegressionEffectOperation, ParamArray indices As Integer()) As RegressionEffectModel
        Return RegressionEffectAuthoringPolicy.Apply(Catalog, indices, model, op).Model
    End Function
    Private Shared Sub Orders(model As RegressionEffectModel)
        CollectionAssert.AreEqual(Enumerable.Range(0, model.Effects.Count).ToArray(), model.Effects.Select(Function(e) e.Spec.Order).ToArray())
    End Sub
    <DataTestMethod> <DataRow(0)> <DataRow(1)> <DataRow(2)> <DataRow(3)> <DataRow(4)>
    Public Sub SelectionOrderRepeatedAdditionAndCoreRawIdentity(count As Integer)
        Dim selected = Enumerable.Range(0, count).Reverse().ToArray()
        Dim r = RegressionEffectAuthoringPolicy.Apply(Catalog, selected, Nothing, RegressionEffectOperation.ContinuousMain)
        Assert.AreEqual(count > 0, r.Success)
        CollectionAssert.AreEqual(Catalog.Take(count).ToArray(), r.Model.Effects.Select(Function(e) e.Key).ToArray())
        Dim again = RegressionEffectAuthoringPolicy.Apply(Catalog, selected, r.Model, RegressionEffectOperation.ContinuousMain)
        Assert.AreEqual(RegressionEffectSerialization.Serialize(r.Model), RegressionEffectSerialization.Serialize(again.Model))
        CollectionAssert.AreEqual(Catalog.Take(count).ToArray(), RegressionDesignCore.GetRequiredRawVarKeys(r.Model.Effects.Select(Function(e) e.Key).ToArray(), r.Model.ToTermSpecs()).ToArray())
        Orders(r.Model)
    End Sub
    <DataTestMethod> <DataRow(2, 1)> <DataRow(3, 3)> <DataRow(4, 6)>
    Public Sub PairwiseEnumerationAndPermissiveHierarchy(count As Integer, expected As Integer)
        Dim r = RegressionEffectAuthoringPolicy.Apply(Catalog, Enumerable.Range(0, count).Reverse(), Nothing, RegressionEffectOperation.PairwiseInteractions)
        Assert.IsTrue(r.Success) : Assert.AreEqual(expected, r.Model.Effects.Count)
        Dim keys As New List(Of String)
        For a = 0 To count - 2
            For b = a + 1 To count - 1
                keys.Add(RegressionDesignCore.MakeInteractionEffectKey({Catalog(a), Catalog(b)}))
            Next
        Next
        CollectionAssert.AreEqual(keys.ToArray(), r.Model.Effects.Select(Function(e) e.Key).ToArray())
        Assert.AreEqual(expected * 2, RegressionEffectAuthoringPolicy.InspectHierarchy(r.Model).Where(Function(i) i.Code = "parent-missing").Count())
        Orders(r.Model)
    End Sub
    <TestMethod>
    Public Sub ConflictsPartialSuccessAndRemovalCascade()
        Dim model = Add(Nothing, RegressionEffectOperation.CategoricalMain, 1)
        Dim r = RegressionEffectAuthoringPolicy.Apply(Catalog, {1, 0}, model, RegressionEffectOperation.ContinuousMain)
        Assert.IsFalse(r.Success) : Assert.AreEqual("categorical-conflict", r.Diagnostics.Single().Code)
        Assert.IsTrue(r.Changed) : Assert.IsTrue(r.PartialSuccess)
        Assert.AreEqual(Catalog(1), r.Diagnostics(0).RawKey) : Assert.AreEqual(2, r.Model.Effects.Count)
        model = Add(r.Model, RegressionEffectOperation.PairwiseInteractions, 0, 1)
        Assert.AreEqual(PredictorScale.Categorical, model.Effects(2).Spec.Scale)
        Assert.AreEqual(2, RegressionEffectAuthoringPolicy.InspectHierarchy(model).Where(Function(i) i.Code = "parent-available").Count())
        r = RegressionEffectAuthoringPolicy.Apply(Catalog, {1, 2}, model, RegressionEffectOperation.Polynomial, 3)
        Assert.AreEqual("categorical-polynomial", r.Diagnostics.Single().Code) : Assert.AreEqual(4, r.Model.Effects.Count)
        model = RegressionEffectAuthoringPolicy.Remove(r.Model, {RegressionDesignCore.MakeCategoricalEffectKey(Catalog(1))}).Model
        Assert.AreEqual(2, model.Effects.Count) : Orders(model)
        model = Add(model, RegressionEffectOperation.ContinuousMain, 1)
        Assert.AreEqual(Catalog(1), model.Effects.Last().Key)
        r = RegressionEffectAuthoringPolicy.Apply(Catalog, {1, 3}, model, RegressionEffectOperation.CategoricalMain)
        Assert.AreEqual("used-conflict", r.Diagnostics.Single().Code) : Assert.AreEqual(4, r.Model.Effects.Count)
        Assert.AreEqual(0, RegressionEffectAuthoringPolicy.Remove(r.Model, Nothing, True).Model.Effects.Count)
    End Sub
    <DataTestMethod> <DataRow(0)> <DataRow(1)> <DataRow(2)> <DataRow(3)> <DataRow(Integer.MaxValue)>
    Public Sub DegreesAndCultureAreLegacyCompatible(degree As Integer)
        Dim old = CultureInfo.CurrentCulture
        Try
            Dim canonical As String = Nothing
            For Each culture In {"en-US", "sk-SK"}
                CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo(culture)
                Dim r = RegressionEffectAuthoringPolicy.Apply(Catalog, {0}, Nothing, RegressionEffectOperation.Polynomial, degree)
                Assert.AreEqual(degree >= 2, r.Success)
                If degree < 2 Then
                    Assert.AreEqual("degree", r.Diagnostics.Single().Code)
                Else
                    Assert.AreEqual(RegressionDesignCore.MakePolynomialEffectKey(Catalog(0), degree), r.Model.Effects(0).Key)
                    Assert.AreEqual("Age^" & degree.ToString(CultureInfo.InvariantCulture), r.Model.Effects(0).Spec.DisplayNameForCoef)
                End If
                Dim serialized = RegressionEffectSerialization.Serialize(r.Model)
                If canonical IsNot Nothing Then Assert.AreEqual(canonical, serialized)
                canonical = serialized
            Next
        Finally
            CultureInfo.CurrentCulture = old
        End Try
    End Sub
    <DataTestMethod> <DataRow(0)> <DataRow(1)>
    Public Sub TooFewInteractionSelectionsAndErrorPrecedence(count As Integer)
        For Each op In {RegressionEffectOperation.PairwiseInteractions, RegressionEffectOperation.CustomInteraction}
            Dim r = RegressionEffectAuthoringPolicy.Apply(Catalog, Enumerable.Range(0, count), Nothing, op)
            Assert.AreEqual("Please select at least two variables.", r.Diagnostics.Single().Message)
        Next
        Assert.AreEqual("degree", RegressionEffectAuthoringPolicy.Apply(Nothing, Nothing, Nothing, RegressionEffectOperation.Polynomial, 1).Diagnostics.Single().Code)
    End Sub
    <DataTestMethod> <DataRow("A:B")> <DataRow("'quote'")> <DataRow("[Cat] x")> <DataRow(" x ")>
    <DataRow("a""b")> <DataRow("λ雪")> <DataRow("=SHELL('data') + factor(B)")>
    Public Sub PunctuationUnicodeAndFormulaLookingKeysAreLosslessData(key As String)
        Dim r = RegressionEffectAuthoringPolicy.Apply({key}, {0}, Nothing, RegressionEffectOperation.ContinuousMain)
        Assert.IsTrue(r.Success) : Assert.AreEqual(key, r.Model.Effects(0).Key)
        Dim text = RegressionEffectSerialization.Serialize(r.Model), restored As RegressionEffectModel = Nothing, issue As RegressionEffectDiagnostic = Nothing
        Assert.IsTrue(RegressionEffectSerialization.TryDeserialize(text, restored, issue))
        Assert.AreEqual(text, RegressionEffectSerialization.Serialize(restored))
        CollectionAssert.AreEqual({key}, restored.Effects(0).Spec.BaseVarKeys.ToArray())
    End Sub
    <TestMethod>
    Public Sub ReferenceLevelsDisplayMetadataAndSnapshotsAreDeepCopies()
        Dim s As New TermSpec With {.Kind = "MainEffect", .BaseVarKeys = New List(Of String) From {"a"}, .Degree = 1,
            .BaseVarDisplayNames = New List(Of String) From {"custom"}, .DisplayNameForCoef = "a", .Scale = PredictorScale.Categorical,
            .ReferenceValue = 2, .ObservedLevels = New List(Of Double) From {1, 2, 3}}
        Dim m As New RegressionEffectModel({New RegressionAuthoredEffect("[Cat] a", s)}, False)
        Dim serialized = RegressionEffectSerialization.Serialize(m)
        s.BaseVarKeys.Clear() : s.ObservedLevels.Clear()
        m.Effects(0).Spec.ObservedLevels.Clear() : m.ToTermSpecs()("[Cat] a").BaseVarDisplayNames.Clear()
        Assert.AreEqual(serialized, RegressionEffectSerialization.Serialize(m))
        Dim restored As RegressionEffectModel = Nothing, issue As RegressionEffectDiagnostic = Nothing
        Assert.IsTrue(RegressionEffectSerialization.TryDeserialize(serialized, restored, issue))
        Assert.IsFalse(restored.IncludeIntercept) : Assert.AreEqual(2.0, restored.Effects(0).Spec.ReferenceValue.Value)
        CollectionAssert.AreEqual({1.0, 2.0, 3.0}, restored.Effects(0).Spec.ObservedLevels.ToArray())
        Assert.IsFalse(RegressionEffectSerialization.ProjectFormula(restored, {"a"}).Representable)
    End Sub
    <DataTestMethod> <DataRow(True)> <DataRow(False)>
    Public Sub FormulaParserAndTypedCompilerUseSeparateIntercept(intercept As Boolean)
        Dim names = {"Age", "Group", "雪"}
        Dim m As New RegressionEffectModel(includeIntercept:=intercept)
        m = RegressionEffectAuthoringPolicy.Apply(names, {0, 2}, m, RegressionEffectOperation.ContinuousMain).Model
        m = RegressionEffectAuthoringPolicy.Apply(names, {1}, m, RegressionEffectOperation.CategoricalMain).Model
        m = RegressionEffectAuthoringPolicy.Apply(names, {0, 1}, m, RegressionEffectOperation.CustomInteraction).Model
        Dim projection = RegressionEffectSerialization.ProjectFormula(m, names)
        Assert.IsTrue(projection.Representable, String.Join(";", projection.Diagnostics.Select(Function(d) d.Message)))
        Assert.AreEqual("A + C + factor(B) + A:factor(B)", projection.Rhs)
        Dim rows As New List(Of List(Of SpreadsheetCellValue)) From {
            names.Prepend("Y").Select(Function(v) SpreadsheetCellValue.TextCell(v)).ToList()}
        For i = 1 To 8
            rows.Add({CDbl(i * i), CDbl(i), CDbl(1 + i Mod 2), CDbl(i Mod 3)}.Select(Function(v) SpreadsheetCellValue.NumberCell(v)).ToList())
        Next
        Dim source As New SpreadsheetRangeData With {.Rows = rows, .RowCount = rows.Count, .ColumnCount = 4}
        Dim compiled = SpreadsheetModelInputCompiler.Compile(source, New SpreadsheetModelParseOptions With {
            .FormulaText = projection.Rhs, .IncludeIntercept = intercept, .CategoricalColumnIndices = {2}})
        Assert.IsTrue(compiled.Success, String.Join(";", compiled.Validation.Issues.Select(Function(d) d.Message)))
        Assert.AreEqual(intercept, compiled.Data.IncludesIntercept)
        Assert.AreEqual(8, compiled.Data.ObservationCount)
        Dim parsed = RegressionFormulaParser.ParseFormulaToDesignSpec(projection.Rhs, RegressionVariableCatalog.Build(names, names))
        Assert.AreEqual(projection.Rhs, parsed.NormalizedFormulaText)
        Dim design As New RegressionFormulaDesignSpec With {.EffectItems = m.Effects.Select(Function(e) e.Key).ToList(), .TermSpecs = m.ToTermSpecs(),
            .RequiredRawVarKeys = RegressionDesignCore.GetRequiredRawVarKeys(m.Effects.Select(Function(e) e.Key).ToArray(), m.ToTermSpecs())}
        Dim a = RegressionFormulaDesignService.BuildExpandedPredictorMatrixFromDesignSpec(compiled.Data.RawPredictorMatrix, names, design, intercept)
        Dim b = RegressionFormulaDesignService.BuildExpandedPredictorMatrixFromDesignSpec(compiled.Data.RawPredictorMatrix, names, parsed, intercept)
        CollectionAssert.AreEqual(a.ExpandedPredictorNames, b.ExpandedPredictorNames, String.Join("|", a.ExpandedPredictorNames) & " <> " & String.Join("|", b.ExpandedPredictorNames))
        CollectionAssert.AreEqual(a.ExpandedPredictorMatrix, b.ExpandedPredictorMatrix)
    End Sub
    <TestMethod>
    Public Sub ExistingCorePolynomialInteractionLabelDifferenceIsNotSilentlyNormalized()
        Dim names = {"Age", "Group"}
        Dim m = RegressionEffectAuthoringPolicy.Apply(names, {0}, Nothing, RegressionEffectOperation.ContinuousMain).Model
        m = RegressionEffectAuthoringPolicy.Apply(names, {1}, m, RegressionEffectOperation.CategoricalMain).Model
        m = RegressionEffectAuthoringPolicy.Apply(names, {0}, m, RegressionEffectOperation.Polynomial, 3).Model
        Assert.IsTrue(RegressionEffectSerialization.ProjectFormula(m, names).Representable)
        m = RegressionEffectAuthoringPolicy.Apply(names, {0, 1}, m, RegressionEffectOperation.CustomInteraction).Model
        Assert.IsFalse(RegressionEffectSerialization.ProjectFormula(m, names).Representable)
        Dim design As New RegressionFormulaDesignSpec With {.EffectItems = m.Effects.Select(Function(e) e.Key).ToList(), .TermSpecs = m.ToTermSpecs(), .RequiredRawVarKeys = names.ToList()}
        Dim parsed = RegressionFormulaParser.ParseFormulaToDesignSpec("A + factor(B) + A^3 + A:factor(B)", RegressionVariableCatalog.Build(names, names))
        Dim raw As Double(,) = {{1, 1}, {2, 2}, {3, 1}, {4, 2}}
        Dim a = RegressionFormulaDesignService.BuildExpandedPredictorMatrixFromDesignSpec(raw, names, design, False)
        Dim b = RegressionFormulaDesignService.BuildExpandedPredictorMatrixFromDesignSpec(raw, names, parsed, False)
        CollectionAssert.AreEqual(a.ExpandedPredictorMatrix, b.ExpandedPredictorMatrix)
        Assert.AreEqual("Age^3:Group[1]", a.ExpandedPredictorNames(4))
        Assert.AreEqual("Age:Group[1]", b.ExpandedPredictorNames(4))
    End Sub
    <TestMethod>
    Public Sub FormulaNormalizationRefusesChangedInteractionOrderAndEmptyFallback()
        Dim m = RegressionEffectAuthoringPolicy.Apply({"z", "a"}, {0, 1}, Nothing, RegressionEffectOperation.CustomInteraction).Model
        Dim projection = RegressionEffectSerialization.ProjectFormula(m, {"z", "a"})
        Assert.IsFalse(projection.Representable) : Assert.AreEqual("formula-not-representable", projection.Diagnostics.Single().Code)
        For Each intercept As Boolean In {True, False}
            Assert.IsFalse(RegressionEffectSerialization.ProjectFormula(New RegressionEffectModel(includeIntercept:=intercept), {"a"}).Representable)
        Next
    End Sub
    <DataTestMethod> <DataRow(2)> <DataRow(3)>
    Public Sub CategoricalCrossesAndThreeWayPreserveCoreMatrixAndOrder(width As Integer)
        Dim names = {"a", "b", "c"}
        Dim m = RegressionEffectAuthoringPolicy.Apply(names, {0, 1}, Nothing, RegressionEffectOperation.CategoricalMain).Model
        m = RegressionEffectAuthoringPolicy.Apply(names, Enumerable.Range(0, width), m, RegressionEffectOperation.CustomInteraction).Model
        Dim f = RegressionEffectSerialization.ProjectFormula(m, names)
        Assert.IsTrue(f.Representable)
        Dim raw As Double(,) = {{1, 1, 2}, {2, 1, 3}, {1, 2, 4}, {2, 2, 5}}
        Dim parsed = RegressionFormulaParser.ParseFormulaToDesignSpec(f.Rhs, RegressionVariableCatalog.Build(names, names))
        Dim original As New RegressionFormulaDesignSpec With {.EffectItems = m.Effects.Select(Function(e) e.Key).ToList(), .TermSpecs = m.ToTermSpecs(),
            .RequiredRawVarKeys = RegressionDesignCore.GetRequiredRawVarKeys(m.Effects.Select(Function(e) e.Key).ToArray(), m.ToTermSpecs())}
        Dim a = RegressionFormulaDesignService.BuildExpandedPredictorMatrixFromDesignSpec(raw, names, original)
        Dim b = RegressionFormulaDesignService.BuildExpandedPredictorMatrixFromDesignSpec(raw, names, parsed)
        CollectionAssert.AreEqual(a.ExpandedPredictorNames, b.ExpandedPredictorNames)
        CollectionAssert.AreEqual(a.ExpandedPredictorMatrix, b.ExpandedPredictorMatrix)
    End Sub
    <TestMethod>
    Public Sub BoundsMalformedInputsAndCanonicalVersionFailures()
        Assert.AreEqual("unknown-variable", RegressionEffectAuthoringPolicy.Apply(Catalog, {8}, Nothing, RegressionEffectOperation.ContinuousMain).Diagnostics.Single().Code)
        Assert.AreEqual("unknown-variable", RegressionEffectAuthoringPolicy.Apply({CType(Nothing, String)}, {0}, Nothing, RegressionEffectOperation.ContinuousMain).Diagnostics.Single().Code)
        Dim r = RegressionEffectAuthoringPolicy.Apply(Catalog, {0, 9}, Nothing, RegressionEffectOperation.CustomInteraction)
        Assert.AreEqual(0, r.Model.Effects.Count)
        r = RegressionEffectAuthoringPolicy.Apply(Catalog, {0, 1}, Nothing, RegressionEffectOperation.ContinuousMain, maximumEffects:=1)
        Assert.AreEqual(1, r.Model.Effects.Count) : Assert.AreEqual("effect-budget", r.Diagnostics.Single().Code)
        r = RegressionEffectAuthoringPolicy.Apply(Catalog, {0}, Nothing, RegressionEffectOperation.Polynomial, Integer.MaxValue, limits:=New RegressionEffectAuthoringLimits())
        Assert.AreEqual("input-budget", r.Diagnostics.Single().Code)
        Dim longKey = New String("x"c, 40000)
        Assert.AreEqual(longKey, RegressionEffectAuthoringPolicy.Apply({longKey}, {0}, Nothing, RegressionEffectOperation.ContinuousMain).Model.Effects(0).Key)
        Assert.AreEqual("input-budget", RegressionEffectAuthoringPolicy.Apply({longKey}, {0}, Nothing, RegressionEffectOperation.ContinuousMain, limits:=New RegressionEffectAuthoringLimits()).Diagnostics.Single().Code)
        Dim model As RegressionEffectModel = Nothing, diagnostic As RegressionEffectDiagnostic = Nothing
        For Each text As String In {Nothing, "", "<!DOCTYPE x SYSTEM 'file:///secret'><x/>", "<regression-effects version='2'/>", New String("x"c, 100)}
            Assert.IsFalse(RegressionEffectSerialization.TryDeserialize(text, model, diagnostic, 80))
            Assert.AreEqual("serialization", diagnostic.Code) : Assert.IsNull(model)
        Next
    End Sub
    <TestMethod>
    Public Sub CommonAssemblyHasNoHostReferences()
        Dim refs = GetType(RegressionEffectAuthoringPolicy).Assembly.GetReferencedAssemblies().Select(Function(a) a.Name).ToArray()
        For Each forbidden In {"Windows.Forms", "ExcelDna", "Interop", "Office", "WebAssembly"}
            Assert.IsFalse(refs.Any(Function(n) n.IndexOf(forbidden, StringComparison.OrdinalIgnoreCase) >= 0), forbidden)
        Next
    End Sub
    <TestMethod>
    Public Sub NonXmlCodeUnitsAndWhitespaceHaveLosslessTypedSerialization()
        For Each key As String In {"line" & Convert.ToChar(0) & "data", New String({Convert.ToChar(&HD800)}), " x " & Environment.NewLine, "x" & Convert.ToChar(10) & "y"}
            Dim m = RegressionEffectAuthoringPolicy.Apply({key}, {0}, Nothing, RegressionEffectOperation.ContinuousMain).Model
            Dim serialized = RegressionEffectSerialization.Serialize(m), restored As RegressionEffectModel = Nothing, issue As RegressionEffectDiagnostic = Nothing
            Assert.IsTrue(RegressionEffectSerialization.TryDeserialize(serialized, restored, issue))
            Assert.AreEqual(key, restored.Effects(0).Key)
        Next
    End Sub
    <TestMethod>
    Public Sub ExplicitReferenceCodingAndDuplicateCoefficientLabelsKeepRawIdentity()
        Dim names = {"Age | VarB", "Age | VarC"}
        Dim m = RegressionEffectAuthoringPolicy.Apply(names, {0, 1}, Nothing, RegressionEffectOperation.ContinuousMain).Model
        Assert.IsTrue(RegressionEffectSerialization.ProjectFormula(m, names).Representable)
        CollectionAssert.AreEqual(names, RegressionDesignCore.GetRequiredRawVarKeys(m.Effects.Select(Function(e) e.Key).ToArray(), m.ToTermSpecs()).ToArray())
        Dim s = m.Effects(0).Spec : s.Scale = PredictorScale.Categorical : s.ReferenceValue = 2
        m = New RegressionEffectModel({New RegressionAuthoredEffect("[Cat] " & names(0), s)})
        Dim projection = RegressionEffectSerialization.ProjectFormula(m, names)
        Assert.IsTrue(projection.Representable) : Assert.AreEqual("factor(A, ref=2)", projection.Rhs)
        Dim parsed = RegressionFormulaParser.ParseFormulaToDesignSpec(projection.Rhs, RegressionVariableCatalog.Build(names, names))
        Assert.AreEqual(2.0, parsed.TermSpecs("[Cat] " & names(0)).ReferenceValue.Value)
        s.ObservedLevels = New List(Of Double) From {1, 3}
        Dim invalid As New RegressionEffectModel({New RegressionAuthoredEffect("[Cat] " & names(0), s)})
        Assert.AreEqual("observed-reference", RegressionEffectAuthoringPolicy.Inspect(invalid, names).Single().Code)
        Assert.AreEqual("unknown-variable", RegressionEffectAuthoringPolicy.Inspect(m, {"renamed"}).Single().Code)
    End Sub
    <DataTestMethod> <DataRow(0, 1)> <DataRow(1, 1)> <DataRow(2, 2)> <DataRow(2, 3)>
    Public Sub SingleTermFormulaAndCoreDesignRoundTripQuotedUnicodeNames(operation As Integer, degree As Integer)
        Dim names = {"a'雪:quoted"" | VarB"}
        Dim m = RegressionEffectAuthoringPolicy.Apply(names, {0}, Nothing, CType(operation, RegressionEffectOperation), degree).Model
        Dim projection = RegressionEffectSerialization.ProjectFormula(m, names)
        Assert.IsTrue(projection.Representable)
        Dim parsed = RegressionFormulaParser.ParseFormulaToDesignSpec(projection.Rhs, RegressionVariableCatalog.Build(names.Select(Function(n) RegressionDesignCore.GetCoefBaseName(n)), names))
        Dim old As New RegressionFormulaDesignSpec With {.EffectItems = m.Effects.Select(Function(e) e.Key).ToList(), .TermSpecs = m.ToTermSpecs(), .RequiredRawVarKeys = names.ToList()}
        Dim raw As Double(,) = {{1}, {2}, {3}, {1}}
        Dim a = RegressionFormulaDesignService.BuildExpandedPredictorMatrixFromDesignSpec(raw, names, old)
        Dim b = RegressionFormulaDesignService.BuildExpandedPredictorMatrixFromDesignSpec(raw, names, parsed)
        CollectionAssert.AreEqual(a.ExpandedPredictorNames, b.ExpandedPredictorNames)
        CollectionAssert.AreEqual(a.ExpandedPredictorMatrix, b.ExpandedPredictorMatrix)
    End Sub
    <TestMethod>
    Public Sub OrdinalCaseOnlyKeysAndOptionalBudgetsDoNotChangeLegacyDefaults()
        Dim m = RegressionEffectAuthoringPolicy.Apply({"a", "A"}, {1, 0}, Nothing, RegressionEffectOperation.ContinuousMain).Model
        Assert.AreEqual(2, m.ToTermSpecs().Count)
        CollectionAssert.AreEqual({"a", "A"}, m.Effects.Select(Function(e) e.Key).ToArray())
        Assert.IsTrue(RegressionEffectSerialization.ProjectFormula(m, {"a", "A"}).Representable)
        Dim limits As New RegressionEffectAuthoringLimits With {.MaximumCatalogSize = 2, .MaximumInteractionWidth = 2}
        Assert.AreEqual("input-budget", RegressionEffectAuthoringPolicy.Apply(Catalog, {0}, Nothing, RegressionEffectOperation.ContinuousMain, limits:=limits).Diagnostics.Single().Code)
        limits.MaximumCatalogSize = 4
        Assert.AreEqual("input-budget", RegressionEffectAuthoringPolicy.Apply(Catalog, {0, 1, 2}, Nothing, RegressionEffectOperation.CustomInteraction, limits:=limits).Diagnostics.Single().Code)
        Assert.IsTrue(RegressionEffectAuthoringPolicy.Apply(Catalog, {0, 1, 2}, Nothing, RegressionEffectOperation.CustomInteraction).Success)
        Assert.AreEqual("remove-selection", RegressionEffectAuthoringPolicy.Remove(m, Nothing).Diagnostics.Single().Code)
        Assert.AreEqual(0, RegressionEffectAuthoringPolicy.Remove(Nothing, Nothing).Model.Effects.Count)
    End Sub
    <TestMethod>
    Public Sub CanonicalMetadataHasExactRuntimeIndependentDoubleBits()
        Dim s As New TermSpec With {.Kind = "MainEffect", .Degree = 1, .BaseVarKeys = New List(Of String) From {"a"}, .Scale = PredictorScale.Categorical,
            .ReferenceValue = BitConverter.Int64BitsToDouble(&H3FD5555555555555L), .ObservedLevels = New List(Of Double) From {BitConverter.Int64BitsToDouble(Long.MinValue)}}
        Dim text = RegressionEffectSerialization.Serialize(New RegressionEffectModel({New RegressionAuthoredEffect("[Cat] a", s)}))
        StringAssert.Contains(text, "<reference>3FD5555555555555</reference>")
        StringAssert.Contains(text, "<value>8000000000000000</value>")
        Dim restored As RegressionEffectModel = Nothing, issue As RegressionEffectDiagnostic = Nothing
        Assert.IsTrue(RegressionEffectSerialization.TryDeserialize(text, restored, issue))
        Assert.AreEqual(Long.MinValue, BitConverter.DoubleToInt64Bits(restored.Effects(0).Spec.ObservedLevels(0)))
    End Sub
End Class
