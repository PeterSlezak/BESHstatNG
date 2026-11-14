"""First cohesive W2 source move: preserve family grammar and host cell policies.

This is a mechanical producer, not an acceptance verifier. Independent compilation,
sealed public cases, focused tests and source review are required afterwards.
"""
import re,json,hashlib
from pathlib import Path
HERE=Path(__file__).resolve().parent;ROOT=HERE.parents[3]
GEE=Path('BESHstatNG/src/ExcelUDFs/Regression/GeeUDFs.vb')
MMRM=Path('BESHstatNG/src/ExcelUDFs/Regression/MixedModelUDFs.vb')
IMPORT=Path('BESHstatNG/src/ExcelUDFs/UDFdataImport/UdfDataImportMixedModel.vb')
COMMON=Path('BESHStat.Excel.Common/src/Analysis/UdfModelProfilePreparation38.vb')

def read(path):return (ROOT/path).read_text(encoding='utf-8-sig')
def member(text,name):
 m=re.search(r'^([ \t]*)(?:Private|Friend|Public) (?:Shared )?Function '+name+r'\b',text,re.M);assert m,name
 end=text.index('\n'+m[1]+'End Function',m.start())+len('\n'+m[1]+'End Function')
 return text[m.start():end]
def body(text,name):
 s=member(text,name);header_end=s.index('\n',s.index(' As Boolean'));return s[header_end+1:s.rindex('\n')]
def reindent(s,n=12):
 import textwrap
 return '\n'.join(' '*n+line if line else '' for line in textwrap.dedent(s).splitlines())
def put(path,text):
 raw=(ROOT/path).read_bytes() if (ROOT/path).exists() else b''
 newline='\r\n' if b'\r\n' in raw else '\n'
 (ROOT/path).write_bytes(text.replace('\r\n','\n').replace('\n',newline).encode('utf-8'))
def inherited(text,name,base):
 m=re.search(r'^([ \t]*)(Private|Friend) Class '+name+r'\n.*?^\1End Class',text,re.M|re.S);assert m,name
 return text[:m.start()]+m[1]+m[2]+' Class '+name+'\n'+m[1]+'    Inherits Global.BESHStatNG.ExcelCommon.'+base+'\n'+m[1]+'End Class'+text[m.end():]

