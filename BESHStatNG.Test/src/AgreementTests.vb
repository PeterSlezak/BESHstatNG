Option Explicit On
Imports Microsoft.VisualStudio.TestTools.UnitTesting
Imports System

Imports BESHStatNG

<TestClass()>
Public Class Agreement_Tests

    Private Const TOL As Double = 0.000001
    Private Const TOL_CI As Double = 0.00001

    Private Shared Sub AssertAlmostEqual(expected As Double, actual As Double, tol As Double, msg As String)
        If Double.IsNaN(actual) OrElse Double.IsInfinity(actual) Then
            Assert.Fail($"{msg}: expected {expected} but got {actual}.")
        End If
        Dim diff As Double = Math.Abs(expected - actual)
        If diff > tol Then
            Assert.Fail($"{msg}: expected {expected} but got {actual}. |diff|={diff} > tol={tol}.")
        End If
    End Sub

    Private Shared Sub ComputeTwoWayMeanSquares_NoReplication(x(,) As Double,
                                                         ByRef MSC As Double,
                                                         ByRef MSR As Double,
                                                         ByRef MSE As Double,
                                                         ByRef dfC As Integer,
                                                         ByRef dfR As Integer,
                                                         ByRef dfE As Integer)
        Dim n As Integer = x.GetLength(0)
        Dim k As Integer = x.GetLength(1)

        ' grand mean
        Dim sum As Double = 0.0
        For i As Integer = 0 To n - 1
            For j As Integer = 0 To k - 1
                sum += x(i, j)
            Next
        Next
        Dim gm As Double = sum / (n * k)

        ' row means
        Dim rbar(n - 1) As Double
        For i As Integer = 0 To n - 1
            Dim s As Double = 0.0
            For j As Integer = 0 To k - 1
                s += x(i, j)
            Next
            rbar(i) = s / k
        Next

        ' col means
        Dim cbar(k - 1) As Double
        For j As Integer = 0 To k - 1
            Dim s As Double = 0.0
            For i As Integer = 0 To n - 1
                s += x(i, j)
            Next
            cbar(j) = s / n
        Next

        ' sums of squares (no replication)
        Dim SSR As Double = 0.0
        For i As Integer = 0 To n - 1
            Dim d As Double = rbar(i) - gm
            SSR += d * d
        Next
        SSR *= k

        Dim SSC As Double = 0.0
        For j As Integer = 0 To k - 1
            Dim d As Double = cbar(j) - gm
            SSC += d * d
        Next
        SSC *= n

        Dim SSE As Double = 0.0
        For i As Integer = 0 To n - 1
            For j As Integer = 0 To k - 1
                Dim e As Double = x(i, j) - rbar(i) - cbar(j) + gm
                SSE += e * e
            Next
        Next

        dfR = n - 1
        dfC = k - 1
        dfE = (n - 1) * (k - 1)

        MSR = SSR / dfR
        MSC = SSC / dfC
        MSE = SSE / dfE
    End Sub

    ' ---------------- ICC(1,1) / ICC(1,k) ----------------

    <TestMethod>
    Public Sub ICC11_ShroutFleiss1979_example_matches_reference()
        ' Shrout & Fleiss (1979) example (6 targets x 4 raters)
        ' Rows are targets, columns are raters.
        Dim x()() As Double = {
            New Double() {9, 2, 5, 8},
            New Double() {6, 1, 3, 2},
            New Double() {8, 4, 6, 8},
            New Double() {7, 1, 2, 6},
            New Double() {10, 5, 6, 9},
            New Double() {6, 2, 4, 7}
        }

        Dim icc As New Agreement.IntraclassCorrelation()
        Dim res = icc.ICC11(x, 0.05)

        ' Reference values (computed independently using the standard F-based CI method)
        AssertAlmostEqual(0.1657418, res.Estimate, TOL, "ICC(1,1) estimate")
        AssertAlmostEqual(-0.1329323, res.LowerLimit, TOL_CI, "ICC(1,1) 95% CI lower")
        AssertAlmostEqual(0.7225601, res.UpperLimit, TOL_CI, "ICC(1,1) 95% CI upper")
    End Sub

    <TestMethod>
    Public Sub ICC1k_ShroutFleiss1979_example_matches_reference()
        Dim x()() As Double = {
            New Double() {9, 2, 5, 8},
            New Double() {6, 1, 3, 2},
            New Double() {8, 4, 6, 8},
            New Double() {7, 1, 2, 6},
            New Double() {10, 5, 6, 9},
            New Double() {6, 2, 4, 7}
        }

        Dim icc As New Agreement.Agreement.IntraclassCorrelation()
        Dim res = icc.ICC1k(x, 0.05)

        AssertAlmostEqual(0.4427971, res.Estimate, TOL, "ICC(1,k) estimate")
        AssertAlmostEqual(-0.8844422, res.LowerLimit, TOL_CI, "ICC(1,k) 95% CI lower")
        AssertAlmostEqual(0.9124154, res.UpperLimit, TOL_CI, "ICC(1,k) 95% CI upper")
    End Sub

    <TestMethod>
    Public Sub ICC11_throws_on_too_few_or_empty_groups()
        Dim icc As New Agreement.Agreement.IntraclassCorrelation()

        ' < 2 groups
        Dim x1()() As Double = {New Double() {1, 2, 3}}
        Assert.ThrowsException(Of ArgumentException)(Sub() icc.ICC11(x1))

        ' Empty group
        Dim x2()() As Double = {New Double() {1, 2, 3}, New Double() {}}
        Assert.ThrowsException(Of ArgumentException)(Sub() icc.ICC11(x2))
    End Sub

    ' ---------------- ICC(2,1) / ICC(2,k) / ICC(3,1) / ICC(3,k) ----------------

    <TestMethod>
    Public Sub ICC21_ShroutFleiss1979_example_matches_reference()
        ' Shrout & Fleiss (1979) example (6 targets x 4 raters)
        ' Rows are targets, columns are raters.
        Dim x(,) As Double = {
        {9, 2, 5, 8},
        {6, 1, 3, 2},
        {8, 4, 6, 8},
        {7, 1, 2, 6},
        {10, 5, 6, 9},
        {6, 2, 4, 7}
    }

        Dim icc As New Agreement.Agreement.IntraclassCorrelation()
        Dim res = icc.ICC21(x, 0.05)

        ' Reference values (computed independently using the standard F-based CI transform method)
        AssertAlmostEqual(0.2897638, res.Estimate, TOL, "ICC(2,1) estimate")
        AssertAlmostEqual(0.0781413, res.LowerLimit, TOL_CI, "ICC(2,1) 95% CI lower")
        AssertAlmostEqual(0.7398028, res.UpperLimit, TOL_CI, "ICC(2,1) 95% CI upper")
    End Sub

    <TestMethod>
    Public Sub ICC2k_ShroutFleiss1979_example_matches_reference()
        Dim x(,) As Double = {
        {9, 2, 5, 8},
        {6, 1, 3, 2},
        {8, 4, 6, 8},
        {7, 1, 2, 6},
        {10, 5, 6, 9},
        {6, 2, 4, 7}
    }

        Dim icc As New Agreement.Agreement.IntraclassCorrelation()
        Dim res = icc.ICC2k(x, 0.05)

        AssertAlmostEqual(0.6200506, res.Estimate, TOL, "ICC(2,k) estimate")
        AssertAlmostEqual(0.2532074, res.LowerLimit, TOL_CI, "ICC(2,k) 95% CI lower")
        AssertAlmostEqual(0.9191786, res.UpperLimit, TOL_CI, "ICC(2,k) 95% CI upper")
    End Sub

    <TestMethod>
    Public Sub ICC31_ShroutFleiss1979_example_matches_reference()
        Dim x(,) As Double = {
        {9, 2, 5, 8},
        {6, 1, 3, 2},
        {8, 4, 6, 8},
        {7, 1, 2, 6},
        {10, 5, 6, 9},
        {6, 2, 4, 7}
    }

        Dim icc As New Agreement.Agreement.IntraclassCorrelation()
        Dim res = icc.ICC31(x, 0.05)

        AssertAlmostEqual(0.7148407, res.Estimate, TOL, "ICC(3,1) estimate")
        AssertAlmostEqual(0.342464, res.LowerLimit, TOL_CI, "ICC(3,1) 95% CI lower")
        AssertAlmostEqual(0.945858, res.UpperLimit, TOL_CI, "ICC(3,1) 95% CI upper")
    End Sub

    <TestMethod>
    Public Sub ICC3k_ShroutFleiss1979_example_matches_reference()
        Dim x(,) As Double = {
        {9, 2, 5, 8},
        {6, 1, 3, 2},
        {8, 4, 6, 8},
        {7, 1, 2, 6},
        {10, 5, 6, 9},
        {6, 2, 4, 7}
    }

        Dim icc As New Agreement.Agreement.IntraclassCorrelation()
        Dim res = icc.ICC3k(x, 0.05)

        AssertAlmostEqual(0.9093155, res.Estimate, TOL, "ICC(3,k) estimate")
        AssertAlmostEqual(0.6756747, res.LowerLimit, TOL_CI, "ICC(3,k) 95% CI lower")
        AssertAlmostEqual(0.9858917, res.UpperLimit, TOL_CI, "ICC(3,k) 95% CI upper")
    End Sub

    <TestMethod>
    Public Sub ICC2_and_ICC3_throw_on_invalid_dimensions()
        Dim icc As New Agreement.Agreement.IntraclassCorrelation()

        ' n < 2 targets (rows)
        Dim x1(,) As Double = {{1, 2, 3, 4}}
        Assert.ThrowsException(Of ArgumentException)(Sub() icc.ICC21(x1))
        Assert.ThrowsException(Of ArgumentException)(Sub() icc.ICC2k(x1))
        Assert.ThrowsException(Of ArgumentException)(Sub() icc.ICC31(x1))
        Assert.ThrowsException(Of ArgumentException)(Sub() icc.ICC3k(x1))

        ' k < 2 raters (cols)
        Dim x2(,) As Double = {{1}, {2}, {3}}
        Assert.ThrowsException(Of ArgumentException)(Sub() icc.ICC21(x2))
        Assert.ThrowsException(Of ArgumentException)(Sub() icc.ICC2k(x2))
        Assert.ThrowsException(Of ArgumentException)(Sub() icc.ICC31(x2))
        Assert.ThrowsException(Of ArgumentException)(Sub() icc.ICC3k(x2))
    End Sub

    <TestMethod>
    Public Sub ICC2_and_ICC3_throw_when_MSE_is_zero_or_negative()
        Dim icc As New Agreement.Agreement.IntraclassCorrelation()

        ' Degenerate: all values identical => MSE = 0 (and MSR may be 0)
        Dim x(,) As Double = {
        {5, 5, 5, 5},
        {5, 5, 5, 5},
        {5, 5, 5, 5}
    }

        Assert.ThrowsException(Of ArgumentException)(Sub() icc.ICC21(x))
        Assert.ThrowsException(Of ArgumentException)(Sub() icc.ICC2k(x))
        Assert.ThrowsException(Of ArgumentException)(Sub() icc.ICC31(x))
        Assert.ThrowsException(Of ArgumentException)(Sub() icc.ICC3k(x))
    End Sub

    <TestMethod>
    Public Sub RepeatabilityCoefficient_OneWay_ICC11_ShroutFleiss_example_matches_reference()
        ' Shrout & Fleiss (1979) example (6 targets x 4 raters), balanced
        Dim x()() As Double = {
        New Double() {9, 2, 5, 8},
        New Double() {6, 1, 3, 2},
        New Double() {8, 4, 6, 8},
        New Double() {7, 1, 2, 6},
        New Double() {10, 5, 6, 9},
        New Double() {6, 2, 4, 7}
    }

        Dim icc As New Agreement.Agreement.IntraclassCorrelation()
        Dim res = icc.RepeatabilityCoefficient_OneWay(x, averageMeasures:=False, alpha:=0.05)

        ' Reference values from your example output:
        ' RC  = 6.937214315
        ' CI  = 5.241847 to 10.25892
        ' SEM = 2.502776236  (printed as SEM in the worksheet)
        AssertAlmostEqual(6.937214315, res.Estimate, TOL_CI, "RC (ICC(1,1))")
        AssertAlmostEqual(5.241847, res.LowerLimit, 0.001, "RC CI lower (ICC(1,1))")
        AssertAlmostEqual(10.25892, res.UpperLimit, 0.001, "RC CI upper (ICC(1,1))")

        ' SEM is stored in StdErr in the RC result
        AssertAlmostEqual(2.502776236, res.StdErr, 0.00001, "SEM (ICC(1,1))")
    End Sub

    <TestMethod>
    Public Sub RepeatabilityCoefficient_OneWay_ICC1k_scales_by_sqrt_n0_in_balanced_design()
        Dim x()() As Double = {
        New Double() {9, 2, 5, 8},
        New Double() {6, 1, 3, 2},
        New Double() {8, 4, 6, 8},
        New Double() {7, 1, 2, 6},
        New Double() {10, 5, 6, 9},
        New Double() {6, 2, 4, 7}
    }

        Dim icc As New Agreement.Agreement.IntraclassCorrelation()

        Dim res11 = icc.RepeatabilityCoefficient_OneWay(x, averageMeasures:=False, alpha:=0.05)
        Dim res1k = icc.RepeatabilityCoefficient_OneWay(x, averageMeasures:=True, alpha:=0.05)

        ' Balanced: n0 = 4, so SEM and RC should scale by 1/sqrt(4) = 1/2
        Dim scale As Double = 1.0 / Math.Sqrt(4.0)

        AssertAlmostEqual(res11.StdErr * scale, res1k.StdErr, 0.00001, "SEM scales by 1/sqrt(n0)")
        AssertAlmostEqual(res11.Estimate * scale, res1k.Estimate, 0.00001, "RC scales by 1/sqrt(n0)")
        AssertAlmostEqual(res11.LowerLimit * scale, res1k.LowerLimit, 0.0001, "RC CI lower scales")
        AssertAlmostEqual(res11.UpperLimit * scale, res1k.UpperLimit, 0.0001, "RC CI upper scales")
    End Sub

    <TestMethod>
    Public Sub RepeatabilityCoefficient_TwoWay_consistency_matches_manual_MSE_and_CI()
        Dim x(,) As Double = {
        {9, 2, 5, 8},
        {6, 1, 3, 2},
        {8, 4, 6, 8},
        {7, 1, 2, 6},
        {10, 5, 6, 9},
        {6, 2, 4, 7}
    }

        Dim icc As New Agreement.Agreement.IntraclassCorrelation()
        Dim res = icc.RepeatabilityCoefficient_TwoWay(x,
                                                  includeRaterVariance:=False, ' ICC(3,·) consistency-style SEM
                                                  averageMeasures:=False,
                                                  alpha:=0.05)

        ' Manual MS computations (no replication)
        Dim MSC As Double, MSR As Double, MSE As Double
        Dim dfC As Integer, dfR As Integer, dfE As Integer
        ComputeTwoWayMeanSquares_NoReplication(x, MSC, MSR, MSE, dfC, dfR, dfE)

        ' Consistency: V = MSE, SEM = sqrt(MSE), RC = z*sqrt(2)*SEM
        Dim alpha As Double = 0.05
        Dim z As Double = distributions.NormSInv(1.0 - alpha / 2.0)
        Dim semExp As Double = Math.Sqrt(MSE)
        Dim rcExp As Double = z * Math.Sqrt(2.0) * semExp

        ' Exact chi-square CI on variance V with dfE
        Dim chiUpper As Double = distributions.ChiSquareInv(1.0 - alpha / 2.0, dfE)
        Dim chiLower As Double = distributions.ChiSquareInv(alpha / 2.0, dfE)
        Dim vL As Double = (dfE * MSE) / chiUpper
        Dim vU As Double = (dfE * MSE) / chiLower
        Dim rcL As Double = z * Math.Sqrt(2.0) * Math.Sqrt(vL)
        Dim rcU As Double = z * Math.Sqrt(2.0) * Math.Sqrt(vU)

        AssertAlmostEqual(semExp, res.StdErr, 0.00001, "SEM (two-way consistency) = sqrt(MSE)")
        AssertAlmostEqual(rcExp, res.Estimate, 0.0001, "RC (two-way consistency)")
        AssertAlmostEqual(rcL, res.LowerLimit, 0.001, "RC CI lower (two-way consistency)")
        AssertAlmostEqual(rcU, res.UpperLimit, 0.001, "RC CI upper (two-way consistency)")
    End Sub

    <TestMethod>
    Public Sub RepeatabilityCoefficient_TwoWay_agreement_includes_rater_variance_and_scales_for_average_measures()
        Dim x(,) As Double = {
        {9, 2, 5, 8},
        {6, 1, 3, 2},
        {8, 4, 6, 8},
        {7, 1, 2, 6},
        {10, 5, 6, 9},
        {6, 2, 4, 7}
    }

        Dim icc As New Agreement.Agreement.IntraclassCorrelation()

        Dim resA1 = icc.RepeatabilityCoefficient_TwoWay(x,
                                                    includeRaterVariance:=True,  ' ICC(2,·) agreement-style SEM
                                                    averageMeasures:=False,
                                                    alpha:=0.05)

        Dim resAk = icc.RepeatabilityCoefficient_TwoWay(x,
                                                    includeRaterVariance:=True,
                                                    averageMeasures:=True,
                                                    alpha:=0.05)

        ' Manual mean squares to verify the SEM definition (point estimate part)
        Dim MSC As Double, MSR As Double, MSE As Double
        Dim dfC As Integer, dfR As Integer, dfE As Integer
        ComputeTwoWayMeanSquares_NoReplication(x, MSC, MSR, MSE, dfC, dfR, dfE)

        Dim n As Integer = x.GetLength(0)
        Dim k As Integer = x.GetLength(1)

        Dim sigmaR2 As Double = (MSC - MSE) / n
        If sigmaR2 < 0.0 Then sigmaR2 = 0.0

        Dim varSingle As Double = MSE + sigmaR2
        Dim semExpSingle As Double = Math.Sqrt(varSingle)
        AssertAlmostEqual(semExpSingle, resA1.StdErr, 0.00001, "SEM agreement includes rater component")

        ' Average-measures should scale by 1/sqrt(k)
        Dim scale As Double = 1.0 / Math.Sqrt(k)
        AssertAlmostEqual(resA1.StdErr * scale, resAk.StdErr, 0.00001, "SEM scales by 1/sqrt(k) for average-measures")
        AssertAlmostEqual(resA1.Estimate * scale, resAk.Estimate, 0.0001, "RC scales by 1/sqrt(k) for average-measures")

        ' Basic sanity: CI ordering and estimate contained
        Assert.IsTrue(resA1.LowerLimit <= resA1.Estimate AndAlso resA1.Estimate <= resA1.UpperLimit, "RC in CI (agreement, single)")
        Assert.IsTrue(resAk.LowerLimit <= resAk.Estimate AndAlso resAk.Estimate <= resAk.UpperLimit, "RC in CI (agreement, average)")
    End Sub


    ' ---------------- Passing–Bablok ----------------

    <TestMethod>
    Public Sub PassingBablok_perfect_line_returns_exact_slope_and_intercept()
        ' Perfect proportional relationship: y = 2x
        Dim x() As Double = {1, 2, 3, 4, 5}
        Dim y() As Double = {2, 4, 6, 8, 10}
        Dim pb As New Agreement.Agreement.PassinbBablok(x, y, "x", "y")
        Dim res = pb.PassingBablokCI()

        AssertAlmostEqual(2.0, res.SlopeCI.Estimate, TOL, "PB slope")
        AssertAlmostEqual(0.0, res.InterceptCI.Estimate, TOL, "PB intercept")

        ' In a perfect line with no ties, the CI should collapse to the estimate.
        AssertAlmostEqual(2.0, res.SlopeCI.LowerLimit, TOL, "PB slope L")
        AssertAlmostEqual(2.0, res.SlopeCI.UpperLimit, TOL, "PB slope U")
        AssertAlmostEqual(0.0, res.InterceptCI.LowerLimit, TOL, "PB intercept L")
        AssertAlmostEqual(0.0, res.InterceptCI.UpperLimit, TOL, "PB intercept U")
    End Sub

    <TestMethod>
    Public Sub PassingBablok_throws_when_no_valid_slopes()
        ' All x equal => no finite slopes except +/-Inf patterns; depending on implementation,
        ' this should result in no valid pairwise slopes and throw.
        Dim x() As Double = {1, 1, 1, 1}
        Dim y() As Double = {2, 2, 2, 2}
        Dim pb As New Agreement.Agreement.PassinbBablok(x, y, "x", "y")
        Assert.ThrowsException(Of InvalidOperationException)(Sub() pb.PassingBablokCI())
    End Sub

    <TestMethod>
    Public Sub PassingBablok_constructor_validates_inputs()
        Dim x() As Double = {1, 2}
        Dim y() As Double = {1}
        Assert.ThrowsException(Of ArgumentException)(Sub()
                                                         Dim tmp = New Agreement.Agreement.PassinbBablok(x, y, "x", "y")
                                                     End Sub)
    End Sub

    ' ---------------- Deming regression ----------------

    <TestMethod>
    Public Sub Deming_point_and_CI_on_perfect_line()
        Dim x() As Double = {1, 2, 3, 4, 5}
        Dim y() As Double = {2, 4, 6, 8, 10}
        Dim d As New Agreement.Agreement.DemingRegression(x, y, "x", "y")
        d.Lambda = 1.0
        d.alpha = 0.05

        Dim est = d.FitPointEstimate()
        AssertAlmostEqual(2.0, est.Slope, TOL, "Deming slope")
        AssertAlmostEqual(0.0, est.Intercept, TOL, "Deming intercept")

        Dim ci = d.FitJackknifeCI()
        AssertAlmostEqual(2.0, ci.SlopeCI.Estimate, TOL, "Deming slope CI estimate")
        AssertAlmostEqual(0.0, ci.InterceptCI.Estimate, TOL, "Deming intercept CI estimate")

        ' Perfect line => jackknife SE should be ~0, CI collapses.
        AssertAlmostEqual(0.0, d.SlopeSE, TOL, "Deming slope SE")
        AssertAlmostEqual(0.0, d.InterceptSE, TOL, "Deming intercept SE")
        AssertAlmostEqual(2.0, ci.SlopeCI.LowerLimit, TOL, "Deming slope L")
        AssertAlmostEqual(2.0, ci.SlopeCI.UpperLimit, TOL, "Deming slope U")
        AssertAlmostEqual(0.0, ci.InterceptCI.LowerLimit, TOL, "Deming intercept L")
        AssertAlmostEqual(0.0, ci.InterceptCI.UpperLimit, TOL, "Deming intercept U")
    End Sub

    <TestMethod>
    Public Sub Deming_constructor_and_parameter_validation()
        Dim x() As Double = {1, 2}
        Dim y() As Double = {1, 2}
        Assert.ThrowsException(Of ArgumentException)(Sub()
                                                         Dim tmp = New Agreement.Agreement.DemingRegression(x, y, "x", "y")
                                                     End Sub)

        Dim x2() As Double = {1, 2, 3}
        Dim y2() As Double = {1, 2, 3}
        Dim d As New Agreement.Agreement.DemingRegression(x2, y2, "x", "y")
        d.Lambda = 0.0
        Assert.ThrowsException(Of ArgumentOutOfRangeException)(Sub() d.FitJackknifeCI())

        d.Lambda = 1.0
        d.alpha = 1.0
        Assert.ThrowsException(Of ArgumentOutOfRangeException)(Sub() d.FitJackknifeCI())
    End Sub

    <TestMethod>
    Public Sub Deming_analyticalCI_throws_when_Sxy_zero()
        ' y constant => Sxy = 0
        Dim x() As Double = {1, 2, 3, 4}
        Dim y() As Double = {5, 5, 5, 5}
        Dim d As New Agreement.Agreement.DemingRegression(x, y, "x", "y")
        d.Lambda = 1.0
        d.alpha = 0.05
        Assert.ThrowsException(Of InvalidOperationException)(Sub() d.DemingAnalyticalCI())
    End Sub

End Class
