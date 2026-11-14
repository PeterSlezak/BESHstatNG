"""Portable remaining name/report/model-request composition; host policies remain native."""
from source_move_tools_v1 import *
# Unique per-packet preimages, including the two incrementally built Common files.
def read_packet(path):
 if path not in cache:
  raw=(ROOT/path).read_bytes();target=HERE/'mmrm-zip-before'/path;target.parent.mkdir(parents=True,exist_ok=True)
  with target.open('xb') as f:f.write(raw)
  cache[path]=[raw,raw.decode('utf-8-sig')];before.append(dict(path=path,live=pin(ROOT/path),preimage=pin(target)))
 return cache[path][1]
read=read_packet
# The helpers imported above resolve read in their defining module namespace.
import source_move_tools_v1 as mt
mt.read=read_packet
mm='BESHstatNG/src/ExcelUDFs/Regression/MixedModelUDFs.vb'
f=function(mm,'StackResultTables');old=f[f.index('            If rendered.Count = 0'):f.rfind('        End Function')];body=norm(old).rstrip()
sig='Public Shared Function StackRenderedMmrmTables(rendered As List(Of Object(,)), totalRows As Integer, maxCols As Integer) As Object(,)'
report.append('        '+sig+'\n'+body+'\n        End Function');eol='\r\r\n' if '\r\r\n' in old else '\r\n' if '\r\n' in old else '\n'
change(mm,'StackResultTables',old,'            Return Global.BESHStatNG.ExcelCommon.'+REPORT+'.StackRenderedMmrmTables(rendered, totalRows, maxCols)'+eol,dict(owner=REPORT,signature=sig,body=body))
f=function(mm,'EnsureMmrmResultUsesHandleFixedEffectNames');body=phase(f)
guard='If h Is Nothing OrElse h.Result Is Nothing Then Exit Sub';assert body.startswith(guard)
oldbody=body[len(guard):].strip();pairs=[('h.FixedEffectNames','names'),('h.Result','result')]
for a,b in pairs:oldbody=oldbody.replace(a,b)
sig='Public Shared Sub ApplyMmrmHandleNames(result As Global.BESHStatNG.regression.MixedModelResult, names() As String)'
report.append('        '+sig+'\n'+oldbody+'\n        End Sub')
a,b=call_body(mm,'EnsureMmrmResultUsesHandleFixedEffectNames',guard+'\n            Global.BESHStatNG.ExcelCommon.'+REPORT+'.ApplyMmrmHandleNames(h.Result, h.FixedEffectNames)');change(mm,'EnsureMmrmResultUsesHandleFixedEffectNames',a,b,dict(owner=REPORT,signature=sig,body=oldbody,native_guard=guard),[dict(before=x,after=y) for x,y in pairs])

# Keep native y/x/visit imports, covariance Object reconversion, maxIter parsing,
# handle registration/cache and exception context. Move the exact typed request
# initialization/control defaults to Common without modifying any Core engine.
f=function(mm,'MMRM_FIT');start='                Dim req As regression.MixedModelFitRequest';end='                control.CovarianceGradientMode = gradientMode'
sig='Public Shared Function CreateMmrmFitRequest(data As Global.BESHStatNG.regression.MixedModelBlockData, rStruct As Global.BESHStatNG.regression.MixedModelRStruct, fit As Global.BESHStatNG.regression.MixedModelFitMethod, fitNames() As String, fixedInference As Global.BESHStatNG.regression.MixedModelFixedInferenceMethod, visitValues() As Double, traceValue As Boolean, optimizerMode As Global.BESHStatNG.regression.MixedModelCovarianceOptimizerMode, gradientMode As Global.BESHStatNG.regression.MixedModelCovarianceGradientMode) As Global.BESHStatNG.regression.MixedModelFitRequest'
segment(mm,'MMRM_FIT',start,end,sig,'                Dim req As regression.MixedModelFitRequest = Global.BESHStatNG.ExcelCommon.'+PREP+'.CreateMmrmFitRequest(data, rStruct, fit, fitNames, fixedInference, visitValues, traceValue, optimizerMode, gradientMode)\n                Dim control As regression.MixedModelControl = req.Control',PREP,[('regression.','Global.BESHStatNG.regression.')])
prep[-1]=prep[-1].replace('gradientMode As Global.BESHStatNG.regression.MixedModelCovarianceGradientMode) As','gradientMode As Global.BESHStatNG.regression.MixedModelCovarianceGradientMode, ByRef control As Global.BESHStatNG.regression.MixedModelControl) As').replace('Dim control As Global.BESHStatNG.regression.MixedModelControl = req.Control','control = req.Control').replace('\n        End Function','\n            Return req\n        End Function')
move=moves[-1];move['portable']['suffix']='Return req';move['portable']['control_structure_out']='Original control struct is initialized and modified once, passed ByRef to the native local; the native req.Control assignment remains at its original post-maxIter position.'
newcall='                Dim control As regression.MixedModelControl\n                Dim req As regression.MixedModelFitRequest = Global.BESHStatNG.ExcelCommon.'+PREP+'.CreateMmrmFitRequest(data, rStruct, fit, fitNames, fixedInference, visitValues, traceValue, optimizerMode, gradientMode, control)'
newcall=newcall.replace('\n','\r\n' if '\r\n' in move['native_call'] else '\n');cache[mm][1]=cache[mm][1].replace(move['native_call'],newcall,1);move['native_call']=newcall

