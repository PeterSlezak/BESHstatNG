Option Explicit On
Imports Microsoft.Office.Interop.Excel
Imports System.Linq

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
    '''''' <item><description><c>MatCovar</c> (Matrix.vb) builds the sample covariance/correlation matrix (divisor n−1).</description></item>
    '''''' <item><description><c>EIGEN_JK</c> (Matrix.vb) computes eigenpairs via a Jacobi/Kogbetliantz-style iterative orthogonalization.</description></item>
    '''''' <item><description><c>MatrixMult</c> (Matrix.vb) is used to compute component scores.</description></item>
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
    '''''' <seealso cref="Matrix.MatCovar" />
    '''''' <seealso cref="Matrix.EIGEN_JK" />
    '''''' <seealso cref="Matrix.MatrixMult" />
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
        Private Matrix As String 'correlation or covariance
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
        '''''' <para>Calling this method does not run PCA; it only stores inputs. Call <see cref="Calculate"/> to perform the analysis.</para>
        '''''' </remarks>
        Sub dataInputs(arData(,) As Double, arRowIds() As Integer, arVarNames() As String, Optional strExtractMethodLabel As String = "")
            pData = arData
            pRowNums = arRowIds
            pVarNames = arVarNames
            pstrExtractMethodLabel = strExtractMethodLabel
        End Sub

        '''''' <summary>
        '''''' Configures eigen-solver and matrix type settings used by <see cref="Calculate"/>.
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
            Matrix = strAnalyzedMatrixType
        End Sub

        ' Get Values------------------------------------------------------------

        '''''' <summary>
        '''''' Gets the analyzed covariance/correlation matrix S used for PCA.
        '''''' </summary>
        '''''' <returns>A <c>p × p</c> matrix. For covariance PCA: S = Cov(X). For correlation PCA: S = Cor(Z) = Cov(Z).</returns>
        '''''' <remarks>
        '''''' <para>Computed by <c>MatCovar</c> (Matrix.vb) with sample divisor (n−1).</para>
        '''''' </remarks>
        '''''' <seealso cref="Matrix.MatCovar" />
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
        '''''' <para>Only meaningful when Matrix = "Correlation"; for covariance PCA this contains the internally produced standardized matrix (may still be computed).</para>
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
        '''''' <seealso cref="Matrix.MatrixMult" />
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
        '''''' Gets the number of extracted principal components k.
        '''''' </summary>
        '''''' <returns>An integer in [1, p] determined by the extraction rule used in <see cref="Calculate"/>.</returns>
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
        '''''' Call <see cref="Calculate"/> before calling this method.
        '''''' </para>
        '''''' </remarks>
        '''''' <seealso cref="Calculate" />
        Public Function wrapResults() As List(Of ResultTable)
            Dim out As New List(Of ResultTable)
            Dim t = New ResultTable

            'correlation/covariance table
            t.SetBody(Me.pVarCovar)
            t.AddHeaderTopRow(Me.pVarNames)
            t.AddHeaderLeftRow(Me.pVarNames)
            If Matrix = "Correlation" Then
                t.AddTitle("Correlation Matrix")
            Else
                t.AddTitle("Variance-Covariance Matrix")
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
        '''''' <seealso cref="Matrix.MatCovar" />
        '''''' <seealso cref="Matrix.EIGEN_JK" />
        '''''' <seealso cref="Matrix.MatrixMult" />
        '''''' <seealso cref="StatFunc.stDev" />
        Public Sub Calculate(extract_method As String, extract_coef As Double)

            Me.p = UBound(pData, 2) + 1 'Columns of predictor variables and responses
            Me.n = UBound(pData, 1) + 1   '# of observations
            ReDim pStandardData(n - 1, p - 1), pEigenvect(p - 1, p - 1)
            Dim CenteredData(n - 1, p - 1) As Double

            '1. standardize and center data
            '   center data - (required only for covariance matrix but we always compute it)
            For i As Integer = 0 To p - 1
                Dim tmp() As Double = GetColumnFrom2Darray(Me.pData, i)
                Dim tmp2() As Double = center(tmp)
                tmp = standardize(tmp)

                For j As Integer = 0 To n - 1
                    pStandardData(j, i) = tmp(j)
                    CenteredData(j, i) = tmp2(j)
                Next
            Next

            '2. get variance-covariance matrix
            If Matrix = "Correlation" Then
                Me.pVarCovar = MatCovar(Me.pStandardData)
            Else 'Covariance matrix
                Me.pVarCovar = MatCovar(CenteredData)
            End If

            '3. Compute the eigenvectors and eigenvalues
            'The 1st column of the eigen_raw matrix contains eigenvalues and the rest of the p+1 columns are eigenvectors
            Dim eigen_raw = EIGEN_JK(pVarCovar, pMaxiter, pEps)
            Dim sorted = SortEigenpairsDescending(eigen_raw.Item1, eigen_raw.Item2)
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
                Dim col() As Double = GetColumnFrom2Darray(pEigenvect, j)
                Dim minv As Double = col.Min()
                Dim maxv As Double = col.Max()
                Dim flip As Boolean = If(Math.Abs(minv) > Math.Abs(maxv), (minv < 0), (maxv < 0))
                For i As Integer = 0 To p - 1
                    pLoadings(i, j) = If(flip, -pEigenvect(i, j), pEigenvect(i, j))
                Next
            Next


            If Matrix = "Correlation" Then
                pFinalDataset = MatrixMult(pStandardData, pLoadings)
            Else
                pFinalDataset = MatrixMult(CenteredData, pLoadings)
            End If

        End Sub

        '''''' <summary>
        '''''' Creates a 3D scatter plot of variable loadings on PC1, PC2, and PC3.
        '''''' </summary>
        '''''' <exception cref="System.InvalidOperationException">Thrown if fewer than 3 components are available or PCA has not been computed.</exception>
        '''''' <remarks>
        '''''' <para>
        '''''' These plotting methods use <c>Microsoft.Office.Interop.Excel</c> and assume an Excel <c>Application</c>
        '''''' instance named <c>app</c> is available in scope. They create a new chart in the active workbook.
        '''''' </para>
        '''''' <para>
        '''''' Call <see cref="Calculate"/> before plotting.
        '''''' </para>
        '''''' <para>Uses <see cref="GetLoadings"/> and <see cref="PercentExpl"/> for axis labeling.</para>
        '''''' </remarks>
        Public Sub loadingPlot3D()

            If pNoExtractComponents < 3 Then Exit Sub

            Dim XYZ As New graphics.XYZscatter
            Dim pc1() As Double = GetColumnFrom2Darray(pLoadings, 0)
            Dim pc2() As Double = GetColumnFrom2Darray(pLoadings, 1)
            Dim pc3() As Double = GetColumnFrom2Darray(pLoadings, 2)

            With XYZ
                .ChartName = "Loadings Plot3D"
                .dataInputs(pc1, pc2, pc3)
                .axesLabelInputs($"1st Component Scores [{ Format$(PercentExpl(0), "#0.0#") }%]",
                             $"2nd Component Scores [{ Format$(PercentExpl(1), "#0.0#") }%]",
                             $"3rd Component Scores [{ Format$(PercentExpl(2), "#0.0#") }%]")
                .showPlanePointInputs(True, True, True, 3, 3, 3)
                .ScaleAxis(False)
                .settingsInputs(True, True, True)
                .SetDataLabels(pVarNames)
                .draw()
            End With
        End Sub

        '''''' <summary>
        '''''' Creates a 2D scatter plot of variable loadings on PC1 vs PC2.
        '''''' </summary>
        '''''' <exception cref="System.InvalidOperationException">Thrown if fewer than 2 components are available or PCA has not been computed.</exception>
        '''''' <remarks>
        '''''' <para>
        '''''' These plotting methods use <c>Microsoft.Office.Interop.Excel</c> and assume an Excel <c>Application</c>
        '''''' instance named <c>app</c> is available in scope. They create a new chart in the active workbook.
        '''''' </para>
        '''''' <para>
        '''''' Call <see cref="Calculate"/> before plotting.
        '''''' </para>
        '''''' <para>Uses <see cref="GetLoadings"/> and <see cref="PercentExpl"/> for axis labeling.</para>
        '''''' </remarks>
        Public Sub loadingPlot2D()

            If pNoExtractComponents < 2 Then Exit Sub

            Dim pc1() As Double = GetColumnFrom2Darray(pLoadings, 0)
            Dim pc2() As Double = GetColumnFrom2Darray(pLoadings, 1)
            Dim scl1 As Double = Math.Max(Math.Abs(pc1.Min()), Math.Abs(pc1.Max()))
            Dim scl2 As Double = Math.Max(Math.Abs(pc2.Min()), Math.Abs(pc2.Max()))
            Dim udAxisX As graphics.CHARTscale = graphics.ChartScaling(-scl1, scl1)
            Dim udAxisY As graphics.CHARTscale = graphics.ChartScaling(-scl2, scl2)

            app.Charts.Add()
            With app.ActiveWorkbook.ActiveChart
                .Name = "Loadings Plot2D"
                .ChartType = XlChartType.xlXYScatter

                'delete extra series
                Do Until .SeriesCollection.Count = 0
                    .SeriesCollection(1).Delete
                Loop

                With .Axes(XlAxisType.xlCategory)
                    .MinimumScale = udAxisX.Min
                    .MaximumScale = udAxisX.Max
                    .MajorUnit = udAxisX.Scale
                    .CrossesAt = -1.0E+100
                    .MajorTickMark = XlTickMark.xlTickMarkOutside
                    .MajorGridlines.Delete
                End With
                With .Axes(XlAxisType.xlValue)
                    .CrossesAt = -1.0E+100
                    .MinimumScale = udAxisY.Min
                    .MaximumScale = udAxisY.Max
                    .MajorUnit = udAxisY.Scale
                    .MajorTickMark = XlTickMark.xlTickMarkOutside
                    .MajorGridlines.Delete
                End With

                Dim series_id As Integer = 0
                For id As Integer = 0 To p - 1
                    .SeriesCollection.NewSeries
                    series_id += 1
                    With .SeriesCollection(series_id)
                        .ChartType = XlChartType.xlXYScatterLinesNoMarkers
                        .XValues = {0, pc1(id)}
                        .Values = {0, pc2(id)}
                        .Name = "Loading_" & CStr(id)
                        .Format.Line.Weight = 1
                        .Format.Line.Visible = True
                        .Format.Line.ForeColor.RGB = RGB(0, 0, 150)
                        .Format.Line.EndArrowheadStyle = 2 'msoArrowheadTriangle

                        'Attach a label
                        .points(2).HasDataLabel = True
                        .points(2).DataLabel.text = CStr(pVarNames(id))
                        .points(2).DataLabel.Position = XlDataLabelPosition.xlLabelPositionAbove
                        .points(2).DataLabel.Font.Size = 11
                        .points(2).DataLabel.Font.Color = RGB(0, 0, 150)
                    End With
                Next id

                'add zero lines
                .SeriesCollection.NewSeries
                series_id += 1
                With .SeriesCollection(series_id)
                    .XValues = {udAxisX.Min, udAxisX.Max}
                    .Values = {0, 0}
                    .Name = "Y Zero Line"
                    .MarkerStyle = -4142
                    .Border.Color = RGB(0, 0, 0)
                    With .Format.Line
                        .Visible = True
                        .Weight = 1
                    End With
                End With
                .SeriesCollection.NewSeries
                series_id += 1
                With .SeriesCollection(series_id)
                    .XValues = {0, 0}
                    .Values = {udAxisY.Min, udAxisY.Max}
                    .Name = "X Zero Line"
                    .MarkerStyle = -4142
                    .Border.Color = RGB(0, 0, 0)
                    With .Format.Line
                        .Visible = True
                        .Weight = 1
                    End With
                End With

                Try
                    .Legend.Delete()
                Catch
                End Try

                .Axes(XlAxisType.xlValue, XlAxisGroup.xlPrimary).HasTitle = False
                .Axes(XlAxisType.xlValue, XlAxisGroup.xlPrimary).HasTitle = True
                .Axes(XlAxisType.xlValue, XlAxisGroup.xlPrimary).AxisTitle.text = $"2nd Component Scores [{ Format$(PercentExpl(1), "#0.0#") }%]"
                .Axes(XlAxisType.xlValue, XlAxisGroup.xlPrimary).AxisTitle.Font.Size = 16
                .Axes(XlAxisType.xlValue, XlAxisGroup.xlPrimary).TickLabels.Font.Size = 14
                .Axes(XlAxisType.xlCategory, XlAxisGroup.xlPrimary).HasTitle = False
                .Axes(XlAxisType.xlCategory, XlAxisGroup.xlPrimary).HasTitle = True
                .Axes(XlAxisType.xlCategory, XlAxisGroup.xlPrimary).AxisTitle.text = $"1st Component Scores [{ Format$(PercentExpl(0), "#0.0#") }%]"
                .Axes(XlAxisType.xlCategory, XlAxisGroup.xlPrimary).AxisTitle.Font.Size = 16
                .Axes(XlAxisType.xlCategory, XlAxisGroup.xlPrimary).TickLabels.Font.Size = 14
                .HasTitle = False
                .HasTitle = True
                .ChartTitle.Text = "Component Loadings Plot"
                .ChartTitle.Font.Size = 18
                .ChartTitle.Font.Bold = True
            End With
        End Sub

        '''''' <summary>
        '''''' Creates a 3D scatter plot of observation scores on PC1, PC2, and PC3.
        '''''' </summary>
        '''''' <exception cref="System.InvalidOperationException">Thrown if fewer than 3 components are available or PCA has not been computed.</exception>
        '''''' <remarks>
        '''''' <para>
        '''''' These plotting methods use <c>Microsoft.Office.Interop.Excel</c> and assume an Excel <c>Application</c>
        '''''' instance named <c>app</c> is available in scope. They create a new chart in the active workbook.
        '''''' </para>
        '''''' <para>
        '''''' Call <see cref="Calculate"/> before plotting.
        '''''' </para>
        '''''' <para>Uses <see cref="ReducedDataset"/> and <see cref="PercentExpl"/> for axis labeling.</para>
        '''''' </remarks>
        Public Sub scorePlot3D()

            If pNoExtractComponents < 3 Then Exit Sub

            Dim XYZ As New graphics.XYZscatter
            Dim pc1() As Double = GetColumnFrom2Darray(pFinalDataset, 0)
            Dim pc2() As Double = GetColumnFrom2Darray(pFinalDataset, 1)
            Dim pc3() As Double = GetColumnFrom2Darray(pFinalDataset, 2)

            Dim rownums_str(n - 1) As String
            For i = 0 To n - 1
                rownums_str(i) = CStr(pRowNums(i))
            Next

            With XYZ
                .ChartName = "Score Plot3D"
                .dataInputs(pc1, pc2, pc3)
                .axesLabelInputs($"1St Component Scores [{ Format$(PercentExpl(0), "#0.0#") }%]",
                             $"2nd Component Scores [{ Format$(PercentExpl(1), "#0.0#") }%]",
                             $"3Rd Component Scores [{ Format$(PercentExpl(2), "#0.0#") }%]")
                .showPlanePointInputs(True, True, True, 3, 3, 3)
                .ScaleAxis(False)
                .settingsInputs(True, True, True)
                .SetDataLabels(rownums_str)
                .draw
            End With
        End Sub

        '''''' <summary>
        '''''' Creates a 2D scatter plot of observation scores on PC1 vs PC2.
        '''''' </summary>
        '''''' <exception cref="System.InvalidOperationException">Thrown if fewer than 2 components are available or PCA has not been computed.</exception>
        '''''' <remarks>
        '''''' <para>
        '''''' These plotting methods use <c>Microsoft.Office.Interop.Excel</c> and assume an Excel <c>Application</c>
        '''''' instance named <c>app</c> is available in scope. They create a new chart in the active workbook.
        '''''' </para>
        '''''' <para>
        '''''' Call <see cref="Calculate"/> before plotting.
        '''''' </para>
        '''''' <para>Uses <see cref="ReducedDataset"/> and <see cref="PercentExpl"/> for axis labeling. Points may be labeled using row IDs.</para>
        '''''' </remarks>
        Public Sub scorePlot2D()

            If pNoExtractComponents < 2 Then Exit Sub

            Dim pc1() As Double = GetColumnFrom2Darray(pFinalDataset, 0)
            Dim pc2() As Double = GetColumnFrom2Darray(pFinalDataset, 1)
            Dim udAxisX As graphics.CHARTscale = graphics.ChartScaling(pc1.Min(), pc1.Max())
            Dim udAxisY As graphics.CHARTscale = graphics.ChartScaling(pc2.Min(), pc2.Max())

            app.Charts.Add()

            With app.ActiveWorkbook.ActiveChart
                .Name = "Score Plot2D"
                .ChartType = XlChartType.xlXYScatter

                'delete extra series
                Do Until .SeriesCollection.Count = 0
                    .SeriesCollection(1).Delete
                Loop

                With .Axes(XlAxisType.xlCategory)
                    .MinimumScale = udAxisX.Min
                    .MaximumScale = udAxisX.Max
                    .MajorUnit = udAxisX.Scale
                    .CrossesAt = -1.0E+100
                    .MajorTickMark = XlTickMark.xlTickMarkOutside
                    .MajorGridlines.Delete
                End With
                With .Axes(XlAxisType.xlValue)
                    .CrossesAt = -1.0E+100
                    .MinimumScale = udAxisY.Min
                    .MaximumScale = udAxisY.Max
                    .MajorUnit = udAxisY.Scale
                    .MajorTickMark = XlTickMark.xlTickMarkOutside
                    .MajorGridlines.Delete
                End With

                .SeriesCollection.NewSeries
                With .SeriesCollection(1)
                    .XValues = pc1
                    .Values = pc2
                    .Name = "Score plot"
                    '.Format.Line.Weight = 1.5
                    .MarkerStyle = 8
                    .MarkerSize = 5
                    .MarkerForegroundColor = RGB(100, 100, 100)
                    .Format.Fill.Visible = False

                    'Attach a label to each data point in the chart.
                    For i = 1 To n
                        .points(i).HasDataLabel = True
                        .points(i).DataLabel.text = CStr(pRowNums(i - 1))
                        .points(i).DataLabel.Position = XlDataLabelPosition.xlLabelPositionAbove
                        .points(i).DataLabel.Font.Size = 8
                    Next
                End With

                'add zero lines
                .SeriesCollection.NewSeries
                With .SeriesCollection(2)
                    .XValues = {udAxisX.Min, udAxisX.Max}
                    .Values = {0, 0}
                    .Name = "Y Zero Line"
                    .MarkerStyle = -4142
                    .Border.Color = RGB(0, 0, 0)
                    With .Format.Line
                        .Visible = True
                        .Weight = 1
                    End With
                End With
                .SeriesCollection.NewSeries
                With .SeriesCollection(3)
                    .XValues = {0, 0}
                    .Values = {udAxisY.Min, udAxisY.Max}
                    .Name = "X Zero Line"
                    .MarkerStyle = -4142
                    .Border.Color = RGB(0, 0, 0)
                    With .Format.Line
                        .Visible = True
                        .Weight = 1
                    End With
                End With

                Try
                    .Legend.Delete()
                Catch
                End Try

                .Axes(XlAxisType.xlValue, XlAxisGroup.xlPrimary).HasTitle = False
                .Axes(XlAxisType.xlValue, XlAxisGroup.xlPrimary).HasTitle = True
                .Axes(XlAxisType.xlValue, XlAxisGroup.xlPrimary).AxisTitle.text = $"2nd Component Scores [{ Format$(PercentExpl(1), "#0.0#") }%]"
                .Axes(XlAxisType.xlValue, XlAxisGroup.xlPrimary).AxisTitle.Font.Size = 16
                .Axes(XlAxisType.xlValue, XlAxisGroup.xlPrimary).TickLabels.Font.Size = 14
                .Axes(XlAxisType.xlCategory, XlAxisGroup.xlPrimary).HasTitle = False
                .Axes(XlAxisType.xlCategory, XlAxisGroup.xlPrimary).HasTitle = True
                .Axes(XlAxisType.xlCategory, XlAxisGroup.xlPrimary).AxisTitle.text = $"1St Component Scores [{ Format$(PercentExpl(0), "#0.0#") }%]"
                .Axes(XlAxisType.xlCategory, XlAxisGroup.xlPrimary).AxisTitle.Font.Size = 16
                .Axes(XlAxisType.xlCategory, XlAxisGroup.xlPrimary).TickLabels.Font.Size = 14
                .HasTitle = False
                .HasTitle = True
                .ChartTitle.Text = "Scores Plot"
                .ChartTitle.Font.Size = 18
                .ChartTitle.Font.Bold = True
            End With
        End Sub

        '''''' <summary>
        '''''' Creates a PCA biplot (scores + loading vectors) in the PC1/PC2 plane.
        '''''' </summary>
        '''''' <param name="c">Scaling factor for loading vectors (larger values draw longer arrows).</param>
        '''''' <exception cref="System.InvalidOperationException">Thrown if fewer than 2 components are available or PCA has not been computed.</exception>
        '''''' <remarks>
        '''''' <para>
        '''''' These plotting methods use <c>Microsoft.Office.Interop.Excel</c> and assume an Excel <c>Application</c>
        '''''' instance named <c>app</c> is available in scope. They create a new chart in the active workbook.
        '''''' </para>
        '''''' <para>
        '''''' Call <see cref="Calculate"/> before plotting.
        '''''' </para>
        '''''' <para>Plots scores from <see cref="ReducedDataset"/> and loadings from <see cref="GetLoadings"/>.</para>
        '''''' </remarks>
        Public Sub biplot(Optional c As Double = 1.0)
            Dim series_id As Integer, lam(1) As Double, titl As String = String.Empty

            If pNoExtractComponents < 2 Then Exit Sub
            If c < 0.0 Or c > 1.0 Then BSerr.LogAndThrow(New ArgumentException("biplot 'scale' is outside of range [0, 1]"))

            If c = 0.0 Then
                titl = "GH, or column-metric preserving"
            ElseIf c = 1.0 Then
                titl = "JK, or row-metric preserving"
            ElseIf c = 0.5 Then
                titl = "SQ, or symmetric"
            End If

            'Get data to present - loadings and scores for the first two components
            Dim pc1() As Double = GetColumnFrom2Darray(pFinalDataset, 0)
            Dim pc2() As Double = GetColumnFrom2Darray(pFinalDataset, 1)
            Dim Load1() As Double = GetColumnFrom2Darray(pLoadings, 0)
            Dim Load2() As Double = GetColumnFrom2Darray(pLoadings, 1)

            'Scale the data
            For i = 0 To 1
                lam(i) = Math.Sqrt(pEigenval(i)) * Math.Sqrt(n)
                lam(i) = lam(i) ^ (1.0 - c)
            Next

            For i = 0 To n - 1
                pc1(i) /= lam(0)
                pc2(i) /= lam(1)
            Next

            For i = 0 To p - 1
                Load1(i) *= lam(0)
                Load2(i) *= lam(1)
            Next

            'Axis scaling
            Dim udAxisX As graphics.CHARTscale = graphics.ChartScaling(Math.Min(pc1.Min(), Load1.Min()), Math.Max(pc1.Max(), Load1.Max()))
            Dim udAxisY As graphics.CHARTscale = graphics.ChartScaling(Math.Min(pc2.Min(), Load2.Min()), Math.Max(pc2.Max(), Load2.Max()))

            'Create chart
            app.Charts.Add()
            With app.ActiveWorkbook.ActiveChart
                .Name = "Biplot scale=" & CStr(c)
                .ChartType = XlChartType.xlXYScatter

                'delete extra series
                Do Until .SeriesCollection.Count = 0
                    .SeriesCollection(1).Delete
                Loop

                With .Axes(XlAxisType.xlCategory)
                    .MinimumScale = udAxisX.Min
                    .MaximumScale = udAxisX.Max
                    .MajorUnit = udAxisX.Scale
                    .CrossesAt = -1.0E+100
                    .MajorTickMark = XlTickMark.xlTickMarkOutside
                    .MajorGridlines.Delete
                End With
                With .Axes(XlAxisType.xlValue)
                    .CrossesAt = -1.0E+100
                    .MinimumScale = udAxisY.Min
                    .MaximumScale = udAxisY.Max
                    .MajorUnit = udAxisY.Scale
                    .MajorTickMark = XlTickMark.xlTickMarkOutside
                    .MajorGridlines.Delete
                End With

                .SeriesCollection.NewSeries
                series_id = 1
                With .SeriesCollection(series_id)
                    .XValues = pc1
                    .Values = pc2
                    .Name = "Biplot: " & titl
                    .MarkerStyle = 8
                    .MarkerSize = 5
                    .MarkerForegroundColor = RGB(100, 100, 100)
                    .Format.Fill.Visible = False

                    'Attach a label to each data point in the chart.
                    For i = 1 To n
                        .points(i).HasDataLabel = True
                        .points(i).DataLabel.text = CStr(pRowNums(i - 1))
                        .points(i).DataLabel.Position = XlDataLabelPosition.xlLabelPositionAbove
                        .points(i).DataLabel.Font.Size = 8
                    Next i
                End With

                For id = 0 To p - 1
                    .SeriesCollection.NewSeries
                    series_id += 1
                    With .SeriesCollection(series_id)
                        .ChartType = XlChartType.xlXYScatterLinesNoMarkers
                        .XValues = {0, Load1(id)}
                        .Values = {0, Load2(id)}
                        .Name = "Loading_" & CStr(id)
                        .Format.Line.Weight = 1
                        .Format.Line.Visible = True
                        .Format.Line.ForeColor.RGB = RGB(0, 0, 150)
                        .Format.Line.EndArrowheadStyle = 2 'msoArrowheadTriangle

                        'Attach a label
                        .points(2).HasDataLabel = True
                        .points(2).DataLabel.text = CStr(pVarNames(id))
                        .points(2).DataLabel.Position = XlDataLabelPosition.xlLabelPositionAbove
                        .points(2).DataLabel.Font.Size = 11
                        .points(2).DataLabel.Font.Color = RGB(0, 0, 150)
                    End With
                Next id

                'add zero lines
                .SeriesCollection.NewSeries
                series_id = series_id + 1
                With .SeriesCollection(series_id)
                    .XValues = {udAxisX.Min, udAxisX.Max}
                    .Values = {0, 0}
                    .Name = "Y Zero Line"
                    .MarkerStyle = -4142
                    .Border.Color = RGB(0, 0, 0)
                    With .Format.Line
                        .Visible = True
                        .Weight = 1
                    End With
                End With
                .SeriesCollection.NewSeries
                series_id = series_id + 1
                With .SeriesCollection(series_id)
                    .XValues = {0, 0}
                    .Values = {udAxisY.Min, udAxisY.Max}
                    .Name = "X Zero Line"
                    .MarkerStyle = -4142
                    .Border.Color = RGB(0, 0, 0)
                    With .Format.Line
                        .Visible = True
                        .Weight = 1
                    End With
                End With

                Try
                    .Legend.Delete()
                Catch
                End Try

                .Axes(XlAxisType.xlValue, XlAxisGroup.xlPrimary).HasTitle = False
                .Axes(XlAxisType.xlValue, XlAxisGroup.xlPrimary).HasTitle = True
                .Axes(XlAxisType.xlValue, XlAxisGroup.xlPrimary).AxisTitle.text = $"2nd Component Scores [{ Format$(PercentExpl(1), "#0.0#")}%]"
                .Axes(XlAxisType.xlValue, XlAxisGroup.xlPrimary).AxisTitle.Font.Size = 16
                .Axes(XlAxisType.xlValue, XlAxisGroup.xlPrimary).TickLabels.Font.Size = 14
                .Axes(XlAxisType.xlCategory, XlAxisGroup.xlPrimary).HasTitle = False
                .Axes(XlAxisType.xlCategory, XlAxisGroup.xlPrimary).HasTitle = True
                .Axes(XlAxisType.xlCategory, XlAxisGroup.xlPrimary).AxisTitle.text = $"1st Component Scores [{ Format$(PercentExpl(0), "#0.0#")}%]"
                .Axes(XlAxisType.xlCategory, XlAxisGroup.xlPrimary).AxisTitle.Font.Size = 16
                .Axes(XlAxisType.xlCategory, XlAxisGroup.xlPrimary).TickLabels.Font.Size = 14
                .HasTitle = False
                .HasTitle = True
                .ChartTitle.Text = "Biplot: " & titl
                .ChartTitle.Font.Size = 18
                .ChartTitle.Font.Bold = True
            End With

        End Sub

        '''''' <summary>
        '''''' Creates a scree plot of eigenvalues versus component index.
        '''''' </summary>
        '''''' <remarks>
        '''''' <para>
        '''''' These plotting methods use <c>Microsoft.Office.Interop.Excel</c> and assume an Excel <c>Application</c>
        '''''' instance named <c>app</c> is available in scope. They create a new chart in the active workbook.
        '''''' </para>
        '''''' <para>
        '''''' Call <see cref="Calculate"/> before plotting.
        '''''' </para>
        '''''' <para>Uses <see cref="Eigenvalues"/> and <see cref="XaxisComponents"/>.</para>
        '''''' </remarks>
        Public Sub screePlot()

            app.Charts.Add()
            With app.ActiveWorkbook.ActiveChart
                .Name = "Scree Plot"
                .ChartType = XlChartType.xlXYScatter

                'delete extra series
                Do Until .SeriesCollection.Count = 0
                    .SeriesCollection(1).Delete
                Loop

                .SeriesCollection.NewSeries
                With .SeriesCollection(1)
                    .XValues = Me.XaxisComponents
                    .Values = Me.PercentExpl
                    .Name = "Percent Explained"
                    .Format.Line.Weight = 1.5
                    .MarkerStyle = 8
                    .MarkerSize = 5
                    .Border.Color = RGB(100, 100, 100)
                    .MarkerForegroundColor = RGB(100, 100, 100)
                    .MarkerBackgroundColor = RGB(100, 100, 100)

                    'Attach a label to each data point in the chart.
                    For i = 0 To p - 1
                        .points(i + 1).HasDataLabel = True
                        .points(i + 1).DataLabel.text = Format$(Me.PercentExpl(i), "#0.0#")
                        .points(i + 1).DataLabel.Position = XlDataLabelPosition.xlLabelPositionAbove
                        .points(i + 1).DataLabel.Font.Size = 12
                    Next
                End With
                Try
                    .Legend.Delete()
                Catch
                End Try
                .Axes(XlAxisType.xlValue, XlAxisGroup.xlPrimary).HasTitle = False
                .Axes(XlAxisType.xlValue, XlAxisGroup.xlPrimary).HasTitle = True
                .Axes(XlAxisType.xlValue, XlAxisGroup.xlPrimary).AxisTitle.text = "Variance explained [%]"
                .Axes(XlAxisType.xlValue, XlAxisGroup.xlPrimary).AxisTitle.Font.Size = 16
                .Axes(XlAxisType.xlValue, XlAxisGroup.xlPrimary).TickLabels.Font.Size = 14
                .Axes(XlAxisType.xlCategory, XlAxisGroup.xlPrimary).HasTitle = False
                .Axes(XlAxisType.xlCategory, XlAxisGroup.xlPrimary).HasTitle = True
                .Axes(XlAxisType.xlCategory, XlAxisGroup.xlPrimary).AxisTitle.text = "Principal Component"
                .Axes(XlAxisType.xlCategory, XlAxisGroup.xlPrimary).AxisTitle.Font.Size = 16
                .Axes(XlAxisType.xlCategory, XlAxisGroup.xlPrimary).TickLabels.Font.Size = 14
                .HasTitle = False
                .HasTitle = True
                .ChartTitle.Text = "Scree Plot"
                .ChartTitle.Font.Size = 18
                .ChartTitle.Font.Bold = True
            End With

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
        Private Function standardize(vector As Double()) As Double()
            Dim k As Integer = UBound(vector)
            Dim m As Double = vector.Average()
            Dim out(k) As Double
            Dim sd As Double = stDev(vector)
            If sd = 0.0 OrElse Double.IsNaN(sd) OrElse Double.IsInfinity(sd) Then
                BSerr.LogAndThrow(New ArgumentException("Cannot standardize: SD is zero/invalid."))
            End If

            For i As Integer = 0 To k
                out(i) = (vector(i) - m) / sd
            Next
            Return out
        End Function

        '''''' <summary>
        '''''' Centers a vector by subtracting its mean.
        '''''' </summary>
        '''''' <param name="vector">Input data vector (length n).</param>
        '''''' <returns>Centered vector x_i − mean(x).</returns>
        Private Function center(vector() As Double) As Double()
            Dim k As Integer = UBound(vector)
            Dim m As Double = vector.Average()
            Dim out(k) As Double
            For i As Integer = 0 To k
                out(i) = (vector(i) - m)
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
        Private Function SortEigenpairsDescending(vals() As Double, vecs(,) As Double) As (Double(), Double(,))

            Dim pLast As Integer = vals.Length - 1
            Dim order = Enumerable.Range(0, vals.Length).OrderByDescending(Function(i) vals(i)).ToArray()
            Dim vals2(pLast) As Double
            Dim vecs2(pLast, pLast) As Double

            For newJ As Integer = 0 To pLast
                Dim oldJ As Integer = order(newJ)
                vals2(newJ) = vals(oldJ)
                For i As Integer = 0 To pLast
                    vecs2(i, newJ) = vecs(i, oldJ)
                Next
            Next

            Return (vals2, vecs2)
        End Function

    End Class
End Namespace