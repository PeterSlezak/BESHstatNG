Option Explicit On

Imports System.Numerics
Imports BESHStatNG.AppInfrastructure
Imports BESHStatNG.DataManagement

Namespace Matrix


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
            Return MatrixArithmeticCore.OuterProduct(mat1, mat2)
        End Function

        Public Function MatrixTrace(a(,) As Double) As Double
            Return MatrixArithmeticCore.Trace(a)
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
            Return MatrixArithmeticCore.Multiply(Matrix1, Matrix2)
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
            Return MatrixArithmeticCore.Multiply(Matrix1, Matrix2)
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
        Public Function MatrixMult(Matrix1(,) As Double, Matrix2() As Double) As Double(,)
            Return MatrixArithmeticCore.Multiply(Matrix1, Matrix2)
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
            Return MatrixArithmeticCore.Multiply(Matrix1, c)
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
        Public Function MatrixMult(Matrix1() As Double, c As Double) As Double()
            Return MatrixArithmeticCore.Multiply(Matrix1, c)
        End Function

        Public Function NegativeVector(x() As Double) As Double()
            Return MatrixArithmeticCore.NegateVector(x)
        End Function

        Public Function MatrixVectorMultiply(a(,) As Double, x() As Double) As Double()
            Return MatrixArithmeticCore.MultiplyVector(a, x)
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
        Public Function DotProduct(a() As Double, b() As Double) As Double
            Return MatrixArithmeticCore.DotProduct(a, b)
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
        ''' MatrixType addition is performed element-wise:
        ''' </para>
        ''' <code>
        ''' C(i, j) = mat1(i, j) + mat2(i, j)
        ''' </code>
        ''' <para>
        ''' Both matrices must have exactly the same dimensions.
        ''' </para>
        ''' </remarks>
        Function M_ADD(mat1(,) As Double, mat2(,) As Double) As Double(,) 'matrix addition
            Return MatrixArithmeticCore.Add(mat1, mat2)
        End Function

        ''' <summary>
        ''' Computes the scalar quadratic form <c>v' * A * v</c>.
        ''' </summary>
        ''' <param name="v">
        ''' The vector used on both sides of the quadratic form. Its length must match
        ''' both dimensions of <paramref name="a"/>.
        ''' </param>
        ''' <param name="a">
        ''' The square matrix in the middle of the quadratic form. The matrix must have
        ''' the same number of rows and columns as the length of <paramref name="v"/>.
        ''' </param>
        ''' <returns>
        ''' The scalar value of <c>v' * A * v</c>. Returns <see cref="Double.NaN"/>
        ''' when either input is <c>Nothing</c> or when the matrix dimensions do not
        ''' match the vector length.
        ''' </returns>
        ''' <remarks>
        ''' This helper is typically used to compute variances of linear estimates,
        ''' contrasts, or predictions, where <paramref name="v"/> is a contrast or
        ''' design row and <paramref name="a"/> is a covariance matrix.
        ''' </remarks>
        Public Function QuadraticForm(v() As Double, a(,) As Double) As Double
            Return MatrixArithmeticCore.QuadraticForm(v, a)
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
            Return MatrixArithmeticCore.Add(mat1, mat2)
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
            Return MatrixArithmeticCore.Add(mat1, mat2)
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
            Return MatrixArithmeticCore.Add(mat1, c)
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
            Return MatrixArithmeticCore.Subtract(mat1, mat2)
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
            Return MatrixArithmeticCore.Subtract(mat1, mat2)
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
            Return MatrixArithmeticCore.Subtract(mat1, c)
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
            Return MatrixArithmeticCore.Divide(mat1, mat2, strTrace)
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
            Return MatrixArithmeticCore.Divide(mat1, mat2, strTrace)
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
            Return MatrixArithmeticCore.Divide(mat1, mat2, strTrace)
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
            Return MatrixArithmeticCore.Divide(mat1, c, strTrace)
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
            Return MatrixArithmeticCore.Divide(mat1, c, strTrace)
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
        '''   <item><description>2 — MatrixType is not positive-definite</description></item>
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
            Return MatrixFactorizationCore.Cholesky(a, iFault, bErrorRaise)
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
            Return MatrixFactorizationCore.CholeskySolve(L, b)
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
            Return MatrixFactorizationCore.CholeskySolve(L, b)
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
            Return MatrixFactorizationCore.CholeskyInverse(L)
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
            Dim coreResult As MatrixFactorizationCore.LUDecompositionResult = MatrixFactorizationCore.LUDecompose(mat, d, iErr)
            Return New LUdecompOutput With {
                    .LUdecomp = coreResult.Factors,
                    .LUindex = coreResult.PivotIndices
                }
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
            Dim coreResult As New MatrixFactorizationCore.LUDecompositionResult With {
                     .Factors = LU.LUdecomp,
                     .PivotIndices = LU.LUindex
                }
            Return MatrixFactorizationCore.LUSolve(coreResult, RighthandSideVector)
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
            Return MatrixFactorizationCore.Determinant(matrix)
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
            Return MatrixArithmeticCore.IdentityMatrix(n)
        End Function

        Public Function VectorNorm(x() As Double) As Double
            Return MatrixArithmeticCore.VectorNorm(x)
        End Function

        Public Function MatrixIsFinite(a(,) As Double) As Boolean
            Return MatrixArithmeticCore.MatrixIsFinite(a)
        End Function

        Public Function VectorIsFinite(values() As Double) As Boolean
            Return MatrixArithmeticCore.VectorIsFinite(values)
        End Function

        Public Function MatrixIsFiniteAndSymmetric(a(,) As Double, tolerance As Double, ByRef message As String) As Boolean
            Return MatrixArithmeticCore.MatrixIsFiniteAndSymmetric(a, tolerance, message)
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
            Return MatrixArithmeticCore.ConstantVector(n, val)
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
            Return MatrixDecompositionCore.InvertMatrix(mat, method, iErr, bPseudInverse)
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
        Public Function pseudoInverse(ByVal A(,) As Double, Optional tol As Double = -1.0) As Double(,)
            Return MatrixDecompositionCore.ComputePseudoInverse(A, tol)
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
            Dim coreResult As MatrixDecompositionCore.SvdResult = MatrixDecompositionCore.DecomposeSvd(matrix)
            Return New SVDoutput With {
                    .U = coreResult.U,
                    .Wvect = coreResult.SingularValues,
                    .Wmat = coreResult.SingularValueMatrix,
                    .V = coreResult.V
                }
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
        ''' MatrixType of independent variables with dimensions (n × m).  
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
            Return MatrixStatisticsCore.FitLinearRegression(y, x, bIntcpt)
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
        Public Function rowFromArray(mat(,) As Double, row As Integer) As Double()
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
            Return ArrayUtilities.Slice(mat, lStart, lEnd)
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
            Return MatrixArithmeticCore.Transpose(mat)
        End Function

        Public Function CloneMatrix(Of T)(mat(,) As T) As T(,)
            If mat Is Nothing Then Return Nothing
            Return CType(mat.Clone(), T(,))
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
            Return MatrixStatisticsCore.FitWeightedLeastSquares(endog, exog, weights)
        End Function

        ''' <summary>
        ''' Stores the result of a QR decomposition.
        ''' </summary>
        ''' <remarks>
        ''' For an input matrix A of size m x n with m >= n:
        ''' <list type="bullet">
        ''' <item><description><c>Q</c> is the thin/economy orthogonal factor of size m x n.</description></item>
        ''' <item><description><c>R</c> is the upper-triangular factor of size n x n.</description></item>
        ''' </list>
        ''' For square inputs, Q and R are both n x n.
        ''' </remarks>
        Public Class QRout
            'QR decomposition output
            Public R(,) As Double
            Public Q(,) As Double
        End Class

        ''' <summary>
        ''' Solves a linear system or least-squares problem from a QR decomposition.
        ''' </summary>
        ''' <param name="qr">
        ''' QR decomposition returned by <see cref="QRdecomp(Double(,), Double)"/>.
        ''' </param>
        ''' <param name="b">
        ''' Right-hand-side matrix of size m x k for a system based on A of size m x n.
        ''' </param>
        ''' <returns>
        ''' Solution matrix x of size n x k.
        ''' </returns>
        ''' <remarks>
        ''' This routine supports:
        ''' <list type="bullet">
        ''' <item>
        ''' <description>
        ''' square full-rank systems, where A is n x n and the solution is exact;
        ''' </description>
        ''' </item>
        ''' <item>
        ''' <description>
        ''' tall full-column-rank systems, where A is m x n with m >= n, producing the
        ''' ordinary least-squares solution that minimizes ||A*x - b||_2.
        ''' </description>
        ''' </item>
        ''' </list>
        ''' 
        ''' It solves:
        ''' <code>
        ''' x = R^(-1) * Q^T * b
        ''' </code>
        ''' </remarks>
        Function QRsolve(qr As QRout, b(,) As Double) As Double(,)
            Dim coreQr As New MatrixDecompositionCore.QrResult With {
                 .Q = qr.Q,
                 .R = qr.R
                }
            Return MatrixDecompositionCore.SolveQr(coreQr, b)
        End Function

        ''' <summary>
        ''' Computes a Householder QR decomposition for a real matrix with rows >= columns.
        ''' </summary>
        ''' <param name="mat">
        ''' Input matrix A of size m x n, with m >= n.
        ''' </param>
        ''' <param name="prec">
        ''' Small positive threshold used to treat a reflector norm or denominator as numerically zero.
        ''' </param>
        ''' <returns>
        ''' A <see cref="QRout"/> object containing:
        ''' <list type="bullet">
        ''' <item>
        ''' <description>
        ''' <c>Q</c>: the thin/economy orthogonal factor of size m x n.
        ''' For square input matrices, Q is n x n.
        ''' </description>
        ''' </item>
        ''' <item>
        ''' <description>
        ''' <c>R</c>: the upper-triangular factor of size n x n.
        ''' </description>
        ''' </item>
        ''' </list>
        ''' The decomposition satisfies:
        ''' <code>
        ''' A = Q * R
        ''' </code>
        ''' with Q having orthonormal columns.
        ''' </returns>
        ''' <remarks>
        ''' This routine supports both:
        ''' <list type="bullet">
        ''' <item><description>square matrices (n x n)</description></item>
        ''' <item><description>tall matrices (m x n, with m > n)</description></item>
        ''' </list>
        ''' It does not support wide matrices with fewer rows than columns.
        ''' 
        ''' For tall matrices, the returned Q is the thin/economy Q, not the full m x m orthogonal matrix.
        ''' This is the preferred form for least-squares problems and for QR-preconditioning in SVD-based solvers.
        ''' </remarks>
        ''' <exception cref="System.ArgumentNullException">
        ''' Thrown when <paramref name="mat"/> is Nothing.
        ''' </exception>
        ''' <exception cref="System.ArgumentException">
        ''' Thrown when the input matrix is empty or has fewer rows than columns.
        ''' </exception>
        Function QRdecomp(mat(,) As Double, Optional prec As Double = 0.000000000001) As QRout
            Dim coreResult As MatrixDecompositionCore.QrResult = MatrixDecompositionCore.DecomposeQr(mat, prec)
            Return New QRout With {
                    .Q = coreResult.Q,
                    .R = coreResult.R
                }
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
                CoreServices.Errors.LogAndThrow(New ArgumentException("Invalid input array dimensions"))
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
                CoreServices.Errors.LogAndThrow(New ArgumentException("Invalid input array dimensions"))
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
            Return ArrayUtilities.GetColumn(x, nCol)
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
        Public Function MatCovar(mat(,) As Double) As Double(,)
            Return MatrixStatisticsCore.SampleCovariance(mat)
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
            Return MatrixStatisticsCore.DoubleCenter(mat)
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
            Dim result As MatrixStatisticsCore.EigenResult = MatrixStatisticsCore.EigenJk(m, maxiter, eps)
            Return (result.Eigenvalues, result.Eigenvectors)
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
        ''' For Pearson correlation (<c>strCorrTyp = "r"</c>), the host-neutral Core implementation
        ''' uses the shared correlation and Student-t distribution routines to compute the coefficient and significance.
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
        Public Function CorrelMatrix(InputData(,) As Double, strCorrTyp As String) As Double(,)
            Return CorrelationMatrixCore.Compute(InputData, strCorrTyp)
        End Function

    End Module

End Namespace
