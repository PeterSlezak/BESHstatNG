"""Extract pure source-pinned planning coordination/text; no formula reimplementation."""
import importlib.util
from pathlib import Path
import re
s=importlib.util.spec_from_file_location('c26_planning',Path(__file__).with_name('capture-04a26-contract.py'));c=importlib.util.module_from_spec(s);s.loader.exec_module(c)
# Method, semantic family, already-parsed variables in original evaluation order, result type.
SPECS=[
 ('CalculateLogRankSampleSize','LogRank',['hazardRatio','controlEventProportion','experimentalEventProportion','kappa'],'LogRankSampleSizeResult'),
 ('CalculateCoxEventCountContinuousCovariate','Cox',['hazardRatioPerUnit','covariateSd','rSquaredWithOtherCovariates','overallEventProportion'],'CoxEventCountPlanningResult'),
 ('CalculateCoxEventCountBinaryCovariate','Cox',['hazardRatio','kappa','rSquaredWithOtherCovariates','overallEventProportion'],'CoxEventCountPlanningResult'),
 ('CalculateIccHypothesisTestSampleSize','Icc',['nullIcc','alternativeIcc','observationsPerSubjectValue'],'IccHypothesisTestSampleSizeResult'),
 ('CalculateBlandAltmanLoASampleSize','BlandAltman',['sdDifference','desiredHalfWidth','loaMultiplier'],'BlandAltmanAgreementStudyPlanningResult'),
 ('CalculateNonInferiorityIndependentProportions','IndependentProportions',['controlProp','experimentalProp','marginAbsolute','kappa'],'IndependentProportionsSampleSizeResult'),
 ('CalculateEquivalenceIndependentProportions','IndependentProportions',['controlProp','experimentalProp','marginAbsolute','kappa'],'EquivalenceIndependentProportionsSampleSizeResult'),
 ('CalculateIndependentProportions','IndependentProportions',['controlProp','experimentalProp','kappaSuperiority'],'IndependentProportionsSampleSizeResult'),
 ('CalculateSingleProportion','SingleProportion',['prop','H0Prop'],'SingleProportionSampleSizeResult'),
 ('CalculatePairedTTest','PairedT',['diff','sd'],'PairedTTestSampleSizeResult'),
 ('CalculateNonInferiorityUnpairedTTest','UnpairedT',['expectedDifference','marginAbsolute','sd','kappa'],'UnpairedTTestSampleSizeResult'),
 ('CalculateEquivalenceUnpairedTTest','UnpairedT',['expectedDifference','marginAbsolute','sd','kappa'],'EquivalenceUnpairedTTestSampleSizeResult'),
 ('CalculateUnpairedTTest','UnpairedT',['diff','sdSuperiority','kappaSuperiority'],'UnpairedTTestSampleSizeResult')]
def blocks(source):
    found={}
    for m in re.finditer(r'Dim (\w+) = SampleSizeCalculator\.(\w+)\(',source):
        cursor=m.end();depth=1
        while depth:
            ch=source[cursor];depth+= (ch=='(')-(ch==')');cursor+=1
        append=re.search(r'Me\.tbOutput\.AppendText\((\w+)\)',source[cursor:]);assert append
        end=cursor+append.end();body=source[m.start():cursor];tail=source[cursor:cursor+append.start()]
        found[m[2]]=dict(start=m.start(),end=end,call=body,tail=tail,result=m[1],out=append[1])
    assert len(found)==13
    return found
