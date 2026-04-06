Option Explicit On
Imports Microsoft.VisualStudio.TestTools.UnitTesting

' Exact numeric unit tests for Assumptions.vb
' Reference values were generated using independent implementations of the same published test formulas
' (and, where applicable, SciPy implementations that match R closely).
' These tests are intended to be stable snapshots: if you change algorithms, update the reference constants.


<TestClass()>
Public Class AssumptionsExactTestHelpers

    Public Shared Sub AssertAlmostEqual(expected As Double, actual As Double, tol As Double, Optional msg As String = Nothing)
        If Double.IsNaN(expected) OrElse Double.IsNaN(actual) Then
            Assert.Fail("NaN encountered. " & If(msg, ""))
        End If
        Dim diff As Double = Math.Abs(expected - actual)
        If diff > tol Then
            Assert.Fail($"Expected {expected} but got {actual}. |diff|={diff} > tol={tol}. " & If(msg, ""))
        End If
    End Sub

    Public Shared Function GetD(res As Object, propName As String) As Double
        Return CDbl(CallByName(res, propName, CallType.Get))
    End Function

    Public Shared Function GetS(res As Object, propName As String) As String
        Dim o = CallByName(res, propName, CallType.Get)
        If o Is Nothing Then Return Nothing
        Return CStr(o)
    End Function

    Public Shared Sub AssertTestResult(res As Object, expectedStat1 As Double, expectedP As Double, tolStat As Double, tolP As Double)
        Assert.IsNotNull(res, "TestResult is Nothing.")
        Dim stat1 As Double = GetD(res, "TestStatistics1")
        Dim p As Double = GetD(res, "Pvalue")
        AssertAlmostEqual(expectedStat1, stat1, tolStat, "TestStatistics1 mismatch.")
        AssertAlmostEqual(expectedP, p, tolP, "Pvalue mismatch.")
    End Sub

End Class

