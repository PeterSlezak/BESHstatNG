Option Explicit On
Option Strict On
Imports System
Imports System.Linq
Imports BESHStatNG
Imports BESHStatNG.ExcelCommon
Imports BESHStatNG.Multivariate
Namespace Global.Frozen37
Public Class Current37
Public Shared Function PcaLoadingPlot2D(model As PCA) As Object
If model Is Nothing Then Throw New ArgumentNullException(NameOf(model))
            If model.NoExtractComponents < 2 Then Return Nothing

            Dim loadings As Double(,) = model.GetLoadings
            Dim varNames() As String = model.VariableNames
            Dim p As Integer = model.VariableCount
            Dim pct() As Double = model.PercentExpl

            Dim pc1() As Double = MultivariatePlotGeometry37.Column(loadings, 0)
            Dim pc2() As Double = MultivariatePlotGeometry37.Column(loadings, 1)
            Dim axes = MultivariatePlotGeometry37.SymmetricAxes(pc1, pc2)
            Dim udAxisX As ChartAxisScaleResult = axes.X
            Dim udAxisY As ChartAxisScaleResult = axes.Y

            Return New Object(){pc1,pc2,varNames,pct,udAxisX.Min,udAxisX.Max,udAxisX.Scale,udAxisY.Min,udAxisY.Max,udAxisY.Scale}
End Function
Public Shared Function PcaLoadingPlot3D(model As PCA) As Object
If model Is Nothing Then Throw New ArgumentNullException(NameOf(model))
            If model.NoExtractComponents < 3 Then Return Nothing

            Dim loadings As Double(,) = model.GetLoadings
            Dim pc1() As Double = MultivariatePlotGeometry37.Column(loadings, 0)
            Dim pc2() As Double = MultivariatePlotGeometry37.Column(loadings, 1)
            Dim pc3() As Double = MultivariatePlotGeometry37.Column(loadings, 2)
            Dim pct() As Double = model.PercentExpl

            Return New Object(){pc1,pc2,pc3,model.VariableNames,pct}
End Function
Public Shared Function PcaScorePlot2D(model As PCA, rowLabels() As String, groupLabels() As String) As Object
If model Is Nothing Then Throw New ArgumentNullException(NameOf(model))
            If model.NoExtractComponents < 2 Then Return Nothing

            Dim scores As Double(,) = model.ReducedDataset
            Dim n As Integer = model.ObservationCount
            Dim pct() As Double = model.PercentExpl
            Dim scoreLabels() As String = MultivariateChartPresentation37.ScoreLabels(model, rowLabels)
            Dim normalizedGroups() As String = MultivariateChartPresentation37.NormalizeGroups(groupLabels, n)

            Dim pc1() As Double = MultivariatePlotGeometry37.Column(scores, 0)
            Dim pc2() As Double = MultivariatePlotGeometry37.Column(scores, 1)
            Dim axes = MultivariatePlotGeometry37.Axes(pc1, pc2)
            Dim udAxisX As ChartAxisScaleResult = axes.X
            Dim udAxisY As ChartAxisScaleResult = axes.Y

            Return New Object(){pc1,pc2,scoreLabels,normalizedGroups,pct,udAxisX.Min,udAxisX.Max,udAxisX.Scale,udAxisY.Min,udAxisY.Max,udAxisY.Scale}
End Function
Public Shared Function PcaScorePlot3D(model As PCA, rowLabels() As String, groupLabels() As String) As Object
If model Is Nothing Then Throw New ArgumentNullException(NameOf(model))
            If model.NoExtractComponents < 3 Then Return Nothing

            Dim scores As Double(,) = model.ReducedDataset
            Dim pc1() As Double = MultivariatePlotGeometry37.Column(scores, 0)
            Dim pc2() As Double = MultivariatePlotGeometry37.Column(scores, 1)
            Dim pc3() As Double = MultivariatePlotGeometry37.Column(scores, 2)
            Dim pct() As Double = model.PercentExpl
            Dim n As Integer = model.ObservationCount

            Dim scoreLabels() As String = MultivariateChartPresentation37.ScoreLabels(model, rowLabels)
            Dim normalizedGroups() As String = MultivariateChartPresentation37.NormalizeGroups(groupLabels, n)

            Return New Object(){pc1,pc2,pc3,scoreLabels,normalizedGroups,pct}
End Function
Public Shared Function PcaBiplot(model As PCA, c As Double, rowLabels() As String, groupLabels() As String) As Object
If model Is Nothing Then Throw New ArgumentNullException(NameOf(model))
            If model.NoExtractComponents < 2 Then Return Nothing
            If c < 0.0 Or c > 1.0 Then Throw New ArgumentException("biplot 'scale' is outside of range [0, 1]")

            Dim titl As String = MultivariateChartPresentation37.BiplotDescription(c)

            Dim scores As Double(,) = model.ReducedDataset
            Dim loadings As Double(,) = model.GetLoadings
            Dim varNames() As String = model.VariableNames
            Dim eigenvalues() As Double = model.Eigenvalues
            Dim pct() As Double = model.PercentExpl
            Dim n As Integer = model.ObservationCount
            Dim p As Integer = model.VariableCount
            Dim scoreLabels() As String = MultivariateChartPresentation37.ScoreLabels(model, rowLabels)
            Dim normalizedGroups() As String = MultivariateChartPresentation37.NormalizeGroups(groupLabels, n)

            Dim coordinates = MultivariatePlotGeometry37.Biplot(scores, loadings, eigenvalues, n, p, c)
            Dim pc1() As Double = coordinates.ScoreX
            Dim pc2() As Double = coordinates.ScoreY
            Dim Load1() As Double = coordinates.LoadingX
            Dim Load2() As Double = coordinates.LoadingY
            Dim axes = MultivariatePlotGeometry37.CombinedAxes(pc1, pc2, Load1, Load2)
            Dim udAxisX As ChartAxisScaleResult = axes.X
            Dim udAxisY As ChartAxisScaleResult = axes.Y

            Return New Object(){pc1,pc2,Load1,Load2,scoreLabels,normalizedGroups,pct,titl,udAxisX.Min,udAxisX.Max,udAxisX.Scale,udAxisY.Min,udAxisY.Max,udAxisY.Scale}
