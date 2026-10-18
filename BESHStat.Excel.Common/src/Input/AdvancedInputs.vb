Option Explicit On
Option Strict On

Imports System
Imports System.Collections.Generic

Namespace ExcelCommon
    ''' <summary>Common row policy; indices are zero-based in the selected rectangle.</summary>
    Public Class SpreadsheetMatrixParseOptions
        Public Property Header As SpreadsheetHeaderMode = SpreadsheetHeaderMode.Required
        ''' <summary>Nothing selects every column not assigned another role. Empty is invalid.</summary>
        Public Property NumericColumnIndices As Integer() = Nothing
        Public Property MinimumRows As Integer = 1
        Public Property MinimumColumns As Integer = 1
        Public Property RequireSquare As Boolean = False
        Public Property Missing As SpreadsheetIncompleteObservationPolicy = SpreadsheetIncompleteObservationPolicy.Reject
        Public Property CellPolicy As SpreadsheetCellPolicyOptions = New SpreadsheetCellPolicyOptions()
    End Class

    Public Class SpreadsheetClassificationParseOptions
        Inherits SpreadsheetMatrixParseOptions
        Public Property ClassColumnIndex As Integer = 0
        Public Property MinimumClasses As Integer = 2
        Public Property IgnoreCategoryCase As Boolean = False
    End Class

    Public NotInheritable Class SpreadsheetPsmParseOptions
        Inherits SpreadsheetMatrixParseOptions
        Public Property TreatmentColumnIndex As Integer = 0
        ''' <summary>Defaults to numeric 1 versus numeric 0. Both arms must occur.</summary>
        Public Property TreatedLevel As SpreadsheetCategoryValue = New SpreadsheetCategoryValue(SpreadsheetCellValue.NumberCell(1))
        Public Property ControlLevel As SpreadsheetCategoryValue = New SpreadsheetCategoryValue(SpreadsheetCellValue.NumberCell(0))
    End Class

    Public Class SpreadsheetSpcParseOptions
        Inherits SpreadsheetMatrixParseOptions
        ''' <summary>-1 uses source row order. Otherwise finite coordinates must strictly increase.</summary>
        Public Property OrderColumnIndex As Integer = -1
        Public Property InterpretOrderAsDate As Boolean = False
        ''' <summary>Optional subgroup identifier; each source row is one subgroup of numeric measurements.</summary>
        Public Property SubgroupColumnIndex As Integer = -1
    End Class

    Public NotInheritable Class SpreadsheetChartSourceParseOptions
        Inherits SpreadsheetMatrixParseOptions
        ''' <summary>-1 uses source row positions; otherwise typed categories or numeric X values.</summary>
        Public Property XColumnIndex As Integer = -1
        Public Property RequireNumericX As Boolean = False
        Public Property RequireAscendingX As Boolean = False
        Public Property InterpretXAsDate As Boolean = False
        Public Property GroupColumnIndex As Integer = -1
    End Class

    ''' <summary>An aligned row of numeric observations and optional typed roles, retaining original cells.</summary>
    Public NotInheritable Class SpreadsheetAdvancedRow
        Friend Sub New(values As IEnumerable(Of SpreadsheetNumericObservation), label As SpreadsheetFrameCell,
                       group As SpreadsheetFrameCell, coordinate As SpreadsheetFrameCell,
                       coordinateValue As Nullable(Of Double), coordinateDate As Nullable(Of DateTime),
                       treated As Nullable(Of Boolean), labelLevel As SpreadsheetCategoryValue)
            Me.Values = New List(Of SpreadsheetNumericObservation)(values).AsReadOnly()
            Me.Label = label
            Me.Group = group
            Me.Coordinate = coordinate
            Me.CoordinateValue = coordinateValue
            Me.CoordinateDate = coordinateDate
            Me.IsTreated = treated
            Me.LabelLevel = labelLevel
        End Sub
        Public ReadOnly Property Values As IReadOnlyList(Of SpreadsheetNumericObservation)
        Public ReadOnly Property Label As SpreadsheetFrameCell
        ''' <summary>Class/treatment identity under the configured case policy; Label retains raw spelling.</summary>
        Public ReadOnly Property LabelLevel As SpreadsheetCategoryValue
        Public ReadOnly Property Group As SpreadsheetFrameCell
        Public ReadOnly Property Coordinate As SpreadsheetFrameCell
        Public ReadOnly Property CoordinateValue As Nullable(Of Double)
        Public ReadOnly Property CoordinateDate As Nullable(Of DateTime)
        Public ReadOnly Property IsTreated As Nullable(Of Boolean)
        Public ReadOnly Property SelectedRow As Integer
            Get
                Return Values(0).SelectedRow
            End Get
        End Property
        Public ReadOnly Property WorksheetRow As Integer
            Get
                Return Values(0).WorksheetRow
            End Get
        End Property
    End Class

    ''' <summary>Immutable numeric rectangle plus source snapshot, including omitted rows and native errors.</summary>
    Public Class SpreadsheetMatrixInput
        Inherits SpreadsheetAnalysisInput
        Friend Sub New(source As SpreadsheetRectangularDataFrameInput, columns As IEnumerable(Of SpreadsheetFrameColumn),
                       rows As IEnumerable(Of SpreadsheetAdvancedRow), Optional dateSerialOrigin As Nullable(Of DateTime) = Nothing)
            Me.Source = source
            Me.Columns = New List(Of SpreadsheetFrameColumn)(columns).AsReadOnly()
            Me.Rows = New List(Of SpreadsheetAdvancedRow)(rows).AsReadOnly()
            Me.DateSerialOrigin = dateSerialOrigin
        End Sub
        Public ReadOnly Property Source As SpreadsheetRectangularDataFrameInput
        Public ReadOnly Property DateSerialOrigin As Nullable(Of DateTime)
        Public ReadOnly Property Columns As IReadOnlyList(Of SpreadsheetFrameColumn)
        Public ReadOnly Property Rows As IReadOnlyList(Of SpreadsheetAdvancedRow)
        Public ReadOnly Property RowCount As Integer
            Get
                Return Rows.Count
            End Get
        End Property
        Public ReadOnly Property ColumnCount As Integer
            Get
                Return Columns.Count
            End Get
        End Property
        Public Function ToArray() As Double(,)
            Dim result(RowCount - 1, ColumnCount - 1) As Double
            For r As Integer = 0 To RowCount - 1
                For c As Integer = 0 To ColumnCount - 1
                    result(r, c) = Rows(r).Values(c).Value
                Next
            Next
            Return result
        End Function
    End Class

    Public NotInheritable Class SpreadsheetClassificationInput
        Inherits SpreadsheetMatrixInput
        Friend Sub New(matrix As SpreadsheetMatrixInput, levels As IEnumerable(Of SpreadsheetCategoryValue))
            MyBase.New(matrix.Source, matrix.Columns, matrix.Rows, matrix.DateSerialOrigin)
            Me.Classes = New List(Of SpreadsheetCategoryValue)(levels).AsReadOnly()
        End Sub
        Public ReadOnly Property Classes As IReadOnlyList(Of SpreadsheetCategoryValue)
    End Class

    Public NotInheritable Class SpreadsheetPsmInput
        Inherits SpreadsheetMatrixInput
        Friend Sub New(matrix As SpreadsheetMatrixInput, treated As SpreadsheetCategoryValue, control As SpreadsheetCategoryValue)
            MyBase.New(matrix.Source, matrix.Columns, matrix.Rows, matrix.DateSerialOrigin)
            Me.TreatedLevel = treated
            Me.ControlLevel = control
        End Sub
        Public ReadOnly Property TreatedLevel As SpreadsheetCategoryValue
        Public ReadOnly Property ControlLevel As SpreadsheetCategoryValue
    End Class

    Public NotInheritable Class SpreadsheetSpcInput
        Inherits SpreadsheetMatrixInput
        Friend Sub New(matrix As SpreadsheetMatrixInput)
            MyBase.New(matrix.Source, matrix.Columns, matrix.Rows, matrix.DateSerialOrigin)
        End Sub
    End Class

    Public NotInheritable Class SpreadsheetChartSourceInput
        Inherits SpreadsheetMatrixInput
        Friend Sub New(matrix As SpreadsheetMatrixInput)
            MyBase.New(matrix.Source, matrix.Columns, matrix.Rows, matrix.DateSerialOrigin)
        End Sub
    End Class
End Namespace
