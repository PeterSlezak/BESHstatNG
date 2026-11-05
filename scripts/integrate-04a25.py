"""Apply only 04A.25 shipping seams after entry validation; no Core or old test edits."""
import importlib.util
from pathlib import Path
import re

s=importlib.util.spec_from_file_location('c25_integrate',Path(__file__).with_name('capture-04a25-contract.py'));c=importlib.util.module_from_spec(s);s.loader.exec_module(c)
def edit(path,transform):
    p=c.ROOT/path;raw=p.read_bytes();bom=raw.startswith(b'\xef\xbb\xbf');text=raw.decode('utf-8-sig').replace('\r\n','\n')
    new=transform(text)
    if b'\r\n' in raw:new=new.replace('\n','\r\n')
    p.write_bytes((b'\xef\xbb\xbf' if bom else b'')+new.encode())
def form(text):
    text='Imports BESHStatNG.ExcelCommon\n'+text
    text=text.replace('        Public SourceRowIds As Integer()','        Public SourceRowIds As Integer()\n        Public AlignedAxis As PsmAlignedInputAxis')
    text=text.replace('        out.SourceRowIds = importer.RowIds','        out.SourceRowIds = importer.RowIds\n        out.AlignedAxis = importer.AlignedAxis')
    text=text.replace('        NormalizeFitOptionsForRunMethod(fitOptions)\n        Return fitOptions','        Return New PsmRequestSelection(fitOptions).CreateEffectiveOptions()')
    methods=c.methods(text)
    text=text.replace(methods['NormalizeFitOptionsForRunMethod'],'''    Private Sub NormalizeFitOptionsForRunMethod(fitOptions As PsmComprehensiveFitOptions)
        PsmRequestSelection.Normalize(fitOptions)
    End Sub''')
    text=text.replace('        Dim supplied As Boolean = (scoreMethod = PsmScoreMethod.Supplied)','        Dim intent As New PsmControlIntent(runMethod, estimand, scoreMethod)\n        Dim supplied As Boolean = intent.Supplied')
    for member,prop in [('UsesMatchingControls','Matching'),('UsesNearestNeighborOptions','NearestNeighbor'),('UsesOptimalPairOptions','OptimalPair'),('UsesSubclassificationControls','Subclassification'),('UsesCemControls','Cem')]:
        text=text.replace('PsmMethodCapabilities.'+member+'(runMethod)','intent.'+prop)
    text=text.replace('PsmMethodCapabilities.SupportsDoublyRobust(runMethod, estimand)','intent.DoublyRobust')
    text=text.replace('PsmMethodCapabilities.SupportedEstimands(runMethod)','PsmControlIntent.Estimands(runMethod)')
    replacement='''    Private Sub WritePsmResults(myData As PsmGuiData, fitResult As PsmComprehensiveResult)
        'Preserve the original second UI snapshot and each later checkbox-read boundary.
        Dim fitOptions As PsmComprehensiveFitOptions = BuildFitOptions()
        Dim wb As Excel.Workbook = CreatePsmResultWorkbook()
        Dim presentation As New PsmResultPresentation(myData.Input.RowCount, fitResult)
        Dim sourceRows As Integer() = myData.AlignedAxis.SourceRowIds()
        Dim initial = PsmReportSelectionPolicy.Plan(False, False, False, False, False)
        Dim inputWs As Excel.Worksheet = CreateOrAddResultSheet(wb, initial(0).SheetName, reuseFirstSheet:=True)
        WriteResultTablesToWorksheet(wb, inputWs, initial(0).BuildTables(myData.Input, sourceRows, fitResult, fitOptions, Nothing))

        Dim resultWs As Excel.Worksheet = CreateOrAddResultSheet(wb, initial(1).SheetName, reuseFirstSheet:=False)
        WriteResultTablesToWorksheet(wb, resultWs, initial(1).BuildTables(myData.Input, sourceRows, fitResult, fitOptions, DataImportSummaryTable(myData)))

        For Each sheet In PsmReportSelectionPolicy.DiagnosticSheets(Me.chkWriteDiagnostics.Checked)
            WriteResultTablesToNewSheet(wb, sheet.SheetName, sheet.BuildTables(myData.Input, sourceRows, fitResult, fitOptions, Nothing))
        Next
        For Each sheet In PsmReportSelectionPolicy.LoveSheets(Me.chkLovePlot.Checked, presentation.HasLoveRows)
            WriteLovePlotSheetIfAvailable(wb, fitResult)
        Next
        For Each sheet In PsmReportSelectionPolicy.MatchSheets(Me.chkWriteMatches.Checked)
            WriteResultTablesToNewSheet(wb, sheet.SheetName, sheet.BuildTables(myData.Input, sourceRows, fitResult, fitOptions, Nothing))
        Next
        For Each sheet In PsmReportSelectionPolicy.CemSheets(presentation.HasCem)
            WriteResultTablesToNewSheet(wb, sheet.SheetName, sheet.BuildTables(myData.Input, sourceRows, fitResult, fitOptions, Nothing))
        Next
        Try
            resultWs.Activate()
        Catch
        End Try
    End Sub'''
    text=text.replace(methods['WritePsmResults'],replacement)
    text=text.replace('        Dim ws As Excel.Worksheet = CreateOrAddResultSheet(wb, "Love Plot", reuseFirstSheet:=False)','        Dim projection As New PsmLovePlotProjection(fitResult.LovePlotRows)\n        Dim ws As Excel.Worksheet = CreateOrAddResultSheet(wb, "Love Plot", reuseFirstSheet:=False)')
    text=text.replace('graphics.PsmLovePlotExcel.BuildPlotDataTable(fitResult.LovePlotRows)','graphics.PsmLovePlotExcel.BuildProjectedDataTable(projection)')
    text=text.replace('graphics.PsmLovePlotExcel.AddChart(ws, fitResult.LovePlotRows)','graphics.PsmLovePlotExcel.AddProjectedChart(ws, projection)')
    return text
