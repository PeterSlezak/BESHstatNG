Option Explicit On
Option Strict On
Imports System
Imports System.Collections.Generic
Imports System.Globalization
Imports System.Runtime.InteropServices
Imports BESHStatNG.DataManagement
Imports BESHStatNG.ExcelCommon
Imports ExcelDna.Integration
Namespace Matrix
Public Module Matrix
        Public Function DotProduct(a() As Double, b() As Double) As Double
            Return MatrixArithmeticCore.DotProduct(a, b)
        End Function
        Public Function MatInv(ByVal mat(,) As Double,
                               Optional method As String = "LU",
                               ByRef Optional iErr As Integer = 0,
                               Optional bPseudInverse As Boolean = True) As Double(,)
            Return MatrixDecompositionCore.InvertMatrix(mat, method, iErr, bPseudInverse)
        End Function
        Public Function GetColumnFrom2Darray(Of T)(x(,) As T, nCol As Integer) As T()
            Return ArrayUtilities.GetColumn(x, nCol)
        End Function
        Public Function MatCovar(mat(,) As Double) As Double(,)
            Return MatrixStatisticsCore.SampleCovariance(mat)
        End Function
End Module
End Namespace
Namespace graphics
Friend Module ChartingFunc
        Public Function HistogramBinsComputation(arInput() As Double, strType As String) As Object(,)
            Return Global.BESHStatNG.HistogramBinningCore.HistogramBinsComputation(arInput, strType)
        End Function
        Function GaussOverlayComputation(arData() As Double, arBinMidVal() As Double) As Double(,)
            Return Global.BESHStatNG.HistogramBinningCore.GaussOverlayComputation(arData, arBinMidVal)
        End Function
