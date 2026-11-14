Option Explicit On
Option Strict On

Imports System
Imports System.Globalization
Imports Microsoft.VisualStudio.TestTools.UnitTesting
Imports BESHStatNG
Imports BESHStatNG.ExcelCommon

<TestClass>
Public Class UdfPreparation38Tests
    <TestMethod>
    Public Sub ExplicitTextPolicyRetainsWidthCaseAndNullComparison()
        For Each name As String In {String.Empty, "sk-SK"}
            Dim previous As CultureInfo = CultureInfo.CurrentCulture
            Try
                CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo(name)
                Assert.IsTrue(LegacyUdfText38.EqualText(Nothing, String.Empty))
                Assert.IsTrue(LegacyUdfText38.EqualText("ml", "ML"))
                Assert.IsTrue(LegacyUdfText38.EqualText("ＭＬ", "ML"))
                Assert.AreEqual("ML", LegacyUdfText38.CaseToken("ｍｌ", {"REML", "ML"}))
            Finally
                CultureInfo.CurrentCulture = previous
            End Try
        Next
    End Sub

    <TestMethod>
    Public Sub MixedOptionStrictnessAndAliasDefaultsRemainDistinct()
        Assert.AreEqual(Global.BESHStatNG.regression.MixedModelFitMethod.REML, UdfScalarUtilitiesOptions38.ParseMixedModelFitMethodStrict(String.Empty, "MMRM"))
        Assert.AreEqual(Global.BESHStatNG.regression.MixedModelFitMethod.ML, UdfScalarUtilitiesOptions38.ParseMixedModelFitMethodStrict("maximum likelihood", "MMRM"))
        Dim errorValue = Assert.ThrowsException(Of ArgumentException)(Sub() UdfScalarUtilitiesOptions38.ParseMixedModelFitMethodStrict("unknown", "MMRM"))
        Assert.AreEqual("Unsupported MMRM fitMethod. Use REML or ML.", errorValue.Message)
    End Sub

    <TestMethod>
    Public Sub NormalizationKeepsSeparateSeparatorPolicies()
        Assert.AreEqual("AB_C", UdfScalarUtilitiesOptions38.NormalizeMatchToken(" a-b_c "))
        Assert.AreEqual("abc", UdfScalarUtilitiesOptions38.NormalizeKey(" A-B_C "))
        Assert.AreEqual("AR1", UdfScalarUtilitiesOptions38.NormalizeMixedModelOptionToken(" AR(1) "))
    End Sub

    <TestMethod>
    Public Sub ReportProjectionDefensivelyCopiesValuesAndMetadata()
        Dim raw As Object(,) = {{"Header", BitConverter.Int64BitsToDouble(Long.MinValue)}, {"Nested", New Double() {1.0, 2.0}}}
        Dim model As New ResultTableOutputModel(raw, pvalueColumns:={2}, pvalueCells:={New ResultTableCellAddress(1, 2)})
        Dim projection As New UdfReportProjection38(model)
        raw(0, 0) = "changed"
        DirectCast(raw(1, 1), Double())(0) = 99.0
        model.PvalueColumns.Clear()
        Dim first As ResultTableOutputModel = projection.ToOutputModel()
        Assert.AreEqual("Header", first.Values(0, 0))
        Assert.AreEqual(1.0, DirectCast(first.Values(1, 1), Double())(0))
        Assert.AreEqual(Long.MinValue, BitConverter.DoubleToInt64Bits(CDbl(first.Values(0, 1))))
        first.Values(0, 0) = "modified getter"
        first.PvalueCells.Clear()
        Assert.AreEqual("Header", projection.ToOutputModel().Values(0, 0))
        Assert.AreEqual(1, projection.ToOutputModel().PvalueCells.Count)
        Assert.AreEqual(1, projection.ToOutputModel().PvalueColumns.Count)
    End Sub

    <TestMethod>
    Public Sub ProjectionRetainsEmptyDimensionsAndArrayBounds()
        Dim empty As New UdfReportProjection38(New ResultTableOutputModel(New Object(-1, 2) {}))
        Assert.AreEqual(0, empty.ToOutputModel().RowCount)
        Assert.AreEqual(3, empty.ToOutputModel().ColumnCount)
        Dim bounded As Array = Array.CreateInstance(GetType(Object), {2, 2}, {3, 5})
        bounded.SetValue("source", 3, 5)
        Dim projection As New UdfReportProjection38(New ResultTableOutputModel(DirectCast(bounded, Object(,))))
        Assert.AreEqual(3, projection.ToOutputModel().Values.GetLowerBound(0))
        Assert.AreEqual(5, projection.ToOutputModel().Values.GetLowerBound(1))
    End Sub

    <TestMethod>
    Public Sub TypedReportBuildersPreserveHeaderAndObservationIdentity()
        Dim report As UdfReportProjection38 = UdfOutputTablesReports38.BuildCaseMatrixOutput("Row", {17, 4}, {"PC1", "PC2"}, New Double(,) {{1.0, 2.0}, {3.0, 4.0}}, True)
        Dim values As Object(,) = report.ToOutputModel().Values
        Assert.AreEqual("Row", values(0, 0))
        Assert.AreEqual(17, values(1, 0))
        Assert.AreEqual(4, values(2, 0))
        Assert.AreEqual(4.0, values(2, 2))
        values(1, 0) = 999
        Assert.AreEqual(17, report.ToOutputModel().Values(1, 0))
        Assert.AreEqual(UdfReportPreparation38.NormalizeBlanks, report.Preparation)
    End Sub

    <TestMethod>
    Public Sub NativeAndMutablePayloadsCannotEnterReportProjection()
        Assert.ThrowsException(Of ArgumentException)(Sub()
                                                        Dim rejected As New UdfReportProjection38(New ResultTableOutputModel(New Object(,) {{DayOfWeek.Monday}}))
                                                    End Sub)
        Assert.ThrowsException(Of ArgumentException)(Sub()
                                                        Dim rejected As New UdfReportProjection38(New ResultTableOutputModel(New Object(,) {{New Object()}}))
                                                    End Sub)
    End Sub

    <TestMethod>
    Public Sub ExplicitSafeNumberUsesExistingTypedErrorIntent()
        Assert.AreEqual(OutputErrorKind.InvalidNumber, UdfReportProjection38.SafeNumber(Double.NaN).ErrorKind.Value)
        Assert.AreEqual(OutputErrorKind.InvalidNumber, UdfReportProjection38.SafeNumber(Double.PositiveInfinity).ErrorKind.Value)
        Assert.AreEqual(Long.MinValue, BitConverter.DoubleToInt64Bits(UdfReportProjection38.SafeNumber(BitConverter.Int64BitsToDouble(Long.MinValue)).BinaryValue.Value))
    End Sub

    <TestMethod>
    Public Sub ScalarChoicesRetainTruncationAndDistinctBooleanRules()
        Dim binary As Integer
        Assert.IsTrue(UdfScalarChoice38.TryBinary(0.75, Nothing, binary))
        Assert.AreEqual(0, binary)
        Assert.AreEqual(-1, UdfScalarChoice38.OptionalInteger(-1.9, 7))
        Assert.IsTrue(UdfScalarChoice38.OptionalBoolean("yes", False))
        Assert.IsFalse(UdfScalarChoice38.OptionalBoolean("unknown", False))
        Assert.ThrowsException(Of OverflowException)(Sub() UdfScalarChoice38.OptionalInteger(Double.MaxValue, 0))
    End Sub

    <TestMethod>
    Public Sub NumericChoicesRetainFailureResultsAndInvariantFirstParsing()
        Dim whole As Integer = 9
        Assert.IsFalse(UdfScalarChoice38.TryWholeNumber(1.5, whole))
        Assert.AreEqual(0, whole)
        Assert.IsTrue(UdfScalarChoice38.TryWholeNumber(1.00000001, whole))
        Assert.AreEqual(1, whole)
        Dim parsed As Double
        Assert.IsFalse(UdfScalarChoice38.TryFiniteText("NaN", parsed))
        Assert.IsTrue(Double.IsNaN(parsed))
        Assert.IsTrue(UdfScalarChoice38.TryPositiveInteger(Nothing, True, whole, 4, 1))
        Assert.AreEqual(4, whole)
    End Sub

    <TestMethod>
    Public Sub HeaderDecisionDoesNotReadCellsBehindAnEarlyGuard()
        Dim read As Func(Of Integer, Integer, Boolean) = Function(row, column) ThrowUnexpectedRead38()
        Assert.IsFalse(UdfRangePreparation38.HasNumericMatrixHeader(0, 2, read, read))
        Assert.IsFalse(UdfRangePreparation38.HasNumericColumnHeader(0, 1, read, read))
        Assert.IsFalse(UdfRangePreparation38.HasBinaryColumnHeader(0, read))
        Assert.IsFalse(UdfRangePreparation38.LooksLikeHeaderRow(1, {0}, read))
        Assert.IsFalse(UdfRangePreparation38.LooksLikeSingleColumnHeader(1, read))
    End Sub

    Private Shared Function ThrowUnexpectedRead38() As Boolean
        Throw New InvalidOperationException("An early guard must not evaluate the cell reader.")
    End Function

    <TestMethod>
    Public Sub TrailingBlankScanRetainsBottomUpRowMajorEvaluation()
        Dim calls As New System.Collections.Generic.List(Of String)()
        Dim last As Integer = UdfRangePreparation38.FindLastNonBlankRow(3, 2,
            Function(row, column)
                calls.Add(row.ToString() & ":" & column.ToString())
                Return row <> 1 OrElse column <> 0
            End Function)
        Assert.AreEqual(1, last)
        CollectionAssert.AreEqual(New String() {"2:0", "2:1", "1:0"}, calls.ToArray())
    End Sub

    <TestMethod>
    Public Sub NumericMatrixHeaderDistinguishesMissingDataFromLabelRow()
        Dim blanks As Func(Of Integer, Integer, Boolean) = Function(row, column) False
        Assert.IsTrue(UdfRangePreparation38.HasNumericMatrixHeader(1, 2, blanks, Function(row, column) row = 1))
        Assert.IsFalse(UdfRangePreparation38.HasNumericMatrixHeader(1, 2, blanks, Function(row, column) row = 1 OrElse column = 1))
    End Sub

    <TestMethod>
    Public Sub PvalueAddressIntentRetainsOrderAndDuplicateCells()
        Dim model As New ResultTableOutputModel(New Object(2, 1) {}, headerTopRows:=1,
            pvalueColumns:={2}, pvalueCells:={New ResultTableCellAddress(1, 2)})
        Dim cells As UdfSpillCell38() = UdfSpillPreparation38.PvalueCells(model, 3, 2)
        Assert.AreEqual(3, cells.Length)
        Assert.AreEqual(1, cells(0).RowOrdinal)
        Assert.AreEqual(2, cells(1).RowOrdinal)
        Assert.AreEqual(1, cells(2).RowOrdinal)
        Assert.AreEqual(1, cells(2).ColumnOrdinal)
        cells(0) = Nothing
        Assert.IsNotNull(UdfSpillPreparation38.PvalueCells(model, 3, 2)(0))
    End Sub

    <TestMethod>
    Public Sub NativeScalarReferencesUseSourceOrdinalRatherThanWorksheetRows()
        Dim projection As UdfReportProjection38 = UdfSpillPreparation38.TwoValueTable("Test", "Statistic", "P-value")
        Dim model As ResultTableOutputModel = projection.ToOutputModel()
        Assert.AreEqual(0, DirectCast(model.Values(1, 1), UdfSourceCellReference38).SourceOrdinal)
        Assert.AreEqual(1, DirectCast(model.Values(2, 1), UdfSourceCellReference38).SourceOrdinal)
        Assert.AreEqual(1, UdfSpillPreparation38.PvalueCells(model, 3, 2).Length)
        Assert.IsTrue(UdfSpillPreparation38.IsPValueLabel("Two-sided p-value"))
        Assert.IsFalse(UdfSpillPreparation38.IsPValueLabel("Estimate"))
    End Sub

    <TestMethod>
    Public Sub ParameterNamePoliciesKeepTheirDifferentFallbackRules()
        CollectionAssert.AreEqual(New String() {"X1"}, MixedModelUDFsMetadata38.NormalizeMmrmNameList({"X1"}, 1, "Z"))
        CollectionAssert.AreEqual(New String() {"Z1"}, LmmUDFsMetadata38.NormalizeNameList({"X1"}, 1, "Z"))
        CollectionAssert.AreEqual(New String() {"Intercept", "x", "Param3"}, GEEUDFsMetadata38.BuildParameterNames({"x"}, 3))
    End Sub

    <TestMethod>
    Public Sub FormulaDiagnosticKeepsQuotedFragmentAndCharacterOrdinal()
        Dim fragment As String = Nothing
        Dim position As Integer
        Assert.IsTrue(RegressionFormulaUDFsMetadata38.TryLocateOffendingFragment("y ~ 'a b'", "Unknown variable 'a b'", fragment, position))
        Assert.AreEqual("'a b'", fragment)
        Assert.AreEqual(4, position)
    End Sub

    <TestMethod>
    Public Sub GroupExtractionNamesOnlyNonemptyColumnsInAcceptedOrder()
        Dim values As Double?(,) = {{1.0, Nothing, 7.0}, {2.0, Nothing, Nothing}}
        Dim groups As Double()() = Nothing
        Dim names As String() = Nothing
        Assert.IsTrue(UdfNumericImportPreparation38.TryExtractNumericGroupsFromMatrix(2, 3, 0, False, "Group ",
            Function(row, column) values(row, column), Function(row, column) "unused", groups, names))
        CollectionAssert.AreEqual(New String() {"Group 1", "Group 2"}, names)
        CollectionAssert.AreEqual(New Double() {1.0, 2.0}, groups(0))
        CollectionAssert.AreEqual(New Double() {7.0}, groups(1))
    End Sub

    <TestMethod>
    Public Sub CompleteCaseImportRetainsLegacySmallSampleShape()
        Dim matrix As Double(,) = Nothing
        Dim rows As Integer
        Dim columns As Integer
        Assert.IsTrue(UdfNumericImportPreparation38.ExtractCompleteNumericMatrixCompleteCases(1, 2,
            Function(row, column) CType(column + 1.0, Double?), matrix, rows, columns))
        Assert.AreEqual(1, rows)
        Assert.AreEqual(2, columns)
        Assert.IsNull(matrix)
    End Sub

    <TestMethod>
    Public Sub StrictVectorSkipsOneHeaderTrimsOnlyTrailingBlanksAndRejectsInteriorGaps()
        Dim cells As Double?() = {Nothing, 2.0, 3.0, Nothing}
        Dim result As Double() = Nothing
        Assert.IsTrue(UdfNumericImportPreparation38.TryStrictVector(4, Function(index) index = 3, Function(index) cells(index), result))
        CollectionAssert.AreEqual(New Double() {2.0, 3.0}, result)
        cells(3) = 4.0
        Assert.IsFalse(UdfNumericImportPreparation38.TryStrictVector(4, Function(index) index = 2, Function(index) cells(index), result))
        Assert.IsNull(result)
    End Sub

    <TestMethod>
    Public Sub NonparametricChoicesRetainSeparateUnknownOptionPolicies()
        Dim result As Integer
        Assert.IsTrue(UdfScalarChoice38.TryKwType(" H_COR ", False, result))
        Assert.AreEqual(1, result)
        Assert.IsFalse(UdfScalarChoice38.TryKwType("unknown", True, result))
        Assert.AreEqual(0, result)
        Assert.AreEqual(1, UdfScalarChoice38.FriedmanType("unknown", False))
        Assert.AreEqual(2, UdfScalarChoice38.FriedmanType("", False))
    End Sub

    <TestMethod>
    Public Sub AlphaParsingPreservesParsedValueOnRejectedBoundary()
        Dim alpha As Double = 0.05
        Assert.IsFalse(UdfScalarChoice38.TryAlphaText("1", alpha))
        Assert.AreEqual(1.0, alpha)
        Assert.IsFalse(UdfScalarChoice38.IsOpenProbability(Double.NaN))
        Assert.IsTrue(UdfScalarChoice38.TryAlphaText("0.025", alpha))
        Assert.AreEqual(0.025, alpha)
    End Sub

    <TestMethod>
    Public Sub PositiveClassHonorsExplicitLabelWithoutReadingTheData()
        Dim result As String = Nothing
        Assert.IsTrue(UdfClassLabelPreparation38.GuessPositiveClass("yes", Function() ThrowUnexpectedCount38(),
            Function(index) "unused", Function(index) False, result))
        Assert.AreEqual("yes", result)
    End Sub

    Private Shared Function ThrowUnexpectedCount38() As Integer
        Throw New InvalidOperationException("Explicit label must return before acquiring data dimensions.")
    End Function

    <TestMethod>
    Public Sub PositiveClassRetainsEmptyBinaryAndFirstSeenFallbackSemantics()
        Dim result As String = Nothing
        Assert.IsTrue(UdfClassLabelPreparation38.GuessPositiveClass(Nothing, Function() 0, Function(index) "unused", Function(index) False, result))
        Assert.AreEqual("1", result)
        Dim labels As String() = {"control", "treated", "control"}
        Assert.IsTrue(UdfClassLabelPreparation38.GuessPositiveClass(Nothing, Function() 3, Function(index) labels(index), Function(index) False, result))
        Assert.AreEqual("control", result)
    End Sub
End Class
