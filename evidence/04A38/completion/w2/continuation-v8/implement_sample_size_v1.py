"""Extract complete sample-size planning orchestration, never its numerical engines."""
from common import *
import re,textwrap
path='BESHstatNG/src/ExcelUDFs/SampleSizeUDFs.vb'
before=(ROOT/path).read_text(encoding='utf-8-sig')
assert pin(ROOT/path)==json.loads((HERE/'implementation-plan-v1.json').read_bytes())['source_before'][0]['live']
specs=[
('SSIZE_TTEST_UNPAIRED','Unpaired',['meanDifference','commonSd','controlToExperimentalRatio','alpha','beta','hypothesisType','margin']),
('SSIZE_PROP_SINGLE','SingleProportion',['anticipatedProportion','nullProportion','alpha','beta']),
('SSIZE_PROP_INDEP','IndependentProportions',['controlProportion','experimentalProportion','controlToExperimentalRatio','alpha','beta','hypothesisType','margin']),
('SSIZE_LOGRANK','LogRank',['hazardRatio','controlEventProportion','experimentalEventProportion','controlToExperimentalRatio','alpha','beta','twoSided']),
('SSIZE_COX_BINARY','CoxBinary',['hazardRatio','controlToExperimentalRatio','alpha','beta','rSquaredWithOtherCovariates','overallEventProportion','twoSided']),
('SSIZE_COX_CONTINUOUS','CoxContinuous',['hazardRatioPerUnit','covariateSd','alpha','beta','rSquaredWithOtherCovariates','overallEventProportion','twoSided']),
('SSIZE_ICC','Icc',['nullIcc','alternativeIcc','observationsPerSubject','alpha','beta']),
('SSIZE_BLANDALTMAN','BlandAltman',['sdDifference','desiredHalfWidth','alpha','loaMultiplier'])]
pieces=[];rows=[];after=before
for native,portable,args in specs:
    start=before.index('        Public Function '+native+'(')
    stop=before.index('        End Function',start)+len('        End Function')
    full=before[start:stop];begin=full.index('            Try\n')+len('            Try\n');end=full.index('            Catch ex As Exception\n')
    body=full[begin:end];prepared=body
    readers=[('TryGetDouble','readNumber','Func(Of Object, Double?)')]
    if 'ResolveSampleSizeHypothesisType(' in body:readers.append(('ResolveSampleSizeHypothesisType','readHypothesis','Func(Of Object, UdfSampleSizeHypothesis38)'))
    if 'IsMissingArg(' in body:readers.append(('IsMissingArg','isMissing','Func(Of Object, Boolean)'))
    if 'GetOptionalBool(' in body:readers.append(('GetOptionalBool','readOptionalBool','Func(Of Object, Boolean, Boolean)'))
    if 'IsOpenUnitInterval(' in body:readers.append(('IsOpenUnitInterval','isOpenUnitInterval','Func(Of Double, Boolean)'))
    if 'IsClosedUnitInterval(' in body:readers.append(('IsClosedUnitInterval','isClosedUnitInterval','Func(Of Double, Boolean)'))
    for old,new,typ in readers:prepared=re.sub(r'\b'+old+r'\(',new+'(',prepared)
    prepared=prepared.replace('Dim hypothesis As String','Dim hypothesis As UdfSampleSizeHypothesis38')
    prepared=prepared.replace('If hypothesis Is Nothing Then','If hypothesis = UdfSampleSizeHypothesis38.Invalid Then')
    for old,new in [('HypothesisSuperiority','Superiority'),('HypothesisNonInferiority','NonInferiority'),('HypothesisEquivalence','Equivalence')]:prepared=prepared.replace(old,'UdfSampleSizeHypothesis38.'+new)
    prepared=prepared.replace('Return ExcelError.ExcelErrorValue','Return Failed(errorKind, UdfSampleSizeError38.Value)').replace('Return ExcelError.ExcelErrorNum','Return Failed(errorKind, UdfSampleSizeError38.Numeric)')
    prepared=re.sub(r'\bReturn (Make\w+Table)\(',r'Return SampleSizeUDFsReports38.\1(',prepared)
    parameters=[a+' As Object' for a in args]+[name+' As '+typ for old,name,typ in readers]+['ByRef errorKind As UdfSampleSizeError38']
    pieces.append('        Public Shared Function Prepare'+portable+'('+', _\n            '.join(parameters)+') As UdfReportProjection38\n'+textwrap.indent(textwrap.dedent(prepared),'            ')+'        End Function\n')
    targets=['AddressOf '+('ResolveSampleSizeHypothesisKind38' if old=='ResolveSampleSizeHypothesisType' else old) for old,_,_ in readers]
    replacement='                Dim errorKind As Global.BESHStatNG.ExcelCommon.UdfSampleSizeError38\n                Dim report As Global.BESHStatNG.ExcelCommon.UdfReportProjection38 =\n                    Global.BESHStatNG.ExcelCommon.UdfSampleSizePreparation38.Prepare'+portable+'(\n                        '+', '.join(args+targets+['errorKind'])+')\n                Return MaterializeSampleSizeResult38(report, errorKind)\n'
    changed=full[:begin]+replacement+full[end:]
    assert after.count(full)==1;after=after.replace(full,changed)
    rows.append(dict(native=native,portable='Prepare'+portable,parameters=args,readers=[dict(original=o,prepared=n,type=t) for o,n,t in readers],full_native_before=full,original_try_body=body,prepared_try_body=prepared,current_native=changed))