def generate():
    source=(c.RUN/'preimages/Ui12SampleSizeTtestSingleProp.vb').read_text(encoding='utf-8-sig');found=blocks(source)
    imports='Option Explicit On\nOption Strict On\nOption Infer On\nImports System\nImports BESHStatNG.SampleSizeCalc\n'
    cases=[];text_methods=[];legacy=[];legacy_calls=[];metadata=[];integration=[]
    for index,(name,family,variables,result_type) in enumerate(SPECS):
        b=found[name];short=name.removeprefix('Calculate')
        bindings='\n'.join(f'            Dim {v} As Double = request.Value({i})' for i,v in enumerate(variables))+'\n            Dim Alpha = request.Alpha, Beta = request.Beta\n'
        call=b['call'].replace('CInt(observationsPerSubjectValue)','request.ObservationsPerSubject')
        tail=b['tail'].replace('CInt(observationsPerSubjectValue)','request.ObservationsPerSubject')
        text_methods.append(f'        Public Shared Function {short}(request As SampleSizePlanningRequest, {b["result"]} As {result_type}) As String\n'+bindings+tail.replace('vbNewLine','PlanningTextNewLine.Value')+f'            Return {b["out"]}\n        End Function\n')
        legacy.append(f'        Public Shared Function {short}(request As SampleSizePlanningRequest, {b["result"]} As {result_type}) As String\n'+bindings+tail+f'            Return {b["out"]}\n        End Function\n')
        legacy_calls.append(f'                Case SampleSizePlanningMethod.{short}\n'+bindings+call+f'\n                    Return {b["result"]}')
        cases.append(f'                Case SampleSizePlanningMethod.{short}\n'+bindings+call+f'\n                    Return New SampleSizePlanningReport(request, {b["result"]}, SampleSizeTextProjection.{short}(request, {b["result"]}))')
        metadata.append(f'                Case SampleSizePlanningMethod.{short} : Return New SampleSizeMethodDescription(SampleSizeFamily.{family}, "{short}", New String() {{'+', '.join('"'+v+'"' for v in variables)+'})')
        beta='Double.NaN' if family=='BlandAltman' else 'Beta'
        extra=', observationsPerSubject:=CInt(observationsPerSubjectValue)' if family=='Icc' else ''
        if family=='Cox':extra+=', overallEventProvided:=Me.tbKappa.Text.Trim() <> String.Empty, rSquaredProvided:=Me.tbCustom4.Text.Trim() <> String.Empty'
        replacement=f'Dim request As New SampleSizePlanningRequest(SampleSizePlanningMethod.{short}, Alpha, {beta}, New Double() {{'+', '.join(variables)+'}'+extra+')\n        Me.tbOutput.AppendText(SampleSizePlanningPolicy.Calculate(request).Text)'
        integration.append(dict(start=b['start'],end=b['end'],replacement=replacement,method=name))
    common=imports+'\nNamespace ExcelCommon\n    Public NotInheritable Class SampleSizePlanningPolicy\n        Private Sub New()\n        End Sub\n        Public Shared Function Calculate(request As SampleSizePlanningRequest) As SampleSizePlanningReport\n            Select Case request.Method\n'+'\n'.join(cases)+'\n                Case Else : Throw New ArgumentOutOfRangeException(NameOf(request))\n            End Select\n        End Function\n    End Class\nEnd Namespace\n'
    (c.ROOT/'BESHStat.Excel.Common/src/Analysis/SampleSizePlanningPolicy.vb').write_text(common,encoding='utf-8',newline='\n')
    text=imports+'\nNamespace ExcelCommon\n    Public NotInheritable Class SampleSizeTextProjection\n        Private Sub New()\n        End Sub\n'+ '\n'.join(text_methods)+'    End Class\nEnd Namespace\n'
    (c.ROOT/'BESHStat.Excel.Common/src/Analysis/SampleSizeTextProjection.vb').write_text(text,encoding='utf-8',newline='\n')
    test=imports+'Imports BESHStatNG.ExcelCommon\nPublic NotInheritable Class Planning26LegacyText\n'+ '\n'.join(legacy)+'End Class\n'
    (c.ROOT/'BESHStat.Excel.Common.Tests/src/Planning26LegacyText.vb').write_text(test,encoding='utf-8',newline='\n')
    (c.ROOT/'BESHStat.Excel.Common.Tests/src/Planning26LegacyCore.vb').write_text(imports+'Imports BESHStatNG.ExcelCommon\nPublic NotInheritable Class Planning26LegacyCore\n    Public Shared Function Calculate(request As SampleSizePlanningRequest) As Object\n        Select Case request.Method\n'+'\n'.join(legacy_calls)+'\n            Case Else : Throw New ArgumentOutOfRangeException(NameOf(request))\n        End Select\n    End Function\nEnd Class\n',encoding='utf-8',newline='\n')
    (c.RUN/'planning-integration.json').write_text(__import__('json').dumps(integration),encoding='utf-8')
    (c.RUN/'planning-metadata.txt').write_text('\n'.join(metadata),encoding='utf-8')
    descriptions=imports+'\nNamespace ExcelCommon\n    Public NotInheritable Class SampleSizePlanningDescriptions\n        Private Sub New()\n        End Sub\n        Public Shared Function Describe(method As SampleSizePlanningMethod) As SampleSizeMethodDescription\n            Select Case method\n'+'\n'.join(metadata)+'\n                Case Else : Throw New ArgumentOutOfRangeException(NameOf(method))\n            End Select\n        End Function\n    End Class\nEnd Namespace\n'
    (c.ROOT/'BESHStat.Excel.Common/src/Analysis/SampleSizePlanningDescriptions.vb').write_text(descriptions,encoding='utf-8',newline='\n')
    c.emit('sample-size-extraction-provenance.json',dict(source=c.FORM,source_sha256=c.sha(c.RUN/'preimages/Ui12SampleSizeTtestSingleProp.vb'),methods=[dict(core_method=n,family=f,parsed_variables=v,result_type=t) for n,f,v,t in SPECS],transformations=['Existing Core calls moved to Common coordinator, arguments/branch selection unchanged.','Original interpolated text moved verbatim; vbNewLine becomes explicit CRLF through PlanningTextNewLine.Value.','ICC CInt stays Windows-owned; typed count provided to request.','Source-pinned test text derivative retains original vbNewLine expressions.'],status='NOT_GOLDEN / SAME_CORE_COMPATIBILITY_ONLY'))
if __name__=='__main__':generate()
