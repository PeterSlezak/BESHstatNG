Imports System
Imports System.Collections.Generic
Imports System.Drawing
Imports System.Windows.Forms
Imports BESHStatNG.AppInfrastructure
Imports Microsoft.Office.Interop.Excel

Public Class Ui01ConvexHullPlot
    Private Const DefaultChartWidthPoints As Double = 620.0R
    Private Const DefaultChartHeightPoints As Double = 420.0R
    Private Const ChartColumnGap As Integer = 2

    Private NotInheritable Class ComboItem(Of T)
        Public Sub New(displayText As String, value As T)
            Me.DisplayText = displayText
            Me.Value = value
        End Sub

        Public ReadOnly Property DisplayText As String
        Public ReadOnly Property Value As T

        Public Overrides Function ToString() As String
            Return Me.DisplayText
        End Function
    End Class

    Public Sub New(tagn As Integer)
        ' This call is required by the designer.
        InitializeComponent()
        Me.Tag = tagn

        ' Add any initialization after the InitializeComponent() call.
        Me.RefEdit_Y.ExcelConnector = AppGlobals.app
        Me.RefEdit_X.ExcelConnector = AppGlobals.app
        Me.RefEdit_GroupID.ExcelConnector = AppGlobals.app

        InitializeOptionControls()
        Me.WireHelp(Me.btnHelp)
    End Sub

    ''' <summary>
    ''' Populates the appearance ComboBoxes and applies the backend defaults to
    ''' controls whose values are selected by the user.
    ''' </summary>
    Private Sub InitializeOptionControls()
        InitializeGroupStyleModeComboBox()
        InitializeMarkerStyleComboBox()
        InitializeHullLineStyleComboBox()

        Me.nudPaddingPercentX.Minimum = 0D
        Me.nudPaddingPercentX.Maximum = 100D
        Me.nudPaddingPercentX.DecimalPlaces = 2
        Me.nudPaddingPercentX.Increment = 1D
        Me.nudPaddingPercentX.Value = 0D

        Me.nudPaddingPercentY.Minimum = 0D
        Me.nudPaddingPercentY.Maximum = 100D
        Me.nudPaddingPercentY.DecimalPlaces = 2
        Me.nudPaddingPercentY.Increment = 1D
        Me.nudPaddingPercentY.Value = 0D

        Me.NumericUpDown1.Minimum = 2D
        Me.NumericUpDown1.Maximum = 72D
        Me.NumericUpDown1.DecimalPlaces = 0
        Me.NumericUpDown1.Increment = 1D
        Me.NumericUpDown1.Value = 6D

        Me.NumericUpDown2.Minimum = 0.25D
        Me.NumericUpDown2.Maximum = 10D
        Me.NumericUpDown2.DecimalPlaces = 2
        Me.NumericUpDown2.Increment = 0.25D
        Me.NumericUpDown2.Value = 1.5D

        Me.ckIncludeCollinearBoundaryPoints.Checked = True
        Me.ckShowLegend.Checked = True
        Me.ckShowMajorGridlines.Checked = True
        Me.tbCollinearityTolerance.Text = "0"

        Dim defaults As New ConvexHullPlotAppearance()
        Me.pnlMarkerColor.BackColor = ColorTranslator.FromOle(defaults.MarkerForegroundColor)
        Me.pnlHullLineColor.BackColor = ColorTranslator.FromOle(defaults.HullLineColor)
        Me.pnlMarkerColor.BorderStyle = BorderStyle.FixedSingle
        Me.pnlHullLineColor.BorderStyle = BorderStyle.FixedSingle
    End Sub

    Private Sub InitializeGroupStyleModeComboBox()
        With Me.cbGroupStyleMode
            .BeginUpdate()
            Try
                .Items.Clear()
                .DropDownStyle = ComboBoxStyle.DropDownList
                .Items.Add(New ComboItem(Of ConvexHullGroupStyleMode)("Same style", ConvexHullGroupStyleMode.None))
                .Items.Add(New ComboItem(Of ConvexHullGroupStyleMode)("Color", ConvexHullGroupStyleMode.Color))
                .Items.Add(New ComboItem(Of ConvexHullGroupStyleMode)("Marker symbol", ConvexHullGroupStyleMode.Marker))
                .Items.Add(New ComboItem(Of ConvexHullGroupStyleMode)("Line style", ConvexHullGroupStyleMode.LineStyle))
                .Items.Add(New ComboItem(Of ConvexHullGroupStyleMode)("Color and marker", ConvexHullGroupStyleMode.ColorAndMarker))
                .Items.Add(New ComboItem(Of ConvexHullGroupStyleMode)("Color and line style", ConvexHullGroupStyleMode.ColorAndLineStyle))
                .Items.Add(New ComboItem(Of ConvexHullGroupStyleMode)("Marker and line style", ConvexHullGroupStyleMode.MarkerAndLineStyle))
                .Items.Add(New ComboItem(Of ConvexHullGroupStyleMode)("Color, marker and line style", ConvexHullGroupStyleMode.ColorMarkerAndLineStyle))
                SelectComboValue(Me.cbGroupStyleMode, ConvexHullGroupStyleMode.ColorAndMarker)
            Finally
                .EndUpdate()
            End Try
        End With
    End Sub

    Private Sub InitializeMarkerStyleComboBox()
        With Me.cbMarkerStyle
            .BeginUpdate()
            Try
                .Items.Clear()
                .DropDownStyle = ComboBoxStyle.DropDownList
                .Items.Add(New ComboItem(Of XlMarkerStyle)("Circle", XlMarkerStyle.xlMarkerStyleCircle))
                .Items.Add(New ComboItem(Of XlMarkerStyle)("Square", XlMarkerStyle.xlMarkerStyleSquare))
                .Items.Add(New ComboItem(Of XlMarkerStyle)("Triangle", XlMarkerStyle.xlMarkerStyleTriangle))
                .Items.Add(New ComboItem(Of XlMarkerStyle)("Diamond", XlMarkerStyle.xlMarkerStyleDiamond))
                .Items.Add(New ComboItem(Of XlMarkerStyle)("X", XlMarkerStyle.xlMarkerStyleX))
                .Items.Add(New ComboItem(Of XlMarkerStyle)("Plus", XlMarkerStyle.xlMarkerStylePlus))
                .Items.Add(New ComboItem(Of XlMarkerStyle)("Star", XlMarkerStyle.xlMarkerStyleStar))
                .Items.Add(New ComboItem(Of XlMarkerStyle)("Dash", XlMarkerStyle.xlMarkerStyleDash))
                SelectComboValue(Me.cbMarkerStyle, XlMarkerStyle.xlMarkerStyleCircle)
            Finally
                .EndUpdate()
            End Try
        End With
    End Sub

    Private Sub InitializeHullLineStyleComboBox()
        With Me.cbHullLineStyle
            .BeginUpdate()
            Try
                .Items.Clear()
                .DropDownStyle = ComboBoxStyle.DropDownList
                .Items.Add(New ComboItem(Of XlLineStyle)("Continuous", XlLineStyle.xlContinuous))
                .Items.Add(New ComboItem(Of XlLineStyle)("Dash", XlLineStyle.xlDash))
                .Items.Add(New ComboItem(Of XlLineStyle)("Dot", XlLineStyle.xlDot))
                .Items.Add(New ComboItem(Of XlLineStyle)("Dash-dot", XlLineStyle.xlDashDot))
                .Items.Add(New ComboItem(Of XlLineStyle)("Dash-dot-dot", XlLineStyle.xlDashDotDot))
                SelectComboValue(Me.cbHullLineStyle, XlLineStyle.xlContinuous)
            Finally
                .EndUpdate()
            End Try
        End With
    End Sub

    Private Shared Sub SelectComboValue(Of T)(comboBox As ComboBox, value As T)
        For itemIndex As Integer = 0 To comboBox.Items.Count - 1
            Dim item As ComboItem(Of T) = TryCast(comboBox.Items(itemIndex), ComboItem(Of T))
            If item IsNot Nothing AndAlso EqualityComparer(Of T).Default.Equals(item.Value, value) Then
                comboBox.SelectedIndex = itemIndex
                Return
            End If
        Next

        If comboBox.Items.Count > 0 Then comboBox.SelectedIndex = 0
    End Sub

    Private Shared Function GetComboValue(Of T)(comboBox As ComboBox,
                                                 controlDescription As String) As T
        Dim item As ComboItem(Of T) = TryCast(comboBox.SelectedItem, ComboItem(Of T))
        If item Is Nothing Then
            Throw New InvalidOperationException("Select " & controlDescription & ".")
        End If
        Return item.Value
    End Function

    ''' <summary>
    ''' Validates that X, Y, and the optional grouping input are continuous,
    ''' single-column, row-aligned ranges on the same worksheet.
    ''' </summary>
    ''' <returns><see langword="True"/> when validation failed.</returns>
    Private Function CheckInputs() As Boolean
        Dim invalid As Boolean = False

        'The shared RefEdit helpers resolve unqualified references against the
        'active workbook, so activate the workbook captured by the first RefEdit.
        If Me.RefEdit_X.ExcelWorkBook IsNot Nothing Then
            Me.RefEdit_X.ExcelWorkBook.Activate()
        End If

        If CheckRefEdit(Me.RefEdit_X.Address, True) Then
            RefEditReset(Me.RefEdit_X)
            invalid = True
        End If

        If CheckRefEdit(Me.RefEdit_Y.Address, True) Then
            RefEditReset(Me.RefEdit_Y)
            invalid = True
        End If

        Dim hasGrouping As Boolean = Not String.IsNullOrWhiteSpace(Me.RefEdit_GroupID.Address)
        If hasGrouping AndAlso CheckRefEdit(Me.RefEdit_GroupID.Address, True) Then
            RefEditReset(Me.RefEdit_GroupID)
            invalid = True
        End If

        If invalid Then Return True

        Try
            Dim xWorksheet As Worksheet = WorksheetFromRefAdress(Me.RefEdit_X.Address,
                                                                 Me.RefEdit_X.ExcelWorkBook)
            Dim yWorksheet As Worksheet = WorksheetFromRefAdress(Me.RefEdit_Y.Address,
                                                                 Me.RefEdit_Y.ExcelWorkBook)
            Dim groupWorksheet As Worksheet = Nothing
            If hasGrouping Then
                groupWorksheet = WorksheetFromRefAdress(Me.RefEdit_GroupID.Address,
                                                        Me.RefEdit_GroupID.ExcelWorkBook)
            End If

            Dim xWorkbook As Workbook = DirectCast(xWorksheet.Parent, Workbook)
            Dim yWorkbook As Workbook = DirectCast(yWorksheet.Parent, Workbook)

            If Not SameWorksheet(xWorkbook, xWorksheet, yWorkbook, yWorksheet) Then
                MsgBox("X and Y ranges must be on the same worksheet.",
                       vbExclamation,
                       AppGlobals.gsAPP_TITLE)
                Return True
            End If

            If hasGrouping Then
                Dim groupWorkbook As Workbook = DirectCast(groupWorksheet.Parent, Workbook)
                If Not SameWorksheet(xWorkbook, xWorksheet, groupWorkbook, groupWorksheet) Then
                    MsgBox("X, Y, and grouping ranges must be on the same worksheet.",
                           vbExclamation,
                           AppGlobals.gsAPP_TITLE)
                    Return True
                End If
            End If

            Dim xRange As Range = xWorksheet.Range(Me.RefEdit_X.Address)
            Dim yRange As Range = yWorksheet.Range(Me.RefEdit_Y.Address)
            Dim groupRange As Range = Nothing
            If hasGrouping Then groupRange = groupWorksheet.Range(Me.RefEdit_GroupID.Address)

            If xRange.Areas.Count <> 1 OrElse
               yRange.Areas.Count <> 1 OrElse
               (hasGrouping AndAlso groupRange.Areas.Count <> 1) Then
                MsgBox("Each convex-hull input must be one continuous column range.",
                       vbExclamation,
                       AppGlobals.gsAPP_TITLE)
                Return True
            End If

            If xRange.Row <> yRange.Row OrElse xRange.Rows.Count <> yRange.Rows.Count Then
                MsgBox("X and Y ranges must start on the same row and contain the same number of rows.",
                       vbExclamation,
                       AppGlobals.gsAPP_TITLE)
                Return True
            End If

            If hasGrouping AndAlso
               (groupRange.Row <> xRange.Row OrElse groupRange.Rows.Count <> xRange.Rows.Count) Then
                MsgBox("The grouping range must start on the same row and contain the same number of rows as X and Y.",
                       vbExclamation,
                       AppGlobals.gsAPP_TITLE)
                Return True
            End If

            Dim tolerance As Double
            If Not TryParseUiDouble(Me.tbCollinearityTolerance.Text, tolerance) OrElse
               Double.IsNaN(tolerance) OrElse
               Double.IsInfinity(tolerance) OrElse
               tolerance < 0.0R Then
                MsgBox("Collinearity tolerance must be a finite, nonnegative number.",
                       vbExclamation,
                       AppGlobals.gsAPP_TITLE)
                Me.tbCollinearityTolerance.Focus()
                Me.tbCollinearityTolerance.SelectAll()
                Return True
            End If

            Return False
        Catch ex As Exception
            CoreServices.Errors.LogAndThrow(ex,
                                            False,
                                            True,
                                            "Unable to validate the convex-hull input ranges")
            Return True
        End Try
    End Function

    Private Shared Function SameWorksheet(firstWorkbook As Workbook,
                                          firstWorksheet As Worksheet,
                                          secondWorkbook As Workbook,
                                          secondWorksheet As Worksheet) As Boolean
        Return String.Equals(firstWorkbook.FullName,
                             secondWorkbook.FullName,
                             StringComparison.OrdinalIgnoreCase) AndAlso
               String.Equals(firstWorksheet.Name,
                             secondWorksheet.Name,
                             StringComparison.OrdinalIgnoreCase)
    End Function

    ''' <summary>
    ''' Imports X, Y, and the optional group in one operation so worksheet-row
    ''' alignment is retained when missing observations are present.
    ''' </summary>
    Private Function GetData(ByRef errorText As String) As DataObj
        Dim inputWorkbook As Workbook = Me.RefEdit_X.ExcelWorkBook
        If inputWorkbook IsNot Nothing Then inputWorkbook.Activate()

        Dim xReference As String = prepareRef2D(Me.RefEdit_X.Address, inputWorkbook)
        Dim yReference As String = prepareRef2D(Me.RefEdit_Y.Address,
                                                Me.RefEdit_Y.ExcelWorkBook)
        yReference = RemoveWorksheetQualifier(yReference)

        Dim hasGrouping As Boolean = Not String.IsNullOrWhiteSpace(Me.RefEdit_GroupID.Address)
        Dim combinedReference As String
        Dim characterColumns As Integer
        Dim expectedColumnCount As Integer

        If hasGrouping Then
            'Place the group first because DataObj accepts character data only in
            'leading columns. Numeric grouping IDs are accepted as well.
            Dim groupReference As String = prepareRef2D(Me.RefEdit_GroupID.Address,
                                                        Me.RefEdit_GroupID.ExcelWorkBook)
            combinedReference = groupReference & ", " &
                                RemoveWorksheetQualifier(xReference) & ", " &
                                yReference
            characterColumns = 0
            expectedColumnCount = 3
        Else
            combinedReference = xReference & ", " & yReference
            characterColumns = -1
            expectedColumnCount = 2
        End If

        Dim columnData As New DataObj()
        columnData.bAllowMissing = True
        ExcelDnaDataImporter.ImportInto(columnData,
                                        combinedReference,
                                        True,
                                        CharCols:=characterColumns)

        If columnData.bZeroValid OrElse columnData.FinalData Is Nothing Then
            errorText = "No usable numeric X/Y observations were found in the selected ranges."
            Return Nothing
        End If

        If columnData.nCols <> expectedColumnCount Then
            errorText = If(hasGrouping,
                           "X and Y must contain numeric data, and the grouping range must contain text or numeric group IDs.",
                           "Both X and Y inputs must contain numeric data.")
            Return Nothing
        End If

        Return columnData
    End Function

    Private Shared Function RemoveWorksheetQualifier(reference As String) As String
        If String.IsNullOrWhiteSpace(reference) Then Return reference
        Dim worksheetDelimiter As Integer = reference.LastIndexOf("!"c)
        If worksheetDelimiter < 0 Then Return reference
        Return reference.Substring(worksheetDelimiter + 1)
    End Function

    Private Function GetPlotOptions() As ConvexHullPlotOptions
        Return New ConvexHullPlotOptions With {
            .IncludeCollinearBoundaryPoints = Me.ckIncludeCollinearBoundaryPoints.Checked,
            .PaddingPercentX = CDbl(Me.nudPaddingPercentX.Value),
            .PaddingPercentY = CDbl(Me.nudPaddingPercentY.Value),
            .CollinearityTolerance = ParseUiDouble(Me.tbCollinearityTolerance.Text,
                                                   "collinearity tolerance")
        }
    End Function

    ''' <summary>
    ''' Creates the chart appearance represented by the controls on the Appearance tab.
    ''' Properties omitted from the form retain their ConvexHullPlotAppearance defaults.
    ''' </summary>
    Private Function GetPlotAppearance(data As DataObj,
                                       xColumn As Integer,
                                       yColumn As Integer) As ConvexHullPlotAppearance
        Dim appearance As New ConvexHullPlotAppearance()

        appearance.XAxisTitle = GetVariableName(data, xColumn, "X")
        appearance.YAxisTitle = GetVariableName(data, yColumn, "Y")
        appearance.ShowLegend = Me.ckShowLegend.Checked
        appearance.ShowGroupLegend = Me.ckShowLegend.Checked
        appearance.ShowMajorGridlines = Me.ckShowMajorGridlines.Checked
        appearance.GroupStyleMode = GetComboValue(Of ConvexHullGroupStyleMode)(Me.cbGroupStyleMode,
                                                                                "how grouped observations should be differentiated")
        appearance.MarkerStyle = GetComboValue(Of XlMarkerStyle)(Me.cbMarkerStyle,
                                                                  "a marker symbol")
        appearance.MarkerSize = Decimal.ToInt32(Me.NumericUpDown1.Value)
        appearance.MarkerForegroundColor = ColorTranslator.ToOle(Me.pnlMarkerColor.BackColor)
        appearance.MarkerBackgroundColor = appearance.MarkerForegroundColor
        appearance.HullLineStyle = GetComboValue(Of XlLineStyle)(Me.cbHullLineStyle,
                                                                  "a hull line style")
        appearance.HullLineWeight = CSng(Me.NumericUpDown2.Value)
        appearance.HullLineColor = ColorTranslator.ToOle(Me.pnlHullLineColor.BackColor)

        Return appearance
    End Function

    Private Shared Function GetVariableName(data As DataObj,
                                            columnIndex As Integer,
                                            fallback As String) As String
        If data IsNot Nothing AndAlso
           data.varNames IsNot Nothing AndAlso
           columnIndex >= 0 AndAlso
           columnIndex < data.varNames.Length AndAlso
           Not String.IsNullOrWhiteSpace(data.varNames(columnIndex)) Then
            Return data.varNames(columnIndex).Trim()
        End If
        Return fallback
    End Function

    ''' <summary>
    ''' Computes grouped or ungrouped hulls and creates an embedded XY-scatter chart
    ''' beside the selected input columns.
    ''' </summary>
    Private Sub btCompute_Click(sender As Object, e As System.EventArgs) Handles btCompute.Click
        Try
            If Me.CheckInputs() Then Exit Sub

            Dim errorText As String = String.Empty
            Dim data As DataObj = Me.GetData(errorText)
            If errorText <> String.Empty Then
                MsgBox(errorText, vbExclamation, AppGlobals.gsAPP_TITLE)
                Exit Sub
            End If

            Dim cleanData As Object(,) = data.FinalData
            Dim hasGrouping As Boolean = Not String.IsNullOrWhiteSpace(Me.RefEdit_GroupID.Address)
            Dim groupColumn As Integer = If(hasGrouping, 0, -1)
            Dim xColumn As Integer = If(hasGrouping, 1, 0)
            Dim yColumn As Integer = If(hasGrouping, 2, 1)

            Dim xValues As New List(Of Double)(cleanData.GetLength(0))
            Dim yValues As New List(Of Double)(cleanData.GetLength(0))
            Dim groupValues As List(Of Object) = If(hasGrouping,
                                                    New List(Of Object)(cleanData.GetLength(0)),
                                                    Nothing)

            For rowIndex As Integer = 0 To cleanData.GetLength(0) - 1
                xValues.Add(ToDoubleOrNaN(cleanData(rowIndex, xColumn)))
                yValues.Add(ToDoubleOrNaN(cleanData(rowIndex, yColumn)))
                If hasGrouping Then groupValues.Add(cleanData(rowIndex, groupColumn))
            Next

            Dim result As ConvexHullPlotResult
            If hasGrouping Then
                result = ConvexHullPlot.ComputeGrouped(xValues.ToArray(),
                                                       yValues.ToArray(),
                                                       groupValues.ToArray(),
                                                       Me.GetPlotOptions())
            Else
                result = ConvexHullPlot.Compute(xValues.ToArray(),
                                                yValues.ToArray(),
                                                Me.GetPlotOptions())
            End If

            Dim inputWorksheet As Worksheet = DirectCast(data.ws, Worksheet)
            DirectCast(inputWorksheet.Parent, Workbook).Activate()
            inputWorksheet.Activate()

            Dim xRange As Range = inputWorksheet.Range(Me.RefEdit_X.Address)
            Dim yRange As Range = inputWorksheet.Range(Me.RefEdit_Y.Address)
            Dim lastInputColumn As Integer = Math.Max(xRange.Column + xRange.Columns.Count - 1,
                                                      yRange.Column + yRange.Columns.Count - 1)
            Dim anchorRow As Integer = Math.Min(xRange.Row, yRange.Row)

            If hasGrouping Then
                Dim groupRange As Range = inputWorksheet.Range(Me.RefEdit_GroupID.Address)
                lastInputColumn = Math.Max(lastInputColumn,
                                           groupRange.Column + groupRange.Columns.Count - 1)
                anchorRow = Math.Min(anchorRow, groupRange.Row)
            End If

            Dim anchorColumn As Integer = lastInputColumn + ChartColumnGap
            If anchorColumn > inputWorksheet.Columns.Count Then anchorColumn = 1
            Dim chartAnchor As Range = DirectCast(inputWorksheet.Cells(anchorRow, anchorColumn), Range)

            Dim appearance As ConvexHullPlotAppearance = Me.GetPlotAppearance(data,
                                                                              xColumn,
                                                                              yColumn)
            Dim createdChart As Chart = ConvexHullPlotExcel.AddChart(inputWorksheet,
                                                                     result,
                                                                     appearance,
                                                                     CDbl(chartAnchor.Left),
                                                                     CDbl(chartAnchor.Top),
                                                                     DefaultChartWidthPoints,
                                                                     DefaultChartHeightPoints)
        Catch ex As Exception
            CoreServices.Errors.LogAndThrow(ex,
                                            False,
                                            True,
                                            "Unable to create the convex-hull plot")
        End Try
    End Sub

    Private Sub btnMarkerColor_Click(sender As Object, e As System.EventArgs) Handles btnMarkerColor.Click
        SelectColor(Me.pnlMarkerColor, "Select marker color")
    End Sub

    Private Sub btnHullLineColor_Click(sender As Object, e As System.EventArgs) Handles btnHullLineColor.Click
        SelectColor(Me.pnlHullLineColor, "Select hull line color")
    End Sub

    Private Shared Sub SelectColor(previewPanel As Panel, dialogTitle As String)
        Using dialog As New ColorDialog()
            dialog.AllowFullOpen = True
            dialog.AnyColor = True
            dialog.FullOpen = True
            dialog.Color = previewPanel.BackColor

            If dialog.ShowDialog() = DialogResult.OK Then
                previewPanel.BackColor = dialog.Color
            End If
        End Using
    End Sub

    Private Shared Function ToDoubleOrNaN(value As Object) As Double
        If value Is Nothing OrElse value Is DBNull.Value Then Return Double.NaN
        Return CDbl(value)
    End Function
End Class
