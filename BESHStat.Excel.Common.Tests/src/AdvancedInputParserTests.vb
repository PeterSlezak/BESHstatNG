Option Explicit On
Option Strict On
Option Infer On

Imports System
Imports System.Collections.Generic
Imports System.Globalization
Imports System.Linq
Imports BESHStatNG.ExcelCommon
Imports Microsoft.VisualStudio.TestTools.UnitTesting

<TestClass>
Public Class AdvancedInputParserTests
    Private Shared Function Source(ParamArray data As Object()()) As SpreadsheetRangeData
        Dim rows As New List(Of List(Of SpreadsheetCellValue))()
        For Each row In data
            Dim cells As New List(Of SpreadsheetCellValue)()
            For Each value In row
                If value Is Nothing Then
                    cells.Add(SpreadsheetCellValue.BlankCell())
                ElseIf TypeOf value Is SpreadsheetCellValue Then
                    cells.Add(DirectCast(value, SpreadsheetCellValue))
                ElseIf TypeOf value Is Boolean Then
                    cells.Add(SpreadsheetCellValue.BooleanCell(CBool(value)))
                ElseIf TypeOf value Is String Then
                    cells.Add(SpreadsheetCellValue.TextCell(CStr(value)))
                Else
                    cells.Add(SpreadsheetCellValue.NumberCell(Convert.ToDouble(value, CultureInfo.InvariantCulture)))
                End If
            Next
            rows.Add(cells)
        Next
        Return New SpreadsheetRangeData With {.WorkbookId = "book-id", .WorkbookName = "Study",
            .WorksheetId = "sheet-id", .WorksheetName = "Observations", .Address = "C20:F30",
            .SelectionId = "selection", .SelectionRevision = "revision", .StartRow = 20, .StartColumn = 3,
            .RowCount = rows.Count, .ColumnCount = If(rows.Count = 0, 0, rows(0).Count), .Rows = rows}
    End Function

    Private Shared Function Issue(Of T As SpreadsheetAnalysisInput)(result As SpreadsheetTypedParseResult(Of T), code As String) As SpreadsheetValidationIssue
        Assert.IsFalse(result.Success)
        Assert.IsNull(result.Data)
        Dim found = result.Validation.Issues.FirstOrDefault(Function(item) item.Code = code)
        Assert.IsNotNull(found, code)
        Return found
    End Function

    <TestMethod>
    Public Sub Matrix_PreservesSelectedColumnOrderAndDefensiveCopies()
        Dim sourceData = Source(New Object() {"A", "B", "C"}, New Object() {1, 2, 3}, New Object() {4, 5, 6})
        Dim opts As New SpreadsheetMatrixParseOptions With {.NumericColumnIndices = New Integer() {2, 0}, .RequireSquare = True}
        Dim result = AdvancedInputParsers.ParseMatrix(sourceData, opts)
        Assert.IsTrue(result.Success)
        Assert.AreEqual(2, result.Data.RowCount)
        Assert.AreEqual("C", result.Data.Columns(0).Name)
        Assert.AreEqual(5, result.Data.Columns(0).WorksheetColumn)
        Assert.AreEqual(21, result.Data.Rows(0).WorksheetRow)
        Assert.AreEqual(2, result.Data.Rows(0).SelectedRow)
        Dim matrix = result.Data.ToArray()
        Assert.AreEqual(3.0R, matrix(0, 0))
        matrix(0, 0) = -100
        opts.NumericColumnIndices(0) = 1
        sourceData.Rows(1)(2).NumberValue = 999
        Assert.AreEqual(3.0R, result.Data.ToArray()(0, 0))
        Assert.AreEqual(3.0R, result.Data.Source.Cell(0, 2).NumberValue)
        Assert.AreEqual("book-id", result.Data.Source.WorkbookId)
        Assert.AreEqual("sheet-id", result.Data.Source.WorksheetId)
        Assert.AreEqual("revision", result.Data.Source.SelectionRevision)
        Assert.AreEqual("C20:F30", result.Data.Source.SourceAddress)
    End Sub

    <TestMethod>
    Public Sub Matrix_MissingRowsAreOmittedTogetherWithoutRenumbering()
        Dim data = Source(New Object() {"A", "B"}, New Object() {1, 2}, New Object() {Nothing, 4}, New Object() {5, 6})
        Issue(AdvancedInputParsers.ParseMatrix(data), "advanced.missing")
        Dim result = AdvancedInputParsers.ParseMatrix(data, New SpreadsheetMatrixParseOptions With {.Missing = SpreadsheetIncompleteObservationPolicy.Skip})
        Assert.IsTrue(result.Success)
        Assert.IsTrue(result.Validation.HasWarnings)
        Assert.AreEqual(2, result.Data.RowCount)
        Assert.AreEqual(4, result.Data.Rows(1).SelectedRow)
        Assert.AreEqual(23, result.Data.Rows(1).WorksheetRow)
        Assert.AreEqual(5.0R, result.Data.Rows(1).Values(0).Value)
        Assert.AreEqual(6.0R, result.Data.Rows(1).Values(1).Value)
        Assert.AreEqual(3, result.Validation.Issues(0).SelectedRow)
    End Sub

    <TestMethod>
    Public Sub Matrix_DimensionsCheckedAfterOmission()
        Dim data = Source(New Object() {"A", "B"}, New Object() {1, 2}, New Object() {Nothing, 4})
        Issue(AdvancedInputParsers.ParseMatrix(data, New SpreadsheetMatrixParseOptions With {
            .RequireSquare = True, .Missing = SpreadsheetIncompleteObservationPolicy.Skip}), "matrix.square")
        Issue(AdvancedInputParsers.ParseMatrix(data, New SpreadsheetMatrixParseOptions With {.MinimumColumns = 3}), "matrix.columns")
        Issue(AdvancedInputParsers.ParseMatrix(data, New SpreadsheetMatrixParseOptions With {
            .MinimumRows = 2, .Missing = SpreadsheetIncompleteObservationPolicy.Skip}), "matrix.rows")
    End Sub

    <TestMethod>
    Public Sub Matrix_InvalidOptionsAndRoleIndicesProduceDiagnostics()
        Dim data = Source(New Object() {"A", "B"}, New Object() {1, 2})
        For Each indices In New Integer()() {New Integer() {}, New Integer() {0, 0}, New Integer() {-1}, New Integer() {2}}
            Assert.IsFalse(AdvancedInputParsers.ParseMatrix(data, New SpreadsheetMatrixParseOptions With {.NumericColumnIndices = indices}).Success)
        Next
        Issue(AdvancedInputParsers.ParseMatrix(data, New SpreadsheetMatrixParseOptions With {.Header = SpreadsheetHeaderMode.Auto}), "advanced.options")
        Issue(AdvancedInputParsers.ParseMatrix(data, New SpreadsheetMatrixParseOptions With {.CellPolicy = Nothing}), "advanced.options")
        Issue(AdvancedInputParsers.ParseMatrix(data, New SpreadsheetMatrixParseOptions With {.MinimumRows = 0}), "advanced.options")
        Issue(AdvancedInputParsers.ParseMatrix(data, New SpreadsheetMatrixParseOptions With {.Missing = CType(99, SpreadsheetIncompleteObservationPolicy)}), "advanced.options")
    End Sub

    <TestMethod>
    Public Sub Matrix_RejectsMalformedRangeNonfiniteAndImplicitCoercion()
        Assert.IsFalse(AdvancedInputParsers.ParseMatrix(Nothing).Success)
        Dim ragged = Source(New Object() {1, 2}, New Object() {3})
        Assert.IsFalse(AdvancedInputParsers.ParseMatrix(ragged).Success)
        Dim opts As New SpreadsheetMatrixParseOptions With {.Header = SpreadsheetHeaderMode.None}
        Issue(AdvancedInputParsers.ParseMatrix(Source(New Object() {Double.NaN}), opts), "cell.invalid-value")
        Issue(AdvancedInputParsers.ParseMatrix(Source(New Object() {Double.PositiveInfinity}), opts), "cell.invalid-value")
        Issue(AdvancedInputParsers.ParseMatrix(Source(New Object() {"1"}), opts), "cell.not-numeric")
        Issue(AdvancedInputParsers.ParseMatrix(Source(New Object() {True}), opts), "cell.not-numeric")
        opts.CellPolicy.NumericText = SpreadsheetNumericTextPolicy.Invariant
        Assert.IsTrue(AdvancedInputParsers.ParseMatrix(Source(New Object() {"1.25"}), opts).Success)
        opts.CellPolicy.AllowBooleanNumbers = True
        Assert.IsTrue(AdvancedInputParsers.ParseMatrix(Source(New Object() {True}), opts).Success)
    End Sub

    <TestMethod>
    Public Sub Matrix_ErrorPolicyRetainsNativeMetadataInSnapshot()
        Dim err As New SpreadsheetCellValue With {.Kind = SpreadsheetCellKind.CellError, .TextValue = "host error", .NativeErrorCode = "native-42"}
        Dim data = Source(New Object() {"A"}, New Object() {err}, New Object() {7})
        Dim issueData = Issue(AdvancedInputParsers.ParseMatrix(data), "cell.spreadsheet-error")
        Assert.AreEqual(21, issueData.WorksheetRow)
        Assert.AreEqual(3, issueData.WorksheetColumn)
        Dim opts As New SpreadsheetMatrixParseOptions With {.Missing = SpreadsheetIncompleteObservationPolicy.Skip}
        opts.CellPolicy.Errors = SpreadsheetErrorPolicy.Preserve
        Issue(AdvancedInputParsers.ParseMatrix(data, opts), "cell.spreadsheet-error")
        opts.CellPolicy.Errors = SpreadsheetErrorPolicy.TreatAsMissing
        Dim result = AdvancedInputParsers.ParseMatrix(data, opts)
        Assert.IsTrue(result.Success)
        Assert.AreEqual("native-42", result.Data.Source.Cell(0, 0).NativeErrorCode)
        Assert.AreEqual("host error", result.Data.Source.Cell(0, 0).TextValue)
        Assert.AreEqual(3, result.Data.Rows(0).SelectedRow)
    End Sub

    <TestMethod>
    Public Sub Classification_UsesTypedLabelsAndCompleteRows()
        Dim data = Source(New Object() {"Class", "X"}, New Object() {1, 10}, New Object() {"1", 20}, New Object() {True, 30})
        Dim result = AdvancedInputParsers.ParseClassification(data)
        Assert.IsTrue(result.Success)
        Assert.AreEqual(3, result.Data.Classes.Count)
        Assert.AreEqual("N:1", result.Data.Classes(0).Key)
        Assert.AreEqual("T:1:1", result.Data.Classes(1).Key)
        Assert.AreEqual("B:1", result.Data.Classes(2).Key)
        Assert.AreEqual(SpreadsheetCellKind.Bool, result.Data.Rows(2).Label.Kind)
        Assert.AreEqual(4, result.Data.Rows(2).SelectedRow)
        Assert.AreEqual(4, result.Data.Rows(0).Values(0).WorksheetColumn)
    End Sub

    <TestMethod>
    Public Sub Classification_CasePolicyAndMinimumClassesApplyAfterMissing()
        Dim data = Source(New Object() {"Class", "X"}, New Object() {"a", 10}, New Object() {"A", 20}, New Object() {"b", Nothing})
        Dim opts As New SpreadsheetClassificationParseOptions With {.Missing = SpreadsheetIncompleteObservationPolicy.Skip}
        Assert.IsTrue(AdvancedInputParsers.ParseClassification(data, opts).Success)
        opts.IgnoreCategoryCase = True
        Issue(AdvancedInputParsers.ParseClassification(data, opts), "classification.classes")
        opts.MinimumClasses = 0
        Issue(AdvancedInputParsers.ParseClassification(data, opts), "classification.options")
    End Sub

    <TestMethod>
    Public Sub Classification_FoldedRowKeysMapToDeclaredClassesWithoutLosingSpelling()
        Dim data = Source(New Object() {"Class", "X"}, New Object() {"a", 1}, New Object() {"A", 2}, New Object() {"b", 3})
        Dim result = AdvancedInputParsers.ParseClassification(data, New SpreadsheetClassificationParseOptions With {.IgnoreCategoryCase = True})
        Assert.IsTrue(result.Success)
        Assert.AreEqual(2, result.Data.Classes.Count)
        Assert.AreEqual(result.Data.Classes(0).Key, result.Data.Rows(0).LabelLevel.Key)
        Assert.AreEqual(result.Data.Rows(0).LabelLevel.Key, result.Data.Rows(1).LabelLevel.Key)
        Assert.AreEqual("a", result.Data.Rows(0).Label.TextValue)
        Assert.AreEqual("A", result.Data.Rows(1).Label.TextValue)
    End Sub

    <TestMethod>
    Public Sub Classification_RejectsMissingLabelsAndOverlappingRoles()
        Dim data = Source(New Object() {"Class", "X"}, New Object() {Nothing, 10}, New Object() {"b", 20})
        Assert.AreEqual(2, Issue(AdvancedInputParsers.ParseClassification(data), "advanced.missing").SelectedRow)
        Issue(AdvancedInputParsers.ParseClassification(data, New SpreadsheetClassificationParseOptions With {.ClassColumnIndex = -1}), "classification.options")
        Issue(AdvancedInputParsers.ParseClassification(data, New SpreadsheetClassificationParseOptions With {.NumericColumnIndices = New Integer() {0}}), "advanced.roles")
    End Sub

    <TestMethod>
    Public Sub Psm_NumericArmsAlignWithCovariates()
        Dim data = Source(New Object() {"T", "X", "Z"}, New Object() {0, 10, 11}, New Object() {1, Nothing, 21}, New Object() {1, 30, 31})
        Dim result = AdvancedInputParsers.ParsePsm(data, New SpreadsheetPsmParseOptions With {.Missing = SpreadsheetIncompleteObservationPolicy.Skip})
        Assert.IsTrue(result.Success)
        Assert.AreEqual(2, result.Data.RowCount)
        Assert.IsFalse(result.Data.Rows(0).IsTreated.Value)
        Assert.IsTrue(result.Data.Rows(1).IsTreated.Value)
        Assert.AreEqual(4, result.Data.Rows(1).SelectedRow)
        Assert.AreEqual(30.0R, result.Data.Rows(1).Values(0).Value)
        Assert.AreEqual(31.0R, result.Data.Rows(1).Values(1).Value)
    End Sub

    <TestMethod>
    Public Sub Psm_RequiresExactDeclaredTypedLevels()
        For Each value As Object In New Object() {2, "1", True}
            Dim data = Source(New Object() {"T", "X"}, New Object() {0, 10}, New Object() {value, 20})
            Assert.AreEqual(3, Issue(AdvancedInputParsers.ParsePsm(data), "psm.treatment").SelectedRow)
        Next
        Dim opts As New SpreadsheetPsmParseOptions With {
            .TreatedLevel = New SpreadsheetCategoryValue(SpreadsheetCellValue.TextCell("treated")),
            .ControlLevel = New SpreadsheetCategoryValue(SpreadsheetCellValue.TextCell("control"))}
        Dim result = AdvancedInputParsers.ParsePsm(Source(New Object() {"T", "X"}, New Object() {"control", 1}, New Object() {"treated", 2}), opts)
        Assert.IsTrue(result.Success)
        Assert.IsTrue(result.Data.Rows(1).IsTreated.Value)
        opts.ControlLevel = opts.TreatedLevel
        Issue(AdvancedInputParsers.ParsePsm(Nothing, opts), "psm.options")
    End Sub

    <TestMethod>
    Public Sub Psm_BothArmsRequiredAfterOmissionAndInvalidTreatmentNeverSkipped()
        Dim data = Source(New Object() {"T", "X"}, New Object() {0, 10}, New Object() {1, Nothing})
        Dim opts As New SpreadsheetPsmParseOptions With {.Missing = SpreadsheetIncompleteObservationPolicy.Skip}
        Issue(AdvancedInputParsers.ParsePsm(data, opts), "psm.arms")
        data.Rows(2)(0) = SpreadsheetCellValue.NumberCell(2)
        Issue(AdvancedInputParsers.ParsePsm(data, opts), "psm.treatment")
    End Sub

    <TestMethod>
    Public Sub Spc_SubgroupMatrixPreservesGlobalOrderAndSourceIdentity()
        Dim data = Source(New Object() {"Time", "Subgroup", "A", "B"}, New Object() {1, "a", 10, 11}, New Object() {2, "b", 12, 13})
        Dim opts As New SpreadsheetSpcParseOptions With {.OrderColumnIndex = 0, .SubgroupColumnIndex = 1, .MinimumColumns = 2}
        Dim result = AdvancedInputParsers.ParseSpc(data, opts)
        Assert.IsTrue(result.Success)
        Assert.AreEqual(2, result.Data.ColumnCount)
        Assert.AreEqual("b", result.Data.Rows(1).Group.TextValue)
        Assert.AreEqual(2.0R, result.Data.Rows(1).CoordinateValue.Value)
        Assert.AreEqual(3, result.Data.Rows(0).Coordinate.WorksheetColumn)
        data.Rows(2)(0) = SpreadsheetCellValue.NumberCell(1)
        Assert.AreEqual(3, Issue(AdvancedInputParsers.ParseSpc(data, opts), "advanced.order").SelectedRow)
    End Sub

    <TestMethod>
    Public Sub Spc_DefaultsToSourceOrderAndRejectsBadOrderRoles()
        Dim data = Source(New Object() {"A"}, New Object() {4}, New Object() {3})
        Dim result = AdvancedInputParsers.ParseSpc(data)
        Assert.IsTrue(result.Success)
        Assert.AreEqual(2.0R, result.Data.Rows(0).CoordinateValue.Value)
        Issue(AdvancedInputParsers.ParseSpc(data, New SpreadsheetSpcParseOptions With {.OrderColumnIndex = -2}), "advanced.roles")
        Issue(AdvancedInputParsers.ParseSpc(data, New SpreadsheetSpcParseOptions With {.InterpretOrderAsDate = True}), "advanced.options")
        Dim textOrder = Source(New Object() {"Time", "Y"}, New Object() {"Monday", 1})
        Issue(AdvancedInputParsers.ParseSpc(textOrder, New SpreadsheetSpcParseOptions With {.OrderColumnIndex = 0}), "advanced.coordinate-type")
    End Sub

    <TestMethod>
    Public Sub Spc_DateCoordinatesRequireExplicitHostPolicy()
        Dim data = Source(New Object() {"Time", "Y"}, New Object() {1, 10}, New Object() {2, 20})
        Dim opts As New SpreadsheetSpcParseOptions With {.OrderColumnIndex = 0, .InterpretOrderAsDate = True}
        Issue(AdvancedInputParsers.ParseSpc(data, opts), "advanced.date")
        data.DateSerialOrigin = New DateTime(2000, 1, 1)
        Dim result = AdvancedInputParsers.ParseSpc(data, opts)
        Assert.IsTrue(result.Success)
        Assert.AreEqual(New DateTime(2000, 1, 2), result.Data.Rows(0).CoordinateDate.Value)
        Assert.AreEqual(data.DateSerialOrigin, result.Data.DateSerialOrigin)
        data.DateSystem = SpreadsheetDateSystem.Excel1900
        Issue(AdvancedInputParsers.ParseSpc(data, opts), "advanced.date")
        data.DateSerialOrigin = Nothing
        data.Rows(1)(0) = SpreadsheetCellValue.NumberCell(60)
        Issue(AdvancedInputParsers.ParseSpc(data, opts), "advanced.date")
    End Sub

    <TestMethod>
    Public Sub Chart_CategoryXAndMultipleSeriesKeepLabelsAndGroups()
        Dim data = Source(New Object() {"X", "Y", "Z", "Group"}, New Object() {"Jan", 1, 2, "a"}, New Object() {"Feb", 3, 4, "a"})
        Dim opts As New SpreadsheetChartSourceParseOptions With {.XColumnIndex = 0, .GroupColumnIndex = 3}
        Dim result = AdvancedInputParsers.ParseChartSource(data, opts)
        Assert.IsTrue(result.Success)
        Assert.AreEqual("Jan", result.Data.Rows(0).Coordinate.TextValue)
        Assert.IsFalse(result.Data.Rows(0).CoordinateValue.HasValue)
        Assert.AreEqual("a", result.Data.Rows(1).Group.TextValue)
        Assert.AreEqual(2, result.Data.ColumnCount)
        Assert.AreEqual("Z", result.Data.Columns(1).Name)
        opts.RequireNumericX = True
        Issue(AdvancedInputParsers.ParseChartSource(data, opts), "advanced.coordinate-type")
    End Sub

    <TestMethod>
    Public Sub Chart_OrderingIsOptInAndCheckedWithinTypedGroups()
        Dim data = Source(New Object() {"X", "Y", "G"}, New Object() {2, 10, 1}, New Object() {1, 20, "1"}, New Object() {3, 30, 1})
        Dim opts As New SpreadsheetChartSourceParseOptions With {.XColumnIndex = 0, .GroupColumnIndex = 2, .RequireAscendingX = True}
        Assert.IsTrue(AdvancedInputParsers.ParseChartSource(data, opts).Success)
        data.Rows(3)(0) = SpreadsheetCellValue.NumberCell(2)
        Issue(AdvancedInputParsers.ParseChartSource(data, opts), "advanced.order")
        opts.RequireAscendingX = False
        Assert.IsTrue(AdvancedInputParsers.ParseChartSource(data, opts).Success)
        opts.GroupColumnIndex = 0
        Issue(AdvancedInputParsers.ParseChartSource(data, opts), "advanced.roles")
    End Sub

    <TestMethod>
    Public Sub Chart_MissingSeriesOmitsWholePointButRetainsOriginalX()
        Dim data = Source(New Object() {"X", "A", "B"}, New Object() {10, 1, 2}, New Object() {20, 3, Nothing}, New Object() {30, 4, 5})
        Dim opts As New SpreadsheetChartSourceParseOptions With {.XColumnIndex = 0, .Missing = SpreadsheetIncompleteObservationPolicy.Skip}
        Dim result = AdvancedInputParsers.ParseChartSource(data, opts)
        Assert.IsTrue(result.Success)
        Assert.AreEqual(2, result.Data.RowCount)
        Assert.AreEqual(30.0R, result.Data.Rows(1).CoordinateValue.Value)
        Assert.AreEqual(4, result.Data.Rows(1).SelectedRow)
        data.Rows(2)(1) = SpreadsheetCellValue.TextCell("invalid")
        Issue(AdvancedInputParsers.ParseChartSource(data, opts), "cell.not-numeric")
    End Sub

    <TestMethod>
    Public Sub Chart_Excel1904DatesAndSourceRowXAreExplicit()
        Dim data = Source(New Object() {"Time", "Y"}, New Object() {0, 10}, New Object() {1, 20})
        data.DateSystem = SpreadsheetDateSystem.Excel1904
        Dim result = AdvancedInputParsers.ParseChartSource(data, New SpreadsheetChartSourceParseOptions With {.XColumnIndex = 0, .InterpretXAsDate = True})
        Assert.IsTrue(result.Success)
        Assert.AreEqual(New DateTime(1904, 1, 1), result.Data.Rows(0).CoordinateDate.Value)
        Dim implicitX = AdvancedInputParsers.ParseChartSource(Source(New Object() {10}, New Object() {20}),
            New SpreadsheetChartSourceParseOptions With {.Header = SpreadsheetHeaderMode.None})
        Assert.IsTrue(implicitX.Success)
        Assert.AreEqual(1.0R, implicitX.Data.Rows(0).CoordinateValue.Value)
        Assert.IsNull(implicitX.Data.Rows(0).Coordinate)
    End Sub

    <TestMethod>
    Public Sub Matrix_HostNativeUnknownCoordinatesAndUnusedErrorsArePreserved()
        Dim data = Source(New Object() {"Y", "Unused"}, New Object() {1, SpreadsheetCellValue.ErrorCell("native", "E42")})
        data.AddressDialect = SpreadsheetAddressDialect.HostNative
        data.Address = "opaque host selection"
        data.StartRow = 0
        data.StartColumn = 0
        Dim result = AdvancedInputParsers.ParseMatrix(data, New SpreadsheetMatrixParseOptions With {.NumericColumnIndices = New Integer() {0}})
        Assert.IsTrue(result.Success)
        Assert.AreEqual(0, result.Data.Rows(0).WorksheetRow)
        Assert.AreEqual(0, result.Data.Rows(0).Values(0).WorksheetColumn)
        Assert.AreEqual("opaque host selection", result.Data.Source.SourceAddress)
        Assert.AreEqual("E42", result.Data.Source.Cell(0, 1).NativeErrorCode)
    End Sub

    <TestMethod>
    Public Sub Chart_MissingGroupAndWhitespacePolicyAreAppliedToWholeRows()
        Dim data = Source(New Object() {"X", "Y", "Group"}, New Object() {1, 2, "a"}, New Object() {2, 3, " "}, New Object() {3, 4, "a"})
        Dim opts As New SpreadsheetChartSourceParseOptions With {.XColumnIndex = 0, .GroupColumnIndex = 2, .Missing = SpreadsheetIncompleteObservationPolicy.Skip}
        opts.CellPolicy.MissingText = SpreadsheetMissingTextPolicy.BlankOrWhitespaceText
        Dim result = AdvancedInputParsers.ParseChartSource(data, opts)
        Assert.IsTrue(result.Success)
        Assert.AreEqual(2, result.Data.RowCount)
        Assert.AreEqual(4, result.Data.Rows(1).SelectedRow)
        Assert.AreEqual(3, result.Validation.Issues(0).SelectedColumn)
        data.Rows(2)(2) = SpreadsheetCellValue.ErrorCell("native", "E1")
        Issue(AdvancedInputParsers.ParseChartSource(data, opts), "cell.spreadsheet-error")
    End Sub

    <TestMethod>
    Public Sub Compatibility_NewParsersDoNotChangeLegacyGroupedInputs()
        Dim data = Source(New Object() {"A", "B"}, New Object() {1, 2}, New Object() {3, 4})
        Dim before = GroupedNumericRangeParser.Parse(data)
        Assert.IsTrue(AdvancedInputParsers.ParseMatrix(data).Success)
        Dim after = GroupedNumericRangeParser.Parse(data)
        CollectionAssert.AreEqual(before.Groups(0), after.Groups(0))
    End Sub
End Class
