"""Extract whole report phases; retain host acquisition/catches and Core formulas."""
from common_v2 import *
from source_views import body
import re
u=json.loads((PREVIOUS/'current-universe-packet-v1.json').read_bytes())
def member(name,owner=None):return next(n for n in u['members'] if n['name']==name and n['kind']=='FunctionBlock' and (owner is None or n['owner']==owner))
def text(n):return body((ROOT/n['path']).read_bytes(),n)[0].replace('\r\n','\n')
def new_source(path,source):
    with (ROOT/path).open('x',encoding='utf-8',newline='\r\n') as f:f.write(source)
def class_source(name,imports,methods):return 'Option Explicit On\nOption Strict On\n\n'+imports+'\nNamespace ExcelCommon\n    Public NotInheritable Class '+name+'\n        Private Sub New()\n        End Sub\n\n'+'\n\n'.join(methods)+'\n    End Class\nEnd Namespace\n'
agreement_path='BESHstatNG/src/ExcelUDFs/AgreementMethodUDFs.vb';agreement=(ROOT/agreement_path).read_text(encoding='utf-8-sig');reports=[];bindings=[]
specs=[('PASSINGBABLOK_COEF','BuildRegressionCoefficients','fit.SlopeCI, fit.InterceptCI, "Passing–Bablok Coefficients"'),('DEMING_COEF','BuildRegressionCoefficients','mdl.SlopeCI, mdl.InterceptCI, "Deming Regression Coefficients"'),('BLANDALTMAN_STATS','BuildBlandAltmanStatistics','res'),('ICC_RC','BuildRepeatability','rc, title')]
for name,api,args in specs:
    n=member(name);old=text(n);start=old.index('                Dim t As New ResultTable');end=old.index('                Return PrepareResultTableForUdf(t)',start)+len('                Return PrepareResultTableForUdf(t)');phase=old[start:end]
    if name!='DEMING_COEF':
        signature={'PASSINGBABLOK_COEF':'slope As ConfidenceIntervalResult, intercept As ConfidenceIntervalResult, title As String','BLANDALTMAN_STATS':'res As Agreement.BlandAltmanResult','ICC_RC':'rc As ConfidenceIntervalResult, title As String'}[name]
        prepared=phase.replace('Return PrepareResultTableForUdf(t)','Return t')
        if name=='PASSINGBABLOK_COEF':prepared=prepared.replace('fit.SlopeCI','slope').replace('fit.InterceptCI','intercept').replace('t.AddTitle("Passing–Bablok Coefficients")','t.AddTitle(title)')
        reports.append('        Public Shared Function '+api+'('+signature+') As ResultTable\n'+prepared+'\n        End Function')
    replacement='                Return PrepareResultTableForUdf(Global.BESHStatNG.ExcelCommon.UdfAgreementReportPreparation38.'+api+'('+args+'))'
    changed=old[:start]+replacement+old[end:];assert old in agreement;agreement=agreement.replace(old,changed,1)
    bindings.append(dict(parent=n['symbol_id'],api=api,original_phase=phase,new_call=replacement,actual_arguments=args))
new_source('BESHStat.Excel.Common/src/Results/UdfAgreementReportPreparation38.vb',class_source('UdfAgreementReportPreparation38','Imports BESHStatNG\n',reports));write_source(agreement_path,agreement)

