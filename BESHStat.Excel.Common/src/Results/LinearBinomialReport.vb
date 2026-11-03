Option Explicit On
Option Strict On
Imports System
Imports System.Collections.Generic
Imports System.Linq

Namespace ExcelCommon
    ''' <summary>Immutable response and ordered table snapshot. All numeric/section authority is
    ''' Core wrapResults. Fresh legacy tables bridge the existing native table writer.</summary>
    Public NotInheritable Class LinearBinomialReport
        Friend Sub New(intent As LinearBinomialFitIntent, plan As RegressionReportPlan, actualAlpha As Double)
            Me.Intent = intent : Me.Report = plan : Me.ActualFitAlpha = actualAlpha
        End Sub
        Public ReadOnly Property Intent As LinearBinomialFitIntent
        Public ReadOnly Property Report As RegressionReportPlan
        Public ReadOnly Property ActualFitAlpha As Double
        Public ReadOnly Property Rows As RegressionRowOutput
            Get
                Return Report.Rows
            End Get
        End Property
        Public ReadOnly Property Document As AnalysisResultDocument
            Get
                Return Report.Document
            End Get
        End Property
        Public ReadOnly Property Convergence As RegressionConvergence
            Get
                Return Report.Convergence
            End Get
        End Property
        Public Function LegacyTables() As List(Of ResultTable)
            Dim result As New List(Of ResultTable)
            For Each section In Document.Tables
                result.Add(Rehydrate(section.Model))
            Next
            Return result
        End Function
        ' This is a table-contract bridge, not an output writer. Preserve assembled cells,
        ' including header intersections and Nothing, plus all existing formatting metadata.
        Friend Shared Function Rehydrate(m As ResultTableOutputModel) As ResultTable
            Dim t As New ResultTable With {.bLeftHeaderAdjustUp = True}
            Dim values = m.Values
            Dim firstBody = m.TitleRows + m.HeaderTopRows
            Dim endBody = m.RowCount - m.FooterRows
            Dim body(endBody - firstBody - 1, m.ColumnCount - m.HeaderLeftColumns - 1) As Object
            For r = firstBody To endBody - 1
                For c = m.HeaderLeftColumns To m.ColumnCount - 1
                    body(r - firstBody, c - m.HeaderLeftColumns) = values(r, c)
                Next
            Next
            t.SetBody(body)
            For r = 0 To m.TitleRows - 1 : t.AddTitle(DirectCast(values(r, 0), String)) : Next
            For r = m.TitleRows To firstBody - 1
                Dim header(m.ColumnCount - 1) As String
                For c = 0 To m.ColumnCount - 1 : header(c) = DirectCast(values(r, c), String) : Next
                t.AddHeaderTopRow(header)
            Next
            For c = 0 To m.HeaderLeftColumns - 1
                Dim header(endBody - m.TitleRows - 1) As String
                For r = m.TitleRows To endBody - 1 : header(r - m.TitleRows) = DirectCast(values(r, c), String) : Next
                t.AddHeaderLeftRow(header)
            Next
            For r = endBody To m.RowCount - 1 : t.AddFootnote(DirectCast(values(r, 0), String)) : Next
            For Each col In m.PvalueColumns : t.AddPvalueToFormat(col) : Next
            For Each cell In m.PvalueCells : t.AddPvalueCellToFormat(cell.BodyRow, cell.BodyColumn) : Next
            Return t
        End Function
    End Class

    ''' <summary>Post-fit classification snapshot. The caller must supply the options read at
    ''' the original post-fit boundary, never substitute the request's earlier UI snapshot.</summary>
    Public NotInheritable Class LinearBinomialClassificationPlan
        Private ReadOnly _options As RegressionOptions
        Private ReadOnly _rowIds As Integer()
        Public Sub New(request As RegressionRequest, postFitOptions As RegressionOptions, classification As RegressionClassificationReport)
            If request Is Nothing OrElse postFitOptions Is Nothing OrElse classification Is Nothing Then Throw New ArgumentNullException()
            _options = postFitOptions.Copy()
            _rowIds = request.Primary.RowIds
            Document = classification.Document
        End Sub
        Public ReadOnly Property Options As RegressionOptions
            Get
                Return _options.Copy()
            End Get
        End Property
        Public ReadOnly Property RowIds As Integer()
            Get
                Return DirectCast(_rowIds.Clone(), Integer())
            End Get
        End Property
        Public ReadOnly Property Document As AnalysisResultDocument
        ''' <summary>Native hook order after the classification tables: ROC, then calibration
        ''' when Core supplies bins. No native chart instance is retained.</summary>
        Public ReadOnly Property ChartHooks As IReadOnlyList(Of String)
            Get
                If Document.Tables.Count = 0 Then Return Array.Empty(Of String)()
                Return If(_options.Calibration, Array.AsReadOnly({"ROC", "Calibration"}), Array.AsReadOnly({"ROC"}))
            End Get
        End Property
        Public Function LegacyTables() As List(Of ResultTable)
            Return Document.Tables.Select(Function(t) LinearBinomialReport.Rehydrate(t.Model)).ToList()
        End Function
    End Class
End Namespace
