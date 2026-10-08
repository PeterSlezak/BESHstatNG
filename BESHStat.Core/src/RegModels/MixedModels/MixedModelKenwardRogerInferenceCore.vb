Option Explicit On
Option Strict On

Imports System
Imports System.Collections.Generic
Imports BESHStatNG.Matrix

Namespace regression

    ''' <summary>Portable Kenward-Roger denominator-DF and F-scaling result.</summary>
    Public Class MixedModelKenwardRogerDfResult
        Public Property NumDF As Integer = 0
        Public Property DenDF As Double = Double.NaN
        Public Property Lambda As Double = Double.NaN
        Public Property A1 As Double = Double.NaN
        Public Property A2 As Double = Double.NaN
        Public Property B As Double = Double.NaN
        Public Property EStar As Double = Double.NaN
        Public Property VStar As Double = Double.NaN
        Public Property Rho As Double = Double.NaN
        Public Property DiagnosticMessage As String = String.Empty

    End Class

    ''' <summary>Host-neutral Kenward-Roger moment-matching calculations.</summary>
    Public Module MixedModelKenwardRogerInferenceCore

        Public Function TryComputeFScalingAndDenominatorDF(ws As MixedModelKrWorkspace,
                                                           lMatrix(,) As Double,
                                                           ByRef scaling As Double,
                                                           ByRef df As Double,
                                                           Optional ByRef diagnostic As String = Nothing) As Boolean
            scaling = Double.NaN
            df = Double.NaN
            diagnostic = String.Empty
            Dim info As MixedModelKenwardRogerDfResult = Nothing
            If Not TryComputeDegreesOfFreedomAndScaling(ws, lMatrix, info, diagnostic) Then Return False
            scaling = info.Lambda
            df = info.DenDF
            diagnostic = info.DiagnosticMessage
            Return True
        End Function

        Public Function TryComputeDegreesOfFreedomAndScaling(ws As MixedModelKrWorkspace,
                                                             lMatrix(,) As Double,
                                                             ByRef info As MixedModelKenwardRogerDfResult,
                                                             Optional ByRef diagnostic As String = Nothing) As Boolean
            info = Nothing
            diagnostic = String.Empty
            If ws Is Nothing Then
                diagnostic = "KR workspace is unavailable."
                Return False
            End If
            If lMatrix Is Nothing Then
                diagnostic = "L matrix is Nothing."
                Return False
            End If

            Dim q As Integer = lMatrix.GetLength(0)
            Dim p As Integer = lMatrix.GetLength(1)
            If q <= 0 OrElse p <> ws.P Then
                diagnostic = "L matrix is not conformable with KR workspace P."
                Return False
            End If

            Dim effectiveL(,) As Double = Nothing
            Dim rankDiagnostic As String = String.Empty
            If Not TryBuildFullRowRankRestriction(lMatrix, effectiveL, rankDiagnostic) Then
                diagnostic = "Could not build a full-row-rank KR restriction matrix: " & rankDiagnostic
                Return False
            End If
            lMatrix = effectiveL
            q = lMatrix.GetLength(0)

            If ws.VarBeta Is Nothing OrElse ws.Pmats Is Nothing OrElse ws.ThetaCovariance Is Nothing Then
                diagnostic = "Need VarBeta, Pmats, and ThetaCovariance to compute KR denominator DF and scaling."
                Return False
            End If
            If ws.VarBeta.GetLength(0) <> ws.P OrElse ws.VarBeta.GetLength(1) <> ws.P Then
                diagnostic = "KR VarBeta dimension mismatch."
                Return False
            End If
            If ws.Pmats.GetLength(0) <> ws.K OrElse ws.Pmats.GetLength(1) <> ws.P OrElse ws.Pmats.GetLength(2) <> ws.P Then
                diagnostic = "KR Pmats dimension mismatch."
                Return False
            End If
            If ws.ThetaCovariance.GetLength(0) <> ws.K OrElse ws.ThetaCovariance.GetLength(1) <> ws.K Then
                diagnostic = "KR ThetaCovariance dimension mismatch."
                Return False
            End If

            If ws.DfScalingCache Is Nothing Then ws.DfScalingCache = New Dictionary(Of String, MixedModelKenwardRogerDfResult)()
            Dim cacheKey As String = MixedModelNumericalDiagnostics.BuildMatrixSignature(lMatrix, digits:=12)
            If ws.DfScalingCache.ContainsKey(cacheKey) Then
                info = CloneDfResult(ws.DfScalingCache(cacheKey))
                diagnostic = info.DiagnosticMessage & " Reused cached KR DF/scaling trace products."
                info.DiagnosticMessage = diagnostic
                Return True
            End If

            Dim phi(,) As Double = ws.VarBeta
            Dim cov0(,) As Double = ComputeLCovLt(lMatrix, phi)
            Dim cov0Inv(,) As Double = Nothing
            Dim invDiagnostic As String = String.Empty
            If Not TryInvertPositiveDefinite(cov0, cov0Inv, invDiagnostic) Then
                diagnostic = "Could not invert L * VarBeta * L' for KR denominator DF/scaling: " & invDiagnostic
                Return False
            End If

            Dim mMat(,) As Double = BuildRestrictionProjection(lMatrix, cov0Inv)
            Dim traces(ws.K - 1) As Double
            Dim mPhiPphi(ws.K - 1, p - 1, p - 1) As Double

            For h As Integer = 0 To ws.K - 1
                Dim ph(,) As Double = MixedModelKenwardRogerBackend.Slice3D(ws.Pmats, h)
                Dim one(,) As Double = MatrixArithmeticCore.Multiply(
                    MatrixArithmeticCore.Multiply(MatrixArithmeticCore.Multiply(mMat, phi), ph), phi)
                traces(h) = MatrixArithmeticCore.Trace(one)
                For r As Integer = 0 To p - 1
                    For c As Integer = 0 To p - 1
                        mPhiPphi(h, r, c) = one(r, c)
                    Next
                Next
            Next

            Dim a1 As Double = 0.0
            Dim a2 As Double = 0.0
            For h As Integer = 0 To ws.K - 1
                For j As Integer = 0 To ws.K - 1
                    Dim whj As Double = ws.ThetaCovariance(h, j)
                    If whj = 0.0 Then Continue For
                    a1 += whj * traces(h) * traces(j)
                    Dim mh(,) As Double = Slice3DLocal(mPhiPphi, h)
                    Dim mj(,) As Double = Slice3DLocal(mPhiPphi, j)
                    a2 += whj * MatrixArithmeticCore.Trace(MatrixArithmeticCore.Multiply(mh, mj))
                Next
            Next

            If Not AppInfrastructure.IsFinite(a1) OrElse Not AppInfrastructure.IsFinite(a2) Then
                diagnostic = "KR moment components A1/A2 are not finite."
                Return False
            End If

            Dim qD As Double = Convert.ToDouble(q)
            Const eps As Double = 0.000000000001
            If Math.Abs(a2) <= eps Then
                info = New MixedModelKenwardRogerDfResult With {
                    .NumDF = q, .DenDF = 1000000.0, .Lambda = 1.0,
                    .A1 = a1, .A2 = a2, .B = (a1 + 6.0 * a2) / (2.0 * qD),
                    .EStar = 1.0, .VStar = 2.0 / qD, .Rho = 1.0,
                    .DiagnosticMessage = "KR A2 is approximately zero; using lambda=1 and large denominator DF."
                }
                ws.DfScalingCache(cacheKey) = CloneDfResult(info)
                diagnostic = info.DiagnosticMessage
                Return True
            End If

            Dim bb As Double = (a1 + 6.0 * a2) / (2.0 * qD)
            Dim eStar As Double = 1.0 / (1.0 - (a2 / qD))
            Dim g As Double = (((qD + 1.0) * a1) - ((qD + 4.0) * a2)) / ((qD + 2.0) * a2)
            Dim denom As Double = 3.0 * qD + 2.0 - 2.0 * g
            If Math.Abs(denom) <= eps Then
                diagnostic = "KR moment denominator for c1/c2/c3 is approximately zero."
                Return False
            End If

            Dim c1 As Double = g / denom
            Dim c2 As Double = (qD - g) / denom
            Dim c3 As Double = (qD + 2.0 - g) / denom
            If Math.Abs(1.0 - (a2 / qD)) <= eps OrElse Math.Abs(1.0 - c2 * bb) <= eps OrElse Math.Abs(1.0 - c3 * bb) <= eps Then
                diagnostic = "KR moment denominator is approximately zero."
                Return False
            End If

            Dim vStar As Double = (2.0 / qD) * ((1.0 + c1 * bb) / ((1.0 - c2 * bb) * (1.0 - c2 * bb) * (1.0 - c3 * bb)))
            Dim rho As Double = vStar / (2.0 * eStar * eStar)
            If Not AppInfrastructure.IsFinite(eStar) OrElse eStar <= 0.0 OrElse
               Not AppInfrastructure.IsFinite(vStar) OrElse vStar <= 0.0 OrElse
               Not AppInfrastructure.IsFinite(rho) Then
                diagnostic = "KR moment-matched E*, V*, or rho is invalid."
                Return False
            End If

            Dim dfDenom As Double = qD * rho - 1.0
            If Math.Abs(dfDenom) <= eps Then
                diagnostic = "KR denominator-DF moment denominator is approximately zero."
                Return False
            End If
            Dim denDf As Double = 4.0 + (qD + 2.0) / dfDenom
            If Not AppInfrastructure.IsFinite(denDf) OrElse denDf <= 2.0 Then
                diagnostic = "Computed KR denominator DF is not greater than 2 and finite."
                Return False
            End If
            Dim lambda As Double = denDf / (eStar * (denDf - 2.0))
            If Not AppInfrastructure.IsFinite(lambda) OrElse lambda <= 0.0 Then
                diagnostic = "Computed KR F scaling lambda is not positive and finite."
                Return False
            End If

            denDf = Math.Max(1.0, Math.Min(1000000.0, denDf))
            info = New MixedModelKenwardRogerDfResult With {
                .NumDF = q, .DenDF = denDf, .Lambda = lambda,
                .A1 = a1, .A2 = a2, .B = bb, .EStar = eStar, .VStar = vStar, .Rho = rho,
                .DiagnosticMessage = "KR denominator DF and F scaling lambda computed by R mmrm-style h_kr_df moment matching on " &
                                     ws.ParameterScale.ToString() & " parameter scale."
            }
            ws.DfScalingCache(cacheKey) = CloneDfResult(info)
            diagnostic = info.DiagnosticMessage
            Return True
        End Function

        Private Function TryBuildFullRowRankRestriction(lMatrix(,) As Double,
                                                        ByRef effectiveL(,) As Double,
                                                        ByRef diagnostic As String) As Boolean
            effectiveL = Nothing
            diagnostic = String.Empty
            If lMatrix Is Nothing Then
                diagnostic = "L matrix is Nothing."
                Return False
            End If
            Dim q As Integer = lMatrix.GetLength(0)
            Dim p As Integer = lMatrix.GetLength(1)
            If q <= 0 OrElse p <= 0 Then
                diagnostic = "L matrix must have at least one row and one column."
                Return False
            End If

            Dim maxRowNorm As Double = 0.0
            For r As Integer = 0 To q - 1
                Dim nrm As Double = 0.0
                For c As Integer = 0 To p - 1
                    nrm += lMatrix(r, c) * lMatrix(r, c)
                Next
                maxRowNorm = Math.Max(maxRowNorm, Math.Sqrt(nrm))
            Next
            If maxRowNorm <= 0.0 OrElse Not AppInfrastructure.IsFinite(maxRowNorm) Then
                diagnostic = "All rows in L are numerically zero."
                Return False
            End If

            Dim tol As Double = Math.Max(0.000000000001, 0.0000000001 * Convert.ToDouble(Math.Max(q, p)) * maxRowNorm)
            Dim basis As New List(Of Double())()
            For r As Integer = 0 To q - 1
                Dim work(p - 1) As Double
                For c As Integer = 0 To p - 1
                    work(c) = lMatrix(r, c)
                Next
                For Each basisRow As Double() In basis
                    Dim projection As Double = MatrixArithmeticCore.DotProduct(work, basisRow)
                    For c As Integer = 0 To p - 1
                        work(c) -= projection * basisRow(c)
                    Next
                Next
                Dim norm As Double = MatrixArithmeticCore.VectorNorm(work)
                If norm > tol AndAlso AppInfrastructure.IsFinite(norm) Then
                    For c As Integer = 0 To p - 1
                        work(c) /= norm
                    Next
                    basis.Add(work)
                End If
            Next

            If basis.Count = 0 Then
                diagnostic = "L has numerical row rank zero."
                Return False
            End If
            If basis.Count = q Then
                effectiveL = DirectCast(lMatrix.Clone(), Double(,))
                diagnostic = "Restriction matrix is full row rank."
                Return True
            End If

            Dim reduced(basis.Count - 1, p - 1) As Double
            For r As Integer = 0 To basis.Count - 1
                For c As Integer = 0 To p - 1
                    reduced(r, c) = basis(r)(c)
                Next
            Next
            effectiveL = reduced
            diagnostic = "Restriction matrix had " & q.ToString() & " requested rows and numerical row rank " & basis.Count.ToString() & "."
            Return True
        End Function

        Private Function BuildRestrictionProjection(lMatrix(,) As Double, lPhiLtInv(,) As Double) As Double(,)
            Dim p As Integer = lMatrix.GetLength(1)
            Dim q As Integer = lMatrix.GetLength(0)
            Dim output(p - 1, p - 1) As Double
            For r As Integer = 0 To p - 1
                For c As Integer = 0 To p - 1
                    Dim value As Double = 0.0
                    For aa As Integer = 0 To q - 1
                        For bb As Integer = 0 To q - 1
                            value += lMatrix(aa, r) * lPhiLtInv(aa, bb) * lMatrix(bb, c)
                        Next
                    Next
                    output(r, c) = value
                Next
            Next
            MixedModelCovariance.SymmetrizeInPlace(output)
            Return output
        End Function

        Private Function ComputeLCovLt(lMatrix(,) As Double, covariance(,) As Double) As Double(,)
            Dim q As Integer = lMatrix.GetLength(0)
            Dim p As Integer = lMatrix.GetLength(1)
            Dim temp(q - 1, p - 1) As Double
            For r As Integer = 0 To q - 1
                For c As Integer = 0 To p - 1
                    Dim value As Double = 0.0
                    For h As Integer = 0 To p - 1
                        value += lMatrix(r, h) * covariance(h, c)
                    Next
                    temp(r, c) = value
                Next
            Next
            Dim output(q - 1, q - 1) As Double
            For r As Integer = 0 To q - 1
                For c As Integer = 0 To q - 1
                    Dim value As Double = 0.0
                    For h As Integer = 0 To p - 1
                        value += temp(r, h) * lMatrix(c, h)
                    Next
                    output(r, c) = value
                Next
            Next
            MixedModelCovariance.SymmetrizeInPlace(output)
            Return output
        End Function

        Private Function TryInvertPositiveDefinite(a(,) As Double,
                                                   ByRef inv(,) As Double,
                                                   ByRef diagnostic As String) As Boolean
            inv = Nothing
            diagnostic = String.Empty
            Dim detail As New MixedModelNumericalInverseResult()
            If Not MixedModelNumericalDiagnostics.TryInvertSymmetric(a, inv, diagnostic, allowPseudoInverse:=True, inverseResult:=detail) Then
                Return False
            End If
            Dim warning As String = MixedModelNumericalDiagnostics.WarningForConditionNumber("KR hypothesis covariance", detail.ConditionNumber)
            If Not String.IsNullOrWhiteSpace(warning) Then diagnostic &= " " & warning
            If detail.UsedPseudoInverse Then diagnostic &= " Used SVD pseudoinverse fallback."
            Return inv IsNot Nothing
        End Function

        Private Function Slice3DLocal(a(,,) As Double, h As Integer) As Double(,)
            Dim p As Integer = a.GetLength(1)
            Dim output(p - 1, p - 1) As Double
            For r As Integer = 0 To p - 1
                For c As Integer = 0 To p - 1
                    output(r, c) = a(h, r, c)
                Next
            Next
            Return output
        End Function

        Private Function CloneDfResult(source As MixedModelKenwardRogerDfResult) As MixedModelKenwardRogerDfResult
            If source Is Nothing Then Return Nothing
            Return New MixedModelKenwardRogerDfResult With {
                .NumDF = source.NumDF, .DenDF = source.DenDF, .Lambda = source.Lambda,
                .A1 = source.A1, .A2 = source.A2, .B = source.B,
                .EStar = source.EStar, .VStar = source.VStar, .Rho = source.Rho,
                .DiagnosticMessage = source.DiagnosticMessage
            }
        End Function

    End Module

End Namespace
