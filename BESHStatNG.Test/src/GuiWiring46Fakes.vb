Option Strict On
Option Explicit On
Imports System
Imports System.Collections.Generic
Imports System.Drawing
Namespace Harness46
 Friend Module FakeEvents46
  Friend ReadOnly Delivered As New List(Of String)
 End Module
 Friend Class FakeControl46
  Friend Enabled As Boolean = True, Visible As Boolean = True, CanFocus As Boolean = True
  Public Text As String = "", Selections As Integer, SelectedItem As Object
  Friend Minimum As Decimal, Maximum As Decimal = 100, DecimalPlaces As Integer, Increment As Decimal = 1
  Public BackColor As Color
  Friend ReadOnly Items As New List(Of Object)
  Private checkedValue As Boolean, indexValue As Integer = -1, numericValue As Decimal
  Private ReadOnly handlersClick As New List(Of EventHandler)
  Friend Custom Event Click As EventHandler
   AddHandler(value As EventHandler)
    handlersClick.Add(value)
   End AddHandler
   RemoveHandler(value As EventHandler)
    handlersClick.Remove(value)
   End RemoveHandler
   RaiseEvent(sender As Object, e As System.EventArgs)
    For Each handler In handlersClick.ToArray()
     FakeEvents46.Delivered.Add(handler.Method.DeclaringType.Name & "." & handler.Method.Name)
     handler(sender, e)
    Next
   End RaiseEvent
  End Event
  Private ReadOnly handlersCheckedChanged As New List(Of EventHandler)
  Friend Custom Event CheckedChanged As EventHandler
   AddHandler(value As EventHandler)
    handlersCheckedChanged.Add(value)
   End AddHandler
   RemoveHandler(value As EventHandler)
    handlersCheckedChanged.Remove(value)
   End RemoveHandler
   RaiseEvent(sender As Object, e As System.EventArgs)
    For Each handler In handlersCheckedChanged.ToArray()
     FakeEvents46.Delivered.Add(handler.Method.DeclaringType.Name & "." & handler.Method.Name)
     handler(sender, e)
    Next
   End RaiseEvent
  End Event
  Private ReadOnly handlersSelectedIndexChanged As New List(Of EventHandler)
  Friend Custom Event SelectedIndexChanged As EventHandler
   AddHandler(value As EventHandler)
    handlersSelectedIndexChanged.Add(value)
   End AddHandler
   RemoveHandler(value As EventHandler)
    handlersSelectedIndexChanged.Remove(value)
   End RemoveHandler
   RaiseEvent(sender As Object, e As System.EventArgs)
    For Each handler In handlersSelectedIndexChanged.ToArray()
     FakeEvents46.Delivered.Add(handler.Method.DeclaringType.Name & "." & handler.Method.Name)
     handler(sender, e)
    Next
   End RaiseEvent
  End Event
  Private ReadOnly handlersValueChanged As New List(Of EventHandler)
  Friend Custom Event ValueChanged As EventHandler
   AddHandler(value As EventHandler)
    handlersValueChanged.Add(value)
   End AddHandler
   RemoveHandler(value As EventHandler)
    handlersValueChanged.Remove(value)
   End RemoveHandler
   RaiseEvent(sender As Object, e As System.EventArgs)
    For Each handler In handlersValueChanged.ToArray()
     FakeEvents46.Delivered.Add(handler.Method.DeclaringType.Name & "." & handler.Method.Name)
     handler(sender, e)
    Next
   End RaiseEvent
  End Event
  Friend Property Checked As Boolean
   Get
    Return checkedValue
   End Get
   Set(value As Boolean)
    If value = checkedValue Then Return
    checkedValue = value : RaiseEvent CheckedChanged(Me, System.EventArgs.Empty)
   End Set
  End Property
  Friend Property SelectedIndex As Integer
   Get
    Return indexValue
   End Get
   Set(value As Integer)
    If value = indexValue Then Return
    indexValue = value
    If value >= 0 AndAlso value < Items.Count Then SelectedItem = Items(value)
    RaiseEvent SelectedIndexChanged(Me, System.EventArgs.Empty)
   End Set
  End Property
  Friend Property Value As Decimal
   Get
    Return numericValue
   End Get
   Set(value As Decimal)
    If value = numericValue Then Return
    numericValue = value : RaiseEvent ValueChanged(Me, System.EventArgs.Empty)
   End Set
  End Property
  Friend Sub FireClick()
   RaiseEvent Click(Me, System.EventArgs.Empty)
  End Sub
  Friend Function UserClick() As Boolean
   If Not Enabled Then Return False
   FireClick() : Return True
  End Function
  Friend Sub RaiseNamed(name As String)
   Select Case name
    Case "Click" : RaiseEvent Click(Me, System.EventArgs.Empty)
    Case "CheckedChanged" : RaiseEvent CheckedChanged(Me, System.EventArgs.Empty)
    Case "SelectedIndexChanged" : RaiseEvent SelectedIndexChanged(Me, System.EventArgs.Empty)
    Case "ValueChanged" : RaiseEvent ValueChanged(Me, System.EventArgs.Empty)
    Case Else : Throw New ArgumentException("Unsupported fake event")
   End Select
  End Sub
  Friend Sub FireChanged()
   RaiseEvent CheckedChanged(Me, System.EventArgs.Empty)
   RaiseEvent SelectedIndexChanged(Me, System.EventArgs.Empty)
   RaiseEvent ValueChanged(Me, System.EventArgs.Empty)
  End Sub
  Friend Sub [Select]()
   Selections += 1
  End Sub
  Friend Sub AppendText(value As String)
   Text &= value
  End Sub
 End Class
 Friend Class FakeStringControl46
  Inherits FakeControl46
  Friend Shadows Property SelectedItem As String
   Get
    Return CStr(MyBase.SelectedItem)
   End Get
   Set(value As String)
    MyBase.SelectedItem = value
   End Set
  End Property
 End Class
 Friend Class FakeRefEdit46
  Inherits FakeControl46
  Friend Address As String = "synthetic-selection"
  Friend txtAddress As New FakeControl46()
 End Class
 Friend Module FakeMessages46
  Friend ReadOnly Messages As New List(Of String)
  Friend Response As Integer = 1
  Friend Function Record(value As Object, Optional buttons As Object = Nothing, Optional title As Object = Nothing) As Integer
   Messages.Add(CStr(value)) : Return Response
  End Function
 End Module
 Friend Module FakeRanges46
  Friend ReadOnly Calls As New List(Of Tuple(Of String, Boolean))
  Friend ReadOnly Reject As New HashSet(Of String)(StringComparer.Ordinal)
  Friend Function CheckRefEdit(address As String, Optional oneColumn As Boolean = False) As Boolean
   Calls.Add(Tuple.Create(address, oneColumn)) : Return Reject.Contains(address)
  End Function
  Friend Sub RefEditReset(control As FakeRefEdit46)
   control.Address = "" : control.txtAddress.Text = "" : control.txtAddress.Select()
  End Sub
  Friend Sub Reset()
   Calls.Clear() : Reject.Clear() : FakeMessages46.Messages.Clear()
  End Sub
 End Module
End Namespace
