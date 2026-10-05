Option Strict On
Option Explicit On

Imports System

Namespace regression

    ''' <summary>
    ''' Specifies which category is treated as the reference (baseline) category.
    ''' Categories are first sorted in ascending order of their observed values.
    ''' </summary>
    Public Enum ReferenceCategory
        ''' <summary>Use the first (smallest) observed category as baseline.</summary>
        First = 0
        ''' <summary>Use the last (largest) observed category as baseline.</summary>
        Last = 1
    End Enum

    ''' <summary>
    ''' Container for a confusion matrix and common classification diagnostics.
    ''' </summary>
    Public Class ClassificationCrosstab
        Public Categories() As Integer                 ' original category labels in ascending order
        Public Counts(,) As Double                     ' (obs x pred) weighted or unweighted
        Public RowTotals() As Double                   ' obs totals
        Public ColTotals() As Double                   ' pred totals
        Public OverallAccuracy As Double               ' sum(diag)/sum(all)
        Public OverallAccuracyPct As Double
        Public RecallPct() As Double                   ' per observed class: diag/row total * 100
        Public PrecisionPct() As Double                ' per predicted class: diag/col total * 100
        Public ColTotalsPrct() As Double
    End Class

    ''' <summary>
    ''' Identifies the type of residual-related quantity for which column names should be generated.
    ''' </summary>
    Public Enum ResidualColumnType
        ''' <summary>Observed counts y_{ik} (one-hot or weighted counts).</summary>
        Observed = 0
        ''' <summary>Fitted probabilities p_{ik}.</summary>
        FittedProbability = 1
        ''' <summary>Fitted means μ_{ik} = m_i p_{ik}.</summary>
        FittedMean = 2
        ''' <summary>Response (raw) residuals y_{ik} - μ_{ik}.</summary>
        ResponseResidual = 3
        ''' <summary>Pearson residuals (y_{ik}-μ_{ik}) / sqrt(m_i p_{ik} (1-p_{ik})).</summary>
        PearsonResidual = 4
        ''' <summary>Standardized Pearson residuals divided by sqrt(1 - h_i).</summary>
        StdPearsonResidual = 5
    End Enum


    ''' <summary>
    ''' Utility functions for baseline-category multinomial logit computations.
    ''' </summary>
    Public Module CategoricalLogitUtils

        ''' <summary>
        ''' Computes <c>log( exp(0) + sum_k exp(eta_k) )</c> stably, where the baseline category has logit 0.
        ''' </summary>
        ''' <param name="eta">Linear predictors for the non-baseline categories (length K-1).</param>
        ''' <returns>Stable log-sum-exp including the baseline exp(0).</returns>
        Public Function LogSumExpBaselineZero(eta() As Double) As Double
            Dim maxv As Double = 0.0
            For i As Integer = 0 To eta.Length - 1
                If eta(i) > maxv Then maxv = eta(i)
            Next
            Dim s As Double = Math.Exp(-maxv) ' baseline term exp(0-maxv)
            For i As Integer = 0 To eta.Length - 1
                s += Math.Exp(eta(i) - maxv)
            Next
            Return maxv + Math.Log(s)
        End Function

        ''' <summary>
        ''' Multiplies a square matrix by a vector.
        ''' </summary>
        ''' <param name="A">Square matrix (q x q).</param>
        ''' <param name="v">Vector length q.</param>
        ''' <returns>Vector length q equal to A*v.</returns>
        Public Function MatTimesVec(A(,) As Double, v() As Double) As Double()
            Dim q As Integer = A.GetLength(0) - 1
            Dim out(q) As Double
            For i As Integer = 0 To q
                Dim s As Double = 0.0
                For j As Integer = 0 To q
                    s += A(i, j) * v(j)
                Next
                out(i) = s
            Next
            Return out
        End Function

        ''' <summary>
        ''' Returns the maximum absolute value in a vector.
        ''' </summary>
        Public Function MaxAbs(v() As Double) As Double
            Dim m As Double = 0.0R
            For i As Integer = 0 To v.Length - 1
                Dim a As Double = Math.Abs(v(i))
                If a > m Then m = a
            Next
            Return m
        End Function

        ''' <summary>
        ''' Returns index of maximum element (ties optionally break to smallest index).
        ''' </summary>
        Public Function ArgMax(v() As Double, tieBreakToSmallest As Boolean) As Integer
            Dim bestIdx As Integer = 0
            Dim bestVal As Double = v(0)
            For i As Integer = 1 To v.Length - 1
                If v(i) > bestVal Then
                    bestVal = v(i)
                    bestIdx = i
                ElseIf tieBreakToSmallest AndAlso v(i) = bestVal Then
                    ' keep smallest index
                End If
            Next
            Return bestIdx
        End Function

        Friend Function Concatenate(Of T)(first() As T, second() As T) As T()
            If first Is Nothing Then Throw New ArgumentNullException(NameOf(first))
            If second Is Nothing Then Throw New ArgumentNullException(NameOf(second))

            Dim output(first.Length + second.Length - 1) As T
            Array.Copy(first, 0, output, 0, first.Length)
            Array.Copy(second, 0, output, first.Length, second.Length)
            Return output
        End Function

        Friend Function ConcatenateColumns(Of T)(first(,) As T, second(,) As T) As T(,)
            If first Is Nothing Then Throw New ArgumentNullException(NameOf(first))
            If second Is Nothing Then Throw New ArgumentNullException(NameOf(second))
            If first.GetLength(0) <> second.GetLength(0) Then
                Throw New ArgumentException("Input arrays must have the same number of rows.")
            End If

            Dim rows As Integer = first.GetLength(0)
            Dim firstCols As Integer = first.GetLength(1)
            Dim secondCols As Integer = second.GetLength(1)
            Dim output(rows - 1, firstCols + secondCols - 1) As T

            For row As Integer = 0 To rows - 1
                For col As Integer = 0 To firstCols - 1
                    output(row, col) = first(row, col)
                Next
                For col As Integer = 0 To secondCols - 1
                    output(row, firstCols + col) = second(row, col)
                Next
            Next

            Return output
        End Function

        Friend Function ArrayToString(Of T)(values() As T) As String
            If values Is Nothing Then Return "[]"

            Dim builder As New System.Text.StringBuilder("[")
            For i As Integer = 0 To values.Length - 1
                If i > 0 Then builder.Append(", ")
                Dim value As T = values(i)
                If CObj(value) IsNot Nothing Then builder.Append(value.ToString())
            Next
            builder.Append("]")
            Return builder.ToString()
        End Function
    End Module


    ''' <summary>
    ''' Stores residuals and related per-observation diagnostics for a multinomial logit model.
    ''' </summary>
    ''' <remarks>
    ''' For each observation i and category k:
    ''' <para>
    ''' - Observed "count": y_{ik} (for individual records, y_{ik}=m_i for the observed category and 0 otherwise)
    ''' - Fitted probability: p_{ik}
    ''' - Fitted mean: μ_{ik} = m_i p_{ik}
    ''' - Response residual: r^{(R)}_{ik} = y_{ik} - μ_{ik}
    ''' - Pearson residual: r^{(P)}_{ik} = (y_{ik}-μ_{ik}) / sqrt(m_i p_{ik} (1-p_{ik}))
    ''' </para>
    ''' <para>
    ''' Deviance residual magnitude (per row):
    ''' d_i = sqrt( 2 * Σ_k y_{ik} * log( y_{ik} / μ_{ik} ) ), ignoring terms with y_{ik}=0.
    ''' For one-trial rows, this simplifies to d_i = sqrt( 2*m_i*log(1/p_{i,y_i}) ) and is nonnegative.
    ''' </para>
    ''' <para>
    ''' Standardization uses leverage h_i: r_std = r / sqrt(1 - h_i).
    ''' </para>
    ''' </remarks>
    Public Class MultinomialResiduals
        Public Categories() As Integer
        Public Observed(,) As Double
        Public Probabilities(,) As Double
        Public FittedMeans(,) As Double
        Public ResponseResiduals(,) As Double
        Public PearsonResiduals(,) As Double
        Public Leverage() As Double
        Public StdPearsonResiduals(,) As Double
        Public DevianceResiduals() As Double
        Public StdDevianceResiduals() As Double
        Public DevianceContrib() As Double
    End Class


End Namespace
