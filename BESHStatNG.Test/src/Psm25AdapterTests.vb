Option Explicit On
Option Strict On
Option Infer On
Imports System
Imports System.Collections.Generic
Imports System.Globalization
Imports System.Linq
Imports System.Reflection
Imports BESHStatNG
Imports BESHStatNG.CausalInference
Imports BESHStatNG.ExcelCommon
Imports Microsoft.VisualStudio.TestTools.UnitTesting

<TestClass>
Public Class Psm25AdapterTests
    Private Shared Function Calls(method As MethodInfo) As List(Of MethodBase)
        Dim bytes = method.GetMethodBody().GetILAsByteArray(), values As New List(Of MethodBase)
        For offset = 0 To bytes.Length - 5
            If bytes(offset) <> &H28 AndAlso bytes(offset) <> &H6F AndAlso bytes(offset) <> &H73 Then Continue For
            Try
                values.Add(method.Module.ResolveMethod(BitConverter.ToInt32(bytes, offset + 1)))
            Catch ex As ArgumentException
            End Try
        Next
        Return values
    End Function
    <TestMethod>
    Public Sub CompiledRunUsesCommonEffectiveOptionsAndExactlyOneCoreFit()
        Dim flags = BindingFlags.Instance Or BindingFlags.NonPublic
        Dim owner = GetType(Ui20PropensityScoreMatching)
        Dim run = Calls(owner.GetMethod("RunPSM", flags))
        Assert.AreEqual(1, run.Where(Function(m) m.DeclaringType = GetType(PsmComprehensiveBackend) AndAlso m.Name = "Fit").Count())
        Assert.IsTrue(run.Any(Function(m) m.Name = "BuildFitOptions"))
        Dim build = Calls(owner.GetMethod("BuildFitOptions", flags))
        Assert.IsTrue(build.Any(Function(m) m.DeclaringType = GetType(PsmRequestSelection) AndAlso m.Name = "CreateEffectiveOptions"))
        Dim getData = Calls(owner.GetMethod("GetData", flags))
        Assert.IsTrue(getData.Any(Function(m) m.DeclaringType = GetType(psmData) AndAlso m.Name = "get_AlignedAxis"))
        Dim import = Calls(GetType(psmData).GetMethod("BuildPsmInput", flags))
        Assert.IsTrue(import.Any(Function(m) m.DeclaringType = GetType(PsmAlignedInputAxis) AndAlso m.IsConstructor))
    End Sub
    <TestMethod>
    Public Sub CompiledWriterUsesPhasePlansSourceRowsAndOneSharedLoveProjection()
        Dim owner = GetType(Ui20PropensityScoreMatching), flags = BindingFlags.NonPublic Or BindingFlags.Instance
        Dim writer = Calls(owner.GetMethod("WritePsmResults", flags))
        For Each name In {"Plan", "DiagnosticSheets", "LoveSheets", "MatchSheets", "CemSheets"}
            Assert.IsTrue(writer.Any(Function(m) m.DeclaringType = GetType(PsmReportSelectionPolicy) AndAlso m.Name = name), name)
        Next
        Assert.IsTrue(writer.Any(Function(m) m.DeclaringType = GetType(PsmReportSheet) AndAlso m.Name = "BuildTables"))
        Assert.IsTrue(writer.Any(Function(m) m.DeclaringType = GetType(PsmAlignedInputAxis) AndAlso m.Name = "SourceRowIds"))
        Dim love = Calls(owner.GetMethod("WriteLovePlotSheetIfAvailable", flags))
        Assert.AreEqual(1, love.Where(Function(m) m.DeclaringType = GetType(PsmLovePlotProjection) AndAlso m.IsConstructor).Count())
        For Each name In {"BuildProjectedDataTable", "AddProjectedChart"}
            Assert.IsTrue(love.Any(Function(m) m.DeclaringType = GetType(graphics.PsmLovePlotExcel) AndAlso m.Name = name))
        Next
        For Each name In {"BuildPlotDataTable", "AddChart"}
            Dim renderer = Calls(GetType(graphics.PsmLovePlotExcel).GetMethod(name, BindingFlags.Public Or BindingFlags.Static))
            Assert.IsTrue(renderer.Any(Function(m) m.DeclaringType = GetType(PsmLovePlotProjection) AndAlso m.IsConstructor))
        Next
    End Sub
    <DataTestMethod><DataRow(0)><DataRow(1)><DataRow(2)><DataRow(3)><DataRow(4)><DataRow(5)><DataRow(6)><DataRow(7)>
    Public Sub ActualHostSourceTableFacadeMatchesFrozenWindowsWithoutExcel(id As Integer)
        Dim rows = Psm25Fixture.LoveRows(id)
        Assert.AreEqual(Spc23Fixture.Hash(Psm25Legacy.BuildPlotDataTable(rows)), Spc23Fixture.Hash(graphics.PsmLovePlotExcel.BuildPlotDataTable(rows)))
        GetType(graphics.PsmLovePlotExcel).GetMethod("AddChart").Invoke(Nothing, New Object() {Nothing, rows, 420.0R, 20.0R, 720.0R, 420.0R})
    End Sub
    <DataTestMethod><DataRow(1, False)><DataRow(20, False)><DataRow(200, False)><DataRow(20, True)>
    Public Sub RawImporterKeepsAlignmentAndPhysicalRows(first As Integer, formula As Boolean)
        Dim spec As New PsmDataRawMatrixSpec With {
            .ModelRawInput = New Object(,) {{1.0R, 1.2R}, {0.0R, 1.3R}, {1.0R, 1.4R}, {0.0R, 1.5R}, {1.0R, 1.6R}, {0.0R, 1.7R}},
            .ModelVariableNames = {"t", "x"}, .TreatmentKey = "t", .SelectedCovariateKeys = New List(Of String) From {"x"},
            .EffectItems = New String() {"x"}, .TermSpecs = New Dictionary(Of String, TermSpec)(StringComparer.Ordinal),
            .OutcomeRawInput = New Object(,) {{10.0R}, {7.0R}, {Nothing}, {6.0R}, {9.0R}, {8.0R}}, .OutcomeVariableNames = {"y"},
            .SuppliedScoreRawInput = New Object(,) {{0.2R}, {0.21R}, {0.3R}, {0.31R}, {0.4R}, {0.41R}}, .SuppliedScoreVariableNames = {"ps"},
            .IdRawInput = New Object(,) {{"duplicate"}, {"duplicate"}, {"third"}, {"fourth"}, {"fifth"}, {"sixth"}}, .IdVariableNames = {"id"},
            .ScoreMethod = PsmScoreMethod.Supplied, .FirstSourceRow = first}
        If formula Then spec.FormulaText = "'x'"
        Dim imported As New psmData()
        imported.DataImportFromRawMatrices(spec)
        Assert.IsFalse(imported.bZeroValid)
        Assert.AreEqual(1, imported.DroppedRowsDuringAlignment)
        CollectionAssert.AreEqual({first, first + 1, first + 3, first + 4, first + 5}, imported.RowIds)
        CollectionAssert.AreEqual(imported.RowIds, imported.AlignedAxis.SourceRowIds())
        Assert.AreEqual(5, imported.Input.RowCount)
        Assert.AreEqual("duplicate", imported.AlignedAxis.Rows(0).Id)
        Assert.AreEqual("duplicate", imported.AlignedAxis.Rows(1).Id)
        CollectionAssert.AreEqual(imported.Input.CovariateNames, imported.AlignedAxis.ExpandedCovariates.ToArray())
        Assert.AreEqual(1, imported.AlignedAxis.DroppedRows)
    End Sub
    <TestMethod>
    Public Sub ExactLabelRetainsOriginalColumnOrderNothingBlankUnicodeAndDelimiter()
        Dim method = GetType(psmData).GetMethod("BuildExactLabel", BindingFlags.NonPublic Or BindingFlags.Static)
        Dim data As Object(,) = {{"Ω", Nothing, "B | C"}, {"", "", ""}}
        Assert.AreEqual("Ω |  | B | C", CStr(method.Invoke(Nothing, New Object() {data, 0})))
        Assert.AreEqual(" |  | ", CStr(method.Invoke(Nothing, New Object() {data, 1})))
    End Sub
    <DataTestMethod><DataRow("")><DataRow("sk-SK")>
    Public Sub WindowsCultureParsingFeedsTypedOptionsUnchanged(culture As String)
        Dim saved = CultureInfo.CurrentCulture
        Try
            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo(culture)
            Dim text = 0.2R.ToString(CultureInfo.CurrentCulture)
            Dim parsed = UIprocedures.ParseUiDouble(text, "caliper")
            Dim options = Psm25Fixture.Options(0, 0, 0)
            options.StandardOptions.Caliper = parsed : options.StandardOptions.CaliperScale = PsmCaliperScale.RawPropensityScore
            Assert.AreEqual(0.2R, New PsmRequestSelection(options).CreateEffectiveOptions().StandardOptions.Caliper)
            Assert.AreEqual(5, UIprocedures.ParseUiInteger("5", "strata"))
            Assert.ThrowsException(Of FormatException)(Sub() UIprocedures.ParseUiInteger("1.2", "strata"))
        Finally
            CultureInfo.CurrentCulture = saved
        End Try
    End Sub

    <DataTestMethod><DataRow(False)><DataRow(True)>
    Public Sub RawCategoricalAndInteractionDesignRemainsHostOwned(useFormula As Boolean)
        Dim spec As New PsmDataRawMatrixSpec With {
            .ModelRawInput = New Object(,) {{1.0R, 1.2R, 1.0R}, {0.0R, 1.3R, 2.0R}, {1.0R, 1.4R, 1.0R}, {0.0R, 1.5R, 2.0R}, {1.0R, 1.6R, 1.0R}, {0.0R, 1.7R, 2.0R}},
            .ModelVariableNames = {"t", "x", "site"}, .TreatmentKey = "t", .SelectedCovariateKeys = New List(Of String) From {"x", "site"},
            .TermSpecs = New Dictionary(Of String, TermSpec)(StringComparer.Ordinal),
            .OutcomeRawInput = New Object(,) {{10.0R}, {7.0R}, {9.0R}, {6.0R}, {9.0R}, {8.0R}}, .OutcomeVariableNames = {"y"},
            .ExactGroupRawInput = New Object(,) {{"Ω", "A"}, {"Ω", "A"}, {"Ω", "B"}, {"Ω", "B"}, {"Ω", "A"}, {"Ω", "A"}}, .ExactGroupVariableNames = {"site", "other"}, .FirstSourceRow = 77}
        If useFormula Then spec.FormulaText = "'x' + factor('site') + 'x':factor('site')"
        Dim imported As New psmData
        imported.DataImportFromRawMatrices(spec)
        Assert.AreEqual(6, imported.Input.RowCount)
        Assert.AreEqual(If(useFormula, 3, 2), imported.Input.CovariateCount)
        Assert.AreEqual("77", imported.Input.Ids(0))
        Assert.AreEqual("Ω | A", imported.AlignedAxis.Rows(0).ExactLabel)
        Assert.IsTrue(imported.AlignedAxis.HasExactGroups)
        Assert.IsFalse(imported.AlignedAxis.HasSuppliedScores)
        CollectionAssert.AreEqual(imported.Input.CovariateNames, imported.AlignedAxis.ExpandedCovariates.ToArray())
        For i = 0 To imported.Input.RowCount - 1
            Assert.AreEqual(imported.RowIds(i), imported.AlignedAxis.Rows(i).SourceRow.Value)
            For j = 0 To imported.Input.CovariateCount - 1
                Assert.AreEqual(imported.ExpandedData(i, j + 1), imported.Input.Covariates(i, j))
            Next
        Next
    End Sub

    <TestMethod>
    Public Sub RawTextModelCovariatesRetainExistingNumericImportRejection()
        Dim spec As New PsmDataRawMatrixSpec With {.ModelRawInput = New Object(,) {{1.0R, "A"}, {0.0R, "B"}}, .ModelVariableNames = {"t", "site"}, .TreatmentKey = "t", .SelectedCovariateKeys = New List(Of String) From {"site"}, .TermSpecs = New Dictionary(Of String, TermSpec), .OutcomeRawInput = New Object(,) {{10.0R}, {7.0R}}, .OutcomeVariableNames = {"y"}}
        Dim imported As New psmData
        imported.DataImportFromRawMatrices(spec)
        Assert.IsTrue(imported.bZeroValid)
        Assert.IsNull(imported.Input)
        Assert.IsNull(imported.AlignedAxis)
    End Sub
End Class
