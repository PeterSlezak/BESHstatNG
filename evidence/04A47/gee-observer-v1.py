from common47 import *
lock();folder=PRIVATE/'gee-observer-v1';folder.mkdir();source=HERE/'fixtures/GeeObserver47.vb'
code='''Option Strict On
Imports System
Imports System.Collections.Generic
Imports System.Globalization
Imports System.Web.Script.Serialization
Imports BESHStatNG.ExcelCommon
Imports BESHStatNG.Test
Module GeeObserver47
 Sub Main(args As String())
  Dim rows As New List(Of Object)
  For Each culture In {"", "en-US", "sk-SK"}
   CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo(culture)
   For Each binary In {False, True}
    Dim raw = Gee12Fixture.Raw(If(binary, "Binomial|Logit|Independence|Robust|time", "Gaussian|Identity|Independence|Robust|time"))
    Dim request = Gee12PolicyTests.Request("Gaussian|Identity|Independence|Robust|time", raw)
    Dim fit = request.CreateCoreModel() : fit.Fit(False)
    Dim report = BESHStatNG.WindowsGeeWorkflowAdapter.Report(request, fit.wrapResults(), fit.PredictedResponses, fit.AllResiduals, fit.ObservedResponses, fit.ObservationWeights)
    Dim options As New RegressionOptions With {.Family = RegressionFamily.Binomial, .Classification = True, .ThresholdTable = True}
    Dim exceptionType As String = Nothing, exceptionMessage As String = Nothing, tables = 0
    Try
     tables = BESHStatNG.WindowsGeeWorkflowAdapter.DeferredPostFitFamilyReport(report, options, Double.NaN).LegacyTables().Count
    Catch ex As ArgumentException
     exceptionType = ex.GetType().FullName : exceptionMessage = ex.Message
    End Try
    If binary AndAlso (exceptionType IsNot Nothing OrElse tables = 0) Then Throw New InvalidOperationException("Binary mismatch branch changed")
    If Not binary AndAlso (exceptionType <> "System.ArgumentException" OrElse exceptionMessage <> "Observed outcomes must be coded as 0/1.") Then Throw New InvalidOperationException("Nonbinary shipping error changed")
    If GeeReportPolicy.Classification(report, options).LegacyTables().Count <> 0 Then Throw New InvalidOperationException("Fitted-family Common boundary changed")
    rows.Add(New With {.culture = culture, .binaryResponses = binary, .shippingException = exceptionType, .message = exceptionMessage, .legacyTables = tables, .commonTables = 0})
   Next
  Next
  IO.File.WriteAllText(args(0), New JavaScriptSerializer().Serialize(rows))
 End Sub
End Module
'''
with source.open('x',encoding='utf-8',newline='\n') as f:f.write(code)
locator=Path(os.environ['ProgramFiles(x86)'])/'Microsoft Visual Studio/Installer/vswhere.exe';vbc=subprocess.check_output([str(locator),'-products','*','-version','[18.0,19.0)','-find','MSBuild/Current/Bin/Roslyn/vbc.exe'],text=True).splitlines()[0];framework=Path(os.environ['ProgramFiles(x86)'])/'Reference Assemblies/Microsoft/Framework/.NETFramework/v4.8';results=[]
for config in ['Debug','Release']:
 lib=PRIVATE/'builds/a47-final-v1'/config/'legacy';exe=lib/'GeeObserver47.exe';r=command('GEE-observer-'+config+'-compile',[vbc,'/nologo','/target:exe','/sdkpath:'+str(framework),'/out:'+str(exe),'/optimize'+('+' if config=='Release' else '-'),*[('/r:'+str(framework/n)) for n in ['System.dll','System.Core.dll','System.Web.Extensions.dll']],*[('/r:'+str(lib/n)) for n in ['BESHStat.Core.dll','BESHStat.Excel.Common.dll','BESHStatNG.dll','BESHStatNG.Test.dll']],str(source)]);assert not r['exit_code']
 out=folder/(config+'.json');r=command('GEE-observer-'+config+'-execute',[str(exe),str(out)]);assert not r['exit_code'];rows=load(out);assert len(rows)==6
 for row in rows:
  if row['message']:row['message_sha256']=sha(row.pop('message').encode())
  else:row.pop('message')
 results.append(dict(configuration=config,rows=rows,raw=pin(out)))
assert results[0]['rows']==results[1]['rows']
save('fixtures/gee-observer-v1.json',dict(label='CURRENT_MAIN_PRE_EDIT_04A47_SOURCE_BACKED_OBSERVER_NOT_NATIVE',source=pin(source),shipping_adapter=pin('BESHstatNG/src/DM/WindowsGeeWorkflowAdapter.vb'),case_count_per_configuration=6,results=results,new_managed_identities=0,frozen_test_project_changed=False,qualification='Explicitly preserves both nonbinary ArgumentException and binary-response post-fit selected/fitted-family mismatch. Reuses unchanged accepted fixture and real Core fit;software compatibility only,not a scientific reference or native observation.',native='NOT_RUN'))
print('GEE observer6 cases/config:nonbinary error retained;binary mismatch retained;zero new managed identities',flush=True)
