Option Explicit On
Option Strict On

Imports System
Imports System.Collections.Generic
Imports System.Collections.ObjectModel
Imports System.Globalization

Namespace ExcelCommon

    ''' <summary>An immutable, lossless snapshot of a rectangular source cell.</summary>
    Public NotInheritable Class SpreadsheetFrameCell
        Public Sub New(cell As SpreadsheetCellValue, selectedRow As Integer, selectedColumn As Integer,
                       worksheetRow As Integer, worksheetColumn As Integer)
            If selectedRow < 1 OrElse selectedColumn < 1 OrElse worksheetRow < 0 OrElse worksheetColumn < 0 Then
                Throw New ArgumentOutOfRangeException(NameOf(selectedRow))
            End If
            Dim value As SpreadsheetCellValue = If(cell, SpreadsheetCellValue.BlankCell())
            If Not [Enum].IsDefined(GetType(SpreadsheetCellKind), value.Kind) Then Throw New ArgumentException("Unknown cell kind.", NameOf(cell))
            If value.Kind = SpreadsheetCellKind.Number AndAlso (Double.IsNaN(value.NumberValue) OrElse Double.IsInfinity(value.NumberValue)) Then
                Throw New ArgumentException("Non-finite spreadsheet number.", NameOf(cell))
            End If
            Me.Kind = value.Kind
            Me.NumberValue = value.NumberValue
            Me.TextValue = If(value.TextValue, String.Empty)
            Me.BooleanValue = value.BooleanValue
            Me.NumberFormat = If(value.NumberFormat, String.Empty)
            Me.NativeErrorCode = If(value.NativeErrorCode, String.Empty)
            Me.SelectedRow = selectedRow
            Me.SelectedColumn = selectedColumn
            Me.WorksheetRow = worksheetRow
            Me.WorksheetColumn = worksheetColumn
        End Sub
        Public ReadOnly Property Kind As SpreadsheetCellKind
        Public ReadOnly Property NumberValue As Double
        Public ReadOnly Property TextValue As String
        Public ReadOnly Property BooleanValue As Boolean
        Public ReadOnly Property NumberFormat As String
        Public ReadOnly Property NativeErrorCode As String
        Public ReadOnly Property SelectedRow As Integer
        Public ReadOnly Property SelectedColumn As Integer
        Public ReadOnly Property WorksheetRow As Integer
        Public ReadOnly Property WorksheetColumn As Integer
        Public Function ToCellValue() As SpreadsheetCellValue
            Return New SpreadsheetCellValue With {.Kind = Kind, .NumberValue = NumberValue,
                .TextValue = TextValue, .BooleanValue = BooleanValue, .NumberFormat = NumberFormat,
                .NativeErrorCode = NativeErrorCode}
        End Function
    End Class

    Public NotInheritable Class SpreadsheetFrameColumn
        Public Sub New(name As String, selectedColumn As Integer, worksheetColumn As Integer)
            If String.IsNullOrWhiteSpace(name) Then Throw New ArgumentException("Column name cannot be blank.", NameOf(name))
            If selectedColumn < 1 OrElse worksheetColumn < 0 Then Throw New ArgumentOutOfRangeException(NameOf(selectedColumn))
            Me.Name = name
            Me.SelectedColumn = selectedColumn
            Me.WorksheetColumn = worksheetColumn
        End Sub
        Public ReadOnly Property Name As String
        Public ReadOnly Property SelectedColumn As Integer
        Public ReadOnly Property WorksheetColumn As Integer
    End Class

    ''' <summary>Typed, defensively copied rectangle. Data rows exclude an optional heading row.</summary>
    Public NotInheritable Class SpreadsheetRectangularDataFrameInput
        Inherits SpreadsheetAnalysisInput
        Private ReadOnly _columns As ReadOnlyCollection(Of SpreadsheetFrameColumn)
        Private ReadOnly _rows As ReadOnlyCollection(Of IReadOnlyList(Of SpreadsheetFrameCell))

        Public Sub New(columns As IEnumerable(Of SpreadsheetFrameColumn), rows As IEnumerable(Of IReadOnlyList(Of SpreadsheetFrameCell)),
                       workbookName As String, worksheetName As String, worksheetId As String,
                       Optional workbookId As String = Nothing, Optional sourceAddress As String = Nothing,
                       Optional selectionId As String = Nothing, Optional selectionRevision As String = Nothing,
                       Optional dateSystem As SpreadsheetDateSystem = SpreadsheetDateSystem.Unknown)
            If columns Is Nothing Then Throw New ArgumentNullException(NameOf(columns))
            If rows Is Nothing Then Throw New ArgumentNullException(NameOf(rows))
            Dim columnCopy As New List(Of SpreadsheetFrameColumn)(columns)
            If columnCopy.Count = 0 OrElse columnCopy.Contains(Nothing) Then Throw New ArgumentException("Invalid columns.", NameOf(columns))
            Dim rowCopy As New List(Of IReadOnlyList(Of SpreadsheetFrameCell))()
            For Each row As IReadOnlyList(Of SpreadsheetFrameCell) In rows
                If row Is Nothing OrElse row.Count <> columnCopy.Count Then Throw New ArgumentException("Invalid frame row width.", NameOf(rows))
                Dim item As New List(Of SpreadsheetFrameCell)(row)
                If item.Contains(Nothing) Then Throw New ArgumentException("Frame cells cannot be null.", NameOf(rows))
                rowCopy.Add(item.AsReadOnly())
            Next
            _columns = columnCopy.AsReadOnly()
            _rows = rowCopy.AsReadOnly()
            Me.WorkbookId = If(workbookId, String.Empty)
            Me.SourceAddress = If(sourceAddress, String.Empty)
            Me.SelectionId = If(selectionId, String.Empty)
            Me.SelectionRevision = If(selectionRevision, String.Empty)
            Me.DateSystem = dateSystem
            Me.WorkbookName = If(workbookName, String.Empty)
            Me.WorksheetName = If(worksheetName, String.Empty)
            Me.WorksheetId = If(worksheetId, String.Empty)
        End Sub
        Public ReadOnly Property WorkbookId As String
        Public ReadOnly Property SourceAddress As String
        Public ReadOnly Property SelectionId As String
        Public ReadOnly Property SelectionRevision As String
        Public ReadOnly Property DateSystem As SpreadsheetDateSystem
        Public ReadOnly Property WorkbookName As String
        Public ReadOnly Property WorksheetName As String
        Public ReadOnly Property WorksheetId As String
        Public ReadOnly Property Columns As IReadOnlyList(Of SpreadsheetFrameColumn)
            Get
                Return _columns
            End Get
        End Property
        Public ReadOnly Property Rows As IReadOnlyList(Of IReadOnlyList(Of SpreadsheetFrameCell))
            Get
                Return _rows
            End Get
        End Property
        Public ReadOnly Property RowCount As Integer
            Get
                Return _rows.Count
            End Get
        End Property
        Public ReadOnly Property ColumnCount As Integer
            Get
                Return _columns.Count
            End Get
        End Property
        Public Function Cell(rowIndex As Integer, columnIndex As Integer) As SpreadsheetFrameCell
            Return _rows(rowIndex)(columnIndex)
        End Function
    End Class

    ''' <summary>Disjoint, type-tagged category identity: text "1", number 1 and TRUE never collide.</summary>
    Public NotInheritable Class SpreadsheetCategoryValue
        Public Sub New(cell As SpreadsheetCellValue, Optional ignoreTextCase As Boolean = False)
            If cell Is Nothing Then Throw New ArgumentNullException(NameOf(cell))
            Me.Kind = cell.Kind
            Select Case cell.Kind
                Case SpreadsheetCellKind.Text
                    TextValue = If(cell.TextValue, String.Empty)
                    Dim identityText As String = If(ignoreTextCase, TextValue.ToUpperInvariant(), TextValue)
                    Key = "T:" & identityText.Length.ToString(CultureInfo.InvariantCulture) & ":" & identityText
                    DisplayText = TextValue
                Case SpreadsheetCellKind.Number
                    If Double.IsNaN(cell.NumberValue) OrElse Double.IsInfinity(cell.NumberValue) Then Throw New ArgumentException("Non-finite category number.", NameOf(cell))
                    NumberValue = cell.NumberValue
                    Key = "N:" & cell.NumberValue.ToString("R", CultureInfo.InvariantCulture)
                    DisplayText = cell.NumberValue.ToString("G17", CultureInfo.InvariantCulture)
                Case SpreadsheetCellKind.Bool
                    BooleanValue = cell.BooleanValue
                    Key = If(BooleanValue, "B:1", "B:0")
                    DisplayText = If(BooleanValue, "TRUE", "FALSE")
                Case Else
                    Throw New ArgumentException("Blank and error cells cannot be category levels.", NameOf(cell))
            End Select
        End Sub
        Public ReadOnly Property Kind As SpreadsheetCellKind
        Public ReadOnly Property NumberValue As Double
        Public ReadOnly Property TextValue As String
        Public ReadOnly Property BooleanValue As Boolean
        Public ReadOnly Property Key As String
        Public ReadOnly Property DisplayText As String
    End Class

    Public NotInheritable Class SpreadsheetCategoricalObservation
        Public Sub New(level As SpreadsheetCategoryValue, selectedRow As Integer, selectedColumn As Integer,
                       worksheetRow As Integer, worksheetColumn As Integer)
            If level Is Nothing Then Throw New ArgumentNullException(NameOf(level))
            If selectedRow < 1 OrElse selectedColumn < 1 OrElse worksheetRow < 0 OrElse worksheetColumn < 0 Then Throw New ArgumentOutOfRangeException(NameOf(selectedRow))
            Me.Level = level
            Me.SelectedRow = selectedRow
            Me.SelectedColumn = selectedColumn
            Me.WorksheetRow = worksheetRow
            Me.WorksheetColumn = worksheetColumn
        End Sub
        Public ReadOnly Property Level As SpreadsheetCategoryValue
        Public ReadOnly Property SelectedRow As Integer
        Public ReadOnly Property SelectedColumn As Integer
        Public ReadOnly Property WorksheetRow As Integer
        Public ReadOnly Property WorksheetColumn As Integer
    End Class

    Public NotInheritable Class SpreadsheetCategoricalVectorInput
        Inherits SpreadsheetAnalysisInput
        Private ReadOnly _observations As ReadOnlyCollection(Of SpreadsheetCategoricalObservation)
        Private ReadOnly _levels As ReadOnlyCollection(Of SpreadsheetCategoryValue)
        Public Sub New(name As String, observations As IEnumerable(Of SpreadsheetCategoricalObservation),
                       levels As IEnumerable(Of SpreadsheetCategoryValue))
            If observations Is Nothing Then Throw New ArgumentNullException(NameOf(observations))
            If levels Is Nothing Then Throw New ArgumentNullException(NameOf(levels))
            Dim copied As New List(Of SpreadsheetCategoricalObservation)(observations)
            Dim levelCopy As New List(Of SpreadsheetCategoryValue)(levels)
            If copied.Contains(Nothing) OrElse levelCopy.Contains(Nothing) Then Throw New ArgumentException("Null observations or levels.")
            Me.Name = If(name, String.Empty)
            _observations = copied.AsReadOnly()
            _levels = levelCopy.AsReadOnly()
        End Sub
        Public ReadOnly Property Name As String
        Public ReadOnly Property Observations As IReadOnlyList(Of SpreadsheetCategoricalObservation)
            Get
                Return _observations
            End Get
        End Property
        Public ReadOnly Property Levels As IReadOnlyList(Of SpreadsheetCategoryValue)
            Get
                Return _levels
            End Get
        End Property
        Public ReadOnly Property Count As Integer
            Get
                Return _observations.Count
            End Get
        End Property
    End Class

    ''' <summary>Validated R×C nonnegative count matrix with immutable typed level labels.</summary>
    Public NotInheritable Class SpreadsheetContingencyTableInput
        Inherits SpreadsheetAnalysisInput
        Private ReadOnly _rowLevels As ReadOnlyCollection(Of SpreadsheetCategoryValue)
        Private ReadOnly _columnLevels As ReadOnlyCollection(Of SpreadsheetCategoryValue)
        Private ReadOnly _counts As Long()()

        Public Sub New(rowLevels As IEnumerable(Of SpreadsheetCategoryValue),
                       columnLevels As IEnumerable(Of SpreadsheetCategoryValue), counts As Long()())
            If rowLevels Is Nothing Then Throw New ArgumentNullException(NameOf(rowLevels))
            If columnLevels Is Nothing Then Throw New ArgumentNullException(NameOf(columnLevels))
            If counts Is Nothing Then Throw New ArgumentNullException(NameOf(counts))
            Dim rr As New List(Of SpreadsheetCategoryValue)(rowLevels)
            Dim cc As New List(Of SpreadsheetCategoryValue)(columnLevels)
            If rr.Count = 0 OrElse cc.Count = 0 OrElse rr.Contains(Nothing) OrElse cc.Contains(Nothing) OrElse counts.Length <> rr.Count Then
                Throw New ArgumentException("Contingency matrix dimensions are inconsistent.", NameOf(counts))
            End If
            Dim copiedCounts(rr.Count - 1)() As Long
            _counts = copiedCounts
            Dim total As Long = 0
            For r As Integer = 0 To rr.Count - 1
                If counts(r) Is Nothing OrElse counts(r).Length <> cc.Count Then Throw New ArgumentException("Contingency matrix is not rectangular.", NameOf(counts))
                _counts(r) = DirectCast(counts(r).Clone(), Long())
                For Each count As Long In _counts(r)
                    If count < 0 Then Throw New ArgumentOutOfRangeException(NameOf(counts), "Negative counts are invalid.")
                    total = CheckedAdd(total, count)
                Next
            Next
            _rowLevels = rr.AsReadOnly()
            _columnLevels = cc.AsReadOnly()
            GrandTotal = total
        End Sub
        Private Shared Function CheckedAdd(a As Long, b As Long) As Long
            If a > Long.MaxValue - b Then Throw New OverflowException("Contingency total exceeds Int64 capacity.")
            Return a + b
        End Function
        Public ReadOnly Property RowLevels As IReadOnlyList(Of SpreadsheetCategoryValue)
            Get
                Return _rowLevels
            End Get
        End Property
        Public ReadOnly Property ColumnLevels As IReadOnlyList(Of SpreadsheetCategoryValue)
            Get
                Return _columnLevels
            End Get
        End Property
        Public ReadOnly Property RowCount As Integer
            Get
                Return _rowLevels.Count
            End Get
        End Property
        Public ReadOnly Property ColumnCount As Integer
            Get
                Return _columnLevels.Count
            End Get
        End Property
        Public ReadOnly Property GrandTotal As Long
        Public Function Count(rowIndex As Integer, columnIndex As Integer) As Long
            Return _counts(rowIndex)(columnIndex)
        End Function
        Public ReadOnly Property Counts As Long()()
            Get
                Dim clone(_counts.Length - 1)() As Long
                For i As Integer = 0 To _counts.Length - 1
                    clone(i) = DirectCast(_counts(i).Clone(), Long())
                Next
                Return clone
            End Get
        End Property
        Public ReadOnly Property RowTotals As Long()
            Get
                Dim result(RowCount - 1) As Long
                For r As Integer = 0 To RowCount - 1
                    For c As Integer = 0 To ColumnCount - 1
                        result(r) += _counts(r)(c)
                    Next
                Next
                Return result
            End Get
        End Property
        Public ReadOnly Property ColumnTotals As Long()
            Get
                Dim result(ColumnCount - 1) As Long
                For c As Integer = 0 To ColumnCount - 1
                    For r As Integer = 0 To RowCount - 1
                        result(c) += _counts(r)(c)
                    Next
                Next
                Return result
            End Get
        End Property
    End Class

    Public NotInheritable Class SpreadsheetDataFrameParseOptions
        Public Property Header As SpreadsheetHeaderMode = SpreadsheetHeaderMode.Required
        Public Property RejectDuplicateNames As Boolean = True
    End Class

    Public NotInheritable Class SpreadsheetCategoricalVectorParseOptions
        Public Property Orientation As SpreadsheetVectorOrientation = SpreadsheetVectorOrientation.Column
        Public Property Header As SpreadsheetHeaderMode = SpreadsheetHeaderMode.None
        Public Property ColumnIndex As Integer = 0
        Public Property RowIndex As Integer = 0
        Public Property MinimumObservations As Integer = 1
        Public Property Missing As SpreadsheetIncompleteObservationPolicy = SpreadsheetIncompleteObservationPolicy.Skip
        Public Property IgnoreTextCase As Boolean = False
        Public Property CellPolicy As SpreadsheetCellPolicyOptions = New SpreadsheetCellPolicyOptions()
    End Class

    Public NotInheritable Class SpreadsheetContingencyCountParseOptions
        Public Property HasColumnHeaders As Boolean = True
        Public Property HasRowLabels As Boolean = True
        Public Property MinimumRows As Integer = 2
        Public Property MinimumColumns As Integer = 2
        Public Property TreatMissingAsZero As Boolean = False
        Public Property IgnoreTextCase As Boolean = False
        Public Property CellPolicy As SpreadsheetCellPolicyOptions = New SpreadsheetCellPolicyOptions()
    End Class

    Public NotInheritable Class SpreadsheetContingencyPairsParseOptions
        Public Property Header As SpreadsheetHeaderMode = SpreadsheetHeaderMode.None
        Public Property FirstColumnIndex As Integer = 0
        Public Property SecondColumnIndex As Integer = 1
        Public Property MinimumRows As Integer = 2
        Public Property MinimumColumns As Integer = 2
        Public Property Missing As SpreadsheetIncompleteObservationPolicy = SpreadsheetIncompleteObservationPolicy.Reject
        Public Property IgnoreTextCase As Boolean = False
        Public Property CellPolicy As SpreadsheetCellPolicyOptions = New SpreadsheetCellPolicyOptions()
    End Class
End Namespace
