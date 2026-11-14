"""Fresh original/current UDF compilation with observational MMRM branch hooks."""
import re,shutil
from common import *

TRACE='''Option Strict On
Imports System
Imports System.Collections.Generic
Imports System.Globalization
Public Module MmrmBranch38
 Private ReadOnly Events As New List(Of String)()
 Public Sub Hit(name As String, value As Object)
  Dim text As String
  If value Is Nothing Then
   text = "N"
  ElseIf TypeOf value Is Boolean() Then
   Dim mask As Boolean() = DirectCast(value, Boolean())
   text = "System.Boolean[" & mask.GetLowerBound(0).ToString(CultureInfo.InvariantCulture) & ":" & mask.Length.ToString(CultureInfo.InvariantCulture) & "]{"
   For i As Integer = 0 To mask.Length - 1
    If i > 0 Then text &= "|"
    text &= If(mask(i), "B1", "B0")
   Next
   text &= "}"
  Else
   text = If(TypeOf value Is Double, "D" & BitConverter.DoubleToInt64Bits(Convert.ToDouble(value, CultureInfo.InvariantCulture)).ToString("X16", CultureInfo.InvariantCulture), Convert.ToString(value, CultureInfo.InvariantCulture))
  End If
  Events.Add(name & "=" & text)
 End Sub
 Public Sub Clear()
  Events.Clear()
 End Sub
 Public Function Snapshot() As String()
  Return Events.ToArray()
 End Function
End Module
'''

def instrument(text,filename):
 archived=text;maps=[]
 def member(name,edits):
  nonlocal text
  match=re.search(r'(?m)^\s*(?:Private|Friend|Public) (?:Function|Sub) '+name+r'\b[\s\S]*?^\s*End (?:Function|Sub)',text);assert match,name
  block=match[0]
  original_start=archived.index(('Sub ' if 'Sub '+name in archived else 'Function ')+name)
  for anchor,event,value,after in edits:
   assert block.count(anchor)==1,(name,anchor)
   call='MmrmBranch38.Hit("'+event+'", '+value+')'
   replacement=anchor+'\n'+call if after else call+'\n'+anchor
   line=archived[:archived.index(anchor,original_start)].count('\n')+1
   maps.append(dict(source_path=filename,member=name,archived_source_line=line,source_anchor=anchor,event=event,value_expression=value,after_anchor=after))
   block=block.replace(anchor,replacement,1)
  text=text[:match.start()]+block+text[match.end():]
 if filename.endswith('/GeeUDFs.vb'):
  member('GEE_LSMESTIMATE',[
   ('If Not TryGetGeeLsmEstimateSpec(spec, parameterNames, components, errorMessage) Then','G1.public.spec_rejected','True',True),
   ('If Not TryGetGeeLsmEstimateSpec(spec, parameterNames, components, errorMessage) Then','G1.public.components_count','components.Count',True),
   ('If Not TryGetGeeLsmEstimateSpec(spec, parameterNames, components, errorMessage) Then','G1.public.error','errorMessage',True)])
  member('TryGetGeeLsmEstimateSpec',[
   ('components = New List(Of GeeLsmEstimateComponent)()','G1.spec.entry','True',False),
   ('Dim profileColumns As New List(Of KeyValuePair(Of Integer, Integer))()','G1.header.weight_col','weightCol',False),
   ('Dim profileColumns As New List(Of KeyValuePair(Of Integer, Integer))()','G1.header.label_col','labelCol',False),
   ('Dim idx As Integer = GeeFindDesignColumnIndex(parameterNames, header(c))','G1.lookup.index','idx',True),
   ('If String.Equals(parameterNames(idx), "Intercept", StringComparison.OrdinalIgnoreCase) OrElse','G1.lookup.name','parameterNames(idx)',False),
   ('String.Equals(parameterNames(idx), "(Intercept)", StringComparison.OrdinalIgnoreCase) Then','G1.guard.intercept','True',True),
   ('errorMessage = "the intercept column should not be used as a profile column; it is supplied automatically by the fitted design."','G1.guard.error','errorMessage',True),
   ('errorMessage = "the intercept column should not be used as a profile column; it is supplied automatically by the fitted design."','G1.guard.components_count','components.Count',True)])
 elif filename.endswith('/UdfDataImportMixedModel.vb'):
  member('TryGetMmrmLsmEstimateAtSpec',[
   ('atProfile = Nothing','G2.parser.entry','True',False),
   ('If attemptedHeaderlessAt Then Return False','G2.headerless.attempted','attemptedHeaderlessAt',False),
   ('Dim profileColumns As New List(Of KeyValuePair(Of Integer, Integer))()','G2.branch.wide','True',False),
   ('Dim visitCol As Integer = MmrmFindHeaderIndex(header, "visit", "time")','G2.header.visit_col','visitCol',True),
   ('If dataRow >= 0 Then','G2.wide.row','r',False),
   ('If dataRow >= 0 Then','G2.wide.prior_data_row','dataRow',False),
   ('If dataRow >= 0 Then','G2.guard.multirow','True',True),
   ('dataRow = r','G2.first_row.accepted','dataRow',True),
   ('errorMessage = "wide-form at ranges must contain exactly one nonblank data row. Use name/value form for multiple AT settings by row."','G2.guard.error','errorMessage',True),
   ('errorMessage = "wide-form at ranges must contain exactly one nonblank data row. Use name/value form for multiple AT settings by row."','G2.guard.atProfile_Nothing','atProfile Is Nothing',True)])
  member('TryGetMmrmHeaderlessNameValueAtSpec',[
   ('If arr Is Nothing OrElse h Is Nothing OrElse atProfile Is Nothing Then Return False','G2.headerless.bad_precondition','arr Is Nothing OrElse h Is Nothing OrElse atProfile Is Nothing',False),
   ('If arr.GetLength(1) <> 2 Then Return False','G2.headerless.columns','arr.GetLength(1)',False),
   ('If arr.GetLength(1) <> 2 Then Return False','G2.headerless.column_guard_true','arr.GetLength(1) <> 2',False)])
 elif filename.endswith('/MixedModelUDFs.vb'):
  member('MMRM_LSMESTIMATE',[
   ('Dim atProfile As MmrmLsmEstimateAtProfile = Nothing','G2.spec.accepted','True',False),
   ('If Not Global.BESHStatNG.UdfDataImport.TryGetMmrmLsmEstimateAtSpec(at, h, atProfile, errorMessage) Then','G2.public.at_rejected','True',True),
   ('If Not Global.BESHStatNG.UdfDataImport.TryGetMmrmLsmEstimateAtSpec(at, h, atProfile, errorMessage) Then','G2.public.atProfile_Nothing','atProfile Is Nothing',True),
   ('If Not Global.BESHStatNG.UdfDataImport.TryGetMmrmLsmEstimateAtSpec(at, h, atProfile, errorMessage) Then','G2.public.error','errorMessage',True)])

 return text,maps