helpers='''
        Private Function ResolveSampleSizeHypothesisKind38(value As Object) As Global.BESHStatNG.ExcelCommon.UdfSampleSizeHypothesis38
            Dim hypothesis As String = ResolveSampleSizeHypothesisType(value)
            If hypothesis Is Nothing Then Return Global.BESHStatNG.ExcelCommon.UdfSampleSizeHypothesis38.Invalid
            Select Case hypothesis
                Case HypothesisSuperiority
                    Return Global.BESHStatNG.ExcelCommon.UdfSampleSizeHypothesis38.Superiority
                Case HypothesisNonInferiority
                    Return Global.BESHStatNG.ExcelCommon.UdfSampleSizeHypothesis38.NonInferiority
                Case HypothesisEquivalence
                    Return Global.BESHStatNG.ExcelCommon.UdfSampleSizeHypothesis38.Equivalence
                Case Else
                    Return Global.BESHStatNG.ExcelCommon.UdfSampleSizeHypothesis38.Invalid
            End Select
        End Function

        Private Function MaterializeSampleSizeResult38(report As Global.BESHStatNG.ExcelCommon.UdfReportProjection38,
                                                       errorKind As Global.BESHStatNG.ExcelCommon.UdfSampleSizeError38) As Object
            If report Is Nothing Then
                If errorKind = Global.BESHStatNG.ExcelCommon.UdfSampleSizeError38.Numeric Then Return ExcelError.ExcelErrorNum
                Return ExcelError.ExcelErrorValue
            End If
            Return DirectCast(MaterializePortableReport38(report), Object(,))
        End Function
'''
after=after.replace('    End Module',helpers+'\n    End Module')
new='BESHStat.Excel.Common/src/Analysis/UdfSampleSizePreparation38.vb'
header='''Option Explicit On
Option Strict On

Imports System
Imports BESHStatNG.SampleSizeCalc

Namespace ExcelCommon
    Public Enum UdfSampleSizeError38
        Value
        Numeric
    End Enum

    Public Enum UdfSampleSizeHypothesis38
        Invalid
        Superiority
        NonInferiority
        Equivalence
    End Enum

    '''+"''' <summary>Ordered UDF planning orchestration over opaque host arguments. Readers retain host conversion policy.</summary>\n"+'''    Public NotInheritable Class UdfSampleSizePreparation38
        Private Sub New()
        End Sub

        Private Shared Function Failed(ByRef errorKind As UdfSampleSizeError38, value As UdfSampleSizeError38) As UdfReportProjection38
            errorKind = value
            Return Nothing
        End Function

'''
with (ROOT/new).open('x',encoding='utf-8',newline='\r\n') as f:f.write(header+'\n'.join(pieces)+'    End Class\nEnd Namespace\n')
write_source(path,after)
save('sample-size-extraction-v1.json',dict(rows=rows,host_helpers=helpers,source_before=json.loads((HERE/'implementation-plan-v1.json').read_bytes())['source_before'][0],source_after=pin(ROOT/path),portable=pin(ROOT/new),semantics='Original Try/Catch/log labels retained; actual host readers/guards invoked in original order, optional readers stay lazy, original engines and existing Raw report builders reused. Error output is ignored on success; exceptions propagate to original native catch. Enum bridge retains the original native Text Select over the already accepted option parser.',original_queries=0))
print('Extracted8 complete planning orchestrations; original engines untouched')
