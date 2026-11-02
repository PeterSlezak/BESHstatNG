Option Strict On
Option Explicit On
Option Infer On
Imports System
Imports Microsoft.VisualStudio.TestTools.UnitTesting

<TestClass>
Public Class MigrationRefEditBoundary09Tests
    Private NotInheritable Class Source
        Friend Callback As Action(Of Integer)
        Friend Detached As Integer
        Friend OnSubscribe As Action
        Friend OnDetach As Action
        Friend Function Subscribe(action As Action(Of Integer)) As Action
            Callback = action
            If OnSubscribe IsNot Nothing Then OnSubscribe()
            Return Sub()
                       Detached += 1
                       If OnDetach IsNot Nothing Then OnDetach()
                   End Sub
        End Function
    End Class

    <TestMethod> Public Sub CurrentCallbackRoutesThroughActualProposedCoordinator()
        Dim source As New Source, lease As New RefEditEventLease(Of Integer), observed As Integer
        lease.Rebind(AddressOf source.Subscribe, Sub(v) observed = v)
        source.Callback(7)
        Assert.AreEqual(7, observed)
        lease.Dispose()
    End Sub

    <TestMethod> Public Sub DisposeTwiceDetachesExactlyOnceAndIgnoresLateCallback()
        Dim source As New Source, lease As New RefEditEventLease(Of Integer), calls As Integer
        lease.Rebind(AddressOf source.Subscribe, Sub(v) calls += 1)
        lease.Dispose() : lease.Dispose() : source.Callback(7)
        Assert.AreEqual(1, source.Detached) : Assert.AreEqual(0, calls)
    End Sub

    <TestMethod> Public Sub OldCallbackCannotRetargetNewSource()
        Dim first As New Source, second As New Source, lease As New RefEditEventLease(Of Integer), observed As Integer
        lease.Rebind(AddressOf first.Subscribe, Sub(v) observed = v)
        lease.Rebind(AddressOf second.Subscribe, Sub(v) observed = v)
        first.Callback(9) : Assert.AreEqual(0, observed)
        second.Callback(3) : Assert.AreEqual(3, observed)
        Assert.AreEqual(1, first.Detached) : lease.Dispose()
    End Sub

    <TestMethod> Public Sub InvalidatedBeforeDetachReentrantCallback()
        Dim source As New Source, lease As New RefEditEventLease(Of Integer), calls As Integer
        lease.Rebind(AddressOf source.Subscribe, Sub(v) calls += 1)
        source.OnDetach = Sub() source.Callback(9)
        lease.Clear()
        Assert.AreEqual(0, calls) : Assert.AreEqual(1, source.Detached)
    End Sub

    <TestMethod> Public Sub FailedDetachBlocksReplacementAndDoesNotDetachTwice()
        Dim source As New Source, replacement As New Source, lease As New RefEditEventLease(Of Integer)
        lease.Rebind(AddressOf source.Subscribe, Sub(v) Assert.Fail())
        source.OnDetach = Sub() Throw New InvalidOperationException("MOCK disconnected RCW")
        Assert.ThrowsException(Of InvalidOperationException)(Sub() lease.Rebind(AddressOf replacement.Subscribe, Sub(v) Assert.Fail()))
        source.Callback(1)
        Assert.IsNull(replacement.Callback) : lease.Dispose()
        Assert.AreEqual(1, source.Detached) : Assert.IsTrue(lease.IsClosed)
    End Sub

    <TestMethod> Public Sub CallbackDuringSubscribeIsNotArmed()
        Dim source As New Source, lease As New RefEditEventLease(Of Integer), calls As Integer
        source.OnSubscribe = Sub() source.Callback(1)
        lease.Rebind(AddressOf source.Subscribe, Sub(v) calls += 1)
        Assert.AreEqual(0, calls) : source.Callback(2) : Assert.AreEqual(1, calls) : lease.Dispose()
    End Sub

    <TestMethod> Public Sub DisposeDuringSubscribeCleansUpReturnedSubscription()
        Dim source As New Source, lease As New RefEditEventLease(Of Integer)
        source.OnSubscribe = Sub() lease.Dispose()
        Assert.ThrowsException(Of InvalidOperationException)(Sub() lease.Rebind(AddressOf source.Subscribe, Sub(v) Assert.Fail()))
        source.Callback(1) : Assert.AreEqual(1, source.Detached)
    End Sub

    <TestMethod> Public Sub AttachFailureLeavesClosedLease()
        Dim lease As New RefEditEventLease(Of Integer)
        Assert.ThrowsException(Of InvalidOperationException)(Sub() lease.Rebind(Function(cb) ThrowAttach(), Sub(v) Assert.Fail()))
        Assert.IsTrue(lease.IsClosed)
    End Sub
    Private Shared Function ThrowAttach() As Action
        Throw New InvalidOperationException("MOCK attach failure")
    End Function

    <TestMethod> Public Sub ReentrantBindFailsClosed()
        Dim first As New Source, second As New Source, lease As New RefEditEventLease(Of Integer)
        first.OnSubscribe = Sub() lease.Rebind(AddressOf second.Subscribe, Sub(v) Assert.Fail())
        Assert.ThrowsException(Of InvalidOperationException)(Sub() lease.Rebind(AddressOf first.Subscribe, Sub(v) Assert.Fail()))
        first.Callback(1) : Assert.IsTrue(lease.IsClosed) : Assert.IsNull(second.Callback)
    End Sub

    <TestMethod> Public Sub ThrowingCallbackInvalidatesFutureForwarding()
        Dim source As New Source, lease As New RefEditEventLease(Of Integer), calls As Integer
        lease.Rebind(AddressOf source.Subscribe, Sub(v)
                                                   calls += 1
                                                   Throw New InvalidOperationException("MOCK callback")
                                               End Sub)
        Assert.ThrowsException(Of InvalidOperationException)(Sub() source.Callback(1))
        source.Callback(2) : Assert.AreEqual(1, calls) : lease.Dispose()
    End Sub

    <TestMethod> Public Sub SameSourceRebindStillDetachesPriorSubscription()
        Dim source As New Source, lease As New RefEditEventLease(Of Integer)
        lease.Rebind(AddressOf source.Subscribe, Sub(v) Return)
        lease.Rebind(AddressOf source.Subscribe, Sub(v) Return)
        Assert.AreEqual(1, source.Detached) : lease.Dispose() : Assert.AreEqual(2, source.Detached)
    End Sub

    <TestMethod> Public Sub ThrowingDisposeCannotRearmOrDoubleDetach()
        Dim source As New Source, lease As New RefEditEventLease(Of Integer)
        lease.Rebind(AddressOf source.Subscribe, Sub(v) Assert.Fail())
        source.OnDetach = Sub() Throw New InvalidOperationException("MOCK detach")
        Assert.ThrowsException(Of InvalidOperationException)(Sub() lease.Dispose())
        lease.Dispose() : source.Callback(1) : Assert.AreEqual(1, source.Detached)
    End Sub
End Class
