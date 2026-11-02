Option Explicit On
Option Strict On
Imports System
Imports System.Collections.Generic
Imports System.Globalization
Imports System.Linq
Imports System.Threading
Imports BESHStatNG.ExcelCommon
Imports Microsoft.VisualStudio.TestTools.UnitTesting

<TestClass>
Public Class AnovaWorkflow07AdapterTests
    Private Shared Function Groups() As MultiGroupsUnpairedData
        Return New MultiGroupsUnpairedData With {.X = {New Double() {7, 1, 3, 4}, New Double() {9, 2, 6, 8, 3}, New Double() {11, 4, 5}}, .varNames = {"Z'eta", "0", "alpha"}}
    End Function
    Private Shared Function Copy(data As MultiGroupsUnpairedData) As MultiGroupsUnpairedData
        Return New MultiGroupsUnpairedData With {.X = data.X.Select(Function(g) DirectCast(g.Clone(), Double())).ToArray(), .varNames = DirectCast(data.varNames.Clone(), String())}
    End Function
    Private Shared Sub SameModel(expected As ResultTableOutputModel, actual As ResultTableOutputModel)
        CollectionAssert.AreEqual(expected.Values, actual.Values)
        Assert.AreEqual(expected.HeaderTopRows, actual.HeaderTopRows)
        Assert.AreEqual(expected.HeaderLeftColumns, actual.HeaderLeftColumns)
        Assert.AreEqual(expected.FooterRows, actual.FooterRows)
        Assert.AreEqual(expected.TitleRows, actual.TitleRows)
        CollectionAssert.AreEqual(expected.PvalueColumns, actual.PvalueColumns)
        CollectionAssert.AreEqual(expected.PvalueCells.Select(Function(c) c.BodyRow).ToArray(), actual.PvalueCells.Select(Function(c) c.BodyRow).ToArray())
        CollectionAssert.AreEqual(expected.PvalueCells.Select(Function(c) c.BodyColumn).ToArray(), actual.PvalueCells.Select(Function(c) c.BodyColumn).ToArray())
        For r = 0 To expected.RowCount - 1
            For c = 0 To expected.ColumnCount - 1
                If expected.Values(r, c) IsNot Nothing Then Assert.AreEqual(expected.Values(r, c).GetType(), actual.Values(r, c).GetType())
            Next
        Next
    End Sub
    <DataTestMethod> <DataRow(0)> <DataRow(1)> <DataRow(2)> <DataRow(4)> <DataRow(8)> <DataRow(16)> <DataRow(31)>
    Public Sub ActualGroupedBridgeMatchesPreExtractionDispatcherAndDoesNotMutateOriginal(bits As Integer)
        Dim before = Groups(), after = Copy(before)
        Dim legacyOptions As New OneWayAnovaAnalysisOptions With {.IncludeWelch = (bits And 1) <> 0, .IncludeFisherLsd = (bits And 2) <> 0,
            .IncludeBonferroni = (bits And 4) <> 0, .IncludeTukeyKramer = (bits And 8) <> 0, .IncludeGamesHowell = (bits And 16) <> 0, .Alpha = 0.1}
        Dim expected = SharedAnalysisApplicationService.Run(New SpreadsheetAnalysisRequest(SpreadsheetAnalysisKind.OneWayAnova, ExcelDnaSharedApplicationAdapter.ToGroupedNumericData(before), legacyOptions))
        Dim result = WindowsAnovaWorkflowAdapter.AnalyzeGrouped(after, New AnovaGroupedWorkflowOptions(SpreadsheetAnalysisKind.OneWayAnova, 0.1,
            welch:=(bits And 1) <> 0, lsd:=(bits And 2) <> 0, bonferroni:=(bits And 4) <> 0, tukey:=(bits And 8) <> 0, gamesHowell:=(bits And 16) <> 0))
        Assert.AreEqual(expected.Tables.Count, result.LegacyAnalysis.Tables.Count)
        For t = 0 To expected.Tables.Count - 1
            SameModel(expected.Tables(t), result.LegacyAnalysis.Tables(t))
        Next
        For g = 0 To before.X.Length - 1
            CollectionAssert.AreEqual(before.X(g), after.X(g))
        Next
    End Sub

    Public Shared Function SupplementCases() As IEnumerable(Of Object())
        Return From kind In New SpreadsheetAnalysisKind() {SpreadsheetAnalysisKind.OneWayAnova, SpreadsheetAnalysisKind.KruskalWallis}
               From flags In Enumerable.Range(0, 8) Select New Object() {kind, flags}
    End Function
    <DataTestMethod> <DynamicData(NameOf(SupplementCases), DynamicDataSourceType.Method)>
    Public Sub RealSupplementBridgeMatchesFrozenHelpersAndBoxCalculations(kind As SpreadsheetAnalysisKind, flags As Integer)
        Dim before = Groups(), after = Copy(before)
        Dim options As New AnovaGroupedWorkflowOptions(kind, homogeneity:=If((flags And 1) <> 0, HomogeneityMethods.All, HomogeneityMethods.None), descriptive:=(flags And 2) <> 0, boxSummary:=(flags And 4) <> 0)
        Dim expected As New List(Of ResultTable)
        If kind = SpreadsheetAnalysisKind.OneWayAnova Then
            If (flags And 1) <> 0 Then expected.Add(LegacyAssumption06Characterization.ComputeVarianceHomogeneity(before, True, True, True, True))
            If (flags And 2) <> 0 Then expected.Add(LegacyAssumption06Characterization.ComputeDescriptiveStats(before))
        Else
            If (flags And 2) <> 0 Then expected.Add(LegacyAssumption06Characterization.ComputeDescriptiveStats(before))
            If (flags And 1) <> 0 Then expected.Add(LegacyAssumption06Characterization.ComputeVarianceHomogeneity(before, True, True, True, True))
        End If
        If (flags And 4) <> 0 Then
            Dim box = DirectCast(Activator.CreateInstance(GetType(graphics.BoxPlot), New Object() {before.X, before.varNames, Nothing, ""}), graphics.BoxPlot)
            box.Calculate() : box.CalcForPlotting() : expected.Add(box.wrapResults())
        End If
        Dim actual = WindowsAnovaWorkflowAdapter.PrepareSupplements(after, options)
        Assert.AreEqual(expected.Count, actual.Tables.Count)
        For t = 0 To expected.Count - 1
            SameModel(expected(t).ToOutputModel(), actual.Tables(t).ToOutputModel())
        Next
        Assert.AreEqual((flags And 4) <> 0, actual.Box IsNot Nothing)
        For g = 0 To before.X.Length - 1
            CollectionAssert.AreEqual(before.X(g), after.X(g))
        Next
    End Sub

    Public Shared Function RepeatedCases() As IEnumerable(Of Object())
        Return From bits In Enumerable.Range(0, 32) From culture In New String() {"", "sk-SK"} Select New Object() {bits, culture}
    End Function
    Private Shared Function RepeatedData() As MultiGroupsPairedData
        Return New MultiGroupsPairedData With {.X = New Double(,) {{1, 1.2, 1.1}, {0.9, 1.1, 1}, {1.1, 1.3, 1.25}, {1.05, 1.15, 1.2}, {0.95, 1.05, 1}, {1.2, 1.25, 1.3}}, .varNames = {"A", "B", "C"}}
    End Function
    <DataTestMethod> <DynamicData(NameOf(RepeatedCases), DynamicDataSourceType.Method)>
    Public Sub RealRepeatedBridgeMatchesFrozenSourceAllOptions(bits As Integer, culture As String)
        Dim saved = Thread.CurrentThread.CurrentCulture
        Try
            Thread.CurrentThread.CurrentCulture = CultureInfo.GetCultureInfo(culture)
            Dim before = RepeatedData(), after = RepeatedData()
            Dim expected = LegacyAnovaWorkflow07.Repeated(before, 0.1, (bits And 1) <> 0, (bits And 2) <> 0, (bits And 4) <> 0, (bits And 8) <> 0, (bits And 16) <> 0)
            Dim options As New RepeatedAnovaWorkflowOptions(0.1, greenhouseGeisser:=(bits And 1) <> 0, huynhFeldt:=(bits And 2) <> 0,
                tukey:=(bits And 4) <> 0, mauchly:=(bits And 8) <> 0, descriptive:=(bits And 16) <> 0)
            Dim actual = WindowsAnovaWorkflowAdapter.Repeated(after, options)
            Assert.AreEqual(expected.Count, actual.Tables.Count)
            For t = 0 To expected.Count - 1
                SameModel(expected(t).ToOutputModel(), actual.Tables(t).ToOutputModel())
            Next
            CollectionAssert.AreEqual(before.X, after.X)
        Finally
            Thread.CurrentThread.CurrentCulture = saved
        End Try
    End Sub

    <DataTestMethod> <DataRow(False, "")> <DataRow(True, "")> <DataRow(False, "sk-SK")> <DataRow(True, "sk-SK")>
    Public Sub RepeatedBoxRemainsHostComputationAndNoChartIsRendered(desc As Boolean, culture As String)
        Dim saved = Thread.CurrentThread.CurrentCulture
        Try
            Thread.CurrentThread.CurrentCulture = CultureInfo.GetCultureInfo(culture)
            Dim before = RepeatedData(), after = RepeatedData()
            Dim expected = LegacyAnovaWorkflow07.Repeated(before, 0.05, True, True, True, True, desc)
            Dim box = DirectCast(Activator.CreateInstance(GetType(graphics.BoxPlot), New Object() {before.X, before.varNames, Nothing, ""}), graphics.BoxPlot)
            box.Calculate() : box.CalcForPlotting() : expected.Add(box.wrapResults())
            Dim actual = WindowsAnovaWorkflowAdapter.Repeated(after, New RepeatedAnovaWorkflowOptions(0.05, True, True, True, True, desc, True))
            Assert.IsNotNull(actual.Box)
            For t = 0 To expected.Count - 1
                SameModel(expected(t).ToOutputModel(), actual.Tables(t).ToOutputModel())
            Next
        Finally
            Thread.CurrentThread.CurrentCulture = saved
        End Try
    End Sub

    Private Shared Function NestedData(Optional unbalanced As Boolean = False) As MultiGroupsPairedDataObj
        Dim rows As New List(Of Object()) From {New Object() {"B", "b2", 10.0}, New Object() {"A", "a1", 1.0}, New Object() {"A", "a2", 7.0},
            New Object() {"B", "b1", 2.0}, New Object() {"A", "a1", 3.0}, New Object() {"B", "b2", 8.0}, New Object() {"A", "a2", 5.0}, New Object() {"B", "b1", 4.0}}
        If unbalanced Then rows.Add(New Object() {"A", "a1", 2.0})
        Dim matrix(rows.Count - 1, 2) As Object
        For r = 0 To rows.Count - 1
            For c = 0 To 2
                matrix(r, c) = rows(r)(c)
            Next
        Next
        Return New MultiGroupsPairedDataObj With {.X = matrix, .varNames = {"Parent", "Child", "Response"}}
    End Function
    <DataTestMethod> <DataRow(False, "")> <DataRow(True, "")> <DataRow(False, "sk-SK")> <DataRow(True, "sk-SK")>
    Public Sub NestedTablesBannerSeparatorsAndSortingMatchSource(unbalanced As Boolean, culture As String)
        Dim saved = Thread.CurrentThread.CurrentCulture
        Try
            Thread.CurrentThread.CurrentCulture = CultureInfo.GetCultureInfo(culture)
            Dim before = NestedData(unbalanced), after = NestedData(unbalanced)
            Dim expected = LegacyAnovaWorkflow07.Nested(before), actual = WindowsAnovaWorkflowAdapter.Nested(after)
            Assert.AreEqual(expected.Item2, actual.BalancedDesign.Value)
            For t = 0 To expected.Item1.Count - 1
                SameModel(expected.Item1(t).ToOutputModel(), actual.ToLegacyTables()(t).ToOutputModel())
            Next
            CollectionAssert.AreEqual(before.X, after.X)
            Assert.AreEqual("Two-Way Nested ANOVA", actual.Banner(0, 0))
            Assert.AreEqual("Balanced design = " & expected.Item2.ToString(), actual.Banner(1, 0))
            CollectionAssert.AreEqual(New Object(,) {{"", ""}, {"Satterthwaite approximation", ""}}, actual.Separators(0))
        Finally
            Thread.CurrentThread.CurrentCulture = saved
        End Try
    End Sub

    <TestMethod>
    Public Sub ActualBlockProducerKeepsGapsAndFailureAtomicity()
        Dim source As New DataObj
        source.DataImportRawMatrix(New Object(,) {{1, 3, 2}, {Nothing, 2, 9}, {4, 7, 2}, {Nothing, Nothing, Nothing},
            {Nothing, 4, 5}, {7, Nothing, 9}, {6, 3, 7}}, {"A", "B", "C"}, firstSourceRow:=101)
        Dim errorText As String = ""
        Dim data = WindowsPairedInputAdapter.SelectBlocks(source, errorText)
        Assert.IsNotNull(data, errorText)
        Dim evidence = WindowsPairedInputAdapter.GetBlockEvidence(data)
        CollectionAssert.AreEqual(New Integer() {101, 103, 107}, evidence.Blocks.Select(Function(b) b(0).WorksheetRow).ToArray())
        Dim result = WindowsAnovaWorkflowAdapter.Repeated(data, New RepeatedAnovaWorkflowOptions())
        CollectionAssert.AreEqual(New Integer() {101, 103, 107}, result.Report.Blocks.Blocks.Select(Function(b) b(0).WorksheetRow).ToArray())
        data.X(0, 0) = 1000 'negative control: never attach original rows to changed values
        Dim writes = 0
        Try
            Dim invalid = WindowsAnovaWorkflowAdapter.Repeated(data, New RepeatedAnovaWorkflowOptions())
            writes += invalid.Tables.Count
        Catch ex As SpreadsheetInputValidationException
            Assert.IsTrue(ex.Report.HasErrors)
        End Try
        Assert.AreEqual(0, writes)
    End Sub

    <TestMethod>
    Public Sub ActualNestedProducerRetainsOriginalRowMapWhileCoreSortsSeparateScratch()
        Dim raw = NestedData().X, source As New DataObj
        source.DataImportRawMatrix(raw, {"Parent", "Child", "Response"}, firstSourceRow:=201, CharCols:=1)
        Dim errorText As String = ""
        Dim data = WindowsPairedInputAdapter.SelectNested(source, errorText)
        Assert.IsNotNull(data, errorText)
        Dim original = DirectCast(data.X.Clone(), Object(,))
        Dim report = WindowsAnovaWorkflowAdapter.Nested(data)
        Assert.IsTrue(report.Success)
        CollectionAssert.AreEqual(Enumerable.Range(201, 8).ToArray(), report.Nested.Responses.Blocks.Select(Function(b) b(0).WorksheetRow).ToArray())
        For r = 0 To original.GetLength(0) - 1
            Assert.AreEqual(original(r, 0), report.Nested.GroupLabels(r))
            Assert.AreEqual(CDbl(original(r, 2)), report.Nested.Responses.Blocks(r)(0).Value)
        Next
        Assert.AreEqual("A", data.X(0, 0)) 'legacy Core sorting replayed; row map explicitly remains original
    End Sub

    <TestMethod>
    Public Sub ActualUnivariateProducerPreservesOriginalRowsAndDoesNotInventIdentity()
        Dim a As New DataObj, b As New DataObj
        a.DataImportRawMatrix(New Object(,) {{7}, {Nothing}, {1}, {3}, {4}}, {"second"}, firstSourceRow:=101)
        b.DataImportRawMatrix(New Object(,) {{2}, {4}, {5}, {8}, {9}}, {"first"}, firstSourceRow:=201)
        Dim errorText As String = ""
        Dim data = WindowsUnivariateInputAdapter.SelectColumns({a, b}, errorText)
        Dim report = WindowsAnovaWorkflowAdapter.AnalyzeGrouped(data, New AnovaGroupedWorkflowOptions(SpreadsheetAnalysisKind.OneWayAnova))
        CollectionAssert.AreEqual(New Integer() {101, 103, 104, 105, 201, 202, 203, 204, 205}, report.Input.Sources.Select(Function(s) s.WorksheetRow).ToArray())
        Assert.IsTrue(report.Input.Sources.All(Function(s) s.WorkbookId = "" AndAlso s.WorksheetId = ""))
        WindowsAnovaWorkflowAdapter.PrepareSupplements(data, New AnovaGroupedWorkflowOptions(SpreadsheetAnalysisKind.OneWayAnova, descriptive:=True))
        CollectionAssert.AreEqual(New Double() {7, 1, 3, 4}, report.Input.Groups(0))
        CollectionAssert.AreEqual(New Double() {1, 3, 4, 7}, data.X(0))
    End Sub

    <TestMethod>
    Public Sub CancelledAndInvalidAdaptersNeverReturnWritableResults()
        Assert.ThrowsException(Of OperationCanceledException)(Sub() WindowsAnovaWorkflowAdapter.AnalyzeGrouped(Groups(), New AnovaGroupedWorkflowOptions(SpreadsheetAnalysisKind.OneWayAnova), New CancellationToken(True)))
        Assert.ThrowsException(Of OperationCanceledException)(Sub() WindowsAnovaWorkflowAdapter.Repeated(RepeatedData(), New RepeatedAnovaWorkflowOptions(), New CancellationToken(True)))
        Assert.ThrowsException(Of OperationCanceledException)(Sub() WindowsAnovaWorkflowAdapter.Nested(NestedData(), New CancellationToken(True)))
        Assert.ThrowsException(Of ArgumentException)(Sub() WindowsAnovaWorkflowAdapter.Repeated(Nothing, New RepeatedAnovaWorkflowOptions()))
    End Sub

    <DataTestMethod> <DataRow("")> <DataRow("sk-SK")>
    Public Sub NumericTextColumnKeepsRawEvidenceAndSurvivingFrameworkCoercion(culture As String)
        Dim saved = Thread.CurrentThread.CurrentCulture
        Try
            Thread.CurrentThread.CurrentCulture = CultureInfo.GetCultureInfo(culture)
            Dim raw = NestedData().X
            raw(0, 2) = 1.2345678901234567
            raw(1, 2) = 3.0.ToString(CultureInfo.CurrentCulture)
            Dim source As New DataObj
            source.DataImportRawMatrix(raw, {"Parent", "Child", "Response"}, firstSourceRow:=301, CharCols:=1)
            Dim errorText As String = ""
            Dim data = WindowsPairedInputAdapter.SelectNested(source, errorText)
            Assert.IsNotNull(data, errorText)
            Dim original = WindowsPairedInputAdapter.GetNestedEvidence(data)
            Dim coreInput = WindowsPairedInputAdapter.GetNestedCoreInput(data)
            Dim rawValue = CDbl(raw(0, 2)), surviving = Convert.ToDouble(data.X(0, 2), CultureInfo.CurrentCulture)
            Assert.AreEqual(rawValue, original.Responses.Blocks(0)(0).Value)
            Assert.AreEqual(surviving, coreInput.Responses.Blocks(0)(0).Value)
            Assert.AreNotEqual(rawValue, surviving, "This net48 fixture must exercise the existing 15-digit stringification.")
            Assert.AreEqual(301, coreInput.Responses.Blocks(0)(0).WorksheetRow)
            Dim before As New MultiGroupsPairedDataObj With {.X = DirectCast(data.X.Clone(), Object(,)), .varNames = data.varNames}
            Dim expected = LegacyAnovaWorkflow07.Nested(before), result = WindowsAnovaWorkflowAdapter.Nested(data)
            Assert.AreEqual(rawValue, result.OriginalNestedInput.Responses.Blocks(0)(0).Value)
            Assert.AreEqual(surviving, result.Nested.Responses.Blocks(0)(0).Value)
            For t = 0 To expected.Item1.Count - 1
                SameModel(expected.Item1(t).ToOutputModel(), result.ToLegacyTables()(t).ToOutputModel())
            Next
        Finally
            Thread.CurrentThread.CurrentCulture = saved
        End Try
    End Sub
End Class
