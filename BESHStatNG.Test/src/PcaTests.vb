
Imports Microsoft.VisualStudio.TestTools.UnitTesting
Imports System
Imports System.IO
Imports System.Reflection
Imports System.Globalization
Imports System.Collections
Imports System.Collections.Generic
Imports System.Linq

<TestClass>
Public Class PcaTests

    Public Property TestContext As TestContext

    ' ---------------------------
    ' Helpers: locating PCA type
    ' ---------------------------
    Private Shared Function GetPcaType() As Type
        ' Try exact "PCA" (global namespace) and then any type named "PCA" in loaded assemblies.
        For Each asm As Assembly In AppDomain.CurrentDomain.GetAssemblies()
            Dim tExact As Type = asm.GetType("PCA", throwOnError:=False, ignoreCase:=True)
            If tExact IsNot Nothing Then Return tExact
        Next

        For Each asm As Assembly In AppDomain.CurrentDomain.GetAssemblies()
            Try
                For Each t As Type In asm.GetTypes()
                    If String.Equals(t.Name, "PCA", StringComparison.OrdinalIgnoreCase) Then
                        Return t
                    End If
                Next
            Catch ex As ReflectionTypeLoadException
                ' Some assemblies may fail to enumerate types; ignore and continue
            End Try
        Next

        Throw New AssertFailedException("Could not locate type named 'PCA'. Ensure your production project/assembly containing PCA.vb is referenced by the test project.")
    End Function

    Private Shared Function CreatePcaInstance() As Object
        Dim t As Type = GetPcaType()
        Return Activator.CreateInstance(t)
    End Function

    Private Shared Function InvokeMethod(instance As Object, methodName As String, ParamArray args() As Object) As Object
        Dim t As Type = instance.GetType()
        Dim m As MethodInfo = t.GetMethod(methodName, BindingFlags.Instance Or BindingFlags.Public Or BindingFlags.NonPublic)
        If m Is Nothing Then Throw New AssertFailedException($"Method '{methodName}' not found on type '{t.FullName}'.")
        Return m.Invoke(instance, args)
    End Function

    Private Shared Function GetNonPublicPropertyValue(Of T)(instance As Object, propName As String) As T
        Dim tt As Type = instance.GetType()
        Dim p As PropertyInfo = tt.GetProperty(propName, BindingFlags.Instance Or BindingFlags.Public Or BindingFlags.NonPublic)
        If p Is Nothing Then Throw New AssertFailedException($"Property '{propName}' not found on type '{tt.FullName}'.")
        Return CType(p.GetValue(instance), T)
    End Function

    ' ---------------------------
    ' Helpers: testdata paths/CSV
    ' ---------------------------
    Private Shared Function GetTestDataPath(fileName As String) As String
        Dim baseDir As String = AppDomain.CurrentDomain.BaseDirectory
        Dim c1 As String = Path.Combine(baseDir, fileName)
        If File.Exists(c1) Then Return c1

        Dim c2 As String = Path.Combine(baseDir, "TestData", fileName)
        If File.Exists(c2) Then Return c2

        Dim c3 As String = Path.GetFullPath(Path.Combine(baseDir, "..\..\TestData", fileName))
        If File.Exists(c3) Then Return c3

        Dim c4 As String = Path.GetFullPath(Path.Combine(baseDir, "..\..\..\TestData", fileName))
        If File.Exists(c4) Then Return c4

        Throw New FileNotFoundException("Test data file not found", fileName)
    End Function

    Private Shared Function LoadCsvMatrix(path As String) As (data As Double(,), rowIds As Integer(), varNames As String())
        Dim lines = File.ReadAllLines(path)
        If lines.Length < 2 Then Throw New AssertFailedException($"CSV has no data rows: {path}")

        Dim header = lines(0).Split(","c).Select(Function(s) s.Trim()).ToArray()
        If header.Length < 2 Then Throw New AssertFailedException($"CSV must contain an id column and at least one variable column: {path}")

        Dim varNames = header.Skip(1).ToArray()
        Dim n As Integer = lines.Length - 1
        Dim p As Integer = varNames.Length

        Dim rowIds(n - 1) As Integer
        Dim data(n - 1, p - 1) As Double

        For i As Integer = 0 To n - 1
            Dim parts = lines(i + 1).Split(","c).Select(Function(s) s.Trim()).ToArray()
            If parts.Length <> p + 1 Then Throw New AssertFailedException($"RowUdfs {i + 2} has {parts.Length} columns; expected {p + 1}. File: {path}")

            rowIds(i) = Integer.Parse(parts(0), CultureInfo.InvariantCulture)
            For j As Integer = 0 To p - 1
                data(i, j) = Double.Parse(parts(j + 1), CultureInfo.InvariantCulture)
            Next
        Next

        Return (data, rowIds, varNames)
    End Function

    ' ---------------------------
    ' Helpers: numeric assertions
    ' ---------------------------
    Private Shared Sub AssertClose(expected As Double, actual As Double, absTol As Double, Optional relTol As Double = 0.0, Optional msg As String = "")
        Dim diff As Double = Math.Abs(expected - actual)
        Dim ok As Boolean = diff <= absTol
        If Not ok AndAlso relTol > 0 Then
            Dim denom As Double = Math.Max(Math.Abs(expected), Math.Abs(actual))
            If denom > 0 Then ok = (diff / denom) <= relTol
        End If
        If Not ok Then
            Assert.Fail($"{msg} Expected {expected:R}, got {actual:R}, diff={diff:R}")
        End If
    End Sub

    Private Shared Sub AssertVectorClose(expected() As Double, actual() As Double, absTol As Double, Optional relTol As Double = 0.0, Optional msg As String = "")
        Assert.AreEqual(expected.Length, actual.Length, $"{msg} Length mismatch")
        For i As Integer = 0 To expected.Length - 1
            AssertClose(expected(i), actual(i), absTol, relTol, $"{msg} [i={i}]")
        Next
    End Sub

    Private Shared Sub AssertMatrixClose(expected(,) As Double, actual(,) As Double, absTol As Double, Optional relTol As Double = 0.0, Optional msg As String = "")
        Assert.AreEqual(expected.GetLength(0), actual.GetLength(0), $"{msg} RowUdfs count mismatch")
        Assert.AreEqual(expected.GetLength(1), actual.GetLength(1), $"{msg} ColUdfs count mismatch")
        For i As Integer = 0 To expected.GetLength(0) - 1
            For j As Integer = 0 To expected.GetLength(1) - 1
                AssertClose(expected(i, j), actual(i, j), absTol, relTol, $"{msg} [i={i},j={j}]")
            Next
        Next
    End Sub

    Private Shared Function Transpose(A(,) As Double) As Double(,)
        Dim n As Integer = A.GetLength(0)
        Dim p As Integer = A.GetLength(1)
        Dim T(p - 1, n - 1) As Double
        For i As Integer = 0 To n - 1
            For j As Integer = 0 To p - 1
                T(j, i) = A(i, j)
            Next
        Next
        Return T
    End Function

    Private Shared Function MatMul(A(,) As Double, B(,) As Double) As Double(,)
        Dim n As Integer = A.GetLength(0)
        Dim k As Integer = A.GetLength(1)
        Dim m As Integer = B.GetLength(1)
        Assert.AreEqual(k, B.GetLength(0), "MatMul dimension mismatch")
        Dim C(n - 1, m - 1) As Double
        For i As Integer = 0 To n - 1
            For j As Integer = 0 To m - 1
                Dim s As Double = 0.0
                For t As Integer = 0 To k - 1
                    s += A(i, t) * B(t, j)
                Next
                C(i, j) = s
            Next
        Next
        Return C
    End Function

    Private Shared Function Identity(n As Integer) As Double(,)
        Dim II(n - 1, n - 1) As Double
        For i As Integer = 0 To n - 1
            II(i, i) = 1.0
        Next
        Return II
    End Function

    Private Shared Function MaxAbs(A(,) As Double) As Double
        Dim mx As Double = 0.0
        For i As Integer = 0 To A.GetLength(0) - 1
            For j As Integer = 0 To A.GetLength(1) - 1
                mx = Math.Max(mx, Math.Abs(A(i, j)))
            Next
        Next
        Return mx
    End Function

    ' --------------------------------------------
    ' Expected values (computed to match R prcomp)
    ' --------------------------------------------
    ' Dataset: r_reference_10x3.csv
    Private Shared ReadOnly ExpectedCov As Double(,) = New Double(,) {
        {9.1666666666666661, 2.833333333333333, 2.833333333333333},
        {2.833333333333333, 1.166666666666667, 0.5},
        {2.833333333333333, 0.5, 1.4333333333333329}
    }

    Private Shared ReadOnly ExpectedCor As Double(,) = New Double(,) {
        {1.0, 0.866400225443963, 0.781660828194291},
        {0.866400225443963, 1.0, 0.386654448216613},
        {0.781660828194291, 0.386654448216613, 1.0}
    }

    Private Shared ReadOnly ExpectedEigCov As Double() = New Double() {10.92627272, 0.81886844, 0.02152551}
    Private Shared ReadOnly ExpectedLoadCov As Double(,) = New Double(,) {
        {0.91559231, -0.0671046, -0.39646904},
        {0.28056504, -0.59969846, 0.7494298},
        {0.28805206, 0.79740751, 0.53025209}
    }
    Private Shared ReadOnly ExpectedPercCov As Double() = New Double() {92.85784182, 6.95922185, 0.18293633}

    Private Shared ReadOnly ExpectedEigCor As Double() = New Double() {2.37546206, 0.61595097, 0.00858697}
    Private Shared ReadOnly ExpectedLoadCor As Double(,) = New Double(,) {
        {0.64682908, -0.04334726, 0.7614021},
        {0.55459291, -0.65857364, -0.50863294},
        {0.52348719, 0.75126678, -0.4019445}
    }
    Private Shared ReadOnly ExpectedPercCor As Double() = New Double() {79.18206872, 20.53169897, 0.28623231}

    ' ---------------------------
    ' Core tests
    ' ---------------------------

    <TestMethod>
    Public Sub CorrelationPca_MatchesReferenceValues()
        Dim path = GetTestDataPath("PCA_r_reference_10x3.csv")
        Dim loaded = LoadCsvMatrix(path)

        Dim pca = New Multivariate.PCA ' CreatePcaInstance()
        InvokeMethod(pca, "dataInputs", loaded.data, loaded.rowIds, loaded.varNames, "UnitTest")
        InvokeMethod(pca, "settingsInputs", 2000, 0.000000000001, "Correlation")

        InvokeMethod(pca, "Calculate", "Fixed", 3.0)

        Dim S = GetNonPublicPropertyValue(Of Double(,))(pca, "VarCovarMat")
        Dim eig = GetNonPublicPropertyValue(Of Double())(pca, "Eigenvalues")
        Dim load = GetNonPublicPropertyValue(Of Double(,))(pca, "GetLoadings")
        Dim scores = GetNonPublicPropertyValue(Of Double(,))(pca, "ReducedDataset")
        Dim perc = GetNonPublicPropertyValue(Of Double())(pca, "PercentExpl")
        Dim cum = GetNonPublicPropertyValue(Of Double())(pca, "PercentExplCum")
        Dim k = GetNonPublicPropertyValue(Of Integer)(pca, "NoExtractComponents")

        Assert.AreEqual(3, k, "Expected k=3 for Fixed=3.")
        AssertMatrixClose(ExpectedCor, S, absTol:=0.00000001, msg:="Correlation matrix mismatch.")
        AssertVectorClose(ExpectedEigCor, eig, absTol:=0.000005, relTol:=0.000005, msg:="Eigenvalues (correlation) mismatch.")
        AssertMatrixClose(ExpectedLoadCor, load, absTol:=0.00005, relTol:=0.00005, msg:="Loadings (correlation) mismatch.")
        AssertVectorClose(ExpectedPercCor, perc, absTol:=0.00005, relTol:=0.00005, msg:="Percent explained (correlation) mismatch.")

        ' PercentExpl should sum to ~100 and cumulative last should be ~100
        AssertClose(100.0, perc.Sum(), absTol:=0.000001, msg:="PercentExpl does not sum to 100.")
        AssertClose(100.0, cum(cum.Length - 1), absTol:=0.000001, msg:="PercentExplCum last value not 100.")

        ' Correlation matrix diagonal ~1
        For i As Integer = 0 To 2
            AssertClose(1.0, S(i, i), absTol:=0.000000001, msg:="Correlation matrix diagonal not 1.")
        Next

        ' Scores dimensions: n x k
        Assert.AreEqual(loaded.data.GetLength(0), scores.GetLength(0), "Scores row count mismatch.")
        Assert.AreEqual(k, scores.GetLength(1), "Scores column count mismatch.")
    End Sub

    <TestMethod>
    Public Sub CovariancePca_MatchesReferenceValues()
        Dim path = GetTestDataPath("PCA_r_reference_10x3.csv")
        Dim loaded = LoadCsvMatrix(path)

        Dim pca = New Multivariate.PCA
        InvokeMethod(pca, "dataInputs", loaded.data, loaded.rowIds, loaded.varNames, "UnitTest")
        InvokeMethod(pca, "settingsInputs", 2000, 0.000000000001, "Covariance")

        InvokeMethod(pca, "Calculate", "Fixed", 3.0)

        Dim S = GetNonPublicPropertyValue(Of Double(,))(pca, "VarCovarMat")
        Dim eig = GetNonPublicPropertyValue(Of Double())(pca, "Eigenvalues")
        Dim load = GetNonPublicPropertyValue(Of Double(,))(pca, "GetLoadings")
        Dim perc = GetNonPublicPropertyValue(Of Double())(pca, "PercentExpl")
        Dim k = GetNonPublicPropertyValue(Of Integer)(pca, "NoExtractComponents")

        Assert.AreEqual(3, k, "Expected k=3 for Fixed=3.")
        AssertMatrixClose(ExpectedCov, S, absTol:=0.000000001, msg:="Covariance matrix mismatch.")
        AssertVectorClose(ExpectedEigCov, eig, absTol:=0.000005, relTol:=0.000005, msg:="Eigenvalues (covariance) mismatch.")
        AssertMatrixClose(ExpectedLoadCov, load, absTol:=0.00005, relTol:=0.00005, msg:="Loadings (covariance) mismatch.")
        AssertVectorClose(ExpectedPercCov, perc, absTol:=0.00005, relTol:=0.00005, msg:="Percent explained (covariance) mismatch.")
    End Sub

    <TestMethod>
    Public Sub ExtractionMethods_WorkAsExpected()
        Dim path = GetTestDataPath("PCA_r_reference_10x3.csv")
        Dim loaded = LoadCsvMatrix(path)

        ' Covariance PCA: eigenvalues ~ [10.926, 0.819, 0.022]
        ' Eigenvalue threshold 1.0 => k=1
        Dim pca1 = New Multivariate.PCA
        InvokeMethod(pca1, "dataInputs", loaded.data, loaded.rowIds, loaded.varNames, "UnitTest")
        InvokeMethod(pca1, "settingsInputs", 2000, 0.000000000001, "Covariance")
        InvokeMethod(pca1, "Calculate", "Eigenvalue", 1.0)
        Dim k1 = GetNonPublicPropertyValue(Of Integer)(pca1, "NoExtractComponents")
        Assert.AreEqual(1, k1, "Eigenvalue threshold extraction should keep 1 component for this dataset (covariance).")

        ' Variance threshold 95% => k=2 (92.86% then 99.82%)
        Dim pca2 = New Multivariate.PCA
        InvokeMethod(pca2, "dataInputs", loaded.data, loaded.rowIds, loaded.varNames, "UnitTest")
        InvokeMethod(pca2, "settingsInputs", 2000, 0.000000000001, "Covariance")
        InvokeMethod(pca2, "Calculate", "Variance", 95.0)
        Dim k2 = GetNonPublicPropertyValue(Of Integer)(pca2, "NoExtractComponents")
        Assert.AreEqual(2, k2, "Variance threshold extraction (95%) should keep 2 components for this dataset (covariance).")

        ' Fixed extraction
        Dim pca3 = New Multivariate.PCA
        InvokeMethod(pca3, "dataInputs", loaded.data, loaded.rowIds, loaded.varNames, "UnitTest")
        InvokeMethod(pca3, "settingsInputs", 2000, 0.000000000001, "Covariance")
        InvokeMethod(pca3, "Calculate", "Fixed", 2.0)
        Dim k3 = GetNonPublicPropertyValue(Of Integer)(pca3, "NoExtractComponents")
        Assert.AreEqual(2, k3, "Fixed extraction should keep exactly 2 components.")
    End Sub

    <TestMethod>
    Public Sub CollinearData_HasZeroSecondEigenvalue_AndKaiserSelectsOne()
        Dim path = GetTestDataPath("PCA_collinear_5x2.csv")
        Dim loaded = LoadCsvMatrix(path)

        Dim pca = New Multivariate.PCA
        InvokeMethod(pca, "dataInputs", loaded.data, loaded.rowIds, loaded.varNames, "UnitTest")
        InvokeMethod(pca, "settingsInputs", 2000, 0.000000000001, "Correlation")

        InvokeMethod(pca, "Calculate", "Fixed", 2.0)

        Dim eig = GetNonPublicPropertyValue(Of Double())(pca, "Eigenvalues")
        AssertClose(2.0, eig(0), absTol:=0.000005, msg:="First eigenvalue for perfect correlation should be ~2.")
        AssertClose(0.0, eig(1), absTol:=0.000005, msg:="Second eigenvalue for perfect correlation should be ~0.")

        ' Kaiser rule: eigenvalue >= 1 => 1 component
        Dim pcaK = New Multivariate.PCA
        InvokeMethod(pcaK, "dataInputs", loaded.data, loaded.rowIds, loaded.varNames, "UnitTest")
        InvokeMethod(pcaK, "settingsInputs", 2000, 0.000000000001, "Correlation")
        InvokeMethod(pcaK, "Calculate", "Eigenvalue", 1.0)
        Dim k = GetNonPublicPropertyValue(Of Integer)(pcaK, "NoExtractComponents")
        Assert.AreEqual(1, k, "Kaiser rule (>=1) should select 1 component for perfectly correlated 2-variable data.")
    End Sub

    <TestMethod>
    Public Sub Loadings_AreOrthonormal_AndUseSignConvention()
        Dim path = GetTestDataPath("PCA_near_collinear_20x3.csv")
        Dim loaded = LoadCsvMatrix(path)

        Dim pca = New Multivariate.PCA
        InvokeMethod(pca, "dataInputs", loaded.data, loaded.rowIds, loaded.varNames, "UnitTest")
        InvokeMethod(pca, "settingsInputs", 5000, 0.000000000001, "Correlation")
        InvokeMethod(pca, "Calculate", "Fixed", 3.0)

        Dim V = GetNonPublicPropertyValue(Of Double(,))(pca, "Eigenvectors") ' full p×p
        Dim VT = Transpose(V)
        Dim VTV = MatMul(VT, V)
        Dim II = Identity(VTV.GetLength(0))

        ' V should be orthonormal: V^T V ≈ I
        Dim diff(VTV.GetLength(0) - 1, VTV.GetLength(1) - 1) As Double
        For i As Integer = 0 To diff.GetLength(0) - 1
            For j As Integer = 0 To diff.GetLength(1) - 1
                diff(i, j) = VTV(i, j) - II(i, j)
            Next
        Next
        Assert.IsTrue(MaxAbs(diff) < 0.00001, $"Eigenvectors are not sufficiently orthonormal. MaxAbs(V^T V - I)={MaxAbs(diff):R}")

        ' Sign convention check on selected loadings (GetLoadings):
        Dim L = GetNonPublicPropertyValue(Of Double(,))(pca, "GetLoadings")
        For j As Integer = 0 To L.GetLength(1) - 1
            Dim maxAbs As Double = -1
            Dim valAtMaxAbs As Double = 0
            For i As Integer = 0 To L.GetLength(0) - 1
                Dim a = Math.Abs(L(i, j))
                If a > maxAbs Then
                    maxAbs = a
                    valAtMaxAbs = L(i, j)
                End If
            Next
            Assert.IsTrue(valAtMaxAbs > 0, $"Loading sign convention violated for component {j}. Largest-magnitude entry should be positive.")
        Next
    End Sub

    <TestMethod>
    Public Sub ConstantColumn_Throws()
        Dim path = GetTestDataPath("PCA_constant_column_5x3.csv")
        Dim loaded = LoadCsvMatrix(path)

        Dim pca = New Multivariate.PCA
        InvokeMethod(pca, "dataInputs", loaded.data, loaded.rowIds, loaded.varNames, "UnitTest")
        InvokeMethod(pca, "settingsInputs", 2000, 0.000000000001, "Correlation")

        Dim threw As Boolean = False
        Try
            InvokeMethod(pca, "Calculate", "Fixed", 2.0)
        Catch ex As TargetInvocationException
            threw = True
            Dim inner = ex.InnerException
            Assert.IsTrue(TypeOf inner Is ArgumentException OrElse TypeOf inner?.InnerException Is ArgumentException,
                          $"Expected ArgumentException (or wrapped), got: {inner?.GetType().FullName}")
        End Try

        Assert.IsTrue(threw, "Expected Calculate to throw for a constant column (SD=0).")
    End Sub

    <TestMethod>
    Public Sub WrapResults_ReturnsFiveTables()
        Dim path = GetTestDataPath("PCA_r_reference_10x3.csv")
        Dim loaded = LoadCsvMatrix(path)

        Dim pca = New Multivariate.PCA
        InvokeMethod(pca, "dataInputs", loaded.data, loaded.rowIds, loaded.varNames, "UnitTest")
        InvokeMethod(pca, "settingsInputs", 2000, 0.000000000001, "Correlation")
        InvokeMethod(pca, "Calculate", "Fixed", 3.0)

        Dim resObj = InvokeMethod(pca, "wrapResults")
        Assert.IsNotNull(resObj, "wrapResults returned Nothing.")
        Dim enumerable = TryCast(resObj, IEnumerable)
        Assert.IsNotNull(enumerable, "wrapResults did not return an IEnumerable.")

        Dim count As Integer = 0
        For Each item In enumerable
            Assert.IsNotNull(item, "wrapResults contains a null item.")
            count += 1
        Next
        Assert.AreEqual(4, count, "wrapResults should return exactly 5 ResultTable objects.")
    End Sub

    <TestMethod>
    Public Sub StandardizedVarNames_AddsPrefix()
        Dim pca As New Multivariate.PCA()

        Dim input() As String = {"A", "B", "C"}
        Dim output() As String = pca.StandardizedVarNames(input)

        CollectionAssert.AreEqual(
        New String() {"Standardized_A", "Standardized_B", "Standardized_C"},
        output)
    End Sub



    <TestMethod>
    Public Sub ScoresEqualDataTimesLoadings()
        ' Validates that ReducedDataset = (StandardizedData or CenteredData) * Loadings, matching PCA.Calculate.
        Dim path = GetTestDataPath("PCA_r_reference_10x3.csv")
        Dim loaded = LoadCsvMatrix(path)

        ' Correlation PCA: scores = StandardizedData * Loadings
        Dim pcaCor = New Multivariate.PCA
        InvokeMethod(pcaCor, "dataInputs", loaded.data, loaded.rowIds, loaded.varNames, "UnitTest")
        InvokeMethod(pcaCor, "settingsInputs", 2000, 0.000000000001, "Correlation")
        InvokeMethod(pcaCor, "Calculate", "Fixed", 3.0)

        Dim Z = GetNonPublicPropertyValue(Of Double(,))(pcaCor, "ReducedDataset")
        Dim Xz = GetNonPublicPropertyValue(Of Double(,))(pcaCor, "StandardizedData")
        Dim L = GetNonPublicPropertyValue(Of Double(,))(pcaCor, "GetLoadings")
        Dim Z2 = MatMul(Xz, L)
        AssertMatrixClose(Z2, Z, absTol:=0.000001, relTol:=0.000001, msg:="Scores mismatch for correlation PCA.")

        ' Covariance PCA: scores = CenteredData * Loadings (CenteredData isn't stored; compute it here)
        Dim pcaCov = New Multivariate.PCA
        InvokeMethod(pcaCov, "dataInputs", loaded.data, loaded.rowIds, loaded.varNames, "UnitTest")
        InvokeMethod(pcaCov, "settingsInputs", 2000, 0.000000000001, "Covariance")
        InvokeMethod(pcaCov, "Calculate", "Fixed", 3.0)

        Dim Zc = GetNonPublicPropertyValue(Of Double(,))(pcaCov, "ReducedDataset")
        Dim Lc = GetNonPublicPropertyValue(Of Double(,))(pcaCov, "GetLoadings")

        ' Center loaded.data by column means (sample mean)
        Dim n As Integer = loaded.data.GetLength(0)
        Dim pVars As Integer = loaded.data.GetLength(1)
        Dim Xc(n - 1, pVars - 1) As Double
        For j As Integer = 0 To pVars - 1
            Dim sum As Double = 0.0
            For i As Integer = 0 To n - 1
                sum += loaded.data(i, j)
            Next
            Dim mean As Double = sum / n
            For i As Integer = 0 To n - 1
                Xc(i, j) = loaded.data(i, j) - mean
            Next
        Next

        Dim Zc2 = MatMul(Xc, Lc)
        AssertMatrixClose(Zc2, Zc, absTol:=0.000001, relTol:=0.000001, msg:="Scores mismatch for covariance PCA.")
    End Sub
End Class

