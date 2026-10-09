Option Explicit On
Option Strict On

Imports System.Collections.Generic
Imports System.Globalization

Namespace ExcelCommon

    ''' <summary>
    ''' Parsed grouped numeric input used by one-way ANOVA and other grouped-data workflows.
    ''' </summary>
    Public Class GroupedNumericData

        Public Sub New(groupNames As String(), groups As Double()())
            If groupNames Is Nothing Then Throw New ArgumentNullException(NameOf(groupNames))
            If groups Is Nothing Then Throw New ArgumentNullException(NameOf(groups))

            Me.GroupNames = groupNames
            Me.Groups = groups
        End Sub

        Public ReadOnly Property GroupNames As String()
        Public ReadOnly Property Groups As Double()()

        Public ReadOnly Property GroupCounts As Integer()
            Get
                Dim counts(Groups.Length - 1) As Integer
                For i As Integer = 0 To Groups.Length - 1
                    counts(i) = If(Groups(i) Is Nothing, 0, Groups(i).Length)
                Next
                Return counts
            End Get
        End Property
    End Class

    ''' <summary>
    ''' Shared grouped-data parser. It deliberately knows nothing about Office.js, Excel-DNA,
    ''' COM, WinForms, or a browser. Host adapters only need to supply SpreadsheetRangeData.
    ''' </summary>
    Public NotInheritable Class GroupedNumericRangeParser

        Private Sub New()
        End Sub

        Public Shared Function Parse(input As SpreadsheetRangeData) As GroupedNumericData
            If input Is Nothing Then Throw New ArgumentNullException(NameOf(input))
            If input.Rows Is Nothing Then Throw New ArgumentException("The selected range has no row data.", NameOf(input))
            If input.ColumnCount < 2 Then Throw New ArgumentException("Select at least two group columns.", NameOf(input))
            If input.RowCount < 3 Then Throw New ArgumentException("The selection must contain a header row plus at least two data rows.", NameOf(input))
            If input.Rows.Count <> input.RowCount Then Throw New ArgumentException("The supplied row count does not match the worksheet range payload.", NameOf(input))

            For rowIndex As Integer = 0 To input.Rows.Count - 1
                Dim row As List(Of SpreadsheetCellValue) = input.Rows(rowIndex)
                If row Is Nothing OrElse row.Count <> input.ColumnCount Then
                    Throw New ArgumentException("The worksheet range payload is not rectangular.", NameOf(input))
                End If
            Next

            Dim names(input.ColumnCount - 1) As String
            Dim groups(input.ColumnCount - 1)() As Double

            For columnIndex As Integer = 0 To input.ColumnCount - 1
                names(columnIndex) = HeaderText(input.Rows(0)(columnIndex), columnIndex)

                Dim values As New List(Of Double)()
                For rowIndex As Integer = 1 To input.RowCount - 1
                    Dim cell As SpreadsheetCellValue = input.Rows(rowIndex)(columnIndex)
                    If cell Is Nothing OrElse cell.Kind = SpreadsheetCellKind.Blank Then Continue For

                    If cell.Kind <> SpreadsheetCellKind.Number Then
                        Throw New ArgumentException(
                            "Non-numeric value found in group '" & names(columnIndex) & "' at selected row " &
                            (rowIndex + 1).ToString(CultureInfo.InvariantCulture) &
                            ". Data cells must be numeric or blank.",
                            NameOf(input))
                    End If

                    If Double.IsNaN(cell.NumberValue) OrElse Double.IsInfinity(cell.NumberValue) Then
                        Throw New ArgumentException(
                            "A non-finite numeric value was found in group '" & names(columnIndex) & "'.",
                            NameOf(input))
                    End If

                    values.Add(cell.NumberValue)
                Next

                If values.Count < 2 Then
                    Throw New ArgumentException(
                        "Group '" & names(columnIndex) & "' contains fewer than two numeric observations.",
                        NameOf(input))
                End If

                groups(columnIndex) = values.ToArray()
            Next

            Return New GroupedNumericData(names, groups)
        End Function

        Private Shared Function HeaderText(cell As SpreadsheetCellValue, columnIndex As Integer) As String
            If cell Is Nothing OrElse cell.Kind = SpreadsheetCellKind.Blank Then
                Return "Group " & (columnIndex + 1).ToString(CultureInfo.InvariantCulture)
            End If

            Select Case cell.Kind
                Case SpreadsheetCellKind.Number
                    Return cell.NumberValue.ToString("G15", CultureInfo.InvariantCulture)
                Case SpreadsheetCellKind.Bool
                    Return cell.BooleanValue.ToString(CultureInfo.InvariantCulture)
                Case SpreadsheetCellKind.Text, SpreadsheetCellKind.CellError
                    Dim text As String = If(cell.TextValue, String.Empty).Trim()
                    If text.Length > 0 Then Return text
            End Select

            Return "Group " & (columnIndex + 1).ToString(CultureInfo.InvariantCulture)
        End Function
    End Class

End Namespace
