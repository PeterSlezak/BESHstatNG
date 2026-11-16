from common46 import *
import re,collections
lock();path='BESHstatNG/src/UI/Ui3XYZplot.vb';source=(ROOT/path).read_text(encoding='utf-8-sig')
names=['spinBtnR_ValueChanged','spinBtnG_ValueChanged','spinBtnB_ValueChanged','spinBtnLatitudeRings_ValueChanged','spinBtnLongitudeRings_ValueChanged','spinBtnPointsPerRing_ValueChanged','ckScaleAxes_CheckedChanged','ckXreverseAxis_CheckedChanged','ckYreverseAxis_CheckedChanged','ckZreverseAxis_CheckedChanged','cbMarkerSymbol_SelectedIndexChanged']
blocks=[];records=[]
for name in names:
 m=re.search(r'(?ms)^    Private Sub '+name+r'\(.*?^    End Sub',source);assert m
 binding=re.search(r'Handles\s+(\w+)\.(\w+)',m.group());assert binding
 blocks.append(m.group());records.append(dict(owner='Ui3XYZplot',method=name,source=pin(path),original_text=m.group(),source_text_sha256=sha(m.group().encode()),control=binding[1],event=binding[2]))
code='''Option Strict On
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
'''
for r in records:code+=' Friend WithEvents '+r['control']+' As New OfflineXyzControl46()\n'
code+='\n'.join(blocks)+'\nEnd Class\nPublic Module ObserveXyz46\n Sub Main(args As String())\n  Dim rows As New List(Of Object)\n'
for r in records:
 name=r['control'];code+=f'''  For state = 0 To 3
   Dim form As New OfflineXyz46()
   If state = 1 OrElse state = 2 Then form.pFigure = New Object()
   form.{name}.Fire()
   If state = 2 Then form.{name}.Fire()
   Dim expected = If(state = 2, 2, If(state = 1, 1, 0))
   If form.Calls <> expected Then Throw New InvalidOperationException("XYZ event routing mismatch")
   rows.Add(New With {{.method = "{r['method']}", .state = state, .calls = form.Calls, .expected = expected}})
  Next
'''
code+='  IO.File.WriteAllText(args(0), New JavaScriptSerializer().Serialize(rows))\n End Sub\nEnd Module\n'
p=HERE/'fixtures/ObserveXyz46.vb'
with p.open('x',encoding='utf-8',newline='\n') as f:f.write(code)
folder=PRIVATE/'xyz-observer-v1';folder.mkdir();locator=Path(os.environ['ProgramFiles(x86)'])/'Microsoft Visual Studio/Installer/vswhere.exe';vbc=subprocess.check_output([str(locator),'-products','*','-version','[18.0,19.0)','-find','MSBuild/Current/Bin/Roslyn/vbc.exe'],text=True).splitlines()[0];framework=Path(os.environ['ProgramFiles(x86)'])/'Reference Assemblies/Microsoft/Framework/.NETFramework/v4.8';results=[]
for c in ['Debug','Release']:
 exe=folder/(c+'.exe');r=command('XYZ-observer-'+c+'-compile',[vbc,'/nologo','/target:exe','/sdkpath:'+str(framework),'/out:'+str(exe),'/optimize'+('+' if c=='Release' else '-'),*[('/r:'+str(framework/f)) for f in ['System.dll','System.Core.dll','System.Web.Extensions.dll']],str(p)]);assert not r['exit_code']
 out=folder/(c+'.json');r=command('XYZ-observer-'+c+'-execute',[str(exe),str(out)]);assert not r['exit_code'];rows=load(out);assert len(rows)==44;results.append(dict(configuration=c,rows=rows,raw=pin(out)))
assert results[0]['rows']==results[1]['rows']
save('fixtures/xyz-observer-v1.json',dict(kind='CURRENT_MAIN_PRE_EDIT_04A46_EXACT_SOURCE_EVENT_OBSERVER_NOT_NATIVE',members=records,projection=pin(p),results=results,cases_per_configuration=44,new_managed_identities=0,frozen_test_project_changed=False,qualification='Eleven additional pure XYZ option event bodies copied unchanged into an isolated net48 executable with fake event controls and Recalculate sink. No real figure/WinForms/Excel object. Full managed343 test generation remains unchanged.',native='NOT_RUN'))
d=load(HERE/'scope/coverage-v2.json');selected=set(names)
for r in d['members']:
 if r['owner']=='BESHStatNG.Ui3XYZplot' and r['name'] in selected:r.update(classification='OFFLINE_FAKE_EXECUTED',evidence='evidence/04A46/fixtures/xyz-observer-v1.json',proof='Actual copied Handles event raised in isolated closed fake;44 observed/asserted no-figure/figure/repeated-event vectors/config;not a new managed-test identity.')
d.update(counts=dict(collections.Counter(r['classification'] for r in d['members'])),source_projected_methods=162,additional_xyz_event_bindings=11,additional_xyz_observer_cases_per_configuration=44)
save('scope/coverage-v3.json',d)
print('11 additional XYZ option event bindings/44 cases per configuration;frozen project unchanged',flush=True)
