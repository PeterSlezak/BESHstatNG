Imports System.ComponentModel
Imports System.Drawing
Imports System.Runtime.InteropServices
Imports System.Security.Cryptography
Imports System.Windows.Forms
Imports BESHStatNG.AppInfrastructure
Imports Microsoft.Office.Interop
Imports Microsoft.Office.Interop.Excel

' code is based on https://www.codeproject.com/articles/RefEdit-Emulation-for-NET with
' few modifications here and there

''' <summary>
''' This structure is used by the Resize method to store Parent and Control dimensions.
''' </summary>
''' <remarks></remarks>
Public Structure RefEditState
    Public ParentClientSize As Size
    Public ParentMinimumSize As Size
    Public IsParentMinimized As Boolean
    Public ParentPrevBorder As FormBorderStyle
    Public ShowParentControlBox As Boolean
    Public ControlPrevX As Integer
    Public ControlPrevY As Integer
    Public ControlAnchor As AnchorStyles
    Public ActualParent As Control
End Structure

<DefaultEvent("Changed")>
Public Class Excel2007RefEdit

    Private xlBook As Excel.Workbook
    Private xlSheet As Excel.Worksheet
    Private xlApp As Excel.Application
    ' Borrowed RCWs; leases own only the event subscriptions, never the RCWs.
    Private ReadOnly selectionLease As New RefEditEventLease(Of Excel.Range)
    Private ReadOnly workbookLease As New RefEditEventLease(Of Object)
    Private captureFaulted As Boolean
    Private captureTransitioning As Boolean
    Private DisplayState As RefEditState
    Private pbInTextAdressHandler As Boolean = False

    Public ReadOnly Property ExcelWorkBook() As Excel.Workbook
        Get
            Return Me.xlBook
        End Get
    End Property

    Public Sub New()

        ' This call is required by the designer.
        InitializeComponent()

        ' Add any initialization after the InitializeComponent() call.
        Me.btnState.Image = _ImageMaximized
    End Sub

#Region "Base Properties that are Browsable = False"
    <Browsable(False)>
    Public Overrides Property Font As System.Drawing.Font
        Get
            Return MyBase.Font
        End Get
        Set(ByVal value As System.Drawing.Font)
            MyBase.Font = value
        End Set
    End Property

    <Browsable(False)>
    Public Overrides Property ForeColor As System.Drawing.Color
        Get
            Return MyBase.ForeColor
        End Get
        Set(ByVal value As System.Drawing.Color)
            MyBase.ForeColor = value
        End Set
    End Property

    <Browsable(False)>
    Public Overrides Property RightToLeft As System.Windows.Forms.RightToLeft
        Get
            Return MyBase.RightToLeft
        End Get
        Set(ByVal value As System.Windows.Forms.RightToLeft)
            MyBase.RightToLeft = value
        End Set
    End Property
