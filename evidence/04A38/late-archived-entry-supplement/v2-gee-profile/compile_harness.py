"""Compile plain and source-instrumented original libraries; no UDF execution."""
import json,re,shutil,sys
from pathlib import Path
from inspect_entry import ROOT,HERE,BASE,CLASS,pin,sha,save
from build_original import PRIVATE,TREE,compiler,library_pin
sys.path.insert(0,str(ROOT/'evidence/04A38'))
from run import m,PRIVATE as RUNROOT
from baseline_report import clean

TRACE='''Option Strict On
Imports System
Imports System.Collections.Generic
Imports System.Globalization
Public Module GeeBranch38
 Private ReadOnly Events As New List(Of String)()
 Public Sub Hit(name As String, value As Object)
  Dim text As String = If(TypeOf value Is Double, "D" & BitConverter.DoubleToInt64Bits(Convert.ToDouble(value, CultureInfo.InvariantCulture)).ToString("X16", CultureInfo.InvariantCulture), Convert.ToString(value, CultureInfo.InvariantCulture))
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

def instrument(text):
    archived=text
    maps=[]
    def member(name, substitutions):
        nonlocal text
        match=re.search(r'(?m)^\s*(?:Private|Public) Function '+name+r'\b[\s\S]*?^\s*End Function',text)
        assert match,name
        block=match[0]
        for old,event,value in substitutions:
            assert block.count(old)==1,(name,old)
            line=text[:match.start()+block.index(old)].count('\n')+1
            new='GeeBranch38.Hit("'+event+'", '+value+')\n'+old
            maps.append(dict(member=name,source_path='BESHstatNG/src/ExcelUDFs/Regression/GeeUDFs.vb',
                             source_anchor=old,event=event,value_expression=value,line_in_transformed_sequence=line,
                             archived_source_line=archived[:archived.index(old,archived.index('Function '+name))].count('\n')+1))
            block=block.replace(old,new,1)
        text=text[:match.start()]+block+text[match.end():]
    member('TryGetGeeLsmEstimateAtSpec',[
        ('atProfile = Nothing','parser.entry','True'),
        ('Dim requestedName As String = ExcelArgReaders.CellToTrimmedText(arr(r, nameCol))','parser.name_value','True'),
        ('Dim profileColumns As New List(Of KeyValuePair(Of Integer, Integer))()','parser.wide','True'),
        ('errorMessage = "wide-form at ranges must contain exactly one nonblank data row. Use name/value form for multiple AT settings by row."','guard.wide_multiple_rows','True'),
        ('atProfile = parsed','profile.formed','True')])
    member('AddGeeLsmEstimateAtValue',[
        ('If atProfile Is Nothing Then','helper.entry','requestedName'),
        ('errorMessage = "at specifies time more than once."','guard.duplicate_time','requestedName'),
        ('atProfile.TimeSpecified = True','selection.time','requestedName'),
        ('atProfile.TimeValue = value','selection.time_value','value'),
        ('errorMessage = "at specifies design column """ & parameterNames(idx) & """ more than once."','guard.duplicate_design','parameterNames(idx)'),
        ('atProfile.ProfileValues.Add(New GeeLsmEstimateProfileValue With {','selection.design','parameterNames(idx)')])
    member('GEE_LSMESTIMATE',[
        ('Dim atProfile As GeeLsmEstimateAtProfile = Nothing','spec.accepted','String.Join("|", parameterNames)'),
        ('Dim labels As New List(Of String)()','profile.applied','True'),
        ('Return "BESH.REGR.GEE_LSMESTIMATE error: no observed design rows matched profile for label \'" & component.Label & "\'."','later.no_matching_rows','True'),
        ('Return PreparePValueTableForUdf(out, 5)','report.returned','True')])
    return text,maps

def main():
    config,mode=sys.argv[1:3];assert config in ['Debug','Release'] and mode in ['plain','trace']
    build=PRIVATE/'original-build'/config
    assert (HERE/('build-original-'+config+'.json')).is_file()
    folder=PRIVATE/('original-'+config+'-'+mode);folder.mkdir(exist_ok=False)
    for path in build.glob('*.dll'):shutil.copy2(path,folder/path.name)
    rows=json.loads((ROOT/'evidence/04A38/preedit/entry-source-snapshot.json').read_bytes())['files']
    sources=[];maps=[]
    for row in rows:
        source=TREE/row['source']['path']
        if mode=='trace' and source.name=='GeeUDFs.vb':
            text,maps=instrument(source.read_text(encoding='utf-8-sig'))
            source=folder/source.name;source.write_text(text,encoding='utf-8',newline='\n')
        sources.append(source)
    bindings=json.loads((ROOT/'evidence/04A38/preedit/native-safety/dependency-bindings.json').read_bytes())
    for row in bindings['bindings']:
        assert row['original'].replace('\r\n','\n').strip() in (TREE/row['path']).read_text(encoding='utf-8-sig')
    assert sha((ROOT/'evidence/04A38/preedit/native-safety/DependencySlices38.vb').read_bytes())==bindings['generated']['sha256']
    sources += [TREE/'BESHstatNG/src/DM/WindowsSpreadsheetTransport.vb',TREE/'BESHstatNG/src/RegModels/MixedModels/MixedModelFrontEndHelpers.vb',
                ROOT/'evidence/04A38/preedit/native-safety/NativeBoundary38.vb',ROOT/'evidence/04A38/preedit/native-safety/DependencySlices38.vb']
    if mode=='trace':
        path=folder/'GeeBranch38.vb';path.write_text(TRACE,encoding='utf-8',newline='\n');sources.append(path)
    facade=Path('C:/Program Files (x86)/Reference Assemblies/Microsoft/Framework/.NETFramework/v4.8/Facades/netstandard.dll')
    refs=[folder/name for name in ['BESHStatNG.dll','BESHStat.Core.dll','BESHStat.Excel.Common.dll','ExcelDna.Integration.dll']]+[facade]
    args=[compiler('vbc.exe'),'/nologo','/target:library','/rootnamespace:BESHStatNG','/optioncompare:text','/optioninfer+','/optionexplicit+',
          '/imports:System,System.Collections,System.Collections.Generic,System.Diagnostics,System.Linq,System.Globalization',
          '/optimize'+('+' if config=='Release' else '-'),'/out:'+str(folder/'BESHStat.Core.Tests.dll'),
          '/reference:System.Core.dll','/reference:System.dll','/reference:System.Data.dll','/reference:System.Xml.Linq.dll',
          *['/reference:'+str(p) for p in refs],*map(str,sources)]
    result=m.Runner(RUNROOT).command('gee38-v2-compile-original-'+config+'-'+mode,args);assert result['exit_code']==0
    source_records=[]
    for p in sources:
        data=p.read_bytes();source_records.append(dict(path=('<ORIGINAL_TREE>/'+p.relative_to(TREE).as_posix()) if p.is_relative_to(TREE) else '<HARNESS>/'+p.name,bytes=len(data),sha256=sha(data)))
    save('compile-original-'+config+'-'+mode+'.json',dict(source_commit=BASE,reference_class=CLASS,configuration=config,mode=mode,
         command=clean(result),sources=source_records,instrumentation=maps,dependencies=[library_pin(p) for p in sorted(folder.glob('*.dll'))],
         actual_source_hash=sha((TREE/'BESHstatNG/src/ExcelUDFs/Regression/GeeUDFs.vb').read_bytes()),
         current_production_assemblies_consumed=False,reference_executed=False))

if __name__=='__main__':main()
