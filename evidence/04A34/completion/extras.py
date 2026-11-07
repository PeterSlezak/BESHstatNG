"""Remaining explicit-measurement expressions, frozen before adapter replacement."""
from work import *
F=[]
def add(family,name,member,args,fields,body,returns):
    old=(HERE/'entry-source'/((family+'ChartExcel' if family=='Bullet' else family+'PlotExcel')+'.vb.txt')).read_text(encoding='utf-8')
    original=method(old,member);kind=family+name+'Geometry'
    function='Public Function '+name+'('+', '.join(n+' As '+t for n,t in args)+') As '+kind+'\n'+body+'\nReturn New '+kind+'('+', '.join(returns)+')\nEnd Function\n'
    F.append(dict(family=family,name=name,member=member,args=args,fields=fields,type=kind,function=function,original_member=original,original_sha256=sha(original.encode()),adaptation='Source numerical expressions with explicit scalar measurements; native enums/objects omitted. Exact statement/order and rounding retained.'))
D=lambda *names:[(n,'Double') for n in names]
add('Bullet','ScaleWidth','DrawScale',[('tickCount','Integer')]+D('plotWidth'),D('Width'),'''Dim nominalWidth As Double
If tickCount <= 1 Then
nominalWidth = 48.0R
Else
nominalWidth = Math.Max(24.0R, Math.Min(64.0R, plotWidth / CDbl(tickCount - 1) * 0.92R))
End If''',['nominalWidth'])
add('Bullet','Tick','DrawScale',D('tickValue','scaleMaximum','plotLeft','plotWidth','bandBottom','tickLength'),D('X','Bottom'),'''Dim normalized As Double = If(scaleMaximum <= 0.0R, 0.0R, tickValue / scaleMaximum)
Dim x As Double = BulletMeasuredGeometryCore.MapX(normalized, plotLeft, plotWidth)''',['x','bandBottom + tickLength'])
add('Bullet','ScaleLabel','DrawScale',[('tickIndex','Integer'),('tickCount','Integer')]+D('x','plotLeft','plotWidth','nominalWidth'),D('Left'),'''Dim textLeft As Double
If tickIndex = 0 Then
textLeft = plotLeft
ElseIf tickIndex = tickCount - 1 Then
textLeft = plotLeft + plotWidth - nominalWidth
Else
textLeft = x - nominalWidth / 2.0R
End If
textLeft = Math.Max(plotLeft, Math.Min(plotLeft + plotWidth - nominalWidth, textLeft))''',['textLeft'])
add('Bullet','LeftArea','DrawLeftLabels',D('padding','plotLeft','labelGap'),D('Left','Width'),'Dim labelRight As Double = plotLeft - labelGap',['padding','Math.Max(1.0R, labelRight - padding)'])
add('Bullet','PairedLabels','DrawLeftLabels',D('bandCentreY','mainFontSize','subtitleFontSize'),D('MainHeight','SubtitleHeight','Top'),'''Dim mainHeight As Double = Math.Max(12.0R, mainFontSize * 1.35R)
Dim subtitleHeight As Double = Math.Max(10.0R, subtitleFontSize * 1.35R)
Dim totalHeight As Double = mainHeight + subtitleHeight
Dim top As Double = bandCentreY - totalHeight / 2.0R''',['mainHeight','subtitleHeight','top'])
add('Bullet','SingleLabel','DrawLeftLabels',D('bandCentreY','fontSize'),D('Height','Top'),'''Dim boxHeight As Double = Math.Max(12.0R, fontSize * 1.45R)
Dim top As Double = bandCentreY - boxHeight / 2.0R''',['boxHeight','top'])
add('Bullet','CenteredLabel','DrawActualValueLabel',D('centreY','height'),D('Top'),'', ['centreY - height / 2.0R'])
add('Bullet','Title','DrawTitle',D('chartWidth','padding'),D('Width'),'', ['Math.Max(1.0R, chartWidth - 2.0R * padding)'])
add('Bullet','PaletteIndex','ResolveBandColor',[(n,'Integer') for n in ['rank','rangeCount','paletteLength']],[('Index','Integer')],'''If paletteLength = 1 OrElse rangeCount <= 1 Then Return New BulletPaletteIndexGeometry(0)
Dim fraction As Double = CDbl(rank) / CDbl(rangeCount - 1)
Dim selectionIndex As Integer = CInt(Math.Round(fraction * CDbl(paletteLength - 1), MidpointRounding.AwayFromZero))
selectionIndex = Math.Max(0, Math.Min(paletteLength - 1, selectionIndex))''',['selectionIndex'])
add('Sankey','Title','DrawTitle',D('chartWidth','padding','legendWidth','legendLeft'),D('Width'),'''Dim titleWidth As Double = chartWidth - 2.0R * padding
If legendWidth > 0.0R Then titleWidth = Math.Max(30.0R, legendLeft - padding)''',['titleWidth'])
add('Sankey','StageLabel','DrawStageLabels',[('stageCount','Integer')]+D('plotWidth','stageLeft','stageRight','plotTop','plotHeight'),D('Width','Top'),'''Dim stepWidth As Double = If(stageCount <= 1, plotWidth, (stageRight - stageLeft) / CDbl(stageCount - 1))
Dim labelWidth As Double = Math.Max(50.0R, Math.Min(150.0R, stepWidth * 0.9R))
Dim top As Double = plotTop + plotHeight + 1.0R''',['labelWidth','top'])
add('Sankey','StageLabelLeft','DrawStageLabels',D('centre','width'),D('Left'),'', ['centre - width / 2.0R'])
add('Sankey','LegendItem','DrawLegend',[('itemIndex','Integer'),('rowsPerColumn','Integer')]+D('legendLeft','legendTop','titleOffset','columnWidth','rowHeight','swatchSize'),D('Left','Top','SwatchTop'),'''Dim column As Integer = itemIndex \ rowsPerColumn
Dim row As Integer = itemIndex Mod rowsPerColumn
Dim itemLeft As Double = legendLeft + column * columnWidth
Dim itemTop As Double = legendTop + titleOffset + row * rowHeight
Dim swatchTop As Double = itemTop + (rowHeight - swatchSize) / 2.0R''',['itemLeft','itemTop','swatchTop'])
add('Sankey','LegendText','DrawLegend',D('itemLeft','swatchSize','columnWidth'),D('Left','Width'),'''Dim textLeft As Double = itemLeft + swatchSize + 4.0R
Dim textWidth As Double = Math.Max(20.0R, columnWidth - swatchSize - 6.0R)''',['textLeft','textWidth'])
def freeze():
    assert not (HERE/'pre-edit-extra-geometry.csv').exists()
    save('extra-geometry-spec.json',dict(authority='PRE_EDIT_LEGACY',scientific='NOT_GOLDEN',functions=F))
    text=HEADER+'Namespace Global.Batch34CompletionFixtures\n'
    for family in ['Bullet','Sankey']:
        text+='Public Module Legacy'+family+'Extras34\n'+'\n'.join(r['function'] for r in F if r['family']==family)+'End Module\n'
        text+='\n'.join(dto(r['type'],r['fields']) for r in F if r['family']==family)
    write('BESHStat.Core.Tests/src/Graphics/LegacyCompletionExtras34.vb',text+'End Namespace\n')
    print('Frozen',len(F),'remaining measured expression functions')
