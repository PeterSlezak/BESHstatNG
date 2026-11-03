Option Explicit On
Option Strict On
Imports System
Imports System.Collections.Generic
Imports System.Globalization
Imports System.Linq
Imports BESHStatNG
Imports BESHStatNG.ExcelCommon
Imports BESHStatNG.regression
Imports ExcelDna.Integration
Imports Microsoft.VisualStudio.TestTools.UnitTesting

<TestClass>
Public Class Regression09AdapterTests
    Private NotInheritable Class MemoryWriter
        Inherits ResultTableWriterBase
        Public ReadOnly Blocks As New List(Of ResultTableOutputBlock)
        Protected Overrides Sub WriteOutputBlock(block As ResultTableOutputBlock)
            Blocks.Add(block)
        End Sub
    End Class
    Private Shared Function Model(f As Regression09Fixture, intercept As Boolean) As RegressionEffectModel
        Return RegressionEffectModel.FromProjection(f.Names.Skip(1), New Dictionary(Of String, TermSpec), intercept)
    End Function
    Private Shared Function Raw(f As Regression09Fixture, includeVectors As Boolean) As glmData
        Return New glmData With {.RowIds = DirectCast(f.Ids.Clone(), Integer()), .bOffset = includeVectors, .bWeights = includeVectors,
            .OffsetData = f.Offset, .WeightData = f.Weights, .OffsetVarName = "offset", .WeightVarName = "weight"}
    End Function

    <DataTestMethod><DataRow("LM", False)><DataRow("LM", True)><DataRow("GLM", False)><DataRow("GLM", True)><DataRow("NB2", False)><DataRow("NB2", True)><DataRow("ZIP", False)><DataRow("ZIP", True)><DataRow("Ordinal", False)><DataRow("Ordinal", True)><DataRow("Multinomial", False)><DataRow("Multinomial", True)>
    Public Sub ShippingAdapterSixPathsAndRealWriterPayloads(kindName As String, alternate As Boolean)
        Dim prior = CultureInfo.CurrentCulture
        Try
            CultureInfo.CurrentCulture = CultureInfo.InvariantCulture
            Dim f = Regression09Fixture.Fit(kindName, alternate), kind = Regression09PolicyTests.KindFromName(kindName)
            Dim options = Regression09PolicyTests.Options(kind)
            options.Residuals = f.Residual IsNot Nothing
            If kindName = "GLM" AndAlso alternate Then options.Family = RegressionFamily.Binomial : options.Link = RegressionLink.Logit
            Dim rawData = Raw(f, kindName <> "LM" AndAlso kindName <> "ZIP")
            If kindName = "LM" AndAlso alternate Then rawData.bWeights = True
            If kindName = "ZIP" Then rawData.bOffset = True
            Dim z As Regression09Fixture = Nothing
            If kindName = "ZIP" Then z = Regression09Fixture.Load("zip_dataset_basic_zero.csv")
            Dim request = WindowsRegressionRequestAdapter.Capture(kind, "y", rawData, f.Data, f.Names, Model(f, Not (kindName = "LM" AndAlso alternate)), options,
                logisticRaw:=If(z Is Nothing, Nothing, Raw(z, False)), logisticExpanded:=If(z Is Nothing, Nothing, z.Data),
                logisticNames:=If(z Is Nothing, Nothing, z.Names), logisticEffects:=If(z Is Nothing, Nothing, Model(z, Not alternate)))
            Assert.IsTrue(WindowsRegressionRequestAdapter.Validate(request).IsValid, String.Join(";", WindowsRegressionRequestAdapter.Validate(request).Diagnostics.Select(Function(d) d.Message)))
            Dim plan = WindowsRegressionRequestAdapter.Report(request, f.Tables, f.Fitted, f.Residual, f.Converged, categories:=f.Categories, fittedRowIds:=f.Ids)
            Regression09PolicyTests.AssertTables(f.Tables, plan.Document)
            CollectionAssert.AreEqual(f.Ids, plan.Rows.RowIds)
            If f.Categories IsNot Nothing Then CollectionAssert.AreEqual(f.Categories, plan.Rows.Categories)
            ' The shipping writer base sees the same matrix for old vector/tall and new aligned matrix.
            If f.Fitted.GetLength(1) = 1 Then
                Dim vector = Enumerable.Range(0, f.Fitted.GetLength(0)).Select(Function(i) f.Fitted(i, 0)).ToArray()
                Dim oldWriter As New MemoryWriter, newWriter As New MemoryWriter
                oldWriter.write(vector, True) : newWriter.write(plan.Rows.Fitted)
                SameBlock(oldWriter.Blocks.Single(), newWriter.Blocks.Single())
            End If
            If f.Residual IsNot Nothing Then
                Dim oldWriter As New MemoryWriter, newWriter As New MemoryWriter
                oldWriter.write(f.Residual) : newWriter.write(plan.Rows.Residuals)
                SameBlock(oldWriter.Blocks.Single(), newWriter.Blocks.Single())
            End If
        Finally
            CultureInfo.CurrentCulture = prior
        End Try
    End Sub

    Private Shared Sub SameBlock(a As ResultTableOutputBlock, b As ResultTableOutputBlock)
        Assert.AreEqual(a.StartRow, b.StartRow) : Assert.AreEqual(a.StartColumn, b.StartColumn)
        Assert.AreEqual(a.Model.RowCount, b.Model.RowCount) : Assert.AreEqual(a.Model.ColumnCount, b.Model.ColumnCount)
        CollectionAssert.AreEqual(a.Model.Values.Cast(Of Object)().ToArray(), b.Model.Values.Cast(Of Object)().ToArray())
    End Sub

    <TestMethod>
    Public Sub LegacyMissingSentinelAndExcelNaStayAtHostBoundary()
        Dim missing = BinaryClassificationReporting.BuildThresholdTableUdfOutput(Nothing)
        Assert.IsTrue(BinaryClassificationReporting.IsMissingOutputValue(missing))
        Assert.AreEqual(ExcelError.ExcelErrorNA, UDFhelpers.ConvertBinaryClassificationOutputForExcel(missing))
        Dim summary = BinaryClassificationReporting.ComputeBinarySummary({0.0, 0}, {0.2, 0.1})
        Dim spill = DirectCast(BinaryClassificationReporting.BuildBinaryCrosstabUdfOutput(summary), Object(,))
        Assert.IsTrue(spill.Cast(Of Object)().Any(Function(v) BinaryClassificationReporting.IsMissingOutputValue(v)))
        Dim converted = DirectCast(UDFhelpers.ConvertBinaryClassificationOutputForExcel(spill), Object(,))
        Assert.IsTrue(converted.Cast(Of Object)().Any(Function(v) TypeOf v Is ExcelError))
        Dim o As New RegressionOptions With {.Family = RegressionFamily.Binomial, .Classification = True, .Brier = True}
        Dim report = BinaryClassificationReporting.ComposeGlmReport({0.0, 0}, {0.2, 0.1}, Nothing, o, WorksheetFunctions.ComputeWeightedEventRate({0.0, 0}, Nothing))
        Assert.IsFalse(report.Document.Tables.SelectMany(Function(t) t.Model.Values.Cast(Of Object)()).Any(Function(v) TypeOf v Is ExcelError OrElse BinaryClassificationReporting.IsMissingOutputValue(v)))
        Assert.ThrowsException(Of ArgumentException)(Sub() GC.KeepAlive(New RegressionRowOutput({11}, Nothing, New Object(,) {{"header"}, {ExcelError.ExcelErrorNA}})))
    End Sub

    <TestMethod>
    Public Sub ImportOffsetBeforeWeightsAndExcludedRowTransportRemainAligned()
        Dim rawData As New glmData With {.bOffset = True, .bWeights = True}
        rawData.DataImportRawMatrix(New Object(,) {{1, 2, 0.1, 1}, {2, Nothing, 0.2, 2}, {3, 4, 0.3, 3}}, {"y", "x", "offset", "weight"}, firstSourceRow:=47)
        Dim expanded = New Double(,) {{1, 2}, {3, 4}}
        Dim request = WindowsRegressionRequestAdapter.Capture(RegressionAnalysisKind.GeneralizedLinearModel, "y", rawData, expanded, {"y", "x"}, Regression09PolicyTests.Effects(), Regression09PolicyTests.Options(RegressionAnalysisKind.GeneralizedLinearModel))
        Assert.IsTrue(WindowsRegressionRequestAdapter.Validate(request).IsValid)
        CollectionAssert.AreEqual({47, 49}, request.Primary.RowIds)
        CollectionAssert.AreEqual({0.1, 0.3}, request.Offset.Values)
        CollectionAssert.AreEqual({1.0, 3.0}, request.Weights.Values)
        Assert.AreEqual("offset", request.Offset.Key) : Assert.AreEqual("weight", request.Weights.Key)
    End Sub

    <TestMethod>
    Public Sub PhysicalImporterProvenanceReusesAcceptedTransportWithoutCom()
        Dim table = ExcelDnaDataImporter.ImportPhysicalSelection("'Data'!A17:B19", {"A17:B19"},
            Function(addresses)
                Dim selection As New SpreadsheetRangeSelection With {.SelectionId = "regression09-fixture"}
                selection.Ranges.Add(SpreadsheetRangeReference.FromAddress("A17:B19", "Data", "Fixture.xlsx"))
                Return ExcelDnaSharedApplicationAdapter.SnapshotSelection(selection,
                    Function(reference) ExcelDnaSharedApplicationAdapter.SnapshotValues(reference, New Object(,) {{1, 2}, {2, Nothing}, {3, 4}}, Function(r, c) "General", Nothing, SpreadsheetDateSystem.Excel1900))
            End Function)
        Dim rawData As New glmData
        rawData.LoadCoreDataTable(table, cloneTable:=False)
        Dim request = WindowsRegressionRequestAdapter.Capture(RegressionAnalysisKind.LinearModel, "y", rawData, New Double(,) {{1, 2}, {3, 4}}, {"y", "x"}, Regression09PolicyTests.Effects(), New RegressionOptions)
        Assert.IsTrue(WindowsRegressionRequestAdapter.Validate(request).IsValid)
        Assert.AreEqual(2, request.Primary.ResponseSources.Count)
        Assert.AreEqual(17, request.Primary.ResponseSources(0).WorksheetRow)
        Assert.AreEqual(19, request.Primary.ResponseSources(1).WorksheetRow)
        Assert.AreEqual(1, request.Primary.ResponseSources(0).WorksheetColumn)
    End Sub

    <TestMethod>
    Public Sub ZipClearReaddAndFailedValidationCannotMutateOtherModel()
        Dim primary = Regression09PolicyTests.Effects(), logistic = RegressionEffectAuthoringPolicy.Remove(Regression09PolicyTests.Effects(False), {}, clear:=True).Model
        Dim data As New glmData With {.RowIds = {5, 9}}, x = New Double(,) {{0, 1}, {2, 3}}
        Dim before = RegressionEffectSerialization.Serialize(primary)
        Dim failed = WindowsRegressionRequestAdapter.Capture(RegressionAnalysisKind.ZeroInflatedPoisson, "y", data, x, {"y", "x"}, primary, New RegressionOptions,
            initial:={0.0, 0}, logisticRaw:=data, logisticExpanded:=New Double(,) {{0}, {2}}, logisticNames:={"y"}, logisticEffects:=logistic, logisticInitial:={})
        Assert.IsFalse(WindowsRegressionRequestAdapter.Validate(failed).IsValid)
        logistic = RegressionEffectAuthoringPolicy.Apply({"z"}, {0}, logistic, RegressionEffectOperation.ContinuousMain).Model
        Dim current = WindowsRegressionRequestAdapter.Capture(RegressionAnalysisKind.ZeroInflatedPoisson, "y", data, x, {"y", "x"}, primary, New RegressionOptions,
            initial:={0.0, 0}, logisticRaw:=data, logisticExpanded:=x, logisticNames:={"y", "z"}, logisticEffects:=logistic, logisticInitial:={0.0})
        Assert.IsTrue(WindowsRegressionRequestAdapter.Validate(current).IsValid)
        Assert.AreEqual(before, RegressionEffectSerialization.Serialize(primary))
        Assert.AreEqual("x", current.Primary.Effects.Effects.Single().Key) : Assert.AreEqual("z", current.Logistic.Effects.Effects.Single().Key)
    End Sub
End Class
