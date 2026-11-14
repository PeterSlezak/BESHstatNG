"""Coherent exact-source report, MMRM row/design, ZIP and survival preparation moves."""
from common import *
import re
assert json.loads((HERE/'entry-authentication-v1.json').read_bytes())['status']=='ENTRY_GATES_PASS'
moves=[];before=[];cache={};report=[];prep=[]
REPORT='UdfW2CompletionReports38';PREP='UdfW2CompletionPreparation38'
def read(path):
 if path not in cache:
  raw=(ROOT/path).read_bytes();target=HERE/'shared-packets-before'/path;target.parent.mkdir(parents=True,exist_ok=True)
  with target.open('xb') as f:f.write(raw)
  cache[path]=[raw,raw.decode('utf-8-sig')];before.append(dict(path=path,live=pin(ROOT/path),preimage=pin(target)))
 return cache[path][1]
def function(path,name):
 s=read(path);m=re.search(r'(?m)^[ \t]*(?:Private|Friend|Public)(?: Shared)? (Function|Sub) '+re.escape(name)+r'\b',s);assert m,(path,name)
 end=re.search(r'(?m)^[ \t]*End '+m[1]+r'\b',s[m.start():]);assert end
 return s[m.start():m.start()+end.end()]
def norm(s):return s.replace('\r','')
def phase(s):
 s=norm(s);m=re.search(r'\b(Function|Sub)\s+\w+\(',s);assert m
 start=s.index('(',m.start());depth=0;quoted=False;i=start
 while i<len(s):
  ch=s[i]
  if ch=='"':
   if quoted and i+1<len(s) and s[i+1]=='"':i+=2;continue
   quoted=not quoted
  if not quoted:
   if ch=='(':depth+=1
   if ch==')':
    depth-=1
    if depth==0:break
  i+=1
 return s[s.index('\n',i)+1:s.rfind('End '+m[1])].strip()
def change(path,host,old,new,portable,aliases=None):
 text=read(path);assert text.count(old)==1,(path,host,old[:80]);cache[path][1]=text.replace(old,new,1)
 moves.append(dict(path=path,host=host,original_segment=old,native_call=new,portable=portable,aliases=aliases or [],original_queries=0))
def call_body(path,host,call):
 old=function(path,host);s=norm(old);start=s.index(phase(s));ending=list(re.finditer(r'(?m)^[ \t]*End (?:Function|Sub)\b',s))[-1];new=s[:start]+call+'\n'+s[ending.start():]
 eol='\r\r\n' if '\r\r\n' in old else '\r\n' if '\r\n' in old else '\n'
 return old,new.replace('\n',eol)
def lift(path,host,signature,call,dest,aliases=None):
 old=function(path,host);body=phase(old);pairs=aliases or []
 for a,b in pairs:body=body.replace(a,b)
 portable='        '+signature+'\n'+ '\n'.join('            '+line.lstrip() if not line.startswith(' ') else line for line in body.splitlines())+'\n        End '+('Sub' if ' Sub ' in signature else 'Function')
 (report if dest==REPORT else prep).append(portable)
 a,b=call_body(path,host,call);change(path,host,a,b,dict(owner=dest,signature=signature,body=body),[dict(before=x,after=y) for x,y in pairs])
def segment(path,host,start,end,signature,call,dest,aliases=None):
 f=function(path,host);a=f.index(start);b=f.index(end,a)+len(end);old=f[a:b];body=norm(old);pairs=aliases or []
 for x,y in pairs:body=body.replace(x,y)
 portable='        '+signature+'\n'+body+'\n        End '+('Sub' if ' Sub ' in signature else 'Function')
 (report if dest==REPORT else prep).append(portable)
 eol='\r\r\n' if '\r\r\n' in old else '\r\n' if '\r\n' in old else '\n'
 change(path,host,old,call.replace('\n',eol),dict(owner=dest,signature=signature,body=body),[dict(before=x,after=y) for x,y in pairs])

# Existing Core result contracts are public typed data. Excel error values remain
# caller-supplied materialization data, with no Excel-DNA reference in Common.
for file,name,extra in [('MultinomialLogitUDFs','Multinomial',', categories() As Integer'),('OrdinalLogitUDFs','Ordinal','')]:
 p='BESHstatNG/src/ExcelUDFs/Regression/'+file+'.vb'
 args='res, '+('categories, ' if extra else '')+'includeHeader, ExcelError.ExcelErrorNA'
 lift(p,'BuildAllResidualOutput','Public Shared Function Build'+name+'ResidualOutput(res As regression.MultinomialResiduals'+extra+', includeHeader As Boolean, unavailable As Object) As Object','Return Global.BESHStatNG.ExcelCommon.'+REPORT+'.Build'+name+'ResidualOutput('+args+')',REPORT,[('ExcelError.ExcelErrorNA','unavailable'),('CategoryHeaders(','UdfRegressionOutputReports38.CategoryHeaders(')])

