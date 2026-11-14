"""Remaining categorical reports, survival preparation and bounded native imports."""
from source_move_tools_v1 import *
for family,tag in [('MultinomialLogitUDFs','MNLOGIT'),('OrdinalLogitUDFs','ORDLOGIT')]:
 path='BESHstatNG/src/ExcelUDFs/Regression/'+family+'.vb';kind='Multinomial' if tag=='MNLOGIT' else 'Ordinal'
 aliases=[('h.Model.results','result'),('h.VarNames','names'),('h.PredictorCount','predictorCount'),('ExpForDisplay(','expForDisplay('),('Return PreparePValueTableForUdf(out, 6)','Return out')]
 sig='Public Shared Function Build'+kind+'Summary(result As LMresult, names() As String, predictorCount As Integer, hdr As Boolean, alphaValue As Double, expForDisplay As Func(Of Double, Object)) As Object(,)'
 segment(path,tag+'_SUMMARY','                Dim q As Integer','Return PreparePValueTableForUdf(out, 6)',sig,'                Return PreparePValueTableForUdf(Global.BESHStatNG.ExcelCommon.'+REPORT+'.Build'+kind+'Summary(h.Model.results, h.VarNames, '+('h.PredictorCount' if tag=='ORDLOGIT' else '0')+', hdr, alphaValue, AddressOf ExpForDisplay), 6)',REPORT,aliases)
 sig='Public Shared Function Build'+kind+'Tests(labels() As String, vals(,) As Object, hdr As Boolean) As Object(,)'
 segment(path,tag+'_TESTS','                Dim outRows As Integer','Return PreparePValueTableForUdf(out, 4)',sig,'                Return PreparePValueTableForUdf(Global.BESHStatNG.ExcelCommon.'+REPORT+'.Build'+kind+'Tests(labels, vals, hdr), 4)',REPORT,[('Return PreparePValueTableForUdf(out, 4)','Return out')])
 # n is originally computed before includeHeader conversion; preserve that read.
 report[-1]=report[-1].replace('Public Shared Function Build'+kind+'Tests(labels()','Public Shared Function Build'+kind+'Tests(n As Integer, labels()')
 move=moves[-1];move['portable']['signature']=move['portable']['signature'].replace('(labels()','(n As Integer, labels()')
 new=move['native_call'].replace('(labels, vals, hdr)','(n, labels, vals, hdr)');cache[path][1]=cache[path][1].replace(move['native_call'],new,1);move['native_call']=new
 sig='Public Shared Function Build'+kind+'Classification(cls As regression.ClassificationCrosstab, cats() As Integer, k As Integer, hdr As Boolean) As Object(,)'
 segment(path,tag+'_CLASS','                Dim outRows As Integer','Return out',sig,'                Return Global.BESHStatNG.ExcelCommon.'+REPORT+'.Build'+kind+'Classification(cls, cats, k, hdr)',REPORT)
 aliases=[('h.EquationParameterCount','equationParameterCount'),('h.InterceptIncluded','interceptIncluded'),('h.BaselineCategory','baselineCategory'),('h.PredictorCount','predictorCount'),('ComputeMultinomialEtas(','Global.BESHStatNG.WorksheetCompatibility.UdfNumericPreparation38.ComputeMultinomialEtas('),('ComputeMultinomialProbabilities(','Global.BESHStatNG.WorksheetCompatibility.UdfNumericPreparation38.ComputeMultinomialProbabilities('),('ComputeOrdinalProbabilities(','Global.BESHStatNG.WorksheetCompatibility.UdfNumericPreparation38.ComputeOrdinalProbabilities('),('ExcelError.ExcelErrorValue','invalid')]
 extra='equationParameterCount As Integer, interceptIncluded As Boolean, baselineCategory As Integer' if tag=='MNLOGIT' else 'predictorCount As Integer, invalid As Object'
 args='h.EquationParameterCount, h.InterceptIncluded, h.BaselineCategory' if tag=='MNLOGIT' else 'h.PredictorCount, ExcelError.ExcelErrorValue'
 sig='Public Shared Function Build'+kind+'Prediction(expandedX(,) As Double, b() As Double, cats() As Integer, offsetVals() As Double, nRows As Integer, hdr As Boolean, '+extra+') As Object'
 segment(path,tag+'_PRED','                Dim outRows As Integer','Return out',sig,'                Return Global.BESHStatNG.ExcelCommon.'+REPORT+'.Build'+kind+'Prediction(expandedX, b, cats, offsetVals, nRows, hdr, '+args+')',REPORT,aliases)
 if tag=='MNLOGIT':
  # k was validated before native nRows/includeHeader conversion; preserve it.
  report[-1]=report[-1].replace('baselineCategory As Integer) As Object','baselineCategory As Integer, k As Integer) As Object')
  move=moves[-1];move['portable']['signature']=move['portable']['signature'].replace('baselineCategory As Integer) As Object','baselineCategory As Integer, k As Integer) As Object');new=move['native_call'].replace('h.BaselineCategory)','h.BaselineCategory, k)');cache[path][1]=cache[path][1].replace(move['native_call'],new,1);move['native_call']=new

