Option Explicit On
Option Infer On

Imports System
Imports System.Collections.Generic
Imports System.Drawing
Imports System.Globalization
Imports System.Windows.Forms
Imports BESHStatNG.AppInfrastructure
Imports Microsoft.Office.Interop.Excel

Public Class Ui01LadderPlot

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
        InitializeComponent()
        Me.Tag = tagn

        Me.RefEdit_GroupingVariable.ExcelConnector = AppGlobals.app
        Me.RefEdit_FirstValues.ExcelConnector = AppGlobals.app
        Me.RefEdit_SecondValues.ExcelConnector = AppGlobals.app
        Me.RefEdit_RecordLabels.ExcelConnector = AppGlobals.app
        Me.RefEditOutput.ExcelConnector = AppGlobals.app

        Me.InitializeChoiceControls()
        Me.InitializeAppearanceControls()
        Me.UpdateOutputRangeState()
        Me.UpdateOptionControlStates()
        Me.WireHelp(Me.btnHelp)
    End Sub

    ''' <summary>
    ''' Populates the user-facing choice controls while retaining strongly typed values
    ''' for the ladder backend and Excel renderer.
    ''' </summary>
    Private Sub InitializeChoiceControls()
        With Me.cmbColorMode
            .BeginUpdate()
            Try
                .Items.Clear()
                .DropDownStyle = ComboBoxStyle.DropDownList
                .Items.Add(New ComboItem(Of LadderColorMode)("Single color", LadderColorMode.SingleColor))
                .Items.Add(New ComboItem(Of LadderColorMode)("By observation", LadderColorMode.ByObservation))
                .Items.Add(New ComboItem(Of LadderColorMode)("By group", LadderColorMode.ByGroup))
                SelectComboValue(Me.cmbColorMode, (New LadderPlotAppearance()).ColorMode)
            Finally
                .EndUpdate()
            End Try
        End With

        With Me.cmdMarkerStyle
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
                SelectComboValue(Me.cmdMarkerStyle, (New LadderPlotAppearance()).MarkerStyle)
            Finally
                .EndUpdate()
            End Try
        End With

        With Me.cmbConnectorLineStyle
            .BeginUpdate()
            Try
                .Items.Clear()
                .DropDownStyle = ComboBoxStyle.DropDownList
                .Items.Add(New ComboItem(Of LadderLineStyle)("Solid", LadderLineStyle.Solid))
                .Items.Add(New ComboItem(Of LadderLineStyle)("Dash", LadderLineStyle.Dash))
                .Items.Add(New ComboItem(Of LadderLineStyle)("Dot", LadderLineStyle.Dot))
                .Items.Add(New ComboItem(Of LadderLineStyle)("Dash-dot", LadderLineStyle.DashDot))
                .Items.Add(New ComboItem(Of LadderLineStyle)("Dash-dot-dot", LadderLineStyle.DashDotDot))
                SelectComboValue(Me.cmbConnectorLineStyle, (New LadderPlotAppearance()).LineStyle)
            Finally
                .EndUpdate()
            End Try
        End With

        'The current Designer uses a TextBox for this setting.  Keep the three renderer
        'choices available as simple user-facing text without requiring a Designer change.
        If String.IsNullOrWhiteSpace(Me.cmbEndpointLabelContent.Text) Then
            Me.cmbEndpointLabelContent.Text = "Record label + value"
        End If
    End Sub

    ''' <summary>
    ''' Applies renderer defaults to the colour preview panels.  NumericUpDown, CheckBox
    ''' and TextBox defaults selected in the Windows Forms Designer are otherwise retained.
    ''' </summary>
    Private Sub InitializeAppearanceControls()
        Dim defaults As New LadderPlotAppearance()
        Dim defaultColor As Color = ColorTranslator.FromOle(defaults.SingleColor)

        Me.pnlSingleColor.BackColor = defaultColor
        Me.pnlConnectorColor.BackColor = defaultColor
        Me.pnlSingleColor.BorderStyle = BorderStyle.FixedSingle
        Me.pnlConnectorColor.BorderStyle = BorderStyle.FixedSingle
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

    Private Sub optOutputRange_CheckedChanged(sender As Object, e As System.EventArgs) Handles optOutputRange.CheckedChanged
        Me.UpdateOutputRangeState()
    End Sub

    Private Sub optWorksheet_CheckedChanged(sender As Object, e As System.EventArgs) Handles optWorksheet.CheckedChanged
        Me.UpdateOutputRangeState()
    End Sub

    Private Sub optWorkbook_CheckedChanged(sender As Object, e As System.EventArgs) Handles optWorkbook.CheckedChanged
        Me.UpdateOutputRangeState()
    End Sub

    Private Sub UpdateOutputRangeState()
        Me.RefEditOutput.Enabled = Me.optOutputRange.Checked
        If Me.optOutputRange.Checked AndAlso Me.RefEditOutput.CanFocus Then
            Me.RefEditOutput.txtAddress.Select()
        End If
    End Sub

    Private Sub ckHorizontalJitter_CheckedChanged(sender As Object, e As System.EventArgs) Handles ckHorizontalJitter.CheckedChanged
        Me.UpdateOptionControlStates()
    End Sub

    Private Sub ckShowLines_CheckedChanged(sender As Object, e As System.EventArgs) Handles ckShowLines.CheckedChanged
        Me.UpdateOptionControlStates()
    End Sub

    Private Sub ckShowMarkers_CheckedChanged(sender As Object, e As System.EventArgs) Handles ckShowMarkers.CheckedChanged
        Me.UpdateOptionControlStates()
    End Sub

    Private Sub cmbColorMode_SelectedIndexChanged(sender As Object, e As System.EventArgs) Handles cmbColorMode.SelectedIndexChanged
        Me.UpdateOptionControlStates()
    End Sub

    Private Sub ckShowFirstEndpointLabels_CheckedChanged(sender As Object, e As System.EventArgs) Handles ckShowFirstEndpointLabels.CheckedChanged
        Me.UpdateOptionControlStates()
    End Sub

    Private Sub ckShowSecondEndpointLabels_CheckedChanged(sender As Object, e As System.EventArgs) Handles ckShowSecondEndpointLabels.CheckedChanged
        Me.UpdateOptionControlStates()
    End Sub

    Private Sub UpdateOptionControlStates()
        Me.nudHorizontalJitter.Enabled = Me.ckHorizontalJitter.Checked

        Dim showMarkers As Boolean = Me.ckShowMarkers.Checked
        Me.cmdMarkerStyle.Enabled = showMarkers
        Me.nudMarkerSize.Enabled = showMarkers
        Me.lblMarkerStyle.Enabled = showMarkers
        Me.lblMarkerSize.Enabled = showMarkers

        Dim showLines As Boolean = Me.ckShowLines.Checked
        Me.cmbConnectorLineStyle.Enabled = showLines
        Me.nudConnectorWidth.Enabled = showLines
        Me.nudConnectorTransparency.Enabled = showLines
        Me.lblLineStyle.Enabled = showLines
        Me.lblLineWidth.Enabled = showLines
        Me.lblTransparency.Enabled = showLines

        Dim singleColorMode As Boolean = False
        Dim selectedColorMode As ComboItem(Of LadderColorMode) = TryCast(Me.cmbColorMode.SelectedItem,
                                                                         ComboItem(Of LadderColorMode))
        If selectedColorMode IsNot Nothing Then
            singleColorMode = selectedColorMode.Value = LadderColorMode.SingleColor
        End If

        Me.btnSingleColor.Enabled = singleColorMode
        Me.pnlSingleColor.Enabled = singleColorMode
        Me.lblSingleColor.Enabled = singleColorMode

        'The current renderer colours lines and markers together for By observation/By group.
        'The separate connector colour is therefore meaningful only in Single color mode.
        Me.btnConnectorColor.Enabled = showLines AndAlso singleColorMode
        Me.pnlConnectorColor.Enabled = showLines AndAlso singleColorMode
        Me.lblConnectorColor.Enabled = showLines AndAlso singleColorMode

        Dim showEndpointLabels As Boolean = Me.ckShowFirstEndpointLabels.Checked OrElse
                                            Me.ckShowSecondEndpointLabels.Checked
        Me.cmbEndpointLabelContent.Enabled = showEndpointLabels
        Me.txtEndpointValueFormat.Enabled = showEndpointLabels
        Me.lblEndpointLabelContent.Enabled = showEndpointLabels
        Me.lblEndpointValueFormat.Enabled = showEndpointLabels
        Me.ckAvoidEndpointLabelOverlap.Enabled = showEndpointLabels
        Me.ckColorEndpointLabelsBySeries.Enabled = showEndpointLabels
    End Sub

    ''' <summary>
    ''' Validates required paired ranges, optional label/grouping ranges, output destination,
    ''' and appearance settings before any chart is created.
    ''' </summary>
    ''' <returns>True when validation failed.</returns>
    Private Function CheckInputs() As Boolean
        Dim inputWorkbook As Workbook = Me.GetPrimaryInputWorkbook()
        If inputWorkbook IsNot Nothing Then inputWorkbook.Activate()

        Dim firstRange As Range = Nothing
        If Not Me.TryResolveRange(Me.RefEdit_FirstValues,
                                  "First values",
                                  requireSingleColumn:=True,
                                  requireSingleArea:=True,
                                  resolvedRange:=firstRange) Then
            Return True
        End If

        Dim secondRange As Range = Nothing
        If Not Me.TryResolveRange(Me.RefEdit_SecondValues,
                                  "Second values",
                                  requireSingleColumn:=True,
                                  requireSingleArea:=True,
                                  resolvedRange:=secondRange) Then
            Return True
        End If

        If Not Me.ValidateAlignedInputRange(firstRange, secondRange, "Second values") Then Return True

        Dim hasRecordLabels As Boolean = Not String.IsNullOrWhiteSpace(Me.RefEdit_RecordLabels.Address)
        If hasRecordLabels Then
            Dim recordRange As Range = Nothing
            If Not Me.TryResolveRange(Me.RefEdit_RecordLabels,
                                      "Record labels",
                                      requireSingleColumn:=True,
                                      requireSingleArea:=True,
                                      resolvedRange:=recordRange) Then
                Return True
            End If
            If Not Me.ValidateAlignedInputRange(firstRange, recordRange, "Record labels") Then Return True
        End If

        Dim hasGrouping As Boolean = Not String.IsNullOrWhiteSpace(Me.RefEdit_GroupingVariable.Address)
        If hasGrouping Then
            Dim groupingRange As Range = Nothing
            If Not Me.TryResolveRange(Me.RefEdit_GroupingVariable,
                                      "Grouping variable",
                                      requireSingleColumn:=True,
                                      requireSingleArea:=True,
                                      resolvedRange:=groupingRange) Then
                Return True
            End If
            If Not Me.ValidateAlignedInputRange(firstRange, groupingRange, "Grouping variable") Then Return True
        End If

        If Me.optOutputRange.Checked Then
            Dim outputRange As Range = Nothing
            If Not Me.TryResolveRange(Me.RefEditOutput,
                                      "Output range",
                                      requireSingleColumn:=False,
                                      requireSingleArea:=True,
                                      resolvedRange:=outputRange) Then
                Return True
            End If
        End If

        Try
            Dim colorMode As LadderColorMode = GetComboValue(Of LadderColorMode)(Me.cmbColorMode,
                                                                                 "a color mode")
            If colorMode = LadderColorMode.ByGroup AndAlso Not hasGrouping Then
                Throw New ArgumentException("By group coloring requires a grouping variable.")
            End If

            If Not Me.ckShowLines.Checked AndAlso Not Me.ckShowMarkers.Checked Then
                Throw New ArgumentException("Select Show lines, Show markers, or both.")
            End If

            Dim ignoredOptions As LadderPlotOptions = Me.GetPlotOptions()
            Dim ignoredAppearance As LadderPlotAppearance = Me.GetPlotAppearance("Before", "After")
        Catch ex As ArgumentException
            MsgBox(ex.Message, vbExclamation, AppGlobals.gsAPP_TITLE)
            Return True
        Catch ex As InvalidOperationException
            MsgBox(ex.Message, vbExclamation, AppGlobals.gsAPP_TITLE)
            Return True
        End Try

        Return False
    End Function

    Private Function TryResolveRange(refEdit As Excel2007RefEdit,
                                     displayName As String,
                                     requireSingleColumn As Boolean,
                                     requireSingleArea As Boolean,
                                     ByRef resolvedRange As Range) As Boolean
        resolvedRange = Nothing

        If refEdit Is Nothing OrElse String.IsNullOrWhiteSpace(refEdit.Address) Then
            MsgBox(displayName & " range is empty. Please select a range.",
                   vbExclamation,
                   AppGlobals.gsAPP_TITLE)
            Return False
        End If

        Try
            Dim wb As Workbook = refEdit.ExcelWorkBook
            If wb Is Nothing Then wb = AppGlobals.app.ActiveWorkbook
            If wb Is Nothing Then Throw New InvalidOperationException("No active Excel workbook is available.")

            Dim ws As Worksheet = WorksheetFromRefAdress(refEdit.Address, wb)
            If ws Is Nothing Then Throw New InvalidOperationException("Unable to resolve the selected worksheet.")

            resolvedRange = ws.Range(refEdit.Address)

            If requireSingleArea AndAlso resolvedRange.Areas.Count <> 1 Then
                MsgBox(displayName & " must be one continuous range.",
                       vbExclamation,
                       AppGlobals.gsAPP_TITLE)
                Return False
            End If

            If requireSingleColumn AndAlso resolvedRange.Columns.Count <> 1 Then
                MsgBox(displayName & " must contain exactly one column.",
                       vbExclamation,
                       AppGlobals.gsAPP_TITLE)
                Return False
            End If

            Return True
        Catch
            MsgBox(displayName & " is not a valid Excel range.",
                   vbExclamation,
                   AppGlobals.gsAPP_TITLE)
            Return False
        End Try
    End Function

    Private Function ResolveRefEditRange(refEdit As Excel2007RefEdit) As Range
        If refEdit Is Nothing Then Throw New ArgumentNullException(NameOf(refEdit))
        If String.IsNullOrWhiteSpace(refEdit.Address) Then
            Throw New ArgumentException("The input range is empty.", NameOf(refEdit))
        End If

        Dim wb As Workbook = refEdit.ExcelWorkBook
        If wb Is Nothing Then wb = AppGlobals.app.ActiveWorkbook
        If wb Is Nothing Then Throw New InvalidOperationException("No active Excel workbook is available.")

        Dim ws As Worksheet = WorksheetFromRefAdress(refEdit.Address, wb)
        If ws Is Nothing Then Throw New InvalidOperationException("Unable to resolve the worksheet for the selected input range.")
        Return ws.Range(refEdit.Address)
    End Function

    Private Function ValidateAlignedInputRange(referenceRange As Range,
                                               otherRange As Range,
                                               displayName As String) As Boolean
        If Not AreRangesOnSameWorksheet(referenceRange, otherRange) Then
            MsgBox("First values, Second values, and optional label/grouping variables must be on the same worksheet. " &
                   displayName & " is on a different worksheet.",
                   vbExclamation,
                   AppGlobals.gsAPP_TITLE)
            Return False
        End If

        If Not AreRangesRowAligned(referenceRange, otherRange) Then
            MsgBox(displayName & " must start on the same row and contain the same number of rows as First values.",
                   vbExclamation,
                   AppGlobals.gsAPP_TITLE)
            Return False
        End If

        Return True
    End Function

    Private Shared Function AreRangesRowAligned(firstRange As Range, secondRange As Range) As Boolean
        If firstRange Is Nothing OrElse secondRange Is Nothing Then Return False
        Return firstRange.Row = secondRange.Row AndAlso firstRange.Rows.Count = secondRange.Rows.Count
    End Function

    Private Shared Function AreRangesOnSameWorksheet(firstRange As Range, secondRange As Range) As Boolean
        If firstRange Is Nothing OrElse secondRange Is Nothing Then Return False

        Dim firstWorksheet As Worksheet = TryCast(firstRange.Parent, Worksheet)
        Dim secondWorksheet As Worksheet = TryCast(secondRange.Parent, Worksheet)
        If firstWorksheet Is Nothing OrElse secondWorksheet Is Nothing Then Return False

        Dim firstWorkbook As Workbook = TryCast(firstWorksheet.Parent, Workbook)
        Dim secondWorkbook As Workbook = TryCast(secondWorksheet.Parent, Workbook)
        If firstWorkbook Is Nothing OrElse secondWorkbook Is Nothing Then Return False

        Return String.Equals(firstWorkbook.FullName,
                             secondWorkbook.FullName,
                             StringComparison.OrdinalIgnoreCase) AndAlso
               String.Equals(firstWorksheet.Name,
                             secondWorksheet.Name,
                             StringComparison.OrdinalIgnoreCase)
    End Function

    Private Function GetPrimaryInputWorkbook() As Workbook
        Dim wb As Workbook = Me.RefEdit_FirstValues.ExcelWorkBook
        If wb Is Nothing Then wb = Me.RefEdit_SecondValues.ExcelWorkBook
        If wb Is Nothing Then wb = Me.RefEdit_RecordLabels.ExcelWorkBook
        If wb Is Nothing Then wb = Me.RefEdit_GroupingVariable.ExcelWorkBook
        If wb Is Nothing Then wb = AppGlobals.app.ActiveWorkbook
        Return wb
    End Function

    ''' <summary>
    ''' Trims whole-column/to-bottom RefEdit selections to the last used cell while
    ''' preserving explicitly bounded selections exactly as selected by the user.
    ''' </summary>
    Private Shared Function GetEffectiveLastRow(inputRange As Range) As Integer
        If inputRange Is Nothing Then Throw New ArgumentNullException(NameOf(inputRange))

        Dim ws As Worksheet = TryCast(inputRange.Parent, Worksheet)
        If ws Is Nothing Then Throw New InvalidOperationException("Unable to resolve the worksheet for the selected input range.")

        Dim firstRow As Integer = inputRange.Row
        Dim selectedLastRow As Long = CLng(firstRow) + CLng(inputRange.Rows.Count) - 1L

        If selectedLastRow < CLng(ws.Rows.Count) Then
            Return CInt(selectedLastRow)
        End If

        Dim lastUsedRow As Integer = firstRow
        For columnOffset As Integer = 0 To inputRange.Columns.Count - 1
            Dim columnNumber As Integer = inputRange.Column + columnOffset
            Dim bottomCell As Range = DirectCast(ws.Cells(ws.Rows.Count, columnNumber), Range)
            Dim candidate As Integer = CInt(bottomCell.End(XlDirection.xlUp).Row)
            If candidate >= firstRow AndAlso candidate > lastUsedRow Then lastUsedRow = candidate
        Next

        Return lastUsedRow
    End Function

    Private Shared Function GetCommonInputLastRow(ParamArray ranges() As Range) As Integer
        If ranges Is Nothing OrElse ranges.Length = 0 Then
            Throw New ArgumentException("At least one input range is required.", NameOf(ranges))
        End If

        Dim commonFirstRow As Integer = -1
        Dim commonLastRow As Integer = 0

        For Each inputRange As Range In ranges
            If inputRange Is Nothing Then Continue For

            If commonFirstRow < 0 Then
                commonFirstRow = inputRange.Row
            ElseIf inputRange.Row <> commonFirstRow Then
                Throw New ArgumentException("Aligned ladder-plot input ranges must start on the same row.")
            End If

            Dim lastRow As Integer = GetEffectiveLastRow(inputRange)
            If lastRow > commonLastRow Then commonLastRow = lastRow
        Next

        If commonLastRow <= 0 Then Throw New ArgumentException("No ladder-plot input data could be found.")
        Return commonLastRow
    End Function

    Private Shared Function BuildBoundedRange(inputRange As Range, lastRow As Integer) As Range
        If inputRange Is Nothing Then Throw New ArgumentNullException(NameOf(inputRange))

        Dim ws As Worksheet = TryCast(inputRange.Parent, Worksheet)
        If ws Is Nothing Then Throw New InvalidOperationException("Unable to resolve the worksheet for the selected input range.")
        If lastRow < inputRange.Row Then Throw New ArgumentException("The selected range does not contain any rows.")

        Dim firstCell As Range = DirectCast(ws.Cells(inputRange.Row, inputRange.Column), Range)
        Dim lastCell As Range = DirectCast(ws.Cells(lastRow, inputRange.Column), Range)
        Return ws.Range(firstCell, lastCell)
    End Function

    Private Shared Function ReadRangeVector(inputRange As Range) As Object()
        If inputRange Is Nothing Then Throw New ArgumentNullException(NameOf(inputRange))
        If inputRange.Areas.Count <> 1 OrElse inputRange.Columns.Count <> 1 Then
            Throw New ArgumentException("A ladder-plot input must be one continuous column range.", NameOf(inputRange))
        End If

        Dim rowCount As Integer = inputRange.Rows.Count
        If rowCount < 1 Then Return Array.Empty(Of Object)()

        Dim result(rowCount - 1) As Object
        Dim raw As Object = inputRange.Value(XlRangeValueDataType.xlRangeValueDefault)

        If rowCount = 1 Then
            result(0) = raw
            Return result
        End If

        Dim rawArray As Array = TryCast(raw, Array)
        If rawArray Is Nothing OrElse rawArray.Rank <> 2 Then
            Throw New InvalidOperationException("Unable to read the selected Excel range as a column vector.")
        End If

        Dim rowLower As Integer = rawArray.GetLowerBound(0)
        Dim columnLower As Integer = rawArray.GetLowerBound(1)
        For rowIndex As Integer = 0 To rowCount - 1
            result(rowIndex) = rawArray.GetValue(rowLower + rowIndex, columnLower)
        Next

        Return result
    End Function

    ''' <summary>
    ''' Mirrors the ladder backend closely enough to recognise a leading variable-name row.
    ''' </summary>
    Private Shared Function IsNumericDataValue(rawValue As Object) As Boolean
        If rawValue Is Nothing OrElse rawValue Is DBNull.Value Then Return False
        If TypeOf rawValue Is Boolean Then Return False

        Dim numericValue As Double
        If TypeOf rawValue Is Byte OrElse
           TypeOf rawValue Is SByte OrElse
           TypeOf rawValue Is Short OrElse
           TypeOf rawValue Is UShort OrElse
           TypeOf rawValue Is Integer OrElse
           TypeOf rawValue Is UInteger OrElse
           TypeOf rawValue Is Long OrElse
           TypeOf rawValue Is ULong OrElse
           TypeOf rawValue Is Single OrElse
           TypeOf rawValue Is Double OrElse
           TypeOf rawValue Is Decimal Then
            Try
                numericValue = Convert.ToDouble(rawValue, CultureInfo.InvariantCulture)
                Return Not Double.IsNaN(numericValue) AndAlso Not Double.IsInfinity(numericValue)
            Catch
                Return False
            End Try
        End If

        Dim text As String = TryCast(rawValue, String)
        If text Is Nothing Then Return False
        text = text.Trim()
        If text.Length = 0 Then Return False

        If Double.TryParse(text,
                           NumberStyles.Float Or NumberStyles.AllowThousands,
                           CultureInfo.CurrentCulture,
                           numericValue) OrElse
           Double.TryParse(text,
                           NumberStyles.Float Or NumberStyles.AllowThousands,
                           CultureInfo.InvariantCulture,
                           numericValue) Then
            Return Not Double.IsNaN(numericValue) AndAlso Not Double.IsInfinity(numericValue)
        End If

        Return False
    End Function

    Private Shared Function HasLeadingValueHeader(firstValues As Object(), secondValues As Object()) As Boolean
        If firstValues Is Nothing OrElse secondValues Is Nothing Then Return False
        If firstValues.Length < 2 OrElse secondValues.Length < 2 Then Return False

        Dim firstRowIsData As Boolean = IsNumericDataValue(firstValues(0)) AndAlso
                                        IsNumericDataValue(secondValues(0))
        Dim secondRowIsData As Boolean = IsNumericDataValue(firstValues(1)) AndAlso
                                         IsNumericDataValue(secondValues(1))

        Return Not firstRowIsData AndAlso secondRowIsData AndAlso
               Not IsNumericDataValue(firstValues(0)) AndAlso
               Not IsNumericDataValue(secondValues(0))
    End Function

    Private Shared Function DropFirstVectorItem(values As Object()) As Object()
        If values Is Nothing OrElse values.Length <= 1 Then Return Array.Empty(Of Object)()

        Dim result(values.Length - 2) As Object
        Array.Copy(values, 1, result, 0, result.Length)
        Return result
    End Function

    Private Shared Function HeaderText(rawValue As Object, fallback As String) As String
        If rawValue Is Nothing OrElse Convert.IsDBNull(rawValue) Then Return fallback
        Dim text As String = Convert.ToString(rawValue, CultureInfo.CurrentCulture)
        If String.IsNullOrWhiteSpace(text) Then Return fallback
        Return text.Trim()
    End Function

    ''' <summary>
    ''' If the user selected only the data cells rather than including the variable-name row,
    ''' use a text cell immediately above the selected range as the side label when available.
    ''' </summary>
    Private Shared Function GetAdjacentVariableName(inputRange As Range, fallback As String) As String
        If inputRange Is Nothing OrElse inputRange.Row <= 1 Then Return fallback

        Try
            Dim ws As Worksheet = TryCast(inputRange.Parent, Worksheet)
            If ws Is Nothing Then Return fallback

            Dim headerCell As Range = DirectCast(ws.Cells(inputRange.Row - 1, inputRange.Column), Range)
            Dim raw As Object = headerCell.Value(XlRangeValueDataType.xlRangeValueDefault)
            Dim text As String = TryCast(raw, String)
            If String.IsNullOrWhiteSpace(text) Then Return fallback
            Return text.Trim()
        Catch
            Return fallback
        End Try
    End Function

    Private Function GetPlotOptions() As LadderPlotOptions
        Return New LadderPlotOptions With {
            .OmitIncompleteRows = Me.ckOmitIncompleteRows.Checked,
            .HorizontalJitterFraction = If(Me.ckHorizontalJitter.Checked,
                                           CDbl(Me.nudHorizontalJitter.Value),
                                           0.0R)
        }
    End Function

    ''' <summary>
    ''' Builds renderer appearance from the controls that remain on the form. Properties
    ''' deliberately omitted from the GUI retain LadderPlotAppearance defaults.
    ''' </summary>
    Private Function GetPlotAppearance(firstAxisLabel As String,
                                       secondAxisLabel As String) As LadderPlotAppearance
        Dim yMinimum As Nullable(Of Double) = ParseOptionalAxisValue(Me.txtYAxisMinimum.Text, "Y minimum")
        Dim yMaximum As Nullable(Of Double) = ParseOptionalAxisValue(Me.txtYAxisMaximum.Text, "Y maximum")
        Dim majorUnit As Nullable(Of Double) = ParseOptionalAxisValue(Me.txtYAxisMajorUnit.Text, "Major interval")

        If majorUnit.HasValue AndAlso majorUnit.Value <= 0.0R Then
            Throw New ArgumentException("Major interval must be greater than zero.")
        End If
        If yMinimum.HasValue AndAlso yMaximum.HasValue AndAlso yMinimum.Value >= yMaximum.Value Then
            Throw New ArgumentException("Y minimum must be smaller than Y maximum.")
        End If

        Dim appearance As New LadderPlotAppearance()
        appearance.YAxisTitle = Me.txtYAxisTitle.Text.Trim()
        appearance.FirstAxisLabel = If(firstAxisLabel, String.Empty).Trim()
        appearance.SecondAxisLabel = If(secondAxisLabel, String.Empty).Trim()
        appearance.ColorMode = GetComboValue(Of LadderColorMode)(Me.cmbColorMode, "a color mode")
        appearance.SingleColor = ColorTranslator.ToOle(Me.pnlSingleColor.BackColor)
        appearance.ShowLines = Me.ckShowLines.Checked
        appearance.LineWeight = CSng(Me.nudConnectorWidth.Value)
        appearance.LineTransparency = CSng(Me.nudConnectorTransparency.Value / 100D)
        appearance.LineStyle = GetComboValue(Of LadderLineStyle)(Me.cmbConnectorLineStyle, "a connector line style")
        appearance.ShowMarkers = Me.ckShowMarkers.Checked
        appearance.MarkerStyle = GetComboValue(Of XlMarkerStyle)(Me.cmdMarkerStyle, "a marker style")
        appearance.MarkerSize = Decimal.ToInt32(Me.nudMarkerSize.Value)
        appearance.ShowLegend = Me.ckShowLegend.Checked
        appearance.ShowFirstEndpointLabels = Me.ckShowFirstEndpointLabels.Checked
        appearance.ShowSecondEndpointLabels = Me.ckShowSecondEndpointLabels.Checked
        appearance.EndpointLabelContent = GetEndpointLabelContent()
        appearance.EndpointValueDisplayFormat = Me.txtEndpointValueFormat.Text.Trim()
        appearance.AvoidEndpointLabelOverlap = Me.ckAvoidEndpointLabelOverlap.Checked
        appearance.ColorEndpointLabelsBySeries = Me.ckColorEndpointLabelsBySeries.Checked
        appearance.ShowSideLabels = Me.ckShowSideLabels.Checked
        appearance.ShowHorizontalGridlines = Me.ckShowHorizontalGridlines.Checked
        appearance.YAxisMinimum = yMinimum
        appearance.YAxisMaximum = yMaximum
        appearance.YAxisMajorUnit = majorUnit
        appearance.YAxisNumberFormat = Me.txtYAxisNumberFormat.Text.Trim()
        appearance.YAxisIncludeZero = Me.ckYAxisIncludeZero.Checked
        Return appearance
    End Function

    Private Function GetEndpointLabelContent() As LadderEndpointLabelContent
        Dim raw As String = If(Me.cmbEndpointLabelContent.Text, String.Empty).Trim()
        If raw.Length = 0 Then Return LadderEndpointLabelContent.RecordLabelAndValue

        Dim normalized As String = raw.ToLowerInvariant().
                                       Replace("_", " ").
                                       Replace("-", " ").
                                       Replace("&", " and ")
        normalized = String.Join(" ", normalized.Split(New Char() {" "c, ControlChars.Tab},
                                                       StringSplitOptions.RemoveEmptyEntries))

        Select Case normalized
            Case "value", "values"
                Return LadderEndpointLabelContent.Value
            Case "record label", "record labels", "label", "labels"
                Return LadderEndpointLabelContent.RecordLabel
            Case "record label + value", "record label+value", "record label and value",
                 "label + value", "label+value", "label and value",
                 "record labels + value", "record labels and value"
                Return LadderEndpointLabelContent.RecordLabelAndValue
            Case Else
                Throw New ArgumentException("Endpoint label content must be 'Value', 'Record label', or 'Record label + value'.")
        End Select
    End Function

    Private Shared Function ParseOptionalAxisValue(text As String,
                                                   displayName As String) As Nullable(Of Double)
        If String.IsNullOrWhiteSpace(text) Then Return Nothing

        Dim numericValue As Double
        Dim trimmed As String = text.Trim()
        If Double.TryParse(trimmed,
                           NumberStyles.Float Or NumberStyles.AllowThousands,
                           CultureInfo.CurrentCulture,
                           numericValue) OrElse
           Double.TryParse(trimmed,
                           NumberStyles.Float Or NumberStyles.AllowThousands,
                           CultureInfo.InvariantCulture,
                           numericValue) Then
            If Double.IsNaN(numericValue) OrElse Double.IsInfinity(numericValue) Then
                Throw New ArgumentException(displayName & " must be finite.")
            End If
            Return numericValue
        End If

        Throw New ArgumentException(displayName & " must be a valid number.")
    End Function

    Private Sub btCompute_Click(sender As Object, e As System.EventArgs) Handles btCompute.Click
        Dim previousCursor As Cursor = Me.Cursor

        Try
            If Me.CheckInputs() Then Exit Sub

            Me.Cursor = Cursors.WaitCursor
            Me.btCompute.Enabled = False

            Dim inputWorkbook As Workbook = Me.GetPrimaryInputWorkbook()
            If inputWorkbook Is Nothing Then Throw New InvalidOperationException("No input workbook is available.")
            inputWorkbook.Activate()

            Dim firstRange As Range = Me.ResolveRefEditRange(Me.RefEdit_FirstValues)
            Dim secondRange As Range = Me.ResolveRefEditRange(Me.RefEdit_SecondValues)
            Dim hasRecordLabels As Boolean = Not String.IsNullOrWhiteSpace(Me.RefEdit_RecordLabels.Address)
            Dim hasGrouping As Boolean = Not String.IsNullOrWhiteSpace(Me.RefEdit_GroupingVariable.Address)
            Dim recordRange As Range = If(hasRecordLabels,
                                          Me.ResolveRefEditRange(Me.RefEdit_RecordLabels),
                                          Nothing)
            Dim groupingRange As Range = If(hasGrouping,
                                            Me.ResolveRefEditRange(Me.RefEdit_GroupingVariable),
                                            Nothing)

            Dim inputRanges As New List(Of Range) From {firstRange, secondRange}
            If recordRange IsNot Nothing Then inputRanges.Add(recordRange)
            If groupingRange IsNot Nothing Then inputRanges.Add(groupingRange)

            Dim lastRow As Integer = GetCommonInputLastRow(inputRanges.ToArray())
            Dim firstValues As Object() = ReadRangeVector(BuildBoundedRange(firstRange, lastRow))
            Dim secondValues As Object() = ReadRangeVector(BuildBoundedRange(secondRange, lastRow))
            Dim recordLabels As Object() = If(recordRange Is Nothing,
                                               Nothing,
                                               ReadRangeVector(BuildBoundedRange(recordRange, lastRow)))
            Dim groupingValues As Object() = If(groupingRange Is Nothing,
                                                 Nothing,
                                                 ReadRangeVector(BuildBoundedRange(groupingRange, lastRow)))

            'Use the selected variable names for the two visible ladder-side labels.  When
            'the header row itself is part of the selection, capture it before removing it;
            'otherwise look one row above a data-only selection.
            Dim firstAxisLabel As String = GetAdjacentVariableName(firstRange, "Before")
            Dim secondAxisLabel As String = GetAdjacentVariableName(secondRange, "After")

            If HasLeadingValueHeader(firstValues, secondValues) Then
                firstAxisLabel = HeaderText(firstValues(0), firstAxisLabel)
                secondAxisLabel = HeaderText(secondValues(0), secondAxisLabel)
                firstValues = DropFirstVectorItem(firstValues)
                secondValues = DropFirstVectorItem(secondValues)
                If recordLabels IsNot Nothing Then recordLabels = DropFirstVectorItem(recordLabels)
                If groupingValues IsNot Nothing Then groupingValues = DropFirstVectorItem(groupingValues)
            End If

            Dim options As LadderPlotOptions = Me.GetPlotOptions()
            Dim result As LadderPlotResult = LadderPlot.Compute(firstValues,
                                                                secondValues,
                                                                recordLabels,
                                                                groupingValues,
                                                                options)

            Dim appearance As LadderPlotAppearance = Me.GetPlotAppearance(firstAxisLabel, secondAxisLabel)
            Dim outputWorksheet As Worksheet = Nothing
            Dim chartAnchor As Range = Nothing
            Me.ResolveOutputTarget(inputWorkbook, outputWorksheet, chartAnchor)

            DirectCast(outputWorksheet.Parent, Workbook).Activate()
            outputWorksheet.Activate()

            Dim chart As Chart = LadderPlotExcel.AddChart(outputWorksheet,
                                                          result,
                                                          appearance,
                                                          CDbl(chartAnchor.Left),
                                                          CDbl(chartAnchor.Top),
                                                          CDbl(Me.nudChartWidth.Value),
                                                          CDbl(Me.nudChartHeight.Value))

            'LadderPlotAppearance intentionally uses one series colour.  The current form
            'retains a separate connector-colour control, so honour it in Single color mode
            'after the renderer has created its data/helper series.  Group/observation modes
            'continue to colour both markers and connectors from the renderer palette.
            If appearance.ShowLines AndAlso appearance.ColorMode = LadderColorMode.SingleColor Then
                ApplySingleModeConnectorColor(chart,
                                              result.ObservationCount,
                                              ColorTranslator.ToOle(Me.pnlConnectorColor.BackColor))
            End If
        Catch ex As Exception
            CoreServices.Errors.LogAndThrow(ex,
                                            False,
                                            True,
                                            "Unable to create the ladder plot")
        Finally
            Me.btCompute.Enabled = True
            Me.Cursor = previousCursor
        End Try
    End Sub

    Private Shared Sub ApplySingleModeConnectorColor(chart As Chart,
                                                     observationCount As Integer,
                                                     connectorColor As Integer)
        If chart Is Nothing OrElse observationCount <= 0 Then Return

        Try
            Dim seriesCollection As SeriesCollection = DirectCast(chart.SeriesCollection(), SeriesCollection)
            Dim dataSeriesCount As Integer = Math.Min(observationCount, seriesCollection.Count)
            For seriesIndex As Integer = 1 To dataSeriesCount
                Dim series As Object = DirectCast(seriesCollection.Item(seriesIndex), Series)
                Try
                    series.Format.Line.ForeColor.RGB = connectorColor
                Catch
                    Try
                        series.Border.Color = connectorColor
                    Catch
                    End Try
                End Try
            Next
        Catch
            'Connector recolouring is cosmetic; retain a successfully created ladder plot.
        End Try
    End Sub

    Private Sub ResolveOutputTarget(inputWorkbook As Workbook,
                                    ByRef outputWorksheet As Worksheet,
                                    ByRef chartAnchor As Range)
        outputWorksheet = Nothing
        chartAnchor = Nothing

        If Me.optWorkbook.Checked Then
            Dim wb As Workbook = AppGlobals.app.Workbooks.Add()
            outputWorksheet = DirectCast(wb.Worksheets(1), Worksheet)
            chartAnchor = DirectCast(outputWorksheet.Cells(1, 1), Range)
            Return
        End If

        If Me.optWorksheet.Checked Then
            If inputWorkbook Is Nothing Then Throw New ArgumentNullException(NameOf(inputWorkbook))
            inputWorkbook.Activate()
            outputWorksheet = DirectCast(inputWorkbook.Worksheets.Add(), Worksheet)
            chartAnchor = DirectCast(outputWorksheet.Cells(1, 1), Range)
            Return
        End If

        If Me.optOutputRange.Checked Then
            Dim selectedRange As Range = Nothing
            If Not Me.TryResolveRange(Me.RefEditOutput,
                                      "Output range",
                                      requireSingleColumn:=False,
                                      requireSingleArea:=True,
                                      resolvedRange:=selectedRange) Then
                Throw New ArgumentException("A valid output range is required.")
            End If

            outputWorksheet = TryCast(selectedRange.Parent, Worksheet)
            If outputWorksheet Is Nothing Then Throw New InvalidOperationException("Unable to resolve the output worksheet.")
            chartAnchor = DirectCast(selectedRange.Cells(1, 1), Range)
            Return
        End If

        Throw New InvalidOperationException("Select an output destination.")
    End Sub

    Private Sub btnSingleColor_Click(sender As Object, e As System.EventArgs) Handles btnSingleColor.Click
        SelectColor(Me.pnlSingleColor)
    End Sub

    Private Sub btnConnectorColor_Click(sender As Object, e As System.EventArgs) Handles btnConnectorColor.Click
        SelectColor(Me.pnlConnectorColor)
    End Sub

    Private Shared Sub SelectColor(previewPanel As Panel)
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

End Class
