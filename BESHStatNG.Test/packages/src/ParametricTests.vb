Option Explicit On
Imports Microsoft.VisualStudio.TestTools.UnitTesting
Imports System
Imports System.Globalization
Imports System.Linq

<TestClass()>
Public Class Parametric_Tests

    ' These tests are designed for your .NET Framework 4.8 project (no external libs).
    ' Where the production code formats values via CSng(...).ToString(), the tests
    ' compare against the same Single-rounded values by parsing strings back to Single.

    Private Shared ReadOnly Invariant As CultureInfo = CultureInfo.InvariantCulture

    Private Const TOL_DBL As Double = 0.000000001
    Private Const TOL_SNG As Single = 0.0F
    Const tolCI As Single = 0.00001F


    Private Shared Sub AssertAlmostEqualD(expected As Double, actual As Double, tol As Double, msg As String)
        If Double.IsNaN(actual) OrElse Double.IsInfinity(actual) Then
            Assert.Fail($"{msg}: expected {expected} but got {actual}.")
        End If
        Dim diff As Double = Math.Abs(expected - actual)
        If diff > tol Then
            Assert.Fail($"{msg}: expected {expected} but got {actual}. |diff|={diff} > tol={tol}.")
        End If
    End Sub

    Private Shared Function SampleVariance(x() As Double) As Double
        Dim n As Integer = x.Length
        If n <= 1 Then Return 0.0
        Dim mean As Double = 0.0
        For i = 0 To n - 1
            mean += x(i)
        Next
        mean /= n
        Dim s As Double = 0.0
        For i = 0 To n - 1
            Dim d As Double = x(i) - mean
            s += d * d
        Next
        Return s / (n - 1)
    End Function

    Private Shared Function SampleStDev(x() As Double) As Double
        Return Math.Sqrt(SampleVariance(x))
    End Function

    Private Shared Function ParseSingleWithCulture(s As String) As Single
        ' Parse numbers that may be formatted with CurrentCulture.
        Return Single.Parse(s.Trim(), CultureInfo.CurrentCulture)
    End Function

    Private Shared Sub ParseDiffCi(s As String, ByRef diff As Single, ByRef lcl As Single, ByRef ucl As Single)
        ' Format: "<diff> (<lcl> to <ucl>)"
        Dim p0 As Integer = s.IndexOf("("c)
        Dim p1 As Integer = s.IndexOf(")"c)
        Dim diffStr As String = If(p0 > 0, s.Substring(0, p0).Trim(), s.Trim())
        diff = ParseSingleWithCulture(diffStr)

        Dim inner As String = s.Substring(p0 + 1, p1 - p0 - 1)
        Dim parts() As String = inner.Split(New String() {"to"}, StringSplitOptions.None)
        lcl = ParseSingleWithCulture(parts(0))
        ucl = ParseSingleWithCulture(parts(1))
    End Sub

    Private Shared Function ParsePStop(s As String) As Tuple(Of Single, Boolean)
        ' Format: "<p>" or "<p> stop" (case-insensitive)
        Dim bstop As Boolean = s.IndexOf("stop", StringComparison.OrdinalIgnoreCase) >= 0
        Dim pStr As String = s

        If bstop Then
            Dim idx As Integer = pStr.IndexOf("stop", StringComparison.OrdinalIgnoreCase)
            If idx >= 0 Then
                pStr = (pStr.Substring(0, idx) & pStr.Substring(idx + 4)).Trim()
            End If
        End If

        Return Tuple.Create(ParseSingleWithCulture(pStr), bstop)
    End Function

    ' Helper record types (avoid System.Tuple TRest nesting with 8+ items)
    Private Class CompLSD
        Public i As Integer
        Public j As Integer
        Public diff As Double
        Public t As Double
        Public p As Double
        Public lcl As Double
        Public ucl As Double
        Public absdiff As Double
    End Class

    Private Class CompTK
        Public i As Integer
        Public j As Integer
        Public diff As Double
        Public q As Double
        Public p As Double
        Public lcl As Double
        Public ucl As Double
    End Class

    Private Class CompGH
        Public i As Integer
        Public j As Integer
        Public diff As Double
        Public q As Double
        Public df As Double
        Public p As Double
    End Class




    Private Class CompRM
        Public i As Integer
        Public j As Integer
        Public diff As Double
        Public q As Double
        Public p As Double
        Public lcl As Double
        Public ucl As Double
    End Class

    ' ---------------- One-way ANOVA ----------------

    <TestMethod>
    Public Sub OneWayANOVA_compute_matches_reference()
        Dim x()() As Double = {
            New Double() {1, 2, 3},
            New Double() {2, 3, 4},
            New Double() {5, 6, 7}
        }
        Dim names() As String = {"G1", "G2", "G3"}

        Dim a As New BESHStatNG.Parametric.OneWayANOVA(x, names)
        Dim tbl = a.compute()

        ' Hand-checked reference:
        ' SSb=26, DFb=2, MSb=13, F=13, p=0.006591796875
        ' SSerr=6, DFerr=6, MSerr=1
        ' SStot=32, DFtot=8
        AssertAlmostEqualD(26.0, CDbl(tbl(0, 0)), 0.000000000001, "SS_between")
        AssertAlmostEqualD(2.0, CDbl(tbl(0, 1)), 0.000000000001, "DF_between")
        AssertAlmostEqualD(13.0, CDbl(tbl(0, 2)), 0.000000000001, "MS_between")
        AssertAlmostEqualD(13.0, CDbl(tbl(0, 3)), 0.000000000001, "F")
        AssertAlmostEqualD(0.006591796875, CDbl(tbl(0, 4)), 0.000000001, "p(F)")

        AssertAlmostEqualD(6.0, CDbl(tbl(1, 0)), 0.000000000001, "SS_within")
        AssertAlmostEqualD(6.0, CDbl(tbl(1, 1)), 0.000000000001, "DF_within")
        AssertAlmostEqualD(1.0, CDbl(tbl(1, 2)), 0.000000000001, "MS_within")

        AssertAlmostEqualD(32.0, CDbl(tbl(2, 0)), 0.000000000001, "SS_total")
        AssertAlmostEqualD(8.0, CDbl(tbl(2, 1)), 0.000000000001, "DF_total")
    End Sub

    <TestMethod>
    Public Sub OneWayANOVA_FisherLSD_matches_reference()
        Dim x()() As Double = {
            New Double() {1, 2, 3},
            New Double() {2, 3, 4},
            New Double() {5, 6, 7}
        }
        Dim names() As String = {"G1", "G2", "G3"}

        Dim a As New BESHStatNG.Parametric.OneWayANOVA(x, names)
        a.compute()

        Dim got = a.FisherLSD(False)

        ' Build expected using the same formulas as FisherLSD() but independently.
        Dim means() As Double = {2.0, 3.0, 6.0}
        Dim n() As Integer = {3, 3, 3}
        Dim DFerr As Double = 6.0
        Dim MSerr As Double = 1.0

        Dim comps As New List(Of CompLSD)()
        ' i, j, diff, t, p, lcl, ucl, absdiff
        For i = 0 To 2
            For j = i + 1 To 2
                Dim diff As Double = means(i) - means(j)
                Dim se As Double = Math.Sqrt(MSerr * (1.0 / (n(i) - 1) + 1.0 / (n(j) - 1)))
                Dim t As Double = diff / se
                Dim p As Double = BESHStatNG.Distributions.T_2T(Math.Abs(t), DFerr)
                Dim tcrit As Double = BESHStatNG.Distributions.T_Inv_2T(0.05, DFerr)
                Dim lcl As Double = t - tcrit * se
                Dim ucl As Double = t + tcrit * se
                comps.Add(New CompLSD With {.i = i, .j = j, .diff = diff, .t = t, .p = p, .lcl = lcl, .ucl = ucl, .absdiff = Math.Abs(diff)})
            Next
        Next

        ' Sort by p asc, then absdiff desc (matches QuickSort2D "5,A,8,D")
        comps.Sort(Function(a1, a2)
                       Dim c As Integer = a1.p.CompareTo(a2.p)
                       If c <> 0 Then Return c
                       Return a2.absdiff.CompareTo(a1.absdiff)
                   End Function)

        For k = 0 To comps.Count - 1
            Dim i = comps(k).i, j = comps(k).j
            Dim expLabel As String = names(i) & " vs. " & names(j)
            Assert.AreEqual(expLabel, CStr(got(k, 0)), "Label mismatch (FisherLSD)")

            Dim adiff As Single, alcl As Single, aucl As Single
            ParseDiffCi(CStr(got(k, 1)), adiff, alcl, aucl)

            Assert.AreEqual(CSng(comps(k).diff), adiff, TOL_SNG, "Diff mismatch (FisherLSD)")
            Assert.AreEqual(CSng(comps(k).lcl), alcl, tolCI, "LCI mismatch (FisherLSD)")
            Assert.AreEqual(CSng(comps(k).ucl), aucl, tolCI, "UCI mismatch (FisherLSD)")

            Dim at As Single = ParseSingleWithCulture(CStr(got(k, 2)))
            Assert.AreEqual(CSng(comps(k).t), at, TOL_SNG, "t mismatch (FisherLSD)")

            Dim pStop = ParsePStop(CStr(got(k, 3)))
            Assert.AreEqual(CSng(comps(k).p), pStop.Item1, tolCI, "p mismatch (FisherLSD)")
        Next
    End Sub

    <TestMethod>
    Public Sub OneWayANOVA_TukeyKramer_matches_reference()
        Dim x()() As Double = {
            New Double() {1, 2, 3},
            New Double() {2, 3, 4},
            New Double() {5, 6, 7}
        }
        Dim names() As String = {"G1", "G2", "G3"}

        Dim a As New BESHStatNG.Parametric.OneWayANOVA(x, names)
        a.compute()
        Dim got = a.TukeyKramer()

        Dim means() As Double = {2.0, 3.0, 6.0}
        Dim n() As Integer = {3, 3, 3}
        Dim MSerr As Double = 1.0
        Dim df As Integer = 9 - 3 'sum(n)-k
        Dim ifault As Integer = 0
        Dim qcrit As Double = distributions.QTRNG(0.95, CDbl(df), 3.0, ifault)

        Dim comps As New List(Of CompTK)()
        ' i, j, diff, Q, p, lcl, ucl, Q_for_sort
        For i = 0 To 2
            For j = i + 1 To 2
                Dim diff As Double = means(i) - means(j)
                Dim ad As Double = Math.Abs(diff)
                Dim q As Double = ad / Math.Sqrt(0.5 * MSerr * (1.0 / n(i) + 1.0 / n(j)))
                Dim p As Double = 1.0 - distributions.PRTRNG(q, CDbl(df), 3.0, ifault)
                Dim halfWidth As Double = (qcrit / Math.Sqrt(2.0)) * Math.Sqrt(MSerr) * Math.Sqrt(1.0 / n(i) + 1.0 / n(j))
                Dim lcl As Double = diff - halfWidth
                Dim ucl As Double = diff + halfWidth
                comps.Add(New CompTK With {.i = i, .j = j, .diff = diff, .q = q, .p = p, .lcl = lcl, .ucl = ucl})
            Next
        Next
        ' Sort by p asc, then Q desc (matches QuickSort2D "5,A,2,D")
        comps.Sort(Function(a1, a2)
                       Dim c As Integer = a1.p.CompareTo(a2.p)
                       If c <> 0 Then Return c
                       Return a2.q.CompareTo(a1.q)
                   End Function)

        For k = 0 To comps.Count - 1
            Dim i = comps(k).i, j = comps(k).j
            Dim expLabel As String = names(i) & " vs. " & names(j)
            Assert.AreEqual(expLabel, CStr(got(k, 0)), "Label mismatch (TukeyKramer)")

            Dim adiff As Single, alcl As Single, aucl As Single
            ParseDiffCi(CStr(got(k, 1)), adiff, alcl, aucl)
            Assert.AreEqual(CSng(comps(k).diff), adiff, TOL_SNG, "Diff mismatch (TukeyKramer)")
            Assert.AreEqual(CSng(comps(k).lcl), alcl, tolCI, "LCI mismatch (TukeyKramer)")
            Assert.AreEqual(CSng(comps(k).ucl), aucl, tolCI, "UCI mismatch (TukeyKramer)")

            Dim aQ As Single = ParseSingleWithCulture(CStr(got(k, 2)))
            Assert.AreEqual(CSng(comps(k).q), aQ, tolCI, "Q mismatch (TukeyKramer)")

            Dim pStop = ParsePStop(CStr(got(k, 3)))
            Assert.AreEqual(CSng(comps(k).p), pStop.Item1, tolCI, "p mismatch (TukeyKramer)")
        Next
    End Sub

    <TestMethod>
    Public Sub OneWayANOVA_GamesHowell_matches_reference()
        Dim x()() As Double = {
            New Double() {1, 2, 3},
            New Double() {2, 3, 4},
            New Double() {5, 6, 7}
        }
        Dim names() As String = {"G1", "G2", "G3"}

        Dim a As New BESHStatNG.Parametric.OneWayANOVA(x, names)
        a.compute()
        Dim got = a.GamesHowell()

        Dim means() As Double = {2.0, 3.0, 6.0}
        Dim n() As Integer = {3, 3, 3}
        Dim vars() As Double = {SampleVariance(x(0)), SampleVariance(x(1)), SampleVariance(x(2))}
        Dim ifault As Integer = 0

        Dim comps As New List(Of CompGH)()
        ' i,j, diff, Q, df, p
        For i = 0 To 2
            Dim varNi As Double = vars(i) / n(i)
            For j = i + 1 To 2
                Dim varNj As Double = vars(j) / n(j)
                Dim diff As Double = means(i) - means(j)
                Dim se As Double = Math.Sqrt(0.5 * (varNi + varNj))
                Dim q As Double = Math.Abs(diff) / se
                Dim df As Double = ((varNi + varNj) ^ 2) / (((varNi ^ 2) / (n(i) - 1)) + ((varNj ^ 2) / (n(j) - 1)))
                Dim p As Double = 1.0 - distributions.PRTRNG(q, df, 3.0, ifault)
                comps.Add(New CompGH With {.i = i, .j = j, .diff = diff, .q = q, .df = df, .p = p})
            Next
        Next
        comps.Sort(Function(a1, a2)
                       Dim c As Integer = a1.p.CompareTo(a2.p)
                       If c <> 0 Then Return c
                       Return a2.q.CompareTo(a1.q)
                   End Function)

        For k = 0 To comps.Count - 1
            Dim i = comps(k).i, j = comps(k).j
            Assert.AreEqual(names(i) & " vs. " & names(j), CStr(got(k, 0)), "Label mismatch (GamesHowell)")

            Assert.AreEqual(CSng(comps(k).diff), CSng(got(k, 1)), TOL_SNG, "Diff mismatch (GamesHowell)")
            Assert.AreEqual(CSng(comps(k).q), CSng(got(k, 2)), TOL_SNG, "Q mismatch (GamesHowell)")
            Assert.AreEqual(CSng(comps(k).df), CSng(got(k, 3)), TOL_SNG, "DF mismatch (GamesHowell)")

            Dim pStop = ParsePStop(CStr(got(k, 4)))
            Assert.AreEqual(CSng(comps(k).p), pStop.Item1, tolCI, "p mismatch (GamesHowell)")
        Next
    End Sub

    <TestMethod>
    Public Sub OneWayANOVA_WelshANOVA_matches_reference()
        Dim x()() As Double = {
            New Double() {1, 2, 3},
            New Double() {2, 3, 4},
            New Double() {5, 6, 7}
        }
        Dim names() As String = {"G1", "G2", "G3"}

        Dim a As New BESHStatNG.Parametric.OneWayANOVA(x, names)
        a.compute()
        Dim res = a.WelshANOVA()

        ' Reference computed per WelshANOVA() implementation:
        AssertAlmostEqualD(4.0, res.DF1, 0.000000000001, "Welch DFerr (stored in DF1)")
        AssertAlmostEqualD(11.142857142857142, res.TestStatistics1, 0.000000001, "Welch F")
        AssertAlmostEqualD(0.023156899810964086, res.Pvalue, 0.00000005, "Welch p")
    End Sub

    ' ---------------- Repeated-measures one-way ANOVA ----------------

    <TestMethod>
    Public Sub OneWayRmANOVA_compute_matches_reference()
        Dim x(,) As Double = {
            {1, 2, 3},
            {2, 2, 4},
            {3, 4, 5},
            {4, 3, 6}
        }
        Dim names() As String = {"C1", "C2", "C3"}

        Dim a As New BESHStatNG.Parametric.OneWayRmANOVA(x, names)
        Dim tbl = a.compute()

        AssertAlmostEqualD(9.5, CDbl(tbl(0, 0)), 0.000000000001, "RM SS_between")
        AssertAlmostEqualD(2.0, CDbl(tbl(0, 1)), 0.000000000001, "RM DF_between")
        AssertAlmostEqualD(4.75, CDbl(tbl(0, 2)), 0.000000000001, "RM MS_between")
        AssertAlmostEqualD(15.545454545454524, CDbl(tbl(0, 3)), 0.000000001, "RM F_between")
        AssertAlmostEqualD(0.0042330297170771669, CDbl(tbl(0, 4)), 0.00000005, "RM p_between")
    End Sub

    <TestMethod>
    Public Sub OneWayRmANOVA_Tukey_matches_reference()
        Dim x(,) As Double = {
            {1, 2, 3},
            {2, 2, 4},
            {3, 4, 5},
            {4, 3, 6}
        }
        Dim names() As String = {"C1", "C2", "C3"}

        Dim a As New BESHStatNG.Parametric.OneWayRmANOVA(x, names)
        Dim ANOVAtable = a.compute()
        Dim got = a.Tukey()

        Dim noBlocks As Integer = 4
        Dim noGroups As Integer = 3
        Dim df As Integer = (noGroups * noBlocks) + 1 - noGroups - noBlocks
        Dim ifault As Integer = 0
        Dim qcrit As Double = distributions.QTRNG(0.95, CDbl(df), CDbl(noGroups), ifault)

        ' Means per condition:
        Dim means() As Double = {(1 + 2 + 3 + 4) / 4.0, (2 + 2 + 4 + 3) / 4.0, (3 + 4 + 5 + 6) / 4.0} ' 2.5, 2.75, 4.5
        Dim MSerr As Double = CDbl(ANOVAtable(1, 2))

        Dim comps As New List(Of CompRM)()
        ' i,j,diff,Q,p,lcl,ucl
        For i = 0 To noGroups - 1
            For j = i + 1 To noGroups - 1
                Dim diff As Double = means(i) - means(j)
                Dim ad As Double = Math.Abs(diff)
                Dim q As Double = ad / Math.Sqrt(0.5 * MSerr * (1.0 / noBlocks + 1.0 / noBlocks))
                Dim p As Double = 1.0 - distributions.PRTRNG(q, CDbl(df), CDbl(noGroups), ifault)
                Dim halfWidth As Double = (qcrit / Math.Sqrt(2.0)) * Math.Sqrt(MSerr) * Math.Sqrt(1.0 / noBlocks + 1.0 / noBlocks)
                Dim lcl As Double = diff - halfWidth
                Dim ucl As Double = diff + halfWidth
                comps.Add(New CompRM With {.i = i, .j = j, .diff = diff, .q = q, .p = p, .lcl = lcl, .ucl = ucl})
            Next
        Next
        comps.Sort(Function(a1, a2)
                       Dim c As Integer = a1.p.CompareTo(a2.p)
                       If c <> 0 Then Return c
                       Return a2.q.CompareTo(a1.q)
                   End Function)

        For k = 0 To comps.Count - 1
            Dim i = comps(k).i, j = comps(k).j
            Assert.AreEqual(names(i) & " vs. " & names(j), CStr(got(k, 0)), "Label mismatch (RM Tukey)")

            Dim adiff As Single, alcl As Single, aucl As Single
            ParseDiffCi(CStr(got(k, 1)), adiff, alcl, aucl)
            Assert.AreEqual(CSng(comps(k).diff), adiff, TOL_SNG, "Diff mismatch (RM Tukey)")
            Assert.AreEqual(CSng(comps(k).lcl), alcl, TOL_SNG, "LCI mismatch (RM Tukey)")
            Assert.AreEqual(CSng(comps(k).ucl), aucl, TOL_SNG, "UCI mismatch (RM Tukey)")

            Dim qAct As Single = ParseSingleWithCulture(CStr(got(k, 2)))
            Assert.AreEqual(CSng(comps(k).q), qAct, tolCI, "Q mismatch (RM Tukey)")

            Dim pStop = ParsePStop(CStr(got(k, 3)))
            Assert.AreEqual(CSng(comps(k).p), pStop.Item1, tolCI, "p mismatch (RM Tukey)")
        Next
    End Sub

    <TestMethod>
    Public Sub OneWayRmANOVA_TukeyKramerRM2_matches_reference()
        Dim x(,) As Double = {
            {1, 2, 3},
            {2, 2, 4},
            {3, 4, 5},
            {4, 3, 6}
        }
        Dim names() As String = {"C1", "C2", "C3"}

        Dim a As New BESHStatNG.Parametric.OneWayRmANOVA(x, names)
        a.compute()
        Dim got = a.TukeyKramerRM2()

        Dim noBlocks As Integer = 4
        Dim noGroups As Integer = 3
        Dim df As Integer = noBlocks - 1
        Dim ifault As Integer = 0
        Dim qcrit As Double = distributions.QTRNG(0.95, CDbl(df), CDbl(noGroups), ifault)

        Dim comps As New List(Of CompRM)()
        ' i,j, meanDiff, Q, p, lcl, ucl
        For i = 0 To noGroups - 1
            For j = i + 1 To noGroups - 1
                Dim diffs(noBlocks - 1) As Double
                For b = 0 To noBlocks - 1
                    diffs(b) = x(b, i) - x(b, j)
                Next
                Dim md As Double = diffs.Average()
                Dim sd As Double = SampleStDev(diffs)
                Dim se As Double = sd / Math.Sqrt(noBlocks)
                Dim q As Double = Math.Abs(md) / ((1.0 / Math.Sqrt(2.0)) * se)
                Dim p As Double = 1.0 - distributions.PRTRNG(q, CDbl(df), CDbl(noGroups), ifault)
                Dim halfWidth As Double = (qcrit / Math.Sqrt(2.0)) * se
                Dim lcl As Double = md - halfWidth
                Dim ucl As Double = md + halfWidth
                comps.Add(New CompRM With {.i = i, .j = j, .diff = md, .q = q, .p = p, .lcl = lcl, .ucl = ucl})
            Next
        Next
        ' Sort by Q desc (matches QuickSort2D "0,D")
        comps.Sort(Function(a1, a2) a2.q.CompareTo(a1.q))

        For k = 0 To comps.Count - 1
            Dim i = comps(k).i, j = comps(k).j
            Assert.AreEqual(names(i) & " vs. " & names(j), CStr(got(k, 0)), "Label mismatch (RM2)")

            Dim adiff As Single, alcl As Single, aucl As Single
            ParseDiffCi(CStr(got(k, 1)), adiff, alcl, aucl)
            Assert.AreEqual(CSng(comps(k).diff), adiff, TOL_SNG, "Diff mismatch (RM2)")
            Assert.AreEqual(CSng(comps(k).lcl), alcl, tolCI, "LCI mismatch (RM2)")
            Assert.AreEqual(CSng(comps(k).ucl), aucl, tolCI, "UCI mismatch (RM2)")

            Dim qAct As Single = ParseSingleWithCulture(CStr(got(k, 2)))
            Assert.AreEqual(CSng(comps(k).q), qAct, tolCI, "Q mismatch (RM2)")

            Dim pStop = ParsePStop(CStr(got(k, 3)))
            Assert.AreEqual(CSng(comps(k).p), pStop.Item1, TOL_SNG, "p mismatch (RM2)")
        Next
    End Sub

    <TestMethod>
    Public Sub OneWayRmANOVA_GG_and_HF_match_reference()
        Dim x(,) As Double = {
            {1, 2, 3},
            {2, 2, 4},
            {3, 4, 5},
            {4, 3, 6}
        }
        Dim names() As String = {"C1", "C2", "C3"}

        Dim a As New BESHStatNG.Parametric.OneWayRmANOVA(x, names)
        a.compute()

        Dim gg = a.GreenhouseGeisser()
        Dim hf = a.HuyhnFeldt()

        AssertAlmostEqualD(0.5, gg.TestStatistics1, 0.000000001, "GG epsilon")
        AssertAlmostEqualD(0.029083017747989295, gg.Pvalue, 0.00000005, "GG p")

        AssertAlmostEqualD(0.5, hf.TestStatistics1, 0.000000001, "HF epsilon")
        AssertAlmostEqualD(0.029083017747989295, hf.Pvalue, 0.00000005, "HF p")
    End Sub

    ' ---------------- t-tests ----------------

    <TestMethod>
    Public Sub UnpairedTtest_compute_matches_reference()
        Dim x()() As Double = {
            New Double() {1, 2, 3, 4, 5},
            New Double() {2, 4, 6, 8, 10}
        }
        Dim names() As String = {"A", "B"}

        Dim t As New BESHStatNG.Parametric.UnpairedTtest(x, names)
        Dim res = t.compute()

        AssertAlmostEqualD(-1.8973665961010275, res.TestStatistics1, 0.000000001, "Unpaired pooled t")
        AssertAlmostEqualD(8.0, res.DF1, 0.000000000001, "Unpaired pooled df")
        AssertAlmostEqualD(0.094349772842437674, res.Pvalue, 0.00000005, "Unpaired pooled p")

        AssertAlmostEqualD(-1.8973665961010275, res.TestStatistics2, 0.000000001, "Unpaired Welch t")
        AssertAlmostEqualD(5.8823529411764719, res.DF2, 0.000000001, "Unpaired Welch df")
        AssertAlmostEqualD(0.10753119493062724, res.Pvalue2, 0.00000005, "Unpaired Welch p")
    End Sub

    <TestMethod>
    Public Sub PairedTtest_compute_matches_reference()
        Dim x(,) As Double = {
            {1, 1},
            {2, 1},
            {3, 2},
            {4, 3},
            {5, 5}
        }
        Dim names() As String = {"X1", "X2"}

        Dim t As New BESHStatNG.Parametric.PairedTtest(x, names)
        Dim res = t.compute()

        AssertAlmostEqualD(2.4494897427831779, res.TestStatistics1, 0.000000001, "Paired t")
        AssertAlmostEqualD(4.0, res.DF1, 0.000000000001, "Paired df")
        AssertAlmostEqualD(0.070483996910219962, res.Pvalue, 0.00000005, "Paired p")
    End Sub

    ' ---------------- Hotelling's T^2 + CI ----------------

    <TestMethod>
    Public Sub HotelingsT_independent_equal_cov_matches_reference()
        Dim x1(,) As Double = {
            {1, 2},
            {2, 1},
            {3, 4},
            {4, 3}
        }
        Dim x2(,) As Double = {
            {0, 1},
            {1, 0},
            {2, 2},
            {1, 2}
        }
        Dim names() As String = {"V1", "V2"}

        Dim ht As New BESHStatNG.Parametric.HotelingsT_independent(x1, x2, names)
        Dim res = ht.calculate(True)

        AssertAlmostEqualD(4.1960784313725492, res.TestStatistics1, 0.00000005, "Hotelling independent T2")
        AssertAlmostEqualD(0.26564106156647416, res.Pvalue, 0.00000005, "Hotelling independent p")
    End Sub

    <TestMethod>
    Public Sub HotelingsT_independent_CI_matches_reference()
        Dim x1(,) As Double = {
            {1, 2},
            {2, 1},
            {3, 4},
            {4, 3}
        }
        Dim x2(,) As Double = {
            {0, 1},
            {1, 0},
            {2, 2},
            {1, 2}
        }
        Dim names() As String = {"V1", "V2"}

        Dim ht As New BESHStatNG.Parametric.HotelingsT_independent(x1, x2, names)
        Dim ci = ht.CI(0.05) ' alpha

        Assert.AreEqual(2, ci.Count, "CI length mismatch (independent).")

        Dim n1 As Integer = 4, n2 As Integer = 4, p As Integer = 2
        Dim df2 As Integer = n1 + n2 - 1 - p

        Dim tcrit As Double = Math.Sqrt(BESHStatNG.Distributions.F_Inv(0.05, p, df2) * p * (n1 + n2 - 2) / df2)

        For j = 0 To p - 1
            Dim col1() As Double = {x1(0, j), x1(1, j), x1(2, j), x1(3, j)}
            Dim col2() As Double = {x2(0, j), x2(1, j), x2(2, j), x2(3, j)}
            Dim meanDiff As Double = col1.Average() - col2.Average()
            Dim pooledVar As Double = ((n1 - 1) * SampleVariance(col1) + (n2 - 1) * SampleVariance(col2)) / (n1 + n2 - 2)
            Dim se As Double = Math.Sqrt(pooledVar) * Math.Sqrt(1.0 / n1 + 1.0 / n2)
            Dim lcl As Single = CSng(meanDiff - tcrit * se)
            Dim ucl As Single = CSng(meanDiff + tcrit * se)

            Assert.AreEqual(lcl, ci(j).LowerLimit, tolCI, $"CI LCL mismatch (independent) var {j}.")
            Assert.AreEqual(ucl, ci(j).UpperLimit, tolCI, $"CI UCL mismatch (independent) var {j}.")
        Next
    End Sub

    <TestMethod>
    Public Sub HotelingsT_single_matches_reference()
        Dim x(,) As Double = {
            {1, 2},
            {2, 1},
            {3, 3},
            {4, 4}
        }
        Dim H0() As Double = {0, 0}
        Dim names() As String = {"V1", "V2"}

        Dim ht As New BESHStatNG.Parametric.HotelingsT_single(x, H0, names)
        Dim res = ht.calculate()

        AssertAlmostEqualD(16.666666666666671, res.TestStatistics1, 0.00000005, "Hotelling one-sample T2")
        AssertAlmostEqualD(0.15254237288135586, res.Pvalue, 0.00000005, "Hotelling one-sample p")
    End Sub

    <TestMethod>
    Public Sub HotelingsT_single_CI_matches_reference()
        Dim x(,) As Double = {
            {1, 2},
            {2, 1},
            {3, 3},
            {4, 4}
        }
        Dim H0() As Double = {0, 0}
        Dim names() As String = {"V1", "V2"}

        Dim ht As New BESHStatNG.Parametric.HotelingsT_single(x, H0, names)
        Dim ci = ht.CI(0.05) ' alpha

        Assert.AreEqual(2, ci.Count, "CI length mismatch (single).")

        Dim n As Integer = 4, p As Integer = 2
        Dim tcrit As Double = Math.Sqrt(p * (n - 1) / (n - p) * BESHStatNG.Distributions.F_Inv_RT(0.05, p, n - p))

        For j = 0 To p - 1
            Dim col() As Double = {x(0, j), x(1, j), x(2, j), x(3, j)}
            Dim meanDiff As Double = col.Average() - H0(j)
            Dim se As Double = Math.Sqrt(SampleVariance(col) / n)
            Dim lcl As Single = CSng(meanDiff - tcrit * se)
            Dim ucl As Single = CSng(meanDiff + tcrit * se)

            Assert.AreEqual(lcl, ci(j).LowerLimit, tolCI, $"CI LCL mismatch (single) var {j}.")
            Assert.AreEqual(ucl, ci(j).UpperLimit, tolCI, $"CI UCL mismatch (single) var {j}.")
        Next
    End Sub

    <TestMethod>
    Public Sub HotelingsT_paired_matches_reference()
        Dim x1(,) As Double = {
            {1, 2},
            {2, 1},
            {3, 4},
            {4, 3}
        }
        Dim x2(,) As Double = {
            {1, 1},
            {1, 1},
            {2, 3},
            {3, 2}
        }
        Dim names() As String = {"V1", "V2"}

        Dim ht As New BESHStatNG.Parametric.HotelingsT_paired(x1, x2, names)
        Dim res = ht.calculate()

        AssertAlmostEqualD(27.0, res.TestStatistics1, 0.00000005, "Hotelling paired T2")
        AssertAlmostEqualD(0.1, res.Pvalue, 0.00000005, "Hotelling paired p")
    End Sub

    <TestMethod>
    Public Sub HotelingsT_paired_CI_matches_reference()
        Dim x1(,) As Double = {
            {1, 2},
            {2, 1},
            {3, 4},
            {4, 3}
        }
        Dim x2(,) As Double = {
            {1, 1},
            {1, 1},
            {2, 3},
            {3, 2}
        }
        Dim names() As String = {"V1", "V2"}

        Dim ht As New BESHStatNG.Parametric.HotelingsT_paired(x1, x2, names)
        Dim ci = ht.CI(0.05) ' delegates to one-sample on differences

        Assert.AreEqual(2, ci.Count, "CI length mismatch (paired).")

        Dim n As Integer = 4, p As Integer = 2
        Dim tcrit As Double = Math.Sqrt(p * (n - 1) / (n - p) * BESHStatNG.Distributions.F_Inv_RT(0.05, p, n - p))

        For j = 0 To p - 1
            Dim d() As Double = {x1(0, j) - x2(0, j), x1(1, j) - x2(1, j), x1(2, j) - x2(2, j), x1(3, j) - x2(3, j)}
            Dim meanDiff As Double = d.Average()
            Dim se As Double = Math.Sqrt(SampleVariance(d) / n)
            Dim lcl As Single = CSng(meanDiff - tcrit * se)
            Dim ucl As Single = CSng(meanDiff + tcrit * se)

            Assert.AreEqual(lcl, ci(j).LowerLimit, tolCI, $"CI LCL mismatch (paired) var {j}.")
            Assert.AreEqual(ucl, ci(j).UpperLimit, tolCI, $"CI UCL mismatch (paired) var {j}.")
        Next
    End Sub

End Class