"""Narrow, reviewable Ui22 post-fit delegation from frozen actual main."""
from pathlib import Path
import importlib.util
import re
ROOT=Path(__file__).resolve().parents[1]
s=importlib.util.spec_from_file_location('capture24',ROOT/'scripts/capture-04a24-contract.py');c=importlib.util.module_from_spec(s);s.loader.exec_module(c)
def integrate():
    p=ROOT/c.FORM;raw=p.read_bytes();assert c.sha(p)==c.sha(c.RUN/'preimages/Ui22MultivariateControlCharts.vb')
    text=raw.decode('utf-8-sig').replace('\r\n','\n');original=c.methods(text)
    write=original['WriteResultsToNewWorkbook'].replace('        Dim firstAvailable As Boolean = True','        Dim presentation As New SpcMultivariateReportProjection(result)\n        Dim firstAvailable As Boolean = True',1)
    sections=[('Multivariate Summary','Summary'),('Model Details','ModelDetails'),('Multivariate Chart','Chart'),('Chart Data','ChartData'),('Signals','Signals'),('Diagnostics','Diagnostics'),('Settings and Audit','SettingsAudit')]
    for title,section in sections:write=write.replace('"'+title+'", firstAvailable','SpcMultivariateSections.Definition(SpcMultivariateReportSection.'+section+').SheetName, firstAvailable')
    for name in ['BuildSummaryTables','BuildModelTables','BuildChartDataTable','BuildSignalsTable','BuildDiagnosticTables','BuildAuditTables']:write=write.replace(name+'(result)',name+'(presentation)')
    write=write.replace('                graphics.SpcControlChartExcel.AddCharts(sheet, BuildChartRenderingResult(result), BuildAppearanceOptions())','''                Dim payload = BuildChartRenderingResult(presentation)
                Dim appearance = BuildAppearanceOptions()
                Dim channels = payload.ProjectSeries(appearance.UseSequenceValuesForHorizontalAxis)
                graphics.SpcControlChartExcel.AddCharts(sheet, payload.RenderingResult, appearance, channels)''')
    text=text.replace(original['WriteResultsToNewWorkbook'],write)
    replacement='''    Private Function BuildChartRenderingResult(presentation As SpcMultivariateReportProjection) As SpcMultivariateChartPayload
        Dim title As String = tbChartTitle.Text
        Dim caption As String = If(String.IsNullOrWhiteSpace(title), SelectedChartChoice().DisplayText, String.Empty)
        Dim intent As New SpcMultivariateChartIntent(title, tbValueAxisTitle.Text, caption)
        Return SpcMultivariateChartProjectionPolicy.Prepare(presentation, intent)
    End Function'''
    text=text.replace(original['BuildChartRenderingResult'],replacement)
    for name in ['BuildSummaryTables','BuildModelTables','BuildChartDataTable','BuildSignalsTable','BuildDiagnosticTables','BuildAuditTables']:
        return_type='ResultTable' if name in ['BuildChartDataTable','BuildSignalsTable'] else 'List(Of ResultTable)'
        if name=='BuildDiagnosticTables':body='''        Dim signalOnly As Boolean = SelectedComboValue(Of DiagnosticsScope)(cbDiagnosticsScope) = DiagnosticsScope.SignalledPointsOnly
        Return presentation.BuildDiagnosticTables(signalOnly)'''
        elif name=='BuildAuditTables':body='''        Dim view As New SpcMultivariateAuditView(tbChartTitle.Text.Trim(), tbValueAxisTitle.Text.Trim(), tbHorizontalAxisTitle.Text.Trim(),
            chkUseSequenceValuesForHorizontalAxis.Checked, spinChartWidth.Value, spinChartHeight.Value)
        Return presentation.BuildAuditTables(view)'''
        else:body='        Return presentation.'+name+'()'
        text=text.replace(original[name],'    Private Function '+name+'(presentation As SpcMultivariateReportProjection) As '+return_type+'\n'+body+'\n    End Function')
    for name in ['BuildWarningsTable','CollectWarnings','CreateTable','CreateVectorTable','CreateIndexedVectorTable','CreateMatrixTable','ComponentNames','AddVectorValues','Row','MissingPolicyText','ModelSourceText','SequenceGapBehaviorText','ChartTypeText','YesNo','NullableValue','JoinIntegers']:
        # Includes both overloads of NullableValue; other native helpers stay.
        pattern=r'^    Private (?:Shared )?(Sub|Function) '+name+r'\b.*?^    End \1\n?'
        text=re.sub(pattern,'',text,flags=re.M|re.S)
    for name,arg in [('PhaseText','value'),('ExclusionScopeText','scope')]:
        body=c.methods(text)[name]
        signature=body.split('\n')[0]
        # Parameter spelling remains the actual native signature.
        parameter=re.search(r'\((\w+) As',signature)[1]
        text=text.replace(body,signature+'\n        Return SpcMultivariateReportProjection.'+name+'('+parameter+')\n    End Function')
    assert text.count('SpcMultivariate.Fit(')==1
    for name in ['BuildRequest','btCompute_Click','BuildAppearanceOptions','WriteResultTables','CreateResultWorkbook','CreateResultSheet','ValidateSelections','BuildHistoricalModel']:
        assert c.methods(text)[name]==original[name],name
    newline='\r\n' if b'\r\n' in raw else '\n';p.write_bytes(text.replace('\n',newline).encode('utf-8'))
    print('Ui22 post-fit delegation integrated; request/Fit/native writer/appearance unchanged')
if __name__=='__main__':integrate()
