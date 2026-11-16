Option Strict On
Imports System
Imports System.Collections.Generic
Imports System.Reflection
Imports System.Web.Script.Serialization
Friend Class OfflineXyzControl46
 Friend Event ValueChanged As EventHandler
 Friend Event CheckedChanged As EventHandler
 Friend Event SelectedIndexChanged As EventHandler
 Friend Sub Fire()
  RaiseEvent ValueChanged(Me, System.EventArgs.Empty)
  RaiseEvent CheckedChanged(Me, System.EventArgs.Empty)
  RaiseEvent SelectedIndexChanged(Me, System.EventArgs.Empty)
 End Sub
End Class
Friend Class OfflineXyz46
 Friend pFigure As Object
 Friend Calls As Integer
 Private Sub Recalculate()
  Calls += 1
 End Sub
 Friend WithEvents spinBtnR As New OfflineXyzControl46()
 Friend WithEvents spinBtnG As New OfflineXyzControl46()
 Friend WithEvents spinBtnB As New OfflineXyzControl46()
 Friend WithEvents spinBtnLatitudeRings As New OfflineXyzControl46()
 Friend WithEvents spinBtnLongitudeRings As New OfflineXyzControl46()
 Friend WithEvents spinBtnPointsPerRing As New OfflineXyzControl46()
 Friend WithEvents ckScaleAxes As New OfflineXyzControl46()
 Friend WithEvents ckXreverseAxis As New OfflineXyzControl46()
 Friend WithEvents ckYreverseAxis As New OfflineXyzControl46()
 Friend WithEvents ckZreverseAxis As New OfflineXyzControl46()
 Friend WithEvents cbMarkerSymbol As New OfflineXyzControl46()
    Private Sub spinBtnR_ValueChanged(sender As Object, e As System.EventArgs) Handles spinBtnR.ValueChanged
        If pFigure IsNot Nothing Then Call Recalculate()
    End Sub
    Private Sub spinBtnG_ValueChanged(sender As Object, e As System.EventArgs) Handles spinBtnG.ValueChanged
        If pFigure IsNot Nothing Then Call Recalculate()
    End Sub
    Private Sub spinBtnB_ValueChanged(sender As Object, e As System.EventArgs) Handles spinBtnB.ValueChanged
        If pFigure IsNot Nothing Then Call Recalculate()
    End Sub
    Private Sub spinBtnLatitudeRings_ValueChanged(sender As Object, e As System.EventArgs) Handles spinBtnLatitudeRings.ValueChanged
        If pFigure IsNot Nothing Then Call Recalculate()
    End Sub
    Private Sub spinBtnLongitudeRings_ValueChanged(sender As Object, e As System.EventArgs) Handles spinBtnLongitudeRings.ValueChanged
        If pFigure IsNot Nothing Then Call Recalculate()
    End Sub
    Private Sub spinBtnPointsPerRing_ValueChanged(sender As Object, e As System.EventArgs) Handles spinBtnPointsPerRing.ValueChanged
        If pFigure IsNot Nothing Then Call Recalculate()
    End Sub
    Private Sub ckScaleAxes_CheckedChanged(sender As Object, e As System.EventArgs) Handles ckScaleAxes.CheckedChanged
        If pFigure IsNot Nothing Then Call Recalculate()
    End Sub
    Private Sub ckXreverseAxis_CheckedChanged(sender As Object, e As System.EventArgs) Handles ckXreverseAxis.CheckedChanged
        If pFigure IsNot Nothing Then Call Recalculate()
    End Sub
    Private Sub ckYreverseAxis_CheckedChanged(sender As Object, e As System.EventArgs) Handles ckYreverseAxis.CheckedChanged
        If pFigure IsNot Nothing Then Call Recalculate()
    End Sub
    Private Sub ckZreverseAxis_CheckedChanged(sender As Object, e As System.EventArgs) Handles ckZreverseAxis.CheckedChanged
        If pFigure IsNot Nothing Then Call Recalculate()
    End Sub
    Private Sub cbMarkerSymbol_SelectedIndexChanged(sender As Object, e As System.EventArgs) Handles cbMarkerSymbol.SelectedIndexChanged
        If pFigure IsNot Nothing Then Call Recalculate()
    End Sub
