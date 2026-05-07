Option Explicit On

Imports System.Xml
Imports BESHStatNG.AppInfrastructure
Imports Microsoft.Office.Interop.Excel

''' <summary>
''' A flexible table‑construction utility supporting multi‑row top headers,
''' multi‑column left headers, body matrices, titles, footnotes, and
''' p‑value formatting metadata. Designed for statistical output tables.
''' </summary>
''' <remarks>
''' <para>
''' A <c>ResultTable</c> object stores all table components separately:
''' top headers, left headers, body matrix, titles, footnotes, and
''' p‑value column indices. The final assembled table is produced by
''' <c>returnSelf()</c>.
''' </para>
''' 
''' <para><b>Table Structure</b></para>
''' <list type="bullet">
'''   <item><description><b>HeaderTop</b>: One or more rows above the body.</description></item>
'''   <item><description><b>HeaderLeft</b>: One or more columns to the left of the body.</description></item>
'''   <item><description><b>Body</b>: A 2D matrix of values (Object).</description></item>
'''   <item><description><b>Titles</b>: Rows inserted above everything.</description></item>
'''   <item><description><b>Footnotes</b>: Rows appended below everything.</description></item>
'''   <item><description><b>PvalueColumns</b>: Column indices flagged for p‑value formatting.</description></item>
''' </list>
''' 
''' <para><b>Assembly Order</b></para>
''' <para>
''' Titles → Top Headers → Left Headers + Body → Footnotes.
''' </para>
''' 
''' <para><b>Padding Rules</b></para>
''' <para>
''' When adding headers, shorter rows are padded with blank strings either
''' to the left or right depending on <c>bPadLeft</c>.
''' </para>
''' 
''' </remarks>
Public Class ResultTable
    Private HeaderTop As List(Of String()) = New List(Of String())
    Private HeaderLeft As List(Of String()) = New List(Of String())
    Private Footnotes As List(Of String) = New List(Of String)
    Private Titles As List(Of String) = New List(Of String)
    Private PvalueColumns As List(Of Integer) = New List(Of Integer)
    Private Body(,) As Object = Nothing
    Public bLeftHeaderAdjustUp As Boolean = False

    ''' <summary>
    ''' Returns the total number of rows in the final assembled table,
    ''' including titles, top headers, body rows, and footnotes.
    ''' </summary>

    Public ReadOnly Property TotalRows() As Integer
        Get
            Dim i As Integer = 0
            If Me.Body IsNot Nothing Then i += UBound(Body, 1) + 1
            i += HeaderTop.Count
            i += Me.Footnotes.Count
            i += Me.Titles.Count
            Return i
        End Get
    End Property

    ''' <summary>
    ''' Returns the total number of columns in the final assembled table,
    ''' including left headers and body columns. Ensures at least one column
    ''' exists if only titles or footnotes are present.
    ''' </summary>

    Public ReadOnly Property TotalCols() As Integer
        Get
            Dim i As Integer = 0
            If Me.Body IsNot Nothing Then i += UBound(Body, 2) + 1
            i += HeaderLeft.Count
            If i = 0 And (Me.Titles.Count > 0 Or Me.Footnotes.Count > 0) Then i = 1
            Return i
        End Get
    End Property

    ''' <summary>
    ''' Returns the list of column indices that should be formatted as p‑values.
    ''' </summary>

    Public ReadOnly Property PvalColumns() As List(Of Integer)
        Get
            Return Me.PvalueColumns
        End Get
    End Property

    ''' <summary>
    ''' Returns the number of top‑header rows.
    ''' </summary>

    Public ReadOnly Property HeadersTopCount() As Integer
        Get
            Return Me.HeaderTop.Count
        End Get
    End Property

    ''' <summary>
    ''' Returns the number of left‑header columns.
    ''' </summary>

    Public ReadOnly Property HeadersLeftCount() As Integer
        Get
            Return Me.HeaderLeft.Count
        End Get
    End Property

    ''' <summary>
    ''' Returns the number of footnotes appended to the table.
    ''' </summary>

    Public ReadOnly Property FootersCount() As Integer
        Get
            Return Me.Footnotes.Count
        End Get
    End Property

    ''' <summary>
    ''' Returns the number of title rows inserted above the table.
    ''' </summary>

    Public ReadOnly Property TitlesCount() As Integer
        Get
            Return Me.Titles.Count
        End Get
    End Property

    ''' <summary>
    ''' Sets the body matrix of the table using a 2D Object array.
    ''' </summary>
    ''' <param name="b">A 2D Object array representing the table body.</param>

    Public Sub SetBody(b(,) As Object)
        Me.Body = b
    End Sub

    ''' <summary>
    ''' Sets the body matrix using a 1D Object array, converting it into
    ''' a single‑column 2D matrix.
    ''' </summary>
    ''' <param name="b">A 1D Object array.</param>

    Public Sub SetBody(b() As Object)
        Dim out(,) As Object
        ReDim out(UBound(b), 0)
        For i = 0 To UBound(b)
            out(i, 0) = b(i)
        Next
        Me.Body = out
    End Sub

    ''' <summary>
    ''' Sets the body matrix using a 2D Double array, converting all values
    ''' to Object for storage.
    ''' </summary>
    ''' <param name="b">A 2D Double array.</param>

    Public Sub SetBody(b(,) As Double)
        Dim out(,) As Object
        ReDim out(UBound(b, 1), UBound(b, 2))
        For i = 0 To UBound(b, 1)
            For j = 0 To UBound(b, 2)
                out(i, j) = b(i, j)
            Next
        Next
        Me.Body = out
    End Sub

    ''' <summary>
    ''' Sets the body matrix using a 2D Integer array, converting all values
    ''' to Object for storage.
    ''' </summary>
    ''' <param name="b">A 2D Integer array.</param>

    Public Sub SetBody(b(,) As Integer)
        Dim out(,) As Object
        ReDim out(UBound(b, 1), UBound(b, 2))
        For i = 0 To UBound(b, 1)
            For j = 0 To UBound(b, 2)
                out(i, j) = b(i, j)
            Next
        Next
        Me.Body = out
    End Sub

    ''' <summary>
    ''' Adds a footnote row to the bottom of the table.
    ''' </summary>
    ''' <param name="x">The footnote text.</param>

    Public Sub AddFootnote(x As String)
        Me.Footnotes.Add(x)
    End Sub

    ''' <summary>
    ''' Adds a title row to the top of the table.
    ''' </summary>
    ''' <param name="x">The title text.</param>

    Public Sub AddTitle(x As String)
        Me.Titles.Add(x)
    End Sub

    ''' <summary>
    ''' Marks a body-column index as containing p-values for statistical formatting.
    ''' </summary>
    ''' <param name="columnNumber">
    ''' One-based column number within the table body, excluding left-header columns.
    ''' For example, if the body columns are Estimate, Std. Error, z, p-value, then pass 4.
    ''' </param>
    ''' <remarks>
    ''' <para>
    ''' The value is one-based because <see cref="WriteResults.format"/> later adds the
    ''' number of left-header columns and uses Excel's one-based range indexing.
    ''' </para>
    ''' <para>
    ''' This method validates against the number of body columns.  The older implementation
    ''' accidentally used <c>UBound(Me.Body)</c>, which refers to the first dimension
    ''' (rows) and could either reject valid p-value columns or allow incorrect ones.
    ''' </para>
    ''' </remarks>
    Public Sub AddPvalueToFormat(columnNumber As Integer)
        If Me.Body Is Nothing Then Exit Sub

        Dim bodyCols As Integer = UBound(Me.Body, 2) + 1

        If columnNumber >= 1 AndAlso columnNumber <= bodyCols Then
            If Not Me.PvalueColumns.Contains(columnNumber) Then Me.PvalueColumns.Add(columnNumber)
        End If
    End Sub

    ''' <summary>
    ''' Adds a row to the top header section. If the new header is shorter
    ''' than previous headers, it is padded with blank strings either to the
    ''' left or right depending on <c>bPadLeft</c>.
    ''' </summary>
    ''' <param name="header">The header row as a string array.</param>
    ''' <param name="bPadLeft">If True, pad on the left; otherwise pad on the right.</param>

    Public Sub AddHeaderTopRow(header() As String, Optional bPadleft As Boolean = False)
        'bPadleft - if input header is shorther then the previous header items it is pad by blank strings to the right by default
        '         - specify TRUE if pading blanks to the left
        Dim tmp() As String, i As Integer, j As Integer

        If Me.HeaderTop.Count = 0 Then
            Me.HeaderTop.Add(header)
        Else
            If Me.HeaderTop.Last.Length = header.Length Then
                Me.HeaderTop.Add(header)
            ElseIf Me.HeaderTop.Last.Length > header.Length Then
                ReDim tmp(Me.HeaderTop.Last.Length - 1)

                If bPadleft Then
                    For i = 0 To UBound(header)
                        tmp(i) = header(i)
                    Next
                Else
                    j = Me.HeaderTop.Last.Length - 1
                    For i = UBound(header) To 0 Step -1
                        tmp(j) = header(i)
                        j -= 1
                    Next
                End If
                Me.HeaderTop.Add(tmp)

            ElseIf Me.HeaderTop.Last.Length < header.Length Then
                AppGlobals.BSerr.LogAndThrow(New ArgumentException("Input Header has too many elements."))
            End If
        End If
    End Sub

    ''' <summary>
    ''' Adds a column to the left header section. If the new header is shorter
    ''' than previous headers, it is padded with blank strings either to the
    ''' left or right depending on <c>bPadLeft</c>.
    ''' </summary>
    ''' <param name="header">The header column as a string array.</param>
    ''' <param name="bPadLeft">If True, pad on the left; otherwise pad on the right.</param>

    Public Sub AddHeaderLeftRow(header() As String, Optional bPadleft As Boolean = False)
        'bPadleft - if input header is shorther then the previous header items it is pad by blank strings to the right by default
        '         - specify TRUE if pading blanks to the left
        Dim tmp() As String, i As Integer, j As Integer

        If Me.HeaderLeft.Count = 0 Then
            Me.HeaderLeft.Add(header)
        Else
            If Me.HeaderLeft.Last.Length = header.Length Then
                Me.HeaderLeft.Add(header)
            ElseIf Me.HeaderLeft.Last.Length > header.Length Then
                ReDim tmp(Me.HeaderLeft.Last.Length - 1)

                If bPadleft Then
                    For i = 0 To UBound(header)
                        tmp(i) = header(i)
                    Next
                Else
                    j = Me.HeaderLeft.Last.Length - 1
                    For i = UBound(header) To 0 Step -1
                        tmp(j) = header(i)
                        j -= 1
                    Next
                End If
                Me.HeaderLeft.Add(tmp)

            ElseIf Me.HeaderLeft.Last.Length < header.Length Then
                AppGlobals.BSerr.LogAndThrow(New ApplicationException("Input Header has too many elements."))
            End If
        End If
    End Sub

    ''' <summary>
    ''' Assembles and returns the complete table as a 2D Object array,
    ''' including titles, top headers, left headers, body, and footnotes.
    ''' </summary>
    ''' <returns>A fully assembled 2D Object array representing the table.</returns>
    ''' <remarks>
    ''' <para><b>Assembly Order:</b></para>
    ''' <list type="number">
    '''   <item><description>Titles</description></item>
    '''   <item><description>Top headers</description></item>
    '''   <item><description>Left headers + body</description></item>
    '''   <item><description>Footnotes</description></item>
    ''' </list>
    ''' </remarks>

    Public Function returnSelf() As Object(,)
        Dim Out(,) As Object
        Dim i As Integer, j As Integer, ii As Integer, jj As Integer

        Dim bodyRows As Integer = If(Me.Body Is Nothing, 0, UBound(Me.Body, 1) + 1)
        Dim bodyCols As Integer = If(Me.Body Is Nothing, 0, UBound(Me.Body, 2) + 1)

        Dim nRows As Integer = Me.HeaderTop.Count + bodyRows
        Dim nCols As Integer = Me.HeaderLeft.Count + bodyCols

        'Ensure enough rows for left headers
        If Me.HeaderLeft.Count > 0 Then
            Dim maxLeftLen = Me.HeaderLeft.Max(Function(a) a.Length)
            nRows = Math.Max(nRows, maxLeftLen)
        End If

        'Ensure enough cols for top headers
        If Me.HeaderTop.Count > 0 Then
            Dim maxTopLen = Me.HeaderTop.Max(Function(a) a.Length)
            nCols = Math.Max(nCols, maxTopLen)
        End If

        If nRows < 1 Then nRows = 1
        If nCols < 1 Then nCols = 1

        ReDim Out(nRows - 1, nCols - 1)
        'Add top headers to the output
        If Me.HeaderTop.Count > 0 Then
            For i = 0 To Me.HeaderTop.Count - 1
                Dim hdr = Me.HeaderTop.Item(i)
                Dim take = Math.Min(hdr.Length, nCols)
                For k = 0 To take - 1
                    Out(i, nCols - 1 - k) = hdr(hdr.Length - 1 - k)
                Next
            Next
        End If


        'Add Left headers to the output
        If Me.HeaderLeft.Count > 0 Then
            For i = 0 To Me.HeaderLeft.Count - 1
                Dim hdr = Me.HeaderLeft.Item(i)
                Dim take = Math.Min(hdr.Length, nRows)

                If bLeftHeaderAdjustUp Then
                    For k = 0 To take - 1
                        Out(k, i) = hdr(k)
                    Next
                Else
                    For k = 0 To take - 1
                        Out(nRows - 1 - k, i) = hdr(hdr.Length - 1 - k)
                    Next
                End If
            Next
        End If


        ' Add Body (only if present)
        If Me.Body IsNot Nothing Then
            ii = If(Me.HeaderTop.Count > 0, Me.HeaderTop.Count, 0)
            For i = 0 To UBound(Me.Body, 1)
                jj = If(Me.HeaderLeft.Count > 0, Me.HeaderLeft.Count, 0)
                For j = 0 To UBound(Me.Body, 2)
                    Out(ii, jj) = Me.Body(i, j)
                    jj += 1
                Next
                ii += 1
            Next
        End If

        ' footnotes/titles stacking (your existing code)
        If Me.Footnotes.Count > 0 Then
            Dim fnotes(Me.Footnotes.Count - 1, 0) As Object
            For i = 0 To Me.Footnotes.Count - 1
                fnotes(i, 0) = Me.Footnotes(i)
            Next
            Out = Matrix.HorizontalStackArrays(Out, fnotes, True)
        End If

        If Me.Titles.Count > 0 Then
            Dim titls(Me.Titles.Count - 1, 0) As Object
            For i = 0 To Me.Titles.Count - 1
                titls(i, 0) = Me.Titles(i)
            Next
            Out = Matrix.HorizontalStackArrays(titls, Out, True)
        End If

        Return Out
    End Function

