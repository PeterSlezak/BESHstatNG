Option Explicit On
Imports System.Text
Imports Microsoft.VisualStudio.TestTools.UnitTesting
Imports BESHStatNG
Imports BESHStatNG.Matrix

<TestClass()>
Public Class MatrixMult_Tests

    '<ClassInitialize>
    'Public Sub TestFixtureSetup()
    '    pTest2DArray1 = New Double(,) {{-2.0#, 3.0#, 4.0#}, {-2.0#, 3.0#, 4.0#}}
    'End Sub

    <TestCategory("MatrixForUdfs")>
    <TestMethod()>
    <DataRow({-2.0, 3.0, 4.0}, 2.0, {-4.0, 6.0, 8.0})>
    <DataRow({2.0, -3.0, 4.0}, 0.0, {0.0, 0.0, 0.0})>
    Public Sub MatrixMult_Test1Dc_equal(m1() As Double, x As Double, out() As Double)
        Dim res() As Double = MatrixMult(m1, x)
        For i = 0 To m1.Length - 1
            Assert.AreEqual(out(i), res(i))
        Next
    End Sub

    <TestCategory("MatrixForUdfs")>
    <TestMethod()>
    <DataRow({1.5}, 2.5, {0.0})>
    Public Sub MatrixMult_Test1Dc_notequal(m1() As Double, x As Double, out() As Double)
        Dim res() As Double = MatrixMult(m1, x)
        For i = 0 To m1.Length - 1
            Assert.AreNotEqual(out(i), res(i))
        Next
    End Sub

    <TestCategory("MatrixForUdfs")>
    <TestMethod()>
    Public Sub MatrixMult_Test2Dc_equal()
        'arrange
        Dim m1(,) As Double = New Double(,) {{-2.0, 3.0, 4.0}, {-1.0, 2.0, 3.0}}
        Dim x As Double = 2.0
        Dim out(,) As Double = New Double(,) {{-4.0, 6.0, 8.0}, {-2.0, 4.0, 6.0}}

        'act
        Dim res(,) As Double = MatrixMult(m1, x)

        'assess
        For i = 0 To UBound(m1, 1)
            For j = 0 To UBound(m1, 2)
                Assert.AreEqual(out(i, j), res(i, j))
            Next
        Next
    End Sub

    <TestCategory("MatrixForUdfs")>
    <TestMethod()>
    Public Sub MatrixMult_Test2Dc_notequal()
        'arrange
        Dim m1(,) As Double = New Double(,) {{-2.0, 3.0, 4.0}, {-1.0, 2.0, 3.0}}
        Dim x As Double = 1.0#
        Dim out(,) As Double = New Double(,) {{-4.0, 7.0, 8.0}, {-2.0, 4.0, 6.0}}

        'act
        Dim res(,) As Double = MatrixMult(m1, x)

        'assess
        For i = 0 To UBound(m1, 1)
            For j = 0 To UBound(m1, 2)
                Assert.AreNotEqual(out(i, j), res(i, j))
            Next
        Next
    End Sub

    <TestCategory("MatrixForUdfs")>
    <TestMethod()>
    Public Sub MatrixMult_Test2D1D_equal()
        'arrange
        Dim m1(,) As Double = New Double(,) {{-2.0, 3.0, 4.0}, {-1.0, 2.0, 3.0}}
        Dim m2() As Double = New Double() {3.0, 4.0, 5.0}
        Dim out(,) As Double = New Double(,) {{26.0}, {20.0}}

        'act
        Dim res(,) As Double = MatrixMult(m1, m2)

        'assess
        For i = 0 To UBound(out, 1)
            For j = 0 To UBound(out, 2)
                Assert.AreEqual(out(i, j), res(i, j))
            Next
        Next
    End Sub

    <TestCategory("MatrixForUdfs")>
    <TestMethod()>
    Public Sub MatrixMult_Test2D1D_notequal()
        'arrange
        Dim m1(,) As Double = New Double(,) {{-2.0, 3.0, 4.0}, {-1.0, 2.0, 3.0}}
        Dim m2() As Double = New Double() {1.0, 2.0, 1.0}
        Dim out(,) As Double = New Double(,) {{26.0}, {20.0}}

        'act
        Dim res(,) As Double = MatrixMult(m1, m2)

        'assess
        For i = 0 To UBound(out, 1)
            For j = 0 To UBound(out, 2)
                Assert.AreNotEqual(out(i, j), res(i, j))
            Next
        Next
    End Sub

    <TestCategory("MatrixForUdfs")>
    <TestMethod()>
    Public Sub MatrixMult_Test2D2D_equal()
        'arrange
        Dim m1(,) As Double = New Double(,) {{-2.0, 3.0, 4.0}, {-1.0, 2.0, 3.0}}
        Dim m2(,) As Double = New Double(,) {{3.0, -2.0}, {4.0, 4.0}, {5.0, 0}}
        Dim out(,) As Double = New Double(,) {{26.0, 16.0}, {20.0, 10.0}}

        'act
        Dim res(,) As Double = MatrixMult(m1, m2)

        'assess
        For i = 0 To UBound(out, 1)
            For j = 0 To UBound(out, 2)
                Assert.AreEqual(out(i, j), res(i, j))
            Next
        Next
    End Sub

