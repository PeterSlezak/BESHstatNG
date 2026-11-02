Option Explicit On
Option Strict On
Option Infer On

Imports System.Collections.Generic
Imports System.Globalization
Imports System.Linq
Imports BESHStatNG.ExcelCommon
Imports Microsoft.VisualStudio.TestTools.UnitTesting

<TestClass>
Public Class UnivariateSelectionAdapterTests
    Private Shared Function Imported(raw As Object(,), names As String(), Optional firstRow As Integer = 20,
                                     Optional byId As Boolean = False) As DataObj
        Dim result As New DataObj()
        result.DataImportRawMatrix(raw, names, firstSourceRow:=firstRow, CharCols:=If(byId, 0, -1))
        Return result
    End Function

    ' Characterization from main 9e2326d UibyID.getDataMultipleGroups, before extraction.
    ' Test-only frozen oracle: no independent numerical/scientific claim.
    Private Shared Function BeforeColumns(columns As DataObj(), ByRef errorText As String) As MultiGroupsUnpairedData
        Dim groups As New List(Of Double())()
        Dim names As New List(Of String)()
        For Each column In columns
            If column.FinalData IsNot Nothing Then
                groups.Add(Matrix.GetColumnFrom2Darray(column.DataDbl, 0))
                names.Add(column.varNames(0))
            End If
        Next
        If groups.Count = 0 Then
            errorText = "No valid data"
            Return Nothing
        End If
        Return New MultiGroupsUnpairedData With {.X = groups.ToArray(), .varNames = names.ToArray()}
    End Function

    Private Shared Function BeforeById(imported As DataObj) As MultiGroupsUnpairedData
        Return New MultiGroupsUnpairedData With {
            .X = imported.DataByID2ByColumn(),
            .varNames = Global.BESHStatNG.DataManagement.ArrayUtilities.Array2strArray(
                Matrix.GetColumnFrom2Darray(imported.FinalData, 0).Distinct().ToArray())}
    End Function

    Private Shared Sub Same(before As MultiGroupsUnpairedData, after As MultiGroupsUnpairedData)
        CollectionAssert.AreEqual(before.varNames, after.varNames)
        Assert.AreEqual(before.X.Length, after.X.Length)
        For g As Integer = 0 To before.X.Length - 1
            CollectionAssert.AreEqual(before.X(g), after.X(g))
        Next
    End Sub

    <DataTestMethod>
    <DataRow(HelpTopic.NormalityTests, False)> <DataRow(HelpTopic.NormalityTests, True)>
    <DataRow(HelpTopic.DescriptiveStatistics, False)> <DataRow(HelpTopic.DescriptiveStatistics, True)>
    <DataRow(HelpTopic.OneWayANOVA, False)> <DataRow(HelpTopic.OneWayANOVA, True)>
    <DataRow(HelpTopic.KruskalWallisTest, False)> <DataRow(HelpTopic.KruskalWallisTest, True)>
    Public Sub ActualAdapter_AllTargetRoleRoutes_MatchFrozenManagedSelection(topic As Integer, byId As Boolean)
        Assert.IsTrue(New Integer() {HelpTopic.NormalityTests, HelpTopic.DescriptiveStatistics,
            HelpTopic.OneWayANOVA, HelpTopic.KruskalWallisTest}.Contains(topic))
        Dim roles = UnivariateSelectionPolicy.InputRoles(False, byId)
        Assert.AreEqual(byId, roles.FirstInputRequired)
        Dim errorText As String = String.Empty
        If byId Then
            Dim data = Imported(New Object(,) {{"ID", "Value"}, {"B", 8}, {"A", 1}, {"B", Nothing},
                {"B", 9}, {"A", 2}}, {"ID", "Value"}, byId:=True)
            Same(BeforeById(data), WindowsUnivariateInputAdapter.SelectById(data, errorText))
            Dim selected = UnivariateSelectionPolicy.SelectImportedById(data)
            CollectionAssert.AreEqual(New Integer() {21, 24}, selected.Sources.Where(Function(s) s.GroupIndex = 0).
                Select(Function(s) s.WorksheetRow).ToArray())
        Else
            Dim columns = {Imported(New Object(,) {{"B"}, {8}, {Nothing}, {9}}, {"B"}),
                Imported(New Object(,) {{Nothing}, {Nothing}, {Nothing}}, {"empty"}),
                Imported(New Object(,) {{"A"}, {1}, {2}, {3}}, {"A"})}
            Dim beforeError As String = String.Empty
            Dim captured = columns.Select(Function(column) WindowsUnivariateInputAdapter.CaptureColumn(column)).ToArray()
            Same(BeforeColumns(columns, beforeError), WindowsUnivariateInputAdapter.SelectCapturedColumns(captured, errorText))
            Assert.AreEqual(beforeError, errorText)
            Dim selected = UnivariateSelectionPolicy.SelectImportedColumns(columns)
            CollectionAssert.AreEqual(New Integer() {21, 23}, selected.Sources.Where(Function(s) s.GroupIndex = 0).
                Select(Function(s) s.WorksheetRow).ToArray())
            Assert.IsTrue(selected.Sources.All(Function(s) s.WorksheetColumn = 0), "Importer has no physical source-column metadata")
        End If
        Assert.AreEqual(String.Empty, errorText)
    End Sub

    <TestMethod>
    Public Sub Capture_IsImmutable_AndDoesNotRetainRawImporterMatrices()
        Dim importedData = Imported(New Object(,) {{1}, {Nothing}, {3}}, {"A"})
        Dim captured = WindowsUnivariateInputAdapter.CaptureColumn(importedData)
        importedData.FinalData(0, 0) = 999
        importedData.varNames(0) = "mutated"
        importedData.RowIds(0) = 999
        Dim errorText As String = String.Empty
        Dim after = WindowsUnivariateInputAdapter.SelectCapturedColumns({captured}, errorText)
        CollectionAssert.AreEqual(New Double() {1, 3}, after.X(0))
        Assert.AreEqual("A", after.varNames(0))
        Assert.AreEqual(20, captured.Sources(0).WorksheetRow)
        Assert.ThrowsException(Of NotSupportedException)(Sub() DirectCast(captured.Values, IList(Of Double)).Clear())
    End Sub

    <TestMethod>
    Public Sub GuiUdfAlignmentCandidate_HeaderTrimming_IsExecutableAndUnchanged()
        Dim raw As Object(,) = {{" A ", "B"}, {1, 3}, {2, 4}}
        Dim udfGroups()() As Double = Nothing, udfNames() As String = Nothing
        Assert.IsTrue(UdfDataImport.TryGetGroupedNumericColumns(raw, udfGroups, udfNames))
        Dim columns = {Imported(New Object(,) {{" A "}, {1}, {2}}, {" A "}),
            Imported(New Object(,) {{"B"}, {3}, {4}}, {"B"})}
        Dim errorText As String = String.Empty
        Dim gui = WindowsUnivariateInputAdapter.SelectColumns(columns, errorText)
        CollectionAssert.AreEqual(udfGroups(0), gui.X(0))
        Assert.AreEqual(" A ", gui.varNames(0))
        Assert.AreEqual("A", udfNames(0), "Alignment candidate; no UDF or GUI default was changed")
    End Sub

    <DataTestMethod>
    <DataRow("")> <DataRow("sk-SK")>
    Public Sub Compatibility_PreservesNumericTextHeaderWhitespaceCaseAndImporterCategoryNormalization(culture As String)
        Dim previous = CultureInfo.CurrentCulture
        Try
            CultureInfo.CurrentCulture = If(culture = "", CultureInfo.InvariantCulture, CultureInfo.GetCultureInfo(culture))
            Dim columns = {Imported(New Object(,) {{"0"}, {1}, {2}, {"bad"}, {True}, {New DateTime(2020, 1, 1)}}, {"0"})}
            Dim errorText As String = String.Empty, beforeError As String = String.Empty
            Dim after = WindowsUnivariateInputAdapter.SelectColumns(columns, errorText)
            Same(BeforeColumns(columns, beforeError), after)
            CollectionAssert.AreEqual(New Double() {0, 1, 2}, after.X(0), "Legacy numeric text remains an observation")
            Dim ids = Imported(New Object(,) {{" A ", 1}, {"A", 2}, {"a", 3}, {1.0R, 4}, {"1", 5}},
                {"ID", "Value"}, byId:=True)
            Dim byId = WindowsUnivariateInputAdapter.SelectById(ids, errorText)
            Same(BeforeById(ids), byId)
            CollectionAssert.AreEqual(New String() {" A ", "A", "a", "1"}, byId.varNames)
            Assert.AreEqual(4, byId.X.Length, "CharCols=0 normalizes numeric and text IDs to strings before selection")
            CollectionAssert.AreEqual(New Double() {4, 5}, byId.X(3))
        Finally
            CultureInfo.CurrentCulture = previous
        End Try
    End Sub

    <TestMethod>
    Public Sub InvalidWideInput_HasExactLegacyDiagnostic_AndNoResultToWrite()
        Dim columns = {Imported(New Object(,) {{Nothing}, {"bad"}, {False}}, {"A"})}
        Dim beforeError As String = String.Empty, errorText As String = String.Empty
        Assert.IsNull(BeforeColumns(columns, beforeError))
        Assert.IsNull(WindowsUnivariateInputAdapter.SelectColumns(columns, errorText))
        Assert.AreEqual("No valid data", errorText)
        Assert.AreEqual(beforeError, errorText)
        Dim invalid = UnivariateSelectionPolicy.SelectImportedColumns(columns)
        Assert.AreEqual("groups.no-valid-data", invalid.Validation.Issues.Single().Code)
        Assert.AreEqual(0, invalid.Sources.Count)
    End Sub

    <TestMethod>
    Public Sub ExistingWorksheetDiagnostic_IsPreservedOnSuccessfulConversion()
        Dim errorText = "Input reference range adresses are from different sheets. Input can be only from one sheet."
        Dim original = errorText
        Dim data = Imported(New Object(,) {{"A", 1}, {"B", 2}}, {"ID", "Data"}, byId:=True)
        Assert.IsNotNull(WindowsUnivariateInputAdapter.SelectById(data, errorText))
        Assert.AreEqual(original, errorText, "Form rejects before running/writing")
    End Sub

    <TestMethod>
    Public Sub LegacyOneGroupSingleObservationAndDuplicateColumnNames_AreNotNewlyRejected()
        Dim columns = {Imported(New Object(,) {{1}}, {"A"}), Imported(New Object(,) {{2}, {3}}, {"A"})}
        Dim errorText As String = String.Empty, beforeError As String = String.Empty
        Same(BeforeColumns(columns, beforeError), WindowsUnivariateInputAdapter.SelectColumns(columns, errorText))
        Assert.IsNotNull(WindowsUnivariateInputAdapter.SelectColumns({columns(0)}, errorText))
        Dim data = Imported(New Object(,) {{"A", 1}}, {"ID", "Value"}, byId:=True)
        Same(BeforeById(data), WindowsUnivariateInputAdapter.SelectById(data, errorText))
    End Sub

    <TestMethod>
    Public Sub LegacyNonFiniteCompatibility_IsDisclosedWithoutChangingStatisticsOrPromotingIt()
        Dim columns = {Imported(New Object(,) {{Double.NaN}, {Double.PositiveInfinity}, {1}}, {"A"})}
        Dim beforeError As String = String.Empty, errorText As String = String.Empty
        Same(BeforeColumns(columns, beforeError), WindowsUnivariateInputAdapter.SelectColumns(columns, errorText))
        ' Raw typed policy rejects this; native importer correction needs its own approved decision.
    End Sub

    <TestMethod>
    Public Sub EmptyByIdCompatibility_RetainsExistingExceptionUntilExplicitCorrection()
        Dim data = Imported(New Object(,) {{Nothing, Nothing}}, {"ID", "Value"}, byId:=True)
        Dim errorText As String = String.Empty
        Assert.ThrowsException(Of NullReferenceException)(Sub() BeforeById(data))
        Assert.ThrowsException(Of NullReferenceException)(Sub() WindowsUnivariateInputAdapter.SelectById(data, errorText))
    End Sub
End Class
