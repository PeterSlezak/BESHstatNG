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
                block.Model.TitleRows)
        End If
    End Sub

    ''' <summary>
    ''' Applies statistical-table formatting to a written Excel range, including borders,
    ''' header shading, bolding, footer styling, title styling, and p-value highlighting.
    ''' </summary>
    Private Sub format(rng As Range, hTop As Integer, hLeft As Integer, foots As Integer, Pvals As List(Of Integer), TitlesCount As Integer)
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

        If Pvals IsNot Nothing AndAlso Pvals.Count > 0 Then
            Dim pHighlightAlpha As Double = AppGlobals.DefaultAlpha

            'highlight pvalue <= current default alpha
            For Each i As Integer In Pvals
                For j As Integer = 1 + hTop + TitlesCount To rng.Rows.Count - foots
                    Try
                        If CDbl(rng(j, i + hLeft).value) <= pHighlightAlpha Then rng(j, i + hLeft).font.color = RGB(50, 255, 50)
                    Catch
                    End Try
                Next
            Next
        End If
    End Sub
End Class