End Class

<TestClass()>
Friend Module MatrixTestHelpers
    Public Const TOL As Double = 0.0000000001

    Public Sub AssertMatrixAlmostEqual(expected As Double(,), actual As Double(,), Optional tol As Double = TOL)
        Assert.AreEqual(UBound(expected, 1), UBound(actual, 1), "RowUdfs count differs.")
        Assert.AreEqual(UBound(expected, 2), UBound(actual, 2), "Column count differs.")
        For i = 0 To UBound(expected, 1)
            For j = 0 To UBound(expected, 2)
                Assert.AreEqual(expected(i, j), actual(i, j), tol, $"Mismatch at ({i},{j})")
            Next
        Next
    End Sub

    Public Sub AssertVectorAlmostEqual(expected As Double(), actual As Double(), Optional tol As Double = TOL)
        Assert.AreEqual(UBound(expected), UBound(actual), "Vector length differs.")
        For i = 0 To UBound(expected)
            Assert.AreEqual(expected(i), actual(i), tol, $"Mismatch at index {i}")
        Next
    End Sub

    Public Function Eye(n As Integer) As Double(,)
        Return IdentityMat(n)
    End Function

End Module

<TestClass()>
Public Class Matrix_Basic_Operations_Tests

    <TestMethod()>
    Public Sub M_OUTERPRODUCT_double()
        Dim a() As Double = {1.5, -2.0}
        Dim b() As Double = {3.0, 4.0, -1.0}
        Dim got = M_OUTERPRODUCT(a, b)
        Dim expected(,) As Double = {
            {4.5, 6.0, -1.5},
            {-6.0, -8.0, 2.0}
        }
        AssertMatrixAlmostEqual(expected, got)
    End Sub

    <TestMethod()>
    Public Sub DotProduct_basic()
        Dim a() As Double = {1.0, 2.0, 3.0}
        Dim b() As Double = {4.0, -1.0, 0.5}
        Dim expected As Double = 1.0 * 4.0 + 2.0 * (-1.0) + 3.0 * 0.5
        Assert.AreEqual(expected, DotProduct(a, b), 0.0)
    End Sub

    <TestMethod()>
    Public Sub Transpose_basic()
        Dim m(,) As Double = {{1, 2, 3}, {4, 5, 6}}
        Dim got = trans(m)
        Dim expected(,) As Double = {{1, 4}, {2, 5}, {3, 6}}
        AssertMatrixAlmostEqual(expected, got, 0.0)
    End Sub

    <TestMethod()>
    Public Sub MatrixMult_matrix_matrix()
        Dim a(,) As Double = {{2, 1}, {1, 3}}
        Dim b(,) As Double = {{1, 2}, {3, 4}}
        Dim got = MatrixMult(a, b)
        Dim expected(,) As Double = {{2 * 1 + 1 * 3, 2 * 2 + 1 * 4},
                                     {1 * 1 + 3 * 3, 1 * 2 + 3 * 4}}
        AssertMatrixAlmostEqual(expected, got, 0.0)
    End Sub

    <TestMethod()>
    Public Sub MatrixMult_matrix_vector()
        Dim a(,) As Double = {{2, 1}, {1, 3}}
        Dim v() As Double = {1, 2}
        Dim got(,) = MatrixMult(a, v)
        Dim expected(,) As Double = {{2 * 1 + 1 * 2}, {1 * 1 + 3 * 2}}

        AssertMatrixAlmostEqual(expected, got, 0.0)
    End Sub

    <TestMethod()>
    Public Sub MatrixMult_vector_matrix()
        Dim v() As Double = {1, 2}
        Dim a(,) As Double = {{2, 1}, {1, 3}}
        Dim got = MatrixMult(v, a)
        ' [1,2] * [[2,1],[1,3]] = [1*2+2*1, 1*1+2*3] = [4,7]
        Dim expected(,) As Double = {{4, 7}}
        AssertMatrixAlmostEqual(expected, got, 0.0)
    End Sub

    <TestMethod()>
    Public Sub M_ADD_M_SUB_M_DIV_scalar_vector()
        Dim v() As Double = {1, 2, 3}
        AssertVectorAlmostEqual({2, 3, 4}, M_ADD(v, 1.0), 0.0)
        AssertVectorAlmostEqual({0, 1, 2}, M_SUB(v, 1.0), 0.0)
        AssertVectorAlmostEqual({0.5, 1.0, 1.5}, M_DIV(v, 2.0), 0.0)
    End Sub

    <TestMethod()>
    Public Sub M_ADD_M_SUB_M_DIV_matrix_matrix()
        Dim a(,) As Double = {{1, 2}, {3, 4}}
        Dim b(,) As Double = {{10, 20}, {30, 40}}

        Dim add = M_ADD(a, b)
        Dim su = M_SUB(b, a)
        Dim div = M_DIV(b, a)

        AssertMatrixAlmostEqual({{11, 22}, {33, 44}}, add, 0.0)
        AssertMatrixAlmostEqual({{9, 18}, {27, 36}}, su, 0.0)
        AssertMatrixAlmostEqual({{10.0, 10.0}, {10.0, 10.0}}, div, 0.0)
    End Sub

    <TestMethod()>
    Public Sub SubsetArray_basic()
        Dim a() As Integer = {10, 20, 30, 40, 50}
        CollectionAssert.AreEqual(New Integer() {20, 30, 40}, SubsetArray(a, 1, 3))
        CollectionAssert.AreEqual(New Integer() {10, 20, 30, 40, 50}, SubsetArray(a))
        CollectionAssert.AreEqual(New Integer() {50}, SubsetArray(a, 99, -1)) ' start> end coerces to end
    End Sub

