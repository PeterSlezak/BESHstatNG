Option Explicit On

Imports System
Imports System.Collections.Generic
Imports System.Linq
Imports BESHStatNG.AppInfrastructure

Public Class DescriptiveStat

    Private pData() As Double
    Private pClean As Boolean
    Private pVariableName As String
    Private pValidN As Integer
    Private pMean As Double
    Private pMedian As Double
    Private pSD As Double
    Private pSEM As Double
    Private pVariance As Double
    Private pCoefficientofVariation As Double
    Private pSkewness As Double
    Private pKurtosis As Double
    Private pLQuartile As Double
    Private pUQuartile As Double
    Private pIQR As Double
    Private pMinimum As Double
    Private pMaximum As Double
    Private pRange As Double
    Private pSWstat As Double = Double.NaN
    Private pSWPvalue As Double = Double.NaN
    Public Sub New(x() As Double)
        pData = x
    End Sub

    Public ReadOnly Property LQuartile() As Double
        Get
            Return pLQuartile
        End Get
    End Property

    Public ReadOnly Property UQuartile() As Double
        Get
            Return pUQuartile
        End Get
    End Property

    Public ReadOnly Property Median() As Double
        Get
            Return pMedian
        End Get
    End Property

    Public ReadOnly Property IQR() As Double
        Get
            Return pIQR
        End Get
    End Property

    Public ReadOnly Property Mean() As Double
        Get
            Return pMean
        End Get
    End Property

    Public ReadOnly Property Maximum() As Double
        Get
            Return pMaximum
        End Get
    End Property

    Public ReadOnly Property Minimum() As Double
        Get
            Return pMinimum
        End Get
    End Property

    Public Function wrapSelf(bLabels As Boolean, Optional statsToReturn As List(Of String) = Nothing) As Object(,)
        Dim out(,) As Object, i As Integer

        If statsToReturn Is Nothing Then 'provide full statistic
            If bLabels Then
                out = {{"Valid Data", Me.pValidN}, {"Mean", Me.pMean}, {"Median", Me.pMedian}, {"SD", Me.pSD}, {"SEM", Me.pSEM},
                       {"Variance", Me.pVariance}, {"Coefficient of Variation", Me.pCoefficientofVariation},
                       {"Skewness", Me.pSkewness}, {"Kurtosis", Me.pKurtosis}, {"Q1", Me.pLQuartile}, {"Q3", Me.pUQuartile},
                       {"IQR", Me.pIQR}, {"Minimum", Me.pMinimum}, {"Maximum", Me.pMaximum}, {"Range", Me.pRange},
                       {"Shapiro-Wilk W", Me.pSWstat}, {"Two-sided p-value", Me.pSWPvalue}}
            Else
                out = {{Me.pValidN}, {Me.pMean}, {Me.pMedian}, {Me.pSD}, {Me.pSEM}, {Me.pVariance}, {Me.pCoefficientofVariation},
                   {Me.pSkewness}, {Me.pKurtosis}, {Me.pLQuartile}, {Me.pUQuartile}, {Me.pIQR}, {Me.pMinimum}, {Me.pMaximum},
                   {Me.pRange}, {Me.pSWstat}, {Me.pSWPvalue}}
            End If
        Else
            If bLabels Then
                ReDim out(statsToReturn.Count - 1, 1)
            Else
                ReDim out(statsToReturn.Count - 1, 0)
            End If

            For i = 0 To statsToReturn.Count - 1
                If statsToReturn(i).ToLower = "mean" Or statsToReturn(i).ToLower = "avg" Or statsToReturn(i).ToLower = "average" Then
                    If bLabels Then
                        out(i, 0) = "Mean"
                        out(i, 1) = Me.pMean
                    Else
                        out(i, 0) = Me.pMean
                    End If
                ElseIf statsToReturn(i).ToLower = "n" Then
                    If bLabels Then
                        out(i, 0) = "Valid Data"
                        out(i, 1) = Me.pValidN
                    Else
                        out(i, 0) = Me.pValidN
                    End If
                ElseIf statsToReturn(i).ToLower = "median" Or statsToReturn(i).ToLower = "q2" Then
                    If bLabels Then
                        out(i, 0) = "Median"
                        out(i, 1) = Me.pMedian
                    Else
                        out(i, 0) = Me.pMedian
                    End If
                ElseIf statsToReturn(i).ToLower = "sd" Or statsToReturn(i).ToLower = "standard deviation" Or statsToReturn(i).ToLower = "std" Then
                    If bLabels Then
                        out(i, 0) = "SD"
                        out(i, 1) = Me.pSD
                    Else
                        out(i, 0) = Me.pSD
                    End If
                ElseIf statsToReturn(i).ToLower = "sem" Then
                    If bLabels Then
                        out(i, 0) = "SEM"
                        out(i, 1) = Me.pSEM
                    Else
                        out(i, 0) = Me.pSEM
                    End If
                ElseIf statsToReturn(i).ToLower = "var" Or statsToReturn(i).ToLower = "variance" Then
                    If bLabels Then
                        out(i, 0) = "Variance"
                        out(i, 1) = Me.pVariance
                    Else
                        out(i, 0) = Me.pVariance
                    End If
                ElseIf statsToReturn(i).ToLower = "cv" Then
                    If bLabels Then
                        out(i, 0) = "Coefficient of Variation"
                        out(i, 1) = Me.pCoefficientofVariation
                    Else
                        out(i, 0) = Me.pCoefficientofVariation
                    End If
                ElseIf statsToReturn(i).ToLower = "skew" Or statsToReturn(i).ToLower = "skewness" Then
                    If bLabels Then
                        out(i, 0) = "Skewness"
                        out(i, 1) = Me.pSkewness
                    Else
                        out(i, 0) = Me.pSkewness
                    End If
                ElseIf statsToReturn(i).ToLower = "kurt" Or statsToReturn(i).ToLower = "kurtosis" Then
                    If bLabels Then
                        out(i, 0) = "Kurtosis"
                        out(i, 1) = Me.pKurtosis
                    Else
                        out(i, 0) = Me.pKurtosis
                    End If
                ElseIf statsToReturn(i).ToLower = "q1" Then
                    If bLabels Then
                        out(i, 0) = "Q1"
                        out(i, 1) = Me.pLQuartile
                    Else
                        out(i, 0) = Me.pLQuartile
                    End If
                ElseIf statsToReturn(i).ToLower = "q3" Then
                    If bLabels Then
                        out(i, 0) = "Q3"
                        out(i, 1) = Me.pUQuartile
                    Else
                        out(i, 0) = Me.pUQuartile
                    End If
                ElseIf statsToReturn(i).ToLower = "min" Or statsToReturn(i).ToLower = "minimum" Then
                    If bLabels Then
                        out(i, 0) = "Minimum"
                        out(i, 1) = Me.pMinimum
                    Else
                        out(i, 0) = Me.pMinimum
                    End If
                ElseIf statsToReturn(i).ToLower = "max" Or statsToReturn(i).ToLower = "maximum" Then
                    If bLabels Then
                        out(i, 0) = "Maximum"
                        out(i, 1) = Me.pMaximum
                    Else
                        out(i, 0) = Me.pMaximum
                    End If
                ElseIf statsToReturn(i).ToLower = "range" Then
                    If bLabels Then
                        out(i, 0) = "Range"
                        out(i, 1) = Me.pRange
                    Else
                        out(i, 0) = Me.pRange
                    End If
                ElseIf statsToReturn(i).ToLower = "iqr" Then
                    If bLabels Then
                        out(i, 0) = "IQR"
                        out(i, 1) = Me.pIQR
                    Else
                        out(i, 0) = Me.pIQR
                    End If
                ElseIf statsToReturn(i).ToLower = "swstat" Then
                    If bLabels Then
                        out(i, 0) = "Shapiro-Wilk W"
                        out(i, 1) = Me.pSWstat
                    Else
                        out(i, 0) = Me.pSWstat
                    End If
                ElseIf statsToReturn(i).ToLower = "swpvalue" Then
                    If bLabels Then
                        out(i, 0) = "Two-sided p-value"
                        out(i, 1) = Me.pSWPvalue
                    Else
                        out(i, 0) = Me.pSWPvalue
                    End If
                Else
                    CoreServices.Errors.LogAndThrow(New ArgumentException("Unrecognized statistic"))
                End If
            Next
        End If
        Return out
    End Function

    Public Sub compute(Optional bShapiroWilk As Boolean = True)

        Dim Quartiles As udQuartiles, arData() As Double
        Dim strErrTmp As String = String.Empty, SWout = New TestResult

        arData = pData

        'Quartiles computes using user defined subs
        Quartiles = QuartilesComp(arData)
        pLQuartile = Quartiles.Q1
        pMedian = Quartiles.Median
        pUQuartile = Quartiles.Q3
        pIQR = pUQuartile - pLQuartile
        'Skewness and kurtosis computed using standard definitions
        pSkewness = Skewness(arData)
        pKurtosis = Kurtosis(arData)


        pValidN = pData.Length
        pMean = pData.Average()
        pMinimum = pData.Min()
        pMaximum = pData.Max()
        pRange = pMaximum - pMinimum
        If pValidN > 1 Then pVariance = variance(pData)
        If pValidN > 1 Then pSD = stDev(pData)
        If pValidN > 0 Then pSEM = pSD / Math.Sqrt(pValidN)
        If pMean <> 0 Then pCoefficientofVariation = pSD / pMean

        'Compute Shapiro-Wilk test
        If pValidN > 3 And pValidN < 5000 And bShapiroWilk = True Then
            SWout = BESHStatNG.assumptions.ShapiroWilk(arData, strErrTmp)
            pSWstat = SWout.TestStatistics1
            pSWPvalue = SWout.Pvalue
        Else
            pSWstat = Double.NaN
            pSWPvalue = Double.NaN
        End If
    End Sub

End Class