path='BESHstatNG/src/RegModels/BinaryClassificationReporting.vb';native=(ROOT/path).read_text(encoding='utf-8-sig');source=native;methods=[];class_bindings=[]
names=['BuildBinaryCrosstabUdfOutput','BuildThresholdTableUdfOutput','BuildCalibrationTableUdfOutput','BuildBrierScoreUdfOutput']
for name in names:
    match=re.search(r'(?m)^        Public Function '+name+r'\(',source);assert match
    end=source.index('        End Function',match.start())+len('        End Function');old=source[match.start():end]
    header_end=old.index(' As Object\n')+len(' As Object');header=old[:header_end]
    method_body=old[header_end+1:-len('        End Function')].rstrip()
    # The sentinel remains the original host-owned readonly singleton, passed opaquely.
    method_body=method_body.replace('MissingOutputValue','missingValue').replace('PrepareResultTableForUdfLocal(out)','UdfSpillPreparation38.NormalizeBlanks(out)')
    method_body=re.sub(r'PercentOrMissingValue\((summary\.\w+|r\.\w+)\)',r'PercentOrMissingValue(\1, missingValue)',method_body)
    common_header=header.replace('Public Function','Public Shared Function').replace(',\n',',\n',1)
    # Add required sentinel before all existing optional arguments.
    if name=='BuildBrierScoreUdfOutput':common_header=common_header.replace('score As Double,','score As Double, missingValue As Object,',1)
    elif name=='BuildBinaryCrosstabUdfOutput':common_header=common_header.replace('summary As BinaryClassificationSummary,','summary As BinaryClassificationSummary, missingValue As Object,',1)
    elif name=='BuildThresholdTableUdfOutput':common_header=common_header.replace('rows As IList(Of BinaryThresholdRow),','rows As IList(Of BinaryThresholdRow), missingValue As Object,',1)
    else:common_header=common_header.replace('rows As IList(Of CalibrationBinSummary),','rows As IList(Of CalibrationBinSummary), missingValue As Object,',1)
    args={'BuildBinaryCrosstabUdfOutput':'summary, MissingOutputValue, includeHeader','BuildThresholdTableUdfOutput':'rows, MissingOutputValue, includeHeader','BuildCalibrationTableUdfOutput':'rows, MissingOutputValue, includeHeader','BuildBrierScoreUdfOutput':'score, MissingOutputValue, n, eventRate, includeHeader'}[name]
    call='            Return Global.BESHStatNG.ExcelCommon.UdfClassificationReportPreparation38.'+name+'('+args+')'
    replacement=header+'\n'+call+'\n        End Function';native=native.replace(old,replacement,1);methods.append(common_header+'\n'+method_body+'\n        End Function');class_bindings.append(dict(name=name,original_whole_body=old,original_phase=old[header_end+1:-len('        End Function')].rstrip(),new_call=call))
methods.append('''        Private Shared Function PercentOrMissingValue(value As Double, missingValue As Object) As Object
            If Double.IsNaN(value) OrElse Double.IsInfinity(value) Then Return missingValue
            Return 100.0R * value
        End Function''')
new_source('BESHStat.Excel.Common/src/Results/UdfClassificationReportPreparation38.vb',class_source('UdfClassificationReportPreparation38','Imports System\nImports System.Collections.Generic\nImports BESHStatNG.regression\n',methods));write_source(path,native)

path='BESHstatNG/src/ExcelUDFs/ContingencyTableUDFs.vb';native=(ROOT/path).read_text(encoding='utf-8-sig');methods=[];ct_bindings=[]
for name,api in [('CT_CHI2','ChiSquaredBody'),('CT_NOMINAL_ASSOC','NominalAssociationBody'),('CT_FISHER_2X2','Fisher2x2Body'),('CT_FFH_EXACT','FreemanHaltonBody')]:
    n=member(name);old=text(n);start=old.index('                Dim exact As New FisherExactEngine' if name=='CT_FFH_EXACT' else '                Dim res')
    end=old.index('                Return BuildResultTable(',start);phase=old[start:end].rstrip();finite=name!='CT_NOMINAL_ASSOC'
    prepared=phase.replace('Return ExcelError.ExcelErrorNum','Return Nothing').replace('Dim body As Object(,)','Dim reportBody As Object(,)')
    signature='tab(,) As Integer'+(', isFinite As Func(Of Double, Boolean)' if finite else '')
    methods.append('        Public Shared Function '+api+'('+signature+') As Object(,)\n'+prepared+'\n                Return reportBody\n        End Function')
    call='                Dim body As Object(,) = Global.BESHStatNG.ExcelCommon.UdfContingencyPreparation38.'+api+'(tab'+(', AddressOf IsFinite' if finite else '')+')\n                If body Is Nothing Then Return ExcelError.ExcelErrorNum\n'
    changed=old[:start]+call+old[end:];native=native.replace(old,changed,1);ct_bindings.append(dict(parent=n['symbol_id'],api=api,original_phase=phase,new_call=call))
new_source('BESHStat.Excel.Common/src/Analysis/UdfContingencyPreparation38.vb',class_source('UdfContingencyPreparation38','Imports System\nImports BESHStatNG\nImports BESHStatNG.contingencytable\nImports BESHStatNG.WorksheetCompatibility.UdfNumericPreparation38\n',methods));write_source(path,native)
save('source-extraction-v1.json',dict(agreement=bindings,classification=class_bindings,contingency=ct_bindings,original_queries=0,Core_changes=0,host_policy='Original acquisition, options/conversion, global sentinel, catches/log labels and final Excel materialization retained. Report CI cache writes occur in their original row order. New named finite callback binds the actual original Core guard; no anonymous callback introduced.',normalizer='Existing accepted UdfSpillPreparation38.NormalizeBlanks reuses the original zero-based row-major blank-copy phase. Bounded to generated Object(,) tables; no eager text conversion.'))
print('C9 coherent Agreement/Classification/Contingency report phases implemented')