End Class

<TestClass()>
Public Class Matrix_Decomposition_Tests

    <TestMethod()>
    Public Sub IdentityMat_and_IdentityVect()
        Dim I = IdentityMat(1)
        Dim expectedI(,) As Double = {{1.0, 0.0},
                                 {0.0, 1.0}}
        AssertMatrixAlmostEqual(expectedI, I, 0.0)

        Dim v = IdentityVect(1)
        Dim expectedV() As Double = {1.0, 1.0}
        AssertVectorAlmostEqual(expectedV, v, 0.0)
    End Sub


    <TestMethod()>
    Public Sub MDeterm_matches_known()
        Dim a(,) As Double = {{2, 1}, {1, 3}} ' det = 5
        Assert.AreEqual(5.0, MDeterm(a), 0.000000000001)
    End Sub

    <TestMethod()>
    Public Sub MDeterm_singular_returns_zero()
        Dim a(,) As Double = {{1, 2}, {2, 4}}
        Assert.AreEqual(0.0, MDeterm(a), 0.000000000001)
    End Sub

    <TestMethod()>
    Public Sub MatInv_LU_inverts_matrix()
        Dim a(,) As Double = {{2, 1}, {1, 3}}
        Dim ierr As Integer = 0
        Dim inv = MatInv(a, "LU", ierr)
        Assert.AreEqual(0, ierr)

        Dim expectedInv(,) As Double = {{0.6, -0.2}, {-0.2, 0.4}}
        AssertMatrixAlmostEqual(expectedInv, inv, 0.0000000001)

        Dim prod = MatrixMult(a, inv)
        AssertMatrixAlmostEqual(IdentityMat(UBound(a, 1)), prod, 0.000000001)
    End Sub

    <TestMethod()>
    Public Sub LU_solve_matches_known_solution()
        Dim a(,) As Double = {{2, 1}, {1, 3}}
        Dim d As Double = 1.0
        Dim ierr As Integer = 0
        Dim lu = LUdecomp(a, d, ierr)
        Assert.AreEqual(0, ierr)

        Dim b() As Double = {1, 2}
        Dim x = LUbacksub(lu, b)
        AssertVectorAlmostEqual({0.2, 0.6}, x, 0.0000000001)
    End Sub

    <TestMethod()>
    Public Sub Cholesky_and_CholSolve_reconstruct_and_solve()
        Dim a(,) As Double = {{2, 1}, {1, 3}}
        Dim ierr As Integer = 0
        Dim L = Cholesky(a, ierr)
        Assert.AreEqual(0, ierr)

        Dim recon = MatrixMult(L, trans(L))
        AssertMatrixAlmostEqual(a, recon, 0.000000001)

        Dim b() As Double = {1, 2}
        Dim x = CholSolve(L, b)
        AssertVectorAlmostEqual({0.2, 0.6}, x, 0.000000001)

        Dim inv = CholInv(L)
        Dim prod = MatrixMult(a, inv)
        AssertMatrixAlmostEqual(IdentityMat(UBound(a, 1)), prod, 0.00000001)
    End Sub

