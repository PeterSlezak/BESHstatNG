"""Bounded adapter transformations; numerical authority is the frozen entry source."""
from pathlib import Path
import re
import hashlib

ROOT = Path(__file__).resolve().parents[2]


def member(source, name):
    match = re.search(r'^\s*Private Shared (?:Function|Sub) ' + name + r'\(', source, re.M)
    assert match
    end = re.search(r'^\s*End (Function|Sub)\s*$', source[match.start():], re.M)
    return source[match.start():match.start() + end.end()]


def write(name, text):
    (ROOT / ('BESHstatNG/src/Graphics/' + name + 'PlotExcel.vb')).write_text(text, encoding='utf-8', newline='\n')


def base(name):
    return (ROOT / ('evidence/04A37/preedit/' + name + 'PlotExcel.vb.txt')).read_text(encoding='utf-8-sig').replace(
        'Imports System.Linq', 'Imports System.Linq\nImports BESHStatNG.ExcelCommon')


def arrows_and_references(s):
    s = re.sub(r'\{0, ((?:pc|Load|f)[12]\(id\))\}', r'MultivariatePlotGeometry37.LoadingVector(\1)', s)
    for x in ['axisX', 'udAxisX', 'udPlotAxisX']:
        s = s.replace('{' + x + '.Min, ' + x + '.Max}', 'ChartReferenceSeries.Horizontal(' + x + '.Min, ' + x + '.Max, 0).XValues')
    for y in ['axisY', 'udAxisY', 'udPlotAxisY']:
        s = s.replace('{' + y + '.Min, ' + y + '.Max}', 'ChartReferenceSeries.Vertical(0, ' + y + '.Min, ' + y + '.Max).YValues')
    return s.replace('{0, 0}', 'MultivariatePlotGeometry37.ZeroPair()')


