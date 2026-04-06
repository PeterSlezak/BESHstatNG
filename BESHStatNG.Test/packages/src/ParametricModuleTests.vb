Option Explicit On
Imports Microsoft.VisualStudio.TestTools.UnitTesting
Imports System
Imports System.Globalization
Imports System.Linq
Imports System.Collections.Generic

' Thorough unit tests for Parametric.vb (BESHStatNG)
' Targets .NET Framework 4.8, no external libraries required.

<TestClass()>
Public Class Parametric_Module_Tests

    Private Shared ReadOnly Invariant As CultureInfo = CultureInfo.InvariantCulture

    Private Shared Sub AssertAlmostEqual(expected As Double, actual As Double, tol As Double, msg As String)
        If Double.IsNaN(actual) OrElse Double.IsInfinity(actual) Then
            Assert.Fail($"{msg}: expected {expected} but got {actual}.")
        End If
        Dim diff = Math.Abs(expected - actual)
        If diff > tol Then
            Assert.Fail($"{msg}: expected {expected} but got {actual}. |diff|={diff} > tol={tol}.")
        End If
    End Sub

    Private Shared Function Mean(x() As Double) As Double
        Return x.Average()
    End Function

    Private Shared Function DevSq(x() As Double) As Double
        Dim m = Mean(x)
        Dim s As Double = 0
        For Each v In x
            Dim d = v - m
            s += d * d
        Next
        Return s
    End Function

    Private Shared Function SampleVar(x() As Double) As Double
        Return DevSq(x) / (x.Length - 1.0)
    End Function

    <TestMethod>
    Public Sub UnpairedTtest_compute_matches_closed_form()
        Dim g1() As Double = {1, 2, 3, 4, 5}
        Dim g2() As Double = {2, 3, 4, 5, 6}

        Dim tt As New BESHStatNG.Parametric.UnpairedTtest(New Double()() {g1, g2}, New String() {"G1", "G2"})
        Dim res = tt.compute()

        Dim n1 = g1.Length
        Dim n2 = g2.Length
        Dim m1 = Mean(g1)
        Dim m2 = Mean(g2)
        Dim diff = m1 - m2

        ' pooled SE as implemented in Parametric.vb:
        Dim pooledVar = (DevSq(g1) + DevSq(g2)) / (n1 + n2 - 2.0)
        Dim sePooled = Math.Sqrt(pooledVar * (1.0 / n1 + 1.0 / n2))
        Dim tPooled = diff / sePooled
        Dim dfPooled = n1 + n2 - 2

        ' welch SE/df
        Dim s1 = SampleVar(g1)
        Dim s2 = SampleVar(g2)
        Dim seWelch = Math.Sqrt(s1 / n1 + s2 / n2)
        Dim dfWelch = (seWelch ^ 4) / (((s1 / n1) ^ 2 / (n1 - 1.0)) + ((s2 / n2) ^ 2 / (n2 - 1.0)))
        Dim tWelch = diff / seWelch

        AssertAlmostEqual(tPooled, res.TestStatistics1, 1.0E-12, "Pooled t")
        AssertAlmostEqual(tWelch, res.TestStatistics2, 1.0E-12, "Welch t")
        AssertAlmostEqual(dfPooled, res.DF1, 0.0, "Pooled df")
        AssertAlmostEqual(dfWelch, res.DF2, 1.0E-12, "Welch df")

        ' p-values are computed using your distribution functions; check they are consistent and within [0,1]
        Assert.IsTrue(res.Pvalue >= 0 AndAlso res.Pvalue <= 1, "Two-sided p out of range")
        Assert.IsTrue(res.Pvalue2 >= 0 AndAlso res.Pvalue2 <= 1, "Second p out of range")
    End Sub

    <TestMethod>
    Public Sub PairedTtest_compute_matches_closed_form()
        ' Production PairedTtest expects an (n x 2) matrix:
        '   rows = paired observations, columns = the two paired variables.
        Dim x(,) As Double = {
            {10, 10.5},
            {11, 11.5},
            {12, 12.5},
            {13, 12.0},
            {14, 14.0}
        }

        Dim pt As New BESHStatNG.Parametric.PairedTtest(x, New String() {"pre", "post"})
        Dim res = pt.compute()

        Dim n As Integer = x.GetLength(0)
        Dim d(n - 1) As Double
        For i = 0 To n - 1
            d(i) = x(i, 0) - x(i, 1)
        Next

        Dim md = Mean(d)
        Dim sd = Math.Sqrt(SampleVar(d))
        Dim se = sd / Math.Sqrt(n)
        Dim t = md / se

        AssertAlmostEqual(t, res.TestStatistics1, 0.000000000001, "Paired t")
        AssertAlmostEqual(n - 1, res.DF1, 0.0, "Paired df")
        Assert.IsTrue(res.Pvalue >= 0 AndAlso res.Pvalue <= 1, "Paired p out of range")
    End Sub

    <TestMethod>
    Public Sub OneWayANOVA_compute_matches_closed_form()
        ' 3 groups, balanced
        Dim a() As Double = {1, 2, 3}
        Dim b() As Double = {2, 3, 4}
        Dim c() As Double = {5, 6, 7}

        Dim ow As New BESHStatNG.Parametric.OneWayANOVA(New Double()() {a, b, c}, New String() {"A", "B", "C"})
        Dim tab As Object(,) = ow.compute()

        ' Closed-form ANOVA components (matches Parametric.vb)
        Dim all As Double() = a.Concat(b).Concat(c).ToArray()
        Dim n As Integer = all.Length
        Dim grand As Double = Mean(all)

        Dim ssTot As Double = all.Sum(Function(v) (v - grand) * (v - grand))
        Dim ssB As Double = (a.Sum() ^ 2) / a.Length + (b.Sum() ^ 2) / b.Length + (c.Sum() ^ 2) / c.Length - (all.Sum() ^ 2) / n
        Dim dfB As Integer = 3 - 1
        Dim dfE As Integer = n - 3
        Dim msB As Double = ssB / dfB
        Dim ssE As Double = ssTot - ssB
        Dim msE As Double = ssE / dfE
        Dim f As Double = msB / msE

        AssertAlmostEqual(ssB, CDbl(tab(0, 0)), 0.000000000001, "SS between")
        AssertAlmostEqual(dfB, CDbl(tab(0, 1)), 0.0, "DF between")
        AssertAlmostEqual(msB, CDbl(tab(0, 2)), 0.000000000001, "MS between")
        AssertAlmostEqual(f, CDbl(tab(0, 3)), 0.000000000001, "F")
        Assert.IsTrue(CDbl(tab(0, 4)) >= 0 AndAlso CDbl(tab(0, 4)) <= 1, "ANOVA p out of range")

        AssertAlmostEqual(ssE, CDbl(tab(1, 0)), 0.000000000001, "SS within")
        AssertAlmostEqual(dfE, CDbl(tab(1, 1)), 0.0, "DF within")
        AssertAlmostEqual(msE, CDbl(tab(1, 2)), 0.000000000001, "MS within")

        AssertAlmostEqual(ssTot, CDbl(tab(2, 0)), 0.000000000001, "SS total")
        AssertAlmostEqual(n - 1, CDbl(tab(2, 1)), 0.0, "DF total")
    End Sub

    <TestMethod>
    Public Sub OneWayANOVA_WelshANOVA_runs_and_returns_valid_statistic()
        ' Deliberately unequal variances
        Dim a() As Double = {1, 1, 1, 1, 10}
        Dim b() As Double = {2, 2, 2, 2, 2}
        Dim c() As Double = {3, 3, 3, 3, 3}

        Dim ow As New BESHStatNG.Parametric.OneWayANOVA(New Double()() {a, b, c}, New String() {"A", "B", "C"})
        ow.compute() ' ensure ANOVA table exists for post-hoc dependencies
        Dim welch As BESHStatNG.TestResult = ow.WelshANOVA()

        ' In Parametric.vb WelshANOVA returns:
        '   TestStatistics1 = F*
        '   DF1 = df_error (Satterthwaite-type denominator df)
        '   (numerator df = k-1 is not stored in TestResult)
        Assert.IsTrue(Not Double.IsNaN(welch.TestStatistics1) AndAlso Not Double.IsInfinity(welch.TestStatistics1), "Welch F* should be finite")
        Assert.IsTrue(welch.TestStatistics1 >= 0, "Welch F* should be non-negative")

        Assert.IsTrue(Not Double.IsNaN(welch.DF1) AndAlso Not Double.IsInfinity(welch.DF1), "Welch error df should be finite")
        Assert.IsTrue(welch.DF1 > 0, "Welch error df (stored in DF1) should be > 0")

        ' DF2 is not populated by this implementation; do not assert on it.
        Assert.IsTrue(welch.Pvalue >= 0 AndAlso welch.Pvalue <= 1, "Welch p out of range")
    End Sub

    <TestMethod>
    Public Sub OneWayANOVA_posthoc_outputs_have_expected_shape_and_pairs()
        Dim a() As Double = {1, 2, 3}
        Dim b() As Double = {2, 3, 4}
        Dim c() As Double = {5, 6, 7}

        Dim ow As New BESHStatNG.Parametric.OneWayANOVA(New Double()() {a, b, c}, New String() {"A", "B", "C"})
        ow.compute()

        ' Fisher LSD
        Dim lsd As Object(,) = ow.FisherLSD(False)
        Assert.AreEqual(3, lsd.GetLength(0), "LSD: number of pairwise comparisons for 3 groups should be 3")
        Assert.AreEqual(4, lsd.GetLength(1), "LSD: expected 4 columns")
        Dim names As New HashSet(Of String)(StringComparer.OrdinalIgnoreCase)
        For i = 0 To lsd.GetLength(0) - 1
            names.Add(CStr(lsd(i, 0)))
        Next
        Assert.IsTrue(names.Contains("A vs. B"))
        Assert.IsTrue(names.Contains("A vs. C"))
        Assert.IsTrue(names.Contains("B vs. C"))

        ' Tukey-Kramer
        Dim tk As Object(,) = ow.TukeyKramer()
        Assert.AreEqual(3, tk.GetLength(0), "TK: expected 3 comparisons for 3 groups")
        Assert.IsTrue(tk.GetLength(1) >= 4, "TK: expected at least 4 columns")

        ' Games-Howell (works for unequal variances too)
        Dim gh As Object(,) = ow.GamesHowell()
        Assert.AreEqual(3, gh.GetLength(0), "GH: expected 3 comparisons for 3 groups")
        Assert.IsTrue(gh.GetLength(1) >= 4, "GH: expected at least 4 columns")
    End Sub

    <TestMethod>
    Public Sub OneWayRmANOVA_compute_runs_and_GG_HF_are_valid()
        ' 6 subjects, 3 conditions
        Dim x(,) As Double = {
            {10, 11, 12},
            {10, 12, 12},
            {9, 11, 13},
            {11, 10, 12},
            {10, 11, 11},
            {9, 10, 12}
        }

        Dim rm As New BESHStatNG.Parametric.OneWayRmANOVA(x, New String() {"C1", "C2", "C3"})
        Dim tab As Object(,) = rm.compute()
        Assert.IsNotNull(tab)

        Dim gg As BESHStatNG.TestResult = rm.GreenhouseGeisser()
        Assert.IsTrue(gg.TestStatistics1 >= 0, "GG statistic should be non-negative")
        Assert.IsTrue(gg.Pvalue >= 0 AndAlso gg.Pvalue <= 1, "GG p out of range")

        Dim hf As BESHStatNG.TestResult = rm.HuyhnFeldt()
        Assert.IsTrue(hf.TestStatistics1 >= 0, "HF statistic should be non-negative")
        Assert.IsTrue(hf.Pvalue >= 0 AndAlso hf.Pvalue <= 1, "HF p out of range")
    End Sub

    <TestMethod>
    Public Sub OneWayRmANOVA_posthoc_Tukey_and_TukeyKramerRM2_have_expected_shape()
        Dim x(,) As Double = {
            {10, 11, 12},
            {10, 12, 12},
            {9, 11, 13},
            {11, 10, 12},
            {10, 11, 11},
            {9, 10, 12}
        }

        Dim rm As New BESHStatNG.Parametric.OneWayRmANOVA(x, New String() {"C1", "C2", "C3"})
        rm.compute()

        Dim tuk As Object(,) = rm.Tukey()
        Assert.IsNotNull(tuk)
        Assert.IsTrue(tuk.GetLength(0) > 0, "RM Tukey: expected at least one comparison")
        Assert.IsTrue(tuk.GetLength(1) >= 4, "RM Tukey: expected >=4 columns")

        Dim tk2 As Object(,) = rm.TukeyKramerRM2()
        Assert.IsNotNull(tk2)
        Assert.IsTrue(tk2.GetLength(0) > 0, "RM2 Tukey-Kramer: expected at least one comparison")
        Assert.IsTrue(tk2.GetLength(1) >= 4, "RM2 Tukey-Kramer: expected >=4 columns")
    End Sub

    <TestMethod>
    Public Sub TwoWayNestedANOVA_compute_runs_and_returns_table()
        ' Simple nested design: 2 groups, 2 subgroups per group, 3 observations each
        ' Parametric.TwoWayNestedANOVA expects Object(,) input; we provide numeric with group labels
        Dim x(,) As Object = {
            {"G1", "S1", 10.0},
            {"G1", "S1", 12.0},
            {"G1", "S1", 11.0},
            {"G1", "S2", 9.0},
            {"G1", "S2", 10.0},
            {"G1", "S2", 11.0},
            {"G2", "S3", 20.0},
            {"G2", "S3", 19.0},
            {"G2", "S3", 21.0},
            {"G2", "S4", 18.0},
            {"G2", "S4", 17.0},
            {"G2", "S4", 19.0}
        }

        Dim tw As New BESHStatNG.Parametric.TwoWayNestedANOVA(x, {"var1", "var2", "var3"})
        Dim tab As Object(,) = tw.compute()
        Assert.IsNotNull(tab)
        Assert.IsTrue(tab.GetLength(0) >= 3, "Expected at least 3 rows in nested ANOVA table")
        Assert.IsTrue(tab.GetLength(1) >= 5, "Expected at least 5 columns in nested ANOVA table")
    End Sub

    <TestMethod>
    Public Sub HotellingsT_independent_calculate_and_CI_are_consistent()
        ' Two samples, p=2 variables
        Dim x1(,) As Double = {
            {1, 2},
            {2, 1},
            {3, 2},
            {4, 3},
            {5, 4}
        }
        Dim x2(,) As Double = {
            {1, 1},
            {2, 2},
            {2, 3},
            {3, 3},
            {4, 4}
        }

        Dim ht As New BESHStatNG.Parametric.HotelingsT_independent(x1, x2, New String() {"V1", "V2"})
        Dim resEq As BESHStatNG.TestResult = ht.calculate(True)
        Assert.IsTrue(resEq.TestStatistics1 >= 0, "Hotelling T2 should be non-negative")
        Assert.IsTrue(resEq.Pvalue >= 0 AndAlso resEq.Pvalue <= 1, "p out of range")

        Dim ci = ht.CI(0.05)
        Assert.IsNotNull(ci)
        Assert.IsTrue(ci.Count = 2, "Expected CI list length equals number of variables (2)")
        For Each c In ci
            Assert.IsTrue(c.UpperLimit >= c.LowerLimit, "CI upper should be >= lower")
        Next
    End Sub

    <TestMethod>
    Public Sub HotellingsT_single_calculate_and_CI_valid()
        Dim x(,) As Double = {
            {1, 2},
            {2, 1},
            {3, 2},
            {4, 3},
            {5, 4}
        }
        Dim mu() As Double = {2.5, 2.0}

        Dim ht As New BESHStatNG.Parametric.HotelingsT_single(x, mu, New String() {"V1", "V2"})
        Dim res = ht.calculate()
        Assert.IsTrue(res.TestStatistics1 >= 0, "T2 should be non-negative")
        Assert.IsTrue(res.Pvalue >= 0 AndAlso res.Pvalue <= 1, "p out of range")

        Dim ci = ht.CI(0.05)
        Assert.IsNotNull(ci)
        Assert.IsTrue(ci.Count = 2)
    End Sub

    <TestMethod>
    Public Sub HotellingsT_paired_calculate_and_CI_valid()
        Dim x1(,) As Double = {
            {1, 2},
            {2, 1},
            {3, 2},
            {4, 3},
            {5, 4}
        }
        Dim x2(,) As Double = {
            {1, 1},
            {2, 2},
            {2, 3},
            {3, 3},
            {4, 4}
        }

        Dim ht As New BESHStatNG.Parametric.HotelingsT_paired(x1, x2, New String() {"V1", "V2"})
        Dim res = ht.calculate()
        Assert.IsTrue(res.TestStatistics1 >= 0, "Paired T2 should be non-negative")
        Assert.IsTrue(res.Pvalue >= 0 AndAlso res.Pvalue <= 1, "p out of range")

        Dim ci = ht.CI(0.05)
        Assert.IsNotNull(ci)
        Assert.IsTrue(ci.Count = 2)
    End Sub

    <TestMethod>
    Public Sub UnpairedTtest_invalid_inputs_throw()
        Dim g1() As Double = {1, 2}
        Dim g2() As Double = {1}

        Assert.ThrowsException(Of ArgumentException)(
            Sub()
                Dim a_ = New BESHStatNG.Parametric.UnpairedTtest(New Double()() {g1, g2}, New String() {"G1", "G2"})
            End Sub)
    End Sub

    <TestMethod>
    Public Sub OneWayANOVA_single_group_throws()
        Dim a() As Double = {1, 2, 3}
        Assert.ThrowsException(Of ArgumentException)(
            Sub()
                Dim a_ = New BESHStatNG.Parametric.OneWayANOVA(New Double()() {a}, New String() {"A"})
            End Sub)
    End Sub

End Class
