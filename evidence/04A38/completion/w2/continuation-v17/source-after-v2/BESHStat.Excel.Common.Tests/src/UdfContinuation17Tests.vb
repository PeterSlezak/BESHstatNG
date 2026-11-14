Option Explicit On
Option Strict On
Imports System
Imports BESHStatNG.ExcelCommon
Imports Microsoft.VisualStudio.TestTools.UnitTesting

<TestClass>
Public Class UdfContinuation17Tests
    <DataTestMethod>
    <DataRow("ID", "Identity")>
    <DataRow("exchangeable", "CS")>
    <DataRow("DIAG", "Diagonal")>
    <DataRow("CSH", "Heterogeneous CS")>
    <DataRow("auto-regressive", "AR(1)")>
    <DataRow("ARH", "Heterogeneous AR(1)")>
    <DataRow("TOEP", "Toeplitz (TOEP)")>
    <DataRow("TOEPH", "Heterogeneous Toeplitz (TOEPH)")>
    <DataRow("UN", "UN")>
    Public Sub CovarianceClassifier17UsesTheSingleCanonicalAliasMap(raw As String, expected As String)
        Dim canonical As String = "stale"
        Assert.IsTrue(UdfScalarUtilitiesOptions38.TryClassifyMixedModelResidualCovarianceName(raw, "fallback", canonical))
        Assert.AreEqual(expected, canonical)
        Assert.AreEqual(expected, UdfScalarUtilitiesOptions38.NormalizeMixedModelResidualCovarianceName(raw, "fallback"))
    End Sub

    <TestMethod>
    Public Sub CovarianceClassifier17UnknownResetsOnlyItsNewOutParameterAndOldStringApiReturnsOriginalReference()
        Dim raw As New String(New Char() {"u"c, "?"c})
        Dim canonical As String = "stale"
        Assert.IsFalse(UdfScalarUtilitiesOptions38.TryClassifyMixedModelResidualCovarianceName(raw, "fallback", canonical))
        Assert.IsNull(canonical)
        Assert.AreSame(raw, UdfScalarUtilitiesOptions38.NormalizeMixedModelResidualCovarianceName(raw, "fallback"))
    End Sub

    <TestMethod>
    Public Sub CovarianceClassifier17BlankAndByRefAliasDefaultsRetainValues()
        Dim canonical As String = "stale"
        Assert.IsTrue(UdfScalarUtilitiesOptions38.TryClassifyMixedModelResidualCovarianceName(Nothing, Nothing, canonical))
        Assert.IsNull(canonical)
        Dim aliased As String = "UN"
        Assert.IsTrue(UdfScalarUtilitiesOptions38.TryClassifyMixedModelResidualCovarianceName(aliased, aliased, aliased))
        Assert.AreEqual("UN", aliased)
    End Sub

    <TestMethod>
    Public Sub Mmrm17AlignmentDropsCompleteRowsInOriginalOrderAndRetainsVisitAlignment()
        Dim y As Object(,) = {{1.0}, {"bad"}, {3.0}, {4.0}}
        Dim x As Object(,) = {{10.0, 11.0}, {20.0, 21.0}, {30.0, 31.0}, {Double.NaN, 41.0}}
        Dim subjects As Object(,) = {{"b"}, {"a"}, {"a"}, {"c"}}
        Dim visits As Object(,) = {{2.0}, {1.0}, {3.0}, {4.0}}
        Dim yy() As Double = Nothing, xx(,) As Double = Nothing, ss() As Object = Nothing, vv() As Double = Nothing
        Dim dropped As Integer = 0, failure As String = Nothing
        Assert.IsTrue(UdfW2CompletionPreparation38.AlignMmrmRows(y, x, subjects, visits, New Integer() {1, 0}, True, 4, AddressOf StrictNumber17, Function(v) Convert.ToString(v), yy, xx, ss, vv, dropped, failure))
        CollectionAssert.AreEqual(New Double() {1, 3}, yy)
        CollectionAssert.AreEqual(New Object() {"b", "a"}, ss)
        CollectionAssert.AreEqual(New Double() {2, 3}, vv)
        Assert.AreEqual(11.0, xx(0, 0))
        Assert.AreEqual(30.0, xx(1, 1))
        Assert.AreEqual(2, dropped)
        Assert.IsNull(failure)
    End Sub

    Private Shared Function StrictNumber17(value As Object) As Double?
        If Not TypeOf value Is Double Then Return Nothing
        Dim d As Double = DirectCast(value, Double)
        If Double.IsNaN(d) OrElse Double.IsInfinity(d) Then Return Nothing
        Return d
    End Function

    <TestMethod>
    Public Sub Mmrm17AlignmentNoValidRowsPreservesFailureAndUnpublishedOutputs()
        Dim yy() As Double = Nothing, xx(,) As Double = Nothing, ss() As Object = Nothing, vv() As Double = Nothing
        Dim dropped As Integer = 0, failure As String = Nothing
        Assert.IsFalse(UdfW2CompletionPreparation38.AlignMmrmRows(New Object(,) {{"bad"}}, New Object(,) {{1.0}}, New Object(,) {{"s"}}, Nothing, New Integer() {0}, False, 1, AddressOf StrictNumber17, Function(v) Convert.ToString(v), yy, xx, ss, vv, dropped, failure))
        Assert.AreEqual(1, dropped)
        Assert.IsNull(yy)
        Assert.IsNull(xx)
        Assert.IsTrue(failure.StartsWith("no valid complete rows remain", StringComparison.Ordinal))
    End Sub

    <TestMethod>
    Public Sub Mmrm17FormulaBlankClonesNamesAndRetainsRawColumnOrder()
        Dim indices() As Integer = Nothing, selected() As String = Nothing, letters() As String = Nothing, failure As String = Nothing
        Dim names() As String = {"dose", "group"}
        Assert.IsTrue(UdfW2CompletionPreparation38.ResolveMmrmFormulaColumns("", "names", 2, names, New String() {"D", "E"}, True, indices, selected, letters, failure))
        CollectionAssert.AreEqual(New Integer() {0, 1}, indices)
        CollectionAssert.AreEqual(names, selected)
        Assert.AreNotSame(names, selected)
        CollectionAssert.AreEqual(New String() {"D", "E"}, letters)
    End Sub

    <TestMethod>
    Public Sub Mmrm17AbsoluteFormulaRejectsMissingRangeBeforeParsing()
        Dim indices() As Integer = Nothing, selected() As String = Nothing, letters() As String = Nothing, failure As String = Nothing
        Assert.IsFalse(UdfW2CompletionPreparation38.ResolveMmrmFormulaColumns("invalid(", "absolute", 2, New String() {"a", "b"}, Nothing, False, indices, selected, letters, failure))
        Assert.IsTrue(failure.StartsWith("formulaAddressing='absolute' requires", StringComparison.Ordinal))
    End Sub

    <TestMethod>
    Public Sub Mmrm17LetterMappingUsesFirstDuplicateAndFallbackWithoutChangingSource()
        Dim original() As String = {"first", "second", "other"}
        Dim mapped = UdfW2CompletionPreparation38.MapMmrmNamesByLetter(original, New String() {"A", "A", "B"}, New String() {"B", "A", "C"}, New String() {"x", "y", "z"}, 3)
        CollectionAssert.AreEqual(New String() {"other", "first", "z"}, mapped)
        CollectionAssert.AreEqual(New String() {"first", "second", "other"}, original)
    End Sub

    <TestMethod>
    Public Sub Categorical17AllResidualBlocksKeepDifferentObservedColumnShapes()
        Dim res As New Global.BESHStatNG.regression.MultinomialResiduals With {
            .Categories = New Integer() {1, 4}, .Observed = New Double(,) {{1, 0}}, .FittedMeans = New Double(,) {{0.3, 0.7}},
            .Probabilities = New Double(,) {{0.3, 0.7}}, .ResponseResiduals = New Double(,) {{0.7, -0.7}},
            .PearsonResiduals = New Double(,) {{1, -1}}, .StdPearsonResiduals = New Double(,) {{2, -2}},
            .DevianceResiduals = New Double() {3}, .StdDevianceResiduals = New Double() {4}, .Leverage = New Double() {0.5}}
        Dim mn = DirectCast(UdfW2CompletionReports38.BuildMultinomialResidualOutput(res, res.Categories, True, "NA"), Object(,))
        Dim ord = DirectCast(UdfW2CompletionReports38.BuildOrdinalResidualOutput(res, True, "NA"), Object(,))
        Assert.AreEqual(15, mn.GetLength(1))
        Assert.AreEqual(13, ord.GetLength(1))
        Assert.AreEqual(2, mn.GetLength(0))
        Assert.AreEqual(1.0, mn(1, 0))
        Assert.AreEqual(0.3, ord(1, 0))
        Assert.AreEqual(0.5, mn(1, 14))
        Assert.AreEqual("NA", UdfW2CompletionReports38.BuildOrdinalResidualOutput(Nothing, True, "NA"))
    End Sub

    <TestMethod>
    Public Sub Zip17MaterializationPadsTruncatesAndRetainsOneEmptyRow()
        Dim rows As New Collections.Generic.List(Of Object()) From {New Object() {1, 2, 3}, New Object() {4}}
        Dim table = DirectCast(UdfW2CompletionReports38.MaterializeZipRows(rows, True, New String() {"a", "b"}), Object(,))
        Assert.AreEqual(3, table.GetLength(0))
        Assert.AreEqual(2, table.GetLength(1))
        Assert.AreEqual(2, table(1, 1))
        Assert.IsNull(table(2, 1))
        Dim empty = DirectCast(UdfW2CompletionReports38.MaterializeZipRows(New Collections.Generic.List(Of Object())(), False, Nothing), Object(,))
        Assert.AreEqual(1, empty.Length)
    End Sub

    <TestMethod>
    Public Sub Response17IntegerRejectionRetainsThePartialByRefArray()
        Dim response() As Integer = Nothing
        Assert.IsFalse(UdfW2CompletionPreparation38.TryExtractNonnegativeIntegerResponseObjects(New Object(,) {{2.0}, {1.5}}, response, AddressOf StrictNumber17))
        CollectionAssert.AreEqual(New Integer() {2, 0}, response)
        Assert.IsFalse(UdfW2CompletionPreparation38.TryExtractNonnegativeIntegerResponseObjects(New Object(,) {{-1.0}}, response, AddressOf StrictNumber17))
        CollectionAssert.AreEqual(New Integer() {0}, response)
    End Sub

    <TestMethod>
    Public Sub Mmrm17StackingKeepsBlankSeparatorsAndOriginalColumnWidths()
        Dim tables As New Collections.Generic.List(Of Object(,)) From {New Object(,) {{"a", 2}}, New Object(,) {{"b"}}}
        Dim result = UdfW2CompletionReports38.StackRenderedMmrmTables(tables, 2, 2)
        Assert.AreEqual(3, result.GetLength(0))
        Assert.AreEqual("a", result(0, 0))
        Assert.IsNull(result(1, 0))
        Assert.AreEqual("b", result(2, 0))
        Assert.IsNull(result(2, 1))
    End Sub

    <DataTestMethod>
    <DataRow("LR", "logrank")>
    <DataRow("wilcoxon", "gehan-breslow")>
    <DataRow("ware", "tarone-ware")>
    <DataRow("peto-peto", "peto")>
    <DataRow("anderson", "modified peto")>
    Public Sub Survival17WeightAliasesRemainOriginal(raw As String, expected As String)
        Assert.AreEqual(expected, UdfW2CompletionPreparation38.ParseSurvivalWeightText(raw))
        Assert.IsNull(UdfW2CompletionPreparation38.ParseSurvivalWeightText("unknown"))
    End Sub


    <TestMethod>
    Public Sub Migrated17SelectorsRetainWindowsTextComparisonForWidthAndCase()
        Assert.AreEqual(Global.BESHStatNG.regression.ReferenceCategory.First, UdfW2CompletionPreparation38.GetReferenceCategory("?????"))
        Assert.AreEqual("logrank", UdfW2CompletionPreparation38.ParseSurvivalWeightText("??"))
    End Sub
End Class
