"""Freeze accepted same-Core rejection identities; NOT a numerical oracle."""
import json,hashlib
from pathlib import Path
ROOT=Path(__file__).resolve().parents[1]
SOURCE=ROOT/'evidence/04A23/multivariate-spc-same-core-comparisons-v1.json'
def literal(s):return '"'+s.replace('"','""').replace('\r','" & vbCr & "').replace('\n','" & vbLf & "')+'"'
def generate():
    data=json.loads(SOURCE.read_bytes());rows={}
    for r in data['records']:
        p=data['payloads'][r['profile_id']]
        if p['id'].startswith('published-') or p['outcome']=='POSITIVE_PARITY':continue
        key='|'.join([r['tfm'],p['id'],p['culture']]);v=(p['exception']['type'],p['exception']['message'])
        assert key not in rows or rows[key]==v
        rows[key]=v
    code='''Option Explicit On
Option Strict On
Imports System
Imports System.Collections.Generic
'Accepted same-Core compatibility rejections only. Never scientific goldens.
Public NotInheritable Class Spc24ExpectedRejections
    Public Const SourceSha256 As String = "'''+hashlib.sha256(SOURCE.read_bytes()).hexdigest()+'''"
    Public Shared Function Cases() As IReadOnlyDictionary(Of String, String())
        Dim rows As New Dictionary(Of String, String())(StringComparer.Ordinal)
'''
    for k,v in sorted(rows.items()):code+='        rows.Add('+literal(k)+', New String() {'+','.join(map(literal,v))+'})\n'
    code+='        Return rows\n    End Function\nEnd Class\n'
    dest=ROOT/'BESHStat.Excel.Common.Tests/src/Spc24ExpectedRejections.vb'
    with dest.open('x',encoding='utf-8',newline='\n') as f:f.write(code)
    print('Frozen',len(rows),'rejection identities, source hash',hashlib.sha256(SOURCE.read_bytes()).hexdigest())
if __name__=='__main__':generate()
