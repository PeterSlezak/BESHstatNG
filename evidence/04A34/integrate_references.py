from entry import ROOT,HERE
from integrate import header,write,replace_member
from freeze_slices import member,body,source
import re
if __name__=='__main__':
    assert (HERE/'pre-edit-reference-capture.csv').is_file()
    s=source('AgreementPlotExcel')
    s=s.replace('''{plotData.Intercept + plotData.Slope * plotData.MinX,
                           plotData.Intercept + plotData.Slope * plotData.MaxX}''','ChartReferenceSeries.Affine(plotData.MinX, plotData.MaxX, plotData.Intercept, plotData.Slope).YValues')
    s=s.replace('{plotData.MinX, plotData.MaxX}','ChartReferenceSeries.Identity(plotData.MinX, plotData.MaxX).XValues')
    s=s.replace('New Double() {plotData.MinValue, plotData.MaxValue}','ChartReferenceSeries.Identity(plotData.MinValue, plotData.MaxValue).XValues')
    s=s.replace('Dim lineY As Double() = {yValue, yValue}','Dim lineY As Double() = ChartReferenceSeries.Horizontal(lineX(0), lineX(1), yValue).YValues')
    write('BESHstatNG/src/Graphics/AgreementPlotExcel.vb',s)
    write('BESHStat.Core/src/Graphics/ComparisonCoordinatesCore.vb',header()+'''
'''+'\n'.join([
        'Public Module ComparisonCoordinatesCore',
        '    Public Function OrdinalY(count As Integer) As Double()',
        '        Dim y(count - 1) As Double',
        '        For i As Integer = 0 To count - 1', '            y(i) = count - i', '        Next',
        '        Return y', '    End Function',
        '    Public Function IncludeZeroBounds(minimum As Double, maximum As Double) As ChartAxisScaleResult',
        '        Return New ChartAxisScaleResult(Math.Min(minimum, 0.0R), Math.Max(maximum, 0.0R), 0.0R)', '    End Function', 'End Module','']))
    s=source('MultipleComparisonPlotExcel').replace('Dim y(n - 1) As Double','Dim y As Double() = ComparisonCoordinatesCore.OrdinalY(n)').replace('                y(i) = n - i\n','')
    s=s.replace('''            minCI = Math.Min(minCI, 0.0)
            maxCI = Math.Max(maxCI, 0.0)''','''            Dim referenceBounds As ChartAxisScaleResult = ComparisonCoordinatesCore.IncludeZeroBounds(minCI, maxCI)
            minCI = referenceBounds.Min
            maxCI = referenceBounds.Max''')
    s=s.replace('New Double() {0.0, 0.0}','ChartReferenceSeries.Vertical(0.0R, 0.5R, n + 0.5R).XValues').replace('New Double() {0.5, n + 0.5}','ChartReferenceSeries.Vertical(0.0R, 0.5R, n + 0.5R).YValues')
    write('BESHstatNG/src/Graphics/MultipleComparisonPlotExcel.vb',s)
    finite=member('PsmLovePlotExcel','HasAnyFinite').replace('Private Shared ','Public ').replace('AppInfrastructure.IsFinite(v)','Not Double.IsNaN(v) AndAlso Not Double.IsInfinity(v)')
    write('BESHStat.Excel.Common/src/Analysis/PsmLoveSeriesPolicy.vb',header()+'''Namespace ExcelCommon
Public Module PsmLoveSeriesPolicy
'''+finite+'''    Public Function ThresholdReference(threshold As Double, count As Integer) As ChartReferenceSeries
        Return ChartReferenceSeries.Vertical(threshold, 0.5R, count + 0.5R)
    End Function
End Module
End Namespace
''')
    s=source('PsmLovePlotExcel')
    s=replace_member(s,'HasAnyFinite','''    Private Shared Function HasAnyFinite(values As IEnumerable(Of Double)) As Boolean
        Return PsmLoveSeriesPolicy.HasAnyFinite(values)
    End Function''')
    s=s.replace('New Double() {threshold, threshold}','PsmLoveSeriesPolicy.ThresholdReference(threshold, n).XValues').replace('New Double() {0.5R, n + 0.5R}','PsmLoveSeriesPolicy.ThresholdReference(threshold, n).YValues')
    write('BESHstatNG/src/Graphics/PsmLovePlotExcel.vb',s)
    raw=member('SpcControlChartExcel','ComputeYAxisBounds');b=body(raw);start=b.index('            If Double.IsPositiveInfinity(minimum)')
    tail=b[start:].replace('panel.PanelType','panelType').replace('New AxisBounds(0.0R, 1.0R)','New ChartAxisScaleResult(0.0R, 1.0R, 0.0R)').replace('New AxisBounds(minimum, maximum)','New ChartAxisScaleResult(minimum, maximum, 0.0R)')
    helper=member('SpcControlChartExcel','HasNaturalZero').replace('Private Shared ','Private ')
    write('BESHStat.Core/src/Graphics/SpcAxisBoundsCore.vb',header('Imports BESHStatNG.StatisticalProcessControl\n')+'Public Module SpcAxisBoundsCore\n    Public Function FromExtents(minimum As Double, maximum As Double, panelType As SpcPanelType) As ChartAxisScaleResult\n'+tail+'    End Function\n'+helper+'End Module\n')
    s=source('SpcControlChartExcel')
    # Keep marshalling/extents in their existing order; only numeric padding and
    # domain clamp move. No ten-channel/stage/label behavior is changed here.
    originalTail=b[start:]
    newTail='''            Dim bounds As ChartAxisScaleResult = SpcAxisBoundsCore.FromExtents(minimum, maximum, panel.PanelType)
            Return New AxisBounds(bounds.Min, bounds.Max)
'''
    assert originalTail in s;s=s.replace(originalTail,newTail)
    s=replace_member(s,'HasNaturalZero','')
    write('BESHstatNG/src/Graphics/SpcControlChartExcel.vb',s)
    print('Delegated Agreement, comparison and Love foundational references; SPC numeric bounds')
