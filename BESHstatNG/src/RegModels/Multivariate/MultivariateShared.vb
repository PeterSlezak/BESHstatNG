Option Explicit On
Option Strict On
Imports BESHStatNG.AppInfrastructure

Namespace Multivariate

    Friend Module MultivariateShared

        '''''' <summary>
        '''''' Standardizes a vector to mean 0 and sample SD 1.
        '''''' </summary>
        '''''' <param name="vector">Input data vector (length n).</param>
        '''''' <returns>Standardized vector z_i = (x_i − mean(x))/sd(x).</returns>
        '''''' <exception cref="System.ArgumentException">Thrown if the standard deviation is zero or not finite.</exception>
        '''''' <remarks>
        '''''' <para>Uses <see cref="StatFunc.stDev"/> (sample SD with divisor n−1).</para>
        '''''' </remarks>
        '''''' <seealso cref="StatFunc.stDev" />
        Friend Function Standardize(vector() As Double,
                                    Optional errorMessage As String = "Cannot standardize: SD is zero/invalid.") As Double()
            Dim out(vector.Length - 1) As Double
            Dim mean As Double = vector.Average()
            Dim sd As Double = stDev(vector)

            If sd <= 0.0 OrElse Double.IsNaN(sd) OrElse Double.IsInfinity(sd) Then CoreServices.Errors.LogAndThrow(New ArgumentException(errorMessage))

            For i As Integer = 0 To vector.Length - 1
                out(i) = (vector(i) - mean) / sd
            Next
            Return out
        End Function

        '''''' <summary>
        '''''' Centers a vector by subtracting its mean.
        '''''' </summary>
        '''''' <param name="vector">Input data vector (length n).</param>
        '''''' <returns>Centered vector x_i − mean(x).</returns>
        Friend Function Center(vector() As Double) As Double()
            Dim out(vector.Length - 1) As Double
            Dim mean As Double = vector.Average()
            For i As Integer = 0 To vector.Length - 1
                out(i) = vector(i) - mean
            Next
            Return out
        End Function

        '''''' <summary>
        '''''' Sorts eigenvalues descending and reorders the corresponding eigenvector columns.
        '''''' </summary>
        '''''' <param name="vals">Eigenvalues array (length p).</param>
        '''''' <param name="vecs">Eigenvector matrix (p × p), where columns align with vals.</param>
        '''''' <returns>A tuple (sortedVals, sortedVecs) with consistent ordering.</returns>
        '''''' <remarks>
        '''''' <para>Sorting is required for correct explained-variance calculations and component selection.</para>
        '''''' </remarks>
        Friend Function SortEigenpairsDescending(vals() As Double,
                                                 vecs(,) As Double) As (Double(), Double(,))
            Dim order = Enumerable.Range(0, vals.Length).OrderByDescending(Function(i) vals(i)).ToArray()
            Dim vals2(vals.Length - 1) As Double
            Dim vecs2(vecs.GetLength(0) - 1, vecs.GetLength(1) - 1) As Double

            For newJ As Integer = 0 To order.Length - 1
                Dim oldJ As Integer = order(newJ)
                vals2(newJ) = vals(oldJ)
                For i As Integer = 0 To vecs.GetLength(0) - 1
                    vecs2(i, newJ) = vecs(i, oldJ)
                Next
            Next

            Return (vals2, vecs2)
        End Function

        ''' <summary>
        ''' Converts leading eigenpairs into a loading matrix by scaling each eigenvector with the square root of its eigenvalue.
        ''' </summary>
        ''' <param name="vals">Eigenvalues in descending order.</param>
        ''' <param name="vecs">Eigenvector matrix whose columns align with <paramref name="vals"/>.</param>
        ''' <param name="nFactors">Number of leading columns to convert into loadings.</param>
        ''' <returns>A <c>p × m</c> loading matrix.</returns>
        Friend Function BuildLoadingsFromEigenpairs(vals() As Double,
                                                    vecs(,) As Double,
                                                    nFactors As Integer) As Double(,)
            Dim out(vecs.GetLength(0) - 1, nFactors - 1) As Double
            For j As Integer = 0 To nFactors - 1
                Dim root As Double = Math.Sqrt(Math.Max(vals(j), 0.0))
                For i As Integer = 0 To vecs.GetLength(0) - 1
                    out(i, j) = vecs(i, j) * root
                Next
            Next
            Return out
        End Function

        ''' <summary>
        ''' Returns the diagonal of a square matrix.
        ''' </summary>
        Friend Function DiagonalValues(mat(,) As Double) As Double()
            Dim n As Integer = Math.Min(mat.GetLength(0), mat.GetLength(1))
            Dim out(n - 1) As Double
            For i As Integer = 0 To n - 1
                out(i) = mat(i, i)
            Next
            Return out
        End Function

        ''' <summary>
        ''' Forms a diagonal matrix from a vector.
        ''' </summary>
        Friend Function DiagonalMatrix(diag() As Double) As Double(,)
            Return Matrix.DiagMatFromVector(diag)
        End Function

        ''' <summary>
        ''' Restricts a scalar to the closed interval [<paramref name="lower"/>, <paramref name="upper"/>].
        ''' </summary>
        Friend Function Clamp(value As Double, lower As Double, upper As Double) As Double
            If value < lower Then Return lower
            If value > upper Then Return upper
            Return value
        End Function

        ''' <summary>
        ''' Computes the maximum absolute element-wise difference between two vectors.
        ''' </summary>
        Friend Function MaxAbsDifference(a() As Double, b() As Double) As Double
            Dim out As Double = 0.0
            For i As Integer = 0 To a.Length - 1
                out = Math.Max(out, Math.Abs(a(i) - b(i)))
            Next
            Return out
        End Function

        ''' <summary>
        ''' Computes a numerically robust inverse by trying a preferred factorization first and falling back to more general alternatives.
        ''' </summary>
        ''' <param name="mat">Matrix to invert.</param>
        ''' <param name="preferCholesky">If <c>True</c>, the routine tries Cholesky inversion before LU inversion.</param>
        ''' <returns>An inverse or pseudoinverse of <paramref name="mat"/>.</returns>
        Friend Function SafeInverse(mat(,) As Double,
                                    Optional preferCholesky As Boolean = True) As Double(,)
            Try
                If preferCholesky Then
                    Return Matrix.MatInv(mat, method:="CHOL")
                Else
                    Return Matrix.MatInv(mat, method:="LU")
                End If
            Catch
                Try
                    Return Matrix.MatInv(mat, method:="LU")
                Catch
                    Return Matrix.pseudoInverse(mat)
                End Try
            End Try
        End Function

    End Module

End Namespace