#End Region

    Private Sub xlSheetChangeHandle(ByVal target As Excel.Worksheet)
        If Me.IsDisposed OrElse captureFaulted OrElse captureTransitioning OrElse Not pbInTextAdressHandler Then Return
        RunCaptureBoundary("SheetActivate", Sub()
                                               If target Is Nothing Then
                                                   xlSheet = Nothing
                                                   Return
                                               End If
                                               If Not Object.ReferenceEquals(target.Parent, xlBook) OrElse
                                                  Not Object.ReferenceEquals(_ExcelConnector.ActiveWorkbook, xlBook) Then Return
                                               xlSheet = target
                                               BindSelection(target)
                                               pbInTextAdressHandler = True
                                           End Sub)
    End Sub

    Private Sub BindWorkbook(book As Excel.Workbook)
        workbookLease.Rebind(Function(callback)
                                 Dim handler As Excel.WorkbookEvents_SheetActivateEventHandler = Sub(value) callback(value)
                                 AddHandler book.SheetActivate, handler
                                 Return Sub() RemoveHandler book.SheetActivate, handler
                             End Function,
                             Sub(value)
                                 If Object.ReferenceEquals(book, xlBook) Then xlSheetChangeHandle(TryCast(value, Excel.Worksheet))
                             End Sub)
    End Sub

    Private Sub BindSelection(sheet As Excel.Worksheet)
        selectionLease.Rebind(Function(callback)
                                  Dim handler As Excel.DocEvents_SelectionChangeEventHandler = Sub(value) callback(value)
                                  AddHandler sheet.SelectionChange, handler
                                  Return Sub() RemoveHandler sheet.SelectionChange, handler
                              End Function, AddressOf SelectionChange)
    End Sub

    ' Callable production boundary: host-free tests inject detach failures into its
    ' leases. They do not simulate Excel's COM connection points or STA ordering.
    Friend Sub RunCaptureBoundary(context As String, operation As System.Action)
        pbInTextAdressHandler = False
        If Me.IsDisposed OrElse captureFaulted Then Return
        If captureTransitioning Then
            FaultCapture(context, New InvalidOperationException("Reentrant RefEdit transition"))
            Return
        End If
        captureTransitioning = True
        Try
            selectionLease.Clear()
            If selectionLease.IsClosed OrElse workbookLease.IsClosed Then Throw New InvalidOperationException("Closed RefEdit lease")
            operation()
            If captureFaulted OrElse selectionLease.IsClosed OrElse workbookLease.IsClosed Then Throw New InvalidOperationException("Context closed during transition")
        Catch ex As Exception
            FaultCapture(context, ex)
        Finally
            captureTransitioning = False
        End Try
    End Sub

    Friend ReadOnly Property CaptureRequiresRecreation As Boolean
        Get
            Return captureFaulted OrElse selectionLease.IsClosed OrElse workbookLease.IsClosed
        End Get
    End Property

    Private Sub FaultCapture(context As String, failure As Exception)
        pbInTextAdressHandler = False
        captureFaulted = True ' Never reset: close and recreate the control after a fault.
        LogRefEdit($"{context}: capture disabled; recreate control: {failure}")
        CloseCaptureLeases()
        xlSheet = Nothing
        xlBook = Nothing
        xlApp = Nothing
        _ExcelConnector = Nothing
    End Sub

    Private Sub CloseCaptureLeases()
        ' Invalidate BOTH callback generations before either provider is called.
        selectionLease.Invalidate()
        workbookLease.Invalidate()
        Try
            selectionLease.Dispose()
        Catch ex As Exception
            LogRefEdit($"Selection detach failure: {ex}")
        End Try
        Try
            workbookLease.Dispose()
        Catch ex As Exception
            LogRefEdit($"Workbook detach failure: {ex}")
        End Try
    End Sub


