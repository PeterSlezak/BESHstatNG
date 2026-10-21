Option Explicit On
Option Strict On

Imports System.Threading.Tasks
Imports BESHStatNG.ExcelCommon

Namespace AppInfrastructure
    ''' <summary>Uses only string overloads of the existing logger. Never owns/disposes it.</summary>
    Public NotInheritable Class CoreLoggerDiagnosticSink
        Implements IDiagnosticSink
        Private ReadOnly _logger As IAppLogger
        Public Sub New(logger As IAppLogger)
            _logger = logger
        End Sub
        Public Function EmitAsync(record As DiagnosticRecord) As Task Implements IDiagnosticSink.EmitAsync
            If _logger Is Nothing Then Throw New ObjectDisposedException(NameOf(CoreLoggerDiagnosticSink))
            Dim line = record.EventCode & "|run=" & record.RunId & "|operation=" & record.Operation & "|stage=" & record.Stage
            For Each pair In record.Metadata
                line &= "|" & pair.Key & "=" & pair.Value
            Next
            Select Case record.Severity
                Case DiagnosticSeverity.Error : _logger.WriteError(line)
                Case DiagnosticSeverity.Warning : _logger.Warn(line)
                Case DiagnosticSeverity.Debug : _logger.Debug(line)
                Case Else : _logger.Info(line)
            End Select
            Return Task.CompletedTask
        End Function
    End Class

    ''' <summary>Synchronous Excel/COM-thread scope over the synchronous logger bridge.</summary>
    Friend NotInheritable Class WindowsDiagnosticRun
        Implements IDisposable
        Private ReadOnly _run As DiagnosticRunContext
        Private _stage As String = "validation"
        Private _failureReported As Boolean
        Private _failureCode As String = "diagnostic.run.failed"
        Friend Sub New(operation As String, trace As Boolean, sink As CoreLoggerDiagnosticSink)
            _run = New DiagnosticEmitter(sink).StartAsync(operation, trace).GetAwaiter().GetResult()
        End Sub
        Friend ReadOnly Property RunId As String
            Get
                Return _run.RunId
            End Get
        End Property
        Friend Sub EnterStage(stage As String)
            Select Case stage
                Case "validation", "acquisition", "analysis", "output", "invocation" : _stage = stage
                Case Else : _stage = "redacted"
            End Select
            _run.ReportAsync("diagnostic.stage.entered", _stage).GetAwaiter().GetResult()
        End Sub
        Friend Sub Reject()
            _failureReported = True
            _failureCode = "diagnostic.validation.rejected"
            _run.ReportAsync("diagnostic.validation.rejected", _stage,
                New Dictionary(Of String, String) From {{"category", "input"}}).GetAwaiter().GetResult()
        End Sub
        Friend Sub Fail()
            If _failureReported Then Return
            _failureReported = True
            _failureCode = "diagnostic.run.failed"
            _run.ReportAsync("diagnostic.run.failed", _stage,
                New Dictionary(Of String, String) From {{"category", If(_stage = "output", "output", "unexpected")}}).GetAwaiter().GetResult()
        End Sub
        Friend Sub OutputAttempted()
            _stage = "output"
            _run.ReportAsync("diagnostic.output.attempted", _stage).GetAwaiter().GetResult()
        End Sub
        Friend Sub OutputFinished(state As OutputWriteState)
            Dim outcome As String
            Select Case state
                Case OutputWriteState.Completed : outcome = "completed"
                Case OutputWriteState.CancelledByUser : outcome = "cancelled"
                Case OutputWriteState.RejectedBeforeWrite : outcome = "rejected"
                Case Else : outcome = "incomplete"
            End Select
            _failureCode = If(state = OutputWriteState.Completed, "diagnostic.output.completed", "diagnostic.output.incomplete")
            _run.ReportAsync(_failureCode,
                "output", New Dictionary(Of String, String) From {{"outcome", outcome}}).GetAwaiter().GetResult()
        End Sub
        Friend ReadOnly Property FailureDisplay As String
            Get
                Return "Check the selected input and analysis/output options, then retry. Run " & RunId &
                    " | " & _failureCode & " | " & _stage
            End Get
        End Property
        Public Sub Dispose() Implements IDisposable.Dispose
            _run.EndAsync().GetAwaiter().GetResult()
        End Sub
    End Class
End Namespace
