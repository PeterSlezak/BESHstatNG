Option Explicit On
Option Strict On

Imports System
Imports BESHStatNG.AppInfrastructure

Namespace Matrix

    ''' <summary>
    ''' Host-neutral foundational matrix/vector arithmetic used by statistical models.
    ''' The methods intentionally preserve the established BESHStatNG numerical and
    ''' exception semantics so the .NET Framework facade can forward to this implementation.
    ''' </summary>
    Friend Module MatrixArithmeticCore

        Public Function OuterProduct(Of T)(mat1() As T, mat2() As T) As T(,)
            Dim output(mat1.Length - 1, mat2.Length - 1) As T
            Dim outputArray As Array = output

            If Not (GetType(T) Is GetType(Double) OrElse
                    GetType(T) Is GetType(Integer) OrElse
                    GetType(T) Is GetType(Single) OrElse
                    GetType(T) Is GetType(Long)) Then
                CoreServices.Errors.LogAndThrow(
                    New NotSupportedException($"Type {GetType(T).Name} is not supported. Allowed types: Double, Integer, Single, Long."))
            End If

            For i As Integer = 0 To mat1.Length - 1
                For j As Integer = 0 To mat2.Length - 1
                    Dim value As Double = Convert.ToDouble(mat1(i)) * Convert.ToDouble(mat2(j))
                    outputArray.SetValue(Convert.ChangeType(value, GetType(T)), i, j)
                Next
            Next

            Return output
        End Function

        Public Function Trace(a(,) As Double) As Double
            If a Is Nothing Then Return Double.NaN
            If a.GetLength(0) <> a.GetLength(1) Then Return Double.NaN

            Dim output As Double = 0.0R
            For i As Integer = 0 To a.GetLength(0) - 1
                output += a(i, i)
            Next
            Return output
        End Function

        Public Function Multiply(matrix1(,) As Double, matrix2(,) As Double) As Double(,)
            Dim noRow1 As Integer = matrix1.GetUpperBound(0)
            Dim noRow2 As Integer = matrix2.GetUpperBound(0)
            Dim noColumn1 As Integer = matrix1.GetUpperBound(1)
            Dim noColumn2 As Integer = matrix2.GetUpperBound(1)

            If noRow2 <> noColumn1 Then
                CoreServices.Errors.LogAndThrow(New ArgumentException("Inapropriate matrix dimensions in input matrix."))
            End If

            Dim output(noRow1, noColumn2) As Double
            For i As Integer = 0 To noRow1
                For j As Integer = 0 To noColumn2
                    Dim temp As Double = 0.0R
                    For k As Integer = 0 To noRow2
                        temp += matrix1(i, k) * matrix2(k, j)
                    Next
                    output(i, j) = temp
                Next
            Next
            Return output
        End Function

        Public Function Multiply(vector() As Double, matrix(,) As Double) As Double(,)
            Dim vector2D(0, vector.Length - 1) As Double
            For i As Integer = 0 To vector.Length - 1
                vector2D(0, i) = vector(i)
            Next
            Return Multiply(vector2D, matrix)
        End Function

        Public Function Multiply(matrix(,) As Double, vector() As Double) As Double(,)
            Dim vector2D(vector.Length - 1, 0) As Double
            For i As Integer = 0 To vector.Length - 1
                vector2D(i, 0) = vector(i)
            Next
            Return Multiply(matrix, vector2D)
        End Function

        Public Function Multiply(matrix(,) As Double, scalar As Double) As Double(,)
            Dim output(matrix.GetUpperBound(0), matrix.GetUpperBound(1)) As Double
            For i As Integer = 0 To matrix.GetUpperBound(0)
                For j As Integer = 0 To matrix.GetUpperBound(1)
                    output(i, j) = matrix(i, j) * scalar
                Next
            Next
            Return output
        End Function

        Public Function Multiply(vector() As Double, scalar As Double) As Double()
            Dim output(vector.Length - 1) As Double
            For i As Integer = 0 To vector.Length - 1
                output(i) = vector(i) * scalar
            Next
            Return output
        End Function

        Public Function NegateVector(values() As Double) As Double()
            If values Is Nothing Then Return Nothing
            Return Multiply(values, -1.0R)
        End Function

        Public Function MultiplyVector(matrix(,) As Double, vector() As Double) As Double()
            If matrix Is Nothing Then Throw New ArgumentNullException(NameOf(matrix))
            If vector Is Nothing Then Throw New ArgumentNullException(NameOf(vector))
            If matrix.GetLength(1) <> vector.Length Then
                Throw New ApplicationException($"MatrixVectorMultiply dimension mismatch: matrix columns={matrix.GetLength(1)}, vector length={vector.Length}.")
            End If

            Dim temp(,) As Double = Multiply(matrix, vector)
            Dim output(temp.GetLength(0) - 1) As Double
            For i As Integer = 0 To temp.GetLength(0) - 1
                output(i) = temp(i, 0)
            Next
            Return output
        End Function

        Public Function DotProduct(a() As Double, b() As Double) As Double
            If a Is Nothing Then Throw New ArgumentNullException(NameOf(a))
            If b Is Nothing Then Throw New ArgumentNullException(NameOf(b))
            If a.Length <> b.Length Then Throw New ApplicationException("DotProduct vectors must have the same length.")

            Dim output As Double = 0.0R
            For i As Integer = 0 To a.Length - 1
                output += a(i) * b(i)
            Next
            Return output
        End Function

        Public Function Add(mat1(,) As Double, mat2(,) As Double) As Double(,)
            If mat1.GetLength(0) <> mat2.GetLength(0) Then
                CoreServices.Errors.LogAndThrow(New ArgumentException("1st dimension of input matrices is not equal."))
            End If
            If mat1.GetLength(1) <> mat2.GetLength(1) Then
                CoreServices.Errors.LogAndThrow(New ArgumentException("2st dimension of input matrices is not equal."))
            End If

            Dim output(mat1.GetUpperBound(0), mat1.GetUpperBound(1)) As Double
            For i As Integer = 0 To mat1.GetUpperBound(0)
                For j As Integer = 0 To mat1.GetUpperBound(1)
                    output(i, j) = mat1(i, j) + mat2(i, j)
                Next
            Next
            Return output
        End Function

        Public Function QuadraticForm(v() As Double, a(,) As Double) As Double
            If v Is Nothing OrElse a Is Nothing Then Return Double.NaN

            Dim p As Integer = v.Length
            If a.GetLength(0) <> p OrElse a.GetLength(1) <> p Then Return Double.NaN

            Dim value As Double = 0.0R
            For i As Integer = 0 To p - 1
                Dim vi As Double = v(i)
                For j As Integer = 0 To p - 1
                    value += vi * a(i, j) * v(j)
                Next
            Next
            Return value
        End Function

        Public Function Add(mat1(,) As Double, mat2() As Double) As Double(,)
            If mat1.GetLength(0) <> mat2.Length Then
                CoreServices.Errors.LogAndThrow(New ArgumentException("1st dimension of input matrices is not equal."))
            End If

            ' Preserve the established allocation semantics of the legacy implementation.
            Dim output(mat1.GetUpperBound(0), mat2.Length - 1) As Double
            For i As Integer = 0 To mat1.GetUpperBound(0)
                For j As Integer = 0 To mat1.GetUpperBound(1)
                    output(i, j) = mat1(i, j) + mat2(i)
                Next
            Next
            Return output
        End Function

        Public Function Add(mat1() As Double, mat2() As Double) As Double()
            If mat1.Length <> mat2.Length Then
                CoreServices.Errors.LogAndThrow(New ApplicationException("1st dimension of input matrices is not equal."))
            End If

            Dim output(mat1.Length - 1) As Double
            For i As Integer = 0 To mat1.Length - 1
                output(i) = mat1(i) + mat2(i)
            Next
            Return output
        End Function

        Public Function Add(mat1() As Double, scalar As Double) As Double()
            Dim output(mat1.Length - 1) As Double
            For i As Integer = 0 To mat1.Length - 1
                output(i) = mat1(i) + scalar
            Next
            Return output
        End Function

        Public Function Subtract(mat1(,) As Double, mat2(,) As Double) As Double(,)
            If mat1.GetUpperBound(0) <> mat2.GetUpperBound(0) Then
                CoreServices.Errors.LogAndThrow(New ArgumentException("1st dimension of input matrices is not equal."))
            End If
            If mat1.GetUpperBound(1) <> mat2.GetUpperBound(1) Then
                CoreServices.Errors.LogAndThrow(New ArgumentException("2st dimension of input matrices is not equal."))
            End If

            Dim output(mat1.GetUpperBound(0), mat1.GetUpperBound(1)) As Double
            For i As Integer = 0 To mat1.GetUpperBound(0)
                For j As Integer = 0 To mat1.GetUpperBound(1)
                    output(i, j) = mat1(i, j) - mat2(i, j)
                Next
            Next
            Return output
        End Function

        Public Function Subtract(mat1() As Double, mat2() As Double) As Double()
            If mat1.Length <> mat2.Length Then
                CoreServices.Errors.LogAndThrow(New ArgumentException("1st dimension of input matrices is not equal."))
            End If

            Dim output(mat1.Length - 1) As Double
            For i As Integer = 0 To mat1.Length - 1
                output(i) = mat1(i) - mat2(i)
            Next
            Return output
        End Function

        Public Function Subtract(mat1() As Double, scalar As Double) As Double()
            Dim output(mat1.Length - 1) As Double
            For i As Integer = 0 To mat1.Length - 1
                output(i) = mat1(i) - scalar
            Next
            Return output
        End Function

        Public Function Divide(mat1(,) As Double, mat2(,) As Double, ByRef Optional trace As String = "") As Double(,)
            If mat1.GetLength(0) <> mat2.GetLength(0) Then
                CoreServices.Errors.LogAndThrow(New ArgumentException("1st dimension of input matrices is not equal."))
            End If
            If mat1.GetLength(1) <> mat2.GetLength(1) Then
                CoreServices.Errors.LogAndThrow(New ArgumentException("2st dimension of input matrices is not equal."))
            End If

            Dim output(mat1.GetUpperBound(0), mat1.GetUpperBound(1)) As Double
            For i As Integer = 0 To mat1.GetUpperBound(0)
                For j As Integer = 0 To mat1.GetUpperBound(1)
                    If mat2(i, j) <> 0.0R Then
                        output(i, j) = mat1(i, j) / mat2(i, j)
                    Else
                        trace &= " WARNING: M_DIV Division by zero. mat2="
                    End If
                Next
            Next
            Return output
        End Function

        Public Function Divide(mat1() As Double, mat2() As Double, ByRef Optional trace As String = "") As Double()
            If mat1.Length <> mat2.Length Then
                CoreServices.Errors.LogAndThrow(New ArgumentException("1st dimension of input matrices is not equal."))
            End If

            Dim output(mat1.Length - 1) As Double
            For i As Integer = 0 To mat1.Length - 1
                If mat2(i) <> 0.0R Then
                    output(i) = mat1(i) / mat2(i)
                Else
                    trace &= " WARNING: M_DIV Division by zero. mat2="
                End If
            Next
            Return output
        End Function

        Public Function Divide(mat1(,) As Double, mat2() As Double, ByRef Optional trace As String = "") As Double(,)
            If mat1.GetLength(0) <> mat2.Length Then
                CoreServices.Errors.LogAndThrow(New ArgumentException("1st dimension of input matrices is not equal."))
            End If

            Dim output(mat1.GetUpperBound(0), mat1.GetUpperBound(1)) As Double
            For i As Integer = 0 To mat1.GetUpperBound(0)
                For j As Integer = 0 To mat1.GetUpperBound(1)
                    If mat2(i) <> 0.0R Then
                        output(i, j) = mat1(i, j) / mat2(i)
                    Else
                        trace &= " WARNING: M_DIV Division by zero. mat2="
                    End If
                Next
            Next
            Return output
        End Function

        Public Function Divide(mat1(,) As Double, scalar As Double, ByRef Optional trace As String = "") As Double(,)
            Dim output(mat1.GetUpperBound(0), mat1.GetUpperBound(1)) As Double
            For i As Integer = 0 To mat1.GetUpperBound(0)
                For j As Integer = 0 To mat1.GetUpperBound(1)
                    If scalar <> 0.0R Then
                        output(i, j) = mat1(i, j) / scalar
                    Else
                        trace &= " WARNING: M_DIV Division by zero. mat2="
                    End If
                Next
            Next
            Return output
        End Function

        Public Function Divide(mat1() As Double, scalar As Double, ByRef Optional trace As String = "") As Double()
            Dim output(mat1.Length - 1) As Double
            For i As Integer = 0 To mat1.Length - 1
                If scalar <> 0.0R Then
                    output(i) = mat1(i) / scalar
                Else
                    trace &= " WARNING: M_DIV Division by zero. mat2="
                End If
            Next
            Return output
        End Function

        Public Function IdentityMatrix(highestIndex As Integer) As Double(,)
            Dim output(highestIndex, highestIndex) As Double
            For i As Integer = 0 To highestIndex
                output(i, i) = 1.0R
            Next
            Return output
        End Function

        Public Function VectorNorm(values() As Double) As Double
            If values Is Nothing Then Return Double.NaN

            Dim sumSquares As Double = 0.0R
            For i As Integer = 0 To values.Length - 1
                sumSquares += values(i) * values(i)
            Next
            Return Math.Sqrt(sumSquares)
        End Function

        Public Function MatrixIsFinite(matrix(,) As Double) As Boolean
            If matrix Is Nothing Then Return False
            For row As Integer = 0 To matrix.GetLength(0) - 1
                For column As Integer = 0 To matrix.GetLength(1) - 1
                    If Not NumericGuards.IsFinite(matrix(row, column)) Then Return False
                Next
            Next
            Return True
        End Function

        Public Function VectorIsFinite(values() As Double) As Boolean
            If values Is Nothing Then Return False
            For Each value As Double In values
                If Not NumericGuards.IsFinite(value) Then Return False
            Next
            Return True
        End Function

        Public Function MatrixIsFiniteAndSymmetric(matrix(,) As Double,
                                                    tolerance As Double,
                                                    ByRef message As String) As Boolean
            message = String.Empty

            If matrix Is Nothing Then
                message = "matrix is Nothing."
                Return False
            End If

            If matrix.GetLength(0) <> matrix.GetLength(1) Then
                message = "matrix is not square."
                Return False
            End If

            Dim n As Integer = matrix.GetLength(0)
            For row As Integer = 0 To n - 1
                For column As Integer = 0 To n - 1
                    Dim value As Double = matrix(row, column)
                    If Not NumericGuards.IsFinite(value) Then
                        message = "matrix contains a non-finite value at (" & row.ToString() & ", " & column.ToString() & ")."
                        Return False
                    End If

                    If Math.Abs(value - matrix(column, row)) > tolerance Then
                        message = "matrix is not symmetric within tolerance at (" & row.ToString() & ", " & column.ToString() & ")."
                        Return False
                    End If
                Next
            Next

            Return True
        End Function

        Public Function ConstantVector(highestIndex As Integer, Optional value As Double = 1.0R) As Double()
            Dim output(highestIndex) As Double
            For i As Integer = 0 To highestIndex
                output(i) = value
            Next
            Return output
        End Function

        Public Function Transpose(Of T)(matrix(,) As T) As T(,)
            Dim output(matrix.GetUpperBound(1), matrix.GetUpperBound(0)) As T
            For i As Integer = 0 To matrix.GetUpperBound(0)
                For j As Integer = 0 To matrix.GetUpperBound(1)
                    output(j, i) = matrix(i, j)
                Next
            Next
            Return output
        End Function

    End Module

End Namespace
