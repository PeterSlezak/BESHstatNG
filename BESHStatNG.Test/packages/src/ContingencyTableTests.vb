Option Explicit On
Imports Microsoft.VisualStudio.TestTools.UnitTesting

' Unit tests for ContingencyTable.vb
' These tests use fixed reference values (computed independently of the implementation)
' and validate key numeric outputs: estimates, test statistics, and p-values.

<TestClass()>
    Public Class ContingencyTableTestHelpers

        Public Shared Sub AssertAlmostEqual(expected As Double, actual As Double, tol As Double, Optional msg As String = "")
            If Double.IsNaN(actual) OrElse Double.IsInfinity(actual) Then
                Assert.Fail($"NaN/Inf encountered. {msg}")
            End If
            Dim diff As Double = Math.Abs(expected - actual)
            If diff > tol Then
                Assert.Fail($"Expected {expected} but got {actual}. |diff|={diff} > tol={tol}. {msg}")
            End If
        End Sub

        Public Shared Sub AssertPValueValid(p As Double, Optional msg As String = "")
            Assert.IsFalse(Double.IsNaN(p), "P-value is NaN. " & msg)
            Assert.IsFalse(Double.IsInfinity(p), "P-value is infinite. " & msg)
            Assert.IsTrue(p >= 0.0 AndAlso p <= 1.0, $"P-value out of range: {p}. {msg}")
        End Sub

    End Class

    ' ---------- FisherExact2x2 reference helpers (match ContingencyTable.FisherExact2x2 definition) ----------

    Friend Module FisherExact2x2Reference

        Private Function LogFact_CT(n As Integer) As Double
            If n < 2 Then Return 0.0#
            Dim s As Double = 0.0#
            For k As Integer = 2 To n
                s += Math.Log(k)
            Next
            Return s
        End Function

        Private Function LogChoose_CT(n As Integer, k As Integer) As Double
            If k < 0 OrElse k > n Then Return Double.NegativeInfinity
            Return LogFact_CT(n) - LogFact_CT(k) - LogFact_CT(n - k)
        End Function

        Private Function HyperProb_CT(a As Integer, b As Integer, c As Integer, d As Integer) As Double
            Dim n As Integer = a + b + c + d
            Dim lp As Double = LogChoose_CT(a + c, a) + LogChoose_CT(b + d, b) - LogChoose_CT(n, a + b)
            Return Math.Exp(lp)
        End Function

        Friend Sub ComputeFisherExact2x2Reference(a0 As Integer, b0 As Integer, c0 As Integer, d0 As Integer,
                                             ByRef pObs As Double,
                                             ByRef oneTailMin As Double,
                                             ByRef twoTail As Double,
                                             ByRef midOneTail As Double,
                                             ByRef midTwoTail As Double)

            ' Replicate production's rotation so min cell becomes "a"
            Dim a As Integer = a0, b As Integer = b0, c As Integer = c0, d As Integer = d0
            Dim mn As Integer = Math.Min(Math.Min(a, b), Math.Min(c, d))
            Dim buffer As Integer
            Do Until a = mn
                buffer = a : a = b : b = d : d = c : c = buffer
            Loop

            pObs = HyperProb_CT(a, b, c, d)

            Dim r1 As Integer = a + b
            Dim c1 As Integer = a + c
            Dim n As Integer = a + b + c + d

            Dim amin As Integer = Math.Max(0, r1 - (n - c1))
            Dim amax As Integer = Math.Min(r1, c1)

            ' Tail 1: a' <= a (production iterates downward from observed a)
            Dim p1 As Double = 0.0#
            For ap As Integer = amin To a
                Dim bp As Integer = r1 - ap
                Dim cp As Integer = c1 - ap
                Dim dp As Integer = n - ap - bp - cp
                Dim p As Double = HyperProb_CT(ap, bp, cp, dp)
                If p <= pObs + 0.000000000000001 Then p1 += p
            Next

            ' Tail 2: a' >= a (production iterates upward from observed a)
            Dim p2 As Double = 0.0#
            For ap As Integer = a To amax
                Dim bp As Integer = r1 - ap
                Dim cp As Integer = c1 - ap
                Dim dp As Integer = n - ap - bp - cp
                Dim p As Double = HyperProb_CT(ap, bp, cp, dp)
                If p <= pObs + 0.000000000000001 Then p2 += p
            Next

            oneTailMin = Math.Min(p1, p2)
            twoTail = p1 + p2 - pObs
            midOneTail = oneTailMin - pObs / 2.0#
            midTwoTail = midOneTail * 2.0#
        End Sub

    End Module


