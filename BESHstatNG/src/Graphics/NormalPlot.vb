Option Explicit On
Imports System.Security.Cryptography
Imports System.Security.Cryptography.X509Certificates
Imports Microsoft.Office.Interop.Excel

Namespace graphics


    ''' <summary>
    ''' Implements a normal probability plot (Q–Q plot) for assessing normality of a
    ''' univariate dataset.  
    ''' 
    ''' The class supports:
    ''' <list type="bullet">
    '''   <item><description>Computation of normal scores using Blom, Rankit, or Van der Waerden formulas</description></item>
    '''   <item><description>Reference‑line estimation using SPSS, OLS, or R‑style quartile matching</description></item>
    '''   <item><description>Excel scatter‑plot generation with optional reference line</description></item>
    ''' </list>
    ''' 
    ''' External dependencies:
    ''' <list type="bullet">
    '''   <item><description><c>ComputeAvgRanks</c> — rank computation with ties</description></item>
    '''   <item><description><c>NormSInv</c>, <c>QNorm</c> — inverse normal CDF</description></item>
    '''   <item><description><c>Slope</c>, <c>Intercept</c> — linear regression helpers</description></item>
    '''   <item><description><c>QuartilesComp</c> — quartile computation</description></item>
    '''   <item><description><c>GeneralScatterPlot</c> — Excel scatter‑plot generator</description></item>
    ''' </list>
    ''' </summary>
    Public Class NormalPlot
        ''' <summary>Sorted input data.</summary>
        Private pData() As Double

        ''' <summary>Name of the variable being analyzed.</summary>
        Private pvarName As String

        ''' <summary>Number of observations.</summary>
        Private n As Integer

        ''' <summary>Computed normal scores (Z‑values).</summary>
        Private pZ() As Double

        ''' <summary>Computed plotting positions (probabilities).</summary>
        Private pP() As Double

        ''' <summary>Reference line coordinates (2×2 matrix: Xmin/Xmax, Ymin/Ymax).</summary>
        Private pRefLine(1, 1) As Double

        ''' <summary>Normal score method used (Blom, Rankit, Van der Waerden).</summary>
        Private pstrNormalScores As String

        ''' <summary>
        ''' Initializes the normal plot object by sorting the data and storing the
        ''' variable name.
        ''' </summary>
        ''' <param name="arrData">Numeric data to be analyzed.</param>
        ''' <param name="varName">Name of the variable.</param>
        Sub New(arrData() As Double, varName As String)
            Array.Sort(arrData)
            pData = arrData
            Me.pvarName = varName
            n = pData.Length
        End Sub

        ''' <summary>
        ''' Computes normal scores (expected Z‑values) using one of three methods:
        ''' <list type="bullet">
        '''   <item><description><c>Blom</c>: (r − 0.375) / (n + 0.25)</description></item>
        '''   <item><description><c>Rankit</c>: (r − 0.5) / n</description></item>
        '''   <item><description><c>Van der Waerden</c>: r / (n + 1)</description></item>
        ''' </list>
        ''' where <c>r</c> is the average rank of each observation.
        ''' 
        ''' The resulting probabilities are transformed to Z‑scores using
        ''' <c>NormSInv</c>.
        ''' </summary>
        ''' <param name="strMethod">Normal score method: Blom, Rankit, or Van Der Waerden.</param>
        ''' <returns>An array of Z‑scores for the normal probability plot.</returns>
        Public Function compute(strMethod As String) As Double()
            pstrNormalScores = strMethod

            ReDim pZ(n - 1), pP(n - 1)
            Dim ranks = nonparametric.ComputeAvgRanks(pData)

            If pstrNormalScores = "Blom" Then
                For i = 0 To n - 1
                    pP(i) = (ranks(i) - 0.375) / (n + 0.25)
                    pZ(i) = distributions.NormSInv(pP(i))
                Next
            ElseIf pstrNormalScores = "Rankit" Then
                For i = 0 To n - 1
                    pP(i) = (ranks(i) - 0.5) / n
                    pZ(i) = distributions.NormSInv(pP(i))
                Next
            ElseIf pstrNormalScores = "Van Der Waerden" Then
                For i = 0 To n - 1
                    pP(i) = ranks(i) / (n + 1)
                    pZ(i) = distributions.NormSInv(pP(i))
                Next
            End If

            Return pZ
        End Function

        ''' <summary>
        ''' Computes the reference line for the normal probability plot using one of
        ''' three methods:
        ''' 
        ''' <list type="bullet">
        '''   <item><description><c>SPSS</c> — matches min/max Z‑scores to corresponding
        '''     theoretical quantiles using sample mean and SD.</description></item>
        '''   <item><description><c>OLS</c> — ordinary least squares regression of
        '''     <c>pData</c> on <c>pZ</c>.</description></item>
        '''   <item><description><c>R</c> — uses first and third quartiles of data and
        '''     corresponding theoretical Z‑scores.</description></item>
        ''' </list>
        ''' 
        ''' The method returns a 2×2 matrix:
        ''' <code>
        ''' [ (Xmin, Ymin),
        '''   (Xmax, Ymax) ]
        ''' </code>
        ''' defining the endpoints of the reference line.
        ''' </summary>
        ''' <param name="strMethod">Reference‑line method: SPSS, OLS, or R.</param>
        ''' <returns>A 2×2 matrix containing X and Y coordinates of the line endpoints.</returns>
        Public Function computeRefLIne(strMethod As String) As Double(,)
            Dim dSlope As Double, dIntercept As Double

            If strMethod = "SPSS" Then
                '1st calculate two points on line
                Dim dMean As Double = pData.Average()
                Dim dSD As Double = stDev(pData)

                pRefLine(0, 0) = distributions.QNorm(pP.Min(), dMean, dSD)
                pRefLine(1, 0) = distributions.QNorm(pP.Max(), dMean, dSD)
                pRefLine(0, 1) = pZ.Min()
                pRefLine(1, 1) = pZ.Max()

                'calculate parameters of the reference line
                'and coordinate for the 1st and last X value coordinate that will define reference line in plot
                dSlope = Slope(Matrix.GetColumnFrom2Darray(pRefLine, 1), Matrix.GetColumnFrom2Darray(pRefLine, 0))
                dIntercept = Intercept(Matrix.GetColumnFrom2Darray(pRefLine, 1), Matrix.GetColumnFrom2Darray(pRefLine, 0))
            ElseIf strMethod = "OLS" Then
                dSlope = Slope(pZ, pData)
                dIntercept = Intercept(pZ, pData)
            ElseIf strMethod = "R" Then
                '1st calculate paires of values coresponding to 1st and 3rd quartile
                'center the rank in the middle i.e. .5 if it is fraction or 0
                Dim dRankQ = {(RoundDown((0.25 * (n + 1)), 0) + RoundUp((0.25 * (n + 1)), 0)) / 2,
                              (RoundDown((0.75 * (n + 1)), 0) + RoundUp((0.75 * (n + 1)), 0)) / 2}

                'compute quartiles using user defined function
                Dim Quartiles = QuartilesComp(pData)
                Dim dQ() = {Quartiles.Q1, Quartiles.Q3}
                'according which z-score calculation methods was used, calculate Z to above-calculated ranks
                Dim ZQ() As Double = Nothing
                If pstrNormalScores = "Blom" Then
                    ZQ = {distributions.NormSInv((dRankQ(0) - 0.375) / (n + 0.25)), distributions.NormSInv((dRankQ(1) - 0.375) / (n + 0.25))}
                ElseIf pstrNormalScores = "Rankit" Then
                    ZQ = {distributions.NormSInv((dRankQ(0) - 0.5) / n), distributions.NormSInv((dRankQ(1) - 0.5) / n)}
                ElseIf pstrNormalScores = "Van Der Waerden" Then
                    ZQ = {distributions.NormSInv(dRankQ(0) / (n + 1)), distributions.NormSInv(dRankQ(1) / (n + 1))}
                End If

                dSlope = Slope(ZQ, dQ)
                dIntercept = Intercept(ZQ, dQ)
            End If

            pRefLine(0, 1) = dIntercept + dSlope * pData.Min()
            pRefLine(1, 1) = dIntercept + dSlope * pData.Max()
            pRefLine(0, 0) = pData.Min()
            pRefLine(1, 0) = pData.Max()

            Return pRefLine
        End Function

        ''' <summary>
        ''' Creates a normal probability plot (Q–Q plot) in an Excel worksheet.
        ''' 
        ''' Features:
        ''' <list type="bullet">
        '''   <item><description>Scatter plot of observed data vs. expected Z‑scores</description></item>
        '''   <item><description>Automatic chart title and axis labeling</description></item>
        '''   <item><description>Reference line overlay using <c>pRefLine</c></description></item>
        '''   <item><description>Integration with <c>GeneralScatterPlot</c></description></item>
        ''' </list>
        ''' </summary>
        ''' <param name="ws">Worksheet where the plot will be created.</param>
        Public Sub addChart(ws As Worksheet)

            With GeneralScatterPlot(pData, pZ, "Normal Expected Scores", "Observed: " & Me.pvarName, ws)

                On Error Resume Next
                .Legend.Delete()
                .HasTitle = False
                .HasTitle = True
                .ChartTitle.Text = "Normal Plot"
                On Error GoTo 0

                'add and plot reference line
                'in arReferenceY() and arReferenceX() are stored Y and X coordinate of the largest and smallest value calculated
                'according selected reference line type (SPSS or OLS or R)

                .SeriesCollection.NewSeries
                With .SeriesCollection(2)
                    .XValues = Matrix.GetColumnFrom2Darray(pRefLine, 0)
                    .Values = Matrix.GetColumnFrom2Darray(pRefLine, 1)
                    .Name = "Reference Line"
                    .MarkerStyle = XlMarkerStyle.xlMarkerStyleNone
                    .Border.Color = RGB(255, 0, 0)
                    With .Format.Line
                        .Visible = True
                        .ForeColor.RGB = RGB(255, 0, 0)
                        .Weight = 1.5
                    End With
                End With
            End With
        End Sub

    End Class
End Namespace