<TestClass()>
Public Class Assumptions_Exact_Numeric_Tests

    <TestCategory("Assumptions")>
    <TestMethod()>
    Public Sub ShapiroWilk_matches_reference()
        Dim err As String = ""
        Dim data() As Double = {-1.2, -0.7, -0.3, 0, 0.15, 0.32, 0.5, 0.9, 1.1, 1.3, -0.1, 0.22, 0.45, -0.55, 0.78, 1.05, -0.95, 0.6, -0.4, 0.12}
        Dim res = assumptions.ShapiroWilk(data, err)
        Assert.IsTrue(String.IsNullOrEmpty(err), "Unexpected error: " & err)
        AssumptionsExactTestHelpers.AssertTestResult(res, 0.97948446567442238, 0.927270910891278, 0.0000000001, 0.0000000001)
    End Sub

    <TestCategory("Assumptions")>
    <TestMethod()>
    Public Sub DAgostino_matches_reference()
        Dim err As String = ""
        Dim data() As Double = {-1.2, -0.7, -0.3, 0, 0.15, 0.32, 0.5, 0.9, 1.1, 1.3, -0.1, 0.22, 0.45, -0.55, 0.78, 1.05, -0.95, 0.6, -0.4, 0.12}
        Dim res = assumptions.DAgostino(data, err)
        Assert.IsNotNull(res, "DAgostino returned Nothing. Err: " & err)
        AssumptionsExactTestHelpers.AssertTestResult(res, 0.67283158079777516, 0.71432603415388807, 0.0000000001, 0.0000000001)
    End Sub

    <TestCategory("Assumptions")>
    <TestMethod()>
    Public Sub AndersonDarlingTEST_matches_reference()
        Dim data() As Double = {-1.2, -0.7, -0.3, 0, 0.15, 0.32, 0.5, 0.9, 1.1, 1.3, -0.1, 0.22, 0.45, -0.55, 0.78, 1.05, -0.95, 0.6, -0.4, 0.12}
        Dim res = assumptions.AndersonDarlingTEST(data)
        AssumptionsExactTestHelpers.AssertTestResult(res, 0.13249538810949948, 0.98099404317838035, 0.000000000001, 0.000000000001)
    End Sub

    <TestCategory("Assumptions")>
    <TestMethod()>
    Public Sub SymmetryTest_MiaoGelGastwirth_matches_reference()
        Dim data() As Double = {-1.2, -0.7, -0.3, 0, 0.15, 0.32, 0.5, 0.9, 1.1, 1.3, -0.1, 0.22, 0.45, -0.55, 0.78, 1.05, -0.95, 0.6, -0.4, 0.12}
        Dim res = assumptions.SymmetryTest(data, "Miao-Gel-Gastwirth")
        AssumptionsExactTestHelpers.AssertTestResult(res, -0.173669463961316, 0.862125237025476, 0.000000000001, 0.000000000001)
    End Sub

    <TestCategory("Assumptions")>
    <TestMethod()>
    Public Sub SymmetryTest_CabilioMasaro_matches_reference()
        Dim data() As Double = {-1.2, -0.7, -0.3, 0, 0.15, 0.32, 0.5, 0.9, 1.1, 1.3, -0.1, 0.22, 0.45, -0.55, 0.78, 1.05, -0.95, 0.6, -0.4, 0.12}
        Dim res = assumptions.SymmetryTest(data, "Cabilio-Masaro")
        AssumptionsExactTestHelpers.AssertTestResult(res, -0.173680305223952, 0.862116716426564, 0.000000000001, 0.000000000001)
    End Sub

    Private Shared Function Groups_3x8() As Double()()
        Dim g1() As Double = {10.2, 9.8, 10, 10.5, 9.7, 10.1, 10.3, 9.9}
        Dim g2() As Double = {10.1, 10, 9.9, 10.2, 10.3, 9.8, 10.1, 10}
        Dim g3() As Double = {9.5, 9.7, 9.8, 9.6, 9.9, 9.4, 9.6, 9.7}
        Return New Double()() {g1, g2, g3}
    End Function

    <TestCategory("Assumptions")>
    <TestMethod()>
    Public Sub BartlettTEST_matches_reference()
        Dim groups = Groups_3x8()
        Dim res = assumptions.BartlettTEST(groups)
        AssumptionsExactTestHelpers.AssertTestResult(res, 2.4523417257880138, 0.29341395161297124, 0.000000000001, 0.000000000001)
    End Sub

    <TestCategory("Assumptions")>
    <TestMethod()>
    Public Sub LeveneTEST_mean_matches_reference()
        Dim groups = Groups_3x8()
        Dim res = assumptions.LeveneTEST(groups, False)
        AssumptionsExactTestHelpers.AssertTestResult(res, 1.7323232323232287, 0.20120727553939535, 0.000000000001, 0.000000000001)
    End Sub

    <TestCategory("Assumptions")>
    <TestMethod()>
    Public Sub LeveneTEST_median_matches_reference()
        Dim groups = Groups_3x8()
        Dim res = assumptions.LeveneTEST(groups, True)
        AssumptionsExactTestHelpers.AssertTestResult(res, 1.7236180904522587, 0.2027169338023673, 0.000000000001, 0.000000000001)
    End Sub

    <TestCategory("Assumptions")>
    <TestMethod()>
    Public Sub FlignerKilleenTEST_matches_reference()
        Dim groups = Groups_3x8()
        Dim res = assumptions.FlignerKilleenTEST(groups)
        AssumptionsExactTestHelpers.AssertTestResult(res, 2.8113973133030843, 0.24519568899734323, 0.0000000001, 0.0000000001)
    End Sub

    <TestCategory("Assumptions")>
    <TestMethod()>
    Public Sub SquaredRanksTestVARIANCE_matches_reference()
        Dim groups = Groups_3x8()
        Dim res = assumptions.SquaredRanksTestVARIANCE(groups)
        AssumptionsExactTestHelpers.AssertTestResult(res, 2.99355476018239, 0.22385038372488703, 0.000000000001, 0.000000000001)
    End Sub

    <TestCategory("Assumptions")>
    <TestMethod()>
    Public Sub BoxM_matches_reference()
        Dim cov(,,) As Double = New Double(2, 1, 1) {}
        cov(0, 0, 0) = 0.016666666666666663
        cov(0, 0, 1) = 0.0050000000000000027
        cov(0, 1, 0) = 0.0050000000000000027
        cov(0, 1, 1) = 0.01691666666666667
        cov(1, 0, 0) = 0.01666666666666667
        cov(1, 0, 1) = 0.0084444444444444437
        cov(1, 1, 0) = 0.0084444444444444437
        cov(1, 1, 1) = 0.0042888888888888881
        cov(2, 0, 0) = 0.0039555555555555628
        cov(2, 0, 1) = 0.0030444444444444503
        cov(2, 1, 0) = 0.0030444444444444503
        cov(2, 1, 1) = 0.0044000000000000089
        Dim ns() As Integer = {10, 10, 10}
        Dim res = assumptions.BoxM(cov, ns)
        AssumptionsExactTestHelpers.AssertTestResult(res, 63.87942360937069, 0.00000000018783344961015503, 0.000000001, 0.000000000001)
    End Sub

    <TestCategory("Assumptions")>
    <TestMethod()>
    Public Sub MauchlyTest_matches_reference()
        Dim data(,) As Double = {
                {1, 1.2, 1.1},
                {0.9, 1.1, 1},
                {1.1, 1.3, 1.25},
                {1.05, 1.15, 1.2},
                {0.95, 1.05, 1},
                {1.2, 1.25, 1.3}
            }
        Dim res = assumptions.MauchlyTest(data)
        AssumptionsExactTestHelpers.AssertTestResult(res, 1.7236099959966962, 0.63169757298113693, 0.000000001, 0.000000001)
    End Sub

    <TestCategory("Assumptions")>
    <TestMethod()>
    Public Sub Grubbs_matches_reference()
        Dim data() As Double = {10, 12, 12, 13, 12, 11, 10, 12, 13, 12, 11, 10, 12, 13, 12, 11, 10, 12, 13, 100}
        Dim res = assumptions.Grubbs(data, 0.05)
        Assert.IsNotNull(res)
        AssumptionsExactTestHelpers.AssertAlmostEqual(2.708245645805754, AssumptionsExactTestHelpers.GetD(res, "TestStatistics1"), 0.000000000001, "Gcrit mismatch")
        AssumptionsExactTestHelpers.AssertAlmostEqual(4.24269371936076, AssumptionsExactTestHelpers.GetD(res, "TestStatistics2"), 0.000000000001, "G mismatch")
        Dim normalized = res.strSpecialInformation.Replace("100.0", "100")
        Assert.AreEqual("Maximum value 100 Is an outlier.", normalized)
    End Sub

    <TestCategory("Assumptions")>
    <TestMethod()>
    Public Sub Rosner_matches_reference()
        Dim data() As Double = {10, 11, 12, 11, 10, 12, 13, 11, 10, 12, 11, 12, 10, 11, 12, 11, 10, 12, 13, 11, 10, 12, 11, 50, 100}
        Dim outliers = assumptions.Rosner(data, 0.05)
        Assert.IsNotNull(outliers, "Expected outliers but got Nothing.")
        CollectionAssert.AreEqual(New Double() {100.0, 50.0}, outliers)
    End Sub

End Class