Option Explicit On
Option Strict On

Imports System
Imports System.Collections.Generic
Imports System.Linq
Imports BESHStatNG.ExcelCommon
Imports BESHStatNG.WorksheetFunctions
Imports ExcelDna.Integration

''' <summary>Detached Windows host values into the canonical Common transport.
''' No controls, active application, workbook writes or owned COM wrappers.</summary>
Friend NotInheritable Class WindowsSpreadsheetTransport
    Private Sub New()
    End Sub

    Friend Shared Function CopyValues(values As Array) As Object(,)
        If values Is Nothing OrElse values.Rank <> 2 Then Throw New ArgumentException("A two-dimensional host array is required.")
        Dim result(values.GetLength(0) - 1, values.GetLength(1) - 1) As Object
        For r = 0 To result.GetLength(0) - 1
            For c = 0 To result.GetLength(1) - 1
                result(r, c) = values.GetValue(values.GetLowerBound(0) + r, values.GetLowerBound(1) + c)
            Next
        Next
        Return result
    End Function

    ''' <summary>UDF acquisition stays Excel-DNA-owned. Preflight precedes xlCoerce;
    ''' this seam invokes no callback for arrays/invalid shapes/oversized references.
    ''' Default permits one full column but never an unbounded full-sheet request.</summary>
    Friend Shared Function ReadUdfMatrix(argument As Object, coerce As Func(Of ExcelReference, Object),
                                         Optional limits As InputSizeLimits = Nothing) As Object(,)
        Dim reference = TryCast(argument, ExcelReference)
        Dim matrix = TryCast(argument, Array)
        Dim rows As Long, columns As Long
        If reference IsNot Nothing Then
            If reference.InnerReferences.Count <> 1 Then Throw New ArgumentException("One rectangular UDF reference is required.")
            rows = CLng(reference.RowLast) - reference.RowFirst + 1
            columns = CLng(reference.ColumnLast) - reference.ColumnFirst + 1
        ElseIf matrix IsNot Nothing AndAlso matrix.Rank = 2 Then
            rows = matrix.GetLength(0) : columns = matrix.GetLength(1)
        Else
            Return Nothing
        End If
        Dim policy = If(limits, New InputSizeLimits(maxRangeCells:=1048576, maxTotalCells:=1048576,
            maxChunkCells:=1048576, maxEstimatedBytes:=134217728, estimatedBytesPerCell:=64))
        Dim budget As New InputAcquisitionBudget(policy)
        budget.AddRange(rows, columns)
        If rows * columns > policy.MaxChunkCells Then
            Throw New SpreadsheetInputValidationException(New SpreadsheetValidationReport({
                New SpreadsheetValidationIssue("input.udf-response", SpreadsheetValidationSeverity.Error,
                    "The unchunked UDF payload exceeds the explicit response-cell limit.", "maxChunkCells")}))
        End If
        If reference IsNot Nothing Then
            If coerce Is Nothing Then Throw New ArgumentNullException(NameOf(coerce))
            Dim value = coerce(reference)
            budget.Checkpoint()
            matrix = TryCast(value, Array)
            If matrix Is Nothing Then
                If rows <> 1 OrElse columns <> 1 Then Throw New ArgumentException("A multi-cell reference returned scalar values.")
                Return New Object(,) {{value}}
            End If
        End If
        If matrix.Rank <> 2 OrElse matrix.GetLength(0) <> rows OrElse matrix.GetLength(1) <> columns Then Throw New ArgumentException("Coerced shape differs from the original reference.")
        Dim result = CopyValues(matrix)
        budget.Checkpoint()
        Return result
    End Function

    ''' <summary>Opaque arrays have no physical coordinates. A supplied reference is
    ''' observed Excel metadata, never derived from the dimensions of the values.</summary>
    Friend Shared Function Capture(values As Object(,), Optional reference As SpreadsheetRangeReference = Nothing,
                                   Optional dateSystem As SpreadsheetDateSystem = SpreadsheetDateSystem.Unknown) As SpreadsheetTransportFrame
        If values Is Nothing Then Throw New ArgumentNullException(NameOf(values))
        Dim snapshot As SpreadsheetRangeData
        If reference IsNot Nothing Then
            snapshot = ExcelDnaSharedApplicationAdapter.SnapshotValues(reference, values, Nothing, Nothing, dateSystem)
        Else
            snapshot = New SpreadsheetRangeData With {.RowCount = values.GetLength(0), .ColumnCount = values.GetLength(1),
                .AddressDialect = SpreadsheetAddressDialect.Unknown, .DateSystem = dateSystem}
            Dim detached = CopyValues(values)
            For r = 0 To snapshot.RowCount - 1
                Dim row As New List(Of SpreadsheetCellValue)
                For c = 0 To snapshot.ColumnCount - 1
                    row.Add(ExcelDnaSharedApplicationAdapter.ToSnapshotCell(detached(r, c), Nothing, r + 1, c + 1))
                Next
                snapshot.Rows.Add(row)
            Next
        End If
        Return Require(SpreadsheetSelectionTransport.Capture(snapshot))
    End Function

    Friend Shared Function Require(result As SpreadsheetTypedParseResult(Of SpreadsheetTransportFrame)) As SpreadsheetTransportFrame
        If Not result.Success Then Throw New SpreadsheetInputValidationException(result.Validation)
        Return result.Data
    End Function

    ''' <summary>ExcelReference coordinates are available even when xlCoerce returns
    ''' a plain array. SheetId is only an operation-local token; workbook/name and
    ''' durable identity remain unknown. Trim offsets are explicit, never guessed.</summary>
    Friend Shared Function CaptureUdf(values As Object(,), argument As Object,
                                      Optional selectedRowOffset As Integer = 0) As SpreadsheetTransportFrame
        If selectedRowOffset < 0 Then Throw New ArgumentOutOfRangeException(NameOf(selectedRowOffset))
        Dim frame = Capture(values)
        Dim reference = TryCast(argument, ExcelReference)
        If reference Is Nothing Then Return frame
        If reference.InnerReferences.Count <> 1 OrElse values.GetLength(0) + CLng(selectedRowOffset) > CLng(reference.RowLast) - reference.RowFirst + 1 OrElse
            values.GetLength(1) <> reference.ColumnLast - reference.ColumnFirst + 1 Then Throw New ArgumentException("Coerced values disagree with the selected reference.")
        Dim data = frame.Data
        data.StartRow = reference.RowFirst + 1 + selectedRowOffset
        data.StartColumn = reference.ColumnFirst + 1
        data.WorksheetId = "ExcelReference:" & reference.SheetId.ToString()
        Return Require(SpreadsheetSelectionTransport.Capture(data))
    End Function

    ''' <summary>Preserve UDF numeric eligibility, then use the same Common frame
    ''' and Core-table bridge as GUI adapters. No physical rows invented for arrays.</summary>
    Friend Shared Function NumericFrame(values As Object(,), Optional argument As Object = Nothing,
                                        Optional selectedRowOffset As Integer = 0) As SpreadsheetTransportFrame
        Dim raw = CopyValues(values)
        For r = 0 To raw.GetLength(0) - 1
            For c = 0 To raw.GetLength(1) - 1
                Dim number = ExcelArgNumeric.TryGetDouble(raw(r, c))
                raw(r, c) = If(number.HasValue, CType(number.Value, Object), Nothing)
            Next
        Next
        Return CaptureUdf(raw, argument, selectedRowOffset)
    End Function

    Friend Shared Function NumericMatrix(values As Object(,), Optional argument As Object = Nothing,
                                         Optional selectedRowOffset As Integer = 0) As Double(,)
        Dim frame = NumericFrame(values, argument, selectedRowOffset)
        Dim names = Enumerable.Range(1, values.GetLength(1)).Select(Function(i) "Series" & i.ToString(Globalization.CultureInfo.InvariantCulture)).ToArray()
        Return SpreadsheetCoreTableBridge.ToCoreDataTable(frame.Data, names).NumericMatrix
    End Function

    ''' <summary>Two references provide physical context; two coerced arrays only
    ''' promise ordinal correspondence. Never infer an array's book/sheet from its size.</summary>
    Friend Shared Function PairedArgumentsAligned(first As Object, second As Object) As Boolean
        Dim a = TryCast(first, ExcelReference), b = TryCast(second, ExcelReference)
        If a Is Nothing OrElse b Is Nothing Then Return True
        Return a.InnerReferences.Count = 1 AndAlso b.InnerReferences.Count = 1 AndAlso
            a.SheetId = b.SheetId AndAlso a.RowFirst = b.RowFirst AndAlso a.RowLast = b.RowLast
    End Function

    ''' <summary>Existing UDF numeric eligibility precedes the shared complete-pair
    ''' policy. Text/logical/error/nonfinite values remain excluded as whole pairs.
    ''' This method never independently compacts and zips two series.</summary>
    Friend Shared Function ExtractNumericPairs(first As Object(,), second As Object(,)) As List(Of Double())
        Dim result As New List(Of Double())
        If first Is Nothing OrElse second Is Nothing OrElse first.GetLength(1) <> 1 OrElse second.GetLength(1) <> 1 OrElse first.GetLength(0) <> second.GetLength(0) OrElse first.GetLength(0) = 0 Then Return result
        Dim a = CopyValues(first), b = CopyValues(second)
        Dim values(a.GetLength(0) - 1, 1) As Object
        For r = 0 To a.GetLength(0) - 1
            Dim x = ExcelArgNumeric.TryGetDouble(a(r, 0)), y = ExcelArgNumeric.TryGetDouble(b(r, 0))
            values(r, 0) = If(x.HasValue, CType(x.Value, Object), Nothing)
            values(r, 1) = If(y.HasValue, CType(y.Value, Object), Nothing)
        Next
        Dim frame = Capture(values)
        Dim parsed = TypedNumericRangeParsers.ParsePaired(frame.Data, New SpreadsheetPairedParseOptions With {
            .Missing = SpreadsheetIncompleteObservationPolicy.Skip, .WarnOnSkippedPairs = False})
        If Not parsed.Success Then
            If parsed.Validation.Issues.All(Function(i) i.Code = "paired.too-few") Then Return result
            Throw New SpreadsheetInputValidationException(parsed.Validation)
        End If
        For Each pair In parsed.Data.Pairs
            result.Add({pair.First.Value, pair.Second.Value})
        Next
        Return result
    End Function

    ''' <summary>Internal corrected-import seam. Not wired into legacy Import:
    ''' native pre-change timing disposition is required before activation.</summary>
    Friend Shared Function ImportAreas(areas As SpreadsheetRangeSetData, names As String(),
                                       alignment As SpreadsheetAreaAlignment,
                                       Optional cellPolicy As SpreadsheetCellPolicyOptions = Nothing) As CoreDataTable
        Return Require(SpreadsheetSelectionTransport.JoinColumns(areas, alignment)).ToCoreDataTable(names, cellPolicy)
    End Function
End Class
