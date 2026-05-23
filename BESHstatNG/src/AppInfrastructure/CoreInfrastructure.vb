Option Explicit On
Option Strict On

Imports System.Diagnostics
Imports System.IO
Imports System.Runtime.ExceptionServices
Imports System.Text
Imports System.Threading

Namespace AppInfrastructure

    ''' <summary>
    ''' Specifies the severity level for logging messages in host-neutral code.
    ''' </summary>
    Public Enum LogMsgType
        Warn = 1
        Debug = 2
        Trace = 3
    End Enum

    ''' <summary>
    ''' Minimal logger abstraction used by statistical/core code and host adapters.
    ''' Implementations may write to a file, diagnostics trace, a server log, or nowhere.
    ''' </summary>
    Public Interface IAppLogger
        Inherits IDisposable

        Sub Info(message As String)
        Sub Warn(message As String)
        Sub Debug(message As String)
        Sub Trace(message As String)
        Sub WriteError(message As String)
        Sub WriteError(ex As Exception, message As String)
        Sub [Error](message As String)
        Sub [Error](ex As Exception, Optional message As String = Nothing)
    End Interface

    ''' <summary>
    ''' Logger used by default in non-Excel hosts and tests. It intentionally does nothing.
    ''' </summary>
    Public NotInheritable Class NullLogger
        Implements IAppLogger

        Public Sub Info(message As String) Implements IAppLogger.Info
        End Sub

        Public Sub Warn(message As String) Implements IAppLogger.Warn
        End Sub

        Public Sub Debug(message As String) Implements IAppLogger.Debug
        End Sub

        Public Sub Trace(message As String) Implements IAppLogger.Trace
        End Sub

        Public Sub WriteError(message As String) Implements IAppLogger.WriteError
        End Sub

        Public Sub WriteError(ex As Exception, message As String) Implements IAppLogger.WriteError
        End Sub

        Public Sub [Error](message As String) Implements IAppLogger.Error
        End Sub

        Public Sub [Error](ex As Exception, Optional message As String = Nothing) Implements IAppLogger.Error
        End Sub

        Public Sub Dispose() Implements IDisposable.Dispose
        End Sub
    End Class

    Public Interface IUserMessageService
        Sub ShowError(message As String, title As String)
    End Interface

    Public NotInheritable Class NullUserMessageService
        Implements IUserMessageService

        Public Sub ShowError(message As String, title As String) Implements IUserMessageService.ShowError
        End Sub
    End Class

    Public Interface IErrorReporter
        Sub LogAndThrow(err As Exception,
                        Optional bThrow As Boolean = True,
                        Optional bShowMsg As Boolean = False,
                        Optional context As String = Nothing)
    End Interface

    Public Class DefaultErrorReporter
        Implements IErrorReporter

        Private ReadOnly _logger As IAppLogger
        Private ReadOnly _messages As IUserMessageService
        Private ReadOnly _applicationTitle As String

        Public Sub New(logger As IAppLogger,
                       Optional messages As IUserMessageService = Nothing,
                       Optional applicationTitle As String = "BESH-Stat-NG")
            _logger = If(logger, New NullLogger())
            _messages = If(messages, New NullUserMessageService())
            _applicationTitle = If(String.IsNullOrWhiteSpace(applicationTitle), "BESH-Stat-NG", applicationTitle)
        End Sub

        Public Sub LogAndThrow(err As Exception,
                               Optional bThrow As Boolean = True,
                               Optional bShowMsg As Boolean = False,
                               Optional context As String = Nothing) Implements IErrorReporter.LogAndThrow
            If err Is Nothing Then err = New Exception("Unknown error.")

            Dim logMessage As String = If(String.IsNullOrWhiteSpace(context), err.Message, context)
            Dim uiMessage As String = If(String.IsNullOrWhiteSpace(context), err.Message, context & ": " & err.Message)

            Try
                System.Diagnostics.Debug.Print(err.ToString())
                _logger.Error(err, logMessage)

                If bShowMsg Then
                    _messages.ShowError("An error occurred: " & uiMessage & vbCrLf & "Check log for more information.",
                                        _applicationTitle)
                End If
            Catch logEx As Exception
                System.Diagnostics.Debug.Print("Logging failure: " & logEx.ToString())
            End Try

            If bThrow Then ExceptionDispatchInfo.Capture(err).Throw()
        End Sub
    End Class

    Public Interface IProgressReporter
        Sub Report(percent As Integer, Optional message As String = Nothing)
    End Interface

    Public NotInheritable Class NullProgressReporter
        Implements IProgressReporter

        Public Sub Report(percent As Integer, Optional message As String = Nothing) Implements IProgressReporter.Report
        End Sub
    End Class

    ''' <summary>
    ''' Host-neutral defaults used by statistical/core code when a procedure does not receive
    ''' an explicit alpha or random seed. Excel-DNA configures this object from the global
    ''' settings UI; server/test hosts can replace it with request-specific or host-specific
    ''' defaults without referencing AppGlobals.
    ''' </summary>
    Public Class AnalysisDefaultsOptions
        Public Const NoExplicitRandomSeed As Integer = Integer.MinValue

        Private _defaultAlpha As Double = 0.05
        Private _defaultRandomSeed As Integer = NoExplicitRandomSeed

        Public Sub New()
        End Sub

        Public Sub New(Optional defaultAlpha As Double = 0.05,
                       Optional defaultRandomSeed As Integer = NoExplicitRandomSeed)
            Me.DefaultAlpha = defaultAlpha
            Me.DefaultRandomSeed = defaultRandomSeed
        End Sub

        ''' <summary>
        ''' Gets or sets the global fallback significance level. Invalid values are normalized to 0.05.
        ''' </summary>
        Public Property DefaultAlpha As Double
            Get
                Return _defaultAlpha
            End Get
            Set(value As Double)
                If Double.IsNaN(value) OrElse Double.IsInfinity(value) OrElse value <= 0.0 OrElse value >= 1.0 Then
                    _defaultAlpha = 0.05
                Else
                    _defaultAlpha = value
                End If
            End Set
        End Property

        ''' <summary>
        ''' Gets or sets the global fallback random seed. <see cref="Integer.MinValue"/> means no global seed.
        ''' </summary>
        Public Property DefaultRandomSeed As Integer
            Get
                Return _defaultRandomSeed
            End Get
            Set(value As Integer)
                If value = 0 Then
                    _defaultRandomSeed = NoExplicitRandomSeed
                Else
                    _defaultRandomSeed = value
                End If
            End Set
        End Property

        ''' <summary>
        ''' Resolves an explicit alpha if supplied; otherwise returns the configured global default alpha.
        ''' </summary>
        Public Function ResolveAlpha(Optional requestedAlpha As Double = Double.NaN) As Double
            If Not Double.IsNaN(requestedAlpha) Then
                ValidateAlpha(requestedAlpha, NameOf(requestedAlpha))
                Return requestedAlpha
            End If

            ValidateAlpha(DefaultAlpha, NameOf(DefaultAlpha))
            Return DefaultAlpha
        End Function

        ''' <summary>
        ''' Resolves a random seed. When no explicit or global seed exists and
        ''' <paramref name="generateWhenMissing"/> is true, a time-based seed is returned.
        ''' </summary>
        Public Function ResolveRandomSeed(Optional requestedSeed As Integer = NoExplicitRandomSeed,
                                          Optional generateWhenMissing As Boolean = True) As Integer
            If requestedSeed <> NoExplicitRandomSeed Then Return requestedSeed
            If DefaultRandomSeed <> NoExplicitRandomSeed Then Return DefaultRandomSeed
            If generateWhenMissing Then Return Environment.TickCount
            Return NoExplicitRandomSeed
        End Function

        ''' <summary>
        ''' Creates a <see cref="Random"/> using the explicit seed, the configured global seed,
        ''' or a runtime-generated seed when neither is set.
        ''' </summary>
        Public Function CreateRandom(Optional explicitSeed As Integer = NoExplicitRandomSeed) As Random
            If explicitSeed <> NoExplicitRandomSeed Then Return New Random(explicitSeed)
            If DefaultRandomSeed <> NoExplicitRandomSeed Then Return New Random(DefaultRandomSeed)
            Return New Random()
        End Function

        ''' <summary>
        ''' Creates a <see cref="Random"/> and returns the concrete seed used to initialize it.
        ''' </summary>
        Public Function CreateRandomWithResolvedSeed(Optional requestedSeed As Integer = NoExplicitRandomSeed) As (Rng As Random, SeedUsed As Integer)
            Dim seed = ResolveRandomSeed(requestedSeed, generateWhenMissing:=True)
            Return (New Random(seed), seed)
        End Function

        Private Shared Sub ValidateAlpha(value As Double, paramName As String)
            If Double.IsNaN(value) OrElse Double.IsInfinity(value) OrElse value <= 0.0 OrElse value >= 1.0 Then
                Throw New ArgumentOutOfRangeException(paramName, "Alpha must be between 0 and 1.")
            End If
        End Sub
    End Class

    Public Interface IUiMessagePump
        Sub Pump()
    End Interface

    Public NotInheritable Class NoOpUiMessagePump
        Implements IUiMessagePump

        Public Sub Pump() Implements IUiMessagePump.Pump
        End Sub
    End Class

    ''' <summary>
    ''' Coordinates cancellation, interruption, and UI message pumping for the current host.
    ''' This coordinator is intentionally global because the Excel-DNA front end runs one
    ''' long-running regression workflow at a time. Server or API hosts that may run
    ''' concurrent jobs should prefer per-request callbacks, CancellationToken values,
    ''' or model-specific request objects such as MixedModelFitRequest instead of sharing
    ''' this singleton coordinator across jobs.
    ''' </summary>
    Public Class RegressionComputationCoordinator
        Private _messagePump As IUiMessagePump = New NoOpUiMessagePump()

        Public Property CancellationRequested As Func(Of Boolean) = Nothing
        Public Property InterruptionRequested As Func(Of Boolean) = Nothing

        Public Property MessagePump As IUiMessagePump
            Get
                Return _messagePump
            End Get
            Set(value As IUiMessagePump)
                _messagePump = If(value, New NoOpUiMessagePump())
            End Set
        End Property

        Public Sub SetCallbacks(cancelRequested As Func(Of Boolean),
                                interruptRequested As Func(Of Boolean))
            CancellationRequested = cancelRequested
            InterruptionRequested = interruptRequested
        End Sub

        Public Sub ClearCallbacks()
            CancellationRequested = Nothing
            InterruptionRequested = Nothing
        End Sub

        Public Function IsCancellationRequested() As Boolean
            If CancellationRequested Is Nothing Then Return False

            Try
                Return CancellationRequested.Invoke()
            Catch
                Return False
            End Try
        End Function

        Public Function IsInterruptionRequested() As Boolean
            If InterruptionRequested Is Nothing Then Return False

            Try
                Return InterruptionRequested.Invoke()
            Catch
                Return False
            End Try
        End Function

        Public Sub ThrowIfCancellationRequested(Optional message As String = "Calculation cancelled by user.")
            PumpMessages()
            If IsCancellationRequested() Then Throw New OperationCanceledException(message)
        End Sub

        Public Sub PumpMessages()
            If CancellationRequested Is Nothing AndAlso InterruptionRequested Is Nothing Then Return

            Try
                MessagePump.Pump()
            Catch
            End Try
        End Sub
    End Class

    Public Module CoreServices
        Private _logger As IAppLogger = New NullLogger()
        Private _messages As IUserMessageService = New NullUserMessageService()
        Private _errors As IErrorReporter = Nothing
        Private _analysisDefaults As AnalysisDefaultsOptions = New AnalysisDefaultsOptions()
        Private ReadOnly _regression As New RegressionComputationCoordinator()

        Public Property TraceExecutionLoggingEnabled As Boolean = True

        Public Property Logger As IAppLogger
            Get
                Return _logger
            End Get
            Set(value As IAppLogger)
                _logger = If(value, New NullLogger())
                If _errors Is Nothing OrElse TypeOf _errors Is DefaultErrorReporter Then
                    _errors = New DefaultErrorReporter(_logger, _messages)
                End If
            End Set
        End Property

        Public Property UserMessages As IUserMessageService
            Get
                Return _messages
            End Get
            Set(value As IUserMessageService)
                _messages = If(value, New NullUserMessageService())
                If _errors Is Nothing OrElse TypeOf _errors Is DefaultErrorReporter Then
                    _errors = New DefaultErrorReporter(_logger, _messages)
                End If
            End Set
        End Property

        Public Property Errors As IErrorReporter
            Get
                If _errors Is Nothing Then _errors = New DefaultErrorReporter(_logger, _messages)
                Return _errors
            End Get
            Set(value As IErrorReporter)
                _errors = If(value, New DefaultErrorReporter(_logger, _messages))
            End Set
        End Property

        Public Property AnalysisDefaults As AnalysisDefaultsOptions
            Get
                Return _analysisDefaults
            End Get
            Set(value As AnalysisDefaultsOptions)
                _analysisDefaults = If(value, New AnalysisDefaultsOptions())
            End Set
        End Property

        Public ReadOnly Property Regression As RegressionComputationCoordinator
            Get
                Return _regression
            End Get
        End Property

        Public Sub Log(message As String, Optional msgType As LogMsgType = LogMsgType.Trace)
            Select Case msgType
                Case LogMsgType.Warn
                    Logger.Warn(message)
                Case LogMsgType.Debug
                    Logger.Debug(message)
                Case Else
                    Logger.Trace(message)
            End Select
        End Sub
    End Module

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
            If Not CoreServices.TraceExecutionLoggingEnabled Then Return
            WriteEntry("DEBUG", message, False, Nothing, False, _allWriter)
        End Sub

        Public Sub Trace(message As String) Implements IAppLogger.Trace
            If Not CoreServices.TraceExecutionLoggingEnabled Then Return
            WriteEntry("TRACE", message, False, Nothing, False, _allWriter)
        End Sub

        Public Sub WriteError(message As String) Implements IAppLogger.WriteError
            WriteEntry("ERROR", message, True, Nothing, True, _allWriter, _warnWriter, _errorWriter)
        End Sub

        Public Sub WriteError(ex As Exception, message As String) Implements IAppLogger.WriteError
            Dim finalMessage = If(String.IsNullOrWhiteSpace(message), If(ex Is Nothing, "", ex.Message), message)
            WriteEntry("ERROR", finalMessage, True, ex, True, _allWriter, _warnWriter, _errorWriter)
        End Sub

        Public Sub [Error](message As String) Implements IAppLogger.Error
            WriteError(message)
        End Sub

        Public Sub [Error](ex As Exception, Optional message As String = Nothing) Implements IAppLogger.Error
            If ex Is Nothing Then
                WriteError(If(String.IsNullOrWhiteSpace(message), "Unknown error.", message))
            Else
                WriteError(ex, If(String.IsNullOrWhiteSpace(message), ex.Message, message))
            End If
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

                If flushNow Then FlushTargetsUnsafe(targets)
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
            If String.IsNullOrEmpty(declaringTypeName) Then Return False
            If declaringTypeName.StartsWith("System.", StringComparison.Ordinal) Then Return True
            If declaringTypeName.StartsWith("Microsoft.VisualBasic.", StringComparison.Ordinal) Then Return True
            If declaringTypeName.Contains("AppInfrastructure.SimpleFileLogger") Then Return True
            If declaringTypeName.Contains("AppInfrastructure.CoreServices") Then Return True
            If declaringTypeName.Contains("AppInfrastructure.DefaultErrorReporter") Then Return True
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

End Namespace
