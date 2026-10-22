Option Explicit On
Option Strict On
Imports System
Imports System.Threading
Imports BESHStatNG.AppInfrastructure
Imports BESHStatNG.ExcelCommon
Imports Microsoft.VisualStudio.TestTools.UnitTesting

<TestClass>
Public Class DocumentLifecycleAdapterTests
    Private Shared Function Snapshot(Optional book As String = "book-a", Optional sheet As String = "sheet-a",
                                     Optional name As String = "Data", Optional available As Boolean = True) As DocumentLifecycleSnapshot
        Return New DocumentLifecycleSnapshot(book, sheet, name, available:=available)
    End Function

    <DataTestMethod>
    <DataRow("switch")> <DataRow("rename")> <DataRow("delete")> <DataRow("replace")> <DataRow("unknown")>
    Public Sub FreshObservationRejectsStaleTargetBeforeDelegate(change As String)
        Dim observed = Snapshot()
        Using run = New WindowsExecutionDispatcher().TryBegin()
            run.Execution.Start()
            run.Lifecycle = New WindowsDocumentLifecycle()
            run.Lifecycle.Watch(Function() observed)
            Select Case change
                Case "switch" : observed = Snapshot(book:="book-b")
                Case "rename" : observed = Snapshot(name:="Renamed")
                Case "delete" : observed = Snapshot(available:=False)
                Case "replace" : observed = Snapshot(sheet:="replacement")
                Case "unknown" : observed = Snapshot(sheet:="")
            End Select
            Dim writes As Integer
            Assert.ThrowsException(Of DocumentLifecycleException)(Sub() run.Write(Function()
                writes += 1
                Return OutputWriteState.Completed
            End Function))
            Assert.AreEqual(0, writes)
        End Using
    End Sub

    <TestMethod>
    Public Sub ScopedEventsIgnoreOtherBooksAndDetachOnceWithLateCallbackIsolation()
        Dim notify As Action(Of DocumentInvalidationReason, String, String) = Nothing
        Dim detached As Integer, reads As Integer
        Dim observer As New WindowsDocumentLifecycle(Function(callback)
            notify = callback
            Return Sub() detached += 1
        End Function)
        observer.Watch(Function()
            reads += 1
            Return Snapshot()
        End Function)
        notify(DocumentInvalidationReason.WorkbookClosed, "other-book", Nothing)
        observer.EnsureCurrent()
        notify(DocumentInvalidationReason.SelectionChanged, "book-a", "sheet-a")
        observer.EnsureCurrent() ' RefEdit coordinates are pinned; cell selection is unrelated.
        notify(DocumentInvalidationReason.WorksheetRemoved, "book-a", "sheet-a")
        Assert.ThrowsException(Of DocumentLifecycleException)(AddressOf observer.EnsureCurrent)
        observer.Dispose() : observer.Dispose()
        Dim before = reads
        notify(DocumentInvalidationReason.WorkbookClosed, "book-a", Nothing)
        Assert.AreEqual(before, reads) : Assert.AreEqual(1, detached)
    End Sub

    <DataTestMethod>
    <DataRow(SpreadsheetOutputDestination.CurrentWorksheet)>
    <DataRow(SpreadsheetOutputDestination.NewWorksheet)>
    <DataRow(SpreadsheetOutputDestination.NewWorkbook)>
    Public Sub IntentionalDestinationCreationDoesNotRetargetPinnedOwner(destination As SpreadsheetOutputDestination)
        Dim source = Snapshot(), created = Snapshot(book:="new-book", sheet:="new-sheet")
        Using observer As New WindowsDocumentLifecycle()
            observer.Watch(Function() source)
            If destination = SpreadsheetOutputDestination.NewWorksheet Then observer.Watch(Function() Snapshot(sheet:=""), requiresWorksheet:=False)
            observer.EnsureCurrent()
            ' Capture the object returned by Add, rather than the now-active workbook.
            If destination <> SpreadsheetOutputDestination.CurrentWorksheet Then observer.Watch(Function() created)
            observer.EnsureCurrent()
            source = Snapshot(available:=False)
            Assert.ThrowsException(Of DocumentLifecycleException)(AddressOf observer.EnsureCurrent)
        End Using
    End Sub

    <TestMethod>
    Public Sub ReconciliationIsNeverInvokedFromAnotherThread()
        Dim reads As Integer, failure As Exception = Nothing
        Using observer As New WindowsDocumentLifecycle()
            observer.Watch(Function()
                reads += 1
                Return Snapshot()
            End Function)
            Dim worker As New Thread(Sub()
                Try : observer.EnsureCurrent() : Catch ex As Exception : failure = ex : End Try
            End Sub)
            worker.Start() : worker.Join()
            Assert.IsInstanceOfType(failure, GetType(DocumentLifecycleException))
            Assert.AreEqual(1, reads)
        End Using
    End Sub

    <TestMethod>
    Public Sub UnexpectedEventThreadInvalidatesMetadataWithoutNativeObservation()
        Dim notify As Action(Of DocumentInvalidationReason, String, String) = Nothing
        Dim reads As Integer
        Using observer As New WindowsDocumentLifecycle(Function(callback)
            notify = callback
            Return Sub()
                   End Sub
        End Function)
            observer.Watch(Function()
                reads += 1
                Return Snapshot()
            End Function)
            Dim worker As New Thread(Sub() notify(DocumentInvalidationReason.WorkbookClosed, "book-a", Nothing))
            worker.Start() : worker.Join()
            Assert.AreEqual(1, reads)
            Assert.ThrowsException(Of DocumentLifecycleException)(AddressOf observer.EnsureCurrent)
            Assert.AreEqual(1, reads)
        End Using
    End Sub

    <TestMethod>
    Public Sub RefEditEventUpdatesCannotRetargetAnAlreadyBoundAcquisition()
        Dim liveAddress As String = "'Original'!A1:B6"
        Using run = New WindowsExecutionDispatcher().TryBegin()
            run.Execution.Start()
            run.Lifecycle = New WindowsDocumentLifecycle()
            run.Lifecycle.Watch(Function() Snapshot())
            run.BindInputAddresses(liveAddress, "'Original'!C1:C6")
            liveAddress = "'Other sheet'!D1:E6" ' RefEdit's host event can update a disabled control.
            Dim acquired = run.InvokeStage(ExecutionStage.Acquisition, Function() run.SourceAddress)
            Assert.AreEqual("'Original'!A1:B6", acquired)
            Assert.AreEqual("'Original'!C1:C6", run.GroupAddress)
            Assert.AreNotEqual(liveAddress, acquired)
            Assert.ThrowsException(Of DocumentLifecycleException)(Sub() run.BindInputAddresses(liveAddress, Nothing))
        End Using
    End Sub

    <TestMethod>
    Public Sub CompletedDisposedRunCannotStartAnotherNativeDelegate()
        Dim writes As Integer
        Using run = New WindowsExecutionDispatcher().TryBegin()
            run.Execution.Start()
            run.Write(Function() OutputWriteState.Completed)
            run.Dispose()
            Assert.ThrowsException(Of DocumentLifecycleException)(Sub() run.Write(Function()
                writes += 1
                Return OutputWriteState.Completed
            End Function))
            Assert.AreEqual(0, writes)
            Assert.AreEqual(ExecutionState.Completed, run.Execution.Snapshot.State)
        End Using
    End Sub
End Class
