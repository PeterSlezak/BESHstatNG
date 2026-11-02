Option Explicit On
Option Strict On
Option Infer On
Imports System
Imports System.Collections.Generic
Imports System.Runtime.InteropServices
Imports BESHStatNG.AppInfrastructure
Imports BESHStatNG.ExcelCommon
Imports Microsoft.VisualStudio.TestTools.UnitTesting

' Compiled only into a new isolated friend-test assembly by C07.HostFreeSeamRunner.py.
' Exercises the REAL injectable Windows observer/run; never constructs RefEdit/Form/Excel.
<TestClass>
Public Class MigrationRefEditBoundary07Tests
    Private Class FakeEventSource
        Public Callback As Action(Of DocumentInvalidationReason, String, String)
        Public Adds As Integer
        Public Removes As Integer
        Public SwallowedDisconnects As Integer
        Public DisconnectOnRemove As Boolean
        Public BorrowedOwner As New Object()
        Public Function Subscribe(callbackValue As Action(Of DocumentInvalidationReason, String, String)) As Action
            Adds += 1 : Callback = callbackValue
            Dim removed As Boolean
            Return Sub()
                If removed Then Return
                removed = True : Removes += 1
                Try
                    If DisconnectOnRemove Then Throw New COMException("MOCK disconnected event source, not native reproduction", -2147221080)
                Catch ex As COMException
                    ' Mirrors the CONTRACT of the production native subscription wrapper:
                    ' safe guarded detach is supplied by the injected host, not the observer.
                    SwallowedDisconnects += 1
                End Try
            End Sub
        End Function
    End Class

    Private Shared Function Snapshot(Optional sheet As String = "original", Optional book As String = "owned") As DocumentLifecycleSnapshot
        Return New DocumentLifecycleSnapshot(book, sheet, "Data", New OutputRectangle(1, 1, 4, 3), "r1")
    End Function

    <DataTestMethod> <DataRow(False)> <DataRow(True)>
    Public Sub SubscribeDisposeDoubleRemoveAndLateCallbackAreHostFree(disconnected As Boolean)
        Dim host As New FakeEventSource With {.DisconnectOnRemove = disconnected}
        Dim borrowed = host.BorrowedOwner, reads As Integer
        Dim observer As New WindowsDocumentLifecycle(AddressOf host.Subscribe)
        Dim lease = observer.Watch(Function()
            reads += 1
            Return Snapshot()
        End Function)
        observer.EnsureCurrent()
        observer.Dispose() : observer.Dispose()
        Dim before = reads
        host.Callback(DocumentInvalidationReason.WorksheetRemoved, "owned", "original")
        Assert.AreEqual(before, reads)
        Assert.AreEqual(1, host.Adds) : Assert.AreEqual(1, host.Removes)
        Assert.AreEqual(If(disconnected, 1, 0), host.SwallowedDisconnects)
        Assert.AreSame(borrowed, host.BorrowedOwner, "Borrowed managed token is not disposed/released by observer")
        Assert.AreEqual(DocumentLifecycleStatus.Disposed, lease.Check(Snapshot()))
    End Sub

    <DataTestMethod> <DataRow(DocumentInvalidationReason.WorksheetRemoved)>
    <DataRow(DocumentInvalidationReason.WorkbookClosed)>
    Public Sub RebindNeedsNewOwnerAndCannotRetargetOldPinnedLease(reason As DocumentInvalidationReason)
        Dim oldHost As New FakeEventSource(), newHost As New FakeEventSource()
        Dim oldObserver As New WindowsDocumentLifecycle(AddressOf oldHost.Subscribe)
        Dim oldLease = oldObserver.Watch(Function() Snapshot())
        oldHost.Callback(reason, "owned", "original")
        Assert.ThrowsException(Of DocumentLifecycleException)(AddressOf oldObserver.EnsureCurrent)
        oldObserver.Dispose()
        Using rebound As New WindowsDocumentLifecycle(AddressOf newHost.Subscribe)
            rebound.Watch(Function() Snapshot(sheet:="replacement"))
            oldHost.Callback(DocumentInvalidationReason.WorkbookClosed, "owned", Nothing)
            rebound.EnsureCurrent()
            Assert.AreEqual(DocumentLifecycleStatus.Disposed, oldLease.Check(Snapshot()))
        End Using
        Assert.AreEqual(1, oldHost.Removes) : Assert.AreEqual(1, newHost.Removes)
    End Sub

    <DataTestMethod> <DataRow("sheet")> <DataRow("book")> <DataRow("cancel")>
    Public Sub InvalidContextOrCancellationNeverCallsWriteDelegate(change As String)
        Dim host As New FakeEventSource(), current = Snapshot(), writes As Integer
        Using run = New WindowsExecutionDispatcher().TryBegin()
            run.Execution.Start()
            run.Lifecycle = New WindowsDocumentLifecycle(AddressOf host.Subscribe)
            run.Lifecycle.Watch(Function() current)
            run.BindInputAddresses("'Data'!A1:C4", Nothing)
            If change = "cancel" Then
                run.Execution.RequestCancellation()
                Assert.ThrowsException(Of OperationCanceledException)(Sub() run.Write(Function()
                    writes += 1 : Return OutputWriteState.Completed
                End Function))
            Else
                current = If(change = "sheet", Snapshot(sheet:="replacement"), Snapshot(book:="other"))
                Assert.ThrowsException(Of DocumentLifecycleException)(Sub() run.Write(Function()
                    writes += 1 : Return OutputWriteState.Completed
                End Function))
            End If
            Assert.AreEqual("'Data'!A1:C4", run.SourceAddress)
            Assert.AreEqual(0, writes)
        End Using
    End Sub

    <TestMethod>
    Public Sub InjectedDetachExceptionIsNotSilentlyClaimedSafeCleanup()
        Dim notice As Action(Of DocumentInvalidationReason, String, String) = Nothing
        Dim observer As New WindowsDocumentLifecycle(Function(callback)
            notice = callback
            Return Sub() Throw New InvalidOperationException("deliberately unsafe injected detach")
        End Function)
        Dim reads As Integer
        Dim lease = observer.Watch(Function()
            reads += 1 : Return Snapshot()
        End Function)
        Assert.ThrowsException(Of InvalidOperationException)(AddressOf observer.Dispose)
        Assert.AreEqual(DocumentLifecycleStatus.Disposed, lease.Check(Snapshot()))
        Dim before = reads
        notice(DocumentInvalidationReason.WorkbookClosed, "owned", Nothing)
        Assert.AreEqual(before, reads)
        observer.Dispose() ' Already disposed: no second detach attempt.
        ' Characterization only. Native Subscribe supplies guarded detach; a general
        ' safe-cleanup claim for arbitrary injected code would be false.
    End Sub

    <TestMethod>
    Public Sub EventForOtherWorkbookDoesNotInvalidateOwnedRun()
        Dim host As New FakeEventSource()
        Using observer As New WindowsDocumentLifecycle(AddressOf host.Subscribe)
            observer.Watch(Function() Snapshot())
            host.Callback(DocumentInvalidationReason.WorkbookClosed, "unrelated", Nothing)
            observer.EnsureCurrent()
        End Using
    End Sub
End Class
