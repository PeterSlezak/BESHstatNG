Option Explicit On
Option Strict On

Imports System
Imports System.Collections.Generic
Imports BESHStatNG.AppInfrastructure

Namespace Matrix

    ''' <summary>
    ''' Host-neutral matrix decompositions, pseudoinverse, and inversion routines.
    ''' The legacy .NET Framework Matrix module forwards to these implementations.
    ''' </summary>
    Friend Module MatrixDecompositionCore

        Public NotInheritable Class SvdResult
            Public Property U As Double(,)
            Public Property SingularValues As Double()
            Public Property SingularValueMatrix As Double(,)
            Public Property V As Double(,)
        End Class

        Public NotInheritable Class QrResult
            Public Property R As Double(,)
            Public Property Q As Double(,)
        End Class

        Public Function InvertMatrix(matrix(,) As Double,
                               Optional method As String = "LU",
                               ByRef Optional errorCode As Integer = 0,
                               Optional allowPseudoinverse As Boolean = True) As Double(,)

            Dim n As Integer = matrix.GetUpperBound(0)
            If n <> matrix.GetUpperBound(1) Then
                CoreServices.Errors.LogAndThrow(New ArgumentException("Wrong input matrices dimensions."))
            End If

            Dim output(n, n) As Double
            Dim matrixCopy(,) As Double = DirectCast(matrix.Clone(), Double(,))
            Dim normalizedMethod As String = method.ToUpper().Trim()

            If normalizedMethod = "LU" Then
                Dim parity As Double
                Dim decomposition As MatrixFactorizationCore.LUDecompositionResult =
                    MatrixFactorizationCore.LUDecompose(matrixCopy, parity, errorCode)

                For column As Integer = 0 To n
                    Dim unitVector(n) As Double
                    unitVector(column) = 1.0R
                    Dim solution() As Double = MatrixFactorizationCore.LUSolve(decomposition, unitVector)
                    For row As Integer = 0 To n
                        output(row, column) = solution(row)
                    Next
                Next

            ElseIf normalizedMethod = "CHOL" Then
                Dim lower(,) As Double = MatrixFactorizationCore.Cholesky(matrixCopy, errorCode)
                If errorCode = 2 Then
                    If allowPseudoinverse Then
                        CoreServices.Log(
                            $"WARNING: CHOLESKY. mat not positive-definite. Calling pseudoInverse. mat={FormatMatrix(matrixCopy)}",
                            LogMsgType.Warn)
                        output = ComputePseudoInverse(matrixCopy)
                        CoreServices.Log($"NOTE: pseudoInverse output ={FormatMatrix(output)}")
                    Else
                        CoreServices.Errors.LogAndThrow(New ApplicationException("MatrixType not positive definite"))
                    End If
                Else
                    output = MatrixFactorizationCore.CholeskyInverse(lower)
                End If

            ElseIf normalizedMethod = "SVD" Then
                Dim svd As SvdResult = DecomposeSvd(matrixCopy)
                Dim highestIndex As Integer = svd.SingularValues.GetUpperBound(0)

                Dim maximumSingularValue As Double = 0.0R
                For i As Integer = 0 To highestIndex
                    Dim absoluteValue As Double = Math.Abs(svd.SingularValues(i))
                    If absoluteValue > maximumSingularValue Then maximumSingularValue = absoluteValue
                Next

                Const MachineEpsilon As Double = 0.00000000000000022204460492503131R
                Dim tolerance As Double = MachineEpsilon * (highestIndex + 1) * maximumSingularValue

                For i As Integer = 0 To highestIndex
                    If Math.Abs(svd.SingularValues(i)) <= tolerance Then
                        errorCode = 1
                        CoreServices.Errors.LogAndThrow(
                            New ApplicationException(
                                $"Matrix is singular or numerically rank-deficient for strict SVD inverse. " &
                                $"sigma[{i}]={svd.SingularValues(i)}, tol={tolerance}"))
                    End If
                Next

                Dim inverseSingularValues(highestIndex, highestIndex) As Double
                For i As Integer = 0 To highestIndex
                    inverseSingularValues(i, i) = 1.0R / svd.SingularValues(i)
                Next

                output = MatrixArithmeticCore.Multiply(
                    MatrixArithmeticCore.Multiply(svd.V, inverseSingularValues),
                    MatrixArithmeticCore.Transpose(svd.U))
                errorCode = 0
            Else
                CoreServices.Errors.LogAndThrow(
                    New NotImplementedException("Not implemented error. method = " & method))
            End If

            Return output
        End Function

        Public Function ComputePseudoInverse(matrix(,) As Double, Optional tolerance As Double = -1.0R) As Double(,)
            If matrix Is Nothing Then
                CoreServices.Errors.LogAndThrow(New ArgumentNullException(NameOf(matrix)))
            End If

            Const MachineEpsilon As Double = 0.00000000000000022204460492503131R

            Dim matrixCopy(,) As Double = DirectCast(matrix.Clone(), Double(,))
            Dim svd As SvdResult = DecomposeSvd(matrixCopy)
            Dim highestIndex As Integer = svd.SingularValues.GetUpperBound(0)

            If tolerance < 0.0R Then
                Dim rowCount As Integer = svd.U.GetLength(0)
                Dim columnCount As Integer = svd.U.GetLength(1)

                Dim maximumSingularValue As Double = 0.0R
                For i As Integer = 0 To highestIndex
                    Dim absoluteValue As Double = Math.Abs(svd.SingularValues(i))
                    If absoluteValue > maximumSingularValue Then maximumSingularValue = absoluteValue
                Next

                tolerance = MachineEpsilon * Math.Max(rowCount, columnCount) * maximumSingularValue
            End If

            Dim reciprocalSingularValues(highestIndex, highestIndex) As Double
            For i As Integer = 0 To highestIndex
                Dim singularValue As Double = svd.SingularValues(i)
                reciprocalSingularValues(i, i) =
                    If(Math.Abs(singularValue) > tolerance, 1.0R / singularValue, 0.0R)
            Next

            Return MatrixArithmeticCore.Multiply(
                MatrixArithmeticCore.Multiply(svd.V, reciprocalSingularValues),
                MatrixArithmeticCore.Transpose(svd.U))
        End Function

        Public Function DecomposeSvd(matrix(,) As Double) As SvdResult
            Dim i As Integer
            Dim l As Integer
            Dim nm As Integer
            Dim j As Integer
            Dim kIndex As Integer
            Dim jj As Integer
            Dim iteration As Integer
            Dim c As Double
            Dim f As Double
            Dim h As Double
            Dim s As Double
            Dim x As Double
            Dim y As Double
            Dim z As Double
            Dim a(,) As Double = DirectCast(matrix.Clone(), Double(,))

            Dim m As Integer = a.GetUpperBound(0)
            Dim n As Integer = a.GetUpperBound(1)

            Dim singularValues(n) As Double
            Dim v(n, n) As Double
            Dim rv1(n) As Double

            Dim g As Double = 0.0R
            Dim scale As Double = 0.0R
            Dim anorm As Double = 0.0R

            ' Householder reduction to bidiagonal form.
            For i = 0 To n
                l = i + 1
                rv1(i) = scale * g
                g = 0.0R
                s = 0.0R
                scale = 0.0R

                If i <= m Then
                    For kIndex = i To m
                        scale += Math.Abs(a(kIndex, i))
                    Next
                    If scale <> 0.0R Then
                        For kIndex = i To m
                            a(kIndex, i) /= scale
                            s += a(kIndex, i) * a(kIndex, i)
                        Next

                        f = a(i, i)
                        If f >= 0.0R Then g = -Math.Sqrt(s) Else g = Math.Sqrt(s)
                        h = f * g - s
                        a(i, i) = f - g

                        For j = l To n
                            s = 0.0R
                            For kIndex = i To m
                                s += a(kIndex, i) * a(kIndex, j)
                            Next
                            f = s / h
                            For kIndex = i To m
                                a(kIndex, j) += f * a(kIndex, i)
                            Next
                        Next

                        For kIndex = i To m
                            a(kIndex, i) = scale * a(kIndex, i)
                        Next
                    End If
                End If

                singularValues(i) = scale * g
                g = 0.0R
                s = 0.0R
                scale = 0.0R

                If i <= m AndAlso i <> n Then
                    For kIndex = l To n
                        scale += Math.Abs(a(i, kIndex))
                    Next
                    If scale <> 0.0R Then
                        For kIndex = l To n
                            a(i, kIndex) /= scale
                            s += a(i, kIndex) * a(i, kIndex)
                        Next

                        f = a(i, l)
                        If f >= 0.0R Then g = -Math.Sqrt(s) Else g = Math.Sqrt(s)
                        h = f * g - s
                        a(i, l) = f - g

                        For kIndex = l To n
                            rv1(kIndex) = a(i, kIndex) / h
                        Next

                        For j = l To m
                            s = 0.0R
                            For kIndex = l To n
                                s += a(j, kIndex) * a(i, kIndex)
                            Next
                            For kIndex = l To n
                                a(j, kIndex) += s * rv1(kIndex)
                            Next
                        Next

                        For kIndex = l To n
                            a(i, kIndex) = scale * a(i, kIndex)
                        Next
                    End If
                End If

                If anorm < Math.Abs(singularValues(i)) + Math.Abs(rv1(i)) Then
                    anorm = Math.Abs(singularValues(i)) + Math.Abs(rv1(i))
                End If
            Next

            ' Accumulation of right-hand transformations.
            For i = n To 0 Step -1
                If i < n Then
                    If g <> 0.0R Then
                        For j = l To n
                            v(j, i) = (a(i, j) / a(i, l)) / g
                        Next

                        For j = l To n
                            s = 0.0R
                            For kIndex = l To n
                                s += a(i, kIndex) * v(kIndex, j)
                            Next
                            For kIndex = l To n
                                v(kIndex, j) += s * v(kIndex, i)
                            Next
                        Next
                    End If

                    For j = l To n
                        v(i, j) = 0.0R
                        v(j, i) = 0.0R
                    Next
                End If

                v(i, i) = 1.0R
                g = rv1(i)
                l = i
            Next

            ' Accumulation of left-hand transformations.
            Dim minimumIndex As Integer = Math.Min(m, n)
            For i = minimumIndex To 0 Step -1
                l = i + 1
                g = singularValues(i)

                For j = l To n
                    a(i, j) = 0.0R
                Next

                If g <> 0.0R Then
                    g = 1.0R / g
                    For j = l To n
                        s = 0.0R
                        For kIndex = l To m
                            s += a(kIndex, i) * a(kIndex, j)
                        Next
                        f = (s / a(i, i)) * g
                        For kIndex = i To m
                            a(kIndex, j) += f * a(kIndex, i)
                        Next
                    Next

                    For j = i To m
                        a(j, i) *= g
                    Next
                Else
                    For j = i To m
                        a(j, i) = 0.0R
                    Next
                End If
                a(i, i) += 1.0R
            Next

            ' Diagonalization of the bidiagonal form.
            For kIndex = n To 0 Step -1
                For iteration = 1 To 100
                    For l = kIndex To 0 Step -1
                        nm = l - 1
                        If Math.Abs(rv1(l)) + anorm = anorm Then GoTo SplitOk
                        If Math.Abs(singularValues(nm)) + anorm = anorm Then Exit For
                    Next

                    c = 0.0R
                    s = 1.0R

                    For i = l To kIndex
                        nm = i - 1
                        f = s * rv1(i)
                        rv1(i) = c * rv1(i)
                        If Math.Abs(f) + anorm = anorm Then Exit For

                        g = singularValues(i)
                        h = Pythag(f, g)
                        singularValues(i) = h
                        h = 1.0R / h
                        c = g * h
                        s = -(f * h)

                        For j = 0 To m
                            y = a(j, nm)
                            z = a(j, i)
                            a(j, nm) = y * c + z * s
                            a(j, i) = -(y * s) + z * c
                        Next
                    Next

