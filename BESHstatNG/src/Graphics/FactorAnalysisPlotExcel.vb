Option Explicit On

Imports System.Linq
Imports BESHStatNG.ExcelCommon
Imports BESHStatNG.AppInfrastructure
Imports Microsoft.Office.Interop.Excel

Namespace graphics

    ''' <summary>
    ''' Excel-specific renderer for factor-analysis plots.
    ''' </summary>
    ''' <remarks>
    ''' This adapter keeps Excel chart creation out of the FactorAnalysis statistical model class.
    ''' The FactorAnalysis class computes eigenvalues, loadings, matrices, and result tables only;
    ''' Excel chart rendering belongs to the Excel-DNA graphics/front-end layer.
    ''' </remarks>
    Public NotInheritable Class FactorAnalysisPlotExcel

        Private Sub New()
        End Sub

        ''' <summary>
        ''' Creates a scree plot of the initial factor-analysis eigenvalue profile.
        ''' </summary>
        Public Shared Sub ScreePlot(model As Multivariate.FactorAnalysis)
            If model Is Nothing Then Throw New ArgumentNullException(NameOf(model))

            Dim initialEigenvalues() As Double = model.InitialEigenvalues
            If initialEigenvalues Is Nothing OrElse initialEigenvalues.Length = 0 Then Exit Sub

            Dim factorAxis() As Integer = MultivariatePlotGeometry37.ComponentAxis(initialEigenvalues.Length)
            Dim initialPct() As Double = MultivariatePlotGeometry37.InitialFactorPercent(initialEigenvalues, model.WorkingMatrix)

            AppGlobals.app.Charts.Add()
            With AppGlobals.app.ActiveWorkbook.ActiveChart
                .Name = MultivariateChartPresentation37.ScreeName
                .ChartType = XlChartType.xlXYScatter

                Do Until .SeriesCollection.Count = 0
                    .SeriesCollection(1).Delete
                Loop

                .SeriesCollection.NewSeries
                With .SeriesCollection(1)
                    .XValues = factorAxis
                    .Values = initialPct
                    .Name = MultivariateChartPresentation37.InitialVarianceSeriesName
                    .Format.Line.Weight = 1.5
                    .MarkerStyle = 8
                    .MarkerSize = 5
                    .Border.Color = RGB(100, 100, 100)
                    .MarkerForegroundColor = RGB(100, 100, 100)
                    .MarkerBackgroundColor = RGB(100, 100, 100)

                    For i As Integer = 0 To initialEigenvalues.Length - 1
                        .Points(i + 1).HasDataLabel = True
                        .Points(i + 1).DataLabel.Text = MultivariateChartPresentation37.PercentLabel(initialPct(i))
                        .Points(i + 1).DataLabel.Position = XlDataLabelPosition.xlLabelPositionAbove
                        .Points(i + 1).DataLabel.Font.Size = 12
                    Next
                End With

                Try
                    .Legend.Delete()
                Catch
                End Try

                .Axes(XlAxisType.xlValue, XlAxisGroup.xlPrimary).HasTitle = False
                .Axes(XlAxisType.xlValue, XlAxisGroup.xlPrimary).HasTitle = True
                .Axes(XlAxisType.xlValue, XlAxisGroup.xlPrimary).AxisTitle.Text = MultivariateChartPresentation37.VarianceAxisTitle
                .Axes(XlAxisType.xlValue, XlAxisGroup.xlPrimary).AxisTitle.Font.Size = 16
                .Axes(XlAxisType.xlValue, XlAxisGroup.xlPrimary).TickLabels.Font.Size = 14
                .Axes(XlAxisType.xlCategory, XlAxisGroup.xlPrimary).HasTitle = False
                .Axes(XlAxisType.xlCategory, XlAxisGroup.xlPrimary).HasTitle = True
                .Axes(XlAxisType.xlCategory, XlAxisGroup.xlPrimary).AxisTitle.Text = MultivariateChartPresentation37.FactorAxisTitle
                .Axes(XlAxisType.xlCategory, XlAxisGroup.xlPrimary).AxisTitle.Font.Size = 16
                .Axes(XlAxisType.xlCategory, XlAxisGroup.xlPrimary).TickLabels.Font.Size = 14
                .HasTitle = False
                .HasTitle = True
                .ChartTitle.Text = MultivariateChartPresentation37.ScreeName
                .ChartTitle.Font.Size = 18
                .ChartTitle.Font.Bold = True
            End With
        End Sub

        ''' <summary>
        ''' Creates a 2D scatter plot of variable loadings on the first two retained factors.
        ''' </summary>
        Public Shared Sub LoadingPlot2D(model As Multivariate.FactorAnalysis)
            If model Is Nothing Then Throw New ArgumentNullException(NameOf(model))
            If model.PatternMatrix Is Nothing OrElse model.NumberOfFactors < 2 Then Exit Sub

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

            AppGlobals.app.Charts.Add()
            With AppGlobals.app.ActiveWorkbook.ActiveChart
                .Name = MultivariateChartPresentation37.FactorLoading2DName
                .ChartType = XlChartType.xlXYScatter

                Do Until .SeriesCollection.Count = 0
                    .SeriesCollection(1).Delete
                Loop

                With .Axes(XlAxisType.xlCategory)
                    .MinimumScale = udAxisX.Min
                    .MaximumScale = udAxisX.Max
                    .MajorUnit = udAxisX.Scale
                    .CrossesAt = -1.0E+100
                    .MajorTickMark = XlTickMark.xlTickMarkOutside
                    .MajorGridlines.Delete
                End With
                With .Axes(XlAxisType.xlValue)
                    .CrossesAt = -1.0E+100
                    .MinimumScale = udAxisY.Min
                    .MaximumScale = udAxisY.Max
                    .MajorUnit = udAxisY.Scale
                    .MajorTickMark = XlTickMark.xlTickMarkOutside
                    .MajorGridlines.Delete
                End With

                Dim seriesId As Integer = 0
                For id As Integer = 0 To p - 1
                    .SeriesCollection.NewSeries
                    seriesId += 1
                    With .SeriesCollection(seriesId)
                        .ChartType = XlChartType.xlXYScatterLinesNoMarkers
                        .XValues = MultivariatePlotGeometry37.LoadingVector(f1(id))
                        .Values = MultivariatePlotGeometry37.LoadingVector(f2(id))
                        .Name = MultivariateChartPresentation37.LoadingSeriesName(id)
                        .Format.Line.Weight = 1
                        .Format.Line.Visible = True
                        .Format.Line.ForeColor.RGB = RGB(0, 0, 150)
                        .Format.Line.EndArrowheadStyle = 2

                        .Points(2).HasDataLabel = True
                        .Points(2).DataLabel.Text = CStr(varNames(id))
                        .Points(2).DataLabel.Position = XlDataLabelPosition.xlLabelPositionAbove
                        .Points(2).DataLabel.Font.Size = 11
                        .Points(2).DataLabel.Font.Color = RGB(0, 0, 150)
                    End With
                Next

                .SeriesCollection.NewSeries
                seriesId += 1
                With .SeriesCollection(seriesId)
                    .XValues = ChartReferenceSeries.Horizontal(udAxisX.Min, udAxisX.Max, 0).XValues
                    .Values = MultivariatePlotGeometry37.ZeroPair()
                    .Name = MultivariateChartPresentation37.YZeroName
                    .MarkerStyle = -4142
                    .Border.Color = RGB(0, 0, 0)
                    With .Format.Line
                        .Visible = True
                        .Weight = 1
                    End With
                End With
                .SeriesCollection.NewSeries
                seriesId += 1
                With .SeriesCollection(seriesId)
                    .XValues = MultivariatePlotGeometry37.ZeroPair()
                    .Values = ChartReferenceSeries.Vertical(0, udAxisY.Min, udAxisY.Max).YValues
                    .Name = MultivariateChartPresentation37.XZeroName
                    .MarkerStyle = -4142
                    .Border.Color = RGB(0, 0, 0)
                    With .Format.Line
                        .Visible = True
                        .Weight = 1
                    End With
                End With

                Try
                    .Legend.Delete()
                Catch
                End Try

                .Axes(XlAxisType.xlValue, XlAxisGroup.xlPrimary).HasTitle = False
                .Axes(XlAxisType.xlValue, XlAxisGroup.xlPrimary).HasTitle = True
                .Axes(XlAxisType.xlValue, XlAxisGroup.xlPrimary).AxisTitle.Text = MultivariateChartPresentation37.FactorCaption(factorNames(1), factorPct(1))
                .Axes(XlAxisType.xlValue, XlAxisGroup.xlPrimary).AxisTitle.Font.Size = 16
                .Axes(XlAxisType.xlValue, XlAxisGroup.xlPrimary).TickLabels.Font.Size = 14
                .Axes(XlAxisType.xlCategory, XlAxisGroup.xlPrimary).HasTitle = False
                .Axes(XlAxisType.xlCategory, XlAxisGroup.xlPrimary).HasTitle = True
                .Axes(XlAxisType.xlCategory, XlAxisGroup.xlPrimary).AxisTitle.Text = MultivariateChartPresentation37.FactorCaption(factorNames(0), factorPct(0))
                .Axes(XlAxisType.xlCategory, XlAxisGroup.xlPrimary).AxisTitle.Font.Size = 16
                .Axes(XlAxisType.xlCategory, XlAxisGroup.xlPrimary).TickLabels.Font.Size = 14
                .HasTitle = False
                .HasTitle = True
                .ChartTitle.Text = MultivariateChartPresentation37.FactorLoadingTitle
                .ChartTitle.Font.Size = 18
                .ChartTitle.Font.Bold = True
            End With
        End Sub

        ''' <summary>
        ''' Creates a 3D scatter plot of variable loadings on the first three retained factors.
        ''' </summary>
        Public Shared Sub LoadingPlot3D(model As Multivariate.FactorAnalysis)
            If model Is Nothing Then Throw New ArgumentNullException(NameOf(model))
            If model.PatternMatrix Is Nothing OrElse model.NumberOfFactors < 3 Then Exit Sub

            Dim patternMatrix As Double(,) = model.PatternMatrix
            Dim structureMatrix As Double(,) = model.StructureMatrix
            Dim factorPct() As Double = MultivariatePlotGeometry37.FactorPercent(patternMatrix, structureMatrix, model.WorkingMatrix)
            Dim factorNames() As String = model.FactorNames()
            Dim f1() As Double = MultivariatePlotGeometry37.Column(patternMatrix, 0)
            Dim f2() As Double = MultivariatePlotGeometry37.Column(patternMatrix, 1)
            Dim f3() As Double = MultivariatePlotGeometry37.Column(patternMatrix, 2)

            Dim XYZ As New XYZscatter
            With XYZ
                .ChartName = MultivariateChartPresentation37.FactorLoading3DName
                .dataInputs(f1, f2, f3)
                .axesLabelInputs(MultivariateChartPresentation37.FactorCaption(factorNames(0), factorPct(0)),
                                 MultivariateChartPresentation37.FactorCaption(factorNames(1), factorPct(1)),
                                 MultivariateChartPresentation37.FactorCaption(factorNames(2), factorPct(2)))
                .showPlanePointInputs(True, True, True, 3, 3, 3)
                .ScaleAxis(False)
                .settingsInputs(True, True, True)
                .SetDataLabels(model.VariableNames)
                .draw()
            End With
        End Sub

    End Class

End Namespace