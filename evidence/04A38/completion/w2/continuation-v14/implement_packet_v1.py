"""Exact native segments and reusable typed PSM preparations; no numerical rewrite."""
from common import *
import re
plan=json.loads((HERE/'implementation-plan-v1.json').read_bytes());assert len(plan['source_paths'])==7
for r in plan['source_before']:assert pin(ROOT/r['live']['path'])==r['live']
decode=lambda p:(ROOT/p).read_bytes().decode('utf-8-sig').replace('\r\n','\n')
moves=[]
path='BESHstatNG/src/ExcelUDFs/PropensityScoreUDFs.vb';text=decode(path)
old='''            Dim fitOptions As New PsmComprehensiveFitOptions()
            fitOptions.StandardOptions = New PsmOptions()
''';new='''            Dim fitOptions As PsmComprehensiveFitOptions = Global.BESHStatNG.ExcelCommon.UdfPsmDataPreparation38.CreateFitOptionsPrepared()
'''
assert text.count(old)==1;text=text.replace(old,new,1);moves.append(dict(path=path,parent='BuildFitOptions',api='CreateFitOptionsPrepared',original_segment=old,actual_native_call=new,portable_segment=old+'            Return fitOptions\n',bindings={}))
old='''            NormalizeFitOptions(fitOptions)
            PsmMethodCapabilities.ValidateFitOptions(fitOptions)
            Return fitOptions
''';new='''            Return Global.BESHStatNG.ExcelCommon.UdfPsmDataPreparation38.CompleteFitOptionsPrepared(fitOptions)
'''
assert text.count(old)==1;text=text.replace(old,new,1);moves.append(dict(path=path,parent='BuildFitOptions',api='CompleteFitOptionsPrepared',original_segment=old,actual_native_call=new,portable_segment=old.replace('NormalizeFitOptions(fitOptions)','UdfPsmPreparation38.NormalizeFitOptions(fitOptions)'),bindings={'NormalizeFitOptions':'UdfPsmPreparation38.NormalizeFitOptions'}))
a=text.index('        Private Function ResolveNamedTable(');b=text.index('        End Function',a);segment=text[a:b];case_lines=re.findall(r'(?m)^                Case (.+)$',segment);aliases=[x for x in case_lines if x!='Else'];names=['RunSummary','Options','ImportSummary','SampleSize','ScoreModel','RowAudit','Matches','MatchedData','Balance','Effect','DoublyRobust','WeightDiagnostics','OverlapSummary','OverlapBins','LovePlot','Subclass','CemStrata','CemWeights','Warnings'];assert len(aliases)==len(names)==19
oldhead='            Dim name As String = NormalizeOptionKey(requested)\n';newhead='            Dim name As Global.BESHStatNG.ExcelCommon.PsmUdfTableKind38 = Global.BESHStatNG.ExcelCommon.UdfPsmDataPreparation38.ResolveTableKindPrepared(requested)\n'
changed=segment.replace(oldhead,newhead,1)
for keys,name in zip(aliases,names):changed=changed.replace('                Case '+keys+'\n','                Case Global.BESHStatNG.ExcelCommon.PsmUdfTableKind38.'+name+'\n',1)
text=text[:a]+changed+text[b:]
route='            Dim name As String = PropensityScoreUDFsOptions38.NormalizeOptionKey(requested)\n'
allkeys=', '.join(keys for keys in aliases)
route+='            Select Case LegacyUdfText38.CaseToken(name, New String() {'+allkeys+'})\n'
for keys,name in zip(aliases,names):route+='                Case '+keys+'\n                    Return PsmUdfTableKind38.'+name+'\n'
route+='                Case Else\n                    Return PsmUdfTableKind38.Unknown\n            End Select\n'
moves.append(dict(path=path,parent='ResolveNamedTable',api='ResolveTableKindPrepared',original_whole_prefix=segment,current_whole_prefix=changed,portable_segment=route,case_bindings=[dict(original_case=keys,portable_case=name) for keys,name in zip(aliases,names)],bindings={'NormalizeOptionKey':'PropensityScoreUDFsOptions38.NormalizeOptionKey'}))
old='''            Dim table(rows.Count, 1) As Object
            table(0, 0) = "Item"
            table(0, 1) = "Value"
            For i As Integer = 0 To rows.Count - 1
                table(i + 1, 0) = rows(i)(0)
                table(i + 1, 1) = rows(i)(1)
            Next
            Return table
''';new='''            Return Global.BESHStatNG.ExcelCommon.UdfPsmDataPreparation38.BuildImportSummaryPrepared(rows)
'''
assert text.count(old)==1;text=text.replace(old,new,1);moves.append(dict(path=path,parent='DataImportSummaryTable',api='BuildImportSummaryPrepared',original_segment=old,actual_native_call=new,portable_segment=old,bindings={}))
write_source(path,text)
path='BESHstatNG/src/ExcelUDFs/UDFdataImport/UdfDataImportPsm.vb';text=decode(path)
for api,begin,end,new in [
 ('BuildModelNamesPrepared','        Dim modelNames(p) As String\n','        Next\n','        Dim modelNames() As String = Global.BESHStatNG.ExcelCommon.UdfPsmDataPreparation38.BuildModelNamesPrepared(treatmentKey, covariateNames, p)\n'),
 ('BuildModelInputPrepared','        Dim modelInput(n - 1, p) As Object\n','        Next\n','        Dim modelInput(,) As Object = Global.BESHStatNG.ExcelCommon.UdfPsmDataPreparation38.BuildModelInputPrepared(treatmentCol, covariateMatrix, n, p)\n')]:
 a=text.index(begin)
 if api=='BuildModelInputPrepared':b=text.index('        Dim scoreInput',a);old=text[a:b].rstrip()+'\n'
 else:b=text.index(end,a)+len(end);old=text[a:b]
 text=text[:a]+text[a:].replace(old,new,1)
 moves.append(dict(path=path,parent='TryGetPsmData',api=api,original_segment=old,actual_native_call=new,portable_segment=old+'        Return '+('modelNames' if api=='BuildModelNamesPrepared' else 'modelInput')+'\n',bindings={}))