End Class
Public Module ObserveXyz46
 Sub Main(args As String())
  Dim rows As New List(Of Object)
  For state = 0 To 3
   Dim form As New OfflineXyz46()
   If state = 1 OrElse state = 2 Then form.pFigure = New Object()
   form.spinBtnR.Fire()
   If state = 2 Then form.spinBtnR.Fire()
   Dim expected = If(state = 2, 2, If(state = 1, 1, 0))
   If form.Calls <> expected Then Throw New InvalidOperationException("XYZ event routing mismatch")
   rows.Add(New With {.method = "spinBtnR_ValueChanged", .state = state, .calls = form.Calls, .expected = expected})
  Next
  For state = 0 To 3
   Dim form As New OfflineXyz46()
   If state = 1 OrElse state = 2 Then form.pFigure = New Object()
   form.spinBtnG.Fire()
   If state = 2 Then form.spinBtnG.Fire()
   Dim expected = If(state = 2, 2, If(state = 1, 1, 0))
   If form.Calls <> expected Then Throw New InvalidOperationException("XYZ event routing mismatch")
   rows.Add(New With {.method = "spinBtnG_ValueChanged", .state = state, .calls = form.Calls, .expected = expected})
  Next
  For state = 0 To 3
   Dim form As New OfflineXyz46()
   If state = 1 OrElse state = 2 Then form.pFigure = New Object()
   form.spinBtnB.Fire()
   If state = 2 Then form.spinBtnB.Fire()
   Dim expected = If(state = 2, 2, If(state = 1, 1, 0))
   If form.Calls <> expected Then Throw New InvalidOperationException("XYZ event routing mismatch")
   rows.Add(New With {.method = "spinBtnB_ValueChanged", .state = state, .calls = form.Calls, .expected = expected})
  Next
  For state = 0 To 3
   Dim form As New OfflineXyz46()
   If state = 1 OrElse state = 2 Then form.pFigure = New Object()
   form.spinBtnLatitudeRings.Fire()
   If state = 2 Then form.spinBtnLatitudeRings.Fire()
   Dim expected = If(state = 2, 2, If(state = 1, 1, 0))
   If form.Calls <> expected Then Throw New InvalidOperationException("XYZ event routing mismatch")
   rows.Add(New With {.method = "spinBtnLatitudeRings_ValueChanged", .state = state, .calls = form.Calls, .expected = expected})
  Next
  For state = 0 To 3
   Dim form As New OfflineXyz46()
   If state = 1 OrElse state = 2 Then form.pFigure = New Object()
   form.spinBtnLongitudeRings.Fire()
   If state = 2 Then form.spinBtnLongitudeRings.Fire()
   Dim expected = If(state = 2, 2, If(state = 1, 1, 0))
   If form.Calls <> expected Then Throw New InvalidOperationException("XYZ event routing mismatch")
   rows.Add(New With {.method = "spinBtnLongitudeRings_ValueChanged", .state = state, .calls = form.Calls, .expected = expected})
  Next
  For state = 0 To 3
   Dim form As New OfflineXyz46()
   If state = 1 OrElse state = 2 Then form.pFigure = New Object()
   form.spinBtnPointsPerRing.Fire()
   If state = 2 Then form.spinBtnPointsPerRing.Fire()
   Dim expected = If(state = 2, 2, If(state = 1, 1, 0))
   If form.Calls <> expected Then Throw New InvalidOperationException("XYZ event routing mismatch")
   rows.Add(New With {.method = "spinBtnPointsPerRing_ValueChanged", .state = state, .calls = form.Calls, .expected = expected})
  Next
  For state = 0 To 3
   Dim form As New OfflineXyz46()
   If state = 1 OrElse state = 2 Then form.pFigure = New Object()
   form.ckScaleAxes.Fire()
   If state = 2 Then form.ckScaleAxes.Fire()
   Dim expected = If(state = 2, 2, If(state = 1, 1, 0))
   If form.Calls <> expected Then Throw New InvalidOperationException("XYZ event routing mismatch")
   rows.Add(New With {.method = "ckScaleAxes_CheckedChanged", .state = state, .calls = form.Calls, .expected = expected})
  Next
  For state = 0 To 3
   Dim form As New OfflineXyz46()
   If state = 1 OrElse state = 2 Then form.pFigure = New Object()
   form.ckXreverseAxis.Fire()
   If state = 2 Then form.ckXreverseAxis.Fire()
   Dim expected = If(state = 2, 2, If(state = 1, 1, 0))
   If form.Calls <> expected Then Throw New InvalidOperationException("XYZ event routing mismatch")
   rows.Add(New With {.method = "ckXreverseAxis_CheckedChanged", .state = state, .calls = form.Calls, .expected = expected})
  Next
  For state = 0 To 3
   Dim form As New OfflineXyz46()
   If state = 1 OrElse state = 2 Then form.pFigure = New Object()
   form.ckYreverseAxis.Fire()
   If state = 2 Then form.ckYreverseAxis.Fire()
   Dim expected = If(state = 2, 2, If(state = 1, 1, 0))
   If form.Calls <> expected Then Throw New InvalidOperationException("XYZ event routing mismatch")
   rows.Add(New With {.method = "ckYreverseAxis_CheckedChanged", .state = state, .calls = form.Calls, .expected = expected})
  Next
  For state = 0 To 3
   Dim form As New OfflineXyz46()
   If state = 1 OrElse state = 2 Then form.pFigure = New Object()
   form.ckZreverseAxis.Fire()
   If state = 2 Then form.ckZreverseAxis.Fire()
   Dim expected = If(state = 2, 2, If(state = 1, 1, 0))
   If form.Calls <> expected Then Throw New InvalidOperationException("XYZ event routing mismatch")
   rows.Add(New With {.method = "ckZreverseAxis_CheckedChanged", .state = state, .calls = form.Calls, .expected = expected})
  Next
  For state = 0 To 3
   Dim form As New OfflineXyz46()
   If state = 1 OrElse state = 2 Then form.pFigure = New Object()
   form.cbMarkerSymbol.Fire()
   If state = 2 Then form.cbMarkerSymbol.Fire()
   Dim expected = If(state = 2, 2, If(state = 1, 1, 0))
   If form.Calls <> expected Then Throw New InvalidOperationException("XYZ event routing mismatch")
   rows.Add(New With {.method = "cbMarkerSymbol_SelectedIndexChanged", .state = state, .calls = form.Calls, .expected = expected})
  Next
  IO.File.WriteAllText(args(0), New JavaScriptSerializer().Serialize(rows))
 End Sub
End Module
