Option Explicit On
Option Strict On
Option Infer On

Imports System
Imports System.Collections.Generic
Imports System.Linq

Namespace CausalInference

    Public NotInheritable Class PsmBalanceDiagnostics
        Private Sub New()
        End Sub

        Public Shared Function ComputeBalance(input As PsmInputData, weights As Double(), sample As PsmBalanceSample, options As PsmOptions) As List(Of PsmBalanceRow)
            If input Is Nothing Then Throw New ArgumentNullException("input")
            If weights Is Nothing Then weights = Matrix.MatrixArithmeticCore.ConstantVector(input.RowCount - 1)
            If weights.Length <> input.RowCount Then Throw New ArgumentException("Weights length must match input row count.")

            Dim rows As New List(Of PsmBalanceRow)()
            For j As Integer = 0 To input.CovariateCount - 1
                Dim treatedValues As New List(Of Double)()
                Dim treatedWeights As New List(Of Double)()
                Dim controlValues As New List(Of Double)()
                Dim controlWeights As New List(Of Double)()
                For i As Integer = 0 To input.RowCount - 1
                    If weights(i) <= 0 OrElse Not AppInfrastructure.IsFinite(weights(i)) Then Continue For
                    If input.Treatment(i) >= 0.5 Then
                        treatedValues.Add(input.Covariates(i, j))
                        treatedWeights.Add(weights(i))
                    Else
                        controlValues.Add(input.Covariates(i, j))
                        controlWeights.Add(weights(i))
                    End If
                Next

                Dim tv As Double() = treatedValues.ToArray()
                Dim tw As Double() = treatedWeights.ToArray()
                Dim cv As Double() = controlValues.ToArray()
                Dim cw As Double() = controlWeights.ToArray()
                Dim mt As Double = PsmMath.WeightedMean(tv, tw)
                Dim mc As Double = PsmMath.WeightedMean(cv, cw)
                Dim vt As Double = PsmMath.WeightedVariance(tv, tw, True)
                Dim vc As Double = PsmMath.WeightedVariance(cv, cw, True)
                Dim smd As Double = PsmMath.StandardizedMeanDifference(mt, mc, vt, vc)
                Dim vr As Double = PsmMath.VarianceRatio(vt, vc)
                Dim ecdf = PsmMath.EcdfDifference(tv, tw, cv, cw)
                Dim flag As String = BalanceFlag(smd, vr, options)

                rows.Add(New PsmBalanceRow With {
                    .Sample = sample,
                    .VariableName = input.GetCovariateName(j),
                    .TreatedMean = mt,
                    .ControlMean = mc,
                    .TreatedVariance = vt,
                    .ControlVariance = vc,
                    .StandardizedMeanDifference = smd,
                    .VarianceRatio = vr,
                    .EcdfMeanDifference = ecdf.Item1,
                    .EcdfMaxDifference = ecdf.Item2,
                    .TreatedN = tw.Sum(),
                    .ControlN = cw.Sum(),
                    .Flag = flag
                })
            Next
            Return rows
        End Function

        Private Shared Function BalanceFlag(smd As Double, varianceRatio As Double, options As PsmOptions) As String
            Dim problems As New List(Of String)()
            If AppInfrastructure.IsFinite(smd) AndAlso Math.Abs(smd) > options.BalanceSmdThreshold Then problems.Add("SMD")
            If AppInfrastructure.IsFinite(varianceRatio) AndAlso (varianceRatio < options.BalanceVarianceRatioLower OrElse varianceRatio > options.BalanceVarianceRatioUpper) Then problems.Add("VR")
            If problems.Count = 0 Then Return "OK"
            Return String.Join(",", problems.ToArray())
        End Function
    End Class

End Namespace
