Option Explicit On
Option Infer On
Option Strict Off

Imports System
Imports System.Collections.Generic
Imports Microsoft.VisualStudio.TestTools.UnitTesting
Imports BESHStatNG.regression

<TestClass()>
Public Class MixedModelCovarianceDerivativeProviderTests

    <TestMethod()>
    Public Sub RDerivativeProviders_MatchFiniteDifferences_ForFourVisitResidualStructures()
        Dim data As MixedModelBlockData = CreateVisitData(4)

        AssertRDerivativesMatchFiniteDifferences(New IdentityR(),
                                                 New Double() {Math.Log(1.7)},
                                                 data,
                                                 "IdentityR 4-visit")

        AssertRDerivativesMatchFiniteDifferences(New DiagonalHeterogeneousR(),
                                                 New Double() {Math.Log(0.8), Math.Log(1.1), Math.Log(1.5), Math.Log(2.0)},
                                                 data,
                                                 "Diagonal heterogeneous 4-visit")

        AssertRDerivativesMatchFiniteDifferences(New CompoundSymmetryR(),
                                                 New Double() {Math.Log(1.4), AtanhForTest(0.32)},
                                                 data,
                                                 "Compound symmetry moderate correlation")

        AssertRDerivativesMatchFiniteDifferences(New HeterogeneousCSR(),
                                                 New Double() {Math.Log(0.9), Math.Log(1.2), Math.Log(1.6), Math.Log(2.1), AtanhForTest(0.28)},
                                                 data,
                                                 "Heterogeneous CS 4-visit")

        AssertRDerivativesMatchFiniteDifferences(New AR1R(),
                                                 New Double() {Math.Log(1.6), AtanhForTest(0.41)},
                                                 data,
                                                 "AR1 moderate correlation")

        AssertRDerivativesMatchFiniteDifferences(New HeterogeneousAR1R(),
                                                 New Double() {Math.Log(0.75), Math.Log(1.05), Math.Log(1.45), Math.Log(1.95), AtanhForTest(0.36)},
                                                 data,
                                                 "Heterogeneous AR1 4-visit")

        AssertRDerivativesMatchFiniteDifferences(New ToeplitzR(),
                                                 New Double() {Math.Log(1.25), AtanhForTest(0.22), AtanhForTest(-0.08), AtanhForTest(0.04)},
                                                 data,
                                                 "Toeplitz 4-visit",
                                                 relativeTolerance:=0.0002,
                                                 absoluteTolerance:=0.000005)

        AssertRDerivativesMatchFiniteDifferences(New HeterogeneousToeplitzR(),
                                                 New Double() {Math.Log(0.85), Math.Log(1.05), Math.Log(1.35), Math.Log(1.7), AtanhForTest(0.2), AtanhForTest(-0.06), AtanhForTest(0.03)},
                                                 data,
                                                 "Heterogeneous Toeplitz 4-visit",
                                                 relativeTolerance:=0.0002,
                                                 absoluteTolerance:=0.000005)
    End Sub

    <TestMethod()>
    Public Sub RDerivativeProviders_AR1AndCS_NearZeroCorrelations_MatchFiniteDifferences()
        Dim data As MixedModelBlockData = CreateVisitData(4)

        AssertRDerivativesMatchFiniteDifferences(New CompoundSymmetryR(),
                                                 New Double() {Math.Log(1.4), AtanhForTest(0.0)},
                                                 data,
                                                 "Compound symmetry zero correlation")

        AssertRDerivativesMatchFiniteDifferences(New HeterogeneousCSR(),
                                                 New Double() {Math.Log(0.9), Math.Log(1.2), Math.Log(1.6), Math.Log(2.1), AtanhForTest(0.0)},
                                                 data,
                                                 "Heterogeneous CS zero correlation")

        AssertRDerivativesMatchFiniteDifferences(New AR1R(),
                                                 New Double() {Math.Log(1.6), AtanhForTest(0.0)},
                                                 data,
                                                 "AR1 zero correlation")

        AssertRDerivativesMatchFiniteDifferences(New HeterogeneousAR1R(),
                                                 New Double() {Math.Log(0.75), Math.Log(1.05), Math.Log(1.45), Math.Log(1.95), AtanhForTest(0.0)},
                                                 data,
                                                 "Heterogeneous AR1 zero correlation")
    End Sub

    <TestMethod()>
    Public Sub RDerivativeProviders_UnstructuredFourAndSixVisitCases_MatchFiniteDifferences()
        Dim data4 As MixedModelBlockData = CreateVisitData(4)
        Dim theta4() As Double = {
            Math.Log(1.0),
            0.12, Math.Log(1.1),
            -0.08, 0.16, Math.Log(0.95),
            0.06, -0.04, 0.11, Math.Log(1.25)
        }

        AssertRDerivativesMatchFiniteDifferences(New UnstructuredR(),
                                                 theta4,
                                                 data4,
                                                 "Unstructured 4-visit with missing-visit subject")

        Dim data6 As MixedModelBlockData = CreateVisitData(6)
        Dim theta6() As Double = CreateUnstructuredTheta(6)

        AssertRDerivativesMatchFiniteDifferences(New UnstructuredR(),
                                                 theta6,
                                                 data6,
                                                 "Unstructured 6-visit with missing-visit subject",
                                                 relativeTolerance:=0.00008,
                                                 absoluteTolerance:=0.000002)
    End Sub

    <TestMethod()>
    Public Sub MixedModelCovarianceStructureFactories_AcceptCommonAliasesAndReturnExpectedParameterCounts()
        Dim rData As MixedModelBlockData = CreateVisitData(4)

        Assert.IsInstanceOfType(MixedModelRStructUtils.createMixedModelRStruct("ID"), GetType(IdentityR))
        Assert.IsInstanceOfType(MixedModelRStructUtils.createMixedModelRStruct("TOEP"), GetType(ToeplitzR))
        Assert.IsInstanceOfType(MixedModelRStructUtils.createMixedModelRStruct("TOEPH"), GetType(HeterogeneousToeplitzR))
        Assert.IsInstanceOfType(MixedModelRStructUtils.createMixedModelRStruct("HAR1"), GetType(HeterogeneousAR1R))

        Assert.AreEqual(4, (New ToeplitzR()).ParamCount(rData), "R-side TOEP should use one variance plus lag correlations.")
        Assert.AreEqual(7, (New HeterogeneousToeplitzR()).ParamCount(rData), "R-side TOEPH should use visit variances plus lag correlations.")

        Dim q As Integer = 4
        Assert.IsInstanceOfType(MixedModelGStructUtils.createMixedModelGStruct("VC"), GetType(VarianceComponentsRandomEffects))
        Assert.IsInstanceOfType(MixedModelGStructUtils.createMixedModelGStruct("ID"), GetType(IdentityRandomEffects))
        Assert.IsInstanceOfType(MixedModelGStructUtils.createMixedModelGStruct("CSH"), GetType(HeterogeneousCompoundSymmetryRandomEffects))
        Assert.IsInstanceOfType(MixedModelGStructUtils.createMixedModelGStruct("ARH1"), GetType(HeterogeneousAutoregressiveRandomEffects))
        Assert.IsInstanceOfType(MixedModelGStructUtils.createMixedModelGStruct("TOEPH"), GetType(HeterogeneousToeplitzRandomEffects))

        Assert.AreEqual(1, (New IdentityRandomEffects()).ParamCount(q))
        Assert.AreEqual(q, (New VarianceComponentsRandomEffects()).ParamCount(q))
        Assert.AreEqual(2, (New CompoundSymmetryRandomEffects()).ParamCount(q))
        Assert.AreEqual(q + 1, (New HeterogeneousCompoundSymmetryRandomEffects()).ParamCount(q))
        Assert.AreEqual(2, (New AutoregressiveRandomEffects()).ParamCount(q))
        Assert.AreEqual(q + 1, (New HeterogeneousAutoregressiveRandomEffects()).ParamCount(q))
        Assert.AreEqual(q, (New ToeplitzRandomEffects()).ParamCount(q))
        Assert.AreEqual(2 * q - 1, (New HeterogeneousToeplitzRandomEffects()).ParamCount(q))
    End Sub

    <TestMethod()>
    Public Sub RDerivativeProvider_RejectsUnsupportedInputs_WithDiagnosticMessage()
        Dim data As MixedModelBlockData = CreateVisitData(4)
        Dim block As MixedModelSubjectBlock = data.GetBlock(0)
        Dim derivs As Double(,,) = Nothing
        Dim msg As String = Nothing

        Assert.IsFalse(MixedModelCovarianceDerivatives.TryBuildRDerivatives(Nothing,
                                                                            New Double() {0.0},
                                                                            block,
                                                                            data,
                                                                            derivs,
                                                                            msg))
        Assert.IsTrue(msg.IndexOf("missing", StringComparison.OrdinalIgnoreCase) >= 0)
        Assert.IsNull(derivs)
    End Sub

    <TestMethod()>
    Public Sub GDerivativeProviders_MatchFiniteDifferences_ForRandomEffectsStructures()
        AssertGDerivativesMatchFiniteDifferences(New RandomIntercept(),
                                                 New Double() {Math.Log(0.85)},
                                                 CreateRandomEffectsData(1),
                                                 "Random intercept G-side derivative")

        AssertGDerivativesMatchFiniteDifferences(New RandomInterceptSlope(),
                                                 New Double() {Math.Log(0.7), Math.Log(0.3), AtanhForTest(0.18)},
                                                 CreateRandomEffectsData(2),
                                                 "Random intercept/slope G-side derivatives")

        AssertGDerivativesMatchFiniteDifferences(New VarianceComponentsRandomEffects(),
                                                 New Double() {Math.Log(0.7), Math.Log(0.4), Math.Log(0.25)},
                                                 CreateRandomEffectsData(3),
                                                 "Variance-components G-side derivatives")

        AssertGDerivativesMatchFiniteDifferences(New IdentityRandomEffects(),
                                                 New Double() {Math.Log(0.55)},
                                                 CreateRandomEffectsData(3),
                                                 "Identity G-side derivatives")

        AssertGDerivativesMatchFiniteDifferences(New CompoundSymmetryRandomEffects(),
                                                 New Double() {Math.Log(0.6), AtanhForTest(0.18)},
                                                 CreateRandomEffectsData(3),
                                                 "Compound-symmetry G-side derivatives",
                                                 relativeTolerance:=0.00008,
                                                 absoluteTolerance:=0.000002)

        AssertGDerivativesMatchFiniteDifferences(New HeterogeneousCompoundSymmetryRandomEffects(),
                                                 New Double() {Math.Log(0.72), Math.Log(0.38), Math.Log(0.21), AtanhForTest(0.16)},
                                                 CreateRandomEffectsData(3),
                                                 "Heterogeneous compound-symmetry G-side derivatives",
                                                 relativeTolerance:=0.00008,
                                                 absoluteTolerance:=0.000002)

        AssertGDerivativesMatchFiniteDifferences(New AutoregressiveRandomEffects(),
                                                 New Double() {Math.Log(0.6), AtanhForTest(0.24)},
                                                 CreateRandomEffectsData(3),
                                                 "Autoregressive G-side derivatives",
                                                 relativeTolerance:=0.00008,
                                                 absoluteTolerance:=0.000002)

        AssertGDerivativesMatchFiniteDifferences(New HeterogeneousAutoregressiveRandomEffects(),
                                                 New Double() {Math.Log(0.72), Math.Log(0.38), Math.Log(0.21), AtanhForTest(0.19)},
                                                 CreateRandomEffectsData(3),
                                                 "Heterogeneous autoregressive G-side derivatives",
                                                 relativeTolerance:=0.00008,
                                                 absoluteTolerance:=0.000002)

        AssertGDerivativesMatchFiniteDifferences(New ToeplitzRandomEffects(),
                                                 New Double() {Math.Log(0.58), AtanhForTest(0.18), AtanhForTest(-0.05)},
                                                 CreateRandomEffectsData(3),
                                                 "Toeplitz G-side derivatives",
                                                 relativeTolerance:=0.0002,
                                                 absoluteTolerance:=0.000005)

        AssertGDerivativesMatchFiniteDifferences(New HeterogeneousToeplitzRandomEffects(),
                                                 New Double() {Math.Log(0.72), Math.Log(0.38), Math.Log(0.21), AtanhForTest(0.16), AtanhForTest(-0.04)},
                                                 CreateRandomEffectsData(3),
                                                 "Heterogeneous Toeplitz G-side derivatives",
                                                 relativeTolerance:=0.0002,
                                                 absoluteTolerance:=0.000005)

        AssertGDerivativesMatchFiniteDifferences(New UnstructuredRandomEffects(),
                                                 New Double() {Math.Log(0.7), 0.04, Math.Log(0.35), -0.03, 0.02, Math.Log(0.22)},
                                                 CreateRandomEffectsData(3),
                                                 "Unstructured random-effects G-side derivatives",
                                                 relativeTolerance:=0.00008,
                                                 absoluteTolerance:=0.000002)
    End Sub

    <TestMethod()>
    Public Sub GDerivativeProvider_NoRandomEffects_SucceedsWithoutParameters()
        Dim data As MixedModelBlockData = CreateVisitData(4)
        Dim block As MixedModelSubjectBlock = data.GetBlock(0)
        Dim derivs As Double(,,) = Nothing
        Dim msg As String = Nothing

        Assert.IsTrue(MixedModelCovarianceDerivatives.TryBuildGDerivatives(New NoRandomEffects(),
                                                                           Array.Empty(Of Double)(),
                                                                           block,
                                                                           data,
                                                                           derivs,
                                                                           msg),
                      "NoRandomEffects should be a supported zero-parameter G-side derivative provider. " & If(msg, String.Empty))
        Assert.IsNull(derivs, "NoRandomEffects has no derivative slices because it has no covariance parameters.")
    End Sub

    <TestMethod()>
    Public Sub GDerivativeProvider_RejectsUnsupportedInputs_WithDiagnosticMessage()
        Dim data As MixedModelBlockData = CreateVisitData(4)
        Dim block As MixedModelSubjectBlock = data.GetBlock(0)
        Dim derivs As Double(,,) = Nothing
        Dim msg As String = Nothing

        Assert.IsFalse(MixedModelCovarianceDerivatives.TryBuildGDerivatives(Nothing,
                                                                            New Double() {0.0},
                                                                            block,
                                                                            data,
                                                                            derivs,
                                                                            msg))
        Assert.IsNull(derivs)
        Assert.IsTrue(msg.IndexOf("missing", StringComparison.OrdinalIgnoreCase) >= 0)
    End Sub

    Private Shared Sub AssertRDerivativesMatchFiniteDifferences(residualStruct As MixedModelRStruct,
                                                                theta() As Double,
                                                                data As MixedModelBlockData,
                                                                label As String,
                                                                Optional relativeTolerance As Double = 0.00005,
                                                                Optional absoluteTolerance As Double = 0.000001)
        Assert.AreEqual(theta.Length, residualStruct.ParamCount(data), label & ": theta length should match structure parameter count.")

        For blockIndex As Integer = 0 To data.NoSubjects - 1
            Dim block As MixedModelSubjectBlock = data.GetBlock(blockIndex)
            Dim analytic As Double(,,) = Nothing
            Dim msg As String = Nothing

            Assert.IsTrue(MixedModelCovarianceDerivatives.TryBuildRDerivatives(residualStruct,
                                                                               theta,
                                                                               block,
                                                                               data,
                                                                               analytic,
                                                                               msg),
                          label & ": derivative builder should succeed for subject " & block.SubjectKey & ". " & If(msg, String.Empty))

            Assert.IsNotNull(analytic, label & ": analytic derivative array should be populated.")
            Assert.AreEqual(theta.Length, analytic.GetLength(0), label & ": derivative parameter dimension.")
            Assert.AreEqual(block.Nobs, analytic.GetLength(1), label & ": derivative row dimension.")
            Assert.AreEqual(block.Nobs, analytic.GetLength(2), label & ": derivative column dimension.")

            For h As Integer = 0 To theta.Length - 1
                Dim numeric As Double(,) = NumericalDerivative(residualStruct, theta, h, block, data)
                For i As Integer = 0 To block.Nobs - 1
                    For j As Integer = 0 To block.Nobs - 1
                        Dim expected As Double = numeric(i, j)
                        Dim actual As Double = analytic(h, i, j)
                        Dim scale As Double = Math.Max(1.0, Math.Max(Math.Abs(expected), Math.Abs(actual)))
                        Dim diff As Double = Math.Abs(expected - actual)
                        Assert.IsTrue(diff <= absoluteTolerance + relativeTolerance * scale,
                                      label & ": subject=" & block.SubjectKey &
                                      ", parameter=" & h.ToString() &
                                      ", row=" & i.ToString() &
                                      ", col=" & j.ToString() &
                                      ", numeric=" & expected.ToString("R") &
                                      ", analytic=" & actual.ToString("R") &
                                      ", diff=" & diff.ToString("R"))
                    Next
                Next
            Next
        Next
    End Sub

    Private Shared Sub AssertGDerivativesMatchFiniteDifferences(gStruct As MixedModelGStruct,
                                                                thetaG() As Double,
                                                                data As MixedModelBlockData,
                                                                label As String,
                                                                Optional relativeTolerance As Double = 0.00005,
                                                                Optional absoluteTolerance As Double = 0.000001)
        Assert.AreEqual(thetaG.Length, gStruct.ParamCount(data.Q), label & ": theta length should match structure parameter count.")

        For blockIndex As Integer = 0 To data.NoSubjects - 1
            Dim block As MixedModelSubjectBlock = data.GetBlock(blockIndex)
            Dim analytic As Double(,,) = Nothing
            Dim msg As String = Nothing

            Assert.IsTrue(MixedModelCovarianceDerivatives.TryBuildGDerivatives(gStruct,
                                                                               thetaG,
                                                                               block,
                                                                               data,
                                                                               analytic,
                                                                               msg),
                          label & ": derivative builder should succeed for subject " & block.SubjectKey & ". " & If(msg, String.Empty))

            Assert.IsNotNull(analytic, label & ": analytic derivative array should be populated.")
            Assert.AreEqual(thetaG.Length, analytic.GetLength(0), label & ": derivative parameter dimension.")
            Assert.AreEqual(block.Nobs, analytic.GetLength(1), label & ": derivative row dimension.")
            Assert.AreEqual(block.Nobs, analytic.GetLength(2), label & ": derivative column dimension.")

            For h As Integer = 0 To thetaG.Length - 1
                Dim numeric As Double(,) = NumericalGDerivative(gStruct, thetaG, h, block, data)
                For i As Integer = 0 To block.Nobs - 1
                    For j As Integer = 0 To block.Nobs - 1
                        Dim expected As Double = numeric(i, j)
                        Dim actual As Double = analytic(h, i, j)
                        Dim scale As Double = Math.Max(1.0, Math.Max(Math.Abs(expected), Math.Abs(actual)))
                        Dim diff As Double = Math.Abs(expected - actual)
                        Assert.IsTrue(diff <= absoluteTolerance + relativeTolerance * scale,
                                      label & ": subject=" & block.SubjectKey &
                                      ", parameter=" & h.ToString() &
                                      ", row=" & i.ToString() &
                                      ", col=" & j.ToString() &
                                      ", numeric=" & expected.ToString("R") &
                                      ", analytic=" & actual.ToString("R") &
                                      ", diff=" & diff.ToString("R"))
                    Next
                Next
            Next
        Next
    End Sub

    Private Shared Function NumericalGDerivative(gStruct As MixedModelGStruct,
                                                 thetaG() As Double,
                                                 parameterIndex As Integer,
                                                 block As MixedModelSubjectBlock,
                                                 data As MixedModelBlockData) As Double(,)
        Dim stepSize As Double = 0.000001 * Math.Max(1.0, Math.Abs(thetaG(parameterIndex)))
        Dim plus(thetaG.Length - 1) As Double
        Dim minus(thetaG.Length - 1) As Double
        Array.Copy(thetaG, plus, thetaG.Length)
        Array.Copy(thetaG, minus, thetaG.Length)
        plus(parameterIndex) += stepSize
        minus(parameterIndex) -= stepSize

        Dim residualStruct As New IdentityR()
        Dim thetaR() As Double = {Math.Log(0.55)}
        Dim plusVi As Double(,) = MixedModelCovariance.BuildVi(block, data, gStruct, residualStruct, plus, thetaR)
        Dim minusVi As Double(,) = MixedModelCovariance.BuildVi(block, data, gStruct, residualStruct, minus, thetaR)
        Dim n As Integer = block.Nobs
        Dim out(n - 1, n - 1) As Double

        For i As Integer = 0 To n - 1
            For j As Integer = 0 To n - 1
                out(i, j) = (plusVi(i, j) - minusVi(i, j)) / (2.0 * stepSize)
            Next
        Next

        Return out
    End Function

    Private Shared Function NumericalDerivative(residualStruct As MixedModelRStruct,
                                                theta() As Double,
                                                parameterIndex As Integer,
                                                block As MixedModelSubjectBlock,
                                                data As MixedModelBlockData) As Double(,)
        Dim stepSize As Double = 0.000001 * Math.Max(1.0, Math.Abs(theta(parameterIndex)))
        Dim plus(theta.Length - 1) As Double
        Dim minus(theta.Length - 1) As Double
        Array.Copy(theta, plus, theta.Length)
        Array.Copy(theta, minus, theta.Length)
        plus(parameterIndex) += stepSize
        minus(parameterIndex) -= stepSize

        Dim plusRi As Double(,) = residualStruct.BuildRi(plus, block, data)
        Dim minusRi As Double(,) = residualStruct.BuildRi(minus, block, data)
        Dim n As Integer = block.Nobs
        Dim out(n - 1, n - 1) As Double

        For i As Integer = 0 To n - 1
            For j As Integer = 0 To n - 1
                out(i, j) = (plusRi(i, j) - minusRi(i, j)) / (2.0 * stepSize)
            Next
        Next

        Return out
    End Function

    Private Shared Function CreateVisitData(visitCount As Integer) As MixedModelBlockData
        Dim subjects As New List(Of Object)
        Dim visits As New List(Of Double)
        Dim y As New List(Of Double)

        For v As Integer = 1 To visitCount
            subjects.Add("S1")
            visits.Add(CDbl(v))
            y.Add(10.0 + 0.1 * CDbl(v))
        Next

        For v As Integer = 1 To visitCount
            If v <> 2 Then
                subjects.Add("S2")
                visits.Add(CDbl(v))
                y.Add(11.0 + 0.2 * CDbl(v))
            End If
        Next

        For v As Integer = 1 To visitCount
            If v Mod 2 = 0 OrElse v = visitCount Then
                subjects.Add("S3")
                visits.Add(CDbl(v))
                y.Add(12.0 + 0.15 * CDbl(v))
            End If
        Next

        Dim x(y.Count - 1, 0) As Double
        For i As Integer = 0 To y.Count - 1
            x(i, 0) = 1.0
        Next

        Return MixedModelBlockData.FromArrays(y:=y.ToArray(),
                                              x:=x,
                                              subjectId:=subjects.ToArray(),
                                              z:=Nothing,
                                              visit:=visits.ToArray(),
                                              sortWithinSubjectByVisit:=True)
    End Function

    Private Shared Function CreateRandomEffectsData(q As Integer) As MixedModelBlockData
        Dim subjectCount As Integer = 5
        Dim visitCount As Integer = 4
        Dim n As Integer = subjectCount * visitCount
        Dim y(n - 1) As Double
        Dim x(n - 1, 1) As Double
        Dim z(n - 1, q - 1) As Double
        Dim subject(n - 1) As Object
        Dim visit(n - 1) As Double

        Dim row As Integer = 0
        For s As Integer = 0 To subjectCount - 1
            For v As Integer = 1 To visitCount
                Dim time As Double = CDbl(v - 1) - 1.5
                subject(row) = "G" & s.ToString("000")
                visit(row) = CDbl(v)
                y(row) = 3.0 + 0.2 * time + 0.04 * CDbl(s Mod 3)
                x(row, 0) = 1.0
                x(row, 1) = time
                z(row, 0) = 1.0
                If q >= 2 Then z(row, 1) = time
                If q >= 3 Then z(row, 2) = time * time - 1.25
                row += 1
            Next
        Next

        Return MixedModelBlockData.FromArrays(y:=y,
                                              x:=x,
                                              subjectId:=subject,
                                              z:=z,
                                              visit:=visit,
                                              sortWithinSubjectByVisit:=True)
    End Function

    Private Shared Function CreateUnstructuredTheta(visitCount As Integer) As Double()
        Dim theta(visitCount * (visitCount + 1) \ 2 - 1) As Double
        Dim k As Integer = 0
        For i As Integer = 0 To visitCount - 1
            For j As Integer = 0 To i
                If i = j Then
                    theta(k) = Math.Log(0.9 + 0.08 * CDbl(i + 1))
                Else
                    theta(k) = 0.02 * CDbl(i + 1) - 0.015 * CDbl(j + 1)
                End If
                k += 1
            Next
        Next
        Return theta
    End Function

    Private Shared Function AtanhForTest(rho As Double) As Double
        Return 0.5 * Math.Log((1.0 + rho) / (1.0 - rho))
    End Function

End Class