# DataObj is a Windows import object. Read its actual public dimensions and data
# in the original order, then retain the original complete integer conversion
# phase in Common; flexible Excel conversions remain explicitly host callbacks.
datafile='BESHstatNG/src/ExcelUDFs/UDFdataImport/UdfDataImport.vb'
lift(datafile,'ResponseColumnsMatchObjects','Public Shared Function ResponseColumnsMatchObjects(countData(,) As Object, zeroData(,) As Object, readNumber As Func(Of Object, Double?)) As Boolean','Return Global.BESHStatNG.ExcelCommon.'+PREP+'.ResponseColumnsMatchObjects(countData, zeroData, AddressOf Global.BESHStatNG.WorksheetFunctions.ExcelArgNumeric.TryGetDouble)',PREP,[('Global.BESHStatNG.WorksheetFunctions.ExcelArgNumeric.TryGetDouble','readNumber')])
lift(datafile,'TryExtractNonnegativeIntegerResponseObjects','Public Shared Function TryExtractNonnegativeIntegerResponseObjects(data(,) As Object, ByRef response() As Integer, readNumber As Func(Of Object, Double?)) As Boolean','Return Global.BESHStatNG.ExcelCommon.'+PREP+'.TryExtractNonnegativeIntegerResponseObjects(data, response, AddressOf Global.BESHStatNG.WorksheetFunctions.ExcelArgNumeric.TryGetDouble)',PREP,[('Global.BESHStatNG.WorksheetFunctions.ExcelArgNumeric.TryGetDouble','readNumber')])

surv='BESHstatNG/src/ExcelUDFs/UDFdataImport/UdfDataImportSurvival.vb'
readers=[('Global.BESHStatNG.WorksheetFunctions.ExcelArgNumeric.TryGetFiniteDoubleFlexible','readFinite'),('Global.BESHStatNG.WorksheetFunctions.ExcelArgNumeric.TryGetStatus01Flexible','readStatus'),('Global.BESHStatNG.WorksheetFunctions.ExcelArgReaders.CellToTrimmedText','readText')]
delegates='readFinite As UdfReadFiniteDouble38, readStatus As UdfReadStatusInteger38, readText As Func(Of Object, String)'
args='AddressOf Global.BESHStatNG.WorksheetFunctions.ExcelArgNumeric.TryGetFiniteDoubleFlexible, AddressOf Global.BESHStatNG.WorksheetFunctions.ExcelArgNumeric.TryGetStatus01Flexible, AddressOf Global.BESHStatNG.WorksheetFunctions.ExcelArgReaders.CellToTrimmedText'
for host,name,start,sigargs,callargs,suffixaliases in [
 ('TryGetKaplanMeierRecords','PrepareKaplanMeierRecords','        Dim map As New Dictionary', 'tArr(,) As Object, sArr(,) As Object, gArr(,) As Object, hasGroup As Boolean, ByRef records As List(Of survival.SurvivalRecord)', 'tArr, sArr, gArr, hasGroup, records', []),
 ('TryComputeSurvivalMedianCi','ComputeSurvivalMedianCi','        Dim tList As New List', 'timeArr(,) As Object, statusArr(,) As Object, groupArr(,) As Object, hasGroup As Boolean, alpha As Double, ByRef outTable(,) As Object, unavailable As Object','timeArr, statusArr, groupArr, hasGroup, alpha, outTable, ExcelError.ExcelErrorNA',[('CType(ExcelError.ExcelErrorNA, Object)','unavailable')]),
 ('TryComputeSurvivalLogRank','ComputeSurvivalLogRank','        Dim tList As New List','timeArr(,) As Object, statusArr(,) As Object, groupArr(,) As Object, strataArr(,) As Object, hasStrata As Boolean, weightMethod As String, ByRef result As TestResult, ByRef errText As String','timeArr, statusArr, groupArr, strataArr, hasStrata, weightMethod, result, errText',[])]:
 f=function(surv,host);old=f[f.index(start):f.rfind('    End Function')];body=norm(old).rstrip()
 for a,b in readers+suffixaliases:body=body.replace(a,b)
 sig='Public Shared Function '+name+'('+sigargs+', '+delegates+') As Boolean'
 prep.append('        '+sig+'\n'+body+'\n        End Function')
 eol='\r\r\n' if '\r\r\n' in old else '\r\n' if '\r\n' in old else '\n'
 call='        Return Global.BESHStatNG.ExcelCommon.'+PREP+'.'+name+'('+callargs+', '+args+')'
 change(surv,host,old,call+eol,dict(owner=PREP,signature=sig,body=body),[dict(before=x,after=y) for x,y in readers+suffixaliases])

