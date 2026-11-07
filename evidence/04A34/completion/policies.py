"""Capture and consume explicit plot-square and ordered legend policy seams."""
from work import *
FUNCTIONS='''Public Function PolarSquare(width As Double, height As Double, left As Double, top As Double) As PolarSquareLayout34
Dim side As Double = Math.Min(width,height)
Dim adjustedLeft As Double = left + (width - side) / 2.0R
Dim adjustedTop As Double = top + (height - side) / 2.0R
Return New PolarSquareLayout34(adjustedLeft,adjustedTop,side)
End Function
Public Function SankeyNodeOrder(nodes As SankeyNode()) As SankeyNode()
Return nodes.OrderBy(Function(n) n.FirstAppearanceIndex).ToArray()
End Function
Public Function SankeyLegendOrder(firstIndices As Integer()) As Integer()
Return Enumerable.Range(0,firstIndices.Length).OrderBy(Function(i) firstIndices(i)).ToArray()
End Function
Public Function CdfLegendChoice(showLegend As Boolean, allCurves As Boolean, hasEmpirical As Boolean, hasFitted As Boolean) As CdfLegendChoice34
If Not showLegend Then Return New CdfLegendChoice34(False,False,False)
If allCurves Then Return New CdfLegendChoice34(hasEmpirical,hasFitted,False)
If hasFitted Then Return New CdfLegendChoice34(False,True,True)
If hasEmpirical Then Return New CdfLegendChoice34(True,False,True)
Return New CdfLegendChoice34(False,False,False)
End Function
Public Function LadderLegendChoice(showLegend As Boolean, mode As Integer, observationIndex As Integer) As LadderLegendChoice34
If Not showLegend Then Return New LadderLegendChoice34(False,False)
Select Case mode
Case 1
Return New LadderLegendChoice34(True,False)
Case 2
Return New LadderLegendChoice34(False,True)
Case Else
Return New LadderLegendChoice34(observationIndex = 0,False)
End Select
End Function
'''
TYPES=dto('PolarSquareLayout34',[(n,'Double') for n in ['Left','Top','Side']])+dto('CdfLegendChoice34',[(n,'Boolean') for n in ['KeepEmpirical','KeepFitted','RenameRepresentative']])+dto('LadderLegendChoice34',[(n,'Boolean') for n in ['Keep','RecordGroup']])
def freeze():
    assert not (HERE/'pre-edit-policies.csv').exists()
    originals={file+':'+name:method((HERE/'entry-source'/(file+'.vb.txt')).read_text(encoding='utf-8'),name) for file,name in [('PolarPlot','MakePlotAreaSquare'),('SankeyPlotExcel','ResolveCategoryStyles'),('CumulativeDistributionPlotExcel','RegisterLegendSeries'),('LadderPlotExcel','ShouldKeepLegendEntry')]}
    save('policy-spec.json',dict(authority='PRE_EDIT_LEGACY',scientific='NOT_GOLDEN',originals=originals,functions=FUNCTIONS,qualification='Original scalar square arithmetic and ordered legend branch policies; explicit measurements and logical series-presence flags replace native reads. Per-chart HashSet/native PlotOrder getters/setters stay Windows. Sankey order uses only accepted Core FirstAppearanceIndex.'))
    write('BESHStat.Core.Tests/src/Graphics/LegacyCompletionPolicies34.vb',HEADER+'Namespace Global.Batch34PolicyFixtures\nPublic Module LegacyPolicies34\n'+FUNCTIONS+'End Module\n'+TYPES+'End Namespace\n')
