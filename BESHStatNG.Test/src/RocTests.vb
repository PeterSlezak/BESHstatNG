Option Explicit On
Imports Microsoft.VisualStudio.TestTools.UnitTesting
Imports System.Reflection

' ROC (Receiver Operating Characteristic) tests
' Focus on compute() and wrapResults(). We avoid addROCplot() because it depends on a live Excel instance.

<TestClass()>
Public Class ROC_Tests

    Private Const TOL As Double = 0.000000000001

    ' ---------------------------
    ' Helpers
    ' ---------------------------

    Private Shared Function NewRoc(patients() As Double, controls() As Double) As graphics.ROC
        Dim x()() As Double = {patients, controls}
        Dim names() As String = {"marker"}
        Return New graphics.ROC(x, names)
    End Function

    ''' <summary>
    ''' Fetch a private field or (auto-)property value by name using reflection.
    ''' ROC.vb stores some members as private fields (pAUC, pseAUC, pPvalue, arrays, etc.)
    ''' and some as private auto-properties (pdelongSE, pdelongCI).
    ''' </summary>
    Private Shared Function GetPrivateMember(Of T)(instance As Object, name As String) As T
        Dim tt As Type = instance.GetType()

        Dim f As FieldInfo = tt.GetField(name, BindingFlags.Instance Or BindingFlags.NonPublic)
        If f IsNot Nothing Then
            Return CType(f.GetValue(instance), T)
        End If

        Dim p As PropertyInfo = tt.GetProperty(name, BindingFlags.Instance Or BindingFlags.NonPublic)
        If p IsNot Nothing Then
            Return CType(p.GetValue(instance), T)
        End If

        Throw New AssertFailedException($"Member '{name}' not found on type '{tt.FullName}'.")
    End Function

    ''' <summary>
    ''' Independent (slow but clear) DeLong SE reference implementation using the phi kernel.
    ''' This avoids relying on the same midrank implementation as production code.
    ''' </summary>
    Private Shared Function DeLongReference(patients() As Double, controls() As Double, Optional alpha As Double = 0.05) As (Auc As Double, Se As Double, CiLo As Double, CiHi As Double)
        Dim m As Integer = patients.Length
        Dim n As Integer = controls.Length
        If m < 2 OrElse n < 2 Then Throw New ArgumentException("Need at least 2 observations per group.")

        ' Influence values
        Dim v(m - 1) As Double
        Dim w(n - 1) As Double

        For i As Integer = 0 To m - 1
            Dim s As Double = 0
            For j As Integer = 0 To n - 1
                Dim xi As Double = patients(i)
                Dim yj As Double = controls(j)
                If xi > yj Then
                    s += 1.0
                ElseIf xi = yj Then
                    s += 0.5
                End If
            Next
            v(i) = s / n
        Next

        For j As Integer = 0 To n - 1
            Dim s As Double = 0
            For i As Integer = 0 To m - 1
                Dim xi As Double = patients(i)
                Dim yj As Double = controls(j)
                If xi > yj Then
                    s += 1.0
                ElseIf xi = yj Then
                    s += 0.5
                End If
            Next
            w(j) = s / m
        Next

        Dim auc As Double = v.Average()

        ' Sample variances
        Dim meanV As Double = auc
        Dim meanW As Double = w.Average()

        Dim sV As Double = 0
        For i As Integer = 0 To m - 1
            Dim d As Double = v(i) - meanV
            sV += d * d
        Next
        sV /= (m - 1)

        Dim sW As Double = 0
        For j As Integer = 0 To n - 1
            Dim d As Double = w(j) - meanW
            sW += d * d
        Next
        sW /= (n - 1)

        Dim varAuc As Double = (sV / m) + (sW / n)
        If varAuc < 0 Then varAuc = 0
        Dim se As Double = Math.Sqrt(varAuc)

        Dim z As Double = distributions.NormSInv(1.0 - alpha / 2.0)
        Dim ciLo As Double = auc - z * se
        Dim ciHi As Double = auc + z * se

        Return (auc, se, ciLo, ciHi)
    End Function

    Private Shared Sub AssertClose(expected As Double, actual As Double, tol As Double, Optional msg As String = "")
        If Double.IsNaN(actual) OrElse Double.IsInfinity(actual) Then
            Assert.Fail($"{msg} expected {expected:R} but got {actual:R}.")
        End If
        Dim diff As Double = Math.Abs(expected - actual)
        If diff > tol Then
            Assert.Fail($"{msg} expected {expected:R} but got {actual:R}. |diff|={diff:R} > tol={tol:R}.")
        End If
    End Sub

    Private Shared Function PairwiseAuc(patients() As Double, controls() As Double) As Double
        Dim s As Double = 0.0
        For Each p In patients
            For Each c In controls
                If p > c Then
                    s += 1.0
                ElseIf p = c Then
                    s += 0.5
                End If
            Next
        Next
        Return s / (patients.Length * controls.Length)
    End Function

    Private Shared Sub AssertNonIncreasing(values() As Double, msg As String)
        For i As Integer = 1 To values.Length - 1
            If values(i) > values(i - 1) + 0.000000000000001 Then
                Assert.Fail($"{msg}: sequence increased at i={i}: {values(i - 1):R} -> {values(i):R}.")
            End If
        Next
    End Sub

    ' ---------------------------
    ' Tests: compute()
    ' ---------------------------

    <TestCategory("ROC")>
    <TestMethod()>
    Public Sub ROC_PerfectSeparation_AUC_is_one_and_pvalue_near_zero()
        Dim patients() As Double = {2.0, 3.0, 4.0, 5.0}
        Dim controls() As Double = {0.0, 0.5, 1.0}

        Dim roc = NewRoc(patients, controls)
        roc.compute(alpha:=0.05)

        Dim auc As Double = GetPrivateMember(Of Double)(roc, "pAUC")
        Dim se As Double = GetPrivateMember(Of Double)(roc, "pseAUC")
        Dim pval As Double = GetPrivateMember(Of Double)(roc, "pPvalue")
        Dim ci As ConfidenceIntervalResult = GetPrivateMember(Of ConfidenceIntervalResult)(roc, "pCI")

        Dim delSe As Double = GetPrivateMember(Of Double)(roc, "pdelongSE")
        Dim delCi As ConfidenceIntervalResult = GetPrivateMember(Of ConfidenceIntervalResult)(roc, "pdelongCI")

        AssertClose(1.0, auc, TOL, "AUC")
        Assert.IsTrue(se <= 0.000000000001, $"Expected SE near 0 for perfect separation, got {se:R}.")

        ' Note: ROC.vb uses a *different* (finite-sample) SE specifically for the p-value,
        ' not pseAUC. With small samples, even AUC=1 will not necessarily give an extremely
        ' small p-value.
        Dim n1 As Integer = patients.Length
        Dim n2 As Integer = controls.Length
        Dim seForPvalue As Double = Math.Sqrt((0.25 + (n1 + n2 - 2) * (0.0833333333333)) / (CDbl(n1) * CDbl(n2)))
        Dim expectedP As Double = 2.0 * distributions.PNorm(-Math.Abs(auc - 0.5) / seForPvalue)
        AssertClose(expectedP, pval, 0.00000000001, "p-value (matches ROC.vb normal approx)")
        AssertClose(auc, ci.LowerLimit, 0.0000000001, "CI lower")
        AssertClose(auc, ci.UpperLimit, 0.0000000001, "CI upper")

        Assert.IsTrue(delSe <= 0.000000000001, $"Expected DeLong SE near 0 for perfect separation, got {delSe:R}.")
        AssertClose(auc, delCi.LowerLimit, 0.0000000001, "DeLong CI lower")
        AssertClose(auc, delCi.UpperLimit, 0.0000000001, "DeLong CI upper")

        ' ROC curve endpoints for plotting
        Dim sens() As Double = GetPrivateMember(Of Double())(roc, "parSensitivity")
        Dim fpr() As Double = GetPrivateMember(Of Double())(roc, "par1minusSpec")
        AssertClose(1.0, sens(0), TOL, "Sensitivity(0)")
        AssertClose(1.0, fpr(0), TOL, "FPR(0)")
        AssertClose(0.0, sens(sens.Length - 1), TOL, "Sensitivity(last)")
        AssertClose(0.0, fpr(fpr.Length - 1), TOL, "FPR(last)")
    End Sub

    <TestCategory("ROC")>
    <TestMethod()>
    Public Sub ROC_NoDiscrimination_AUC_is_half_and_pvalue_is_one()
        ' Identical distributions with ties -> Wilcoxon AUC should be 0.5
        Dim patients() As Double = {0.0, 1.0, 2.0, 3.0}
        Dim controls() As Double = {0.0, 1.0, 2.0, 3.0}

        Dim roc = NewRoc(patients, controls)
        roc.compute(alpha:=0.05)

        Dim auc As Double = GetPrivateMember(Of Double)(roc, "pAUC")
        Dim pval As Double = GetPrivateMember(Of Double)(roc, "pPvalue")

        AssertClose(0.5, auc, TOL, "AUC")
        AssertClose(1.0, pval, 0.000000000001, "p-value")
    End Sub

    <TestCategory("ROC")>
    <TestMethod()>
    Public Sub ROC_WithTies_AUC_matches_pairwise_definition_and_cutoff_stats_are_correct()
        Dim patients() As Double = {1.0, 2.0, 2.0}
        Dim controls() As Double = {0.0, 2.0, 3.0}

        Dim expectedAuc As Double = PairwiseAuc(patients, controls) ' 4/9

        Dim roc = NewRoc(patients, controls)
        roc.compute(alpha:=0.05)

        Dim auc As Double = GetPrivateMember(Of Double)(roc, "pAUC")
        AssertClose(expectedAuc, auc, TOL, "AUC")

        ' DeLong SE should match a kernel-based reference implementation
        Dim delSe As Double = GetPrivateMember(Of Double)(roc, "pdelongSE")
        Dim delRef = DeLongReference(patients, controls, alpha:=0.05)
        AssertClose(delRef.Auc, auc, 0.000000000001, "DeLong reference AUC")
        AssertClose(delRef.Se, delSe, 0.0000000001, "DeLong SE")

        Dim cut() As Double = GetPrivateMember(Of Double())(roc, "parCutOff")
        Dim sens() As Double = GetPrivateMember(Of Double())(roc, "parSensitivity")
        Dim spec() As Double = GetPrivateMember(Of Double())(roc, "parSpecificity")
        Dim fpr() As Double = GetPrivateMember(Of Double())(roc, "par1minusSpec")

        ' Unique values are {0,1,2,3} -> 4 cutoffs, 5 ROC points (with endpoints)
        Assert.AreEqual(4, cut.Length, "Cutoff count")
        Assert.AreEqual(5, sens.Length, "Sensitivity array length")
        Assert.AreEqual(4, spec.Length, "Specificity array length")
        Assert.AreEqual(5, fpr.Length, "FPR array length")

        ' Check cutoffs are as constructed: midpoints and max+1
        AssertClose(0.5, cut(0), TOL, "Cutoff(0)")
        AssertClose(1.5, cut(1), TOL, "Cutoff(1)")
        AssertClose(2.5, cut(2), TOL, "Cutoff(2)")
        AssertClose(4.0, cut(3), TOL, "Cutoff(last)")

        ' At cutoff 0.5: sens=1, spec=1/3
        AssertClose(1.0, sens(1), TOL, "Sensitivity@0.5")
        AssertClose(1.0 / 3.0, spec(0), TOL, "Specificity@0.5")

        ' At cutoff 1.5: sens=2/3, spec=1/3
        AssertClose(2.0 / 3.0, sens(2), TOL, "Sensitivity@1.5")
        AssertClose(1.0 / 3.0, spec(1), TOL, "Specificity@1.5")

        ' At cutoff 2.5: sens=0, spec=2/3
        AssertClose(0.0, sens(3), TOL, "Sensitivity@2.5")
        AssertClose(2.0 / 3.0, spec(2), TOL, "Specificity@2.5")

        ' At cutoff 4.0: sens=0, spec=1
        AssertClose(0.0, sens(4), TOL, "Sensitivity@4.0")
        AssertClose(1.0, spec(3), TOL, "Specificity@4.0")

        ' Monotonicity for ROC plotting arrays (they are constructed in descending FPR and sensitivity)
        AssertNonIncreasing(sens, "Sensitivity should be non-increasing")
        AssertNonIncreasing(fpr, "FPR should be non-increasing")

        ' Sanity: all within [0,1]
        For Each v In sens
            Assert.IsTrue(v >= -0.000000000000001 AndAlso v <= 1.0 + 0.000000000000001, $"Sensitivity out of range: {v:R}")
        Next
        For Each v In fpr
            Assert.IsTrue(v >= -0.000000000000001 AndAlso v <= 1.0 + 0.000000000000001, $"FPR out of range: {v:R}")
        Next
        For Each v In spec
            Assert.IsTrue(v >= -0.000000000000001 AndAlso v <= 1.0 + 0.000000000000001, $"Specificity out of range: {v:R}")
        Next
    End Sub

    <TestCategory("ROC")>
    <TestMethod()>
    Public Sub ROC_Swapping_groups_inverts_AUC()
        Dim patients() As Double = {1.0, 2.0, 2.0}
        Dim controls() As Double = {0.0, 2.0, 3.0}

        Dim roc1 = NewRoc(patients, controls)
        roc1.compute()
        Dim auc1 As Double = GetPrivateMember(Of Double)(roc1, "pAUC")

        Dim roc2 = NewRoc(controls, patients)
        roc2.compute()
        Dim auc2 As Double = GetPrivateMember(Of Double)(roc2, "pAUC")

        AssertClose(1.0 - auc1, auc2, 0.000000000001, "AUC swapped groups should be 1 - AUC")
    End Sub

    <TestCategory("ROC")>
    <TestMethod()>
    Public Sub ROC_ConfidenceInterval_matches_normal_approximation_and_alpha_affects_width()
        ' Use an overlap case to ensure SE>0
        Dim patients() As Double = {-1.0, 0.0, 1.0, 2.0, 2.0}
        Dim controls() As Double = {-2.0, 0.0, 0.5, 1.5, 3.0}

        Dim roc95 = NewRoc(patients, controls)
        roc95.compute(alpha:=0.05)
        Dim auc95 As Double = GetPrivateMember(Of Double)(roc95, "pAUC")
        Dim se95 As Double = GetPrivateMember(Of Double)(roc95, "pseAUC")
        Dim ci95 As ConfidenceIntervalResult = GetPrivateMember(Of ConfidenceIntervalResult)(roc95, "pCI")

        Dim delSe95 As Double = GetPrivateMember(Of Double)(roc95, "pdelongSE")
        Dim delCi95 As ConfidenceIntervalResult = GetPrivateMember(Of ConfidenceIntervalResult)(roc95, "pdelongCI")

        Dim z95 As Double = distributions.NormSInv(1.0 - 0.05 / 2.0)
        AssertClose(auc95 - z95 * se95, ci95.LowerLimit, 0.0000000001, "CI95 lower")
        AssertClose(auc95 + z95 * se95, ci95.UpperLimit, 0.0000000001, "CI95 upper")

        AssertClose(auc95 - z95 * delSe95, delCi95.LowerLimit, 0.0000000001, "DeLong CI95 lower")
        AssertClose(auc95 + z95 * delSe95, delCi95.UpperLimit, 0.0000000001, "DeLong CI95 upper")

        Dim roc99 = NewRoc(patients, controls)
        roc99.compute(alpha:=0.01)
        Dim ci99 As ConfidenceIntervalResult = GetPrivateMember(Of ConfidenceIntervalResult)(roc99, "pCI")
        Dim delCi99 As ConfidenceIntervalResult = GetPrivateMember(Of ConfidenceIntervalResult)(roc99, "pdelongCI")

        Dim width95 As Double = ci95.UpperLimit - ci95.LowerLimit
        Dim width99 As Double = ci99.UpperLimit - ci99.LowerLimit
        Assert.IsTrue(width99 > width95, $"Expected 99% CI width > 95% CI width, got {width99:R} vs {width95:R}.")

        Dim delWidth95 As Double = delCi95.UpperLimit - delCi95.LowerLimit
        Dim delWidth99 As Double = delCi99.UpperLimit - delCi99.LowerLimit
        Assert.IsTrue(delWidth99 > delWidth95, $"Expected DeLong 99% CI width > 95% CI width, got {delWidth99:R} vs {delWidth95:R}.")
    End Sub

    ' ---------------------------
    ' Tests: wrapResults()
    ' ---------------------------

    <TestCategory("ROC")>
    <TestMethod()>
    Public Sub ROC_wrapResults_returns_expected_tables_and_core_values()
        Dim patients() As Double = {1.0, 2.0, 2.0}
        Dim controls() As Double = {0.0, 2.0, 3.0}

        Dim roc = NewRoc(patients, controls)
        roc.compute(alpha:=0.05)

        Dim tables = roc.wrapResults()
        Assert.IsNotNull(tables)
        Assert.AreEqual(2, tables.Count, "wrapResults should return exactly 2 ResultTable objects")

        ' Table 1: summary
        Dim t1 As ResultTable = tables(0)
        Dim m1 As Object(,) = t1.returnSelf()
        Assert.AreEqual("Receiver Operating Characteristic (ROC) Curve", CStr(m1(0, 0)), "Summary header")
        Assert.AreEqual("Wilcoxon AUC", CStr(m1(1, 0)), "Summary first label")

        Dim expectedAuc As Double = PairwiseAuc(patients, controls)
        Dim aucFromTable As Double = CDbl(m1(1, 1))
        AssertClose(expectedAuc, aucFromTable, TOL, "AUC in summary table")

        ' DeLong rows should exist and contain a numeric SE
        Assert.AreEqual("DeLong 95% Confidence Interval", CStr(m1(2, 0)), "DeLong CI label")
        Assert.AreEqual("DeLong Standard error", CStr(m1(3, 0)), "DeLong SE label")
        Dim delSeFromTable As Double = CDbl(m1(3, 1))
        Dim delSe As Double = GetPrivateMember(Of Double)(roc, "pdelongSE")
        AssertClose(delSe, delSeFromTable, 0.000000000001, "DeLong SE in summary table")

        ' Hanley–McNeil rows should exist and contain a numeric SE
        Assert.AreEqual("Hanley–McNeil 95% Confidence Interval", CStr(m1(4, 0)), "HM CI label")
        Assert.AreEqual("Hanley–McNeil Standard error", CStr(m1(5, 0)), "HM SE label")
        Dim hmSeFromTable As Double = CDbl(m1(5, 1))
        Dim hmSe As Double = GetPrivateMember(Of Double)(roc, "pseAUC")
        AssertClose(hmSe, hmSeFromTable, 0.000000000001, "Hanley–McNeil SE in summary table")

        ' Table 2: cutoffs
        Dim t2 As ResultTable = tables(1)
        Dim m2 As Object(,) = t2.returnSelf()
        Assert.AreEqual("Cut-Off", CStr(m2(0, 0)), "Cutoff header")
        Assert.AreEqual("Sensitivity", CStr(m2(0, 1)), "Sensitivity header")
        Assert.AreEqual("Specificity", CStr(m2(0, 2)), "Specificity header")

        ' First computed cutoff row: 0.5, sens=1, spec=1/3
        AssertClose(0.5, CDbl(m2(1, 0)), TOL, "Cutoff table cutoff(0)")
        AssertClose(1.0, CDbl(m2(1, 1)), TOL, "Cutoff table sens(0)")
        AssertClose(1.0 / 3.0, CDbl(m2(1, 2)), TOL, "Cutoff table spec(0)")
    End Sub

End Class