End Class

<TestClass()>
Public Class Matrix_Factorization_Tests

    <TestMethod()>
    Public Sub QRdecomp_reconstructs_matrix()
        Dim a(,) As Double = {{2, 1}, {1, 3}}
        Dim qr = QRdecomp(a)
        Dim recon = MatrixMult(qr.Q, qr.R)
        AssertMatrixAlmostEqual(a, recon, 0.000000001)

        ' Q should be orthonormal: Q^T Q = I
        Dim qtq = MatrixMult(trans(qr.Q), qr.Q)
        AssertMatrixAlmostEqual(IdentityMat(UBound(qtq, 1)), qtq, 0.000000001)
    End Sub

    <TestMethod()>
    Public Sub QRsolve_solves_system()
        Dim a(,) As Double = {{2, 1}, {1, 3}}
        Dim b(,) As Double = {{1.0}, {2.0}}
        Dim qr = QRdecomp(a)
        Dim x = QRsolve(qr, b)
        Assert.AreEqual(0.2, x(0, 0), 0.000000001)
        Assert.AreEqual(0.6, x(1, 0), 0.000000001)
    End Sub

    <TestMethod()>
    Public Sub SVD_decomp_reconstructs_matrix()
        Dim a(,) As Double = {{2, 1}, {1, 3}}
        Dim svd = SVD_decomp(a)
        Dim recon = MatrixMult(MatrixMult(svd.U, svd.Wmat), trans(svd.V))
        AssertMatrixAlmostEqual(a, recon, 0.0000001) ' SVD tolerance looser
    End Sub

    <TestMethod()>
    Public Sub PseudoInverse_satisfies_penrose_property_A_Apinv_A()
        Dim A(,) As Double = {{1.0, 2.0, 3.0},
                              {4.0, 5.0, 6.0}}
        Dim Ap(,) As Double = pseudoInverse(A)
        Dim AApA(,) As Double = MatrixMult(MatrixMult(A, Ap), A)

        AssertMatrixAlmostEqual(A, AApA, 0.00000001)
    End Sub

    <TestMethod>
    Public Sub PseudoInverse_full_column_rank_satisfies_ApinvA_equals_I()
        ' Full column rank (3x2) matrix: rank = 2
        Dim A(,) As Double = {
        {1.0, 0.0},
        {0.0, 1.0},
        {1.0, 1.0}}

        Dim Ap(,) As Double = pseudoInverse(A)

        ' ApA should be the 2x2 identity when A has full column rank
        Dim ApA(,) As Double = MatrixMult(Ap, A)

        Dim I(,) As Double = IdentityMat(UBound(A, 2)) ' UBound=1 => 2x2 identity
        AssertMatrixAlmostEqual(I, ApA, 0.00000001)
    End Sub


    <TestMethod()>
    Public Sub EIGEN_JK_diagonal_matrix()
        Dim a(,) As Double = {{2.0, 0.0}, {0.0, 3.0}}
        Dim ei = EIGEN_JK(a, maxiter:=5, eps:=0.000000000001)
        ' ei(j,0) holds non-negative values (column norms after orthogonalization).
        ' For a diagonal matrix, these equal the diagonal entries.
        Dim vals As Double() = ei.item1
        Array.Sort(vals)
        Assert.AreEqual(2.0, vals(0), 0.000000001)
        Assert.AreEqual(3.0, vals(1), 0.000000001)

        ' Eigenvectors should be orthonormal-ish: columns 1..n
        Dim evecs(,) As Double = ei.item2
        Dim vt_v = MatrixMult(trans(evecs), evecs)
        AssertMatrixAlmostEqual(IdentityMat(UBound(vt_v, 1)), vt_v, 0.000000001)
    End Sub

