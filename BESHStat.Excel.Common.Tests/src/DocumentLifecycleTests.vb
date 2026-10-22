Option Explicit On
Option Strict On
Imports System
Imports BESHStatNG.ExcelCommon
Imports Microsoft.VisualStudio.TestTools.UnitTesting

<TestClass>
Public Class DocumentLifecycleTests
    Private Shared Function Snapshot(Optional book As String = "book-a", Optional sheet As String = "sheet-a",
                                     Optional name As String = "Data", Optional revision As String = "1",
                                     Optional available As Boolean = True) As DocumentLifecycleSnapshot
        Return New DocumentLifecycleSnapshot(book, sheet, name, New OutputRectangle(1, 1, 5, 2), revision, available:=available)
    End Function

    <TestMethod>
    Public Sub VerifiedBindingRemainsValidAtRepeatedBoundaries()
        Using lease As New DocumentLifecycleLease(Snapshot(), selectionBound:=True)
            Assert.AreEqual(DocumentLifecycleStatus.VerifiedEqual, lease.Check(Snapshot()))
            lease.MarkInputCopied()
            lease.EnsureCurrent(Snapshot(revision:="2"))
        End Using
    End Sub

    <DataTestMethod>
    <DataRow("book-b", "sheet-a", "Data", DocumentInvalidationReason.DocumentChanged)>
    <DataRow("book-a", "replacement", "Data", DocumentInvalidationReason.WorksheetReplaced)>
    <DataRow("book-a", "sheet-a", "Renamed", DocumentInvalidationReason.WorksheetRenamed)>
    Public Sub ChangedPhysicalIdentityAndNameAddressingAreSticky(book As String, sheet As String, name As String, reason As DocumentInvalidationReason)
        Using lease As New DocumentLifecycleLease(Snapshot())
            Assert.AreEqual(DocumentLifecycleStatus.Stale, lease.Check(Snapshot(book, sheet, name)))
            Assert.AreEqual(reason, lease.Reason)
            Assert.AreEqual(DocumentLifecycleStatus.Stale, lease.Check(Snapshot()))
        End Using
    End Sub

    <TestMethod>
    Public Sub ObjectAddressedRenameCanRemainValidButNameAddressedBindingCannot()
        Using lease As New DocumentLifecycleLease(Snapshot(), nameAddressed:=False)
            lease.EnsureCurrent(Snapshot(name:="New name"))
        End Using
    End Sub

    <DataTestMethod> <DataRow("", "sheet-a")> <DataRow("book-a", "")>
    <DataRow(" ", "sheet-a")> <DataRow("book-a", " ")>
    Public Sub UnknownIdentityNeverCountsAsEquality(book As String, sheet As String)
        Using lease As New DocumentLifecycleLease(Snapshot(book, sheet))
            Assert.AreEqual(DocumentLifecycleStatus.Unavailable, lease.Check(Snapshot(book, sheet)))
        End Using
    End Sub

    <TestMethod>
    Public Sub ContextContinuityDoesNotFabricateWorkbookIdentity()
        Dim binding = New DocumentLifecycleSnapshot(Nothing, "sheet-a", "Data", contextIdentity:="native-context-a")
        Using lease As New DocumentLifecycleLease(binding)
            lease.EnsureCurrent(binding)
            Assert.AreEqual(String.Empty, lease.Binding.WorkbookIdentity)
            Assert.AreEqual(DocumentLifecycleStatus.Stale, lease.Check(
                New DocumentLifecycleSnapshot(Nothing, "sheet-a", "Data", contextIdentity:="native-context-b")))
        End Using
    End Sub

    <DataTestMethod> <DataRow(False)> <DataRow(True)>
    Public Sub VolatileSelectionRequiresVerifiedRevisionAndCopiedInputHasIndependentTarget(copied As Boolean)
        Using source As New DocumentLifecycleLease(Snapshot(), selectionBound:=True), target As New DocumentLifecycleLease(Snapshot())
            If copied Then source.MarkInputCopied()
            Assert.AreEqual(If(copied, DocumentLifecycleStatus.VerifiedEqual, DocumentLifecycleStatus.Stale), source.Check(Snapshot(revision:="2")))
            target.EnsureCurrent(Snapshot(revision:="2"))
            Assert.AreEqual(DocumentLifecycleStatus.Stale, target.Check(Snapshot(sheet:="replacement")))
        End Using
    End Sub

    <TestMethod>
    Public Sub UnsupportedRevisionRejectedForVolatileInputOnly()
        Using volatile As New DocumentLifecycleLease(Snapshot(revision:=""), selectionBound:=True), pinned As New DocumentLifecycleLease(Snapshot(revision:=""))
            Assert.AreEqual(DocumentLifecycleStatus.Unavailable, volatile.Check(Snapshot(revision:="")))
            pinned.EnsureCurrent(Snapshot(revision:=""))
            Assert.IsFalse(pinned.Invalidate(DocumentInvalidationReason.SelectionChanged))
            Assert.IsFalse(pinned.Invalidate(DocumentInvalidationReason.ActiveDocumentSwitched))
        End Using
    End Sub

    <DataTestMethod>
    <DataRow(DocumentInvalidationReason.WorkbookClosed)> <DataRow(DocumentInvalidationReason.WorksheetRemoved)>
    <DataRow(DocumentInvalidationReason.ContextDisposed)> <DataRow(DocumentInvalidationReason.ActiveDocumentSwitched)>
    Public Sub InvalidationAndDisposalAreIdempotentAndOperationsAreIsolated(reason As DocumentInvalidationReason)
        Using first As New DocumentLifecycleLease(Snapshot(), selectionBound:=True), second As New DocumentLifecycleLease(Snapshot(book:="other"))
            Assert.IsTrue(first.Invalidate(reason))
            Assert.IsFalse(first.Invalidate(reason))
            second.EnsureCurrent(Snapshot(book:="other"))
            first.Dispose() : first.Dispose()
            Assert.IsFalse(first.Invalidate(reason))
            Assert.AreEqual(DocumentLifecycleStatus.Disposed, first.Check(Snapshot()))
        End Using
    End Sub

    <TestMethod>
    Public Sub UnavailableObservationFailsClosedAndMetadataIsBounded()
        Using lease As New DocumentLifecycleLease(Snapshot())
            Assert.ThrowsException(Of DocumentLifecycleException)(Sub() lease.EnsureCurrent(Snapshot(available:=False)))
        End Using
        Assert.ThrowsException(Of ArgumentException)(Function() New DocumentLifecycleSnapshot(New String("x"c, 1025), "id", "name"))
        Assert.ThrowsException(Of ArgumentException)(Function() New DocumentLifecycleSnapshot("id", "id", "bad" & vbCrLf))
        For Each propertyInfo In GetType(DocumentLifecycleSnapshot).GetProperties()
            Assert.IsFalse(propertyInfo.CanWrite)
            Assert.IsFalse(propertyInfo.Name.Contains("Values"))
        Next
    End Sub

    <TestMethod>
    Public Sub LifecycleNeverReclassifiesCompletedOutputAndFaultsAreSanitized()
        Using execution As New ExecutionCoordinator(), lease As New DocumentLifecycleLease(Snapshot())
            execution.Start()
            lease.EnsureCurrent(Snapshot())
            execution.TryComplete()
            lease.Invalidate(DocumentInvalidationReason.WorkbookClosed)
            execution.RequestCancellation()
            Assert.AreEqual(ExecutionState.Completed, execution.Snapshot.State)
            Assert.IsNull(UserFaultMapper.FromOutcome(execution.Snapshot, failureException:=New DocumentLifecycleException(lease.Reason)))
            Dim fault = UserFaultMapper.FromException(New DocumentLifecycleException(lease.Reason))
            Assert.AreEqual(UserFaultCodes.DocumentChanged, fault.Code)
            Assert.AreEqual(fault.Code, UserFaultMapper.FromWriteState(OutputWriteState.RejectedBeforeWrite, "output.lifecycle-stale").Code)
            Assert.AreEqual(WriteCodes.MutationUncertain, UserFaultMapper.FromWriteState(OutputWriteState.PartiallyWritten, "output.lifecycle-stale", True).Code)
        End Using
    End Sub
End Class