#Region "Properties"

    Private _ExcelConnector As Excel.Application
    Private _ImageMinimized As Image = My.Resources.imgMinimized
    Private _ImageMaximized As Image = My.Resources.imgMaximized
    Private _IncludeSheetName As Boolean = True
    Private _ShowRowAbsoluteIndicator As Boolean = True
    Private _ShowColumnAbsoluteIndicator As Boolean = True

    ''' <summary>
    ''' This connections the control to an instance of Excel.
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks>It is not visible in the controls PropertyGrid.</remarks>
    <Browsable(False), Category("RefEdit"), Description("Connects the control to an instance of Excel.")>
    Public Property ExcelConnector As Excel.Application
        Get
            Return _ExcelConnector
        End Get
        Set(ByVal value As Excel.Application)
            RunCaptureBoundary("ExcelConnector", Sub()
                                                    workbookLease.Clear()
                                                    xlBook = Nothing
                                                    xlSheet = Nothing
                                                    xlApp = Nothing
                                                    _ExcelConnector = Nothing
                                                    If value Is Nothing Then Return
                                                    ' Publish a new connector only after the old sources detach.
                                                    _ExcelConnector = value
                                                    xlApp = value
                                                    If value.Workbooks.Count = 0 Then Return
                                                    Dim book = value.ActiveWorkbook
                                                    If book Is Nothing Then Return
                                                    xlBook = book
                                                    BindWorkbook(book)
                                                    xlSheet = TryCast(book.ActiveSheet, Excel.Worksheet)
                                                End Sub)
        End Set
    End Property

    ''' <summary>
    ''' Indicates whether the component should draw right-to-left for RTL languages.
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    <Category("RefEdit Appearance"), Description("Indicates whether the component should draw right-to-left for RTL languages."), DisplayName("RightToLeaf") _
    , DefaultValue(GetType(RightToLeft), "No")>
    Public Property RefEditRightToLeft As System.Windows.Forms.RightToLeft
        Get
            Return Me.txtAddress.RightToLeft
        End Get
        Set(ByVal value As System.Windows.Forms.RightToLeft)
            Me.txtAddress.RightToLeft = value
        End Set
    End Property

    ''' <summary>
    ''' The font used to display text in the RefEdit control.
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    <Category("RefEdit Appearance"), Description("The font used to display text in the RefEdit control."), DisplayName("Font")>
    Public Property RefEditFont As System.Drawing.Font
        Get
            Return Me.txtAddress.Font
        End Get
        Set(ByVal value As System.Drawing.Font)
            Me.txtAddress.Font = value
        End Set
    End Property

    ''' <summary>
    ''' The foreground color of this RefEdit control, which is used to display text.
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    <Category("RefEdit Appearance"), Description("The foreground color of this RefEdit control, which is used to display text."), DisplayName("Forecolor") _
    , DefaultValue(GetType(Color), "WindowText")>
    Public Property RefEditForecolor As System.Drawing.Color
        Get
            Return Me.txtAddress.ForeColor
        End Get
        Set(ByVal value As System.Drawing.Color)
            Me.txtAddress.ForeColor = value
        End Set
    End Property

    ''' <summary>
    ''' The RefEdit Controls displayed range.
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    <Category("RefEdit Appearance"), Description("The selected range's address.")>
    Public Property Address As String
        Get
            Return Me.txtAddress.Text
        End Get
        Set(value As String)
            Dim prev = Me.txtAddress.Text
            Me.txtAddress.Text = value
            'LogRefEdit($"Address set; prev='{prev}', new='{value}', Visible={Me.Visible}, Enabled={Me.Enabled}, ForeColor={Me.txtAddress.ForeColor}, BackColor={Me.txtAddress.BackColor}, Color [WindowText]={System.Drawing.SystemColors.WindowText.ToArgb().ToString()}, Color [Window]={System.Drawing.SystemColors.Window.ToArgb().ToString()}")
            Dim fg As Color = Me.txtAddress.ForeColor
            Dim bg As Color = Me.txtAddress.BackColor
        End Set
    End Property

    ''' <summary>
    ''' Indicates if the worksheet name should be included in the selected range.
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    <Category("RefEdit Appearance"), Description("Indicates if the worksheet name should be included in the selected range."), DefaultValue(True)>
    Public Property IncludeSheetName As Boolean
        Get
            Return _IncludeSheetName
        End Get
        Set(ByVal value As Boolean)
            _IncludeSheetName = value
            Me.Invalidate()
        End Set
    End Property

    ''' <summary>
    ''' Shows the row absolute indicator ($) in the selected range.
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    <Category("RefEdit Appearance"), Description("Shows the row absolute indicator ($) in the selected range."), DefaultValue(True)>
    Public Property ShowRowAbsoluteIndicator As Boolean
        Get
            Return _ShowRowAbsoluteIndicator
        End Get
        Set(ByVal value As Boolean)
            _ShowRowAbsoluteIndicator = value
            Me.Invalidate()
        End Set
    End Property

    ''' <summary>
    ''' Shows the column absolute indicator ($) in the selected range.
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    <Category("RefEdit Appearance"), Description("Shows the column absolute indicator ($) in the selected range."), DefaultValue(True)>
    Public Property ShowColumnAbsoluteIndicator As Boolean
        Get
            Return _ShowColumnAbsoluteIndicator
        End Get
        Set(ByVal value As Boolean)
            _ShowColumnAbsoluteIndicator = value
            Me.Invalidate()
        End Set
    End Property

    ''' <summary>
    ''' For buttons whose FlatStyle is FlatStyle.Flat, determines the appearance of the border and the colors used to indicate check state and mouse state.
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    <Category("RefEdit DropButton"), Description("For buttons whose FlatStyle is FlatStyle.Flat, determines the appearance of the border and the colors used to indicate check state and mouse state.") _
    , DisplayName("FlatAppearance")>
    Public Property FlatAppearance As FlatButtonAppearance
        Get
            Return Me.btnState.FlatAppearance
        End Get
        Set(ByVal value As FlatButtonAppearance)
            With Me.btnState.FlatAppearance
                .BorderColor = value.BorderColor
                .BorderSize = value.BorderSize
                .CheckedBackColor = value.CheckedBackColor
                .MouseDownBackColor = value.MouseDownBackColor
                .MouseOverBackColor = value.MouseOverBackColor
            End With
        End Set
    End Property

    ''' <summary>
    ''' Determines the appearance of the control when a user moves the mouse over the control and clicks.
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    <Category("RefEdit DropButton"), Description("Determines the appearance of the control when a user moves the mouse over the control and clicks."), DisplayName("FlatStyle") _
    , DefaultValue(GetType(FlatStyle), "Standard")>
    Public Property DropButtonFlatStyle As FlatStyle
        Get
            Return Me.btnState.FlatStyle
        End Get
        Set(ByVal value As FlatStyle)
            Me.btnState.FlatStyle = value
        End Set
    End Property

    ''' <summary>
    ''' The image displayed when the control has been minimized.
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    <Category("RefEdit DropButton"), Description("The image displayed when the control has been minimized.")>
    Public Property ImageMinimized As Image
        Get
            Return _ImageMinimized
        End Get
        Set(ByVal value As Image)

            If value Is Nothing Then
                _ImageMinimized = My.Resources.imgMinimized
            Else
                _ImageMinimized = value
            End If

            Me.Invalidate()
        End Set
    End Property

    ''' <summary>
    ''' The image displayed when the control has been maximized.
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    <Category("RefEdit DropButton"), Description("The image displayed when the control has been maximized.")>
    Public Property ImageMaximized As Image
        Get
            Return _ImageMaximized
        End Get
        Set(ByVal value As Image)

            If value Is Nothing Then
                _ImageMaximized = My.Resources.imgMinimized
            Else
                _ImageMaximized = value
            End If

            btnState.Image = _ImageMaximized

            Me.Invalidate()
        End Set
    End Property

