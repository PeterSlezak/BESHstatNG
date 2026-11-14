"""Compile authentic unregistered original UDFs, with separate trace-only copy."""
import json,os,re,shutil,subprocess,sys
from pathlib import Path
from prepare_original import ROOT,HERE,PRIVATE,TREE,BASE,sha,pin,save
sys.path.insert(0,str(ROOT/'evidence/04A38'))
from run import m,PRIVATE as RUNROOT
from baseline_report import clean

TRACE='''Option Strict On
Imports System
Imports System.Collections.Generic
Imports System.Globalization
Public Module LateBranch38
    Private ReadOnly Events As New List(Of String)()
    Public Sub Hit(name As String, value As Object)
        Events.Add(name & "=" & Convert.ToString(value, CultureInfo.InvariantCulture))
    End Sub
    Public Sub Clear()
        Events.Clear()
    End Sub
    Public Function Snapshot() As String()
        Return Events.ToArray()
    End Function
End Module
'''

def instrument(text,name):
 pattern=r'(?m)^\s*(?:Private|Friend|Public) Function '+name+r'\b[\s\S]*?^\s*End Function'
 match=re.search(pattern,text);assert match,name
 block=match[0];changes=[]
 def replace(old,new):
  nonlocal block
  assert block.count(old)==1,(name,old);block=block.replace(old,new,1);changes.append(dict(original=old,instrumented=new))
 if name=='CALIB_POINTS':
  replace('Dim yy() As Double = yVals.ToArray()', 'LateBranch38.Hit("calibration.method", methodName)\n                LateBranch38.Hit("calibration.bins", binCount)\n                Dim yy() As Double = yVals.ToArray()')
 elif name=='BuildCalibrationPointsTable':
  replace('Dim out(rows.Count, 7) As Object','LateBranch38.Hit("calibration.report_enter", rows.Count)\n            Dim out(rows.Count, 7) As Object')
  replace('Dim r = rows(i)','Dim r = rows(i)\n                LateBranch38.Hit("calibration.lower_is_nan", Double.IsNaN(r.LowerCI))\n                LateBranch38.Hit("calibration.upper_is_nan", Double.IsNaN(r.UpperCI))')
 elif name=='TryGetGroupedCompleteMatrix':
  replace('Dim usableRows As Integer = dataRows - startData','LateBranch38.Hit("grouped.header", dataHasHeader)\n        LateBranch38.Hit("grouped.startData", startData)\n        Dim usableRows As Integer = dataRows - startData')
  replace('Dim buckets As New Dictionary','LateBranch38.Hit("grouped.startGroup", startGroup)\n        Dim buckets As New Dictionary')
  replace('If String.IsNullOrWhiteSpace(label) Then Continue For','If String.IsNullOrWhiteSpace(label) Then\n                LateBranch38.Hit("grouped.skip_label", i)\n                Continue For\n            End If')
  replace('If Not ok Then Continue For','If Not ok Then\n                LateBranch38.Hit("grouped.skip_invalid_row", i)\n                Continue For\n            End If')
  replace('Return True','LateBranch38.Hit("grouped.prepared_groups", groupMatrices.Count)\n        Return True')
 else:
  replace('Dim startRow As Integer = If(hasHeader, 1, 0)','LateBranch38.Hit("nested.header", hasHeader)\n        Dim startRow As Integer = If(hasHeader, 1, 0)')
  replace('Return True','LateBranch38.Hit("nested.prepared_rows", rowsOut.Count)\n        Return True')
 return text[:match.start()]+block+text[match.end():],changes

