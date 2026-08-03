Imports System
Imports System.Collections.Generic
Imports BESHStatNG.AppInfrastructure
Imports Microsoft.Office.Interop.Excel

Public Class Ui01PolarPlot
    Private Const DefaultChartSizePoints As Double = 420.0R
    Private Const ChartColumnGap As Integer = 2

    Sub New(tagn As Integer)

        ' This call is required by the designer.
        InitializeComponent()
        Me.Tag = tagn

        ' Add any initialization after the InitializeComponent() call.
        Me.RefEdit_Angle.ExcelConnector = AppGlobals.app
        Me.RefEdit_Radius.ExcelConnector = AppGlobals.app
        Me.RefEdit_GroupID.ExcelConnector = AppGlobals.app
        Me.WireHelp(Me.btnHelp)
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
    ''' Computes the polar geometry and creates a square embedded chart beside the input columns.
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
            DirectCast(inputWorksheet.Parent, Workbook).Activate()
            inputWorksheet.Activate()

            Dim radiusRange As Range = inputWorksheet.Range(Me.RefEdit_Radius.Address)
            Dim angleRange As Range = inputWorksheet.Range(Me.RefEdit_Angle.Address)
            Dim lastInputColumn As Integer = Math.Max(radiusRange.Column + radiusRange.Columns.Count - 1,
                                                      angleRange.Column + angleRange.Columns.Count - 1)
            If hasGrouping Then
                Dim groupRange As Range = inputWorksheet.Range(Me.RefEdit_GroupID.Address)
                lastInputColumn = Math.Max(lastInputColumn,
                                           groupRange.Column + groupRange.Columns.Count - 1)
            End If
            Dim maximumColumn As Integer = inputWorksheet.Columns.Count
            Dim anchorColumn As Integer = lastInputColumn + ChartColumnGap
            If anchorColumn > maximumColumn Then anchorColumn = 1

            Dim anchorRow As Integer = Math.Min(radiusRange.Row, angleRange.Row)
            If hasGrouping Then
                Dim groupRange As Range = inputWorksheet.Range(Me.RefEdit_GroupID.Address)
                anchorRow = Math.Min(anchorRow, groupRange.Row)
            End If
            Dim chartAnchor As Range = DirectCast(inputWorksheet.Cells(anchorRow, anchorColumn), Range)

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

            Dim createdChart As Chart = PolarPlotExcel.AddChart(inputWorksheet,
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
    ''' Converts one imported numeric cell to <see cref="Double"/> while preserving
    ''' permitted missing cells as <see cref="Double.NaN"/>.
    ''' </summary>
    Private Shared Function ToDoubleOrNaN(value As Object) As Double
        If value Is Nothing OrElse value Is DBNull.Value Then Return Double.NaN
        Return CDbl(value)
    End Function
End Class
