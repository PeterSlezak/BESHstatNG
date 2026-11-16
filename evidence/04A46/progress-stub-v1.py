from common46 import *
import re
lock();path='BESHstatNG/src/AppInfrastructure/ExcelDnaHostInfrastructure.vb';old=(PRIVATE/'source-preimages'/path).read_bytes().decode('utf-8-sig');now=(ROOT/path).read_bytes().decode('utf-8-sig')
def fragment(s,name):return re.search(r'    Public NotInheritable Class WinFormsProgressReporter[\s\S]*?    End Class',s)[0].replace('WinFormsProgressReporter',name)
source='''Option Strict On
Imports System
Imports System.Collections.Generic
Imports BESHStatNG.AppInfrastructure
Imports Forms = Stubs
Public Module Trace44
 Public ReadOnly Events As New List(Of String)
End Module
Namespace Stubs
 Public Class Control
  Public Property InvokeRequired As Boolean
  Public Sub Invoke(action As Action)
   Trace44.Events.Add("invoke") : action()
  End Sub
 End Class
 Public Class ProgressBar
  Inherits Control
  Public Property Value As Integer
   Get
    Return 0
   End Get
   Set(value As Integer)
    Trace44.Events.Add("bar:" & value.ToString(Globalization.CultureInfo.InvariantCulture))
   End Set
  End Property
 End Class
 Public Class Label
  Inherits Control
  Public Property Text As String
   Get
    Return ""
   End Get
   Set(value As String)
    Trace44.Events.Add("label:" & value)
   End Set
  End Property
 End Class
 Public Class Application
  Public Shared Sub DoEvents()
   Trace44.Events.Add("pump")
  End Sub
 End Class
End Namespace
'''+fragment(old,'PreReporter')+'\n'+fragment(now,'PostReporter')+'''
Public Module Entry44
 Public Sub Main(args As String())
  Dim rows As New List(Of Object)
  For Each owner As Type In {GetType(PreReporter),GetType(PostReporter)}
   For mask As Integer = 0 To 3
    For Each invoke As Boolean In {False,True}
     For Each message As String In New String(){Nothing,"","fixed"}
      For Each percent As Integer In {-5,45,120}
       Dim bar As Stubs.ProgressBar=If((mask And 1)<>0,New Stubs.ProgressBar With {.InvokeRequired=invoke},Nothing)
       Dim label As Stubs.Label=If((mask And 2)<>0,New Stubs.Label With {.InvokeRequired=invoke},Nothing)
       Dim reporter=DirectCast(Activator.CreateInstance(owner,New Object(){bar,label}),IProgressReporter)
       Trace44.Events.Clear() : reporter.Report(percent,message)
       rows.Add(New With {.owner=owner.Name,.mask=mask,.invoke=invoke,.message=message,.percent=percent,.events=Trace44.Events.ToArray()})
      Next
     Next
    Next
   Next
  Next
  IO.File.WriteAllText(args(0),New Web.Script.Serialization.JavaScriptSerializer().Serialize(rows))
 End Sub
End Module
'''
folder=PRIVATE/'progress-source-stub-v1';folder.mkdir();code=folder/'SourceBackedProgress44.vb'
with code.open('x',encoding='utf-8',newline='\n') as f:f.write(source)
locator=Path(os.environ['ProgramFiles(x86)'])/'Microsoft Visual Studio/Installer/vswhere.exe';vbc=subprocess.check_output([str(locator),'-products','*','-version','[18.0,19.0)','-find','MSBuild/Current/Bin/Roslyn/vbc.exe'],text=True).splitlines()[0];framework=Path(os.environ['ProgramFiles(x86)'])/'Reference Assemblies/Microsoft/Framework/.NETFramework/v4.8'
for config in ['Debug','Release']:
 lib=PRIVATE/'compiled'/config/'lib';exe=folder/('Trace'+config+'.exe')
 refs=[framework/p for p in ['System.dll','System.Core.dll','System.Web.Extensions.dll','Facades/netstandard.dll']]+[lib/'BESHStat.Core.dll',lib/'BESHStat.Excel.Common.dll']
 r=command('progress-stub-'+config+'-compile',[vbc,'/nologo','/target:exe','/sdkpath:'+str(framework),'/out:'+str(exe),*[('/r:'+str(p)) for p in refs],str(code)]);assert r['exit_code']==0
 import shutil
 for p in lib.glob('*.dll'):shutil.copy2(p,folder/p.name)
 output=folder/(config+'.json');r=command('progress-stub-'+config+'-execute',[str(exe),str(output)]);assert r['exit_code']==0
 data=load(output);assert len(data)==144;left=[dict(r,owner='same') for r in data if r['owner']=='PreReporter'];right=[dict(r,owner='same') for r in data if r['owner']=='PostReporter'];assert left==right
 for row in data:
  expected=[]
  if row['mask']:
   if row['invoke']:expected+=['invoke']
   if row['mask']&1:expected+=['bar:'+str(max(0,min(100,row['percent'])))]
   if row['mask']&2 and row['message'] is not None:expected+=['label:'+row['message']]
   expected+=['pump']
  assert expected==row['events']
 save('fixtures/'+config+'-progress-stub-v1.json',dict(kind='LATE_SEALED_ENTRY_SOURCE_FRAGMENT_AND_CURRENT_DELEGATE_STUB_SIMULATION;NOT_PRE_EDIT_CAPTURE_OR_NATIVE_WINFORMS',cases=72,exact=True,expected_event_order_verified=True,source_before=pin(PRIVATE/'source-preimages'/path),source_after=pin(path),generated_stub=pin(code),raw=pin(output),transform='Only original class name and Forms alias substituted. Fake controls record Invoke/value/label/DoEvents;real UI controls/Excel never constructed or pumped.',live_Excel='NOT_RUN',gates=GATES))
print('72 source-backed progress traces/configuration exact;Invoke/label/pump ordering verified in explicit fake host,not native',flush=True)
