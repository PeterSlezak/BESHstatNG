"""Final reusable formula/name/inference setup phases; existing engines reused unchanged."""
from source_move_tools_v1 import *
def local_read(path):
 if path not in cache:
  raw=(ROOT/path).read_bytes();target=HERE/'formula-mmrm-tail-before'/path;target.parent.mkdir(parents=True,exist_ok=True)
  with target.open('xb') as f:f.write(raw)
  cache[path]=[raw,raw.decode('utf-8-sig')];before.append(dict(path=path,live=pin(ROOT/path),preimage=pin(target)))
 return cache[path][1]
import source_move_tools_v1 as mt
mt.read=local_read;read=local_read
formula='BESHstatNG/src/ExcelUDFs/Regression/RegressionFormulaUDFs.vb'
f=function(formula,'FORMULA_VALIDATE');start='                Dim designBuild As RegressionFormulaMatrixBuildResult'
end='                                                     parserOrBuilderMessage:=designErr)'
sig='Public Shared Function ValidateWorksheetFormula(xMat(,) As Double, rawVarNames() As String, formulaText As String, addressingMode As String, absoluteColumnLetters() As String, allowRelativeColumnLetters As Boolean, allowAbsoluteColumnLetters As Boolean, allowQuotedVariableNames As Boolean) As Object'
segment(formula,'FORMULA_VALIDATE',start,end,sig,'                Return Global.BESHStatNG.ExcelCommon.'+PREP+'.ValidateWorksheetFormula(xMat, rawVarNames, formulaText, addressingMode, absoluteColumnLetters, allowRelativeColumnLetters, allowAbsoluteColumnLetters, allowQuotedVariableNames)',PREP,[('BuildValidationFailureMessage(','RegressionFormulaUDFsMetadata38.BuildValidationFailureMessage(')])
mm='BESHstatNG/src/ExcelUDFs/Regression/MixedModelUDFs.vb'
sig='Public Shared Sub ApplyMmrmInference(req As Global.BESHStatNG.regression.MixedModelFitRequest, fixedInference As Global.BESHStatNG.regression.MixedModelFixedInferenceMethod)'
segment(mm,'MMRM_FIT','                Select Case fixedInference','                End Select',sig,'                Global.BESHStatNG.ExcelCommon.'+PREP+'.ApplyMmrmInference(req, fixedInference)',PREP,[('regression.','Global.BESHStatNG.regression.')])
f=function(mm,'ResolveMmrmImportedPredictorNames');start='                    Dim byLetter As New Dictionary';end='                    If anyMapped Then Return NormalizeMmrmNameList(mapped, selectedCount, "X")'
sig='Public Shared Function MapMmrmNamesByLetter(suppliedNames() As String, suppliedLetters() As String, selectedAbsoluteLetters() As String, inferred() As String, selectedCount As Integer) As String()'
segment(mm,'ResolveMmrmImportedPredictorNames',start,end,sig,'                    Return Global.BESHStatNG.ExcelCommon.'+PREP+'.MapMmrmNamesByLetter(suppliedNames, suppliedLetters, selectedAbsoluteLetters, inferred, selectedCount)',PREP,[('NormalizeMmrmNameList(','MixedModelUDFsMetadata38.NormalizeMmrmNameList(')])
prep[-1]=prep[-1].replace('\n        End Function','\n            Return inferred\n        End Function');moves[-1]['portable']['suffix']='Return inferred'
# The original code returns inferred immediately after leaving the two nested
# guards when no mapping exists; both paths have no further side effects.
path='BESHStat.Excel.Common/src/Analysis/'+PREP+'.vb';s=local_read(path);end=s.rfind('    End Class');cache[path][1]=s[:end]+'\n\n'.join(prep)+'\n'+s[end:]
for path,(raw,text) in cache.items():
 data=text.encode('utf-8');data=b'\xef\xbb\xbf'+data if raw.startswith(b'\xef\xbb\xbf') else data;(ROOT/path).write_bytes(data)
save('formula-mmrm-tail-source-moves-v1.json',dict(moves=moves,before=before,current=[pin(ROOT/p) for p in cache],original_queries=0,tests_run=False))
print('Formula/MMRM tail',len(moves),'source-bound portable phases with native imports/session/error boundaries retained',flush=True)