SplitOk:
                    z = singularValues(kIndex)
                    If l = kIndex Then
                        If z < 0.0R Then
                            singularValues(kIndex) = -z
                            For j = 0 To n
                                v(j, kIndex) = -v(j, kIndex)
                            Next
                        End If
                        Exit For
                    End If

                    If iteration = 30 Then
                        CoreServices.Log("SVD: No convergence!", LogMsgType.Warn)
                    End If

                    x = singularValues(l)
                    nm = kIndex - 1
                    y = singularValues(nm)
                    g = rv1(nm)
                    h = rv1(kIndex)
                    f = ((y - z) * (y + z) + (g - h) * (g + h)) / (2.0R * h * y)
                    g = Pythag(f, 1.0R)
                    If f >= 0.0R Then g = Math.Abs(g) Else g = -Math.Abs(g)

                    f = ((x - z) * (x + z) + h * ((y / (f + g)) - h)) / x
                    c = 1.0R
                    s = 1.0R

                    For j = l To nm
                        i = j + 1
                        g = rv1(i)
                        y = singularValues(i)
                        h = s * g
                        g = c * g
                        z = Pythag(f, h)
                        rv1(j) = z
                        c = f / z
                        s = h / z
                        f = x * c + g * s
                        g = -(x * s) + g * c
                        h = y * s
                        y *= c

                        For jj = 0 To n
                            x = v(jj, j)
                            z = v(jj, i)
                            v(jj, j) = x * c + z * s
                            v(jj, i) = -(x * s) + z * c
                        Next

                        z = Pythag(f, h)
                        singularValues(j) = z
                        If z <> 0.0R Then
                            z = 1.0R / z
                            c = f * z
                            s = h * z
                        End If

                        f = c * g + s * y
                        x = -(s * g) + c * y

                        For jj = 0 To m
                            y = a(jj, j)
                            z = a(jj, i)
                            a(jj, j) = y * c + z * s
                            a(jj, i) = -(y * s) + z * c
                        Next
                    Next

                    rv1(l) = 0.0R
                    rv1(kIndex) = f
                    singularValues(kIndex) = x
                Next
            Next

            Return New SvdResult With {
                .U = a,
                .SingularValues = singularValues,
                .SingularValueMatrix = DiagonalMatrixFromVector(singularValues),
                .V = v
            }
        End Function

        Public Function SolveQr(qr As QrResult, rightHandSide(,) As Double) As Double(,)
            Dim qtTimesB(,) As Double = MatrixArithmeticCore.Multiply(
                MatrixArithmeticCore.Transpose(qr.Q), rightHandSide)
            Dim highestIndex As Integer = qr.R.GetUpperBound(0)
            Dim solution(highestIndex, rightHandSide.GetUpperBound(1)) As Double

            For column As Integer = 0 To rightHandSide.GetUpperBound(1)
                For i As Integer = highestIndex To 0 Step -1
                    Dim sum As Double = 0.0R
                    For j As Integer = i + 1 To highestIndex
                        sum += qr.R(i, j) * solution(j, column)
                    Next
                    solution(i, column) = (qtTimesB(i, column) - sum) / qr.R(i, i)
                Next
            Next

            Return solution
        End Function

        Public Function DecomposeQr(matrix(,) As Double,
                                    Optional precision As Double = 0.000000000001R) As QrResult
            If matrix Is Nothing Then
                CoreServices.Errors.LogAndThrow(New ArgumentNullException(NameOf(matrix)))
            End If

            Dim rowCount As Integer = matrix.GetUpperBound(0) + 1
            Dim columnCount As Integer = matrix.GetUpperBound(1) + 1

            If rowCount <= 0 OrElse columnCount <= 0 Then
                CoreServices.Errors.LogAndThrow(New ArgumentException("Input matrix must be non-empty."))
            End If

            If rowCount < columnCount Then
                CoreServices.Errors.LogAndThrow(
                    New ArgumentException("QRdecomp requires rows >= columns (m >= n)."))
            End If

            Dim rWorking(,) As Double = DirectCast(matrix.Clone(), Double(,))
            Dim reflectors As New List(Of Double())()
            Dim taus As New List(Of Double)()

            For k As Integer = 0 To columnCount - 1
                Dim length As Integer = rowCount - k
                Dim x(length - 1) As Double

                For i As Integer = 0 To length - 1
                    x(i) = rWorking(k + i, k)
                Next

                Dim scale As Double = 0.0R
                Dim sumSquares As Double = 1.0R
                For i As Integer = 0 To length - 1
                    Dim absoluteX As Double = Math.Abs(x(i))
                    If absoluteX <> 0.0R Then
                        If scale < absoluteX Then
                            Dim ratio As Double = If(scale = 0.0R, 0.0R, scale / absoluteX)
                            sumSquares = 1.0R + sumSquares * ratio * ratio
                            scale = absoluteX
                        Else
                            Dim ratio As Double = absoluteX / scale
                            sumSquares += ratio * ratio
                        End If
                    End If
                Next
                Dim normX As Double = If(scale = 0.0R, 0.0R, scale * Math.Sqrt(sumSquares))

                Dim v(length - 1) As Double
                Dim tau As Double = 0.0R

                If normX <= precision Then
                    v(0) = 1.0R
                    reflectors.Add(v)
                    taus.Add(tau)
                    Continue For
                End If

                Array.Copy(x, v, length)

                Dim alpha As Double = x(0)
                Dim signAlpha As Double = If(alpha >= 0.0R, 1.0R, -1.0R)
                Dim beta As Double = -signAlpha * normX
                Dim denominator As Double = alpha - beta

                v(0) = 1.0R
                If Math.Abs(denominator) > precision Then
                    For i As Integer = 1 To length - 1
                        v(i) /= denominator
                    Next
                    tau = (beta - alpha) / beta
                Else
                    tau = 0.0R
                End If

                reflectors.Add(v)
                taus.Add(tau)

                If tau <> 0.0R Then
                    For j As Integer = k To columnCount - 1
                        Dim dot As Double = 0.0R
                        For i As Integer = 0 To length - 1
                            dot += v(i) * rWorking(k + i, j)
                        Next
                        dot *= tau

                        For i As Integer = 0 To length - 1
                            rWorking(k + i, j) -= dot * v(i)
                        Next
                    Next
                End If

                rWorking(k, k) = beta
                For i As Integer = k + 1 To rowCount - 1
                    rWorking(i, k) = 0.0R
                Next
            Next

            Dim qThin(rowCount - 1, columnCount - 1) As Double
            For i As Integer = 0 To rowCount - 1
                For j As Integer = 0 To columnCount - 1
                    qThin(i, j) = If(i = j, 1.0R, 0.0R)
                Next
            Next

            For k As Integer = columnCount - 1 To 0 Step -1
                Dim v() As Double = reflectors(k)
                Dim tau As Double = taus(k)
                Dim length As Integer = v.Length

                If tau <> 0.0R Then
                    For j As Integer = 0 To columnCount - 1
                        Dim dot As Double = 0.0R
                        For i As Integer = 0 To length - 1
                            dot += v(i) * qThin(k + i, j)
                        Next
                        dot *= tau

                        For i As Integer = 0 To length - 1
                            qThin(k + i, j) -= dot * v(i)
                        Next
                    Next
                End If
            Next

            Dim rUpper(columnCount - 1, columnCount - 1) As Double
            For i As Integer = 0 To columnCount - 1
                For j As Integer = i To columnCount - 1
                    rUpper(i, j) = rWorking(i, j)
                Next
            Next

            Return New QrResult With {
                .Q = qThin,
                .R = rUpper
            }
        End Function

        Private Function DiagonalMatrixFromVector(values() As Double) As Double(,)
            Dim output(values.GetUpperBound(0), values.GetUpperBound(0)) As Double
            For i As Integer = 0 To values.GetUpperBound(0)
                output(i, i) = values(i)
            Next
            Return output
        End Function

        Private Function Pythag(x As Double, y As Double) As Double
            Dim absoluteX As Double = Math.Abs(x)
            Dim absoluteY As Double = Math.Abs(y)

            If absoluteX > absoluteY Then
                Return absoluteX * Math.Sqrt(1.0R + (absoluteY / absoluteX) ^ 2)
            End If

            If absoluteY = 0.0R Then Return 0.0R
            Return absoluteY * Math.Sqrt(1.0R + (absoluteX / absoluteY) ^ 2)
        End Function

        Private Function FormatMatrix(matrix(,) As Double) As String
            Dim text As String = "["
            For i As Integer = 0 To matrix.GetUpperBound(0)
                text &= "["
                For j As Integer = 0 To matrix.GetUpperBound(1)
                    text &= matrix(i, j).ToString()
                    If j < matrix.GetUpperBound(1) Then text &= ", "
                Next
                text &= "]"
                If i < matrix.GetUpperBound(0) Then text &= "; "
            Next
            text &= "]"
            Return text
        End Function

    End Module

End Namespace
