from common46 import *
lock();p=ROOT/'BESHStatNG.Test/src/GuiWiring46Fakes.vb';s=p.read_text()
for name in ['Click','CheckedChanged','SelectedIndexChanged','ValueChanged']:
 a='  Friend Event '+name+' As EventHandler'
 b=f'''  Private ReadOnly handlers{name} As New List(Of EventHandler)
  Friend Custom Event {name} As EventHandler
   AddHandler(value As EventHandler)
    handlers{name}.Add(value)
   End AddHandler
   RemoveHandler(value As EventHandler)
    handlers{name}.Remove(value)
   End RemoveHandler
   RaiseEvent(sender As Object, e As System.EventArgs)
    For Each handler In handlers{name}.ToArray()
     FakeEvents46.Delivered.Add(handler.Method.DeclaringType.Name & "." & handler.Method.Name)
     handler(sender, e)
    Next
   End RaiseEvent
  End Event'''
 assert a in s;s=s.replace(a,b)
s=s.replace('  Friend Text As String','  Public Text As String').replace('  Friend BackColor As Color','  Public BackColor As Color')
s=s.replace('  Friend Sub FireChanged()', '''  Friend Sub RaiseNamed(name As String)
   Select Case name
    Case "Click" : RaiseEvent Click(Me, System.EventArgs.Empty)
    Case "CheckedChanged" : RaiseEvent CheckedChanged(Me, System.EventArgs.Empty)
    Case "SelectedIndexChanged" : RaiseEvent SelectedIndexChanged(Me, System.EventArgs.Empty)
    Case "ValueChanged" : RaiseEvent ValueChanged(Me, System.EventArgs.Empty)
    Case Else : Throw New ArgumentException("Unsupported fake event")
   End Select
  End Sub
  Friend Sub FireChanged()''')
s=s.replace(' Friend Class FakeControl46',' Friend Module FakeEvents46\n  Friend ReadOnly Delivered As New List(Of String)\n End Module\n Friend Class FakeControl46',1)
p.write_text(s,encoding='utf-8',newline='\n')
