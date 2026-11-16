Option Strict On
Option Explicit On
Imports System
Imports System.Collections.Generic
Imports System.Drawing
Imports System.Globalization
Imports BESHStatNG.Test.Harness46
Namespace Harness47
 Friend Class FakeControl47
  Inherits FakeControl46
  Friend Style As Integer, DropDownStyle As Integer
  Private selected As Integer = -1
  Friend Shadows Property SelectedIndex As Integer
   Get
    Return selected
   End Get
   Set(value As Integer)
    Dim changed = value <> selected
    selected = value
    SelectedItem = If(value >= 0 AndAlso value < Items.Count, Items(Math.Max(0, value)), Nothing)
    Text = If(SelectedItem Is Nothing, "", CStr(SelectedItem))
    If changed Then RaiseNamed("SelectedIndexChanged")
   End Set
  End Property
  Friend Sub BeginUpdate()
  End Sub
  Friend Sub EndUpdate()
  End Sub
  Friend Function FindStringExact(value As String) As Integer
   For i = 0 To Items.Count - 1
    If String.Equals(CStr(Items(i)), value, StringComparison.CurrentCulture) Then Return i
   Next
   Return -1
  End Function
 End Class
 Friend Module FakeText47
  Friend Sub setTextBoxProperties(control As FakeControl47, color As Color, text As String)
   control.BackColor = color : control.Text = text
  End Sub
    Public Function TryParseUiInteger(text As String, ByRef value As Integer) As Boolean
        Dim d As Double
        If Not SourceNumbers46.TryParseUiDouble(text, d) Then
            value = 0
            Return False
        End If

        Return ExcelCommon.NumberTextPolicy.TryIntegerValue(d, value)
    End Function
    Public Function ParseUiInteger(text As String,
                               Optional fieldName As String = "integer value") As Integer
        Dim value As Integer
        If Not TryParseUiInteger(text, value) Then
            Throw New FormatException($"Cannot parse {fieldName}: ""{text}"".")
        End If
        Return value
    End Function
 End Module
 Friend Module FakeLogger47
  Friend ReadOnly Warnings As New List(Of String)
  Friend Sub Warn(message As String)
   Warnings.Add(message)
  End Sub
 End Module
 Friend Module FakeAcquisition47
  Friend Calls As Integer
  Friend Sub RefreshMMRM()
   Calls += 1
   Throw New InvalidOperationException("HOST_PENDING_MMRM_REFRESH")
  End Sub
 End Module
 Friend Enum FakeProgressStyle47
  Continuous
  Marquee
 End Enum
 Friend Class FakePump47
  Friend Calls As Integer, Fail As Boolean
  Friend Sub DoEvents()
   Calls += 1
   If Fail Then Throw New InvalidOperationException("FAKE_PUMP_FAILURE")
  End Sub
 End Class
 Friend Class FakeRegressionBridge47
  Friend Cancel As Func(Of Boolean), Interrupt As Func(Of Boolean), Sets As Integer, Clears As Integer
  Friend Sub SetCallbacks(cancellation As Func(Of Boolean), interruption As Func(Of Boolean))
   Sets += 1 : Cancel = cancellation : Interrupt = interruption
  End Sub
  Friend Sub ClearCallbacks()
   Clears += 1 : Cancel = Nothing : Interrupt = Nothing
  End Sub
 End Class
End Namespace
