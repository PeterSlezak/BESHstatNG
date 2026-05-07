Option Explicit On
Option Infer On
Option Strict Off

Imports System
Imports Microsoft.VisualStudio.TestTools.UnitTesting
Imports BESHStatNG
Imports BESHStatNG.regression
'Imports NLog

<TestClass>
Public Class FamilyLink_Tests

    Private Const TOL As Double = 1.0E-9
    Private Const TOL_DERIV As Double = 1.0E-6
    Private Const H As Double = 0.000001

    '<TestInitialize>
    'Public Sub InitLogger()
    '    ' Avoid NullReferenceException when production code logs via global logger
    '    BESHstatGlobals.gLogger = LogManager.GetLogger("unit-tests")
    'End Sub

    ' -----------------------
    ' Numeric helpers
    ' -----------------------
    Private Shared Function CentralDiff(f As Func(Of Double, Double), x As Double, h As Double) As Double
        Return (f(x + h) - f(x - h)) / (2.0 * h)
    End Function

    Private Shared Function SecondDiff(f As Func(Of Double, Double), x As Double, h As Double) As Double
        Return (f(x + h) - 2.0 * f(x) + f(x - h)) / (h * h)
    End Function

    Private Shared Sub AssertAlmostEqual(expected As Double, actual As Double, tol As Double, msg As String)
        If Double.IsNaN(expected) Then
            Assert.IsTrue(Double.IsNaN(actual), msg & " expected NaN.")
        ElseIf Double.IsInfinity(expected) Then
            Assert.IsTrue(Double.IsInfinity(actual), msg & " expected Infinity.")
        Else
            Assert.AreEqual(expected, actual, tol, msg & $" (expected={expected}, actual={actual})")
        End If
    End Sub

    ' -----------------------
    ' Factory tests
    ' -----------------------
    <TestCategory("FamilyLink")>
    <TestMethod>
    Public Sub LinkUtils_createLink_supported_types()
        Assert.IsInstanceOfType(LinkUtils.createLink("logit"), GetType(Logit))
        Assert.IsInstanceOfType(LinkUtils.createLink("probit"), GetType(Probit))
        Assert.IsInstanceOfType(LinkUtils.createLink("log"), GetType(Log))
        Assert.IsInstanceOfType(LinkUtils.createLink("identity"), GetType(Identity))
        Assert.IsInstanceOfType(LinkUtils.createLink("sqrt"), GetType(Sqrt))
        Assert.IsInstanceOfType(LinkUtils.createLink("inverse"), GetType(Inverse))

        Dim p As regression.Link = LinkUtils.createLink("power", 2.0)
        Assert.IsInstanceOfType(p, GetType(Power))
        Assert.AreEqual(2.0, DirectCast(p, Power).pwr, 0.0, "Power exponent mismatch")
    End Sub

    <TestCategory("FamilyLink")>
    <TestMethod>
    Public Sub LinkUtils_createLink_unsupported_throws()
        Assert.ThrowsException(Of ApplicationException)(
            Sub()
                Dim x = LinkUtils.createLink("does-not-exist")
            End Sub)

        Assert.ThrowsException(Of ApplicationException)(
            Sub()
                Dim x = LinkUtils.createLink("also-does-not-exist", 2.0)
            End Sub)
    End Sub


    <TestCategory("FamilyLink")>
    <TestMethod>
    Public Sub FamilyUtils_createFamily_supported_types()
        Assert.IsInstanceOfType(FamilyUtils.createFamily("binomial"), GetType(Binomial))
        Assert.IsInstanceOfType(FamilyUtils.createFamily("poisson"), GetType(Poisson))
        Assert.IsInstanceOfType(FamilyUtils.createFamily("gaussian"), GetType(Gaussian))
        Assert.IsInstanceOfType(FamilyUtils.createFamily("gamma"), GetType(Gamma))
        Assert.IsInstanceOfType(FamilyUtils.createFamily("negativebinomial", 0.7), GetType(NegativeBinomial))
    End Sub

    <TestCategory("FamilyLink")>
    <TestMethod>
    Public Sub FamilyUtils_createFamily_unsupported_throws()
        Assert.ThrowsException(Of ApplicationException)(
            Sub()
                Dim f = FamilyUtils.createFamily("does-not-exist")
            End Sub)
    End Sub

    ' -----------------------
    ' Link math tests
    ' -----------------------
    <TestCategory("FamilyLink")>
    <TestMethod>
    Public Sub Links_roundtrip_inverse_of_transform()
        Dim links As regression.Link() = {
            New Logit(),
            New Probit(),
            New Log(),
            New Identity(),
            New Sqrt(),
            New Inverse(),
            New Power(2.0)
        }

        For Each l In links
            Dim mus As Double()

            If TypeOf l Is Logit OrElse TypeOf l Is Probit Then
                mus = New Double() {0.2, 0.5, 0.8}
            ElseIf TypeOf l Is Log Then
                mus = New Double() {0.3, 1.0, 2.5}
            ElseIf TypeOf l Is Inverse Then
                mus = New Double() {0.5, 1.0, 3.0}
            Else
                mus = New Double() {0.5, 1.0, 2.0}
            End If

            For Each mu In mus
                Dim eta As Double = l.transform(mu)
                Dim mu2 As Double = l.inverse(eta)
                Assert.AreEqual(mu, mu2, 1.0E-8, $"{l.tostring()} inverse(transform(mu)) mismatch at mu={mu}")
            Next
        Next
    End Sub

    <TestCategory("FamilyLink")>
    <TestMethod>
    Public Sub Links_derivatives_match_numeric()
        Dim links As regression.Link() = {
            New Logit(),
            New Probit(),
            New Log(),
            New Identity(),
            New Sqrt(),
            New Inverse(),
            New Power(2.0)
        }

        For Each l In links
            Dim mu As Double
            If TypeOf l Is Logit OrElse TypeOf l Is Probit Then
                mu = 0.37
            ElseIf TypeOf l Is Log Then
                mu = 1.7
            ElseIf TypeOf l Is Inverse Then
                mu = 2.0
            Else
                mu = 1.3
            End If

            ' g'(mu)
            Dim num1 As Double = CentralDiff(Function(x) l.transform(x), mu, H)
            Dim ana1 As Double = l.deriv(mu)
            Assert.AreEqual(num1, ana1, 1.0E-5, $"{l.tostring()} deriv(mu) mismatch")

            ' g''(mu)
            Dim num2 As Double = SecondDiff(Function(x) l.transform(x), mu, 1.0E-4)
            Dim ana2 As Double = l.deriv2(mu)
            Assert.AreEqual(num2, ana2, 1.0E-3, $"{l.tostring()} deriv2(mu) mismatch")

            ' (g^-1)'(eta)
            Dim eta As Double = l.transform(mu)
            Dim numInv1 As Double = CentralDiff(Function(z) l.inverse(z), eta, H)
            Dim anaInv1 As Double = l.inverseDeriv(eta)
            Assert.AreEqual(numInv1, anaInv1, 1.0E-5, $"{l.tostring()} inverseDeriv(eta) mismatch")

            ' (g^-1)''(eta)
            Dim numInv2 As Double = SecondDiff(Function(z) l.inverse(z), eta, 1.0E-4)
            Dim anaInv2 As Double = l.inverseDeriv2(eta)
            Assert.AreEqual(numInv2, anaInv2, 1.0E-3, $"{l.tostring()} inverseDeriv2(eta) mismatch")
        Next
    End Sub

    <TestCategory("FamilyLink")>
    <TestMethod>
    Public Sub Logit_clips_extreme_mu_to_stay_finite()
        Dim l As New Logit()
        Dim eta1 As Double = l.transform(0.0)
        Dim eta2 As Double = l.transform(1.0)
        Assert.IsFalse(Double.IsNaN(eta1) OrElse Double.IsInfinity(eta1), "Logit.transform(0) should be finite due to clipping.")
        Assert.IsFalse(Double.IsNaN(eta2) OrElse Double.IsInfinity(eta2), "Logit.transform(1) should be finite due to clipping.")
    End Sub

    ' -----------------------
    ' Family math tests
    ' -----------------------
    <TestCategory("FamilyLink")>
    <TestMethod>
    Public Sub Family_variance_and_derivative_match_definitions()
        Dim fPois As New Poisson()
        AssertAlmostEqual(2.3, fPois.Variance(2.3), TOL, "Poisson variance")
        AssertAlmostEqual(1.0, fPois.varianceDeriv(2.3), TOL, "Poisson varianceDeriv")

        Dim fBin As New Binomial()
        AssertAlmostEqual(0.21, fBin.Variance(0.3), TOL, "Binomial variance")
        AssertAlmostEqual(0.4, fBin.varianceDeriv(0.3), TOL, "Binomial varianceDeriv")

        Dim fGam As New Gamma()
        AssertAlmostEqual(9.0, fGam.Variance(3.0), TOL, "Gamma variance")
        AssertAlmostEqual(6.0, fGam.varianceDeriv(3.0), TOL, "Gamma varianceDeriv")

        Dim fGau As New Gaussian()
        AssertAlmostEqual(1.0, fGau.Variance(123.0), TOL, "Gaussian variance")
        AssertAlmostEqual(0.0, fGau.varianceDeriv(123.0), TOL, "Gaussian varianceDeriv")
    End Sub

    <TestCategory("FamilyLink")>
    <TestMethod>
    Public Sub Family_validata_matches_current_constraints()
        Dim fPois As New Poisson()
        Assert.IsTrue(fPois.validata(0.0))
        Assert.IsTrue(fPois.validata(2.5))
        Assert.IsFalse(fPois.validata(-0.1))

        Dim fBin As New Binomial()
        Assert.IsTrue(fBin.validata(0.0))
        Assert.IsTrue(fBin.validata(0.5))
        Assert.IsTrue(fBin.validata(1.0))
        Assert.IsFalse(fBin.validata(-0.01))
        Assert.IsFalse(fBin.validata(1.01))

        Dim fGam As New Gamma()
        Assert.IsTrue(fGam.validata(0.0), "Gamma.validata currently allows 0 in production code (only checks <0).")
        Assert.IsFalse(fGam.validata(-0.1))
    End Sub

    <TestCategory("FamilyLink")>
    <TestMethod>
    Public Sub Family_poisson_loglike_and_deviance_boundary_cases()
        Dim f As New Poisson()

        ' y=0, mu=0 => loglike = 0 per implementation
        Dim ll00 As Double = f.loglike_obs(0.0, 0.0, 1.0)
        AssertAlmostEqual(0.0, ll00, TOL, "Poisson loglike y=0 mu=0")

        ' y>0, mu=0 => -Inf
        Dim ll10 As Double = f.loglike_obs(1.0, 0.0, 1.0)
        Assert.IsTrue(Double.IsNegativeInfinity(ll10), "Poisson loglike y>0 mu=0 should be -Inf")

        ' deviance: y=0 => 2*mu
        Dim dev As Double = f.residDev_(0.0, 3.0)
        AssertAlmostEqual(6.0, dev, TOL, "Poisson deviance y=0")
    End Sub

    <TestCategory("FamilyLink")>
    <TestMethod>
    Public Sub Family_testLink_support_matrix_sane()
        Dim fams As regression.Family() = {New Binomial(), New Poisson(), New Gaussian(), New Gamma(), New NegativeBinomial(0.7)}
        Dim links As String() = {"Log", "Identity", "Inverse", "Sqrt", "Power", "Logit", "Probit"}

        For Each f In fams
            For Each l In links
                Dim ok As Boolean = f.testLink(l)
                ' Simple sanity: at least Log/Identity should be allowed for all families in this codebase
                If l = "Log" OrElse l = "Identity" Then
                    Assert.IsTrue(ok, $"{f.tostring()} should accept {l}")
                End If
            Next
        Next

        ' Known: Binomial accepts Logit, Probit
        Assert.IsTrue((New Binomial()).testLink("Logit"))
        Assert.IsTrue((New Binomial()).testLink("Probit"))
        ' Known: Poisson should NOT accept Logit
        Assert.IsFalse((New Poisson()).testLink("Logit"))
    End Sub


    <TestCategory("FamilyLink")>
    <TestMethod>
    Public Sub PowerBasedLinks_deriv2_matches_numeric_when_fixed()
        Dim links As regression.Link() = {New Sqrt(), New Inverse(), New Power(2.0)}
        For Each l In links
            Dim mu As Double = If(TypeOf l Is Inverse, 2.0, 1.3)
            Dim num2 As Double = SecondDiff(Function(x) l.transform(x), mu, 1.0E-4)
            Dim ana2 As Double = l.deriv2(mu)
            Assert.AreEqual(num2, ana2, 1.0E-3, $"{l.tostring()} deriv2(mu) mismatch")
        Next
    End Sub

End Class
