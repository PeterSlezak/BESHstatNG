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
 if filename.endswith('/UdfDataImportMixedModel.vb'):
  member('TryGetMmrmLsmEstimateAtSpec',[
   ('atProfile = Nothing','parser.entry','True',False),
   ('If attemptedHeaderlessAt Then Return False','headerless.attempted','attemptedHeaderlessAt',False),
   ('Dim parsed As New Global.BESHStatNG.WorksheetFunctions.MixedModelUDFs.MmrmLsmEstimateAtProfile()','header.name_col','nameCol',False),
   ('Dim parsed As New Global.BESHStatNG.WorksheetFunctions.MixedModelUDFs.MmrmLsmEstimateAtProfile()','header.value_col','valueCol',False),
   ('If nameCol >= 0 AndAlso valueCol >= 0 AndAlso nameCol <> valueCol Then','branch.headered_name_value','True',True),
   ('Dim requestedName As String = Global.BESHStatNG.WorksheetFunctions.ExcelArgReaders.CellToTrimmedText(arr(r, nameCol))','headered.row','r',False),
   ('Dim profileColumns As New List(Of KeyValuePair(Of Integer, Integer))()','branch.wide','True',False),
   ('atProfile = headerlessAt','branch.headerless_success','True',False),
   ('atProfile = parsed','profile.formed','True',False)])
  member('AddMmrmLsmEstimateAtValue',[
   ('If atProfile Is Nothing Then','helper.entry','requestedName',False),
   ('If atProfile Is Nothing Then','helper.value','value',False),
   ('If atProfile.VisitSpecified Then','visit.previously_specified','atProfile.VisitSpecified',False),
   ('If atProfile.VisitSpecified Then','visit.normalized_dispatch','MmrmNormalizeDesignColumnName(requestedName)',False),
   ('If atProfile.VisitSpecified Then','guard.duplicate_visit_time','True',True),
   ('atProfile.VisitSpecified = True','selection.visit','requestedName',False),
   ('atProfile.VisitValue = value','selection.visit_value','value',False),
   ('Dim idx As Integer = MmrmFindDesignColumnIndex(h.FixedEffectNames, requestedName)','design.resolved_index','idx',True),
   ('For Each existing As Global.BESHStatNG.WorksheetFunctions.MixedModelUDFs.MmrmLsmEstimateProfileValue In atProfile.ProfileValues','design.existing_count','atProfile.ProfileValues.Count',False),
   ('If existing.ColumnIndex = idx Then','design.existing_index','existing.ColumnIndex',False),
   ('If existing.ColumnIndex = idx Then','guard.duplicate_design','True',True),
   ('atProfile.ProfileValues.Add(New Global.BESHStatNG.WorksheetFunctions.MixedModelUDFs.MmrmLsmEstimateProfileValue With {','selection.design','h.FixedEffectNames(idx)',False)])
 elif filename.endswith('/MixedModelUDFs.vb'):
  member('MMRM_LSMEANS',[
   ('ElseIf IsMmrmWorksheetRangeArgument(group) Then','public.range','True',True),
   ('ElseIf IsMmrmWorksheetRangeArgument(group) Then','public.alpha_missing','ExcelArgPredicates.IsMissingArg(alpha)',True),
   ('If Not TryBuildMmrmAtProfileRowMask(h, atProfile, rowMask, profileDescription, errorMessage) Then','public.mask_rejected','True',True),
   ('If Not TryBuildMmrmAtProfileRowMask(h, atProfile, rowMask, profileDescription, errorMessage) Then','public.mask','rowMask',True),
   ('If Not TryBuildMmrmAtProfileRowMask(h, atProfile, rowMask, profileDescription, errorMessage) Then','public.description','profileDescription',True),
   ('If Not TryBuildMmrmAtProfileRowMask(h, atProfile, rowMask, profileDescription, errorMessage) Then','public.error','errorMessage',True),
   ('If table Is Nothing Then Return ExcelError.ExcelErrorNA','report.continued','True',False)])
  member('TryBuildMmrmAtProfileRowMask',[
   ('rowMask = Nothing','mask.entry','True',False),
   ('rowMask = New Boolean(n - 1) {}','mask.rows','n',False),
   ('rowMask = New Boolean(n - 1) {}','mask.visit_specified','atProfile.VisitSpecified',False),
   ('rowMask = New Boolean(n - 1) {}','mask.target_visit','atProfile.VisitValue',False),
   ('rowMask(i) = keep','row.index','i',False),
   ('rowMask(i) = keep','row.visit','h.VisitValues(i)',False),
   ('rowMask(i) = keep','row.keep','keep',False),
   ('If matchedCount <= 0 Then','guard.unmatched','True',True),
   ('If matchedCount <= 0 Then','guard.matched_count','matchedCount',True),
   ('If matchedCount <= 0 Then','guard.mask','rowMask',True),
   ('errorMessage = "no observed design rows matched the supplied LS-means profile."','guard.error','errorMessage',True),
   ('errorMessage = "no observed design rows matched the supplied LS-means profile."','guard.description','profileDescription',True)])

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
  if mode=='trace' and source.name in ['UdfDataImportMixedModel.vb','MixedModelUDFs.vb']:
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
