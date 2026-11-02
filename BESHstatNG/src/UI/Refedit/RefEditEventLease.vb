Option Strict On
Option Explicit On
Option Infer On
Imports System

' Shipping Windows event subscription owner. Excel RCWs remain borrowed.
' STA/reentrant lifetime guard; no cross-thread synchronization or COM release.
Friend NotInheritable Class RefEditEventLease(Of T)
    Implements IDisposable

    Private generation As Long
    Private armed As Long = -1
    Private disposed As Boolean
    Private faulted As Boolean
    Private transitioning As Boolean
    Private detach As Action

    Friend ReadOnly Property IsClosed As Boolean
        Get
            Return disposed OrElse faulted
        End Get
    End Property

    Friend ReadOnly Property CurrentGeneration As Long
        Get
            Return generation
        End Get
    End Property

    Friend Function IsCurrent(value As Long) As Boolean
        Return Not IsClosed AndAlso value = generation AndAlso armed = value
    End Function

    Friend Sub Invalidate()
        generation += 1
        armed = -1
    End Sub

    Friend Sub Clear()
        If transitioning Then
            Invalidate()
            faulted = True
            Throw New InvalidOperationException("Reentrant event clear")
        End If
        transitioning = True
        Try
            ClearCore()
            If faulted Then Throw New InvalidOperationException("Faulted event lease")
        Finally
            transitioning = False
        End Try
    End Sub

    Private Sub ClearCore()
        generation += 1
        armed = -1
        Dim previous = detach
        detach = Nothing
        If previous Is Nothing Then Return
        Try
            previous()
        Catch
            faulted = True
            Throw
        End Try
    End Sub

    Friend Sub Rebind(subscribe As Func(Of Action(Of T), Action), callback As Action(Of T))
        If disposed OrElse faulted Then Throw New InvalidOperationException("Closed event lease")
        If transitioning Then
            generation += 1
            armed = -1
            faulted = True
            Throw New InvalidOperationException("Reentrant event bind")
        End If
        transitioning = True
        Try
            If subscribe Is Nothing Then Throw New ArgumentNullException(NameOf(subscribe))
            If callback Is Nothing Then Throw New ArgumentNullException(NameOf(callback))
            ClearCore() ' Invalidate before any potentially reentrant detach/provider call.
            If IsClosed Then Throw New InvalidOperationException("Context closed during detach")
            Dim captured = generation
            Dim cleanup = subscribe(Sub(value)
                                        If disposed OrElse faulted OrElse captured <> generation OrElse armed <> captured Then Return
                                        Try
                                            callback(value)
                                        Catch
                                            armed = -1
                                            faulted = True
                                            Throw
                                        End Try
                                    End Sub)
            If cleanup Is Nothing Then Throw New InvalidOperationException("Missing detach ownership")
            If disposed OrElse faulted OrElse generation <> captured Then
                cleanup()
                Throw New InvalidOperationException("Context changed during subscription")
            End If
            detach = cleanup
            armed = captured
        Catch
            armed = -1
            faulted = True
            Throw
        Finally
            transitioning = False
        End Try
    End Sub

    Public Sub Dispose() Implements IDisposable.Dispose
        If disposed Then Return
        disposed = True ' Even a throwing detach cannot reactivate callbacks.
        ClearCore()
    End Sub
End Class
