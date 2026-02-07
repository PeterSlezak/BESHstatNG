Imports System.IO
Imports System.Runtime.InteropServices.ComTypes
Imports System.Windows
Imports System.Windows.Forms
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
        Me.RefEdit1_X.ExcelConnector = BESHstatGlobals.app
        Me.RefEdit2_Y.ExcelConnector = BESHstatGlobals.app
        Me.RefEdit3_Z.ExcelConnector = BESHstatGlobals.app
        Me.RefEdit4_Group.ExcelConnector = BESHstatGlobals.app
        Me.RefEdit5_Labels.ExcelConnector = BESHstatGlobals.app
        Me.RefEdit6_AnimatedGif.ExcelConnector = BESHstatGlobals.app

        With Me.cbPointLabelPosition.Items
            .Add("Right")
            .Add("Left")
            .Add("Above")
            .Add("Below")
        End With
        Me.cbPointLabelPosition.SelectedIndex = 0

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

        Return bOut
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
            grpData = Array2strArray(GetColumnFrom2Darray(d, 0))
            colId += 1
        End If
        'check if we have data lebels data input
        If Me.RefEdit5_Labels.Address <> String.Empty Then 'We have only One group
            labelData = Array2strArray(GetColumnFrom2Darray(d, colId))
            colId += 1
        End If

        Dim Xdata = Array2dblArray(GetColumnFrom2Darray(d, colId))
        Dim Ydata = Array2dblArray(GetColumnFrom2Darray(d, colId + 1))
        Dim Zdata = Array2dblArray(GetColumnFrom2Darray(d, colId + 2))
        Dim xlbl = data.varNames(colId)
        Dim ylbl = data.varNames(colId + 1)
        Dim zlbl = data.varNames(colId + 2)

        If Me.pFigure Is Nothing Then
            app.ActiveWorkbook.Charts.Add()
            Me.pFigure = app.ActiveWorkbook.ActiveChart
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
                            GetLabelPositionFromCombobox(Me.cbPointLabelPosition.Text), Me.spinBtnMarkerSize.Value)

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
            BSerr.LogAndThrow(ex, False, True)
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

    Private Sub ckScaleAxes_CheckedChanged(sender As Object, e As System.EventArgs) Handles ckScaleAxes.CheckedChanged
        If pFigure IsNot Nothing Then Call Recalculate()
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
            grpData = Array2strArray(GetColumnFrom2Darray(d, 0))
            colId += 1
        End If
        'check if we have data lebels data input
        If Me.RefEdit5_Labels.Address <> String.Empty Then 'We have only One group
            labelData = Array2strArray(GetColumnFrom2Darray(d, colId))
            colId += 1
        End If

        Dim Xdata = Array2dblArray(GetColumnFrom2Darray(d, colId))
        Dim Ydata = Array2dblArray(GetColumnFrom2Darray(d, colId + 1))
        Dim Zdata = Array2dblArray(GetColumnFrom2Darray(d, colId + 2))
        Dim xlbl = data.varNames(colId)
        Dim ylbl = data.varNames(colId + 1)
        Dim zlbl = data.varNames(colId + 2)

        If Not ChooseAnimatedGifOutput() Then Exit Sub

        Dim XYZplot As New graphics.XYZscatter
        With XYZplot
            .dataInputs(Xdata, Ydata, Zdata)
            .axesLabelInputs(xlbl, ylbl, zlbl)
            .showPlanePointInputs(Me.ckXYplanePoints.Checked, Me.ckYZplanePoints.Checked, Me.ckXZplanePoints.Checked,
                                      Int(Me.spinBtnXYPlanePointSize.Value), Int(Me.spinBtnYZPlanePointSize.Value), Int(Me.spinBtnXZPlanePointSize.Value))
            .ScaleAxis(Me.ckScaleAxes.Checked)
            .settingsInputs(Me.ckDataPointLabels.Checked, Me.ckZdropLines.Checked, Me.ckGridlines.Checked, Int(Me.spinBtnLabelFontSize.Value),
                                GetLabelPositionFromCombobox(Me.cbPointLabelPosition.Text), Me.spinBtnMarkerSize.Value)

            If Me.ckDataPointLabels.Checked Then .SetDataLabels(labelData)
            If grpData IsNot Nothing Then .SetGroups(grpData)
        End With

        Dim gifList As New List(Of String)
        Dim delayList As New List(Of Integer)

        For i = 0 To n - 1

            If Me.pFigure Is Nothing Then
                app.ActiveWorkbook.Charts.Add()
                Me.pFigure = app.ActiveWorkbook.ActiveChart
                'Else
                'delete old figure and recrate it
                'app.DisplayAlerts = False
                'Me.pFigure.Delete()
                'app.DisplayAlerts = True
                'app.ActiveWorkbook.Charts.Add()
                'Me.pFigure = app.ActiveWorkbook.ActiveChart
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

End Class