Option Explicit On
Option Strict On

Imports ExcelDna.Integration
Imports Excel = Microsoft.Office.Interop.Excel
Imports Forms = System.Windows.Forms

Namespace AppInfrastructure

    ''' <summary>
    ''' Excel-DNA/WinForms host services. Keep this module out of future portable core projects.
    ''' </summary>
    Public Module ExcelDnaHost
        Public Property ExcelApplication As Excel.Application
        Public Property XllName As String
        Public Property XllPath As String

        Public ReadOnly Property ExcelMainHwnd As IntPtr
            Get
                Try
                    Dim h = ExcelDnaUtil.WindowHandle
                    If h <> IntPtr.Zero Then Return h
                Catch
                End Try

                Try
                    If ExcelApplication IsNot Nothing Then Return New IntPtr(ExcelApplication.Hwnd)
                Catch
                End Try

                Return IntPtr.Zero
            End Get
        End Property

        Public Sub ConfigureCoreServicesForExcelDna(Optional applicationTitle As String = "BESH-Stat-NG")
            CoreServices.UserMessages = New WinFormsUserMessageService(applicationTitle)
            CoreServices.Regression.MessagePump = New WinFormsUiMessagePump()
        End Sub
    End Module

    ''' <summary>
    ''' WinForms implementation of the message pump used while polling cancellation/interruption.
    ''' </summary>
    Public NotInheritable Class WinFormsUiMessagePump
        Implements IUiMessagePump

        Public Sub Pump() Implements IUiMessagePump.Pump
            Forms.Application.DoEvents()
        End Sub
    End Class

    ''' <summary>
    ''' WinForms progress reporter used by Excel-DNA UI code.
    ''' Statistical/core code should depend only on <see cref="IProgressReporter"/>.
    ''' </summary>
    Public NotInheritable Class WinFormsProgressReporter
        Implements IProgressReporter

        Private ReadOnly _progressBar As Forms.ProgressBar
        Private ReadOnly _label As Forms.Label

        Public Sub New(progressBar As Forms.ProgressBar, Optional progressLabel As Forms.Label = Nothing)
            _progressBar = progressBar
            _label = progressLabel
        End Sub

        Public Sub Report(percent As Integer, Optional message As String = Nothing) Implements IProgressReporter.Report
            Dim target As Forms.Control = Nothing
            If _progressBar IsNot Nothing Then
                target = _progressBar
            ElseIf _label IsNot Nothing Then
                target = _label
            End If
            If target Is Nothing Then Return

            Dim boundedPercent As Integer = Math.Max(0, Math.Min(100, percent))
            Dim update As Action = Sub()
                                       If _progressBar IsNot Nothing Then _progressBar.Value = boundedPercent
                                       If _label IsNot Nothing AndAlso message IsNot Nothing Then _label.Text = message
                                   End Sub

            If target.InvokeRequired Then
                target.Invoke(update)
            Else
                update()
            End If

            Forms.Application.DoEvents()
        End Sub
    End Class

    ''' <summary>
    ''' WinForms implementation for user-visible error messages in the Excel add-in host.
    ''' </summary>
    Public NotInheritable Class WinFormsUserMessageService
        Implements IUserMessageService

        Private ReadOnly _applicationTitle As String

        Public Sub New(Optional applicationTitle As String = "BESH-Stat-NG")
            _applicationTitle = If(String.IsNullOrWhiteSpace(applicationTitle), "BESH-Stat-NG", applicationTitle)
        End Sub

        Public Sub ShowError(message As String, title As String) Implements IUserMessageService.ShowError
            Dim caption = If(String.IsNullOrWhiteSpace(title), _applicationTitle, title)
            MsgBox(message, MsgBoxStyle.Exclamation, caption)
        End Sub
    End Class

End Namespace