def main():
 config=sys.argv[1];traced=len(sys.argv)>2 and sys.argv[2]=='trace';assert config in ['Debug','Release']
 build=PRIVATE/'original-build'/config;assert (HERE/('build-original-'+config+'.json')).exists()
 tag='original-'+config+('-trace' if traced else '-plain');folder=PRIVATE/tag;folder.mkdir(exist_ok=False)
 for p in build.glob('*.dll'):shutil.copy2(p,folder/p.name)
 original=json.loads((ROOT/'evidence/04A38/preedit/entry-source-snapshot.json').read_bytes())['files'];sources=[];changes=[]
 for row in original:
  p=TREE/row['source']['path'];data=p.read_bytes();source=p
  names=[]
  if p.name=='PlotDataUDFs.vb':names=['CALIB_POINTS','BuildCalibrationPointsTable']
  if p.name=='UdfDataImport.vb':names=['TryGetGroupedCompleteMatrix','TryGetNestedThreeColumnData']
  if traced and names:
   value=data.decode('utf-8-sig').replace('\r\n','\n')
   for name in names:
    value,edits=instrument(value,name);changes.append(dict(path=row['source']['path'],member=name,edits=edits))
   source=folder/p.name;source.write_text(value,encoding='utf-8',newline='\n')
  sources.append(source)
 sources += [TREE/'BESHstatNG/src/DM/WindowsSpreadsheetTransport.vb',TREE/'BESHstatNG/src/RegModels/MixedModels/MixedModelFrontEndHelpers.vb']
 # These frozen source slices delegate to existing original Core methods. Their
 # bodies are crosschecked independently against the authenticated original tree.
 bindings=json.loads((ROOT/'evidence/04A38/preedit/native-safety/dependency-bindings.json').read_bytes())
 for item in bindings['bindings']:
  source=(TREE/item['path']).read_text(encoding='utf-8-sig').replace('\r\n','\n')
  original_body=item['original'].replace('\r\n','\n');assert original_body.strip() in source,item['member']
 native=ROOT/'evidence/04A38/preedit/native-safety/NativeBoundary38.vb'
 slices=ROOT/'evidence/04A38/preedit/native-safety/DependencySlices38.vb'
 assert sha(slices.read_bytes())==bindings['generated']['sha256']
 sources += [native,slices]
 if traced:
  trace=folder/'LateBranch38.vb';trace.write_text(TRACE,encoding='utf-8',newline='\n');sources.append(trace)
 locator=Path(os.environ['ProgramFiles(x86)'])/'Microsoft Visual Studio/Installer/vswhere.exe'
 compiler=subprocess.check_output([str(locator),'-products','*','-version','[18.0,19.0)','-find','MSBuild/Current/Bin/Roslyn/vbc.exe'],text=True).splitlines()[0]
 facade=Path(os.environ['ProgramFiles(x86)'])/'Reference Assemblies/Microsoft/Framework/.NETFramework/v4.8/Facades/netstandard.dll'
 refs=[folder/name for name in ['BESHStatNG.dll','BESHStat.Core.dll','BESHStat.Excel.Common.dll','ExcelDna.Integration.dll']]+[facade]
 args=[compiler,'/nologo','/target:library','/rootnamespace:BESHStatNG','/optioncompare:text','/optioninfer+','/optionexplicit+',
  '/imports:System,System.Collections,System.Collections.Generic,System.Diagnostics,System.Linq,System.Globalization',
  '/optimize'+('+' if config=='Release' else '-'),'/out:'+str(folder/'BESHStat.Core.Tests.dll'),'/reference:System.Core.dll','/reference:System.dll','/reference:System.Data.dll','/reference:System.Xml.Linq.dll',
  *['/reference:'+str(p) for p in refs],*map(str,sources)]
 result=m.Runner(RUNROOT).command('late38-compile-'+tag,args);assert result['exit_code']==0
 def source_pin(p):
  data=p.read_bytes()
  return dict(path=('<ORIGINAL_TREE>/'+p.relative_to(TREE).as_posix()) if p.is_relative_to(TREE) else ('<ISOLATED_HARNESS>/'+p.name) if p.is_relative_to(PRIVATE) else p.relative_to(ROOT).as_posix(),bytes=len(data),sha256=sha(data))
 save('compile-'+tag+'.json',dict(status='INDEPENDENT_PINNED_ORIGINAL_UDF_LIBRARY_COMPILED',configuration=config,source_commit=BASE,trace_only=traced,
  library=file_record(folder/'BESHStat.Core.Tests.dll'),sources=[source_pin(p) for p in sources],instrumentation=changes,
  command=clean(result),native_stub='Only wizard=False and blocked/counted XlCall; no numeric/import/report/statistical logic replaced',
  dependencies=[file_record(p) for p in sorted(folder.glob('*.dll'))],new_current_helpers_allowed=False,
  chronology='CAPTURED_AFTER_INITIAL_04A38_IMPLEMENTATION_FROM_PINNED_ENTRY',reference_class='LATE_ARCHIVED_ENTRY_SUPPLEMENT',native='NOT_RUN',scientific='NOT_GOLDEN'))
 print('Compiled',tag,'authentic source and original dependencies; no reference executed')

def file_record(path):
 data=path.read_bytes();return dict(name=path.name,bytes=len(data),sha256=sha(data))

if __name__=='__main__':main()
