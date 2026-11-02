Option Explicit On
Option Strict On
Imports System
Imports System.Collections.Generic
Imports System.Threading.Tasks
Imports Microsoft.VisualStudio.TestTools.UnitTesting

' Shipping lease executed with fake producers only. No controls, STA or COM proof.
<TestClass>
Public Class Transport27LeaseTests
    <DataTestMethod>
    <DataRow("workbook-switch")>
    <DataRow("sheet-switch")>
    <DataRow("empty-selection")>
    <DataRow("cancel")>
    <DataRow("closed-sheet")>
    Public Sub InvalidationRejectsDeferredSelectionUntilExplicitRebind(reason As String)
        Dim lease As New RefEditEventLease(Of String), deferred As Action(Of String) = Nothing
        Dim observed As New List(Of String), detached As Integer
        Dim subscribe As Func(Of Action(Of String), Action) =
            Function(callback)
                deferred = callback
                Return Sub() detached += 1
            End Function
        lease.Rebind(subscribe, Sub(s) observed.Add(s))
        Dim old = deferred, generation = lease.CurrentGeneration
        old("before")
        lease.Invalidate() ' Actual control owns detecting these reasons; this tests its shipping guard.
        Assert.IsFalse(lease.IsCurrent(generation))
        old(reason)
        lease.Rebind(subscribe, Sub(s) observed.Add(s))
        old("late") : deferred("after")
        CollectionAssert.AreEqual(New String() {"before", "after"}, observed)
        lease.Dispose() : lease.Dispose() : deferred("disposed")
        Assert.AreEqual(2, detached)
    End Sub

    <TestMethod>
    Public Sub ReentrantDetachCannotReactivatePoisonedLease()
        Dim lease As New RefEditEventLease(Of Integer), callback As Action(Of Integer) = Nothing
        Dim detachCount As Integer
        lease.Rebind(Function(c)
                         callback = c
                         Return Sub()
                                    detachCount += 1
                                    lease.Clear()
                                End Sub
                     End Function, Sub(v) Assert.Fail("Stale selection forwarded"))
        Assert.ThrowsException(Of InvalidOperationException)(Sub() lease.Clear())
        callback(1)
        Assert.IsTrue(lease.IsClosed)
        Assert.ThrowsException(Of InvalidOperationException)(Sub() lease.Rebind(Function(c) Sub() Return, Sub(v) Return))
        lease.Dispose()
        Assert.AreEqual(1, detachCount)
    End Sub

    <TestMethod>
    Public Sub FreshConcurrentLeaseInstancesDoNotShareGenerationOrSubscriptionState()
        Dim runs As New List(Of Task)
        For i = 1 To 8
            runs.Add(Task.Run(Sub()
                                  Dim lease As New RefEditEventLease(Of Integer), callback As Action(Of Integer) = Nothing
                                  Dim observed As Integer, detached As Integer
                                  lease.Rebind(Function(c)
                                                   callback = c
                                                   Return Sub() detached += 1
                                               End Function, Sub(v) observed += v)
                                  callback(3) : lease.Dispose() : callback(9)
                                  Assert.AreEqual(3, observed) : Assert.AreEqual(1, detached)
                              End Sub))
        Next
        Task.WaitAll(runs.ToArray())
    End Sub
End Class