End Function
Public Shared Function PcaScreePlot(model As PCA) As Object
If model Is Nothing Then Throw New ArgumentNullException(NameOf(model))

            Dim pct() As Double = model.PercentExpl
            Dim p As Integer = model.VariableCount

            Return New Object(){model.XaxisComponents,pct}
End Function
Public Shared Function FactorScreePlot(model As FactorAnalysis) As Object
If model Is Nothing Then Throw New ArgumentNullException(NameOf(model))

            Dim initialEigenvalues() As Double = model.InitialEigenvalues
            If initialEigenvalues Is Nothing OrElse initialEigenvalues.Length = 0 Then Return Nothing

            Dim factorAxis() As Integer = MultivariatePlotGeometry37.ComponentAxis(initialEigenvalues.Length)
            Dim initialPct() As Double = MultivariatePlotGeometry37.InitialFactorPercent(initialEigenvalues, model.WorkingMatrix)

            Return New Object(){factorAxis,initialPct}
End Function
Public Shared Function FactorLoadingPlot2D(model As FactorAnalysis) As Object
If model Is Nothing Then Throw New ArgumentNullException(NameOf(model))
            If model.PatternMatrix Is Nothing OrElse model.NumberOfFactors < 2 Then Return Nothing

            Dim patternMatrix As Double(,) = model.PatternMatrix
            Dim structureMatrix As Double(,) = model.StructureMatrix
            Dim varNames() As String = model.VariableNames
            Dim p As Integer = model.VariableCount
            Dim f1() As Double = MultivariatePlotGeometry37.Column(patternMatrix, 0)
            Dim f2() As Double = MultivariatePlotGeometry37.Column(patternMatrix, 1)
            Dim factorPct() As Double = MultivariatePlotGeometry37.FactorPercent(patternMatrix, structureMatrix, model.WorkingMatrix)
            Dim factorNames() As String = model.FactorNames()

            Dim axes = MultivariatePlotGeometry37.SymmetricAxes(f1, f2)
            Dim udAxisX As ChartAxisScaleResult = axes.X
            Dim udAxisY As ChartAxisScaleResult = axes.Y

            Return New Object(){f1,f2,varNames,factorPct,factorNames,udAxisX.Min,udAxisX.Max,udAxisX.Scale,udAxisY.Min,udAxisY.Max,udAxisY.Scale}
End Function
Public Shared Function FactorLoadingPlot3D(model As FactorAnalysis) As Object
If model Is Nothing Then Throw New ArgumentNullException(NameOf(model))
            If model.PatternMatrix Is Nothing OrElse model.NumberOfFactors < 3 Then Return Nothing

            Dim patternMatrix As Double(,) = model.PatternMatrix
            Dim structureMatrix As Double(,) = model.StructureMatrix
            Dim factorPct() As Double = MultivariatePlotGeometry37.FactorPercent(patternMatrix, structureMatrix, model.WorkingMatrix)
            Dim factorNames() As String = model.FactorNames()
            Dim f1() As Double = MultivariatePlotGeometry37.Column(patternMatrix, 0)
            Dim f2() As Double = MultivariatePlotGeometry37.Column(patternMatrix, 1)
            Dim f3() As Double = MultivariatePlotGeometry37.Column(patternMatrix, 2)

            Return New Object(){f1,f2,f3,model.VariableNames,factorPct,factorNames}
End Function
Public Shared Function CaContribution(model As CA,lAxis As Integer,bRow As Boolean) As Object
If model Is Nothing Then Throw New ArgumentNullException(NameOf(model))

            
            
            Dim contrib() As Double

            contrib = MultivariatePlotGeometry37.Contribution(model, lAxis, bRow)

            Return contrib
End Function
Public Shared Function CaBounds(model As CA) As Object
If model Is Nothing Then Throw New ArgumentNullException(NameOf(model))
            If model.FactorCount < 2 Then Return Nothing

            
            

            Dim axes = MultivariatePlotGeometry37.CorrespondenceAxes(model)
            Dim udPlotAxisY As ChartAxisScaleResult = axes.Y
            Dim udPlotAxisX As ChartAxisScaleResult = axes.X

            Return New Object(){udPlotAxisX.Min,udPlotAxisX.Max,udPlotAxisX.Scale,udPlotAxisY.Min,udPlotAxisY.Max,udPlotAxisY.Scale}
End Function

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
