Option Explicit On
Imports System.Linq
Imports BESHStatNG.AppInfrastructure

Namespace Multivariate

    '''''' <summary>
    '''''' Performs Principal Component Analysis (PCA) on a numeric data matrix.
    '''''' </summary>
    '''''' <remarks>
    '''''' <para>
    '''''' This class accepts an <c>n × p</c> matrix <c>X</c> (n observations/rows, p variables/columns) and computes
    '''''' principal components either from the covariance matrix (original scale) or from the correlation matrix
    '''''' (standardized variables).
    '''''' </para>
    '''''' <para>
    '''''' Mathematical outline:
    '''''' <list type="bullet">
    '''''' <item><description>Center each column: Xc(i,j) = X(i,j) − mean_j.</description></item>
    '''''' <item><description>If correlation PCA: standardize each column: Z(i,j) = (X(i,j) − mean_j)/sd_j (sample sd, n−1).</description></item>
    '''''' <item><description>Form S = Cov(Xc) or Cov(Z): S = (1/(n−1)) * (Xc^T * Xc) (or Z^T * Z).</description></item>
    '''''' <item><description>Eigen-decompose: S v_j = λ_j v_j (λ sorted descending).</description></item>
    '''''' <item><description>Scores (reduced data): T = Xc * V_k (covariance PCA) or T = Z * V_k (correlation PCA).</description></item>
    '''''' </list>
    '''''' </para>
    '''''' <para>
    '''''' Internal dependencies:
    '''''' <list type="bullet">
    '''''' <item><description><c>MatCovar</c> (MatrixType.vb) builds the sample covariance/correlation matrix (divisor n−1).</description></item>
    '''''' <item><description><c>EIGEN_JK</c> (MatrixType.vb) computes eigenpairs via a Jacobi/Kogbetliantz-style iterative orthogonalization.</description></item>
    '''''' <item><description><c>MatrixMult</c> (MatrixType.vb) is used to compute component scores.</description></item>
    '''''' <item><description><c>stDev</c> (StatFunc.vb) provides sample standard deviation used for standardization.</description></item>
    '''''' </list>
    '''''' </para>
    '''''' <para>
    '''''' Comparison to R:
    '''''' <list type="bullet">
    '''''' <item><description>Covariance PCA corresponds to <c>prcomp(X, center=TRUE, scale.=FALSE)</c>.</description></item>
    '''''' <item><description>Correlation PCA corresponds to <c>prcomp(X, center=TRUE, scale.=TRUE)</c>.</description></item>
    '''''' <item><description>R's <c>prcomp</c> uses an SVD-based implementation (often more numerically stable than forming S then eigen).
    '''''' This implementation eigen-decomposes S; results should match up to column sign flips and (for near-tied eigenvalues)
    '''''' possible rotations within the degenerate subspace.</description></item>
    '''''' </list>
    '''''' </para>
    '''''' </remarks>
    '''''' <seealso cref="MatrixType.MatCovar" />
    '''''' <seealso cref="MatrixType.EIGEN_JK" />
    '''''' <seealso cref="MatrixType.MatrixMult" />
    '''''' <seealso cref="StatFunc.stDev" />
    Public Class PCA

        Private pData(,) As Double
        Private pRowNums() As Integer 'observation IDs
        Private pVarNames() As String 'column labels
        Private n As Integer '# of observations
        Private p As Integer '# of parameters
        Private pVarCovar(,) As Double 'variance-convariance matrix
        Private pStandardData(,) As Double 'standardized data
        Private pEigenval() As Double
        Private pEigenvect(,) As Double
        Private pFinalDataset(,) As Double
        Private pLoadings(,) As Double
        Private pMaxiter As Integer
        Private pEps As Double
        Private MatrixType As String 'correlation or covariance
        Private pNoExtractComponents As Integer
        Private pstrExtractMethodLabel As String

        '''''' <summary>
        '''''' Provides the input dataset and associated row/variable labels used by result tables and plots.
        '''''' </summary>
        '''''' <param name="arData">Input data matrix <c>X</c> with shape <c>n × p</c> (rows=observations, columns=variables).</param>
        '''''' <param name="arRowIds">Observation identifiers (length n). Used primarily for labeling score plots.</param>
        '''''' <param name="arVarNames">Variable names (length p). Used for headers and loading/biplot labels.</param>
        '''''' <param name="strExtractMethodLabel">Optional label displayed in titles produced by <see cref="wrapResults"/>.</param>
        '''''' <remarks>
        '''''' <para>Calling this method does not run PCA; it only stores inputs. Call <see cref="Fit"/> to perform the analysis.</para>
        '''''' </remarks>
        Sub dataInputs(arData(,) As Double, arRowIds() As Integer, arVarNames() As String, Optional strExtractMethodLabel As String = "")
            pData = arData
            pRowNums = arRowIds
            pVarNames = arVarNames
            pstrExtractMethodLabel = strExtractMethodLabel
        End Sub

        '''''' <summary>
        '''''' Configures eigen-solver and matrix type settings used by <see cref="Fit"/>.
        '''''' </summary>
        '''''' <param name="maximumIteration">Maximum number of sweeps/iterations passed to <c>EIGEN_JK</c>.</param>
        '''''' <param name="dEps">Convergence tolerance passed to <c>EIGEN_JK</c> (smaller values are stricter).</param>
        '''''' <param name="strAnalyzedMatrixType">Either <c>"Correlation"</c> or <c>"Covariance"</c>. Determines whether PCA is run on standardized variables.</param>
        '''''' <exception cref="System.ArgumentException">Thrown if an unsupported matrix type is provided (recommended to enforce).</exception>
        '''''' <remarks>
        '''''' <para>
        '''''' If <c>strAnalyzedMatrixType</c> is <c>"Correlation"</c>, each variable is standardized using sample SD (n−1).
        '''''' If <c>"Covariance"</c>, variables are only centered.
        '''''' </para>
        '''''' </remarks>
        Sub settingsInputs(maximumIteration As Integer, dEps As Double, strAnalyzedMatrixType As String)
            pMaxiter = maximumIteration
            pEps = dEps
            MatrixType = strAnalyzedMatrixType
        End Sub

        ' Get Values------------------------------------------------------------

        '''''' <summary>
        '''''' Gets the analyzed covariance/correlation matrix S used for PCA.
        '''''' </summary>
        '''''' <returns>A <c>p × p</c> matrix. For covariance PCA: S = Cov(X). For correlation PCA: S = Cor(Z) = Cov(Z).</returns>
        '''''' <remarks>
        '''''' <para>Computed by <c>MatCovar</c> (MatrixType.vb) with sample divisor (n−1).</para>
        '''''' </remarks>
        '''''' <seealso cref="MatrixType.MatCovar" />
        ReadOnly Property VarCovarMat() As Double(,)
            Get
                Return pVarCovar
            End Get
        End Property

        '''''' <summary>
        '''''' Gets the standardized data matrix Z used for correlation PCA.
        '''''' </summary>
        '''''' <returns>An <c>n × p</c> matrix with column means 0 and sample standard deviations 1.</returns>
        '''''' <remarks>
        '''''' <para>Only meaningful when MatrixType = "Correlation"; for covariance PCA this contains the internally produced standardized matrix (may still be computed).</para>
        '''''' </remarks>
        '''''' <seealso cref="StatFunc.stDev" />
        ReadOnly Property StandardizedData() As Double(,)
            Get
                Return pStandardData
            End Get
        End Property

        '''''' <summary>
        '''''' Gets eigenvalues λ (variances explained by each principal component), sorted descending.
        '''''' </summary>
        '''''' <returns>Length p array of non-negative eigenvalues.</returns>
        '''''' <remarks>
        '''''' <para>Eigenvalues are sorted to align with <see cref="Eigenvectors"/> columns and explained variance methods.</para>
        '''''' </remarks>
        ReadOnly Property Eigenvalues() As Double()
            Get
                Return pEigenval
            End Get
        End Property

        '''''' <summary>
        '''''' Gets eigenvectors V whose columns are principal directions (loadings directions).
        '''''' </summary>
        '''''' <returns>A <c>p × p</c> matrix where column j is the eigenvector for <see cref="Eigenvalues"/>(j).</returns>
        '''''' <remarks>
        '''''' <para>Eigenvectors are orthonormal (V^T V ≈ I). The sign of each eigenvector is arbitrary; this class applies a sign convention when creating <see cref="GetLoadings"/>.</para>
        '''''' </remarks>
        ReadOnly Property Eigenvectors() As Double(,)
            Get
                Return pEigenvect
            End Get
        End Property

        '''''' <summary>
        '''''' Gets the reduced dataset (principal component scores).
        '''''' </summary>
        '''''' <returns>An <c>n × k</c> matrix T of scores, where k = <see cref="NoExtractComponents"/>.</returns>
        '''''' <remarks>
        '''''' <para>Scores are computed as T = Xc * V_k (covariance PCA) or T = Z * V_k (correlation PCA).</para>
        '''''' </remarks>
        '''''' <seealso cref="MatrixType.MatrixMult" />
        ReadOnly Property ReducedDataset() As Double(,)
            Get
                Return pFinalDataset
            End Get
        End Property

        '''''' <summary>
        '''''' Gets the selected component loading vectors (principal directions) for the extracted components.
        '''''' </summary>
        '''''' <returns>A <c>p × k</c> matrix V_k, after applying a deterministic sign convention.</returns>
        '''''' <remarks>
        '''''' <para>
        '''''' Each loading column may be multiplied by −1 without changing the PCA solution.
        '''''' This implementation flips a component if its largest-magnitude loading is negative, making results
        '''''' more consistent across runs and easier to compare to other software.
        '''''' </para>
        '''''' </remarks>
        ReadOnly Property GetLoadings() As Double(,)
            Get
                Return pLoadings
            End Get
        End Property

        '''''' <summary>
        '''''' Gets observation identifiers associated with the PCA input rows.
        '''''' </summary>
        ReadOnly Property RowIds() As Integer()
            Get
                Return pRowNums
            End Get
        End Property

        '''''' <summary>
        '''''' Gets variable names associated with the PCA input columns.
        '''''' </summary>
        ReadOnly Property VariableNames() As String()
            Get
                Return pVarNames
            End Get
        End Property

        '''''' <summary>
        '''''' Gets the number of observations used in the PCA fit.
        '''''' </summary>
        ReadOnly Property ObservationCount() As Integer
            Get
                Return n
            End Get
        End Property

        '''''' <summary>
        '''''' Gets the number of variables used in the PCA fit.
        '''''' </summary>
        ReadOnly Property VariableCount() As Integer
            Get
                Return p
            End Get
        End Property

        '''''' <summary>
        '''''' Gets the number of extracted principal components k.
        '''''' </summary>
        '''''' <returns>An integer in [1, p] determined by the extraction rule used in <see cref="Fit"/>.</returns>
        ReadOnly Property NoExtractComponents() As Integer
            Get
                Return pNoExtractComponents
            End Get
        End Property

        '''''' <summary>
        '''''' Gets the percentage of total variance explained by each component.
        '''''' </summary>
        '''''' <returns>Length p array: 100 * λ_j / Σλ.</returns>
        '''''' <remarks>
        '''''' <para>Uses the eigenvalues in <see cref="Eigenvalues"/>. Values sum to ~100 (floating point tolerance).</para>
        '''''' </remarks>
        ReadOnly Property PercentExpl() As Double()
            Get
                Dim out(p - 1) As Double
                Dim tot As Double = pEigenval.Sum()
                For i As Integer = 0 To p - 1
                    out(i) = 100.0 * pEigenval(i) / tot
                Next
                Return out
            End Get
        End Property

        '''''' <summary>
        '''''' Gets the cumulative percentage of total variance explained by components 1..j.
        '''''' </summary>
        '''''' <returns>Length p array where element j is Σ_{i<=j} PercentExpl(i).</returns>
        ReadOnly Property PercentExplCum() As Double()
            Get
                Dim prc() As Double = Me.PercentExpl
                Dim cum As Double = 0
                Dim out(p - 1) As Double
                For i As Integer = 0 To p - 1
                    cum += prc(i)
                    out(i) = cum
                Next
                Return out
            End Get
        End Property

        '''''' <summary>
        '''''' Gets 1-based component indices for plotting (1..p).
        '''''' </summary>
        '''''' <returns>Integer array of length p containing [1, 2, ..., p].</returns>
        ReadOnly Property XaxisComponents() As Integer()
            Get
                Dim out(p - 1) As Integer
                For i As Integer = 0 To p - 1 : out(i) = i + 1 : Next i
                Return out
            End Get
        End Property

        '''''' <summary>
        '''''' Gets display names for extracted components (PC1..PCk), optionally prefixed.
        '''''' </summary>
        '''''' <param name="prefix">Optional prefix to prepend to each name (e.g., "PC" or "Component ").</param>
        '''''' <returns>String array of length k with names like "PC1", "PC2", ...</returns>
        '''''' <remarks>
        '''''' <para>Uses <see cref="NoExtractComponents"/> to determine k.</para>
        '''''' </remarks>
        ReadOnly Property PCnames(Optional prefix As String = "") As String()
            Get
                Dim out(pNoExtractComponents - 1) As String
                For i As Integer = 0 To pNoExtractComponents - 1
                    out(i) = prefix & CStr(i + 1)
                Next
                Return out
            End Get
        End Property

        '''''' <summary>
        '''''' Builds a list of tabular outputs describing the PCA results.
        '''''' </summary>
        '''''' <returns>A list of <c>ResultTable</c> objects, typically including: analyzed matrix, eigenvectors, eigenvalues, explained variance, and selected loadings.</returns>
        '''''' <remarks>
        '''''' <para>
        '''''' Tables are constructed using the external <c>ResultTable</c> type. The exact formatting depends on
        '''''' <c>ResultTable</c> implementation.
        '''''' </para>
        '''''' <para>
        '''''' Call <see cref="Fit"/> before calling this method.
        '''''' </para>
        '''''' </remarks>
        '''''' <seealso cref="Fit" />
        Public Function wrapResults() As List(Of ResultTable)
            Dim out As New List(Of ResultTable)
            Dim t = New ResultTable

            'correlation/covariance table
            t.SetBody(Me.pVarCovar)
            t.AddHeaderTopRow(Me.pVarNames)
            t.AddHeaderLeftRow(Me.pVarNames)
            If MatrixType = "Correlation" Then
                t.AddTitle("Correlation MatrixType")
            Else
                t.AddTitle("Variance-Covariance MatrixType")
            End If
            out.Add(t)

            'Eigenvectors
            t = New ResultTable
            t.SetBody(Me.pEigenvect)
            t.AddTitle("Eigenvectors")
            t.AddHeaderLeftRow(Me.pVarNames)
            out.Add(t)

            'Eigenvalues
            t = New ResultTable
            t.AddHeaderLeftRow({"Eigenvalues", "% Variance Explained", "Cumulative % Explained"})
            Dim o(2, Me.pVarNames.Length - 1) As Object
            For i = 0 To Me.pVarNames.Length - 1
                o(0, i) = Me.pEigenval(i)
                o(1, i) = Me.PercentExpl(i)
                o(2, i) = Me.PercentExplCum(i)
            Next
            t.SetBody(o)
            out.Add(t)

            'Selected Components
            t = New ResultTable
            t.AddTitle($"Selected Component Loadings (Method: {Me.pstrExtractMethodLabel})")
            t.AddHeaderLeftRow(Me.pVarNames)
            t.AddHeaderTopRow(Me.PCnames("PC"))
            t.SetBody(Me.GetLoadings)
            out.Add(t)

            Return out
        End Function

        '''''' <summary>
        '''''' Runs PCA end-to-end: preprocessing, covariance/correlation computation, eigen-decomposition, component selection, loadings, and scores.
        '''''' </summary>
        '''''' <param name="extract_method">Extraction rule: "Eigenvalue", "Fixed", or "Variance".</param>
        '''''' <param name="extract_coef">Method parameter: eigenvalue cutoff (Eigenvalue), number of components (Fixed), or target cumulative variance percent (Variance).</param>
        '''''' <exception cref="System.InvalidOperationException">Thrown if input data/labels are missing or inconsistent (recommended).</exception>
        '''''' <exception cref="System.ArgumentException">Thrown for unsupported extract method or invalid coefficient values.</exception>
        '''''' <remarks>
        '''''' <para>
        '''''' Algorithm:
        '''''' <list type="number">
        '''''' <item><description>Determine matrix dimensions n and p from the input data.</description></item>
        '''''' <item><description>Center (and optionally standardize) each variable.</description></item>
        '''''' <item><description>Compute analyzed matrix S using <c>MatCovar</c>.</description></item>
        '''''' <item><description>Compute eigenpairs (λ, V) using <c>EIGEN_JK</c>, then sort λ descending and reorder V accordingly.</description></item>
        '''''' <item><description>Select k according to <paramref name="extract_method"/> and <paramref name="extract_coef"/>, clamped to [1, p].</description></item>
        '''''' <item><description>Form loadings V_k from the first k eigenvectors and apply a sign convention.</description></item>
        '''''' <item><description>Compute scores T = Xc·V_k (covariance PCA) or Z·V_k (correlation PCA) using <c>MatrixMult</c>.</description></item>
        '''''' </list>
        '''''' </para>
        '''''' <para>
        '''''' Notes on numerical comparability:
        '''''' <list type="bullet">
        '''''' <item><description>Eigenvector signs may differ from R/Excel; scores will differ by the same sign per component.</description></item>
        '''''' <item><description>When eigenvalues are nearly tied, component directions may vary by an orthonormal rotation within the tied subspace.</description></item>
        '''''' </list>
        '''''' </para>
        '''''' </remarks>
        '''''' <seealso cref="MatrixType.MatCovar" />
        '''''' <seealso cref="MatrixType.EIGEN_JK" />
        '''''' <seealso cref="MatrixType.MatrixMult" />
        '''''' <seealso cref="StatFunc.stDev" />
        Public Sub Calculate(extract_method As String, extract_coef As Double)

            Me.p = UBound(pData, 2) + 1 'Columns of predictor variables and responses
            Me.n = UBound(pData, 1) + 1   '# of observations
            ReDim pStandardData(n - 1, p - 1), pEigenvect(p - 1, p - 1)
            Dim CenteredData(n - 1, p - 1) As Double

            '1. standardize and center data
            '   center data - (required only for covariance matrix but we always compute it)
            For i As Integer = 0 To p - 1
                Dim tmp() As Double = Matrix.GetColumnFrom2Darray(Me.pData, i)
                Dim tmp2() As Double = MultivariateShared.Center(tmp)
                tmp = MultivariateShared.Standardize(tmp)

                For j As Integer = 0 To n - 1
                    pStandardData(j, i) = tmp(j)
                    CenteredData(j, i) = tmp2(j)
                Next
            Next

            '2. get variance-covariance matrix
            If MatrixType = "Correlation" Then
                Me.pVarCovar = Matrix.MatCovar(Me.pStandardData)
            Else 'Covariance matrix
                Me.pVarCovar = Matrix.MatCovar(CenteredData)
            End If

            '3. Compute the eigenvectors and eigenvalues
            'The 1st column of the eigen_raw matrix contains eigenvalues and the rest of the p+1 columns are eigenvectors
            Dim eigen_raw = Matrix.EIGEN_JK(pVarCovar, pMaxiter, pEps)
            Dim sorted = MultivariateShared.SortEigenpairsDescending(eigen_raw.Item1, eigen_raw.Item2)
            Me.pEigenval = sorted.Item1
            Me.pEigenvect = sorted.Item2

            '4. Reduced Model
            'How many components to extract?
            If extract_method = "Eigenvalue" Then
                pNoExtractComponents = pEigenval.TakeWhile(Function(x) x >= extract_coef).Count()

            ElseIf extract_method = "Fixed" Then
                pNoExtractComponents = CInt(extract_coef)

            ElseIf extract_method = "Variance" Then
                Dim i As Integer
                For i = 0 To p - 1
                    If PercentExplCum(i) >= extract_coef Then Exit For
                Next
                pNoExtractComponents = i + 1

            End If

            'Sanity check. We always want to extract at least one component
            pNoExtractComponents = Math.Max(1, Math.Min(p, pNoExtractComponents))

            'create Feature vector/matrix
            ReDim pLoadings(p - 1, pNoExtractComponents - 1)
            For j As Integer = 0 To pNoExtractComponents - 1
                Dim col() As Double = Matrix.GetColumnFrom2Darray(pEigenvect, j)
                Dim minv As Double = col.Min()
                Dim maxv As Double = col.Max()
                Dim flip As Boolean = If(Math.Abs(minv) > Math.Abs(maxv), (minv < 0), (maxv < 0))
                For i As Integer = 0 To p - 1
                    pLoadings(i, j) = If(flip, -pEigenvect(i, j), pEigenvect(i, j))
                Next
            Next


            If MatrixType = "Correlation" Then
                pFinalDataset = Matrix.MatrixMult(pStandardData, pLoadings)
            Else
                pFinalDataset = Matrix.MatrixMult(CenteredData, pLoadings)
            End If

        End Sub

        '''''' <summary>
        '''''' Builds standardized variable name labels (e.g., "Standardized_Height").
        '''''' </summary>
        '''''' <param name="varNames">Original variable names.</param>
        '''''' <returns>A new array where each element is prefixed with "Standardized_".</returns>
        '''''' <remarks>
        '''''' <para>Used for labeling standardized variables in outputs; does not modify PCA computation.</para>
        '''''' </remarks>
        Public Function StandardizedVarNames(varNames() As String) As String()
            Dim varOut(UBound(varNames)) As String
            For i As Integer = 0 To UBound(varNames)
                varOut(i) = "Standardized_" & varNames(i)
            Next
            Return varOut
        End Function

    End Class
End Namespace