def pca():
    s = base('Pca')
    for name in ['ResolveScoreLabels', 'ResolveGroupLabels']:
        s = s.replace(member(s, name), '')
    s = s.replace('ResolveScoreLabels(', 'MultivariateChartPresentation37.ScoreLabels(').replace('ResolveGroupLabels(', 'MultivariateChartPresentation37.GroupLabels(')
    s = s.replace('Matrix.GetColumnFrom2Darray(', 'MultivariatePlotGeometry37.Column(')
    a = s.index('            Dim scl1 As Double'); b = s.index('            AppGlobals.app.Charts.Add()', a)
    s = s[:a] + '''            Dim axes = MultivariatePlotGeometry37.SymmetricAxes(pc1, pc2)
            Dim udAxisX As ChartAxisScaleResult = axes.X
            Dim udAxisY As ChartAxisScaleResult = axes.Y

''' + s[b:]
    a = s.index('            Dim udAxisX As CHARTscale = ChartScaling(pc1.Min()'); b = s.index('            AppGlobals.app.Charts.Add()', a)
    s = s[:a] + '''            Dim axes = MultivariatePlotGeometry37.Axes(pc1, pc2)
            Dim udAxisX As ChartAxisScaleResult = axes.X
            Dim udAxisY As ChartAxisScaleResult = axes.Y

''' + s[b:]
    a = s.index('            Dim titl As String'); b = s.index('            Dim scores As Double', a)
    s = s[:a] + '            Dim titl As String = MultivariateChartPresentation37.BiplotDescription(c)\n\n' + s[b:]
    a = s.index('            Dim pc1() As Double', s.index('Public Shared Sub Biplot'))
    b = s.index('            AppGlobals.app.Charts.Add()', a)
    s = s[:a] + '''            Dim coordinates = MultivariatePlotGeometry37.Biplot(scores, loadings, eigenvalues, n, p, c)
            Dim pc1() As Double = coordinates.ScoreX
            Dim pc2() As Double = coordinates.ScoreY
            Dim Load1() As Double = coordinates.LoadingX
            Dim Load2() As Double = coordinates.LoadingY
            Dim axes = MultivariatePlotGeometry37.CombinedAxes(pc1, pc2, Load1, Load2)
            Dim udAxisX As ChartAxisScaleResult = axes.X
            Dim udAxisY As ChartAxisScaleResult = axes.Y

''' + s[b:]
    s = re.sub(r'\$"([123](?:st|St|nd|rd|Rd)) Component Scores \[\{\s*Format\$\(pct\(([012])\), "#0.0#"\)\s*\}%\]"',
               lambda m: 'MultivariateChartPresentation37.ComponentCaption(pct(' + m[2] + '), ' + m[2] + ', ' + ('True' if m[1] in ['1St', '3Rd'] else 'False') + ')', s)
    s = s.replace('Format$(pct(i), "#0.0#")', 'MultivariateChartPresentation37.PercentLabel(pct(i))')
    s = s.replace('scale As CHARTscale', 'scale As ChartAxisScaleResult').replace('axisX As CHARTscale', 'axisX As ChartAxisScaleResult').replace('axisY As CHARTscale', 'axisY As ChartAxisScaleResult')
    a = s.index('            Dim groupIds As New List(Of String)'); b = s.index("            'First four colors", a)
    s = s[:a] + '            Dim prepared = MultivariateChartPresentation37.ScoreSeries(xValues, yValues, pointLabels, groupLabels, ungroupedSeriesName)\n\n' + s[b:]
    a = s.index('                Dim xs As New List(Of Double)'); b = s.index('                seriesCollection.NewSeries', a)
    s = s[:a] + '                Dim pointSeries = prepared(g)\n\n' + s[b:]
    s = s.replace('groupIds.Count', 'prepared.Count').replace('xs.ToArray()', 'pointSeries.XValues').replace('ys.ToArray()', 'pointSeries.YValues').replace('groupIds(g)', 'pointSeries.Name').replace('labels.ToArray()', 'pointSeries.Labels')
    # The original ungrouped native style path is unchanged; it also consumes an ordered payload.
    a = s.index('            If groupLabels Is Nothing Then', s.index('Private Shared Function AddScoreSeries2D'))
    s = s[:a] + '            Dim ungrouped = If(groupLabels Is Nothing, MultivariateChartPresentation37.ScoreSeries(xValues, yValues, pointLabels, Nothing, ungroupedSeriesName)(0), Nothing)\n' + s[a:]
    a = s.index('            If groupLabels Is Nothing Then', s.index('Private Shared Function AddScoreSeries2D')); b = s.index('            Dim prepared =', a)
    t = s[a:b].replace('.XValues = xValues', '.XValues = ungrouped.XValues').replace('.Values = yValues', '.Values = ungrouped.YValues').replace('.Name = ungroupedSeriesName', '.Name = ungrouped.Name').replace('AddPointLabels(.points, pointLabels)', 'AddPointLabels(.points, ungrouped.Labels)')
    s = s[:a] + t + s[b:]
    write('Pca', arrows_and_references(s))