# Native predictor import and zero-column/offset precedence remain in their
# original wrappers. Typed design expansion is a reusable Core orchestration.
zipfile='BESHstatNG/src/ExcelUDFs/Regression/ZipUDFs.vb'
for host,prefix in [('TryPrepareZipCountPredictionInputs','Count'),('TryPrepareZipZeroPredictionInputs','Zero')]:
 f=function(zipfile,host);old=f[f.index('            Dim expandedNames() As String'):f.rfind('        End Function')];body=norm(old).rstrip()
 pairs=[('h.'+prefix+'DesignSpec','designSpec'),('h.'+prefix+'OmitCategoricalReference','omitCategoricalReference'),('h.'+prefix+'ExpandedPredictorNames','expectedNames'),('imported.DataDbl','rawX'),('imported.nRows','importedRows'),('imported.bOffset','importedHasOffset'),('imported.OffsetData','importedOffset'),('UdfDataImport.HasOnlyFinite','Global.BESHStatNG.WorksheetCompatibility.UdfNumericPreparation38.HasOnlyFinite')]
 for a,b in pairs:body=body.replace(a,b)
 extra=', importedHasOffset As Boolean, importedOffset() As Double, ByRef offsetVals() As Double' if prefix=='Count' else ''
 sig='Public Shared Function PrepareZip'+prefix+'Design(rawX(,) As Double, rawPredictorKeys() As String, designSpec As RegressionFormulaDesignSpec, omitCategoricalReference As Boolean, expectedNames() As String, importedRows As Integer, ByRef expandedX(,) As Double, ByRef nRows As Integer'+extra+') As Boolean'
 prep.append('        '+sig+'\n'+body+'\n        End Function');eol='\r\r\n' if '\r\r\n' in old else '\r\n' if '\r\n' in old else '\n'
 args=', imported.bOffset, imported.OffsetData, offsetVals' if prefix=='Count' else ''
 call='            Return Global.BESHStatNG.ExcelCommon.'+PREP+'.PrepareZip'+prefix+'Design(imported.DataDbl, rawPredictorKeys, h.'+prefix+'DesignSpec, h.'+prefix+'OmitCategoricalReference, h.'+prefix+'ExpandedPredictorNames, imported.nRows, expandedX, nRows'+args+')'
 change(zipfile,host,old,call+eol,dict(owner=PREP,signature=sig,body=body),[dict(before=x,after=y) for x,y in pairs])

# ZIP prediction retains the existing Core linear-predictor implementation,
# logistic routine, and fixed Windows number/error display callback.
sig='Public Shared Function BuildZipPredictionRows(countExpandedX(,) As Double, zeroExpandedX(,) As Double, betaCount() As Double, betaZero() As Double, countIntercept As Boolean, zeroIntercept As Boolean, offsetVals() As Double, nRows As Integer, safeNumber As Func(Of Double, Object)) As List(Of Object())'
f=function(zipfile,'ZIP_PRED');start='                Dim rows As New List(Of Object())';end='                Next'
segment(zipfile,'ZIP_PRED',start,end,sig,'                Dim rows As List(Of Object()) = Global.BESHStatNG.ExcelCommon.'+REPORT+'.BuildZipPredictionRows(countExpandedX, zeroExpandedX, betaCount, betaZero, h.CountInterceptIncluded, h.ZeroInterceptIncluded, offsetVals, nRows, AddressOf SafeExcelNumber)',REPORT,[('h.CountInterceptIncluded','countIntercept'),('h.ZeroInterceptIncluded','zeroIntercept'),('SafeExcelNumber(','safeNumber('),('ComputeLinearPredictor(','Global.BESHStatNG.WorksheetCompatibility.UdfNumericPreparation38.ComputeLinearPredictor('),('regression.','Global.BESHStatNG.regression.')])
report[-1]=report[-1].replace('\n        End Function','\n            Return rows\n        End Function');moves[-1]['portable']['suffix']='Return rows'

for name,methods,role in [(REPORT,report,'Results'),(PREP,prep,'Analysis')]:
 p='BESHStat.Excel.Common/src/'+role+'/'+name+'.vb';s=read_packet(p);end=s.rfind('    End Class');assert end>=0;cache[p][1]=s[:end]+'\n\n'.join(methods)+'\n'+s[end:]
for path,(raw,text) in cache.items():
 data=text.encode('utf-8');data=b'\xef\xbb\xbf'+data if raw.startswith(b'\xef\xbb\xbf') else data;(ROOT/path).write_bytes(data)
save('mmrm-zip-source-moves-v1.json',dict(moves=moves,before=before,current=[pin(ROOT/p) for p in cache],original_queries=0,tests_run=False))
print('MMRM/ZIP packet',len(moves),'typed report/request/design phases consumed by Windows',flush=True)
