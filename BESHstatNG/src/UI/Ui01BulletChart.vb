Option Explicit On
Option Strict Off
Option Infer On

Imports System
Imports System.Collections.Generic
Imports System.Drawing
Imports System.Globalization
Imports System.Windows.Forms
Imports BESHStatNG.AppInfrastructure
Imports Microsoft.Office.Interop.Excel

Public Class Ui01BulletChart

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

    Private NotInheritable Class BandPaletteChoice
        Public Sub New(displayText As String, colors As Integer())
            If String.IsNullOrWhiteSpace(displayText) Then Throw New ArgumentException("Palette name cannot be blank.", NameOf(displayText))
            If colors Is Nothing OrElse colors.Length = 0 Then Throw New ArgumentException("Palette must contain at least one color.", NameOf(colors))

            Me.DisplayText = displayText
            Me.Colors = DirectCast(colors.Clone(), Integer())
        End Sub

        Public ReadOnly Property DisplayText As String
        Public ReadOnly Property Colors As Integer()

        Public Overrides Function ToString() As String
            Return Me.DisplayText
        End Function
    End Class

    Public Sub New(tagn As Integer)
        InitializeComponent()
        Me.Tag = tagn

        Me.RefEdit_MeasureLabels.ExcelConnector = AppGlobals.app
        Me.RefEdit_Subtitles.ExcelConnector = AppGlobals.app
        Me.RefEdit_ActualValues.ExcelConnector = AppGlobals.app
        Me.RefEdit_TargetValues.ExcelConnector = AppGlobals.app
        Me.RefEdit_QualitativeRanges.ExcelConnector = AppGlobals.app
        Me.RefEdit_Directions.ExcelConnector = AppGlobals.app
        Me.RefEdit_ScaleMaximums.ExcelConnector = AppGlobals.app
        Me.RefEdit_MajorIntervals.ExcelConnector = AppGlobals.app
        Me.RefEditOutput.ExcelConnector = AppGlobals.app

        Me.InitializeChoiceControls()
        Me.InitializeControlDefaults()
        Me.InitializeAppearanceControls()
        Me.UpdateOutputRangeState()
        Me.UpdateOptionControlStates()
        Me.WireHelp(Me.btnHelp)
    End Sub

    Private Sub InitializeChoiceControls()
        With Me.cmbDefaultDirection
            .BeginUpdate()
            Try
                .Items.Clear()
                .DropDownStyle = ComboBoxStyle.DropDownList
                .Items.Add(New ComboItem(Of BulletChartDirection)("Higher is better", BulletChartDirection.HigherIsBetter))
                .Items.Add(New ComboItem(Of BulletChartDirection)("Lower is better", BulletChartDirection.LowerIsBetter))
                SelectComboValue(Me.cmbDefaultDirection, (New BulletChartOptions()).DefaultDirection)
            Finally
                .EndUpdate()
            End Try
        End With

        Dim defaults As New BulletChartAppearance()
        With Me.cmbBandPalette
            .BeginUpdate()
            Try
                .Items.Clear()
                .DropDownStyle = ComboBoxStyle.DropDownList
                .Items.Add(New BandPaletteChoice("Grayscale", defaults.BandColorsByDesirability))
                .Items.Add(New BandPaletteChoice("Blue", New Integer() {
                    OleColor(103, 132, 158),
                    OleColor(132, 158, 181),
                    OleColor(164, 184, 201),
                    OleColor(197, 211, 222),
                    OleColor(226, 235, 242)
                }))
                .Items.Add(New BandPaletteChoice("Green", New Integer() {
                    OleColor(105, 139, 107),
                    OleColor(134, 164, 135),
                    OleColor(166, 190, 166),
                    OleColor(199, 216, 198),
                    OleColor(228, 239, 227)
                }))
                .Items.Add(New BandPaletteChoice("Warm", New Integer() {
                    OleColor(151, 119, 94),
                    OleColor(177, 148, 124),
                    OleColor(202, 178, 157),
                    OleColor(224, 207, 190),
                    OleColor(241, 232, 222)
                }))
                .SelectedIndex = 0
            Finally
                .EndUpdate()
            End Try
        End With
    End Sub

    ''' <summary>
    ''' Applies the backend/renderer defaults explicitly so the UI remains synchronized
    ''' even if a Designer value is accidentally changed later.
    ''' </summary>
    Private Sub InitializeControlDefaults()
        Dim options As New BulletChartOptions()
        Dim appearance As New BulletChartAppearance()

        Me.ckOmitIncompleteRows.Checked = options.OmitIncompleteRows

        Me.nudTargetMajorTickCount.Minimum = 3D
        Me.nudTargetMajorTickCount.Maximum = 20D
        SetNumericValue(Me.nudTargetMajorTickCount, CDec(options.TargetMajorTickCount))

        Me.ckShowMeasureLabels.Checked = appearance.ShowMeasureLabels
        Me.ckShowSubtitles.Checked = appearance.ShowSubtitles
        Me.ckShowScaleLabels.Checked = appearance.ShowScaleLabels
        Me.ckShowScaleTickMarks.Checked = appearance.ShowScaleTickMarks
        Me.ckShowActualValueLabels.Checked = appearance.ShowActualValueLabels
        Me.ckShowTargetValueLabels.Checked = appearance.ShowTargetValueLabels
        Me.txtScaleNumberFormat.Text = appearance.ScaleNumberFormat
        Me.txtValueNumberFormat.Text = appearance.ValueNumberFormat

        Me.ckShowBandOutlines.Checked = appearance.ShowQualitativeBandOutlines
        Me.ckActualBarOutline.Checked = appearance.ShowActualBarOutline

        Me.nudActualBarHeight.Minimum = 1D
        Me.nudActualBarHeight.Maximum = 100D
        SetNumericValue(Me.nudActualBarHeight, CDec(appearance.ActualBarHeightFraction * 100.0R))

        Me.nudTargetMarkerHeight.Minimum = 1D
        Me.nudTargetMarkerHeight.Maximum = 100D
        SetNumericValue(Me.nudTargetMarkerHeight, CDec(appearance.TargetMarkerHeightFraction * 100.0R))
        SetNumericValue(Me.nudTargetMarkerWidth, CDec(appearance.TargetMarkerWeight))

        SetNumericValue(Me.nudChartWidth, 760D)
        SetNumericValue(Me.nudChartHeight, 460D)

        If Not Me.optOutputRange.Checked AndAlso Not Me.optWorksheet.Checked AndAlso Not Me.optWorkbook.Checked Then
            Me.optWorksheet.Checked = True
        End If
    End Sub

    Private Sub InitializeAppearanceControls()
        Dim defaults As New BulletChartAppearance()

        Me.pnlActualBarColor.BackColor = ColorTranslator.FromOle(defaults.ActualBarColor)
        Me.pnlTargetColor.BackColor = ColorTranslator.FromOle(defaults.TargetMarkerColor)
        Me.pnlActualBarColor.BorderStyle = BorderStyle.FixedSingle
        Me.pnlTargetColor.BorderStyle = BorderStyle.FixedSingle
    End Sub

    Private Shared Function OleColor(red As Integer, green As Integer, blue As Integer) As Integer
        Return ColorTranslator.ToOle(Color.FromArgb(red, green, blue))
    End Function

    Private Shared Sub SetNumericValue(control As NumericUpDown, value As Decimal)
        If control Is Nothing Then Return
        If value < control.Minimum Then value = control.Minimum
        If value > control.Maximum Then value = control.Maximum
        control.Value = value
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

    Private Sub ckShowScaleLabels_CheckedChanged(sender As Object, e As System.EventArgs) Handles ckShowScaleLabels.CheckedChanged
        Me.UpdateOptionControlStates()
    End Sub

    Private Sub ckShowActualValueLabels_CheckedChanged(sender As Object, e As System.EventArgs) Handles ckShowActualValueLabels.CheckedChanged
        Me.UpdateOptionControlStates()
    End Sub

    Private Sub ckShowTargetValueLabels_CheckedChanged(sender As Object, e As System.EventArgs) Handles ckShowTargetValueLabels.CheckedChanged
        Me.UpdateOptionControlStates()
    End Sub

    Private Sub UpdateOptionControlStates()
        Me.txtScaleNumberFormat.Enabled = Me.ckShowScaleLabels.Checked
        Me.lblScaleNumberFormat.Enabled = Me.txtScaleNumberFormat.Enabled

        Dim showValueLabels As Boolean = Me.ckShowActualValueLabels.Checked OrElse
                                         Me.ckShowTargetValueLabels.Checked
        Me.txtValueNumberFormat.Enabled = showValueLabels
        Me.lblValueNumberFormat.Enabled = showValueLabels
    End Sub

    ''' <summary>
    ''' Validates the selected range shapes and UI settings before data import or chart creation.
    ''' Returns True when validation failed, following the convention used by the other graphics forms.
    ''' </summary>
    Private Function CheckInputs() As Boolean
        Try
            Dim inputWorkbook As Workbook = Me.GetPrimaryInputWorkbook()
            If inputWorkbook IsNot Nothing Then inputWorkbook.Activate()

            Dim measureRange As Range = Nothing
            If Not Me.TryResolveRange(Me.RefEdit_MeasureLabels,
                                      "Measure labels",
                                      requireSingleColumn:=True,
                                      requireSingleArea:=True,
                                      resolvedRange:=measureRange) Then
                Return True
            End If

            Dim actualRange As Range = Nothing
            If Not Me.TryResolveRange(Me.RefEdit_ActualValues,
                                      "Actual values",
                                      requireSingleColumn:=True,
                                      requireSingleArea:=True,
                                      resolvedRange:=actualRange) Then
                Return True
            End If
            If Not Me.ValidateAlignedInputRange(measureRange, actualRange, "Actual values") Then Return True

            Dim targetRange As Range = Nothing
            If Not Me.TryResolveRange(Me.RefEdit_TargetValues,
                                      "Target values",
                                      requireSingleColumn:=True,
                                      requireSingleArea:=True,
                                      resolvedRange:=targetRange) Then
                Return True
            End If
            If Not Me.ValidateAlignedInputRange(measureRange, targetRange, "Target values") Then Return True

            Dim qualitativeRange As Range = Nothing
            If Not Me.TryResolveRange(Me.RefEdit_QualitativeRanges,
                                      "Qualitative ranges",
                                      requireSingleColumn:=False,
                                      requireSingleArea:=True,
                                      resolvedRange:=qualitativeRange) Then
                Return True
            End If
            If qualitativeRange.Columns.Count < 2 OrElse qualitativeRange.Columns.Count > 5 Then
                MsgBox("Qualitative ranges must contain between 2 and 5 adjacent columns of cumulative upper boundaries.",
                       vbExclamation,
                       AppGlobals.gsAPP_TITLE)
                Return True
            End If
            If Not Me.ValidateAlignedInputRange(measureRange, qualitativeRange, "Qualitative ranges") Then Return True

            If Not Me.ValidateOptionalAlignedRange(Me.RefEdit_Subtitles,
                                                   measureRange,
                                                   "Subtitle / unit",
                                                   True) Then Return True
            If Not Me.ValidateOptionalAlignedRange(Me.RefEdit_Directions,
                                                   measureRange,
                                                   "Direction",
                                                   True) Then Return True
            If Not Me.ValidateOptionalAlignedRange(Me.RefEdit_ScaleMaximums,
                                                   measureRange,
                                                   "Scale maximum",
                                                   True) Then Return True
            If Not Me.ValidateOptionalAlignedRange(Me.RefEdit_MajorIntervals,
                                                   measureRange,
                                                   "Major interval",
                                                   True) Then Return True

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

            Dim ignoredOptions As BulletChartOptions = Me.GetPlotOptions()
            Dim ignoredAppearance As BulletChartAppearance = Me.GetPlotAppearance()
            ValidateOptionalNumericFormat(Me.txtScaleNumberFormat.Text, "Scale number format")
            ValidateOptionalNumericFormat(Me.txtValueNumberFormat.Text, "Value number format")
        Catch ex As ArgumentException
            MsgBox(ex.Message, vbExclamation, AppGlobals.gsAPP_TITLE)
            Return True
        Catch ex As InvalidOperationException
            MsgBox(ex.Message, vbExclamation, AppGlobals.gsAPP_TITLE)
            Return True
        Catch ex As Exception
            CoreServices.Errors.LogAndThrow(ex,
                                            False,
                                            True,
                                            "Unable to validate the bullet-chart inputs")
            Return True
        End Try

        Return False
    End Function

    Private Function ValidateOptionalAlignedRange(refEdit As Excel2007RefEdit,
                                                  referenceRange As Range,
                                                  displayName As String,
                                                  requireSingleColumn As Boolean) As Boolean
        If refEdit Is Nothing OrElse String.IsNullOrWhiteSpace(refEdit.Address) Then Return True

        Dim optionalRange As Range = Nothing
        If Not Me.TryResolveRange(refEdit,
                                  displayName,
                                  requireSingleColumn,
                                  True,
                                  optionalRange) Then
            Return False
        End If

        Return Me.ValidateAlignedInputRange(referenceRange, optionalRange, displayName)
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
            MsgBox("All bullet-chart input ranges must be on the same worksheet. " &
                   displayName & " is on a different worksheet.",
                   vbExclamation,
                   AppGlobals.gsAPP_TITLE)
            Return False
        End If

        If Not AreRangesRowAligned(referenceRange, otherRange) Then
            MsgBox(displayName & " must start on the same row and contain the same number of rows as Measure labels.",
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
        Dim wb As Workbook = Me.RefEdit_MeasureLabels.ExcelWorkBook
        If wb Is Nothing Then wb = Me.RefEdit_ActualValues.ExcelWorkBook
        If wb Is Nothing Then wb = Me.RefEdit_TargetValues.ExcelWorkBook
        If wb Is Nothing Then wb = Me.RefEdit_QualitativeRanges.ExcelWorkBook
        If wb Is Nothing Then wb = Me.RefEdit_Subtitles.ExcelWorkBook
        If wb Is Nothing Then wb = Me.RefEdit_Directions.ExcelWorkBook
        If wb Is Nothing Then wb = Me.RefEdit_ScaleMaximums.ExcelWorkBook
        If wb Is Nothing Then wb = Me.RefEdit_MajorIntervals.ExcelWorkBook
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

    Private Shared Function GetCommonInputLastRow(ranges As IEnumerable(Of Range)) As Integer
        If ranges Is Nothing Then Throw New ArgumentNullException(NameOf(ranges))

        Dim commonFirstRow As Integer = -1
        Dim commonLastRow As Integer = 0
        Dim foundRange As Boolean = False

        For Each inputRange As Range In ranges
            If inputRange Is Nothing Then Continue For
            foundRange = True

            If commonFirstRow < 0 Then
                commonFirstRow = inputRange.Row
            ElseIf inputRange.Row <> commonFirstRow Then
                Throw New ArgumentException("Aligned bullet-chart input ranges must start on the same row.")
            End If

            Dim lastRow As Integer = GetEffectiveLastRow(inputRange)
            If lastRow > commonLastRow Then commonLastRow = lastRow
        Next

        If Not foundRange OrElse commonLastRow <= 0 Then
            Throw New ArgumentException("No bullet-chart input data could be found.")
        End If
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

    Private Shared Function ReadRangeVector(inputRange As Range) As Object()
        If inputRange Is Nothing Then Throw New ArgumentNullException(NameOf(inputRange))
        If inputRange.Areas.Count <> 1 OrElse inputRange.Columns.Count <> 1 Then
            Throw New ArgumentException("A bullet-chart vector input must be one continuous column range.", NameOf(inputRange))
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

    Private Shared Function ReadRangeMatrix(inputRange As Range) As Object(,)
        If inputRange Is Nothing Then Throw New ArgumentNullException(NameOf(inputRange))
        If inputRange.Areas.Count <> 1 Then
            Throw New ArgumentException("A bullet-chart matrix input must be one continuous range.", NameOf(inputRange))
        End If

        Dim rowCount As Integer = inputRange.Rows.Count
        Dim columnCount As Integer = inputRange.Columns.Count
        If rowCount < 1 OrElse columnCount < 1 Then
            Return DirectCast(Array.CreateInstance(GetType(Object), 0, 0), Object(,))
        End If

        Dim result(rowCount - 1, columnCount - 1) As Object
        Dim raw As Object = inputRange.Value(XlRangeValueDataType.xlRangeValueDefault)

        If rowCount = 1 AndAlso columnCount = 1 Then
            result(0, 0) = raw
            Return result
        End If

        Dim rawArray As Array = TryCast(raw, Array)
        If rawArray Is Nothing OrElse rawArray.Rank <> 2 Then
            Throw New InvalidOperationException("Unable to read the selected Excel range as a matrix.")
        End If

        Dim rowLower As Integer = rawArray.GetLowerBound(0)
        Dim columnLower As Integer = rawArray.GetLowerBound(1)
        For rowIndex As Integer = 0 To rowCount - 1
            For columnIndex As Integer = 0 To columnCount - 1
                result(rowIndex, columnIndex) = rawArray.GetValue(rowLower + rowIndex,
                                                                 columnLower + columnIndex)
            Next
        Next

        Return result
    End Function

    Private Shared Function IsMissingValue(rawValue As Object) As Boolean
        If rawValue Is Nothing OrElse rawValue Is DBNull.Value Then Return True
        Dim text As String = TryCast(rawValue, String)
        Return text IsNot Nothing AndAlso String.IsNullOrWhiteSpace(text)
    End Function

    Private Shared Function IsNumericDataValue(rawValue As Object) As Boolean
        If IsMissingValue(rawValue) Then Return False
        If TypeOf rawValue Is Boolean OrElse TypeOf rawValue Is Date Then Return False

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
            Dim number As Double = Convert.ToDouble(rawValue, CultureInfo.InvariantCulture)
            Return Not Double.IsNaN(number) AndAlso Not Double.IsInfinity(number)
        End If

        Dim text As String = Convert.ToString(rawValue, CultureInfo.CurrentCulture).Trim()
        If text = String.Empty Then Return False

        Dim parsed As Double
        If Double.TryParse(text,
                           NumberStyles.Float Or NumberStyles.AllowThousands,
                           CultureInfo.CurrentCulture,
                           parsed) OrElse
           Double.TryParse(text,
                           NumberStyles.Float Or NumberStyles.AllowThousands,
                           CultureInfo.InvariantCulture,
                           parsed) Then
            Return Not Double.IsNaN(parsed) AndAlso Not Double.IsInfinity(parsed)
        End If

        Return False
    End Function

    Private Shared Function IsNonNumericHeaderText(rawValue As Object) As Boolean
        Dim text As String = TryCast(rawValue, String)
        If text Is Nothing OrElse String.IsNullOrWhiteSpace(text) Then Return False
        Return Not IsNumericDataValue(rawValue)
    End Function

    ''' <summary>
    ''' Recognises a conventional header row without ever using the text measure-label column
    ''' as evidence (all valid measure labels are text).  At least two numeric-input headings
    ''' must be textual, and the following row must look like real bullet-chart data.
    ''' </summary>
    Private Shared Function HasLeadingHeader(actualValues As Object(),
                                             targetValues As Object(),
                                             qualitativeRanges As Object(,)) As Boolean
        If actualValues Is Nothing OrElse targetValues Is Nothing OrElse qualitativeRanges Is Nothing Then Return False
        If actualValues.Length < 2 OrElse targetValues.Length < 2 OrElse qualitativeRanges.GetLength(0) < 2 Then Return False

        Dim headerTextCount As Integer = 0
        If IsNonNumericHeaderText(actualValues(0)) Then headerTextCount += 1
        If IsNonNumericHeaderText(targetValues(0)) Then headerTextCount += 1
        For columnIndex As Integer = 0 To qualitativeRanges.GetLength(1) - 1
            If IsNonNumericHeaderText(qualitativeRanges(0, columnIndex)) Then headerTextCount += 1
        Next
        If headerTextCount < 2 Then Return False

        If Not IsNumericDataValue(actualValues(1)) OrElse Not IsNumericDataValue(targetValues(1)) Then Return False

        Dim numericBoundaries As Integer = 0
        For columnIndex As Integer = 0 To qualitativeRanges.GetLength(1) - 1
            If IsNumericDataValue(qualitativeRanges(1, columnIndex)) Then
                numericBoundaries += 1
            ElseIf IsMissingValue(qualitativeRanges(1, columnIndex)) Then
                Exit For
            Else
                Return False
            End If
        Next

        Return numericBoundaries >= 2
    End Function

    Private Shared Function DropFirstVectorItem(values As Object()) As Object()
        If values Is Nothing Then Return Nothing
        If values.Length <= 1 Then Return Array.Empty(Of Object)()

        Dim result(values.Length - 2) As Object
        Array.Copy(values, 1, result, 0, result.Length)
        Return result
    End Function

    Private Shared Function DropFirstMatrixRow(values As Object(,)) As Object(,)
        If values Is Nothing Then Return Nothing

        Dim rowCount As Integer = values.GetLength(0)
        Dim columnCount As Integer = values.GetLength(1)
        If rowCount <= 1 Then
            Return DirectCast(Array.CreateInstance(GetType(Object), 0, columnCount), Object(,))
        End If

        Dim result(rowCount - 2, columnCount - 1) As Object
        For rowIndex As Integer = 1 To rowCount - 1
            For columnIndex As Integer = 0 To columnCount - 1
                result(rowIndex - 1, columnIndex) = values(rowIndex, columnIndex)
            Next
        Next
        Return result
    End Function

    Private Function GetPlotOptions() As BulletChartOptions
        Return New BulletChartOptions With {
            .OmitIncompleteRows = Me.ckOmitIncompleteRows.Checked,
            .DefaultDirection = GetComboValue(Of BulletChartDirection)(Me.cmbDefaultDirection, "a default direction"),
            .TargetMajorTickCount = CInt(Me.nudTargetMajorTickCount.Value)
        }
    End Function

    Private Function GetPlotAppearance() As BulletChartAppearance
        Dim palette As BandPaletteChoice = TryCast(Me.cmbBandPalette.SelectedItem, BandPaletteChoice)
        If palette Is Nothing Then Throw New InvalidOperationException("Select a range palette.")

        Dim appearance As New BulletChartAppearance With {
            .BandColorsByDesirability = DirectCast(palette.Colors.Clone(), Integer()),
            .ShowQualitativeBandOutlines = Me.ckShowBandOutlines.Checked,
            .ActualBarColor = ColorTranslator.ToOle(Me.pnlActualBarColor.BackColor),
            .ShowActualBarOutline = Me.ckActualBarOutline.Checked,
            .ActualBarHeightFraction = CDbl(Me.nudActualBarHeight.Value) / 100.0R,
            .TargetMarkerColor = ColorTranslator.ToOle(Me.pnlTargetColor.BackColor),
            .TargetMarkerWeight = CSng(Me.nudTargetMarkerWidth.Value),
            .TargetMarkerHeightFraction = CDbl(Me.nudTargetMarkerHeight.Value) / 100.0R,
            .ShowMeasureLabels = Me.ckShowMeasureLabels.Checked,
            .ShowSubtitles = Me.ckShowSubtitles.Checked,
            .ShowScaleLabels = Me.ckShowScaleLabels.Checked,
            .ShowScaleTickMarks = Me.ckShowScaleTickMarks.Checked,
            .ShowActualValueLabels = Me.ckShowActualValueLabels.Checked,
            .ShowTargetValueLabels = Me.ckShowTargetValueLabels.Checked,
            .ScaleNumberFormat = Me.txtScaleNumberFormat.Text.Trim(),
            .ValueNumberFormat = Me.txtValueNumberFormat.Text.Trim()
        }

        'The current form intentionally does not expose an overall chart-title control;
        'retain the renderer's standard title until/unless one is added to the Designer.
        Return appearance
    End Function

    Private Shared Sub ValidateOptionalNumericFormat(formatText As String, displayName As String)
        If String.IsNullOrWhiteSpace(formatText) Then Return

        Try
            Dim ignored As String = 12.5R.ToString(formatText.Trim(), CultureInfo.CurrentCulture)
        Catch ex As FormatException
            Throw New ArgumentException(displayName & " is not a valid .NET numeric format string.")
        End Try
    End Sub

    Private Sub btCompute_Click(sender As Object, e As System.EventArgs) Handles btCompute.Click
        Dim previousCursor As Cursor = Me.Cursor

        Try
            If Me.CheckInputs() Then Exit Sub

            Me.Cursor = Cursors.WaitCursor
            Me.btCompute.Enabled = False

            Dim inputWorkbook As Workbook = Me.GetPrimaryInputWorkbook()
            If inputWorkbook Is Nothing Then Throw New InvalidOperationException("No input workbook is available.")
            inputWorkbook.Activate()

            Dim measureRange As Range = Me.ResolveRefEditRange(Me.RefEdit_MeasureLabels)
            Dim actualRange As Range = Me.ResolveRefEditRange(Me.RefEdit_ActualValues)
            Dim targetRange As Range = Me.ResolveRefEditRange(Me.RefEdit_TargetValues)
            Dim qualitativeRange As Range = Me.ResolveRefEditRange(Me.RefEdit_QualitativeRanges)

            Dim subtitleRange As Range = Me.ResolveOptionalRefEditRange(Me.RefEdit_Subtitles)
            Dim directionRange As Range = Me.ResolveOptionalRefEditRange(Me.RefEdit_Directions)
            Dim scaleMaximumRange As Range = Me.ResolveOptionalRefEditRange(Me.RefEdit_ScaleMaximums)
            Dim majorIntervalRange As Range = Me.ResolveOptionalRefEditRange(Me.RefEdit_MajorIntervals)

            Dim inputRanges As New List(Of Range) From {
                measureRange,
                actualRange,
                targetRange,
                qualitativeRange
            }
            If subtitleRange IsNot Nothing Then inputRanges.Add(subtitleRange)
            If directionRange IsNot Nothing Then inputRanges.Add(directionRange)
            If scaleMaximumRange IsNot Nothing Then inputRanges.Add(scaleMaximumRange)
            If majorIntervalRange IsNot Nothing Then inputRanges.Add(majorIntervalRange)

            Dim lastRow As Integer = GetCommonInputLastRow(inputRanges)

            Dim measureLabels As Object() = ReadRangeVector(BuildBoundedRange(measureRange, lastRow))
            Dim actualValues As Object() = ReadRangeVector(BuildBoundedRange(actualRange, lastRow))
            Dim targetValues As Object() = ReadRangeVector(BuildBoundedRange(targetRange, lastRow))
            Dim qualitativeRanges As Object(,) = ReadRangeMatrix(BuildBoundedRange(qualitativeRange, lastRow))

            Dim subtitles As Object() = If(subtitleRange Is Nothing,
                                           Nothing,
                                           ReadRangeVector(BuildBoundedRange(subtitleRange, lastRow)))
            Dim directions As Object() = If(directionRange Is Nothing,
                                            Nothing,
                                            ReadRangeVector(BuildBoundedRange(directionRange, lastRow)))
            Dim scaleMaximums As Object() = If(scaleMaximumRange Is Nothing,
                                               Nothing,
                                               ReadRangeVector(BuildBoundedRange(scaleMaximumRange, lastRow)))
            Dim majorIntervals As Object() = If(majorIntervalRange Is Nothing,
                                                Nothing,
                                                ReadRangeVector(BuildBoundedRange(majorIntervalRange, lastRow)))

            'Accept the common Excel workflow where users select the column headings together
            'with the data.  The detection deliberately ignores the text measure-label column.
            If HasLeadingHeader(actualValues, targetValues, qualitativeRanges) Then
                measureLabels = DropFirstVectorItem(measureLabels)
                actualValues = DropFirstVectorItem(actualValues)
                targetValues = DropFirstVectorItem(targetValues)
                qualitativeRanges = DropFirstMatrixRow(qualitativeRanges)
                subtitles = DropFirstVectorItem(subtitles)
                directions = DropFirstVectorItem(directions)
                scaleMaximums = DropFirstVectorItem(scaleMaximums)
                majorIntervals = DropFirstVectorItem(majorIntervals)
            End If

            Dim options As BulletChartOptions = Me.GetPlotOptions()
            Dim result As BulletChartResult = BulletChart.Compute(measureLabels,
                                                                  actualValues,
                                                                  targetValues,
                                                                  qualitativeRanges,
                                                                  subtitles,
                                                                  scaleMaximums,
                                                                  majorIntervals,
                                                                  directions,
                                                                  options)

            Dim appearance As BulletChartAppearance = Me.GetPlotAppearance()
            Dim outputWorksheet As Worksheet = Nothing
            Dim chartAnchor As Range = Nothing
            Me.ResolveOutputTarget(inputWorkbook, outputWorksheet, chartAnchor)

            DirectCast(outputWorksheet.Parent, Workbook).Activate()
            outputWorksheet.Activate()

            BulletChartExcel.AddChart(outputWorksheet,
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
                                            "Unable to create the bullet chart")
        Finally
            Me.btCompute.Enabled = True
            Me.Cursor = previousCursor
        End Try
    End Sub

    Private Function ResolveOptionalRefEditRange(refEdit As Excel2007RefEdit) As Range
        If refEdit Is Nothing OrElse String.IsNullOrWhiteSpace(refEdit.Address) Then Return Nothing
        Return Me.ResolveRefEditRange(refEdit)
    End Function

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

    Private Sub btnActualBarColor_Click(sender As Object, e As System.EventArgs) Handles btnActualBarColor.Click
        SelectColor(Me.pnlActualBarColor)
    End Sub

    Private Sub btnTargetColor_Click(sender As Object, e As System.EventArgs) Handles btnTargetColor.Click
        SelectColor(Me.pnlTargetColor)
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
