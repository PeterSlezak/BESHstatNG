Option Explicit On
Option Strict On

Imports System
Imports BESHStatNG.DataManagement

Namespace Matrix

    ''' <summary>
    ''' Host-neutral correlation-matrix calculation used by the legacy Matrix.CorrelMatrix facade.
    ''' The upper triangle contains coefficients and the lower triangle contains p-values.
    ''' </summary>
    Friend NotInheritable Class CorrelationMatrixCore

        Private Sub New()
        End Sub

        Friend Shared Function Compute(inputData(,) As Double, correlationType As String) As Double(,)
            Dim rowCount As Integer = inputData.GetLength(0)
            Dim variableCount As Integer = inputData.GetLength(1)
            Dim correlationMatrix(variableCount - 1, variableCount - 1) As Double

            For i As Integer = 0 To variableCount - 1
                Dim firstVariable() As Double = ArrayUtilities.GetColumn(inputData, i)

                For j As Integer = 0 To variableCount - 1
                    If i >= j Then
                        Dim secondVariable() As Double = ArrayUtilities.GetColumn(inputData, j)

                        If correlationType = "r" Then
                            Dim coefficient As Double = StatFunc.Correl(firstVariable, secondVariable)
                            correlationMatrix(j, i) = coefficient
                            If i <> j Then
                                Dim testStatistic As Double = Math.Abs(
                                    coefficient * Math.Sqrt(rowCount - 2) /
                                    (1.0R - Math.Sqrt(coefficient ^ 2)))
                                correlationMatrix(i, j) = distributions.T_2T(testStatistic, rowCount)
                            End If
                        ElseIf correlationType = "rho" Then
                            Dim spearman As New nonparametric.SpearmanRho(firstVariable, secondVariable, "x", "pY")
                            spearman.Compute()
                            correlationMatrix(i, j) = spearman.pvalue
                            correlationMatrix(j, i) = spearman.correlCoef
                        ElseIf correlationType = "tau" Then
                            Dim kendall As New nonparametric.KendallsTau(firstVariable, secondVariable, "x", "pY")
                            kendall.compute()
                            correlationMatrix(i, j) = kendall.pvalue
                            correlationMatrix(j, i) = kendall.correlCoef
                        End If
                    End If
                Next
            Next

            Return correlationMatrix
        End Function

    End Class

End Namespace
