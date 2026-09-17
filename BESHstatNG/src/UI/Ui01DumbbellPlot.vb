Option Explicit On
Option Strict On
Option Infer On

Imports System
Imports System.Collections.Generic
Imports System.Drawing
Imports System.Globalization
Imports System.Windows.Forms
Imports BESHStatNG.AppInfrastructure
Imports Microsoft.Office.Interop.Excel

Public Class Ui01DumbbellPlot

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

        Me.RefEdit_CategoryLabels.ExcelConnector = AppGlobals.app
        Me.RefEdit_FirstValues.ExcelConnector = AppGlobals.app
        Me.RefEdit_SecondValues.ExcelConnector = AppGlobals.app
        Me.RefEdit_AdditionalObservationValues.ExcelConnector = AppGlobals.app
        Me.RefEdit_AdditionalObservationCat.ExcelConnector = AppGlobals.app
        Me.RefEditOutput.ExcelConnector = AppGlobals.app

        Me.InitializeChoiceControls()
        Me.InitializeAppearanceControls()
        Me.UpdateOutputRangeState()
        Me.WireHelp(Me.btnHelp)
    End Sub

    ''' <summary>
    ''' Populates user-facing choice controls while retaining strongly typed values
    ''' for the backend and Excel renderer.
    ''' </summary>
    Private Sub InitializeChoiceControls()
        With Me.cmbSortMode
            .BeginUpdate()
            Try
                .Items.Clear()
                .DropDownStyle = ComboBoxStyle.DropDownList
                .Items.Add(New ComboItem(Of DumbbellPlotSortMode)("Input order", DumbbellPlotSortMode.InputOrder))
                .Items.Add(New ComboItem(Of DumbbellPlotSortMode)("First value - ascending", DumbbellPlotSortMode.FirstAscending))
                .Items.Add(New ComboItem(Of DumbbellPlotSortMode)("First value - descending", DumbbellPlotSortMode.FirstDescending))
                .Items.Add(New ComboItem(Of DumbbellPlotSortMode)("Second value - ascending", DumbbellPlotSortMode.SecondAscending))
                .Items.Add(New ComboItem(Of DumbbellPlotSortMode)("Second value - descending", DumbbellPlotSortMode.SecondDescending))
                .Items.Add(New ComboItem(Of DumbbellPlotSortMode)("Difference - ascending", DumbbellPlotSortMode.DifferenceAscending))
                .Items.Add(New ComboItem(Of DumbbellPlotSortMode)("Difference - descending", DumbbellPlotSortMode.DifferenceDescending))
                .Items.Add(New ComboItem(Of DumbbellPlotSortMode)("Absolute difference - ascending", DumbbellPlotSortMode.AbsoluteDifferenceAscending))
                .Items.Add(New ComboItem(Of DumbbellPlotSortMode)("Absolute difference - descending", DumbbellPlotSortMode.AbsoluteDifferenceDescending))
                .Items.Add(New ComboItem(Of DumbbellPlotSortMode)("Category - ascending", DumbbellPlotSortMode.CategoryAscending))
                .Items.Add(New ComboItem(Of DumbbellPlotSortMode)("Category - descending", DumbbellPlotSortMode.CategoryDescending))
                SelectComboValue(Me.cmbSortMode, DumbbellPlotSortMode.InputOrder)
            Finally
                .EndUpdate()
            End Try
        End With

        'Shared endpoint marker-style selector.
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
                SelectComboValue(Me.cmdMarkerStyle, XlMarkerStyle.xlMarkerStyleCircle)
            Finally
                .EndUpdate()
            End Try
        End With

        With Me.cmbConnectorLineStyle
            .BeginUpdate()
            Try
                .Items.Clear()
                .DropDownStyle = ComboBoxStyle.DropDownList
                .Items.Add(New ComboItem(Of DumbbellConnectorLineStyle)("Solid", DumbbellConnectorLineStyle.Solid))
                .Items.Add(New ComboItem(Of DumbbellConnectorLineStyle)("Dash", DumbbellConnectorLineStyle.Dash))
                .Items.Add(New ComboItem(Of DumbbellConnectorLineStyle)("Dot", DumbbellConnectorLineStyle.Dot))
                .Items.Add(New ComboItem(Of DumbbellConnectorLineStyle)("Dash-dot", DumbbellConnectorLineStyle.DashDot))
                .Items.Add(New ComboItem(Of DumbbellConnectorLineStyle)("Dash-dot-dot", DumbbellConnectorLineStyle.DashDotDot))
                SelectComboValue(Me.cmbConnectorLineStyle, DumbbellConnectorLineStyle.Solid)
            Finally
                .EndUpdate()
            End Try
        End With
    End Sub

    ''' <summary>
    ''' Applies renderer colors to the preview panels. NumericUpDown and TextBox
    ''' defaults remain those selected in the current Windows Forms Designer.
    ''' </summary>
    Private Sub InitializeAppearanceControls()
        Dim defaults As New DumbbellPlotAppearance()

        Me.pnlFirstMarkerColor.BackColor = ColorTranslator.FromOle(defaults.FirstMarkerColor)
        Me.pnlSecondMarkerColor.BackColor = ColorTranslator.FromOle(defaults.SecondMarkerColor)
        Me.pnlConnectorColor.BackColor = ColorTranslator.FromOle(defaults.ConnectorColor)
        Me.pnlAddObsColor.BackColor = ColorTranslator.FromOle(defaults.AuxiliaryMarkerColor)

        Me.pnlFirstMarkerColor.BorderStyle = BorderStyle.FixedSingle
        Me.pnlSecondMarkerColor.BorderStyle = BorderStyle.FixedSingle
        Me.pnlConnectorColor.BorderStyle = BorderStyle.FixedSingle
        Me.pnlAddObsColor.BorderStyle = BorderStyle.FixedSingle
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

    ''' <summary>
    ''' Validates the selected endpoint ranges, optional observation pair, output
    ''' destination, and user-entered axis settings.
    ''' </summary>
    ''' <returns>True when validation failed.</returns>
    Private Function CheckInputs() As Boolean
        Dim inputWorkbook As Workbook = Me.GetPrimaryInputWorkbook()
        If inputWorkbook IsNot Nothing Then inputWorkbook.Activate()

        Dim categoryRange As Range = Nothing
        If Not Me.TryResolveRange(Me.RefEdit_CategoryLabels,
                                  "Category labels",
                                  requireSingleColumn:=True,
                                  requireSingleArea:=True,
                                  resolvedRange:=categoryRange) Then
            Return True
        End If

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

        If Not AreRangesOnSameWorksheet(categoryRange, firstRange) OrElse
           Not AreRangesOnSameWorksheet(categoryRange, secondRange) Then
            MsgBox("Category labels, First values, and Second values must be on the same worksheet.",
                   vbExclamation,
                   AppGlobals.gsAPP_TITLE)
            Return True
        End If

        If Not AreRangesRowAligned(categoryRange, firstRange) OrElse
           Not AreRangesRowAligned(categoryRange, secondRange) Then
            MsgBox("Category labels, First values, and Second values must start on the same row and contain the same number of rows.",
                   vbExclamation,
                   AppGlobals.gsAPP_TITLE)
            Return True
        End If

        Dim hasAuxCategory As Boolean = Not String.IsNullOrWhiteSpace(Me.RefEdit_AdditionalObservationCat.Address)
        Dim hasAuxValue As Boolean = Not String.IsNullOrWhiteSpace(Me.RefEdit_AdditionalObservationValues.Address)

        If hasAuxCategory Xor hasAuxValue Then
            MsgBox("Additional observation categories and observation values must either both be selected or both be left blank.",
                   vbExclamation,
                   AppGlobals.gsAPP_TITLE)
            Return True
        End If

        If hasAuxCategory Then
            Dim auxiliaryCategoryRange As Range = Nothing
            If Not Me.TryResolveRange(Me.RefEdit_AdditionalObservationCat,
                                      "Observation categories",
                                      requireSingleColumn:=True,
                                      requireSingleArea:=True,
                                      resolvedRange:=auxiliaryCategoryRange) Then
                Return True
            End If

            Dim auxiliaryValueRange As Range = Nothing
            If Not Me.TryResolveRange(Me.RefEdit_AdditionalObservationValues,
                                      "Observation values",
                                      requireSingleColumn:=True,
                                      requireSingleArea:=True,
                                      resolvedRange:=auxiliaryValueRange) Then
                Return True
            End If

            If Not AreRangesOnSameWorksheet(auxiliaryCategoryRange, auxiliaryValueRange) Then
                MsgBox("Observation categories and observation values must be on the same worksheet.",
                       vbExclamation,
                       AppGlobals.gsAPP_TITLE)
                Return True
            End If

            If Not AreRangesRowAligned(auxiliaryCategoryRange, auxiliaryValueRange) Then
                MsgBox("Observation categories and observation values must start on the same row and contain the same number of rows.",
                       vbExclamation,
                       AppGlobals.gsAPP_TITLE)
                Return True
            End If
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
            'Build both objects once during validation so an invalid sort/marker/axis
            'setting is reported before Excel chart creation begins.
            Dim ignoredOptions As DumbbellPlotOptions = Me.GetPlotOptions()
            Dim ignoredAppearance As DumbbellPlotAppearance = Me.GetPlotAppearance()
        Catch ex As ArgumentException
            MsgBox(ex.Message, vbExclamation, AppGlobals.gsAPP_TITLE)
            Return True
        Catch ex As InvalidOperationException
            MsgBox(ex.Message, vbExclamation, AppGlobals.gsAPP_TITLE)
            Return True
        End Try

        Return False
    End Function

    ''' <summary>
    ''' Resolves a RefEdit against the workbook captured by that RefEdit.
    ''' </summary>
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
        Dim wb As Workbook = Me.RefEdit_CategoryLabels.ExcelWorkBook
        If wb Is Nothing Then wb = AppGlobals.app.ActiveWorkbook
        Return wb
    End Function

    ''' <summary>
    ''' Trims whole-column / to-bottom RefEdit selections to the last used cell while
    ''' preserving explicitly bounded selections exactly as the user selected them.
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
                Throw New ArgumentException("Aligned dumbbell input ranges must start on the same row.")
            End If

            Dim lastRow As Integer = GetEffectiveLastRow(inputRange)
            If lastRow > commonLastRow Then commonLastRow = lastRow
        Next

        If commonLastRow <= 0 Then Throw New ArgumentException("No dumbbell input data could be found.")
        Return commonLastRow
    End Function

    Private Shared Function BuildBoundedRange(inputRange As Range, lastRow As Integer) As Range
        If inputRange Is Nothing Then Throw New ArgumentNullException(NameOf(inputRange))

        Dim ws As Worksheet = TryCast(inputRange.Parent, Worksheet)
        If ws Is Nothing Then Throw New InvalidOperationException("Unable to resolve the worksheet for the selected input range.")
        If lastRow < inputRange.Row Then Throw New ArgumentException("The selected range does not contain any rows.")

        Dim firstCell As Range = DirectCast(ws.Cells(inputRange.Row, inputRange.Column), Range)
        Dim lastColumn As Integer = inputRange.Column + inputRange.Columns.Count - 1
        Dim lastCell As Range = DirectCast(ws.Cells(lastRow, lastColumn), Range)
        Return ws.Range(firstCell, lastCell)
    End Function

    ''' <summary>
    ''' Reads one Excel column into a normal one-dimensional Object array. Range.Value
    ''' (rather than Value2) is intentional so date-formatted Excel cells can reach the
    ''' backend as DateTime values and trigger its date-axis formatting.
    ''' </summary>
    Private Shared Function ReadRangeVector(inputRange As Range) As Object()
        If inputRange Is Nothing Then Throw New ArgumentNullException(NameOf(inputRange))
        If inputRange.Areas.Count <> 1 OrElse inputRange.Columns.Count <> 1 Then
            Throw New ArgumentException("A dumbbell input must be one continuous column range.", NameOf(inputRange))
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
    ''' Returns True when a value can be used on the dumbbell X axis. This mirrors
    ''' the backend's numeric/date acceptance closely enough to recognise a header row
    ''' before the data are passed to DumbbellPlot.Compute.
    ''' </summary>
    Private Shared Function IsAxisDataValue(rawValue As Object) As Boolean
        If rawValue Is Nothing OrElse rawValue Is DBNull.Value Then Return False

        If TypeOf rawValue Is DateTime Then Return True

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

        Dim dateValue As DateTime
        Return DateTime.TryParse(text,
                                 CultureInfo.CurrentCulture,
                                 DateTimeStyles.AllowWhiteSpaces,
                                 dateValue) OrElse
               DateTime.TryParse(text,
                                 CultureInfo.InvariantCulture,
                                 DateTimeStyles.AllowWhiteSpaces,
                                 dateValue)
    End Function

    Private Shared Function DropFirstVectorItem(values As Object()) As Object()
        If values Is Nothing OrElse values.Length <= 1 Then Return Array.Empty(Of Object)()

        Dim result(values.Length - 2) As Object
        Array.Copy(values, 1, result, 0, result.Length)
        Return result
    End Function

    ''' <summary>
    ''' Removes one leading header row when the first endpoint row is not valid axis
    ''' data but the following row is. This allows convenient full-column selections
    ''' such as A:A, B:B and C:C when row 1 contains column headings.
    ''' </summary>
    Private Shared Sub RemoveEndpointHeaderIfPresent(ByRef categories As Object(),
                                                      ByRef firstValues As Object(),
                                                      ByRef secondValues As Object())
        If categories Is Nothing OrElse firstValues Is Nothing OrElse secondValues Is Nothing Then Return
        If categories.Length < 2 OrElse firstValues.Length < 2 OrElse secondValues.Length < 2 Then Return

        Dim firstRowIsData As Boolean = IsAxisDataValue(firstValues(0)) AndAlso IsAxisDataValue(secondValues(0))
        Dim secondRowIsData As Boolean = IsAxisDataValue(firstValues(1)) AndAlso IsAxisDataValue(secondValues(1))

        If Not firstRowIsData AndAlso secondRowIsData Then
            categories = DropFirstVectorItem(categories)
            firstValues = DropFirstVectorItem(firstValues)
            secondValues = DropFirstVectorItem(secondValues)
        End If
    End Sub

    ''' <summary>
    ''' Removes one leading header row from the optional observation pair when its
    ''' value column starts with text and the following row contains numeric/date data.
    ''' </summary>
    Private Shared Sub RemoveAuxiliaryHeaderIfPresent(ByRef categories As Object(),
                                                       ByRef values As Object())
        If categories Is Nothing OrElse values Is Nothing Then Return
        If categories.Length < 2 OrElse values.Length < 2 Then Return

        If Not IsAxisDataValue(values(0)) AndAlso IsAxisDataValue(values(1)) Then
            categories = DropFirstVectorItem(categories)
            values = DropFirstVectorItem(values)
        End If
    End Sub

    Private Function GetPlotOptions() As DumbbellPlotOptions
        Return New DumbbellPlotOptions With {
            .SortMode = GetComboValue(Of DumbbellPlotSortMode)(Me.cmbSortMode, "how categories should be sorted"),
            .ReverseCategoryOrder = Me.ckReverseCategoryOrder.Checked,
            .OmitIncompleteRows = Me.ckOmitIncompleteRows.Checked,
            .AuxiliaryJitterFraction = CDbl(Me.nudAddObsJitter.Value)
        }
    End Function

    Private Function GetPlotAppearance() As DumbbellPlotAppearance
        Dim xMinimum As Nullable(Of Double) = ParseOptionalAxisValue(Me.txtXAxisMinimum.Text, "X minimum", allowDate:=True)
        Dim xMaximum As Nullable(Of Double) = ParseOptionalAxisValue(Me.txtXAxisMaximum.Text, "X maximum", allowDate:=True)
        Dim majorUnit As Nullable(Of Double) = ParseOptionalAxisValue(Me.txtXAxisMajorUnit.Text, "Major interval", allowDate:=False)

        If majorUnit.HasValue AndAlso majorUnit.Value <= 0.0R Then
            Throw New ArgumentException("Major interval must be greater than zero.")
        End If
        If xMinimum.HasValue AndAlso xMaximum.HasValue AndAlso xMinimum.Value >= xMaximum.Value Then
            Throw New ArgumentException("X minimum must be smaller than X maximum.")
        End If

        Dim markerStyle As XlMarkerStyle = GetComboValue(Of XlMarkerStyle)(Me.cmdMarkerStyle, "an endpoint marker style")
        Dim hasAuxiliary As Boolean = Not String.IsNullOrWhiteSpace(Me.RefEdit_AdditionalObservationCat.Address) AndAlso
                                      Not String.IsNullOrWhiteSpace(Me.RefEdit_AdditionalObservationValues.Address)

        Return New DumbbellPlotAppearance With {
            .ChartTitle = Me.txtChartTitle.Text.Trim(),
            .XAxisTitle = Me.txtXAxisTitle.Text.Trim(),
            .FirstSeriesName = Me.txtFirstSeriesName.Text.Trim(),
            .SecondSeriesName = Me.txtSecondSeriesName.Text.Trim(),
            .AuxiliarySeriesName = Me.txtAdditionalSeriesName.Text.Trim(),
            .ShowLegend = Me.ckShowLegend.Checked,
            .ShowConnectors = True,
            .ShowAuxiliaryObservations = hasAuxiliary,
            .ShowCategoryLabels = True,
            .ShowEndpointValueLabels = Me.ckShowEndpointValueLabels.Checked,
            .FirstMarkerStyle = markerStyle,
            .SecondMarkerStyle = markerStyle,
            .AuxiliaryMarkerStyle = XlMarkerStyle.xlMarkerStyleCircle,
            .FirstMarkerSize = Decimal.ToInt32(Me.nudMarkerSize.Value),
            .SecondMarkerSize = Decimal.ToInt32(Me.nudMarkerSize.Value),
            .AuxiliaryMarkerSize = Decimal.ToInt32(Me.nudAddObsMarkerSize.Value),
            .FirstMarkerColor = ColorTranslator.ToOle(Me.pnlFirstMarkerColor.BackColor),
            .SecondMarkerColor = ColorTranslator.ToOle(Me.pnlSecondMarkerColor.BackColor),
            .AuxiliaryMarkerColor = ColorTranslator.ToOle(Me.pnlAddObsColor.BackColor),
            .AuxiliaryMarkerTransparency = CSng(Me.nudAddObsTransparency.Value / 100D),
            .ConnectorColor = ColorTranslator.ToOle(Me.pnlConnectorColor.BackColor),
            .ConnectorWeight = CSng(Me.nudConnectorWidth.Value),
            .ConnectorTransparency = CSng(Me.nudConnectorTransparency.Value / 100D),
            .ConnectorLineStyle = GetComboValue(Of DumbbellConnectorLineStyle)(Me.cmbConnectorLineStyle, "a connector line style"),
            .ShowVerticalGridlines = Me.ckShowVerticalGridlines.Checked,
            .ShowHorizontalGridlines = Me.ckShowHorizontalGridlines.Checked,
            .XAxisMinimum = xMinimum,
            .XAxisMaximum = xMaximum,
            .XAxisMajorUnit = majorUnit,
            .XAxisNumberFormat = Me.txtXAxisNumberFormat.Text.Trim()
        }
    End Function

    Private Shared Function ParseOptionalAxisValue(text As String,
                                                   displayName As String,
                                                   allowDate As Boolean) As Nullable(Of Double)
        If String.IsNullOrWhiteSpace(text) Then Return Nothing

        Dim trimmed As String = text.Trim()
        Dim numericValue As Double
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

        If allowDate Then
            Dim dateValue As DateTime
            If DateTime.TryParse(trimmed,
                                 CultureInfo.CurrentCulture,
                                 DateTimeStyles.AllowWhiteSpaces,
                                 dateValue) OrElse
               DateTime.TryParse(trimmed,
                                 CultureInfo.InvariantCulture,
                                 DateTimeStyles.AllowWhiteSpaces,
                                 dateValue) Then
                Return dateValue.ToOADate()
            End If
        End If

        If allowDate Then
            Throw New ArgumentException(displayName & " must be a valid number or date.")
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

            Dim categoryRange As Range = Me.ResolveRefEditRange(Me.RefEdit_CategoryLabels)
            Dim firstRange As Range = Me.ResolveRefEditRange(Me.RefEdit_FirstValues)
            Dim secondRange As Range = Me.ResolveRefEditRange(Me.RefEdit_SecondValues)

            Dim endpointLastRow As Integer = GetCommonInputLastRow(categoryRange, firstRange, secondRange)
            Dim categories As Object() = ReadRangeVector(BuildBoundedRange(categoryRange, endpointLastRow))
            Dim firstValues As Object() = ReadRangeVector(BuildBoundedRange(firstRange, endpointLastRow))
            Dim secondValues As Object() = ReadRangeVector(BuildBoundedRange(secondRange, endpointLastRow))
            RemoveEndpointHeaderIfPresent(categories, firstValues, secondValues)

            Dim options As DumbbellPlotOptions = Me.GetPlotOptions()
            Dim result As DumbbellPlotResult

            Dim hasAuxiliary As Boolean = Not String.IsNullOrWhiteSpace(Me.RefEdit_AdditionalObservationCat.Address) AndAlso
                                          Not String.IsNullOrWhiteSpace(Me.RefEdit_AdditionalObservationValues.Address)
            If hasAuxiliary Then
                Dim auxiliaryCategoryRange As Range = Me.ResolveRefEditRange(Me.RefEdit_AdditionalObservationCat)
                Dim auxiliaryValueRange As Range = Me.ResolveRefEditRange(Me.RefEdit_AdditionalObservationValues)
                Dim auxiliaryLastRow As Integer = GetCommonInputLastRow(auxiliaryCategoryRange, auxiliaryValueRange)

                Dim auxiliaryCategories As Object() = ReadRangeVector(BuildBoundedRange(auxiliaryCategoryRange, auxiliaryLastRow))
                Dim auxiliaryValues As Object() = ReadRangeVector(BuildBoundedRange(auxiliaryValueRange, auxiliaryLastRow))
                RemoveAuxiliaryHeaderIfPresent(auxiliaryCategories, auxiliaryValues)

                result = DumbbellPlot.ComputeWithAuxiliary(categories,
                                                           firstValues,
                                                           secondValues,
                                                           auxiliaryCategories,
                                                           auxiliaryValues,
                                                           options)
            Else
                result = DumbbellPlot.Compute(categories, firstValues, secondValues, options)
            End If

            Dim appearance As DumbbellPlotAppearance = Me.GetPlotAppearance()
            Dim outputWorksheet As Worksheet = Nothing
            Dim chartAnchor As Range = Nothing
            Me.ResolveOutputTarget(inputWorkbook, outputWorksheet, chartAnchor)

            DirectCast(outputWorksheet.Parent, Workbook).Activate()
            outputWorksheet.Activate()

            DumbbellPlotExcel.AddChart(outputWorksheet,
                                       result,
                                       appearance,
                                       CDbl(chartAnchor.Left),
                                       CDbl(chartAnchor.Top),
                                       CDbl(Me.nudChartWidth.Value),
                                       CDbl(Me.nudChartHeight.Value))
        Catch ex As Exception
            CoreServices.Errors.LogAndThrow(ex,
                                            False,
                                            True,
                                            "Unable to create the dumbbell plot")
        Finally
            Me.btCompute.Enabled = True
            Me.Cursor = previousCursor
        End Try
    End Sub

    ''' <summary>
    ''' Resolves the standard BESHStatNG graphical output choices to one worksheet
    ''' and top-left chart anchor. No cells are overwritten by the chart renderer.
    ''' </summary>
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

    Private Sub btnFirstMarkerColor_Click(sender As Object, e As System.EventArgs) Handles btnFirstMarkerColor.Click
        SelectColor(Me.pnlFirstMarkerColor)
    End Sub

    Private Sub btnSecondMarkerColor_Click(sender As Object, e As System.EventArgs) Handles btnSecondMarkerColor.Click
        SelectColor(Me.pnlSecondMarkerColor)
    End Sub

    Private Sub btnConnectorColor_Click(sender As Object, e As System.EventArgs) Handles btnConnectorColor.Click
        SelectColor(Me.pnlConnectorColor)
    End Sub

    Private Sub btnAddObsColor_Click(sender As Object, e As System.EventArgs) Handles btnAddObsColor.Click
        SelectColor(Me.pnlAddObsColor)
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
