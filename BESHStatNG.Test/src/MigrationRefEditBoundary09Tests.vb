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

<TestClass>
Public Class MigrationRefEditBoundary10Tests
    <TestMethod> Public Sub ReentrantBindDuringClearCannotInstallReplacement()
        Dim lease As New RefEditEventLease(Of Integer), replacement As Boolean
        lease.Rebind(Function(cb) Sub() lease.Rebind(Function(other)
                                                       replacement = True
                                                       Return Sub() Return
                                                   End Function, Sub(v) Return), Sub(v) Return)
        Assert.ThrowsException(Of InvalidOperationException)(AddressOf lease.Clear)
        Assert.IsFalse(replacement) : Assert.IsTrue(lease.IsClosed) : lease.Dispose()
    End Sub

    <TestMethod> Public Sub CaughtReentrantBindStillClosesOuterTransition()
        Dim lease As New RefEditEventLease(Of Integer), subscribed As Integer
        lease.Rebind(Function(cb) Sub()
                                     Try
                                         lease.Rebind(Function(other)
                                                          subscribed += 1
                                                          Return Sub() Return
                                                      End Function, Sub(v) Return)
                                     Catch ex As InvalidOperationException
                                     End Try
                                 End Sub, Sub(v) Return)
        Assert.ThrowsException(Of InvalidOperationException)(AddressOf lease.Clear)
        Assert.AreEqual(0, subscribed) : Assert.IsTrue(lease.IsClosed) : lease.Dispose()
    End Sub

    <TestMethod> Public Sub DisposeDuringUnsubscribePreventsNextSubscribe()
        Dim lease As New RefEditEventLease(Of Integer), replacement As Boolean
        lease.Rebind(Function(cb) Sub() lease.Dispose(), Sub(v) Return)
        Assert.ThrowsException(Of InvalidOperationException)(Sub() lease.Rebind(Function(cb)
                                                                                  replacement = True
                                                                                  Return Sub() Return
                                                                              End Function, Sub(v) Return))
        Assert.IsFalse(replacement) : Assert.IsTrue(lease.IsClosed)
    End Sub

    <TestMethod> Public Sub ClearDuringSubscribeInvalidatesSavedCallback()
        Dim lease As New RefEditEventLease(Of Integer), saved As Action(Of Integer), calls As Integer
        Assert.ThrowsException(Of InvalidOperationException)(Sub() lease.Rebind(Function(cb)
                                                                                  saved = cb
                                                                                  lease.Clear()
                                                                                  Return Sub() Return
                                                                              End Function, Sub(v) calls += 1))
        saved(1) : Assert.AreEqual(0, calls) : Assert.IsTrue(lease.IsClosed) : lease.Dispose()
    End Sub

    <TestMethod> Public Sub MissingSubscribeFailsClosed()
        Dim lease As New RefEditEventLease(Of Integer)
        Assert.ThrowsException(Of ArgumentNullException)(Sub() lease.Rebind(Nothing, Sub(v) Return))
        Assert.IsTrue(lease.IsClosed)
    End Sub

    <TestMethod> Public Sub MissingCallbackFailsClosedBeforeAttach()
        Dim lease As New RefEditEventLease(Of Integer), subscribed As Boolean
        Assert.ThrowsException(Of ArgumentNullException)(Sub() lease.Rebind(Function(cb)
                                                                               subscribed = True
                                                                               Return Sub() Return
                                                                           End Function, Nothing))
        Assert.IsFalse(subscribed) : Assert.IsTrue(lease.IsClosed)
    End Sub

    <TestMethod> Public Sub InvalidateBothSourcesBeforeAnyDetach()
        Dim first As New RefEditEventLease(Of Integer), second As New RefEditEventLease(Of Integer)
        Dim one As Action(Of Integer), two As Action(Of Integer), calls As Integer
        first.Rebind(Function(cb)
                         one = cb
                         Return Sub() two(2)
                     End Function, Sub(v) calls += 1)
        second.Rebind(Function(cb)
                          two = cb
                          Return Sub() one(1)
                      End Function, Sub(v) calls += 1)
        first.Invalidate() : second.Invalidate() : first.Dispose() : second.Dispose()
        Assert.AreEqual(0, calls)
    End Sub

    <TestMethod> Public Sub NullEventTargetCanBeHandledWithoutWriting()
        Dim lease As New RefEditEventLease(Of Object), saved As Action(Of Object), writes As Integer
        lease.Rebind(Function(cb)
                         saved = cb
                         Return Sub() Return
                     End Function, Sub(value)
                                       If value IsNot Nothing Then writes += 1
                                   End Sub)
        saved(Nothing) : lease.Dispose() : Assert.AreEqual(0, writes)
    End Sub

