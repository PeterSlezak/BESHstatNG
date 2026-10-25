Option Explicit On
Option Strict On

Imports System.Collections.Generic
Imports System.Collections.ObjectModel
Imports System.Threading.Tasks

Namespace ExcelCommon
    Public Enum DiagnosticSeverity
        Debug = 0
        Information = 1
        Warning = 2
        [Error] = 3
    End Enum

    Public Enum DiagnosticDelivery
        Delivered = 0
        Suppressed = 1
        Unavailable = 2
    End Enum

    ''' <summary>Host-owned, best-effort sink. No retries, persistence or consent identity in Common.</summary>
    Public Interface IDiagnosticSink
        Function EmitAsync(record As DiagnosticRecord) As Task
    End Interface

    ''' <summary>
    ''' Immutable safe projection. Exact ordinal allowlists, not text scrubbing. Unknown
    ''' fields are dropped; unknown values become "redacted". No worksheet objects accepted.
    ''' Codes describe diagnostic events only, not the future user-facing fault taxonomy.
    ''' </summary>
    Public NotInheritable Class DiagnosticRecord
        Public ReadOnly Property EventCode As String
        Public ReadOnly Property Severity As DiagnosticSeverity
        Public ReadOnly Property Operation As String
        Public ReadOnly Property RunId As String
        Public ReadOnly Property Stage As String
        Public ReadOnly Property Timestamp As DateTimeOffset?
        Public ReadOnly Property Metadata As IReadOnlyDictionary(Of String, String)

        Public Shared Function Create(eventCode As String, operation As String, runId As Guid,
                                      stage As String, Optional timestamp As DateTimeOffset? = Nothing,
                                      Optional metadata As IDictionary(Of String, String) = Nothing) As DiagnosticRecord
            Dim code = Allowed(eventCode, "diagnostic.run.started", "diagnostic.validation.rejected",
                "diagnostic.output.attempted", "diagnostic.output.completed", "diagnostic.output.incomplete",
                "diagnostic.run.failed", "diagnostic.run.ended", "diagnostic.stage.entered",
                "output.sheet-name-adjusted", "diagnostic.host.ready", "diagnostic.host.unavailable",
                "diagnostic.host.retry", "diagnostic.user.cancelled", "diagnostic.output.consent", "diagnostic.analysis.completed",
                "diagnostic.command.invoked", "diagnostic.command.completed", "diagnostic.command.rejected")
            If code = "redacted" Then code = "diagnostic.event.redacted"
            Dim severity = DiagnosticSeverity.Information
            Select Case code
                Case "diagnostic.stage.entered" : severity = DiagnosticSeverity.Debug
                Case "diagnostic.validation.rejected", "diagnostic.output.incomplete", "diagnostic.event.redacted",
                     "output.sheet-name-adjusted", "diagnostic.host.unavailable", "diagnostic.command.rejected"
                    severity = DiagnosticSeverity.Warning
                Case "diagnostic.run.failed" : severity = DiagnosticSeverity.Error
            End Select
            Dim safe As New Dictionary(Of String, String)(StringComparer.Ordinal)
            If metadata IsNot Nothing Then
                ' At most three lookups/copies, regardless of source size. Never enumerate raw values.
                CopyValue(metadata, safe, "category", {"input", "host", "analysis", "output", "unexpected"})
                CopyValue(metadata, safe, "outcome", {"completed", "rejected", "failed", "cancelled", "incomplete", "ended"})
                CopyValue(metadata, safe, "host", {"windows", "office"})
            End If
            Return New DiagnosticRecord(code, severity, Allowed(operation, AnalysisIds.OneWayAnova, AnalysisIds.KruskalWallis, "office"),
                runId.ToString("N"), Allowed(stage, "invocation", "validation", "acquisition", "analysis", "output"),
                timestamp, New ReadOnlyDictionary(Of String, String)(safe))
        End Function

        Private Sub New(code As String, severity As DiagnosticSeverity, operation As String, runId As String,
                        stage As String, timestamp As DateTimeOffset?, metadata As IReadOnlyDictionary(Of String, String))
            Me.EventCode = code
            Me.Severity = severity
            Me.Operation = operation
            Me.RunId = runId
            Me.Stage = stage
            Me.Timestamp = timestamp
            Me.Metadata = metadata
        End Sub

        Private Shared Sub CopyValue(source As IDictionary(Of String, String), target As Dictionary(Of String, String),
                                     key As String, values As String())
            ' Only canonical keys are copied; the source comparer cannot introduce new keys.
            Dim value As String = Nothing
            If source.TryGetValue(key, value) Then target.Add(key, Allowed(value, values))
        End Sub

        Private Shared Function Allowed(value As String, ParamArray values As String()) As String
            If value IsNot Nothing AndAlso value.Length <= 64 Then
                For Each candidate In values
                    If String.Equals(value, candidate, StringComparison.Ordinal) Then Return candidate
                Next
            End If
            Return "redacted"
        End Function
    End Class

    ''' <summary>Factory dependencies are instance-owned. Never an ambient/global current run.</summary>
    Public NotInheritable Class DiagnosticEmitter
        Private ReadOnly _sink As IDiagnosticSink
        Private ReadOnly _clock As Func(Of DateTimeOffset)
        Private ReadOnly _ids As Func(Of Guid)
        Public Sub New(sink As IDiagnosticSink, Optional clock As Func(Of DateTimeOffset) = Nothing,
                       Optional ids As Func(Of Guid) = Nothing)
            _sink = sink
            _clock = If(clock, Function() DateTimeOffset.UtcNow)
            _ids = If(ids, Function() Guid.NewGuid())
        End Sub
        Public Async Function StartAsync(operation As String, traceEnabled As Boolean) As Task(Of DiagnosticRunContext)
            Dim id As Guid
            Try
                id = _ids()
            Catch
                id = Guid.NewGuid()
            End Try
            If id = Guid.Empty Then id = Guid.NewGuid()
            Dim run As New DiagnosticRunContext(_sink, _clock, id, operation, traceEnabled)
            Await run.ReportAsync("diagnostic.run.started", "invocation").ConfigureAwait(False)
            Return run
        End Function
    End Class

    ''' <summary>
    ''' Explicit per-invocation correlation lifetime, including across awaits. EndAsync in
    ''' the host's Finally. This is an event scope, not execution/progress/cancellation state.
    ''' Hosts emit output.completed only after their existing writer reports completion.
    ''' </summary>
    Public NotInheritable Class DiagnosticRunContext
        Private ReadOnly _sink As IDiagnosticSink
        Private ReadOnly _clock As Func(Of DateTimeOffset)
        Private ReadOnly _id As Guid
        Private ReadOnly _operation As String
        Private ReadOnly _trace As Boolean
        Private ReadOnly _gate As New Object()
        Private _ended As Boolean
        Friend Sub New(sink As IDiagnosticSink, clock As Func(Of DateTimeOffset), id As Guid, operation As String, trace As Boolean)
            _sink = sink
            _clock = clock
            _id = id
            _operation = operation
            _trace = trace
        End Sub
        Public ReadOnly Property RunId As String
            Get
                Return _id.ToString("N")
            End Get
        End Property

        Public Async Function ReportAsync(code As String, stage As String,
                                         Optional metadata As IDictionary(Of String, String) = Nothing) As Task(Of DiagnosticDelivery)
            SyncLock _gate
                If _ended Then Return DiagnosticDelivery.Suppressed
                If code = "diagnostic.run.ended" Then _ended = True
            End SyncLock
            Try
                Dim timestamp As DateTimeOffset? = Nothing
                Try
                    timestamp = _clock()
                Catch
                    ' An unavailable clock removes the optional timestamp, not the operation.
                End Try
                Dim record = DiagnosticRecord.Create(code, _operation, _id, stage, timestamp, metadata)
                If record.Severity = DiagnosticSeverity.Debug AndAlso Not _trace Then Return DiagnosticDelivery.Suppressed
                If _sink Is Nothing Then Return DiagnosticDelivery.Unavailable
                Await _sink.EmitAsync(record).ConfigureAwait(False)
                Return DiagnosticDelivery.Delivered
            Catch
                ' Never log the sink's exception, recurse, retry or replace the real failure.
                Return DiagnosticDelivery.Unavailable
            End Try
        End Function

        Public Function EndAsync() As Task(Of DiagnosticDelivery)
            Return ReportAsync("diagnostic.run.ended", "invocation")
        End Function
    End Class
End Namespace
