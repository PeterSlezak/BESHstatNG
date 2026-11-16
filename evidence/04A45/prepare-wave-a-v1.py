from common45 import *
import re
lock()
source=(ROOT/'BESHstatNG/BESHStatAddIn.vb').read_text(encoding='utf-8-sig')
body=source[source.index('Public Class Ribbon'):]
subs=[('Public Class Ribbon','Friend Class RibbonProjection45'),('Inherits ExcelRibbon','Inherits FakeRibbonBase45'),('My.Resources.RibbonXML','FakeTrace45.RibbonXml'),('AppGlobals.','FakeGlobals45.'),('HelpLinks.','FakeHelp45.'),('ExcelDna.Integration.ExcelAsyncUtil.','FakeAsync45.'),('ExcelDnaUtil.','FakeDna45.'),('MessageBox.Show','FakeMessage45.Show'),('MessageBoxButtons.OK','0'),('MessageBoxIcon.Information','0'),('ExcelWindowWrapper','FakeWindow45'),(' As Worksheet',' As FakeSheet45')]
for a,b in subs:body=body.replace(a,b)
forms=sorted(set(re.findall(r'New (Ui\w+)\(',body)))
stub='''Option Strict On
Option Explicit On
Imports System
Imports System.Collections.Generic
Imports ExcelDna.Integration.CustomUI
Namespace Harness45
Friend Module FakeTrace45
 Friend ReadOnly Events As New List(Of String)
 Friend RibbonXml As String
 Friend FailAt As String
 Friend Sub Record(value As String)
  Events.Add(value)
  If FailAt = value Then Throw New InvalidOperationException("synthetic fault")
 End Sub
 Friend Sub Reset()
  Events.Clear() : FailAt = Nothing : FakeGlobals45.app = New FakeApp45()
  FakeDna45.WindowHandle = New IntPtr(45) : FakeForm45.Charts = True
 End Sub
End Module
Friend Class FakeRibbonBase45
 Public Overridable Function GetCustomUI(id As String) As String
  Return Nothing
 End Function
End Class
Friend Class FakeSheet45
End Class
Friend Class FakeBooks45
 Friend Count As Integer = 1
 Friend Sub Add()
  FakeTrace45.Record("Workbooks.Add") : Count += 1
 End Sub
End Class
Friend Class FakeApp45
 Friend Workbooks As New FakeBooks45()
 Friend ActiveSheet As New FakeSheet45()
End Class
Friend Module FakeGlobals45
 Friend app As New FakeApp45()
End Module
Friend Module FakeDna45
 Friend WindowHandle As IntPtr
End Module
Friend Module FakeAsync45
 Friend Sub QueueAsMacro(action As Action)
  FakeTrace45.Record("Queue") : action()
 End Sub
End Module
Friend Module FakeHelp45
 Friend Const BaseUrl As String = "synthetic-help"
 Friend Sub OpenUrl(value As String)
  FakeTrace45.Record("Help:" & value)
 End Sub
End Module
Friend Module FakeMessage45
 Friend Sub Show(ParamArray args() As Object)
  FakeTrace45.Record("Message")
 End Sub
End Module
Friend Class FakeWindow45
 Friend Sub New(value As IntPtr)
  FakeTrace45.Record("Window")
 End Sub
End Class
Friend Class FakeForm45
 Friend Shared Charts As Boolean = True
 Friend Sub New(ParamArray args() As Object)
  FakeTrace45.Record("Construct:" & Me.GetType().Name)
 End Sub
 Friend Sub Show(ParamArray args() As Object)
  FakeTrace45.Record("Show:" & Me.GetType().Name)
 End Sub
 Friend Sub Populate(sheet As FakeSheet45)
  FakeTrace45.Record("Populate")
 End Sub
 Friend Shared Function WorkbookHasAnyCharts(app As FakeApp45) As Boolean
  FakeTrace45.Record("Charts") : Return Charts
 End Function
End Class
'''
for f in forms:
 stub+=f'Friend Class {f}\n Inherits FakeForm45\n Friend Sub New(ParamArray args() As Object)\n  MyBase.New(args)\n End Sub\nEnd Class\n'
projection=ROOT/'BESHStatNG.Test/src/RibbonProjection45.vb'
with projection.open('x',encoding='utf-8',newline='\n') as f:f.write(stub+body+'\nEnd Namespace\n')
save('fixtures/ribbon-projection-v1.json',dict(kind='CURRENT_MAIN_PRE_EDIT_04A45_SOURCE_DERIVED_FAKE',source=pin('BESHstatNG/BESHStatAddIn.vb'),projection=pin(projection),exact_class_slice_sha256=sha(source[source.index('Public Class Ribbon'):].encode()),substitutions=subs,fake_forms=forms,registration='Real ExcelRibbon metadata checked separately;source projection has only closed fake dependencies',native='NOT_RUN',scientific='NOT_GOLDEN'))
print('Ribbon source projection prepared:',len(forms),'fake form types',flush=True)