def apply():
    assert (HERE/'pre-edit-policies.csv').exists()
    a=FUNCTIONS.index('Public Function SankeyNodeOrder')
    write('BESHStat.Core/src/Graphics/PolarMeasuredLayoutCore.vb',HEADER+'Public Module PolarMeasuredLayoutCore\n'+FUNCTIONS[:a]+'End Module\n'+dto('PolarSquareLayout34',[(n,'Double') for n in ['Left','Top','Side']]))
    write('BESHStat.Excel.Common/src/Analysis/SpecialtyLegendPolicy34.vb',HEADER+'Namespace ExcelCommon.Chart34\nPublic Module SpecialtyLegendPolicy34\n'+FUNCTIONS[a:]+'End Module\n'+dto('CdfLegendChoice34',[(n,'Boolean') for n in ['KeepEmpirical','KeepFitted','RenameRepresentative']])+dto('LadderLegendChoice34',[(n,'Boolean') for n in ['Keep','RecordGroup']])+'End Namespace\n')
    s=src('PolarPlot');start=s.index('            Dim side As Double',s.index('Private Shared Sub MakePlotAreaSquare'));end=s.index('            plotArea.InsideLeft =',start)
    s=s[:start]+'''            Dim prepared = PolarMeasuredLayoutCore.PolarSquare(currentWidth, currentHeight, plotArea.InsideLeft, plotArea.InsideTop)
            Dim side As Double = prepared.Side
            Dim adjustedLeft As Double = prepared.Left
            Dim adjustedTop As Double = prepared.Top

'''+s[end:];write('BESHstatNG/src/Graphics/PolarPlot.vb',s)
    s=src('SankeyPlotExcel');s=s.replace('result.Nodes.OrderBy(Function(n) n.FirstAppearanceIndex).ToArray()','ExcelCommon.Chart34.SpecialtyLegendPolicy34.SankeyNodeOrder(result.Nodes)')
    s=s.replace('legendItems.OrderBy(Function(x) x.FirstAppearanceIndex).ToList()','ExcelCommon.Chart34.SpecialtyLegendPolicy34.SankeyLegendOrder(legendItems.Select(Function(x) x.FirstAppearanceIndex).ToArray()).Select(Function(i) legendItems(i)).ToList()')
    write('BESHstatNG/src/Graphics/SankeyPlotExcel.vb',s)
    s=src('CumulativeDistributionPlotExcel');h,b=header_body(method(s,'RegisterLegendSeries'))
    s=replace(s,'RegisterLegendSeries',h+'''
        If Not appearance.ShowLegend Then Return
        Dim intent = ExcelCommon.Chart34.SpecialtyLegendPolicy34.CdfLegendChoice(True, appearance.LegendMode = CumulativeDistributionLegendMode.AllCurves, empiricalSeries IsNot Nothing, fittedSeries IsNot Nothing)
        If intent.RenameRepresentative Then
            If intent.KeepFitted Then
                fittedSeries.Name = ResolveGroupLegendName(result)
                legendSeriesIndices.Add(fittedSeries.PlotOrder)
            ElseIf intent.KeepEmpirical Then
                empiricalSeries.Name = ResolveGroupLegendName(result)
                legendSeriesIndices.Add(empiricalSeries.PlotOrder)
            End If
        Else
            If intent.KeepEmpirical Then legendSeriesIndices.Add(empiricalSeries.PlotOrder)
            If intent.KeepFitted Then legendSeriesIndices.Add(fittedSeries.PlotOrder)
        End If
    End Sub''');write('BESHstatNG/src/Graphics/CumulativeDistributionPlotExcel.vb',s)
    s=src('LadderPlotExcel');h,b=header_body(method(s,'ShouldKeepLegendEntry'))
    s=replace(s,'ShouldKeepLegendEntry',h+'''
        If Not appearance.ShowLegend Then Return False
        Dim intent = ExcelCommon.Chart34.SpecialtyLegendPolicy34.LadderLegendChoice(True, CInt(appearance.ColorMode), observationIndex)
        If intent.RecordGroup Then Return seenLegendGroups.Add(observation.GroupLevelIndex)
        Return intent.Keep
    End Function''');write('BESHstatNG/src/Graphics/LadderPlotExcel.vb',s)
if __name__=='__main__':globals()[sys.argv[1]]()
