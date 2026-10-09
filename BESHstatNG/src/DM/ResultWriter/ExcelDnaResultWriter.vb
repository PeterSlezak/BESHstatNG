Option Explicit On

Imports BESHStatNG.AppInfrastructure
Imports Microsoft.Office.Interop.Excel

''' <summary>
''' Excel-DNA/Excel Interop writer for host-neutral result output blocks.
''' </summary>
''' <remarks>
''' This class contains the Excel-specific worksheet write and formatting code that previously
''' lived inside <c>ResultTable.vb</c>. Keeping it separate allows <c>ResultTable</c> to evolve
''' into a portable result object while preserving the existing Windows Excel-DNA behavior.
''' </remarks>
Public Class ExcelDnaResultWriter
    Inherits ResultTableWriterBase

    Public wb As Workbook
    Public ws As Worksheet

    Public Sub New(Optional row As Integer = 1, Optional col As Integer = 1)
        MyBase.New(row, col)
    End Sub

    Protected Overrides Sub WriteOutputBlock(block As ResultTableOutputBlock)
        WriteOutputBlockCore(block, Nothing, Nothing, False)
    End Sub

    ''' <summary>
    ''' Writes a host-neutral shared-layer request using the already configured Windows
    ''' workbook/worksheet and row/column pointers. This is the Excel-DNA counterpart to
    ''' the Office.js writer used by the WebAssembly host.
    ''' </summary>
    Public Sub WriteSharedRequest(request As Global.BESHStatNG.ExcelCommon.ResultTableWriteRequest)
        If request Is Nothing Then Throw New ArgumentNullException(NameOf(request))

        Dim model As ResultTableOutputModel = ToOutputModel(request)
        Dim block As New ResultTableOutputBlock(Me.RowID, Me.ColID, model)

        WriteOutputBlockCore(
            block,
            request.PvalueNumberFormat,
            request.PvalueHighlightAlpha,
            request.AutoFit)

        Me.shiftRowPointer(model.RowCount)
    End Sub

    Private Sub WriteOutputBlockCore(block As ResultTableOutputBlock,
                                     pvalueNumberFormatOverride As String,
                                     pvalueAlphaOverride As Nullable(Of Double),
                                     autoFit As Boolean)
        If block Is Nothing OrElse block.Model Is Nothing Then Exit Sub
        If block.Model.Values Is Nothing OrElse block.Model.RowCount = 0 OrElse block.Model.ColumnCount = 0 Then Exit Sub

        If Me.ws Is Nothing Then Throw New InvalidOperationException("ExcelDnaResultWriter.ws must be set before writing.")

        Dim rng = Me.ws.Range(
            ws.Cells(block.StartRow, block.StartColumn),
            ws.Cells(block.EndRow, block.EndColumn))

        rng.Value = block.Model.Values

        If block.Model.IsResultTable Then
            Me.format(
                rng,
                block.Model.HeaderTopRows,
                block.Model.HeaderLeftColumns,
                block.Model.FooterRows,
                block.Model.PvalueColumns,
                block.Model.PvalueCells,
                block.Model.TitleRows,
                pvalueNumberFormatOverride,
                pvalueAlphaOverride)
        End If

        If autoFit Then
            Try
                rng.Columns.AutoFit()
            Catch ex As Exception
                CoreServices.Logger.Warn("Failed to autofit a shared result range. " & ex.Message)
            End Try
        End If
    End Sub

    Private Shared Function ToOutputModel(request As Global.BESHStatNG.ExcelCommon.ResultTableWriteRequest) As ResultTableOutputModel
        If request.Values Is Nothing OrElse request.Values.Count = 0 Then
            Throw New ArgumentException("The shared result request contains no rows.", NameOf(request))
        End If
        If request.Values(0) Is Nothing OrElse request.Values(0).Count = 0 Then
            Throw New ArgumentException("The shared result request contains no columns.", NameOf(request))
        End If

        Dim rowCount As Integer = request.Values.Count
        Dim columnCount As Integer = request.Values(0).Count
        Dim values(rowCount - 1, columnCount - 1) As Object

        For rowIndex As Integer = 0 To rowCount - 1
            Dim row As List(Of Object) = request.Values(rowIndex)
            If row Is Nothing OrElse row.Count <> columnCount Then
                Throw New ArgumentException("The shared result request is not rectangular.", NameOf(request))
            End If

            For columnIndex As Integer = 0 To columnCount - 1
                values(rowIndex, columnIndex) = row(columnIndex)
            Next
        Next

        Dim pvalueCells As New List(Of ResultTableCellAddress)()
        If request.PvalueCells IsNot Nothing Then
            For Each address As Global.BESHStatNG.ExcelCommon.ResultTableBodyCellAddress In request.PvalueCells
                pvalueCells.Add(New ResultTableCellAddress(address.BodyRow, address.BodyColumn))
            Next
        End If

        Return New ResultTableOutputModel(
            values,
            request.HeaderTopRows,
            request.HeaderLeftColumns,
            request.FooterRows,
            request.PvalueColumns,
            request.TitleRows,
            request.IsResultTable,
            pvalueCells)
    End Function


    ''' <summary>
    ''' Applies statistical-table formatting to a written Excel range, including borders,
    ''' header shading, bolding, footer styling, title styling, and p-value highlighting.
    ''' </summary>
    Private Sub format(rng As Range,
                       hTop As Integer,
                       hLeft As Integer,
                       foots As Integer,
                       Pvals As List(Of Integer),
                       PvalueCells As List(Of ResultTableCellAddress),
                       TitlesCount As Integer,
                       Optional pvalueNumberFormatOverride As String = Nothing,
                       Optional pvalueAlphaOverride As Nullable(Of Double) = Nothing)
        With rng
            'remove borders first
            .Borders(XlBordersIndex.xlInsideHorizontal).LineStyle = XlLineStyle.xlLineStyleNone
            .Borders(XlBordersIndex.xlInsideVertical).LineStyle = XlLineStyle.xlLineStyleNone
            .Borders(XlBordersIndex.xlEdgeLeft).LineStyle = XlLineStyle.xlLineStyleNone
            .Borders(XlBordersIndex.xlEdgeRight).LineStyle = XlLineStyle.xlLineStyleNone

            'set them as we want them now
            If TitlesCount = 0 Then .Borders(XlBordersIndex.xlEdgeTop).LineStyle = XlLineStyle.xlContinuous

            'set bottom border line (excluding footers)
            .Rows(.Rows.Count - foots).Borders(XlBordersIndex.xlEdgeBottom).LineStyle = XlLineStyle.xlContinuous

            'HorizontalAlignment is causing error some times. In the help it is stated that:
            'Some of these constants may not be available to you, depending on the language support (U.S. English, for example)
            'that you've selected or installed.
            Try
                .HorizontalAlignment = XlHAlign.xlHAlignLeft
                .Interior.ColorIndex = XlColorIndex.xlColorIndexNone
            Catch
            End Try

            With .Font
                .Name = "Calibri Light"
                .Size = 10
                .Strikethrough = False
                .Superscript = False
                .Subscript = False
                .OutlineFont = False
                .Shadow = False
                .Underline = XlUnderlineStyle.xlUnderlineStyleNone
                .ColorIndex = XlColorIndex.xlColorIndexAutomatic
                .TintAndShade = 0
                .ThemeFont = XlThemeFont.xlThemeFontNone
                .Italic = False
                .Bold = False
            End With
        End With

        'bold Top headers, set background color and borders
        For i As Integer = 1 To hTop
            With rng.Rows(i + TitlesCount)
                If i = hTop Then .Borders(XlBordersIndex.xlEdgeBottom).LineStyle = XlLineStyle.xlContinuous
                .Interior.Color = 14540253
                .Font.Bold = True
            End With
        Next

        'bold Left headers
        For i As Integer = 1 To hLeft - foots
            With rng.Columns(i)
                .Font.Bold = True
            End With
        Next

        'footers formatting
        For i As Integer = rng.Rows.Count - foots + 1 To rng.Rows.Count
            With rng.Rows(i)
                .Font.Size = 8
            End With
        Next

        'Titles formatting
        For i As Integer = 1 To TitlesCount
            With rng.Rows(i)
                .Font.Size = 10
                Try
                    .Interior.ColorIndex = XlColorIndex.xlColorIndexNone
                Catch
                End Try
                .Font.Bold = False
                If i = TitlesCount Then .Borders(XlBordersIndex.xlEdgeBottom).LineStyle = XlLineStyle.xlContinuous
            End With
        Next

        ApplyPValueFormatting(
            rng,
            hTop,
            hLeft,
            foots,
            Pvals,
            PvalueCells,
            TitlesCount,
            pvalueNumberFormatOverride,
            pvalueAlphaOverride)
    End Sub

    ''' <summary>
    ''' Applies display formatting and significance highlighting to p-value columns and cells.
    ''' Excel custom number formats preserve the underlying numeric values and full precision.
    ''' </summary>
    Private Sub ApplyPValueFormatting(rng As Range,
                                      hTop As Integer,
                                      hLeft As Integer,
                                      foots As Integer,
                                      pvalueColumns As List(Of Integer),
                                      pvalueCells As List(Of ResultTableCellAddress),
                                      titlesCount As Integer,
                                      Optional pvalueNumberFormatOverride As String = Nothing,
                                      Optional pvalueAlphaOverride As Nullable(Of Double) = Nothing)
        Dim hasColumns As Boolean = pvalueColumns IsNot Nothing AndAlso pvalueColumns.Count > 0
        Dim hasCells As Boolean = pvalueCells IsNot Nothing AndAlso pvalueCells.Count > 0
        If Not hasColumns AndAlso Not hasCells Then Exit Sub

        Dim firstBodyRow As Integer = 1 + hTop + titlesCount
        Dim lastBodyRow As Integer = rng.Rows.Count - foots
        If firstBodyRow > lastBodyRow Then Exit Sub

        Dim numberFormat As String = pvalueNumberFormatOverride
        If String.IsNullOrWhiteSpace(numberFormat) Then
            numberFormat = PValuePresentation.BuildExcelNumberFormat(AppGlobals.PValuePresentation)
        End If

        Dim pHighlightAlpha As Double = AppGlobals.DefaultAlpha
        If pvalueAlphaOverride.HasValue Then
            Dim requestedAlpha As Double = pvalueAlphaOverride.Value
            If Not Double.IsNaN(requestedAlpha) AndAlso Not Double.IsInfinity(requestedAlpha) AndAlso
               requestedAlpha > 0.0 AndAlso requestedAlpha < 1.0 Then
                pHighlightAlpha = requestedAlpha
            End If
        End If

        If hasColumns Then
            For Each bodyColumn As Integer In pvalueColumns
                Dim rangeColumn As Integer = bodyColumn + hLeft
                If rangeColumn < 1 OrElse rangeColumn > rng.Columns.Count Then Continue For

                Try
                    Dim firstCell As Range = DirectCast(rng.Cells(firstBodyRow, rangeColumn), Range)
                    Dim lastCell As Range = DirectCast(rng.Cells(lastBodyRow, rangeColumn), Range)
                    Dim pvalueRange As Range = Me.ws.Range(firstCell, lastCell)
                    pvalueRange.NumberFormat = numberFormat
                Catch ex As Exception
                    CoreServices.Logger.Warn("Failed to apply p-value number format to a result column. " & ex.Message)
                End Try

                For rowIndex As Integer = firstBodyRow To lastBodyRow
                    Try
                        ApplyPValueHighlight(DirectCast(rng.Cells(rowIndex, rangeColumn), Range), pHighlightAlpha)
                    Catch
                        'A non-addressable cell in a marked column is intentionally ignored.
                    End Try
                Next
            Next
        End If

        If hasCells Then
            For Each address As ResultTableCellAddress In pvalueCells
                Dim rangeRow As Integer = firstBodyRow + address.BodyRow - 1
                Dim rangeColumn As Integer = hLeft + address.BodyColumn

                If rangeRow < firstBodyRow OrElse rangeRow > lastBodyRow OrElse
                   rangeColumn < 1 OrElse rangeColumn > rng.Columns.Count Then Continue For

                Try
                    Dim pvalueCell As Range = DirectCast(rng.Cells(rangeRow, rangeColumn), Range)
                    pvalueCell.NumberFormat = numberFormat
                    ApplyPValueHighlight(pvalueCell, pHighlightAlpha)
                Catch ex As Exception
                    CoreServices.Logger.Warn("Failed to apply p-value formatting to a result cell. " & ex.Message)
                End Try
            Next
        End If
    End Sub

    Private Shared Sub ApplyPValueHighlight(cell As Range, alpha As Double)
        If cell Is Nothing Then Exit Sub

        Try
            Dim rawValue As Object = cell.Value2
            If rawValue Is Nothing OrElse TypeOf rawValue Is Boolean Then Exit Sub

            Dim pvalue As Double = Convert.ToDouble(rawValue, Globalization.CultureInfo.CurrentCulture)
            If Not Double.IsNaN(pvalue) AndAlso Not Double.IsInfinity(pvalue) AndAlso
               pvalue >= 0.0 AndAlso pvalue <= alpha Then
                cell.Font.Color = RGB(50, 255, 50)
            End If
        Catch
            'Text/error cells in a marked column are intentionally left unchanged.
        End Try
    End Sub
End Class