def apply():
    assert (HERE/'pre-edit-extra-geometry.csv').exists()
    for family in ['Bullet','Sankey']:
        p='BESHStat.Core/src/Graphics/'+family+'MeasuredGeometryCore.vb';text=(ROOT/p).read_text(encoding='utf-8');module=family+'MeasuredGeometryCore'
        functions='\n'.join(r['function'].replace('Public Function '+r['name']+'(','Public Function '+family+r['name']+'(') for r in F if r['family']==family)
        text=text.replace('Public Module '+module,'Public Module '+module+'\n'+functions)+'\n'+'\n'.join(dto(r['type'],r['fields']) for r in F if r['family']==family)
        write(p,text)
    # Native loops, branch admission and every native statement remain in place.
    s=src('BulletChartExcel')
    def change(old,new):
        nonlocal s
        assert old in s,old;s=s.replace(old,new)
    start=s.index('        Dim nominalWidth As Double',s.index('Private Shared Sub DrawScale'));end=s.index('        For tickIndex',start)
    s=s[:start]+'        Dim nominalWidth As Double = BulletMeasuredGeometryCore.BulletScaleWidth(ticks.Length, layout.PlotWidth).Width\n\n'+s[end:]
    start=s.index('            Dim normalized As Double',s.index('Private Shared Sub DrawScale'));end=s.index('            If appearance.ShowScaleTickMarks',start)
    s=s[:start]+'''            Dim tickGeometry = BulletMeasuredGeometryCore.BulletTick(tickValue, item.ScaleMaximum, layout.PlotLeft, layout.PlotWidth, rowLayout.BandBottom, appearance.ScaleTickLength)
            Dim x As Double = tickGeometry.X

'''+s[end:]
    change('CSng(rowLayout.BandBottom + appearance.ScaleTickLength)','CSng(tickGeometry.Bottom)')
    start=s.index('                Dim textLeft As Double',s.index('Private Shared Sub DrawScale'));end=s.index('                AddTextShape',start)
    s=s[:start]+'''                Dim textLeft As Double = BulletMeasuredGeometryCore.BulletScaleLabel(tickIndex, ticks.Length, x, layout.PlotLeft, layout.PlotWidth, nominalWidth).Left
                Dim alignment As Integer = If(tickIndex = 0, MsoAlignLeft, If(tickIndex = ticks.Length - 1, MsoAlignRight, MsoAlignCenter))

'''+s[end:]
    change('''        Dim labelRight As Double = layout.PlotLeft - appearance.LabelGap
        width = Math.Max(1.0R, labelRight - left)''','''        Dim labelArea = BulletMeasuredGeometryCore.BulletLeftArea(appearance.ChartPadding, layout.PlotLeft, appearance.LabelGap)
        width = labelArea.Width''')
    start=s.index('            Dim mainHeight As Double',s.index('Private Shared Sub DrawLeftLabels'));end=s.index('            AddTextShape',start)
    s=s[:start]+'''            Dim paired = BulletMeasuredGeometryCore.BulletPairedLabels(rowLayout.BandCentreY, appearance.MeasureLabelFontSize, appearance.SubtitleFontSize)
            Dim mainHeight As Double = paired.MainHeight
            Dim subtitleHeight As Double = paired.SubtitleHeight
            Dim top As Double = paired.Top

'''+s[end:]
    start=s.index('            Dim boxHeight As Double',s.index('Private Shared Sub DrawLeftLabels'));end=s.index('            AddTextShape',start)
    s=s[:start]+'''            Dim singleLabel = BulletMeasuredGeometryCore.BulletSingleLabel(rowLayout.BandCentreY, fontSize)
            Dim boxHeight As Double = singleLabel.Height
            Dim top As Double = singleLabel.Top

'''+s[end:]
    change('rowLayout.BandCentreY - appearance.ValueLabelHeight / 2.0R','BulletMeasuredGeometryCore.BulletCenteredLabel(rowLayout.BandCentreY, appearance.ValueLabelHeight).Top')
    change('Math.Max(1.0R, layout.ChartWidth - 2.0R * appearance.ChartPadding)','BulletMeasuredGeometryCore.BulletTitle(layout.ChartWidth, appearance.ChartPadding).Width')
    h,b=header_body(method(s,'ResolveBandColor'))
    s=replace(s,'ResolveBandColor',h+'''\n        Dim palette As Integer() = appearance.BandColorsByDesirability
        Return palette(BulletMeasuredGeometryCore.BulletPaletteIndex(band.DesirabilityRank, item.QualitativeRangeCount, palette.Length).Index)
    End Function''')
    write('BESHstatNG/src/Graphics/BulletChartExcel.vb',s)
    s=src('SankeyPlotExcel')
    start=s.index('        Dim titleWidth As Double',s.index('Private Shared Sub DrawTitle'));end=s.index('        AddTextShape',start)
    s=s[:start]+'        Dim titleWidth As Double = SankeyMeasuredGeometryCore.SankeyTitle(layout.ChartWidth, appearance.ChartPadding, layout.LegendWidth, layout.LegendLeft).Width\n\n'+s[end:]
    start=s.index('        Dim stepWidth As Double',s.index('Private Shared Sub DrawStageLabels'));end=s.index('        For i As Integer',start)
    s=s[:start]+'''        Dim stageLabel = SankeyMeasuredGeometryCore.SankeyStageLabel(result.StageCount, layout.PlotWidth, layout.StageCentreLeft, layout.StageCentreRight, layout.PlotTop, layout.PlotHeight)
        Dim labelWidth As Double = stageLabel.Width
        Dim top As Double = stageLabel.Top

'''+s[end:]
    s=s.replace('stageX(i) - labelWidth / 2.0R','SankeyMeasuredGeometryCore.SankeyStageLabelLeft(stageX(i), labelWidth).Left')
    start=s.index('            Dim column As Integer',s.index('Private Shared Sub DrawLegend'));end=s.index('            Dim swatch As Object',start)
    s=s[:start]+'''            Dim legendItem = SankeyMeasuredGeometryCore.SankeyLegendItem(i, layout.LegendRowsPerColumn, layout.LegendLeft, layout.LegendTop, titleOffset, appearance.LegendColumnWidth, appearance.LegendRowHeight, appearance.LegendSwatchSize)
            Dim itemLeft As Double = legendItem.Left
            Dim itemTop As Double = legendItem.Top
            Dim swatchTop As Double = legendItem.SwatchTop

'''+s[end:]
    start=s.index('            Dim textLeft As Double',s.index('Private Shared Sub DrawLegend'));end=s.index('            AddTextShape',start)
    s=s[:start]+'''            Dim legendText = SankeyMeasuredGeometryCore.SankeyLegendText(itemLeft, appearance.LegendSwatchSize, appearance.LegendColumnWidth)
            Dim textLeft As Double = legendText.Left
            Dim textWidth As Double = legendText.Width
'''+s[end:]
    write('BESHstatNG/src/Graphics/SankeyPlotExcel.vb',s)
    print('Consumed remaining Bullet/Sankey measured scale/label/legend geometry')
if __name__=='__main__':globals()[sys.argv[1]]()