<TestClass()>
Public Class ContingencyTable_Function_Tests

    <TestCategory("ContingencyTable")>
    <TestMethod()>
    Public Sub MantelHaenszel_matches_reference()
        ' Two stratified 2x2 tables stacked as rows:
        ' [a b]
        ' [c d]
        Dim data(,) As Double = {
                {12, 8},
                {5, 15},
                {20, 10},
                {7, 13}
            }

        Dim got = ContingencyTable.MantelHaenszel(data)
        Dim tst As TestResult = got.Item1
        Dim ci As ConfidenceIntervalResult = got.Item2

        Const expChi As Double = 8.3344705068989686#
        Const expP As Double = 0.0038899813945567447#
        Const expOR As Double = 4.041666666666667#
        Const expL As Double = 1.6545682377428139#
        Const expU As Double = 9.872708221891763#

        ContingencyTableTestHelpers.AssertAlmostEqual(expChi, tst.TestStatistics1, 0.000000001, "MH chi-square")
        ContingencyTableTestHelpers.AssertAlmostEqual(expP, tst.Pvalue, 0.0000000005, "MH p-value")
        ContingencyTableTestHelpers.AssertPValueValid(tst.Pvalue, "MH p-value")

        ContingencyTableTestHelpers.AssertAlmostEqual(expOR, ci.Estimate, 0.000000000001, "MH OR estimate")
        ContingencyTableTestHelpers.AssertAlmostEqual(expL, ci.LowerLimit, 0.000000001, "MH OR CI lower")
        ContingencyTableTestHelpers.AssertAlmostEqual(expU, ci.UpperLimit, 0.000000001, "MH OR CI upper")
    End Sub

    <TestCategory("ContingencyTable")>
    <TestMethod()>
    Public Sub SingleProportion_matches_reference()
        Dim got = ContingencyTable.SingleProportion(12, 20)

        Const expEst As Double = 0.6#
        Const expL As Double = 0.38657794231520604#
        Const expU As Double = 0.78119603258580728#

        ContingencyTableTestHelpers.AssertAlmostEqual(expEst, got.Estimate, 0.0, "Single proportion estimate")
        ContingencyTableTestHelpers.AssertAlmostEqual(expL, got.LowerLimit, 0.000000000001, "Single proportion CI lower")
        ContingencyTableTestHelpers.AssertAlmostEqual(expU, got.UpperLimit, 0.000000000001, "Single proportion CI upper")
    End Sub

    <TestCategory("ContingencyTable")>
    <TestMethod()>
    Public Sub TwoIndependentProportions_matches_reference()
        Dim got = ContingencyTable.TwoIndependentProportions(12, 20, 5, 18)

        Const expEst As Double = 0.32222222222222224#
        Const expL As Double = 0.0077571814852357493#
        Const expU As Double = 0.55923354710642192#

        ContingencyTableTestHelpers.AssertAlmostEqual(expEst, got.Estimate, 0.000000000000001, "Two independent proportions estimate")
        ContingencyTableTestHelpers.AssertAlmostEqual(expL, got.LowerLimit, 0.000000000001, "Two independent proportions CI lower")
        ContingencyTableTestHelpers.AssertAlmostEqual(expU, got.UpperLimit, 0.000000000001, "Two independent proportions CI upper")
    End Sub

    <TestCategory("ContingencyTable")>
    <TestMethod()>
    Public Sub FisherExact2x2_matches_reference_definition()
        Dim got = ContingencyTable.FisherExact2x2(1, 9, 11, 3)

        Dim expPobs As Double, expOneTail As Double, expTwoTail As Double, expMidOneTail As Double, expMidTwoTail As Double
        FisherExact2x2Reference.ComputeFisherExact2x2Reference(1, 9, 11, 3, expPobs, expOneTail, expTwoTail, expMidOneTail, expMidTwoTail)

        ' Production outputs:
        '   PvalueLowerSide  -> "one tail" (min of the two probability-based tails)
        '   Pvalue           -> "two tail" (sum of tails minus pObs)
        '   pValueExactLowerSide -> "mid one tail"
        '   Pvalue2          -> "mid two tail"
        Const tol As Double = 0.000000001

        ContingencyTableTestHelpers.AssertAlmostEqual(expOneTail, got.PvalueLowerSide, tol, "Fisher one-tail (min)")
        ContingencyTableTestHelpers.AssertAlmostEqual(expTwoTail, got.Pvalue, tol, "Fisher two-tail")
        ContingencyTableTestHelpers.AssertAlmostEqual(expMidOneTail, got.pValueExactLowerSide, tol, "Fisher mid one-tail")
        ContingencyTableTestHelpers.AssertAlmostEqual(expMidTwoTail, got.Pvalue2, tol, "Fisher mid two-tail")
    End Sub

    <TestCategory("ContingencyTable")>
    <TestMethod()>
    Public Sub PairedProportions_matches_reference()
        ' TotalN pairs, with:
        ' - NoResp1: only response 1
        ' - NoResp2: only response 2
        ' - RespBoth: both responses
        Dim got = ContingencyTable.PairedProportions(100, 10, 20, 30)

        Const expEst As Double = -0.1#
        Const expL As Double = -0.20343426590248823#
        Const expU As Double = 0.0074344034806163341#

        ContingencyTableTestHelpers.AssertAlmostEqual(expEst, got.Estimate, 0.000000000000001, "Paired proportions estimate")
        ContingencyTableTestHelpers.AssertAlmostEqual(expL, got.LowerLimit, 0.0000000001, "Paired proportions CI lower")
        ContingencyTableTestHelpers.AssertAlmostEqual(expU, got.UpperLimit, 0.0000000001, "Paired proportions CI upper")
    End Sub

    <TestCategory("ContingencyTable")>
    <TestMethod()>
    Public Sub Liddell_McNemar_matches_reference()
        Dim tbl(,) As Integer = {{10, 20}, {5, 40}}
        Dim got = ContingencyTable.Liddell_McNemar(tbl)

        Dim tst As TestResult = got.Item1
        Dim ci As ConfidenceIntervalResult = got.Item2

        Const expP As Double = 0.0040773153305053711#
        Const expEst As Double = 4.0#
        Const expL As Double = 1.4567765043175498#
        Const expU As Double = 13.638831336088019#

        ContingencyTableTestHelpers.AssertAlmostEqual(expP, tst.Pvalue, 0.000000000001, "Liddell McNemar p-value")
        ContingencyTableTestHelpers.AssertPValueValid(tst.Pvalue, "Liddell McNemar")

        ContingencyTableTestHelpers.AssertAlmostEqual(expEst, ci.Estimate, 0.0, "Liddell OR estimate")
        ContingencyTableTestHelpers.AssertAlmostEqual(expL, ci.LowerLimit, 0.000000001, "Liddell CI lower")
        ContingencyTableTestHelpers.AssertAlmostEqual(expU, ci.UpperLimit, 0.000000001, "Liddell CI upper")
    End Sub

    <TestCategory("ContingencyTable")>
    <TestMethod()>
    Public Sub Chi2TESTindependence_matches_reference()
        Dim tbl(,) As Integer = {
                {10, 20, 30},
                {6, 9, 17}
            }
        Dim got = ContingencyTable.Chi2TESTindependence(tbl)

        Dim tst As TestResult = got.Item1
        Dim cramerv As Double = got.Item2
        Dim pearson As Double = got.Item3
        Dim phi As Double = got.Item4

        Const expChi As Double = 0.27157465150403504#
        Const expP As Double = 0.873028283380073#
        Const expCramV As Double = 0.054331375704222917#
        Const expPear As Double = 0.054251362453827022#
        Const expPhi As Double = 0.054331375704222917#

        ContingencyTableTestHelpers.AssertAlmostEqual(expChi, tst.TestStatistics1, 0.000000000001, "Chi-square statistic")
        ContingencyTableTestHelpers.AssertAlmostEqual(expP, tst.Pvalue, 0.0000000001, "Chi-square p-value")
        ContingencyTableTestHelpers.AssertPValueValid(tst.Pvalue, "Chi2 independence")

        ContingencyTableTestHelpers.AssertAlmostEqual(expCramV, cramerv, 0.000000000001, "Cramer's V")
        ContingencyTableTestHelpers.AssertAlmostEqual(expPear, pearson, 0.000000000001, "Pearson contingency coefficient")
        ContingencyTableTestHelpers.AssertAlmostEqual(expPhi, phi, 0.000000000001, "Phi")
    End Sub

    <TestCategory("ContingencyTable")>
    <TestMethod()>
    Public Sub OddsRatio_matches_reference()
        Dim tbl(,) As Integer = {{12, 8}, {5, 15}}
        Dim got = ContingencyTable.OddsRatio(tbl)

        Dim woolf As ConfidenceIntervalResult = got.Item1
        Dim cornfield As ConfidenceIntervalResult = got.Item2

        Const expEst As Double = 4.5#
        Const expWoolfL As Double = 1.1656343450444522#
        Const expWoolfU As Double = 17.372514876633765#
        Const expCornL As Double = 0.97421107252635852#
        Const expCornU As Double = 22.15937807578533#

        ContingencyTableTestHelpers.AssertAlmostEqual(expEst, woolf.Estimate, 0.000000000001, "OR estimate")
        ContingencyTableTestHelpers.AssertAlmostEqual(expWoolfL, woolf.LowerLimit, 0.000000002, "Woolf CI lower")
        ContingencyTableTestHelpers.AssertAlmostEqual(expWoolfU, woolf.UpperLimit, 0.00000002, "Woolf CI upper")

        ContingencyTableTestHelpers.AssertAlmostEqual(expEst, cornfield.Estimate, 0.000000000001, "Cornfield OR estimate")
        ContingencyTableTestHelpers.AssertAlmostEqual(expCornL, cornfield.LowerLimit, 0.000000002, "Cornfield CI lower")
        ContingencyTableTestHelpers.AssertAlmostEqual(expCornU, cornfield.UpperLimit, 0.00000002, "Cornfield CI upper")
    End Sub

    <TestCategory("ContingencyTable")>
    <TestMethod()>
    Public Sub RiskRatio_matches_reference()
        Dim tbl(,) As Integer = {{12, 8}, {5, 15}}
        Dim got = ContingencyTable.RiskRatio(tbl)

        Const expEst As Double = 2.0294117647058822#
        Const expL As Double = 1.0720074381157021#
        Const expU As Double = 3.8418689687133791#

        ContingencyTableTestHelpers.AssertAlmostEqual(expEst, got.Estimate, 0.000000000001, "RR estimate")

        ' NOTE: RiskRatio stores CI limits on the log scale in LowerLimit/UpperLimit.
        ContingencyTableTestHelpers.AssertAlmostEqual(expL, Math.Exp(got.LowerLimit), 0.000000002, "RR CI lower (exp)")
        ContingencyTableTestHelpers.AssertAlmostEqual(expU, Math.Exp(got.UpperLimit), 0.00000002, "RR CI upper (exp)")
    End Sub

    <TestCategory("ContingencyTable")>
    <TestMethod()>
    Public Sub CochranArmitage_matches_reference()
        Dim tbl(,) As Integer = {
                {12, 8},
                {15, 15},
                {20, 25},
                {18, 30}
            }

        Dim got = ContingencyTable.CochranArmitage(tbl)

        Const expChiTrend As Double = 3.1564344941956883
        Const expPTrend As Double = 0.075628194026305592
        Const expChiDepart As Double = 0.043750690989494689
        Const expPDepart As Double = 0.97836218470435754

        ContingencyTableTestHelpers.AssertAlmostEqual(expChiTrend, got.TestStatistics1, 0.000000000001, "Cochran-Armitage chi-square")
        ContingencyTableTestHelpers.AssertAlmostEqual(expPTrend, got.Pvalue, 0.0000000001, "Cochran-Armitage p-value")
        ContingencyTableTestHelpers.AssertPValueValid(got.Pvalue, "Cochran-Armitage")

        ContingencyTableTestHelpers.AssertAlmostEqual(expChiDepart, got.TestStatistics2, 0.000000001, "Departure chi-square")
        ContingencyTableTestHelpers.AssertAlmostEqual(expPDepart, got.Pvalue2, 0.0000000001, "Departure p-value")
        ContingencyTableTestHelpers.AssertPValueValid(got.Pvalue2, "Departure from linear trend")
    End Sub

    <TestCategory("ContingencyTable")>
    <TestMethod()>
    Public Sub cTableORDINALassoc_matches_reference()
        Dim tbl(,) As Integer = {
                {10, 20, 30},
                {6, 9, 17},
                {5, 8, 15}
            }

        Dim got = ContingencyTable.cTableORDINALassoc(tbl)

        Dim taub As TestResult = got.Item1
        Dim tauC As TestResult = got.Item2
        Dim gamma As TestResult = got.Item3
        Dim somers As TestResult = got.Item4

        Const expTaub As Double = 0.017365521707076833#
        Const expTaubSE As Double = 0.082334804371603745#
        Const expTaubP As Double = 0.83295477012498065#

        Const expTauC As Double = 0.016041666666666666#
        Const expTauCSE As Double = 0.07605803667022723#
        Const expTauCP As Double = 0.83295477012498065#

        Const expGamma As Double = 0.028215463539758154#
        Const expGammaSE As Double = 0.13377741884100064#
        Const expGammaP As Double = 0.83295477012498065#

        Const expSomers As Double = 0.017126334519572954#
        Const expSomersSE As Double = 0.081200656301523025#
        Const expSomersP As Double = 0.832954578399685#

        ContingencyTableTestHelpers.AssertAlmostEqual(expTaub, taub.TestStatistics1, 0.000000000001, "Kendall tau-b")
        ContingencyTableTestHelpers.AssertAlmostEqual(expTaubSE, taub.DF1, 0.000000000001, "Kendall tau-b SE")
        ContingencyTableTestHelpers.AssertAlmostEqual(expTaubP, taub.Pvalue, 0.0000000001, "Kendall tau-b p-value")

        ContingencyTableTestHelpers.AssertAlmostEqual(expTauC, tauC.TestStatistics1, 0.000000000001, "Stuart tau-c")
        ContingencyTableTestHelpers.AssertAlmostEqual(expTauCSE, tauC.DF1, 0.000000000001, "Stuart tau-c SE")
        ContingencyTableTestHelpers.AssertAlmostEqual(expTauCP, tauC.Pvalue, 0.0000000001, "Stuart tau-c p-value")

        ContingencyTableTestHelpers.AssertAlmostEqual(expGamma, gamma.TestStatistics1, 0.000000000001, "Goodman-Kruskal gamma")
        ContingencyTableTestHelpers.AssertAlmostEqual(expGammaSE, gamma.DF1, 0.000000000001, "Gamma SE")
        ContingencyTableTestHelpers.AssertAlmostEqual(expGammaP, gamma.Pvalue, 0.0000000001, "Gamma p-value")

        ContingencyTableTestHelpers.AssertAlmostEqual(expSomers, somers.TestStatistics1, 0.000000000001, "Somers' D")
        ContingencyTableTestHelpers.AssertAlmostEqual(expSomersSE, somers.DF1, 0.000000000001, "Somers' D SE")
        ContingencyTableTestHelpers.AssertAlmostEqual(expSomersP, somers.Pvalue, 0.0000000001, "Somers' D p-value")

        ContingencyTableTestHelpers.AssertPValueValid(taub.Pvalue, "taub")
        ContingencyTableTestHelpers.AssertPValueValid(tauC.Pvalue, "tauC")
        ContingencyTableTestHelpers.AssertPValueValid(gamma.Pvalue, "gamma")
        ContingencyTableTestHelpers.AssertPValueValid(somers.Pvalue, "somers")
    End Sub

    <TestCategory("ContingencyTable")>
    <TestMethod()>
    Public Sub MantelHaenszel_wrong_dimensions_throws()
        Dim bad(,) As Double = {{1, 2, 3}, {4, 5, 6}} 'not 2 columns
        Assert.ThrowsException(Of ArgumentException)(
                Sub() ContingencyTable.MantelHaenszel(bad)
            )
    End Sub

End Class
