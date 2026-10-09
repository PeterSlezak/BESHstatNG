Option Explicit On
Option Strict On

Imports System.Collections.Generic
Imports BESHStatNG.AppInfrastructure

Namespace ExcelCommon

    ''' <summary>
    ''' JSON/interop-friendly one-based body-cell address used by host result writers.
    ''' </summary>
    Public Class ResultTableBodyCellAddress

        Public Sub New()
        End Sub

        Public Sub New(bodyRow As Integer, bodyColumn As Integer)
            Me.BodyRow = bodyRow
            Me.BodyColumn = bodyColumn
        End Sub

        Public Property BodyRow As Integer
        Public Property BodyColumn As Integer
    End Class

    ''' <summary>
    ''' Portable write request produced from Core ResultTable metadata. The request contains
    ''' only primitive values and presentation instructions; it has no Office.js or COM dependency.
    ''' </summary>
    Public Class ResultTableWriteRequest

        Public Sub New()
            Values = New List(Of List(Of Object))()
            PvalueColumns = New List(Of Integer)()
            PvalueCells = New List(Of ResultTableBodyCellAddress)()
            PvalueNumberFormat = String.Empty
            OutputTarget = New SpreadsheetOutputTarget()
            PvalueHighlightAlpha = 0.05
            AutoFit = True
        End Sub

        Public Property Values As List(Of List(Of Object))
        Public Property HeaderTopRows As Integer
        Public Property HeaderLeftColumns As Integer
        Public Property FooterRows As Integer
        Public Property TitleRows As Integer
        Public Property IsResultTable As Boolean
        Public Property PvalueColumns As List(Of Integer)
        Public Property PvalueCells As List(Of ResultTableBodyCellAddress)
        Public Property PvalueNumberFormat As String
        Public Property PvalueHighlightAlpha As Double
        Public Property OutputTarget As SpreadsheetOutputTarget
        Public Property AutoFit As Boolean
    End Class

    Public NotInheritable Class ResultTableWriteRequestFactory

        Private Sub New()
        End Sub

        Public Shared Function Create(model As Global.BESHStatNG.ResultTableOutputModel,
                                      pvalueSettings As PValuePresentationSettings,
                                      alpha As Double,
                                      outputTarget As SpreadsheetOutputTarget) As ResultTableWriteRequest
            If model Is Nothing Then Throw New ArgumentNullException(NameOf(model))
            If model.Values Is Nothing Then Throw New ArgumentException("The result table has no values.", NameOf(model))

            Dim safeTarget As SpreadsheetOutputTarget = If(outputTarget, New SpreadsheetOutputTarget())
            safeTarget.EnsureDefaults()

            Dim safeAlpha As Double = alpha
            If Double.IsNaN(safeAlpha) OrElse Double.IsInfinity(safeAlpha) OrElse safeAlpha <= 0.0 OrElse safeAlpha >= 1.0 Then
                safeAlpha = 0.05
            End If

            Dim request As New ResultTableWriteRequest With {
                .Values = ToRows(model.Values),
                .HeaderTopRows = model.HeaderTopRows,
                .HeaderLeftColumns = model.HeaderLeftColumns,
                .FooterRows = model.FooterRows,
                .TitleRows = model.TitleRows,
                .IsResultTable = model.IsResultTable,
                .PvalueNumberFormat = Global.BESHStatNG.PValuePresentation.BuildExcelNumberFormat(pvalueSettings),
                .PvalueHighlightAlpha = safeAlpha,
                .OutputTarget = New SpreadsheetOutputTarget With {
                    .Destination = safeTarget.Destination,
                    .WorksheetName = safeTarget.WorksheetName,
                    .StartRow = safeTarget.StartRow,
                    .StartColumn = safeTarget.StartColumn
                },
                .AutoFit = True
            }

            If model.PvalueColumns IsNot Nothing Then
                For Each bodyColumn As Integer In model.PvalueColumns
                    request.PvalueColumns.Add(bodyColumn)
                Next
            End If

            If model.PvalueCells IsNot Nothing Then
                For Each address As Global.BESHStatNG.ResultTableCellAddress In model.PvalueCells
                    request.PvalueCells.Add(New ResultTableBodyCellAddress(address.BodyRow, address.BodyColumn))
                Next
            End If

            Return request
        End Function

        Private Shared Function ToRows(values As Object(,)) As List(Of List(Of Object))
            Dim rows As New List(Of List(Of Object))(values.GetLength(0))

            For rowIndex As Integer = 0 To values.GetLength(0) - 1
                Dim row As New List(Of Object)(values.GetLength(1))
                For columnIndex As Integer = 0 To values.GetLength(1) - 1
                    row.Add(NormalizeValue(values(rowIndex, columnIndex)))
                Next
                rows.Add(row)
            Next

            Return rows
        End Function

        Private Shared Function NormalizeValue(value As Object) As Object
            If value Is Nothing Then Return Nothing

            If TypeOf value Is Double Then
                Dim d As Double = DirectCast(value, Double)
                If Double.IsNaN(d) Then Return "#N/A"
                If Double.IsPositiveInfinity(d) Then Return "#Pinf"
                If Double.IsNegativeInfinity(d) Then Return "#Ninf"
                Return d
            End If

            If TypeOf value Is Single Then
                Dim f As Single = DirectCast(value, Single)
                If Single.IsNaN(f) Then Return "#N/A"
                If Single.IsPositiveInfinity(f) Then Return "#Pinf"
                If Single.IsNegativeInfinity(f) Then Return "#Ninf"
                Return f
            End If

            Return value
        End Function
    End Class

End Namespace
