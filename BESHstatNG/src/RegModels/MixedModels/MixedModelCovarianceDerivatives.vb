Option Explicit On
Option Strict On

Imports System

Namespace regression

    ''' <summary>
    ''' Covariance-derivative provider used by the analytic-gradient implementation batches.
    ''' </summary>
    ''' <remarks>
    ''' AG-BFGS-P2 implements R-side residual covariance derivatives for the MMRM residual
    ''' structures. AG-BFGS-P6 adds G-side random-effects derivatives for the LMM
    ''' structures. The returned three-dimensional array is indexed as
    ''' <c>derivatives(parameterIndex, rowIndex, columnIndex)</c>, where each slice is
    ''' <c>dV_i / dtheta_h</c> on the internal optimizer parameter scale used by BFGS.
    ''' </remarks>
    Friend Module MixedModelCovarianceDerivatives

        Friend Const UnsupportedGDerivativeMessage As String = "G-side covariance derivatives are not implemented for the requested random-effects structure."
        Private Const CorrelationAbsLimit As Double = 0.995

        Friend Function TryBuildRDerivatives(residualStruct As MixedModelRStruct,
                                             thetaR() As Double,
                                             block As MixedModelSubjectBlock,
                                             data As MixedModelBlockData,
                                             ByRef derivatives As Double(,,),
                                             Optional ByRef message As String = Nothing) As Boolean
            derivatives = Nothing
            message = Nothing

            If residualStruct Is Nothing Then
                message = "Residual covariance structure is missing."
                Return False
            End If
            If thetaR Is Nothing Then
                message = "R-side parameter vector is missing."
                Return False
            End If
            If block Is Nothing Then
                message = "Subject block is missing."
                Return False
            End If
            If data Is Nothing Then
                message = "Mixed-model block data is missing."
                Return False
            End If

            Try
                If TypeOf residualStruct Is IdentityR Then
                    derivatives = BuildIdentityDerivatives(thetaR, block)
                    Return True
                End If

                If TypeOf residualStruct Is DiagonalHeterogeneousR Then
                    derivatives = BuildDiagonalHeterogeneousDerivatives(thetaR, block, data)
                    Return True
                End If

                If TypeOf residualStruct Is CompoundSymmetryR Then
                    derivatives = BuildCompoundSymmetryDerivatives(thetaR, block)
                    Return True
                End If

                If TypeOf residualStruct Is HeterogeneousCSR Then
                    derivatives = BuildHeterogeneousCsDerivatives(thetaR, block, data)
                    Return True
                End If

                If TypeOf residualStruct Is AR1R Then
                    derivatives = BuildAr1Derivatives(thetaR, block, data)
                    Return True
                End If

                If TypeOf residualStruct Is HeterogeneousAR1R Then
                    derivatives = BuildHeterogeneousAr1Derivatives(thetaR, block, data)
                    Return True
                End If

                If TypeOf residualStruct Is UnstructuredR Then
                    derivatives = BuildUnstructuredDerivatives(thetaR, block, data)
                    Return True
                End If

                message = "R-side covariance derivatives are not implemented for residual structure '" & residualStruct.ToString() & "'."
                Return False
            Catch ex As Exception
                derivatives = Nothing
                message = "Failed to build R-side covariance derivatives for residual structure '" & residualStruct.ToString() & "': " & ex.Message
                Return False
            End Try
        End Function

        Private Function BuildRandomInterceptGDerivatives(theta() As Double,
                                                        block As MixedModelSubjectBlock) As Double(,,)
            ValidateThetaLength(theta, 1, "RandomIntercept")
            ValidateBlockQ(block, 1, "RandomIntercept")

            Dim n As Integer = block.Nobs
            Dim out(0, n - 1, n - 1) As Double
            Dim z(,) As Double = block.Z
            Dim varB As Double = Math.Exp(theta(0))

            For i As Integer = 0 To n - 1
                For j As Integer = 0 To n - 1
                    out(0, i, j) = varB * z(i, 0) * z(j, 0)
                Next
            Next

            Return out
        End Function

        Private Function BuildRandomInterceptSlopeGDerivatives(theta() As Double,
                                                             block As MixedModelSubjectBlock) As Double(,,)
            ValidateThetaLength(theta, 3, "RandomInterceptSlope")
            ValidateBlockQ(block, 2, "RandomInterceptSlope")

            Dim sd0 As Double = Math.Exp(theta(0))
            Dim sd1 As Double = Math.Exp(theta(1))
            Dim rho As Double = Math.Tanh(theta(2))
            Dim drho As Double = 1.0 - rho * rho

            Dim dG(2, 1, 1) As Double
            dG(0, 0, 0) = 2.0 * sd0 * sd0
            dG(0, 0, 1) = rho * sd0 * sd1
            dG(0, 1, 0) = dG(0, 0, 1)

            dG(1, 1, 1) = 2.0 * sd1 * sd1
            dG(1, 0, 1) = rho * sd0 * sd1
            dG(1, 1, 0) = dG(1, 0, 1)

            dG(2, 0, 1) = drho * sd0 * sd1
            dG(2, 1, 0) = dG(2, 0, 1)

            Return TransformGDerivativesToV(block, dG)
        End Function

        Private Function BuildUnstructuredGDerivatives(theta() As Double,
                                                      block As MixedModelSubjectBlock,
                                                      q As Integer) As Double(,,)
            ValidateBlockQ(block, q, "UnstructuredRandomEffects")
            Dim expected As Integer = q * (q + 1) \ 2
            ValidateThetaLength(theta, expected, "UnstructuredRandomEffects")

            Dim l(q - 1, q - 1) As Double
            Dim paramRow(expected - 1) As Integer
            Dim paramCol(expected - 1) As Integer
            Dim dLValue(expected - 1) As Double

            Dim k As Integer = 0
            For i As Integer = 0 To q - 1
                For j As Integer = 0 To i
                    paramRow(k) = i
                    paramCol(k) = j
                    If i = j Then
                        l(i, j) = Math.Exp(theta(k))
                        dLValue(k) = l(i, j)
                    Else
                        l(i, j) = theta(k)
                        dLValue(k) = 1.0
                    End If
                    k += 1
                Next
            Next

            Dim dG(expected - 1, q - 1, q - 1) As Double
            For h As Integer = 0 To expected - 1
                Dim a As Integer = paramRow(h)
                Dim b As Integer = paramCol(h)
                Dim d As Double = dLValue(h)

                For i As Integer = 0 To q - 1
                    For j As Integer = 0 To q - 1
                        Dim value As Double = 0.0
                        If i = a Then value += d * l(j, b)
                        If j = a Then value += l(i, b) * d
                        dG(h, i, j) = value
                    Next
                Next
            Next

            Return TransformGDerivativesToV(block, dG)
        End Function

        Private Function TransformGDerivativesToV(block As MixedModelSubjectBlock,
                                                  dG(,,) As Double) As Double(,,)
            If block Is Nothing Then Throw New ArgumentNullException(NameOf(block))
            If dG Is Nothing Then Throw New ArgumentNullException(NameOf(dG))
            If Not block.HasRandomEffectsDesign() Then Throw New ApplicationException("Subject block has no random-effects design matrix Z.")

            Dim z(,) As Double = block.Z
            Dim paramCount As Integer = dG.GetLength(0)
            Dim n As Integer = block.Nobs
            Dim q As Integer = block.Q
            If dG.GetLength(1) <> q OrElse dG.GetLength(2) <> q Then
                Throw New ArgumentException("G-side derivative dimension is incompatible with the block Z matrix.")
            End If

            Dim out(paramCount - 1, n - 1, n - 1) As Double
            For h As Integer = 0 To paramCount - 1
                For i As Integer = 0 To n - 1
                    For j As Integer = 0 To n - 1
                        Dim value As Double = 0.0
                        For a As Integer = 0 To q - 1
                            Dim zia As Double = z(i, a)
                            If zia = 0.0 Then Continue For
                            For b As Integer = 0 To q - 1
                                value += zia * dG(h, a, b) * z(j, b)
                            Next
                        Next
                        out(h, i, j) = value
                    Next
                Next
            Next

            Return out
        End Function

        Friend Function TryBuildGDerivatives(gStruct As MixedModelGStruct,
                                             thetaG() As Double,
                                             block As MixedModelSubjectBlock,
                                             data As MixedModelBlockData,
                                             ByRef derivatives As Double(,,),
                                             Optional ByRef message As String = Nothing) As Boolean
            derivatives = Nothing
            message = Nothing

            If gStruct Is Nothing Then
                message = "Random-effects covariance structure is missing."
                Return False
            End If
            If thetaG Is Nothing Then
                message = "G-side parameter vector is missing."
                Return False
            End If
            If block Is Nothing Then
                message = "Subject block is missing."
                Return False
            End If
            If data Is Nothing Then
                message = "Mixed-model block data is missing."
                Return False
            End If

            Try
                If TypeOf gStruct Is NoRandomEffects Then
                    ValidateThetaLength(thetaG, 0, "NoRandomEffects")
                    message = "No G-side parameters are present for NoRandomEffects."
                    derivatives = Nothing
                    Return True
                End If

                If Not block.HasRandomEffectsDesign() Then
                    message = "Subject block has no random-effects design matrix Z."
                    Return False
                End If

                If data.Q <> block.Q Then
                    message = "Random-effects design dimension mismatch between block and dataset."
                    Return False
                End If

                If TypeOf gStruct Is RandomIntercept Then
                    derivatives = BuildRandomInterceptGDerivatives(thetaG, block)
                    Return True
                End If

                If TypeOf gStruct Is RandomInterceptSlope Then
                    derivatives = BuildRandomInterceptSlopeGDerivatives(thetaG, block)
                    Return True
                End If

                If TypeOf gStruct Is UnstructuredRandomEffects Then
                    derivatives = BuildUnstructuredGDerivatives(thetaG, block, data.Q)
                    Return True
                End If

                message = UnsupportedGDerivativeMessage & " Structure='" & gStruct.ToString() & "'."
                Return False
            Catch ex As Exception
                derivatives = Nothing
                message = "Failed to build G-side covariance derivatives for random-effects structure '" & gStruct.ToString() & "': " & ex.Message
                Return False
            End Try
        End Function

        Private Function BuildIdentityDerivatives(theta() As Double,
                                                  block As MixedModelSubjectBlock) As Double(,,)
            ValidateThetaLength(theta, 1, "IdentityR")
            Dim n As Integer = block.Nobs
            Dim out(0, n - 1, n - 1) As Double
            Dim sigma2 As Double = Math.Exp(theta(0))
            For i As Integer = 0 To n - 1
                out(0, i, i) = sigma2
            Next
            Return out
        End Function

        Private Function BuildDiagonalHeterogeneousDerivatives(theta() As Double,
                                                               block As MixedModelSubjectBlock,
                                                               data As MixedModelBlockData) As Double(,,)
            Dim m As Integer = VisitDimension(data)
            ValidateThetaLength(theta, m, "DiagonalHeterogeneousR")
            Dim n As Integer = block.Nobs
            Dim out(m - 1, n - 1, n - 1) As Double
            Dim idx() As Integer = GetBlockVisitIndices(block)
            ValidateVisitIndices(idx, m)

            For i As Integer = 0 To n - 1
                Dim h As Integer = idx(i)
                out(h, i, i) = Math.Exp(theta(h))
            Next

            Return out
        End Function

        Private Function BuildCompoundSymmetryDerivatives(theta() As Double,
                                                          block As MixedModelSubjectBlock) As Double(,,)
            ValidateThetaLength(theta, 2, "CompoundSymmetryR")
            Dim n As Integer = block.Nobs
            Dim out(1, n - 1, n - 1) As Double
            Dim sigma2 As Double = Math.Exp(theta(0))
            Dim rho As Double = 0.0
            Dim drho As Double = 0.0
            CorrelationAndDerivative(theta(1), rho, drho)

            For i As Integer = 0 To n - 1
                For j As Integer = 0 To n - 1
                    Dim corr As Double = If(i = j, 1.0, rho)
                    out(0, i, j) = sigma2 * corr
                    out(1, i, j) = If(i = j, 0.0, sigma2 * drho)
                Next
            Next

            Return out
        End Function

        Private Function BuildHeterogeneousCsDerivatives(theta() As Double,
                                                         block As MixedModelSubjectBlock,
                                                         data As MixedModelBlockData) As Double(,,)
            Dim m As Integer = VisitDimension(data)
            ValidateThetaLength(theta, m + 1, "HeterogeneousCSR")
            Dim n As Integer = block.Nobs
            Dim out(m, n - 1, n - 1) As Double
            Dim idx() As Integer = GetBlockVisitIndices(block)
            ValidateVisitIndices(idx, m)

            Dim rho As Double = 0.0
            Dim drho As Double = 0.0
            CorrelationAndDerivative(theta(m), rho, drho)

            Dim sd(n - 1) As Double
            For i As Integer = 0 To n - 1
                sd(i) = Math.Sqrt(Math.Exp(theta(idx(i))))
            Next

            For i As Integer = 0 To n - 1
                For j As Integer = 0 To n - 1
                    Dim corr As Double = If(i = j, 1.0, rho)
                    Dim rij As Double = sd(i) * sd(j) * corr
                    Dim hi As Integer = idx(i)
                    Dim hj As Integer = idx(j)

                    out(hi, i, j) += 0.5 * rij
                    out(hj, i, j) += 0.5 * rij
                    If i <> j Then out(m, i, j) = sd(i) * sd(j) * drho
                Next
            Next

            Return out
        End Function

        Private Function BuildAr1Derivatives(theta() As Double,
                                             block As MixedModelSubjectBlock,
                                             data As MixedModelBlockData) As Double(,,)
            ValidateThetaLength(theta, 2, "AR1R")
            Dim n As Integer = block.Nobs
            Dim out(1, n - 1, n - 1) As Double
            Dim idx() As Integer = GetBlockVisitIndices(block)
            ValidateVisitIndices(idx, VisitDimension(data))

            Dim sigma2 As Double = Math.Exp(theta(0))
            Dim rho As Double = 0.0
            Dim drho As Double = 0.0
            CorrelationAndDerivative(theta(1), rho, drho)

            For i As Integer = 0 To n - 1
                For j As Integer = 0 To n - 1
                    Dim lag As Integer = Math.Abs(idx(i) - idx(j))
                    out(0, i, j) = sigma2 * SafePow(rho, lag)
                    If lag > 0 Then
                        out(1, i, j) = sigma2 * Ar1PowerDerivativeWithRespectToRho(rho, lag) * drho
                    End If
                Next
            Next

            Return out
        End Function

        Private Function BuildHeterogeneousAr1Derivatives(theta() As Double,
                                                          block As MixedModelSubjectBlock,
                                                          data As MixedModelBlockData) As Double(,,)
            Dim m As Integer = VisitDimension(data)
            ValidateThetaLength(theta, m + 1, "HeterogeneousAR1R")
            Dim n As Integer = block.Nobs
            Dim out(m, n - 1, n - 1) As Double
            Dim idx() As Integer = GetBlockVisitIndices(block)
            ValidateVisitIndices(idx, m)

            Dim rho As Double = 0.0
            Dim drho As Double = 0.0
            CorrelationAndDerivative(theta(m), rho, drho)

            Dim sd(n - 1) As Double
            For i As Integer = 0 To n - 1
                sd(i) = Math.Sqrt(Math.Exp(theta(idx(i))))
            Next

            For i As Integer = 0 To n - 1
                For j As Integer = 0 To n - 1
                    Dim lag As Integer = Math.Abs(idx(i) - idx(j))
                    Dim corr As Double = SafePow(rho, lag)
                    Dim rij As Double = sd(i) * sd(j) * corr
                    Dim hi As Integer = idx(i)
                    Dim hj As Integer = idx(j)

                    out(hi, i, j) += 0.5 * rij
                    out(hj, i, j) += 0.5 * rij
                    If lag > 0 Then
                        out(m, i, j) = sd(i) * sd(j) * Ar1PowerDerivativeWithRespectToRho(rho, lag) * drho
                    End If
                Next
            Next

            Return out
        End Function

        Private Function BuildUnstructuredDerivatives(theta() As Double, block As MixedModelSubjectBlock, data As MixedModelBlockData) As Double(,,)
            Dim m As Integer = VisitDimension(data)
            Dim expected As Integer = m * (m + 1) \ 2
            ValidateThetaLength(theta, expected, "UnstructuredR")
            Dim n As Integer = block.Nobs
            Dim idx() As Integer = GetBlockVisitIndices(block)
            ValidateVisitIndices(idx, m)

            Dim l(m - 1, m - 1) As Double
            Dim paramRow(expected - 1) As Integer
            Dim paramCol(expected - 1) As Integer
            Dim dLValue(expected - 1) As Double

            Dim k As Integer = 0
            For i As Integer = 0 To m - 1
                For j As Integer = 0 To i
                    paramRow(k) = i
                    paramCol(k) = j
                    If i = j Then
                        l(i, j) = Math.Exp(theta(k))
                        dLValue(k) = l(i, j)
                    Else
                        l(i, j) = theta(k)
                        dLValue(k) = 1.0
                    End If
                    k += 1
                Next
            Next

            Dim out(expected - 1, n - 1, n - 1) As Double
            For h As Integer = 0 To expected - 1
                Dim a As Integer = paramRow(h)
                Dim b As Integer = paramCol(h)
                Dim d As Double = dLValue(h)

                For obsI As Integer = 0 To n - 1
                    Dim fullI As Integer = idx(obsI)
                    For obsJ As Integer = 0 To n - 1
                        Dim fullJ As Integer = idx(obsJ)
                        Dim value As Double = 0.0
                        If fullI = a Then value += d * l(fullJ, b)
                        If fullJ = a Then value += l(fullI, b) * d
                        out(h, obsI, obsJ) = value
                    Next
                Next
            Next

            Return out
        End Function

        Private Function VisitDimension(data As MixedModelBlockData) As Integer
            If data.HasVisit AndAlso data.UniqueVisitValues IsNot Nothing AndAlso data.UniqueVisitValues.Length > 0 Then
                Return data.UniqueVisitValues.Length
            End If
            Return data.MaxClusterSize()
        End Function

        Private Function GetBlockVisitIndices(block As MixedModelSubjectBlock) As Integer()
            Dim n As Integer = block.Nobs
            Dim source() As Integer = block.VisitIndex
            Dim out(n - 1) As Integer

            If source IsNot Nothing AndAlso source.Length = n Then
                Array.Copy(source, out, n)
            Else
                For i As Integer = 0 To n - 1
                    out(i) = i
                Next
            End If

            Return out
        End Function

        Private Sub ValidateBlockQ(block As MixedModelSubjectBlock, expectedQ As Integer, structureName As String)
            If block Is Nothing Then Throw New ArgumentNullException(NameOf(block))
            If block.Q <> expectedQ Then
                Throw New ArgumentException(structureName & " expects random-effects design dimension q=" & expectedQ.ToString(System.Globalization.CultureInfo.InvariantCulture) & ", received q=" & block.Q.ToString(System.Globalization.CultureInfo.InvariantCulture) & ".")
            End If
        End Sub

        Private Sub ValidateVisitIndices(indices() As Integer, visitDimension As Integer)
            If indices Is Nothing Then Throw New ArgumentNullException(NameOf(indices))
            For i As Integer = 0 To indices.Length - 1
                If indices(i) < 0 OrElse indices(i) >= visitDimension Then
                    Throw New ArgumentOutOfRangeException(NameOf(indices), "Block visit index is outside the global visit dimension.")
                End If
            Next
        End Sub

        Private Sub ValidateThetaLength(theta() As Double, expected As Integer, structureName As String)
            Dim actual As Integer = If(theta Is Nothing, 0, theta.Length)
            If actual <> expected Then
                Throw New ArgumentException("Unexpected parameter length for " & structureName & ". Expected " & expected.ToString(System.Globalization.CultureInfo.InvariantCulture) & ", received " & actual.ToString(System.Globalization.CultureInfo.InvariantCulture) & ".")
            End If
        End Sub

        Private Sub CorrelationAndDerivative(theta As Double, ByRef rho As Double, ByRef derivative As Double)
            Dim raw As Double = Math.Tanh(theta)
            If raw > CorrelationAbsLimit Then
                rho = CorrelationAbsLimit
                derivative = 0.0
            ElseIf raw < -CorrelationAbsLimit Then
                rho = -CorrelationAbsLimit
                derivative = 0.0
            Else
                rho = raw
                derivative = 1.0 - raw * raw
            End If
        End Sub

        Private Function SafePow(rho As Double, lag As Integer) As Double
            If lag = 0 Then Return 1.0
            If lag = 1 Then Return rho
            If rho = 0.0 Then Return 0.0
            Return rho ^ lag
        End Function

        Private Function Ar1PowerDerivativeWithRespectToRho(rho As Double, lag As Integer) As Double
            If lag <= 0 Then Return 0.0
            If lag = 1 Then Return 1.0
            If rho = 0.0 Then Return 0.0
            Return CDbl(lag) * (rho ^ (lag - 1))
        End Function

    End Module

End Namespace
