Option Explicit On
Option Strict On
Option Infer On

Imports System
Imports System.Collections.Generic
Imports System.Globalization
Imports BESHStatNG.CausalInference
Imports BESHStatNG.ExcelCommon
Imports Microsoft.VisualStudio.TestTools.UnitTesting

<TestClass>
Public Class UdfContinuation14Tests
    <DataTestMethod, DataRow(""), DataRow("sk-SK")>
    Public Sub AliasOutputSurvivesFalseAndCallbackException(culture As String)
        Dim oldCulture = CultureInfo.CurrentCulture
        Try
            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo(culture)
            Dim label As String = "converted", calls As Integer
            Dim tokens = New String() {"red", "green", "blue"}
            Assert.IsFalse(UdfClassLabelPreparation38.GuessPositiveClassAfterConversion(" ", Function() 3,
                Function(i)
                    calls += 1
                    label = "row" & i.ToString(CultureInfo.InvariantCulture)
                    Return tokens(i)
                End Function, Function(i) False, label))
            Assert.AreEqual(3, calls)
            Assert.AreEqual("row2", label)
            Dim expected As New InvalidOperationException("row callback")
            Dim observed = Assert.ThrowsException(Of InvalidOperationException)(
                Sub() UdfClassLabelPreparation38.GuessPositiveClassAfterConversion(Nothing, Function() 1,
                    Function(i)
                        label = "throw-side-effect"
                        Throw expected
                    End Function, Function(i) False, label))
            Assert.AreSame(expected, observed)
            Assert.AreEqual("throw-side-effect", label)
        Finally
            CultureInfo.CurrentCulture = oldCulture
        End Try
    End Sub

    <DataTestMethod, DataRow(Nothing), DataRow(""), DataRow(" ")>
    Public Sub OldResettingContractAndNewPostConversionContractAreSeparate(explicitText As String)
        Dim labels = New String() {"red", "green", "blue"}, result As String = "converted"
        Assert.IsFalse(UdfClassLabelPreparation38.GuessPositiveClass(explicitText, Function() 3, Function(i) labels(i), Function(i) False, result))
        Assert.IsNull(result)
        result = "converted"
        Assert.IsFalse(UdfClassLabelPreparation38.GuessPositiveClassAfterConversion(explicitText, Function() 3, Function(i) labels(i), Function(i) False, result))
        Assert.AreEqual("converted", result)
    End Sub

    <TestMethod>
    Public Sub ExplicitTokenReturnsBeforeNullCallbacksAndBinaryEmptyStillSucceeds()
        Dim result As String = "converted", token As New String("event".ToCharArray())
        Assert.IsTrue(UdfClassLabelPreparation38.GuessPositiveClassAfterConversion(token, Nothing, Nothing, Nothing, result))
        Assert.AreSame(token, result)
        Assert.IsTrue(UdfClassLabelPreparation38.GuessPositiveClassAfterConversion("", Function() 0, Nothing, Nothing, result))
        Assert.AreEqual("1", result)
        result = "converted"
        Assert.ThrowsException(Of NullReferenceException)(Sub() UdfClassLabelPreparation38.GuessPositiveClassAfterConversion("", Nothing, Nothing, Nothing, result))
        Assert.AreEqual("converted", result)
    End Sub

    <DataTestMethod, DataRow("", "case", "control", "case"), DataRow("sk-SK", "no", "yes", "yes"), DataRow("", "treated", "control", "treated")>
    Public Sub TwoLabelsKeepPreferredTokenAndFirstSeenOrder(culture As String, first As String, second As String, expected As String)
        Dim oldCulture = CultureInfo.CurrentCulture
        Try
            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo(culture)
            Dim labels = New String() {first, second, first}, result As String = "converted"
            Assert.IsTrue(UdfClassLabelPreparation38.GuessPositiveClassAfterConversion(Nothing, Function() 3, Function(i) labels(i), Function(i) False, result))
            Assert.AreEqual(expected, result)
        Finally
            CultureInfo.CurrentCulture = oldCulture
        End Try
    End Sub

    <DataTestMethod>
    <DataRow("summary", PsmUdfTableKind38.RunSummary)>
    <DataRow("options", PsmUdfTableKind38.Options)>
    <DataRow("data-import", PsmUdfTableKind38.ImportSummary)>
    <DataRow("sample_size_summary", PsmUdfTableKind38.SampleSize)>
    <DataRow("model", PsmUdfTableKind38.ScoreModel)>
    <DataRow("weights", PsmUdfTableKind38.RowAudit)>
    <DataRow("matched-pairs", PsmUdfTableKind38.Matches)>
    <DataRow("matcheddata", PsmUdfTableKind38.MatchedData)>
    <DataRow("diagnostics", PsmUdfTableKind38.Balance)>
    <DataRow("effects", PsmUdfTableKind38.Effect)>
    <DataRow("aipw", PsmUdfTableKind38.DoublyRobust)>
    <DataRow("weightdiagnostics", PsmUdfTableKind38.WeightDiagnostics)>
    <DataRow("overlap", PsmUdfTableKind38.OverlapSummary)>
    <DataRow("overlaphistogram", PsmUdfTableKind38.OverlapBins)>
    <DataRow(" LOVE_PLOT_DATA ", PsmUdfTableKind38.LovePlot)>
    <DataRow("subclassification", PsmUdfTableKind38.Subclass)>
    <DataRow("cemstrata", PsmUdfTableKind38.CemStrata)>
    <DataRow("cemweights", PsmUdfTableKind38.CemWeights)>
    <DataRow("warnings", PsmUdfTableKind38.Warnings)>
    <DataRow(Nothing, PsmUdfTableKind38.Unknown)>
    <DataRow("nonsense", PsmUdfTableKind38.Unknown)>
    Public Sub PsmNamedTableAliasesPreserveTheirOriginalRoute(value As String, expected As PsmUdfTableKind38)
        Assert.AreEqual(expected, UdfPsmDataPreparation38.ResolveTableKindPrepared(value))
    End Sub

    <DataTestMethod, DataRow(""), DataRow("sk-SK")>
    Public Sub PsmOptionStagesKeepIdentityPrecedenceAndNonmatchingNormalization(culture As String)
        Dim oldCulture = CultureInfo.CurrentCulture
        Try
            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo(culture)
            Dim first = UdfPsmDataPreparation38.CreateFitOptionsPrepared(), other = UdfPsmDataPreparation38.CreateFitOptionsPrepared()
            Assert.AreNotSame(first.StandardOptions, other.StandardOptions)
            UdfPsmPreparation38.ApplyOptionText(first, "method=optimal;ratio=4;replacement=true")
            UdfPsmPreparation38.ApplyOptionText(first, "ratio=3")
            Assert.AreEqual(3, first.StandardOptions.MatchingRatio)
            Assert.AreSame(first, UdfPsmDataPreparation38.CompleteFitOptionsPrepared(first))
            Assert.AreEqual(1, first.StandardOptions.MatchingRatio)
            Assert.IsFalse(first.StandardOptions.WithReplacement)
            first.RunMethod = PsmBackendRunMethod.WeightingOnly
            first.StandardOptions.Caliper = 0.2
            UdfPsmDataPreparation38.CompleteFitOptionsPrepared(first)
            Assert.IsTrue(Double.IsNaN(first.StandardOptions.Caliper))
            Assert.AreEqual(PsmCaliperScale.None, first.StandardOptions.CaliperScale)
            Assert.AreEqual(PsmUdfTableKind38.RunSummary, UdfPsmDataPreparation38.ResolveTableKindPrepared(" RUN-SUMMARY "))
        Finally
            CultureInfo.CurrentCulture = oldCulture
        End Try
    End Sub

    <TestMethod>
    Public Sub PsmModelPreparationKeepsRowsNamesCellTypesAndReferenceIdentity()
        Dim marker As New Object(), treatments(,) As Object = {{1}, {0}}, covariates(,) As Object = {{marker, Nothing}, {2.5, DBNull.Value}}
        Dim matrix = UdfPsmDataPreparation38.BuildModelInputPrepared(treatments, covariates, 2, 2)
        Assert.AreEqual(2, matrix.GetLength(0))
        Assert.AreEqual(3, matrix.GetLength(1))
        Assert.AreEqual(0, matrix.GetLowerBound(0))
        Assert.AreEqual(1, matrix(0, 0))
        Assert.AreEqual(0, matrix(1, 0))
        Assert.AreSame(marker, matrix(0, 1))
        Assert.IsNull(matrix(0, 2))
        Assert.AreSame(DBNull.Value, matrix(1, 2))
        CollectionAssert.AreEqual(New String() {"T", "b", "a"}, UdfPsmDataPreparation38.BuildModelNamesPrepared("T", New String() {"b", "a"}, 2))
        Assert.AreEqual(0, UdfPsmDataPreparation38.BuildModelInputPrepared(treatments, covariates, 0, 2).GetLength(0))
    End Sub

    <TestMethod>
    Public Sub PsmModelPreparationDoesNotMaskOriginalBoundsAndNullFailures()
        Dim nonzero = DirectCast(Array.CreateInstance(GetType(Object), New Integer() {1, 1}, New Integer() {1, 1}), Object(,))
        Assert.ThrowsException(Of IndexOutOfRangeException)(Sub() UdfPsmDataPreparation38.BuildModelInputPrepared(nonzero, New Object(,) {{1}}, 1, 1))
        Assert.ThrowsException(Of NullReferenceException)(Sub() UdfPsmDataPreparation38.BuildModelInputPrepared(Nothing, New Object(,) {{1}}, 1, 1))
        Assert.ThrowsException(Of IndexOutOfRangeException)(Sub() UdfPsmDataPreparation38.BuildModelNamesPrepared("T", New String() {}, 1))
        Assert.ThrowsException(Of IndexOutOfRangeException)(Sub() UdfPsmDataPreparation38.BuildModelNamesPrepared("T", Nothing, -1))
    End Sub

    <TestMethod>
    Public Sub PsmImportReportKeepsEmptyHeaderCellsAndBoundsErrors()
        Dim marker As New Object(), rows As New List(Of Object()) From {New Object() {"ID", marker}, New Object() {Nothing, DBNull.Value}}
        Dim table = UdfPsmDataPreparation38.BuildImportSummaryPrepared(rows)
        Assert.AreEqual("Item", table(0, 0))
        Assert.AreEqual("Value", table(0, 1))
        Assert.AreSame(marker, table(1, 1))
        Assert.IsNull(table(2, 0))
        Assert.AreSame(DBNull.Value, table(2, 1))
        Assert.AreEqual(1, UdfPsmDataPreparation38.BuildImportSummaryPrepared(New List(Of Object())()).GetLength(0))
        Assert.ThrowsException(Of NullReferenceException)(Sub() UdfPsmDataPreparation38.BuildImportSummaryPrepared(Nothing))
        Assert.ThrowsException(Of IndexOutOfRangeException)(Sub() UdfPsmDataPreparation38.BuildImportSummaryPrepared(New List(Of Object()) From {New Object() {"short"}}))
    End Sub
End Class
