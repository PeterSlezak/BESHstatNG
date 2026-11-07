"""Freeze and extract foundational reference vectors; detailed CI/report work stays 35/36."""
from entry import ROOT,HERE,save,pin,sha,git,BASE
from integrate import header,write,replace_member
from freeze_slices import source,member,body
import json
if __name__=='__main__':
    files=['AgreementPlotExcel','MultipleComparisonPlotExcel','PsmLovePlotExcel','SpcControlChartExcel']
    for file in files:
        path='BESHstatNG/src/Graphics/'+file+'.vb'
        assert (ROOT/path).read_bytes().replace(b'\r\n',b'\n')==__import__('subprocess').check_output(['git','-c','safe.directory='+ROOT.as_posix(),'show',BASE+':'+path],cwd=ROOT).replace(b'\r\n',b'\n')
    records=[dict(pin('BESHstatNG/src/Graphics/'+n+'.vb'),authority='PRE_EDIT_LEGACY_SOURCE',source_lf=source(n)) for n in files]
    assert not (HERE/'pre-edit-reference-source.json').exists()
    save('pre-edit-reference-source.json',dict(files=records,scientific='NOT_GOLDEN',native='NATIVE_NOT_RUN'))
    # Original scalar loops, frozen independently before changing the adapters.
    raw=member('MultipleComparisonPlotExcel','AddPlot')
    b=raw[raw.index('            Dim n As Integer'):raw.index('            Dim xScale As CHARTscale')]
    b=b.replace('            Dim height As Double = SuggestedHeight(data)\n','')
    b=b.replace('data.Estimates','estimates').replace('data.LowerLimits','lower').replace('data.UpperLimits','upper')
    b=b.replace('            Dim plusError(n - 1) As Double\n','').replace('            Dim minusError(n - 1) As Double\n','')
    b='\n'.join(line for line in b.splitlines() if 'plusError(i)' not in line and 'minusError(i)' not in line)+'\n'
    multi='Public Function ComparisonBasis(estimates As Double(), lower As Double(), upper As Double()) As Double()()\n'+b+'Return New Double()(){y,New Double(){minCI,maxCI}}\nEnd Function\n'
    raw=member('SpcControlChartExcel','ComputeYAxisBounds');b=body(raw)
    b=b[b.index('            If Double.IsPositiveInfinity(minimum)'):]
    b=b.replace('New AxisBounds(','New ChartAxisScaleResult(').replace('0.0R, 1.0R)','0.0R, 1.0R, 0.0R)').replace('minimum, maximum)','minimum, maximum, 0.0R)')
    spc='Public Function SpcBounds(minimum As Double, maximum As Double, panelType As SpcPanelType) As ChartAxisScaleResult\n'+b.replace('panel.PanelType','panelType')+'End Function\n'+member('SpcControlChartExcel','HasNaturalZero').replace('Private Shared ','Private ')
    fit='Public Function FitLine(minimum As Double, maximum As Double, intercept As Double, slope As Double) As Double()()\nReturn New Double()(){New Double(){minimum,maximum},New Double(){intercept + slope * minimum,intercept + slope * maximum}}\nEnd Function\n'
    finite=member('PsmLovePlotExcel','HasAnyFinite').replace('Private Shared ','Public ').replace('AppInfrastructure.IsFinite(v)','Not Double.IsNaN(v) AndAlso Not Double.IsInfinity(v)')
    fixture=header('Imports BESHStatNG\nImports BESHStatNG.StatisticalProcessControl\n')+'Namespace BESHStatNG.TestFixtures34\nPublic Module LegacyReference34\n'+multi+spc+fit+finite+'End Module\nEnd Namespace\n'
    write('evidence/04A34/LegacyReference34.vb',fixture)
    save('pre-edit-reference-specification.json',dict(authority='PRE_EDIT_LEGACY',fixture=pin('evidence/04A34/LegacyReference34.vb'),notes='Source-adapted scalar signatures; comparison CI errors and report/title/validation unchanged. SPC only post-extents numerical padding/clamping; channel masks remain accepted Common. Fit expression copied from original Passing-Bablok/Deming rendering.'))
    # Do not edit production until this fixture has been compiled and executed.
