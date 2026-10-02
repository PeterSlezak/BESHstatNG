Option Explicit On
Option Strict On

Imports System
Imports BESHStatNG.AppInfrastructure

Namespace Matrix

    ''' <summary>
    ''' Host-neutral Cholesky and LU factorization primitives. Behavior intentionally
    ''' matches the established BESHStatNG routines during the compatibility migration.
    ''' </summary>
    Friend Module MatrixFactorizationCore

        Public NotInheritable Class LUDecompositionResult
            Public Property Factors As Double(,)
            Public Property PivotIndices As Double()
        End Class

        Public Function Cholesky(a(,) As Double,
                                 ByRef Optional fault As Integer = 0,
                                 Optional errorRaise As Boolean = True) As Double(,)
            Dim sum As Double
            Dim n As Integer = a.GetUpperBound(0)
            Dim lower(n, n) As Double

            For i As Integer = 0 To n
                sum = 0.0R
                For j As Integer = 0 To i - 1
                    sum += lower(i, j) * lower(i, j)
                Next

                lower(i, i) = a(i, i) - sum
                If lower(i, i) <= 0.0R Then
                    fault = 2
                    If errorRaise Then
                        CoreServices.Errors.LogAndThrow(
                            New ApplicationException($"MatrixType not positive-definite. {FormatMatrix(a)}"))
                    End If
                    Return lower
                End If

                lower(i, i) = Math.Sqrt(lower(i, i))

                For k As Integer = i + 1 To n
                    sum = 0.0R
                    For j As Integer = 0 To i - 1
                        sum += lower(k, j) * lower(i, j)
                    Next
                    lower(k, i) = (a(k, i) - sum) / lower(i, i)
                Next
            Next

            Return lower
        End Function

        Public Function CholeskySolve(lower(,) As Double, b() As Double) As Double()
            Dim temp As Double
            Dim rows As Integer = lower.GetUpperBound(0)
            Dim y(rows) As Double
            Dim x(rows) As Double

            For i As Integer = 0 To rows
                temp = b(i)
                For j As Integer = i - 1 To 0 Step -1
                    temp -= lower(i, j) * y(j)
                Next
                y(i) = temp / lower(i, i)
            Next

            For i As Integer = rows To 0 Step -1
                temp = y(i)
                For j As Integer = i + 1 To rows
                    temp -= lower(j, i) * x(j)
                Next
                x(i) = temp / lower(i, i)
            Next

            Return x
        End Function

        Public Function CholeskySolve(lower(,) As Double, b(,) As Double) As Double(,)
            Dim temp As Double
            Dim rows As Integer = lower.GetUpperBound(0)
            Dim y(rows, b.GetUpperBound(1)) As Double
            Dim x(rows, b.GetUpperBound(1)) As Double

            For rhs As Integer = 0 To b.GetUpperBound(1)
                For i As Integer = 0 To rows
                    temp = b(i, rhs)
                    For j As Integer = i - 1 To 0 Step -1
                        temp -= lower(i, j) * y(j, rhs)
                    Next
                    y(i, rhs) = temp / lower(i, i)
                Next

                For i As Integer = rows To 0 Step -1
                    temp = y(i, rhs)
                    For j As Integer = i + 1 To rows
                        temp -= lower(j, i) * x(j, rhs)
                    Next
                    x(i, rhs) = temp / lower(i, i)
                Next
            Next

            Return x
        End Function

        Public Function CholeskyInverse(lower(,) As Double) As Double(,)
            Dim p As Integer = lower.GetUpperBound(0)
            Dim upper(p, p) As Double
            Dim inverse(p, p) As Double

            For j As Integer = p To 0 Step -1
                upper(j, j) = 1.0R / lower(j, j)
                For k As Integer = j - 1 To 0 Step -1
                    For i As Integer = k + 1 To j
                        upper(k, j) -= lower(i, k) * upper(i, j) / lower(k, k)
                    Next
                Next
            Next

            For i As Integer = 0 To p
                For j As Integer = i To p
                    For k As Integer = j To p
                        inverse(i, j) += upper(i, k) * upper(j, k)
                    Next
                    inverse(j, i) = inverse(i, j)
                Next
            Next

            Return inverse
        End Function

        Public Function LUDecompose(matrix(,) As Double,
                                    ByRef parity As Double,
                                    ByRef Optional errorCode As Integer = 0) As LUDecompositionResult
            Const Tiny As Double = 0.000000000000002R

            Dim a(,) As Double = DirectCast(matrix.Clone(), Double(,))
            Dim n As Integer = a.GetUpperBound(0)
            If n <> a.GetUpperBound(1) Then
                CoreServices.Errors.LogAndThrow(New ArgumentException("Input matrix is not squared."))
            End If

            Dim scaling(n) As Double
            Dim pivotIndices(n) As Double

            For i As Integer = 0 To n
                Dim largest As Double = 0.0R
                For j As Integer = 0 To n
                    Dim candidate As Double = Math.Abs(a(i, j))
                    If candidate > largest Then largest = candidate
                Next

                If largest = 0.0R Then
                    errorCode = 2
                    CoreServices.Errors.LogAndThrow(New ApplicationException("Singular matrix."))
                End If
                scaling(i) = 1.0R / largest
            Next

            For j As Integer = 0 To n
                Dim sum As Double

                For i As Integer = 0 To j - 1
                    sum = a(i, j)
                    For k As Integer = 0 To i - 1
                        sum -= a(i, k) * a(k, j)
                    Next
                    a(i, j) = sum
                Next

                Dim largest As Double = 0.0R
                Dim pivotRow As Integer = 0

                For i As Integer = j To n
                    sum = a(i, j)
                    For k As Integer = 0 To j - 1
                        sum -= a(i, k) * a(k, j)
                    Next
                    a(i, j) = sum

                    Dim merit As Double = scaling(i) * Math.Abs(sum)
                    If merit >= largest Then
                        pivotRow = i
                        largest = merit
                    End If
                Next

                If j <> pivotRow Then
                    For k As Integer = 0 To n
                        Dim temp As Double = a(pivotRow, k)
                        a(pivotRow, k) = a(j, k)
                        a(j, k) = temp
                    Next
                    parity = -parity
                    scaling(pivotRow) = scaling(j)
                End If

                pivotIndices(j) = pivotRow
                If a(j, j) = 0.0R Then a(j, j) = Tiny

                If j <> n Then
                    Dim reciprocalPivot As Double = 1.0R / a(j, j)
                    For i As Integer = j + 1 To n
                        a(i, j) *= reciprocalPivot
                    Next
                End If
            Next

            Return New LUDecompositionResult With {
                .Factors = a,
                .PivotIndices = pivotIndices
            }
        End Function

        Public Function LUSolve(lu As LUDecompositionResult, rightHandSideVector() As Double) As Double()
            Dim factors(,) As Double = lu.Factors
            Dim pivotIndices() As Double = lu.PivotIndices
            Dim b() As Double = rightHandSideVector
            Dim n As Integer = factors.GetUpperBound(0)

            ' Preserve the legacy conjunction semantics during migration.
            If n <> factors.GetUpperBound(1) And
               n <> pivotIndices.GetUpperBound(0) And
               n <> b.GetUpperBound(0) Then
                CoreServices.Errors.LogAndThrow(New ArgumentException("Wrong input matrices dimensions."))
            End If

            Dim firstNonzero As Integer = -1
            For i As Integer = 0 To n
                Dim pivotRow As Integer = Convert.ToInt32(pivotIndices(i))
                Dim sum As Double = b(pivotRow)
                b(pivotRow) = b(i)

                If firstNonzero <> -1 Then
                    For j As Integer = firstNonzero To i - 1
                        sum -= factors(i, j) * b(j)
                    Next
                ElseIf sum <> 0.0R Then
                    firstNonzero = i
                End If
                b(i) = sum
            Next

            For i As Integer = n To 0 Step -1
                Dim sum As Double = b(i)
                For j As Integer = i + 1 To n
                    sum -= factors(i, j) * b(j)
                Next
                b(i) = sum / factors(i, i)
            Next

            Return b
        End Function

        Public Function Determinant(matrix(,) As Double) As Double
            Dim n As Integer = matrix.GetLength(0)
            If n <> matrix.GetLength(1) Then Return Double.NaN

            Dim parity As Double = 1.0R
            Dim errorCode As Integer = 0
            Dim lu As LUDecompositionResult = LUDecompose(matrix, parity, errorCode)

            If errorCode <> 0 Then Return 0.0R

            Dim determinantValue As Double = parity
            For i As Integer = 0 To n - 1
                determinantValue *= lu.Factors(i, i)
            Next
            Return determinantValue
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
