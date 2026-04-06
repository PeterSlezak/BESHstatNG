Option Explicit On
Imports System.Drawing
Imports Microsoft.Office
Imports Microsoft.Office.Interop.Excel

Namespace graphics

    ''' <summary>
    ''' Implements a full box‑and‑whisker plot engine for grouped numeric data,
    ''' including:
    ''' <list type="bullet">
    '''   <item><description>Quartile computation (Q1, Median, Q3)</description></item>
    '''   <item><description>Tukey outlier detection (1.5 × IQR rule)</description></item>
    '''   <item><description>Upper and lower whisker computation</description></item>
    '''   <item><description>Group‑wise descriptive statistics via <c>DescriptiveStat</c></description></item>
    '''   <item><description>Excel box‑and‑whisker chart generation using the
    '''     Peltier stacked‑column technique</description></item>
    '''   <item><description>Tabular summary output</description></item>
    ''' </list>
    ''' 
    ''' The class accepts either:
    ''' <list type="bullet">
    '''   <item><description>A jagged array <c>Double()()</c> (one array per group)</description></item>
    '''   <item><description>A 2D matrix <c>Double(,)</c> where columns represent groups</description></item>
    ''' </list>
    ''' 
    ''' External dependencies:
    ''' <list type="bullet">
    '''   <item><description><c>DescriptiveStat</c> — computes quartiles, IQR, mean, min, max</description></item>
    '''   <item><description><c>GetColumnFrom2Darray</c> — extracts group vectors</description></item>
    '''   <item><description><c>ChartScaling</c> — determines axis limits</description></item>
    '''   <item><description>Excel interop (<c>Worksheet</c>, <c>Chart</c>, <c>SeriesCollection</c>)</description></item>
    ''' </list>
    ''' </summary>
    Public Class BoxPlot
        ''' <summary>Grouped input data (each group is a separate array).</summary>
        Private pData()() As Double

        ''' <summary>Sample sizes for each group.</summary>
        Private pNs() As Long

        ''' <summary>Group labels.</summary>
        Private pGroupNames() As String

        ''' <summary>Total number of groups.</summary>
        Private pNoGroups As Long

        ''' <summary>Collection of <see cref="DescriptiveStat"/> objects (one per group).</summary>
        Private pDSCollection As New Collection

        ''' <summary>MatrixType of large outlier values (above Q3 + 1.5·IQR).</summary>
        Private pArOutliersBig(,) As Double

        ''' <summary>MatrixType of small outlier values (below Q1 − 1.5·IQR).</summary>
        Private pArOutliersSmall(,) As Double

        ''' <summary>Number of large outliers per group.</summary>
        Private pArNOutliersBig() As Long

        ''' <summary>Number of small outliers per group.</summary>
        Private pArNOutliersSmall() As Long

        ''' <summary>Arrays used to construct stacked‑column boxplot segments.</summary>
        Private pPlotBlank() As Double
        Private pPlotMedian() As Double
        Private pPlotQ3() As Double
        Private pPlotMedianMinus() As Double
        Private pPlotQ3Minus() As Double

        ''' <summary>Whisker lengths above Q3 and below Q1.</summary>
        Private pWhiskerQ3() As Double
        Private pWhiskerQ1() As Double

        ''' <summary>Quartiles for each group.</summary>
        Private pQ3() As Double
        Private pQ1() As Double

        ''' <summary>Group means.</summary>
        Private pPlotMeans() As Double

        ''' <summary>Group minima and maxima.</summary>
        Private pMins() As Double
        Private pMaxs() As Double

        ''' <summary>Worksheet and workbook for plotting.</summary>
        Private pWS As Worksheet
        Private pWB As Workbook

        ''' <summary>Y‑axis label for the plot.</summary>
        Private pYName As String

        ''' <summary>Group medians.</summary>
        Private pMedians() As Double

        ''' <summary>
        ''' Initializes the boxplot object using a jagged array of groups.
        ''' </summary>
        ''' <param name="arrData">Array of groups, each containing numeric observations.</param>
        ''' <param name="varNames">Group labels.</param>
        ''' <param name="ws">Optional worksheet for plotting.</param>
        ''' <param name="strYname">Optional Y‑axis label.</param>
        Sub New(arrData()() As Double, varNames() As String, Optional ws As Worksheet = Nothing, Optional strYname As String = "")
            pData = arrData
            pNoGroups = arrData.Length
            ReDim pNs(pNoGroups - 1)
            For i = 0 To pNoGroups - 1
                pNs(i) = arrData(i).Length
            Next
            pGroupNames = varNames
            pWS = ws
            pYName = strYname
        End Sub

        ''' <summary>
        ''' Initializes the boxplot object using a 2D matrix where columns represent groups.
        ''' </summary>
        ''' <param name="arrData">2D matrix of observations.</param>
        ''' <param name="varNames">Group labels.</param>
        ''' <param name="ws">Optional worksheet for plotting.</param>
        ''' <param name="strYname">Optional Y‑axis label.</param>
        Sub New(arrData(,) As Double, varNames() As String, Optional ws As Worksheet = Nothing, Optional strYname As String = "")
            pNoGroups = UBound(arrData, 2) + 1
            ReDim pNs(pNoGroups - 1)
            pData = New Double(pNoGroups - 1)() {}
            For i = 0 To pNoGroups - 1
                pNs(i) = UBound(arrData, 1) + 1
                pData(i) = Matrix.GetColumnFrom2Darray(arrData, i)
            Next
            pGroupNames = varNames
            pWS = ws
            If Me.pWS IsNot Nothing Then pWB = ws.Parent
            pYName = strYname
        End Sub

        ''' <summary>
        ''' Assigns the worksheet used for plotting and stores its parent workbook.
        ''' </summary>
        Public WriteOnly Property SetWs() As Worksheet
            Set(ws As Worksheet)
                pWS = ws
                pWB = ws.Parent
            End Set
        End Property

        ''' <summary>Returns Q1 for each group.</summary>
        Public ReadOnly Property Q1 As Double()
            Get
                Q1 = pQ1
            End Get
        End Property

        ''' <summary>Returns medians for each group.</summary>
        Public ReadOnly Property Medians As Double()
            Get
                Medians = pMedians
            End Get
        End Property

        ''' <summary>Returns Q3 for each group.</summary>
        Public ReadOnly Property Q3 As Double()
            Get
                Q3 = pQ3
            End Get
        End Property

        ''' <summary>Returns number of large outliers per group.</summary>
        Public ReadOnly Property BigOutliers As Long()
            Get
                BigOutliers = pArNOutliersBig
            End Get
        End Property

        ''' <summary>Returns number of small outliers per group.</summary>
        Public ReadOnly Property SmallOutliers As Long()
            Get
                SmallOutliers = pArNOutliersSmall
            End Get
        End Property

        ''' <summary>
        ''' Creates a box‑and‑whisker plot in Excel using the Peltier stacked‑column
        ''' technique.  
        ''' 
        ''' The plot includes:
        ''' <list type="bullet">
        '''   <item><description>Box (Q1–Median–Q3)</description></item>
        '''   <item><description>Upper and lower whiskers</description></item>
        '''   <item><description>Means and medians</description></item>
        '''   <item><description>Small and large outliers (each as separate series)</description></item>
        ''' </list>
        ''' 
        ''' Axis scaling is computed using <c>ChartScaling</c>.
        ''' </summary>
        Sub AddBoxPlot()
            Dim i As Long, j As Long, ii As Long
            Dim udBoxPlotAxis As CHARTscale 'for axis scaling

            'compute optimal axis borders and scale
            udBoxPlotAxis = ChartScaling(pMins.Min(), pMaxs.Max())

            With Me.pWS.Shapes.AddChart
                With .Chart
                    .ChartType = XlChartType.xlColumnStacked

                    Do Until .SeriesCollection.Count = 0
                        .SeriesCollection(1).Delete
                    Loop

                    'Plot Blank
                    .SeriesCollection.NewSeries
                    With .SeriesCollection(1)
                        .Values = pPlotBlank
                        .XValues = pGroupNames 'Group names i.e. xaxes categories labels
                        .Name = "PlotBlanks"
                        .Format.Fill.Visible = False ' MsoTriState.msoFalse
                    End With

                    'Plot Median
                    .SeriesCollection.NewSeries
                    With .SeriesCollection(2)
                        .Values = pPlotMedian
                        .Name = "PlotMedian"
                        With .Format.Fill
                            .Visible = True 'app.MsoTriState.msoTrue
                            .ForeColor.RGB = RGB(192, 192, 192)
                        End With
                    End With

                    'Plot Q3
                    .SeriesCollection.NewSeries
                    With .SeriesCollection(3)
                        .Values = pPlotQ3
                        .Name = "PlotQ3"
                        With .Format.Fill
                            .Visible = True 'MsoTriState.msoTrue
                            .ForeColor.RGB = RGB(192, 192, 192)
                        End With
                    End With

                    'Plot Q3 Minus
                    .SeriesCollection.NewSeries
                    With .SeriesCollection(4)
                        .Values = pPlotQ3Minus
                        .Name = "PlotQ3Minus"
                        With .Format.Fill
                            .Visible = True 'MsoTriState.msoTrue
                            .ForeColor.RGB = RGB(192, 192, 192)
                        End With
                    End With

                    'Plot Median Minus
                    .SeriesCollection.NewSeries
                    With .SeriesCollection(5)
                        .Values = pPlotMedianMinus
                        .Name = "PlotMedianMinus"
                        With .Format.Fill
                            .Visible = True 'MsoTriState.msoTrue
                            .ForeColor.RGB = RGB(192, 192, 192)
                        End With
                    End With

                    'Q3 and Upper Whisker
                    .SeriesCollection.NewSeries
                    With .SeriesCollection(6)
                        .Values = pQ3
                        .Name = "Q3"
                        .ChartType = XlChartType.xlLine
                        .Format.Line.Visible = False 'MsoTriState.msoFalse
                        .HasErrorBars = True
                        .ErrorBar(Direction:=XlErrorBarDirection.xlY, Include:=Constants.xlPlusValues, Type:=XlErrorBarType.xlErrorBarTypeCustom, amount:=pWhiskerQ3)
                        .ErrorBars.Format.Line.Weight = 1.5
                    End With

                    'Q1 and Lower Whisker
                    .SeriesCollection.NewSeries
                    With .SeriesCollection(7)
                        .Values = pQ1
                        .Name = "Q1"
                        .ChartType = XlChartType.xlLine
                        .Format.Line.Visible = False 'MsoTriState.msoFalse
                        .HasErrorBars = True
                        .ErrorBar(Direction:=XlErrorBarDirection.xlY, Include:=Constants.xlMinusValues, Type:=XlErrorBarType.xlErrorBarTypeCustom, amount:=-0, MinusValues:=pWhiskerQ1)
                        .ErrorBars.Format.Line.Weight = 1.5
                    End With

                    'Mean
                    .SeriesCollection.NewSeries
                    With .SeriesCollection(8)
                        .Values = pPlotMeans
                        .Name = "Means"
                        .ChartType = XlChartType.xlLine
                        .Format.Line.Visible = False 'MsoTriState.msoFalse
                        .MarkerStyle = XlMarkerStyle.xlMarkerStyleDiamond
                        .MarkerSize = 5
                        .MarkerBackgroundColor = RGB(255, 255, 255)
                        .MarkerForegroundColor = RGB(0, 0, 0)
                    End With

                    'Median
                    .SeriesCollection.NewSeries
                    With .SeriesCollection(9)
                        .Values = pMedians
                        .Name = "Medians"
                        .ChartType = XlChartType.xlXYScatter
                        .Format.Line.Visible = False 'MsoTriState.msoFalse
                        .MarkerStyle = -4142 'no marker
                        .HasErrorBars = True
                        .ErrorBar(Direction:=XlErrorBarDirection.xlX, Include:=Constants.xlBoth, Type:=XlErrorBarType.xlErrorBarTypeFixedValue, amount:=0.2)
                        .ErrorBar(Direction:=XlErrorBarDirection.xlY, Include:=Constants.xlBoth, Type:=XlErrorBarType.xlErrorBarTypeFixedValue, amount:=0) 'don't show Y error bar
                        With .ErrorBars
                            .EndStyle = XlEndStyleCap.xlNoCap
                            .Format.Line.Weight = 1
                        End With
                    End With

                    .Legend.Delete()
                    '.SetElement(MsoChartElementType.msoElementChartTitleAboveChart)
                    .HasTitle = False
                    .HasTitle = True
                    .ChartTitle.Text = "Box and Whiskers plot"
                    '.SetElement(MsoChartElementType.msoElementPrimaryValueAxisTitleRotated)
                    If Me.pYName <> String.Empty Then
                        .Axes(XlAxisType.xlValue, XlAxisGroup.xlPrimary).HasTitle = False
                        .Axes(XlAxisType.xlValue, XlAxisGroup.xlPrimary).HasTitle = True
                        .Axes(XlAxisType.xlValue, XlAxisGroup.xlPrimary).AxisTitle.text = pYName
                    End If
                    .Axes(XlAxisType.xlValue).CrossesAt = -1.0E+50 'if there are negative values then move the axis intercept down

                    With .Axes(XlAxisType.xlValue)
                        .MinimumScale = udBoxPlotAxis.Min
                        .MaximumScale = udBoxPlotAxis.Max
                        .MajorUnit = udBoxPlotAxis.Scale
                        .MajorGridlines.Delete
                    End With

                    'add outliers. Each outlier point is a separate series
                    ii = 10 'seriescollection number
                    For i = 0 To pNoGroups - 1
                        If pArNOutliersSmall(i) > 0 Then
                            For j = 0 To pArNOutliersSmall(i) - 1
                                .SeriesCollection.NewSeries
                                With .SeriesCollection(ii)
                                    .Values = pArOutliersSmall(j, i)
                                    .ChartType = XlChartType.xlXYScatter
                                    .XValues = i + 1
                                    With .points(1)
                                        .MarkerStyle = 8
                                        .MarkerSize = 4
                                        .Format.Fill.Visible = False 'MsoTriState.msoFalse
                                        With .Format.Line
                                            .Visible = True 'MsoTriState.msoTrue
                                            .ForeColor.RGB = RGB(255, 0, 0)
                                            .Weight = 1.25
                                        End With
                                    End With
                                End With
                                ii += 1
                            Next
                        End If
                        If pArNOutliersBig(i) > 0 Then
                            For j = 0 To pArNOutliersBig(i) - 1
                                .SeriesCollection.NewSeries
                                With .SeriesCollection(ii)
                                    .Values = pArOutliersBig(j, i)
                                    .ChartType = XlChartType.xlXYScatter
                                    .XValues = i + 1
                                    With .points(1)
                                        .MarkerStyle = 8
                                        .MarkerSize = 4
                                        .Format.Fill.Visible = False 'MsoTriState.msoFalse
                                        With .Format.Line
                                            .Visible = True 'MsoTriState.msoTrue
                                            .ForeColor.RGB = RGB(255, 0, 0)
                                            .Weight = 1.25
                                        End With
                                    End With
                                End With
                                ii += 1
                            Next
                        End If
                    Next i
                End With
            End With

        End Sub

        ''' <summary>
        ''' Produces a summary table containing:
        ''' <list type="bullet">
        '''   <item><description>Group name</description></item>
        '''   <item><description>Q1</description></item>
        '''   <item><description>Median</description></item>
        '''   <item><description>Q3</description></item>
        '''   <item><description>Number of small outliers</description></item>
        '''   <item><description>Number of large outliers</description></item>
        ''' </list>
        ''' </summary>
        ''' <returns>A <see cref="ResultTable"/> summarizing boxplot statistics.</returns>
        Public Function wrapResults() As ResultTable
            Dim t = New ResultTable, tmp(,) As Object

            ReDim tmp(Me.pNoGroups - 1, 5)
            For i = 0 To Me.pNoGroups - 1
                tmp(i, 0) = Me.pGroupNames(i)
                tmp(i, 1) = Me.pQ1(i)
                tmp(i, 2) = Me.pMedians(i)
                tmp(i, 3) = Me.pQ3(i)
                tmp(i, 4) = Me.pArNOutliersSmall(i)
                tmp(i, 5) = Me.pArNOutliersBig(i)
            Next
            t.SetBody(tmp)
            t.AddHeaderTopRow({"Groups", "Q1", "Median", "Q3", "Outliers small", "Outliers big"})

            Return t
        End Function

        ''' <summary>
        ''' Computes descriptive statistics for each group using <c>DescriptiveStat</c>,
        ''' and identifies outliers using Tukey’s 1.5 × IQR rule:
        ''' <code>
        ''' Small outliers:  x .lt. Q1 − 1.5·IQR
        ''' Large outliers:  x > Q3 + 1.5·IQR
        ''' </code>
        ''' 
        ''' Results are stored in:
        ''' <list type="bullet">
        '''   <item><description><c>pArOutliersSmall</c>, <c>pArNOutliersSmall</c></description></item>
        '''   <item><description><c>pArOutliersBig</c>, <c>pArNOutliersBig</c></description></item>
        '''   <item><description><c>pDSCollection</c></description></item>
        ''' </list>
        ''' </summary>
        Sub Calculate()
            'redim outliars arrays
            ReDim pArOutliersBig(pNs.Max() / 2, pNoGroups - 1)
            ReDim pArOutliersSmall(pNs.Max() / 2, pNoGroups - 1)
            ReDim pArNOutliersBig(pNoGroups - 1), pArNOutliersSmall(pNoGroups - 1)

            'Compute Descpriptive Statistics
            For i = 0 To pNoGroups - 1
                Dim arTemporal() As Double = pData(i)

                'Fit quantiles and outliers for each group
                Dim DescriptiveS As DescriptiveStat = New DescriptiveStat(arTemporal)
                DescriptiveS.compute(False)
                pDSCollection.Add(DescriptiveS) 'Add to Collection

                'outliers
                With DescriptiveS
                    For j = 0 To pNs(i) - 1
                        'small outliers
                        If arTemporal(j) < (.LQuartile - 1.5 * .IQR) Then
                            pArNOutliersSmall(i) += 1
                            pArOutliersSmall(pArNOutliersSmall(i) - 1, i) = arTemporal(j)
                        End If
                        'big outliers
                        If arTemporal(j) > (.UQuartile + 1.5 * .IQR) Then
                            pArNOutliersBig(i) += 1
                            pArOutliersBig(pArNOutliersBig(i) - 1, i) = arTemporal(j)
                        End If
                    Next
                End With
            Next i 'next group

        End Sub

        ''' <summary>
        ''' Computes all stacked‑column components required for Excel box‑and‑whisker
        ''' plotting using the Peltier method:
        ''' <list type="bullet">
        '''   <item><description>Blank offset</description></item>
        '''   <item><description>Median height</description></item>
        '''   <item><description>Q3 height</description></item>
        '''   <item><description>Negative segments for values below zero</description></item>
        '''   <item><description>Upper and lower whisker lengths</description></item>
        '''   <item><description>Means, minima, maxima</description></item>
        ''' </list>
        ''' 
        ''' Uses results from <c>Fit()</c> and <c>DescriptiveStat</c>.
        ''' </summary>
        Sub CalcForPlotting()
            'Fit values for plotting (use values generated by Fit sub and modify them
            'COMPUTE VALUES FOR NEW BOX and WHISKERS CHARTS ACCORDING TO JOHN PELTIER
            'http://peltiertech.com/excel-box-and-whisker-diagrams-box-plots/

            Dim j As Long

            ReDim pPlotBlank(pNoGroups - 1), pPlotMedian(pNoGroups - 1), pPlotQ3(pNoGroups - 1)
            ReDim pPlotQ3Minus(pNoGroups - 1), pPlotMedianMinus(pNoGroups - 1), pWhiskerQ3(pNoGroups - 1)
            ReDim pWhiskerQ1(pNoGroups - 1), pQ1(pNoGroups - 1), pQ3(pNoGroups - 1), pPlotMeans(pNoGroups - 1)
            ReDim pMins(pNoGroups - 1), pMaxs(pNoGroups - 1), pMedians(pNoGroups - 1)

            For i = 0 To pNoGroups - 1

                With Me.pDSCollection.Item(i + 1) 'DescriptiveS
                    'Plot Blank

                    If .LQuartile > 0 Then
                        pPlotBlank(i) = .LQuartile
                    Else
                        pPlotBlank(i) = If(.UQuartile < 0, .UQuartile, 0)
                    End If

                    'Plot Median
                    If .Median > 0 Then
                        If .LQuartile > 0 Then
                            pPlotMedian(i) = If(.Median > .LQuartile, .Median - .LQuartile, 0)
                        Else
                            pPlotMedian(i) = .Median
                        End If
                    Else
                        pPlotMedian(i) = 0
                    End If

                    'Plot Q3
                    If .UQuartile > 0 Then
                        If .Median > 0 Then
                            pPlotQ3(i) = If(.UQuartile > .Median, .UQuartile - .Median, 0)
                        Else
                            pPlotQ3(i) = .UQuartile
                        End If
                    Else
                        pPlotQ3(i) = 0
                    End If

                    'Plot Q3 Minus
                    If .Median < 0 Then
                        If .UQuartile < 0 Then
                            pPlotQ3Minus(i) = If(.Median < .UQuartile, .Median - .UQuartile, 0)
                        Else
                            pPlotQ3Minus(i) = .Median
                        End If
                    Else
                        pPlotQ3Minus(i) = 0
                    End If

                    'Plot Median Minus
                    If .LQuartile < 0 Then
                        If .Median < 0 Then
                            pPlotMedianMinus(i) = If(.LQuartile < .Median, .LQuartile - .Median, 0)
                        Else
                            pPlotMedianMinus(i) = .LQuartile
                        End If
                    Else
                        pPlotMedianMinus(i) = 0
                    End If

                    Dim arTemporal() As Double = pData(i)
                    Array.Sort(arTemporal)

                    If .Maximum > .UQuartile Then
                        If pArNOutliersBig(i) = 0 Then
                            pWhiskerQ3(i) = .Maximum - .UQuartile
                        Else 'show outliers
                            For j = pNs(i) - 1 To pNs(i) / 2 - 1 Step -1
                                If (arTemporal(j) - (.UQuartile + 1.5 * .IQR)) <= 0 Then Exit For
                            Next
                            pWhiskerQ3(i) = arTemporal(j) - .UQuartile
                        End If
                    Else
                        pWhiskerQ3(i) = 0
                    End If

                    'Whisker Q1
                    If .LQuartile > .Minimum Then
                        If pArNOutliersSmall(i) = 0 Then
                            pWhiskerQ1(i) = .LQuartile - .Minimum
                        Else 'show outliers
                            For j = 0 To pNs(i) / 2 - 1
                                If (arTemporal(j) - (.LQuartile - 1.5 * .IQR)) >= 0 Then Exit For
                            Next
                            pWhiskerQ1(i) = .LQuartile - arTemporal(j)
                        End If
                    Else
                        pWhiskerQ1(i) = 0
                    End If

                    pQ1(i) = .LQuartile
                    pQ3(i) = .UQuartile
                    pPlotMeans(i) = .Mean
                    pMins(i) = .Minimum
                    pMaxs(i) = .Maximum
                    pMedians(i) = .Median
                End With
            Next i
        End Sub
    End Class

End Namespace
