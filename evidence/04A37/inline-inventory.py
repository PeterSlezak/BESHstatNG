"""Explicit split/merge map for original inline preparations, without fictitious headers."""
from pathlib import Path
import csv
import hashlib
import json

ROOT=Path(__file__).resolve().parents[2]
HERE=Path(__file__).resolve().parent


def main():
    members=json.loads((HERE/'entry-members.json').read_bytes())['entry']
    specs=[
        ('Pca','LoadingPlot3D','columns-percent','Dim loadings','Dim XYZ','MultivariatePlotGeometry37.Column; existing PCA.PercentExpl','PORTED_AND_CONSUMED','PcaLoadingPlot3D'),
        ('Pca','LoadingPlot2D','columns-symmetric-axes','Dim loadings','AppGlobals.app.Charts.Add','MultivariatePlotGeometry37.Column/SymmetricAxes','PORTED_AND_CONSUMED','PcaLoadingPlot2D'),
        ('Pca','LoadingPlot2D','loading-arrows-labels','For id','Next id','MultivariatePlotGeometry37.LoadingVector; model variable ordinal -> native point2','PORTED_AND_CONSUMED','Graphics37 arrow/IL tests'),
        ('Pca','ScorePlot3D','coordinates-labels-groups','Dim scores','Dim XYZ','MultivariatePlotGeometry37.Column; MultivariateChartPresentation37.ScoreLabels/NormalizeGroups','PORTED_AND_CONSUMED','PcaScorePlot3D'),
        ('Pca','ScorePlot2D','coordinates-labels-groups-axes','Dim scores','AppGlobals.app.Charts.Add','MultivariatePlotGeometry37.Column/Axes; Common label policies','PORTED_AND_CONSUMED','PcaScorePlot2D'),
        ('Pca','Biplot','metric-title','Dim titl','Dim scores','MultivariateChartPresentation37.BiplotDescription','PORTED_AND_CONSUMED','PcaBiplot'),
        ('Pca','Biplot','labels-groups','Dim scoreLabels','Dim pc1','MultivariateChartPresentation37.ScoreLabels/NormalizeGroups','PORTED_AND_CONSUMED','PcaBiplot'),
        ('Pca','Biplot','scaling-and-bounds','Dim pc1','AppGlobals.app.Charts.Add','MultivariatePlotGeometry37.Biplot/CombinedAxes','PORTED_AND_CONSUMED','PcaBiplot'),
        ('Pca','Biplot','loading-arrows-labels','For id','Next id','MultivariatePlotGeometry37.LoadingVector; model variable ordinal -> native point2','PORTED_AND_CONSUMED','Graphics37 arrow/IL tests'),
        ('Pca','ScreePlot','core-percent-profile','Dim pct','AppGlobals.app.Charts.Add','Existing PCA.PercentExpl/VariableCount/XaxisComponents','EXISTING_PORTABLE_REUSED','PcaScreePlot'),
        ('Pca','ScreePlot','late-percent-labels','For i = 0','Next','MultivariateChartPresentation37.PercentLabel; native point i+1','PORTED_AND_CONSUMED','Captions; exceptional-format tests'),
        ('Pca','ResolveScoreLabels','fallback-source-ids','Dim n','Return out','MultivariateChartPresentation37.ScoreLabels; actual model RowIds','PORTED_AND_CONSUMED','PcaScorePlot2D/3D; model identity tests'),
        ('Pca','ResolveGroupLabels','trim-and-missing','If groupLabels Is Nothing','Return out','MultivariateChartPresentation37.NormalizeGroups','PORTED_AND_CONSUMED','PcaScorePlot2D/3D'),
        ('Pca','AddScoreSeries2D','first-occurrence-names','Dim groupIds','Dim colors','MultivariateChartPresentation37.ScoreSeries','PORTED_AND_CONSUMED','Groups'),
        ('Pca','AddScoreSeries2D','culture-membership-coordinate-label-selection','For g As Integer','seriesCollection.NewSeries','MultivariateChartPresentation37.ScoreSeries; explicit source positions/model IDs','PORTED_AND_CONSUMED','Groups; model identity/copy tests'),
        ('Pca','ConfigureScoreLegend','legend-intent','If groupSeriesCount','chart.HasLegend = True','MultivariateChartPresentation37.ShowScoreLegend; native legend application retained','PORTED_AND_CONSUMED','compiled Graphics37 seams/source write order'),
        ('Pca','AddZeroLines','reference-vectors-and-names','seriesCollection.NewSeries',None,'ChartReferenceSeries.Horizontal/Vertical; MultivariatePlotGeometry37.ZeroPair; Common names','PORTED_AND_CONSUMED','Graphics37 reference types; source write order'),
        ('FactorAnalysis','ScreePlot','initial-eigen-trace-profile','Dim factorAxis','AppGlobals.app.Charts.Add','MultivariatePlotGeometry37.ComponentAxis/InitialFactorPercent','PORTED_AND_CONSUMED','FactorScreePlot'),
        ('FactorAnalysis','LoadingPlot2D','pattern-structure-percent-columns-axes','Dim patternMatrix','AppGlobals.app.Charts.Add','MultivariatePlotGeometry37.FactorPercent/Column/SymmetricAxes','PORTED_AND_CONSUMED','FactorLoadingPlot2D'),
        ('FactorAnalysis','LoadingPlot2D','loading-arrows','For id','Next','MultivariatePlotGeometry37.LoadingVector; native labels/styles retained','PORTED_AND_CONSUMED','Graphics37 arrow/IL tests'),
        ('FactorAnalysis','LoadingPlot2D','zero-reference-series','.XValues = {udAxisX.Min, udAxisX.Max}','Try','ChartReferenceSeries.Horizontal/Vertical; MultivariatePlotGeometry37.ZeroPair; Common names','PORTED_AND_CONSUMED','reference type tests; native source order'),
        ('FactorAnalysis','LoadingPlot3D','pattern-structure-percent-columns','Dim patternMatrix','Dim XYZ','MultivariatePlotGeometry37.FactorPercent/Column; Common factor captions','PORTED_AND_CONSUMED','FactorLoadingPlot3D'),
        ('FactorAnalysis','ColumnSumsOfSquares','pattern-times-structure','Dim out','Return out','Folded exactly into MultivariatePlotGeometry37.FactorPercent','PORTED_AND_CONSUMED','FactorLoadingPlot2D/3D'),
        ('FactorAnalysis','PercentOfTotal','trace-percentage','Dim out','Return out','MultivariatePlotGeometry37.PercentOfTotal; consumed through initial/retained helpers','PORTED_AND_CONSUMED','FactorScreePlot/LoadingPlot2D/3D'),
        ('FactorAnalysis','TotalVariance','diagonal-trace','Dim s','Return s','MultivariatePlotGeometry37.TotalVariance; consumed through initial/retained helpers','PORTED_AND_CONSUMED','FactorScreePlot/LoadingPlot2D/3D'),
        ('CorrespondenceAnalysis','ContributionPlot','row-or-column-contribution','If bRow','If ws Is Nothing','MultivariatePlotGeometry37.Contribution','PORTED_AND_CONSUMED','CaContribution'),
        ('CorrespondenceAnalysis','ContributionPlot','category-label-separator','.XValues = model.PlotLabels','.Values = contrib','MultivariateChartPresentation37.ContributionLabels; original CRLF separator','PORTED_AND_CONSUMED','CaLabels'),
        ('CorrespondenceAnalysis','CorrespondencePlot','both-group-axis-extents','Dim udPlotAxisY','If ws Is Nothing','MultivariatePlotGeometry37.CorrespondenceAxes; Y before X including hidden MCA rows','PORTED_AND_CONSUMED','CaBounds'),
        ('CorrespondenceAnalysis','CorrespondencePlot','column-point-series','.XValues = model.ColFactors','.MarkerStyle','MultivariateChartPresentation37.CorrespondenceSeries(False)','PORTED_AND_CONSUMED','CaPoints'),
        ('CorrespondenceAnalysis','CorrespondencePlot','row-point-series','.XValues = model.RowFactors','.MarkerStyle','MultivariateChartPresentation37.CorrespondenceSeries(True)','PORTED_AND_CONSUMED','CaPoints'),
        ('CorrespondenceAnalysis','CorrespondencePlot','late-point-labels','For i As Integer = 1 To model.ColumNames.Length','Next','MultivariateChartPresentation37.CorrespondenceLabel; native one-based point indices','PORTED_AND_CONSUMED','CaLabels/CaPoints; compiled seams'),
        ('CorrespondenceAnalysis','CorrespondencePlot','mca-row-omission','If Not model.IsMultiple','seriesID += 1','MultivariateChartPresentation37.ShowCorrespondenceRows','PORTED_AND_CONSUMED','MCA omission source/compiled checks'),
        ('CorrespondenceAnalysis','CorrespondencePlot','zero-reference-series','.XValues = {udPlotAxisX.Min, udPlotAxisX.Max}','Try','ChartReferenceSeries.Horizontal/Vertical; MultivariatePlotGeometry37.ZeroPair; Common names','PORTED_AND_CONSUMED','reference type tests; native source order'),
        ('CorrespondenceAnalysis','CorrespondencePlot','legacy-percent-indices','Dim pct As Double(,)','.ChartTitle.Font.Bold','MultivariateChartPresentation37.CorrespondenceCaption; original (2,1)/(1,1), late Try/Catch','PORTED_AND_CONSUMED','CaCaptions'),
        ('ClusterAnalysis','CreateDendrogramChart','authoritative-layout-forward','Dim layout','Return CreateExcelChart','Existing HierarchicalClusterResult.CreateDendrogramLayout','EXISTING_PORTABLE_REUSED','fitted series corpus; compiled route tests'),
        ('ClusterAnalysis','CreateExcelChart','pre-creation-validation','If layout Is Nothing','If figure Is Nothing','Retained Windows error policy/validation before native creation','HOST_ONLY','null/layout guards; source order'),
        ('ClusterAnalysis','CreateExcelChart','native-chart-routing','If figure Is Nothing','With figure','Retained Windows supplied/chart-sheet/embedded/anchor/size/native errors','HOST_ONLY','source contract; native NOT_RUN'),
        ('ClusterAnalysis','CreateExcelChart','cluster-series-admission','If layout.ClusterPolylineX','If layout.CutLineX','DendrogramChartPresentation37.Clusters; original cluster slots retained','PORTED_AND_CONSUMED','1536 series rows'),
        ('ClusterAnalysis','CreateExcelChart','cut-series-admission','If layout.CutLineX IsNot Nothing AndAlso layout.CutLineY IsNot Nothing','If labelMode','DendrogramChartPresentation37.CutSeries','PORTED_AND_CONSUMED','1536 series rows'),
        ('ClusterAnalysis','CreateExcelChart','leaf-series-and-source-order','If labelMode','Dim xMin','DendrogramChartPresentation37.LeafSeries; Core LeafOriginalRowOrder; native placement retained','PORTED_AND_CONSUMED','1536 series rows; dropped-row identity test'),
        ('ClusterAnalysis','CreateExcelChart','bounds-padding','Dim xMin','With .Axes(XlAxisType.xlCategory)','DendrogramChartGeometry37.Bounds; existing ClusterAnalysisHelpers.AxisPadding','PORTED_AND_CONSUMED','DendrogramBounds;1536 series rows'),
        ('ClusterAnalysis','CreateExcelChart','native-orientation-ticks','If layout.Orientation = DendrogramOrientation.Left','ApplyDendrogramAxisTitles','Retained Windows axis/tick/label style application from portable Orientation','HOST_ONLY','source write order; native NOT_RUN'),
        ('ClusterAnalysis','ApplyDendrogramAxisTitles','distance-title-fallback','Dim resolvedDistanceTitle','Select Case layout.Orientation','DendrogramChartPresentation37.DistanceTitle; Core existing suggested leaf-axis text','PORTED_AND_CONSUMED','DendrogramTitles; all label/orientation modes')]
    rows=[]
    for family,name,role,start,end,target,disposition,tests in specs:
        n=next(n for n in members if Path(n['path']).stem==family+'PlotExcel' and n['name']==name)
        source=(HERE/('preedit/'+Path(n['path']).name+'.txt')).read_bytes().decode('utf-8-sig')
        assert all(ord(c)<=65535 for c in source),'Source contains surrogate pairs; require explicit UTF16 offset conversion'
        body=source[n['span_start']:n['span_start']+n['span_length']]
        a=body.index(start);b=body.index(end,a+len(start)) if end else len(body)
        fragment=body[a:b]
        parent=n['key']+'@'+str(n['span_start'])+':'+str(n['span_length'])
        rows.append(dict(inline_id=parent+'|inline:'+role,parent_member_id=parent,family=family,member=name,role=role,
            identity_boundary='INLINE_FRAGMENT_NO_STANDALONE_HEADER',path=n['path'],
            source_sha256=hashlib.sha256(source.encode('utf-8')).hexdigest(),fragment_sha256=hashlib.sha256(fragment.encode('utf-8')).hexdigest(),
            start_line=source[:n['span_start']+a].count('\n')+1,end_line=source[:n['span_start']+b].count('\n')+1,
            start_marker=start,end_marker=end or 'member end',disposition=disposition,successor_dependency=target,
            implementation_to_renderer=family+'PlotExcel.'+name+' -> '+target+' -> original native Chart/Series/XYZ application',
            tests_or_capture=tests,native_retained='All native styling/point/axis creation and placement remain host-owned; no visual-equivalence claim'))
    with (HERE/'inline-preparation-map.csv').open('w',encoding='utf-8',newline='') as f:
        w=csv.DictWriter(f,fieldnames=list(rows[0]));w.writeheader();w.writerows(rows)
    print('Mapped',len(rows),'explicit original inline preparation/style fragments; separate from32 member populations')


if __name__=='__main__':main()
