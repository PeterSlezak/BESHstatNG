"""Freeze original executable pure source slices BEFORE any production modification.

Only native statements are omitted. Exact source ranges and hashes are recorded.
Captions and grouping execute with the shipping /optioncompare:text setting.
No Excel, mock renderer, or new projection is loaded by this generator.
"""
from pathlib import Path
import hashlib
import json
import re

ROOT = Path(__file__).resolve().parents[2]
HERE = Path(__file__).resolve().parent


def method(text, name):
    match = re.search(r'^\s*(?:Public|Private) Shared (?:Sub|Function) ' + name + r'\(', text, re.M)
    assert match, name
    tail = text[match.start():]
    end = re.search(r'^\s*End (?:Sub|Function)\s*$', tail, re.M)
    assert end
    return tail[:end.end()]


def main():
    out = HERE / 'preedit/Original37.vb'
    assert not out.exists()
    sources = {name: (HERE / ('preedit/' + name + 'PlotExcel.vb.txt')).read_text(encoding='utf-8-sig')
               for name in ['Pca', 'FactorAnalysis', 'CorrespondenceAnalysis', 'ClusterAnalysis']}
    pieces = ['Option Explicit On\nOption Strict On\nOption Compare Text\nImports System\nImports System.Linq\nImports System.Collections.Generic\nImports BESHStatNG\nImports BESHStatNG.Multivariate\nNamespace Frozen37\nPublic Class Original37']
    slices = []

    def add(name, signature, body, source_name, original):
        pieces.append('Public Shared Function ' + name + '(' + signature + ') As Object\n' + body + '\nEnd Function\n')
        slices.append(dict(operation=name, source=source_name + 'PlotExcel.vb', original_sha256=hashlib.sha256(original.encode()).hexdigest(),
                           original=original, qualification='Executed pure source prefix/expression; native operations omitted; phase identified separately.'))

    pca = sources['Pca']
    for name in ['LoadingPlot2D', 'LoadingPlot3D', 'ScorePlot2D', 'ScorePlot3D', 'Biplot', 'ScreePlot']:
        old = method(pca, name)
        start = old.index('If model Is Nothing')
        marker = 'Dim XYZ As New XYZscatter' if '3D' in name else 'AppGlobals.app.Charts.Add()'
        body = old[start:old.index(marker)].replace('Exit Sub', 'Return Nothing')
        body = body.replace('Matrix.GetColumnFrom2Darray', 'ArrayUtilities.GetColumn').replace('As CHARTscale', 'As ChartAxisScaleResult').replace('ChartScaling(', 'ChartAxisScalingCore.Compute(')
        body = body.replace('CoreServices.Errors.LogAndThrow(New ArgumentException("biplot \'scale\' is outside of range [0, 1]"))', 'Throw New ArgumentException("biplot \'scale\' is outside of range [0, 1]")')
        values = {'LoadingPlot2D': 'pc1, pc2, varNames, pct, udAxisX.Min, udAxisX.Max, udAxisX.Scale, udAxisY.Min, udAxisY.Max, udAxisY.Scale',
                  'LoadingPlot3D': 'pc1, pc2, pc3, model.VariableNames, pct',
                  'ScorePlot2D': 'pc1, pc2, scoreLabels, normalizedGroups, pct, udAxisX.Min, udAxisX.Max, udAxisX.Scale, udAxisY.Min, udAxisY.Max, udAxisY.Scale',
                  'ScorePlot3D': 'pc1, pc2, pc3, scoreLabels, normalizedGroups, pct',
                  'Biplot': 'pc1, pc2, Load1, Load2, scoreLabels, normalizedGroups, pct, titl, udAxisX.Min, udAxisX.Max, udAxisX.Scale, udAxisY.Min, udAxisY.Max, udAxisY.Scale',
                  'ScreePlot': 'model.XaxisComponents, pct'}[name]
        signature = 'model As PCA'
        if name == 'Biplot': signature += ', c As Double'
        if name in ['Biplot', 'ScorePlot2D', 'ScorePlot3D']: signature += ', rowLabels() As String, groupLabels() As String'
        add('Pca' + name, signature, body + 'Return New Object(){' + values + '}', 'Pca', old[:old.index(marker)])
    for helper in ['ResolveScoreLabels', 'ResolveGroupLabels']:
        pieces.append(method(pca, helper))
    groups = method(pca, 'AddScoreSeries2D')
    a = groups.index('Dim groupIds As New List(Of String)'); b = groups.index("'First four colors", a)
    c = groups.index('For g As Integer = 0'); d = groups.index('seriesCollection.NewSeries', c)
    block = groups[c:d] + 'result.Add(New Object(){groupIds(g), xs.ToArray(), ys.ToArray(), labels.ToArray()})\nNext\nReturn result.ToArray()'
    body = 'If groupLabels Is Nothing Then Return New Object(){New Object(){ungroupedSeriesName, xValues, yValues, pointLabels}}\n' + groups[a:b] + '\nDim result As New List(Of Object)\n' + block
    add('Groups', 'xValues() As Double, yValues() As Double, pointLabels() As String, groupLabels() As String, ungroupedSeriesName As String', body, 'Pca', groups)
    factor = sources['FactorAnalysis']
    for helper in ['ColumnSumsOfSquares', 'PercentOfTotal', 'TotalVariance']:
        pieces.append(method(factor, helper))
    for name in ['ScreePlot', 'LoadingPlot2D', 'LoadingPlot3D']:
        old = method(factor, name)
        marker = 'Dim XYZ As New XYZscatter' if '3D' in name else 'AppGlobals.app.Charts.Add()'
        body = old[old.index('If model Is Nothing'):old.index(marker)].replace('Exit Sub', 'Return Nothing')
        body = body.replace('Matrix.GetColumnFrom2Darray', 'ArrayUtilities.GetColumn').replace('As CHARTscale', 'As ChartAxisScaleResult').replace('ChartScaling(', 'ChartAxisScalingCore.Compute(')
        values = {'ScreePlot': 'factorAxis, initialPct', 'LoadingPlot2D': 'f1, f2, varNames, factorPct, factorNames, udAxisX.Min, udAxisX.Max, udAxisX.Scale, udAxisY.Min, udAxisY.Max, udAxisY.Scale',
                  'LoadingPlot3D': 'f1, f2, f3, model.VariableNames, factorPct, factorNames'}[name]
        add('Factor' + name, 'model As FactorAnalysis', body + 'Return New Object(){' + values + '}', 'FactorAnalysis', old[:old.index(marker)])
    ca = sources['CorrespondenceAnalysis']
    old = method(ca, 'ContributionPlot')
    body = old[old.index('If model Is Nothing'):old.index('If ws Is Nothing')].replace('Dim figure As Chart', '').replace('Dim seriesID As Integer', '')
    add('CaContribution', 'model As CA, lAxis As Integer, bRow As Boolean', body + 'Return contrib', 'CorrespondenceAnalysis', body)
    old = method(ca, 'CorrespondencePlot')
    body = old[old.index('If model Is Nothing'):old.index('If ws Is Nothing')].replace('Exit Sub', 'Return Nothing').replace('Dim figure As Chart', '').replace('Dim seriesID As Integer', '').replace('As CHARTscale', 'As ChartAxisScaleResult').replace('ChartScaling(', 'ChartAxisScalingCore.Compute(')
    add('CaBounds', 'model As CA', body + 'Return New Object(){udPlotAxisX.Min,udPlotAxisX.Max,udPlotAxisX.Scale,udPlotAxisY.Min,udPlotAxisY.Max,udPlotAxisY.Scale}', 'CorrespondenceAnalysis', body)
    add('CaPoints', 'model As CA, bRow As Boolean', 'Return New Object(){If(bRow,model.RowFactors(0),model.ColFactors(0)), If(bRow,model.RowFactors(1),model.ColFactors(1)), model.PlotLabels(bRow, Nothing)}', 'CorrespondenceAnalysis', old)
    add('CaLabels', 'model As CA, bRow As Boolean', 'Return model.PlotLabels(bRow, vbNewLine)', 'CorrespondenceAnalysis', method(ca, 'ContributionPlot'))
    add('CaCaptions', 'model As CA', 'Dim pct As Double(,) = model.Percents\nReturn New Object(){$"Factor 2 [{ Format$(pct(2, 1), "#0.0#") }%]", $"Factor 1 [{ Format$(pct(1, 1), "#0.0#") }%]"}', 'CorrespondenceAnalysis', old)
    cluster = sources['ClusterAnalysis']
    old = method(cluster, 'CreateExcelChart')
    body = old[old.index('Dim xMin As Double'):old.index('With .Axes(XlAxisType.xlCategory)', old.index('Dim xMin As Double'))]
    add('DendrogramBounds', 'layout As DendrogramLayout', body + 'Return New Object(){xMin-xPad,xMax+xPad,yMin-yPad,yMax+yPad}', 'ClusterAnalysis', body)
    title = method(cluster, 'ApplyDendrogramAxisTitles')
    body = title[title.index('Dim resolvedDistanceTitle'):title.index('Select Case layout.Orientation')]
    add('DendrogramTitles', 'layout As DendrogramLayout, distanceAxisTitle As String', body + 'Return New Object(){resolvedDistanceTitle, layout.GetSuggestedAxisTitle()}', 'ClusterAnalysis', title)
    # Stable formatting expressions are executed separately at their original late caption phase.
    expressions = []
    for family in ['Pca', 'FactorAnalysis']:
        for text in re.findall(r'\$"(?:[^"\n]|"#0\.0#")*"', sources[family]):
            if 'Format$' in text and text not in expressions: expressions.append(text)
    body = 'Return New Object(){' + ',\n'.join(expressions) + '}'
    add('Captions', 'pct() As Double, factorPct() As Double, factorNames() As String', body, 'Pca', '\n'.join(expressions))
    pieces.append('End Class\nEnd Namespace\n')
    out.write_text('\n'.join(pieces), encoding='utf-8', newline='\n')
    (HERE / 'preedit/source-slices.json').write_text(json.dumps(slices, ensure_ascii=False, indent=2) + '\n', encoding='utf-8', newline='\n')
    print('Frozen', len(slices), 'executable operations; original sources retained; no production edit')


if __name__ == '__main__':
    main()