#End Region

#Region "Events"

    ''' <summary>
    ''' Event is fired after a selection has been made.
    ''' </summary>
    ''' <remarks></remarks>
    <Category("RefEdit"), Description("This event is fired after a selection is made on the Excel spreadsheet.")>
    Public Event Changed As EventHandler

    ''' <summary>
    ''' Event is fired after the Dropbutton has been clicked.
    ''' </summary>
    ''' <remarks></remarks>
    <Category("RefEdit"), Description("This event is fired after the DropButton is clicked.")>
    Public Event DropButtonClicked As EventHandler

    ''' <summary>
    ''' Event is fired after the control has been minimized/maximized using the DropButton.
    ''' </summary>
    ''' <remarks></remarks>
    <Category("RefEdit"), Description("This event is fired after the control is resized via the DropButton.")>
    Public Event AfterResize As EventHandler(Of EventArgs.AfterResizeEventArgs)

    ''' <summary>
    ''' Event is fired before the control has been minimized/maximized using the DropButton.
    ''' </summary>
    ''' <remarks>This event can be cancelled.</remarks>
    <Category("RefEdit"), Description("This event is fired before the control is resized via the DropButton.")>
    Public Event BeforeResize As EventHandler(Of EventArgs.BeforeResizeEventArgs)

#End Region

