Option Explicit On
Option Strict On

''' <summary>
''' Host-neutral special functions shared by probability-distribution calculations.
''' These members are internal implementation details; the public StatFunc
''' entry points remain compatibility forwarders.
''' </summary>
Friend NotInheritable Class StatisticalSpecialFunctions

    Private Sub New()
    End Sub

    Friend Shared Function LogCombin(n As Integer, k As Integer) As Double
        If k < 0 OrElse k > n Then Return Double.NegativeInfinity
        If k = 0 OrElse k = n Then Return 0.0

        If k > n \ 2 Then k = n - k

        Dim sum As Double = 0.0
        For i As Integer = 1 To k
            sum += Math.Log(n - k + i) - Math.Log(i)
        Next

        Return sum
    End Function

    Friend Shared Function LogGamma(z As Double) As Double
        Dim p() As Double = {
            0.99999999999980993,
            676.5203681218851,
            -1259.1392167224028,
            771.32342877765313,
            -176.61502916214059,
            12.507343278686905,
            -0.13857109526572012,
            0.0000099843695780195716,
            0.00000015056327351493116
        }

        If z <= 0.0 AndAlso z = Math.Floor(z) Then
            Global.BESHStatNG.AppInfrastructure.CoreServices.Errors.LogAndThrow(
                New ArgumentException("Gamma function is undefined for non-positive integers."))
        End If

        If z < 0.5 Then
            Return Math.Log(Math.PI) - Math.Log(Math.Sin(Math.PI * z)) - LogGamma(1.0 - z)
        End If

        z -= 1.0
        Dim x As Double = p(0)
        For i As Integer = 1 To p.Length - 1
            x += p(i) / (z + i)
        Next

        Dim t As Double = z + 7.5
        Return 0.5 * Math.Log(2.0 * Math.PI) +
               (z + 0.5) * Math.Log(t) - t + Math.Log(x)
    End Function

    Friend Shared Function LowerIncompleteGamma(a As Double, x As Double) As Double
        If Double.IsNaN(a) OrElse Double.IsNaN(x) Then Return Double.NaN
        If x < 0.0 OrElse a <= 0.0 Then Return Double.NaN
        If x = 0.0 Then Return 0.0
        If Double.IsInfinity(x) Then Return 1.0

        Dim logPref As Double = -x + a * Math.Log(x) - LogGamma(a)

        If logPref > 709.0 Then
            Return If(x >= a, 1.0, 0.0)
        End If

        If x < a + 1.0 Then
            Dim sum As Double = 1.0 / a
            Dim term As Double = sum
            Dim n As Integer = 1
            Const maxIter As Integer = 200000

            While Math.Abs(term) > 0.000000000000001 AndAlso n < maxIter
                term *= x / (a + n)
                sum += term
                n += 1
                If Double.IsNaN(sum) OrElse Double.IsInfinity(sum) Then Exit While
            End While

            Dim result As Double = sum * Math.Exp(logPref)
            If Double.IsNaN(result) Then Return Double.NaN
            If result < 0.0 Then Return 0.0
            If result > 1.0 Then Return 1.0
            Return result
        End If

        Dim b As Double = x + 1.0 - a
        Dim c As Double = 1.0 / 1.0E-30
        Dim d As Double = 1.0 / b
        Dim h As Double = d
        Dim iteration As Integer = 1
        Const maxCfIterations As Integer = 100000

        While iteration < maxCfIterations
            Dim iValue As Double = Convert.ToDouble(iteration)
            Dim an As Double = -iValue * (iValue - a)
            b += 2.0

            d = an * d + b
            If Math.Abs(d) < 1.0E-30 Then d = 1.0E-30

            c = b + an / c
            If Math.Abs(c) < 1.0E-30 Then c = 1.0E-30

            d = 1.0 / d
            Dim delta As Double = d * c

            If Double.IsNaN(delta) OrElse Double.IsInfinity(delta) Then Exit While

            h *= delta

            If Double.IsNaN(h) OrElse Double.IsInfinity(h) Then Exit While
            If Math.Abs(delta - 1.0) < 0.00000000000001 Then Exit While

            iteration += 1
        End While

        If Double.IsNaN(h) OrElse Double.IsInfinity(h) OrElse iteration >= maxCfIterations Then
            If x > a Then Return 1.0
            Return Double.NaN
        End If

        Dim q As Double = h * Math.Exp(logPref)
        Dim p As Double = 1.0 - q

        If Double.IsNaN(p) Then Return Double.NaN
        If p < 0.0 Then Return 0.0
        If p > 1.0 Then Return 1.0
        Return p
    End Function

End Class
