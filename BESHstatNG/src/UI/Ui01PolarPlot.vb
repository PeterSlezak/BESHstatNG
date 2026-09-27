Imports System
Imports System.Collections.Generic
Imports BESHStatNG.AppInfrastructure
Imports Microsoft.Office.Interop.Excel

Public Class Ui01PolarPlot
    Private Const DefaultChartSizePoints As Double = 420.0R

    Sub New(tagn As Integer)

        ' This call is required by the designer.
        InitializeComponent()
        Me.Tag = tagn

        ' Add any initialization after the InitializeComponent() call.
        Me.RefEdit_Angle.ExcelConnector = AppGlobals.app
        Me.RefEdit_Radius.ExcelConnector = AppGlobals.app
        Me.RefEdit_GroupID.ExcelConnector = AppGlobals.app
        Me.RefEditOutput.ExcelConnector = AppGlobals.app
        Me.UpdateOutputRangeState()
        Me.WireHelp(Me.btnHelp)
    End Sub

    Private Sub optOutputRange_CheckedChanged(sender As Object, e As System.EventArgs) Handles optOutputRange.CheckedChanged
        Me.UpdateOutputRangeState()
    End Sub

    Private Sub optWorksheet_CheckedChanged(sender As Object, e As System.EventArgs) Handles optWorksheet.CheckedChanged
        Me.UpdateOutputRangeState()
    End Sub

    Private Sub optWorkbook_CheckedChanged(sender As Object, e As System.EventArgs) Handles optWorkbook.CheckedChanged
        Me.UpdateOutputRangeState()
    End Sub

    ''' <summary>
    ''' Enables the output RefEdit only when the user chooses an explicit output range.
    ''' </summary>
    Private Sub UpdateOutputRangeState()
        Me.RefEditOutput.Enabled = Me.optOutputRange.Checked
        If Me.optOutputRange.Checked AndAlso Me.RefEditOutput.CanFocus Then
            Me.RefEditOutput.txtAddress.Select()
        End If
    End Sub

    ''' <summary>
    ''' Validates that radius, angle, and the optional group are single, row-aligned columns.
    ''' </summary>
    ''' <returns><see langword="True"/> when validation failed; otherwise <see langword="False"/>.</returns>
    Private Function CheckInputs() As Boolean
        Dim invalid As Boolean = False

        'The shared range helpers resolve unqualified sheet names through Excel's
        'active workbook, so return to the workbook captured by the RefEdit first.
        If Me.RefEdit_Radius.ExcelWorkBook IsNot Nothing Then
            Me.RefEdit_Radius.ExcelWorkBook.Activate()
        End If

        If CheckRefEdit(Me.RefEdit_Radius.Address, True) Then
            RefEditReset(Me.RefEdit_Radius)
            invalid = True
        End If

        If CheckRefEdit(Me.RefEdit_Angle.Address, True) Then
            RefEditReset(Me.RefEdit_Angle)
            invalid = True
        End If

        Dim hasGrouping As Boolean = Not String.IsNullOrWhiteSpace(Me.RefEdit_GroupID.Address)
        If hasGrouping AndAlso CheckRefEdit(Me.RefEdit_GroupID.Address, True) Then
            RefEditReset(Me.RefEdit_GroupID)
            invalid = True
        End If

        If invalid Then Return True

        Try
            Dim radiusWorksheet As Worksheet = WorksheetFromRefAdress(Me.RefEdit_Radius.Address,
                                                                      Me.RefEdit_Radius.ExcelWorkBook)
            Dim angleWorksheet As Worksheet = WorksheetFromRefAdress(Me.RefEdit_Angle.Address,
                                                                     Me.RefEdit_Angle.ExcelWorkBook)
            Dim groupWorksheet As Worksheet = Nothing
            If hasGrouping Then
                groupWorksheet = WorksheetFromRefAdress(Me.RefEdit_GroupID.Address,
                                                        Me.RefEdit_GroupID.ExcelWorkBook)
            End If
            Dim radiusWorkbook As Workbook = DirectCast(radiusWorksheet.Parent, Workbook)
            Dim angleWorkbook As Workbook = DirectCast(angleWorksheet.Parent, Workbook)

            If Not String.Equals(radiusWorkbook.FullName,
                                 angleWorkbook.FullName,
                                 StringComparison.OrdinalIgnoreCase) OrElse
               Not String.Equals(radiusWorksheet.Name,
                                 angleWorksheet.Name,
                                 StringComparison.OrdinalIgnoreCase) Then
                MsgBox("Radius and angle ranges must be on the same worksheet.",
                       vbExclamation,
                       AppGlobals.gsAPP_TITLE)
                Return True
            End If

            If hasGrouping Then
                Dim groupWorkbook As Workbook = DirectCast(groupWorksheet.Parent, Workbook)
                If Not String.Equals(radiusWorkbook.FullName,
                                     groupWorkbook.FullName,
                                     StringComparison.OrdinalIgnoreCase) OrElse
                   Not String.Equals(radiusWorksheet.Name,
                                     groupWorksheet.Name,
                                     StringComparison.OrdinalIgnoreCase) Then
                    MsgBox("Radius, angle, and grouping ranges must be on the same worksheet.",
                           vbExclamation,
                           AppGlobals.gsAPP_TITLE)
                    Return True
                End If
            End If

            Dim radiusRange As Range = radiusWorksheet.Range(Me.RefEdit_Radius.Address)
            Dim angleRange As Range = angleWorksheet.Range(Me.RefEdit_Angle.Address)
            Dim groupRange As Range = Nothing
            If hasGrouping Then groupRange = groupWorksheet.Range(Me.RefEdit_GroupID.Address)

            If radiusRange.Areas.Count <> 1 OrElse
               angleRange.Areas.Count <> 1 OrElse
               (hasGrouping AndAlso groupRange.Areas.Count <> 1) Then
                MsgBox("Each polar-plot input must be one continuous column range.",
                       vbExclamation,
                       AppGlobals.gsAPP_TITLE)
                Return True
            End If

            If radiusRange.Columns.Count <> 1 OrElse
               angleRange.Columns.Count <> 1 OrElse
               (hasGrouping AndAlso groupRange.Columns.Count <> 1) Then
                MsgBox("Each polar-plot input must contain exactly one column.",
                       vbExclamation,
                       AppGlobals.gsAPP_TITLE)
                Return True
            End If

            If hasGrouping AndAlso
               (groupRange.Row <> radiusRange.Row OrElse
                groupRange.Rows.Count <> radiusRange.Rows.Count) Then
                MsgBox("The grouping range must start on the same row and contain the same number of rows as radius and angle.",
                       vbExclamation,
                       AppGlobals.gsAPP_TITLE)
                Return True
            End If

            If radiusRange.Row <> angleRange.Row OrElse
               radiusRange.Rows.Count <> angleRange.Rows.Count Then
                MsgBox("Radius and angle ranges must start on the same row and contain the same number of rows.",
                       vbExclamation,
                       AppGlobals.gsAPP_TITLE)
                Return True
            End If

            If Not CheckNumeric(Me.tbAngularTickInterval) Then Return True
            If Not CheckNumeric(Me.tbRadialTickInterval) Then Return True

            If Me.optOutputRange.Checked Then
                Dim outputRange As Range = Nothing
                If Not Me.TryResolveRange(Me.RefEditOutput,
                                          "Output range",
                                          requireSingleColumn:=False,
                                          requireSingleArea:=True,
                                          resolvedRange:=outputRange) Then
                    Return True
                End If
            ElseIf Not Me.optWorksheet.Checked AndAlso Not Me.optWorkbook.Checked Then
                MsgBox("Select an output destination for the polar plot.",
                       vbExclamation,
                       AppGlobals.gsAPP_TITLE)
                Return True
            End If

            Return False
        Catch ex As Exception
            CoreServices.Errors.LogAndThrow(ex,
                                            False,
                                            True,
                                            "Unable to validate the polar-plot input ranges")
            Return True
        End Try
    End Function

    ''' <summary>
    ''' Resolves a RefEdit against the workbook captured when the range was selected.
    ''' </summary>
    ''' <param name="refEdit">RefEdit containing the Excel address.</param>
    ''' <param name="displayName">User-facing name used in validation messages.</param>
    ''' <param name="requireSingleColumn">Whether the selected range must contain exactly one column.</param>
    ''' <param name="requireSingleArea">Whether the selected range must be continuous.</param>
    ''' <param name="resolvedRange">Receives the resolved Excel range.</param>
    ''' <returns><see langword="True"/> when the range is valid; otherwise <see langword="False"/>.</returns>
    Private Function TryResolveRange(refEdit As Excel2007RefEdit,
                                     displayName As String,
                                     requireSingleColumn As Boolean,
                                     requireSingleArea As Boolean,
                                     ByRef resolvedRange As Range) As Boolean
        resolvedRange = Nothing

        If refEdit Is Nothing OrElse String.IsNullOrWhiteSpace(refEdit.Address) Then
            MsgBox(displayName & " is empty. Please select a range.",
                   vbExclamation,
                   AppGlobals.gsAPP_TITLE)
            Return False
        End If

        Try
            Dim workbook As Workbook = refEdit.ExcelWorkBook
            If workbook Is Nothing Then workbook = AppGlobals.app.ActiveWorkbook
            If workbook Is Nothing Then Throw New InvalidOperationException("No active Excel workbook is available.")

            Dim worksheet As Worksheet = WorksheetFromRefAdress(refEdit.Address, workbook)
            If worksheet Is Nothing Then Throw New InvalidOperationException("Unable to resolve the selected worksheet.")

            resolvedRange = worksheet.Range(refEdit.Address)

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

    ''' <summary>
    ''' Imports the paired ranges and optional grouping range together so worksheet-row alignment is retained.
    ''' </summary>
    ''' <param name="errorText">Receives a user-facing validation message when no usable data were imported.</param>
    ''' <returns>The imported two- or three-column data object, or <see langword="Nothing"/> on validation failure.</returns>
    Private Function GetData(ByRef errorText As String) As DataObj
        Dim inputWorkbook As Workbook = Me.RefEdit_Radius.ExcelWorkBook
        If inputWorkbook IsNot Nothing Then inputWorkbook.Activate()

        Dim radiusReference As String = prepareRef2D(Me.RefEdit_Radius.Address, inputWorkbook)
        Dim angleReference As String = prepareRef2D(Me.RefEdit_Angle.Address,
                                                    Me.RefEdit_Angle.ExcelWorkBook)
        angleReference = RemoveWorksheetQualifier(angleReference)

        Dim hasGrouping As Boolean = Not String.IsNullOrWhiteSpace(Me.RefEdit_GroupID.Address)
        Dim combinedReference As String
        Dim characterColumns As Integer
        Dim expectedColumnCount As Integer

        If hasGrouping Then
            'Place the group first because DataObj accepts character data only in
            'leading columns; numeric group IDs are accepted and normalized to text.
            Dim groupReference As String = prepareRef2D(Me.RefEdit_GroupID.Address,
                                                        Me.RefEdit_GroupID.ExcelWorkBook)
            combinedReference = groupReference & ", " &
                                RemoveWorksheetQualifier(radiusReference) & ", " &
                                angleReference
            characterColumns = 0
            expectedColumnCount = 3
        Else
            combinedReference = radiusReference & ", " & angleReference
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
            errorText = "No numeric radius-angle observations were found in the selected ranges."
            Return Nothing
        End If

        If columnData.nCols <> expectedColumnCount Then
            errorText = If(hasGrouping,
                           "Radius and angle must contain numeric data, and the selected grouping range must contain at least one text or numeric group ID.",
                           "Both radius and angle inputs must contain at least one numeric value.")
            Return Nothing
        End If

        Return columnData
    End Function

    ''' <summary>
    ''' Removes the worksheet qualifier from a prepared range reference so it can
    ''' be appended to another range on the already-qualified worksheet.
    ''' </summary>
    Private Shared Function RemoveWorksheetQualifier(reference As String) As String
        If String.IsNullOrWhiteSpace(reference) Then Return reference
        Dim worksheetDelimiter As Integer = reference.LastIndexOf("!"c)
        If worksheetDelimiter < 0 Then Return reference
        Return reference.Substring(worksheetDelimiter + 1)
    End Function

    ''' <summary>
    ''' Creates the backend options represented by the current radio buttons and check box.
    ''' </summary>
    ''' <returns>A complete set of polar-plot options.</returns>
    Private Function GetPlotOptions() As PolarPlotOptions
        Dim options As New PolarPlotOptions()

        If Me.optRadians.Checked Then
            options.AngleUnit = PolarAngleUnit.Radians
        ElseIf Me.optPercentage.Checked Then
            options.AngleUnit = PolarAngleUnit.Percentage
        ElseIf Me.optDegrees.Checked Then
            options.AngleUnit = PolarAngleUnit.Degrees
        Else
            Throw New InvalidOperationException("Select an angle unit for the polar plot.")
        End If

        If Me.optClockwise.Checked Then
            options.Rotation = PolarRotation.Clockwise
        ElseIf Me.optCounterClockwise.Checked Then
            options.Rotation = PolarRotation.Counterclockwise
        Else
            Throw New InvalidOperationException("Select a rotation direction for the polar plot.")
        End If

        If Me.optNorth.Checked Then
            options.ZeroAngle = PolarZeroAngle.North
        ElseIf Me.optSouth.Checked Then
            options.ZeroAngle = PolarZeroAngle.South
        ElseIf Me.optWest.Checked Then
            options.ZeroAngle = PolarZeroAngle.West
        ElseIf Me.optEast.Checked Then
            options.ZeroAngle = PolarZeroAngle.East
        Else
            Throw New InvalidOperationException("Select a zero-angle position for the polar plot.")
        End If

        options.ConnectPoints = Me.ckConnectPoints.Checked

        If Me.tbAngularTickInterval.Text <> String.Empty And CheckNumeric(Me.tbAngularTickInterval) Then options.AngularTickInterval = ParseUiDouble(Me.tbAngularTickInterval.Text, "Angular Tick Interval")
        If Me.tbRadialTickInterval.Text <> String.Empty And CheckNumeric(Me.tbRadialTickInterval) Then options.RadialTickInterval = ParseUiDouble(Me.tbRadialTickInterval.Text, "Radial Tick Interval")

        Return options
    End Function


    ''' <summary>
    ''' Computes the polar geometry and creates a square embedded chart at the selected output destination.
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
            Dim radiusColumn As Integer = If(hasGrouping, 1, 0)
            Dim angleColumn As Integer = If(hasGrouping, 2, 1)

            Dim radiusValues As New List(Of Double)(cleanData.GetLength(0))
            Dim angleValues As New List(Of Double)(cleanData.GetLength(0))
            Dim groupValues As List(Of Object) = If(hasGrouping,
                                                    New List(Of Object)(cleanData.GetLength(0)),
                                                    Nothing)

            For rowIndex As Integer = 0 To cleanData.GetLength(0) - 1
                'DataObj intentionally drops completely blank rows. Reinsert one
                'NaN pair for every source-row discontinuity so all connected lines retain gaps.
                If rowIndex > 0 AndAlso
                   data.RowIds IsNot Nothing AndAlso
                   data.RowIds(rowIndex) > data.RowIds(rowIndex - 1) + 1 Then
                    radiusValues.Add(Double.NaN)
                    angleValues.Add(Double.NaN)
                    If hasGrouping Then groupValues.Add(Nothing)
                End If

                radiusValues.Add(ToDoubleOrNaN(cleanData(rowIndex, radiusColumn)))
                angleValues.Add(ToDoubleOrNaN(cleanData(rowIndex, angleColumn)))
                If hasGrouping Then groupValues.Add(cleanData(rowIndex, groupColumn))
            Next

            Dim plot As PolarPlot
            If hasGrouping Then
                plot = New PolarPlot(radiusValues.ToArray(),
                                     angleValues.ToArray(),
                                     Me.GetPlotOptions(),
                                     groupValues.ToArray())
            Else
                plot = New PolarPlot(radiusValues.ToArray(),
                                     angleValues.ToArray(),
                                     Me.GetPlotOptions())
            End If
            Dim result As PolarPlotResult = plot.Compute()

            Dim inputWorksheet As Worksheet = DirectCast(data.ws, Worksheet)
            Dim inputWorkbook As Workbook = DirectCast(inputWorksheet.Parent, Workbook)
            Dim outputWorksheet As Worksheet = Nothing
            Dim chartAnchor As Range = Nothing
            Me.ResolveOutputTarget(inputWorkbook, outputWorksheet, chartAnchor)

            DirectCast(outputWorksheet.Parent, Workbook).Activate()
            outputWorksheet.Activate()

            Dim seriesName As String = "Data"
            If data.varNames IsNot Nothing AndAlso
               data.varNames.Length > radiusColumn AndAlso
               Not String.IsNullOrWhiteSpace(data.varNames(radiusColumn)) Then
                seriesName = data.varNames(radiusColumn).Trim()
            End If

            Dim appearance As New PolarPlotAppearance With {
                .ChartTitle = "Polar plot",
                .SeriesName = seriesName,
                .ShowGroupLegend = True,
                .GroupStyleMode = PolarGroupStyleMode.ColorAndMarker
            }

            Dim createdChart As Chart = PolarPlotExcel.AddChart(outputWorksheet,
                                                                result,
                                                                appearance,
                                                                CDbl(chartAnchor.Left),
                                                                CDbl(chartAnchor.Top),
                                                                DefaultChartSizePoints,
                                                                DefaultChartSizePoints)
            'createdChart.Activate()
        Catch ex As Exception
            CoreServices.Errors.LogAndThrow(ex,
                                            False,
                                            True,
                                            "Unable to create the polar plot")
        End Try
    End Sub

    ''' <summary>
    ''' Resolves the selected standard graphical-output option to a worksheet and chart anchor.
    ''' </summary>
    ''' <param name="inputWorkbook">Workbook containing the polar-plot input data.</param>
    ''' <param name="outputWorksheet">Receives the worksheet on which the chart will be created.</param>
    ''' <param name="chartAnchor">Receives the cell whose upper-left corner anchors the chart.</param>
    Private Sub ResolveOutputTarget(inputWorkbook As Workbook,
                                    ByRef outputWorksheet As Worksheet,
                                    ByRef chartAnchor As Range)
        outputWorksheet = Nothing
        chartAnchor = Nothing

        If Me.optWorkbook.Checked Then
            Dim outputWorkbook As Workbook = AppGlobals.app.Workbooks.Add()
            outputWorksheet = DirectCast(outputWorkbook.Worksheets(1), Worksheet)
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
            If outputWorksheet Is Nothing Then
                Throw New InvalidOperationException("Unable to resolve the output worksheet.")
            End If
            chartAnchor = DirectCast(selectedRange.Cells(1, 1), Range)
            Return
        End If

        Throw New InvalidOperationException("Select an output destination for the polar plot.")
    End Sub

    ''' <summary>
    ''' Converts one imported numeric cell to <see cref="Double"/> while preserving
    ''' permitted missing cells as <see cref="Double.NaN"/>.
    ''' </summary>
    Private Shared Function ToDoubleOrNaN(value As Object) As Double
        If value Is Nothing OrElse value Is DBNull.Value Then Return Double.NaN
        Return CDbl(value)
    End Function
End Class