#Region "Internal SelectionChange Events"
    ''' <summary>
    ''' Used to manage cross threading.
    ''' </summary>
    ''' <param name="value"></param>
    ''' <remarks></remarks>
    Private Delegate Sub WriteValue(ByVal value As String, ByVal generation As Long)

    ''' <summary>
    ''' Internal method that handles the xlSheet.SelectionChange event.
    ''' </summary>
    ''' <param name="target"></param>
    ''' <remarks></remarks>
    Private Sub SelectionChange(ByVal target As Excel.Range)
        If Me.IsDisposed OrElse captureFaulted OrElse captureTransitioning OrElse Not pbInTextAdressHandler Then Return
        Try
            If target Is Nothing Then Return
            Dim generation = selectionLease.CurrentGeneration
            If Not selectionLease.IsCurrent(generation) Then Return
            If Not Object.ReferenceEquals(target.Worksheet, xlSheet) OrElse
               Not Object.ReferenceEquals(target.Worksheet.Parent, xlBook) OrElse
               Not Object.ReferenceEquals(_ExcelConnector.ActiveWorkbook, xlBook) Then Return
            Dim address As String = ""
            Dim SheetName As String = "'" & target.Worksheet.Name & "'!"

            For Each rng As Excel.Range In target.Areas

                If address.Length > 0 Then address &= ","
                If _IncludeSheetName Then address &= SheetName
                address &= rng.Address(_ShowRowAbsoluteIndicator, _ShowColumnAbsoluteIndicator, Excel.XlReferenceStyle.xlA1)

                'LogRefEdit($"SelectionChange: area address fragment='{address}'")
                Call WriteData(address, generation)
                If Not selectionLease.IsCurrent(generation) Then Return
                ' Borrowed event/Areas RCW: no ReleaseComObject.
            Next

            'Call _NAR(target)
        Catch ex As Exception
            FaultCapture("SelectionChange", ex)
        End Try
    End Sub

    ''' <summary>
    ''' This method writes the value to the textbox and Address property.
    ''' </summary>
    ''' <param name="value"></param>
    ''' <remarks></remarks>
    Private Sub WriteData(ByVal value As String, ByVal generation As Long)

        Try
            If Me.IsDisposed OrElse captureFaulted OrElse Not pbInTextAdressHandler OrElse Not selectionLease.IsCurrent(generation) Then Return

            If Not Me.IsHandleCreated Then
                Try
                    Dim h = Me.Handle ' force handle creation
                Catch ex As Exception
                    LogRefEdit($"WriteData: failed to create handle: {ex}; skipping UI update")
                    Return
                End Try
            End If

            If Me.InvokeRequired Then
                Me.BeginInvoke(New WriteValue(AddressOf WriteData), New Object() {value, generation})
                Return
            End If

            If _ExcelConnector Is Nothing OrElse xlBook Is Nothing OrElse xlSheet Is Nothing Then Return
            If Not Object.ReferenceEquals(_ExcelConnector.ActiveWorkbook, xlBook) OrElse
               Not Object.ReferenceEquals(xlBook.ActiveSheet, xlSheet) Then Return
            Me.Address = value
            Me.EnsureReadableText()

            ' bring to front and select to avoid z-order issues
            Try
                Me.txtAddress.BringToFront()
                Me.txtAddress.Select()
            Catch ex As Exception
                LogRefEdit($"BringToFront/Select error: {ex}")
            End Try

            ' Force repaint/update to avoid invisible text due to focus/paint race
            Try
                Me.txtAddress.Invalidate()
                Me.txtAddress.Update()
                Me.txtAddress.Refresh()

            Catch ex As Exception
                LogRefEdit($"WriteData: UI refresh error: {ex}")
            End Try

            If selectionLease.IsCurrent(generation) Then RaiseEvent Changed(Me, New System.EventArgs())
        Catch ex As Exception
            FaultCapture("WriteData", ex)
        End Try
    End Sub

    Private Sub EnsureReadableText()
        Dim fg = Me.txtAddress.ForeColor
        Dim bg = Me.txtAddress.BackColor
        If fg.ToArgb() = bg.ToArgb() Or fg.A < 255 Then
            ' compute brightness of background
            Dim brightness = (bg.R * 0.299 + bg.G * 0.587 + bg.B * 0.114)
            If brightness > 128 Then
                Me.txtAddress.ForeColor = Color.Black
                Me.txtAddress.BackColor = Color.White
            Else
                Me.txtAddress.ForeColor = Color.White
                Me.txtAddress.BackColor = Color.Black
            End If
        End If
    End Sub
#End Region

#Region "Internal txtAddress Events"

    ''' <summary>
    ''' Maintains the Handler to the SelectionChange event.
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    ''' <remarks></remarks>
    Private Sub txtAddress_Enter(ByVal sender As Object, ByVal e As System.EventArgs) Handles txtAddress.Enter
        RunCaptureBoundary("txtAddress_Enter", Sub()
                                                   If _ExcelConnector Is Nothing Then Return
                                                   Dim activeBook = _ExcelConnector.ActiveWorkbook
                                                   If activeBook Is Nothing Then Return
                                                   If Not Object.ReferenceEquals(activeBook, xlBook) Then
                                                       workbookLease.Clear()
                                                       xlBook = Nothing
                                                       xlSheet = Nothing
                                                       xlBook = activeBook
                                                       BindWorkbook(activeBook)
                                                   End If
                                                   If xlSheet Is Nothing Then xlSheet = TryCast(activeBook.ActiveSheet, Excel.Worksheet)
                                                   If xlSheet Is Nothing Then Return
                                                   ' Retain the entered address on every failure or workbook change.
                                                   If Me.Address <> String.Empty Then
                                                       If Not Object.ReferenceEquals(xlSheet, activeBook.ActiveSheet) Then xlSheet.Activate()
                                                       Try
                                                           xlSheet.Range(Me.Address).Select()
                                                       Catch ex As Exception
                                                           Dim addressedSheet = WorksheetFromRefAdress(Me.Address)
                                                           If addressedSheet Is Nothing OrElse Not Object.ReferenceEquals(addressedSheet.Parent, activeBook) Then Throw
                                                           xlSheet = addressedSheet
                                                           xlSheet.Activate()
                                                           xlSheet.Range(Me.Address).Select()
                                                       End Try
                                                   Else
                                                       xlSheet = TryCast(activeBook.ActiveSheet, Excel.Worksheet)
                                                       If xlSheet Is Nothing Then Return
                                                   End If
                                                   BindSelection(xlSheet)
                                                   If Me.FindForm() IsNot Nothing Then Me.FindForm().TopMost = True
                                                   pbInTextAdressHandler = True
                                               End Sub)
    End Sub

    'Private Sub xlApp_WorkbookActivate(Wb As Excel.Workbook) Handles xlApp.WorkbookActivate
    '    LogRefEdit($"WorkbookActivate: {Wb.Name}")
    '    xlBook = Wb

    '    If xlBook.Sheets.Count > 0 AndAlso xlBook.ActiveSheet.Type = XlSheetType.xlWorksheet Then
    '        ' Move selection handler to the active sheet if we are in capture mode
    '        If pbInTextAdressHandler Then
    '            If xlSheet IsNot Nothing Then
    '                RemoveHandler xlSheet.SelectionChange, AddressOf SelectionChange
    '                LogRefEdit($"Handler removed from old sheet={xlSheet.Name}")
    '            End If

    '            xlSheet = CType(xlBook.ActiveSheet, Excel.Worksheet)
    '            AddHandler xlSheet.SelectionChange, AddressOf SelectionChange
    '            LogRefEdit($"Handler attached to sheet={xlSheet.Name} in workbook={xlBook.Name}")
    '        Else
    '            xlSheet = CType(xlBook.ActiveSheet, Excel.Worksheet)
    '        End If
    '    End If
    'End Sub

    ''' <summary>
    ''' Removes the Handler once the textbox is no longer in use.
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    ''' <remarks></remarks>
    Private Sub txtAddress_Leave(ByVal sender As Object, ByVal e As System.EventArgs) Handles txtAddress.Leave
        ' Cleanup is independent of connector/sheet presence. Capture stays off.
        RunCaptureBoundary("txtAddress_Leave", Sub() Return)
    End Sub

    ''' <summary>
    ''' Maintains the resize of the control if applicable.
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    ''' <remarks></remarks>
    Private Sub txtAddress_PreviewKeyDown(ByVal sender As Object, ByVal e As System.Windows.Forms.PreviewKeyDownEventArgs) Handles txtAddress.PreviewKeyDown
        If e.KeyCode = Keys.F4 Then
            Call _Resize()
            Me.txtAddress.Focus()
        End If
    End Sub

