Option Explicit On
Imports Microsoft.Office.Interop.Excel

Namespace graphics


    ''' <summary>
    ''' Represents axis‑scaling parameters for Excel charts.
    ''' 
    ''' A <see cref="CHARTscale"/> object contains:
    ''' <list type="bullet">
    '''   <item><description><c>Min</c> — lower axis limit</description></item>
    '''   <item><description><c>Max</c> — upper axis limit</description></item>
    '''   <item><description><c>Scale</c> — major tick interval</description></item>
    ''' </list>
    ''' 
    ''' These values are typically computed using <c>ChartScaling</c>, which applies
    ''' the algorithm described in:
    ''' <para>
    ''' Rob Bovey, Dennis Wallentin, Stephen Bullen, John Green,  
    ''' <i>Professional Excel Development</i>, 2nd ed., Addison‑Wesley, 2009, p. 706.
    ''' </para>
    ''' </summary>
    Public Class CHARTscale
        Public Min As Double
        Public Max As Double
        Public Scale As Double
    End Class

    ''' <summary>
    ''' Provides helper functions for statistical charting in Excel, including:
    ''' <list type="bullet">
    '''   <item><description>Color selection for multiple series</description></item>
    '''   <item><description>General scatter‑plot creation</description></item>
    '''   <item><description>Axis scaling using the Bovey–Wallentin–Bullen–Green algorithm</description></item>
    '''   <item><description>Histogram bin computation (Sturges, Doane, Scott, Freedman–Diaconis)</description></item>
    '''   <item><description>Gaussian overlay curve computation</description></item>
    ''' </list>
    ''' 
    ''' These utilities are used throughout your plotting classes:
    ''' <list type="bullet">
    '''   <item><description><c>Histogram</c></description></item>
    '''   <item><description><c>NormalPlot</c></description></item>
    '''   <item><description><c>BoxPlot</c></description></item>
    '''   <item><description><c>Survival_KM_LR</c></description></item>
    ''' </list>
    ''' </summary>
    Module ChartingFunc

        ''' <summary>
        ''' Returns a predefined RGB color for the <paramref name="i"/>‑th series.
        ''' 
        ''' The function provides a palette of 16 distinct colors suitable for
        ''' multi‑series statistical plots.  
        ''' If <paramref name="i"/> exceeds the predefined range, a fallback color is
        ''' generated using <c>Timer * 60</c>.
        ''' </summary>
        ''' <param name="i">Series index (1‑based).</param>
        ''' <returns>An RGB color value.</returns>
        Public Function GetColor(i As Integer) As Integer

            If i = 1 Then
                GetColor = RGB(255, 0, 0)     'red
            ElseIf i = 2 Then
                GetColor = RGB(0, 0, 255)     'blue
            ElseIf i = 3 Then
                GetColor = RGB(0, 255, 0)     'green
            ElseIf i = 4 Then
                GetColor = RGB(255, 0, 255)   'purple
            ElseIf i = 5 Then
                GetColor = RGB(255, 250, 0)   'yellow
            ElseIf i = 6 Then
                GetColor = RGB(0, 250, 255)   'cyan
            ElseIf i = 7 Then
                GetColor = RGB(189, 183, 107) 'darkkhaki
            ElseIf i = 8 Then
                GetColor = RGB(255, 127, 0)   'darkorange
            ElseIf i = 9 Then
                GetColor = RGB(210, 105, 30)  'chocolate
            ElseIf i = 10 Then
                GetColor = RGB(238, 92, 66)   'tomato
            ElseIf i = 11 Then
                GetColor = RGB(0, 0, 128)     'navy
            ElseIf i = 12 Then
                GetColor = RGB(50, 50, 50)    'dark gray
            ElseIf i = 13 Then
                GetColor = RGB(100, 100, 100) 'medium gray
            ElseIf i = 14 Then
                GetColor = RGB(150, 150, 150) 'medium2 gray
            ElseIf i = 15 Then
                GetColor = RGB(200, 200, 200) 'light gray
            ElseIf i = 16 Then
                GetColor = RGB(0, 0, 0)       'black
            Else
                GetColor = Timer * 60
            End If

        End Function

        ''' <summary>
        ''' Creates a general‑purpose XY scatter plot in Excel for paired numeric data.
        ''' 
        ''' Features:
        ''' <list type="bullet">
        '''   <item><description>Automatic axis scaling using <c>ChartScaling</c></description></item>
        '''   <item><description>Single‑series scatter plot with customizable axis titles</description></item>
        '''   <item><description>Gridline removal and clean formatting</description></item>
        '''   <item><description>Used by regression and diagnostic plots (e.g., Theil–Sen, NormalPlot)</description></item>
        ''' </list>
        ''' 
        ''' External dependencies:
        ''' <list type="bullet">
        '''   <item><description><c>ChartScaling</c></description></item>
        '''   <item><description>Excel interop (<c>Worksheet</c>, <c>Chart</c>)</description></item>
        ''' </list>
        ''' </summary>
        ''' <param name="arX">X‑coordinates.</param>
        ''' <param name="arY">Y‑coordinates.</param>
        ''' <param name="strYname">Y‑axis label.</param>
        ''' <param name="strXname">X‑axis label.</param>
        ''' <param name="ws">Worksheet where the chart will be created.</param>
        ''' <returns>An Excel <see cref="Microsoft.Office.Interop.Excel.Chart"/> object.</returns>
        Public Function GeneralScatterPlot(arX() As Double, arY() As Double,
                                           strYname As String, strXname As String, ws As Worksheet,
                                           Optional strTitle As String = "Theil-Sen nonparametric regression plot") As Microsoft.Office.Interop.Excel.Chart
            'sub for ploting nonparametric simple linear regression plot of Theil-Sen regression
            'compute optimal scaling
            Dim udPlotAxis As CHARTscale = ChartScaling(arX.Min(), arX.Max())

            With ws.Shapes.AddChart
                With .Chart
                    .ChartType = XlChartType.xlXYScatter

                    'delete extra series
                    Do Until .SeriesCollection.Count = 0
                        .SeriesCollection(1).Delete
                    Loop

                    .Legend.Delete()
                    .Axes(XlAxisType.xlValue).MajorGridlines.Delete
                    .Axes(XlAxisType.xlCategory).MinimumScale = udPlotAxis.Min
                    .Axes(XlAxisType.xlCategory).MaximumScale = udPlotAxis.Max
                    .Axes(XlAxisType.xlCategory).MajorUnit = udPlotAxis.Scale
                    .Axes(XlAxisType.xlValue).CrossesAt = -1.0E+100
                    .Axes(XlAxisType.xlCategory).CrossesAt = -1.0E+100

                    .SeriesCollection.NewSeries
                    With .SeriesCollection(1)
                        .XValues = arX
                        .Values = arY
                        .Name = "Data"
                        .MarkerStyle = 8
                        .MarkerSize = 5
                        .MarkerForegroundColor = RGB(100, 100, 100)
                        .Format.Fill.Visible = False
                    End With

                    On Error Resume Next
                    .Axes(XlAxisType.xlValue, XlAxisGroup.xlPrimary).HasTitle = False
                    .Axes(XlAxisType.xlValue, XlAxisGroup.xlPrimary).HasTitle = True
                    .Axes(XlAxisType.xlValue, XlAxisGroup.xlPrimary).AxisTitle.text = strYname
                    .Axes(XlAxisType.xlCategory, XlAxisGroup.xlPrimary).HasTitle = False
                    .Axes(XlAxisType.xlCategory, XlAxisGroup.xlPrimary).HasTitle = True
                    .Axes(XlAxisType.xlCategory, XlAxisGroup.xlPrimary).AxisTitle.text = strXname
                    .HasTitle = False
                    .HasTitle = True
                    .ChartTitle.Text = strTitle
                    On Error GoTo 0
                End With
                Return .Chart
            End With

        End Function

        ''' <summary>
        ''' Computes optimal axis limits and major tick spacing for Excel charts using
        ''' the algorithm from:
        ''' <para>
        ''' Rob Bovey et al., <i>Professional Excel Development</i>, 2nd ed., p. 706.
        ''' </para>
        ''' 
        ''' Steps:
        ''' <list type="number">
        '''   <item><description>Ensures <c>dMin</c> and <c>dMax</c> are ordered and non‑degenerate.</description></item>
        '''   <item><description>Expands the range slightly to avoid boundary clipping.</description></item>
        '''   <item><description>Computes the order of magnitude of the data range.</description></item>
        '''   <item><description>Selects a “nice” major unit (0.2, 0.5, 1.0, 2.0 × 10ᵏ).</description></item>
        '''   <item><description>Rounds axis limits to multiples of the major unit.</description></item>
        ''' </list>
        ''' 
        ''' Returns a <see cref="CHARTscale"/> object containing:
        ''' <list type="bullet">
        '''   <item><description><c>Min</c> — lower axis limit</description></item>
        '''   <item><description><c>Max</c> — upper axis limit</description></item>
        '''   <item><description><c>Scale</c> — major tick interval</description></item>
        ''' </list>
        ''' </summary>
        ''' <param name="dMin">Minimum data value.</param>
        ''' <param name="dMax">Maximum data value.</param>
        ''' <returns>A <see cref="CHARTscale"/> object.</returns>
        Function ChartScaling(dMin As Double, dMax As Double) As CHARTscale
            'Function to calculate optimal Chart Axes Scales
            'Taken from:    Rob Bovey, Dennis Wallentin, Stephen Bullen, John Green.:
            '               Professional Excel Development 2nd ed., Addison-Wesley Professional, 2009, page 706
            Dim dPower As Double, dScale As Double
            Dim out = New CHARTscale
            Dim localDMin As Double = dMin
            Dim localDMax As Double = dMax

            'Check if the max and min are the same
            If localDMax = localDMin Then
                dScale = localDMax
                localDMax = localDMax * 1.01
                localDMin = localDMin * 0.99
            End If

            'Check if dMax is bigger than dMin - swap them if not
            If localDMax < localDMin Then
                dScale = localDMax
                localDMax = localDMin
                localDMin = dScale
            End If

            'Make dMax a little bigger and dMin a little smaller
            If localDMax > 0 Then
                localDMax = localDMax + (localDMax - localDMin) * 0.01
            Else
                localDMax = localDMax - (localDMax - localDMin) * 0.01
            End If
            If localDMin > 0 Then
                localDMin = localDMin + (localDMax - localDMin) * 0.01
            Else
                localDMin = localDMin - (localDMax - localDMin) * 0.01
            End If

            'What if they are both 0?
            If localDMax = 0 And localDMin = 0 Then localDMax = 1

            'This bit rounds the maximum and minimum values to reasonable values to chart.
            'Find the range of values covered
            dPower = Math.Log(localDMax - localDMin) / Math.Log(10)
            dScale = 10 ^ (dPower - Int(dPower))

            'Find the scaling factor
            Select Case dScale
                Case 0.0 To 2.5
                    dScale = 0.2
                Case 2.5 To 5.0
                    dScale = 0.5
                Case 5.0 To 7.5
                    dScale = 1.0
                Case Else
                    dScale = 2.0
            End Select

            'Fit the scaling factor (major unit)
            dScale = dScale * 10 ^ Int(dPower)

            'Round the axis values to the nearest scaling factor
            out.Min = dScale * Int(localDMin / dScale)
            out.Max = dScale * (Int(localDMax / dScale) + 1)
            out.Scale = dScale
            Return out
        End Function

        ''' <summary>Preserves the legacy histogram UDF and chart helper API.</summary>
        Public Function HistogramBinsComputation(arInput() As Double, strType As String) As Object(,)
            Return Global.BESHStatNG.HistogramBinningCore.HistogramBinsComputation(arInput, strType)
        End Function

        ''' <summary>Preserves the legacy histogram Gaussian overlay API.</summary>
        Function GaussOverlayComputation(arData() As Double, arBinMidVal() As Double) As Double(,)
            Return Global.BESHStatNG.HistogramBinningCore.GaussOverlayComputation(arData, arBinMidVal)
        End Function

    End Module

End Namespace