def factor():
    s = base('FactorAnalysis')
    for name in ['ColumnSumsOfSquares', 'PercentOfTotal', 'TotalVariance']:
        s = s.replace(member(s, name), '')
    a = s.index('            Dim factorAxis('); b = s.index('            AppGlobals.app.Charts.Add()', a)
    s = s[:a] + '''            Dim factorAxis() As Integer = MultivariatePlotGeometry37.ComponentAxis(initialEigenvalues.Length)
            Dim initialPct() As Double = MultivariatePlotGeometry37.InitialFactorPercent(initialEigenvalues, model.WorkingMatrix)

''' + s[b:]
    s = s.replace('Matrix.GetColumnFrom2Darray(', 'MultivariatePlotGeometry37.Column(')
    s = s.replace('PercentOfTotal(ColumnSumsOfSquares(patternMatrix, structureMatrix), TotalVariance(model.WorkingMatrix))', 'MultivariatePlotGeometry37.FactorPercent(patternMatrix, structureMatrix, model.WorkingMatrix)')
    a = s.index('            Dim scl1 As Double'); b = s.index('            AppGlobals.app.Charts.Add()', a)
    s = s[:a] + '''            Dim axes = MultivariatePlotGeometry37.SymmetricAxes(f1, f2)
            Dim udAxisX As ChartAxisScaleResult = axes.X
            Dim udAxisY As ChartAxisScaleResult = axes.Y

''' + s[b:]
    s = re.sub(r'\$"\{factorNames\(([012])\)\} \[\{Format\$\(factorPct\(\1\), "#0.0#"\)\}%\]"',
               lambda m: 'MultivariateChartPresentation37.FactorCaption(factorNames(' + m[1] + '), factorPct(' + m[1] + '))', s)
    s = s.replace('Format$(initialPct(i), "#0.0#")', 'MultivariateChartPresentation37.PercentLabel(initialPct(i))')
    write('FactorAnalysis', arrows_and_references(s))


def ca():
    s = base('CorrespondenceAnalysis')
    a = s.index('            If bRow Then'); b = s.index('            If ws Is Nothing', a)
    s = s[:a] + '            contrib = MultivariatePlotGeometry37.Contribution(model, lAxis, bRow)\n\n' + s[b:]
    s = s.replace('"Contribution Plot: Axis " & CStr(lAxis + 1)', 'MultivariateChartPresentation37.ContributionTitle(lAxis)')
    s = s.replace('model.PlotLabels(bRow, vbNewLine)', 'MultivariateChartPresentation37.ContributionLabels(model, bRow)')
    a = s.index('            Dim udPlotAxisY As CHARTscale'); b = s.index('            If ws Is Nothing', a)
    s = s[:a] + '''            Dim axes = MultivariatePlotGeometry37.CorrespondenceAxes(model)
            Dim udPlotAxisY As ChartAxisScaleResult = axes.Y
            Dim udPlotAxisX As ChartAxisScaleResult = axes.X

''' + s[b:]
    s = s.replace('model.PlotLabels(False, vbNullString)(i - 1)', 'MultivariateChartPresentation37.CorrespondenceLabel(model, False, i - 1)').replace('model.PlotLabels(True, vbNullString)(i - 1)', 'MultivariateChartPresentation37.CorrespondenceLabel(model, True, i - 1)')
    s = re.sub(r'\$"Factor ([12]) \[\{ Format\$\(pct\(\1, 1\), "#0.0#"\) \}%\]"', lambda m: 'MultivariateChartPresentation37.CorrespondenceCaption(pct(' + m[1] + ', 1), ' + m[1] + ')', s)
    write('CorrespondenceAnalysis', arrows_and_references(s))