write_source(path,text)
path='BESHStat.Excel.Common/src/Analysis/UdfPsmDataPreparation38.vb';assert not (ROOT/path).exists()
signatures=dict(CreateFitOptionsPrepared='() As PsmComprehensiveFitOptions',CompleteFitOptionsPrepared='(fitOptions As PsmComprehensiveFitOptions) As PsmComprehensiveFitOptions',ResolveTableKindPrepared='(requested As String) As PsmUdfTableKind38',BuildImportSummaryPrepared='(rows As IList(Of Object())) As Object(,)',BuildModelNamesPrepared='(treatmentKey As String, covariateNames() As String, p As Integer) As String()',BuildModelInputPrepared='(treatmentCol(,) As Object, covariateMatrix(,) As Object, n As Integer, p As Integer) As Object(,)')
output='Option Explicit On\nOption Strict On\nOption Infer On\n\nImports System\nImports System.Collections.Generic\nImports BESHStatNG.CausalInference\n\nNamespace ExcelCommon\n    Public Enum PsmUdfTableKind38\n        Unknown\n'+''.join('        '+n+'\n' for n in names)+'    End Enum\n\n    Public NotInheritable Class UdfPsmDataPreparation38\n        Private Sub New()\n        End Sub\n\n'
for m in moves:
 code=m['portable_segment'];lines=code.splitlines();indent=min(len(line)-len(line.lstrip()) for line in lines if line.strip());code='\n'.join('            '+line[indent:] if line.strip() else '' for line in lines)+'\n'
 output+='        Public Shared Function '+m['api']+signatures[m['api']]+'\n'+code+'        End Function\n\n';m['portable_signature']=m['api']+signatures[m['api']];m['portable_segment']=code
output+='    End Class\nEnd Namespace\n';(ROOT/path).write_text(output,encoding='utf-8',newline='\n')
path='BESHStat.Excel.Common/src/Analysis/UdfClassLabelPreparation38.vb';text=decode(path)
addition='''
        ' The native wrapper has already reset its output before converting the
        ' explicit-positive cell. False/throw must retain conversion/callback aliases.
        ' Existing callers keep the original resetting GuessPositiveClass contract.
        Public Shared Function GuessPositiveClassAfterConversion(explicitText As String, rowCount As Func(Of Integer),
                                                                  text As Func(Of Integer, String), binary As Func(Of Integer, Boolean),
                                                                  ByRef positiveLabel As String) As Boolean
            Dim prepared As String = Nothing
            Dim result As Boolean = GuessPositiveClass(explicitText, rowCount, text, binary, prepared)
            If result Then positiveLabel = prepared
            Return result
        End Function
'''
assert text.count('    End Class')==1;text=text.replace('    End Class',addition+'    End Class',1);write_source(path,text)
path='BESHstatNG/src/ExcelUDFs/UDFdataImport/UdfDataImportPlot.vb';text=decode(path);old='Global.BESHStatNG.ExcelCommon.UdfClassLabelPreparation38.GuessPositiveClass(';new=old.replace('GuessPositiveClass(','GuessPositiveClassAfterConversion(');assert text.count(old)==1;write_source(path,text.replace(old,new,1))
save('implementation-source-composition-v1.json',dict(phases=moves,ROC_new_method=addition,ROC_original_native_target=old,ROC_current_native_target=new,original_queries=0))
print('Typed PSM construction/routing/layout/input preparations and bounded ROC alias repair implemented')
