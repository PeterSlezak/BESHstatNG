Option Explicit On
Option Strict On

Imports System
Imports System.Linq

''' <summary>Ordinary Q-Q only: exact legacy score/line choices and X=data, Y=normal orientation.
''' Unknown score methods still yield zero vectors; unknown line methods yield the legacy zero line.
''' No detrending, new quartile rule, finite policy or corrected R semantics.</summary>
Public NotInheritable Class NormalPlotDataCore

    Private Sub New()
    End Sub

    Public Shared Function SortedCopy(values As Double()) As Double()
        If values Is Nothing Then
            Array.Sort(values)
        End If

        Dim copy = DirectCast(values.Clone(), Double())
        Array.Sort(copy)
        Return copy
    End Function

    Public Shared Function Compute(values As Double(), method As String) As NormalScoreData
        Return Scores(SortedCopy(values), method)
    End Function

    ''' <summary>Compatibility entry for a wrapper whose returned score vector is mutable.
    ''' Inputs are copied, including any caller edits made to that vector.</summary>
    Public Shared Function ReferenceLine(sortedData As Double(), probabilities As Double(), normalScores As Double(), scoreMethod As String, lineMethod As String) As Double(,)
        Return ReferenceLine(New NormalScoreData(sortedData, probabilities, normalScores, scoreMethod), lineMethod)
    End Function

    Public Shared Function Scores(sortedData As Double(), strMethod As String) As NormalScoreData
        Dim pData = DirectCast(sortedData.Clone(), Double())
        Dim n = pData.Length
        Dim pstrNormalScores As String
        Dim pZ() As Double, pP() As Double
        pstrNormalScores = strMethod
        ReDim pZ(n - 1), pP(n - 1)
        Dim ranks = nonparametric.ComputeAvgRanks(pData)
        If pstrNormalScores = "Blom" Then
            For i = 0 To n - 1
                pP(i) =(ranks(i) - 0.375) / (n + 0.25)
                pZ(i) = distributions.NormSInv(pP(i))
            Next
        ElseIf pstrNormalScores = "Rankit" Then
            For i = 0 To n - 1
                pP(i) =(ranks(i) - 0.5) / n
                pZ(i) = distributions.NormSInv(pP(i))
            Next
        ElseIf pstrNormalScores = "Van Der Waerden" Then
            For i = 0 To n - 1
                pP(i) = ranks(i) / (n + 1)
                pZ(i) = distributions.NormSInv(pP(i))
            Next
        End If

        Return New NormalScoreData(pData, pP, pZ, pstrNormalScores)
    End Function

    Public Shared Function ReferenceLine(scores As NormalScoreData, strMethod As String) As Double(,)
        Dim pData = scores.Data, pP = scores.Probabilities, pZ = scores.NormalScores
        Dim pstrNormalScores = scores.Method
        Dim n = pData.Length
        Dim pRefLine(1, 1) As Double
        Dim dSlope As Double, dIntercept As Double
        If strMethod = "SPSS" Then
            Dim dMean As Double = pData.Average()
            Dim dSD As Double = stDev(pData)
            pRefLine(0, 0) = distributions.QNorm(pP.Min(), dMean, dSD)
            pRefLine(1, 0) = distributions.QNorm(pP.Max(), dMean, dSD)
            pRefLine(0, 1) = pZ.Min()
            pRefLine(1, 1) = pZ.Max()
            dSlope = Slope(DataManagement.ArrayUtilities.GetColumn(pRefLine, 1), DataManagement.ArrayUtilities.GetColumn(pRefLine, 0))
            dIntercept = Intercept(DataManagement.ArrayUtilities.GetColumn(pRefLine, 1), DataManagement.ArrayUtilities.GetColumn(pRefLine, 0))
        ElseIf strMethod = "OLS" Then
            dSlope = Slope(pZ, pData)
            dIntercept = Intercept(pZ, pData)
        ElseIf strMethod = "R" Then
            Dim dRankQ = {(RoundDown((0.25 * (n + 1)), 0) + RoundUp((0.25 * (n + 1)), 0)) / 2, (RoundDown((0.75 * (n + 1)), 0) + RoundUp((0.75 * (n + 1)), 0)) / 2}
            Dim Quartiles = QuartilesComp(pData)
            Dim dQ() = {Quartiles.Q1, Quartiles.Q3}
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
End Class

Public NotInheritable Class NormalScoreData

    Private ReadOnly values() As Double, probabilitiesData() As Double, z() As Double

    Friend Sub New(data() As Double, probabilities() As Double, normalScores() As Double, method As String)
        values = DirectCast(data.Clone(), Double())
        probabilitiesData = If(probabilities Is Nothing, Nothing, DirectCast(probabilities.Clone(), Double()))
        z = If(normalScores Is Nothing, Nothing, DirectCast(normalScores.Clone(), Double())) : Me.Method = method
    End Sub

    Public ReadOnly Property Method As String

    Public ReadOnly Property Data As Double()
        Get
            Return DirectCast(values.Clone(), Double())
        End Get
    End Property

    Public ReadOnly Property Probabilities As Double()
        Get
            Return If(probabilitiesData Is Nothing, Nothing, DirectCast(probabilitiesData.Clone(), Double()))
        End Get
    End Property

    Public ReadOnly Property NormalScores As Double()
        Get
            Return If(z Is Nothing, Nothing, DirectCast(z.Clone(), Double()))
        End Get
    End Property
End Class

