Option Explicit On
Option Strict On

Imports System.Collections.Concurrent
Imports System.Collections.Generic
Imports System.Linq
Imports System.Threading.Tasks
Imports BESHStatNG.AppInfrastructure
Imports BESHStatNG.ExcelCommon
Imports Microsoft.VisualStudio.TestTools.UnitTesting

<TestClass>
Public Class DiagnosticTests
    Private Shared ReadOnly FixedId As Guid = Guid.Parse("4f58068b-2754-4ba7-b232-c6d936bc8f28")
    Private Shared ReadOnly FixedTime As New DateTimeOffset(2026, 10, 21, 12, 0, 0, TimeSpan.Zero)

    Private Class Sink
        Implements IDiagnosticSink
        Public ReadOnly Records As New ConcurrentQueue(Of DiagnosticRecord)
        Public Failure As Exception
        Public Attempts As Integer
        Public Async Function EmitAsync(record As DiagnosticRecord) As Task Implements IDiagnosticSink.EmitAsync
            Threading.Interlocked.Increment(Attempts)
            Await Task.Yield()
            If Failure IsNot Nothing Then Throw Failure
            Records.Enqueue(record)
        End Function
    End Class

    <TestMethod>
    Public Async Function InjectedClockAndIds_KeepAllLifecycleRecordsCorrelatedAcrossAwait() As Task
        Dim target As New Sink()
        Dim run = Await New DiagnosticEmitter(target, Function() FixedTime, Function() FixedId).StartAsync(AnalysisIds.OneWayAnova, True)
        Await Task.Yield()
        Await run.ReportAsync("diagnostic.validation.rejected", "validation")
        Await run.EndAsync()
        Assert.AreEqual(FixedId.ToString("N"), run.RunId)
        CollectionAssert.AreEqual({"diagnostic.run.started", "diagnostic.validation.rejected", "diagnostic.run.ended"},
                                  target.Records.Select(Function(r) r.EventCode).ToArray())
        Assert.IsTrue(target.Records.All(Function(r) r.RunId = run.RunId AndAlso r.Timestamp.HasValue AndAlso r.Timestamp.Value = FixedTime AndAlso r.Operation = AnalysisIds.OneWayAnova))
    End Function

    <TestMethod>
    Public Async Function ParallelRunsHaveUniqueIdsAndNoAmbientState() As Task
        Dim target As New Sink()
        Dim emitter As New DiagnosticEmitter(target)
        Dim ids = Await Task.WhenAll(Enumerable.Range(1, 64).Select(Async Function(i)
            Dim op = If(i Mod 2 = 0, AnalysisIds.OneWayAnova, AnalysisIds.KruskalWallis)
            Dim run = Await emitter.StartAsync(op, True)
            Await Task.Yield()
            Await run.ReportAsync("diagnostic.output.attempted", "output")
            Await run.ReportAsync("diagnostic.output.completed", "output")
            Await run.EndAsync()
            Return run.RunId
        End Function))
        Assert.AreEqual(64, ids.Distinct().Count())
        Assert.AreEqual(256, target.Records.Count)
        For Each group In target.Records.GroupBy(Function(r) r.RunId)
            Assert.AreEqual(4, group.Count())
            Assert.AreEqual(1, group.Select(Function(r) r.Operation).Distinct().Count())
        Next
    End Function

    <TestMethod>
    Public Async Function TraceOffSuppressesDebugButRetainsRejectionErrorAndLifecycleWithoutChangingDefaults() As Task
        Dim original = CoreServices.TraceExecutionLoggingEnabled
        Dim data = PreferenceSnapshot.Defaults().ToData()
        data.TraceEnabled = False
        Dim settings = PreferenceSnapshot.Normalize(data).Snapshot
        Dim target As New Sink()
        Dim run = Await New DiagnosticEmitter(target).StartAsync(AnalysisIds.KruskalWallis, settings.TraceEnabled)
        Assert.AreEqual(DiagnosticDelivery.Suppressed, Await run.ReportAsync("diagnostic.stage.entered", "analysis"))
        Await run.ReportAsync("diagnostic.validation.rejected", "validation")
        Await run.ReportAsync("diagnostic.run.failed", "analysis")
        Await run.EndAsync()
        Assert.AreEqual(4, target.Records.Count)
        Assert.IsTrue(target.Records.Any(Function(r) r.Severity = DiagnosticSeverity.Warning))
        Assert.IsTrue(target.Records.Any(Function(r) r.Severity = DiagnosticSeverity.Error))
        Assert.AreEqual(original, CoreServices.TraceExecutionLoggingEnabled)
    End Function

    <DataTestMethod>
    <DataRow("=SUM(A1:B99)")>
    <DataRow("'Payroll'!$A$1")>
    <DataRow("C:\Users\private\salary.xlsx")>
    <DataRow("https://private.example/token?secret=abc")>
    <DataRow("patient Alice account 12345")>
    <DataRow("秘密🔒")>
    <DataRow("")>
    <DataRow(Nothing)>
    Public Sub UntrustedStringsCannotEnterAnyStructuredField(payload As String)
        Dim metadata As New Dictionary(Of String, String) From {{"category", payload}, {"outcome", payload},
            {"host", payload}, {"exception", payload}, {"cells", payload}}
        Dim record = DiagnosticRecord.Create(payload, payload, FixedId, payload, FixedTime, metadata)
        Assert.AreEqual("diagnostic.event.redacted", record.EventCode)
        Assert.AreEqual(DiagnosticSeverity.Warning, record.Severity)
        Assert.AreEqual("redacted", record.Operation)
        Assert.AreEqual("redacted", record.Stage)
        Assert.AreEqual(3, record.Metadata.Count)
        Assert.IsTrue(record.Metadata.Values.All(Function(v) v = "redacted"))
        Assert.AreEqual(FixedId.ToString("N"), record.RunId)
    End Sub

    <TestMethod>
    Public Sub LongStringsControlCharactersAndInjectionAreReplacedRatherThanTruncated()
        For Each payload In {New String("x"c, 100000), "input" & ChrW(13) & ChrW(10) & "forged=secret", "input" & ChrW(0), "input" & ChrW(&H2028)}
            UntrustedStringsCannotEnterAnyStructuredField(payload)
        Next
    End Sub

    <TestMethod>
    Public Sub MetadataIsBoundedDefensivelyCopiedAndReadOnly()
        Dim values As New Dictionary(Of String, String) From {{"category", "input"}, {"outcome", "rejected"}, {"host", "office"}}
        For i = 0 To 999
            values.Add("unknown" & i.ToString(), "secret")
        Next
        Dim record = DiagnosticRecord.Create("diagnostic.validation.rejected", AnalysisIds.OneWayAnova, FixedId, "validation", metadata:=values)
        values("category") = "=SECRET()"
        values.Clear()
        Assert.AreEqual(3, record.Metadata.Count)
        Assert.AreEqual("input", record.Metadata("category"))
        Assert.AreEqual("rejected", record.Metadata("outcome"))
        Assert.AreEqual("office", record.Metadata("host"))
        Assert.ThrowsException(Of NotSupportedException)(Sub() DirectCast(record.Metadata, IDictionary(Of String, String)).Add("cells", "raw"))
        Assert.IsTrue(GetType(DiagnosticRecord).GetProperties().All(Function(p) Not p.CanWrite))
    End Sub

    <DataTestMethod>
    <DataRow(False)> <DataRow(True)>
    Public Async Function ThrowingOrDisposedSinkNeverMasksOriginalValidationFailure(disposed As Boolean) As Task
        Dim target As New Sink With {.Failure = If(disposed, CType(New ObjectDisposedException("=SECRET()"), Exception), New Exception("Payroll!A1"))}
        Dim original As New ArgumentException("=SECRET(A1)" & Environment.NewLine & "C:\private.xlsx")
        Dim run = Await New DiagnosticEmitter(target).StartAsync(AnalysisIds.OneWayAnova, False)
        Dim caught As Exception = Nothing
        Try
            Try
                Throw original
            Finally
                Assert.AreEqual(DiagnosticDelivery.Unavailable, run.ReportAsync("diagnostic.validation.rejected", "validation").GetAwaiter().GetResult())
                run.EndAsync().GetAwaiter().GetResult()
            End Try
        Catch ex As Exception
            caught = ex
        End Try
        Assert.AreSame(original, caught)
        Assert.AreEqual(3, target.Attempts)
        Assert.AreEqual(0, target.Records.Count)
    End Function

    <TestMethod>
    Public Async Function UnavailableSinkAndClockAndIdFailuresHaveSafeLocalResults() As Task
        Dim run = Await New DiagnosticEmitter(Nothing, Function() ThrowClock(), Function() Guid.Empty).StartAsync("secret workbook", True)
        Assert.AreNotEqual(Guid.Empty.ToString("N"), run.RunId)
        Assert.AreEqual(DiagnosticDelivery.Unavailable, Await run.ReportAsync("diagnostic.run.failed", "analysis"))
        Await run.EndAsync()
        Assert.AreEqual(DiagnosticDelivery.Suppressed, Await run.EndAsync())
        Dim target As New Sink()
        Dim other = Await New DiagnosticEmitter(target, Function() ThrowClock(), Function() ThrowId()).StartAsync(AnalysisIds.OneWayAnova, True)
        Assert.IsFalse(target.Records.Single().Timestamp.HasValue)
        Assert.AreNotEqual(run.RunId, other.RunId)
        Await other.EndAsync()
    End Function

    Private Shared Function ThrowClock() As DateTimeOffset
        Throw New Exception("secret clock configuration")
    End Function
    Private Shared Function ThrowId() As Guid
        Throw New Exception("secret id configuration")
    End Function

    <TestMethod>
    Public Async Function ActualInvalidSourceIsNotRetainedOrLogged() As Task
        Dim target As New Sink()
        Dim run = Await New DiagnosticEmitter(target).StartAsync(AnalysisIds.OneWayAnova, True)
        Try
            GroupedNumericRangeParser.Parse(Nothing)
            Assert.Fail("Expected source validation failure")
        Catch ex As ArgumentException
            run.ReportAsync("diagnostic.validation.rejected", "acquisition").GetAwaiter().GetResult()
        Finally
            run.EndAsync().GetAwaiter().GetResult()
        End Try
        Assert.AreEqual(3, target.Records.Count)
        Assert.IsTrue(target.Records.All(Function(r) r.Metadata.Count = 0))
        Assert.IsFalse(GetType(DiagnosticRecord).GetFields(Reflection.BindingFlags.Instance Or Reflection.BindingFlags.NonPublic).
            Any(Function(f) f.FieldType Is GetType(SpreadsheetRangeData) OrElse f.FieldType.IsArray))
    End Function
End Class
