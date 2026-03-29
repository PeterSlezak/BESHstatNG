Imports System.IO
Imports System.Runtime.InteropServices.ComTypes
Imports System.Windows
Imports System.Windows.Forms
Imports BESHStatNG.AppInfrastructure
Imports BESHStatNG.GifAnimator
Imports Microsoft.Office.Interop.Excel

Public Class Ui3XYZplot

    Private pFigure As Chart = Nothing
    Private pbPreventEvents As Boolean = False
    'Animated GIF workflow 
    Private pAnimatedGifFinalPath As String = String.Empty
    Private pAnimatedGifWorkDir As String = String.Empty

    Sub New()

        ' This call is required by the designer.
        InitializeComponent()

        ' Add any initialization after the InitializeComponent() call.
        Me.RefEdit1_X.ExcelConnector = AppGlobals.app
        Me.RefEdit2_Y.ExcelConnector = AppGlobals.app
        Me.RefEdit3_Z.ExcelConnector = AppGlobals.app
        Me.RefEdit4_Group.ExcelConnector = AppGlobals.app
        Me.RefEdit5_Labels.ExcelConnector = AppGlobals.app
        Me.RefEdit6_AnimatedGif.ExcelConnector = AppGlobals.app
        Me.RefEdit1_3Dobjects.ExcelConnector = AppGlobals.app

        With Me.cbPointLabelPosition.Items
            .Add("Right")
            .Add("Left")
            .Add("Above")
            .Add("Below")
        End With
        Me.cbPointLabelPosition.SelectedIndex = 0

        With Me.cbMarkerSymbol.Items
            .Add("Circle")
            .Add("Square")
            .Add("Diamond")
            .Add("Triangle")
            .Add("X")
            .Add("Plus")
            .Add("Star")
        End With
        Me.cbMarkerSymbol.SelectedIndex = 0

        Me.RefEdit1_X.txtAddress.Select()
        Me.WireHelp(Me.btnHelp)
    End Sub

    Private Function checkInputs() As Boolean
        Dim bOut As Boolean
        'check input data------------------------------
        If CheckRefEdit(Me.RefEdit1_X.Address, True) Then
            RefEditReset(Me.RefEdit1_X)
            bOut = True
        End If

        If CheckRefEdit(Me.RefEdit2_Y.Address, True) Then
            RefEditReset(Me.RefEdit2_Y)
            bOut = True
        End If

        If CheckRefEdit(Me.RefEdit3_Z.Address, True) Then
            RefEditReset(Me.RefEdit3_Z)
            bOut = True
        End If

        'these are optional
        If Me.RefEdit4_Group.Address <> String.Empty Then
            If CheckRefEdit(Me.RefEdit4_Group.Address, True) Then
                RefEditReset(Me.RefEdit4_Group)
                bOut = True
            End If
        End If

        If Me.RefEdit5_Labels.Address <> String.Empty Then
            If CheckRefEdit(Me.RefEdit5_Labels.Address) Then
                RefEditReset(Me.RefEdit5_Labels)
                bOut = True
            End If
        End If

        If Me.RefEdit1_3Dobjects.Address <> String.Empty Then
            If CheckRefEdit(Me.RefEdit1_3Dobjects.Address) Then
                RefEditReset(Me.RefEdit1_3Dobjects)
                bOut = True
            End If
        End If

        'Axis scaling numbers check if numeric (if provided)
        If Not CheckNumeric(Me.tbXmax) Then bOut = True
        If Not CheckNumeric(Me.tbXmin) Then bOut = True
        If Not CheckNumeric(Me.tbYmax) Then bOut = True
        If Not CheckNumeric(Me.tbYmin) Then bOut = True
        If Not CheckNumeric(Me.tbZmax) Then bOut = True
        If Not CheckNumeric(Me.tbZmin) Then bOut = True

        Return bOut
    End Function

    Private Function ValidateAxisLimitPair(axisName As String,
                                           tbMin As System.Windows.Forms.TextBox,
                                           tbMax As System.Windows.Forms.TextBox,
                                           ByRef errText As String) As Boolean
        errText = ""

        Dim hasMin As Boolean = tbMin.Text <> String.Empty
        Dim hasMax As Boolean = tbMax.Text <> String.Empty

        If hasMin Xor hasMax Then
            errText = $"{axisName}: both minimum and maximum must be provided."
            Return False
        End If

        If Not hasMin AndAlso Not hasMax Then
            Return True
        End If

        Dim minVal As Double, maxVal As Double, sErr As String = ""

        If Not CheckNumeric(tbMin, minVal, sErr) Then
            errText = $"{axisName} minimum: {sErr}"
            Return False
        End If

        If Not CheckNumeric(tbMax, maxVal, sErr) Then
            errText = $"{axisName} maximum: {sErr}"
            Return False
        End If

        If Not graphics.XYZscatter.ValidateAxisMinMax(axisName, minVal, maxVal, errText) Then
            Return False
        End If

        Return True
    End Function

    Private Function TryApplyManualAxisLimits(plot As graphics.XYZscatter,
                                          ByRef errText As String) As Boolean
        errText = ""

        If Not ValidateAxisLimitPair("X axis", Me.tbXmin, Me.tbXmax, errText) Then Return False
        If Not ValidateAxisLimitPair("Y axis", Me.tbYmin, Me.tbYmax, errText) Then Return False
        If Not ValidateAxisLimitPair("Z axis", Me.tbZmin, Me.tbZmax, errText) Then Return False

        If Me.tbXmin.Text <> String.Empty Then
            plot.SetAxisLimitsX(ParseUiDouble(Me.tbXmin.Text, "X axis minimum"),
                                ParseUiDouble(Me.tbXmax.Text, "X axis maximum"))
        End If
        If Me.tbYmin.Text <> String.Empty Then
            plot.SetAxisLimitsY(ParseUiDouble(Me.tbYmin.Text, "Y axis minimum"),
                                ParseUiDouble(Me.tbYmax.Text, "Y axis maximum"))
        End If
        If Me.tbZmin.Text <> String.Empty Then
            plot.SetAxisLimitsZ(ParseUiDouble(Me.tbZmin.Text, "Z axis minimum"),
                                ParseUiDouble(Me.tbZmax.Text, "Z axis maximum"))
        End If

        Return True
    End Function

    Private Function get3DobjectsData() As DataObj
        If Me.RefEdit1_3Dobjects.Address <> String.Empty Then
            Dim IdData = New DataObj
            Dim ref As String
            Dim CharCols As Integer = 0

            Dim wks As String = WorksheetNameFromRefAdress(Me.RefEdit1_3Dobjects.Address, True)
            ref = prepareRef2D(Me.RefEdit1_3Dobjects.Address)
            IdData.bAllowMissing = True
            IdData.DataInport(ref, True, 1)

            Return IdData
        Else
            Return Nothing
        End If
    End Function

    Private Function Create3DObjectsList(data3Dobj As DataObj, ByRef errText As String) As List(Of graphics.IXYZDrawable3D)
        errText = String.Empty
        Dim out As New List(Of graphics.IXYZDrawable3D)

        If data3Dobj Is Nothing OrElse data3Dobj.FinalData Is Nothing Then
            Return out
        End If

        Dim d = data3Dobj.FinalData
        Dim nRows As Integer = UBound(d, 1)
        Dim nCols As Integer = UBound(d, 2) + 1

        If nCols < 5 Then
            errText = "3D objects table must have at least 5 columns: [type, X, Y, Z, diameter]. For ellipsoid also provide [diameterX, diameterY, diameterZ]."
            Return Nothing
        End If

        For i As Integer = 0 To nRows
            Dim rowNum As Integer = i + 1 '1-based for user-friendly messages

            'Object type (col 1)
            Dim objType As String = ""
            If d(i, 0) IsNot Nothing Then objType = CStr(d(i, 0)).Trim().ToLowerInvariant()

            'Convenience aliases
            Select Case objType
                Case "s", "sphere", "wire_sphere", "wiresphere"
                    objType = "sphere"
                Case "e", "ellipsoid", "wire_ellipsoid", "wireellipsoid"
                    objType = "ellipsoid"
            End Select

            If objType = "" Then
                errText = $"3D objects row {rowNum}: object type (column 1) is empty. Allowed: sphere, ellipsoid."
                Return Nothing
            End If

            'Common numeric checks (cols 2..4)
            Dim x As Double, y As Double, z As Double
            Try
                x = CDbl(d(i, 1))
                y = CDbl(d(i, 2))
                z = CDbl(d(i, 3))
            Catch
                errText = $"3D objects row {rowNum}: X, Y, Z must be numeric."
                Return Nothing
            End Try

            If Double.IsNaN(x) OrElse Double.IsInfinity(x) OrElse
               Double.IsNaN(y) OrElse Double.IsInfinity(y) OrElse
               Double.IsNaN(z) OrElse Double.IsInfinity(z) Then
                errText = $"3D objects row {rowNum}: X, Y, Z must be finite numbers."
                Return Nothing
            End If

            If objType = "sphere" Then
                'Sphere needs at least 5 columns (diameter in col 5)
                Dim diam As Double
                Try
                    diam = CDbl(d(i, 4))
                Catch
                    errText = $"3D objects row {rowNum} (sphere): diameter (column 5) must be numeric."
                    Return Nothing
                End Try

                If Double.IsNaN(diam) OrElse Double.IsInfinity(diam) Then
                    errText = $"3D objects row {rowNum} (sphere): diameter must be a finite number."
                    Return Nothing
                End If

                If diam < 0 Then
                    errText = $"3D objects row {rowNum} (sphere): diameter must be >= 0."
                    Return Nothing
                End If

                'Skip zero-size objects (no error)
                If diam = 0 Then Continue For

                out.Add(New graphics.WireSphere3D(cx:=x, cy:=y, cz:=z, diameter:=diam) With {
                            .LatitudeRings = Me.spinBtnLatitudeRings.Value,
                            .LongitudeRings = Me.spinBtnLongitudeRings.Value,
                            .PointsPerRing = Me.spinBtnPointsPerRing.Value,
                            .ColorR = Me.spinBtnR.Value, .ColorG = Me.spinBtnG.Value, .ColorB = Me.spinBtnB.Value
                            })

            ElseIf objType = "ellipsoid" Then
                'Ellipsoid needs 7 columns (diameterX/Y/Z in cols 5..7)
                If nCols < 7 Then
                    errText = "3D objects table contains an 'ellipsoid' row but has fewer than 7 columns. Required: [type, X, Y, Z, diameterX, diameterY, diameterZ]."
                    Return Nothing
                End If

                Dim dx As Double, dy As Double, dz As Double
                Try
                    dx = CDbl(d(i, 4))
                    dy = CDbl(d(i, 5))
                    dz = CDbl(d(i, 6))
                Catch
                    errText = $"3D objects row {rowNum} (ellipsoid): diameterX, diameterY, diameterZ (columns 5–7) must be numeric."
                    Return Nothing
                End Try

                If Double.IsNaN(dx) OrElse Double.IsInfinity(dx) OrElse
               Double.IsNaN(dy) OrElse Double.IsInfinity(dy) OrElse
               Double.IsNaN(dz) OrElse Double.IsInfinity(dz) Then
                    errText = $"3D objects row {rowNum} (ellipsoid): diameters must be finite numbers."
                    Return Nothing
                End If

                If dx < 0 OrElse dy < 0 OrElse dz < 0 Then
                    errText = $"3D objects row {rowNum} (ellipsoid): diameterX, diameterY, diameterZ must be >= 0."
                    Return Nothing
                End If

                'Skip degenerate ellipsoids (no error)
                If dx = 0 OrElse dy = 0 OrElse dz = 0 Then Continue For

                out.Add(New graphics.WireEllipsoid3D(cx:=x, cy:=y, cz:=z,
                                                     diameterX:=dx, diameterY:=dy, diameterZ:=dz) With {
                            .LatitudeRings = Me.spinBtnLatitudeRings.Value,
                            .LongitudeRings = Me.spinBtnLongitudeRings.Value,
                            .PointsPerRing = Me.spinBtnPointsPerRing.Value,
                            .ColorR = Me.spinBtnR.Value, .ColorG = Me.spinBtnG.Value, .ColorB = Me.spinBtnB.Value
                            })
            Else
                errText = $"3D objects row {rowNum}: unknown type '{objType}'. Allowed: sphere, ellipsoid."
                Return Nothing
            End If
        Next

        Return out
    End Function

    Private Function getData(ByRef strErr As String) As DataObj
        Dim out = New MultiGroupsUnpairedData
        Dim byIdData = New DataObj
        Dim refGroup As String = String.Empty, refLabels As String = String.Empty
        Dim CharCols As Integer = 0

        Dim wks As String = WorksheetNameFromRefAdress(Me.RefEdit1_X.Address, True)
        If wks <> WorksheetNameFromRefAdress(Me.RefEdit2_Y.Address, True) Then
            strErr = "Input reference range adresses are from different sheets. Input can be only from one sheet."
        End If
        If wks <> WorksheetNameFromRefAdress(Me.RefEdit3_Z.Address, True) Then
            strErr = "Input reference range adresses are from different sheets. Input can be only from one sheet."
        End If

        Dim refX As String = prepareRef2D(Me.RefEdit1_X.Address)
        Dim refY As String = prepareRef2D(Me.RefEdit2_Y.Address)
        Dim refZ As String = prepareRef2D(Me.RefEdit3_Z.Address)

        If Me.RefEdit4_Group.Address <> String.Empty Then
            If wks <> WorksheetNameFromRefAdress(Me.RefEdit4_Group.Address, True) Then
                strErr = "Input reference range adresses are from different sheets. Input can be only from one sheet."
            End If
            CharCols += 1
            refGroup = prepareRef2D(Me.RefEdit4_Group.Address) 'reuse refData
        End If

        If Me.RefEdit5_Labels.Address <> String.Empty Then
            If wks <> WorksheetNameFromRefAdress(Me.RefEdit5_Labels.Address, True) Then
                strErr = "Input reference range adresses are from different sheets. Input can be only from one sheet."
            End If
            CharCols += 1
            refLabels = prepareRef2D(Me.RefEdit5_Labels.Address) 'reuse refData
        End If

        Dim refFinal As String = refGroup
        refFinal = If(refFinal = String.Empty, refLabels, refFinal & ", " & Replace(refLabels, wks & "!", String.Empty))

        If Trim$(refFinal) = String.Empty Then 'no group, no label. We need to make sure the refFinal starts with the sheet name
            refFinal = refX & ", " &
                       Replace(refY, wks & "!", String.Empty) & ", " &  'Remove "Sheet1!" from string
                       Replace(refZ, wks & "!", String.Empty)
        Else
            refFinal = refFinal & ", " & Replace(refX, wks & "!", String.Empty) & ", " &
                       Replace(refY, wks & "!", String.Empty) & ", " &
                       Replace(refZ, wks & "!", String.Empty) 'Remove "Sheet1!" from string
        End If

        byIdData.DataInport(refFinal, True, CharCols)

        Return byIdData

    End Function

    Private Sub Recalculate()
        Dim errText As String = String.Empty
        Dim data As DataObj
        Dim grpData() As String = Nothing, labelData() As String = Nothing
        Dim colId As Integer = 0

        'Get Data
        data = Me.getData(errText)
        If errText <> String.Empty Then
            MsgBox(errText, vbExclamation)
            Exit Sub
        End If

        Dim d = data.FinalData

        'check if we have groups data input
        If Me.RefEdit4_Group.Address <> String.Empty Then 'We have only One group
            grpData = Matrix.Array2strArray(Matrix.GetColumnFrom2Darray(d, 0))
            colId += 1
        End If
        'check if we have data lebels data input
        If Me.RefEdit5_Labels.Address <> String.Empty Then 'We have only One group
            labelData = Matrix.Array2strArray(Matrix.GetColumnFrom2Darray(d, colId))
            colId += 1
        End If

        Dim Xdata = Matrix.Array2dblArray(Matrix.GetColumnFrom2Darray(d, colId))
        Dim Ydata = Matrix.Array2dblArray(Matrix.GetColumnFrom2Darray(d, colId + 1))
        Dim Zdata = Matrix.Array2dblArray(Matrix.GetColumnFrom2Darray(d, colId + 2))
        Dim xlbl = data.varNames(colId)
        Dim ylbl = data.varNames(colId + 1)
        Dim zlbl = data.varNames(colId + 2)

        'get 3D object data
        Dim objects3D As New List(Of graphics.IXYZDrawable3D)
        If Me.RefEdit1_3Dobjects.Address <> String.Empty Then
            Dim data3Dobj = Me.get3DobjectsData()
            Dim err3D As String = ""
            objects3D = Create3DObjectsList(data3Dobj, err3D)
            If objects3D Is Nothing Then
                MsgBox(err3D, vbExclamation)
                Exit Sub
            End If
        End If

        If Me.pFigure Is Nothing Then
            AppGlobals.app.ActiveWorkbook.Charts.Add()
            Me.pFigure = AppGlobals.app.ActiveWorkbook.ActiveChart
        End If

        Dim XYZplot As New graphics.XYZscatter
        With XYZplot
            .dataInputs(Xdata, Ydata, Zdata)
            .axesLabelInputs(xlbl, ylbl, zlbl)
            .showPlanePointInputs(Me.ckXYplanePoints.Checked, Me.ckYZplanePoints.Checked, Me.ckXZplanePoints.Checked,
                                  Int(Me.spinBtnXYPlanePointSize.Value), Int(Me.spinBtnYZPlanePointSize.Value), Int(Me.spinBtnXZPlanePointSize.Value))
            .ScaleAxis(Me.ckScaleAxes.Checked)
            .rotationAndZoomInputs(CDbl(Me.spinBtnZoom.Value), CDbl(Me.spinBtnShiftY.Value), CDbl(Me.spinBtnShiftX.Value),
                                   CDbl(Me.spinBtnRotationX.Value), CDbl(Me.spinBtnRotationZ.Value))
            .settingsInputs(Me.ckDataPointLabels.Checked, Me.ckZdropLines.Checked, Me.ckGridlines.Checked, Int(Me.spinBtnLabelFontSize.Value),
                     GetLabelPositionFromCombobox(Me.cbPointLabelPosition.Text), CInt(Me.spinBtnMarkerSize.Value), GetMarkerStyleFromCombobox(Me.cbMarkerSymbol.Text))
            'axis scaling
            Dim axisErr As String = ""
            If Not TryApplyManualAxisLimits(XYZplot, axisErr) Then
                MsgBox(axisErr, vbExclamation)
                Exit Sub
            End If

            ' Add wire sphere in RAW coordinates (same units as your data)
            .ClearObjects()
            .SetObjects(objects3D)

            ' Reverse X and Y display direction
            .AxisDirectionInputs(flipX:=Me.ckXreverseAxis.Checked, flipY:=Me.ckYreverseAxis.Checked, flipZ:=Me.ckZreverseAxis.Checked)

            If Me.ckDataPointLabels.Checked Then .SetDataLabels(labelData)
            If grpData IsNot Nothing Then .SetGroups(grpData)

            .draw(Me.pFigure)
        End With

    End Sub

    Private Function GetLabelPositionFromCombobox(txt_pos As String) As Long
        If txt_pos = "Above" Then
            GetLabelPositionFromCombobox = XlDataLabelPosition.xlLabelPositionAbove
        ElseIf txt_pos = "Below" Then
            GetLabelPositionFromCombobox = XlDataLabelPosition.xlLabelPositionBelow
        ElseIf txt_pos = "Left" Then
            GetLabelPositionFromCombobox = XlDataLabelPosition.xlLabelPositionLeft
        ElseIf txt_pos = "Right" Then
            GetLabelPositionFromCombobox = XlDataLabelPosition.xlLabelPositionRight
        Else 'Return default value
            GetLabelPositionFromCombobox = XlDataLabelPosition.xlLabelPositionRight
        End If
    End Function

    Private Sub btOK_Click(sender As Object, e As System.EventArgs) Handles btOK.Click
        Try
            'Validate Inputs
            If Me.checkInputs() Then Exit Sub

            Recalculate()
        Catch ex As Exception
            AppGlobals.BSerr.LogAndThrow(ex, False, True)
        End Try
    End Sub

    '--------------------------------------------------------------------------
    ' View settings
    '--------------------------------------------------------------------------
    Private Sub spinBtnRotationX_ValueChanged(sender As Object, e As System.EventArgs) Handles spinBtnRotationX.ValueChanged
        If pFigure IsNot Nothing AndAlso Not Me.pbPreventEvents Then Call Recalculate()
    End Sub

    Private Sub spinBtnRotationZ_ValueChanged(sender As Object, e As System.EventArgs) Handles spinBtnRotationZ.ValueChanged
        If pFigure IsNot Nothing AndAlso Not Me.pbPreventEvents Then Call Recalculate()
    End Sub

    Private Sub spinBtnZoom_ValueChanged(sender As Object, e As System.EventArgs) Handles spinBtnZoom.ValueChanged
        If pFigure IsNot Nothing AndAlso Not Me.pbPreventEvents Then Call Recalculate()
    End Sub

    Private Sub spinBtnShiftX_ValueChanged(sender As Object, e As System.EventArgs) Handles spinBtnShiftX.ValueChanged
        If pFigure IsNot Nothing AndAlso Not Me.pbPreventEvents Then Call Recalculate()
    End Sub
    Private Sub spinBtnShiftY_ValueChanged(sender As Object, e As System.EventArgs) Handles spinBtnShiftY.ValueChanged
        If pFigure IsNot Nothing AndAlso Not Me.pbPreventEvents Then Call Recalculate()
    End Sub

    Private Sub btResetView_Click(sender As Object, e As System.EventArgs) Handles btResetView.Click
        Me.pbPreventEvents = True
        Me.spinBtnRotationX.Value = 120
        Me.spinBtnRotationZ.Value = 60
        Me.spinBtnZoom.Value = 0
        Me.spinBtnShiftY.Value = 50
        Me.spinBtnShiftX.Value = 50
        Me.pbPreventEvents = False
        If pFigure IsNot Nothing Then Call Recalculate()
    End Sub

    '--------------------------------------------------------------------------
    ' Chart settings
    '--------------------------------------------------------------------------
    Private Sub spinBtnXYPlanePointSize_ValueChanged(sender As Object, e As System.EventArgs) Handles spinBtnXYPlanePointSize.ValueChanged
        If pFigure IsNot Nothing And Me.ckXYplanePoints.Checked Then Call Recalculate()
    End Sub

    Private Sub spinBtnYZPlanePointSize_ValueChanged(sender As Object, e As System.EventArgs) Handles spinBtnYZPlanePointSize.ValueChanged
        If pFigure IsNot Nothing And Me.ckYZplanePoints.Checked Then Call Recalculate()
    End Sub

    Private Sub spinBtnXZPlanePointSize_ValueChanged(sender As Object, e As System.EventArgs) Handles spinBtnXZPlanePointSize.ValueChanged
        If pFigure IsNot Nothing And Me.ckXZplanePoints.Checked Then Call Recalculate()
    End Sub

    Private Sub spinBtnLabelFontSize_ValueChanged(sender As Object, e As System.EventArgs) Handles spinBtnLabelFontSize.ValueChanged
        If pFigure IsNot Nothing And Me.ckDataPointLabels.Checked Then Call Recalculate()
    End Sub

    Private Sub ckGridlines_CheckedChanged(sender As Object, e As System.EventArgs) Handles ckGridlines.CheckedChanged
        If pFigure IsNot Nothing Then Call Recalculate()
    End Sub

    Private Sub spinBtnMarkerSize_ValueChanged(sender As Object, e As System.EventArgs) Handles spinBtnMarkerSize.ValueChanged
        If pFigure IsNot Nothing Then Call Recalculate()
    End Sub

    Private Sub cbPointLabelPosition_SelectedIndexChanged(sender As Object, e As System.EventArgs) Handles cbPointLabelPosition.SelectedIndexChanged
        If pFigure IsNot Nothing And Me.ckDataPointLabels.Checked Then Call Recalculate()
    End Sub

    Private Sub ckDataPointLabels_CheckedChanged(sender As Object, e As System.EventArgs) Handles ckDataPointLabels.CheckedChanged
        If pFigure IsNot Nothing Then Call Recalculate()
    End Sub

    Private Sub ckXYplanePoints_CheckedChanged(sender As Object, e As System.EventArgs) Handles ckXYplanePoints.CheckedChanged
        If pFigure IsNot Nothing Then Call Recalculate()
    End Sub

    Private Sub ckYZplanePoints_CheckedChanged(sender As Object, e As System.EventArgs) Handles ckYZplanePoints.CheckedChanged
        If pFigure IsNot Nothing Then Call Recalculate()
    End Sub

    Private Sub ckXZplanePoints_CheckedChanged(sender As Object, e As System.EventArgs) Handles ckXZplanePoints.CheckedChanged
        If pFigure IsNot Nothing Then Call Recalculate()
    End Sub

    Private Sub ckZdropLines_CheckedChanged(sender As Object, e As System.EventArgs) Handles ckZdropLines.CheckedChanged
        If pFigure IsNot Nothing Then Call Recalculate()
    End Sub

    Private Sub btnAnimatedGif_Click(sender As Object, e As System.EventArgs) Handles btnAnimatedGif.Click

        '----------------------------------------------------------------------
        ' get animated gif data
        '----------------------------------------------------------------------
        If Me.RefEdit6_AnimatedGif.Address = String.Empty Then
            MsgBox("Animated Gif Inputs must be specified with 6 columns <Rotation X axis [degree]>, <Rotation Z axis [degree]>, <Zoom>, <Shift in X Direction>, <Shift in Y Direction>, <Delay>")
            Exit Sub
        Else
            If CheckRefEdit(Me.RefEdit6_AnimatedGif.Address, False) Then
                RefEditReset(Me.RefEdit6_AnimatedGif)
                Exit Sub
            End If
        End If

        Dim fData As MultiGroupsPairedData = getDataMultipleGroups()
        If fData.varNames.Length < 6 Then
            MsgBox("Animated Gif Inputs must be specified with 6 columns <Rotation X axis [degree]>, <Rotation Z axis [degree]>, <Zoom>, <Shift in X Direction>, <Shift in Y Direction>, <Delay>")
            Exit Sub
        End If

        Dim n As Integer = fData.X.GetLength(0)
        For i = 0 To n - 1
            If fData.X(i, 0) < 0 Or fData.X(i, 0) > 360 Then
                MsgBox($"<Rotation X axis [degree]> column values should be in range [0...360] but got {fData.X(i, 0)}. It should be in the 1st input column.")
                Exit Sub
            End If
            If fData.X(i, 1) < 0 Or fData.X(i, 1) > 360 Then
                MsgBox($"<Rotation Z axis [degree]> column values should be in range [0...360] but got {fData.X(i, 1)}. It should be in the 2nd input column.")
                Exit Sub
            End If
            If fData.X(i, 2) < 0 Then
                MsgBox($"<Zoom> column values should be .ge. 0 but got {fData.X(i, 2)}. It should be in the 3rd input column.")
                Exit Sub
            End If
            If fData.X(i, 3) < 0 Or fData.X(i, 3) > 100 Then
                MsgBox($"<Shift in X Direction> column values should be in range [0...100] but got {fData.X(i, 3)}. It should be in the 4th input column.")
                Exit Sub
            End If
            If fData.X(i, 4) < 0 Or fData.X(i, 4) > 100 Then
                MsgBox($"<Shift in Y Direction> column values should be in range [0...100] but got {fData.X(i, 4)}. It should be in the 5th input column.")
                Exit Sub
            End If
            If fData.X(i, 5) < 0 Then
                MsgBox($"<Delay [ms]> column values should be .ge. 0 but got {fData.X(i, 5)}. It should be in the 5th input column.")
                Exit Sub
            End If
        Next

        '----------------------------------------------------------------------
        ' get Actual chart data
        '----------------------------------------------------------------------
        Dim errText As String = String.Empty
        Dim data As DataObj
        Dim grpData() As String = Nothing, labelData() As String = Nothing
        Dim colId As Integer = 0

        'Get Data
        data = Me.getData(errText)
        If errText <> String.Empty Then
            MsgBox(errText, vbExclamation)
            Exit Sub
        End If

        Dim d = data.FinalData

        'check if we have groups data input
        If Me.RefEdit4_Group.Address <> String.Empty Then 'We have only One group
            grpData = Matrix.Array2strArray(Matrix.GetColumnFrom2Darray(d, 0))
            colId += 1
        End If
        'check if we have data lebels data input
        If Me.RefEdit5_Labels.Address <> String.Empty Then 'We have only One group
            labelData = Matrix.Array2strArray(Matrix.GetColumnFrom2Darray(d, colId))
            colId += 1
        End If

        Dim Xdata = Matrix.Array2dblArray(Matrix.GetColumnFrom2Darray(d, colId))
        Dim Ydata = Matrix.Array2dblArray(Matrix.GetColumnFrom2Darray(d, colId + 1))
        Dim Zdata = Matrix.Array2dblArray(Matrix.GetColumnFrom2Darray(d, colId + 2))
        Dim xlbl = data.varNames(colId)
        Dim ylbl = data.varNames(colId + 1)
        Dim zlbl = data.varNames(colId + 2)

        'get 3D object data
        Dim objects3D As New List(Of graphics.IXYZDrawable3D)
        If Me.RefEdit1_3Dobjects.Address <> String.Empty Then
            Dim data3Dobj = Me.get3DobjectsData()
            Dim err3D As String = ""
            objects3D = Create3DObjectsList(data3Dobj, err3D)
            If objects3D Is Nothing Then
                MsgBox(err3D, vbExclamation)
                Exit Sub
            End If
        End If

        If Not ChooseAnimatedGifOutput() Then Exit Sub

        Dim XYZplot As New graphics.XYZscatter
        With XYZplot
            .dataInputs(Xdata, Ydata, Zdata)
            .axesLabelInputs(xlbl, ylbl, zlbl)
            .showPlanePointInputs(Me.ckXYplanePoints.Checked, Me.ckYZplanePoints.Checked, Me.ckXZplanePoints.Checked,
                                      Int(Me.spinBtnXYPlanePointSize.Value), Int(Me.spinBtnYZPlanePointSize.Value), Int(Me.spinBtnXZPlanePointSize.Value))
            .ScaleAxis(Me.ckScaleAxes.Checked)
            .settingsInputs(Me.ckDataPointLabels.Checked, Me.ckZdropLines.Checked, Me.ckGridlines.Checked, Int(Me.spinBtnLabelFontSize.Value),
                     GetLabelPositionFromCombobox(Me.cbPointLabelPosition.Text), CInt(Me.spinBtnMarkerSize.Value), GetMarkerStyleFromCombobox(Me.cbMarkerSymbol.Text))

            'axis scaling
            Dim axisErr As String = ""
            If Not TryApplyManualAxisLimits(XYZplot, axisErr) Then
                MsgBox(axisErr, vbExclamation)
                Exit Sub
            End If

            ' Add wire sphere in RAW coordinates (same units as your data)
            .ClearObjects()
            .SetObjects(objects3D)

            ' Reverse X and Y display direction
            .AxisDirectionInputs(flipX:=Me.ckXreverseAxis.Checked, flipY:=Me.ckYreverseAxis.Checked, flipZ:=Me.ckZreverseAxis.Checked)

            If Me.ckDataPointLabels.Checked Then .SetDataLabels(labelData)
            If grpData IsNot Nothing Then .SetGroups(grpData)
        End With

        Dim gifList As New List(Of String)
        Dim delayList As New List(Of Integer)

        For i = 0 To n - 1

            If Me.pFigure Is Nothing Then
                AppGlobals.app.ActiveWorkbook.Charts.Add()
                Me.pFigure = AppGlobals.app.ActiveWorkbook.ActiveChart
            End If

            Dim rotX = fData.X(i, 0)
            Dim rotZ = fData.X(i, 1)
            Dim zoom = fData.X(i, 2)
            Dim shiftX = fData.X(i, 3)
            Dim shiftY = fData.X(i, 4)

            With XYZplot
                .rotationAndZoomInputs(zoom, shiftY, shiftX, rotX, rotZ)
                .draw(Me.pFigure)
            End With

            Dim framePath As String = Path.Combine(pAnimatedGifWorkDir, CStr(i) & ".gif")
            If File.Exists(framePath) Then File.Delete(framePath)
            Me.pFigure.Export(Filename:=framePath, FilterName:="GIF")
            gifList.Add(framePath)
            delayList.Add(CInt(fData.X(i, 5)))

            ProgressBar1.Value = 100 * ((i + 1) / n)
            System.Windows.Forms.Application.DoEvents()
        Next

        'create animated gif
        GifAnimator.CreateAnimatedGif(gifList, Me.pAnimatedGifFinalPath, delayList, 0,, Me.ProgressBar1)
        MsgBox("Animated gif was created " & Me.pAnimatedGifFinalPath)
    End Sub


    Private Function getDataMultipleGroups() As MultiGroupsPairedData
        Dim out = New MultiGroupsPairedData
        Dim columData = New DataObj
        Dim ref As String = prepareRef2D(Me.RefEdit6_AnimatedGif.Address, Me.RefEdit6_AnimatedGif.ExcelWorkBook)

        columData.DataInport(ref, True)
        out.X = columData.DataDbl()
        out.varNames = columData.varNames

        Return out
    End Function

    ''' <summary>
    ''' Prompts the user for the final animated GIF path (not created yet) and creates
    ''' a working subfolder in the same directory for individual frame GIFs.
    ''' </summary>
    Private Function ChooseAnimatedGifOutput() As Boolean
        Using sfd As New SaveFileDialog()
            sfd.Title = "Choose output animated GIF (frames will be saved to a subfolder)"
            sfd.Filter = "GIF image (*.gif)|*.gif"
            sfd.DefaultExt = "gif"
            sfd.AddExtension = True
            sfd.OverwritePrompt = True
            sfd.FileName = "xyz_animation.gif"

            If sfd.ShowDialog(Me) <> DialogResult.OK Then
                Return False
            End If

            pAnimatedGifFinalPath = sfd.FileName
        End Using

        Dim outDir = Path.GetDirectoryName(pAnimatedGifFinalPath)
        Dim baseName = Path.GetFileNameWithoutExtension(pAnimatedGifFinalPath)
        pAnimatedGifWorkDir = Path.Combine(outDir, baseName & "_frames")

        Directory.CreateDirectory(pAnimatedGifWorkDir)
        Return True
    End Function

    Private Sub cbXreverseAxis_CheckedChanged(sender As Object, e As System.EventArgs)
        If pFigure IsNot Nothing Then Call Recalculate()
    End Sub

    Private Sub cbYreverseAxis_CheckedChanged(sender As Object, e As System.EventArgs)
        If pFigure IsNot Nothing Then Call Recalculate()
    End Sub

    Private Sub cbZreverseAxis_CheckedChanged(sender As Object, e As System.EventArgs)
        If pFigure IsNot Nothing Then Call Recalculate()
    End Sub

    Private Sub spinBtnR_ValueChanged(sender As Object, e As System.EventArgs) Handles spinBtnR.ValueChanged
        If pFigure IsNot Nothing Then Call Recalculate()
    End Sub

    Private Sub spinBtnG_ValueChanged(sender As Object, e As System.EventArgs) Handles spinBtnG.ValueChanged
        If pFigure IsNot Nothing Then Call Recalculate()
    End Sub

    Private Sub spinBtnB_ValueChanged(sender As Object, e As System.EventArgs) Handles spinBtnB.ValueChanged
        If pFigure IsNot Nothing Then Call Recalculate()
    End Sub

    Private Sub spinBtnLatitudeRings_ValueChanged(sender As Object, e As System.EventArgs) Handles spinBtnLatitudeRings.ValueChanged
        If pFigure IsNot Nothing Then Call Recalculate()
    End Sub

    Private Sub spinBtnLongitudeRings_ValueChanged(sender As Object, e As System.EventArgs) Handles spinBtnLongitudeRings.ValueChanged
        If pFigure IsNot Nothing Then Call Recalculate()
    End Sub

    Private Sub spinBtnPointsPerRing_ValueChanged(sender As Object, e As System.EventArgs) Handles spinBtnPointsPerRing.ValueChanged
        If pFigure IsNot Nothing Then Call Recalculate()
    End Sub

    Private Sub ckScaleAxes_CheckedChanged(sender As Object, e As System.EventArgs) Handles ckScaleAxes.CheckedChanged
        If pFigure IsNot Nothing Then Call Recalculate()
    End Sub

    Private Sub ckXreverseAxis_CheckedChanged(sender As Object, e As System.EventArgs) Handles ckXreverseAxis.CheckedChanged
        If pFigure IsNot Nothing Then Call Recalculate()
    End Sub

    Private Sub ckYreverseAxis_CheckedChanged(sender As Object, e As System.EventArgs) Handles ckYreverseAxis.CheckedChanged
        If pFigure IsNot Nothing Then Call Recalculate()
    End Sub

    Private Sub ckZreverseAxis_CheckedChanged(sender As Object, e As System.EventArgs) Handles ckZreverseAxis.CheckedChanged
        If pFigure IsNot Nothing Then Call Recalculate()
    End Sub

    Private Sub cbMarkerSymbol_SelectedIndexChanged(sender As Object, e As System.EventArgs) Handles cbMarkerSymbol.SelectedIndexChanged
        If pFigure IsNot Nothing Then Call Recalculate()
    End Sub

    Private Function GetMarkerStyleFromCombobox(txt_style As String) As XlMarkerStyle
        Select Case txt_style
            Case "Circle"
                Return XlMarkerStyle.xlMarkerStyleCircle
            Case "Square"
                Return XlMarkerStyle.xlMarkerStyleSquare
            Case "Diamond"
                Return XlMarkerStyle.xlMarkerStyleDiamond
            Case "Triangle"
                Return XlMarkerStyle.xlMarkerStyleTriangle
            Case "X"
                Return XlMarkerStyle.xlMarkerStyleX
            Case "Plus"
                Return XlMarkerStyle.xlMarkerStylePlus
            Case "Star"
                Return XlMarkerStyle.xlMarkerStyleStar
            Case Else
                Return XlMarkerStyle.xlMarkerStyleCircle
        End Select
    End Function
End Class