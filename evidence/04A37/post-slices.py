"""Compile the shipping adapter's CURRENT numerical prefixes, never a parallel algorithm."""
from pathlib import Path
import re
import json
import hashlib
from freeze import method

ROOT = Path(__file__).resolve().parents[2]
HERE = Path(__file__).resolve().parent
OUT = ROOT / '.test-rationalization-04A37.tmp/Current37.vb'


def main():
    pieces = ['Option Explicit On\nOption Strict On\nImports System\nImports System.Linq\nImports BESHStatNG\nImports BESHStatNG.ExcelCommon\nImports BESHStatNG.Multivariate\nNamespace Frozen37\nPublic Class Current37']
    slices = []
    for family, names in [('Pca', ['LoadingPlot2D', 'LoadingPlot3D', 'ScorePlot2D', 'ScorePlot3D', 'Biplot', 'ScreePlot']),
                          ('FactorAnalysis', ['ScreePlot', 'LoadingPlot2D', 'LoadingPlot3D']),
                          ('CorrespondenceAnalysis', ['ContributionPlot', 'CorrespondencePlot'])]:
        path = 'BESHstatNG/src/Graphics/' + family + 'PlotExcel.vb'
        source = (ROOT / path).read_text(encoding='utf-8-sig')
        for name in names:
            original = method(source, name)
            marker = 'Dim XYZ As New XYZscatter' if '3D' in name else 'If ws Is Nothing' if family == 'CorrespondenceAnalysis' else 'AppGlobals.app.Charts.Add()'
            body = original[original.index('If model Is Nothing'):original.index(marker)].replace('Exit Sub', 'Return Nothing')
            body = body.replace('Dim figure As Chart', '').replace('Dim seriesID As Integer', '')
            body = body.replace('CoreServices.Errors.LogAndThrow(New ArgumentException("biplot \'scale\' is outside of range [0, 1]"))', 'Throw New ArgumentException("biplot \'scale\' is outside of range [0, 1]")')
            if family == 'Pca':
                signature = 'model As PCA'
                if name == 'Biplot': signature += ', c As Double'
                if name in ['Biplot', 'ScorePlot2D', 'ScorePlot3D']: signature += ', rowLabels() As String, groupLabels() As String'
                operation = 'Pca' + name
                values = {'LoadingPlot2D': 'pc1,pc2,varNames,pct,udAxisX.Min,udAxisX.Max,udAxisX.Scale,udAxisY.Min,udAxisY.Max,udAxisY.Scale',
                    'LoadingPlot3D': 'pc1,pc2,pc3,model.VariableNames,pct',
                    'ScorePlot2D': 'pc1,pc2,scoreLabels,normalizedGroups,pct,udAxisX.Min,udAxisX.Max,udAxisX.Scale,udAxisY.Min,udAxisY.Max,udAxisY.Scale',
                    'ScorePlot3D': 'pc1,pc2,pc3,scoreLabels,normalizedGroups,pct',
                    'Biplot': 'pc1,pc2,Load1,Load2,scoreLabels,normalizedGroups,pct,titl,udAxisX.Min,udAxisX.Max,udAxisX.Scale,udAxisY.Min,udAxisY.Max,udAxisY.Scale',
                    'ScreePlot': 'model.XaxisComponents,pct'}[name]
            elif family == 'FactorAnalysis':
                signature = 'model As FactorAnalysis'; operation = 'Factor' + name
                values = {'ScreePlot': 'factorAxis,initialPct',
                    'LoadingPlot2D': 'f1,f2,varNames,factorPct,factorNames,udAxisX.Min,udAxisX.Max,udAxisX.Scale,udAxisY.Min,udAxisY.Max,udAxisY.Scale',
                    'LoadingPlot3D': 'f1,f2,f3,model.VariableNames,factorPct,factorNames'}[name]
            else:
                signature = 'model As CA'; operation = 'CaBounds'; values = 'udPlotAxisX.Min,udPlotAxisX.Max,udPlotAxisX.Scale,udPlotAxisY.Min,udPlotAxisY.Max,udPlotAxisY.Scale'
                if name == 'ContributionPlot':
                    operation = 'CaContribution'; signature += ',lAxis As Integer,bRow As Boolean'; values = None
            pieces.append('Public Shared Function ' + operation + '(' + signature + ') As Object\n' + body + ('Return New Object(){' + values + '}' if values else 'Return contrib') + '\nEnd Function')
            slices.append(dict(operation=operation, source=path, actual_prefix_sha256=hashlib.sha256(body.encode()).hexdigest()))
    pieces.append('''
Public Shared Function Groups(xValues() As Double,yValues() As Double,pointLabels() As String,groupLabels() As String,ungroupedSeriesName As String) As Object
Return MultivariateChartPresentation37.ScoreSeries(xValues,yValues,pointLabels,groupLabels,ungroupedSeriesName).Select(Function(s) CType(New Object(){s.Name,s.XValues,s.YValues,s.Labels},Object)).ToArray()
End Function
Public Shared Function CaPoints(model As CA,bRow As Boolean) As Object
Dim s=MultivariateChartPresentation37.CorrespondenceSeries(model,bRow)
Return New Object(){s.XValues,s.YValues,model.PlotLabels(bRow,Nothing)}
End Function
Public Shared Function CaLabels(model As CA,bRow As Boolean) As Object
Return MultivariateChartPresentation37.ContributionLabels(model,bRow)
End Function
Public Shared Function CaCaptions(model As CA) As Object
Dim pct=model.Percents
Return New Object(){MultivariateChartPresentation37.CorrespondenceCaption(pct(2,1),2),MultivariateChartPresentation37.CorrespondenceCaption(pct(1,1),1)}
End Function
Public Shared Function DendrogramBounds(layout As DendrogramLayout) As Object
Dim b=New DendrogramChartGeometry37(layout).Bounds()
Return New Object(){b.XMin,b.XMax,b.YMin,b.YMax}
End Function
Public Shared Function DendrogramTitles(layout As DendrogramLayout,distanceAxisTitle As String) As Object
Dim g As New DendrogramChartGeometry37(layout),p As New DendrogramChartPresentation37(g,DendrogramLabelMode.DataLabels)
Return New Object(){p.DistanceTitle(distanceAxisTitle),g.SuggestedAxisTitle()}
End Function
Public Shared Function Captions(pct() As Double,factorPct() As Double,factorNames() As String) As Object
Return New Object(){MultivariateChartPresentation37.ComponentCaption(pct(0),0,False),MultivariateChartPresentation37.ComponentCaption(pct(1),1,False),MultivariateChartPresentation37.ComponentCaption(pct(2),2,False),MultivariateChartPresentation37.ComponentCaption(pct(0),0,True),MultivariateChartPresentation37.ComponentCaption(pct(2),2,True),MultivariateChartPresentation37.ComponentCaption(pct(0),0,False),MultivariateChartPresentation37.ComponentCaption(pct(1),1,False),MultivariateChartPresentation37.FactorCaption(factorNames(1),factorPct(1)),MultivariateChartPresentation37.FactorCaption(factorNames(0),factorPct(0)),MultivariateChartPresentation37.FactorCaption(factorNames(2),factorPct(2))}
End Function
End Class
End Namespace
''')
    OUT.write_text('\n'.join(pieces), encoding='utf-8', newline='\n')
    print('Prepared current shipping prefixes:', len(slices))


if __name__ == '__main__':
    main()
