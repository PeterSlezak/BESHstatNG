Option Strict On
Option Explicit On
Option Infer On
Imports System

' Review-only Windows coordinator. Not included in either shipping project.
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

    Friend Sub Clear()
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
            Clear() ' Invalidate before any potentially reentrant detach/provider call.
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
        Clear()
    End Sub
End Class