zipfile='BESHstatNG/src/ExcelUDFs/Regression/ZipUDFs.vb'
lift(zipfile,'AppendZipSummaryRows','Public Shared Sub AppendZipSummaryRows(rows As List(Of Object()), componentLabel As String, predictorNames() As String, coeffs() As Double, ses() As Double, interceptIncluded As Boolean, ciAlpha As Double)','Global.BESHStatNG.ExcelCommon.'+REPORT+'.AppendZipSummaryRows(rows, componentLabel, predictorNames, coeffs, ses, interceptIncluded, ciAlpha)',REPORT)
lift(zipfile,'MaterializeRows','Public Shared Function MaterializeZipRows(rows As List(Of Object()), includeHeader As Boolean, headers() As String) As Object','Return Global.BESHStatNG.ExcelCommon.'+REPORT+'.MaterializeZipRows(rows, includeHeader, headers)',REPORT)

# MMRM consumes the exact existing LMM table composition, retaining its native
# handle guards, missing-array defaults and report/error lifetime.
mm='BESHstatNG/src/ExcelUDFs/Regression/MixedModelUDFs.vb'
for name,call in [('MMRM_FITTED','BuildFittedTable(fitted, hdr)'),('MMRM_RESID','BuildResidualTable(fitted, resid, n, hdr)')]:
 f=function(mm,name);start=next(line for line in f.splitlines() if 'Dim out(' in line)
 segment(mm,name,start,'Return out','Public Shared Function Unused'+name+'() As Object','Return Global.BESHStatNG.ExcelCommon.UdfLmmReportPreparation38.'+call,REPORT)
 # The destination already exists and is independently source/CIL reconciled.
 report.pop();moves[-1]['portable']=dict(owner='UdfLmmReportPreparation38',existing=True,call=call,body=norm(moves[-1]['original_segment']))

# Required-column selection uses Core parser/catalog, preserving all real lambdas
# and their source-order capture. Worksheet address/name import remains native.
mixed='BESHstatNG/src/ExcelUDFs/UDFdataImport/UdfDataImportMixedModel.vb'
f=function(mixed,'TryResolveMmrmFormulaRequiredXColumns');start='        Dim normalizedFormula As String'
segment(mixed,'TryResolveMmrmFormulaRequiredXColumns',start,'Return True\r\n    End Function' if False else '        Return True',
 'Public Shared Function ResolveMmrmFormulaColumns(formulaText As String, formulaAddressingMode As String, pAll As Integer, allDisplayNames() As String, allAbsoluteLetters() As String, hasAbsoluteLetters As Boolean, ByRef columnIndices() As Integer, ByRef selectedNames() As String, ByRef selectedAbsoluteLetters() As String, ByRef errorMessage As String) As Boolean',
 '        Return Global.BESHStatNG.ExcelCommon.'+PREP+'.ResolveMmrmFormulaColumns(formulaText, formulaAddressingMode, pAll, allDisplayNames, allAbsoluteLetters, hasAbsoluteLetters, columnIndices, selectedNames, selectedAbsoluteLetters, errorMessage)',PREP)
# Select the entire suffix, not the first early Return True: correct the planned
# range before writing using the authenticated untouched method source below.
move=moves.pop();prep.pop();cache[mixed][1]=cache[mixed][1].replace(move['native_call'],move['original_segment'],1)
f=function(mixed,'TryResolveMmrmFormulaRequiredXColumns');old=f[f.index(start):f.rfind('    End Function')];body=norm(old).rstrip()
sig='Public Shared Function ResolveMmrmFormulaColumns(formulaText As String, formulaAddressingMode As String, pAll As Integer, allDisplayNames() As String, allAbsoluteLetters() As String, hasAbsoluteLetters As Boolean, ByRef columnIndices() As Integer, ByRef selectedNames() As String, ByRef selectedAbsoluteLetters() As String, ByRef errorMessage As String) As Boolean'
prep.append('        '+sig+'\n'+body+'\n        End Function')
call='        Return Global.BESHStatNG.ExcelCommon.'+PREP+'.ResolveMmrmFormulaColumns(formulaText, formulaAddressingMode, pAll, allDisplayNames, allAbsoluteLetters, hasAbsoluteLetters, columnIndices, selectedNames, selectedAbsoluteLetters, errorMessage)'
eol='\r\r\n' if '\r\r\n' in old else '\r\n' if '\r\n' in old else '\n'
change(mixed,'TryResolveMmrmFormulaRequiredXColumns',old,call+eol,dict(owner=PREP,signature=sig,body=body))

