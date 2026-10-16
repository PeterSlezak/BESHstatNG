Option Explicit On
Option Strict On

Imports System
Imports System.Collections.Generic
Imports System.Globalization
Imports BESHStatNG.ExcelCommon
Imports Microsoft.VisualStudio.TestTools.UnitTesting

<TestClass>
Public Class CellValuePolicyTests

    <TestMethod>
    Public Sub DefaultPolicy_PreservesBlankVsEmptyTextAndWhitespace()
        Dim blank = SpreadsheetCellValuePolicy.Evaluate(SpreadsheetCellValue.BlankCell())
        Dim emptyText = SpreadsheetCellValuePolicy.Evaluate(SpreadsheetCellValue.TextCell(""))
        Dim spacedText = SpreadsheetCellValuePolicy.Evaluate(SpreadsheetCellValue.TextCell("  "))
        Assert.IsTrue(blank.IsMissing)
        Assert.IsFalse(emptyText.IsMissing)
        Assert.IsFalse(spacedText.IsMissing)
        Assert.IsFalse(emptyText.HasNumericValue)
        Assert.IsTrue(SpreadsheetCellValuePolicy.Evaluate(SpreadsheetCellValue.TextCell(""),
            New SpreadsheetCellPolicyOptions With {.MissingText = SpreadsheetMissingTextPolicy.BlankOrEmptyText}).IsMissing)
        Assert.IsTrue(SpreadsheetCellValuePolicy.Evaluate(SpreadsheetCellValue.TextCell("  "),
            New SpreadsheetCellPolicyOptions With {.MissingText = SpreadsheetMissingTextPolicy.BlankOrWhitespaceText}).IsMissing)
    End Sub

    <TestMethod>
    Public Sub ErrorCell_IsNotTheSameAsErrorLookingText()
        Dim errorCell = SpreadsheetCellValue.ErrorCell("#n/a")
        Dim errorResult = SpreadsheetCellValuePolicy.Evaluate(errorCell)
        Dim textResult = SpreadsheetCellValuePolicy.Evaluate(SpreadsheetCellValue.TextCell("#N/A"))
        Assert.IsTrue(errorResult.IsError)
        Assert.AreEqual("#N/A", errorResult.Error.Code)
        Assert.IsFalse(textResult.IsError)
        Assert.IsFalse(textResult.IsMissing)
        Assert.AreEqual(String.Empty, SpreadsheetExcelErrors.Canonicalize("some error"))
        Assert.AreEqual("#DIV/0!", SpreadsheetExcelErrors.Canonicalize(" #div/0! "))
        Assert.AreEqual("#SPILL!", SpreadsheetExcelErrors.Canonicalize("#spill!"))
        Assert.ThrowsException(Of ArgumentException)(Function() SpreadsheetCellValuePolicy.ReadNumeric(errorCell))
    End Sub

    <TestMethod>
    Public Sub ErrorPolicy_RejectPreserveAndMissingAreDistinct()
        Dim errorCell = SpreadsheetCellValue.ErrorCell("#VALUE!")
        Dim preserved = SpreadsheetCellValuePolicy.Evaluate(errorCell,
            New SpreadsheetCellPolicyOptions With {.Errors = SpreadsheetErrorPolicy.Preserve})
        Dim missing = SpreadsheetCellValuePolicy.Evaluate(errorCell,
            New SpreadsheetCellPolicyOptions With {.Errors = SpreadsheetErrorPolicy.TreatAsMissing})
        Assert.IsTrue(preserved.IsError)
        Assert.IsFalse(preserved.IsMissing)
        Assert.IsTrue(missing.IsMissing)
        Assert.IsTrue(missing.IsError)
    End Sub

    <TestMethod>
    Public Sub NumericValues_NonfiniteRejectedAndBooleanNotImplicitlyNumeric()
        Assert.AreEqual(4.25R, SpreadsheetCellValuePolicy.ReadNumeric(SpreadsheetCellValue.NumberCell(4.25R)), 0.0R)
        Assert.ThrowsException(Of ArgumentException)(Function() SpreadsheetCellValuePolicy.Evaluate(SpreadsheetCellValue.NumberCell(Double.NaN)))
        Assert.ThrowsException(Of ArgumentException)(Function() SpreadsheetCellValuePolicy.Evaluate(SpreadsheetCellValue.NumberCell(Double.PositiveInfinity)))
        Assert.ThrowsException(Of ArgumentException)(Function() SpreadsheetCellValuePolicy.ReadNumeric(SpreadsheetCellValue.BooleanCell(True)))
        Dim explicitBoolean As New SpreadsheetCellPolicyOptions With {.AllowBooleanNumbers = True}
        Assert.AreEqual(1.0R, SpreadsheetCellValuePolicy.ReadNumeric(SpreadsheetCellValue.BooleanCell(True), explicitBoolean), 0.0R)
        Assert.AreEqual(0.0R, SpreadsheetCellValuePolicy.ReadNumeric(SpreadsheetCellValue.BooleanCell(False), explicitBoolean), 0.0R)
    End Sub

    <TestMethod>
    Public Sub NumericText_IsStrictByDefaultAndCultureIsExplicit()
        Assert.ThrowsException(Of ArgumentException)(Function() SpreadsheetCellValuePolicy.ReadNumeric(SpreadsheetCellValue.TextCell("2.5")))
        Dim invariant As New SpreadsheetCellPolicyOptions With {.NumericText = SpreadsheetNumericTextPolicy.Invariant}
        Dim slovak As New SpreadsheetCellPolicyOptions With {
            .NumericText = SpreadsheetNumericTextPolicy.ExplicitCulture,
            .NumericCultureName = "sk-SK"
        }
        Assert.AreEqual(2.5R, SpreadsheetCellValuePolicy.ReadNumeric(SpreadsheetCellValue.TextCell("2.5"), invariant), 0.0000001R)
        Assert.AreEqual(2.5R, SpreadsheetCellValuePolicy.ReadNumeric(SpreadsheetCellValue.TextCell("2,5"), slovak), 0.0000001R)
        Assert.ThrowsException(Of ArgumentException)(Function() SpreadsheetCellValuePolicy.ReadNumeric(SpreadsheetCellValue.TextCell("2,5"), invariant))
        Assert.ThrowsException(Of ArgumentException)(Function() SpreadsheetCellValuePolicy.ReadNumeric(SpreadsheetCellValue.TextCell("1,200"), invariant))
    End Sub

    <TestMethod>
    Public Sub WorkbookDateBasis_DiffersBy1462DaysAndRoundTrips()
        Dim serial1900 As Double = ExcelDateSerialConverter.ToSerial(New DateTime(2024, 3, 1), SpreadsheetDateSystem.Excel1900)
        Dim serial1904 As Double = ExcelDateSerialConverter.ToSerial(New DateTime(2024, 3, 1), SpreadsheetDateSystem.Excel1904)
        Assert.AreEqual(1462.0R, serial1900 - serial1904, 0.0R)
        Dim actual As DateTime
        Assert.IsTrue(ExcelDateSerialConverter.TryToDateTime(serial1900, SpreadsheetDateSystem.Excel1900, actual))
        Assert.AreEqual(New DateTime(2024, 3, 1), actual)
        Assert.IsTrue(ExcelDateSerialConverter.TryToDateTime(serial1904, SpreadsheetDateSystem.Excel1904, actual))
        Assert.AreEqual(New DateTime(2024, 3, 1), actual)
        Assert.IsFalse(ExcelDateSerialConverter.TryToDateTime(serial1900, SpreadsheetDateSystem.Unknown, actual))
    End Sub

    <TestMethod>
    Public Sub Excel1900_RejectsPhantomLeapDayAndPreservesAdjacentDates()
        Dim actual As DateTime
        Assert.IsTrue(ExcelDateSerialConverter.TryToDateTime(59.0R, SpreadsheetDateSystem.Excel1900, actual))
        Assert.AreEqual(New DateTime(1900, 2, 28), actual)
        Assert.IsFalse(ExcelDateSerialConverter.TryToDateTime(60.0R, SpreadsheetDateSystem.Excel1900, actual))
        Assert.IsFalse(ExcelDateSerialConverter.TryToDateTime(60.5R, SpreadsheetDateSystem.Excel1900, actual))
        Assert.IsTrue(ExcelDateSerialConverter.TryToDateTime(61.0R, SpreadsheetDateSystem.Excel1900, actual))
        Assert.AreEqual(New DateTime(1900, 3, 1), actual)
        Assert.IsTrue(ExcelDateSerialConverter.TryToDateTime(0.0R, SpreadsheetDateSystem.Excel1904, actual))
        Assert.AreEqual(New DateTime(1904, 1, 1), actual)
        Assert.IsFalse(ExcelDateSerialConverter.TryToDateTime(Double.PositiveInfinity, SpreadsheetDateSystem.Excel1900, actual))
        Assert.IsFalse(ExcelDateSerialConverter.TryToDateTime(-1.0R, SpreadsheetDateSystem.Excel1904, actual))
        Assert.ThrowsException(Of ArgumentException)(Function() ExcelDateSerialConverter.ToSerial(New DateTime(2024, 1, 1), SpreadsheetDateSystem.Unknown))
    End Sub

    <TestMethod>
    Public Sub FormattedNumber_IsOnlyAValidDateWithExplicitDateIntentAndBasis()
        Dim cell = SpreadsheetCellValue.NumberCell(45352.0R)
        cell.NumberFormat = "yyyy-mm-dd"
        Dim regular = SpreadsheetCellValuePolicy.Evaluate(cell)
        Assert.IsTrue(regular.HasNumericValue)
        Assert.IsFalse(regular.HasDateValue)
        Dim asDate = SpreadsheetCellValuePolicy.Evaluate(cell, Nothing, SpreadsheetDateSystem.Excel1900, True)
        Assert.IsTrue(asDate.HasDateValue)
        Assert.IsFalse(asDate.HasNumericValue)
        Assert.AreEqual(New DateTime(2024, 3, 1), asDate.DateValue)
        Dim both = SpreadsheetCellValuePolicy.Evaluate(cell,
            New SpreadsheetCellPolicyOptions With {.AllowDateSerialNumbers = True}, SpreadsheetDateSystem.Excel1900, True)
        Assert.IsTrue(both.HasNumericValue)
        Assert.IsTrue(both.HasDateValue)
        Assert.ThrowsException(Of ArgumentException)(Function() SpreadsheetCellValuePolicy.Evaluate(cell, Nothing, SpreadsheetDateSystem.Unknown, True))
    End Sub

    <TestMethod>
    Public Sub CoreBridge_PreservesSourceRowsNamesKindsAndStrictNumericMask()
        Dim range As New SpreadsheetRangeData With {
            .WorksheetName = "Study Data", .Address = "C6:F7", .RowCount = 2, .ColumnCount = 4,
            .StartRow = 6, .StartColumn = 3,
            .Rows = New List(Of List(Of SpreadsheetCellValue)) From {
                New List(Of SpreadsheetCellValue) From {
                    SpreadsheetCellValue.NumberCell(2.0R), SpreadsheetCellValue.TextCell("2.5"),
                    SpreadsheetCellValue.BooleanCell(True), SpreadsheetCellValue.BlankCell()},
                New List(Of SpreadsheetCellValue) From {
                    SpreadsheetCellValue.NumberCell(4.0R), SpreadsheetCellValue.TextCell("#N/A"),
                    SpreadsheetCellValue.TextCell(""), SpreadsheetCellValue.NumberCell(8.0R)}
            }
        }
        Dim result = SpreadsheetCoreTableBridge.ToCoreDataTable(range, New String() {"A", "B", "C", "D"})
        Assert.AreEqual(6, result.RowIds(0))
        Assert.AreEqual(7, result.RowIds(1))
        Assert.AreEqual(3, result.FirstSourceColumn)
        Assert.AreEqual("Study Data", result.SourceInfo.SheetName)
        Assert.AreEqual(2.0R, result.NumericMatrix(0, 0), 0.0R)
        Assert.IsTrue(Double.IsNaN(result.NumericMatrix(0, 1)))
        Assert.IsTrue(Double.IsNaN(result.NumericMatrix(0, 2)))
        Assert.IsTrue(result.MissingMask(0, 3))
        Assert.IsFalse(result.MissingMask(1, 2))
        Assert.IsFalse(result.MissingMask(1, 1))
        Assert.AreEqual("#N/A", CStr(result.ObjectMatrix(1, 1)))
        Assert.AreEqual("A", result.ColumnNames(0))
    End Sub

    <TestMethod>
    Public Sub CoreBridge_ErrorsHaveExplicitPreserveRejectOrMissingSemantics()
        Dim range As New SpreadsheetRangeData With {
            .WorksheetName = "Data", .Address = "A1:B1", .RowCount = 1, .ColumnCount = 2,
            .Rows = New List(Of List(Of SpreadsheetCellValue)) From {
                New List(Of SpreadsheetCellValue) From {SpreadsheetCellValue.ErrorCell("#REF!"), SpreadsheetCellValue.TextCell("#REF!")}
            }
        }
        Assert.ThrowsException(Of ArgumentException)(Function() SpreadsheetCoreTableBridge.ToCoreDataTable(range, {"Error", "Literal"}))
        Dim kept = SpreadsheetCoreTableBridge.ToCoreDataTable(range, {"Error", "Literal"},
            New SpreadsheetCellPolicyOptions With {.Errors = SpreadsheetErrorPolicy.Preserve})
        Assert.IsInstanceOfType(kept.ObjectMatrix(0, 0), GetType(SpreadsheetExcelErrorValue))
        Assert.AreEqual("#REF!", DirectCast(kept.ObjectMatrix(0, 0), SpreadsheetExcelErrorValue).Code)
        Assert.IsFalse(kept.MissingMask(0, 0))
        Dim skipped = SpreadsheetCoreTableBridge.ToCoreDataTable(range, {"Error", "Literal"},
            New SpreadsheetCellPolicyOptions With {.Errors = SpreadsheetErrorPolicy.TreatAsMissing})
        Assert.IsTrue(skipped.MissingMask(0, 0))
        Assert.IsNull(skipped.ObjectMatrix(0, 0))
        Assert.IsFalse(skipped.MissingMask(0, 1))
    End Sub

    <TestMethod>
    Public Sub CoreBridge_PolicyOverridesCurrentCultureStringCoercion()
        Dim previous As CultureInfo = CultureInfo.CurrentCulture
        Try
            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("sk-SK")
            Dim input As New SpreadsheetRangeData With {
                .RowCount = 1, .ColumnCount = 1,
                .Rows = New List(Of List(Of SpreadsheetCellValue)) From {
                    New List(Of SpreadsheetCellValue) From {SpreadsheetCellValue.TextCell("2,5")}
                }
            }
            Dim strict = SpreadsheetCoreTableBridge.ToCoreDataTable(input, {"X"})
            Assert.IsTrue(Double.IsNaN(strict.NumericMatrix(0, 0)))
            Dim opted = SpreadsheetCoreTableBridge.ToCoreDataTable(input, {"X"},
                New SpreadsheetCellPolicyOptions With {
                    .NumericText = SpreadsheetNumericTextPolicy.ExplicitCulture,
                    .NumericCultureName = "sk-SK"
                })
            Assert.AreEqual(2.5R, opted.NumericMatrix(0, 0), 0.00000001R)
        Finally
            CultureInfo.CurrentCulture = previous
        End Try
    End Sub

    <TestMethod>
    Public Sub ExistingGroupedParser_StillRejectsErrorsAndAcceptsBlankData()
        Dim input As New SpreadsheetRangeData With {
            .RowCount = 3, .ColumnCount = 2,
            .Rows = New List(Of List(Of SpreadsheetCellValue)) From {
                New List(Of SpreadsheetCellValue) From {SpreadsheetCellValue.TextCell("A"), SpreadsheetCellValue.TextCell("B")},
                New List(Of SpreadsheetCellValue) From {SpreadsheetCellValue.NumberCell(2), SpreadsheetCellValue.NumberCell(3)},
                New List(Of SpreadsheetCellValue) From {SpreadsheetCellValue.NumberCell(4), SpreadsheetCellValue.NumberCell(5)}
            }
        }
        Dim result = GroupedNumericRangeParser.Parse(input)
        Assert.AreEqual(2, result.Groups.Length)
        input.Rows(1)(1) = SpreadsheetCellValue.ErrorCell("#N/A")
        Assert.ThrowsException(Of ArgumentException)(Function() GroupedNumericRangeParser.Parse(input))
    End Sub
    <TestMethod>
    Public Sub NonExcelSerialDate_RequiresExplicitOriginAndIntent()
        Dim cell = SpreadsheetCellValue.NumberCell(1.5R)
        Dim source As New SpreadsheetRangeData With {
            .WorksheetName = "Data", .DateSystem = SpreadsheetDateSystem.Unknown,
            .DateSerialOrigin = New DateTime(2001, 1, 1)
        }
        Dim number = SpreadsheetCellValuePolicy.Evaluate(cell)
        Assert.IsTrue(number.HasNumericValue)
        Assert.IsFalse(number.HasDateValue)
        Assert.ThrowsException(Of ArgumentException)(Function() SpreadsheetCellValuePolicy.Evaluate(
            cell, Nothing, source.DateSystem, True))
        Dim asDate = SpreadsheetCellValuePolicy.Evaluate(cell, Nothing, source.DateSystem, True, source.DateSerialOrigin)
        Assert.IsFalse(asDate.HasNumericValue)
        Assert.AreEqual(New DateTime(2001, 1, 2, 12, 0, 0), asDate.DateValue)
        Assert.ThrowsException(Of ArgumentException)(Function() SpreadsheetCellValuePolicy.Evaluate(
            cell, Nothing, SpreadsheetDateSystem.Excel1900, True, source.DateSerialOrigin))
    End Sub

    <TestMethod>
    Public Sub HostDateOrigin_RejectsUnverifiedOrInvalidOrigins()
        Dim d As DateTime
        Assert.IsFalse(SpreadsheetDateSerialConverter.TryToDateTime(Double.NaN, New DateTime(2000, 1, 1), d))
        Assert.IsFalse(SpreadsheetDateSerialConverter.TryToDateTime(Double.PositiveInfinity, New DateTime(2000, 1, 1), d))
        Assert.IsFalse(SpreadsheetDateSerialConverter.TryToDateTime(1.0R, New DateTime(2000, 1, 1, 10, 0, 0), d))
        Assert.IsFalse(SpreadsheetDateSerialConverter.TryToDateTime(1.0R, DateTime.MaxValue.Date, d))
        Assert.IsTrue(SpreadsheetDateSerialConverter.TryToDateTime(-1.0R, New DateTime(2000, 1, 1), d))
        Assert.AreEqual(New DateTime(1999, 12, 31), d)
        Assert.IsFalse(ExcelDateSerialConverter.TryToDateTime(1.0R, SpreadsheetDateSystem.Unknown, d))
    End Sub

    <TestMethod>
    Public Sub NativeErrorCode_IsPreservedWithoutInventingExcelErrorCode()
        Dim input = SpreadsheetCellValue.ErrorCell("HOST_ERR_47", "error:47")
        Dim result = SpreadsheetCellValuePolicy.Evaluate(input,
            New SpreadsheetCellPolicyOptions With {.Errors = SpreadsheetErrorPolicy.Preserve})
        Assert.IsTrue(result.IsError)
        Assert.AreEqual("HOST_ERR_47", result.Error.RawText)
        Assert.AreEqual("error:47", result.Error.NativeCode)
        Assert.AreEqual(String.Empty, result.Error.Code)
        Dim literal = SpreadsheetCellValuePolicy.Evaluate(SpreadsheetCellValue.TextCell("HOST_ERR_47"))
        Assert.IsFalse(literal.IsError)
        Assert.AreEqual("#N/A", SpreadsheetCellValuePolicy.Evaluate(SpreadsheetCellValue.ErrorCell("#N/A")).Error.Code)
    End Sub

End Class
