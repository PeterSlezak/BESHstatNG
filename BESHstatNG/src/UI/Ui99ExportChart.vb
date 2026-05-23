Imports System.Drawing
Imports System.Threading
Imports System.Windows.Forms
Imports BESHStatNG.AppInfrastructure
Imports ExcelDna.Integration
Imports Excel = Microsoft.Office.Interop.Excel

Public Class Ui99ExportChart

    ' --- Chart size refresh + aspect ratio support ---
    Private _suppressAspectUpdate As Boolean = False
    Private _aspectWH As Double = 1.0 ' width/height
    Private _loading As Boolean = False

    ' Represents one entry in cbSheets
    Private Class SheetEntry
        Public Property Kind As String  ' "worksheet" or "chartsheet"
        Public Property Name As String  ' display name
        Public Property SheetObject As Object ' Excel.Worksheet or Excel.Chart
        Public Overrides Function ToString() As String
            Return Name
        End Function
    End Class

    ' Represents one entry in cbCharts
    Private Class ChartEntry
        Public Property Name As String                  ' display
        Public Property Chart As Excel.Chart            ' the chart (always available)
        Public Property WidthPts As Double              ' points
        Public Property HeightPts As Double             ' points

        ' NEW: where it comes from
        Public Property IsChartSheet As Boolean
        Public Property Worksheet As Excel.Worksheet    ' only for embedded charts
        Public Property ChartObjectName As String       ' only for embedded charts

        Public Overrides Function ToString() As String
            Return Name
        End Function
    End Class


    ' Call this from Ribbon BEFORE showing the form
    Public Shared Function WorkbookHasAnyCharts(app As Excel.Application) As Boolean
        If app Is Nothing Then Return False
        Dim wb As Excel.Workbook = Nothing
        Try
            wb = app.ActiveWorkbook
            If wb Is Nothing Then Return False

            ' 1) Chart sheets (Workbook.Charts)
            If wb.Charts IsNot Nothing AndAlso wb.Charts.Count > 0 Then Return True

            ' 2) Embedded charts (Worksheet.ChartObjects)
            For Each ws As Excel.Worksheet In wb.Worksheets
                Try
                    Dim cos As Excel.ChartObjects = CType(ws.ChartObjects(), Excel.ChartObjects)
                    If cos IsNot Nothing AndAlso cos.Count > 0 Then Return True
                Catch
                End Try
            Next

            Return False
        Catch
            Return False
        End Try
    End Function

    Sub New()

        ' This call is required by the designer.
        InitializeComponent()

        ' Add any initialization after the InitializeComponent() call.
        Me.cbFormat.DropDownStyle = ComboBoxStyle.DropDownList
        Me.cbFormat.DataSource = [Enum].GetValues(GetType(ChartExport.ExportFormat))
        Me.cbFormat.SelectedItem = ChartExport.ExportFormat.PNG
        Me.WireHelp(Me.btnHelp)
    End Sub


    ' Form init: populate combos once shown
    Private Sub Ui99ExportChart_Shown(sender As Object, e As System.EventArgs) Handles Me.Shown
        ExcelAsyncUtil.QueueAsMacro(Sub()
                                        LoadSheetsAndCharts_SelectInitial()
                                        UpdateJpgQualityUi()
                                        UpdateVectorRasterUi()
                                        UpdateDpiUi()
                                    End Sub)
    End Sub


    ' Helper: get currently selected chart for export
    Private Function GetSelectedChartForExport() As Excel.Chart
        Dim ce As ChartEntry = TryCast(cbCharts.SelectedItem, ChartEntry)
        If ce Is Nothing Then Return Nothing
        Return ce.Chart
    End Function

    Private Sub SetNumericSafe(nud As NumericUpDown, value As Integer)
        Dim v As Decimal = CDec(value)
        If v < nud.Minimum Then v = nud.Minimum
        If v > nud.Maximum Then v = nud.Maximum
        nud.Value = v
    End Sub

    ' Enable JPG quality controls only when JPG is selected
    Private Sub UpdateJpgQualityUi()
        Dim isJpg As Boolean = False

        Try
            If cbFormat.SelectedItem IsNot Nothing Then
                If TypeOf cbFormat.SelectedItem Is ChartExport.ExportFormat Then
                    Dim fmt As ChartExport.ExportFormat = CType(cbFormat.SelectedItem, ChartExport.ExportFormat)
                    isJpg = (fmt = ChartExport.ExportFormat.JPG)
                Else
                    ' Fallback if the combo holds strings
                    Dim s As String = cbFormat.SelectedItem.ToString()
                    isJpg = s.Equals("JPG", StringComparison.OrdinalIgnoreCase) OrElse
                        s.Equals("JPEG", StringComparison.OrdinalIgnoreCase)
                End If
            End If
        Catch
            isJpg = False
        End Try

        lblJPGquality.Enabled = isJpg
        spinBtnJPGquality.Enabled = isJpg
    End Sub

    Private Sub UpdateDpiUi()
        Dim fmt As ChartExport.ExportFormat = CType(Me.cbFormat.SelectedItem, ChartExport.ExportFormat)
        Dim isGif As Boolean = (fmt = ChartExport.ExportFormat.GIF)
        Dim isEmf As Boolean = (fmt = ChartExport.ExportFormat.EMF)

        ' If your label name differs, rename lblDPI accordingly.
        Me.lblDPI.Enabled = Not isGif And Not isEmf
        Me.spinBtnDPI.Enabled = Not isGif And Not isEmf
    End Sub

    ' Fill spinBtnWidth/spinBtnHeight with EXPORT pixel size at chosen DPI
    Private Sub UpdateSizeFromSelectedChart()
        Dim ce As ChartEntry = TryCast(cbCharts.SelectedItem, ChartEntry)
        If ce Is Nothing Then Return

        Dim dpi As Integer = CInt(spinBtnDPI.Value)

        Dim wPx As Integer = Math.Max(1, CInt(Math.Round((ce.WidthPts / 72.0R) * dpi)))
        Dim hPx As Integer = Math.Max(1, CInt(Math.Round((ce.HeightPts / 72.0R) * dpi)))

        If wPx < CInt(spinBtnWidth.Value) And hPx < CInt(spinBtnHeight.Value) Then
            'if it should lead to reduced picture size the skip it. It saves me time during batch exprting.
            Exit Sub
        End If

        _aspectWH = wPx / CDbl(hPx)

        _suppressAspectUpdate = True
        Try
            SetNumericSafe(spinBtnWidth, wPx)
            SetNumericSafe(spinBtnHeight, hPx)
        Finally
            _suppressAspectUpdate = False
        End Try
    End Sub

    Private Sub ActivateSelectedSheetInExcel()
        Dim se As SheetEntry = TryCast(cbSheets.SelectedItem, SheetEntry)
        If se Is Nothing Then Return

        ExcelAsyncUtil.QueueAsMacro(Sub()
                                        Try
                                            If se.Kind = "worksheet" Then
                                                Dim ws As Excel.Worksheet = CType(se.SheetObject, Excel.Worksheet)
                                                ws.Activate()
                                            ElseIf se.Kind = "chartsheet" Then
                                                Dim ch As Excel.Chart = CType(se.SheetObject, Excel.Chart)
                                                ch.Activate()
                                            End If
                                        Catch
                                            ' ignore activation failures
                                        End Try
                                    End Sub)
    End Sub

    Private Sub ActivateSelectedChartInExcel()
        Dim ce As ChartEntry = TryCast(cbCharts.SelectedItem, ChartEntry)
        If ce Is Nothing Then Return

        ExcelAsyncUtil.QueueAsMacro(Sub()
                                        Try
                                            If ce.IsChartSheet Then
                                                ' Chart sheet itself
                                                ce.Chart.Activate()
                                            Else
                                                ' Embedded chart: activate worksheet, then the chart object
                                                If ce.Worksheet IsNot Nothing Then ce.Worksheet.Activate()

                                                If ce.Worksheet IsNot Nothing AndAlso Not String.IsNullOrEmpty(ce.ChartObjectName) Then
                                                    Dim cos As Excel.ChartObjects = CType(ce.Worksheet.ChartObjects(), Excel.ChartObjects)
                                                    Dim co As Excel.ChartObject = cos.Item(ce.ChartObjectName)
                                                    co.Activate()
                                                    ' Optional: ensure it becomes the selection
                                                    co.Select()
                                                Else
                                                    ' Fallback (sometimes works)
                                                    ce.Chart.Activate()
                                                End If
                                            End If
                                        Catch
                                            ' ignore activation failures
                                        End Try
                                    End Sub)
    End Sub

    Private Sub UpdateVectorRasterUi()
        Dim fmt As ChartExport.ExportFormat = CType(cbFormat.SelectedItem, ChartExport.ExportFormat)
        Dim isEmf As Boolean = (fmt = ChartExport.ExportFormat.EMF)

        ' EMF ignores raster sizing/DPI
        lblDPI.Enabled = Not isEmf
        spinBtnDPI.Enabled = Not isEmf
        lblWidth.Enabled = Not isEmf
        spinBtnWidth.Enabled = Not isEmf
        lblHeight.Enabled = Not isEmf
        spinBtnHeight.Enabled = Not isEmf
        cbAspectRatio.Enabled = Not isEmf

        ' Your JPG quality controls
        Dim isJpg As Boolean = (fmt = ChartExport.ExportFormat.JPG)
        lblJPGquality.Enabled = isJpg
        spinBtnJPGquality.Enabled = isJpg
    End Sub

    ' Determines what should be selected initially 
    Private Sub GetInitialSelection(ByRef sheetKind As String,
                                ByRef sheetName As String,
                                ByRef chartName As String)

        sheetKind = Nothing
        sheetName = Nothing
        chartName = Nothing

        Dim app As Excel.Application = AppGlobals.app
        If app Is Nothing OrElse app.ActiveWorkbook Is Nothing Then Return

        ' Rule 1: If a chart is active (embedded or chart sheet), select it
        Try
            Dim activeChart As Excel.Chart = app.ActiveChart
            If activeChart IsNot Nothing Then
                ' Chart sheet?
                Try
                    Dim activeSheetChart As Excel.Chart = TryCast(app.ActiveSheet, Excel.Chart)
                    If activeSheetChart IsNot Nothing Then
                        sheetKind = "chartsheet"
                        sheetName = activeSheetChart.Name
                        chartName = activeSheetChart.Name
                        Return
                    End If
                Catch
                End Try

                ' Embedded chart: best signal is Selection as ChartObject
                Try
                    Dim sel As Object = app.Selection
                    If sel IsNot Nothing AndAlso TypeOf sel Is Excel.ChartObject Then
                        Dim co As Excel.ChartObject = DirectCast(sel, Excel.ChartObject)
                        sheetKind = "worksheet"
                        sheetName = co.Parent.Name   ' worksheet name
                        chartName = co.Name          ' chartobject name
                        Return
                    End If
                Catch
                End Try

                ' Fallback: use active sheet + chart name (may match ChartObject name)
                Try
                    Dim ws As Excel.Worksheet = TryCast(app.ActiveSheet, Excel.Worksheet)
                    If ws IsNot Nothing Then
                        sheetKind = "worksheet"
                        sheetName = ws.Name
                        chartName = activeChart.Name
                        Return
                    End If
                Catch
                End Try
            End If
        Catch
        End Try

        ' Rule 2: if active sheet has embedded charts, select active sheet + first chart
        Try
            Dim ws As Excel.Worksheet = TryCast(app.ActiveSheet, Excel.Worksheet)
            If ws IsNot Nothing Then
                Dim cos As Excel.ChartObjects = CType(ws.ChartObjects(), Excel.ChartObjects)
                If cos IsNot Nothing AndAlso cos.Count > 0 Then
                    sheetKind = "worksheet"
                    sheetName = ws.Name
                    chartName = cos.Item(1).Name
                    Return
                End If
            End If
        Catch
        End Try

        ' Rule 3 handled by caller: first sheet / first chart
    End Sub

    Private Function FindSheetIndex(kind As String, name As String) As Integer
        If String.IsNullOrEmpty(kind) OrElse String.IsNullOrEmpty(name) Then Return -1

        For i As Integer = 0 To cbSheets.Items.Count - 1
            Dim se As SheetEntry = TryCast(cbSheets.Items(i), SheetEntry)
            If se Is Nothing Then Continue For
            If String.Equals(se.Kind, kind, StringComparison.OrdinalIgnoreCase) AndAlso
           String.Equals(se.Name, name, StringComparison.OrdinalIgnoreCase) Then
                Return i
            End If
        Next

        Return -1
    End Function

    Private Function FindChartIndexByName(chartName As String) As Integer
        If String.IsNullOrEmpty(chartName) Then Return -1

        For i As Integer = 0 To cbCharts.Items.Count - 1
            Dim ce As ChartEntry = TryCast(cbCharts.Items(i), ChartEntry)
            If ce Is Nothing Then Continue For

            ' Prefer ChartObjectName match for embedded charts, else Name match
            If (Not String.IsNullOrEmpty(ce.ChartObjectName) AndAlso
            String.Equals(ce.ChartObjectName, chartName, StringComparison.OrdinalIgnoreCase)) _
           OrElse String.Equals(ce.Name, chartName, StringComparison.OrdinalIgnoreCase) _
           OrElse (ce.Chart IsNot Nothing AndAlso String.Equals(ce.Chart.Name, chartName, StringComparison.OrdinalIgnoreCase)) Then
                Return i
            End If
        Next

        Return -1
    End Function

    ' Populate both combos
    Private Sub LoadSheetsAndCharts()
        Dim app As Excel.Application = AppGlobals.app
        Dim wb As Excel.Workbook = AppGlobals.app.ActiveWorkbook

        _loading = True
        Try
            cbSheets.BeginUpdate()
            cbCharts.BeginUpdate()

            cbSheets.Items.Clear()
            cbCharts.Items.Clear()

            ' --- Add worksheets that have embedded charts
            For Each ws As Excel.Worksheet In wb.Worksheets
                Dim hasCharts As Boolean = False
                Try
                    Dim cos As Excel.ChartObjects = CType(ws.ChartObjects(), Excel.ChartObjects)
                    hasCharts = (cos IsNot Nothing AndAlso cos.Count > 0)
                Catch
                    hasCharts = False
                End Try

                If hasCharts Then
                    cbSheets.Items.Add(New SheetEntry With {
                    .Kind = "worksheet",
                    .Name = ws.Name,
                    .SheetObject = ws
                })
                End If
            Next

            ' --- Add chart sheets (Workbook.Charts)
            Try
                For Each ch As Excel.Chart In wb.Charts
                    cbSheets.Items.Add(New SheetEntry With {
                    .Kind = "chartsheet",
                    .Name = ch.Name,
                    .SheetObject = ch
                })
                Next
            Catch
                ' ignore
            End Try

            ' Select first item by default
            If cbSheets.Items.Count > 0 Then
                cbSheets.SelectedIndex = 0
            End If

        Finally
            cbCharts.EndUpdate()
            cbSheets.EndUpdate()
            _loading = False
        End Try
    End Sub

    Private Sub LoadSheetsAndCharts_SelectInitial()
        ' Populate lists
        LoadSheetsAndCharts()

        ' Decide what to select (rules 1/2/3)
        Dim kind As String = Nothing
        Dim sheetName As String = Nothing
        Dim chartName As String = Nothing
        GetInitialSelection(kind, sheetName, chartName)

        ' Select sheet (if found), otherwise first
        _loading = True
        Try
            Dim idx As Integer = FindSheetIndex(kind, sheetName)
            If idx >= 0 Then
                cbSheets.SelectedIndex = idx
            ElseIf cbSheets.Items.Count > 0 Then
                cbSheets.SelectedIndex = 0
            End If
        Finally
            _loading = False
        End Try

        ' Populate charts for that sheet
        PopulateChartsForSelectedSheet(chartName)
    End Sub

    Private Sub btExport_Click(sender As Object, e As System.EventArgs) Handles btExport.Click

        ' Collect settings from UI first (dpi, format, file path)
        Dim dpi As Integer = CInt(Me.spinBtnDPI.Value)
        Dim wPx As Integer = CInt(spinBtnWidth.Value)
        Dim hPx As Integer = CInt(spinBtnHeight.Value)
        Dim quality As Integer = CInt(spinBtnJPGquality.Value)
        Dim fmt As ChartExport.ExportFormat = CType(Me.cbFormat.SelectedItem, ChartExport.ExportFormat)
        Dim ch As Excel.Chart = GetSelectedChartForExport()

        If ch Is Nothing Then
            MessageBox.Show(Me, "Please select a chart first.", "Export Chart",
                            MessageBoxButtons.OK, MessageBoxIcon.Information)
            Return
        End If

        Using sfd As New SaveFileDialog()

            sfd.Filter = "PNG (*.png)|*.png|TIFF (*.tif)|*.tif|JPEG (*.jpg)|*.jpg|GIF (*.gif)|*.gif|Bitmap (*.bmp)|*.bmp|Enhanced Metafile (*.emf)|*.emf"
            sfd.AddExtension = True
            sfd.OverwritePrompt = True

            ' Make the dialog default to the format selected in cbFormat:
            Dim filterIndex As Integer = GetFilterIndex(fmt)
            sfd.FilterIndex = filterIndex
            sfd.DefaultExt = GetDefaultExt(fmt)

            ' Optional: suggest a filename + correct extension up front
            sfd.FileName = "Chart" & GetDefaultExt(fmt)

            If sfd.ShowDialog(Me) <> DialogResult.OK Then Return

            Dim path As String = EnsureExtension(sfd.FileName, fmt)

            Me.btExport.Enabled = False

            ExcelDna.Integration.ExcelAsyncUtil.QueueAsMacro(Sub()
                                                                 Try
                                                                     ChartExport.ExportChart(ch, path, fmt, dpi, wPx, hPx, quality)
                                                                     MessageBox.Show(Me, "Chart exported successfully.", "Export Chart",
                                                                                      MessageBoxButtons.OK, MessageBoxIcon.Information)
                                                                 Catch ex As Exception
                                                                     CoreServices.Logger.Error(ex, $"Chart export failed. path='{path}'; format={fmt}; dpi={dpi}; widthPx={wPx}; heightPx={hPx}")
                                                                     MessageBox.Show(Me, ex.Message, "Export Chart", MessageBoxButtons.OK, MessageBoxIcon.Error)
                                                                 Finally
                                                                     Me.btExport.Enabled = True
                                                                 End Try
                                                             End Sub)

        End Using
    End Sub

    Private Function GetFilterIndex(fmt As ChartExport.ExportFormat) As Integer
        Select Case fmt
            Case ChartExport.ExportFormat.PNG : Return 1
            Case ChartExport.ExportFormat.TIFF : Return 2
            Case ChartExport.ExportFormat.JPG : Return 3
            Case ChartExport.ExportFormat.GIF : Return 4
            Case ChartExport.ExportFormat.BMP : Return 5
            Case ChartExport.ExportFormat.EMF : Return 6
            Case Else : Return 1
        End Select
    End Function

    Private Function GetDefaultExt(fmt As ChartExport.ExportFormat) As String
        Select Case fmt
            Case ChartExport.ExportFormat.PNG : Return ".png"
            Case ChartExport.ExportFormat.TIFF : Return ".tif"
            Case ChartExport.ExportFormat.JPG : Return ".jpg"
            Case ChartExport.ExportFormat.GIF : Return ".gif"
            Case ChartExport.ExportFormat.BMP : Return ".bmp"
            Case ChartExport.ExportFormat.EMF : Return ".emf"
            Case Else : Return ".png"
        End Select
    End Function

    Private Function EnsureExtension(path As String, fmt As ChartExport.ExportFormat) As String
        Dim desired As String = GetDefaultExt(fmt)
        Dim ext As String = IO.Path.GetExtension(path)

        If String.IsNullOrEmpty(ext) OrElse Not ext.Equals(desired, StringComparison.OrdinalIgnoreCase) Then
            Return IO.Path.ChangeExtension(path, desired.TrimStart("."c))
        End If

        Return path
    End Function

    ' When sheet changes, populate charts for that sheet
    Private Sub cbSheets_SelectedIndexChanged(sender As Object, e As System.EventArgs) Handles cbSheets.SelectedIndexChanged
        If _loading Then Return
        ActivateSelectedSheetInExcel()
        PopulateChartsForSelectedSheet(Nothing)
    End Sub

    Private Sub PopulateChartsForSelectedSheet(Optional preferredChartName As String = Nothing)
        Dim se As SheetEntry = TryCast(cbSheets.SelectedItem, SheetEntry)
        If se Is Nothing Then Return

        _loading = True
        Try
            cbCharts.BeginUpdate()
            cbCharts.Items.Clear()

            If se.Kind = "worksheet" Then
                Dim ws As Excel.Worksheet = CType(se.SheetObject, Excel.Worksheet)
                Dim cos As Excel.ChartObjects = CType(ws.ChartObjects(), Excel.ChartObjects)

                For i As Integer = 1 To cos.Count
                    Dim co As Excel.ChartObject = cos.Item(i)
                    Dim ch As Excel.Chart = co.Chart
                    cbCharts.Items.Add(New ChartEntry With {
                                            .Name = co.Name,
                                            .Chart = ch,
                                            .WidthPts = co.Width,
                                            .HeightPts = co.Height,
                                            .IsChartSheet = False,
                                            .Worksheet = ws,
                                            .ChartObjectName = co.Name
                                        })

                Next

            ElseIf se.Kind = "chartsheet" Then
                Dim ch As Excel.Chart = CType(se.SheetObject, Excel.Chart)
                ' For chart sheets: use ChartArea size
                cbCharts.Items.Add(New ChartEntry With {
                                    .Name = ch.Name,
                                    .Chart = ch,
                                    .WidthPts = ch.ChartArea.Width,
                                    .HeightPts = ch.ChartArea.Height,
                                    .IsChartSheet = True,
                                    .Worksheet = Nothing,
                                    .ChartObjectName = Nothing
                                })
            End If

            If cbCharts.Items.Count > 0 Then
                Dim cidx As Integer = FindChartIndexByName(preferredChartName)
                If cidx >= 0 Then
                    cbCharts.SelectedIndex = cidx
                Else
                    cbCharts.SelectedIndex = 0
                End If
            End If

        Finally
            cbCharts.EndUpdate()
            _loading = False
        End Try

        UpdateSizeFromSelectedChart()
    End Sub

    Private Sub cbCharts_SelectedIndexChanged(sender As Object, e As System.EventArgs) Handles cbCharts.SelectedIndexChanged
        If _loading Then Return
        ActivateSelectedChartInExcel()
        UpdateSizeFromSelectedChart()
    End Sub

    Private Sub spinBtnDPI_ValueChanged(sender As Object, e As System.EventArgs) Handles spinBtnDPI.ValueChanged
        If _loading Then Return
        UpdateSizeFromSelectedChart()
    End Sub

    ' Refresh button uses the same logic
    Private Sub btRefresh_Click(sender As Object, e As System.EventArgs)
        ExcelAsyncUtil.QueueAsMacro(Sub()
                                        LoadSheetsAndCharts()
                                    End Sub)
    End Sub

    ' Keep aspect ratio when user edits Width/Height (optional, since you have cbAspectRatio)
    Private Sub spinBtnWidth_ValueChanged(sender As Object, e As System.EventArgs) Handles spinBtnWidth.ValueChanged
        If _suppressAspectUpdate Then Return
        If Not cbAspectRatio.Checked Then Return

        ' Width changed -> adjust Height
        Dim w As Double = CDbl(spinBtnWidth.Value)
        Dim h As Integer = Math.Max(1, CInt(Math.Round(w / _aspectWH)))

        _suppressAspectUpdate = True
        Try
            SetNumericSafe(spinBtnHeight, h)
        Finally
            _suppressAspectUpdate = False
        End Try
    End Sub

    Private Sub spinBtnHeight_ValueChanged(sender As Object, e As System.EventArgs) Handles spinBtnHeight.ValueChanged
        If _suppressAspectUpdate Then Return
        If Not cbAspectRatio.Checked Then Return

        ' Height changed -> adjust Width
        Dim h As Double = CDbl(spinBtnHeight.Value)
        Dim w As Integer = Math.Max(1, CInt(Math.Round(h * _aspectWH)))

        _suppressAspectUpdate = True
        Try
            SetNumericSafe(spinBtnWidth, w)
        Finally
            _suppressAspectUpdate = False
        End Try
    End Sub

    Private Sub cbFormat_SelectedIndexChanged(sender As Object, e As System.EventArgs) Handles cbFormat.SelectedIndexChanged
        UpdateJpgQualityUi()
        UpdateVectorRasterUi()
        UpdateDpiUi()
    End Sub

End Class