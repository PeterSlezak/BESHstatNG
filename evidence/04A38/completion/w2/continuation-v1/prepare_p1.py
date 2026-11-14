"""Preserve narrow preimages, then move deterministic preparation unchanged."""
import re
from common import *
paths=[
 'BESHStat.Excel.Common/src/Analysis/UdfModelProfilePreparation38.vb',
 'BESHStat.Excel.Common/src/Analysis/UdfNumericImportPreparation38.vb',
 'BESHstatNG/src/ExcelUDFs/Regression/GeeUDFs.vb',
 'BESHstatNG/src/ExcelUDFs/Regression/MixedModelUDFs.vb',
 'BESHstatNG/src/ExcelUDFs/UDFdataImport/UdfDataImport.vb',
 'BESHStat.Excel.Common.Tests/src/UdfModelProfilePreparation38Tests.vb',
 'BESHStatNG.Test/src/UDFs/UdfPreparation38AdapterTests.vb']
def read(p):return (ROOT/p).read_text(encoding='utf-8-sig')
def write(p,s):
 old=(ROOT/p).read_bytes();nl='\r\n' if b'\r\n' in old else '\n'
 (ROOT/p).write_bytes((b'\xef\xbb\xbf' if old.startswith(b'\xef\xbb\xbf') else b'')+s.replace('\r\n','\n').replace('\n',nl).encode('utf-8'))
def member(s,name):
 p=re.compile(r'^([ \t]*)(?:Private|Friend|Public) (?:Shared )?(Function|Sub) '+re.escape(name)+r'\b.*?^\1End \2',re.M|re.S)
 m=p.search(s);assert m,name;return m.group()
def body(m):return m[m.index('\n',m.index(') As ')):] if False else m
if __name__=='__main__':
 snapshots=[]
 for p in paths:
  dest=HERE/'source-before/p1'/ (p+'.txt');dest.parent.mkdir(parents=True,exist_ok=True)
  with dest.open('xb') as f:f.write((ROOT/p).read_bytes())
  snapshots.append(dict(live=pin(ROOT/p),preimage=pin(dest)))
 save('source-before-p1.json',dict(files=snapshots,plan=['GEE/MMRM apply and column membership with copied values','Grouped/nested numeric preparation with host cell policies'],new_original_queries=0))
 portable=read(paths[0]);added=[]
 for family,path,apply,has in [('Gee',paths[2],'ApplyGeeLsmEstimateAtProfile','GeeComponentHasProfileColumn'),('Mmrm',paths[3],'ApplyMmrmLsmEstimateAtProfile','ComponentHasProfileColumn')]:
  s=read(path);old=member(s,apply);oldhas=member(s,has)
  moved=old[old.index('            If components'):]
  moved=moved.replace(f'{family}LsmEstimateComponent','TComponent').replace(f'{family}LsmEstimateProfileValue','TValue').replace(has,f'{family}ComponentHasProfileColumn(Of TComponent, TValue)')
  new=f'''        Public Shared Sub Apply{family}AtProfile(Of TComponent As Udf{family}Component38(Of TValue), TProfile As Udf{family}AtProfile38(Of TValue), TValue As {{UdfLsmProfileValue38, New}})(
            components As List(Of TComponent), atProfile As TProfile)
'''+moved
  movedhas=oldhas[oldhas.index('            If component'):].replace(f'{family}LsmEstimateProfileValue','TValue')
  newhas=f'''        Private Shared Function {family}ComponentHasProfileColumn(Of TComponent As Udf{family}Component38(Of TValue), TValue As UdfLsmProfileValue38)(
            component As TComponent, columnIndex As Integer) As Boolean
'''+movedhas
  added.extend([new,newhas])
  signature=old[:old.index('            If components')]
  wrapper=signature+f'''            Global.BESHStatNG.ExcelCommon.UdfModelProfilePreparation38.Apply{family}AtProfile(Of {family}LsmEstimateComponent, {family}LsmEstimateAtProfile, {family}LsmEstimateProfileValue)(components, atProfile)
        End Sub'''
  s=s.replace(old,wrapper).replace(oldhas,'');write(path,s)
 portable=portable.replace('        Public Delegate Function FiniteDoubleReader','\n\n'.join(added)+'\n\n        Public Delegate Function FiniteDoubleReader');write(paths[0],portable)
 host=read(paths[4]);common=read(paths[1]);moved=[]
 for name in ['TryGetGroupedCompleteMatrix','TryGetNestedThreeColumnData']:
  old=member(host,name)
  if name=='TryGetGroupedCompleteMatrix':
   code=old[old.index('        If dataArr Is Nothing'):]
   code=code.replace('LooksLikeHeaderRow(dataArr, dataColIndexes)','header(dataArr, dataColIndexes)').replace('Convert.ToString(groupArr(startGroup + i, 0)).Trim()','labelText(groupArr(startGroup + i, 0))').replace('Global.BESHStatNG.WorksheetFunctions.ExcelArgNumeric.TryGetDouble','numeric').replace('RowsToMatrix(', 'Global.BESHStatNG.WorksheetCompatibility.UdfNumericPreparation38.RowsToMatrix(')
   signature='''    Public Shared Function TryGetGroupedCompleteMatrix(dataArr As Object(,), groupArr As Object(,),
        header As Func(Of Object(,), Integer(), Boolean), labelText As Func(Of Object, String), numeric As Func(Of Object, Double?),
        ByRef groupMatrices As List(Of Double(,)), ByRef groupNames As List(Of String)) As Boolean
        groupMatrices = Nothing
        groupNames = Nothing
'''
   wrapper=old[:old.index('        If dataArr Is Nothing')]+'''        Return Global.BESHStatNG.ExcelCommon.UdfNumericImportPreparation38.TryGetGroupedCompleteMatrix(
            dataArr, groupArr, AddressOf LooksLikeHeaderRow, Function(cell) Convert.ToString(cell).Trim(),
            AddressOf Global.BESHStatNG.WorksheetFunctions.ExcelArgNumeric.TryGetDouble, groupMatrices, groupNames)
    End Function'''
  else:
   code=old[old.index('        If arr Is Nothing'):]
   code=re.sub(r'Convert.ToString\((arr\([^\n]*?\))\).Trim\(\)',r'labelText(\1)',code)
   code=code.replace('Global.BESHStatNG.WorksheetFunctions.ExcelArgNumeric.TryGetDouble','numeric').replace('g <> "" AndAlso sg <> ""','Not LegacyUdfText38.EqualText(g, "") AndAlso Not LegacyUdfText38.EqualText(sg, "")')
   signature='''    Public Shared Function TryGetNestedThreeColumnData(arr As Object(,), labelText As Func(Of Object, String),
        numeric As Func(Of Object, Double?), ByRef data As Object(,), ByRef names As String()) As Boolean
        data = Nothing
        names = Nothing
'''
   wrapper=old[:old.index('        If arr Is Nothing')]+'''        Return Global.BESHStatNG.ExcelCommon.UdfNumericImportPreparation38.TryGetNestedThreeColumnData(
            arr, Function(cell) Convert.ToString(cell).Trim(),
            AddressOf Global.BESHStatNG.WorksheetFunctions.ExcelArgNumeric.TryGetDouble, data, names)
    End Function'''
  moved.append(signature+code);host=host.replace(old,wrapper)
 common=common.replace('        Public Shared Function TryStrictVector','\n\n'.join(moved)+'\n\n        Public Shared Function TryStrictVector');write(paths[1],common);write(paths[4],host)
 print('P1 source transitions prepared; compile and independently verify before closure')