End Class

<TestClass()>
Public Class Matrix_Array_Utilities_Tests

    <TestMethod()>
    Public Sub Stack_and_column_helpers()
        Dim a1(,) As Integer = {{1, 2}, {3, 4}}
        Dim a2(,) As Integer = {{5, 6}, {7, 8}}

        ' VerticalStackArrays appends columns (requires same number of rows)
        Dim v = VerticalStackArrays(a1, a2)
        Dim expectedV(,) As Integer = {{1, 2, 5, 6}, {3, 4, 7, 8}}
        CollectionAssert.AreEqual(expectedV, v)

        ' HorizontalStackArrays appends rows (requires same number of columns)
        Dim h = HorizontalStackArrays(a1, a2)
        Dim expectedH(,) As Integer = {{1, 2}, {3, 4}, {5, 6}, {7, 8}}
        CollectionAssert.AreEqual(expectedH, h)

        Dim col = GetColumnFrom2Darray(New Double(,) {{1, 2}, {3, 4}, {5, 6}}, 1)
        AssertVectorAlmostEqual({2, 4, 6}, col, 0.0)
    End Sub

    <TestMethod()>
    Public Sub Array_conversions_handle_Nothing()
        Dim obj() As Object = {1, Nothing, 3.5}
        Dim dbl = Array2dblArray(obj)
        AssertVectorAlmostEqual({1.0, 0.0, 3.5}, dbl, 0.0)

        Dim ints = Array2intArray(obj)
        CollectionAssert.AreEqual(New Integer() {1, 0, 4}, ints) ' Convert.ToInt32("3.5") rounds to 4
    End Sub

    <TestMethod()>
    Public Sub Array2objArray_roundtrip()
        Dim d(,) As Double = {{1.25, -2.0}, {0.0, 3.5}}
        Dim o = Array2objArray(d)
        Assert.AreEqual(1.25, CDbl(o(0, 0)), 0.0)
        Assert.AreEqual(-2.0, CDbl(o(0, 1)), 0.0)
        Assert.AreEqual(3.5, CDbl(o(1, 1)), 0.0)
    End Sub

End Class

<TestClass()>
Public Class Matrix_Statistics_Tests

    <TestMethod()>
    Public Sub MatCovar_matches_known()
        Dim data(,) As Double = {{1, 2}, {2, 0}, {3, 1}} ' 3x2
        Dim cov = MatCovar(data)
        Dim expected(,) As Double = {{1.0, -0.5}, {-0.5, 1.0}}
        AssertMatrixAlmostEqual(expected, cov, 0.000000000001)
    End Sub

    <TestMethod()>
    Public Sub MatDoubleCenter_matches_definition()
        Dim a(,) As Double = {{1.0, 2.0}, {3.0, 4.0}}
        Dim dc = MatDoubleCenter(a)

        ' definition: a_ij - rowMean_i - colMean_j + totalMean
        Dim row0 As Double = (1.0 + 2.0) / 2.0
        Dim row1 As Double = (3.0 + 4.0) / 2.0
        Dim col0 As Double = (1.0 + 3.0) / 2.0
        Dim col1 As Double = (2.0 + 4.0) / 2.0
        Dim tot As Double = (1.0 + 2.0 + 3.0 + 4.0) / 4.0

        Dim expected(,) As Double = {
            {1.0 - row0 - col0 + tot, 2.0 - row0 - col1 + tot},
            {3.0 - row1 - col0 + tot, 4.0 - row1 - col1 + tot}
        }
        AssertMatrixAlmostEqual(expected, dc, 0.000000000001)
    End Sub

    <TestMethod()>
    Public Sub DiagMatFromVector_basic()
        Dim v() As Double = {1, 2, 3}
        Dim d = DiagMatFromVector(v)
        Dim expected(,) As Double = {{1, 0, 0}, {0, 2, 0}, {0, 0, 3}}
        AssertMatrixAlmostEqual(expected, d, 0.0)
    End Sub

End Class