#If Not C10_ISOLATED Then
    ' These tests invoke actual shipping managed entrypoints without opening a
    ' form or Excel. Injected delegates are not COM connection point providers.
    Private Shared Sub OnSta(body As Action)
        Dim failure As Exception = Nothing
        Dim thread As New System.Threading.Thread(Sub()
                                                      Try
                                                          body()
                                                      Catch ex As Exception
                                                          failure = ex
                                                      End Try
                                                  End Sub)
        thread.IsBackground = True
        thread.SetApartmentState(System.Threading.ApartmentState.STA)
        thread.Start()
        Assert.IsTrue(thread.Join(10000), "Managed boundary test exceeded deadline")
        If failure IsNot Nothing Then Throw failure
    End Sub

    Private Shared Function Field(control As Excel2007RefEdit, name As String) As Object
        Return GetType(Excel2007RefEdit).GetField(name, Reflection.BindingFlags.NonPublic Or Reflection.BindingFlags.Instance).GetValue(control)
    End Function

    Private Shared Sub InvokeHandler(control As Excel2007RefEdit, name As String, ParamArray args As Object())
        GetType(Excel2007RefEdit).GetMethod(name, Reflection.BindingFlags.NonPublic Or Reflection.BindingFlags.Instance).Invoke(control, args)
    End Sub

    Private Shared Function Arm(control As Excel2007RefEdit, name As String, throws As Boolean, calls As Action) As Action
        Dim lease = Field(control, name)
        Return DirectCast(GetType(MigrationRefEditBoundary10Tests).GetMethod("ArmTyped", Reflection.BindingFlags.NonPublic Or Reflection.BindingFlags.Static).
                          MakeGenericMethod(lease.GetType().GetGenericArguments()).Invoke(Nothing, {lease, throws, calls}), Action)
    End Function

    Private Shared Function ArmTyped(Of T)(value As Object, throws As Boolean, calls As Action) As Action
        Dim lease = DirectCast(value, RefEditEventLease(Of T)), saved As Action(Of T) = Nothing
        lease.Rebind(Function(cb)
                         saved = cb
                         Return Sub()
                                    If throws Then Throw New InvalidOperationException("Injected disconnected detach")
                                End Sub
                     End Function, Sub(v) calls())
        Return Sub() saved(Nothing)
    End Function

    Private Shared Sub AssertSafe(control As Excel2007RefEdit)
        Assert.IsTrue(control.CaptureRequiresRecreation)
        Assert.IsFalse(CBool(Field(control, "pbInTextAdressHandler")))
        Assert.AreEqual("'Data'!$A$1", control.Address)
        For Each name In {"xlSheet", "xlBook", "xlApp", "_ExcelConnector"}
            Assert.IsNull(Field(control, name), name)
        Next
    End Sub

    <TestMethod> Public Sub ActualFocusEnterContainsThrowingClearAndPreservesAddress()
        OnSta(Sub()
                  Using control As New Excel2007RefEdit
                      control.Address = "'Data'!$A$1"
                      Dim calls As Integer, late = Arm(control, "selectionLease", True, Sub() calls += 1)
                      InvokeHandler(control, "txtAddress_Enter", Nothing, System.EventArgs.Empty)
                      AssertSafe(control) : late() : Assert.AreEqual(0, calls)
                      Dim ran As Boolean
                      control.RunCaptureBoundary("retry", Sub() ran = True)
                      Assert.IsFalse(ran)
                  End Using
              End Sub)
    End Sub

    <TestMethod> Public Sub ActualConnectorSetterContainsThrowingClear()
        OnSta(Sub()
                  Using control As New Excel2007RefEdit
                      control.Address = "'Data'!$A$1"
                      Dim late = Arm(control, "selectionLease", True, Sub() Assert.Fail("Stale callback"))
                      GetType(Excel2007RefEdit).GetProperty("ExcelConnector").SetValue(control, Nothing)
                      AssertSafe(control) : late()
                  End Using
              End Sub)
    End Sub

    <TestMethod> Public Sub ActualConnectorSetterContainsWorkbookDetachFailure()
        OnSta(Sub()
                  Using control As New Excel2007RefEdit
                      control.Address = "'Data'!$A$1"
                      Dim late = Arm(control, "workbookLease", True, Sub() Assert.Fail("Stale activation"))
                      GetType(Excel2007RefEdit).GetProperty("ExcelConnector").SetValue(control, Nothing)
                      AssertSafe(control) : late()
                  End Using
              End Sub)
    End Sub

    <TestMethod> Public Sub ActualLeaveCleansArmedLeaseWithNullSheetAndConnector()
        OnSta(Sub()
                  Using control As New Excel2007RefEdit
                      Dim calls As Integer, late = Arm(control, "selectionLease", False, Sub() calls += 1)
                      Assert.IsNull(Field(control, "xlSheet"))
                      InvokeHandler(control, "txtAddress_Leave", Nothing, System.EventArgs.Empty)
                      late() : Assert.AreEqual(0, calls)
                      Assert.IsFalse(control.CaptureRequiresRecreation)
                      Assert.IsFalse(CBool(Field(control, "pbInTextAdressHandler")))
                  End Using
              End Sub)
    End Sub

    <TestMethod> Public Sub ActualLeaveContainsThrowingDetachWithNullSheet()
        OnSta(Sub()
                  Using control As New Excel2007RefEdit
                      control.Address = "'Data'!$A$1"
                      Arm(control, "selectionLease", True, Sub() Assert.Fail())
                      InvokeHandler(control, "txtAddress_Leave", Nothing, System.EventArgs.Empty)
                      AssertSafe(control)
                  End Using
              End Sub)
    End Sub

    <TestMethod> Public Sub ActualDisposeTwiceClosesBothSourcesAndClearsReferences()
        OnSta(Sub()
                  Dim control As New Excel2007RefEdit
                  control.Address = "'Data'!$A$1"
                  Dim calls As Integer, first = Arm(control, "selectionLease", True, Sub() calls += 1)
                  Dim second = Arm(control, "workbookLease", True, Sub() calls += 1)
                  control.Dispose() : control.Dispose() : first() : second()
                  Assert.AreEqual(0, calls) : Assert.IsTrue(control.CaptureRequiresRecreation)
                  For Each name In {"xlSheet", "xlBook", "xlApp", "_ExcelConnector"}
                      Assert.IsNull(Field(control, name), name)
                  Next
              End Sub)
    End Sub

    <TestMethod> Public Sub ActualNullTargetDoesNotChangeAddressOrRaiseChanged()
        OnSta(Sub()
                  Using control As New Excel2007RefEdit
                      control.Address = "'Data'!$A$1"
                      Dim writes As Integer
                      AddHandler control.Changed, Sub(sender, args) writes += 1
                      GetType(Excel2007RefEdit).GetField("pbInTextAdressHandler", Reflection.BindingFlags.NonPublic Or Reflection.BindingFlags.Instance).SetValue(control, True)
                      InvokeHandler(control, "SelectionChange", New Object() {Nothing})
                      Assert.AreEqual(0, writes) : Assert.AreEqual("'Data'!$A$1", control.Address)
                  End Using
              End Sub)
    End Sub

    <TestMethod> Public Sub ActualBoundaryOperationFailureIsObservableAndDoesNotRetry()
        OnSta(Sub()
                  Using control As New Excel2007RefEdit
                      control.Address = "'Data'!$A$1"
                      control.RunCaptureBoundary("injected provider", Sub() Throw New InvalidOperationException("provider"))
                      AssertSafe(control)
                  End Using
              End Sub)
    End Sub

    <TestMethod> Public Sub ActualReentrantBoundaryCannotReactivateCapture()
        OnSta(Sub()
                  Using control As New Excel2007RefEdit
                      control.Address = "'Data'!$A$1"
                      control.RunCaptureBoundary("outer", Sub()
                                                              control.RunCaptureBoundary("inner", Sub() Assert.Fail())
                                                              GetType(Excel2007RefEdit).GetField("pbInTextAdressHandler", Reflection.BindingFlags.NonPublic Or Reflection.BindingFlags.Instance).SetValue(control, True)
                                                          End Sub)
                      AssertSafe(control)
                  End Using
              End Sub)
    End Sub

    <TestMethod> Public Sub ActualNullSheetActivationClearsOldSelectionBeforeReturning()
        OnSta(Sub()
                  Using control As New Excel2007RefEdit
                      Dim calls As Integer, late = Arm(control, "selectionLease", False, Sub() calls += 1)
                      GetType(Excel2007RefEdit).GetField("pbInTextAdressHandler", Reflection.BindingFlags.NonPublic Or Reflection.BindingFlags.Instance).SetValue(control, True)
                      InvokeHandler(control, "xlSheetChangeHandle", New Object() {Nothing})
                      late() : Assert.AreEqual(0, calls)
                      Assert.IsNull(Field(control, "xlSheet"))
                      Assert.IsFalse(CBool(Field(control, "pbInTextAdressHandler")))
                  End Using
              End Sub)
    End Sub
#End If

    <TestMethod> Public Sub SavedUiGenerationCannotApplyAfterClearOrNewWorkbook()
        Dim lease As New RefEditEventLease(Of Object)
        lease.Rebind(Function(cb) Sub() Return, Sub(value) Return)
        Dim old = lease.CurrentGeneration
        Assert.IsTrue(lease.IsCurrent(old))
        lease.Clear()
        lease.Rebind(Function(cb) Sub() Return, Sub(value) Return)
        Assert.IsFalse(lease.IsCurrent(old))
        Assert.IsTrue(lease.IsCurrent(lease.CurrentGeneration))
        lease.Dispose()
    End Sub
End Class
