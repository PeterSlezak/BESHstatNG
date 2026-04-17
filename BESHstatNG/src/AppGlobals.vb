Option Explicit On
Option Strict On
Imports System.IO
Imports System.Linq
Imports System.Reflection
Imports System.Runtime.ExceptionServices
Imports System.Text
Imports System.Threading
Imports System.Windows.Forms
Imports ExcelDna.Integration
Imports Microsoft.Office.Interop.Excel


Namespace AppInfrastructure

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
    Public Module AppGlobals

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
        ''' Global settings store used to load/save user settings.
        ''' </summary>
        Public gSettingsStore As BeshStatNgSettingsStore

        ''' <summary>
        ''' In-memory settings object currently active for this session.
        ''' </summary>
        Public gSettings As BeshStatNgSettings

        Public ReadOnly Property ExcelMainHwnd As IntPtr
            Get
                Try
                    Dim h = ExcelDnaUtil.WindowHandle
                    If h <> IntPtr.Zero Then Return h
                Catch
                End Try

                Try
                    If app IsNot Nothing Then Return New IntPtr(AppGlobals.app.Hwnd)
                Catch
                End Try

                Return IntPtr.Zero
            End Get
        End Property

        ''' <summary>
        ''' Global application logger instance used throughout the add-in.
        ''' </summary>
        Public gLogger As IAppLogger

        ''' <summary>
        ''' Minimal logger abstraction used by the application.
        ''' </summary>
        Public Interface IAppLogger
            Inherits IDisposable

            Sub Info(message As String)
            Sub Warn(message As String)
            Sub Debug(message As String)
            Sub Trace(message As String)
            Sub WriteError(message As String)
            Sub WriteError(ex As Exception, message As String)
        End Interface

        ''' <summary>
        ''' Simple thread-safe file logger output line format:
        ''' yyyy-MM-dd HH:mm:ss.ffff|LEVEL|LoggerName|Message
        '''
        ''' Routing:
        ''' - Trace/Debug/Info -> all.log
        ''' - Warn            -> all.log + errwarn.log
        ''' - Error           -> all.log + errwarn.log + err.log
        '''
        ''' Warning and error entries automatically include the current call stack.
        ''' Exception-based errors also include Exception.ToString().
        ''' </summary>
        Public Class SimpleFileLogger
            Implements IAppLogger

            Private ReadOnly _syncRoot As New Object()
            Private ReadOnly _loggerName As String
            Private ReadOnly _allWriter As StreamWriter
            Private ReadOnly _warnWriter As StreamWriter
            Private ReadOnly _errorWriter As StreamWriter
            Private ReadOnly _flushTimer As System.Threading.Timer

            Private _disposed As Boolean

            Private Const FlushIntervalMs As Integer = 1000
            Private Const MaxStackFrames As Integer = 8

            Public Sub New(baseDirectory As String,
                           loggerName As String,
                           Optional resetTraceLog As Boolean = True)

                If String.IsNullOrWhiteSpace(baseDirectory) Then
                    Throw New ArgumentException("baseDirectory must not be empty.", NameOf(baseDirectory))
                End If

                _loggerName = If(String.IsNullOrWhiteSpace(loggerName), "BESHStatNG.BESHStatAddIn", loggerName)

                Dim logDirectory = Path.Combine(baseDirectory, "Logs")
                Directory.CreateDirectory(logDirectory)

                _allWriter = CreateWriter(Path.Combine(logDirectory, "all.log"), If(resetTraceLog, FileMode.Create, FileMode.Append))
                _warnWriter = CreateWriter(Path.Combine(logDirectory, "errwarn.log"), FileMode.Append)
                _errorWriter = CreateWriter(Path.Combine(logDirectory, "err.log"), FileMode.Append)

                _flushTimer = New System.Threading.Timer(AddressOf FlushTimerCallback, Nothing, FlushIntervalMs, FlushIntervalMs)
            End Sub

            Private Shared Function CreateWriter(path As String, mode As FileMode) As StreamWriter
                Dim fs As New FileStream(path, mode, FileAccess.Write, FileShare.ReadWrite)
                If mode = FileMode.Append Then fs.Seek(0, SeekOrigin.End)

                Dim writer As New StreamWriter(fs, New UTF8Encoding(False))
                writer.AutoFlush = False
                Return writer
            End Function

            Public Sub Info(message As String) Implements IAppLogger.Info
                WriteEntry("INFO", message, False, Nothing, False, _allWriter)
            End Sub

            Public Sub Warn(message As String) Implements IAppLogger.Warn
                WriteEntry("WARN", message, True, Nothing, True, _allWriter, _warnWriter)
            End Sub

            Public Sub Debug(message As String) Implements IAppLogger.Debug
                WriteEntry("DEBUG", message, False, Nothing, False, _allWriter)
            End Sub

            Public Sub Trace(message As String) Implements IAppLogger.Trace
                WriteEntry("TRACE", message, False, Nothing, False, _allWriter)
            End Sub

            Public Sub WriteError(message As String) Implements IAppLogger.WriteError
                WriteEntry("ERROR", message, True, Nothing, True, _allWriter, _warnWriter, _errorWriter)
            End Sub

            Public Sub WriteError(ex As Exception, message As String) Implements IAppLogger.WriteError
                Dim finalMessage = If(String.IsNullOrWhiteSpace(message), If(ex Is Nothing, "", ex.Message), message)
                WriteEntry("ERROR", finalMessage, True, ex, True, _allWriter, _warnWriter, _errorWriter)
            End Sub

            Private Sub WriteEntry(level As String,
                                   message As String,
                                   includeCallStack As Boolean,
                                   ex As Exception,
                                   flushNow As Boolean,
                                   ParamArray targets() As StreamWriter)

                Dim sb As New StringBuilder(256)
                sb.Append(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.ffff", Globalization.CultureInfo.InvariantCulture))
                sb.Append("|")
                sb.Append(level)
                sb.Append("|")
                sb.Append(_loggerName)
                sb.Append("|")
                sb.Append(If(message, ""))

                If ex IsNot Nothing Then
                    sb.AppendLine()
                    sb.AppendLine("Exception:")
                    sb.Append(ex.ToString())
                End If

                If includeCallStack Then
                    Dim stackText = CaptureRelevantCallStack()
                    If Not String.IsNullOrWhiteSpace(stackText) Then
                        sb.AppendLine()
                        sb.AppendLine("CallStack:")
                        sb.Append(stackText)
                    End If
                End If

                Dim text = sb.ToString()

                SyncLock _syncRoot
                    If _disposed Then Return

                    For Each target In targets
                        If target Is Nothing Then Continue For
                        target.WriteLine(text)
                    Next

                    If flushNow Then
                        FlushTargetsUnsafe(targets)
                    End If
                End SyncLock
            End Sub

            Private Sub FlushTimerCallback(state As Object)
                SyncLock _syncRoot
                    If _disposed Then Return
                    FlushTargetsUnsafe(_allWriter, _warnWriter, _errorWriter)
                End SyncLock
            End Sub

            Private Shared Sub FlushTargetsUnsafe(ParamArray targets() As StreamWriter)
                For Each target In targets
                    If target Is Nothing Then Continue For
                    target.Flush()
                Next
            End Sub

            Private Function CaptureRelevantCallStack() As String
                Dim st As New StackTrace(False)
                Dim frames = st.GetFrames()
                If frames Is Nothing OrElse frames.Length = 0 Then Return String.Empty

                Dim sb As New StringBuilder()
                Dim written As Integer = 0

                For Each frame In frames
                    Dim method = frame.GetMethod()
                    If method Is Nothing Then Continue For

                    Dim declaringType = method.DeclaringType
                    Dim declaringTypeName = If(declaringType Is Nothing, "", declaringType.FullName)

                    If ShouldSkipFrame(declaringTypeName) Then Continue For

                    sb.Append("   at ")
                    If Not String.IsNullOrWhiteSpace(declaringTypeName) Then
                        sb.Append(declaringTypeName)
                        sb.Append(".")
                    End If
                    sb.Append(method.Name)
                    sb.Append("()")
                    sb.AppendLine()

                    written += 1
                    If written >= MaxStackFrames Then Exit For
                Next

                Return sb.ToString().TrimEnd()
            End Function

            Private Shared Function ShouldSkipFrame(declaringTypeName As String) As Boolean
                If String.IsNullOrWhiteSpace(declaringTypeName) Then Return False

                If declaringTypeName.StartsWith("System.", StringComparison.Ordinal) Then Return True
                If declaringTypeName.StartsWith("Microsoft.VisualBasic.", StringComparison.Ordinal) Then Return True
                If declaringTypeName.Contains("AppInfrastructure.AppGlobals+SimpleFileLogger") Then Return True
                If declaringTypeName.Contains("AppInfrastructure.AppGlobals+BSlogg") Then Return True
                If declaringTypeName.Contains("AppInfrastructure.AppGlobals+BSerr") Then Return True

                Return False
            End Function

            Public Sub Dispose() Implements IDisposable.Dispose
                SyncLock _syncRoot
                    If _disposed Then Return
                    _disposed = True

                    If _flushTimer IsNot Nothing Then _flushTimer.Dispose()

                    FlushTargetsUnsafe(_allWriter, _warnWriter, _errorWriter)

                    If _allWriter IsNot Nothing Then _allWriter.Dispose()
                    If _warnWriter IsNot Nothing Then _warnWriter.Dispose()
                    If _errorWriter IsNot Nothing Then _errorWriter.Dispose()
                End SyncLock
            End Sub
        End Class

        ''' <summary>
        ''' Full path to the active log file.
        ''' </summary>
        Public gLogFile As String

        Public Function GetCurrentSettings() As BeshStatNgSettings
            If gSettings Is Nothing Then
                gSettings = New BeshStatNgSettings()
                gSettings.EnsureDefaults()
            End If

            Return gSettings
        End Function

        Public ReadOnly Property TraceExecutionLoggingEnabled As Boolean
            Get
                Return GetCurrentSettings().Diagnostics.TraceExecutionLoggingEnabled
            End Get
        End Property

        ''' <summary>
        ''' Gets the session default two-sided alpha level.
        ''' Falls back to 0.05 if the settings file is missing or contains an invalid value.
        ''' </summary>
        Public ReadOnly Property DefaultAlpha As Double
            Get
                Dim alpha = GetCurrentSettings().DefaultAlpha
                If alpha <= 0.0 OrElse alpha >= 1.0 Then Return 0.05
                Return alpha
            End Get
        End Property

        ''' <summary>
        ''' Returns the configured default alpha clamped to the range accepted by a UI numeric control.
        ''' </summary>
        Public Function GetDefaultAlphaDecimal(minimum As Decimal, maximum As Decimal) As Decimal
            Dim value As Decimal = CDec(DefaultAlpha)
            If value < minimum Then value = minimum
            If value > maximum Then value = maximum
            Return value
        End Function

        ''' <summary>
        ''' Gets the session default pseudo-random seed.
        ''' The sentinel <see cref="Integer.MinValue"/> means that no deterministic default seed is configured.
        ''' </summary>
        Public ReadOnly Property DefaultRandomSeed As Integer
            Get
                Return GetCurrentSettings().DefaultRandomSeed
            End Get
        End Property

        ''' <summary>
        ''' Returns the configured default seed as text for UI seed editors.
        ''' Returns an empty string when no deterministic default seed is configured.
        ''' </summary>
        Public Function GetDefaultRandomSeedText() As String
            Dim seed = DefaultRandomSeed
            If seed = Integer.MinValue Then Return String.Empty
            Return seed.ToString(Globalization.CultureInfo.InvariantCulture)
        End Function

        ''' <summary>
        ''' Creates a pseudo-random generator using an explicit seed if supplied, otherwise the configured global default seed.
        ''' If neither is available, a time-based seed is used.
        ''' </summary>
        Public Function CreateRandom(Optional explicitSeed As Integer = Integer.MinValue) As Random
            Dim seed = explicitSeed
            If seed = Integer.MinValue Then seed = DefaultRandomSeed
            If seed = Integer.MinValue Then Return New Random()
            Return New Random(seed)
        End Function

        Public Sub ApplySettings(settings As BeshStatNgSettings)
            If settings Is Nothing Then
                settings = New BeshStatNgSettings()
            End If

            settings.EnsureDefaults()
            gSettings = settings
        End Sub

        ''' <summary>
        ''' Provides centralized error logging and optional exception throwing.
        ''' </summary>
        Public Class BSerr

            ''' <summary>
            ''' Logs an exception using the global logger and optionally rethrows it while preserving the original stack trace.
            ''' </summary>
            ''' <param name="err">The exception to log.</param>
            ''' <param name="bThrow">If True, the exception is re-thrown.</param>
            ''' <param name="bShowMsg">If True, displays a message box with the error text.</param>
            ''' <param name="context">Optional contextual message to prefix the error.</param>
            Public Shared Sub LogAndThrow(err As Exception,
                                          Optional bThrow As Boolean = True,
                                          Optional bShowMsg As Boolean = False,
                                          Optional context As String = Nothing)
                If err Is Nothing Then err = New Exception("Unknown error.")
                Dim logMessage As String = If(String.IsNullOrWhiteSpace(context), err.Message, context)
                Dim uiMessage As String = If(String.IsNullOrWhiteSpace(context), err.Message, context & ": " & err.Message)

                Try
                    System.Diagnostics.Debug.Print(err.ToString())
                    BSlogg.Error(err, logMessage)

                    If bShowMsg Then
                        MsgBox("An error occurred: " & uiMessage & vbCrLf & "Check log for more information.",
                               MsgBoxStyle.Exclamation, gsAPP_TITLE)
                    End If
                Catch logEx As Exception
                    System.Diagnostics.Debug.Print("Logging failure: " & logEx.ToString())
                End Try

                If bThrow Then ExceptionDispatchInfo.Capture(err).Throw()
            End Sub
        End Class

        ''' <summary>
        ''' Specifies the severity level for logging messages.
        ''' </summary>
        Public Enum LogMsgType
            ''' <summary>Warning message.</summary>
            Warn = 1
            ''' <summary>Debug-level message.</summary>
            Debug = 2
            ''' <summary>Trace-level message.</summary>
            Trace = 3
        End Enum

        ''' <summary>
        ''' Provides lightweight logging utilities for warnings, debug messages,
        ''' trace diagnostics, informational messages, and simple error messages.
        ''' </summary>
        Public Class BSlogg

            Private Shared Sub SafeLog(act As System.Action)
                Try
                    act()
                Catch ex As Exception
                    System.Diagnostics.Debug.Print("Logging failure: " & ex.ToString())
                End Try
            End Sub

            Public Shared Sub Info(txt As String)
                SafeLog(Sub()
                            If gLogger IsNot Nothing Then gLogger.Info(txt)
                        End Sub)
            End Sub

            Public Shared Sub Warn(txt As String)
                SafeLog(Sub()
                            If gLogger IsNot Nothing Then gLogger.Warn(txt)
                        End Sub)
            End Sub

            Public Shared Sub Debug(txt As String)
                SafeLog(Sub()
                            If gLogger IsNot Nothing AndAlso AppGlobals.TraceExecutionLoggingEnabled Then gLogger.Debug(txt)
                        End Sub)
            End Sub

            Public Shared Sub Trace(txt As String)
                SafeLog(Sub()
                            If gLogger IsNot Nothing AndAlso AppGlobals.TraceExecutionLoggingEnabled Then gLogger.Trace(txt)
                        End Sub)
            End Sub

            Public Shared Sub [Error](txt As String)
                SafeLog(Sub()
                            If gLogger IsNot Nothing Then gLogger.WriteError(txt)
                        End Sub)
            End Sub

            Public Shared Sub [Error](ex As Exception, Optional txt As String = Nothing)
                SafeLog(Sub()
                            If gLogger Is Nothing Then Return

                            If ex Is Nothing Then
                                gLogger.WriteError(If(String.IsNullOrWhiteSpace(txt), "Unknown error.", txt))
                            Else
                                gLogger.WriteError(ex, If(String.IsNullOrWhiteSpace(txt), ex.Message, txt))
                            End If
                        End Sub)
            End Sub

            ''' <summary>
            ''' Writes a log message using the global logger, respecting the selected
            ''' message type and trace-logging settings.
            ''' </summary>
            ''' <param name="txt">The message text.</param>
            ''' <param name="msgType">The severity level (default = Trace).</param>
            Public Shared Sub Log(txt As String, Optional msgType As LogMsgType = LogMsgType.Trace)
                Select Case msgType
                    Case LogMsgType.Warn
                        Warn(txt)
                    Case LogMsgType.Debug
                        Debug(txt)
                    Case Else
                        Trace(txt)
                End Select
            End Sub
        End Class

    End Module
End Namespace