End Class


''' <summary>
''' Utility class for writing arrays, matrices, and <c>ResultTable</c> objects
''' into an Excel worksheet while maintaining row/column pointers and applying
''' optional statistical‑table formatting.
''' </summary>
''' <remarks>
''' <para>
''' The class maintains internal write pointers (<c>lastRowID</c>, <c>lastColumID</c>)
''' that determine where the next write operation begins. These pointers can be
''' set or shifted manually, and are automatically advanced after each write.
''' </para>
''' 
''' <para><b>Supported Data Types</b></para>
''' <list type="bullet">
'''   <item><description>1D arrays (horizontal or vertical)</description></item>
'''   <item><description>2D arrays</description></item>
'''   <item><description><c>ResultTable</c> objects (formatted output)</description></item>
''' </list>
''' 
''' <para><b>Formatting</b></para>
''' <para>
''' When writing a <c>ResultTable</c>, the class applies header, footer, title,
''' and p‑value formatting using the private <c>format()</c> method.
''' </para>
''' 
''' <para><b>Pointer Logic</b></para>
''' <para>
''' After writing a block of size (rows × columns), the row pointer advances by
''' <c>rows + 1</c>, allowing sequential table output without overlap.
''' </para>
''' </remarks>

Public Class WriteResults
    Public wb As Workbook
    Public ws As Worksheet

    'these should be set before .wrtie call
    Private lastRowID As Integer
    Private lastColumID As Integer

    ''' <summary>
    ''' Returns the current row pointer indicating where the next write will begin.
    ''' </summary>
    Public ReadOnly Property RowID() As Integer
        Get
            Return lastRowID
        End Get
    End Property

    ''' <summary>
    ''' Returns the current column pointer indicating where the next write will begin.
    ''' </summary>
    Public ReadOnly Property ColID() As Integer
        Get
            Return lastColumID
        End Get
    End Property

    ''' <summary>
    ''' Sets the internal row pointer to a specific row index.
    ''' </summary>
    ''' <param name="r">The row number to set (default = 1).</param>
    Sub setRowPointer(Optional r As Integer = 1)
        Me.lastRowID = r
    End Sub

    Private Shared Function NormalizeForExcel(value As Object) As Object
        If value Is Nothing Then Return Nothing
        If Not IsArray(value) Then Return NormalizeScalarForExcel(value)

        Dim arr As Array = DirectCast(value, Array)

        If arr.Rank = 1 Then
            Dim out(arr.GetUpperBound(0) - arr.GetLowerBound(0)) As Object
            For ii As Integer = arr.GetLowerBound(0) To arr.GetUpperBound(0)
                out(ii - arr.GetLowerBound(0)) = NormalizeScalarForExcel(arr.GetValue(ii))
            Next
            Return out
        ElseIf arr.Rank = 2 Then
            Dim out(arr.GetUpperBound(0) - arr.GetLowerBound(0), arr.GetUpperBound(1) - arr.GetLowerBound(1)) As Object
            For ii As Integer = arr.GetLowerBound(0) To arr.GetUpperBound(0)
                For jj As Integer = arr.GetLowerBound(1) To arr.GetUpperBound(1)
                    out(ii - arr.GetLowerBound(0), jj - arr.GetLowerBound(1)) = NormalizeScalarForExcel(arr.GetValue(ii, jj))
                Next
            Next
            Return out
        End If

        Return value
    End Function

    Private Shared Function NormalizeScalarForExcel(value As Object) As Object
        If value Is Nothing Then Return Nothing

        If TypeOf value Is Double Then
            Dim d As Double = CDbl(value)

            If Double.IsNaN(d) Then Return "#N/A"
            If Double.IsPositiveInfinity(d) Then Return "#Pinf"
            If Double.IsNegativeInfinity(d) Then Return "#Ninf"

            Return d
        End If

        If TypeOf value Is Single Then
            Dim d As Double = CDbl(value)

            If Double.IsNaN(d) Then Return "#N/A"
            If Double.IsPositiveInfinity(d) Then Return "#Pinf"
            If Double.IsNegativeInfinity(d) Then Return "#Ninf"

            Return value
        End If

        Return value
    End Function

    ''' <summary>
    ''' Shifts the internal row pointer downward by a specified number of rows.
    ''' </summary>
    ''' <param name="by">Number of rows to shift (default = 1).</param>
    Sub shiftRowPointer(Optional by As Integer = 1)
        Me.lastRowID += by
    End Sub

    ''' <summary>
    ''' Sets the internal column pointer to a specific column index.
    ''' </summary>
    ''' <param name="c">The column number to set (default = 1).</param>
    Sub setColumnPointer(Optional c As Integer = 1)
        Me.lastColumID = c
    End Sub

    ''' <summary>
    ''' Shifts the internal column pointer to the right by a specified number of columns.
    ''' </summary>
    ''' <param name="by">Number of columns to shift (default = 1).</param>
    Sub shiftColumnPointer(Optional by As Integer = 1)
        Me.lastColumID += by
    End Sub

    ''' <summary>
    ''' Initializes a new <c>WriteResults</c> instance with optional starting
    ''' row and column pointers.
    ''' </summary>
    ''' <param name="row">Initial row pointer (default = 1).</param>
    ''' <param name="col">Initial column pointer (default = 1).</param>
    Sub New(Optional row As Integer = 1, Optional col As Integer = 1)
        Me.lastRowID = row
        Me.lastColumID = col
    End Sub

    ''' <summary>
    ''' Writes data into the worksheet at the current pointer location. Supports
    ''' 1D arrays, 2D arrays, and <c>ResultTable</c> objects. Automatically advances
    ''' the row pointer after writing.
    ''' </summary>
    ''' <param name="ds">
    ''' The data source to write. May be a 1D array, 2D array, or a
    ''' <c>ResultTable</c> instance.
    ''' </param>
    ''' <param name="bTall">
    ''' If True, 1D arrays are written vertically; otherwise horizontally.
    ''' </param>
    ''' <remarks>
    ''' <para>
    ''' If <c>ds</c> is a <c>ResultTable</c>, the method extracts its assembled
    ''' matrix via <c>returnSelf()</c> and applies formatting using <c>format()</c>.
    ''' </para>
    ''' <para>
    ''' After writing, <c>lastRowID</c> is incremented by the number of rows written
    ''' plus one blank row.
    ''' </para>
    ''' <para>
    ''' Before writing to Excel, numeric non-finite values are normalized for safe display.
    ''' <c>NaN</c> values are written as <c>#N/A</c>, positive infinity as <c>#Pinf</c>,
    ''' and negative infinity as <c>#Ninf</c>.
    ''' </para>
    ''' </remarks>
    Sub write(ds As Object, Optional bTall As Boolean = False)
        'ds - data to present
        Dim rowIncr As Integer, colIncr As Integer, _ds As Object, bFormat As Boolean = False

        If ds.GetType() Is GetType(ResultTable) Then
            _ds = ds.returnSelf()
            bFormat = True 'Header footer formating is possible now
        Else
            _ds = ds
        End If

        _ds = NormalizeForExcel(_ds)

        If IsArray(_ds) Then
            If _ds.Rank = 1 Then
                If bTall Then
                    colIncr = 0
                    rowIncr = ds.Length - 1
                Else
                    colIncr = ds.Length - 1
                    rowIncr = 0
                End If
            ElseIf _ds.Rank = 2 Then
                colIncr = UBound(_ds, 2)
                rowIncr = UBound(_ds, 1)
            End If
        End If
        If bTall Then
            Me.ws.Range(ws.Cells(Me.lastRowID, Me.lastColumID), ws.Cells(Me.lastRowID + rowIncr, Me.lastColumID + colIncr)).Value = AppGlobals.app.WorksheetFunction.Transpose(_ds)
        Else
            Dim rng = Me.ws.Range(ws.Cells(Me.lastRowID, Me.lastColumID), ws.Cells(Me.lastRowID + rowIncr, Me.lastColumID + colIncr))
            rng.Value = _ds
            If bFormat Then Me.format(rng, ds.HeadersTopCount, ds.HeadersLeftCount, ds.FootersCount, ds.PvalColumns, ds.TitlesCount)
        End If
        Me.lastRowID += rowIncr + 1
    End Sub

    ''' <summary>
    ''' Applies statistical‑table formatting to a written range, including borders,
    ''' header shading, bolding, footer styling, title styling, and p‑value highlighting.
    ''' </summary>
    ''' <param name="rng">The Excel range containing the written table.</param>
    ''' <param name="hTop">Number of top‑header rows.</param>
    ''' <param name="hLeft">Number of left‑header columns.</param>
    ''' <param name="foots">Number of footer rows.</param>
    ''' <param name="Pvals">List of column indices containing p‑values.</param>
    ''' <param name="TitlesCount">Number of title rows.</param>
    ''' <remarks>
    ''' <para>
    ''' This method is intended only for formatting tables produced by
    ''' <c>ResultTable</c>. It is not applied to raw arrays.
    ''' </para>
    ''' <para>
    ''' Features include:
    ''' </para>
    ''' <list type="bullet">
    '''   <item><description>Border cleanup and reconstruction</description></item>
    '''   <item><description>Shaded and bold top headers</description></item>
    '''   <item><description>Bold left headers</description></item>
    '''   <item><description>Reduced font size for footnotes</description></item>
    '''   <item><description>Title styling with bottom border</description></item>
    '''   <item><description>Conditional p-value highlighting (p ≤ current default alpha)</description></item>
    ''' </list>
    ''' </remarks>
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
        For i = 1 To hTop
            With rng.Rows(i + TitlesCount)
                If i = hTop Then .Borders(XlBordersIndex.xlEdgeBottom).LineStyle = XlLineStyle.xlContinuous
                .Interior.Color = 14540253
                .Font.Bold = True
            End With
        Next

        'bold Left headers
        For i = 1 To hLeft - foots
            With rng.Columns(i)
                .Font.Bold = True
            End With
        Next

        'footers formating
        For i = rng.Rows.Count - foots + 1 To rng.Rows.Count
            With rng.Rows(i)
                .Font.Size = 8
            End With
        Next

        'Titles formating
        For i = 1 To TitlesCount
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

        If Pvals.Count > 0 Then
            Dim pHighlightAlpha As Double = AppGlobals.DefaultAlpha

            'highlight pvalue <= current default alpha
            For Each i In Pvals
                For j = 1 + hTop + TitlesCount To rng.Rows.Count - foots
                    Try
                        If CDbl(rng(j, i + hLeft).value) <= pHighlightAlpha Then rng(j, i + hLeft).font.color = RGB(50, 255, 50)
                    Catch
                    End Try
                Next
            Next
        End If
    End Sub
End Class


''' <summary>
''' Processes a list of <c>ResultTable</c> objects and writes them sequentially
''' to an Excel worksheet using a <c>WriteResults</c> writer. Supports optional
''' separator blocks between tables and provides utilities for computing the
''' combined row/column footprint of all tables.
''' </summary>
''' <remarks>
''' <para>
''' This class is designed for batch‑output scenarios where multiple statistical
''' tables must be written to Excel in a controlled layout. Tables may be stacked
''' vertically (“below” mode) or arranged side‑by‑side (“beside” mode).
''' </para>
''' 
''' <para><b>Separator Logic</b></para>
''' <para>
''' Custom separator matrices may be inserted between tables via
''' <c>SetSeparators()</c>. If no separators are provided, optional blank‑row
''' separation can be enabled using <c>bDefaultBlankRowSep</c> in
''' <c>writeToSheet()</c>.
''' </para>
''' 
''' <para><b>Dimension Aggregation</b></para>
''' <list type="bullet">
'''   <item><description><c>TotRows("below")</c>: sum of row counts</description></item>
'''   <item><description><c>TotRows("beside")</c>: maximum row count</description></item>
'''   <item><description><c>TotCols("below")</c>: maximum column count</description></item>
'''   <item><description><c>TotCols("beside")</c>: sum of column counts</description></item>
''' </list>
''' </remarks>
Public Class ProcessListofResultTables
    Private inList As List(Of ResultTable)
    Private pSep As List(Of Object(,)) = Nothing

    ''' <summary>
    ''' Initializes a new processor for a list of <c>ResultTable</c> objects.
    ''' </summary>
    ''' <param name="xlist">The list of tables to be processed.</param>
    Public Sub New(xlist As List(Of ResultTable))
        If xlist Is Nothing Then AppGlobals.BSerr.LogAndThrow(New ArgumentNullException(NameOf(xlist)))
        Me.inList = xlist
    End Sub


    ''' <summary>
    ''' Computes the total number of rows across all tables, depending on layout mode.
    ''' </summary>
    ''' <param name="layout">
    ''' Layout mode:
    ''' <list layout="bullet">
    '''   <item><description>"below": sum of row counts</description></item>
    '''   <item><description>"beside": maximum row count</description></item>
    ''' </list>
    ''' </param>
    ''' <returns>Total row count across all tables.</returns>
    Public ReadOnly Property TotRows(Optional layout As String = "below") As Integer
        Get
            If Me.inList Is Nothing OrElse Me.inList.Count = 0 Then Return 0
            Dim below As Boolean = String.Equals(layout, "below", StringComparison.OrdinalIgnoreCase)

            Dim nR As Integer = 0
            For Each x As ResultTable In Me.inList
                If x Is Nothing Then Continue For

                Dim t As Object(,) = x.returnSelf()
                If t Is Nothing Then Continue For  ' <-- critical

                Dim rows As Integer = UBound(t, 1) + 1
                If below Then
                    nR += rows
                Else
                    nR = Math.Max(nR, rows)
                End If
            Next
            Return nR
        End Get
    End Property

    ''' <summary>
    ''' Computes the total number of columns across all tables, depending on layout mode.
    ''' </summary>
    ''' <param name="layout">
    ''' Layout mode:
    ''' <list layout="bullet">
    '''   <item><description>"below": maximum column count</description></item>
    '''   <item><description>"beside": sum of column counts</description></item>
    ''' </list>
    ''' </param>
    ''' <returns>Total column count across all tables.</returns>
    Public ReadOnly Property TotCols(Optional layout As String = "below") As Integer
        Get
            If Me.inList Is Nothing OrElse Me.inList.Count = 0 Then Return 0
            Dim below As Boolean = String.Equals(layout, "below", StringComparison.OrdinalIgnoreCase)

            Dim nC As Integer = 0
            For Each x As ResultTable In Me.inList
                If x Is Nothing Then Continue For

                Dim t As Object(,) = x.returnSelf()
                If t Is Nothing Then Continue For  ' <-- critical

                Dim cols As Integer = UBound(t, 2) + 1
                If below Then
                    nC = Math.Max(nC, cols)
                Else
                    nC += cols
                End If
            Next
            Return nC
        End Get
    End Property

    ''' <summary>
    ''' Assigns custom separator blocks to be inserted between tables when writing
    ''' to Excel. The number of separators must be exactly one less than the number
    ''' of tables.
    ''' </summary>
    ''' <param name="x">A list of 2D Object arrays representing separator blocks.</param>
    ''' <exception cref="ArgumentException">
    ''' Thrown when the number of separators does not match <c>inList.Count - 1</c>.
    ''' </exception>
    Public Sub SetSeparators(x As List(Of Object(,)))
        If x.Count <> Me.inList.Count - 1 Then
            AppGlobals.BSerr.LogAndThrow(New ArgumentException("Incorrect list items count."))
        End If
        pSep = x
    End Sub

    ''' <summary>
    ''' Writes all tables in sequence to an Excel worksheet using the provided
    ''' <c>WriteResults</c> writer. Optional separators or blank rows may be inserted
    ''' between tables.
    ''' </summary>
    ''' <param name="w">The <c>WriteResults</c> instance responsible for writing to Excel.</param>
    ''' <param name="bDefaultBlankRowSep">
    ''' If True and no custom separators are defined, a blank row is inserted between tables.
    ''' </param>
    ''' <remarks>
    ''' <para>
    ''' If custom separators were assigned via <c>SetSeparators()</c>, they are written
    ''' between tables in the order provided. Otherwise, blank‑row separation may be used.
    ''' </para>
    ''' <para>
    ''' Each table is written using <c>w.write()</c>, which automatically advances the
    ''' writer’s row pointer.
    ''' </para>
    ''' </remarks>
    Public Sub writeToSheet(w As WriteResults, Optional bDefaultBlankRowSep As Boolean = False)
        Dim i As Integer = -1
        For Each tab In Me.inList
            If Me.pSep IsNot Nothing Then
                If i >= 0 Then w.write(Me.pSep(i))
            ElseIf bDefaultBlankRowSep And i > -1 Then
                w.shiftRowPointer()
            End If
            w.write(tab)
            i += 1
        Next
    End Sub

End Class
