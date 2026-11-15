from common41 import *
import re
lock();assert load(HERE/'fixtures/pre-edit-current-main-v1.json')['cases']==390
entry=load(HERE/'entry/source-preimages-v1.json')['source_pins']
paths=['BESHstatNG/src/ExcelUDFs/'+n+'.vb' for n in ['ParametricUDFs','NonparametricUDFs','AssumptionsUDFs']]+['BESHStat.Excel.Common/src/Results/'+n+'.vb' for n in ['UdfInferencePreparation38','UdfTypedReportPreparation38','UdfSpillPreparation38']]
for p in paths:assert pin(p)['sha256']==entry[p]
texts={p:(ROOT/p).read_text(encoding='utf-8-sig') for p in paths}
def replace(p,a,b):
    assert texts[p].count(a)==1,'EXACT_REPLACEMENT_NOT_UNIQUE:'+p
    texts[p]=texts[p].replace(a,b)
inf=paths[3]
methods='''
        '''+"''' <summary>Retains compute-before-option-read ordering used by the Windows UDF.</summary>"+'''
        Public Shared Function BuildRepeatedAnovaTables(mat(,) As Double, names() As String, readCorrection As Func(Of String)) As List(Of ResultTable)
            Dim mdl As New parametric.OneWayRmANOVA(mat, names)
            mdl.compute()
            If Not ApplyRepeatedCorrection(mdl, readCorrection()) Then Return Nothing
            Return mdl.wrapResults()
        End Function

        Public Shared Function BuildNestedAnovaTables(mat(,) As Object, names() As String) As List(Of ResultTable)
            Dim mdl As New parametric.TwoWayNestedANOVA(mat, names)
            mdl.compute()
            Return mdl.wrapResults()
        End Function

        Public Shared Function BuildIndependentTTables(data()() As Double, names() As String, alpha As Double) As List(Of ResultTable)
            Dim mdl As New parametric.UnpairedTtest(data, names)
            mdl.compute(alpha)
            Return mdl.wrapResults()
        End Function

        Public Shared Function BuildPairedTTables(mat(,) As Double, names() As String) As List(Of ResultTable)
            Dim mdl As New parametric.PairedTtest(mat, names)
            mdl.compute()
            Return mdl.wrapResults()
        End Function

        Public Shared Function ComputeMannWhitney(first() As Double, second() As Double) As TestResult
            Dim data As Double()() = {first, second}
            Dim mdl As New MannWhitney(data, "group1", "group2")
            Return mdl.Compute()
        End Function

        Public Shared Function ComputeWilcoxon(pairs(,) As Double) As TestResult
            Dim mdl As New WilcoxonTest(pairs, "x", "y")
            Return mdl.Compute()
        End Function
'''
replace(inf,'    End Class\nEnd Namespace',methods+'    End Class\nEnd Namespace')
param=paths[0]
replace(param,'''                Dim mdl As New parametric.OneWayRmANOVA(mat, names)
                mdl.compute()

                Dim which As String = NormalizeText(correction)
                If Not Global.BESHStatNG.ExcelCommon.UdfInferencePreparation38.ApplyRepeatedCorrection(mdl, which) Then Return ExcelError.ExcelErrorValue

                Dim tables = mdl.wrapResults()''','''                Dim tables = Global.BESHStatNG.ExcelCommon.UdfInferencePreparation38.BuildRepeatedAnovaTables(
                    mat, names, Function() NormalizeText(correction))
                If tables Is Nothing Then Return ExcelError.ExcelErrorValue''')
for engine,method,args in [('TwoWayNestedANOVA','BuildNestedAnovaTables','mat, names'),('UnpairedTtest','BuildIndependentTTables','data, names, alphaValue'),('PairedTtest','BuildPairedTTables','mat, names')]:
    ctor='data, names' if engine=='UnpairedTtest' else 'mat, names'
    before='                Dim mdl As New parametric.'+engine+'('+ctor+')\n                mdl.compute('+('alphaValue' if engine=='UnpairedTtest' else '')+')\n'
    if engine=='TwoWayNestedANOVA':before+='\n'
    before+='                Dim tables = mdl.wrapResults()'
    replace(param,before,'                Dim tables = Global.BESHStatNG.ExcelCommon.UdfInferencePreparation38.'+method+'('+args+')')
nonparam=paths[1]
old='''            Dim data As Double()() = {x1, x2}
            Dim mw As New MannWhitney(data, "group1", "group2")
            Dim res As TestResult = mw.Compute()'''
assert texts[nonparam].count(old)==2
texts[nonparam]=texts[nonparam].replace(old,'            Dim res As TestResult = Global.BESHStatNG.ExcelCommon.UdfInferencePreparation38.ComputeMannWhitney(x1, x2)')
old='''            Dim w As New WilcoxonTest(pairs, "x", "y")
            Dim res As TestResult = w.Compute()'''