#End Region

#Region "Internal btnState Events"
    ''' <summary>
    ''' Maintains the resize of the control.
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    ''' <remarks></remarks>
    Private Sub btnState_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnState.Click
        Call _Resize()
        Me.txtAddress.Focus()

        RaiseEvent DropButtonClicked(Me, New System.EventArgs)
    End Sub

    ''' <summary>
    ''' Executes prior to the resize occuring. Allowing for the chance of the resize event to be cancelled.
    ''' </summary>
    ''' <param name="e"></param>
    ''' <remarks></remarks>
    Private Sub OnBeforeResize(ByVal e As EventArgs.BeforeResizeEventArgs)
        RaiseEvent BeforeResize(Me, e)
    End Sub

    ''' <summary>
    ''' This method minimizes/maximizes the control based on its current display state.
    ''' </summary>
    ''' <remarks></remarks>
    Private Sub _Resize()

        ' Manages the BeforeResize Event
        Dim args As New EventArgs.BeforeResizeEventArgs With {.DisplayState = Me.DisplayState}
        Call OnBeforeResize(args)
        If args.Cancel Then Return

        ' Hides/Shows all of the controls on the form
        For Each c As Control In Me.FindForm().Controls 'ParentForm.Controls
            c.Visible = DisplayState.IsParentMinimized
        Next

        ' Makes sure the control is visible
        Me.Visible = True

        ' This is a fix related to placing the control into other container types such
        ' as TabControls or Groupboxes.
        If DisplayState.ActualParent Is Nothing Then DisplayState.ActualParent = Me.Parent

        If Not DisplayState.IsParentMinimized Then

            ' This is a fix related to placing the control into other container types such
            ' as TabControls or Groupboxes.
            If Not TypeOf DisplayState.ActualParent Is Form Then
                Me.ParentForm.Controls.Add(Me)
            End If

            ' Set the button's image to the minimized image
            Me.btnState.Image = _ImageMinimized

            ' Store the current state of the form
            DisplayState.ParentClientSize = ParentForm.ClientSize
            DisplayState.ParentMinimumSize = ParentForm.MinimumSize
            DisplayState.ParentPrevBorder = ParentForm.FormBorderStyle
            DisplayState.ShowParentControlBox = ParentForm.ControlBox

            ' Store the current state of the control
            DisplayState.ControlPrevX = Me.Left
            DisplayState.ControlPrevY = Me.Top
            DisplayState.ControlAnchor = Me.Anchor

            ' Set the new state minimized state of the form.
            ParentForm.MinimumSize = New Size(Me.Width, Me.Height)
            ParentForm.ClientSize = New Size(Me.Width, Me.Height)
            Me.ParentForm.FormBorderStyle = FormBorderStyle.FixedDialog
            Me.ParentForm.ControlBox = False

            ' Set the location of the control
            Me.Anchor = AnchorStyles.Left
            Me.Left = 0
            Me.Top = 0

        Else

            ' This is a fix related to placing the control into other container types such
            ' as TabControls or Groupboxes.
            If Not TypeOf DisplayState.ActualParent Is Form Then
                Me.ParentForm.Controls.Remove(Me)
                DisplayState.ActualParent.Controls.Add(Me)
            End If

            ' Set the button's image to the maximized image
            Me.btnState.Image = _ImageMaximized

            ' Set the form to the stored state
            ParentForm.ClientSize = DisplayState.ParentClientSize
            ParentForm.MinimumSize = DisplayState.ParentMinimumSize
            ParentForm.FormBorderStyle = DisplayState.ParentPrevBorder
            ParentForm.ControlBox = DisplayState.ShowParentControlBox

            ' Set the control to the stored state
            Me.Anchor = DisplayState.ControlAnchor
            Me.Left = DisplayState.ControlPrevX
            Me.Top = DisplayState.ControlPrevY

        End If

        ' Set the display state of the parent
        DisplayState.IsParentMinimized = Not DisplayState.IsParentMinimized

        ' Raises the AfterResize event
        RaiseEvent AfterResize(Me, New EventArgs.AfterResizeEventArgs With {.DisplayState = DisplayState})

    End Sub

