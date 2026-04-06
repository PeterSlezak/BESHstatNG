Imports Microsoft.VisualStudio.TestTools.UnitTesting
Imports System
Imports System.Linq
Imports System.Reflection
Imports BESHStatNG.Multivariate

''' <summary>
''' Thorough unit tests for <see cref="CA"/> (Correspondence Analysis / Multiple Correspondence Analysis).
''' 
''' These tests use tiny, deterministic inline datasets (no file IO) and validate:
''' - Results against reference values (generated with the R scripts included in comments).
''' - Core CA/MCA algebraic identities (masses sum to 1, inertia identities, contribution sums, barycenter = 0, etc.).
''' - Internal consistency of reported statistics: distances, cos^2, contributions, angles, eigenvalue contributions, percents.
''' 
''' Notes on sign:
''' CA/MCA axes are defined up to a sign flip. Tests compare factor coordinates after aligning the sign per axis.
''' </summary>
<TestClass()>
Public Class CA_Tests

    '========================
    ' Tolerances
    '========================
    Private Const TolTight As Double = 0.0000000001
    Private Const Tol As Double = 0.00000001
    Private Const TolLoose As Double = 0.000001

    '========================
    ' Reflection helpers (work whether CA exposes members as Public or Friend)
    '========================
    Private Shared Function GetPropValue(Of T)(obj As Object, propName As String, ParamArray indexArgs() As Object) As T
        Dim flags As BindingFlags = BindingFlags.Instance Or BindingFlags.Public Or BindingFlags.NonPublic
        Dim tt As Type = obj.GetType()

        Dim p = tt.GetProperties(flags).
            FirstOrDefault(Function(pi) String.Equals(pi.Name, propName, StringComparison.OrdinalIgnoreCase) AndAlso
                                       pi.GetIndexParameters().Length = indexArgs.Length)

        If p Is Nothing Then
            Throw New MissingMemberException($"Property '{propName}' with {indexArgs.Length} index args not found on type '{tt.FullName}'.")
        End If

        Dim v As Object = p.GetValue(obj, indexArgs)
        Return CType(v, T)
    End Function

    Private Shared Sub AssertClose(expected As Double, actual As Double, tol As Double, message As String)
        Dim diff As Double = Math.Abs(expected - actual)
        Assert.IsTrue(diff <= tol, $"{message} Expected={expected}, Actual={actual}, |diff|={diff}, tol={tol}")
    End Sub

    Private Shared Sub AssertVectorClose(expected() As Double, actual() As Double, tol As Double, message As String)
        Assert.AreEqual(expected.Length, actual.Length, $"{message} Length mismatch.")
        For i As Integer = 0 To expected.Length - 1
            AssertClose(expected(i), actual(i), tol, $"{message} (idx {i})")
        Next
    End Sub

    Private Shared Function AlignSign(expected() As Double, actual() As Double) As Double()
        'If dot(expected, actual) < 0, flip actual.
        Dim dot As Double = 0
        For i As Integer = 0 To expected.Length - 1
            dot += expected(i) * actual(i)
        Next
        If dot < 0 Then
            Dim flipped(actual.Length - 1) As Double
            For i As Integer = 0 To actual.Length - 1
                flipped(i) = -actual(i)
            Next
            Return flipped
        End If
        Return actual
    End Function

    Private Shared Function Sum(v() As Double) As Double
        Dim s As Double = 0
        For Each x In v
            s += x
        Next
        Return s
    End Function

    Private Shared Function WeightedSum(weights() As Double, values() As Double) As Double
        Dim s As Double = 0
        For i As Integer = 0 To weights.Length - 1
            s += weights(i) * values(i)
        Next
        Return s
    End Function

    ''' <summary>Squared Euclidean distance between two rows of a coordinate matrix (row-major).</summary>
    Private Shared Function SquaredDist(coords(,) As Double, i As Integer, j As Integer) As Double
        Dim d As Double = 0
        Dim p As Integer = coords.GetLength(1)
        For ax As Integer = 0 To p - 1
            Dim diff As Double = coords(i, ax) - coords(j, ax)
            d += diff * diff
        Next
        Return d
    End Function


    '========================
    ' Test datasets (inline)
    '========================

    'Simple CA: 2x3 contingency table (counts)
    'Rows: R1,R2; Cols: A,B,C
    '
    'R script to reproduce reference:
    '  X <- matrix(c(12,15,20, 8,25,5), nrow=2, byrow=TRUE)
    '  library(ca)
    '  res <- ca(X)
    '  res$eig; res$rowcoord; res$colcoord
    '
    Private Shared ReadOnly CA_Counts(,) As Integer = {
        {12, 15, 20},
        {8, 25, 5}
    }

    'Reference values for this table (standard CA, principal coordinates):
    'Computed from the standard CA formulas (and matches R ca::ca up to sign).
    Private Shared ReadOnly CA_Eig() As Double = {0.13500839865621495}
    Private Shared ReadOnly CA_RowMass() As Double = {0.55294117647058827, 0.44705882352941179}
    Private Shared ReadOnly CA_ColMass() As Double = {0.23529411764705882, 0.47058823529411764, 0.29411764705882354}
    Private Shared ReadOnly CA_RowDist() As Double = {0.10915573091665665, 0.16698406714490172}
    Private Shared ReadOnly CA_ColDist() As Double = {0.0089585712252711457, 0.12808826253243591, 0.24692048525764274}
    Private Shared ReadOnly CA_RowF1() As Double = {-0.33038723741986642, 0.40863684584246912}
    Private Shared ReadOnly CA_ColF1() As Double = {-0.094649704881909338, 0.35789419642971615, -0.49691094990745871}

    'MCA dataset: 12 individuals, 3 variables; category ordering is alphabetical per variable.
    '
    'R script to reproduce:
    '  library(FactoMineR)
    '  df <- data.frame(
    '    Color=c("Red","Red","Red","Blue","Blue","Blue","Green","Green","Green","Blue","Red","Green"),
    '    Shape=c("Circle","Square","Circle","Circle","Square","Square","Circle","Square","Circle","Circle","Square","Square"),
    '    Texture=c("Smooth","Smooth","Rough","Smooth","Rough","Smooth","Rough","Smooth","Smooth","Rough","Rough","Rough")
    '  )
    '  res <- MCA(df, graph=FALSE)  # method="Indicator" (default)
    '  res$eig; res$var$coord; res$ind$coord
    '
    'Expected category order in our implementation:
    '  Color:   Blue, Green, Red
    '  Shape:   Circle, Square
    '  Texture: Rough, Smooth
    Private Shared ReadOnly MCA_VarNames() As String = {"Color", "Shape", "Texture"}

    Private Shared ReadOnly MCA_Data(,) As String = {
        {"Red", "Circle", "Smooth"},
        {"Red", "Square", "Smooth"},
        {"Red", "Circle", "Rough"},
        {"Blue", "Circle", "Smooth"},
        {"Blue", "Square", "Rough"},
        {"Blue", "Square", "Smooth"},
        {"Green", "Circle", "Rough"},
        {"Green", "Square", "Smooth"},
        {"Green", "Circle", "Smooth"},
        {"Blue", "Circle", "Rough"},
        {"Red", "Square", "Rough"},
        {"Green", "Square", "Rough"}
    }

    Private Shared ReadOnly MCA_ColMass() As Double = {1.0 / 9.0, 1.0 / 9.0, 1.0 / 9.0, 1.0 / 6.0, 1.0 / 6.0, 1.0 / 6.0, 1.0 / 6.0}

    'Eigenvalues from CA on the indicator matrix (MCA "indicator" convention):
    'First four are 1/3, remaining are approximately 0 for this balanced dataset.
    Private Shared ReadOnly MCA_Eig_First4() As Double = {1.0 / 3.0, 1.0 / 3.0, 1.0 / 3.0, 1.0 / 3.0}

    'Category principal coordinates (first two axes) from standard CA on the indicator matrix (up to sign per axis).
    'Order: [Blue, Green, Red, Circle, Square, Rough, Smooth]
    Private Shared ReadOnly MCA_ColF_Axis0() As Double = {
        0.000000000000000048074067159589089,
        -0.97330230960907627,
        0.97330230960907627,
        0.47444963724336003,
        -0.47444963724336009,
        0.37861935612374376,
        -0.37861935612374376
    }

    Private Shared ReadOnly MCA_ColF_Axis1() As Double = {
        1.4142135623730951,
        -0.70710678118654757,
        -0.70710678118654757,
        -0.000000000000000033993498887762956,
        -0.000000000000000067986997775525911,
        -0.000000000000000067986997775525911,
        0.0
    }

    'Individual principal coordinates (first two axes) for the 12 rows (up to sign per axis, aligned with category axes).
    Private Shared ReadOnly MCA_RowF_Axis0() As Double = {
        0.61726399205222859,
        0.069416739835907224,
        1.0544559619409306,
        0.055327644805490322,
        -0.055327644805490329,
        -0.49251961269319267,
        -0.069416739835907224,
        -1.0544559619409306,
        -0.50660870772360955,
        0.49251961269319267,
        0.50660870772360955,
        -0.61726399205222859
    }

    Private Shared ReadOnly MCA_RowF_Axis1() As Double = {
        -0.40824829046386296,
        -0.40824829046386296,
        -0.40824829046386296,
        0.81649658092772615,
        0.81649658092772615,
        0.81649658092772615,
        -0.40824829046386296,
        -0.40824829046386296,
        -0.40824829046386296,
        0.81649658092772615,
        -0.40824829046386296,
        -0.40824829046386296
    }

    '========================
    ' CA tests (simple correspondence analysis)
    '========================

    <TestMethod()>
    Public Sub CA_Simple_MatchesReference_Eigen_Masses_Factors_Distances()
        Dim ca As New CA()
        ca.data(CA_Counts, rows:=New String() {"R1", "R2"}, cols:=New String() {"A", "B", "C"})
        ca.Calculate()

        Dim eig = GetPropValue(Of Double())(ca, "Eigenvalues")
        Assert.AreEqual(1, eig.Length, "CA: expected 1 non-trivial eigenvalue for a 2x3 table (min(R,C)-1).")
        AssertClose(CA_Eig(0), eig(0), 0.0000000001, "CA: eigenvalue(1) mismatch.")

        Dim rowMass = GetPropValue(Of Double())(ca, "RowMass")
        Dim colMass = GetPropValue(Of Double())(ca, "ColMass")
        AssertVectorClose(CA_RowMass, rowMass, TolTight, "CA: row masses mismatch.")
        AssertVectorClose(CA_ColMass, colMass, TolTight, "CA: col masses mismatch.")
        AssertClose(1.0, Sum(rowMass), TolTight, "CA: row masses must sum to 1.")
        AssertClose(1.0, Sum(colMass), TolTight, "CA: col masses must sum to 1.")

        Dim rowDist = GetPropValue(Of Double())(ca, "RowDistance")
        Dim colDist = GetPropValue(Of Double())(ca, "ColDistance")
        AssertVectorClose(CA_RowDist, rowDist, TolLoose, "CA: row distances mismatch.")
        AssertVectorClose(CA_ColDist, colDist, TolLoose, "CA: col distances mismatch.")

        Dim rowF = GetPropValue(Of Double())(ca, "RowFactors", 0)
        Dim colF = GetPropValue(Of Double())(ca, "ColFactors", 0)

        'Align axis sign using column factors (axis sign is arbitrary)
        Dim dot As Double = 0
        For i As Integer = 0 To CA_ColF1.Length - 1
            dot += CA_ColF1(i) * colF(i)
        Next
        Dim signFlip As Double = If(dot < 0, -1.0, 1.0)

        Dim colFAligned = colF.Select(Function(x) signFlip * x).ToArray()
        Dim rowFAligned = rowF.Select(Function(x) signFlip * x).ToArray()

        AssertVectorClose(CA_ColF1, colFAligned, 0.0000005, "CA: column factor (axis1) mismatch (after sign alignment).")
        AssertVectorClose(CA_RowF1, rowFAligned, 0.0000005, "CA: row factor (axis1) mismatch (after sign alignment).")

        'Percents: for 2x3 CA there is only one axis, so percent and cumulative should both be 1.
        Dim per = GetPropValue(Of Double(,))(ca, "Percents")
        Assert.AreEqual(1, per.GetLength(0), "CA: Percents rows should equal number of eigenvalues.")
        Assert.AreEqual(2, per.GetLength(1), "CA: Percents should have 2 columns (percent, cumulative).")
        AssertClose(100, per(0, 0), TolTight, "CA: percent inertia should be 1.0 for the only axis.")
        AssertClose(100, per(0, 1), TolTight, "CA: cumulative inertia should be 1.0 for the only axis.")
    End Sub


    <TestMethod()>
    Public Sub CA_Simple_InternalIdentities_Mass_Inertia_Corr_Contrib_Angles()
        Dim ca As New CA()
        ca.data(CA_Counts)
        ca.Calculate()

        Dim eig = GetPropValue(Of Double())(ca, "Eigenvalues")
        Dim lambda As Double = eig(0)

        Dim rowMass = GetPropValue(Of Double())(ca, "RowMass")
        Dim colMass = GetPropValue(Of Double())(ca, "ColMass")

        Dim rowDist = GetPropValue(Of Double())(ca, "RowDistance")
        Dim colDist = GetPropValue(Of Double())(ca, "ColDistance")

        Dim rowInertia = GetPropValue(Of Double())(ca, "RowInertia")
        Dim colInertia = GetPropValue(Of Double())(ca, "ColInertia")

        '--- Inertia identities (NOTE: class stores normalized inertia shares) ---
        Dim totalRawInertiaRows As Double = 0.0
        For i As Integer = 0 To rowMass.Length - 1
            totalRawInertiaRows += rowMass(i) * rowDist(i)
        Next

        Dim totalRawInertiaCols As Double = 0.0
        For j As Integer = 0 To colMass.Length - 1
            totalRawInertiaCols += colMass(j) * colDist(j)
        Next

        'Raw inertia totals must match between rows/cols
        AssertClose(totalRawInertiaRows, totalRawInertiaCols, 0.0000000001, "CA: total raw inertia must match for rows and columns.")
        'For 1-axis case, total inertia equals the single eigenvalue
        AssertClose(lambda, totalRawInertiaRows, 0.00000001, "CA: total raw inertia must equal sum of eigenvalues (here: single eigenvalue).")

        'Row/col inertia arrays in this implementation are normalized shares, so they must sum to 1
        AssertClose(1.0, Sum(rowInertia), 0.000001, "CA: row inertia shares must sum to 1.")
        AssertClose(1.0, Sum(colInertia), 0.000001, "CA: col inertia shares must sum to 1.")

        'And each element must equal (mass*dist)/totalRawInertia
        For i As Integer = 0 To rowMass.Length - 1
            Dim expectedShare As Double = (rowMass(i) * rowDist(i)) / totalRawInertiaRows
            AssertClose(expectedShare, rowInertia(i), TolLoose, $"CA: row inertia identity (normalized) failed at row {i}.")
        Next
        For j As Integer = 0 To colMass.Length - 1
            Dim expectedShare As Double = (colMass(j) * colDist(j)) / totalRawInertiaCols
            AssertClose(expectedShare, colInertia(j), TolLoose, $"CA: col inertia identity (normalized) failed at col {j}.")
        Next

        '--- Barycenter property: weighted coordinate sum = 0 for each axis ---
        Dim rowF = GetPropValue(Of Double())(ca, "RowFactors", 0) 'axis1 (0-based in public API)
        Dim colF = GetPropValue(Of Double())(ca, "ColFactors", 0)
        AssertClose(0.0, WeightedSum(rowMass, rowF), 0.0000001, "CA: row factor barycenter should be ~0.")
        AssertClose(0.0, WeightedSum(colMass, colF), 0.0000001, "CA: col factor barycenter should be ~0.")

        '--- Cos^2 and contributions ---
        Dim rowCorr = GetPropValue(Of Double())(ca, "RowCorr", 0)
        Dim colCorr = GetPropValue(Of Double())(ca, "ColCorr", 0)
        Dim rowContr = GetPropValue(Of Double())(ca, "RowContribution", 0)
        Dim colContr = GetPropValue(Of Double())(ca, "ColContribution", 0)
        Dim rowAngle = GetPropValue(Of Double())(ca, "RowAngle", 0)
        Dim colAngle = GetPropValue(Of Double())(ca, "ColAngle", 0)
        Dim rowEigContr = GetPropValue(Of Double())(ca, "RowEigenvalueContrib", 0)
        Dim colEigContr = GetPropValue(Of Double())(ca, "ColEigenvalueContrib", 0)
        Dim rowQual = GetPropValue(Of Double())(ca, "RowQuality")
        Dim colQual = GetPropValue(Of Double())(ca, "ColQuality")

        'Contribution sums per axis should be 1
        AssertClose(1.0, Sum(rowContr), 0.000001, "CA: sum of row contributions for an axis must be 1.")
        AssertClose(1.0, Sum(colContr), 0.000001, "CA: sum of col contributions for an axis must be 1.")

        'Cos^2 formula and quality (with only one axis computed here, quality == cos^2)
        For i As Integer = 0 To rowMass.Length - 1
            If rowDist(i) > 0 Then
                Dim expectedCorr As Double = (rowF(i) * rowF(i)) / rowDist(i)
                AssertClose(expectedCorr, rowCorr(i), 0.000001, $"CA: row cos^2 formula failed at row {i}.")
                AssertClose(rowCorr(i), rowQual(i), 0.000001, $"CA: row quality should equal cos^2 for 1-axis case at row {i}.")

                'Angle formula: angle = acos(sqrt(cos^2)) in degrees (corr clamped)
                Dim c As Double = Math.Max(0.0, Math.Min(1.0, rowCorr(i)))
                Dim expectedAngle As Double = 180.0 * Math.Acos(Math.Sqrt(c)) / Math.PI
                AssertClose(expectedAngle, rowAngle(i), 0.000001, $"CA: row angle formula failed at row {i}.")
            Else
                'With the new guard: dist=0 => corr=0 and angle=0
                AssertClose(0.0, rowCorr(i), 0.000000000001, $"CA: row corr should be 0 when distance is 0 at row {i}.")
                AssertClose(0.0, rowAngle(i), 0.000000000001, $"CA: row angle should be 0 when distance is 0 at row {i}.")
                AssertClose(0.0, rowQual(i), 0.000000000001, $"CA: row quality should be 0 when distance is 0 at row {i}.")
            End If

            'Eigenvalue contribution identity: eigContrib = contribution * eigenvalue
            AssertClose(rowContr(i) * lambda, rowEigContr(i), 0.000001, $"CA: row eigenvalue contribution failed at row {i}.")
        Next

        For j As Integer = 0 To colMass.Length - 1
            If colDist(j) > 0 Then
                Dim expectedCorr As Double = (colF(j) * colF(j)) / colDist(j)
                AssertClose(expectedCorr, colCorr(j), 0.000001, $"CA: col cos^2 formula failed at col {j}.")
                AssertClose(colCorr(j), colQual(j), 0.000001, $"CA: col quality should equal cos^2 for 1-axis case at col {j}.")

                Dim c As Double = Math.Max(0.0, Math.Min(1.0, colCorr(j)))
                Dim expectedAngle As Double = 180.0 * Math.Acos(Math.Sqrt(c)) / Math.PI
                AssertClose(expectedAngle, colAngle(j), 0.000001, $"CA: col angle formula failed at col {j}.")
            Else
                AssertClose(0.0, colCorr(j), 0.000000000001, $"CA: col corr should be 0 when distance is 0 at col {j}.")
                AssertClose(0.0, colAngle(j), 0.000000000001, $"CA: col angle should be 0 when distance is 0 at col {j}.")
                AssertClose(0.0, colQual(j), 0.000000000001, $"CA: col quality should be 0 when distance is 0 at col {j}.")
            End If

            AssertClose(colContr(j) * lambda, colEigContr(j), 0.000001, $"CA: col eigenvalue contribution failed at col {j}.")
        Next
    End Sub


    '========================
    ' MCA tests (multiple correspondence analysis via indicator matrix)
    '========================

    <TestMethod()>
    Public Sub MCA_Indicator_MatchesReference_Eigen_Masses_Factors_And_MatrixShapes()
        Dim ca As New CA()
        ca.DataMultiple(MCA_Data, MCA_VarNames)
        ca.Calculate()

        'Design matrix checks: N x K, each row has Q ones
        Dim Z = GetPropValue(Of Integer(,))(ca, "DesignMatrix")
        Dim n As Integer = Z.GetLength(0)
        Dim k As Integer = Z.GetLength(1)
        Assert.AreEqual(12, n, "MCA: expected 12 individuals.")
        Assert.AreEqual(7, k, "MCA: expected 7 category columns (3 + 2 + 2).")

        For i As Integer = 0 To n - 1
            Dim rs As Integer = 0
            For j As Integer = 0 To k - 1
                rs += Z(i, j)
                Assert.IsTrue(Z(i, j) = 0 OrElse Z(i, j) = 1, "MCA: design matrix must be 0/1.")
            Next
            Assert.AreEqual(3, rs, $"MCA: each individual row should have exactly Q=3 active categories (row {i}).")
        Next

        'Eigenvalues: first 4 = 1/3, remaining ~0 (for this balanced toy dataset)
        Dim eig = GetPropValue(Of Double())(ca, "Eigenvalues")
        Assert.IsTrue(eig.Length >= 4, "MCA: expected at least 4 eigenvalues.")
        For i As Integer = 0 To 3
            AssertClose(MCA_Eig_First4(i), eig(i), 0.000001, $"MCA: eigenvalue {i + 1} mismatch.")
        Next
        For i As Integer = 4 To Math.Min(eig.Length - 1, 6)
            Assert.IsTrue(Math.Abs(eig(i)) < 0.0000000001, $"MCA: eigenvalue {i + 1} should be ~0 for this dataset.")
        Next

        'Masses
        Dim rowMass = GetPropValue(Of Double())(ca, "RowMass")
        Assert.AreEqual(12, rowMass.Length, "MCA: row mass vector should be length N.")
        For i As Integer = 0 To rowMass.Length - 1
            AssertClose(1.0 / 12.0, rowMass(i), 0.0000000001, $"MCA: row mass should be 1/N at row {i}.")
        Next
        AssertClose(1.0, Sum(rowMass), 0.0000000001, "MCA: row masses must sum to 1.")

        Dim colMass = GetPropValue(Of Double())(ca, "ColMass")
        AssertVectorClose(MCA_ColMass, colMass, 0.0000000001, "MCA: column masses mismatch.")
        'Category/individual coordinates are not uniquely defined axis-by-axis here because this toy dataset
        'has a 4-dimensional eigenspace with repeated eigenvalues (=1/3). Different SVD implementations can
        'return any orthonormal basis within that subspace (i.e., axes can rotate/mix, not just flip sign).
        '
        'So instead of comparing axis-wise coordinates, we compare rotation-invariant quantities:
        '  - eigenvalues (already checked above)
        '  - pairwise squared Euclidean distances in the factor space using the full non-zero subspace (first 4 axes)

        Dim kCats As Integer = GetPropValue(Of Double())(ca, "ColMass").Length

        'Build K x 4 category coordinate matrix from CA output
        Dim catCoord(kCats - 1, 3) As Double
        For ax As Integer = 0 To 3
            Dim v = GetPropValue(Of Double())(ca, "ColFactors", ax)
            For i As Integer = 0 To kCats - 1
                catCoord(i, ax) = v(i)
            Next
        Next

        'Expected rotation-invariant pairwise squared distances between the 7 category points in the full 4D subspace.
        'Order: [Blue, Green, Red, Circle, Square, Rough, Smooth]
        Dim expD(,) As Double = {
            {0, 6, 6, 3, 3, 3, 3},
            {6, 0, 6, 3, 3, 3, 3},
            {6, 6, 0, 3, 3, 3, 3},
            {3, 3, 3, 0, 4, 2, 2},
            {3, 3, 3, 4, 0, 2, 2},
            {3, 3, 3, 2, 2, 0, 4},
            {3, 3, 3, 2, 2, 4, 0}
        }

        'Compare squared-distance matrices (rotation-invariant within the repeated-eigenvalue subspace)
        For i As Integer = 0 To kCats - 1
            For j As Integer = 0 To kCats - 1
                Dim d As Double = 0
                For ax As Integer = 0 To 3
                    Dim diff As Double = catCoord(i, ax) - catCoord(j, ax)
                    d += diff * diff
                Next
                AssertClose(expD(i, j), d, 0.000005, $"MCA: category pairwise distance mismatch at ({i},{j}).")
            Next
        Next

        'Optionally also check a few individual pairwise distances in the full 4D subspace (same invariance logic).
        Dim nInd As Integer = GetPropValue(Of Integer(,))(ca, "DesignMatrix").GetLength(0)
        Dim indCoord(nInd - 1, 3) As Double
        For ax As Integer = 0 To 3
            Dim v = GetPropValue(Of Double())(ca, "RowFactors", ax)
            For i As Integer = 0 To nInd - 1
                indCoord(i, ax) = v(i)
            Next
        Next

        'A small set of stable reference distances (computed from CA on the indicator matrix)
        AssertClose(1.3333333333333333, SquaredDist(indCoord, 0, 1), 0.000005, "MCA: ind dist(0,1) mismatch.")
        AssertClose(2.0, SquaredDist(indCoord, 0, 3), 0.000005, "MCA: ind dist(0,3) mismatch.")
        AssertClose(4.666666666666667, SquaredDist(indCoord, 0, 4), 0.000005, "MCA: ind dist(0,4) mismatch.")
        AssertClose(2.6666666666666665, SquaredDist(indCoord, 3, 4), 0.000005, "MCA: ind dist(3,4) mismatch.")
    End Sub

    <TestMethod()>
    Public Sub MCA_Indicator_InternalIdentities_Inertia_Contrib_Corr_Angles_Burt()
        Dim ca As New CA()
        ca.DataMultiple(MCA_Data, MCA_VarNames)
        ca.Calculate()

        Dim eig = GetPropValue(Of Double())(ca, "Eigenvalues")
        Dim rowMass = GetPropValue(Of Double())(ca, "RowMass")
        Dim colMass = GetPropValue(Of Double())(ca, "ColMass")

        Dim rowDist = GetPropValue(Of Double())(ca, "RowDistance")
        Dim colDist = GetPropValue(Of Double())(ca, "ColDistance")
        Dim rowInertia = GetPropValue(Of Double())(ca, "RowInertia")
        Dim colInertia = GetPropValue(Of Double())(ca, "ColInertia")

        '--- Inertia identities (NOTE: class stores normalized inertia shares, not raw r_i*d_i) ---
        Dim totalRawInertiaRows As Double = 0.0
        For i As Integer = 0 To rowMass.Length - 1
            totalRawInertiaRows += rowMass(i) * rowDist(i)
        Next

        Dim totalRawInertiaCols As Double = 0.0
        For j As Integer = 0 To colMass.Length - 1
            totalRawInertiaCols += colMass(j) * colDist(j)
        Next

        AssertClose(totalRawInertiaRows, totalRawInertiaCols, 0.00000001, "MCA: total raw inertia must match for rows and columns.")

        'Row/col inertia arrays are normalized shares -> must sum to 1
        AssertClose(1.0, Sum(rowInertia), 0.000001, "MCA: row inertia shares must sum to 1.")
        AssertClose(1.0, Sum(colInertia), 0.000001, "MCA: col inertia shares must sum to 1.")

        'Element-wise normalized identity
        For i As Integer = 0 To rowMass.Length - 1
            Dim expectedShare As Double = (rowMass(i) * rowDist(i)) / totalRawInertiaRows
            AssertClose(expectedShare, rowInertia(i), 0.000001, $"MCA: row inertia identity (normalized) failed at row {i}.")
        Next
        For j As Integer = 0 To colMass.Length - 1
            Dim expectedShare As Double = (colMass(j) * colDist(j)) / totalRawInertiaCols
            AssertClose(expectedShare, colInertia(j), 0.000001, $"MCA: col inertia identity (normalized) failed at col {j}.")
        Next

        '--- Total inertia vs eigenvalues ---
        'In this implementation Sum(RowInertia)=1 by construction, so compare eigenvalues to the RAW inertia total:
        AssertClose(Sum(eig), totalRawInertiaRows, 0.000001, "MCA: total raw inertia must equal sum of eigenvalues.")

        '--- Axis-wise contribution sums (only first two axes are computed for corr/contrib) ---
        Dim axesToCheck As Integer = Math.Min(2, eig.Length)
        For axis As Integer = 0 To axesToCheck - 1
            Dim rowContr = GetPropValue(Of Double())(ca, "RowContribution", axis)
            Dim colContr = GetPropValue(Of Double())(ca, "ColContribution", axis)
            AssertClose(1.0, Sum(rowContr), 0.000001, $"MCA: sum of row contributions must be 1 for axis {axis}.")
            AssertClose(1.0, Sum(colContr), 0.000001, $"MCA: sum of col contributions must be 1 for axis {axis}.")
        Next

        '--- Cos^2, angle and eigenvalue contributions for first two axes ---
        For axis As Integer = 0 To axesToCheck - 1
            Dim rowF = GetPropValue(Of Double())(ca, "RowFactors", axis)
            Dim colF = GetPropValue(Of Double())(ca, "ColFactors", axis)

            Dim rowCorr = GetPropValue(Of Double())(ca, "RowCorr", axis)
            Dim colCorr = GetPropValue(Of Double())(ca, "ColCorr", axis)

            Dim rowAngle = GetPropValue(Of Double())(ca, "RowAngle", axis)
            Dim colAngle = GetPropValue(Of Double())(ca, "ColAngle", axis)

            Dim rowEigContr = GetPropValue(Of Double())(ca, "RowEigenvalueContrib", axis)
            Dim colEigContr = GetPropValue(Of Double())(ca, "ColEigenvalueContrib", axis)

            Dim rowContr = GetPropValue(Of Double())(ca, "RowContribution", axis)
            Dim colContr = GetPropValue(Of Double())(ca, "ColContribution", axis)

            For i As Integer = 0 To rowF.Length - 1
                If rowDist(i) > 0 Then
                    AssertClose((rowF(i) * rowF(i)) / rowDist(i), rowCorr(i), 0.000001, $"MCA: row cos^2 failed at row {i}, axis {axis}.")
                    Dim c As Double = Math.Max(0.0, Math.Min(1.0, rowCorr(i)))
                    Dim expectedAngle As Double = 180.0 * Math.Acos(Math.Sqrt(c)) / Math.PI
                    AssertClose(expectedAngle, rowAngle(i), 0.000001, $"MCA: row angle failed at row {i}, axis {axis}.")
                Else
                    'With the guard: dist=0 => corr=0 and angle=0
                    AssertClose(0.0, rowCorr(i), 0.000000000001, $"MCA: row corr should be 0 when distance is 0 at row {i}, axis {axis}.")
                    AssertClose(0.0, rowAngle(i), 0.000000000001, $"MCA: row angle should be 0 when distance is 0 at row {i}, axis {axis}.")
                End If
                AssertClose(rowContr(i) * eig(axis), rowEigContr(i), 0.000001, $"MCA: row eigenvalue contribution failed at row {i}, axis {axis}.")
            Next

            For j As Integer = 0 To colF.Length - 1
                If colDist(j) > 0 Then
                    AssertClose((colF(j) * colF(j)) / colDist(j), colCorr(j), 0.000001, $"MCA: col cos^2 failed at col {j}, axis {axis}.")
                    Dim c As Double = Math.Max(0.0, Math.Min(1.0, colCorr(j)))
                    Dim expectedAngle As Double = 180.0 * Math.Acos(Math.Sqrt(c)) / Math.PI
                    AssertClose(expectedAngle, colAngle(j), 0.000001, $"MCA: col angle failed at col {j}, axis {axis}.")
                Else
                    AssertClose(0.0, colCorr(j), 0.000000000001, $"MCA: col corr should be 0 when distance is 0 at col {j}, axis {axis}.")
                    AssertClose(0.0, colAngle(j), 0.000000000001, $"MCA: col angle should be 0 when distance is 0 at col {j}, axis {axis}.")
                End If
                AssertClose(colContr(j) * eig(axis), colEigContr(j), 0.000001, $"MCA: col eigenvalue contribution failed at col {j}, axis {axis}.")
            Next
        Next

        '--- Barycenter (weighted means) should be approximately 0 for each computed axis ---
        For axis As Integer = 0 To axesToCheck - 1
            Dim rowF = GetPropValue(Of Double())(ca, "RowFactors", axis)
            Dim colF = GetPropValue(Of Double())(ca, "ColFactors", axis)
            AssertClose(0.0, WeightedSum(rowMass, rowF), 0.0000001, $"MCA: row barycenter failed for axis {axis}.")
            AssertClose(0.0, WeightedSum(colMass, colF), 0.0000001, $"MCA: col barycenter failed for axis {axis}.")
        Next

        '--- Burt table correctness: B = Z^T Z ---
        Dim Z = GetPropValue(Of Integer(,))(ca, "DesignMatrix")
        Dim B = GetPropValue(Of Integer(,))(ca, "BurtTable")
        Assert.AreEqual(Z.GetLength(1), B.GetLength(0), "MCA: Burt dimension mismatch.")
        Assert.AreEqual(Z.GetLength(1), B.GetLength(1), "MCA: Burt dimension mismatch.")

        Dim K As Integer = Z.GetLength(1)
        For i As Integer = 0 To K - 1
            For j As Integer = 0 To K - 1
                Dim s As Integer = 0
                For n As Integer = 0 To Z.GetLength(0) - 1
                    s += Z(n, i) * Z(n, j)
                Next
                Assert.AreEqual(s, B(i, j), $"MCA: Burt entry mismatch at ({i},{j}).")
            Next
        Next
    End Sub


End Class