# Missing/error/cell conversion precedence stays native; the pure selector is
# passed only the already read cell text after both original missing guards.
f=function(surv,'ParseSurvivalWeightMethod');start='        Select Case s.Trim().ToLowerInvariant()';old=f[f.index(start):f.rfind('    End Function')];body=norm(old).rstrip()
sig='Public Shared Function ParseSurvivalWeightText(s As String) As String';prep.append('        '+sig+'\n'+body+'\n        End Function')
eol='\r\r\n' if '\r\r\n' in old else '\r\n' if '\r\n' in old else '\n';change(surv,'ParseSurvivalWeightMethod',old,'        Return Global.BESHStatNG.ExcelCommon.'+PREP+'.ParseSurvivalWeightText(s)'+eol,dict(owner=PREP,signature=sig,body=body))

km='BESHstatNG/src/ExcelUDFs/SurvivalUDFs.vb'
f=function(km,'KM_TABLE');start='                Dim totalRows As Integer';end='Return result'
segment(km,'KM_TABLE',start,end,'Public Shared Function BuildKaplanMeierTable(outObj As Object(), noRows As Object) As Object','                Return Global.BESHStatNG.ExcelCommon.'+REPORT+'.BuildKaplanMeierTable(outObj, ExcelError.ExcelErrorNum)',REPORT,[('ExcelError.ExcelErrorNum','noRows')])

for name,methods,role in [(REPORT,report,'Results'),(PREP,prep,'Analysis')]:
 p='BESHStat.Excel.Common/src/'+role+'/'+name+'.vb';old=read(p);end=old.rfind('    End Class');assert end>=0
 cache[p][1]=old[:end]+'\n\n'.join(methods)+'\n'+old[end:]
 if name==PREP:
  # Public typed delegates bind the actual ByRef Windows conversion methods.
  cache[p][1]=cache[p][1].replace('Namespace ExcelCommon\n','Namespace ExcelCommon\n    Public Delegate Function UdfReadFiniteDouble38(value As Object, ByRef parsed As Double) As Boolean\n    Public Delegate Function UdfReadStatusInteger38(value As Object, ByRef parsed As Integer) As Boolean\n',1)
for path,(raw,text) in cache.items():
 data=text.encode('utf-8');data=b'\xef\xbb\xbf'+data if raw.startswith(b'\xef\xbb\xbf') else data;(ROOT/path).write_bytes(data)
save('remaining-packets-source-moves-v1.json',dict(moves=moves,before=before,current=[pin(ROOT/p) for p in cache],original_queries=0,tests_run=False))
print('Substantial remaining packet',len(moves),'complete preparation/report phases consumed by native callers',flush=True)