#End Region

#Region "Release of COM objects"
    Private Sub LogRefEdit(msg As String)
        Try
            AppInfrastructure.CoreServices.Log("RefEdit: " & msg)
        Catch
        End Try
    End Sub

    ''' <summary>
    ''' Safe check for Application.EnableEvents; logs and returns False on any error.
    ''' </summary>
    Private Function IsEnableEventsEnabled() As Boolean
        Try
            If _ExcelConnector Is Nothing Then
                Return False
            End If
            Return _ExcelConnector.EnableEvents
        Catch ex As Exception
            LogRefEdit($"IsEnableEventsEnabled error: {ex}")
            Return False
        End Try
    End Function

    ''' <summary>
    ''' Releases a COM object.
    ''' </summary>
    ''' <param name="ComObj">The COM object to be released and disposed.</param>
    ''' <remarks></remarks>
    Private Sub _NAR(ByVal ComObj As Object)
        If ComObj Is Nothing Then Return
        Try
            Marshal.ReleaseComObject(ComObj)
        Catch ex As Runtime.InteropServices.COMException
            LogRefEdit($"_NAR COMException: {ex}")
        Finally
            ComObj = Nothing
        End Try
    End Sub

    ''' <summary>
    ''' When the control is disposed, make sure all the COM Objects are disposed of and the handlers removed.
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    ''' <remarks></remarks>
    Private Sub Excel2007RefEdit_Disposed(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Disposed
        pbInTextAdressHandler = False
        captureFaulted = True
        Try
            CloseCaptureLeases()
        Finally
            xlSheet = Nothing
            xlBook = Nothing
            xlApp = Nothing
            _ExcelConnector = Nothing
            DisplayState = Nothing
        End Try
    End Sub

#End Region

End Class

Namespace EventArgs
    ''' <summary>
    ''' Event Args related to the AfterResize Event.
    ''' </summary>
    ''' <remarks></remarks>
    Public Class AfterResizeEventArgs
        Inherits System.EventArgs

        Public Property DisplayState As RefEditState
    End Class

    ''' <summary>
    ''' Event Args related to the BeforeResize Event.
    ''' </summary>
    ''' <remarks></remarks>
    Public Class BeforeResizeEventArgs
        Inherits System.ComponentModel.CancelEventArgs

        Public Property DisplayState As RefEditState
    End Class
End Namespace
