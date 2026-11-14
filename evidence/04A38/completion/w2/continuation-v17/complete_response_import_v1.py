"""Finish portable integer and ZIP matrix preparation before any test campaign."""
from source_move_tools_v1 import *
import source_move_tools_v1 as mt
def local_read(path):
 if path not in cache:
  raw=(ROOT/path).read_bytes();target=HERE/'response-import-before'/path;target.parent.mkdir(parents=True,exist_ok=True)
  with target.open('xb') as f:f.write(raw)
  cache[path]=[raw,raw.decode('utf-8-sig')];before.append(dict(path=path,live=pin(ROOT/path),preimage=pin(target)))
 return cache[path][1]
mt.read=local_read
path='BESHstatNG/src/ExcelUDFs/UDFdataImport/UdfDataImport.vb'
f=function(path,'TryGetIntegerOutcomeColumn')
a=f.index('        For i As Integer = 0 To data.nRows - 1');b=f.index('        End Function') if '        End Function' in f else f.rfind('    End Function')
old=f[a:b];text=norm(old).strip()
pairs=[('data.nRows','rowCount'),('CDbl(data.FinalData(i, 0))','convertDouble(matrix(i, 0))')]
for x,y in pairs:text=text.replace(x,y)
signature='Public Shared Function ReadIntegerOutcomes(matrix(,) As Object, rowCount As Integer, values As List(Of Integer), convertDouble As Func(Of Object, Double)) As Boolean'
prep.append('        '+signature+'\n'+text+'\n        End Function')
newline='\r\n' if '\r\n' in old else '\n'
change(path,'TryGetIntegerOutcomeColumn',old,'        Return Global.BESHStatNG.ExcelCommon.'+PREP+'.ReadIntegerOutcomes(data.FinalData, data.nRows, values, AddressOf Microsoft.VisualBasic.CompilerServices.Conversions.ToDouble)'+newline,dict(owner=PREP,signature=signature,body=text),[dict(before=x,after=y) for x,y in pairs])
f=function(path,'TryGetZipData');a=f.index('        Dim countCols As Integer');b=f.index('        Dim countOut As New glmData')
old=f[a:b];text=norm(old).rstrip()
for declaration in ['Dim rawCount(rowCount - 1, countCols + If(hasOffset, 1, 0)) As Object','Dim countNames(countCols + If(hasOffset, 1, 0)) As String','Dim rawZero(rowCount - 1, zeroCols) As Object','Dim zeroNames(zeroCols) As String']:
 name=declaration.split('(')[0].removeprefix('Dim ')
 text=text.replace(declaration,'ReDim '+declaration.removeprefix('Dim ').rsplit(' As ',1)[0])
signature='Public Shared Sub BuildZipRawMatrices(yCol(,) As Object, xCountMat(,) As Object, xZeroMat(,) As Object, offsetCol(,) As Object, rowCount As Integer, hasOffset As Boolean, yName As String, offsetName As String, countPredictorNames() As String, zeroPredictorNames() As String, ByRef rawCount(,) As Object, ByRef countNames() As String, ByRef rawZero(,) As Object, ByRef zeroNames() As String)'
prep.append('        '+signature+'\n'+text+'\n        End Sub')
call='''        Dim rawCount(,) As Object = Nothing
        Dim countNames() As String = Nothing
        Dim rawZero(,) As Object = Nothing
        Dim zeroNames() As String = Nothing
        Global.BESHStatNG.ExcelCommon.UdfW2CompletionPreparation38.BuildZipRawMatrices(yCol, xCountMat, xZeroMat, offsetCol, rowCount, hasOffset, yName, offsetName, countPredictorNames, zeroPredictorNames, rawCount, countNames, rawZero, zeroNames)

'''
change(path,'TryGetZipData',old,call.replace('\n',newline),dict(owner=PREP,signature=signature,body=text,projection='Local arrays become ByRef unpublished native locals; allocations/copy ordering unchanged.'),[])
common='BESHStat.Excel.Common/src/Analysis/'+PREP+'.vb';s=local_read(common);index=s.rfind('    End Class');cache[common][1]=s[:index]+'\n\n'.join(prep)+'\n'+s[index:]
for p,(raw,s) in cache.items():
 data=s.encode('utf-8');(ROOT/p).write_bytes((b'\xef\xbb\xbf' if raw.startswith(b'\xef\xbb\xbf') else b'')+data)
save('response-import-source-moves-v1.json',dict(moves=moves,before=before,current=[pin(ROOT/p) for p in cache],supersedes_candidate_freeze=pin(HERE/'production-test-freeze-v1.json'),campaigns_started=0,original_queries=0))
oldfreeze=json.loads((HERE/'production-test-freeze-v1.json').read_bytes())
for row in oldfreeze['files']:
 row['after']=pin(ROOT/row['path']);p=HERE/'source-after-v2'/row['path'];p.parent.mkdir(parents=True,exist_ok=True)
 with p.open('xb') as f:f.write((ROOT/row['path']).read_bytes())
 row['snapshot']=pin(p);row['changed_since_v16']=row['after']!=row['prior_freeze']
 if row['path']=='BESHStatNG.Test/src/UDFs/UdfPreparation38AdapterTests.vb':row['preimage']=json.loads((HERE/'packet0-test-source-v1.json').read_bytes())['existing_test_before']
oldfreeze['sources']=[r['after'] for r in oldfreeze['files']];oldfreeze['prior_candidate']=pin(HERE/'production-test-freeze-v1.json')
save('production-test-freeze-v2.json',oldfreeze)
for before_name,after_name in [('build_final_Debug_v1.py','build_final_Debug_v2.py'),('inspect_final_Debug_v1.py','inspect_final_Debug_v2.py')]:
 s=(HERE/before_name).read_text(encoding='utf-8').replace('production-test-freeze-v1.json','production-test-freeze-v2.json')
 if after_name.startswith('inspect'):s=s.replace('final-v1','final-v2').replace('inspection-v1','inspection-v2').replace('-v17-final1','-v17-final2')
 with (HERE/after_name).open('x',encoding='utf-8',newline='\n') as f:f.write(s)
print('Response preparation completed; combined freeze2; no test campaign started')