assert texts[nonparam].count(old)==2
texts[nonparam]=texts[nonparam].replace(old,'            Dim res As TestResult = Global.BESHStatNG.ExcelCommon.UdfInferencePreparation38.ComputeWilcoxon(pairs)')
spill=paths[5]
overload='''        '''+"''' <summary>Supplies computed scalar values to the existing two-value layout without duplicating its grammar.</summary>"+'''
        Public Shared Function TwoValueTable(title As String, firstLabel As String, firstValue As Double,
                                             secondLabel As String, secondValue As Double) As UdfReportProjection38
            Dim projection = TwoValueTable(title, firstLabel, secondLabel)
            Dim model = projection.ToOutputModel()
            Dim values() As Double = {firstValue, secondValue}
            For row = model.Values.GetLowerBound(0) To model.Values.GetUpperBound(0)
                For column = model.Values.GetLowerBound(1) To model.Values.GetUpperBound(1)
                    Dim reference = TryCast(model.Values(row, column), UdfSourceCellReference38)
                    If reference IsNot Nothing Then model.Values(row, column) = values(reference.SourceOrdinal)
                Next
            Next
            Return New UdfReportProjection38(model, projection.Preparation)
        End Function

'''
replace(spill,'        '''+"''' <summary>Retains column-first then explicit-cell order, including repeated cells.</summary>",overload+'        '+"''' <summary>Retains column-first then explicit-cell order, including repeated cells.</summary>")
assump=paths[2];reports=paths[4];additions=[];moved=[]
# Exact engine/result fragments, including original labels, are taken from the frozen Windows source.
specs=[('SHAPIRO_WILK','PrepareShapiroWilk','x() As Double','x','                Dim errText As String'),('DAGOSTINO_PEARSON','PrepareDAgostino','x() As Double','x','                Dim errText As String'),('ANDERSON_DARLING','PrepareAndersonDarling','x() As Double','x','                Dim res As TestResult'),('FLIGNER_KILLEEN','PrepareFlignerKilleen','data()() As Double','data','                Dim res As TestResult'),('LEVENE','PrepareLevene','data()() As Double, useMedian As Boolean, label As String','data, useMedian, label','                Dim res As TestResult'),('BARTLETT','PrepareBartlett','data()() As Double','data','                For Each g In data'),('SQUARED_RANKS','PrepareSquaredRanks','data()() As Double','data','                Dim res As TestResult'),('MAUCHLY','PrepareMauchly','mat(,) As Double','mat','                Dim res As TestResult'),('SYMMETRY','PrepareSymmetry','x() As Double, methodInternal As String, title As String','x, methodInternal, title','                Dim res As TestResult'),('GRUBBS','PrepareGrubbs','x() As Double, alphaValue As Double','x, alphaValue','                Dim res As TestResult')]
for source_name,new_name,signature,args,start_text in specs:
    s=texts[assump];a=re.search(r'(?m)^        Public Function '+source_name+r'\(',s).start();end=s.index('            Catch ex As Exception',a)
    begin=s.index(start_text,a,end);fragment=s[begin:end].rstrip()
    moved.append(dict(function=source_name,source_fragment_sha256=sha(fragment.encode()),destination=new_name))
    body=fragment.replace('Return ExcelError.ExcelErrorNum','Return UdfReportProjection38.FromFault(UdfReportFault38.InvalidNumber)').replace('Return BuildTwoValueTable(','Return UdfSpillPreparation38.TwoValueTable(')
    body='\n'.join(line[4:] if line.startswith('    ') else line for line in body.splitlines())
    additions.append('        Public Shared Function '+new_name+'('+signature+') As UdfReportProjection38\n'+body+'\n        End Function\n')
    texts[assump]=s[:begin]+'                Return MaterializePortableReport38(Global.BESHStatNG.ExcelCommon.AssumptionsUDFsReports38.'+new_name+'('+args+'))\n'+s[end:]
replace(reports,'    Public NotInheritable Class AssumptionsUDFsReports38\n        Private Sub New()\n        End Sub',
        '    Public NotInheritable Class AssumptionsUDFsReports38\n        Private Sub New()\n        End Sub\n\n'+ '\n'.join(additions))
# These moved fragments call the same finite predicate previously supplied by the Windows numeric facade.
if 'Imports BESHStatNG.WorksheetCompatibility.UdfNumericPreparation38' not in texts[reports]:
    texts[reports]=texts[reports].replace('Imports System\n','Imports System\nImports BESHStatNG.WorksheetCompatibility.UdfNumericPreparation38\n',1)
for p in paths:
    with (ROOT/p).open('w',encoding='utf-8',newline='\r\n') as f:f.write(texts[p])
save('waves/wave1-source-v1.json',dict(files=[dict(before=dict(path=p,sha256=entry[p]),after=pin(p)) for p in paths],assumptions_engine_fragments=moved,numerical_algorithms_changed=False,Windows_acquisition_option_read_order_and_domain_guards_retained=True,new_public_Common_methods=16,new_classes=0,gates=GATES))
print('Wave1: six existing files; shared engines/results, unchanged Windows ordering/guards',flush=True)