def main():
 assert json.loads((HERE/'entry.json').read_bytes())['status']=='W2_STARTED_AFTER_APPROVAL_G_PASS'
 assert not (ROOT/COMMON).exists()
 g=read(GEE);m=read(MMRM);i=read(IMPORT)
 for path,text in [(GEE,g),(MMRM,m),(IMPORT,i)]:assert text==(HERE/'source-before'/path).read_text(encoding='utf-8-sig')
 methods=[];records=[]
 for family,source,meta in [('Gee',g,'GEEUDFsMetadata38'),('Mmrm',i,'UdfDataImportMetadata38')]:
  gp='Gee' if family=='Gee' else 'Mmrm';host='Global.BESHStatNG.WorksheetFunctions.'
  old=member(source,'TryGet'+family+'LsmEstimateAtSpec')
  # Host keeps missing/range acquisition and its existing error/null initialization.
  core=old[old.index('        Dim rows As Integer'):] if family=='Mmrm' else old[old.index('            Dim rows As Integer'):]
  core=core[:core.rindex('\n')]
  core=reindent(core)
  core=core.replace('Global.BESHStatNG.WorksheetFunctions.MixedModelUDFs.MmrmLsmEstimateAtProfile','TProfile').replace('New GeeLsmEstimateAtProfile()','New TProfile()')
  core=core.replace('Global.BESHStatNG.WorksheetFunctions.ExcelArgPredicates.IsBlankCell','isBlank').replace('ExcelArgPredicates.IsBlankCell','isBlank')
  core=core.replace('Global.BESHStatNG.WorksheetFunctions.ExcelArgReaders.CellToTrimmedText','cellText').replace('ExcelArgReaders.CellToTrimmedText','cellText')
  core=core.replace('Global.BESHStatNG.WorksheetFunctions.ExcelArgNumeric.TryGetFiniteDouble','tryFinite').replace('ExcelArgNumeric.TryGetFiniteDouble','tryFinite')
  core=core.replace(gp+'FindHeaderIndex(',meta+'.'+gp+'FindHeaderIndex(').replace(gp+'FindDesignColumnIndex(',meta+'.'+gp+'FindDesignColumnIndex(')
  core=core.replace('Add'+gp+'LsmEstimateAtValue(', 'Add'+gp+'AtValue(Of TProfile, TValue)(')
  if family=='Mmrm':
   core=core.replace('h.FixedEffectNames','names()').replace('parsed, h,','parsed, names,')
   core=core.replace('TryGetMmrmHeaderlessNameValueAtSpec(arr, h, headerlessAt, errorMessage, attemptedHeaderlessAt)', 'TryGetMmrmHeaderlessNameValueAtSpec(Of TProfile, TValue)(arr, modelAvailable, names, headerlessAt, isBlank, cellText, tryFinite, errorMessage, attemptedHeaderlessAt)')
   args='arr As Object(,), modelAvailable As Boolean, names As Func(Of String()),'
  else:args='arr As Object(,), parameterNames As String(),'
  signature='''        Public Shared Function TryGetFAMILYAtSpec(Of TProfile As {UdfFAMILYAtProfile38(Of TValue), New}, TValue As {UdfLsmProfileValue38, New})(
            ARGS
            isBlank As Func(Of Object, Boolean), cellText As Func(Of Object, String), tryFinite As FiniteDoubleReader,
            ByRef atProfile As TProfile, ByRef errorMessage As String) As Boolean
'''.replace('FAMILY',family).replace('ARGS',args)
  methods.append(signature+core+'\n        End Function')
  oldadd=member(source,'Add'+gp+'LsmEstimateAtValue');addbody=oldadd[oldadd.index('        If atProfile Is Nothing'):] if family=='Mmrm' else oldadd[oldadd.index('            If atProfile Is Nothing'):]
  addbody=reindent(addbody[:addbody.rindex('\n')])
  addbody=addbody.replace('Global.BESHStatNG.WorksheetFunctions.MixedModelUDFs.MmrmLsmEstimateProfileValue','TValue').replace('New GeeLsmEstimateProfileValue','New TValue')
  for name in ['NormalizeDesignColumnName','FindDesignColumnIndex']:addbody=addbody.replace(gp+name+'(',meta+'.'+gp+name+'(')
  if family=='Mmrm':addbody=addbody.replace('h.FixedEffectNames','names()')
  modelarg='names As Func(Of String())' if family=='Mmrm' else 'parameterNames As String()'
  methods.append(('''        Public Shared Function AddFAMILYAtValue(Of TProfile As UdfFAMILYAtProfile38(Of TValue), TValue As {UdfLsmProfileValue38, New})(
            atProfile As TProfile, MODELARG, requestedName As String, value As Double,
            sourceDescription As String, ByRef errorMessage As String) As Boolean
'''.replace('FAMILY',family).replace('MODELARG',modelarg))+addbody+'\n        End Function')
  # Replace the old host grammar by a delegation after Get2D succeeds.
  keep=old[:old.index('        Dim rows As Integer')] if family=='Mmrm' else old[:old.index('            Dim rows As Integer')]
  indent='        ' if family=='Mmrm' else '            '
  parameters='arr, h IsNot Nothing, Function() h.FixedEffectNames,' if family=='Mmrm' else 'arr, parameterNames,'
  call=indent+'Return Global.BESHStatNG.ExcelCommon.UdfModelProfilePreparation38.TryGet'+family+'AtSpec(Of '+family+'LsmEstimateAtProfile, '+family+'LsmEstimateProfileValue)(\n'+indent+'    '+parameters+'\n'+indent+'    AddressOf '+host+'ExcelArgPredicates.IsBlankCell, AddressOf '+host+'ExcelArgReaders.CellToTrimmedText,\n'+indent+'    AddressOf '+host+'ExcelArgNumeric.TryGetFiniteDouble, atProfile, errorMessage)\n'+indent[:-4]+'End Function'
  # MMRM module uses fully qualified private DTOs in its signature, unlike GEE.
  if family=='Mmrm':call=call.replace('Of MmrmLsmEstimateAtProfile, MmrmLsmEstimateProfileValue','Of '+host+'MixedModelUDFs.MmrmLsmEstimateAtProfile, '+host+'MixedModelUDFs.MmrmLsmEstimateProfileValue')
  source=source.replace(old,keep+call)
  head=oldadd[:oldadd.index('        If atProfile Is Nothing')] if family=='Mmrm' else oldadd[:oldadd.index('            If atProfile Is Nothing')]
  argtype=('Of '+host+'MixedModelUDFs.MmrmLsmEstimateAtProfile, '+host+'MixedModelUDFs.MmrmLsmEstimateProfileValue') if family=='Mmrm' else 'Of GeeLsmEstimateAtProfile, GeeLsmEstimateProfileValue'
  replace=head+indent+'Return Global.BESHStatNG.ExcelCommon.UdfModelProfilePreparation38.Add'+family+'AtValue('+argtype+')(\n'+indent+'    atProfile, '+('Function() h.FixedEffectNames' if family=='Mmrm' else 'parameterNames')+', requestedName, value, sourceDescription, errorMessage)\n'+indent[:-4]+'End Function'
  # Parser now calls its portable Add directly; keep a native private member only
  # where there is another real host caller (MMRM headerless wrapper below moves).
  # All previous Add callers moved with parser; remove host Add to avoid dead code.
  source=source.replace(oldadd,'')
  if family=='Mmrm':i=source
  else:g=source
  records.append(dict(family=family,moved=['TryGet'+family+'LsmEstimateAtSpec','Add'+family+'LsmEstimateAtValue']))
 old=member(i,'TryGetMmrmHeaderlessNameValueAtSpec')
 b=old[old.index('        attempted = False'):];b=reindent(b[:b.rindex('\n')])
 b=b.replace('h Is Nothing','Not modelAvailable').replace('h.FixedEffectNames','names()')
 b=b.replace('New Global.BESHStatNG.WorksheetFunctions.MixedModelUDFs.MmrmLsmEstimateAtProfile()','New TProfile()')
 b=b.replace('Global.BESHStatNG.WorksheetFunctions.ExcelArgPredicates.IsBlankCell','isBlank').replace('Global.BESHStatNG.WorksheetFunctions.ExcelArgReaders.CellToTrimmedText','cellText').replace('Global.BESHStatNG.WorksheetFunctions.ExcelArgNumeric.TryGetFiniteDouble','tryFinite')
 b=b.replace('AddMmrmLsmEstimateAtValue(probe, h,','AddMmrmAtValue(Of TProfile, TValue)(probe, names,').replace('AddMmrmLsmEstimateAtValue(atProfile, h,','AddMmrmAtValue(Of TProfile, TValue)(atProfile, names,')
 methods.append('''        Private Shared Function TryGetMmrmHeaderlessNameValueAtSpec(Of TProfile As {UdfMmrmAtProfile38(Of TValue), New}, TValue As {UdfLsmProfileValue38, New})(
            arr As Object(,), modelAvailable As Boolean, names As Func(Of String()), atProfile As TProfile,
            isBlank As Func(Of Object, Boolean), cellText As Func(Of Object, String), tryFinite As FiniteDoubleReader,
            ByRef errorMessage As String, ByRef attempted As Boolean) As Boolean
'''+b+'\n        End Function')
 i=i.replace(old,'')
 # Scalar grouping: preserve h validation in host; move column/role/copy policy.
 old=member(m,'TryGetDesignColumnValues');b=old[old.index('            If h.FixedEffectNames Is Nothing'):];b=reindent(b[:b.rindex('\n')])
 b=b.replace('h.FixedEffectNames','names').replace('h.FittedDesign','design').replace('FindDesignColumnIndex(','MixedModelUDFsMetadata38.FindDesignColumnIndex(')
 methods.append('''        Public Shared Function TryGetMmrmDesignColumnValues(design As Double(,), names As String(), requestedName As String,
            ByRef resolvedName As String, ByRef values As Double(), ByRef errorMessage As String) As Boolean
'''+b+'\n        End Function')
 keep=old[:old.index('            If h.FixedEffectNames Is Nothing')]
 m=m.replace(old,keep+'''            Return Global.BESHStatNG.ExcelCommon.UdfModelProfilePreparation38.TryGetMmrmDesignColumnValues(
                h.FittedDesign, h.FixedEffectNames, requestedName, resolvedName, values, errorMessage)
        End Function''')
 # Pure data inheritance preserves native private/friend type identities and lists.
 for name in ['GeeLsmEstimateProfileValue','MmrmLsmEstimateProfileValue']:
  if name.startswith('Gee'):g=inherited(g,name,'UdfLsmProfileValue38')
  else:m=inherited(m,name,'UdfLsmProfileValue38')
 g=inherited(g,'GeeLsmEstimateAtProfile','UdfGeeAtProfile38(Of GeeLsmEstimateProfileValue)')
 m=inherited(m,'MmrmLsmEstimateAtProfile','UdfMmrmAtProfile38(Of MmrmLsmEstimateProfileValue)')
 portable='''Option Explicit On
Option Strict On

Imports System
Imports System.Collections.Generic
Imports System.Globalization

Namespace ExcelCommon
    '''+"''' <summary>Pure profile values; native model/cache objects never cross this boundary.</summary>"+'''
    Public Class UdfLsmProfileValue38
        Public Property Name As String
        Public Property ColumnIndex As Integer
        Public Property Value As Double
    End Class

    Public Class UdfGeeAtProfile38(Of TValue)
        Public Property TimeSpecified As Boolean = False
        Public Property TimeValue As Double = Double.NaN
        Public Property ProfileValues As New List(Of TValue)()
    End Class

    Public Class UdfMmrmAtProfile38(Of TValue)
        Public Property VisitSpecified As Boolean = False
        Public Property VisitValue As Double = Double.NaN
        Public Property ProfileValues As New List(Of TValue)()
    End Class

    '''+"''' <summary>Legacy family-specific profile grammars; cell interpretation remains a host policy.</summary>"+'''
    Public NotInheritable Class UdfModelProfilePreparation38
        Private Sub New()
        End Sub

        Public Delegate Function FiniteDoubleReader(cell As Object, ByRef value As Double) As Boolean

'''+ '\n\n'.join(methods)+'\n    End Class\nEnd Namespace\n'
 for token in ['ExcelArg','Global.BESHStatNG.WorksheetFunctions',' h,','h.']:assert token not in portable,token
 for path,text in [(GEE,g),(MMRM,m),(IMPORT,i),(COMMON,portable)]:put(path,text)
 with (HERE/'profile-extraction-v1.json').open('x',encoding='utf-8') as f:json.dump(dict(status='IMPLEMENTED_NOT_YET_COMPILED_OR_ACCEPTED',source_moves=records,additional=['MMRM headerless parser','MMRM scalar grouping column policy'],changed=[str(p) for p in [GEE,MMRM,IMPORT,COMMON]],numerical_policy_changes=0,host='Get2D/missing/error-cell policies, registration and model/cache remain Windows-owned'),f,indent=2)
 print('Profile preparation source move written; compilation and exact compatibility pending')
if __name__=='__main__':main()