End Module
End Namespace
Friend NotInheritable Class ExcelDnaSharedApplicationAdapter

    Friend Shared Function ValidateAcquisitionReference(reference As SpreadsheetRangeReference) As SpreadsheetRangeReference
        If reference Is Nothing Then Throw New ArgumentNullException(NameOf(reference))
        If reference.AddressDialect <> SpreadsheetAddressDialect.ExcelA1 Then Throw New ArgumentException("Windows acquisition requires Excel A1 references.")
        If Not String.IsNullOrEmpty(reference.WorkbookId) Then Throw New ArgumentException("A workbook ID cannot be verified by this Windows adapter.")
        If Not String.IsNullOrEmpty(reference.SelectionRevision) Then Throw New ArgumentException("A selection revision cannot be verified by this Windows adapter.")
        Dim normalized As SpreadsheetRangeReference = SpreadsheetRangeReference.FromAddress(reference.Address, reference.WorksheetName, reference.WorkbookName)
        If String.IsNullOrWhiteSpace(normalized.WorksheetName) Then Throw New ArgumentException("A worksheet name is required.")
        CheckCoordinate(reference.StartRow, normalized.StartRow)
        CheckCoordinate(reference.StartColumn, normalized.StartColumn)
        CheckCoordinate(reference.RowCount, normalized.RowCount)
        CheckCoordinate(reference.ColumnCount, normalized.ColumnCount)
        normalized.WorksheetId = If(reference.WorksheetId, String.Empty)
        normalized.SelectionId = If(reference.SelectionId, String.Empty)
        Return normalized
    End Function

    Private Shared Sub CheckCoordinate(requested As Integer, actual As Integer)
        If requested <> 0 AndAlso requested <> actual Then Throw New ArgumentException("Source coordinates conflict with the requested address.")
    End Sub
    Friend Shared Function SnapshotValues(reference As SpreadsheetRangeReference, values As Object,
                                         numberFormatAt As Func(Of Integer, Integer, String),
                                         errorTextAt As Func(Of Integer, Integer, String),
                                         dateSystem As SpreadsheetDateSystem,
                                         Optional budget As InputAcquisitionBudget = Nothing) As SpreadsheetRangeData
        Dim normalized As SpreadsheetRangeReference = ValidateAcquisitionReference(reference)
        Dim matrix As Array = TryCast(values, Array)
        If matrix IsNot Nothing Then
            If matrix.Rank <> 2 OrElse matrix.GetLength(0) <> normalized.RowCount OrElse matrix.GetLength(1) <> normalized.ColumnCount Then
                Throw New ArgumentException("Host values do not match the range dimensions.", NameOf(values))
            End If
        ElseIf normalized.RowCount <> 1 OrElse normalized.ColumnCount <> 1 Then
            Throw New ArgumentException("A multi-cell range requires a two-dimensional host value array.", NameOf(values))
        End If
        Dim result As New SpreadsheetRangeData With {
            .WorkbookName = normalized.WorkbookName, .WorksheetName = normalized.WorksheetName,
            .WorksheetId = normalized.WorksheetId, .Address = normalized.Address,
            .StartRow = normalized.StartRow, .StartColumn = normalized.StartColumn,
            .RowCount = normalized.RowCount, .ColumnCount = normalized.ColumnCount,
            .DateSystem = dateSystem, .SelectionId = normalized.SelectionId,
            .SelectionRevision = String.Empty
        }
        For row As Integer = 1 To result.RowCount
            If budget IsNot Nothing Then budget.Checkpoint()
            Dim cells As New List(Of SpreadsheetCellValue)()
            For column As Integer = 1 To result.ColumnCount
                Dim value As Object = If(matrix Is Nothing, values,
                    matrix.GetValue(matrix.GetLowerBound(0) + row - 1, matrix.GetLowerBound(1) + column - 1))
                Dim cell As SpreadsheetCellValue = ToSnapshotCell(value, errorTextAt, row, column)
                cell.NumberFormat = If(numberFormatAt Is Nothing, String.Empty, If(numberFormatAt(row, column), String.Empty))
                cells.Add(cell)
            Next
            result.Rows.Add(cells)
        Next
        Return WindowsSpreadsheetTransport.Require(SpreadsheetSelectionTransport.Capture(result)).Data
    End Function

    Friend Shared Function ToSnapshotCell(value As Object, errorTextAt As Func(Of Integer, Integer, String),
                                          row As Integer, column As Integer) As SpreadsheetCellValue
        If value Is Nothing OrElse TypeOf value Is DBNull OrElse TypeOf value Is ExcelEmpty OrElse TypeOf value Is ExcelMissing Then Return SpreadsheetCellValue.BlankCell()
        Dim nativeCode As String = Nothing
        If TypeOf value Is ExcelError Then
            nativeCode = CInt(DirectCast(value, ExcelError)).ToString(CultureInfo.InvariantCulture)
        ElseIf TypeOf value Is ErrorWrapper Then
            nativeCode = DirectCast(value, ErrorWrapper).ErrorCode.ToString(CultureInfo.InvariantCulture)
        ElseIf TypeOf value Is Integer AndAlso (CInt(value) And &HFFFF0000) = &H800A0000 Then
            ' VT_ERROR values returned by Excel COM use the FACILITY_CONTROL HRESULT.
            ' Preserve the supplied HRESULT, including newer Excel errors, without an error-name lookup.
            nativeCode = CInt(value).ToString(CultureInfo.InvariantCulture)
        End If
        If nativeCode IsNot Nothing Then
            Return SpreadsheetCellValue.ErrorCell(If(errorTextAt Is Nothing, String.Empty, errorTextAt(row, column)), nativeCode)
        End If
        If TypeOf value Is String Then Return SpreadsheetCellValue.TextCell(DirectCast(value, String))
        If TypeOf value Is Boolean Then Return SpreadsheetCellValue.BooleanCell(CBool(value))
        Select Case Type.GetTypeCode(value.GetType())
            Case TypeCode.Byte, TypeCode.SByte, TypeCode.Int16, TypeCode.UInt16, TypeCode.Int32, TypeCode.UInt32,
                 TypeCode.Int64, TypeCode.UInt64, TypeCode.Single, TypeCode.Double, TypeCode.Decimal
                Return SpreadsheetCellValue.NumberCell(Convert.ToDouble(value, CultureInfo.InvariantCulture))
            Case Else
                Throw New ArgumentException("Unsupported host cell value type: " & value.GetType().FullName)
        End Select
    End Function
End Class
