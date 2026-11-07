"""Small remaining SPC base preparation, distinct from36 stage/zone annotations."""
from work import *
FUNCTIONS='''Public Function SpcOptionalExtent(value As Nullable(Of Double), minimum As Double, maximum As Double) As ChartAxisScaleResult
If Not value.HasValue Then Return New ChartAxisScaleResult(minimum,maximum,0.0R)
If value.Value < minimum Then minimum = value.Value
If value.Value > maximum Then maximum = value.Value
Return New ChartAxisScaleResult(minimum,maximum,0.0R)
End Function
Public Function SpcConstantReference(count As Integer, value As Double) As Double()
Dim values(count - 1) As Double
For i As Integer = 0 To values.Length - 1
values(i) = value
Next
Return values
End Function
Public Function SpcPanelTop(top As Double, panelIndex As Integer, panelHeight As Double, panelSpacing As Double) As Double
Return top + panelIndex * (panelHeight + panelSpacing)
End Function
'''
def freeze():
    assert not (HERE/'pre-edit-spc-basis.csv').exists()
    text=(HERE/'entry-source/SpcControlChartExcel.vb.txt').read_text(encoding='utf-8')
    save('spc-basis-spec.json',dict(authority='PRE_EDIT_LEGACY',scientific='NOT_GOLDEN',originals={n:method(text,n) for n in ['IncludeNullableInBounds','AddConstantReferenceLine','AddCharts']},adaptation='Typed numerical extent return replaces private ByRef updates; original Object boxing of constant Double becomes a fresh Double vector and stays host-marshalled. Existing panel top expression is unchanged. No channel/stage/zone logic moved.',functions=FUNCTIONS))
    write('BESHStat.Core.Tests/src/Graphics/LegacySpcBasis34.vb',HEADER+'Namespace Global.Batch34CompletionFixtures\nPublic Module LegacySpcBasis34\n'+FUNCTIONS+'End Module\nEnd Namespace\n')
def apply():
    assert (HERE/'pre-edit-spc-basis.csv').exists()
    p='BESHStat.Core/src/Graphics/SpcAxisBoundsCore.vb';s=(ROOT/p).read_text(encoding='utf-8');s=s.replace('Public Module SpcAxisBoundsCore','Public Module SpcAxisBoundsCore\n'+FUNCTIONS);write(p,s)
    s=src('SpcControlChartExcel');h,b=header_body(method(s,'IncludeNullableInBounds'))
    s=replace(s,'IncludeNullableInBounds',h+'''\n            Dim prepared = SpcAxisBoundsCore.SpcOptionalExtent(value, minimum, maximum)
            minimum = prepared.Min
            maximum = prepared.Max
        End Sub''')
    old='''            Dim values(categories.Length - 1) As Object
            For i As Integer = 0 To values.Length - 1
                values(i) = value
            Next'''
    assert old in s;s=s.replace(old,'''            Dim values As Object() = SpcAxisBoundsCore.SpcConstantReference(categories.Length, value).Cast(Of Object)().ToArray()''')
    old='''options.Top +
                        panelIndex * (options.PanelHeight + options.PanelSpacing)'''
    assert old in s;s=s.replace(old,'SpcAxisBoundsCore.SpcPanelTop(options.Top, panelIndex, options.PanelHeight, options.PanelSpacing)')
    if 'Imports System.Linq' not in s:s=s.replace('Imports System\n','Imports System\nImports System.Linq\n')
    write('BESHstatNG/src/Graphics/SpcControlChartExcel.vb',s)
if __name__=='__main__':globals()[sys.argv[1]]()
