"""Approved native reconversion control flow and one portable token map."""
from common import *
import re
assert json.loads((HERE/'entry-authentication-v1.json').read_bytes())['status']=='ENTRY_GATES_PASS'
paths=['BESHstatNG/src/ExcelUDFs/UdfScalarUtilities.vb','BESHStat.Excel.Common/src/Analysis/UdfOptionPreparation38.vb']
before=[];moves=[]
def snapshot(path):
 data=(ROOT/path).read_bytes();target=HERE/'packet0-before'/path;target.parent.mkdir(parents=True,exist_ok=True)
 with target.open('xb') as f:f.write(data)
 before.append(dict(path=path,live=pin(ROOT/path),preimage=pin(target)))
 return data,data.decode('utf-8-sig').replace('\r\n','\n')
def store(path,raw,text):
 eol='\r\n' if b'\r\n' in raw else '\n';data=text.replace('\n',eol).encode('utf-8')
 if raw.startswith(b'\xef\xbb\xbf'):data=b'\xef\xbb\xbf'+data
 (ROOT/path).write_bytes(data)
common=paths[1];raw,s=snapshot(common)
pattern=r'        Public Shared Function NormalizeMixedModelResidualCovarianceName\(arg As String, defaultValue As String\) As String\n.*?        End Function'
matches=list(re.finditer(pattern,s,re.S));assert len(matches)==1;old=matches[0].group()
mapping=old.split('\n',1)[1].rsplit('        End Function',1)[0]
lines=[]
for line in mapping.splitlines():
 if line.strip()=='Return arg':lines.extend(['            canonical = Nothing','            Return False']);continue
 m=re.fullmatch(r'(\s*)If (.*) Then Return (.*)',line)
 if m:lines.extend([m[1]+'If '+m[2]+' Then',m[1]+'    canonical = '+m[3],m[1]+'    Return True',m[1]+'End If'])
 else:lines.append(line)
classifier='''        Public Shared Function TryClassifyMixedModelResidualCovarianceName(arg As String, defaultValue As String, ByRef canonical As String) As Boolean
'''+ '\n'.join(lines)+'\n        End Function'
wrapper='''        Public Shared Function NormalizeMixedModelResidualCovarianceName(arg As String, defaultValue As String) As String
            Dim canonical As String = Nothing
            If TryClassifyMixedModelResidualCovarianceName(arg, defaultValue, canonical) Then Return canonical
            Return arg
        End Function'''
new=wrapper+'\n\n'+classifier;assert s.count(old)==1;s=s.replace(old,new)
store(common,raw,s);moves.append(dict(path=common,original_segment=old,current_segment=new,portable_classifier=classifier,old_string_API_behavior='Recognized/blank returns canonical/default; unknown returns the exact String argument reference; no conversion or cloning is added.'))
native=paths[0];raw,s=snapshot(native)
old='''    Friend Function NormalizeMixedModelResidualCovarianceName(arg As Object, defaultValue As String) As String
        Return Global.BESHStatNG.ExcelCommon.UdfScalarUtilitiesOptions38.NormalizeMixedModelResidualCovarianceName(Global.BESHStatNG.WorksheetFunctions.ExcelArgReaders.AsString(arg), defaultValue)
    End Function'''
new='''    Friend Function NormalizeMixedModelResidualCovarianceName(arg As Object, defaultValue As String) As String
        Dim canonical As String = Nothing
        If Global.BESHStatNG.ExcelCommon.UdfScalarUtilitiesOptions38.TryClassifyMixedModelResidualCovarianceName(Global.BESHStatNG.WorksheetFunctions.ExcelArgReaders.AsString(arg), defaultValue, canonical) Then Return canonical
        Return Global.BESHStatNG.WorksheetFunctions.ExcelArgReaders.AsString(arg)
    End Function'''
assert s.count(old)==1;store(native,raw,s.replace(old,new));moves.append(dict(path=native,original_segment=old,current_segment=new,policy='First native AsString before pure classification. Second native AsString only after false (unknown nonblank token). No native exception handler or object copy is introduced.'))
save('packet0-source-transitions-v1.json',dict(before=before,after=[pin(ROOT/p) for p in paths],moves=moves,original_queries=0))
print('Native covariance reconversion control flow and single Common classifier implemented; no tests launched',flush=True)
