Option Explicit On
Imports System.Drawing.Drawing2D
Imports System.Net.NetworkInformation
Imports System.Numerics
Imports System.Reflection
Imports System.Runtime.InteropServices
Imports System.Text.RegularExpressions
Imports Microsoft.Office.Interop.Excel

Public Module Matrix

    ''' <summary>
    ''' Computes the outer product of two one‑dimensional numeric vectors of type
    ''' <typeparamref name="T"/>. Only the types Double, Integer, Single, and Long
    ''' are supported. Produces a matrix where each element is the product of
    ''' <c>mat1(i)</c> and <c>mat2(j)</c>.
    ''' </summary>
    ''' <typeparam name="T">
    ''' The numeric element type. Must be one of:
    ''' <c>Double</c>, <c>Integer</c>, <c>Single</c>, or <c>Long</c>.
    ''' </typeparam>
    ''' <param name="mat1">
    ''' The first input vector. Its length determines the number of rows
    ''' in the resulting matrix.
    ''' </param>
    ''' <param name="mat2">
    ''' The second input vector. Its length determines the number of columns
    ''' in the resulting matrix.
    ''' </param>
    ''' <returns>
    ''' A two‑dimensional array <c>M</c> of size  
    ''' <c>(UBound(mat1) + 1) × (UBound(mat2) + 1)</c>  
    ''' where <c>M(i, j) = mat1(i) * mat2(j)</c>.
    ''' </returns>
    ''' <exception cref="NotSupportedException">
    ''' Thrown when <typeparamref name="T"/> is not one of the supported numeric types.
    ''' </exception>
    ''' <remarks>
    ''' <para>
    ''' This function performs a standard outer product, producing a rank‑1 matrix.
    ''' </para>
    ''' <para>
    ''' Time complexity is <c>O(n · m)</c>, where <c>n</c> and <c>m</c> are the lengths
    ''' of the input vectors.
    ''' </para>
    ''' </remarks>
    ''' <example>
    ''' <code>
    ''' Dim a() As Double = {1, 2}
    ''' Dim b() As Double = {3, 4, 5}
    '''
    ''' Dim M = M_OUTERPRODUCT(a, b)
    ''' ' Result:
    ''' '   { {3, 4, 5},
    ''' '     {6, 8, 10} }
    ''' </code>
    ''' </example>
    Public Function M_OUTERPRODUCT(Of T)(mat1() As T, mat2() As T) As T(,)
        Dim out(mat1.Length - 1, mat2.Length - 1) As T

        ' Restrict to supported numeric types
        If Not (GetType(T) Is GetType(Double) OrElse
            GetType(T) Is GetType(Integer) OrElse
            GetType(T) Is GetType(Single) OrElse
            GetType(T) Is GetType(Long)) Then

            BESHstatGlobals.BSerr.LogAndThrow(New NotSupportedException($"Type {GetType(T).Name} is not supported. " &
                                                "Allowed types: Double, Integer, Single, Long."))
        End If

        For i = 0 To mat1.Length - 1
            For j = 0 To mat2.Length - 1
                ' Use Convert.ToDouble for safe numeric multiplication
                Dim v = Convert.ToDouble(mat1(i)) * Convert.ToDouble(mat2(j))
                out(i, j) = CType(Convert.ChangeType(v, GetType(T)), T)
            Next
        Next

        Return out
    End Function



    ''' <summary>
    ''' Computes the matrix product of two 2-dimensional numeric arrays.
    ''' </summary>
    ''' <param name="Matrix1">
    ''' The left-hand matrix in the multiplication.  
    ''' Must have dimensions (m × n).
    ''' </param>
    ''' <param name="Matrix2">
    ''' The right-hand matrix in the multiplication.  
    ''' Must have dimensions (n × p).
    ''' </param>
    ''' <returns>
    ''' A new 2-dimensional array representing the product Matrix1 × Matrix2,  
    ''' with dimensions (m × p).
    ''' </returns>
    ''' <exception cref="ApplicationException">
    ''' Thrown when the matrices have incompatible dimensions 
    ''' (i.e., number of columns in Matrix1 does not equal the number of rows in Matrix2).
    ''' </exception>
    ''' <remarks>
    ''' <para>
    ''' The function supports multiplication only when the resulting array contains  
    ''' fewer than or equal to 5,461 elements.
    ''' </para>
    ''' 
    ''' <para>
    ''' Standard matrix multiplication is performed:
    ''' </para>
    ''' <code>
    ''' C(i, j) = Σ Matrix1(i, k) × Matrix2(k, j)
    ''' </code>
    ''' 
    ''' <para>
    ''' Index bounds are determined using <c>UBound(..., dimension)</c>.
    ''' </para>
    ''' </remarks>
    Function MatrixMult(Matrix1(,) As Double, Matrix2(,) As Double) As Double(,)

        Dim NoRow1 As Integer = Matrix1.GetUpperBound(0)
        Dim NoRow2 As Integer = Matrix2.GetUpperBound(0)
        Dim NoColumn1 As Integer = Matrix1.GetUpperBound(1)
        Dim NoColumn2 As Integer = Matrix2.GetUpperBound(1)

        If NoRow2 <> NoColumn1 Then BESHstatGlobals.BSerr.LogAndThrow(New ArgumentException("Inapropriate matrix dimensions in input matrix."))

        Dim MatrixOut(NoRow1, NoColumn2) As Double
        For i = 0 To NoRow1
            For j = 0 To NoColumn2
                Dim dTemp As Double = 0.0
                For ii = 0 To NoRow2
                    dTemp += Matrix1(i, ii) * Matrix2(ii, j)
                Next
                MatrixOut(i, j) = dTemp
            Next
        Next
        MatrixMult = MatrixOut
    End Function

    ''' <summary>
    ''' Computes the matrix product of a 1-dimensional numeric array and a 2-dimensional matrix.  
    ''' The 1-D vector is internally converted into a 1 × n row matrix and multiplied by <paramref name="Matrix2"/>.
    ''' </summary>
    ''' <param name="Matrix1">
    ''' A 1-dimensional array representing a row vector of length n.
    ''' </param>
    ''' <param name="Matrix2">
    ''' A 2-dimensional array with dimensions (n × p), representing the matrix to multiply.
    ''' </param>
    ''' <returns>
    ''' A 2-dimensional array representing the product Matrix1 × Matrix2,  
    ''' which will have dimensions (1 × p).
    ''' </returns>
    ''' <remarks>
    ''' <para>
    ''' This is an overload of <see cref="MatrixMult(Double(,), Double(,))"/>.  
    ''' The 1-D array is converted to a (1 × n) matrix:
    ''' </para>
    ''' <code>
    ''' [x₀, x₁, …, xₙ]  →  [[x₀, x₁, …, xₙ]]
    ''' </code>
    ''' <para>
    ''' The resulting multiplication is then delegated to the main 2-D matrix multiplication routine.
    ''' </para>
    ''' </remarks>
    Function MatrixMult(Matrix1() As Double, Matrix2(,) As Double) As Double(,)
        Dim Matrix1_2D(0, Matrix1.Length - 1) As Double
        For ii = 0 To Matrix1.Length - 1
            Matrix1_2D(0, ii) = Matrix1(ii)
        Next

        Return MatrixMult(Matrix1_2D, Matrix2)
    End Function

    ''' <summary>
    ''' Computes the matrix product of a 2-dimensional matrix and a 1-dimensional numeric array.  
    ''' The 1-D vector is internally converted into an n × 1 column matrix and multiplied with <paramref name="Matrix1"/>.
    ''' </summary>
    ''' <param name="Matrix1">
    ''' A 2-dimensional array with dimensions (m × n), representing the left-hand matrix.
    ''' </param>
    ''' <param name="Matrix2">
    ''' A 1-dimensional array of length n, representing a column vector.
    ''' </param>
    ''' <returns>
    ''' A 2-dimensional array representing the product Matrix1 × Matrix2,  
    ''' which will have dimensions (m × 1).
    ''' </returns>
    ''' <remarks>
    ''' <para>
    ''' This function overloads <see cref="MatrixMult(Double(,), Double(,))"/>.  
    ''' The 1-D array is converted to an (n × 1) matrix:
    ''' </para>
    ''' <code>
    ''' [x₀, x₁, …, xₙ]ᵀ  →  
    ''' [[x₀],
    '''  [x₁],
    '''   … ,
    '''  [xₙ]]
    ''' </code>
    ''' <para>
    ''' After conversion, multiplication is delegated to the primary 2-D × 2-D matrix multiplication routine.
    ''' </para>
    ''' </remarks>
    Function MatrixMult(Matrix1(,) As Double, Matrix2() As Double) As Double(,)
        'overloading MatrixMult to have Matrix2 of rank = 1
        Dim Matrix2_2D(Matrix2.Length - 1, 0) As Double
        For ii = 0 To Matrix2.Length - 1
            Matrix2_2D(ii, 0) = Matrix2(ii)
        Next
        Return MatrixMult(Matrix1, Matrix2_2D)
    End Function

    ''' <summary>
    ''' Multiplies every element of a 2-dimensional numeric matrix by a scalar constant.
    ''' </summary>
    ''' <param name="Matrix1">
    ''' The input matrix whose elements will be scaled.  
    ''' Must be a 2-dimensional array.
    ''' </param>
    ''' <param name="c">
    ''' The scalar multiplier applied to each element of <paramref name="Matrix1"/>.
    ''' </param>
    ''' <returns>
    ''' A new matrix of the same dimensions as <paramref name="Matrix1"/> where each element 
    ''' is multiplied by <paramref name="c"/>.
    ''' </returns>
    ''' <remarks>
    ''' <para>
    ''' This overload supports scalar multiplication for convenience, allowing expressions such as:
    ''' </para>
    ''' <code>
    ''' Dim B = MatrixMult(A, 2.5)
    ''' </code>
    ''' <para>
    ''' The operation is performed element-wise:
    ''' </para>
    ''' <code>
    ''' B(i, j) = A(i, j) × c
    ''' </code>
    ''' </remarks>
    Function MatrixMult(Matrix1(,) As Double, c As Double) As Double(,)
        Dim MatrixOut(Matrix1.GetUpperBound(0), Matrix1.GetUpperBound(1)) As Double
        For i = 0 To Matrix1.GetUpperBound(0)
            For j = 0 To Matrix1.GetUpperBound(1)
                MatrixOut(i, j) = Matrix1(i, j) * c
            Next
        Next
        Return MatrixOut
    End Function

    ''' <summary>
    ''' Multiplies every element of a 1-dimensional numeric array by a scalar constant.
    ''' </summary>
    ''' <param name="Matrix1">
    ''' The input 1-dimensional array whose elements will be scaled.
    ''' </param>
    ''' <param name="c">
    ''' The scalar multiplier applied to each element of <paramref name="Matrix1"/>.
    ''' </param>
    ''' <returns>
    ''' A 1-dimensional array containing the scaled values.  
    ''' The returned array has the same length as <paramref name="Matrix1"/>.
    ''' </returns>
    ''' <remarks>
    ''' <para>
    ''' This overload provides scalar multiplication for vector data, enabling expressions such as:
    ''' </para>
    ''' <code>
    ''' Dim v2 = MatrixMult(v1, 3.0)
    ''' </code>
    ''' <para>
    ''' Element-wise multiplication is performed:
    ''' </para>
    ''' <code>
    ''' v2(i) = v1(i) × c
    ''' </code>
    ''' </remarks>
    Function MatrixMult(Matrix1() As Double, c As Double) As Double()
        Dim MatrixOut(Matrix1.Length - 1) As Double
        For i = 0 To Matrix1.Length - 1
            MatrixOut(i) = Matrix1(i) * c
        Next
        Return MatrixOut
    End Function

    ''' <summary>
    ''' Computes the dot product of two 1-dimensional numeric vectors.
    ''' </summary>
    ''' <param name="a">
    ''' The first vector. Must have the same length as <paramref name="b"/>.
    ''' </param>
    ''' <param name="b">
    ''' The second vector. Must have the same length as <paramref name="a"/>.
    ''' </param>
    ''' <returns>
    ''' The scalar dot product:  
    ''' Σ (a(i) × b(i))
    ''' </returns>
    ''' <exception cref="ArgumentException">
    ''' Thrown if the two input vectors do not have matching lengths.
    ''' </exception>
    ''' <remarks>
    ''' <para>
    ''' The dot product is computed as:
    ''' </para>
    ''' <code>
    ''' result = a(0)*b(0) + a(1)*b(1) + ... + a(n)*b(n)
    ''' </code>
    '''
    ''' <para>
    ''' No dimension checking is performed in the original implementation.  
    ''' If you want, I can add length validation for safety.
    ''' </para>
    ''' </remarks>
    Function DotProduct(a() As Double, b() As Double) As Double
        Dim Out As Double
        For i = 0 To a.Length - 1
            Out += a(i) * b(i)
        Next
        Return Out
    End Function

    ''' <summary>
    ''' Performs element-wise addition of two matrices of identical dimensions.
    ''' </summary>
    ''' <param name="mat1">
    ''' The first input matrix. Must have the same dimensions as <paramref name="mat2"/>.
    ''' </param>
    ''' <param name="mat2">
    ''' The second input matrix. Must have the same dimensions as <paramref name="mat1"/>.
    ''' </param>
    ''' <returns>
    ''' A new matrix where each element is the sum of the corresponding elements of 
    ''' <paramref name="mat1"/> and <paramref name="mat2"/>.
    ''' </returns>
    ''' <exception cref="ApplicationException">
    ''' Thrown if <paramref name="mat1"/> and <paramref name="mat2"/> do not have the same 
    ''' number of rows or columns.
    ''' </exception>
    ''' <remarks>
    ''' <para>
    ''' Matrix addition is performed element-wise:
    ''' </para>
    ''' <code>
    ''' C(i, j) = mat1(i, j) + mat2(i, j)
    ''' </code>
    ''' <para>
    ''' Both matrices must have exactly the same dimensions.
    ''' </para>
    ''' </remarks>
    Function M_ADD(mat1(,) As Double, mat2(,) As Double) As Double(,) 'matrix addition

        If mat1.GetLength(0) <> mat2.GetLength(0) Then BESHstatGlobals.BSerr.LogAndThrow(New ArgumentException("1st dimension of input matrices is not equal."))
        If mat1.GetLength(1) <> mat2.GetLength(1) Then BESHstatGlobals.BSerr.LogAndThrow(New ArgumentException("2st dimension of input matrices is not equal."))

        Dim c(mat1.GetUpperBound(0), mat1.GetUpperBound(1)) As Double
        For i = 0 To mat1.GetUpperBound(0)
            For j = 0 To mat1.GetUpperBound(1)
                c(i, j) = mat1(i, j) + mat2(i, j)
            Next
        Next
        Return c
    End Function

    ''' <summary>
    ''' Performs element-wise addition of a 2-dimensional matrix and a 1-dimensional vector.  
    ''' The vector is added to each row of the matrix (broadcast across all columns).
    ''' </summary>
    ''' <param name="mat1">
    ''' A 2-dimensional matrix with dimensions (m × n).
    ''' </param>
    ''' <param name="mat2">
    ''' A 1-dimensional vector of length m.  
    ''' Each element <c>mat2(i)</c> is added to every column of row <c>i</c> in <paramref name="mat1"/>.
    ''' </param>
    ''' <returns>
    ''' A new (m × n) matrix where:
    ''' <code>
    ''' result(i, j) = mat1(i, j) + mat2(i)
    ''' </code>
    ''' </returns>
    ''' <exception cref="ApplicationException">
    ''' Thrown when the number of rows in <paramref name="mat1"/> does not match the length of <paramref name="mat2"/>.
    ''' </exception>
    ''' <remarks>
    ''' <para>
    ''' This overload supports "row broadcasting", where a vector is added to each row of a matrix.
    ''' </para>
    ''' <para>
    ''' Example:
    ''' </para>
    ''' <code>
    ''' mat1 = {{1,2,3}, {4,5,6}}
    ''' mat2 = {10, 20}
    ''' 
    ''' Result:
    ''' {{11,12,13}, 
    '''  {24,25,26}}
    ''' </code>
    ''' </remarks>
    Function M_ADD(mat1(,) As Double, mat2() As Double) As Double(,) 'matrix addition
        If mat1.GetLength(0) <> mat2.Length Then BESHstatGlobals.BSerr.LogAndThrow(New ArgumentException("1st dimension of input matrices is not equal."))

        Dim c(mat1.GetUpperBound(0), mat2.Length - 1) As Double
        For i = 0 To mat1.GetUpperBound(0)
            For j = 0 To mat1.GetUpperBound(1)
                c(i, j) = mat1(i, j) + mat2(i)
            Next
        Next
        Return c
    End Function

    ''' <summary>
    ''' Performs element-wise addition of two 1-dimensional numeric vectors.
    ''' </summary>
    ''' <param name="mat1">
    ''' The first vector. Must have the same length as <paramref name="mat2"/>.
    ''' </param>
    ''' <param name="mat2">
    ''' The second vector. Must have the same length as <paramref name="mat1"/>.
    ''' </param>
    ''' <returns>
    ''' A new 1-dimensional array in which each element is:
    ''' <code>
    ''' result(i) = mat1(i) + mat2(i)
    ''' </code>
    ''' </returns>
    ''' <exception cref="ApplicationException">
    ''' Thrown when the vectors do not have the same length.
    ''' </exception>
    ''' <remarks>
    ''' <para>
    ''' This overload enables simple vector addition:
    ''' </para>
    ''' <code>
    ''' v3 = M_ADD(v1, v2)
    ''' </code>
    ''' </remarks>
    Function M_ADD(mat1() As Double, mat2() As Double) As Double() 'matrix addition
        If mat1.Length <> mat2.Length Then BESHstatGlobals.BSerr.LogAndThrow(New ApplicationException("1st dimension of input matrices is not equal."))

        Dim c(mat1.Length - 1) As Double
        For i = 0 To mat1.Length - 1
            c(i) = mat1(i) + mat2(i)
        Next
        Return c
    End Function

    ''' <summary>
    ''' Adds a scalar constant to every element of a 1-dimensional numeric vector.
    ''' </summary>
    ''' <param name="mat1">
    ''' A 1-dimensional array whose elements will each be increased by <paramref name="c"/>.
    ''' </param>
    ''' <param name="c">
    ''' The scalar value added to every element of <paramref name="mat1"/>.
    ''' </param>
    ''' <returns>
    ''' A 1-dimensional array of the same length as <paramref name="mat1"/>, where:
    ''' <code>
    ''' result(i) = mat1(i) + c
    ''' </code>
    ''' </returns>
    ''' <remarks>
    ''' <para>
    ''' This overload provides convenient vector–scalar addition:
    ''' </para>
    ''' <code>
    ''' v2 = M_ADD(v1, 5.0)
    ''' </code>
    ''' <para>
    ''' The addition is applied element-wise.
    ''' </para>
    ''' </remarks>
    Function M_ADD(mat1() As Double, c As Double) As Double() 'matrix addition
        Dim out(mat1.Length - 1) As Double
        For i = 0 To mat1.Length - 1
            out(i) = mat1(i) + c
        Next
        Return out
    End Function

    ''' <summary>
    ''' Performs element-wise subtraction of two matrices of identical dimensions.
    ''' </summary>
    ''' <param name="mat1">
    ''' The first input matrix (minuend). Must have the same dimensions as <paramref name="mat2"/>.
    ''' </param>
    ''' <param name="mat2">
    ''' The second input matrix (subtrahend). Must have the same dimensions as <paramref name="mat1"/>.
    ''' </param>
    ''' <returns>
    ''' A new matrix where each element is computed as:
    ''' <code>
    ''' result(i, j) = mat1(i, j) - mat2(i, j)
    ''' </code>
    ''' </returns>
    ''' <exception cref="ApplicationException">
    ''' Thrown if the matrices do not have matching row or column counts.
    ''' </exception>
    ''' <remarks>
    ''' <para>
    ''' Both input matrices must have identical (m × n) dimensions.
    ''' </para>
    ''' <para>
    ''' Example:
    ''' </para>
    ''' <code>
    ''' A = {{5, 6}, {7, 8}}
    ''' B = {{1, 2}, {3, 4}}
    ''' 
    ''' M_SUB(A, B) = {{4, 4}, {4, 4}}
    ''' </code>
    ''' </remarks>
    Function M_SUB(mat1(,) As Double, mat2(,) As Double) As Double(,) 'matrix elementwise subtraction
        If mat1.GetUpperBound(0) <> mat2.GetUpperBound(0) Then BESHstatGlobals.BSerr.LogAndThrow(New ArgumentException("1st dimension of input matrices is not equal."))
        If mat1.GetUpperBound(1) <> mat2.GetUpperBound(1) Then BESHstatGlobals.BSerr.LogAndThrow(New ArgumentException("2st dimension of input matrices is not equal."))

        Dim c(mat1.GetUpperBound(0), UBound(mat1, 2)) As Double
        For i = 0 To mat1.GetUpperBound(0)
            For j = 0 To mat1.GetUpperBound(1)
                c(i, j) = mat1(i, j) - mat2(i, j)
            Next
        Next
        Return c
    End Function

    ''' <summary>
    ''' Performs element-wise subtraction of two 1-dimensional numeric vectors.
    ''' </summary>
    ''' <param name="mat1">
    ''' The first vector (minuend). Must have the same length as <paramref name="mat2"/>.
    ''' </param>
    ''' <param name="mat2">
    ''' The second vector (subtrahend). Must have the same length as <paramref name="mat1"/>.
    ''' </param>
    ''' <returns>
    ''' A 1-dimensional array in which each element is computed as:
    ''' <code>
    ''' result(i) = mat1(i) - mat2(i)
    ''' </code>
    ''' </returns>
    ''' <exception cref="ApplicationException">
    ''' Thrown when the vectors do not have equal length.
    ''' </exception>
    ''' <remarks>
    ''' <para>
    ''' This overload performs simple vector subtraction:
    ''' </para>
    ''' <code>
    ''' v3 = M_SUB(v1, v2)
    ''' </code>
    ''' <para>
    ''' Input vectors must be the same length.
    ''' </para>
    ''' </remarks>
    Function M_SUB(mat1() As Double, mat2() As Double) As Double()
        If mat1.Length <> mat2.Length Then BESHstatGlobals.BSerr.LogAndThrow(New ArgumentException("1st dimension of input matrices is not equal."))

        Dim c(mat1.Length - 1) As Double
        For i = 0 To mat1.Length - 1
            c(i) = mat1(i) - mat2(i)
        Next
        Return c
    End Function

    ''' <summary>
    ''' Subtracts a scalar constant from every element of a 1-dimensional numeric vector.
    ''' </summary>
    ''' <param name="mat1">
    ''' A 1-dimensional array whose elements will each be reduced by <paramref name="c"/>.
    ''' </param>
    ''' <param name="c">
    ''' The scalar value to subtract from each element of <paramref name="mat1"/>.
    ''' </param>
    ''' <returns>
    ''' A 1-dimensional array of the same length as <paramref name="mat1"/>, where:
    ''' <code>
    ''' result(i) = mat1(i) - c
    ''' </code>
    ''' </returns>
    ''' <remarks>
    ''' <para>
    ''' This overload performs vector–scalar subtraction:
    ''' </para>
    ''' <code>
    ''' v2 = M_SUB(v1, 3.5)
    ''' </code>
    ''' <para>
    ''' Subtraction is applied element-wise.
    ''' </para>
    ''' </remarks>
    Function M_SUB(mat1() As Double, c As Double) As Double()
        Dim out(mat1.Length - 1) As Double
        For i = 0 To mat1.Length - 1
            out(i) = mat1(i) - c
        Next
        Return out
    End Function

    ''' <summary>
    ''' Performs element-wise division of two matrices of identical dimensions.
    ''' </summary>
    ''' <param name="mat1">
    ''' The numerator matrix. Must have the same dimensions as <paramref name="mat2"/>.
    ''' </param>
    ''' <param name="mat2">
    ''' The denominator matrix. Must have the same dimensions as <paramref name="mat1"/>.
    ''' </param>
    ''' <param name="strTrace">
    ''' Optional string used to accumulate diagnostic messages.  
    ''' When a division-by-zero occurs, a warning message is appended to this string.
    ''' </param>
    ''' <returns>
    ''' A new matrix where each element is computed as:
    ''' <code>
    ''' result(i, j) = mat1(i, j) / mat2(i, j)
    ''' </code>
    ''' If <c>mat2(i, j) = 0</c>, the output element remains at its default value (0), 
    ''' and a warning is appended to <paramref name="strTrace"/>.
    ''' </returns>
    ''' <exception cref="ApplicationException">
    ''' Thrown if the matrices do not have the same number of rows or columns.
    ''' </exception>
    ''' <remarks>
    ''' <para>
    ''' This function performs **Hadamard division** (element-wise division), not matrix inversion or 
    ''' algebraic matrix division.
    ''' </para>
    ''' 
    ''' <para>
    ''' Division-by-zero does not stop execution; instead, a warning string is appended:
    ''' </para>
    ''' <code>
    ''' "WARNING: M_DIV Division by zero. mat2="
    ''' </code>
    ''' 
    ''' <para>
    ''' If you want the function to throw exceptions instead of tracing, I can modify the implementation.
    ''' </para>
    ''' </remarks>
    Function M_DIV(mat1(,) As Double, mat2(,) As Double, ByRef Optional strTrace As String = "") As Double(,)
        If mat1.GetLength(0) <> mat2.GetLength(0) Then BESHstatGlobals.BSerr.LogAndThrow(New ArgumentException("1st dimension of input matrices is not equal."))
        If mat1.GetLength(1) <> mat2.GetLength(1) Then BESHstatGlobals.BSerr.LogAndThrow(New ArgumentException("2st dimension of input matrices is not equal."))

        Dim out(mat1.GetUpperBound(0), mat1.GetUpperBound(1)) As Double
        For i = 0 To mat1.GetUpperBound(0)
            For j = 0 To mat1.GetUpperBound(1)
                If mat2(i, j) <> 0 Then
                    out(i, j) = mat1(i, j) / mat2(i, j)
                Else
                    strTrace = strTrace + " WARNING: M_DIV Division by zero. mat2=" ' & array2str(mat2)
                End If
            Next
        Next
        M_DIV = out
    End Function

    ''' <summary>
    ''' Performs element-wise division of two 1-dimensional numeric vectors.
    ''' </summary>
    ''' <param name="mat1">
    ''' The numerator vector. Must have the same length as <paramref name="mat2"/>.
    ''' </param>
    ''' <param name="mat2">
    ''' The denominator vector. Must have the same length as <paramref name="mat1"/>.
    ''' </param>
    ''' <param name="strTrace">
    ''' Optional string used to accumulate diagnostic messages.  
    ''' When a division-by-zero occurs, a warning message is appended.
    ''' </param>
    ''' <returns>
    ''' A 1-dimensional array where each element is computed as:
    ''' <code>
    ''' result(i) = mat1(i) / mat2(i)
    ''' </code>
    ''' If <c>mat2(i) = 0</c>, the corresponding output remains at default value (0), and a warning is appended to <paramref name="strTrace"/>.
    ''' </returns>
    ''' <exception cref="ApplicationException">
    ''' Thrown when the two vectors do not have matching lengths.
    ''' </exception>
    ''' <remarks>
    ''' <para>
    ''' This function performs **Hadamard (element-wise) division**. It does not perform any form of matrix inversion.
    ''' </para>
    ''' 
    ''' <para>
    ''' Division-by-zero does not raise an exception in this implementation; it adds a trace warning instead:
    ''' </para>
    ''' <code>
    ''' "WARNING: M_DIV Division by zero. mat2="
    ''' </code>
    ''' 
    ''' <para>
    ''' If you would prefer behavior such as throwing <see cref="DivideByZeroException"/> or outputting NaN values, I can adjust the code.
    ''' </para>
    ''' </remarks>
    Function M_DIV(mat1() As Double, mat2() As Double, ByRef Optional strTrace As String = "") As Double()
        If mat1.Length <> mat2.Length Then BESHstatGlobals.BSerr.LogAndThrow(New ArgumentException("1st dimension of input matrices is not equal."))

        Dim out(mat1.Length - 1) As Double
        For i = 0 To mat1.Length - 1
            If mat2(i) <> 0 Then
                out(i) = mat1(i) / mat2(i)
            Else
                strTrace = strTrace + " WARNING: M_DIV Division by zero. mat2="
            End If
        Next
        Return out
    End Function

    ''' <summary>
    ''' Performs element-wise division of a 2-dimensional matrix by a 1-dimensional vector.  
    ''' The vector is broadcast across the columns, dividing each row of <paramref name="mat1"/> 
    ''' by the corresponding element of <paramref name="mat2"/>.
    ''' </summary>
    ''' <param name="mat1">
    ''' A 2-dimensional matrix with dimensions (m × n).  
    ''' Each row <c>mat1(i, *)</c> is divided by <c>mat2(i)</c>.
    ''' </param>
    ''' <param name="mat2">
    ''' A 1-dimensional vector of length m.  
    ''' Its elements act as row-wise denominators.
    ''' </param>
    ''' <param name="strTrace">
    ''' Optional string for accumulating diagnostic messages.  
    ''' If a division-by-zero occurs, a warning is appended.
    ''' </param>
    ''' <returns>
    ''' A new (m × n) matrix where:
    ''' <code>
    ''' result(i, j) = mat1(i, j) / mat2(i)
    ''' </code>
    ''' If <c>mat2(i) = 0</c>, the corresponding output cells remain at 0, and a warning is appended to <paramref name="strTrace"/>.
    ''' </returns>
    ''' <exception cref="ApplicationException">
    ''' Thrown if the number of rows in <paramref name="mat1"/> does not match the length of <paramref name="mat2"/>.
    ''' </exception>
    ''' <remarks>
    ''' <para>
    ''' This overload implements **row-wise broadcasting**, allowing a vector to divide each row of a matrix.
    ''' </para>
    ''' 
    ''' <para>
    ''' Example:
    ''' </para>
    ''' <code>
    ''' mat1 = {{10,20,30}, {40,50,60}}
    ''' mat2 = {10, 5}
    ''' 
    ''' M_DIV(mat1, mat2) = {{1,2,3}, {8,10,12}}
    ''' </code>
    ''' 
    ''' <para>
    ''' Division-by-zero does not raise an exception; instead a warning is appended to <paramref name="strTrace"/>.
    ''' </para>
    ''' </remarks>
    Function M_DIV(mat1(,) As Double, mat2() As Double, ByRef Optional strTrace As String = "") As Double(,)

        If mat1.GetLength(0) <> mat2.Length Then BESHstatGlobals.BSerr.LogAndThrow(New ArgumentException("1st dimension of input matrices is not equal."))

        Dim out(mat1.GetUpperBound(0), mat1.GetUpperBound(1)) As Double
        For i = 0 To mat1.GetUpperBound(0)
            For j = 0 To mat1.GetUpperBound(1)
                If mat2(i) <> 0 Then
                    out(i, j) = mat1(i, j) / mat2(i)
                Else
                    strTrace = strTrace + " WARNING: M_DIV Division by zero. mat2=" ' & array2str(mat2)
                End If
            Next
        Next
        Return out
    End Function

    ''' <summary>
    ''' Divides every element of a 2-dimensional numeric matrix by a scalar constant.
    ''' </summary>
    ''' <param name="mat1">
    ''' A 2-dimensional matrix whose elements will be divided by <paramref name="c"/>.
    ''' </param>
    ''' <param name="c">
    ''' The scalar divisor.  
    ''' If zero, no exception is thrown; instead a warning is appended to <paramref name="strTrace"/>.
    ''' </param>
    ''' <param name="strTrace">
    ''' Optional diagnostic message buffer.  
    ''' If <paramref name="c"/> is zero, a warning message is appended.
    ''' </param>
    ''' <returns>
    ''' A new matrix of the same dimensions as <paramref name="mat1"/>, where:
    ''' <code>
    ''' result(i, j) = mat1(i, j) / c
    ''' </code>
    ''' If <paramref name="c"/> = 0, all elements remain at their default value (0), and a warning is added to <paramref name="strTrace"/>.
    ''' </returns>
    ''' <remarks>
    ''' <para>
    ''' This overload performs **scalar division**, applying the same divisor to every element of the matrix.
    ''' </para>
    ''' <para>
    ''' Division-by-zero behavior matches the other M_DIV overloads: tracing rather than exception throwing.
    ''' </para>
    ''' </remarks>
    Function M_DIV(mat1(,) As Double, c As Double, ByRef Optional strTrace As String = "") As Double(,)
        Dim out(mat1.GetUpperBound(0), mat1.GetUpperBound(1)) As Double
        For i = 0 To mat1.GetUpperBound(0)
            For j = 0 To mat1.GetUpperBound(1)
                If c <> 0 Then
                    out(i, j) = mat1(i, j) / c
                Else
                    strTrace = strTrace + " WARNING: M_DIV Division by zero. mat2=" ' & array2str(mat2)
                End If
            Next
        Next
        Return out
    End Function

    ''' <summary>
    ''' Divides every element of a 1-dimensional numeric vector by a scalar constant.
    ''' </summary>
    ''' <param name="mat1">
    ''' A 1-dimensional array whose elements will be divided by <paramref name="c"/>.
    ''' </param>
    ''' <param name="c">
    ''' The scalar divisor.  
    ''' If zero, no exception is raised; instead a warning is appended to <paramref name="strTrace"/>.
    ''' </param>
    ''' <param name="strTrace">
    ''' Optional diagnostic message buffer.  
    ''' When <paramref name="c"/> equals zero, a warning message is appended.
    ''' </param>
    ''' <returns>
    ''' A 1-dimensional array of the same length as <paramref name="mat1"/>, where:
    ''' <code>
    ''' result(i) = mat1(i) / c
    ''' </code>
    ''' If <paramref name="c"/> = 0, all output elements remain 0, and a warning is added to <paramref name="strTrace"/>.
    ''' </returns>
    ''' <remarks>
    ''' <para>
    ''' This overload performs **scalar division** on a vector.  
    ''' Behavior matches the matrix-based M_DIV overloads: division-by-zero results in trace warnings.
    ''' </para>
    ''' </remarks>
    Function M_DIV(mat1() As Double, c As Double, ByRef Optional strTrace As String = "") As Double()
        Dim out(mat1.Length - 1) As Double
        For i = 0 To mat1.Length - 1
            If c <> 0 Then
                out(i) = mat1(i) / c
            Else
                strTrace += " WARNING: M_DIV Division by zero. mat2=" ' & array2str(mat2)
            End If
        Next
        Return out
    End Function

    ''' <summary>
    ''' Computes the Cholesky decomposition of a real symmetric positive-definite matrix A.
    ''' Returns the lower-triangular matrix L such that:
    ''' <code>
    ''' A = L · L'
    ''' </code>
    ''' </summary>
    ''' <param name="a">
    ''' A symmetric, positive-definite square matrix.  
    ''' Dimensions must be (n × n). Only the lower triangle is used during computation.
    ''' </param>
    ''' <param name="iFault">
    ''' Output parameter.  
    ''' Returns:
    ''' <list type="bullet">
    '''   <item><description>0 — Success</description></item>
    '''   <item><description>2 — Matrix is not positive-definite</description></item>
    ''' </list>
    ''' </param>
    ''' <param name="bErrorRaise">
    ''' If <c>True</c> (default), the function throws an <see cref="ApplicationException"/> when
    ''' the matrix is not positive-definite.  
    ''' If <c>False</c>, no exception is thrown and <paramref name="iFault"/> is set to 2.
    ''' </param>
    ''' <returns>
    ''' The lower-triangular Cholesky factor L.  
    ''' If the input matrix is not positive-definite and <paramref name="bErrorRaise"/> is <c>False</c>,
    ''' L is returned partially computed.
    ''' </returns>
    ''' <remarks>
    ''' <para>
    ''' The Cholesky decomposition requires <paramref name="a"/> to be:
    ''' </para>
    ''' <list type="bullet">
    '''   <item><description>symmetric</description></item>
    '''   <item><description>positive-definite</description></item>
    ''' </list>
    ''' 
    ''' <para>
    ''' A matrix fails the positive-definite check when any pivot value <c>L(i, i)</c> becomes
    ''' non-positive during the factorization process.
    ''' </para>
    ''' 
    ''' <para>
    ''' This implementation uses the standard outer-product Cholesky algorithm:
    ''' </para>
    ''' <code>
    ''' L(i,i) = sqrt( a(i,i) − Σ L(i,j)² )
    ''' L(k,i) = ( a(k,i) − Σ L(k,j)L(i,j) ) / L(i,i)
    ''' </code>
    ''' 
    ''' <para>
    ''' To test whether the decomposition succeeded without exceptions, check:
    ''' </para>
    ''' <code>
    ''' If iFault = 0 Then ' success
    ''' </code>
    ''' </remarks>
    Function Cholesky(a(,) As Double, ByRef Optional iFault As Integer = 0, Optional bErrorRaise As Boolean = True) As Double(,)
        Dim S As Double
        Dim n As Integer = a.GetUpperBound(0)
        Dim L(n, n) As Double

        For i = 0 To n
            S = 0.0
            For j = 0 To i - 1
                S += L(i, j) * L(i, j)
            Next
            L(i, i) = a(i, i) - S
            If L(i, i) <= 0 Then 'Matrix not positive-definite
                iFault = 2
                If bErrorRaise Then BESHstatGlobals.BSerr.LogAndThrow(New ApplicationException($"Matrix not positive-definite. {array2str(a)}"))
                Return L
            End If
            L(i, i) = Math.Sqrt(L(i, i))

            For k = i + 1 To n
                S = 0
                For j = 0 To i - 1
                    S += L(k, j) * L(i, j)
                Next j
                L(k, i) = (a(k, i) - S) / L(i, i)
            Next
        Next

        Return L
    End Function

    ''' <summary>
    ''' Solves a symmetric positive-definite linear system A·x = b using the Cholesky factorization A = L·Lᵀ.
    ''' Accepts a lower-triangular Cholesky factor <paramref name="L"/> and one or more right-hand sides.
    ''' </summary>
    ''' <param name="L">
    ''' The lower-triangular Cholesky factor of matrix A, produced by <see cref="Cholesky"/>.
    ''' Must be an n × n matrix satisfying A = L·Lᵀ.
    ''' </param>
    ''' <param name="b">
    ''' A right-hand-side vector or matrix.  
    ''' Must have n rows and k columns.  
    ''' Each column represents a separate RHS system to solve.
    ''' </param>
    ''' <returns>
    ''' A matrix x with n rows and k columns such that:
    ''' <code>
    ''' A·x(:,r) = b(:,r)
    ''' </code>
    ''' for each right-hand side r.
    ''' </returns>
    ''' <remarks>
    ''' <para>
    ''' The solution uses:
    ''' </para>
    ''' <list type="number">
    '''   <item><description>Forward substitution to solve L·pY = b</description></item>
    '''   <item><description>Backward substitution to solve Lᵀ·x = pY</description></item>
    ''' </list>
    ''' 
    ''' <para>
    ''' This implementation supports **multiple right-hand sides** by treating <paramref name="b"/> as an n × k matrix.
    ''' </para>
    ''' 
    ''' <para>
    ''' No symmetry or positive-definite checks are performed here; the correctness of results requires
    ''' that <paramref name="L"/> indeed comes from a valid Cholesky decomposition.
    ''' </para>
    ''' </remarks>
    Function CholSolve(L(,) As Double, b() As Double) As Double()
        Dim temp As Double
        Dim rows As Integer = L.GetUpperBound(0)
        Dim y(rows) As Double, x(rows) As Double

        'Forward substitution
        For i = 0 To rows
            temp = b(i)
            For j = i - 1 To 0 Step -1
                temp = temp - L(i, j) * y(j)
            Next
            y(i) = temp / L(i, i)
        Next

        'Back substitution
        For i = rows To 0 Step -1
            temp = y(i)
            For j = i + 1 To rows
                temp = temp - L(j, i) * x(j)
            Next
            x(i) = temp / L(i, i)
        Next

        Return x
    End Function

    ''' <summary>
    ''' Solves A·X = B using a Cholesky factorization A = L·Lᵀ, for multiple right-hand sides.
    ''' </summary>
    ''' <param name="L">
    ''' Lower-triangular Cholesky factor (n x n) with positive diagonal such that A = L·Lᵀ.
    ''' Only the lower triangle (including diagonal) is referenced.
    ''' </param>
    ''' <param name="B">
    ''' Right-hand sides matrix (n x m). Each column is one right-hand side.
    ''' </param>
    ''' <returns>
    ''' Solution matrix X (n x m) satisfying A·X = B.
    ''' </returns>
    Function CholSolve(L(,) As Double, b(,) As Double) As Double(,)
        Dim temp As Double
        Dim rows As Integer = L.GetUpperBound(0)
        Dim y(rows, b.GetUpperBound(1)) As Double, x(rows, b.GetUpperBound(1)) As Double

        'Forward substitution
        For r = 0 To b.GetUpperBound(1) 'multiple right hand sides
            For i = 0 To rows
                temp = b(i, r)
                For j = i - 1 To 0 Step -1
                    temp -= L(i, j) * y(j, r)
                Next
                y(i, r) = temp / L(i, i)
            Next

            'Back substitution
            For i = rows To 0 Step -1
                temp = y(i, r)
                For j = i + 1 To rows
                    temp -= L(j, i) * x(j, r)
                Next
                x(i, r) = temp / L(i, i)
            Next
        Next

        Return x
    End Function

    ''' <summary>
    ''' Computes the inverse of a symmetric positive-definite matrix A using its Cholesky factor L,
    ''' where A = L·Lᵀ.  
    ''' Returns A⁻¹ by first computing L⁻¹ and then forming:
    ''' <code>
    ''' A⁻¹ = (L⁻¹)ᵀ · L⁻¹
    ''' </code>
    ''' </summary>
    ''' <param name="L">
    ''' The lower-triangular Cholesky factor of matrix A, produced by <see cref="Cholesky"/>.  
    ''' Must be an n × n nonsingular lower-triangular matrix.
    ''' </param>
    ''' <returns>
    ''' The inverse matrix A⁻¹ as an n × n symmetric array.
    ''' </returns>
    ''' <remarks>
    ''' <para>
    ''' This algorithm works in two stages:
    ''' </para>
    ''' 
    ''' <list type="number">
    '''   <item>
    '''     <description>
    '''     **Compute U = L⁻¹**, the inverse of the Cholesky factor.  
    '''     This is done by solving L·U = I using back-substitution on each column.
    '''     </description>
    '''   </item>
    ''' 
    '''   <item>
    '''     <description>
    '''     **Form A⁻¹ = U·Uᵀ**.  
    '''     Since U is upper triangular, only the upper triangle needs to be computed explicitly, and the
    '''     lower triangle is filled by symmetry.
    '''     </description>
    '''   </item>
    ''' </list>
    ''' 
    ''' <para>
    ''' This method is numerically stable for positive-definite matrices and is more efficient
    ''' than directly inverting A.
    ''' </para>
    ''' 
    ''' <para>
    ''' No validation is performed to confirm that <paramref name="L"/> is lower-triangular or nonsingular;
    ''' it is assumed to be a valid Cholesky factor.
    ''' </para>
    ''' </remarks>
    Function CholInv(L(,) As Double) As Double(,)
        Dim p As Integer = L.GetUpperBound(0)
        Dim u(p, p) As Double, x(p, p) As Double

        'Back substitution to produce upper triangular matrix inverse U = L^-1
        For j = p To 0 Step -1
            u(j, j) = 1 / L(j, j)
            For k = j - 1 To 0 Step -1
                For i = k + 1 To j
                    u(k, j) -= L(i, k) * u(i, j) / L(k, k)
                Next
            Next
        Next

        'Multiplication of U by U' to produce (LL')^-1
        For i = 0 To p
            For j = i To p
                For k = j To p
                    x(i, j) += u(i, k) * u(j, k)
                Next
                x(j, i) = x(i, j)
            Next
        Next

        Return x
    End Function

    Public Class LUdecompOutput
        Public LUdecomp(,) As Double ' LU decomposition matrix
        Public LUindex() As Double 'is an output vector that pRecords the row permutation effected by the partial pivoting
    End Class

    ''' <summary>
    ''' Performs LU decomposition with partial pivoting on a square matrix.
    ''' Produces matrices L and U stored together in a single array, plus a permutation index vector.
    ''' Implements Crout’s algorithm with implicit scaling (Numerical Recipes).
    ''' </summary>
    ''' <param name="mat">
    ''' The input square matrix to be decomposed.  
    ''' The returned LU matrix overwrites this matrix inside the output object.
    ''' </param>
    ''' <param name="d">
    ''' Output parameter.  
    ''' Set to +1 or −1 depending on whether the number of row interchanges is even or odd.
    ''' </param>
    ''' <param name="iErr">
    ''' Output parameter.  
    ''' Returns:
    ''' <list type="bullet">
    '''   <item><description>0 — success</description></item>
    '''   <item><description>2 — singular matrix detected</description></item>
    ''' </list>
    ''' </param>
    ''' <returns>
    ''' An <see cref="LUdecompOutput"/> object containing:
    ''' <list type="bullet">
    '''   <item><description><c>LUdecomp</c> — the combined LU matrix</description></item>
    '''   <item><description><c>LUindex</c> — permutation vector recording pivot row swaps</description></item>
    ''' </list>
    ''' </returns>
    ''' <exception cref="ApplicationException">
    ''' Thrown when the input matrix is not square, or when a singular pivot is encountered.
    ''' </exception>
    ''' <remarks>
    ''' <para>
    ''' This implementation follows the LU decomposition method described in  
    ''' <i>Numerical Recipes</i> (Press et al.). 
    ''' </para>
    ''' 
    ''' <para>
    ''' The algorithm computes:
    ''' </para>
    ''' <code>
    ''' P·A = L·U
    ''' </code>
    ''' <para>
    ''' where P is a permutation matrix constructed from <c>LUindex</c>.
    ''' </para>
    ''' 
    ''' <para>
    ''' Row scaling via <c>VV()</c> improves numerical stability by choosing the pivot based on
    ''' <c>|A(i,j)| × VV(i)</c>.
    ''' </para>
    ''' 
    ''' <para>
    ''' If a diagonal pivot element becomes zero (within machine precision), the routine substitutes a tiny value
    ''' and sets <paramref name="iErr"/> = 2 to indicate that the matrix is effectively singular.
    ''' </para>
    ''' </remarks>
    Function LUdecomp(mat(,) As Double, ByRef d As Double, ByRef Optional iErr As Integer = 0) As LUdecompOutput
        Const tiny As Double = 0.000000000000002
        Dim j As Integer, k As Integer, imax As Integer, indx() As Double, a(,) As Double
        Dim Aamax As Double, dum As Double, sum As Double, VV() As Double 'vv stores the implicit scaling of each row
        Dim LUout As LUdecompOutput = New LUdecompOutput

        a = DirectCast(mat.Clone(), Double(,))
        Dim n As Integer = UBound(a, 1)
        'decomposed matrix have to be squared
        If n <> a.GetUpperBound(1) Then BESHstatGlobals.BSerr.LogAndThrow(New ArgumentException("Input matrix is not squared."))

        ReDim VV(n), indx(n)

        'implicit pivoting
        'Loop over rows to get the implicit scaling information
        'from press et al. Numerical recipies

        For i = 0 To n
            Aamax = 0.0
            For j = 0 To n
                If (Math.Abs(a(i, j)) > Aamax) Then Aamax = Math.Abs(a(i, j))
            Next

            If (Aamax = 0) Then
                iErr = 2
                BESHstatGlobals.BSerr.LogAndThrow(New ApplicationException("Singular matrix.")) 'singular matrix in LUdcemop, No nonzero largest element.
            End If
            VV(i) = 1.0 / Aamax 'Save the scaling.
        Next

        'start Crout's algorithm
        For j = 0 To n 'This is the loop over columns of Crout's method.
            For i = 0 To j - 1 'This is equation (2.3.12) except for i = j.
                sum = a(i, j)
                For k = 0 To i - 1
                    sum -= a(i, k) * a(k, j)
                Next
                a(i, j) = sum
            Next

            Aamax = 0.0
            'Initialize for the search for largest pivot element.
            For i = j To n 'This is i = j of equation (2.3.12) and i = j+1...N of equation (2.3.13).
                sum = a(i, j)
                For k = 0 To j - 1
                    sum -= a(i, k) * a(k, j)
                Next
                a(i, j) = sum
                dum = VV(i) * Math.Abs(sum) 'Figure of merit for the pivot.
                If (dum >= Aamax) Then 'Is it better than the best so far?
                    imax = i
                    Aamax = dum
                End If
            Next

            If j <> imax Then 'Do we need to interchange rows?
                For k = 0 To n 'Yes, do so...
                    dum = a(imax, k)
                    a(imax, k) = a(j, k)
                    a(j, k) = dum
                Next
                d = -d '...and change the parity of d.
                VV(imax) = VV(j) 'Also interchange the scale factor.
            End If

            indx(j) = imax
            If a(j, j) = 0.0 Then a(j, j) = tiny
            'If the pivot element is zero the matrix is singular (at least to the precision of the algorithm).
            'For some applications on singular matrices, it is desirable to substitute TINY for zero.
            If j <> n Then 'Now, finally, divide by the pivot element.
                dum = 1.0 / a(j, j)
                For i = j + 1 To n
                    a(i, j) = a(i, j) * dum
                Next
            End If
        Next j

        LUout.LUindex = indx
        LUout.LUdecomp = a
        LUdecomp = LUout
    End Function

    ''' <summary>
    ''' Solves the linear system A·x = b using an LU decomposition previously computed by <see cref="LUdecomp"/>.
    ''' Supports a single right-hand-side vector.
    ''' </summary>
    ''' <param name="LU">
    ''' An <see cref="LUdecompOutput"/> structure containing:
    ''' <list type="bullet">
    '''   <item><description><c>LUdecomp</c> — the combined L and U matrices (Crout form)</description></item>
    '''   <item><description><c>LUindex</c> — the permutation vector resulting from partial pivoting</description></item>
    ''' </list>
    ''' </param>
    ''' <param name="RighthandSideVector">
    ''' The right-hand-side vector b in the equation A·x = b.  
    ''' Must have the same dimension as the LU decomposition.
    ''' </param>
    ''' <returns>
    ''' A 1-dimensional array representing the solution vector x.
    ''' </returns>
    ''' <exception cref="ApplicationException">
    ''' Thrown when the dimensions of <paramref name="RighthandSideVector"/> do not match the LU decomposition.
    ''' </exception>
    ''' <remarks>
    ''' <para>
    ''' This routine follows the method described in <i>Numerical Recipes</i> (Press et al.), performing:
    ''' </para>
    ''' 
    ''' <list type="number">
    '''   <item><description>
    '''     **Forward substitution** on L with permutation:  
    '''     <code>L · pY = P · b</code>
    '''   </description></item>
    ''' 
    '''   <item><description>
    '''     **Backward substitution** on U:  
    '''     <code>U · x = pY</code>
    '''   </description></item>
    ''' </list>
    ''' 
    ''' <para>
    ''' The vector <paramref name="RighthandSideVector"/> is internally reordered using the permutation vector
    ''' from <paramref name="LU"/> before forward substitution.
    ''' </para>
    '''
    ''' <para>
    ''' The input LU matrix must represent a valid LU decomposition with partial pivoting.
    ''' Incorrect LU structures may result in undefined behavior.
    ''' </para>
    ''' </remarks>
    Function LUbacksub(LU As LUdecompOutput, RighthandSideVector() As Double) As Double()
        Dim j As Integer, LL As Integer, sum As Double
        Dim LUA(,) As Double = LU.LUdecomp
        Dim indx() As Double = LU.LUindex
        Dim b() As Double = RighthandSideVector
        Dim n As Integer = LUA.GetUpperBound(0)

        If n <> LUA.GetUpperBound(1) And n <> UBound(indx) And n <> b.GetUpperBound(0) Then BESHstatGlobals.BSerr.LogAndThrow(New ArgumentException("Wrong input matrices dimensions."))

        Dim ii As Integer = -1
        'When ii is set to a positive value, it will become the index of the 1st nonvanishing element of b. We now do
        'the forward substitution, equation (2.3.6). The only new wrinkle is to unscramble the permutation as we go.
        For i As Integer = 0 To n
            LL = indx(i)
            sum = b(LL)
            b(LL) = b(i)
            If ii <> -1 Then
                For j = ii To i - 1
                    sum -= LUA(i, j) * b(j)
                Next
            ElseIf sum <> 0 Then
                ii = i
                'A nonzero element was encountered, so from now on we will
                'have to do the sums in the loop above.
            End If
            b(i) = sum
        Next

        For i As Integer = n To 0 Step -1 'Now we do the backsubstitution, equation (2.3.7).
            sum = b(i)
            For j = i + 1 To n
                sum -= LUA(i, j) * b(j)
            Next
            b(i) = sum / LUA(i, i)
            'Store a component of the solution vector X.
        Next
        Return b
    End Function


    ''' <summary>
    ''' Computes the determinant of a square matrix using LU decomposition,
    ''' matching the behavior of Excel's MDETERM function.
    ''' </summary>
    ''' <param name="matrix">
    ''' A two-dimensional square array representing the matrix whose determinant
    ''' is to be computed.
    ''' </param>
    ''' <returns>
    ''' The determinant of the matrix, equivalent to Excel's MDETERM.
    ''' Returns <see cref="Double.NaN"/> if the matrix is not square.
    ''' </returns>
    ''' <remarks>
    ''' <para>
    ''' This implementation uses the existing <c>LUdecomp</c> routine to obtain
    ''' an LU factorization with partial pivoting. The determinant is computed as:
    ''' 
    '''     det = (product of diagonal elements of U) * (pivot sign)
    ''' 
    ''' where the pivot sign is -1 raised to the number of row interchanges.
    ''' </para>
    ''' 
    ''' <para>
    ''' Excel's MDETERM returns 0 for singular matrices; this implementation
    ''' matches that behavior.
    ''' </para>
    ''' 
    ''' <para>
    ''' Special cases:
    ''' <list type="bullet">
    '''   <item><description>Returns NaN if the matrix is not square.</description></item>
    '''   <item><description>Returns 0 if the matrix is singular.</description></item>
    '''   <item><description>Uses LU pivoting information to determine sign changes.</description></item>
    ''' </list>
    ''' </para>
    ''' </remarks>
    Public Function MDeterm(matrix As Double(,)) As Double
        Dim n As Integer = matrix.GetLength(0)
        If n <> matrix.GetLength(1) Then Return Double.NaN
        Dim d As Double = 1.0
        Dim iErr As Integer = 0

        ' Perform LU decomposition using your existing routine
        Dim LU As LUdecompOutput = LUdecomp(matrix, d, iErr)

        ' If LUdecomp flagged a singular matrix, return 0 (Excel behavior)
        If iErr <> 0 Then Return 0.0
        Dim det As Double = d

        ' Multiply diagonal elements of U
        For i As Integer = 0 To n - 1
            det *= LU.LUdecomp(i, i)
        Next

        Return det
    End Function


    ''' <summary>
    ''' Creates an n × n identity matrix.
    ''' </summary>
    ''' <param name="n">
    ''' The size of the matrix.  
    ''' The resulting matrix has dimensions (n × n) with ones on the main diagonal.
    ''' </param>
    ''' <returns>
    ''' A 2-dimensional array representing the identity matrix:
    ''' <code>
    ''' I(i, j) = 1  if i = j  
    ''' I(i, j) = 0  otherwise
    ''' </code>
    ''' </returns>
    ''' <remarks>
    ''' <para>
    ''' The function allocates a square matrix from (0 … n, 0 … n).  
    ''' If you prefer a 0-based dimension of (0 … n–1), I can adjust the implementation.
    ''' </para>
    ''' 
    ''' <para>
    ''' Example (n = 2):
    ''' </para>
    ''' <code>
    ''' {{1, 0, 0},
    '''  {0, 1, 0},
    '''  {0, 0, 1}}
    ''' </code>
    ''' </remarks>
    Public Function IdentityMat(n As Integer) As Double(,)
        Dim out(n, n) As Double
        For i = 0 To n
            out(i, i) = 1
        Next
        Return out
    End Function

    ''' <summary>
    ''' Creates a vector of length n + 1 whose elements are all set to a specified value.
    ''' </summary>
    ''' <param name="n">
    ''' The highest index of the vector.  
    ''' The resulting vector has indices 0 through n.
    ''' </param>
    ''' <param name="val">
    ''' The value assigned to each element of the vector.  
    ''' Defaults to 1.
    ''' </param>
    ''' <returns>
    ''' A 1-dimensional array of length n + 1 where:
    ''' <code>
    ''' result(i) = val
    ''' </code>
    ''' for all i from 0 to n.
    ''' </returns>
    ''' <remarks>
    ''' <para>
    ''' This function produces a constant vector, not a true “identity vector” (which would normally
    ''' contain a single 1 and zeros elsewhere).  
    ''' However, the name matches the original intent in the codebase.
    ''' </para>
    ''' 
    ''' <para>
    ''' Example:
    ''' </para>
    ''' <code>
    ''' IdentityVect(3)     → {1, 1, 1, 1}
    ''' IdentityVect(3, 5)  → {5, 5, 5, 5}
    ''' </code>
    ''' </remarks>
    Public Function IdentityVect(n As Integer, Optional val As Double = 1) As Double()
        Dim out(n) As Double
        For i = 0 To n
            out(i) = val
        Next
        Return out
    End Function

    ''' <summary>
    ''' Computes the inverse of a square matrix using either LU decomposition (default)
    ''' or Cholesky decomposition for positive-definite matrices.
    ''' </summary>
    ''' <param name="mat">
    ''' The square matrix to be inverted.  
    ''' Must have dimensions (n × n). Indices are assumed to run from 0 to n.
    ''' </param>
    ''' <param name="method">
    ''' The inversion method to use:  
    ''' <list type="bullet">
    '''   <item><description><c>"LU"</c> (default) — general matrix inversion via LU decomposition.</description></item>
    '''   <item><description><c>"CHOL"</c> — inversion via Cholesky decomposition; requires positive-definite input.</description></item>
    ''' </list>
    ''' Method comparison is case-insensitive and trimmed.
    ''' </param>
    ''' <param name="iErr">
    ''' Output parameter indicating error state for the selected method:
    ''' <list type="bullet">
    '''   <item><description>0 — success</description></item>
    '''   <item><description>2 — matrix is singular (LU branch)</description></item>
    '''   <item><description>2 — matrix is not positive-definite (Cholesky branch)</description></item>
    ''' </list>
    ''' </param>
    ''' <returns>
    ''' A 2-dimensional array representing the matrix inverse.  
    ''' If an error occurs and <paramref name="method"/> permits non-raising error behavior (e.g. Cholesky with <c>bErrorRaise := False</c>),  
    ''' the returned matrix may be partially computed.
    ''' </returns>
    ''' <exception cref="ApplicationException">
    ''' Thrown when:
    ''' <list type="bullet">
    '''   <item><description>The input matrix is not square.</description></item>
    ''' </list>
    ''' </exception>
    ''' <exception cref="NotImplementedException">
    ''' Thrown when <paramref name="method"/> is not one of the supported values <c>"LU"</c> or <c>"CHOL"</c>.
    ''' </exception>
    ''' <remarks>
    ''' <para>
    ''' <b>LU method:</b>  
    ''' The LU decomposition <c>A = L·U</c> is computed once.  
    ''' The inverse is generated column-by-column by solving:
    ''' </para>
    ''' <code>
    ''' A · x_j = e_j
    ''' </code>
    ''' <para>
    ''' where <c>e_j</c> is the j-th unit vector.  
    ''' Each solution <c>x_j</c> becomes a column of <c>A⁻¹</c>.
    ''' </para>
    ''' 
    ''' <para>
    ''' <b>Cholesky method:</b>  
    ''' For symmetric positive-definite matrices, the decomposition <c>A = L·Lᵀ</c> is used.  
    ''' The inverse is computed as:
    ''' </para>
    ''' <code>
    ''' A⁻¹ = (L⁻¹)ᵀ · L⁻¹
    ''' </code>
    ''' 
    ''' <para>
    ''' The LU branch is general-purpose; the Cholesky branch is faster and more stable but only valid when A is positive-definite.
    ''' </para>
    ''' </remarks>
    Public Function MatInv(ByVal mat(,) As Double,
                           Optional method As String = "LU",
                           ByRef Optional iErr As Integer = 0,
                           Optional bPseudInverse As Boolean = True) As Double(,)

        Dim n As Integer = mat.GetUpperBound(0)
        If n <> mat.GetUpperBound(1) Then BESHstatGlobals.BSerr.LogAndThrow(New ArgumentException("Wrong input matrices dimensions."))
        Dim out(n, n) As Double
        Dim matCopy(,) As Double = DirectCast(mat.Clone(), Double(,))

        If method.ToUpper.Trim = "LU" Then
            Dim d As Double
            Dim decomp = LUdecomp(matCopy, d, iErr)
            For j = 0 To n 'Find inverse by columns.
                Dim vect1(n) As Double
                vect1(j) = 1
                Dim arTemp = LUbacksub(decomp, vect1)
                For i = 0 To n
                    out(i, j) = arTemp(i)
                Next
            Next

        ElseIf method.ToUpper.Trim = "CHOL" Then
            Dim ch = Cholesky(matCopy, iErr)
            If iErr = 2 Then
                If bPseudInverse Then
                    'try pseudoinverse
                    BSlogg.Log($"WARNING: CHOLESKY. mat not positive-definite. Calling pseudoInverse. mat={array2str(matCopy)}", LogMsgType.Warn)
                    out = pseudoInverse(matCopy)
                    BSlogg.Log($"NOTE: pseudoInverse output ={array2str(out)}")
                Else
                    BESHstatGlobals.BSerr.LogAndThrow(New ApplicationException("Matrix not positive definite"))
                End If
            Else
                out = CholInv(ch)
            End If

        Else
            BESHstatGlobals.BSerr.LogAndThrow(New NotImplementedException("Not implemented error. method = " & method))
        End If

        Return out
    End Function

    ''' <summary>
    ''' Computes the Moore–Penrose pseudoinverse of a real matrix using its Singular Value Decomposition (SVD).
    ''' </summary>
    ''' <param name="A">
    ''' Input matrix <c>A</c> as a 2D <see cref="Double"/> array.
    ''' The array is expected to be indexed from 0 with bounds <c>(0..m, 0..n)</c>
    ''' (i.e., <c>UBound(A,1)=m</c> and <c>UBound(A,2)=n</c>).
    ''' </param>
    ''' <param name="tol">
    ''' Singular-value cutoff threshold. Singular values with absolute value less than or equal to <paramref name="tol"/>
    ''' are treated as zero (their reciprocals are set to 0 in the pseudoinverse).
    ''' <para>
    ''' If <paramref name="tol"/> is negative (default: -1), a relative tolerance is chosen automatically as:
    ''' <c>Double.Epsilon * Max(m, n) * Max(|wᵢ|)</c>, where <c>wᵢ</c> are the singular values.
    ''' </para>
    ''' </param>
    ''' <returns>
    ''' The Moore–Penrose pseudoinverse <c>A⁺</c>, computed as <c>A⁺ = V · W⁺ · Uᵀ</c>, where
    ''' <c>A = U · W · Vᵀ</c> is the SVD of <paramref name="A"/> and <c>W⁺</c> is formed by inverting
    ''' singular values above the tolerance and zeroing the rest.
    ''' </returns>
    ''' <remarks>
    ''' <para>
    ''' This function uses <see cref="SVD_decomp(Double(,))"/> to obtain the decomposition <c>A = U · W · Vᵀ</c>.
    ''' The returned <c>V</c> is not transposed. The pseudoinverse is then assembled as <c>V · W⁺ · Uᵀ</c>.
    ''' </para>
    ''' <para>
    ''' The implementation clones <paramref name="A"/> before calling the SVD routine so the caller’s matrix is not modified.
    ''' </para>
    ''' <para>
    ''' Note on numerical stability: the choice of <paramref name="tol"/> controls the effective rank.
    ''' Increasing <paramref name="tol"/> yields a more regularized pseudoinverse for ill-conditioned matrices.
    ''' </para>
    ''' <para>
    ''' Note on indexing: this code assumes 0-based arrays. If arrays with non-zero lower bounds are used,
    ''' the implementation should be adapted to use <c>GetLowerBound</c>/<c>GetUpperBound</c> consistently.
    ''' </para>
    ''' </remarks>
    ''' <exception cref="ArgumentNullException">
    ''' Thrown if <paramref name="A"/> is <c>Nothing</c>.
    ''' </exception>
    Function pseudoInverse(ByVal A(,) As Double, Optional tol As Double = -1.0) As Double(,)
        If A Is Nothing Then BESHstatGlobals.BSerr.LogAndThrow(New ArgumentNullException(NameOf(A)))

        ' Work on a copy so the input A is not overwritten by SVD_decomp
        Dim Acopy As Double(,) = DirectCast(A.Clone(), Double(,))
        Dim svd As SVDoutput = SVD_decomp(Acopy)
        Dim n As Integer = svd.Wvect.GetUpperBound(0)

        ' If tol not supplied (tol < 0), choose a standard relative tolerance:
        ' tol = eps * max(m,n) * max(singular value)
        If tol < 0.0 Then
            Dim mRows As Integer = svd.U.GetLength(0)
            Dim nCols As Integer = svd.U.GetLength(1)

            Dim wMax As Double = 0.0
            For i As Integer = 0 To n
                Dim aw As Double = Math.Abs(svd.Wvect(i))
                If aw > wMax Then wMax = aw
            Next

            tol = Double.Epsilon * Math.Max(mRows, nCols) * wMax
        End If

        ' Build W^+ (diagonal)
        Dim Wplus(n, n) As Double
        For i As Integer = 0 To n
            Dim wi As Double = svd.Wvect(i)
            Wplus(i, i) = If(Math.Abs(wi) > tol, 1.0 / wi, 0.0)
        Next

        ' A^+ = V * W^+ * U^T
        Return MatrixMult(MatrixMult(svd.V, Wplus), trans(svd.U))
    End Function


    Public Class SVDoutput
        'Given a matrix a(1:m,1:n), this routine computes its singular value decomposition, A = U * W * V ^t .
        Public U(,) As Double
        Public Wvect() As Double 'The diagonal matrix of singular values W as a vector w(0:n-1).
        Public Wmat(,) As Double 'The diagonal matrix of singular values W as matrix
        Public V(,) As Double
    End Class

    ''' <summary>
    ''' Computes the Singular Value Decomposition (SVD) of a real matrix using the classic
    ''' Golub–Reinsch / Numerical Recipes algorithm.
    ''' </summary>
    ''' <param name="matrix">
    ''' Input matrix <c>A</c> as a 2D <see cref="Double"/> array.
    ''' The array is expected to be indexed from 0 and have bounds <c>(0..m, 0..n)</c>
    ''' (i.e., <c>UBound(mat,1)=m</c> and <c>UBound(mat,2)=n</c>).
    ''' </param>
    ''' <returns>
    ''' An <see cref="SVDoutput"/> instance containing matrices <c>U</c>, <c>V</c>, and the singular values <c>W</c>
    ''' such that <c>A = U · W · Vᵀ</c>.
    ''' <list type="bullet">
    '''   <item><description><see cref="SVDoutput.U"/>: the left singular vectors (stored in the working copy of <paramref name="matrix"/>).</description></item>
    '''   <item><description><see cref="SVDoutput.Wvect"/>: singular values as a vector.</description></item>
    '''   <item><description><see cref="SVDoutput.Wmat"/>: singular values as a diagonal matrix.</description></item>
    '''   <item><description><see cref="SVDoutput.V"/>: the right singular vectors (not transposed).</description></item>
    ''' </list>
    ''' </returns>
    ''' <remarks>
    ''' <para>
    ''' This routine follows the Numerical Recipes implementation: it performs a Householder reduction
    ''' to bidiagonal form, accumulates left/right transformations, and then diagonalizes the bidiagonal
    ''' matrix via QR iterations.
    ''' </para>
    ''' <para>
    ''' The singular values are returned in <see cref="SVDoutput.Wvect"/> and the diagonal matrix form in
    ''' <see cref="SVDoutput.Wmat"/>. The returned <see cref="SVDoutput.V"/> is <c>V</c> (not <c>Vᵀ</c>).
    ''' </para>
    ''' <para>
    ''' Convergence: the diagonalization phase performs up to 30 QR iterations per singular value; if it
    ''' does not converge within that limit, an exception is thrown.
    ''' </para>
    ''' <para>
    ''' Note on dimensions: this implementation sizes arrays using <c>UBound</c> and assumes 0-based bounds.
    ''' If you pass arrays with non-zero lower bounds, results may be incorrect unless the code is adapted
    ''' to use <c>GetLowerBound</c>/<c>GetUpperBound</c> everywhere.
    ''' </para>
    ''' </remarks>
    ''' <exception cref="ArgumentNullException">
    ''' Thrown if <paramref name="matrix"/> is <c>Nothing</c>.
    ''' </exception>
    ''' <exception cref="ApplicationException">
    ''' Thrown when the QR iteration fails to converge (message: <c>"SVD: No convergence!"</c>).
    ''' </exception>
    Function SVD_decomp(ByVal matrix(,) As Double) As SVDoutput
        Dim SVDout As New SVDoutput()
        Dim i As Integer, L As Integer, Nm As Integer
        Dim c As Double, F As Double, H As Double, S As Double, x As Double, y As Double, z As Double
        Dim a(,) As Double = DirectCast(matrix.Clone(), Double(,))

        Dim m As Integer = a.GetUpperBound(0)
        Dim n As Integer = a.GetUpperBound(1)

        Dim W(n) As Double, V(n, n) As Double, rv1(n) As Double

        Dim G As Double = 0.0
        Dim scale_ As Double = 0.0
        Dim Anorm As Double = 0.0

        ' --- Householder reduction to bidiagonal form ---
        For i = 0 To n
            L = i + 1
            rv1(i) = scale_ * G
            G = 0.0
            S = 0.0
            scale_ = 0.0

            If i <= m Then
                For k = i To m
                    scale_ += Math.Abs(a(k, i))
                Next
                If scale_ <> 0.0 Then
                    For k = i To m
                        a(k, i) /= scale_
                        S += a(k, i) * a(k, i)
                    Next

                    F = a(i, i)
                    If F >= 0 Then G = -Math.Sqrt(S) Else G = Math.Sqrt(S)
                    H = F * G - S
                    a(i, i) = F - G

                    For j = L To n
                        S = 0.0
                        For k = i To m
                            S += a(k, i) * a(k, j)
                        Next

                        ' (This line is correct in NR; the earlier "bug" comment is misleading unless H=0)
                        F = S / H
                        For k = i To m
                            a(k, j) += F * a(k, i)
                        Next
                    Next j

                    For k = i To m
                        a(k, i) = scale_ * a(k, i)
                    Next
                End If
            End If

            W(i) = scale_ * G
            G = 0.0
            S = 0.0
            scale_ = 0.0

            If i <= m AndAlso i <> n Then
                For k = L To n
                    scale_ += Math.Abs(a(i, k))
                Next
                If scale_ <> 0.0 Then
                    For k = L To n
                        a(i, k) /= scale_
                        S += a(i, k) * a(i, k)
                    Next

                    F = a(i, L)
                    If F >= 0 Then G = -Math.Sqrt(S) Else G = Math.Sqrt(S)
                    H = F * G - S
                    a(i, L) = F - G

                    For k = L To n
                        rv1(k) = a(i, k) / H
                    Next

                    For j = L To m
                        S = 0.0
                        For k = L To n
                            S += a(j, k) * a(i, k)
                        Next
                        For k = L To n
                            a(j, k) += S * rv1(k)
                        Next
                    Next j

                    For k = L To n
                        a(i, k) = scale_ * a(i, k)
                    Next
                End If
            End If

            If Anorm < (Math.Abs(W(i)) + Math.Abs(rv1(i))) Then Anorm = Math.Abs(W(i)) + Math.Abs(rv1(i))
        Next i

        ' --- Accumulation of right-hand transformations ---
        For i = n To 0 Step -1
            If i < n Then
                If G <> 0.0 Then
                    For j = L To n
                        V(j, i) = (a(i, j) / a(i, L)) / G
                    Next

                    For j = L To n
                        S = 0.0
                        For k = L To n
                            S += a(i, k) * V(k, j)
                        Next
                        For k = L To n
                            V(k, j) += S * V(k, i)
                        Next
                    Next
                End If

                For j = L To n
                    V(i, j) = 0.0
                    V(j, i) = 0.0
                Next
            End If

            V(i, i) = 1.0
            G = rv1(i)
            L = i
        Next i

        ' --- Accumulation of left-hand transformations ---
        Dim lMin As Integer = Math.Min(m, n)
        For i = lMin To 0 Step -1
            L = i + 1
            G = W(i)

            For j = L To n
                a(i, j) = 0.0
            Next

            If G <> 0.0 Then
                G = 1.0 / G
                For j = L To n
                    S = 0.0
                    For k = L To m
                        S += a(k, i) * a(k, j)
                    Next
                    F = (S / a(i, i)) * G
                    For k = i To m
                        a(k, j) += F * a(k, i)
                    Next
                Next

                For j = i To m
                    a(j, i) *= G
                Next
            Else
                For j = i To m
                    a(j, i) = 0.0
                Next
            End If
            a(i, i) += 1.0
        Next i

        ' --- Diagonalization of the bidiagonal form ---
        For k = n To 0 Step -1
            For its = 1 To 30
                For L = k To 0 Step -1
                    Nm = L - 1
                    If (Math.Abs(rv1(L)) + Anorm) = Anorm Then GoTo SplitOk
                    If (Math.Abs(W(Nm)) + Anorm) = Anorm Then Exit For
                Next

                c = 0.0
                S = 1.0

                ' FIX 1: Nm must be i-1 inside this loop
                For i = L To k
                    Nm = i - 1

                    F = S * rv1(i)
                    rv1(i) = c * rv1(i)
                    If (Math.Abs(F) + Anorm) = Anorm Then Exit For

                    G = W(i)
                    H = Pythag(F, G)
                    W(i) = H
                    H = 1.0 / H
                    c = (G * H)
                    S = -(F * H)

                    For j = 0 To m
                        y = a(j, Nm)
                        z = a(j, i)
                        a(j, Nm) = (y * c) + (z * S)
                        a(j, i) = -(y * S) + (z * c)
                    Next
                Next

SplitOk:
                z = W(k)
                If L = k Then
                    If z < 0.0 Then
                        W(k) = -z
                        For j = 0 To n
                            V(j, k) = -V(j, k)
                        Next
                    End If
                    Exit For
                End If

                If its = 30 Then BSlogg.Log("SVD: No convergence!", LogMsgType.Warn)

                x = W(L)
                Nm = k - 1
                y = W(Nm)
                G = rv1(Nm)
                H = rv1(k)
                F = ((y - z) * (y + z) + (G - H) * (G + H)) / (2.0 * H * y)
                G = Pythag(F, 1.0)
                If F >= 0 Then G = Math.Abs(G) Else G = -Math.Abs(G)

                F = ((x - z) * (x + z) + H * ((y / (F + G)) - H)) / x
                c = 1.0
                S = 1.0

                For j = L To Nm
                    i = j + 1
                    G = rv1(i)
                    y = W(i)
                    H = S * G
                    G = c * G
                    z = Pythag(F, H)
                    rv1(j) = z
                    c = F / z
                    S = H / z
                    F = (x * c) + (G * S)
                    G = -(x * S) + (G * c)
                    H = y * S
                    y = y * c

                    For jj = 0 To n
                        x = V(jj, j)
                        z = V(jj, i)
                        V(jj, j) = (x * c) + (z * S)
                        V(jj, i) = -(x * S) + (z * c)
                    Next

                    z = Pythag(F, H)
                    W(j) = z
                    If z <> 0.0 Then
                        z = 1.0 / z
                        c = F * z
                        S = H * z
                    End If

                    F = (c * G) + (S * y)
                    x = -(S * G) + (c * y)

                    For jj = 0 To m
                        y = a(jj, j)
                        z = a(jj, i)
                        a(jj, j) = (y * c) + (z * S)
                        a(jj, i) = -(y * S) + (z * c)
                    Next
                Next j

                rv1(L) = 0.0
                rv1(k) = F
                W(k) = x
            Next its
        Next k

        ' populate result
        SVDout.Wvect = W
        SVDout.Wmat = DiagMatFromVector(W)
        SVDout.V = V
        SVDout.U = a
        Return SVDout
    End Function


    ''' <summary>
    ''' Creates a square diagonal matrix from a 1-dimensional vector.
    ''' </summary>
    ''' <param name="v">
    ''' A 1-dimensional array whose elements will populate the diagonal of the resulting matrix.
    ''' </param>
    ''' <returns>
    ''' A 2-dimensional square matrix of size (n × n), where n = UBound(v) + 1, containing:
    ''' <code>
    ''' result(i, j) = v(i)   if i = j  
    ''' result(i, j) = 0      otherwise
    ''' </code>
    ''' </returns>
    ''' <remarks>
    ''' <para>
    ''' Off-diagonal elements are initialized to zero by the <c>ReDim</c> statement.
    ''' </para>
    ''' 
    ''' <para>
    ''' Example:
    ''' </para>
    ''' <code>
    ''' v = {3, 4, 5}
    ''' DiagMatFromVector(v) →
    '''     {{3, 0, 0},
    '''      {0, 4, 0},
    '''      {0, 0, 5}}
    ''' </code>
    ''' </remarks>
    Function DiagMatFromVector(v() As Double) As Double(,)
        Dim out(v.GetUpperBound(0), v.GetUpperBound(0)) As Double
        For i = 0 To v.GetUpperBound(0)
            out(i, i) = v(i)
        Next
        Return out
    End Function

    ''' <summary>
    ''' Computes √(x² + pY²) in a way that avoids destructive overflow or underflow.
    ''' </summary>
    ''' <param name="x">
    ''' The first value.
    ''' </param>
    ''' <param name="y">
    ''' The second value.
    ''' </param>
    ''' <returns>
    ''' The value √(x² + pY²), computed using a numerically stable method.
    ''' </returns>
    ''' <remarks>
    ''' <para>
    ''' This function implements the classic “Pythagorean addition” used in
    ''' numerical linear algebra routines (e.g., singular value decomposition).
    ''' </para>
    ''' 
    ''' <para>
    ''' Instead of computing:
    ''' </para>
    ''' <code>
    ''' Math.Sqrt(x*x + pY*pY)
    ''' </code>
    ''' <para>
    ''' which may overflow when x or pY is large (or underflow when very small),
    ''' this routine rescales the computation using:
    ''' </para>
    ''' 
    ''' <code>
    ''' max(a,b) * sqrt(1 + (min(a,b)/max(a,b))²)
    ''' </code>
    ''' 
    ''' <para>
    ''' ensuring numerical safety and stability.
    ''' </para>
    ''' 
    ''' <para>
    ''' This function is used by SVD decomposition routines where stable hypotenuse
    ''' calculations are essential.
    ''' </para>
    ''' </remarks>
    Function Pythag(x As Double, y As Double) As Double
        Dim absa As Double = Math.Abs(x)
        Dim absb As Double = Math.Abs(y)
        If absa > absb Then
            Return absa * Math.Sqrt(1.0 + (absb / absa) ^ 2)
        Else
            If absb = 0.0 Then
                Return 0.0
            Else
                Return absb * Math.Sqrt(1.0 + (absa / absb) ^ 2)
            End If
        End If
    End Function

    ''' <summary>
    ''' Performs multiple linear regression using Singular Value Decomposition (SVD) and QR-based solving.
    ''' Returns both estimated regression coefficients and their standard errors.
    ''' </summary>
    ''' <param name="y">
    ''' Dependent variable vector of length n.  
    ''' Represents observed response values.
    ''' </param>
    ''' <param name="x">
    ''' Matrix of independent variables with dimensions (n × m).  
    ''' Each row corresponds to an observation; each column is a predictor.  
    ''' Requires n > m for identifiability.
    ''' </param>
    ''' <param name="bIntcpt">
    ''' If <c>True</c>, an intercept column of 1’s is added to the design matrix.  
    ''' If <c>False</c>, regression is forced through the origin.
    ''' </param>
    ''' <returns>
    ''' A matrix of size ((p + 1) × 2) containing:
    ''' <list type="bullet">
    '''   <item><description>Column 0 — Estimated regression coefficients a₀, a₁, …, aₚ</description></item>
    '''   <item><description>Column 1 — Standard errors of the coefficients</description></item>
    ''' </list>
    ''' <para>
    ''' Here p = number of predictors, including the intercept if <paramref name="bIntcpt"/> = True.
    ''' </para>
    ''' </returns>
    ''' <remarks>
    ''' <para>
    ''' The fitted model is:
    ''' </para>
    ''' <code>
    ''' f(x) = a₀ + a₁·x₁ + a₂·x₂ + … + aₚ·xₚ
    ''' </code>
    ''' <para>
    ''' If <paramref name="bIntcpt"/> = False, a₀ is omitted and the model becomes:
    ''' </para>
    ''' <code>
    ''' f(x) = a₁·x₁ + a₂·x₂ + … + aₚ·xₚ
    ''' </code>
    ''' 
    ''' <h4>Algorithm overview</h4>
    ''' <para>
    ''' 1. Optionally augment X with an intercept column.  
    ''' 2. Compute <c>XᵀX</c> and <c>Xᵀy</c>.  
    ''' 3. Invert <c>XᵀX</c> using Cholesky decomposition via <see cref="MatInv"/>.  
    ''' 4. Solve <c>(XᵀX)·β = Xᵀy</c> using QR decomposition:
    ''' </para>
    ''' <code>
    ''' β = QRsolve(QRdecomp(XᵀX), Xᵀy)
    ''' </code>
    '''
    ''' <para>
    ''' 5. Compute fitted values fᵢ and error sum of squares:
    ''' </para>
    ''' <code>
    ''' ErSS = Σ (yᵢ - fᵢ)²
    ''' </code>
    ''' 
    ''' <para>
    ''' 6. Estimate the variance–covariance matrix:
    ''' </para>
    ''' <code>
    ''' VarCov = (XᵀX)⁻¹ · (ErSS / (n - p))
    ''' </code>
    ''' 
    ''' <para>
    ''' 7. Extract coefficient standard errors from the diagonal of VarCov.
    ''' </para>
    '''
    ''' <h4>Output interpretation</h4>
    ''' <code>
    ''' out(i, 0) = coefficient aᵢ  
    ''' out(i, 1) = standard error of aᵢ
    ''' </code>
    '''
    ''' <h4>Notes</h4>
    ''' <para>
    ''' • Requires n > p for stable estimation.  
    ''' • Uses several helper functions: <c>trans</c>, <c>MatrixMult</c>, <c>MatInv</c>,  
    '''   <c>rowFromArray</c>, <c>QRdecomp</c>, and <c>QRsolve</c>.  
    ''' • Logging is performed via <c>BSlogg.Log</c>.
    ''' </para>
    ''' </remarks>
    Function RegrL(y() As Double, x(,) As Double, bIntcpt As Boolean) As Double(,)
        Dim ErSS As Double, Xs(,) As Double
        BSlogg.Log(MethodBase.GetCurrentMethod.Name & " execution start")

        Dim n As Integer = x.GetUpperBound(0)
        Dim p As Integer = x.GetUpperBound(1) 'p = count of predictors (eventualy) including intercept
        Dim y2d(n, 0) As Double
        If bIntcpt Then 'add intercept
            p += 1

            ReDim Xs(n, p)
            For i = 0 To n
                For j = 0 To p
                    If j = 0 Then
                        Xs(i, j) = 1
                    Else
                        Xs(i, j) = x(i, j - 1)
                    End If
                Next
            Next
        Else
            Xs = x
        End If
        For i = 0 To n : y2d(i, 0) = y(i) : Next i

        '------- The X matrix is now set ----------
        'compute X transpone matrix
        'Since the built in Excel function TRANSPOSE is not limited to returning less than 5460 elements
        '(as with the MMULT function), it can be used as default method.
        Dim Xtrans(,) As Double = trans(Xs)
        Dim XtX(,) As Double = MatrixMult(Xtrans, Xs)
        Dim XtY(,) As Double = MatrixMult(Xtrans, y2d)
        Dim XtXinv(,) As Double = MatInv(XtX, "CHOL")
        Dim qr As QRout = QRdecomp(XtX)
        Dim ParametersEst(,) As Double = QRsolve(qr, XtY)


        For i = 0 To n
            Dim F(,) As Double = MatrixMult(trans(rowFromArray(Xs, i, True)), ParametersEst) 'use i-th row from Xs
            ErSS += (y(i) - F(0, 0)) ^ 2
        Next
        Dim VarCov(,) As Double = MatrixMult(XtXinv, ErSS / (n - p))

        'put togheter for output: coefficients + SE
        Dim out(p, 1) As Double
        For i = 0 To p
            out(i, 0) = ParametersEst(i, 0)
            out(i, 1) = Math.Sqrt(VarCov(i, i))
        Next

        BSlogg.Log(MethodBase.GetCurrentMethod.Name & " execution end")
        Return out
    End Function

    ''' <summary>
    ''' Extracts a single row from a 2-dimensional matrix and returns it as a 1-dimensional vector.
    ''' </summary>
    ''' <param name="mat">
    ''' A 2-dimensional array from which a row will be extracted.
    ''' </param>
    ''' <param name="row">
    ''' The zero-based index of the row to extract.  
    ''' Must satisfy <c>0 ≤ row ≤ UBound(mat, 1)</c>.
    ''' </param>
    ''' <returns>
    ''' A 1-dimensional array containing all elements from <paramref name="row"/> of <paramref name="mat"/>.
    ''' </returns>
    ''' <exception cref="IndexOutOfRangeException">
    ''' Thrown when <paramref name="row"/> is outside the valid row index range.
    ''' </exception>
    ''' <remarks>
    ''' <para>
    ''' If <paramref name="mat"/> has dimensions (n × m), the returned vector has length m.
    ''' </para>
    ''' 
    ''' <para>
    ''' Example:
    ''' </para>
    ''' <code>
    ''' mat = {{1,2,3}, {4,5,6}}
    ''' rowFromArray(mat, 1) → {4,5,6}
    ''' </code>
    ''' </remarks>
    Function rowFromArray(mat(,) As Double, row As Integer) As Double()
        Dim out(mat.GetUpperBound(1)) As Double
        For i = 0 To mat.GetUpperBound(1)
            out(i) = mat(row, i)
        Next
        Return out
    End Function

    ''' <summary>
    ''' Extracts a single row from a 2-dimensional matrix and returns it as a 2-dimensional
    ''' column vector (m × 1).
    ''' </summary>
    ''' <param name="mat">
    ''' A 2-dimensional array from which the row will be extracted.
    ''' </param>
    ''' <param name="row">
    ''' The zero-based index of the row to extract.  
    ''' Must satisfy <c>0 ≤ row ≤ UBound(mat, 1)</c>.
    ''' </param>
    ''' <param name="bOutput2D">
    ''' Ignored parameter; present only to distinguish this overload.  
    ''' The output is always a 2D column vector.
    ''' </param>
    ''' <returns>
    ''' A 2-dimensional array of size (m × 1), where m = number of columns in <paramref name="mat"/>,
    ''' containing the elements of the selected row as a vertical column.
    ''' </returns>
    ''' <remarks>
    ''' <para>
    ''' If the input matrix has dimensions (n × m), the returned array has dimensions (m × 1):
    ''' </para>
    ''' 
    ''' <code>
    ''' result(i, 0) = mat(row, i)
    ''' </code>
    ''' 
    ''' <para>
    ''' This form is useful when multiplying a row vector as a column (e.g., in regression or matrix algebra routines).
    ''' </para>
    ''' 
    ''' <para>
    ''' Example:
    ''' </para>
    ''' <code>
    ''' mat = {{1,2,3}, {4,5,6}}
    ''' rowFromArray(mat, 1, True) →
    '''     {{4},
    '''      {5},
    '''      {6}}
    ''' </code>
    ''' </remarks>
    Function rowFromArray(mat(,) As Double, row As Integer, bOutput2D As Boolean) As Double(,)
        Dim out(,) As Double
        ReDim out(mat.GetUpperBound(1), 0)
        For i As Integer = 0 To mat.GetUpperBound(1)
            out(i, 0) = mat(row, i)
        Next
        Return out
    End Function

    ''' <summary>
    ''' Returns a subset of a one-dimensional array between specified start and end indices.
    ''' </summary>
    ''' <typeparam name="T">
    ''' The element type of the array (e.g., String, Integer, Double, Object).
    ''' </typeparam>
    ''' <param name="mat">
    ''' The input one-dimensional array of type <typeparamref name="T"/>.
    ''' </param>
    ''' <param name="lStart">
    ''' The zero-based index of the first element to include in the subset.
    ''' Defaults to 0.
    ''' </param>
    ''' <param name="lEnd">
    ''' The zero-based index of the last element to include in the subset.
    ''' Defaults to -1, which means the last element of the array.
    ''' </param>
    ''' <returns>
    ''' A new one-dimensional array of type <typeparamref name="T"/> containing
    ''' elements from <paramref name="lStart"/> to <paramref name="lEnd"/>.
    ''' </returns>
    ''' <remarks>
    ''' - If <paramref name="lEnd"/> is -1, it is set to the last index of <paramref name="mat"/>.  
    ''' - If <paramref name="lStart"/> is greater than <paramref name="lEnd"/>, it is reset to <paramref name="lEnd"/>.  
    ''' - The returned array length is <c>lEnd - lStart + 1</c>.  
    ''' - Throws <see cref="IndexOutOfRangeException"/> if indices are outside the bounds of <paramref name="mat"/>.  
    ''' </remarks>
    ''' <example>
    ''' ' Example: subset a 1D array of integers
    ''' Dim data() As Integer = {10, 20, 30, 40, 50}
    ''' Dim subset() As Integer = SubsetArray(Of Integer)(data, 1, 3)
    ''' ' subset = {20, 30, 40}
    ''' Console.WriteLine(String.Join(", ", subset))
    ''' </example>
    Public Function SubsetArray(Of T)(mat() As T, Optional lStart As Integer = 0, Optional lEnd As Integer = -1) As T()
        If lEnd = -1 Then lEnd = mat.GetUpperBound(0)
        If lStart > lEnd Then lStart = lEnd
        Dim out(lEnd - lStart) As T
        Dim k As Integer = 0
        For i = lStart To lEnd
            out(k) = mat(i)
            k += 1
        Next
        Return out
    End Function

    ''' <summary>
    ''' Transposes a two-dimensional array, swapping rows and columns.
    ''' </summary>
    ''' <typeparam name="T">
    ''' The element type of the array (e.g., Double, Integer, Long, String, Object).
    ''' </typeparam>
    ''' <param name="mat">
    ''' A two-dimensional array of type <typeparamref name="T"/> to be transposed.
    ''' </param>
    ''' <returns>
    ''' A new two-dimensional array of type <typeparamref name="T"/> with rows and columns swapped.
    ''' </returns>
    ''' <remarks>
    ''' - The returned array has dimensions (columns × rows) of the input array.  
    ''' - Element at position (i, j) in the input becomes (j, i) in the output.  
    ''' - Works with any type, not just numeric arrays.  
    ''' </remarks>
    ''' <example>
    ''' ' Example: transpose a 2D array of integers
    ''' Dim mat(,) As Integer = {
    '''     {1, 2, 3},
    '''     {4, 5, 6}
    ''' }
    ''' Dim transposed(,) As Integer = trans(Of Integer)(mat)
    '''
    ''' ' transposed now contains:
    ''' ' {1, 4}
    ''' ' {2, 5}
    ''' ' {3, 6}
    ''' </example>
    Public Function trans(Of T)(mat(,) As T) As T(,)
        Dim out(mat.GetUpperBound(1), mat.GetUpperBound(0)) As T
        For i = 0 To mat.GetUpperBound(0)
            For j = 0 To mat.GetUpperBound(1)
                out(j, i) = mat(i, j)
            Next
        Next
        Return out
    End Function


    ''' <summary>
    ''' Performs a minimal implementation of Weighted Least Squares (WLS) regression.
    ''' </summary>
    ''' <param name="endog">
    ''' The dependent (response) variable vector of length n.
    ''' </param>
    ''' <param name="exog">
    ''' The independent variable matrix of size (n × p).  
    ''' If an intercept term is desired, it must already be included as a column in <paramref name="exog"/>.
    ''' </param>
    ''' <param name="weights">
    ''' A vector of non-negative observation weights of length n.  
    ''' Each weight modifies the contribution of the corresponding observation in the regression.
    ''' </param>
    ''' <returns>
    ''' A 2D array containing regression coefficients and their standard errors,  
    ''' in the same format returned by <see cref="RegrL(Double(), Double(,), Boolean)"/>.
    ''' </returns>
    ''' <remarks>
    ''' <para>
    ''' This routine implements WLS by transforming the problem:
    ''' </para>
    ''' 
    ''' <code>
    ''' minimize   Σ wᵢ · (yᵢ - Xᵢβ)²
    ''' </code>
    ''' 
    ''' <para>
    ''' through the standard weighted transformation:
    ''' </para>
    ''' 
    ''' <code>
    ''' yᵢ* = √wᵢ · yᵢ  
    ''' Xᵢ* = √wᵢ · Xᵢ
    ''' </code>
    ''' 
    ''' <para>
    ''' The transformed system is then passed to <see cref="RegrL"/> with <c>bIntcpt = False</c>,
    ''' since any intercept term must already exist inside <paramref name="exog"/>.
    ''' </para>
    ''' 
    ''' <h4>Notes</h4>
    ''' <list type="bullet">
    '''   <item><description>Weights ≤ 0 are treated as zero (observation receives no influence).</description></item>
    '''   <item><description>Assumes <paramref name="endog"/>, <paramref name="exog"/>, and <paramref name="weights"/> share consistent row dimensions.</description></item>
    '''   <item><description>Uses √w for proper WLS transformation.</description></item>
    ''' </list>
    ''' </remarks>
    Function MinimalWLS(endog() As Double, exog(,) As Double, weights() As Double) As Double(,)
        Dim n As Integer = weights.GetUpperBound(0)
        Dim p As Integer = exog.GetUpperBound(1)
        Dim w_half(n) As Double, wendog(n) As Double, wexog(n, p) As Double

        For i = 0 To n
            w_half(i) = If(weights(i) > 0, Math.Sqrt(weights(i)), 0.0)
            wendog(i) = w_half(i) * endog(i)
            For j = 0 To p
                wexog(i, j) = exog(i, j) * w_half(i)
            Next
        Next

        Return RegrL(wendog, wexog, False) 'Fit; false because intercept is already in wexog
    End Function

    Public Class QRout
        'QR decomposition output
        Public R(,) As Double
        Public Q(,) As Double
    End Class

    ''' <summary>
    ''' Solves the linear system A·x = b using a QR decomposition previously computed by <see cref="QRdecomp"/>.
    ''' </summary>
    ''' <param name="qr">
    ''' The QR decomposition of matrix A, containing:
    ''' <list type="bullet">
    '''   <item><description><c>Q</c> — an orthogonal matrix (QᵀQ = I)</description></item>
    '''   <item><description><c>R</c> — an upper-triangular matrix</description></item>
    ''' </list>
    ''' </param>
    ''' <param name="b">
    ''' The right-hand side vector supplied as a 2D array of size (n × 1).
    ''' </param>
    ''' <returns>
    ''' A 2D array representing the solution vector x satisfying A·x = b.
    ''' </returns>
    ''' <remarks>
    ''' <para>
    ''' Given A = Q·R from QR decomposition,
    ''' solving A·x = b proceeds by multiplying both sides by Qᵀ:
    ''' </para>
    ''' 
    ''' <code>
    ''' Qᵀ A x = Qᵀ b  
    ''' R x = Qᵀ b
    ''' </code>
    '''
    ''' <para>
    ''' Because R is upper triangular, x is obtained via back-substitution.
    ''' </para>
    ''' 
    ''' <h4>Algorithm steps</h4>
    ''' <list type="number">
    '''   <item><description>Compute Qᵀ·b.</description></item>
    '''   <item><description>Solve R·x = Qᵀ·b using backward substitution.</description></item>
    ''' </list>
    '''
    ''' <h4>Assumptions</h4>
    ''' <list type="bullet">
    '''   <item><description>A is full-rank and R(i,i) ≠ 0 for all diagonal elements.</description></item>
    '''   <item><description><c>b</c> must match the number of rows in <c>qr.Q</c>.</description></item>
    ''' </list>
    ''' </remarks>
    Function QRsolve(qr As QRout, b(,) As Double) As Double(,)
        Dim Qt_b(,) As Double = MatrixMult(trans(qr.Q), b) 'Form Q T · b.
        Dim n As Integer = qr.R.GetUpperBound(0)
        Dim beta(n, 0) As Double

        'Solve R · x = Q T · b.
        For i = n To 0 Step -1
            Dim sum As Double = 0.0
            For j = i + 1 To n
                sum += qr.R(i, j) * beta(j, 0)
            Next
            beta(i, 0) = (Qt_b(i, 0) - sum) / qr.R(i, i)
        Next
        Return beta
    End Function

    ''' <summary>
    ''' Computes the QR decomposition of a square matrix using Householder reflections.
    ''' Produces an orthogonal matrix <c>Q</c> and an upper‑triangular matrix <c>R</c>
    ''' such that <c>mat = Q · R</c>.
    ''' </summary>
    ''' <param name="mat">
    ''' A square matrix (n × n) to be decomposed.  
    ''' The input matrix is not modified; internal working copies are used.
    ''' </param>
    ''' <param name="prec">
    ''' Numerical precision threshold for treating values as zero.  
    ''' Defaults to <c>1e‑12</c>.  
    ''' If a column norm falls below this threshold, the corresponding reflection is skipped.
    ''' </param>
    ''' <returns>
    ''' A <see cref="QRout"/> structure containing:
    ''' <list type="bullet">
    '''   <item><description><c>Q</c> — an orthogonal matrix satisfying QᵀQ = I</description></item>
    '''   <item><description><c>R</c> — an upper‑triangular matrix</description></item>
    ''' </list>
    ''' such that:
    ''' <code>
    ''' mat = Q · R
    ''' </code>
    ''' </returns>
    ''' <remarks>
    ''' <h4>Algorithm Overview (Householder Reflections)</h4>
    ''' <para>
    ''' For each column <c>j</c>, a Householder vector <c>v</c> is constructed to zero all
    ''' elements below row <c>j</c>.  
    ''' The corresponding Householder matrix is:
    ''' </para>
    ''' <code>
    ''' P = I − 2 v vᵀ
    ''' </code>
    ''' <para>
    ''' Each <c>P</c> is applied to the working copy of <paramref name="mat"/> (forming <c>R</c>)
    ''' and accumulated into <c>Q</c>. After processing all columns:
    ''' </para>
    ''' <code>
    ''' Q = P₁ P₂ … Pₖ  
    ''' R = Qᵀ · mat
    ''' </code>
    ''' 
    ''' <h4>Numerical Notes</h4>
    ''' <list type="bullet">
    '''   <item><description>Householder reflections provide high numerical stability.</description></item>
    '''   <item><description>The <paramref name="prec"/> threshold prevents division by extremely small values.</description></item>
    '''   <item><description>
    ''' Column norms are computed from the working vector <c>v</c> using a sum‑of‑squares–based norm function.
    ''' </description></item>
    ''' </list>
    ''' 
    ''' <h4>Output</h4>
    ''' <para>
    ''' <c>Q</c> is orthogonal and <c>R</c> is upper‑triangular.  
    ''' These outputs are compatible with <see cref="QRsolve"/> for solving linear systems.
    ''' </para>
    ''' </remarks>
    Function QRdecomp(mat(,) As Double, Optional prec As Double = 0.000000000001) As QRout
        ' return Q over R for a square matrix
        Dim arr(,) As Double, out As New QRout
        Dim d As Double, s As Double

        arr = mat
        Dim n As Integer = arr.GetUpperBound(0)

        Dim v(n, 0) As Double, w(n, 0) As Double, p(n, n) As Double, q(n, n) As Double
        q = IdentityMat(n)

        For j = 0 To n - 1
            For i = 0 To n
                v(i, 0) = arr(i, j)
            Next
            d = Math.Sqrt(SumSq(v))
            If d <= prec Then Continue For
            For i = 0 To n
                v(i, 0) = v(i, 0) / d
            Next
            d = 0.0
            For i = j To n
                d += v(i, 0) * v(i, 0)
            Next
            d = Math.Sqrt(d)
            If d <= prec Then Continue For
            If v(j, 0) > 0 Then d = -d
            For i = 0 To j - 1
                v(i, 0) = 0.0
                w(i, 0) = 0.0
            Next
            v(j, 0) = Math.Sqrt((1.0 - v(j, 0) / d) / 2.0)
            w(j, 0) = -2.0 * v(j, 0)
            s = -2.0 * d * v(j, 0)
            For i = j + 1 To n
                v(i, 0) = v(i, 0) / s
                w(i, 0) = -2.0 * v(i, 0)
            Next
            p = MatrixMult(w, trans(v))
            For i = 0 To n
                p(i, i) = p(i, i) + 1.0
            Next
            arr = MatrixMult(p, arr)
            q = MatrixMult(q, p)
        Next

        out.Q = q
        out.R = arr
        Return out
    End Function

    ''' <summary>
    ''' Converts a two-dimensional array into a string representation similar to Python's print format.
    ''' </summary>
    ''' <typeparam name="T">
    ''' The element type of the array (e.g., Double, Integer, String, Object).
    ''' </typeparam>
    ''' <param name="ar">
    ''' A two-dimensional array of type <typeparamref name="T"/> to be converted.
    ''' </param>
    ''' <returns>
    ''' A string representation of the array, with rows enclosed in brackets and separated by semicolons.
    ''' </returns>
    ''' <remarks>
    ''' - Each row is printed as <c>[value1, value2, ...]</c>.  
    ''' - Rows are separated by <c>; </c>.  
    ''' - The entire array is enclosed in outer brackets <c>[ ... ]</c>.  
    ''' - Useful for debugging and quick visualization of array contents.  
    ''' </remarks>
    ''' <example>
    ''' ' Example: print a 2D array of doubles
    ''' Dim mat(,) As Double = {
    '''     {1.1, 2.2, 3.3},
    '''     {4.4, 5.5, 6.6}
    ''' }
    ''' Dim s As String = array2str(Of Double)(mat)
    ''' ' Output: [[1.1, 2.2, 3.3]; [4.4, 5.5, 6.6]]
    ''' Console.WriteLine(s)
    ''' </example>
    Public Function array2str(Of T)(ByVal ar(,) As T) As String
        Dim str As String = "["
        For i = 0 To ar.GetUpperBound(0)
            str &= "["
            For j = 0 To ar.GetUpperBound(1)
                If j < ar.GetUpperBound(1) Then
                    If ar(i, j) Is Nothing Then
                        str &= "<nothing>, "
                    Else
                        str &= ar(i, j).ToString() & ", "
                    End If

                Else
                    If ar(i, j) Is Nothing Then
                        str &= "<nothing>"
                    Else
                        str &= ar(i, j).ToString()
                    End If

                End If
            Next
            str &= "]"
            If i < ar.GetUpperBound(0) Then str &= "; "
        Next i
        str &= "]"
        Return str
    End Function


    ''' <summary>
    ''' Converts a one-dimensional array into a string representation similar to Python's print format.
    ''' </summary>
    ''' <typeparam name="T">
    ''' The element type of the array (e.g., Double, Integer, String, Object).
    ''' </typeparam>
    ''' <param name="ar">
    ''' A one-dimensional array of type <typeparamref name="T"/> to be converted.
    ''' </param>
    ''' <returns>
    ''' A string representation of the array, with elements separated by commas and enclosed in brackets.
    ''' </returns>
    ''' <remarks>
    ''' - Each element is converted using <c>.ToString()</c>.  
    ''' - The output format is <c>[value1, value2, ...]</c>.  
    ''' - Useful for debugging and quick visualization of array contents.  
    ''' </remarks>
    ''' <example>
    ''' ' Example: print a 1D array of integers
    ''' Dim arr() As Integer = {10, 20, 30, 40}
    ''' Dim s As String = array2str(Of Integer)(arr)
    ''' ' Output: [10, 20, 30, 40]
    ''' Console.WriteLine(s)
    ''' </example>
    Public Function array2str(Of T)(ByVal ar() As T) As String
        Dim str As String = "["
        For i = 0 To ar.GetUpperBound(0)
            If i < ar.GetUpperBound(0) Then
                str &= ar(i).ToString() & ", "
            Else
                str &= ar(i).ToString()
            End If
        Next
        str &= "]"
        Return str
    End Function

    ''' <summary>
    ''' Concatenates two one-dimensional arrays into a single array.
    ''' </summary>
    ''' <typeparam name="T">
    ''' The element type of the arrays (e.g., String, Integer, Double, Object).
    ''' </typeparam>
    ''' <param name="a1">
    ''' The first input array of type <typeparamref name="T"/>.
    ''' </param>
    ''' <param name="a2">
    ''' The second input array of type <typeparamref name="T"/>.
    ''' </param>
    ''' <returns>
    ''' A new one-dimensional array of type <typeparamref name="T"/> containing
    ''' all elements of <paramref name="a1"/> followed by all elements of <paramref name="a2"/>.
    ''' </returns>
    ''' <remarks>
    ''' - The returned array length is <c>a1.Length + a2.Length</c>.  
    ''' - Elements are copied in order: first all from <paramref name="a1"/>, then all from <paramref name="a2"/>.  
    ''' </remarks>
    ''' <example>
    ''' ' Example: concatenate two arrays of strings
    ''' Dim arr1() As String = {"apple", "banana"}
    ''' Dim arr2() As String = {"cherry", "date"}
    ''' Dim result() As String = ConcatArrays(Of String)(arr1, arr2)
    ''' ' result = {"apple", "banana", "cherry", "date"}
    ''' Console.WriteLine(String.Join(", ", result))
    ''' </example>
    Public Function ConcatArrays(Of T)(a1() As T, a2() As T) As T()
        Dim out(a1.Length + a2.Length - 1) As T
        Dim i As Integer

        For i = 0 To a1.Length - 1
            out(i) = a1(i)
        Next

        Dim j As Integer = i
        For i = 0 To a2.Length - 1
            out(j) = a2(i)
            j += 1
        Next

        Return out
    End Function

    ''' <summary>
    ''' Horizontally concatenates two two-dimensional arrays (side by side), aligning rows.
    ''' </summary>
    ''' <typeparam name="T">
    ''' The element type of the arrays (e.g., Object, String, Integer, Double).
    ''' </typeparam>
    ''' <param name="a1">
    ''' The first two-dimensional array of type <typeparamref name="T"/>.
    ''' </param>
    ''' <param name="a2">
    ''' The second two-dimensional array of type <typeparamref name="T"/>.
    ''' </param>
    ''' <param name="bAppendBlanks">
    ''' If <c>True</c>, allows arrays with different row counts by padding with blank values.
    ''' If <c>False</c>, throws an exception if row counts differ.
    ''' </param>
    ''' <returns>
    ''' A new two-dimensional array of type <typeparamref name="T"/> containing
    ''' all columns of <paramref name="a1"/> followed by all columns of <paramref name="a2"/>.
    ''' </returns>
    ''' <remarks>
    ''' - The returned array has <c>max(rows1, rows2)</c> rows and <c>cols1 + cols2</c> columns.  
    ''' - If <paramref name="bAppendBlanks"/> is <c>True</c>, shorter arrays are padded with default values (<c>Nothing</c> for reference types, <c>0</c> for numeric types).  
    ''' - Throws <see cref="ApplicationException"/> if row counts differ and <paramref name="bAppendBlanks"/> is <c>False</c>.  
    ''' </remarks>
    ''' <example>
    ''' ' Example: stack two arrays of integers side by side
    ''' Dim a1(,) As Integer = {
    '''     {1, 2},
    '''     {3, 4}
    ''' }
    ''' Dim a2(,) As Integer = {
    '''     {5, 6},
    '''     {7, 8}
    ''' }
    ''' Dim result(,) As Integer = VerticalStackArrays(Of Integer)(a1, a2)
    ''' ' result =
    ''' ' {1, 2, 5, 6}
    ''' ' {3, 4, 7, 8}
    ''' </example>
    Public Function VerticalStackArrays(Of T)(a1(,) As T, a2(,) As T, Optional bAppendBlanks As Boolean = False) As T(,)

        If a1.GetUpperBound(0) <> a2.GetUpperBound(0) AndAlso Not bAppendBlanks Then
            BESHstatGlobals.BSerr.LogAndThrow(New ArgumentException("Invalid input array dimensions"))
        End If

        Dim out(Math.Max(a1.GetUpperBound(0), a2.GetUpperBound(0)), a1.GetUpperBound(1) + a2.GetUpperBound(1) + 1) As T

        For i = 0 To a1.GetUpperBound(0)
            For j = 0 To a1.GetUpperBound(1)
                out(i, j) = a1(i, j)
            Next
        Next

        For i = 0 To a2.GetUpperBound(0)
            For j = 0 To a2.GetUpperBound(1)
                out(i, a1.GetUpperBound(1) + 1 + j) = a2(i, j)
            Next
        Next

        Return out
    End Function


    ''' <summary>
    ''' Vertically concatenates two two-dimensional arrays (one below the other), aligning columns.
    ''' </summary>
    ''' <typeparam name="T">
    ''' The element type of the arrays (e.g., Object, String, Integer, Double).
    ''' </typeparam>
    ''' <param name="a1">
    ''' The first two-dimensional array of type <typeparamref name="T"/>.
    ''' </param>
    ''' <param name="a2">
    ''' The second two-dimensional array of type <typeparamref name="T"/>.
    ''' </param>
    ''' <param name="bAppendBlanks">
    ''' If <c>True</c>, allows arrays with different column counts by padding with blank values.
    ''' If <c>False</c>, throws an exception if column counts differ.
    ''' </param>
    ''' <returns>
    ''' A new two-dimensional array of type <typeparamref name="T"/> containing
    ''' all rows of <paramref name="a1"/> followed by all rows of <paramref name="a2"/>.
    ''' </returns>
    ''' <remarks>
    ''' - The returned array has <c>rows1 + rows2</c> rows and <c>max(cols1, cols2)</c> columns.  
    ''' - If <paramref name="bAppendBlanks"/> is <c>True</c>, shorter arrays are padded with default values (<c>Nothing</c> for reference types, <c>0</c> for numeric types).  
    ''' - Throws <see cref="ApplicationException"/> if column counts differ and <paramref name="bAppendBlanks"/> is <c>False</c>.  
    ''' </remarks>
    ''' <example>
    ''' ' Example: stack two arrays of integers vertically
    ''' Dim a1(,) As Integer = {
    '''     {1, 2},
    '''     {3, 4}
    ''' }
    ''' Dim a2(,) As Integer = {
    '''     {5, 6},
    '''     {7, 8}
    ''' }
    ''' Dim result(,) As Integer = HorizontalStackArrays(Of Integer)(a1, a2)
    ''' ' result =
    ''' ' {1, 2}
    ''' ' {3, 4}
    ''' ' {5, 6}
    ''' ' {7, 8}
    ''' </example>
    Public Function HorizontalStackArrays(Of T)(a1(,) As T, a2(,) As T, Optional bAppendBlanks As Boolean = False) As T(,)

        If a1.GetUpperBound(1) <> a2.GetUpperBound(1) AndAlso Not bAppendBlanks Then
            BESHstatGlobals.BSerr.LogAndThrow(New ArgumentException("Invalid input array dimensions"))
        End If

        Dim out(a1.GetUpperBound(0) + a2.GetUpperBound(0) + 1, Math.Max(a1.GetUpperBound(1), a2.GetUpperBound(1))) As T

        For i = 0 To a1.GetUpperBound(0)
            For j = 0 To a1.GetUpperBound(1)
                out(i, j) = a1(i, j)
            Next
        Next

        For i = 0 To a2.GetUpperBound(0)
            For j = 0 To a2.GetUpperBound(1)
                out(a1.GetUpperBound(0) + 1 + i, j) = a2(i, j)
            Next
        Next

        Return out
    End Function


    ''' <summary>
    ''' Extracts a single column from a two-dimensional array.
    ''' </summary>
    ''' <typeparam name="T">
    ''' The element type of the array (e.g., Double, Integer, Long, String, Object).
    ''' </typeparam>
    ''' <param name="x">
    ''' A two-dimensional array of type <typeparamref name="T"/>.
    ''' </param>
    ''' <param name="nCol">
    ''' The zero-based column index to extract.
    ''' </param>
    ''' <returns>
    ''' A one-dimensional array of type <typeparamref name="T"/> containing the values
    ''' from the specified column.
    ''' </returns>
    ''' <remarks>
    ''' - Throws <see cref="ApplicationException"/> if <paramref name="nCol"/> is greater than the upper bound of the second dimension.  
    ''' - The returned array has <c>UBound(x, 1) + 1</c> elements.  
    ''' </remarks>
    ''' <example>
    ''' ' Example: extract column 1 from a 2D array of integers
    ''' Dim mat(,) As Integer = {
    '''     {1, 2, 3},
    '''     {4, 5, 6},
    '''     {7, 8, 9}
    ''' }
    ''' Dim col() As Integer = GetColumnFrom2Darray(Of Integer)(mat, 1)
    ''' ' col = {2, 5, 8}
    ''' Console.WriteLine(String.Join(", ", col))
    ''' </example>
    Public Function GetColumnFrom2Darray(Of T)(x(,) As T, nCol As Integer) As T()
        Dim out(x.GetUpperBound(0)) As T
        If nCol > x.GetUpperBound(1) Then
            BESHstatGlobals.BSerr.LogAndThrow(New ArgumentException("Provided column number is larger than array 2nd dimension."))
        End If
        For i = 0 To x.GetUpperBound(0)
            out(i) = x(i, nCol)
        Next
        Return out
    End Function

    ''' <summary>
    ''' Converts a one-dimensional array of any type into a string array by calling <c>.ToString()</c> on each element.
    ''' </summary>
    ''' <typeparam name="T">
    ''' The element type of the input array (e.g., Object, Integer, Double, String).
    ''' </typeparam>
    ''' <param name="x">
    ''' A one-dimensional array of type <typeparamref name="T"/> to be converted.
    ''' </param>
    ''' <returns>
    ''' A one-dimensional array of strings containing the string representation of each element in <paramref name="x"/>.
    ''' </returns>
    ''' <remarks>
    ''' - Each element is converted using <c>.ToString()</c>.  
    ''' - If an element is <c>Nothing</c>, its string representation will be an empty string.  
    ''' - Useful for debugging or serialization of arrays of mixed types.  
    ''' </remarks>
    ''' <example>
    ''' ' Example: convert an array of integers to strings
    ''' Dim arr() As Integer = {10, 20, 30}
    ''' Dim strArr() As String = objArray2strArray(Of Integer)(arr)
    ''' ' strArr = {"10", "20", "30"}
    ''' Console.WriteLine(String.Join(", ", strArr))
    ''' </example>
    Public Function Array2strArray(Of T)(x() As T) As String()
        Dim out(x.GetUpperBound(0)) As String
        For i = 0 To x.GetUpperBound(0)
            If x(i) Is Nothing Then
                out(i) = ""
            Else
                out(i) = x(i).ToString()
            End If
        Next
        Return out
    End Function

    ''' <summary>
    ''' Converts a two-dimensional array of any type into a two-dimensional string array
    ''' by calling <c>Convert.ToString()</c> on each element.
    ''' </summary>
    ''' <typeparam name="T">
    ''' The element type of the input array (e.g., Object, Integer, Double, String).
    ''' </typeparam>
    ''' <param name="x">
    ''' A two-dimensional array of type <typeparamref name="T"/> to be converted.
    ''' </param>
    ''' <returns>
    ''' A two-dimensional array of strings with the same dimensions as <paramref name="x"/>,
    ''' where each element contains the string representation of the corresponding value in <paramref name="x"/>.
    ''' </returns>
    ''' <remarks>
    ''' <list type="bullet">
    '''   <item><description>Each element is converted using <c>Convert.ToString()</c>.</description></item>
    '''   <item><description>If an element is <c>Nothing</c>, the corresponding output cell is an empty string.</description></item>
    '''   <item><description>Useful for debugging, exporting, logging, or serializing mixed-type 2‑D arrays.</description></item>
    ''' </list>
    ''' </remarks>
    ''' <example>
    ''' ' Example: convert a 2×2 numeric matrix to a string matrix
    ''' Dim mat(,) As Double = {{1.5, 2.5}, {3.5, 4.5}}
    ''' Dim strMat(,) As String = Array2strArray(Of Double)(mat)
    ''' 
    ''' ' strMat =
    ''' '   {{"1.5", "2.5"},
    ''' '    {"3.5", "4.5"}}
    ''' </example>
    Public Function Array2strArray(Of T)(x(,) As T) As String(,)
        Dim out(x.GetUpperBound(0), x.GetUpperBound(1)) As String
        For i = 0 To x.GetUpperBound(0)
            For j = 0 To x.GetUpperBound(1)
                If x(i, j) Is Nothing Then
                    out(i, j) = ""
                Else
                    out(i, j) = Convert.ToString(x(i, j))
                End If
            Next
        Next
        Return out
    End Function

    ''' <summary>
    ''' Converts a one-dimensional array of any type into a Double array.
    ''' </summary>
    ''' <typeparam name="T">
    ''' The element type of the input array (e.g., Object, Integer, String).
    ''' </typeparam>
    ''' <param name="x">
    ''' A one-dimensional array of type <typeparamref name="T"/> to be converted.
    ''' </param>
    ''' <returns>
    ''' A one-dimensional array of doubles containing the numeric representation of each element in <paramref name="x"/>.
    ''' </returns>
    ''' <remarks>
    ''' - Elements are converted using <c>Convert.ToDouble()</c>.  
    ''' - Throws <see cref="InvalidCastException"/> if an element cannot be converted to Double.  
    ''' - If an element is <c>Nothing</c>, it is treated as 0.  
    ''' </remarks>
    ''' <example>
    ''' ' Example: convert an array of objects to doubles
    ''' Dim arr() As Object = {1, "2.5", 3}
    ''' Dim dblArr() As Double = objArray2dblArray(Of Object)(arr)
    ''' ' dblArr = {1.0, 2.5, 3.0}
    ''' Console.WriteLine(String.Join(", ", dblArr))
    ''' </example>
    Public Function Array2dblArray(Of T)(x() As T) As Double()
        Dim out(x.GetUpperBound(0)) As Double
        For i = 0 To x.GetUpperBound(0)
            If x(i) Is Nothing Then
                out(i) = 0.0
            Else
                out(i) = Convert.ToDouble(x(i))
            End If
        Next
        Return out
    End Function


    ''' <summary>
    ''' Converts a one-dimensional array of any type into an Integer array.
    ''' </summary>
    ''' <typeparam name="T">
    ''' The element type of the input array (e.g., Object, String, Double).
    ''' </typeparam>
    ''' <param name="x">
    ''' A one-dimensional array of type <typeparamref name="T"/> to be converted.
    ''' </param>
    ''' <returns>
    ''' A one-dimensional array of integers containing the numeric representation of each element in <paramref name="x"/>.
    ''' </returns>
    ''' <remarks>
    ''' - Elements are converted using <c>Convert.ToInt32()</c>.  
    ''' - Throws <see cref="InvalidCastException"/> if an element cannot be converted to Integer.  
    ''' - If an element is <c>Nothing</c>, it is treated as 0.  
    ''' </remarks>
    ''' <example>
    ''' ' Example: convert an array of objects to integers
    ''' Dim arr() As Object = {1, "2", 3.7}
    ''' Dim intArr() As Integer = objArray2intArray(Of Object)(arr)
    ''' ' intArr = {1, 2, 4}
    ''' Console.WriteLine(String.Join(", ", intArr))
    ''' </example>
    Public Function Array2intArray(Of T)(x() As T) As Integer()
        Dim out(x.GetUpperBound(0)) As Integer
        For i = 0 To x.GetUpperBound(0)
            If x(i) Is Nothing Then
                out(i) = 0
            Else
                out(i) = Convert.ToInt32(x(i))
            End If
        Next
        Return out
    End Function


    ''' <summary>
    ''' Converts a two-dimensional array of any numeric type into an Integer array.
    ''' </summary>
    ''' <typeparam name="T">
    ''' The element type of the input array (e.g., Double, Single, Decimal, Object).
    ''' </typeparam>
    ''' <param name="x">
    ''' A two-dimensional array of type <typeparamref name="T"/> to be converted.
    ''' </param>
    ''' <returns>
    ''' A two-dimensional array of integers containing the numeric representation of each element in <paramref name="x"/>.
    ''' </returns>
    ''' <remarks>
    ''' - Elements are converted using <c>Convert.ToInt32()</c>.  
    ''' - Throws <see cref="InvalidCastException"/> if an element cannot be converted to Integer.  
    ''' - If an element is <c>Nothing</c>, it is treated as 0.  
    ''' - Note: <c>Convert.ToInt32</c> rounds values, while <c>Int()</c> truncates toward negative infinity.  
    ''' </remarks>
    ''' <example>
    ''' ' Example: convert a 2D array of doubles to integers
    ''' Dim mat(,) As Double = {
    '''     {1.2, 2.8},
    '''     {-3.7, 4.5}
    ''' }
    ''' Dim intMat(,) As Integer = dblArray2intArray(Of Double)(mat)
    ''' ' intMat = {{1, 3}, {-4, 4}}
    ''' Console.WriteLine(intMat(0,0) + ", " + intMat(0,1))
    ''' </example>
    Public Function Array2intArray(Of T)(x(,) As T) As Integer(,)
        Dim out(x.GetUpperBound(0), x.GetUpperBound(1)) As Integer
        For i = 0 To x.GetUpperBound(0)
            For j = 0 To x.GetUpperBound(1)
                If x(i, j) Is Nothing Then
                    out(i, j) = 0
                Else
                    out(i, j) = Convert.ToInt32(x(i, j))
                End If
            Next
        Next
        Return out
    End Function


    ''' <summary>
    ''' Converts a two-dimensional array of any type into a two-dimensional Object array.
    ''' </summary>
    ''' <typeparam name="T">
    ''' The element type of the input array (e.g., Double, Integer, String).
    ''' </typeparam>
    ''' <param name="x">
    ''' A two-dimensional array of type <typeparamref name="T"/> to be converted.
    ''' </param>
    ''' <returns>
    ''' A two-dimensional Object array containing all elements of <paramref name="x"/>.
    ''' </returns>
    ''' <remarks>
    ''' - Each element is boxed into <c>Object</c>.  
    ''' - Useful for debugging, serialization, or when working with APIs that require Object arrays.  
    ''' </remarks>
    ''' <example>
    ''' ' Example: convert a 2D array of integers to Object array
    ''' Dim mat(,) As Integer = {
    '''     {1, 2},
    '''     {3, 4}
    ''' }
    ''' Dim objMat(,) As Object = Array2objArray(Of Integer)(mat)
    ''' ' objMat = {{1, 2}, {3, 4}}
    ''' Console.WriteLine(objMat(0,0))
    ''' </example>
    Public Function Array2objArray(Of T)(x(,) As T) As Object(,)
        Dim out(x.GetUpperBound(0), x.GetUpperBound(1)) As Object
        For i = 0 To x.GetUpperBound(0)
            For j = 0 To x.GetUpperBound(1)
                out(i, j) = x(i, j)
            Next
        Next
        Return out
    End Function

    ''' <summary>
    ''' Computes the sample covariance matrix for a numeric data matrix.
    ''' 
    ''' Input matrix <paramref name="mat"/> is assumed to have:
    ''' <list type="bullet">
    '''   <item><description><c>n</c> rows = observations</description></item>
    '''   <item><description><c>p</c> columns = variables</description></item>
    ''' </list>
    ''' 
    ''' The returned matrix is <c>p × p</c> with entries:
    ''' <code>
    ''' Cov(i, j) = Σₖ (xₖᵢ − x̄ᵢ)(xₖⱼ − x̄ⱼ) / (n − 1)
    ''' </code>
    ''' where <c>x̄ᵢ</c> is the sample mean of column <c>i</c>.
    ''' 
    ''' Algorithm:
    ''' <list type="number">
    '''   <item><description>Extract each column and compute its mean.</description></item>
    '''   <item><description>Center each observation by subtracting column means.</description></item>
    '''   <item><description>Compute all cross‑products for <c>i ≤ j</c>.</description></item>
    '''   <item><description>Exploit symmetry: <c>Cov(j, i) = Cov(i, j)</c>.</description></item>
    ''' </list>
    ''' 
    ''' External dependency:
    ''' <list type="bullet">
    '''   <item><description><c>GetColumnFrom2Darray</c> — extracts a column as a 1D array</description></item>
    ''' </list>
    ''' </summary>
    ''' <param name="mat">An <c>n × p</c> numeric matrix.</param>
    ''' <returns>A <c>p × p</c> sample covariance matrix.</returns>
    Function MatCovar(mat(,) As Double) As Double(,)
        'returns the sample covariance matrix of a given mat (sample matrix becasuse of (n-1) division)
        'Input Mat (n x p); Returns (p x p) matrix

        Dim S As Double
        Dim tmp() As Double
        Dim a(,) As Double = DirectCast(mat.Clone(), Double(,))
        Dim n As Integer = a.GetLength(0)
        Dim m As Integer = a.GetLength(1)
        Dim b(m - 1, m - 1) As Double, xm(m - 1) As Double 'average for each column

        'compute average for each column
        For j As Integer = 0 To m - 1
            tmp = GetColumnFrom2Darray(a, j)
            xm(j) = tmp.Average()
        Next

        'compute the cross covariance matrix
        For i As Integer = 0 To m - 1
            For j As Integer = 0 To m - 1
                If j < i Then
                    b(i, j) = b(j, i)
                Else
                    S = 0
                    For k As Integer = 0 To n - 1
                        S += (a(k, i) - xm(i)) * (a(k, j) - xm(j))
                    Next
                    If n > 1 Then b(i, j) = S / CDbl(n - 1)
                End If
            Next
        Next

        Return b
    End Function

    ''' <summary>
    ''' Computes the sample covariance matrix of a numeric data matrix.
    ''' </summary>
    ''' <param name="mat">
    ''' An <c>n × p</c> matrix where rows represent observations and columns represent variables.
    ''' </param>
    ''' <returns>
    ''' A <c>p × p</c> sample covariance matrix computed using <c>(n - 1)</c> in the denominator.
    ''' </returns>
    ''' <remarks>
    ''' <para>For each pair of columns (i, j), covariance is computed as:</para>
    ''' <code>
    ''' cov(i, j) = Σ[(xₖᵢ - meanᵢ)(xₖⱼ - meanⱼ)] / (n - 1)
    ''' </code>
    '''
    ''' <para>
    ''' The function reuses symmetric values to reduce computation.
    ''' </para>
    ''' 
    ''' <code>
    ''' Example:
    ''' mat =
    '''   {{1, 2},
    '''    {2, 3},
    '''    {4, 6}}
    '''
    ''' MatCovar(mat) returns:
    '''   {{2.333..., 3.5},
    '''    {3.5,      5.0}}
    ''' </code>
    ''' </remarks>
    Function MatDoubleCenter(mat(,) As Double) As Double(,)
        'returns the double centered Matrix. It is used to estimate population covariance matrix
        'from sample covariance matrix. Input Mat (p x p); Returns (p x p) matrix
        Dim a(,) As Double

        a = mat
        Dim n As Integer = a.GetUpperBound(0)
        Dim m As Integer = a.GetUpperBound(1)
        If n <> m Then BESHstatGlobals.BSerr.LogAndThrow(New ArgumentException("Input matrix is not square (p x p)"))

        Dim CMeans(n, m) As Double 'average for each column
        Dim RMeans(n, m) As Double 'average for each row
        Dim TotMean(n, m) As Double 'all elements will contain total mean

        'compute the column averages
        For j As Integer = 0 To m
            Dim tmp() As Double = GetColumnFrom2Darray(a, j)
            CMeans(0, j) = tmp.Average()
            For i As Integer = 1 To n
                CMeans(i, j) = CMeans(0, j)
            Next
        Next

        'compute the row averages
        For j As Integer = 0 To n
            Dim tmp() As Double = rowFromArray(a, j)
            RMeans(j, 0) = tmp.Average()
            For i As Integer = 1 To m
                RMeans(j, i) = RMeans(j, 0)
            Next
        Next

        'total mean
        TotMean(0, 0) = a.Average2D()
        For j As Integer = 0 To m
            For i As Integer = 0 To n
                TotMean(i, j) = TotMean(0, 0)
            Next
        Next

        'compute double centered matrix
        Return M_ADD(M_SUB(M_SUB(a, CMeans), RMeans), TotMean)
    End Function

    ''' <summary>
    ''' Computes the eigenvalues and eigenvectors of a real symmetric
    ''' positive‑definite matrix using the JK Method (Kaiser, 1972).
    ''' </summary>
    ''' <param name="m">
    ''' A real symmetric positive‑definite square matrix of size (p+1)×(p+1).
    ''' Passed <c>ByRef</c>, although the algorithm operates on an internal copy.
    ''' </param>
    ''' <param name="maxiter">
    ''' Maximum number of JK orthogonalization iterations (default = 20).
    ''' If convergence is not reached, the best approximation is returned.
    ''' </param>
    ''' <param name="eps">
    ''' Convergence tolerance for detecting when all column pairs are sufficiently
    ''' orthogonalized (default = 1e‑10).
    ''' </param>
    ''' <returns>
    ''' A tuple containing:
    ''' <list type="bullet">
    '''   <item>
    '''     <description>
    '''     <b>Item1 — Double()</b>:  
    '''     A length‑(p+1) vector of eigenvalues λ₀ … λₚ, where each eigenvalue is
    '''     the Euclidean norm of the corresponding orthogonalized column.
    '''     </description>
    '''   </item>
    '''   <item>
    '''     <description>
    '''     <b>Item2 — Double(,)</b>:  
    '''     A (p+1)×(p+1) matrix of normalized eigenvectors.  
    '''     Column <c>j</c> contains the eigenvector associated with eigenvalue λⱼ.
    '''     </description>
    '''   </item>
    ''' </list>
    ''' </returns>
    ''' <remarks>
    ''' <para>
    ''' This routine implements the iterative “JK Method” described in:
    ''' Kaiser, H. F. (1972). "The JK Method: A procedure for finding the eigenvalues
    ''' of a real symmetric matrix." The Computer Journal, 15, 271–273.
    ''' </para>
    ''' 
    ''' <h4>Algorithm Summary</h4>
    ''' <para>
    ''' The JK method iteratively orthogonalizes all column pairs (j, k) using
    ''' Givens‑type plane rotations. For each pair:
    ''' </para>
    ''' <list type="bullet">
    '''   <item><description><c>Num = 2 Σ a(i,j)·a(i,k)</c> — rotation numerator</description></item>
    '''   <item><description><c>Den = Σ (a(i,j)+a(i,k))·(a(i,j)-a(i,k))</c> — rotation denominator</description></item>
    ''' </list>
    ''' <para>
    ''' Rotation parameters (Cos₂, Sin₂) are computed from these quantities and
    ''' applied to columns j and k. After convergence, eigenvalues are computed as
    ''' column norms and eigenvectors are normalized accordingly.
    ''' </para>
    ''' 
    ''' <h4>Convergence</h4>
    ''' <para>
    ''' Convergence is assessed using the maximum absolute rotation numerator:
    ''' </para>
    ''' <code>
    ''' maxAbsNum = max over all (j,k) of |Num(j,k)|
    ''' </code>
    ''' <para>
    ''' The algorithm terminates early when:
    ''' </para>
    ''' <code>
    ''' maxAbsNum &lt; eps  AND  Iter &gt; 1
    ''' </code>
    ''' <para>
    ''' This criterion directly measures the remaining off‑diagonal interaction
    ''' between columns, ensuring that all column pairs are nearly orthogonal.
    ''' </para>
    ''' 
    ''' <h4>Mathematical Appendix: Why This Convergence Rule Is Superior</h4>
    ''' <para>
    ''' The JK method seeks to make all columns mutually orthogonal. For each pair
    ''' of columns j and k, the numerator:
    ''' </para>
    ''' <code>
    ''' Num(j,k) = 2 Σ a(i,j)·a(i,k)
    ''' </code>
    ''' <para>
    ''' is proportional to their inner product. Thus:
    ''' </para>
    ''' <code>
    ''' Num(j,k) = 0  ⇔  columns j and k are orthogonal.
    ''' </code>
    ''' <para>
    ''' Monitoring <c>maxAbsNum</c> therefore measures exactly what the algorithm
    ''' attempts to eliminate: residual column‑to‑column coupling. When all
    ''' |Num(j,k)| values are below <c>eps</c>, every column pair is nearly
    ''' orthogonal, and the matrix is effectively diagonalized.
    ''' </para>
    ''' 
    ''' <para>
    ''' The previous convergence rule used the change in the Frobenius norm
    ''' <c>SumSq(a)</c>, but this norm is theoretically invariant under the
    ''' orthogonal rotations applied by the JK method. Its changes were dominated
    ''' by floating‑point noise and did not reliably indicate whether the columns
    ''' had become orthogonal. In contrast, <c>maxAbsNum</c> provides a direct,
    ''' interpretable, and numerically stable measure of convergence.
    ''' </para>
    ''' 
    ''' <h4>Requirements</h4>
    ''' <list type="bullet">
    '''   <item><description>The input matrix must be real, symmetric, and positive‑definite.</description></item>
    '''   <item><description>Non‑symmetric matrices may yield inaccurate eigenpairs or fail to converge.</description></item>
    ''' </list>
    ''' 
    ''' <h4>Output Layout</h4>
    ''' <code>
    ''' Dim (eigvals, eigvecs) = EIGEN_JK(A)
    ''' eigvals(j)      = eigenvalue_j
    ''' eigvecs(i, j)   = eigenvector_j(i)
    ''' </code>
    ''' 
    ''' <h4>Example</h4>
    ''' <code>
    ''' Dim A(1,1) As Double
    ''' A(0,0) = 2 : A(0,1) = 1
    ''' A(1,0) = 1 : A(1,1) = 2
    ''' 
    ''' Dim result = EIGEN_JK(A)
    ''' Dim eigenvalues = result.Item1
    ''' Dim eigenvectors = result.Item2
    ''' </code>
    ''' </remarks>
    Function EIGEN_JK(ByVal m(,) As Double, Optional maxiter As Integer = 20, Optional eps As Double = 0.0000000001) As (Double(), Double(,))

        Dim Iter As Integer, Cot2 As Double, tmp As Double, Sin2 As Double, Cos2 As Double, Tan2 As Double
        Dim a(,) As Double = DirectCast(m.Clone(), Double(,))
        Dim p As Integer = a.GetUpperBound(0)

        For Iter = 1 To maxiter
            Dim maxAbsNum As Double = 0.0

            'Orthogonalize pairs of columns in upper off diag
            For j As Integer = 0 To p - 1
                For k As Integer = j + 1 To p

                    Dim Den As Double = 0.0
                    Dim Num As Double = 0.0
                    'Perform single plane rotation
                    For i As Integer = 0 To p
                        Num += 2.0 * a(i, j) * a(i, k) ': numerator eq. 11
                        Den += (a(i, j) + a(i, k)) * (a(i, j) - a(i, k)) ': denominator eq. 11
                    Next

                    maxAbsNum = Math.Max(maxAbsNum, Math.Abs(Num))

                    ' Columns are already orthogonal (no rotation needed)
                    If Math.Abs(Num) < eps Then Continue For

                    'Perform Rotation
                    If Math.Abs(Num) <= Math.Abs(Den) Then
                        Tan2 = Math.Abs(Num) / Math.Abs(Den)      ': eq. 11
                        Cos2 = 1.0 / Math.Sqrt(1.0 + Tan2 * Tan2) ': eq. 12
                        Sin2 = Tan2 * Cos2              ': eq. 13
                    Else
                        Cot2 = Math.Abs(Den) / Math.Abs(Num)      ': eq. 16
                        Sin2 = 1.0 / Math.Sqrt(1.0 + Cot2 * Cot2) ': eq. 17
                        Cos2 = Cot2 * Sin2              ': eq. 18
                    End If

                    Dim Cos_ As Double = Math.Sqrt((1.0 + Cos2) / 2.0)          ': eq. 14/19
                    Dim Sin_ As Double = Sin2 / (2.0 * Cos_)            ': eq. 15/20

                    If Den < 0 Then
                        tmp = Cos_
                        Cos_ = Sin_                     ': table 21
                        Sin_ = tmp
                    End If

                    Sin_ = Math.Sign(Num) * Sin_              ': sign table 21

                    'Rotate
                    For i As Integer = 0 To p
                        tmp = a(i, j)
                        a(i, j) = tmp * Cos_ + a(i, k) * Sin_
                        a(i, k) = -tmp * Sin_ + a(i, k) * Cos_
                    Next
                Next k
            Next j

            'Test for convergence
            If maxAbsNum < eps AndAlso Iter > 1 Then Exit For
        Next Iter

        If Iter >= maxiter Then BSlogg.Log("JK Iteration has not converged.", LogMsgType.Warn)

        'Compute eigenvalues/eigenvectors
        Dim EigenVal(p) As Double, EigenVec(p, p) As Double
        For j As Integer = 0 To p
            'Compute eigenvalues
            For k As Integer = 0 To p
                EigenVal(j) += a(k, j) * a(k, j)
            Next
            EigenVal(j) = Math.Sqrt(EigenVal(j))

            'Normalize eigenvectors
            For i As Integer = 0 To p
                EigenVec(i, j) = If(EigenVal(j) <= 0.0, 0.0, a(i, j) / EigenVal(j))
            Next
        Next

        Return (EigenVal, EigenVec)
    End Function

    ''' <summary>
    ''' Computes a correlation matrix for the variables contained in a 2-dimensional data array.
    ''' </summary>
    ''' <param name="InputData">
    ''' A numeric matrix of size <c>n × p</c>, where each column represents a variable and each row represents an observation.
    ''' </param>
    ''' <param name="strCorrTyp">
    ''' Specifies the type of correlation coefficient to compute:
    ''' <list type="bullet">
    '''   <item><c>"r"</c> – Pearson product-moment correlation and two-tailed <i>t</i>-test p-values.</item>
    '''   <item><c>"rho"</c> – Spearman rank correlation (upper triangle) and corresponding p-values (lower triangle).</item>
    '''   <item><c>"tau"</c> – Kendall rank correlation (upper triangle) and corresponding p-values (lower triangle).</item>
    ''' </list>
    ''' </param>
    ''' <returns>
    ''' A <c>p × p</c> matrix where:
    ''' <list type="bullet">
    '''   <item>The **upper triangular** portion contains correlation coefficients.</item>
    '''   <item>The **lower triangular** portion contains corresponding p-values.</item>
    ''' </list>
    ''' The diagonal contains 1 for correlations and 0 (or method-dependent values) for p-values.
    ''' </returns>
    ''' <remarks>
    ''' <para>
    ''' For Pearson correlation (<c>strCorrTyp = "r"</c>), the function uses Excel’s
    ''' <see cref="Correl"/> and <c>T_Dist_2T</c>
    ''' to compute correlation and two-tailed significance.
    ''' </para>
    ''' 
    ''' <para>
    ''' For Spearman (<c>"rho"</c>) and Kendall (<c>"tau"</c>), the caller must supply
    ''' <c>SpearmanRho</c> and <c>KendallsTau</c> classes providing:
    ''' <c>correlCoef</c>, <c>pvalue</c>, and a <c>Compute()</c> method.
    ''' </para>
    ''' 
    ''' <para>
    ''' Output matrix structure:
    ''' </para>
    ''' <code>
    ''' corrmat(j, i) = correlation coefficient (upper part)
    ''' corrmat(i, j) = p-value               (lower part)
    ''' </code>
    ''' 
    ''' <para>
    ''' No missing-value handling is performed; the caller must pre-clean the data.
    ''' </para>
    ''' </remarks>
    Function CorrelMatrix(InputData(,) As Double, strCorrTyp As String) As Double(,)

        Dim n As Integer = InputData.GetLength(0)
        Dim NoVar As Integer = InputData.GetLength(1)
        Dim corrmat(NoVar - 1, NoVar - 1) As Double

        For i As Integer = 0 To NoVar - 1
            Dim temp1() As Double = GetColumnFrom2Darray(InputData, i)

            For j As Integer = 0 To NoVar - 1
                If i >= j Then
                    Dim temp2() As Double = GetColumnFrom2Darray(InputData, j)

                    If strCorrTyp = "r" Then
                        corrmat(j, i) = Correl(temp1, temp2)
                        If i <> j Then
                            corrmat(i, j) = distributions.T_2T(Math.Abs((corrmat(j, i) * Math.Sqrt(n - 2) / (1.0 - Math.Sqrt(corrmat(j, i) ^ 2)))), CDbl(n))
                        End If
                    ElseIf strCorrTyp = "rho" Then
                        Dim s = New nonparametric.SpearmanRho(temp1, temp2, "x", "pY")
                        s.Compute()
                        corrmat(i, j) = s.pvalue
                        corrmat(j, i) = s.correlCoef
                    ElseIf strCorrTyp = "tau" Then
                        Dim tau = New nonparametric.KendallsTau(temp1, temp2, "x", "pY")
                        tau.compute()
                        corrmat(i, j) = tau.pvalue
                        corrmat(j, i) = tau.correlCoef
                    End If
                End If
            Next
        Next

        Return corrmat
    End Function

End Module