def compile_library(config,role,mode):
 assert config in ['Debug','Release'] and role in ['original','current'] and mode in ['plain','trace']
 if role=='current':
  seal=json.loads((HERE/'original-output-seal.json').read_bytes());assert seal['status']=='ORIGINAL_OUTPUT_SEALED'
  for row in seal['files']:assert pin(ROOT/row['path'])==row
  previous=json.loads((ROOT/PREVIOUS).read_bytes())
  for row in previous['entries']:
   if row['sha256'] is not None:assert pin(ROOT/row['path'])==row
 folder=PRIVATE/(role+'-'+config+'-'+mode+'-v2');folder.mkdir(parents=True,exist_ok=False)
 build=PRIVATE/'original-build'/config if role=='original' else RUNROOT/'builds/validation-v2'/config/'legacy'
 for name in ['BESHStatNG.dll','BESHStat.Core.dll','BESHStat.Excel.Common.dll','ExcelDna.Integration.dll']:shutil.copy2(build/name,folder/name)
 sources=[];maps=[];base=TREE if role=='original' else ROOT
 for row in json.loads((ROOT/'evidence/04A38/preedit/entry-source-snapshot.json').read_bytes())['files']:
  source=base/row['source']['path']
  if mode=='trace' and source.name in ['GeeUDFs.vb','MixedModelUDFs.vb','UdfDataImportMixedModel.vb']:
   value,edits=instrument(source.read_text(encoding='utf-8-sig'),row['source']['path']);maps+=edits
   source=folder/source.name;source.write_text(value,encoding='utf-8',newline='\n')
  sources.append(source)
 bindings=json.loads((ROOT/'evidence/04A38/preedit/native-safety/dependency-bindings.json').read_bytes())
 for row in bindings['bindings']:assert row['original'].replace('\r\n','\n').strip() in (TREE/row['path']).read_text(encoding='utf-8-sig')
 assert sha((ROOT/'evidence/04A38/preedit/native-safety/DependencySlices38.vb').read_bytes())==bindings['generated']['sha256']
 sources += [base/'BESHstatNG/src/DM/WindowsSpreadsheetTransport.vb',base/'BESHstatNG/src/RegModels/MixedModels/MixedModelFrontEndHelpers.vb']
 sources += [ROOT/'evidence/04A38/preedit/native-safety'/name for name in ['NativeBoundary38.vb','DependencySlices38.vb','FixtureInputs38.vb']]
 if mode=='trace':
  trace=folder/'MmrmBranch38.vb';trace.write_text(TRACE,encoding='utf-8',newline='\n');sources.append(trace)
 facade=Path('C:/Program Files (x86)/Reference Assemblies/Microsoft/Framework/.NETFramework/v4.8/Facades/netstandard.dll')
 refs=[folder/name for name in ['BESHStatNG.dll','BESHStat.Core.dll','BESHStat.Excel.Common.dll','ExcelDna.Integration.dll']]+[facade]
 args=[compiler('vbc.exe'),'/nologo','/target:library','/rootnamespace:BESHStatNG','/optioncompare:text','/optioninfer+','/optionexplicit+',
       '/imports:System,System.Collections,System.Collections.Generic,System.Diagnostics,System.Linq,System.Globalization',
       '/optimize'+('+' if config=='Release' else '-'),'/out:'+str(folder/'BESHStat.Core.Tests.dll'),
       '/reference:System.Core.dll','/reference:System.dll','/reference:System.Data.dll','/reference:System.Xml.Linq.dll',*['/reference:'+str(p) for p in refs],*map(str,sources)]
 result=command('compile-'+role+'-'+config+'-'+mode,args);assert result['exit_code']==0
 records=[]
 for p in sources:
  record=library_pin(p);record['path']=('<ORIGINAL_TREE>/'+p.relative_to(TREE).as_posix()) if p.is_relative_to(TREE) else '<ISOLATED_HARNESS>/'+p.name if p.is_relative_to(PRIVATE) else p.relative_to(ROOT).as_posix();records.append(record)
 save('compile-'+role+'-'+config+'-'+mode+'.json',dict(configuration=config,role=role,mode=mode,source_commit=BASE,reference_class=CLASS,
      sources=records,instrumentation=maps,assemblies=[library_pin(p) for p in sorted(folder.glob('*.dll'))],command=clean(result),native='NOT_RUN'))
 return folder
if __name__=='__main__':compile_library(*sys.argv[1:4])