def importer(text):
    text=text.replace('Imports BESHStatNG.CausalInference','Imports BESHStatNG.CausalInference\nImports BESHStatNG.ExcelCommon')
    text=text.replace('    Public Property Input As PsmInputData','    Public Property AlignedAxis As PsmAlignedInputAxis\n    Public Property Input As PsmInputData')
    text=text.replace('        Input = Nothing','        Input = Nothing\n        AlignedAxis = Nothing')
    text=text.replace('            .ExactGroupLabels = exactLabels\n        }','            .ExactGroupLabels = exactLabels\n        }\n        AlignedAxis = New PsmAlignedInputAxis(Input, Me.RowIds, RawCovariateKeys,\n            EffectiveEffectItems.Select(Function(item) If(item Is Nothing, Nothing, item.ToString())), DroppedRowsDuringAlignment)')
    return text
def renderer(text):
    text=text.replace('Imports BESHStatNG.CausalInference','Imports BESHStatNG.CausalInference\nImports BESHStatNG.ExcelCommon')
    start=text.index('        Public Shared Function BuildPlotDataTable(');end=text.index('        End Function',start)+len('        End Function')
    text=text[:start]+'''        Public Shared Function BuildPlotDataTable(rows As IList(Of PsmLovePlotRow)) As Object(,)
            Return BuildProjectedDataTable(New PsmLovePlotProjection(rows))
        End Function

        Public Shared Function BuildProjectedDataTable(projection As PsmLovePlotProjection) As Object(,)
            Return projection.BuildDataTable()
        End Function'''+text[end:]
    start=text.index('            Dim cleanRows',text.index('        Public Shared Sub AddChart('));end=text.index('            Dim shp',start)
    text=text[:start]+'''            AddProjectedChart(ws, New PsmLovePlotProjection(rows), left, top, width, height)
        End Sub

        Public Shared Sub AddProjectedChart(ws As Worksheet, projection As PsmLovePlotProjection,
                                            Optional left As Double = 420, Optional top As Double = 20,
                                            Optional width As Double = 720, Optional height As Double = 420)
            If ws Is Nothing OrElse projection Is Nothing OrElse projection.Count = 0 Then Return
            Dim yValues = projection.YValues
            Dim xBefore = projection.Before
            Dim xAfterMatching = projection.AfterMatching
            Dim xAfterWeighting = projection.AfterWeighting
            Dim labels = projection.Labels
            Dim threshold = projection.Threshold
            Dim maxX = projection.MaximumX

'''+text[end:]
    text=text.replace('cleanRows.Count','projection.Count')
    for name in ['AbsForChart','AbsOrMissing','MaxFiniteAbs','MaxFinite']:
        pattern=r'^        Private Shared Function '+name+r'\b.*?^        End Function\n'
        text=re.sub(pattern,'',text,flags=re.M|re.S)
    return text
def install():
    assert not c.git('diff','--cached','--name-only')
    assert (c.OUT/'entry-validation.json').exists()
    for name in ['PsmAlignedInputAxis','PsmResultPresentation','PsmReportSelectionPolicy','Psm25Tests','Psm25Fixture']:
        folder='BESHStat.Excel.Common.Tests/src/' if name.startswith('Psm25') else 'BESHStat.Excel.Common/src/Analysis/'
        (c.ROOT/(folder+name+'.vb')).write_bytes((c.RUN/(name+'.pending')).read_bytes())
    (c.ROOT/'BESHStatNG.Test/src/Psm25AdapterTests.vb').write_bytes((c.RUN/'Psm25AdapterTests.pending').read_bytes())
    edit(c.FORM,form);edit('BESHstatNG/src/DM/PSMdataImport.vb',importer);edit('BESHstatNG/src/Graphics/PsmLovePlotExcel.vb',renderer)
    def project(text):
        additions='\n'.join('    <Compile Include="..\\BESHStat.Excel.Common.Tests\\src\\'+name+'.vb"><Link>Common\\'+name+'.vb</Link></Compile>' for name in ['Psm25Tests','Psm25Fixture','Psm25Legacy'])
        return text.replace('    <Compile Include="src\\PsmCapabilityContractTests.vb" />','    <Compile Include="src\\PsmCapabilityContractTests.vb" />\n    <Compile Include="src\\Psm25AdapterTests.vb" />\n'+additions)
    edit('BESHStatNG.Test/BESHStatNG.Tests.vbproj',project)
if __name__=='__main__':install()
