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
Public Class LongitudinalParserTests

    Private Shared Function Source(ParamArray data As Object()()) As SpreadsheetRangeData
        Dim rows As New List(Of List(Of SpreadsheetCellValue))()
        For Each values As Object() In data
            Dim cells As New List(Of SpreadsheetCellValue)()
            For Each value As Object In values
                If value Is Nothing Then
                    cells.Add(SpreadsheetCellValue.BlankCell())
                ElseIf TypeOf value Is SpreadsheetCellValue Then
                    cells.Add(DirectCast(value, SpreadsheetCellValue))
                ElseIf TypeOf value Is Boolean Then
                    cells.Add(SpreadsheetCellValue.BooleanCell(CBool(value)))
                ElseIf TypeOf value Is Integer OrElse TypeOf value Is Double OrElse TypeOf value Is Long Then
                    cells.Add(SpreadsheetCellValue.NumberCell(Convert.ToDouble(value, CultureInfo.InvariantCulture)))
                Else
                    cells.Add(SpreadsheetCellValue.TextCell(Convert.ToString(value, CultureInfo.InvariantCulture)))
                End If
            Next
            rows.Add(cells)
        Next
        Return New SpreadsheetRangeData With {
            .WorkbookId = "workbook-1", .WorksheetId = "sheet-1", .WorksheetName = "Trial",
            .StartRow = 20, .StartColumn = 3, .RowCount = rows.Count,
            .ColumnCount = If(rows.Count = 0, 0, rows(0).Count), .Rows = rows}
    End Function

    Private Shared Function Issue(Of T As SpreadsheetAnalysisInput)(result As SpreadsheetTypedParseResult(Of T), code As String) As SpreadsheetValidationIssue
        Return result.Validation.Issues.FirstOrDefault(Function(item) item.Code = code)
    End Function

    <TestMethod>
    Public Sub Survival_EventCensorEntryAndTypedStrataKeepSourceRows()
        Dim data = Source(New Object() {"End", "Event", "Entry", "Stratum"},
                          New Object() {6, 1, 2, "Treatment"},
                          New Object() {9, 0, 0, "Control"},
                          New Object() {12, 1, 5, 2})
        Dim opts As New SpreadsheetSurvivalParseOptions With {.EntryColumnIndex = 2, .StratumColumnIndex = 3}
        Dim parsed = LongitudinalParsers.ParseSurvival(data, opts)
        Assert.IsTrue(parsed.Success)
        Assert.AreEqual(3, parsed.Data.Count)
        Assert.AreEqual(2, parsed.Data.EventCount)
        Assert.AreEqual(6.0R, parsed.Data.Observations(0).Time.Value)
        Assert.AreEqual(2.0R, parsed.Data.Observations(0).EntryTime.Value)
        Assert.IsFalse(parsed.Data.Observations(1).IsEvent)
        Assert.AreEqual("N:2", parsed.Data.Observations(2).Stratum.Key)
        Assert.AreEqual(2, parsed.Data.Observations(0).SelectedRow)
        Assert.AreEqual(21, parsed.Data.Observations(0).WorksheetRow)
        Assert.AreEqual(3, parsed.Data.Observations(0).Time.WorksheetColumn)
        Assert.AreEqual(5, parsed.Data.Observations(0).EntryTime.WorksheetColumn)
    End Sub

    <TestMethod>
    Public Sub Survival_ZeroCensoredEventsAllowedAndNoHeader()
        Dim opts As New SpreadsheetSurvivalParseOptions With {.Header = SpreadsheetHeaderMode.None}
        Dim parsed = LongitudinalParsers.ParseSurvival(Source(New Object() {1, 0}, New Object() {2, 0}), opts)
        Assert.IsTrue(parsed.Success)
        Assert.AreEqual(0, parsed.Data.EventCount)
        Assert.IsNull(parsed.Data.Observations(0).EntryTime)
        Assert.IsNull(parsed.Data.Observations(0).Stratum)
    End Sub

    <TestMethod>
    Public Sub Survival_RejectsNegativeTimeAndZeroWhenStrict()
        Dim parsed = LongitudinalParsers.ParseSurvival(Source(New Object() {"Time", "Event"}, New Object() {0, 1}, New Object() {-2, 0}))
        Assert.IsFalse(parsed.Success)
        Assert.IsNotNull(Issue(parsed, "survival.interval"))
    End Sub

    <TestMethod>
    Public Sub Survival_DelayedEntryMustPrecedeStop()
        Dim opts As New SpreadsheetSurvivalParseOptions With {.EntryColumnIndex = 2}
        Dim result = LongitudinalParsers.ParseSurvival(Source(New Object() {"Time", "Event", "Entry"}, New Object() {5, 1, 5}), opts)
        Assert.IsNotNull(Issue(result, "survival.interval"))
        Assert.IsFalse(result.Success)
    End Sub

    <TestMethod>
    Public Sub Survival_RejectsInvalidEventCodingWithoutImplicitBooleanOrText()
        Dim wrong = LongitudinalParsers.ParseSurvival(Source(New Object() {"Time", "Event"}, New Object() {3, 2}))
        Assert.IsNotNull(Issue(wrong, "survival.status"))
        Assert.IsNotNull(Issue(LongitudinalParsers.ParseSurvival(Source(New Object() {"Time", "Event"}, New Object() {3, True})), "survival.status"))
        Dim textValue = LongitudinalParsers.ParseSurvival(Source(New Object() {"Time", "Event"}, New Object() {3, "Yes"}))
        Assert.IsFalse(textValue.Success)
        Assert.IsNotNull(Issue(textValue, "survival.status"))
    End Sub

    <TestMethod>
    Public Sub Survival_OptInStatusCodingDoesNotShiftRows()
        Dim data = Source(New Object() {"Time", "Event"}, New Object() {2, "YES"}, New Object() {3, "no"}, New Object() {4, True})
        Dim opts As New SpreadsheetSurvivalParseOptions With {
            .EventText = "yes", .CensorText = "no", .IgnoreStatusCase = True,
            .AllowBooleanStatus = True}
        Dim parsed = LongitudinalParsers.ParseSurvival(data, opts)
        Assert.IsTrue(parsed.Success)
        Assert.AreEqual(2, parsed.Data.EventCount)
        Assert.AreEqual(23, parsed.Data.Observations(2).WorksheetRow)
    End Sub

    <TestMethod>
    Public Sub Survival_MissingRowsSkipOnlyWhenExplicitAndRetainCoordinates()
        Dim data = Source(New Object() {"Time", "Event"}, New Object() {1, 1}, New Object() {2, Nothing}, New Object() {4, 0})
        Dim rejected = LongitudinalParsers.ParseSurvival(data)
        Assert.IsNotNull(Issue(rejected, "survival.missing"))
        Dim opts As New SpreadsheetSurvivalParseOptions With {.Missing = SpreadsheetIncompleteObservationPolicy.Skip}
        Dim skipped = LongitudinalParsers.ParseSurvival(data, opts)
        Assert.IsTrue(skipped.Success)
        Assert.IsTrue(skipped.Validation.HasWarnings)
        Assert.IsNotNull(Issue(skipped, "survival.missing-skipped"))
        Assert.AreEqual(2, skipped.Data.Count)
        Assert.AreEqual(23, skipped.Data.Observations(1).WorksheetRow)
    End Sub

    <TestMethod>
    Public Sub Survival_ErrorRejectedWithoutTextCoercion()
        Dim parsed = LongitudinalParsers.ParseSurvival(Source(New Object() {"Time", "Event"}, New Object() {
            SpreadsheetCellValue.ErrorCell("HOST_ERR_47", "native-47"), 1}))
        Assert.IsNotNull(Issue(parsed, "cell.excel-error"))
        Assert.AreEqual(21, Issue(parsed, "cell.excel-error").WorksheetRow)
        Assert.AreEqual(3, Issue(parsed, "cell.excel-error").WorksheetColumn)
    End Sub

    <TestMethod>
    Public Sub Repeated_SubjectVisitTypedIdentityResponseAndOptionalGroup()
        Dim data = Source(New Object() {"ID", "Visit", "Y", "Arm"},
                          New Object() {"01", 0, 9, "A"},
                          New Object() {"01", 1, 11, "A"},
                          New Object() {1, 0, 2, "B"})
        Dim opts As New SpreadsheetRepeatedParseOptions With {.GroupColumnIndex = 3, .MinimumSubjects = 2}
        Dim parsed = LongitudinalParsers.ParseRepeatedSubjects(data, opts)
        Assert.IsTrue(parsed.Success)
        Assert.AreEqual(3, parsed.Data.Count)
        Assert.AreEqual(2, parsed.Data.Subjects.Count)
        Assert.AreNotEqual(parsed.Data.Observations(0).Subject.Key, parsed.Data.Observations(2).Subject.Key)
        Assert.AreEqual("A", parsed.Data.Observations(1).TreatmentGroup.TextValue)
        Assert.AreEqual(1.0R, parsed.Data.Observations(1).VisitTime.Value)
        Assert.AreEqual(22, parsed.Data.Observations(1).WorksheetRow)
        Assert.AreEqual(5, parsed.Data.Observations(1).Response.WorksheetColumn)
    End Sub

    <TestMethod>
    Public Sub Repeated_DuplicateWithinSubjectRejectedNotAcrossSubjects()
        Dim data = Source(New Object() {"ID", "Visit", "Y"}, New Object() {"A", "W0", 1},
                          New Object() {"B", "W0", 2}, New Object() {"A", "W0", 3})
        Dim parsed = LongitudinalParsers.ParseRepeatedSubjects(data)
        Assert.IsFalse(parsed.Success)
        Assert.AreEqual(23, Issue(parsed, "repeat.duplicate-visit").WorksheetRow)
        Assert.AreEqual(4, Issue(parsed, "repeat.duplicate-visit").WorksheetColumn)
    End Sub

    <TestMethod>
    Public Sub Repeated_TextVisitSupportedButCannotRequestNumericOrdering()
        Dim data = Source(New Object() {"ID", "Visit", "Y"}, New Object() {"A", "Baseline", 2},
                          New Object() {"A", "Week 4", 3})
        Assert.IsTrue(LongitudinalParsers.ParseRepeatedSubjects(data).Success)
        Dim ordered = LongitudinalParsers.ParseRepeatedSubjects(data,
            New SpreadsheetRepeatedParseOptions With {.RequireAscendingVisits = True})
        Assert.IsNotNull(Issue(ordered, "repeat.visit-type"))
    End Sub

    <TestMethod>
    Public Sub Repeated_NumericVisitOrderIsPerSubjectInSourceOrder()
        Dim data = Source(New Object() {"ID", "Visit", "Y"}, New Object() {"A", 3, 1},
                          New Object() {"B", 0, 2}, New Object() {"A", 2, 3})
        Dim parsed = LongitudinalParsers.ParseRepeatedSubjects(data,
            New SpreadsheetRepeatedParseOptions With {.RequireAscendingVisits = True})
        Assert.IsNotNull(Issue(parsed, "repeat.visit-order"))
    End Sub

    <TestMethod>
    Public Sub Repeated_MissingWholeRowSkipPreservesPairs()
        Dim data = Source(New Object() {"ID", "Visit", "Y"}, New Object() {"A", 0, 4},
                          New Object() {"A", 1, Nothing}, New Object() {"A", 2, 7})
        Dim parsed = LongitudinalParsers.ParseRepeatedSubjects(data,
            New SpreadsheetRepeatedParseOptions With {.Missing = SpreadsheetIncompleteObservationPolicy.Skip})
        Assert.IsTrue(parsed.Success)
        Assert.AreEqual(2, parsed.Data.Count)
        Assert.IsNotNull(Issue(parsed, "repeat.missing-skipped"))
        Assert.AreEqual(23, parsed.Data.Observations(1).WorksheetRow)
    End Sub

    <TestMethod>
    Public Sub Repeated_CalendarVisitsRequireVerifiedHostDateOrigin()
        Dim data = Source(New Object() {"ID", "Visit", "Y"}, New Object() {"A", 1.5R, 4})
        Dim opts As New SpreadsheetRepeatedParseOptions With {.InterpretVisitAsDate = True}
        Assert.IsNotNull(Issue(LongitudinalParsers.ParseRepeatedSubjects(data, opts), "repeat.date-origin"))
        data.DateSerialOrigin = New DateTime(2001, 1, 1)
        Dim parsed = LongitudinalParsers.ParseRepeatedSubjects(data, opts)
        Assert.IsTrue(parsed.Success)
        Assert.AreEqual(New DateTime(2001, 1, 2, 12, 0, 0), parsed.Data.Observations(0).VisitDate.Value)
        data.DateSystem = SpreadsheetDateSystem.Excel1900
        Assert.IsNotNull(Issue(LongitudinalParsers.ParseRepeatedSubjects(data, opts), "repeat.date-origin"))
        data.DateSerialOrigin = Nothing
        data.DateSystem = SpreadsheetDateSystem.Excel1904
        Assert.IsTrue(LongitudinalParsers.ParseRepeatedSubjects(data, opts).Success)
    End Sub

    <TestMethod>
    Public Sub Mixed_NoVisitLmmWithGroupsAndRawCovariates()
        Dim data = Source(New Object() {"Subject", "Outcome", "Treatment", "Site"},
                          New Object() {"A", 3, "T", "North"}, New Object() {"A", 5, "C", "North"})
        Dim opts As New SpreadsheetMixedModelParseOptions With {
            .SubjectColumnIndex = 0, .VisitColumnIndex = -1, .ResponseColumnIndex = 1,
            .FixedEffectColumnIndices = New Integer() {2}, .RandomGroupColumnIndices = New Integer() {0, 3}}
        Dim parsed = LongitudinalParsers.ParseMixedModel(data, opts)
        Assert.IsTrue(parsed.Success)
        Assert.AreEqual(2, parsed.Data.Rows.Count)
        Assert.AreEqual("Treatment", parsed.Data.CovariateNames(0))
        Assert.AreEqual("Site", parsed.Data.RandomGroupNames(1))
        Assert.AreEqual(SpreadsheetCellKind.Text, parsed.Data.Rows(0).Covariates(0).Kind)
        Assert.AreEqual("T", parsed.Data.Rows(0).Covariates(0).TextValue)
        Assert.AreEqual(21, parsed.Data.Rows(0).Covariates(0).WorksheetRow)
        Assert.AreEqual(5, parsed.Data.Rows(0).Covariates(0).WorksheetColumn)
        Assert.AreEqual("North", parsed.Data.Rows(1).RandomGroups(1).TextValue)
        Assert.IsNull(parsed.Data.Rows(0).Repeated.Visit)
        data.Rows(1)(2).TextValue = "Modified"
        Assert.AreEqual("T", parsed.Data.Rows(0).Covariates(0).TextValue)
    End Sub

    <TestMethod>
    Public Sub Mixed_VisitAndCovariatePreserveHostErrorIdentityInFrame()
        Dim err = SpreadsheetCellValue.ErrorCell("HOST_ERR_47", "native-47")
        Dim data = Source(New Object() {"ID", "Visit", "Y", "X"}, New Object() {"A", 1, 5, err})
        Dim opts As New SpreadsheetMixedModelParseOptions With {.FixedEffectColumnIndices = New Integer() {3}}
        Dim parsed = LongitudinalParsers.ParseMixedModel(data, opts)
        Assert.IsNotNull(Issue(parsed, "cell.excel-error"))
        Dim frame = RectangularCategoricalParsers.ParseDataFrame(data)
        Assert.AreEqual("native-47", frame.Data.Cell(0, 3).NativeErrorCode)
        Assert.AreEqual("native-47", frame.Data.Cell(0, 3).ToCellValue().NativeErrorCode)
    End Sub

    <TestMethod>
    Public Sub Mixed_MissingFixedCovariateSkipWholeRowAndWarning()
        Dim data = Source(New Object() {"ID", "Visit", "Y", "X"},
                          New Object() {"A", 0, 2, "T"}, New Object() {"A", 1, 3, Nothing},
                          New Object() {"A", 2, 4, "C"})
        Dim opts As New SpreadsheetMixedModelParseOptions With {
            .FixedEffectColumnIndices = New Integer() {3}, .Missing = SpreadsheetIncompleteObservationPolicy.Skip}
        Dim parsed = LongitudinalParsers.ParseMixedModel(data, opts)
        Assert.IsTrue(parsed.Success)
        Assert.IsNotNull(Issue(parsed, "mixed.missing-skipped"))
        Assert.AreEqual(2, parsed.Data.Rows.Count)
        Assert.AreEqual(23, parsed.Data.Rows(1).Repeated.WorksheetRow)
    End Sub

    <TestMethod>
    Public Sub Mixed_SkippedCovariateRowDoesNotProduceFalseDuplicateVisit()
        Dim data = Source(New Object() {"ID", "Visit", "Y", "X"},
                          New Object() {"A", 0, 2, Nothing}, New Object() {"A", 0, 3, "T"})
        Dim opts As New SpreadsheetMixedModelParseOptions With {
            .FixedEffectColumnIndices = New Integer() {3}, .Missing = SpreadsheetIncompleteObservationPolicy.Skip}
        Dim parsed = LongitudinalParsers.ParseMixedModel(data, opts)
        Assert.IsTrue(parsed.Success)
        Assert.AreEqual(1, parsed.Data.Rows.Count)
        Assert.IsNull(Issue(parsed, "repeat.duplicate-visit"))
        Assert.AreEqual(22, parsed.Data.Rows(0).Repeated.WorksheetRow)
    End Sub

    <TestMethod>
    Public Sub Mixed_InvalidPrimaryIndicesAreDiagnosedBeforeInspectingCovariates()
        Dim data = Source(New Object() {"ID", "Visit", "Y", "X"}, New Object() {"A", 0, 3, Nothing})
        Dim opts As New SpreadsheetMixedModelParseOptions With {
            .ResponseColumnIndex = 99, .FixedEffectColumnIndices = New Integer() {3},
            .Missing = SpreadsheetIncompleteObservationPolicy.Reject}
        Dim parsed = LongitudinalParsers.ParseMixedModel(data, opts)
        Assert.IsFalse(parsed.Success)
        Assert.IsNotNull(Issue(parsed, "repeat.options"))
    End Sub

    <TestMethod>
    Public Sub Mixed_InvalidColumnRoleAndDuplicateFixedColumnsReported()
        Dim data = Source(New Object() {"ID", "Visit", "Y", "X"}, New Object() {"A", 0, 2, 3})
        Dim opts As New SpreadsheetMixedModelParseOptions With {.FixedEffectColumnIndices = New Integer() {3, 3}}
        Assert.IsNotNull(Issue(LongitudinalParsers.ParseMixedModel(data, opts), "mixed.columns"))
        opts.FixedEffectColumnIndices = New Integer() {2}
        Assert.IsNotNull(Issue(LongitudinalParsers.ParseMixedModel(data, opts), "mixed.columns"))
        opts.FixedEffectColumnIndices = New Integer() {4}
        Assert.IsNotNull(Issue(LongitudinalParsers.ParseMixedModel(data, opts), "mixed.columns"))
    End Sub

    <TestMethod>
    Public Sub InvalidShapeAndAmbiguousHeaderReturnDiagnosticsNotThrows()
        Dim data = Source(New Object() {"ID", "Visit", "Y"}, New Object() {"A", 0})
        Assert.IsNotNull(Issue(LongitudinalParsers.ParseRepeatedSubjects(data), "range.not-rectangular"))
        Dim proper = Source(New Object() {"ID", "Visit", "Y"}, New Object() {"A", 0, 3})
        Assert.IsNotNull(Issue(LongitudinalParsers.ParseRepeatedSubjects(proper,
            New SpreadsheetRepeatedParseOptions With {.Header = SpreadsheetHeaderMode.Auto}), "header.ambiguous"))
    End Sub

End Class