f=function(mixed,'TryGetMmrmAlignedInputs');old=f[f.index('        Dim p As Integer = xColumnIndices.Length'):f.rfind('    End Function')];body=norm(old).rstrip()
pairs=[('Global.BESHStatNG.WorksheetFunctions.ExcelArgNumeric.TryGetDouble','readNumber'),('Global.BESHStatNG.WorksheetFunctions.ExcelArgReaders.CellToTrimmedText','readText')]
for a,b in pairs:body=body.replace(a,b)
sig='Public Shared Function AlignMmrmRows(yCol(,) As Object, xRaw(,) As Object, subjectCol(,) As Object, visitCol(,) As Object, xColumnIndices() As Integer, visitSupplied As Boolean, n As Integer, readNumber As Func(Of Object, Double?), readText As Func(Of Object, String), ByRef yValues() As Double, ByRef xValues(,) As Double, ByRef subjectValues() As Object, ByRef visitValues() As Double, ByRef droppedRows As Integer, ByRef errorMessage As String) As Boolean'
prep.append('        '+sig+'\n'+body+'\n        End Function')
call='        Return Global.BESHStatNG.ExcelCommon.'+PREP+'.AlignMmrmRows(yCol, xRaw, subjectCol, visitCol, xColumnIndices, visitSupplied, n, AddressOf Global.BESHStatNG.WorksheetFunctions.ExcelArgNumeric.TryGetDouble, AddressOf Global.BESHStatNG.WorksheetFunctions.ExcelArgReaders.CellToTrimmedText, yValues, xValues, subjectValues, visitValues, droppedRows, errorMessage)'
eol='\r\r\n' if '\r\r\n' in old else '\r\n' if '\r\n' in old else '\n';change(mixed,'TryGetMmrmAlignedInputs',old,call+eol,dict(owner=PREP,signature=sig,body=body),[dict(before=x,after=y) for x,y in pairs])

# Native string conversion remains outside the portable reference-category policy.
opts='BESHstatNG/src/ExcelUDFs/UDFdataImport/UdfDataImportRegressionOptions.vb'
old=function(opts,'GetReferenceCategory');b=phase(old);first='Dim s As String = Global.BESHStatNG.WorksheetFunctions.ExcelArgReaders.AsString(arg)';assert b.startswith(first)
body=b[len(first):].strip();sig='Public Shared Function GetReferenceCategory(s As String) As regression.ReferenceCategory'
prep.append('        '+sig+'\n'+body+'\n        End Function')
a,new=call_body(opts,'GetReferenceCategory','Return Global.BESHStatNG.ExcelCommon.'+PREP+'.GetReferenceCategory(Global.BESHStatNG.WorksheetFunctions.ExcelArgReaders.AsString(arg))');change(opts,'GetReferenceCategory',a,new,dict(owner=PREP,signature=sig,body=body,one_native_conversion=True))

for name,methods,role in [(REPORT,report,'Results'),(PREP,prep,'Analysis')]:
 path='BESHStat.Excel.Common/src/'+role+'/'+name+'.vb';text='Option Explicit On\nOption Strict On\nOption Infer On\nImports System\nImports System.Collections.Generic\nImports System.Globalization\nImports System.Linq\nNamespace ExcelCommon\n    Public NotInheritable Class '+name+'\n        Private Sub New()\n        End Sub\n\n'+'\n\n'.join(methods)+'\n    End Class\nEnd Namespace\n'
 with (ROOT/path).open('x',encoding='utf-8',newline='\n') as out:out.write(text)
for path,(raw,text) in cache.items():
 data=text.encode('utf-8');data=(b'\xef\xbb\xbf'+data) if raw.startswith(b'\xef\xbb\xbf') else data;(ROOT/path).write_bytes(data)
save('shared-packets-source-moves-v1.json',dict(moves=moves,before=before,current=[pin(ROOT/p) for p in cache],new_common=[pin(ROOT/('BESHStat.Excel.Common/src/'+role+'/'+name+'.vb')) for name,role in [(REPORT,'Results'),(PREP,'Analysis')]],original_queries=0,tests_run=False))
print('Coherent shared packet',len(moves),'complete source phases moved; no unit/Python/Node campaign',flush=True)
