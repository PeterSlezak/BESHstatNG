Option Explicit On
Imports System.Reflection
Imports System.Linq
Imports System.Windows.Forms
Imports ExcelDna.Integration
Imports Microsoft.Office.Interop.Excel

''' <summary>
''' Global variables, constants, logging utilities, and help‑file integration
''' used throughout the BESH‑Stat‑NG Excel add‑in.  
''' Provides:
''' <list type="bullet">
'''   <item><description>Application‑wide constants (title, version, build)</description></item>
'''   <item><description>Paths and filenames for the add‑in and log files</description></item>
'''   <item><description>Global Excel application reference</description></item>
'''   <item><description>Help‑topic identifiers for UI components</description></item>
'''   <item><description>Error logging and exception‑handling helpers</description></item>
'''   <item><description>Trace/debug/warning logging utilities</description></item>
''' </list>
''' This module centralizes shared state and infrastructure for the add‑in.
''' </summary>
Public Module BESHstatGlobals

    ''' <summary>
    ''' Application title displayed in UI dialogs and message boxes.
    ''' </summary>
    Public Const gsAPP_TITLE As String = "BESH-Stat-NG"

    ''' <summary>
    ''' Name of the loaded XLL add‑in file.
    ''' </summary>
    Public gXllName As String

    ''' <summary>
    ''' Full path to the loaded XLL add‑in file.
    ''' </summary>
    Public gXllPath As String

    ''' <summary>
    ''' Global NLog logger instance used throughout the add‑in.
    ''' </summary>
    Public gLogger As NLog.Logger

    ''' <summary>
    ''' Full path to the active log file.
    ''' </summary>
    Public gLogFile As String

    ''' <summary>
    ''' Enables or disables trace‑level logging.
    ''' </summary>
    Public gbTraceLogging As Boolean = True

    ''' <summary>
    ''' Add‑in version number displayed in UI and used for update checks.
    ''' </summary>
    Public ReadOnly Property gAddinVersion As String
        'Public Const gAddinVersion As String = "0.0.3.0"
        Get
            Dim v = Assembly.GetExecutingAssembly().GetName().Version
            Return v.ToString() ' x.x.x.x
        End Get
    End Property

    ''' <summary>
    ''' Add‑in build identifier ( e.g. "2026-01-07" format).
    ''' </summary>
    Public Function GetBuildDateIso() As String
        Dim asm = Assembly.GetExecutingAssembly()

        Dim meta = asm.GetCustomAttributes(GetType(AssemblyMetadataAttribute), False) _
                      .OfType(Of AssemblyMetadataAttribute)() _
                      .FirstOrDefault(Function(a) a.Key = "BuildDate")

        Return If(meta?.Value, "")
    End Function


    ''' <summary>
    ''' URL used to check for new versions of the add‑in.
    ''' </summary>
    Public Const gCheckUpdateUrl As String = "http://beshstat.eu/wp-content/uploads/addinversion/versionNG.xml"

    ''' <summary>
    ''' Global reference to the Excel application object.
    ''' </summary>
    Public app As Microsoft.Office.Interop.Excel.Application


    ''' <summary>
    ''' Provides centralized error logging and optional exception throwing.
    ''' </summary>
    Public Class BSerr

        ''' <summary>
        ''' Logs an exception using the global logger and optionally throws it.
        ''' </summary>
        ''' <param name="err">The exception to log.</param>
        ''' <param name="bThrow">If True, the exception is re‑thrown.</param>
        ''' <param name="bShowMsg">If True, displays a message box with the error text.</param>
        Public Shared Sub LogAndThrow(err As Exception, Optional bThrow As Boolean = True, Optional bShowMsg As Boolean = False)
            Try
                Debug.Print(err.Message)
                If gLogger IsNot Nothing Then gLogger.Error(err, err.Message)
                If bShowMsg Then MsgBox("An error occured: " & err.Message & " Check log for more information.")
            Catch
                ' Swallow logging failures (e.g., gLogger not initialized in unit tests)
            End Try

            If bThrow Then Throw err
        End Sub
    End Class

    ''' <summary>
    ''' Specifies the severity level for logging messages.
    ''' </summary>
    Public Enum LogMsgType
        ''' <summary>Warning message.</summary>
        Warn = 1
        ''' <summary>Debug‑level message.</summary>
        Debug = 2
        ''' <summary>Trace‑level message.</summary>
        Trace = 3
    End Enum


    ''' <summary>
    ''' Provides lightweight logging utilities for warnings, debug messages,
    ''' and trace‑level diagnostics.
    ''' </summary>
    Public Class BSlogg

        ''' <summary>
        ''' Writes a log message using the global logger, respecting the selected
        ''' message type and trace‑logging settings.
        ''' </summary>
        ''' <param name="txt">The message text.</param>
        ''' <param name="msgType">The severity level (default = Trace).</param>
        Public Shared Sub Log(txt As String, Optional msgType As LogMsgType = LogMsgType.Trace)
            Try
                If gLogger IsNot Nothing Then
                    If msgType = LogMsgType.Warn Then
                        gLogger.Warn(txt)
                    ElseIf msgType = LogMsgType.Debug Then
                        gLogger.Debug(txt)
                    ElseIf msgType = LogMsgType.Trace Then
                        If BESHStatNG.gbTraceLogging Then gLogger.Trace(txt)
                    End If
                End If
            Catch
                ' Swallow logging failures (e.g., gLogger not initialized in unit tests)
            End Try
        End Sub
    End Class

    Public ReadOnly Property ExcelMainHwnd As IntPtr
        Get
            Try
                Dim h = ExcelDnaUtil.WindowHandle
                If h <> IntPtr.Zero Then Return h
            Catch
            End Try

            Try
                If app IsNot Nothing Then Return New IntPtr(BESHstatGlobals.app.Hwnd)
            Catch
            End Try

            Return IntPtr.Zero
        End Get
    End Property

End Module
