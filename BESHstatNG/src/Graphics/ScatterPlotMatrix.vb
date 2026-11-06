Option Explicit On
Imports Microsoft.Office.Interop.Excel

Namespace graphics


    ''' <summary>
    ''' Creates a scatter‑plot matrix (SPLOM) for multivariate numeric data,
    ''' displaying all pairwise relationships between variables in a single
    ''' composite Excel chart.
    ''' 
    ''' Features:
    ''' <list type="bullet">
    '''   <item><description>p × p grid of scatterplots for p variables</description></item>
    '''   <item><description>Automatic **panel scaling** to normalized coordinates</description></item>
    '''   <item><description>Optional display of **Pearson correlation coefficients**</description></item>
    '''   <item><description>Optional **simple linear regression lines** in each off‑diagonal panel</description></item>
    '''   <item><description>Variable names displayed along the diagonal</description></item>
    '''   <item><description>Grid lines separating panels</description></item>
    ''' </list>
    ''' 
    ''' External dependencies:
    ''' <list type="bullet">
    '''   <item><description><c>GetColumnFrom2Darray</c> — extract variable vectors</description></item>
    '''   <item><description><c>CorrelMatrix</c> — compute Pearson correlation matrix</description></item>
    '''   <item><description><c>Slope</c>, <c>Intercept</c> — regression helpers</description></item>
    '''   <item><description><c>ChartScaling</c> — axis scaling for panel layout</description></item>
    '''   <item><description>Excel interop (<c>Workbook</c>, <c>Chart</c>, <c>SeriesCollection</c>)</description></item>
    ''' </list>
    ''' </summary>
    Public Class ScatterPlotMatrix

        ''' <summary>Internal constant used to hide unwanted regression‑line segments.</summary>
        Private Const gToDeleteGridLineValue As Integer = -100

        ''' <summary>Workbook where the scatter‑plot matrix will be created.</summary>
        Private wb As Workbook

        ''' <summary>Input data matrix (n × p).</summary>
        Private pData(,) As Double

        ''' <summary>Variable names for labeling panels.</summary>
        Private pVarNames() As String

        ''' <summary>Number of observations.</summary>
        Private n As Integer

        ''' <summary>Number of variables.</summary>
        Private p As Integer

        ''' <summary>Margin fraction inside each panel (0–1).</summary>
        Private pMargin As Double

        ''' <summary>Scaled data mapped into panel coordinates.</summary>
        Private ScaledData(,) As Double

        ''' <summary>X‑coordinates for variable‑name labels.</summary>
        Private x_var_labels() As Double

        ''' <summary>Pearson correlation matrix.</summary>
        Private CorrMatrix(,) As Double

        ''' <summary>Coordinates for correlation‑coefficient labels.</summary>
        Private x_corr_labels() As Double
        Private y_corr_labels() As Double

        ''' <summary>Coordinates for regression‑line segments.</summary>
        Private x_regline() As Double
        Private y_regline() As Double

        ''' <summary>Flags controlling optional features.</summary>
        Private pbDisplayCorrCoef As Boolean
        Private pbDisplayRegLines As Boolean


        ''' <summary>
        ''' Initializes the scatter‑plot matrix with the given data and variable names.
        ''' </summary>
        ''' <param name="arData">Numeric matrix (n × p).</param>
        ''' <param name="varNames">Variable names for labeling.</param>
        ''' <param name="wb">Workbook where the chart will be created.</param>
        ''' <param name="dMargin">Optional panel margin (default 0.1).</param>
        Sub New(arData(,) As Double, varNames() As String, wb As Workbook, Optional dMargin As Double = 0.1)

            pData = arData
            n = UBound(pData, 1) + 1
            p = UBound(pData, 2) + 1

            pVarNames = varNames
            pMargin = dMargin
            Me.wb = wb

            pMargin = 0.1 'fraction of 1
            pbDisplayCorrCoef = True
            pbDisplayRegLines = True
        End Sub

        ''' <summary>
        ''' Sets optional display flags for:
        ''' <list type="bullet">
        '''   <item><description>Correlation coefficients</description></item>
        '''   <item><description>Regression lines</description></item>
        ''' </list>
        ''' </summary>
        ''' <param name="bDisplayCorrCoef">If <c>True</c>, displays correlation coefficients.</param>
        ''' <param name="bDisplayRegLines">If <c>True</c>, displays regression lines.</param>
        Sub settingInputs(bDisplayCorrCoef As Boolean, bDisplayRegLines As Boolean)
            pbDisplayCorrCoef = bDisplayCorrCoef
            pbDisplayRegLines = bDisplayRegLines
        End Sub

        ''' <summary>
        ''' Constructs the full scatter‑plot matrix in Excel.
        ''' 
        ''' Steps:
        ''' <list type="number">
        '''   <item><description>Transforms raw data into **panel‑scaled coordinates**.</description></item>
        '''   <item><description>Creates a blank XY scatter chart.</description></item>
        '''   <item><description>Draws horizontal and vertical grid lines to form p × p panels.</description></item>
        '''   <item><description>Plots each pairwise scatterplot (i ≠ j).</description></item>
        '''   <item><description>Optionally overlays **regression lines** in each panel.</description></item>
        '''   <item><description>Places variable names along the diagonal.</description></item>
        '''   <item><description>Optionally displays **correlation coefficients** in each panel.</description></item>
        ''' </list>
        ''' </summary>
        ''' <param name="figure">Optional existing chart to draw into; otherwise a new chart is created.</param>
        Sub compute(Optional figure As Chart = Nothing)

            Dim seriesID As Integer, ii As Integer, tmp1() As Double, tmp2() As Double

            If figure Is Nothing Then
                wb.Charts.Add()
                figure = wb.ActiveChart
            End If

            Me.transformData()

            With figure
                Try
                    .ChartTitle.Delete()
                    .HasLegend = False
                Catch
                End Try

                .ChartType = XlChartType.xlXYScatter
                Try
                    .Axes(XlAxisType.xlCategory).MinimumScale = 0
                    .Axes(XlAxisType.xlCategory).MaximumScale = 1
                    '.Axes(XlAxisType.xlCategory).ReversePlotOrder = True
                    .Axes(XlAxisType.xlCategory).Delete
                    .Axes(XlAxisType.xlValue).MinimumScale = 0
                    .Axes(XlAxisType.xlValue).MaximumScale = 1
                    .Axes(XlAxisType.xlValue).Delete
                    .Axes(XlAxisType.xlValue).MajorGridlines.Delete
                Catch
                End Try

                .PlotArea.Border.LineStyle = XlLineStyle.xlContinuous

                'delete extra series
                Do Until .SeriesCollection.Count = 0
                    .SeriesCollection(1).Delete
                Loop

                'Horizontal Grids
                For i = 1 To p - 1
                    seriesID += 1
                    .SeriesCollection.NewSeries
                    With .SeriesCollection(seriesID)
                        .ChartType = XlChartType.xlXYScatterLinesNoMarkers
                        .XValues = {0, 1}
                        .Values = {ScatterMatrixSeriesCore.GridPosition(i, p), ScatterMatrixSeriesCore.GridPosition(i, p)}
                        .Name = "HorizontalGrid" & CStr(i)
                        .Format.Line.Weight = 1
                        .Format.Line.Visible = True
                        .Format.Line.ForeColor.RGB = RGB(100, 100, 100)
                    End With
                Next

                'Vertical Grids
                For i = 1 To p - 1
                    seriesID += 1
                    .SeriesCollection.NewSeries
                    With .SeriesCollection(seriesID)
                        .ChartType = XlChartType.xlXYScatterLinesNoMarkers
                        .XValues = {ScatterMatrixSeriesCore.GridPosition(i, p), ScatterMatrixSeriesCore.GridPosition(i, p)}
                        .Values = {0, 1}
                        .Name = "VerticalGrid" & CStr(i)
                        .Format.Line.Weight = 1
                        .Format.Line.Visible = True
                        .Format.Line.ForeColor.RGB = RGB(100, 100, 100)
                    End With
                Next

                'Plot Data
                For i = 0 To p - 1
                    For j = 0 To p - 1
                        If i <> j Then
                            tmp1 = ScatterMatrixSeriesCore.PanelValues(ScaledData, i)
                            tmp2 = ScatterMatrixSeriesCore.PanelValues(ScaledData, j)

                            seriesID += 1
                            .SeriesCollection.NewSeries
                            With .SeriesCollection(seriesID)
                                .ChartType = XlChartType.xlXYScatter
                                .XValues = tmp1
                                .Values = tmp2
                                .Name = pVarNames(i) & " vs " & pVarNames(j)
                                .MarkerStyle = 8
                                .MarkerSize = 4
                                .MarkerForegroundColor = RGB(200, 0, 0)
                                .MarkerBackgroundColor = RGB(200, 0, 0)
                                .Format.Fill.Visible = True
                            End With
                        End If
                    Next j
                Next i

                'display regression lines
                If pbDisplayRegLines Then
                    seriesID += 1
                    .SeriesCollection.NewSeries
                    With .SeriesCollection(seriesID)
                        .ChartType = XlChartType.xlXYScatterLinesNoMarkers
                        .XValues = x_regline
                        .Values = y_regline
                        .Name = "Regression Lines"
                        .Format.Line.Weight = 1.5
                        .Format.Line.Visible = True
                        .Format.Line.ForeColor.RGB = RGB(0, 0, 100)

                        'Make redundant line segments invisible
                        For i = 0 To UBound(x_regline)
                            If x_regline(i) = gToDeleteGridLineValue Then
                                .points(i + 1).Format.Line.Visible = False
                                If i + 1 <= UBound(x_regline) Then
                                    .points(i + 2).Format.Line.Visible = False
                                    i += 1
                                End If
                            End If
                        Next
                    End With
                End If

                'Plot Variable names
                seriesID += 1
                .SeriesCollection.NewSeries
                With .SeriesCollection(seriesID)
                    .ChartType = XlChartType.xlXYScatter
                    .XValues = x_var_labels
                    .Values = x_var_labels
                    .Name = "Variabe Names"
                    .MarkerStyle = XlMarkerStyle.xlMarkerStyleNone
                    .Format.Fill.Visible = False

                    .HasDataLabels = True
                    'adjust to suit
                    With .DataLabels
                        .Position = XlDataLabelPosition.xlLabelPositionCenter
                        .AutoScaleFont = False
                        .Font.Size = 14
                        .Font.Bold = True
                        .Font.ColorIndex = RGB(0, 0, 0)
                    End With
                    For i = 1 To p
                        .points(i).DataLabel.text = pVarNames(i - 1)
                    Next
                End With

                'Plot correlation coefficient values
                If pbDisplayCorrCoef Then
                    seriesID += 1
                    .SeriesCollection.NewSeries
                    With .SeriesCollection(seriesID)
                        .ChartType = XlChartType.xlXYScatter
                        .XValues = x_corr_labels
                        .Values = y_corr_labels
                        .Name = "Correlation Coefficient"
                        .MarkerStyle = XlMarkerStyle.xlMarkerStyleNone
                        .Format.Fill.Visible = False
                        .HasDataLabels = True

                        With .DataLabels
                            .Position = XlDataLabelPosition.xlLabelPositionRight
                            .AutoScaleFont = False
                            .Font.Size = 8
                            .Font.Bold = True
                            .Font.ColorIndex = RGB(0, 0, 0)
                        End With
                        ii = 1
                        For i = 1 To p
                            For j = 1 To p
                                If i <> j Then
                                    If i > j Then
                                        .points(ii).DataLabel.text = "p = " & Format$(CorrMatrix(i - 1, j - 1), "0.0000")
                                    ElseIf i < j Then
                                        .points(ii).DataLabel.text = "r = " & Format$(CorrMatrix(i - 1, j - 1), "0.00")
                                    End If
                                Else
                                    .points(ii).HasDataLabel = False
                                End If
                                ii += 1
                            Next
                        Next i
                    End With
                End If
                Try
                    .Legend.Delete()
                Catch
                End Try
            End With
        End Sub

        ''' <summary>
        ''' Transforms raw data into panel‑scaled coordinates and prepares all
        ''' auxiliary arrays required for plotting:
        ''' <list type="bullet">
        '''   <item><description>ScaledData — each variable scaled into its panel</description></item>
        '''   <item><description>x_var_labels — diagonal label positions</description></item>
        '''   <item><description>CorrMatrix — Pearson correlations (if enabled)</description></item>
        '''   <item><description>x_corr_labels, y_corr_labels — correlation label coordinates</description></item>
        '''   <item><description>x_regline, y_regline — regression‑line segments (if enabled)</description></item>
        ''' </list>
        ''' 
        ''' Scaling method:
        ''' <para>
        ''' Each variable is linearly mapped into the interval  
        ''' <c>[panel_left + margin, panel_right − margin]</c>  
        ''' inside its corresponding panel.
        ''' </para>
        ''' 
        ''' Regression lines:
        ''' <para>
        ''' For each pair (i, j), computes  
        ''' <c>y = slope × x + intercept</c>  
        ''' using scaled coordinates.
        ''' </para>
        ''' </summary>
        Sub transformData()
            Dim tmp() As Double, tmp2() As Double

            ReDim ScaledData(n - 1, p - 1), x_corr_labels(p * p - 1), y_corr_labels(p * p - 1), x_var_labels(p - 1)
            Dim intercepts_scaled(p - 1, p - 1) As Double, slopes_scaled(p - 1, p - 1) As Double
            ReDim x_regline((p * (p - 1) * 3) - 1), y_regline((p * (p - 1) * 3) - 1)

            Dim normalized = ScatterMatrixDataCore.Normalize(pData, pMargin)
            ScaledData = normalized.Values
            x_var_labels = normalized.DiagonalLabelX

            If pbDisplayCorrCoef Then CorrMatrix = ScatterMatrixSeriesCore.Correlations(pData)
            Dim coordinates = ScatterMatrixSeriesCore.PanelCoordinates(ScaledData, pMargin, pbDisplayRegLines)
            x_corr_labels = coordinates.CorrelationX : y_corr_labels = coordinates.CorrelationY
            x_regline = coordinates.RegressionX : y_regline = coordinates.RegressionY
        End Sub

    End Class
End Namespace
