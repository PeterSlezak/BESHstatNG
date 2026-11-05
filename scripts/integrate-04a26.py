"""One-time 04A.26 native consumption. Preserve imports/coercion/writers and unrelated modes."""
import importlib.util
import json
from pathlib import Path
import re
s=importlib.util.spec_from_file_location('c26_native',Path(__file__).with_name('capture-04a26-contract.py'));c=importlib.util.module_from_spec(s);s.loader.exec_module(c)
def write_source(path,text):
    pre=c.RUN/'preimages'/Path(path).name;raw=pre.read_bytes()
    if b'\r\n' in raw:text=text.replace('\n','\r\n')
    (c.ROOT/path).write_bytes((b'\xef\xbb\xbf' if raw.startswith(b'\xef\xbb\xbf') else b'')+text.encode('utf-8'))
def install():
    p=c.FORM;text=(c.RUN/'preimages'/Path(p).name).read_text(encoding='utf-8-sig')
    for item in sorted(json.loads((c.RUN/'planning-integration.json').read_bytes()),key=lambda r:r['start'],reverse=True):
        pos=item['start'];indent=text[text.rfind('\n',0,pos)+1:pos]
        replacement=item['replacement'].replace('\n        ','\n'+indent)
        method=re.search(r'SampleSizePlanningMethod\.(\w+)',replacement)[1]
        replacement=re.sub(r'\brequest\b','request'+method,replacement)
        text=text[:pos]+replacement+text[item['end']:]
    # Default-text formatting remains native; only the numeric intent is detached.
    paired=text.index('If Me.Text = "Sample Size - Paired T-test" Then')
    stop=text.index('ElseIf Me.Text = "Sample Size - Unpaired T-test" Then',paired)
    segment=text[paired:stop].replace('FormatUiDouble(5.0)','FormatUiDouble(SampleSizePlanningDefaults.Inputs(SampleSizePlanningMethod.PairedTTest)(0))').replace('FormatUiDouble(10.0)','FormatUiDouble(SampleSizePlanningDefaults.Inputs(SampleSizePlanningMethod.PairedTTest)(1))')
    text=text[:paired]+segment+text[stop:]
    single=text.index('ElseIf Me.Text = "Sample Size - Single Proportion" Then')
    stop=text.index('ElseIf Me.Text = "Sample Size - Independent Proportions" Then',single)
    segment=text[single:stop].replace('FormatUiDouble(0.5)','FormatUiDouble(SampleSizePlanningDefaults.Inputs(SampleSizePlanningMethod.SingleProportion)(0))').replace('FormatUiDouble(0.4)','FormatUiDouble(SampleSizePlanningDefaults.Inputs(SampleSizePlanningMethod.SingleProportion)(1))')
    text=text[:single]+segment+text[stop:]
    write_source(p,'Imports BESHStatNG.ExcelCommon\n'+text)

    p='BESHstatNG/src/UI/UiTwoInputRefedits.vb';text=(c.RUN/'preimages'/Path(p).name).read_text(encoding='utf-8-sig')
    text='Imports BESHStatNG.ExcelCommon\n'+text
    match=re.search(r'Public Class \w+',text);text=text[:match.end()]+'''\n    Private ReadOnly agreementSourceRows As New System.Runtime.CompilerServices.ConditionalWeakTable(Of TwoGroupsPairedData, Integer())
'''+text[match.end():]
    methods=c.methods(text);get=methods['getData']
    get=get.replace('        Return out','''        If (Me.Tag = HelpTopic.DemingRegression OrElse Me.Tag = HelpTopic.LinsConcordanceCorrelationCoefficient) AndAlso columData.RowIds IsNot Nothing Then
            agreementSourceRows.Add(out, CType(columData.RowIds.Clone(), Integer()))
        End If
        Return out''');text=text.replace(methods['getData'],get)
    cat=methods['getPairedCategoricalData'].replace('name2 As String)','name2 As String, axis As AgreementInputAxis)')
    cat=cat.replace('Return (Nothing, Nothing, Nothing, Nothing)','Return (Nothing, Nothing, Nothing, Nothing, Nothing)')
    cat=cat.replace('                columData.varNames(1))','''                columData.varNames(1),
                New AgreementInputAxis(columData.FinalData.GetLength(0), columData.varNames, columData.RowIds,
                    categoricalFirst:=Matrix.GetColumnFrom2Darray(columData.FinalData, 0),
                    categoricalSecond:=Matrix.GetColumnFrom2Darray(columData.FinalData, 1), header:=Me.ckFirstRow.Checked))''')
    text=text.replace(methods['getPairedCategoricalData'],cat)
    for name,kind in [('RunCohensKappa','Kappa'),('RunLinsCCC','LinConcordance'),('RunDeming','Deming')]:
        body=methods[name]
        if kind=='Kappa':
            body=body.replace('catData.x, catData.y','catData.axis.FirstCategories(), catData.axis.SecondCategories()')
            start=body.index('        Select Case Me.cmbWeightingSchemeKappa');end=body.index('        Try',start)
            body=body[:start]+'''        Dim selection As New KappaRequestSelection(opts, Me.cmbWeightingSchemeKappa.SelectedIndex,
            Me.optKappaAnalytical.Checked, Me.optKappaJackknife.Checked, Me.optKappaBootstrapPercentile.Checked, Me.optKappaBootstrapBCa.Checked)
        opts = selection.CreateOptions()

'''+body[end:]
            start=body.index('            Dim useResampling As Boolean');end=body.index('            If useResampling',start)
            body=body[:start]+'            Dim useResampling As Boolean = selection.UsesResampling\n'+body[end:]
            axis='catData.axis'
        else:
            # Original matrix extraction order remains before Core construction; no new input rejection.
            start=body.index('        Dim tt = New Agreement.');end=body.index('        Dim opts As New',start)
            owner='WeightedDemingRegression' if kind=='Deming' else 'LinConcordanceCorrelation'
            body=body[:start]+f'''        Dim first = Matrix.GetColumnFrom2Darray(data.X, 0)
        Dim second = Matrix.GetColumnFrom2Darray(data.X, 1)
        Dim physicalRows As Integer() = Nothing
        agreementSourceRows.TryGetValue(data, physicalRows)
        Dim axis As New AgreementInputAxis(first.Length, {{data.name1, data.name2}}, physicalRows, first, second)
        Dim tt = New Agreement.{owner}(axis.First(), axis.Second(), data.name1, data.name2)

'''+body[end:]
            axis='axis'
            if kind=='LinConcordance':
                start=body.index('        If Me.optLinCCCAnalytical.Checked');end=body.index('        tt.Options = opts',start)
                body=body[:start]+'''        Dim selection As New LinRequestSelection(opts, Me.optLinCCCAnalytical.Checked, Me.optLinCCCBootstrapPercentile.Checked, Me.optLinCCCBootstrapBCa.Checked)
        opts = selection.CreateOptions()
        Dim useBootstrap As Boolean = selection.UsesBootstrap

'''+body[end:]
            else:
                for model in ['ConstantCV','KnownPointwiseSD','ConstantLambda']:
                    body=body.replace('opts.VarianceModel = Agreement.DemingVarianceModel.'+model,'opts.VarianceModel = DemingRequestSelection.VarianceForIndex(Me.cmbDemingVarianceModel.SelectedIndex)')
                start=body.index('        If Me.optDemingBootstrapPercentile.Checked');end=body.index('        Try',start)
                body=body[:start]+'''        Dim selection As New DemingRequestSelection(opts, Me.optDemingBootstrapPercentile.Checked, Me.optDemingBootstrapBCa.Checked,
            Me.optAnalyticalClosedForm.Checked, Me.optAnalyticalLinnet.Checked)
        opts = selection.CreateOptions()
        tt.Options = opts
        Dim useBootstrap As Boolean = selection.Execution = DemingExecutionIntent.Bootstrap

'''+body[end:]
                body=body.replace('opts.CiMethod = Agreement.AgreementCiMethod.Analytical AndAlso Me.optAnalyticalLinnet.Checked','selection.Execution = DemingExecutionIntent.AnalyticalMcr').replace('ElseIf opts.CiMethod = Agreement.AgreementCiMethod.Analytical Then','ElseIf selection.Execution = DemingExecutionIntent.AnalyticalClosedForm Then').replace('ElseIf opts.CiMethod = Agreement.AgreementCiMethod.Jackknife Then','ElseIf selection.Execution = DemingExecutionIntent.Jackknife Then')
        body=body.replace('Dim res = tt.wrapResults()',f'Dim presentation As New AgreementReportPlan(AgreementMethod.{kind}, tt.wrapResults(), {axis})\n            Dim res = presentation.TablesForWriter()')
        body=body.replace('rr.TotRows + res.Count - 1','presentation.NativeRowSpan(rr.TotRows)')
        body=body.replace('            tt.AddPlot(WriteRes.ws)','            If presentation.PlotRequested Then tt.AddPlot(WriteRes.ws)')
        text=text.replace(methods[name],body)
    # Resolve actual live enum spelling rather than imposing an archive spelling.
    if 'HelpTopic.LinsConcordanceCorrelationCoefficient' not in (c.RUN/'preimages/UiTwoInputRefedits.vb').read_text(encoding='utf-8-sig'):
        existing=re.findall(r'HelpTopic\.(\w*[Ll]in\w*)',(c.RUN/'preimages/UiTwoInputRefedits.vb').read_text(encoding='utf-8-sig'));assert existing
        text=text.replace('HelpTopic.LinsConcordanceCorrelationCoefficient','HelpTopic.'+existing[0])
    write_source(p,text)

    p='BESHstatNG/src/UI/Ui9ANOVA2nested.vb';text=(c.RUN/'preimages'/Path(p).name).read_text(encoding='utf-8-sig');text='Imports BESHStatNG.ExcelCommon\n'+text
    match=re.search(r'Public Class \w+',text);text=text[:match.end()]+'\n    Private ReadOnly blandSourceRows As New System.Runtime.CompilerServices.ConditionalWeakTable(Of MultiGroupsPairedDataObj, Integer())\n'+text[match.end():]
    methods=c.methods(text);get=methods['getDataPB'].replace('        Return out','''        If Me.Tag = HelpTopic.BlandAltman AndAlso byIdData.RowIds IsNot Nothing Then blandSourceRows.Add(out, CType(byIdData.RowIds.Clone(), Integer()))
        Return out''');text=text.replace(methods['getDataPB'],get)
    body=methods['RunBlandAltman'];start=body.index('        Select Case Me.cmbBlandXAxis');end=body.index('        If Not WindowsPairedInputAdapter.Accept',start)
    body=body[:start]+'''        Dim selection As New BlandRequestSelection(opts, Me.cmbBlandXAxis.SelectedIndex, Me.cmbBlandPlotMode.SelectedIndex)
        opts = selection.CreateOptions()

'''+body[end:]
    start=body.index('        Dim ba As New Agreement.BlandAltmanAgreement');end=body.index('        Dim fit = ba.Fit()',start)
    body=body[:start]+'''        Dim referenceName = If(hasSubjectIds, d.varNames(1), d.varNames(0))
        Dim testName = If(hasSubjectIds, d.varNames(2), d.varNames(1))
        Dim physicalRows As Integer() = Nothing
        blandSourceRows.TryGetValue(d, physicalRows)
        Dim axis As New AgreementInputAxis(x.Length, {referenceName, testName}, physicalRows, x, y, subIds)
        Dim ba As New Agreement.BlandAltmanAgreement(axis.First(), axis.Second(), referenceName, testName, opts)
'''+body[end:]
    body=body.replace('            res.Add(Me.BuildBlandAllowableBiasTable(biasAssessment, scaleText, fit.UsedRepeatedModel))\n            res.Add(Me.BuildBlandDecisionLimitTable(loaAssessment, scaleText, fit.UsedRepeatedModel))','''            AgreementReportPlan.AppendDecisionTables(res,
                Me.BuildBlandAllowableBiasTable(biasAssessment, scaleText, fit.UsedRepeatedModel),
                Me.BuildBlandDecisionLimitTable(loaAssessment, scaleText, fit.UsedRepeatedModel))''')
    body=body.replace('        Dim rr = New ProcessListofResultTables(res)','        Dim presentation As New AgreementReportPlan(AgreementMethod.BlandAltman, res, axis)\n        res = presentation.TablesForWriter()\n        Dim rr = New ProcessListofResultTables(res)').replace('rr.TotRows + res.Count + res.Count - 1','presentation.NativeRowSpan(rr.TotRows)').replace('        ba.AddPlot(WriteRes.ws)','        If presentation.PlotRequested Then ba.AddPlot(WriteRes.ws)')
    text=text.replace(methods['RunBlandAltman'],body);write_source(p,text)

    p='BESHstatNG/src/UI/Ui0OneRefeditMulticol.vb';text=(c.RUN/'preimages'/Path(p).name).read_text(encoding='utf-8-sig')
    match=re.search(r'Public Class \w+',text);text=text[:match.end()]+'\n    Private ReadOnly iccSourceAxes As New System.Runtime.CompilerServices.ConditionalWeakTable(Of MultiGroupsUnpairedData, AgreementInputAxis)\n'+text[match.end():]
    methods=c.methods(text);get=methods['getDataICC11'].replace('        Return out','        iccSourceAxes.Add(out, New AgreementInputAxis(g, columData.varNames, columData.RowIds))\n        Return out');text=text.replace(methods['getDataICC11'],get)
    body=methods['RunICC'];start=body.index('        Dim strIccType As String')
    intent='''        Dim intent As New IccRequestIntent(Me.optICC11.Checked, Me.optICC1k.Checked, Me.optICC21.Checked,
            Me.optICC2k.Checked, Me.optICC31.Checked, Me.optICC3k.Checked, CDbl(Me.spinBtnAlphaICC.Value), Me.ckRepeatabilityCoefficient.Checked)
        Dim axis As AgreementInputAxis = Nothing
        If data1 IsNot Nothing Then
            iccSourceAxes.TryGetValue(data1, axis)
        Else
            Dim evidence = WindowsPairedInputAdapter.GetBlockEvidence(data2)
            axis = If(evidence Is Nothing, New AgreementInputAxis(data2.X.GetLength(0), data2.varNames, Nothing), AgreementInputAxis.FromBlocks(evidence))
        End If
'''
    body=body[:start]+intent+body[start:]
    # Leave the pre-import acquisition predicate unchanged; only the post-import Core dispatch uses the snapshot.
    start=body.index('        Dim strIccType As String');tail=body[start:]
    for old,new in [('optICC11','OneOne'),('optICC1k','OneAverage'),('optICC21','TwoOne'),('optICC2k','TwoAverage'),('optICC31','ThreeOne'),('optICC3k','ThreeAverage')]:tail=tail.replace('Me.'+old+'.Checked','intent.Choice = IccEstimatorChoice.'+new)
    tail=tail.replace('Me.spinBtnAlphaICC.Value','intent.Alpha').replace('Me.ckRepeatabilityCoefficient.Checked','intent.Repeatability')
    tail=re.sub(r'strIccType = (?:\$?"ICC\([^\n]+)',lambda m:'strIccType = intent.Label(data2.X.GetLength(1))' if 'data2' in m[0] else 'strIccType = intent.Label(0)',tail)
    tail=tail.replace('        Dim res = icc.wrapResults(iccNum, strIccType)','        Dim presentation As New AgreementReportPlan(AgreementMethod.Icc, icc.wrapResults(iccNum, strIccType), axis)\n        Dim res = presentation.TablesForWriter()').replace('rr.TotRows + res.Count - 1','presentation.NativeRowSpan(rr.TotRows)')
    body=body[:start]+tail;text=text.replace(methods['RunICC'],body);write_source(p,text)
if __name__=='__main__':install()