def cluster():
    s = base('ClusterAnalysis').replace('Imports BESHStatNG.AppInfrastructure', 'Imports BESHStatNG.ExcelCommon\nImports BESHStatNG.AppInfrastructure')
    a = s.index('            If figure Is Nothing Then')
    s = s[:a] + '''            Dim geometry As New DendrogramChartGeometry37(layout)
            Dim presentation As New DendrogramChartPresentation37(geometry, labelMode)

''' + s[a:]
    a = s.index('                .SeriesCollection.NewSeries()'); b = s.index('                Dim xMin As Double', a)
    t = s[a:b]
    t = '                Dim baseSeries = presentation.BaseSeries()\n' + t
    t = t.replace('.Name = "Dendrogram"', '.Name = baseSeries.Name').replace('.XValues = layout.PolylineX', '.XValues = baseSeries.XValues').replace('.Values = layout.PolylineY', '.Values = baseSeries.YValues')
    c = t.index('                If layout.ClusterPolylineX'); d = t.index('                        Dim clr As Integer', c)
    t = t[:c] + '                For Each clusterSeries In presentation.Clusters()\n                        Dim i As Integer = clusterSeries.SourceOrdinals(0)\n' + t[d:]
    t = t.replace('.Name = $"Cluster {i + 1}"', '.Name = clusterSeries.Name').replace('.XValues = layout.ClusterPolylineX(i)', '.XValues = clusterSeries.XValues').replace('.Values = layout.ClusterPolylineY(i)', '.Values = clusterSeries.YValues')
    t = t.replace('                    Next\n                End If\n\n                If layout.CutLineX', '                Next\n\n                If layout.CutLineX', 1)
    c = t.index('                If layout.CutLineX'); d = t.index('                    .SeriesCollection.NewSeries()', c)
    t = t[:c] + '                Dim cutSeries = presentation.CutSeries()\n                If cutSeries IsNot Nothing Then\n' + t[d:]
    t = t.replace('.Name = "Cut"', '.Name = cutSeries.Name').replace('.XValues = layout.CutLineX', '.XValues = cutSeries.XValues').replace('.Values = layout.CutLineY', '.Values = cutSeries.YValues')
    c = t.index('                If labelMode ='); d = t.index('                    .SeriesCollection.NewSeries()', c)
    t = t[:c] + '                Dim leafSeries = presentation.LeafSeries()\n                If leafSeries IsNot Nothing Then\n' + t[d:]
    t = t.replace('.Name = "LeafLabels"', '.Name = leafSeries.Name').replace('.XValues = layout.LeafX', '.XValues = leafSeries.XValues').replace('.Values = layout.LeafY', '.Values = leafSeries.YValues').replace('layout.LeafLabels', 'leafSeries.Labels').replace('layout.Orientation', 'geometry.Orientation')
    s = s[:a] + t + s[b:]
    a = s.index('                Dim xMin As Double'); b = s.index('                With .Axes(XlAxisType.xlCategory)', a)
    s = s[:a] + '                Dim bounds = geometry.Bounds()\n\n' + s[b:]
    s = s.replace('xMin - xPad', 'bounds.XMin').replace('xMax + xPad', 'bounds.XMax').replace('yMin - yPad', 'bounds.YMin').replace('yMax + yPad', 'bounds.YMax')
    a = s.index('                If layout.Orientation'); b = s.index('        Public Shared Sub WriteObjectTable', a)
    t = s[a:b].replace('layout.Orientation', 'geometry.Orientation')
    t = t.replace('ApplyDendrogramAxisTitles(figure, layout, labelMode, distanceAxisTitle)', 'ApplyDendrogramAxisTitles(figure, presentation, labelMode, distanceAxisTitle)')
    t = t.replace('layout As DendrogramLayout,', 'presentation As DendrogramChartPresentation37,').replace('OrElse layout Is Nothing', 'OrElse presentation Is Nothing')
    c = t.index('            Dim resolvedDistanceTitle'); d = t.index('            Select Case', c)
    t = t[:c] + '            Dim geometry = presentation.Geometry\n            Dim resolvedDistanceTitle As String = presentation.DistanceTitle(distanceAxisTitle)\n\n' + t[d:]
    t = t.replace('layout.GetSuggestedAxisTitle()', 'geometry.SuggestedAxisTitle()')
    s = s[:a] + t + s[b:]
    write('ClusterAnalysis', s)


if __name__ == '__main__':
    # Archived first-pass tool: refuse to overwrite final or unrelated work.
    for name in ['Pca','FactorAnalysis','ClusterAnalysis','CorrespondenceAnalysis']:
        live = ROOT / ('BESHstatNG/src/Graphics/' + name + 'PlotExcel.vb')
        frozen = ROOT / ('evidence/04A37/preedit/' + name + 'PlotExcel.vb.txt')
        assert hashlib.sha256(live.read_bytes()).digest() == hashlib.sha256(frozen.read_bytes()).digest(), 'First-pass tool requires untouched entry sources'
    pca()
    factor()
    ca()
    cluster()
    print('Four shipping adapters delegate preparation; native statements retained at their existing phases')
